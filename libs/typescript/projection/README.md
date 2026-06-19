# [PROJECTION]

The file router and external-package registry for `projection`, the host-free read-side fold-algebra owner of the TypeScript branch. The folder folds every decoded `interchange` feed into `SubscriptionRef`-backed keyed maps under one stream policy; it consumes the C# wire only, owns no geometry, dials no transport, and re-mints no shared identity. The domain map is in `ARCHITECTURE.md`, the forward concepts in `IDEAS.md`, the open work in `TASKLOG.md`.

## [1]-[ROUTER]

The design pages, grouped by sub-domain in build order — the fold core every other page composes, then the three read-model engines, then the receipt and confidence projection.

- [fold-core/stream-policy](.planning/fold-core/stream-policy.md): the one `StreamPolicy` and the `withPolicy` decorator every fold pipes its source through.
- [fold-core/keyed-fold](.planning/fold-core/keyed-fold.md): the `foldStream` scalar primitive and the `keyedFold` keyed-map combinator built on it.
- [fold-core/projection](.planning/fold-core/projection.md): the `Projection<A>` `Subscribable` adapter and `derive` — the one atom-bridge read face every store exposes.
- [feed-stores/live-cells](.planning/feed-stores/live-cells.md): the `FeedKind` vocabulary and the `feedStore` entrypoint over the runtime/health/snapshot/progress feed rows.
- [standing-query/watermark](.planning/standing-query/watermark.md): event-time projection, the `Watermark` mark, the `allowedLateness` horizon.
- [standing-query/window-fold](.planning/standing-query/window-fold.md): `WindowKind`/`bucketSet`/`windowFold` Z-set signed-delta IVM over `d2ts`.
- [live-query/reactive-query](.planning/live-query/reactive-query.md): the `LiveQuery`/`queryStore` versionless `d2mini` reactive-query surface folded into a `SubscriptionRef`.
- [convergence/lww-merge](.planning/convergence/lww-merge.md): `opMerge` LWW-by-HLC, tombstone guard, the `ConflictOutcomeKind` ledger.
- [convergence/presence](.planning/convergence/presence.md): the `ConflictPresenceStore` and the ephemeral-TTL presence row.
- [convergence/convergence-law](.planning/convergence/convergence-law.md): the `fast-check` permutation law proving cross-peer strong eventual consistency.
- [causality-graph/version-vector](.planning/causality-graph/version-vector.md): the `VectorOrderKind` partial order and the skew-fused `concurrent-uncertain` verdict.
- [causal-delivery/causal-buffer](.planning/causal-delivery/causal-buffer.md): the `CausalBuffer` dependency-gated release and the `stabilityFrontier` `SortedSet`-cursor meet.
- [temporal-query/as-of-query](.planning/temporal-query/as-of-query.md): the `AsOf` coordinate, the `asOfQuery` `reconstructAt` projection, and the two-coordinate diff.
- [envelope/receipt-envelope](.planning/envelope/receipt-envelope.md): the `ReceiptEnvelopeCarrier` payload-bound `Schema` factory.
- [evidence/evidence-correlation](.planning/evidence/evidence-correlation.md): the `EvidenceProjection` store and content-keyed evidence correlation.
- [availability/availability-gate](.planning/availability/availability-gate.md): the `AvailabilityStore` read gate the gateway dials against.
- [clock-uncertainty/skew-band](.planning/clock-uncertainty/skew-band.md): the `SkewBand` "within +/-N ms" interval and the `bandsOverlap` ordering input.
- [frontier-gc/frontier-gc](.planning/frontier-gc/frontier-gc.md): the `Frontier` antichain and `finalizeBelow` unifying every keyed-map TTL under one horizon.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list. Versions are centralized in the one workspace catalog; new admissions land here from the folder's ideas and tasks.

- `@electric-sql/d2ts`: the multi-dimensional-versioned differential-dataflow operator graph carrying the frontier antichain; the `standing-query` event-time engine and the `frontier-gc` antichain re-found on it.
- `@electric-sql/d2mini`: the versionless browser-resident incremental-query runtime backing the `live-query` reactive-query surface, never the event-time frontier engine.

## [3]-[CROSS_CUTTING]

Branch-level cross-cutting packages consumed by this folder.

- `effect`: `Stream`, `SubscriptionRef`, `Subscribable`, `Schedule`, `Duration`, `Schema`, `Match`, `Data.TaggedEnum`, `HashMap`, `HashSet`, `SortedMap`, `SortedSet`, `RedBlackTree`, `Equivalence`, `Order`, and the `Stream.scan`/`broadcast`/`changes` combinators — the substrate of every fold; the ordered-collection, `Subscribable`, and equality modules carried by the branch `.api/effect.md` coverage extension the ordered-index and projection tasks read.
- `@rasm/interchange`: the decode-owning sibling whose published `.` surface carries every `*Wire` shape, the `ContentKey` brand, `RasmPackage`, and the `ConflictOutcomeKind` literal the fold consumes verbatim; the fold re-validates and re-mints none of it, and imports no `interchange` interior.
- `@effect-atom/atom`: the reactive-store bridge target; each `SubscriptionRef` store binds through `Atom.subscriptionRef`/`Atom.subscribable` at the `ui` boundary, never imported into the fold interior.
- `fast-check`: the algebraic property-testing arbitrary spine; the `Arbitrary`/`property`/`shuffledSubarray` permutation law drives the `convergence-law` harness.
- `@effect/vitest`: the Effect-aware test runner binding the property spine to the scoped fold owners through `it.scoped.prop`.
- `@stryker-mutator/core`: the mutation kill-ratio gate over the convergence and window-fold laws, with its co-admitted `typescript-checker`/`vitest-runner` plugins registered once at the branch.
