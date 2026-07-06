# [CONFIG]

Frontmatter is the only live channel a fence configures itself through: the block on line 1 selects the layout engine, look, per-type config, and accessibility directives the diagram type admits, while the host owns everything a fence can only request.

Sections: [01] frontmatter - [02] layout - [03] look - [04] accessibility - [05] render environment - [06] traps.

## [01]-[FRONTMATTER]

An opening `---` on line 1 of the fence body carries `title:` and `config:`, closing with `---` before the diagram header. `%%{init:...}%%` directives are deprecated — frontmatter is the current channel.

```mermaid
---
title: Render contract
config:
  look: neo
  layout: elk
  htmlLabels: false
  markdownAutoWrap: false
  fontFamily: monospace
  deterministicIds: true
  elk:
    mergeEdges: true
    nodePlacementStrategy: NETWORK_SIMPLEX
    considerModelOrder: NODES_AND_EDGES
  flowchart:
    curve: basis
    defaultRenderer: elk
---
flowchart LR
  A --> B --> C
  A --> C
```

- Keys are case-sensitive; a misspelled key silently no-ops, and malformed YAML kills the whole diagram.
- Precedence runs Mermaid defaults, then site `initialize()`, then diagram frontmatter as the highest.
- `secure`, `securityLevel`, `startOnLoad`, `maxTextSize`, `suppressErrorRendering`, and `maxEdges` are blocked from frontmatter by the secure config model — they resolve through `initialize()` alone.
- Root keys with render impact: `htmlLabels` (supersedes deprecated `flowchart.htmlLabels`), `markdownAutoWrap`, `fontFamily`, `deterministicIds`/`deterministicIDSeed`, `handDrawnSeed`, `themeCSS`.
- Every diagram type nests its own block — `flowchart:`, `sequence:`, `er:`, `architecture:`, `kanban:`, and the rest — carrying that type's own keys.

Frontmatter requests capability; the host provides it. `layout: elk`, icon packs, zenuml, and tidy-tree each need a registered loader — the CLI registers ELK and zenuml itself, a browser must register the rest.

## [02]-[LAYOUT]

ELK is the standing layout engine: every ELK-capable diagram declares `layout: elk`, and the remaining families route to the engine that owns them. Direction `LR|RL|TB|BT` rides the header for flowchart, ER, class, and state; sequence is implicitly vertical.

| [INDEX] | [FAMILY]                    | [ENGINE]                                                             |
| :-----: | :-------------------------- | :------------------------------------------------------------------- |
|  [01]   | flowchart family            | `elk`                                                                |
|  [02]   | swimlane                    | layered orthogonal layout consuming `flowchart.defaultRenderer: elk` |
|  [03]   | architecture                | fcose under `architecture:` knobs                                    |
|  [04]   | mindmap                     | `tidy-tree`                                                          |
|  [05]   | sequence, state, ER, charts | type-owned; `layout:` is dead                                        |

```mermaid
---
config:
  layout: elk
  elk:
    nodePlacementStrategy: BRANDES_KOEPF
    cycleBreakingStrategy: GREEDY
---
graph TD
  A --> B
  B --> C
  A --> C
```

- A flowchart takes ELK through `layout: elk` or `flowchart.defaultRenderer: elk`; swimlane consumes only the `flowchart.defaultRenderer: elk` route into its own layout.
- Dagre is the engine's unset-layout behavior, never this corpus's declaration; `flowchart TD` with `dagre-d3` is rejected by the detector, and legacy `graph TD` plus `dagre-d3` exists only as an engine boundary fact.
- The shared-renderer default edge curve is `rounded` — `flowchart.curve: basis` restores splines.

ELK tuning nests under `elk:`:

| [INDEX] | [KEY]                   | [VALUES]                                                                          |
| :-----: | :---------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `mergeEdges`            | `true` \| `false`                                                                 |
|  [02]   | `nodePlacementStrategy` | `SIMPLE` \| `NETWORK_SIMPLEX` \| `LINEAR_SEGMENTS` \| `BRANDES_KOEPF`             |
|  [03]   | `cycleBreakingStrategy` | `GREEDY` \| `DEPTH_FIRST` \| `INTERACTIVE` \| `MODEL_ORDER` \| `SCC_CONNECTIVITY` |
|  [04]   | `forceNodeModelOrder`   | `true` \| `false`                                                                 |
|  [05]   | `considerModelOrder`    | `NONE` \| `NODES_AND_EDGES` \| `PREFER_EDGES` \| `PREFER_NODES`                   |

ELK engine facts, each carrying its authoring rule:

- `nodeSpacing` and `rankSpacing` are inert under ELK — density tunes through `nodePlacementStrategy` and the split move, never those keys.
- A nested subgraph title wider than its content overflows the parent — a subgraph title stays shorter than its member row.
- `subGraphTitleMargin` displaces edge labels — the key stays out of ELK diagrams.
- An inner subgraph `direction` drops when any member links outside the block — the containers rule owned by the styling reference.
- An invisible `~~~` link renders visible and an open link grows a phantom arrowhead — rank control under ELK rides `considerModelOrder` and `forceNodeModelOrder`, and every edge declares its ends.
- A self-referential edge lands misplaced — a self-loop routes through an explicit intermediate node.
- Interactive link tooltips are dead under ELK — the label carries the fact.

## [03]-[LOOK]

`look` selects `classic`, `handDrawn`, or `neo`; state and sequence accept `look: neo`, and `handDrawnSeed` pins hand-drawn jitter. Schema themes are `default`, `base`, `dark`, `forest`, `neutral`, `neo`, `neo-dark`, `redux`, `redux-dark`, `redux-color`, and `redux-dark-color`. Theme selection and `themeVariables` palette work ride the same frontmatter `config:` block, owned by the theming reference — only `base` accepts `themeVariables`.

## [04]-[ACCESSIBILITY]

`accTitle:` (one line) and `accDescr:` (one line, or `accDescr { ... }` for a block) follow the diagram header and generate the SVG `<title>`/`<desc>` with aria attributes. `accDescr` states the relation the diagram encodes, not a roster of its nodes. `block` and `mindmap` mis-handle the directives — omit them there rather than emit broken nodes.

## [05]-[RENDER_ENVIRONMENT]

`mmdc` renders a fence to a file, deriving format from the output extension:

```bash
mmdc -i input.mmd -o output.svg
mmdc -i input.mmd -o output.png -b transparent
mmdc -i input.md -o rendered.md -a ./artefacts -j 4
mmdc -i - -o - -e svg
```

- `--theme` exposes only `default`, `forest`, `dark`, and `neutral`; `base` plus `themeVariables` and every schema theme require `--configFile` JSON.
- `--iconPacks @iconify-json/<pack>` fetches over the network — an icon-bearing diagram needs network access or a cached pack.
- The CLI loads KaTeX and FontAwesome CSS, registers ELK and zenuml, and waits on `document.fonts` before render.

A schema theme or `themeVariables` reaches the CLI only through `--configFile`, since `--theme` cannot select it:

```json
{
  "theme": "base",
  "look": "neo",
  "layout": "elk",
  "flowchart": { "defaultRenderer": "elk" }
}
```

A sandboxed or root CI passes Puppeteer launch args through `--puppeteerConfigFile`:

```json
{ "args": ["--no-sandbox", "--disable-setuid-sandbox"] }
```

- SVG output carries labels in `foreignObject` HTML that downstream SVG consumers drop — PNG is the portable export.
- Repeated runs are not byte-identical, so render comparison keys on raster output, never SVG hashes.
- Markdown-mode fence detection misses a fence whose info string carries irregular whitespace.

## [06]-[TRAPS]

- `xychart` point labels render only on `line`; the syntax parses on `bar` but the labels are silently ignored.
- Packet `themeVariables` are inert.
- TreeView icons need registered packs; an unregistered icon renders as `?`.
- Flowchart image shapes distort the node box without `constraint: on`.
- Sequence actor links and menus die under strict-security or sandboxed hosts.
- Sankey CSV must have exactly three columns; blank lines are permitted only without comma separators.
- `architecture.randomize: false` is not determinism — `architecture.seed` is the lock.
- Architecture `align row|column` fails when declared order contradicts a directional-edge constraint.
