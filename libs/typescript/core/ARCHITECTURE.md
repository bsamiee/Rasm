# [TS_CORE_ARCHITECTURE]

`core` is the branch's S0 vocabulary-and-law package: `value`, `state`, `interchange`, and `observe` meet through one content identity, one clock law, one fault vocabulary, and one keyed-decode wire registry. Core owns decode, vocabulary, and the capability dial — never serving or persistence. `value` roots the internal graph — every other sub-domain composes it and none feeds back.

## [01]-[DOMAIN_MAP]

```text codemap
core/
└── src/
    ├── value/            # Cross-language value floor — every brand decodes once and travels settled
    │   ├── schema.ts     # Shape vocabulary, Shape.Refined brands, and Shape.Ingress decode ceilings
    │   ├── identity.ts   # Identity.App deployment spine and the Identity.Tenant scope key
    │   ├── contentKey.ts # Digest engine and its Digest.Key content identity
    │   ├── clock.ts      # Clock.Hlc hybrid-logical stamp and Clock.Uncertainty windows
    │   ├── quantity.ts   # Quantity magnitude and its Quantity.Dimension vector, canonicalized at admission
    │   └── fault.ts      # Fault.Class vocabulary, Fault.Budget ledger, and Fault.Degrade ladder
    ├── state/            # Host-free state algebra over the value floor
    │   ├── merge.ts      # Lawful CRDT algebra and the Merge.Law table
    │   ├── fold.ts       # Keyed folds, Fold.AsOf, Fold.Replay, and Fold.Window
    │   ├── causal.ts     # Version-vector lattice, causal delivery buffer, and stability frontier
    │   ├── commit.ts     # Content-keyed commit graph, branch heads, and Merkle summaries
    │   ├── machine.ts    # Transition invocation, data-driven statechart, and host-neutral restore
    │   ├── evidence.ts   # Decoded outcomes, receipts, progress, and Evidence.Availability verdicts
    │   ├── feed.ts       # Clock.Hlc-ordered, tenant-keyed evidence timeline and its column band
    │   └── presence.ts   # Actor-presence CRDT over proven merge rows
    ├── interchange/      # Contract wire plane — generated bindings, codecs, and the capability dial; never serving
    │   ├── format.ts     # Byte-dialect engines behind one decode transform
    │   ├── codec.ts      # Wire families over one keyed-decode registry and the closed family census
    │   ├── frame.ts      # Frame reassembly, geometry tensor views, and residency under Shape.Ingress ceilings
    │   ├── contract.ts   # Descriptor-drift diff into graded verdicts
    │   ├── carrier.ts    # W3C propagation-context value, its total folds, and the closed per-transport dialect table
    │   └── invoke.ts     # Capability dial and both directions of the command contract
    └── observe/          # Observability vocabulary and derivation; zero exporters live here
        ├── convention.ts # Typed semconv, metric, and event vocabulary with wire-name translation, the metric-plane roster, and one instrument mount
        ├── slo.ts        # Reliability objective, SLI, SLO, filter, and burn-rate alert algebra
        ├── board.ts      # Dashboard model, the query algebra with its render targets, pack dispatch, metric snapshot
        └── tap.ts        # Hook-point name rows, the veto/observe/replay modality table, and the tap contract
```

## [02]-[STRATA]

- S0 `value` — mints the floor once; `identity` and `fault` compose only `Shape.Refined` from `schema`.
- S1 `state` — pure algebra over the value floor; the merge↔fold cycle never forms: `Fold.run` arrives as a caller parameter, never an import.
- S1 `commit` rides beside `causal` on `Digest.Key`; `presence` rides beside `merge`; `machine` composes no interior sibling.
- S1 `observe` — vocabulary and derivation over the value floor alone; peer to `state` with no edge between them.
- S2 `interchange` — the decode boundary composing all three ranks; `contract` and `invoke` consume `codec`'s `Wire` beside `frame`.
- S2 `carrier` takes the same census union type-only, so its typed-metadata roster closes against the wire families with no value edge.
- S2 `format` reads the `Shape.Ingress` ceiling its framed lane applies to every admitted message.

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
    accDescr: Three interior strata — interchange over the state and observe peers onto the value floor; imports downward, one forbidden upward edge.
    subgraph S2["S2 INTERCHANGE"]
        Invoke[invoke]
        CarrierP[carrier]
        Contract[contract]
        Codec[codec · Wire]
        Format[format]
        Frame[frame]
    end
    subgraph S1["S1 STATE + OBSERVE"]
        Tap[tap]
        Board[board]
        Reliability[slo · Reliability]
        Convention[convention]
        Feed[feed]
        Evidence[evidence]
        Transition[machine · Transition]
        Fold[fold]
        Causal["causal · commit"]
        Merge["merge · presence"]
    end
    subgraph S0["S0 VALUE"]
        Identity[identity]
        Quantity[quantity]
        Clock[clock]
        Digest[contentKey · Digest]
        Schema[schema]
        Fault[fault]
    end
    Codec i1@--> Format
    Frame e1@-->|"[IMPORT]: Wire"| Codec
    Contract i2@--> Codec
    Invoke i3@--> Codec
    Invoke i12@--> CarrierP
    CarrierP e19@-.->|"[TYPE]: Wire.Family"| Codec
    Format e20@-->|"[IMPORT]: Shape.Ingress"| Schema
    CarrierP e18@-->|"[IMPORT]: Identity.Tenant"| Identity
    Codec e2@-->|"[IMPORT]: Tally"| Evidence
    Invoke e3@-->|"[IMPORT]: Convention"| Convention
    Frame e4@-->|"[IMPORT]: Shape.Ingress"| Schema
    Causal e5@-->|"[IMPORT]: Clock.Hlc"| Clock
    Causal i4@--> Merge
    Causal e6@-->|"[IMPORT]: Digest.Key"| Digest
    Fold i5@--> Causal
    Evidence i6@--> Fold
    Evidence e7@-->|"[IMPORT]: Identity.Tenant"| Identity
    Feed i7@--> Evidence
    Feed e8@-->|"[IMPORT]: Quantity.Dimension"| Quantity
    Transition e21@-->|"[IMPORT]: Fault.Class"| Fault
    Reliability i8@--> Convention
    Tap i12@--> Convention
    Board i9@--> Reliability
    Convention e9@-->|"[IMPORT]: Identity.App"| Identity
    Tap e10@-->|"[IMPORT]: Fault.Class"| Fault
    Tap e11@-->|"[IMPORT]: Identity.App"| Identity
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
    accTitle: Core wire-plane seam registry
    accDescr: Wire, Frame, and Invoke admit peer-owned contracts; every external edge preserves its producer spelling.
    subgraph core[CORE]
        Digest[Digest]
        Quantity[Quantity]
        Wire[Wire families]
        Frame[Frame reassembly]
        Invoke[Capability dial]
    end
    Rasm{{Rasm}}
    Compute{{Rasm.Compute}}
    Element{{Rasm.Element}}
    Persistence[(Rasm.Persistence)]
    Bim([Rasm.Bim])
    Materials([Rasm.Materials])
    AppUi([Rasm.AppUi])
    AppHost([Rasm.AppHost])
    Artifacts([python:artifacts])
    Rasm e1@<-->|"[CONTENT_KEY]: XxHash128"| Digest
    Compute e2@<-->|"[WIRE]: QuantityFamily"| Quantity
    Compute e3@-->|"[WIRE]: ReceiptEnvelopeWire"| Wire
    Element e4@<-->|"[WIRE]: rasm.element.v1"| Wire
    Persistence e5@-->|"[WIRE]: CrdtOpWire"| Wire
    Persistence e6@-->|"[WIRE]: SnapshotHeader"| Wire
    Bim e7@-->|"[WIRE]: IfcWire"| Frame
    Bim e8@-->|"[WIRE]: BcfTopicWire"| Wire
    Bim e20@-->|"[WIRE]: PredicateWire"| Wire
    Materials e10@-->|"[WIRE]: MaterialWire"| Wire
    Materials e18@-->|"[WIRE]: TextureSetWire"| Wire
    Artifacts e19@-->|"[WIRE]: AssetSetManifest"| Wire
    AppUi e11@-->|"[WIRE]: CommandPayloadWire"| Invoke
    AppUi e12@-->|"[WIRE]: GeometryResidencyWire"| Frame
    AppUi e13@-->|"[WIRE]: EvidenceTimelineWire"| Wire
    AppHost e14@-->|"[WIRE]: DescriptorPinWire"| Invoke
    AppHost e15@-->|"[WIRE]: ReceiptEnvelopeWire"| Wire
    AppHost e16@-->|"[WIRE]: CommandAvailabilityWire"| Wire
    AppHost e17@-->|"[WIRE]: BindingStatusWire"| Wire
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
    accDescr: Core sends value, state, wire, observe, and carrier shapes; data and IaC return Board.Query targets.
    subgraph core[CORE]
        Digest[Digest]
        Wire[Wire families]
        Frame[Frame reassembly]
        Fold[Keyed fold]
        Feed[Evidence feed]
        Identity[Identity]
        Fault[Fault policy]
        Convention[Semconv]
        Reliability[Reliability]
        Board[Dashboard]
        Tap[Hook rail]
        Carrier[Propagation carrier]
    end
    Runtime{{runtime}}
    Data[(data)]
    Ui([ui])
    Security([security])
    Iac([iac])
    Digest e1@-->|"[CONTENT_KEY]: Digest.Key&lt;&quot;content&quot;&gt;"| Data
    Digest e2@-->|"[CONTENT_KEY]: Digest.Key&lt;&quot;content&quot;&gt;"| Runtime
    Wire e3@-->|"[SHAPE]: FlagVerdict"| Runtime
    Fold e4@-->|"[SHAPE]: Fold.Plan"| Data
    Feed e5@-->|"[SHAPE]: Feed.Document"| Ui
    Identity e6@-->|"[SHAPE]: Identity.Tenant"| Security
    Identity e7@-->|"[SHAPE]: Identity.Tenant"| Data
    Identity e8@-->|"[SHAPE]: Identity.App"| Runtime
    Fault e9@-->|"[SHAPE]: Fault.Budget"| Runtime
    Convention e10@-->|"[SHAPE]: Convention"| Runtime
    Convention e12@-->|"[SHAPE]: Convention"| Data
    Convention e13@-->|"[SHAPE]: Convention"| Security
    Convention e22@-->|"[SHAPE]: Convention"| Iac
    Board e11@-->|"[PROJECTION]: Board.DashboardModel/Board.Query"| Iac
    Reliability e14@-->|"[PROJECTION]: Reliability.Alert.Spec"| Iac
    Frame e15@-->|"[SHAPE]: Residency.Ledger"| Ui
    Wire e21@-->|"[SHAPE]: Wire.ModelDiff/Wire.BcfTopic/Wire.BcfViewpoint/Wire.ControlIntent/Wire.LayoutProgram/Wire.CommandGate/Wire.EvidenceTimeline/Wire.GeoFeature/Wire.PbrGroups/Wire.TextureSet/Wire.AssetSetManifest"| Ui
    Reliability e16@-->|"[PROJECTION]: Reliability.Objective"| Iac
    Reliability e29@-->|"[PROJECTION]: Reliability.Filter"| Iac
    Tap e17@-->|"[SHAPE]: Tap.Registry"| Runtime
    Carrier e18@-->|"[SHAPE]: Carrier.Context"| Runtime
    Tap e19@-->|"[SHAPE]: Tap.Point"| Data
    Tap e20@-->|"[SHAPE]: Tap.Name/Modality/Handler/Veto/Breach"| Ui
    Tap e23@-->|"[SHAPE]: Tap.Name/Modality/Handler"| Security
    Board e24@-->|"[SHAPE]: Board.Query.Residence"| Data
    Wire e25@-->|"[SHAPE]: Hops"| Data
    Data e26@-->|"[SHAPE]: Board.Query.Target"| Board
    Board e27@-->|"[PROJECTION]: Board.DashboardModel.Signal"| Data
    Iac e28@-->|"[PROJECTION]: Board.Query.Target"| Board
```

## [04]-[INTERNAL]

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Core capability spine
    accDescr: Value owners ground state and observe; both land through interchange as typed contracts.
    Input([program inputs])
    Value[value · Shape + Identity + Digest + Clock + Quantity + Fault]
    State[state · Merge + Fold + Transition + Evidence]
    Observe[observe · Convention + Reliability + Board + Tap]
    Interchange[interchange · Format + Wire + Frame + Carrier + Invoke]
    Output([typed contracts])
    Input e1@-->|"admit: settled values"| Value
    Value e2@-->|"compose: state algebra"| State
    Value e3@-->|"compose: signal policy"| Observe
    State e4@-->|"land: state families"| Interchange
    Observe e5@-->|"govern: crossing vocabulary"| Interchange
    Interchange e6@-->|"emit: typed contracts"| Output
```

One authority per concept and growth-as-row is the organization law: `value` mints each floor primitive exactly once and everything above composes it settled, `state` stays pure algebra whose one `AsOf` coordinate forbids a second replay vocabulary, `interchange` lands a new contract family as one census row with its landing row — never a page — and `observe` owns vocabulary and derivation only.

Growth is one edit at the owner: a wire family is a census row, a fault class a table entry, an identity dimension one static. Exact delegating sites and per-owner wiring live on the owning implementation pages.

## [05]-[BOUNDARIES]

- Core imports nothing from the branch and nothing host-bound; every module runs identically under node, bun, and the browser.
- Core owns TypeScript contract bindings; one registry encodes or decodes each conforming wire family for every later-stratum consumer.
- Every cross-language primitive admits and brands at one seam, and cross-runtime parity proves bit-identical against the frozen contract corpus.
- Secret derivation is the security folder's concern; the digest engine here is content identity only.
- Persistence, serving, transport hosting, rendering, and exporters are later-wave concerns; core defines the shapes they carry and nothing they run.
