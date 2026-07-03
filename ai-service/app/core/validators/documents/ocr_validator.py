import re

from app.core.validators.documents.models import ChunkValidationIssue
from app.services.document.chunker import LegalChunk


class OcrSuspiciousValidator:
    """Detect suspicious OCR artifacts without rewriting legal text."""

    SUSPICIOUS_DAY_RE = re.compile("\\bng\u00e0y\\s+(?P<value>\\d{3,})\\b", re.IGNORECASE)
    SUSPICIOUS_POINT_MARKER_RE = re.compile("(?<!\\w)\\(?\u00f8\\)", re.IGNORECASE)
    SUSPICIOUS_ROMAN_MARKER_RE = re.compile("^\\s*I1\\.(?:\\s+|$)", re.MULTILINE)
    SUSPICIOUS_VIETNAMESE_WORD_RE = re.compile(
        "\\bqu\u1ed1c\\s+1a\\b",
        re.IGNORECASE,
    )
    ROMAN_HEADING_RE = re.compile(
        "^\\s*(?:Ch\u01b0\u01a1ng|M\u1ee5c)\\s+(?P<label>[IVXLCDM]+)\\b",
        re.IGNORECASE | re.MULTILINE,
    )

    def validate(self, chunk: LegalChunk) -> tuple[ChunkValidationIssue, ...]:
        issues: list[ChunkValidationIssue] = []
        text = chunk.text

        for match in self.SUSPICIOUS_POINT_MARKER_RE.finditer(text):
            issues.append(
                ChunkValidationIssue(
                    code="suspicious_point_marker_ocr",
                    message="Text contains a point marker that may be an OCR artifact.",
                    value=match.group(0),
                )
            )

        for match in self.SUSPICIOUS_ROMAN_MARKER_RE.finditer(text):
            issues.append(
                ChunkValidationIssue(
                    code="suspicious_roman_numeral_marker",
                    message="Text contains 'I1.' where a Roman numeral marker may have been intended.",
                    value=match.group(0).strip(),
                )
            )

        for match in self.SUSPICIOUS_VIETNAMESE_WORD_RE.finditer(text):
            issues.append(
                ChunkValidationIssue(
                    code="suspicious_vietnamese_word_ocr",
                    message="Text contains a Vietnamese word fragment that may be an OCR artifact.",
                    value=match.group(0),
                )
            )

        for value in self._heading_values(chunk):
            if value is None:
                continue
            issues.extend(self._validate_roman_heading(value))

        for match in self.SUSPICIOUS_DAY_RE.finditer(text):
            issues.append(
                ChunkValidationIssue(
                    code="suspicious_day_value",
                    message="Date-like phrase contains a day value with three or more digits.",
                    value=match.group(0),
                )
            )

        return tuple(issues)

    def _validate_roman_heading(self, text: str) -> list[ChunkValidationIssue]:
        issues: list[ChunkValidationIssue] = []

        for match in self.ROMAN_HEADING_RE.finditer(text):
            label = match.group("label").upper()
            if not self._is_valid_roman_numeral(label):
                issues.append(
                    ChunkValidationIssue(
                        code="suspicious_roman_numeral_heading",
                        message="Chapter or section heading contains an invalid Roman numeral pattern.",
                        value=match.group(0).strip(),
                    )
                )

        return issues

    @staticmethod
    def _heading_values(chunk: LegalChunk) -> tuple[str | None, ...]:
        return (
            chunk.text,
            chunk.position.chapter,
            chunk.position.section,
        )

    @staticmethod
    def _is_valid_roman_numeral(value: str) -> bool:
        if not value:
            return False
        roman_re = re.compile(
            "^M{0,3}(CM|CD|D?C{0,3})"
            "(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$"
        )
        return bool(roman_re.fullmatch(value))
