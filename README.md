# LMS Analytics Demo

This project provides a lightweight FastAPI service exposing analytics queries for courses, groups, and learner progress backed by an SQLite database. It also includes an Admin Analytics dashboard with interactive charts and CSV exports powered by Chart.js.

## Getting started

```bash
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
uvicorn app.main:app --reload
```

The application seeds demo data on startup. Visit [http://localhost:8000/admin/analytics](http://localhost:8000/admin/analytics) to explore the dashboard.

## API endpoints

| Endpoint | Description |
| --- | --- |
| `GET /api/analytics/course-progress` | Aggregated metrics by course. Optional filters: `course_id`, `start_date`, `end_date`. |
| `GET /api/analytics/group-progress` | Aggregated metrics by group with filters for `course_id`, `group_id`, `start_date`, `end_date`. |
| `GET /api/analytics/learner-progress` | Learner-level metrics filtered by `course_id`, `group_id`, `learner_id`, `status`, and date range. |
| `GET /api/analytics/*.csv` | CSV exports for each dataset using the same filter parameters. |

All endpoints return JSON payloads sourced from SQL views that compute progress, completion, and scoring statistics across the seeded LMS tables.
