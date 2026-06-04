# [README_STANDARDS]

A README is an entry document for one boundary: repository, package, directory, documentation corpus, tool, or local service. It orients the reader, gives the shortest route to one useful next action, and links every deeper concern to the document that owns it. A README is the front door and local index, not an architecture page, API catalog, tutorial, runbook, roadmap, ADR, or contribution workflow.

## [1][USE_WHEN]

Use a README to open exactly one boundary:

- repository root for users, adopters, and contributors.
- package, library, or directory for local purpose and public entrypoints.
- documentation corpus for routing a reader to the next page.
- tool, command, or local service for the first successful invocation.

Route architecture structure, API or symbol catalogs, learning paths, operational recovery, future-work sequences, durable decisions, and contribution workflow to their owning standards. Keep the README to orientation, first path, and routing.

## [2][README_BASELINES]

README conventions come from platform rendering, open-source README baselines, badge providers, and local routing policy. Use the source that owns the question.

Use GitHub README documentation for platform-visible rendering, discovery, and recognized file placement. Use the standard-readme specification and Make a README as background baselines narrowed by local owner routing. Use Shields.io documentation for badge rendering, not status truth. Repository source, manifests, lockfiles, generated contracts, runnable command output, and owner documents outrank generic README advice.

When a README would restate a fact another document owns, link the owner and avoid a second copy. Local Rasm routing intentionally keeps API catalogs, contribution workflow bodies, roadmap sequences, runbook procedures, and architecture rationale out of README files even when a generic README baseline lists adjacent sections.

## [3][PROFILES]

Choose one primary profile by the boundary the README opens, not by the directory where it sits.

| [INDEX] | [PROFILE]     | [READER]                    | [BOUNDARY]      | [SUCCESS_SIGNAL]                       |
| :-----: | :------------ | :-------------------------- | :-------------- | :------------------------------------- |
|   [1]   | Root entry    | user or contributor         | repository      | first run plus owner-doc route         |
|   [2]   | Package entry | package consumer or editor  | package         | purpose, status, and entrypoints       |
|   [3]   | Hub index     | corpus navigator            | docs corpus     | correct child page on first read       |
|   [4]   | Tool entry    | command or service operator | tool or service | first command runs and verifies effect |

Name the profile in the opening paragraph when the file could plausibly read as another profile.

## [4][README_CONTRACT]

Every README obeys one shared contract:

- Name the file `README.md` unless a hosting platform or localization standard requires a documented variant.
- Use an H1 that matches the repository, package, directory, tool, or corpus display name.
- Use plain public README section headings such as `Install`, `Usage`, and `License` when a hosting platform, package registry, or public convention exposes headings to readers. Use the bracketed heading idiom only for repo-internal documentation where this corpus controls the renderer and convention.
- Place a one-paragraph description directly after the title, and after any badge or banner, before any other section.
- Use relative links for repository-local targets so the README renders on the hosting platform and in a local checkout.
- Show advertised install, setup, usage, and verification commands exactly as a reader runs them in fenced, intent-labeled code blocks.
- Mark an unverified advertised command as not run in the current change rather than implying it ran; [proof.md](../proof.md) owns the exact evidence details when a claim needs them.

Field cardinality: file name, H1, and description are required and singular. Section-heading convention is required and singular: choose public plain headings or repo-internal bracketed headings, not both. Badges and banners are optional and repeatable. Relative links are required for every local link.

## [5][BADGES_BANNERS]

Badges are status claims, not decoration. Include a badge only when it reflects maintained automation, a release, security posture, or package fact that a reader can verify.

- Keep the lead block to two to four high-value badges such as build, version, coverage, and license.
- Link each badge to the service that produced the status.
- Use static badges only for stable labels such as license or documentation availability. Use dynamic badges driven by maintained CI, registry, coverage, or security services for changing facts.
- Remove retired or unverifiable badges.
- Use a table only when badge-backed facts are part of a real status comparison; do not use tables for badge layout.

A badge is a support or status claim and inherits the proof obligation of the service that produces it.

Banners are identity or orientation media, not proof. Include a banner only when the hosting surface renders local images reliably, the image has useful alt text, and the README still opens with a visible text description. Do not use a banner as the only carrier of product name, status, support, or call-to-action content.

## [6][REQUIRED_STRUCTURE]

Use the core template for the chosen profile, then add only the conditional sections that the profile triggers. Public and registry-visible README files use plain section headings; repo-internal README files may translate the same section names into the bracketed heading idiom only when local standards control the renderer.

Root entry:

```markdown template
# <Repository display name>

<One-paragraph description naming purpose and boundary.>

## Install

## Usage

## Documentation
```

Package entry:

```markdown template
# <Package display name>

<One-paragraph purpose and boundary.>

## Status

## Entrypoints
```

Hub index:

```markdown template
# <Corpus display name>

<One-paragraph corpus purpose and boundary.>

## Choose

## Pages
```

Tool entry:

```markdown template
# <Tool display name>

<One-paragraph tool purpose and boundary.>

## Requirements

## First command

## Verify
```

Profile additions:

Root entry
    Required when installable or runnable: `Install`, `Usage`.
    Required when owner documents exist: `Documentation`.
    Required when published: `License`, placed last.
    Conditional: `Status`, `Support`, `Security`, `Contributing`, `Maintainers`, `Owner`.

Package entry
    Required: `Status`, `Entrypoints`.
    Conditional: `Usage`, `Documents`, `Constraints`, `Owner`, `License`.
    Omit: repository-wide contribution, security, support, or roadmap bodies unless the package owns a distinct local policy.

Hub index
    Required: `Choose`, `Pages`.
    Conditional: child-set maintenance note when the page list changes outside normal review.
    Omit: install, usage, support, and command sections unless the corpus itself owns that first action.

Tool entry
    Required: `Requirements`, `First command`, `Verify`.
    Conditional: `Commands`, `Output`, `Troubleshooting`, `Owner`.
    Omit: full command catalogs and operational recovery; route them to reference or runbook owners.

Section cardinality:

- Title and description: required, single for every profile.
- `Status`: conditional for root entry when pre-release, experimental, or under a support phase; required for package entry; omitted for hub index and tool entry unless status itself changes reader action.
- `Install`: required for root entries a reader installs or runs.
- `Usage`: required when the root or package can produce one observable result; conditional for package entries directly runnable from their directory.
- `Documentation`: required when owner documents exist.
- `Support`, `Security`, `Contributing`, `Maintainers`, `Owner`: optional when a durable owner or policy exists.
- `License`: required for published repositories and placed last.
- `Entrypoints`: required for package entries.
- `Documents`: optional, repeatable for package owner documents.
- `Constraints`: conditional when a reader would otherwise assume unsupported capability.
- `Choose`: required for hub indexes.
- `Pages`: required and repeatable for hub indexes.
- Child-set maintenance note: conditional for hub indexes whose child set changes outside normal review.
- `Requirements`, `First command`, and `Verify`: required for tool entries.
- `Commands`: optional for first-run-adjacent commands only; full command inventories route to [reference.md](reference.md) or [api.md](api.md).
- `Output`: conditional when the tool writes files, captures, external state, or durable artifacts.
- `Troubleshooting`: optional for first-run setup failures only; multi-symptom operational recovery routes to [runbook.md](../task/runbook.md).

## [7][CONTENT_REQUIREMENTS]

A README must carry the facts its profile needs and no deeper owner's body.

- Boundary: name what the README opens and the nearest concern it excludes in the first paragraph.
- First path: for root, package, and tool profiles that advertise commands, provide the shortest path to one observable result and proof status for each command.
- Hub route: for hub indexes, route by reader need and keep child links current; do not invent install or verification commands.
- Owner map: include one link per owner document for deeper concerns the README only summarizes.
- Status: source package or support claims to a manifest, lockfile, generated contract, support matrix, or current tool output.
- License: name the SPDX identifier and owner for any published repository.
- Tool side effects: name output, artifact, or external-state locations and the verification signal.

## [8][STRUCTURE_RULES]

Use structure where it improves scanning:

- Status, maturity, and support: status-tagged record with `Status`, an exit or support-phase condition, and a nearby verification note when the claim can drift.
- Entrypoints, import surfaces, and command maps: lookup table or definition block, never prose that hides a key-to-effect map.
- Documentation and child maps: definition block under eight entries; table only when entries carry comparable columns such as owner or status.
- First-run troubleshooting: decision table only when more than one setup symptom maps to a distinct response; otherwise use one short paragraph.
- Commands: fenced blocks with an info string and intent label.
- License: closing section for published repositories.

Troubleshooting in a README is limited to reaching the first successful path. Ongoing incidents, triage, rollback, and recovery belong to a runbook.

Use an owner route card when a README links deeper documents that change reader action. Keep one card per owner boundary, not one card per link:

```text template
Owner: `<package, tool, corpus, or service>`
Why linked: `<one sentence naming the reader decision this route changes>`
Routes: `<manual, architecture, roadmap, support, how-to, tutorial, or other adjacent owner; omit untriggered routes>`
```

Use a package or tool entry card when status, entrypoints, and first proof would otherwise scatter across several short sections:

```text template
Entrypoints: `<import surface, command, executable, or public type family>`
Verify: `<observable result, command, or status check; omit when no runnable path exists>`
Status: `<declared local status term and source; omit when status does not change reader action>`
First command: `<copyable command; omit when the package has no first command>`
```

The card does not replace `Architecture`, `Roadmap`, `Support`, or task documents. It gives the README one dense route into those owners without copying their bodies.

## [9][EXAMPLES]

A hub index example teaches routing by reader need rather than a flat link dump:

```markdown conceptual
# Documentation

This corpus routes readers to the document type that owns their current need.

## [1][CHOOSE]

Read by what you are doing, then open one child.

## [2][PAGES]

[architecture.md](../explanation/architecture.md): current structure, boundaries, and invariants.
[runbook.md](../task/runbook.md): symptom-to-fix response when a service degrades.
[support-matrix.md](support-matrix.md): supported versions, platforms, and runtimes.
```

The example is conceptual because the child set does not need a separate maintenance note. It keeps one link per child and gives each child a role that separates it from siblings.

Rejected child prose:

```markdown rejected
The architecture document covers lots of helpful context about the overall
structure, boundaries, invariants, and related details.
```

The rejected shape buries the route and duplicates the child page's content.

Root first path example:

````markdown conceptual
# Example Project

Example Project provides the local service boundary for reviewed plans; architecture, support, and contribution workflow live in owner documents linked below.

## Install

```bash copy-safe
example install
```


## Usage

```bash copy-safe
example run --plan ./plan.lock
```

Verify: command prints the reviewed plan ID and writes no external state.

## Documentation

Owner: `example service`
Architecture: `docs/architecture.md`
Roadmap: `ROADMAP.md`
Support: `docs/support/example.md`
Why linked: architecture owns service boundaries, roadmap owns unfinished sequence, and support owns supported runtime facts.
````

The example is conceptual: it shows the minimum root README path with command proof and owner routing, not a universal public section set.

## [10][BOUNDARIES]

- [README.md](../README.md) owns document-type routing, placement, splitting, and lifecycle.
- [architecture.md](../explanation/architecture.md) owns current structure and invariants.
- [api.md](api.md) owns generated endpoint and symbol catalogs.
- [reference.md](reference.md) owns fact lookup leaves.
- [tutorial.md](../learning/tutorial.md) owns learning paths.
- [runbook.md](../task/runbook.md) owns operational recovery.
- [roadmap.md](../explanation/roadmap.md) owns future-work sequence and milestone exit proof.
- [contributing.md](../task/contributing.md) owns contribution workflow and pull-request evidence.

## [11][REVIEW_CHECKLIST]

- [ ] Exactly one profile is primary, and the opening paragraph names it where ambiguity exists.
- [ ] Source-role deviations from generic README baselines are intentional and route deeper bodies to owner documents.
- [ ] The H1 matches the display name unless a repo-internal standard intentionally controls the heading idiom.
- [ ] Public or registry-visible README files use plain section headings; repo-internal README files use bracketed headings only where local convention controls.
- [ ] The description sits directly after the title or badge block.
- [ ] Required sections for the chosen profile are present; conditional sections appear only when triggered.
- [ ] No optional or conditional heading appears empty.
- [ ] Runnable proof applies only to profiles and sections that advertise runnable commands.
- [ ] Hub indexes with changing child sets route by reader need and keep child links current.
- [ ] Status and support claims use records with claim-level proof delegated to [proof.md](../proof.md).
- [ ] Entrypoints and command maps use lookup tables or definition blocks.
- [ ] Owner route cards and package or tool entry cards appear only when they make routing denser than prose.
- [ ] Troubleshooting stays limited to first-run setup; operational recovery links to a runbook.
- [ ] Badges are linked, maintained, high-value status claims, and banners are not the only carrier of meaning.
- [ ] Every deeper concern links to its owner instead of duplicating owner content.
- [ ] License closes published repository README files.
- [ ] Every relative link resolves from the README's directory.
