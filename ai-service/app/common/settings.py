from functools import lru_cache

from pydantic_settings import BaseSettings, SettingsConfigDict

from app.common.messaging_constants import AnalysisMessagingConstants


class Settings(BaseSettings):
    service_name: str = "RiskTrace AI Service"
    rabbitmq_url: str = "amqp://guest:guest@localhost:5672/"
    analysis_exchange: str = AnalysisMessagingConstants.EXCHANGE
    analysis_request_queue: str = AnalysisMessagingConstants.REQUEST_QUEUE
    analysis_result_routing_key: str = AnalysisMessagingConstants.RESULT_ROUTING_KEY

    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8")


@lru_cache
def get_settings() -> Settings:
    return Settings()


settings = get_settings()
