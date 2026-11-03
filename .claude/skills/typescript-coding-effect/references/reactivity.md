# [REACTIVITY]

## [01]-[RPC]

One `RpcGroup` of schema-typed procedures derives server handlers, clients, protocols, and tests:
- `Rpc.make(tag, { payload, success, error, stream: true })` declares a procedure, `stream: true` returning a `Stream` with its error in the stream
- `RpcGroup.make(...rpcs)` composes procedures, `.middleware(M)` reaching the ones added before it, and `.toLayer(Effect<handlers>)` builds every handler from services read once
- Handlers return an `Effect` or a `Stream`, and `Rpc.fork` runs one outside the server's concurrency bound
- `RpcServer.layerHttp({ group, path, protocol })` mounts on `HttpRouter`, and `layerProtocolStdio` and `layerProtocolSocketServer` serve other transports
- `RpcClient.make(group)` returns `client.tag(payload)` methods, reading `RpcClient.Protocol` and `Scope`, and `{ flatten: true }` one `call(tag, payload)` function
- `RpcClient.Protocol.make` over a one-shot host call sends each `Request` message, passes `writeResponse` every reply, and sets `supportsAck: false`
- `Socket.fromTransformStream` joins a child process's stdin and stdout into the `Socket` that `layerProtocolSocket` reads
- `RpcClient.layerProtocolHttp({ url })` batches requests, and `layerProtocolSocket` reconnects forever by default
- `retryPolicy: Schedule.recurs(0)` ends a socket client on its first error, and `pingInterval: Duration.infinity` sends no pings
- `RpcClient.ConnectionHooks` runs `onConnect` and `onDisconnect` as a socket protocol opens and closes, a registry of accepted peers adding and removing each
- Accepted sockets build their own client middleware through `Layer.build(Layer.provideMerge(middleware, Layer.succeed(RpcClient.Protocol, protocol)))`
- `RpcSerialization.layerNdjson` frames streams and sockets, `layerJson` HTTP requests, and `layerJsonRpc` speaks JSON-RPC 2.0
- `RpcMiddleware.Service` declares server middleware, and `RpcMiddleware.layerClient(M, f)` its client half
- `RpcTest.makeClient(group)` runs a client against the handler layer in memory
- `Rpc.Payload<R>`, `Rpc.Success<R>`, `Rpc.Error<R>`, and `RpcGroup.Rpcs<typeof G>` read a procedure's types

## [02]-[ATOMS]

Atoms hold derived async state in a registry, and `Reactivity` keys connect each write to the reads it invalidates:

```ts
const Notes = RpcGroup.make(Rpc.make('list', { success: Schema.Array(Note) }), Rpc.make('save', { payload: Note, success: Note, error: StoreError }));

class NotesClient extends AtomRpc.Service<NotesClient>()('@app/notes/NotesClient', {
    group: Notes,
    protocol: RpcClient.layerProtocolHttp({ url: '/rpc' }).pipe(Layer.provide([FetchHttpClient.layer, RpcSerialization.layerNdjson])),
}) {}
```

`Note` is a struct schema that keys reads and writes through `Hash.hash`, and `StoreError` the package error. `NotesClient.query('list', undefined, { reactivityKeys: [Note] })` and `NotesClient.mutation('save')` return atoms memoized per argument, and a save written with the same `reactivityKeys` reruns the list on success:
- `AtomHttpApi.Service<Self>()(id, { api, httpClient })` exposes `query(group, endpoint, { reactivityKeys })` and `mutation(group, endpoint)` over an `HttpApi`
- `{ notes: [id] }` keys invalidate both `notes` and `notes:<id>`
- `Atom.runtime(layer)` returns a runtime with `atom`, `fn`, `pull`, and `subscriptionRef` reading the layer's services
- `runtime.fn` atoms restart on each write, `concurrent: true` keeping earlier calls, and writing `Atom.Reset` or `Atom.Interrupt` resets or cancels one
- Atoms dispose when unmounted, `Atom.keepAlive` pins one, `Atom.setIdleTTL(d)` keeps one for a duration, and `Atom.family(key => atom)` memoizes by key
- `Atom.swr({ staleTime })`, `withRefresh(d)`, `debounce`, `optimistic`, and `withEquality(eq)` state refresh, update, and notify policies
- External stores enter an atom through `get.addFinalizer(store.subscribe(() => get.setSelf(store.getState())))` inside `Atom.make`
- `Atom.kvs({ runtime, key, schema, defaultValue })` persists through a `KeyValueStore`, and `Atom.searchParam(name, schema)` binds a URL parameter
- `AsyncResult.builder(result).onInitial(...).onSuccess(...).onErrorTag(...).render()` folds a result for a view, `Failure.previousSuccess` holding the last value
- `useAtomValue`, `useAtomSet`, `useAtomSuspense`, and `useAtomRefresh` read atoms in React, and `RegistryProvider` with `HydrationBoundary` supplies server values
