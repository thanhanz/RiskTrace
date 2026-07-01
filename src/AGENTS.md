# AGENTS.md — RiskTrace

## What This Project Is

ASP.NET Core Web API for a legal-tech document review workflow. Users create review sessions, upload legal documents, receive AI-generated review results, and chat within a session.

Stack: .NET 10, EF Core, PostgreSQL, JWT auth.

---

## Solution Layout

```
src/
├── RiskTrace.Api              # Controllers, Program.cs (composition root)
├── RiskTrace.Core             # Result<T>, PagedResult, shared abstractions
├── RiskTrace.Domain           # Entities, enums, request/response DTOs
├── RiskTrace.UseCases         # Use case interfaces + implementations
└── RiskTrace.Infrastructure   # EF Core, repositories, JWT, file storage, AI client
```

### Dependency Direction — Never Violate This

```
Api → UseCases
Infrastructure → UseCases
UseCases → Domain, Core
Infrastructure → Domain, Core
Domain → Core
```

- `Domain` and `UseCases` must never reference `Infrastructure`.
- `Api` only touches `Infrastructure` inside `Program.cs`.

---

## Domain Model

```
User
└── ReviewSession
    ├── Document
    ├── Message
    └── ReviewResult
```

Entities → `RiskTrace.Domain/Entities/`  
Enums → `RiskTrace.Domain/Enums/`

---

## How to Implement a Feature

Always follow this order:

1. **Domain** — entity, enums, request/response DTOs
2. **Use case interface** — `I{Action}{Entity}UseCase` in `UseCases`
3. **Repository interface** — add query methods to the repo interface in `UseCases`
4. **Use case implementation** — in `UseCases`, depends only on repo interfaces + domain types
5. **Repository implementation** — in `Infrastructure`, implements the interface
6. **EF configuration** — update fluent config if schema changes; generate migration if needed
7. **Controller** — thin, no logic; inject use case interface, map `Result<T>` to HTTP
8. **DI registration** — register use case + repo in `Program.cs` or extension methods

---

## Code Patterns to Follow

> **Division of labor:** Agent writes code. Human builds, tests, and migrates.

### Result pattern — always use this, never throw for domain errors

```csharp
return Result<ReviewSessionResponse>.Success(response);
return Result<ReviewSessionResponse>.Failure("Session not found.");
```

Controllers map `Result<T>` → HTTP:
- Success → `200 OK` or `201 Created`
- Failure → `400` or `404` depending on context

### Use case interface

```csharp
public interface ICreateReviewSessionUseCase
{
    Task<Result<ReviewSessionResponse>> ExecuteAsync(CreateReviewSessionRequest request, CancellationToken ct = default);
}
```

### EF configuration (one file per entity)

```csharp
public class ReviewSessionConfiguration : IEntityTypeConfiguration<ReviewSession>
{
    public void Configure(EntityTypeBuilder<ReviewSession> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Title).IsRequired().HasMaxLength(255);
        builder.HasOne(s => s.User)
               .WithMany(u => u.ReviewSessions)
               .HasForeignKey(s => s.UserId);
    }
}
```

### Controller (thin — no logic)

```csharp
[Authorize]
[HttpPost]
public async Task<IActionResult> Create(CreateReviewSessionRequest request)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var result = await _createSessionUseCase.ExecuteAsync(request with { UserId = userId });
    return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
}
```

---

## Naming Conventions

| Artifact | Pattern | Example |
|---|---|---|
| Use case interface | `I{Action}{Entity}UseCase` | `ICreateReviewSessionUseCase` |
| Use case impl | `{Action}{Entity}UseCase` | `CreateReviewSessionUseCase` |
| Repository interface | `I{Entity}Repository` | `IReviewSessionRepository` |
| Repository impl | `{Entity}Repository` | `ReviewSessionRepository` |
| Request DTO | `{Action}{Entity}Request` | `CreateReviewSessionRequest` |
| Response DTO | `{Entity}Response` | `ReviewSessionResponse` |
| EF config | `{Entity}Configuration` | `ReviewSessionConfiguration` |

---

## What Still Needs to Be Implemented

These files exist as stubs — implement them in place, do not rename or move:

- `UseCases/Sessions/` — session CRUD
- `UseCases/Documents/` — upload and ingestion
- `UseCases/Messages/` — chat within a session
- `UseCases/ReviewResults/` — result generation and retrieval
- `Infrastructure/AI/LegalAiHttpClient.cs` — HTTP call to AI service
- `Infrastructure/Storage/LocalFileStorage.cs` — file write/read adapter
- Controllers for all of the above in `Api/Controllers/`

---

## Build & Test — Human Responsibility

The agent must NOT run, fix, or interact with any of the following:

- `dotnet build`
- `dotnet test`
- `dotnet run`
- `dotnet ef` (migrations)
- Any shell command that compiles or executes the project

### What this means in practice

- Do not attempt to verify your output compiles — write correct code by following the patterns in this file.
- Do not generate or run migrations — describe the schema change and let the human apply it.
- Do not fix build errors speculatively — if something is ambiguous, ask before writing code.
- Do not add NuGet packages without listing them explicitly and waiting for human confirmation.

### What the agent IS responsible for

- Writing code that follows the patterns, naming conventions, and dependency rules in this file.
- Flagging anything that *would* require a build or test run to verify, so the human can handle it.


---

## Hard Rules

- No business logic in controllers.
- No `AppDbContext` references outside `Infrastructure`.
- No exceptions for expected domain failures — use `Result<T>`.
- No project reference that violates the dependency direction above.
- No hardcoded secrets — read from `IConfiguration`.
- No fabricated legal citations, risk rules, or Vietnamese-law logic — that belongs to ai-service/, and even there it must come from the user, not be invented.
