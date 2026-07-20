from functools import lru_cache

from pydantic_settings import BaseSettings, SettingsConfigDict

from app.core.constants import AnalysisMessagingConstants


class Settings(BaseSettings):
    service_name: str = "RiskTrace AI Service"
    ingest_knowledge_base_on_startup: bool = False
    index_knowledge_base_on_startup: bool = True
    knowledge_base_dir: str | None = None
    
    # OCR settings
    ocr_enabled: bool = True
    ocr_language: str = "vie"
    ocr_dpi: int = 300
    ocr_min_text_chars_per_page: int = 20
    
    # Embedding settings
    embedding_model_name: str = "BAAI/bge-m3"
    embedding_version: str = "v1"
    embedding_device: str | None = None
    embedding_batch_size: int = 32 #Maximum batch size for embedding requests
    embedding_normalize: bool = True
    
    # Vector database settings
    vector_db_url: str | None = None
    vector_db_min_pool_size: int = 1
    vector_db_max_pool_size: int = 10
    vector_db_table: str = "knowledge_base_vectors"
    
    # Messaging settings
    rabbitmq_url: str = "amqp://guest:guest@localhost:5672/"
    analysis_exchange: str = AnalysisMessagingConstants.EXCHANGE
    analysis_request_queue: str = AnalysisMessagingConstants.DOCUMENT_UPLOADED_QUEUE
    analysis_result_queue: str = AnalysisMessagingConstants.AI_RESPONSES_QUEUE
    analysis_request_routing_key: str = AnalysisMessagingConstants.DOCUMENT_UPLOADED_ROUTING_KEY
    analysis_result_routing_key: str = AnalysisMessagingConstants.AI_REVIEW_COMPLETED_ROUTING_KEY

    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8")


@lru_cache
def get_settings() -> Settings:
    return Settings()


settings = get_settings()
