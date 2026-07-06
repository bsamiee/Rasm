# [TS_CORE_ARCHITECTURE]

The domain map of `core` — the wave-0 vocabulary-and-law package every branch folder composes. Four sub-domains (`value`, `state`, `interchange`, `observe`) meet through the one content identity, the one clock law, the one fault vocabulary, and the one keyed-decode wire registry; core owns decode and vocabulary, never transport, persistence, or serving.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, camelCase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
core/
└── src/
    ├── value/                 # The cross-language value floor: brands, identity, digest, clock, quantity, fault policy
    │   ├── schema.ts          # Refined branded-primitive vocabulary (Guid-v7, OrdinalKey, JsonPointer, Locale) + Ingress decode-budget ceilings
    │   ├── identity.ts        # AppIdentity four-dimension value + TenantContext with its derived scope key
    │   ├── contentKey.ts      # ContentKey — XxHash128 seed-zero :x32 digest — and the Digest engine beneath it
    │   ├── clock.ts           # Hlc two-half hybrid-logical stamp under the C# compose-order law + Uncertainty grade windows
    │   ├── quantity.ts        # Quantity — SI-coherent magnitude + seven-axis Dimension vector, canonicalized once at C# admission
    │   └── fault.ts           # FaultClass ten-class severity vocabulary, FaultCapture/FaultEnricher, the Budget retry ledger
    ├── state/                 # Host-free state algebra: lawful merge, keyed folds, causality, machines, evidence, presence
    │   ├── merge.ts           # Merge.Instance lawful CRDT algebra over @effect/typeclass atoms + the Converge law surface
    │   ├── fold.ts            # Fold.Plan keyed-fold owner, the AsOf time coordinate, the Replay versioned memory lane
    │   ├── causal.ts          # Vector version-vector lattice, causal delivery buffer, stability frontier
    │   ├── commit.ts          # Commit content-keyed graph + branch heads + Merkle summaries
    │   ├── machine.ts         # Transition statechart — node tree + guarded rows as data, macrostep fold, serializable actor
    │   ├── evidence.ts        # Receipt/ReceiptEnvelope, ProgressMark, availability — the decoded outcome family
    │   ├── feed.ts            # Feed — the Hlc-ordered evidence timeline + the Feed.Document column band
    │   └── presence.ts        # Actor-presence CRDT over Merge.struct proven rows
    ├── interchange/           # The C#-minted wire plane: byte dialects, keyed decode, frames, drift, capability
    │   ├── format.ts          # Four byte-dialect engines (proto, cbor, msgpack, ndjson) behind Schema.transformOrFail
    │   ├── codec.ts           # Wire — ONE keyed-decode registry over the closed wire-family census; WireFault; Parity; feed rows
    │   ├── frame.ts           # Frame reassembly — keyed Mealy fold over ordinal frames under the Ingress budget
    │   ├── contract.ts        # ContractDrift — FileDescriptorSet reflection diff into graded verdicts
    │   └── invoke.ts          # CapabilityDescriptor + Dial — both directions of the command contract
    └── observe/               # Observability vocabulary and derivation; zero exporters live here
        ├── convention.ts      # Convention — typed semconv attribute/metric/event vocabulary rows
        ├── slo.ts             # Objective/Sli algebra + the multi-window multi-burn-rate alert derivation
        └── board.ts           # DashboardModel + Query + the pack/suite dispatch riding the same owner
```

## [02]-[SEAMS]

```text seams
value/contentKey     ⇄  csharp:Rasm                   # [CONTENT_KEY]: XxHash128 seed-zero :x32 content identity
value/contentKey     ⇄  typescript:data/object        # [CONTENT_KEY]: ObjectKey IS ContentKey — a delegating mint site, never a second hash
value/contentKey     ⇄  typescript:runtime/browser    # [CONTENT_KEY]: Digest.mint("content") off-thread reassembly verify — a delegating mint site
value/contentKey     ←  csharp:Rasm.Compute           # [WIRE]: XxHash128 seed-zero two-half reproduced off-thread [gated: hash-wasm]
value/clock          ←  csharp:Rasm.AppHost           # [WIRE]: Hlc two-half compose-order parity
value/quantity       ⇄  csharp:Rasm.Compute           # [WIRE]: QuantityWire SI-scalar decode
interchange/codec    ←  csharp:Rasm.AppHost           # [WIRE]: ReceiptEnvelope/TenantContext/livewire triple/FlagVerdict/CredentialPem landings
interchange/invoke   ←  csharp:Rasm.AppHost           # [CONTENT_KEY]: CapabilityDescriptor command-shape
interchange/codec    ←  csharp:Rasm.Compute           # [WIRE]: proto suite + FaultDetail + ProgressMark frames
interchange/codec    ←  csharp:Rasm.Persistence       # [WIRE]: OpLog/Snapshot CRDT wire + JsonPatch egress
interchange/codec    ⇄  csharp:Rasm.Element           # [WIRE]: ElementGraph content-keyed wire under the drift gate
interchange/codec    ←  csharp:Rasm.Bim               # [WIRE]: BcfTopic/BcfViewpoint + BimWire/DiffWire/OpLogWire/IdsAudit + GeoFeature WKB
interchange/codec    ←  csharp:Rasm.Materials         # [WIRE]: MaterialWire/PbrGroups appearance decode
interchange/invoke   ←  csharp:Rasm.AppUi/Shell       # [WIRE]: CommandPayloadWire
interchange/codec    ←  csharp:Rasm.AppUi/Shell       # [WIRE]: CommandGateWire + ControlIntentWire + LayoutConstraintWire
interchange/frame    ←  csharp:Rasm.AppUi/Render      # [WIRE]: GeometryResidencyWire
interchange/codec    ←  csharp:Rasm.AppUi/Render      # [WIRE]: RenderReceiptWire
state/feed           ←  csharp:Rasm.AppUi/Diagnostics # [WIRE]: EvidenceFeed / EvidenceTimeline envelope rows absorbed through Feed.timeline
interchange/contract ←  csharp:Rasm.Compute           # [WIRE]: FileDescriptorSet generations feeding the drift verdict
interchange/codec    →  typescript:runtime/proc       # [SHAPE]: FlagVerdict OpenFeature-contract landing the flag service consumes
state/fold           →  typescript:data/read          # [SHAPE]: Fold.Plan bound at the durable projection altitude
state/feed           →  typescript:ui/view            # [SHAPE]: Feed.Document column band driving the dynamic grid fold
value/fault          →  typescript:runtime/net        # [SHAPE]: Budget ledger rows compiled into lane pulses
observe/convention   →  typescript:runtime/otel       # [SHAPE]: Convention rows stamped at every emission
observe/board        →  typescript:iac/operate        # [PROJECTION]: DashboardModel.Encoded + Alert.Spec + Slo.Objective realized as grafana rows
```

## [03]-[ORGANIZATION]

`value` is the floor: every brand decodes once at the boundary and travels settled — `contentKey` owns the one digest mint (delegating sites are `interchange/frame`, the runtime browser decode worker, and the data object store), `clock` owns the one time vocabulary, `fault` owns the one severity/retry policy every rail inherits. `state` is pure algebra over those values: `merge` proves the lattice laws `causal`, `presence`, and `fold` compose; `fold` carries the single `AsOf` time coordinate so no second replay vocabulary exists; `machine` compiles the node-tree statechart once at `Transition.spec` — the pure macrostep fold, the batch and stream drivers, and the serializable actor all run one compiled value. `interchange` is the decode boundary: `format` admits bytes, `codec` is the ONE registry where every C#-minted family lands — a new wire family is one census row plus one landing row, never a page — `contract` grades descriptor drift before decode ever fails at runtime, and `invoke` carries the capability contract both directions. `observe` owns vocabulary and derivation only; the OTLP wire lives in the runtime folder, and the grafana realization lives in iac.

## [04]-[BOUNDARIES]

- Core imports nothing from the branch and nothing host-bound; every module runs identically under node, bun, and the browser.
- C# owns every `*Wire` shape; core decodes verbatim INTO owned vocabulary and authors no wire. Families whose consumers live in later waves land wire-owned decoded shapes here once, adopted-verbatim on the C#-minted names.
- Secret derivation (argon2id/bcrypt/scrypt/pbkdf2) is the security folder's concern; the digest engine here is content identity only.
- Persistence, transport, serving, rendering, and exporters are later-wave concerns; core defines the shapes they carry and nothing they run.
