# [PROJECTION_ARCHITECTURE]

The domain map of `projection` — the read-side fold-algebra owner of the TypeScript branch. The `fold`, `query`, `convergence`, `causality`, and `evidence` sub-domains fold the decoded `interchange` shapes into keyed read models.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
projection/
├── fold/                # Fold foundation every other sub-domain composes
│   ├── policy.ts        # StreamPolicy + withPolicy — bounded reconnect/back-pressure vocabulary
│   ├── combinators.ts   # foldStream scalar primitive + keyedFold keyed-map combinator
│   └── projection.ts    # Projection<A> Subscribable adapter + derive, one atom-bridge read face
├── query/               # Read-model engine tier: event-time IVM, versionless reactive, as-of
│   ├── watermark.ts     # eventNanos projection, Watermark mark, allowedLateness horizon
│   ├── window.ts        # WindowKind/bucketSet/signTable — Z-set signed-delta IVM over d2ts
│   ├── reactive.ts      # LiveQuery + queryStore — d2mini MultiSet pipeline into a SubscriptionRef
│   ├── asof.ts          # AsOf coordinate + asOfQuery reconstructAt projection
│   └── diff.ts          # asOfDiff — two-coordinate snapshot diff the conflict inspector reads
├── convergence/         # Strong-eventual-consistency CRDT fold + watermark-driven retention
│   ├── merge.ts         # opMerge LWW-by-HLC, tombstone guard, ConflictOutcomeKind ledger
│   ├── presence.ts      # ConflictPresenceStore + ephemeral-TTL presence row
│   ├── law.ts           # Fast-check permutation law proving cross-peer SEC
│   └── retention.ts     # Frontier antichain + finalizeBelow — bounded-memory late-arrival horizon
├── causality/           # Concurrency-detection + causal-delivery + clock confidence
│   ├── vector.ts        # VectorOrderKind partial order + skew-fused concurrent-uncertain verdict
│   ├── buffer.ts        # CausalBuffer dependency-gated release + DeliveryVerdict + causalDelivery
│   ├── frontier.ts      # stabilityFrontier SortedSet-of-cursors greatest-lower-bound meet
│   └── skew.ts          # SkewBand interval + bandsOverlap ordering input
└── evidence/            # Receipt + confidence projection: cells, envelope, correlation, availability
    ├── cells.ts         # FeedKind vocabulary + feedStore — latest receipt per slot per feed
    ├── envelope.ts      # ReceiptEnvelopeCarrier — one payload-bound Schema factory
    ├── correlation.ts   # EvidenceProjection store + correlation on assembled ContentKey
    └── availability.ts  # AvailabilityStore + isEnabled — disabled command never fires
```

`fold` is the foundation every sub-domain composes; `query` is the read-model engine tier (event-time IVM, versionless reactive, as-of); `convergence` is the SEC CRDT plus retention; `causality` unifies concurrency detection, causal delivery, and clock confidence; `evidence` is the receipt-and-confidence projection.

## [2]-[SEAMS]

```text seams
*                      ←  csharp:Rasm.Persistence      # [WIRE]: ElementSet stable receipt algebra
evidence/*             ←  csharp:Rasm.AppHost          # [WIRE]: DegradationLevel / CommandAvailabilityWire
evidence/availability  ←  csharp:Rasm.AppUi/Shell      # [WIRE]: CommandAvailabilityWire rows
evidence/cells         ←  csharp:Rasm.Compute/Runtime  # [WIRE]: ProgressMarkWire
evidence               ←  csharp:Rasm.AppHost/Runtime  # [PROJECTION]: PhaseReceipt / BootMarker / FaultRecord
evidence/cells         ←  csharp:Rasm.AppHost/Runtime  # [PROJECTION]: RuntimeFeed PhaseReceiptWire / BootMarkerWire / FaultRecordWire / DrainReceiptWire
evidence/correlation   ←  csharp:Rasm.AppUi/Render     # [PROJECTION]: EvidenceFeed / EvidenceRowWire
evidence               ←  csharp:Rasm.AppHost/Wire     # [RECEIPT]: ModalityReceipt / RosterReceipt via envelope
evidence/correlation   ←  csharp:Rasm.Compute/Runtime  # [RECEIPT]: ComputeReceiptWire / ContentKey
evidence/availability  →  typescript:ui/interaction    # [PORT]: AvailabilityStore.isEnabled dial-time gate
fold/policy            ⇄  typescript:platform/config   # [PORT]: StreamPolicy reconnect reuse
causality/vector       →  typescript:ui/overlay        # [WIRE]: CrdtField.EphemeralMap beat/leave over HlcWire
```

## [3]-[CHARTERS]

- `convergence/merge` and `convergence/presence` ride one `ConflictPresenceStore`: the durable LWW-converged op-log state and the ephemeral-TTL presence row are distinct in kind, folded separately, and a presence op never mutates the live geometry cell; `convergence/law` proves the fold's delivery-order independence as mutation-killable evidence.
- `causality/vector` is the read model the C# stamp surrenders: the HLC keeps O(1) width by erasing concurrency into a total order, so the version-vector slot map a browser peer reconstructs off the wire is the only place `Concurrent` is recoverable, fused with the `causality/skew` band into a `concurrent-uncertain` ordering input the desktop AppUi cannot produce.
- `causality` unifies concurrency-detection, causal-delivery, and clock confidence under one folder because they share the `version-vector` `dominates` algebra: `causality/vector` tags each op's order but applies it immediately, so an out-of-causal-order arrival paints a state no peer ever held; `causality/buffer` buffers an op until its `context` version vector is dominated by the held cursor and releases the dependency-satisfied suffix in causal order (`CausalBuffer`, the `DeliveryVerdict` Held/Released dispatch, `causalDelivery`); `causality/frontier` owns the `SortedSet`-of-cursors greatest-lower-bound meet that names the causally-stable horizon the buffer settles to, that `convergence/retention` finalizes against, and that the Merkle reconciliation treats as the stable prefix unit; `causality/skew` fuses into the `vector` concurrent-uncertain verdict — one discipline, never a second ordering surface.
- `query` is the read-model engine tier the always-latest fold lacks. The as-of axis answers "the state as of version V / event time T / the stable frontier", so the `AsOf` coordinate discriminates one query surface that materializes the keyed map through the `d2ts` `Index.reconstructAt` version-trace for the windowed path and the watermark replay-to-mark fold for the versionless path — `query/asof` owns the `AsOf` coordinate and the `asOfQuery` projection (the Version coordinate reading the `d2ts` retained trace the `query/window` dataflow re-founding produces), and `query/diff` owns the two-coordinate diff the conflict inspector reads — never a second store holding a parallel history. `query/reactive` is the versionless reactive-query surface over `d2mini`, disjoint by frontier requirement from the `query/watermark`+`query/window` event-time engine over `d2ts`: a fully-arrived or order-insensitive composite view folds here, and the windowed event-time IVM never re-founds here.
- `evidence/correlation` consumes the `interchange`-assembled `ArtifactBlob` and 16-byte `ContentKey` and correlates evidence by digest; a second hash mint here is the named cross-language drift defect. `evidence/availability` is a fold at the same altitude as the stream stores; the `interchange` `CommandGateway` reads `isEnabled` at dial time across the boundary, and co-location follows altitude not read-dependency, so the gateway stays in `interchange`.
- `causality/skew` is the distributed-systems read model the C# desktop AppUi structurally cannot own — honest interval confidence about event timing the host face has no read model to produce, load-bearing in both render and the `causality/vector` ordering input.
- `convergence/retention` sits in `convergence` because its primary job is GC of converged tombstones and presence rows: an antichain frontier advances with event time and finalizes-and-evicts any tombstone, presence row, or `query/window` cell below the frontier minus `allowedLateness`, so a long-lived browser session stays bounded-memory with a closed late-arrival horizon. It also finalizes against the `causality/frontier` stability horizon — one retention rule across that cross-cut, not a split surface, unifying the per-row TTL scans `convergence/merge` and `convergence/presence` carry today.
