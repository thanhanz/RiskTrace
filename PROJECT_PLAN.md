# Risk Trace - Project Plan

## Overview
- Goal: Build a backend-first legal risk tracing system for Vietnam real estate contracts. Users will upload contracts, and the system will compare contract content against a legal knowledge base using RAG and an LLM-based AI service to identify risks and recommend actions.
- Current phase: Portfolio backend architecture development.
- Current business scope: Real estate contract risk detection under current Vietnamese law.
- Current technical focus: ASP.NET Core backend architecture, authentication, persistence, session/message workflow, document workflow completion, and trace logging coverage.
- Next phase: AI service implementation and RAG-based legal risk analysis.
- Last updated: 2026-06-21

## Milestones
| Milestone | Target Date | Status |
| --- | --- | --- |
| Backend foundation and clean architecture setup | 2026-06-02 | Done |
| Authentication vertical slice | 2026-06-09 | Done |
| Session and document workflow implementation | 2026-06-16 | Done |
| Review result and message workflow implementation | 2026-06-23 | In Progress |
| Backend MVP stabilization and portfolio documentation | 2026-06-30 | Planned |
| AI service and RAG design phase | 2026-07-07 | Planned |

## Sprint Plan
| Sprint | Dates | Focus | Expected Outcome |
| --- | --- | --- | --- |
| Sprint 1 | 2026-06-09 to 2026-06-15 | Session and document workflow | Users can create and manage review sessions; document upload and session attachment remain to be completed. |
| Sprint 2 | 2026-06-16 to 2026-06-22 | Review result, message workflow, and trace logging | Backend supports sending and listing session messages, and implemented use cases now emit structured trace logs; review result workflow remains to be implemented. |
| Sprint 3 | 2026-06-23 to 2026-06-29 | Backend MVP cleanup | Controllers, use cases, repositories, logging, API contracts, and README are aligned for portfolio review. |
| Sprint 4 | 2026-06-30 to 2026-07-06 | AI service planning | Define AI service boundary, RAG flow, legal knowledge base structure, and integration contract. |

## Feature Progress
| Feature | Status | Notes |
| --- | --- | --- |
| Clean Architecture backend foundation | Done | Solution is split into Api, UseCases, Domain, Infrastructure, and Core layers. |
| PostgreSQL persistence and EF Core migrations | Done | Database schema exists for users, sessions, documents, messages, review results, and refresh tokens. |
| Authentication | Done | Register, login, refresh token rotation, logout, password hashing, JWT, and cookie support are implemented. |
| User profile endpoint | Done | `/api/v1/me` exists for authenticated user information. |
| Redis token blacklist | Done | Redis is wired for token blacklist support during logout. |
| Docker Compose deployment scaffold | Done | API, PostgreSQL, and Redis services are defined. |
| Review session management | Done | Create, list, detail, rename, and soft-delete session APIs are implemented with authenticated ownership checks. |
| Document upload and local storage | Done | Upload workflow and controller implementation are completed, with local storage handling in place. |
| Message/chat workflow | Done | Create message API and get-by-session cursor pagination API are implemented using UUIDv7 ordering by `Id`. |
| Use case trace logging | Done | Authentication, session, document, and message use cases emit structured logs through the Core logger abstraction with an NLog adapter in Infrastructure. |
| Document analysis workflow | In Progress | Next step is to connect the backend to the AI service before implementing the AI workflow itself. |
| External legal AI service integration | In Progress | Backend-to-AI service connection is the next active step. |
| Review result generation and retrieval | Planned | Domain and repository structure exist, but AI-backed result generation is not implemented yet. |
| AI service implementation | Planned | `ai-service` exists as a placeholder and will be implemented after the service connection is working. |
| Feature plan backlog | Planned | New technologies and future improvements will be added here as the project evolves. |

## Feature Plan Backlog
| Feature / Technology | Purpose | Status | Notes |
| --- | --- | --- | --- |
| RAG pipeline | Compare contract content against a Vietnam legal knowledge base | Planned | Next phase after backend MVP. |
| Legal knowledge base | Store law references and real estate contract rules | Planned | Needs source strategy and update process. |
| Contract risk classification | Label detected risks by severity | Planned | Can use existing `RiskLevel` enum. |
| Citation support | Show which law or rule supports each risk finding | Planned | Important for trust and legal explainability. |
| OCR support | Handle scanned contracts | Planned | Later improvement after text-based upload works. |
| Vector database | Store and retrieve legal knowledge chunks | Planned | Technology not selected yet. |
| Audit logging | Track important user and document actions | Planned | Useful for legal-tech traceability. |
| Log centralization and observability | Extend file-based tracing into searchable operational monitoring | Planned | Current logging is local NLog file tracing only. |

## Risks & Blockers
| Risk | Impact | Mitigation |
| --- | --- | --- |
| AI service is not implemented yet | Contract risk detection cannot work end-to-end | Complete backend MVP first, then define AI service contract and RAG workflow. |
| Legal knowledge base scope may become too broad | Real estate risk detection may become hard to validate | Keep MVP focused on real estate contracts in Vietnam. |
| Legal accuracy requirements are high | Wrong recommendations could reduce user trust | Add citations, clear disclaimers, and reviewable risk explanations. |
| Review result workflow is still placeholder-only | AI-backed legal analysis cannot be demonstrated yet | Implement placeholder review result generation before real AI integration. |
| Current tracing is file-based only | Operational visibility is limited outside the host machine | Add centralized sinks or log shipping after backend MVP stabilizes. |
| Solo development capacity | Scope can expand faster than implementation | Track only weekly sprint goals and move new ideas into the Feature Plan Backlog. |
| No frontend scope yet | User-facing workflow cannot be demonstrated visually | Use Swagger/API documentation for backend portfolio demo until frontend is planned. |

## Next Actions
- [x] Implement review session create/list/detail/rename/delete workflow. (Completed: 2026-06-12)
- [x] Implement document upload and attach documents to review sessions. (Completed: 2026-06-20)
- [x] Complete document repository, use cases, and controller endpoints. (Completed: 2026-06-20)
- [x] Implement message list/send workflow for review sessions. (Completed: 2026-06-14)
- [x] Add structured trace logging for implemented use cases. (Completed: 2026-06-21)
- [ ] Connect backend to AI service. (Due: 2026-06-24)
- [ ] Define AI service API contract and RAG architecture for the next phase. (Due: 2026-06-27)
- [ ] Implement AI service workflow after backend connection succeeds. (Due: 2026-07-01)
- [ ] Implement review result create/retrieve workflow using the AI service output. (Due: 2026-07-08)

## Change Log
| Date | Change |
| --- | --- |
| 2026-06-09 | Created initial solo project plan based on repository scan and owner input. |
| 2026-06-14 | Updated progress to reflect completed session CRUD, implemented message send/list APIs, and remaining document/review result work. |
| 2026-06-20 | Updated feature progress to mark document upload, local storage, and message workflow as complete; next step is backend-to-AI service integration. |
| 2026-06-20 | Updated next actions to mark backend MVP items complete and reorder the remaining AI integration steps. |
| 2026-06-21 | Updated the plan to reflect completed session/document/message workflow coverage and added use case trace logging with the Core logger abstraction and NLog adapter. |
