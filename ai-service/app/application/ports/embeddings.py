from typing import Protocol


class EmbeddingPort(Protocol):
    """Application-facing contract for converting text into vectors."""

    def embed_text(self, text: str) -> list[float]:
        ...

    def embed_documents(self, texts: list[str]) -> list[list[float]]:
        ...
