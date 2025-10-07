import sqlite3
from pathlib import Path
from typing import Iterable

DB_PATH = Path(__file__).resolve().parent.parent / "data" / "lms.db"


def get_connection() -> sqlite3.Connection:
    DB_PATH.parent.mkdir(parents=True, exist_ok=True)
    conn = sqlite3.connect(DB_PATH, detect_types=sqlite3.PARSE_DECLTYPES | sqlite3.PARSE_COLNAMES)
    conn.row_factory = sqlite3.Row
    return conn


def run_script(conn: sqlite3.Connection, statements: Iterable[str]) -> None:
    cursor = conn.cursor()
    try:
        for statement in statements:
            cursor.executescript(statement)
        conn.commit()
    finally:
        cursor.close()


def initialize_database() -> None:
    schema_statements = [
        """
        CREATE TABLE IF NOT EXISTS courses (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT NOT NULL,
            category TEXT,
            description TEXT,
            created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS groups (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            course_id INTEGER NOT NULL,
            name TEXT NOT NULL,
            facilitator TEXT,
            created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (course_id) REFERENCES courses(id)
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS learners (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            first_name TEXT NOT NULL,
            last_name TEXT NOT NULL,
            email TEXT NOT NULL UNIQUE,
            created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS group_memberships (
            group_id INTEGER NOT NULL,
            learner_id INTEGER NOT NULL,
            assigned_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
            PRIMARY KEY (group_id, learner_id),
            FOREIGN KEY (group_id) REFERENCES groups(id),
            FOREIGN KEY (learner_id) REFERENCES learners(id)
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS learner_course_progress (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            learner_id INTEGER NOT NULL,
            course_id INTEGER NOT NULL,
            status TEXT NOT NULL CHECK (status IN ('not_started', 'in_progress', 'completed', 'stalled')),
            progress_percent REAL NOT NULL DEFAULT 0,
            score REAL,
            last_activity_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
            completed_activities INTEGER DEFAULT 0,
            total_activities INTEGER DEFAULT 0,
            UNIQUE (learner_id, course_id),
            FOREIGN KEY (learner_id) REFERENCES learners(id),
            FOREIGN KEY (course_id) REFERENCES courses(id)
        );
        """,
    ]

    view_statements = [
        """
        DROP VIEW IF EXISTS course_progress_view;
        """,
        """
        CREATE VIEW course_progress_view AS
        SELECT
            c.id AS course_id,
            c.title AS course_title,
            c.category AS course_category,
            c.created_at AS course_created_at,
            COUNT(DISTINCT lcp.learner_id) AS total_learners,
            SUM(CASE WHEN lcp.status = 'completed' THEN 1 ELSE 0 END) AS completed_learners,
            SUM(CASE WHEN lcp.status = 'in_progress' THEN 1 ELSE 0 END) AS in_progress_learners,
            SUM(CASE WHEN lcp.status = 'not_started' THEN 1 ELSE 0 END) AS not_started_learners,
            SUM(CASE WHEN lcp.status = 'stalled' THEN 1 ELSE 0 END) AS stalled_learners,
            ROUND(AVG(lcp.progress_percent), 2) AS average_progress_percent,
            ROUND(AVG(lcp.score), 2) AS average_score,
            MAX(lcp.last_activity_at) AS last_activity_at
        FROM courses c
        LEFT JOIN learner_course_progress lcp ON lcp.course_id = c.id
        GROUP BY c.id;
        """,
        """
        DROP VIEW IF EXISTS group_progress_view;
        """,
        """
        CREATE VIEW group_progress_view AS
        SELECT
            g.id AS group_id,
            g.name AS group_name,
            g.course_id,
            c.title AS course_title,
            c.category AS course_category,
            COUNT(DISTINCT gm.learner_id) AS total_learners,
            SUM(CASE WHEN lcp.status = 'completed' THEN 1 ELSE 0 END) AS completed_learners,
            SUM(CASE WHEN lcp.status = 'in_progress' THEN 1 ELSE 0 END) AS in_progress_learners,
            SUM(CASE WHEN lcp.status = 'not_started' THEN 1 ELSE 0 END) AS not_started_learners,
            SUM(CASE WHEN lcp.status = 'stalled' THEN 1 ELSE 0 END) AS stalled_learners,
            ROUND(AVG(lcp.progress_percent), 2) AS average_progress_percent,
            ROUND(AVG(lcp.score), 2) AS average_score,
            MAX(lcp.last_activity_at) AS last_activity_at
        FROM groups g
        JOIN courses c ON c.id = g.course_id
        LEFT JOIN group_memberships gm ON gm.group_id = g.id
        LEFT JOIN learner_course_progress lcp
            ON lcp.learner_id = gm.learner_id AND lcp.course_id = g.course_id
        GROUP BY g.id;
        """,
        """
        DROP VIEW IF EXISTS learner_progress_view;
        """,
        """
        CREATE VIEW learner_progress_view AS
        SELECT
            l.id AS learner_id,
            l.first_name,
            l.last_name,
            l.email,
            c.id AS course_id,
            c.title AS course_title,
            c.category AS course_category,
            g.id AS group_id,
            g.name AS group_name,
            lcp.status,
            lcp.progress_percent,
            lcp.score,
            lcp.completed_activities,
            lcp.total_activities,
            lcp.last_activity_at
        FROM learners l
        LEFT JOIN learner_course_progress lcp ON lcp.learner_id = l.id
        LEFT JOIN courses c ON c.id = lcp.course_id
        LEFT JOIN group_memberships gm ON gm.learner_id = l.id
        LEFT JOIN groups g ON g.id = gm.group_id
        WHERE lcp.id IS NOT NULL;
        """,
    ]

    with get_connection() as conn:
        run_script(conn, schema_statements)
        run_script(conn, view_statements)


def seed_demo_data() -> None:
    sample_inserts = [
        """
        INSERT OR IGNORE INTO courses (id, title, category, description, created_at) VALUES
            (1, 'Python Fundamentals', 'Programming', 'Introductory Python curriculum', '2024-02-01T09:00:00Z'),
            (2, 'Data Analytics', 'Data Science', 'Statistics and BI foundations', '2024-03-15T09:00:00Z'),
            (3, 'Project Management', 'Leadership', 'Agile and PM essentials', '2024-04-10T09:00:00Z')
        """,
        """
        INSERT OR IGNORE INTO groups (id, course_id, name, facilitator, created_at) VALUES
            (1, 1, 'Cohort Alpha', 'Jordan Blake', '2024-02-02T10:00:00Z'),
            (2, 1, 'Cohort Beta', 'Jordan Blake', '2024-02-20T10:00:00Z'),
            (3, 2, 'Analysts West', 'Skyler Ray', '2024-03-18T10:00:00Z'),
            (4, 3, 'PM Europe', 'Zara Iqbal', '2024-04-12T10:00:00Z')
        """,
        """
        INSERT OR IGNORE INTO learners (id, first_name, last_name, email, created_at) VALUES
            (1, 'Alex', 'Lopez', 'alex.lopez@example.com', '2024-02-02T11:00:00Z'),
            (2, 'Bailey', 'Nguyen', 'bailey.nguyen@example.com', '2024-02-05T11:00:00Z'),
            (3, 'Casey', 'Patel', 'casey.patel@example.com', '2024-02-07T11:00:00Z'),
            (4, 'Devin', 'Kim', 'devin.kim@example.com', '2024-02-12T11:00:00Z'),
            (5, 'Emery', 'Singh', 'emery.singh@example.com', '2024-03-20T11:00:00Z'),
            (6, 'Francis', 'Osei', 'francis.osei@example.com', '2024-04-15T11:00:00Z')
        """,
        """
        INSERT OR IGNORE INTO group_memberships (group_id, learner_id, assigned_at) VALUES
            (1, 1, '2024-02-03T09:30:00Z'),
            (1, 2, '2024-02-04T09:30:00Z'),
            (2, 3, '2024-02-21T09:30:00Z'),
            (3, 4, '2024-03-19T09:30:00Z'),
            (3, 5, '2024-03-21T09:30:00Z'),
            (4, 6, '2024-04-16T09:30:00Z')
        """,
        """
        INSERT INTO learner_course_progress (learner_id, course_id, status, progress_percent, score, last_activity_at, completed_activities, total_activities)
        VALUES
            (1, 1, 'completed', 100, 94, '2024-03-01T17:00:00Z', 10, 10),
            (2, 1, 'in_progress', 65, 80, '2024-03-05T17:00:00Z', 6, 10),
            (3, 1, 'stalled', 40, 72, '2024-03-18T17:00:00Z', 4, 10),
            (4, 2, 'in_progress', 55, 70, '2024-04-10T17:00:00Z', 5, 12),
            (5, 2, 'not_started', 0, NULL, '2024-04-12T17:00:00Z', 0, 12),
            (6, 3, 'in_progress', 30, 65, '2024-05-01T17:00:00Z', 3, 8)
        ON CONFLICT(learner_id, course_id) DO UPDATE SET
            status=excluded.status,
            progress_percent=excluded.progress_percent,
            score=excluded.score,
            last_activity_at=excluded.last_activity_at,
            completed_activities=excluded.completed_activities,
            total_activities=excluded.total_activities;
        """,
    ]

    with get_connection() as conn:
        run_script(conn, sample_inserts)


__all__ = ["get_connection", "initialize_database", "seed_demo_data", "DB_PATH"]
"""Lightweight SQLite helper used by the LMS service."""
from __future__ import annotations

import sqlite3
from contextlib import contextmanager
from datetime import datetime
from pathlib import Path
from typing import Any, Dict, Iterator
from urllib.parse import urlparse

from .config import get_settings


class Database:
    """Provide convenience helpers for working with SQLite."""

    def __init__(self, url: str) -> None:
        parsed = urlparse(url)
        if parsed.scheme != "sqlite":
            raise ValueError("Only sqlite URLs are supported in this demo implementation")

        if parsed.netloc not in ("", "/"):
            raise ValueError("SQLite URLs must not define a hostname")

        path = parsed.path.lstrip("/") or "app.db"
        self._path = Path(path)
        self._path.parent.mkdir(parents=True, exist_ok=True)
        self._connection_kwargs: Dict[str, Any] = {"check_same_thread": False}

        self._initialise()

    @property
    def path(self) -> Path:
        return self._path

    def _initialise(self) -> None:
        with self.get_connection() as conn:
            conn.execute("PRAGMA foreign_keys = ON;")
            conn.execute(
                """
                CREATE TABLE IF NOT EXISTS scorm_packages (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    course_id TEXT NOT NULL,
                    version TEXT,
                    entry_point TEXT NOT NULL,
                    object_prefix TEXT NOT NULL,
                    manifest TEXT,
                    created_at TEXT NOT NULL,
                    updated_at TEXT NOT NULL
                );
                """
            )
            conn.execute(
                """
                CREATE TABLE IF NOT EXISTS scorm_attempts (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    package_id INTEGER NOT NULL REFERENCES scorm_packages(id) ON DELETE CASCADE,
                    attempt_token TEXT NOT NULL UNIQUE,
                    expires_at TEXT NOT NULL,
                    created_at TEXT NOT NULL
                );
                """
            )
            conn.commit()

    @contextmanager
    def get_connection(self) -> Iterator[sqlite3.Connection]:
        conn = sqlite3.connect(self._path, **self._connection_kwargs)
        conn.row_factory = sqlite3.Row
        try:
            yield conn
        finally:
            conn.close()

    def insert_package(
        self,
        course_id: str,
        version: str,
        entry_point: str,
        object_prefix: str,
        manifest: str,
    ) -> int:
        now = datetime.utcnow().isoformat()
        with self.get_connection() as conn:
            cursor = conn.execute(
                """
                INSERT INTO scorm_packages (course_id, version, entry_point, object_prefix, manifest, created_at, updated_at)
                VALUES (?, ?, ?, ?, ?, ?, ?);
                """,
                (course_id, version, entry_point, object_prefix, manifest, now, now),
            )
            conn.commit()
            return int(cursor.lastrowid)

    def find_latest_package(self, course_id: str) -> sqlite3.Row | None:
        with self.get_connection() as conn:
            cursor = conn.execute(
                """
                SELECT * FROM scorm_packages
                WHERE course_id = ?
                ORDER BY created_at DESC, id DESC
                LIMIT 1;
                """,
                (course_id,),
            )
            return cursor.fetchone()

    def create_attempt(self, package_id: int, token: str, expires_at: datetime) -> None:
        now = datetime.utcnow().isoformat()
        with self.get_connection() as conn:
            conn.execute(
                """
                INSERT INTO scorm_attempts (package_id, attempt_token, expires_at, created_at)
                VALUES (?, ?, ?, ?);
                """,
                (package_id, token, expires_at.isoformat(), now),
            )
            conn.commit()


_database_instance: Database | None = None


def get_database() -> Database:
    global _database_instance
    if _database_instance is None:
        settings = get_settings()
        _database_instance = Database(settings.database_url)
    return _database_instance


__all__ = ["Database", "get_database"]
