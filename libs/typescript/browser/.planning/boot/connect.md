# [BROWSER_CONNECT]

`boot/connect.ts` is the browser runtime-state plane: every ambient host signal — connectivity, page visibility, network profile — lives in exactly one owned cell advanced only by its owned event fold, and every consumer reads the cell, never the navigator. `Connect` holds three `SubscriptionRef` cells seeded at construction and driven by scoped DOM-event streams, projects the derived edges siblings act on — `redials` (the offline-to-online rising edge `shell/worker` drains on), `hidden` (the visibility falling edge flush folds fire on) — and owns the native `SyncManager` background-wake registration as an optional capability feeding the same drain fold. A private `navigator.onLine` read, a stray `visibilitychange` listener, or a second connectivity notion anywhere in the folder is the named per-component-probe defect; the retired private `visibilitychange` ingresses land here and nowhere else.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                      | [PUBLIC]  |
| :-----: | :-------------- | :----------------------------------------------------------- | :-------- |
|  [01]   | `SIGNAL_CELLS`  | the three seeded cells and their owned event folds           | `Connect` |
|  [02]   | `DERIVED_EDGES` | the transition-edge streams and the `SyncManager` wake       | `Connect` |

## [2]-[SIGNAL_CELLS]

[SIGNAL_CELLS]:
- Owner: `Connect`, one scoped `Effect.Service` — `online: SubscriptionRef<boolean>` seeded from `navigator.onLine` and advanced only by the merged `online`/`offline` window-event fold; `visible: SubscriptionRef<boolean>` seeded from `document.visibilityState` and advanced only by the `visibilitychange` fold; `profile: SubscriptionRef<Option<Connect.Profile>>` seeded and advanced from the experimental `navigator.connection` surface, `Option.none` where the host ships none.
- Law: cells are read-only structurally — each publishes as `Subscribable`, the write half stays on the interior `SubscriptionRef`, and each is advanced only by its owned capture fiber forked `Effect.forkScoped` at construction, so listeners die with the runtime scope and a consumer write is unspellable, never merely forbidden.
- Law: the network profile is a closed vocabulary, never a raw string — `_GRADES` maps the host `effectiveType` rows onto the three-grade axis (`swift`/`steady`/`strained`) and `frugal` carries `saveData`, so byte-budget consumers (`transport/fetch` flow rows, `transport/pool` scheduling) dispatch on grade rows and an unrecognized host string folds to `Option.none`, never a throw.
- Law: `navigator.connection` and the registration's `sync` member are absent from the DOM lib — `_NetSource` and `_SyncHost` are the boundary refinements pinned at this owner, the only places the experimental surfaces are spelled; a consumer never touches either.
- Growth: a new ambient signal (battery, memory pressure, page freeze) is one cell plus one owned fold on this service — never a sibling owner, never a consumer-side listener.
- Boundary: `telemetry/signal/vital` owns RUM measurement; this page owns only the runtime-state cells its flush edges read. `host/flag` reconnect fallback, `shell/worker` replay, and `transport/fetch` offline gates read these cells through the requirement channel.
- Packages: `effect` (`SubscriptionRef`, `Subscribable`, `Stream`, `Option`, `Record`, `Array`); `@effect/platform-browser` (`BrowserStream.fromEventListenerWindow`, `BrowserStream.fromEventListenerDocument`).

## [3]-[DERIVED_EDGES]

[DERIVED_EDGES]:
- Owner: the edge projections and the wake — `redials: Stream<void>` projects each offline-to-online rising transition from the `online` cell's change feed; `hidden: Stream<void>` projects each visible-to-hidden falling transition; `wake(tag)` registers a `SyncManager` background-sync tag against the active service-worker registration, answering `false` where the host ships no `sync` member so absence is a value, never a second queue.
- Law: edges derive from cells — `SubscriptionRef.changes` replays the current value to a late subscriber, so the edge fold pairs each element with its predecessor through `Stream.zipWithPrevious` and admits only the genuine transition; a consumer subscribing raw DOM events to re-derive an edge is the probe defect in stream clothing.
- Law: the wake and the `redials` edge feed one downstream drain — `shell/worker` merges both into its single replay fold, so `SyncManager` support widens wake coverage and its absence changes nothing structurally.
- Receipt: `wake` answers `boolean` — registration accepted or capability absent — so boot stamps the wake posture without a probe.
- Boundary: what drains on a redial is `shell/worker`'s replay law; what flushes on `hidden` is its subscriber's; this page owns only the edges.

```typescript
import { BrowserStream } from "@effect/platform-browser"
import { Array, Effect, Option, Record, Stream, Subscribable, SubscriptionRef } from "effect"

const _GRADES = { "4g": "swift", "3g": "steady", "2g": "strained", "slow-2g": "strained" } as const

declare namespace Connect {
  type Grade = (typeof _GRADES)[keyof typeof _GRADES]
  type Profile = { readonly grade: Grade; readonly frugal: boolean }
}

type _NetSource = EventTarget & { readonly effectiveType?: string; readonly saveData?: boolean }
type _SyncHost = ServiceWorkerRegistration & { readonly sync: { readonly register: (tag: string) => Promise<void> } }

const _profiled = (source: _NetSource): Option.Option<Connect.Profile> =>
  Option.map(
    Array.findFirst(Record.toEntries(_GRADES), ([host]) => host === source.effectiveType),
    ([, grade]) => ({ grade, frugal: source.saveData === true }),
  )

const _connection = (): Option.Option<_NetSource> =>
  Option.fromNullable((globalThis.navigator as Navigator & { readonly connection?: _NetSource }).connection)

const _edged = (feed: Stream.Stream<boolean>, from: boolean): Stream.Stream<void> =>
  feed.pipe(
    Stream.changes,
    Stream.zipWithPrevious,
    Stream.filterMap(([prior, next]) =>
      next !== from && Option.getOrElse(Option.map(prior, (held) => held === from), () => false)
        ? Option.some(undefined)
        : Option.none(),
    ),
  )

class Connect extends Effect.Service<Connect>()("browser/boot/Connect", {
  scoped: Effect.gen(function* () {
    const _online = yield* SubscriptionRef.make(globalThis.navigator.onLine)
    const _visible = yield* SubscriptionRef.make(globalThis.document.visibilityState === "visible")
    const _profile = yield* SubscriptionRef.make(Option.flatMap(_connection(), _profiled))
    yield* Stream.merge(
      Stream.as(BrowserStream.fromEventListenerWindow("online"), true),
      Stream.as(BrowserStream.fromEventListenerWindow("offline"), false),
    ).pipe(
      Stream.runForEach((up) => SubscriptionRef.set(_online, up)),
      Effect.forkScoped,
    )
    yield* BrowserStream.fromEventListenerDocument("visibilitychange").pipe(
      Stream.runForEach(() => SubscriptionRef.set(_visible, globalThis.document.visibilityState === "visible")),
      Effect.forkScoped,
    )
    yield* Option.match(_connection(), {
      onNone: () => Effect.void,
      onSome: (source) =>
        Stream.fromEventListener(source, "change").pipe(
          Stream.runForEach(() => SubscriptionRef.set(_profile, _profiled(source))),
          Effect.forkScoped,
        ),
    })
    const wake = (tag: string): Effect.Effect<boolean> =>
      Effect.tryPromise(() => globalThis.navigator.serviceWorker.ready).pipe(
        Effect.flatMap((registration) =>
          "sync" in registration
            ? Effect.as(Effect.tryPromise(() => (registration as _SyncHost).sync.register(tag)), true)
            : Effect.succeed(false),
        ),
        Effect.orElseSucceed(() => false),
      )
    const online: Subscribable.Subscribable<boolean> = _online
    const visible: Subscribable.Subscribable<boolean> = _visible
    const profile: Subscribable.Subscribable<Option.Option<Connect.Profile>> = _profile
    return {
      online,
      visible,
      profile,
      redials: _edged(_online.changes, false),
      hidden: _edged(_visible.changes, true),
      wake,
    }
  }),
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Connect }
```
