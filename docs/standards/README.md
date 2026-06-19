# [DOCUMENTATION_STANDARDS]

This folder is the active standards library and router for durable documentation. Use it to classify the reader need, pick one document type, and route each rule to its owner.

## [01]-[USE_WHEN]

Use this index when:
- creating or rewriting Markdown documentation
- deciding which reader need a draft serves
- choosing the document type
- splitting, merging, moving, or deleting a document
- locating the standard that owns a rule.

## [02]-[AUTHORITY]

[RULE_AUTHORITY]:
- Active instruction files control work order and local constraints.
- This README controls reader need, document-type choice, corpus placement, split/link rules, and lifecycle routing.
- Shared standards control form, craft, claim discipline, and notation.
- Type standards control artifact-specific structure, status vocabulary, local fields, and examples.

[CLAIM_CONFIDENCE]:
- `proof.md` controls claim discipline, artifact confidence, gaps, generated artifacts, and preservation.
- Current repository material, manifests, generated contracts, and local tool output decide present-tense repository claims.
- Active repository instructions apply when the change involves cross-stack owner precedence, implementation confirmation, host-library routing, or command/tooling claims outside this standards corpus.

Current repository state is input. Future-facing standards target the stronger rule, not current drift.

## [03]-[READ_ORDER]

Read order is workflow order; owner routing still decides conflicts.

1. Use the reader-need matrix and type chooser to pick one primary document type.
2. Apply page-anatomy and opening-contract rules in [information-structure.md](information-structure.md).
3. Apply the chosen type standard.
4. Apply the shared standards: form, craft, claim discipline, and notation.

[NON_TYPE_ROUTES]:

| [INDEX] | [SURFACE]                        | [OWNER_ROUTE]                                                        |
| :-----: | :------------------------------- | :------------------------------------------------------------------- |
|  [01]   | mirrors, indexes, retrieval, MCP | [proof](proof.md), [information-structure](information-structure.md) |
|  [02]   | structured output and checks     | [proof](proof.md)                                                    |
|  [03]   | confidence fields and gaps       | [proof](proof.md)                                                    |
|  [04]   | containers and examples          | [information-structure](information-structure.md)                    |

## [04]-[READER_NEED_MAP]

Classify a document by the reader action:

| [INDEX] | [PRIMARY_AXIS] | [SECOND_AXIS] | [DOCUMENT_FAMILY] | [USE]                            |
| :-----: | :------------- | :------------ | :---------------- | :------------------------------- |
|  [01]   | action         | learning      | Tutorial          | teach a tested path              |
|  [02]   | action         | work          | How-to            | complete one real task           |
|  [03]   | cognition      | work          | Reference         | look up accurate facts           |
|  [04]   | cognition      | learning      | Explanation       | understand structure or decision |

Publish only documents that answer a real reader need. If no quadrant fits cleanly, reduce the scope until one reader outcome is primary.

## [05]-[CHOOSE_TYPE]

| [INDEX] | [NEED]                  | [TYPE]                                        | [ROUTE_AWAY]                                          |
| :-----: | :---------------------- | :-------------------------------------------- | :---------------------------------------------------- |
|  [01]   | current structure       | [architecture](explanation/architecture.md)   | [ADR](explanation/adr.md), roadmap, design            |
|  [02]   | durable decision        | [ADR](explanation/adr.md)                     | design, architecture                                  |
|  [03]   | proposal review         | [design](explanation/design-doc.md)           | [ADR](explanation/adr.md), architecture               |
|  [04]   | target sequence         | [roadmap](explanation/roadmap.md)             | release route, [support](reference/support-matrix.md) |
|  [05]   | gate policy             | [test strategy](explanation/test-strategy.md) | [how-to](task/how-to.md), [runbook](task/runbook.md)  |
|  [06]   | entrypoint or hub       | [README](reference/readme.md)                 | [reference](reference/reference.md), tutorial         |
|  [07]   | lookup facts            | [reference](reference/reference.md)           | [how-to](task/how-to.md), tutorial                    |
|  [08]   | callable API contract   | [API](reference/api.md)                       | symbol docs, [roadmap](explanation/roadmap.md)        |
|  [09]   | public symbol semantics | [code docs](reference/code-documentation.md)  | endpoint catalogs, tutorial                           |
|  [10]   | support comparison      | [support matrix](reference/support-matrix.md) | [roadmap](explanation/roadmap.md), migration route    |
|  [11]   | repeatable task         | [how-to](task/how-to.md)                      | [tutorial](learning/tutorial.md), runbook             |
|  [12]   | symptom response        | [runbook](task/runbook.md)                    | [how-to](task/how-to.md), postmortem                  |
|  [13]   | contribution workflow   | [contributing](task/contributing.md)          | [onboarding](learning/onboarding.md), incidents       |
|  [14]   | tested learning path    | [tutorial](learning/tutorial.md)              | [reference](reference/reference.md), variants         |
|  [15]   | readiness ramp          | [onboarding](learning/onboarding.md)          | [how-to](task/how-to.md), contributing                |

## [06]-[SHARED_STANDARDS]

| [INDEX] | [AXIS]           | [OWNER]                                              | [CONTROLS]                             |
| :-----: | :--------------- | :--------------------------------------------------- | :------------------------------------- |
|  [01]   | form             | [information-structure.md](information-structure.md) | containers, diagrams, chunks, examples |
|  [02]   | craft            | [style-guide.md](style-guide.md)                     | prose, terms, links, accessibility     |
|  [03]   | claim discipline | [proof.md](proof.md)                                 | confidence, gaps, generated artifacts  |
|  [04]   | notation         | [formatting.md](formatting.md)                       | markers, tables, whitespace, headings  |

[HYGIENE_ROUTES]:
- Agent-usable docs: [information-structure](information-structure.md) chooses the carrier; [style guide](style-guide.md) makes the rule direct.
- Process narration, research-history prose, and non-load-bearing hedges: [style guide](style-guide.md) removes them.
- Confidence fields, gaps, generated artifacts, and preservation: [proof](proof.md) owns the shape.
- External-library implementation policy: route cross-stack precedence through [usage](../usage/README.md), and language-specific decisions through `docs/stacks/<language>/`.

## [07]-[PLACEMENT]

| [INDEX] | [LOOKUP_TRIGGER]                           | [PLACE]               | [OWNER_ROUTE]                                       |
| :-----: | :----------------------------------------- | :-------------------- | :-------------------------------------------------- |
|  [01]   | corpus-wide entry document                 | repository root       | root README or root instruction file                |
|  [02]   | package, product, tool, or subsystem truth | scope-local directory | local README, architecture, reference, or runbook   |
|  [03]   | shared cross-scope material                | `docs/`               | relevant docs route                                 |
|  [04]   | authoring standards                        | `docs/standards/`     | this standards library                              |
|  [05]   | public symbol rationale                    | source files          | code documentation standard                         |
|  [06]   | scope-local target set                     | `<scope>/.planning/`  | roadmap, architecture, design spec, optional README |

Prefer one owner for a claim. Link across owners instead of copying the same claim into multiple pages.

Use `.planning/` as a flat scope-local target container only when roadmap, target architecture, and `SPEC.<slug>.md` files need to live together without turning the scope README into a target-sequence surface.

## [08]-[SPLIT_LINK]

When a draft serves more than one primary reader need, split it:

| [INDEX] | [CONTENT_SMELL]                          | [REMOVE_FROM]           | [DESTINATION]    |
| :-----: | :--------------------------------------- | :---------------------- | :--------------- |
|  [01]   | background in a procedure                | tutorial or how-to      | explanation      |
|  [02]   | catalog, option table, command inventory | how-to or explanation   | reference or API |
|  [03]   | step-by-step work in lookup facts        | reference               | how-to           |
|  [04]   | operational response                     | how-to                  | runbook          |
|  [05]   | durable decision rationale               | architecture            | ADR              |
|  [06]   | implementation sequence                  | architecture or README  | roadmap          |
|  [07]   | proposal review                          | roadmap or architecture | design doc       |
|  [08]   | contribution workflow                    | README                  | contributing     |
|  [09]   | scope-local target routes                | README                  | `.planning/`     |

After splitting, add the smallest cross-link that changes reader action or maintenance. Do not leave a summary copy that can drift.

## [09]-[LIFECYCLE]

- Create the smallest useful document that answers the reader need.
- Update docs in the same change that alters the current behavior they describe.
- Delete or replace dead documentation when it is wrong.
- Add a refresh trigger for a claim that can drift.
- Preserve every load-bearing fact during replacement through `proof.md`.
- Do not preserve old paths, terminology, commands, or product claims as compatibility notes unless the document is a concrete support surface.

## [10]-[FOLDER_LAYOUT]

```text conceptual
docs/standards/
├── explanation/               # architecture, ADR, design, roadmap, test strategy
├── reference/                 # README, lookup, API, code docs, support matrix
├── task/                      # how-to, runbook, contributing
├── learning/                  # tutorial, onboarding
├── README.md                  # this router
├── information-structure.md   # form
├── style-guide.md             # craft
├── proof.md                   # claim discipline
└── formatting.md              # notation
```

Active standards are the files in this layout except folders explicitly marked retired by a trusted local instruction or route owner.

## [11]-[ANTI_PATTERNS]

[TYPE_DRIFT]:
- README files that carry design history, tutorials, or API catalogs.
- Architecture documents that carry task plans or incident response.
- ADRs that include execution procedures instead of decision confirmation.
- Tutorials that branch into production variants.
- Reference leaves that hide procedures in dense tables.
- Runbooks without an observable trigger, route, result check, or escalation.

[SURFACE_DRIFT]:
- Generated API documentation forked by hand-written endpoint or symbol tables.
- Standards that mention task-prompt or session-process language, removed workflow names, or transient task labels instead of durable document behavior.
- Durable docs that carry report leads, research trails, confirmation tails, external reference blocks, or origin-history narration.

[MIXED_MODULE_FILES]:

| [INDEX] | [CONTENT_KIND]         | [DESTINATION]                                         |
| :-----: | :--------------------- | :---------------------------------------------------- |
|  [01]   | current code structure | [architecture](explanation/architecture.md)           |
|  [02]   | callable contracts     | [API](reference/api.md)                               |
|  [03]   | lookup facts           | [reference](reference/reference.md)                   |
|  [04]   | symbol semantics       | [code documentation](reference/code-documentation.md) |
|  [05]   | target sequence        | [roadmap](explanation/roadmap.md)                     |
|  [06]   | failure response       | [runbook](task/runbook.md)                            |
|  [07]   | contribution workflow  | [contributing](task/contributing.md)                  |

## [12]-[MAINTENANCE_RULES]

- Keep this README a route map; put detailed rules in the owner standard.
- Prefer restructuring, deletion, and route links over duplicated guidance.
- Route adjacent document updates through relation records only when the changed fact alters reader action or maintenance.
- Use a table only when comparison or lookup is clearer than a list.
- Remove stale standards instead of keeping legacy aliases.
- Keep release history in the project's release mechanism, not in this index.
- When project-bound docs feed reusable skills, prompts, templates, or standards, extract only the portable rule shape and replace local names, paths, package facts, commands, task IDs, and route details with neutral placeholders.

## [13]-[BOUNDARIES]

- Shared standards carry cross-cutting rules; this README only routes readers to them.
- Type standards carry artifact-specific structure; this README only chooses the primary type.
- Active repository instructions carry cross-stack implementation precedence outside the standards library.
