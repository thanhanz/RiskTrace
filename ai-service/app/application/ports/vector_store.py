from collections.abc import Sequence
from typing import Protocol

from app.domain.models.knowledge_base import KnowledgeBaseEmbeddingChunk


class VectorStorePort(Protocol):
    async def ensure_schema(self) -> None:
        ...

    async def upsert(self, chunks: Sequence[KnowledgeBaseEmbeddingChunk]) -> int:
        ...

    async def search(
        self,
        query: str,
        limit: int = 5,
        source_id: str | None = None,
    ) -> list[dict]:
        ...

    async def delete_source(self, source_id: str) -> int:
        ...
