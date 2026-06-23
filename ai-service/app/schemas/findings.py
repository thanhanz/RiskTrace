from pydantic import BaseModel


class RiskFinding(BaseModel):
    clause_id: str
    severity: str
    confidence: float
    summary: str
    recommendation: str | None = None
