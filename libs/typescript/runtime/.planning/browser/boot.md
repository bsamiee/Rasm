# [RUNTIME_BOOT]

The browser boot plane: one `BrowserRuntime.runMain` call boots the document — a second boot in the same document is the named defect, and the decode-worker entry is its own thread's boot under the same law — and `AppSpec` is the budget VALUE every app constructs before anything runs: the core `AppIdentity` it boots as, the feed rows it drives, and the numeric ceilings the folder's layer factories consume at composition. A host that calls in repeatedly — the view atom bridge, a web-component mount, a foreign SDK callback registry — holds the ONE `ManagedRuntime` handle `Boot.make` mints, and the document boot itself runs THROUGH that handle, so there is exactly one graph: the document run and every call-in observe the same scoped instances, and a per-call graph rebuild or a second boot graph is unspellable. The page also owns the ambient host-signal plane — connectivity, visibility, network profile, and permission state live in exactly one owned cell or feed advanced only by its owned event fold, so every consumer reads the cell and never the navigator — and the Web-API capability roster (`Clipboard`, `Geolocation`, `Permissions`) the root merges so `ui`-declared ports resolve to platform Layers at composition. The render posture is law here: client-rendered PWA plus build-time prerender rows own the SEO surface, `Boot.hydrated` hands the prerendered document to the app's mount, and a streaming-SSR server runtime is the named non-goal. The module is `runtime/src/browser/boot.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                        | [PUBLIC]     |
| :-----: | :---------------- | :---------------------------------------------------------------------------- | :----------- |
|  [01]   | `BUDGET_VALUE`    | the `AppSpec` shape — identity, feed rows, ceilings                           | `AppSpec`    |
|  [02]   | `SINGLE_BOOT`     | the one `runMain` seam, the spec Tag, the call-in handle, the hydration read  | `Boot`       |
|  [03]   | `SIGNAL_CELLS`    | the seeded cells, their owned folds, the derived edges, wake, permission feed | `Connect`    |
|  [04]   | `CAPABILITY_ROWS` | the Web-API service roster the root merges                                    | `Capability` |

## [02]-[BUDGET_VALUE]

[BUDGET_VALUE]:
- Owner: `AppSpec`, one `Schema.Class` — `identity` (the core `AppIdentity` composed as a field at full depth, so every identity dimension — app key, tenancy, namespace, build, instance, host fingerprint, environment, ring, region — arrives proven), `feeds` (the app's feed rows: a name plus the `net/client` lane it dials on — the budget is countable, named, and lane-typed), and `ceilings` (the numeric budget the composition root threads into factories: `workers` into `fetch#WIRE_PROTOCOL`'s pool layer, `settle` into `route#ADMISSION_FOLD`'s guard, `outbox` as the soft depth `shell#REPLAY_DRAIN` honors).
- Law: the lane axis is `net/client`'s, governed both ways — the interior tuple `satisfies` `Client.Lane` and the `_Spans` guard demands full coverage, so a lane rename or addition on the client table breaks this anchor at compile time, never as a stale budget row.
- Law: the spec is constructed once from validated material — `proc/config`'s provider chain resolves the identity at boot and the app assembles the value; nothing downstream re-reads an environment, and the spec's decode is the one admission of the budget.
- Law: consumers read rows, never re-declare knobs — the composition root threads spec rows into layer factories, so retuning an app is editing its spec value with zero lib edits.
- Law: `otel/emit`'s OTel `Resource` and this boot stamp derive from the SAME `AppIdentity` value — one identity spine, so hundreds of apps emit and boot through one vocabulary and a per-app identity fork is structurally impossible.
- Growth: a new budget axis is one `ceilings` field; a new feed fact is one field on the feed row.
- Boundary: what each feed DOES is the owning page's law; this owner counts and types them.
- Packages: `@rasm/ts/core` (`AppIdentity`); `effect` (`Schema`); `../net/client.ts` (type `Client`).

```typescript
import { BrowserRuntime, BrowserStream, Clipboard, Geolocation, Permissions } from "@effect/platform-browser"
import { AppIdentity } from "@rasm/ts/core"
import { Array, Context, Effect, Layer, ManagedRuntime, Option, Record, Schema, Stream, Subscribable, SubscriptionRef } from "effect"
import type { Client } from "../net/client.ts"

const _LANES = ["live", "batch", "feed"] as const satisfies ReadonlyArray<Client.Lane>

const _Feed = Schema.Struct({
  name: Schema.NonEmptyString,
  lane: Schema.Literal(..._LANES),
})

class AppSpec extends Schema.Class<AppSpec>("AppSpec")({
  identity: AppIdentity,
  feeds: Schema.Array(_Feed),
  ceilings: Schema.Struct({
    workers: Schema.Int.pipe(Schema.between(1, 16)),
    outbox: Schema.Int.pipe(Schema.positive()),
    settle: Schema.Duration,
  }),
}) {
  get label(): string {
    return `${this.identity.label}#${this.feeds.length}`
  }
}

declare namespace AppSpec {
  type Feed = Schema.Schema.Type<typeof _Feed>
  type Lane = (typeof _LANES)[number]
  type _Spans<K extends Lane = Client.Lane> = K
}
```

## [03]-[SINGLE_BOOT]

[SINGLE_BOOT]:
- Owner: `Boot`, one `Context.Tag` class — the Tag itself is the spec slot, so a service that earns a budget read writes `yield* Boot` with zero second hop; `Boot.make(spec, root)` mints the ONE `ManagedRuntime` for the document — the spec provided beneath the app-selected root, built under the module's one `Layer.MemoMap` — and every downstream seam holds this handle; `Boot.main(handle, app)`, the one `BrowserRuntime.runMain` seam riding it as a static: the app effect annotated with the identity stamp and provided FROM the handle (`Effect.provide` accepts a `ManagedRuntime`, so the document graph and every call-in observe the same scoped instances), with the handle's `dispose` chained into the boot teardown; `Boot.hydrated`, the prerender handoff read.
- Law: `main` is called exactly once per document, from the app's `main.ts`, and that module exports nothing — the empty surface is the structural proof it is terminal; every other module in the branch is barred from any `run*` call, and `fetch#RUNNER_ENTRY` is the one sibling boot, its own thread's.
- Law: one graph, one owner per acquisition — the document run and the imperative call-in (`handle.runPromise` from the view atom bridge, a web-component mount, a foreign SDK callback) both resolve through the handle `make` minted, so a scoped service has exactly one construction and one teardown path; a second `ManagedRuntime` or a run-main graph built beside the handle is the per-call-rebuild defect this owner makes unspellable.
- Law: the boot line is the only imperative seam — `runMain` installs error reporting and teardown wiring; `disableErrorReporting`/`disablePrettyLogger` stay default because crash visibility is `otel/crash`'s Layer concern, never a boot flag.
- Law: hydration is boot's law — the build emits per-route static HTML stamped with the `data-rasm-prerender` marker; `Boot.hydrated` reads the marker (`Option`-carried) so the app's mount takes over a prerendered document instead of re-rendering it, and a document without the marker is a cold client render; the marker read is this cluster's one DOM touch.
- Exemption: the `_memo` mint and the `dispose` chain are the platform-forced boot-seam run calls — this module is the edge where `Promise` is legal.
- Receipt: `main` returns `void` — everything observable thereafter flows through the composed graph; the annotation on `main`'s signature is the whole boot contract.
- Boundary: which Layer families merge into `root` is the app's selection across the branch; the `runMain` mechanics are `@effect/platform-browser`'s; view mounting is the ui wave's behind its atom bridge holding the same handle.
- Packages: `@effect/platform-browser` (`BrowserRuntime`); `effect` (`Context`, `Effect`, `Layer`, `ManagedRuntime`, `Option`).

```typescript
const _memo: Layer.MemoMap = Effect.runSync(Layer.makeMemoMap)

class Boot extends Context.Tag("runtime/browser/AppSpec")<Boot, AppSpec>() {
  static readonly hydrated: Effect.Effect<Option.Option<string>> = Effect.sync(() =>
    Option.fromNullable(globalThis.document.documentElement.getAttribute("data-rasm-prerender")),
  )
  static readonly make = <R, E>(
    spec: AppSpec,
    root: Layer.Layer<R, E>,
  ): ManagedRuntime.ManagedRuntime<R | Boot, E> =>
    ManagedRuntime.make(Layer.mergeAll(root, Layer.succeed(Boot, spec)), _memo)
  static readonly main = <A, E, R, E2>(
    handle: ManagedRuntime.ManagedRuntime<R | Boot, E2>,
    app: Effect.Effect<A, E, R | Boot>,
  ): void =>
    BrowserRuntime.runMain(
      Effect.flatMap(Boot, (spec) => Effect.annotateLogs(app, { app: spec.label })).pipe(
        Effect.provide(handle), // one graph: the document run resolves through the same handle every call-in holds
        Effect.ensuring(Effect.promise(() => handle.dispose())),
      ),
    )
}
```

## [04]-[SIGNAL_CELLS]

[SIGNAL_CELLS]:
- Owner: `Connect`, one scoped `Effect.Service` — `online: SubscriptionRef<boolean>` seeded from `navigator.onLine` and advanced only by the merged `online`/`offline` window-event fold; `visible` seeded from `document.visibilityState` and advanced only by the `visibilitychange` fold; `profile: SubscriptionRef<Option<Connect.Profile>>` seeded and advanced from the nonstandard `navigator.connection` surface, `Option.none` where the host ships none; the derived edges — `redials` (the offline-to-online rising edge `shell#REPLAY_DRAIN` drains on), `hidden` (the visibility falling edge flush folds fire on); `wake(tag)`, the `SyncManager` background-wake registration; `granted(name)`, the permission-state feed over the native `PermissionStatus` change target.
- Law: cells are read-only structurally — each publishes as `Subscribable`, the write half stays on the interior `SubscriptionRef`, and each is advanced only by its owned capture fiber forked `Effect.forkScoped` at construction, so listeners die with the runtime scope and a consumer write is unspellable, never merely forbidden.
- Law: the network profile is a closed vocabulary, never a raw string — `_GRADES` maps the host `effectiveType` rows onto the three-grade axis (`swift`/`steady`/`strained`) and `frugal` carries `saveData`, so byte-budget consumers (`fetch#FLOW_ROWS`, `fetch#DEPOT_SCHEDULER`) dispatch on grade rows and an unrecognized host string folds to `Option.none`, never a throw.
- Law: edges derive from cells — `SubscriptionRef.changes` replays the current value to a late subscriber, so the edge fold pairs each element with its predecessor through `Stream.zipWithPrevious` and admits only the genuine transition; a consumer subscribing raw DOM events to re-derive an edge is the probe defect in stream clothing.
- Law: `granted` folds capability absence to silence — a host without `navigator.permissions` yields the empty stream and the consumer seeds its own default posture; a present host emits the current `PermissionState` then every `change`, so a permission affordance renders transitions, never polls.
- Law: `navigator.connection` and the registration's `sync` member are absent from the DOM lib — `_NetSource` and `_SyncHost` are the boundary refinements pinned at this owner, the only places the nonstandard surfaces are spelled; a consumer never touches either.
- Receipt: `wake` answers `boolean` — registration accepted or capability absent — so boot stamps the wake posture without a probe.
- Growth: a new ambient signal (battery, memory pressure, page freeze) is one cell plus one owned fold on this service — never a sibling owner, never a consumer-side listener.
- Boundary: `otel/vital` owns RUM measurement; this cluster owns only the runtime-state cells its flush edges read; what drains on a redial is `shell#REPLAY_DRAIN`'s law.
- Packages: `effect` (`Effect`, `Option`, `Record`, `Stream`, `Subscribable`, `SubscriptionRef`); `@effect/platform-browser` (`BrowserStream.fromEventListenerWindow`, `BrowserStream.fromEventListenerDocument`).

```typescript
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

const _granted = (name: PermissionName): Stream.Stream<PermissionState> =>
  Stream.unwrap(
    Effect.tryPromise(() => globalThis.navigator.permissions.query({ name })).pipe(
      Effect.map((status) =>
        Stream.concat(
          Stream.succeed(status.state),
          Stream.map(Stream.fromEventListener(status, "change"), () => status.state),
        ),
      ),
      Effect.orElseSucceed(() => Stream.empty),
    ),
  )

class Connect extends Effect.Service<Connect>()("runtime/browser/Connect", {
  scoped: Effect.gen(function* () {
    const _online = yield* SubscriptionRef.make(globalThis.navigator.onLine)
    const _visible = yield* SubscriptionRef.make(globalThis.document.visibilityState === "visible")
    const _profile = yield* SubscriptionRef.make(Option.flatMap(_connection(), (source) => _profiled(source)))
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
      Effect.tryPromise(async () => {
        const registration = await globalThis.navigator.serviceWorker.ready
        if (!("sync" in registration)) return false
        await (registration as _SyncHost).sync.register(tag) // BOUNDARY ADAPTER: SyncManager is absent from the DOM lib; the refinement is pinned above
        return true
      }).pipe(Effect.orElseSucceed(() => false))
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
      granted: _granted,
    }
  }),
  accessors: true,
}) {}
```

## [05]-[CAPABILITY_ROWS]

[CAPABILITY_ROWS]:
- Owner: `Capability`, the Web-API service roster — one merged Layer satisfying the `Clipboard.Clipboard`, `Geolocation.Geolocation`, and `Permissions.Permissions` Tags with the platform's browser implementations, composed into the app root beside the transport rows `fetch#BINDING_ROWS` carries.
- Law: `ui` declares the capability port and this roster satisfies it at composition — `ui` never imports this package, so the copy affordance, the position watch (`Geolocation.watchPosition`), and the permission query reach components through the requirement channel only; a direct `navigator.clipboard`/`navigator.geolocation` touch in a view is the ungated-native-call defect.
- Law: each service carries its own tagged fault rail (`ClipboardError`, `GeolocationError`, `PermissionsError`) — the platform family rides untouched, and a consumer folds refusal as data, never a caught `DOMException`.
- Law: permission OBSERVATION rides `Connect.granted` — the platform `Permissions` service answers point queries, the change feed is the signal plane's fold — one observation owner, one query owner, never a second listener.
- Growth: a new Web-API capability (share, badging, wake-lock) lands as one roster row satisfying its `ui`-declared port; the roster is the single admission gate for browser-native surface.
- Boundary: which capabilities an app composes is root selection; the service member surfaces are the platform package's own.
- Packages: `@effect/platform-browser` (`Clipboard`, `Geolocation`, `Permissions`).

```typescript
const Capability: Layer.Layer<Clipboard.Clipboard | Geolocation.Geolocation | Permissions.Permissions> =
  Layer.mergeAll(Clipboard.layer, Geolocation.layer, Permissions.layer)

// --- [EXPORTS] --------------------------------------------------------------------------

export { AppSpec, Boot, Capability, Connect }
```
