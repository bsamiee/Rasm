# [STATE]

The data model is the single source of truth, the DOM renders a projection of it, and every capture leaves through the export rail.

## [01]-[STATE_MODEL]

One plain state object holds every fact the artifact captures; `render()` projects it into the DOM and the DOM position is never read back as authority. Load freezes a baseline snapshot through `structuredClone`, and every changed-key and diff readout derives from state-versus-baseline. Undo is a snapshot stack pushed before each mutation.

```js
const state = { rows: [], filter: "", selection: [], tab: "plan" };
const baseline = structuredClone(state);
const undo = [];
const render = () => { /* project state into the DOM; the DOM is never re-read as source */ };
const commit = mutate => { undo.push(structuredClone(state)); mutate(state); render(); };
const undoLast = () => { if (undo.length) { Object.assign(state, undo.pop()); render(); } };
const changedKeys = () => Object.keys(state)
  .filter(k => JSON.stringify(state[k]) !== JSON.stringify(baseline[k]));
```

## [02]-[EMBEDDED_DATA]

The dataset ships as one `<script type="application/json">` payload parsed once at boot; the parser output is the sole data source and markup never duplicates a field. The payload is sanitized before embedding so it can never terminate its own script tag — `</` sequences escape to their `<`-form and the U+2028/U+2029 line separators escape — and repeated rows hydrate by cloning a `<template>` rather than concatenating markup strings.

```js
// build-time: sanitize before the JSON lands inside the script element
const embed = data => JSON.stringify(data)
  .replace(/</g, "\\u003c")
  .replace(//g, "\\u2028").replace(//g, "\\u2029");
```

```html
<script type="application/json" id="payload">{ "rows": [] }</script>
<template id="row-tpl"><tr><td data-cell="name"></td><td data-cell="weight"></td></tr></template>
```

```js
const data = JSON.parse(document.getElementById("payload").textContent);
const tpl = document.getElementById("row-tpl");
const mountRows = rows => {
  const frag = document.createDocumentFragment();
  rows.forEach(r => {
    const node = tpl.content.cloneNode(true);
    node.querySelector("[data-cell=name]").textContent = r.name;
    node.querySelector("[data-cell=weight]").textContent = r.weight;
    frag.append(node);
  });
  document.querySelector("tbody").replaceChildren(frag);
};
```

## [03]-[SCALE]

Render cost binds to row count; the artifact holds a view, never a corpus.

| [INDEX] | [ROWS]    | [RENDER_LAW]                                                          |
| :-----: | :-------- | :-------------------------------------------------------------------- |
|  [01]   | under ~1k | render every row; filter and sort in place                            |
|  [02]   | 1k–10k    | paginate or pre-aggregate the render; the filter walks the full model |
|  [03]   | above 10k | the data moves to a linked file; the artifact keeps the view only     |

The filter recomputes visible aggregates from the shown rows — a total that ignores the active filter lies — and its input debounces. Sort re-appends existing nodes, since `append` moves a node with its handlers intact rather than rebuilding them. A long listing carries a scroll minimap mapping row offsets to viewport-scaled markers, rebuilt on resize and on filter.

```js
let t;
document.querySelector("[data-filter-box]").addEventListener("input", e => {
  clearTimeout(t);
  const q = e.target.value.trim().toLowerCase();
  t = setTimeout(() => {
    let shown = 0, sum = 0;
    document.querySelectorAll("[data-filter]").forEach(row => {
      const hit = !q || row.textContent.toLowerCase().includes(q);
      row.hidden = !hit;
      if (hit) { shown++; sum += Number(row.dataset.weight || 0); }
    });
    document.getElementById("total").textContent = `${shown} rows · ${sum}`;
    buildMinimap();
  }, 120);
});
```

```js
const sortBy = (tbody, key, dir) => {
  [...tbody.querySelectorAll("tr")]
    .sort((a, b) => (a.dataset[key] > b.dataset[key] ? 1 : -1) * dir)
    .forEach(tr => tbody.append(tr)); // append moves the node; handlers survive
};
```

```css
.minimap{position:fixed;top:0;right:0;width:var(--s2);height:100vh}
.minimap>span{position:absolute;right:0;width:100%;height:2px;background:var(--accent)}
```

```js
const buildMinimap = () => {
  const scale = window.innerHeight / document.body.scrollHeight;
  const rail = document.querySelector(".minimap");
  rail.replaceChildren();
  document.querySelectorAll("[data-filter]:not([hidden])").forEach(row => {
    const mark = document.createElement("span");
    mark.style.top = `${row.offsetTop * scale}px`;
    rail.append(mark);
  });
};
addEventListener("resize", buildMinimap);
```

## [04]-[URL_STATE]

Shareable view state — mode, filter, selection, active tab — lives in the fragment through `history.replaceState`, so a share is a copied URL and the history stack never fills. `URLSearchParams` reads and writes the fragment body. Personal transient state — collapse, sort direction — stays out of the URL. The plain-params form is preferred until size forces packing; then state packs as JSON to gzip through `CompressionStream` to a URL-safe base64 token. Every decode path falls back to a valid default state on malformed input.

```js
const writeUrl = view => history.replaceState(null, "",
  "#" + new URLSearchParams(view).toString());
const readUrl = () => {
  try { return Object.fromEntries(new URLSearchParams(location.hash.slice(1))); }
  catch { return { tab: "plan", filter: "" }; }
};
```

```js
const pack = async state => {
  const bytes = new TextEncoder().encode(JSON.stringify(state));
  const gz = await new Response(new Blob([bytes]).stream()
    .pipeThrough(new CompressionStream("gzip"))).arrayBuffer();
  return btoa(String.fromCharCode(...new Uint8Array(gz)))
    .replaceAll("+", "-").replaceAll("/", "_").replace(/=+$/, "");
};
const unpack = async token => {
  try {
    const raw = Uint8Array.from(
      atob(token.replaceAll("-", "+").replaceAll("_", "/")), c => c.charCodeAt(0));
    const out = await new Response(new Blob([raw]).stream()
      .pipeThrough(new DecompressionStream("gzip"))).arrayBuffer();
    return JSON.parse(new TextDecoder().decode(out));
  } catch { return null; } // caller substitutes the default state
};
```

## [05]-[EGRESS]

The export bar is the sole durable egress. `snapshot()` reads the live state; markdown copies to the clipboard, JSON downloads as a Blob, and a readonly textarea mirrors what leaves. Prose and table copy writes dual `text/html` + `text/plain` through `ClipboardItem` so a paste lands clean in a doc or an issue; a token or JSON copy writes plain text. The JSON Blob downloads through `createObjectURL` and revokes the URL after the click. Import reads a picked file through `text()` — export then re-import reproduces the state, the round-trip contract — and every clipboard write pairs with the visible textarea or the download fallback.

```js
const snapshot = () => structuredClone(state);
const download = (name, mime, text) => {
  const url = URL.createObjectURL(new Blob([text], { type: mime }));
  Object.assign(document.createElement("a"), { href: url, download: name }).click();
  setTimeout(() => URL.revokeObjectURL(url), 0);
};
```

```js
const copyRich = (html, text) => navigator.clipboard.write([new ClipboardItem({
  "text/html": new Blob([html], { type: "text/html" }),
  "text/plain": new Blob([text], { type: "text/plain" }),
})]);
const copyPlain = text => navigator.clipboard.writeText(text);
```

```js
document.querySelector("input[type=file]").addEventListener("change", async e => {
  const file = e.target.files[0];
  if (!file) return;
  try { Object.assign(state, JSON.parse(await file.text())); render(); }
  catch { /* a malformed import leaves the current state intact */ }
});
```

## [06]-[DIFF_MODEL]

A review or editor diffs as data before it renders. A pair of states folds into path-addressed change records `{ type, path, oldValue, newValue }`, and the render walks the records, never the raw objects. An array of records diffs by stable key, not index — reordered rows misread under index pairing. A string-to-string modification nests a bounded character or word diff, held by the LCS budget: the character diff caps at short strings, and longer text diffs by word or line.

```js
const diffObjects = (a, b, path = "", out = []) => {
  new Set([...Object.keys(a ?? {}), ...Object.keys(b ?? {})]).forEach(k => {
    const p = path ? `${path}.${k}` : k, av = a?.[k], bv = b?.[k];
    if (av === undefined) out.push({ type: "add", path: p, newValue: bv });
    else if (bv === undefined) out.push({ type: "remove", path: p, oldValue: av });
    else if (av && bv && typeof av === "object") diffObjects(av, bv, p, out);
    else if (av !== bv) out.push({ type: "modify", path: p, oldValue: av, newValue: bv });
  });
  return out;
};
```

```js
const keyOf = row => row.id; // stable domain key, never the array index
const diffRows = (a, b) => {
  const bx = new Map(b.map(r => [keyOf(r), r]));
  return a.flatMap(r => diffObjects(r, bx.get(keyOf(r)) ?? {}, `#${keyOf(r)}`));
};
```

```js
const CHAR_CAP = 200;
// charDiff runs the LCS backtrack into typed ops; wordDiff splits on whitespace first
const textDiff = (a, b) => a.length + b.length > CHAR_CAP * 2
  ? wordDiff(a, b) : charDiff(a, b);
```

## [07]-[FILE_URL_CONTRACT]

An artifact opened from `file://` is not a local server. Whatever it needs, it embeds; a design that reaches for a sibling file stops being self-contained.

| [INDEX] | [PRIMITIVE]                             | [FILE_URL] |
| :-----: | :-------------------------------------- | :--------- |
|  [01]   | inline script and CSS, embedded SVG     | RUNS       |
|  [02]   | data URL, Blob URL, `createObjectURL`   | RUNS       |
|  [03]   | user-picked file via `input` + `text()` | RUNS       |
|  [04]   | fragment and query URL state            | RUNS       |
|  [05]   | sibling `fetch`/XHR of a neighbor file  | DEAD       |
|  [06]   | module import from a neighbor file      | DEAD       |
|  [07]   | service worker, Cache API               | DEAD       |
|  [08]   | webfont via `@font-face` from a sibling | DEAD       |
|  [09]   | `localStorage` persistence              | UNDEFINED  |

`localStorage` under `file://` is undefined behavior, so a draft persist is an enhancement that may silently vanish, never the record; the export rail remains the sole durable carrier.

```js
const DKEY = "draft:" + document.title.toLowerCase().replace(/[^a-z0-9]+/g, "-");
const saveDraft = () => { try { localStorage.setItem(DKEY, JSON.stringify(state)); } catch {} };
// restore is best-effort; a vanished draft costs nothing the export rail did not already hold
```
