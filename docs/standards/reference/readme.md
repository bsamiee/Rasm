# [README_STANDARDS]

A README is an entry document for one boundary: repository, package, directory, documentation corpus, tool, or local service. It orients the reader, gives the shortest route to one useful next action, and links every deeper concern to the document that carries it. A README is the front door and local index, not an architecture page, API catalog, tutorial, runbook, roadmap, ADR, or contribution workflow.

## [1][USE_WHEN]

Use a README to open exactly one boundary:
- repository root for users, adopters, and contributors.
- package, library, or directory for local purpose and public entrypoints.
- documentation corpus for routing a reader to the next page.
- tool, command, or local service for the first successful invocation.

Route architecture structure, API or symbol catalogs, learning paths, operational recovery, future-work sequences, durable decisions, and contribution workflow to their controlling standards. Route agent-only read order, behavioral overlays, validation ladders, forbidden patterns, and provider-loading rules to `AGENTS.md`. A README may link an instruction file when it changes the reader's route, but it must not copy the instruction body. Keep the README to orientation, first path, action-changing status, and routing.

Authoring contract:
- Agent use: identify the one boundary the README opens, choose the matching profile, and keep the page to first success plus maintained routing.
- Required produced structure: H1, one-paragraph description, profile-required first-path or routing sections, optional status/support facts only when they change action, and routed documentation links.
- Section cardinality: one H1 and one description; one primary profile; required profile sections appear once; conditional sections appear only when the boundary consumes them.
- Adjacent checks: check architecture, API, reference, support matrix, tutorial, how-to, runbook, roadmap, contributing, and code documentation only when README status, first path, entrypoints, or routes change.
- Maintenance triggers: update the README when display name, boundary, first command, verification signal, status, support fact, entrypoint, side effect, license, child page, or maintained route changes.

## [2][AUTHORING_CONTRACT]

README files carry four useful facts: boundary, first successful action, action-changing status, and routes. Do not add a section unless it changes one of those facts.

- Agent use: identify the one boundary the README opens, choose the matching profile, and route deeper bodies to their routes.
- Produced structure: title, description, profile-required first path or routing sections, status/support facts only when they change action, and maintained document routes.
- Section cardinality: one title, one description, one primary profile, required profile sections once, conditional sections only when triggered.
- Adjacent checks: check `AGENTS.md`, architecture, API, reference, support matrix, tutorial, how-to, runbook, roadmap, contributing, and code documentation only when README status, first path, entrypoints, or routes change.
- Maintenance triggers: display name, boundary, first command, verification signal, status, support fact, entrypoint, side effect, license, child page, or maintained route changes.

## [3][README_BASELINES]

README conventions come from platform rendering, open-source README baselines, badge providers, and local routing policy. Use the source that carries the question.

Use GitHub README documentation for platform-visible rendering, discovery, and recognized file placement. Use the standard-readme specification and Make a README as background baselines narrowed by local route routing. Use Shields.io documentation for badge rendering, not status truth. Repository source, manifests, lockfiles, generated contracts, runnable command output, and source documents outrank generic README advice.

When a README would restate a fact another document carries, link the route and avoid a second copy. Local Rasm routing intentionally keeps API catalogs, contribution workflow bodies, roadmap sequences, runbook procedures, and architecture rationale out of README files even when a generic README baseline lists adjacent sections.

README heading modes are:

| [INDEX] | [SURFACE]                                  | [HEADING_MODE]  | [RULE]                                                 |
| :-----: | :----------------------------------------- | :-------------- | :----------------------------------------------------- |
|   [1]   | public or registry surface                 | plain H1/H2     | use names such as `# Rasm`, `## Install`, `## Usage`   |
|   [2]   | repo-internal standards-controlled surface | bracketed H1/H2 | use `# [PACKAGE_OR_DIRECTORY_NAME]`, not `# [H1][...]` |
|   [3]   | mixed mode                                 | rejected        | choose one mode and migrate when touched               |

## [4][PROFILES]

Choose one primary profile by the boundary the README opens, not by the directory where it sits.

| [INDEX] | [PROFILE]     | [READER]                    | [BOUNDARY]      | [SUCCESS_SIGNAL]                       |
| :-----: | :------------ | :-------------------------- | :-------------- | :------------------------------------- |
|   [1]   | Root entry    | user or contributor         | repository      | first run plus route-doc route         |
|   [2]   | Package entry | package consumer or editor  | package         | purpose, status, and entrypoints       |
|   [3]   | Hub index     | corpus navigator            | docs corpus     | correct child page on first read       |
|   [4]   | Tool entry    | command or service operator | tool or service | first command runs and verifies effect |

Name the profile in the opening paragraph when the file could plausibly read as another profile.

## [5][README_CONTRACT]

Every README obeys one shared contract:
- Name the file `README.md` unless a hosting platform or localization standard requires a documented variant.
- Use an H1 that matches the repository, package, directory, tool, or corpus display name.
- Use plain public README section headings such as `Install`, `Usage`, and `License` when a hosting platform, package registry, or public convention exposes headings to readers. Use the bracketed heading idiom only for repo-internal documentation where this corpus controls the renderer and convention. When a repo-internal README uses bracketed headings, it follows [formatting.md](../formatting.md): the H1 is `# [PACKAGE_OR_DIRECTORY_NAME]`, not `# [H1][...]`.
- Place a one-paragraph description directly after the title, and after any badge or banner, before any other section.
- Use relative links for repository-local targets so the README renders on the hosting platform and in a local checkout.
- Show advertised install, setup, usage, and verification commands exactly as a reader runs them in fenced, intent-labeled code blocks.
- Mark an unverified advertised command as not run in the current change rather than implying it ran; [proof.md](../proof.md) carries the exact evidence details when a claim needs them.

Field cardinality: file name, H1, and description are required and singular. Section-heading convention is required and singular: choose public plain headings or repo-internal bracketed headings, not both. Badges and banners are optional and repeatable. Relative links are required for every local link.

## [6][REQUIRED_STRUCTURE]

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

Root entry
    Required when installable or runnable: `Install`, `Usage`.
    Required when source documents exist: `Documentation`.
    Required when published: `License`, placed last.
    Conditional: `Status`, `Support`, `Security`, `Contributing`.

Package entry
    Required: `Status`, `Entrypoints`.
    Conditional: `Usage`, `Documents`, `Constraints`, `License`.
    Omit: repository-wide contribution, security, support, or roadmap bodies unless the package carries a distinct local policy.

Hub index
    Required: `Choose`, `Pages`.
    Conditional: child-set maintenance note when the page list changes outside normal review.
    Omit: install, usage, support, and command sections unless the corpus itself carries that first action.

Tool entry
    Required: `Requirements`, `First command`, `Verify`.
    Conditional: `Command reference link`, `Output`, `Troubleshooting`, `Support route`.
    Omit: full command catalogs and operational recovery; route them to reference or runbook routes.

README section cardinality uses these fields:
- Title and description: required, single for every profile.
- `Status`: conditional for root entry when pre-release, experimental, or under a support phase; required for package entry; omitted for hub index and tool entry unless status itself changes reader action.
- `Install`: required for root entries a reader installs or runs.
- `Usage`: required when the root or package can produce one observable result; conditional for package entries directly runnable from their directory.
- `Documentation`: required when source documents exist.
- `License`: required for published repositories and placed last.
- `Entrypoints`: required for package entries.
- `Documents`: optional, repeatable for package source documents.
- `Constraints`: conditional only when a reader-facing assumption would make first use unsafe if omitted; each retained constraint names `Constraint`, `Reader risk`, `Source`, and `Update when`.
- `Choose`: required for hub indexes.
- `Pages`: required and repeatable for hub indexes.
- Child-set maintenance note: conditional for hub indexes whose child set changes outside normal review.
- `Requirements`, `First command`, and `Verify`: required for tool entries.
- `Command reference link`: optional for first-run-adjacent command families only; full command inventories route to [reference.md](reference.md) or [api.md](api.md).
- `Output`: conditional when the tool writes files, captures, external state, or durable artifacts.
- `Troubleshooting`: optional for first-run setup failures only; multi-symptom operational recovery routes to [runbook.md](../task/runbook.md).

Package entry field order is a README-local exception because package readers decide from status and entrypoints first: `Status`, `Entrypoints`, `First path` or `First command` when runnable, `Verify` when a first path exists, `Documents`, `Constraints`, `Update when`, `Route-away`.

A tool README may show the first command, one verification command, and side effects needed to interpret first success. CLI inventories, flags, envelope fields, status exits, command maps, generated command reference, maintenance gates, and recovery procedures route to API, reference, architecture, runbook, or `AGENTS.md` routes.

## [7][BADGES_BANNERS]

Badges are status claims, not decoration. Include a badge only when it reflects maintained automation, a release, security posture, or package fact that a reader can verify.

- Keep the lead block to two to four high-value badges such as build, version, coverage, and license.
- Link each badge to the service that produced the status.
- Use static badges only for stable labels such as license or documentation availability. Use dynamic badges driven by maintained CI, registry, coverage, or security services for changing facts.
- Remove retired or unverifiable badges.
- Use a table only when badge-backed facts are part of a real status comparison; do not use tables for badge layout.

A badge is a support or status claim and inherits the proof obligation of the service that produces it.

Banners are identity or orientation media, not proof. Include a banner only when the hosting surface renders local images reliably, the image has useful alt text, and the README still opens with a visible text description. Do not use a banner as the only carrier of product name, status, support, or call-to-action content.

## [8][CONTENT_REQUIREMENTS]

A README must carry the facts its profile needs and no deeper route's body.

- Boundary: name what the README opens and the nearest concern it excludes in the first paragraph.
- First path: for root, package, and tool profiles that advertise commands, provide the shortest path to one observable result and proof status for each command.
- Hub route: for hub indexes, route by reader need and keep child links current; do not invent install or verification commands.
- Route map: include one link per maintained document for deeper concerns the README only summarizes.
- Status: source package or support claims to a manifest, lockfile, generated contract, support matrix, or current tool output.
- License: name the SPDX identifier and route for any published repository.
- Tool side effects: name output, artifact, or external-state locations and the verification signal.

When the README's first path, status, constraints, entrypoints, or maintained-document links change, check the routed documents and update only the controlling facts that changed. A README route change does not justify copying architecture, support, runbook, API, roadmap, tutorial, contribution, or agent-instruction bodies into the README.

Use this surface-split table:

| [INDEX] | [SURFACE]                    | [OWNS]                                                                                        | [README_ROUTE]                                        |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------------------- | :---------------------------------------------------- |
|   [1]   | `README.md`                  | boundary, first successful action, status that changes action, entrypoints, document routes   | link deeper route only when the reader must choose it |
|   [2]   | `AGENTS.md`                  | agent read order, local invariants, forbidden patterns, validation gates, conflict precedence | `Agent editing rules` route only                      |
|   [3]   | `_ARCHITECTURE.md`           | code shape, codemaps, package matrices, invariants                                            | route structure/detail away                           |
|   [4]   | API/reference/code docs      | command, symbol, field, lookup, and generated-reference catalogs                              | route catalogs away                                   |
|   [5]   | runbook/contributing/roadmap | recovery, PR workflow, sequence, blockers, deferred work                                      | route procedures and planning away                    |

README files do not use invocation markers such as `[CRITICAL]`, `[ALWAYS]`, or `[NEVER]`; those belong in instruction surfaces.

## [9][STRUCTURE_RULES]

Use structure where it improves scanning:
- Status, maturity, and support: status-tagged record with `Surface`, `Status`, `Reader action`, `Exit`, `Evidence` or `Source of truth`, and `Review trigger` when the claim can drift. `Phase N`, `in progress`, and similar progress labels are invalid README status unless the row names an exit condition and routes implementation sequence to roadmap.
- Entrypoints, import surfaces, and first-run command cards: lookup table or definition block, never prose that hides a key-to-effect map. Entrypoints are import surfaces, commands, executable files, public type families, or scope-local API names a reader can use or edit first; adjacent files are `Documents`, not entrypoints.
- Documentation and child maps: definition block under eight entries; table only when entries carry comparable columns such as route or status.
- First-run troubleshooting: decision table only when more than one setup symptom maps to a distinct response; otherwise use one short paragraph.
- Commands: fenced blocks with an info string and intent label.
- License: closing section for published repositories.

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
Documents: `<adjacent document links>`
Constraints: `<reader-facing assumptions only>`
Update when: `<status, entrypoint, first path, verification, document route, or constraint changes>`
Route-away: `<deep body content that stays out of the README>`
```

The card does not replace `Architecture`, `Roadmap`, `Support`, or task documents. It gives the README one dense path into those documents without copying their bodies.

```text conceptual
Entrypoints: `uv run python -m tools.quality`
First command: `uv run python -m tools.quality api doctor`
Verify: command emits one JSON Envelope and does not launch Rhino.
Route-away: full command catalog, envelope fields, status exits, rail architecture, and operational recovery stay in reference, API, architecture, and runbook documents.
```

The card above is valid because the fields decide first use. Instruction files may appear in a README map only as agent instruction targets; do not list `AGENTS.md` as a package, corpus, or tool entrypoint. The rejected shape below creates one card per link and turns routing into low-value field spam:

```text rejected
Need: current structure
Open: `ARCHITECTURE.md`

Need: future sequence
Open: `ROADMAP.md`
```

Reject this multi-route card because it mixes command, API, support, and architecture bodies:

```text rejected
Changed fact: README links changed.
Consumed by: architecture, roadmap, runbook, API, support, tutorial.
Use in this document: everything should be updated.
```

## [10][EXAMPLES]

A hub index example teaches routing by reader need rather than a flat link dump:

```markdown conceptual
# Documentation

This corpus routes readers to the document type that carries their current need.

## [1][CHOOSE]

Read by what you are doing, then open one child.

## [2][PAGES]

[architecture.md](../explanation/architecture.md): current structure, boundaries, and invariants.
[runbook.md](../task/runbook.md): symptom-to-fix response when a service degrades.
[support-matrix.md](support-matrix.md): supported versions, platforms, and runtimes.
```

The example is conceptual because the child set does not need a separate maintenance note. It keeps one link per child and gives each child a reason that separates it from siblings.

Use this local hub-index pattern:

```markdown conceptual
## [1][CHOOSE]

| [INDEX] | [READ_FOR]                                                | [OPEN]                          |
| :-----: | :-------------------------------------------------------- | :------------------------------ |
|   [1]   | BCL, package, and host-reference posture                  | `docs/system-api-map/README.md` |
|   [2]   | external product-library facts and local adoption posture | `docs/external-libs/README.md`  |
|   [3]   | test-tool API facts and local rail routing                | `docs/testing-libs/README.md`   |
```

The row labels are reader questions, not just filenames. File-only lists route poorly when the corpus mixes API, reference, support, and strategy facts.

Reject this child prose because it duplicates the child page:

```markdown rejected
The architecture document covers lots of helpful context about the overall
structure, boundaries, invariants, and related details.
```

The rejected shape buries the route and duplicates the child page's content.

Use this root first-path example:

````markdown conceptual
# Rasm

Rasm provides Rhino/GH2-aware geometry libraries and local quality rails; architecture, support, and contribution workflow live in source documents linked below.

## Install

```bash copy-safe
uv sync
```


## Usage

```bash copy-safe
uv run python -m tools.quality api doctor
```

Verify: command prints one JSON `Envelope` and does not launch Rhino.

## Documentation

Need: inspect local quality/API rail behavior.
Go to: `tools/quality/README.md`
Use when: the first command succeeds and the reader needs the command family or Envelope contract.
Route-away: command catalog, Envelope fields, recovery, and support policy stay in tool/API/reference/runbook routes.

Need: verify host-reference or package source truth.
Go to: `docs/system-api-map/README.md`
Use when: code touches `System.*`, host references, package state, or replacement APIs.
Route-away: BCL/package tables and host API lookup stay in system-api-map routes.
````

The example is conceptual: it shows the minimum root README path with command proof and route routing, not a universal public section set.

Root README correction: a root `## Commands` section that copies static, bridge, package, deploy, publish, and verify verbs is rejected. The root may show one first command and one verification signal; command inventories belong to API/reference routes, recovery to runbook, and execution rules to `AGENTS.md`.

## [11][BOUNDARIES]

- [README.md](../README.md) carries document-type routing, placement, splitting, and lifecycle.
- [AGENTS.md](../AGENTS.md) carries agent read order, behavioral overlays, forbidden patterns, and validation gates.
- [architecture.md](../explanation/architecture.md) carries current structure and invariants.
- [api.md](api.md) carries generated endpoint and symbol catalogs.
- [reference.md](reference.md) carries fact lookup leaves.
- [tutorial.md](../learning/tutorial.md) carries learning paths.
- [runbook.md](../task/runbook.md) carries operational recovery.
- [roadmap.md](../explanation/roadmap.md) carries future-work sequence and milestone exit proof.
- [contributing.md](../task/contributing.md) carries contribution workflow and pull-request evidence.

## [12][CHECKLIST]

Use this checklist by group:

Profile and shape:
- [ ] Exactly one profile is primary, and the opening paragraph names it where ambiguity exists.
- [ ] Source-route deviations from generic README baselines are intentional and route deeper bodies to source documents.
- [ ] The H1 matches the display name unless a repo-internal standard intentionally controls the heading idiom.
- [ ] Public or registry-visible README files use plain section headings; repo-internal README files use bracketed headings only where local convention controls.
- [ ] The description sits directly after the title or badge block.
- [ ] Required sections for the chosen profile are present; conditional sections appear only when triggered.
- [ ] No optional or conditional heading appears empty.

Routes and content:
- [ ] Runnable proof applies only to profiles and sections that advertise runnable commands.
- [ ] Hub indexes with changing child sets route by reader need and keep child links current.
- [ ] Status and support claims use records with claim-level proof delegated to [proof.md](../proof.md).
- [ ] Entrypoints and command maps use lookup tables or definition blocks.
- [ ] Route cards and package or tool entry cards appear only when they make maintained routes denser than prose.
- [ ] Tool README files contain no command inventory beyond the first path; deeper command, API, runbook, architecture, and maintenance bodies are linked to their routes.
- [ ] Package README status rows name reader action, exit, evidence or source truth, and review trigger.

Proof and closeout:
- [ ] Troubleshooting stays limited to first-run setup; operational recovery links to a runbook.
- [ ] Badges are linked, maintained, high-value status claims, and banners are not the only carrier of meaning.
- [ ] Every deeper concern links to its route instead of duplicating route content.
- [ ] License closes published repository README files.
- [ ] Every relative link resolves from the README's directory.
