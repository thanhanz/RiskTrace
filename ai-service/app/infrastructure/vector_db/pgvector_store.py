"""PostgreSQL/pgVector adapter for the AI-owned knowledge base."""

from __future__ import annotations

import asyncio
import json
import re
from collections.abc import Sequence
from pathlib import Path
from typing import Any

from app.application.ports.embeddings import EmbeddingPort
from app.application.ports.vector_store import VectorStorePort
from app.core.settings import settings
from app.domain.models.knowledge_base import KnowledgeBaseEmbeddingChunk


_IDENTIFIER_RE = re.compile(r"^[a-z_][a-z0-9_]*$")
_VECTOR_DIMENSIONS = 1024
_SCHEMA_PATH = Path(__file__).with_name("schema.sql")


class PgVectorStore(VectorStorePort):
    """Store and search legal knowledge chunks using cosine similarity."""

    def __init__(
        self,
        embedder: EmbeddingPort,
        *,
        database_url: str | None = None,
        min_pool_size: int | None = None,
        max_pool_size: int | None = None,
        table_name: str | None = None,
    ) -> None:
        self.embedder = embedder
        self.database_url = database_url or settings.vector_db_url
        self.min_pool_size = (
            min_pool_size
            if min_pool_size is not None
            else settings.vector_db_min_pool_size
        )
        self.max_pool_size = (
            max_pool_size
            if max_pool_size is not None
            else settings.vector_db_max_pool_size
        )
        self.table_name = table_name or settings.vector_db_table
        self._pool: Any | None = None

        _validate_identifier(self.table_name)
        if self.min_pool_size < 1:
            raise ValueError("Vector DB minimum pool size must be greater than zero.")
        if self.max_pool_size < self.min_pool_size:
            raise ValueError(
                "Vector DB maximum pool size must be greater than or equal to minimum pool size."
            )

    async def connect(self) -> None:
        """Create the connection pool if it has not been created yet."""

        if self._pool is not None:
            return
        if not self.database_url:
            raise RuntimeError("VECTOR_DB_URL must be configured before connecting.")

        try:
            import asyncpg
            from pgvector.asyncpg import register_vector
        except ImportError as exc:
            raise RuntimeError(
                "asyncpg and pgvector are required for the PostgreSQL vector store."
            ) from exc

        # pgvector's ``vector`` type must exist before register_vector() can
        # inspect it during pool initialization. Bootstrap the extension with
        # a temporary connection before creating the application pool.
        bootstrap_connection = await asyncpg.connect(self.database_url)
        try:
            await bootstrap_connection.execute("CREATE EXTENSION IF NOT EXISTS vector")
        finally:
            await bootstrap_connection.close()

        async def initialize_connection(connection: Any) -> None:
            await register_vector(connection)
            await connection.set_type_codec(
                "jsonb",
                encoder=json.dumps,
                decoder=json.loads,
                schema="pg_catalog",
            )

        self._pool = await asyncpg.create_pool(
            dsn=self.database_url,
            min_size=self.min_pool_size,
            max_size=self.max_pool_size,
            init=initialize_connection,
        )

    async def close(self) -> None:
        """Close the connection pool."""

        if self._pool is not None:
            await self._pool.close()
            self._pool = None

    async def ensure_schema(self) -> None:
        """Create the pgVector extension, table, and index explicitly."""

        pool = await self._get_pool()
        schema = _SCHEMA_PATH.read_text(encoding="utf-8")
        schema = (
            schema.replace("{{TABLE_NAME}}", _quote_identifier(self.table_name))
            .replace(
                "{{HNSW_INDEX_NAME}}",
                _quote_identifier(f"{self.table_name}_embedding_hnsw_idx"),
            )
            .replace(
                "{{SOURCE_INDEX_NAME}}",
                _quote_identifier(f"{self.table_name}_source_id_idx"),
            )
        )
        async with pool.acquire() as connection:
            await connection.execute(schema)

    async def upsert(self, chunks: Sequence[KnowledgeBaseEmbeddingChunk]) -> int:
        """Embed and idempotently upsert knowledge-base chunks."""

        if not chunks:
            return 0

        vectors = await asyncio.to_thread(
            self.embedder.embed_documents,
            [chunk.embedding_text for chunk in chunks],
        )
        if len(vectors) != len(chunks):
            raise ValueError("Embedding count does not match chunk count.")
        if any(len(vector) != _VECTOR_DIMENSIONS for vector in vectors):
            raise ValueError(
                f"BGE-M3 embeddings must contain {_VECTOR_DIMENSIONS} dimensions."
            )

        rows = [
            _chunk_row(chunk, vector, self.embedder)
            for chunk, vector in zip(chunks, vectors, strict=True)
        ]
        pool = await self._get_pool()
        query = _upsert_query(self.table_name)
        async with pool.acquire() as connection:
            async with connection.transaction():
                await connection.executemany(query, rows)
        return len(rows)

    async def search(
        self,
        query: str,
        limit: int = 5,
        source_id: str | None = None,
    ) -> list[dict]:
        """Return chunks ranked by cosine similarity to a raw text query."""

        if not isinstance(query, str) or not query.strip():
            raise ValueError("Search query must not be empty.")
        if limit <= 0:
            raise ValueError("Search limit must be greater than zero.")

        vector = await asyncio.to_thread(self.embedder.embed_text, query)
        pool = await self._get_pool()
        sql = _search_query(self.table_name, source_id is not None)
        parameters: list[Any] = [vector]
        if source_id is not None:
            parameters.append(source_id)
        parameters.append(limit)

        async with pool.acquire() as connection:
            rows = await connection.fetch(sql, *parameters)
        return [dict(row) for row in rows]

    async def delete_source(self, source_id: str) -> int:
        """Delete all vectors belonging to one legal source."""

        if not source_id or not source_id.strip():
            raise ValueError("Source ID must not be empty.")

        pool = await self._get_pool()
        query = f"DELETE FROM {_quote_identifier(self.table_name)} WHERE source_id = $1"
        async with pool.acquire() as connection:
            status = await connection.execute(query, source_id)
        return int(status.rsplit(" ", 1)[-1])

    async def _get_pool(self) -> Any:
        if self._pool is None:
            await self.connect()
        return self._pool


def _chunk_row(
    chunk: KnowledgeBaseEmbeddingChunk,
    vector: list[float],
    embedder: EmbeddingPort,
) -> tuple[Any, ...]:
    record = chunk.to_dict()
    source = record["source"]
    position = record["position"]
    metadata = {
        key: value
        for key, value in record.items()
        if key
        not in {
            "chunk_id",
            "chunk_type",
            "raw_text",
            "clean_text",
            "embedding_text",
            "position",
            "source",
            "embedding_model",
            "embedding_version",
            "embedding",
        }
    }
    return (
        chunk.chunk_id,
        chunk.chunk_type,
        source["source_id"],
        source["title"],
        source["status"],
        source["version"],
        chunk.raw_text,
        chunk.clean_text,
        chunk.embedding_text,
        position,
        source,
        metadata,
        chunk.embedding_model or getattr(embedder, "model_name", settings.embedding_model_name),
        chunk.embedding_version or settings.embedding_version,
        vector,
    )


def _upsert_query(table_name: str) -> str:
    table = _quote_identifier(table_name)
    return f"""
        INSERT INTO {table} (
            chunk_id, chunk_type, source_id, source_title, source_status,
            source_version, raw_text, clean_text, embedding_text, position,
            source, metadata, embedding_model, embedding_version, embedding
        )
        VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15)
        ON CONFLICT (chunk_id) DO UPDATE SET
            chunk_type = EXCLUDED.chunk_type,
            source_id = EXCLUDED.source_id,
            source_title = EXCLUDED.source_title,
            source_status = EXCLUDED.source_status,
            source_version = EXCLUDED.source_version,
            raw_text = EXCLUDED.raw_text,
            clean_text = EXCLUDED.clean_text,
            embedding_text = EXCLUDED.embedding_text,
            position = EXCLUDED.position,
            source = EXCLUDED.source,
            metadata = EXCLUDED.metadata,
            embedding_model = EXCLUDED.embedding_model,
            embedding_version = EXCLUDED.embedding_version,
            embedding = EXCLUDED.embedding,
            updated_at = CURRENT_TIMESTAMP
    """


def _search_query(table_name: str, has_source_filter: bool) -> str:
    table = _quote_identifier(table_name)
    source_filter = "AND source_id = $2" if has_source_filter else ""
    limit_parameter = "$3" if has_source_filter else "$2"
    return f"""
        SELECT
            chunk_id, chunk_type, raw_text, clean_text, embedding_text,
            position, source, metadata, embedding_model, embedding_version,
            1 - (embedding <=> $1) AS similarity
        FROM {table}
        WHERE embedding IS NOT NULL
            {source_filter}
        ORDER BY embedding <=> $1
        LIMIT {limit_parameter}
    """


def _validate_identifier(identifier: str) -> None:
    if not _IDENTIFIER_RE.fullmatch(identifier):
        raise ValueError(
            "Vector DB table name must contain only lowercase letters, numbers, and underscores."
        )


def _quote_identifier(identifier: str) -> str:
    _validate_identifier(identifier)
    return f'"{identifier}"'
