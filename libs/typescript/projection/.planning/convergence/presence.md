# [PROJECTION_PRESENCE]

The ephemeral presence row beside the convergence fold — a `keyed-fold#KEYED_FOLD` row over the `PresenceRowWire` stream where each row expires on its declared `expiresAt` instant, so an expired row never gates a live editing surface. Presence rides the `ConflictPresenceStore` as a separate fold from `lww-merge#LWW_MERGE`; a presence op never mutates the live geometry cell. The row is ephemeral by TTL, distinct in kind from the durable LWW-converged op-log state.

## [1]-[INDEX]

One cluster: `[2]-[PRESENCE]` owns `ConflictPresenceStore`, `presenceMerge`, the `presenceKey` row-identity projection, and the `conflictPresenceStore` constructor.

## [2]-[PRESENCE]

- Owner: `ConflictPresenceStore`, the store binding the `lww-merge#LWW_MERGE` converged `state` and the presence `keyedFold` row together; `presenceMerge`, the TTL merge arm that keeps the row with the later `expiresAt`; `presenceKey`, the row-identity projection over the `PresenceRowWire` `{ actor, entityKind, entityKey, expiresAt }` shape — the `actor`-on-`entityKey` composite addressing one editor's presence on one synced entity, distinct from the `ContentKey` convergence identity; `conflictPresenceStore`, the constructor that forks both folds into one `Scope`.
- Cases: a fresh presence row installs its cell; a later row replaces it only when its `expiresAt` is at or beyond the held row's, so a stale heartbeat never resurrects an expired editor; an expired row is read off the cell as inactive by the gate that consumes it; two editors on one entity occupy two distinct cells because the `actor` half of the key disambiguates, and one editor on two entities occupies two cells because the `entityKey` half disambiguates.
- Packages: `effect` for `SubscriptionRef`, `Option`, `Effect`, `HashMap`, and `Scope`.
- Growth: a new presence dimension lands as one `PresenceRowWire` field, never a parallel store; the TTL horizon unifies with the `standing-query/watermark#WATERMARK` frontier once frontier garbage collection lands.
- Boundary: the row reads `expiresAt` as a decode-admitted instant, never re-validated; presence keys on the `actor`-on-`entityKey` row identity and never on the `ContentKey` convergence identity; both folds pipe through `stream-policy#STREAM_POLICY`.

```ts contract
import { Effect, HashMap, Option, Scope, Stream, SubscriptionRef } from "effect";
import type { ConflictReceiptWire, OpLogEntryWire, PresenceRowWire } from "@rasm/interchange";
import { keyedFold } from "../fold-core/keyed-fold";
import type { StreamPolicy } from "../fold-core/stream-policy";
import { conflictPresenceFold, type ConflictPresenceState } from "./lww-merge";

interface ConflictPresenceStore {
  readonly state: SubscriptionRef.SubscriptionRef<ConflictPresenceState>;
  readonly presence: SubscriptionRef.SubscriptionRef<HashMap.HashMap<string, PresenceRowWire>>;
}

const expiresMs = (row: PresenceRowWire): number => {
  const ms = Date.parse(row.expiresAt);
  return Number.isNaN(ms) ? 0 : ms;
};

const presenceMerge = (prior: Option.Option<PresenceRowWire>, event: PresenceRowWire): PresenceRowWire =>
  Option.match(prior, {
    onNone: () => event,
    onSome: (p) => (expiresMs(event) >= expiresMs(p) ? event : p),
  });

const presenceKey = (row: PresenceRowWire): string => `${row.actor}@${row.entityKey}`;

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
