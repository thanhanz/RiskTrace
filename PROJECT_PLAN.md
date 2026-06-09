# Risk Trace — Project Plan

## Overview
- Goal: Build a backend-first legal risk tracing system for Vietnam real estate contracts. Users will upload contracts, and the system will compare contract content against a legal knowledge base using RAG and an LLM-based AI service to identify risks and recommend actions.
- Current phase: Portfolio backend architecture development.
- Current business scope: Real estate contract risk detection under current Vietnamese law.
- Current technical focus: ASP.NET Core backend architecture, authentication, persistence, and core workflow scaffolding.
- Next phase: AI service implementation and RAG-based legal risk analysis.
- Last updated: 2026-06-09

## Milestones
| Milestone | Target Date | Status |
| --- | --- | --- |
| Backend foundation and clean architecture setup | 2026-06-02 | ✅ Done |
| Authentication vertical slice | 2026-06-09 | ✅ Done |
| Session and document workflow implementation | 2026-06-16 | 🔄 In Progress |
| Review result and message workflow implementation | 2026-06-23 | 📌 Planned |
| Backend MVP stabilization and portfolio documentation | 2026-06-30 | 📌 Planned |
| AI service and RAG design phase | 2026-07-07 | 📌 Planned |

## Sprint Plan
| Sprint | Dates | Focus | Expected Outcome |
| --- | --- | --- | --- |
| Sprint 1 | 2026-06-09 to 2026-06-15 | Session and document workflow | Users can create review sessions and attach uploaded documents to sessions. |
| Sprint 2 | 2026-06-16 to 2026-06-22 | Review result and message workflow | Backend supports session messages and stores review results from future AI analysis. |
| Sprint 3 | 2026-06-23 to 2026-06-29 | Backend MVP cleanup | Controllers, use cases, repositories, API contracts, and README are aligned for portfolio review. |
| Sprint 4 | 2026-06-30 to 2026-07-06 | AI service planning | Define AI service boundary, RAG flow, legal knowledge base structure, and integration contract. |

## ✅ Feature Progress
| Feature | Status | Notes |
| --- | --- | --- |
| Clean Architecture backend foundation | ✅ Done | Solution is split into Api, UseCases, Domain, Infrastructure, and Core layers. |
| PostgreSQL persistence and EF Core migrations | ✅ Done | Database schema exists for users, sessions, documents, messages, review results, and refresh tokens. |
| Authentication | ✅ Done | Register, login, refresh token rotation, logout, password hashing, JWT, and cookie support are implemented. |
| User profile endpoint | ✅ Done | `/api/v1/me` exists for authenticated user information. |
| Redis token blacklist | ✅ Done | Redis is wired for token blacklist support during logout. |
| Docker Compose deployment scaffold | ✅ Done | API, PostgreSQL, and Redis services are defined. |
| Review session management | 🔄 In Progress | Domain, repository, interface, and use case files exist, but controller and use case implementation need completion. |
| Document upload and local storage | 🔄 In Progress | Storage boundary exists, but upload workflow and controller implementation need completion. |
| Document analysis workflow | 📌 Planned | Intended to connect uploaded contracts with future AI/RAG analysis. |
| Message/chat workflow | 📌 Planned | Domain and repository structure exist, but API and business logic are not implemented yet. |
| Review result generation and retrieval | 📌 Planned | Domain and repository structure exist, but AI-backed result generation is not implemented yet. |
| External legal AI service integration | 📌 Planned | `LegalAiHttpClient` boundary exists; real integration is planned for the next phase. |
| AI service implementation | 📌 Planned | `ai-service` exists as a placeholder. |
| Feature plan backlog | 📌 Planned | New technologies and future improvements will be added here as the project evolves. |

## Feature Plan Backlog
| Feature / Technology | Purpose | Status | Notes |
| --- | --- | --- | --- |
| RAG pipeline | Compare contract content against a Vietnam legal knowledge base | 📌 Planned | Next phase after backend MVP. |
| Legal knowledge base | Store law references and real estate contract rules | 📌 Planned | Needs source strategy and update process. |
| Contract risk classification | Label detected risks by severity | 📌 Planned | Can use existing `RiskLevel` enum. |
| Citation support | Show which law or rule supports each risk finding | 📌 Planned | Important for trust and legal explainability. |
| OCR support | Handle scanned contracts | 📌 Planned | Later improvement after text-based upload works. |
| Vector database | Store and retrieve legal knowledge chunks | 📌 Planned | Technology not selected yet. |
| Audit logging | Track important user and document actions | 📌 Planned | Useful for legal-tech traceability. |

## ⚠️ Risks & Blockers
| Risk | Impact | Mitigation |
| --- | --- | --- |
| AI service is not implemented yet | Contract risk detection cannot work end-to-end | Complete backend MVP first, then define AI service contract and RAG workflow. |
| Legal knowledge base scope may become too broad | Real estate risk detection may become hard to validate | Keep MVP focused on real estate contracts in Vietnam. |
| Legal accuracy requirements are high | Wrong recommendations could reduce user trust | Add citations, clear disclaimers, and reviewable risk explanations. |
| Several backend workflow files are placeholders | Portfolio may look incomplete if controllers and use cases remain empty | Prioritize sessions, documents, messages, and review results during the next sprints. |
| Solo development capacity | Scope can expand faster than implementation | Track only weekly sprint goals and move new ideas into the Feature Plan Backlog. |
| No frontend scope yet | User-facing workflow cannot be demonstrated visually | Use Swagger/API documentation for backend portfolio demo until frontend is planned. |

## Next Actions
- [ ] Implement review session create/list/detail/rename/delete workflow. (Due: 2026-06-15)
- [ ] Implement document upload and attach documents to review sessions. (Due: 2026-06-15)
- [ ] Wire session and document use cases into dependency injection and controllers. (Due: 2026-06-15)
- [ ] Implement message list/send workflow for review sessions. (Due: 2026-06-22)
- [ ] Implement review result create/retrieve workflow using a placeholder AI response first. (Due: 2026-06-22)
- [ ] Update README to match the completed backend MVP status. (Due: 2026-06-29)
- [ ] Define AI service API contract and RAG architecture for the next phase. (Due: 2026-07-06)

## Change Log
| Date | Change | Author |
| --- | --- | --- |
| 2026-06-09 | Created initial solo project plan based on repository scan and owner input. | Codex |
