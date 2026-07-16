# [PY_RUNTIME_ARCHITECTURE]

`runtime` maps the host-free execution foundation every `libs/python` sibling composes: one polymorphic owner per sub-domain closes its concern, each folder mapping to exactly one module namespace. Content identity reproduces the C# `XxHash128` seed bit-identically and never re-mints, so a value carries one key across the runtime boundary; companion decode admits only C#-minted wire shapes and owns no wire vocabulary. It references no sibling — alignment travels through seam contracts and the content-keyed wire.

## [01]-[DOMAIN_MAP]

```text codemap
runtime/
├── observability/      # Local evidence production: receipts, signals, and the one OTLP install gate
│   ├── receipts.py     # Receipt union, drain taxonomy, and contributor-fold port
│   ├── metrics.py      # One MeterProvider's instruments and the record mapping
│   └── telemetry.py    # Profile-gated OTLP install owner
├── reliability/        # One fault family and resilience policy every sibling returns through
│   ├── faults.py       # Boundary-fault union and its exception-to-fault projector
│   └── resilience.py   # Retry policy table, one row per retryable class
├── transport/          # Resource roots, the companion server, the wire vocabulary, and the wire codec
│   ├── roots.py        # Resource roots and refs over fsspec and the remote transports
│   ├── serve.py        # gRPC server lifecycle, route roster, and credential admit
│   ├── shapes.py       # Proto vocabulary and its descriptor drift gate
│   └── wire.py         # Protobuf transcode, frame legs, and the CRDT-op codec
├── execution/          # Caller-owned host-fact admission, bounded concurrency, and recipe execution
│   ├── admission.py    # Runtime context, causal frames, and settings admission
│   ├── lanes.py        # Lane-policy task groups and the stage-plan DAG
│   └── recipe.py       # Content-keyed recipe execution on the thread lane
├── evidence/           # Content-addressing, the seed-parity corpus, and structural-surface evidence
│   ├── identity.py     # Content identity and key reproducing the C# seed bit-identically
│   ├── reproduction.py # Seed-reproduction corpus and its parity fold
│   └── evidence.py     # Evidence union, catalogue member facts, and grammar registry
└── clock/              # Logical time: the host-minted HLC stamp and content-stable element id
    └── clock.py        # HLC stamp, element id, tenant, and causal frame
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
    accTitle: Runtime C# platform and kernel seams
    accDescr: Runtime sub-domain owners exchanging content keys, wire codecs, gRPC transport, and clock stamps with the kernel, element, compute, persistence, and app-host packages, edge rails colored by kind and nodes classed by seam direction.
    subgraph runtime[RUNTIME]
        Evidence[Evidence]
        Transport[Transport]
        Observability[Observability]
        Clock[Clock]
    end
    Rasm{{Rasm}}
    Element{{Rasm.Element}}
    Compute{{Rasm.Compute}}
    Persistence[(Rasm.Persistence)]
    AppHost{{Rasm.AppHost}}
    Evidence e1@<-->|"[CONTENT_KEY]: XxHash128"| Rasm
    Evidence e2@<-->|"[CONTENT_KEY]: ContentAddress"| Element
    Compute e3@-->|"[WIRE]: XxHash128"| Evidence
    Transport e4@<-->|"[WIRE]: PROTO_VOCABULARY"| Compute
    Transport e5@<-->|"[WIRE]: CrdtOp"| Persistence
    Transport e6@<-->|"[TRANSPORT]: ServerHost"| AppHost
    Observability e7@<-->|"[TRANSPORT]: OTLP egress"| AppHost
    AppHost e8@-->|"[PORT]: Hlc stamp"| Clock
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Evidence,Transport,Observability,Clock primary
    class Rasm,Element,Compute,AppHost external
    class Persistence data
    class e1,e2,e3,e4,e5 edgeData
    class e6,e7 edgeExternal
    class e8 edgeControl
```

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
    accTitle: Runtime cross-package Python seams
    accDescr: Runtime sub-domain owners exchanging content identity, tessellation wire, resource transport, recipe ports, and receipts with the geometry, data, artifacts, and compute Python packages, edge rails colored by kind and nodes classed by seam direction.
    subgraph runtime[RUNTIME]
        Evidence[Evidence]
        Transport[Transport]
        Execution[Execution]
        Observability[Observability]
    end
    Geometry{{python:geometry}}
    Data{{python:data}}
    Artifacts{{python:artifacts}}
    Compute([python:compute])
    Geometry e1@-->|"[CONTENT_KEY]: ContentIdentity"| Evidence
    Evidence e2@<-->|"[CONTENT_KEY]: ContentIdentity"| Data
    Evidence e3@-->|"[CONTENT_KEY]: ContentIdentity"| Artifacts
    Evidence e4@-->|"[CONTENT_KEY]: ParityReceipt"| Compute
    Transport e5@<-->|"[WIRE]: Tessellation"| Geometry
    Transport e6@-->|"[TRANSPORT]: ResourceRef"| Data
    Transport e7@-->|"[BOUNDARY]: ResourceRef"| Compute
    Execution e8@-->|"[CONTENT_KEY]: ContentKey"| Artifacts
    Geometry e9@-->|"[PORT]: RecipeSpec"| Execution
    Observability e10@<-->|"[RECEIPT]: ArtifactReceipt"| Artifacts
    Data e11@-->|"[RECEIPT]: QueryReceipt"| Observability
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    classDef edgeSuccess stroke:#50FA7B,color:#F8F8F2
    class Evidence,Transport,Execution,Observability primary
    class Geometry,Data,Artifacts external
    class Compute annotation
    class e1,e2,e3,e4,e5,e8 edgeData
    class e6 edgeExternal
    class e7,e9 edgeControl
    class e10,e11 edgeSuccess
```

## [03]-[BOUNDARIES]

Each sub-domain charter is the codemap comment; the boundary law below fixes the one ownership each holds, so a planned-but-empty sub-domain and a misplaced concern both read as gaps. Exact refusals and their enforcing mechanisms live on the owning implementation pages.

- `observability` — produces local evidence only, never an AppHost envelope or health status.
- One shared OTLP exporter and one `MeterProvider` install behind the profile gate; every receipt folds through one attribute-keyed drain.
- Every span rides the inbound C# parent context.
- `reliability` — owns the one boundary-fault surface and the single retry policy; every failure returns as a typed fault, never a sentinel.
- `execution` — admits host facts caller-owned, reads secrets through the settings-admitted boundary, and mints no stamp beside the inbound frame.
- Concurrency stays bounded under `StagePlan` and the one scheduler owner, every lane draining to a `DrainReceipt`.
- `evidence` — keys identity by content through the one hashing owner reproducing the C# `XxHash128` seed.
- Evidence catalogue and grammar surfaces emit what the `assay code` rail consumes.
- `clock` — owns the one `Hlc`/`ElementId`/`Tenant` spelling; the two-half stamp reproduces the C# mint bit-identically and is never re-minted.
- A stamp's physical half is host-minted rather than wall-clock, its element id content-stable; the wire codec and admission consume this owner.
