# TODO Register

This file records TODO markers found in the codebase and explains why each task exists, so the original intent is not lost.

## AI Service Knowledge Base Ingestion Trigger

- **Source:** `ai-service/app/application/use_cases/ingest_knowledge_base.py`
- **TODO:** This use case should only be called after an admin uploads knowledge-base PDFs and metadata to `knowledge_base/sources/`. A RabbitMQ consumer may trigger ingestion when a new PDF is uploaded through a future .NET endpoint such as `admin/upload-knowledge-base`.
- **Reason:** Startup ingestion is useful for the current proof of concept, but production ingestion should be an explicit admin workflow. Legal knowledge-base updates are controlled data operations: they need ownership, validation, failure reporting, and should not run unexpectedly every time the AI service starts.

## Knowledge Base Ingestion Command Surface

- **Source:** `ai-service/app/application/use_cases/ingest_knowledge_base.py`
- **TODO:** Consider adding a CLI command to trigger ingestion, or integrate the process with FastAPI startup if needed.
- **Reason:** The current module-level `main()` works for direct Python execution, but it is not a stable operator/admin interface. A clear command surface would make local ingestion, re-ingestion, and troubleshooting repeatable without relying on application startup behavior.

## Document Extraction Domain Types

- **Source:** `ai-service/app/services/document/extractor.py`
- **TODO:** Move entity/value-object-style classes such as `ExtractionWarning`, `ExtractedPage`, and `ExtractedDocument` to the domain layer.
- **Reason:** These types describe extracted-document data shared across extractors, chunking, and ingestion. Keeping them inside the extractor service couples domain data structures to one implementation area; moving them to a domain/model layer would make OCR, hybrid extraction, chunking, and later retrieval code depend on stable shared concepts.

## Startup Knowledge Base Ingestion Behavior

- **Source:** `ai-service/app/main.py`
- **TODO:** The startup-triggered ingestion is currently proof of concept. Decide whether ingestion should happen on application startup, through an admin action, or through a specific event.
- **Reason:** OCR and chunking can be slow and operationally expensive. Running ingestion during startup can delay service readiness, repeat work after restarts, and hide failures in application lifecycle logs. The final design should separate normal service startup from controlled knowledge-base refresh operations.

## Multipart Upload Abort Cleanup

- **Source:** `src/RiskTrace.UseCases/Interfaces/Ports/Storage/ICloudStorage.cs`
- **TODO:** Add multipart abort cleanup if abandoned uploads become a cost issue.
- **Reason:** Multipart uploads can leave unfinished upload sessions or temporary storage state when clients fail, users cancel, or network errors occur. If these abandoned uploads accumulate, they can increase storage cost and operational noise. Cleanup should be added when upload volume or provider billing makes it necessary.
