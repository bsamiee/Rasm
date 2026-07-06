# [INTERACTION]

Vanilla-JS patterns, zero dependencies, each dropping inside the one `<script>`. Compose only what a type needs.

## [01]-[THEME_TOGGLE]

Stamps `data-theme` on `documentElement`, persists under a title-slug key, defaults dark when unset. Bind a control with `data-toggle-theme`.

```js
(function(){
  var key="theme:"+document.title.toLowerCase().replace(/[^a-z0-9]+/g,"-").replace(/^-|-$/g,"");
  var root=document.documentElement;
  root.setAttribute("data-theme", localStorage.getItem(key) || "dark");
  document.addEventListener("click",function(e){
    if(!e.target.closest("[data-toggle-theme]")) return;
    var next=root.getAttribute("data-theme")==="dark"?"light":"dark";
    root.setAttribute("data-theme",next); localStorage.setItem(key,next);
  });
})();
```

## [02]-[EXPORT_BAR]

The egress contract: a `snapshot()` reads live UI state, one control copies markdown, another downloads JSON, and a readonly mirror shows what leaves. The export is the durable record; nothing traps state in the browser.

```html
<footer class="export-bar no-print">
  <button class="btn" data-export-md>Copy markdown</button>
  <button class="btn" data-export-json>Download JSON</button>
  <textarea id="exported" readonly aria-label="Exported state"></textarea>
</footer>
```

```js
var snapshot=function(){ return { cards:readCards(), filters:readFilters() }; };
var downloadText=function(name,mime,text){
  var url=URL.createObjectURL(new Blob([text],{type:mime}));
  var link=Object.assign(document.createElement("a"),{href:url,download:name});
  link.click(); setTimeout(function(){URL.revokeObjectURL(url);},0);
};
document.querySelector("[data-export-md]").onclick=function(){
  var md=toMarkdown(snapshot()); document.getElementById("exported").value=md;
  if(navigator.clipboard) navigator.clipboard.writeText(md);
};
document.querySelector("[data-export-json]").onclick=function(){
  downloadText("state.json","application/json",JSON.stringify(snapshot(),null,2));
};
```

Prose and table copy-back writes both formats so a paste lands clean in a doc or an issue; token, command, and JSON copy uses plain text. The legacy `execCommand("copy")` textarea path is the deep fallback only.

```js
var copyRich=function(html,text){ return navigator.clipboard.write([new ClipboardItem({
  "text/html": new Blob([html],{type:"text/html"}),
  "text/plain": new Blob([text],{type:"text/plain"})
})]); };
```

## [03]-[ESCAPE_RENDER]

Source, diff, and answer text reaches the DOM escaped through a detached node's `textContent`, then a classified span wraps it. Every injected string passes here first.

```js
var escapeHtml=function(v){ var n=document.createElement("span"); n.textContent=v; return n.innerHTML; };
```

## [04]-[DIFF_SPINE]

The patch renders as a preserved line stream beside a sticky annotation rail; code and critique stay synchronized in a two-column grid, each line escaped before it renders.

```css
.review{display:grid;grid-template-columns:minmax(0,1fr) 280px;gap:var(--s5)}
.line{display:block;white-space:pre-wrap;font-family:var(--font-mono);font-size:var(--f1)}
.added{background:color-mix(in srgb,var(--ok) 14%,transparent)}
.removed{background:color-mix(in srgb,var(--fail) 14%,transparent)}
.rail{position:sticky;top:var(--s4);align-self:start}
@media (max-width:760px){.review{grid-template-columns:1fr}}
```

```js
var renderDiff=function(parts){ return parts.map(function(p){
  var cls=p.type==="add"?"added":p.type==="remove"?"removed":"same";
  return '<span class="line '+cls+'">'+escapeHtml(p.text)+"</span>";
}).join(""); };
```

## [05]-[CHANGED_KEY_DIFF]

An editor freezes its initial state and derives every change from a comparison against that baseline; the data model is source of truth, never the DOM.

```js
var baseline=JSON.stringify(readState());
var changedKeys=function(){ var now=readState(), base=JSON.parse(baseline), out=[];
  Object.keys(now).forEach(function(k){ if(now[k]!==base[k]) out.push(k); }); return out; };
```

## [06]-[DRAG_BOARD]

Work items drag across buckets with move state reduced to card-id plus target bucket; the model re-renders after each drop and the DOM position is never authoritative.

```js
document.addEventListener("dragstart",function(e){ var c=e.target.closest("[data-id]");
  if(c) e.dataTransfer.setData("text/plain",c.dataset.id); });
document.addEventListener("dragover",function(e){ if(e.target.closest("[data-bucket]")) e.preventDefault(); });
document.addEventListener("drop",function(e){ var b=e.target.closest("[data-bucket]"); if(!b) return;
  moveCard(e.dataTransfer.getData("text/plain"),b.dataset.bucket); render(); });
```

## [07]-[DECK]

A small state machine over semantic `<section class="slide">` slides: one active, arrow-key and button advance, a visible counter.

```js
var slides=[].slice.call(document.querySelectorAll(".slide")); var index=0;
var show=function(next){ slides[index].classList.remove("active");
  index=Math.max(0,Math.min(slides.length-1,next)); slides[index].classList.add("active");
  document.getElementById("counter").textContent=(index+1)+" / "+slides.length; };
addEventListener("keydown",function(e){ var d={ArrowRight:1,ArrowLeft:-1}[e.key]; if(d) show(index+d); });
show(0);
```

## [08]-[COLLAPSIBLE]

Native `details`/`summary`; style the marker and expand every section for print.

```css
details{border:1px solid var(--line);border-radius:var(--r2);padding:var(--s3);margin:var(--s3) 0;background:var(--raised)}
summary{cursor:pointer;font-weight:600;list-style:none}
summary::before{content:"\25B8";display:inline-block;margin-right:var(--s2);transition:transform .15s}
details[open]>summary::before{transform:rotate(90deg)}
```

```js
addEventListener("beforeprint",function(){document.querySelectorAll("details").forEach(function(d){d._o=d.open;d.open=true;});});
addEventListener("afterprint",function(){document.querySelectorAll("details").forEach(function(d){d.open=d._o;});});
```

## [09]-[TABS]

A tablist wires `role=tab`/`tablist`/`tabpanel`; one active tab holds `tabindex=0`, the rest `-1`, and arrow keys roll focus. Buttons carry `data-tab`, panels a matching `id`.

```html
<div data-tabs role="tablist">
  <button data-tab="p1" role="tab" aria-selected="true" tabindex="0">One</button>
  <button data-tab="p2" role="tab" aria-selected="false" tabindex="-1">Two</button>
</div>
<section id="p1" data-panel role="tabpanel"></section>
<section id="p2" data-panel role="tabpanel" hidden></section>
```

```js
var activate=function(group,tab){
  group.querySelectorAll("[data-tab]").forEach(function(b){
    var on=b===tab; b.setAttribute("aria-selected",on); b.tabIndex=on?0:-1; });
  document.querySelectorAll("[data-panel]").forEach(function(p){ p.hidden=p.id!==tab.getAttribute("data-tab"); });
};
document.addEventListener("click",function(e){ var t=e.target.closest("[data-tab]"); if(t) activate(t.closest("[data-tabs]"),t); });
document.addEventListener("keydown",function(e){ var t=e.target.closest("[data-tab]"); if(!t) return;
  var step={ArrowRight:1,ArrowLeft:-1}[e.key]; if(!step) return;
  var tabs=[].slice.call(t.closest("[data-tabs]").querySelectorAll("[data-tab]"));
  var n=tabs[(tabs.indexOf(t)+step+tabs.length)%tabs.length]; n.focus(); activate(t.closest("[data-tabs]"),n); });
```

## [10]-[FILTER]

Live-narrows any collection by a query against each item's text. Items carry `data-filter`.

```js
document.addEventListener("input",function(e){
  var box=e.target.closest("[data-filter-box]"); if(!box) return;
  var q=box.value.trim().toLowerCase();
  document.querySelectorAll("[data-filter]").forEach(function(el){
    el.hidden = q && el.textContent.toLowerCase().indexOf(q)<0;
  });
});
```

## [11]-[COMPARE_HIGHLIGHT]

Highlights one criterion across parallel cards. A selector names the criterion; matching rows lift.

```js
document.addEventListener("change",function(e){
  var sel=e.target.closest("[data-compare]"); if(!sel) return;
  var k=sel.value;
  document.querySelectorAll("[data-criterion]").forEach(function(r){
    r.classList.toggle("hl", k!=="none" && r.getAttribute("data-criterion")===k);
  });
});
```

## [12]-[DEEP_LINK]

On load, scroll to the hash target and mark its TOC anchor active; keep it current on hash change.

```js
function markActive(){
  var id=location.hash.slice(1); if(!id) return;
  document.querySelectorAll(".toc a").forEach(function(a){
    a.style.color = a.getAttribute("href")==="#"+id ? "var(--text)" : "";
  });
}
addEventListener("hashchange",markActive); markActive();
```

## [13]-[KEYBOARD_NAV]

A skip-link jumps to main content; a grid or list is one roving-tabindex composite so arrow keys move focus across cells while the collection holds one tab stop.

```html
<a class="skip" href="#main">Skip to content</a>
<main id="main" tabindex="-1"></main>
```

```css
.skip{position:absolute;left:-9999px}
.skip:focus{left:var(--s4);top:var(--s4);position:fixed;background:var(--raised);padding:var(--s2) var(--s3);border:1px solid var(--line);border-radius:var(--r1)}
```

```js
document.addEventListener("keydown",function(e){
  var cell=e.target.closest("[data-cell]"); if(!cell) return;
  var step={ArrowRight:1,ArrowLeft:-1,ArrowDown:1,ArrowUp:-1}[e.key]; if(!step) return;
  var cells=[].slice.call(cell.closest("[data-roving]").querySelectorAll("[data-cell]"));
  var n=cells[Math.max(0,Math.min(cells.length-1,cells.indexOf(cell)+step))];
  cells.forEach(function(c){c.tabIndex=-1;}); n.tabIndex=0; n.focus(); e.preventDefault();
});
```

## [14]-[MARGIN_NOTE]

Definitions and caveats float beside the claim as sidenotes with a sticky glossary rail, collapsing to block flow on narrow screens so the argument stays uninterrupted.

```css
.explainer{display:grid;grid-template-columns:minmax(0,1fr) 260px;gap:var(--s6)}
.glossary{position:sticky;top:var(--s4);align-self:start;font-size:var(--f1)}
.sidenote{float:right;clear:right;width:38%;margin-right:-42%;font-size:var(--f1);color:var(--muted)}
@media (max-width:760px){.explainer{display:block}.sidenote{float:none;display:block;width:auto;margin:var(--s2) 0}}
```

## [15]-[DRAFT_PERSIST]

A namespaced `localStorage` draft protects in-session edits, restored on load and saved debounced on input. The draft is never the durable record — the export bar is the sole egress.

```js
var DKEY="draft:"+document.title.toLowerCase().replace(/[^a-z0-9]+/g,"-");
function readState(){ return {}; }
function writeState(s){ }
var saved=localStorage.getItem(DKEY); if(saved) try{writeState(JSON.parse(saved));}catch(e){}
var t; document.addEventListener("input",function(){
  clearTimeout(t); t=setTimeout(function(){localStorage.setItem(DKEY,JSON.stringify(readState()));},250);
});
```

## [16]-[COPY_CLIPBOARD]

One listener copies the text of the element referenced by a copy button's `data-copy` selector and restores its label.

```js
document.addEventListener("click",function(e){
  var b=e.target.closest("[data-copy]"); if(!b) return;
  var src=document.querySelector(b.getAttribute("data-copy"));
  if(src&&navigator.clipboard){navigator.clipboard.writeText(src.textContent);
    var o=b.textContent; b.textContent="COPIED"; setTimeout(function(){b.textContent=o;},1200);}
});
```
