from collections.abc import AsyncIterator
from contextlib import asynccontextmanager

from aio_pika import Channel, ExchangeType, RobustConnection, connect_robust

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
