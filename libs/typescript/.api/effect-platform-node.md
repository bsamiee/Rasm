# [TS_BRANCH_API_EFFECT_PLATFORM_NODE]

`@effect/platform-node` is the Node-runtime binding tier: it provides the concrete `Layer`s that satisfy the abstract `@effect/platform` `Context.Tag` contracts on a Node process, so `host` and `edge` code written against `FileSystem`/`HttpClient`/`HttpServer`/`Command`/`Worker` runs unchanged. It owns the process entry (`NodeRuntime.runMain` — the `Effect.runFork` edge that drains fibers and finalizers on `SIGINT`/`SIGTERM`), the aggregate context Layer (`NodeContext.layer` binding filesystem, path, command executor, terminal, and worker in one), the undici-backed HTTP client (`NodeHttpClient` with `Dispatcher`/`HttpAgent` connection pooling and HTTP/2), the Node HTTP server (`NodeHttpServer.layer`/`layerConfig`/`layerTest`), the socket and WebSocket bindings (`NodeSocket`, `NodeSocketServer` over `ws`), the worker-thread pools (`NodeWorker`/`NodeWorkerRunner`), the readable/writable-stream bridges (`NodeStream`/`NodeSink` — Node stream ⇄ Effect `Stream`/`Channel`, plus `stdin`/`stdout`/`stderr`), the cluster transports (`NodeClusterHttp`, `NodeClusterSocket` with the K8s runner-discovery client), and the raw `undici` re-export. It authors no contract of its own — it is the runtime half of the platform tier, swappable with `@effect/platform-bun` behind identical Tags.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/platform-node`
- package: `@effect/platform-node` (0.107.0, MIT, © Effectful Technologies)
- module format: ESM + CJS dual (`dist/esm` + `dist/cjs`, types `dist/dts`), `sideEffects: []`; per-module deep-import subpaths (`@effect/platform-node/NodeRuntime`, `@effect/platform-node/NodeHttpServer`, …)
- runtime target: Node — imports `node:*` builtins; bundles `undici@^7.10` (HTTP client + `Dispatcher`), `ws@^8.18` (WebSocket), `mime@^3`, and `@effect/platform-node-shared@^0.60` (the base shared with `-bun`)
- peer: `effect@^3.21.2`, `@effect/platform@^0.96.1`, `@effect/cluster@^0.59.0`, `@effect/rpc@^0.75.1`, `@effect/sql@^0.51.1` — all hard peers (`peerDependenciesMeta` is null, so none is optional); the cluster/rpc/sql trio backs only the `NodeCluster*` bindings but is required at install
- asset: pure-TypeScript runtime library binding platform Tags to `node:*`; no compiled addon
- rail: node runtime binding (host, edge, work; catalogued once at the branch tier)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: process entry, aggregate context, and HTTP-client dispatch
- rail: system-apis

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]   | [CONSUMER]                                                        |
| :-----: | :---------------------------------------------- | :-------------- | :--------------------------------------------------------------- |
|  [01]   | `NodeContext.layer`                              | context layer    | `host` — one `Layer` binding `FileSystem`+`Path`+`CommandExecutor`+`Terminal`+`Worker` |
|  [02]   | `NodeHttpClient.Dispatcher` / `NodeHttpClient.HttpAgent` | pool Tags | `host/net/client.ts` — undici connection-pool / keep-alive agent behind `HttpClient` |
|  [03]   | `NodeHttpServer` (server `Layer` factory)        | server binding   | `edge/serve` — binds `HttpServer` to a `node:http` listener |
|  [04]   | `Undici` (re-export of `undici`)                 | raw client       | escape hatch for a raw undici request; domain code stays on `HttpClient` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: process runtime and aggregate context
- rail: system-apis

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]  | [CONSUMER]                                                   |
| :-----: | :--------------------------------------------------------------------- | :-------------- | :---------------------------------------------------------- |
|  [01]   | `NodeRuntime.runMain(effect, { disableErrorReporting?, teardown? })`   | run-main        | `host/exec/runtime.ts` — the Node `Effect.runFork` edge; drains on `SIGINT`/`SIGTERM` |
|  [02]   | `NodeContext.layer`                                                    | context layer   | provided at the app root under every node service — fs, path, command, terminal, worker |
|  [03]   | `NodeFileSystem.layer` / `NodePath.layer` / `NodePath.layerPosix`      | single binding  | when a folder needs one contract without the full `NodeContext` aggregate |
|  [04]   | `NodeCommandExecutor.layer` / `NodeTerminal.layer`                     | exec / tty      | `host/exec/process.ts` subprocess execution; `edge/cli` interactive terminal |

[ENTRYPOINT_SCOPE]: HTTP client and server bindings
- rail: boundaries

| [INDEX] | [SURFACE]                                                                         | [ENTRY_FAMILY]  | [CONSUMER]                                                   |
| :-----: | :-------------------------------------------------------------------------------- | :-------------- | :---------------------------------------------------------- |
|  [01]   | `NodeHttpClient.layerUndici` / `.layer` / `.layerWithoutAgent`                     | client layer    | `host/net/client.ts` — the undici-backed `HttpClient` (HTTP/2, pooling) |
|  [02]   | `NodeHttpClient.dispatcherLayer` / `.dispatcherLayerGlobal` / `.makeDispatcher`    | dispatcher      | tune the undici `Dispatcher` (connections, pipelining, TLS) under the client |
|  [03]   | `NodeHttpServer.layer(createServer, listenOptions)` / `.layerConfig(createServer, config)` | server layer    | `edge/serve` — bind `HttpServer` to a `node:http` server; `layerConfig` reads host/port from `Config` |
|  [04]   | `NodeHttpServer.layerTest` / `NodeHttpServer.makeHandler`                          | test / handler  | kit-driven in-process server specs; a raw `IncomingMessage => ServerResponse` handler |
|  [05]   | `NodeHttpServerRequest.toIncomingMessage` / `.toServerResponse`                    | node interop    | `edge` — reach the raw `node:http` objects at the boundary (streaming, upgrade) |

[ENTRYPOINT_SCOPE]: sockets, workers, and stream bridges
- rail: system-apis

| [INDEX] | [SURFACE]                                                                         | [ENTRY_FAMILY]  | [CONSUMER]                                                   |
| :-----: | :-------------------------------------------------------------------------------- | :-------------- | :---------------------------------------------------------- |
|  [01]   | `NodeSocket.layerWebSocket(url)` / `NodeSocketServer.layer` / `.layerWebSocket`    | socket          | `host/net/channel.ts` — `ws`-backed WebSocket client/server behind `Socket`/`SocketServer` |
|  [02]   | `NodeWorker.layer(spawn)` / `.layerManager` / `.layerPlatform`                     | worker pool     | `host/exec` — worker-thread pool binding for `Worker.makePoolSerialized` |
|  [03]   | `NodeWorkerRunner.layer`                                                           | worker runner   | the worker-thread entrypoint side of a `WorkerRunner` handler |
|  [04]   | `NodeStream.fromReadable` / `.toReadable` / `.pipeThroughDuplex` / `.stdin` / `.stdout` | stream bridge | `host/exec/process.ts` — Node `Readable`/`Duplex` ⇄ Effect `Stream`; process stdio |
|  [05]   | `NodeSink.fromWritable` / `NodeSink.stdout` / `NodeSink.stderr`                    | sink bridge     | write an Effect `Stream` into a Node `Writable`; CLI/process output |
|  [06]   | `NodeKeyValueStore.layerFileSystem(dir)`                                           | kv layer        | `store/lane` — filesystem-backed `KeyValueStore` binding on node |

[ENTRYPOINT_SCOPE]: cluster transports and runner discovery
- rail: system-apis

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY]  | [CONSUMER]                                                   |
| :-----: | :----------------------------------------------------------------------- | :-------------- | :---------------------------------------------------------- |
|  [01]   | `NodeClusterHttp.layer` / `NodeClusterHttp.layerHttpServer`              | cluster http    | `work/engine` — HTTP transport for `@effect/cluster` entity messaging |
|  [02]   | `NodeClusterSocket.layer` / `NodeClusterSocket.layerDispatcherK8s`       | cluster socket  | `work/engine` — socket transport + the K8s pod-discovery dispatcher |
|  [03]   | `NodeClusterSocket.layerK8sHttpClient`                                   | discovery       | `work/engine/entity.ts` — the K8s runner-discovery client (discovery only, never provisioning) |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_NODE_TOPOLOGY]:
- This package is binding, not contract: every export is a `Layer` (or a factory returning one) that satisfies a `@effect/platform` Tag on Node. `NodeContext.layer` is the aggregate — provide it once at the app root and `FileSystem`/`Path`/`CommandExecutor`/`Terminal`/`Worker` resolve for every service beneath. A folder never imports `NodeFileSystem` in domain code; it imports the abstract `FileSystem.FileSystem` Tag and the app root supplies the Node Layer.
- `NodeRuntime.runMain` is the one Node execution edge: it runs the root `Effect` as a forked fiber, installs `SIGINT`/`SIGTERM` handlers that interrupt the fiber so `acquireRelease`/`Layer.scoped` finalizers run, and reports unhandled failures through the `Cause` pretty-printer. `host/exec/runtime.ts` selects `NodeRuntime.runMain` versus the Bun edge as a runtime row — the app body above it is identical.
- The HTTP client is undici, not `node:http`: `NodeHttpClient.layerUndici` binds `HttpClient.HttpClient` to an undici `Dispatcher` with connection pooling, keep-alive, and HTTP/2. `host/net/client.ts` layers the branch default policy (timeout, retry, proxy) on top through `HttpClient.retryTransient`/`.mapRequest`, so the pooling is a `Dispatcher` config and the policy is composed transformers — one client, both concerns.
- Server binding is `Config`-driven: `NodeHttpServer.layerConfig` reads host/port from the `ConfigProvider`, and `HttpApiBuilder.serve` (from `@effect/platform`) mounts the assembled `HttpApi` onto it. `edge/serve` selects `NodeHttpServer` versus `BunHttpServer` versus `toWebHandler` as a serve row — the served `HttpApi` value is runtime-agnostic.
- `NodeStream`/`NodeSink` are the Node-boundary bridges: `NodeStream.fromReadable` lifts a `node:stream.Readable` into an Effect `Stream` with backpressure, `NodeStream.stdin`/`stdout` expose process stdio as streams, and `NodeSink.fromWritable` drains a `Stream` into a `Writable`. These are the only place a raw Node stream is admitted; downstream stays on the Effect `Stream`/`Channel` rail.
- Cluster transports are node-only capability: `NodeClusterSocket.layerK8sHttpClient` discovers runner pods via the Kubernetes API (discovery, never provisioning — provisioning is `iac`), and `NodeClusterHttp`/`NodeClusterSocket` carry `@effect/cluster` entity messages. `work/engine` composes these behind the `MessageStorage`/`Sharding` Tags the app root satisfies with a `store` driver.

[STACKS_WITH]:
- `@effect/platform` (`.api/effect-platform.md`): this package is its runtime half — every `Node*` Layer satisfies a platform Tag. Domain code composes the abstract contract; the app root provides the Node Layer. There is no Node-specific domain API to learn.
- `effect` (`.api/effect.md`): `NodeRuntime.runMain` is the `Effect.runFork` edge; the Layers plug into the `Layer` graph; `NodeHttpServer.layerConfig` reads `Config` through the `ConfigProvider`. The Node tier adds bindings, never a new rail.
- `@effect/platform-bun` (`.api/effect-platform-bun.md`): the peer runtime — `BunContext.layer`, `BunHttpServer.layer`, `BunRuntime.runMain` satisfy the same platform Tags. `host/exec/runtime.ts` and `edge/serve` select node versus bun as a `Layer` row; a bun swap touches only the app root.
- `@effect/opentelemetry` (`.api/effect-opentelemetry.md`): the `NodeSdk` OTel Layer (from `@effect/opentelemetry/NodeSdk`) is provided beside `NodeContext.layer` to bind the `Tracer`/`MetricRegistry`; `telemetry/otlp/export.ts` owns the `NodeSdk` row.
- `@effect/cluster` + `@effect/sql` (catalogued at `libs/typescript/work|store/.api/`): `NodeClusterHttp`/`NodeClusterSocket` transport cluster messages, and the `MessageStorage` Tag is satisfied by a `store` `@effect/sql` driver Layer at the app root — the `work`/`store` seam meets at these bindings.

[LOCAL_ADMISSION]:
- Use the abstract `@effect/platform` Tag in domain code and provide `NodeContext.layer` (plus `NodeHttpClient.layerUndici`, `NodeHttpServer.layer`) only at the app composition root; never import `Node*` Layers into a folder's domain modules.
- Use `NodeRuntime.runMain` as the single node process edge; never `Effect.runPromise` at the top of a long-lived service (it does not install signal-draining teardown).
- Use `NodeHttpClient.layerUndici` for the `HttpClient` binding and tune the `Dispatcher`; never import `undici` directly except through the `Undici` re-export at a proven escape-hatch boundary.
- Use `NodeStream.fromReadable`/`NodeSink.fromWritable` to cross the Node-stream boundary; never consume a `node:stream` with raw `.on("data")` listeners — that bypasses backpressure and the `Effect` error channel.
- Use `NodeClusterSocket.layerK8sHttpClient` for runner discovery only; provisioning the cluster is `iac`, not a runtime import.

[RAIL_LAW]:
- Package: `@effect/platform-node`
- Owns: the Node bindings for every `@effect/platform` Tag — `NodeContext`/`NodeFileSystem`/`NodePath`/`NodeCommandExecutor`/`NodeTerminal`/`NodeWorker`/`NodeWorkerRunner`, `NodeHttpClient` (undici) + `Dispatcher`/`HttpAgent`, `NodeHttpServer`, `NodeSocket`/`NodeSocketServer` (ws), `NodeStream`/`NodeSink` bridges, `NodeKeyValueStore`, the `NodeCluster*` transports, `NodeRuntime.runMain`, and the raw `Undici` re-export
- Accept: `NodeContext.layer` + targeted `Node*` Layers at the app root, `NodeRuntime.runMain` as the process edge, `NodeHttpClient.layerUndici` with a tuned `Dispatcher`, `NodeStream`/`NodeSink` for stream boundaries, `NodeClusterSocket.layerK8sHttpClient` for discovery
- Reject: `Node*` Layers in domain modules, `Effect.runPromise` as a long-lived process edge, direct `undici`/`ws`/`node:stream` consumption in domain code, cluster provisioning through a runtime import
