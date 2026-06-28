from dataclasses import dataclass, field
from pathlib import Path
from typing import Protocol


@dataclass(frozen=True)
class ExtractionWarning:
    page_number: int | None
    code: str
    message: str


@dataclass(frozen=True)
class ExtractedPage:
    page_number: int
    text: str
    warnings: tuple[ExtractionWarning, ...] = ()


@dataclass(frozen=True)
class ExtractedDocument:
    source_path: Path
    pages: tuple[ExtractedPage, ...]
    warnings: tuple[ExtractionWarning, ...] = field(default_factory=tuple)

    @property
    def text(self) -> str:
        return "\n\n".join(page.text for page in self.pages if page.text.strip())

#TODO: Move all the class stored like entities or value objects to Domain layer.
class DocumentExtractor(Protocol):
    def extract(self, path: Path) -> ExtractedDocument:
        ...


class PdfTextExtractor:
    """Extract selectable text from PDFs while preserving page numbers."""

    def __init__(self, min_text_chars_per_page: int = 20) -> None:
        self.min_text_chars_per_page = min_text_chars_per_page

    def extract(self, path: Path) -> ExtractedDocument:
        pdf_path = Path(path)
        if pdf_path.suffix.lower() != ".pdf":
            raise ValueError(f"Unsupported document type: {pdf_path.suffix}")

        try:
            from pypdf import PdfReader
        except ImportError as exc:
            raise RuntimeError(
                "pypdf is required for PDF text extraction. Install dependencies from "
                "ai-service/requirements.txt before running ingestion."
            ) from exc

        reader = PdfReader(str(pdf_path))
        pages: list[ExtractedPage] = []
        document_warnings: list[ExtractionWarning] = []

        for index, page in enumerate(reader.pages, start=1):
            raw_text = page.extract_text() or ""
            text = self._normalize_text(raw_text)
            page_warnings: list[ExtractionWarning] = []

            if not text:
                page_warnings.append(
                    ExtractionWarning(
                        page_number=index,
                        code="empty_page_text",
                        message="No selectable text was extracted from this page.",
                    )
                )
            elif len(text) < self.min_text_chars_per_page:
                page_warnings.append(
                    ExtractionWarning(
                        page_number=index,
                        code="low_page_text",
                        message=(
                            "Very little selectable text was extracted from this page; "
                            "the PDF may need OCR."
                        ),
                    )
                )

            document_warnings.extend(page_warnings)
            pages.append(
                ExtractedPage(
                    page_number=index,
                    text=text,
                    warnings=tuple(page_warnings),
                )
            )

        if not any(page.text.strip() for page in pages):
            document_warnings.append(
                ExtractionWarning(
                    page_number=None,
                    code="empty_document_text",
                    message="No selectable text was extracted from the PDF.",
                )
            )

        return ExtractedDocument(
            source_path=pdf_path,
            pages=tuple(pages),
            warnings=tuple(document_warnings),
        )

    @staticmethod
    def normalize_text(text: str) -> str:
        lines = [" ".join(line.split()) for line in text.replace("\x00", "").splitlines()]
        return "\n".join(line for line in lines if line)

    @staticmethod
    def _normalize_text(text: str) -> str:
        return PdfTextExtractor.normalize_text(text)
