from typing import Any, Protocol


class MessageBusPort(Protocol):
    async def publish(self, event: dict[str, Any], routing_key: str | None = None) -> None:
        ...
