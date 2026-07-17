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

## [02]-[STRATA]

- W0 `core` — imports nothing and runs identically under node, bun, and the browser; `value` roots the interior — `state` is pure algebra over it, `interchange` the one keyed-decode registry every C#-minted wire family lands on, `observe` composing the same floor — and every runtime folder composes it.
- W1 `security` — composes core alone (`TenantContext`); every stateful obligation is a port Tag a downstream folder satisfies, and the folder never imports `data`.
- W2 `data` — composes core (`ContentKey`) plus security's one edge (`journal/retain` Shredder, `lane/tenant` TenantScope); a backend is a semantic-guarantee row.
- W3 `runtime` — composes core, security, and data (`Budget`, `CookieSpec`, `Embedder`) across both process planes; the browser condition is the same package, never a sibling.
- W4 `ui` + `iac` — `ui` imports core alone (`Feed.Document`) and reaches runtime only through its declared `GlbViewport` port and atom-bridge bindings the app root satisfies, `viewer` a second Nx project under the same law; `iac` composes core and data as type/value reads (`DashboardModel`, `Pg`), plane-distinct outside the runtime graph.

Port satisfaction happens at app composition, never as an import: every port Tag a folder declares binds to another folder's Layer at the composition root — `security` ports fill from `data`, `ui`'s `GlbViewport` fills from runtime `Depot` arrivals — so no folder reaches across for its dependency. One value crosses back: typed `StackOutputs.sharding` read by `runtime` `ShardingConfig.layerFromEnv` — an env fact, never an import.

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
flowchart TB
    accTitle: TypeScript branch import strata
    accDescr: Five import waves — ui and iac over runtime over data over security onto the core foundation — every wave importing downward only, each labeled import naming one sourced surface, iac composing data and core directly, ui reaching runtime through its dashed GlbViewport port binding rather than an import, one forbidden upward edge styled red, and core's interior members state, interchange, and observe composing the value floor with interchange composing all three.
    subgraph W4["W4 APP + DEPLOY"]
        Ui[ui]
        Iac[iac]
    end
    subgraph W3["W3 RUNTIME"]
        Runtime[runtime]
    end
    subgraph W2["W2 DATA"]
        Data[data]
    end
    subgraph W1["W1 SECURITY"]
        Security[security]
    end
    subgraph W0["W0 CORE"]
        Value[value]
        State[state]
        Interchange[interchange]
        Observe[observe]
    end
    Ui e1@-.->|"[PORT]: GlbViewport"| Runtime
    Iac e2@-->|"[IMPORT]: Pg.rows"| Data
    Runtime e3@-->|"[IMPORT]: CookieSpec"| Security
    Runtime e4@-->|"[IMPORT]: Embedder"| Data
    Data e5@-->|"[IMPORT]: TenantScope"| Security
    Ui e6@-->|"[IMPORT]: Feed.Document"| W0
    Iac e7@-->|"[IMPORT]: DashboardModel"| W0
    Runtime e8@-->|"[IMPORT]: Budget"| W0
    Data e9@-->|"[IMPORT]: ContentKey"| W0
    Security e10@-->|"[IMPORT]: TenantContext"| W0
    Ui ~~~ Runtime
    W0 f1@-->|"forbidden: upward import"| W4
    State i1@--> Value
    Interchange i2@--> Value
    Observe i3@--> Value
    Interchange i4@--> State
    Interchange i5@--> Observe
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    classDef edgeError stroke:#FF5555,stroke-width:3px,color:#F8F8F2
    class Ui,Iac,Runtime,Data,Security primary
    class Value,State,Interchange,Observe recessed
    class e1,e2,e3,e4,e5,e6,e7,e8,e9,e10,i1,i2,i3,i4,i5 edgeControl
    class f1 edgeError
```

## [03]-[SEAMS]

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
    accTitle: TypeScript branch C# seam registry
    accDescr: TypeScript owners core, runtime, and ui exchanging kinded wires with the C# packages — data-bearing kinds on the orange rail, telemetry transport on the cyan rail — persistence as the one data store, bidirectional peers as hexagons, one-way sources as stadiums.
    subgraph ts[LIBS/TYPESCRIPT]
        Core[core]
        Runtime[runtime]
        Ui[ui]
    end
    Rasm{{Rasm}}
    Element{{Rasm.Element}}
    Compute{{Rasm.Compute}}
    Persistence[(Rasm.Persistence)]
    Bim([Rasm.Bim])
    Materials([Rasm.Materials])
    AppUi([Rasm.AppUi])
    AppHost([Rasm.AppHost])
    Rasm e1@<-->|"[CONTENT_KEY]: XxHash128"| Core
    Element e2@<-->|"[WIRE]: rasm.element.v1"| Core
    Compute e3@<-->|"[WIRE]: QuantityFamily"| Core
    Persistence e4@-->|"[WIRE]: CrdtOpWire"| Core
    Bim e6@-->|"[WIRE]: IfcWire"| Core
    Materials e7@-->|"[WIRE]: MaterialWire"| Core
    AppUi e8@-->|"[WIRE]: CommandPayloadWire"| Core
    AppHost e5@-->|"[WIRE]: ReceiptEnvelopeWire"| Core
    Bim e12@-->|"[WIRE]: GlobalIdSet"| Ui
    Materials e13@-->|"[WIRE]: PbrGroups"| Ui
    AppUi e11@-->|"[WIRE]: ControlIntent"| Ui
    AppHost e10@-->|"[WIRE]: BindingStatus"| Ui
    AppHost e9@-->|"[TRANSPORT]: OtelExport"| Runtime
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    class Core,Runtime,Ui primary
    class Rasm,Element,Compute external
    class Persistence data
    class AppHost,Bim,Materials,AppUi recessed
    class e1,e2,e3,e4,e5,e6,e7,e8,e10,e11,e12,e13 edgeData
    class e9 edgeExternal
```

Every C#-minted family decodes exactly once through the core interchange codec registry; the `ui` edges name where the decoded landings materialize. Each edge freezes the single load-bearing contract at its partner grain, spelled verbatim from the C# endpoint file — companion wires (`CapabilityDescriptor`, `DegradationLevel`, `BindingStatusWire`, `SnapshotHeader`, `GeometryResidencyWire`, `EvidenceFeed`, `RenderReceipt`, `BcfTopicWire`, `BcfViewpointWire`, `DiffWire`, `GeoFeatureWkb`) fold to the folder pages. TS consumes the GLB tessellation rail through the C#-owned wire; no TS↔Python seam exists. Folder-level seam rows live in each folder's `[03]-[SEAMS]` and mirror the csharp endpoint files verbatim.

## [04]-[ADMISSION_POLICY]

One workspace manifest (`pnpm-workspace.yaml`) owns package admission and version bounds; `viewer` is the second Nx project inside `ui` carrying the same edge set, and dev infrastructure stays under `tests/`, never the branch. Installation rationale stays in the manifest; folder pages name capability, entrypoints, boundaries, and exclusions.
