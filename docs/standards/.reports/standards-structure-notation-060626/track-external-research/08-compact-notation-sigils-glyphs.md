Question: Which compact notation systems should inform serious technical Markdown rules for bracket tokens, ASCII sigils, glyph alphabets, progress bars, text graphics, and emoji avoidance?
Type: standards-research
Lane: track-external-research
Merge key: `formatting.md :: compact glyph notation :: external source decision`
Target owner: `docs/standards/formatting.md`, with form boundaries in `docs/standards/information-structure.md`
Source basis: official GitHub/GFM docs, W3C WCAG 2.2, Microsoft Style Guide emoji guidance, Unicode 17.0 charts, Org manual, Conventional Commits, Keep a Changelog, SemVer, and todo.txt format docs accessed 2026-06-06
Promotion target: `docs/standards/formatting.md`
Outcome: HOLD

## [FINDINGS]

### [1][DECISION]

The current Rasm direction is correct: use terse text notation only when it is a closed alphabet with a declared job, and drop notation that is decorative, emoji-like, or source-clever without a reader action. External sources support bracketed checkboxes, GitHub alert labels, uppercase normative tokens, version/change sigils inside their own grammars, and monospace block/box glyphs when the meaning is recoverable from text. They do not support a broad global glyph alphabet whose symbols carry unstated semantics.

The best promotion path is a minimal global rule: keep full bracket tokens for shared status/result/change families; keep `[x]` as the only globally mapped compact fail marker unless the user chooses a wider alphabet; require every other compact glyph alphabet to be locally declared before first use. This preserves the user's preference for terse technical indicators without letting `[?]`, `[~]`, or `[$]` become author-local folklore.

### [2][OPTION_CATALOG]

Promote these systems only where their grammar and owner are explicit:

| [INDEX] | [SYSTEM] | [SOURCE_MODEL] | [USE_IN_RASM] | [RECOMMENDATION] | [OWNER_ROUTE] |
| :-----: | :------- | :------------- | :------------ | :--------------- | :------------ |
|   [1]   | GFM task checkbox | `- [ ]` and `- [x]` render as semantic task checkboxes in GitHub-flavored Markdown. | Completion assertions in checklists only. | PROMOTE | `information-structure.md` checklists; `formatting.md` `[x]` boundary |
|   [2]   | GitHub alert labels | `> [!NOTE]`, `> [!TIP]`, `> [!IMPORTANT]`, `> [!WARNING]`, `> [!CAUTION]` are renderer-specific blockquote callouts. | Interrupting callouts only, not inline status or instruction weighting. | PROMOTE | `information-structure.md` callouts; `formatting.md` alert surface |
|   [3]   | Full bracket status tokens | Closed uppercase words such as `[PASS]`, `[FAIL]`, `[ADDED]`, `[BLOCKED]`. | Shared result, change, lifecycle mirror, and explicit runtime state where filtering matters. | PROMOTE | `formatting.md` token families |
|   [4]   | BCP 14 uppercase keywords | Uppercase requirement words have defined meaning only when declared by the document. | Inspiration for invocation markers and requirement strength, not a new token family. | HOLD | `formatting.md`; `style-guide.md` |
|   [5]   | Keep a Changelog categories | `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security` group change types for humans. | Supports current change-token family; do not import release-history shape. | PROMOTE | `formatting.md`; release docs route |
|   [6]   | Conventional Commits sigils | `type(scope)!:` and `BREAKING CHANGE:` encode breaking-change attention inside commit grammar. | Evidence that `!` can mean exceptional attention only inside a declared grammar. | HOLD | `formatting.md` local glyph alphabets |
|   [7]   | SemVer hyphen and plus | `-` marks prerelease identifiers and `+` marks build metadata only inside version strings. | Drop as general doc sigils; keep only inside version grammar. | DROP | package/version owners |
|   [8]   | todo.txt sigils | `(A)`, `@context`, and `+project` encode plaintext task metadata. | Useful precedent for plain-text task metadata, but too task-format-specific for global Markdown notation. | HOLD | task standards if Markdown task planning expands |
|   [9]   | Org progress cookies | `[n/m]` and `[%]` summarize checkbox or TODO completion in an outline. | Keep as research support for count/percentage basis; do not replace Rasm's 20-cell bar. | HOLD | `information-structure.md` progress basis |
|  [10]   | Unicode block elements | `█`, `░`, and related block elements are standard Unicode glyphs. | Progress cells and tiny dense state graphics with ASCII fallback and adjacent text basis. | PROMOTE | `formatting.md` progress and glyph rules |
|  [11]   | Unicode box drawing | `├──`, `└──`, `│`, and box outlines are standard Unicode glyphs. | File trees, codemap trees, and small source-readable stacks when alignment is load-bearing. | PROMOTE | `information-structure.md` monospace text |
|  [12]   | Emoji and emoticons | Emoji can have screen-reader names but are informal and not always supported; emoticons are weaker for assistive tech. | Reject for serious technical Markdown and never use as bullets, status, proof, or lifecycle markers. | DROP | `formatting.md` rejection list |

### [3][GLYPH_CHOICES]

The unresolved choice is not whether glyphs are allowed. They are already justified for density, progress, hierarchy, alignment, and comparison. The unresolved choice is how much meaning should be globally reserved.

[RECOMMENDED_DEFAULT]:
- Global compact map: `[x]` only, meaning compact fail/rejected/invalid where a full `[FAIL]` cell is too wide.
- Everything else: local alphabet declared before first use, with a text equivalent when meaning is not recoverable from row/column labels.
- Reason: this matches the external pattern. Bracketed checkboxes, alerts, BCP 14 terms, Conventional Commits, SemVer, todo.txt, and Org cookies all work because their symbols mean something inside a declared grammar, not everywhere.

[USER_CHOICE_A_GLOBAL_MAP]:
- `[o]`: proven/pass/ok.
- `[x]`: failed/rejected/invalid.
- `[!]`: attention/risk/manual review.
- `[?]`: unknown/proof gap.
- `[+]`: added/introduced.
- `[-]`: removed/skipped-off/absent by action.
- `[=]`: unchanged/same.
- `[/]`: partial/incomplete.
- `[~]`: approximate/provisional.
- `[$]`: cached/saved artifact.

Use this only if the user wants a repo-wide terse alphabet. If promoted, the active standard must state one meaning per glyph, reject synonyms in one column, and require full tokens where a column cannot stay dense.

[USER_CHOICE_B_LOCAL_ONLY]:
- Keep `[x]` global because current formatting already assigns it.
- Remove the global compact glyph row or rewrite it as "local compact glyph alphabets may use bracketed single characters after declaration."
- Require a local legend with `Glyph`, `Meaning`, `Text equivalent`, and `Removal or close rule` before first production use.

This is the strongest correctness option. It preserves terse technical notation while preventing a reader from guessing whether `[~]` means approximate, deferred, changed, unstable, or fuzzy.

[USER_CHOICE_C_ASCII_FIRST]:
- Prefer ASCII bracket glyphs for status tables: `[x]`, `[!]`, `[?]`, `[+]`, `[-]`, `[=]`, `[/]`, `[~]`.
- Reserve Unicode block and box drawing for progress bars, codemap trees, and aligned text graphics only.
- Require ASCII fallback for every Unicode progress or tree alphabet when the target surface may be copied into terminals, issue trackers, or renderers with uncertain fonts.

This fits maximum portability, but it loses the user's preferred niche glyph density in source-readable codemaps.

### [4][ASCII_FALLBACKS]

Rasm should distinguish fallback alphabets from alternate meanings:

| [INDEX] | [JOB] | [UNICODE] | [ASCII_FALLBACK] | [RULE] |
| :-----: | :---- | :-------- | :--------------- | :----- |
|   [1]   | Progress filled cell | `█` | `#` | Same numerator and fill calculation. |
|   [2]   | Progress remaining cell | `░` | `-` | Same denominator and closure rule. |
|   [3]   | File tree branch | `├──` | `|--` | Use only in monospace fences. |
|   [4]   | File tree final branch | `└──` | `` `--`` | Use only when backtick does not confuse Markdown parsing inside a fence. |
|   [5]   | File tree riser | `│` | `|` | Preserve column alignment exactly. |
|   [6]   | Stack frame | `┌─┐`, `├─┤`, `└─┘` | `+-+`, `| |` | Use only when the box carries layer or boundary meaning. |
|   [7]   | Edge arrow | `->` or `=>` | same | Prefer ASCII arrows over decorative Unicode arrows in Markdown prose. |

Do not create two progress standards. The Unicode and ASCII bars are the same representation with different glyph alphabets. The adjacent proof basis still owns numerator, denominator, percentage, and closure.

### [5][ACCESSIBILITY]

WCAG 2.2 supports the existing rule that glyph meaning cannot be purely visual when it carries content. A glyph, progress bar, codemap, or text graphic that changes reader action needs a text equivalent or adjacent fields that make the same state recoverable without shape, color, or decoration. Microsoft emoji guidance also supports rejecting emoji for serious technical content: even where emoji have accessibility names, meaning must still work without them and serious topics should not rely on them.

For Rasm, the accessible representation rule should be:
- Full token first when clarity matters: `[PASS]` beats `[o]` in narrative, reports, and non-dense tables.
- Compact glyph only in dense cells where the row/column headers and local legend make the equivalent text obvious.
- Progress bar only after the proof basis states numerator, denominator, closure, and proof surface.
- Text graphic only when alignment encodes hierarchy, state, or topology; otherwise use a list, table, record, Mermaid diagram, or prose.
- Emoji, emoticons, ornamental separators, and source banners are rejected because they do not add machine-readable status, proof, or route information.

## [EVIDENCE]

[PRIMARY_SOURCES]:
- GitHub Docs, [Basic writing and formatting syntax](https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax), accessed 2026-06-06. Supports GitHub task lists, footnotes, alerts, nested-list alignment, and alert limits.
- GitHub, [GitHub Flavored Markdown Spec](https://github.github.com/gfm/), accessed 2026-06-06. Defines task-list item marker shape and semantic checkbox rendering.
- W3C, [Web Content Accessibility Guidelines 2.2](https://www.w3.org/TR/WCAG22/), accessed 2026-06-06. Supports text alternatives for non-text content and not relying on visual presentation alone.
- Microsoft Learn, [emoji, emoticons](https://learn.microsoft.com/es-es/style-guide/a-z-word-list-term-collections/e/emoticons-emoji), accessed 2026-06-06. Supports not using emoji for serious topics, not replacing words with emoji, and ensuring meaning works without emoji.
- Unicode Consortium, [Box Drawing, Unicode 17.0](https://www.unicode.org/charts/PDF/U2500.pdf), accessed 2026-06-06. Confirms the box-drawing block used by file trees and source-readable frames.
- Unicode Consortium, [Block Elements, Unicode 17.0](https://www.unicode.org/charts/PDF/U2580.pdf), accessed 2026-06-06. Confirms block glyphs used by progress cells.
- Org Mode, [Checkboxes](https://orgmode.org/manual/Checkboxes.html), accessed 2026-06-06. Supports bracketed checkboxes, partial checkbox state, and count/percentage progress cookies inside a declared outline grammar.
- Conventional Commits, [1.0.0 specification](https://www.conventionalcommits.org/en/v1.0.0/), accessed 2026-06-06. Supports `!` only as a breaking-change attention marker inside commit-message grammar.
- Keep a Changelog, [1.1.0](https://keepachangelog.com/en/1.1.0/), accessed 2026-06-06. Supports grouped change categories for human-readable release notes.
- Semantic Versioning, [2.0.0](https://semver.org/), accessed 2026-06-06. Supports `-` and `+` only inside version grammar.
- todo.txt, [format rules](https://github.com/todotxt/todo.txt), accessed 2026-06-06. Supports terse plaintext task metadata such as priority, context, and project sigils only inside the todo.txt grammar.

[LOCAL_EVIDENCE_EXTENDED]:
- `track-formatting-notation/02-formatting-cross-corpus-usage.md` already found the compact glyph alphabet is globally listed but unmapped except for `[x]`.
- `track-repo-markdown/01-claude-markdown-patterns.md` already found prompt and skill glyphs that encode sequence or placeholders but lack declared alphabets and accessibility equivalents.
- `docs/standards/formatting.md` already rejects emojis, checkmark/crossmark substitution, ornamental frames, terminal animations, and decorative glyphs.
- `docs/standards/information-structure.md` already requires progress basis before progress rendering and limits monospace text to load-bearing hierarchy, order, state, progress, dependency, alignment, or comparison.

## [RECOMMENDATIONS]

[PROMOTE]:
- Keep full bracketed status/result/change tokens as the global, machine-filterable notation layer.
- Keep GitHub alert bracket labels only as callout block syntax.
- Keep GFM task checkboxes only for completion assertions; do not use `[x]` as a checkbox substitute outside `- [x]`.
- Keep Unicode block elements and box drawing as permitted alphabets for progress, codemap trees, and small text graphics, with ASCII fallback rules.
- Add or retain a text-equivalent requirement for glyph alphabets, progress bars, and text graphics whose meaning is not already carried by adjacent text.

[DROP]:
- Drop emoji, emoticons, checkmark/crossmark glyphs, ornamental separators, FIGlet-style banners, and decorative source frames as active documentation notation.
- Drop SemVer `-` and `+` as general documentation sigils; they remain only version-string grammar.
- Drop Conventional Commit `!` as a global "important" marker; it remains evidence that `!` needs declared local grammar.

[HOLD]:
- Hold the global compact glyph map until the user chooses one of the glyph policies above.
- Hold todo.txt-style `@context` and `+project` sigils unless a Markdown task-planning surface needs declared plaintext metadata.
- Hold Org-style `[n/m]` or `[%]` progress cookies as research only; Rasm's current 20-cell progress bar is clearer once backed by numerator, denominator, closure, and proof.

## [CANDIDATE_WORDING]

Candidate wording for `docs/standards/formatting.md` if the minimal-global policy is chosen:

```markdown template
Compact glyphs are local alphabets unless this section assigns a global meaning. `[x]` is the only global compact glyph and means fail, rejected, or invalid where a full `[FAIL]` token is too wide. Every other single-character bracket glyph, such as `[o]`, `[!]`, `[?]`, `[+]`, `[-]`, `[=]`, `[/]`, `[~]`, or `[$]`, must be declared before first use with one meaning, a text equivalent, and a close or removal rule. Use full bracket tokens when clarity matters more than cell width.
```

Candidate wording if the user chooses a global map:

```markdown template
Compact glyphs use this closed global map: `[o]` proven, `[x]` failed, `[!]` attention, `[?]` unknown, `[+]` added, `[-]` removed, `[=]` unchanged, `[/]` partial, `[~]` approximate, `[$]` cached. A local surface may narrow the set but must not remap a global glyph. If a row or column needs a different meaning, use a full token or declare a local alphabet with different glyphs.
```

Candidate wording for `docs/standards/information-structure.md`:

```markdown template
A glyph legend is required when a compact symbol carries meaning not already present in row headers, column headers, adjacent fields, or visible prose. The legend is a record or short table with `Glyph`, `Meaning`, `Text equivalent`, and `Close when` fields. Omit the legend only for globally defined tokens and progress bars whose basis record already supplies the equivalent text.
```

## [PROOF_GAPS]

- User choice remains required for the global compact glyph alphabet versus local-declare-only policy.
- No active standards were edited in this report, so no active-corpus validation or token scan was run.
- Renderer behavior was sourced from maintained external documentation, not locally rendered in GitHub.
- Screen-reader behavior for individual glyph strings was not tested locally; accessibility recommendations rely on WCAG text-alternative principles and Microsoft emoji guidance.
