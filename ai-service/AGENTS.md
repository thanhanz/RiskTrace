# AGENTS.md — RiskTrace AI Service (ai-service/)

> Project-wide context, legal-domain caution, and cross-service contract:
> see `../AGENTS.md`. This file covers ai-service-only structure and rules.

## What This Service Owns

Everything related to actually reading and judging a legal document: extraction/OCR,
chunking, retrieval (RAG over a Vietnamese real estate legal knowledge base), risk
scoring, severity classification, recommendations, and LLM-based review generation.
It consumes a "document uploaded" event and publishes an "analysis completed" event.
It does **not** own auth, sessions, or persistence of review history — that's
`src/` (backend).

## Where Things Live (so you don't have to scan the project)

```
app/
├── core/            # Settings, logging, constants, errors, time. Cross-cutting only.
├── domain/          # Models (document, analysis, finding, session) + event types.
│                     # No business logic here — just shape of data.
├── application/
│   ├── use_cases/   # Orchestration: "analyze a document", "ingest knowledge base".
│   │                 # This is where a business workflow is coordinated end-to-end.
│   └── ports/        # Interfaces for external dependencies (storage, vector store,
│                     # message bus, LLM, backend client). Business logic depends on
│                     # these, never on a concrete adapter directly.
├── services/
│   ├── document/    # Extraction, OCR, chunking logic.
│   ├── retrieval/   # Embedding, retrieval, citation mapping — the RAG pipeline.
│   ├── risk/        # Risk scoring, severity classification, recommendations.
│   │                 # ⚠️ Highest-stakes folder — see "Legal Logic" section below.
│   └── llm/         # Prompt templates and response parsing.
├── infrastructure/  # Concrete adapters: object storage (R2), vector DB (Qdrant),
│                     # RabbitMQ, LLM provider (OpenAI), backend HTTP client.
├── interfaces/
│   ├── api/         # HTTP surface (currently just health check).
│   └── consumers/   # RabbitMQ message consumers — entrypoints, no logic here.
└── schemas/         # Wire/transport schemas for events, findings, reports.
                      # ⚠️ Shared contract with backend — see root AGENTS.md.
prompts/              # LLM prompt template files.
```

Rule of thumb for "where do I implement this":

| You're implementing... | Put it in |
|---|---|
| A new data shape (finding, document, event) | `domain/` |
| A new external dependency's interface | `application/ports/` |
| A new end-to-end workflow (e.g. "re-analyze on request") | `application/use_cases/` |
| Document parsing / OCR / chunking logic | `services/document/` |
| Retrieval / embedding / RAG logic | `services/retrieval/` |
| Risk scoring or severity/recommendation logic | `services/risk/` (read caution below first) |
| Prompt wording or LLM response parsing | `services/llm/` |
| A concrete integration (Qdrant, R2, RabbitMQ, OpenAI, backend HTTP) | `infrastructure/` |
| A new consumer or HTTP route | `interfaces/` (must stay thin) |
| A change to the message/event shape sent to or from the backend | `schemas/` |

## Dependency Direction — Never Violate This

```
interfaces      → application, infrastructure (wiring only, no logic)
infrastructure  → application (implements ports), domain
services        → domain
application     → domain, services, ports
domain          → (nothing external — no frameworks, no I/O)
```

`domain/` stays framework-free. `application/use_cases/` must depend on `ports/`
interfaces, never on a concrete `infrastructure/` adapter directly.

## Business Areas and Their Status

| Area | Status | Notes |
|---|---|---|
| Document uploaded → analysis pipeline | **In progress** | Main use case, not fully wired |
| Knowledge base ingestion (RAG) | **In progress** | Needed before retrieval is meaningful |
| OCR for scanned documents | **Not implemented** | |
| Risk scoring / severity classification | **In progress** | See legal-logic caution below |
| Legal citation mapping | **Not implemented** | Do not fabricate citations in the meantime |
| Result publishing back to backend | Partially implemented | Publisher exists; full payload not finalized |

## Legal / Risk Logic — Read Before Touching `services/risk/`

This is the highest-stakes area of the whole project.

- Never invent what counts as a legal risk, a missing/required clause, a red flag,
  or a severity level for Vietnamese real estate law. If the specific rule isn't
  already defined in code or given explicitly by the user, **ask** — don't guess.
- Every finding surfaced to a user must be phrased as advisory ("this may warrant
  review by a lawyer"), never as a definitive legal conclusion.
- Do not fabricate legal citations (statute names, article numbers) anywhere —
  in prompts, parsers, or fallback/default logic. Leave a citation field empty
  rather than producing a plausible-looking but unverified one.
- Treat any change to prompt wording that affects what the LLM is told to check for
  as a legal-logic change, not a copy edit — flag it for human review.

## Messaging Contract (with Backend)

- Consumes: "document uploaded" event, produced by the backend.
- Publishes: "analysis completed" event, consumed by the backend.
- Exchange/queue/routing key names are defined in root `AGENTS.md` and in
  `app/core/constants.py` / `app/core/settings.py`.
- The payload shape is defined in `schemas/`. This is a shared contract — changing
  it requires checking whether backend (`src/`) needs a matching update.

## Build & Test — Human Responsibility

Do not run the service, its tests, or install packages (`pytest`, `uvicorn`,
`docker compose up`, `pip install`) unless a human explicitly asks. Describe what
would need to run and let a human verify it. If something is ambiguous, ask before
writing code rather than guessing.

## Hard Rules

- No framework or infrastructure imports inside `domain/`.
- No direct `infrastructure/` imports from `application/use_cases/` — go through
  `application/ports/`.
- No business logic inside `interfaces/` (routes or consumers) — they only wire a
  request/event to a use case.
- No hardcoded secrets — read from settings/environment.
- No fabricated legal citations or invented Vietnamese legal rules — see the
  Legal/Risk Logic section above.
- No change to `schemas/` (the backend-facing contract) without checking backend
  impact first.
