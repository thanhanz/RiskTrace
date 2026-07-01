# AGENTS.md — RiskTrace

## What This Project Is

RiskTrace is a legal-tech system for early-stage risk screening of real estate legal
documents under **Vietnamese law** (sale and purchase agreements, deposit agreements,
lease contracts, supporting paperwork). It is a support tool, not a replacement for a
lawyer — outputs must always be treated as a starting point for a qualified legal
professional to verify, never as a final legal conclusion.

The system has two independently developed parts that communicate over RabbitMQ:

```
RiskTrace/
├── src/            # ASP.NET Core backend — auth, sessions, documents, messaging
│                    # → see src/AGENTS.md
└── ai-service/      # Python service — document analysis, risk scoring, LLM review
                     # → see ai-service/AGENTS.md
```

Always read the AGENTS.md inside the subtree being edited. Do not assume a rule from
one subtree applies to the other unless it's stated here.

## Main Workflow (for orientation only)

1. User registers/signs in and creates a review session.
2. User uploads one or more legal documents.
3. Backend stores document metadata and publishes an analysis request event.
4. AI service analyzes the document against Vietnamese real estate legal scope and
   publishes a result event.
5. Backend stores the result and serves it to the user, who can continue asking
   questions in the session.

## Cross-Service Contract

Backend → AI service: "document uploaded" event.
AI service → Backend: "analysis completed" event.

- Exchange: `risktrace.events`
- Request queue / routing key: `risktrace.documents.uploaded` / `document.uploaded_request`
- Result queue / routing key: `risktrace.ai.responses` / `ai.review_completed`

The event schema is the **only** coupling between the two services. Neither service
should reach into the other's database, internals, or code. If a message shape
changes on one side, the matching schema on the other side must be updated in the
same change — do not let them drift.

## Hard Rules (apply to both services)

- **No hardcoded secrets.** Read from environment variables / configuration. Never
  commit real keys, connection strings, or credentials — not even as "example" values
  that look real.
- **Do not run, build, or execute the project** (compiling, starting servers,
  migrations, tests, Docker up, package installs) unless a human explicitly asks.
  Write code following the patterns in the relevant AGENTS.md; flag anything that
  needs a human to verify by running it.
- **Do not invent Vietnamese real estate legal logic.** What counts as a legal risk,
  a missing clause, a red flag, a required disclosure, or a citation must come from
  the user/product owner or existing code — never fabricated. If asked to implement
  risk-detection logic and the specific rule isn't given, ask instead of guessing.
- **Never state or imply certainty in legal findings.** Anything user-facing must
  read as "may indicate a risk to verify with a lawyer," not as a legal conclusion.
- Keep business/legal logic decisions in the subtree AGENTS.md files — don't
  duplicate or fork content between root and subtrees.
