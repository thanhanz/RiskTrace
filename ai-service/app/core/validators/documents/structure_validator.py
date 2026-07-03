import re
from collections import defaultdict
from collections.abc import Iterable

from app.core.validators.documents.models import ChunkValidationIssue
from app.services.document.chunker import LegalChunk


class ChunkStructureValidator:
    """Validate document hierarchy and chunk sequence after chunking."""

    STRUCTURED_TYPES = {
        "article",
        "article_part",
        "clause",
        "clause_part",
        "point",
        "point_part",
    }
    ARTICLE_RE = re.compile("^(?P<number>\\d+)(?P<suffix>[a-zA-Z]?)$")
    POINT_MARKER_RE = re.compile("^\\s*[a-zA-Z\u0111\u0110]\\)", re.MULTILINE)
    POINT_ORDER = [
        "a",
        "b",
        "c",
        "d",
        "\u0111",
        "e",
        "g",
        "h",
        "i",
        "k",
        "l",
        "m",
        "n",
        "o",
        "p",
        "q",
        "r",
        "s",
        "t",
        "u",
        "v",
        "x",
        "y",
    ]
    POINT_ORDER_INDEX = {label: index + 1 for index, label in enumerate(POINT_ORDER)}

    def validate_many(
        self,
        chunks: list[LegalChunk],
    ) -> dict[int, tuple[ChunkValidationIssue, ...]]:
        issue_lists: dict[int, list[ChunkValidationIssue]] = {
            index: [] for index in range(len(chunks))
        }
        self._validate_articles(chunks, issue_lists)
        self._validate_clauses(chunks, issue_lists)
        self._validate_points(chunks, issue_lists)
        self._validate_nested_point_markers(chunks, issue_lists)
        return {index: tuple(issues) for index, issues in issue_lists.items()}

    def _validate_articles(
        self,
        chunks: list[LegalChunk],
        issue_lists: dict[int, list[ChunkValidationIssue]],
    ) -> None:
        groups: dict[
            tuple[str, str | None, str | None],
            list[tuple[int, LegalChunk, tuple[int, int]]],
        ] = defaultdict(list)
        seen_article_keys: set[tuple[str, str | None, str | None, str]] = set()
        seen_article_chunk_keys: set[tuple[str, str | None, str | None, str]] = set()

        for index, chunk in enumerate(chunks):
            if chunk.chunk_type not in self.STRUCTURED_TYPES:
                continue
            key = self._article_key(chunk)
            article_value = self._parse_article_value(chunk.position.article_number)
            if article_value is None:
                continue
            if chunk.chunk_type == "article" and key in seen_article_chunk_keys:
                issue_lists[index].append(
                    ChunkValidationIssue(
                        code="duplicate_article",
                        message="Article appears more than once in the same source, chapter, and section.",
                        value=chunk.position.article_number,
                    )
                )
                continue
            if chunk.chunk_type == "article":
                seen_article_chunk_keys.add(key)
            if key not in seen_article_keys:
                seen_article_keys.add(key)
                groups[self._article_scope_key(chunk)].append((index, chunk, article_value))

        for entries in groups.values():
            previous: tuple[int, int] | None = None
            previous_label: str | None = None
            for index, chunk, current in entries:
                if previous is None:
                    previous = current
                    previous_label = chunk.position.article_number
                    continue
                if current <= previous:
                    issue_lists[index].append(
                        ChunkValidationIssue(
                            code="article_sequence_regression",
                            message="Article number goes backward relative to the previous accepted article.",
                            value=f"previous={previous_label}; current={chunk.position.article_number}",
                        )
                    )
                elif not self._is_next_article(previous, current):
                    issue_lists[index].append(
                        ChunkValidationIssue(
                            code="missing_article",
                            message="Article sequence skips one or more expected article numbers.",
                            value=f"previous={previous_label}; current={chunk.position.article_number}",
                        )
                    )
                previous = current
                previous_label = chunk.position.article_number

    def _validate_clauses(
        self,
        chunks: list[LegalChunk],
        issue_lists: dict[int, list[ChunkValidationIssue]],
    ) -> None:
        groups: dict[tuple[str, str | None, str | None, str], list[tuple[int, LegalChunk, int]]] = defaultdict(list)
        seen_clause_keys: set[tuple[str, str | None, str | None, str, str]] = set()
        seen_clause_chunk_keys: set[tuple[str, str | None, str | None, str, str]] = set()

        for index, chunk in enumerate(chunks):
            clause_number = chunk.position.clause_number
            if chunk.chunk_type not in {"clause", "clause_part", "point", "point_part"}:
                continue
            if clause_number is None or not clause_number.isdigit():
                continue
            group_key = self._article_key(chunk)
            clause_key = (*group_key, clause_number)
            if chunk.chunk_type == "clause" and clause_key in seen_clause_chunk_keys:
                issue_lists[index].append(
                    ChunkValidationIssue(
                        code="duplicate_clause",
                        message="Clause appears more than once in the same article.",
                        value=clause_number,
                    )
                )
                continue
            if chunk.chunk_type == "clause":
                seen_clause_chunk_keys.add(clause_key)
            if clause_key not in seen_clause_keys:
                seen_clause_keys.add(clause_key)
                groups[group_key].append((index, chunk, int(clause_number)))

        for entries in groups.values():
            self._validate_numeric_sequence(
                entries,
                issue_lists,
                missing_code="missing_clause",
                regression_code="clause_sequence_regression",
                label_getter=lambda chunk: chunk.position.clause_number or "",
                missing_message="Clause sequence skips one or more expected clause numbers.",
                regression_message="Clause number goes backward relative to the previous accepted clause.",
            )

    def _validate_points(
        self,
        chunks: list[LegalChunk],
        issue_lists: dict[int, list[ChunkValidationIssue]],
    ) -> None:
        groups: dict[tuple[str, str | None, str | None, str, str], list[tuple[int, LegalChunk, int]]] = defaultdict(list)
        seen_point_keys: set[tuple[str, str | None, str | None, str, str, str]] = set()
        seen_point_chunk_keys: set[tuple[str, str | None, str | None, str, str, str]] = set()

        for index, chunk in enumerate(chunks):
            point_label = chunk.position.point_label
            clause_number = chunk.position.clause_number
            if chunk.chunk_type not in {"point", "point_part"}:
                continue
            if point_label is None or clause_number is None:
                continue
            point_order = self.POINT_ORDER_INDEX.get(point_label.lower())
            if point_order is None:
                continue
            article_key = self._article_key(chunk)
            group_key = (*article_key, clause_number)
            point_key = (*group_key, point_label.lower())
            if chunk.chunk_type == "point" and point_key in seen_point_chunk_keys:
                issue_lists[index].append(
                    ChunkValidationIssue(
                        code="duplicate_point",
                        message="Point appears more than once in the same clause.",
                        value=point_label,
                    )
                )
                continue
            if chunk.chunk_type == "point":
                seen_point_chunk_keys.add(point_key)
            if point_key not in seen_point_keys:
                seen_point_keys.add(point_key)
                groups[group_key].append((index, chunk, point_order))

        for entries in groups.values():
            self._validate_numeric_sequence(
                entries,
                issue_lists,
                missing_code="missing_point",
                regression_code="point_sequence_regression",
                label_getter=lambda chunk: chunk.position.point_label or "",
                missing_message="Point sequence skips one or more expected point labels.",
                regression_message="Point label goes backward relative to the previous accepted point.",
            )

    def _validate_nested_point_markers(
        self,
        chunks: list[LegalChunk],
        issue_lists: dict[int, list[ChunkValidationIssue]],
    ) -> None:
        for index, chunk in enumerate(chunks):
            if chunk.chunk_type not in {"point", "point_part"}:
                continue
            matches = list(self.POINT_MARKER_RE.finditer(chunk.text))
            own_marker_seen = False
            for match in matches:
                marker = match.group(0).strip().rstrip(")").lower()
                if not own_marker_seen and marker == (chunk.position.point_label or "").lower():
                    own_marker_seen = True
                    continue
                issue_lists[index].append(
                    ChunkValidationIssue(
                        code="nested_point_marker",
                        message="Point chunk text contains another point marker.",
                        value=match.group(0).strip(),
                    )
                )
                break

    def _validate_numeric_sequence(
        self,
        entries: Iterable[tuple[int, LegalChunk, int]],
        issue_lists: dict[int, list[ChunkValidationIssue]],
        *,
        missing_code: str,
        regression_code: str,
        label_getter,
        missing_message: str,
        regression_message: str,
    ) -> None:
        previous_value: int | None = None
        previous_label: str | None = None

        for index, chunk, current_value in entries:
            current_label = label_getter(chunk)
            if previous_value is None:
                previous_value = current_value
                previous_label = current_label
                continue
            if current_value <= previous_value:
                issue_lists[index].append(
                    ChunkValidationIssue(
                        code=regression_code,
                        message=regression_message,
                        value=f"previous={previous_label}; current={current_label}",
                    )
                )
            elif current_value != previous_value + 1:
                issue_lists[index].append(
                    ChunkValidationIssue(
                        code=missing_code,
                        message=missing_message,
                        value=f"previous={previous_label}; current={current_label}",
                    )
                )
            previous_value = current_value
            previous_label = current_label

    @staticmethod
    def _article_key(chunk: LegalChunk) -> tuple[str, str | None, str | None, str]:
        return (
            chunk.source.source_id,
            chunk.position.chapter,
            chunk.position.section,
            chunk.position.article_number,
        )

    @staticmethod
    def _article_scope_key(chunk: LegalChunk) -> tuple[str, str | None, str | None]:
        return (
            chunk.source.source_id,
            chunk.position.chapter,
            chunk.position.section,
        )

    @classmethod
    def _parse_article_value(cls, article_number: str) -> tuple[int, int] | None:
        match = cls.ARTICLE_RE.fullmatch(article_number)
        if not match:
            return None
        number = int(match.group("number"))
        suffix = match.group("suffix").lower()
        suffix_order = 0 if not suffix else ord(suffix) - ord("a") + 1
        return number, suffix_order

    @staticmethod
    def _is_next_article(previous: tuple[int, int], current: tuple[int, int]) -> bool:
        previous_number, previous_suffix = previous
        current_number, current_suffix = current
        if current == (previous_number + 1, 0):
            return True
        if previous_suffix == 0:
            return current == (previous_number, 1)
        return current == (previous_number, previous_suffix + 1)
