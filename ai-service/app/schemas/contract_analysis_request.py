from pydantic import BaseModel


class ContractAnalysisRequest(BaseModel):
    session_id: str
    document_id: str
    file_url: str | None = None
