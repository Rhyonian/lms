"""Configuration helpers for the LMS service."""
from __future__ import annotations

from functools import lru_cache
from pydantic import BaseSettings, Field


class Settings(BaseSettings):
    """Application settings loaded from environment variables."""

    database_url: str = Field("sqlite:///./app.db", env="DATABASE_URL")
    s3_endpoint_url: str | None = Field(default=None, env="S3_ENDPOINT_URL")
    s3_region: str | None = Field(default=None, env="S3_REGION")
    s3_access_key: str | None = Field(default=None, env="S3_ACCESS_KEY")
    s3_secret_key: str | None = Field(default=None, env="S3_SECRET_KEY")
    s3_bucket: str = Field("lms-scorm", env="S3_BUCKET")
    s3_use_ssl: bool = Field(True, env="S3_USE_SSL")
    presign_ttl_seconds: int = Field(900, env="SCORM_PRESIGN_TTL")
    attempt_ttl_seconds: int = Field(3600, env="SCORM_ATTEMPT_TTL")

    class Config:
        env_file = ".env"
        case_sensitive = False


@lru_cache()
def get_settings() -> Settings:
    """Return cached settings instance."""

    return Settings()


__all__ = ["Settings", "get_settings"]
