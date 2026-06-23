import asyncio
from contextlib import asynccontextmanager

from fastapi import FastAPI

from app.core.logger import get_logger
from app.core.settings import settings
from app.infrastructure.messaging.rabbitmq_consumer import consume_events
from app.interfaces.api.health import router as health_router
from app.interfaces.consumers.document_uploaded_consumer import handle_document_uploaded

logger = get_logger(__name__)


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
app.include_router(health_router)
