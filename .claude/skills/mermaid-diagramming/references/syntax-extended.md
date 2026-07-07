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
|  [26]   | `eventmodeling`      | command-event timeline   |

`zenuml` is an external diagram the CLI registers.

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

The fence asserts one root owning two branch families to leaf depth; the family mis-handles `accTitle`/`accDescr`, so the relation sentence rides beside the fence. Root leads; consistent indentation sets depth, mixed tabs and spaces are rejected, and explicit edges are invalid.

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

The fence asserts a three-column raster whose nested group takes one labeled link; the family mis-handles `accTitle`/`accDescr`, so the relation sentence rides beside the fence. The keyword is `block`; `columns N` precedes a row, a `:n` span widens a block, `space` inserts a filler, and a bare block without a span is valid. A nested `block:id:span ... end` holds its own `columns`, and a labeled edge joins two blocks. A block arrow is `blockArrowId<["Label"]>(dir)` with `dir` one of `right`, `left`, `up`, `down`, `x`, `y`, or a compound like `(x, down)`.

## [04]-[JOURNEY]

```mermaid
---
config:
  theme: base
  look: classic
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    titleColor: "#F8F8F2"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    fillType0: "#FF79C6"
    fillType1: "#8BE9FD"
    fillType2: "#50FA7B"
    fillType3: "#FFB86C"
    fillType4: "#BD93F9"
    fillType5: "#FF5555"
    fillType6: "#FFD866"
    fillType7: "#6272A4"
---
journey
  accTitle: Journey demo
  accDescr: Two intake steps and a close scored across actor and peer.
  title Label
  section Intake
    Submit: 3: Actor
    Review: 5: Actor, Peer
  section Resolve
    Close: 2: Peer
```

Scores are integers `1` through `5`; a task belongs under a `section`, an actor needs no declaration, and an out-of-range score is invalid. The score faces and section fills read `fillType0`–`fillType7`, never `primaryColor` — a journey themed without the `fillType` set renders its whole point unstyled.

## [05]-[REQUIREMENT_DIAGRAM]

```mermaid
---
config:
  theme: base
  look: classic
  htmlLabels: false
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    requirementBackground: "#44475A"
    requirementBorderColor: "#BD93F9"
    requirementTextColor: "#F8F8F2"
    relationColor: "#FF79C6"
    relationLabelColor: "#F8F8F2"
    relationLabelBackground: "#21222C"
---
requirementDiagram
  accTitle: Requirement trace demo
  accDescr: One document element satisfying and tracing a functional requirement.
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
  classDef primary fill:#44475A,stroke:#BD93F9,color:#F8F8F2
  classDef external fill:#282A36,stroke:#8BE9FD,color:#F8F8F2
  class req primary
  class ent external
```

Types are `requirement`, `functionalRequirement`, `interfaceRequirement`, `performanceRequirement`, `physicalRequirement`, `designConstraint`; `risk` takes `Low`/`Medium`/`High` and `verifymethod` takes `Analysis`/`Inspection`/`Test`/`Demonstration`. Relations `contains`, `copies`, `derives`, `satisfies`, `verifies`, `refines`, `traces` spell both `a - satisfies -> b` and `b <- traces - a`, quoted text carries markdown, and the diagram takes `direction` plus the hand-drawn look. Box fills theme through `classDef`/`class`, never `requirement*` variables alone — those leave the box naked cream — and `htmlLabels: false` stacks the body attributes cleanly; the relation-label background is engine-derived and ignores `relationLabelBackground`.

## [06]-[PIE]

```mermaid
---
config:
  theme: base
  look: classic
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    pieSectionTextColor: "#282A36"
    pieLegendTextColor: "#F8F8F2"
    pieTitleTextColor: "#F8F8F2"
    pieStrokeColor: "#282A36"
    pie1: "#FF79C6"
    pie2: "#8BE9FD"
    pie3: "#50FA7B"
    pie4: "#FFB86C"
---
pie showData
  accTitle: Share demo
  accDescr: Four labeled slices with printed percentages.
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
  look: classic
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
---
quadrantChart
  accTitle: Two-axis position demo
  accDescr: Two items placed across promote, assess, retire, and hold quadrants.
  x-axis Low --> High
  y-axis Low --> High
  quadrant-1 Promote
  quadrant-2 Assess
  quadrant-3 Retire
  quadrant-4 Hold
  A:::success: [0.3, 0.6]
  B: [0.8, 0.1] color: #FFB86C, radius: 10
  classDef success color: #50FA7B
```

Coordinates bind to `0` through `1` and quadrants number `1` top-right through `4` bottom-right; per-point styling trails the coordinates (`color`, `radius`, `stroke-width`, `stroke-color`) and `:::class` plus `classDef` styles a point. Non-ASCII unquoted labels — CJK, emoji, accented Latin-1 — parse unquoted.

## [08]-[SANKEY]

```mermaid
---
config:
  theme: base
  look: classic
  sankey:
    labelStyle: outlined
    linkColor: gradient
    nodeWidth: 15
    nodePadding: 20
    nodeColors:
      Source: "#BD93F9"
      Hub: "#FF79C6"
      Store: "#FFB86C"
      Homes: "#50FA7B"
      Industry: "#8BE9FD"
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    titleColor: "#F8F8F2"
---
sankey

Source,Hub,120
Source,Store,80
Hub,Homes,70
Hub,Industry,50
Store,Homes,30
Store,Industry,50
```

The fence asserts a source volume splitting across a hub and a store into two demand sinks; the family rejects `accTitle`/`accDescr` at parse, so the relation sentence rides beside the fence. The keyword is `sankey`; the body is three-column CSV `source,target,value` with blank lines allowed and CSV quoting for embedded commas. Config carries `linkColor` (`gradient`, `source`, `target`, or a hex), `nodeAlignment`, `showValues`, `prefix`, `suffix`, the styling knobs, and the `nodeColors` name-to-hex map — a committed sankey maps every node to a palette hex so no node falls to the engine default.

## [09]-[XYCHART]

```mermaid
---
config:
  theme: base
  look: classic
  xyChart:
    showDataLabel: true
    xAxis:
      labelRotation: 45
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    xyChart:
      backgroundColor: "#282A36"
      titleColor: "#F8F8F2"
      xAxisLabelColor: "#F8F8F2"
      xAxisTitleColor: "#F8F8F2"
      xAxisTickColor: "#6272A4"
      xAxisLineColor: "#6272A4"
      yAxisLabelColor: "#F8F8F2"
      yAxisTitleColor: "#F8F8F2"
      yAxisTickColor: "#6272A4"
      yAxisLineColor: "#6272A4"
      plotColorPalette: "#BD93F9, #FF79C6"
---
xychart
  accTitle: Bar and line demo
  accDescr: Quarterly scale as bars with a labeled line overlay.
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
  look: classic
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
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
  accTitle: Profile demo
  accDescr: A positional and a keyed curve compared across three axes.
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
  look: classic
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    taskTextOutsideColor: "#F8F8F2"
    gridColor: "#6272A4"
    sectionBkgColor: "#21222C"
    altSectionBkgColor: "#282A36"
    taskBkgColor: "#44475A"
    taskBorderColor: "#BD93F9"
    activeTaskBkgColor: "#6272A4"
    activeTaskBorderColor: "#BD93F9"
    critBkgColor: "#FF555580"
    critBorderColor: "#FF5555"
    todayLineColor: "#FF79C6"
  themeCSS: ".sectionTitle{font-size:12px;font-weight:600}.taskText,.taskTextOutsideRight,.taskTextOutsideLeft{font-size:12px}"
---
gantt
  accTitle: Schedule demo
  accDescr: A draft-review-ship chain with an active task, a critical review, and a milestone.
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
  theme: base
  look: classic
  treemap:
    valueFormat: '$0,0'
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    titleColor: "#F8F8F2"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
---
treemap-beta
accTitle: Area hierarchy demo
accDescr: A budget decomposed into operations and marketing with classed branches.
"Budget"
    "Operations":::ops
        "Salaries": 700000
    "Marketing":::focus
        "Advertising": 400000
classDef ops fill:#44475A,stroke:#8BE9FD,color:#F8F8F2
classDef focus fill:#44475A,stroke:#FFB86C,color:#F8F8F2
```

Indentation sets hierarchy and a leaf carries a numeric value; `:::class` plus `classDef` styles a node, and `valueFormat` formats values through d3-format grammar alongside `showValues`, `nodeWidth`, `diagramPadding`. Every top-level branch carries a class — a single classed node beside default siblings reads as an accident, not a system.

## [13]-[C4]

```mermaid
C4Context
  accTitle: Landscape demo
  accDescr: An external user reaching an app that notifies a store, every element and relation styled.
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
  look: classic
  architecture:
    seed: 7
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    primaryTextColor: "#F8F8F2"
    archEdgeColor: "#FF79C6"
    archGroupBorderColor: "#D6BCFA"
---
architecture-beta
  accTitle: Infrastructure demo
  accDescr: A database group feeding an MCP service through a junction inside the API group.
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
accTitle: Bit layout demo
accDescr: UDP header fields across two 32-bit rows with no gaps or overlaps.
title UDP Packet
+16: "Source Port"
+16: "Destination Port"
32-47: "Length"
48-63: "Checksum"
```

The keyword is `packet`, never `packet-beta`; `start-end: "name"` ranges and `+count: "name"` auto-counted fields mix in one diagram under an optional `title`. Theme-variable propagation is broken, so a packet diagram takes no theme.

## [16]-[TIMELINE]

```mermaid
---
config:
  theme: base
  look: classic
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    cScale0: "#FF79C6"
    cScale1: "#8BE9FD"
    cScaleLabel0: "#282A36"
    cScaleLabel1: "#282A36"
---
timeline
  accTitle: Period demo
  accDescr: Two phases each carrying dated events in chronological order.
  title Label
  section Phase One
    2021 : Kickoff : Draft
  section Phase Two
    2022 : Review : Release
```

A multi-event row repeats `:`, styling uses `cScale0` through `cScale11`, and timeline takes `direction`.

## [17]-[GITGRAPH]

```mermaid
---
config:
  theme: base
  look: classic
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    git0: "#BD93F9"
    git1: "#FF79C6"
    gitBranchLabel0: "#282A36"
    gitBranchLabel1: "#282A36"
    commitLabelColor: "#F8F8F2"
    commitLabelBackground: "#44475A"
    tagLabelColor: "#282A36"
    tagLabelBackground: "#FFD866"
---
gitGraph LR:
  accTitle: Branch history demo
  accDescr: A feature branch cut from main and merged back with a release tag.
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

The fence asserts two queues holding one card each, one carrying ticket metadata; the family mis-handles `accTitle`/`accDescr` as columns, so the relation sentence rides beside the fence. Tasks indent under columns, metadata keys are `assigned`, `ticket`, `priority`, and priorities are exactly `Very High`, `High`, `Low`, `Very Low`; `kanban.ticketBaseUrl` links each ticket by substituting the task ticket for `#TICKET#`.

## [19]-[TREEVIEW]

```mermaid
treeView-beta
accTitle: File tree demo
accDescr: A source folder with annotated entries beside root files.
├── src/
│   ├── App.tsx :::highlight icon(logos:react) ## main component
│   └── index.ts ## entry point
├── Dockerfile
└── package.json
```

The tree parses box-drawing input, a trailing `/` marking a directory; annotations trail an entry as `:::class`, `## description`, and `icon(name)`/`icon(none)`. Config carries `showIcons`, `defaultIconPack`, `filenameIcons`, `extensionIcons`, and an unregistered icon renders as a question mark.

## [20]-[CYNEFIN]

```mermaid
---
config:
  theme: base
  look: classic
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    cynefin:
      textColor: "#F8F8F2"
      labelColor: "#F8F8F2"
      boundaryColor: "#6272A4"
      arrowColor: "#FF79C6"
      complexBg: "#44475A"
      complicatedBg: "#21222C"
      clearBg: "#44475A"
      chaoticBg: "#21222C"
      confusionBg: "#282A36"
---
cynefin-beta
  accTitle: Domain sort demo
  accDescr: Practices sorted into clear and complicated with one labeled reclassification.
  clear
    "Restart service"
  complicated
    "Analyze data"
  clear --> complicated : "Pattern found"
```

The five domains are `complex`, `complicated`, `clear`, `chaotic`, `confusion`, each holding quoted items, and a transition spells `domain --> domain : "label"`; domain fills, boundary, cliff, and arrow color nest under `cynefin:`. The current geometry draws axis-aligned rectangles, not the canonical curved domain boundaries.

## [21]-[RAILROAD]

```mermaid
---
config:
  theme: base
  look: classic
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    primaryColor: "#44475A"
    primaryBorderColor: "#BD93F9"
---
railroad-ebnf-beta
accTitle: Grammar demo
accDescr: A signed number production over an optional sign rule.
title "Optional Sign"

sign = "+" | "-" ;
number = sign? digit+ ;
```

The keyword selects the grammar parser — `railroad-ebnf-beta` for EBNF, `railroad-abnf-beta` for ABNF, `railroad-peg-beta` for PEG, and `railroad-beta` for Mermaid's intermediate constructors. Production, terminal, and reference nodes share the `primaryColor` fill; the beta carries no class route, so grammar-role differentiation waits on the engine, never on an invented selector.

## [22]-[SWIMLANE]

```mermaid
---
config:
  theme: base
  look: classic
  flowchart:
    defaultRenderer: elk
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    tertiaryColor: "#21222C"
    lineColor: "#FF79C6"
---
swimlane-beta
  accTitle: Lane demo
  accDescr: An intake flow stepping through review to approval.
  Intake([Intake]) --> Review[Review]
  Review --> Approve([Approve])
  classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
  classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
  classDef success fill:#50FA7B,stroke:#50FA7B,color:#282A36
  class Intake boundary
  class Review primary
  class Approve success
```

A standalone diagram reusing flowchart body syntax under a dedicated layered orthogonal layout, it honors `flowchart.defaultRenderer: elk` and the flowchart general variables — a swimlane ships the flowchart theming floor, never a bare header. `tertiaryColor` sets the lane background — left unset it derives to an off-palette olive. `look: neo` deforms swimlane output, so swimlane holds `look: classic`; its PNG export diverges from its SVG in current builds.

## [23]-[EVENTMODELING]

```mermaid
---
config:
  theme: base
  look: classic
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    emUiFill: "#44475A"
    emUiStroke: "#BD93F9"
    emCommandFill: "#44475A"
    emCommandStroke: "#8BE9FD"
    emEventFill: "#44475A"
    emEventStroke: "#FFB86C"
    emProcessorFill: "#21222C"
    emProcessorStroke: "#6272A4"
    emReadModelFill: "#21222C"
    emReadModelStroke: "#50FA7B"
    emSwimlaneBackgroundOdd: "#282A36"
    emSwimlaneBackgroundStroke: "#44475A"
    emArrowhead: "#FF79C6"
    emRelationStroke: "#FF79C6"
---
eventmodeling
  tf 01 ui CartUI { "sku": "A1" }
  tf 02 cmd AddItem [[AddItem]]
  tf 03 evt ItemAdded `json`{ "qty": 1 }
  rf 10 pcr InventoryProcessor
```

The fence asserts a cart UI issuing a command whose event feeds an inventory processor; `accTitle`/`accDescr` break the parser before the frames, so the relation sentence rides beside the fence. `tf`/`timeframe` orders frames left to right and `rf`/`resetframe` restarts the clock; frame kinds are `ui`, `cmd`, `evt`, `pcr` (processor), and `rmo` (read model), relations infer from frame order, and an inline `{ ... }` or `` `json`{ ... } `` payload annotates a frame. Namespaced frame ids map onto swimlanes, and each frame kind reads its own `em*Fill`/`em*Stroke` pair — the committed fence themes every kind it uses.

## [24]-[VENN]

```mermaid
---
config:
  theme: base
  look: classic
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    vennTitleTextColor: "#F8F8F2"
    vennSetTextColor: "#F8F8F2"
    venn1: "#BD93F9"
    venn2: "#8BE9FD"
    venn3: "#50FA7B"
---
venn-beta
  title Coverage Overlap
  set A["Authored"]: 80
  set B["Rendered"]: 65
  union A,B: 52
```

The fence asserts two weighted sets sharing a measured overlap; the family rejects `accTitle`/`accDescr` at parse, so the relation sentence rides beside the fence. `set id["Label"]: size` declares a weighted set and `union A,B: size` sizes an overlap region; up to eight sets read `venn1`–`venn8`. Higher-arity unions list every member id, and an unlabeled set renders its id.

## [25]-[WARDLEY]

```mermaid
---
config:
  theme: base
  look: classic
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    wardley:
      backgroundColor: "#282A36"
      axisColor: "#6272A4"
      axisTextColor: "#F8F8F2"
      gridColor: "#44475A"
      componentFill: "#44475A"
      componentStroke: "#BD93F9"
      componentLabelColor: "#F8F8F2"
      linkStroke: "#FF79C6"
      evolutionStroke: "#50FA7B"
---
wardley-beta
  accTitle: Evolution map demo
  accDescr: A user need anchored on a CLI capability that depends on a browser component.
  title Render Capability
  anchor User [0.95, 0.6]
  component MermaidCLI [0.72, 0.55]
  component Browser [0.48, 0.4]
  User->MermaidCLI
  MermaidCLI->Browser
```

Coordinates are `[visibility, evolution]` on `0`–`1`; `anchor` places a user need, `component` a capability, `->` a dependency link. OWM grammar adds `evolve`, `pipeline`, `note`, inertia markers, and build/buy/outsource annotations; theme variables nest under `wardley:`.

## [26]-[ISHIKAWA]

```mermaid
---
config:
  theme: base
  look: classic
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    textColor: "#F8F8F2"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
---
ishikawa-beta
  "Render Failure"
    Browser
      "Chromium missing"
      "Sandbox denied"
    Assets
      "Remote icon"
      "Remote image"
    Syntax
      "Beta drift"
      "Reserved word"
```

The fence asserts a render failure traced to browser, asset, and syntax cause families; the family mis-handles `accTitle`/`accDescr`/`title` as spurious head nodes, so the relation sentence rides beside the fence. The quoted head names the effect, top-level identifiers are cause categories, and quoted children are causes; depth rides indentation. The beta reads the general variables only.
