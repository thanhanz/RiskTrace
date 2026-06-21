import json
from typing import Any

from aio_pika import DeliveryMode, Message

from app.common.settings import settings
from app.messaging.rabbitmq_connection import declare_analysis_exchange, rabbitmq_channel


async def publish_event(
    event: dict[str, Any],
    routing_key: str | None = None,
) -> None:
    async with rabbitmq_channel() as channel:
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
