from __future__ import annotations

from pathlib import Path
from fastapi import Depends, FastAPI, Request
from fastapi.responses import HTMLResponse
from fastapi.staticfiles import StaticFiles
from fastapi.templating import Jinja2Templates

from .database import initialize_database, seed_demo_data
from routers.analytics import router as analytics_router

BASE_DIR = Path(__file__).resolve().parent.parent
TEMPLATES_DIR = BASE_DIR / "templates"
STATIC_DIR = BASE_DIR / "static"

app = FastAPI(title="LMS Analytics API", version="1.0.0")
app.include_router(analytics_router)


@app.on_event("startup")
def startup() -> None:
    initialize_database()
    seed_demo_data()


def get_templates() -> Jinja2Templates:
    return Jinja2Templates(directory=str(TEMPLATES_DIR))


app.mount("/static", StaticFiles(directory=str(STATIC_DIR)), name="static")


@app.get("/admin/analytics", response_class=HTMLResponse)
async def admin_analytics(request: Request, templates: Jinja2Templates = Depends(get_templates)) -> HTMLResponse:
    return templates.TemplateResponse("admin_analytics.html", {"request": request})
"""FastAPI application exposing SCORM upload and launch endpoints."""
from __future__ import annotations

import mimetypes
import posixpath
import zipfile
from datetime import datetime, timedelta
from tempfile import NamedTemporaryFile
from typing import Annotated
from uuid import uuid4

from fastapi import Depends, FastAPI, File, HTTPException, Path, UploadFile
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles

from .config import Settings, get_settings
from .database import Database, get_database
from .schemas import LaunchResponse, UploadResponse
from .scorm import ManifestNotFoundError, ManifestParseError, read_manifest
from .storage import S3Storage, get_storage


def get_app() -> FastAPI:
    app = FastAPI(title="LMS Service", version="1.0.0")
    app.add_middleware(
        CORSMiddleware,
        allow_origins=["*"],
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )
    app.mount("/static", StaticFiles(directory="static"), name="static")

    @app.post("/courses/{course_id}/scorm/upload", response_model=UploadResponse)
    async def upload_scorm_package(
        course_id: Annotated[str, Path(description="Identifier of the course")],
        file: UploadFile = File(..., description="Zipped SCORM package"),
        db: Database = Depends(get_database),
        storage: S3Storage = Depends(get_storage),
    ) -> UploadResponse:
        if not file.filename or not file.filename.lower().endswith(".zip"):
            raise HTTPException(status_code=400, detail="Only .zip uploads are supported")

        with NamedTemporaryFile(suffix=".zip", delete=True) as tmp_file:
            contents = await file.read()
            tmp_file.write(contents)
            tmp_file.flush()

            with zipfile.ZipFile(tmp_file.name) as archive:
                try:
                    manifest = read_manifest(archive)
                except ManifestNotFoundError as exc:
                    raise HTTPException(status_code=400, detail=str(exc)) from exc
                except ManifestParseError as exc:
                    raise HTTPException(status_code=400, detail=str(exc)) from exc

                object_prefix = f"courses/{course_id}/scorm/{uuid4()}"

                for item in archive.infolist():
                    if item.is_dir():
                        continue
                    name = posixpath.normpath(item.filename)
                    if name.startswith("../") or "..\\" in name:
                        raise HTTPException(status_code=400, detail="Archive contains unsafe paths")
                    with archive.open(item) as data:
                        mime_type, _ = mimetypes.guess_type(name)
                        storage.upload(f"{object_prefix}/{name}", data, mime_type)

                db.insert_package(
                    course_id=course_id,
                    version=manifest.version,
                    entry_point=manifest.entry_point,
                    object_prefix=object_prefix,
                    manifest=manifest.manifest_xml,
                )

        return UploadResponse(
            course_id=course_id,
            version=manifest.version,
            entry_point=manifest.entry_point,
            object_prefix=object_prefix,
        )

    @app.get("/courses/{course_id}/scorm/launch", response_model=LaunchResponse)
    async def launch_scorm_package(
        course_id: Annotated[str, Path(description="Identifier of the course")],
        settings: Settings = Depends(get_settings),
        db: Database = Depends(get_database),
        storage: S3Storage = Depends(get_storage),
    ) -> LaunchResponse:
        package = db.find_latest_package(course_id)
        if package is None:
            raise HTTPException(status_code=404, detail="SCORM package not found for course")

        attempt_token = uuid4().hex
        expires_at = datetime.utcnow() + timedelta(seconds=settings.attempt_ttl_seconds)
        db.create_attempt(int(package["id"]), attempt_token, expires_at)

        key = f"{package['object_prefix']}/{package['entry_point']}"
        presigned_url = storage.presign(key, settings.presign_ttl_seconds)

        return LaunchResponse(
            launch_url=presigned_url,
            attempt_token=attempt_token,
            expires_at=expires_at,
            version=package["version"],
        )

    return app


app = get_app()


__all__ = ["app", "get_app"]
