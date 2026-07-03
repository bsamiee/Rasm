# [API_CATALOGUE] @effect/platform-node

`@effect/platform-node` is a barrel of 21 Node-runtime namespaces, each providing a Layer or constructor that satisfies an abstract `@effect/platform` service tag with a Node implementation. Six namespaces re-export their bulk surface from `@effect/platform-node-shared`.

---

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/platform-node`
- package: `@effect/platform-node`
- version: `0.107.0`
- license: `MIT`
- effect-peer: `effect ^3.21.x`, `@effect/platform ^0.96.x` (the abstract Tags every `Node*.layer` satisfies), `@effect/cluster ^0.59.x`, `@effect/rpc ^0.75.x`, `@effect/sql ^0.51.x` (the node bindings `NodeClusterSocket`/`NodeClusterHttp` provision) — `.api/effect-platform.md`, `.api/effect-cluster.md`, `.api/effect-rpc.md`, `.api/effect-sql.md`, `.api/effect.md`
- dep: `undici ^7.10.x` (the undici HTTP-client backend + the `Undici` barrel), `ws ^8.18.x` (WebSocket socket client/server), `mime ^3.x` (content-type resolution), `@effect/platform-node-shared ^0.60.x` (the bulk surface six namespaces re-export) — bundled, not peer
- runtime: node only — `worker_threads`/`child_process`/`node:http`/`node:net`/`node:tls`/`node:stream`; the bun tier is `@effect/platform-bun`, the browser tier `@effect/platform-browser`
- entry: `@effect/platform-node` (namespace barrel); each name is a module namespace object, no flat export
- modules: `NodeContext`, `NodeRuntime`, `NodeFileSystem`, `NodePath`, `NodeCommandExecutor`, `NodeTerminal`, `NodeKeyValueStore`, `NodeHttpClient`, `NodeHttpServer`, `NodeHttpPlatform`, `NodeHttpServerRequest`, `NodeSocket`, `NodeSocketServer`, `NodeStream`, `NodeSink`, `NodeMultipart`, `NodeWorker`, `NodeWorkerRunner`, `NodeClusterHttp`, `NodeClusterSocket`, `Undici`
- asset: per-runtime `Layer`/constructor bindings for the abstract `@effect/platform` Tags plus the node cluster transport layers
- rail: host / runtime boundary
- owner consumers: composition roots boot through `NodeRuntime.runMain` + `NodeContext.layer`; `RunnerBackplane` (`execution/backplane#RUNNER_AND_SCHEDULING` — the cluster rides `NodeClusterSocket.layer`/`NodeClusterHttp.layer`); `InternalRpc` (`messaging/rpc#INTERNAL_RPC` — the RPC server hosts on `NodeHttpServer`/`NodeSocketServer`); `McpTransport` (`agent/mcp#MCP_TRANSPORT` — `NodeStream.stdin`/`NodeSink.stdout` stdio pipe)

## [02]-[INDEX_BARREL]

```ts
// @effect/platform-node
export * as NodeClusterHttp from "./NodeClusterHttp.js"
export * as NodeClusterSocket from "./NodeClusterSocket.js"
export * as NodeCommandExecutor from "./NodeCommandExecutor.js"
export * as NodeContext from "./NodeContext.js"
export * as NodeFileSystem from "./NodeFileSystem.js"
export * as NodeHttpClient from "./NodeHttpClient.js"
export * as NodeHttpPlatform from "./NodeHttpPlatform.js"
export * as NodeHttpServer from "./NodeHttpServer.js"
export * as NodeHttpServerRequest from "./NodeHttpServerRequest.js"
export * as NodeKeyValueStore from "./NodeKeyValueStore.js"
export * as NodeMultipart from "./NodeMultipart.js"
export * as NodePath from "./NodePath.js"
export * as NodeRuntime from "./NodeRuntime.js"
export * as NodeSink from "./NodeSink.js"
export * as NodeSocket from "./NodeSocket.js"
export * as NodeSocketServer from "./NodeSocketServer.js"
export * as NodeStream from "./NodeStream.js"
export * as NodeTerminal from "./NodeTerminal.js"
export * as NodeWorker from "./NodeWorker.js"
export * as NodeWorkerRunner from "./NodeWorkerRunner.js"
export * as Undici from "./Undici.js"
```

Consumption is always namespaced: `import { NodeRuntime, NodeContext } from "@effect/platform-node"`. There is no flat top-level export; each name above is a module namespace object.

---

## [03]-[NODE_CONTEXT]

```ts
// @effect/platform-node/NodeContext
import type * as CommandExecutor from "@effect/platform/CommandExecutor"
import type * as FileSystem from "@effect/platform/FileSystem"
import type * as Path from "@effect/platform/Path"
import type * as Terminal from "@effect/platform/Terminal"
import type * as Worker from "@effect/platform/Worker"
import * as Layer from "effect/Layer"

export type NodeContext =
  | CommandExecutor.CommandExecutor
  | FileSystem.FileSystem
  | Path.Path
  | Terminal.Terminal
  | Worker.WorkerManager

export declare const layer: Layer.Layer<NodeContext>
```

The all-in-one Node platform layer: one `Layer.provide(NodeContext.layer)` satisfies `FileSystem`, `Path`, `CommandExecutor`, `Terminal`, and `WorkerManager` tags at once. `NodeContext` is the union of those five service tags (no error, no requirement).

---

## [04]-[NODE_RUNTIME]

```ts
// @effect/platform-node/NodeRuntime
import type { RunMain } from "@effect/platform/Runtime"

export declare const runMain: RunMain
```

`runMain` is the program boot. `RunMain` (defined in `@effect/platform/Runtime`) accepts an `Effect<unknown, unknown, never>` plus an options object `{ readonly disableErrorReporting?: boolean; readonly disablePrettyLogger?: boolean; readonly teardown?: Teardown }` and runs it as the process main: installs SIGINT/SIGTERM interruption, reports defects, and sets the exit code. This is the sole legal top-level `Effect` runner for a Node process.

---

## [05]-[OS_SERVICE_LAYERS]

```ts
// @effect/platform-node/NodeFileSystem
import type { FileSystem } from "@effect/platform/FileSystem"
import type { Layer } from "effect/Layer"

export declare const layer: Layer<FileSystem>
```

```ts
// @effect/platform-node/NodePath
import type { Path } from "@effect/platform/Path"
import type { Layer } from "effect/Layer"

export declare const layer: Layer<Path>
export declare const layerPosix: Layer<Path>
export declare const layerWin32: Layer<Path>
```

```ts
// @effect/platform-node/NodeCommandExecutor
import type { CommandExecutor } from "@effect/platform/CommandExecutor"
import type { FileSystem } from "@effect/platform/FileSystem"
import type { Layer } from "effect/Layer"

export declare const layer: Layer<CommandExecutor, never, FileSystem>
```

```ts
// @effect/platform-node/NodeTerminal
import type { Terminal, UserInput } from "@effect/platform/Terminal"
import type { Effect } from "effect/Effect"
import type { Layer } from "effect/Layer"
import type { Scope } from "effect/Scope"

export declare const make: (shouldQuit?: (input: UserInput) => boolean) => Effect<Terminal, never, Scope>
export declare const layer: Layer<Terminal>
```

`NodePath.layer` defaults to the OS-native separator; `layerPosix`/`layerWin32` pin the dialect. `NodeCommandExecutor.layer` requires `FileSystem` in its `R` channel — it is layered on top of `NodeFileSystem.layer`. `NodeTerminal.make` takes an optional quit predicate over `UserInput` and produces a scoped `Terminal`.

---

## [06]-[NODE_KEY_VALUE_STORE]

```ts
// @effect/platform-node/NodeKeyValueStore
import type * as PlatformError from "@effect/platform/Error"
import type * as KeyValueStore from "@effect/platform/KeyValueStore"
import type * as Layer from "effect/Layer"

export declare const layerFileSystem: (directory: string) =>
  Layer.Layer<KeyValueStore.KeyValueStore, PlatformError.PlatformError>
```

The only constructor: a directory-rooted `KeyValueStore` whose layer can fail with `PlatformError` (directory access). No in-memory or schema variant in this package — those live in `@effect/platform/KeyValueStore` itself.

---

## [07]-[NODE_HTTP_CLIENT]

```ts
// @effect/platform-node/NodeHttpClient
import type * as Client from "@effect/platform/HttpClient"
import * as Context from "effect/Context"
import type * as Effect from "effect/Effect"
import type * as Layer from "effect/Layer"
import type * as Scope from "effect/Scope"
import type * as Http from "node:http"
import type * as Https from "node:https"
import type * as Undici from "./Undici.js"

export declare const HttpAgentTypeId: unique symbol
export type HttpAgentTypeId = typeof HttpAgentTypeId

export interface HttpAgent {
  readonly [HttpAgentTypeId]: typeof HttpAgentTypeId
  readonly http: Http.Agent
  readonly https: Https.Agent
}

export declare const HttpAgent: Context.Tag<HttpAgent, HttpAgent>
export declare const makeAgent: (options?: Https.AgentOptions) => Effect.Effect<HttpAgent, never, Scope.Scope>
export declare const agentLayer: Layer.Layer<HttpAgent>
export declare const makeAgentLayer: (options?: Https.AgentOptions) => Layer.Layer<HttpAgent>

export declare const make: Effect.Effect<Client.HttpClient, never, HttpAgent>
export declare const layer: Layer.Layer<Client.HttpClient>
export declare const layerWithoutAgent: Layer.Layer<Client.HttpClient, never, HttpAgent>

export interface Dispatcher {
  readonly _: unique symbol
}
export declare const Dispatcher: Context.Tag<Dispatcher, Undici.Dispatcher>
export declare const makeDispatcher: Effect.Effect<Undici.Dispatcher, never, Scope.Scope>
export declare const dispatcherLayer: Layer.Layer<Dispatcher>
export declare const dispatcherLayerGlobal: Layer.Layer<Dispatcher>

export declare class UndiciRequestOptions extends Context.TagClass<
  UndiciRequestOptions,
  "@effect/platform-node/NodeHttpClient/undiciOptions",
  Undici.Dispatcher.RequestOptions<null>
>() {}

export declare const makeUndici: (dispatcher: Undici.Dispatcher) => Client.HttpClient
export declare const layerUndici: Layer.Layer<Client.HttpClient>
export declare const layerUndiciWithoutDispatcher: Layer.Layer<Client.HttpClient, never, Dispatcher>
```

Two client backends. The `node:http`/`node:https` agent backend: `layer` is self-contained (agent built in), `layerWithoutAgent` requires an `HttpAgent` tag supplied via `agentLayer`/`makeAgentLayer`. The undici backend: `layerUndici` is self-contained, `layerUndiciWithoutDispatcher` requires the `Dispatcher` tag from `dispatcherLayer` (scoped) or `dispatcherLayerGlobal` (process-global undici dispatcher). `UndiciRequestOptions` is a `Context.TagClass` carrying per-request undici `RequestOptions` for fiber-local override. `make`/`makeUndici` are the raw constructors behind the layers.

---

## [08]-[NODE_HTTP_SERVER]

```ts
// @effect/platform-node/NodeHttpServer
import type * as Etag from "@effect/platform/Etag"
import type * as App from "@effect/platform/HttpApp"
import type * as HttpClient from "@effect/platform/HttpClient"
import type * as Middleware from "@effect/platform/HttpMiddleware"
import type * as Platform from "@effect/platform/HttpPlatform"
import type * as Server from "@effect/platform/HttpServer"
import type { ServeError } from "@effect/platform/HttpServerError"
import type * as ServerRequest from "@effect/platform/HttpServerRequest"
import type * as Config from "effect/Config"
import type * as ConfigError from "effect/ConfigError"
import type * as Effect from "effect/Effect"
import type { LazyArg } from "effect/Function"
import type * as Layer from "effect/Layer"
import type * as Scope from "effect/Scope"
import type * as Http from "node:http"
import type * as Net from "node:net"
import type * as NodeContext from "./NodeContext.js"

export declare const make: (
  evaluate: LazyArg<Http.Server<typeof Http.IncomingMessage, typeof Http.ServerResponse>>,
  options: Net.ListenOptions,
) => Effect.Effect<Server.HttpServer, ServeError, Scope.Scope>

export declare const makeHandler: {
  <R, E>(httpApp: App.Default<E, R>): Effect.Effect<
    (nodeRequest: Http.IncomingMessage, nodeResponse: Http.ServerResponse) => void,
    never,
    Exclude<R, ServerRequest.HttpServerRequest | Scope.Scope>
  >
  <R, E, App extends App.Default<any, any>>(
    httpApp: App.Default<E, R>,
    middleware: Middleware.HttpMiddleware.Applied<App, E, R>,
  ): Effect.Effect<
    (nodeRequest: Http.IncomingMessage, nodeResponse: Http.ServerResponse) => void,
    never,
    Exclude<Effect.Effect.Context<App>, ServerRequest.HttpServerRequest | Scope.Scope>
  >
}

export declare const layerServer: (
  evaluate: LazyArg<Http.Server>,
  options: Net.ListenOptions,
) => Layer.Layer<Server.HttpServer, ServeError>

export declare const layer: (
  evaluate: LazyArg<Http.Server<typeof Http.IncomingMessage, typeof Http.ServerResponse>>,
  options: Net.ListenOptions,
) => Layer.Layer<Platform.HttpPlatform | Etag.Generator | NodeContext.NodeContext | Server.HttpServer, ServeError>

export declare const layerConfig: (
  evaluate: LazyArg<Http.Server<typeof Http.IncomingMessage, typeof Http.ServerResponse>>,
  options: Config.Config.Wrap<Net.ListenOptions>,
) => Layer.Layer<
  Platform.HttpPlatform | Etag.Generator | NodeContext.NodeContext | Server.HttpServer,
  ConfigError.ConfigError | ServeError
>

export declare const layerTest: Layer.Layer<
  HttpClient.HttpClient | Server.HttpServer | Platform.HttpPlatform | Etag.Generator | NodeContext.NodeContext,
  ServeError
>

export declare const layerContext: Layer.Layer<Platform.HttpPlatform | Etag.Generator | NodeContext.NodeContext>
```

```ts
// @effect/platform-node/NodeHttpPlatform
import type * as Etag from "@effect/platform/Etag"
import type * as FileSystem from "@effect/platform/FileSystem"
import type * as Platform from "@effect/platform/HttpPlatform"
import type * as Effect from "effect/Effect"
import type * as Layer from "effect/Layer"

export declare const make: Effect.Effect<Platform.HttpPlatform, never, FileSystem.FileSystem | Etag.Generator>
export declare const layer: Layer.Layer<Platform.HttpPlatform>
```

```ts
// @effect/platform-node/NodeHttpServerRequest
import type * as ServerRequest from "@effect/platform/HttpServerRequest"
import type * as Http from "node:http"

export declare const toIncomingMessage: (self: ServerRequest.HttpServerRequest) => Http.IncomingMessage
export declare const toServerResponse: (self: ServerRequest.HttpServerRequest) => Http.ServerResponse
```

`NodeHttpServer.layer` is the production server layer: it folds in `HttpPlatform`, `Etag.Generator`, `NodeContext`, and `HttpServer` and can fail with `ServeError` (bind failure). `layerConfig` takes a `Config.Config.Wrap<Net.ListenOptions>` so listen options resolve from the Effect config provider, adding `ConfigError` to the error channel. `layerTest` binds a server on a random port and also provides an `HttpClient` prefixed with the running server URL — the in-process integration-test layer. `layerContext` provides platform/etag/context without binding a socket. `make` returns a scoped `HttpServer`; `makeHandler` adapts an `HttpApp.Default` into a raw Node `(req, res) => void` request listener, with a two-arity overload threading applied `HttpMiddleware`. `NodeHttpServerRequest.toIncomingMessage`/`toServerResponse` are the escape hatches from the abstract `HttpServerRequest` down to the raw Node objects.

---

## [09]-[NODE_SOCKET]

```ts
// @effect/platform-node/NodeSocket
//   locally declared:
import * as Socket from "@effect/platform/Socket"
import * as Layer from "effect/Layer"

export * from "@effect/platform-node-shared/NodeSocket"

export declare const layerWebSocket: (
  url: string,
  options?: { readonly closeCodeIsError?: (code: number) => boolean },
) => Layer.Layer<Socket.Socket>

export declare const layerWebSocketConstructor: Layer.Layer<Socket.WebSocketConstructor>

//   re-exported verbatim from @effect/platform-node-shared/NodeSocket:
import * as Channel from "effect/Channel"
import type * as Chunk from "effect/Chunk"
import * as Context from "effect/Context"
import type * as Duration from "effect/Duration"
import * as Effect from "effect/Effect"
import * as Scope from "effect/Scope"
import * as Net from "node:net"
import type { Duplex } from "node:stream"

export interface NetSocket { readonly _: unique symbol }
export declare const NetSocket: Context.Tag<NetSocket, Net.Socket>

export declare const makeNet: (
  options: Net.NetConnectOpts & { readonly openTimeout?: Duration.DurationInput | undefined },
) => Effect.Effect<Socket.Socket, Socket.SocketError>

export declare const fromDuplex: <RO>(
  open: Effect.Effect<Duplex, Socket.SocketError, RO>,
  options?: { readonly openTimeout?: Duration.DurationInput | undefined },
) => Effect.Effect<Socket.Socket, never, Exclude<RO, Scope.Scope>>

export declare const makeNetChannel: <IE = never>(
  options: Net.NetConnectOpts,
) => Channel.Channel<
  Chunk.Chunk<Uint8Array>,
  Chunk.Chunk<Uint8Array | string | Socket.CloseEvent>,
  Socket.SocketError | IE,
  IE,
  void,
  unknown
>

export declare const layerNet: (options: Net.NetConnectOpts) => Layer.Layer<Socket.Socket, Socket.SocketError>
```

```ts
// @effect/platform-node/NodeSocketServer
//   re-exported verbatim from @effect/platform-node-shared/NodeSocketServer:
import * as Socket from "@effect/platform/Socket"
import * as SocketServer from "@effect/platform/SocketServer"
import * as Context from "effect/Context"
import * as Effect from "effect/Effect"
import * as Layer from "effect/Layer"
import * as Scope from "effect/Scope"
import type * as Http from "node:http"
import * as Net from "node:net"
import * as WS from "ws"

export declare class IncomingMessage extends Context.TagClass<
  IncomingMessage,
  "@effect/platform-node-shared/NodeSocketServer/IncomingMessage",
  Http.IncomingMessage
>() {}

export declare const make: (
  options: Net.ServerOpts & Net.ListenOptions,
) => Effect.Effect<
  {
    readonly address: SocketServer.Address
    readonly run: <R, E, _>(
      handler: (socket: Socket.Socket) => Effect.Effect<_, E, R>,
    ) => Effect.Effect<never, SocketServer.SocketServerError, R>
  },
  SocketServer.SocketServerError,
  Scope.Scope
>

export declare const layer: (
  options: Net.ServerOpts & Net.ListenOptions,
) => Layer.Layer<SocketServer.SocketServer, SocketServer.SocketServerError>

export declare const makeWebSocket: (
  options: WS.ServerOptions<typeof WS.WebSocket, typeof Http.IncomingMessage>,
) => Effect.Effect<SocketServer.SocketServer["Type"], SocketServer.SocketServerError, Scope.Scope>

export declare const layerWebSocket: (
  options: WS.ServerOptions,
) => Layer.Layer<SocketServer.SocketServer, SocketServer.SocketServerError>
```

`NodeSocket` locally declares the WebSocket-client surface: `layerWebSocket` provides a `Socket.Socket` bound to a `ws://`/`wss://` URL with an optional predicate deciding which close codes count as errors; `layerWebSocketConstructor` provides the `WebSocketConstructor` tag (the Node `ws` constructor) for code that builds its own sockets. From shared it re-exports the TCP-client surface: `makeNet` (effect-level TCP `Socket` from `NetConnectOpts` + optional `openTimeout`), `makeNetChannel` (the same as a duplex `Channel`), `fromDuplex` (wrap any `node:stream` `Duplex` as a `Socket`), `layerNet` (TCP `Socket` as a `Layer`), and the `NetSocket` tag carrying the raw `Net.Socket`. `NodeSocketServer` is a pure shared re-export: `make`/`layer` bind a raw-TCP `SocketServer`, `makeWebSocket`/`layerWebSocket` bind a `ws`-backed `SocketServer`, and `IncomingMessage` is the `Context.TagClass` exposing the raw `Http.IncomingMessage` to socket handlers. Note both `NodeSocket` and `NodeSocketServer` export a `layerWebSocket`, distinct per namespace (client vs server).

---

## [10]-[NODE_STREAM_AND_SINK]

```ts
// @effect/platform-node/NodeStream  (pure re-export from @effect/platform-node-shared/NodeStream)
import type { PlatformError } from "@effect/platform/Error"
import type { SizeInput } from "@effect/platform/FileSystem"
import type { Channel } from "effect/Channel"
import type { Chunk } from "effect/Chunk"
import type { Effect } from "effect/Effect"
import type { LazyArg } from "effect/Function"
import * as Stream from "effect/Stream"
import type { Duplex, Readable } from "node:stream"

export interface FromReadableOptions {
  readonly chunkSize?: SizeInput
  readonly closeOnDone?: boolean | undefined
}
export interface FromWritableOptions {
  readonly endOnDone?: boolean
  readonly encoding?: BufferEncoding
}

export declare const fromReadable: <E, A = Uint8Array<ArrayBufferLike>>(
  evaluate: LazyArg<Readable | NodeJS.ReadableStream>,
  onError: (error: unknown) => E,
  options?: FromReadableOptions,
) => Stream.Stream<A, E>

export declare const fromReadableChannel: <E, A = Uint8Array<ArrayBufferLike>>(
  evaluate: LazyArg<Readable | NodeJS.ReadableStream>,
  onError: (error: unknown) => E,
  options?: FromReadableOptions | undefined,
) => Channel<Chunk<A>, unknown, E>

export declare const fromDuplex: <IE, E, I = Uint8Array, O = Uint8Array>(
  evaluate: LazyArg<Duplex>,
  onError: (error: unknown) => E,
  options?: FromReadableOptions & FromWritableOptions,
) => Channel<Chunk<O>, Chunk<I>, IE | E, IE, void, unknown>

export declare const pipeThroughDuplex: {
  <E2, B = Uint8Array>(
    duplex: LazyArg<Duplex>,
    onError: (error: unknown) => E2,
    options?: (FromReadableOptions & FromWritableOptions) | undefined,
  ): <R, E, A>(self: Stream.Stream<A, E, R>) => Stream.Stream<B, E2 | E, R>
  <R, E, A, E2, B = Uint8Array>(
    self: Stream.Stream<A, E, R>,
    duplex: LazyArg<Duplex>,
    onError: (error: unknown) => E2,
    options?: (FromReadableOptions & FromWritableOptions) | undefined,
  ): Stream.Stream<B, E | E2, R>
}

export declare const pipeThroughSimple: {
  (duplex: LazyArg<Duplex>): <R, E>(
    self: Stream.Stream<string | Uint8Array, E, R>,
  ) => Stream.Stream<Uint8Array, E | PlatformError, R>
  <R, E>(
    self: Stream.Stream<string | Uint8Array, E, R>,
    duplex: LazyArg<Duplex>,
  ): Stream.Stream<Uint8Array, PlatformError | E, R>
}

export declare const toReadable: <E, R>(
  stream: Stream.Stream<string | Uint8Array, E, R>,
) => Effect<Readable, never, R>
export declare const toReadableNever: <E>(stream: Stream.Stream<string | Uint8Array, E, never>) => Readable

export declare const toString: <E>(
  readable: LazyArg<Readable | NodeJS.ReadableStream>,
  options: {
    readonly onFailure: (error: unknown) => E
    readonly encoding?: BufferEncoding | undefined
    readonly maxBytes?: SizeInput | undefined
  },
) => Effect<string, E>

export declare const toUint8Array: <E>(
  readable: LazyArg<Readable | NodeJS.ReadableStream>,
  options: { readonly onFailure: (error: unknown) => E; readonly maxBytes?: SizeInput | undefined },
) => Effect<Uint8Array, E>

export declare const stdin: Stream.Stream<Uint8Array>
export declare const stdout: Stream.Stream<Uint8Array>
export declare const stderr: Stream.Stream<Uint8Array>
```

```ts
// @effect/platform-node/NodeSink  (pure re-export from @effect/platform-node-shared/NodeSink)
import type { PlatformError } from "@effect/platform/Error"
import type { Channel } from "effect/Channel"
import type { Chunk } from "effect/Chunk"
import type { LazyArg } from "effect/Function"
import type * as Sink from "effect/Sink"
import type { Writable } from "stream"
import type { FromWritableOptions } from "./NodeStream.js"

export declare const fromWritable: <E, A = string | Uint8Array>(
  evaluate: LazyArg<Writable | NodeJS.WritableStream>,
  onError: (error: unknown) => E,
  options?: FromWritableOptions,
) => Sink.Sink<void, A, never, E>

export declare const fromWritableChannel: <IE, OE, A>(
  writable: LazyArg<Writable | NodeJS.WritableStream>,
  onError: (error: unknown) => OE,
  options?: FromWritableOptions,
) => Channel<Chunk<never>, Chunk<A>, IE | OE, IE, void, unknown>

export declare const stdout: Sink.Sink<void, string | Uint8Array, never, PlatformError>
export declare const stderr: Sink.Sink<void, string | Uint8Array, never, PlatformError>
export declare const stdin: Sink.Sink<void, string | Uint8Array, never, PlatformError>
```

Pure shared re-export barrels. `NodeStream` bridges Node `Readable`/`Duplex` into Effect `Stream`/`Channel`: `fromReadable` (chunk size + close policy via `FromReadableOptions`), `fromReadableChannel` (channel form), `fromDuplex` (bidirectional duplex channel), `pipeThroughDuplex`/`pipeThroughSimple` (data-first + data-last duplex pipelines, `pipeThroughSimple` widening the error channel with `PlatformError`), `toReadable`/`toReadableNever` (Effect `Stream` back to a Node `Readable`, scoped vs `never`-requirement), and the consuming sinks `toString`/`toUint8Array` (drain a readable with a failure mapper and optional `maxBytes` cap). `stdin`/`stdout`/`stderr` are the pre-bound process streams. `NodeSink` bridges Node `Writable` into Effect `Sink`: `fromWritable` (with `FromWritableOptions` end/encoding policy), `fromWritableChannel` (channel form), and `stdin`/`stdout`/`stderr` as `PlatformError`-failing process sinks.

---

## [11]-[NODE_MULTIPART]

```ts
// @effect/platform-node/NodeMultipart  (pure re-export from @effect/platform-node-shared/NodeMultipart)
import type * as FileSystem from "@effect/platform/FileSystem"
import type * as Multipart from "@effect/platform/Multipart"
import type * as Path from "@effect/platform/Path"
import type * as Effect from "effect/Effect"
import type * as Scope from "effect/Scope"
import type * as Stream from "effect/Stream"
import type { IncomingHttpHeaders } from "node:http"
import type { Readable } from "node:stream"

export declare const stream: (
  source: Readable,
  headers: IncomingHttpHeaders,
) => Stream.Stream<Multipart.Part, Multipart.MultipartError>

export declare const persisted: (
  source: Readable,
  headers: IncomingHttpHeaders,
) => Effect.Effect<Multipart.Persisted, Multipart.MultipartError, FileSystem.FileSystem | Path.Path | Scope.Scope>

export declare const fileToReadable: (file: Multipart.File) => Readable
```

Pure shared re-export barrel. `stream` parses a Node `Readable` + `IncomingHttpHeaders` into a `Stream` of `Multipart.Part` (failing with `Multipart.MultipartError`); `persisted` parses to a `Multipart.Persisted` snapshot, spilling file parts to disk and therefore requiring `FileSystem | Path | Scope`. `fileToReadable` converts a parsed `Multipart.File` back into a Node `Readable`. The `MultipartError` rail and `Part`/`Persisted`/`File` shapes are owned by `@effect/platform/Multipart`.

---

## [12]-[NODE_WORKER]

```ts
// @effect/platform-node/NodeWorker
import type * as Worker from "@effect/platform/Worker"
import type * as Layer from "effect/Layer"
import type * as ChildProcess from "node:child_process"
import type * as WorkerThreads from "node:worker_threads"

export declare const layerManager: Layer.Layer<Worker.WorkerManager>
export declare const layerWorker: Layer.Layer<Worker.PlatformWorker>
export declare const layer: (
  spawn: (id: number) => WorkerThreads.Worker | ChildProcess.ChildProcess,
) => Layer.Layer<Worker.WorkerManager | Worker.Spawner>
export declare const layerPlatform: (
  spawn: (id: number) => WorkerThreads.Worker | ChildProcess.ChildProcess,
) => Layer.Layer<Worker.PlatformWorker | Worker.Spawner>
```

```ts
// @effect/platform-node/NodeWorkerRunner
import type * as Runner from "@effect/platform/WorkerRunner"
import type * as Layer from "effect/Layer"

export { launch } from "@effect/platform/WorkerRunner"

export declare const layer: Layer.Layer<Runner.PlatformRunner>
```

Host (main-thread) side: `NodeWorker.layer(spawn)` provides `WorkerManager + Spawner` where `spawn` maps a worker id to either a `worker_threads.Worker` or a `child_process.ChildProcess` (both pooling targets supported). `layerManager`/`layerWorker` are the pre-split halves. Worker (spawned-process) side: `NodeWorkerRunner.layer` provides the `PlatformRunner` and `launch` (re-exported from `@effect/platform/WorkerRunner`) boots the runner inside the worker.

---

## [13]-[NODE_CLUSTER]

```ts
// @effect/platform-node/NodeClusterSocket
import * as K8sHttpClient from "@effect/cluster/K8sHttpClient"
import * as MessageStorage from "@effect/cluster/MessageStorage"
import * as Runners from "@effect/cluster/Runners"
import * as RunnerStorage from "@effect/cluster/RunnerStorage"
import type { Sharding } from "@effect/cluster/Sharding"
import * as ShardingConfig from "@effect/cluster/ShardingConfig"
import { layerClientProtocol, layerSocketServer } from "@effect/platform-node-shared/NodeClusterSocket"
import type * as SocketServer from "@effect/platform/SocketServer"
import type { SqlClient } from "@effect/sql/SqlClient"
import type { ConfigError } from "effect/ConfigError"
import * as Layer from "effect/Layer"
import * as NodeHttpClient from "./NodeHttpClient.js"

// re-exported verbatim from @effect/platform-node-shared/NodeClusterSocket:
//   layerClientProtocol: Layer.Layer<Runners.RpcClientProtocol, never, RpcSerialization.RpcSerialization>
//   layerSocketServer:   Layer.Layer<SocketServer.SocketServer, SocketServer.SocketServerError, ShardingConfig.ShardingConfig>
export { layerClientProtocol, layerSocketServer }

export declare const layer: <
  const ClientOnly extends boolean = false,
  const Storage extends "local" | "sql" | "byo" = never,
  const Health extends "ping" | "k8s" = never,
>(options?: {
  readonly serialization?: "msgpack" | "ndjson" | undefined
  readonly clientOnly?: ClientOnly | undefined
  readonly storage?: Storage | undefined
  readonly runnerHealth?: Health | undefined
  readonly runnerHealthK8s?: { readonly namespace?: string | undefined; readonly labelSelector?: string | undefined } | undefined
  readonly shardingConfig?: Partial<ShardingConfig.ShardingConfig["Type"]> | undefined
}) => ClientOnly extends true
  ? Layer.Layer<
      Sharding | Runners.Runners | ("byo" extends Storage ? never : MessageStorage.MessageStorage),
      ConfigError,
      "local" extends Storage ? never : "byo" extends Storage ? (MessageStorage.MessageStorage | RunnerStorage.RunnerStorage) : SqlClient
    >
  : Layer.Layer<
      Sharding | Runners.Runners | ("byo" extends Storage ? never : MessageStorage.MessageStorage),
      SocketServer.SocketServerError | ConfigError,
      "local" extends Storage ? never : "byo" extends Storage ? (MessageStorage.MessageStorage | RunnerStorage.RunnerStorage) : SqlClient
    >

export declare const layerDispatcherK8s: Layer.Layer<NodeHttpClient.Dispatcher>
export declare const layerK8sHttpClient: Layer.Layer<K8sHttpClient.K8sHttpClient>
```

```ts
// @effect/platform-node/NodeClusterHttp
import * as MessageStorage from "@effect/cluster/MessageStorage"
import * as Runners from "@effect/cluster/Runners"
import * as RunnerStorage from "@effect/cluster/RunnerStorage"
import type { Sharding } from "@effect/cluster/Sharding"
import * as ShardingConfig from "@effect/cluster/ShardingConfig"
import type * as Etag from "@effect/platform/Etag"
import type { HttpPlatform } from "@effect/platform/HttpPlatform"
import type { HttpServer } from "@effect/platform/HttpServer"
import type { ServeError } from "@effect/platform/HttpServerError"
import type { SqlClient } from "@effect/sql/SqlClient"
import type { ConfigError } from "effect/ConfigError"
import * as Layer from "effect/Layer"
import type { NodeContext } from "./NodeContext.js"

export { layerK8sHttpClient } from "./NodeClusterSocket.js"

export declare const layer: <
  const ClientOnly extends boolean = false,
  const Storage extends "local" | "sql" | "byo" = never,
  const Health extends "ping" | "k8s" = never,
>(options: {
  readonly transport: "http" | "websocket"
  readonly serialization?: "msgpack" | "ndjson" | undefined
  readonly clientOnly?: ClientOnly | undefined
  readonly storage?: Storage | undefined
  readonly runnerHealth?: Health | undefined
  readonly runnerHealthK8s?: { readonly namespace?: string | undefined; readonly labelSelector?: string | undefined } | undefined
  readonly shardingConfig?: Partial<ShardingConfig.ShardingConfig["Type"]> | undefined
}) => ClientOnly extends true
  ? Layer.Layer<
      Sharding | Runners.Runners | ("byo" extends Storage ? never : MessageStorage.MessageStorage),
      ConfigError,
      "local" extends Storage ? never : "byo" extends Storage ? (MessageStorage.MessageStorage | RunnerStorage.RunnerStorage) : SqlClient
    >
  : Layer.Layer<
      Sharding | Runners.Runners | ("byo" extends Storage ? never : MessageStorage.MessageStorage),
      ServeError | ConfigError,
      "local" extends Storage ? never : "byo" extends Storage ? (MessageStorage.MessageStorage | RunnerStorage.RunnerStorage) : SqlClient
    >

export declare const layerHttpServer: Layer.Layer<
  HttpPlatform | Etag.Generator | NodeContext | HttpServer,
  ServeError,
  ShardingConfig.ShardingConfig
>
```

Both `layer` functions are conditional-typed over three const type parameters: `ClientOnly` (drops the server error channel), `Storage` (`"local"` requires nothing, `"sql"` requires `SqlClient`, `"byo"` requires `MessageStorage + RunnerStorage`), and `Health` (`"ping"` vs `"k8s"`). The HTTP variant additionally requires `transport: "http" | "websocket"` and uses `ServeError`; the socket variant defaults transport and uses `SocketServer.SocketServerError`. `layerDispatcherK8s`/`layerK8sHttpClient` supply the Kubernetes service-discovery client; `layerClientProtocol` (provides `Runners.RpcClientProtocol`, requires `RpcSerialization.RpcSerialization`) and `layerSocketServer` (provides `SocketServer.SocketServer`, fails `SocketServerError`, requires `ShardingConfig.ShardingConfig`) re-export the wire protocol layers from shared. `layerHttpServer` is the shard runner's own HTTP listen layer requiring `ShardingConfig`.

---

## [14]-[UNDICI]

```ts
// @effect/platform-node/Undici
export { default } from "undici"
export * from "undici"
```

Full pass-through of the `undici` package surface (`fetch`, `Dispatcher`, `Agent`, `Pool`, `Client`, `request`, `RequestOptions`, etc.). Consumed by `NodeHttpClient` for the undici backend; `Undici.Dispatcher` and `Undici.Dispatcher.RequestOptions<null>` are the types threaded into `NodeHttpClient.Dispatcher`/`UndiciRequestOptions`. Exact undici signatures are owned by the `undici` catalogue, not this page.

---

## [15]-[INTEGRATION_LAW]

[NODE_TOPOLOGY]:
- Binding, not contract: every export is a `Layer` (or a factory returning one) that satisfies an
  abstract `@effect/platform` `Context.Tag` on Node (`.api/effect-platform.md`). Domain code composes the
  abstract Tag; the composition root provides the `Node*` Layer. There is no node-specific domain API —
  swapping `@effect/platform-bun` (`.api/effect-platform-bun.md`: `BunContext`/`BunHttpServer`/`BunRuntime`)
  or `@effect/platform-browser` behind identical Tags touches only the root row.
- Boot pair: `NodeRuntime.runMain(effect, { disableErrorReporting?, disablePrettyLogger?, teardown? })` is
  the sole legal process edge — it forks the root fiber (`.api/effect.md` `Effect.runFork`), installs
  SIGINT/SIGTERM interruption so `acquireRelease`/`Layer.scoped` finalizers drain, and sets the exit code
  from the `Cause`. Never `Effect.runPromise` at the top of a long-lived service. `NodeContext.layer` is
  the aggregate binding (`FileSystem`+`Path`+`CommandExecutor`+`Terminal`+`WorkerManager`) provided once
  beneath every node service; single-contract folders take a targeted `Node*.layer` instead of the aggregate.
- Stream-boundary law: `NodeStream.fromReadable`/`NodeSink.fromWritable`/`NodeMultipart.stream` are the
  ONLY admitted raw-`node:stream` crossings — downstream stays on the Effect `Stream`/`Channel` rail with
  backpressure and the typed error channel; `NodeHttpServerRequest.toIncomingMessage`/`toServerResponse`
  are the raw-object escape hatches (protocol upgrade, manual streaming) at the boundary only.

[STACKING]:
- RPC transports resolve their requirement channel against these node bindings (`.api/effect-rpc.md`):
  client `RpcClient.layerProtocolHttp` needs `HttpClient.HttpClient` ← `NodeHttpClient.layerUndici`;
  `layerProtocolSocket` needs `Socket.Socket` ← `NodeSocket.layerNet`/`layerWebSocket`;
  `layerProtocolWorker` needs `Worker.PlatformWorker`+`Worker.Spawner` ← `NodeWorker.layerPlatform(spawn)`.
  Server `RpcServer.layerProtocolHttpRouter` needs `HttpLayerRouter.HttpRouter` served by
  `NodeHttpServer.layer`; `layerProtocolSocketServer` needs `SocketServer.SocketServer` ←
  `NodeSocketServer.layer`; `layerProtocolWorkerRunner` needs `WorkerRunner.PlatformRunner` ←
  `NodeWorkerRunner.layer`. `InternalRpc` (`messaging/rpc#INTERNAL_RPC`) wires one client+server pair.
- Stdio pipe: `NodeStream.stdin` (`Stream<Uint8Array>`) + `NodeSink.stdout` (`Sink`) feed
  `RpcServer.makeProtocolStdio({ stdin, stdout })` — the `McpTransport` (`agent/mcp#MCP_TRANSPORT`) node
  stdio transport, and `RpcWorker.InitialMessage` is the handshake the worker pool sends once at spawn.
- Cluster ⇄ SQL: `NodeClusterSocket.layer({ storage: "sql" })` / `NodeClusterHttp.layer({ transport,
  storage: "sql" })` provision `Sharding`+`Runners`+`MessageStorage` (`.api/effect-cluster.md`) whose
  `SqlClient` requirement (the `"sql"` storage arm; `"local"` needs nothing, `"byo"` needs
  `MessageStorage`+`RunnerStorage`) is discharged by the one `PgClient` Layer (`.api/effect-sql-pg.md`,
  `.api/effect-sql.md`). `RunnerBackplane` (`execution/backplane#RUNNER_AND_SCHEDULING`) rides these two
  layers; `layerDispatcherK8s`/`layerK8sHttpClient` are pod-DISCOVERY only — cluster provisioning is `iac`,
  never a runtime import.
- Telemetry: the `@effect/opentelemetry` `NodeSdk` Layer (`.api/effect-opentelemetry.md`) is provided beside
  `NodeContext.layer` to bind `Tracer`/`MetricRegistry`; `spanAttributes` threaded through the
  `NodeHttpClient`/`NodeHttpServer` bindings and the rpc/sql layers feed the same OTLP trace tree.
- HTTP one-client-both-concerns: `NodeHttpClient.layerUndici` binds `HttpClient.HttpClient` to a tuned
  undici `Dispatcher` (pooling, keep-alive, HTTP/2 via `dispatcherLayer`/`makeDispatcher`/
  `UndiciRequestOptions`); branch retry/timeout/proxy policy composes ON TOP as `HttpClient` transformers
  (`.api/effect-platform.md` `retryTransient`/`mapRequest`) — the pool is `Dispatcher` config, the policy
  is composed transformers, one client.

[RAIL_LAW]:
- Rail: host / runtime boundary; node-only tier (`worker_threads`/`child_process`/`node:http`/`node:net`/
  `node:tls`/`node:stream`). The bun tier is `@effect/platform-bun`, the browser tier `@effect/platform-browser`.
- Owns: the Node binding for every `@effect/platform` Tag plus `NodeRuntime.runMain`, the `NodeCluster*`
  transports, and the raw `Undici` re-export. Authors no contract of its own — the runtime half of the
  platform tier, swappable behind identical Tags. `@effect/cluster`/`@effect/rpc`/`@effect/sql` are hard
  peers (not optional) that back only the `NodeCluster*` bindings.
- Reject: `Node*` Layers in domain modules; `Effect.runPromise` as a long-lived process edge; direct
  `undici`/`ws`/`node:stream` consumption outside the `Undici` re-export and the `NodeStream`/`NodeSink`
  bridges; cluster provisioning through a runtime import.


