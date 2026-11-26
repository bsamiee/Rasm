# [CLIENTS]

## [01]-[CONTRACT_CLIENTS]

Contract clients run a family's own server and client code in one process, and each stops at a layer that only a real transport or engine reaches:

| [INDEX] | [FAMILY] | [CLIENT]                                   | [ENCODES]                      | [MISSES]                           |
| :-----: | :------- | :----------------------------------------- | :----------------------------- | :--------------------------------- |
|  [01]   | HttpApi  | `HttpApiTest.groups(Api, ids)`             | Routing, codecs, middleware    | Sockets, aborts, body draining     |
|  [02]   | MCP      | `McpTest.makeClient(layer, { protocols })` | JSON-RPC frames, notifications | HTTP, sessions, server requests    |
|  [03]   | RPC      | `RpcTest.makeClient(group, { flatten })`   | Acks, interrupts, middleware   | Codecs, server `concurrency`       |
|  [04]   | Cluster  | `Entity.makeTestClient(entity, layer)`     | Handlers per entity id         | Codecs, storage, sharding          |
|  [05]   | Cluster  | `TestRunner.layer`                         | Sharding, encoded envelopes    | Runner transport, restarts         |
|  [06]   | Workflow | `WorkflowEngine.layerMemory`               | Replay, activities, clocks     | Process restart                    |
|  [07]   | GraphQL  | `GraphQLClient.layerTest(Service, group)`  | Handler answers alone          | Codecs, middleware, partial errors |
|  [08]   | Chat     | `ChatTest.layer(options)`                  | Limits, capabilities, attempts | Drift of a real platform           |

[HTTP_API]:
- `HttpApiTest.groups(Api, ['<group>'])` reads handlers of the named groups from context and answers every endpoint of other groups with an `Unhandled endpoint` defect, so `ids` lists each group a test calls
- Requirements are `FileSystem`, `Etag.Generator`, `HttpPlatform`, and `Path`, which `HttpServer.layerServices` supplies with a file system holding no files
- Calls run in a child scope of the client's, held open by a streamed response until its body ends, and a `Stream` payload reaches its handler unbuffered
- Client aborts, `499` answers, connection close, and unread-body draining are not emulated, so tests of them serve on port 0

[MCP]:
- `McpTest.makeClient` builds `McpServer.layer` over an in-memory `RpcServer.Protocol` and sends each message as a serialized JSON-RPC frame, so results and errors decode as a wire client sees them
- `protocolVersion` selects one adapter of `protocols`, the first by default, and a version outside them fails with `IllegalArgumentError`
- Stateful versions send `initialize` and `notifications/initialized` before the client returns, and the stateless version attaches client metadata to every request
- `client.notifications` queues server notifications in wire encoding and delivery order, read through `Stream.fromQueue` and a schema decode (`resources.test.ts` `updated`)
- `McpRequestContext` reaches every handler and `McpServerClient` only when every adapter in `protocols` is stateful, and another `McpRequest` requirement takes MCP middleware
- Server-to-client requests (elicitation, sampling, roots) answer method-not-found, so a tool asking its client for input tests over HTTP
- `fixtures.ts` `opened` posts JSON-RPC to `HttpRouter.toWebHandler(layer)`, reaching session headers, protocol headers, and DNS-rebinding checks with no socket

```ts
const client = yield* McpTest.makeClient(Layer.provide(app(probe), Server.layer({ ...SETTINGS, scheme })), { protocols: [McpProtocol.v2025_11_25] });
```

`app(probe)` registers the suite's tools and resources over a `Probe` service of queues and refs, and `Server.layer` is `@rasm/mcp`'s server under test.

[RPC]:
- `RpcTest.makeClient` joins `RpcClient.makeNoSerialization` to `RpcServer.makeNoSerialization` over handlers from context, so values cross undecoded and the server runs every call unbounded
- Stream chunks take acknowledgements as on a socket protocol, and interrupts, headers, and middleware run their production code
- Schema transforms in a group (`OptionFromOptionalKey`, `ErrorInstance`), decode failures, and the order a `concurrency: 1` server imposes reach a test over a real socket alone
- `transport.test.ts` serves `@rasm/rpc` `listen` on port 0 and dials it, the form for a group's wire behavior
- `{ disableFatalDefects: true }` matches a production server returning a defect as its own call's exit

[CLUSTER]:
- `Entity.makeTestClient(entity, layer)` builds `layer` against a sharding stub recording `registerEntity` and returns `(entityId) => client`, one handler set per id built at its first call
- `ShardingConfig.layer()` supplies its requirement, handlers run at the entity's registered `concurrency` (1 by default), and a layer registering no such entity dies
- `TestRunner.layer` runs `Sharding` over `MessageStorage.layerMemory` and in-memory runner storage, so persisted messages save as `Envelope.Encoded` under their primary key
- `MessageStorage.MemoryDriver` holds `journal`, `requests`, and `requestsByPrimaryKey` for assertions on what was stored

[WORKFLOW]:
- `WorkflowEngine.layerMemory` replays a workflow body on each resume, keeping activity results by execution id, activity name, and attempt
- `DurableClock` sleeps run through `Effect.delay`, so under `it.effect` a test moves `TestClock` past a clock's duration to wake its workflow
- State ends with the layer's scope, so recovery after a restart tests `ClusterWorkflowEngine.layer`, which keeps executions in `MessageStorage`

[GRAPHQL]:
- `GraphQLClient.layerTest(Service, group)(handlers)` answers each operation from a handler over decoded values, and runs no codec, validation, or middleware
- Operations with no handler die with `UnimplementedError`, which `catchTag` in code under test cannot swallow
- `{ partial: true }` returns `{ data, errors: [] }` and subscriptions never retry, so partial data with errors, codecs, and subscription retry take a scripted `GraphQLProtocol` layer (`execute`, `subscribe`) under `GraphQLClient.make`
- Methods keep middleware service requirements, provided even though handlers ignore them

[CHAT]:
- `ChatTest.layer(options)` builds `Chat`, its `Registry`, and one fake adapter, every capability on unless `capabilities` lists fewer
- `userPosts`, `deliver`, `click`, `command`, `submit`, and `interact` of the `ChatTest` service build inbound traffic, and `calls` lists every attempt with its outcome
- `failNext(op, reason, { times, landed })` and `refuseFiles` inject faults, `landed: true` writing the message as an uncertain failure does
- Stream age, the interaction deadline (3 seconds), and follow-up age read `Clock`, so `it.effect` moves them through `TestClock`
- `ChatTest.conformance(options)` returns rules as effects keyed by name over `Chat` and `Registry`, and a rule needing a capability the descriptor lacks inverts to expect `Unsupported`
- `retries/*` rules exist only with `failNext`, and `retries/retrySafe` waits for real time, so the suite runs in a `layer` block with `excludeTestServices: true`
- `checkMessage`, `checkEvent`, and `checkInteraction` assert invariants every message, event, and interaction holds, in fixture tests and live runs alike

## [02]-[SERVERS]

Tests serve on port 0 when behavior needs a socket (client aborts, upgrades, reconnects, a peer in another process), reading the bound port from `HttpServer.address`:
- `NodeHttpServer.layerTest` listens on `127.0.0.1` port 0 and provides `HttpServer`, platform services, and an `HttpClient` prefixing each request with the server origin
- Its client runs a fresh `FetchHttpClient.layer` with `keepalive: false`, so no connection outlives a test
- `HttpServer.layerTestClient` builds that client over any `HttpServer` and `HttpClient`, replacing an unspecified listen address with IPv4 loopback and dying on a Unix socket address
- `NodeHttpServer.layerServer(createServer, options)` provides the listening `HttpServer` alone, for an app configured from its own bound address
- `transport.test.ts` closes a server's scope and listens again on its bound port, the form for a redial test

`official.test.ts` builds the server first, configures allowed hosts from its address, then serves the app on it for the conformance CLI to reach:

```ts
const listening = Context.get(yield* Layer.build(layerServer(createServer, { host: '127.0.0.1', port: 0 })), HttpServer.HttpServer);
const url = yield* Match.valueTags(listening.address, { UnixPathAddress: Effect.die, InetAddressV4: served, InetAddressV6: served });
const handler = yield* HttpRouter.toHttpEffect((yield* hosted(new URL(url).host)).layer);
yield* Effect.provideService(Layer.build(HttpServer.serve(handler)), HttpServer.HttpServer, listening);
```

`served` maps an address through `NetAddress.toUrl`, and `hosted` builds the MCP server layer accepting that host.

## [03]-[ROUTED_FETCH]

Code calling `HttpClient` through `FetchHttpClient.layer` reaches a handler in process through the `fetch` it reads:
- `FetchHttpClient.Fetch` is a reference each request reads, `globalThis.fetch` by default, and `Layer.succeed(FetchHttpClient.Fetch, fetch)` replaces it for the code inside
- References cache their default at first read, so a global `fetch` stub installs once per file and tests vary answers through a map (`store-publish.test.ts`)
- `transport(app)` of `@rasm/testing/http` runs an `HttpServerRequest` handler for each `fetch` through `HttpEffect.toWebHandlerWith`, as web `Request` and `Response` with no socket
- Apps under `transport` read services of the context building the layer, so a delivery test moves `TestClock` across `retry-after` delays (`delivery.test.ts`)
- `HttpRouter.toWebHandler(layer)` serves a whole router as `fetch`, and a `MutableRef` holding its handler swaps the peer mid-test (`fixtures.ts` `proxied` `restart`)
- Replayed traffic answers from `Response` values keyed by `<method> <url>`, cloning each answer, answering 404 with the key it lacks, and recording every request for assertion
