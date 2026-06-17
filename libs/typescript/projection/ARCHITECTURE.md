# [PROJECTION_ARCHITECTURE]

The professional-domain map of `projection`, the read-side fold-algebra owner of the TypeScript branch. The sub-domain folders mirror the eventual source tree, each named by its real domain concept with a one-line charter in the codemap; planned-but-empty sub-domains are visible gaps that fuel the ideas and tasks. Dependency direction lives once in the branch `ARCHITECTURE.md`; boundaries and wires live on the tasks that build them.

## [1]-[DOMAIN_MAP]

`fold-core` is the foundation every other sub-domain composes. The next three carry three distinct read-model theories — `feed-stores` is reactive-store algebra, `standing-query` is differential-dataflow incremental-view-maintenance, `convergence` is strong-eventual-consistency CRDT — and the receipt-and-confidence projection spreads across `envelope`, `evidence`, `availability`, and `clock-uncertainty`. Each charter below is the load-bearing distinction the codemap comment cannot carry; the rest is the tree.

```text codemap
projection/
├── fold-core/                  # StreamPolicy + the foldStream/keyedFold combinators every fold composes
│   ├── stream-policy.md        #   StreamPolicy + withPolicy — the bounded reconnect/back-pressure vocabulary
│   └── keyed-fold.md           #   foldStream scalar primitive + keyedFold keyed-map combinator
├── feed-stores/                # live-cell stream stores keyed by the verbatim C# discriminant
│   └── live-cells.md           #   RuntimeFeed/HealthStore/SnapshotFeed/ProgressStore — latest receipt per slot
├── standing-query/             # event-time windowing + incremental-view-maintenance
│   ├── watermark.md            #   eventNanos projection, the Watermark mark, allowedLateness horizon
│   └── window-fold.md          #   WindowKind/bucketSet/signOf/windowFold — Z-set signed-delta IVM
├── convergence/                # strong-eventual-consistency CRDT fold over the sync op-log
│   ├── lww-merge.md            #   opMerge LWW-by-HLC, tombstone guard, ConflictOutcomeKind ledger
│   └── presence.md             #   ConflictPresenceStore + ephemeral-TTL presence row
├── envelope/                   # the receipt envelope binding every structured-text payload
│   └── receipt-envelope.md     #   ReceiptEnvelopeCarrier — the one payload-bound Schema factory
├── evidence/                   # the receipt + evidence projection, content-keyed correlation
│   └── evidence-correlation.md #   ReceiptStore/EvidenceFeed + correlation on the assembled ContentKey
├── availability/               # the command-availability read gate the gateway dials against
│   └── availability-gate.md    #   AvailabilityStore + isEnabled — disabled command never fires
├── clock-uncertainty/          # the HLC skew-band confidence projection as product UI
│   └── skew-band.md            #   SkewBand — { midpointMs, radiusMs } "within +/-N ms" confidence interval
└── frontier-gc/                # (planned) watermark-driven retention/compaction across every keyed map
```

## [2]-[CHARTERS]

- `convergence` and `presence` ride one `ConflictPresenceStore`: the durable LWW-converged op-log state and the ephemeral-TTL presence row are distinct in kind, folded separately, and a presence op never mutates the live geometry cell.
- `evidence` consumes the `interchange`-assembled `ArtifactBlob` and 16-byte `ContentKey` and correlates evidence by digest; a second hash mint here is the named cross-language drift defect.
- `availability` is a fold at the same altitude as the stream stores; the `interchange` `CommandGateway` reads `isEnabled` at dial time across the boundary, and co-location follows altitude not read-dependency, so the gateway stays in `interchange`.
- `clock-uncertainty` is the distributed-systems read model the C# desktop AppUi structurally cannot own — honest interval confidence about event timing the host face has no read model to produce.
- `frontier-gc` (planned): an antichain frontier advances with event time and finalizes-and-evicts any window cell, tombstone, or presence row below the frontier minus `allowedLateness`, so a long-lived browser session stays bounded-memory with a closed late-arrival horizon. It unifies the per-row TTL scans that `convergence` and `presence` carry today under one frontier rule.
