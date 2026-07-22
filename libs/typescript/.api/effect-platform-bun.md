# [TS_BRANCH_API_EFFECT_PLATFORM_BUN]

`@effect/platform-bun` satisfies the abstract `@effect/platform` service Tags with Bun-native implementations, so a Node↔Bun runtime change is a Layer selection in the app root, never a fork. It owns the `BunRuntime.runMain` boot, the `BunContext` aggregate, `BunHttpServer` over `Bun.serve`, subprocess and worker-pool exec, filesystem KV, socket transport, and the `@effect/cluster` runners the tables below roster. It is the `runtime:node` lane the edge ledger fences against `runtime:browser`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/platform-bun`
- package: `@effect/platform-bun`
- license: `MIT`
- effect-peer: `effect catalog`, `@effect/platform catalog` (abstract Tags this satisfies), `@effect/cluster`, `@effect/rpc`, `@effect/sql` (cluster-runner peers; `.api/effect.md`, `.api/effect-platform.md`)
- runtime: `runtime:node` lane — edge-ledger banned inside `runtime:browser`; peers on `@effect/platform-node-shared` for `Sink`/`Stream`/`SocketServer`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime + aggregate context
- rail: platform/bun
- `BunRuntime.runMain` is the Bun `RunMain` instance (identical shape to `BrowserRuntime.runMain`/`NodeRuntime.runMain`). `BunContext.layer` merges the whole Bun platform surface into one context Layer — the `CommandExecutor | FileSystem | Path | Terminal | WorkerManager` aggregate the app root wires, not five separate merges.

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY]  | [CONSUMER_BOUNDARY]                   |
| :-----: | :-------------------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `BunRuntime.runMain`                                                  | `RunMain`      | Bun boot; `proc/exec` + `serve` entry |
|  [02]   | `BunContext.BunContext` / `.layer`                                    | context layer  | the aggregate platform context Tag    |
|  [03]   | `BunFileSystem.layer` / `BunPath.layer` / `layerPosix` / `layerWin32` | platform layer | `FileSystem`/`Path` Tag satisfaction  |
|  [04]   | `BunTerminal.layer` / `.make`                                         | terminal layer | `serve/cli` interactive terminal      |

[PUBLIC_TYPE_SCOPE]: HTTP serve + request
- rail: platform/bun
- `BunHttpServer` is the `serve/route` Bun row over `Bun.serve` (`layer`/`layerServer`/`layerConfig`/`layerContext`/`layerTest`/`make`), producing `HttpServer` + `HttpPlatform` + `Etag.Generator` + `BunContext`. `BunMultipart`/`BunHttpServerRequest` handle inbound bodies.

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                   |
| :-----: | :---------------------------------------------------- | :-------------- | :---------------------------------------------------- |
|  [01]   | `BunHttpServer.layer` / `layerServer` / `layerConfig` | server layer    | `serve/route` Bun `HttpServer`; `EventLogServer` host |
|  [02]   | `BunHttpServer.layerContext` / `layerTest` / `make`   | server layer    | context-merged / in-process test / raw make           |
|  [03]   | `BunHttpPlatform.layer` / `.make`                     | platform layer  | file-serving `HttpPlatform` for the server            |
|  [04]   | `BunHttpServerRequest.toRequest`                      | request adapter | Bun `Request` from an Effect `HttpServerRequest`      |
|  [05]   | `BunMultipart.stream` / `.persisted`                  | multipart       | inbound multipart body → stream / persisted files     |

[PUBLIC_TYPE_SCOPE]: process exec + worker pools
- rail: platform/bun
- `BunCommandExecutor` runs subprocesses; `BunWorker`/`BunWorkerRunner` are the worker-pool client/runner (`proc/exec` worker pools), spawn parameterized by a `spawn(id)` factory, satisfying `CommandExecutor`/`WorkerManager`/`Spawner`/`PlatformRunner`/`KeyValueStore`.

| [INDEX] | [SYMBOL]                                                             | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]          |
| :-----: | :------------------------------------------------------------------- | :------------ | :--------------------------- |
|  [01]   | `BunCommandExecutor.layer`                                           | exec layer    | `proc/exec` subprocess       |
|  [02]   | `BunWorker.layer` / `layerPlatform` / `layerManager` / `layerWorker` | worker client | `proc/exec` worker pools     |
|  [03]   | `BunWorkerRunner.layer`                                              | worker runner | worker-side entrypoint Layer |
|  [04]   | `BunKeyValueStore.layerFileSystem`                                   | KV layer      | `proc/config` file source    |

[PUBLIC_TYPE_SCOPE]: socket transport + cluster runner
- rail: platform/bun
- `BunSocket`/`BunSocketServer` are the socket transport satisfying `Socket.WebSocketConstructor`; `BunClusterHttp`/`BunClusterSocket` are the `@effect/cluster` sharding runners over Bun — the `work/entity` runner entrypoint on the Bun runtime. `BunSink`/`BunStream`/`BunSocketServer` re-export `@effect/platform-node-shared`.

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]  | [CONSUMER_BOUNDARY]                    |
| :-----: | :------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `BunSocket.layerWebSocket` / `layerWebSocketConstructor` | socket layer   | `net/channel`, EventLog WS sync        |
|  [02]   | `BunSocketServer.*` (`node-shared`)                      | socket server  | inbound socket server                  |
|  [03]   | `BunClusterHttp.layer` / `layerHttpServer`               | cluster runner | `work/entity` sharding runner (HTTP)   |
|  [04]   | `BunClusterSocket.layer`                                 | cluster runner | `work/entity` sharding runner (socket) |
|  [05]   | `BunSink.*` / `BunStream.*` (`node-shared`)              | io             | file/stream sink + source              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: runtime boot + HTTP serve
- rail: platform/bun
- `BunRuntime.runMain(effect, { disableErrorReporting?, disablePrettyLogger?, teardown? })` boots; `BunHttpServer.layer(options: ServeOptions)` serves, producing `Layer<HttpServer | HttpPlatform | Etag.Generator | BunContext>`; `layerConfig` wraps the serve options in `Config.Config.Wrap` for boot-time config; `layerTest` binds an in-process test server + client, so the `serve/route` row needs only this one Layer.

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `BunRuntime.runMain(effect, opts)`                          | boot           | `proc/exec` Bun row; `serve` entry                 |
|  [02]   | `BunHttpServer.layer(options: ServeOptions)`                | server layer   | `serve/route` serve row → `HttpServer` + aggregate |
|  [03]   | `BunHttpServer.layerConfig` / `layerTest` / `make(options)` | server layer   | boot-config serve; in-process test server+client   |
|  [04]   | `BunContext.layer`                                          | context layer  | the aggregate Bun platform context                 |
|  [05]   | `BunMultipart.stream(request)` / `.persisted(request)`      | multipart      | inbound multipart handling                         |

[ENTRYPOINT_SCOPE]: process exec + worker pools + cluster
- rail: platform/bun
- `BunCommandExecutor.layer: Layer<CommandExecutor, never, FileSystem>` + `BunWorker.layer(spawn: (id: number) => Worker): Layer<WorkerManager | Spawner>` back `proc/exec`; `BunKeyValueStore.layerFileSystem(directory)` is the filesystem KV. `BunClusterHttp.layer({ transport: "http" | "websocket", serialization?: "msgpack" | "ndjson", clientOnly?, storage?: "local" | "sql" | "byo" })` is the `@effect/cluster` sharding runner — transport/serialization/storage as policy values.

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                              |
| :-----: | :---------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `BunCommandExecutor.layer`                                        | exec layer     | `proc/exec` subprocess execution                 |
|  [02]   | `BunWorker.layer(spawn)`                                          | worker client  | `proc/exec` WorkerRunner pools                   |
|  [03]   | `BunKeyValueStore.layerFileSystem(directory)`                     | KV layer       | `proc/config` filesystem KV; `EventLog` identity |
|  [04]   | `BunSocket.layerWebSocket(url)` / `layerWebSocketConstructor`     | socket layer   | `EventLogRemote.layerWebSocket` constructor      |
|  [05]   | `BunClusterHttp.layer(options)`                                   | cluster runner | `work/entity` sharding runner                    |
|  [06]   | `BunClusterHttp.layerHttpServer` / `BunClusterSocket.layer(opts)` | cluster runner | HTTP-hosted / socket-transport runner            |

## [04]-[IMPLEMENTATION_LAW]

[BUN_BOUNDARY_TOPOLOGY]:
- Node↔Bun is a Layer swap: `proc/exec` holds `Node | Bun` runtime rows; selecting Bun is providing `BunContext.layer` + `BunHttpServer.layer` + `BunRuntime.runMain` at the app root, never a code fork. Every Bun `layer*` satisfies the same abstract `@effect/platform` Tag its Node counterpart does.
- one aggregate context: `BunContext.layer` merges `CommandExecutor | FileSystem | Path | Terminal | WorkerManager`; folder code that needs any of them composes the one `BunContext` Layer, not per-service merges.
- `runtime:node` purity: edge-ledger bans this package inside `runtime:browser`; the `tests/typescript/_architecture` suite audits it.

[STACKING]:
- Stack with `@effect/platform` `HttpApp` + `@effect/experimental` EventLog server: `BunHttpServer.layer` hosts the app-assembled `HttpApi` (`serve/route`) and the `EventLogServer.makeHandlerHttp` upgrade handler (`serve/live` mount port); `BunSocket.layerWebSocketConstructor` and `BunKeyValueStore.layerFileSystem` satisfy the `EventLogRemote`/`EventLog.layerIdentityKvs` requirements node-side.
- Stack with `@effect/cluster` + `@effect/sql`: `BunClusterHttp.layer({ storage: "sql" })` is the `work/entity` sharding runner; the `MessageStorage` Tag is satisfied by the data-owned `SqlClient` driver at the app root (`work` never imports `data` — the app root wires it).
- Stack with `@effect/opentelemetry`: the Bun `HttpClient` (via `@effect/platform` `FetchHttpClient` under Bun, or the node-shared client) is the `HttpClient` the native `Otlp.layer` requires; `NodeSdk.layer` is the SDK-bridge alternative for the Bun/node lane.
- Stack with `net/client`: `BunSocket`/`BunSocketServer` back the `net/channel` framed-transport rows; `BunCommandExecutor` + `BunWorker` back the `proc/exec` subprocess + worker choreography `work` runner discovery executes over.

[LOCAL_ADMISSION]:
- imported only inside `runtime:node` subpaths; a browser rail that imports it is the defect the `tests/typescript/_architecture` import audit catches.
- `BunContext.layer` is the aggregate binding; individual `BunFileSystem`/`BunPath`/`BunCommandExecutor` merges serve only a rail needing a single service.
- cluster runners (`BunClusterHttp`/`BunClusterSocket`) require the `@effect/cluster`/`@effect/sql` peers; admit them only in `work` with the data driver satisfying `MessageStorage`.

[RAIL_LAW]:
- Package: `@effect/platform-bun`
- Owns: the `BunRuntime.runMain` boot + `BunContext` aggregate, `BunHttpServer` over `Bun.serve` + `BunMultipart` body parsing, `BunCommandExecutor`/`BunWorker` exec, `BunKeyValueStore` filesystem KV, `BunSocket` transport, the `BunClusterHttp`/`BunClusterSocket` `@effect/cluster` runners, and the Bun `FileSystem`/`Path`/`Terminal`/`HttpPlatform` bindings
- Accept: `runMain` as the Bun boot, `BunContext.layer` as the aggregate context, `BunHttpServer.layer` as the `serve/route` row hosting the app `HttpApi` + EventLog server, Bun `layer*` satisfying abstract `@effect/platform` Tags, `BunClusterHttp` as the `work` runner with transport/serialization/storage as policy values
- Reject: this package inside `runtime:browser`, a second `runMain`, per-service merges where `BunContext.layer` suffices, hardcoded transport/serialization/storage where the cluster options own the axis, hand-rolled `Bun.serve`/subprocess/WebSocket wrappers
