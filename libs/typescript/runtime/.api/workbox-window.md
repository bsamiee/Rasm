# [TS_RUNTIME_API_WORKBOX_WINDOW]

`Workbox` owns the WINDOW-side service-worker lifecycle: registration, the update handshake, and the event target whose lifecycle tags surface every phase transition. `messageSW` carries the sole window↔worker channel.

`Workbox` registers exactly the build-emitted worker asset (the `workbox-build` altitude), so a cache-strategy row authored here is the two-owner defect.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `workbox-window`
- package: `workbox-window` (MIT)
- module: types `index.d.ts`; ESM `build/workbox-window.prod.es5.mjs`, UMD `build/workbox-window.prod.umd.js`; barrel re-exports `Workbox`, `messageSW`, and the `utils/WorkboxEvent` type family
- runtime: window-only — touches `navigator.serviceWorker` and `window`, absent in a Worker or Node bundle; peer `trusted-types` types the script URL
- rail: `browser/shell`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the registration owner and the lifecycle/message event algebra its target dispatches

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :----------------------------- | :------------ | :--------------------------------------- |
|  [01]   | `Workbox`                      | class         | registration and update owner            |
|  [02]   | `WorkboxEvent<K>`              | class         | Event-shim base for every dispatched tag |
|  [03]   | `WorkboxMessageEvent`          | interface     | `message` payload: `data`, `ports`       |
|  [04]   | `WorkboxLifecycleEvent`        | interface     | lifecycle event carrying `isUpdate`      |
|  [05]   | `WorkboxLifecycleWaitingEvent` | interface     | waiting event adding refinement flag     |
|  [06]   | `WorkboxLifecycleEventMap`     | interface     | lifecycle tag → event-type map           |
|  [07]   | `WorkboxEventMap`              | interface     | lifecycle map with `message`             |

- `WorkboxLifecycleEventMap` keys the addEventListener discriminant: `installing`/`installed`/`activating`/`activated`/`controlling`/`redundant` → `WorkboxLifecycleEvent`, `waiting` → `WorkboxLifecycleWaitingEvent`; `WorkboxEventMap` adds `message` → `WorkboxMessageEvent`.
- `WorkboxEvent<K>`: carries `sw`, `originalEvent`, `isExternal`, and `target`; `WorkboxLifecycleWaitingEvent.wasWaitingBeforeRegister` marks a worker already waiting at `register()`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the `Workbox` construction, update, messaging, and event-target surface with the standalone message function

| [INDEX] | [SURFACE]                                                                          | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :--------------------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `new Workbox(string\|TrustedScriptURL, {}?)`                                       | ctor     | bind an instance to a SW script   |
|  [02]   | `Workbox.register({immediate?}?) -> Promise<ServiceWorkerRegistration\|undefined>` | instance | register the bound worker         |
|  [03]   | `Workbox.update() -> Promise<void>`                                                | instance | poll for a fresh worker           |
|  [04]   | `Workbox.active -> Promise<ServiceWorker>`                                         | property | resolve when the SW activates     |
|  [05]   | `Workbox.controlling -> Promise<ServiceWorker>`                                    | property | resolve when the SW controls      |
|  [06]   | `Workbox.getSW() -> Promise<ServiceWorker>`                                        | instance | resolve the matching SW handle    |
|  [07]   | `Workbox.messageSW(object) -> Promise<any>`                                        | instance | request/response into the SW      |
|  [08]   | `Workbox.messageSkipWaiting() -> void`                                             | instance | post `SKIP_WAITING` to the waiter |
|  [09]   | `Workbox.addEventListener(K, (WorkboxEventMap[K]) => any) -> void`                 | instance | subscribe one tag                 |
|  [10]   | `Workbox.removeEventListener(K, (WorkboxEventMap[K]) => any) -> void`              | instance | release one listener              |
|  [11]   | `Workbox.dispatchEvent(WorkboxEvent<any>) -> void`                                 | instance | dispatch to listeners             |
|  [12]   | `messageSW(ServiceWorker, {}) -> Promise<any>`                                     | static   | request/response into any SW      |

- `Workbox.register`: defers registration until window `load` unless `immediate: true`.
- `Workbox.messageSW` / `messageSW`: resolves only when the worker replies via `event.ports[0].postMessage`; a handler setting no response leaves the promise pending.
- `Workbox.messageSkipWaiting`: no-op when no worker sits in `waiting`.
- `Workbox.active` / `Workbox.controlling`: resolve to the already-controlling worker when the script URL matches, else await the next update's activate or control.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A registered worker installs then either activates or stalls in `waiting`; `Workbox` fires `installing`→`installed`→(`waiting` | `activating`→`activated`), `controlling` on the container's `controllerchange`, and `redundant` on replacement.
- `waiting` refines a first-install stall from a genuine update: `isUpdate` marks a worker already controlling at `register()` and `wasWaitingBeforeRegister` one already waiting — either flags an actionable refresh, a bare stall renders none.
- `messageSkipWaiting` closes an ordered handshake: a consumer commits its reload intent first, so the ensuing `controlling` fires exactly one reload and every other `controllerchange` passes through inert.

[STACKING]:
- `effect`(`.api/effect.md`): hold `Workbox` as `Effect.acquireRelease` released by `update()`; bridge one `addEventListener` per lifecycle tag through `Stream.asyncScoped` and fold each `WorkboxEventMap[K]` via `Match.exhaustive` into a `SwLifecycle` `SubscriptionRef`; `register`/`messageSW` ride `Effect.tryPromise`, `messageSkipWaiting` an `Effect.sync` sequenced after the cell sets `Reloading`.
- `workbox-build`(`.api/workbox-build.md`): the asset `injectManifest(InjectManifestOptions)` emits is exactly the script `new Workbox(script)` registers; a type-only `RuntimeCaching.options.backgroundSync.name` names the SW-side replay queue the window observes by decoding the `message` event into a `Replayed` report — this package holds no queue.
- `browser/shell.md`: `Sw` `Effect.Service` acquires one `Workbox` through `Sw.Default(script)`, `_lifecycle(wb)` bridges its event target, `apply` sets `Reloading` then calls `wb.messageSkipWaiting()`, `signal` wraps `wb.messageSW`, `reports` decodes the `message` feed, and `register` maps `wb.register()` to a boolean phase fact.

[LOCAL_ADMISSION]:
- Folder-local `# browser` catalog group; one `Workbox` per app held as a scoped resource with register/update/message/skip collapsed behind the `Sw` service, its event target the single phase writer.

[RAIL_LAW]:
- Package: `workbox-window`
- Owns: the window-side service-worker lifecycle — registration, the update handshake, window↔worker messaging, and lifecycle-event observation.
- Accept: one `Workbox` scoped resource; the lifecycle event stream folded into a `SwLifecycle` cell; `messageSW`/`messageSkipWaiting` as the only window→worker channel.
- Reject: a raw `navigator.serviceWorker`/`caches` call outside the owner; authoring cache strategy here (build-time `RuntimeCaching` owns it); a second `Workbox` boot.
