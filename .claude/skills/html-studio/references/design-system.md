# [DESIGN_SYSTEM]

NOCTURNE is the single canonical `<style>` baseline: a Dracula-descended dark-first system on a violet-black ground with two committed accent roles and an editorial serif voice. Every template embeds `[03]-[BASELINE]` verbatim and adds only template-local rules after it, in the declared layers. A second color vocabulary, a raw hex outside the token block, or an sRGB `color-mix` is a defect the gate flags.

## [01]-[CONTRACT]

One `<!doctype html>` file: one `<title>`, all CSS in one `<style>`, all JS in one `<script>`. Zero network references — no CDN, no webfont, no `https://` in any `src`/`href`, no `url()` that resolves off-machine.

| [INDEX] | [ALLOW]        | [FORM]                                                     |
| :-----: | :------------- | :--------------------------------------------------------- |
|  [01]   | In-page anchor | `href="#section-id"`                                       |
|  [02]   | Inline asset   | `data:` URI                                                |
|  [03]   | Sibling page   | relative `href` to a sibling                               |
|  [04]   | Return channel | server-injected `artifact-return` + `artifact-token` metas |

Wide content — tables, diagrams, code — rides inside an `overflow-x:auto` container (`.twrap`, `pre`); `body` sets `overflow-x:hidden` so the page never scrolls sideways.

## [02]-[TOKENS]

Semantic names only; a consumer reads intent, never hex. Dark is the shipped base; light arrives through `data-theme="light"` and the system-preference media query.

- Elevation: `--bg` `--surface` `--raised` `--raised-2` `--overlay` — each step perceptibly lighter than the one below, so depth is carried by tone; `--line`/`--line-strong` are white-alpha hairlines that read at every elevation, and `--boundary` is the bright containment stroke every dashed zone and container border binds — the shared boundary color the mermaid skill draws its containers with.
- Text: `--text` `--text-muted` `--text-faint` — `--text-muted` is contrast-guaranteed legal body copy; `--text-faint` is legal only for pure decoration at 13px and above (grip dots, disabled controls, filter-dimmed de-emphasis). Any label carrying real information at 12px or below binds `--text-muted`, never `--text-faint` — at 11px on `--raised`, faint sits near 3.4:1 and fails the text floor.
- Violet (`--accent` family): the interactive role — buttons, links, focus, selection, toggles, meters.
- Copper (`--editorial` family): the editorial role — eyebrows, section numerals, keyline callouts, figure captions.
- Status: `--ok` `--warn` `--fail` `--info` — chips, verdicts, rails; hue reinforces, the bracket marker carries state.
- Series: `--series-1`..`--series-6` — chart marks only; a dataviz skill, when present, owns their assignment.
- Space: `--s1`..`--s8` (4/8/12/16/24/32/48/64) — every gap, pad, and margin rides a token.
- Type: `--fs-2xs`..`--fs-4xl` on a 1.20 modular scale. Mono is the systemic voice — every numeral, stat figure, label, tag, chip, keycap, data cell, stamp, and code span rides `--font-mono`; serif (`--font-display`) is a restrained editorial accent spent only on `h1`/`h2` and an optional deck lead, and `--font-sans` carries sentences and nothing else.
- Motion: `--dur-1/2/3` with `--ease-out` `--ease-standard` `--ease-spring`; reduced-motion zeroes every duration.
- Shape: `--r-1/2/3` `--r-full` `--shadow-1/2` `--measure`.

Status color never carries state alone: the chip's bracket text — `[DONE]` `[BLOCKED]` `[OPEN]` `[AT-RISK]` — is the accessible carrier and the hue is reinforcement. All derived color is `color-mix(in oklch, ...)`.

## [03]-[BASELINE]

The baseline declares the layer order and populates `reset`, `tokens`, `base`, `components`, `print`, and `overrides`; template-local rules land in `components`, `utilities`, or `overrides` — never unlayered. Print flips to the light palette on white; forced colors hand the palette to the system.

```html copy-safe
<style>
/* --- [NOCTURNE_BASELINE] --- verbatim across templates; overrides ride the later template-local layers */
@layer reset,tokens,base,components,utilities,print,overrides;
@layer reset{
*,*::before,*::after{box-sizing:border-box}
html{-webkit-text-size-adjust:100%}body{margin:0}img,svg{max-width:100%;height:auto}
}
@layer tokens{
:root{
  color-scheme:dark;
  --bg:oklch(0.16 0.022 290);--surface:oklch(0.205 0.024 290);--raised:oklch(0.245 0.026 290);
  --raised-2:oklch(0.295 0.028 290);--overlay:oklch(0.345 0.03 290);
  --line:oklch(1 0 0/.09);--line-strong:oklch(1 0 0/.16);--boundary:oklch(0.84 0.089 304);
  --text:oklch(0.965 0.008 290);--text-muted:oklch(0.78 0.03 290);--text-faint:oklch(0.66 0.032 290);
  --accent:oklch(0.72 0.185 292);--accent-hover:oklch(0.78 0.18 292);--accent-active:oklch(0.655 0.175 292);
  --accent-weak:oklch(0.72 0.185 292/.16);--on-accent:oklch(0.16 0.02 292);
  --editorial:oklch(0.71 0.13 55);--editorial-weak:oklch(0.71 0.13 55/.14);
  --ok:oklch(0.76 0.15 150);--warn:oklch(0.8 0.14 78);--fail:oklch(0.68 0.2 25);--info:oklch(0.79 0.12 225);
  --series-1:var(--accent);--series-2:oklch(0.72 0.19 350);--series-3:var(--info);
  --series-4:var(--ok);--series-5:oklch(0.78 0.13 60);--series-6:oklch(0.85 0.11 100);
  --focus:var(--accent);
  --s1:4px;--s2:8px;--s3:12px;--s4:16px;--s5:24px;--s6:32px;--s7:48px;--s8:64px;
  --fs-2xs:.694rem;--fs-xs:.833rem;--fs-sm:.9rem;--fs-md:1rem;--fs-lg:1.2rem;
  --fs-xl:1.44rem;--fs-2xl:1.728rem;--fs-3xl:2.074rem;
  --fs-4xl:clamp(2.074rem,1.6rem + 2.2cqi,2.986rem);
  --lh-tight:1.15;--lh-heading:1.25;--lh-body:1.6;--lh-data:1.4;
  --font-display:ui-serif,Georgia,'Iowan Old Style','Times New Roman',serif;
  --font-sans:ui-sans-serif,system-ui,-apple-system,'Segoe UI',Roboto,'Helvetica Neue',sans-serif;
  --font-mono:ui-monospace,'SF Mono','SFMono-Regular',Menlo,'Cascadia Mono','Segoe UI Mono',Consolas,'Liberation Mono',monospace;
  --dur-1:120ms;--dur-2:200ms;--dur-3:320ms;
  --ease-out:cubic-bezier(.16,1,.3,1);--ease-standard:cubic-bezier(.2,0,0,1);
  --ease-spring:cubic-bezier(.34,1.56,.64,1);
  --r-1:6px;--r-2:10px;--r-3:16px;--r-full:999px;--measure:1100px;
  --shadow-1:0 1px 2px oklch(0 0 0/.45),0 1px 1px oklch(0 0 0/.3);
  --shadow-2:0 6px 24px oklch(0 0 0/.55),0 2px 6px oklch(0 0 0/.4);
}
@media (prefers-color-scheme:light){:root:not([data-theme]){
  color-scheme:light;
  --bg:oklch(0.975 0.008 85);--surface:oklch(0.945 0.012 85);--raised:oklch(1 0 0);
  --raised-2:oklch(1 0 0);--overlay:oklch(1 0 0);
  --line:oklch(0 0 0/.1);--line-strong:oklch(0 0 0/.16);--boundary:oklch(0.55 0.16 301);
  --text:oklch(0.21 0.012 290);--text-muted:oklch(0.43 0.02 290);--text-faint:oklch(0.54 0.02 290);
  --accent:oklch(0.52 0.19 292);--accent-hover:oklch(0.47 0.19 292);--accent-active:oklch(0.44 0.18 292);
  --accent-weak:oklch(0.52 0.19 292/.12);--on-accent:oklch(0.99 0 0);
  --editorial:oklch(0.55 0.12 55);--editorial-weak:oklch(0.55 0.12 55/.1);
  --ok:oklch(0.52 0.15 150);--warn:oklch(0.56 0.13 70);--fail:oklch(0.54 0.2 25);--info:oklch(0.52 0.12 235);
  --shadow-1:0 1px 2px oklch(0 0 0/.08),0 1px 1px oklch(0 0 0/.05);
  --shadow-2:0 6px 24px oklch(0 0 0/.12),0 2px 6px oklch(0 0 0/.08);
}}
:root[data-theme="light"]{
  color-scheme:light;
  --bg:oklch(0.975 0.008 85);--surface:oklch(0.945 0.012 85);--raised:oklch(1 0 0);
  --raised-2:oklch(1 0 0);--overlay:oklch(1 0 0);
  --line:oklch(0 0 0/.1);--line-strong:oklch(0 0 0/.16);--boundary:oklch(0.55 0.16 301);
  --text:oklch(0.21 0.012 290);--text-muted:oklch(0.43 0.02 290);--text-faint:oklch(0.54 0.02 290);
  --accent:oklch(0.52 0.19 292);--accent-hover:oklch(0.47 0.19 292);--accent-active:oklch(0.44 0.18 292);
  --accent-weak:oklch(0.52 0.19 292/.12);--on-accent:oklch(0.99 0 0);
  --editorial:oklch(0.55 0.12 55);--editorial-weak:oklch(0.55 0.12 55/.1);
  --ok:oklch(0.52 0.15 150);--warn:oklch(0.56 0.13 70);--fail:oklch(0.54 0.2 25);--info:oklch(0.52 0.12 235);
  --shadow-1:0 1px 2px oklch(0 0 0/.08),0 1px 1px oklch(0 0 0/.05);
  --shadow-2:0 6px 24px oklch(0 0 0/.12),0 2px 6px oklch(0 0 0/.08);
}
@media (prefers-reduced-motion:reduce){:root{--dur-1:0ms;--dur-2:0ms;--dur-3:0ms}}
}
@layer base{
body{background:var(--bg);color:var(--text);font-family:var(--font-sans);font-size:var(--fs-md);line-height:var(--lh-body);overflow-x:hidden}
.wrap{max-width:var(--measure);margin:0 auto;padding:var(--s6) var(--s4) var(--s8)}
h1,h2{font-family:var(--font-display);font-weight:600;letter-spacing:-.01em}
h1{font-size:var(--fs-3xl);line-height:var(--lh-tight);margin:0 0 var(--s3)}
h2{font-size:var(--fs-xl);line-height:var(--lh-heading);margin:var(--s7) 0 var(--s3)}
h3{font-size:var(--fs-lg);font-weight:600;line-height:var(--lh-heading);margin:var(--s5) 0 var(--s2)}
h1,h2,h3{text-wrap:balance}
p,li{text-wrap:pretty}
p{margin:0 0 var(--s3)}
.muted{color:var(--text-muted)}small{font-size:var(--fs-xs);color:var(--text-muted)}
.num{font-variant-numeric:tabular-nums}
a{color:var(--accent);text-decoration:underline;text-decoration-color:color-mix(in oklch,var(--accent) 40%,transparent);text-underline-offset:2px;transition:text-decoration-color var(--dur-1) var(--ease-standard);&:hover{text-decoration-color:var(--accent)}}
hr{border:0;border-top:1px solid var(--line);margin:var(--s5) 0}
button,select,input,textarea{font:inherit;color:inherit}
:focus-visible{outline:none;box-shadow:0 0 0 2px var(--bg),0 0 0 4px var(--focus);border-radius:var(--r-1)}
::selection{background:var(--accent-weak)}
}
@layer components{
.eyebrow{font-family:var(--font-mono);font-size:var(--fs-2xs);font-weight:600;letter-spacing:.08em;text-transform:uppercase;color:var(--editorial)}
.deck{font-size:var(--fs-lg);color:var(--text-muted);max-width:60ch}
.card{background:var(--raised);border:1px solid var(--line);border-radius:var(--r-2);padding:var(--s4);box-shadow:var(--shadow-1)}
.grid{display:grid;gap:var(--s4);grid-template-columns:repeat(auto-fit,minmax(240px,1fr))}
.section{counter-increment:sec;&>h2::before{content:counter(sec,decimal-leading-zero);font-family:var(--font-mono);font-size:var(--fs-sm);font-weight:500;color:var(--editorial);margin-right:var(--s3);letter-spacing:.06em}}
.chip{display:inline-flex;align-items:center;gap:var(--s1);font-family:var(--font-mono);font-size:var(--fs-2xs);font-weight:600;letter-spacing:.04em;text-transform:uppercase;padding:2px var(--s2);border-radius:var(--r-full);border:1px solid var(--line-strong);color:var(--text-muted);background:var(--surface);
&.ok{color:var(--ok);border-color:color-mix(in oklch,var(--ok) 45%,transparent);background:color-mix(in oklch,var(--ok) 14%,transparent)}&.warn{color:var(--warn);border-color:color-mix(in oklch,var(--warn) 45%,transparent);background:color-mix(in oklch,var(--warn) 14%,transparent)}
&.fail{color:var(--fail);border-color:color-mix(in oklch,var(--fail) 45%,transparent);background:color-mix(in oklch,var(--fail) 14%,transparent)}&.info{color:var(--info);border-color:color-mix(in oklch,var(--info) 45%,transparent);background:color-mix(in oklch,var(--info) 14%,transparent)}}
.stat{display:grid;gap:var(--s1);& b{font-family:var(--font-mono);font-size:var(--fs-2xl);font-weight:600;line-height:var(--lh-tight);font-variant-numeric:tabular-nums;letter-spacing:-.01em}& .delta{font-family:var(--font-mono);font-size:var(--fs-2xs);&.up{color:var(--ok)}&.down{color:var(--fail)}}}
.kbd{font-family:var(--font-mono);font-size:var(--fs-2xs);padding:1px 6px;border:1px solid var(--line-strong);border-bottom-width:2px;border-radius:var(--r-1);background:var(--surface)}
.rail{border-inline-start:3px solid var(--editorial);padding:var(--s3) var(--s4);background:var(--editorial-weak);border-radius:0 var(--r-1) var(--r-1) 0;&.ok{border-color:var(--ok);background:color-mix(in oklch,var(--ok) 8%,transparent)}
&.warn{border-color:var(--warn);background:color-mix(in oklch,var(--warn) 8%,transparent)}&.fail{border-color:var(--fail);background:color-mix(in oklch,var(--fail) 8%,transparent)}}
.toc{position:sticky;top:var(--s4);align-self:start;font-size:var(--fs-xs);
& a{display:block;padding:var(--s1) var(--s2);color:var(--text-muted);text-decoration:none;border-left:2px solid var(--line);transition:color var(--dur-1) var(--ease-standard),border-color var(--dur-1) var(--ease-standard);&:hover{color:var(--text);border-left-color:var(--accent)}&.on{color:var(--text);border-left-color:var(--accent);font-weight:500}}}
.meter{height:6px;border-radius:var(--r-full);background:var(--surface);overflow:hidden;&>span{display:block;height:100%;background:linear-gradient(90deg,var(--accent),var(--accent-active));transition:width var(--dur-2) var(--ease-standard)}
&.seg{display:flex;gap:2px;&>span{transition:none}& .ok{background:var(--ok)}& .warn{background:var(--warn)}& .fail{background:var(--fail)}}}
.twrap{overflow-x:auto;border:1px solid var(--line);border-radius:var(--r-2)}
table{border-collapse:collapse;width:100%;font-size:var(--fs-sm)}
th,td{text-align:left;padding:var(--s2) var(--s3);border-bottom:1px solid var(--line);vertical-align:top}
th{background:var(--surface);font-family:var(--font-mono);font-size:var(--fs-2xs);font-weight:600;letter-spacing:.06em;text-transform:uppercase;color:var(--text-muted)}
tr:last-child td{border-bottom:0}
td.num,th.num{text-align:right;font-family:var(--font-mono);line-height:var(--lh-data)}
code{font-family:var(--font-mono);font-size:.9em;background:var(--surface);padding:1px 5px;border-radius:var(--r-1)}
pre{overflow-x:auto;background:var(--surface);border:1px solid var(--line);border-radius:var(--r-2);padding:var(--s4);font-size:var(--fs-xs);line-height:1.55;& code{background:none;padding:0}}
.btn{font-size:var(--fs-sm);font-weight:500;padding:var(--s2) var(--s4);border:1px solid var(--line-strong);border-radius:var(--r-1);background:var(--surface);cursor:pointer;transition:background var(--dur-1) var(--ease-standard),border-color var(--dur-1) var(--ease-standard),transform var(--dur-2) var(--ease-out),box-shadow var(--dur-2) var(--ease-out);
&:hover{background:var(--raised-2);border-color:var(--accent)}&:active{transform:scale(.985)}
&.primary{background:var(--accent);border-color:var(--accent);color:var(--on-accent);box-shadow:var(--shadow-1);&:hover{background:var(--accent-hover);border-color:var(--accent-hover);transform:translateY(-1px);box-shadow:var(--shadow-2)}&:active{background:var(--accent-active);transform:translateY(0) scale(.985);box-shadow:var(--shadow-1)}}
&.ghost{background:transparent}&[aria-pressed="true"],&.on{background:var(--accent-weak);border-color:var(--accent);box-shadow:inset 0 0 0 1px var(--accent);font-weight:600}
&:disabled{color:var(--text-faint);background:var(--surface);border-color:var(--line);cursor:not-allowed;transform:none;box-shadow:none}}
input,select,textarea{background:var(--bg);border:1px solid var(--line-strong);border-radius:var(--r-1);padding:var(--s2);transition:border-color var(--dur-1) var(--ease-standard)}
input:hover,select:hover,textarea:hover{border-color:var(--accent)}
summary{cursor:pointer;font-weight:500;&::before{content:"\25B8";display:inline-block;margin-right:var(--s2);color:var(--editorial);transition:transform var(--dur-1) var(--ease-standard)}}
details[open]>summary::before{transform:rotate(90deg)}
.export-bar{position:fixed;inset:auto;inset-block-start:50%;inset-inline-end:var(--s4);translate:0 -50%;margin:0;inline-size:min(21rem,calc(100vw - var(--s5)*2));block-size:60vh;flex-direction:column;align-items:stretch;gap:var(--s3);padding:var(--s4);background:var(--raised);color:var(--text);border:1px solid var(--line-strong);border-radius:var(--r-3);box-shadow:var(--shadow-2);overflow:auto;transition:opacity var(--dur-3) var(--ease-out),translate var(--dur-3) var(--ease-out);
&:popover-open{display:flex;@starting-style{opacity:0;translate:var(--s6) -50%}}
&>header{display:flex;align-items:center;justify-content:space-between;gap:var(--s2)}
&>section{display:grid;gap:var(--s2);align-content:start;&+section{border-block-start:1px solid var(--line);padding-block-start:var(--s3)}}
& textarea{inline-size:100%;min-block-size:0;block-size:3.5rem;font-family:var(--font-mono);font-size:var(--fs-xs);transition:block-size var(--dur-2) var(--ease-out);&:focus{block-size:9rem}}}
.drawer-tab{position:fixed;inset-block-end:var(--s5);inset-inline-end:var(--s4);z-index:5;display:inline-flex;align-items:center;gap:var(--s2);font-family:var(--font-mono);font-size:var(--fs-2xs);font-weight:600;letter-spacing:.08em;text-transform:uppercase;padding:var(--s2) var(--s4);border:1px solid var(--line-strong);border-radius:var(--r-full);background:var(--raised);color:var(--text-muted);box-shadow:var(--shadow-2);cursor:pointer;transition:color var(--dur-1) var(--ease-standard),border-color var(--dur-1) var(--ease-standard),background var(--dur-1) var(--ease-standard),transform var(--dur-2) var(--ease-out);&:hover{color:var(--text);border-color:var(--accent);background:var(--raised-2);transform:translateY(-1px)}&:active{transform:translateY(0)}}
}
@layer print{
@media print{
  :root{
    color-scheme:light;
    --bg:oklch(1 0 0);--surface:oklch(0.945 0.012 85);--raised:oklch(1 0 0);--raised-2:oklch(1 0 0);--overlay:oklch(1 0 0);
    --line:oklch(0 0 0/.14);--line-strong:oklch(0 0 0/.2);--boundary:oklch(0.55 0.16 301);
    --text:oklch(0.21 0.012 290);--text-muted:oklch(0.43 0.02 290);--text-faint:oklch(0.54 0.02 290);
    --accent:oklch(0.52 0.19 292);--editorial:oklch(0.55 0.12 55);
    --ok:oklch(0.52 0.15 150);--warn:oklch(0.56 0.13 70);--fail:oklch(0.54 0.2 25);--info:oklch(0.52 0.12 235);
    --shadow-1:none;--shadow-2:none;
  }
  @page{margin:14mm}
  .toc,.no-print,.btn,button,.export-bar{display:none!important}
  details>*{display:block!important}
  .card,tr,pre,figure{break-inside:avoid}
  body,.twrap,pre{overflow:visible}
  *{-webkit-print-color-adjust:exact;print-color-adjust:exact}
}
}
@layer overrides{
@media (forced-colors:active){
  :root{--bg:Canvas;--surface:Canvas;--raised:Canvas;--raised-2:Canvas;--text:CanvasText;--text-muted:CanvasText;--accent:Highlight;--line:CanvasText;--boundary:CanvasText}
  *{box-shadow:none!important;text-shadow:none!important}
}
@media (prefers-contrast:more){
  :focus-visible{box-shadow:0 0 0 2px var(--bg),0 0 0 5px var(--focus)}
}
}
</style>
```

## [04]-[COMPONENTS]

- `.eyebrow` + `h1` + `.deck` — the header triad: copper mono kicker, serif display title, muted deck line.
- `.section` + `h2::before` — CSS-counter section numerals in copper mono; the page body sets `counter-reset:sec`.
- `.card` — one lifted concern block on `--raised` with hairline and shadow.
- `.grid` — responsive `auto-fit` columns for direction sets and swimlanes.
- `.chip` — uppercase mono status token carrying bracket text; add `ok`/`warn`/`fail`/`info`.
- `.stat` — mono display numeral at weight 600 with an optional `.delta.up`/`.delta.down` trend pill.
- `.rail` — 3px keyline callout; copper by default, status hue by class.
- `.toc` — sticky in-page nav; `.on` marks the active section, stamped by an `IntersectionObserver`.
- `.meter` — violet gradient fill; `.meter.seg` stacks `ok`/`warn`/`fail` segments for pass/fail/skip.
- `.btn` — secondary at rest; `.primary` is the filled violet action, `.ghost` the quiet sibling; pressed state rides `aria-pressed="true"`.
- `.export-bar` + `.drawer-tab` — the export drawer: a deliberate `--raised` pill anchored to the bottom-right corner (`inset-block-end:var(--s5);inset-inline-end:var(--s4)`), right-aligned to the drawer's own edge, carrying an uppercase mono label; its `popovertarget` opens a `popover="auto"` side panel — a rounded `--raised` drawer at 60vh, never full-height or full-width. Fixed interior order: the send-to-agent action, then disk egress (copy-markdown, download-JSON), then per-type fields.
- `.twrap` `.kbd` `.num` `pre`/`code` — wide-content shell, keycap, tabular numerals, code surfaces.

Derived structural devices — the heat cell, timeline spine, split pane, sidenote — live in [styling.md](styling.md) with their recipes and ceilings; the baseline owns only the classes above.

## [05]-[THEME]

The base `:root` is NOCTURNE dark; the light palette arrives through `@media (prefers-color-scheme:light)` scoped `:root:not([data-theme])`, so a stamped choice suppresses the media query entirely. `:root[data-theme="light"]` restates the light palette and wins in both directions. The JS toggle stamps `data-theme` on `documentElement` and persists it under a title-slug key; with nothing stored the page follows the system preference and ships dark otherwise.

## [06]-[CHARTS_AND_FIGURES]

Charts and diagrams are inline `<svg>` only — no canvas, no image, no library. Every stroke and fill reads a token through CSS classes (`--accent`, `--ok`, `--line`, `--series-*`); wide plots ride `.twrap`. Chart mark and palette decisions defer to a dataviz skill when the host exposes one; diagram construction law — markers, flows, node interaction, standalone export — is [svg.md](svg.md).

## [07]-[MICRO_SCALE]

Quality lives at the per-element level: every text class carries an exact size, weight, and floor, and any element rendering below its floor is a defect regardless of how the system reads at a glance.

| [INDEX] | [CLASS]                                       | [TOKEN]      | [FAMILY_WEIGHT]         | [FLOOR] |
| :-----: | :-------------------------------------------- | :----------- | :---------------------- | :------ |
|  [01]   | h1                                            | `--fs-3xl`   | serif 600               | 30px    |
|  [02]   | h2                                            | `--fs-xl`    | serif 600               | 21px    |
|  [03]   | h3                                            | `--fs-lg`    | sans 600                | 18px    |
|  [04]   | deck / lead                                   | `--fs-lg`    | sans or serif 400       | 17px    |
|  [05]   | stat numeral, KPI value                       | `--fs-2xl`   | mono 600, tabular       | 22px    |
|  [06]   | body p, li                                    | `--fs-md`    | sans 400                | 15px    |
|  [07]   | table cell                                    | `--fs-sm`    | sans 400 / mono for num | 13px    |
|  [08]   | pre / code block                              | `--fs-xs`    | mono 400                | 12px    |
|  [09]   | small / caption                               | `--fs-xs`    | sans 400                | 12px    |
|  [10]   | svg canvas label                              | 11px literal | mono 400-500            | 11px    |
|  [11]   | eyebrow, chip, th, kbd, delta, stamp, numeral | `--fs-2xs`   | mono 600, uppercase     | 11px    |

- `--fs-2xs` is the label floor and is legal only for uppercase mono at weight 600 with letter-spacing at or above `.04em` — never sentence text, never `--text-faint` ink.
- Letter-spacing per role: eyebrow `.08em`, th and section numerals `.06em`, chips `.04em`; numerals track `-.01em` and always set `tabular-nums`.
- Nothing on an SVG canvas renders below 11px; a value label that must dominate its axis binds `--text` at weight 600 while tick numerals stay `--text-muted`.
- Elevation nesting: a chip, input, code block, or `th` sits one step DOWN from its host (`--surface` on `--raised`); a nested card steps UP to `--raised-2` — never `--raised` on `--raised`. A fill meeting a same-tone neighbor without a `--s4` gap steps its border to `--line-strong`; card-to-card separation is otherwise carried by the gap, since `--line` between equal elevations reads near 1.3:1 and vanishes.
- An inline diagram sits on the page body or inside a container one elevation below its canvas; a diagram whose canvas tone is within one elevation step of its host card loses its edge and either the card steps down or the SVG takes a `--line-strong` frame.
