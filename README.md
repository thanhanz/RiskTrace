# RiskTrace

RiskTrace is an in-progress backend for a legal-tech style document review workflow. The project is being built as an ASP.NET Core Web API with Clean Architecture, PostgreSQL persistence, JWT-based authentication, cloud-backed document upload, RabbitMQ messaging, NLog-based application tracing, and an integration boundary for an external AI service that will analyze uploaded legal documents and support session-based conversations.

The current codebase is stronger in architecture and backend foundations than in end-user feature completeness. Authentication, persistence, database migrations, dependency wiring, cloud storage upload preparation, RabbitMQ publish infrastructure, and use case trace logging are implemented. Review-result work is still incomplete, and a few document-related use case files remain placeholders.

## What Exists Today

### Implemented

- Layered solution split into `Api`, `UseCases`, `Domain`, `Infrastructure`, and `Core`
- ASP.NET Core API entry point with Swagger enabled in development
- PostgreSQL integration with Entity Framework Core and automatic migration on startup
- Database schema for users, review sessions, documents, messages, review results, and refresh tokens
- Authentication use cases for register, login, refresh token rotation, logout, and authenticated profile lookup
- JWT access token generation and hashed refresh token storage
- Password hashing with `BCrypt.Net`
- Redis-backed token blacklist wiring
- Review session use cases for create, list, detail, rename, and soft-delete
- R2/S3-compatible document upload initiation and upload completion metadata persistence
- Session message use cases for send and paginated retrieval
- RabbitMQ publish flow for document-upload events after database commit
- Custom `ILogger<T>` abstraction in `RiskTrace.Core` with an NLog-backed adapter in `RiskTrace.Infrastructure`
- Structured trace logging across implemented auth, session, document, and message use cases
- Docker Compose setup for the API, PostgreSQL database, and Redis

### Partially Implemented / Scaffolded

- Domain entities and enums for review sessions, documents, messages, and review results
- Repository abstractions and EF Core repository implementations
- API controller files and use case files for sessions, documents, messages, and review results
- Messaging contracts and RabbitMQ publisher infrastructure
- AI client boundary for future document analysis and chat integration

### Not Yet Implemented

- RabbitMQ consumer flow for AI review completion messages
- External AI document ingestion subscriber
- Review result generation flow
- Real HTTP integration with the AI service

## Architecture

The solution follows a clean, dependency-conscious structure:

```text
src/
|-- RiskTrace.Api
|-- RiskTrace.Core
|-- RiskTrace.Domain
|-- RiskTrace.UseCases
`-- RiskTrace.Infrastructure
```

### Layer Responsibilities

- `RiskTrace.Api`: HTTP entry point, controllers, composition root
- `RiskTrace.UseCases`: application orchestration and use case contracts
- `RiskTrace.Domain`: domain entities, enums, request/response models, event contracts, messaging constants
- `RiskTrace.Infrastructure`: EF Core, repositories, auth services, storage, RabbitMQ publisher, NLog logger adapter, AI integration adapters
- `RiskTrace.Core`: shared abstractions, logger contracts, and common result/pagination primitives

### Dependency Direction

```text
Api -> UseCases, Core
Infrastructure -> UseCases, Domain, Core
UseCases -> Domain, Core
Domain -> Core
```

This keeps business-facing code separated from infrastructure concerns and makes it easier to evolve adapters such as storage, database access, or AI integration later.

## Current Domain Model

The backend is organized around a review session owned by a user:

```text
User
`-- ReviewSession
    |-- Document
    |-- Message
    `-- ReviewResult
```

Current entities in the model:

- `User`
- `RefreshToken`
- `ReviewSession`
- `Document`
- `Message`
- `ReviewResult`

This model sets up the intended workflow: a user creates a review session, uploads one or more documents, receives generated review output, and continues the review through session messages.

## Authentication Design

Authentication is the most complete vertical slice in the project right now, and it now includes trace logging at the use case layer.

Implemented auth flow:

- `POST /api/v1/register`
- `POST /api/v1/login`
- `POST /api/v1/refresh`
- `POST /api/v1/logout`

Technical details:

- Access tokens are generated as JWTs
- Refresh tokens are generated separately, hashed with SHA-256 before persistence, and rotated on refresh
- Passwords are hashed with BCrypt
- Access tokens are also written to an HTTP-only cookie

## Persistence and Database

The project uses Entity Framework Core with PostgreSQL.

Current persistence work includes:

- `AppDbContext` with entity sets for all core tables
- Fluent configuration classes under `Infrastructure/Persistence/Configurations`
- Initial schema migration
- Additional migration for refresh tokens
- Generic read/write repository abstractions plus specific repositories for domain aggregates

The database is started through `docker-compose.yaml`, and the API applies migrations on startup.

## Document Upload and Messaging

The document upload flow is currently split into two backend steps:

- `InitiateDocumentUploadUseCase` creates a document id and returns a presigned upload target for R2/S3-compatible storage
- `CompleteDocumentUploadUseCase` validates the uploaded object, saves document metadata, commits the database transaction, then publishes a RabbitMQ event

RabbitMQ is exposed to use cases through the `IMessageQueueService` port in `RiskTrace.UseCases`. The concrete publisher lives in `RiskTrace.Infrastructure/Messaging`, so application logic does not depend on RabbitMQ client types.

Current messaging contract:

- Exchange: `risktrace.events`
- Document indexing queue: `risktrace.documents.index`
- Document upload routing key: `document.indexing_requested`
- Published event: `DocumentUploadedEvent`

The publish operation happens after `SaveChangesAsync` succeeds. If RabbitMQ publishing fails, the error is logged and the HTTP upload-completion response still succeeds. This is intentional for now; a transactional outbox or dead-letter/retry strategy can be added later when stronger delivery guarantees are needed.

## Logging and Trace Flow

Application tracing is wired through a custom `RiskTrace.Core.Interfaces.Logger.ILogger<T>` abstraction so use case code does not depend directly on `NLog` or `Microsoft.Extensions.Logging`.

Current logging design:

- `RiskTrace.Infrastructure` provides `NLogger<T>` as the concrete adapter
- NLog writes rolling log files using `LoggerConstants`
- The logger is registered in infrastructure dependency injection as an open generic singleton
- Authentication, session, document, and message use cases log request entry, expected failure branches, and successful completion

RabbitMQ settings are configured under the `RabbitMq` section:

```json
{
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  }
}
```

## Tech Stack

- `.NET 10`
- `ASP.NET Core Web API`
- `Entity Framework Core`
- `PostgreSQL`
- `Redis`
- `RabbitMQ`
- `NLog`
- `BCrypt.Net`
- `JWT`
- `R2 / S3-compatible object storage`
- `Swagger / Swashbuckle`
- `Docker Compose`

## Project Status

This project is currently in the `foundation + vertical slice` stage.

What that means in practice:

- The backend structure and boundaries are already established
- Authentication is implemented end-to-end
- The main business workflow is already modeled in the solution and database
- Session management and message workflow are implemented
- Document upload has a working backend path through cloud storage metadata completion and RabbitMQ event publishing
- Implemented use cases emit structured trace logs for application flow tracking
- The remaining work is primarily filling in the AI consuming/processing and review-result flows

For a technical reviewer, the current repository demonstrates backend architecture decisions, authentication design, persistence setup, and early system decomposition more than complete product delivery.

## Planned / Next

Short-term next steps:

- Add RabbitMQ consuming for AI review completion events
- Add a transactional outbox if RabbitMQ publishing needs guaranteed delivery
- Connect `LegalAiHttpClient` to the AI service endpoints
- Implement review-result generation and retrieval
- Extend logging beyond file-based tracing if centralized observability is needed

Possible later improvements already noted in the architecture document:

- Vector database support
- Citation support in AI responses
- OCR for document ingestion
- Audit logging
- Multi-document comparison

## Why This Project Matters

Even in its current state, this project shows several backend engineering concerns that are relevant for junior `.NET` roles:

- Structuring a multi-project solution with clear layer boundaries
- Designing a relational data model for a non-trivial workflow
- Implementing JWT and refresh-token based authentication
- Using EF Core migrations and repository abstractions with PostgreSQL
- Publishing domain events through a RabbitMQ port/adapter boundary
- Keeping application trace logging behind a Core abstraction with an Infrastructure adapter
- Preparing external service integration through ports/adapters instead of hard-coupling business logic to HTTP details

## Repository Notes

Two details are worth calling out honestly:

- `RiskTrace.Api` currently references `RiskTrace.Infrastructure` directly in the composition root, and there is already a comment in `Program.cs` noting this should be cleaned up later
- Several controller and use case files exist as placeholders, so the repository should be read as an actively developing backend rather than a finished product
- RabbitMQ consuming and AI-side document processing are not complete yet; the current backend work only publishes document-upload events
- Review result workflow is still not implemented, even though auth, sessions, documents, and messages now have concrete use case coverage and trace logging

That is still acceptable for a portfolio project, as long as the README is explicit about what is complete and what is not.
