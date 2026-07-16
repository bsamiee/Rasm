# [TYPESCRIPT_BRANCH_ARCHITECTURE]

`libs/typescript` in dependency waves — capability domains, acyclic with `core` at the base. Wire decode is the core interchange plane's boundary concern, never the branch center; deployment (`iac`) is the plane-distinct citizen outside the runtime graph; dev infrastructure lives under `tests/` (`tests/contracts/`, `tests/typescript/`), never the branch. Data-spine law is `dataflow-system.md`.

## [01]-[DOMAIN_MAP]

```text codemap
libs/typescript/
├── core/       # The acyclic branch law every folder composes — one authority per cross-language concept
├── security/   # Identity and custody, stateless behind port Tags satisfied downstream
├── data/       # The durable-persistence plane and record of truth; a backend is a guarantee row
├── runtime/    # The execution substrate across both process planes and the browser condition
├── ui/         # The browser product surface; viewer the spatial second Nx project, render-only
└── iac/        # The deploy plane outside the runtime graph; nothing imports it at runtime
```

## [02]-[SEAMS]

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    clusterBkg: "#21222C"
    clusterBorder: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
    titleColor: "#D6BCFA"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: TypeScript branch seam registry
    accDescr: TypeScript branch owners core, runtime, and ui consuming kinded wires produced by the C# packages, edge rails colored by kind and nodes classed by seam direction.
    subgraph ts[LIBS/TYPESCRIPT]
        Core[Core interchange plane]
        Runtime[Runtime execution]
        Ui[UI view plane]
    end
    Rasm{{Rasm}}
    AppHost([Rasm.AppHost])
    Compute{{Rasm.Compute}}
    Persistence[(Rasm.Persistence)]
    Element{{Rasm.Element}}
    Bim([Rasm.Bim])
    Materials([Rasm.Materials])
    AppUi([Rasm.AppUi])
    Rasm e1@<-->|"[CONTENT_KEY]: XxHash128"| Core
    AppHost e2@-->|"[WIRE]: ReceiptEnvelopeWire"| Core
    Compute e3@<-->|"[WIRE]: QuantityFamily"| Core
    Persistence e4@-->|"[WIRE]: CrdtOpWire"| Core
    Element e5@<-->|"[WIRE]: rasm.element.v1"| Core
    Bim e6@-->|"[WIRE]: IfcWire"| Core
    Materials e7@-->|"[WIRE]: MaterialWire"| Core
    AppUi e8@-->|"[WIRE]: CommandPayloadWire"| Core
    AppHost e9@-->|"[TRANSPORT]: OtelExport"| Runtime
    AppHost e10@-->|"[WIRE]: BindingStatusWire"| Ui
    AppUi e11@-->|"[WIRE]: ControlIntentWire"| Ui
    AppUi e12@-->|"[RECEIPT]: RenderReceipt"| Ui
    Bim e13@-->|"[WIRE]: GlobalIdSet"| Ui
    Materials e14@-->|"[WIRE]: PbrGroups"| Ui
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeSuccess stroke:#50FA7B,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    class Core,Runtime,Ui primary
    class Rasm,Compute,Element external
    class Persistence data
    class AppHost,Bim,Materials,AppUi annotation
    class e1,e2,e3,e4,e5,e6,e7,e8,e10,e11,e13,e14 edgeData
    class e12 edgeSuccess
    class e9 edgeExternal
```

Every C#-minted family decodes exactly once through the core interchange codec registry; the `ui` edges name where the decoded landings materialize. TS consumes the GLB tessellation rail through the C#-owned wire; no TS↔Python seam exists. Folder-level seam rows live in each folder's `[02]-[SEAMS]` and mirror the csharp endpoint files verbatim.

## [03]-[DEPENDENCY_DIRECTION]

Dependency flows strictly downward through the waves — W0 `core`, W1 `security`, W2 `data`, W3 `runtime`, W4 `ui`/`iac`. Permitted edges are the whole import law:

| [INDEX] | [FROM]     | [MAY_IMPORT]               | [NOTES]                                                                               |
| :-----: | :--------- | :------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `core`     | (nothing)                  | Runs identically under node, bun, and the browser; imported by every runtime folder   |
|  [02]   | `security` | `core`                     | State lives behind port Tags; the folder never imports `data`                         |
|  [03]   | `data`     | `core`, `security`         | The one `data → security` edge: `journal/retain` Shredder + `lane/tenant` TenantScope |
|  [04]   | `runtime`  | `core`, `security`, `data` | Both process planes; the browser condition is the same package, never a sibling       |
|  [05]   | `ui`       | `core`, `runtime`          | `viewer` is a second Nx project inside the folder with the same edge set              |
|  [06]   | `iac`      | `core`, `data`             | Type/value reads only (`DashboardModel`, `Alert`, `Slo.Objective`, `Pg`)              |

Port satisfaction happens at app composition, never as an upward import: every port Tag a folder declares binds to a downstream folder's Layer at the composition root, so a stateless upper folder never reaches up for its dependency. One value crosses back: typed `StackOutputs.sharding` read by `runtime` `ShardingConfig.layerFromEnv` — an env fact, never an import.
