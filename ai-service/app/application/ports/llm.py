from typing import Protocol


class LlmPort(Protocol):
    async def complete(self, prompt: str) -> str:
        ...
