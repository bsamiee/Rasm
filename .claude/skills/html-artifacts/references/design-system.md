# [DESIGN_SYSTEM]

The single canonical `<style>` baseline. Every template embeds `[03]-[BASELINE]` verbatim and adds only template-local rules after it. Author page content against these tokens; never introduce a second color vocabulary.

## [01]-[CONTRACT]

One `<!doctype html>` file: one `<title>`, all CSS in one `<style>`, all JS in one `<script>`. Zero network references. No CDN, no webfont, no `https://` in any `src`/`href`, no `url()` that resolves off-machine.

| [INDEX] | [ALLOW]        | [FORM]               |
| :-----: | :------------- | :------------------- |
|  [01]   | In-page anchor | `href="#section-id"` |
|  [02]   | Inline asset   | `data:` URI          |
|  [03]   | Contact link   | `mailto:`            |

Wide content (tables, diagrams, code) rides inside an `overflow-x:auto` container (`.twrap`, `pre`); `body` sets `overflow-x:hidden` so the page never scrolls sideways.

## [02]-[TOKENS]

Semantic names only. A consumer reads intent, not hex. Dark values ship through the media query and `data-theme="dark"`, light through `data-theme="light"` and the no-preference base.

- Surface: `--bg` `--surface` `--raised` `--line` — page floor, sunken fill, lifted card, hairlines.
- Text: `--text` `--muted` — body copy, secondary and label copy.
- Accent: `--accent` `--accent-muted` — links, primary action, active state.
- Status: `--ok` `--warn` `--fail` `--info` — chips, meters, verdicts, all color-coded.
- Space: `--s1`..`--s6` (4/8/12/16/24/32) — every gap, pad, and margin; no raw px.
- Type: `--f0`..`--f5` (12/13.5/15/18/24/32) — `--f2` body, `--f0` chips and labels, `--f5` page title.
- Shape: `--r1`..`--r3` `--shadow-1` `--shadow-2` `--measure` — radii, elevation, content column.
- Font: `--font-sans` `--font-mono` — system stack, mono for code, chips, numerics.

System stacks only — `ui-sans-serif, system-ui` and `ui-monospace, SFMono-Regular, Menlo`. A webfont breaches the no-network contract, so the stack stays native.

## [03]-[BASELINE]

```html
<style>
/* --- [DESIGN_SYSTEM_BASELINE] --- verbatim across templates; edit token values only */
*,*::before,*::after{box-sizing:border-box}
html{-webkit-text-size-adjust:100%}body{margin:0}img,svg{max-width:100%;height:auto}
:root{
  color-scheme:light;
  --bg:#fff;--surface:#f6f8fa;--raised:#fff;--line:#d0d7de;--text:#1f2328;--muted:#656d76;
  --accent:#0969da;--accent-muted:#0969da;--ok:#1a7f37;--warn:#9a6700;--fail:#cf222e;--info:#0550ae;
  --s1:4px;--s2:8px;--s3:12px;--s4:16px;--s5:24px;--s6:32px;
  --f0:12px;--f1:13.5px;--f2:15px;--f3:18px;--f4:24px;--f5:32px;
  --r1:6px;--r2:10px;--r3:14px;--measure:1100px;
  --shadow-1:0 1px 2px rgba(0,0,0,.06);--shadow-2:0 4px 16px rgba(0,0,0,.10);
  --font-sans:ui-sans-serif,system-ui,-apple-system,"Segoe UI",Roboto,sans-serif;
  --font-mono:ui-monospace,SFMono-Regular,Menlo,Consolas,monospace;
}
@media (prefers-color-scheme:dark){:root:not([data-theme]){
  color-scheme:dark;
  --bg:#0d1117;--surface:#161b22;--raised:#1c2333;--line:#30363d;--text:#e6edf3;--muted:#8b949e;
  --accent:#58a6ff;--accent-muted:#58a6ff;--ok:#3fb950;--warn:#d29922;--fail:#f85149;--info:#79c0ff;
  --shadow-1:0 1px 2px rgba(0,0,0,.4);--shadow-2:0 8px 24px rgba(0,0,0,.5);
}}
:root[data-theme="dark"]{
  color-scheme:dark;
  --bg:#0d1117;--surface:#161b22;--raised:#1c2333;--line:#30363d;--text:#e6edf3;--muted:#8b949e;
  --accent:#58a6ff;--accent-muted:#58a6ff;--ok:#3fb950;--warn:#d29922;--fail:#f85149;--info:#79c0ff;
  --shadow-1:0 1px 2px rgba(0,0,0,.4);--shadow-2:0 8px 24px rgba(0,0,0,.5);
}
:root[data-theme="light"]{
  color-scheme:light;
  --bg:#fff;--surface:#f6f8fa;--raised:#fff;--line:#d0d7de;--text:#1f2328;--muted:#656d76;
  --accent:#0969da;--accent-muted:#0969da;--ok:#1a7f37;--warn:#9a6700;--fail:#cf222e;--info:#0550ae;
  --shadow-1:0 1px 2px rgba(0,0,0,.06);--shadow-2:0 4px 16px rgba(0,0,0,.10);
}
body{background:var(--bg);color:var(--text);font-family:var(--font-sans);font-size:var(--f2);line-height:1.55;overflow-x:hidden}
.wrap{max-width:var(--measure);margin:0 auto;padding:var(--s5) var(--s4)}
h1{font-size:var(--f5);line-height:1.15;margin:0 0 var(--s3)}
h2{font-size:var(--f4);line-height:1.2;margin:var(--s6) 0 var(--s3)}
h3{font-size:var(--f3);margin:var(--s5) 0 var(--s2)}
h1,h2,h3{text-wrap:balance}
p,li{text-wrap:pretty}
.num{font-variant-numeric:tabular-nums}
a{color:var(--accent);text-decoration:none}a:hover{text-decoration:underline}
p{margin:0 0 var(--s3)}.muted{color:var(--muted)}small{font-size:var(--f1);color:var(--muted)}
hr{border:0;border-top:1px solid var(--line);margin:var(--s5) 0}
.card{background:var(--raised);border:1px solid var(--line);border-radius:var(--r2);padding:var(--s4);box-shadow:var(--shadow-1)}
.grid{display:grid;gap:var(--s4);grid-template-columns:repeat(auto-fit,minmax(240px,1fr))}
.chip{display:inline-flex;align-items:center;gap:var(--s1);font-family:var(--font-mono);font-size:var(--f0);font-weight:600;letter-spacing:.02em;text-transform:uppercase;padding:2px var(--s2);border-radius:999px;border:1px solid var(--line);color:var(--muted);background:var(--surface)}
.chip::before{content:"";width:7px;height:7px;border-radius:50%;background:currentColor}
.chip.ok{color:var(--ok);border-color:color-mix(in srgb,var(--ok) 40%,transparent);background:color-mix(in srgb,var(--ok) 12%,transparent)}
.chip.warn{color:var(--warn);border-color:color-mix(in srgb,var(--warn) 40%,transparent);background:color-mix(in srgb,var(--warn) 12%,transparent)}
.chip.fail{color:var(--fail);border-color:color-mix(in srgb,var(--fail) 40%,transparent);background:color-mix(in srgb,var(--fail) 12%,transparent)}
.chip.info{color:var(--info);border-color:color-mix(in srgb,var(--info) 40%,transparent);background:color-mix(in srgb,var(--info) 12%,transparent)}
.kbd{font-family:var(--font-mono);font-size:var(--f0);padding:1px 6px;border:1px solid var(--line);border-bottom-width:2px;border-radius:var(--r1);background:var(--surface)}
.toc{position:sticky;top:var(--s4);align-self:start;font-size:var(--f1)}
.toc a{display:block;padding:var(--s1) var(--s2);color:var(--muted);border-left:2px solid var(--line)}
.toc a:hover{color:var(--text);border-left-color:var(--accent);text-decoration:none}
.meter{height:8px;border-radius:999px;background:var(--surface);overflow:hidden}
.meter>span{display:block;height:100%;background:var(--accent)}
.twrap{overflow-x:auto;border:1px solid var(--line);border-radius:var(--r2)}
table{border-collapse:collapse;width:100%;font-size:var(--f1)}
th,td{text-align:left;padding:var(--s2) var(--s3);border-bottom:1px solid var(--line);vertical-align:top}
th{background:var(--surface);font-weight:600}tr:last-child td{border-bottom:0}
code{font-family:var(--font-mono);font-size:.9em;background:var(--surface);padding:1px 5px;border-radius:var(--r1)}
pre{overflow-x:auto;background:var(--surface);border:1px solid var(--line);border-radius:var(--r2);padding:var(--s4);font-size:var(--f1);line-height:1.5}
pre code{background:none;padding:0}
button,select,input,textarea{font:inherit;color:inherit}
.btn{font-size:var(--f1);padding:var(--s2) var(--s3);border:1px solid var(--line);border-radius:var(--r1);background:var(--surface);cursor:pointer}
.btn:hover{border-color:var(--accent)}
input,select,textarea{background:var(--bg);border:1px solid var(--line);border-radius:var(--r1);padding:var(--s2)}
:focus-visible{outline:2px solid var(--accent);outline-offset:1px}
@media print{
  @page{margin:14mm}
  .toc,.no-print,.btn,button{display:none!important}
  details>*{display:block!important}
  .card{box-shadow:none}
  .card,tr,pre,figure{break-inside:avoid}
  body,.twrap,pre{overflow:visible}
  *{-webkit-print-color-adjust:exact;print-color-adjust:exact}
}
</style>
```

## [04]-[COMPONENTS]

- `.card` — one lifted concern block on `--raised` with hairline and shadow.
- `.grid` — responsive `auto-fit` columns for swimlanes and direction sets.
- `.chip` — uppercase mono status token; add `ok`/`warn`/`fail`/`info`.
- `.kbd` — keycap for shortcuts and literal keys.
- `.toc` — sticky in-page nav; anchors color-shift on hover.
- `.meter`+`>span` — progress bar; set child `width:%` inline for fill.
- `.twrap` — horizontal-scroll shell wrapping any wide `table`.
- `.num` — tabular numerals for stat cells, counters, and diff columns so digits align.
- `pre`/`code` — code excerpt and inline literal, both mono on `--surface`.

Status chips carry uppercase bracket markers as text — `[DONE]` `[BLOCKED]` `[OPEN]` `[AT-RISK]` — matched to a color class: `[DONE]`→`ok`, `[AT-RISK]`/`[BLOCKED]`→`warn`/`fail`, `[OPEN]`→`info`.

## [05]-[THEME]

The base `:root` is light; the dark palette arrives through `@media (prefers-color-scheme:dark)` scoped `:root:not([data-theme])`, so a stamped choice suppresses the media query entirely. `:root[data-theme="dark"]` and `:root[data-theme="light"]` restate each palette and win in both directions by specificity and source order. The JS toggle stamps `data-theme` on `documentElement`, persists it under a title-slug key, and stamps `"dark"` when nothing is stored — dark is the shipped default while a light-system reader still lands light before first interaction.

## [06]-[CHARTS]

Charts and diagrams are inline `<svg>` only — no canvas, no image, no library. Every stroke and fill reads a token (`--accent`, `--ok`, `--line`, status colors); wrap wide plots in `.twrap`. When the host exposes a dataviz skill, palette and scale choices defer to it and map onto these tokens.

A structural diagram — a capability map, an atlas edge graph, a roadmap timeline — carries its craft on the SVG itself: a fixed `viewBox` scales it without script, `--line` draws structure and `--accent` marks the active path while status colors mark state, labels stay at or under ten words on `--muted` and anchor clear of node edges, and each stroke width tracks a space token so the figure reads at both themes.
