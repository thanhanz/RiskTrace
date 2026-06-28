from collections.abc import Iterable
from pathlib import Path

from app.core.logger import get_logger
from app.core.settings import settings
from app.services.document.extractor import (
    DocumentExtractor,
    ExtractedDocument,
    ExtractedPage,
    ExtractionWarning,
    PdfTextExtractor,
)


logger = get_logger(__name__)


class OcrPdfExtractor:
    """Extract text from scanned PDF pages using Tesseract OCR."""

    def __init__(
        self,
        language: str = settings.ocr_language,
        dpi: int = settings.ocr_dpi,
        min_text_chars_per_page: int = settings.ocr_min_text_chars_per_page,
    ) -> None:
        self.language = language
        self.dpi = dpi
        self.min_text_chars_per_page = min_text_chars_per_page

    def extract(self, path: Path) -> ExtractedDocument:
        return self._extract(path)

    def extract_pages(self, path: Path, page_numbers: Iterable[int]) -> ExtractedDocument:
        return self._extract(path, set(page_numbers))

    def _extract(
        self,
        path: Path,
        page_numbers: set[int] | None = None,
    ) -> ExtractedDocument:
        pdf_path = Path(path)
        if pdf_path.suffix.lower() != ".pdf":
            raise ValueError(f"Unsupported document type: {pdf_path.suffix}")

        try:
            import fitz
            from PIL import Image
            import pytesseract
        except ImportError as exc:
            raise RuntimeError(
                "OCR extraction requires PyMuPDF, Pillow, and pytesseract. "
                "Install ai-service dependencies before running OCR ingestion."
            ) from exc

        try:
            pytesseract.get_tesseract_version()
        except Exception as exc:
            raise RuntimeError(
                "Tesseract OCR is not available. Install the tesseract binary and "
                "Vietnamese language data before running OCR ingestion."
            ) from exc

        try:
            document = fitz.open(str(pdf_path))
        except Exception as exc:
            raise RuntimeError(f"Unable to open PDF for OCR: {pdf_path}") from exc

        pages: list[ExtractedPage] = []
        document_warnings: list[ExtractionWarning] = []
        scale = self.dpi / 72
        matrix = fitz.Matrix(scale, scale)
        selected_page_count = len(page_numbers) if page_numbers is not None else document.page_count
        logger.info(
            "Starting OCR for '%s': %s page(s), language '%s', %s DPI.",
            pdf_path.name,
            selected_page_count,
            self.language,
            self.dpi,
        )

        try:
            for index, page in enumerate(document, start=1):
                if page_numbers is not None and index not in page_numbers:
                    continue

                logger.info(
                    "OCR page %s/%s for '%s'.",
                    index,
                    document.page_count,
                    pdf_path.name,
                )
                page_warnings = [
                    ExtractionWarning(
                        page_number=index,
                        code="ocr_used",
                        message="OCR was used to extract text from this page.",
                    )
                ]

                try:
                    pixmap = page.get_pixmap(matrix=matrix, alpha=False)
                    image = Image.frombytes(
                        "RGB",
                        (pixmap.width, pixmap.height),
                        pixmap.samples,
                    )
                    raw_text = pytesseract.image_to_string(image, lang=self.language)
                    text = PdfTextExtractor.normalize_text(raw_text)
                except Exception as exc:
                    text = ""
                    page_warnings.append(
                        ExtractionWarning(
                            page_number=index,
                            code="ocr_failed",
                            message=f"OCR failed for this page: {exc}",
                        )
                    )

                if not text:
                    page_warnings.append(
                        ExtractionWarning(
                            page_number=index,
                            code="ocr_empty_page_text",
                            message="OCR did not extract text from this page.",
                        )
                    )
                elif len(text) < self.min_text_chars_per_page:
                    page_warnings.append(
                        ExtractionWarning(
                            page_number=index,
                            code="ocr_low_page_text",
                            message="OCR extracted very little text from this page.",
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
                logger.info(
                    "OCR page %s/%s for '%s' extracted %s character(s).",
                    index,
                    document.page_count,
                    pdf_path.name,
                    len(text),
                )
        finally:
            document.close()

        if not any(page.text.strip() for page in pages):
            document_warnings.append(
                ExtractionWarning(
                    page_number=None,
                    code="ocr_empty_document_text",
                    message="OCR did not extract usable text from the PDF.",
                )
            )

        return ExtractedDocument(
            source_path=pdf_path,
            pages=tuple(pages),
            warnings=tuple(document_warnings),
        )

