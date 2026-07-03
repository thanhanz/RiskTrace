from app.core.validators.documents.metadata_validator import ChunkMetadataValidator
from app.core.validators.documents.models import (
    ChunkValidationIssue,
    ChunkValidationResult,
)
from app.core.validators.documents.ocr_validator import OcrSuspiciousValidator
from app.core.validators.documents.structure_validator import ChunkStructureValidator
from app.services.document.chunker import LegalChunk


class LegalChunkValidator:
    """Validate OCR-derived legal chunks before they become retrieval data."""

    def __init__(
        self,
        metadata_validator: ChunkMetadataValidator | None = None,
        ocr_validator: OcrSuspiciousValidator | None = None,
        structure_validator: ChunkStructureValidator | None = None,
    ) -> None:
        self.metadata_validator = metadata_validator or ChunkMetadataValidator()
        self.ocr_validator = ocr_validator or OcrSuspiciousValidator()
        self.structure_validator = structure_validator or ChunkStructureValidator()

    def validate_many(self, chunks: list[LegalChunk]) -> list[ChunkValidationResult]:
        issue_lists = [
            list(self._validate_single_chunk(chunk))
            for chunk in chunks
        ]

        structure_issues = self.structure_validator.validate_many(chunks)
        for index, issues in structure_issues.items():
            issue_lists[index].extend(issues)

        return [
            ChunkValidationResult(chunk=chunk, issues=tuple(issue_lists[index]))
            for index, chunk in enumerate(chunks)
        ]

    def validate(self, chunk: LegalChunk) -> ChunkValidationResult:
        return ChunkValidationResult(
            chunk=chunk,
            issues=self._validate_single_chunk(chunk),
        )

    def _validate_single_chunk(self, chunk: LegalChunk) -> tuple[ChunkValidationIssue, ...]:
        issues: list[ChunkValidationIssue] = []
        issues.extend(self.metadata_validator.validate(chunk))
        issues.extend(self.ocr_validator.validate(chunk))
        return tuple(issues)
