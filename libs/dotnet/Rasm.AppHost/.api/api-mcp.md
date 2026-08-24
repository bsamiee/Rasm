# [RASM_APPHOST_API_MCP]

`ModelContextProtocol.Core` owns the MCP session, client, server, transport, and primitive surfaces; `ModelContextProtocol` binds DI composition, builder extensions, and hosted-service plumbing; `ModelContextProtocol.AspNetCore` binds HTTP transport and ASP.NET Core authentication-handler registration. The served protocol revision is `2026-07-28`: a client discovers through `server/discover`, HTTP serving is stateless, and a handler needing client input suspends through the multi-round-trip `InputRequiredException` rather than opening a server-to-client request. No public constant spells a revision literal — `McpProtocolVersions` is `internal` — so a host pins a revision through `McpServerOptions.ProtocolVersion` or its own literal.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ModelContextProtocol.Core`
- package: `ModelContextProtocol.Core`
- assembly: `ModelContextProtocol.Core`
- namespace: `ModelContextProtocol`, `ModelContextProtocol.Client`, `ModelContextProtocol.Server`, `ModelContextProtocol.Protocol`, `ModelContextProtocol.Authentication`
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

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]       | [CAPABILITY]                                          |
| :-----: | :----------------------------------------- | :------------------ | :---------------------------------------------------- |
|  [01]   | `McpSession`                               | abstract base class | shared session lifecycle                              |
|  [02]   | `McpServer`                                | abstract class      | server session                                        |
|  [03]   | `McpClient`                                | abstract class      | client session                                        |
|  [04]   | `McpException`                             | exception           | typed protocol failure                                |
|  [05]   | `McpErrorCode`                             | enum                | JSON-RPC error vocabulary                             |
|  [06]   | `RequestOptions`                           | options class       | per-request `_meta`, progress token, serializer       |
|  [07]   | `UnsupportedProtocolVersionException`      | exception           | negotiated-version refusal (`-32022`)                 |
|  [08]   | `MissingRequiredClientCapabilityException` | exception           | capability refusal (`-32021`)                         |
|  [09]   | `McpJsonUtilities`                         | static class        | serializer options and type-info                      |
|  [10]   | `AIContentExtensions`                      | static class        | bidirectional content conversion                      |
|  [11]   | `NullProgress`                             | class               | no-op `IProgress<T>`                                  |
|  [12]   | `ProgressNotificationValue`                | sealed class        | progress payload                                      |
|  [13]   | `UriTemplate`                              | class               | RFC 6570 template evaluation                          |
|  [14]   | `Implementation`                           | sealed class        | protocol identity                                     |
|  [15]   | `MetaKeys`                                 | static class        | the `io.modelcontextprotocol/*` `_meta` key constants |

[META_KEYS]: `ProtocolVersion` `ClientInfo` `ServerInfo` `ClientCapabilities` `LogLevel` `SubscriptionId` — the per-request `_meta` slots carrying what the removed session handshake once held; server identity reads `Meta[MetaKeys.ServerInfo]`, never a result property.

[PUBLIC_TYPE_SCOPE]: multi-round-trip input (MRTR) — the ONLY route from a running handler back to the client

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `InputRequest`           | sealed class  | one `{ method, params }` ask the client resolves locally |
|  [02]   | `InputResponse`          | sealed class  | the client's answer, deserialized by expected type       |
|  [03]   | `InputRequiredResult`    | sealed class  | the incomplete result carrying asks + `RequestState`     |
|  [04]   | `InputRequiredException` | exception     | what a handler throws to suspend and be retried          |
|  [05]   | `ResultOrAlternate<T>`   | class         | a normal result or a foreign-shaped alternate result     |

[INPUT_REQUEST]: `required string Method`, `JsonElement? Params`, with the typed views `ElicitationParams`, `SamplingParams`, `RootsParams`; mint through `InputRequest.ForElicitation(ElicitRequestParams)`, `ForSampling(CreateMessageRequestParams)`, `ForRootsList(ListRootsRequestParams)`.
[INPUT_RESPONSE]: `JsonElement RawValue` plus `Deserialize<T>(JsonTypeInfo<T>)` against the matching `ElicitResultJsonTypeInfo`/`CreateMessageResultJsonTypeInfo`/`ListRootsResultJsonTypeInfo` — the expected type follows from the paired `InputRequest.Method`, and NO wire discriminator distinguishes them; `FromElicitResult`/`FromSamplingResult`/`FromRootsResult` mint the client side.
[INPUT_REQUIRED_RESULT]: `IDictionary<string, InputRequest>? InputRequests`, `string? RequestState`, `ResultType = "input_required"`; the answers arrive on the retried call as `RequestParams.InputResponses`/`RequestState`. A result carrying `RequestState` with NO `InputRequests` is the load-shedding continuation the client auto-retries.
[RESULT_OR_ALTERNATE]: `IsAlternate`, `Result`, `Alternate`, `AlternateTypeInfo`; `ResultOrAlternate<TResult>.FromAlternate<TAlternate>(TAlternate, JsonTypeInfo<TAlternate>)` and an implicit conversion from `TResult`.
[ELICITATION_MODES]: `ElicitRequestParams` carries `Mode` (`form`/`url`), `required string Message`, `RequestedSchema` (`ElicitRequestParams.RequestSchema` — `Properties` over `PrimitiveSchemaDefinition` rows `BooleanSchema`/`StringSchema`/`NumberSchema` plus `Required`), `Url`, and `ElicitationId`; the client gate is `ElicitationCapability` with its `Form`/`Url` sub-capabilities, URL-mode completion rides `ElicitationCompleteNotificationParams` (`required string ElicitationId`), and `UrlElicitationRequiredException` (an `Elicitations` roster over `UrlElicitationRequiredErrorData`) frames the URL-elicitation-required protocol error.

[PUBLIC_TYPE_SCOPE]: discovery, cache hints, and subscription listening

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :-------------------------------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `DiscoverRequestParams`                       | sealed class  | the `server/discover` probe, no parameters                |
|  [02]   | `DiscoverResult`                              | sealed class  | supported versions, capabilities, instructions, TTL       |
|  [03]   | `ICacheableResult`                            | interface     | `TimeSpan? TimeToLive` + `CacheScope? CacheScope`         |
|  [04]   | `CacheScope`                                  | enum          | `Public` / `Private`                                      |
|  [05]   | `SubscriptionsListenRequestParams`            | sealed class  | `required SubscriptionsListenNotifications Notifications` |
|  [06]   | `SubscriptionsListenNotifications`            | sealed class  | per-kind opt-in + `ResourceSubscriptions`                 |
|  [07]   | `SubscriptionsAcknowledgedNotificationParams` | sealed class  | the supported subset the server admits                    |

[DISCOVER_RESULT]: `required IList<string> SupportedVersions`, `required ServerCapabilities Capabilities`, `string? Instructions`, plus the `ICacheableResult` pair; server identity rides `Meta[MetaKeys.ServerInfo]`.
[CACHEABLE_RESULT]: implemented by `server/discover`, `tools/list`, `prompts/list`, `resources/list`, `resources/templates/list`, and `resources/read`; a null `TimeToLive` reads as immediately stale and a null `CacheScope` as `Public`.
[SUBSCRIPTIONS_LISTEN]: `ToolsListChanged`, `PromptsListChanged`, `ResourcesListChanged`, `IList<string>? ResourceSubscriptions` — one opt-in request replaces per-resource subscribe and unsubscribe calls, and the acknowledged notification reports what the server actually honours.

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
|  [12]   | `McpHeaderAttribute`         | sealed attribute | binds a parameter or property to an HTTP header       |
|  [13]   | `StdioServerTransport`       | class            | stdio-backed server transport                         |
|  [14]   | `StreamServerTransport`      | class            | stream-backed server transport                        |
|  [15]   | `McpServerRequestHandler`    | sealed class     | a raw method handler mounted beside the primitives    |

[TOOL_CREATE_OPTIONS]:
- Identity: `Name`, `Title`, `Description`.
- Safety annotations: `ReadOnly`, `Destructive`, `Idempotent`, `OpenWorld` (nullable tri-state).
- Structured output: `UseStructuredContent`, `OutputSchema`.
- Policy: `SerializerOptions`, `SchemaCreateOptions`, `Services`, `Metadata`, `Icons`, `Meta`.

[MCP_HEADER_ATTRIBUTE]: `McpHeaderAttribute(string name)` with `string Name`, valid on a parameter or property; the name must be RFC 9110 tchar and case-insensitively unique, and the SDK mirrors it to the `Mcp-Param-{Name}` HTTP header. Admitted binding types are `string`, `integer`, and `boolean`, an integer bounded by ±2^53−1 and canonicalized to decimal.

[MCP_SERVER_REQUEST_HANDLER]: `required string Method`, `required Func<JsonRpcRequest, CancellationToken, ValueTask<JsonNode?>> Handler`, `string? RoutingNameParameter`; registered through `McpServerOptions.RequestHandlers` to serve a method the primitive families do not cover.

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

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :---------------------------------- | :------------ | :--------------------------------------- |
|  [01]   | `HttpServerTransportOptions`        | class         | HTTP server transport configuration      |
|  [02]   | `McpEndpointRouteBuilderExtensions` | static class  | `MapMcp(pattern)` route registration     |
|  [03]   | `HttpMcpServerBuilderExtensions`    | static class  | HTTP transport and authorization filters |
|  [04]   | `McpAuthenticationExtensions`       | static class  | `AddMcp` authentication scheme builder   |
|  [05]   | `McpAuthenticationDefaults`         | static class  | scheme name constants                    |
|  [06]   | `McpAuthenticationOptions`          | class         | extends `AuthenticationSchemeOptions`    |
|  [07]   | `McpAuthenticationHandler`          | class         | `AuthenticationHandler` implementation   |
|  [08]   | `ResourceMetadataRequestContext`    | class         | resource metadata request context        |

[HTTP_SERVER_TRANSPORT_OPTIONS]:
- `Stateless` (default `true`): the server mints no session, emits no `MCP-Session-Id`, serves no standalone SSE `GET` or `DELETE`, runs `ConfigureSessionOptions`/`RunSessionHandler` per request, and opens no server-to-client request at all; handler-SUSPENSION bridging is also disabled there (`IsMrtrSupported` reads `ClientSupportsMrtr() || HasStatefulTransport()`, and the transparent conversion of an interior ask into an `input_required` result demands the stateful half), so the one stateless mid-call ask is an explicitly thrown `InputRequiredException`, honored for a `2026-07-28` client and a protocol error otherwise; `McpServer.ClientCapabilities` is null per request, so `ElicitAsync` throws rather than asking.
- `ConfigureSessionOptions` (`Func<HttpContext, McpServerOptions, CancellationToken, Task>?`) is the supported seat for per-request `HttpContext` access; `RunSessionHandler` (`Func<HttpContext, McpServer, CancellationToken, Task>?`) is `[Experimental("MCPEXP002")]`.
- `TimeProvider` (default `TimeProvider.System`) drives the transport clock, so a timing assertion runs without wall-clock waits.
- A `2026-07-28` request reaching a server whose `Stateless` was forced off is refused with `-32022`, so a client holding both paths downgrades rather than half-negotiating.

[PUBLIC_TYPE_SCOPE]: request-context surface — `ModelContextProtocol.Server`

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :------------------ | :------------ | :-------------------- |
|  [01]   | `MessageContext`    | base class    | shared request state  |
|  [02]   | `RequestContext<T>` | sealed class  | typed request context |

[MESSAGE_CONTEXT]: declares `.Server`, `.Services`, `.User`, `.Items`.
[REQUEST_CONTEXT]: `RequestContext<T> : MessageContext` adds `.Params`, `.MatchedPrimitive`, `.JsonRpcRequest`, `EnablePollingAsync(TimeSpan, CancellationToken)`, and constructs from `(McpServer, JsonRpcRequest, T)`; `.Server` is inherited from `MessageContext`, never a direct `RequestContext<T>` member, and the `(McpServer, JsonRpcRequest)` ctor is `[Obsolete(DiagnosticId = "MCP9003")]`.

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
|  [09]   | `WithHttpTransport(Action<HttpServerTransportOptions>?)`                | static  | streamable-HTTP transport              |
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
|  [21]   | `AddAuthorizationFilters(IMcpServerBuilder)`                            | static  | per-primitive authorization filters    |
|  [22]   | `WithStreamServerTransport(Stream, Stream)`                             | static  | paired-stream transport                |

- All three primitive families expose the same four `Create` overloads — `(AIFunction, …Options?)`, `(Delegate, …Options?)`, `(MethodInfo, object?, …Options?)`, and `(MethodInfo, Func<RequestContext<T>, object>, …Options?)`; the `AIFunction` form is the programmatic mint and the `MethodInfo` forms serve attributed discovery.
- `McpServerTool.Create(AIFunction, McpServerToolCreateOptions?)` MARSHALS the function's return into `CallToolResult` through a total ladder, so no CLR instance survives the protocol boundary: an `AIContent` becomes one `ContentBlock`; a `string` becomes a `TextContentBlock`; a `ContentBlock`, `IEnumerable<ContentBlock>`, or `IEnumerable<AIContent>` passes through; a `CallToolResult` returns as-is; ANY other value — a domain record included — serializes to one `TextContentBlock` under `AIFunction.JsonSerializerOptions` beside the computed `StructuredContent`. A domain return therefore crosses ONLY as JSON, published under the tool's declared `OutputSchema` with `UseStructuredContent = true`, and a remote caller reconstructs it from `CallToolResult.StructuredContent`. The symmetric client fact: `McpClientTool.InvokeCoreAsync` yields an `AIContent`, an `AIContent[]`, or `JsonSerializer.SerializeToElement(callToolResult, …)` — never a domain object — so a federated tool structurally cannot carry an exact host receipt.
- `McpServerPromptCreateOptions` and `McpServerResourceCreateOptions` share `Services`/`Name`/`Title`/`Description`/`SerializerOptions`/`SchemaCreateOptions`/`Metadata`/`Icons`/`Meta`; the resource options add `UriTemplate` and `MimeType`.

[ENTRYPOINT_SCOPE]: client construction and calls; `McpClient.CreateAsync` is the sole construction point, both factories trail `McpClientOptions?`, `ILoggerFactory?`, and `CancellationToken`, and every session call trails `RequestOptions?` and `CancellationToken`.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `McpClient.CreateAsync(IClientTransport)`                                    | factory  | `-> Task<McpClient>`, initialized          |
|  [02]   | `McpClient.ResumeSessionAsync(IClientTransport, ResumeClientSessionOptions)` | factory  | detached-session resumption                |
|  [03]   | `McpClient.ListToolsAsync()`                                                 | instance | `-> IList<McpClientTool>`                  |
|  [04]   | `McpClient.CallToolAsync(string, IReadOnlyDictionary, IProgress)`            | instance | `-> ValueTask<CallToolResult>`             |
|  [05]   | `McpClient.ListPromptsAsync()`                                               | instance | `-> IList<McpClientPrompt>`                |
|  [06]   | `McpClient.GetPromptAsync(string, IReadOnlyDictionary?)`                     | instance | `-> GetPromptResult`                       |
|  [07]   | `McpClient.ListResourcesAsync()`                                             | instance | `-> IList<McpClientResource>`              |
|  [08]   | `McpClient.SubscribeToResourceAsync(string)`                                 | instance | resource-update subscription               |
|  [09]   | `McpClient.ListResourceTemplatesAsync()`                                     | instance | `-> IList<McpClientResourceTemplate>`      |
|  [10]   | `McpClientTool.InvokeAsync(AIFunctionArguments?)`                            | instance | `AIFunction` invocation                    |
|  [11]   | `McpClientOptions.ClientInfo`                                                | property | peer identity (`Implementation?`)          |
|  [12]   | `McpClientTool.InvokeCoreAsync(AIFunctionArguments, CancellationToken)`      | instance | peer result as content or JSON only        |
|  [13]   | `McpClient.ResolveInputRequestsAsync(IDictionary<string, InputRequest>)`     | instance | `abstract` — answer a handler's MRTR asks  |
|  [14]   | `McpClient.AddKnownTools(IEnumerable<Tool>)`                                 | instance | pre-populate the tool cache before listing |
|  [15]   | `McpClient.ServerInfo` / `.ServerCapabilities` / `.ServerInstructions`       | property | the discovered peer facts                  |
|  [16]   | `McpClientOptions.DiscoverProbeTimeout`                                      | property | bounds the `server/discover` probe         |
|  [17]   | `McpClient.ReadResourceAsync(string, ...)`                                   | instance | `-> ValueTask<ReadResourceResult>`         |

- `McpClient.SubscribeToResourceAsync`: its `Func<ResourceUpdatedNotificationParams, CancellationToken, ValueTask>` handler overload returns `Task<IAsyncDisposable>` and registers a per-URI update handler.
- `McpClient.ReadResourceAsync` serves resources AND templates as two overloads of one member: `(string uri, RequestOptions?, CancellationToken)` reads a concrete resource, and `(string uriTemplate, IReadOnlyDictionary<string, object?> arguments, RequestOptions?, CancellationToken)` evaluates the RFC 6570 template before the read — there is no separate template-read verb.
- `RequestOptions` declares `Meta`, `ProgressToken`, `JsonSerializerOptions`, and `GetMetaForRequest()` — NO per-request timeout exists anywhere in the C# SDK: the five `Timeout` members are lifecycle-scoped (`HttpClientTransportOptions.ConnectionTimeout`, `StdioClientTransportOptions.ShutdownTimeout`, `McpClientOptions.InitializationTimeout`/`.DiscoverProbeTimeout`, `McpServerOptions.InitializationTimeout`), so a call deadline is the caller's own linked cancellation source over the trailing `CancellationToken`.
- `ResumeClientSessionOptions` carries the detached session's `ServerCapabilities`, `ServerInfo`, `ServerInstructions`, and `NegotiatedProtocolVersion` — the version slot is load-bearing, defaulting to `2025-11-25` when null; a null option, capability set, or server info faults `ArgumentNullException`, so a resuming host persists the handshake facts beside the session id.

[ENTRYPOINT_SCOPE]: server configuration on `McpServerOptions`.

| [INDEX] | [SURFACE]                                  | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :----------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `McpServerOptions.ServerInfo`              | property | server identity (`Implementation`)                  |
|  [02]   | `McpServerOptions.Capabilities`            | property | advertised `ServerCapabilities`                     |
|  [03]   | `McpServerOptions.ProtocolVersion`         | property | date-versioned protocol string                      |
|  [04]   | `McpServerOptions.InitializationTimeout`   | property | handshake timeout; 60s default                      |
|  [05]   | `McpServerOptions.ServerInstructions`      | property | client instructions                                 |
|  [06]   | `McpServerOptions.ToolCollection`          | property | `McpServerPrimitiveCollection<McpServerTool>`       |
|  [07]   | `McpServerOptions.PromptCollection`        | property | `McpServerPrimitiveCollection<McpServerPrompt>`     |
|  [08]   | `McpServerOptions.ResourceCollection`      | property | `McpServerResourceCollection`                       |
|  [09]   | `McpServerOptions.RequestHandlers`         | property | `IList<McpServerRequestHandler>?` raw method mounts |
|  [10]   | `McpServerOptions.ScopeRequests`           | property | per-request DI scope; default `true`                |
|  [11]   | `McpServerOptions.KnownClientInfo`         | property | `Implementation?` seeded ahead of discovery         |
|  [12]   | `McpServerOptions.KnownClientCapabilities` | property | `ClientCapabilities?` seeded ahead of discovery     |

[ENTRYPOINT_SCOPE]: `McpServer` session long-running verbs; server-initiated legs require a stateful session, `Create` trails `ILoggerFactory?` and `IServiceProvider?`, and only `ElicitAsync<T>` trails `RequestOptions?` — the params-shaped verbs trail `CancellationToken` alone.

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `McpServer.Create(ITransport, McpServerOptions)`                      | factory  | construct a server over a transport             |
|  [02]   | `McpServer.RunAsync(CancellationToken)`                               | instance | drive the session to completion                 |
|  [03]   | `McpServer.IsMrtrSupported`                                           | property | `virtual bool` — MRTR reachable on this session |
|  [04]   | `McpServer.ElicitAsync(ElicitRequestParams, CancellationToken)`       | instance | structured mid-call input `-> ElicitResult`     |
|  [05]   | `McpServer.ElicitAsync<T>(string, RequestOptions?)`                   | instance | typed mid-call input `-> ElicitResult<T>`       |
|  [06]   | `McpServer.WithOutgoingRequestInterceptor(Func<…>)`                   | instance | reroute outgoing requests onto another channel  |
|  [07]   | `McpServer.{ClientInfo, ClientCapabilities, Services, ServerOptions}` | property | negotiated peer facts and composition roots     |

- `McpServer.WithOutgoingRequestInterceptor`: takes `Func<string, JsonNode?, CancellationToken, ValueTask<JsonNode?>>`.

- `McpServer.ElicitAsync<T>` builds its request schema by REFLECTION over the type's `JsonTypeInfo.Properties` (memoized per options-and-type; a non-object `JsonTypeInfoKind` throws `McpProtocolException`) and always shapes an `elicitation/create` request; that request diverts onto MRTR only inside an active MRTR context — a STATEFUL session whose client speaks `2026-07-28` — while a down-level stateful session rides the SDK's own resolve-and-retry bridge (real `elicitation/create`, handler re-run, capped at 10 rounds on each side), and on the stateless transport the member THROWS (`ClientCapabilities` is null), leaving a hand-built `InputRequest.ForElicitation` inside a thrown `InputRequiredException` as the only stateless ask.
- `WithOutgoingRequestInterceptor` returns a non-mutating facade whose redirected methods SKIP the client-capability check, because the alternate channel — not the negotiated session — owns delivery.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `McpSession : IAsyncDisposable`; `McpServer` and `McpClient` both extend it.
- `McpServerTool`, `McpServerPrompt`, `McpServerResource` all implement `IMcpServerPrimitive`; `[McpServerTool]` on public methods drives reflection-based tool registration.
- Negotiation is discovery-first: the client probes `server/discover` under `McpClientOptions.DiscoverProbeTimeout` and falls back to the legacy handshake, and the three modern refusals (`-32020` header mismatch, `-32021` missing capability, `-32022` unsupported version) never trigger that fallback. `ProtocolVersion` is both the request and the floor.
- Per-request `_meta` carries what the session handshake once held — protocol version, client identity, client capabilities, log level, subscription id — so every request stands alone and a stateless server holds no per-peer state between them.
- On the `2026-07-28` rail MRTR is the only path from a running handler back to the client: the handler throws `InputRequiredException`, the incomplete `InputRequiredResult` carries the asks plus an opaque `RequestState`, the client resolves each locally through `ResolveInputRequestsAsync` and RETRIES the same call with `InputResponses` and the echoed state; a down-level STATEFUL session still opens real server-to-client requests, which the SDK itself uses to bridge suspensions. A handler therefore runs many times per logical call and its own side effects must be idempotent across the retries or keyed off `RequestState`. MRTR wraps exactly `tools/call`, `prompts/get`, and `resources/read`.
- `InputResponse` carries no wire discriminator: the expected result type follows from the paired `InputRequest.Method`, so a caller pairs the ask and the answer by dictionary key and deserializes against the matching `JsonTypeInfo`.
- A list-shaped result carries cache hints through `ICacheableResult`, so a client caches `tools/list`, `prompts/list`, `resources/list`, `resources/templates/list`, `resources/read`, and `server/discover` against its declared `TimeToLive` and `CacheScope` rather than re-listing per turn.

[STACKING]:
- `Microsoft.Extensions.AI.Abstractions`(`libs/dotnet/.api/api-extensions-ai.md`): `McpClientTool : AIFunction`, so server tools surface as MEAI `AIFunction` instances registered in `ChatOptions.Tools`; `AIFunctionMcpServerTool` wraps a MEAI `AIFunction` as a server-side tool, and `AIContentExtensions` bridges `AIContent` across the boundary.
- `api-serilog-hosting.md`: `McpServer.Create(ITransport, McpServerOptions, ILoggerFactory?, IServiceProvider?)` takes the host's composed `ILoggerFactory` and `IServiceProvider`, so protocol diagnostics and primitive resolution ride the one host composition rather than a session-private pair.
- within-host DI: `AddMcpServer().WithTools(...)/WithHttpTransport(...)` folds server registration into the host `IServiceCollection`, `McpServerOptions.ScopeRequests` gives each request its own DI scope, and `McpJsonUtilities.DefaultOptions` supplies the canonical `JsonSerializerOptions` at every boundary.

[LOCAL_ADMISSION]:
- `services.AddMcpServer()` seeds `IMcpServerBuilder`; `WithHttpTransport()` attaches the streamable-HTTP transport; `MapMcp(pattern)` registers the endpoint; `authBuilder.AddMcp()` registers the scheme and `McpAuthenticationHandler` implements token exchange.
- A handler needing client input throws `InputRequiredException` after checking `McpServer.IsMrtrSupported`, and a long-running call opts into polling through `RequestContext<T>.EnablePollingAsync(interval, ct)`; durable out-of-band task state ships in the separate `ModelContextProtocol.Extensions.Tasks` package this catalogue does not admit.
- `AddAuthorizationFilters()` runs tool authorization in the alternate-result pipeline ahead of any background dispatch and is deliberately re-callable — a call-tool filter that swaps the matched tool or the acting user re-arms it.
- A method the primitive families do not cover mounts as one `McpServerRequestHandler` row on `McpServerOptions.RequestHandlers`, never a transport fork.

[RAIL_LAW]:
- Package: `ModelContextProtocol.Core`, `ModelContextProtocol`, `ModelContextProtocol.AspNetCore`
- Owns: MCP session, discovery-first negotiation, server primitives, client tools, transport selection, DI registration, stateless HTTP hosting, the multi-round-trip input protocol, result cache hints, and subscription listening.
- Accept: request-scoped calls through `McpServer`/`McpClient`; tool invocation through `McpClientTool.InvokeAsync`; a client round trip through `InputRequiredException` and its `RequestState` echo; a raw method through one `McpServerRequestHandler` row; long-running calls through `RequestContext<T>.EnablePollingAsync`.
- Reject: hand-rolled JSON-RPC framing; out-of-session protocol message construction; a server-initiated request opened from a handler where MRTR is the route; a host-held session cell, frame buffer, replay cursor, or resume token under stateless serving; a handler whose side effects repeat across an MRTR retry; expecting a domain CLR instance to survive `McpServerTool.Create`'s marshalling ladder; reading `.Server`/`.Services`/`.User`/`.Items` as direct `RequestContext<T>` members; reading server identity off a result property where `Meta[MetaKeys.ServerInfo]` carries it.
