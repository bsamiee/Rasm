# [CODE]

The code doctrine for artifact HTML, CSS, and JS: which platform features are vocabulary, which ride a guard, which never ship, and the shape the one script takes. The stylesheet architecture is [styling.md](styling.md), the state model is [state.md](state.md), behavior patterns are [interaction.md](interaction.md); this file owns the stamped-region contract, platform admission, script shape, and egress resilience.

## [01]-[STAMPED_REGIONS]

Every artifact carries three canonical regions the assembler stamps and the gate byte-verifies; a hand-edited region is drift the gate fails, and a rebuilt canon lands everywhere with one `stamp` run.

| [INDEX] | [REGION]            | [CARRIES]                                              | [SITE]                          |
| :-----: | :------------------ | :----------------------------------------------------- | :------------------------------ |
|  [01]   | `NOCTURNE_BASELINE` | the token, base, component, print, and override layers | head `<style>`, first block     |
|  [02]   | `NOCTURNE_DRAWER`   | drawer tab, export bar, toast output                   | end of `<body>`, before payload |
|  [03]   | `NOCTURNE_RUNTIME`  | the runtime kernel of [03]                             | the one `<script>`, first block |

- The assembler under `scripts/` is the byte owner; the region delimiters are its contract, and `--help` is its verb surface. Template-local CSS appends after the baseline into the declared layers, template-local markup precedes the drawer, and template-local script follows the kernel.
- The runtime kernel owns every cross-template concern: theme stamping, the `el()` hydrate factory, `esc()`, `copyText()`, `download()`, `flash()`, the envelope send, the export wire, the scroll spy, print disclosure expansion, and redaction. A template re-authoring any of these forks the grammar and is a defect; a template composes them and supplies only its model, its renderers, and its capture handlers.
- A composed one-off artifact (no template) starts from a `stamp`-emitted shell, never from a hand-copied baseline.

## [02]-[PLATFORM_ADMISSION]

Admission follows web-platform Baseline: a Baseline-available feature (widely or newly) is plain vocabulary; a Baseline-limited feature rides an `@supports` guard or a JS capability probe and the page stays complete without it; a rejected feature never ships. Verify the band when adopting a feature the tables miss — the band moves, the law does not.

[VOCABULARY] — assumed, unguarded:

| [INDEX] | [FEATURE]                                                 | [KILLS]                                                   |
| :-----: | :-------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `popover` + `popovertarget`                               | overlay managers, outside-click and Escape plumbing       |
|  [02]   | `<dialog>` + `showModal()` + `::backdrop`                 | focus-trap scaffolds, scrim divs, body scroll locks       |
|  [03]   | `<details name>`                                          | accordion exclusivity scripts                             |
|  [04]   | `:has()`                                                  | parent class toggles, sibling-walk scripts                |
|  [05]   | container size queries + `cqi`                            | viewport-only breakpoints, resize listeners               |
|  [06]   | CSS nesting, `@layer`, `@property`, subgrid               | preprocessor habits, specificity wars, equal-height hacks |
|  [07]   | OKLCH, `color-mix()`, relative color syntax               | HSL lightness lies, hand-derived palette variants         |
|  [08]   | `@starting-style` + `transition-behavior: allow-discrete` | JS enter/exit classes, timeout-delayed `display:none`     |
|  [09]   | `inert`                                                   | focus sentinels, `aria-hidden` tree walks                 |
|  [10]   | `:user-valid` / `:user-invalid`                           | touched-field tracking scripts                            |
|  [11]   | `text-wrap: balance`, `clamp()`/`round()`                 | `<br>` headline tuning, JS measurement constants          |
|  [12]   | `content-visibility: auto`                                | manual below-fold virtualization for static sections      |
|  [13]   | `color-scheme` + `data-theme` token branches              | duplicate per-component dark-mode rules                   |
|  [14]   | anchor positioning (`anchor-name`, `position-area`)       | hand-positioned fixed poppers, viewport math              |
|  [15]   | `<search>`, `<output>`, native form semantics             | `role` retrofits on generic containers                    |

[GUARDED] — enhancement behind `@supports` or a probe; the unguarded page stays whole:

| [INDEX] | [FEATURE]                          | [GUARD]                                      | [ENHANCES]                            |
| :-----: | :--------------------------------- | :------------------------------------------- | :------------------------------------ |
|  [01]   | same-document view transitions     | `document.startViewTransition?.(cb) ?? cb()` | deck and board swaps                  |
|  [02]   | scroll-driven animations           | `@supports (animation-timeline: scroll())`   | reading progress, orientation only    |
|  [03]   | container style queries            | attribute-selector twin carries the rule     | density and mode refinement           |
|  [04]   | `::details-content` transition     | plain open state without it                  | disclosure motion                     |
|  [05]   | `interpolate-size: allow-keywords` | instant size change without it               | intrinsic-size transitions            |
|  [06]   | `field-sizing: content`            | fixed `block-size` fallback                  | growing textareas                     |
|  [07]   | invoker commands (`commandfor`)    | `popovertarget`/listener twin                | declarative dialog and popover wiring |
|  [08]   | `text-wrap: pretty`                | plain wrapping                               | rag quality                           |
|  [09]   | `linear()` spring easing           | `--ease-spring` cubic fallback               | one celebratory settle                |
|  [10]   | `hidden="until-found"`             | plain `<details>`                            | find-in-page into collapsed depth     |

[REJECTED] — never in an artifact: masonry/`grid-lanes`, `reading-flow`, CSS `if()`/`@function`, `<dialog closedby>`, customizable `<select>` styling beyond tokens, `blocking="render"`, ES modules, import maps, workers and service workers, `document.execCommand`, runtime framework or diagram libraries, webfonts. Module, worker, and fetch graphs die under `file://` per the contract table [state.md](state.md) owns.

## [03]-[SCRIPT_SHAPE]

One classic `<script>` closes the body: `"use strict"` inside an IIFE, `const`-only bindings, expression-shaped arrow functions, and the fixed block order — KERNEL (stamped), MODEL (payload parse, derived projections), RENDER (pure projections of state into DOM), ACTIONS (the delegated verb tables), WIRE (boot, observers, restore). A reader audits the artifact top-down: what it knows, what it draws, what it does, what starts it.

- One state object, one `render()` projection, one `commit(mutate)` entry — the model law is [state.md](state.md); the code law is that no handler touches the DOM directly except through render or a scoped `paint*` projection, and no render reads the DOM back.
- Events route through delegated verb tables — one document-level listener per event type dispatching on `data-action`/`data-*` keys; per-node `addEventListener` survives only where the target is unique and dynamic (an observer callback, a dialog close). Scoped listener groups take one `AbortController` and die together.

```js copy-safe
const ACTIONS = {
  goto: el => document.getElementById(el.dataset.goto)?.scrollIntoView({ behavior: "smooth" }),
  export: el => doExport(el.dataset.export),
};
document.addEventListener("click", e => {
  const hit = e.target.closest("[data-action]");
  if (hit && !hit.disabled) ACTIONS[hit.dataset.action]?.(hit, e);
});
```

- Repeated markup hydrates by `<template>` cloning or the kernel `el()` factory; data reaches the DOM through `textContent` only. `innerHTML` accepts nothing derived from data — its whole legal domain is constant, authored, audited markup, and generated SVG builds through `createElementNS` or a cloned template, never string concatenation.
- Focus-preserving updates: a render that replaces a subtree containing the active element re-renders the smallest region instead, or restores focus by stable id after `replaceChildren` — a full-page re-render on every keystroke that eats the caret is a defect.
- Numbers, dates, and lists format through `Intl` (`NumberFormat`, `RelativeTimeFormat`, `ListFormat`), never hand-rolled separators or pluralization; snapshots and baselines through `structuredClone`; view state through `URLSearchParams` on the fragment; dynamic selector values through `CSS.escape`.
- JS writes the narrowest runtime API — one `data-*` stamp or one custom property — and CSS derives everything downstream; a JS loop mutating per-node inline styles where a root property serves is a defect. Reads batch before writes inside one `requestAnimationFrame` when input drives repaint.
- The template-local script past the kernel holds near 250 lines; growth past it means logic belongs in the model as data (rows, vocabularies, dispatch keys), not more branches.

## [04]-[IDIOMS]

| [INDEX] | [USE]                                                    | [REPLACES]                                       |
| :-----: | :------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | delegated verb table + `AbortController`                 | per-node listeners, manual listener bookkeeping  |
|  [02]   | `structuredClone(state)`                                 | `JSON.parse(JSON.stringify(...))`, spread copies |
|  [03]   | `Intl.*` formatters                                      | hand-rolled number, date, and list formatting    |
|  [04]   | `navigator.clipboard.writeText` + mirror fallback        | `document.execCommand("copy")`, hidden textareas |
|  [05]   | `matchMedia(...).addEventListener("change", ...)`        | theme polling, duplicate JS theme state          |
|  [06]   | `IntersectionObserver` / `ResizeObserver`                | scroll math, resize polling                      |
|  [07]   | `FormData` + `Object.fromEntries`                        | per-field `querySelector` harvesting             |
|  [08]   | `showPopover()`/`showModal()`/`close()`                  | class-toggle overlays, focus and Escape plumbing |
|  [09]   | `URL`/`URLSearchParams`                                  | regex URL parsing, string splitting              |
|  [10]   | `template.content.cloneNode(true)`                       | `innerHTML` string building for repeated rows    |
|  [11]   | `toggleAttribute`/`dataset`/`classList.toggle(x, force)` | attribute string surgery                         |
|  [12]   | `Object.groupBy`/`Map.groupBy`                           | reduce-into-dict grouping boilerplate            |

## [05]-[EGRESS_RESILIENCE]

Judgment is never stranded: every egress walks one ordered chain and reports where it landed.

- The chain: served send (`POST` per [roundtrip.md](roundtrip.md)) → clipboard `writeText` → the drawer's readonly mirror selected for manual copy, each fall-through announced in the toast and the mirror always holding the exact payload. `execCommand` is dead; the mirror is the terminal fallback and keeps the page interactive.
- A failed send flips the control to its failed state, routes the identical envelope to the clipboard path, and leaves annotations `active` — the idempotence law is [roundtrip.md](roundtrip.md).
- Config, prompt, and dataset payloads run the kernel redaction pass before any copy or send — key-name tokens, known credential prefixes, URL userinfo, high-entropy literals — masking to stable indexed placeholders; the embed-time gate in [state.md](state.md) is the first wall and this pass is the egress wall.
- The export validates its own envelope before anything leaves and visibly blocks a malformed payload; a silent no-op export is a defect.
