# [STYLING]

The engine's full styling grammar — every link form, node shape, container, and style statement under one ruled precedence. Color assignment rides the palette layer; this reference owns the mechanical surface.

Sections: [01] edges and links - [02] shape registry - [03] containers - [04] precedence - [05] per-type matrix - [06] per-family floors - [07] consistency laws - [08] scope.

## [01]-[EDGES]

The flowchart endpoint matrix is the stroke family crossed with the endpoint marker. Every cell is a working link; an open form omits the arrowhead, a marker on the left mirrors the right.

| [INDEX] | [STROKE]  | [OPEN]    | [POINT]    | [CIRCLE]   | [CROSS]    | [POINT_BOTH] | [CIRCLE_BOTH] | [CROSS_BOTH] |
| :-----: | :-------- | :-------- | :--------- | :--------- | :--------- | :----------- | :------------ | :----------- |
|  [01]   | Normal    | `A --- B` | `A --> B`  | `A --o B`  | `A --x B`  | `A <--> B`   | `A o--o B`    | `A x--x B`   |
|  [02]   | Thick     | `A === B` | `A ==> B`  | `A ==o B`  | `A ==x B`  | `A <==> B`   | `A o==o B`    | `A x==x B`   |
|  [03]   | Dotted    | `A -.- B` | `A -.-> B` | `A -.-o B` | `A -.-x B` | `A <-.-> B`  | `A o-.-o B`   | `A x-.-x B`  |
|  [04]   | Invisible | `A ~~~ B` | —          | —          | —          | —            | —             | —            |

Extra dash, equals, or dot characters raise the minimum rank span; the renderer may still lengthen a link to satisfy layout, and the span caps at 10. With a middle label the extra characters sit right of the label: `A -- text ---> B`.

| [INDEX] | [NORMAL]    | [THICK]     | [DOTTED]     |
| :-----: | :---------- | :---------- | :----------- |
|  [01]   | `A --> B`   | `A ==> B`   | `A -.-> B`   |
|  [02]   | `A ---> B`  | `A ===> B`  | `A -..-> B`  |
|  [03]   | `A ----> B` | `A ====> B` | `A -...-> B` |

Labels attach as a pipe pair or a middle segment on any stroke family:

| [INDEX] | [FORM]               | [SYNTAX]           |
| :-----: | :------------------- | :----------------- |
|  [01]   | Pipe on arrow        | `A -->\|text\| B`  |
|  [02]   | Middle on arrow      | `A -- text --> B`  |
|  [03]   | Pipe on open link    | `A ---\|text\| B`  |
|  [04]   | Middle on open link  | `A -- text --- B`  |
|  [05]   | Middle on dotted     | `A -. text .-> B`  |
|  [06]   | Middle on thick      | `A == text ==> B`  |
|  [07]   | Bidirectional thick  | `A <== text ==> B` |
|  [08]   | Bidirectional dotted | `A <-. text .-> B` |

A closing `o` or `x` fused to the next id is lexed as an edge marker, not the id's first character.

| [INDEX] | [WRITTEN] | [PARSED_AS]        | [FIX]                      |
| :-----: | :-------- | :----------------- | :------------------------- |
|  [01]   | `A---oB`  | circle end, id `B` | `A--- oB` or capitalize id |
|  [02]   | `A---xB`  | cross end, id `B`  | `A--- xB` or capitalize id |

An edge id names one edge for behavior metadata: `A e1@--> B` then a metadata block on the id.

| [INDEX] | [METADATA]               | [EFFECT]                                                                     |
| :-----: | :----------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `e1@{ animate: true }`   | Toggles edge animation.                                                      |
|  [02]   | `e1@{ animation: fast }` | Fast preset; `slow` is the slow preset.                                      |
|  [03]   | `e1@{ curve: <value> }`  | Per-edge spline from the documented curve set, overriding the diagram curve. |

The metadata block owns only animate, animation, and curve; stroke width, dash, and label color ride `linkStyle` or an edge-id class — `class e1 edgeError` styles the id's stroke, width, dash, and label through the class system, the engine deriving the arrowhead from the resolved stroke. When an edge is modified more than once the last modification wins.

| [INDEX] | [FORM]                  | [EFFECT]                                  |
| :-----: | :---------------------- | :---------------------------------------- |
|  [01]   | `linkStyle N ...`       | Styles the edge at 0-based parse index N. |
|  [02]   | `linkStyle 1,2,7 ...`   | One style across the listed indices.      |
|  [03]   | `linkStyle default ...` | Styles every edge.                        |

The `linkStyle` property set is `stroke`, `stroke-width`, `stroke-dasharray`, `color`, and `fill`; a non-default edge that declares no `fill` gets `fill:none` injected. The built-in `animate`/`animation` metadata survives alongside an edge class, and a class-borne animation payload adds no second animation over it. A dash animation rides a class rather than the id — `classDef animate stroke-dasharray:9\,5,animation:dash 25s linear infinite` bound by `class e1 animate`, the dasharray comma escaped as `\,`.

Sequence lines carry no `linkStyle`; the stroke shape is the arrow token itself, and grouped backgrounds live in containers.

| [INDEX] | [FAMILY]      | [SOLID]   | [DOTTED]   |
| :-----: | :------------ | :-------- | :--------- |
|  [01]   | Line          | `A->B`    | `A-->B`    |
|  [02]   | Arrow         | `A->>B`   | `A-->>B`   |
|  [03]   | Cross         | `A-xB`    | `A--xB`    |
|  [04]   | Async         | `A-)B`    | `A--)B`    |
|  [05]   | Bidirectional | `A<<->>B` | `A<<-->>B` |

Activation shorthand fuses to the arrow: `A->>+B` opens an activation, `B-->>-A` closes it; every `+` balances a `-`.

State transitions take four forms and carry no per-transition style route — style the states, never the edge: `s1 --> s2`, labeled `s1 --> s2: label`, entry `[*] --> s1`, exit `s1 --> [*]`.

ER relation lines encode strength through the stroke: `--` is identifying, `..` is non-identifying, as in `CUSTOMER \|\|--o{ ORDER : places` against `CUSTOMER \|\|..o{ ORDER : places`.

Class relations pair an endpoint marker with a solid `--` or dashed `..` line.

| [INDEX] | [RELATION]  | [SYNTAX]                  |
| :-----: | :---------- | :------------------------ |
|  [01]   | Inheritance | `Animal <\|-- Duck`       |
|  [02]   | Composition | `Vehicle *-- Wheel`       |
|  [03]   | Aggregation | `Department o-- Employee` |
|  [04]   | Association | `Student --> Course`      |
|  [05]   | Dependency  | `Order ..> Payment`       |
|  [06]   | Realization | `Service ..\|> Impl`      |
|  [07]   | Solid link  | `A -- B`                  |
|  [08]   | Dashed link | `A .. B`                  |

A reverse form places the marker on the opposite end (`Duck --\|> Animal`); the grammar admits a marker on both ends of one line — `Animal <\|--\|> Zebra`, `A *--o B`, `C <..> D` — and a lollipop interface reads `bar ()-- foo`.

## [02]-[SHAPES]

`A@{ shape: <name> }` selects a registered shape by canonical short name or a public alias; the alias resolves to the canonical name, and an unknown name is rejected. This registry is the vocabulary.

| [INDEX] | [SHAPE]      | [ALIASES]                                                        | [ROLE]                |
| :-----: | :----------- | :--------------------------------------------------------------- | :-------------------- |
|  [01]   | `rect`       | `proc`, `process`, `rectangle`                                   | process               |
|  [02]   | `rounded`    | `event`                                                          | rounded event         |
|  [03]   | `stadium`    | `terminal`, `pill`                                               | terminal point        |
|  [04]   | `fr-rect`    | `subprocess`, `subproc`, `framed-rectangle`, `subroutine`        | subprocess            |
|  [05]   | `cyl`        | `db`, `database`, `cylinder`                                     | database              |
|  [06]   | `datastore`  | `data-store`                                                     | data store            |
|  [07]   | `circle`     | `circ`                                                           | start                 |
|  [08]   | `bang`       | none                                                             | bang                  |
|  [09]   | `cloud`      | none                                                             | cloud                 |
|  [10]   | `diam`       | `decision`, `diamond`, `question`                                | decision              |
|  [11]   | `hex`        | `hexagon`, `prepare`                                             | prepare conditional   |
|  [12]   | `lean-r`     | `lean-right`, `in-out`                                           | data input/output     |
|  [13]   | `lean-l`     | `lean-left`, `out-in`                                            | data input/output     |
|  [14]   | `trap-b`     | `priority`, `trapezoid-bottom`, `trapezoid`                      | priority action       |
|  [15]   | `trap-t`     | `manual`, `trapezoid-top`, `inv-trapezoid`                       | manual operation      |
|  [16]   | `dbl-circ`   | `double-circle`                                                  | stop                  |
|  [17]   | `text`       | none                                                             | text block            |
|  [18]   | `notch-rect` | `card`, `notched-rectangle`                                      | card                  |
|  [19]   | `lin-rect`   | `lined-rectangle`, `lined-process`, `lin-proc`, `shaded-process` | lined process         |
|  [20]   | `sm-circ`    | `start`, `small-circle`                                          | start                 |
|  [21]   | `fr-circ`    | `stop`, `framed-circle`                                          | stop                  |
|  [22]   | `fork`       | `join`                                                           | fork/join             |
|  [23]   | `hourglass`  | `collate`                                                        | collate               |
|  [24]   | `brace`      | `comment`, `brace-l`                                             | comment               |
|  [25]   | `brace-r`    | none                                                             | comment right         |
|  [26]   | `braces`     | none                                                             | braces both sides     |
|  [27]   | `bolt`       | `com-link`, `lightning-bolt`                                     | com link              |
|  [28]   | `doc`        | `document`                                                       | document              |
|  [29]   | `delay`      | `half-rounded-rectangle`                                         | delay                 |
|  [30]   | `h-cyl`      | `das`, `horizontal-cylinder`                                     | direct access storage |
|  [31]   | `lin-cyl`    | `disk`, `lined-cylinder`                                         | disk storage          |
|  [32]   | `curv-trap`  | `curved-trapezoid`, `display`                                    | display               |
|  [33]   | `div-rect`   | `div-proc`, `divided-rectangle`, `divided-process`               | divided process       |
|  [34]   | `tri`        | `extract`, `triangle`                                            | extract               |
|  [35]   | `win-pane`   | `internal-storage`, `window-pane`                                | internal storage      |
|  [36]   | `f-circ`     | `junction`, `filled-circle`                                      | junction              |
|  [37]   | `notch-pent` | `loop-limit`, `notched-pentagon`                                 | loop limit            |
|  [38]   | `flip-tri`   | `manual-file`, `flipped-triangle`                                | manual file           |
|  [39]   | `sl-rect`    | `manual-input`, `sloped-rectangle`                               | manual input          |
|  [40]   | `docs`       | `documents`, `st-doc`, `stacked-document`                        | multi-document        |
|  [41]   | `st-rect`    | `procs`, `processes`, `stacked-rectangle`                        | multi-process         |
|  [42]   | `bow-rect`   | `stored-data`, `bow-tie-rectangle`                               | stored data           |
|  [43]   | `cross-circ` | `summary`, `crossed-circle`                                      | summary               |
|  [44]   | `tag-doc`    | `tagged-document`                                                | tagged document       |
|  [45]   | `tag-rect`   | `tagged-rectangle`, `tag-proc`, `tagged-process`                 | tagged process        |
|  [46]   | `flag`       | `paper-tape`                                                     | paper tape            |
|  [47]   | `odd`        | none                                                             | odd                   |
|  [48]   | `lin-doc`    | `lined-document`                                                 | lined document        |

The classic bracket forms are shorthands over that registry.

| [INDEX] | [SHORTHAND]   | [SHAPE]    |
| :-----: | :------------ | :--------- |
|  [01]   | `A[Text]`     | `rect`     |
|  [02]   | `A(Text)`     | `rounded`  |
|  [03]   | `A([Text])`   | `stadium`  |
|  [04]   | `A[[Text]]`   | `fr-rect`  |
|  [05]   | `A[(Text)]`   | `cyl`      |
|  [06]   | `A((Text))`   | `circle`   |
|  [07]   | `A>Text]`     | `odd`      |
|  [08]   | `A{Text}`     | `diam`     |
|  [09]   | `A{{Text}}`   | `hex`      |
|  [10]   | `A[/Text/]`   | `lean-r`   |
|  [11]   | `A[\Text\]`   | `lean-l`   |
|  [12]   | `A[/Text\]`   | `trap-b`   |
|  [13]   | `A[\Text/]`   | `trap-t`   |
|  [14]   | `A(((Text)))` | `dbl-circ` |

Icon and image nodes are special shapes carrying their own parameter set; the icon pack registers at the host, never in the fence.

| [INDEX] | [ICON_KEY] | [VALUES]                                                 |
| :-----: | :--------- | :------------------------------------------------------- |
|  [01]   | `icon`     | Registered icon name such as `fa:user`.                  |
|  [02]   | `form`     | `square`, `circle`, `rounded`; omitted renders unframed. |
|  [03]   | `label`    | Text label.                                              |
|  [04]   | `pos`      | `t` or `b`; default bottom.                              |
|  [05]   | `h`        | Height; default and minimum 48.                          |

| [INDEX] | [IMAGE_KEY]  | [VALUES]                                                 |
| :-----: | :----------- | :------------------------------------------------------- |
|  [01]   | `img`        | Image URL.                                               |
|  [02]   | `label`      | Text label.                                              |
|  [03]   | `pos`        | `t` or `b`; default bottom.                              |
|  [04]   | `w`          | Image width.                                             |
|  [05]   | `h`          | Image height.                                            |
|  [06]   | `constraint` | `on` or `off`; default off, `on` preserves aspect ratio. |

A metadata block also accepts `label` to override the bracket text, and the `text` shape renders a borderless label-only node.

## [03]-[CONTAINERS]

Containers style through the id, never the title, and each type admits a bounded route.

| [INDEX] | [CONTAINER]        | [STYLE_ROUTE]                                                     | [UNSTYLEABLE]                        |
| :-----: | :----------------- | :---------------------------------------------------------------- | :----------------------------------- |
|  [01]   | Flowchart subgraph | `style id`, `class id`, `classDef` by id; inner `direction`       | title text                           |
|  [02]   | State composite    | class on plain states; composite class lands in the DOM           | `[*]` markers; composite fill/stroke |
|  [03]   | Sequence `box`     | named color, `rgb(...)`, `rgba(...)`, `transparent`, or text-only | —                                    |
|  [04]   | Sequence `rect`    | `rect rgb(...)` or `rect rgba(...)` background                    | named color                          |
|  [05]   | Block              | `class`/`style` by id, nesting                                    | title text                           |
|  [06]   | Architecture group | theme variables `archGroupBorder*`, `archEdge*`                   | in-diagram class/style               |
|  [07]   | Class namespace    | theme only                                                        | individual note/namespace            |

A subgraph names its id and title as `subgraph id [Title]`, nests, and takes an inner `direction`; that inner direction is dropped and inherited from the parent the moment any member node links outside the block. `style id` on the subgraph beats `clusterBkg`, and a class on the subgraph id colors the title text over `titleColor`.

## [04]-[PRECEDENCE]

Node styling resolves in application order, later and more specific winning.

- Theme variables and theme CSS set the diagram defaults.
- `classDef default` sets the fallback class where the type supports it — verified for flowchart, ER, and requirement.
- A named `classDef` layers through `class` or `:::` assignments; among conflicting classes on one node the later `classDef` definition wins, never the assignment order. The `classDef` declares at diagram root after the nodes it styles — declared above them it renders unstyled.
- Inline `style id ...` is the direct override and wins last — the engine emits it as an inline `!important` declaration; its property set is `fill`, `stroke`, `stroke-width`, `color`, and `stroke-dasharray`.
- `classDef` rules emit with `!important`, so a `themeCSS` rule without `!important` loses to any class on the same node.

Class assignment splits by family — flowchart binds one class per statement, ER admits a comma list on one node.

| [INDEX] | [FAMILY]  | [FORM]                | [SYNTAX]                        |
| :-----: | :-------- | :-------------------- | :------------------------------ |
|  [01]   | Flowchart | One class inline      | `A:::name`                      |
|  [02]   | Flowchart | One class across many | `class A,B name`                |
|  [03]   | ER        | Comma class list      | `CUSTOMER:::important,external` |

A flowchart node takes one class per statement: stacked `A:::a:::b` raises a parse error, and `class N a,b` binds the single literal class token `a,b` rather than two classes.

Edge styling resolves along a parallel chain: theme line variables, then `linkStyle default`, then positional `linkStyle N`, a repeated index resolving last-wins. A class on an edge id rides the edge group yet never beats a same-edge `linkStyle` stroke; edge metadata `e1@{ animate }`, `e1@{ animation }`, and `e1@{ curve }` owns behavior alone; `style e1 ...` on an edge id renders and silently no-ops; and a `linkStyle` on a `~~~` edge writes inline stroke that overrides the invisible class, turning the rank edge visible, so a rank-only edge never takes `linkStyle`.

```mermaid
---
config:
  theme: base
  flowchart:
    padding: 16
  themeCSS: ".nodeLabel{font-size:14px;font-weight:500}.edgeLabel{font-size:12.5px;font-weight:500}.cluster-label .nodeLabel{font-size:13px;font-weight:600}.cluster rect{stroke-width:1.5px}.edgePaths path{stroke-width:1.5px}"
  themeVariables:
    darkMode: true
    primaryColor: "#44475A"
    primaryBorderColor: "#BD93F9"
    primaryTextColor: "#F8F8F2"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#6272A4"
    edgeLabelBackground: "#44475A"
    titleColor: "#F8F8F2"
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
---
flowchart LR
  accTitle: Style precedence demo
  accDescr: One flowchart spending theme variables, classes, inline style, linkStyle, and edge metadata so later and more specific declarations win.
  Ingress@{ shape: lean-r, label: "Ingress" }
  Router{Route?}
  Cache[(Cache)]
  Worker[[Worker]]
  Sink@{ shape: cyl, label: "Sink" }
  subgraph Core [CORE]
    direction TB
    Router ==> Worker
    Worker e1@--> Cache
  end
  Ingress --> Router
  Router -.-> Sink
  Worker --o Sink
  Cache <--> Sink
  e1@{ curve: linear, animate: true }
  classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
  classDef data fill:#FFB86C,stroke:#FFB86C,color:#282A36
  classDef external fill:#8BE9FD,stroke:#8BE9FD,color:#282A36
  class Router primary
  class Cache data
  class Sink external
  style Cache fill:#282A36,stroke:#BD93F9,color:#F8F8F2
  linkStyle 3 stroke:#8BE9FD,color:#F8F8F2
```

## [05]-[TYPE_MATRIX]

Each diagram type accepts a bounded set of styling statements; `yes` is verified acceptance, `no` is no verified route, and the local mechanism is the type's own styling surface.

| [INDEX] | [TYPE]       | [CLASSDEF] | [TRIPLE_COLON] | [STYLE] | [LINKSTYLE] | [LOCAL]                                              |
| :-----: | :----------- | :--------: | :------------: | :-----: | :---------: | :--------------------------------------------------- |
|  [01]   | Flowchart    |    yes     |      yes       |   yes   |     yes     | edge ids, metadata, curves, animation, icon/image    |
|  [02]   | Sequence     |     no     |       no       |   no    |     no      | `box`/`rect` backgrounds — the family's one lever    |
|  [03]   | State        |    yes     |      yes       |   yes   |     no      | composite state classes; no transition route         |
|  [04]   | Class        |    yes     |      yes       |   yes   |     no      | relation arrows, lollipop interfaces                 |
|  [05]   | ER           |    yes     |      yes       |   yes   |     no      | identifying `--` vs non-identifying `..`             |
|  [06]   | Gantt        |     no     |       no       |   no    |     no      | `todayMarker`, section styles via config             |
|  [07]   | Pie          |     no     |       no       |   no    |     no      | ordinal `pie1`–`pie12` theme variables               |
|  [08]   | Quadrant     |    yes     |      yes       |   no    |     no      | point-local `color`/`radius`/`stroke-*`              |
|  [09]   | Timeline     |     no     |       no       |   no    |     no      | `cScale0`–`cScale11` theme variables                 |
|  [10]   | Mindmap      |     no     |      yes       |   no    |     no      | host-supplied classes, `::icon(...)`                 |
|  [11]   | Kanban       |     no     |       no       |   no    |     no      | task metadata block, config keys                     |
|  [12]   | GitGraph     |     no     |       no       |   no    |     no      | `git0`–`git7` and label theme variables              |
|  [13]   | Requirement  |    yes     |      yes       |   yes   |     no      | direct requirement/element styling                   |
|  [14]   | Architecture |     no     |       no       |   no    |     no      | `archEdge*`/`archGroupBorder*` vars, `align`, `seed` |
|  [15]   | Block        |    yes     |       no       |   yes   |     no      | `class`/`style` by id, nesting                       |
|  [16]   | Sankey       |     no     |       no       |   no    |     no      | link color strategy via config                       |
|  [17]   | XY chart     |     no     |       no       |   no    |     no      | plot palette via config theme variables              |
|  [18]   | Radar        |     no     |       no       |   no    |     no      | `cScale${i}` and nested `radar` variables            |
|  [19]   | Treemap      |    yes     |      yes       |   no    |     no      | node-local `:::class` and `classDef`                 |
|  [20]   | Packet       |     no     |       no       |   no    |     no      | `blockStrokeColor`, `blockFillColor` theme variables |
|  [21]   | Journey      |     no     |       no       |   no    |     no      | `fillType0`–`fillType7` theme variables              |
|  [22]   | C4           |     no     |       no       |   no    |     no      | `UpdateElementStyle`, `UpdateRelStyle`               |

The silent traps live where syntax parses but styling does not apply: mindmap `:::` classes must be supplied by the host, so in-diagram `classDef` never defines them; block `:::` has no verified route; state styling reaches plain states while a composite class parses and lands in the DOM with its fill and stroke non-portable and `[*]` markers unstyleable; and the packet theme-variable block is inert in the current build.

## [06]-[FLOORS]

Every family ships at or above its floor — the minimum styling below which its render is naked. A family whose engine admits no color route states that bound beside the fence instead of shipping a dead theme block.

| [INDEX] | [FAMILY]      | [FLOOR]                                                                                                           |
| :-----: | :------------ | :---------------------------------------------------------------------------------------------------------------- |
|  [01]   | flowchart     | base vars + three or more canonical classes + explicit rail on every non-primary edge + `fontFamily`              |
|  [02]   | sequence      | actor/signal/activation/note vars + one `box` or `rect` grouping around each `alt`/`par`/`critical` region        |
|  [03]   | state         | general vars + a class on every non-`[*]` resting state — dormant `recessed`, fault `error`, terminal `boundary`  |
|  [04]   | class         | general vars + `classText` + classes separating the aggregate or interface from leaf types                        |
|  [05]   | ER            | attribute banding + `relation*` + classes: aggregate root `primary`, junction `recessed`, external ref `external` |
|  [06]   | gantt         | `section*`/`task*`/`active*`/`crit*`/`todayLineColor`/`gridColor` set                                             |
|  [07]   | mindmap       | no in-fence class route — host-registered classes or engine depth colors, the bound stated beside the fence       |
|  [08]   | timeline      | `cScale0`–`cScale11` + `cScaleLabel0`–`cScaleLabel11`                                                             |
|  [09]   | kanban        | `kanban` config keys + priority metadata; the family carries no theme route                                       |
|  [10]   | gitGraph      | `git0`–`git7` + `gitBranchLabel*` + `commit*` + `tag*`                                                            |
|  [11]   | requirement   | `requirement*`/`relation*` + classes separating requirement from element                                          |
|  [12]   | C4            | `personBorder`/`personBkg` + `UpdateElementStyle`/`UpdateRelStyle` on every element and relation                  |
|  [13]   | architecture  | `archEdgeColor`/`archGroupBorderColor` + per-group icon + `architecture.seed`                                     |
|  [14]   | pie           | `pie1`–`pie12` + `pieSectionTextColor` + `pieLegendTextColor`                                                     |
|  [15]   | quadrant      | `primaryColor`/`primaryTextColor` + per-point `color`/`radius` + quadrant fill vars                               |
|  [16]   | sankey        | `theme: base` frontmatter + `sankey` config `linkColor` + node colors mapped to palette hexes                     |
|  [17]   | xychart       | nested `xyChart` block + `plotColorPalette`                                                                       |
|  [18]   | radar         | nested `radar` block + `cScale0`–`cScale11`                                                                       |
|  [19]   | treemap       | `theme: base` frontmatter + a class per top-level branch                                                          |
|  [20]   | packet        | none — style propagation is broken; the one acknowledged naked family                                             |
|  [21]   | journey       | `fillType0`–`fillType7` — the score fills — + `primaryColor`/`textColor`                                          |
|  [22]   | venn          | `venn1`–`venn8` + `vennTitleTextColor`/`vennSetTextColor`                                                         |
|  [23]   | eventmodeling | `em*` fill/stroke pairs + `emSwimlaneBackground*`                                                                 |
|  [24]   | wardley       | nested `wardley` block + `wardleyEvolutionColor`                                                                  |
|  [25]   | cynefin       | nested `cynefin` domain backgrounds + boundary and arrow colors                                                   |
|  [26]   | treeView      | host-registered icon packs and classes; the dependency stated beside the fence                                    |
|  [27]   | ishikawa      | global vars only; the bound stated beside the fence                                                               |
|  [28]   | swimlane      | the flowchart floor + `theme: base`; the family reuses the flowchart body                                         |
|  [29]   | railroad      | `theme: base` + classes separating production, terminal, and reference nodes                                      |

## [07]-[CONSISTENCY_LAWS]

Review binds these seven laws on every committed fence; the values each law spends — hexes, stamps, rail styles — live with their owner, and a law here never redefines them.

- Edge-rail law: every semantic edge carries an explicit rail from the six-rail set the palette layer owns, bound positionally through `linkStyle` or insertion-stably through an edge-id class; only a plain forward hop inherits the default, every fault edge is Red, and every edge insertion recounts positional indices.
- Container law: a container is never naked — a flowchart `subgraph` rides the recessed cluster surface with a border encoding ownership at the ruled cluster stroke weight, a sequence `alt`/`par`/`break`/`critical` region wraps in a `rect` or `box` background, and a state composite sets the recessed tertiary surface; every fill and stroke traces to the role map.
- classDef-completeness law: a flowchart, state, ER, class, or requirement diagram ships every class its semantics demand — an aggregate root, junction, fault, or dormant state rendering identical to its neighbors is the defect.
- Typography law: the ruled mono stack and the micro-scale `themeCSS` stamps reach every committed fence, and no canvas text renders below the theming floor.
- Backing law: label backings ride Selection, one elevation above the canvas — a backing equal to the canvas leaves labels colliding with the strokes they cross.
- Ordinal-completeness law: a type reading an ordinal palette defines the full engine range the base block carries, so no band derives to `primaryColor` mud.
- Single-home law: the palette layer carries every token role, the micro-scale stamps, and the canonical class and rail sets; this reference carries the mechanical grammar and the per-family floors; an extended fence demonstrates the keys it consumes and never privately defines a role.

## [08]-[SCOPE]

Styling that is part of the diagram grammar travels with the fence text: `classDef`, `class`, `style`, `linkStyle`, edge metadata, C4 update calls, and sequence `box`/`rect`. Frontmatter `themeVariables` and `themeCSS` travel with the fence when the renderer honors Mermaid frontmatter, and only the `base` theme reads `themeVariables`. Host `initialize(...)` styling and page CSS stay with the host and never travel with the diagram.
