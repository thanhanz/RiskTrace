from pydantic import BaseModel

from app.schemas.risk_finding import RiskFinding


class FinalReport(BaseModel):
    session_id: str
    document_id: str
    findings: list[RiskFinding]
    summary: str
