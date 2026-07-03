from dataclasses import asdict, dataclass

from app.services.document.chunker import LegalChunk


@dataclass(frozen=True)
class ChunkValidationIssue:
    code: str
    message: str
    value: str | None = None

    def to_dict(self) -> dict:
        return asdict(self)


@dataclass(frozen=True)
class ChunkValidationResult:
    chunk: LegalChunk
    issues: tuple[ChunkValidationIssue, ...]

    @property
    def is_valid(self) -> bool:
        return not self.issues

    def to_quarantine_record(self) -> dict:
        return {
            "chunk": self.chunk.to_dict(),
            "issues": [issue.to_dict() for issue in self.issues],
        }
