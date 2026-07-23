# [RASM_API_GRPC_CLIENT]

`Grpc.Net.Client` owns the client half of the gRPC wire — one long-lived `GrpcChannel` per endpoint over an HTTP/2 transport, the `CallInvoker` chain policy layers under, and the `ServiceConfig` data tree driving retry, hedging, resolution, and balancing without a call-site branch. Server hosting stops at this boundary, and every remote fault leaves as one `RpcException` carrying `Status` with trailers, so the typed fault rail folds at a single point.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Net.Client`
- package: `Grpc.Net.Client` (Apache-2.0)
- assembly: `Grpc.Net.Client`
- namespace: `Grpc.Net.Client`, `Grpc.Net.Client.Configuration`, `Grpc.Net.Client.Balancer`
- rail: remote-client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: channel roots with the `Grpc.Net.Client.Configuration` service-config algebra

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :---------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `GrpcChannel`           | class         | sealed connection root, one per endpoint      |
|  [02]   | `GrpcChannelOptions`    | class         | sealed transport and policy carrier           |
|  [03]   | `ConfigObject`          | class         | abstract JSON-map base under every config row |
|  [04]   | `ServiceConfig`         | class         | channel-wide method and balancing tree        |
|  [05]   | `MethodConfig`          | class         | per-method retry or hedging selection         |
|  [06]   | `MethodName`            | class         | service and method selector for one config    |
|  [07]   | `RetryPolicy`           | class         | bounded backoff retry over a status set       |
|  [08]   | `HedgingPolicy`         | class         | parallel attempts over a non-fatal status set |
|  [09]   | `RetryThrottlingPolicy` | class         | token-budget retry throttle                   |
|  [10]   | `LoadBalancingConfig`   | class         | policy-name-keyed balancing row               |
|  [11]   | `PickFirstConfig`       | class         | `pick_first` balancing row                    |
|  [12]   | `RoundRobinConfig`      | class         | `round_robin` balancing row                   |

[PUBLIC_TYPE_SCOPE]: `Grpc.Net.Client.Balancer` resolution and balancing extension points

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------ | :------------ | :---------------------------------------------- |
|  [01]   | `Resolver`                      | class         | abstract endpoint-resolution root, disposable   |
|  [02]   | `PollingResolver`               | class         | abstract backoff-driven re-resolution base      |
|  [03]   | `ResolverFactory`               | class         | abstract scheme-named resolver mint             |
|  [04]   | `DnsResolverFactory`            | class         | `dns` factory over a refresh interval           |
|  [05]   | `StaticResolverFactory`         | class         | `static` factory over an address callback       |
|  [06]   | `ResolverOptions`               | class         | address, port, and channel context per resolver |
|  [07]   | `ResolverResult`                | class         | resolved addresses with config and status       |
|  [08]   | `BalancerAddress`               | class         | one resolved endpoint with its attributes       |
|  [09]   | `BalancerAttributes`            | class         | typed attribute bag on addresses and results    |
|  [10]   | `BalancerAttributesKey<TValue>` | struct        | typed key into an attribute bag                 |
|  [11]   | `LoadBalancer`                  | class         | abstract endpoint-selection root, disposable    |
|  [12]   | `SubchannelsLoadBalancer`       | class         | abstract subchannel-managing balancer base      |
|  [13]   | `LoadBalancerFactory`           | class         | abstract policy-named balancer mint             |
|  [14]   | `PickFirstBalancerFactory`      | class         | `pick_first` balancer factory                   |
|  [15]   | `RoundRobinBalancerFactory`     | class         | `round_robin` balancer factory                  |
|  [16]   | `LoadBalancerOptions`           | class         | controller and configuration per balancer       |
|  [17]   | `ChannelState`                  | class         | resolver output the balancer folds              |
|  [18]   | `BalancerState`                 | class         | connectivity with picker the balancer publishes |
|  [19]   | `IChannelControlHelper`         | interface     | subchannel mint and state publication           |
|  [20]   | `Subchannel`                    | class         | sealed connection to one endpoint, disposable   |
|  [21]   | `SubchannelOptions`             | class         | address set for a new subchannel                |
|  [22]   | `SubchannelState`               | class         | connectivity with status of one subchannel      |
|  [23]   | `SubchannelPicker`              | class         | abstract per-call subchannel selection          |
|  [24]   | `PickContext`                   | class         | request message under selection                 |
|  [25]   | `PickResult`                    | class         | selection outcome                               |
|  [26]   | `PickResultType`                | enum          | selection-outcome vocabulary                    |
|  [27]   | `ISubchannelCallTracker`        | interface     | per-call start and completion hooks             |
|  [28]   | `CompletionContext`             | class         | address and error one call completed with       |
|  [29]   | `IBackoffPolicy`                | interface     | next-backoff source                             |
|  [30]   | `IBackoffPolicyFactory`         | interface     | backoff-policy mint                             |

[PUBLIC_TYPE_SCOPE]: transitive `Grpc.Core.Api` call contracts, with `ConnectivityState` from `Grpc.Net.Common`

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :--------------------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `CallInvoker`                                  | class         | abstract root over all five call shapes    |
|  [02]   | `CallInvokerExtensions`                        | class         | static `Intercept` chain builder           |
|  [03]   | `Interceptor`                                  | class         | abstract call-override family              |
|  [04]   | `ClientInterceptorContext<TRequest,TResponse>` | struct        | method, host, options under one call       |
|  [05]   | `CallOptions`                                  | struct        | immutable per-call policy carrier          |
|  [06]   | `Metadata`                                     | class         | mutable header and trailer collection      |
|  [07]   | `RpcException`                                 | class         | remote fault carrying status with trailers |
|  [08]   | `Status`                                       | struct        | code, detail, and local debug exception    |
|  [09]   | `StatusCode`                                   | enum          | canonical gRPC fault vocabulary            |
|  [10]   | `CallCredentials`                              | class         | abstract composable per-call identity      |
|  [11]   | `ChannelCredentials`                           | class         | abstract transport trust selection         |
|  [12]   | `AsyncAuthInterceptor`                         | delegate      | async metadata stamp behind a credential   |
|  [13]   | `AuthInterceptorContext`                       | class         | service URL and method under an auth stamp |
|  [14]   | `ConnectivityState`                            | enum          | channel connectivity vocabulary            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `GrpcChannel` lifecycle and connectivity

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `GrpcChannel.ForAddress(Uri, GrpcChannelOptions)`         | static   | mint a channel under explicit policy   |
|  [02]   | `GrpcChannel.ConnectAsync() -> Task`                      | instance | warm the channel to `Ready`            |
|  [03]   | `GrpcChannel.State -> ConnectivityState`                  | property | read current connectivity              |
|  [04]   | `GrpcChannel.WaitForStateChangedAsync(ConnectivityState)` | instance | await departure from an observed state |
|  [05]   | `GrpcChannel.CreateCallInvoker() -> CallInvoker`          | instance | mint the invoker generated stubs bind  |
|  [06]   | `GrpcChannel.Dispose()`                                   | instance | close the channel and its connections  |

- `GrpcChannel.ForAddress`: address takes `string` or `Uri` and the options argument drops independently, so four overloads cover the pair; a `ChannelCredentials` whose security disagrees with the address scheme throws `InvalidOperationException` at construction.
- `GrpcChannel.ConnectAsync`: both async members close on a trailing `CancellationToken cancellationToken = default` and return `Task`.
- `GrpcChannel.State`: `State`, `ConnectAsync`, and `WaitForStateChangedAsync` throw `InvalidOperationException` unless `GrpcChannelOptions.HttpHandler` carries a `SocketsHttpHandler` with no `ConnectCallback`; a supplied `HttpClient` disqualifies all three.

[ENTRYPOINT_SCOPE]: `GrpcChannelOptions` settable policy, grouped by the concern each property drives

[TRANSPORT]: `HttpHandler` `HttpClient` `DisposeHttpClient` `HttpVersion` `HttpVersionPolicy` `Credentials`
[RETRY_BUDGET]: `MaxRetryAttempts` `MaxRetryBufferSize` `MaxRetryBufferPerCallSize`
[RESOLUTION]: `ServiceConfig` `DisableResolverServiceConfig` `ServiceProvider` `InitialReconnectBackoff` `MaxReconnectBackoff`
[CALL_BEHAVIOR]: `ThrowOperationCanceledOnCancellation` `UnsafeUseInsecureChannelCallCredentials` `LoggerFactory`
[PAYLOAD]: `MaxSendMessageSize` `MaxReceiveMessageSize` `CompressionProviders`
- `GrpcChannelOptions.MaxReceiveMessageSize`: 4 MiB by construction while `MaxSendMessageSize` stays null and unbounded, so the asymmetry bites the receive leg first.
- `GrpcChannelOptions.MaxReconnectBackoff`: both backoff setters throw `ArgumentException` on a value at or below zero.
- `GrpcChannelOptions.UnsafeUseInsecureChannelCallCredentials`: an insecure channel silently ignores a call's `CallCredentials` until this is set.
- `GrpcChannelOptions.ServiceProvider`: custom `ResolverFactory` and `LoadBalancerFactory` instances resolve from it and union with the built-in set.

[ENTRYPOINT_SCOPE]: `Grpc.Net.Client.Configuration` rows, each a `ConfigObject` over a JSON-shaped map reachable as `Inner`

[ServiceConfig]: `MethodConfigs` `LoadBalancingConfigs` `RetryThrottling`
[MethodConfig]: `Names` `RetryPolicy` `HedgingPolicy`
[MethodName]: `Service` `Method` `Default`
[RetryPolicy]: `MaxAttempts` `InitialBackoff` `MaxBackoff` `BackoffMultiplier` `RetryableStatusCodes`
[HedgingPolicy]: `MaxAttempts` `HedgingDelay` `NonFatalStatusCodes`
[RetryThrottlingPolicy]: `MaxTokens` `TokenRatio`
[LoadBalancingConfig]: `PolicyName` `PickFirstPolicyName` `RoundRobinPolicyName`

- `MethodConfig.RetryPolicy`: one `MethodConfig` selects retry or hedging, never both.
- `MethodName.Default`: null `Service` with null `Method` is the global row; resolution tries the exact service-and-method key, then the service wildcard, then this row.
- `ServiceConfig.MethodConfigs`: two `MethodName` entries repeating one service-and-method pair throw `InvalidOperationException` at channel construction.
- `LoadBalancingConfig.PolicyName`: reads the single key the inner map carries, which `PickFirstConfig` and `RoundRobinConfig` seed to the `"pick_first"` and `"round_robin"` constants.
- `ServiceConfig`: seeds `GrpcChannelOptions.ServiceConfig` as a whole tree and never touches a generated-client surface.

[ENTRYPOINT_SCOPE]: `Grpc.Net.Client.Balancer` resolution and balancing call shapes

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------------- |
|  [01]   | `ResolverFactory.Name -> string`                                    | property | abstract scheme the channel matches    |
|  [02]   | `ResolverFactory.Create(ResolverOptions) -> Resolver`               | instance | abstract resolver mint                 |
|  [03]   | `DnsResolverFactory(TimeSpan)`                                      | ctor     | `"dns"` over a refresh interval        |
|  [04]   | `StaticResolverFactory(Func<Uri, IEnumerable<BalancerAddress>>)`    | ctor     | `"static"` over an address callback    |
|  [05]   | `Resolver.Start(Action<ResolverResult>)`                            | instance | abstract listener registration         |
|  [06]   | `Resolver.Refresh()`                                                | instance | virtual re-resolution request          |
|  [07]   | `PollingResolver.ResolveAsync(CancellationToken) -> Task`           | instance | protected abstract resolve leg         |
|  [08]   | `ResolverResult.ForResult(IReadOnlyList<BalancerAddress>)`          | static   | admit an address set                   |
|  [09]   | `ResolverResult.ForFailure(Status)`                                 | static   | admit a resolution failure             |
|  [10]   | `BalancerAddress(string, int)`                                      | ctor     | endpoint from host with port           |
|  [11]   | `BalancerAddress(DnsEndPoint)`                                      | ctor     | endpoint from a resolved `DnsEndPoint` |
|  [12]   | `BalancerAttributes.Set(BalancerAttributesKey<T>, T)`               | instance | write a typed attribute                |
|  [13]   | `BalancerAttributes.TryGetValue(BalancerAttributesKey<T>, out T)`   | instance | read a typed attribute                 |
|  [14]   | `LoadBalancerFactory.Create(LoadBalancerOptions) -> LoadBalancer`   | instance | abstract balancer mint                 |
|  [15]   | `LoadBalancer.UpdateChannelState(ChannelState)`                     | instance | abstract fold of resolver output       |
|  [16]   | `LoadBalancer.RequestConnection()`                                  | instance | abstract eager-connect request         |
|  [17]   | `SubchannelsLoadBalancer(IChannelControlHelper, ILoggerFactory)`    | ctor     | protected base, one subchannel each    |
|  [18]   | `IChannelControlHelper.CreateSubchannel(SubchannelOptions)`         | instance | mint a subchannel                      |
|  [19]   | `IChannelControlHelper.UpdateState(BalancerState)`                  | instance | publish connectivity with a picker     |
|  [20]   | `IChannelControlHelper.RefreshResolver()`                           | instance | force re-resolution                    |
|  [21]   | `Subchannel.RequestConnection()`                                    | instance | start connecting this endpoint         |
|  [22]   | `Subchannel.OnStateChanged(Action<SubchannelState>) -> IDisposable` | instance | subscribe state, dispose to detach     |
|  [23]   | `Subchannel.UpdateAddresses(IReadOnlyList<BalancerAddress>)`        | instance | replace this subchannel's addresses    |
|  [24]   | `Subchannel.GetAddresses() -> IReadOnlyList<BalancerAddress>`       | instance | read the current address set           |
|  [25]   | `SubchannelPicker.Pick(PickContext) -> PickResult`                  | instance | abstract per-call selection            |
|  [26]   | `PickResult.ForSubchannel(Subchannel, ISubchannelCallTracker)`      | static   | select with an optional tracker        |
|  [27]   | `PickResult.ForQueue()`                                             | static   | queue until a non-queue result lands   |
|  [28]   | `PickResult.ForFailure(Status)`                                     | static   | fail the call, retry still eligible    |
|  [29]   | `PickResult.ForDrop(Status)`                                        | static   | fail the call and suppress retry       |
|  [30]   | `ISubchannelCallTracker.Complete(CompletionContext)`                | instance | per-call completion hook               |
|  [31]   | `IBackoffPolicy.NextBackoff() -> TimeSpan`                          | instance | next re-resolution delay               |

[ResolverOptions]: `Address` `DefaultPort` `DisableServiceConfig` `LoggerFactory` `ChannelOptions`
[ResolverResult]: `Status` `Addresses` `ServiceConfig` `ServiceConfigStatus` `Attributes`
[ChannelState]: `Addresses` `LoadBalancingConfig` `Status` `Attributes`
[BalancerState]: `ConnectivityState` `Picker`
[SubchannelState]: `State` `Status`
[Subchannel]: `CurrentAddress` `Attributes`
[PickResult]: `Type` `Subchannel` `Status` `SubchannelCallTracker`
[PickResultType]: `Complete` `Queue` `Fail` `Drop`
[CompletionContext]: `Address` `Error`
[LoadBalancerOptions]: `Controller` `LoggerFactory` `Configuration`

- `ResolverResult.ForResult`: a second overload appends `ServiceConfig?` with `Status?` to carry resolver-published config beside the addresses.
- `SubchannelsLoadBalancer`: subclassing it against `PollingResolver` and `IChannelControlHelper` is the whole custom-balancer surface — the base owns one `Subchannel` per address and exposes `Controller` with `State` to the subclass.
- `PickFirstBalancerFactory.Name`: `"pick_first"`, and `RoundRobinBalancerFactory.Name` is `"round_robin"`; a custom factory arrives under its own name and takes precedence over the built-in of that name.

[ENTRYPOINT_SCOPE]: `Interceptor` client overrides and the `CallInvoker` chain

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]              |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------ |
|  [01]   | `Interceptor.BlockingUnaryCall(TRequest) -> TResponse`                          | instance | virtual; sync unary       |
|  [02]   | `Interceptor.AsyncUnaryCall(TRequest) -> AsyncUnaryCall<TResponse>`             | instance | virtual; async unary      |
|  [03]   | `Interceptor.AsyncServerStreamingCall(TRequest) -> AsyncServerStreamingCall<T>` | instance | virtual; server-streaming |
|  [04]   | `Interceptor.AsyncClientStreamingCall() -> AsyncClientStreamingCall<TReq,TRes>` | instance | virtual; client-streaming |
|  [05]   | `Interceptor.AsyncDuplexStreamingCall() -> AsyncDuplexStreamingCall<TReq,TRes>` | instance | virtual; duplex-streaming |
|  [06]   | `CallInvokerExtensions.Intercept(CallInvoker, Interceptor)`                     | static   | wrap with one interceptor |
|  [07]   | `CallInvokerExtensions.Intercept(CallInvoker, Interceptor[])`                   | static   | wrap in argument order    |
|  [08]   | `CallInvokerExtensions.Intercept(CallInvoker, Func<Metadata,Metadata>)`         | static   | wrap a header rewrite     |
|  [09]   | `ClientInterceptorContext(Method<TReq,TRes>, string, CallOptions)`              | ctor     | build an override context |

[ClientInterceptorContext]: `Method` `Host` `Options`

- `Interceptor`: every client override is generic on `<TRequest,TResponse>` with both constrained to `class`, takes `ClientInterceptorContext<TRequest,TResponse>`, and closes on the continuation delegate named by suffixing the member name with `Continuation<TRequest,TResponse>`.
- `ClientInterceptorContext.Host`: null selects the channel's own address, and rewriting `Options` in a new context is how an override threads per-call policy.
- `CallInvokerExtensions.Intercept`: mints an internal wrapping invoker, so a chain is reachable only through these extensions; `Interceptor` also carries server-side handler overrides that no client rail composes.

[ENTRYPOINT_SCOPE]: `CallOptions` per-call threading, every `With*` returning a fresh struct

[CallOptions]: `Headers` `Deadline` `CancellationToken` `WriteOptions` `PropagationToken` `Credentials` `IsWaitForReady`

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :---------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `CallOptions.WithHeaders(Metadata)`                         | instance | stamp the request header set                    |
|  [02]   | `CallOptions.WithDeadline(DateTime)`                        | instance | bind the absolute call deadline                 |
|  [03]   | `CallOptions.WithCancellationToken(CancellationToken)`      | instance | bind cancellation                               |
|  [04]   | `CallOptions.WithCredentials(CallCredentials)`              | instance | bind per-call identity                          |
|  [05]   | `CallOptions.WithWaitForReady(bool)`                        | instance | queue on a non-`Ready` channel, never fail fast |
|  [06]   | `CallOptions.WithWriteOptions(WriteOptions)`                | instance | set per-write flags                             |
|  [07]   | `CallOptions.WithPropagationToken(ContextPropagationToken)` | instance | inherit a parent call's deadline                |

[ENTRYPOINT_SCOPE]: fault surface and credential composition

| [INDEX] | [SURFACE]                                                        | [SHAPE] | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `RpcException(Status, Metadata, string)`                         | ctor    | mint a fault from the wire          |
|  [02]   | `Status(StatusCode, string, Exception)`                          | ctor    | code, detail, and the local cause   |
|  [03]   | `Status.DefaultSuccess`                                          | static  | the `OK` seed                       |
|  [04]   | `Status.DefaultCancelled`                                        | static  | the `Cancelled` seed                |
|  [05]   | `CallCredentials.FromInterceptor(AsyncAuthInterceptor)`          | static  | mint identity from a metadata stamp |
|  [06]   | `CallCredentials.Compose(CallCredentials[])`                     | static  | fold several credentials into one   |
|  [07]   | `ChannelCredentials.Create(ChannelCredentials, CallCredentials)` | static  | bind call credentials to a channel  |
|  [08]   | `ChannelCredentials.SecureSsl`                                   | static  | TLS transport trust                 |
|  [09]   | `ChannelCredentials.Insecure`                                    | static  | plaintext transport                 |

[RpcException]: `Status` `StatusCode` `Trailers`
[Status]: `StatusCode` `Detail` `DebugException`
[AuthInterceptorContext]: `ServiceUrl` `MethodName` `CancellationToken`
[StatusCode]: `OK`=0 `Cancelled`=1 `Unknown`=2 `InvalidArgument`=3 `DeadlineExceeded`=4 `NotFound`=5 `AlreadyExists`=6 `PermissionDenied`=7 `ResourceExhausted`=8 `FailedPrecondition`=9 `Aborted`=10 `OutOfRange`=11 `Unimplemented`=12 `Internal`=13 `Unavailable`=14 `DataLoss`=15 `Unauthenticated`=16

- `RpcException`: `Metadata` and the message each drop from the ctor independently, leaving `Status` the one required argument; `Status` likewise drops its `Exception`.
- `AsyncAuthInterceptor`: `Task AsyncAuthInterceptor(AuthInterceptorContext, Metadata)` writes into the supplied `Metadata` rather than returning one.
- `Status.DebugException`: local evidence only, so a server-side cause arrives through `Status.Detail` or the trailers.
- `RpcException.StatusCode`: keys the fault fold by its numeric value, matching the code a server-side `google.rpc.Status` detail unpacks against.

[ENTRYPOINT_SCOPE]: `SocketsHttpHandler` transport policy, bound through `GrpcChannelOptions.HttpHandler`

[SocketsHttpHandler]: `KeepAlivePingDelay` `KeepAlivePingTimeout` `KeepAlivePingPolicy` `EnableMultipleHttp2Connections` `PooledConnectionIdleTimeout` `PooledConnectionLifetime` `MaxConnectionsPerServer` `ConnectTimeout`
[HttpKeepAlivePingPolicy]: `WithActiveRequests`=0 `Always`=1

- `SocketsHttpHandler.KeepAlivePingPolicy`: `WithActiveRequests` pings only while streams are active, `Always` also pings an idle connection.
- `SocketsHttpHandler.ConnectCallback`: setting it forfeits the channel's connectivity and balancing surface.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `GrpcChannel` is the one long-lived object: minted once per endpoint with its full `GrpcChannelOptions`, reused for every call, disposed once.
- Every call shape resolves through `CreateCallInvoker`, so `CallInvokerExtensions.Intercept` is the single seam policy layers at and an interceptor never reaches into the channel.
- `CallOptions` is an immutable struct: each `With*` returns a fresh copy, so per-call policy threads forward with no shared state.
- `ServiceConfig` is data: retry, hedging, throttling, and balancing each resolve as a `ConfigObject` row the channel reads, never a call-site branch.
- Every remote fault leaves as `RpcException`, so `Status`, `StatusCode`, and `Trailers` fold at one point onto the typed fault rail.

[STACKING]:
- `Grpc.Net.Common`(`Rasm.Compute/.api/api-grpc-common.md`): `ICompressionProvider` rows register on `GrpcChannelOptions.CompressionProviders`, the per-call `grpc-internal-encoding-request` metadata key selects one by `EncodingName`, `ConnectivityState` is the vocabulary `GrpcChannel.State` reports, and `IAsyncStreamReader<T>.ReadAllAsync` drains a server-streaming response.
- `Grpc.Core.Api`(`Rasm.Compute/.api/api-grpc-common.md`): `Metadata` with `Metadata.Entry` is the header carrier `CallOptions.Headers`, `WithHeaders`, and `RpcException.Trailers` all thread; the `-bin` key suffix selects `ValueBytes` over `Value`.
- `Grpc.Tools`(`.api/api-grpc-tools.md`): a generated `<Service>Client` binds the `CallInvoker` from `CreateCallInvoker`, so every interceptor in the chain sits under the typed stub with no generated-code edit.
- `Google.Protobuf`(`.api/api-protobuf.md`): `IMessage<T>` payloads serialize on the call path, and `MaxSendMessageSize` with `MaxReceiveMessageSize` bounds each frame.
- `NodaTime.Serialization.Protobuf`(`.api/api-nodatime-protobuf.md`): `Timestamp` and `Duration` fields project through `ToInstant` and `ToNodaDuration`, so the message clock and the `WithDeadline` budget share one time vocabulary.
- `Grpc.Net.Client.Web`(`Rasm.Compute/.api/api-grpc-client-web.md`): `GrpcWebHandler` is a `DelegatingHandler` wrapping the `SocketsHttpHandler` handed to `GrpcChannelOptions.HttpHandler`, so gRPC-Web composes over the transport instead of replacing it.
- `OpenTelemetry.Instrumentation.GrpcNetClient`(`.api/api-otel-instrumentation-grpcnetclient.md`): client RPC spans emit off the channel with zero interceptor code, and `SuppressDownstreamInstrumentation` collapses the HTTP-transport span so one call is one span.
- `Microsoft.Extensions.ServiceDiscovery`(`Rasm.AppHost/.api/api-service-discovery.md`): balancing enters through the `AddServiceDiscovery` integration's `dns`/`static` factory config, so the `Resolver`/`LoadBalancer`/`Subchannel` extensibility surface stays that package's own — never a hand-subclassed resolver or balancer on the call path.
- `LanguageExt.Core`(`.api/api-languageext.md`): a caught `RpcException` folds to `Fin<A>.Fail` keyed on `StatusCode`, and a fan-in over several calls accumulates through `Validation<Error, A>` where `Fin` short-circuits.
- Within-library: one warm channel per endpoint — `ForAddress` with the full options record, `ConnectAsync` before the first deadline-bearing call, `CreateCallInvoker` wrapped once by `Intercept`, and `CallOptions.With*` threading deadline, cancellation, headers, and credentials per call.
- `Rasm.AppHost`: dials two warm channels — a companion discovery-attach whose `SocketsHttpHandler.ConnectCallback` dials the Unix domain socket under a nominal `http://localhost` address, and a `ServiceDiscovery`-resolved cluster election-and-lock channel — with keepalive and reconnect backoff riding channel policy, never a second handler.

[LOCAL_ADMISSION]:
- Remote calls enter through client channels; server hosting stays outside this package graph.
- Retry and hedging stay data-driven `ServiceConfig` the channel applies under `DisableResolverServiceConfig = true`; the `Polly.Core` outbound hop owns cross-cutting resilience, so gRPC service config never becomes a second resilience owner stacking budgets.
- `StaticResolverFactory` admits a DNS-free fixed-endpoint set; a custom `LoadBalancer` or `SubchannelPicker` is composition-root work, never a call-path decision.

[RAIL_LAW]:
- Package: `Grpc.Net.Client`
- Owns: the client channel, its transport policy, the invoker chain, and the client-side service-config algebra
- Accept: one warm channel per endpoint, one interceptor chain, a per-call `CallOptions` copy, and `ServiceConfig` rows as data
- Reject: a hand-rolled retry loop, a channel minted per call, and a bespoke status-to-fault map beside `RpcException`
