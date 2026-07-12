# [RASM_APPHOST_API_MCP]

`ModelContextProtocol.Core` owns the MCP session, client, server, transport, and primitive surfaces; `ModelContextProtocol` binds DI composition, builder extensions, and hosted-service plumbing; `ModelContextProtocol.AspNetCore` binds HTTP transport, SSE event-stream persistence, and authentication handler registration for ASP.NET Core hosts.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ModelContextProtocol.Core`
- package: `ModelContextProtocol.Core`
- assembly: `ModelContextProtocol`
- namespace: `ModelContextProtocol`, `ModelContextProtocol.Client`, `ModelContextProtocol.Server`, `ModelContextProtocol.Protocol`
- asset: runtime library
- rail: mcp-protocol

[PACKAGE_SURFACE]: `ModelContextProtocol`
- package: `ModelContextProtocol`
- assembly: `ModelContextProtocol`
- namespace: `ModelContextProtocol`, `ModelContextProtocol.Server`, `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: mcp-host

[PACKAGE_SURFACE]: `ModelContextProtocol.AspNetCore`
- package: `ModelContextProtocol.AspNetCore`
- assembly: `ModelContextProtocol.AspNetCore`
- namespace: `ModelContextProtocol.AspNetCore`, `ModelContextProtocol.AspNetCore.Authentication`, `Microsoft.AspNetCore.Builder`, `Microsoft.Extensions.DependencyInjection`
- asset: runtime library
- rail: mcp-host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: session and protocol primitives — `ModelContextProtocol.Core`
- rail: mcp-protocol

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

[PROGRESS_NOTIFICATION_VALUE]:
- Progress: `required float Progress`.
- Total: `float? Total`.
- Message: `string? Message`.
- Setter: `init`.

[IMPLEMENTATION]:
- Namespace: `ModelContextProtocol.Protocol`.
- Required identity: `required string Name` and `required string Version`.
- Title: `Title`.
- Consumers: `McpClientOptions.ClientInfo` and `McpServerOptions.ServerInfo`.

[PUBLIC_TYPE_SCOPE]: server primitives — `ModelContextProtocol.Server`
- rail: mcp-protocol

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

[TOOL_CREATE_OPTIONS]:
- Identity: `Name`, `Title`, and `Description`.
- Safety annotations: `ReadOnly`, `Destructive`, `Idempotent`, and `OpenWorld`.
- Structured output: `UseStructuredContent` and `OutputSchema`.
- Serializer and schema policy: `SerializerOptions` and `SchemaCreateOptions`.
- Metadata: `Metadata`.
- Services: `Services`.

[PUBLIC_TYPE_SCOPE]: client primitives — `ModelContextProtocol.Client`
- rail: mcp-protocol

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :------------------------------- | :------------ | :------------------------------ |
|  [01]   | `McpClientTool`                  | sealed class  | server-tool function adapter    |
|  [02]   | `McpClientPrompt`                | sealed class  | prompt accessor                 |
|  [03]   | `McpClientResource`              | sealed class  | resource accessor               |
|  [04]   | `McpClientResourceTemplate`      | sealed class  | resource-template accessor      |
|  [05]   | `McpClientOptions`               | class         | client configuration            |
|  [06]   | `McpClientHandlers`              | class         | notification-handler registry   |
|  [07]   | `IClientTransport`               | interface     | session-transport factory       |
|  [08]   | `HttpClientTransport`            | sealed class  | HTTP session transport          |
|  [09]   | `StdioClientTransport`           | sealed class  | stdio session transport         |
|  [10]   | `StreamClientTransport`          | sealed class  | paired-stream session transport |
|  [11]   | `StdioClientTransportOptions`    | class         | stdio transport configuration   |
|  [12]   | `HttpClientTransportOptions`     | class         | HTTP transport configuration    |
|  [13]   | `HttpTransportMode`              | enum          | HTTP session-transport mode     |
|  [14]   | `ClientTransportClosedException` | class         | transport-closed failure        |

[CLIENT_TOOL]:
- Base: `AIFunction`.
- Payload: server tool.

[HTTP_CLIENT_TRANSPORT]:
- Contract: `IClientTransport`.
- Selector: `HttpClientTransportOptions.TransportMode`.
- Modes: streamable HTTP and SSE.

[STDIO_CLIENT_TRANSPORT]:
- Contract: `IClientTransport`.

[STREAM_CLIENT_TRANSPORT]:
- Contract: `IClientTransport`.
- Position: third public implementor.
- Namespace: `ModelContextProtocol.Protocol`.
- Constructor: `(Stream serverInput, Stream serverOutput, ILoggerFactory?)`.

[STDIO_CLIENT_TRANSPORT_OPTIONS]:
- Required: `required string Command`.
- Additional: `Arguments` and `Name`.

[HTTP_CLIENT_TRANSPORT_OPTIONS]:
- Required: `required Uri Endpoint`.
- Mode: `HttpTransportMode TransportMode`.
- Name: `Name`.

[HTTP_TRANSPORT_MODE]:
- Values: `AutoDetect`, `StreamableHttp`, and `Sse`.
- Selection: `HttpClientTransport` connect-time session transport.

[PUBLIC_TYPE_SCOPE]: DI and builder — `Microsoft.Extensions.DependencyInjection` (in `ModelContextProtocol`)
- rail: mcp-host

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :---------------------------------- | :------------ | :-------------------------------- |
|  [01]   | `IMcpServerBuilder`                 | interface     | server builder contract           |
|  [02]   | `IMcpMessageFilterBuilder`          | interface     | message-filter builder contract   |
|  [03]   | `IMcpRequestFilterBuilder`          | interface     | request-filter builder contract   |
|  [04]   | `McpServerBuilderExtensions`        | static class  | server primitive registration     |
|  [05]   | `McpMessageFilterBuilderExtensions` | static class  | message-filter registration       |
|  [06]   | `McpRequestFilterBuilderExtensions` | static class  | per-operation filter registration |

[SERVER_BUILDER_EXTENSIONS]: `WithTools`, `WithPrompts`, and `WithResources`.

[MESSAGE_FILTER_BUILDER_EXTENSIONS]: `AddIncomingFilter` and `AddOutgoingFilter`.

[PUBLIC_TYPE_SCOPE]: ASP.NET Core host — `ModelContextProtocol.AspNetCore`
- rail: mcp-host

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :---------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `HttpServerTransportOptions`        | class         | HTTP server transport configuration            |
|  [02]   | `ISessionMigrationHandler`          | interface     | session migration between instances contract   |
|  [03]   | `McpEndpointRouteBuilderExtensions` | static class  | `MapMcp(pattern)` route registration           |
|  [04]   | `HttpMcpServerBuilderExtensions`    | static class  | `WithHttpTransport`, `AddAuthorizationFilters` |
|  [05]   | `McpAuthenticationExtensions`       | static class  | `AddMcp` authentication scheme builder         |
|  [06]   | `McpAuthenticationDefaults`         | static class  | scheme name constants                          |
|  [07]   | `McpAuthenticationOptions`          | class         | extends `AuthenticationSchemeOptions`          |
|  [08]   | `McpAuthenticationHandler`          | class         | `AuthenticationHandler` implementation         |
|  [09]   | `ResourceMetadataRequestContext`    | class         | resource metadata request context              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: server registration
- rail: mcp-host

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY]  | [CAPABILITY]                   |
| :-----: | :--------------------------------- | :-------------- | :----------------------------- |
|  [01]   | `AddMcpServer`                     | DI registration | server registration            |
|  [02]   | `WithTools<TToolType>`             | builder fluent  | attributed tool discovery      |
|  [03]   | `WithTools`                        | builder fluent  | programmatic tool registration |
|  [04]   | `WithPrompts<TPromptType>`         | builder fluent  | attributed prompt discovery    |
|  [05]   | `WithResources<TResourceType>`     | builder fluent  | attributed resource discovery  |
|  [06]   | `WithToolsFromAssembly`            | builder fluent  | assembly tool discovery        |
|  [07]   | `WithListToolsHandler`             | builder fluent  | list-tools handler             |
|  [08]   | `WithCallToolHandler`              | builder fluent  | call-tool handler              |
|  [09]   | `WithHttpTransport`                | builder fluent  | HTTP and SSE transport         |
|  [10]   | `MapMcp`                           | routing         | endpoint registration          |
|  [11]   | `AddMcp`                           | auth            | authentication scheme          |
|  [12]   | `McpServerTool.Create(AIFunction)` | static factory  | function-backed tool           |
|  [13]   | `McpServerTool.Create(Delegate)`   | static factory  | delegate-backed tool           |
|  [14]   | `WithStdioServerTransport`         | builder fluent  | stdio transport                |

[SERVER_REGISTRATION_SIGNATURES]:
- `AddMcpServer(this IServiceCollection)`
- `WithTools<TToolType>(this IMcpServerBuilder)`
- `WithTools(this IMcpServerBuilder, IEnumerable<McpServerTool>)`
- `WithPrompts<TPromptType>(this IMcpServerBuilder)`
- `WithResources<TResourceType>(this IMcpServerBuilder)`
- `WithToolsFromAssembly(this IMcpServerBuilder, Assembly?)`
- `WithListToolsHandler(this IMcpServerBuilder, McpRequestHandler<..>)`
- `WithCallToolHandler(this IMcpServerBuilder, McpRequestHandler<..>)`
- `WithHttpTransport(this IMcpServerBuilder, ...)`
- `MapMcp(this IEndpointRouteBuilder, pattern)`
- `AddMcp(this AuthenticationBuilder, ...)`
- `McpServerTool.Create(AIFunction, McpServerToolCreateOptions?)`
- `McpServerTool.Create(Delegate, McpServerToolCreateOptions?)`
- `WithStdioServerTransport(this IMcpServerBuilder)`

[SERVER_REGISTRATION_CONTRACTS]:
- `AddMcpServer`: Returns `IMcpServerBuilder`.
- `WithTools<TToolType>`: Discovers `[McpServerTool]` methods.
- `WithStdioServerTransport`: Belongs to the host `ModelContextProtocol` package rather than `ModelContextProtocol.Core`.

[ENTRYPOINT_SCOPE]: client construction
- rail: mcp-protocol

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY]  | [CAPABILITY]                 |
| :-----: | :------------------------------------- | :-------------- | :--------------------------- |
|  [01]   | `McpClient.CreateAsync`                | static factory  | initialized client           |
|  [02]   | `McpClient.ResumeSessionAsync`         | static factory  | detached-session resumption  |
|  [03]   | `McpClient.ListToolsAsync`             | session call    | server tool enumeration      |
|  [04]   | `McpClient.CallToolAsync`              | session call    | named tool invocation        |
|  [05]   | `McpClient.ListPromptsAsync`           | session call    | server prompt enumeration    |
|  [06]   | `McpClient.GetPromptAsync`             | session call    | prompt content retrieval     |
|  [07]   | `McpClient.ListResourcesAsync`         | session call    | server resource enumeration  |
|  [08]   | `McpClient.SubscribeToResourceAsync`   | session call    | resource update subscription |
|  [09]   | `McpClient.ListResourceTemplatesAsync` | session call    | template enumeration         |
|  [10]   | `McpClientTool.InvokeAsync`            | tool call       | `AIFunction` invocation      |
|  [11]   | `McpClientOptions.ClientInfo`          | option property | initialize identity          |

[CLIENT_CONSTRUCTION_SIGNATURES]:
- `McpClient.CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?, CT)`
- `McpClient.ResumeSessionAsync(IClientTransport, ResumeClientSessionOptions, ...)`
- `McpClient.ListToolsAsync(RequestOptions?, CT)`
- `McpClient.CallToolAsync(string toolName, IReadOnlyDictionary<string,object?>? args, IProgress<ProgressNotificationValue>? progress, RequestOptions?, CT)`
- `McpClient.ListPromptsAsync(RequestOptions?, CT)`
- `McpClient.GetPromptAsync(name, args, RequestOptions?)`
- `McpClient.ListResourcesAsync(RequestOptions?, CT)`
- `McpClient.SubscribeToResourceAsync(string uri, RequestOptions?, CT)`
- `SubscribeToResourceAsync(string uri, Func<ResourceUpdatedNotificationParams,CancellationToken,ValueTask> handler, RequestOptions?, CT)`
- `McpClient.ListResourceTemplatesAsync(RequestOptions?, CT)`
- `McpClientTool.InvokeAsync(args, CT)`

[CALL_TOOL]:
- Return: `ValueTask<CallToolResult>`.
- Position: `IProgress<ProgressNotificationValue>? progress` precedes `RequestOptions?`.
- Binding: `progress`, `options`, and `cancellationToken` use named arguments.

[SUBSCRIBE_TO_RESOURCE]:
- Base overload: Returns `Task`.
- Handler overload: Registers the per-URI update handler and returns `Task<IAsyncDisposable>`.
- Position: `handler` precedes `RequestOptions?`.
- Binding: `options` and `cancellationToken` use named arguments.

[LIST_RESOURCE_TEMPLATES]:
- Return: `ValueTask<IList<McpClientResourceTemplate>>`.
- Template: `McpClientResourceTemplate` carries `UriTemplate`.

[CLIENT_INFO]:
- Binding: `CreateAsync` options.
- Type: `Implementation?`.
- Identity: `Name` and `Version`.
- Exchange: initialize handshake.

[ENTRYPOINT_SCOPE]: server options surface
- rail: mcp-protocol

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY]  | [CAPABILITY]                  |
| :-----: | :--------------------------------------------- | :-------------- | :---------------------------- |
|  [01]   | `McpServerOptions.ServerInfo`                  | option property | server identity               |
|  [02]   | `McpServerOptions.Capabilities`                | option property | advertised capabilities       |
|  [03]   | `McpServerOptions.ProtocolVersion`             | option property | protocol version              |
|  [04]   | `McpServerOptions.InitializationTimeout`       | option property | handshake timeout             |
|  [05]   | `McpServerOptions.ServerInstructions`          | option property | client instructions           |
|  [06]   | `McpServerOptions.ToolCollection`              | option property | tool collection               |
|  [07]   | `McpServerOptions.PromptCollection`            | option property | prompt collection             |
|  [08]   | `McpServerOptions.ResourceCollection`          | option property | resource collection           |
|  [09]   | `McpServerOptions.TaskStore`                   | option property | out-of-band task persistence  |
|  [10]   | `McpServerOptions.SendTaskStatusNotifications` | option property | task-status notification flag |

[SERVER_INFO]: `Implementation` name and version.

[SERVER_CAPABILITIES]: `ServerCapabilities` advertised to clients.

[PROTOCOL_VERSION]: Date-versioned protocol string.

[INITIALIZATION_TIMEOUT]: 60-second default.

[SERVER_COLLECTIONS]:
- Tools: `McpServerPrimitiveCollection<McpServerTool>`.
- Prompts: `McpServerPrimitiveCollection<McpServerPrompt>`.
- Resources: `McpServerResourceCollection`.

[TASK_STORE]:
- Type: `IMcpTaskStore?`.
- Payload: out-of-band task results.
- Default: `InMemoryMcpTaskStore` when polling is enabled.

[TASK_STATUS_NOTIFICATIONS]:
- Type: `bool`.
- Event: `notifications/tasks/status`.
- Timing: long-running task advancement.

[ENTRYPOINT_SCOPE]: server session long-running verbs — `ModelContextProtocol.Server` (`McpServer`)
- rail: mcp-protocol

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [CAPABILITY]                  |
| :-----: | :------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `McpServer.SampleAsync`                | server→client  | server-initiated LLM sampling |
|  [02]   | `McpServer.SampleAsync`                | server→client  | chat-client sampling          |
|  [03]   | `McpServer.AsSamplingChatClient`       | bridge         | sampling chat-client adapter  |
|  [04]   | `McpServer.ElicitAsync`                | server→client  | structured mid-call input     |
|  [05]   | `McpServer.ElicitAsync<T>`             | server→client  | typed mid-call input          |
|  [06]   | `McpServer.GetTaskAsync`               | task poll      | task status                   |
|  [07]   | `McpServer.GetTaskResultAsync<T>`      | task poll      | stored task result            |
|  [08]   | `McpServer.WaitForTaskResultAsync<T>`  | task await     | terminal task result          |
|  [09]   | `McpServer.PollTaskUntilCompleteAsync` | task poll      | terminal-status poll loop     |

[SERVER_SESSION_SIGNATURES]:
- `McpServer.SampleAsync(CreateMessageRequestParams, CT)`
- `McpServer.SampleAsync(IEnumerable<ChatMessage>, ChatOptions?, JsonSerializerOptions?, CT)`
- `McpServer.AsSamplingChatClient(JsonSerializerOptions?)`
- `McpServer.ElicitAsync(ElicitRequestParams, CT)`
- `McpServer.ElicitAsync<T>(string message, RequestOptions?, CT)`
- `McpServer.GetTaskAsync(string taskId, CT)`
- `McpServer.GetTaskAsync(string, string? sessionId, CT)`
- `McpServer.GetTaskResultAsync<T>(string taskId, JsonSerializerOptions?, CT)`
- `McpServer.WaitForTaskResultAsync<T>(string taskId, JsonSerializerOptions?, CT)`
- `McpServer.PollTaskUntilCompleteAsync(string taskId, CT)`
- `McpServer.PollTaskUntilCompleteAsync(string, RequestOptions?, CT)`

[SERVER_SESSION_RETURNS]:
- `SampleAsync(CreateMessageRequestParams, CT)`: `ValueTask<CreateMessageResult>`.
- `SampleAsync(IEnumerable<ChatMessage>, ChatOptions?, JsonSerializerOptions?, CT)`: `Task<ChatResponse>`.
- `AsSamplingChatClient(JsonSerializerOptions?)`: `IChatClient`.
- `ElicitAsync(ElicitRequestParams, CT)`: `ValueTask<ElicitResult>`.
- `ElicitAsync<T>(string message, RequestOptions?, CT)`: `ValueTask<ElicitResult<T>>` bound to a generated schema.
- `GetTaskAsync(string taskId, CT)`: `ValueTask<McpTask>` carrying current status.
- `GetTaskAsync(string, string? sessionId, CT)`: `Task<McpTask?>`.
- `GetTaskResultAsync<T>(string taskId, JsonSerializerOptions?, CT)`: `ValueTask<T?>` decoding the stored payload.
- `WaitForTaskResultAsync<T>(string taskId, JsonSerializerOptions?, CT)`: `ValueTask<(McpTask Task, T? Result)>` after terminal status.
- `PollTaskUntilCompleteAsync`: `ValueTask<McpTask>` after terminal status.

[PUBLIC_TYPE_SCOPE]: request-context surface — `ModelContextProtocol.Server`
- rail: mcp-protocol

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :------------------ | :------------ | :-------------------- |
|  [01]   | `MessageContext`    | base class    | shared request state  |
|  [02]   | `RequestContext<T>` | sealed class  | typed request context |

[MESSAGE_CONTEXT]:
- Server: `.Server` (`McpServer`).
- Services: `.Services` (`IServiceProvider?`).
- User: `.User` (`ClaimsPrincipal?`).
- Items: `.Items` (`IDictionary<object,object>?`).

[REQUEST_CONTEXT]:
- Base: `MessageContext`.
- Parameters: `.Params` (`T?`).
- Primitive: `.MatchedPrimitive`.
- Request: `.JsonRpcRequest`.
- Polling: `EnablePollingAsync(TimeSpan, CT)`.
- Constructor: `(McpServer, JsonRpcRequest, T)`.

## [04]-[IMPLEMENTATION_LAW]

[SESSION_TOPOLOGY]:
- namespace roots: `ModelContextProtocol`, `ModelContextProtocol.Client`, `ModelContextProtocol.Server`
- session base: `McpSession : IAsyncDisposable`; both `McpServer` and `McpClient` extend it
- tool integration: `McpClientTool : AIFunction` — client-side tools surface as `AIFunction` instances
- primitive base: `McpServerTool`, `McpServerPrompt`, `McpServerResource` all implement `IMcpServerPrimitive`
- attribute discovery: `[McpServerTool]` on public methods drives reflection-based tool registration
- task persistence: `IMcpTaskStore` / `InMemoryMcpTaskStore` back the out-of-band task-result protocol; `McpServerOptions.TaskStore` wires the store and `SendTaskStatusNotifications` toggles `notifications/tasks/status`
- request context: `MessageContext` is the declaring owner of `.Server`/`.Services`/`.User`/`.Items`; `RequestContext<T> : MessageContext` adds `.Params`/`.MatchedPrimitive`/`.JsonRpcRequest`/`EnablePollingAsync(TimeSpan, CT)` and arrives through the `(McpServer, JsonRpcRequest, T)` ctor — never read `.Server` as a direct `RequestContext<T>` property, it is inherited from `MessageContext`
- server long-running verbs: `McpServer.SampleAsync` / `AsSamplingChatClient` / `ElicitAsync` / `ElicitAsync<T>` / `GetTaskAsync` / `GetTaskResultAsync<T>` / `WaitForTaskResultAsync<T>` / `PollTaskUntilCompleteAsync` are `McpServer` session members; `AsSamplingChatClient` is the SDK server-sampling-to-`IChatClient` bridge declared on `McpServer` (host/Core), NOT an `Microsoft.Extensions.AI` member, and is filed here, never in `api-extensions-ai.md`
- stdio transport attribution: `WithStdioServerTransport(IMcpServerBuilder)` is declared on `Microsoft.Extensions.DependencyInjection.McpServerBuilderExtensions` in the **host `ModelContextProtocol`** package beside `WithTools`/`WithPrompts`/`WithResources`, NOT in `ModelContextProtocol.Core`; the `StdioServerTransport`/`StreamServerTransport` types are Core, the builder shortcut is host-package

[LOCAL_ADMISSION]:
- DI registration entry: `services.AddMcpServer()` returns `IMcpServerBuilder`; `WithHttpTransport()` attaches HTTP/SSE
- HTTP host entry: `MapMcp(pattern)` registers the endpoint; default pattern is `""`
- Auth extension: `authBuilder.AddMcp()` registers scheme; `McpAuthenticationHandler` implements token exchange
- Client construction: `McpClient.CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?, CT)` is the sole construction point; `ResumeSessionAsync` resumes a detached session
- JSON options: `McpJsonUtilities.DefaultOptions` carries the canonical `JsonSerializerOptions`; use it at boundaries
- long-running task admission: `McpServerOptions.TaskStore` + `SendTaskStatusNotifications` arm the out-of-band task protocol; a tool method opts a request into polling through `RequestContext<T>.EnablePollingAsync(interval, ct)`, and a client drives the result through `McpServer.GetTaskAsync`/`WaitForTaskResultAsync<T>`/`PollTaskUntilCompleteAsync`
- server-initiated legs: `SampleAsync` (server→client LLM sampling) and `ElicitAsync`/`ElicitAsync<T>` (mid-call structured input) require a stateful session — over HTTP they force `HttpServerTransportOptions.Stateless = false`; `AsSamplingChatClient(JsonSerializerOptions?)` projects the sampling leg as an `IChatClient` the in-process reasoning loop reuses

[RAIL_LAW]:
- Package: `ModelContextProtocol.Core` + `ModelContextProtocol` + `ModelContextProtocol.AspNetCore`
- Owns: MCP session, server primitives, client tools, transport selection, DI registration, HTTP hosting, the server long-running task/sampling/elicitation session surface
- Accept: session-scoped calls through `McpServer`/`McpClient`; tool invocation through `McpClientTool.InvokeAsync`; long-running calls through `RequestContext<T>.EnablePollingAsync` + the `McpServer` task verbs
- Reject: hand-rolled JSON-RPC framing, out-of-session protocol message construction, reading `.Server`/`.Services`/`.User`/`.Items` as direct `RequestContext<T>` members (they are inherited from `MessageContext`), filing `AsSamplingChatClient` under the MEAI catalogue
