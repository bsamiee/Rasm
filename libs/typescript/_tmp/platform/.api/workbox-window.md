# [API_CATALOGUE] workbox-window

`workbox-window` is the window-side companion to a Workbox service worker: the `Workbox` class owns registration, lifecycle-event observation, skip-waiting coordination, and bidirectional `postMessage` exchange, and the standalone `messageSW` posts to an arbitrary `ServiceWorker`. It is the browser-runtime half of the PWA seam whose build-time half (`workbox-build` via `vite-plugin-pwa`) emits the SW script; `platform/Shell/serviceworker` wraps `Workbox` as one Effect scoped resource, folds its eight-key lifecycle map into a `SubscriptionRef` update cell, and drives the redial drain (`platform/Shell/sync`) through `messageSW`. The lifecycle event map is the whole reason no hand-rolled `navigator.serviceWorker` tracking exists.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `workbox-window`
- package: `workbox-window` (7.4.1, MIT, © Google)
- module format: dual UMD + ESM bundles (`main` = `build/workbox-window.prod.umd.js`, `module` = `build/workbox-window.prod.es5.mjs`, plus `.prod.mjs`, ES5 mirrors, `.dev.*` unminified twins); `types` = `index.d.ts`; no `exports` map, so Node legacy resolution allows deep paths — the per-module `.js` (CJS) + `.mjs` (ESM) files under `utils/` and the root `Workbox`/`messageSW` modules ARE real resolvable runtime, but everything under `utils/` is `@private` and only `Workbox`/`messageSW`/the `WorkboxEvent` family are barrelled through `index.js`, so the root barrel is the one supported import site
- runtime target: browser/window only — reads `navigator.serviceWorker`, `window.load`, `ServiceWorkerRegistration`; imports no `node:*`, so it is inert (and must not be imported) inside the service worker or a node build step
- deps: `workbox-core@7.4.1` (pinned exact — the Workbox family version-locks), `@types/trusted-types` (the `TrustedScriptURL` script-URL type); no peer dependencies
- asset: window-side SW registration + lifecycle client
- rail: service-worker-client (the browser-runtime half of the PWA seam; `workbox-build` owns the SW-side behavior)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: root barrel exports — the class, the free function, and the event family
- rail: service-worker-client
- barrel: `index.d.ts` re-exports `Workbox`, `messageSW`, and `export * from './utils/WorkboxEvent.js'` (the six event types); `WorkboxEventTarget` and `ListenerCallback` are NOT re-exported at the root — the base class is reached through a `Workbox` instance, not imported

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [CAPABILITY]                                                          |
| :-----: | :----------------------------- | :----------------- | :------------------------------------------------------------------- |
|  [01]   | `Workbox`                      | class              | registration, lifecycle, skip-waiting, and messaging owner           |
|  [02]   | `WorkboxEvent<K>`              | class (shim)       | `Event`-like carrier: `type: K`, `target?`, `sw?`, `originalEvent?`, `isExternal?` |
|  [03]   | `WorkboxMessageEvent`          | interface          | `extends WorkboxEvent<'message'>`; adds `data: any`, `originalEvent: Event`, `ports: readonly MessagePort[]` |
|  [04]   | `WorkboxLifecycleEvent`        | interface          | `extends WorkboxEvent<keyof LifecycleEventMap>`; adds `isUpdate?: boolean` |
|  [05]   | `WorkboxLifecycleWaitingEvent` | interface          | `extends WorkboxLifecycleEvent`; adds `wasWaitingBeforeRegister?: boolean` |
|  [06]   | `WorkboxLifecycleEventMap`     | interface          | seven lifecycle name→event-type entries (`installing`…`redundant`)   |
|  [07]   | `WorkboxEventMap`              | interface          | `extends WorkboxLifecycleEventMap`; adds `message: WorkboxMessageEvent` (eight total) |

[PUBLIC_TYPE_SCOPE]: inherited base — not root-barrelled, reached through `Workbox`
- rail: service-worker-client

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :------------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `WorkboxEventTarget` | class (shim)  | base of `Workbox`; a minimal `EventTarget` (browsers without constructable `EventTarget`); supplies `addEventListener`/`removeEventListener`/`dispatchEvent` — every `Workbox` inherits these, but the class itself is not exported from the package root |
|  [02]   | `ListenerCallback`   | type alias    | `(event: WorkboxEvent<any>) => any`; declared beside `WorkboxEventTarget`, not re-exported at the root |

[PUBLIC_TYPE_SCOPE]: lifecycle event names (`WorkboxEventMap` keys) — the fold vocabulary
- rail: service-worker-client

| [INDEX] | [EVENT]       | [EVENT_TYPE]                   | [FIRES_WHEN]                                     |
| :-----: | :------------ | :----------------------------- | :----------------------------------------------- |
|  [01]   | `installing`  | `WorkboxLifecycleEvent`        | registered SW entered `installing`               |
|  [02]   | `installed`   | `WorkboxLifecycleEvent`        | registered SW entered `installed` (`isUpdate` set on an update) |
|  [03]   | `waiting`     | `WorkboxLifecycleWaitingEvent` | installed but not activating — the update-prompt trigger |
|  [04]   | `activating`  | `WorkboxLifecycleEvent`        | registered SW entered `activating`               |
|  [05]   | `activated`   | `WorkboxLifecycleEvent`        | registered SW entered `activated`                |
|  [06]   | `controlling` | `WorkboxLifecycleEvent`        | new controller `scriptURL` matches this instance |
|  [07]   | `redundant`   | `WorkboxLifecycleEvent`        | registered SW entered `redundant`                |
|  [08]   | `message`     | `WorkboxMessageEvent`          | a `postMessage` arrived (carries `data` + `ports`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Workbox` instance and standalone messaging
- rail: service-worker-client

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]  | [CAPABILITY]                                                    |
| :-----: | :------------------------------ | :-------------- | :------------------------------------------------------------- |
|  [01]   | `new Workbox(scriptURL, opts?)` | constructor     | `scriptURL: string \| TrustedScriptURL`, `registerOptions?: {}` (the `navigator.serviceWorker.register` options) |
|  [02]   | `register(options?)`            | registration    | `{ immediate?: boolean }` → `Promise<ServiceWorkerRegistration \| undefined>`; delays until `window.load` unless `immediate` |
|  [03]   | `update()`                      | update probe    | `Promise<void>` — checks for an updated registered worker      |
|  [04]   | `get active`                    | getter promise  | `Promise<ServiceWorker>` — resolves once the registered SW is `activated` |
|  [05]   | `get controlling`              | getter promise  | `Promise<ServiceWorker>` — resolves once it controls the page (needs `clients.claim()` on first install) |
|  [06]   | `getSW()`                       | sw accessor     | `Promise<ServiceWorker>` — resolves as soon as a matching SW exists (waiting preferred over active) |
|  [07]   | `messageSW(data)`               | message send    | `Promise<any>` — posts `data` to `getSW()`, resolves with the reply on `event.ports[0]` (never resolves with no reply) |
|  [08]   | `messageSkipWaiting()`          | skip-waiting    | posts `{ type: 'SKIP_WAITING' }` to the `waiting` worker; inert when none waits |
|  [09]   | `addEventListener(type, fn)`    | listener add    | inherited; typed by `WorkboxEventMap[K]` (the fold source)     |
|  [10]   | `removeEventListener(type, fn)` | listener remove | inherited; the release half of the lifecycle stream            |
|  [11]   | `messageSW(sw, data)`           | standalone fn   | `Promise<any>` — posts to an arbitrary `ServiceWorker`, resolves with its reply |

```ts contract
// workbox-window (root barrel)
declare class Workbox extends WorkboxEventTarget {
  constructor(scriptURL: string | TrustedScriptURL, registerOptions?: {})
  register(options?: { immediate?: boolean }): Promise<ServiceWorkerRegistration | undefined>
  update(): Promise<void>
  get active(): Promise<ServiceWorker>
  get controlling(): Promise<ServiceWorker>
  getSW(): Promise<ServiceWorker>
  messageSW(data: object): Promise<any>
  messageSkipWaiting(): void
}
declare function messageSW(sw: ServiceWorker, data: {}): Promise<any>

// inherited base (not root-barrelled)
declare class WorkboxEventTarget {
  addEventListener<K extends keyof WorkboxEventMap>(type: K, listener: (event: WorkboxEventMap[K]) => any): void
  removeEventListener<K extends keyof WorkboxEventMap>(type: K, listener: (event: WorkboxEventMap[K]) => any): void
  dispatchEvent(event: WorkboxEvent<any>): void
}
```

## [04]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- `register` delays registration until after `window.load` by default; `immediate: true` bypasses the load gate and is not recommended (it competes with page-load bandwidth).
- `active` resolves at `activated`; `controlling` resolves once the SW controls the page — first-install activation does not control until the worker calls `clients.claim()`.
- `getSW` prefers a waiting worker over an active one (the waiting worker registered more recently); `messageSW(data)` posts to it and resolves with the reply on `event.ports[0]`, so a worker with no reply handler leaves the promise pending forever — never `await` it without a worker-side responder.
- `messageSkipWaiting` posts `{ type: 'SKIP_WAITING' }` and is inert when no worker is `waiting`; it is the confirm action behind an update prompt.
- Event flags: `isUpdate` is `true` when a worker was already controlling at `register()`; `isExternal` (on `WorkboxEvent`) is `true` for a worker not registered by this instance (a version this tab did not install); `wasWaitingBeforeRegister` (on `waiting`) is `true` when a matching worker was already waiting before `register()`.

[STACKS_WITH]:
- `vite-plugin-pwa` (`.api/vite-plugin-pwa.md`): `VitePWA({ strategies, registerType, injectRegister })` emits the SW script and the `virtual:pwa-register` helper. When `platform` owns registration directly (`injectRegister: false`), the `Workbox` instance replaces `virtual:pwa-register`; the `registerType: 'prompt'` UX is exactly listen-`waiting` → show reload prompt → `messageSkipWaiting()` on confirm → listen-`controlling` → reload. The SW `scriptURL` is the `filename`/`base` the plugin resolves.
- `workbox-build` (`.api/workbox-build.md`): `generateSW`/`injectManifest` produce the SW `Workbox` registers; `RuntimeCaching`/`StrategyName` is SW-side behavior and `workbox-window` is the strictly window-side client — the two halves never share code, only the script URL and the `postMessage` protocol.
- `effect` (`libs/typescript/.api/effect.md`): `platform/Shell/serviceworker` wraps `Workbox` as `Layer.scoped` — `register()` lifts through `Effect.tryPromise`, teardown is an `Effect.acquireRelease` finalizer, and the lifecycle events bridge to `Stream.async` via `acquireRelease`-paired `addEventListener`/`removeEventListener`, folded through `Stream.scan` into a `SubscriptionRef<SwLifecycle>` the `ui` update-notification atom observes. Model the eight event kinds as a `Data.TaggedEnum` so `Match.exhaustive` dispatches the prompt/reload arms; `active`/`controlling`/`getSW` resolve through `Effect.promise`, `messageSW`/`messageSkipWaiting` through `Effect.tryPromise`.
- platform design pages: `Shell/serviceworker` is the single `Workbox` scoped-resource + lifecycle-stream owner; `Shell/sync` drains the offline command queue by posting it through `messageSW(data)` and awaiting the SW ack; `Runtime/connectivity` pairs the SW `SyncManager` background-sync wake with the online/offline redial edge; `idb-keyval` (`.api/idb-keyval.md`) backs the offline queue the `message` round-trip flushes.

[LOCAL_ADMISSION]:
- Instantiate `Workbox` once per SW script URL in the browser entry, with the URL `vite-plugin-pwa`/`workbox-build` emitted; never call `navigator.serviceWorker.register` directly and never hand-track `statechange`/`controllerchange` — the lifecycle map already owns that.
- Drive update-notification UI from `waiting` (show prompt) → `messageSkipWaiting()` (on confirm) → `controlling` (reload); never poll the registration.
- Treat `WorkboxEventTarget` as inherited base only — attach listeners on the `Workbox` instance; do not import the base class or `ListenerCallback` from the package root (not re-exported), and though a deep `workbox-window/utils/WorkboxEventTarget` import resolves, those modules are `@private` — never reach for them.
- Keep `workbox-window` out of the service worker and the node build: it is window-only and pairs with `workbox-build`'s SW-side output across the `postMessage` wire.

[RAIL_LAW]:
- Package: `workbox-window`
- Owns: window-side SW registration, lifecycle observation, skip-waiting, and `postMessage` exchange — the browser-runtime half of the PWA seam
- Accept: one `Workbox` per SW script URL (wrapped as an Effect scoped resource with a lifecycle `SubscriptionRef`); `messageSW` for an arbitrary worker; the eight-key event map as the fold vocabulary
- Reject: raw `navigator.serviceWorker.register` plus hand-rolled lifecycle tracking; importing `WorkboxEventTarget`/`ListenerCallback` from the root; awaiting `messageSW` with no worker-side responder; use inside the SW or the node build
