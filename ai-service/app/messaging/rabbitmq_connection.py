from collections.abc import AsyncIterator
from contextlib import asynccontextmanager

from aio_pika import Channel, ExchangeType, Queue, RobustConnection, connect_robust

from app.common.settings import settings


@asynccontextmanager
async def rabbitmq_channel() -> AsyncIterator[Channel]:
    connection: RobustConnection = await connect_robust(settings.rabbitmq_url)
    async with connection:
        channel = await connection.channel()
        await channel.set_qos(prefetch_count=1)
        yield channel


async def declare_analysis_exchange(channel: Channel):
    return await channel.declare_exchange(
        settings.analysis_exchange,
        ExchangeType.TOPIC,
        durable=True,
    )


async def declare_bound_queue(
    channel: Channel,
    queue_name: str,
    routing_key: str,
) -> Queue:
    exchange = await declare_analysis_exchange(channel)
    queue = await channel.declare_queue(queue_name, durable=True)
    await queue.bind(exchange, routing_key=routing_key)
    return queue
