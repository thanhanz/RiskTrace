"""Domain models for knowledge-base records prepared for vector indexing."""

from __future__ import annotations

from dataclasses import asdict, dataclass, field
from typing import Any


@dataclass(frozen=True)
class EmbeddingChunkPosition:
    article_number: str
    article_title: str | None = None
    chapter: str | None = None
    clause_number: str | None = None
    point_label: str | None = None
    section: str | None = None
    chapter_title: str | None = None
    section_title: str | None = None


@dataclass(frozen=True)
class EmbeddingChunkSource:
    source_id: str
    title: str
    status: str
    version: str
    source_url: str | None = None
    jurisdiction: str | None = None
    document_type: str | None = None


@dataclass(frozen=True)
class KnowledgeBaseEmbeddingChunk:
    """A knowledge-base chunk ready for embedding and vector storage."""

    chunk_id: str
    chunk_type: str
    raw_text: str
    clean_text: str
    embedding_text: str
    position: EmbeddingChunkPosition
    source: EmbeddingChunkSource
    metadata: dict[str, Any] = field(default_factory=dict)
    embedding_model: str | None = None
    embedding_version: str | None = None
    embedding: list[float] | None = None

    def to_dict(self) -> dict[str, Any]:
        """Serialize the domain model using the vector-record field names."""

        record = asdict(self)
        record.update(record.pop("metadata"))
        if self.embedding_model is None:
            record.pop("embedding_model")
        if self.embedding_version is None:
            record.pop("embedding_version")
        if self.embedding is None:
            record.pop("embedding")
        return record
