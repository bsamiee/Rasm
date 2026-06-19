# [PLATFORM_LOCAL_PERSISTENCE]

One page owns the single browser-local store — `LocalPersistence`, the per-domain named-store topology over `idb-keyval` carrying the Schema-encoded last-good snapshot, the offline command queue drained verbatim into the `interchange` `CommandGateway`, the OIDC pending-flow record `identity-session/auth-session.md` persists across the IdP redirect, and the `worker/decode-pool.md` residency-manifest cache. Each logical domain is one `StoreDomain` `Schema.Literal` row mapped through one frozen `Record<StoreDomain, UseStore>` table to its own named `idb-keyval` backing store, so a `snapshot` clear never evicts the offline queue and the offline-queue drain is one atomic `entries`-then-`clear` transaction a mid-drain crash cannot half-apply. The Schema-encoded `lastGood` snapshot rides a `KeyValueStore.make` adapter over the `snapshot` named store so the KV-abstraction premise survives; a direct `localStorage`/`IndexedDB` access outside the `StoreDomain` table, a JSON-stringified blob outside the Schema-encoded snapshot, or a key-prefix convention in one flat store is the deleted form.

## [1]-[INDEX]

[LOCAL_PERSISTENCE]: the per-domain `StoreDomain` named-store table, the Schema-encoded last-good snapshot, and the atomic offline-queue drain.

## [2]-[LOCAL_PERSISTENCE]

- Owner: `LocalPersistence`, the per-domain browser-local store — one `StoreDomain` `Schema.Literal` axis (`snapshot`/`offline-queue`/`auth-flow`/`viewpoint-cache`) mapped through one frozen `Record<StoreDomain, UseStore>` table to per-domain `idb-keyval` `createStore(dbName, storeName)` named backing stores, the Schema-encoded `lastGood` snapshot over a `KeyValueStore.make` adapter bound to the `snapshot` store, the `offlineQueue` enqueue/atomic-drain over the `offline-queue` store, the single-entry `pendingFlow` record over the `auth-flow` store, and the `viewpoint` residency cache over the `viewpoint-cache` store. The store-to-handle map is the one `StoreDomain` table and a per-domain `createStore` call scattered at a consumer or a key-prefix convention in one flat store is the named flat-store defect.
- Cases: `LocalPersistence` resolves every domain through one `keyof typeof`-discriminated `Record<StoreDomain, UseStore>` table built once at service construction, each row a distinct `createStore(\`rasm-${domain}\`, domain)` named store so an independent `clear` on one domain never touches another and IndexedDB transaction isolation holds per domain; the `lastGood<A, I, R>(schema, key)` snapshot carrier `Schema.encode`s the value into the `snapshot` store through a `KeyValueStore.make` adapter wrapping the `idb-keyval` `get`/`set` so the Schema-encode path retains its `KeyValueStore` abstraction and `load` `Schema.decodeUnknown`s the stored string back to `Option.Option<A>`, a missing key projecting to `Option.none`; the `offlineQueue` element is the resolved-intent pair `{ verb: ControlVerb; payload: CommandPayloadWire }` — the same pair `interchange` `IntentRegistry.resolve` yields — so the persisted element carries the verb `interchange` `CommandGateway.invoke` requires and `drainOnRedial` re-dials a resolved intent verbatim, never a verb re-derived off the payload; `enqueue` appends one element through `idb-keyval` `update` over the `offline-queue` store (read-modify-write in one transaction, never a get-then-set race), and `drainOnRedial` is one atomic `entries`-then-`clear` over the `offline-queue` store — `entries` cursor-reads the full ordered queue and `clear` empties the store in the same logical drain, so a mid-drain crash either leaves the whole queue or empties it, never a half-applied tear a per-element read-clear loop admits; `pendingFlow` `set`/`get`/`del`s the single-entry OIDC `PendingFlow` record over the `auth-flow` store under one fixed key, cleared on callback completion.
- Auto: each `StoreDomain` row is a distinct `idb-keyval` named store via `createStore`, so the four domains never collide in one `keyval` object store; the `lastGood` snapshot composes `KeyValueStore.make` over the `snapshot` `UseStore` rather than the `@effect/platform-browser` `BrowserKeyValueStore` `layerLocalStorage`/`layerSessionStorage` surface (which is `localStorage`/`sessionStorage`-backed and exposes NO IndexedDB layer), so the IndexedDB durability the offline cache requires is the one sanctioned `idb-keyval` site `LocalPersistence` owns and a hand-rolled `idb-keyval` call outside this owner is the deleted form; the offline-queue drain reaches the `interchange` `CommandGateway` across the intra-package folder seam (the `offline-cache/background-sync-replay.md` `BackgroundSyncReplay` owns the SW-triggered redial drive over this same `LocalPersistence.offlineQueue`) and re-dials no transport here.
- Packages: `effect` for the `Schema` encode/decode, the `Option` last-good carrier, and the `KeyValueStore.make` adapter the `snapshot` snapshot rides; `idb-keyval` for the per-domain `createStore` named stores, the `update` read-modify-write enqueue, and the atomic `entries`/`clear` drain — the one sanctioned `idb-keyval` site; `@effect/platform` for the `KeyValueStore` tag the `lastGood` adapter satisfies. The `@effect/platform-browser` `BrowserKeyValueStore` `layerLocalStorage`/`layerSessionStorage` surface backs no domain here — it carries no IndexedDB layer, so the durable per-domain table is direct `idb-keyval` under the `StoreDomain` map.
- Growth: a new persisted domain lands as one `StoreDomain` `Schema.Literal` row and one `Record` table entry mapping it to its named store, never a parallel store binding or a key-prefix in an existing domain; a new offline-queue operation lands as one fold arm over the same `offline-queue` store, the drain unchanged; the `viewpoint-cache` residency cache and any future durable domain ride the same per-domain `createStore` law.
- Boundary: `LocalPersistence` is the single browser-local store and the one sanctioned `idb-keyval` site — a direct `localStorage`/`IndexedDB`/`idb-keyval` access outside the `StoreDomain` table is the named defect; the `auth-flow` `StoreDomain` named store is the seam `identity-session/auth-session.md`'s OIDC `PendingFlow` record persists into across the full-page IdP redirect (the one downstream chain), and the `viewpoint-cache` store backs `worker/decode-pool.md`'s residency-manifest last-good; the offline-queue element shape is owned here as the resolved-intent pair, so `BackgroundSyncReplay` consumes `drainOnRedial`/`enqueue` over that pair verbatim with no re-derivation and the persisted element is the durable backing of the one `interchange` `CommandGateway` egress face (`ONE_EGRESS_VERB_FACE`), never a parallel command queue; the page authors no flag, no decode beyond the snapshot Schema round-trip, and dials no transport.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { ControlVerb, CommandPayloadWire } from "@rasm/ts/interchange";
import type { UseStore } from "idb-keyval";
import { Effect, Option, ParseResult, Schema } from "effect";
import { KeyValueStore } from "@effect/platform";
import { createStore, get, set, del, update, entries, clear } from "idb-keyval";

// --- [TYPES] ---------------------------------------------------------------------------
const StoreDomain = Schema.Literal("snapshot", "offline-queue", "auth-flow", "viewpoint-cache");
type StoreDomain = typeof StoreDomain.Type;

// --- [MODELS] --------------------------------------------------------------------------
const QueueItem = Schema.Struct({ verb: Schema.Any, payload: Schema.Any }) as unknown as Schema.Schema<{
  readonly verb: ControlVerb;
  readonly payload: CommandPayloadWire;
}>;
type QueueItem = { readonly verb: ControlVerb; readonly payload: CommandPayloadWire };

const PendingFlow = Schema.Struct({
  state: Schema.String,
  codeVerifier: Schema.Redacted(Schema.String),
  returnTo: Schema.String,
  nonce: Schema.String,
});
type PendingFlow = typeof PendingFlow.Type;

const OFFLINE_QUEUE_KEY = "queue" as const;
const PENDING_FLOW_KEY = "pending" as const;

// --- [SERVICES] ------------------------------------------------------------------------
interface LocalPersistence {
  readonly lastGood: <A, I, R>(schema: Schema.Schema<A, I, R>, key: string) => {
    readonly save: (value: A) => Effect.Effect<void, ParseResult.ParseError, R>;
    readonly load: Effect.Effect<Option.Option<A>, ParseResult.ParseError, R>;
  };
  readonly offlineQueue: {
    readonly enqueue: (item: QueueItem) => Effect.Effect<void>;
    readonly drainOnRedial: Effect.Effect<ReadonlyArray<QueueItem>>;
  };
  readonly pendingFlow: {
    readonly put: (flow: PendingFlow) => Effect.Effect<void>;
    readonly take: Effect.Effect<Option.Option<PendingFlow>>;
    readonly clear: Effect.Effect<void>;
  };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
const stores: Record<StoreDomain, UseStore> = {
  snapshot: createStore("rasm-snapshot", "snapshot"),
  "offline-queue": createStore("rasm-offline-queue", "offline-queue"),
  "auth-flow": createStore("rasm-auth-flow", "auth-flow"),
  "viewpoint-cache": createStore("rasm-viewpoint-cache", "viewpoint-cache"),
};

const snapshotKv: KeyValueStore.KeyValueStore = KeyValueStore.make({
  get: (key) =>
    Effect.promise(() => get<string>(key, stores.snapshot)).pipe(Effect.map(Option.fromNullable)),
  set: (key, value) => Effect.promise(() => set(key, value, stores.snapshot)),
  remove: (key) => Effect.promise(() => del(key, stores.snapshot)),
  clear: Effect.promise(() => clear(stores.snapshot)),
  size: Effect.promise(() => entries<string, string>(stores.snapshot)).pipe(Effect.map((e) => e.length)),
});

// --- [COMPOSITION] ---------------------------------------------------------------------
class LocalPersistenceLive extends Effect.Service<LocalPersistenceLive>()("@rasm/ts/platform/LocalPersistence", {
  sync: (): LocalPersistence => ({
    lastGood: (schema, key) => ({
      save: (value) =>
        Schema.encode(schema)(value).pipe(Effect.flatMap((encoded) => snapshotKv.set(key, JSON.stringify(encoded)).pipe(Effect.orDie))),
      load: snapshotKv.get(key).pipe(
        Effect.orDie,
        Effect.flatMap(Option.match({
          onNone: () => Effect.succeed(Option.none()),
          onSome: (raw) => Schema.decodeUnknown(schema)(JSON.parse(raw)).pipe(Effect.map(Option.some)),
        })),
      ),
    }),
    offlineQueue: {
      enqueue: (item) =>
        Effect.promise(() =>
          update<ReadonlyArray<QueueItem>>(OFFLINE_QUEUE_KEY, (prior) => [...(prior ?? []), item], stores["offline-queue"]),
        ),
      drainOnRedial: Effect.promise(() => entries<string, QueueItem>(stores["offline-queue"])).pipe(
        Effect.map((rows) => rows.map(([, item]) => item)),
        Effect.tap(() => Effect.promise(() => clear(stores["offline-queue"]))),
      ),
    },
    pendingFlow: {
      put: (flow) =>
        Schema.encode(PendingFlow)(flow).pipe(
          Effect.orDie,
          Effect.flatMap((encoded) => Effect.promise(() => set(PENDING_FLOW_KEY, encoded, stores["auth-flow"]))),
        ),
      take: Effect.promise(() => get<typeof PendingFlow.Encoded>(PENDING_FLOW_KEY, stores["auth-flow"])).pipe(
        Effect.map(Option.fromNullable),
        Effect.flatMap(Option.match({
          onNone: () => Effect.succeed(Option.none<PendingFlow>()),
          onSome: (raw) => Schema.decodeUnknown(PendingFlow)(raw).pipe(Effect.map(Option.some), Effect.orElseSucceed(() => Option.none<PendingFlow>())),
        })),
      ),
      clear: Effect.promise(() => del(PENDING_FLOW_KEY, stores["auth-flow"])),
    },
  }),
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------
export { type LocalPersistence, type PendingFlow, type QueueItem, type StoreDomain, LocalPersistenceLive };
```
