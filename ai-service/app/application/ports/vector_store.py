from typing import Protocol


class VectorStorePort(Protocol):
    async def search(self, query: str, limit: int = 5) -> list[dict]:
        ...
