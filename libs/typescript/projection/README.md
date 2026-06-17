# [PROJECTION]

The file router and external-package registry for `projection`, the host-free read-side fold-algebra owner of the TypeScript branch. The folder folds every decoded `interchange` feed into `SubscriptionRef`-backed keyed maps under one stream policy; it consumes the C# wire only, owns no geometry, dials no transport, and re-mints no shared identity. The domain map is in `ARCHITECTURE.md`, the forward concepts in `IDEAS.md`, the open work in `TASKLOG.md`.

## [1]-[PAGE_ROUTER]

The design pages, grouped by sub-domain in build order — the fold core every other page composes, then the three read-model engines, then the receipt and confidence projection.

- [fold-core/stream-policy](.planning/fold-core/stream-policy.md): the one `StreamPolicy` and the `withPolicy` decorator every fold pipes its source through.
- [fold-core/keyed-fold](.planning/fold-core/keyed-fold.md): the `foldStream` scalar primitive and the `keyedFold` keyed-map combinator built on it.
- [feed-stores/live-cells](.planning/feed-stores/live-cells.md): the `RuntimeFeed`/`HealthStore`/`SnapshotFeed`/`ProgressStore` live-cell stores.
- [standing-query/watermark](.planning/standing-query/watermark.md): event-time projection, the `Watermark` mark, the `allowedLateness` horizon.
- [standing-query/window-fold](.planning/standing-query/window-fold.md): `WindowKind`/`bucketSet`/`windowFold` Z-set signed-delta IVM.
- [convergence/lww-merge](.planning/convergence/lww-merge.md): `opMerge` LWW-by-HLC, tombstone guard, the `ConflictOutcomeKind` ledger.
- [convergence/presence](.planning/convergence/presence.md): the `ConflictPresenceStore` and the ephemeral-TTL presence row.
- [envelope/receipt-envelope](.planning/envelope/receipt-envelope.md): the `ReceiptEnvelopeCarrier` payload-bound `Schema` factory.
- [evidence/evidence-correlation](.planning/evidence/evidence-correlation.md): `ReceiptStore`/`EvidenceFeed` and content-keyed evidence correlation.
- [availability/availability-gate](.planning/availability/availability-gate.md): the `AvailabilityStore` read gate the gateway dials against.
- [clock-uncertainty/skew-band](.planning/clock-uncertainty/skew-band.md): the `SkewBand` "within +/-N ms" confidence interval.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list. Versions are centralized in the one workspace catalog; new admissions land here from the folder's ideas and tasks.

- `effect`: `Stream`, `SubscriptionRef`, `Schedule`, `Duration`, `Schema`, `Match`, `Data.TaggedEnum`, `HashMap`, and `HashSet` — the substrate of every fold.
- `@effect-atom/atom`: the reactive-store bridge target; each `SubscriptionRef` store binds through `Atom.subscriptionRef`/`Atom.subscribable` at the `ui` boundary, never imported into the fold interior.
- `@electric-sql/d2ts`: the multi-dimensional-versioned differential-dataflow operator graph carrying the frontier antichain; the standing-query and frontier-gc engines re-found on it — planned.
- `@electric-sql/d2mini`: the versionless browser-resident incremental-query runtime for the non-windowed reactive-query surface, never the event-time frontier engine — planned.
- `fast-check`: the algebraic property-testing arbitrary spine; `fc.commands`/`fc.asyncModelRun` drive the convergence law harness.
- `@effect/vitest`: the Effect-aware test runner binding the property spine to the fold owners.
- `@stryker-mutator/core`: the mutation kill-ratio gate over the convergence and window-fold laws.
