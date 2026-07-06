# [SYNTAX_EXTENDED]

The diagram roster beyond the five core types, each at its 11.16 minimal form with the version gate and render trap that bind it.

## [01]-[REGISTRY]

Pick a type by intent, then its section for the minimal fence, version gate, and traps; a dash marks a pre-11 introduction.

| [INDEX] | [TYPE]               | [INTENT]                  | [SINCE]  |
| :-----: | :------------------- | :------------------------ | :------- |
|  [01]   | `mindmap`            | radial hierarchy          | —        |
|  [02]   | `block`              | manual grid layout        | —        |
|  [03]   | `journey`            | phase sentiment           | —        |
|  [04]   | `requirementDiagram` | requirement traceability  | —        |
|  [05]   | `pie`                | part-to-whole share       | —        |
|  [06]   | `quadrantChart`      | two-axis position map     | —        |
|  [07]   | `sankey`             | weighted directed flow    | —        |
|  [08]   | `xychart`            | bar or line chart         | —        |
|  [09]   | `radar-beta`         | multivariate profile      | 11.6.0   |
|  [10]   | `gantt`              | dated schedule            | —        |
|  [11]   | `treemap-beta`       | area-weighted hierarchy   | 11.8.0   |
|  [12]   | `C4`                 | system landscape views    | —        |
|  [13]   | `architecture-beta`  | infrastructure groups     | 11.1.0   |
|  [14]   | `packet`             | bit-field layout          | 11.0.0   |
|  [15]   | `timeline`           | chronological periods     | —        |
|  [16]   | `gitGraph`           | branch and merge history  | —        |
|  [17]   | `kanban`             | workflow-stage board      | 11.4.0   |
|  [18]   | `treeView-beta`      | file-tree hierarchy       | 11.14.0  |
|  [19]   | `venn-beta`          | set-overlap regions       | 11.13.0  |
|  [20]   | `ishikawa-beta`      | cause-effect fishbone     | 11.13.0  |
|  [21]   | `wardley-beta`       | value-chain evolution     | 11.14.0  |
|  [22]   | `cynefin-beta`       | decision-domain sort      | 11.16.0  |
|  [23]   | `railroad-beta`      | grammar syntax rails      | 11.16.0  |
|  [24]   | `swimlane-beta`      | laned process flow        | 11.16.0  |
|  [25]   | `zenuml`             | sequence via zenuml       | —        |

`venn-beta`, `ishikawa-beta`, and `wardley-beta` carry registry rows without a working fence in scope; `zenuml` is an external diagram the CLI registers through `@mermaid-js/mermaid-zenuml`.

## [02]-[MINDMAP]

```mermaid
mindmap
  Root
    [Square]
      (Rounded)
```
Root leads; consistent indentation sets depth, mixed tabs and spaces are rejected, and explicit edges are invalid; a backtick-quoted markdown string carries bold and multiline labels.

## [03]-[BLOCK]

```mermaid
block
  columns 3
  a["A"] b:2 c d
  block:g:2
    columns 2
    h i
  end
```
Stable keyword `block` since 11.10.0 (formerly `block-beta`); `columns N` precedes a row, a `:n` span widens a block, `space` inserts a filler, and a bare block without a span is valid. A nested `block:id:span ... end` holds its own `columns`.

## [04]-[JOURNEY]

```mermaid
journey
  title Label
  section Phase
    Task: 5: Actor
```
Scores are integers `1` through `5`; a task belongs under a `section`, an actor needs no declaration, and an out-of-range score is invalid.

## [05]-[REQUIREMENT_DIAGRAM]

```mermaid
requirementDiagram
  functionalRequirement req {
    id: 1.1
    risk: Low
    verifymethod: Inspection
  }
  element ent { type: doc }
  ent - satisfies -> req
  req <- traces - ent
```
Types are `requirement`, `functionalRequirement`, `interfaceRequirement`, `performanceRequirement`, `physicalRequirement`, `designConstraint`; `risk` takes `Low`/`Medium`/`High` and `verifymethod` takes `Analysis`/`Inspection`/`Test`/`Demonstration`. Relations `contains`, `copies`, `derives`, `satisfies`, `verifies`, `refines`, `traces` spell both `a - satisfies -> b` and `b <- traces - a`, quoted text carries markdown, and `direction` plus the hand-drawn look are 11.6.0+.

## [06]-[PIE]

```mermaid
pie showData
  title Label
  "A" : 1
  "B" : 2
```
Values sum above `0`, labels are quoted, and `showData` prints percentages; donut, legend, and slice highlight are 11.16.0+.

## [07]-[QUADRANT_CHART]

```mermaid
quadrantChart
  x-axis Low --> High
  y-axis Low --> High
  A:::c1: [0.3, 0.6]
  B: [0.8, 0.1] color: #ff3300, radius: 10
  classDef c1 color: #109060
```
Coordinates bind to `0` through `1` and quadrants number `1` top-right through `4` bottom-right; per-point styling trails the coordinates (`color`, `radius`, `stroke-width`, `stroke-color`) and `:::class` plus `classDef` styles a point. Non-ASCII unquoted labels — CJK, emoji, accented Latin-1 — parse from 11.16.0+.

## [08]-[SANKEY]

```mermaid
sankey

Grid,Homes,113.7
Grid,Industry,342.2
```
Stable keyword `sankey` since 11.10.0 (formerly `sankey-beta`); the body is three-column CSV `source,target,value` with blank lines allowed and CSV quoting for embedded commas. Config carries `linkColor`, `nodeAlignment`, `showValues`, `prefix`, `suffix`, and the 11.15.0+ `labelStyle`, `nodeWidth`, `nodePadding`, and `nodeColors` name-to-hex map.

## [09]-[XYCHART]

```mermaid
xychart
  x-axis "Date" ["Apr 2022", "Feb 2023"]
  y-axis "Params (B)" 0 --> 600
  line [540 "PaLM", 65 "LLaMA-65B"]
```
Stable keyword `xychart` since 11.10.0 (formerly `xychart-beta`); `xychart horizontal` flips orientation and each `bar` or `line` array matches the x-axis category count. A line point label `line [540 "PaLM"]` is 11.16.0+ and renders on `line` only — accepted but ignored on `bar` — while bar values print through `showDataLabel`/`showDataLabelOutsideBar` and `xAxis.labelRotation` rotates tick labels.

## [10]-[RADAR]

```mermaid
radar-beta
  axis m["Math"], s["Science"], e["English"]
  curve alice["Alice"]{85, 90, 80}
  curve keyed{ m: 85, s: 90, e: 80 }
```
Available 11.6.0+; `axis` names the axes, a positional curve `alice["Alice"]{...}` follows axis order and a keyed curve `keyed{ m: 85, ... }` binds by axis id. `showLegend`, `graticule polygon|circle`, `ticks`, `max`, `min` tune the plot, config admits `axisScaleFactor` and `curveTension`, and theme variables nest under `radar:`.

## [11]-[GANTT]

```mermaid
gantt
  dateFormat YYYY-MM-DD
  excludes weekends
  section Work
  Task :active, a, 2026-01-01, 7d
```
Dates match `dateFormat`, `after taskId` and `until taskId` reference existing IDs, and modifiers are `done`, `active`, `crit`, `milestone`, `vert`; repeated `excludes`/`includes` entries are 11.16.0+.

## [12]-[TREEMAP]

```mermaid
treemap-beta
"Budget"
    "Operations"
        "Salaries": 700000
    "Marketing":::focus
        "Advertising": 400000
classDef focus fill:#f96,stroke:#333;
```
Indentation sets hierarchy and a leaf carries a numeric value; `:::class` plus `classDef` styles a node, and `valueFormat` formats values through d3-format grammar (`'$0,0'`) alongside `showValues`, `nodeWidth`, `diagramPadding`.

## [13]-[C4]

```mermaid
C4Context
  Person(a, "Actor")
  System(s, "System")
  Rel(a, s, "Uses")
```
The family covers `C4Context`, `C4Container`, `C4Component`, `C4Dynamic`, `C4Deployment`; an alias exists before `Rel()` and named parameters take `$`. It stays experimental and theming routes through `UpdateElementStyle`/`UpdateRelStyle`, not `themeVariables`.

## [14]-[ARCHITECTURE]

```mermaid
architecture-beta
  group api(cloud)[API]
  service db(database)[DB] in api
  service mcp(server)[MCP] in api
  db:R --> L:mcp
  align column db mcp
```
`group`, `service`, and `junction` place nodes, a member declares `in group`, edge ports are `T|B|L|R`, a group-boundary edge takes `{group}`, and an Iconify icon resolves as `pack:name`. `align row|column` orders members from 11.16.0+ and fails when it contradicts a directional edge; layout is cytoscape fcose, not ELK, with 11.15.0+ knobs `nodeSeparation`, `idealEdgeLengthMultiplier`, `edgeElasticity`, `numIter`, and `architecture.seed` (11.16.0+) is the deterministic lock since `randomize: false` alone does not guarantee identical renders.

## [15]-[PACKET]

```mermaid
packet
title UDP Packet
+16: "Source Port"
32-47: "Length"
48-63: "Checksum"
```
The keyword is `packet`, never `packet-beta`; `start-end: "name"` ranges and `+count: "name"` auto-counted fields (11.7.0+) mix in one diagram under an optional `title`. Theme-variable propagation is broken, so a packet diagram takes no theme.

## [16]-[TIMELINE]

```mermaid
timeline
  title Label
  section Period
  Period : Event A : Event B
```
A multi-event row repeats `:`, styling uses `cScale0` through `cScale11`, and timeline `direction` is 11.14.0+.

## [17]-[GITGRAPH]

```mermaid
gitGraph LR:
  commit id: "a"
  branch feature
  checkout feature
  commit id: "b"
```
Directions are `LR:`, `TB:`, `BT:`; a branch exists before checkout or merge, commit IDs stay unique, and cherry-picking a merge commit adds `parent:`.

## [18]-[KANBAN]

```mermaid
kanban
  Todo
    docs[Write blog post]
  test[Ready for test]
    t4[Parsing tests]@{ ticket: MC-2038, assigned: 'K.S', priority: 'High' }
```
Tasks indent under columns, metadata keys are `assigned`, `ticket`, `priority`, and priorities are exactly `Very High`, `High`, `Low`, `Very Low`; `kanban.ticketBaseUrl` links each ticket by substituting the task ticket for `#TICKET#`.

## [19]-[TREEVIEW]

```mermaid
treeView-beta
├── src/
│   ├── App.tsx :::highlight icon(logos:react) ## main component
│   └── index.ts ## entry point
├── Dockerfile
└── package.json
```
Available 11.14.0+ with box-drawing input from 11.16.0+, a trailing `/` marking a directory; annotations trail an entry as `:::class`, `## description`, and `icon(name)`/`icon(none)`. Config carries `showIcons`, `defaultIconPack`, `filenameIcons`, `extensionIcons`, and an unregistered icon renders as a question mark.

## [20]-[CYNEFIN]

```mermaid
cynefin-beta
  clear
    "Restart service"
  complicated
    "Analyze data"
  complex --> complicated : "Pattern found"
```
Available 11.16.0+; the five domains are `complex`, `complicated`, `clear`, `chaotic`, `confusion`, each holding quoted items, and a transition spells `domain --> domain : "label"`.

## [21]-[RAILROAD]

```mermaid
railroad-ebnf-beta
title "Optional Sign"

sign = "+" | "-" ;
number = sign? digit+ ;
```
Available 11.16.0+; the keyword selects the grammar parser — `railroad-ebnf-beta` for EBNF, `railroad-abnf-beta` for ABNF, `railroad-peg-beta` for PEG, and `railroad-beta` for Mermaid's intermediate constructors.

## [22]-[SWIMLANE]

```mermaid
swimlane-beta
  Intake --> Review
  Review --> Approve
```
Available 11.16.0+ as a standalone diagram reusing flowchart body syntax under a dedicated layered orthogonal layout; it honors `flowchart.defaultRenderer: elk`.
