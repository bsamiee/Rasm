# [CONFIG_THEMING]

Mermaid v11 configuration lands in YAML frontmatter — the layout engine, look, theme tokens, class styling, and accessibility directives a fence admits.

## [01]-[FRONTMATTER]

An opening `---` on line 1 of the fence body carries a `config:` root key and closes with `---` before the diagram header. `%%{init:...}%%` is deprecated — YAML frontmatter is the only live channel.

```yaml
---
config:
  layout: elk
  look: neo
  theme: base
  themeVariables:
    primaryColor: "#4c6ef5"
---
```

- Keys are case-sensitive; a misspelled key is silently ignored, and malformed YAML breaks the whole diagram.
- Precedence runs Mermaid defaults, then site `initialize()`, then diagram frontmatter as the highest.
- Runtime keys (`securityLevel`, `maxTextSize`, `maxEdges`, `suppressErrorRendering`) resolve through `initialize()` only; frontmatter ignores them.

## [02]-[LAYOUT_AND_LOOK]

- `layout`: `dagre` is the default; declare `elk` for a large or cross-linked flowchart rather than shuffling nodes by hand. Flowchart family only — sequence, state, ER, and chart types reject or ignore it.
- `look`: `neo` modern, `classic`, or `handDrawn`. Flowchart, state, and packet.
- direction: `LR`, `RL`, `TB`, or `BT` on the header. Flowchart, ER, class, and state; sequence is implicitly top-to-bottom.
- ELK tuning nests under `elk:` — `mergeEdges`, `nodePlacementStrategy` (`BRANDES_KOEPF`, `NETWORK_SIMPLEX`, `LINEAR_SEGMENTS`, `SIMPLE`), `cycleBreakingStrategy`, `considerModelOrder`. `LR` reads as the default direction; `TB` drives strata and dependency-direction diagrams.

## [03]-[THEMES]

`theme: base` is the one theme that accepts `themeVariables`; `default`, `dark`, `forest`, and `neutral` are fixed. Colors are hex only (`#RRGGBB` or `#RRGGBBAA`) — named colors are unrecognized.

Core variables are `background`, `fontFamily`, `fontSize`, `primaryColor` (which derives secondary, tertiary, and border tones), `primaryTextColor`, `primaryBorderColor`, and `lineColor`. High-traffic per-type variables cover flowchart `mainBkg`/`nodeBorder`/`clusterBkg`, sequence `actorBkg`/`signalColor`/`noteBkgColor`, state `stateBkg`/`transitionColor`, gantt `taskBkgColor`/`critBkgColor`, pie `pie1`-`pie12`, and gitGraph `git0`-`git7`.

Pick mid-saturation hexes that read on light and dark hosts — the fence renders wherever the markdown renders.

## [04]-[CLASSDEF_LINKSTYLE]

- `classDef name fill:#hex,stroke:#hex,stroke-width:2px,color:#hex` declares at diagram end, after the nodes it styles.
- Every `classDef` sets an explicit `color:`; without it a theme keeps fill and stroke but leaves inherited text unreadable, so the class fails to travel between themes.
- Apply through `NodeID:::name` or `class id1,id2 name`; `classDef default ...` restyles every unclassed node.
- Styling supports flowchart, state, class, requirement, quadrant, treemap, and architecture; notes, namespaces, and subgraph titles are not styleable.
- `linkStyle 0,2 stroke:#hex,stroke-width:2px` targets 0-based edge indices in declaration order; `linkStyle default` covers all edges.
- An edge ID (`A e1@--> B` then `e1@{ animate: true }`) owns animation and curve, never `color` or `stroke` — those stay on `linkStyle`.
- Commas inside `stroke-dasharray` escape as `5\,5`.

## [05]-[ACCESSIBILITY]

`accTitle:` (one line) and `accDescr:` (one sentence, or `accDescr { ... }` for multiple lines) follow the diagram header and generate the SVG `<title>` and `<desc>` with aria attributes. `accDescr` states the relation the diagram encodes, not a roster of its nodes. Every committed diagram carries both. `block-beta` and `mindmap` mis-handle the directives — omit them there rather than emit broken nodes.
