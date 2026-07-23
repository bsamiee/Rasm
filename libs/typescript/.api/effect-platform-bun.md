# [TS_BRANCH_API_EFFECT_PLATFORM_BUN]

`@effect/platform-bun` binds every abstract `@effect/platform` service Tag to a Bun-native implementation, so a Node↔Bun runtime change is a Layer selection at the app root, never a code fork. It owns the Bun runtime boot, the aggregate `BunContext`, `Bun.serve` HTTP, subprocess and worker-pool exec, filesystem KV, socket transport, and the `@effect/cluster` sharding runners on Bun. It is the `runtime:node` lane the edge ledger bans inside `runtime:browser`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/platform-bun`
- package: `@effect/platform-bun` (`MIT`)
- module: ESM + CJS dual (`dist/esm` + `dist/cjs`, types `dist/dts`), `sideEffects: []`; per-module deep-import subpaths (`@effect/platform-bun/BunHttpServer`, `/BunRuntime`, …)
- runtime: `runtime:node` lane; peers `@effect/platform-node-shared` for `Sink`/`Stream`/`SocketServer`
- rail: platform/bun — the Bun binding of every abstract `@effect/platform` Tag
- depends: `@effect/platform` (abstract Tags this satisfies), `@effect/cluster`, `@effect/rpc`, `@effect/sql` (cluster-runner peers)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime boot + aggregate context

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY]  | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `BunRuntime.runMain`                                                  | `RunMain`      | Bun boot; `proc/exec` + `serve` entry |
|  [02]   | `BunContext.BunContext` / `.layer`                                    | context layer  | the aggregate platform context Tag    |
|  [03]   | `BunFileSystem.layer` / `BunPath.layer` / `layerPosix` / `layerWin32` | platform layer | `FileSystem`/`Path` Tag satisfaction  |
|  [04]   | `BunTerminal.layer` / `.make`                                         | terminal layer | `serve/cli` interactive terminal      |

[PUBLIC_TYPE_SCOPE]: HTTP serve + inbound body

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY]   | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------------- | :-------------- | :---------------------------------------------------- |
|  [01]   | `BunHttpServer.layer` / `layerServer` / `layerConfig` | server layer    | `serve/route` Bun `HttpServer`; `EventLogServer` host |
|  [02]   | `BunHttpServer.layerContext` / `layerTest` / `make`   | server layer    | context-merged / in-process test / raw make           |
|  [03]   | `BunHttpPlatform.layer` / `.make`                     | platform layer  | file-serving `HttpPlatform` for the server            |
|  [04]   | `BunHttpServerRequest.toRequest`                      | request adapter | Bun `Request` from an Effect `HttpServerRequest`      |
|  [05]   | `BunMultipart.stream` / `.persisted`                  | multipart       | inbound multipart body → stream / persisted files     |

[PUBLIC_TYPE_SCOPE]: process exec + worker pools

| [INDEX] | [SYMBOL]                                                             | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------- | :------------ | :--------------------------- |
|  [01]   | `BunCommandExecutor.layer`                                           | exec layer    | `proc/exec` subprocess       |
|  [02]   | `BunWorker.layer` / `layerPlatform` / `layerManager` / `layerWorker` | worker client | `proc/exec` worker pools     |
|  [03]   | `BunWorkerRunner.layer`                                              | worker runner | worker-side entrypoint Layer |
|  [04]   | `BunKeyValueStore.layerFileSystem`                                   | KV layer      | `proc/config` file source    |

[PUBLIC_TYPE_SCOPE]: socket transport + cluster runner

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `BunSocket.layerWebSocket` / `layerWebSocketConstructor` | socket layer   | `net/channel`, EventLog WS sync        |
|  [02]   | `BunSocketServer.*` (`node-shared`)                      | socket server  | inbound socket server                  |
|  [03]   | `BunClusterHttp.layer` / `layerHttpServer`               | cluster runner | `work/entity` sharding runner (HTTP)   |
|  [04]   | `BunClusterSocket.layer`                                 | cluster runner | `work/entity` sharding runner (socket) |
|  [05]   | `BunSink.*` / `BunStream.*` (`node-shared`)              | io             | file/stream sink + source              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: runtime boot + HTTP serve

| [INDEX] | [SURFACE]                                                   | [SHAPE]       | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `BunRuntime.runMain(effect, opts)`                          | boot          | `proc/exec` Bun row; `serve` entry                 |
|  [02]   | `BunHttpServer.layer(options: ServeOptions)`                | server layer  | `serve/route` serve row → `HttpServer` + aggregate |
|  [03]   | `BunHttpServer.layerConfig` / `layerTest` / `make(options)` | server layer  | boot-config serve; in-process test server+client   |
|  [04]   | `BunContext.layer`                                          | context layer | the aggregate Bun platform context                 |
|  [05]   | `BunMultipart.stream(request)` / `.persisted(request)`      | multipart     | inbound multipart handling                         |

- `BunHttpServer.layer` yields `Layer<HttpServer | HttpPlatform | Etag.Generator | BunContext>`; `layerConfig` wraps `ServeOptions` in `Config.Wrap`, `layerTest` binds an in-process server + client.

[ENTRYPOINT_SCOPE]: process exec + worker pools + cluster

| [INDEX] | [SURFACE]                                                         | [SHAPE]        | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `BunCommandExecutor.layer`                                        | exec layer     | `proc/exec` subprocess execution                 |
|  [02]   | `BunWorker.layer(spawn)`                                          | worker client  | `proc/exec` WorkerRunner pools                   |
|  [03]   | `BunKeyValueStore.layerFileSystem(directory)`                     | KV layer       | `proc/config` filesystem KV; `EventLog` identity |
|  [04]   | `BunSocket.layerWebSocket(url)` / `layerWebSocketConstructor`     | socket layer   | `EventLogRemote.layerWebSocket` constructor      |
|  [05]   | `BunClusterHttp.layer(options)`                                   | cluster runner | `work/entity` sharding runner                    |
|  [06]   | `BunClusterHttp.layerHttpServer` / `BunClusterSocket.layer(opts)` | cluster runner | HTTP-hosted / socket-transport runner            |

- `BunCommandExecutor.layer: Layer<CommandExecutor, never, FileSystem>` and `BunWorker.layer(spawn: (id) => Worker): Layer<WorkerManager | Spawner>` back `proc/exec`.
- `BunClusterHttp.layer` carries `{ transport: "http" | "websocket", serialization?: "msgpack" | "ndjson", clientOnly?, storage?: "local" | "sql" | "byo" }` — transport, serialization, and storage are policy values.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Node↔Bun is a Layer swap: providing `BunContext.layer` + `BunHttpServer.layer` + `BunRuntime.runMain` at the app root selects Bun, and every Bun `layer*` satisfies the same abstract `@effect/platform` Tag its Node counterpart does.
- `BunContext.layer` merges `CommandExecutor | FileSystem | Path | Terminal | WorkerManager`; a folder needing any of them composes the one aggregate Layer, never per-service merges.
- `runtime:node` purity: the edge ledger bans this package inside `runtime:browser`.

[STACKING]:
- `@effect/platform`(`.api/effect-platform.md`): `BunHttpServer.layer` binds `Bun.serve` to the `HttpServer` Tag `HttpApiBuilder.serve` yields, hosting the app-assembled `HttpApi` on the `serve/route` row.
- `@effect/experimental`(`.api/effect-experimental.md`): `BunHttpServer.layer` mounts `EventLogServer.makeHandlerHttp` as the sync-server upgrade handler; `BunSocket.layerWebSocketConstructor` and `BunKeyValueStore.layerFileSystem` satisfy `EventLogRemote.layerWebSocket` and `EventLog.layerIdentityKvs` node-side.
- `@effect/cluster`(`runtime/.api/effect-cluster.md`) + `@effect/sql`(`data/.api/effect-sql.md`): `BunClusterHttp.layer({ storage: "sql" })` is the `work/entity` sharding runner; its `MessageStorage` Tag binds `SqlMessageStorage.layer` over the `store`-owned `SqlClient` at the app root, so `work` never imports `data`.
- `@effect/opentelemetry`(`runtime/.api/effect-opentelemetry.md`): the platform `HttpClient` under the Bun runtime satisfies the `HttpClient` `Otlp.layer` requires; `NodeSdk.layer` is the SDK-bridge alternative on the node/bun lane.
- within-lib: `BunSocket`/`BunSocketServer` back the `net/channel` framed-transport rows, and `BunCommandExecutor` + `BunWorker` back the `proc/exec` subprocess + worker choreography the `work` runner discovery executes over.

[LOCAL_ADMISSION]:
- imported only inside `runtime:node` subpaths; the `tests/typescript/_architecture` import audit catches a browser rail that imports it.
- `BunContext.layer` is the aggregate binding; individual `BunFileSystem`/`BunPath`/`BunCommandExecutor` merges serve only a rail needing a single service.
- cluster runners require the `@effect/cluster`/`@effect/sql` peers; admit them only in `work` with the data driver satisfying `MessageStorage`.

[RAIL_LAW]:
- Package: `@effect/platform-bun`
- Owns: the `BunRuntime.runMain` boot + `BunContext` aggregate, `BunHttpServer` over `Bun.serve` + `BunMultipart` body parsing, `BunCommandExecutor`/`BunWorker` exec, `BunKeyValueStore` filesystem KV, `BunSocket` transport, the `BunClusterHttp`/`BunClusterSocket` `@effect/cluster` runners, and the Bun `FileSystem`/`Path`/`Terminal`/`HttpPlatform` bindings
- Accept: `runMain` as the Bun boot, `BunContext.layer` as the aggregate context, `BunHttpServer.layer` as the `serve/route` row hosting the app `HttpApi` + EventLog server, Bun `layer*` satisfying abstract `@effect/platform` Tags, `BunClusterHttp` as the `work` runner with transport/serialization/storage as policy values
- Reject: this package inside `runtime:browser`, a second `runMain`, per-service merges where `BunContext.layer` suffices, hardcoded transport/serialization/storage where the cluster options own the axis, hand-rolled `Bun.serve`/subprocess/WebSocket wrappers
