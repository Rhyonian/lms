# Learning Management System API Specification

## Relational Model

```sql
CREATE TABLE users (
    id uuid PRIMARY KEY,
    email text NOT NULL UNIQUE,
    full_name text NOT NULL,
    role text NOT NULL CHECK (role IN ('Admin', 'Learner')),
    password_hash text NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE courses (
    id uuid PRIMARY KEY,
    title text NOT NULL,
    description text,
    created_at timestamptz NOT NULL,
    published_at timestamptz NULL
);

CREATE TABLE modules (
    id uuid PRIMARY KEY,
    course_id uuid NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
    title text NOT NULL,
    description text,
    display_order integer NOT NULL,
    UNIQUE(course_id, display_order)
);

CREATE TABLE lessons (
    id uuid PRIMARY KEY,
    module_id uuid NOT NULL REFERENCES modules(id) ON DELETE CASCADE,
    title text NOT NULL,
    content text,
    display_order integer NOT NULL,
    duration_minutes integer NOT NULL,
    UNIQUE(module_id, display_order)
);

CREATE TABLE enrollments (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    course_id uuid NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
    enrolled_at timestamptz NOT NULL,
    status text NOT NULL CHECK (status IN ('Active', 'Completed', 'Dropped')),
    UNIQUE(user_id, course_id)
);

CREATE TABLE lesson_progress (
    id uuid PRIMARY KEY,
    enrollment_id uuid NOT NULL REFERENCES enrollments(id) ON DELETE CASCADE,
    lesson_id uuid NOT NULL REFERENCES lessons(id) ON DELETE CASCADE,
    progress_percent numeric(5,2) NOT NULL,
    completed_at timestamptz NULL,
    UNIQUE(enrollment_id, lesson_id)
);
```

## API Surface

All endpoints require the `X-User-Role` header. Endpoints that allow learner self-service also expect `X-User-Id` to enforce ownership checks.

| Method | Route | Description | Authorization |
| ------ | ----- | ----------- | -------------- |
| GET | `/health` | Basic service heartbeat | Public |
| GET | `/users` | Paged list of users with optional role filter | Admin |
| GET | `/courses` | Paged list of courses with optional title search | Any authenticated role |
| GET | `/courses/{courseId}` | Course detail including modules and lessons | Any authenticated role |
| POST | `/courses` | Create a course | Admin |
| PUT | `/courses/{courseId}` | Update course metadata | Admin |
| DELETE | `/courses/{courseId}` | Delete a course | Admin |
| POST | `/courses/{courseId}/modules` | Add a module to a course | Admin |
| POST | `/modules/{moduleId}/lessons` | Add a lesson to a module | Admin |
| GET | `/courses/{courseId}/enrollments` | Paged enrollments for a course | Admin |
| POST | `/courses/{courseId}/enrollments` | Enroll a learner in a course | Admin |
| GET | `/learners/{learnerId}/enrollments` | Paged enrollments for a learner | Admin or matching learner |
| PUT | `/enrollments/{enrollmentId}/status` | Update enrollment status | Admin |
| PUT | `/enrollments/{enrollmentId}/progress` | Upsert progress for a lesson | Admin or matching learner |
| GET | `/learners/{learnerId}/progress` | Aggregate progress view for a learner | Admin or matching learner |

## Seed Data

The database seeds one administrator account (`admin@lms.test`) and five learner accounts (`learner1@lms.test` through `learner5@lms.test`). Password hashes are deterministic SHA-256 digests of predictable seed passwords to simplify local testing.
