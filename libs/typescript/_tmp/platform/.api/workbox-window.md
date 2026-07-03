# [API_CATALOGUE] workbox-window

`workbox-window` supplies the browser-side companion to Workbox service workers. The `Workbox` class manages service worker registration, lifecycle event observation, skip-waiting coordination, and bidirectional `postMessage` communication. `messageSW` is the standalone function for sending a message to an arbitrary `ServiceWorker`. The lifecycle event map exposes eight typed events for update-notification UI. `WorkboxEventTarget` is a minimal `EventTarget` shim for environments without constructable `EventTarget`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `workbox-window`
- package: `workbox-window`
- module: `workbox-window` (`index.d.ts`), re-exports `utils/WorkboxEvent`
- asset: browser-side service worker registration and lifecycle client
- rail: service-worker-client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: event types and shim
- rail: service-worker-client

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [DESCRIPTION]                                    |
| :-----: | :----------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `Workbox`                      | class         | registration, lifecycle, and messaging owner     |
|  [02]   | `WorkboxEventTarget`           | class         | minimal `EventTarget` shim base of `Workbox`     |
|  [03]   | `WorkboxEvent<K>`              | class         | `Event` subclass shim with `type`/`target`/`sw`  |
|  [04]   | `WorkboxMessageEvent`          | interface     | `{ data, originalEvent, ports }`                 |
|  [05]   | `WorkboxLifecycleEvent`        | interface     | adds optional `isUpdate`                         |
|  [06]   | `WorkboxLifecycleWaitingEvent` | interface     | adds `wasWaitingBeforeRegister`                  |
|  [07]   | `WorkboxLifecycleEventMap`     | interface     | seven lifecycle event-name to event-type entries |
|  [08]   | `WorkboxEventMap`              | interface     | lifecycle map plus `message`                     |
|  [09]   | `ListenerCallback`             | type alias    | `(event: WorkboxEvent<any>) => any`              |

[PUBLIC_TYPE_SCOPE]: lifecycle event names (`WorkboxEventMap` keys)
- rail: service-worker-client

| [INDEX] | [EVENT]       | [EVENT_TYPE]                   | [DESCRIPTION]                                   |
| :-----: | :------------ | :----------------------------- | :---------------------------------------------- |
|  [01]   | `installing`  | `WorkboxLifecycleEvent`        | registered SW entered `installing`              |
|  [02]   | `installed`   | `WorkboxLifecycleEvent`        | registered SW entered `installed`               |
|  [03]   | `waiting`     | `WorkboxLifecycleWaitingEvent` | SW installed but not yet activating             |
|  [04]   | `activating`  | `WorkboxLifecycleEvent`        | registered SW entered `activating`              |
|  [05]   | `activated`   | `WorkboxLifecycleEvent`        | registered SW entered `activated`               |
|  [06]   | `controlling` | `WorkboxLifecycleEvent`        | new controller `scriptURL` matches the instance |
|  [07]   | `redundant`   | `WorkboxLifecycleEvent`        | registered SW entered `redundant`               |
|  [08]   | `message`     | `WorkboxMessageEvent`          | a `postMessage` was received                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Workbox` instance and standalone messaging
- rail: service-worker-client

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]  | [DESCRIPTION]                                          |
| :-----: | :------------------------------ | :-------------- | :----------------------------------------------------- |
|  [01]   | `new Workbox(scriptURL, opts?)` | constructor     | `scriptURL: string \| TrustedScriptURL`, register opts |
|  [02]   | `register(options?)`            | registration    | resolves `ServiceWorkerRegistration \| undefined`      |
|  [03]   | `update()`                      | update probe    | checks for an updated registered worker                |
|  [04]   | `active`                        | getter promise  | resolves once the registered SW is `activated`         |
|  [05]   | `controlling`                   | getter promise  | resolves once the registered SW controls the page      |
|  [06]   | `getSW()`                       | sw accessor     | resolves the matching SW as soon as one is available   |
|  [07]   | `messageSW(data)`               | message send    | posts `data`, resolves with the worker reply           |
|  [08]   | `messageSkipWaiting()`          | skip-waiting    | posts `{ type: 'SKIP_WAITING' }` to the waiting worker |
|  [09]   | `addEventListener(type, fn)`    | listener add    | typed by `WorkboxEventMap[K]`                          |
|  [10]   | `removeEventListener(type, fn)` | listener remove | typed by `WorkboxEventMap[K]`                          |
|  [11]   | `messageSW(sw, data)`           | standalone fn   | posts to an arbitrary `ServiceWorker`                  |

```ts contract
// workbox-window
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

declare class WorkboxEventTarget {
  addEventListener<K extends keyof WorkboxEventMap>(type: K, listener: (event: WorkboxEventMap[K]) => any): void
  removeEventListener<K extends keyof WorkboxEventMap>(type: K, listener: (event: WorkboxEventMap[K]) => any): void
  dispatchEvent(event: WorkboxEvent<any>): void
}
```

## [04]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- `register` delays registration until after `window.load` by default; `immediate: true` bypasses the load gate and is not recommended.
- `active` resolves once the registered SW reaches `activated`; `controlling` resolves once it controls the page, which requires `clients.claim()` in the worker for first-install activation.
- `getSW` resolves as soon as a matching SW is available, preferring a waiting worker over an active one because the waiting worker registered more recently.
- `messageSW(data)` posts to the SW from `getSW` and resolves with the response posted back on `event.ports[0]`; with no response the promise never resolves.
- `messageSkipWaiting` posts `{ type: 'SKIP_WAITING' }` and is inert when no worker is `waiting`.
- `isUpdate` is `true` when a worker was already controlling at `register()`; `isExternal` is `true` for events from a worker not registered by this instance; `wasWaitingBeforeRegister` (on `waiting`) is `true` when a matching worker was already waiting before `register()`.

[LOCAL_ADMISSION]:
- Instantiate `Workbox` in the browser entrypoint with the SW script URL produced by `generateSW`/`injectManifest` (via `workbox-build` or `vite-plugin-pwa`).
- `WorkboxEventTarget` is a custom shim, not the DOM `EventTarget`; it exists for browsers without constructable `EventTarget`.
- Drive update-notification UI from the lifecycle map: listen for `waiting` to show a reload prompt, then call `messageSkipWaiting()` on user confirmation.

[RAIL_LAW]:
- Package: `workbox-window`
- Owns: browser-side SW registration, lifecycle observation, skip-waiting, and `postMessage` exchange
- Accept: a `Workbox` instance per SW script URL; `messageSW` for arbitrary workers
- Reject: raw `navigator.serviceWorker.register` plus hand-rolled lifecycle tracking that duplicates the event map
