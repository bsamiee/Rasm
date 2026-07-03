# [BROWSER_OPFS]

`persist/opfs.ts` owns the durable local band: the OPFS storage-residency verdicts every local-durability decision reads, and the browser half of the local-first arrangement — the `EventLog` overlay client backings and the sqlite-wasm lane seam. Residency is one owner over the native `StorageManager` — the persistence grant, the quota estimate, and the closed pressure-verdict vocabulary consumers dispatch on — spelled once at this seam and nowhere else. The overlay law is absolute (`[R19]`): the `EventLog` client is an OVERLAY that accelerates local-first reads and offline capture; the record of truth is the `store` SQL journal, and a value whose loss corrupts state never lives only here. The sqlite-wasm lane (`[R13]`) meets the browser at the composition root: `store` owns `@effect/sql-sqlite-wasm` and publishes the OPFS driver Layer on its `./wasm` subpath; this folder imports no sql surface and contributes the residency verdicts that lane's health gates read — the same root-met inversion `work` holds with the store drivers.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]            | [OWNS]                                                            | [PUBLIC]   |
| :-----: | :------------------- | :------------------------------------------------------------------ | :--------- |
|  [01]   | `STORAGE_RESIDENCY`  | the grant, the estimate, the pressure-verdict vocabulary            | `Opfs`     |
|  [02]   | `OVERLAY_AND_LANE`   | the EventLog browser backings, the sync row, the wasm-lane seam law | `Overlay`  |

## [2]-[STORAGE_RESIDENCY]

[STORAGE_RESIDENCY]:
- Owner: `Opfs`, one `Effect.Service` over the native `StorageManager` — `retained` reads `navigator.storage.persisted()` (already granted), `retain` requests `navigator.storage.persist()` (granted now), and `budget` folds `navigator.storage.estimate()` into the `Opfs.Budget` receipt: usage, quota, headroom, and the verdict drawn from the closed `_BANDS` vocabulary (`ample`/`tight`/`critical` by usage fraction, `opaque` where the host withholds numbers).
- Law: the native calls are confined — `navigator.storage` is spelled only inside this owner, and a `persist`/`estimate` probe at any consumer is the named ungated-native-call defect; a host without the surface folds to `retained: false` and `opaque`, so capability absence is data.
- Law: the verdict is the one pressure vocabulary — `transport/pool` byte scheduling, `persist/kv` eviction posture, and the sqlite-wasm lane's health gate all dispatch on the verdict rows; a consumer comparing raw byte counts re-derives what the band table already decides, and a denied grant is the signal that every durable band risks eviction under pressure.
- Receipt: `Opfs.Budget` carries the numbers beside the verdict so telemetry stamps evidence while consumers dispatch on the row.
- Growth: a new pressure band is one `_BANDS` row; a new residency fact (a bucket API, a durability probe) is one member on this owner.
- Packages: `effect` (`Effect`, `Option`).

```typescript
import { Effect, Layer, Option } from "effect"

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

class Opfs extends Effect.Service<Opfs>()("browser/persist/Opfs", {
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
      onNone: () =>
        Effect.succeed<Opfs.Budget>({
          usage: Option.none(),
          quota: Option.none(),
          headroom: Option.none(),
          verdict: "opaque",
        }),
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
          (): Opfs.Budget => ({
            usage: Option.none(),
            quota: Option.none(),
            headroom: Option.none(),
            verdict: "opaque",
          }),
        ),
    }),
  }),
  accessors: true,
}) {}
```

## [3]-[OVERLAY_AND_LANE]

[OVERLAY_AND_LANE]:
- Owner: `Overlay` — the browser backing rows the `@effect/experimental` EventLog client requires, assembled once: `Overlay.backing(spec)` merges the IndexedDB journal (`EventJournal.layerIndexedDb`, its own database, never a `persist/kv` store), the client identity over Web Storage (`EventLog.layerIdentityKvs` satisfied by `BrowserKeyValueStore.layerLocalStorage`), and the `Reactivity` bus; `Overlay.sync(url)` is the self-contained browser sync row (`EventLogRemote.layerWebSocketBrowser` — WebSocket plus Web Crypto E2E, requiring only the built `EventLog`).
- Law: the event universe is app data — the app declares its `Event`/`EventGroup` families, freezes them with `EventLog.schema`, and composes `EventLog.layer(schema)` plus its group-handler registrations over this page's backings at the root; the lib ships backings and law, never an event vocabulary, so hundreds of apps ride one overlay spelling.
- Law: overlay, never authority (`[R19]`) — the journal is append-only capture and the reducers fold local reads; anything durable-critical projects from or mirrors to the `store` journal through the sync server the edge mounts, and a lane holding sole custody of critical state is the named boundary breach.
- Law: the sqlite-wasm lane seam (`[R13]`) — heavier local read models than the reducer folds (SQL over materialized projections) ride the store-owned OPFS driver: `store` publishes the `./wasm` `SqlClient` Layer, the app root provides it beneath the app's read models, and this page's `retain`/`budget` verdicts gate whether the lane opens at all (a `critical` verdict or a refused grant demotes the app to the kv/overlay tier); no `@effect/sql*` import exists in this folder.
- Law: local-first boot order is fixed — `retain` first (durability grant before bytes land), backings next, sync last; a sync row without a journal is unbuildable by the requirement channel, which is the assembly proof.
- Growth: a second sync transport (a socket-constructor row for a shared worker) is one row beside `sync`; a journal swap (memory for specs) is Layer substitution at the root, never an edit here.
- Boundary: the server half — storage, encryption at rest, the mountable `HttpApp` handler — is `store`/`edge` material; compaction and reactivity keys are the app's group declarations.
- Packages: `@effect/experimental` (`EventJournal`, `EventLog`, `EventLogRemote`, `Reactivity`); `@effect/platform-browser` (`BrowserKeyValueStore`); `effect` (`Layer`, `Option`).

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

export { Opfs, Overlay }
```
