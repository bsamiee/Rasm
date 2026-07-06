# [SYNTAX_EXTENDED]

The diagram roster beyond the five core types — each admitted row carries its working form and the traps that bind it, and a registered row outside the admitted scope carries its registry entry alone.

## [01]-[REGISTRY]

Pick a type by intent, then its section for the minimal fence, version gate, and traps; a dash marks a pre-11 introduction.

| [INDEX] | [TYPE]               | [INTENT]                 |
| :-----: | :------------------- | :----------------------- |
|  [01]   | `mindmap`            | radial hierarchy         |
|  [02]   | `block`              | manual grid layout       |
|  [03]   | `journey`            | phase sentiment          |
|  [04]   | `requirementDiagram` | requirement traceability |
|  [05]   | `pie`                | part-to-whole share      |
|  [06]   | `quadrantChart`      | two-axis position map    |
|  [07]   | `sankey`             | weighted directed flow   |
|  [08]   | `xychart`            | bar or line chart        |
|  [09]   | `radar-beta`         | multivariate profile     |
|  [10]   | `gantt`              | dated schedule           |
|  [11]   | `treemap-beta`       | area-weighted hierarchy  |
|  [12]   | `C4`                 | system landscape views   |
|  [13]   | `architecture-beta`  | infrastructure groups    |
|  [14]   | `packet`             | bit-field layout         |
|  [15]   | `timeline`           | chronological periods    |
|  [16]   | `gitGraph`           | branch and merge history |
|  [17]   | `kanban`             | workflow-stage board     |
|  [18]   | `treeView-beta`      | file-tree hierarchy      |
|  [19]   | `venn-beta`          | set-overlap regions      |
|  [20]   | `ishikawa-beta`      | cause-effect fishbone    |
|  [21]   | `wardley-beta`       | value-chain evolution    |
|  [22]   | `cynefin-beta`       | decision-domain sort     |
|  [23]   | `railroad-beta`      | grammar syntax rails     |
|  [24]   | `swimlane-beta`      | laned process flow       |
|  [25]   | `zenuml`             | sequence via zenuml      |

`venn-beta`, `ishikawa-beta`, and `wardley-beta` are registered and sit outside the admitted scope until a proven fence lands; `zenuml` is an external diagram the CLI registers.

The quantitative rows — pie, xychart, sankey, radar — serve only when the artifact must stay a mermaid fence; a data visualization routes to the dataviz lane.

## [02]-[MINDMAP]

```mermaid
mindmap
  Root
    [Branch A]
      (Leaf A1)
      (Leaf A2)
    ["`**Branch B**
    second line`"]
      (Leaf B1)
```

Root leads; consistent indentation sets depth, mixed tabs and spaces are rejected, and explicit edges are invalid.

## [03]-[BLOCK]

```mermaid
block
  columns 3
  a["A"] b:2
  c space d
  block:g:2
    columns 2
    h i
  end
  a -- "link" --> g
```

The keyword is `block`; `columns N` precedes a row, a `:n` span widens a block, `space` inserts a filler, and a bare block without a span is valid. A nested `block:id:span ... end` holds its own `columns`, and a labeled edge joins two blocks. A block arrow is `blockArrowId<["Label"]>(dir)` with `dir` one of `right`, `left`, `up`, `down`, `x`, `y`, or a compound like `(x, down)`.

## [04]-[JOURNEY]

```mermaid
---
config:
  theme: base
  themeVariables:
    darkMode: true
    textColor: "#F8F8F2"
    titleColor: "#F8F8F2"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
---
journey
  title Label
  section Intake
    Submit: 3: Actor
    Review: 5: Actor, Peer
  section Resolve
    Close: 2: Peer
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
  element ent {
    type: doc
  }
  ent - satisfies -> req
  req <- traces - ent
```

Types are `requirement`, `functionalRequirement`, `interfaceRequirement`, `performanceRequirement`, `physicalRequirement`, `designConstraint`; `risk` takes `Low`/`Medium`/`High` and `verifymethod` takes `Analysis`/`Inspection`/`Test`/`Demonstration`. Relations `contains`, `copies`, `derives`, `satisfies`, `verifies`, `refines`, `traces` spell both `a - satisfies -> b` and `b <- traces - a`, quoted text carries markdown, and the diagram takes `direction` plus the hand-drawn look.

## [06]-[PIE]

```mermaid
pie showData
  title Label
  "Alpha" : 45
  "Beta" : 30
  "Gamma" : 15
  "Delta" : 10
```

Values sum above `0`, labels are quoted, and `showData` prints percentages; donut, legend, and slice highlight compose on it.

## [07]-[QUADRANT_CHART]

```mermaid
---
config:
  theme: base
  themeVariables:
    darkMode: true
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
---
quadrantChart
  x-axis Low --> High
  y-axis Low --> High
  quadrant-1 Promote
  quadrant-2 Assess
  quadrant-3 Retire
  quadrant-4 Hold
  A:::c1: [0.3, 0.6]
  B: [0.8, 0.1] color: #FFB86C, radius: 10
  classDef c1 color: #50FA7B
```

Coordinates bind to `0` through `1` and quadrants number `1` top-right through `4` bottom-right; per-point styling trails the coordinates (`color`, `radius`, `stroke-width`, `stroke-color`) and `:::class` plus `classDef` styles a point. Non-ASCII unquoted labels — CJK, emoji, accented Latin-1 — parse unquoted.

## [08]-[SANKEY]

```mermaid
---
config:
  sankey:
    labelStyle: outlined
    nodeWidth: 15
    nodePadding: 20
---
sankey

Source,Hub,120
Source,Store,80
Hub,Homes,70
Hub,Industry,50
Store,Homes,30
Store,Industry,50
```

The keyword is `sankey`; the body is three-column CSV `source,target,value` with blank lines allowed and CSV quoting for embedded commas. Config carries `linkColor`, `nodeAlignment`, `showValues`, `prefix`, `suffix`, the styling knobs, and a `nodeColors` name-to-hex map.

## [09]-[XYCHART]

```mermaid
---
config:
  xyChart:
    showDataLabel: true
    xAxis:
      labelRotation: 45
---
xychart
  title "Model Scale"
  x-axis "Quarter" ["Q1", "Q2", "Q3"]
  y-axis "Params (B)" 0 --> 600
  bar [120, 340, 540]
  line [120 "Small", 340 "Mid", 540 "Large"]
```

The keyword is `xychart`; `xychart horizontal` flips orientation and each `bar` or `line` array matches the x-axis category count. Line point labels render on `line` only — accepted but ignored on `bar` — while `showDataLabelOutsideBar` pushes bar values past the bar edge.

## [10]-[RADAR]

```mermaid
---
config:
  theme: base
  themeVariables:
    darkMode: true
    textColor: "#F8F8F2"
    titleColor: "#F8F8F2"
    cScale0: "#FF79C6"
    cScale1: "#8BE9FD"
    radar:
      axisColor: "#6272A4"
      graticuleColor: "#44475A"
      curveOpacity: 0.5
---
radar-beta
  axis m["Math"], s["Science"], e["English"]
  curve alice["Alice"]{85, 90, 80}
  curve keyed{ m: 85, s: 90, e: 80 }
  showLegend true
  graticule polygon
  ticks 5
  max 100
  min 0
```

`axis` names the axes, a positional curve `alice["Alice"]{...}` follows axis order and a keyed curve `keyed{ m: 85, ... }` binds by axis id. `graticule` also accepts `circle`, config admits `axisScaleFactor` and `curveTension`, and theme variables nest under `radar:`.

## [11]-[GANTT]

```mermaid
---
config:
  theme: base
  themeVariables:
    darkMode: true
    textColor: "#F8F8F2"
    taskTextOutsideColor: "#F8F8F2"
    gridColor: "#6272A4"
    sectionBkgColor: "#21222C"
    altSectionBkgColor: "#282A36"
    taskBkgColor: "#44475A"
    taskBorderColor: "#BD93F9"
    activeTaskBkgColor: "#6272A4"
    activeTaskBorderColor: "#BD93F9"
    critBkgColor: "#FF5555"
    critBorderColor: "#FF5555"
    todayLineColor: "#FF79C6"
---
gantt
  dateFormat YYYY-MM-DD
  excludes weekends
  section Build
  Draft :active, a, 2026-01-01, 7d
  Review :crit, b, after a, 3d
  section Release
  Ship :milestone, m, after b, 0d
```

Dates match `dateFormat`, `after taskId` and `until taskId` reference existing IDs, and modifiers are `done`, `active`, `crit`, `milestone`, `vert`; repeated `excludes` and `includes` entries stack.

## [12]-[TREEMAP]

```mermaid
---
config:
  treemap:
    valueFormat: '$0,0'
---
treemap-beta
"Budget"
    "Operations"
        "Salaries": 700000
    "Marketing":::focus
        "Advertising": 400000
classDef focus fill:#44475A,stroke:#FFB86C,color:#F8F8F2
```

Indentation sets hierarchy and a leaf carries a numeric value; `:::class` plus `classDef` styles a node, and `valueFormat` formats values through d3-format grammar alongside `showValues`, `nodeWidth`, `diagramPadding`.

## [13]-[C4]

```mermaid
C4Context
  Person_Ext(user, "External User")
  System(app, "App")
  System(store, "Store")
  Rel(user, app, "Uses")
  Rel(app, store, "Notifies")
  UpdateElementStyle(app, $fontColor="#F8F8F2", $bgColor="#44475A", $borderColor="#BD93F9")
  UpdateElementStyle(store, $fontColor="#F8F8F2", $bgColor="#44475A", $borderColor="#BD93F9")
  UpdateRelStyle(user, app, $textColor="#F8F8F2", $lineColor="#FF79C6")
  UpdateRelStyle(app, store, $textColor="#F8F8F2", $lineColor="#FF79C6")
```

The family covers `C4Context`, `C4Container`, `C4Component`, `C4Dynamic`, `C4Deployment`; an alias exists before `Rel()` and named parameters take `$`. `Enterprise_Boundary` nests boundaries, `System_Ext` marks an external system, and `BiRel` draws a bidirectional relation. Theming routes through `UpdateElementStyle`/`UpdateRelStyle`, not `themeVariables`; the fence stays flat by ruling since boundary label color is uncontrollable.

## [14]-[ARCHITECTURE]

```mermaid
---
config:
  theme: base
  themeVariables:
    darkMode: true
    textColor: "#F8F8F2"
    primaryTextColor: "#F8F8F2"
    archEdgeColor: "#FF79C6"
    archGroupBorderColor: "#6272A4"
  architecture:
    seed: 7
---
architecture-beta
  group api(cloud)[API]
  group data(cloud)[Data]
  service db(database)[DB] in data
  service mcp(server)[MCP] in api
  junction jn in api
  db{group}:R --> L:mcp
  mcp:B --> T:jn
  align column mcp jn
```

`group`, `service`, and `junction` place nodes, a member declares `in group`, edge ports are `T|B|L|R`, a group-boundary edge takes `{group}`, and an Iconify icon resolves as `pack:name`. `align row|column` orders members and fails when it contradicts a directional edge; layout is cytoscape fcose, not ELK, with knobs `nodeSeparation`, `idealEdgeLengthMultiplier`, `edgeElasticity`, `numIter`, and `architecture.seed` is the deterministic lock since `randomize: false` alone does not guarantee identical renders. Two siblings sharing one logical position overlap — `align row|column` or a junction separates them — and long group names collide, so a group label stays short.

## [15]-[PACKET]

```mermaid
packet
title UDP Packet
+16: "Source Port"
+16: "Destination Port"
32-47: "Length"
48-63: "Checksum"
```

The keyword is `packet`, never `packet-beta`; `start-end: "name"` ranges and `+count: "name"` auto-counted fields mix in one diagram under an optional `title`. Theme-variable propagation is broken, so a packet diagram takes no theme.

## [16]-[TIMELINE]

```mermaid
timeline
  title Label
  section Phase One
    2021 : Kickoff : Draft
  section Phase Two
    2022 : Review : Release
```

A multi-event row repeats `:`, styling uses `cScale0` through `cScale11`, and timeline takes `direction`.

## [17]-[GITGRAPH]

```mermaid
gitGraph LR:
  commit id: "a"
  branch feature
  checkout feature
  commit id: "b"
  checkout main
  merge feature
  commit id: "c" tag: "v1.0"
```

Directions are `LR:`, `TB:`, `BT:`; a branch exists before checkout or merge, commit IDs stay unique, and cherry-picking a merge commit adds `parent:`.

## [18]-[KANBAN]

```mermaid
---
config:
  kanban:
    ticketBaseUrl: '<ticket-url>#TICKET#'
---
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

The tree parses box-drawing input, a trailing `/` marking a directory; annotations trail an entry as `:::class`, `## description`, and `icon(name)`/`icon(none)`. Config carries `showIcons`, `defaultIconPack`, `filenameIcons`, `extensionIcons`, and an unregistered icon renders as a question mark.

## [20]-[CYNEFIN]

```mermaid
cynefin-beta
  clear
    "Restart service"
  complicated
    "Analyze data"
  clear --> complicated : "Pattern found"
```

The five domains are `complex`, `complicated`, `clear`, `chaotic`, `confusion`, each holding quoted items, and a transition spells `domain --> domain : "label"`. The current geometry draws axis-aligned rectangles, not the canonical curved domain boundaries.

## [21]-[RAILROAD]

```mermaid
---
config:
  theme: base
  themeVariables:
    darkMode: true
    textColor: "#F8F8F2"
    primaryColor: "#44475A"
    primaryBorderColor: "#BD93F9"
---
railroad-ebnf-beta
title "Optional Sign"

sign = "+" | "-" ;
number = sign? digit+ ;
```

The keyword selects the grammar parser — `railroad-ebnf-beta` for EBNF, `railroad-abnf-beta` for ABNF, `railroad-peg-beta` for PEG, and `railroad-beta` for Mermaid's intermediate constructors.

## [22]-[SWIMLANE]

```mermaid
swimlane-beta
  Intake --> Review
  Review --> Approve
```

A standalone diagram reusing flowchart body syntax under a dedicated layered orthogonal layout, it honors `flowchart.defaultRenderer: elk`. `look: neo` deforms swimlane output, so swimlane holds `look: classic`; its PNG export diverges from its SVG in current builds.
