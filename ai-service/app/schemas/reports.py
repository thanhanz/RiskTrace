from pydantic import BaseModel

from app.schemas.findings import RiskFinding


class FinalReport(BaseModel):
    session_id: str
    document_id: str
    findings: list[RiskFinding]
    summary: str
