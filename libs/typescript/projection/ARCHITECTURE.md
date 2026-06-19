# [PROJECTION_ARCHITECTURE]

The professional-domain map of `projection`, the read-side fold-algebra owner of the TypeScript branch. The sub-domain folders mirror the eventual source tree, each named by its real domain concept with a one-line charter in the codemap; planned-but-empty sub-domains are visible gaps that fuel the ideas and tasks. Dependency direction lives once in the branch `ARCHITECTURE.md`; boundaries and wires live on the tasks that build them.

## [1]-[DOMAIN_MAP]

`fold-core` is the foundation every other sub-domain composes. `feed-stores` is reactive-store algebra, `standing-query` is event-time differential-dataflow IVM over the versioned `d2ts` engine, `live-query` is the disjoint versionless reactive-query surface over `d2mini`, `convergence` is strong-eventual-consistency CRDT, `causality-graph` is the concurrency-detection read model the HLC total order erases, and `causal-delivery` is the causal-ordering discipline that buffers an op until its dependencies arrive and names the causally-stable horizon; `temporal-query` is the point-in-time as-of read axis over the version and event-time coordinates; the receipt-and-confidence projection spreads across `envelope`, `evidence`, `availability`, and `clock-uncertainty`, and `frontier-gc` unifies retention under one frontier. Each charter below is the load-bearing distinction the codemap comment cannot carry; the rest is the tree.

```text codemap
projection/
├── fold-core/                  # StreamPolicy + the foldStream/keyedFold combinators + the Subscribable projection face
│   ├── stream-policy.md        #   StreamPolicy + withPolicy — the bounded reconnect/back-pressure vocabulary
│   ├── keyed-fold.md           #   foldStream scalar primitive + keyedFold keyed-map combinator
│   └── projection.md           #   Projection<A> Subscribable adapter + derive — the one atom-bridge read face every store exposes
├── feed-stores/                # live-cell stream stores keyed by the verbatim C# discriminant
│   └── live-cells.md           #   FeedKind vocabulary + feedStore — latest receipt per slot per feed row
├── standing-query/             # event-time windowing + incremental-view-maintenance over d2ts
│   ├── watermark.md            #   eventNanos projection, the Watermark mark, allowedLateness horizon
│   └── window-fold.md          #   WindowKind/bucketSet/signTable/windowFold — Z-set signed-delta IVM
├── live-query/                 # versionless reactive-query surface over d2mini, disjoint from the d2ts engine
│   └── reactive-query.md       #   LiveQuery + queryStore — d2mini MultiSet pipeline folded into a SubscriptionRef
├── convergence/                # strong-eventual-consistency CRDT fold over the sync op-log
│   ├── lww-merge.md            #   opMerge LWW-by-HLC, tombstone guard, ConflictOutcomeKind ledger
│   ├── presence.md             #   ConflictPresenceStore + ephemeral-TTL presence row
│   └── convergence-law.md      #   the fast-check permutation law proving cross-peer SEC
├── causality-graph/            # the version-vector concurrency-detection read model the HLC total order erases
│   └── version-vector.md       #   VectorOrderKind partial order + skew-fused concurrent-uncertain verdict
├── causal-delivery/            # the causal-ordering discipline: buffer-until-dependencies + the causal-stability frontier
│   └── causal-buffer.md        #   CausalBuffer dependency-gated release + stabilityFrontier SortedSet cursor meet
├── temporal-query/             # the point-in-time as-of read axis over the version and event-time coordinates
│   └── as-of-query.md          #   AsOf coordinate (Version/EventTime/Stable) + asOfQuery reconstructAt projection + diff
├── envelope/                   # the receipt envelope binding every structured-text payload
│   └── receipt-envelope.md     #   ReceiptEnvelopeCarrier — the one payload-bound Schema factory
├── evidence/                   # the receipt + evidence projection, content-keyed correlation
│   └── evidence-correlation.md #   EvidenceProjection store + correlation on the assembled ContentKey
├── availability/               # the command-availability read gate the gateway dials against
│   └── availability-gate.md    #   AvailabilityStore + isEnabled — disabled command never fires
├── clock-uncertainty/          # the HLC skew-band confidence projection, load-bearing in render and ordering
│   └── skew-band.md            #   SkewBand — { midpointMs, radiusMs } interval + bandsOverlap ordering input
└── frontier-gc/                # watermark-driven retention/compaction unifying every keyed-map TTL
    └── frontier-gc.md          #   Frontier antichain + finalizeBelow — bounded-memory closed late-arrival horizon
```

## [2]-[CHARTERS]

- `convergence` and `presence` ride one `ConflictPresenceStore`: the durable LWW-converged op-log state and the ephemeral-TTL presence row are distinct in kind, folded separately, and a presence op never mutates the live geometry cell; `convergence-law` proves the fold's delivery-order independence as mutation-killable evidence.
- `causality-graph` is the read model the C# stamp surrenders: the HLC keeps O(1) width by erasing concurrency into a total order, so the version-vector slot map a browser peer reconstructs off the wire is the only place `Concurrent` is recoverable, fused with the `clock-uncertainty` band into a `concurrent-uncertain` ordering input the desktop AppUi cannot produce.
- `causal-delivery` is the discipline the verdict alone cannot enforce: `causality-graph` tags each op's order but applies it immediately, so an out-of-causal-order arrival paints a state no peer ever held; `causal-delivery` buffers an op until its `context` version vector is dominated by the held cursor and releases the dependency-satisfied suffix in causal order, and the `SortedSet`-cursor meet names the causally-stable horizon `frontier-gc` finalizes against and the Merkle reconciliation treats as the stable prefix unit — one discipline over the `version-vector` `dominates` algebra, never a second ordering surface.
- `temporal-query` is the as-of read axis the always-latest fold lacks: a read-model tier answers "the state as of version V / event time T / the stable frontier", so the `AsOf` coordinate discriminates one query surface that materializes the keyed map through the `d2ts` `Index.reconstructAt` version-trace for the windowed path and the watermark replay-to-mark fold for the versionless path, plus the two-coordinate diff the conflict inspector reads — never a second store holding a parallel history.
- `live-query` is the versionless reactive-query surface over `d2mini`, disjoint by frontier requirement from the `standing-query` event-time engine over `d2ts`: a fully-arrived or order-insensitive composite view folds here, and the windowed event-time IVM never re-founds here.
- `evidence` consumes the `interchange`-assembled `ArtifactBlob` and 16-byte `ContentKey` and correlates evidence by digest; a second hash mint here is the named cross-language drift defect.
- `availability` is a fold at the same altitude as the stream stores; the `interchange` `CommandGateway` reads `isEnabled` at dial time across the boundary, and co-location follows altitude not read-dependency, so the gateway stays in `interchange`.
- `clock-uncertainty` is the distributed-systems read model the C# desktop AppUi structurally cannot own — honest interval confidence about event timing the host face has no read model to produce, load-bearing in both render and the `causality-graph` ordering input.
- `frontier-gc`: an antichain frontier advances with event time and finalizes-and-evicts any window cell, tombstone, or presence row below the frontier minus `allowedLateness`, so a long-lived browser session stays bounded-memory with a closed late-arrival horizon. It unifies the per-row TTL scans that `convergence` and `presence` carry today under one frontier rule.
