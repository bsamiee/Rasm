# [THEMING]

Dracula is the skill's theme system — every committed diagram opens `theme: base` and draws its colors from the Dracula token table below; ad-hoc hexes outside the table are a defect.

## [01]-[PALETTE]

| [INDEX] | [TOKEN]     | [HEX]      | [SEMANTIC_ROLE]                     |
| :-----: | :---------- | :--------- | :---------------------------------- |
|  [01]   | Background  | `#282A36`  | host surface                        |
|  [02]   | Darker      | `#21222C`  | recessed surface                    |
|  [03]   | Selection   | `#44475A`  | node fill                           |
|  [04]   | Comment     | `#6272A4`  | muted line + annotation             |
|  [05]   | Foreground  | `#F8F8F2`  | label text                          |
|  [06]   | Cyan        | `#8BE9FD`  | external system + typed interface   |
|  [07]   | Green       | `#50FA7B`  | success rail + executed capability  |
|  [08]   | Orange      | `#FFB86C`  | data store + durable fact           |
|  [09]   | Pink        | `#FF79C6`  | primary control flow                |
|  [10]   | Purple      | `#BD93F9`  | ownership boundary + focus          |
|  [11]   | Red         | `#FF5555`  | error rail + rejection              |
|  [12]   | Yellow      | `#F1FA8C`  | payload + literal content           |

Current Line and Comment share `#6272A4` by spec; Selection `#44475A` is the fill shade. Hex only — the theming engine rejects named colors.

## [02]-[ROLE_MAP]

| [INDEX] | [DIAGRAM_ROLE]  | [TOKEN_USE]                       |
| :-----: | :-------------- | :-------------------------------- |
|  [01]   | Primary flow    | Pink stroke                       |
|  [02]   | Secondary flow  | Comment stroke                    |
|  [03]   | Boundary        | Purple border on Darker fill      |
|  [04]   | Success         | Green                             |
|  [05]   | Error           | Red                               |
|  [06]   | External system | Cyan                              |
|  [07]   | Data store      | Orange                            |
|  [08]   | Payload         | Yellow accents on Selection fill  |
|  [09]   | Annotation      | Comment text                      |

Same meaning, same token, across every diagram in a corpus. A role outside this table composes from the nearest listed role, never a new hex.

## [03]-[BASE_BLOCK]

The block is the full-surface superset; a diagram carries only the keys its type consumes.

```yaml
---
config:
  theme: base
  themeVariables:
    darkMode: true
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
    edgeLabelBackground: "#282A36"
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
    relationLabelBackground: "#282A36"
    relationLabelColor: "#F8F8F2"
    attributeBackgroundColorOdd: "#282A36"
    attributeBackgroundColorEven: "#21222C"
    pie1: "#FF79C6"
    pie2: "#8BE9FD"
    pie3: "#50FA7B"
    pie4: "#FFB86C"
    pie5: "#BD93F9"
    pie6: "#FF5555"
    pie7: "#F1FA8C"
    pieSectionTextColor: "#282A36"
    pieLegendTextColor: "#F8F8F2"
    cScale0: "#FF79C6"
    cScale1: "#8BE9FD"
    cScale2: "#50FA7B"
    cScale3: "#FFB86C"
    cScale4: "#BD93F9"
    cScale5: "#FF5555"
    cScale6: "#F1FA8C"
    cScaleLabel0: "#282A36"
    cScaleLabel1: "#282A36"
    cScaleLabel2: "#282A36"
    cScaleLabel3: "#282A36"
    cScaleLabel4: "#282A36"
    cScaleLabel5: "#282A36"
    cScaleLabel6: "#282A36"
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
- Bright tokens (Green, Cyan, Yellow, Orange) as fills take `#282A36` text, never `#F8F8F2`.
- Per-type nested objects `xyChart` and `radar` nest inside `themeVariables`, alongside any other type that admits a nested block.
- Partial-consumers: C4 reads only `personBorder`/`personBkg` and routes element colors through `UpdateElementStyle`/`UpdateRelStyle`; packet defines a style block but breaks propagation, so theming drops there; sankey and ishikawa take global vars only.

## [04]-[CLASSDEF_LINKSTYLE]

Each surface owns one color job; ceding it to another is the defect, and every `classDef` sets an explicit `color:` to survive a host swap.

| [INDEX] | [SURFACE]        | [OWNS]                    |
| :-----: | :--------------- | :------------------------ |
|  [01]   | `themeVariables` | diagram-wide defaults     |
|  [02]   | `classDef`       | semantic node classes     |
|  [03]   | `linkStyle`      | per-edge semantic rails   |
|  [04]   | inline `style`   | one-off node exception    |
|  [05]   | `themeCSS`       | renderer escape hatch     |

The canonical Dracula node classes:

```text
classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
classDef success fill:#50FA7B,stroke:#50FA7B,color:#282A36
classDef error fill:#FF5555,stroke:#FF5555,color:#282A36
classDef external fill:#8BE9FD,stroke:#8BE9FD,color:#282A36
classDef data fill:#FFB86C,stroke:#FFB86C,color:#282A36
classDef annotation fill:#44475A,stroke:#6272A4,color:#F8F8F2
```

The edge-rail pattern — default Pink, success Green, error Red, external Cyan:

```text
linkStyle default stroke:#FF79C6,color:#F8F8F2
linkStyle 1 stroke:#50FA7B,color:#F8F8F2
linkStyle 2 stroke:#FF5555,color:#F8F8F2
linkStyle 3 stroke:#8BE9FD,color:#F8F8F2
```

- `classDef` declares at diagram root after the nodes it styles; declared above them it renders unstyled.
- `:::name` or `class a,b name` applies a class to its nodes.
- `linkStyle` indexes edges 0-based in declaration order; `linkStyle default` covers all edges.
- Edge IDs own animate and curve metadata, never stroke or label color — those stay on `linkStyle`.
- Commas inside `stroke-dasharray` escape as `\,`.
- Notes, namespaces, and subgraph titles are not styleable.
- State `classDef` never applies to `[*]` or composite states.

## [05]-[DUAL_HOST]

Node fills and their text travel inside the SVG, so Selection-filled nodes with Foreground text hold on any host. What breaks on a white host is ink drawn over the page — edge strokes, edge labels without a background, transparent-canvas text.

| [TOKEN] | [ON_DARK]    | [ON_WHITE]  |
| :------ | :----------- | :---------- |
| Comment | `3.03` pass  | `4.71` pass |
| Cyan    | `10.29` pass | `1.38` fail |
| Green   | `10.38` pass | `1.37` fail |
| Orange  | `8.36` pass  | `1.70` fail |
| Pink    | `5.97` pass  | `2.39` fail |
| Purple  | `5.90` pass  | `2.41` fail |
| Red     | `4.53` pass  | `3.14` pass |
| Yellow  | `12.74` pass | `1.12` fail |

Red is the only accent passing non-text contrast on both hosts; Comment passes on both; every other accent is dark-host-only ink.

- A diagram whose host is unknown keeps `edgeLabelBackground: "#282A36"` and dark node fills, so labels ride their own surface.
- Pink primary-flow strokes are the standing default; a white-host commitment downgrades line ink to Comment `#6272A4`, the one dual-host line token besides Red.
- A genuinely light-host corpus themes from Alucard, the official light complement, instead of forcing Dracula accents onto white.

Alucard carries the same role map on a light surface: Background `#FFFBEB`, Foreground `#1F1F1F`, Comment `#6C664B`, Selection `#CFCFDE`, Red `#CB3A2A`, Orange `#A34D14`, Yellow `#846E15`, Green `#14710A`, Cyan `#036A96`, Purple `#644AC9`, Pink `#A3144D`.

## [06]-[WHEN_THEMING_DROPS]

Theming is omitted whole, never half-applied. Drop the theme block when the fence targets a host that injects its own mermaid theme, such as a docs site with a site-level `initialize`; when the type ignores `themeVariables`, such as packet or C4 beyond its person tokens; or when a diagram is a throwaway scratch artifact that never ships. A committed diagram in this repo is themed, or it carries the reason in its authoring context, not in the fence.
