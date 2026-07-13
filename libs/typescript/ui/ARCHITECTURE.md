# [TS_UI_ARCHITECTURE]

`ui` maps the browser interface plane and its sibling `viewer` Nx project: `system`, `view`, and `viewer` sub-domains meet through one atom binding, one styled recipe, one motion vocabulary, and one selection plane. Viewer renders decoded wire vocabularies and owns zero geometry or IFC semantics.

Each codemap node names the source file its `.planning/` page becomes in camelCase `.ts`; the scaffold is authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
ui/
├── src/
│   ├── system/                # Component system: token, interaction, state-binding, locale, primitive owners
│   │   ├── token.ts           # Design-token authority computing color and dimension as decode-gated data
│   │   ├── act.ts             # Motion and interaction, discrete accessible events split from continuous gestures
│   │   ├── atom.ts            # One state binding standing the app Layer graph behind the registry
│   │   ├── intl.ts            # Zero-package locale plane riding native Intl behind one cache
│   │   └── primitive.ts       # Headless spine: the one styled recipe and the sanitize gate
│   └── view/                  # View plane composing the system owners into four dense surfaces
│       ├── form.ts            # Schema-driven forms: one kernel Schema owning wire decode and live field validity
│       ├── table.ts           # Data grid: models, virtual windows, and grid semantics under one TableState atom
│       ├── overlay.ts         # Overlay owner: anchoring, sheets, and the command palette over one presence cohort
│       └── chart.ts           # Analytic charts: declarations, streams, and pivots over one Arrow plane
└── viewer/
    └── src/                   # Spatial tier, a second Nx project
        ├── scene.ts           # Content-keyed GLB residency behind the GlbViewport port
        ├── geo.ts             # Geospatial surface: one shared WebGL context as a pure layer value tree
        ├── mark.ts            # GlobalId mark plane: one selection atom every pick pipeline folds into
        ├── panel.ts           # Wire materializer rendering the C#-minted control vocabularies through the owners
        └── probe.ts           # Render evidence: benchmarks paired with wire-decoded receipts, never gating
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
    accTitle: UI package seam registry
    accDescr: UI sub-domain owners exchanging value, wire, port, boundary, and receipt contracts with the core, runtime, Materials, AppHost, AppUi, and Bim packages, edge rails colored by kind and nodes classed by seam direction.
    subgraph ui[UI]
        System[System floor]
        View[View plane]
        Viewer[Viewer tier]
    end
    Core([core])
    Runtime{{runtime}}
    Materials([Rasm.Materials])
    AppHost([Rasm.AppHost])
    AppUi([Rasm.AppUi])
    Bim([Rasm.Bim])
    Core e1@-->|"[SHAPE]: Feed.Document"| View
    System e2@<-->|"[PORT]: Subscribable planes"| Runtime
    Runtime e3@-->|"[PORT]: GlbViewport"| Viewer
    Runtime e4@-->|"[BOUNDARY]: transcoder assets"| Viewer
    Materials e5@-->|"[WIRE]: PbrGroups"| Viewer
    AppHost e6@-->|"[WIRE]: livewire triple"| Viewer
    AppUi e7@-->|"[WIRE]: ControlIntent"| Viewer
    AppUi e8@-->|"[RECEIPT]: RenderReceipt"| Viewer
    Bim e9@-->|"[WIRE]: GlobalIdSet"| Viewer
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeSuccess stroke:#50FA7B,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class System,View,Viewer primary
    class Runtime external
    class Core,Materials,AppHost,AppUi,Bim annotation
    class e1,e2,e3,e4 edgeControl
    class e5,e6,e7,e9 edgeData
    class e8 edgeSuccess
```

## [03]-[ORGANIZATION]

`system` is the capability floor the views instantiate; `view` composes those owners into four dense surfaces — form, grid, overlay, chart — each a single owner where variation is rows (columns, commands, field kinds, chart regimes), never sibling components; `viewer` is the spatial tier as a separate Nx project consuming decoded wire and owning render alone. Selection stays one atom: the grid `RowSelectionState` and the `scrollToIndex` echo project it, never a second plane. Per-owner wiring lives on the owning implementation pages.

## [04]-[BOUNDARIES]

- IFC semantics and geometry stay unowned here; GLB, BCF, and selection vocabularies arrive decoded through the core interchange plane, rendered and never re-authored.
- Browser composition root — `GlbViewport` satisfied from Depot arrivals, host Subscribable planes bound into atoms — is app composition, out of scope here.
- `EXT_meshopt_compression` assets refuse with the `codec-absent` reason until the iac plane admits the wasm decoder identity and its serving row.
- History consumers compose from the landed system pages; a second history owner never appears beside the selection atom.
