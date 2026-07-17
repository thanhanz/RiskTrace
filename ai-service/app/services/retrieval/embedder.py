
"""Local text embedding service backed by BGE-M3."""

from __future__ import annotations

from typing import Any

from app.application.ports.embeddings import EmbeddingPort
from app.core.settings import settings


class BgeM3Embedder(EmbeddingPort):
    """Generate normalized local embeddings with ``BAAI/bge-m3``.

    The LangChain model is loaded lazily on the first embedding request so
    importing the service or constructing this class does not download or load
    the model immediately.
    """

    def __init__(
        self,
        model_name: str | None = None,
        device: str | None = None,
        batch_size: int | None = None,
        normalize_embeddings: bool | None = None,
    ) -> None:
        self.model_name = model_name or settings.embedding_model_name
        self.device = device if device is not None else settings.embedding_device
        self.batch_size = (
            batch_size if batch_size is not None else settings.embedding_batch_size
        )
        self.normalize_embeddings = (
            normalize_embeddings
            if normalize_embeddings is not None
            else settings.embedding_normalize
        )
        if self.batch_size <= 0:
            raise ValueError("Embedding batch_size must be greater than zero.")

        self._embeddings: Any | None = None

    @property
    def embeddings(self) -> Any:
        """Return the lazily initialized LangChain embedding implementation."""

        if self._embeddings is None:
            try:
                from langchain_huggingface import HuggingFaceEmbeddings
            except ImportError as exc:
                raise RuntimeError(
                    "langchain-huggingface and sentence-transformers are required "
                    "for BGE-M3 embeddings. Install ai-service requirements first."
                ) from exc

            model_kwargs: dict[str, str] = {}
            if self.device:
                model_kwargs["device"] = self.device

            self._embeddings = HuggingFaceEmbeddings(
                model_name=self.model_name,
                model_kwargs=model_kwargs,
                encode_kwargs={
                    "batch_size": self.batch_size,
                    "normalize_embeddings": self.normalize_embeddings,
                },
            )
        return self._embeddings

    def embed_text(self, text: str) -> list[float]:
        """Embed one non-empty text value."""

        normalized_text = _require_text(text)
        vector = self.embeddings.embed_query(normalized_text)
        return [float(value) for value in vector]

    def embed_documents(self, texts: list[str]) -> list[list[float]]:
        """Embed a batch of non-empty text values in input order."""

        if not texts:
            return []

        normalized_texts = [_require_text(text) for text in texts]
        vectors = self.embeddings.embed_documents(normalized_texts)
        return [[float(value) for value in vector] for vector in vectors]


def _require_text(text: str) -> str:
    if not isinstance(text, str):
        raise TypeError("Embedding input must be a string.")

    normalized_text = text.strip()
    if not normalized_text:
        raise ValueError("Embedding input must not be empty.")
    return normalized_text
