---
name: html-artifacts
description: >-
  Author single-file, self-contained HTML artifacts for planning work: implementation plans,
  roadmaps, brainstorms with side-by-side directions, wargames and decision matrices,
  diff-and-review pages, comprehension quizzes, buy-in pitches, capability atlases, decks, and
  micro-editors that export their state back to markdown. Each artifact is a repo-committed full
  `<html>` document opened from disk, distinct from a claude.ai-hosted Artifact (body fragment,
  CSP-sandboxed) owned by the Artifact tool. Use when creating or updating any HTML planning
  artifact, when a plan, comparison, roadmap, diff, or review reads better as an interactive page
  than markdown, or when the user says HTML plan, artifact, roadmap, wargame, deck, quiz, or asks
  to visualize a plan or decision.
---

# [HTML_ARTIFACTS]

An HTML artifact trades a document that gets skimmed for one that gets read: spatial layout for comparisons, interaction for decisions, rendered structure for architecture. One file opens from disk with no network, the page is the deliverable, and any artifact that captures intent hands its state back as markdown.

## [01]-[CONTRACT]

- One file: `<!doctype html>`, a `<title>`, all CSS in one `<style>`, all JS in one `<script>`. Zero external references — no CDN, no webfont, no remote image; only `#` anchors, `data:` URIs, `mailto:`, and relative links to sibling artifacts.
- Dark and light both render: dark default through `@media (prefers-color-scheme: dark)` plus `:root[data-theme]` overrides that win in both directions, stamped by a small toggle.
- Wide content — tables, diagrams, code — scrolls inside its own `overflow-x:auto` container; the body never scrolls sideways.
- Export is the egress law: an artifact that captures or edits intent carries a footer export bar serializing live UI state to pasteable markdown and downloadable JSON. Edits happen in the UI, the export is the round-trip, and the browser never traps the state. `localStorage` protects an in-session draft only, never the durable record.
- Escape before inject: source, diff, and answer text reaches the DOM through `textContent` escaping before any span wraps it — an injected string is never trusted as HTML.
- Diagrams are inline pre-rendered SVG, never a runtime diagram library.

## [02]-[TYPE_CHOOSER]

The most specific trigger wins; each row names the misfit sibling it displaces.

| [INDEX] | [TYPE]           | [WHEN]                          | [INSTEAD_OF] |
| :-----: | :--------------- | :------------------------------ | :----------- |
|  [01]   | plan             | staged blueprint for one change | brainstorm   |
|  [02]   | brainstorm       | directions weighed pre-commit   | plan         |
|  [03]   | roadmap          | capability sequenced by horizon | plan         |
|  [04]   | wargame          | options scored on criteria      | brainstorm   |
|  [05]   | diff-review      | a patch read beside critique    | explainer    |
|  [06]   | quiz             | comprehension gated pre-merge   | diff-review  |
|  [07]   | buy-in           | objections answered pre-signoff | explainer    |
|  [08]   | capability-atlas | one owner-and-edge map          | roadmap      |
|  [09]   | explainer        | a settled decision walked       | wargame      |
|  [10]   | deck             | a case walked slide by slide    | explainer    |
|  [11]   | editor           | one plan part edited in place   | plan         |
|  [12]   | design-system    | repo tokens as a taste surface  | explainer    |

Routing:
- plan: [templates/plan.html](templates/plan.html)
- brainstorm: [templates/brainstorm.html](templates/brainstorm.html)
- roadmap: [templates/roadmap.html](templates/roadmap.html)
- wargame: [templates/wargame.html](templates/wargame.html)
- editor: [templates/editor.html](templates/editor.html)
- diff-review, quiz, buy-in, capability-atlas, explainer, deck, design-system: compose from [references/artifact-types.md](references/artifact-types.md)

## [03]-[INVOCATIONS]

A named request shape routes to a type and its interaction contract; the user names the shape and the build follows the row.

| [INDEX] | [SHAPE]               | [RENDERS_AS]     |
| :-----: | :-------------------- | :--------------- |
|  [01]   | `code-approaches`     | brainstorm       |
|  [02]   | `implementation-plan` | plan             |
|  [03]   | `pr-writeup`          | diff-review      |
|  [04]   | `module-map`          | capability-atlas |
|  [05]   | `blindspot`           | editor           |

## [04]-[LIFECYCLE]

- Durable artifacts commit under the host's artifact home (`<artifact-home>`), named `<kind>.<scope>[.<slug>].html`; dated kinds carry `YYYY-MM-DD` in the slug, living kinds update in place, and history is version control.
- Ephemeral artifacts — session plans, one-shot explainers, throwaway editors — live in scratch space and never commit.
- An artifact that moved a durable decision leaves that decision in the durable record; the artifact visualizes truth and is never its sole carrier.

## [05]-[BUILD]

1. Pick the type; copy its template or compose from [references/artifact-types.md](references/artifact-types.md).
2. Embed the design-system baseline verbatim from [references/design-system.md](references/design-system.md); pull interaction patterns from [references/interaction.md](references/interaction.md).
3. Fill content and keep datasets small and inline; an oversized artifact carries data that belongs in a linked file.
4. Gate: `python scripts/check_artifact.py <file.html>` — external refs, `<base href>`, event-handler URLs, doctype, title, dark handling; fix until exit 0.
5. Home the artifact (durable to the artifact home, ephemeral to scratch) and open-check in a browser when one is available.

## [06]-[GOTCHAS]

- A quiet external reference is the most common self-containment breach — a CDN script, a webfont, an `https://` image, a runtime diagram-library include; the gate catches every one.
- Editing the exported markdown instead of re-exporting forks the truth; the UI state is the source until the next export lands.
- A repo-committed full `<html>` document opened from disk is a different artifact from a claude.ai-hosted Artifact — the hosted kind is a body fragment with no `<html>`/`<head>` under a CSP sandbox and belongs to the Artifact tool.

## [07]-[REPO_INTEGRATION]

When the host repo declares an artifact home and naming law (an atlas or artifacts route in its instruction chain), that law binds `<artifact-home>` and filenames; when a `dataviz` skill is available, chart color and mark decisions defer to it.
