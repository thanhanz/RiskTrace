"""Index already-processed knowledge-base chunks into pgVector."""

from __future__ import annotations

import json
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Iterator

from app.application.ports.vector_store import VectorStorePort
from app.core.logger import get_logger
from app.core.settings import settings
from app.services.retrieval.embedding_text import build_embedding_chunk


logger = get_logger(__name__)


@dataclass(frozen=True)
class KnowledgeBaseIndexSummary:
    file_count: int
    chunk_count: int


class IndexKnowledgeBaseUseCase:
    """Read existing chunk JSONL files and index them in batches."""

    def __init__(
        self,
        vector_store: VectorStorePort,
        batch_size: int | None = None,
    ) -> None:
        self.vector_store = vector_store
        self.batch_size = (
            batch_size if batch_size is not None else settings.embedding_batch_size
        )
        if self.batch_size <= 0:
            raise ValueError("Knowledge-base index batch size must be greater than zero.")

    async def execute(
        self,
        knowledge_base_dir: Path | str,
    ) -> KnowledgeBaseIndexSummary:
        processed_dir = Path(knowledge_base_dir) / "processed"
        if not processed_dir.exists():
            raise FileNotFoundError(
                f"Knowledge base processed directory does not exist: {processed_dir}"
            )

        chunk_files = sorted(processed_dir.glob("*.chunks.jsonl"))
        await self.vector_store.ensure_schema()

        total_chunks = 0
        for chunk_file in chunk_files:
            file_count = await self._index_file(chunk_file)
            total_chunks += file_count
            logger.info(
                "Indexed %s chunk(s) from '%s'.",
                file_count,
                chunk_file.name,
            )

        return KnowledgeBaseIndexSummary(
            file_count=len(chunk_files),
            chunk_count=total_chunks,
        )

    async def _index_file(self, chunk_file: Path) -> int:
        batch = []
        indexed_count = 0
        for line_number, record in enumerate(_read_jsonl(chunk_file), start=1):
            try:
                batch.append(build_embedding_chunk(record))
            except (TypeError, ValueError, KeyError) as exc:
                raise ValueError(
                    f"Invalid chunk in {chunk_file.name} on JSONL line {line_number}: {exc}"
                ) from exc

            if len(batch) >= self.batch_size:
                indexed_count += await self.vector_store.upsert(batch)
                batch = []

        if batch:
            indexed_count += await self.vector_store.upsert(batch)
        return indexed_count


def _read_jsonl(path: Path) -> Iterator[dict[str, Any]]:
    with path.open("r", encoding="utf-8") as chunk_file:
        for line in chunk_file:
            if not line.strip():
                continue
            record = json.loads(line)
            if not isinstance(record, dict):
                raise ValueError("Each JSONL record must be an object.")
            yield record
