# [TS_CORE_ARCHITECTURE]

`core` is the branch's wave-0 vocabulary-and-law package: `value`, `state`, `interchange`, and `observe` meet through one content identity, one clock law, one fault vocabulary, and one keyed-decode wire registry. Core owns decode, vocabulary, and the capability dial — never serving or persistence. `value` roots the internal graph — every other sub-domain composes it and none feeds back.

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
    ├── interchange/      # C#-minted wire plane — decode, vocabulary, and the capability dial; never serving
    │   ├── format.ts     # Byte-dialect engines behind one decode transform
    │   ├── codec.ts      # One keyed-decode registry over the closed wire-family census
    │   ├── frame.ts      # Frame reassembly, geometry tensor views, and the residency ledger under the Ingress budget
    │   ├── contract.ts   # Descriptor-drift diff into graded verdicts
    │   └── invoke.ts     # Capability dial and both directions of the command contract
    └── observe/          # Observability vocabulary and derivation; zero exporters live here
        ├── convention.ts # Typed semconv attribute, metric, and event vocabulary
        ├── slo.ts        # Objective/SLI algebra and the burn-rate alert derivation
        └── board.ts      # Dashboard model, query, pack/suite dispatch, and the live metric snapshot
```

## [02]-[STRATA]

- S0 `value` — mints the floor exactly once (`Refined` brands, `Hlc`, `ContentKey` under the `Digest` engine, `Quantity`/`Dimension`, `AppIdentity`/`TenantContext`, `Budget`) and imports no sibling sub-domain; `identity` and `fault` compose `schema`'s `Refined` vocabulary alone.
- S1 `state` — pure algebra over the value floor: `causal` composes `merge` and `Hlc`, `fold` composes `causal`, `evidence` mints `ProgressMark` over `fold` and `TenantContext`, `feed` orders `evidence` by `Hlc` under a `Dimension` band; `commit` rides with `causal` on `ContentKey`, `presence` with `merge`, and `machine` composes no interior sibling — the merge↔fold cycle never forms because `Fold.run` arrives as a caller parameter, never an import.
- S1 `observe` — vocabulary and derivation over `AppIdentity` alone: `convention` roots, `slo` derives `Alert`, `board` composes both into `DashboardModel`; peer to state with no edge between them.
- S2 `interchange` — the decode boundary composing all three: `format` proto engines under `codec`'s keyed registry, `frame`/`contract`/`invoke` over `Wire`, `frame` admitting under `Ingress`, `codec` landing `ProgressMark`, `invoke` landing `Convention`.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
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
```

## [03]-[SEAMS]

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
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
    Bim e9@-->|"[WIRE]: GeoFeatureWire"| Codec
    Materials e10@-->|"[WIRE]: MaterialWire"| Codec
    AppUi e11@-->|"[WIRE]: CommandPayloadWire"| Invoke
    AppUi e12@-->|"[WIRE]: GeometryResidencyWire"| Frame
    AppUi e13@-->|"[WIRE]: EvidenceTimelineWire"| Feed
    AppHost e14@-->|"[CONTENT_KEY]: CapabilityDescriptor"| Invoke
    AppHost e15@-->|"[WIRE]: ReceiptEnvelopeWire"| Codec
    AppHost e16@-->|"[WIRE]: DegradationLevel"| Codec
    AppHost e17@-->|"[WIRE]: BindingStatusWire"| Codec
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Core TypeScript sibling seam registry
    accDescr: Core value, state, interchange, and observe owners handing content identity and decoded shapes to the data, runtime, ui, security, and iac siblings, edge rails colored by kind and nodes classed by seam direction.
    subgraph core[CORE]
        ContentKey[Content key]
        Codec[Wire codec]
        Frame[Frame reassembly]
        Fold[Keyed fold]
        Feed[Evidence feed]
        Identity[App identity]
        Fault[Fault policy]
        Convention[Semconv]
        Slo[SLO derivation]
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
    Convention e12@-->|"[SHAPE]: Convention"| Data
    Convention e13@-->|"[SHAPE]: Convention"| Security
    Board e11@-->|"[PROJECTION]: DashboardModel"| Iac
    Slo e14@-->|"[PROJECTION]: Alert.Spec"| Iac
    Frame e15@-->|"[SHAPE]: Residency.Ledger"| Ui
    Slo e16@-->|"[PROJECTION]: Slo.Objective"| Iac
```

## [04]-[ORGANIZATION]

One authority per concept and growth-as-row is the organization law: `value` mints each floor primitive exactly once and everything above composes it settled, `state` stays pure algebra whose one `AsOf` coordinate forbids a second replay vocabulary, `interchange` lands a new C#-minted wire family as one census row with its landing row — never a page — and `observe` owns vocabulary and derivation only. Exact delegating sites and per-owner wiring live on the owning implementation pages.

## [05]-[BOUNDARIES]

- Core imports nothing from the branch and nothing host-bound; every module runs identically under node, bun, and the browser.
- C# owns every `*Wire` shape; core decodes it verbatim, authors no wire, and lands each family's decoded shape once even for a later-wave consumer.
- Secret derivation is the security folder's concern; the digest engine here is content identity only.
- Persistence, serving, transport hosting, rendering, and exporters are later-wave concerns; core defines the shapes they carry and nothing they run.
