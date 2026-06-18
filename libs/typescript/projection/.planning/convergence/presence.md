# [PROJECTION_PRESENCE]

The ephemeral presence row beside the convergence fold — a `keyed-fold#KEYED_FOLD` row over the `PresenceRowWire` stream where each row expires on its declared `expiresAt` instant, so an expired row never gates a live editing surface. Presence rides the `ConflictPresenceStore` as a separate fold from `lww-merge#LWW_MERGE`; a presence op never mutates the live geometry cell. The row is ephemeral by TTL, distinct in kind from the durable LWW-converged op-log state.

## [1]-[INDEX]

Two clusters:
- `[2]-[PRESENCE]` owns `ConflictPresenceStore`, `presenceMerge`, and the `conflictPresenceStore` constructor.
- `[3]-[RESEARCH]` carries the open `PresenceRowWire` row-identity key field.

## [2]-[PRESENCE]

- Owner: `ConflictPresenceStore`, the store binding the `lww-merge#LWW_MERGE` converged `state` and the presence `keyedFold` row together; `presenceMerge`, the TTL merge arm that keeps the row with the later `expiresAt`; `conflictPresenceStore`, the constructor that forks both folds into one `Scope`.
- Cases: a fresh presence row installs its cell; a later row replaces it only when its `expiresAt` is at or beyond the held row's, so a stale heartbeat never resurrects an expired editor; an expired row is read off the cell as inactive by the gate that consumes it.
- Packages: `effect` for `SubscriptionRef`, `Option`, `Effect`, `HashMap`, and `Scope`.
- Growth: a new presence dimension lands as one `PresenceRowWire` field, never a parallel store; the TTL horizon unifies with the `standing-query/watermark#WATERMARK` frontier once frontier garbage collection lands.
- Boundary: the row reads `expiresAt` as a decode-admitted instant, never re-validated; presence keys on its own row identity and never on the `ContentKey` convergence identity; both folds pipe through `stream-policy#STREAM_POLICY`.

```ts contract
import { Effect, HashMap, Option, Scope, Stream, SubscriptionRef } from "effect";
import type { ConflictReceiptWire, OpLogEntryWire, PresenceRowWire } from "@rasm/ts";
import { keyedFold } from "../fold-core/keyed-fold";
import type { StreamPolicy } from "../fold-core/stream-policy";
import { conflictPresenceFold, type ConflictPresenceState } from "./lww-merge";

interface ConflictPresenceStore {
  readonly state: SubscriptionRef.SubscriptionRef<ConflictPresenceState>;
  readonly presence: SubscriptionRef.SubscriptionRef<HashMap.HashMap<string, PresenceRowWire>>;
}

const presenceMerge = (prior: Option.Option<PresenceRowWire>, event: PresenceRowWire): PresenceRowWire =>
  Option.match(prior, {
    onNone: () => event,
    onSome: (p) => (Date.parse(event.expiresAt) >= Date.parse(p.expiresAt) ? event : p),
  });

const presenceKey = (row: PresenceRowWire): string => row[PRESENCE_ROW_IDENTITY];

const conflictPresenceStore = (
  changefeed: Stream.Stream<OpLogEntryWire | ConflictReceiptWire>,
  presenceFeed: Stream.Stream<PresenceRowWire>,
  policy: StreamPolicy,
): Effect.Effect<ConflictPresenceStore, never, Scope.Scope> =>
  Effect.all({
    state: conflictPresenceFold(changefeed, policy),
    presence: keyedFold(presenceFeed, presenceKey, presenceMerge, policy),
  });
```

## [3]-[RESEARCH]

- [PRESENCE_KEY]: `PRESENCE_ROW_IDENTITY` is the `PresenceRowWire` row-identity field `presenceKey` reads — the editor/session identity each presence heartbeat carries, distinct from the `ContentKey` convergence identity. The exact member spelling resolves against the `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` decoded shape before the fence is transcribed.
