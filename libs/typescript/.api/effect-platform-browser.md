# [@effect/platform-browser] — browser Web-API bindings backing the browser runtime

`@effect/platform-browser` satisfies the abstract `@effect/platform` service Tags with concrete browser Web-API implementations, so folder code types against the platform-neutral contract and the browser binding is a Layer selection at app composition. It owns the single `BrowserRuntime.runMain` boot law; `KeyValueStore` over `localStorage`/`sessionStorage`; the `Worker` client/runner over `Worker`/`SharedWorker`/`MessagePort`; `HttpClient` over `XMLHttpRequest` (the only transport exposing upload/download progress and arraybuffer responses); `Socket` over the native `WebSocket`; DOM-event `Stream` sources; and the `Clipboard`/`Geolocation`/`Permissions` Web-API services. This is the `runtime:browser` lane the edge ledger fences: `@effect/platform-node`/`@effect/platform-bun`/`node:*` are banned inside it, and `@effect/platform-browser` is banned inside `runtime:node` — subpath purity is enforced by the `tests/typescript/_architecture` suite.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/platform-browser`
- package: `@effect/platform-browser`
- version: `0.76.0`
- license: `MIT`
- effect-peer: `effect ^3.21.x`, `@effect/platform ^0.96.x` (the abstract Tags this package satisfies; `.api/effect.md`, `.api/effect-platform.md`)
- catalog-verdict: KEEP
- runtime: `runtime:browser` — edge-ledger banned inside `runtime:node`; bans `@effect/platform-node`/`@effect/platform-bun`/`node:*` inside its own scope
- modules: `BrowserRuntime`, `BrowserKeyValueStore`, `BrowserWorker`, `BrowserWorkerRunner`, `BrowserHttpClient`, `BrowserSocket`, `BrowserStream`, `Clipboard`, `Geolocation`, `Permissions`

[TIER_SPLIT]: this branch-tier catalog vs the native-DOM ingress
- This branch-tier catalog owns the branch-level stacking map: the `runtime browser/*` seam names, the `runtime:browser` purity ledger, and the EventLog / OpenTelemetry composition (`@effect/experimental`, `@effect/opentelemetry`).
- The native-DOM ingress this package does NOT wrap is owned by the runtime browser pages as pinned boundary refinements — `navigator.storage` `StorageManager` (`persist`/`persisted`/`estimate`) at `runtime browser/persist`, the `PermissionStatus.change` `EventTarget` bridge and `navigator.connection`/`SyncManager` at `runtime browser/boot`, `window.navigation` at `runtime browser/route` — each spelled once at its owner, never at a consumer.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime + platform-service bindings
- rail: platform/browser
- Each entry is a `Layer` that satisfies a `@effect/platform` Tag with a browser implementation. `BrowserRuntime.runMain` is the `RunMain` instance (`@effect/platform/Runtime`) that boots the app with exit-code, logging, and interrupt handling — the one boot law (`runtime browser/boot`; a second boot is the named defect).

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                       |
| :-----: | :------------------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `BrowserRuntime.runMain`                                             | `RunMain`      | `runtime browser/boot` single-boot law                     |
|  [02]   | `BrowserKeyValueStore.layerLocalStorage` / `layerSessionStorage`     | KV layer       | `runtime browser/persist`, EventLog identity; satisfies `KeyValueStore` |
|  [03]   | `BrowserWorker.layer` / `layerPlatform` / `layerManager` / `layerWorker` | worker client | `runtime browser/fetch` decode worker pool; satisfies `WorkerManager`/`Spawner` |
|  [04]   | `BrowserWorkerRunner.layer` / `make` / `layerMessagePort`            | worker runner  | worker-side entrypoint (`ui/viewer` GLB decode); satisfies `PlatformRunner` |
|  [05]   | `BrowserHttpClient.layerXMLHttpRequest`                             | HTTP client    | `runtime net/client` browser client; XHR progress + arraybuffer; OTLP export transport |
|  [06]   | `BrowserHttpClient.currentXHRResponseType` / `withXHRArrayBuffer`    | XHR control    | force arraybuffer response for binary frames               |
|  [07]   | `BrowserSocket.layerWebSocket` / `layerWebSocketConstructor`         | socket layer   | `runtime net/channel`, `wire` transport, EventLog WS sync; satisfies `Socket.WebSocketConstructor` |
|  [08]   | `BrowserStream.fromEventListenerWindow` / `fromEventListenerDocument` | DOM stream     | `runtime browser/boot` connectivity/visibility event rows  |

[PUBLIC_TYPE_SCOPE]: Web-API capability services
- rail: platform/browser
- Each is a `Context.Tag` + `layer` over a browser Web API, with a tagged error rail. `ui`/`browser` declare the runtime-capability port and this package provides the Layer at app composition (`ui` never imports `browser`).

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                       |
| :-----: | :-------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `Clipboard.Clipboard` / `Clipboard.layer` / `ClipboardError` | `Context.Tag` | `ui` copy/paste capability; read/write text + blob       |
|  [02]   | `Geolocation.Geolocation` / `Geolocation.layer` / `watchPosition` / `GeolocationError` | `Context.Tag` | `ui`/`viewer` position + watch stream                    |
|  [03]   | `Permissions.Permissions` / `Permissions.layer` / `PermissionsError` | `Context.Tag` | permission-state query/observe over the Permissions API   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: runtime boot + platform-service composition
- rail: platform/browser
- `runMain` is the terminal boot; every other entry is a `Layer` merged into the app's context that satisfies an abstract `@effect/platform` Tag. Worker spawn is parameterized by a `spawn(id)` factory returning `Worker | SharedWorker | MessagePort` — one worker law, every worker kind as the factory's return.

| [INDEX] | [SURFACE]                                                                                                    | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `BrowserRuntime.runMain(effect, { disableErrorReporting?, disablePrettyLogger?, teardown? })`               | boot           | `runtime browser/boot` — the one `runMain`               |
|  [02]   | `BrowserKeyValueStore.layerLocalStorage` / `layerSessionStorage`                                             | KV layer       | satisfies `KeyValueStore`; `EventLog.layerIdentityKvs` backing |
|  [03]   | `BrowserWorker.layer(spawn: (id: number) => Worker \| SharedWorker \| MessagePort): Layer<WorkerManager \| Spawner>` | worker client | `runtime browser/fetch`; `Worker.makePool`/`makePoolLayer` composes over it |
|  [04]   | `BrowserWorkerRunner.layer` / `layerMessagePort(port)` / `make(self)`                                        | worker runner  | worker-side runner entrypoint Layer                      |
|  [05]   | `BrowserHttpClient.layerXMLHttpRequest` + `withXHRArrayBuffer(effect)`                                       | HTTP client    | `runtime net/client` browser client; binary-frame download         |
|  [06]   | `BrowserSocket.layerWebSocket(url, opts?)` / `layerWebSocketConstructor`                                     | socket layer   | `EventLogRemote.layerWebSocket` WS constructor           |
|  [07]   | `BrowserStream.fromEventListenerWindow(type, opts?)` / `fromEventListenerDocument(type, opts?)`              | DOM stream     | connectivity/visibility/network `Stream` rows            |
|  [08]   | `Geolocation.watchPosition(opts?)` / `Clipboard.layer` / `Permissions.layer`                                | Web-API service| `ui`/`viewer` capability Layers                          |

## [04]-[IMPLEMENTATION_LAW]

[BROWSER_BOUNDARY_TOPOLOGY]:
- one boot law: `BrowserRuntime.runMain` is the single entry; `runtime browser/boot` owns it and a second boot is the named defect. It is a `RunMain` instance shared in shape with `BunRuntime.runMain`/`NodeRuntime.runMain`, so the boot contract is identical across runtimes.
- Tag-satisfaction, not reimplementation: folder code types against `@effect/platform`'s abstract `KeyValueStore`/`Worker`/`HttpClient`/`Socket` Tags; this package's `layer*` values satisfy them with browser implementations. The same folder code runs on node/bun when the app root selects the node/bun binding instead — capability is the contract, the binding is the Layer.
- `runtime:browser` purity: the edge ledger bans `@effect/platform-node`/`@effect/platform-bun`/`node:*` inside this scope and bans this package inside `runtime:node`; the `tests/typescript/_architecture` suite audits per-runtime subpath purity the exports map cannot express.

[INTEGRATION_LAW]:
- Stack with `@effect/experimental` EventLog: `BrowserKeyValueStore.layerLocalStorage` satisfies the `KeyValueStore` `EventLog.layerIdentityKvs({ key })` requires; `BrowserSocket.layerWebSocketConstructor` satisfies the `Socket.WebSocketConstructor` `EventLogRemote.layerWebSocket` requires; `EventJournal.layerIndexedDb` provides the journal. The browser EventLog client is these four Layers merged.
- Stack with `@effect/opentelemetry`: `BrowserHttpClient.layerXMLHttpRequest` is the `HttpClient` the native `Otlp.layer` requires in the browser; `WebSdk.layer` is the browser SDK-bridge alternative. Browser RUM export rides the XHR client.
- Stack with `runtime browser/fetch` + `ui/viewer`: `BrowserWorker.layer(spawn)` provides the off-main-thread decode pool; the worker side composes `BrowserWorkerRunner.layer`. Frame reassembly + content-key verify run off-thread, delegating the mint to `core/value/identity`.
- Stack with `@effect/platform` `Stream`/`HttpClient`: `BrowserStream.fromEventListener*` feeds `runtime browser/boot` connectivity rows; the XHR `HttpClient` composes the `runtime net/client` default-policy (timeout/retry) transformers like any other client.

[LOCAL_ADMISSION]:
- imported only inside `runtime:browser` subpaths; a node/bun rail that imports it is the defect the `tests/typescript/_architecture` import audit catches.
- Web-API services (`Clipboard`/`Geolocation`/`Permissions`) are provided as ports `ui` declares and `browser` satisfies; `ui` never imports this package directly.
- the XHR `HttpClient` is the browser transport when upload/download progress or arraybuffer is required; otherwise `@effect/platform`'s `fetch` client suffices.

[RAIL_LAW]:
- Package: `@effect/platform-browser`
- Owns: the `BrowserRuntime.runMain` boot, `KeyValueStore` over Web Storage, `Worker` client/runner over `Worker`/`SharedWorker`/`MessagePort`, `HttpClient` over XHR, `Socket` over `WebSocket`, DOM-event `Stream` sources, and the `Clipboard`/`Geolocation`/`Permissions` services
- Accept: `runMain` as the single browser boot, `layer*` values satisfying abstract `@effect/platform` Tags, `BrowserKeyValueStore`/`BrowserSocket` as EventLog-client backings, the XHR client for OTLP/binary transport, `BrowserWorker` for the decode pool, Web-API services as `ui`-declared ports
- Reject: a second `runMain`, `@effect/platform-node`/`@effect/platform-bun`/`node:*` in `runtime:browser`, this package in `runtime:node`, `ui` importing it directly instead of through a declared port, hand-rolled Web-Storage/WebSocket/Worker wrappers
