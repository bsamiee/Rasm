# [RUNTIME_BOOT]

`runtime/src/browser/boot.ts` owns the browser boot plane: one `BrowserRuntime.runMain` call boots the document, and `Boot.make` mints the ONE `ManagedRuntime` handle the document run and every host call-in share. This page also owns the ambient host-signal plane — connectivity, visibility, network profile, permission state — and the Web-API capability roster the root merges so `ui`-declared ports resolve to platform Layers at composition. Which Layer families an app selects into its root, and what each feed drives, stay at their owning pages.

`AppSpec` is the budget VALUE an app constructs before anything runs: its `Identity.App`, its feed rows, and the ceilings this folder's layer factories consume. Host signals live in one owned cell advanced only by its owned fold, so every consumer reads the cell and never the navigator, and capability refusal splits absence from decision. Render posture is settled: client-rendered PWA beside build-time prerender rows owns the SEO surface, `Boot.hydrated` hands the prerendered document to the mount, and a streaming-SSR server runtime is the named non-goal.

## [01]-[INDEX]

- [02]-[BUDGET_VALUE]: the `AppSpec` shape — identity, feed rows, ceilings; `AppSpec`.
- [03]-[SINGLE_BOOT]: the one `runMain` entry, the spec Tag, the call-in handle, the hydration read; `Boot`.
- [04]-[SIGNAL_CELLS]: the seeded cells, their owned folds, the derived edges, wake, permission feed; `Connect`, `ConnectFault`.
- [05]-[CAPABILITY_ROWS]: the Web-API service roster the root merges; `Capability`.

## [02]-[BUDGET_VALUE]

[BUDGET_VALUE]:
- Law: the lane axis is `net/client`'s, governed both ways — the interior tuple `satisfies` `Client.Lane` and the `_Spans` guard demands full coverage, so a lane rename or addition on the client table breaks this anchor at compile time, never as a stale budget row.
- Law: the spec is constructed once from validated material — `proc/config`'s provider chain resolves the identity at boot and the app assembles the value; nothing downstream re-reads an environment, and the spec's decode is the one admission of the budget.
- Law: consumers read rows, never re-declare knobs — the composition root threads spec rows into layer factories, so retuning an app is editing its spec value with zero lib edits.
- Growth: a new budget axis is one `ceilings` field; a new feed fact is one field on the feed row.
- Boundary: what each feed DOES is the owning page's law; this owner counts and types them.
- Packages: `@rasm/core` (`Identity.App`); `effect` (`Schema`); `../net/client.ts` (type `Client`).

```typescript
import { BrowserRuntime, BrowserStream, Clipboard, Geolocation, Permissions } from "@effect/platform-browser"
import { Fault, Identity } from "@rasm/core"
import { Array, Context, Effect, Layer, ManagedRuntime, Option, Record, Schema, Stream, Subscribable, SubscriptionRef } from "effect"
import type { Client } from "../net/client.ts"

const _LANES = ["live", "batch", "feed"] as const satisfies ReadonlyArray<Client.Lane>

const _Feed = Schema.Struct({
  name: Schema.NonEmptyString,
  lane: Schema.Literal(..._LANES),
})

class AppSpec extends Schema.Class<AppSpec>("AppSpec")({
  identity: Identity.App,
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
- Owner: `Boot`, one `Context.Tag` class — the Tag itself is the spec slot, so a service that earns a budget read writes `yield* Boot` with zero second hop; `Boot.make(spec, root)` mints the ONE `ManagedRuntime` for the document — the spec provided beneath the app-selected root, built under the module's one `Layer.MemoMap` — and every downstream entry holds this handle; `Boot.main(handle, app)`, the one `BrowserRuntime.runMain` entry riding it as a static: the app effect annotated with the identity stamp and provided FROM the handle (`Effect.provide` accepts a `ManagedRuntime`, so the document graph and every call-in observe the same scoped instances), with the handle's `dispose` chained into the boot teardown; `Boot.hydrated`, the prerender handoff read.
- Law: `main` is called exactly once per document, from the app's `main.ts`, and that module exports nothing — the empty surface is the structural proof it is terminal; every other module in the branch is barred from any `run*` call, and `fetch#RUNNER_ENTRY` is the one sibling boot, its own thread's.
- Law: one graph, one owner per acquisition — the document run and the imperative call-in (`handle.runPromise` from the view atom bridge, a web-component mount, a foreign SDK callback) both resolve through the handle `make` minted, so a scoped service has exactly one construction and one teardown path; a second `ManagedRuntime` or a run-main graph built beside the handle is the per-call-rebuild defect this owner makes unspellable.
- Law: the boot line is the only imperative boundary — `runMain` installs error reporting and teardown wiring; `disableErrorReporting`/`disablePrettyLogger` stay default because crash visibility is `otel/crash`'s Layer concern, never a boot flag.
- Law: hydration is boot's law — the build emits per-route static HTML stamped with the `data-rasm-prerender` marker; `Boot.hydrated` reads the marker (`Option`-carried) so the app's mount takes over a prerendered document instead of re-rendering it, and a document without the marker is a cold client render; the marker read is this cluster's one DOM touch.
- Law: teardown rides the typed effect the handle already publishes — `disposeEffect` is `Effect<void>`, and a promise round-trip grades a rejecting scope finalizer `defect` on the shutdown path, past every gate that acts on it.
- Exemption: the `_memo` mint is the one platform-forced boot run call, and this module is the edge where it is legal.
- Output: `main` returns `void` — everything observable thereafter flows through the composed graph; the annotation on `main`'s signature is the whole boot contract.
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
        Effect.provide(handle),
        Effect.ensuring(handle.disposeEffect),
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
- Law: host refusal splits three ways, never onto one value — an absent surface is data the cell already carries, a name the agent cannot parse is a caller fault it grades `absent`, and a registration or query the agent refuses is a decision a caller re-drives, so `ConnectFault` carries the two that reach the error channel and `orElseSucceed` over a whole probe is the collapse this owner forecloses.
- Law: `navigator.connection` and the registration's `sync` member are absent from the DOM lib, so `_NetSource` and `_SyncHost` are this owner's boundary refinements and every byte-budget consumer reads the cell instead of the navigator; `otel/vital` pins its own `connection.type` refinement for the RUM stamp, because dependency direction bars an otel module from reading a browser cell, so the transport word and the byte-budget axis hold one owner each.
- Output: `wake` answers `boolean` — registration accepted or capability absent — so boot stamps the wake posture without a probe, while a refused registration rides the error channel because an agent that just refused is re-drivable and an absent capability never is.
- Growth: a new ambient signal (battery, memory pressure, page freeze) is one cell and one owned fold on this service — never a sibling owner, never a consumer-side listener.
- Boundary: `otel/vital` owns RUM measurement; this cluster owns only the runtime-state cells its flush edges read; what drains on a redial is `shell#REPLAY_DRAIN`'s law.
- Packages: `effect` (`Effect`, `Option`, `Record`, `Schema`, `Stream`, `Subscribable`, `SubscriptionRef`); `@effect/platform-browser` (`BrowserStream.fromEventListenerWindow`, `BrowserStream.fromEventListenerDocument`); `@rasm/core` (`Fault.Class`).

```typescript
const _GRADES = { "4g": "swift", "3g": "steady", "2g": "strained", "slow-2g": "strained" } as const

const _connectFamily = Fault.Class.family(["unparsed", "refused"] as const, {
  unparsed: Fault.Class.row({
    class: "absent",
    leg: "signal",
    detail: Schema.Struct({ name: Schema.String }),
    render: ({ name }) => `the agent parses no permission descriptor named ${name}`,
  }),
  refused: Fault.Class.row({
    class: "denied",
    leg: "signal",
    detail: Schema.Struct({ surface: Schema.Literal("permissions.query", "sync.register"), cause: Schema.String }),
    render: ({ surface, cause }) => `the agent refused ${surface}: ${cause}`,
  }),
})

declare namespace Connect {
  type Grade = (typeof _GRADES)[keyof typeof _GRADES]
  type Profile = { readonly grade: Grade; readonly frugal: boolean }
}

class ConnectFault extends Schema.TaggedError<ConnectFault>()("ConnectFault", {
  case: _connectFamily.payload,
}) {
  get class(): Fault.Class.Kind {
    return _connectFamily.classOf(this.case.reason)
  }
  override get message(): string {
    return _connectFamily.render(this.case)
  }
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

const _permissions = (): Option.Option<Navigator["permissions"]> => Option.fromNullable(globalThis.navigator.permissions)

const _granted = (name: PermissionName): Stream.Stream<PermissionState, ConnectFault> =>
  Option.match(_permissions(), {
    onNone: () => Stream.empty,
    onSome: (permissions) =>
      Stream.unwrap(
        Effect.tryPromise({
          try: () => permissions.query({ name }),
          catch: (defect) =>
            new ConnectFault({
              case: defect instanceof globalThis.TypeError
                ? { reason: "unparsed", name }
                : { reason: "refused", surface: "permissions.query", cause: String(defect) },
            }),
        }).pipe(
          Effect.map((status) =>
            Stream.concat(
              Stream.succeed(status.state),
              Stream.map(Stream.fromEventListener(status, "change"), () => status.state),
            ),
          ),
        ),
      ),
  })

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
    const wake = (tag: string): Effect.Effect<boolean, ConnectFault> =>
      Option.match(Option.fromNullable(globalThis.navigator.serviceWorker), {
        onNone: () => Effect.succeed(false),
        onSome: (container) =>
          Effect.tryPromise({
            try: async () => {
              const registration = await container.ready
              if (!("sync" in registration)) return false
              await (registration as _SyncHost).sync.register(tag)
              return true
            },
            catch: (defect) =>
              new ConnectFault({ case: { reason: "refused", surface: "sync.register", cause: String(defect) } }),
          }),
      })
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
- Law: each service carries its own tagged fault channel (`ClipboardError`, `GeolocationError`, `PermissionsError`) — the platform family rides untouched, and a consumer folds refusal as data, never a caught `DOMException`.
- Law: permission OBSERVATION rides `Connect.granted` — the platform `Permissions` service answers point queries, the change feed is the signal plane's fold — one observation owner, one query owner, never a second listener.
- Growth: a new Web-API capability (share, badging, wake-lock) lands as one roster row satisfying its `ui`-declared port; the roster is the single admission gate for browser-native surface.
- Boundary: which capabilities an app composes is root selection; the service member surfaces are the platform package's own.
- Packages: `@effect/platform-browser` (`Clipboard`, `Geolocation`, `Permissions`).

```typescript
const Capability: Layer.Layer<Clipboard.Clipboard | Geolocation.Geolocation | Permissions.Permissions> =
  Layer.mergeAll(Clipboard.layer, Geolocation.layer, Permissions.layer)

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppSpec, Boot, Capability, Connect, ConnectFault }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
