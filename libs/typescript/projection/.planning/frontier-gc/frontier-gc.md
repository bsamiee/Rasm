# [PROJECTION_FRONTIER_GC]

The watermark-driven retention and compaction rule across every keyed map — `Frontier` is the event-time antichain that advances with the `standing-query/watermark#WATERMARK` mark, `finalizeBelow` evicts any window bucket, convergence tombstone, or presence row whose event time is below the frontier minus `allowedLateness`, and `frontierGc` is the one fold that unifies the per-row `expiresAt` scans `convergence/presence#PRESENCE` and `convergence/lww-merge#LWW_MERGE` carry today under one closed late-arrival horizon. A long-lived browser session stays bounded-memory because the retained state is the frontier-trimmed prefix; replay is deterministic because the trimmed prefix is the same on every peer; a row arriving past the finalized frontier is a hard reject, not an unbounded correction. The frontier advances over the same `eventNanos` projection the window engine folds; the GC mints no clock and trims only what the watermark proves finalized.

## [1]-[INDEX]

One cluster: `[2]-[FRONTIER_GC]` owns `Frontier`, `advanceFrontier`, `finalizeBelow`, the `Reclaimable` retention vocabulary, and the `frontierGc` compaction fold.

## [2]-[FRONTIER_GC]

- Owner: `Frontier`, the event-time antichain lower bound carrying the finalized `eventNanos` horizon; `advanceFrontier`, the monotone lift that raises the horizon to the watermark `eventNanos` minus the `allowedLateness` span and never regresses; `Reclaimable`, the closed `Data.TaggedEnum` retention vocabulary keying the three reclaimable kinds (`WindowBucket`, `Tombstone`, `PresenceRow`) to their own event-time projection; `finalizeBelow`, the predicate that admits a row for eviction exactly when its event time is strictly below the finalized frontier; and `frontierGc`, the `keyed-fold#KEYED_FOLD` fold that advances the frontier per arrived watermark and trims every keyed map's below-frontier cells in one pass.
- Cases: `advanceFrontier` reads the watermark `eventNanos`, subtracts the `allowedLateness` nanos, and lifts the held frontier only upward so a late watermark never reopens a finalized horizon; `finalizeBelow` is the one test all three reclaimable kinds share — a `WindowBucket` finalizes when its bucket start is below the frontier, a `Tombstone` finalizes when its delete event time is below the frontier (the key can never re-live past the horizon so the tombstone is reclaimable), and a `PresenceRow` finalizes when its `expiresAt` is below the frontier (subsuming the per-row TTL scan); a row arriving with event time below the already-finalized frontier is rejected as a hard late-arrival, distinct from the `watermark#WATERMARK` `late` tally which tracks corrections still inside the horizon.
- Packages: `effect` for `Data.TaggedEnum`, `Duration`, `HashMap`, `Option`, `Stream`, `SubscriptionRef`, `Effect`, and `Scope`; `@electric-sql/d2ts` `Antichain`/`sendFrontier` carry the frontier once the `window-fold#WINDOW_FOLD` dataflow re-founding lands, `effect` `HashMap` carrying the trimmed state until then.
- Growth: a new reclaimable kind lands as one `Reclaimable` variant breaking the `finalizeBelow` `$match` at compile time; the `d2ts` `Antichain` frontier replaces the scalar `eventNanos` horizon as one `Frontier` representation swap once the windowed engine carries it, the `finalizeBelow` predicate unchanged.
- Boundary: the GC trims only below-frontier state the watermark proves finalized and never an in-horizon row, so a late correction inside `allowedLateness` is never evicted; the frontier reads the `watermark#WATERMARK` `eventNanos` and `defaultLateness`, never re-deriving event time; the trim is `HashMap.filter` over the persistent structure so the retained prefix shares structure under advance, and the three reclaimable kinds fold into the one `Reclaimable` map rather than three parallel TTL scans; the presence and tombstone TTL unify here rather than per-row `expiresAt` scans in their owning folds; the domain dials no transport.

```ts contract
import { Data, Duration, Effect, HashMap, Option, Scope, Stream, SubscriptionRef } from "effect";
import { keyedFold } from "../fold-core/keyed-fold";
import type { StreamPolicy } from "../fold-core/stream-policy";
import { type Watermark } from "../standing-query/watermark";

interface Frontier {
  readonly finalizedNanos: bigint;
}
const frontierZero: Frontier = { finalizedNanos: 0n };

const advanceFrontier = (frontier: Frontier, mark: Watermark, allowed: Duration.Duration): Frontier => {
  const horizon = mark.eventNanos - Duration.unsafeToNanos(allowed);
  return { finalizedNanos: horizon > frontier.finalizedNanos ? horizon : frontier.finalizedNanos };
};

type Reclaimable = Data.TaggedEnum<{
  readonly WindowBucket: { readonly bucketStart: bigint };
  readonly Tombstone: { readonly deletedAt: bigint };
  readonly PresenceRow: { readonly expiresAtNanos: bigint };
}>;
const Reclaimable = Data.taggedEnum<Reclaimable>();

const eventTimeOf = (row: Reclaimable): bigint =>
  Reclaimable.$match(row, {
    WindowBucket: ({ bucketStart }) => bucketStart,
    Tombstone: ({ deletedAt }) => deletedAt,
    PresenceRow: ({ expiresAtNanos }) => expiresAtNanos,
  });

const finalizeBelow = (frontier: Frontier, row: Reclaimable): boolean => eventTimeOf(row) < frontier.finalizedNanos;

interface GcState<K> {
  readonly frontier: Frontier;
  readonly retained: HashMap.HashMap<K, Reclaimable>;
}

const trim = <K>(state: GcState<K>): HashMap.HashMap<K, Reclaimable> =>
  HashMap.filter(state.retained, (row) => !finalizeBelow(state.frontier, row));

const frontierGc = <K>(
  marks: Stream.Stream<{ readonly key: K; readonly row: Reclaimable; readonly mark: Watermark }>,
  allowed: Duration.Duration,
  policy: StreamPolicy,
): Effect.Effect<SubscriptionRef.SubscriptionRef<HashMap.HashMap<K, GcState<K>>>, never, Scope.Scope> =>
  keyedFold(
    marks,
    (m) => m.key,
    (prior, m) =>
      Option.match(prior, {
        onNone: () => ({ frontier: advanceFrontier(frontierZero, m.mark, allowed), retained: HashMap.make([m.key, m.row]) }),
        onSome: (state) => {
          const advanced = { frontier: advanceFrontier(state.frontier, m.mark, allowed), retained: HashMap.set(state.retained, m.key, m.row) };
          return { ...advanced, retained: trim(advanced) };
        },
      }),
    policy,
  );
```

## [3]-[RESEARCH]

- [FRONTIER_REPRESENTATION]: the scalar `finalizedNanos` horizon is the bounded-memory representation until the `window-fold#WINDOW_FOLD` differential-dataflow re-founding lands the `@electric-sql/d2ts` `Antichain` frontier; the multi-dimensional antichain replaces the scalar horizon as one `Frontier` representation swap, the `finalizeBelow` predicate reading `Antichain` dominance through the version-vector `causality-graph/version-vector#VERSION_VECTOR` `dominates` algebra rather than a scalar `<` — the `d2ts` catalog admission already landed, so this is blocked on the dataflow re-founding alone [BLOCKED on `window-fold#WINDOW_FOLD` d2ts re-founding].
