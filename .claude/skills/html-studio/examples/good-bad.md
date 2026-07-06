# [GOOD_BAD]

Excerpt pairs for the five defect classes the NOCTURNE system deletes. Each pair states the diagnosis; the accepted form is the design-system baseline's own grammar.

## [01]-[ELEVATION]

Depth carried by hairlines alone reads flat; the tone ladder carries depth and the hairline only edges it.

Rejected:

```css
:root{--bg:#282A36;--raised:#343746}
.card{background:var(--raised);border:1px solid #44475A}
```

Accepted:

```css
:root{--bg:oklch(0.16 0.022 290);--surface:oklch(0.205 0.024 290);--raised:oklch(0.245 0.026 290);--raised-2:oklch(0.295 0.028 290)}
.card{background:var(--raised);border:1px solid var(--line);box-shadow:var(--shadow-1)}
```

## [02]-[MUTED_TEXT]

A muted token below body contrast carrying sentences is the faded-text defect; the muted token is contrast-guaranteed legal copy and the faint token carries labels only.

Rejected:

```css
.muted{color:#6272A4}
.phase p,.toc a,.filters label{color:var(--muted)}
```

Accepted:

```css
.muted{color:var(--text-muted)}   /* oklch(0.78 0.03 290) — legal body copy on --raised */
.axis-label{color:var(--text-faint)} /* labels only, never a sentence */
```

## [03]-[ACCENT_COMMITMENT]

An accent spent only in transparent washes reads as grayscale with a color rumor; the primary action is a filled field and the washes are its supporting tier.

Rejected:

```css
.btn{background:var(--surface)}
.btn:hover{border-color:var(--accent)}
.active-tab{background:color-mix(in srgb,var(--accent) 8%,transparent)}
```

Accepted:

```css
.btn.primary{background:var(--accent);color:var(--on-accent);box-shadow:var(--shadow-1)}
.btn[aria-pressed="true"]{background:var(--accent-weak);box-shadow:inset 0 0 0 1px var(--accent)}
```

## [04]-[CONTROL_FLOOR]

A control with a border-only hover and no pressed state gives no affordance; every control carries rest, hover, active, and focus-visible.

Rejected:

```css
.btn{border:1px solid var(--line);background:var(--surface)}
.btn:hover{border-color:var(--accent)}
```

Accepted:

```css
.btn{transition:background var(--dur-1) var(--ease-standard),transform var(--dur-2) var(--ease-out)}
.btn:hover{background:var(--raised-2);border-color:var(--accent)}
.btn:active{transform:scale(.985)}
:focus-visible{box-shadow:0 0 0 2px var(--bg),0 0 0 4px var(--focus)}
```

## [05]-[STATE_DIMMING]

Whole-row opacity drops already-marginal text below legibility; state lands as a rail plus a bracket chip and the text stays readable.

Rejected:

```css
tr.disqualified{opacity:.62}
tr.disqualified td{text-decoration:line-through}
```

Accepted:

```css
tr.dq td:first-child{border-inline-start:3px solid var(--fail)}
tr.dq .total{text-decoration:line-through;color:var(--text-muted)}
```

The row also carries `<span class="chip fail">[DISQUALIFIED]</span>` — the bracket text is the accessible carrier and the hue is reinforcement.
