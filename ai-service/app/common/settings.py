from functools import lru_cache

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    service_name: str = "RiskTrace AI Service"
    rabbitmq_url: str = "amqp://guest:guest@localhost:5672/"
    analysis_exchange: str = "risktrace.analysis"
    analysis_request_queue: str = "risktrace.analysis.requested"
    analysis_result_routing_key: str = "analysis.completed"

    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8")


@lru_cache
def get_settings() -> Settings:
    return Settings()


settings = get_settings()
