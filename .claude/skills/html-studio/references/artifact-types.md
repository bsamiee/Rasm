# [ARTIFACT_TYPES]

A type is a fixed region order composing baseline tokens and named interaction patterns under one export rule. Each type answers a fixed interrogation, and the region spine is the shape that answers it; each row also names the misfit sibling it displaces, so the most specific trigger wins. A type that captures or edits intent carries the export bar and probes the return channel per [roundtrip.md](roundtrip.md); a purely narrative type omits it. Diagram construction routes to [svg.md](svg.md); every visual token routes to [design-system.md](design-system.md). A request no row matches composes the nearest spine rather than falling back to markdown.

[01]-[PLAN]:

- Question: what changes, in what order, and what proves each stage complete.
- Displaces: brainstorm — the direction is chosen; the page hands off execution.
- Regions, in order:
  - Header — eyebrow, title, deck line, one status chip.
  - Summary strip — four stat cells: effort, surfaces touched, new contracts, gate flags.
  - Sticky TOC — anchors per section, active section highlighted.
  - Milestones — a timeline spine of independently reviewable slices, each with date band, dependency connectors, done/planned dots, and scope tags.
  - Data flow — one inline-SVG figure separating request/response from async fan-out.
  - Key code — collapsible panels reserved for the error-prone parts only.
  - Risk register — table of risk, severity chip, concrete mitigation.
  - Open questions — each with decision owner and decision deadline.
- Interaction: theme toggle, deep-link, collapsible, copy clipboard.
- Export: optional-with-form — stages and acceptance as markdown.
- Review: acceptance that names no observable signal, so a stage stays unprovable.

[02]-[BRAINSTORM]:

- Question: which genuinely distinct directions exist and what tips the choice — candidates unnamed until this page names them.
- Displaces: wargame — nothing is scoreable until directions exist; inline option chips — a visual comparison never flattens to a chip row.
- Regions, in order:
  - Header — eyebrow, title, direction count, the prompt block quoting the ask.
  - Direction grid — three to six cards, each RENDERING its option (live variant, code excerpt, layout mock) with a posture label and tradeoff line; each direction takes a distinct treatment while the grid stays structurally parallel.
  - Comparison control — highlights one criterion across every card at once.
  - Verdict footer — the leaning direction and the trigger that flips it.
- Interaction: theme toggle, filter, deep-link, steal/skip chips feeding export.
- Export: required — kept directions and flip trigger as markdown; send-to-agent when the channel is live.
- Review: directions collapsing onto one axis, so the comparison decides nothing.

[03]-[ROADMAP]:

- Question: what lands in which horizon, what each outcome hard-depends on, and where it stands today.
- Displaces: plan — horizons are confidence bands, never dated task lists.
- Regions, in order:
  - Header — eyebrow, title, scope chip, date slot.
  - Lane board — one lane per workstream, bars sized by horizon span, milestone diamonds, a today marker line.
  - Dependency arrows — drawn only for hard dependencies, cross-team handoffs, and the critical path.
  - Detail panel — sticky card filled by the selected bar: outcome, promotion condition, owner.
  - Filter box — live-dims bars by text or tag; counts stay visible.
- Interaction: theme toggle, filter, click-to-detail, deep-link.
- Export: optional-with-form — the horizon-to-outcome map as markdown.
- Review: a dependency drawn forward across horizons, so the sequence cannot hold.

[04]-[WARGAME]:

- Question: how each named option scores under the weights, which wins, and how the winner breaks.
- Displaces: brainstorm — candidates are named; the page evaluates, never generates.
- Regions, in order:
  - Header — eyebrow, title, criteria legend with live weight sliders.
  - Scoring matrix — options as rows, criteria as columns, heatmap cells whose fill intensity tracks score, editable values, a live weighted-total column.
  - Verdict pane — reranks as weights move; carries the sensitivity line: the smallest change that flips the winner.
  - Risk cards — one card per option enumerating failure modes.
- Interaction: theme toggle, weight sliders, editable cells, draft persist, export bar.
- Export: required — scored matrix, weights, and verdict as markdown plus JSON; send-to-agent when live.
- Review: totals that ignore the weights, so the winning row is an arithmetic accident.

[05]-[DIFF_REVIEW]:

- Question: what the patch changes, where the risk sits, and whether it merges.
- Displaces: explainer — the code is the evidence, never a narrative about it.
- Regions, in order:
  - Header — repo line, title, author, branch, delta counts.
  - Risk map — severity chips per file doubling as the TOC; clicking scrolls and flash-highlights the file card.
  - File cards — diff rows in a line-number/sign/code grid, margin annotations under forty words anchored to their lines with severity rails; safe files collapse to a summary row.
  - Next steps — a checklist of the blocking findings.
  - Verdict footer — merge posture.
- Interaction: theme toggle, escape render, anchor flash, collapsible, deep-link.
- Export: optional-with-form — a clarified-review markdown.
- Review: unescaped source reaching the DOM, so the patch text renders as live markup.

[06]-[QUIZ]:

- Question: does the reader grasp the change, its risk, its tests, and its edge cases.
- Displaces: diff-review — comprehension is gated, never assumed from a read.
- Regions, in order:
  - Header — title, change reference, live score slot.
  - Question cards — one per question, choices as inputs, per-answer reveal explaining the correct choice only after selection.
  - Section links — each wrong answer links the owning corpus page.
  - Score bar — live score, serialized on export, restored on re-import.
- Interaction: theme toggle, escape render, deep-link, export bar.
- Export: required — score and per-question outcome as markdown, re-importable; send-to-agent when live.
- Review: a reveal leaking the answer before the choice, so comprehension goes untested.

[07]-[BUY_IN]:

- Question: what was built, what it asks for, and what answers each objection.
- Displaces: explainer — the page persuades a named audience, never walks a mechanism.
- Regions, in order:
  - Header — title, one-line thesis, date slot.
  - Demo lead — the strongest artifact or result first.
  - Objection cards — one per anticipated objection paired with its answer; one lane per audience in a multi-audience variant.
  - Ask and tradeoff list — the decision requested and the costs named.
  - Sign-off list — named approvers, one state chip each.
- Interaction: theme toggle, deep-link, export bar.
- Export: required — a shareable sign-off message as markdown.
- Review: an objection answered softly, so the strongest counter goes unnamed.

[08]-[CAPABILITY_ATLAS]:

- Question: what owners exist, what each operates, and how they stack.
- Displaces: roadmap — the atlas maps standing capability, never sequences it.
- Regions, in order:
  - Header — title, surface name, date slot.
  - Sticky TOC — anchor per owner group.
  - Owner sections — a grid of capability cards stating operation, input shape, output; a module-map variant adds entry, state, boundary, failure, and gotcha cards with exact file and member names.
  - Composition map — inline-SVG node-and-edge figure of owner stacking per [svg.md](svg.md).
  - Coverage table — capability against status chip.
- Interaction: theme toggle, filter, deep-link, keyboard nav.
- Export: optional-with-form — the owner-and-operation map as markdown.
- Review: an edge map with no legible path, so composition reads as a tangle.

[09]-[EXPLAINER]:

- Question: what the mechanism does, why it holds, and where its caveats bite.
- Displaces: wargame — the decision is settled; the page teaches it.
- Feature variant, regions in order:
  - Sticky side nav — section anchors plus a files-read list.
  - Header — title with a TL;DR rail stating the whole answer in three sentences.
  - Path steps — collapsible steps each carrying its exact file locator.
  - Tabbed samples — config, call site, and observed response as tab panes.
  - Gotchas — keyline callouts for the operational traps.
  - FAQ — recurring questions with settled answers.
- Concept variant, regions in order:
  - Header — title, the concrete instance the concept explains.
  - Live simulator — a manipulable inline-SVG model with controls and a readout computing the consequence of each change.
  - Glossary rail — sticky definitions; prose terms hover-link their entries.
  - Comparison table — the concept against its naive alternative, weak cells marked.
  - Where-it-lands — the surfaces in this corpus that carry the concept.
- Interaction: theme toggle, collapsible, tabs, hover-link, scrub control, deep-link.
- Export: optional-with-form — the section outline as markdown.
- Review: interaction that hides the conclusion, so the settled decision stays buried.

[10]-[DECK]:

- Question: what the case is, in what order it lands, and where the argument stands per slide.
- Displaces: explainer — the reader is walked, one section at a time, under keyboard control.
- Regions, in order:
  - Slide sections — one full-viewport `<section>` per slide under `scroll-snap-type:y mandatory`, hero type on a clamp scale, one inverted slide for the decision beat.
  - Nav — ArrowRight/ArrowLeft/Space advance; an `IntersectionObserver` drives the fixed counter and progress.
  - Presenter aside — per-slide notes in an `<aside>` toggled by one key, hidden in present mode.
- Interaction: theme toggle, deck keys, deep-link.
- Export: optional-with-form — a slide outline as markdown.
- Review: argument fragmentation, so slides stop composing one case.

[11]-[EDITOR]:

- Question: what state stands after the edit, what changed from the baseline, and what leaves as the record.
- Displaces: plan — the reader adjusts the artifact instead of reading a fixed one.
- One polymorphic type; the modality is a data-model row, never a sibling file:
  - board — columns and draggable cards; drop mutates the card's column field; per-column counts and totals derive live.
  - config — grouped toggles with dependency warnings surfacing inline and in a banner; the sidebar renders only changed keys.
  - prompt — a caret-preserving contenteditable template with slot highlighting, unknown-slot warnings, and live sample previews.
  - dataset — row curation with keep/cut marks, tag filters that dim, and a detail drawer.
  - annotation — spans or regions marked with a closed verdict vocabulary.
- Regions, in order: header; sticky toolbar (filters, reset, export); the modality surface; changed-state diff against a frozen initial snapshot; export bar.
- Interaction: theme toggle, drag/drop with cleanup, draft persist, diff, export bar.
- Export: required — the durable artifact (markdown board, config diff plus full JSON, prompt text, curated rows), never a UI-state dump; send-to-agent when live.
- Review: state read from the DOM, so the export forks from the true model.

[12]-[DESIGN_SYSTEM]:

- Question: what tokens exist, what each is for, and how a component reads across its states.
- Displaces: explainer — every token renders as the applied decision it is.
- Regions, in order:
  - Header — title, token-source name, date slot.
  - Swatch grid — one card per color token: name, value, intended use, contrast note, failure use.
  - Type scale — live specimens at each step with size, leading, and weight beside each.
  - Spacing and shape — spacing tokens as measured bars, radii and elevation as physical cards.
  - Component contact sheet — each primitive across size, intent, state, density, and theme.
- Interaction: theme toggle, filter, keyboard nav, copy clipboard per token.
- Export: optional-with-form — the token roster as markdown or JSON.
- Review: a knob that changes no behavior, so the sheet drifts into decoration.

[13]-[REPORT]:

- Question: what the state is, what the evidence shows, and what it asks for next.
- Displaces: explainer — the page recurs on a cadence and is skimmed before read.
- Status variant, regions in order:
  - Header — title, period, auto-generated chip.
  - KPI band — stat cards with serif numerals and delta pills; the warning card takes a status rail.
  - Highlights — three keyline entries stating what moved.
  - Shipped table — evidence rows with links, owners, risk chips.
  - Velocity chart — one inline-SVG plot with the peak highlighted.
  - Carryover — tagged rows naming what slipped and why.
- Incident variant, regions in order:
  - Header — incident id, severity and resolution pills, duration, owner.
  - TL;DR — an inverted panel stating cause, impact, and mitigation in four sentences.
  - Timeline spine — minute-stamped dots with impact and mitigated states, ordered by causality.
  - Root cause — prose plus the config or code diff that carried the defect.
  - Impact table — failed requests, error rate, users, data loss, breach status.
  - Action items — owner avatar, due date, done state per row.
- Interaction: theme toggle, collapsible, deep-link, copy clipboard.
- Export: optional-with-form — asks and digest as markdown.
- Review: causality out of order, so the timeline hides what drove the outcome.

[14]-[DASHBOARD]:

- Question: what the numbers say, how they move under a filter, and which rows drive them.
- Displaces: report — the reader interrogates the dataset instead of receiving a digest.
- Regions, in order:
  - Header — title, filter and facet controls with live match counts.
  - Embedded dataset — the JSON payload inlined and sanitized per [state.md](state.md); the secret-redaction gate runs before embedding; pre-aggregate past ten-thousand rows.
  - KPI row — stat cards recomputed under the active filter.
  - Charts — inline-SVG plots reading series tokens.
  - Data table — sortable rows; filters dim nonmatching rows rather than removing them; clicking a row opens a detail drawer.
- Interaction: theme toggle, filter, facet, sort, detail drawer, keyboard nav, export bar.
- Export: required — the filtered view as markdown plus the full JSON.
- Review: totals that ignore the filter, so a KPI lies against the visible rows.

[15]-[CONTACT_SHEET]:

- Question: how one thing reads in each state, which cells to keep, and what the kept set assembles into.
- Displaces: brainstorm — states of one thing, never competing directions.
- Regions, in order:
  - Header — title, specimen axis legend, date slot.
  - Specimen grid — one cell per size × intent × state × density × theme, same scale and grammar across cells.
  - Selection chips — a steal or skip chip per cell marking the kept set.
  - Assembly footer — the selected cells composed into one surface.
- Interaction: theme toggle, filter, keyboard nav, export bar.
- Export: required — the selected assembly as markdown or JSON; send-to-agent when live.
- Review: cells drawn at different scales, so difference reads as noise, not signal.

[16]-[PROTOTYPE]:

- Question: how the interaction feels before it is built, and what stays open.
- Displaces: brainstorm — one direction is felt, never several compared.
- Animation variant, regions in order:
  - Stage — the real component running its micro-interaction on click.
  - Easing panel — selectable curves mutating one runtime custom property.
  - Keyframe timeline — every staged sub-motion labeled with its exact delay.
  - Copy panel — the tuned CSS as a copyable specimen.
- Interaction variant, regions in order:
  - Working surface — the interaction live: drag, reorder, toggle, with full state cleanup.
  - Decision annotations — the feel decisions baked in, stated so the reader can push back.
  - Open questions — the unresolved feel questions beside the working control.
- Interaction: theme toggle, easing selector, scrub, drag/drop, export bar.
- Export: required — the tuned values or the feedback and open questions as markdown.
- Review: fidelity mistaken for architecture, so the mock reads as a build spec.

[17]-[FIGURE_SHEET]:

- Question: which standalone vector figures the document set needs, each ready to lift.
- Displaces: capability-atlas — the figures serve external documents, never this page's argument.
- Regions, in order:
  - Header — title, canvas dimensions, the no-external-asset line.
  - Figure cards — one per figure: inline SVG with its own embedded `<style>`, caption naming the destination document, a per-figure download button serializing that SVG alone.
  - Palette and rules footer — the locked swatches and stroke/radius/label rules the figures obey.
- Interaction: theme toggle, per-figure SVG download.
- Export: required — each figure as a standalone `.svg` per [svg.md](svg.md).
- Review: a figure whose labels depend on page CSS, so the download renders unstyled.

[18]-[ARCHITECTURE]:

- Question: how the system's parts relate and how each named behavior moves across them.
- Displaces: capability-atlas — one full-bleed topology with live flows, never sectioned cards.
- Regions, in order:
  - Control bar — title, theme toggle, one chip per named flow.
  - Stage — the SVG topology filling the remaining viewport: dashed zone rectangles, typed nodes, directed edges with token-fed markers.
  - Detail card — floating panel filled from a detail map when a node is clicked.
  - Flow caption — floating panel walking the selected flow's ordered steps.
- Interaction: flow chips dim everything then light the selected path with animated dash offset; reduced-motion stills the dashes; click-to-detail per node. Construction law is [svg.md](svg.md).
- Export: optional — the stage as a standalone SVG.
- Review: two behaviors drawn as two topologies, so the reader reconciles diagrams instead of flows.
