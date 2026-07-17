CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE IF NOT EXISTS {{TABLE_NAME}} (
    chunk_id TEXT PRIMARY KEY,
    chunk_type TEXT NOT NULL,
    source_id TEXT NOT NULL,
    source_title TEXT NOT NULL,
    source_status TEXT NOT NULL,
    source_version TEXT NOT NULL,
    raw_text TEXT NOT NULL,
    clean_text TEXT NOT NULL,
    embedding_text TEXT NOT NULL,
    position JSONB NOT NULL,
    source JSONB NOT NULL,
    metadata JSONB NOT NULL DEFAULT '{}'::jsonb,
    embedding_model TEXT NOT NULL,
    embedding_version TEXT,
    embedding VECTOR(1024) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS {{HNSW_INDEX_NAME}}
    ON {{TABLE_NAME}}
    USING hnsw (embedding vector_cosine_ops);

CREATE INDEX IF NOT EXISTS {{SOURCE_INDEX_NAME}}
    ON {{TABLE_NAME}} (source_id);
