# Risk Trace - Project Plan

## Overview
- Goal: Build a production-ready legal contract risk analysis system for Vietnamese real estate contracts. Users will upload contracts, and the system will compare contract content against a Vietnamese real estate legal knowledge base using RAG and an LLM-based AI service to identify risks, cite legal sources, and recommend actions.
- Release definition: The project is released when the backend, AI service, and frontend are deployed and integrated end-to-end, and a user can upload a contract through the UI and receive a risk report with citations.
- Current phase: Backend MVP stabilization and AI service planning.
- Current business scope: Real estate contract risk detection under current Vietnamese law.
- Current technical focus: ASP.NET Core backend completion, backend-to-AI integration contract, SSE result streaming design, RabbitMQ event flow, and AI service RAG architecture.
- Next phase: AI service implementation with Qdrant-backed legal knowledge retrieval and structured risk finding output.
- Last updated: 2026-06-25

## Milestones
| Milestone | Target Date | Status |
| --- | --- | --- |
| Backend foundation and clean architecture setup | 2026-06-02 | Done |
| Authentication vertical slice | 2026-06-09 | Done |
| Session and document workflow implementation | 2026-06-16 | Done |
| Review result and message workflow implementation | 2026-06-23 | In Progress |
| Backend MVP stabilization and portfolio documentation | 2026-06-30 | In Progress |
| AI service and RAG design phase | 2026-07-07 | In Progress |
| Backend-to-AI HTTP/SSE integration | 2026-07-14 | Planned |
| AI service RAG MVP | 2026-07-28 | Planned |
| Frontend SSE-first MVP | 2026-08-11 | Planned |
| End-to-end integration and acceptance testing | 2026-08-25 | Planned |
| Hardening, CI/CD, and deployment readiness | 2026-09-08 | Planned |
| Production release | 2026-09-15 | Planned |

## Sprint Plan
| Sprint | Dates | Status | Focus | Expected Outcome |
| --- | --- | --- | --- | --- |
| Sprint 1 | 2026-06-09 to 2026-06-15 | Done | Session and document workflow | Users can create and manage review sessions; the session/message foundation was established and remaining document work moved into the next sprint. |
| Sprint 2 | 2026-06-16 to 2026-06-22 | Done | Document workflow, message workflow, and trace logging | Document upload, message send/list, and structured use case logging were completed; review result workflow remained open. |
| Sprint 3 | 2026-06-23 to 2026-06-29 | In Progress | Backend MVP cleanup and integration contract | Align controllers, use cases, repositories, README, API contracts, and the backend-to-AI handoff before AI implementation starts. |
| Sprint 4 | 2026-06-30 to 2026-07-06 | In Progress | AI service planning and RAG architecture | Define AI service HTTP/SSE contract, RabbitMQ event flow, Qdrant schema, legal knowledge base structure, and structured risk finding format. |
| Sprint 5 | 2026-07-07 to 2026-07-13 | Planned | Backend-to-AI HTTP/SSE integration | Backend can call the AI service, expose SSE progress/results to clients, and persist review result state. |
| Sprint 6 | 2026-07-14 to 2026-07-27 | Planned | AI service implementation | Python service ingests Vietnamese real estate law into Qdrant and returns structured risk findings with severity, citations, and recommendations. |
| Sprint 7 | 2026-07-28 to 2026-08-10 | Planned | Frontend implementation, SSE-first | Build the live analysis view first, including SSE connect, reconnect, close, progress updates, and streamed risk findings. |
| Sprint 8 | 2026-08-11 to 2026-08-24 | Planned | Integration and end-to-end testing | A user can register, create a session, upload a contract, watch live analysis, and view a cited risk report through the UI. |
| Sprint 9 | 2026-08-25 to 2026-09-07 | Planned | Hardening, CI/CD, and deployment readiness | Add deployment configuration, CI/CD pipeline checks, environment secret handling, logging review, and release runbook. |
| Sprint 10 | 2026-09-08 to 2026-09-15 | Planned | Production release | Backend, AI service, and frontend are deployed and validated against the release definition. |

## Feature Progress
| Feature | Status | Notes |
| --- | --- | --- |
| Clean Architecture backend foundation | Done | Solution is split into Api, UseCases, Domain, Infrastructure, and Core layers. |
| PostgreSQL persistence and EF Core migrations | Done | Database schema exists for users, sessions, documents, messages, review results, and refresh tokens. |
| Authentication | Done | Register, login, refresh token rotation, logout, password hashing, JWT, and cookie support are implemented. |
| User profile endpoint | Done | `/api/v1/me` exists for authenticated user information. |
| Redis token blacklist | Done | Redis is wired for token blacklist support during logout. |
| Docker Compose deployment scaffold | Done | API, PostgreSQL, Redis, RabbitMQ, and AI service scaffold are defined for local orchestration. |
| Review session management | Done | Create, list, detail, rename, and soft-delete session APIs are implemented with authenticated ownership checks. |
| Document upload and local storage | Done | Upload workflow and controller implementation are completed, with storage handling in place. |
| Message/chat workflow | Done | Create message API and get-by-session cursor pagination API are implemented using UUIDv7 ordering by `Id`. |
| Use case trace logging | Done | Authentication, session, document, and message use cases emit structured logs through the Core logger abstraction with an NLog adapter in Infrastructure. |
| Document analysis workflow | In Progress | Backend and AI service need a completed contract for analysis requests, progress events, and final results. |
| External legal AI service integration | In Progress | Backend-to-AI service connection is the next active step, including HTTP/SSE boundaries. |
| Review result generation and retrieval | Planned | Domain and repository structure exist, but AI-backed result generation is not implemented yet. |
| AI service implementation | Planned | `ai-service` exists and needs RAG ingestion, Qdrant integration, RabbitMQ handling, and LLM analysis workflow. |
| Feature plan backlog | Planned | New technologies and future improvements will be added here as the project evolves. |
| SSE analysis streaming | Planned | Backend must stream analysis progress and results to the frontend through SSE. |
| Qdrant vector database | Planned | AI service will use Qdrant to retrieve Vietnamese real estate law chunks for RAG. |
| Vietnamese real estate legal knowledge ingestion | Planned | AI service must ingest curated laws and rules before reliable risk analysis is possible. |
| Structured risk finding schema | Planned | AI output must include severity, citation, recommendation, and source context. |
| Frontend application | Planned | UI scope includes login/register, session list, document upload, live analysis view, and risk findings report. |
| CI/CD and deployment pipeline | Planned | Production release requires automated checks, deployment configuration, and secret-safe environment setup. |

## Feature Plan Backlog
| Feature / Technology | Purpose | Status | Notes |
| --- | --- | --- | --- |
| RAG pipeline | Compare contract content against a Vietnam legal knowledge base | Planned | Next phase after backend MVP. |
| Legal knowledge base | Store law references and real estate contract rules | Planned | Needs source strategy and update process. |
| Contract risk classification | Label detected risks by severity | Planned | Can use existing `RiskLevel` enum. |
| Citation support | Show which law or rule supports each risk finding | Planned | Important for trust and legal explainability. |
| OCR support | Handle scanned contracts | Planned | Later improvement after text-based upload works. |
| Vector database | Store and retrieve legal knowledge chunks | Planned | Qdrant is the intended vector database for the AI service. |
| Audit logging | Track important user and document actions | Planned | Useful for legal-tech traceability. |
| Log centralization and observability | Extend file-based tracing into searchable operational monitoring | Planned | Current logging is local NLog file tracing only. |
| SSE lifecycle handling | Keep analysis progress reliable in the UI | Planned | Frontend must handle connect, reconnect, close, and failure states. |
| Deployment runbook | Make release repeatable | Planned | Needed before production release. |

## Risks & Blockers
| Risk | Impact | Mitigation |
| --- | --- | --- |
| AI service is not implemented yet | Contract risk detection cannot work end-to-end | Complete the service contract first, then implement the Qdrant-backed RAG workflow. |
| Legal knowledge base scope may become too broad | Real estate risk detection may become hard to validate | Keep MVP focused on Vietnamese real estate contracts and curated source material. |
| Legal accuracy requirements are high | Wrong recommendations could reduce user trust | Add citations, clear disclaimers, and reviewable risk explanations. |
| Review result workflow is still placeholder-only | AI-backed legal analysis cannot be demonstrated yet | Implement result state persistence and API/SSE delivery around AI service output. |
| SSE lifecycle is not designed yet | Live analysis can appear unreliable to users | Define backend event format and frontend connection behavior before building the UI. |
| Current tracing is file-based only | Operational visibility is limited outside the host machine | Add centralized sinks or log shipping after backend MVP stabilizes. |
| No CI/CD pipeline yet | Production release is manual and harder to trust | Add automated checks, deployment configuration, and secret-safe environment management. |
| Solo development capacity | Scope can expand faster than implementation | Track weekly sprint goals and move non-release ideas into the Feature Plan Backlog. |

## Next Actions
- [x] Implement review session create/list/detail/rename/delete workflow. (Completed: 2026-06-12)
- [x] Implement document upload and attach documents to review sessions. (Completed: 2026-06-20)
- [x] Add structured trace logging for implemented use cases. (Completed: 2026-06-21)
- [ ] Define backend-to-AI HTTP/SSE contract and review result event schema. (Due: 2026-06-27)
- [ ] Implement backend review result persistence and SSE streaming boundary. (Due: 2026-07-13)
- [ ] Implement AI service Qdrant ingestion and RAG analysis workflow. (Due: 2026-07-27)
- [ ] Build frontend live analysis view with SSE connect, reconnect, and close handling. (Due: 2026-08-10)
- [ ] Add CI/CD pipeline and deployment secret handling for backend, AI service, and frontend. (Due: 2026-09-07)

## Change Log
| Date | Change |
| --- | --- |
| 2026-06-09 | Created initial solo project plan based on repository scan and owner input. |
| 2026-06-14 | Updated progress to reflect completed session CRUD, implemented message send/list APIs, and remaining document/review result work. |
| 2026-06-20 | Updated feature progress to mark document upload, local storage, and message workflow as complete; next step is backend-to-AI service integration. |
| 2026-06-20 | Updated next actions to mark backend MVP items complete and reorder the remaining AI integration steps. |
| 2026-06-21 | Updated the plan to reflect completed session/document/message workflow coverage and added use case trace logging with the Core logger abstraction and NLog adapter. |
| 2026-06-25 | Expanded roadmap through production release with backend HTTP/SSE integration, AI service RAG/Qdrant work, frontend SSE-first implementation, integration testing, and CI/CD hardening. |
