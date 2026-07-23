# [RASM_APPHOST_API_HEALTHCHECKS_REDIS]

`AspNetCore.HealthChecks.Redis` (Xabaril) is one concrete `IHealthCheck` proving Redis reachability: it pings every non-`Cluster` server of a supplied `IConnectionMultiplexer` and asserts `cluster_state:ok` on each cluster server. It enters the AppHost health fold as one deploy-gated `store`-tagged driver row bound to the redis sink's shared multiplexer, so a faulted redis degrades the host to `ReadOnly` through the existing Store rule, never a thrown exception.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AspNetCore.HealthChecks.Redis`
- package: `AspNetCore.HealthChecks.Redis` (Apache-2.0)
- assembly: `HealthChecks.Redis`
- namespace: `HealthChecks.Redis`, `Microsoft.Extensions.DependencyInjection`
- target: `net8.0` (also `netstandard2.0`)
- rail: health

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: probe family

`RedisHealthCheck` is the sole public type — an `IHealthCheck` with no public options type and no cluster/endpoint configuration, its probe behavior fixed. Its two public constructors admit the connection: `RedisHealthCheck(string)` connects lazily through a static multiplexer cache, and `RedisHealthCheck(IConnectionMultiplexer)` binds a supplied multiplexer without connecting.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration overloads (`RedisHealthCheckBuilderExtensions`, default name `"redis"`)

Every `AddRedis` overload is a static `IHealthChecksBuilder` extension appending `name, failureStatus, tags, timeout` after its admission argument; the string and instance forms delegate to their factory siblings, and every path adds `HealthCheckRegistration(name ?? "redis", factory, failureStatus, tags, timeout)` with a null `failureStatus` defaulting to `HealthStatus.Unhealthy`.

| [INDEX] | [SURFACE]                                                  | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------------- | :----------------------------------- |
|  [01]   | `AddRedis(string)`                                         | connection string, lazy static cache |
|  [02]   | `AddRedis(Func<IServiceProvider, string>)`                 | DI-resolved connection string        |
|  [03]   | `AddRedis(IConnectionMultiplexer)`                         | shared multiplexer, no connect       |
|  [04]   | `AddRedis(Func<IServiceProvider, IConnectionMultiplexer>)` | DI-resolved multiplexer              |

[PROBE]: `RedisHealthCheck.CheckHealthAsync(HealthCheckContext, CancellationToken)` is the sole probe operation.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `RedisHealthCheck : IHealthCheck` is the entire surface — the constructors, the `AddRedis` overloads, and one probe method, with no options type.
- `CheckHealthAsync` resolves the injected instance, the factory result, or the connection-string-keyed entry in a static `ConcurrentDictionary<string, IConnectionMultiplexer>`, then over `GetEndPoints(configuredOnly: true)` pings each `Standalone`/`Sentinel` server (database and server) and asserts `cluster_state:ok` on each `Cluster` server via `ExecuteAsync("CLUSTER", "INFO")`.
- Success returns `HealthCheckResult.Healthy()`; an exception returns `new HealthCheckResult(FailureStatus, null, exception)` and evicts and disposes the cached connection-string entry so the next probe reconnects; a linked-token timeout returns `FailureStatus` with `"Healthcheck timed out"`.

[STACKING]:
- `api-health.md`(`.api/api-health.md`): `RedisHealthCheck` registers as one `HealthCheckRegistration` on the `IHealthChecksBuilder` and emits `HealthCheckResult`, the `Microsoft.Extensions.Diagnostics.HealthChecks` contract this probe plugs into.
- within-lib: the `RedisHealthCheck(IConnectionMultiplexer)` instance path (`AddRedis` overload [03]/[04]) binds the deploy-bound redis sink's shared singleton multiplexer, defining endpoints and cluster topology once; the connection-string static cache is the within-lib rejected form.

[LOCAL_ADMISSION]:
- `HealthContributorRow.Driver(DriverProbe.Redis, cadence, check)` adapts the probe into one deploy-gated `Store`-tagged sink-tracking row, registering only when the redis sink is bound — never a `Peer` row, never a parallel `AddRedis` registration face; the degradation rule, gRPC projection, and probe deadline stay owned by the health fold.
- It binds the redis sink's shared `IConnectionMultiplexer`, so the connection-string overloads that open a second probe-only multiplexer are the rejected form here.
- A ping failure, an off-`ok` cluster node, or a connect timeout folds into a `HealthSnapshot.Entry` as a typed `HealthCheckResult`, never a thrown exception crossing the fold.

[RAIL_LAW]:
- Package: `AspNetCore.HealthChecks.Redis`
- Owns: Redis reachability and cluster-state as one `store`-tagged driver probe
- Accept: a shared sink `IConnectionMultiplexer`, endpoint ping, cluster-state assertion
- Reject: a probe-only connection-string multiplexer beside the app's sink, or a thrown probe failure crossing the health fold
