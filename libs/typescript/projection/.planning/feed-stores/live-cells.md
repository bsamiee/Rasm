# [PROJECTION_LIVE_CELLS]

The four live-cell stream stores — `RuntimeFeed`, `HealthStore`, `SnapshotFeed`, `ProgressStore` — each one `keyed-fold#KEYED_FOLD` row over its decoded wire union, keyed by the verbatim C# discriminant so the latest receipt per slot is the live cell. The state layer is a projection of the wire vocabulary, never a parallel model: each key reads the discriminant the owning `#TS_PROJECTION` shape carries, the three latest-write stores share one polymorphic `latestWrite` merge, and only `ProgressStore` carries its own monotonic-rank arm. `RuntimeFeed` and `SnapshotFeed` ride the closed app-service budget; `HealthStore` and `ProgressStore` are fold rows beside them.

## [1]-[INDEX]

One cluster: `[2]-[LIVE_CELLS]` owns `RuntimeFeed`, `HealthStore`, `SnapshotFeed`, and `ProgressStore` as `keyedFold` rows.

## [2]-[LIVE_CELLS]

- Owner: `RuntimeFeed` for the lifecycle fold, `HealthStore` for the health-and-degradation fold, `SnapshotFeed` for the snapshot catalog/delta fold, and `ProgressStore` for the monotonic progress cells. Each store is one `keyedFold` row; the variation is the key discriminator and the merge arm.
- Cases: each `*Key` is one `Match.value(event).pipe(...)` over the verbatim C# discriminant field. `runtimeKey` and `healthKey` terminate `Match.exhaustive` so a fourth wire case breaks at compile time rather than aliasing into a silent else; `snapshotKey` terminates `Match.orElse` only because `SnapshotDeltaWire` is the genuine residual (no `id`, no `source`), not a masked new variant. `RuntimeFeed` folds the `PhaseReceiptWire`/`BootMarkerWire`/`FaultRecordWire`/`DrainReceiptWire` union against `csharp:Rasm.AppHost/hosting/lifecycle-and-drain#TS_PROJECTION`; `HealthStore` folds `HealthSnapshotWire`/`DegradationWire`/`AlertReceiptWire` against `csharp:Rasm.AppHost/observability/health-and-degradation#TS_PROJECTION`, the retained-capability set on the degradation cell gating which web surfaces are reachable; `SnapshotFeed` folds `SnapshotCatalogRowWire`/`SnapshotDeltaWire`/`RestoreReceiptWire` against `csharp:Rasm.Persistence/snapshots/codecs#TS_PROJECTION`; `ProgressStore` folds `ProgressMarkWire` against `csharp:Rasm.Compute/progress/progress#TS_PROJECTION` where a mark below the held rank never regresses the cell.
- Packages: `effect` for `Stream`, `SubscriptionRef`, `Option`, `Match`, `Effect`, and `Scope`.
- Growth: a new boundary concept lands as one `keyedFold` store row; a new event kind lands as one `Match.when` arm breaking the exhaustive terminal on its owning store's key dispatch.
- Boundary: no store re-validates a value an earlier decode admitted; the discriminant is imported from `@rasm/ts` verbatim and never re-authored; ordering is borrowed from the wire — monotonic progress for `ProgressStore`, latest-write per slot for the rest; reconnect, buffer, throttle, and batch are owned once by `stream-policy#STREAM_POLICY`.

```ts contract
import { Effect, Match, Option, Scope, Stream, SubscriptionRef } from "effect";
import type {
  AlertReceiptWire, BootMarkerWire, DegradationWire, DrainReceiptWire,
  FaultRecordWire, HealthSnapshotWire, PhaseReceiptWire, ProgressMarkWire,
  RestoreReceiptWire, SnapshotCatalogRowWire, SnapshotDeltaWire,
} from "@rasm/ts";
import { keyedFold } from "../fold-core/keyed-fold";
import type { StreamPolicy } from "../fold-core/stream-policy";

type RuntimeEventWire = PhaseReceiptWire | BootMarkerWire | FaultRecordWire | DrainReceiptWire;

interface RuntimeFeed {
  readonly cells: SubscriptionRef.SubscriptionRef<ReadonlyMap<string, RuntimeEventWire>>;
}

const runtimeKey = (event: RuntimeEventWire): string =>
  Match.value(event).pipe(
    Match.when({ kind: Match.string }, (e) => `fault:${e.kind}`),
    Match.when({ to: Match.string }, (e) => `phase:${e.to}`),
    Match.when({ pid: Match.number }, (e) => `boot:${e.phase}`),
    Match.when({ final: Match.boolean }, (e) => `drain:${e.final}`),
    Match.exhaustive,
  );

const latestWrite = <E>(_prior: Option.Option<E>, event: E): E => event;

type HealthEventWire = HealthSnapshotWire | DegradationWire | AlertReceiptWire;

interface HealthStore {
  readonly cells: SubscriptionRef.SubscriptionRef<ReadonlyMap<string, HealthEventWire>>;
}

const healthKey = (event: HealthEventWire): string =>
  Match.value(event).pipe(
    Match.when({ entries: Match.defined }, () => "health:snapshot"),
    Match.when({ level: Match.defined }, () => "degradation:level"),
    Match.when({ ruleId: Match.string }, (e) => `alert:${e.ruleId}`),
    Match.exhaustive,
  );

type SnapshotEventWire = SnapshotCatalogRowWire | SnapshotDeltaWire | RestoreReceiptWire;

interface SnapshotFeed {
  readonly cells: SubscriptionRef.SubscriptionRef<ReadonlyMap<string, SnapshotEventWire>>;
}

const snapshotKey = (event: SnapshotEventWire): string =>
  Match.value(event).pipe(
    Match.when({ id: Match.string }, (e) => `catalog:${e.id}`),
    Match.when({ source: Match.defined }, (e) => `restore:${e.target}`),
    Match.orElse(() => "delta:head"),
  );

interface ProgressStore {
  readonly marks: SubscriptionRef.SubscriptionRef<ReadonlyMap<string, ProgressMarkWire>>;
}

const progressMerge = (prior: Option.Option<ProgressMarkWire>, event: ProgressMarkWire): ProgressMarkWire =>
  Option.match(prior, { onNone: () => event, onSome: (p) => (event.rank >= p.rank ? event : p) });

const runtimeFeed = (events: Stream.Stream<RuntimeEventWire>, policy: StreamPolicy): Effect.Effect<RuntimeFeed, never, Scope.Scope> =>
  keyedFold(events, runtimeKey, latestWrite, policy).pipe(Effect.map((cells) => ({ cells })));

const healthStore = (events: Stream.Stream<HealthEventWire>, policy: StreamPolicy): Effect.Effect<HealthStore, never, Scope.Scope> =>
  keyedFold(events, healthKey, latestWrite, policy).pipe(Effect.map((cells) => ({ cells })));

const snapshotFeed = (events: Stream.Stream<SnapshotEventWire>, policy: StreamPolicy): Effect.Effect<SnapshotFeed, never, Scope.Scope> =>
  keyedFold(events, snapshotKey, latestWrite, policy).pipe(Effect.map((cells) => ({ cells })));

const progressStore = (events: Stream.Stream<ProgressMarkWire>, policy: StreamPolicy): Effect.Effect<ProgressStore, never, Scope.Scope> =>
  keyedFold(events, (e) => e.scope, progressMerge, policy).pipe(Effect.map((marks) => ({ marks })));
```
