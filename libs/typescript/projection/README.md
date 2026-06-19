# [PROJECTION]

`projection` is the host-free read-side fold-algebra owner of the TypeScript branch. It folds every decoded `interchange` feed into `SubscriptionRef`-backed keyed maps under one stream policy, consuming the C# wire only — it owns no geometry, dials no transport, and re-mints no shared identity. The domain map is in `ARCHITECTURE.md`, the forward concepts in `IDEAS.md`, and the open work in `TASKLOG.md`.

## [1]-[ROUTER]

- [1]-[POLICY](.planning/fold/policy.md): the one `StreamPolicy` and the `withPolicy` decorator every fold pipes its source through.
- [2]-[COMBINATORS](.planning/fold/combinators.md): the `foldStream` scalar primitive and the `keyedFold` keyed-map combinator built on it.
- [3]-[PROJECTION](.planning/fold/projection.md): the `Projection<A>` `Subscribable` adapter and `derive` — the atom-bridge read face every store exposes.
- [4]-[WATERMARK](.planning/query/watermark.md): event-time projection, the `Watermark` mark, and the `allowedLateness` horizon.
- [5]-[WINDOW](.planning/query/window.md): `WindowKind`/`bucketSet`/`windowFold` Z-set signed-delta IVM over `d2ts`.
- [6]-[REACTIVE](.planning/query/reactive.md): the `LiveQuery`/`queryStore` versionless `d2mini` reactive-query surface folded into a `SubscriptionRef`.
- [7]-[ASOF](.planning/query/asof.md): the `AsOf` coordinate (Version/EventTime/Stable) and the `asOfQuery` `reconstructAt` projection.
- [8]-[DIFF](.planning/query/diff.md): the `asOfDiff` two-coordinate snapshot diff the conflict inspector reads.
- [9]-[MERGE](.planning/convergence/merge.md): `opMerge` LWW-by-HLC, tombstone guard, and the `ConflictOutcomeKind` ledger.
- [10]-[PRESENCE](.planning/convergence/presence.md): the `ConflictPresenceStore` and the ephemeral-TTL presence row.
- [11]-[LAW](.planning/convergence/law.md): the `fast-check` permutation law proving cross-peer strong eventual consistency.
- [12]-[RETENTION](.planning/convergence/retention.md): the `Frontier` antichain and `finalizeBelow` unifying every keyed-map TTL under one horizon.
- [13]-[VECTOR](.planning/causality/vector.md): the `VectorOrderKind` partial order and the skew-fused `concurrent-uncertain` verdict.
- [14]-[BUFFER](.planning/causality/buffer.md): the `CausalBuffer` dependency-gated release, the `DeliveryVerdict` Held/Released dispatch, and the `causalDelivery` keyed-fold constructor.
- [15]-[FRONTIER](.planning/causality/frontier.md): the `stabilityFrontier` `SortedSet`-of-cursors greatest-lower-bound meet naming the causally-settled horizon.
- [16]-[SKEW](.planning/causality/skew.md): the `SkewBand` "within +/-N ms" interval and the `bandsOverlap` ordering input.
- [17]-[CELLS](.planning/evidence/cells.md): the `FeedKind` vocabulary and the `feedStore` entrypoint over the runtime/health/snapshot/progress feed rows.
- [18]-[ENVELOPE](.planning/evidence/envelope.md): the `ReceiptEnvelopeCarrier` payload-bound `Schema` factory.
- [19]-[CORRELATION](.planning/evidence/correlation.md): the `EvidenceProjection` store and content-keyed evidence correlation.
- [20]-[AVAILABILITY](.planning/evidence/availability.md): the `AvailabilityStore` read gate the gateway dials against.

## [2]-[DOMAIN_PACKAGES]

Domain libraries this folder owns exclusively; versions are centralized in the one workspace catalog and corroborated by the adjacent `.api/` folder.

[DIFFERENTIAL_DATAFLOW]:
- `@electric-sql/d2ts`: the multi-dimensional-versioned differential-dataflow operator graph carrying the frontier antichain; the `query` event-time engine (`watermark`, `window`) and the `convergence/retention` antichain are founded on it.
- `@electric-sql/d2mini`: the versionless browser-resident incremental-query runtime backing the `query/reactive` surface, never the event-time frontier engine.

## [3]-[SUBSTRATE_PACKAGES]

TypeScript substrate libraries this folder consumes; package charters and API evidence live in `libs/typescript/.planning/README.md` and the adjacent `libs/typescript/.api/` folder.

[RUNTIME_CORE]:
- `effect`: `Stream`, `SubscriptionRef`, `Subscribable`, `Schedule`, `Duration`, `Schema`, `Match`, `Data.TaggedEnum`, `HashMap`, `HashSet`, `SortedMap`, `SortedSet`, `RedBlackTree`, `Equivalence`, `Order`, and the `Stream.scan`/`broadcast`/`changes` combinators — the substrate of every fold.

[REACTIVE_BRIDGE]:
- `@effect-atom/atom`: the reactive-store bridge target; each `SubscriptionRef` store binds through `Atom.subscriptionRef`/`Atom.subscribable` at the `ui` boundary, never imported into the fold interior.

[TEST_SUBSTRATE]:
- `fast-check`: the algebraic property-testing arbitrary spine; the `Arbitrary`/`property`/`shuffledSubarray` permutation law drives the `convergence/law` harness.
- `@effect/vitest`: the Effect-aware test runner binding the property spine to the scoped fold owners through `it.scoped.prop`.
- `@stryker-mutator/core`: the mutation kill-ratio gate over the `convergence` and `query/window` laws, with its co-admitted `typescript-checker`/`vitest-runner` plugins registered once at the branch.
