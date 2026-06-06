Question: Which notation choices in the collective task list are over-standardized, under-specified, or blocked by user choice before promotion?
Type: gap-critique
Lane: track-adversarial-synthesis
Merge key: `formatting.md :: notation promotion surface :: adversarial critique`
Target owner: `docs/standards/formatting.md`, with form ownership in `docs/standards/information-structure.md`
Source basis: `docs/standards/formatting.md`, `docs/standards/information-structure.md`, session manifest, `track-synthesis/00-collective-task-list.md`, `track-formatting-notation/01-formatting-token-systems.md`, `track-formatting-notation/02-formatting-cross-corpus-usage.md`, `track-external-research/08-compact-notation-sigils-glyphs.md`, and `track-external-research/09-code-fence-intent-labels.md`
Promotion target: `docs/standards/formatting.md`; `docs/standards/information-structure.md`; selected type standards after user choices close
Outcome: HOLD

## [FINDINGS]

| [INDEX] | [TASK_REFERENCE] | [JUDGMENT] | [CRITIQUE] | [CORRECTION_ROUTE] | [BLOCKER] |
| :-----: | :--------------- | :--------- | :--------- | :----------------- | :-------- |
|   [1]   | `track-synthesis/00-collective-task-list.md:300` `Resolve compact glyph semantics`; `:700` `Resolve compact glyph alphabet` | UNDER-SPECIFIED | The current global compact glyph row looks standardized but only `[x]` has a stable meaning. Promoting the whole row as global notation would create false precision for `[o]`, `[!]`, `[?]`, `[+]`, `[-]`, `[=]`, `[/]`, `[~]`, and `[$]`. | Keep `[x]` global only, or require a user-selected full map. If no user choice lands, rewrite compact glyphs as locally declared alphabets with `Glyph`, `Meaning`, `Text equivalent`, and `Close when`. | User choice: minimal global `[x]`, full global map, or ASCII-first local policy. Also decide whether `[?]` can mean proof gap instead of unknown value. |
|   [2]   | `track-synthesis/00-collective-task-list.md:314` `Add type-local bracket token and status registry pointer` | UNDER-SPECIFIED | The task is right that formatting is not the status registry, but the suffix case is still unresolved. `[ACTIVE M2]` can be a useful codemap/source-key projection; as a general status grammar it would become a second lifecycle language. | Promote only a boundary rule now: global token families live in `formatting.md`; lifecycle and domain status declarations live in `information-structure.md` and type standards. Hold suffix grammar unless constrained to codemap/source-key surfaces. | User or owner choice: codemap-only suffixes versus any declared type-local marker with a base-token projection. |
|   [3]   | `track-synthesis/00-collective-task-list.md:144` `Move contrast-record form ownership to information structure`; `:72` `Replace the accepted/rejected mini-table`; `:594` `Rename runbook contrast label or declare it local` | OVER-STANDARDIZED IN FORMATTING | `formatting.md` currently defines compact contrast record eligibility, field labels, and the table-vs-record choice. That is form ownership leaking into notation. | Move contrast-record eligibility to `information-structure.md`. Leave `formatting.md` with raw label spelling, colon spacing, indentation, and blank-line rules. Then normalize `Why rejected:` to `Reason:` unless runbook declares a local field. | Optional user choice only if contrast fields should stay type-local. Otherwise this is implementable. |
|   [4]   | `track-synthesis/00-collective-task-list.md:328` `Narrow the heading idiom scope`; `:536` `Correct contradictory README H1 example` | OVER-STANDARDIZED | The heading idiom says documentation and instruction files use bracketed headings, but the README standard intentionally allows plain public and registry README headings. The current global wording would incorrectly normalize externally facing README surfaces. | Limit bracketed heading idiom to repo-internal, standards-controlled documentation and instruction files unless a type standard declares an external-reader exception. Fix the README rejected example so it rejects `# [H1][ASSAY_OPERATOR]` or another genuinely forbidden shape. | None for heading scope. User choice only if the contradictory README example was meant to reject something other than heading-tier theater. |
|   [5]   | `track-synthesis/00-collective-task-list.md:342` `Add .claude/skills/** to invocation-marker surface`; `:728` `Resolve .claude governance boundary` | BLOCKED | Invocation markers are valid only on instruction surfaces. `.claude/skills/**` appears to be instruction material, but the task list also treats `.claude` as source material with legacy heading/table/fence notation. Promoting skill files into the invocation-marker surface without the governance choice would partially bless one marker family while leaving the rest of the corpus ambiguous. | Do not add `.claude/skills/**` to `formatting.md` until `.claude` governance is decided. If governed, normalize headings, table rubrics, code-fence intent labels, and invocation markers together. If not governed, leave `.claude` as source material and do not use it to widen active standards. | User choice: normalize `.claude` in this implementation or treat it as source material only. Provider skill-format research may also be needed. |
|   [6]   | `track-synthesis/00-collective-task-list.md:284` `Clarify ordinary code-fence grammar and intent vocabulary`; `:492` `Normalize pure field-packet fences by shape` | MOSTLY READY, WITH ONE GUARD | The `<language> <intent>` rule is well supported by the fence research. The under-specified part is validator posture: an allowed-intent list is different from a closed pair table. If implemented as only the current pair table, future real-language examples will be falsely blocked; if implemented as free text, intent drift returns. | Define a closed intent vocabulary and a currently-used pair list. Allow new pairs only when the first token is a real renderer/highlighter language and the second token is from the closed intent set. Keep renderer-local `mermaid` exact. Use `text template` for source-neutral label/value packets. | User choice only if Rasm wants a three-token grammar for subtype words such as `schema`, `transcript`, `fixture`, or `contract`. |
|   [7]   | `track-synthesis/00-collective-task-list.md:200` `Add large lookup, matrix, gate-matrix, and row-sidecar decomposition rules`; `:714` `Resolve row-sidecar label pattern` | BLOCKED | Row-sidecar labels are the highest-risk notation choice because they cross table cells, group labels, footnotes, anchors, and row-owned records. Standardizing `[GROUP] [INDEX]` too early creates a mini citation grammar; banning it too early may lose a useful source-scannable affordance for command catalogs and package matrices. | Hold shared rule text. Decide one of three forms: durable source-scannable sidecars, Markdown footnotes, or row-owned records. If sidecars win, `information-structure.md` owns eligibility and row-note decomposition; `formatting.md` owns rendering syntax only. | User choice: `[GROUP] [INDEX]` sidecars versus footnotes versus row-owned records. |
|   [8]   | `track-synthesis/00-collective-task-list.md:356` `Clarify absence values`; `:370` `Add text-graphic and ASCII fallback rules` | NEEDS OWNER SPLIT | Absence values and glyph fallbacks are notation-adjacent but not the same rule. Table-cell absence, omitted record fields, proof gaps, domain `none`, and source unknowns must not collapse into one token family. Unicode progress/tree glyphs also need fallback alphabets without changing meaning. | Add cross-owner boundaries: `formatting.md` renders table-cell absence and fallback alphabets; `information-structure.md` decides when records omit absent fields and when glyph legends/progress bases are required; `proof.md` owns proof gaps. | Decide whether support-matrix `n/a` shares the domain-none rule or stays support-specific. Screen-reader behavior remains untested; keep text-equivalent rule. |
|   [9]   | `track-synthesis/00-collective-task-list.md:520` `Preserve roadmap progress as canonical progress example`; `:506` `Fix architecture Mermaid source indentation` | DO NOT OVER-CORRECT | The roadmap progress example is a good owner split: roadmap defines numerator, denominator, closure, and proof map while formatting renders the bar. The architecture Mermaid indentation defect is local source formatting, not a reason to widen fence-label or renderer rules. | Keep roadmap progress as a positive example. Fix Mermaid indentation locally when active edits start, and run `mmdc` only if the final change claims render proof. | None. These are false-positive guards or local corrections, not policy blockers. |

## [EVIDENCE]

[ACTIVE_STANDARD_READS]:
- `docs/standards/formatting.md` defines token families, invocation markers, table rubrics, group labels, field-line spelling, heading idiom, anchor comments, and validation.
- `docs/standards/information-structure.md` defines container choice, structured records, tables, code-fence intent labels, monospace text, Mermaid, callouts, machine-consumed Markdown, and page anatomy.

[REPORT_READS]:
- `track-formatting-notation/01-formatting-token-systems.md` separates global token families from type-local statuses and flags false positives for matrix stubs, escaped pipes, progress examples, nested fences, and fenced heading placeholders.
- `track-formatting-notation/02-formatting-cross-corpus-usage.md` finds the unmapped compact glyph alphabet, contrast-record owner leak, README heading exception, invocation-marker `.claude/skills/**` gap, legacy prompt/skill packets, row-sidecar ambiguity, and status-vocabulary registry need.
- `track-external-research/08-compact-notation-sigils-glyphs.md` supports full bracket tokens globally, compact glyphs only inside declared grammar, Unicode box/block glyphs with text equivalents, and rejection of decorative emoji-like notation.
- `track-external-research/09-code-fence-intent-labels.md` supports `<language> <intent>` as local source metadata, with first-token language/highlighter semantics and renderer-local tags such as `mermaid` kept exact.

## [RECOMMENDATIONS]

[PROMOTE_NOW]:
- Heading scope: repo-internal bracketed heading idiom, with README public/registry exceptions routed to `docs/standards/reference/readme.md`.
- Code-fence grammar: ordinary fences use `<language> <intent>`; intent labels are local source metadata; renderer-local fences keep exact tags.
- Status registry pointer: `formatting.md` renders tokens, `information-structure.md` and type standards declare lifecycle/status semantics.
- Contrast record owner split: `information-structure.md` owns eligibility; `formatting.md` owns label rendering.

[HOLD_FOR_USER_CHOICE]:
- Compact glyph alphabet.
- Row-sidecar label pattern.
- `.claude` governance boundary and invocation-marker eligibility.
- Three-token fence grammar for subtype words.
- Support-matrix `n/a` versus domain-none boundary if the active edit touches support values.

[DROP_OR_GUARD]:
- Do not globalize task-local step markers such as `Optional:`, `Irreversible:`, or `Safe mutation: none`.
- Do not force `[INDEX]` onto non-enumerable matrices that use a real row-axis stub.
- Do not turn roadmap progress proof semantics into formatting rules.
- Do not bless legacy `.claude` heading packets such as `# [H1][NAME]`.

## [PROOF_GAPS]

- This report did not edit active standards and does not claim renderer proof.
- `git diff --check` is the only validation gate requested for this report.
- User-choice blockers remain for compact glyphs, row sidecars, `.claude` governance, and optional fence subtype grammar.
