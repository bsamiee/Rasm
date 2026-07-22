# [RASM_APPHOST_API_GRPC_CLIENT]

Full surface and stacking: `libs/csharp/.api/api-grpc-client.md` (shared-tier canonical owner).

## [01]-[IMPLEMENTATION_LAW]

[CHANNEL_TOPOLOGY]:
- AppHost dials two outbound channels: a `LiveClient` discovery-attach to the companion server and a `Wire/coordination` channel to the cluster election-and-lock endpoint, each one warm channel per endpoint disposed once.
- discovery-attach mints `GrpcChannel.ForAddress` over a `SocketsHttpHandler` whose `ConnectCallback` dials the companion's Unix domain socket — the `http://localhost` address is nominal, the socket path is binding-spec policy, and a `ConnectCallback` handler forfeits the channel's connectivity and balancing surface by construction.
- keepalive and reconnect backoff ride the `SocketsHttpHandler` transport and `GrpcChannelOptions` the canonical catalog owns; AppHost sets them as channel policy, never a second handler.

[LOCAL_ADMISSION]:
- balancing enters through the `AddServiceDiscovery` integration and its `dns`/`static` factory config; the `Resolver`/`LoadBalancer`/`Subchannel` extensibility surface is `Microsoft.Extensions.ServiceDiscovery`'s to own, never a hand-subclassed resolver or balancer on the call path.
- retry and hedging stay data-driven wire-native `ServiceConfig` the channel applies; the `Wire/outbound` `Polly.Core` hop owns cross-cutting resilience, so gRPC service config never becomes a second resilience owner stacking budgets.
- server hosting stays outside this package graph — `Grpc.AspNetCore` hosts and the client rail composes no server-side handler override; `Grpc.Net.Client` only dials.

[STACK]:
- discovery-attach: `GrpcChannel.ForAddress` with a `ConnectCallback` `SocketsHttpHandler` dials the companion UDS server as one `LiveClient` hop.
- coordination: `Wire/coordination` resolves the cluster election-and-lock endpoint through a resolver `Microsoft.Extensions.ServiceDiscovery` supplies.
- health projection: `Grpc.AspNetCore.HealthChecks` projects channel-target reachability through the standard gRPC health service.

[RAIL_LAW]:
- Package: `Grpc.Net.Client`
- Owns: AppHost outbound channel construction, call invocation, and wire-native service config for the discovery-attach and coordination consumers
- Accept: a UDS-`ConnectCallback` companion channel or a `ServiceDiscovery`-resolved cluster channel, with retry and hedging as data-driven service config
- Reject: a server hosting surface, a hand-subclassed `Resolver`/`LoadBalancer`, or a second resilience path duplicating the `Polly.Core` hop
