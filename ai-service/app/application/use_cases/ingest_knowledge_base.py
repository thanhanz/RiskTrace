from dataclasses import asdict, dataclass
import json
import os
from pathlib import Path
from typing import Any

from app.core.logger import get_logger
from app.services.document.chunker import (
    LegalEffect,
    SourceMetadata,
    LegalKnowledgeBaseChunker,
)
from app.services.document.extractor import DocumentExtractor
from app.services.document.ocr import OcrPdfExtractor

#TODO: This usecase only called after ADMIN uploads the knowledge base PDFs and metadata to the sources/ folder. 
#      Maybe RabbitMQ consumer can be used to trigger this ingestion when a new PDF is uploaded  (via endpoint admin/upload-knowledge-base in .NET) to the sources/ folder.

logger = get_logger(__name__)
DEFAULT_KNOWLEDGE_BASE_DIR = Path(
    os.getenv(
        "KNOWLEDGE_BASE_DIR",
        Path(__file__).resolve().parents[3] / "knowledge_base",
    )
)


@dataclass(frozen=True)
class IngestedSourceSummary:
    source_id: str
    pdf_path: str
    chunks_path: str
    chunk_count: int
    warnings: list[dict[str, Any]]


@dataclass(frozen=True)
class KnowledgeBaseIngestionSummary:
    source_count: int
    chunk_count: int
    sources: list[IngestedSourceSummary]

    def to_dict(self) -> dict[str, Any]:
        return {
            "source_count": self.source_count,
            "chunk_count": self.chunk_count,
            "sources": [asdict(source) for source in self.sources],
        }


class IngestKnowledgeBaseUseCase:
    def __init__(
        self,
        extractor: DocumentExtractor | None = None,
        chunker: LegalKnowledgeBaseChunker | None = None,
    ) -> None:
        self.extractor = extractor or OcrPdfExtractor()
        self.chunker = chunker or LegalKnowledgeBaseChunker()

    def execute(
        self,
        knowledge_base_dir: Path | str = DEFAULT_KNOWLEDGE_BASE_DIR,
    ) -> KnowledgeBaseIngestionSummary:
        root = Path(knowledge_base_dir)
        sources_dir = root / "sources"
        processed_dir = root / "processed"
        processed_dir.mkdir(parents=True, exist_ok=True)

        if not sources_dir.exists():
            raise FileNotFoundError(
                f"Knowledge base sources directory does not exist: {sources_dir}"
            )

        summaries: list[IngestedSourceSummary] = []
        total_chunks = 0
        pdf_paths = sorted(sources_dir.glob("*.pdf"))
        logger.info("Found %s knowledge base PDF source(s) in '%s'.", len(pdf_paths), sources_dir)

        for pdf_path in pdf_paths:
            logger.info("Ingesting knowledge base PDF '%s'.", pdf_path)
            source_summary = self._ingest_pdf(pdf_path, processed_dir)
            summaries.append(source_summary)
            total_chunks += source_summary.chunk_count
            logger.info(
                "Finished PDF '%s': %s chunk(s), %s warning(s).",
                pdf_path.name,
                source_summary.chunk_count,
                len(source_summary.warnings),
            )

        summary = KnowledgeBaseIngestionSummary(
            source_count=len(summaries),
            chunk_count=total_chunks,
            sources=summaries,
        )

        #Logging the summary of the ingestion process to a JSON file in the processed directory.
        summary_path = processed_dir / "ingestion-summary.json" 
        summary_path.write_text(
            json.dumps(summary.to_dict(), ensure_ascii=False, indent=2),
            encoding="utf-8",
        )

        logger.info(
            "Ingested %s knowledge sources into %s chunks.",
            summary.source_count,
            summary.chunk_count,
        )
        return summary

    def _ingest_pdf(
        self,
        pdf_path: Path,
        processed_dir: Path,
    ) -> IngestedSourceSummary:
        metadata = self._load_metadata(pdf_path)
        source = self._build_source_metadata(metadata)
        effect = self._build_legal_effect(metadata)

        logger.info("Extracting text for source '%s' from '%s'.", source.source_id, pdf_path.name)
        
        extracted_document = self.extractor.extract(pdf_path)
        
        logger.info(
            "Extracted %s page(s) for source '%s'.",
            len(extracted_document.pages),
            source.source_id,
        )
        chunks = self.chunker.chunk(extracted_document, source, effect)

        chunks_path = processed_dir / f"{source.source_id}.chunks.jsonl"
        with chunks_path.open("w", encoding="utf-8") as chunks_file:
            for chunk in chunks:
                chunks_file.write(
                    json.dumps(chunk.to_dict(), ensure_ascii=False, sort_keys=True)
                )
                chunks_file.write("\n")

        warnings = [asdict(warning) for warning in extracted_document.warnings]
        return IngestedSourceSummary(
            source_id=source.source_id,
            pdf_path=str(pdf_path),
            chunks_path=str(chunks_path),
            chunk_count=len(chunks),
            warnings=warnings,
        )

    @staticmethod
    def _load_metadata(pdf_path: Path) -> dict[str, Any]:
        metadata_path = _find_metadata_path(pdf_path)
        if not metadata_path.exists():
            raise FileNotFoundError(
                f"Missing metadata sidecar for {pdf_path.name}."
            )

        with metadata_path.open("r", encoding="utf-8") as metadata_file:
            metadata = json.load(metadata_file)

        required_fields = ["source_id", "title", "effective_date"]
        missing_fields = [field for field in required_fields if not metadata.get(field)]
        if missing_fields:
            raise ValueError(
                f"Metadata file {metadata_path.name} is missing required fields: "
                f"{', '.join(missing_fields)}"
            )

        return metadata

    @staticmethod
    def _build_source_metadata(metadata: dict[str, Any]) -> SourceMetadata:
        return SourceMetadata(
            source_id=str(metadata["source_id"]),
            title=str(metadata["title"]),
            source_url=_optional_str(metadata.get("source_url")),
            jurisdiction=str(metadata.get("jurisdiction", "VN")),
            document_type=str(metadata.get("document_type", "law")),
            status=str(metadata.get("status", "active")),
            version=str(metadata.get("version", "v1")),
        )

    @staticmethod
    def _build_legal_effect(metadata: dict[str, Any]) -> LegalEffect:
        return LegalEffect(
            issued_date=_optional_str(metadata.get("issued_date")),
            effective_date=str(metadata["effective_date"]),
        )


def _optional_str(value: Any) -> str | None:
    if value is None:
        return None
    text = str(value).strip()
    return text or None


def _find_metadata_path(pdf_path: Path) -> Path:
    exact_candidates = [
        pdf_path.with_suffix(".json"),
        pdf_path.with_name(f"{pdf_path.stem}-metadata.json"),
        pdf_path.parent / "metadata.json",
        pdf_path.parent.parent / "metadata.json",
    ]
    for candidate in exact_candidates:
        if candidate.exists():
            return candidate

    fallback_candidates = sorted(pdf_path.parent.glob("*-metadata.json"))
    if len(fallback_candidates) == 1 and len(list(pdf_path.parent.glob("*.pdf"))) == 1:
        return fallback_candidates[0]

    return pdf_path.with_suffix(".json")


#Entrypoint function for the ingestion process, can be called from command line or other scripts.
def ingest_knowledge_base(
    knowledge_base_dir: Path | str = DEFAULT_KNOWLEDGE_BASE_DIR,
) -> KnowledgeBaseIngestionSummary:
    return IngestKnowledgeBaseUseCase().execute(knowledge_base_dir)


#TODO: Consider adding a CLI command to trigger this ingestion process, or integrate it with the FastAPI application startup if needed.
def main() -> None:
    import argparse

    parser = argparse.ArgumentParser(
        description="Ingest legal PDFs from the RiskTrace AI knowledge base."
    )
    parser.add_argument(
        "--knowledge-base-dir",
        type=Path,
        default=DEFAULT_KNOWLEDGE_BASE_DIR,
        help="Directory containing sources/ and processed/ folders.",
    )
    args = parser.parse_args()

    summary = ingest_knowledge_base(args.knowledge_base_dir)
    print(json.dumps(summary.to_dict(), ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
