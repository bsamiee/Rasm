# [PROJECTION_IDEAS]

The forward pool of higher-order concepts for the fold-algebra owner. Each open idea is a card — a bracketed slug leader plus the capability, what it unlocks, and the gap or modern technique it draws on — grounded in the read-side domain and the branch's host-free differentiated value. An idea drives one or more `TASKLOG.md` tasks; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition.

## [1]-[OPEN]

[DIFFERENTIAL_DATAFLOW_BACKBONE]: re-found the standing-query engine on a true differential-dataflow operator graph rather than the single hand-rolled `windowFold`.
- A typed pipeline of `map`/`filter`/`keyBy`/`reduce`/`join`/`consolidate` over Z-set `MultiSet` `[value, ±multiplicity]` deltas, with the tumbling/sliding/session bucketing as one reduce stage and the watermark as the frontier antichain.
- Unlocks sub-millisecond incremental live queries over the decoded op-log and snapshot changefeed — filtered evidence views, joined runtime-plus-health projections, grouped progress roll-ups — that update by delta not re-scan, with type-safe pipeline inference and optional SQLite operator-state persistence for resumable large-history folds.
- Draws on `@electric-sql/d2ts` (the multi-dimensional-versioned engine carrying the frontier antichain the event-time watermark maps onto, the load-bearing choice for windowed IVM) and the DBSP Z-set linear-operator theory the existing `signOf`/`windowMerge` signed-delta arithmetic already mirrors; the gap is that `windowFold` owns one aggregation, so any join or distinct or multi-key roll-up forces a parallel hand-rolled fold — the reinvention the dependency policy forbids. `@electric-sql/d2mini` drops versioning and the frontier and serves only the non-windowed reactive-query surface, never the event-time engine.

[FRONTIER_GARBAGE_COLLECTION]: a watermark-driven retention and compaction policy on every keyed map.
- An antichain frontier advances with event time; a bucket, tombstone, or presence row below the frontier minus `allowedLateness` is finalized and evicted, so a long-running browser session's state is bounded rather than monotonically growing.
- Unlocks bounded-memory long-lived projections, a closed late-arrival horizon (a row past the finalized frontier is a hard reject, not an unbounded correction), deterministic replay because the retained state is the frontier-trimmed prefix, and presence-plus-tombstone TTL unified under one frontier rule rather than per-row `expiresAt` scans.
- Draws on the DBSP and timely-dataflow antichain lower-bound concept; the gap is that `windowFold` buckets, the convergence tombstone set, and expired presence rows are never reclaimed, so a multi-hour SPA session leaks memory and the late-row retraction has no closed horizon. Seeds the planned `frontier-gc/` sub-domain.

[CONVERGENCE_PROOF_HARNESS]: elevate the strong-eventual-consistency claim into a first-class algebraic-law artifact.
- A `fast-check` `fc.commands`/`asyncModelRun` model drives two divergent-delivery folds of the same op-set and asserts a byte-identical `ConflictPresenceState`, encoding the commutativity, associativity, and idempotence laws of `opMerge` as the external oracle.
- Unlocks a mechanical convergence guarantee the whole sync surface rests on, a reusable law-matrix any future CRDT op kind must pass before admission, and mutation-killable evidence that delivery-order independence holds.
- Draws on the `testing-ts` algebraic-PBT spine and the LWW-by-HLC CRDT pattern; the gap is that the fold is asserted commutative/associative/idempotent in prose only, so the cross-peer property is unverified and a GraphFork extension lands on an unproven base.

[REACTIVE_QUERY_SURFACE]: a single derived-projection combinator composing any set of base `SubscriptionRef` stores into a new `SubscriptionRef`.
- The combinator folds over the base stores' change streams via `Stream.changes` and the `keyedFold` algebra to express a composite view — a health-gated runtime view, an evidence-correlated-by-`ContentKey` timeline, a watermark-annotated window cell — as one declared derivation.
- Unlocks composite reactive views owned at the right altitude (`projection`, not `ui`), one `Subscribable` per derived concept the `@effect-atom/atom` bridge consumes directly, no view-state library in `ui`, and derivations that are themselves incremental because they fold over base change streams.
- Draws on Effect `SubscriptionRef.changes`/`Subscribable`; the gap is that consumers recombine raw stores manually today, leaking recombination logic into `ui` as a parallel view-state pattern — the named branch prohibition.

[SKEW_AWARE_ORDERING]: promote `SkewBand` from a render-only projection into an ordering input.
- When two evidence or op rows are closer in midpoint than the sum of their `radiusMs` the fold marks them concurrent-uncertain rather than forcing a spurious HLC total order, surfacing a "causally-ambiguous within +/-N ms" band on the evidence timeline and the convergence adjudication.
- Unlocks honest uncertainty in the evidence timeline and conflict adjudication (concurrent-uncertain versus definitively-ordered), a distributed-systems-correct read model the desktop AppUi cannot produce, and a `SkewBand` that is load-bearing in both `clock-uncertainty` and `convergence` rather than a leaf output.
- Draws on HLC clock-skew confidence-interval surfacing; the gap is that the fold forces a strict HLC total order even when physical-clock skew makes two timestamps statistically indistinguishable, so the UI claims false precision about event order. Deepens the thin `clock-uncertainty/` sub-domain.

## [2]-[CLOSED]

None.
