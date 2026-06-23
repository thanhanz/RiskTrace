# RiskTrace AI Service

Python service for legal document analysis workflows.

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
|   |   `-- qdrant_client.py
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
