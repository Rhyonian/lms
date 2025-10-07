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
