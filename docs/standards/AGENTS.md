# [STANDARDS_AGENT_ROUTER]

This file governs edits inside `docs/standards/**`. It is a behavioral overlay for standards work: read the active corpus, route each rule to its owner, preserve load-bearing current facts, and close every change with the required representation and proof checks.

## [1][SCOPE]

These instructions extend `CLAUDE.md` and the root `AGENTS.md`; they do not replace them. They apply only to the standards authoring corpus under `docs/standards/**`.

The active corpus is the current root, shared, and type standards listed by [README.md](README.md). Exclude `_reports/**` and any folder explicitly marked deprecated by a trusted local instruction or route owner unless the task names that source material.

Reports, critique passes, memory notes, prompt assets, session history, external research, and deprecated source material are source material only. Promote only durable rules into the owning standard, and do not copy report transcripts, roles, confidence, task framing, fixed wave counts, or report structure into the active standards corpus.

## [2][READ_ORDER]

Before every `docs/standards/**` file change, use the active instruction chain, [README.md](README.md), the full target file, its routed owner standard, linked adjacent standards, and the affected folder or type family. The table adds mode-specific sources; for root audits, include [agents-md.md](agents-md.md) unless the task excludes it.

| [INDEX] | [MODE]              | [EXTRA_READ]                            | [TRIGGER]                                               |
| :-----: | :------------------ | :-------------------------------------- | :------------------------------------------------------ |
|   [1]   | Root audit or edit  | active root standards                   | routing, boundaries, provider behavior, audit rules     |
|   [2]   | Narrow type edit    | needed shared owners, type family       | adjacent status, proof, produced-structure behavior     |
|   [3]   | Named `_reports/**` | `_reports/AGENTS.md`, named reports     | reusable report promotion                               |
|   [4]   | Cross-stack claim   | project-declared implementation-proof owner, proof route | owner precedence, source evidence, implementation, tooling |

Before rewriting, identify poor information representation: prose hiding lookup data, prose-like tables, one-row tables, duplicated table or diagram bodies, decorative diagrams, missing relation records, empty conditional headings, stale links, and decorative proof fields. Fix the problem in scope or route it away explicitly.

When standards work creates or updates a reusable `_reports/` session, update or verify the nearest non-root `AGENTS.md` for that folder and keep report naming, track numbering, correction, promotion, and pruning mechanics in that `_reports/AGENTS.md` leaf.

## [3][RULE_OWNERS]

Use the owner that controls the changed rule; do not copy that owner's body into this file.

| [INDEX] | [OWNER]                    | [CONTROLS]                                                                     |
| :-----: | :------------------------- | :----------------------------------------------------------------------------- |
|   [1]   | `README.md`                | reader need, type choice, corpus placement, split/link, lifecycle              |
|   [2]   | `agentic-documentation.md` | agent salience, artifact separation, provider posture, instruction positioning |
|   [3]   | `agents-md.md`             | `AGENTS.md` semantic slots, profiles, route-away, anti-fragility               |
|   [4]   | `information-structure.md` | containers, records, tables, diagrams, checklists, cardinality                 |
|   [5]   | `style-guide.md`           | prose, sentence mechanics, terminology, links, code-safe Markdown              |
|   [6]   | `proof.md`                 | evidence, preservation, proof gaps, docs-as-code gate selection                |
|   [7]   | `formatting.md`            | bracketed headings, invocation markers, table styling, whitespace              |
|   [8]   | type standards             | artifact-specific structure, status vocabulary, local proof slots              |

## [4][ROOT_FILE_AUDITS]

Audit root standards files against exactly 5 shared axes: position, form, craft, evidence, and notation. Do not invent a sixth audit axis. Instruction-surface specifics may route to [agents-md.md](agents-md.md), but root-file quality scoring stays on the 5 shared standards.

Each finding must carry:
- `Path + line/section`
- `Axis`
- `Issue`
- `Correction task`
- `Rule/standard to tighten`
- `Proof gap` when the audit depends on an unrun command, renderer, link checker, source check, or provider claim

Read-only audits state that no files were edited and no validation gate ran. Edit audits name exact commands only when they ran in the current change or a current status check proves them.

## [5][ARTIFACT_CONTRACT]

Standards files are executable guidance for future agents. Start with scope, reader action, controlling owner or status, highest-risk constraint, and route-away. End with boundaries, proof, validation, or the next safe route.

Future-facing standards use the strongest verified target standard. Current implementation drift does not weaken the target rule, but present-tense current claims still require repository proof or a proof gap.

Treat older paradigms as replacement targets, not baselines. Prefer direct replacement language over deprecation, compatibility, migration-preservation, or local-implementation qualifiers.

Every type standard states the opening contract before examples, taxonomies, or background:
1. Purpose and boundary in the lead.
2. `Use when`.
3. Route-away rule.
4. Agent use.
5. Required produced-document structure.
6. Section cardinality.
7. Adjacent checks and relation-record rule.
8. Maintenance triggers and stale-source events.

Use adjacent-document relation records only when the adjacent fact changes reader action, proof, status interpretation, validation, or maintenance. Use normal Markdown links for background. Route missing owned content to its owner instead of embedding it.

## [6][EDIT_INVARIANTS]

[PRESERVATION]:
- Preserve every load-bearing current command, path, version, flag, field, qualifier, status value, route pointer, proof field, invariant, and source-backed claim.
- Delete or replace stale facts only when current repository truth, owner routing, or proof evidence shows they are obsolete.
- Account for routed, deleted, or replaced facts before removal.

[ROUTING]:
- Pick one primary reader need before rewriting.
- Split or route mixed concept, task, reference, process, proof, and status bodies.
- Choose section containers through [information-structure.md](information-structure.md); do not restate its container matrix here.
- Use [style-guide.md](style-guide.md) and [agentic-documentation.md](agentic-documentation.md) for prose, position, direct present-tense wording, exact source names, and no prompt, session, or process narration.
- Use [agents-md.md](agents-md.md) for `AGENTS.md` semantic slots, route-away decisions, anti-fragility, trust boundaries, root profile, and corpus rebuild rules.

[FIELDS]:
- Omit absent fields instead of filling them with `none`, `n/a`, placeholders, or filler records.
- People or process metadata fields such as `Owner:`, `Role:`, `Team:`, `Maintainer:`, `Reviewer:`, `Stakeholder:`, `Audience:`, `PM:`, `RACI:`, approval ladders, or organizational accountability are forbidden unless a literal local tool output or source standard consumes that exact field.
- Route-owner or source-owner language is allowed when it identifies the standard or path that controls a rule.

## [7][FORBIDDEN_PATTERNS]

[NEVER]:
- Publish live task instructions, chat excerpts, critique summaries, prompt-source narration, rewrite rationale, fixed sub-agent counts, session state, secrets, or nonpublic machine paths.
- Claim a local linter, link checker, docs build, renderer, CI gate, static gate, test gate, runtime gate, host-verification gate, or deployment gate exists or passed unless repository source, configured tooling, local command output, current CI proof, or current status-check output proves it.
- Claim provider behavior exists unless maintained provider documentation or local provider-surface output proves it.
- Use current baseline, older code, older manifests, older configs, partial adoption, or compatibility pressure as a reason to weaken a future-facing standard.
- Preserve stale commands, removed tool names, legacy aliases, or unsupported progress markers.
- Publish empty conditional headings, decorative diagrams, duplicated table or diagram bodies, `Consumed by: none`, or generic metadata whitelists.
- Hand-maintain generated catalogs, mirrors, or provider claims as independent truth.

## [8][BOUNDARIES]

This file routes standards-folder work and enforces local close checks. It does not define document types, prose craft, proof labels, container forms, status vocabularies, or `AGENTS.md` semantic slots.

When this file and a shared standard appear to overlap, keep the durable rule in the shared standard and keep only the local read, routing, or close-check behavior here.

## [9][VALIDATION]

Use this verification checklist by group:

[READ_SCOPE]:
- [ ] The task mode selected the correct read set before edits.
- [ ] `_reports/**` stayed excluded unless the task named it.
- [ ] Root-file audits or edits read every active root standards file in scope, excluding [agents-md.md](agents-md.md) only when the task explicitly excludes it.

[ROOT_AUDIT]:
- [ ] Root-file findings use the 5 shared axes: position, form, craft, evidence, notation.
- [ ] Each finding names path plus line or section, axis, issue, correction task, rule or standard to tighten, and proof gap when applicable.
- [ ] Poor information representation was identified before rewriting, then fixed in scope or routed away.

[ROUTING_PRESERVATION]:
- [ ] Each changed rule routes to one owner.
- [ ] Load-bearing current facts were preserved, routed, or deleted only with current proof.
- [ ] Type-standard openings follow the required route-away and produced-structure order.

[PROOF_CLOSE]:
- [ ] Local gates and provider claims use the correct proof source class.
- [ ] Unrun gates and unsupported renderer, provider, link, anchor, or docs-build claims are reported as proof gaps.
- [ ] The edited standard closes on boundaries, proof, validation, or the next safe route, not on a negative pattern list.
