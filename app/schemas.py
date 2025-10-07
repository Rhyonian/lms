"""Pydantic schemas used by the LMS service."""
from __future__ import annotations

from datetime import datetime
from pydantic import BaseModel, Field


class UploadResponse(BaseModel):
    course_id: str = Field(..., description="Course identifier")
    version: str = Field(..., description="Detected SCORM version")
    entry_point: str = Field(..., description="Relative path of the SCORM launch file")
    object_prefix: str = Field(..., description="Object storage prefix containing the SCORM assets")


class LaunchResponse(BaseModel):
    launch_url: str = Field(..., description="Pre-signed URL for the SCORM entry point")
    attempt_token: str = Field(..., description="Token used to authenticate CMI commits")
    expires_at: datetime = Field(..., description="Attempt expiration timestamp")
    version: str = Field(..., description="SCORM package version")


__all__ = ["UploadResponse", "LaunchResponse"]
