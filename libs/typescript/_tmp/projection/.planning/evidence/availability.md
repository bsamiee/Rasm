# [PROJECTION_AVAILABILITY]

`AvailabilityStore` is the command-availability read gate — a `combinators#KEYED_FOLD` row folding availability rows into an immutable keyed map exposing `isEnabled`, read across the domain boundary by the `interchange` `CommandGateway` at dial time so a disabled command never fires. The gate value is read across the boundary, the gateway never re-mints availability, and co-location follows altitude not read-dependency, so the dialing gateway lives in `interchange` while the fold stays transport-free here. `isEnabled` is not the wire `available` flag alone: it is the conjunction of that flag and a live degradation gate joined from the `cells#LIVE_CELLS` health cell, so a capability the health snapshot marks below its admitting tier greys its command at the one egress even while its row still reads `available`. The realized TypeScript half of the cross-libs `ONE_HEALTH_DEGRADATION_WIRE` seam: the C# `csharp:Rasm.AppHost/Observability/health#TS_PROJECTION` mints the level, the gate here weighs it at dial time.

## [01]-[INDEX]

- [01]-[AVAILABILITY_GATE]: Owns `AvailabilityStore`, the `availabilityStore` two-arm constructor, and the `isEnabled` conjunction read.
- [02]-[GATE_POLICY]: Owns `DegradationLevel` (the ordered health frame), its `Order`, and `gatePolicy` (the one frozen level-to-admitted-tier table the conjunction reads).

## [02]-[AVAILABILITY_GATE]

- Owner: `AvailabilityStore` and `availabilityStore`, the two-arm fold binding the `combinators#KEYED_FOLD` over `CommandAvailabilityWire` rows against `csharp:Rasm.AppUi/Shell/commands#TS_PROJECTION` to the live `cells#LIVE_CELLS` health-degradation cell, plus the `isEnabled` read the `interchange/Transport/gateway#COMMAND_GATEWAY` `CommandGateway` consumes as a dial-time gate. The wire row is `{ key, available, level }`: `key` is the intent identity each cell folds on, `available` is the boolean gate the wire weighs, and `level` is the per-intent degradation floor the row carries verbatim from the wire — the minimum live host level at which the intent stays admitted.
- Cases: each availability row keys on its `key` intent identity; `isEnabled` reads both the live availability map and the held degradation level, so an unknown intent reports unreachable, a present row whose wire `available` flag is false reports disabled, and a present-and-available row reports enabled only when the live host `DegradationLevel` still admits the intent's per-row floor under `gatePolicy`. The held level is the second arm of the store — `feedStore("health")`'s degradation cell decoded into the ordered `DegradationLevel`, read at dial time so a host that degrades after the row arrives greys the command without a second availability push. The conjunction is total: a row, a flag, and a live level, never a partial gate.
- Packages: `effect` for `Stream`, `SubscriptionRef`, `Effect`, `HashMap`, `Option`, and `Scope`.
- Growth: a new gating input lands as one availability-row field; a new health-gated surface is one `gatePolicy` row, never a second health model branch-side.
- Boundary: the store is a fold in the same altitude as the stream stores and stays transport-free; the degradation level arrives owned from `cells#LIVE_CELLS`, never re-derived here — this fold mints the ordered `DegradationLevel` over the decode-admitted wire `level` column and weighs it, but the level value is the C#-side health authority's; the `CommandGateway` reads the gate at dial time and writes back the command receipt, an intra-neutral read, not a tier reversal; the domain dials no transport.

```ts contract
import { Effect, HashMap, Option, Scope, Stream, SubscriptionRef } from "effect";
import type { CommandAvailabilityWire } from "@rasm/interchange";
import { keyedFold } from "../fold/combinators";
import { withPolicy, type StreamPolicy } from "../fold/policy";
// DegradationLevel, admits, levelOf are owned by the [3]-[GATE_POLICY] cluster below.

interface AvailabilityStore {
  readonly rows: SubscriptionRef.SubscriptionRef<HashMap.HashMap<string, CommandAvailabilityWire>>;
  readonly level: SubscriptionRef.SubscriptionRef<DegradationLevel>;
  readonly isEnabled: (intentKey: string) => Effect.Effect<boolean>;
}

const availabilityStore = (
  rows: Stream.Stream<CommandAvailabilityWire>,
  health: Stream.Stream<DegradationLevel>,
  policy: StreamPolicy,
): Effect.Effect<AvailabilityStore, never, Scope.Scope> =>
  Effect.gen(function* () {
    const cells = yield* keyedFold(rows, (row) => row.key, (_prior, row) => row, policy);
    const level = yield* SubscriptionRef.make<DegradationLevel>(DegradationLevel.Full());
    yield* withPolicy(health, policy).pipe(
      Stream.runForEach((next) => SubscriptionRef.set(level, next)),
      Effect.forkScoped,
    );
    return {
      rows: cells,
      level,
      isEnabled: (intentKey) =>
        Effect.zipWith(SubscriptionRef.get(cells), SubscriptionRef.get(level), (m, held) =>
          Option.match(HashMap.get(m, intentKey), {
            onNone: () => false,
            onSome: (row) => row.available && admits(held, levelOf(row.level)),
          })),
    };
  });
```

The second arm reuses `withPolicy` from `fold/policy#STREAM_POLICY` exactly as `keyedFold` does, so the health feed inherits the same bounded reconnect, buffer, and throttle as the availability rows; `Effect.zipWith` reads both refs once per dial so the conjunction never tears across a mid-read level advance.

## [03]-[GATE_POLICY]

- Owner: `DegradationLevel`, the five-case `Data.TaggedEnum` ordered health frame mirroring the C# `csharp:Rasm.AppHost/Observability/health#TS_PROJECTION` `DegradationLevel` vocabulary (`Full`/`ReducedRemote`/`LocalOnly`/`ReadOnly`/`Suspended`, ranks 0 through 4 of increasing degradation); `degradationRank`, the `as const satisfies Record` rank table keyed by the tag; `DegradationOrder`, the `Order` over the rank so the gate compares levels rather than re-deriving the ladder per site; `levelOf`, the total decode of the decode-admitted wire `level` kebab-string column into the ordered enum; and `admits`, the one predicate the conjunction reads — a held host level admits an intent's per-row degradation floor exactly when the held level's rank is at or below the floor's rank, so a host at `Full` admits every intent and a host at `ReadOnly` greys every intent whose floor is `LocalOnly` or above.
- Cases: `levelOf` reads the verbatim C# kebab spelling (`"full"`/`"reduced-remote"`/`"local-only"`/`"read-only"`/`"suspended"`) the wire stamps and maps it to its `DegradationLevel` tag, flooring an unrecognized string to `Suspended` so an unknown level fails closed rather than admitting a command the host cannot serve; `admits` is `Order.lessThanOrEqualTo(DegradationOrder)` over the held level against the row floor, the total comparison `gatePolicy` is. `gatePolicy` is not a per-level command list — the per-intent floor rides each availability row's `level` column verbatim, so the admitted-tier decision is one `Order` comparison against that floor, never a parallel health model branch-side; widening the ladder is one `DegradationLevel` variant breaking `degradationRank` and `levelOf` at compile time, never a re-encoded threshold ladder.
- Packages: `effect` for `Data.TaggedEnum` and `Order`.
- Growth: a new health level lands as one `DegradationLevel` variant plus one `degradationRank` row, breaking `levelOf` and the rank table at compile time; the `admits` predicate and the `isEnabled` conjunction are unchanged.
- Boundary: the wire `level` column is decode-admitted and read verbatim — this cluster mints the ordered `DegradationLevel` and its `Order` over the wire spelling but re-derives no level value, the level authority being C#-side; the rank ordering is the one ladder the gate weighs, never a second threshold table; the gate fails closed on an unknown level so a decode gap greys rather than admits.

```ts contract
import { Data, Order } from "effect";

type DegradationLevel = Data.TaggedEnum<{
  readonly Full: {};
  readonly ReducedRemote: {};
  readonly LocalOnly: {};
  readonly ReadOnly: {};
  readonly Suspended: {};
}>;
const DegradationLevel = Data.taggedEnum<DegradationLevel>();

const degradationRank = {
  Full: 0,
  ReducedRemote: 1,
  LocalOnly: 2,
  ReadOnly: 3,
  Suspended: 4,
} as const satisfies Record<DegradationLevel["_tag"], number>;

const DegradationOrder: Order.Order<DegradationLevel> = Order.mapInput(Order.number, (level) => degradationRank[level._tag]);

const wireLevel = {
  full: DegradationLevel.Full,
  "reduced-remote": DegradationLevel.ReducedRemote,
  "local-only": DegradationLevel.LocalOnly,
  "read-only": DegradationLevel.ReadOnly,
  suspended: DegradationLevel.Suspended,
} as const satisfies Record<string, () => DegradationLevel>;

const levelOf = (level: string): DegradationLevel =>
  (level in wireLevel ? wireLevel[level as keyof typeof wireLevel] : DegradationLevel.Suspended)();

const admits = (held: DegradationLevel, floor: DegradationLevel): boolean =>
  Order.lessThanOrEqualTo(DegradationOrder)(held, floor);
```
