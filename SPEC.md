# LMS MVP Specification and Scaffold

## Scope

Roles: Admin, Learner.
Core features:

* User and Group management
* Course management
* SCORM upload and playback via embedded player
* Enrollments: direct and via course catalog
* Analytics: progress for learners, groups, courses
* AuthN/Z, audit logging, file storage, job queue

Target stack: ASP.NET Core 9 API, PostgreSQL 16, React 18 + Vite, Azure AD or Auth0 OIDC, Redis, S3 compatible storage (MinIO locally).

---

## Monorepo layout

```
lms/
  apps/
    api/
    web/
    worker/
  packages/
    ui/
    tsconfig/
    eslint-config/
  infra/
    docker/
    terraform/
  scripts/
  .github/workflows/
```

---

## Data model (DDL)

PostgreSQL. Snake_case. Soft delete via deleted_at. Timestamps with time zone.

```sql
create table users (
  id uuid primary key default gen_random_uuid(),
  email text not null unique,
  given_name text not null,
  family_name text not null,
  role text not null check (role in ('admin','learner')),
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  deleted_at timestamptz
);

create table groups (
  id uuid primary key default gen_random_uuid(),
  name text not null unique,
  description text,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  deleted_at timestamptz
);

create table group_members (
  group_id uuid references groups(id) on delete cascade,
  user_id uuid references users(id) on delete cascade,
  primary key (group_id, user_id),
  added_at timestamptz not null default now()
);

create table courses (
  id uuid primary key default gen_random_uuid(),
  slug text not null unique,
  title text not null,
  summary text,
  status text not null default 'draft' check (status in ('draft','published','archived')),
  has_scorm boolean not null default false,
  cover_image_url text,
  created_by uuid references users(id),
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  deleted_at timestamptz
);

create table course_scorm_packages (
  id uuid primary key default gen_random_uuid(),
  course_id uuid not null references courses(id) on delete cascade,
  version int not null default 1,
  storage_key text not null, -- path in S3/MinIO
  manifest_xml text not null,
  entry_launch_url text not null,
  scorm_version text not null, -- '1.2' or '2004'
  uploaded_by uuid references users(id),
  created_at timestamptz not null default now()
);

create table enrollments (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references users(id) on delete cascade,
  course_id uuid not null references courses(id) on delete cascade,
  source text not null check (source in ('admin','catalog','group-sync')),
  status text not null default 'active' check (status in ('active','completed','expired','withdrawn')),
  enrolled_at timestamptz not null default now(),
  unique (user_id, course_id)
);

create table progress (
  id uuid primary key default gen_random_uuid(),
  enrollment_id uuid not null references enrollments(id) on delete cascade,
  percent_complete numeric(5,2) not null default 0,
  last_launched_at timestamptz,
  completed_at timestamptz
);

create table scorm_attempts (
  id uuid primary key default gen_random_uuid(),
  enrollment_id uuid not null references enrollments(id) on delete cascade,
  attempt_no int not null,
  status text not null check (status in ('not-started','in-progress','suspended','completed','passed','failed')),
  total_time interval default '0',
  score_raw numeric(5,2),
  started_at timestamptz not null default now(),
  finished_at timestamptz,
  unique (enrollment_id, attempt_no)
);

create table scorm_cmi (
  id uuid primary key default gen_random_uuid(),
  attempt_id uuid not null references scorm_attempts(id) on delete cascade,
  element text not null, -- cmi.core.lesson_status etc
  value text,
  updated_at timestamptz not null default now(),
  unique (attempt_id, element)
);

create table catalog (
  course_id uuid primary key references courses(id) on delete cascade,
  visible boolean not null default false,
  enrollment_policy text not null default 'self-enroll', -- or 'request-approval'
  tags text[] default '{}'
);

create table audit_logs (
  id bigserial primary key,
  actor_id uuid references users(id),
  action text not null,
  entity text not null,
  entity_id uuid,
  metadata jsonb,
  created_at timestamptz not null default now()
);

create index idx_progress_enrollment on progress(enrollment_id);
create index idx_enroll_user on enrollments(user_id);
create index idx_enroll_course on enrollments(course_id);
```

---

## API surface (OpenAPI sketch)

Base URL: /api

```
GET    /health

POST   /auth/callback
GET    /me

# Users
GET    /users
POST   /users
GET    /users/{id}
PATCH  /users/{id}
DELETE /users/{id}

# Groups
GET    /groups
POST   /groups
GET    /groups/{id}
PATCH  /groups/{id}
DELETE /groups/{id}
POST   /groups/{id}/members   # {userId}
DELETE /groups/{id}/members/{userId}

# Courses
GET    /courses                # admin list with filters
POST   /courses
GET    /courses/{id}
PATCH  /courses/{id}
DELETE /courses/{id}

# SCORM
POST   /courses/{id}/scorm/upload  multipart zip
GET    /courses/{id}/scorm/launch  # returns launch URL and token
POST   /scorm/commit               # SCORM API adapter posts CMI deltas

# Enrollments
GET    /enrollments?userId=&courseId=
POST   /enrollments                # {userId, courseId}
PATCH  /enrollments/{id}           # status

# Catalog
GET    /catalog                    # learner view
PATCH  /catalog/{courseId}         # admin publish, policy, tags
POST   /catalog/{courseId}/enroll  # self enroll

# Analytics
GET    /analytics/overview         # totals
GET    /analytics/course/{courseId}
GET    /analytics/group/{groupId}
GET    /analytics/learner/{userId}
```

AuthZ policy:

* Admin: full on Users, Groups, Courses, Catalog, Analytics.
* Learner: read self, read Catalog, self enroll, launch own enrollments.

---

## SCORM player strategy

* Upload zip. Validate imsmanifest.xml. Extract to S3 prefix: scorm/{courseId}/{packageId}/
* Store entry launch path and SCORM version.
* Serve a thin HTML player that injects a SCORM 1.2 or 2004 API adapter which bridges to REST endpoints.
* Maintain attempt lifecycle: create on first launch if none in-progress, set suspended on unload, completed on cmi.core.lesson_status or completion_status.
* Persist deltas in scorm_cmi. Derive progress percent from lesson_status and cmi.progress_measure when present.

---

## Analytics queries

Examples:

```sql
-- Course completion rate
select c.title,
       count(e.*) as enrolled,
       count(*) filter (where p.completed_at is not null) as completed,
       round(100.0 * count(*) filter (where p.completed_at is not null) / nullif(count(e.*),0), 1) as completion_rate
from courses c
join enrollments e on e.course_id = c.id
left join progress p on p.enrollment_id = e.id
where c.deleted_at is null
group by c.id;

-- Group progress snapshot
select g.name,
       round(avg(p.percent_complete),1) as avg_progress
from groups g
join group_members gm on gm.group_id = g.id
join enrollments e on e.user_id = gm.user_id
join progress p on p.enrollment_id = e.id
where e.course_id = $1
group by g.id;

-- Learner transcript
select c.title, e.status, p.percent_complete, p.completed_at, sa.score_raw
from enrollments e
join courses c on c.id = e.course_id
left join progress p on p.enrollment_id = e.id
left join scorm_attempts sa on sa.enrollment_id = e.id and sa.attempt_no = (
  select max(attempt_no) from scorm_attempts where enrollment_id = e.id
)
where e.user_id = $1
order by c.title;
```

---

## Frontend routes

```
/
/login
/catalog                 # browse, search, tags
/course/:slug            # details, enroll
/learn/:enrollmentId     # launch player

/admin
  /admin/users
  /admin/users/:id
  /admin/groups
  /admin/courses
  /admin/courses/new
  /admin/courses/:id
  /admin/courses/:id/scorm  # upload
  /admin/analytics
  /admin/analytics/course/:id
  /admin/analytics/group/:id
  /admin/analytics/learner/:id
```

UI notes:

* Use shadcn/ui and TanStack Table for grids.
* File uploader with drag and drop. Show manifest parse results.
* Charts: Recharts for completion over time and distribution.

---

## Worker jobs

* scorm_extract_and_index(packageId)
* recompute_progress(enrollmentId)
* nightly_rollups() writes materialized views for analytics

---

## API implementation notes

* ASP.NET Core minimal APIs.
* EF Core for models and migrations.
* FluentValidation for payloads.
* Tuples of DTOs for list endpoints with pagination.
* Signed S3 URLs for launch assets.

Example endpoint snippets:

```csharp
app.MapPost("/courses/{id:guid}/scorm/upload", async (Guid id, IFormFile file, IScormService svc) => {
  var pkg = await svc.ProcessUploadAsync(id, file);
  return Results.Ok(new { packageId = pkg.Id, version = pkg.Version });
});

app.MapGet("/catalog", async (ICatalogService svc, [AsParameters] CatalogQuery q) =>
  Results.Ok(await svc.ListAsync(q)));
```

---

## SCORM API adapter bridge

Client JS injected into player window.

```js
// window.API for 1.2, window.API_1484_11 for 2004
function postDelta(payload){
  return fetch('/api/scorm/commit', {method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify(payload)});
}
```

---

## Course catalog flow

* Admin toggles course to published. Sets policy and tags.
* Learner views catalog, filters by tags, opens course page, clicks Enroll.
* API creates enrollment if allowed. Redirect to /learn/{enrollmentId}.

---

## RBAC and audit

* Policy per route using role claims.
* Write audit_logs for create, update, delete, publish, enroll, launch.

---

## Local dev via Docker

Compose services: api, web, worker, postgres, redis, minio, nginx.

`infra/docker/docker-compose.yml` sketch:

```yaml
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_PASSWORD: postgres
    ports: ["5432:5432"]
  redis:
    image: redis:7
    ports: ["6379:6379"]
  minio:
    image: minio/minio
    command: server /data
    environment:
      MINIO_ROOT_USER: admin
      MINIO_ROOT_PASSWORD: password
    ports: ["9000:9000", "9001:9001"]
  api:
    build: ../../apps/api
    env_file: ../../apps/api/.env
    depends_on: [postgres, redis, minio]
    ports: ["8080:8080"]
  worker:
    build: ../../apps/worker
    env_file: ../../apps/worker/.env
    depends_on: [postgres, redis, minio]
  web:
    build: ../../apps/web
    env_file: ../../apps/web/.env
    ports: ["5173:5173"]
```

---

## Seed data and fixtures

* Seed one admin and five learners.
* Seed two groups and assign learners.
* Seed two courses. One with sample SCORM 1.2 package.

---

## Initial backlog

1. Auth and user model, Admin UI for users and groups.
2. Courses CRUD and catalog publish.
3. Enrollment endpoints and learner flow.
4. SCORM upload, extract, launch with adapter writeback.
5. Progress rollup and analytics pages.
6. Audit logs and CSV export.

---

## Stretch goals

* xAPI statements alongside SCORM to future proof.
* Webhooks for completion events.
* ILT sessions and calendar.
* Multitenancy by org_id in all tables.

---

## Codex prompts to generate code

Use these task prompts verbatim as commands.

**API scaffold**

```
Create an ASP.NET Core 9 minimal API named Lms.Api. Add EF Core with PostgreSQL. Add entities and DbContext for tables defined in the DDL. Generate migrations. Implement endpoints as listed in API surface with basic validation and pagination. Add JWT auth middleware and role based authorization for Admin and Learner.
```

**Worker**

```
Create a .NET worker service named Lms.Worker. Connect to Redis and PostgreSQL. Implement jobs: scorm_extract_and_index, recompute_progress, nightly_rollups. Use a background queue library like Hangfire or Quartz.
```

**Web app**

```
Create a React 18 + Vite app named lms-web. Add routes as listed. Implement Admin grids for Users, Groups, Courses using TanStack Table and shadcn/ui. Implement Catalog pages and Learner launch page. Add OIDC login and protected routes. Add SCORM iframe player page that requests /api/courses/{id}/scorm/launch and mounts the SCORM API adapter.
```

**SCORM adapter**

```
Generate a lightweight SCORM 1.2 and 2004 adapter that captures LMSSetValue and Commit events and posts JSON deltas to /api/scorm/commit with attemptId and enrollmentId. Handle suspend and resume with window beforeunload.
```

**Analytics**

```
Create SQL views and API endpoints for the analytics queries shown. Build Admin charts and tables for overview, per course, per group, per learner.
```

---

## Test plan

* API integration tests for Users, Groups, Courses, Enrollments.
* SCORM adapter e2e using Playwright to simulate commit calls.
* Role checks per route.
* Analytics views snapshot tests.

---

## Security

* Use presigned URLs for SCORM assets.
* Validate manifest and restrict file types.
* Size limits and antivirus scan hook on upload.
* PII minimization and audit on access.

---
