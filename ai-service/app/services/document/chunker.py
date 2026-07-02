from dataclasses import asdict, dataclass
import re

from app.services.document.extractor import ExtractedDocument, ExtractedPage


@dataclass(frozen=True)
class SourceMetadata:
    source_id: str
    title: str
    source_url: str | None
    jurisdiction: str
    document_type: str
    status: str
    version: str


@dataclass(frozen=True)
class LegalEffect:
    issued_date: str | None
    effective_date: str


@dataclass(frozen=True)
class LegalPosition:
    chapter: str | None
    chapter_title: str | None
    section: str | None
    section_title: str | None
    article_number: str
    article_title: str | None
    clause_number: str | None = None
    point_label: str | None = None


@dataclass(frozen=True)
class ChunkTrace:
    page_start: int
    page_end: int


@dataclass(frozen=True)
class ChunkParent:
    parent_chunk_id: str | None
    parent_position: dict[str, str | None]


@dataclass(frozen=True)
class LegalChunk:
    chunk_id: str
    chunk_type: str
    text: str
    source: SourceMetadata
    position: LegalPosition
    effect: LegalEffect
    trace: ChunkTrace
    parent: ChunkParent

    def to_dict(self) -> dict:
        return asdict(self)


@dataclass
class _ArticleLine:
    text: str
    page_number: int


@dataclass
class _ClauseBuffer:
    clause_number: str
    lines: list[_ArticleLine]


@dataclass
class _PointBuffer:
    point_label: str
    lines: list[_ArticleLine]


@dataclass
class _ArticleBuffer:
    article_number: str
    article_title: str | None
    chapter: str | None
    chapter_title: str | None
    section: str | None
    section_title: str | None
    page_start: int
    page_end: int
    lines: list[_ArticleLine]


class LegalKnowledgeBaseChunker:
    """Split Vietnamese legal text into article, clause, and point chunks."""

    CHAPTER_RE = re.compile(
        "^\\s*Ch\u01b0\u01a1ng\\s+([IVXLCDM\\d]+)\\.?\\s*(.*)$",
        re.IGNORECASE,
    )
    SECTION_RE = re.compile(
        "^\\s*M\u1ee5c\\s+([IVXLCDM\\d]+)\\.?\\s*(.*)$",
        re.IGNORECASE,
    )
    ARTICLE_RE = re.compile(
        "^\\s*\u0110i\u1ec1u\\s+(\\d+[a-zA-Z]?)\\.\\s*(.*)$",
        re.IGNORECASE,
    )
    CLAUSE_RE = re.compile("^\\s*(\\d+)\\.(?:\\s+|$)(.*)$")
    POINT_RE = re.compile("^\\s*([a-zA-Z\u0111\u0110])\\)(?:\\s+|$)(.*)$")

    def __init__(self, max_chunk_chars: int = 6000) -> None:
        self.max_chunk_chars = max_chunk_chars

    def chunk(
        self,
        document: ExtractedDocument,
        source: SourceMetadata,
        effect: LegalEffect,
    ) -> list[LegalChunk]:
        current_chapter: str | None = None
        current_chapter_title: str | None = None
        current_section: str | None = None
        current_section_title: str | None = None
        current_article: _ArticleBuffer | None = None
        article_buffers: list[_ArticleBuffer] = []

        for page in document.pages:
            for line in self._iter_lines(page):
                chapter_match = self.CHAPTER_RE.match(line)
                if chapter_match:
                    if current_article:
                        article_buffers.append(current_article)
                        current_article = None
                    current_chapter = f"Ch\u01b0\u01a1ng {chapter_match.group(1)}"
                    current_chapter_title = self._clean_optional(chapter_match.group(2))
                    current_section = None
                    current_section_title = None
                    continue

                section_match = self.SECTION_RE.match(line)
                if section_match:
                    if current_article:
                        article_buffers.append(current_article)
                        current_article = None
                    current_section = f"M\u1ee5c {section_match.group(1)}"
                    current_section_title = self._clean_optional(section_match.group(2))
                    continue

                article_match = self.ARTICLE_RE.match(line)
                if article_match:
                    if current_article:
                        article_buffers.append(current_article)

                    article_number = article_match.group(1)
                    article_title = self._clean_optional(article_match.group(2))
                    current_article = _ArticleBuffer(
                        article_number=article_number,
                        article_title=article_title,
                        chapter=current_chapter,
                        chapter_title=current_chapter_title,
                        section=current_section,
                        section_title=current_section_title,
                        page_start=page.page_number,
                        page_end=page.page_number,
                        lines=[_ArticleLine(text=line, page_number=page.page_number)],
                    )
                    continue

                if current_article:
                    current_article.lines.append(
                        _ArticleLine(text=line, page_number=page.page_number)
                    )
                    current_article.page_end = page.page_number

        if current_article:
            article_buffers.append(current_article)

        chunks: list[LegalChunk] = []
        for article in article_buffers:
            chunks.extend(self._build_chunks(article, source, effect))

        return chunks

    @staticmethod
    def _iter_lines(page: ExtractedPage) -> list[str]:
        return [line.strip() for line in page.text.splitlines() if line.strip()]

    @staticmethod
    def _clean_optional(value: str | None) -> str | None:
        if value is None:
            return None
        cleaned = value.strip(" .\t")
        return cleaned or None

    def _build_chunks(
        self,
        article: _ArticleBuffer,
        source: SourceMetadata,
        effect: LegalEffect,
    ) -> list[LegalChunk]:
        clause_buffers = self._split_article_clauses(article)
        if clause_buffers:
            chunks: list[LegalChunk] = []
            for clause in clause_buffers:
                chunks.extend(self._build_clause_chunks(article, clause, source, effect))
            return chunks

        text = self._join_lines(article.lines)
        base_chunk_id = f"{source.source_id}:article-{article.article_number}"
        position = LegalPosition(
            chapter=article.chapter,
            chapter_title=article.chapter_title,
            section=article.section,
            section_title=article.section_title,
            article_number=article.article_number,
            article_title=article.article_title,
        )
        trace = ChunkTrace(page_start=article.page_start, page_end=article.page_end)
        parent_position = {
            "chapter": article.chapter,
            "section": article.section,
        }

        if len(text) <= self.max_chunk_chars:
            return [
                LegalChunk(
                    chunk_id=base_chunk_id,
                    chunk_type="article",
                    text=text,
                    source=source,
                    position=position,
                    effect=effect,
                    trace=trace,
                    parent=ChunkParent(
                        parent_chunk_id=None,
                        parent_position=parent_position,
                    ),
                )
            ]

        child_texts = self._split_long_text(text)
        return [
            LegalChunk(
                chunk_id=f"{base_chunk_id}:part-{index}",
                chunk_type="article_part",
                text=child_text,
                source=source,
                position=position,
                effect=effect,
                trace=trace,
                parent=ChunkParent(
                    parent_chunk_id=base_chunk_id,
                    parent_position=parent_position,
                ),
            )
            for index, child_text in enumerate(child_texts, start=1)
        ]

    def _build_clause_chunks(
        self,
        article: _ArticleBuffer,
        clause: _ClauseBuffer,
        source: SourceMetadata,
        effect: LegalEffect,
    ) -> list[LegalChunk]:
        point_buffers = self._split_clause_points(clause)
        if point_buffers:
            chunks: list[LegalChunk] = []
            for point in point_buffers:
                chunks.extend(
                    self._build_point_chunks(article, clause, point, source, effect)
                )
            return chunks

        text = self._join_lines(clause.lines)
        article_chunk_id = f"{source.source_id}:article-{article.article_number}"
        clause_chunk_id = f"{article_chunk_id}:clause-{clause.clause_number}"
        position = LegalPosition(
            chapter=article.chapter,
            chapter_title=article.chapter_title,
            section=article.section,
            section_title=article.section_title,
            article_number=article.article_number,
            article_title=article.article_title,
            clause_number=clause.clause_number,
        )
        trace = ChunkTrace(
            page_start=clause.lines[0].page_number,
            page_end=clause.lines[-1].page_number,
        )
        parent_position = {
            "chapter": article.chapter,
            "section": article.section,
            "article": article.article_number,
        }

        if len(text) <= self.max_chunk_chars:
            return [
                LegalChunk(
                    chunk_id=clause_chunk_id,
                    chunk_type="clause",
                    text=text,
                    source=source,
                    position=position,
                    effect=effect,
                    trace=trace,
                    parent=ChunkParent(
                        parent_chunk_id=article_chunk_id,
                        parent_position=parent_position,
                    ),
                )
            ]

        child_texts = self._split_long_text(text)
        return [
            LegalChunk(
                chunk_id=f"{clause_chunk_id}:part-{index}",
                chunk_type="clause_part",
                text=child_text,
                source=source,
                position=position,
                effect=effect,
                trace=trace,
                parent=ChunkParent(
                    parent_chunk_id=clause_chunk_id,
                    parent_position=parent_position,
                ),
            )
            for index, child_text in enumerate(child_texts, start=1)
        ]

    def _build_point_chunks(
        self,
        article: _ArticleBuffer,
        clause: _ClauseBuffer,
        point: _PointBuffer,
        source: SourceMetadata,
        effect: LegalEffect,
    ) -> list[LegalChunk]:
        text = self._join_lines(point.lines)
        article_chunk_id = f"{source.source_id}:article-{article.article_number}"
        clause_chunk_id = f"{article_chunk_id}:clause-{clause.clause_number}"
        point_chunk_id = f"{clause_chunk_id}:point-{point.point_label}"
        position = LegalPosition(
            chapter=article.chapter,
            chapter_title=article.chapter_title,
            section=article.section,
            section_title=article.section_title,
            article_number=article.article_number,
            article_title=article.article_title,
            clause_number=clause.clause_number,
            point_label=point.point_label,
        )
        trace = ChunkTrace(
            page_start=point.lines[0].page_number,
            page_end=point.lines[-1].page_number,
        )
        parent_position = {
            "chapter": article.chapter,
            "section": article.section,
            "article": article.article_number,
            "clause": clause.clause_number,
        }

        if len(text) <= self.max_chunk_chars:
            return [
                LegalChunk(
                    chunk_id=point_chunk_id,
                    chunk_type="point",
                    text=text,
                    source=source,
                    position=position,
                    effect=effect,
                    trace=trace,
                    parent=ChunkParent(
                        parent_chunk_id=clause_chunk_id,
                        parent_position=parent_position,
                    ),
                )
            ]

        child_texts = self._split_long_text(text)
        return [
            LegalChunk(
                chunk_id=f"{point_chunk_id}:part-{index}",
                chunk_type="point_part",
                text=child_text,
                source=source,
                position=position,
                effect=effect,
                trace=trace,
                parent=ChunkParent(
                    parent_chunk_id=point_chunk_id,
                    parent_position=parent_position,
                ),
            )
            for index, child_text in enumerate(child_texts, start=1)
        ]

    def _split_article_clauses(self, article: _ArticleBuffer) -> list[_ClauseBuffer]:
        header_lines = article.lines[:1]
        intro_lines: list[_ArticleLine] = []
        clauses: list[_ClauseBuffer] = []
        current_clause: _ClauseBuffer | None = None

        for line in article.lines[1:]:
            clause_match = self.CLAUSE_RE.match(line.text)
            if clause_match:
                if current_clause:
                    clauses.append(current_clause)

                clause_lines = list(header_lines)
                if not clauses and current_clause is None:
                    clause_lines.extend(intro_lines)
                clause_lines.append(line)
                current_clause = _ClauseBuffer(
                    clause_number=clause_match.group(1),
                    lines=clause_lines,
                )
                continue

            if current_clause:
                current_clause.lines.append(line)
            else:
                intro_lines.append(line)

        if current_clause:
            clauses.append(current_clause)

        return clauses

    def _split_clause_points(self, clause: _ClauseBuffer) -> list[_PointBuffer]:
        clause_header_index = self._find_clause_header_index(clause)
        if clause_header_index is None:
            return []

        context_lines = clause.lines[: clause_header_index + 1]
        intro_lines: list[_ArticleLine] = []
        points: list[_PointBuffer] = []
        current_point: _PointBuffer | None = None

        for line in clause.lines[clause_header_index + 1 :]:
            point_match = self.POINT_RE.match(line.text)
            if point_match:
                if current_point:
                    points.append(current_point)

                point_lines = list(context_lines)
                if not points and current_point is None:
                    point_lines.extend(intro_lines)
                point_lines.append(line)
                current_point = _PointBuffer(
                    point_label=point_match.group(1).lower(),
                    lines=point_lines,
                )
                continue

            if current_point:
                current_point.lines.append(line)
            else:
                intro_lines.append(line)

        if current_point:
            points.append(current_point)

        return points

    def _find_clause_header_index(self, clause: _ClauseBuffer) -> int | None:
        for index, line in enumerate(clause.lines):
            if self.CLAUSE_RE.match(line.text):
                return index
        return None

    @staticmethod
    def _join_lines(lines: list[_ArticleLine]) -> str:
        return "\n".join(line.text for line in lines).strip()

    def _split_long_text(self, text: str) -> list[str]:
        paragraphs = [
            paragraph.strip()
            for paragraph in re.split(r"\n{2,}", text)
            if paragraph.strip()
        ]
        if len(paragraphs) <= 1:
            paragraphs = [line.strip() for line in text.splitlines() if line.strip()]

        chunks: list[str] = []
        current: list[str] = []
        current_size = 0

        for paragraph in paragraphs:
            separator_size = 2 if current else 0
            if current and current_size + separator_size + len(paragraph) > self.max_chunk_chars:
                chunks.append("\n\n".join(current))
                current = [paragraph]
                current_size = len(paragraph)
            else:
                current.append(paragraph)
                current_size += separator_size + len(paragraph)

        if current:
            chunks.append("\n\n".join(current))

        return chunks
