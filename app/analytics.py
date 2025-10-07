from __future__ import annotations

from datetime import datetime
from typing import Any, Dict, Iterable, List, Optional, Tuple

from .database import get_connection

DATE_FORMATS: Tuple[str, ...] = (
    "%Y-%m-%d",
    "%Y-%m-%dT%H:%M:%S",
    "%Y-%m-%dT%H:%M:%SZ",
    "%Y-%m-%d %H:%M:%S",
)


def _parse_date(value: Optional[str]) -> Optional[str]:
    if not value:
        return None
    for fmt in DATE_FORMATS:
        try:
            parsed = datetime.strptime(value, fmt)
        except ValueError:
            continue
        return parsed.isoformat() + "Z"
    raise ValueError(f"Unsupported date format: {value}")


def _apply_date_filters(base_query: str, start: Optional[str], end: Optional[str]) -> Tuple[str, List[Any]]:
    conditions: List[str] = []
    params: List[Any] = []
    if start:
        conditions.append("last_activity_at >= ?")
        params.append(_parse_date(start))
    if end:
        conditions.append("last_activity_at <= ?")
        params.append(_parse_date(end))
    if conditions:
        connector = " AND "
        clause = " WHERE " + connector.join(conditions)
        if " WHERE " in base_query.upper():
            base_query += " AND " + connector.join(conditions)
        else:
            base_query += clause
    return base_query, params


def _fetch_all(query: str, params: Iterable[Any]) -> List[Dict[str, Any]]:
    with get_connection() as conn:
        cursor = conn.execute(query, tuple(params))
        columns = [col[0] for col in cursor.description]
        rows = cursor.fetchall()
    return [dict(zip(columns, row)) for row in rows]


def fetch_course_progress(course_id: Optional[int] = None, start_date: Optional[str] = None, end_date: Optional[str] = None) -> List[Dict[str, Any]]:
    query = "SELECT * FROM course_progress_view"
    params: List[Any] = []
    clauses: List[str] = []

    if course_id is not None:
        clauses.append("course_id = ?")
        params.append(course_id)

    if clauses:
        query += " WHERE " + " AND ".join(clauses)

    query, date_params = _apply_date_filters(query, start_date, end_date)
    params.extend(date_params)

    query += " ORDER BY course_title"
    return _fetch_all(query, params)


def fetch_group_progress(
    course_id: Optional[int] = None,
    group_id: Optional[int] = None,
    start_date: Optional[str] = None,
    end_date: Optional[str] = None,
) -> List[Dict[str, Any]]:
    query = "SELECT * FROM group_progress_view"
    params: List[Any] = []
    clauses: List[str] = []

    if course_id is not None:
        clauses.append("course_id = ?")
        params.append(course_id)
    if group_id is not None:
        clauses.append("group_id = ?")
        params.append(group_id)

    if clauses:
        query += " WHERE " + " AND ".join(clauses)

    query, date_params = _apply_date_filters(query, start_date, end_date)
    params.extend(date_params)

    query += " ORDER BY group_name"
    return _fetch_all(query, params)


def fetch_learner_progress(
    course_id: Optional[int] = None,
    group_id: Optional[int] = None,
    learner_id: Optional[int] = None,
    status: Optional[str] = None,
    start_date: Optional[str] = None,
    end_date: Optional[str] = None,
) -> List[Dict[str, Any]]:
    query = "SELECT * FROM learner_progress_view"
    params: List[Any] = []
    clauses: List[str] = []

    if course_id is not None:
        clauses.append("course_id = ?")
        params.append(course_id)
    if group_id is not None:
        clauses.append("group_id = ?")
        params.append(group_id)
    if learner_id is not None:
        clauses.append("learner_id = ?")
        params.append(learner_id)
    if status is not None:
        clauses.append("status = ?")
        params.append(status)

    if clauses:
        query += " WHERE " + " AND ".join(clauses)

    query, date_params = _apply_date_filters(query, start_date, end_date)
    params.extend(date_params)

    query += " ORDER BY last_activity_at DESC"
    return _fetch_all(query, params)


__all__ = [
    "fetch_course_progress",
    "fetch_group_progress",
    "fetch_learner_progress",
]
