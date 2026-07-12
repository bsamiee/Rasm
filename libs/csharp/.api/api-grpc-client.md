# [RASM_API_GRPC_CLIENT]

`Grpc.Net.Client` supplies gRPC channels, HTTP-backed call invocation, service configuration, retry, hedging, resolver, and load-balancer surfaces for remote execution clients.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Net.Client`
- package: `Grpc.Net.Client`
- assembly: `Grpc.Net.Client`
- namespace: `Grpc.Net.Client`
- asset: runtime library
- rail: remote-client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: channel and call contracts
- rail: remote-client

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]  | [CAPABILITY]            |
| :-----: | :---------------------- | :-------------- | :---------------------- |
|  [01]   | `GrpcChannel`           | channel root    | owns remote channel     |
|  [02]   | `GrpcChannelOptions`    | channel policy  | configures channel      |
|  [03]   | `ConfigObject`          | config base     | base of service config  |
|  [04]   | `ServiceConfig`         | service policy  | configures calls        |
|  [05]   | `MethodConfig`          | method policy   | configures method calls |
|  [06]   | `MethodName`            | method selector | selects service methods |
|  [07]   | `RetryPolicy`           | retry policy    | controls retry attempts |
|  [08]   | `HedgingPolicy`         | hedging policy  | controls parallel calls |
|  [09]   | `RetryThrottlingPolicy` | retry policy    | throttles retries       |

[PUBLIC_TYPE_SCOPE]: resolver and balancer contracts
- rail: remote-client

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]     | [CAPABILITY]             |
| :-----: | :---------------------- | :----------------- | :----------------------- |
|  [01]   | `Resolver`              | resolver root      | resolves endpoints       |
|  [02]   | `ResolverFactory`       | resolver factory   | creates resolvers        |
|  [03]   | `DnsResolverFactory`    | resolver factory   | creates DNS resolver     |
|  [04]   | `StaticResolverFactory` | resolver factory   | creates static resolver  |
|  [05]   | `ResolverResult`        | resolver output    | carries addresses        |
|  [06]   | `LoadBalancer`          | balancer root      | owns endpoint selection  |
|  [07]   | `LoadBalancerFactory`   | balancer factory   | creates balancers        |
|  [08]   | `PickFirstBalancer`     | balancer           | selects first endpoint   |
|  [09]   | `RoundRobinBalancer`    | balancer           | rotates endpoints        |
|  [10]   | `Subchannel`            | connection channel | owns endpoint connection |
|  [11]   | `SubchannelPicker`      | picker contract    | selects subchannels      |
|  [12]   | `PickResult`            | picker result      | carries selection result |

[PUBLIC_TYPE_SCOPE]: transitive `Grpc.Core.Api` call contracts
- rail: remote-client

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]      | [CAPABILITY]                        |
| :-----: | :------------------------- | :------------------ | :---------------------------------- |
|  [01]   | `Interceptor`              | interceptor base    | client call override family         |
|  [02]   | `ClientInterceptorContext` | call context struct | carries call context                |
|  [03]   | `CallInvoker`              | invocation root     | composes interceptors               |
|  [04]   | `CallInvokerExtensions`    | invoker extensions  | `Intercept` factory overloads       |
|  [05]   | `InterceptingCallInvoker`  | interceptor invoker | wraps invoker with one interceptor  |
|  [06]   | `CallOptions`              | call policy struct  | carries call policy                 |
|  [07]   | `Metadata`                 | header collection   | stores metadata entries             |
|  [08]   | `RpcException`             | call failure        | status plus trailers                |
|  [09]   | `Status`                   | status struct       | code plus detail                    |
|  [10]   | `StatusCode`               | status enum         | call-failure taxonomy               |
|  [11]   | `CallCredentials`          | per-call trust      | interceptor-backed call credentials |
|  [12]   | `ChannelCredentials`       | channel trust       | transport credential selection      |
|  [13]   | `ConnectivityState`        | state enum          | channel connectivity taxonomy       |
|  [14]   | `AsyncAuthInterceptor`     | auth delegate       | async metadata injection delegate   |
|  [15]   | `AuthInterceptorContext`   | auth call context   | carries authentication context      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: channel operations
- rail: remote-client

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]    | [CAPABILITY]                |
| :-----: | :------------------------------------- | :-------------- | :-------------------------- |
|  [01]   | `GrpcChannel.ForAddress`               | factory call    | creates channel             |
|  [02]   | `GrpcChannel.ConnectAsync`             | warm-up call    | eagerly connects to `Ready` |
|  [03]   | `GrpcChannel.CreateCallInvoker`        | factory call    | creates call invoker        |
|  [04]   | `GrpcChannel.Dispose`                  | lifetime call   | closes channel              |
|  [05]   | `ServiceConfig`                        | option property | applies service policy      |
|  [06]   | `Credentials`                          | option property | applies channel security    |
|  [07]   | `HttpHandler`                          | option property | selects HTTP transport      |
|  [08]   | `MaxReceiveMessageSize`                | option property | bounds response payloads    |
|  [09]   | `MaxSendMessageSize`                   | option property | bounds request payloads     |
|  [10]   | `ThrowOperationCanceledOnCancellation` | option property | controls cancellation       |

[ENTRYPOINT_SCOPE]: channel-state and compression operations
- rail: remote-client

| [INDEX] | [SURFACE]                                 | [CALL_SHAPE]    | [CAPABILITY]                                          |
| :-----: | :---------------------------------------- | :-------------- | :---------------------------------------------------- |
|  [01]   | `GrpcChannel.State`                       | state property  | reports `ConnectivityState`                           |
|  [02]   | `GrpcChannel.WaitForStateChangedAsync`    | state call      | awaits departure from an observed `ConnectivityState` |
|  [03]   | `GrpcChannelOptions.CompressionProviders` | option property | registers `ICompressionProvider` rows                 |
|  [04]   | `GrpcChannelOptions.HttpVersion`          | option property | selects channel HTTP version                          |
|  [05]   | `grpc-internal-encoding-request`          | metadata key    | selects per-call request compression                  |

[ENTRYPOINT_SCOPE]: `GrpcChannel` members
- source: `Grpc.Net.Client` (`lib/net10.0`) — `Grpc.Net.Client.GrpcChannel` surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

`GrpcChannel` is a sealed `ChannelBase` and `IDisposable`. The `ForAddress` overloads are static, `State` is get-only, `CreateCallInvoker` overrides the base member, and both asynchronous members append `CancellationToken cancellationToken = default` to the parameters shown below. `ConnectAsync` warms the channel to `Ready`.

| [INDEX] | [MEMBER]                               | [RESULT]            | [PARAMETERS]                                        |
| :-----: | :------------------------------------- | :------------------ | :-------------------------------------------------- |
|  [01]   | `GrpcChannel.ForAddress`               | `GrpcChannel`       | `string address`                                    |
|  [02]   | `GrpcChannel.ForAddress`               | `GrpcChannel`       | `string address, GrpcChannelOptions channelOptions` |
|  [03]   | `GrpcChannel.ForAddress`               | `GrpcChannel`       | `Uri address`                                       |
|  [04]   | `GrpcChannel.ForAddress`               | `GrpcChannel`       | `Uri address, GrpcChannelOptions channelOptions`    |
|  [05]   | `GrpcChannel.ConnectAsync`             | `Task`              | —                                                   |
|  [06]   | `GrpcChannel.State`                    | `ConnectivityState` | —                                                   |
|  [07]   | `GrpcChannel.WaitForStateChangedAsync` | `Task`              | `ConnectivityState lastObservedState`               |
|  [08]   | `GrpcChannel.CreateCallInvoker`        | `CallInvoker`       | —                                                   |
|  [09]   | `GrpcChannel.Dispose`                  | `void`              | —                                                   |

`ConnectAsync`, `State`, and `WaitForStateChangedAsync` are unavailable when the channel wraps a caller-supplied `GrpcChannelOptions.HttpClient`; they require the handler-owned (`HttpHandler` / default) transport. Channel-internal defaults the remote-lane policy mirrors: `MaxReceiveMessageSize` 4 MiB (`4194304`), `MaxRetryAttempts` 5, `MaxRetryBufferSize` 16 MiB, `MaxRetryBufferPerCallSize` 1 MiB, `InitialReconnectBackoff` 1 s, `MaxReconnectBackoff` 120 s.

[ENTRYPOINT_SCOPE]: interceptor override signatures
- source: `Grpc.Core.Api` — `Grpc.Core.Interceptors.Interceptor` surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

Every override is `virtual`, generic on `<TRequest,TResponse>`, and constrains both type parameters to `class`. Each receives `ClientInterceptorContext<TRequest,TResponse> context` and the continuation delegate formed by suffixing its member name with `Continuation<TRequest,TResponse>` after the request where present.

| [INDEX] | [MEMBER]                   | [REQUEST]          | [RESULT]                                       |
| :-----: | :------------------------- | :----------------- | :--------------------------------------------- |
|  [01]   | `BlockingUnaryCall`        | `TRequest request` | `TResponse`                                    |
|  [02]   | `AsyncUnaryCall`           | `TRequest request` | `AsyncUnaryCall<TResponse>`                    |
|  [03]   | `AsyncServerStreamingCall` | `TRequest request` | `AsyncServerStreamingCall<TResponse>`          |
|  [04]   | `AsyncClientStreamingCall` | —                  | `AsyncClientStreamingCall<TRequest,TResponse>` |
|  [05]   | `AsyncDuplexStreamingCall` | —                  | `AsyncDuplexStreamingCall<TRequest,TResponse>` |

[ENTRYPOINT_SCOPE]: `ClientInterceptorContext` struct members
- source: `Grpc.Core.Api` — `Grpc.Core.Interceptors.ClientInterceptorContext<TRequest,TResponse>` surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

| [INDEX] | [MEMBER]  | [SIGNATURE]                                                                                      |
| :-----: | :-------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `Method`  | `Method<TRequest,TResponse> Method { get; }`                                                     |
|  [02]   | `Host`    | `string? Host { get; }`                                                                          |
|  [03]   | `Options` | `CallOptions Options { get; }`                                                                   |
|  [04]   | `ctor`    | `ClientInterceptorContext(Method<TRequest,TResponse> method, string? host, CallOptions options)` |

[ENTRYPOINT_SCOPE]: `CallOptions` struct members
- source: `Grpc.Core.Api` — `Grpc.Core.CallOptions` surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

The first four members are get-only properties; every `With*` member returns `CallOptions`. `WithWaitForReady` selects queued startup, `WithWriteOptions` sets the per-call `WriteFlags`, and `WithPropagationToken` inherits a parent server call's deadline and cancellation.

| [INDEX] | [MEMBER]                | [RESULT]            | [PARAMETERS]                               |
| :-----: | :---------------------- | :------------------ | :----------------------------------------- |
|  [01]   | `Headers`               | `Metadata?`         | —                                          |
|  [02]   | `Deadline`              | `DateTime?`         | —                                          |
|  [03]   | `CancellationToken`     | `CancellationToken` | —                                          |
|  [04]   | `Credentials`           | `CallCredentials?`  | —                                          |
|  [05]   | `WithHeaders`           | `CallOptions`       | `Metadata headers`                         |
|  [06]   | `WithDeadline`          | `CallOptions`       | `DateTime deadline`                        |
|  [07]   | `WithCancellationToken` | `CallOptions`       | `CancellationToken cancellationToken`      |
|  [08]   | `WithCredentials`       | `CallOptions`       | `CallCredentials credentials`              |
|  [09]   | `WithWaitForReady`      | `CallOptions`       | `bool waitForReady = true`                 |
|  [10]   | `WithWriteOptions`      | `CallOptions`       | `WriteOptions writeOptions`                |
|  [11]   | `WithPropagationToken`  | `CallOptions`       | `ContextPropagationToken propagationToken` |

`CallSpine.Options` threads `WithDeadline`/`WithCancellationToken` (+ a once-per-call `WithHeaders` re-stamp) on every shape; `WithWaitForReady` is the wait-vs-fail-fast knob (off on the Compute hot path, where the channel is pre-warmed via `ConnectAsync` and a transient failure must surface as a typed fault rather than block inside the budget).

[ENTRYPOINT_SCOPE]: `Grpc.Core.Metadata` header-collection members
- source: `Grpc.Core.Api` — `Grpc.Core.Metadata` / nested `Grpc.Core.Metadata.Entry` surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

`Metadata : IList<Metadata.Entry>` is the mutable header and trailer collection threaded through `CallOptions.Headers`, `WithHeaders`, and `RpcException.Trailers`. `Metadata.Empty` is the read-only empty seed; keyed reads return the last match, while `GetAll` returns every match. `CallSpine` adds `metadata.Add("authorization", "Bearer …")`, and `Merge` folds stamped entries over `Metadata.Empty` with `Add(Metadata.Entry)`. Keys are lowercased, and the `-bin` suffix selects `ValueBytes` instead of `Value`.

| [INDEX] | [MEMBER]                      | [SIGNATURE]                                |
| :-----: | :---------------------------- | :----------------------------------------- |
|  [01]   | `Metadata.ctor`               | `Metadata()`                               |
|  [02]   | `Metadata.Empty`              | `static readonly Metadata Empty`           |
|  [03]   | `Metadata.Add`                | `void Add(string key, string value)`       |
|  [04]   | `Metadata.Add`                | `void Add(string key, byte[] valueBytes)`  |
|  [05]   | `Metadata.Add`                | `void Add(Metadata.Entry entry)`           |
|  [06]   | `Metadata.Get`                | `Entry? Get(string key)`                   |
|  [07]   | `Metadata.GetValue`           | `string? GetValue(string key)`             |
|  [08]   | `Metadata.GetValueBytes`      | `byte[]? GetValueBytes(string key)`        |
|  [09]   | `Metadata.GetAll`             | `IEnumerable<Entry> GetAll(string key)`    |
|  [10]   | `Metadata.Entry.ctor`         | `Entry(string key, string value)`          |
|  [11]   | `Metadata.Entry.ctor`         | `Entry(string key, byte[] valueBytes)`     |
|  [12]   | `Metadata.Entry.Key`          | `string Key { get; }`                      |
|  [13]   | `Metadata.Entry.Value`        | `string Value { get; }`                    |
|  [14]   | `Metadata.Entry.ValueBytes`   | `byte[] ValueBytes { get; }`               |
|  [15]   | `Metadata.Entry.IsBinary`     | `bool IsBinary { get; }`                   |
|  [16]   | `Metadata.BinaryHeaderSuffix` | `const string BinaryHeaderSuffix = "-bin"` |

[ENTRYPOINT_SCOPE]: `RpcException` members
- source: `Grpc.Core.Api` — `Grpc.Core.RpcException` surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

| [INDEX] | [MEMBER]     | [SIGNATURE]                                                      |
| :-----: | :----------- | :--------------------------------------------------------------- |
|  [01]   | `Status`     | `Status Status { get; }`                                         |
|  [02]   | `StatusCode` | `StatusCode StatusCode { get; }`                                 |
|  [03]   | `Trailers`   | `Metadata Trailers { get; }`                                     |
|  [04]   | `ctor`       | `RpcException(Status status)`                                    |
|  [05]   | `ctor`       | `RpcException(Status status, string message)`                    |
|  [06]   | `ctor`       | `RpcException(Status status, Metadata trailers)`                 |
|  [07]   | `ctor`       | `RpcException(Status status, Metadata trailers, string message)` |

[ENTRYPOINT_SCOPE]: `StatusCode` enum members
- source: `Grpc.Core.Api` — `Grpc.Core.StatusCode` surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

| [INDEX] | [MEMBER]             | [SIGNATURE]              |
| :-----: | :------------------- | :----------------------- |
|  [01]   | `OK`                 | `OK = 0`                 |
|  [02]   | `Cancelled`          | `Cancelled = 1`          |
|  [03]   | `Unknown`            | `Unknown = 2`            |
|  [04]   | `InvalidArgument`    | `InvalidArgument = 3`    |
|  [05]   | `DeadlineExceeded`   | `DeadlineExceeded = 4`   |
|  [06]   | `NotFound`           | `NotFound = 5`           |
|  [07]   | `AlreadyExists`      | `AlreadyExists = 6`      |
|  [08]   | `PermissionDenied`   | `PermissionDenied = 7`   |
|  [09]   | `ResourceExhausted`  | `ResourceExhausted = 8`  |
|  [10]   | `FailedPrecondition` | `FailedPrecondition = 9` |
|  [11]   | `Aborted`            | `Aborted = 10`           |
|  [12]   | `OutOfRange`         | `OutOfRange = 11`        |
|  [13]   | `Unimplemented`      | `Unimplemented = 12`     |
|  [14]   | `Internal`           | `Internal = 13`          |
|  [15]   | `Unavailable`        | `Unavailable = 14`       |
|  [16]   | `DataLoss`           | `DataLoss = 15`          |
|  [17]   | `Unauthenticated`    | `Unauthenticated = 16`   |

[ENTRYPOINT_SCOPE]: credential composition members
- source: `Grpc.Core.Api` — `Grpc.Core.CallCredentials` / `Grpc.Core.ChannelCredentials` surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

Every credential member is static; `Insecure` and `SecureSsl` are get-only properties.

| [INDEX] | [MEMBER]                          | [SIGNATURE]                                                                                         |
| :-----: | :-------------------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `CallCredentials.Compose`         | `CallCredentials Compose(params CallCredentials[] credentials)`                                     |
|  [02]   | `CallCredentials.FromInterceptor` | `CallCredentials FromInterceptor(AsyncAuthInterceptor interceptor)`                                 |
|  [03]   | `ChannelCredentials.Create`       | `ChannelCredentials Create(ChannelCredentials channelCredentials, CallCredentials callCredentials)` |
|  [04]   | `ChannelCredentials.Insecure`     | `ChannelCredentials Insecure { get; }`                                                              |
|  [05]   | `ChannelCredentials.SecureSsl`    | `ChannelCredentials SecureSsl { get; }`                                                             |

[ENTRYPOINT_SCOPE]: `CallInvokerExtensions` intercept factory members
- source: `Grpc.Core.Api` — `Grpc.Core.Interceptors.CallInvokerExtensions` surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

| [INDEX] | [MEMBER]                          | [SIGNATURE]                                                                                    |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `CallInvokerExtensions.Intercept` | `static CallInvoker Intercept(this CallInvoker invoker, Interceptor interceptor)`              |
|  [02]   | `CallInvokerExtensions.Intercept` | `static CallInvoker Intercept(this CallInvoker invoker, params Interceptor[] interceptors)`    |
|  [03]   | `CallInvokerExtensions.Intercept` | `static CallInvoker Intercept(this CallInvoker invoker, Func<Metadata, Metadata> interceptor)` |
|  [04]   | `InterceptingCallInvoker.ctor`    | `InterceptingCallInvoker(CallInvoker invoker, Interceptor interceptor)`                        |

[ENTRYPOINT_SCOPE]: `AsyncAuthInterceptor` delegate and `AuthInterceptorContext` members
- source: `Grpc.Core.Api` — `Grpc.Core.AsyncAuthInterceptor` / `Grpc.Core.AuthInterceptorContext` surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

The context rows name `AuthInterceptorContext` members; its properties are get-only.

| [INDEX] | [MEMBER]               | [SIGNATURE]                                                                                         |
| :-----: | :--------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `AsyncAuthInterceptor` | `delegate Task AsyncAuthInterceptor(AuthInterceptorContext context, Metadata metadata)`             |
|  [02]   | `ctor`                 | `AuthInterceptorContext(string serviceUrl, string methodName)`                                      |
|  [03]   | `ctor`                 | `AuthInterceptorContext(string serviceUrl, string methodName, CancellationToken cancellationToken)` |
|  [04]   | `ServiceUrl`           | `string ServiceUrl { get; }`                                                                        |
|  [05]   | `MethodName`           | `string MethodName { get; }`                                                                        |
|  [06]   | `CancellationToken`    | `CancellationToken CancellationToken { get; }`                                                      |

[ENTRYPOINT_SCOPE]: `SocketsHttpHandler` keepalive members (BCL — passed as `GrpcChannelOptions.HttpHandler`)
- source: BCL `System.Net.Http` net10.0 — `System.Net.Http.SocketsHttpHandler` member surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

`HttpKeepAlivePingPolicy.WithActiveRequests` pings while requests are pending, while `HttpKeepAlivePingPolicy.Always` also pings idle connections.

| [INDEX] | [MEMBER]                                            | [SIGNATURE]                                                 |
| :-----: | :-------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `SocketsHttpHandler.KeepAlivePingDelay`             | `TimeSpan KeepAlivePingDelay { get; set; }`                 |
|  [02]   | `SocketsHttpHandler.KeepAlivePingTimeout`           | `TimeSpan KeepAlivePingTimeout { get; set; }`               |
|  [03]   | `SocketsHttpHandler.KeepAlivePingPolicy`            | `HttpKeepAlivePingPolicy KeepAlivePingPolicy { get; set; }` |
|  [04]   | `SocketsHttpHandler.EnableMultipleHttp2Connections` | `bool EnableMultipleHttp2Connections { get; set; }`         |
|  [05]   | `HttpKeepAlivePingPolicy.WithActiveRequests`        | `WithActiveRequests = 0`                                    |
|  [06]   | `HttpKeepAlivePingPolicy.Always`                    | `Always = 1`                                                |

[ENTRYPOINT_SCOPE]: compression provider types
- source: `Grpc.Net.Common` — `Grpc.Net.Compression` namespace surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

`GzipCompressionProvider.EncodingName` returns `"gzip"`, while `DeflateCompressionProvider.EncodingName` returns `"deflate"`.

| [INDEX] | [MEMBER]                                         | [SIGNATURE]                                                                         |
| :-----: | :----------------------------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `ICompressionProvider.EncodingName`              | `string EncodingName { get; }`                                                      |
|  [02]   | `ICompressionProvider.CreateCompressionStream`   | `Stream CreateCompressionStream(Stream stream, CompressionLevel? compressionLevel)` |
|  [03]   | `ICompressionProvider.CreateDecompressionStream` | `Stream CreateDecompressionStream(Stream stream)`                                   |
|  [04]   | `GzipCompressionProvider.ctor`                   | `GzipCompressionProvider(CompressionLevel defaultCompressionLevel)`                 |
|  [05]   | `GzipCompressionProvider.EncodingName`           | `string EncodingName { get; }`                                                      |
|  [06]   | `DeflateCompressionProvider.ctor`                | `DeflateCompressionProvider(CompressionLevel defaultCompressionLevel)`              |
|  [07]   | `DeflateCompressionProvider.EncodingName`        | `string EncodingName { get; }`                                                      |

[ENTRYPOINT_SCOPE]: `GrpcChannelOptions` core properties
- source: `Grpc.Net.Client` — `GrpcChannelOptions` surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

| [INDEX] | [MEMBER]                                  | [SIGNATURE]                                                       |
| :-----: | :---------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `Credentials`                             | `ChannelCredentials? Credentials { get; set; }`                   |
|  [02]   | `MaxSendMessageSize`                      | `int? MaxSendMessageSize { get; set; }`                           |
|  [03]   | `MaxReceiveMessageSize`                   | `int? MaxReceiveMessageSize { get; set; }`                        |
|  [04]   | `MaxRetryAttempts`                        | `int? MaxRetryAttempts { get; set; }`                             |
|  [05]   | `MaxRetryBufferSize`                      | `long? MaxRetryBufferSize { get; set; }`                          |
|  [06]   | `MaxRetryBufferPerCallSize`               | `long? MaxRetryBufferPerCallSize { get; set; }`                   |
|  [07]   | `CompressionProviders`                    | `IList<ICompressionProvider>? CompressionProviders { get; set; }` |
|  [08]   | `HttpClient`                              | `HttpClient? HttpClient { get; set; }`                            |
|  [09]   | `HttpHandler`                             | `HttpMessageHandler? HttpHandler { get; set; }`                   |
|  [10]   | `DisposeHttpClient`                       | `bool DisposeHttpClient { get; set; }`                            |
|  [11]   | `ThrowOperationCanceledOnCancellation`    | `bool ThrowOperationCanceledOnCancellation { get; set; }`         |
|  [12]   | `UnsafeUseInsecureChannelCallCredentials` | `bool UnsafeUseInsecureChannelCallCredentials { get; set; }`      |
|  [13]   | `ServiceConfig`                           | `ServiceConfig? ServiceConfig { get; set; }`                      |
|  [14]   | `DisableResolverServiceConfig`            | `bool DisableResolverServiceConfig { get; set; }`                 |
|  [15]   | `ServiceProvider`                         | `IServiceProvider? ServiceProvider { get; set; }`                 |
|  [16]   | `HttpVersion`                             | `Version? HttpVersion { get; set; }`                              |
|  [17]   | `HttpVersionPolicy`                       | `HttpVersionPolicy? HttpVersionPolicy { get; set; }`              |
|  [18]   | `LoggerFactory`                           | `ILoggerFactory? LoggerFactory { get; set; }`                     |

[ENTRYPOINT_SCOPE]: `GrpcChannelOptions` reconnect properties
- source: `Grpc.Net.Client` — `GrpcChannelOptions` surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

`InitialReconnectBackoff` defaults to 1 s, and `MaxReconnectBackoff` defaults to 120 s.

| [INDEX] | [MEMBER]                  | [SIGNATURE]                                      |
| :-----: | :------------------------ | :----------------------------------------------- |
|  [01]   | `MaxReconnectBackoff`     | `TimeSpan? MaxReconnectBackoff { get; set; }`    |
|  [02]   | `InitialReconnectBackoff` | `TimeSpan InitialReconnectBackoff { get; set; }` |

[ENTRYPOINT_SCOPE]: `Grpc.Net.Client.Configuration` service-config algebra
- source: `Grpc.Net.Client` (`lib/net10.0`) — `Grpc.Net.Client.Configuration.*` surface
- rail: remote-client#CALL_SPINE
- consumer: `remote-lane#CALL_SPINE`

Every configuration type is a sealed class deriving from `ConfigObject`, a JSON-shaped `IDictionary<string,object>` carrier with a public parameterless constructor and properties backed by the inner map. The data tree seeds `GrpcChannelOptions.ServiceConfig` and remains separate from generated-client surfaces. `MethodName.Default` has null `Service` and `Method` values and matches every method; each `MethodConfig` selects retry or hedging, never both.

| [INDEX] | [MEMBER]                                   | [SIGNATURE]                                                |
| :-----: | :----------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `ServiceConfig.MethodConfigs`              | `IList<MethodConfig> MethodConfigs { get; }`               |
|  [02]   | `ServiceConfig.LoadBalancingConfigs`       | `IList<LoadBalancingConfig> LoadBalancingConfigs { get; }` |
|  [03]   | `ServiceConfig.RetryThrottling`            | `RetryThrottlingPolicy? RetryThrottling { get; set; }`     |
|  [04]   | `MethodConfig.Names`                       | `IList<MethodName> Names { get; }`                         |
|  [05]   | `MethodConfig.RetryPolicy`                 | `RetryPolicy? RetryPolicy { get; set; }`                   |
|  [06]   | `MethodConfig.HedgingPolicy`               | `HedgingPolicy? HedgingPolicy { get; set; }`               |
|  [07]   | `RetryPolicy.MaxAttempts`                  | `int? MaxAttempts { get; set; }`                           |
|  [08]   | `RetryPolicy.InitialBackoff`               | `TimeSpan? InitialBackoff { get; set; }`                   |
|  [09]   | `RetryPolicy.MaxBackoff`                   | `TimeSpan? MaxBackoff { get; set; }`                       |
|  [10]   | `RetryPolicy.BackoffMultiplier`            | `double? BackoffMultiplier { get; set; }`                  |
|  [11]   | `RetryPolicy.RetryableStatusCodes`         | `IList<StatusCode> RetryableStatusCodes { get; }`          |
|  [12]   | `HedgingPolicy.MaxAttempts`                | `int? MaxAttempts { get; set; }`                           |
|  [13]   | `HedgingPolicy.HedgingDelay`               | `TimeSpan? HedgingDelay { get; set; }`                     |
|  [14]   | `HedgingPolicy.NonFatalStatusCodes`        | `IList<StatusCode> NonFatalStatusCodes { get; }`           |
|  [15]   | `RetryThrottlingPolicy.MaxTokens`          | `int? MaxTokens { get; set; }`                             |
|  [16]   | `RetryThrottlingPolicy.TokenRatio`         | `double? TokenRatio { get; set; }`                         |
|  [17]   | `LoadBalancingConfig.PolicyName`           | `string PolicyName { get; }`                               |
|  [18]   | `LoadBalancingConfig.PickFirstPolicyName`  | `const string PickFirstPolicyName = "pick_first"`          |
|  [19]   | `LoadBalancingConfig.RoundRobinPolicyName` | `const string RoundRobinPolicyName = "round_robin"`        |
|  [20]   | `PickFirstConfig`                          | `sealed class PickFirstConfig : LoadBalancingConfig`       |
|  [21]   | `RoundRobinConfig`                         | `sealed class RoundRobinConfig : LoadBalancingConfig`      |

[ENTRYPOINT_SCOPE]: `Grpc.Net.Client.Balancer` resolver and balancer extension surface
- source: `Grpc.Net.Client` (`lib/net10.0`) — `Grpc.Net.Client.Balancer.*` surface
- rail: remote-client#BALANCER (advanced — see admission)
- consumer: `remote-lane#CALL_SPINE`

The custom resolution and client-side balancing surface centers on abstract `ResolverFactory`, `Resolver`, `LoadBalancerFactory`, `LoadBalancer`, and `SubchannelPicker` contracts. Factory `Name` and `Create`, `Resolver.Start`, both `LoadBalancer` members, and `SubchannelPicker.Pick` are abstract; `Resolver.Refresh` is virtual. Factory `Name` and `PickResult.Type` are get-only properties. `DnsResolverFactory.Name` returns `"dns"`, while `StaticResolverFactory.Name` returns `"static"`. `Resolver` and `LoadBalancer` are disposable; `Subchannel` is sealed and disposable. The `ResolverResult` and `PickResult` factories are static. `ResolverResult.ForResult` always accepts `IReadOnlyList<BalancerAddress> addresses`; its configured overload adds `ServiceConfig? serviceConfig` and `Status? serviceConfigStatus`. Resolver results carry `Status`, `Addresses`, `ServiceConfig`, `ServiceConfigStatus`, and `Attributes`; balancer addresses carry an `Attributes` bag. `PickResult.ForDrop` suppresses retry, while `ForQueue` re-queues selection before `Ready`.

Every static `ResolverResult` factory returns `ResolverResult`.

| [INDEX] | [MEMBER]     | [PARAMETERS]                                                                                          |
| :-----: | :----------- | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `ForResult`  | `IReadOnlyList<BalancerAddress> addresses`                                                            |
|  [02]   | `ForResult`  | `IReadOnlyList<BalancerAddress> addresses, ServiceConfig? serviceConfig, Status? serviceConfigStatus` |
|  [03]   | `ForFailure` | `Status status`                                                                                       |

The Compute remote lane bypasses this surface for known AppHost endpoints: one warm `GrpcChannel` owns each endpoint, `DisableResolverServiceConfig = true`, and `ServiceConfig` remains unset because the AppHost keyed pipeline owns hop retry. A fixed-endpoint client admits `StaticResolverFactory` only for a DNS-free address set; custom `LoadBalancer` and `SubchannelPicker` implementations remain outside the hot path.

| [INDEX] | [MEMBER]                          | [RESULT]                         | [PARAMETERS]                                                    |
| :-----: | :-------------------------------- | :------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `ResolverFactory.Name`            | `string`                         | —                                                               |
|  [02]   | `ResolverFactory.Create`          | `Resolver`                       | `ResolverOptions options`                                       |
|  [03]   | `DnsResolverFactory.ctor`         | `DnsResolverFactory`             | `TimeSpan refreshInterval`                                      |
|  [04]   | `DnsResolverFactory.Name`         | `string`                         | —                                                               |
|  [05]   | `StaticResolverFactory.ctor`      | `StaticResolverFactory`          | `Func<Uri, IEnumerable<BalancerAddress>> addressesCallback`     |
|  [06]   | `StaticResolverFactory.Name`      | `string`                         | —                                                               |
|  [07]   | `Resolver.Start`                  | `void`                           | `Action<ResolverResult> listener`                               |
|  [08]   | `Resolver.Refresh`                | `void`                           | —                                                               |
|  [09]   | `BalancerAddress.ctor`            | `BalancerAddress`                | `string host, int port`                                         |
|  [10]   | `BalancerAddress.ctor`            | `BalancerAddress`                | `DnsEndPoint endPoint`                                          |
|  [11]   | `LoadBalancerFactory.Name`        | `string`                         | —                                                               |
|  [12]   | `LoadBalancerFactory.Create`      | `LoadBalancer`                   | `LoadBalancerOptions options`                                   |
|  [13]   | `LoadBalancer.UpdateChannelState` | `void`                           | `ChannelState state`                                            |
|  [14]   | `LoadBalancer.RequestConnection`  | `void`                           | —                                                               |
|  [15]   | `Subchannel.RequestConnection`    | `void`                           | —                                                               |
|  [16]   | `Subchannel.OnStateChanged`       | `IDisposable`                    | `Action<SubchannelState> callback`                              |
|  [17]   | `Subchannel.UpdateAddresses`      | `void`                           | `IReadOnlyList<BalancerAddress> addresses`                      |
|  [18]   | `Subchannel.GetAddresses`         | `IReadOnlyList<BalancerAddress>` | —                                                               |
|  [19]   | `SubchannelPicker.Pick`           | `PickResult`                     | `PickContext context`                                           |
|  [20]   | `PickResult.ForSubchannel`        | `PickResult`                     | `Subchannel subchannel, ISubchannelCallTracker? tracker = null` |
|  [21]   | `PickResult.ForFailure`           | `PickResult`                     | `Status status`                                                 |
|  [22]   | `PickResult.ForDrop`              | `PickResult`                     | `Status status`                                                 |
|  [23]   | `PickResult.ForQueue`             | `PickResult`                     | —                                                               |
|  [24]   | `PickResult.Type`                 | `PickResultType`                 | —                                                               |

## [04]-[IMPLEMENTATION_LAW]

[CHANNEL_POLICY]:
- namespace: `Grpc.Net.Client`
- channel root: `GrpcChannel`
- policy root: `GrpcChannelOptions`
- transport root: HTTP handler selection stays explicit at channel creation
- payload bounds: send and receive message sizes are part of remote execution policy

[INTERCEPTOR_SURFACE]:
- namespace: `Grpc.Core.Interceptors`
- base class: `abstract class Interceptor` in `Grpc.Core.Api`
- client overrides: `BlockingUnaryCall`, `AsyncUnaryCall`, `AsyncServerStreamingCall`, `AsyncClientStreamingCall`, `AsyncDuplexStreamingCall` — each virtual with a matching continuation delegate type
- context type: `ClientInterceptorContext<TRequest,TResponse>` struct carrying `Method`, `Host`, and `Options`
- composition: `CallInvokerExtensions.Intercept(this CallInvoker, Interceptor)`, `Intercept(this CallInvoker, params Interceptor[])`, and `Intercept(this CallInvoker, Func<Metadata,Metadata>)` build chains; multiple interceptors invoked in argument order
- runtime wrapper: `InterceptingCallInvoker(CallInvoker, Interceptor)` is `internal sealed` — acquire only through `CallInvokerExtensions.Intercept`
- server overrides: `UnaryServerHandler`, `ClientStreamingServerHandler`, `ServerStreamingServerHandler`, `DuplexStreamingServerHandler` — out of scope for the Compute client rail

[KEEPALIVE_POLICY]:
- keepalive is owned by `SocketsHttpHandler` (BCL) when passed as `GrpcChannelOptions.HttpHandler`
- properties: `SocketsHttpHandler.KeepAlivePingDelay`, `KeepAlivePingTimeout`, `KeepAlivePingPolicy` (`HttpKeepAlivePingPolicy.WithActiveRequests` / `Always`), `EnableMultipleHttp2Connections`
- reconnect backoff: `GrpcChannelOptions.InitialReconnectBackoff` (default 1 s) and `MaxReconnectBackoff` (default 120 s) control exponential backoff between connection attempts

[COMPRESSION_SURFACE]:
- namespace: `Grpc.Net.Compression` (in `Grpc.Net.Common`)
- interface: `ICompressionProvider` with `EncodingName`, `CreateCompressionStream`, `CreateDecompressionStream`
- built-in providers: `GzipCompressionProvider(CompressionLevel)` → encoding `"gzip"`; `DeflateCompressionProvider(CompressionLevel)` → encoding `"deflate"` (wraps `ZLibStream`)
- registration: `GrpcChannelOptions.CompressionProviders` accepts `IList<ICompressionProvider>`
- per-call selection: `grpc-internal-encoding-request` metadata key

[REMOTE_RESILIENCE]:
- namespace: `Grpc.Net.Client.Configuration`; all rows are `ConfigObject` data, seeded onto `GrpcChannelOptions.ServiceConfig`, never generated-client surface.
- retry: `RetryPolicy` carries `MaxAttempts`, `InitialBackoff`, `MaxBackoff`, `BackoffMultiplier`, and the `RetryableStatusCodes` (`IList<StatusCode>`) — bounded by the channel-level `MaxRetryAttempts`/`MaxRetryBufferSize`/`MaxRetryBufferPerCallSize` caps.
- hedging: `HedgingPolicy` carries `MaxAttempts`, `HedgingDelay`, and `NonFatalStatusCodes`; a `MethodConfig` carries retry OR hedging, never both.
- throttling: `RetryThrottlingPolicy` (`MaxTokens`, `TokenRatio`) is server-pressure retry budgeting at the `ServiceConfig` level.
- selection: a `MethodConfig.Names` entry of `MethodName.Default` (service+method null) applies the policy to every method; load balancing rides `ServiceConfig.LoadBalancingConfigs` with `PickFirstConfig`/`RoundRobinConfig` rows (`pick_first`/`round_robin`).
- Compute stance: this whole config tree is the second-retry-owner surface the remote-lane rejects — `DisableResolverServiceConfig = true` and an unset `ServiceConfig` keep the no-retry posture; the AppHost keyed pipeline owns the hop retry and a detected second owner emits Conflict evidence rather than stacking.

[STACK_INTEGRATION]:
- Single channel rail: one `GrpcChannel.ForAddress(Uri, GrpcChannelOptions)` per `ComputeEndpoint`, warmed with `ConnectAsync` before the first deadline-bearing call, observed by a `State` + `WaitForStateChangedAsync` connectivity fold. The options carry `Credentials` (`ChannelCredentials.Create`/`SecureSsl` projected from the credential axis), `CompressionProviders` (the registered `ICompressionProvider` rows), `MaxSend`/`MaxReceiveMessageSize`, `HttpHandler` (a `GrpcWebHandler`-wrapped or bare `SocketsHttpHandler` whose keepalive/pooling members are threaded from one channel-policy owner), and `HttpVersion`/`HttpVersionPolicy`.
- Single call rail: one `Interceptor` (`CallSpine`) overriding all five client shapes stamps correlation, traceparent, the deadline budget, and the per-call credential/compression edges; `CallOptions.WithDeadline`/`WithCancellationToken`/`WithHeaders`/`WithCredentials` thread per call; `CallCredentials.FromInterceptor(AsyncAuthInterceptor)` (composed via `Compose`) mints per-call identity; the per-call `grpc-internal-encoding-request` key selects compression by value against the channel-side registration.
- Single fault rail: a thrown `RpcException` (`Status`/`StatusCode`/`Trailers`) classifies at one fold onto the typed `WireFault` union — the seventeen-code `StatusCode` taxonomy keys by numeric value (non-sequential), and the server-side `google.rpc.Status` detail unpacks back onto the same rail. Server-streaming responses enumerate through `IAsyncStreamReader<T>.ReadAllAsync` (the `Grpc.Net.Common` extension).

[LOCAL_ADMISSION]:
- Compute remote calls enter through client-side channels only.
- Server-side gRPC packages remain outside the Compute package graph.
- The `Grpc.Net.Client.Balancer` resolver/balancer surface and the `ServiceConfig` retry/hedging/load-balancing tree are full-surface but admission-gated OUT of the Compute hot path: `DisableResolverServiceConfig = true`, `ServiceConfig` unset, node affinity rides endpoint-identity rows rather than a load-balancing policy.
- Generated clients are typed edge adapters over Compute request and receipt algebra.

[RAIL_LAW]:
- Package: `Grpc.Net.Client`
- Owns: client channels, call invocation, client policy
- Accept: measured remote execution calls
- Reject: server hosting surface
