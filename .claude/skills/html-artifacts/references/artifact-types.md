# [ARTIFACT_TYPES]

A type is a fixed region order composing baseline tokens and named interaction patterns under one export rule. The questions bind the type to its method: each type answers a fixed interrogation, and the region spine is the shape that answers it. A type that captures or edits intent carries the export bar; a purely narrative type omits it and adds one only where a reader lifts state back into a document.

## [01]-[PLAN]

Trigger: a decision-complete implementation blueprint for one change — stages, acceptance, risk.
Questions: what changes, in what order, and what proves each stage complete.
Regions in order:
- Header — title, one status chip, date slot.
- Sticky TOC — anchors to every `h2` below.
- Context — the problem and the constraints that bound it.
- Decisions — table of decision, ruling, rationale.
- Stages — one `.card` per stage over an ordered acceptance list.
- Risk register — table of risk, likelihood, impact, mitigation.
- Appendix — collapsible section holding a code excerpt.
Patterns: theme toggle, deep-link, collapsible, copy-to-clipboard.
Export: optional-with-form — the stage list and acceptance as markdown.
Review: acceptance that names no observable signal, so a stage stays unprovable.

## [02]-[BRAINSTORM]

Trigger: N candidate directions weighed side-by-side before committing to one.
Questions: which directions exist, how each wins or loses, and what tips the choice.
Regions in order:
- Header — title, direction count, date slot.
- Direction grid — one `.card` per direction over thesis, strengths, costs, and kills; a code-approaches variant adds an executable excerpt and a failure-mode line per direction.
- Comparison control — highlights one criterion row across every card at once.
- Verdict footer — the leaning direction and the trigger that flips it.
Patterns: theme toggle, compare-highlight, deep-link.
Export: optional-with-form — the leaning direction and its flip trigger as markdown.
Review: directions collapsing onto one axis, so the comparison decides nothing.

## [03]-[ROADMAP]

Trigger: sequencing capability across horizons with dependency and status visible at a glance.
Questions: what lands in which horizon, what each depends on, and where it stands now.
Regions in order:
- Header — title, status chip, date slot.
- Horizon swimlanes — `now`, `next`, `later` columns via `.grid`, milestone `.card`s inside each.
- Milestone card — title, status chip, dependency note lines.
- Timeline bar — one inline-SVG horizontal bar marking horizon boundaries.
- Filter box — live-narrows milestone cards by text.
Patterns: theme toggle, filter, deep-link.
Export: optional-with-form — the horizon-to-milestone map as markdown.
Review: a dependency drawn forward across horizons, so the sequence cannot hold.

## [04]-[WARGAME]

Trigger: scoring options against weighted criteria and stress-testing each option's failure modes.
Questions: how each option scores, which wins under the weights, and how each one breaks.
Regions in order:
- Header — title, criteria-weight legend, date slot.
- Scoring matrix — options as rows, criteria as columns, each cell an editable number, over a live weighted-total column and a live best-row highlight.
- Risk cards — one `.card` per option enumerating failure modes.
- Export bar — the scored matrix as markdown.
Patterns: theme toggle, draft persist, export bar.
Export: required — the scored matrix with weighted totals as a markdown table.
Review: totals that ignore the weights, so the winning row is an arithmetic accident.

## [05]-[DIFF_REVIEW]

Trigger: a patch read for review — additions, removals, context — with critique beside the code.
Questions: what the patch changes, where the risk sits, and whether it merges.
Regions in order:
- Header — title, change summary, date slot.
- Diff spine — the patch as a preserved line stream over add, remove, and context classes, every line escaped before render.
- Annotation rail — sticky column of severity chips and jump anchors, each suggested fix beside the line it touches.
- Verdict footer — merge posture and the blocking findings.
Patterns: theme toggle, escape render, diff spine, deep-link.
Export: optional-with-form — a clarified-review markdown.
Review: unescaped source reaching the DOM, so the patch text renders as live markup.

## [06]-[QUIZ]

Trigger: gating comprehension before merge — a reader proves grasp of the change, its risk, its tests, and its edge cases.
Questions: have I understood the change, do I see its risk, can I name its tests and edge cases.
Regions in order:
- Header — title, change reference, score slot.
- Question cards — one `.card` per question over multiple-choice inputs and a per-answer reveal explaining the correct choice.
- Section links — each wrong answer links to the owning corpus page through an `#` anchor or relative sibling link.
- Score bar — the live score, serialized on export and restored on re-import.
Patterns: theme toggle, escape render, deep-link, export bar.
Export: required — the score and per-question outcome as markdown, re-importable.
Review: a reveal leaking the answer before the choice, so comprehension goes untested.

## [07]-[BUY_IN]

Trigger: winning agreement on a built direction — lead with the demo, name the asks, pre-answer the objections.
Questions: what was built, what it asks for, and what answers the objection to it.
Regions in order:
- Header — title, one-line thesis, date slot.
- Demo lead — the strongest artifact or result first.
- Objection cards — one `.card` per anticipated objection paired with its answer, one objection lane per audience in a multi-audience variant.
- Ask and tradeoff list — the decision requested and the costs named.
- Sign-off list — named approvers, one state chip each.
Patterns: theme toggle, deep-link, export bar.
Export: required — a shareable sign-off message as markdown.
Review: an objection answered softly, so the strongest counter goes unnamed.

## [08]-[CAPABILITY_ATLAS]

Trigger: mapping a surface's full capability set — owners, operations, and how they compose.
Questions: what owners exist, what each operates, and how they stack.
Regions in order:
- Header — title, surface name, date slot.
- Sticky TOC — anchor per owner group.
- Owner sections — one `h2` per owner, under each a `.grid` of capability `.card`s stating operation, input shape, and output; a module-map variant adds entry, state, boundary, failure, and gotcha cards with exact file and function names.
- Composition map — inline-SVG node-and-edge sketch of how owners stack, strokes on `--line`, active nodes on `--accent`.
- Coverage table — capability against status chip inside `.twrap`.
Patterns: theme toggle, filter, deep-link, keyboard nav.
Export: optional-with-form — the owner-and-operation map as markdown.
Review: an edge map with no legible path, so composition reads as a tangle.

## [09]-[EXPLAINER]

Trigger: walking a reader through a mechanism, review, or decision already made — narrative over interaction.
Questions: what the mechanism does, why it holds, and where its caveats bite.
Regions in order:
- Header — title, subtitle, date slot.
- Sticky TOC — anchor per section.
- Prose sections — `h2`/`h3` at the content measure, inline `code` and `.chip` for verdicts.
- Sidenote rail — definitions and caveats float beside the claim, collapsing to block flow on narrow screens.
- Callout cards — one `.card` for the load-bearing claim or caveat per section.
- Evidence — collapsible sections holding code excerpts or data tables in `.twrap`.
Patterns: theme toggle, collapsible, deep-link, margin note, copy-to-clipboard.
Export: optional-with-form — the section outline as markdown.
Review: interaction that hides the conclusion, so the settled decision stays buried.

## [10]-[DECK]

Trigger: walking a case slide by slide — one active section, keyboard-driven, still one searchable HTML file.
Questions: what the case is, in what order it lands, and where the argument stands on each slide.
Regions in order:
- Slide sections — one semantic `<section>` per slide, one active at a time.
- Nav — arrow-key and button advance over a visible counter and progress.
- Speaker aside — an optional per-slide note hidden in present mode.
Patterns: theme toggle, deck, keyboard nav, deep-link.
Export: optional-with-form — a slide outline as markdown.
Review: argument fragmentation, so slides stop composing one case.

## [11]-[EDITOR]

Trigger: a micro-editor that captures decision rules and hands them back as clean markdown.
Questions: what rules stand, which changed from the baseline, and what leaves as the record.
Regions in order:
- Header — title, draft-state note, date slot.
- Rule rows — repeating row of text inputs over a select and a remove control, plus an add-row control.
- Changed-key diff — the live state compared against a frozen initial snapshot.
- Export bar — the rules table as markdown, plus clear-draft.
Patterns: theme toggle, draft persist, changed-key diff, export bar, copy-to-clipboard.
Export: required — the rules table as markdown; a tweakable-plan variant exports a clarified spec.
Review: state read from the DOM, so the export forks from the true model.

## [12]-[DESIGN_SYSTEM]

Trigger: rendering repo tokens as a portable taste surface a later agent consumes before authoring UI.
Questions: what tokens exist, what each is for, and how a component reads across its states.
Regions in order:
- Header — title, token-source name, date slot.
- Swatch grid — one `.card` per color token over name, value, intended use, contrast note, failure use.
- Type scale — live specimens at each type token.
- Spacing and shape — spacing tokens as bars, radii and elevation as physical cards.
- Component contact sheet — each component across size, intent, state, density, and theme.
Patterns: theme toggle, filter, keyboard nav, copy-to-clipboard.
Export: optional-with-form — the token roster as markdown or JSON.
Review: a knob that changes no behavior, so the sheet drifts into decoration.

## [13]-[REPORT]

Trigger: a recurring status, incident, or research page carrying evidence for a decision — skimmable before complete.
Questions: what the state is, what the evidence shows, and what it asks for next.
Regions in order:
- Header — title, date, owner.
- Stat row — shipped, slipped, and carryover for the status variant; severity and impact for the incident variant.
- Timeline — a week window for status, minute-by-minute for incident ordered by causality.
- Evidence cards — one `.card` per finding with a jump anchor; the research variant adds a contradictions card.
- Asks and actions — the requested decisions and follow-ups, one owner chip each; the research variant closes on a decision section.
Patterns: theme toggle, collapsible, deep-link, copy-to-clipboard.
Export: optional-with-form — the asks and status digest as markdown.
Review: causality out of order, so the timeline hides what drove the outcome.

## [14]-[DASHBOARD]

Trigger: a filterable data board over an embedded dataset — KPIs and charts that recompute under the active filter.
Questions: what the numbers say, how they move under a filter, and which rows drive them.
Regions in order:
- Header — title, filter controls, date slot.
- Embedded dataset — the JSON payload inlined in a `<script>` seed sanitized before render, pre-aggregated past ten-thousand rows and linked out past one-hundred-thousand.
- KPI row — stat `.card`s recomputed under the active filter.
- Charts — one or two inline-SVG plots reading tokens.
- Data table — filterable and sortable rows inside `.twrap`, a scroll minimap tracking a long listing.
Patterns: theme toggle, filter, keyboard nav, export bar.
Export: required — the filtered view as a markdown table plus the full JSON.
Review: totals that ignore the filter, so a KPI lies against the visible rows.

## [15]-[CONTACT_SHEET]

Trigger: one component or option shown across every state so difference pops — states, not competing directions.
Questions: how the thing reads in each state, which cells to keep, and what the kept set assembles into.
Regions in order:
- Header — title, specimen axis legend, date slot.
- Specimen grid — one cell per size × intent × state × density × theme, same scale and grammar across cells.
- Selection chips — a steal or skip chip per cell marking the kept set.
- Assembly footer — the selected cells composed into one surface.
Patterns: theme toggle, filter, keyboard nav, export bar.
Export: required — the selected assembly as markdown or JSON.
Review: cells drawn at different scales, so difference reads as noise, not signal.

## [16]-[PROTOTYPE]

Trigger: a low-fidelity click-through of an interaction path — deciding feel before implementation, never production architecture.
Questions: what the path is, how each step feels on click, and what stays open.
Regions in order:
- Screen sections — one `<section>` per screen, linked, one active at a time.
- State on click — controls mutate visible state in place, a visible screen counter tracking position.
- Notes rail — annotations and open questions beside the screens.
- Feedback footer — the reaction captured for export.
Patterns: theme toggle, deck, deep-link, export bar.
Export: required — the feedback and open questions as markdown.
Review: fidelity mistaken for architecture, so the mock reads as a build spec.
