from typing import Protocol


class StoragePort(Protocol):
    async def read(self, path: str) -> bytes:
        ...
