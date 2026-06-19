# [PROJECTION_AVAILABILITY_GATE]

`AvailabilityStore` is the command-availability read gate — a `keyed-fold#KEYED_FOLD` row folding availability rows into an immutable keyed map exposing `isEnabled`, read across the domain boundary by the `interchange` `CommandGateway` at dial time so a disabled command never fires. The gate value is read across the boundary, the gateway never re-mints availability, and co-location follows altitude not read-dependency, so the dialing gateway lives in `interchange` while the fold stays transport-free here. The degradation level the gate weighs is an input read off `feed-stores/live-cells#LIVE_CELLS`, never re-derived.

## [1]-[INDEX]

One cluster: `[2]-[AVAILABILITY_GATE]` owns `AvailabilityStore`, the `availabilityStore` keyed-fold constructor, and the `isEnabled` read.

## [2]-[AVAILABILITY_GATE]

- Owner: `AvailabilityStore` and `availabilityStore`, the `keyed-fold#KEYED_FOLD` fold over `CommandAvailabilityWire` rows against `csharp:Rasm.AppUi/commands/commands-availability#TS_PROJECTION` into an immutable keyed map plus the `isEnabled` read the `interchange/gateway/command-gateway#COMMAND_GATEWAY` `CommandGateway` consumes as a dial-time gate. The wire row is `{ key, available, level }`: `key` is the intent identity each cell folds on, `available` is the boolean gate `isEnabled` reads, and `level` is the degradation tier the row carries verbatim from the wire.
- Cases: each availability row keys on its `key` intent identity; `isEnabled` reads the live map so an unknown intent reports unreachable and a present row reports its `available` flag; the `level` degradation tier rides each row as a decode-admitted column, the health-cell degradation off `feed-stores/live-cells#LIVE_CELLS` the C#-side input the wire already weighed, never re-derived branch-side.
- Packages: `effect` for `Stream`, `SubscriptionRef`, `Effect`, `HashMap`, `Option`, and `Scope`.
- Growth: a new gating input lands as one availability-row field; a degradation-derived gating fold lands as one additional input arm on the same store.
- Boundary: the store is a fold in the same altitude as the stream stores and stays transport-free; the `CommandGateway` reads the gate at dial time and writes back the command receipt, an intra-neutral read, not a tier reversal; the domain dials no transport.

```ts contract
import { Effect, HashMap, Option, Scope, Stream, SubscriptionRef } from "effect";
import type { CommandAvailabilityWire } from "@rasm/interchange";
import { keyedFold } from "../fold-core/keyed-fold";
import type { StreamPolicy } from "../fold-core/stream-policy";

interface AvailabilityStore {
  readonly rows: SubscriptionRef.SubscriptionRef<HashMap.HashMap<string, CommandAvailabilityWire>>;
  readonly isEnabled: (intentKey: string) => Effect.Effect<boolean>;
}

const availabilityStore = (
  rows: Stream.Stream<CommandAvailabilityWire>,
  policy: StreamPolicy,
): Effect.Effect<AvailabilityStore, never, Scope.Scope> =>
  keyedFold(rows, (row) => row.key, (_prior, row) => row, policy).pipe(
    Effect.map((cells) => ({
      rows: cells,
      isEnabled: (intentKey) =>
        SubscriptionRef.get(cells).pipe(
          Effect.map((m) => Option.match(HashMap.get(m, intentKey), { onNone: () => false, onSome: (row) => row.available })),
        ),
    })),
  );
```
