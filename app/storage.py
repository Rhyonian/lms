"""S3/MinIO storage helper for SCORM packages."""
from __future__ import annotations

import logging
from io import BufferedReader, BytesIO
from pathlib import Path
from typing import BinaryIO

import boto3
from botocore.client import Config
from botocore.exceptions import ClientError

from .config import get_settings

logger = logging.getLogger(__name__)


class S3Storage:
    """Encapsulates S3 interactions used by the service."""

    def __init__(self) -> None:
        settings = get_settings()
        session = boto3.session.Session(
            aws_access_key_id=settings.s3_access_key,
            aws_secret_access_key=settings.s3_secret_key,
            region_name=settings.s3_region,
        )
        config = Config(signature_version="s3v4")
        self._client = session.client(
            "s3",
            endpoint_url=settings.s3_endpoint_url,
            config=config,
            use_ssl=settings.s3_use_ssl,
        )
        self._bucket = settings.s3_bucket
        self._ensure_bucket()

    def _ensure_bucket(self) -> None:
        try:
            self._client.head_bucket(Bucket=self._bucket)
        except ClientError:
            logger.info("Bucket %s missing, attempting to create", self._bucket)
            self._client.create_bucket(Bucket=self._bucket)

    def upload(self, key: str, data: BinaryIO, content_type: str | None = None) -> None:
        extra_args = {"ACL": "private"}
        if content_type:
            extra_args["ContentType"] = content_type
        self._client.upload_fileobj(data, self._bucket, key, ExtraArgs=extra_args)

    def upload_bytes(self, key: str, payload: bytes, content_type: str | None = None) -> None:
        self.upload(key, BytesIO(payload), content_type)

    def upload_file(self, source_path: Path, destination_key: str) -> None:
        with source_path.open("rb") as handle:
            self.upload(destination_key, handle)

    def presign(self, key: str, expires_in: int) -> str:
        return self._client.generate_presigned_url(
            "get_object", Params={"Bucket": self._bucket, "Key": key}, ExpiresIn=expires_in
        )


_storage_instance: S3Storage | None = None


def get_storage() -> S3Storage:
    global _storage_instance
    if _storage_instance is None:
        _storage_instance = S3Storage()
    return _storage_instance


__all__ = ["S3Storage", "get_storage"]
