# RiskTrace

RiskTrace is a backend-first legal-tech project focused on reviewing real estate legal documents under Vietnamese law. The target users are people who need an early risk screening workflow for contracts and documents related to property transactions in Vietnam, such as sale and purchase agreements, deposit agreements, lease contracts, and supporting legal paperwork.

The system is designed to let users create review sessions, upload documents, receive AI-assisted review results, and continue the review through session-based chat. The long-term goal is not to replace a lawyer, but to help surface legal risks, missing information, suspicious clauses, and points that should be checked against Vietnamese real estate law before a user moves forward.

## Project Focus

RiskTrace is built around a narrow legal domain:

- Real estate laws and legal practice in Vietnam
- Contract review and risk identification
- Document-based legal analysis
- Traceable review sessions for uploaded documents
- AI-assisted explanations with future support for legal citations

The current repository is mainly a backend and AI-service foundation. It demonstrates the architecture, data model, authentication flow, document workflow, messaging boundary, and the planned integration point for legal AI review.

## Main Workflow

The intended user flow is:

1. A user registers or signs in.
2. The user creates a review session for a real estate legal matter.
3. The user uploads one or more legal documents.
4. The backend stores document metadata and publishes an analysis request.
5. The AI service reviews the document content against the Vietnam real estate legal scope.
6. The user receives review results and can continue asking questions inside the session.

Some parts of this workflow are still in progress, especially the final AI-backed review result flow.

## Architecture

```text
RiskTrace
|-- src/
|   |-- RiskTrace.Api
|   |-- RiskTrace.Core
|   |-- RiskTrace.Domain
|   |-- RiskTrace.UseCases
|   `-- RiskTrace.Infrastructure
|-- ai-service/
|-- docs/
|-- docker-compose.yaml
`-- RiskTrace.sln
```

Layer responsibilities:

- `RiskTrace.Api`: ASP.NET Core Web API, controllers, authentication endpoints, and application startup.
- `RiskTrace.UseCases`: application workflows and ports for repositories, storage, messaging, and AI integration.
- `RiskTrace.Domain`: entities, enums, request/response models, and domain contracts.
- `RiskTrace.Infrastructure`: EF Core persistence, PostgreSQL repositories, JWT services, Redis integration, RabbitMQ integration, logging, and external adapters.
- `RiskTrace.Core`: shared primitives such as `Result<T>`, pagination, and cross-cutting abstractions.
- `ai-service`: separate AI service folder. Its own README will describe this part later.

Dependency direction:

```text
Api -> UseCases
Infrastructure -> UseCases
UseCases -> Domain, Core
Infrastructure -> Domain, Core
Domain -> Core
```

The backend follows a clean architecture style so business workflows do not depend directly on infrastructure concerns such as EF Core, RabbitMQ, object storage, or HTTP clients.

## Current Capabilities

Implemented or mostly implemented:

- User registration, login, refresh token rotation, logout, and current-user lookup
- JWT authentication with refresh tokens
- PostgreSQL persistence through Entity Framework Core
- Redis-backed token blacklist support
- Review session create, list, detail, rename, and delete workflows
- Document upload initiation and completion workflow
- Session message send and list workflow
- RabbitMQ messaging boundary for document analysis events
- Hosted consumer structure for AI response messages
- NLog-backed application tracing through a project-level logger abstraction
- Docker Compose services for API, PostgreSQL, Redis, RabbitMQ, and AI service

Still in progress:

- Complete AI review pipeline for Vietnamese real estate legal analysis
- Review result generation and retrieval APIs
- Production-ready legal knowledge base and RAG pipeline
- Legal citation support in AI review results
- OCR support for scanned documents
- Stronger operational hardening around message retries and delivery guarantees

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Redis
- RabbitMQ
- JWT authentication
- NLog
- Docker Compose
- Python AI service scaffold
- Cloudflare R2 / S3-compatible object storage boundary

## Setup

### 1. Fork the repository

Fork this repository to your own GitHub account, then clone your fork:

```bash
git clone https://github.com/thanhanz/RiskTrace.git
cd RiskTrace
```

Add the original repository as `upstream` if you want to keep your fork synchronized:

```bash
git remote add upstream https://github.com/thanhanz/RiskTrace.git
git fetch upstream
```

### 2. Install prerequisites

Required tools:

- .NET SDK 10
- Docker Desktop or a compatible Docker runtime
- Git

Optional tools:

- PostgreSQL client
- RabbitMQ management UI through Docker
- An API client such as Postman, Insomnia, or Swagger UI

### 3. Configure secrets locally

Do not commit API keys, secret keys, access keys, JWT signing keys, or production passwords.

Use local environment variables, user secrets, or a private `.env` file that is not committed. The project expects configuration values such as:

```text
ConnectionStrings__DefaultConnection=<your-postgres-connection-string>
ConnectionStrings__Redis=<your-redis-connection-string>
Jwt__SigningKey=<your-local-jwt-signing-key>
Storage__R2__AccountId=<your-r2-account-id>
Storage__R2__AccessKeyId=<your-r2-access-key-id>
Storage__R2__SecretAccessKey=<your-r2-secret-access-key>
Storage__R2__BucketName=<your-r2-bucket-name>
Storage__R2__PublicBaseUrl=<your-r2-public-base-url>
RabbitMq__Host=<your-rabbitmq-host>
RabbitMq__Username=<your-rabbitmq-username>
RabbitMq__Password=<your-rabbitmq-password>
```

Use development-only values on your machine. Never publish real service credentials in documentation, commits, screenshots, issues, or pull requests.

### 4. Start local services

The repository includes Docker Compose configuration for the API, PostgreSQL, Redis, RabbitMQ, and the AI service scaffold:

```bash
docker compose up -d
```

When running locally, the API (Swagger) is expected to be available on:

```text
http://localhost:8080
```

RabbitMQ management UI is expected to be available on:

```text
http://localhost:15672
```

### 5. Open the API

In development mode, use Swagger UI or your preferred API client to explore the available endpoints.

Common API areas:

- Authentication
- User profile
- Review sessions
- Documents
- Messages
- Review results

## Legal Scope Notice

RiskTrace is a software project for AI-assisted legal document review. It should be treated as a support tool for identifying possible risks in Vietnam real estate documents, not as formal legal advice.

Real estate law is sensitive and changes over time. Any output from the system should be reviewed by a qualified legal professional before being used for an actual transaction or dispute.

## Repository Notes

- The backend is the most developed part of the repository.
- The AI service folder is present and will be documented separately in `ai-service/README.md`.
- The project is still under active development, so some endpoints and workflows are scaffolded or incomplete.
- Secret values must stay local. Replace any local sample values with your own private development configuration before running the project.
