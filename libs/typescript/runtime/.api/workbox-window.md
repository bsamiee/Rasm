# [TS_RUNTIME_API_WORKBOX_WINDOW]

`workbox-window` runs in the WINDOW, never the service worker. `browser/shell.md` holds one `Workbox` instance as an `Effect.acquireRelease` resource, bridges its lifecycle event target through `Stream.asyncScoped` into a single `SwLifecycle` `SubscriptionRef`, and drives the `messageSkipWaiting` update handshake that reloads on the next `controlling` event. The service-worker ASSET it registers is emitted at build time by `workbox-build` — distinct altitude, one concern each, so a cache-strategy row authored here is the named two-owner defect. The runtime SW-side background-sync `Queue` owns replay; this package OBSERVES and kicks it through window→worker messaging.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `workbox-window`
- package: `workbox-window` (MIT)
- module: ESM `index.mjs` + UMD `build/workbox-window.prod.umd.js`; types `index.d.ts`; window-only — touches `navigator.serviceWorker` and `window`, so it belongs to the browser subpath and never a node/wasm bundle
- marker: build-floor baseline browsers with the Service Worker API; `TrustedScriptURL` supported for the script URL
- exports: `Workbox`, `messageSW`, and the `utils/WorkboxEvent` event-map types (`WorkboxLifecycleEventMap`, `WorkboxEventMap`, `WorkboxLifecycleEvent`, `WorkboxLifecycleWaitingEvent`, `WorkboxMessageEvent`)
- bound asset: TSDECL `node_modules/workbox-window/{index,Workbox,messageSW,utils/WorkboxEvent}.d.ts`
- admission: folder-local `# browser` catalog group; version centralized in `pnpm-workspace.yaml`
- role: `browser/shell.md` (SW runtime lifecycle + update handshake + sync-replay observation); `browser/shell.md` reads the update-available cell for a refresh affordance
- rail: `browser/shell`

## [02]-[WORKBOX_LIFECYCLE_OWNER]

`Workbox` is the registration and update owner — a `WorkboxEventTarget` subclass whose async accessors and message methods are the whole runtime handle.

[WORKBOX]: `Workbox(string|TrustedScriptURL,{}?)` `Workbox.register({immediate?:boolean}?) -> Promise<ServiceWorkerRegistration|undefined>` `Workbox.update() -> Promise<void>` `Workbox.active: Promise<ServiceWorker>` `Workbox.controlling: Promise<ServiceWorker>` `Workbox.getSW() -> Promise<ServiceWorker>` `Workbox.messageSW(object) -> Promise<any>` `Workbox.messageSkipWaiting() -> void`

Consumer note: hold as `Effect.acquireRelease(Effect.sync(() => new Workbox("/sw.js")), (wb) => Effect.promise(() => wb.update()).pipe(Effect.ignore))`; wrap `register()`/`messageSW` in `Effect.promise`, `messageSkipWaiting()` in `Effect.sync`. A raw `navigator.serviceWorker.register` outside this owner is the named defect.

## [03]-[LIFECYCLE_EVENTS]

The event target is the state source: seven lifecycle tags plus `message`, each carrying the SW and the original DOM event. `installing`/`activating` fire on transitions the older Workbox releases omitted, so a fold over the full map stays total.

[WORKBOX_EVENT_TARGET]: `WorkboxEventTarget.addEventListener(K,(e:WorkboxEventMap[K])=>any) -> void` `WorkboxEventTarget.removeEventListener(K,(e:WorkboxEventMap[K])=>any) -> void` `WorkboxEventTarget.dispatchEvent(WorkboxEvent<any>) -> void`
[WORKBOX_LIFECYCLE_EVENT_MAP]: `WorkboxLifecycleEventMap.installing: unknown` `WorkboxLifecycleEventMap.installed: unknown` `WorkboxLifecycleEventMap.waiting: unknown` `WorkboxLifecycleEventMap.activating: unknown` `WorkboxLifecycleEventMap.activated: unknown` `WorkboxLifecycleEventMap.controlling: unknown` `WorkboxLifecycleEventMap.redundant: unknown`
[WORKBOX_LIFECYCLE_WAITING_EVENT]: `WorkboxLifecycleWaitingEvent.wasWaitingBeforeRegister: boolean`
[WORKBOX_MESSAGE_EVENT]: `WorkboxMessageEvent.data: any` `WorkboxMessageEvent.originalEvent: Event` `WorkboxMessageEvent.ports: readonly MessagePort[]`
[WORKBOX_EVENT_MAP]: `WorkboxEventMap.message: WorkboxMessageEvent`

Consumer note: `Stream.asyncScoped` acquires one `addEventListener` per tag and releases every listener on scope close; each emission folds through `Match.value`/`Match.exhaustive` into a `SwLifecycle` `SubscriptionRef`. `waiting` flips an `updateAvailable` `SubscriptionRef` to `true`; the `controlling` arm reads the prior phase and reloads exactly once when it is `Reloading` (the `set(Reloading)`-then-`messageSkipWaiting` order is load-bearing).

## [04]-[MESSAGING]

The window↔worker channel is `messageSW` (request/response over a `MessagePort`) and the standalone `messageSW(sw, data)` for an arbitrary SW handle.

[SURFACES]: `messageSW(ServiceWorker,{}) -> Promise<any>`

Consumer note: background-sync replay lives in the SERVICE WORKER — the `Queue` from `workbox-background-sync`, configured at build time via `workbox-build` `RuntimeCaching.options.backgroundSync: { name, options: QueueOptions }` — and drains automatically on reconnect. The window observes drain completion and triggers an on-demand replay through `Workbox.messageSW`/the `message` event; `workbox-window` carries no queue of its own, so the "background-sync replay rows" are this messaging seam, not a local queue API.

## [05]-[STACKING]

- `effect`: `Effect.acquireRelease` (the `Workbox` resource), `Stream.asyncScoped` (event bridge), `SubscriptionRef` (lifecycle + update-available cells), `Data.taggedEnum` (`SwLifecycle`), `Match.value`/`Match.exhaustive` (the fold), `Effect.forkScoped` (drain), `Effect.Service`/`Layer` (the `ServiceWorkerHost` owner Layer the app root selects).
- `@effect/platform-browser`: `BrowserStream` is the alternate typed event-source adapter when a `Stream` from a DOM `EventTarget` is preferred over the hand-bridged `asyncScoped`.
- sibling `workbox-build`: shares the `RuntimeCaching`/`StrategyName` vocabulary across the build/runtime seam — the asset generated at build is exactly what `Workbox` registers; a type-only import keeps the strategy rows one source of truth.
- `ui` port: the `updateAvailable` `SubscriptionRef` surfaces to `ui` through a declared runtime-capability port bound to an `@effect-atom` binding — `browser` provides the Layer at app composition, `ui` never imports `browser`.
- `core/value/identity`: the SW script URL and precache cache-version derive from the `AppIdentity` build fingerprint, so a build bump forces a fresh install rather than a silent stale worker.

## [06]-[RAIL_LAW]

- Owns: the WINDOW-side service-worker lifecycle — registration, the update handshake, and background-sync replay observation.
- Accept: one `Workbox` held as a scoped resource; the lifecycle event stream folded into a `SwLifecycle` cell; `messageSW`/`messageSkipWaiting` as the only window→worker channel.
- Reject: a raw `navigator.serviceWorker`/`caches` call outside the owner (named defect); authoring cache STRATEGY here (build-time `RuntimeCaching` rows own it); a second `Workbox` boot (the single-boot law).
