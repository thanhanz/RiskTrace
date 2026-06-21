# RiskTrace AI Service

Python service for legal document analysis workflows.

## RabbitMQ

Configuration is read from environment variables:

```env
RABBITMQ_URL=amqp://guest:guest@localhost:5672/
ANALYSIS_EXCHANGE=risktrace.analysis
ANALYSIS_REQUEST_QUEUE=risktrace.analysis.requested
ANALYSIS_RESULT_ROUTING_KEY=analysis.completed
```

Publish an event:

```python
from app.messaging.event_publisher import publish_event

await publish_event({"event_type": "analysis.completed", "document_id": "doc-1"})
```

Consume events:

```python
from app.messaging.analysis_consumer import consume_events

async def handle_event(event: dict) -> None:
    print(event)

await consume_events(handle_event)
```
