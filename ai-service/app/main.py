import asyncio
from contextlib import asynccontextmanager

from fastapi import FastAPI

from app.common.logger import get_logger
from app.common.messaging_constants import AnalysisMessagingConstants
from app.common.settings import settings
from app.common.time import utc_now
from app.messaging.analysis_consumer import consume_events
from app.messaging.event_publisher import publish_event

logger = get_logger(__name__)


async def handle_document_uploaded(event: dict) -> None:
    response_event = {
        "event_type": AnalysisMessagingConstants.TEMP_RESPONSE_EVENT_TYPE,
        "session_id": str(event.get("sessionId", "")),
        "document_id": str(event.get("documentId", "")),
        "occurred_at": utc_now().isoformat(),
        "payload": {
            "status": "received",
            "user_id": str(event.get("userId", "")),
            "file_name": str(event.get("fileName", "")),
            "storage_path": str(event.get("storagePath", "")),
        },
    }

    await publish_event(response_event)
    logger.info(
        "Processed document uploaded event for document '%s' and published temporary AI response.",
        response_event["document_id"],
    )


@asynccontextmanager
async def lifespan(_: FastAPI):
    consumer_task = asyncio.create_task(consume_events(handle_document_uploaded))
    try:
        yield
    finally:
        consumer_task.cancel()
        try:
            await consumer_task
        except asyncio.CancelledError:
            logger.info("RabbitMQ consumer task stopped.")


app = FastAPI(title=settings.service_name, lifespan=lifespan)


@app.get("/health")
async def health() -> dict[str, str]:
    return {"status": "ok"}
