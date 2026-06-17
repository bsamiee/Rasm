# [PROJECTION_TASKLOG]

Open and closed work distilled from `IDEAS.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and three to four bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

## [1]-[OPEN]

[QUEUED] Convergence law harness proving cross-peer strong eventual consistency.
- Build the `fc.commands`/`asyncModelRun` model that drives two divergent-delivery folds of the same op-set through `convergence/lww-merge#LWW_MERGE` `opMerge` and asserts a byte-identical `ConflictPresenceState`, encoding commutativity, associativity, and idempotence as the external oracle.
- Integrate `fast-check`, `@effect/vitest`, and `@stryker-mutator/core` so the law is mutation-killable rather than example-only.
- Internal to `convergence/`; the op-log union and `ContentKey` brand arrive from the `interchange` wire and are folded as settled vocabulary, never re-minted. From CONVERGENCE_PROOF_HARNESS.
- Flips the convergence claim from prose-by-construction to mechanically verified; the harness is the gate any future CRDT op kind passes before admission.

[QUEUED] Standing-query live-changefeed replay confirmation.
- Confirm `standing-query/window-fold#WINDOW_FOLD` tumbling/sliding/session bucketing and the `windowMerge` late-row signed-delta retraction-plus-restate against a running op-log replay with real out-of-order arrival, asserting the sliding signed-delta maintenance and the watermark `late` tally hold.
- Integrate `@effect/vitest` and `fast-check` for the out-of-order arrival arbitraries; the Z-set arithmetic grounds on the DBSP retraction law.
- Internal to `standing-query/`; consumes the decoded `OpLogEntryWire` changefeed projected at `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` through the wire fields only — the C# standing-query owner is a peer engine, not a consumer dependency, and no `IQueryable` shape crosses. From DIFFERENTIAL_DATAFLOW_BACKBONE.
- Validates the hand-rolled fold before the dataflow re-founding lands on top of it; the silent drop must never occur, only the tracked correction.

[QUEUED] Differential-dataflow re-founding of the standing-query engine.
- Re-found `standing-query/window-fold#WINDOW_FOLD` on a typed differential-dataflow operator graph — `map`/`filter`/`keyBy`/`reduce`/`join`/`consolidate` over Z-set `MultiSet` deltas — keeping `WindowKind`/`bucketSet` as the windowing reduce stage on top of the dataflow primitives and the watermark as the frontier antichain.
- Integrate `@electric-sql/d2ts` for the versioned operator graph whose frontier antichain the event-time watermark maps onto; `@electric-sql/d2mini` is reserved for the versionless non-windowed incremental-query path and is not imported into the windowed engine. Both require a workspace-catalog admission before import (the package registry lists them planned, the version pin lands in the root `pnpm-workspace.yaml` catalog).
- Internal to `standing-query/`; the engine owns windowing semantics on top of `d2ts` and never reinvents the dataflow itself, mapping `WindowKind`/`bucketSet` onto a reduce stage and the watermark onto the `d2ts` frontier. From DIFFERENTIAL_DATAFLOW_BACKBONE.
- Unlocks joined and grouped incremental live queries over the changefeed; precondition is the catalog admission of `d2ts`, and a join or distinct over the decoded op-log must hold the Z-set retraction law the current `signOf` arithmetic already encodes.

[QUEUED] Frontier garbage collection across every keyed map.
- Build the `frontier-gc/` sub-domain — an antichain frontier that advances with event time and finalizes-and-evicts any window bucket, convergence tombstone, or presence row below the frontier minus `allowedLateness`, so long-lived sessions stay bounded-memory.
- Integrate `@electric-sql/d2ts` `Antichain`/`sendFrontier` once the dataflow re-founding lands; `effect` `HashMap`/`HashSet` carry the trimmed state until then.
- Spans `standing-query/`, `convergence/lww-merge#LWW_MERGE` (tombstone set), and `convergence/presence#PRESENCE` (TTL rows); unifies presence and tombstone TTL under one frontier rule rather than per-row `expiresAt` scans. From FRONTIER_GARBAGE_COLLECTION.
- Closes the late-arrival horizon (a row past the finalized frontier is a hard reject) and makes replay deterministic; depends on the standing-query replay confirmation and benefits from the dataflow re-founding.

[QUEUED] Derived reactive-query combinator.
- Build a single combinator in `fold-core/` that composes any set of base `SubscriptionRef` stores into a new `SubscriptionRef` via `Stream.changes` and the `keyedFold` algebra, expressing composite views (health-gated runtime, evidence-correlated-by-`ContentKey` timeline, watermark-annotated window cell) as one declared derivation.
- Integrate `effect` `SubscriptionRef.changes`/`Subscribable`; the `@effect-atom/atom` bridge consumes each derived `Subscribable` at the `ui` boundary, never imported here.
- Internal to `fold-core/`, composed by `feed-stores/`, `convergence/`, and `evidence/`; keeps recombination at the `projection` altitude so `ui` carries no view-state library — the named branch prohibition. From REACTIVE_QUERY_SURFACE.
- The derivations are themselves incremental because they fold over base change streams; the combinator never grows per composite view.

[QUEUED] Skew-aware ordering feeding convergence and the evidence timeline.
- Promote `clock-uncertainty/skew-band#SKEW_BAND` from a render-only projection into an ordering input — two rows whose midpoint distance is within the summed `radiusMs` are marked concurrent-uncertain rather than forced into a spurious HLC total order, surfacing a "causally-ambiguous within +/-N ms" band.
- Integrate `effect` `Schema`/`Match`; the HLC stamps arrive on the evidence and op-log wire rows already decode-admitted.
- Spans `clock-uncertainty/` and `convergence/lww-merge#LWW_MERGE` (the adjudication weighs concurrent-uncertain distinctly); makes `SkewBand` load-bearing in both rather than a leaf output. From SKEW_AWARE_ORDERING.
- Produces honest uncertainty the desktop AppUi cannot; an HLC drift or counter-overflow fault routes through the fault rail, never swallowed.

[BLOCKED] GraphFork CRDT op vocabulary on the convergence fold.
- Carry the GraphFork CRDT op kind on `convergence/lww-merge#LWW_MERGE` `opMerge` as one additional arm under the same LWW-by-HLC and tombstone laws once the op kind lands on the wire.
- Integrate `effect` `Match` for the new total-dispatch arm; the convergence law harness gates admission.
- Blocked on the upstream op vocabulary landing on the decoded op-log wire at `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` — the wire carries no GraphFork op today, and the fold authors no op vocabulary the wire does not carry. From CONVERGENCE_PROOF_HARNESS.
- Lands only after CONVERGENCE_PROOF_HARNESS so the new arm passes the reusable law-matrix before admission.

## [2]-[CLOSED]

None.
