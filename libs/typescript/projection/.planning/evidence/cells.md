# [PROJECTION_CELLS]

One live-cell owner, `FeedKind`, the closed vocabulary whose every row carries its own decoded wire union, key projection, and slot merge, and one `feedStore` entrypoint that discriminates on the kind value into a `combinators#KEYED_FOLD` map keyed by the verbatim C# discriminant so the latest receipt per slot is the live cell. The lifecycle, health-and-degradation, snapshot-catalog, and progress feeds are four rows of the one vocabulary, never four parallel stores or four sibling constructors; the state layer is a projection of the wire vocabulary, never a parallel model. A new boundary concept lands as one `FeedKind` row, a new event kind as one `Match.when` arm breaking the exhaustive terminal on that row's key projection.

## [1]-[INDEX]

- [1]-[LIVE_CELLS]: Owns `FeedKind`, the four feed rows, and the `feedStore` keyed-fold entrypoint.

## [2]-[LIVE_CELLS]

- Owner: `FeedKind`, the four-row `as const` vocabulary — `runtime`, `health`, `snapshot`, `progress` — each row carrying its wire union, its `key` projection, and its `merge` arm; `feedStore`, the one entrypoint that reads the kind row and folds its source through `keyedFold` into a `SubscriptionRef`-backed cell map, with `FeedEvent<K>` deriving each feed's event type from its row `key` so the call site never re-declares the union. The variation between feeds is row data, never a parallel store type or a sibling factory.
- Cases: each row's `key` is one `Match.value(event).pipe(...)` over the verbatim C# discriminant field. `runtime` and `health` terminate `Match.exhaustive` so a fourth wire case breaks at compile time rather than aliasing into a silent else; `snapshot` terminates `Match.orElse` only because `SnapshotDeltaWire` is the genuine residual (no `id`, no `source`), not a masked new variant. `latestWrite` is the polymorphic merge the `runtime`, `health`, and `snapshot` rows share; the `progress` row carries the monotonic-rank merge where a mark below the held rank never regresses the cell. The `runtime` row folds the `PhaseReceiptWire`/`BootMarkerWire`/`FaultRecordWire`/`DrainReceiptWire` union against `csharp:Rasm.AppHost/Runtime/lifecycle#TS_PROJECTION`; the `health` row folds `HealthSnapshotWire`/`DegradationWire`/`AlertReceiptWire` against `csharp:Rasm.AppHost/Observability/health#TS_PROJECTION`, the retained-capability set on the degradation cell gating which web surfaces are reachable; the `snapshot` row folds `SnapshotCatalogRowWire`/`SnapshotDeltaWire`/`RestoreReceiptWire` against `csharp:Rasm.Persistence/Version/snapshots#TS_PROJECTION`; the `progress` row folds `ProgressMarkWire` against `csharp:Rasm.Compute/Runtime/progress#TS_PROJECTION`.
- Packages: `effect` for `Match`, `Option`, `Stream`, `SubscriptionRef`, `Effect`, `HashMap`, and `Scope`.
- Growth: a new boundary concept lands as one `FeedKind` row carrying its union, key, and merge; a new event kind lands as one `Match.when` arm breaking the exhaustive terminal on that row's key projection. The `feedStore` entrypoint never grows.
- Boundary: no row re-validates a value an earlier decode admitted; the discriminant is imported from `@rasm/interchange` verbatim and never re-authored; ordering is borrowed from the wire — monotonic rank for `progress`, latest-write per slot for the rest; reconnect, buffer, throttle, and batch are owned once by `policy#STREAM_POLICY`; consumers read the `SubscriptionRef` and the `@effect-atom/atom` bridge binds it at the `ui` boundary, never imported here.

```ts contract
import { Effect, HashMap, Match, Option, Scope, Stream, SubscriptionRef } from "effect";
import type {
  AlertReceiptWire, BootMarkerWire, DegradationWire, DrainReceiptWire,
  FaultRecordWire, HealthSnapshotWire, PhaseReceiptWire, ProgressMarkWire,
  RestoreReceiptWire, SnapshotCatalogRowWire, SnapshotDeltaWire,
} from "@rasm/interchange";
import { keyedFold } from "../fold/combinators";
import type { StreamPolicy } from "../fold/policy";

type RuntimeEventWire = PhaseReceiptWire | BootMarkerWire | FaultRecordWire | DrainReceiptWire;
type HealthEventWire = HealthSnapshotWire | DegradationWire | AlertReceiptWire;
type SnapshotEventWire = SnapshotCatalogRowWire | SnapshotDeltaWire | RestoreReceiptWire;

const latestWrite = <E>(_prior: Option.Option<E>, event: E): E => event;

const FeedKind = {
  runtime: {
    key: (event: RuntimeEventWire) =>
      Match.value(event).pipe(
        Match.when({ kind: Match.string }, (e) => `fault:${e.kind}`),
        Match.when({ to: Match.string }, (e) => `phase:${e.to}`),
        Match.when({ pid: Match.number }, (e) => `boot:${e.phase}`),
        Match.when({ final: Match.boolean }, (e) => `drain:${e.final}`),
        Match.exhaustive,
      ),
    merge: latestWrite<RuntimeEventWire>,
  },
  health: {
    key: (event: HealthEventWire) =>
      Match.value(event).pipe(
        Match.when({ entries: Match.defined }, () => "health:snapshot"),
        Match.when({ level: Match.defined }, () => "degradation:level"),
        Match.when({ ruleId: Match.string }, (e) => `alert:${e.ruleId}`),
        Match.exhaustive,
      ),
    merge: latestWrite<HealthEventWire>,
  },
  snapshot: {
    key: (event: SnapshotEventWire) =>
      Match.value(event).pipe(
        Match.when({ id: Match.string }, (e) => `catalog:${e.id}`),
        Match.when({ source: Match.defined }, (e) => `restore:${e.target}`),
        Match.orElse(() => "delta:head"),
      ),
    merge: latestWrite<SnapshotEventWire>,
  },
  progress: {
    key: (event: ProgressMarkWire) => event.scope,
    merge: (prior: Option.Option<ProgressMarkWire>, event: ProgressMarkWire) =>
      Option.match(prior, { onNone: () => event, onSome: (p) => (event.rank >= p.rank ? event : p) }),
  },
} as const;

type FeedEvent<K extends keyof typeof FeedKind> = Parameters<(typeof FeedKind)[K]["key"]>[0];

const feedStore = <K extends keyof typeof FeedKind>(
  kind: K,
  events: Stream.Stream<FeedEvent<K>>,
  policy: StreamPolicy,
): Effect.Effect<SubscriptionRef.SubscriptionRef<HashMap.HashMap<string, FeedEvent<K>>>, never, Scope.Scope> =>
  keyedFold(
    events,
    FeedKind[kind].key as (event: FeedEvent<K>) => string,
    FeedKind[kind].merge as (prior: Option.Option<FeedEvent<K>>, event: FeedEvent<K>) => FeedEvent<K>,
    policy,
  );
```
