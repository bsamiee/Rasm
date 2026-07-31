# [RUNTIME_PERSIST]

The local-persistence plane and the one `idb-keyval` site in the branch: a closed `_domains` vocabulary maps each persisted concern to its own named IndexedDB store and its owning value `Schema`, and one polymorphic lane surface carries every operation — point and batch read, point and atomic-batch write, in-transaction mutate, drop, the atomic drain, and the wipe — over `Effect.tryPromise` conversions into one class-carried `KvFault` rail. Values cross the boundary Schema-encoded to structured-cloneable shapes and decode on read, so the lane's public surface is domain-typed while the stored bytes stay canonical; a direct `idb-keyval`/`indexedDB`/`localStorage` call outside this owner, a key-prefix convention inside one flat store, or a JSON string smuggled past the codec is the named flat-store defect. The page also owns the durable-band residency verdicts over the native `StorageManager` — the persistence grant, the quota estimate, and the closed pressure vocabulary every local-durability decision dispatches on — the file-egress route ladder the ui export capability binds to at composition, and the browser half of the local-first arrangement: the `EventLog` overlay client backings and the sqlite-wasm lane seam. The overlay law is absolute: the `EventLog` client accelerates local-first reads and offline capture; the record of truth is the data journal, and a value whose loss corrupts state never lives only here. The sqlite-wasm lane meets the browser at the composition root — the data folder owns the OPFS driver on its wasm profile row (`lane/sqlite`), this folder imports no sql surface and contributes the residency verdicts that lane's health gate reads. The module is `runtime/src/browser/persist.ts`.

## [01]-[INDEX]

- [02]-[DOMAIN_ROWS]: the store-per-domain vocabulary and each domain's value schema; `Kv` (types).
- [03]-[LANE_SURFACE]: the typed operation family, the codec seam, the atomic drain; `Kv`, `KvFault`.
- [04]-[STORAGE_RESIDENCY]: the grant, the estimate, the pressure-verdict vocabulary, the file-egress routes; `Opfs`, `Egress`.
- [05]-[OVERLAY_AND_LANE]: the EventLog browser backings, the sync row, the wasm-lane seam law; `Overlay`.

## [02]-[DOMAIN_ROWS]

[DOMAIN_ROWS]:
- Owner: the interior `_domains` anchor — one row per persisted concern, each carrying its value schema: `outbox` (the durable replay entries `shell#REPLAY_DRAIN` drains — rows of minted-at plus opaque payload band), `flow` (the single pending redirect record `route#SESSION_PLANE` persists across a full-page departure), `route` (the last-good serialized query string per route key `route#TRAVERSAL_OWNER` restores on cold boot), `cache` (content-keyed byte bands `fetch#DEPOT_SCHEDULER` warms from), `mark` (watermarks — last sync instant, wake posture, boot count).
- Law: one domain, one named store — every row mints `createStore("rasm-" + domain, domain)` once at service construction (the Layer build, never module load, so importing the module opens nothing), IndexedDB transaction isolation holds per domain, a `clear` on one domain can never evict another, and the store roster is a value; the row guard closes the set and a new persisted concern is exactly one row.
- Law: the operation family materializes as one lane per row — `_lane(domain, schema)` captures the store and codecs monomorphically, `_lanes` is the mapped-contract record (`{ [D in Domain]: Kv.Lane<D> }`), and the service members are generic indexed dispatch over it, so every per-domain signature correlates cast-free under the handler-record law.
- Law: the row's schema is the domain's whole type authority — `Kv.Value<D>` derives by indexed access over the row table, so every lane operation is domain-typed with zero call-site type arguments and a value that fails decode on read is `codec` evidence, never a silently trusted blob.
- Law: payload bands stay opaque here — the `outbox` and `cache` rows carry `Uint8Array` bands the owning producer already encoded; this page never re-decodes a sibling's interior, it stores and returns bytes verbatim under the domain schema.
- Growth: a new domain is one `_domains` row; a new facet on a domain's value is one field on its schema — consumers break loudly at decode until aligned.
- Boundary: which entries enter the `outbox` is `shell#REPLAY_DRAIN`'s law; which bands warm the `cache` is `fetch#DEPOT_SCHEDULER`'s; this page owns residency and atomicity only.
- Packages: `idb-keyval` (`createStore`, `UseStore`); `effect` (`Schema`).

## [03]-[LANE_SURFACE]

[LANE_SURFACE]:
- Owner: `Kv`, one `Effect.Service` whose members are domain-generic over the row table — `read(domain, key)` yields `Option<Kv.Value<D>>` with absence as `Option.none`, and `read(domain, keys)` is the batch modality over `getMany`, one transaction answering the whole set positionally; `write(domain, key, value)` encodes then stores, and `write(domain, entries)` is the atomic batch over `setMany` — all entries land or none do, the compensation and hydrate spelling; `mutate(domain, key, step)` runs the read-modify-write inside one IndexedDB transaction with a synchronous `step`, so the transaction never spans an await; `drop(domain, key | keys)` discriminates single from batch on the input shape and deletes atomically; `size(domain)` counts keys without decoding values; `drain(domain)` is the atomic scan-then-clear — a mid-drain crash leaves the whole queue or empties it, never a half-applied tear; `wipe(domain)` resets one store.
- Law: modality follows the input shape — a string is the point call, an array is the batch call, and the batch rows ride the library's own atomic multi-entry transactions (`getMany`, `setMany`, `delMany`); N point round-trips where one batch transaction answers is the named defect the batch modalities delete.
- Law: every rejection converts at this seam — `Effect.tryPromise` folds the `DOMException` family into `KvFault` rows (`quota` for exceeded storage, `absent` for a host without `indexedDB`, `io` for the remainder) and decode skew folds to `codec` carrying the parse detail; no raw promise or thrown value escapes, and the family rides one core `FaultClass.family` mint whose frozen rows derive the reason type and the `class` projection together, so budget gates read the one branch taxonomy and no local guard pair or rank column exists beside it.
- Law: the codec seam is the write and read boundary — `Schema.encode` runs before `set`/`setMany`, `Schema.decodeUnknown` after `get`/`getMany` and the drain, and the `mutate` step operates on decoded values with the encode re-applied inside the same transaction closure; a held value failing decode inside `mutate` folds to absence so the synchronous step overwrites the poisoned cell — read and drain alone surface `codec` evidence — and a consumer never meets a stored representation.
- Law: `mutate` rides `idb-keyval`'s `update` so concurrent writers serialize inside IndexedDB's own transaction — a `read`-then-`write` pair re-spelling it is the torn-write defect; `drain` runs scan-and-clear inside ONE `readwrite` transaction through the `UseStore` closure and `promisifyRequest`, awaiting the transaction itself before any entry is handed out, so a write racing the drain lands wholly before or wholly after it and a cleared queue is a committed fact.
- Law: an entry failing decode after a drain is `codec` evidence and cannot re-enter the store — the producing writer is the defect site, and the fault carries the detail forensics need.
- Entry: the service's seven members are the whole surface; `R` carries nothing — the store handles are construction facts.
- Boundary: `@effect/platform`'s `KeyValueStore` Tag stays unbound here by design — its browser binding is Web-Storage-backed and carries no IndexedDB layer, so the durable lane is direct `idb-keyval` under this one owner; the EventLog journal's own IndexedDB database is `[5]`'s and never shares these stores.
- Packages: `idb-keyval` (`get`, `getMany`, `set`, `setMany`, `update`, `del`, `delMany`, `clear`, `keys`, `promisifyRequest`, `createStore`); `effect` (`Data`, `Effect`, `Option`, `ParseResult`, `Predicate`, `Schema`); `@rasm/ts/core` (`FaultClass`).

```typescript
import { FaultClass } from "@rasm/ts/core"
import { Data, Effect, Layer, Option, type ParseResult, Predicate, Schema } from "effect"
import { clear, createStore, del, delMany, get, getMany, keys, promisifyRequest, set, setMany, update, type UseStore } from "idb-keyval"

const _domains = {
  outbox: Schema.Struct({ minted: Schema.DateTimeUtc, band: Schema.Uint8ArrayFromSelf }),
  flow: Schema.parseJson(Schema.Struct({
    state: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    returnTo: Schema.String,
    minted: Schema.DateTimeUtc,
  })),
  route: Schema.String.pipe(Schema.startsWith("?")), // a last-good query is a non-empty search string; an empty query DROPS the key, so corrupted or vacuous rows refuse at decode
  cache: Schema.Uint8ArrayFromSelf,
  mark: Schema.parseJson(Schema.Struct({ at: Schema.DateTimeUtc, note: Schema.String })),
} as const

const _kvFamily = FaultClass.family(["quota", "absent", "codec", "io"] as const, {
  quota: { class: "exhausted" },
  absent: { class: "absent" },
  codec: { class: "malformed" },
  io: { class: "unavailable" },
})

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
    readonly size: Effect.Effect<number, KvFault>
    readonly drain: Effect.Effect<ReadonlyArray<readonly [string, Value<D>]>, KvFault>
    readonly wipe: Effect.Effect<void, KvFault>
  }
  type _Rows<T extends Record<Domain, Schema.Schema.Any> = typeof _domains> = T
}

declare namespace KvFault {
  type Reason = (typeof _kvFamily.reasons)[number]
}

class KvFault extends Data.TaggedError("KvFault")<{
  readonly reason: KvFault.Reason
  readonly domain: Kv.Domain
  readonly detail: string
}> {
  get class(): FaultClass.Kind {
    return _kvFamily.classOf(this.reason)
  }
  override get message(): string {
    return `<kv:${this.reason}> ${this.domain}: ${this.detail}`
  }
}

const _faulted = (domain: Kv.Domain) => (cause: unknown): KvFault =>
  !("indexedDB" in globalThis)
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
  const write = (...input: readonly [key: string, value: A] | readonly [entries: ReadonlyArray<readonly [string, A]>]) =>
    input.length === 2
      ? encode(input[1]).pipe(
          Effect.mapError(_decoded(domain)),
          Effect.flatMap((encoded) => lift((use) => set(input[0], encoded, use))),
        )
      : Effect.forEach(input[0], ([key, held]) =>
          Effect.map(Effect.mapError(encode(held), _decoded(domain)), (encoded) => [key, encoded] as [IDBValidKey, unknown]),
        ).pipe(Effect.flatMap((rows) => lift((use) => setMany([...rows], use))))
  return {
    read,
    write,
    mutate: (key: string, step: (held: Option.Option<A>) => A) =>
      lift((use) =>
        update<unknown>(key, (raw) => encodeSync(step(Option.flatMap(Option.fromNullable(raw), decodeOption))), use),
      ),
    drop: (keys: string | ReadonlyArray<string>) =>
      Predicate.isString(keys) ? lift((use) => del(keys, use)) : lift((use) => delMany([...keys], use)),
    size: Effect.map(lift((use) => keys(use)), (held) => held.length),
    drain: lift((use) =>
      use("readwrite", (raw) =>
        promisifyRequest(raw.getAllKeys()).then((keys) =>
          promisifyRequest(raw.getAll()).then((values) => {
            raw.clear()
            return promisifyRequest(raw.transaction).then(() =>
              keys.map((key, at) => [String(key), values[at]] as const),
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
    return Predicate.isString(input) ? lanes[domain].read(input) : lanes[domain].read(input) // the shape probe selects the lane overload; no cast crosses
  }
  const write = <D extends Kv.Domain>(
    domain: D,
    ...input: readonly [key: string, value: Kv.Value<D>] | readonly [entries: Kv.Entries<D>]
  ): Effect.Effect<void, KvFault> => input.length === 2
    ? lanes[domain].write(input[0], input[1])
    : lanes[domain].write(input[0])
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
    size: (domain: Kv.Domain): Effect.Effect<number, KvFault> => lanes[domain].size,
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

## [04]-[STORAGE_RESIDENCY]

[STORAGE_RESIDENCY]:
- Owner: `Opfs`, one `Effect.Service` over the native `StorageManager` — `retained` reads `navigator.storage.persisted()` (already granted), `retain` requests `navigator.storage.persist()` (granted now), and `budget` folds `navigator.storage.estimate()` into the `Opfs.Budget` receipt: usage, quota, headroom, and the verdict drawn from the closed `_BANDS` vocabulary (`ample`/`tight`/`critical` by usage fraction, `opaque` where the host withholds numbers).
- Law: the native calls are confined — `navigator.storage` is spelled only inside this owner, and a `persist`/`estimate` probe at any consumer is the named ungated-native-call defect; a host without the surface folds to `retained: false` and `opaque`, so capability absence is data.
- Owner: `Egress` — the file-egress capability VALUE the composition root binds to the ui-declared `Egress` Tag (`view/export`), because ui sits a stratum above and neither package imports the other: `Egress.save(file)` dispatches the one save over the routes the host actually carries, `Egress.share(file)` hands the same file to the host share sheet, `Egress.open(file)` yields the picker's own `WritableStream` for a payload no buffer holds, and `Egress.route` answers the whole carriage record — the save rung plus the share and stream capabilities — without performing anything, so a view renders its affordance off one read instead of probing a native. The root wraps this value member-for-member onto the ui Tag (`deliver(parcel, false)` → `save`, `deliver(parcel, true)` → `share`, `open` → `open`) and maps `EgressFault` onto the ui fault family at the same wrap — `absent` → `egress-absent`, `declined`/`io` → `egress-denied` — because a fault class cannot cross the strata either.
- Law: the picker surface is declared HERE or nowhere — `showSaveFilePicker` is absent from the pinned `lib.dom` and no ambient shim is installed, so `_SavePicker` is this owner's boundary refinement and the one place the native is spelled; a ui page reaching `globalThis.showSaveFilePicker` is the ungated-native-call defect the Tag exists to foreclose.
- Law: the route ladder is ordered by fidelity and total — the File System Access picker first (the user names the destination and the bytes stream to a real handle), the anchor download second (a host with a `document` but no picker), and a host carrying neither refuses `absent`, so capability absence is a typed refusal at the call rather than a silent no-op or a narrowed public surface. The share arm rides `navigator.share` gated by `canShare` over the constructed `File`, and the streaming arm rides the picker alone — an anchor needs the whole `Blob`, so a host without the picker refuses `open` as `absent` rather than buffering what the caller declared unbufferable.
- Law: a dismissed picker is `declined`, never `io` — the native answers an `AbortError` `DOMException` when the user closes the dialog, and folding that to a write failure would drive a retry against a decision the user already made; the same `DOMException` probe the `[03]` seam runs discriminates it.
- Law: the verdict is the one pressure vocabulary — `fetch#DEPOT_SCHEDULER` byte scheduling, this page's eviction posture, and the sqlite-wasm lane's health gate all dispatch on the verdict rows; a consumer comparing raw byte counts re-derives what the band table already decides, and a denied grant is the signal that every durable band risks eviction under pressure.
- Receipt: `Opfs.Budget` carries the numbers beside the verdict so telemetry stamps evidence while consumers dispatch on the row.
- Growth: a new pressure band is one `_BANDS` row; a new residency fact (a bucket API, a durability probe) is one member on this owner; a new egress route (an origin-private-file handle, a share-target hand-off) is one arm on the ladder plus one `Route` literal.
- Packages: `effect` (`Data`, `Effect`, `Exit`, `Option`, `Predicate`, `Scope`); `@rasm/ts/core` (`FaultClass`).

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

// the picker is absent from the pinned `lib.dom`, so the native is spelled once HERE: the refinement is this
// owner's boundary declaration and the ui capability never names a browser API
type _SaveWritable = WritableStream<Uint8Array> & {
  readonly write: (data: Uint8Array) => Promise<void>
  readonly close: () => Promise<void>
  readonly abort: (reason?: unknown) => Promise<void>
}

type _SaveHandle = {
  readonly createWritable: () => Promise<_SaveWritable>
}

// share is the second lib-absent-adjacent native this owner spells: `canShare` gates the payload before the sheet
// opens, so an unsupported file answers `absent` as data rather than a thrown TypeError
type _ShareHost = Navigator & {
  readonly share?: (data: { readonly files: ReadonlyArray<File>; readonly title?: string }) => Promise<void>
  readonly canShare?: (data: { readonly files: ReadonlyArray<File> }) => boolean
}

type _SavePicker = (options: {
  readonly suggestedName: string
  readonly types: ReadonlyArray<{ readonly description: string; readonly accept: Record<string, ReadonlyArray<string>> }>
}) => Promise<_SaveHandle>

const _egressFamily = FaultClass.family(["absent", "declined", "io"] as const, {
  absent: { class: "absent" }, // no route: neither a picker nor a document to anchor against
  declined: { class: "denied" }, // the user closed the dialog — a decision, never a transient to re-drive
  io: { class: "unavailable" },
})

declare namespace EgressFault {
  type Reason = (typeof _egressFamily.reasons)[number]
}

class EgressFault extends Data.TaggedError("EgressFault")<{
  readonly reason: EgressFault.Reason
  readonly detail: string
}> {
  get class(): FaultClass.Kind {
    return _egressFamily.classOf(this.reason)
  }
  override get message(): string {
    return `<egress:${this.reason}> ${this.detail}`
  }
}

declare namespace Egress {
  type Route = "picker" | "anchor" | "absent"
  type Carriage = {
    readonly save: Route
    readonly share: boolean
    readonly stream: boolean
  }
  type File = {
    readonly name: string
    readonly mime: string
    readonly extensions: ReadonlyArray<string>
    readonly octets: Uint8Array
  }
}

const _picker = (): Option.Option<_SavePicker> =>
  Option.fromNullable((globalThis as typeof globalThis & { readonly showSaveFilePicker?: _SavePicker }).showSaveFilePicker)

const _picked = (pick: _SavePicker, file: Egress.File): Effect.Effect<void, EgressFault> =>
  Effect.tryPromise({
    try: async () => {
      const handle = await pick({
        suggestedName: file.name,
        types: [{ description: file.mime, accept: { [file.mime]: file.extensions } }],
      })
      const writable = await handle.createWritable()
      await writable.write(file.octets)
      await writable.close()
    },
    catch: (cause) =>
      cause instanceof DOMException && cause.name === "AbortError"
        ? new EgressFault({ reason: "declined", detail: file.name })
        : new EgressFault({ reason: "io", detail: String(cause) }),
  })

const _sharer = (): Option.Option<Required<Pick<_ShareHost, "share" | "canShare">>> => {
  const host = globalThis.navigator as _ShareHost
  return host.share && host.canShare ? Option.some({ share: host.share, canShare: host.canShare }) : Option.none()
}

const _shared = (file: Egress.File): Effect.Effect<void, EgressFault> =>
  Option.match(_sharer(), {
    onNone: () => Effect.fail(new EgressFault({ reason: "absent", detail: file.name })),
    onSome: ({ share, canShare }) => {
      const payload = { files: [new globalThis.File([file.octets], file.name, { type: file.mime })] }
      return canShare(payload)
        ? Effect.tryPromise({
          try: () => share(payload),
          catch: (cause) =>
            cause instanceof DOMException && (cause.name === "AbortError" || cause.name === "NotAllowedError")
              ? new EgressFault({ reason: "declined", detail: file.name })
              : new EgressFault({ reason: "io", detail: String(cause) }),
        })
        : Effect.fail(new EgressFault({ reason: "absent", detail: file.mime }))
    },
  })

// the streaming arm: the writable is scope-owned — a completed pipe closed it already, so the success release
// swallows the double close, and a failed or interrupted consumer aborts so no partial file survives the scope
const _opened = (pick: _SavePicker, file: Egress.File): Effect.Effect<_SaveWritable, EgressFault, Scope.Scope> =>
  Effect.acquireRelease(
    Effect.tryPromise({
      try: async () => {
        const handle = await pick({
          suggestedName: file.name,
          types: [{ description: file.mime, accept: { [file.mime]: file.extensions } }],
        })
        return await handle.createWritable()
      },
      catch: (cause) =>
        cause instanceof DOMException && cause.name === "AbortError"
          ? new EgressFault({ reason: "declined", detail: file.name })
          : new EgressFault({ reason: "io", detail: String(cause) }),
    }),
    (writable, exit) =>
      Effect.promise(() => (Exit.isSuccess(exit) ? writable.close() : writable.abort()).catch(() => undefined)),
  )

const _anchored = (file: Egress.File): Effect.Effect<void, EgressFault> =>
  Effect.tryPromise({
    // BOUNDARY ADAPTER: the anchor route is a DOM ceremony with no expression form; the object URL revokes in the
    // same frame the synchronous click consumed it, so no blob outlives this kernel
    try: async () => {
      const url = globalThis.URL.createObjectURL(new globalThis.Blob([file.octets], { type: file.mime }))
      const anchor = globalThis.document.createElement("a")
      anchor.href = url
      anchor.download = file.name
      anchor.click()
      globalThis.URL.revokeObjectURL(url)
    },
    catch: (cause) => new EgressFault({ reason: "io", detail: String(cause) }),
  })

const Egress: {
  readonly route: Effect.Effect<Egress.Carriage>
  readonly save: (file: Egress.File) => Effect.Effect<void, EgressFault>
  readonly share: (file: Egress.File) => Effect.Effect<void, EgressFault>
  readonly open: (file: Egress.File) => Effect.Effect<_SaveWritable, EgressFault, Scope.Scope>
} = {
  route: Effect.sync((): Egress.Carriage => {
    const picker = Option.isSome(_picker())
    return {
      save: picker ? "picker" : "document" in globalThis ? "anchor" : "absent",
      share: Option.isSome(_sharer()),
      stream: picker,
    }
  }),
  save: (file) =>
    Option.match(_picker(), {
      onSome: (pick) => _picked(pick, file),
      onNone: () =>
        "document" in globalThis
          ? _anchored(file)
          : Effect.fail(new EgressFault({ reason: "absent", detail: file.name })),
    }),
  share: _shared,
  open: (file) =>
    Option.match(_picker(), {
      onSome: (pick) => _opened(pick, file),
      onNone: () => Effect.fail(new EgressFault({ reason: "absent", detail: file.name })),
    }),
}
```

## [05]-[OVERLAY_AND_LANE]

[OVERLAY_AND_LANE]:
- Owner: `Overlay` — the browser backing rows the `@effect/experimental` EventLog client requires, assembled once: `Overlay.backing(spec)` merges the IndexedDB journal (`EventJournal.layerIndexedDb`, its own database, never a `[3]` store), the client identity over Web Storage (`EventLog.layerIdentityKvs` satisfied by `BrowserKeyValueStore.layerLocalStorage`), and the `Reactivity` bus; `Overlay.sync(url)` is the self-contained browser sync row (`EventLogRemote.layerWebSocketBrowser` — WebSocket plus Web Crypto E2E, requiring only the built `EventLog`).
- Law: the event universe is app data — the app declares its `Event`/`EventGroup` families, freezes them with `EventLog.schema`, and composes `EventLog.layer(schema)` plus its group-handler registrations over this page's backings at the root; the lib ships backings and law, never an event vocabulary, so hundreds of apps ride one overlay spelling.
- Law: overlay, never authority — the journal is append-only capture and the reducers fold local reads; anything durable-critical projects from or mirrors to the data journal through the sync server the edge mounts, and a lane holding sole custody of critical state is the named boundary breach.
- Law: the wasm-lane seam covers BOTH data-owned browser engines under one gate — heavier local read models than the reducer folds ride the OPFS driver on `lane/sqlite`'s wasm profile row, and the browser-resident analytics arm rides `lane/olap`'s `Olap.wasm` row (range-read Parquet over self-hosted bundles, Arrow receipts to the viewer's query surfaces): the sqlite lane publishes its wasm profile as a `SqlClient` Layer the app root provides beneath the read models, the olap lane mints a scoped `Olap.Handle` the browser shell composes at boot and leases per unit of work, and this page's `retain`/`budget` verdicts gate whether EITHER lane opens at all (a `critical` verdict or a refused grant demotes the app to the kv/overlay tier); the engines' own degradation verdicts — `originScope` tenancy, `singleTab` writer, `reactivityHooks` change delivery — are their lane pages' rows, the bundles precache under `shell#CACHE_ROWS`'s asset posture, and no `@effect/sql*` or engine import exists in this folder.
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

export { Egress, EgressFault, Kv, KvFault, Opfs, Overlay }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
