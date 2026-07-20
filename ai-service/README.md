# RiskTrace AI Service

Python service for legal document analysis workflows.

## Current Status

The knowledge-base preparation and vector-indexing foundation is implemented. The service currently supports:

- PDF legal-source ingestion with metadata sidecars.
- Selectable-text extraction with OCR fallback for scanned PDFs.
- Legal chunking and validation with article, clause, point, page, and source traceability.
- Contextual embedding-text generation so chunks retain their legal hierarchy during retrieval.
- Normalized 1024-dimensional embeddings generated with BGE-M3.
- Idempotent vector upserts to PostgreSQL using the pgVector extension.

The default vector table is `knowledge_base_vectors`. Each record stores the chunk text, embedding, source metadata, legal position, embedding model/version, and additional metadata. `chunk_id` is the primary key, so re-indexing updates an existing chunk instead of creating a duplicate.

The next RAG work is to define retrieval strategies for contract clauses and questions, assemble useful legal context, and construct the prompt/context package sent to the LLM. Final risk classification, recommendations, and complete grounded review output are not implemented yet.

## Knowledge-Base and RAG Flow

The implemented indexing flow is:

```text
PDF + metadata
  -> extracted text
  -> validated legal chunks
  -> contextual embedding text
  -> BGE-M3 embeddings
  -> PostgreSQL/pgVector
```

The planned review flow is:

```text
Contract clause or user question
  -> retrieval strategy
  -> pgVector similarity search
  -> legal context and citation selection
  -> prompt/context construction
  -> LLM review
```

The low-level pgVector adapter already supports cosine-similarity search and optional `source_id` filtering. Clause-level retrieval orchestration, retrieval-quality evaluation, and LLM prompt construction remain the next implementation steps.

To verify the number of successfully indexed vectors:

```sql
SELECT COUNT(*) FROM knowledge_base_vectors;
```

The result should equal the number of valid, uniquely identified chunks that completed indexing. A malformed JSONL record stops the indexer at that record, so check the AI-service logs when the count is lower than expected:

```powershell
docker logs risktrace.ai
```

## Source Structure

```text
app/
|-- main.py
|-- core/
|   |-- settings.py
|   |-- logger.py
|   |-- constants.py
|   |-- errors.py
|   `-- time.py
|-- domain/
|   |-- models/
|   |   |-- document.py
|   |   |-- analysis.py
|   |   |-- finding.py
|   |   `-- session.py
|   `-- events/
|       |-- document_uploaded.py
|       `-- analysis_completed.py
|-- application/
|   |-- use_cases/
|   |   |-- analyze_document.py
|   |   `-- ingest_knowledge_base.py
|   `-- ports/
|       |-- storage.py
|       |-- vector_store.py
|       |-- message_bus.py
|       |-- llm.py
|       `-- backend_client.py
|-- services/
|   |-- document/
|   |   |-- extractor.py
|   |   |-- ocr.py
|   |   `-- chunker.py
|   |-- retrieval/
|   |   |-- embedder.py
|   |   |-- retriever.py
|   |   `-- citation_mapper.py
|   |-- risk/
|   |   |-- scorer.py
|   |   |-- severity_classifier.py
|   |   `-- recommendation_engine.py
|   `-- llm/
|       |-- prompt_registry.py
|       `-- response_parser.py
|-- infrastructure/
|   |-- storage/
|   |   `-- r2_client.py
|   |-- vector_db/
|   |   |-- pgvector_store.py
|   |   `-- schema.sql
|   |-- messaging/
|   |   |-- rabbitmq_connection.py
|   |   |-- rabbitmq_consumer.py
|   |   `-- rabbitmq_publisher.py
|   |-- llm/
|   |   `-- openai_client.py
|   `-- backend/
|       `-- http_client.py
|-- interfaces/
|   |-- api/
|   |   `-- health.py
|   `-- consumers/
|       `-- document_uploaded_consumer.py
`-- schemas/
    |-- events.py
    |-- findings.py
    `-- reports.py
```

## Layer Responsibilities

- `core/` contains cross-cutting runtime utilities: settings, logging, constants, errors, and time helpers.
- `domain/` contains domain-facing models and event types.
- `application/` contains use case orchestration and port contracts for external dependencies.
- `services/` contains internal document, retrieval, risk, and LLM helper logic.
- `infrastructure/` contains concrete adapters for RabbitMQ, storage, vector DB, backend HTTP, and LLM providers.
- `interfaces/` contains inbound adapters, including FastAPI routes and message consumers.
- `schemas/` contains transport and report schemas.

Top-level support files:

- `prompts/` stores prompt templates used by the LLM layer.
- `requirements.txt` lists Python dependencies.
- `Dockerfile` defines the service container image.

## Runtime Notes

The service reads RabbitMQ configuration from environment variables:

```env
RABBITMQ_URL=amqp://guest:guest@localhost:5672/
ANALYSIS_EXCHANGE=risktrace.events
ANALYSIS_REQUEST_QUEUE=risktrace.documents.uploaded
ANALYSIS_RESULT_QUEUE=risktrace.ai.responses
ANALYSIS_REQUEST_ROUTING_KEY=document.uploaded_request
ANALYSIS_RESULT_ROUTING_KEY=ai.review_completed
```

These defaults are defined in `app/core/constants.py` and loaded by `app/core/settings.py`.

## Messaging Example

Publish an event:

```python
from app.infrastructure.messaging.rabbitmq_publisher import publish_event

await publish_event({"event_type": "analysis.completed", "document_id": "doc-1"})
```

Consume events:

```python
from app.infrastructure.messaging.rabbitmq_consumer import consume_events

async def handle_event(event: dict) -> None:
    print(event)

await consume_events(handle_event)
```
