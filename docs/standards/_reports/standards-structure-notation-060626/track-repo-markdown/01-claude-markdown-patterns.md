Question: Which `.claude/**` Markdown patterns should inform standards for notation, prompt contracts, skill files, and report-shaped records without becoming active authority themselves?
Type: repo-scan
Lane: track-repo-markdown
Merge key: docs/standards :: .claude markdown notation and structure patterns :: catalogue and route
Target owner: docs/standards/information-structure.md; docs/standards/formatting.md; docs/standards/proof.md
Source basis: repo proof from full reads of `.claude/README.md` and `.claude/prompts/*.md`; sampled `.claude/skills/**/*.md`; pattern sweeps with `fd` and `rg`
Promotion target: none yet; route durable rules to the target owner paths above
Outcome: PROMOTE

## [FINDINGS]

### [F1][PROMPT_CONTRACT_SHAPE]

Source set: full read of `.claude/README.md`; full read of `.claude/prompts/assay-ab-test-session.md`, `.claude/prompts/assay-closeout-session.md`, `.claude/prompts/assay-refine-session.md`, `.claude/prompts/fix-standards-docs.md`, and `.claude/prompts/tool-rebuild.md`; sampled skill entry files and reference/template files listed in `[EVIDENCE]`.
Report file: `docs/standards/_reports/standards-structure-notation-060626/track-repo-markdown/01-claude-markdown-patterns.md`
Path and line/heading: `.claude/prompts/fix-standards-docs.md:72` `[5.1][RETURN_CONTRACT]`; `.claude/prompts/fix-standards-docs.md:136` `[6][MAIN_AGENT_SYNTHESIS]`; `.claude/prompts/tool-rebuild.md:30` stage list
Evidence: The standards prompt requires source set, report path, file findings, line or heading, evidence, weakness, correction, owner, ripple files, decision, and proof gap; the synthesis prompt adds `Task`, `Owner file`, `Section`, `Axis`, `Source basis`, `Decision`, `Correction`, `Ripple files`, `Proof gap`, `Validation`, and `Status`.
Weakness: The prompt corpus has a useful task-output contract, but its wave, phase, and sub-agent language is task-process shape. If promoted directly, it would leak transient orchestration into durable standards.
Correction: Promote only the record schema principle: source scans and repo scans need a compact field packet with source basis, exact source span, correction route, ripple files, decision, and proof gap. Keep wave counts, worker roles, and implementation choreography local to prompt assets.
Owner: for task-output contracts; `information-structure.md` for record shape; `_reports/AGENTS.md` for report-specific field order.
Ripple files: `.claude/prompts/fix-standards-docs.md`, future `.claude/prompts/*.md`, `docs/standards/_reports/**/README.md` if the session manifest is created.
Decision: PROMOTE
Proof gap/question: No session manifest exists in `docs/standards/_reports/standards-structure-notation-060626/`; the archive entrypoint gap should be handled by the session owner, not by this report.

### [F2][CODE_FENCE_INTENT_LABELS]

Source set: full prompt read plus ````rg -n '^```' .claude````.
Report file: `docs/standards/_reports/standards-structure-notation-060626/track-repo-markdown/01-claude-markdown-patterns.md`
Path and line/heading: `.claude/prompts/fix-standards-docs.md:20`, `.claude/prompts/fix-standards-docs.md:26`, `.claude/prompts/fix-standards-docs.md:253`; `.claude/prompts/assay-ab-test-session.md:13`; `.claude/skills/testing-cs/SKILL.md:119`; `.claude/skills/testing-cs/templates/unit-pbt.spec.template.md:9`
Evidence: The standards prompt uses `text template` fences for paths and report locations. Assay prompts use runnable `bash` fences for gates. Skill references and templates use language fences such as `bash`, `csharp`, `ts`, `typescript`, `yaml`, `json`, `dockerfile`, and `mermaid`.
Weakness: `.claude` proves that intent labels already separate fill-in templates from runnable commands, but most skill reference code fences are unlabeled language-only examples. That is acceptable as source material but would violate the active standards rule for ordinary code fences if copied into active docs.
Correction: Standards should preserve the distinction between `text template` path/record packets and runnable `bash copy-safe` commands. When `.claude` skill examples ripple after standards change, add intent labels to non-renderer fences or add visible prose for renderer-local fences.
Owner: `information-structure.md` `[11][CODE_BLOCKS]`; `formatting.md` validation for fence rendering.
Ripple files: `.claude/prompts/fix-standards-docs.md`; `.claude/skills/*/SKILL.md`; `.claude/skills/**/references/*.md`; `.claude/skills/**/templates/*.md`.
Decision: PROMOTE
Proof gap/question: Current `.claude` code fences were catalogued by source search only; no renderer or lint gate was run for every fence.

### [F3][STATUS_LIFECYCLE_VOCABULARY]

Source set: full prompt read; sampled `testing-ts`, `testing-cs`, `coding-*`, and report overlay files.
Report file: `docs/standards/_reports/standards-structure-notation-060626/track-repo-markdown/01-claude-markdown-patterns.md`
Path and line/heading: `.claude/prompts/fix-standards-docs.md:84`; `.claude/prompts/fix-standards-docs.md:169`; `.claude/skills/testing-ts/SKILL.md:202`; `.claude/skills/coding-bash/references/bash-logging.md:218`
Evidence: The report prompt uses `PROMOTE`, `CORRECT`, `MERGE`, `DROP`, and `HOLD`. Testing TS uses `Active` and `Always` in a `[STATUS]` table. Bash logging examples emit `[PASS]`, `[FAIL]`, and `[SKIP]`.
Weakness: `.claude` mixes report decisions, lifecycle statuses, runtime states, and gate result tokens without always declaring which family owns a token. The tokens are useful, but not one vocabulary.
Correction: Active standards should keep separate families: report outcome decisions in `_reports/AGENTS.md`, lifecycle records in `information-structure.md`, inline result markers in `formatting.md`, and local domain statuses only when a type standard declares the closed set.
Owner: `_reports/AGENTS.md` for report outcomes; `information-structure.md` for lifecycle fields; `formatting.md` for inline result markers.
Ripple files: `.claude/prompts/fix-standards-docs.md`, `.claude/skills/testing-ts/SKILL.md`, `.claude/skills/coding-bash/references/bash-logging.md`.
Decision: PROMOTE
Proof gap/question: No current-source research needed; this is repository-local notation, not provider behavior.

### [F4][BRACKET_MARKERS_AND_HEADING_IDIOM]

Source set: full `.claude` prompt read; sampled skill entries and references.
Report file: `docs/standards/_reports/standards-structure-notation-060626/track-repo-markdown/01-claude-markdown-patterns.md`
Path and line/heading: `.claude/README.md:3`; `.claude/prompts/fix-standards-docs.md:5`; `.claude/skills/coding-python/SKILL.md:11`; `.claude/skills/testing-cs/SKILL.md:15`; `.claude/skills/coding-bash/references/array-operations.md:1`
Evidence: `.claude/README.md` uses `## [1][SESSION_START]`. The standards prompt uses `## [1][OUTCOME]`. Several skill files use `# [H1][CODING-PYTHON]`, `# [H1][TESTING_CS]`, or `# [H1][ARRAY-ALGEBRA]`, while some skill sections use sentence headings such as `## Paradigm`.
Weakness: `.claude` carries both the current standards heading idiom and an older `# [H1][NAME]` heading theater pattern. The older form is source evidence for a migration target, not a standard.
Correction: Standards should keep one bracketed heading grammar and name `.claude/skills/*` as ripple targets for removing `# [H1][...]` and mixed sentence-style section headings when `.claude` is normalized.
Owner: `formatting.md` heading idiom.
Ripple files: `.claude/skills/coding-python/SKILL.md`, `.claude/skills/coding-ts/SKILL.md`, `.claude/skills/coding-csharp/SKILL.md`, `.claude/skills/coding-bash/SKILL.md`, `.claude/skills/testing-cs/SKILL.md`, `.claude/skills/testing-ts/SKILL.md`, `.claude/skills/**/references/*.md`.
Decision: CORRECT
Proof gap/question: Heading anchor impact was not checked; a ripple pass needs an in-repo link and anchor validation.

### [F5][GLYPHS_SIGILS_AND_PROGRESS]

Source set: full prompt read; skill samples; pattern sweep for arrows, comparison glyphs, compact markers, and progress examples.
Report file: `docs/standards/_reports/standards-structure-notation-060626/track-repo-markdown/01-claude-markdown-patterns.md`
Path and line/heading: `.claude/prompts/assay-ab-test-session.md:25`; `.claude/prompts/assay-refine-session.md:14`; `.claude/skills/mermaid-diagramming/SKILL.md:34`; `.claude/skills/mermaid-diagramming/SKILL.md:76`; `.claude/skills/testing-ts/templates/unit-pbt.spec.template.md:3`
Evidence: Prompt prose uses arrow glyphs for field transformations and sequencing, a greater-or-equal sign for parity, multiplication signs in matrix labels, and ellipsis placeholders. Mermaid skill links use arrow-prefixed link text. Diagram type tables use dash placeholders in direction cells. Testing templates use box-drawing HTML comments as source-only banners.
Weakness: The glyphs mostly encode sequence, transformation, or placeholders, but `.claude` has no local declaration of glyph alphabets or accessibility equivalents. The TS template box frames are visual scaffolding for source authors and risk becoming decorative if copied into active docs.
Correction: Promote a rule that glyphs in prompt/source material may stay local when they aid author scanning, but active standards examples need declared alphabets or plain-text equivalents. Treat box-drawing banners as source-only template comments, not documentation pattern.
Owner: `formatting.md` glyph rules; artifact separation.
Ripple files: `.claude/prompts/*.md`, `.claude/skills/mermaid-diagramming/SKILL.md`, `.claude/skills/testing-ts/templates/*.md`.
Decision: HOLD
Proof gap/question: User choice remains on how much `.claude` prompt ergonomics should be ASCII-normalized versus preserved as Claude-oriented prompt notation.

### [F6][TABLE_AND_RECORD_DENSITY]

Source set: full prompt read; sampled skill entries and references; table pattern sweep across `.claude/skills/**`.
Report file: `docs/standards/_reports/standards-structure-notation-060626/track-repo-markdown/01-claude-markdown-patterns.md`
Path and line/heading: `.claude/skills/coding-python/SKILL.md:37`; `.claude/skills/testing-cs/SKILL.md:29`; `.claude/skills/testing-cs/SKILL.md:74`; `.claude/skills/coding-python/references/validation.md:60`; `.claude/skills/testing-ts/templates/unit-pbt.spec.template.md:56`
Evidence: Skill files repeatedly use `[INDEX]` tables for libraries, rails, spec shape, detection heuristics, placeholder references, and tool routes. Validation references use table rows as executable search records, for example `G1` through `G18`.
Weakness: The skill corpus shows good lookup-table pressure but also has inconsistent header rubrics (`[IDX]`, `[INDEX]`, `[TASK]`, bold headers) and some wide tables with prose-heavy cells. The tables are valuable source material for standards examples, not all active-compliant tables.
Correction: Promote `.claude` skill tables as examples of lookup and decision-table use, but normalize header rubrics, index labels, width, and prose columns during ripple edits. Detection heuristic tables should become a named record/table pattern in `information-structure.md`.
Owner: `information-structure.md` table and record rules; `formatting.md` table styling.
Ripple files: `.claude/skills/**/SKILL.md`, `.claude/skills/**/references/*.md`, `.claude/skills/**/templates/*.md`.
Decision: PROMOTE
Proof gap/question: No table parser was run; a ripple pass should validate cell counts and escaped pipes.

### [F7][XML_TAGS_AND_SOURCE_MATERIAL_BOUNDARIES]

Source set: full prompt read; sampled skill files; search for XML-like tags and Markdown comments.
Report file: `docs/standards/_reports/standards-structure-notation-060626/track-repo-markdown/01-claude-markdown-patterns.md`
Path and line/heading: `.claude/prompts/tool-rebuild.md:21`; `.claude/skills/testing-ts/templates/unit-pbt.spec.template.md:3`; `.claude/README.md:5`; `.claude/prompts/fix-standards-docs.md:47`
Evidence: Tool rebuild prompts use placeholder path tags such as `<tool>`, `<layer>`, and `<file>`. TS templates use HTML comments as source-only placeholder ledgers. `.claude/README.md` describes startup hook boundaries. The standards prompt explicitly treats relevant `.claude/**` Markdown as source material for the standards pass.
Weakness: Placeholder angle brackets and HTML comments serve different jobs, but both look like XML-ish notation in searches. Without a boundary rule, a future standards edit could overgeneralize prompt placeholders into a documentation delimiter rule.
Correction: Route angle-bracket placeholders to code-block/template intent rules, HTML comments to source-view notation, and provider prompt XML delimiter guidance to only when a provider target requires it.
Owner: provider behavior and artifact separation; `formatting.md` hidden comments; `information-structure.md` code-block intent labels.
Ripple files: `.claude/prompts/tool-rebuild.md`, `.claude/skills/testing-ts/templates/*.md`, `.claude/README.md`.
Decision: PROMOTE
Proof gap/question: No current provider research was run because no XML delimiter behavior is being promoted as a current provider claim in this report.

### [F8][DIAGRAM_AND_PROVIDER_CLAIMS]

Source set: sampled `mermaid-diagramming` skill entry and reference patterns; active proof standard read for renderer/provider evidence routing.
Report file: `docs/standards/_reports/standards-structure-notation-060626/track-repo-markdown/01-claude-markdown-patterns.md`
Path and line/heading: `.claude/skills/mermaid-diagramming/SKILL.md:4`; `.claude/skills/mermaid-diagramming/SKILL.md:16`; `.claude/skills/mermaid-diagramming/SKILL.md:60`; `.claude/skills/mermaid-diagramming/SKILL.md:100`
Evidence: The Mermaid skill claims Mermaid v11+, YAML frontmatter, ELK layout, Dracula palette, 22 diagram types, deprecated init directives, and beta diagram statuses.
Weakness: These are provider and renderer behavior claims. They may be true locally, but they cannot promote into active standards without maintained upstream or local renderer proof. The corpus is useful for diagram notation categories, not as proof of current Mermaid behavior.
Correction: Keep diagram taxonomy and validation checklist shape as source material. Before active standards standardize any Mermaid capability claim, require current maintained Mermaid documentation or local `mmdc` render proof, then record `Last verified` and `Review trigger`.
Owner: `proof.md` for provider/renderer proof; `information-structure.md` for diagram container choice; `formatting.md` for renderer fence exceptions.
Ripple files: `.claude/skills/mermaid-diagramming/SKILL.md`, `.claude/skills/mermaid-diagramming/references/*.md`, future diagram examples in active standards.
Decision: HOLD
Proof gap/question: Current-source research not run; required only if Mermaid capability claims are promoted.

### [F9][EXAMPLES_AND_COLOCATION_DEFECTS]

Source set: sampled TypeScript, C#, Python, Bash, and testing skill references/templates.
Report file: `docs/standards/_reports/standards-structure-notation-060626/track-repo-markdown/01-claude-markdown-patterns.md`
Path and line/heading: `.claude/skills/coding-ts/references/surface.md:84`; `.claude/skills/coding-ts/references/surface.md:130`; `.claude/skills/testing-cs/templates/unit-pbt.spec.template.md:144`; `.claude/skills/coding-python/references/validation.md:58`
Evidence: TS surface reference places code examples directly beside integration contracts. C# test template puts variants and generator rules in one template. Python validation pairs detection heuristic rows with fixes and quick-reference rows.
Weakness: This is a strong co-location pattern: examples, rule rationale, detection, and fix live close together. Some active standards still risk separating "what shape" from "when to use" and "what to reject" too far apart if they abstract this into prose-only guidance.
Correction: Promote co-located example packets: each reusable structure should carry accepted shape, rejected shape or failure mode, when to use, and ripple/validation near the example. Avoid copying huge skill examples; extract minimal pattern rows.
Owner: `information-structure.md` examples, records, and co-location rules; `style-guide.md` for prose density when examples are shortened.
Ripple files: `.claude/skills/coding-ts/references/surface.md`, `.claude/skills/testing-cs/templates/unit-pbt.spec.template.md`, `.claude/skills/coding-python/references/validation.md`.
Decision: PROMOTE
Proof gap/question: Exact active-standard candidate wording belongs in a later wording-packet report.

### [F10][SKILL_FILE_NORMALIZATION]

Source set: sampled `SKILL.md` files: `coding-python`, `coding-ts`, `coding-csharp`, `coding-bash`, `testing-cs`, `mermaid-diagramming`; pattern sweep over all `.claude/skills/*/SKILL.md`.
Report file: `docs/standards/_reports/standards-structure-notation-060626/track-repo-markdown/01-claude-markdown-patterns.md`
Path and line/heading: `.claude/skills/coding-python/SKILL.md:1`; `.claude/skills/coding-python/SKILL.md:95`; `.claude/skills/coding-ts/SKILL.md:125`; `.claude/skills/coding-csharp/SKILL.md:124`; `.claude/skills/testing-cs/SKILL.md:15`; `.claude/skills/mermaid-diagramming/SKILL.md:31`
Evidence: Most skill entry files start with YAML frontmatter, then H1, principles or scope, conventions, contracts, load sequence, validation, eval prompts, and first-class libraries/tools. Testing and diagram skills use numbered bracket sections; coding skills use sentence headings.
Weakness: Skill files have a de facto produced-document structure but no repo-local standard for skill anatomy. Mixed heading idioms and anti-pattern section names make future ripple edits harder.
Correction: Keep `.claude/skills/**` as local source material until standards define a reusable skill anatomy. Then ripple skill files to one structure: frontmatter, scope/use, governing principles, contracts, load sequence, validation, eval prompts, first-class dependencies, boundaries.
Owner: for machine-facing skill surface; `information-structure.md` for produced-document structure; `formatting.md` for headings.
Ripple files: `.claude/skills/*/SKILL.md`.
Decision: HOLD
Proof gap/question: Provider-specific Claude skill file requirements were not checked against current Claude Code documentation; research is required before changing skill frontmatter semantics.

## [EVIDENCE]

Full-read files:
- `.claude/README.md`
- `.claude/prompts/assay-ab-test-session.md`
- `.claude/prompts/assay-closeout-session.md`
- `.claude/prompts/assay-refine-session.md`
- `.claude/prompts/fix-standards-docs.md`
- `.claude/prompts/tool-rebuild.md`

Sampled skill files:
- `.claude/skills/coding-python/SKILL.md`
- `.claude/skills/coding-ts/SKILL.md`
- `.claude/skills/coding-csharp/SKILL.md`
- `.claude/skills/coding-bash/SKILL.md`
- `.claude/skills/testing-cs/SKILL.md`
- `.claude/skills/mermaid-diagramming/SKILL.md`
- `.claude/skills/coding-python/references/validation.md`
- `.claude/skills/coding-ts/references/surface.md`
- `.claude/skills/testing-cs/templates/unit-pbt.spec.template.md`

Pattern sweeps:
- `fd -e md . .claude/prompts`
- `fd -e md . .claude/skills`
- ````rg -n '^```' .claude````
- `rg -n '^---$|^name:|^description:|^## Load sequence|^## Validation gate|^## Skill eval prompts|^## Anti-Patterns|^## Anti Patterns|^## First-class|^\\| \\[INDEX\\]|^\\[CRITICAL\\]:|^\\[IMPORTANT\\]|^\\[VERIFY\\]' .claude/skills`

## [RECOMMENDATIONS]

1. Promote the source-scan record shape, but strip wave and worker-role narration before any active-standard edit.
2. Keep `PROMOTE`, `CORRECT`, `MERGE`, `DROP`, and `HOLD` as report decisions, not general lifecycle states.
3. Normalize `.claude` heading and table notation only after active standards settle the bracketed heading, table-rubric, and code-fence intent rules.
4. Treat provider and renderer claims in `.claude/skills/**` as proof gaps until current maintained docs or local renderer/tool output verifies them.
5. Preserve `.claude` prompt assets as prompt assets: useful reusable source material, never authority above `CLAUDE.md`, `AGENTS.md`, or active standards.

## [PROOF_GAPS]

- The session folder lacks a `README.md` manifest despite `_reports/AGENTS.md` requiring one for multi-report sessions.
- No current provider research was run for Claude Code skill format, Mermaid renderer behavior, Context7, Exa, Perplexity, Tavily, Nx, or other tool-provider claims because this report does not promote those claims as current behavior.
- No Markdown table parser, link checker, anchor checker, or renderer gate was run across `.claude/**`; ripple implementation needs those checks or explicit proof gaps.
- No active standards were edited; this report is source material only.
