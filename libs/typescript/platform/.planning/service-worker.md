# [PLATFORM_SERVICE_WORKER]

One page owns the browser service-worker / PWA + offline-first cache concern — `ServiceWorkerHost`, the Effect-native registration and update-lifecycle owner over `workbox-window`; `CacheStrategy`, the one `Schema.Literal` route-strategy axis (`cache-first`/`network-first`/`stale-while-revalidate`) the Workbox precache and runtime-cache routes resolve against; and `BackgroundSyncReplay`, the redial-driven drain of the `platform` `platform-substrate` `LocalPersistence.offlineQueue` into the `interchange` `CommandGateway`. `BuildPipeline` emits the worker asset at build time; `ServiceWorkerHost` owns its runtime lifecycle — the two are distinct altitudes over one concern, never a two-owner split. The page composes `vite-plugin-pwa`, `workbox-build`, and `workbox-window`, holds no domain state, and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                 | [STATE]                                                                                |
| :-----: | :------------- | :--------------------- | :------------------------------------------------------------------------------------- |
|   [1]   | SERVICE_WORKER | registration lifecycle | `ServiceWorkerHost` SPIKE (live-browser install/activate/`skipWaiting` probe)          |
|   [2]   | SERVICE_WORKER | cache-strategy axis    | `CacheStrategy` FINALIZED                                                              |
|   [3]   | SERVICE_WORKER | background-sync replay | `BackgroundSyncReplay` FINALIZED (gated: `platform-substrate` `offlineQueue` widening) |

`CacheStrategy` is transcription-complete against the verified `workbox-build` `RuntimeCaching`/`StrategyName` APIs with no open gate. `ServiceWorkerHost` is fence-complete against `workbox-window` `Workbox` and `@effect/platform-browser` `BrowserStream`; it stays SPIKE on the one genuine live-browser install/activate/`skipWaiting` lifecycle probe the charter PROOF_GATES `sw lifecycle spike` row tracks (matching charter DENSITY_BAR row [11]). `BackgroundSyncReplay` is fence-complete — `AvailabilityStore` surfaced in `R`, `FaultDetail` re-enqueue via `Effect.catchAll`, body authored against the resolved-intent element shape — but carries the cross-page GAP below: its `offlineQueue` element shape is owned by `platform-substrate.md` and must widen to `{ verb: ControlVerb; payload: CommandPayloadWire }` before this owner compiles end-to-end.

## [2]-[SERVICE_WORKER]

- Owner: `ServiceWorkerHost`, the single service-worker runtime owner — registration through `workbox-window` `Workbox` held as an `Effect.acquireRelease` resource over the registration, the `installing`/`installed`/`waiting`/`activated`/`controlling`/`redundant` lifecycle stream bridged off the `Workbox` event target through `Stream.asyncScoped` and folded into one `SwLifecycle` `SubscriptionRef`, the `messageSkipWaiting` update handshake reloading on the next `controlling` event, the update-available cell the `ui` surfaces a refresh affordance from, and the offline-first navigation fallback; `CacheStrategy`, the `Schema.Literal` route-strategy axis with a `StrategyBehavior` `Record` projecting each route row to its verified Workbox `RuntimeCaching` entry; and `BackgroundSyncReplay`, the redial drain reading `LocalPersistence.offlineQueue` into the `interchange` `CommandGateway`. The cache-strategy literal is the one route-strategy vocabulary and a hand-written `caches.open(...)` call outside it is the named defect.
- Cases: `CacheStrategy` is one `Schema.Literal` union (`cache-first`/`network-first`/`stale-while-revalidate`/`network-only`) folded by `strategyName` to the verified workbox `StrategyName` (`CacheFirst`/`NetworkFirst`/`StaleWhileRevalidate`/`NetworkOnly`), with a `StrategyBehavior` row carrying each route `urlPattern`, cache name, and `maxEntries`/`maxAgeSeconds` expiration and `runtimeCaching` projecting the row to a `workbox-build` `RuntimeCaching` entry, so the static-asset `cache-first` route, the API `network-first` route, and the tile/glb `stale-while-revalidate` route are each one row, never a per-route imperative handler; `ServiceWorkerHost` registers the build-emitted worker through `workbox-window` `Workbox` and folds the `installing`/`installed`/`waiting`/`activated`/`controlling`/`redundant` events into one `SwLifecycle` `SubscriptionRef`, so the `waiting` event flips the update-available cell and the `ui` refresh affordance reads it through the one `AtomBinding`; an accepted refresh sends `messageSkipWaiting` and reloads on the next `controlling` event, so the update handshake is one `Match.value` fold over the Workbox event stream rather than scattered `postMessage` calls; the offline-first navigation fallback is the precache-route `NavigationRoute` the worker asset carries, so a reload while offline serves the precached app-shell rather than a browser error page.
- Auto: `BackgroundSyncReplay` is the redial-triggered drain — it subscribes to the `online` window event through `BrowserStream.fromEventListenerWindow("online")`, registers a native `SyncManager` sync tag where present as the background wake, and on each redial reads `LocalPersistence.offlineQueue.drainOnRedial` (yielding `ReadonlyArray<{ verb: ControlVerb; payload: CommandPayloadWire }>` from the `platform-substrate` owner verbatim — the same resolved-intent pair `interchange` `IntentRegistry.resolve` yields) and replays each queued element through the `interchange` `CommandGateway.invoke`; the gateway's `AvailabilityStore` requirement rides the `invoke` `R` channel and stays surfaced in `run`'s `R` (provided at the SPA composition root per the `interchange` charter, never erased here), so the same offline-queue seam `platform-substrate` names is drained by one owner on reconnect rather than a parallel SW-resident queue; a replay whose gateway invoke faults re-enqueues the element through `offlineQueue.enqueue` via `Effect.catchAll` over the one `interchange` `FaultDetail` error channel — every one of its five tags re-enqueued, never a silent drop. The native `SyncManager` registration and the `online`-event drain both feed the one drain fold; the `SyncManager` absence on a host is a `serviceOption`-style optional capability, never a parallel queue.
- Packages: `workbox-window` for the `Workbox` registration and the lifecycle event target bridged through `Stream.asyncScoped`; `workbox-build` `RuntimeCaching`/`StrategyName` for the `StrategyBehavior` projection and `vite-plugin-pwa` for the build-time precache manifest and the worker asset `BuildPipeline` emits; `@effect/platform-browser` `BrowserStream.fromEventListenerWindow` for the `online` ingress; `effect` `Schema.Literal` for the strategy axis, `SubscriptionRef` for the lifecycle and update-available cells, `Data.taggedEnum` for `SwLifecycle`, `Match` for the lifecycle fold, and `Stream`/`Effect.acquireRelease`/`Effect.forkScoped` for the registration resource and the drain.
- Growth: a new cache route lands as one literal on `CacheStrategy` and one `StrategyBehavior` `Record` row; a new lifecycle signal lands as one arm on the `SwLifecycle` `Match` fold; a new offline-replay concern lands as one row on `BackgroundSyncReplay`, never a parallel queue or a second worker.
- Boundary: `ServiceWorkerHost` owns the SW RUNTIME lifecycle (install/activate/cache strategy/background-sync) and `BuildPipeline` (platform-substrate) EMITS the worker ASSET at build time — distinct altitudes, one concern each, so a strategy or lifecycle row authored on `BuildPipeline` is the named two-owner-one-concern defect; `BackgroundSyncReplay` drains the single `LocalPersistence.offlineQueue` the `platform-substrate` page owns — consuming `drainOnRedial`/`enqueue` over the `{ verb: ControlVerb; payload: CommandPayloadWire }` resolved-intent element verbatim (the queue element widening is owned by `platform-substrate.md`, NOTED as the page GAP) — and replays through the `interchange` `CommandGateway.invoke` across the folder seam, never re-dialing a transport here and never holding a parallel offline queue; a direct `caches`/`navigator.serviceWorker` call outside `ServiceWorkerHost` is the named defect; `ui` reads the update-available cell through the `AtomBinding` and never imports `platform`.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { RuntimeCaching, StrategyName } from "workbox-build";
import type { WorkboxLifecycleEventMap } from "workbox-window/utils/WorkboxEvent.js";
import { Data, Effect, Layer, Match, Option, Record as Rec, Stream, SubscriptionRef } from "effect";
import * as BrowserStream from "@effect/platform-browser/BrowserStream";
import { Workbox } from "workbox-window";
import type { AvailabilityStore, FaultDetail } from "../interchange/gateway-and-quarantine.ts";
import { CommandGatewayLive, type CommandPayloadWire, type ControlVerb } from "../interchange/gateway-and-quarantine.ts";
import { LocalPersistenceLive } from "./platform-substrate.ts";

// --- [TYPES] ---------------------------------------------------------------------------
type CacheStrategy = "cache-first" | "network-first" | "stale-while-revalidate" | "network-only";

// `SwLifecycle` is a closed tag union; `Reloading` is the user-accepted-update transient the
// `controlling` arm consumes to drive the page reload, so the handshake is one fold, not a flag.
type SwLifecycle = Data.TaggedEnum<{
  readonly Unregistered: object;
  readonly Installing: object;
  readonly Installed: object;
  readonly Active: object;
  readonly UpdateWaiting: object;
  readonly Reloading: object;
  readonly Redundant: object;
}>;
const SwLifecycle = Data.taggedEnum<SwLifecycle>();

// --- [MODELS] --------------------------------------------------------------------------
// One route row owns its strategy literal, cache name, and expiration; `runtimeCaching` projects
// the row to the verified `workbox-build` `RuntimeCaching` entry the precache manifest consumes.
interface StrategyBehavior {
  readonly urlPattern: RegExp;
  readonly strategy: CacheStrategy;
  readonly cacheName: string;
  readonly maxEntries: Option.Option<number>;
  readonly maxAgeSeconds: Option.Option<number>;
}

interface ServiceWorkerHost {
  readonly lifecycle: SubscriptionRef.SubscriptionRef<SwLifecycle>;
  readonly updateAvailable: SubscriptionRef.SubscriptionRef<boolean>;
  readonly applyUpdate: Effect.Effect<void>;
}

// The `CommandGatewayLive`/`LocalPersistenceLive` deps are satisfied by `dependencies:` and must
// not appear in `R`; `AvailabilityStore` rides the `gateway.invoke` `R` channel and is provided at
// the SPA composition root per the `interchange` charter, so it stays surfaced — never erased.
interface BackgroundSyncReplay {
  readonly run: Effect.Effect<void, never, AvailabilityStore>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
// The `CacheStrategy` literal folds to the verified workbox `StrategyName`; the row table and the
// projection are the one route-strategy vocabulary, so the precache manifest reads rows, never a
// per-route `registerRoute` call. `network-only` carries no expiration plugin (nothing is stored).
const strategyName: (strategy: CacheStrategy) => StrategyName = Match.type<CacheStrategy>().pipe(
  Match.when("cache-first", () => "CacheFirst" as const),
  Match.when("network-first", () => "NetworkFirst" as const),
  Match.when("stale-while-revalidate", () => "StaleWhileRevalidate" as const),
  Match.when("network-only", () => "NetworkOnly" as const),
  Match.exhaustive,
);

const runtimeCaching = (row: StrategyBehavior): RuntimeCaching => ({
  urlPattern: row.urlPattern,
  handler: strategyName(row.strategy),
  options: {
    cacheName: row.cacheName,
    ...(Option.isSome(row.maxEntries) || Option.isSome(row.maxAgeSeconds)
      ? {
          expiration: {
            ...Option.match(row.maxEntries, { onNone: () => ({}), onSome: (maxEntries) => ({ maxEntries }) }),
            ...Option.match(row.maxAgeSeconds, { onNone: () => ({}), onSome: (maxAgeSeconds) => ({ maxAgeSeconds }) }),
          },
        }
      : {}),
  },
});

const cacheRoutes: Record<string, StrategyBehavior> = {
  static: {
    urlPattern: /\.(?:js|css|woff2|png|svg)$/,
    strategy: "cache-first",
    cacheName: "rasm-static",
    maxEntries: Option.some(128),
    maxAgeSeconds: Option.some(60 * 60 * 24 * 30),
  },
  api: {
    urlPattern: /\/v1\//,
    strategy: "network-first",
    cacheName: "rasm-api",
    maxEntries: Option.some(64),
    maxAgeSeconds: Option.some(60 * 5),
  },
  geo: {
    urlPattern: /\.(?:glb|pbf|mvt)$/,
    strategy: "stale-while-revalidate",
    cacheName: "rasm-geo",
    maxEntries: Option.some(256),
    maxAgeSeconds: Option.some(60 * 60 * 24 * 7),
  },
};

// `vite-plugin-pwa` reads this projection at build time; the worker asset `BuildPipeline` emits
// carries the resulting `RuntimeCaching` array plus the precache `NavigationRoute` app-shell fallback.
const runtimeCachingManifest: ReadonlyArray<RuntimeCaching> = Rec.values(Rec.map(cacheRoutes, runtimeCaching));

// BOUNDARY ADAPTER: bridges the `Workbox` foreign event target into a back-pressured `Stream`,
// registering each lifecycle listener on acquire and removing every one on scope close. The
// `Stream.asyncScoped` register effect returns the cleanup; the emit pushes one event per fire.
const swLifecycleEvents = (wb: Workbox): Stream.Stream<keyof WorkboxLifecycleEventMap> =>
  Stream.asyncScoped<keyof WorkboxLifecycleEventMap>((emit) =>
    Effect.acquireRelease(
      Effect.sync(() => {
        const tags = ["installing", "installed", "waiting", "activated", "controlling", "redundant"] as const;
        const listeners = tags.map((tag) => {
          const fn = (_event: WorkboxLifecycleEventMap[typeof tag]): void => emit.single(tag);
          wb.addEventListener(tag, fn);
          return [tag, fn] as const;
        });
        return listeners;
      }),
      (listeners) => Effect.sync(() => listeners.forEach(([tag, fn]) => wb.removeEventListener(tag, fn))),
    ),
  );

// --- [SERVICES] ------------------------------------------------------------------------
class ServiceWorkerHostLive extends Effect.Service<ServiceWorkerHostLive>()("@rasm/ts/platform/ServiceWorkerHost", {
  scoped: Effect.gen(function* () {
    const wb = yield* Effect.acquireRelease(
      Effect.sync(() => new Workbox("/sw.js")),
      (instance) => Effect.promise(() => instance.update()).pipe(Effect.ignore),
    );
    const lifecycle = yield* SubscriptionRef.make<SwLifecycle>(SwLifecycle.Unregistered());
    const updateAvailable = yield* SubscriptionRef.make(false);

    // One `Match` fold owns every lifecycle transition; `waiting` flips the update-available cell,
    // `controlling` reloads only after an accepted `applyUpdate` set the `Reloading` transient.
    yield* swLifecycleEvents(wb).pipe(
      Stream.mapEffect((tag) =>
        Match.value(tag).pipe(
          Match.when("installing", () => SubscriptionRef.set(lifecycle, SwLifecycle.Installing())),
          Match.when("installed", () => SubscriptionRef.set(lifecycle, SwLifecycle.Installed())),
          Match.when("activated", () => SubscriptionRef.set(lifecycle, SwLifecycle.Active())),
          Match.when("redundant", () => SubscriptionRef.set(lifecycle, SwLifecycle.Redundant())),
          Match.when("waiting", () =>
            SubscriptionRef.set(lifecycle, SwLifecycle.UpdateWaiting()).pipe(
              Effect.zipRight(SubscriptionRef.set(updateAvailable, true)),
            )),
          Match.when("controlling", () =>
            SubscriptionRef.get(lifecycle).pipe(
              Effect.flatMap(
                SwLifecycle.$match({
                  Reloading: () => Effect.sync(() => window.location.reload()),
                  Unregistered: () => Effect.void,
                  Installing: () => Effect.void,
                  Installed: () => Effect.void,
                  Active: () => Effect.void,
                  UpdateWaiting: () => Effect.void,
                  Redundant: () => Effect.void,
                }),
              ),
            )),
          Match.exhaustive,
        )),
      Stream.runDrain,
      Effect.forkScoped,
    );

    yield* Effect.promise(() => wb.register());

    // An accepted refresh sets the `Reloading` transient then messages the waiting worker; the
    // reload fires on the next `controlling` event the fold observes, never inline here. The
    // `set`-then-`zipRight(messageSkipWaiting)` order is LOAD-BEARING: `Reloading` must commit before
    // the handshake so the `controlling` arm reads `Reloading` and reloads exactly once — reordering
    // the `zipRight` silently breaks the reload-once guarantee on synchronous-handshake browsers.
    const applyUpdate = SubscriptionRef.set(lifecycle, SwLifecycle.Reloading()).pipe(
      Effect.zipRight(Effect.sync(() => wb.messageSkipWaiting())),
    );

    return { lifecycle, updateAvailable, applyUpdate } satisfies ServiceWorkerHost;
  }),
}) {}

class BackgroundSyncReplayLive extends Effect.Service<BackgroundSyncReplayLive>()(
  "@rasm/ts/platform/BackgroundSyncReplay",
  {
    dependencies: [CommandGatewayLive.Default, LocalPersistenceLive.Default],
    scoped: Effect.gen(function* () {
      const gateway = yield* CommandGatewayLive;
      const persistence = yield* LocalPersistenceLive;

      // The native `SyncManager` (where present) is the background wake; the `online` event is the
      // universal fallback. Both feed the one drain — `SyncManager` absence never opens a second queue.
      const registerNativeSync: Effect.Effect<void> = Effect.promise(() => navigator.serviceWorker.ready).pipe(
        Effect.flatMap((reg) =>
          "sync" in reg
            ? Effect.tryPromise(() => (reg as ServiceWorkerRegistration & { sync: SyncManager }).sync.register("rasm-offline-drain")).pipe(Effect.ignore)
            : Effect.void,
        ),
        Effect.ignore,
      );

      // `drainOnRedial` yields `ReadonlyArray<{ verb: ControlVerb; payload: CommandPayloadWire }>` from
      // the `platform-substrate` owner verbatim — the same `{ verb, payload }` pair the `interchange`
      // `IntentRegistry.resolve` already yields, so the replay is a verbatim re-dial of resolved intents,
      // never a verb re-derived off the payload. `gateway.invoke` carries `AvailabilityStore` in `R` and
      // `FaultDetail` in `E`; a faulted dial re-enqueues the element through `Effect.catchAll` (the error
      // channel is exactly `FaultDetail`, every one of its five tags re-enqueued), never a silent drop.
      const run: Effect.Effect<void, never, AvailabilityStore> = registerNativeSync.pipe(
        Effect.zipRight(
          BrowserStream.fromEventListenerWindow("online").pipe(
            Stream.mapEffect(() => persistence.offlineQueue.drainOnRedial),
            Stream.flatMap(Stream.fromIterable),
            Stream.mapEffect((item: { readonly verb: ControlVerb; readonly payload: CommandPayloadWire }) =>
              gateway.invoke(item.verb, item.payload).pipe(
                Effect.asVoid,
                Effect.catchAll((_fault: FaultDetail) => persistence.offlineQueue.enqueue(item)),
              )),
            Stream.runDrain,
          ),
        ),
      );

      return { run } satisfies BackgroundSyncReplay;
    }),
  },
) {}

// GAP (cross-page, owned by `platform-substrate.md`): this page replays against an `offlineQueue`
// element of `{ verb: ControlVerb; payload: CommandPayloadWire }` — the same pair `interchange`
// `IntentRegistry.resolve` yields. The substrate owner currently types the element `CommandPayloadWire`
// (`platform-substrate.md` `LocalPersistence.offlineQueue.enqueue`/`drainOnRedial`), which carries no
// verb; `CommandGateway.invoke` requires `(ControlVerb, CommandPayloadWire)`, so the 4-case payload
// union cannot resolve the verb alone. Resolution lives in `platform-substrate.md`: widen the queue
// element to `{ verb: ControlVerb; payload: CommandPayloadWire }` so the persisted element is the
// resolved-intent pair, collapsing the gap rather than re-deriving a verb here. This page authors no
// stub for it — the body above is body-complete against the widened element shape the substrate owns.

// --- [COMPOSITION] ---------------------------------------------------------------------
const ServiceWorkerLayer = Layer.mergeAll(ServiceWorkerHostLive.Default, BackgroundSyncReplayLive.Default);

// --- [EXPORTS] -------------------------------------------------------------------------
export {
  type CacheStrategy,
  type ServiceWorkerHost,
  type StrategyBehavior,
  type SwLifecycle,
  BackgroundSyncReplayLive,
  ServiceWorkerHostLive,
  ServiceWorkerLayer,
  runtimeCachingManifest,
};
```
