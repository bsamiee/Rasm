# [MCP]

## [01]-[SERVERS]

`McpServer` holds tools, resources, templates, prompts, and skills a transport serves. Registrations read `McpServer` from context, `registerToolkit` making one `addTool` call per tool:
- `McpServer.toolkit(kit)`, `resource`, `prompt`, and `skill` are layers, their `register*` forms effects capturing the services they run with
- `McpServer.toolkit(kit, { content })` maps successes to content blocks and drops `outputSchema` and `structuredContent`
- `McpServer.resource` templates take one schema per `${}` hole, `McpSchema.param(name, schema)` naming it, with no `{?query}` expansion
- `McpServer.prompt({ name, parameters, content })` derives argument requirement from field optionality and description from annotations
- `McpSchema.optional(S)` declares a field a client omits, an explicit `undefined` encoding as no key
- Handler services become `McpServer.McpRequest<'Requires', S>` markers middleware or the transport meets, a `Layer.provide` of `S` leaving them
- `layerHttp` turns markers left over into `HttpRouter.Request` markers an `HttpRouter.middleware` provides
- `addTool` and its siblings take `services` merged under each request's context, request services winning

Middleware wraps `tools/call`, `resources/read`, `prompts/get`, `completion/complete`, and `skills/get`, never `initialize`, and runs on list requests only where it wraps a visibility. It reads `{ method, target, payload, requestContext, headers, sessionId }` and sees the handler as an opaque `MiddlewareSuccess`, so it times, authenticates, or rejects a call and never reads its result. `@rasm/mcp` records the operation duration `McpServer` leaves unmeasured:

```ts
const operations: Partial<Record<McpServer.MiddlewareMethod, (target: string) => Readonly<Record<string, string>>>> = {
    'tools/call': (target) => ({ [ATTR_GEN_AI_TOOL_NAME]: target }),
    'resources/read': (target) => ({ [ATTR_MCP_RESOURCE_URI]: target }),
};
const metrics = McpServer.middleware((effect, { method, target }) =>
    UndefinedOr.match(operations[method], {
        onUndefined: () => effect,
        onDefined: Effect.fnUntraced(function* (attributing) {
            const [duration, exit] = yield* Effect.timed(Effect.exit(effect));
            yield* Metric.update(Metric.withAttributes(operationDuration, { [ATTR_MCP_METHOD_NAME]: method, ...attributing(target) }), duration);
            return yield* exit;
        }),
    }),
);
```

`operationDuration` is the `mcp.server.operation.duration` histogram over `Duration.toSeconds`, and `metrics.layerGlobal` installs it once. Methods `operations` lacks pass through, as `MiddlewareMethod` grows between releases. The layer a middleware enters through selects what it wraps:

| [INDEX] | [FORM]                              | [WRAPS]                                                           |
| :-----: | :---------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `m.layer`                           | Registrations it is provided to                                   |
|  [02]   | `m.layerGlobal`, `{ global: true }` | Every registration, outside local middleware, run once            |
|  [03]   | `a.combine(b).layer`                | Registrations it reaches, `b` running outside `a`                 |
|  [04]   | `McpServer.visibility(check)`       | Registrations a check hides per request, on lists and calls alike |
|  [05]   | `McpSchema.EnabledWhen` annotation  | Registrations hidden per client protocol and capability           |

Changes to every registration (an argument the server adds, a result shape, a gate) decorate the service registrations read, as middleware never replaces a result. `McpServer.McpServer.of({ ...server, addTool })` provided in place of the built server routes each `addTool` call through the decoration, and `@rasm/mcp` links reuse Effect's tool-to-MCP conversion with no transport:

```ts
const [server, registered] = yield* Effect.all([McpServer.McpServer.make, Ref.make<Record.ReadonlyRecord<string, Parameters<McpServer.McpServer['Service']['addTool']>[0]>>({})]);
yield* McpServer.registerToolkit(toolkit).pipe(Effect.provideService(McpServer.McpServer, McpServer.McpServer.of({ ...server, addTool: (options) => Ref.update(registered, Record.set(options.tool.name, options)) })));
```

- Server-wide decorations return `Context.make(McpServer.McpServer, decorated)` from `Layer.effectContext`, read by registrations over it
- Decorated `addTool` calls gate a registration by merging `Layer.buildWithScope(McpServer.visibility(check).layer, scope)` into its `services`

Failures reach a client by their kind and the protocol a session speaks:

| [INDEX] | [FAILURE]                               | [CLIENT_SEES]                                                                   |
| :-----: | :-------------------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | Declared `failure`, `'error'` mode      | `isError` text of an `Error` message, else of its JSON encoding                 |
|  [02]   | Declared `failure`, `'return'` mode     | `isError` text of its JSON encoding                                             |
|  [03]   | Parameter decode, `v2025_11_25` onward  | `isError` text `Invalid parameters for tool '<name>'`                           |
|  [04]   | Parameter decode, older protocols       | JSON-RPC `InvalidParams`                                                        |
|  [05]   | Undeclared failure or defect            | `isError` text of one generic message, cause logged and sent to `ErrorReporter` |
|  [06]   | `InternalError` of an `addTool` handler | `isError` text from `v2025_11_25`, JSON-RPC `InternalError` before              |
|  [07]   | Middleware `McpSchema.McpErrorBase`     | JSON-RPC error with its code, message, and data                                 |
|  [08]   | Middleware defect or other failure      | JSON-RPC `InternalError`, cause logged and reported                             |
|  [09]   | Registration hidden by visibility       | Not-found error of its method                                                   |
|  [10]   | Resource or prompt handler failure      | JSON-RPC `InternalError` with its message                                       |
|  [11]   | Template or prompt parameter decode     | JSON-RPC `InvalidParams`                                                        |

Tool successes send `structuredContent` and one JSON text block mirroring it, `Schema.Void` successes empty content. Protocols `v2025_06_18` and `v2025_11_25` drop a non-object `structuredContent` and `outputSchema`:
- Clients reading one channel lose the other (Claude Code shows `structuredContent` alone, Antigravity text alone)
- Servers those clients read send one text block, a decorated `addTool` omitting `outputSchema` and `structuredContent` for proxied tools too

Each `addTool`, `addResource`, `addResourceTemplate`, and `addPrompt` sends its kind's `list_changed`, coalesced to one per kind until the scheduler's next task:
- Visibility checks send no notification when their answer changes, so a server gating on changing state snapshots shown names per kind
- Flips read `Record.some(Record.intersection(now, before, Boolean.xor), Function.identity)`, new names left to `addTool`
- `layerHttpRequestScoped` and `allowSubscriptions: false` advertise `listChanged: false`, other `layerHttp` clients receiving changes

Protocol `v2026_07_28` keeps no session, and each request holds its caller's state:
- `McpServer.elicit` needs the `McpServerClient` a sessionful protocol supplies, a decline failing `ElicitationDeclined`, a cancel interrupting
- Raw `addTool` and `addPrompt` handlers return `McpSchema.InputRequired({ inputRequests, requestState })`, and toolkit handlers have no route to it
- Clients answer with `inputResponses` and the opaque `requestState`, both read from `McpSchema.McpRequestContext` on the next call
- `requestState` crosses the client sealed (`Jwt.encrypt` with the tool as audience), as a client can return any string
- Per-caller state sits under a handle the server issues and every tool takes as an argument, held in an `RcMap` with `idleTimeToLive`
- Handle state is one `TxSubscriptionRef`, events reaching every handle in one `Effect.tx` and followers reading `changesStream`
- Events reach a model inside tool results alone, and `@rasm/mcp` serves handle bind, NDJSON follow, and ack as an `HttpApi` beside `layerHttp`
- `McpSchema.ProgressToken` decodes `requestMetadata.progressToken`, and `server.notifications['notifications/progress']` reports against it

`McpServer` traces each request and serves it over a transport layer:
- Requests run in a server span named `tools/call <tool>`, `prompts/get <prompt>`, or the method alone, with `mcp.*` and `gen_ai.*` attributes
- `traceparent` in `params._meta` becomes the span's parent, and the transport span is linked
- `Toolkit.RecordToolArguments` set `true` adds `gen_ai.tool.call.arguments` to request spans
- `McpServer.layerStdio` holds stdout for the protocol, a server logging through `Logger.LogToStderr`
- `layerHttp` refuses an `Origin` its `allowedOrigins` lacks and checks no `Host` header
- Loopback servers add an `HttpRouter.middleware(f, { global: true })` refusing a `Host` outside `NetAddress.loopbackAddresses` and the bound address
- `layerHttpRequestScoped` serves one server per request over sessionless protocols alone, with no broadcast notification
- `cacheHints` set `v2026_07_28` list freshness (`{ ttl, cacheScope: 'private' }`), each kind left out sending `ttlMs: 0`
- `McpProtocol` exports adapters as its only values, so `Object.values(McpProtocol)` sorted newest first serves each revision a release adds
- `adapter.runtime._tag` (`'Stateless'`, `'Stateful'`) tells sessionless revisions apart

## [02]-[CLIENTS]

`McpClient.make(options)` connects over the `McpClient.Transport` in context, so a transport per protocol tries revisions newest first and keeps every refusal:

```ts
const connect = Effect.flip(Effect.forEach(protocols, (protocol) => Effect.flip(McpClient.make(info).pipe(Effect.provideServiceEffect(McpClient.Transport, McpClient.http({ url, protocol }))))));
```

`protocols` is `Object.values(McpProtocol)` newest first, and `info` the client's `{ name, version }`:
- `McpClient.http({ url, protocol, headers })` reads `HttpClient`, `McpClient.stdio({ command, protocol })` spawns through `ChildProcessSpawner`
- `McpClient.layerHttp` and `layerStdio` fix one protocol and provide `McpClient` to a program reading one server
- `McpClient.ProtocolVersion` types `2025-11-25` and `2026-07-28` while the client runs any adapter, a repository patch widening it
- `timeout` on `make` (60 seconds by default) bounds `initialize` and each discovery scan, `discoveryLimits` capping pages and items
- `callTool`, `getPrompt`, and `readResource` run unbounded unless given their own `timeout`
- `client.callTool({ tool, arguments })` takes a descriptor `listTools` returned and returns the `CallToolResult`, `isError` included
- `client.callTool({ tool, arguments, schema })` fails an `isError` result with reason `ToolError` and decodes `structuredContent` through `schema`
- `McpClient.toolkit(client, { prefix, resultMode, failureMode, trustAnnotations, timeout })` returns a handled toolkit `generateText` takes
- Remote tools take `Tool.Strict` `false` and names of 1 to 64 letters, digits, `_`, or `-`, and a remote change needs a new toolkit
- `McpClient` handles no server notification and lists no template, so a proxy re-lists on a `Schedule.spaced` poll through `transport.request`
- `McpClientError.reason` separates `TransportError`, `ProtocolError`, `HttpError`, `UnsupportedError`, and `ClosedError`
- Remaining reasons are `TimeoutError`, `LimitError`, `ConfigurationError`, `ValidationError`, and `ToolError`
- `ClosedError` with `sessionExpired` and `HttpError` 401 or 403 mark a lost session a new connection replaces
