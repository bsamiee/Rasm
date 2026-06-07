# [DOCUMENTATION_STANDARDS]

This folder is the active standards library and router for documentation. Start here to classify the reader need, pick the document type, and route each rule to its owner. This index is the root route map for the library. Standards link back only at boundaries or when a neighboring rule changes author action.

## [1][USE_WHEN]

Use this index when:
- creating or rewriting Markdown documentation
- deciding which reader need a draft serves and which document type fits
- deciding which standard owns a rule
- splitting, merging, moving, or deleting a document
- checking whether a standard is active

## [2][AUTHORITY_EVIDENCE]

Separate rule authority from claim evidence.

[RULE_AUTHORITY]:
- Active instruction files control work order and local constraints.
- This README controls reader need, document-type choice, corpus placement, split/link rules, and lifecycle routing.
- Shared standards control form, craft, evidence, and notation.
- Type standards control artifact-specific structure, status vocabulary, local proof slots, and examples.
- `AGENTS.md` and `_reports/**` are instruction/source overlays, not standards bodies in the active document-type corpus.

[CLAIM_EVIDENCE]:
- [proof.md](proof.md) controls evidence strength, freshness, conflict handling, proof gaps, and docs-as-code gates.
- Current repository source, manifests, generated contracts, and runnable tool output prove present-tense repository claims.
- The active repository instruction route applies only when the change involves cross-stack owner precedence, implementation proof, host-library routing, or command/tooling claims outside this standards corpus.
- Maintained upstream, product, provider, or specification sources apply only where the owning standard requires them.

Current repository truth proves current facts; it does not weaken future-facing standards.

## [3][READ_ORDER]

Read order is workflow order; owner routing and claim evidence still decide conflicts.

1. For standards-library edits, follow [AGENTS.md](AGENTS.md) first.
2. Use the reader-need matrix and type chooser below to pick one primary document type when the artifact serves a reader-need document type.
3. For document-type standards, apply the page-anatomy and opening-contract rules in [information-structure.md](information-structure.md), then apply the chosen type standard.
4. Apply the 4 shared standards: form, craft, evidence, and notation.

[NON_TYPE_ROUTES]:

| [INDEX] | [SURFACE]                        | [OWNER_ROUTE]                                                        |
| :-----: | :------------------------------- | :------------------------------------------------------------------- |
|   [1]   | instructions and overlay prose   | [AGENTS](agents-md.md)                                               |
|   [2]   | mirrors, indexes, retrieval, MCP | [proof](proof.md), [information-structure](information-structure.md) |
|   [3]   | structured output and checks     | [proof](proof.md)                                                    |
|   [4]   | proof fields and gates           | [proof.md](proof.md)                                                 |
|   [5]   | containers and examples          | [information-structure.md](information-structure.md)                 |

## [4][READER_NEED_MAP]

Classify a document by the reader action:

| [INDEX] | [PRIMARY_AXIS] | [SECOND_AXIS] | [DOCUMENT_FAMILY] | [USE]                                                   |
| :-----: | :------------- | :------------ | :---------------- | :------------------------------------------------------ |
|   [1]   | action         | learning      | Tutorial          | teach a tested path                                     |
|   [2]   | action         | work          | How-to            | complete one real task                                  |
|   [3]   | cognition      | work          | Reference         | look up accurate facts                                  |
|   [4]   | cognition      | learning      | Explanation       | understand context, trade-offs, structure, or decisions |

Publish only documents that answer a real reader need. If no quadrant fits cleanly, reduce the scope until one reader outcome is primary.

## [5][CHOOSE_TYPE]

Map the reader need to the artifact. Route-away cells name the standards that should receive adjacent content instead.

| [INDEX] | [NEED]                  | [TYPE]                                        | [ROUTE_AWAY]                                          |
| :-----: | :---------------------- | :-------------------------------------------- | :---------------------------------------------------- |
|   [1]   | current structure       | [architecture](explanation/architecture.md)   | [ADR](explanation/adr.md), task routes                |
|   [2]   | durable decision        | [ADR](explanation/adr.md)                     | [design](explanation/design-doc.md), architecture     |
|   [3]   | proposal review         | [design](explanation/design-doc.md)           | [ADR](explanation/adr.md), architecture               |
|   [4]   | milestone exit proof    | [roadmap](explanation/roadmap.md)             | release route, [support](reference/support-matrix.md) |
|   [5]   | gate policy             | [test strategy](explanation/test-strategy.md) | [how-to](task/how-to.md), [runbook](task/runbook.md)  |
|   [6]   | entrypoint or hub       | [README](reference/readme.md)                 | [reference](reference/reference.md), tutorial         |
|   [7]   | lookup truth            | [reference](reference/reference.md)           | [how-to](task/how-to.md), tutorial                    |
|   [8]   | callable API contract   | [API](reference/api.md)                       | symbol docs, [roadmap](explanation/roadmap.md)        |
|   [9]   | public symbol semantics | [code docs](reference/code-documentation.md)  | endpoint catalogs, tutorial                           |
|  [10]   | support comparison      | [support matrix](reference/support-matrix.md) | [roadmap](explanation/roadmap.md), migration route    |
|  [11]   | repeatable task         | [how-to](task/how-to.md)                      | [tutorial](learning/tutorial.md), runbook             |
|  [12]   | symptom response        | [runbook](task/runbook.md)                    | [how-to](task/how-to.md), postmortem                  |
|  [13]   | contribution workflow   | [contributing](task/contributing.md)          | [onboarding](learning/onboarding.md), incidents       |
|  [14]   | tested learning path    | [tutorial](learning/tutorial.md)              | [reference](reference/reference.md), variants         |
|  [15]   | readiness ramp          | [onboarding](learning/onboarding.md)          | [how-to](task/how-to.md), contributing                |

## [6][SHARED_STANDARDS]

Each cross-cutting rule routes to exactly one owner:

| [INDEX] | [AXIS]   | [OWNER]                                              | [CONTROLS]                                |
| :-----: | :------- | :--------------------------------------------------- | :---------------------------------------- |
|   [1]   | form     | [information-structure.md](information-structure.md) | containers, diagrams, chunks, examples    |
|   [2]   | craft    | [style-guide.md](style-guide.md)                     | prose, terms, links, accessibility        |
|   [3]   | evidence | [proof.md](proof.md)                                 | proof, freshness, conflicts, preservation |
|   [4]   | notation | [formatting.md](formatting.md)                       | markers, tables, whitespace, headings     |

Audit root standards against form, craft, evidence, and notation only. If a finding does not fit those 4 axes, route it to the owning standard instead of creating a local audit category.

[HYGIENE_ROUTES]:
- Agent-usable docs: [information structure](information-structure.md) chooses the carrier; [style guide](style-guide.md) makes the rule direct.
- Process narration, source-history prose, and non-load-bearing hedges: [style guide](style-guide.md) removes them; [agents-md](agents-md.md) owns instruction-specific cases.
- Proof, source, provenance, and freshness fields: [proof](proof.md) owns evidence shape and proof gaps.
- External-library implementation policy: route docs behavior through [docs overlay](../AGENTS.md), cross-stack precedence through [usage](../usage/README.md), and language-specific decisions through `stacks/<language>/`.

## [7][PLACEMENT]

Place documentation where the reader or tool first looks:

| [INDEX] | [LOOKUP_TRIGGER]                           | [PLACE]               | [OWNER_ROUTE]                                     |
| :-----: | :----------------------------------------- | :-------------------- | :------------------------------------------------ |
|   [1]   | Corpus-wide entry document                 | repository root       | root README or root instruction file              |
|   [2]   | Package, product, tool, or subsystem truth | scope-local directory | local README, architecture, reference, or runbook |
|   [3]   | Shared cross-scope material                | `docs/`               | relevant docs route                               |
|   [4]   | Authoring standards                        | `docs/standards/`     | this standards library                            |
|   [5]   | Public symbol rationale                    | source files          | code documentation standard                       |

Prefer one owner for a claim. Link across owners instead of copying the same claim into multiple pages.

## [8][SPLIT_LINK]

When a draft serves more than one primary reader need, split it:

| [INDEX] | [CONTENT_SMELL]                          | [REMOVE_FROM]           | [DESTINATION]    |
| :-----: | :--------------------------------------- | :---------------------- | :--------------- |
|   [1]   | Background in a procedure                | tutorial or how-to      | explanation      |
|   [2]   | Catalog, option table, command inventory | how-to or explanation   | reference or API |
|   [3]   | Step-by-step work in lookup facts        | reference               | how-to           |
|   [4]   | Operational response                     | how-to                  | runbook          |
|   [5]   | Durable decision rationale               | architecture            | ADR              |
|   [6]   | Implementation sequence                  | architecture or README  | roadmap          |
|   [7]   | Proposal review                          | roadmap or architecture | design doc       |
|   [8]   | Contribution workflow                    | README                  | contributing     |

After splitting, add the smallest cross-link that changes reader action, proof, or maintenance. Do not leave a summary copy that can drift.

## [9][LIFECYCLE]

Maintain documentation with these rules:
- Create the smallest useful document that answers the reader need.
- Update docs in the same change that alters the controlling source they describe.
- Delete or replace dead documentation when current proof shows it is wrong.
- Add a drift condition for a drift-prone claim. Prefer an explicit stale marker over silent uncertainty when a claim cannot be verified during maintenance.
- Preserve every load-bearing fact during replacement through the preservation rule in [proof.md](proof.md).

Unless live product support and evidence justify them, do not preserve old paths, terminology, commands, or product claims as compatibility notes.

## [10][FOLDER_LAYOUT]

```text conceptual
docs/standards/
├── _reports/                  # source-material work reports
├── explanation/               # architecture, ADR, design, roadmap, test strategy
├── reference/                 # README, lookup, API, code docs, support matrix
├── task/                      # how-to, runbook, contributing
├── learning/                  # tutorial, onboarding
├── README.md                  # this router
├── AGENTS.md                  # local instruction overlay
├── information-structure.md   # form
├── style-guide.md             # craft
├── proof.md                   # evidence
├── formatting.md              # notation
└── agents-md.md               # AGENTS.md surface standard
```

Active standards are the files in this layout except `_reports/**` and folders explicitly marked deprecated by a trusted local instruction or route owner. `AGENTS.md` is an instruction overlay, not a standards body.

## [11][ANTI_PATTERNS]

These anti-patterns fall into three groups:

[TYPE_DRIFT]:
- README files that carry design history, tutorials, or API catalogs.
- Architecture documents that carry task plans or incident response.
- ADRs that include execution procedures instead of decision confirmation.
- Tutorials that branch into production variants.
- Reference leaves that hide procedures in dense tables.
- Runbooks without an observable trigger, route, verification, or escalation.

[SURFACE_DRIFT]:
- Generated API documentation forked by hand-written endpoint or symbol tables.
- Standards that mention task-prompt or session-process language, removed workflow names, or transient task labels instead of durable document behavior.

[MIXED_MODULE_FILES]:

| [INDEX] | [CONTENT_KIND]                | [DESTINATION]                                         |
| :-----: | :---------------------------- | :---------------------------------------------------- |
|   [1]   | current code structure        | [architecture](explanation/architecture.md)           |
|   [2]   | callable contracts            | [API](reference/api.md)                               |
|   [3]   | lookup facts                  | [reference](reference/reference.md)                   |
|   [4]   | source-symbol semantics       | [code documentation](reference/code-documentation.md) |
|   [5]   | future work and exit criteria | [roadmap](explanation/roadmap.md)                     |
|   [6]   | failure response              | [runbook](task/runbook.md)                            |
|   [7]   | contribution workflow         | [contributing](task/contributing.md)                  |

## [12][MAINTENANCE_RULES]

- Keep this README a route map; put detailed rules in the owner standard.
- Prefer restructuring, deletion, and route links over duplicated guidance.
- Route adjacent document updates through the relation record in [information-structure.md](information-structure.md) only when the changed fact alters reader action, proof, or maintenance.
- Use a table only when comparison or lookup is clearer than a list.
- Remove a stale standard instead of keeping a legacy alias.
- Keep release history in the project's release mechanism, not in this index.

## [13][BOUNDARIES]

- Shared standards carry cross-cutting rules; this README only routes readers to them.
- Type standards carry artifact-specific structure; this README only chooses the primary type.
- The active repository instruction route carries cross-stack implementation precedence and proof order outside the standards library.
- [AGENTS.md](AGENTS.md) carries local agent routing, read scope, audit contracts, and close checks for edits inside this folder.

## [14][VALIDATION]

Use this verification checklist by axis. Command, link, anchor, renderer, and docs-build gates are selected through [proof.md](proof.md); unrun gates are proof gaps.

[FORM]:
- [ ] The reader-need map and type chooser route to a single primary type.
- [ ] Each type standard appears in exactly one family.
- [ ] Mixed module content routes into the owning type standards.

[CRAFT]:
- [ ] This index routes readers instead of restating standard bodies.
- [ ] Route labels and links name the owner a reader can act on.

[EVIDENCE]:
- [ ] Claim evidence and rule authority stay separate.
- [ ] Cross-stack proof routes to the active repository instruction route only for its scoped cases.

[NOTATION]:
- [ ] Active standards are linked by current filename.
- [ ] Shared-standard counts, route labels, and folder inventories use consistent notation.
- [ ] Root-file audits use only the 4 shared axes.
- [ ] Router targets are links when the path is an action route and code spans only when the literal path is the fact.
