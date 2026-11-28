# [RPC]

## [01]-[GROUPS]

One `RpcGroup` of schema-typed procedures is the contract servers, clients, protocols, and `AtomRpc` services derive from:

```ts
const Failure = Schema.Union([Unmatched, Schema.ErrorInstance()]);
const ApplicationRpcs = RpcGroup.make(
    Rpc.make('events', { payload: { identifiers: Schema.String.pipe(Schema.NonEmptyArray, Schema.optionalKey) }, success: Event, stream: true }),
    Rpc.make('pressure', { success: Schema.Result(Pressure, Schema.ErrorInstance()), stream: true }),
    Rpc.make('act', { payload: { instance: ProcessInstance, ...Act.fields }, success: Act.Result, error: Failure }),
);
```

`Unmatched` is the lookup package's error, and `Event`, `Pressure`, `ProcessInstance`, and `Act` are schemas of the contract a Swift peer serves:
- `payload` fields build a `Schema.Struct`, and a spread of a class's `fields` joins its members to the request's own
- `stream: true` streams `success` and moves `error` into the stream, so a failure ends the subscription and a source whose reads fail and recover streams `Schema.Result` elements
- `Schema.ErrorInstance()` holds a peer's error as `message`, `name`, and `cause` where no package error names it, beside package errors a caller branches on
- `defect` sets the schema a defect crosses the wire through, `Schema.Defect()` by default, and `primaryKey` on a fields payload makes the payload a class with a `PrimaryKey`
- `.add`, `.merge`, `.prefix`, and `.omit` extend, join, rename, or narrow a group, and `.middleware(M)` reaches the procedures the group holds when called
- `group.requests` maps each tag to its `Rpc`, and a peer announces the procedures it serves from `requests.keys()`
- `Rpc.setPayload`, `setSuccess`, and `setError` derive a procedure from a protocol's own (`McpSchema.CallTool.setPayload(...)` over its `payloadSchema.fields`)
- Groups another language's peer serves are its wire, both ends of a field change installing together, as an older end decodes nothing newer

## [02]-[SERVERS]

`RpcGroup.toLayer` binds handlers, and `RpcServer.layer` serves the group over the current `RpcServer.Protocol`:

```ts
const handlers = Link.toLayer({ 'tools/list': list, 'tools/call': call, events: () => Rpc.fork(events) });
const served = RpcServer.layer(Link, { concurrency: 1, disableFatalDefects: true }).pipe(Layer.provide([handlers, RpcServer.layerProtocolSocketServer]));
```

`Link` is a group of MCP's `tools/list` and `tools/call` beside a streamed `events`, `list` and `call` are its handlers, and `events` is the host's event stream:
- `toLayer` takes a handler record or an effect building one and runs each handler with the services present at build, a hand `Effect.context` capture restating it
- `group.of(handlers)` types a record built apart, `toLayerHandler(tag, handler)` binds one procedure, and `accessHandler(tag)` calls one from code
- `concurrency` is one semaphore across every client of the server, unbounded by default, and a stream handler holds its permit until the stream ends
- `Rpc.fork(result)` runs a handler outside that bound and `Rpc.uninterruptible(result)` in an uninterruptible region, so a long-lived stream beside `concurrency: 1` forks
- Serialization through `concurrency` covers non-forked handlers alone, and host work in a forked stream or other fiber (a polled read) overlaps those calls
- Handler defects send a `Defect` frame that fails every open call of the connection, streams included, while the server runs those handlers on
- `disableFatalDefects: true` returns a defect as its own call's exit, the setting for any server holding streams or calls open beside one another
- Socket protocols acknowledge each stream chunk, the server writing the next after the acknowledgement and the client buffering `streamBufferSize` chunks (16)
- Ending a stream's consuming scope sends `Interrupt`, which interrupts its handler fiber, so a resource a stream handler acquires lasts one subscription
- `RpcMiddleware.Service` declares server middleware, its `error` schema joining each procedure's errors

## [03]-[CLIENTS]

`RpcClient.make(group)` reads `RpcClient.Protocol`, `Scope`, and each middleware's client half, and returns `client.<tag>(payload, options)`, `{ flatten: true }` one `client(tag, payload, options)`:
- Call options take `headers` and `context`, `discard: true` resolves once the server starts the handler, and a stream call takes `asQueue: true` and `streamBufferSize`
- `RpcClient.withHeaders(headers)` adds headers to the calls inside it
- `RpcMiddleware.layerClient(M, f)` provides a middleware's client half, and a library building a client captures `Effect.context<Rpc.MiddlewareClient<Rpcs>>()` for its caller
- Client services spread the derived client beside members derived from its calls, a stream's first value seeding state through `Stream.peel`
- Decode failures (a malformed frame, a frame over the buffer cap) fail every open call of the connection with an `RpcClientDefect`

Socket clients keep each connection as their options state:
- `makeProtocolSocket` reconnects under `Schedule.min([Schedule.exponential(500, 1.5), Schedule.spaced(5000)])`, `retryPolicy` replacing it, and a call while disconnected fails with the last error
- `retryPolicy: Schedule.recurs(0)` ends the protocol at the first close, for a peer whose end is final (a child's `stdout`, an accepted socket)
- `retryTransientErrors: true` retries a failed open without failing open calls, and `onTransientError` runs for each retry
- Pings go every 5 seconds, `pingTimeout` counts from the last decoded frame, and a timeout runs `onPingTimeout`, then fails open calls with `SocketReadError`
- `pingInterval: Duration.infinity` sends no ping, for a peer that answers none
- `RpcClient.ConnectionHooks` runs `onConnect` once each connection's reader opens and `onDisconnect` as the connection ends

## [04]-[PROTOCOLS]

Servers take an `RpcServer` protocol layer and clients an `RpcClient` one, and the transport between them selects both:

| [INDEX] | [TRANSPORT]             | [SERVER]                                                  | [CLIENT]                                           |
| :-----: | :---------------------- | :-------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | HTTP                    | `layerHttp({ group, path, protocol: 'http' })`            | `layerProtocolHttp({ url })`                       |
|  [02]   | WebSocket               | `layerHttp({ group, path })`                              | `layerProtocolSocket()` over a WebSocket           |
|  [03]   | TCP or Unix socket      | `layerProtocolSocketServer`                               | `layerProtocolSocket()` over `NodeSocket.layerNet` |
|  [04]   | Child standard streams  | `layerProtocolStdio` in the child                         | `makeProtocolSocket()` over its pipes              |
|  [05]   | Server dials the client | `layerProtocolSocketServer` over a dialing `SocketServer` | `makeProtocolSocket()` over an upgraded request    |

Framing selects the `RpcSerialization` layer:
- `layerJson` serves a transport framing each message (HTTP, WebSocket), `layerNdjson` a byte stream, and `layerSchemaBinary` binary frames
- `layerJsonRpc` and `layerNdJsonRpc` serve a JSON-RPC 2.0 peer, `controlMessages: false` for a peer sending no `@effect/rpc/*` control methods
- Byte-stream layers hold their own 16 MiB `maxBufferSize` (`Ndjson` holds none by default), and a frame over it clears the buffer and fails every open call
- `maxBufferSize: 'unbounded'` (`layerNdjsonWith`, `layerNdJsonRpc`) serves a peer sending frames past the cap (images, recordings)
- `RpcServer.layerHttp` defaults `protocol` to `'websocket'`, and `RpcServer.toHttpEffect(group)` serves a group without `HttpRouter` and takes no `concurrency`
- `layerProtocolHttp` sends one request per call, `batch: { window, maxSize }` joining calls made within `window`

Child process pipes join into a `Socket` a socket protocol reads:

```ts
const layerProtocolChildProcess = ({ stdin, stdout }: Pick<ChildProcessSpawner.ChildProcessHandle, 'stdin' | 'stdout'>) =>
    Effect.gen(function* () {
        const requests = new TransformStream<Uint8Array, Uint8Array>();
        yield* Stream.fromReadableStream({ evaluate: Function.constant(requests.readable), onError: Function.identity }).pipe(Stream.orDie, Stream.run(stdin), Effect.forkScoped);
        const socket = yield* Socket.fromTransformStream(Effect.succeed({ readable: Stream.toReadableStream(stdout), writable: requests.writable }));
        return yield* RpcClient.makeProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) }).pipe(Effect.provideService(Socket.Socket, socket), Effect.provide(RpcSerialization.layerNdJsonRpc({ maxBufferSize: 'unbounded' })));
    }).pipe(Layer.effect(RpcClient.Protocol));
```

Forked pumps copy requests into the child's `stdin`, and the protocol ends with the child's `stdout` in place of reconnecting.

## [05]-[SOCKETS]

`effect/socket` pulls frames with backpressure: acquiring `socket.reader` dials the connection, its `pull` yields non-empty batches, and every end, a clean close included, fails with `SocketError`. Reconnecting is a retry around the scoped consume loop:

```ts
const consume = (socket: Socket.Socket, base: Duration.Input) =>
    Effect.gen(function* () {
        const { pull } = yield* socket.reader;
        return yield* Effect.forever(Effect.flatMap(pull, handle));
    }).pipe(Effect.scoped, Effect.retry({ schedule: Schedule.exponential(base) }));
```

`handle` takes one batch of frames. Code between acquiring `reader` and the first `pull` runs once per connection and holds a handshake:
- `socket.writer` survives reconnects, a write while disconnected waiting for the next connection, and its scope's release half-closes the write side
- `SocketError.reason` is `SocketOpenError`, `SocketReadError`, `SocketWriteError`, `SocketCloseError` with its `code`, or `SocketUpgradeError`
- `Socket.make({ reader, writer })` wraps a socket, a `tapError` on `pull` reading the `SocketCloseError` code its peer closed with
- `writer.write(new Socket.CloseEvent(code, reason))` closes with an application code (4000 to 4999) the peer reads, such as one superseding an older peer
- `Socket.makeWebSocket(url)` and `Socket.layerWebSocket(url)` dial through the `WebSocketConstructor` service
- `NodeSocket.layerNet(options)` dials TCP or a Unix socket, and `NodeSocketServer.layer` accepts connections a `SocketServer` serves

`SocketServer` is a service of `address` and `run(handler)`, and one whose `run` dials lets the peer opening a connection serve the group (a plug-in inside a host application):

```ts
const dialing = (url: URL, redial: Duration.Duration) =>
    Effect.map(Effect.context<Socket.WebSocketConstructor>(), (services) =>
        SocketServer.SocketServer.of({
            address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Loopback, Number(url.port)),
            run: (handler) => Socket.makeWebSocket(url.href).pipe(Effect.flatMap(handler), Effect.scoped, Effect.exit, Effect.repeat(Schedule.spaced(redial)), Effect.andThen(Effect.never), Effect.provideContext(services)),
        }),
    );
```

`RpcServer.layer` over `layerProtocolSocketServer` takes `Layer.effect(SocketServer.SocketServer)(dialing(url, redial))`, and the accepting side upgrades each request on an `HttpRouter` route and runs `makeProtocolSocket` with `retryPolicy: Schedule.recurs(0)` over that socket, keeping each peer's client in a record by key that `ConnectionHooks` update.
