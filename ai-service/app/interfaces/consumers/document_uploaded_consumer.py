from typing import Any

from app.core.constants import AnalysisMessagingConstants
from app.core.logger import get_logger
from app.core.time import utc_now
from app.infrastructure.messaging.rabbitmq_publisher import publish_event

logger = get_logger(__name__)


async def handle_document_uploaded(event: dict[str, Any]) -> None:
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
