import asyncio
import json
from collections.abc import Awaitable, Callable
from typing import Any

from aio_pika import IncomingMessage

from app.core.logger import get_logger
from app.core.settings import settings
from app.infrastructure.messaging.rabbitmq_connection import declare_bound_queue, rabbitmq_channel

logger = get_logger(__name__)
EventHandler = Callable[[dict[str, Any]], Awaitable[None]]


async def consume_events(
    handler: EventHandler,
    queue_name: str | None = None,
    routing_key: str | None = None,
) -> None:
    async with rabbitmq_channel() as channel:
        queue = await declare_bound_queue(
            channel,
            queue_name or settings.analysis_request_queue,
            routing_key or settings.analysis_request_routing_key,
        )

        async def on_message(message: IncomingMessage) -> None:
            logger.info("Raw message received, body length: %d", len(message.body))

            async with message.process(requeue=True):
                try:
                    payload = json.loads(message.body.decode("utf-8"))
                except json.JSONDecodeError as e:
                    logger.error("Failed to decode message body: %s", e)
                    return

                logger.info(
                    "Received document uploaded event for document '%s' in session '%s'.",
                    payload.get("documentId", ""),
                    payload.get("sessionId", ""),
                )
                await handler(payload)

        await queue.consume(on_message)
        logger.info("Consuming RabbitMQ queue '%s'", queue.name)
        await asyncio.Future()
