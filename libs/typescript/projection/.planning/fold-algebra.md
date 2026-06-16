# [PROJECTION_FOLD_ALGEBRA]

One page owns the unified key-discriminated transport-free fold algebra and the five live-cell stream stores built on it — the one `StreamPolicy` (a single `Schedule` reconnect plus a `Stream`-operator vocabulary every fold composes), the `keyedFold` combinator that collapses a `Stream` or receipt sequence into an immutable `SubscriptionRef`-backed keyed map, and the five stores `RuntimeFeed`/`HealthStore`/`SnapshotFeed`/`ProgressStore`/`ConflictPresenceStore`. The discriminant that drives every fold is the C# discriminant consumed verbatim, so the state layer is a projection of the wire vocabulary, never a parallel model. The domain is platform-neutral and transport-free: it depends only on the `@rasm/interchange` decoded shapes and dials nothing.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                            |
| :-----: | :------------ | :--------------------------------------------------------------- |
|   [1]   | FOLD_ALGEBRA  | the StreamPolicy, the keyedFold combinator, and the five stores   |
|   [2]   | RESEARCH      | the standing-query window vocabulary horizon                      |

## [2]-[FOLD_ALGEBRA]

- Owner: `StreamPolicy`, the one bounded reconnect-and-operator vocabulary; `keyedFold`, the single combinator that folds a `Stream`/receipt sequence into an immutable keyed map exposed as a `SubscriptionRef`; and the five live-cell stores — `RuntimeFeed` for the lifecycle fold (surfaced through the web domain), `HealthStore` for the health and degradation fold, `SnapshotFeed` for the snapshot fold, `ProgressStore` for the progress cells, and `ConflictPresenceStore` for the conflicts and presence fold. Every store is one `keyedFold` row over its event kinds, never a parallel store implementation.
- Cases: `RuntimeFeed` folds phases, drain receipts, boot markers, and fault records keyed by phase against `lifecycle-and-drain.md#TS_PROJECTION`; `HealthStore` folds the health snapshot and degradation level and gates which web surfaces are reachable against `health-and-degradation.md#TS_PROJECTION`; `SnapshotFeed` folds decoded binary deltas and catalog rows against `snapshot-codecs.md#TS_PROJECTION`; `ProgressStore` holds monotonic marks against `progress-and-observation.md#TS_PROJECTION` where a mark below the held rank never regresses the cell; `ConflictPresenceStore` folds the sync-segment outcomes and presence rows against `sync-collaboration.md#TS_PROJECTION`, presence expiring on its declared expiry field. `RuntimeFeed` and `SnapshotFeed` ride the closed app-service budget; the others are fold rows beside them.
- Entry: each fold is one `keyedFold` over its event kinds into an immutable keyed map; key-discrimination is driven by the exact discriminant keys the wire shapes carry; a fold reconnects through the `StreamPolicy` bounded `Schedule` and applies its operator set rather than an improvised loop; the staleness-forward retry value the envelope-and-evidence availability cluster carries grounds in this policy, so an improvised reconnect loop or an unbounded retry is the deleted form.
- Packages: `effect` for `Stream`, `SubscriptionRef`, `Schedule`, structured concurrency, and `Schema`; the `@effect-atom/atom` cell bridge is consumed at the `@rasm/web` boundary, never imported here.
- Growth: a new boundary concept lands as one `keyedFold` store row; a new event kind lands as one fold arm on its owning store; a new reconnect posture lands as one `StreamPolicy` schedule or operator row every fold inherits.
- Boundary: no fold re-validates a value an earlier decode gate admitted; ordering is borrowed verbatim from the wire — monotonic progress, last-write-wins or monotonic merge per contract; reconnect and backoff are owned once by `StreamPolicy` and never re-derived per fold; the domain dials no transport, imports no `@connectrpc/*`, and depends only on `@rasm/interchange` decoded shapes.

```ts contract
interface StreamPolicy {
  readonly reconnect: Schedule.Schedule<Duration.Duration, unknown>;
  readonly buffer: { readonly capacity: number; readonly strategy: "dropping" | "sliding" | "suspend" };
  readonly throttle: { readonly cost: number; readonly duration: Duration.Duration };
  readonly groupedWithin: { readonly chunkSize: number; readonly window: Duration.Duration };
  readonly fold: <In, Out>(source: Stream.Stream<In>, initial: Out, step: (state: Out, event: In) => Out) => Stream.Stream<Out>;
}

const defaultStreamPolicy: StreamPolicy = {
  reconnect: Schedule.exponential(Duration.millis(250)).pipe(Schedule.union(Schedule.spaced(Duration.seconds(30)))),
  buffer: { capacity: 256, strategy: "sliding" },
  throttle: { cost: 1, duration: Duration.millis(16) },
  groupedWithin: { chunkSize: 64, window: Duration.millis(100) },
  fold: (source, initial, step) => source.pipe(Stream.scan(initial, step)),
};

const keyedFold = <In, K, V>(
  source: Stream.Stream<In>,
  key: (event: In) => K,
  merge: (prior: Option.Option<V>, event: In) => V,
  policy: StreamPolicy,
): Effect.Effect<SubscriptionRef.SubscriptionRef<ReadonlyMap<K, V>>, never, Scope.Scope> =>
  Effect.gen(function* () {
    const ref = yield* SubscriptionRef.make<ReadonlyMap<K, V>>(new Map());
    yield* source.pipe(
      Stream.retry(policy.reconnect),
      Stream.runForEach((event) =>
        SubscriptionRef.update(ref, (m) => {
          const k = key(event);
          return new Map(m).set(k, merge(Option.fromNullable(m.get(k)), event));
        })),
      Effect.forkScoped,
    );
    return ref;
  });

interface ProgressStore {
  readonly marks: SubscriptionRef.SubscriptionRef<ReadonlyMap<string, ProgressMarkWire>>;
}

const progressMerge = (prior: Option.Option<ProgressMarkWire>, event: ProgressMarkWire): ProgressMarkWire =>
  Option.match(prior, { onNone: () => event, onSome: (p) => (event.rank >= p.rank ? event : p) });
```

## [3]-[RESEARCH]

- [STANDING_QUERY]: the tumbling/sliding/session window vocabulary plus watermark semantics over the `keyedFold` algebra — landed as `StreamPolicy` operator rows and one `keyedFold` window arm — resolves against the C# `query-rail#STANDING_QUERY` window vocabulary the upstream branch authors; until that fence lands, the window arm is the one refinement-horizon entry the fold algebra carries.
