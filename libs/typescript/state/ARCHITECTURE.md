# [STATE_ARCHITECTURE]

The domain map of `state` — the W1 host-free fold algebra between the `kernel` value floor and the W2+ consumers (`wire`, `store`, `edge`, `browser`, `ui`). Five sub-domains — `fold`, `crdt`, `causal`, `evidence`, `query` — carry one algebra at two altitudes: browser apps fold wire-decoded events in memory, node apps fold journal events durably through the `store/project` binding of the same algebra. `state` imports `kernel` only and never imports `wire`; wire decodes INTO the vocabulary this folder owns, so the algebra is transport-free and browser-safe by construction.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
state/src/               # imports kernel ONLY; runtime:neutral — no platform binding, no wire import, no transport
├── fold/                # The keyed fold algebra and its replay law
│   ├── algebra.ts       # keyed folds — ONE algebra, two altitudes (browser in-memory, store durable)
│   └── replay.ts        # replay law + the d2ts incremental-dataflow lane
├── crdt/                # The convergent merge algebra and its laws
│   ├── merge.ts         # CRDT op merge semantics — one algebra generic over the op vocabulary; the C#-minted wire family and app-authored journal families are instances
│   └── converge.ts      # convergence laws; fixture hooks into the tests/contracts corpus (TS readers in tests/typescript/_testkit)
├── causal/              # The causality vocabulary and the delivery order
│   ├── vector.ts        # version vectors, commit/branch shapes, Merkle comparison
│   └── order.ts         # happened-before folds + honest-uncertainty windows (kernel/clock); causal delivery buffer; stability frontier (GLB meet), causal finalize, retention-frontier handoff to store/journal/retain
├── evidence/            # The typed evidence vocabulary and its folds
│   ├── receipt.ts       # ReceiptEnvelope-decoded evidence vocabulary — the typed receipt family, never erased
│   ├── availability.ts  # DegradationLevel / CommandAvailability vocabulary the wire gateway gate types against
│   ├── progress.ts      # progress-mark evidence folds
│   └── timeline.ts      # evidence feed/timeline folds
└── query/               # The live and windowed read surfaces
    ├── live.ts          # Subscribable live queries + presence semantics (edge/live serves these)
    └── window.ts        # windowed query folds + the REPLAY_LAW spine; AsOf 3-coordinate time-travel reads + asOfDiff + HLC event-time watermarks
```

The `fold` sub-domain is the spine: `algebra.ts` declares the keyed fold every consumer binds, `replay.ts` the law that a fold rebuilt from any event prefix converges to the live fold and the incremental lane that maintains it without re-folding. Every other sub-domain is either an instance vocabulary the algebra folds — the `crdt` op families, the `evidence` kinds — or a read/order discipline over the folded result — `causal` delivery, frontiers, and finalize, `query` live and windowed reads. Growth is a row: a new evidence kind is an `evidence/` vocabulary row plus one fold arm; a new CRDT type is a `merge.ts` case with its `converge.ts` law.

## [02]-[SEAMS]

```text seams
evidence/availability  ←  csharp:Rasm.AppHost/Observability/Health.cs  # [WIRE]: DegradationLevel / CommandAvailabilityWire
evidence/receipt       ←  csharp:Rasm.AppHost/Runtime/Ports.cs         # [WIRE]: ReceiptEnvelopeWire / HlcStampWire / TenantContextWire
evidence/progress      ←  csharp:Rasm.Compute/Runtime/progress         # [WIRE]: ProgressMarkWire
causal/vector          ←  csharp:Rasm.Persistence/Version/commits      # [SHAPE]: commit/branch/version-vector/Merkle wire shapes
evidence/timeline      ←  csharp:Rasm.AppUi/Render/evidence            # [PROJECTION]: EvidenceFeed / EvidenceTimeline
```

Decode transits the `wire` codecs and lands here as owned vocabulary: the AppHost envelope rows arrive through `wire/codec/envelope`, the progress mark through `wire/codec/progress`, the version-vector shapes through `wire/codec/version`, and the CRDT op family through `wire/codec/crdt` into `crdt/merge` instances — `state` owns every decoded vocabulary and imports `wire` never, so each row's glyph points inward while the import edge points the other way (`wire → state`). The same vocabularies serve the TS-internal consumers: the `wire` gateway command gate types against `evidence/availability`, `store/project` binds `fold/algebra` for the durable altitude, `causal/order` hands the retention frontier to `store/journal`, and `edge/live` serves the `query/live` presence semantics. `Hlc` and `TenantContext` are `kernel` vocabulary — the receipt family and the `query/window` watermarks compose them, never re-mint.
