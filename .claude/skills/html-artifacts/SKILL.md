---
name: html-artifacts
description: >-
  Author single-file, self-contained HTML artifacts opened from disk: implementation plans,
  roadmaps, brainstorms with side-by-side directions, wargames and decision matrices,
  diff-and-review pages, comprehension quizzes, buy-in pitches, capability atlases, explainers,
  decks, micro-editors that export their state back to markdown, design-system pages, status and
  incident reports, dashboards over embedded data, contact sheets, and click-through prototypes.
  Use when creating or updating any HTML planning artifact, when a plan, comparison, roadmap,
  review, decision, or dataset reads better as an interactive page than markdown, or when the
  user says HTML plan, artifact, roadmap, wargame, deck, quiz, dashboard, or report, or asks to
  visualize a plan, decision, or data. Each artifact is a repo-committed full `<html>` document
  opened from disk, distinct from a claude.ai-hosted Artifact (body fragment, CSP-sandboxed)
  owned by the Artifact tool.
allowed-tools:
  - Bash(python3 ${CLAUDE_SKILL_DIR}/scripts/check_artifact.py *)
---

# [HTML_ARTIFACTS]

An HTML artifact trades a document that gets skimmed for one that gets read: spatial layout for comparisons, interaction for decisions, rendered structure for architecture. One file opens from disk with no network, the page is the deliverable, and any artifact that captures intent hands its state back as markdown. The judgment layer — what to investigate, what earns a place, what earns interactivity, how to review — is [references/method.md](references/method.md); build mechanics live in the references routed below.

## [01]-[BUILD]

1. Investigate and select against [references/method.md](references/method.md): name the decision the page moves, build the question inventory, bind every claim to an evidence object, and resolve the interactivity conflict before any layout thought.
2. Pick the type in [02]; copy its template or compose its region spine from [references/artifact-types.md](references/artifact-types.md).
3. Embed the design-system baseline verbatim from [references/design-system.md](references/design-system.md); template-local rules compose the layer architecture, state system, and composition law from [references/styling.md](references/styling.md); interaction patterns come from [references/interaction.md](references/interaction.md); state, embedded data, scale, and export mechanics follow [references/state.md](references/state.md).
4. Fill content answer-first; datasets embed as sanitized JSON payloads under the scale law, and an oversized dataset belongs in a linked file.
5. An artifact that captures judgment composes the round-trip contract from [references/roundtrip.md](references/roundtrip.md): the closed verdict vocabulary, stable item ids, the versioned envelope, and dual markdown-plus-JSON export.
6. Gate: `python3 ${CLAUDE_SKILL_DIR}/scripts/check_artifact.py <file.html>` — fix until exit 0; warn rows are review pressure, not noise.
7. Review in the fixed order method.md owns — skim, coverage, interaction necessity, density, print — and open-check in a browser when one is available.
8. Home the artifact per [05].

## [02]-[TYPE_CHOOSER]

The most specific trigger wins; each row names the misfit sibling it displaces. A comparison of visual directions, UI options, layouts, or weighted candidates never flattens into inline chips or prose options — the artifact route fires first and the page carries the comparison. A request no row matches composes the nearest type's region spine rather than falling back to markdown.

| [INDEX] | [TYPE]           | [WHEN]                             | [INSTEAD_OF] |
| :-----: | :--------------- | :--------------------------------- | :----------- |
|  [01]   | plan             | staged blueprint for one change    | brainstorm   |
|  [02]   | brainstorm       | directions weighed pre-commit      | plan         |
|  [03]   | roadmap          | capability sequenced by horizon    | plan         |
|  [04]   | wargame          | options scored on criteria         | brainstorm   |
|  [05]   | diff-review      | a patch read beside critique       | explainer    |
|  [06]   | quiz             | comprehension gated pre-merge      | diff-review  |
|  [07]   | buy-in           | objections answered pre-signoff    | explainer    |
|  [08]   | capability-atlas | one owner-and-edge map             | roadmap      |
|  [09]   | explainer        | a settled decision walked          | wargame      |
|  [10]   | deck             | a case walked slide by slide       | explainer    |
|  [11]   | editor           | one plan part edited in place      | plan         |
|  [12]   | design-system    | repo tokens as a taste surface     | explainer    |
|  [13]   | report           | recurring status or evidence page  | explainer    |
|  [14]   | dashboard        | filterable board over one dataset  | report       |
|  [15]   | contact-sheet    | one thing across all its states    | brainstorm   |
|  [16]   | prototype        | an interaction path felt pre-build | brainstorm   |

Templates carry the eight highest-frequency types; the rest compose their region spines from the reference.

- plan: [templates/plan.html](templates/plan.html)
- brainstorm: [templates/brainstorm.html](templates/brainstorm.html)
- roadmap: [templates/roadmap.html](templates/roadmap.html)
- wargame: [templates/wargame.html](templates/wargame.html)
- diff-review: [templates/diff-review.html](templates/diff-review.html)
- deck: [templates/deck.html](templates/deck.html)
- editor: [templates/editor.html](templates/editor.html)
- dashboard: [templates/dashboard.html](templates/dashboard.html)
- quiz, buy-in, capability-atlas, explainer, design-system, report, contact-sheet, prototype: compose from [references/artifact-types.md](references/artifact-types.md)

## [03]-[CONTRACT]

- One file: `<!doctype html>`, a `<title>`, all CSS in one `<style>`, all JS in one `<script>`. Zero external references — no CDN, no webfont, no remote image; only `#` anchors, `data:` URIs, `mailto:`, and relative links to sibling artifacts.
- From disk means from disk: sibling fetch, module imports from neighboring files, service workers, and webfonts are dead under `file://`; everything the artifact needs, it embeds. `localStorage` is undefined from disk — a draft persist is a fallible enhancement, and the export bar is the only durable egress.
- Dark and light both render: dark default through `@media (prefers-color-scheme: dark)` plus `:root[data-theme]` overrides that win in both directions, stamped by a small toggle.
- Wide content — tables, diagrams, code — scrolls inside its own `overflow-x:auto` container; the body never scrolls sideways.
- Export is the egress law: an artifact that captures or edits intent carries a footer export bar serializing live UI state to pasteable markdown and downloadable JSON; export then re-import reproduces the state.
- Escape before inject: source, diff, and answer text reaches the DOM through `textContent` escaping before any span wraps it — an injected string is never trusted as HTML.
- Diagrams are inline pre-rendered SVG, never a runtime diagram library.

## [04]-[INVOCATIONS]

A named request shape routes to a type and its interaction contract; the user names the shape and the build follows the row.

| [INDEX] | [SHAPE]               | [RENDERS_AS]     |
| :-----: | :-------------------- | :--------------- |
|  [01]   | `code-approaches`     | brainstorm       |
|  [02]   | `implementation-plan` | plan             |
|  [03]   | `pr-writeup`          | diff-review      |
|  [04]   | `module-map`          | capability-atlas |
|  [05]   | `blindspot`           | editor           |
|  [06]   | `tweakable-plan`      | editor           |
|  [07]   | `design-directions`   | contact-sheet    |
|  [08]   | `status-report`       | report           |
|  [09]   | `incident-report`     | report           |
|  [10]   | `research-report`     | report           |

## [05]-[LIFECYCLE]

- Durable artifacts commit under the host's artifact home (`<artifact-home>`), named `<kind>.<scope>[.<slug>].html`; dated kinds carry `YYYY-MM-DD` in the slug, living kinds update in place, and history is version control.
- Ephemeral artifacts — session plans, one-shot explainers, throwaway editors — live in scratch space and never commit.
- An artifact that moved a durable decision leaves that decision in the durable record; the artifact visualizes truth and is never its sole carrier.

## [06]-[GOTCHAS]

- A quiet external reference is the most common self-containment breach — a CDN script, a webfont, an `https://` image, a runtime diagram-library include; the gate catches every one.
- An embedded JSON payload that carries a raw `</script>` or U+2028 terminates or breaks its own script element; sanitize on embed per the state reference — the gate warns on the character class.
- Hover-only evidence dies on touch and print; anything load-bearing renders visibly in the default state.
- Editing the exported markdown instead of re-exporting forks the truth; the UI state is the source until the next export lands.
- Index-paired array diffs misread reordered rows; diff records key by identity.
- A repo-committed full `<html>` document opened from disk is a different artifact from a claude.ai-hosted Artifact — the hosted kind is a body fragment with no `<html>`/`<head>` under a CSP sandbox and belongs to the Artifact tool.

## [07]-[REPO_INTEGRATION]

When the host repo declares an artifact home and naming law (an atlas or artifacts route in its instruction chain), that law binds `<artifact-home>` and filenames; when a `dataviz` skill is available, chart color and mark decisions defer to it.
