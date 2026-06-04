# [README_STANDARDS]

A README is an entry document: it orients a reader at a repository, package, directory, tool, or documentation-corpus boundary in one screen, gives the shortest path to one successful action, and routes every deeper concern to the document that owns it. The README is the front door and the local index, never the architecture page, the API catalog, the tutorial, or the contribution workflow.

## [1][USE_WHEN]

Use a README to open exactly one boundary:

- a repository, for users, adopters, and contributors who arrive at the root;
- a package, library, or directory, for the local purpose and import surface;
- a documentation corpus, as the hub that routes a reader to the next page;
- a tool, command, or local service, for the first runnable invocation.

Do not use a README to carry architecture structure, an API or symbol catalog, a learning path, an operational recovery procedure, a future-work sequence, a durable decision record, or a contribution workflow. Route each to its owner by topic and keep the README a one-screen front door.

## [2][CANONICAL_ANCHOR]

This standard adopts the `standard-readme` specification as the professional baseline for section identity and order, the `makeareadme` guidance for section content, and `shields.io` conventions for badges. Where the canonical baseline and this standard differ, this standard's profile model and structure rules control, because the repository routes deeper concerns to owning types rather than expanding the README. Treat the baseline as the section vocabulary every profile draws from, and treat the profile lists below as the binding required set.

`Source of truth:` `standard-readme` spec (`github.com/RichardLitt/standard-readme/blob/main/spec.md`), `makeareadme.com`, `shields.io`. `Last verified:` 2026-06-04. `Review trigger:` the `standard-readme` spec or `shields.io` badge model changes.

## [3][SOURCE_TRUTH]

README claims about names, commands, status, and support are drift-prone, so order their evidence before writing them. Use the strongest source that proves the claim:

1. Repository source, manifests, lockfiles, and generated contracts for names, commands, entrypoints, and package status.
2. Runnable tool output for advertised setup, quick-start, and verification commands, captured during the change.
3. The owning document for any claim the README only summarizes and links.

When the README would restate a fact another document owns, link that document and let the link carry the truth; do not copy the fact into the README where it can drift out of step with its owner.

## [4][PROFILES]

A README resolves to one of four profiles. Each profile fixes a different reader, a different required section set, and a different acceptance bar. Pick one primary profile and name it in the opening paragraph whenever the file could be read as another profile.

| [INDEX] | [PROFILE]     | [READER]                 | [BOUNDARY]      | [SUCCESS_SIGNAL]                       |
| :-----: | :------------ | :----------------------- | :-------------- | :------------------------------------- |
|   [1]   | Root entry    | user or contributor      | repository      | first run plus owner-doc route         |
|   [2]   | Package entry | package consumer/editor  | package         | purpose, status, and import surface    |
|   [3]   | Hub index     | corpus navigator         | docs corpus     | correct child page on first read       |
|   [4]   | Tool entry    | command/service operator | tool or service | first command runs and verifies effect |

Choose the profile by the boundary the file opens, not by where the file sits. A package directory whose README mostly routes a documentation corpus is a hub index, and a repository root whose README mostly drives one command is a tool entry.

## [5][README_CONTRACT]

Every profile obeys one shared contract:

- Name the file `README.md` unless a hosting platform or localization standard requires a documented variant.
- Open with one bracketed H1 whose label names the repository, package, directory, tool, or corpus, such as `# [RASM]`; when a hosting platform explicitly requires an unbracketed display title, explain the exception in the first paragraph.
- Place a one-paragraph description directly after the title, and after any badge or banner, before any other section. Keep the description under roughly 120 characters of leading summary so a package-manager and GitHub blurb match it.
- Use relative links for repository-local targets so the README renders on the hosting platform and in a local checkout alike.
- Show every advertised install, setup, usage, or command exactly as a reader runs it in a fenced, intent-labeled code block, or mark the unverified line as a known gap with `Evidence:`.

Field cardinality for the shared contract: the file name, the H1 title, and the one-paragraph description are required and singular. Badges and banners are optional and repeatable. Relative-link usage is a required constraint on every local link, not a section.

## [6][BADGE_CONTENT_RULES]

Badges are metadata signals, not decoration. Include a badge only when it reflects maintained automation, a real release, security posture, or package metadata, and remove a badge whose automation is retired. Apply these content rules:

- Cap the lead block at two to four high-value badges — build status, version, coverage, license — and push secondary badges lower or into a table.
- Source badges consistently from `shields.io` or the host's native workflow badge so the set reads as one visual group.
- Link each badge to the service that produced it so a reader reaches the underlying status.
- Prefer dynamic badges driven by CI, registry, or coverage state over static text badges for any claim that changes.

A badge is a status claim, so it inherits the evidence obligation: a build or coverage badge must point at the workflow or service that produces it, and a stale badge is a regression, not a leftover.

## [7][PROFILE_STRUCTURES]

Each profile lists its sections in reading order with field cardinality: `required` sections always appear; `optional` sections appear at the author's discretion when the named concern exists but no rule forces them; `conditional` sections are required if and only if a named trigger holds, so the author has no discretion once the trigger is present; and `repeatable` marks a section that holds a list whose length varies. The four cardinality words are the closed vocabulary; `conditional` differs from `optional` in that a present trigger makes the section mandatory rather than merely permitted.

Root entry:

1. Title — required, singular.
2. Badges or banner — optional, repeatable; obey the badge content rules above.
3. Description — required, one paragraph stating purpose and boundary.
4. Status, maturity, or support note — conditional; required when the repository is pre-release, experimental, or under a stated support phase, and render it as a status-tagged record per the structure rules below.
5. Install, setup, or quick start — required for any repository a reader installs or runs; render commands as intent-labeled code blocks.
6. Minimal usage or first successful path — required when a reader can produce one observable result; show the command and its expected output.
7. Documentation map — required when owner documents exist; repeatable definition block or table of one link per owner with a one-line role.
8. Help or support — optional; include when a support channel exists.
9. Security reporting link — optional; include when a security policy or contact exists.
10. Contributing link — optional; include when a contribution workflow is maintained.
11. Maintainers or governance link — optional; include when ownership is durable and published.
12. License — required; name the SPDX identifier and the license owner, and place it last.

Package entry:

1. Purpose and boundary — required; one paragraph naming what the package owns and the nearest concern it explicitly excludes.
2. Current status — required; the package maturity or support phase as a status-tagged record with its evidence source.
3. Public entrypoints or supported import surface — required; the importable names, exports, or commands a consumer depends on, as a lookup table or definition block.
4. Local build, test, or usage command — conditional; required when the package is directly runnable from its own directory, as an intent-labeled code block.
5. Owner documents — optional, repeatable; one link per owner to the architecture, design, API, roadmap, or decision record that the README summarizes.
6. Constraints, non-goals, or known exclusions — conditional; required when a reader would otherwise assume an unsupported capability.
7. Owner, maintainer, or escalation contact — optional; include when ownership is durable.

Hub index:

1. Corpus purpose — required; one paragraph naming the corpus and its boundary.
2. How to choose a child page — required; the reader question or reader need that selects each child, framed for routing rather than inventory.
3. Child links with one-line roles — required, repeatable; one link per child page with the role that distinguishes it from its siblings, as a definition block under eight children or a table when children carry comparable columns such as status or owner.
4. Freshness trigger — conditional; required when the child set changes as the corpus grows, naming the event that makes the map stale.

Tool entry:

1. Tool purpose and boundary — required; what the tool does and the nearest concern it does not.
2. Supported environment or prerequisites — required; the runtime, host, or permission a reader needs before the first command.
3. First successful command — required; one copyable, intent-labeled invocation that produces one observable result.
4. Common command map — optional; the commands (target three to seven) a reader runs after the first successful command, as a lookup table of command to effect.
5. Output, artifact, or side-effect locations — conditional; required when the tool writes files, captures, or external state.
6. Verification command or expected result — required; the command or signal that confirms the tool ran correctly.
7. Troubleshooting or owner escalation — optional; render known failure to known response as a decision table when more than one symptom maps to a distinct fix.

## [8][REQUIRED_STRUCTURE]

Every README instantiates one profile's template below. Copy the matching template, keep `required` headings, add a `conditional` heading whenever its trigger holds, add an `optional` heading only when its concern exists, and repeat a `repeatable` block per item. Heading labels use the bracketed uppercase idiom and may relabel the semantic label only when the profile role and order remain intact; the section identity and order are fixed.

Each template is a faithful render of its profile's section list above, not a re-derivation. Every numbered list item maps to exactly one heading in the same order, except the pre-heading slots that precede the first H2: the H1 title and the lead paragraph in every profile, plus the optional Badges or banner comment a Root entry carries. So a Root entry renders list items 1 through 3 as the H1, a badge comment, and the Description paragraph, and each of the other three profiles renders list item 1 as the H1 and its lead paragraph; every remaining list item becomes exactly one H2. The cardinality comment on each template heading repeats verbatim the cardinality word its list entry carries, so an author who follows the list and an author who follows the template derive the same required set. When a heading rewords a list label, that is the only permitted deviation, and the comment still names the same cardinality and trigger.

Root entry template:

```markdown template
# [REPOSITORY_NAME]

<!-- source-only: badges are optional and repeatable; use 2-4 high-value linked shields.io badges -->

<One-paragraph description: required, singular>

## [1][STATUS_CONDITIONAL_PRE]

## [2][INSTALL_REQUIRED_WHEN]

## [3][USAGE_REQUIRED_WHEN]

## [4][DOCUMENTATION_REQUIRED_WHEN]

## [5][SUPPORT_OPTIONAL]

## [6][SECURITY_OPTIONAL]

## [7][CONTRIBUTING_OPTIONAL]

## [8][MAINTAINERS_OPTIONAL]

## [9][LICENSE_REQUIRED_LAST]

```

Package entry template:

```markdown template
# [PACKAGE_NAME]

<Purpose and boundary: required, singular>

## [1][STATUS_REQUIRED_STATUS]

## [2][ENTRYPOINTS_REQUIRED_IMPORT]

## [3][USAGE_CONDITIONAL_DIRECTLY]

## [4][DOCUMENTS_OPTIONAL_REPEATABLE]

## [5][CONSTRAINTS_CONDITIONAL_UNSUPPORTED]

## [6][OWNER_OPTIONAL]

```

Hub index template:

```markdown template
# [CORPUS_NAME]

<Corpus purpose: required, singular>

## [1][CHOOSE_NEXT_PAGE]

## [2][PAGES_REQUIRED_REPEATABLE]

## [3][FRESHNESS_CONDITIONAL_CHILD]

```

Tool entry template:

```markdown template
# [TOOL_NAME]

<Tool purpose and boundary: required, singular>

## [1][REQUIREMENTS_REQUIRED_RUNTIME]

## [2][FIRST_COMMAND_REQUIRED]

## [3][COMMANDS_OPTIONAL_LOOKUP]

## [4][OUTPUT_CONDITIONAL_TOOL]

## [5][VERIFY_REQUIRED_CONFIRMS]

## [6][TROUBLESHOOTING_OPTIONAL_DECISION]

```

## [9][STRUCTURE_RULES]

Apply the agent-friendly form at the sections where it fits; prose is the fallback only where no structure applies:

- Status, maturity, and support: render as a status-tagged record carrying `Status` from the closed set (`PLANNED`, `IN-PROGRESS`, `BLOCKED`, `DONE`, `DROPPED`), an `Exit` or support-phase condition, and an `Evidence:` source. A maturity claim with no status field and no source is the prose drift this rule removes.
- Import surface, entrypoints, and command maps: render as a lookup table (key to effect) or a definition block, never as a paragraph the reader must parse into an implicit list. A command-to-effect set is a lookup table; a single record read by field is a definition block.
- Troubleshooting and symptom routing: render as a decision table — condition columns left, the resulting fix right — once more than one symptom maps to a distinct response. A single symptom-fix pair stays prose.
- Documentation and child maps: render as a definition block of one `link: role` per line under eight entries, and pivot to a table only when entries gain comparable columns such as status or owner. Keep one link per entry and a role that separates it from its siblings.
- Verification and acceptance: render the install, usage, and verify commands so each is copyable and intent-labeled, and confirm completion through the review checklist's checkbox form rather than asserting it in prose.
- License: state the SPDX identifier and the license owner, and place the section last so the binding legal constraint closes the file.

## [10][CONTENT_REQUIREMENTS]

A README that omits these facts is incomplete regardless of how well it reads. Each profile must carry, with evidence where the fact can drift:

- The boundary it opens and the nearest concern it excludes, stated in the first paragraph.
- The shortest path to one observable result: the install or first command, then the usage or verify step that proves it, both copyable and run during the change.
- One link per owner document for every deeper concern the README only summarizes, with no concern both summarized and owned in the README.
- A current status or support claim sourced to a manifest, lockfile, or tool output for any package that is pre-release, experimental, or under a stated support phase.
- A license with its SPDX identifier and owner for any published repository.
- For a tool, the output or side-effect locations and the verification signal; for a hub, the reader need that selects each child; for a package, the supported import surface a consumer depends on.

## [11][EXAMPLES]

A hub index is the profile most readers misbuild as a flat link dump, so show the routing shape directly. Use a definition block for the child map when each child is read by field and the corpus holds fewer than eight children; pivot to a table only when the child set adds comparable columns such as status or owner.

```markdown template
# [DOCUMENTATION]

## [1][CHOOSE_NEXT_PAGE]

Read by what you are doing, then open one child.

[architecture.md](../explanation/architecture.md): current structure, boundaries, and invariants
[runbook.md](../task/runbook.md): symptom-to-fix steps when a service degrades
[support-matrix.md](support-matrix.md): which versions, platforms, and runtimes are supported
```

That block is a template example: it shows the routing shape, not a complete runnable file. It keeps one link per child, gives each child a role that separates it from its siblings, and routes by reader need rather than by alphabetized inventory.

The opposite shape misroutes the reader. The rejected example pads the same map with prose that hides the route and duplicates the child's own content:

```markdown rejected
The architecture document is a very important and comprehensive resource that
you should definitely read carefully, and it covers many topics including the
overall structure as well as some of the boundaries and a few invariants...
```

That block is `rejected`: it buries the link, restates what the child owns, and forces the reader to parse a paragraph to find one route. A README that explains a child instead of linking it has stopped being an index.

A package status claim is the fact most often flattened into undated prose, so render it as a status-tagged record with its source:

```markdown template
## [1][STATUS]

Status: IN-PROGRESS
Exit: public API frozen and the package published to the registry.
Evidence: Directory.Packages.props; tools.quality static build output.
```

## [12][BOUNDARIES]

- [README.md](../README.md) owns document-type routing: which type a draft becomes, where a document is placed, when it is split, and which standard owns a rule. Route every "is this a README or an architecture page, an API catalog, a tutorial, a runbook, a roadmap, or a contributing guide" question to that router by topic. When a README starts to carry one of those concerns, split the concern to its owning type and leave one link behind.

## [13][REVIEW_CHECKLIST]

- [ ] Exactly one profile is primary, and the opening paragraph names it when the file could read as another profile.
- [ ] The H1 matches the repository, package, directory, tool, or corpus name, or explains the intended mismatch in the first paragraph.
- [ ] The one-paragraph description sits directly after the title or banner and before any other section.
- [ ] The file instantiates the matching required-structure template, with every required heading present, every conditional heading present wherever its trigger holds, and optional headings only where their concern exists.
- [ ] Status and support claims use a status-tagged record with `Status`, an exit or support-phase condition, and an evidence source.
- [ ] Import surfaces and command maps use a lookup table or definition block; troubleshooting uses a decision table once more than one symptom maps to a fix.
- [ ] Install, setup, usage, and verification commands are copyable, intent-labeled, and were run in this change, or each unverified line carries an `Evidence:` gap note.
- [ ] Badges are capped at a high-value lead block, linked to their service, sourced consistently, and free of retired automation.
- [ ] Every link routes a deeper concern to its owner instead of duplicating the owner's content.
- [ ] Name, status, and support claims cite repository source, manifest, or tool output, not recall.
- [ ] The license names its SPDX identifier and owner and closes the file.
- [ ] Every relative link resolves from this file's directory.
