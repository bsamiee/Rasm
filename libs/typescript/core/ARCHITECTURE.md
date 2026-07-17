# [TS_CORE_ARCHITECTURE]

`core` is the branch's wave-0 vocabulary-and-law package: `value`, `state`, `interchange`, and `observe` meet through one content identity, one clock law, one fault vocabulary, and one keyed-decode wire registry. Core owns decode and vocabulary, never transport, persistence, or serving. `value` roots the internal graph — every other sub-domain composes it and none feeds back.

## [01]-[DOMAIN_MAP]

```text codemap
core/
└── src/
    ├── value/            # Cross-language value floor — every brand decodes once and travels settled
    │   ├── schema.ts     # Refined branded-primitive vocabulary and the Ingress decode-budget ceilings
    │   ├── identity.ts   # AppIdentity deployment spine and the TenantContext scope key
    │   ├── contentKey.ts # One content-identity digest and the Digest engine beneath it
    │   ├── clock.ts      # Hlc hybrid-logical stamp and the Uncertainty grade windows
    │   ├── quantity.ts   # SI-coherent magnitude and its Dimension vector, canonicalized at admission
    │   └── fault.ts      # Fault severity vocabulary, the retry Budget ledger, and the Degrade ladder
    ├── state/            # Host-free state algebra over the value floor
    │   ├── merge.ts      # Lawful CRDT merge algebra and the Converge law surface
    │   ├── fold.ts       # Keyed-fold owner, the AsOf time coordinate, and the Replay memory lane
    │   ├── causal.ts     # Version-vector lattice, causal delivery buffer, and stability frontier
    │   ├── commit.ts     # Content-keyed commit graph, branch heads, and Merkle summaries
    │   ├── machine.ts    # Data-driven statechart, its macrostep fold, and the serializable actor
    │   ├── evidence.ts   # Decoded outcome family — receipts, progress, and availability
    │   ├── feed.ts       # Hlc-ordered evidence timeline and its column band
    │   └── presence.ts   # Actor-presence CRDT over proven merge rows
    ├── interchange/      # C#-minted wire plane — decode and vocabulary, never transport
    │   ├── format.ts     # Byte-dialect engines behind one decode transform
    │   ├── codec.ts      # One keyed-decode registry over the closed wire-family census
    │   ├── frame.ts      # Keyed frame reassembly fold under the Ingress budget
    │   ├── contract.ts   # Descriptor-drift diff into graded verdicts
    │   └── invoke.ts     # Both directions of the command capability contract
    └── observe/          # Observability vocabulary and derivation; zero exporters live here
        ├── convention.ts # Typed semconv attribute, metric, and event vocabulary
        ├── slo.ts        # Objective/SLI algebra and the burn-rate alert derivation
        └── board.ts      # Dashboard model, query, and pack/suite dispatch
```

## [02]-[STRATA]

- S0 `value` — mints the floor exactly once (`Refined` brands, `Hlc`, `ContentKey` under the `Digest` engine, `Quantity`/`Dimension`, `AppIdentity`/`TenantContext`, `Budget`) and imports no sibling sub-domain; `identity` and `fault` compose `schema`'s `Refined` vocabulary alone.
- S1 `state` — pure algebra over the value floor: `causal` composes `merge` and `Hlc`, `fold` composes `causal`, `evidence` mints `ProgressMark` over `fold` and `TenantContext`, `feed` orders `evidence` by `Hlc` under a `Dimension` band; `commit` rides with `causal` on `ContentKey`, `presence` with `merge`, and `machine` composes no interior sibling — the merge↔fold cycle never forms because `Fold.run` arrives as a caller parameter, never an import.
- S1 `observe` — vocabulary and derivation over `AppIdentity` alone: `convention` roots, `slo` derives `Alert`, `board` composes both into `DashboardModel`; peer to state with no edge between them.
- S2 `interchange` — the decode boundary composing all three: `format` proto engines under `codec`'s keyed registry, `frame`/`contract`/`invoke` over `Wire`, `frame` admitting under `Ingress`, `codec` landing `ProgressMark`, `invoke` landing `Convention`.

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
    accTitle: Core interior import strata
    accDescr: Three interior waves — interchange over the state and observe peer wave onto the value floor — every import downward or floor-interior, labeled edges naming one sourced type each, and one forbidden upward edge styled red.
    subgraph S2["S2 INTERCHANGE"]
        Invoke[invoke]
        Contract[contract]
        Codec[codec]
        Format[format]
        Frame[frame]
    end
    subgraph S1["S1 STATE + OBSERVE"]
        Board[board]
        Slo[slo]
        Convention[convention]
        Feed[feed]
        Evidence[evidence]
        Fold[fold]
        Causal["causal · commit"]
        Merge["merge · presence"]
    end
    subgraph S0["S0 VALUE"]
        Identity[identity]
        Quantity[quantity]
        Clock[clock]
        ContentKey[contentKey]
        Schema[schema]
        Fault[fault]
    end
    Codec i1@--> Format
    Frame e1@-->|"[IMPORT]: Wire"| Codec
    Contract i2@--> Codec
    Invoke i3@--> Codec
    Codec e2@-->|"[IMPORT]: ProgressMark"| Evidence
    Invoke e3@-->|"[IMPORT]: Convention"| Convention
    Frame e4@-->|"[IMPORT]: Ingress"| Schema
    Causal e5@-->|"[IMPORT]: Hlc"| Clock
    Causal i4@--> Merge
    Causal e6@-->|"[IMPORT]: ContentKey"| ContentKey
    Fold i5@--> Causal
    Evidence i6@--> Fold
    Evidence e7@-->|"[IMPORT]: TenantContext"| Identity
    Feed i7@--> Evidence
    Feed e8@-->|"[IMPORT]: Dimension"| Quantity
    Slo i8@--> Convention
    Board i9@--> Slo
    Convention e9@-->|"[IMPORT]: AppIdentity"| Identity
    Identity i10@--> Schema
    Fault i11@--> Schema
    Invoke ~~~ Board
    S0 f1@-->|"forbidden: upward import"| S2
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    classDef edgeError stroke:#FF5555,stroke-width:3px,color:#F8F8F2
    class Format,Codec,Frame,Contract,Invoke,Merge,Causal,Fold,Evidence,Feed,Convention,Slo,Board primary
    class Schema,Clock,ContentKey,Quantity,Identity,Fault recessed
    class e1,e2,e3,e4,e5,e6,e7,e8,e9,i1,i2,i3,i4,i5,i6,i7,i8,i9,i10,i11 edgeControl
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
    accTitle: Core C# wire-plane seam registry
    accDescr: Core value, interchange, and state owners decoding kinded wires spelled verbatim from the C# endpoint pages — the kernel, compute, element, persistence, bim, materials, the app UI, and the app host — edge rails colored by kind and nodes classed by seam direction.
    subgraph core[CORE]
        ContentKey[Content key]
        Quantity[Quantity]
        Codec[Wire codec]
        Feed[Evidence feed]
        Frame[Frame reassembly]
        Invoke[Capability]
    end
    Rasm{{Rasm}}
    Compute{{Rasm.Compute}}
    Element{{Rasm.Element}}
    Persistence[(Rasm.Persistence)]
    Bim([Rasm.Bim])
    Materials([Rasm.Materials])
    AppUi([Rasm.AppUi])
    AppHost([Rasm.AppHost])
    Rasm e1@<-->|"[CONTENT_KEY]: XxHash128"| ContentKey
    Compute e2@<-->|"[WIRE]: QuantityFamily"| Quantity
    Compute e3@-->|"[WIRE]: ReceiptEnvelopeWire"| Codec
    Element e4@<-->|"[WIRE]: rasm.element.v1"| Codec
    Persistence e5@-->|"[WIRE]: CrdtOpWire"| Codec
    Persistence e6@-->|"[WIRE]: SnapshotHeader"| Codec
    Bim e7@-->|"[WIRE]: IfcWire"| Codec
    Bim e8@-->|"[WIRE]: BcfTopicWire"| Codec
    Bim e9@-->|"[WIRE]: GeoFeatureWkb"| Codec
    Materials e10@-->|"[WIRE]: MaterialWire"| Codec
    AppUi e11@-->|"[WIRE]: CommandPayloadWire"| Invoke
    AppUi e12@-->|"[WIRE]: GeometryResidencyWire"| Frame
    AppUi e13@-->|"[WIRE]: EvidenceFeed"| Feed
    AppHost e14@-->|"[CONTENT_KEY]: CapabilityDescriptor"| Invoke
    AppHost e15@-->|"[WIRE]: ReceiptEnvelopeWire"| Codec
    AppHost e16@-->|"[WIRE]: DegradationLevel"| Codec
    AppHost e17@-->|"[WIRE]: BindingStatusWire"| Codec
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef recessed fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    class ContentKey,Quantity,Codec,Feed,Frame,Invoke primary
    class Rasm,Compute,Element external
    class Persistence data
    class Bim,Materials,AppUi,AppHost recessed
    class e1,e2,e3,e4,e5,e6,e7,e8,e9,e10,e11,e12,e13,e14,e15,e16,e17 edgeData
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
    accTitle: Core TypeScript sibling seam registry
    accDescr: Core value, state, interchange, and observe owners handing content identity and decoded shapes to the data, runtime, ui, security, and iac siblings, edge rails colored by kind and nodes classed by seam direction.
    subgraph core[CORE]
        ContentKey[Content key]
        Codec[Wire codec]
        Fold[Keyed fold]
        Feed[Evidence feed]
        Identity[App identity]
        Fault[Fault policy]
        Convention[Semconv]
        Board[Dashboard]
    end
    Runtime{{runtime}}
    Data[(data)]
    Ui([ui])
    Security([security])
    Iac([iac])
    ContentKey e1@-->|"[CONTENT_KEY]: ContentKey"| Data
    ContentKey e2@-->|"[CONTENT_KEY]: Digest"| Runtime
    Codec e3@-->|"[SHAPE]: FlagVerdict"| Runtime
    Fold e4@-->|"[SHAPE]: Fold.Plan"| Data
    Feed e5@-->|"[SHAPE]: Feed.Document"| Ui
    Identity e6@-->|"[SHAPE]: TenantContext"| Security
    Identity e7@-->|"[SHAPE]: TenantContext"| Data
    Identity e8@-->|"[SHAPE]: AppIdentity"| Runtime
    Fault e9@-->|"[SHAPE]: Budget"| Runtime
    Convention e10@-->|"[SHAPE]: Convention"| Runtime
    Board e11@-->|"[PROJECTION]: DashboardModel"| Iac
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef annotation fill:#21222C,stroke:#6272A4,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    classDef edgeControl stroke:#FF79C6,color:#F8F8F2
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    class ContentKey,Codec,Fold,Feed,Identity,Fault,Convention,Board primary
    class Runtime external
    class Data data
    class Ui,Security,Iac annotation
    class e1,e2 edgeData
    class e3,e4,e5,e6,e7,e8,e9,e10 edgeControl
    class e11 edgeExternal
```

## [04]-[ORGANIZATION]

`value` is the floor: every brand decodes once at the boundary and travels settled — `contentKey` owns the one digest mint, `clock` the one time vocabulary, `fault` the one severity and retry policy every rail inherits. `state` is pure algebra over those values: `merge` proves the lattice laws `causal`, `presence`, and `fold` compose, `fold` carries the single `AsOf` time coordinate so no second replay vocabulary exists, and `machine` compiles its statechart once into the macrostep fold, its drivers, and the serializable actor. `interchange` is the decode boundary: `codec` is the one registry every C#-minted family lands on, and a new wire family is one census row plus one landing row, never a page. `observe` owns vocabulary and derivation only. Exact delegating sites and per-owner wiring live on the owning implementation pages.

## [05]-[BOUNDARIES]

- Core imports nothing from the branch and nothing host-bound; every module runs identically under node, bun, and the browser.
- C# owns every `*Wire` shape; core decodes it verbatim, authors no wire, and lands each family's decoded shape once even for a later-wave consumer.
- Secret derivation is the security folder's concern; the digest engine here is content identity only.
- Persistence, transport, serving, rendering, and exporters are later-wave concerns; core defines the shapes they carry and nothing they run.
