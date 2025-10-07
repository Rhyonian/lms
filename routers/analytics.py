from __future__ import annotations

from fastapi import APIRouter, HTTPException, Query
from fastapi.responses import StreamingResponse

from app.analytics import (
    fetch_course_progress,
    fetch_group_progress,
    fetch_learner_progress,
)

router = APIRouter(prefix="/api/analytics", tags=["analytics"])


@router.get("/course-progress")
def course_progress(
    course_id: int | None = Query(default=None, description="Optional course identifier"),
    start_date: str | None = Query(default=None, description="ISO-8601 start date filter"),
    end_date: str | None = Query(default=None, description="ISO-8601 end date filter"),
):
    try:
        return fetch_course_progress(course_id=course_id, start_date=start_date, end_date=end_date)
    except ValueError as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc


@router.get("/group-progress")
def group_progress(
    course_id: int | None = Query(default=None),
    group_id: int | None = Query(default=None),
    start_date: str | None = Query(default=None),
    end_date: str | None = Query(default=None),
):
    try:
        return fetch_group_progress(
            course_id=course_id,
            group_id=group_id,
            start_date=start_date,
            end_date=end_date,
        )
    except ValueError as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc


@router.get("/learner-progress")
def learner_progress(
    course_id: int | None = Query(default=None),
    group_id: int | None = Query(default=None),
    learner_id: int | None = Query(default=None),
    status: str | None = Query(default=None),
    start_date: str | None = Query(default=None),
    end_date: str | None = Query(default=None),
):
    try:
        return fetch_learner_progress(
            course_id=course_id,
            group_id=group_id,
            learner_id=learner_id,
            status=status,
            start_date=start_date,
            end_date=end_date,
        )
    except ValueError as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc


def _as_csv(rows: list[dict[str, object]], filename: str) -> StreamingResponse:
    import csv
    from io import StringIO

    buffer = StringIO()
    if rows:
        writer = csv.DictWriter(buffer, fieldnames=list(rows[0].keys()))
        writer.writeheader()
        writer.writerows(rows)
    else:
        buffer.write("No data available\n")

    buffer.seek(0)
    headers = {
        "Content-Disposition": f"attachment; filename={filename}",
    }
    return StreamingResponse(buffer, media_type="text/csv", headers=headers)


@router.get("/course-progress.csv")
def course_progress_csv(
    course_id: int | None = Query(default=None),
    start_date: str | None = Query(default=None),
    end_date: str | None = Query(default=None),
):
    rows = fetch_course_progress(course_id=course_id, start_date=start_date, end_date=end_date)
    return _as_csv(rows, "course-progress.csv")


@router.get("/group-progress.csv")
def group_progress_csv(
    course_id: int | None = Query(default=None),
    group_id: int | None = Query(default=None),
    start_date: str | None = Query(default=None),
    end_date: str | None = Query(default=None),
):
    rows = fetch_group_progress(course_id=course_id, group_id=group_id, start_date=start_date, end_date=end_date)
    return _as_csv(rows, "group-progress.csv")


@router.get("/learner-progress.csv")
def learner_progress_csv(
    course_id: int | None = Query(default=None),
    group_id: int | None = Query(default=None),
    learner_id: int | None = Query(default=None),
    status: str | None = Query(default=None),
    start_date: str | None = Query(default=None),
    end_date: str | None = Query(default=None),
):
    rows = fetch_learner_progress(
        course_id=course_id,
        group_id=group_id,
        learner_id=learner_id,
        status=status,
        start_date=start_date,
        end_date=end_date,
    )
    return _as_csv(rows, "learner-progress.csv")
