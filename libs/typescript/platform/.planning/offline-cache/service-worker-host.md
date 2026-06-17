# [PLATFORM_SERVICE_WORKER_HOST]

One page owns the service-worker runtime lifecycle and the offline-first cache strategy — `ServiceWorkerHost`, the Effect-native registration and update-lifecycle owner over `workbox-window`, and `CacheStrategy`, the one `Schema.Literal` route-strategy axis (`cache-first`/`network-first`/`stale-while-revalidate`/`network-only`) the Workbox precache and runtime-cache routes resolve against, with navigation-preload arming the network-first HTML route. `build-pipeline`'s `BuildPipeline` emits the worker asset at build time; `ServiceWorkerHost` owns its runtime lifecycle — distinct altitudes over one concern, never a two-owner split. The page holds no domain state and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]          | [OWNS]                                                         |
| :-----: | :----------------- | :----------------------------------------------------------- |
|   [1]   | SERVICE_WORKER_HOST | the registration lifecycle and the cache-strategy axis         |

## [2]-[SERVICE_WORKER_HOST]

- Owner: `ServiceWorkerHost`, the single service-worker runtime owner — registration through `workbox-window` `Workbox` held as an `Effect.acquireRelease` resource, the `installing`/`installed`/`waiting`/`activated`/`controlling`/`redundant` lifecycle stream bridged off the `Workbox` event target through `Stream.asyncScoped` and folded into one `SwLifecycle` `SubscriptionRef`, the `messageSkipWaiting` update handshake reloading on the next `controlling` event, the update-available cell the `ui` surfaces a refresh affordance from, the navigation-preload arming for the network-first HTML route, and the offline-first navigation fallback; and `CacheStrategy`, the `Schema.Literal` route-strategy axis with a `StrategyBehavior` `Record` projecting each route row to its verified Workbox `RuntimeCaching` entry. The cache-strategy literal is the one route-strategy vocabulary and a hand-written `caches.open(...)` call outside it is the named defect.
- Cases: `CacheStrategy` is one `Schema.Literal` union (`cache-first`/`network-first`/`stale-while-revalidate`/`network-only`) folded by `strategyName` to the verified workbox `StrategyName` (`CacheFirst`/`NetworkFirst`/`StaleWhileRevalidate`/`NetworkOnly`), with a `StrategyBehavior` row carrying each route `urlPattern`, cache name, and `maxEntries`/`maxAgeSeconds` expiration and `runtimeCaching` projecting the row to a `workbox-build` `RuntimeCaching` entry, so the static-asset `cache-first` route, the API `network-first` route, and the tile/glb `stale-while-revalidate` route are each one row, never a per-route imperative handler; `ServiceWorkerHost` registers the build-emitted worker through `workbox-window` `Workbox` and folds the lifecycle events into one `SwLifecycle` `SubscriptionRef`, so the `waiting` event flips the update-available cell and the `ui` refresh affordance reads it through the one `AtomBinding`; an accepted refresh sets the `Reloading` transient then messages the waiting worker — the `set`-then-`zipRight(messageSkipWaiting)` order is load-bearing so the `controlling` arm reads `Reloading` and reloads exactly once — so the update handshake is one `Match.value` fold over the Workbox event stream rather than scattered `postMessage` calls; the network-first HTML route arms navigation-preload so the first navigation request races the cache lookup against the network preload rather than blocking on worker startup; the offline-first navigation fallback is the precache-route `NavigationRoute` the worker asset carries, so a reload while offline serves the precached app-shell rather than a browser error page.
- Packages: `workbox-window` for the `Workbox` registration and the lifecycle event target bridged through `Stream.asyncScoped`; `workbox-build` `RuntimeCaching`/`StrategyName` for the `StrategyBehavior` projection; `vite-plugin-pwa` for the build-time precache manifest `BuildPipeline` consumes; `effect` `Schema.Literal` for the strategy axis, `SubscriptionRef` for the lifecycle and update-available cells, `Data.taggedEnum` for `SwLifecycle`, `Match` for the lifecycle fold, and `Stream`/`Effect.acquireRelease`/`Effect.forkScoped` for the registration resource.
- Growth: a new cache route lands as one literal on `CacheStrategy` and one `StrategyBehavior` `Record` row; a new lifecycle signal lands as one arm on the `SwLifecycle` `Match` fold, never a parallel queue or a second worker.
- Boundary: `ServiceWorkerHost` owns the SW RUNTIME lifecycle and `BuildPipeline` (`build-pipeline`) EMITS the worker ASSET at build time — distinct altitudes, one concern each, so a strategy or lifecycle row authored on `BuildPipeline` is the named two-owner-one-concern defect; a direct `caches`/`navigator.serviceWorker` call outside `ServiceWorkerHost` is the named defect; `ui` reads the update-available cell through the `AtomBinding` and never imports `platform`.

```ts contract
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { RuntimeCaching, StrategyName } from "workbox-build";
import type { WorkboxLifecycleEventMap } from "workbox-window/utils/WorkboxEvent.js";
import { Data, Effect, Layer, Match, Option, Record as Rec, Stream, SubscriptionRef } from "effect";
import * as BrowserStream from "@effect/platform-browser/BrowserStream";
import { Workbox } from "workbox-window";

// --- [TYPES] ---------------------------------------------------------------------------
type CacheStrategy = "cache-first" | "network-first" | "stale-while-revalidate" | "network-only";

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
  readonly register: Effect.Effect<void>;
  readonly applyUpdate: Effect.Effect<void>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
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

const runtimeCachingManifest: ReadonlyArray<RuntimeCaching> = Rec.values(Rec.map(cacheRoutes, runtimeCaching));

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

    const register = Effect.promise(() => wb.register()).pipe(Effect.asVoid);

    const applyUpdate = SubscriptionRef.set(lifecycle, SwLifecycle.Reloading()).pipe(
      Effect.zipRight(Effect.sync(() => wb.messageSkipWaiting())),
    );

    return { lifecycle, updateAvailable, register, applyUpdate } satisfies ServiceWorkerHost;
  }),
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------
export {
  type CacheStrategy,
  type ServiceWorkerHost,
  type StrategyBehavior,
  type SwLifecycle,
  ServiceWorkerHostLive,
  runtimeCachingManifest,
};
```
