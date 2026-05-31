# Real Estate LegalTech - Backend Architecture

## Solution Structure

``` text
src/
├── RiskTrace.Api
├── RiskTrace.Core
├── RiskTrace.Domain
├── RiskTrace.UseCases
└── RiskTrace.Infrastructure
```

------------------------------------------------------------------------

# Domain

Business entities and rules only.

``` text
Domain/
├── Entities/
│   ├── User.cs
│   ├── ReviewSession.cs
│   ├── Document.cs
│   ├── Message.cs
│   └── ReviewResult.cs
│
├── Enums/
│   ├── UserRole.cs
│   ├── MessageRole.cs
│   ├── SessionStatus.cs
│   ├── DocumentStatus.cs
│   └── RiskLevel.cs
│
├── ValueObjects/
└── Exceptions/
```

## Entities


### BaseModel (Abstract)


``` text
CreatedAt
UpdatedAt
IsActive
```

### User

``` text
Id
Email
PasswordHash
FullName
Role
```

### Session

``` text
Id
UserId
Title
```

### Document

``` text
Id
SessionId
FileName
FilePath
ContentType
FileSize
```

### Message

``` text
Id
SessionId
Role(User/Assistant)
Content
```

### ReviewResult

``` text
Id
SessionId
OverallRiskLevel
Summary
ResultJson
```

------------------------------------------------------------------------

# UseCases

Application orchestration layer.

``` text
UseCases/
├── Sessions/
│   ├── CreateSessionUseCase.cs
│   ├── GetSessionDetailUseCase.cs
│   ├── GetUserSessionsUseCase.cs
│   ├── RenameSessionUseCase.cs
│   └── DeleteSessionUseCase.cs
│
├── Documents/
│   ├── UploadDocumentUseCase.cs
│   ├── DeleteDocumentUseCase.cs
│   └── AnalyzeDocumentUseCase.cs
│
├── Messages/
│   ├── SendMessageUseCase.cs
│   └── GetSessionMessagesUseCase.cs
│
├── ReviewResults/
│   ├── GenerateReviewResultUseCase.cs
│   └── GetReviewResultUseCase.cs
│
└── Ports/ 
    ├── Repositories/
    ├── AI/
    ├── Storage/
    └── Auth/
```

## Ports

### Repositories

``` text
IUserRepository
IReviewSessionRepository
IDocumentRepository
IMessageRepository
IReviewResultRepository
```

### AI

``` text
ILegalAiClient
```

### Storage

``` text
IFileStorage
```

### Auth

``` text
IPasswordHasher
IJwtTokenService
ICurrentUserProvider
```

------------------------------------------------------------------------

# Infrastructure

Technical implementations.

``` text
Infrastructure/
├── Persistence/
│   ├── AppDbContext.cs
│   ├── Configurations/
│   ├── Repositories/
│   └── Migrations/
│
├── AI/
│   └── LegalAiHttpClient.cs
│
├── Storage/
│   └── LocalFileStorage.cs
│
├── Auth/
│   ├── PasswordHasher.cs
│   ├── JwtTokenService.cs
│   └── CurrentUserProvider.cs
│
└── DependencyInjection.cs
```

------------------------------------------------------------------------

# Core

Shared kernel.

``` text
Core/
├── Common/
│   ├── Result.cs
│   ├── Error.cs
│   ├── PaginatedResult.cs
│   └── Entity.cs
│
├── Abstractions/
│   ├── IUnitOfWork.cs
│   └── IDateTimeProvider.cs
│
├── Constants/
└── Exceptions/
```

------------------------------------------------------------------------

# API

HTTP entry point.

``` text
Api/
├── Controllers/
│   ├── AuthController.cs
│   ├── SessionsController.cs
│   ├── DocumentsController.cs
│   ├── MessagesController.cs
│   └── ReviewResultsController.cs
│
├── Contracts/
│   ├── Auth/
│   ├── Sessions/
│   ├── Documents/
│   ├── Messages/
│   └── ReviewResults/
│
├── Middleware/
└── Program.cs
```

------------------------------------------------------------------------

# Database Model

``` text
User (1)
  |
  └── ReviewSession (N)
          |
          ├── Document (N)
          |
          ├── Message (N)
          |
          └── ReviewResult (N)
```

------------------------------------------------------------------------

# AI Service

``` text
ai-service/
├── api/
├── application/
├── domain/
└── infrastructure/
```

## AI Endpoints

``` text
POST /ai/sessions/{sessionId}/ingest-document
POST /ai/sessions/{sessionId}/analyze-risk
POST /ai/sessions/{sessionId}/ask
```

------------------------------------------------------------------------

# Dependency Rule

``` text
Api -> UseCases

UseCases -> Domain
UseCases -> Core

Infrastructure -> UseCases
Infrastructure -> Domain
Infrastructure -> Core

Domain -> Core
```

Forbidden:

``` text
Domain -> Infrastructure
Domain -> Api
UseCases -> Infrastructure
```

------------------------------------------------------------------------

# MVP Scope

Features:

1.  Register/Login
2.  Create Session
3.  Upload Document
4.  Generate Review Result
5.  Chat in Session
6.  View Review History

Future:

1.  Vector Database
2.  Citations
3.  OCR
4.  Audit Logs
5.  Multi-document comparison
