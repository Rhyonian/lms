# Learning Management Service

This repository contains a minimal FastAPI-based service that supports SCORM package uploads and launches. It also exposes lightweight browser adapters for SCORM 1.2 and SCORM 2004 content.

## Features

- `POST /courses/{id}/scorm/upload` accepts a SCORM package zip, validates the manifest, extracts files, uploads them to MinIO/S3 storage, and persists metadata in SQLite.
- `GET /courses/{id}/scorm/launch` returns a pre-signed launch URL for the SCORM entry point and a fresh attempt token.
- Static adapters (`/static/scorm/scorm12-adapter.js` and `/static/scorm/scorm2004-adapter.js`) provide thin API bridges that forward CMI deltas to `/api/scorm/commit`.

## Running locally

1. Install dependencies:

   ```bash
   pip install -r requirements.txt
   ```

2. Export S3/MinIO credentials (adjust for your environment):

   ```bash
   export S3_ENDPOINT_URL="http://localhost:9000"
   export S3_ACCESS_KEY="minioadmin"
   export S3_SECRET_KEY="minioadmin"
   export S3_BUCKET="lms-scorm"
   export S3_USE_SSL="false"
   ```

3. Start the API:

   ```bash
   uvicorn app.main:app --reload
   ```

4. Upload a package:

   ```bash
   curl -X POST "http://localhost:8000/courses/course-123/scorm/upload" \
     -F "file=@path/to/package.zip"
   ```

5. Launch a package:

   ```bash
   curl "http://localhost:8000/courses/course-123/scorm/launch"
   ```

The returned JSON includes a signed URL and attempt token that can be consumed by the SCORM adapters.
