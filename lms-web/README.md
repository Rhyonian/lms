# LMS Web

A React 18 + Vite single-page application scaffold for a learning management system. The project
uses Tailwind CSS with shadcn-inspired components and TanStack Table to render interactive admin
grids.

## Available routes

- `/catalog` – course catalog overview.
- `/course/:courseId` – course detail view with hero video and lesson playlist.
- `/learn/:courseId/:lessonId?` – learner experience with iframe player and navigation controls.
- `/admin/users`, `/admin/groups`, `/admin/courses` – admin dashboards with sortable data grids.
- `/oidc/sign-in`, `/oidc/callback` – placeholder screens for wiring up OIDC flows.

## Getting started

Install dependencies and launch the dev server:

```bash
npm install
npm run dev
```

> **Note:** The codebase ships with mock data only. Replace the sample data sources and OIDC
> placeholders with live integrations for production use.
