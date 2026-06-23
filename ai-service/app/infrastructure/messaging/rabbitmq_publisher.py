import json
from typing import Any

from aio_pika import DeliveryMode, Message

from app.core.logger import get_logger
from app.core.settings import settings
from app.infrastructure.messaging.rabbitmq_connection import (
    declare_analysis_exchange,
    declare_bound_queue,
    rabbitmq_channel,
)

logger = get_logger(__name__)


async def publish_event(
    event: dict[str, Any],
    routing_key: str | None = None,
) -> None:
    async with rabbitmq_channel() as channel:
        await declare_bound_queue(
            channel,
            settings.analysis_result_queue,
            settings.analysis_result_routing_key,
        )
        exchange = await declare_analysis_exchange(channel)

        message = Message(
            body=json.dumps(event, default=str).encode("utf-8"),
            content_type="application/json",
            delivery_mode=DeliveryMode.PERSISTENT,
        )
        await exchange.publish(
            message,
            routing_key=routing_key or settings.analysis_result_routing_key,
        )
        logger.info(
            "Published AI response event with routing key '%s' for document '%s'.",
            routing_key or settings.analysis_result_routing_key,
            event.get("document_id", ""),
        )
