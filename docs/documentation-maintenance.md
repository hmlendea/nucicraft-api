# Documentation Maintenance

| Metadata | Value |
|----------|-------|
| Purpose | Define how future contributors and coding agents keep this knowledge base synchronised with implementation. |
| Scope | Every document under `docs/` plus related root README, architecture, and security material. |
| Primary sources | [Documentation index](README.md), [coverage map](documentation-coverage.md), [repository structure](repository-structure.md), and [change guide](change-guide.md) |
| Related documents | [Ambiguity register](ambiguities-and-open-questions.md), [design decisions](design-decisions.md), and [invariants](invariants.md) |

## Authority And Accuracy

Source code is authoritative for current behaviour. Tests, root documentation, comments, package metadata, and this corpus are evidence but can become stale.

When documents conflict with implementation:
1. Verify the controlling implementation path.
2. Document current behaviour.
3. Preserve documented intent only when labelled as intent or history.
4. Add a discrepancy or ambiguity when rationale remains indeterminate.
5. Revise every affected document in the identical modification.

Do not convert probable intent into confirmed behaviour.

## Mandatory Synchronisation Rules

- Behavioural modifications require the owning document under [behaviour](behaviour/) and relevant trace under [flows](flows/) to be revised.
- Architectural modifications require [architecture.md](architecture.md), affected component documents, [repository-structure.md](repository-structure.md), and [design-decisions.md](design-decisions.md) review.
- Novel or revised configuration requires [configuration.md](configuration.md) and consuming component documentation review.
- Novel integrations or package-boundary changes require [integrations.md](integrations.md), [dependencies.md](dependencies.md), security, error, and flow review.
- Persistence representation changes require [data-model.md](data-model.md), [state-and-persistence.md](state-and-persistence.md), migration guidance, invariants, and test documentation review.
- Novel invariants or revised comparison semantics require [invariants.md](invariants.md).
- HTTP route, request, response, status, or HMAC changes require [interfaces.md](interfaces.md) and the HTTP component.
- Concurrency, async, locking, worker, or cancellation changes require [concurrency-and-scheduling.md](concurrency-and-scheduling.md).
- Build, CI, release, runtime requirement, or operational procedure changes require [build-and-deployment.md](build-and-deployment.md) and public README review.
- Test additions or removals that alter confidence boundaries require [testing.md](testing.md).
- Novel unresolved matters require [ambiguities-and-open-questions.md](ambiguities-and-open-questions.md).
- Every substantial source area must remain mapped by [documentation-coverage.md](documentation-coverage.md).
- The [documentation index](README.md) must link every added, renamed, or removed document.

## Change-Impact Matrix

| Code Modification | Documentation To Review |
|-------------------|-------------------------|
| Controller route or action | HTTP component, interfaces, owning behaviour, owning flow, data model, change guide, coverage, root README. |
| Request or response property | Data model, interfaces, HMAC and serialisation concerns, owning behaviour, security for sensitive fields, invariants. |
| Service method or domain rule | Owning component, behaviour, flow, invariants, errors, tests, coverage. |
| Data object or mapping | Data model, persistence component, state, compatibility, tests, migration and deployment guidance. |
| Repository registration or store | Architecture, host component, repository structure, configuration, state, startup flow, deployment, testing. |
| Settings property or default | Configuration, consuming component, deployment, security if secret, integration if destination. |
| Middleware or pipeline order | Architecture, host component, security, error handling, cross-cutting concerns, testing. |
| DI lifetime | Architecture, component dependencies, concurrency, state, design decisions, tests. |
| External client or protocol | Integration, dependency, external component, external flow, security, error handling, configuration, tests. |
| Logging field or sink | Cross-cutting concerns, security, configuration, operations, data classification. |
| Timestamp or identifier algorithm | Data model, invariants, owning component and behaviour, persistence compatibility, tests. |
| Package version | Dependencies, integrations, affected boundary documents, build, tests, ambiguity register when semantics remain external. |
| CI workflow or task | Build and deployment, testing, repository structure, root README. |
| Release script | Build and deployment, integrations, security, ambiguities. |
| Background worker or scheduler | Architecture, concurrency, state, error handling, configuration, build and deployment, tests, coverage. |
| Security model | Security, interfaces, configuration, errors, components, flows, root SECURITY and README. |

## Document Roles

| Document Category | Canonical Knowledge |
|-------------------|---------------------|
| Repository overview | Purpose, boundaries, principal capabilities, and explicit exclusions. |
| Architecture | Decomposition, dependency direction, composition, topology, and lifecycle. |
| Repository structure | Physical placement and preservation conventions. |
| Component documents | Ownership, dependencies, internals, configuration, and extension guidance. |
| Behaviour documents | Capability conditions, branches, effects, outputs, and failures. |
| Flow documents | Ordered method transitions and intermediate representations. |
| Data model | Field-level representation and conversion semantics. |
| Interfaces | Public and internal contracts. |
| Configuration | Sources, keys, defaults, validation timing, and consumers. |
| Integrations and dependencies | External systems and package boundaries. |
| State, errors, security, concurrency | Cross-cutting operational semantics. |
| Testing and deployment | Verification and delivery conduct. |
| Decisions and invariants | Non-obvious constraints future changes must preserve. |
| Change guide | Practical modification map. |
| Coverage map | Source-to-document audit and exposed gaps. |
| Ambiguity register | Material unknowns that must not be asserted as fact. |

## Editing Rules

- Use British English.
- Use ASCII punctuation; do not introduce em dashes, en dashes, Unicode ellipses, Unicode arrows, or box characters.
- Include metadata with purpose, scope, primary sources, and related documents in every substantial document.
- Link every referenced repository file or directory with a verified relative Markdown link.
- Prefer symbol names and semantic explanations over source reproduction.
- Retain enough local context for selective retrieval without copying canonical sections wholesale.
- Use Mermaid diagrams only when they add process or relationship information.
- Never include secret values, personal records from local stores, private keys, or tokens.
- Do not use volatile line-number links or generation timestamps.
- Identify external package conduct as external unless directly verified.

## Review Procedure

For each implementation modification:
1. Search the corpus for changed symbols, routes, keys, and concepts.
2. Use the impact matrix to enumerate documents.
3. Revise canonical detail first.
4. Revise local summaries and links.
5. Update coverage mapping.
6. Verify every local Markdown link.
7. Search for forbidden Unicode punctuation and stale terminology.
8. Compile and execute relevant tests.
9. Inspect the final diff for unsupported claims or copied personal data.

## Link Validation

Every relative link must resolve from its containing document. Links from files under `docs/components`, `docs/behaviour`, or `docs/flows` commonly require two parent traversals to source, while root `docs` files require one.

When moving or renaming a document, search every Markdown file before completing the move. The index and coverage map are mandatory consumers.

## Mermaid Maintenance

- Keep node identifiers simple and labels quoted when punctuation could confuse parsing.
- Use sequence diagrams for ordered interactions and flowcharts for decisions or dependencies.
- Ensure diagrams agree with prose and source call order.
- Mermaid is supplementary; preserve the complete semantic sequence in prose or tables.

## Ambiguity Maintenance

Resolve an ambiguity only with evidence. When resolved:
1. Revise implementation-facing canonical documents.
2. Remove or mark the ambiguity resolved.
3. Add a design decision when the resolution establishes a durable constraint.
4. Add tests when executable evidence is practical.

Add a novel ambiguity when package behaviour, historical rationale, external deployment, or conflicting evidence materially affects safe modification.

## Coverage Audit Procedure

Before a substantial release or architecture revision:
1. Enumerate tracked production source files and project metadata.
2. Enumerate controllers and public actions.
3. Enumerate service interfaces and methods.
4. Enumerate root data objects and store settings.
5. Enumerate outbound integrations and middleware.
6. Enumerate test fixtures.
7. Compare each category with [documentation-coverage.md](documentation-coverage.md).
8. Investigate every unreferenced substantial file or capability.
9. Revalidate the index and all links.

Do not assert complete coverage solely because every directory is named. Semantic processes, failure branches, and external boundaries must also be represented.
