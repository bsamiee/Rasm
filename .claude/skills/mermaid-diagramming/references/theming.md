# [THEMING]

Dracula is the skill's theme system — every committed diagram opens `theme: base` and draws its colors from the Dracula token table below; ad-hoc hexes outside the table are a defect. Sections: [01] palette, [02] role map, [03] base block, [04] classDef and linkStyle, [05] micro scale, [06] dual host, [07] when theming drops.

## [01]-[PALETTE]

| [INDEX] | [TOKEN]    | [HEX]     | [SEMANTIC_ROLE]                    |
| :-----: | :--------- | :-------- | :--------------------------------- |
|  [01]   | Background | `#282A36` | host surface                       |
|  [02]   | Darker     | `#21222C` | recessed surface + dormant fill    |
|  [03]   | Selection  | `#44475A` | node fill                          |
|  [04]   | Comment    | `#6272A4` | muted line + annotation            |
|  [05]   | Foreground | `#F8F8F2` | label text                         |
|  [06]   | Cyan       | `#8BE9FD` | external system + typed interface  |
|  [07]   | Green      | `#50FA7B` | success rail + executed capability |
|  [08]   | Orange     | `#FFB86C` | data store + durable fact          |
|  [09]   | Pink       | `#FF79C6` | primary control flow               |
|  [10]   | Purple     | `#BD93F9` | ownership boundary + focus         |
|  [11]   | Red        | `#FF5555` | error rail + rejection             |
|  [12]   | Yellow     | `#F1FA8C` | payload + literal content          |

Current Line and Comment share `#6272A4`; Selection `#44475A` is the fill shade. `themeVariables` take hex only — the theming engine rejects named colors there, and this corpus carries palette hexes on every styling surface.

## [02]-[ROLE_MAP]

Every token carries its role on two surfaces at once: the `themeVariables` that spend it diagram-wide and the `classDef` or `linkStyle` rail that spends it per node and per edge. A token holding a role with no rail is unspendable, so every row closes both columns.

| [INDEX] | [TOKEN]    | [ROLE]                        | [THEME_CARRIERS]                                                                  | [CLASS_OR_RAIL]                              |
| :-----: | :--------- | :---------------------------- | :-------------------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | Background | canvas + bright-fill text     | `background`, bright-ordinal `*Label*`                                             | `color:#282A36` on every bright class        |
|  [02]   | Darker     | recessed / dormant / done     | `clusterBkg`, `tertiaryColor`, `doneTaskBkgColor`, `sectionBkgColor`               | `recessed` class                             |
|  [03]   | Selection  | neutral fill + label backing  | `mainBkg`, `primaryColor`, `actorBkg`, `taskBkgColor`, `noteBkgColor`, `edgeLabelBackground`, `relationLabelBackground` | `primary` fill |
|  [04]   | Comment    | secondary rail + muted stroke | `secondaryColor`, `clusterBorder`, `gridColor`, `activationBkgColor`, `git7`       | `annotation` stroke; dashed trace rail       |
|  [05]   | Foreground | label text                    | every `*TextColor`, `textColor`, `titleColor`                                      | `color:#F8F8F2` on dark classes              |
|  [06]   | Cyan       | external system / typed iface | `git2`, `pie2`, `cScale1`, `plotColorPalette`                                      | `external` class; external rail              |
|  [07]   | Green      | success / executed            | `git3`, `pie3`, `cScale2`                                                          | `success` class; success rail                |
|  [08]   | Orange     | data store / durable fact     | `git4`, `pie4`, `cScale3`                                                          | `data` class; data rail                      |
|  [09]   | Pink       | primary control flow          | `lineColor`, `arrowheadColor`, `signalColor`, `relationColor`, `todayLineColor`    | `primary` stroke; default rail               |
|  [10]   | Purple     | ownership boundary / focus    | `nodeBorder`, `primaryBorderColor`, `actorBorder`, `taskBorderColor`, `git0`       | `boundary` stroke                            |
|  [11]   | Red        | error / rejection / forbidden | `critBkgColor`, `critBorderColor`, `git5`, `pie6`, `cScale5`                       | `error` class; error rail on every fault edge |
|  [12]   | Yellow     | payload / literal / tag       | `tagLabelBackground`, `git6`, `pie7`, `cScale6`                                    | `payload` class; payload rail                |

Same meaning, same token, across every diagram in a corpus. A role outside this table composes from the nearest listed role, never a new hex.

## [03]-[BASE_BLOCK]

The block carries the high-traffic themed families; a family absent here takes its keys from its type's section or its local style law, and a diagram carries only the keys its type consumes.

```yaml
---
config:
  theme: base
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    secondaryColor: "#6272A4"
    secondaryTextColor: "#F8F8F2"
    secondaryBorderColor: "#6272A4"
    tertiaryColor: "#21222C"
    tertiaryTextColor: "#F8F8F2"
    tertiaryBorderColor: "#44475A"
    mainBkg: "#44475A"
    nodeBorder: "#BD93F9"
    nodeTextColor: "#F8F8F2"
    lineColor: "#FF79C6"
    arrowheadColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#6272A4"
    edgeLabelBackground: "#44475A"
    labelBackgroundColor: "#44475A"
    titleColor: "#F8F8F2"
    noteBkgColor: "#44475A"
    noteTextColor: "#F8F8F2"
    noteBorderColor: "#6272A4"
    actorBkg: "#44475A"
    actorBorder: "#BD93F9"
    actorTextColor: "#F8F8F2"
    signalColor: "#FF79C6"
    signalTextColor: "#F8F8F2"
    activationBkgColor: "#6272A4"
    activationBorderColor: "#BD93F9"
    loopTextColor: "#F8F8F2"
    classText: "#F8F8F2"
    requirementBackground: "#44475A"
    requirementBorderColor: "#BD93F9"
    requirementTextColor: "#F8F8F2"
    relationColor: "#FF79C6"
    relationLabelBackground: "#44475A"
    relationLabelColor: "#F8F8F2"
    attributeBackgroundColorOdd: "#282A36"
    attributeBackgroundColorEven: "#21222C"
    sectionBkgColor: "#21222C"
    altSectionBkgColor: "#282A36"
    sectionBkgColor2: "#21222C"
    gridColor: "#6272A4"
    taskBkgColor: "#44475A"
    taskBorderColor: "#BD93F9"
    taskTextColor: "#F8F8F2"
    taskTextOutsideColor: "#F8F8F2"
    activeTaskBkgColor: "#6272A4"
    activeTaskBorderColor: "#BD93F9"
    doneTaskBkgColor: "#21222C"
    doneTaskBorderColor: "#6272A4"
    critBkgColor: "#FF5555"
    critBorderColor: "#FF5555"
    todayLineColor: "#FF79C6"
    fillType0: "#FF79C6"
    fillType1: "#8BE9FD"
    fillType2: "#50FA7B"
    fillType3: "#FFB86C"
    fillType4: "#BD93F9"
    fillType5: "#FF5555"
    fillType6: "#F1FA8C"
    fillType7: "#6272A4"
    personBorder: "#BD93F9"
    personBkg: "#44475A"
    archEdgeColor: "#FF79C6"
    archEdgeArrowColor: "#FF79C6"
    archGroupBorderColor: "#6272A4"
    git0: "#BD93F9"
    git1: "#FF79C6"
    git2: "#8BE9FD"
    git3: "#50FA7B"
    git4: "#FFB86C"
    git5: "#FF5555"
    git6: "#F1FA8C"
    git7: "#6272A4"
    gitBranchLabel0: "#282A36"
    gitBranchLabel1: "#282A36"
    gitBranchLabel2: "#282A36"
    gitBranchLabel3: "#282A36"
    gitBranchLabel4: "#282A36"
    gitBranchLabel5: "#282A36"
    gitBranchLabel6: "#282A36"
    gitBranchLabel7: "#282A36"
    commitLabelColor: "#F8F8F2"
    commitLabelBackground: "#44475A"
    tagLabelColor: "#282A36"
    tagLabelBackground: "#F1FA8C"
    pie1: "#FF79C6"
    pie2: "#8BE9FD"
    pie3: "#50FA7B"
    pie4: "#FFB86C"
    pie5: "#BD93F9"
    pie6: "#FF5555"
    pie7: "#F1FA8C"
    pie8: "#8BE9FD"
    pie9: "#50FA7B"
    pie10: "#FFB86C"
    pie11: "#BD93F9"
    pie12: "#FF5555"
    pieSectionTextColor: "#282A36"
    pieLegendTextColor: "#F8F8F2"
    cScale0: "#FF79C6"
    cScale1: "#8BE9FD"
    cScale2: "#50FA7B"
    cScale3: "#FFB86C"
    cScale4: "#BD93F9"
    cScale5: "#FF5555"
    cScale6: "#F1FA8C"
    cScale7: "#8BE9FD"
    cScale8: "#50FA7B"
    cScale9: "#FFB86C"
    cScale10: "#BD93F9"
    cScale11: "#FF5555"
    cScaleLabel0: "#282A36"
    cScaleLabel1: "#282A36"
    cScaleLabel2: "#282A36"
    cScaleLabel3: "#282A36"
    cScaleLabel4: "#282A36"
    cScaleLabel5: "#282A36"
    cScaleLabel6: "#282A36"
    cScaleLabel7: "#282A36"
    cScaleLabel8: "#282A36"
    cScaleLabel9: "#282A36"
    cScaleLabel10: "#282A36"
    cScaleLabel11: "#282A36"
    xyChart:
      backgroundColor: "#282A36"
      titleColor: "#F8F8F2"
      dataLabelColor: "#F8F8F2"
      legendTextColor: "#F8F8F2"
      xAxisLabelColor: "#F8F8F2"
      xAxisTitleColor: "#F8F8F2"
      xAxisTickColor: "#6272A4"
      xAxisLineColor: "#6272A4"
      yAxisLabelColor: "#F8F8F2"
      yAxisTitleColor: "#F8F8F2"
      yAxisTickColor: "#6272A4"
      yAxisLineColor: "#6272A4"
      plotColorPalette: "#FF79C6,#8BE9FD,#50FA7B,#FFB86C,#BD93F9,#FF5555,#F1FA8C"
    radar:
      axisColor: "#6272A4"
      graticuleColor: "#44475A"
      curveOpacity: 0.55
      curveStrokeWidth: 2
---
```

- `theme: base` is the only theme accepting `themeVariables`; every unset variable derives from `primaryColor`, `background`, or `darkMode`, and a direct override always wins over a derived default.
- `darkMode: true` flips the derived-color math toward the dark host.
- `fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"` is the ruled mono stack — every node, edge, cluster, actor, note, and section label is mono, resolving through Menlo on Apple and Cascadia Mono or Consolas on Windows; a bare `fontFamily: "monospace"` is a defect, and a hyphenated family token — `ui-monospace`, `SFMono-Regular` — makes the engine emit an empty declaration, silently falling back to the sans default.
- `fontSize` stays inert for gantt, ER, and flowchart — per-class sizing rides `themeCSS` per [05], never that variable.
- `edgeLabelBackground`, `relationLabelBackground`, and the state family's `labelBackgroundColor` carry Selection `#44475A`, one elevation above the canvas, so a label reads as a chip lifted off both the `#282A36` surface and any crossing stroke; a backing equal to `background` renders invisible, a backing left unset derives to near-black, and either collides labels with Pink strokes.
- Bright tokens (Green, Cyan, Yellow, Orange) as fills take `#282A36` text, never `#F8F8F2`.
- Ordinal families run their full engine range here — `pie1`–`pie12`, `cScale0`–`cScale11` with `cScaleLabel0`–`cScaleLabel11`, `fillType0`–`fillType7`, `git0`–`git7` — so no band derives to `primaryColor` mud.
- Per-type nested objects `xyChart` and `radar` nest inside `themeVariables`, alongside any other type that admits a nested block.
- Partial-consumers: C4 reads `personBorder`/`personBkg` from this block and routes element and relation colors through `UpdateElementStyle`/`UpdateRelStyle`; packet defines a style block but breaks propagation, so theming drops there; sankey and ishikawa take global vars only.
- This block is the single home for every corpus-wide token: an extended fence demonstrates the keys it consumes and never privately defines a role — `architecture`, `journey`, and C4 tokens live here, not in their fences.

## [04]-[CLASSDEF_LINKSTYLE]

Each surface owns one color job; ceding it to another is the defect, and every `classDef` sets an explicit `color:` to survive a host swap. Class names are free-form — an archetype names its own semantic classes — while every hex a class carries traces to the palette table.

| [INDEX] | [SURFACE]        | [OWNS]                                     |
| :-----: | :--------------- | :----------------------------------------- |
|  [01]   | `themeVariables` | diagram-wide defaults                      |
|  [02]   | `classDef`       | semantic node classes, id-bound edge rails |
|  [03]   | `linkStyle`      | positional per-edge semantic rails         |
|  [04]   | inline `style`   | one-off node exception                     |
|  [05]   | `themeCSS`       | renderer escape hatch                      |

The canonical Dracula node classes — nine, one per role the role map binds:

```text
classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
classDef success fill:#50FA7B,stroke:#50FA7B,color:#282A36
classDef error fill:#FF5555,stroke:#FF5555,color:#282A36
classDef external fill:#8BE9FD,stroke:#8BE9FD,color:#282A36
classDef data fill:#FFB86C,stroke:#FFB86C,color:#282A36
classDef payload fill:#F1FA8C,stroke:#F1FA8C,color:#282A36
classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
```

`recessed` fills a dormant, done, or terminal-adjacent node; `annotation` shares its surface with a Comment stroke and carries side commentary, never flow. On an ER entity a bright class fill floods the attribute rows and collides with the banding, so ER role classes stroke-encode instead — an external registry rides `fill:#21222C,stroke:#8BE9FD,color:#8BE9FD`, the role carried by stroke and title ink on a recessed surface. The six edge rails — every semantic edge takes its rail explicitly, and only a plain forward hop rides the default:

```text
linkStyle default stroke:#FF79C6,color:#F8F8F2
linkStyle 1 stroke:#50FA7B,color:#F8F8F2
linkStyle 2 stroke:#FF5555,stroke-width:2px,color:#F8F8F2
linkStyle 3 stroke:#8BE9FD,color:#F8F8F2
linkStyle 4 stroke:#FFB86C,color:#F8F8F2
linkStyle 5 stroke:#6272A4,color:#F8F8F2,stroke-dasharray:4 3
```

Rail semantics: Pink primary, Green success, Red error — mandatory on every fault edge — Cyan external, Orange data-carrying, Comment-dashed trace and secondary. Every rail declares `color:#F8F8F2` so its label never falls to a derived color, and the fault rail alone carries `stroke-width:2px` so weight discriminates a fault hop from a primary hop; the engine derives each styled edge's arrowhead marker from its resolved stroke, so a Red rail terminates in a Red head with no extra key, and `arrowheadColor` governs only unstyled edges. A rail binds two ways with identical semantics: positionally through `linkStyle N` — indices are 0-based parse positions, so every edge insertion or deletion recounts every positional index before the diagram ships — or insertion-stably through an edge id, `A e1@--> B` then `class e1 edgeError`, where the id form survives the insertions that renumber every positional index; a fence past its edge budget or under ongoing edits prefers the id form. The canonical edge classes mirror the five non-default rails:

```text
classDef edgeSuccess stroke:#50FA7B,color:#F8F8F2
classDef edgeError stroke:#FF5555,stroke-width:2px,color:#F8F8F2
classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
classDef edgeData stroke:#FFB86C,color:#F8F8F2
classDef edgeTrace stroke:#6272A4,color:#F8F8F2,stroke-dasharray:4 3
```

## [05]-[MICRO_SCALE]

Per-element sizing rides `themeCSS`, never `themeVariables.fontSize`. Every value is an exact stamp over the SVG-px scale; `[FLOOR]` is the absolute minimum, and nothing on a mermaid canvas renders below 12px — SVG text carries no hinting, so the floor sits above a hinted HTML equivalent.

| [INDEX] | [CLASS]                    | [SELECTOR]               | [PX] | [WEIGHT]               | [FLOOR] |
| :-----: | :------------------------- | :----------------------- | :--: | :--------------------- | :-----: |
|  [01]   | node label                 | `.nodeLabel`             |  14  | 500                    |   13    |
|  [02]   | actor label (sequence)     | `text.actor tspan`       |  14  | 600                    |   13    |
|  [03]   | ER entity name             | `.name .nodeLabel`       |  14  | 600                    |   13    |
|  [04]   | cluster / composite title  | `.cluster-label .nodeLabel` | 13 | 600, uppercase in text |   12    |
|  [05]   | edge label                 | `.edgeLabel`             | 12.5 | 500                    |   12    |
|  [06]   | message text (sequence)    | `.messageText`           | 12.5 | 500                    |   12    |
|  [07]   | note text                  | `.noteText`              | 12.5 | 400                    |   12    |
|  [08]   | ER attribute cell          | `.nodeLabel` under ER    |  12  | 400                    |   12    |
|  [09]   | loop / group label         | `.loopText`, `.labelText` | 12  | 500                    |   11    |
|  [10]   | section title (gantt)      | `.sectionTitle`          |  12  | 600                    |   11    |

The canonical `themeCSS` strings — one per family, copied verbatim into the fence frontmatter:

```text
flowchart:  ".nodeLabel{font-size:14px;font-weight:500}.edgeLabel{font-size:12.5px;font-weight:500}.edgePaths path{stroke-width:1.5px}"
+ clusters: ".cluster-label .nodeLabel{font-size:13px;font-weight:600}.cluster rect{stroke-width:1.5px}"
state:      ".nodeLabel{font-size:14px;font-weight:500}.edgeLabel{font-size:12.5px;font-weight:500}.cluster-label .nodeLabel{font-size:13px;font-weight:600}.transition{stroke-width:1.5px}"
sequence:   "text.actor tspan{font-size:14px;font-weight:600}.messageText{font-size:12.5px;font-weight:500}.noteText{font-size:12.5px}.loopText,.labelText{font-size:12px;font-weight:500}"
er:         ".nodeLabel{font-size:12px}.name .nodeLabel{font-size:14px;font-weight:600}.edgeLabel .nodeLabel{font-size:12.5px;font-weight:500}.relationshipLine{stroke-width:1.5px}"
```

- `themeCSS` admits no `>` combinator: the sanitizer drops the entire injected block on the first `>`, silently reverting every rule in it — use the descendant space. A `classDef` emits `!important`, so a `themeCSS` rule targeting the same property on a classed node loses; the strings above target label typography and bare strokes, which no class carries.
- The engine measures label boxes before `themeCSS` applies, so a `text-transform` clips its target — the uppercase cluster title rides the title text itself (`subgraph core[CORE PACKAGE]`), and any size stamped above the measured 16px default clips the same way; the stamps above only step down.
- Stroke-weight law: `1.5px` is the standing edge weight, `2px` the fault and emphasis weight, and the two never coincide — weight alone discriminates a primary hop from a fault hop. Cluster borders and grid strokes step to `1.5px` so a Comment boundary reads as a containment edge, not a smudge; a hairline never renders heavier than the structural line beside it.
- Dashed-rhythm law: `4 3` reads as trace and annotation, `6 3` as planned and deferred, solid as realized — one rhythm system across every diagram, matching the dotted-token and Comment-rail semantics.
- Canvas law: flowchart fences carry `flowchart: { padding: 16 }` so no node or label touches the SVG edge; `subGraphTitleMargin` stays out of ELK diagrams — it displaces edge labels there.
- Marker proportion: arrowhead length runs ≈6× stroke width and rides the renderer; it needs no stamp, only the weight law above so heads stay proportionate.

## [06]-[DUAL_HOST]

Node fills and their text travel inside the SVG, so Selection-filled nodes with Foreground text hold on any host. What breaks on a white host is ink drawn over the page — edge strokes, edge labels without a background, transparent-canvas text.

| [INDEX] | [TOKEN] | [ON_DARK]    | [ON_WHITE]  |
| :-----: | :------ | :----------- | :---------- |
|  [01]   | Comment | `3.03` large | `4.71` pass |
|  [02]   | Cyan    | `10.29` pass | `1.38` fail |
|  [03]   | Green   | `10.38` pass | `1.37` fail |
|  [04]   | Orange  | `8.36` pass  | `1.70` fail |
|  [05]   | Pink    | `5.97` pass  | `2.39` fail |
|  [06]   | Purple  | `5.90` pass  | `2.41` fail |
|  [07]   | Red     | `4.53` pass  | `3.14` pass |
|  [08]   | Yellow  | `12.74` pass | `1.12` fail |

Red is the only accent passing non-text contrast on both hosts; Comment passes normal text on white and only large-text and non-text duty on dark; every other accent is dark-host-only ink.

- A diagram whose host is unknown keeps `edgeLabelBackground: "#44475A"` and dark node fills, so labels ride their own lifted surface on any page.
- On a dark host page the `#282A36` canvas reads as a raised panel — inline the SVG on the page body; a container whose fill sits within one elevation step of the canvas erases the diagram edge, so the container steps down a level or frames the SVG with a visible border.
- Pink primary-flow strokes are the standing default; a white-host commitment downgrades line ink to Comment `#6272A4`, the one dual-host line token besides Red.
- A genuinely light-host corpus themes from Alucard, the official light complement, instead of forcing Dracula accents onto white.

Alucard carries the same role map on a light surface: Background `#FFFBEB`, Foreground `#1F1F1F`, Comment `#6C664B`, Selection `#CFCFDE`, Red `#CB3A2A`, Orange `#A34D14`, Yellow `#846E15`, Green `#14710A`, Cyan `#036A96`, Purple `#644AC9`, Pink `#A3144D`.

## [07]-[WHEN_THEMING_DROPS]

Theming is omitted whole, never half-applied. Drop the theme block when the fence targets a host that injects its own mermaid theme, such as a docs site with a site-level `initialize`; when the type ignores `themeVariables`, such as packet; or when a diagram is a throwaway scratch artifact that never ships. A committed diagram in this repo is themed, or it carries the reason in its authoring context, not in the fence.
