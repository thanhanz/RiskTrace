from datetime import datetime
from typing import Any

from pydantic import BaseModel, Field

from app.core.time import utc_now


class AnalysisEvent(BaseModel):
    event_type: str
    session_id: str
    document_id: str
    occurred_at: datetime = Field(default_factory=utc_now)
    payload: dict[str, Any] = Field(default_factory=dict)
