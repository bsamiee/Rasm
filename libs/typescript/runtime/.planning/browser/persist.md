# [RUNTIME_PERSIST]

The local-persistence plane and the one `idb-keyval` site in the branch: a closed `_domains` vocabulary maps each persisted concern to its own named IndexedDB store and its owning value `Schema`, and one polymorphic lane surface carries every operation — point and batch read, point and atomic-batch write, in-transaction mutate, drop, the atomic drain, and the wipe — over `Effect.tryPromise` conversions into one class-carried `KvFault` rail. Values cross the boundary Schema-encoded to structured-cloneable shapes and decode on read, so the lane's public surface is domain-typed while the stored bytes stay canonical; a direct `idb-keyval`/`indexedDB`/`localStorage` call outside this owner, a key-prefix convention inside one flat store, or a JSON string smuggled past the codec is the named flat-store defect. The page also owns the durable-band residency verdicts over the native `StorageManager` — the persistence grant, the quota estimate, and the closed pressure vocabulary every local-durability decision dispatches on — and the browser half of the local-first arrangement: the `EventLog` overlay client backings and the sqlite-wasm lane seam. The overlay law is absolute: the `EventLog` client accelerates local-first reads and offline capture; the record of truth is the data journal, and a value whose loss corrupts state never lives only here. The sqlite-wasm lane meets the browser at the composition root — the data folder owns the OPFS driver on its wasm profile row (`lane/sqlite`), this folder imports no sql surface and contributes the residency verdicts that lane's health gate reads. The module is `runtime/src/browser/persist.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                            | [PUBLIC]        |
| :-----: | :----------------- | :------------------------------------------------------------------ | :-------------- |
|  [01]   | `DOMAIN_ROWS`      | the store-per-domain vocabulary and each domain's value schema      | `Kv` (types)    |
|  [02]   | `LANE_SURFACE`     | the typed operation family, the codec seam, the atomic drain        | `Kv`, `KvFault` |
|  [03]   | `STORAGE_RESIDENCY`| the grant, the estimate, the pressure-verdict vocabulary            | `Opfs`          |
|  [04]   | `OVERLAY_AND_LANE` | the EventLog browser backings, the sync row, the wasm-lane seam law | `Overlay`       |

## [2]-[DOMAIN_ROWS]

[DOMAIN_ROWS]:
- Owner: the interior `_domains` anchor — one row per persisted concern, each carrying its value schema: `outbox` (the durable replay entries `shell#REPLAY_DRAIN` drains — rows of minted-at plus opaque payload band), `flow` (the single pending redirect record `route#SESSION_PLANE` persists across a full-page departure), `route` (the last-good serialized query string per route key `route#TRAVERSAL_OWNER` restores on cold boot), `cache` (content-keyed byte bands `fetch#DEPOT_SCHEDULER` warms from), `mark` (watermarks — last sync instant, wake posture, boot count).
- Law: one domain, one named store — every row mints `createStore("rasm-" + domain, domain)` once at service construction (the Layer build, never module load, so importing the module opens nothing), IndexedDB transaction isolation holds per domain, a `clear` on one domain can never evict another, and the store roster is a value; the row guard closes the set and a new persisted concern is exactly one row.
- Law: the operation family materializes as one lane per row — `_lane(domain, schema)` captures the store and codecs monomorphically, `_lanes` is the mapped-contract record (`{ [D in Domain]: Kv.Lane<D> }`), and the service members are generic indexed dispatch over it, so every per-domain signature correlates cast-free under the handler-record law.
- Law: the row's schema is the domain's whole type authority — `Kv.Value<D>` derives by indexed access over the row table, so every lane operation is domain-typed with zero call-site type arguments and a value that fails decode on read is `codec` evidence, never a silently trusted blob.
- Law: payload bands stay opaque here — the `outbox` and `cache` rows carry `Uint8Array` bands the owning producer already encoded; this page never re-decodes a sibling's interior, it stores and returns bytes verbatim under the domain schema.
- Growth: a new domain is one `_domains` row; a new facet on a domain's value is one field on its schema — consumers break loudly at decode until aligned.
- Boundary: which entries enter the `outbox` is `shell#REPLAY_DRAIN`'s law; which bands warm the `cache` is `fetch#DEPOT_SCHEDULER`'s; this page owns residency and atomicity only.
- Packages: `idb-keyval` (`createStore`, `UseStore`); `effect` (`Schema`).

## [3]-[LANE_SURFACE]

[LANE_SURFACE]:
- Owner: `Kv`, one `Effect.Service` whose members are domain-generic over the row table — `read(domain, key)` yields `Option<Kv.Value<D>>` with absence as `Option.none`, and `read(domain, keys)` is the batch modality over `getMany`, one transaction answering the whole set positionally; `write(domain, key, value)` encodes then stores, and `write(domain, entries)` is the atomic batch over `setMany` — all entries land or none do, the compensation and hydrate spelling; `mutate(domain, key, step)` runs the read-modify-write inside one IndexedDB transaction with a synchronous `step`, so the transaction never spans an await; `drop(domain, key | keys)` discriminates single from batch on the input shape and deletes atomically; `drain(domain)` is the atomic scan-then-clear — a mid-drain crash leaves the whole queue or empties it, never a half-applied tear; `wipe(domain)` resets one store.
- Law: modality follows the input shape — a string is the point call, an array is the batch call, and the batch rows ride the library's own atomic multi-entry transactions (`getMany`, `setMany`, `delMany`); N point round-trips where one batch transaction answers is the named defect the batch modalities delete.
- Law: every rejection converts at this seam — `Effect.tryPromise` folds the `DOMException` family into `KvFault` rows (`quota` for exceeded storage, `absent` for a host without `indexedDB`, `io` for the remainder) and decode skew folds to `codec` carrying the parse detail; no raw promise or thrown value escapes, and `KvFault.class` projects each reason into the core `FaultClass` vocabulary so budget gates read the one branch taxonomy.
- Law: the codec seam is the write and read boundary — `Schema.encode` runs before `set`/`setMany`, `Schema.decodeUnknown` after `get`/`getMany` and the drain, and the `mutate` step operates on decoded values with the encode re-applied inside the same transaction closure; a held value failing decode inside `mutate` folds to absence so the synchronous step overwrites the poisoned cell — read and drain alone surface `codec` evidence — and a consumer never meets a stored representation.
- Law: `mutate` rides `idb-keyval`'s `update` so concurrent writers serialize inside IndexedDB's own transaction — a `read`-then-`write` pair re-spelling it is the torn-write defect; `drain` runs scan-and-clear inside ONE `readwrite` transaction through the `UseStore` closure and `promisifyRequest`, awaiting the transaction itself before any entry is handed out, so a write racing the drain lands wholly before or wholly after it and a cleared queue is a committed fact.
- Law: an entry failing decode after a drain is `codec` evidence and cannot re-enter the store — the producing writer is the defect site, and the fault carries the detail forensics need.
- Entry: the service's six members are the whole surface; `R` carries nothing — the store handles are construction facts.
- Boundary: `@effect/platform`'s `KeyValueStore` Tag stays unbound here by design — its browser binding is Web-Storage-backed and carries no IndexedDB layer, so the durable lane is direct `idb-keyval` under this one owner; the EventLog journal's own IndexedDB database is `[5]`'s and never shares these stores.
- Packages: `idb-keyval` (`get`, `getMany`, `set`, `setMany`, `update`, `del`, `delMany`, `clear`, `promisifyRequest`, `createStore`); `effect` (`Data`, `Effect`, `Option`, `ParseResult`, `Predicate`, `Schema`); `@rasm/ts/core` (`FaultClass`).

```typescript
import { FaultClass } from "@rasm/ts/core"
import { Data, Effect, Layer, Option, type ParseResult, Predicate, Schema } from "effect"
import { clear, createStore, del, delMany, get, getMany, promisifyRequest, set, setMany, update, type UseStore } from "idb-keyval"

const _domains = {
  outbox: Schema.Struct({ minted: Schema.DateTimeUtc, band: Schema.Uint8ArrayFromSelf }),
  flow: Schema.parseJson(Schema.Struct({
    state: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    returnTo: Schema.String,
    minted: Schema.DateTimeUtc,
  })),
  route: Schema.String,
  cache: Schema.Uint8ArrayFromSelf,
  mark: Schema.parseJson(Schema.Struct({ at: Schema.DateTimeUtc, note: Schema.String })),
} as const

const _kvFaults = {
  quota: { class: "exhausted" },
  absent: { class: "absent" },
  codec: { class: "malformed" },
  io: { class: "unavailable" },
} as const

declare namespace Kv {
  type Domain = keyof typeof _domains
  type Value<D extends Domain = Domain> = Schema.Schema.Type<(typeof _domains)[D]>
  type Entries<D extends Domain = Domain> = ReadonlyArray<readonly [string, Value<D>]>
  type Lane<D extends Domain = Domain> = {
    readonly read: {
      (key: string): Effect.Effect<Option.Option<Value<D>>, KvFault>
      (keys: ReadonlyArray<string>): Effect.Effect<ReadonlyArray<Option.Option<Value<D>>>, KvFault>
    }
    readonly write: {
      (key: string, value: Value<D>): Effect.Effect<void, KvFault>
      (entries: Entries<D>): Effect.Effect<void, KvFault>
    }
    readonly mutate: (key: string, step: (held: Option.Option<Value<D>>) => Value<D>) => Effect.Effect<void, KvFault>
    readonly drop: (keys: string | ReadonlyArray<string>) => Effect.Effect<void, KvFault>
    readonly drain: Effect.Effect<ReadonlyArray<readonly [string, Value<D>]>, KvFault>
    readonly wipe: Effect.Effect<void, KvFault>
  }
  type _Rows<T extends Record<Domain, Schema.Schema.Any> = typeof _domains> = T
}

declare namespace KvFault {
  type Reason = keyof typeof _kvFaults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _kvFaults> = T
}

class KvFault extends Data.TaggedError("KvFault")<{
  readonly reason: KvFault.Reason
  readonly domain: Kv.Domain
  readonly detail: string
}> {
  get class(): FaultClass.Kind {
    return _kvFaults[this.reason].class
  }
}

const _faulted = (domain: Kv.Domain) => (cause: unknown): KvFault =>
  "indexedDB" in globalThis === false
    ? new KvFault({ reason: "absent", domain, detail: String(cause) })
    : cause instanceof DOMException && cause.name === "QuotaExceededError"
      ? new KvFault({ reason: "quota", domain, detail: cause.message })
      : new KvFault({ reason: "io", domain, detail: String(cause) })

const _decoded = (domain: Kv.Domain) => (fault: ParseResult.ParseError): KvFault =>
  new KvFault({ reason: "codec", domain, detail: String(fault) })

const _lane = <A, I>(domain: Kv.Domain, schema: Schema.Schema<A, I>) => {
  const store = createStore(`rasm-${domain}`, domain)
  const lift = <T>(run: (use: UseStore) => Promise<T>): Effect.Effect<T, KvFault> =>
    Effect.tryPromise({ try: () => run(store), catch: _faulted(domain) })
  const decode = Schema.decodeUnknown(schema)
  const decodeOption = Schema.decodeUnknownOption(schema)
  const encode = Schema.encode(schema)
  const encodeSync = Schema.encodeSync(schema)
  const _admit = (raw: unknown): Effect.Effect<Option.Option<A>, KvFault> =>
    Option.match(Option.fromNullable(raw), {
      onNone: () => Effect.succeedNone,
      onSome: (held) => Effect.asSome(Effect.mapError(decode(held), _decoded(domain))),
    })
  function read(key: string): Effect.Effect<Option.Option<A>, KvFault>
  function read(keys: ReadonlyArray<string>): Effect.Effect<ReadonlyArray<Option.Option<A>>, KvFault>
  function read(input: string | ReadonlyArray<string>) {
    return Predicate.isString(input)
      ? Effect.flatMap(lift((use) => get<unknown>(input, use)), _admit)
      : Effect.flatMap(lift((use) => getMany<unknown>([...input], use)), Effect.forEach(_admit))
  }
  function write(key: string, value: A): Effect.Effect<void, KvFault>
  function write(entries: ReadonlyArray<readonly [string, A]>): Effect.Effect<void, KvFault>
  function write(input: string | ReadonlyArray<readonly [string, A]>, value?: A) {
    return Predicate.isString(input)
      ? encode(value as A).pipe(
          Effect.mapError(_decoded(domain)),
          Effect.flatMap((encoded) => lift((use) => set(input, encoded, use))),
        )
      : Effect.forEach(input, ([key, held]) =>
          Effect.map(Effect.mapError(encode(held), _decoded(domain)), (encoded) => [key, encoded] as [IDBValidKey, unknown]),
        ).pipe(Effect.flatMap((rows) => lift((use) => setMany([...rows], use))))
  }
  return {
    read,
    write,
    mutate: (key: string, step: (held: Option.Option<A>) => A) =>
      lift((use) =>
        update<unknown>(key, (raw) => encodeSync(step(Option.flatMap(Option.fromNullable(raw), decodeOption))), use),
      ),
    drop: (keys: string | ReadonlyArray<string>) =>
      Predicate.isString(keys) ? lift((use) => del(keys, use)) : lift((use) => delMany([...keys], use)),
    drain: lift((use) =>
      use("readwrite", (raw) =>
        promisifyRequest(raw.getAllKeys()).then((keys) =>
          promisifyRequest(raw.getAll()).then((values) => {
            raw.clear()
            return promisifyRequest(raw.transaction).then(() =>
              keys.map((key, at) => [String(key), values[at] as unknown] as const),
            )
          }),
        ),
      ),
    ).pipe(
      Effect.flatMap(
        Effect.forEach(([key, raw]) =>
          decode(raw).pipe(
            Effect.mapError(_decoded(domain)),
            Effect.map((value) => [key, value] as const),
          ),
        ),
      ),
    ),
    wipe: lift((use) => clear(use)),
  }
}

const _service = (lanes: { readonly [D in Kv.Domain]: Kv.Lane<D> }) => {
  function read<D extends Kv.Domain>(domain: D, key: string): Effect.Effect<Option.Option<Kv.Value<D>>, KvFault>
  function read<D extends Kv.Domain>(domain: D, keys: ReadonlyArray<string>): Effect.Effect<ReadonlyArray<Option.Option<Kv.Value<D>>>, KvFault>
  function read<D extends Kv.Domain>(domain: D, input: string | ReadonlyArray<string>) {
    return lanes[domain].read(input as string)
  }
  function write<D extends Kv.Domain>(domain: D, key: string, value: Kv.Value<D>): Effect.Effect<void, KvFault>
  function write<D extends Kv.Domain>(domain: D, entries: Kv.Entries<D>): Effect.Effect<void, KvFault>
  function write<D extends Kv.Domain>(domain: D, input: string | Kv.Entries<D>, value?: Kv.Value<D>) {
    return Predicate.isString(input) ? lanes[domain].write(input, value as Kv.Value<D>) : lanes[domain].write(input)
  }
  return {
    read,
    write,
    mutate: <D extends Kv.Domain>(
      domain: D,
      key: string,
      step: (held: Option.Option<Kv.Value<D>>) => Kv.Value<D>,
    ): Effect.Effect<void, KvFault> => lanes[domain].mutate(key, step),
    drop: (domain: Kv.Domain, keys: string | ReadonlyArray<string>): Effect.Effect<void, KvFault> =>
      lanes[domain].drop(keys),
    drain: <D extends Kv.Domain>(domain: D): Effect.Effect<ReadonlyArray<readonly [string, Kv.Value<D>]>, KvFault> =>
      lanes[domain].drain,
    wipe: (domain: Kv.Domain): Effect.Effect<void, KvFault> => lanes[domain].wipe,
  }
}

class Kv extends Effect.Service<Kv>()("runtime/browser/Kv", {
  sync: () =>
    _service({
      outbox: _lane("outbox", _domains.outbox),
      flow: _lane("flow", _domains.flow),
      route: _lane("route", _domains.route),
      cache: _lane("cache", _domains.cache),
      mark: _lane("mark", _domains.mark),
    }),
}) {}
```

## [4]-[STORAGE_RESIDENCY]

[STORAGE_RESIDENCY]:
- Owner: `Opfs`, one `Effect.Service` over the native `StorageManager` — `retained` reads `navigator.storage.persisted()` (already granted), `retain` requests `navigator.storage.persist()` (granted now), and `budget` folds `navigator.storage.estimate()` into the `Opfs.Budget` receipt: usage, quota, headroom, and the verdict drawn from the closed `_BANDS` vocabulary (`ample`/`tight`/`critical` by usage fraction, `opaque` where the host withholds numbers).
- Law: the native calls are confined — `navigator.storage` is spelled only inside this owner, and a `persist`/`estimate` probe at any consumer is the named ungated-native-call defect; a host without the surface folds to `retained: false` and `opaque`, so capability absence is data.
- Law: the verdict is the one pressure vocabulary — `fetch#DEPOT_SCHEDULER` byte scheduling, this page's eviction posture, and the sqlite-wasm lane's health gate all dispatch on the verdict rows; a consumer comparing raw byte counts re-derives what the band table already decides, and a denied grant is the signal that every durable band risks eviction under pressure.
- Receipt: `Opfs.Budget` carries the numbers beside the verdict so telemetry stamps evidence while consumers dispatch on the row.
- Growth: a new pressure band is one `_BANDS` row; a new residency fact (a bucket API, a durability probe) is one member on this owner.
- Packages: `effect` (`Effect`, `Option`).

```typescript
const _BANDS = { ample: 0.5, tight: 0.85, critical: 1 } as const

declare namespace Opfs {
  type Verdict = keyof typeof _BANDS | "opaque"
  type Budget = {
    readonly usage: Option.Option<number>
    readonly quota: Option.Option<number>
    readonly headroom: Option.Option<number>
    readonly verdict: Verdict
  }
  type _Rows<T extends Record<keyof typeof _BANDS, number> = typeof _BANDS> = T
}

type _StorageHost = Navigator & {
  readonly storage?: {
    readonly persisted: () => Promise<boolean>
    readonly persist: () => Promise<boolean>
    readonly estimate: () => Promise<{ readonly usage?: number; readonly quota?: number }>
  }
}

const _storage = (): Option.Option<NonNullable<_StorageHost["storage"]>> =>
  Option.fromNullable((globalThis.navigator as _StorageHost).storage)

const _verdict = (usage: number, quota: number): Opfs.Verdict =>
  quota <= 0
    ? "opaque"
    : usage / quota < _BANDS.ample
      ? "ample"
      : usage / quota < _BANDS.tight
        ? "tight"
        : "critical"

const _VOID_BUDGET: Opfs.Budget = {
  usage: Option.none(),
  quota: Option.none(),
  headroom: Option.none(),
  verdict: "opaque",
}

class Opfs extends Effect.Service<Opfs>()("runtime/browser/Opfs", {
  sync: () => ({
    retained: Option.match(_storage(), {
      onNone: () => Effect.succeed(false),
      onSome: (storage) => Effect.orElseSucceed(Effect.tryPromise(() => storage.persisted()), () => false),
    }),
    retain: Option.match(_storage(), {
      onNone: () => Effect.succeed(false),
      onSome: (storage) => Effect.orElseSucceed(Effect.tryPromise(() => storage.persist()), () => false),
    }),
    budget: Option.match(_storage(), {
      onNone: () => Effect.succeed(_VOID_BUDGET),
      onSome: (storage) =>
        Effect.orElseSucceed(
          Effect.map(Effect.tryPromise(() => storage.estimate()), (held): Opfs.Budget => {
            const usage = Option.fromNullable(held.usage)
            const quota = Option.fromNullable(held.quota)
            return {
              usage,
              quota,
              headroom: Option.zipWith(quota, usage, (all, used) => all - used),
              verdict: Option.match(Option.zipWith(usage, quota, _verdict), {
                onNone: (): Opfs.Verdict => "opaque",
                onSome: (band) => band,
              }),
            }
          }),
          () => _VOID_BUDGET,
        ),
    }),
  }),
  accessors: true,
}) {}
```

## [5]-[OVERLAY_AND_LANE]

[OVERLAY_AND_LANE]:
- Owner: `Overlay` — the browser backing rows the `@effect/experimental` EventLog client requires, assembled once: `Overlay.backing(spec)` merges the IndexedDB journal (`EventJournal.layerIndexedDb`, its own database, never a `[3]` store), the client identity over Web Storage (`EventLog.layerIdentityKvs` satisfied by `BrowserKeyValueStore.layerLocalStorage`), and the `Reactivity` bus; `Overlay.sync(url)` is the self-contained browser sync row (`EventLogRemote.layerWebSocketBrowser` — WebSocket plus Web Crypto E2E, requiring only the built `EventLog`).
- Law: the event universe is app data — the app declares its `Event`/`EventGroup` families, freezes them with `EventLog.schema`, and composes `EventLog.layer(schema)` plus its group-handler registrations over this page's backings at the root; the lib ships backings and law, never an event vocabulary, so hundreds of apps ride one overlay spelling.
- Law: overlay, never authority — the journal is append-only capture and the reducers fold local reads; anything durable-critical projects from or mirrors to the data journal through the sync server the edge mounts, and a lane holding sole custody of critical state is the named boundary breach.
- Law: the sqlite-wasm lane seam — heavier local read models than the reducer folds ride the data-owned OPFS driver on `lane/sqlite`'s wasm profile row: the data folder publishes the wasm `SqlClient` Layer, the app root provides it beneath the app's read models, and this page's `retain`/`budget` verdicts gate whether the lane opens at all (a `critical` verdict or a refused grant demotes the app to the kv/overlay tier); the wasm profile's degradation verdicts — `originScope` tenancy, `singleTab` writer, `reactivityHooks` change delivery — are `lane/sqlite`'s rows, and no `@effect/sql*` import exists in this folder.
- Law: local-first boot order is fixed — `retain` first (durability grant before bytes land), backings next, sync last; a sync row without a journal is unbuildable by the requirement channel, which is the assembly proof.
- Growth: a second sync transport (a socket-constructor row for a shared worker) is one row beside `sync`; a journal swap (memory for specs) is Layer substitution at the root, never an edit here.
- Boundary: the server half — storage, encryption at rest, the mountable sync handler — is the data/edge waves' material; compaction and reactivity keys are the app's group declarations.
- Packages: `@effect/experimental` (`EventJournal`, `EventLog`, `EventLogRemote`, `Reactivity`); `@effect/platform-browser` (`BrowserKeyValueStore`); `effect` (`Layer`).

```typescript
import { EventJournal, EventLog, EventLogRemote, Reactivity } from "@effect/experimental"
import { BrowserKeyValueStore } from "@effect/platform-browser"

declare namespace Overlay {
  type Spec = {
    readonly database: string
    readonly identity: string
  }
}

const Overlay: {
  readonly backing: (
    spec: Overlay.Spec,
  ) => Layer.Layer<EventJournal.EventJournal | EventLog.Identity | Reactivity.Reactivity>
  readonly sync: (url: string) => Layer.Layer<never, never, EventLog.EventLog>
} = {
  backing: (spec) =>
    Layer.mergeAll(
      EventJournal.layerIndexedDb({ database: spec.database }),
      Layer.provide(
        EventLog.layerIdentityKvs({ key: spec.identity }),
        BrowserKeyValueStore.layerLocalStorage,
      ),
      Reactivity.layer,
    ),
  sync: (url) => EventLogRemote.layerWebSocketBrowser(url),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Kv, KvFault, Opfs, Overlay }
```
