# [BROWSER_KV]

`persist/kv.ts` is the one `idb-keyval` site in the branch: a closed `_domains` vocabulary maps each persisted concern to its own named IndexedDB store and its owning value `Schema`, and one polymorphic lane surface carries every operation — point read, write, in-transaction mutate, drop, and the atomic drain — over `Effect.tryPromise` conversions into one `KvFault` rail. Values cross the boundary Schema-encoded to structured-cloneable shapes and decode on read, so the lane's public surface is domain-typed while the stored bytes stay canonical; a direct `idb-keyval`/`indexedDB`/`localStorage` call outside this owner, a key-prefix convention inside one flat store, or a JSON string smuggled past the codec is the named flat-store defect. This lane is the overlay cache and the durable outbox — never the record of truth, which is `persist/opfs.ts`'s local-first law.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                            | [PUBLIC]       |
| :-----: | :-------------- | :------------------------------------------------------------------ | :------------- |
|  [01]   | `DOMAIN_ROWS`   | the store-per-domain vocabulary and each domain's value schema      | `Kv` (types)   |
|  [02]   | `LANE_SURFACE`  | the typed operation family, the codec seam, the atomic drain        | `Kv`, `KvFault`|

## [2]-[DOMAIN_ROWS]

[DOMAIN_ROWS]:
- Owner: the interior `_domains` anchor — one row per persisted concern, each carrying its value schema: `outbox` (the durable replay entries `shell/worker` drains — `Kv.Value<"outbox">` rows of minted-at plus opaque payload band), `flow` (the single pending ceremony record `session/ceremony` persists across a full-page redirect), `route` (the last-good serialized query string per route key `route/navigate` restores on cold boot), `cache` (content-keyed byte bands and manifests `transport/pool` warms from), `mark` (watermarks — last sync instant, wake posture, boot count).
- Law: one domain, one named store — every row mints `createStore("rasm-" + domain, domain)` once at service construction (the Layer build, never module load, so importing the module opens nothing), IndexedDB transaction isolation holds per domain, a `clear` on one domain can never evict another, and the store roster is a value; the row guard closes the set and a new persisted concern is exactly one row.
- Law: the operation family materializes as one lane per row — `_lane(domain, schema)` captures the store and codecs monomorphically, `_lanes` is the mapped-contract record (`{ [D in Domain]: Kv.Lane<D> }`), and the service members are generic indexed dispatch over it, so every per-domain signature correlates cast-free under the handler-record law.
- Law: the row's schema is the domain's whole type authority — `Kv.Value<D>` derives by indexed access over the row table, so every lane operation is domain-typed with zero call-site type arguments and a value that fails decode on read is `KvFault.codec` evidence, never a silently trusted blob.
- Law: payload bands stay opaque here — the `outbox` and `cache` rows carry `Uint8Array` bands the owning producer already encoded; this page never re-decodes a sibling's interior, it stores and returns bytes verbatim under the domain schema.
- Growth: a new domain is one `_domains` row; a new facet on a domain's value is one field on its schema — consumers break loudly at decode until aligned.
- Boundary: which entries enter the `outbox` is `shell/worker`'s replay law; which manifests warm the `cache` is `transport/pool`'s; this page owns residency and atomicity only.
- Packages: `idb-keyval` (`createStore`, `UseStore`); `effect` (`Schema`).

## [3]-[LANE_SURFACE]

[LANE_SURFACE]:
- Owner: `Kv`, one `Effect.Service` whose members are domain-generic over the row table — `read(domain, key)` yields `Option<Kv.Value<D>>` with absence as `Option.none`; `write(domain, key, value)` encodes then stores; `mutate(domain, key, step)` runs the read-modify-write inside one IndexedDB transaction with a synchronous `step` from `Option<Kv.Value<D>>` to the successor value, so the transaction never spans an await and the delete modality stays `drop`'s; `drop(domain, key | keys)` discriminates single from batch on the input shape and deletes atomically; `drain(domain)` is the atomic `entries`-then-`clear` — a mid-drain crash leaves the whole queue or empties it, never a half-applied tear; `wipe(domain)` resets one store.
- Law: every rejection converts at this seam — `Effect.tryPromise` folds the `DOMException` family into `KvFault` rows (`quota` for exceeded storage, `absent` for a host without `indexedDB`, `io` for the remainder) and decode skew folds to `codec` carrying the parse detail; no raw promise or thrown value escapes, and the policy table's `retry` column tells recovery which rows re-drive.
- Law: the codec seam is the write and read boundary — `Schema.encode` runs before `set`, `Schema.decodeUnknown` after `get` and the drain, and the `mutate` step operates on decoded values with the encode re-applied inside the same transaction closure; a held value failing decode inside `mutate` folds to absence so the synchronous step overwrites the poisoned cell — read and drain alone surface `codec` evidence — and a consumer never meets a stored representation.
- Law: `mutate` rides `idb-keyval`'s `update` so concurrent writers serialize inside IndexedDB's own transaction — a `read`-then-`write` pair re-spelling it is the torn-write defect; `drain` runs read-and-clear inside ONE `readwrite` transaction through the `UseStore` closure and `promisifyRequest`, awaiting the transaction itself before any entry is handed out, so a write racing the drain lands wholly before or wholly after it and a cleared queue is a committed fact.
- Law: an entry failing decode after a drain is `codec` evidence and cannot re-enter the store — the producing writer is the defect site, and the fault carries the detail forensics need.
- Entry: the service's six members are the whole surface; `R` carries nothing — the store handles are construction facts.
- Boundary: `@effect/platform`'s `KeyValueStore` Tag stays unbound here by design — its browser binding is Web-Storage-backed and carries no IndexedDB layer, so the durable lane is direct `idb-keyval` under this one owner; the EventLog journal's own IndexedDB database is `persist/opfs`'s and never shares these stores.
- Packages: `idb-keyval` (`get`, `set`, `update`, `del`, `delMany`, `clear`, `promisifyRequest`, `createStore`); `effect` (`Effect`, `Option`, `Schema`, `Data`, `ParseResult`, `Predicate`).

```typescript
import { Data, Effect, Option, type ParseResult, Predicate, Schema } from "effect"
import { clear, createStore, del, delMany, get, promisifyRequest, set, update, type UseStore } from "idb-keyval"

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

const KvFaultPolicy = {
  quota: { rank: 4, retry: false },
  absent: { rank: 5, retry: false },
  codec: { rank: 3, retry: false },
  io: { rank: 2, retry: true },
} as const

declare namespace Kv {
  type Domain = keyof typeof _domains
  type Value<D extends Domain = Domain> = Schema.Schema.Type<(typeof _domains)[D]>
  type Lane<D extends Domain = Domain> = {
    readonly read: (key: string) => Effect.Effect<Option.Option<Value<D>>, KvFault>
    readonly write: (key: string, value: Value<D>) => Effect.Effect<void, KvFault>
    readonly mutate: (key: string, step: (held: Option.Option<Value<D>>) => Value<D>) => Effect.Effect<void, KvFault>
    readonly drop: (keys: string | ReadonlyArray<string>) => Effect.Effect<void, KvFault>
    readonly drain: Effect.Effect<ReadonlyArray<readonly [string, Value<D>]>, KvFault>
    readonly wipe: Effect.Effect<void, KvFault>
  }
  type _Rows<T extends Record<Domain, Schema.Schema.Any> = typeof _domains> = T
}

declare namespace KvFault {
  type Reason = keyof typeof KvFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean }
  type _Rows<T extends Record<Reason, Row> = typeof KvFaultPolicy> = T
}

class KvFault extends Data.TaggedError("KvFault")<{
  readonly reason: KvFault.Reason
  readonly domain: Kv.Domain
  readonly detail: string
}> {
  get policy(): KvFault.Row {
    return KvFaultPolicy[this.reason]
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
  return {
    read: (key: string) =>
      lift((use) => get<unknown>(key, use)).pipe(
        Effect.flatMap((raw) =>
          Option.match(Option.fromNullable(raw), {
            onNone: () => Effect.succeedNone,
            onSome: (held) => Effect.asSome(Effect.mapError(decode(held), _decoded(domain))),
          }),
        ),
      ),
    write: (key: string, value: A) =>
      encode(value).pipe(
        Effect.mapError(_decoded(domain)),
        Effect.flatMap((encoded) => lift((use) => set(key, encoded, use))),
      ),
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

const _service = (lanes: { readonly [D in Kv.Domain]: Kv.Lane<D> }) => ({
  read: <D extends Kv.Domain>(domain: D, key: string): Effect.Effect<Option.Option<Kv.Value<D>>, KvFault> =>
    lanes[domain].read(key),
  write: <D extends Kv.Domain>(domain: D, key: string, value: Kv.Value<D>): Effect.Effect<void, KvFault> =>
    lanes[domain].write(key, value),
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
})

class Kv extends Effect.Service<Kv>()("browser/persist/Kv", {
  sync: () =>
    _service({
      outbox: _lane("outbox", _domains.outbox),
      flow: _lane("flow", _domains.flow),
      route: _lane("route", _domains.route),
      cache: _lane("cache", _domains.cache),
      mark: _lane("mark", _domains.mark),
    }),
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Kv, KvFault }
```
