from typing import Protocol


class BackendClientPort(Protocol):
    async def notify_analysis_completed(self, document_id: str) -> None:
        ...
