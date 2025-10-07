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
