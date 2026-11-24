# [RPC]

## [01]-[GROUPS]

One `RpcGroup` of schema-typed procedures derives server handlers, clients, protocols, and atoms:

```ts
const Notes = RpcGroup.make(Rpc.make('list', { success: Schema.Array(Note) }), Rpc.make('save', { payload: Note, success: Note, error: StoreError }), Rpc.make('watch', { success: Note, stream: true }));
```

`Note` is a schema class and `StoreError` the package error. `Notes.toLayer(Effect.map(Store, (store) => Notes.of({ ... })))` builds every handler, `watch` returning a `Stream` with failures arriving in the stream:
- `.add`, `.merge`, `.prefix`, and `.omit` extend, join, rename, or narrow a group, and `.middleware(M)` reaches procedures added before it
- Handlers of one client run concurrently up to the server's `concurrency`, and `Rpc.fork(effect)` runs one outside that bound
- `primaryKey` on `Rpc.make` gives a payload an identity, and `defect` sets the schema a defect crosses the wire through
- `RpcMiddleware.Service` declares server middleware, and `RpcMiddleware.layerClient(M, f)` its client half
- `RpcClient.make(group)` reads `RpcClient.Protocol` and `Scope` and returns `client.<tag>(payload)`, `{ flatten: true }` one `client(tag, payload)`
- `RpcClient.withHeaders(headers)` adds headers to the calls inside it

## [02]-[PROTOCOLS]

Servers take an `RpcServer` protocol layer and clients an `RpcClient` one, and the transport between them selects both:

| [INDEX] | [TRANSPORT]            | [SERVER]                                       | [CLIENT]                                           |
| :-----: | :--------------------- | :--------------------------------------------- | :------------------------------------------------- |
|  [01]   | HTTP                   | `layerHttp({ group, path, protocol: 'http' })` | `layerProtocolHttp({ url })`                       |
|  [02]   | WebSocket              | `layerHttp({ group, path })`                   | `layerProtocolSocket()` over a WebSocket           |
|  [03]   | TCP or Unix socket     | `layerProtocolSocketServer`                    | `layerProtocolSocket()` over `NodeSocket.layerNet` |
|  [04]   | Child standard streams | `layerProtocolStdio` in the child              | `makeProtocolSocket()` over its pipes              |
|  [05]   | Worker                 | `layerProtocolWorkerRunner`                    | `layerProtocolWorker({ size })`                    |

Framing selects the `RpcSerialization` layer: `layerJson` where the transport frames each message (HTTP, WebSocket), `layerNdjson` over a byte stream, and `layerJsonRpc` or `layerNdJsonRpc` for a JSON-RPC 2.0 peer. A child process's pipes join into a `Socket` a socket protocol reads:

```ts
const child = ({ stdin, stdout }: Pick<ChildProcessSpawner.ChildProcessHandle, 'stdin' | 'stdout'>) =>
    Layer.effect(RpcClient.Protocol)(
        Effect.gen(function* () {
            const requests = new TransformStream<Uint8Array, Uint8Array>();
            yield* Stream.fromReadableStream({ evaluate: Function.constant(requests.readable), onError: Function.identity }).pipe(Stream.orDie, Stream.run(stdin), Effect.forkScoped);
            const socket = yield* Socket.fromTransformStream(Effect.succeed({ readable: Stream.toReadableStream(stdout), writable: requests.writable }));
            return yield* RpcClient.makeProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) }).pipe(Effect.provideService(Socket.Socket, socket), Effect.provide(RpcSerialization.layerNdjson));
        }),
    );
```

Forked pumps copy requests into the child's `stdin`, and the protocol ends with its `stdout` in place of reconnecting:
- `RpcServer.layerHttp` defaults `protocol` to `'websocket'`, and `RpcServer.toHttpEffect(group)` serves a group without `HttpRouter`
- `layerProtocolHttp` sends one request per call, `batch: { window, maxSize }` joining calls made within `window`
- `layerProtocolSocket` reconnects forever under exponential delays from 500 milliseconds capped at 5 seconds, `retryPolicy` replacing that schedule
- Socket clients ping every 5 seconds, `pingInterval: Duration.infinity` sending none and `onPingTimeout` running before a silent connection drops
- `RpcClient.ConnectionHooks` runs `onConnect` and `onDisconnect` on each socket open and close

## [03]-[SOCKETS]

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
- `Socket.makeWebSocket(url)` and `Socket.layerWebSocket(url)` dial through the `WebSocketConstructor` service
- `NodeSocket.layerNet(options)` dials TCP or a Unix socket, `NodeSocketServer.layer` accepts connections a `SocketServer` serves

## [04]-[ATOMS]

Atoms hold derived state in a registry that `@effect/atom-react` hooks read, and `Reactivity` keys connect each write to the reads it invalidates. Client services over a group derive one atom per call:

```ts
class NotesClient extends AtomRpc.Service<NotesClient>()('@app/notes/NotesClient', {
    group: Notes,
    protocol: RpcClient.layerProtocolHttp({ url: '/rpc' }).pipe(Layer.provide([FetchHttpClient.layer, RpcSerialization.layerNdjson])),
}) {}

const notes = NotesClient.query('list', undefined, { reactivityKeys: ['notes'] });
const save = NotesClient.mutation('save');
```

Writing `{ payload, reactivityKeys: ['notes'] }` to `save` reruns `notes` once the save succeeds. `AtomHttpApi.Service<Self>()(id, { api, httpClient })` derives the same `query(group, endpoint, request)` and `mutation(group, endpoint)` over an `HttpApi`, and the state an atom holds selects its constructor:

| [INDEX] | [STATE]                          | [ATOM]                                                                                   |
| :-----: | :------------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | Value a view writes              | `Atom.make(initial)`                                                                     |
|  [02]   | Value derived from other atoms   | `Atom.make((get) => ...)`, `Atom.mapResult`                                              |
|  [03]   | Effect result over a service     | `Client.runtime.atom(effect)`, `Atom.runtime(layer).atom`                                |
|  [04]   | Write running an effect          | `runtime.fn((arg, get) => effect, { reactivityKeys })`                                   |
|  [05]   | Server read and write            | `Client.query`, `Client.mutation`                                                        |
|  [06]   | Browser storage or URL parameter | `Atom.kvs({ runtime, key, schema, defaultValue })`, `Atom.searchParam(name, { schema })` |
|  [07]   | External store                   | `Atom.make` with `get.addFinalizer(store.subscribe(...))`                                |

Atoms dispose when no component mounts them:
- `Atom.keepAlive` holds one for the registry's life, `Atom.setIdleTTL(d)` for `d` after its last reader
- `Atom.family(key => atom)` memoizes atoms by key
- `runtime.fn` atoms interrupt a write on the next unless `concurrent: true`, and writing `Atom.Reset` or `Atom.Interrupt` resets or cancels one
- `Atom.writable(read, write, refresh)` declares one writable atom over a request atom, `get.registry.update` editing it from a `runtime.fn`
- `Atom.swr({ staleTime })`, `withRefresh(d)`, `debounce(d)`, `optimistic`, and `withEquality(eq)` set refresh, update, and notify policies
- `{ notes: [id] }` keys invalidate `notes` and `notes:<id>`, and `Atom.withReactivity(keys)` attaches keys to any atom
- Interfaces merged into `Reactivity.Keys` type every key
- `keepAlive` atoms over `Effect.acquireRelease` share one resource among their readers
- `AtomRegistry.layer` and `Reactivity.layer` provide both services outside React, `Atom.get` and `Atom.getResult(atom)` reading an atom in an effect
- `AsyncResult.builder(result).onInitial(f).onSuccess(f).onErrorTag(tag, f).render()` renders a result
- Failed results keep their last success in `previousSuccess`

`@effect/atom-react` reads atoms in components and passes server state to the client:
- `useAtomValue(atom, select)`, `useAtomSet`, `useAtomSuspense`, and `useAtomRefresh` read and write atoms
- `useAtom(atom, { mode: 'promise' })` awaits a write
- `useAtomMount(atom)` keeps an atom no component reads mounted for a component's life
- `Atom.withServerValue(() => value)` renders `value` on the server and during hydration for an atom over browser state
- `Atom.withServerValueInitial` renders an async atom's initial state on the server and during hydration
- `Atom.serializable({ key, schema })` marks an atom for `Hydration.dehydrate`, and `HydrationBoundary` under `RegistryProvider` loads it as `state`
