# RiskTrace AI Implementation Plan

## Source Code Readout

The current product foundation supports account access, review sessions, document upload, message history, cloud storage, and an event-based handoff from the main backend to the AI service. The AI service structure exists, but knowledge base ingestion, document extraction, legal retrieval, clause risk scoring, citations, recommendations, and final report generation still need to be implemented before the two core product features are usable end to end.

This plan is written for business execution in Jira. It focuses on user and system outcomes, not implementation files.

---

## 1. Epics

### Epic E1: Secure Review Workspace Foundation

**Business goal:** Give users a secure workspace where they can sign in, create real estate review sessions, upload contracts, and track the review lifecycle. This creates the product shell that every AI review experience depends on.

**Done looks like:** A user can authenticate, create a review session, upload a supported contract, see upload and analysis status, and view the session message history without seeing another user's data.

### Epic E2: Vietnamese Real Estate Legal Knowledge Base

**Business goal:** Build a trusted legal knowledge base for Vietnamese real estate laws and regulations so the AI service has a controlled source of truth. This makes future risk findings traceable instead of purely generative.

**Done looks like:** Approved legal sources can be imported, processed, versioned, searched, and linked back to specific legal references used in analysis.

### Epic E3: Contract Intake and Clause Preparation

**Business goal:** Convert user-uploaded contracts into clean, reviewable text and clauses that the AI service can compare against legal knowledge. This reduces analysis noise and enables clause-level risk reporting.

**Done looks like:** The system can retrieve an uploaded contract, extract readable content, split it into meaningful clauses, and start analysis while showing clear status to the user.

### Epic E4: AI Risk Analysis and Legal Reasoning

**Business goal:** Identify risky, missing, unusual, or disadvantageous contract clauses using the legal knowledge base and AI-assisted reasoning. This is the core value proposition of RiskTrace AI.

**Done looks like:** Each reviewed contract produces risk findings with severity, confidence, explanation, recommended action, and supporting legal references where available.

### Epic E5: Review Results, Chat, and User Guidance

**Business goal:** Deliver AI findings in a useful review experience, then let users ask follow-up questions within the same session. This turns a one-time analysis into a guided legal review workflow.

**Done looks like:** Users can view the final report, inspect individual risks, ask follow-up questions grounded in the uploaded contract and legal references, and export or share a review summary.

### Epic E6: Operational Readiness, Quality, and Trust

**Business goal:** Make the system reliable, secure, observable, and safe enough for repeated legal-tech demos and later production hardening. This protects user documents and improves team confidence in releases.

**Done looks like:** Sensitive configuration is not exposed, failures are visible, core workflows are covered by tests, and the team can run an end-to-end demo with repeatable test data.

---

## 2. Tickets

### E1 Tickets: Secure Review Workspace Foundation

#### Ticket RT-001: Confirm MVP Legal Review Scope

**Type:** Feature  
**Priority:** Critical  
**Description:** Define the first release scope for Vietnamese real estate contract review, including supported contract types, supported languages, and out-of-scope legal scenarios. This prevents the knowledge base and AI review from becoming too broad for a reliable MVP.

**Acceptance criteria:**
- Product stakeholders can see a clear list of supported first-release document types.
- The system has a visible disclaimer that RiskTrace AI is an early screening tool, not formal legal advice.
- Out-of-scope cases are documented so support and testing teams know when a document should not be accepted.

**Dependencies:** None

#### Ticket RT-002: Validate Secure Account and Session Access

**Type:** Testing  
**Priority:** High  
**Description:** Confirm that users can register, sign in, refresh access, sign out, and only access their own sessions. This ticket verifies the existing foundation from a product behavior perspective.

**Acceptance criteria:**
- A signed-in user can create, rename, list, view, and delete only their own sessions.
- A signed-out user cannot access protected review workflows.
- Attempts to access another user's session are rejected.

**Dependencies:** None

#### Ticket RT-003: Complete User Contract Upload Experience

**Type:** Feature  
**Priority:** Critical  
**Description:** Ensure users can upload supported contract files into a review session and receive a clear confirmation that the file was accepted. The user should not need to understand storage details.

**Acceptance criteria:**
- A user can upload a supported contract to an active review session.
- Unsupported file types or invalid uploads show a clear user-facing error.
- A successfully uploaded document appears in the review session with file name, size, and status.

**Dependencies:** RT-002

#### Ticket RT-004: Show Review Session Timeline and Messages

**Type:** Feature  
**Priority:** High  
**Description:** Present the session as a timeline of user messages, uploaded documents, and system responses. This helps users understand what has happened in a legal review without switching contexts.

**Acceptance criteria:**
- A user can send a message inside a session.
- A user can view previous session messages in the correct order.
- Empty sessions show a clear state that invites the next user action.

**Dependencies:** RT-002

#### Ticket RT-005: Add User-Friendly Analysis Status

**Type:** Feature  
**Priority:** High  
**Description:** Show whether a document is waiting, being analyzed, completed, or failed. This makes the asynchronous AI review workflow understandable to users.

**Acceptance criteria:**
- After upload, the document enters a waiting or analysis state.
- When analysis finishes, the user can see that the review is ready.
- If analysis fails, the user sees a clear failure state and suggested next action.

**Dependencies:** RT-003

---

### E2 Tickets: Vietnamese Real Estate Legal Knowledge Base

#### Ticket RT-101: Define Approved Legal Source Catalog

**Type:** Feature  
**Priority:** Critical  
**Description:** Create a controlled catalog of Vietnamese real estate laws, decrees, circulars, and regulations that RiskTrace AI is allowed to use. Each source needs ownership, status, publication date, and effective date.

**Acceptance criteria:**
- Legal admins can identify which sources are approved for AI review.
- Each legal source has enough metadata to support citations and updates.
- Deprecated or superseded sources are clearly marked and not used by default.

**Dependencies:** RT-001

#### Ticket RT-102: Import Legal Documents Into the Knowledge Base

**Type:** Feature  
**Priority:** Critical  
**Description:** Allow approved legal source documents to be imported into the system for later retrieval. The ingestion workflow should support repeatable imports without creating duplicate knowledge.

**Acceptance criteria:**
- The system can ingest an approved legal source and mark it as available for search.
- Re-importing the same source does not create duplicate active legal references.
- Failed imports produce an error that an admin can review.

**Dependencies:** RT-101

#### Ticket RT-103: Structure Legal Content for Retrieval

**Type:** Feature  
**Priority:** High  
**Description:** Break legal documents into searchable legal sections while preserving article, clause, title, and source context. This is required for precise retrieval during contract review.

**Acceptance criteria:**
- Legal text can be searched at a section level rather than only as whole documents.
- Search results preserve their original legal source and section identity.
- Long legal documents remain searchable without losing citation context.

**Dependencies:** RT-102

#### Ticket RT-104: Add Legal Citation Metadata

**Type:** Feature  
**Priority:** High  
**Description:** Attach citation-ready metadata to every searchable legal section so risk findings can reference the law behind the conclusion. Citations are essential for user trust.

**Acceptance criteria:**
- A retrieved legal reference includes law name, section label, effective date, and source status.
- Risk findings can point to one or more supporting legal references.
- Users can distinguish current legal references from older or superseded references.

**Dependencies:** RT-103

#### Ticket RT-105: Validate Legal Retrieval Quality

**Type:** Testing  
**Priority:** High  
**Description:** Test whether common real estate contract questions retrieve useful Vietnamese legal references. This confirms that the knowledge base supports actual contract review scenarios.

**Acceptance criteria:**
- A prepared test set of real estate legal questions returns relevant legal sections.
- Search results include citations that a reviewer can verify.
- The team records retrieval gaps for future source expansion.

**Dependencies:** RT-103, RT-104

#### Ticket RT-106: Add Knowledge Base Refresh Process

**Type:** DevOps  
**Priority:** Medium  
**Description:** Provide a controlled process to refresh the legal knowledge base when laws change or new sources are added. The process should avoid disrupting active reviews.

**Acceptance criteria:**
- Admins can add a new legal source version without deleting historical references.
- Active reviews use a known knowledge base version.
- The team can roll back a problematic knowledge base update.

**Dependencies:** RT-102, RT-104

---

### E3 Tickets: Contract Intake and Clause Preparation

#### Ticket RT-201: Extract Text From Uploaded Contracts

**Type:** Feature  
**Priority:** Critical  
**Description:** Convert uploaded contract files into clean text for AI review. The user should receive a clear error if the contract cannot be read.

**Acceptance criteria:**
- A readable uploaded contract produces extracted text for analysis.
- Empty or unreadable files are rejected with a clear status.
- Extracted text remains linked to the original review session and document.

**Dependencies:** RT-003

#### Ticket RT-202: Support Scanned Contract OCR

**Type:** Feature  
**Priority:** High  
**Description:** Add support for scanned PDFs where text cannot be extracted directly. This is important because many legal documents are scanned copies.

**Acceptance criteria:**
- A scanned contract can be converted into readable text.
- Low-quality scans produce a user-visible warning when confidence is low.
- The final report indicates when OCR was used.

**Dependencies:** RT-201

#### Ticket RT-203: Split Contracts Into Reviewable Clauses

**Type:** Feature  
**Priority:** Critical  
**Description:** Segment extracted contract text into clauses or sections so the system can assess specific risks. Clause-level analysis makes findings easier to understand and act on.

**Acceptance criteria:**
- The system separates a contract into ordered clauses or sections.
- Each clause remains traceable to its position in the original document.
- Very short or malformed sections are handled without stopping the whole review.

**Dependencies:** RT-201

#### Ticket RT-204: Start Analysis Automatically After Upload

**Type:** Integration  
**Priority:** Critical  
**Description:** Connect the completed upload workflow to the AI analysis workflow so users do not need to manually trigger review. This creates the first real end-to-end product flow.

**Acceptance criteria:**
- Completing an upload automatically starts the analysis process.
- The user sees the document status move through the analysis lifecycle.
- Duplicate analysis requests for the same document are avoided.

**Dependencies:** RT-003, RT-201

#### Ticket RT-205: Handle Analysis Failures Gracefully

**Type:** Feature  
**Priority:** High  
**Description:** Provide clear behavior when document reading, legal retrieval, or AI analysis fails. Failures should not leave the session stuck indefinitely.

**Acceptance criteria:**
- Failed analysis updates the document or session with a clear failed status.
- The user can retry analysis when the failure is recoverable.
- Support logs include enough context to investigate the failed review.

**Dependencies:** RT-204

---

### E4 Tickets: AI Risk Analysis and Legal Reasoning

#### Ticket RT-301: Define AI Review Output Contract

**Type:** Feature  
**Priority:** Critical  
**Description:** Define the required structure for AI review output, including summary, clause findings, severity, confidence, recommendations, and legal references. This creates a stable agreement between the AI service and the user-facing backend.

**Acceptance criteria:**
- Every AI review returns a consistent report shape.
- Invalid or incomplete AI responses are rejected and marked for investigation.
- The report structure supports both full report display and individual clause display.

**Dependencies:** RT-203, RT-104

#### Ticket RT-302: Retrieve Relevant Legal References Per Clause

**Type:** Integration  
**Priority:** Critical  
**Description:** For each contract clause, retrieve the most relevant Vietnamese real estate legal references from the knowledge base. This grounds AI findings in approved legal material.

**Acceptance criteria:**
- Each clause analysis can include supporting legal references.
- Irrelevant or weak matches are excluded from the final finding.
- The system records when no useful legal reference is found.

**Dependencies:** RT-103, RT-203

#### Ticket RT-303: Classify Clause Risk Severity

**Type:** Feature  
**Priority:** Critical  
**Description:** Classify each risky clause by severity so users can quickly prioritize what to review. Severity should reflect both legal risk and disadvantage to the user.

**Acceptance criteria:**
- Each risk finding has one severity level: low, medium, high, or critical.
- Critical and high findings are clearly distinguishable from informational findings.
- Low-confidence classifications are marked rather than hidden.

**Dependencies:** RT-301, RT-302

#### Ticket RT-304: Generate Plain-Language Recommendations

**Type:** Feature  
**Priority:** High  
**Description:** Provide practical next steps for each risk finding in language a non-lawyer can understand. Recommendations should help users decide what to ask a lawyer, counterparty, or property professional.

**Acceptance criteria:**
- Each material risk includes a recommended user action.
- Recommendations avoid presenting themselves as formal legal advice.
- Recommendations reference the risky clause and supporting legal context where available.

**Dependencies:** RT-303

#### Ticket RT-305: Generate Final Contract Review Report

**Type:** Feature  
**Priority:** High  
**Description:** Combine all findings into a single final report with overall risk level, executive summary, top risks, clause-level details, and recommended next steps. This is the main user-facing output.

**Acceptance criteria:**
- A completed analysis produces one final report for the uploaded contract.
- The report includes overall risk level and a concise summary.
- The report lists risks in a user-friendly order, with the highest-risk items first.

**Dependencies:** RT-303, RT-304

#### Ticket RT-306: Add Confidence, Disclaimer, and Review Boundaries

**Type:** Feature  
**Priority:** High  
**Description:** Add transparency cues to AI-generated findings, including confidence, legal scope, and professional-review disclaimers. This helps users understand the limits of automated review.

**Acceptance criteria:**
- Every report includes a clear legal-use disclaimer.
- Findings include confidence or uncertainty indicators.
- The report states when a conclusion could not be supported by the available legal knowledge base.

**Dependencies:** RT-305

---

### E5 Tickets: Review Results, Chat, and User Guidance

#### Ticket RT-401: Save Completed Review Results

**Type:** Integration  
**Priority:** Critical  
**Description:** Store completed AI review results so users can return to them later. Results should remain tied to the correct user, session, and document.

**Acceptance criteria:**
- A completed analysis result is saved and linked to its session and document.
- The saved result can be retrieved after the user leaves and returns.
- A user cannot access another user's saved result.

**Dependencies:** RT-305

#### Ticket RT-402: Provide User-Ready Review Result View

**Type:** Feature  
**Priority:** Critical  
**Description:** Expose the saved report in a format that can power a user interface or API demo. The result should be organized for business users rather than raw AI output.

**Acceptance criteria:**
- A user can request the final review report for a session or document.
- The response includes summary, overall risk, findings, recommendations, and citations.
- If no report is ready, the user receives a clear pending or not-found response.

**Dependencies:** RT-401

#### Ticket RT-403: Notify Users When Analysis Completes

**Type:** Integration  
**Priority:** High  
**Description:** Add a completion signal so users know when their contract review is ready. This can begin as status refresh behavior and later support real-time notifications.

**Acceptance criteria:**
- The session status changes when analysis completes.
- The user can see that a new report is available.
- Failed and completed outcomes are visibly different.

**Dependencies:** RT-401, RT-005

#### Ticket RT-404: Ground Follow-Up Chat in Contract and Results

**Type:** Feature  
**Priority:** High  
**Description:** Let users ask questions after a report is generated, using the uploaded contract, risk findings, and legal references as context. This turns the report into an interactive review assistant.

**Acceptance criteria:**
- A user can ask a follow-up question inside the same review session.
- Answers reference the user's contract review context when relevant.
- The system avoids answering outside the supported legal scope without a clear boundary message.

**Dependencies:** RT-402, RT-306

#### Ticket RT-405: Export Review Summary

**Type:** Feature  
**Priority:** Medium  
**Description:** Allow users to download or share a concise review summary for follow-up with lawyers, agents, or counterparties. The export should be useful without exposing internal system details.

**Acceptance criteria:**
- A user can export a report summary after analysis completes.
- The export includes date, document name, overall risk, key findings, and disclaimer.
- The export excludes hidden system data and secrets.

**Dependencies:** RT-402

---

### E6 Tickets: Operational Readiness, Quality, and Trust

#### Ticket RT-501: Remove Hardcoded Sensitive Configuration

**Type:** DevOps  
**Priority:** Critical  
**Description:** Move sensitive service configuration out of committed project settings and into environment-managed secrets. This is required before demos are shared outside a local development machine.

**Acceptance criteria:**
- No real access keys or secret values are stored in committed configuration.
- Local setup documentation explains how developers provide private values.
- The application fails clearly when required secret configuration is missing.

**Dependencies:** None

#### Ticket RT-502: Add Review Workflow Monitoring and Audit Trail

**Type:** DevOps  
**Priority:** High  
**Description:** Add visibility into major review workflow events such as upload accepted, analysis started, analysis completed, and analysis failed. This supports troubleshooting and legal-tech traceability.

**Acceptance criteria:**
- Support staff can trace a document review through major lifecycle events.
- Analysis failures are visible with actionable context.
- User document content is not written into operational logs.

**Dependencies:** RT-204, RT-401

#### Ticket RT-503: Create Automated Regression Coverage

**Type:** Testing  
**Priority:** High  
**Description:** Add repeatable tests for the highest-risk workflows: authentication, session ownership, upload completion, knowledge ingestion, clause analysis, and result retrieval. This reduces release risk as AI features are added.

**Acceptance criteria:**
- Core user workflows have automated regression coverage.
- Security-sensitive ownership checks are covered.
- Test results can be reviewed by the team before each release.

**Dependencies:** RT-402

#### Ticket RT-504: Run End-to-End User Acceptance Review

**Type:** Testing  
**Priority:** Critical  
**Description:** Validate the complete user journey using representative Vietnamese real estate contract samples. This confirms that the product can demonstrate both knowledge ingestion and contract risk analysis.

**Acceptance criteria:**
- A test user can upload a contract and receive a completed risk report.
- The report includes at least one clause-level finding with a legal reference.
- Stakeholders can complete the demo without developer intervention.

**Dependencies:** RT-105, RT-402, RT-503

#### Ticket RT-505: Prepare Demo Deployment Checklist

**Type:** DevOps  
**Priority:** High  
**Description:** Create a repeatable checklist for preparing a local or demo environment, including required services, secrets, sample legal sources, sample contracts, and reset steps. This makes demos more reliable.

**Acceptance criteria:**
- A developer can prepare the demo environment from the checklist.
- Required sample data is identified and versioned.
- The checklist includes cleanup and reset instructions for repeated demos.

**Dependencies:** RT-501, RT-504

---

## 3. Sprint Plan

### Sprint 1: Secure Review Workspace and MVP Scope

**Sprint goal:** Confirm the MVP legal scope and stabilize the user workspace that contract review depends on.

**Tickets in this sprint:** RT-001, RT-002, RT-003, RT-004, RT-005, RT-501

**Demo at sprint end:** A user signs in, creates a review session, uploads a supported contract, sees session messages, and sees clear document analysis status without exposing sensitive configuration.

### Sprint 2: Legal Knowledge Base Ingestion

**Sprint goal:** Build the first controlled legal knowledge base for Vietnamese real estate review.

**Tickets in this sprint:** RT-101, RT-102, RT-103, RT-104, RT-106

**Demo at sprint end:** An approved legal source is imported, structured into searchable legal sections, versioned, and displayed with citation-ready metadata.

### Sprint 3: Contract Intake and Analysis Start

**Sprint goal:** Turn uploaded contracts into analyzable clause-level content and start analysis automatically.

**Tickets in this sprint:** RT-201, RT-203, RT-204, RT-205

**Demo at sprint end:** A user uploads a readable contract, the system extracts text, splits it into clauses, starts analysis, and shows success or failure status clearly.

### Sprint 4: Legal Retrieval and AI Risk Findings

**Sprint goal:** Produce grounded clause-level risk findings using the legal knowledge base.

**Tickets in this sprint:** RT-105, RT-301, RT-302, RT-303, RT-304, RT-306

**Demo at sprint end:** The system reviews contract clauses, retrieves relevant legal references, classifies risk severity, and produces recommendations with confidence and disclaimers.

### Sprint 5: Final Report, Results Delivery, and Follow-Up Chat

**Sprint goal:** Deliver completed review reports to users and support guided follow-up questions.

**Tickets in this sprint:** RT-305, RT-401, RT-402, RT-403, RT-404, RT-502

**Demo at sprint end:** A completed analysis is saved, the user can view the final report, see the session marked complete, and ask a follow-up question grounded in the review.

### Sprint 6: OCR, Export, Regression, and Demo Readiness

**Sprint goal:** Harden the product for reliable demonstrations and broader real-world document samples.

**Tickets in this sprint:** RT-202, RT-405, RT-503, RT-504, RT-505

**Demo at sprint end:** The team runs an end-to-end demo with representative contracts, including scanned-document support, report export, regression coverage review, and a repeatable deployment checklist.

