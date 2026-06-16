# [RASM_APPHOST_API_GRPC_CLIENT]

`Grpc.Net.Client` supplies the managed gRPC channel, factory, call invoker, and channel
options surface for outbound gRPC hops. Credential composition, per-channel service config,
compression, and HttpHandler injection are all owned here.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Net.Client`
- package: `Grpc.Net.Client`
- assembly: `Grpc.Net.Client` (net10.0)
- transitive: `Grpc.Core.Api` (ChannelCredentials, CallInvoker, CallCredentials), `Grpc.Net.Common` (ICompressionProvider)
- namespace primary: `Grpc.Net.Client`
- namespace config: `Grpc.Net.Client.Configuration`
- namespace compression: `Grpc.Net.Compression` (via `Grpc.Net.Common`)
- rail: outbound-resilience, HOP_AXIS

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: channel family
- rail: outbound-resilience

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [RAIL]                   |
| :-----: | :------------------- | :------------- | :----------------------- |
|   [1]   | `GrpcChannel`        | channel        | outbound hop entry point |
|   [2]   | `GrpcChannelOptions` | options record | channel configuration    |

[PUBLIC_TYPE_SCOPE]: credential family (Grpc.Core)
- rail: outbound-resilience

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]       | [RAIL]                   |
| :-----: | :------------------- | :------------------ | :----------------------- |
|   [1]   | `ChannelCredentials` | transport cred base | TLS / insecure selector  |
|   [2]   | `CallCredentials`    | per-call cred       | auth interceptor wrapper |
|   [3]   | `CallInvoker`        | abstract invoker    | RPC dispatch surface     |

[PUBLIC_TYPE_SCOPE]: configuration family
- rail: outbound-resilience

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]     | [RAIL]                    |
| :-----: | :-------------- | :---------------- | :------------------------ |
|   [1]   | `ServiceConfig` | service config    | retry / hedging / LB root |
|   [2]   | `MethodConfig`  | per-method config | retry or hedging policy   |
|   [3]   | `RetryPolicy`   | retry config      | method-level retry        |
|   [4]   | `HedgingPolicy` | hedging config    | method-level hedging      |

[PUBLIC_TYPE_SCOPE]: compression family (Grpc.Net.Common)
- rail: outbound-resilience

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]    | [RAIL]                 |
| :-----: | :------------------------ | :--------------- | :--------------------- |
|   [1]   | `ICompressionProvider`    | compression port | encode/decode contract |
|   [2]   | `GzipCompressionProvider` | built-in impl    | gzip codec             |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: channel factory
- rail: outbound-resilience

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY]   | [RAIL]                        |
| :-----: | :--------------------------------------------------------------------------- | :--------------- | :---------------------------- |
|   [1]   | `GrpcChannel.ForAddress(string)`                                             | static factory   | address-only channel          |
|   [2]   | `GrpcChannel.ForAddress(Uri)`                                                | static factory   | uri-only channel              |
|   [3]   | `GrpcChannel.ForAddress(string, GrpcChannelOptions)`                         | static factory   | configured channel            |
|   [4]   | `GrpcChannel.ForAddress(Uri, GrpcChannelOptions)`                            | static factory   | configured channel (uri)      |
|   [5]   | `GrpcChannel.CreateCallInvoker()`                                            | invoker factory  | returns `CallInvoker`         |
|   [6]   | `GrpcChannel.ConnectAsync(CancellationToken)`                                | explicit connect | pre-warm connection           |
|   [7]   | `GrpcChannel.WaitForStateChangedAsync(ConnectivityState, CancellationToken)` | state observe    | await connectivity transition |
|   [8]   | `GrpcChannel.Dispose()`                                                      | disposal         | channel teardown              |

[ENTRYPOINT_SCOPE]: credential composition
- rail: outbound-resilience

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]   | [RAIL]                      |
| :-----: | :--------------------------------------------------------------- | :--------------- | :-------------------------- |
|   [1]   | `ChannelCredentials.Create(ChannelCredentials, CallCredentials)` | static compose   | composite cred construction |
|   [2]   | `ChannelCredentials.Insecure`                                    | static singleton | http:// transport           |
|   [3]   | `ChannelCredentials.SecureSsl`                                   | static singleton | https:// transport          |
|   [4]   | `CallCredentials.FromInterceptor(AsyncAuthInterceptor)`          | static factory   | auth header interceptor     |
|   [5]   | `CallCredentials.Compose(CallCredentials[])`                     | static compose   | multi-interceptor chain     |

[ENTRYPOINT_SCOPE]: CallInvoker dispatch (Grpc.Core abstract)
- rail: outbound-resilience

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY]  | [RAIL]                  |
| :-----: | :------------------------- | :-------------- | :---------------------- |
|   [1]   | `AsyncUnaryCall`           | abstract method | unary RPC               |
|   [2]   | `BlockingUnaryCall`        | abstract method | blocking unary RPC      |
|   [3]   | `AsyncServerStreamingCall` | abstract method | server-streaming RPC    |
|   [4]   | `AsyncClientStreamingCall` | abstract method | client-streaming RPC    |
|   [5]   | `AsyncDuplexStreamingCall` | abstract method | bidirectional streaming |

## [4]-[IMPLEMENTATION_LAW]

[CHANNEL_TOPOLOGY]:
- `GrpcChannel` has no public constructor; all instances come from `ForAddress` factory overloads.
- `HttpClient` and `HttpHandler` are mutually exclusive on `GrpcChannelOptions`; setting both throws at channel construction.
- `DisposeHttpClient = false` by default; channel disposal does not tear down an externally-supplied handler unless explicitly opted in.
- `State` and `WaitForStateChangedAsync` require `SocketsHttpHandler`; absent that transport, connectivity state is unknown.
- `ConnectAsync` is optional pre-warming; first RPC implicitly connects.

[CREDENTIAL_LAW]:
- `ChannelCredentials` on `GrpcChannelOptions.Credentials` must match the address scheme: `Insecure` for `http://`, `SecureSsl` for `https://`.
- `ChannelCredentials.Create(channelCred, callCred)` wraps transport + per-call auth into a `CompositeChannelCredentials`; return type is `ChannelCredentials`.
- Per-call credentials on an insecure channel are suppressed by default; set `UnsafeUseInsecureChannelCallCredentials = true` to propagate them.
- `CallCredentials.FromInterceptor` takes `AsyncAuthInterceptor = Func<AuthInterceptorContext, Metadata, Task>`.

[RETRY_LAW]:
- Retry and hedging are disabled by default; activate by setting `GrpcChannelOptions.ServiceConfig` with a `MethodConfig` carrying either `RetryPolicy` or `HedgingPolicy`.
- `GrpcChannelOptions.MaxRetryAttempts` (default 5) caps any per-method `MaxAttempts`; set to `null` to lift the cap.
- `MaxRetryBufferSize` (default 16 MB) and `MaxRetryBufferPerCallSize` (default 1 MB) gate retry buffer; set to `null` to lift.
- Retry and hedging cannot both be configured on the same `MethodConfig`.

[COMPRESSION_LAW]:
- `GrpcChannelOptions.CompressionProviders` is `IList<Grpc.Net.Compression.ICompressionProvider>`; defaults to empty (no compression).
- `ICompressionProvider` contract: `string EncodingName { get; }`, `CreateCompressionStream(Stream, CompressionLevel?)`, `CreateDecompressionStream(Stream)`.
- Built-in: `GzipCompressionProvider(CompressionLevel)` from `Grpc.Net.Common`.
- Enabling compression requires registering the provider in the list and specifying the encoding name in per-call `WriteOptions`.

[SERVICE_CONFIG_LAW]:
- `ServiceConfig` members: `MethodConfigs`, `LoadBalancingConfigs`, `RetryThrottling`, `Inner` (raw JSON dict).
- `DisableResolverServiceConfig = true` prevents the resolver from overriding the client-side service config.
- `ServiceConfig` is experimental; the API may change without notice.

[LOCAL_ADMISSION]:
- Each outbound gRPC seam owns one `GrpcChannel` instance; channels are reused across calls.
- Retry policy and Polly HTTP resilience do not stack on the same seam — use one or the other per hop.
- `HttpHandler` injection point is `SocketsHttpHandler` for connection-state observability.
- Per-call auth uses `CallCredentials.FromInterceptor`; token refresh is the interceptor's responsibility.
