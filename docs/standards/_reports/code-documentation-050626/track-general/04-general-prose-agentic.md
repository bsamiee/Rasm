# [GENERAL_04_PROSE_AGENTIC_RESEARCH]

Research scope: `docs/standards/reference/code-documentation.md` with emphasis on prose ordering, comment structure, agent usability, examples, omission discipline, and selective major-style-guide guidance. This report evaluates the current worktree content only and does not edit active standards.

## [1][BOUNDARY]

- Assigned output: `docs/standards/_reports/code-documentation-050626/track-general/04-general-prose-agentic.md`.
- Active standards edited: none.
- Worktree note: `docs/standards/reference/code-documentation.md` was already modified before this report write. The review treats the worktree version as the evaluated baseline and does not infer authorship.
- Current date: 2026-06-05.

## [2][TRANSCRIPT]

1. Read `CLAUDE.md` fully.
2. Read root `AGENTS.md` fully.
3. Read `docs/standards/AGENTS.md` fully.
4. Read the requested active standards fully:
   - `docs/standards/reference/code-documentation.md`
   - `docs/standards/style-guide.md`
   - `docs/standards/README.md`
   - `docs/standards/AGENTS.md`
5. Checked active standards corpus with `fd -a . docs/standards -t f -e md | sort`.
6. Checked current worktree state for the requested files with `git status --short -- docs/standards/_reports docs/standards/reference/code-documentation.md docs/standards/style-guide.md docs/standards/README.md docs/standards/AGENTS.md`.
7. Reviewed the current diff for `docs/standards/reference/code-documentation.md` to understand the live baseline without editing it.
8. Used current primary style-guide sources for drift-prone external claims.
9. Created only the assigned `_reports/` report path.

## [3][LOCAL_SOURCES]

- `CLAUDE.md`: required skill routing, no what-comments, greenfield collapse, and docs-only validation boundaries.
- `AGENTS.md`: docs route through `docs/standards/README.md`; root rejects root command catalogs and stale subtree facts.
- `docs/standards/AGENTS.md`: active corpus definition, read order, owner routing, artifact contract, omission rules, and close checks.
- `docs/standards/README.md`: reader-need routing, source precedence, shared-standard owners, placement, lifecycle, split-link, boundaries, and validation.
- `docs/standards/style-guide.md`: prose shape, front-and-close paragraphs, condition-before-action ordering, wording precedence, examples, links, code-safe Markdown, and final proofreading.
- ``: serial-position salience, task/durable artifact separation, constraint stacking, evidence-before-synthesis, provider behavior, retrieval provenance, and agent-surface validation.
- `docs/standards/reference/code-documentation.md`: evaluated target.

## [4][EXTERNAL_SOURCES]

Use these only as supporting evidence. Repo standards and local source truth control conflicts.

| [INDEX] | [SOURCE]                                                                                                                                                                  | [FRESHNESS]                                              | [USED_FOR]                                                                                                                                                                                                           |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | Google Developer Documentation Style Guide, Highlights, https://developers.google.com/style/highlights                                                                    | Last updated 2025-04-02 UTC                              | Confirms active voice, second person, descriptive links, accessibility, condition-before-instruction, code font, list choices, and alt text are mainstream developer-doc rules.                                      |
|   [2]   | Google Developer Documentation Style Guide, API reference code comments, https://developers.google.com/style/api-reference-comments                                       | Last updated 2025-12-01 UTC                              | Supports first-sentence importance, avoiding name echo, documenting prerequisites/exceptions, boolean/default parameter behavior, and replacement paths for deprecations.                                            |
|   [3]   | Google Developer Documentation Style Guide, Sentence structure, https://developers.google.com/style/sentence-structure                                                    | Last updated 2024-10-15 UTC                              | Supports condition, circumstance, or goal before instruction.                                                                                                                                                        |
|   [4]   | Google Developer Documentation Style Guide, Cross-references and linking, https://developers.google.com/style/cross-references                                            | Current page fetched 2026-06-05                          | Supports selective links, link cognitive load, descriptive link text, and most-relevant destination.                                                                                                                 |
|   [5]   | Google Developer Documentation Style Guide, Future features, https://developers.google.com/style/future                                                                   | Last updated 2026-05-19 UTC                              | Supports avoiding future-feature claims in durable docs unless approved through the governing process.                                                                                                               |
|   [6]   | Google C++ Style Guide, Comments, https://google.github.io/styleguide/cppguide.html                                                                                       | Current page fetched 2026-06-05                          | Supports not stating the obvious, comments for ownership/lifetime/nullability/invariants, implementation comments for non-obvious choices, and fixing unclear arguments through stronger code shape before comments. |
|   [7]   | Microsoft Writing Style Guide, Writing step-by-step instructions, https://learn.microsoft.com/en-us/style-guide/procedures-instructions/writing-step-by-step-instructions | Last updated 2026-04-02                                  | Supports necessary introductions only, parallel headings, place-before-action steps, imperative verbs, and not overwhelming a procedure.                                                                             |
|   [8]   | Microsoft Writing Style Guide, Procedures and instructions, https://learn.microsoft.com/en-us/style-guide/procedures-instructions/                                        | Last updated 2022-06-24                                  | Supports omission of unnecessary procedures and input-neutral language when multiple interaction modes exist.                                                                                                        |
|   [9]   | GOV.UK Style Guide landing page, https://www.gov.uk/guidance/style-guide/                                                                                                 | Updated 2026-05-08 but says the manual is being replaced | Used only as a no-import signal: old GOV.UK guide is not the best primary source for new rules.                                                                                                                      |
|  [10]   | Apple Style Guide, https://support.apple.com/guide/applestyleguide/welcome/web                                                                                            | June 2025 guide                                          | Used only for scope awareness: it is a major style guide with technical notation coverage, but no non-obvious rule was stronger than existing repo guidance for this pass.                                           |

## [5][CHECKS]

[REPO_CHECKS]:
- Full requested files read: yes.
- Active standards edit avoided: yes.
- Assigned report file written: yes.
- Existing dirty active standard detected: yes, `docs/standards/reference/code-documentation.md`.
- Static, test, bridge, generated-reference rails: not run, because this is a report-only docs research pass.

[SOURCE_CHECKS]:
- Drift-prone external style claims were checked against current primary pages where possible.
- Stable editorial rules were used only when they added value beyond existing `style-guide.md` and .
- No secondary blog, Reddit, or copied PDF source was used for recommendations.

## [6][CURRENT_STRENGTHS]

[PROSE_ORDERING]:
- The target already leads with the core omission rule: comments exist only for caller-visible semantics that declarations cannot express.
- The high-risk route-away rule appears in the lead and again before language capsules.
- The file now puts decision, produced shape, source truth, and surface model before language-specific mechanics. This matches `docs/standards/AGENTS.md` and .

[COMMENT_STRUCTURE]:
- The comment decision is a review problem, not a blanket public-symbol mandate.
- The produced-shape review record gives agents a concrete checklist before writing comments.
- The surface model separates pure, rail, throwing, script, and catalog surfaces before per-language syntax.
- The validation checklist ties the model back to route, symbol contract, generated reference, and docs-only proof.

[AGENT_USABILITY]:
- The file is scan-friendly for agents: `DOCUMENT_WHEN`, `OMIT_WHEN`, `ROUTE_AWAY`, relation fields, anti-patterns, and validation are explicit.
- The opening authoring contract names adjacent checks and maintenance triggers before examples or language capsules.
- The generated-reference handoff is conditional and prevents agents from emitting empty proof fields.

[OMISSION_DISCIPLINE]:
- Omission is consistently reinforced in the lead, decision router, produced-shape template, surface model, examples rule, generated handoff language, anti-patterns, and validation.
- The target avoids placeholder-heavy `none` patterns and says to omit absent fields.
- The current relation field model keeps `proof.md`-style labels conditional instead of making every code-comment decision carry all proof metadata.

## [7][FINDINGS]

### [F1][ADD_DECLARATION_REPAIR_GATE]

Finding: The target strongly says to omit comments when declarations already carry the fact, but it does not yet make the prior greenfield step explicit: improve the declaration when a comment would merely compensate for a weak public shape.

Why it matters: Rasm policy prefers stronger types, signatures, value objects, enums, schemas, and catalog objects over prose that rescues weak declarations. Google C++ comment guidance gives the same non-obvious move at the argument level: when argument meaning is non-obvious, consider stronger constants, enum arguments, or an options object before relying on comments.

Recommendation: add a short declaration-repair checkpoint before `DOCUMENT_WHEN` or inside `OMIT_WHEN`.

Candidate rule:

```text
[REPAIR_FIRST]:
- If a stronger name, type, enum, value object, schema field, SQL object, shell declaration, or catalog comment can carry the caller-visible fact, change the declaration route first and write a source comment only for the remaining semantic obligation.
```

Confidence: high.

### [F2][ADD_GENERATED_SUMMARY_EXTRACTION_RULE]

Finding: The target requires a summary and validates that the lead sentence carries contract rather than name echo, but it does not explicitly connect the first sentence to generated-reference extraction where generators consume summaries.

Why it matters: Google API reference guidance notes that documentation tools often extract the first sentence for list and summary views. For an agent, this is a practical rule: the first sentence should be unique, short, and contract-bearing when generated docs consume it.

Recommendation: add a conditional generator-aware sentence to `PRODUCED_SHAPE`, `SURFACE_MODEL`, or the generated-reference validation group.

Candidate rule:

```text
- When a generated reference extracts the first sentence, make that sentence unique, short, and contract-bearing; do not start by repeating the symbol name or toolchain-local profile.
```

Do not make this universal for every source comment. Keep it conditional on a generated-reference consumer.

Confidence: high.

### [F3][CHANGE_EXAMPLES_TO_PROVE_MISUSE]

Finding: The language capsules include concise conceptual examples, but most are syntax cues rather than misuse-prevention examples. `style-guide.md` says examples must prove shape or prevent misuse, and should prefer a positive/rejected pair when the distinction matters.

Why it matters: The target's most important distinction is not syntax; it is "semantic contract versus type echo." A compact contrast near the decision router or anti-patterns would teach the standard better than adding more fenced language examples.

Recommendation: add one compact contrast record and avoid multiplying examples per language.

Candidate shape:

```text
Accepted: `source` is a caller-owned stream positioned at the payload boundary.
Rejected: `source` is a `PlanSource`.
Reason: the accepted text states the caller obligation; the rejected text repeats the declared type.
```

Confidence: high.

### [F4][CHANGE_PROFILE_RECORDS_ONLY_IF_AGENT_SCAN_FAILS]

Finding: The `SURFACE_MODEL` profiles are currently prose records with indented `Applies`, `Comment owns`, and `Failure channel` lines. They are readable, but they are less mechanically uniform than the surrounding bracketed lists.

Why it matters: Agent scan performance is usually better when peer shapes use the same record grammar. However, the current block is short and not ambiguous, so this is not an immediate defect.

Recommendation: no immediate edit required. If later agents misread profile fields, convert each profile to the same compact record form rather than adding new prose.

Confidence: medium.

### [F5][KEEP_LINK_GUIDANCE_LOCAL]

Finding: External link guidance mostly confirms what the local standards already say: links should be selective, descriptive, and placed where they change reader action. Google explicitly warns that each link adds a decision and can pull readers away from the page.

Why it matters: This supports the current Rasm rule against decorative cross-links. It does not justify importing a broader link-style catalog into `code-documentation.md`.

Recommendation: no active change. Keep route links only where generated references, README, architecture, support, runbook, how-to, or tutorial behavior actually changes.

Confidence: high.

### [F6][DO_NOT_IMPORT_GENERIC_STYLE_GUIDE_CATALOGS]

Finding: Major style guides agree on active voice, present tense, condition-before-action, descriptive links, code font for code terms, accessibility, and concise examples. `style-guide.md` already owns these mechanics.

Why it matters: Copying these into `code-documentation.md` would duplicate shared guidance and weaken owner routing. The target should reference `style-guide.md`, not become a second style guide.

Recommendation: no active change except for the two code-comment-specific additions above: declaration repair before comment, and generated first-sentence extraction where relevant.

Confidence: high.

## [8][ADD_RECOMMENDATIONS]

1. Add a `REPAIR_FIRST` checkpoint before `DOCUMENT_WHEN`.
2. Add a conditional generated-summary extraction rule for first sentences consumed by generated references.
3. Add one compact Accepted/Rejected/Reason example that distinguishes caller obligation from type echo.

## [9][CHANGE_RECOMMENDATIONS]

1. Change examples from language syntax cues toward one or two misuse-prevention contrasts when editing this file.
2. Change the surface profile block to a stricter record grammar only if later review shows agents confuse `Applies`, `Comment owns`, or `Failure channel`.
3. Change any future external-style-guide imports into route notes or local examples, not duplicated generic prose mechanics.

## [10][REMOVE_RECOMMENDATIONS]

1. Remove no current route-away, omission, relation-field, or validation structure.
2. Do not remove the generated-reference handoff. It is now scoped enough to avoid metadata spam.
3. Do not remove language capsules; they are useful as syntax/toolchain routing, provided the examples stay compact.
4. Do not import or preserve the old GOV.UK style-guide landing page as active authority; it says the manual is being replaced.

## [11][NO_CHANGE_CONFIRMATIONS]

[GENERAL_ORDER]:
- No change recommended to the current top-level order: lead, use when, decision router, produced shape, source truth, surface model, language capsules, lifecycle, anti-patterns, boundaries, validation.

[OMISSION]:
- No change recommended to the omission model. It is one of the strongest parts of the file.

[AGENT_CONTRACT]:
- No change recommended to the authoring contract fields. They give future agents the right decision surface.

[ROUTE_AWAY]:
- No change recommended to route-away placement. It appears early enough to prevent source comments from becoming mini READMEs, generated API catalogs, support pages, or runbooks.

[EXTERNAL_STYLE]:
- No broad style-guide import recommended. The shared `style-guide.md` and already cover general prose mechanics.

[VALIDATION]:
- No C#, TypeScript, Python, Bash, SQL, static, test, bridge, or generated-reference validation should run for this report-only work.

## [12][CONFIDENCE]

- High confidence: declaration-repair gate, generated first-sentence rule, examples-as-misuse-proof, no broad style-guide import.
- Medium confidence: profile-record normalization, because the current block is acceptable and the value depends on future agent misread frequency.
- Low risk: recommended changes are small, local, and compatible with current source precedence.

## [13][CLOSE]

The strongest GENERAL 4 improvement is to make the standard more greenfield: before documenting a public surface, ask whether the declaration can carry the fact more precisely. The current file already handles prose order, route-away behavior, omission discipline, and agent-facing structure well; the next edit should add only the declaration-repair checkpoint, a conditional generated-summary rule, and one compact contrast example.
