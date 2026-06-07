Question: Which current GitHub Flavored Markdown and GitHub Markdown capabilities should shape Rasm standards for fences, tables, anchors, task lists, alerts, details, footnotes, and hidden comments?
Type: source-scan
Lane: track-external-research
Merge key: docs/standards/proof.md :: GitHub Markdown renderer capabilities :: source proof packet
Target owner: `docs/standards/proof.md`
Source basis: active standards and report-session reads; official GitHub Docs, GitHub Flavored Markdown Spec, and CommonMark 0.30 accessed 2026-06-06
Promotion target: `docs/standards/proof.md`, with owner-routed changes to `docs/standards/information-structure.md` and `docs/standards/formatting.md`
Outcome: PROMOTE

## [FINDINGS]

Finding 1
    Active owner/section: `docs/standards/information-structure.md` `[11][CODE_BLOCKS]`; `docs/standards/formatting.md` `[9][VALIDATION]`.
    Evidence source URLs: https://spec.commonmark.org/0.30/#fenced-code-blocks; https://github.github.com/gfm/#fenced-code-blocks; https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-and-highlighting-code-blocks.
    Finding: CommonMark and GFM define fenced code blocks with optional info strings. The first info-string word is conventionally used as the code language, but CommonMark does not mandate renderer treatment. GitHub Docs says GitHub uses an optional language identifier for syntax highlighting and uses Linguist for language detection and grammar selection.
    Weakness/inconsistency: Rasm's `language intent` convention is valid as a source-level convention only if the first word remains the renderer language. The active rule already says renderer-local fences keep exact tags, but it does not explicitly state that trailing intent words are not GitHub renderer semantics.
    Proposed correction: Add a sentence to the code-block rule: the first info-string word is the renderer language identifier; any following Rasm intent label is source-level convention for agents and validators, not a GitHub highlighting guarantee. Keep renderer tags such as `mermaid`, `geojson`, `topojson`, and `stl` exact and move intent to nearby prose when the tag drives rendering.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/formatting.md`; `docs/standards/reference/code-documentation.md`; type standards with fenced templates.
    Decision: PROMOTE.
    Proof gap/question: If Rasm later validates fence labels, the validator must parse the first word separately from Rasm's trailing intent word.

Finding 2
    Active owner/section: `docs/standards/information-structure.md` `[3][TABLES]`; `docs/standards/formatting.md` `[4][TABLE_STYLING]`; `docs/standards/proof.md` `[9][DOCS_CODE_VERIFICATION]`.
    Evidence source URLs: https://github.github.com/gfm/#tables-extension-; https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/organizing-information-with-tables.
    Finding: GFM tables are row-and-column leaf blocks with one header row, one delimiter row, and data rows. Inlines can appear in cells, block-level elements cannot. The delimiter row supports colon alignment. Literal pipes inside cells must be escaped. GitHub Docs also tells authors to include a blank line before a table for correct rendering.
    Weakness/inconsistency: The active standards correctly reject multiline and nested table cells and correctly style alignment, but they state renderer facts without adjacent provider proof. The local blank-line rule in `formatting.md` is stronger than raw GFM and matches GitHub Docs, but the source class is not named beside the claim.
    Proposed correction: Add a renderer-support proof packet in `proof.md` for GFM/GitHub table claims, then keep table shape in `information-structure.md` and table surface styling in `formatting.md`.
    Active owner: `docs/standards/proof.md`.
    Ripple files: `docs/standards/information-structure.md`; `docs/standards/formatting.md`; future table validators.
    Decision: PROMOTE.
    Proof gap/question: Local GitHub-render proof was not run; official GitHub Docs are enough for maintained-provider proof unless a repository publishing path depends on exact rendered output.

Finding 3
    Active owner/section: `docs/standards/information-structure.md` `[7][CHECKLISTS]`; `docs/standards/formatting.md` `[2][STATUS_RESULT_MARKERS]`.
    Evidence source URLs: https://github.github.com/gfm/#task-list-items-extension-; https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/about-tasklists.
    Finding: GFM task list markers are list-item prefixes using `[ ]`, `[x]`, or `[X]`, rendered as semantic checkbox elements. GFM allows arbitrary nesting, while GitHub tasklists in comments and issue bodies can be interactive and issue-aware. GitHub tasklist blocks are retired in favor of sub-issues, but ordinary Markdown tasklists still exist.
    Weakness/inconsistency: Rasm checklists are documentation verification, acceptance, and status containers. They should not inherit GitHub issue tasklist workflow semantics, automatic issue tracking, or sub-issue replacement behavior.
    Proposed correction: Keep `- [ ]` and `- [x]` as Markdown checklist notation, and add a proof-owned boundary note: GitHub issue tasklist behavior is provider UI behavior, not a standards-corpus lifecycle model.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/formatting.md`; task and learning type standards with checklists.
    Decision: MERGE.
    Proof gap/question: No local renderer proof needed unless the repository claims interactive GitHub issue behavior.

Finding 4
    Active owner/section: `docs/standards/formatting.md` `[7][ANCHORS_COMMENTS]`.
    Evidence source URLs: https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax#section-links; https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes#section-links-in-markdown-files-and-blob-pages.
    Finding: GitHub automatically generates anchors for headings, exposes them in rendered files, and suffixes duplicate heading anchors such as `-1`. GitHub warns that editing headings or changing order of identical anchors requires updating links. GitHub also supports custom HTML anchors, but they are not included in the document outline/table of contents.
    Weakness/inconsistency: Rasm's bracket-heading anchor convention is explicitly local, which is good. The active rule should continue rejecting duplicate heading anchors instead of relying on GitHub duplicate suffixes, because stable in-repo links matter more than GitHub's fallback behavior.
    Proposed correction: Keep the local anchor rule in `formatting.md`; add maintained-provider proof for GitHub's auto-anchor and duplicate-suffix behavior only as background. Do not promote custom HTML anchors as a general standards escape hatch.
    Active owner: `docs/standards/formatting.md`.
    Ripple files: active type standards with heading templates; local link/anchor validation scripts if added.
    Decision: MERGE.
    Proof gap/question: Rasm's exact bracket-heading slugger remains a local validation convention and still needs local validator proof if claimed as automated.

Finding 5
    Active owner/section: `docs/standards/information-structure.md` `[14][CALLOUTS_COLLAPSIBLE_FOOTNOTES]`; `docs/standards/formatting.md` `[3][INVOCATION_MARKERS]`.
    Evidence source URLs: https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax#alerts.
    Finding: GitHub alerts are a blockquote-based extension with five documented types: `NOTE`, `TIP`, `IMPORTANT`, `WARNING`, and `CAUTION`. GitHub advises using alerts only when crucial, limiting them to one or two per article, avoiding consecutive alerts, and not nesting them inside other elements.
    Weakness/inconsistency: The active standards already separate GitHub alerts from instruction invocation markers, but the limit and non-nesting constraints belong in the alert rule or a proof packet. Current alert-demotion tasks should use GitHub's own scarcity rule as external support.
    Proposed correction: Promote an alert scarcity and non-nesting proof note. Keep syntax and marker spelling in `formatting.md`, but keep alert use/demotion tests in `information-structure.md`.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/formatting.md`; `docs/standards/task/runbook.md`; `docs/standards/reference/api.md`.
    Decision: PROMOTE.
    Proof gap/question: Local render proof is optional for the rule; required only when a changed document claims exact visual rendering.

Finding 6
    Active owner/section: `docs/standards/information-structure.md` `[14][CALLOUTS_COLLAPSIBLE_FOOTNOTES]`; `docs/standards/proof.md` `[9][DOCS_CODE_VERIFICATION]`.
    Evidence source URLs: https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/organizing-information-with-collapsed-sections; https://github.github.com/gfm/#html-blocks.
    Finding: GitHub documents collapsible sections with `<details>` and `<summary>`, including Markdown content inside the details block and an optional `open` attribute. GFM/CommonMark classify many HTML blocks, including `details` and `summary`, as HTML block starts.
    Weakness/inconsistency: The active standards correctly classify `<details>` as a low-salience support container, but provider behavior is drift-prone and belongs beside maintained-source proof. The rule should not imply all renderers treat nested Markdown inside raw HTML identically.
    Proposed correction: Add a GitHub-specific proof record for `<details>` support and keep the Rasm use rule narrow: hide low-salience support, never proof, warnings, or first-read procedures.
    Active owner: `docs/standards/proof.md`.
    Ripple files: `docs/standards/information-structure.md`; `docs/standards/formatting.md`.
    Decision: PROMOTE.
    Proof gap/question: If Rasm publishes through non-GitHub renderers, record a renderer-specific proof gap before relying on nested Markdown inside `<details>`.

Finding 7
    Active owner/section: `docs/standards/information-structure.md` `[14][CALLOUTS_COLLAPSIBLE_FOOTNOTES]`; `docs/standards/proof.md` `[4][PROOF_FIELDS]`.
    Evidence source URLs: https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax#footnotes.
    Finding: GitHub supports footnotes with `[^label]` references and `[^label]:` definitions. GitHub states footnote source position does not control rendered position; footnotes render at the bottom of the Markdown. GitHub Docs also states footnotes are not supported in wikis.
    Weakness/inconsistency: Rasm's rule that footnotes may attach short provenance but never replace visible proof fields is correct. The standards should add the GitHub wiki limitation and rendered-position behavior only when publishing surface matters.
    Proposed correction: Keep visible proof fields as the default. Add a footnote proof note: footnotes are low-salience provenance, not proof-field replacement, and GitHub wiki support is a provider-surface exception.
    Active owner: `docs/standards/proof.md`.
    Ripple files: `docs/standards/information-structure.md`; `docs/standards/formatting.md`; docs that may target GitHub wikis.
    Decision: MERGE.
    Proof gap/question: If Rasm docs are copied into GitHub wikis, footnote-dependent provenance must be rewritten or marked unsupported.

Finding 8
    Active owner/section: `docs/standards/formatting.md` `[7][ANCHORS_COMMENTS]`; `docs/standards/information-structure.md` `[5][TABLES_PROSE]`.
    Evidence source URLs: https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax#hiding-content-with-comments; https://github.github.com/gfm/#html-blocks; https://github.github.com/gfm/#lists.
    Finding: GitHub supports HTML comments for hidden rendered content. GFM recognizes HTML comments as HTML blocks. The GFM list section also uses a blank HTML comment as a way to separate consecutive lists or separate a list from an indented code block that would otherwise bind to the list.
    Weakness/inconsistency: Rasm correctly restricts hidden comments to source-view hints and forbids using them as safety, proof, or required constraints. The active rule could be tightened to reject blank separator comments as a documentation-normalization pattern unless a parser ambiguity proves they are needed.
    Proposed correction: Keep `<!-- source-only: ... -->` as the only normal hidden-comment shape. Add a narrow exception: blank comments may separate ambiguous adjacent Markdown blocks only when visible structure cannot do it without changing rendered meaning.
    Active owner: `docs/standards/formatting.md`.
    Ripple files: future Markdown validators; any generated Markdown that uses hidden comments.
    Decision: PROMOTE.
    Proof gap/question: Need a repository scan before promoting a blank-comment exception to know whether the pattern exists locally.

Finding 9
    Active owner/section: `docs/standards/proof.md` `[9][DOCS_CODE_VERIFICATION]`; `.reports/standards-structure-notation-060626/track-synthesis/00-collective-task-list.md` `[3.4][PROOF]`.
    Evidence source URLs: all GitHub/GFM/CommonMark URLs in this report.
    Finding: Renderer-dependent claims split into two source classes: stable syntax/spec facts from CommonMark/GFM, and GitHub-product behavior from GitHub Docs. Examples: table grammar is GFM spec; GitHub's blank-before-table instruction, alerts, rendered anchor exposure, tasklist UI behavior, details rendering, footnote placement, and hidden rendered comments are GitHub-product behavior.
    Weakness/inconsistency: The current task list asks for a renderer-support proof packet, but active standards do not yet expose the spec-vs-provider split near the claims. Without that split, future edits can overclaim GitHub-specific behavior as universal GFM.
    Proposed correction: Add a compact renderer-support proof packet in `proof.md` with fields for `Capability`, `Source class`, `Maintained source`, `Applies to`, `Owner rule`, `Last verified: 2026-06-06`, and `Review trigger`.
    Active owner: `docs/standards/proof.md`.
    Ripple files: `docs/standards/information-structure.md`; `docs/standards/formatting.md`; type standards with renderer-sensitive examples.
    Decision: PROMOTE.
    Proof gap/question: Local render proof remains unrun; mark it separately if exact GitHub-rendered output is a claim in an active edit.

Finding 10
    Active owner/section: `docs/standards/information-structure.md` `[2][CONTAINER_CHOOSER]`, `[14][CALLOUTS_COLLAPSIBLE_FOOTNOTES]`.
    Evidence source URLs: https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax#alerts; https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/organizing-information-with-collapsed-sections; https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax#footnotes.
    Finding: GitHub offers three low- or interrupt-salience carriers: alerts interrupt, details hide optional support, and footnotes move provenance to the rendered bottom. Each has a different reader-action cost.
    Weakness/inconsistency: The active standard lists all three containers, but the decision table could more sharply distinguish "interrupt reading path", "hide low-salience support", and "attach non-load-bearing provenance".
    Proposed correction: Add a three-row carrier choice note: alert for crucial interruption, details for optional support, footnote for short non-load-bearing provenance. Route proof, warning, and first-read procedure content away from details and footnotes.
    Active owner: `docs/standards/information-structure.md`.
    Ripple files: `docs/standards/proof.md`; `docs/standards/formatting.md`; runbook and API standards that use alerts.
    Decision: MERGE.
    Proof gap/question: None for the source-backed distinction; local render proof only if exact layout is claimed.

## [EVIDENCE]

[SOURCE_SET_READ]:
- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`
- `.reports/AGENTS.md`
- `.reports/standards-structure-notation-060626/README.md`
- `docs/standards/information-structure.md`
- `docs/standards/formatting.md`
- `docs/standards/proof.md`
- `.reports/standards-structure-notation-060626/track-synthesis/00-collective-task-list.md`
- Existing adjacent reports in `track-formatting-notation/01-formatting-token-systems.md` and `track-information-structure/01-information-structure-container-toolbelt.md` to avoid retreading known findings.

[PRIMARY_SOURCE_SET_ACCESSED_2026_06_06]:
- GitHub Docs, Basic writing and formatting syntax: https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax.
- GitHub Docs, Creating and highlighting code blocks: https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-and-highlighting-code-blocks.
- GitHub Docs, Organizing information with tables: https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/organizing-information-with-tables.
- GitHub Docs, About tasklists: https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/about-tasklists.
- GitHub Docs, Organizing information with collapsed sections: https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/organizing-information-with-collapsed-sections.
- GitHub Docs, Creating diagrams: https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams.
- GitHub Docs, About repository README files: https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes.
- GitHub Flavored Markdown Spec: https://github.github.com/gfm/.
- CommonMark 0.30: https://spec.commonmark.org/0.30/.

## [RECOMMENDATIONS]

[PROMOTE]:
- Add a renderer-support proof packet in `docs/standards/proof.md` that separates GFM/CommonMark syntax proof from GitHub-product behavior proof.
- Add explicit first-word renderer-language guidance for fenced code blocks in `docs/standards/information-structure.md`.
- Add current GitHub-source proof for table, alert, details, footnote, anchor, and hidden-comment behavior near the owning rules.
- Add a narrow hidden-comment exception only for parser ambiguity, not proof or safety content.

[MERGE]:
- Merge task-list source facts into the existing checklist rule without importing GitHub issue tasklist workflow semantics.
- Merge anchor proof as background while preserving Rasm's stricter duplicate-heading rejection.
- Merge alert, details, and footnote choice into the container chooser as three distinct reader-action costs.

[HOLD]:
- Hold local renderer-proof claims until a GitHub render, docs build, or configured checker is actually run.
- Hold any custom-anchor promotion until an active standard names a real non-heading target that needs one.

## [PROOF_GAPS]

- Local GitHub-render proof was not run. This report uses maintained official GitHub Docs, GFM, and CommonMark sources only.
- No active standards were edited, so no active-corpus docs gate ran.
- If a future promotion claims repository automation for table parsing, anchor validation, or fence-label validation, source or tool proof must name the configured validator.
