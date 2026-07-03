from dataclasses import asdict, dataclass
from datetime import date
import re

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

"""
    Validation for:
        - Metadata dates: strict YYYY-MM-DD
        - Article metadata/header: Điều <number>.
        - Decree references: Nghị định số <number>/<year>/NĐ-CP
        - Date references: dd/mm/yyyy, ngày <day> tháng <month> năm <year>
        - Suspicious OCR date fragments like ngày 1280

  - Ingestion now writes valid chunks to <source_id>.chunks.jsonl
  - Failed chunks are written to <source_id>.quarantine.jsonl with validation issue details
  - Ingestion summary now includes quarantine_path and quarantined_chunk_count
"""

class LegalChunkValidator:
    """Validate OCR-derived legal chunks before they become retrieval data."""

    """_summary_

    Returns:
        "Điều" + số (có thể kèm chữ cái, vd "12a") + dấu chấm. Ví dụ: "Điều 5.", "Điều 12a."
    """
    ARTICLE_HEADER_RE = re.compile(
        "^\\s*\u0110i\u1ec1u\\s+(?P<number>\\d+[a-zA-Z]?)\\.",
        re.IGNORECASE,
    )
    
    """_summary_

    Returns:
        Kiểm tra số điều hợp lệ: chuỗi chỉ gồm số, có thể kèm 1 chữ cái ở cuối (vd "5", "12a"), không có gì khác.
    """
    ARTICLE_NUMBER_RE = re.compile("^\\d+[a-zA-Z]?$")
    
    """_summary_

    Returns:
         YYYY-MM-DD, ví dụ "2024-01-15".
    """
    ISO_DATE_RE = re.compile("^\\d{4}-\\d{2}-\\d{2}$")
    
    """_summary_

    Returns:
        Bắt ngày dạng dd/mm/yyyy (có thể 1 hoặc 2 chữ số cho ngày/tháng): ví dụ "5/3/2023", "15/12/2023".
    """
    DATE_SLASH_RE = re.compile("\\b(?P<day>\\d{1,2})/(?P<month>\\d{1,2})/(?P<year>\\d{4})\\b")
    
    """_summary_

    Returns:
        Bắt ngày viết theo kiểu văn bản pháp luật Việt Nam: dạng "ngày X tháng Y năm ZZZZ", ví dụ "ngày 5 tháng 3 năm 2023".
    """
    VIETNAMESE_DATE_RE = re.compile(
        "\\bng\u00e0y\\s+(?P<day>\\d{1,2})\\s+th\u00e1ng\\s+"
        "(?P<month>\\d{1,2})\\s+n\u0103m\\s+(?P<year>\\d{4})\\b",
        re.IGNORECASE,
    )
    
    """_summary_

    Returns:
        Phát hiện số "ngày" bất thường: bắt trường hợp sau chữ "ngày" là số có từ 3 chữ số trở lên (không hợp lý vì ngày chỉ có 1-2 chữ số) — dùng để cảnh báo lỗi dữ liệu, ví dụ "ngày 123".
    """
    SUSPICIOUS_DAY_RE = re.compile("\\bng\u00e0y\\s+(?P<value>\\d{3,})\\b", re.IGNORECASE)
    
    """_summary_

    Returns:
        Bắt trích dẫn đầy đủ của Nghị định: dạng "Nghị định số XX/YYYY/NĐ-CP", ví dụ "Nghị định số 15/2023/NĐ-CP".
    """
    DECREE_FULL_RE = re.compile(
        "\\bNgh\u1ecb\\s+\u0111\u1ecbnh\\s+s\u1ed1\\s+"
        "\\d+/\\d{4}/N\u0110-CP\\b",
        re.IGNORECASE,
    )
    
    """_summary_

    Returns:
        - Bắt phần đầu của trích dẫn Nghị định (không yêu cầu đủ định dạng): khớp "Nghị định số" + bất kỳ chuỗi ký tự nào không phải khoảng trắng/dấu câu (, . ; : )) theo sau 
        — Dùng để bắt cả trường hợp trích dẫn thiếu hoặc sai định dạng, nhằm phát hiện lỗi.
    """
    DECREE_PREFIX_RE = re.compile(
        "\\bNgh\u1ecb\\s+\u0111\u1ecbnh\\s+s\u1ed1\\s+[^\\s,.;:)]*",
        re.IGNORECASE,
    )

    def validate_many(self, chunks: list[LegalChunk]) -> list[ChunkValidationResult]:
        issue_lists = [list(self.validate(chunk).issues) for chunk in chunks]

        for chunk_index, issue in self._validate_article_sequence(chunks):
            issue_lists[chunk_index].append(issue)

        return [
            ChunkValidationResult(chunk=chunk, issues=tuple(issue_lists[index]))
            for index, chunk in enumerate(chunks)
        ]

    def validate(self, chunk: LegalChunk) -> ChunkValidationResult:
        issues: list[ChunkValidationIssue] = []
        issues.extend(self._validate_article(chunk))
        issues.extend(self._validate_effect_dates(chunk))
        issues.extend(self._validate_decree_references(chunk.text))
        issues.extend(self._validate_date_references(chunk.text))

        return ChunkValidationResult(chunk=chunk, issues=tuple(issues))

    def _validate_article(self, chunk: LegalChunk) -> list[ChunkValidationIssue]:
        issues: list[ChunkValidationIssue] = []
        article_number = chunk.position.article_number

        if not self.ARTICLE_NUMBER_RE.fullmatch(article_number):
            issues.append(
                ChunkValidationIssue(
                    code="invalid_article_number",
                    message="Article number metadata does not match expected digits plus optional suffix.",
                    value=article_number,
                )
            )

        if chunk.chunk_type not in {"article", "article_part"}:
            return issues

        article_match = self.ARTICLE_HEADER_RE.match(chunk.text)
        if not article_match:
            if chunk.chunk_type == "article_part":
                return issues
            issues.append(
                ChunkValidationIssue(
                    code="missing_article_header",
                    message="Article chunk text does not start with the expected 'Dieu <number>.' header.",
                    value=chunk.text[:80],
                )
            )
            return issues

        header_number = article_match.group("number")
        if header_number != article_number:
            issues.append(
                ChunkValidationIssue(
                    code="article_number_mismatch",
                    message="Article header number does not match chunk position metadata.",
                    value=f"header={header_number}; metadata={article_number}",
                )
            )

        return issues

    def _validate_effect_dates(self, chunk: LegalChunk) -> list[ChunkValidationIssue]:
        issues: list[ChunkValidationIssue] = []
        date_fields = {
            "effective_date": chunk.effect.effective_date,
            "issued_date": chunk.effect.issued_date,
        }

        for field_name, value in date_fields.items():
            if value is None:
                continue
            if not self.ISO_DATE_RE.fullmatch(value):
                issues.append(
                    ChunkValidationIssue(
                        code=f"invalid_{field_name}",
                        message="Metadata date must use full ISO format YYYY-MM-DD.",
                        value=value,
                    )
                )
                continue
            if not self._is_valid_date_parts(value[0:4], value[5:7], value[8:10]):
                issues.append(
                    ChunkValidationIssue(
                        code=f"invalid_{field_name}",
                        message="Metadata date has invalid year, month, or day.",
                        value=value,
                    )
                )

        return issues

    def _validate_decree_references(self, text: str) -> list[ChunkValidationIssue]:
        issues: list[ChunkValidationIssue] = []

        for match in self.DECREE_PREFIX_RE.finditer(text):
            value = match.group(0)
            if not self.DECREE_FULL_RE.fullmatch(value):
                issues.append(
                    ChunkValidationIssue(
                        code="invalid_decree_number",
                        message="Decree reference must match 'Nghi dinh so <number>/<year>/ND-CP'.",
                        value=value,
                    )
                )

        return issues

    def _validate_date_references(self, text: str) -> list[ChunkValidationIssue]:
        issues: list[ChunkValidationIssue] = []

        for match in self.DATE_SLASH_RE.finditer(text):
            if not self._is_valid_date_parts(
                match.group("year"),
                match.group("month"),
                match.group("day"),
            ):
                issues.append(
                    ChunkValidationIssue(
                        code="invalid_slash_date",
                        message="Slash date has invalid day or month.",
                        value=match.group(0),
                    )
                )

        for match in self.VIETNAMESE_DATE_RE.finditer(text):
            if not self._is_valid_date_parts(
                match.group("year"),
                match.group("month"),
                match.group("day"),
            ):
                issues.append(
                    ChunkValidationIssue(
                        code="invalid_vietnamese_date",
                        message="Vietnamese date phrase has invalid day or month.",
                        value=match.group(0),
                    )
                )

        for match in self.SUSPICIOUS_DAY_RE.finditer(text):
            issues.append(
                ChunkValidationIssue(
                    code="suspicious_day_value",
                    message="Date-like phrase contains a day value with three or more digits.",
                    value=match.group(0),
                )
            )

        return issues

    def _validate_article_sequence(
        self,
        chunks: list[LegalChunk],
    ) -> list[tuple[int, ChunkValidationIssue]]:
        issues: list[tuple[int, ChunkValidationIssue]] = []
        last_accepted_article: tuple[int, int] | None = None
        last_accepted_label: str | None = None
        seen_article_chunk_keys: set[tuple[str, str | None, str | None, str]] = set()

        for index, chunk in enumerate(chunks):
            if chunk.chunk_type not in {
                "article",
                "article_part",
                "clause",
                "clause_part",
                "point",
                "point_part",
            }:
                continue

            if chunk.chunk_type in {
                "article_part",
                "clause",
                "clause_part",
                "point",
                "point_part",
            }:
                article_key = (
                    chunk.source.source_id,
                    chunk.position.chapter,
                    chunk.position.section,
                    chunk.position.article_number,
                )
                if article_key in seen_article_chunk_keys:
                    continue
                seen_article_chunk_keys.add(article_key)

            current_article = self._parse_article_sequence_value(
                chunk.position.article_number
            )
            if current_article is None:
                continue

            if last_accepted_article is not None and current_article <= last_accepted_article:
                issues.append(
                    (
                        index,
                        ChunkValidationIssue(
                            code="article_sequence_regression",
                            message=(
                                "Article number goes backward or repeats relative "
                                "to the previous accepted article."
                            ),
                            value=(
                                f"previous={last_accepted_label}; "
                                f"current={chunk.position.article_number}"
                            ),
                        ),
                    )
                )
                continue

            last_accepted_article = current_article
            last_accepted_label = chunk.position.article_number

        return issues

    @staticmethod
    def _parse_article_sequence_value(article_number: str) -> tuple[int, int] | None:
        match = re.fullmatch("(?P<number>\\d+)(?P<suffix>[a-zA-Z]?)", article_number)
        if not match:
            return None

        number = int(match.group("number"))
        suffix = match.group("suffix").lower()
        suffix_order = 0 if not suffix else ord(suffix) - ord("a") + 1
        return number, suffix_order

    @staticmethod
    def _is_valid_date_parts(year: str, month: str, day: str) -> bool:
        try:
            date(int(year), int(month), int(day))
        except ValueError:
            return False
        return True
