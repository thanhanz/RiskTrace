from pydantic import BaseModel


class ReviewSession(BaseModel):
    id: str
    user_id: str
    title: str | None = None
