# [DOCUMENTATION_STANDARDS]

This folder is the active standards library and router for Rasm documentation. Start here to find the reader need, pick the document type, and route to the shared standard that carries each rule. This index is the one file that links across the library; the standards it points to name their neighbors by topic and link back only at their boundaries.

## [1][USE_WHEN]

Use this index when:
- creating or rewriting Markdown documentation;
- deciding which reader need a draft serves and which type fits;
- deciding which standard carries a rule;
- splitting, merging, moving, or deleting a document;
- checking whether a standard is active.

## [2][SOURCE_PRECEDENCE]

Use the strongest applicable source:
1. Current repository source, manifests, generated contracts, and runnable tool output.
2. `docs/usage.md` for cross-stack source precedence and proof order.
3. The document-type standard in this folder.
4. The shared standard for position, form, craft, evidence, or notation.
5. Maintained source required by the active local standard.

When a claim can drift, prove it with repository truth before web examples or prior notes.

## [3][READ_ORDER]

Read order is workflow order; source precedence still decides conflicts.

1. Use the reader-need map and type chooser below to pick one document type.
2. For document-type standards, apply the page-anatomy and opening-contract rules in [information-structure.md](information-structure.md), then apply the chosen type standard.
3. Apply the five shared standards: position, form, craft, evidence, and notation.
4. Apply exactly one type standard when the artifact serves a reader-need document type. Shared standards, instruction files, generated mirrors, indexes, retrieval chunks, MCP catalogs, and structured-output contracts route through the shared route instead.

## [4][READER_NEED_MAP]

Classify a document by what the reader does:
- Tutorial: a tested path that teaches a learner.
- How-to: one real task for a competent reader.
- Reference: accurate facts a working reader looks up.
- Explanation: context, trade-offs, structure, or decisions a reader wants to understand.

Resolve edge cases by action and cognition: action plus learning is tutorial, action plus work is how-to, cognition plus work is reference, and cognition plus learning is explanation. Publish only documents that answer a real reader need; do not create empty quadrant folders to look complete.

## [5][CHOOSE_TYPE]

Map the reader need to the artifact. The linked path encodes the document family, so each row routes one reader need to one standard without a redundant family column.

| [INDEX] | [READER_NEED]                            | [TYPE_PATH]                                                        | [ROUTE_AWAY]                  |
| :-----: | :--------------------------------------- | :----------------------------------------------------------------- | :---------------------------- |
|   [1]   | Current structure and invariants         | [explanation/architecture.md](explanation/architecture.md)         | decisions, tasks, incidents   |
|   [2]   | Durable decision and consequences        | [explanation/adr.md](explanation/adr.md)                           | proposals, architecture       |
|   [3]   | Proposal or RFC-style review             | [explanation/design-doc.md](explanation/design-doc.md)             | accepted decisions, structure |
|   [4]   | Milestone sequence and exit proof        | [explanation/roadmap.md](explanation/roadmap.md)                   | release history, support      |
|   [5]   | Test portfolio and gate policy           | [explanation/test-strategy.md](explanation/test-strategy.md)       | test procedures               |
|   [6]   | Entry point or local hub                 | [reference/readme.md](reference/readme.md)                         | catalogs, tutorials           |
|   [7]   | Curated lookup truth                     | [reference/reference.md](reference/reference.md)                   | procedures, teaching          |
|   [8]   | HTTP or generated API contract           | [reference/api.md](reference/api.md)                               | symbol docs, plans            |
|   [9]   | Public symbol intent and failures        | [reference/code-documentation.md](reference/code-documentation.md) | endpoint catalogs, tutorials  |
|  [10]   | Support by version, platform, or feature | [reference/support-matrix.md](reference/support-matrix.md)         | roadmaps, migrations          |
|  [11]   | Repeatable competent-reader task         | [task/how-to.md](task/how-to.md)                                   | learning, recovery            |
|  [12]   | Operational symptom response             | [task/runbook.md](task/runbook.md)                                 | normal tasks, postmortems     |
|  [13]   | Contribution workflow and PR evidence    | [task/contributing.md](task/contributing.md)                       | onboarding, incidents         |
|  [14]   | Tested learning path                     | [learning/tutorial.md](learning/tutorial.md)                       | reference, variants           |
|  [15]   | Readiness ramp                           | [learning/onboarding.md](learning/onboarding.md)                   | task guides, contributing     |

If no quadrant fits cleanly, reduce the scope until one reader outcome is primary.

## [6][SHARED_STANDARDS]

Every document obeys five shared standards; each rule has exactly one source:
- [agentic-documentation.md](agentic-documentation.md): position and agent cognition — salience and ordering within a unit, artifact separation, provider behavior, instruction files, indexes, retrieval, and catalogs.
- [information-structure.md](information-structure.md): form — container selection, tables, structured records, lists, diagrams, code blocks, page anatomy, line wrapping, and chunks.
- [style-guide.md](style-guide.md): craft — sentences, terminology, punctuation, links, examples, code-safe Markdown, and accessibility.
- [proof.md](proof.md): evidence — strength, freshness, source conflicts, verification, agent-surface evaluation, and preservation under refactor.
- [formatting.md](formatting.md): notation — status and invocation markers, table styling, whitespace, and the heading idiom.

Use these 5 shared standards as the root audit axes. A valid root standards file is scoreable on position, form, craft, evidence, and notation without inventing a sixth local axis or turning the audit into session commentary.

Specialized instruction-surface authoring routes through [agents-md.md](agents-md.md). It defines the semantic slots, root profile, local extension grammar, route-away rules, trust boundaries, and corpus rebuild rules for `AGENTS.md` files while the five shared standards remain the audit axes.

## [7][PLACEMENT]

Place documentation where the reader or tool first looks:
- Repository root for corpus-wide entry documents.
- Scope-local directory for package, product, tool, or subsystem truth.
- `docs/` for shared cross-scope material and generated documentation entrypoints.
- `docs/standards/` for authoring standards only.
- Source files for public symbol documentation and rationale that names and types cannot express.

Prefer one source for a claim, and link across sources instead of copying the same claim into multiple pages.

## [8][SPLIT_LINK]

When a draft serves more than one primary reader need, split it:
- Move background from tutorials and how-to guides into explanation.
- Move catalogs, option tables, and command inventories into reference or API documentation.
- Move step-by-step work from reference leaves into how-to guides.
- Move operational response from how-to guides into runbooks.
- Move durable decision rationale from architecture into ADRs.
- Move implementation sequence and exit criteria into roadmaps or design docs.
- Move contribution workflow from README files into contributing guides.

After splitting, add the smallest useful cross-link and do not leave a summary copy that can drift.

## [9][LIFECYCLE]

Maintain documentation with these code-like rules:
- Create the smallest useful document that answers the reader need.
- Update docs in the same change that alters the controlling source they describe.
- Delete or replace dead documentation when it is known to be wrong.
- Add a drift condition for a drift-prone claim, and prefer an explicit stale marker over silent uncertainty when a claim cannot be verified during maintenance.

Unless live product support and evidence justify them, do not preserve old paths, terminology, commands, or product claims as compatibility notes.

## [10][FOLDER_LAYOUT]

- Root files are the five shared standards, this index, [agents-md.md](agents-md.md), and `AGENTS.md`, the agent instruction file for `docs/standards/**`.
- `explanation/` holds decision, design, architecture, roadmap, and test-strategy standards.
- `reference/` holds entry, lookup, API, code-documentation, and support-matrix standards.
- `task/` holds normal-task, operational-recovery, and contribution standards.
- `learning/` holds tutorial and onboarding standards.

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
- Standards that mention authoring interactions, removed workflow names, or transient task labels instead of durable document behavior.

[MIXED_MODULE_FILES]:
- Mixed module file: a module document that carries contracts, command maps, snippets, status vocabularies, path structure, risks, gotchas, and implementation sequence under one design doc, README, or architecture page must split by reader need: current code structure to architecture, callable contracts to API, lookup facts to reference, source-symbol semantics to code documentation, future work to roadmap, and failure response to runbook.

## [12][MAINTENANCE_RULES]

- Keep this README a route map; put detailed rules in the source standard.
- Prefer restructuring, deletion, and route links over duplicated guidance.
- Restructure without dropping load-bearing facts; diff content coverage before replacing a document, per the preservation rule in proof.
- Route adjacent document updates through the relation record in [information-structure.md](information-structure.md) only when the changed fact alters reader action, proof, or maintenance.
- Use a table only when comparison or lookup value beats a list.
- Remove a stale standard instead of keeping a legacy alias.
- Keep release history in the project's release mechanism, not in this index.

## [13][BOUNDARIES]

- Shared standards carry cross-cutting rules; this README only routes readers to them.
- [agents-md.md](agents-md.md) carries `AGENTS.md` semantic slots, profiles, and anti-fragility rules; [agentic-documentation.md](agentic-documentation.md) carries salience, provider behavior, and artifact separation.
- Type standards carry artifact-specific structure; this README only chooses the primary type.
- `docs/usage.md` carries cross-stack implementation precedence and proof order outside the standards library.
- `docs/standards/AGENTS.md` carries local agent routing, read scope, and close checks for edits inside this folder.

## [14][VALIDATION]

- [ ] Active standards are linked by current filename.
- [ ] Each shared standard has one clear source.
- [ ] Root-file audits use the five shared standards as their axes.
- [ ] Each type standard appears in exactly one family.
- [ ] The reader-need map and type chooser route to a single primary type.
- [ ] This index routes readers instead of restating standard bodies.
- [ ] Mixed module files are split into the source type standards instead of preserved as one polished file.
