# [TS_BRANCH_API_EFFECT_PLATFORM_BROWSER]

`@effect/platform-browser` satisfies the abstract `@effect/platform` system-API Tags with browser Web-API implementations, so folder code types against the platform-neutral contract and the binding is a `Layer` selection at app composition. It owns the single `BrowserRuntime.runMain` boot and feeds the `runtime:browser` lane the edge ledger fences.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/platform-browser`
- package: `@effect/platform-browser` (MIT)
- effect-peer: `effect catalog`, `@effect/platform catalog` (the abstract Tags this package satisfies; `.api/effect.md`, `.api/effect-platform.md`)
- runtime: `runtime:browser` (browser-only; peer swap of `@effect/platform-node`/`@effect/platform-bun` behind the same `@effect/platform` Tags)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime + platform-service bindings
- rail: platform/browser

| [INDEX] | [SYMBOL]                                                                 | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `BrowserRuntime.runMain`                                                 | `RunMain`     | `browser/boot` single-boot law              |
|  [02]   | `BrowserKeyValueStore.layerLocalStorage` / `layerSessionStorage`         | KV layer      | `browser/persist`; EventLog identity        |
|  [03]   | `BrowserWorker.layer` / `layerPlatform` / `layerManager` / `layerWorker` | worker client | `browser/fetch` decode worker pool          |
|  [04]   | `BrowserWorkerRunner.layer` / `make` / `layerMessagePort`                | worker runner | worker entry; `ui/viewer` GLB decode        |
|  [05]   | `BrowserHttpClient.layerXMLHttpRequest`                                  | HTTP client   | `net/client` browser XHR; OTLP transport    |
|  [06]   | `BrowserHttpClient.currentXHRResponseType` / `withXHRArrayBuffer`        | XHR control   | force arraybuffer for binary frames         |
|  [07]   | `BrowserSocket.layerWebSocket` / `layerWebSocketConstructor`             | socket layer  | `net/channel`, `core/interchange`, EventLog |
|  [08]   | `BrowserStream.fromEventListenerWindow` / `fromEventListenerDocument`    | DOM stream    | `browser/boot` connectivity/visibility rows |

[PUBLIC_TYPE_SCOPE]: Web-API capability services
- rail: platform/browser
- Each is a `Context.Tag` + `layer` over a browser Web API with a tagged error rail (`ClipboardError`/`GeolocationError`/`PermissionsError`); `ui` declares the capability port, this package binds the `Layer`.

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `Clipboard.Clipboard` / `.layer`                       | `Context.Tag` | `ui` copy/paste; read/write text + blob |
|  [02]   | `Geolocation.Geolocation` / `.layer` / `watchPosition` | `Context.Tag` | `ui`/`viewer` position + watch stream   |
|  [03]   | `Permissions.Permissions` / `.layer`                   | `Context.Tag` | permission-state query/observe          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: runtime boot + platform-service composition
- rail: platform/browser
- `runMain` is the terminal boot; every other entry is a `Layer` satisfying an abstract `@effect/platform` Tag. `BrowserWorker.layer(spawn)` takes a `spawn(id)` factory returning `Worker | SharedWorker | MessagePort` and yields `Layer<WorkerManager | Spawner>` that `Worker.makePool`/`makePoolLayer` compose over.

| [INDEX] | [SURFACE]                                                              | [SHAPE]         | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `BrowserRuntime.runMain(effect, opts)`                                 | boot            | `browser/boot` — the one `runMain`            |
|  [02]   | `BrowserKeyValueStore.layerLocalStorage` / `layerSessionStorage`       | KV layer        | `EventLog.layerIdentityKvs` backing           |
|  [03]   | `BrowserWorker.layer(spawn)`                                           | worker client   | `browser/fetch` decode pool                   |
|  [04]   | `BrowserWorkerRunner.layer` / `layerMessagePort(port)` / `make(self)`  | worker runner   | worker-side runner entrypoint Layer           |
|  [05]   | `BrowserHttpClient.layerXMLHttpRequest` + `withXHRArrayBuffer(effect)` | HTTP client     | `net/client`; binary-frame download           |
|  [06]   | `BrowserSocket.layerWebSocket(url)` / `layerWebSocketConstructor`      | socket layer    | `EventLogRemote.layerWebSocket` constructor   |
|  [07]   | `BrowserStream.fromEventListenerWindow` / `fromEventListenerDocument`  | DOM stream      | connectivity/visibility/network `Stream` rows |
|  [08]   | `Geolocation.watchPosition` / `Clipboard.layer` / `Permissions.layer`  | Web-API service | `ui`/`viewer` capability Layers               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one boot law: `BrowserRuntime.runMain` is the single entry `browser/boot` owns, a `RunMain` instance sharing its shape with `BunRuntime.runMain`/`NodeRuntime.runMain` so the boot contract is identical across runtimes; a second boot is the named defect.
- Tag-satisfaction, not reimplementation: folder code types against `@effect/platform`'s abstract `KeyValueStore`/`Worker`/`HttpClient`/`Socket` Tags, and this package's `layer*` values satisfy them with browser implementations, so identical code runs on node/bun by Layer selection — capability is the contract, the binding is the `Layer`.
- `runtime:browser` purity: the edge ledger bans `@effect/platform-node`/`@effect/platform-bun`/`node:*` inside this scope and this package inside `runtime:node`; the `tests/typescript/_architecture` suite audits per-runtime subpath purity the exports map cannot express.

[STACKING]:
- `@effect/experimental`(`.api/effect-experimental.md`): `BrowserKeyValueStore.layerLocalStorage` satisfies the `KeyValueStore` `EventLog.layerIdentityKvs({ key })` requires, `BrowserSocket.layerWebSocketConstructor` satisfies the `Socket.WebSocketConstructor` `EventLogRemote.layerWebSocket` requires, and `EventJournal.layerIndexedDb` backs the journal — the browser EventLog client is these Layers merged.
- `@effect/opentelemetry`(`runtime/.api/effect-opentelemetry.md`): `BrowserHttpClient.layerXMLHttpRequest` is the `HttpClient` the native `Otlp.layer` requires in the browser, and `WebSdk.layer` is the browser SDK-bridge alternative — RUM export rides the XHR client.
- `@effect/platform`(`.api/effect-platform.md`): `BrowserStream.fromEventListener*` feeds the `browser/boot` connectivity rows, and the XHR `HttpClient` composes the `net/client` default-policy transformers behind the `HttpClient` Tag like any other client.
- `browser/fetch` + `ui/viewer`: `BrowserWorker.layer(spawn)` backs the off-main-thread decode pool the worker side composes with `BrowserWorkerRunner.layer`; frame reassembly and content-key verify run off-thread, delegating the mint to `core/value/contentKey`.

[LOCAL_ADMISSION]:
- Imported only inside `runtime:browser` subpaths; native-DOM ingress this package does not wrap is pinned at its `browser/*` owner page (`browser/persist`, `browser/boot`, `browser/route`), never re-spelled at a consumer.
- Web-API services (`Clipboard`/`Geolocation`/`Permissions`) are `ui`-declared ports this package satisfies; `ui` never imports it directly.
- XHR `HttpClient` is the browser transport for upload/download progress or arraybuffer; otherwise `@effect/platform`'s `fetch` client suffices.

[RAIL_LAW]:
- Package: `@effect/platform-browser`
- Owns: the `BrowserRuntime.runMain` boot, `KeyValueStore` over Web Storage, `Worker` client/runner over `Worker`/`SharedWorker`/`MessagePort`, `HttpClient` over XHR, `Socket` over `WebSocket`, DOM-event `Stream` sources, and the `Clipboard`/`Geolocation`/`Permissions` services
- Accept: `runMain` as the single browser boot, `layer*` values satisfying abstract `@effect/platform` Tags, `BrowserKeyValueStore`/`BrowserSocket` as EventLog-client backings, the XHR client for OTLP/binary transport, `BrowserWorker` for the decode pool, Web-API services as `ui`-declared ports
- Reject: a second `runMain`, `@effect/platform-node`/`@effect/platform-bun`/`node:*` in `runtime:browser`, this package in `runtime:node`, `ui` importing it directly instead of through a declared port, hand-rolled Web-Storage/WebSocket/Worker wrappers
