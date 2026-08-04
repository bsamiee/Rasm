# [RASM_APPHOST_API_MCP]

`ModelContextProtocol.Core` owns the MCP session, client, server, transport, and primitive surfaces; `ModelContextProtocol` binds DI composition, builder extensions, and hosted-service plumbing; `ModelContextProtocol.AspNetCore` binds HTTP transport, SSE event-stream persistence, and ASP.NET Core authentication-handler registration.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ModelContextProtocol.Core`
- package: `ModelContextProtocol.Core`
- assembly: `ModelContextProtocol.Core`
- namespace: `ModelContextProtocol`, `ModelContextProtocol.Client`, `ModelContextProtocol.Server`, `ModelContextProtocol.Protocol`
- rail: mcp-protocol

[PACKAGE_SURFACE]: `ModelContextProtocol`
- package: `ModelContextProtocol`
- assembly: `ModelContextProtocol`
- namespace: `ModelContextProtocol`, `ModelContextProtocol.Server`, `Microsoft.Extensions.DependencyInjection`
- rail: mcp-host

[PACKAGE_SURFACE]: `ModelContextProtocol.AspNetCore`
- package: `ModelContextProtocol.AspNetCore`
- assembly: `ModelContextProtocol.AspNetCore`
- namespace: `ModelContextProtocol.AspNetCore`, `ModelContextProtocol.AspNetCore.Authentication`, `Microsoft.AspNetCore.Builder`, `Microsoft.Extensions.DependencyInjection`
- rail: mcp-host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: session and protocol primitives — `ModelContextProtocol.Core`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]       | [CAPABILITY]                     |
| :-----: | :-------------------------- | :------------------ | :------------------------------- |
|  [01]   | `McpSession`                | abstract base class | shared session lifecycle         |
|  [02]   | `McpServer`                 | abstract class      | server session                   |
|  [03]   | `McpClient`                 | abstract class      | client session                   |
|  [04]   | `McpException`              | exception           | typed protocol failure           |
|  [05]   | `McpErrorCode`              | enum                | JSON-RPC error vocabulary        |
|  [06]   | `RequestOptions`            | options class       | request timeout and cancellation |
|  [07]   | `IMcpTaskStore`             | interface           | task-result persistence          |
|  [08]   | `InMemoryMcpTaskStore`      | class               | `IMcpTaskStore` implementation   |
|  [09]   | `McpJsonUtilities`          | static class        | serializer options and type-info |
|  [10]   | `AIContentExtensions`       | static class        | bidirectional content conversion |
|  [11]   | `NullProgress`              | class               | no-op `IProgress<T>`             |
|  [12]   | `ProgressNotificationValue` | sealed class        | progress payload                 |
|  [13]   | `UriTemplate`               | class               | RFC 6570 template evaluation     |
|  [14]   | `Implementation`            | sealed class        | protocol identity                |

[PROGRESS_NOTIFICATION_VALUE]: `required float Progress`, `float? Total`, `string? Message`, all `init`.

[IMPLEMENTATION]: `ModelContextProtocol.Protocol`; `required string Name` and `required string Version` with `string? Title`, bound at `McpClientOptions.ClientInfo` and `McpServerOptions.ServerInfo`.

[PUBLIC_TYPE_SCOPE]: server primitives — `ModelContextProtocol.Server`

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]    | [CAPABILITY]                                          |
| :-----: | :--------------------------- | :--------------- | :---------------------------------------------------- |
|  [01]   | `McpServerTool`              | abstract class   | tool primitive base, implements `IMcpServerPrimitive` |
|  [02]   | `McpServerPrompt`            | abstract class   | prompt primitive base                                 |
|  [03]   | `McpServerResource`          | abstract class   | resource primitive base                               |
|  [04]   | `AIFunctionMcpServerTool`    | sealed class     | `AIFunction`-backed tool adapter                      |
|  [05]   | `DelegatingMcpServerTool`    | class            | delegate-wrapping tool                                |
|  [06]   | `McpServerToolCreateOptions` | sealed class     | tool create policy bag                                |
|  [07]   | `McpServerOptions`           | sealed class     | server configuration root                             |
|  [08]   | `McpServerHandlers`          | class            | server request-handler registry                       |
|  [09]   | `McpServerFilters`           | class            | server message and request filter registry            |
|  [10]   | `McpServerToolAttribute`     | sealed attribute | marks a method as an MCP tool                         |
|  [11]   | `McpMetaAttribute`           | sealed attribute | attaches metadata to server primitives                |
|  [12]   | `StdioServerTransport`       | class            | stdio-backed server transport                         |
|  [13]   | `StreamServerTransport`      | class            | stream-backed server transport                        |
|  [14]   | `ISseEventStreamStore`       | interface        | SSE event persistence for resumability                |
|  [15]   | `ISseEventStreamWriter`      | interface        | per-stream event writer, `IAsyncDisposable`           |
|  [16]   | `ISseEventStreamReader`      | interface        | replay reader from a `Last-Event-ID` cursor           |
|  [17]   | `SseEventStreamOptions`      | sealed class     | stream creation policy                                |
|  [18]   | `SseEventStreamMode`         | enum             | `Streaming` / `Polling` read behaviour                |

[TOOL_CREATE_OPTIONS]:
- Identity: `Name`, `Title`, `Description`.
- Safety annotations: `ReadOnly`, `Destructive`, `Idempotent`, `OpenWorld` (nullable tri-state).
- Structured output: `UseStructuredContent`, `OutputSchema`, `Execution` (`ToolExecution?`).
- Policy: `SerializerOptions`, `SchemaCreateOptions`, `Services`, `Metadata`, `Icons`, `Meta`.

[SSE_EVENT_STREAM_STORE]: the RESUMABILITY owner — a host that buffers frames itself duplicates it.
- `ISseEventStreamStore.CreateStreamAsync(SseEventStreamOptions, CancellationToken)` `-> ValueTask<ISseEventStreamWriter>`; `GetStreamReaderAsync(string lastEventId, CancellationToken)` `-> ValueTask<ISseEventStreamReader?>`, `null` when no stream matches.
- `SseEventStreamOptions`: `required string SessionId`, `required string StreamId` (unique per session), `SseEventStreamMode Mode` (defaults `Streaming`).
- `ISseEventStreamWriter.WriteEventAsync(SseItem<JsonRpcMessage?>, CancellationToken)` `-> ValueTask<SseItem<JsonRpcMessage?>>` returning the id-stamped item; an item arriving WITH an event id is skipped, so the store assigns every id and ids are unique across all sessions and streams. `SetModeAsync(SseEventStreamMode, CancellationToken)` flips the read behaviour.
- `ISseEventStreamReader`: `SessionId`, `StreamId`, `ReadEventsAsync(CancellationToken)` `-> IAsyncEnumerable<SseItem<JsonRpcMessage?>>` — `Polling` yields only the currently-available events after the cursor, `Streaming` continues until the paired writer disposes.
- Bound through `HttpServerTransportOptions.EventStreamStore` or resolved from DI; when set the server generates per-message event ids, stores them, replays on a `Last-Event-ID` reconnect, and sends priming events to establish resumability before any message.

[DISTRIBUTED_CACHE_EVENT_STREAM_STORE]: the package SHIPS a durable implementation whose backing is an `IDistributedCache` — a host already holding a transactional store implements the interface against THAT rather than admitting a second durability backing, and a host with no such store takes this one rather than hand-rolling.
- `DistributedCacheEventStreamStore : ISseEventStreamStore` (`ModelContextProtocol.Server`, in the `ModelContextProtocol` package) persists every event and its stream metadata into an `IDistributedCache`; `WithDistributedCacheEventStreamStore(Action<DistributedCacheEventStreamStoreOptions>?)` on `IMcpServerBuilder` registers it. Its `DistributedCacheEventIdFormatter` key codec is `internal`, so an implementation off this rail mints and parses its own event ids.
- `DistributedCacheEventStreamStoreOptions`: `Cache` (`IDistributedCache?`, resolved from DI when unset), `EventSlidingExpiration` (30m), `EventAbsoluteExpiration` (2h), `MetadataSlidingExpiration` (1h), `MetadataAbsoluteExpiration` (4h), `StreamReaderPollingInterval` (1s) — the reader polls the cache at that interval, so it bounds replay latency, and the four expirations bound the resumable window.

[PUBLIC_TYPE_SCOPE]: client primitives — `ModelContextProtocol.Client`

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :------------------------------- | :------------ | :------------------------------- |
|  [01]   | `McpClientTool`                  | sealed class  | server-tool `AIFunction` adapter |
|  [02]   | `McpClientPrompt`                | sealed class  | prompt accessor                  |
|  [03]   | `McpClientResource`              | sealed class  | resource accessor                |
|  [04]   | `McpClientResourceTemplate`      | sealed class  | resource-template accessor       |
|  [05]   | `McpClientOptions`               | class         | client configuration             |
|  [06]   | `McpClientHandlers`              | class         | notification-handler registry    |
|  [07]   | `IClientTransport`               | interface     | session-transport factory        |
|  [08]   | `HttpClientTransport`            | sealed class  | HTTP session transport           |
|  [09]   | `StdioClientTransport`           | sealed class  | stdio session transport          |
|  [10]   | `StreamClientTransport`          | sealed class  | paired-stream session transport  |
|  [11]   | `StdioClientTransportOptions`    | class         | stdio transport configuration    |
|  [12]   | `HttpClientTransportOptions`     | class         | HTTP transport configuration     |
|  [13]   | `HttpTransportMode`              | enum          | HTTP session-transport mode      |
|  [14]   | `ClientTransportClosedException` | class         | transport-closed failure         |

Every `*ClientTransport` implements `IClientTransport`; `StreamClientTransport` lives in `ModelContextProtocol.Protocol` and constructs from `(Stream serverInput, Stream serverOutput, ILoggerFactory?)`.

- `StdioClientTransportOptions`: `required string Command`, with `Arguments` and `Name`.
- `HttpClientTransportOptions`: `required Uri Endpoint`, `HttpTransportMode TransportMode`, `Name`.
- `HttpTransportMode`: `AutoDetect`, `StreamableHttp`, `Sse`; `HttpClientTransport` selects at connect time.

[PUBLIC_TYPE_SCOPE]: DI and builder — `Microsoft.Extensions.DependencyInjection` (in `ModelContextProtocol`)

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :---------------------------------- | :------------ | :-------------------------------- |
|  [01]   | `IMcpServerBuilder`                 | interface     | server builder contract           |
|  [02]   | `IMcpMessageFilterBuilder`          | interface     | message-filter builder contract   |
|  [03]   | `IMcpRequestFilterBuilder`          | interface     | request-filter builder contract   |
|  [04]   | `McpServerBuilderExtensions`        | static class  | server primitive registration     |
|  [05]   | `McpMessageFilterBuilderExtensions` | static class  | message-filter registration       |
|  [06]   | `McpRequestFilterBuilderExtensions` | static class  | per-operation filter registration |

[SERVER_BUILDER_EXTENSIONS]: `WithTools` `WithPrompts` `WithResources` `WithMessageFilters` `WithRequestFilters`.
[MESSAGE_FILTER_BUILDER_EXTENSIONS]: `AddIncomingFilter` `AddOutgoingFilter`.

- `McpMessageFilter` is handler-wrapping, not call-shaped: `delegate McpMessageHandler McpMessageFilter(McpMessageHandler next)` over `delegate Task McpMessageHandler(MessageContext context, CancellationToken cancellationToken)`, so a filter returns a handler closing over `next` and registration order is outermost-first.

[PUBLIC_TYPE_SCOPE]: ASP.NET Core host — `ModelContextProtocol.AspNetCore`

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :---------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `HttpServerTransportOptions`        | class         | HTTP server transport configuration            |
|  [02]   | `ISessionMigrationHandler`          | interface     | session migration between instances contract   |
|  [03]   | `McpEndpointRouteBuilderExtensions` | static class  | `MapMcp(pattern)` route registration           |
|  [04]   | `HttpMcpServerBuilderExtensions`    | static class  | HTTP transport, filters, cache event store     |
|  [05]   | `McpAuthenticationExtensions`       | static class  | `AddMcp` authentication scheme builder         |
|  [06]   | `McpAuthenticationDefaults`         | static class  | scheme name constants                          |
|  [07]   | `McpAuthenticationOptions`          | class         | extends `AuthenticationSchemeOptions`          |
|  [08]   | `McpAuthenticationHandler`          | class         | `AuthenticationHandler` implementation         |
|  [09]   | `ResourceMetadataRequestContext`    | class         | resource metadata request context              |

[HTTP_SERVER_TRANSPORT_OPTIONS]:
- `EventStreamStore` (`ISseEventStreamStore?`) arms SSE resumability; unset, the server resolves one from DI.
- `SessionMigrationHandler` (`ISessionMigrationHandler?`) admits a session whose id this instance does not hold; unset, the server resolves one from DI.
- `Stateless` (default `false`): `true` nulls `McpSession.SessionId`, drops the `MCP-Session-Id` header, disables `/sse`, calls `RunSessionHandler` per request, and disables every server-initiated leg — sampling, elicitation, and roots included.
- `ConfigureSessionOptions` (`Func<HttpContext, McpServerOptions, CancellationToken, Task>?`) runs once per session at `initialize`, and per request under `Stateless`.
- `RunSessionHandler` (`Func<HttpContext, McpServer, CancellationToken, Task>?`) is `[Experimental("MCPEXP002")]`; `ConfigureSessionOptions` is the supported seat for initialize-time `HttpContext` access.
- `EnableLegacySse` is `[Obsolete(DiagnosticId = "MCP9004")]` — the `/sse` + `/message` pair returns `202 Accepted` with no handler-concurrency backpressure, and setting it beside `Stateless` throws at startup.
- `PerSessionExecutionContext` (default `false`): `true` runs every handler under the `ExecutionContext` that ran `ConfigureSessionOptions`/`RunSessionHandler`, so a session-lifetime `AsyncLocal<T>` persists — and `IHttpContextAccessor` stops resolving in handlers, since the flowed context is no longer the request's.
- `IdleTimeout` (2h) and `MaxIdleSessionCount` (10000) bound in-memory session retention: past the count the server logs critical and terminates the oldest idle sessions ahead of their timeout; a session with any active HTTP request is not idle, and legacy-SSE sessions are subject to neither because their lifetime is the open GET `/sse` request.
- `TimeProvider` (default `TimeProvider.System`) drives the idle clock, so the timeout is testable without wall-clock waits.

[SESSION_MIGRATION_HANDLER]: `OnSessionInitializedAsync(HttpContext, string sessionId, InitializeRequestParams, CancellationToken)` persists the handshake params externally; `AllowSessionMigrationAsync(HttpContext, string sessionId, CancellationToken)` `-> ValueTask<InitializeRequestParams?>` returns them to admit the migration or `null` to answer 404, and authorizes off `HttpContext.User`.

[PUBLIC_TYPE_SCOPE]: request-context surface — `ModelContextProtocol.Server`

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :------------------ | :------------ | :-------------------- |
|  [01]   | `MessageContext`    | base class    | shared request state  |
|  [02]   | `RequestContext<T>` | sealed class  | typed request context |

[MESSAGE_CONTEXT]: declares `.Server`, `.Services`, `.User`, `.Items`.
[REQUEST_CONTEXT]: `RequestContext<T> : MessageContext` adds `.Params`, `.MatchedPrimitive`, `.JsonRpcRequest`, `EnablePollingAsync(TimeSpan)`, and constructs from `(McpServer, JsonRpcRequest, T)`; `.Server` is inherited from `MessageContext`, never a direct `RequestContext<T>` member.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: server registration extensions on `IMcpServerBuilder`; `AddMcpServer` seeds the builder, `WithStdioServerTransport` ships in the host `ModelContextProtocol` package beside `WithTools`/`WithPrompts`/`WithResources`.

| [INDEX] | [SURFACE]                                                               | [SHAPE] | [CAPABILITY]                           |
| :-----: | :---------------------------------------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `AddMcpServer(IServiceCollection)`                                      | static  | seeds `IMcpServerBuilder`              |
|  [02]   | `WithTools<TToolType>`                                                  | static  | attributed `[McpServerTool]` discovery |
|  [03]   | `WithTools(IEnumerable<McpServerTool>)`                                 | static  | programmatic tool registration         |
|  [04]   | `WithPrompts<TPromptType>`                                              | static  | attributed prompt discovery            |
|  [05]   | `WithResources<TResourceType>`                                          | static  | attributed resource discovery          |
|  [06]   | `WithToolsFromAssembly(Assembly?)`                                      | static  | assembly tool discovery                |
|  [07]   | `WithListToolsHandler(McpRequestHandler<..>)`                           | static  | list-tools handler                     |
|  [08]   | `WithCallToolHandler(McpRequestHandler<..>)`                            | static  | call-tool handler                      |
|  [09]   | `WithHttpTransport(Action<HttpServerTransportOptions>?)`                | static  | HTTP and SSE transport                 |
|  [10]   | `MapMcp(string pattern)`                                                | static  | endpoint registration; default `""`    |
|  [11]   | `AddMcp(AuthenticationBuilder)`                                         | static  | authentication scheme                  |
|  [12]   | `McpServerTool.Create(AIFunction, McpServerToolCreateOptions?)`         | factory | function-backed tool                   |
|  [13]   | `McpServerTool.Create(Delegate, McpServerToolCreateOptions?)`           | factory | delegate-backed tool                   |
|  [14]   | `WithStdioServerTransport`                                              | static  | stdio transport                        |
|  [15]   | `WithPrompts(IEnumerable<McpServerPrompt>)`                             | static  | programmatic prompt registration       |
|  [16]   | `WithResources(IEnumerable<McpServerResource>)`                         | static  | programmatic resource registration     |
|  [17]   | `McpServerPrompt.Create(AIFunction, McpServerPromptCreateOptions?)`     | factory | function-backed prompt                 |
|  [18]   | `McpServerResource.Create(AIFunction, McpServerResourceCreateOptions?)` | factory | function-backed resource               |
|  [19]   | `WithMessageFilters(Action<IMcpMessageFilterBuilder>)`                  | static  | opens the message-filter builder       |
|  [20]   | `WithRequestFilters(Action<IMcpRequestFilterBuilder>)`                  | static  | opens the request-filter builder       |
|  [21]   | `WithDistributedCacheEventStreamStore(Action<…Options>?)`               | static  | durable SSE replay over a cache        |
|  [22]   | `AddAuthorizationFilters(IMcpServerBuilder)`                            | static  | per-primitive authorization filters    |

- All three primitive families expose the same four `Create` overloads — `(AIFunction, …Options?)`, `(Delegate, …Options?)`, `(MethodInfo, object?, …Options?)`, and `(MethodInfo, Func<RequestContext<T>, object>, …Options?)`; the `AIFunction` form is the programmatic mint and the `MethodInfo` forms serve attributed discovery.
- `McpServerTool.Create(AIFunction, McpServerToolCreateOptions?)` MARSHALS the function's return into `CallToolResult` through a total ladder, so no CLR instance survives the protocol boundary: an `AIContent` becomes one `ContentBlock`; a `string` becomes a `TextContentBlock`; a `ContentBlock`, `IEnumerable<ContentBlock>`, or `IEnumerable<AIContent>` passes through; a `CallToolResult` returns as-is; ANY other value — a domain record included — serializes to one `TextContentBlock` under `AIFunction.JsonSerializerOptions` beside the computed `StructuredContent`. A domain return therefore crosses ONLY as JSON, published under the tool's declared `OutputSchema` with `UseStructuredContent = true`, and a remote caller reconstructs it from `CallToolResult.StructuredContent`. The symmetric client fact: `McpClientTool.InvokeCoreAsync` yields an `AIContent`, an `AIContent[]`, or `JsonSerializer.SerializeToElement(callToolResult, …)` — never a domain object — so a federated tool structurally cannot carry an exact host receipt.
- `McpServerPromptCreateOptions` and `McpServerResourceCreateOptions` share `Services`/`Name`/`Title`/`Description`/`SerializerOptions`/`SchemaCreateOptions`/`Metadata`/`Icons`/`Meta`; the resource options add `UriTemplate` and `MimeType`.

[ENTRYPOINT_SCOPE]: client construction and calls; `McpClient.CreateAsync` is the sole construction point, and every session call trails `RequestOptions?` and `CancellationToken`.

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `McpClient.CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?)` | factory  | `-> Task<McpClient>`, initialized       |
|  [02]   | `McpClient.ResumeSessionAsync(IClientTransport, ResumeClientSessionOptions)`  | factory  | detached-session resumption             |
|  [03]   | `McpClient.ListToolsAsync()`                                                  | instance | `-> IList<McpClientTool>`               |
|  [04]   | `McpClient.CallToolAsync(string, IReadOnlyDictionary, IProgress)`             | instance | `-> ValueTask<CallToolResult>`          |
|  [05]   | `McpClient.ListPromptsAsync()`                                                | instance | `-> IList<McpClientPrompt>`             |
|  [06]   | `McpClient.GetPromptAsync(string, IReadOnlyDictionary?)`                      | instance | `-> GetPromptResult`                    |
|  [07]   | `McpClient.ListResourcesAsync()`                                              | instance | `-> IList<McpClientResource>`           |
|  [08]   | `McpClient.SubscribeToResourceAsync(string)`                                  | instance | resource-update subscription            |
|  [09]   | `McpClient.ListResourceTemplatesAsync()`                                      | instance | `-> IList<McpClientResourceTemplate>`   |
|  [10]   | `McpClientTool.InvokeAsync(AIFunctionArguments?)`                             | instance | `AIFunction` invocation                 |
|  [11]   | `McpClientOptions.ClientInfo`                                                 | property | initialize identity (`Implementation?`) |
|  [12]   | `McpClientTool.InvokeCoreAsync(AIFunctionArguments, CancellationToken)`       | instance | peer result as content or JSON only     |

- `McpClient.SubscribeToResourceAsync`: its `Func<ResourceUpdatedNotificationParams, CancellationToken, ValueTask>` handler overload returns `Task<IAsyncDisposable>` and registers a per-URI update handler.
- `ResumeClientSessionOptions` carries the detached session's `ServerCapabilities` and `ServerInfo`; a null option, capability set, or server info faults `ArgumentNullException`, so a resuming host persists the handshake facts beside the session id.

[ENTRYPOINT_SCOPE]: server configuration on `McpServerOptions`.

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `McpServerOptions.ServerInfo`                  | property | server identity (`Implementation`)                    |
|  [02]   | `McpServerOptions.Capabilities`                | property | advertised `ServerCapabilities`                       |
|  [03]   | `McpServerOptions.ProtocolVersion`             | property | date-versioned protocol string                        |
|  [04]   | `McpServerOptions.InitializationTimeout`       | property | handshake timeout; 60s default                        |
|  [05]   | `McpServerOptions.ServerInstructions`          | property | client instructions                                   |
|  [06]   | `McpServerOptions.ToolCollection`              | property | `McpServerPrimitiveCollection<McpServerTool>`         |
|  [07]   | `McpServerOptions.PromptCollection`            | property | `McpServerPrimitiveCollection<McpServerPrompt>`       |
|  [08]   | `McpServerOptions.ResourceCollection`          | property | `McpServerResourceCollection`                         |
|  [09]   | `McpServerOptions.TaskStore`                   | property | `IMcpTaskStore?`; `InMemoryMcpTaskStore` when polling |
|  [10]   | `McpServerOptions.SendTaskStatusNotifications` | property | emits `notifications/tasks/status`                    |

[ENTRYPOINT_SCOPE]: `McpServer` session long-running verbs; server-initiated legs require a stateful session and trail `RequestOptions?`/`CancellationToken`.

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `McpServer.SampleAsync(CreateMessageRequestParams)`                   | instance | server LLM sampling `-> CreateMessageResult` |
|  [02]   | `McpServer.SampleAsync(IEnumerable<ChatMessage>, ChatOptions?)`       | instance | chat sampling `-> Task<ChatResponse>`        |
|  [03]   | `McpServer.AsSamplingChatClient(JsonSerializerOptions?)`              | instance | sampling-to-`IChatClient` bridge             |
|  [04]   | `McpServer.ElicitAsync(ElicitRequestParams)`                          | instance | structured mid-call input `-> ElicitResult`  |
|  [05]   | `McpServer.ElicitAsync<T>(string message)`                            | instance | typed mid-call input `-> ElicitResult<T>`    |
|  [06]   | `McpServer.GetTaskAsync(string)`                                      | instance | task status `-> McpTask`                     |
|  [07]   | `McpServer.GetTaskResultAsync<T>(string, JsonSerializerOptions?)`     | instance | stored result `-> T?`                        |
|  [08]   | `McpServer.WaitForTaskResultAsync<T>(string, JsonSerializerOptions?)` | instance | terminal result `-> (McpTask, T?)`           |
|  [09]   | `McpServer.PollTaskUntilCompleteAsync(string)`                        | instance | terminal-status poll `-> McpTask`            |

- `McpServer.ElicitAsync<T>`: binds the result to a source-generated schema for `T`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `McpSession : IAsyncDisposable`; `McpServer` and `McpClient` both extend it.
- `McpServerTool`, `McpServerPrompt`, `McpServerResource` all implement `IMcpServerPrimitive`; `[McpServerTool]` on public methods drives reflection-based tool registration.
- `IMcpTaskStore`/`InMemoryMcpTaskStore` back the out-of-band task-result protocol.

[STACKING]:
- `Microsoft.Extensions.AI.Abstractions`(`libs/csharp/.api/api-extensions-ai.md`): `McpClientTool : AIFunction`, so server tools surface as MEAI `AIFunction` instances registered in `ChatOptions.Tools`; `AIFunctionMcpServerTool` wraps a MEAI `AIFunction` as a server-side tool, and `AIContentExtensions` bridges `AIContent` across the boundary.
- `Microsoft.Extensions.AI.Abstractions`(`libs/csharp/.api/api-extensions-ai.md`): `McpServer.AsSamplingChatClient(JsonSerializerOptions?)` projects the server-sampling leg as an `IChatClient` the in-process reasoning loop reuses; it is declared on `McpServer` (host/Core), never an `Microsoft.Extensions.AI` member.
- within-host DI: `AddMcpServer().WithTools(...)/WithHttpTransport(...)` folds server registration into the host `IServiceCollection`, and `McpJsonUtilities.DefaultOptions` supplies the canonical `JsonSerializerOptions` at every boundary.

[LOCAL_ADMISSION]:
- `services.AddMcpServer()` seeds `IMcpServerBuilder`; `WithHttpTransport()` attaches HTTP/SSE; `MapMcp(pattern)` registers the endpoint; `authBuilder.AddMcp()` registers the scheme and `McpAuthenticationHandler` implements token exchange.
- `McpServerOptions.TaskStore` and `SendTaskStatusNotifications` arm the out-of-band task protocol; a tool method opts a request into polling through `RequestContext<T>.EnablePollingAsync(interval, ct)`, and the client drives the result through `GetTaskAsync`/`WaitForTaskResultAsync<T>`/`PollTaskUntilCompleteAsync`.
- `SampleAsync` and `ElicitAsync`/`ElicitAsync<T>` require a stateful session; over HTTP they force `HttpServerTransportOptions.Stateless = false`.
- Resumability and cross-instance continuity are TRANSPORT options, not host state: `WithHttpTransport(o => { o.EventStreamStore = …; o.SessionMigrationHandler = …; })` arms `Last-Event-ID` replay and session migration, and the client re-attaches through `McpClient.ResumeSessionAsync`.
- `WithDistributedCacheEventStreamStore(...)` is the store owner wherever an `IDistributedCache` is already composed — a hand-written `ISseEventStreamStore` earns its seat only against a backing store the package does not reach.

[RAIL_LAW]:
- Package: `ModelContextProtocol.Core`, `ModelContextProtocol`, `ModelContextProtocol.AspNetCore`
- Owns: MCP session, server primitives, client tools, transport selection, DI registration, HTTP hosting, SSE event persistence and session resumption, and the server long-running task/sampling/elicitation surface.
- Accept: session-scoped calls through `McpServer`/`McpClient`; tool invocation through `McpClientTool.InvokeAsync`; long-running calls through `RequestContext<T>.EnablePollingAsync` plus the `McpServer` task verbs; resumability through the shipped `DistributedCacheEventStreamStore` or an `ISseEventStreamStore` over a store it cannot reach.
- Reject: hand-rolled JSON-RPC framing; out-of-session protocol message construction; a host-owned frame buffer, replay cursor, or resume token beside `ISseEventStreamStore`; expecting a domain CLR instance to survive `McpServerTool.Create`'s marshalling ladder; reading `.Server`/`.Services`/`.User`/`.Items` as direct `RequestContext<T>` members; filing `AsSamplingChatClient` under the MEAI catalogue.
