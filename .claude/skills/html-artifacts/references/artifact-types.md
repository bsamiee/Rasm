# [ARTIFACT_TYPES]

A type is a fixed region order composing baseline tokens and named interaction patterns under one export rule. A type that captures or edits intent carries the export bar; a purely narrative type omits it and adds one only where the reader lifts state back into a document.

## [01]-[PLAN]

Trigger: a decision-complete implementation blueprint for one change — stages, acceptance, risk.

Regions in order:
- Header — title, one status chip, date slot.
- Sticky TOC — anchors to every `h2` below.
- Context — the problem and the constraints that bound it.
- Decisions — table of decision, ruling, rationale.
- Stages — one `.card` per stage, each with an ordered acceptance list.
- Risk register — table of risk, likelihood, impact, mitigation.
- Appendix — collapsible section holding a code excerpt.

Patterns: theme toggle, deep-link, collapsible, copy-to-clipboard. Export optional.

## [02]-[BRAINSTORM]

Trigger: N candidate directions weighed side-by-side before committing to one.

Regions in order:
- Header — title, direction count, date slot.
- Direction grid — one `.card` per direction with thesis line and strengths, costs, kills lists.
- Comparison control — highlights one criterion row across every card at once.
- Verdict footer — the leaning direction and the trigger that flips it.

Patterns: theme toggle, compare-highlight, deep-link. Export optional. A code-approaches variant carries an executable excerpt and a failure-mode line per direction.

## [03]-[ROADMAP]

Trigger: sequencing capability across horizons with dependency and status visible at a glance.

Regions in order:
- Header — title, status chip, date slot.
- Horizon swimlanes — `now`, `next`, `later` columns via `.grid`, milestone `.card`s inside each.
- Milestone card — title, status chip, dependency note lines.
- Timeline bar — one inline-SVG horizontal bar marking horizon boundaries.
- Filter box — live-narrows milestone cards by text.

Patterns: theme toggle, filter, deep-link, inline SVG. Export optional.

## [04]-[WARGAME]

Trigger: scoring options against weighted criteria and stress-testing each option's failure modes.

Regions in order:
- Header — title, criteria-weight legend, date slot.
- Scoring matrix — options as rows, criteria as columns, each cell an editable number; a live weighted-total column and a live best-row highlight.
- Risk cards — one `.card` per option enumerating failure modes.
- Export bar — export-to-markdown of the scored matrix.

Patterns: theme toggle, live totals, draft persist, export bar. Export required.

## [05]-[DIFF_REVIEW]

Trigger: a patch read for review — additions, removals, context — with critique beside the code.

Regions in order:
- Header — title, change summary, date slot.
- Diff spine — the patch as a preserved line stream on add, remove, and context classes; every line escaped before render.
- Annotation rail — sticky column of severity chips and jump anchors; each suggested fix sits beside the line it touches.
- Verdict footer — merge posture and the blocking findings.

Patterns: theme toggle, escape-first render, diff spine, deep-link. Export optional — a clarified-review markdown.

## [06]-[QUIZ]

Trigger: gating comprehension before merge — the reader proves grasp of the change, its risk, its tests, and its edge cases.

Regions in order:
- Header — title, change reference, score slot.
- Question cards — one `.card` per question with multiple-choice inputs and a per-answer reveal explaining the correct choice.
- Section links — each wrong answer links to the owning corpus page through an `#` anchor or relative sibling link.
- Score bar — the live score; export serializes it and re-import restores it.

Patterns: theme toggle, scored reveal, escape-first render, export bar. Export required.

## [07]-[BUY_IN]

Trigger: winning agreement on a built direction — lead with the demo, name the asks, pre-answer the objections.

Regions in order:
- Header — title, one-line thesis, date slot.
- Demo lead — the strongest artifact or result first.
- Objection cards — one `.card` per anticipated objection paired with its answer; a multi-audience variant carries one objection lane per audience.
- Ask and tradeoff list — the decision requested and the costs named.
- Sign-off list — named approvers, one state chip each.

Patterns: theme toggle, objection cards, export bar. Export required — a shareable sign-off message.

## [08]-[CAPABILITY_ATLAS]

Trigger: mapping a surface's full capability set — owners, operations, and how they compose.

Regions in order:
- Header — title, surface name, date slot.
- Sticky TOC — anchor per owner group.
- Owner sections — one `h2` per owner; under each a `.grid` of capability `.card`s stating operation, input shape, and output.
- Composition map — inline-SVG node-and-edge sketch of how owners stack, strokes on `--line`, active nodes on `--accent`.
- Coverage table — capability against status chip inside `.twrap`.

Patterns: theme toggle, filter, deep-link, inline SVG, keyboard grid nav. Export optional. A module-map variant adds entry, state, boundary, failure, and gotcha cards with exact file and function names.

## [09]-[EXPLAINER]

Trigger: walking a reader through a mechanism, review, or decision already made — narrative over interaction.

Regions in order:
- Header — title, subtitle, date slot.
- Sticky TOC — anchor per section.
- Prose sections — `h2`/`h3` at the content measure; inline `code` and `.chip` for verdicts.
- Sidenote rail — definitions and caveats float beside the claim, collapsing to block flow on narrow screens.
- Callout cards — one `.card` for the load-bearing claim or caveat per section.
- Evidence — collapsible sections holding code excerpts or data tables in `.twrap`.

Patterns: theme toggle, collapsible, deep-link, margin notes, copy-to-clipboard. Export optional.

## [10]-[DECK]

Trigger: walking a case slide by slide — one active section, keyboard-driven, still one searchable HTML file.

Regions in order:
- Slide sections — one semantic `<section>` per slide, one active at a time.
- Nav — arrow-key and button advance with a visible counter and progress.
- Speaker aside — an optional per-slide note hidden in present mode.

Patterns: theme toggle, deck state machine, keyboard nav, deep-link. Export optional — a slide outline markdown.

## [11]-[EDITOR]

Trigger: a micro-editor that captures decision rules and hands them back as clean markdown.

Regions in order:
- Header — title, draft-state note, date slot.
- Rule rows — repeating row of text inputs plus a select and a remove control; an add-row control.
- Changed-key diff — the live state compared against a frozen initial snapshot.
- Export bar — export-to-markdown of the rules table, plus clear-draft.

Patterns: theme toggle, draft persist, add-remove rows, changed-key diff, export bar, copy-to-clipboard. Export required. A tweakable-plan variant lifts the open decisions above mechanical detail and exports a clarified spec.

## [12]-[DESIGN_SYSTEM]

Trigger: rendering repo tokens as a portable taste surface a later agent consumes before authoring UI.

Regions in order:
- Header — title, token-source name, date slot.
- Swatch grid — one `.card` per color token with name, value, intended use, contrast note, failure use.
- Type scale — live specimens at each type token.
- Spacing and shape — spacing tokens as bars, radii and elevation as physical cards.
- Component contact sheet — each component across size, intent, state, density, and theme.

Patterns: theme toggle, filter, copy-to-clipboard, keyboard grid nav. Export optional. A knob appears only where a token meaningfully changes a component's behavior.
