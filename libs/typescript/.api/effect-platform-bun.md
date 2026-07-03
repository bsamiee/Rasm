# [@effect/platform-bun] — Bun runtime bindings backing host exec rows and edge serve rows

`@effect/platform-bun` satisfies the abstract `@effect/platform` service Tags with Bun-native implementations, so a Node↔Bun runtime change is a Layer selection in the app root, never a fork. It owns `BunRuntime.runMain` + the `BunContext` aggregate (FileSystem + Path + CommandExecutor + Terminal + WorkerManager); `BunHttpServer` over `Bun.serve` (the `edge/api/serve` row); `BunCommandExecutor` subprocess exec and `BunWorker`/`BunWorkerRunner` worker pools (`host/exec/process`); `BunKeyValueStore` filesystem KV; `BunSocket`/`BunSocketServer` transport; and the `BunClusterHttp`/`BunClusterSocket` `@effect/cluster` runners (`work/engine`). It is the `runtime:node` lane the edge ledger fences against `runtime:browser`, and its rows go load-bearing only after the Leg-16 install proof `[R1]` — the peer ranges (`effect ^3.21.2`, `@effect/platform ^0.96.1`, `@effect/sql ^0.51.1`, `@effect/cluster`, `@effect/rpc`) are registry-verified.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/platform-bun`
- package: `@effect/platform-bun`
- version: `0.90.0`
- license: `MIT`
- effect-peer: `effect ^3.21.x`, `@effect/platform ^0.96.x` (abstract Tags this satisfies), `@effect/cluster`, `@effect/rpc`, `@effect/sql` (cluster-runner peers; `.api/effect.md`, `.api/effect-platform.md`)
- catalog-verdict: ADD `[R1]` — peer ranges registry-verified; bun rows load-bearing only after the Leg-16 install proof
- runtime: `runtime:node` lane — edge-ledger banned inside `runtime:browser`; peers on `@effect/platform-node-shared` for `Sink`/`Stream`/`SocketServer`
- modules: `BunRuntime`, `BunContext`, `BunHttpServer`, `BunHttpPlatform`, `BunHttpServerRequest`, `BunMultipart`, `BunCommandExecutor`, `BunWorker`, `BunWorkerRunner`, `BunTerminal`, `BunClusterHttp`, `BunClusterSocket`, `BunFileSystem`, `BunPath`, `BunKeyValueStore`, `BunSocket`, `BunSocketServer`, `BunSink`, `BunStream`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime + aggregate context
- rail: platform/bun
- `BunRuntime.runMain` is the Bun `RunMain` instance (identical shape to `BrowserRuntime.runMain`/`NodeRuntime.runMain`). `BunContext.layer` merges the whole Bun platform surface — one context Layer the app root provides, not five separate merges.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                       |
| :-----: | :-------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `BunRuntime.runMain`                                | `RunMain`      | Bun boot; `host/exec/runtime` Bun row, `edge` Bun entry     |
|  [02]   | `BunContext.BunContext` / `BunContext.layer`        | context layer  | `CommandExecutor \| FileSystem \| Path \| Terminal \| WorkerManager` aggregate |
|  [03]   | `BunFileSystem.layer` / `BunPath.layer` / `layerPosix` / `layerWin32` | platform layer | `FileSystem`/`Path` Tag satisfaction                    |
|  [04]   | `BunTerminal.layer` / `BunTerminal.make`            | terminal layer | `edge/cli` interactive terminal                            |

[PUBLIC_TYPE_SCOPE]: HTTP serve + request
- rail: platform/bun
- `BunHttpServer` is the `edge/api/serve` Bun row over `Bun.serve`, producing `HttpServer` + `HttpPlatform` + `Etag.Generator` + `BunContext`. `BunMultipart`/`BunHttpServerRequest` handle inbound bodies.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                       |
| :-----: | :-------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `BunHttpServer.layer` / `layerServer` / `layerConfig` / `layerContext` / `layerTest` / `make` | server layer | `edge/api/serve` Bun `HttpServer`; `EventLogServer` host |
|  [02]   | `BunHttpPlatform.layer` / `BunHttpPlatform.make`    | platform layer | file-serving `HttpPlatform` for the server                 |
|  [03]   | `BunHttpServerRequest.toRequest`                    | request adapter| Bun `Request` from an Effect `HttpServerRequest`           |
|  [04]   | `BunMultipart.stream` / `BunMultipart.persisted`    | multipart      | inbound multipart body → stream / persisted files          |

[PUBLIC_TYPE_SCOPE]: process exec + worker pools
- rail: platform/bun
- `BunCommandExecutor` runs subprocesses; `BunWorker`/`BunWorkerRunner` are the worker-pool client/runner (`host/exec/process` WorkerRunner pools). Worker spawn is parameterized by a `spawn(id)` factory.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                       |
| :-----: | :-------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `BunCommandExecutor.layer`                          | exec layer     | `host/exec/process` subprocess; satisfies `CommandExecutor` |
|  [02]   | `BunWorker.layer` / `layerPlatform` / `layerManager` / `layerWorker` | worker client | `host/exec/process` worker pools; satisfies `WorkerManager`/`Spawner` |
|  [03]   | `BunWorkerRunner.layer`                             | worker runner  | worker-side entrypoint Layer; satisfies `PlatformRunner`   |
|  [04]   | `BunKeyValueStore.layerFileSystem`                  | KV layer       | `host/config` file source; satisfies `KeyValueStore`       |

[PUBLIC_TYPE_SCOPE]: socket transport + cluster runner
- rail: platform/bun
- `BunSocket`/`BunSocketServer` are the socket transport; `BunClusterHttp`/`BunClusterSocket` are the `@effect/cluster` sharding runners over Bun — the `work/engine` runner entrypoint on the Bun runtime `[R5]`. `BunSink`/`BunStream`/`BunSocketServer` re-export `@effect/platform-node-shared`.

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                       |
| :-----: | :-------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `BunSocket.layerWebSocket` / `layerWebSocketConstructor` | socket layer | `host/net/channel`, EventLog WS sync; satisfies `Socket.WebSocketConstructor` |
|  [02]   | `BunSocketServer.*` (`node-shared`)                 | socket server  | inbound socket server                                      |
|  [03]   | `BunClusterHttp.layer` / `layerHttpServer`          | cluster runner | `work/engine/entity` sharding runner (HTTP transport) `[R5]` |
|  [04]   | `BunClusterSocket.layer`                            | cluster runner | `work/engine/entity` sharding runner (socket transport)    |
|  [05]   | `BunSink.*` / `BunStream.*` (`node-shared`)         | io             | file/stream sink + source                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: runtime boot + HTTP serve
- rail: platform/bun
- `runMain` boots; `BunHttpServer.layer(options)` serves; `layerConfig` wraps the serve options in `Config.Config.Wrap` for boot-time config; `layerTest` provides an in-process test server + client. The Bun `HttpServer` layer carries `HttpPlatform` + `Etag.Generator` + `BunContext`, so the `edge/api/serve` row needs only this one Layer.

| [INDEX] | [SURFACE]                                                                                                    | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `BunRuntime.runMain(effect, { disableErrorReporting?, disablePrettyLogger?, teardown? })`                    | boot           | `host/exec/runtime` Bun row; `edge` Bun entry            |
|  [02]   | `BunHttpServer.layer(options: ServeOptions): Layer<HttpServer \| HttpPlatform \| Etag.Generator \| BunContext>` | server layer | `edge/api/serve` Bun serve row                          |
|  [03]   | `BunHttpServer.layerConfig(Config.Wrap<ServeOptions>)` / `layerTest` / `make(options)`                       | server layer   | boot-config serve; in-process test server+client         |
|  [04]   | `BunContext.layer`                                                                                           | context layer  | the aggregate Bun platform context                       |
|  [05]   | `BunMultipart.stream(request)` / `BunMultipart.persisted(request)`                                           | multipart      | inbound multipart handling                               |

[ENTRYPOINT_SCOPE]: process exec + worker pools + cluster
- rail: platform/bun
- `BunCommandExecutor.layer` + `BunWorker.layer(spawn)` back `host/exec/process`. `BunClusterHttp.layer({ transport, serialization?, clientOnly?, storage? })` is the `@effect/cluster` sharding runner — one runner, transport/serialization/storage as policy values.

| [INDEX] | [SURFACE]                                                                                                    | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `BunCommandExecutor.layer: Layer<CommandExecutor, never, FileSystem>`                                        | exec layer     | `host/exec/process` subprocess execution                 |
|  [02]   | `BunWorker.layer(spawn: (id: number) => Worker): Layer<WorkerManager \| Spawner>`                            | worker client  | `host/exec/process` WorkerRunner pools                   |
|  [03]   | `BunKeyValueStore.layerFileSystem(directory: string)`                                                        | KV layer       | `host/config` filesystem KV; `EventLog` identity backing |
|  [04]   | `BunSocket.layerWebSocket(url, opts?)` / `layerWebSocketConstructor`                                         | socket layer   | `EventLogRemote.layerWebSocket` WS constructor           |
|  [05]   | `BunClusterHttp.layer({ transport: "http" \| "websocket", serialization?: "msgpack" \| "ndjson", clientOnly?, storage?: "local" \| "sql" \| "byo" })` | cluster runner | `work/engine` sharding runner `[R5]`         |
|  [06]   | `BunClusterHttp.layerHttpServer` / `BunClusterSocket.layer(opts)`                                            | cluster runner | HTTP-server-hosted / socket-transport runner             |

## [04]-[IMPLEMENTATION_LAW]

[BUN_BOUNDARY_TOPOLOGY]:
- Node↔Bun is a Layer swap: `host/exec/runtime` holds `Node | Bun` runtime rows; selecting Bun is providing `BunContext.layer` + `BunHttpServer.layer` + `BunRuntime.runMain` at the app root, never a code fork. Every Bun `layer*` satisfies the same abstract `@effect/platform` Tag its Node counterpart does.
- one aggregate context: `BunContext.layer` merges `CommandExecutor | FileSystem | Path | Terminal | WorkerManager`; folder code that needs any of them provides the one `BunContext` Layer, not per-service merges.
- `[R1]` install gate: bun rows are catalogued as ADD but go load-bearing only after the Leg-16 install proof verifies the peer ranges resolve on the target platform; until then Node is the proven serve/exec runtime.
- `runtime:node` purity: edge-ledger bans this package inside `runtime:browser`; the `tests/typescript/_architecture` suite audits it.

[INTEGRATION_LAW]:
- Stack with `@effect/platform` `HttpApp` + `@effect/experimental` EventLog server: `BunHttpServer.layer` hosts the app-assembled `HttpApi` (`edge/api/serve`) and the `EventLogServer.makeHandlerHttp` upgrade handler (`edge/live` mount port); `BunSocket.layerWebSocketConstructor` and `BunKeyValueStore.layerFileSystem` satisfy the `EventLogRemote`/`EventLog.layerIdentityKvs` requirements node-side.
- Stack with `@effect/cluster` + `@effect/sql`: `BunClusterHttp.layer({ storage: "sql" })` is the `work/engine` sharding runner; the `MessageStorage` Tag is satisfied by the store-owned `SqlClient` driver at the app root (`work` never imports `store` — the app root wires it) `[R5]`.
- Stack with `@effect/opentelemetry`: the Bun `HttpClient` (via `@effect/platform` `FetchHttpClient` under Bun, or the node-shared client) is the `HttpClient` the native `Otlp.layer` requires; `NodeSdk.layer` is the SDK-bridge alternative for the Bun/node lane.
- Stack with `host/net`: `BunSocket`/`BunSocketServer` back the `host/net/channel` framed-transport rows; `BunCommandExecutor` + `BunWorker` back the `host/exec/process` subprocess + worker choreography that `work` runner discovery and the `assay`-style ops verbs execute over.

[LOCAL_ADMISSION]:
- imported only inside `runtime:node` subpaths; a browser rail that imports it is the defect the `tests/typescript/_architecture` import audit catches.
- prefer `BunContext.layer` over individual `BunFileSystem`/`BunPath`/`BunCommandExecutor` merges unless a rail genuinely needs a single service.
- cluster runners (`BunClusterHttp`/`BunClusterSocket`) require the `@effect/cluster`/`@effect/sql` peers; admit them only in `work` with the store driver satisfying `MessageStorage`.

[RAIL_LAW]:
- Package: `@effect/platform-bun`
- Owns: the `BunRuntime.runMain` boot + `BunContext` aggregate, `BunHttpServer` over `Bun.serve`, `BunCommandExecutor`/`BunWorker` exec, `BunKeyValueStore` filesystem KV, `BunSocket` transport, the `BunClusterHttp`/`BunClusterSocket` `@effect/cluster` runners, and the Bun `FileSystem`/`Path`/`Terminal`/`HttpPlatform` bindings
- Accept: `runMain` as the Bun boot, `BunContext.layer` as the aggregate context, `BunHttpServer.layer` as the `edge` serve row hosting the app `HttpApi` + EventLog server, Bun `layer*` satisfying abstract `@effect/platform` Tags, `BunClusterHttp` as the `work` runner with transport/storage as policy values, `[R1]` install-proof gating
- Reject: this package inside `runtime:browser`, a second `runMain`, per-service merges where `BunContext.layer` suffices, hardcoded transport/serialization/storage where the cluster options own the axis, hand-rolled `Bun.serve`/subprocess/WebSocket wrappers, load-bearing bun rows before the `[R1]` Leg-16 proof
