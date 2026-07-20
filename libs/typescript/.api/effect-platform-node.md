# [TS_BRANCH_API_EFFECT_PLATFORM_NODE]

`@effect/platform-node` is the Node-runtime binding tier: concrete `Layer`s satisfying the abstract `@effect/platform` `Context.Tag` contracts on a Node process, so folder code written against the abstract Tags runs unchanged. It owns the `NodeRuntime.runMain` process edge draining fibers and finalizers on `SIGINT`/`SIGTERM`, the `NodeContext.layer` aggregate, the undici-backed HTTP client, and the Node server, socket, worker, stream-bridge, and cluster-transport bindings the tables below roster. It authors no contract of its own — the runtime half of the platform tier, swappable with `@effect/platform-bun` behind identical Tags.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/platform-node`
- package: `@effect/platform-node` (MIT, © Effectful Technologies)
- module format: ESM + CJS dual (`dist/esm` + `dist/cjs`, types `dist/dts`), `sideEffects: []`; per-module deep-import subpaths (`@effect/platform-node/NodeRuntime`, `@effect/platform-node/NodeHttpServer`, …)
- runtime target: Node — imports `node:*` builtins; bundles `undici catalog` (HTTP client + `Dispatcher`), `ws catalog` (WebSocket), `mime catalog`, and `@effect/platform-node-shared catalog` (the base shared with `-bun`)
- peer: `effect@^catalog`, `@effect/platform@^catalog`, `@effect/cluster@^catalog`, `@effect/rpc@^catalog`, `@effect/sql@^catalog` — all hard peers (`peerDependenciesMeta` is null, so none is optional); the cluster/rpc/sql trio backs only the `NodeCluster*` bindings but is required at install
- asset: pure-TypeScript runtime library binding platform Tags to `node:*`; no compiled addon
- rail: node runtime binding (proc, serve, work; catalogued once at the branch tier)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: process entry, aggregate context, and HTTP-client dispatch
- rail: system-apis
- `NodeContext.layer` is the one aggregate binding `FileSystem`+`Path`+`CommandExecutor`+`Terminal`+`Worker`; `NodeHttpClient` exposes the undici `Dispatcher`/`HttpAgent` pool Tags behind `HttpClient`.

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]  | [CONSUMER]                                            |
| :-----: | :----------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `NodeContext.layer`                        | context layer  | `proc` — the aggregate context Layer                  |
|  [02]   | `NodeHttpClient.Dispatcher` / `.HttpAgent` | pool Tags      | `net/client` — undici pool + keep-alive agent         |
|  [03]   | `NodeHttpServer` (server `Layer` factory)  | server binding | `serve/route` — `HttpServer` on `node:http`           |
|  [04]   | `Undici` (re-export of `undici`)           | raw client     | raw-undici escape hatch; domain stays on `HttpClient` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: process runtime and aggregate context
- rail: system-apis
- `NodeRuntime.runMain(effect, { disableErrorReporting?, teardown? })` is the Node `Effect.runFork` edge draining fibers/finalizers on `SIGINT`/`SIGTERM`, selected by `proc/exec`; single `Node*` bindings serve a folder needing one contract without the `NodeContext` aggregate.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [CONSUMER]                                           |
| :-----: | :-------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `NodeRuntime.runMain(effect, opts)`                       | run-main       | `proc/exec` — the Node `Effect.runFork` edge         |
|  [02]   | `NodeContext.layer`                                       | context layer  | app-root aggregate — fs/path/command/terminal/worker |
|  [03]   | `NodeFileSystem.layer` / `NodePath.layer` / `.layerPosix` | single binding | one contract without the `NodeContext` aggregate     |
|  [04]   | `NodeCommandExecutor.layer` / `NodeTerminal.layer`        | exec / tty     | `proc/exec` subprocess; `serve/cli` terminal         |

[ENTRYPOINT_SCOPE]: HTTP client and server bindings
- rail: boundaries
- `NodeHttpClient` exposes `layerUndici`/`layer`/`layerWithoutAgent` (the undici `HttpClient`) and `dispatcherLayer`/`dispatcherLayerGlobal`/`makeDispatcher` (Dispatcher tuning); `NodeHttpServer.layer(createServer, listenOptions)`/`layerConfig(createServer, config)` bind `HttpServer` to a `node:http` server, `layerConfig` reading host/port from `Config`, and the server Layer itself yields `HttpPlatform` + `Etag.Generator` + `NodeContext`. `NodeHttpPlatform.layer` binds file-serving alone; `NodeMultipart.stream`/`.persisted` parse an inbound multipart body into a field stream or persisted files.

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [CONSUMER]                                       |
| :-----: | :--------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `layerUndici` / `.layer` / `.layerWithoutAgent`                  | client layer   | `net/client` — undici `HttpClient` (HTTP/2)      |
|  [02]   | `dispatcherLayer` / `.dispatcherLayerGlobal` / `.makeDispatcher` | dispatcher     | undici `Dispatcher` (connections/pipelining/TLS) |
|  [03]   | `NodeHttpServer.layer` / `.layerConfig`                          | server layer   | `serve/route` — `HttpServer` on `node:http`      |
|  [04]   | `NodeHttpServer.layerTest` / `.makeHandler`                      | test / handler | in-process specs; raw `IncomingMessage` handler  |
|  [05]   | `NodeHttpPlatform.layer` / `NodeMultipart.stream` / `.persisted` | serve / body   | file-serving `HttpPlatform`; inbound multipart   |
|  [06]   | `NodeHttpServerRequest.toIncomingMessage` / `.toServerResponse`  | node interop   | `serve/route` — raw `node:http` (stream/upgrade) |

[ENTRYPOINT_SCOPE]: sockets, workers, and stream bridges
- rail: system-apis
- `NodeSocket`/`NodeSocketServer` bind `ws`-backed WebSocket behind `Socket`/`SocketServer` (`net/channel`); `NodeStream`/`NodeSink` bridge Node `Readable`/`Duplex`/`Writable` ⇄ Effect `Stream` and process stdio; `NodeWorker.layer(spawn)` binds the worker-thread pool for `Worker.makePoolSerialized`.

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [CONSUMER]                                           |
| :-----: | :--------------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `NodeSocket.layerWebSocket`                                      | socket         | `net/channel` — `ws` WS client                       |
|  [02]   | `NodeSocketServer.layer` / `.layerWebSocket`                     | socket server  | inbound `ws` WS server                               |
|  [03]   | `NodeWorker.layer(spawn)` / `.layerManager` / `.layerPlatform`   | worker pool    | `proc/worker` — pool for `Worker.makePoolSerialized` |
|  [04]   | `NodeWorkerRunner.layer`                                         | worker runner  | worker-thread entrypoint side of a `WorkerRunner`    |
|  [05]   | `NodeStream.fromReadable` / `.toReadable` / `.pipeThroughDuplex` | stream bridge  | `proc/exec` — `Readable`/`Duplex` ⇄ `Stream`         |
|  [06]   | `NodeStream.stdin` / `.stdout`                                   | stream bridge  | process stdio as `Stream`                            |
|  [07]   | `NodeSink.fromWritable` / `.stdout` / `.stderr`                  | sink bridge    | `Stream` → Node `Writable`; process output           |
|  [08]   | `NodeKeyValueStore.layerFileSystem(dir)`                         | kv layer       | `data/lane` — filesystem `KeyValueStore` on node     |

[ENTRYPOINT_SCOPE]: cluster transports and runner discovery
- rail: system-apis
- `NodeClusterHttp`/`NodeClusterSocket` carry `@effect/cluster` entity messages over HTTP/socket transport for `work/entity`; `NodeClusterSocket.layerK8sHttpClient` is the K8s runner-discovery client, discovery only, never provisioning.

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [CONSUMER]                                            |
| :-----: | :------------------------------------------------ | :------------- | :---------------------------------------------------- |
|  [01]   | `NodeClusterHttp.layer` / `.layerHttpServer`      | cluster http   | `work/entity` — HTTP transport for entity messaging   |
|  [02]   | `NodeClusterSocket.layer` / `.layerDispatcherK8s` | cluster socket | `work/entity` — socket transport + K8s pod-discovery  |
|  [03]   | `NodeClusterSocket.layerK8sHttpClient`            | discovery      | `work/entity` — K8s runner-discovery client           |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_NODE_TOPOLOGY]:
- This package is binding, not contract: every export is a `Layer` (or a factory returning one) that satisfies a `@effect/platform` Tag on Node. `NodeContext.layer` is the aggregate — provide it once at the app root and `FileSystem`/`Path`/`CommandExecutor`/`Terminal`/`Worker` resolve for every service beneath. A folder never imports `NodeFileSystem` in domain code; it imports the abstract `FileSystem.FileSystem` Tag and the app root supplies the Node Layer.
- `NodeRuntime.runMain` is the one Node execution edge: it runs the root `Effect` as a forked fiber, installs `SIGINT`/`SIGTERM` handlers that interrupt the fiber so `acquireRelease`/`Layer.scoped` finalizers run, and reports unhandled failures through the `Cause` pretty-printer. `proc/exec` selects `NodeRuntime.runMain` versus the Bun edge as a runtime row — the app body above it is identical.
- HTTP client is undici, not `node:http`: `NodeHttpClient.layerUndici` binds `HttpClient.HttpClient` to an undici `Dispatcher` with connection pooling, keep-alive, and HTTP/2. `net/client` layers the branch default policy (timeout, retry, proxy) on top through `HttpClient.retryTransient`/`.mapRequest`, so the pooling is a `Dispatcher` config and the policy is composed transformers — one client, both concerns.
- Server binding is `Config`-driven: `NodeHttpServer.layerConfig` reads host/port from the `ConfigProvider`, and `HttpApiBuilder.serve` (from `@effect/platform`) mounts the assembled `HttpApi` onto it. `serve/route` selects `NodeHttpServer` versus `BunHttpServer` versus `toWebHandler` as a serve row — the served `HttpApi` value is runtime-agnostic.
- `NodeStream`/`NodeSink` are the Node-boundary bridges: `NodeStream.fromReadable` lifts a `node:stream.Readable` into an Effect `Stream` with backpressure, `NodeStream.stdin`/`stdout` expose process stdio as streams, and `NodeSink.fromWritable` drains a `Stream` into a `Writable`. These are the only place a raw Node stream is admitted; downstream stays on the Effect `Stream`/`Channel` rail.
- Cluster transports are node-only capability: `NodeClusterSocket.layerK8sHttpClient` discovers runner pods via the Kubernetes API (discovery, never provisioning — provisioning is `iac`), and `NodeClusterHttp`/`NodeClusterSocket` carry `@effect/cluster` entity messages. `work/entity` composes these behind the `MessageStorage`/`Sharding` Tags the app root satisfies with a `data` driver.

[STACKING]:
- `@effect/platform` (`.api/effect-platform.md`): this package is its runtime half — every `Node*` Layer satisfies a platform Tag. Domain code composes the abstract contract; the app root supplies the Node Layer. No Node-specific domain API exists to learn.
- `effect` (`.api/effect.md`): `NodeRuntime.runMain` is the `Effect.runFork` edge; the Layers plug into the `Layer` graph; `NodeHttpServer.layerConfig` reads `Config` through the `ConfigProvider`. Node bindings extend the `Layer` graph, never a new rail.
- `@effect/platform-bun` (`.api/effect-platform-bun.md`): the peer runtime — `BunContext.layer`, `BunHttpServer.layer`, `BunRuntime.runMain` satisfy the same platform Tags. `proc/exec` and `serve/route` select node versus bun as a `Layer` row; a bun swap touches only the app root.
- `@effect/opentelemetry`: `NodeSdk.layer` rides beside `NodeContext.layer` to bind the `Tracer`/`MetricRegistry`; `otel/emit` owns the `NodeSdk` row.
- `@effect/cluster` + `@effect/sql` (catalogued at `libs/typescript/runtime/.api/` and `libs/typescript/data/.api/`): `NodeClusterHttp`/`NodeClusterSocket` transport cluster messages, and the `MessageStorage` Tag is satisfied by a `data` `@effect/sql` driver Layer at the app root — the runtime work/data lane seam meets at these bindings.

[LOCAL_ADMISSION]:
- Use the abstract `@effect/platform` Tag in domain code and provide `NodeContext.layer` (with `NodeHttpClient.layerUndici`, `NodeHttpServer.layer`) only at the app composition root; never import `Node*` Layers into a folder's domain modules.
- Use `NodeRuntime.runMain` as the single node process edge; never `Effect.runPromise` at the top of a long-lived service (it does not install signal-draining teardown).
- Use `NodeHttpClient.layerUndici` for the `HttpClient` binding and tune the `Dispatcher`; never import `undici` directly except through the `Undici` re-export at a proven escape-hatch boundary.
- Use `NodeStream.fromReadable`/`NodeSink.fromWritable` to cross the Node-stream boundary; never consume a `node:stream` with raw `.on("data")` listeners — that bypasses backpressure and the `Effect` error channel.
- Use `NodeClusterSocket.layerK8sHttpClient` for runner discovery only; provisioning the cluster is `iac`, not a runtime import.

[RAIL_LAW]:
- Package: `@effect/platform-node`
- Owns: the Node bindings for every `@effect/platform` Tag — `NodeContext`/`NodeFileSystem`/`NodePath`/`NodeCommandExecutor`/`NodeTerminal`/`NodeWorker`/`NodeWorkerRunner`, `NodeHttpClient` (undici) + `Dispatcher`/`HttpAgent`, `NodeHttpServer`/`NodeHttpPlatform`/`NodeMultipart`, `NodeSocket`/`NodeSocketServer` (ws), `NodeStream`/`NodeSink` bridges, `NodeKeyValueStore`, the `NodeCluster*` transports, `NodeRuntime.runMain`, and the raw `Undici` re-export
- Accept: `NodeContext.layer` + targeted `Node*` Layers at the app root, `NodeRuntime.runMain` as the process edge, `NodeHttpClient.layerUndici` with a tuned `Dispatcher`, `NodeStream`/`NodeSink` for stream boundaries, `NodeClusterSocket.layerK8sHttpClient` for discovery
- Reject: `Node*` Layers in domain modules, `Effect.runPromise` as a long-lived process edge, direct `undici`/`ws`/`node:stream` consumption in domain code, cluster provisioning through a runtime import
