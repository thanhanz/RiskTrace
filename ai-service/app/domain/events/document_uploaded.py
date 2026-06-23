from pydantic import BaseModel


class DocumentUploaded(BaseModel):
    session_id: str
    document_id: str
    user_id: str
    file_name: str
    storage_path: str
