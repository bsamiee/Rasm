# [SVG]

Inline SVG is the artifact's pen: structural diagrams, flowcharts, topologies, and figure illustrations are drawn as real vector elements the reader can inspect, click, and download — never a raster, never a runtime diagram library. This file owns structural diagrams; charts of data defer to a dataviz skill when the host exposes one, mapped onto the `--series-*` tokens.

## [01]-[CONSTRUCTION]

- One stable `viewBox` per SVG; the frame scales without script and the coordinate system never changes after authoring.
- Arrowheads and terminals live as `<marker>` elements in `<defs>`, one marker id per edge class, each `fill` reading a CSS variable.
- Every stroke and fill routes through a CSS class reading a token (`--line`, `--accent`, `--ok`, `--fail`, `--text-muted`); a hard-coded hex inside a themed SVG is a defect — the diagram flips with the page theme or it lies in one of them.
- SVG `<text>` does not wrap: size each box from its label length, hold at least 40px between nodes, and back every edge label with a `--bg`-filled rect so crossing lines never strike through words. A genuinely long label rides `<foreignObject>`; a short one never does.
- Inner node labels are mono at 11-12px; outer annotations are sans at 12px in `--text-muted`.
- A diagram past roughly twelve elements splits by zone or phase — two legible figures beat one spaghetti map, and calling out a genuinely tangled region beats hiding it.

## [02]-[EDGE_AND_NODE_SEMANTICS]

Marks carry meaning by class, one vocabulary across every diagram in the artifact:

| [INDEX] | [CLASS]     | [MARK]                          | [MEANS]                        |
| :-----: | :---------- | :------------------------------ | :----------------------------- |
|  [01]   | `edge`      | 1.5px solid `--line-strong`     | synchronous call or data flow  |
|  [02]   | `edge.async`| 1.5px dashed                    | async, fallback, or cold path  |
|  [03]   | `edge.hot`  | 2.5px solid `--accent`          | the hot or primary path        |
|  [04]   | `edge.fail` | dashed `--fail`                 | failure or rejection route     |
|  [05]   | `node`      | `--raised` fill, `--line-strong`| owner, service, process        |
|  [06]   | `node.gate` | diamond path                    | decision or readiness gate     |
|  [07]   | `node.store`| cylinder or `rx` tall rect      | durable store                  |
|  [08]   | `node.ext`  | dashed border, `--info` stroke  | external system                |
|  [09]   | `node.on`   | 2.5px `--accent` stroke         | selected or active node        |

Zones group nodes as dashed-stroke rects with `--surface` fills and mono zone labels; status hues mark state on nodes (`--ok` healthy, `--warn` degraded, `--fail` down), reinforced by a text badge because hue never carries state alone.

## [03]-[INTERACTIVE_DIAGRAM]

Detail text lives outboard, never crammed inside boxes: each interactive node is a `<g data-k="...">`, a `DETAIL` map keyed by `data-k` owns title, metadata, and body, and a click fills the sticky side panel and moves `node.on`. Animation exists only to show flow, never decoration, and `prefers-reduced-motion` disables it.

```js copy-safe
const DETAIL = { api: { title: "API", meta: "boundary", body: "Single ingress; verifies and routes." } };
const panel = document.querySelector("[data-detail]");
document.querySelectorAll(".diagram [data-k]").forEach(g => g.addEventListener("click", () => {
  document.querySelectorAll(".diagram .on").forEach(n => n.classList.remove("on"));
  g.classList.add("on");
  const d = DETAIL[g.dataset.k];
  panel.querySelector("h3").textContent = d.title;
  panel.querySelector(".meta").textContent = d.meta;
  panel.querySelector("p").textContent = d.body;
}));
```

A multi-behavior system draws one stable topology and overlays named flows — never one diagram per behavior. A `FLOWS` map owns each flow's edge ids, node ids, and ordered step captions; selecting a flow chip dims every node and edge, lights the flow's members, animates the lit edges by dash offset, and walks the captions in a floating card.

```js copy-safe
const FLOWS = { publish: { edges: ["e1", "e3"], nodes: ["api", "queue"], steps: ["Client submits", "Queue fans out"] } };
const setFlow = key => {
  const flow = FLOWS[key];
  document.querySelectorAll(".diagram [id]").forEach(el => el.classList.toggle("dim", Boolean(flow)));
  (flow?.edges ?? []).concat(flow?.nodes ?? []).forEach(id =>
    document.getElementById(id)?.classList.remove("dim"));
  (flow?.edges ?? []).forEach(id => document.getElementById(id)?.classList.add("lit"));
};
```

```css copy-safe
.diagram .dim{opacity:.25;transition:opacity var(--dur-2) var(--ease-standard)}
.diagram .lit{stroke:var(--accent);stroke-dasharray:6 4;animation:flow 1.2s linear infinite}
@keyframes flow{to{stroke-dashoffset:-20}}
@media (prefers-reduced-motion:reduce){.diagram .lit{animation:none}}
```

## [04]-[FIGURE_SHEET]

A figure sheet delivers standalone illustrations — doc headers, README figures — each exportable on its own. The frame is 720x320 (`viewBox="0 0 720 320"`); rectangles carry `rx="10"`; strokes are 1.5px neutral and 2px for emphasized containers; flat fills only — no gradients, no drop shadows. Each `<svg>` embeds its own `<style>` in `<defs>` carrying its label fonts and — for export-bound figures only — literal palette values resolved from the tokens, so the downloaded file stands alone outside the page. Each figure block pairs canvas, caption naming the usage context, and a per-figure download button.

```js copy-safe
const downloadSvg = (svg, filename) => {
  const url = URL.createObjectURL(new Blob(
    [new XMLSerializer().serializeToString(svg)],
    { type: "image/svg+xml;charset=utf-8" }));
  Object.assign(document.createElement("a"), { href: url, download: filename }).click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
};
```

## [05]-[GOTCHAS]

- A `<text>` sized by guess truncates on the reader's font stack; measure against the longest label and pad.
- Marker `fill` set as a presentation attribute overrides class-based theming — keep marker color in CSS.
- A downloaded figure that read page-level CSS renders black-on-transparent outside the page; the embedded `<style>` in `[04]` is the fix.
- `getBBox()` reads zero inside `display:none` containers — measure after reveal or use `visibility:hidden` staging.
- An SVG inside a `.card` inherits the card's padding math; size the frame to the content box, never the viewport.
