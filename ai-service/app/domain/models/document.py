from pydantic import BaseModel


class Document(BaseModel):
    id: str
    session_id: str
    file_name: str
    storage_path: str | None = None
