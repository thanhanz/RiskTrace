"""Build contextual text for embedding legal knowledge-base chunks.

This module deliberately does not call an embedding model or a vector database.
It only enriches an existing chunk record with text that is ready to be sent to
the embedding step later in the pipeline.
"""

from __future__ import annotations

import json
import re
from pathlib import Path
from typing import Any, Iterator

from app.domain.models.knowledge_base import (
    EmbeddingChunkPosition,
    EmbeddingChunkSource,
    KnowledgeBaseEmbeddingChunk,
)


_WHITESPACE_RE = re.compile(r"\s+")


def clean_chunk_text(text: str) -> str:
    """Flatten extraction line breaks and normalize repeated whitespace."""

    return _WHITESPACE_RE.sub(" ", text).strip()


def build_embedding_text(chunk: dict[str, Any]) -> str:
    """Build contextual embedding text from one processed chunk record.

    The source title and structural position are included so a short clause can
    still be understood when it is retrieved without its neighboring chunks.
    Existing ``clean_text`` is preferred; otherwise it is derived from
    ``raw_text`` or the current chunker's ``text`` field.
    """

    source = _mapping(chunk.get("source"))
    position = _mapping(chunk.get("position"))

    source_title = _first_non_empty(source, "title")
    chapter = _first_non_empty(position, "chapter")
    section = _first_non_empty(position, "section")
    article_number = _first_non_empty(position, "article_number")
    article_title = _first_non_empty(position, "article_title")
    clause_number = _first_non_empty(position, "clause_number")
    point_label = _first_non_empty(position, "point_label")

    text = _first_non_empty(chunk, "clean_text", "raw_text", "text")
    if not text:
        raise ValueError("Chunk is missing text; expected clean_text, raw_text, or text.")

    context: list[str] = []
    if source_title:
        context.append(source_title)
    if chapter:
        context.append(chapter)
    if section:
        context.append(section)
    if article_number:
        article = f"Điều {article_number}"
        if article_title:
            article += f": {article_title}"
        context.append(article)
    if clause_number:
        context.append(f"Khoản {clause_number}")
    if point_label:
        context.append(f"Điểm {point_label}")

    clean_text = clean_chunk_text(str(text))
    return ". ".join([*context, clean_text]) if context else clean_text


def enrich_chunk_record(chunk: dict[str, Any]) -> dict[str, Any]:
    """Return a serialized domain model with embedding text fields.

    The existing ``text`` field is retained for backward compatibility with the
    current chunker. If a record already contains ``raw_text`` or ``clean_text``,
    those values are preserved as the source values.
    """

    enriched = dict(chunk)
    raw_text = _first_non_empty(enriched, "raw_text", "text")
    if not raw_text:
        raise ValueError("Chunk is missing text; expected raw_text or text.")

    clean_text = _first_non_empty(enriched, "clean_text")
    enriched["raw_text"] = str(raw_text)
    enriched["clean_text"] = clean_chunk_text(str(clean_text or raw_text))
    enriched["embedding_text"] = build_embedding_text(enriched)

    model = build_embedding_chunk(enriched)

    # Keep chunker metadata such as effect, trace, and parent until the future
    # vector-store mapping decides which fields belong in payload metadata.
    result = dict(enriched)
    result.update(model.to_dict())
    return result


def build_embedding_chunk(chunk: dict[str, Any]) -> KnowledgeBaseEmbeddingChunk:
    """Build the domain model used by the embedder and vector store."""

    enriched = dict(chunk)
    raw_text = _first_non_empty(enriched, "raw_text", "text")
    if not raw_text:
        raise ValueError("Chunk is missing text; expected raw_text or text.")

    clean_text = _first_non_empty(enriched, "clean_text")
    enriched["raw_text"] = str(raw_text)
    enriched["clean_text"] = clean_chunk_text(str(clean_text or raw_text))
    enriched["embedding_text"] = build_embedding_text(enriched)

    return KnowledgeBaseEmbeddingChunk(
        chunk_id=_required(enriched, "chunk_id"),
        chunk_type=_required(enriched, "chunk_type"),
        raw_text=enriched["raw_text"],
        clean_text=enriched["clean_text"],
        embedding_text=enriched["embedding_text"],
        position=_position_from_record(enriched),
        source=_source_from_record(enriched),
        metadata=_metadata_from_record(enriched),
        embedding_model=enriched.get("embedding_model"),
        embedding_version=enriched.get("embedding_version"),
        embedding=enriched.get("embedding"),
    )


def enrich_chunks_jsonl(
    input_path: Path | str,
    output_path: Path | str | None = None,
) -> Path:
    """Enrich a chunks JSONL file and return the path of the new JSONL file.

    When no output path is provided, ``.chunks.jsonl`` becomes
    ``.embedding.jsonl``. The input file is never overwritten implicitly.
    """

    source_path = Path(input_path)
    destination = Path(output_path) if output_path else _default_output_path(source_path)
    if source_path.resolve() == destination.resolve():
        raise ValueError("Output path must differ from the input chunks JSONL path.")

    destination.parent.mkdir(parents=True, exist_ok=True)
    with source_path.open("r", encoding="utf-8") as source_file, destination.open(
        "w", encoding="utf-8"
    ) as destination_file:
        for line_number, chunk in enumerate(_read_jsonl(source_file), start=1):
            try:
                enriched = enrich_chunk_record(chunk)
            except (TypeError, ValueError) as exc:
                raise ValueError(f"Invalid chunk on JSONL line {line_number}: {exc}") from exc
            destination_file.write(
                json.dumps(enriched, ensure_ascii=False, sort_keys=True) + "\n"
            )

    return destination


def _read_jsonl(lines: Iterator[str]) -> Iterator[dict[str, Any]]:
    for line in lines:
        if not line.strip():
            continue
        record = json.loads(line)
        if not isinstance(record, dict):
            raise ValueError("Each JSONL record must be an object.")
        yield record


def _default_output_path(input_path: Path) -> Path:
    if input_path.name.endswith(".chunks.jsonl"):
        return input_path.with_name(input_path.name.removesuffix(".chunks.jsonl") + ".embedding.jsonl")
    return input_path.with_name(input_path.stem + ".embedding.jsonl")


def _mapping(value: Any) -> dict[str, Any]:
    return value if isinstance(value, dict) else {}


def _required(values: dict[str, Any], key: str) -> str:
    value = _first_non_empty(values, key)
    if not value:
        raise ValueError(f"Chunk is missing required field: {key}.")
    return value


def _position_from_record(chunk: dict[str, Any]) -> EmbeddingChunkPosition:
    position = _mapping(chunk.get("position"))
    article_number = _first_non_empty(position, "article_number")
    if not article_number:
        raise ValueError("Chunk position is missing required field: article_number.")

    return EmbeddingChunkPosition(
        article_number=article_number,
        article_title=_first_non_empty(position, "article_title"),
        chapter=_first_non_empty(position, "chapter"),
        clause_number=_first_non_empty(position, "clause_number"),
        point_label=_first_non_empty(position, "point_label"),
        section=_first_non_empty(position, "section"),
        chapter_title=_first_non_empty(position, "chapter_title"),
        section_title=_first_non_empty(position, "section_title"),
    )


def _source_from_record(chunk: dict[str, Any]) -> EmbeddingChunkSource:
    source = _mapping(chunk.get("source"))
    return EmbeddingChunkSource(
        source_id=_required(source, "source_id"),
        title=_required(source, "title"),
        status=_required(source, "status"),
        version=_required(source, "version"),
        source_url=_first_non_empty(source, "source_url"),
        jurisdiction=_first_non_empty(source, "jurisdiction"),
        document_type=_first_non_empty(source, "document_type"),
    )


def _metadata_from_record(chunk: dict[str, Any]) -> dict[str, Any]:
    reserved = {
        "chunk_id",
        "chunk_type",
        "raw_text",
        "clean_text",
        "text",
        "embedding_text",
        "position",
        "source",
        "embedding_model",
        "embedding_version",
        "embedding",
    }
    return {key: value for key, value in chunk.items() if key not in reserved}


def _first_non_empty(values: dict[str, Any], *keys: str) -> str | None:
    for key in keys:
        value = values.get(key)
        if value is not None and str(value).strip():
            return str(value).strip()
    return None
