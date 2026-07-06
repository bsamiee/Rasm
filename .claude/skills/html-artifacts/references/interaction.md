# [INTERACTION]

Each pattern drops into the one `<script>`/`<style>` pair and earns its place only by the reader question it answers. One document-level listener owns each concern, native primitives own every overlay and disclosure the platform ships, and every injected string escapes through `textContent` before a span wraps it.

## [01]-[FOUNDATION]

Two preludes ride every artifact: the theme stamp and the escape gate.

[THEME_TOGGLE]:

Stamps `data-theme` on `documentElement`; dark is the shipped default, a `localStorage` slug key restores the last choice where the origin grants storage.

```js copy-safe
const themeKey = "theme:" + document.title.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
const root = document.documentElement;
root.dataset.theme = localStorage.getItem(themeKey) ?? "dark";
document.addEventListener("click", e => {
  if (!e.target.closest("[data-toggle-theme]")) return;
  const next = root.dataset.theme === "dark" ? "light" : "dark";
  try { localStorage.setItem(themeKey, root.dataset.theme = next); } catch { root.dataset.theme = next; }
});
```

[ESCAPE_RENDER]:

Source, diff, and answer text reaches the DOM escaped through a detached node; an injected string never renders as trusted HTML.

```js copy-safe
const esc = v => { const n = document.createElement("span"); n.textContent = v; return n.innerHTML; };
```

## [02]-[EGRESS]

Egress moves state out of the page: the export bar is the durable rail, every clipboard write keeps a visible fallback, and a draft is never the record.

[EXPORT_BAR]:

A `snapshot()` reads live UI state; one control copies markdown, one downloads JSON, and a readonly mirror shows what leaves.

```html copy-safe
<footer class="export-bar no-print">
  <button class="btn" data-export="md">Copy markdown</button>
  <button class="btn" data-export="json">Download JSON</button>
  <textarea id="egress" readonly aria-label="Exported state"></textarea>
</footer>
```

```js conceptual
const download = (name, mime, text) => {
  const url = URL.createObjectURL(new Blob([text], { type: mime }));
  Object.assign(document.createElement("a"), { href: url, download: name }).click();
  setTimeout(() => URL.revokeObjectURL(url), 0);
};
document.addEventListener("click", e => {
  const b = e.target.closest("[data-export]"), s = b && snapshot(); if (!b) return;
  if (b.dataset.export === "md") { egress.value = toMarkdown(s); navigator.clipboard?.writeText(egress.value); }
  else download("state.json", "application/json", JSON.stringify(s, null, 2));
});
```

[COPY_CLIPBOARD]:

One listener copies the element named by a button's `data-copy` selector; prose and tables write both `text/html` and `text/plain` so a paste lands clean in a doc or an issue, tokens and commands write plain text, and the toast confirms.

```js copy-safe
const copyRich = (html, text) => navigator.clipboard.write([new ClipboardItem({
  "text/html": new Blob([html], { type: "text/html" }), "text/plain": new Blob([text], { type: "text/plain" }),
})]);
document.addEventListener("click", e => {
  const b = e.target.closest("[data-copy]"), src = b && document.querySelector(b.dataset.copy); if (!src) return;
  navigator.clipboard?.writeText(src.textContent).then(() => flash("Copied"));
});
```

[DRAFT_PERSIST]:

A namespaced `localStorage` draft protects in-session edits, restored on load and saved debounced; a `file://` origin may withhold storage, so the export is the durable record, never the draft.

```js conceptual
const draftKey = "draft:" + themeKey.slice(6);
try { const s = localStorage.getItem(draftKey); if (s) writeState(JSON.parse(s)); } catch {}
let saveTimer;
document.addEventListener("input", () => { clearTimeout(saveTimer); saveTimer = setTimeout(() => { try { localStorage.setItem(draftKey, JSON.stringify(readState())); } catch {} }, 250); });
```

## [03]-[OVERLAYS]

The top layer owns every transient surface; the browser owns dismiss, focus, and stacking.

[POPOVER_ANCHOR]:

Menus, tooltips, inspector panels, and transient status toasts ride the top layer as a native `popover`; an anchored panel tethers to its trigger by anchor positioning while a `popover="manual"` toast a timer auto-hides carries copy and save feedback, and the browser owns light-dismiss, escape close, and stacking. Entry animates from `@starting-style` with `display` flipped discretely; a `commandfor`/`command` button toggles a panel declaratively.

```html copy-safe
<button popovertarget="acts" aria-label="Actions">⋯</button><menu id="acts" popover class="pop">…</menu>
<output id="toast" popover="manual" role="status" aria-live="polite"></output>
```

```css copy-safe
[popovertarget="acts"] { anchor-name: --acts }
.pop { position: absolute; position-anchor: --acts; position-area: block-end span-inline-end; margin: var(--s2) 0 0; padding: var(--s3); transform: translateY(-.25rem) }
#toast { position: fixed; inset: auto var(--s4) var(--s4) auto; margin: 0; padding: var(--s2) var(--s3); border-radius: var(--r1) }
.pop, #toast { border: 1px solid var(--line); border-radius: var(--r2); background: var(--raised); box-shadow: var(--shadow-2); opacity: 0; transition: opacity .16s, transform .16s, display .16s allow-discrete }
.pop:popover-open, #toast:popover-open { opacity: 1; transform: translateY(0) }
@starting-style { .pop:popover-open, #toast:popover-open { opacity: 0 } }
```

```js copy-safe
let toastTimer;
const flash = msg => { toast.textContent = msg; toast.showPopover(); clearTimeout(toastTimer); toastTimer = setTimeout(() => toast.hidePopover(), 1600); };
```

[DIALOG]:

A confirm or destructive-choice flow is a native `<dialog>`; `showModal()` makes the page inert, `form method="dialog"` closes on the chosen `value`, and `returnValue` carries the decision back with no focus-trap or backdrop plumbing. A `commandfor`/`command` button opens it declaratively.

```html copy-safe
<dialog id="confirm"><form method="dialog">
  <p>Discard the draft?</p>
  <button value="cancel" class="btn">Keep</button><button value="ok" class="btn">Discard</button>
</form></dialog>
```

```js conceptual
const ask = dlg => new Promise(res => { dlg.showModal(); dlg.addEventListener("close", () => res(dlg.returnValue === "ok"), { once: true }); });
document.addEventListener("click", async e => {
  if (e.target.closest("[data-confirm-discard]") && await ask(document.getElementById("confirm"))) clearDraft();
});
```

## [04]-[DISCLOSURE]

Disclosure keeps the spine short while depth loads in place.

[COLLAPSIBLE]:

Plan sections, critique records, and glossary entries are native `<details>`; a shared `name` makes a group mutually exclusive with no accordion script, `::details-content` styles the open box, and `:has(> details[open])` lifts the owning section. Print expands every disclosure; height interpolation is an enhancement the reveal never depends on.

```css copy-safe
details { border: 1px solid var(--line); border-radius: var(--r2); padding: var(--s3); margin: var(--s3) 0; background: var(--raised) }
summary { cursor: pointer; font-weight: 600; list-style: none }
summary::before { content: "\25B8"; display: inline-block; margin-right: var(--s2); transition: transform .15s }
details[open] > summary::before { transform: rotate(90deg) }
details::details-content { opacity: 0; content-visibility: hidden; transition: opacity .2s, content-visibility .2s allow-discrete }
details[open]::details-content { opacity: 1; content-visibility: visible }
.section:has(> details[open]) { border-color: var(--accent) }
```

```js copy-safe
addEventListener("beforeprint", () => document.querySelectorAll("details").forEach(d => { d.dataset.wasOpen = d.open; d.open = true; }));
addEventListener("afterprint", () => document.querySelectorAll("details").forEach(d => { d.open = d.dataset.wasOpen === "true"; }));
```

[TABS]:

A tablist wires `role=tab`/`tablist`/`tabpanel`; the active tab holds `tabindex=0` and the rest `-1`, arrow keys roll focus, and one delegated listener drives selection and panel visibility.

```html copy-safe
<div data-tabs role="tablist">
  <button data-tab="p1" role="tab" aria-selected="true" tabindex="0">One</button>
  <button data-tab="p2" role="tab" aria-selected="false" tabindex="-1">Two</button>
</div>
<section id="p1" data-panel role="tabpanel"></section><section id="p2" data-panel role="tabpanel" hidden></section>
```

```js copy-safe
const activate = (group, tab) => {
  group.querySelectorAll("[data-tab]").forEach(b => { const on = b === tab; b.setAttribute("aria-selected", on); b.tabIndex = on ? 0 : -1; });
  document.querySelectorAll("[data-panel]").forEach(p => { p.hidden = p.id !== tab.dataset.tab; });
};
document.addEventListener("click", e => { const t = e.target.closest("[data-tab]"); if (t) activate(t.closest("[data-tabs]"), t); });
document.addEventListener("keydown", e => {
  const t = e.target.closest("[data-tab]"), step = { ArrowRight: 1, ArrowLeft: -1 }[e.key]; if (!t || !step) return;
  const tabs = [...t.closest("[data-tabs]").querySelectorAll("[data-tab]")], n = tabs[(tabs.indexOf(t) + step + tabs.length) % tabs.length];
  n.focus(); activate(t.closest("[data-tabs]"), n);
});
```

## [05]-[COLLECTIONS]

Collection patterns narrow, reorder, and traverse repeated records; the model stays the source and the DOM follows.

[FILTER]:

A live query narrows any `[data-filter]` collection inside a `<search>` landmark; a fixed facet set drops the script entirely, since `:has(input:checked)` flows the checked state upward to hide the misses. A criterion selector lifts one row across parallel cards so a single axis reads at a glance.

```html copy-safe
<search><label>Filter <input type="search" data-filter-box></label></search>
```

```css copy-safe
.toolbar:has(#only-open:checked) ~ .items > :not([data-open]) { display: none }
```

```js copy-safe
document.addEventListener("input", e => {
  const box = e.target.closest("[data-filter-box]"); if (!box) return;
  const q = box.value.trim().toLowerCase();
  document.querySelectorAll("[data-filter]").forEach(el => { el.hidden = q !== "" && !el.textContent.toLowerCase().includes(q); });
});
document.addEventListener("change", e => {
  const sel = e.target.closest("[data-compare]"); if (!sel) return;
  document.querySelectorAll("[data-criterion]").forEach(r => r.classList.toggle("hl", sel.value !== "none" && r.dataset.criterion === sel.value));
});
```

[DRAG_BOARD]:

Work items drag across buckets with move state reduced to card id plus target bucket; the model re-renders after each drop and DOM position is never authoritative.

```js conceptual
document.addEventListener("dragstart", e => { const c = e.target.closest("[data-id]"); if (c) e.dataTransfer.setData("text/plain", c.dataset.id); });
document.addEventListener("dragover", e => { if (e.target.closest("[data-bucket]")) e.preventDefault(); });
document.addEventListener("drop", e => {
  const b = e.target.closest("[data-bucket]"); if (!b) return;
  moveCard(e.dataTransfer.getData("text/plain"), b.dataset.bucket); render();
});
```

[KEYBOARD_NAV]:

A skip link jumps to `#main`; a grid or list is one roving-tabindex composite, so arrow keys move focus across cells while the collection holds a single tab stop.

```html copy-safe
<a class="skip" href="#main">Skip to content</a><main id="main" tabindex="-1"></main>
```

```css copy-safe
.skip { position: absolute; left: -9999px }
.skip:focus { left: var(--s4); top: var(--s4); position: fixed; background: var(--raised); padding: var(--s2) var(--s3); border: 1px solid var(--line); border-radius: var(--r1) }
```

```js copy-safe
document.addEventListener("keydown", e => {
  const cell = e.target.closest("[data-cell]"), step = { ArrowRight: 1, ArrowLeft: -1, ArrowDown: 1, ArrowUp: -1 }[e.key]; if (!cell || !step) return;
  const cells = [...cell.closest("[data-roving]").querySelectorAll("[data-cell]")], n = cells[Math.max(0, Math.min(cells.length - 1, cells.indexOf(cell) + step))];
  cells.forEach(c => { c.tabIndex = -1; }); n.tabIndex = 0; n.focus(); e.preventDefault();
});
```

## [06]-[REVIEW]

Review renders change as data beside critique.

[DIFF]:

The patch renders as a preserved line stream beside a sticky annotation rail; a two-column grid keeps code and critique synchronized, subgrid aligns hunk rows, and each line escapes before it renders. An editor freezes a `structuredClone` baseline at load and derives every change against it, so the data model is the source of truth, never the DOM.

```css copy-safe
.review-wrap { container: review / inline-size }
.review { display: grid; grid-template-columns: minmax(0,1fr) 280px; gap: var(--s5) }
.line { display: block; white-space: pre-wrap; font-family: var(--font-mono); font-size: var(--f1) }
.added { background: color-mix(in srgb, var(--ok) 14%, transparent) }
.removed { background: color-mix(in srgb, var(--fail) 14%, transparent) }
.rail { position: sticky; top: var(--s4); align-self: start }
@container review (width < 48rem) { .review { grid-template-columns: 1fr } }
```

```js conceptual
const renderDiff = parts => parts.map(p => `<span class="line ${p.type === "add" ? "added" : p.type === "remove" ? "removed" : "same"}">${esc(p.text)}</span>`).join("");
const baseline = structuredClone(readState());
const changedKeys = () => { const now = readState(); return Object.keys(now).filter(k => now[k] !== baseline[k]); };
```

## [07]-[NARRATIVE]

Narrative patterns walk a reader through an argument and keep position shareable.

[DECK]:

A small state machine over `<section class="slide">` holds one active slide; arrow keys and buttons advance, a counter shows position, and `startViewTransition` wraps the swap as a guarded enhancement that degrades to an instant cut. An inline stepper is the same machine over `<li>` stages: one active step carries `aria-current`, and a typed `@property` variable fills the progress meter.

```css copy-safe
@property --reach { syntax: "<number>"; inherits: true; initial-value: 0 }
.stepper { counter-reset: step; display: flex; gap: var(--s4); list-style: none; padding: 0 }
.stepper > li { counter-increment: step; color: var(--muted) }
.stepper > li::before { content: counter(step); margin-right: var(--s2); font-variant-numeric: tabular-nums }
.stepper > [aria-current="step"] { color: var(--text); font-weight: 600 }
.stepper-bar > span { display: block; height: 4px; border-radius: 999px; background: var(--accent); inline-size: calc(var(--reach) * 1%); transition: --reach .25s }
```

```js conceptual
const slides = [...document.querySelectorAll(".slide")], swap = document.startViewTransition ? cb => document.startViewTransition(cb) : cb => cb();
let index = 0;
const paint = () => { slides.forEach((s, i) => s.classList.toggle("active", i === index)); counter.textContent = `${index + 1} / ${slides.length}`; };
const show = next => { const c = Math.max(0, Math.min(slides.length - 1, next)); if (c !== index) { index = c; swap(paint); } };
addEventListener("keydown", e => { const d = { ArrowRight: 1, ArrowLeft: -1 }[e.key]; if (d) show(index + d); });
paint();
```

[SCRUB_CONTROL]:

A slider or segmented control binds to one state object; each input recomputes the object and repaints the SVG figure and metric readout, so the reader drives the explainer live.

```html copy-safe
<label>Load <input type="range" min="0" max="100" value="40" data-scrub="load"></label>
<svg viewBox="0 0 100 12" role="img" aria-label="Load figure"><rect data-bar x="0" y="0" width="40" height="12" fill="var(--accent)"></rect></svg>
<output data-readout aria-live="polite"></output>
```

```js copy-safe
const scrub = { load: 40 };
const paintFig = () => { document.querySelector("[data-bar]").setAttribute("width", scrub.load); document.querySelector("[data-readout]").textContent = `${scrub.load}%`; };
document.addEventListener("input", e => { const c = e.target.closest("[data-scrub]"); if (!c) return; scrub[c.dataset.scrub] = Number(c.value); paintFig(); });
paintFig();
```

[DEEP_LINK]:

On load the page scrolls to the hash target and marks its TOC anchor active; `URLSearchParams` packed onto the fragment carries selected tab, lane, filter, and theme, so a reopened file restores its view with no storage.

```js copy-safe
const params = () => new URLSearchParams(location.hash.slice(1));
const writeView = patch => { const p = params(); for (const [k, v] of Object.entries(patch)) p.set(k, v); history.replaceState(null, "", "#" + p); };
const markActive = () => {
  const id = params().get("at"); if (!id) return;
  document.getElementById(id)?.scrollIntoView();
  document.querySelectorAll(".toc a").forEach(a => a.classList.toggle("here", a.hash === "#at=" + id));
};
addEventListener("hashchange", markActive); markActive();
```

[MARGIN_NOTE]:

Definitions and caveats float beside the claim as sidenotes with a sticky glossary rail; a container query collapses them to block flow when the column narrows, so the argument stays unbroken at any width.

```css copy-safe
.explainer { container: explainer / inline-size; display: grid; grid-template-columns: minmax(0,1fr) 260px; gap: var(--s6) }
.glossary { position: sticky; top: var(--s4); align-self: start; font-size: var(--f1) }
.sidenote { float: right; clear: right; width: 38%; margin-right: -42%; font-size: var(--f1); color: var(--muted) }
@container explainer (width < 40rem) { .explainer { display: block } .sidenote { float: none; width: auto; margin: var(--s2) 0 } }
```
