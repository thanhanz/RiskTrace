# RiskTrace

RiskTrace is an in-progress backend for a legal-tech style document review workflow. The project is being built as an ASP.NET Core Web API with a layered architecture, PostgreSQL persistence, JWT-based authentication, and an integration boundary for an external AI service that will analyze uploaded legal documents and support session-based conversations.

The current codebase is stronger in architecture and backend foundations than in end-user feature completeness. Authentication, persistence, database migrations, dependency wiring, and deployment scaffolding are implemented. Session, document, message, and review-result flows are already modeled in the domain and solution structure, but most of those use cases are still placeholders.

## What Exists Today

### Implemented

- Layered solution split into `Api`, `UseCases`, `Domain`, `Infrastructure`, and `Core`
- ASP.NET Core API entry point with Swagger enabled in development
- PostgreSQL integration with Entity Framework Core and automatic migration on startup
- Database schema for users, review sessions, documents, messages, review results, and refresh tokens
- Authentication use cases for register, login, refresh token rotation, and logout
- JWT access token generation and hashed refresh token storage
- Password hashing with `BCrypt.Net`
- Docker Compose setup for the API and PostgreSQL database

### Partially Implemented / Scaffolded

- Domain entities and enums for review sessions, documents, messages, and review results
- Repository abstractions and EF Core repository implementations
- API controller files and use case files for sessions, documents, messages, and review results
- Local file storage boundary
- AI client boundary for future document analysis and chat integration

### Not Yet Implemented

- Full session management endpoints and business logic
- Document upload and ingestion flow
- Message/chat workflow
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
- `RiskTrace.Domain`: domain entities, enums, request/response models
- `RiskTrace.Infrastructure`: EF Core, repositories, auth services, storage, AI integration adapters
- `RiskTrace.Core`: shared abstractions and common result/pagination primitives

### Dependency Direction

```text
Api -> UseCases
Infrastructure -> UseCases
UseCases -> Domain, Core
Infrastructure -> Domain, Core
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

Authentication is the most complete vertical slice in the project right now.

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

## Tech Stack

- `.NET 10`
- `ASP.NET Core Web API`
- `Entity Framework Core`
- `PostgreSQL`
- `BCrypt.Net`
- `JWT`
- `Swagger / Swashbuckle`
- `Docker Compose`

## Project Status

This project is currently in the `foundation + vertical slice` stage.

What that means in practice:

- The backend structure and boundaries are already established
- One important feature area, authentication, is implemented end-to-end
- The main business workflow is already modeled in the solution and database
- The remaining work is primarily filling in the session, document, AI, and review-result flows

For a technical reviewer, the current repository demonstrates backend architecture decisions, authentication design, persistence setup, and early system decomposition more than complete product delivery.

## Planned / Next

Short-term next steps:

- Implement session CRUD and session detail/query endpoints
- Implement document upload and local storage flow
- Connect `LegalAiHttpClient` to the AI service endpoints
- Implement review-result generation and retrieval
- Implement message/chat flow inside a review session

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
- Preparing external service integration through ports/adapters instead of hard-coupling business logic to HTTP details

## Repository Notes

Two details are worth calling out honestly:

- `RiskTrace.Api` currently references `RiskTrace.Infrastructure` directly in the composition root, and there is already a comment in `Program.cs` noting this should be cleaned up later
- Several controller and use case files exist as placeholders, so the repository should be read as an actively developing backend rather than a finished product

That is still acceptable for a portfolio project, as long as the README is explicit about what is complete and what is not.
