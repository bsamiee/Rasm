# [REACTIVITY]

## [01]-[RPC]

One `RpcGroup` of schema-typed procedures derives server handlers, clients, protocols, and tests:
- `Rpc.make(tag, { payload, success, error, stream: true })` declares a procedure, `stream: true` returning a `Stream` with its error in the stream
- Payload classes hold a `static readonly Result`, and one `request(tag, Payload)` over `Rpc.make` fixes a failure union every procedure shares
- `RpcGroup.make(...rpcs)` composes procedures, and `.middleware(M)` reaches procedures added before it
- `group.toLayer(Effect<handlers>)` builds every handler from services read once
- Handlers return an `Effect` or a `Stream`, and `Rpc.fork` runs one outside the server's concurrency bound
- `RpcServer.layerHttp({ group, path, protocol })` mounts on `HttpRouter`
- `RpcServer.layerProtocolStdio` and `layerProtocolSocketServer` serve stdio and sockets
- `RpcClient.make(group)` reads `RpcClient.Protocol` and `Scope` and returns `client.tag(payload)` methods
- `RpcClient.make(group, { flatten: true })` returns one `call(tag, payload)` function
- `RpcClient.Protocol.make` over a one-shot host call sends each `Request` message, passes `writeResponse` every reply, and sets `supportsAck: false`
- `Socket.fromTransformStream` joins a child process's stdin and stdout into the `Socket` that `layerProtocolSocket` reads
- `RpcClient.layerProtocolHttp({ url })` batches requests, and `layerProtocolSocket` reconnects forever by default
- `retryPolicy: Schedule.recurs(0)` ends a socket client on its first error, and `pingInterval: Duration.infinity` sends no pings
- `RpcClient.ConnectionHooks` runs `onConnect` and `onDisconnect` on socket open and close, adding and removing accepted peers in a registry
- Accepted sockets build client middleware through `Layer.build(Layer.provideMerge(middleware, Layer.succeed(RpcClient.Protocol, protocol)))`
- `RpcSerialization.layerNdjson` frames streams and sockets, `layerJson` HTTP requests, and `layerJsonRpc` speaks JSON-RPC 2.0
- Encoders outside a protocol (a defect reply written past the server) take `RpcSerialization.json`, the value behind protocol `layerJson`
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
- `AtomHttpApi.Service<Self>()(id, { api, httpClient })` exposes `query(group, endpoint, { reactivityKeys })` and `mutation(group, endpoint)`
- `runtime.atom` and `runtime.fn((arg, get) => ...)` of a service hold work spanning calls, `get.set` and `get.refresh` updating other atoms
- `{ notes: [id] }` keys invalidate both `notes` and `notes:<id>`
- `Atom.runtime(layer)` returns a runtime with `atom`, `fn`, `pull`, and `subscriptionRef` reading the layer's services
- `runtime.fn` atoms restart on each write unless `concurrent: true`, and writing `Atom.Reset` or `Atom.Interrupt` resets or cancels one
- Atoms dispose when unmounted, `Atom.keepAlive` pins one, `Atom.setIdleTTL(d)` keeps one for `d`, and `Atom.family(key => atom)` memoizes by key
- `Atom.swr({ staleTime })`, `withRefresh(d)`, `debounce`, `optimistic`, and `withEquality(eq)` state refresh, update, and notify policies
- External stores enter an atom through `get.addFinalizer(store.subscribe(() => get.setSelf(store.getState())))` inside `Atom.make`
- `Atom.kvs({ runtime, key, schema, defaultValue })` persists through a `KeyValueStore`, and `Atom.searchParam(name, { schema })` binds an `Option`
- Atoms over browser state (`searchParam`, `kvs`) pipe through `Atom.withServerValue(() => value)`, hydration rendering `value` before browser state
- `Atom.readable((get) => get(state))` publishes a module-private writable atom read-only
- `Atom.get` and `Atom.getResult(atom)` read an atom in an effect through `AtomRegistry`, a `keepAlive` atom over `Effect.acquireRelease` sharing one resource
- `AsyncResult.builder(result).onInitial(f).onSuccess(f).onErrorTag(f).render()` renders a result, `Failure.previousSuccess` holding the last value
- `useAtomValue(atom, select)`, `useAtomSet`, `useAtomSuspense`, and `useAtomRefresh` read atoms in React
- `useAtomMount(atom)` keeps an atom no component reads (a host registration) mounted for a component's life
- `RegistryProvider` with `HydrationBoundary` supplies server values to atoms
