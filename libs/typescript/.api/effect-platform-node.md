# [TS_BRANCH_API_EFFECT_PLATFORM_NODE]

`@effect/platform-node` binds every abstract `@effect/platform` `Context.Tag` to a concrete Node `Layer`, so folder code written against the abstract Tags runs on a Node process unchanged. It authors no contract of its own — the runtime half of the platform tier, swappable with `@effect/platform-bun` behind identical Tags — and owns the `NodeRuntime.runMain` process edge draining fibers and finalizers on `SIGINT`/`SIGTERM`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/platform-node`
- package: `@effect/platform-node` (MIT, © Effectful Technologies)
- module: ESM + CJS dual (`dist/esm` + `dist/cjs`, types `dist/dts`), `sideEffects: []`, per-module deep-import subpaths (`@effect/platform-node/NodeRuntime`, `NodeHttpServer`, …)
- runtime: Node — `node:*` builtins, no compiled addon; bundles `undici` (HTTP `Dispatcher`), `ws` (WebSocket), `mime`, and `@effect/platform-node-shared` (the base shared with `-bun`)
- depends: hard peers on `effect`, `@effect/platform`, `@effect/cluster`, `@effect/rpc`, `@effect/sql` (none optional); the cluster/rpc/sql trio backs the `NodeCluster*` bindings and is required at install
- rail: node runtime binding — proc, serve, work

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: process entry, aggregate context, and HTTP-client dispatch
- rail: system-apis
- `NodeContext.layer` aggregates `FileSystem`/`Path`/`CommandExecutor`/`Terminal`/`Worker`; `NodeHttpClient` surfaces the undici `Dispatcher`/`HttpAgent` pool Tags behind the abstract `HttpClient`.

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]  | [CAPABILITY]                                          |
| :-----: | :----------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `NodeContext.layer`                        | context layer  | `proc` — the aggregate context Layer                  |
|  [02]   | `NodeHttpClient.Dispatcher` / `.HttpAgent` | pool Tags      | `net/client` — undici pool + keep-alive agent         |
|  [03]   | `NodeHttpServer` (server `Layer` factory)  | server binding | `serve/route` — `HttpServer` on `node:http`           |
|  [04]   | `Undici` (re-export of `undici`)           | raw client     | raw-undici escape hatch; domain stays on `HttpClient` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: process runtime and aggregate context
- rail: system-apis
- Single `Node*` bindings serve a folder needing one contract without the `NodeContext` aggregate; the aggregate resolves fs/path/command/terminal/worker in one Layer.

| [INDEX] | [SURFACE]                                                 | [SHAPE]        | [CAPABILITY]                                         |
| :-----: | :-------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `NodeRuntime.runMain(effect, opts)`                       | run-main       | `proc/exec` — the Node `Effect.runFork` edge         |
|  [02]   | `NodeContext.layer`                                       | context layer  | app-root aggregate — fs/path/command/terminal/worker |
|  [03]   | `NodeFileSystem.layer` / `NodePath.layer` / `.layerPosix` | single binding | one contract without the `NodeContext` aggregate     |
|  [04]   | `NodeCommandExecutor.layer` / `NodeTerminal.layer`        | exec / tty     | `proc/exec` subprocess; `serve/cli` terminal         |

[ENTRYPOINT_SCOPE]: HTTP client and server bindings
- rail: boundaries
- `NodeHttpServer.layer` yields `HttpPlatform` + `Etag.Generator` + `NodeContext` beside `HttpServer`; `layerConfig` reads host/port from `Config`, `NodeHttpPlatform.layer` binds file-serving alone, and `NodeMultipart` parses an inbound multipart body into a field stream or persisted files.

| [INDEX] | [SURFACE]                                                        | [SHAPE]        | [CAPABILITY]                                     |
| :-----: | :--------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `layerUndici` / `.layer` / `.layerWithoutAgent`                  | client layer   | `net/client` — undici `HttpClient` (HTTP/2)      |
|  [02]   | `dispatcherLayer` / `.dispatcherLayerGlobal` / `.makeDispatcher` | dispatcher     | undici `Dispatcher` (connections/pipelining/TLS) |
|  [03]   | `NodeHttpServer.layer` / `.layerConfig`                          | server layer   | `serve/route` — `HttpServer` on `node:http`      |
|  [04]   | `NodeHttpServer.layerTest` / `.makeHandler`                      | test / handler | in-process specs; raw `IncomingMessage` handler  |
|  [05]   | `NodeHttpPlatform.layer` / `NodeMultipart.stream` / `.persisted` | serve / body   | file-serving `HttpPlatform`; inbound multipart   |
|  [06]   | `NodeHttpServerRequest.toIncomingMessage` / `.toServerResponse`  | node interop   | `serve/route` — raw `node:http` (stream/upgrade) |

[ENTRYPOINT_SCOPE]: sockets, workers, and stream bridges
- rail: system-apis
- `NodeSocket`/`NodeSocketServer` bind `ws`-backed WebSocket behind `Socket`/`SocketServer`; `NodeStream`/`NodeSink` bridge Node `Readable`/`Duplex`/`Writable` ⇄ Effect `Stream` and process stdio; `NodeWorker.layer(spawn)` backs `Worker.makePoolSerialized`.

| [INDEX] | [SURFACE]                                                        | [SHAPE]       | [CAPABILITY]                                         |
| :-----: | :--------------------------------------------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `NodeSocket.layerWebSocket`                                      | socket        | `net/channel` — `ws` WS client                       |
|  [02]   | `NodeSocketServer.layer` / `.layerWebSocket`                     | socket server | inbound `ws` WS server                               |
|  [03]   | `NodeWorker.layer(spawn)` / `.layerManager` / `.layerPlatform`   | worker pool   | `proc/worker` — pool for `Worker.makePoolSerialized` |
|  [04]   | `NodeWorkerRunner.layer`                                         | worker runner | worker-thread entrypoint side of a `WorkerRunner`    |
|  [05]   | `NodeStream.fromReadable` / `.toReadable` / `.pipeThroughDuplex` | stream bridge | `proc/exec` — `Readable`/`Duplex` ⇄ `Stream`         |
|  [06]   | `NodeStream.stdin` / `.stdout`                                   | stream bridge | process stdio as `Stream`                            |
|  [07]   | `NodeSink.fromWritable` / `.stdout` / `.stderr`                  | sink bridge   | `Stream` → Node `Writable`; process output           |
|  [08]   | `NodeKeyValueStore.layerFileSystem(dir)`                         | kv layer      | `data/lane` — filesystem `KeyValueStore` on node     |

[ENTRYPOINT_SCOPE]: cluster transports and runner discovery
- rail: system-apis
- `NodeClusterHttp`/`NodeClusterSocket` carry `@effect/cluster` entity messages over HTTP/socket transport; `NodeClusterSocket.layerK8sHttpClient` discovers runner pods, discovery only.

| [INDEX] | [SURFACE]                                         | [SHAPE]        | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------ | :------------- | :--------------------------------------------------- |
|  [01]   | `NodeClusterHttp.layer` / `.layerHttpServer`      | cluster http   | `work/entity` — HTTP transport for entity messaging  |
|  [02]   | `NodeClusterSocket.layer` / `.layerDispatcherK8s` | cluster socket | `work/entity` — socket transport + K8s pod-discovery |
|  [03]   | `NodeClusterSocket.layerK8sHttpClient`            | discovery      | `work/entity` — K8s runner-discovery client          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Binding, not contract: every export is a `Layer` (or a factory returning one) satisfying a `@effect/platform` Tag on Node, and `NodeContext.layer` is the aggregate — provide it once at the app root and all five services resolve for every service beneath.
- `NodeRuntime.runMain` is the one execution edge: it forks the root `Effect`, installs `SIGINT`/`SIGTERM` interruption so `acquireRelease`/`Layer.scoped` finalizers run, and reports failures through the `Cause` pretty-printer.
- HTTP client is undici, not `node:http`: `layerUndici` binds `HttpClient` to a pooled undici `Dispatcher` (keep-alive, HTTP/2), and `net/client` layers timeout/retry/proxy on top through `HttpClient.retryTransient`/`.mapRequest`.
- Server binding is `Config`-driven: `layerConfig` reads host/port from the `ConfigProvider`, the served `HttpApi` value stays runtime-agnostic, and `serve/route` selects Node versus Bun versus `toWebHandler` as a row.
- `NodeStream`/`NodeSink` are the sole Node-stream admission point: `fromReadable` lifts a `Readable` into a backpressured `Stream`, `fromWritable` drains a `Stream` into a `Writable`, and downstream stays on the `Stream`/`Channel` rail.
- Cluster transport is Node-only: `NodeClusterHttp`/`NodeClusterSocket` carry `@effect/cluster` entity messages, `layerK8sHttpClient` discovers runner pods via the Kubernetes API, and `work/entity` composes them behind the `MessageStorage`/`Sharding` Tags.

[STACKING]:
- `@effect/platform`(`.api/effect-platform.md`): this package is its runtime half — every `Node*` Layer satisfies a platform Tag, domain code composes the abstract contract, and the app root supplies the Node Layer.
- `effect`(`.api/effect.md`): `NodeRuntime.runMain` is the `Effect.runFork` edge, the Layers plug into the `Layer` graph, and `layerConfig` reads `Config` through the `ConfigProvider`.
- `@effect/platform-bun`(`.api/effect-platform-bun.md`): the peer runtime — `BunContext.layer`/`BunHttpServer.layer`/`BunRuntime.runMain` satisfy the same Tags, so a Bun swap touches only the app root.
- `@effect/opentelemetry`(`runtime/.api/effect-opentelemetry.md`): `NodeSdk.layer` (SDK-bridge, `sdk-trace-node`) rides beside `NodeContext.layer` to bind `Tracer.OtelTracer`/metrics; the native `Otlp.layer` over the undici `HttpClient` is the default lane, `otel/emit` owns the row.
- `@effect/cluster`(`runtime/.api/effect-cluster.md`) + `@effect/sql`(`data/.api/effect-sql.md`): `NodeClusterHttp`/`NodeClusterSocket` transport cluster messages, and a `data` `@effect/sql` driver Layer satisfies the `MessageStorage` Tag at the app root — the work/data seam meets here.

[LOCAL_ADMISSION]:
- `Node*` Layers are admitted only at the app composition root; domain modules import the abstract `@effect/platform` Tag.
- Cluster peers (`@effect/cluster`/`@effect/rpc`/`@effect/sql`) are admitted only in `work`, with a `data` `@effect/sql` driver satisfying `MessageStorage`; `layerK8sHttpClient` is runner discovery, provisioning is `iac`.

[RAIL_LAW]:
- Package: `@effect/platform-node`
- Owns: the Node binding for every `@effect/platform` Tag — context/fs/path/command/terminal/worker, the undici `HttpClient` + `Dispatcher`/`HttpAgent`, `NodeHttpServer`/`NodeHttpPlatform`/`NodeMultipart`, the `ws` socket transport, the `NodeStream`/`NodeSink` bridges, `NodeKeyValueStore`, the `NodeCluster*` transports, `NodeRuntime.runMain`, and the raw `Undici` re-export
- Accept: `NodeContext.layer` + targeted `Node*` Layers at the app root, `NodeRuntime.runMain` as the process edge, `NodeHttpClient.layerUndici` with a tuned `Dispatcher`, `NodeStream`/`NodeSink` for stream boundaries, `NodeClusterSocket.layerK8sHttpClient` for discovery
- Reject: `Node*` Layers in domain modules, `Effect.runPromise` as a long-lived process edge, direct `undici`/`ws`/`node:stream` consumption in domain code, cluster provisioning through a runtime import
