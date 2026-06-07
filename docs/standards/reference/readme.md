# [README_STANDARDS]

A README is an entry document for one boundary: repository, package, directory, documentation corpus, tool, or local service. It orients the reader, gives the shortest route to one useful next action, surfaces the high-signal operating facts needed to use or route the boundary safely, and links every exhaustive concern to the document that carries it. A README is a focused index and operating surface, not an exhaustive architecture page, API catalog, generated CLI help dump, tutorial, runbook, roadmap, ADR, or contribution workflow.

## [1]-[USE_WHEN]

Use a README to open exactly one boundary:
- repository root for users, adopters, and contributors.
- package, library, or directory for local purpose and public entrypoints.
- documentation corpus for routing a reader to the next page.
- tool, command, or local service for the first successful invocation and curated operating surface.

Route exhaustive architecture structure, generated API or symbol catalogs, complete flag help, learning paths, operational recovery, future-work sequences, durable decisions, and contribution workflow to their controlling standards. Route agent-only read order, behavioral overlays, validation ladders, forbidden patterns, and provider-loading rules to `AGENTS.md`. A README may link an instruction file when it changes the reader's route, but it must not copy the instruction body. Keep the README to orientation, first path, action-changing status, curated operating facts, and routing.

## [2]-[AUTHORING_CONTRACT]

README files carry useful operating facts at summary depth: boundary, first successful action, action-changing status, curated commands or entrypoints, integration and machine-contract facts that change use, and routes. Do not add a section unless it changes one of those facts.

- Agent use: identify the one boundary the README opens, choose the matching profile, and route deeper bodies to their routes.
- Produced structure: title, description, profile-required first path or routing sections, status/support facts only when they change action, curated operating sections when triggered, and maintained document routes.
- Section cardinality: one title, one description, one primary profile, required profile sections once, conditional sections only when triggered.
- Adjacent checks: check `AGENTS.md`, architecture, API, reference, support matrix, tutorial, how-to, runbook, roadmap, contributing, and code documentation only when README status, first path, entrypoints, or routes change.
- Maintenance triggers: display name, boundary, first command, verification signal, status, support fact, entrypoint, curated command surface, side effect, machine contract, integration, license, child page, or maintained route changes.

## [3]-[README_BASELINES]

README conventions come from platform rendering, local routing policy, and repository evidence. Use the maintained source that carries the question.

Use platform-visible rendering, discovery, and recognized file placement only when a maintained route or current platform behavior proves it. Repository source, manifests, lockfiles, generated contracts, runnable command output, and source documents outrank generic README advice.

When a README would restate a fact another document carries, link the route and avoid a second copy. Local routing policy keeps API catalogs, contribution workflow bodies, roadmap sequences, runbook procedures, and architecture rationale out of README files even when a generic README baseline lists adjacent sections.

README heading modes are:

| [INDEX] | [SURFACE]                                  | [HEADING_MODE]  | [RULE]                                                                    |
| :-----: | :----------------------------------------- | :-------------- | :------------------------------------------------------------------------ |
|   [1]   | public or registry surface                 | plain H1/H2     | use names such as `# <Repository display name>`, `## Install`, `## Usage` |
|   [2]   | repo-internal standards-controlled surface | bracketed H1/H2 | use `# [PACKAGE_OR_DIRECTORY_NAME]`, not `# [H1][...]`                    |
|   [3]   | mixed mode                                 | rejected        | choose one mode and migrate when touched                                  |

Rejected repo-internal H1:

```markdown rejected
# [H1][TOOL_OPERATOR]
```

Use the boundary name directly:

```markdown conceptual
# [TOOL_OPERATOR]
```

## [4]-[PROFILES]

Choose one primary profile by the boundary the README opens, not by the directory where it sits.

| [INDEX] | [PROFILE]     | [READER]                    | [BOUNDARY]      | [SUCCESS_SIGNAL]                       |
| :-----: | :------------ | :-------------------------- | :-------------- | :------------------------------------- |
|   [1]   | Root entry    | user or contributor         | repository      | first run plus route-doc route         |
|   [2]   | Package entry | package consumer or editor  | package         | purpose, status, and entrypoints       |
|   [3]   | Hub index     | corpus navigator            | docs corpus     | correct child page on first read       |
|   [4]   | Tool entry    | command or service operator | tool or service | first command runs and verifies effect |

Name the profile in the opening paragraph when the file could plausibly read as another profile.

## [5]-[README_CONTRACT]

Every README obeys one shared contract:
- Name the file `README.md` unless a hosting platform or localization standard requires a documented variant.
- Use an H1 that matches the repository, package, directory, tool, or corpus display name.
- Use plain public README section headings such as `Install`, `Usage`, and `License` when a hosting platform, package registry, or public convention exposes headings to readers. Use the bracketed heading idiom only for repo-internal documentation where this corpus controls the renderer and convention. When a repo-internal README uses bracketed headings, it follows [formatting.md](../formatting.md): the H1 is `# [PACKAGE_OR_DIRECTORY_NAME]`, not `# [H1][...]`.
- Place a one-paragraph description directly after the title, and after any badge or banner, before any other section.
- Use relative links for repository-local targets so the README renders on the hosting platform and in a local checkout.
- Show advertised install, setup, usage, and verification commands exactly as a reader runs them in fenced, intent-labeled code blocks.
- Mark an unverified advertised command as not run in the current change rather than implying it ran; [proof.md](../proof.md) carries the exact evidence details when a claim needs them.

Field cardinality: file name, H1, and description are required and singular. Section-heading convention is required and singular: choose public plain headings or repo-internal bracketed headings, not both. Badges and banners are optional and repeatable. Relative links are required for every local link.

## [6]-[REQUIRED_STRUCTURE]

Use the core template for the chosen profile, then add only the conditional sections that the profile triggers. Public and registry-visible README files use plain section headings; repo-internal README files may translate the same section names into the bracketed heading idiom only when local standards control the renderer.

Use this root-entry template:

```markdown template
# <Repository display name>

<One-paragraph description naming purpose and boundary.>

## Install

## Usage

## Documentation
```

Use this package-entry template:

```markdown template
# <Package display name>

<One-paragraph purpose and boundary.>

## Status

## Entrypoints
```

Use this hub-index template:

```markdown template
# <Corpus display name>

<One-paragraph corpus purpose and boundary.>

## Choose

## Pages
```

Use this tool-entry template:

```markdown template
# <Tool display name>

<One-paragraph tool purpose and boundary.>

## Requirements

## First command

## Verify
```

Use these profile additions only when the chosen profile triggers them:

[ROOT_ENTRY]:
- Required when installable or runnable: `Install`, `Usage`.
- Required when source documents exist: `Documentation`.
- Required when published: `License`, placed last.
- Conditional: `Status`, `Support`, `Security`, `Contributing`.

[PACKAGE_ENTRY]:
- Required: `Status`, `Entrypoints`.
- Conditional: `Usage`, `Documents`, `Constraints`, `License`.
- Omit: repository-wide contribution, security, support, or roadmap bodies unless the package carries a distinct local policy.

[HUB_INDEX]:
- Required: `Choose`, `Pages`.
- Conditional: child-set maintenance note when the page list changes outside normal review.
- Omit: install, usage, support, and command sections unless the corpus itself carries that first action.

[TOOL_ENTRY]:
- Required: `Requirements`, `First command`, `Verify`.
- Conditional: `Curated command surface`, `Machine contract`, `Integrations`, `Diagram`, `Replacement/adoption`, `Command reference link`, `Output`, `Troubleshooting`, `Support route`.
- Omit: exhaustive generated command catalogs, full schemas, source signatures, status algebra, and operational recovery; route them to reference, API, architecture, source, or runbook routes.

README section cardinality uses these groups:

[UNIVERSAL_ROOT]:
- Title and description: required, single for every profile.
- `Status`: conditional for root entry when pre-release, experimental, or under a support phase; required for package entry; omitted for hub index and tool entry unless status itself changes reader action.
- `Install`: required for root entries a reader installs or runs.
- `Usage`: required when the root or package can produce one observable result; conditional for package entries directly runnable from their directory.
- `Documentation`: required when source documents exist.
- `License`: required for published repositories and placed last.

[PACKAGE_HUB]:
- `Entrypoints`: required for package entries.
- `Documents`: optional, repeatable for package source documents.
- `Constraints`: conditional only when a reader-facing assumption would make first use unsafe if omitted; each retained constraint names `Constraint`, `Reader risk`, `Source`, and `Update when`.
- `Choose`: required for hub indexes.
- `Pages`: required and repeatable for hub indexes.
- Child-set maintenance note: conditional for hub indexes whose child set changes outside normal review.

[TOOL_FIRST_PATH]:
- `Requirements`, `First command`, and `Verify`: required for tool entries.
- `Curated command surface`: conditional for multi-verb CLIs, local services with multiple operator modes, replacement tools, or tools whose commands are the reader-facing product surface. Include command families and action-changing flags at summary depth; route exhaustive help and every flag variant to [reference.md](reference.md), [api.md](api.md), generated help, or source.
- `Machine contract`: conditional when stdout, stderr, exit codes, durable artifacts, or external state are consumed by agents, CI, automation, or another tool. Summarize the stable shape and one verification signal; route field-by-field schema, status algebra, and typed variants away.
- `Integrations`: conditional when external tools, package managers, env vars, services, filesystem backends, telemetry, runtime hosts, or automation hooks change operation. Name what the integration enables and the reader action it changes; route full configuration matrices away.
- `Diagram`: conditional when a Mermaid or image diagram explains command flow, boundary crossing, lifecycle, ownership, or machine contract faster than prose. Diagrams are encouraged when they answer one reader question, not mandatory decoration.
- Architecture diagram fallback: allowed only when no sibling `ARCHITECTURE.md` exists and the README opens a small scope whose current structure fits summary depth. Promote the diagram to `ARCHITECTURE.md` when the scope gains multiple entrypoints, generated contracts, dependency rules, nontrivial flow, or roadmap-status overlays.
- `Replacement/adoption`: conditional when a README covers a tool, alias, package, or operator replacing another surface, especially before repo policy has fully migrated. Name `Replaces`, `Current policy`, `Use now`, `Migration blocker`, `Update when`, and `Route deeper`.
- `Command reference link`: optional for generated or exhaustive command families after the curated README surface.
- `Output`: conditional when the tool writes files, captures, external state, durable artifacts, or machine-readable streams.
- `Troubleshooting`: optional for first-run setup failures only; multi-symptom operational recovery routes to [runbook.md](../task/runbook.md).

Package entry field order is a README-local exception because package readers decide from status and entrypoints first: `Status`, `Entrypoints`, `First path` or `First command` when runnable, `Verify` when a first path exists, `Documents`, `Constraints`, `Update when`, `Route-away`.

A tool README may show the first command, one verification command, side effects needed to interpret first success, and a curated command or integration surface when those facts are the operator-facing product. Full generated command help, exhaustive flags, complete envelope field catalogs, status algebra, source signatures, maintenance gates, and recovery procedures route to API, reference, architecture, runbook, source, or `AGENTS.md` routes.

## [7]-[BADGES_BANNERS]

Badges are status claims, not decoration. Include a badge only when it reflects maintained automation, a release, security posture, or package fact that a reader can verify.

- Keep the lead block to two to four high-value badges such as build, version, coverage, and license.
- Link each badge to the service that produced the status.
- Use static badges only for stable labels such as license or documentation availability. Use dynamic badges driven by maintained CI, registry, coverage, or security services for changing facts.
- Remove retired or unverifiable badges.
- Use a table only when badge-backed facts are part of a real status comparison; do not use tables for badge layout.

A badge is a support or status claim and inherits the proof obligation of the service that produces it.

Banners are identity or orientation media, not proof. Include a banner only when the hosting surface renders local images reliably, the image has useful alt text, and the README still opens with a visible text description. Do not use a banner as the only carrier of product name, status, support, or call-to-action content.

## [8]-[CONTENT_REQUIREMENTS]

A README must carry the facts its profile needs and no deeper route's body.

- Boundary: name what the README opens and the nearest concern it excludes in the first paragraph.
- First path: for root, package, and tool profiles that advertise commands, provide the shortest path to one observable result and proof status for each command.
- Hub route: for hub indexes, route by reader need and keep child links current; do not invent install or verification commands.
- Route map: include one link per maintained document for deeper concerns the README only summarizes.
- Status: source package or support claims to a manifest, lockfile, generated contract, support matrix, or current tool output.
- Curated operating surface: when the boundary is a multi-command tool or service, include the command families, flags, integrations, and caveats that change first-use or safe operation. Keep this at summary depth; do not paste generated help.
- Machine contract: when output is consumed by agents, CI, automation, or tools, summarize stdout, stderr, exit behavior, durable artifacts, and the verification signal.
- Diagrams: include diagrams only when they clarify flow, ownership, lifecycle, or machine contract; provide nearby text equivalent and keep them render-validated.
- Architecture fallback: if no sibling `ARCHITECTURE.md` exists, a small-scope README may include a compact current codemap, flow, or ownership diagram; route planned structure and task state to `.planning/` or roadmap rather than the README.
- License: name the SPDX identifier and route for any published repository.
- Tool side effects: name output, artifact, or external-state locations and the verification signal.

When the README's first path, status, constraints, entrypoints, curated command surface, integration facts, machine contract, diagram, or maintained-document links change, check the routed documents and update only the controlling facts that changed. A README route change does not justify copying exhaustive architecture, support, runbook, API, roadmap, tutorial, contribution, generated help, or agent-instruction bodies into the README.

Use this surface-split selector. Rows name ownership routes, not inventories:

| [INDEX] | [SURFACE]                    | [README_KEEPS]                       | [ROUTE_AWAY]                          |
| :-----: | :--------------------------- | :----------------------------------- | :------------------------------------ |
|   [1]   | `README.md`                  | boundary, first path, status, routes | exhaustive catalogs and workflows     |
|   [2]   | `AGENTS.md`                  | agent route link                     | read order, invariants, gates         |
|   [3]   | `ARCHITECTURE.md`            | current-structure link               | codemaps, matrices, invariants        |
|   [4]   | API/reference/code docs      | generated or lookup route link       | commands, schemas, symbols, fields    |
|   [5]   | runbook/contributing/roadmap | recovery, PR, or sequence route link | workflows, active work, terminal work |

README files do not use invocation markers such as `[CRITICAL]`, `[ALWAYS]`, or `[NEVER]`; those belong in instruction surfaces.

## [9]-[STRUCTURE_RULES]

Use structure where it improves scanning:
- Status, maturity, and support: status-tagged record with `Surface`, `Status`, `Reader action`, `Exit`, `Evidence` or `Controlling source`, and `Review trigger` when the claim can drift. `Phase N`, `in progress`, and similar progress labels are invalid README status unless the row names an exit condition and routes implementation sequence to roadmap.
- Entrypoints, import surfaces, and first-run command cards: lookup table or GroupedRecord, never prose that hides a key-to-effect map. Entrypoints are import surfaces, commands, executable files, public type families, or scope-local API names a reader can use or edit first; adjacent files are `Documents`, not entrypoints.
- README tables: table use is conditional, not the default. Before a README table, name the warrant in prose: either `This table is a lookup by <key>` or `This table compares <row set> across <attributes>`. If no warrant can be stated plainly, use a GroupedRecord or a package/tool card.
- Curated command surfaces: start with a compact lookup table only when the family-to-verbs map is the reader question. Put flags, examples, caveats, mutation behavior, and semantic notes in GroupedRecord command clusters or H3 command cards; route exhaustive generated help away.
- Machine contracts: use GroupedRecord clusters for stdout, stderr, exit behavior, artifacts, and automation streams unless the values are short enough for a real lookup table. Keep one compact example or verification signal; route full schemas and status algebra away.
- Integrations: use GroupedRecord clusters keyed by toolchain, capability backend, runtime backend, service, or environment surface. Use a table only when the integration rows share short comparable attributes; dependency essays and full configuration matrices route away.
- Diagrams: Mermaid is allowed when it answers a route, flow, ownership, lifecycle, or machine-contract question. It must have a nearby text equivalent and render proof when touched.
- Replacement/adoption: status record or package/tool card naming `Replaces`, `Current policy`, `Use now`, `Migration blocker`, `Update when`, and `Route deeper`. Use ordinary README fields, not invocation markers.
- Documentation and child maps: GroupedRecord under eight entries; table only when entries carry comparable columns such as route or status.
- First-run troubleshooting: decision table only when more than one setup symptom maps to a distinct response; otherwise use one short paragraph.
- Commands: fenced blocks with an info string and intent label.
- License: closing section for published repositories.

README-local `Status` values are allowed only when they are declared before the first status row or package/tool card. The declaration names casing, active values, blocked values, terminal values, omitted shared lifecycle states, removal behavior, and source/proof field use. If a README consumes roadmap lifecycle, support status, package maturity, or tool health, keep those as separate record families rather than mixing the terms in one `Status` column.

Troubleshooting in a README is limited to reaching the first successful path. Ongoing incidents, triage, rollback, and recovery belong to a runbook.

Use a `Documents` block as the default package link map. Use fields `Document`, `Carries`, `Read when`, and `Route-away`, one entry per adjacent route. Use a reader-route card when a route helps a reader choose where to go:

```text template
Need: `<reader decision or first-use question>`
Go to: `<source document or section>`
Use when: `<condition that makes this route relevant>`
Route-away: `<deep body content that stays in the consuming route>`
```

Use the full adjacent-document relation record from [information-structure.md](../information-structure.md) only when a changed README fact requires another maintained document to update. Use multiple consumed-by paths only when the same changed fact updates them together; otherwise use a short `Documents` list.

Use a package or tool entry card when status, entrypoints, and first proof would otherwise scatter across several short sections:

```text template
Status: `<declared local status term and source; omit when status does not change reader action>`
Entrypoints: `<import surface, command, executable, or public type family>`
First command: `<copyable command; omit when the package has no first command>`
Verify: `<observable result, command, or status check; omit when no runnable path exists>`
Machine contract: `<stdout/stderr/exit/artifact summary; omit when not a machine-consumed tool>`
Agent route: `<AGENTS.md route; omit when no local instruction surface changes reader behavior>`
Documents: `<adjacent document links>`
Constraints: `<reader-facing assumptions only>`
Update when: `<status, entrypoint, first path, verification, document route, or constraint changes>`
Route-away: `<deep body content that stays out of the README>`
```

The card may satisfy the profile-required fields for a small package or tool when it is denser than separate headings. It does not replace `Architecture`, `Roadmap`, `Support`, API/reference, or task documents. It gives the README one dense path into those documents without copying their bodies:

```text conceptual
Entrypoints: `<tool invocation>`
First command: `<tool> <health-check-command>`
Verify: command emits the declared machine envelope and does not touch runtime state unless stated.
Route-away: full command catalog, envelope fields, status exits, tool architecture, and operational recovery stay in reference, API, architecture, and runbook documents.
```

The card above is valid because the fields decide first use. Instruction files may appear in a README map only as agent instruction targets; do not list `AGENTS.md` as a package, corpus, or tool entrypoint.

Rejected table stack:

```text rejected
Status table
Migration table
Command table
Output table
Integration table
Artifact table
Source-owner table
```

[TABLE_STACK]:
- Reason: back-to-back README tables turn the page into a reference dump. Keep only tables with a named lookup or comparison warrant, and move heterogeneous rows with caveats into records.

Rejected split cards:

```text rejected
Need: current structure
Open: `ARCHITECTURE.md`

Need: future sequence
Open: `ROADMAP.md`
```

[SPLIT_CARDS]:
- Reason: the rejected shape creates one card per link and turns routing into low-value field spam.

Rejected multi-route card:

```text rejected
Changed fact: README links changed.
Consumed by: architecture, roadmap, runbook, API, support, tutorial.
Use in this document: everything should be updated.
```

[MULTI_ROUTE_CARD]:
- Reason: the rejected card mixes command, API, support, and architecture bodies.

## [10]-[READER_ROUTE_EXAMPLES]

These examples stay together because they show how README profile, heading mode, first-path proof, child routing, and route-away records interact. Move a misuse example beside its controlling rule when it teaches only one rule.

A hub index example teaches routing by reader need rather than a flat link dump:

```markdown conceptual
# Documentation

This corpus routes readers to the document type that carries their current need.

## [1]-[CHOOSE]

Read by what you are doing, then open one child.

## [2]-[PAGES]

[architecture.md](../explanation/architecture.md): current structure, boundaries, and invariants.
[runbook.md](../task/runbook.md): symptom-to-fix response when a service degrades.
[support-matrix.md](support-matrix.md): supported versions, platforms, and runtimes.
```

The example is conceptual because the child set does not need a separate maintenance note. It keeps one link per child and gives each child a reason that separates it from siblings.

Use this hub-index pattern:

```markdown conceptual
## [1]-[CHOOSE]

| [INDEX] | [READ_FOR]          | [OPEN]              |
| :-----: | :------------------ | :------------------ |
|   [1]   | `<reader question>` | `<reference-route>` |
|   [2]   | `<reader question>` | `<reference-route>` |
|   [3]   | `<reader question>` | `<reference-route>` |
```

The row labels are reader questions, not just filenames. File-only lists route poorly when the corpus mixes API, reference, support, and strategy facts.

[CHILD_PROSE]:
- Rejected: The architecture document covers lots of helpful context about the overall structure, boundaries, invariants, and related details.
- Reason: the rejected shape buries the route and duplicates the child page's content.

Use this root first-path example:

````markdown conceptual
# <Repository display name>

<Repository display name> provides one primary product surface; architecture, support, and contribution workflow live in source documents linked below.

## Install

```bash copy-safe
<install command>
```


## Usage

```bash copy-safe
<first-use command>
```

Verify: command prints the declared machine envelope and leaves runtime state unchanged unless stated.

## Documentation

Need: inspect the local tool or API behavior.
Go to: `<tool-reference>`
Use when: the first command succeeds and the reader needs the command family or machine contract.
Route-away: command catalog, machine-contract fields, recovery, and support policy stay in tool/API/reference/runbook routes.

Need: inspect runtime, host, or dependency owner evidence.
Go to: `<runtime-or-dependency-reference>`
Use when: code touches standard-library APIs, host references, package state, or replacement APIs.
Route-away: runtime, package, and host API lookup stay in reference routes.
````

The example is conceptual: it shows the minimum root README path with command proof and route selection, not a universal public section set.

Root README correction: a root `## Commands` section that copies build, runtime, packaging, deployment, publishing, and verification verbs is rejected. The root may show one first command and one verification signal; command inventories belong to API/reference routes, recovery to runbook, and execution rules to `AGENTS.md`.

## [11]-[BOUNDARIES]

[SOURCE_ROUTES]:
- [README.md](../README.md) carries document-type routing, placement, splitting, and lifecycle.
- [AGENTS.md](../AGENTS.md) carries agent read order, behavioral overlays, forbidden patterns, and validation gates.
- [architecture.md](../explanation/architecture.md) carries current structure and invariants.
- [roadmap.md](../explanation/roadmap.md) carries `.planning/` roadmap shape and planning-file boundaries.
- [api.md](api.md) carries generated endpoint and symbol catalogs.
- [reference.md](reference.md) carries fact lookup leaves.

[TASK_LEARNING_ROUTES]:
- [tutorial.md](../learning/tutorial.md) carries learning paths.
- [runbook.md](../task/runbook.md) carries operational recovery.
- [roadmap.md](../explanation/roadmap.md) carries future-work sequence, task identity, and task exit proof.
- [contributing.md](../task/contributing.md) carries contribution workflow and pull-request evidence.

## [12]-[VALIDATION]

Use this verification checklist by group:

[PROFILE_SHAPE]:
- [ ] Exactly one profile is primary, and the opening paragraph names it where ambiguity exists.
- [ ] Source-route deviations from generic README baselines are intentional and route deeper bodies to source documents.
- [ ] The H1 matches the display name unless a repo-internal standard intentionally controls the heading idiom.
- [ ] Public or registry-visible README files use plain section headings; repo-internal README files use bracketed headings only where local convention controls.
- [ ] The description sits directly after the title or badge block.
- [ ] Required sections for the chosen profile are present; conditional sections appear only when triggered.
- [ ] No optional or conditional heading appears empty.

[ROUTES_CONTENT]:
- [ ] Runnable proof applies only to profiles and sections that advertise runnable commands.
- [ ] Hub indexes with changing child sets route by reader need and keep child links current.
- [ ] Status and support claims use records with claim-level proof delegated to [proof.md](../proof.md).
- [ ] Entrypoints and command maps use lookup tables or definition blocks.
- [ ] Multi-operator tool READMEs include a curated command surface for first-use and action-changing command families.
- [ ] Every README table has a stated lookup or comparison warrant before it.
- [ ] README tables have atomic cells, stay bounded, and would lose reader value if converted to definition records.
- [ ] README sections do not stack status, migration, command, output, integration, artifact, and source-owner tables as the default structure.
- [ ] Curated command surfaces do not paste generated help, every flag variant, source signatures, or exhaustive schemas.
- [ ] Machine contracts summarize stdout, stderr, exit behavior, artifacts, and streams when those are consumed by agents, CI, automation, or tools.
- [ ] Integration rows name the reader action changed by each tool, backend, service, env var, or runtime hook.
- [ ] Diagrams answer one route, flow, ownership, lifecycle, or machine-contract question and have nearby text equivalent.
- [ ] README architecture diagrams appear only without a sibling `ARCHITECTURE.md`, stay current-only, and promote when architecture complexity exceeds summary depth.
- [ ] Replacement/adoption records name the retired surface, current policy, use-now route, migration blocker, update trigger, and deeper route.
- [ ] Route cards and package or tool entry cards appear only when they make maintained routes denser than prose.
- [ ] Tool README command coverage stays curated; deeper generated help, API, runbook, architecture, source, and maintenance bodies are linked to their routes.
- [ ] Package README status rows name reader action, exit, evidence or controlling source, and review trigger.

[FIRST_PATH_PROOF]:
- [ ] Troubleshooting stays limited to first-run setup; operational recovery links to a runbook.
- [ ] Badges are linked, maintained, high-value status claims, and banners are not the only carrier of meaning.
- [ ] Every deeper concern links to its route instead of duplicating route content.
- [ ] System Information, prompt text, task state, and local provider context are not promoted into durable README requirements unless repo source, manifests, generated contracts, or current command output prove the fact.
- [ ] License closes published repository README files.
- [ ] Every relative link resolves from the README's directory.
