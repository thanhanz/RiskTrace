import asyncio
import json
from collections.abc import Awaitable, Callable
from typing import Any

from aio_pika import IncomingMessage

from app.common.logger import get_logger
from app.common.messaging_constants import AnalysisMessagingConstants
from app.common.settings import settings
from app.messaging.rabbitmq_connection import declare_analysis_exchange, rabbitmq_channel

logger = get_logger(__name__)
EventHandler = Callable[[dict[str, Any]], Awaitable[None]]


async def consume_events(
    handler: EventHandler,
    queue_name: str | None = None,
    routing_key: str = AnalysisMessagingConstants.REQUEST_ROUTING_KEY,
) -> None:
    async with rabbitmq_channel() as channel:
        exchange = await declare_analysis_exchange(channel)
        queue = await channel.declare_queue(
            queue_name or settings.analysis_request_queue,
            durable=True,
        )
        await queue.bind(exchange, routing_key=routing_key)

        async def on_message(message: IncomingMessage) -> None:
            async with message.process(requeue=True):
                payload = json.loads(message.body.decode("utf-8"))
                await handler(payload)

        await queue.consume(on_message)
        logger.info("Consuming RabbitMQ queue '%s'", queue.name)
        await asyncio.Future()
