import asyncio
from contextlib import asynccontextmanager

from fastapi import FastAPI

from app.core.logger import get_logger
from app.core.settings import settings
from app.application.use_cases.index_knowledge_base import IndexKnowledgeBaseUseCase
from app.application.use_cases.ingest_knowledge_base import (
    DEFAULT_KNOWLEDGE_BASE_DIR,
    ingest_knowledge_base,
)
from app.infrastructure.vector_db.pgvector_store import PgVectorStore
from app.infrastructure.messaging.rabbitmq_consumer import consume_events
from app.interfaces.api.health import router as health_router
from app.interfaces.consumers.document_uploaded_consumer import handle_document_uploaded
from app.services.retrieval.embedder import BgeM3Embedder

logger = get_logger(__name__)


async def _run_startup_knowledge_base_ingestion() -> None:
    knowledge_base_dir = settings.knowledge_base_dir or DEFAULT_KNOWLEDGE_BASE_DIR
    logger.info("Starting knowledge base ingestion from '%s'.", knowledge_base_dir)
    try:
        summary = await asyncio.to_thread(ingest_knowledge_base, knowledge_base_dir)
    except Exception:
        logger.exception("Knowledge base ingestion failed.")
        return

    logger.info(
        "Knowledge base ingestion finished: %s sources, %s chunks.",
        summary.source_count,
        summary.chunk_count,
    )


async def _run_startup_knowledge_base_indexing(
    ingestion_task: asyncio.Task | None = None,
) -> None:
    knowledge_base_dir = settings.knowledge_base_dir or DEFAULT_KNOWLEDGE_BASE_DIR
    vector_store = PgVectorStore(BgeM3Embedder())
    logger.info("Starting knowledge-base vector indexing from '%s'.", knowledge_base_dir)
    try:
        if ingestion_task:
            await ingestion_task
        summary = await IndexKnowledgeBaseUseCase(vector_store).execute(
            knowledge_base_dir
        )
    except Exception:
        logger.exception("Knowledge-base vector indexing failed.")
        return
    finally:
        await vector_store.close()

    logger.info(
        "Knowledge-base vector indexing finished: %s file(s), %s chunk(s).",
        summary.file_count,
        summary.chunk_count,
    )

#TODO: I dont know this, just PoC that the ingestion process can be triggered on application startup. 
#      Consider if this is the desired behavior or if it should be triggered by an admin action or a specific event.
@asynccontextmanager
async def lifespan(_: FastAPI):
    ingestion_task: asyncio.Task | None = None
    indexing_task: asyncio.Task | None = None
    if settings.ingest_knowledge_base_on_startup:
        ingestion_task = asyncio.create_task(_run_startup_knowledge_base_ingestion())
    if settings.index_knowledge_base_on_startup:
        indexing_task = asyncio.create_task(
            _run_startup_knowledge_base_indexing(ingestion_task)
        )

    consumer_task = asyncio.create_task(consume_events(handle_document_uploaded))
    try:
        yield
    finally:
        if ingestion_task and not ingestion_task.done():
            ingestion_task.cancel()
            try:
                await ingestion_task
            except asyncio.CancelledError:
                logger.info("Knowledge base ingestion task stopped.")

        if indexing_task and not indexing_task.done():
            indexing_task.cancel()
            try:
                await indexing_task
            except asyncio.CancelledError:
                logger.info("Knowledge base vector indexing task stopped.")

        consumer_task.cancel()
        try:
            await consumer_task
        except asyncio.CancelledError:
            logger.info("RabbitMQ consumer task stopped.")


app = FastAPI(title=settings.service_name, lifespan=lifespan)
app.include_router(health_router)
