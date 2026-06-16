# [PLATFORM_SERVICE_WORKER]

One page owns the browser service-worker / PWA + offline-first cache concern — `ServiceWorkerHost`, the Effect-native registration and update-lifecycle owner over `workbox-window`; `CacheStrategy`, the one `Schema.Literal` route-strategy axis (`cache-first`/`network-first`/`stale-while-revalidate`) the Workbox precache and runtime-cache routes resolve against; and `BackgroundSyncReplay`, the redial-driven drain of the `platform` `platform-substrate` `LocalPersistence.offlineQueue` into the `interchange` `CommandGateway`. `BuildPipeline` emits the worker asset at build time; `ServiceWorkerHost` owns its runtime lifecycle — the two are distinct altitudes over one concern, never a two-owner split. The page composes `vite-plugin-pwa`, `workbox-build`, and `workbox-window`, holds no domain state, and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                |
| :-----: | :------------- | :-------------------------------------------------------------------- |
|   [1]   | SERVICE_WORKER | the registration lifecycle, the cache-strategy axis, and sync replay  |

## [2]-[SERVICE_WORKER]

- Owner: `ServiceWorkerHost`, the single service-worker runtime owner — registration through `workbox-window` `Workbox`, the install/activate/`skipWaiting` update lifecycle folded over a `SubscriptionRef`, the update-available cell the `ui` surfaces a refresh affordance from, and the offline-first navigation fallback; `CacheStrategy`, the `Schema.Literal` route-strategy axis with a behavior `Record` mapping each route pattern to its Workbox strategy; and `BackgroundSyncReplay`, the redial drain reading `LocalPersistence.offlineQueue` into the `interchange` `CommandGateway`. The cache-strategy literal is the one route-strategy vocabulary and a hand-written `caches.open(...)` call outside it is the named defect.
- Cases: `CacheStrategy` is one `Schema.Literal` union (`cache-first`/`network-first`/`stale-while-revalidate`/`network-only`) with a `StrategyBehavior` `Record` carrying each route glob and its Workbox `RuntimeCaching` strategy, so the app-shell precache, the static-asset `cache-first` route, the API `network-first` route, and the tile/glb `stale-while-revalidate` route are each one row, never a per-route imperative handler; `ServiceWorkerHost` registers the build-emitted worker through `workbox-window` `Workbox` and folds the `waiting`/`controlling`/`activated` events into one `SwLifecycle` `SubscriptionRef`, so the `waiting` event flips the update-available cell and the `ui` refresh affordance reads it through the one `AtomBinding`; an accepted refresh sends `messageSkipWaiting` and reloads on the next `controlling` event, so the update handshake is one fold over the Workbox event stream rather than scattered `postMessage` calls; the offline-first navigation fallback serves the precached app-shell when the network read fails, so a reload while offline restores the SPA rather than a browser error page.
- Auto: `BackgroundSyncReplay` is the redial-triggered drain — it subscribes to the `online` window event through `BrowserStream.fromEventListenerWindow("online")`, and on each redial reads `LocalPersistence.offlineQueue.drainOnRedial` and replays each queued `(verb, payload)` command through the `interchange` `CommandGateway.invoke`, threading the `projection` `AvailabilityStore` the gateway gates against, so the same offline-queue seam `platform-substrate` names is drained by one owner on reconnect rather than a parallel SW-resident queue; a replay whose gateway invoke faults re-enqueues the command and folds to the `interchange` `FaultDetail` family, never a silent drop. The background-sync registration uses the native `SyncManager` where present and the `online`-event drain as the universal fallback, both feeding the one drain.
- Packages: `workbox-window` for the `Workbox` registration and the lifecycle event stream; `workbox-build` and `vite-plugin-pwa` for the build-time precache manifest and the worker asset `BuildPipeline` emits; `@effect/platform-browser` `BrowserStream.fromEventListenerWindow` for the `online`/`controllerchange` ingress; `effect` for the `Schema.Literal` strategy axis, the `SubscriptionRef` lifecycle cell, and the drain fold.
- Growth: a new cache route lands as one literal on `CacheStrategy` and one `StrategyBehavior` `Record` row; a new lifecycle signal lands as one arm on the `SwLifecycle` fold; a new offline-replay concern lands as one row on `BackgroundSyncReplay`, never a parallel queue or a second worker.
- Boundary: `ServiceWorkerHost` owns the SW RUNTIME lifecycle (install/activate/cache strategy/background-sync) and `BuildPipeline` (platform-substrate) EMITS the worker ASSET at build time — distinct altitudes, one concern each, so a strategy or lifecycle row authored on `BuildPipeline` is the named two-owner-one-concern defect; `BackgroundSyncReplay` drains the single `LocalPersistence.offlineQueue` the `platform-substrate` page owns and replays through the `interchange` `CommandGateway` across the folder seam, never re-dialing a transport here and never holding a parallel offline queue; a direct `caches`/`navigator.serviceWorker` call outside `ServiceWorkerHost` is the named defect; `ui` reads the update-available cell through the `AtomBinding` and never imports `platform`.

```ts contract
type CacheStrategy = "cache-first" | "network-first" | "stale-while-revalidate" | "network-only";

interface StrategyBehavior {
  readonly urlPattern: RegExp;
  readonly strategy: CacheStrategy;
  readonly cacheName: string;
  readonly maxEntries: Option.Option<number>;
}

type SwLifecycle =
  | { readonly _tag: "Unregistered" }
  | { readonly _tag: "Installing" }
  | { readonly _tag: "Active" }
  | { readonly _tag: "UpdateWaiting" }
  | { readonly _tag: "Reloading" };

type QueuedCommand = { readonly verb: ControlVerb; readonly payload: CommandPayloadWire };

interface BackgroundSyncReplay {
  readonly run: Effect.Effect<void, never, CommandGateway | LocalPersistence | AvailabilityStore | Scope.Scope>;
}

interface ServiceWorkerHost {
  readonly lifecycle: SubscriptionRef.SubscriptionRef<SwLifecycle>;
  readonly updateAvailable: SubscriptionRef.SubscriptionRef<boolean>;
  readonly applyUpdate: Effect.Effect<void>;
}

const makeServiceWorkerHost: Effect.Effect<ServiceWorkerHost, never, Scope.Scope> = Effect.gen(function* () {
  const wb = yield* Effect.acquireRelease(
    Effect.sync(() => new Workbox("/sw.js")),
    () => Effect.void,
  );
  const lifecycle = yield* SubscriptionRef.make<SwLifecycle>({ _tag: "Unregistered" });
  const updateAvailable = yield* SubscriptionRef.make(false);
  yield* swEvents(wb).pipe(
    Stream.mapEffect((ev) =>
      Match.value(ev).pipe(
        Match.when({ type: "installing" }, () => SubscriptionRef.set(lifecycle, { _tag: "Installing" } as SwLifecycle)),
        Match.when({ type: "activated" }, () => SubscriptionRef.set(lifecycle, { _tag: "Active" } as SwLifecycle)),
        Match.when({ type: "waiting" }, () =>
          SubscriptionRef.set(lifecycle, { _tag: "UpdateWaiting" } as SwLifecycle).pipe(
            Effect.zipRight(SubscriptionRef.set(updateAvailable, true)),
          )),
        Match.when({ type: "controlling" }, () =>
          SubscriptionRef.get(lifecycle).pipe(
            Effect.flatMap((s) => (s._tag === "Reloading" ? Effect.sync(() => window.location.reload()) : Effect.void)),
          )),
        Match.orElse(() => Effect.void),
      )),
    Stream.runDrain,
    Effect.forkScoped,
  );
  yield* Effect.promise(() => wb.register());
  const applyUpdate = SubscriptionRef.set(lifecycle, { _tag: "Reloading" }).pipe(
    Effect.zipRight(Effect.sync(() => wb.messageSkipWaiting())),
  );
  return { lifecycle, updateAvailable, applyUpdate } satisfies ServiceWorkerHost;
});

const makeBackgroundSyncReplay: BackgroundSyncReplay = {
  run: Effect.gen(function* () {
    const gateway = yield* CommandGateway;
    const persistence = yield* LocalPersistence;
    yield* BrowserStream.fromEventListenerWindow("online").pipe(
      Stream.mapEffect(() => persistence.offlineQueue.drainOnRedial),
      Stream.flatMap(Stream.fromIterable),
      Stream.mapEffect((command: QueuedCommand) =>
        gateway.invoke(command.verb, command.payload).pipe(
          Effect.asVoid,
          Effect.catchAll(() => persistence.offlineQueue.enqueue(command)),
        )),
      Stream.runDrain,
    );
  }),
};
```
