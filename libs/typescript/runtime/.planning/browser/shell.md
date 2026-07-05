# [RUNTIME_SHELL]

The PWA shell plane: the web-app manifest as a typed VALUE the app constructs and the build encodes — never a hand-authored JSON asset — one `Workbox` instance held as a scoped resource owning registration and the update handshake, one `SwLifecycle` cell folded from the Workbox event target as the phase truth every affordance reads, the cache-strategy vocabulary as a closed row table projected type-only onto `workbox-build`'s `RuntimeCaching` shape for the app build to inject, the background-sync replay as one drain fold over `persist#DOMAIN_ROWS`'s `outbox` fed by every wake source at once, and the install/update affordance state as one owned cell. The altitude split is law: `workbox-build` emits the worker ASSET at app build, this module owns its RUNTIME lifecycle — a strategy row authored in the SW source, a raw `navigator.serviceWorker`/`caches` call outside this owner, a second `beforeinstallprompt` listener, or a second replay queue beside the outbox is the named two-owner defect. The update affordance distinguishes a genuine update from a first-install wait through the lifecycle event's own refinement flags, so a fresh install never renders a refresh prompt. The module is `runtime/src/browser/shell.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                  | [PUBLIC]                        |
| :-----: | :---------------- | :------------------------------------------------------------------------ | :------------------------------ |
|  [01]   | `MANIFEST_VALUE`  | the typed manifest owner and its emitted wire twin                        | `Manifest`                      |
|  [02]   | `LIFECYCLE_OWNER` | the Workbox resource, the phase cell, the update handshake, the fault     | `Sw`, `SwFault`, `SwLifecycle`  |
|  [03]   | `CACHE_ROWS`      | the strategy vocabulary, the `RuntimeCaching` projection, the build partial | `Sw.caching`, `Sw.build`      |
|  [04]   | `REPLAY_DRAIN`    | the outbox enqueue and the merged-wake drain fold                         | `Sw`                            |
|  [05]   | `INSTALL_OWNER`   | the prompt capture, the stance cell, the ask/refresh affordances          | `Install`, `InstallFault`, `InstallStance` |

## [2]-[MANIFEST_VALUE]

[MANIFEST_VALUE]:
- Owner: `Manifest`, one `Schema.Class` — `name`, `shortName`, `description`, `startUrl`, `scope`, `display` (the closed display-mode literal), `themeColor`/`backgroundColor` (`Option`-carried), and `icons` as a `Schema.NonEmptyArray` of icon rows — with every dialect spelling divergence handled as a field-level `Schema.fromKey` rename, so `shortName` encodes as `short_name`, `startUrl` as `start_url`, and no parallel wire interface exists.
- Law: the manifest is app DATA constructed at build — the app builds a `Manifest` value from its own identity and `Manifest.json` (the fused `Schema.parseJson` twin riding the owner as a static) encodes it to the `.webmanifest` string the build emits beside the precache; hydration and serving are the app build's, the shape law is this owner's.
- Law: non-emptiness is a type fact — an installable manifest without icons is unconstructible, so the PWA install criteria fail at compile time, never at an audit.
- Growth: a new manifest member (screenshots, shortcuts, share_target) is one field row with its rename; a new display mode is one literal on the axis.
- Boundary: cache identity and precache emission are `[3]`'s build rows; this owner carries only the manifest contract.
- Packages: `effect` (`Schema`, `Option`).

```typescript
import { FaultClass } from "@rasm/ts/core"
import { Data, DateTime, Effect, Option, Record, Ref, Schema, Stream, Subscribable, SubscriptionRef } from "effect"
import type { RuntimeCaching, StrategyName } from "workbox-build"
import { Workbox, type WorkboxLifecycleWaitingEvent } from "workbox-window"
import { Connect } from "./boot.ts"
import { Kv, type KvFault } from "./persist.ts"

const _Icon = Schema.Struct({
  src: Schema.NonEmptyString,
  sizes: Schema.NonEmptyString,
  type: Schema.NonEmptyString,
  purpose: Schema.optionalWith(Schema.Literal("any", "maskable", "monochrome"), { as: "Option" }),
})

class Manifest extends Schema.Class<Manifest>("Manifest")({
  name: Schema.NonEmptyString,
  shortName: Schema.propertySignature(Schema.NonEmptyString).pipe(Schema.fromKey("short_name")),
  description: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  startUrl: Schema.propertySignature(Schema.NonEmptyString).pipe(Schema.fromKey("start_url")),
  scope: Schema.NonEmptyString,
  display: Schema.Literal("standalone", "browser", "minimal-ui", "fullscreen"),
  themeColor: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }).pipe(Schema.fromKey("theme_color")),
  backgroundColor: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }).pipe(Schema.fromKey("background_color")),
  icons: Schema.NonEmptyArray(_Icon),
}) {
  static readonly json: Schema.SchemaClass<Manifest, string> = Schema.parseJson(Manifest)
}
```

## [3]-[LIFECYCLE_OWNER]

[LIFECYCLE_OWNER]:
- Owner: `Sw`, one scoped `Effect.Service` built through `Sw.Default(script)` — the `Workbox` instance acquired once over the app's build-emitted worker script and released by a final update check; `phase`, the `SwLifecycle` cell (`Unregistered`/`Installing`/`Installed`/`Waiting { update }`/`Active`/`Reloading`/`Redundant`) advanced only by the bridged event fold and published `Subscribable`, so the fold stays the one writer structurally; `update`, the poll for a fresh worker; `apply`, the update handshake; `reports`, the decoded window-bound message feed; `signal`, the request/response channel into the live worker.
- Law: the event bridge is the module's platform-forced statement seam — `Stream.asyncScoped` acquires one listener per lifecycle tag against the `Workbox` event target and releases every one on scope close; the implementer carries the `// BOUNDARY ADAPTER` mark on `_lifecycle`'s first line. Non-waiting tags advance the cell through the keyed `_PHASES` lookup — a `Match` ladder over a keyed correspondence is the rejected dispatch — and `controlling` alone carries logic: it reads the prior phase and reloads the document exactly once when that phase is `Reloading`.
- Law: the `waiting` arm reads the event's own refinement flags — `Waiting.update` holds `isUpdate === true || wasWaitingBeforeRegister === true`, so a first-install wait and a genuine update are distinct facts in the phase cell and `Install.fresh` gates on the flag, never on the bare phase.
- Law: the apply order is load-bearing — `apply` sets `Reloading` and only then calls `messageSkipWaiting`, so the `controlling` arm observes the intent and a `controllerchange` arriving for any other reason never reloads.
- Law: `unsupported` short-circuits at construction — a host without `navigator.serviceWorker` yields the service in its inert posture (`Unregistered` forever, `register` answering the typed fault) so every consumer folds capability absence as data.
- Law: the fault is class-carried — `SwFault.class` projects the reason row into the core `FaultClass` vocabulary, so budget gates and severity folds read the one branch taxonomy and no local rank table exists.
- Receipt: `register` yields `boolean` — controlled now or awaiting first load — and the phase cell carries everything else; no consumer touches the registration object.
- Boundary: the refresh affordance is `[6]`'s read; the worker asset, precache manifest, and offline shell are the app build's, emitted through `workbox-build`'s `injectManifest` over the authored SW.
- Packages: `workbox-window` (`Workbox`, the lifecycle event map, `WorkboxLifecycleWaitingEvent`); `effect` (`Data`, `DateTime`, `Effect`, `Option`, `Record`, `Ref`, `Schema`, `Stream`, `Subscribable`, `SubscriptionRef`); `@rasm/ts/core` (`FaultClass`).

```typescript
type SwLifecycle = Data.TaggedEnum<{
  Unregistered: {}
  Installing: {}
  Installed: {}
  Waiting: { readonly update: boolean }
  Active: {}
  Reloading: {}
  Redundant: {}
}>
const SwLifecycle: Data.TaggedEnum.Constructor<SwLifecycle> = Data.taggedEnum<SwLifecycle>()

const _swFaults = {
  unsupported: { class: "absent" },
  register: { class: "unavailable" },
  message: { class: "conflicted" },
} as const

declare namespace SwFault {
  type Reason = keyof typeof _swFaults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _swFaults> = T
}

class SwFault extends Data.TaggedError("SwFault")<{
  readonly reason: SwFault.Reason
  readonly detail: string
}> {
  get class(): FaultClass.Kind {
    return _swFaults[this.reason].class
  }
}

const _TAGS = ["installing", "installed", "waiting", "activating", "activated", "redundant"] as const

const _PHASES: Record<Exclude<(typeof _TAGS)[number], "waiting">, SwLifecycle> = {
  installing: SwLifecycle.Installing(),
  installed: SwLifecycle.Installed(),
  activating: SwLifecycle.Active(),
  activated: SwLifecycle.Active(),
  redundant: SwLifecycle.Redundant(),
}

const _Report = Schema.Union(
  Schema.TaggedStruct("Replayed", { queue: Schema.NonEmptyString, remained: Schema.Int.pipe(Schema.nonNegative()) }),
  Schema.TaggedStruct("Claimed", { build: Schema.NonEmptyString }),
)

type _Signal = { readonly tag: (typeof _TAGS)[number] | "controlling"; readonly update: boolean }

const _lifecycle = (wb: Workbox): Stream.Stream<_Signal> =>
  Stream.asyncScoped((emit) =>
    Effect.acquireRelease(
      Effect.sync(() =>
        [..._TAGS, "controlling" as const].map((tag) => {
          const handler = (event: { readonly isUpdate?: boolean }) =>
            void emit.single({
              tag,
              update: event.isUpdate === true ||
                (event as WorkboxLifecycleWaitingEvent).wasWaitingBeforeRegister === true,
            })
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

## [4]-[CACHE_ROWS]

[CACHE_ROWS]:
- Owner: the interior `_STRATEGIES` lookup (the four-grade strategy axis onto `workbox-build`'s `StrategyName`) and the `_lanes` route table — `asset` (hashed static assets: cache-first, long retention), `api` (API reads: network-first with a raced timeout and the SW-side replay queue), `band` (media, tiles, GLB and KTX2 byte bands: stale-while-revalidate under an entry cap with partial-content range service) — each row carrying its URL pattern source, retention, raced timeout, replay-queue name, and range posture; `Sw.caching(mark)` projects the table to `ReadonlyArray<RuntimeCaching>` with every cache name stamped by the build fingerprint, and `Sw.build(spec)` projects the build partial — the offline app-shell route, the network-first HTML race, and the cache identity — the app's `injectManifest` config spreads.
- Law: `workbox-build` is type-only here — `RuntimeCaching` and `StrategyName` type imports keep the strategy rows one source of truth across the build/runtime seam while the Node-only value surface never enters the bundle; the app build script value-imports these projections and hands them to `injectManifest` beside the authored SW.
- Law: the fingerprint stamps identity — `mark` derives from the core `AppIdentity` build block at the composition root, so a build bump mints fresh cache names and a stale worker cannot serve mixed generations; the precache side inherits the same identity through `Sw.build`'s `cacheId`.
- Law: a cache behavior is a row, never a branch — the `sync` column names a replayable lane's SW-side queue, the `range` column arms `rangeRequests` plus its `cacheableResponse` admission for the large byte bands partial-content readers seek into, and the `race` column pairs `networkTimeoutSeconds` with `Sw.build`'s `navigationPreload` so the network-first HTML route races the cache.
- Law: `Sw.build` never sets `skipWaiting` — the update lands through `[3]`'s apply handshake so a refresh is user intent, never a mid-session takeover; `cleanupOutdatedCaches` and `clientsClaim` stay armed because generation hygiene has no user ceremony.
- RESEARCH: the `BroadcastCacheUpdateOptions` member spellings behind `RuntimeCaching.options.broadcastUpdate` are unverified; the cache-update announce column lands on the row table only after the folder catalogue documents them.
- Growth: a new cache posture is one `_lanes` row; a new strategy grade is one `_STRATEGIES` entry — the row type breaks until both align.
- Packages: `workbox-build` (type-only `RuntimeCaching`, `StrategyName`); `effect` (`Option`, `Record`).

```typescript
const _STRATEGIES = {
  bank: "CacheFirst",
  live: "NetworkFirst",
  tile: "StaleWhileRevalidate",
  pass: "NetworkOnly",
} as const satisfies Record<string, StrategyName>

const _lanes = {
  asset: { strategy: "bank", pattern: "\\.(?:js|css|wasm|woff2)$", keep: { entries: 256, days: 30 }, sync: Option.none<string>(), race: Option.none<number>(), range: false },
  api: { strategy: "live", pattern: "/api/", keep: { entries: 64, days: 1 }, sync: Option.some("rasm-outbox"), race: Option.some(3), range: false },
  band: { strategy: "tile", pattern: "\\.(?:glb|ktx2|png|webp|pmtiles)$", keep: { entries: 128, days: 7 }, sync: Option.none<string>(), race: Option.none<number>(), range: true },
} as const

declare namespace _lanes {
  type Kind = keyof typeof _lanes
  type Row = {
    readonly strategy: keyof typeof _STRATEGIES
    readonly pattern: string
    readonly keep: { readonly entries: number; readonly days: number }
    readonly sync: Option.Option<string>
    readonly race: Option.Option<number>
    readonly range: boolean
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
        ...(row.range ? { rangeRequests: true, cacheableResponse: { statuses: [0, 200] } } : {}),
        ...Option.match(row.sync, { onNone: () => ({}), onSome: (name) => ({ backgroundSync: { name } }) }),
        ...Option.match(row.race, { onNone: () => ({}), onSome: (seconds) => ({ networkTimeoutSeconds: seconds }) }),
      },
    })),
  )

const _build = (spec: { readonly mark: string; readonly shell: string }): {
  readonly cacheId: string
  readonly cleanupOutdatedCaches: true
  readonly clientsClaim: true
  readonly navigateFallback: string
  readonly navigationPreload: true
} => ({
  cacheId: spec.mark,
  cleanupOutdatedCaches: true,
  clientsClaim: true,
  navigateFallback: spec.shell,
  navigationPreload: true,
})
```

## [5]-[REPLAY_DRAIN]

[REPLAY_DRAIN]:
- Owner: the outbox lane on the same service — `queue(band)` appends one durable entry (minted instant plus the caller's already-encoded payload band) into `persist#DOMAIN_ROWS`'s `outbox` domain under a monotonic key; `relayed(relay)` is the single drain fold: every wake source — `Connect.redials`, the worker's `Replayed` reports, and the one-shot `Connect.wake` registration at construction — merges into one stream, each wake drains the outbox atomically and hands every entry to the app-supplied `relay` leg in minted order, and every refused entry re-enqueues in ONE atomic batch write so nothing drops silently and a mid-compensation crash re-enqueues all or none.
- Law: the element is opaque here — the outbox row is `Kv.Value<"outbox">` (minted, band); the producing rail encoded the band and the relay leg decodes it, so this fold never inspects payloads and the wire vocabulary stays with its owner.
- Law: one queue, two altitudes — failed same-origin fetches replay inside the SW through the `workbox-background-sync` queue the cache rows configure; app-level intents replay here through the window drain; the `Replayed` report is the seam where the SW's drain completion wakes the window's, so the two altitudes converge without sharing storage.
- Law: the drain is serial and self-quenching — `relay` runs per entry with the fold awaiting each, the refused set gathers through `Effect.partition` (the atomic drain already cleared the store, so the batch re-write is the compensation), a storage fault folds the whole wake to a no-op the next wake redrives, and a wake arriving mid-drain queues behind the running fold rather than starting a second.
- Boundary: what the band contains and where `relay` dials is the composing app's selection over `fetch#DIAL_SURFACE`; this cluster owns durability, ordering, and wake fan-in only.
- Packages: `effect` (`DateTime`, `Effect`, `Ref`, `Stream`); `./boot.ts` (`Connect`); `./persist.ts` (`Kv`).

```typescript
class Sw extends Effect.Service<Sw>()("runtime/browser/Sw", {
  scoped: (script: string) =>
    Effect.gen(function* () {
      const connect = yield* Connect
      const kv = yield* Kv
      const _phase = yield* SubscriptionRef.make<SwLifecycle>(SwLifecycle.Unregistered())
      const carried = "serviceWorker" in globalThis.navigator
      const wb = yield* Effect.acquireRelease(
        Effect.sync(() => new Workbox(script)),
        (held) => (carried ? Effect.ignore(Effect.tryPromise(() => held.update())) : Effect.void),
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
        Stream.runForEach((signal) =>
          signal.tag === "controlling"
            ? Effect.flatMap(SubscriptionRef.get(_phase), (held) =>
                SwLifecycle.$is("Reloading")(held)
                  ? Effect.sync(() => globalThis.location.reload())
                  : SubscriptionRef.set(_phase, SwLifecycle.Active()),
              )
            : signal.tag === "waiting"
              ? SubscriptionRef.set(_phase, SwLifecycle.Waiting({ update: signal.update }))
              : SubscriptionRef.set(_phase, _PHASES[signal.tag]),
        ),
        Effect.forkScoped,
      )
      const counter = yield* Ref.make(0)
      const queue = (band: Uint8Array): Effect.Effect<void, KvFault> =>
        Effect.gen(function* () {
          const minted = yield* DateTime.now
          const turn = yield* Ref.getAndUpdate(counter, (n) => n + 1)
          yield* kv.write("outbox", `${DateTime.toEpochMillis(minted)}:${String(turn).padStart(8, "0")}`, { minted, band })
        })
      const relayed = <E, R>(relay: (entry: Kv.Value<"outbox">) => Effect.Effect<void, E, R>) =>
        Stream.merge(
          connect.redials,
          Stream.as(Stream.filter(reports, (report) => report._tag === "Replayed"), undefined),
        ).pipe(
          Stream.runForEach(() =>
            kv.drain("outbox").pipe(
              Effect.flatMap((held) =>
                Effect.flatMap(
                  Effect.partition(held, ([key, entry]) => Effect.mapError(relay(entry), () => [key, entry] as const), {
                    concurrency: 1,
                  }),
                  ([refused]) => (refused.length === 0 ? Effect.void : Effect.ignore(kv.write("outbox", refused))),
                ),
              ),
              Effect.catchAll(() => Effect.void),
            ),
          ),
          Effect.forkScoped,
        )
      yield* Effect.ignore(connect.wake("rasm-outbox"))
      const phase: Subscribable.Subscribable<SwLifecycle> = _phase
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
        apply: SubscriptionRef.set(_phase, SwLifecycle.Reloading()).pipe(
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
  static readonly build: typeof _build = _build
}
```

## [6]-[INSTALL_OWNER]

[INSTALL_OWNER]:
- Owner: `Install`, one scoped `Effect.Service` over `Sw` — `stance`, the `InstallStance` cell (`Browser`/`Ready`/`Installed`/`Standalone`) published `Subscribable` with its writers interior; `ask`, the install affordance firing the captured prompt and folding the user's choice; `fresh`, the update-available feed derived from the worker's `Waiting` phase gated on its `update` flag; `refresh`, the one update verb delegating to `Sw.apply`.
- Law: the prompt capture is the module's platform-forced statement seam — `beforeinstallprompt` is nonstandard (absent from `WindowEventMap`, spelled here once as the `_PromptEvent` boundary refinement) and its `preventDefault` runs synchronously inside the native handler or the browser consumes the prompt; the capture bridge therefore attaches its own listener under `Stream.asyncScoped`, deferring and emitting in the same synchronous frame, and the implementer carries the `// BOUNDARY ADAPTER` mark on `_prompts`' first line.
- Law: the prompt is single-use and the take is atomic — `ask` swaps the slot to `none` in one `modify`, so two racing asks cannot double-fire, then folds `accepted` to `Installed` and `dismissed` back to `Browser`; an `ask` with no captured prompt is the typed `unavailable` fault, never a silent no-op.
- Law: stance is evidence-ordered — the standalone display-mode probe (seeded from `matchMedia`, advanced by its `change` events) and the `appinstalled` event both fold to their stance directly; running standalone is terminal for the session, so no later fold demotes it.
- Law: the update affordance is a derivation, not a state — `fresh` maps the worker phase feed through the `Waiting` refinement AND its `update` flag, so a first-install wait renders nothing, install and update read one truth, and the ui wave binds both through its atom bridge at app composition; this module exposes no second phase cell.
- Receipt: `ask` yields the fold's stance so the caller renders the outcome without re-reading the cell.
- Boundary: the worker phase and the apply handshake are `[3]`'s; the affordance rendering is the ui wave's through the app-composed port.
- Packages: `effect` (`Data`, `Effect`, `Option`, `Stream`, `Subscribable`, `SubscriptionRef`); `@rasm/ts/core` (`FaultClass`).

```typescript
type InstallStance = Data.TaggedEnum<{
  Browser: {}
  Ready: {}
  Installed: {}
  Standalone: {}
}>
const InstallStance: Data.TaggedEnum.Constructor<InstallStance> = Data.taggedEnum<InstallStance>()

const _installFaults = {
  unavailable: { class: "absent" },
  ceremony: { class: "denied" },
} as const

declare namespace InstallFault {
  type Reason = keyof typeof _installFaults
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _installFaults> = T
}

class InstallFault extends Data.TaggedError("InstallFault")<{
  readonly reason: InstallFault.Reason
  readonly detail: string
}> {
  get class(): FaultClass.Kind {
    return _installFaults[this.reason].class
  }
}

type _PromptEvent = Event & {
  readonly prompt: () => Promise<void>
  readonly userChoice: Promise<{ readonly outcome: "accepted" | "dismissed" }>
}

const _prompts: Stream.Stream<_PromptEvent> = Stream.asyncScoped((emit) =>
  Effect.acquireRelease(
    Effect.sync(() => {
      const handler = (event: Event) => {
        event.preventDefault()
        void emit.single(event as _PromptEvent)
      }
      globalThis.addEventListener("beforeinstallprompt", handler)
      return handler
    }),
    (handler) => Effect.sync(() => globalThis.removeEventListener("beforeinstallprompt", handler)),
  ),
)

class Install extends Effect.Service<Install>()("runtime/browser/Install", {
  scoped: Effect.gen(function* () {
    const sw = yield* Sw
    const standalone = yield* Effect.sync(() => globalThis.matchMedia("(display-mode: standalone)"))
    const _stance = yield* SubscriptionRef.make<InstallStance>(
      standalone.matches ? InstallStance.Standalone() : InstallStance.Browser(),
    )
    const held = yield* SubscriptionRef.make(Option.none<_PromptEvent>())
    yield* _prompts.pipe(
      Stream.runForEach((prompt) =>
        SubscriptionRef.set(held, Option.some(prompt)).pipe(
          Effect.zipRight(SubscriptionRef.set(_stance, InstallStance.Ready())),
        ),
      ),
      Effect.forkScoped,
    )
    yield* Stream.fromEventListener(globalThis, "appinstalled").pipe(
      Stream.runForEach(() => SubscriptionRef.set(_stance, InstallStance.Installed())),
      Effect.forkScoped,
    )
    yield* Stream.fromEventListener(standalone, "change").pipe(
      Stream.runForEach(() =>
        standalone.matches ? SubscriptionRef.set(_stance, InstallStance.Standalone()) : Effect.void,
      ),
      Effect.forkScoped,
    )
    const ask: Effect.Effect<InstallStance, InstallFault> = Effect.gen(function* () {
      const slot = yield* SubscriptionRef.modify(held, (taken) => [taken, Option.none<_PromptEvent>()] as const)
      const prompt = yield* Option.match(slot, {
        onNone: () => new InstallFault({ reason: "unavailable", detail: "<no-captured-prompt>" }),
        onSome: Effect.succeed,
      })
      const choice = yield* Effect.tryPromise({
        try: () => prompt.prompt().then(() => prompt.userChoice),
        catch: (cause) => new InstallFault({ reason: "ceremony", detail: String(cause) }),
      })
      const landed = choice.outcome === "accepted" ? InstallStance.Installed() : InstallStance.Browser()
      yield* SubscriptionRef.set(_stance, landed)
      return landed
    })
    const stance: Subscribable.Subscribable<InstallStance> = _stance
    return {
      stance,
      ask,
      fresh: Stream.map(sw.phase.changes, (phase) => (SwLifecycle.$is("Waiting")(phase) ? phase.update : false)),
      refresh: sw.apply,
    }
  }),
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Install, InstallFault, InstallStance, Manifest, Sw, SwFault, SwLifecycle }
```
