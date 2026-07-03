# [BROWSER_WORKER]

`shell/worker.ts` is the window-side service-worker plane: one `Workbox` instance held as a scoped resource owns registration and the update handshake, one `SwLifecycle` cell folded from the Workbox event target is the phase truth every affordance reads, the cache-strategy vocabulary is a closed row table projected type-only onto `workbox-build`'s `RuntimeCaching` shape for the app build to inject, and the background-sync replay is one drain fold over `persist/kv`'s `outbox` fed by every wake source at once — the reconnect edge, the `SyncManager` wake, and the worker's own replay reports. The altitude split is law: `workbox-build` emits the worker ASSET at app build, this module owns its RUNTIME lifecycle — a strategy row authored in the SW source, a raw `navigator.serviceWorker`/`caches` call outside this owner, or a second replay queue beside the outbox is the named two-owner defect.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                               | [PUBLIC]            |
| :-----: | :---------------- | :-------------------------------------------------------------------- | :------------------ |
|  [01]   | `LIFECYCLE_OWNER` | the Workbox resource, the phase cell, the update handshake, the fault | `Sw`, `SwFault`, `SwLifecycle` |
|  [02]   | `CACHE_ROWS`      | the strategy vocabulary and the build-facing `RuntimeCaching` projection | `Sw.caching`     |
|  [03]   | `REPLAY_DRAIN`    | the outbox enqueue and the merged-wake drain fold                     | `Sw`                |

## [2]-[LIFECYCLE_OWNER]

[LIFECYCLE_OWNER]:
- Owner: `Sw`, one scoped `Effect.Service` built through `Sw.Default(script)` — the `Workbox` instance acquired once over the app's build-emitted worker script and released by a final update check; `phase`, the `SwLifecycle` cell (`Unregistered`/`Installing`/`Installed`/`Waiting`/`Active`/`Reloading`/`Redundant`) advanced only by the bridged event fold; `update`, the poll for a fresh worker; `apply`, the update handshake; `reports`, the decoded window-bound message feed; `signal`, the request/response channel into the live worker.
- Law: the event bridge is the module's platform-forced statement seam — `Stream.asyncScoped` acquires one listener per lifecycle tag against the `Workbox` event target and releases every one on scope close; the implementer carries the `// BOUNDARY ADAPTER` mark on `_lifecycle`'s first line. Six tags advance the cell through the keyed `_PHASES` lookup — a `Match` ladder over a keyed correspondence is the rejected dispatch — and `controlling` alone carries logic: it reads the prior phase and reloads the document exactly once when that phase is `Reloading`.
- Law: the apply order is load-bearing — `apply` sets `Reloading` and only then calls `messageSkipWaiting`, so the `controlling` arm observes the intent and a `controllerchange` arriving for any other reason never reloads; the `waiting` arm is what flips the update affordance `shell/install` surfaces.
- Law: `unsupported` short-circuits at construction — a host without `navigator.serviceWorker` yields the service in its inert posture (`Unregistered` forever, `register` answering the typed fault) so every consumer folds capability absence as data.
- Receipt: `register` yields `boolean` — controlled now or awaiting first load — and the phase cell carries everything else; no consumer touches the registration object.
- Boundary: the refresh affordance is `shell/install`'s read; the worker asset, precache manifest, and `NavigationRoute` offline shell are the app build's, emitted through `workbox-build`'s `injectManifest` over the authored SW.
- Packages: `workbox-window` (`Workbox`, lifecycle event map); `effect` (`Effect`, `Stream`, `SubscriptionRef`, `Data`, `Record`, `Option`, `Schema`, `Ref`, `DateTime`).

```typescript
import { Data, DateTime, Effect, Option, Record, Ref, Schema, Stream, SubscriptionRef } from "effect"
import { Workbox } from "workbox-window"
import { Connect } from "../boot/connect.ts"
import { Kv, type KvFault } from "../persist/kv.ts"

type SwLifecycle = Data.TaggedEnum<{
  Unregistered: {}
  Installing: {}
  Installed: {}
  Waiting: {}
  Active: {}
  Reloading: {}
  Redundant: {}
}>
const SwLifecycle: Data.TaggedEnum.Constructor<SwLifecycle> = Data.taggedEnum<SwLifecycle>()

const SwFaultPolicy = {
  unsupported: { rank: 4, retry: false },
  register: { rank: 3, retry: true },
  message: { rank: 2, retry: true },
} as const

declare namespace SwFault {
  type Reason = keyof typeof SwFaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean }
  type _Rows<T extends Record<Reason, Row> = typeof SwFaultPolicy> = T
}

class SwFault extends Data.TaggedError("SwFault")<{
  readonly reason: SwFault.Reason
  readonly detail: string
}> {
  get policy(): SwFault.Row {
    return SwFaultPolicy[this.reason]
  }
}

const _TAGS = ["installing", "installed", "waiting", "activating", "activated", "redundant"] as const

const _PHASES: Record<(typeof _TAGS)[number], SwLifecycle> = {
  installing: SwLifecycle.Installing(),
  installed: SwLifecycle.Installed(),
  waiting: SwLifecycle.Waiting(),
  activating: SwLifecycle.Active(),
  activated: SwLifecycle.Active(),
  redundant: SwLifecycle.Redundant(),
}

const _Report = Schema.Union(
  Schema.TaggedStruct("Replayed", { queue: Schema.NonEmptyString, remained: Schema.Int.pipe(Schema.nonNegative()) }),
  Schema.TaggedStruct("Claimed", { build: Schema.NonEmptyString }),
)

const _lifecycle = (wb: Workbox): Stream.Stream<(typeof _TAGS)[number] | "controlling"> =>
  Stream.asyncScoped((emit) =>
    Effect.acquireRelease(
      Effect.sync(() =>
        [..._TAGS, "controlling" as const].map((tag) => {
          const handler = () => void emit.single(tag)
          wb.addEventListener(tag, handler)
          return [tag, handler] as const
        }),
      ),
      (held) =>
        Effect.sync(() => {
          for (const [tag, handler] of held) wb.removeEventListener(tag, handler)
        }),
    ),
  )
```

## [3]-[CACHE_ROWS]

[CACHE_ROWS]:
- Owner: the interior `_STRATEGIES` lookup (the four-grade strategy axis onto `workbox-build`'s `StrategyName`) and the `_lanes` route table — `bank` (hashed static assets: cache-first, long retention), `live` (API reads: network-first with a raced timeout), `tile` (media, tiles, GLB bands: stale-while-revalidate under an entry cap), `pass` (never cached) — each row carrying its URL pattern source, cache name stem, retention, and optional background-sync queue name; `Sw.caching(mark)` projects the table to `ReadonlyArray<RuntimeCaching>` with every cache name stamped by the build fingerprint.
- Law: `workbox-build` is type-only here — `RuntimeCaching` and `StrategyName` type imports keep the strategy rows one source of truth across the build/runtime seam while the Node-only value surface never enters the bundle; the app build script value-imports this projection and hands it to `injectManifest` beside the authored SW.
- Law: the fingerprint stamps identity — `mark` derives from the kernel `AppIdentity` build block at the composition root, so a build bump mints fresh cache names and a stale worker cannot serve mixed generations; the precache side inherits the same identity through the build's `cacheId`.
- Law: a cache behavior is a row, never a branch — the `sync` column is where a replayable lane names its SW-side queue (`workbox-background-sync` config the build injects), so the SW-resident queue and the window's outbox stay two altitudes of one replay concern.
- Growth: a new cache posture is one `_lanes` row; a new strategy grade is one `_STRATEGIES` entry — the row type breaks until both align.
- Packages: `workbox-build` (type-only `RuntimeCaching`, `StrategyName`); `effect` (`Record`, `Option`).

```typescript
import type { RuntimeCaching, StrategyName } from "workbox-build"

const _STRATEGIES = {
  bank: "CacheFirst",
  live: "NetworkFirst",
  tile: "StaleWhileRevalidate",
  pass: "NetworkOnly",
} as const satisfies Record<string, StrategyName>

const _lanes = {
  asset: { strategy: "bank", pattern: "\\.(?:js|css|wasm|woff2)$", keep: { entries: 256, days: 30 }, sync: Option.none<string>(), race: Option.none<number>() },
  api: { strategy: "live", pattern: "/api/", keep: { entries: 64, days: 1 }, sync: Option.some("rasm-outbox"), race: Option.some(3) },
  band: { strategy: "tile", pattern: "\\.(?:glb|ktx2|png|webp|pmtiles)$", keep: { entries: 128, days: 7 }, sync: Option.none<string>(), race: Option.none<number>() },
} as const

declare namespace _lanes {
  type Kind = keyof typeof _lanes
  type Row = {
    readonly strategy: keyof typeof _STRATEGIES
    readonly pattern: string
    readonly keep: { readonly entries: number; readonly days: number }
    readonly sync: Option.Option<string>
    readonly race: Option.Option<number>
  }
  type _Rows<T extends Record<Kind, Row> = typeof _lanes> = T
}

const _caching = (mark: string): ReadonlyArray<RuntimeCaching> =>
  Record.values(
    Record.map(_lanes, (row, kind) => ({
      handler: _STRATEGIES[row.strategy],
      urlPattern: new RegExp(row.pattern),
      options: {
        cacheName: `${kind}-${mark}`,
        expiration: { maxEntries: row.keep.entries, maxAgeSeconds: row.keep.days * 86400 },
        ...Option.match(row.sync, { onNone: () => ({}), onSome: (name) => ({ backgroundSync: { name } }) }),
        ...Option.match(row.race, { onNone: () => ({}), onSome: (seconds) => ({ networkTimeoutSeconds: seconds }) }),
      },
    })),
  )
```

## [4]-[REPLAY_DRAIN]

[REPLAY_DRAIN]:
- Owner: the outbox lane on the same service — `queue(band)` appends one durable entry (minted instant plus the caller's already-encoded payload band) into `persist/kv`'s `outbox` domain under a monotonic key; `relayed(relay)` is the single drain fold: every wake source — `Connect.redials`, the worker's `Replayed` reports, and the one-shot `Connect.wake` registration at construction — merges into one stream, each wake drains the outbox atomically and hands every entry to the app-supplied `relay` leg in minted order, and a refused entry re-enqueues under its original key so nothing drops silently.
- Law: the element is opaque here — the outbox row is `Kv.Value<"outbox">` (minted, band); the producing rail encoded the band and the relay leg decodes it, so this fold never inspects payloads and the wire vocabulary stays with its owner.
- Law: one queue, two altitudes — failed same-origin fetches replay inside the SW through the `workbox-background-sync` queue the cache rows configure; app-level intents replay here through the window drain; the `Replayed` report is the seam where the SW's drain completion wakes the window's, so the two altitudes converge without sharing storage.
- Law: the drain is serial and self-quenching — `relay` runs per entry with the fold awaiting each, a mid-drain fault re-enqueues the remainder untouched (the drain already cleared the store, so the re-write is the compensation), and a wake arriving mid-drain queues behind the running fold rather than starting a second.
- Boundary: what the band contains and where `relay` dials is the composing app's selection from `wire`'s gateway surface; this page owns durability, ordering, and wake fan-in only.
- Packages: `effect` (`Stream`, `Effect`, `DateTime`, `Ref`); `../boot/connect.ts` (`Connect`); `../persist/kv.ts` (`Kv`).

```typescript
class Sw extends Effect.Service<Sw>()("browser/shell/Sw", {
  scoped: (script: string) =>
    Effect.gen(function* () {
      const connect = yield* Connect
      const kv = yield* Kv
      const phase = yield* SubscriptionRef.make<SwLifecycle>(SwLifecycle.Unregistered())
      const carried = "serviceWorker" in globalThis.navigator
      const wb = yield* Effect.acquireRelease(
        Effect.sync(() => new Workbox(script)),
        (held) => (carried ? Effect.ignore(Effect.promise(() => held.update())) : Effect.void),
      )
      const reports = yield* Stream.asyncScoped<unknown>((emit) =>
        Effect.acquireRelease(
          Effect.sync(() => {
            const handler = (event: { readonly data: unknown }) => void emit.single(event.data)
            wb.addEventListener("message", handler)
            return handler
          }),
          (handler) => Effect.sync(() => wb.removeEventListener("message", handler)),
        ),
      ).pipe(
        Stream.mapEffect((data) => Effect.option(Schema.decodeUnknown(_Report)(data)), { concurrency: 1 }),
        Stream.filterMap((decoded) => decoded),
        Stream.share({ capacity: 16 }),
      )
      yield* _lifecycle(wb).pipe(
        Stream.runForEach((tag) =>
          tag === "controlling"
            ? Effect.flatMap(SubscriptionRef.get(phase), (held) =>
                SwLifecycle.$is("Reloading")(held)
                  ? Effect.sync(() => globalThis.location.reload())
                  : SubscriptionRef.set(phase, SwLifecycle.Active()),
              )
            : SubscriptionRef.set(phase, _PHASES[tag]),
        ),
        Effect.forkScoped,
      )
      const counter = yield* Ref.make(0)
      const queue = (band: Uint8Array): Effect.Effect<void, KvFault> =>
        Effect.gen(function* () {
          const minted = yield* DateTime.now
          const turn = yield* Ref.getAndUpdate(counter, (n) => n + 1)
          yield* kv.write("outbox", `${DateTime.toEpochMillis(minted)}:${turn}`, { minted, band })
        })
      const relayed = <E, R>(relay: (entry: Kv.Value<"outbox">) => Effect.Effect<void, E, R>) =>
        Stream.merge(connect.redials, Stream.as(Stream.filter(reports, (report) => report._tag === "Replayed"), undefined)).pipe(
          Stream.runForEach(() =>
            Effect.gen(function* () {
              const held = yield* kv.drain("outbox")
              yield* Effect.forEach(
                held,
                ([key, entry]) =>
                  relay(entry).pipe(Effect.catchAll(() => kv.write("outbox", key, entry))),
                { discard: true },
              )
            }),
          ),
          Effect.forkScoped,
        )
      yield* Effect.ignore(connect.wake("rasm-outbox"))
      return {
        phase,
        reports,
        register: carried
          ? Effect.tryPromise({
              try: () => wb.register(),
              catch: (cause) => new SwFault({ reason: "register", detail: String(cause) }),
            }).pipe(Effect.map((registration) => registration !== undefined))
          : Effect.fail(new SwFault({ reason: "unsupported", detail: "<no-service-worker>" })),
        update: Effect.tryPromise({
          try: () => wb.update(),
          catch: (cause) => new SwFault({ reason: "register", detail: String(cause) }),
        }),
        apply: SubscriptionRef.set(phase, SwLifecycle.Reloading()).pipe(
          Effect.zipRight(Effect.sync(() => wb.messageSkipWaiting())),
        ),
        signal: (data: object) =>
          Effect.tryPromise({
            try: () => wb.messageSW(data),
            catch: (cause) => new SwFault({ reason: "message", detail: String(cause) }),
          }),
        queue,
        relayed,
      }
    }),
}) {
  static readonly caching: (mark: string) => ReadonlyArray<RuntimeCaching> = _caching
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Sw, SwFault, SwLifecycle }
```
