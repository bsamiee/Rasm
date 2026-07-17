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
├── execution/          # Caller-owned host-fact admission, bounded concurrency, the worker fabric, and recipe execution
│   ├── admission.py    # Runtime context, causal frames, and settings admission
│   ├── lanes.py        # Lane-policy task groups and the stage-plan DAG
│   ├── workers.py      # Worker fabric: kind family, kernel crossing, warm pools, remote/device arms, the guest sandbox, and supervision
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
    Transport e4@<-->|"[WIRE]: ProtoVocabulary"| Compute
    Transport e5@<-->|"[WIRE]: OpLogEntry"| Persistence
    Transport e6@<-->|"[WIRE]: DiscoveryResult"| AppHost
    Observability e7@<-->|"[TRANSPORT]: TraceContext"| AppHost
    AppHost e8@<-->|"[WIRE]: HlcStamp"| Clock
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    class Evidence,Transport,Observability,Clock primary
    class Rasm,Element,Compute,AppHost external
    class Persistence data
    class e1,e2,e3,e4,e5,e6,e8 edgeData
    class e7 edgeExternal
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
        Transport[Transport]
        Evidence[Evidence]
        Observability[Observability]
        Execution[Execution]
    end
    Geometry{{python:geometry}}
    Data{{python:data}}
    Artifacts{{python:artifacts}}
    Compute([python:compute])
    Transport e5@<-->|"[WIRE]: TessellationRequest"| Geometry
    Geometry e1@-->|"[CONTENT_KEY]: ContentIdentity"| Evidence
    Geometry e9@-->|"[PORT]: RecipeInterface"| Execution
    Data e2@-->|"[CONTENT_KEY]: ContentIdentity"| Evidence
    Transport e6@-->|"[TRANSPORT]: ResourceRef"| Data
    Data e11@-->|"[RECEIPT]: QueryReceipt"| Observability
    Execution e14@-->|"[BOUNDARY]: on_thread"| Data
    Evidence e3@-->|"[CONTENT_KEY]: ContentIdentity"| Artifacts
    Artifacts e10@-->|"[RECEIPT]: ArtifactReceipt"| Observability
    Execution e8@-->|"[CONTENT_KEY]: ContentKey"| Artifacts
    Execution e15@-->|"[PORT]: Kernel"| Artifacts
    Evidence e4@-->|"[CONTENT_KEY]: ParityReceipt"| Compute
    Transport e7@-->|"[BOUNDARY]: ResourceRef"| Compute
    Observability e13@-->|"[PORT]: measured"| Compute
    Execution e12@-->|"[PORT]: Kernel"| Compute
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    classDef edgeSuccess stroke:#50FA7B,color:#F8F8F2
    class Evidence,Transport,Execution,Observability primary
    class Geometry,Data,Artifacts external
    class Compute recessed
    class e1,e2,e3,e4,e5,e8 edgeData
    class e6 edgeExternal
    class e7,e9,e12,e13,e14,e15 edgeControl
    class e10,e11 edgeSuccess
```

Each fence's home roster holds only the sub-domains carrying a seam with that peer plane: `reliability` crosses no boundary, `clock` faces only the C# plane, `execution` only the Python plane. Frozen registry names spell from the counterpart's endpoint page; `ServerHost`/`CommandReceipt`, `PROTO_VOCABULARY`, `CrdtOp`, and `ContentKey` are this package's interior spellings behind the `DiscoveryResult`, `ProtoVocabulary`, `OpLogEntry`, and `ContentAddress` wires.

## [03]-[INTERNAL]

Interior composition is one acyclic import rail: `faults` roots the graph, every module returns through it, and `serve` is the terminal composition tier. Edges below are the transitive reduction of the real module imports — a drawn edge is a direct import no shorter chain explains.

- S0 `faults` — mints the one boundary-fault union and rail (`BoundaryFault`, `RuntimeRail`) exactly once and imports nothing; every module above returns through it.
- S1–S3 identity band — `clock` (`Hlc`/`ElementId`/`Tenant`), `identity` (`ContentKey`), and `shapes` (`PROTO_VOCABULARY`) sit directly on faults; `receipts` composes identity, and `metrics`, `reproduction` (`ParityReceipt`), and `evidence` fold through receipts.
- S4–S6 execution band — `resilience` (the `RetryClass` policy table) composes metrics; `roots` (`ResourceRef`), `admission`, and `wire` (`CrdtOp`) return through resilience; `workers` (`Kernel`) composes roots, and `telemetry` gates on admission.
- S7–S9 composition tier — `lanes` (`StagePlan`) drives admission and workers, `recipe` (`RecipeInterface`) composes lanes and roots, and `serve` (`DiscoveryResult`/`CommandReceipt`) is the terminal tier wiring recipe, telemetry, and the wire codec.

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
    accTitle: Runtime interior import rail
    accDescr: Transitive-reduced module import rail descending from the serve composition tier through recipe, lanes, and workers onto roots, telemetry and the wire codec joining at admission and resilience, converging through metrics and receipts onto identity and the faults root, with reproduction and evidence joining at receipts and shapes and clock at faults.
    Serve[serve]
    Recipe[recipe]
    Telemetry[telemetry]
    Wire[wire]
    Lanes[lanes]
    Roots[roots]
    Admission[admission]
    Workers[workers]
    Shapes[shapes]
    Clock[clock]
    Resilience[resilience]
    Reproduction[reproduction]
    Evidence[evidence]
    Metrics[metrics]
    Receipts[receipts]
    Identity[identity]
    Faults[faults]
    Serve r1@--> Recipe
    Serve r2@--> Telemetry
    Serve r3@--> Wire
    Recipe r4@--> Lanes
    Workers r5@--> Roots
    Telemetry r6@--> Admission
    Lanes r7@--> Admission
    Lanes r8@--> Workers
    Wire r9@--> Clock
    Wire r10@--> Shapes
    Admission r11@--> Clock
    Admission r12@--> Resilience
    Wire r13@--> Resilience
    Roots r14@--> Resilience
    Resilience r15@--> Metrics
    Metrics r16@--> Receipts
    Reproduction r17@--> Receipts
    Evidence r18@--> Receipts
    Receipts r19@--> Identity
    Identity r20@--> Faults
    Shapes r21@--> Faults
    Clock r22@--> Faults
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    class Serve,Recipe,Telemetry,Wire,Lanes,Roots,Admission,Workers,Shapes,Clock,Resilience,Reproduction,Evidence,Metrics,Receipts,Identity primary
    class Faults recessed
    class r1,r2,r3,r4,r5,r6,r7,r8,r9,r10,r11,r12,r13,r14,r15,r16,r17,r18,r19,r20,r21,r22 edgeControl
```

## [04]-[BOUNDARIES]

Each sub-domain charter is the codemap comment; the boundary law below fixes the one ownership each holds, so a planned-but-empty sub-domain and a misplaced concern both read as gaps. Exact refusals and their enforcing mechanisms live on the owning implementation pages.

- `observability` — produces local evidence only, never an AppHost envelope or health status.
- One shared OTLP exporter and one `MeterProvider` install behind the profile gate; every receipt folds through one attribute-keyed drain.
- Every span rides the inbound C# parent context.
- `reliability` — owns the one boundary-fault surface and the single retry policy; every failure returns as a typed fault, never a sentinel.
- `execution` — admits host facts caller-owned, reads secrets through the settings-admitted boundary, and mints no stamp beside the inbound frame.
- Concurrency stays bounded under `StagePlan` and the one scheduler owner, every lane draining to a `DrainReceipt`.
- Every kernel leaves the loop as one `Kernel` value on the closed worker-kind family; warm pools, restart actuation, and the verdict projection onto the serve health flip stay the workers owner's, and lane capacity projects from the admitted profile row.
- `evidence` — keys identity by content through the one hashing owner reproducing the C# `XxHash128` seed.
- Evidence catalogue and grammar surfaces emit what the `assay code` rail consumes.
- `clock` — owns the one `Hlc`/`ElementId`/`Tenant` spelling; the two-half stamp reproduces the C# mint bit-identically and is never re-minted.
- A stamp's physical half is host-minted rather than wall-clock, its element id content-stable; the wire codec and admission consume this owner.
