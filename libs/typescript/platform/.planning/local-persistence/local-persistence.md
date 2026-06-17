# [PLATFORM_LOCAL_PERSISTENCE]

One page owns the single browser-local store — `LocalPersistence`, the Schema-encoded last-good snapshot and the offline command queue over the Effect `KeyValueStore` (`idb-keyval` backing), the one owner the offline-redial drain reads. A snapshot persists as a Schema-encoded value and the offline command queue drains in order on redial; a hand-rolled `localStorage` read or a JSON-stringified blob outside the Schema-encoded store is the deleted form.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]         | [OWNS]                                                       |
| :-----: | :---------------- | :--------------------------------------------------------- |
|   [1]   | LOCAL_PERSISTENCE | the last-good snapshot store and the offline command queue   |

## [2]-[LOCAL_PERSISTENCE]

- Owner: `LocalPersistence`, the last-good-snapshot and offline-command-queue store over the browser key-value binding.
- Cases: `LocalPersistence` holds the Schema-encoded last-good store snapshots and the offline command queue so a redial restores from the last good state rather than a cold boot; the offline-queue element is the resolved-intent pair `{ verb: ControlVerb; payload: CommandPayloadWire }` — the same pair `interchange` `IntentRegistry.resolve` yields — so the persisted element carries the verb the `interchange` `CommandGateway.invoke` requires and the redial drain re-dials a resolved intent verbatim, never a verb re-derived off the payload.
- Auto: `LocalPersistence` is the `@effect/platform-browser` `KeyValueStore`/`BrowserKeyValueStore` surface composing `idb-keyval` as the BACKING store only — never a hand-rolled `idb-keyval` call — so a snapshot persists as a Schema-encoded value and the offline command queue drains in order on redial; a hand-rolled `localStorage` read or a JSON-stringified blob outside the Schema-encoded store is the deleted form.
- Packages: `effect` for the `Schema` encode/decode and the `Option` last-good carrier; `@effect/platform` and `@effect/platform-browser` for the `KeyValueStore`/`BrowserKeyValueStore` surface; `idb-keyval` as the IndexedDB backing store under the KV abstraction.
- Growth: a new persisted store lands as one `LocalPersistence` key-value row, never a parallel store.
- Boundary: `LocalPersistence` is the single browser-local store and a direct `localStorage`/`IndexedDB`/`idb-keyval` access outside it is the named defect; the offline-queue drain reaches the `interchange` `CommandGateway` across the intra-package folder seam (the `offline-cache` `BackgroundSyncReplay` owns the SW-triggered redial drive over this same `LocalPersistence.offlineQueue`) and never re-dials a transport here; the queue element shape is owned here as the resolved-intent pair, so `BackgroundSyncReplay` consumes `drainOnRedial`/`enqueue` over that pair verbatim with no re-derivation; the key-value backing store is composed over `runtime-composition`'s `BrowserPlatform` binding, never a second store binding.

```ts contract
interface LocalPersistence {
  readonly store: KeyValueStore.KeyValueStore;
  readonly lastGood: <A, I, R>(schema: Schema.Schema<A, I, R>, key: string) => {
    readonly save: (value: A) => Effect.Effect<void, ParseResult.ParseError, R>;
    readonly load: Effect.Effect<Option.Option<A>, ParseResult.ParseError, R>;
  };
  readonly offlineQueue: {
    readonly enqueue: (item: { readonly verb: ControlVerb; readonly payload: CommandPayloadWire }) => Effect.Effect<void>;
    readonly drainOnRedial: Effect.Effect<ReadonlyArray<{ readonly verb: ControlVerb; readonly payload: CommandPayloadWire }>>;
  };
}
```
