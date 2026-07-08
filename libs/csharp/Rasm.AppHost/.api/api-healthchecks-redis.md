# [RASM_APPHOST_API_HEALTHCHECKS_REDIS]

`AspNetCore.HealthChecks.Redis` (Xabaril) is a single concrete `IHealthCheck` that proves Redis reachability by pinging every non-`Cluster` server of an admitted `StackExchange.Redis` `IConnectionMultiplexer` and asserting `cluster_state:ok` on cluster servers. It is one `store`-tagged probe row feeding the AppHost capability-health fold; the integration-bearing path passes the SAME multiplexer the L2 cache lane already owns, so a faulted cache routes through the `ReadOnly` degradation rule.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AspNetCore.HealthChecks.Redis`
- package: `AspNetCore.HealthChecks.Redis`
- license: `Apache-2.0`
- assembly: `HealthChecks.Redis`
- namespace: `HealthChecks.Redis`
- namespace: `Microsoft.Extensions.DependencyInjection`
- target: `net8.0` (also `netstandard2.0`); the `net10.0` consumer binds the `net8.0` asset, `RefSafetyRules(11)` nullable-annotated
- dependency floor: `Microsoft.Extensions.Diagnostics.HealthChecks` (`IHealthCheck`/`HealthCheckResult`/`HealthCheckRegistration`), `StackExchange.Redis` (`IConnectionMultiplexer`, `ConnectionMultiplexer`, `IServer`, `IDatabase`, `RedisResult`) — both admitted in this folder at higher pins (`StackExchange.Redis` 3.x)
- asset: runtime library
- rail: health

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: probe family
- rail: health

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]        | [RAIL]                                                      |
| :-----: | :----------------- | :------------------- | :--------------------------------------------------------- |
|  [01]   | `RedisHealthCheck` | `IHealthCheck` probe | endpoint ping + cluster-state probe (no public options type) |

[PUBLIC_MEMBER_SCOPE]: `RedisHealthCheck` constructors
- rail: health

| [INDEX] | [MEMBER]                                                | [SHAPE]              | [NOTE]                                                              |
| :-----: | :----------------------------------------------------- | :------------------- | :----------------------------------------------------------------- |
|  [01]   | `RedisHealthCheck(string redisConnectionString)`      | connection-string ctor | lazily connects and caches the multiplexer in a static dictionary |
|  [02]   | `RedisHealthCheck(IConnectionMultiplexer multiplexer)` | multiplexer ctor     | the shared-instance integration path — no new connection           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations (`RedisHealthCheckBuilderExtensions`, default name `"redis"`)
- rail: health

| [INDEX] | [SURFACE]                                                                                                                                                            | [ENTRY_FAMILY]      | [RAIL]                                                |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------ | :--------------------------------------------------- |
|  [01]   | `AddRedis(this IHealthChecksBuilder, string redisConnectionString, string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout)` | string admission    | lazy connect, cached by connection string            |
|  [02]   | `AddRedis(this IHealthChecksBuilder, Func<IServiceProvider, string> connectionStringFactory, string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout)` | factory admission   | connection string resolved from DI                   |
|  [03]   | `AddRedis(this IHealthChecksBuilder, IConnectionMultiplexer connectionMultiplexer, string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout)` | instance admission  | the shared singleton multiplexer                      |
|  [04]   | `AddRedis(this IHealthChecksBuilder, Func<IServiceProvider, IConnectionMultiplexer> connectionMultiplexerFactory, string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout)` | factory admission   | multiplexer resolved from DI — shares the app instance |

[ENTRYPOINT_SCOPE]: probe operation
- rail: health

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [RAIL]                                                                  |
| :-----: | :--------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `RedisHealthCheck.CheckHealthAsync(HealthCheckContext, CancellationToken)` | probe          | pings each non-`Cluster` server; on a `Cluster` server asserts `cluster_state:ok`; any exception maps to `FailureStatus` |

## [04]-[IMPLEMENTATION_LAW]

[REDIS_TOPOLOGY]:
- one type: `RedisHealthCheck : IHealthCheck`; the public surface is two constructors and the four `AddRedis` extension overloads. There is NO public options type and NO public cluster/endpoint configuration — the probe behavior is fixed.
- probe mechanics: `CheckHealthAsync` resolves the multiplexer (the injected instance, the factory result, or a connection-string-keyed entry from a static `ConcurrentDictionary<string, IConnectionMultiplexer>`), then for each `GetEndPoints(configuredOnly: true)`: on a server whose `IServer.ServerType` is not `Cluster` (the enum is `Standalone=0`, `Sentinel=1`, `Cluster=2`) it `PingAsync` both the default database and the server, and on a `Cluster` server it `ExecuteAsync("CLUSTER", "INFO")` and fails unless the reply contains `cluster_state:ok`. Success returns `HealthCheckResult.Healthy()`; any exception returns `new HealthCheckResult(FailureStatus, null, exception)`.
- connection caching: the connection-string overloads connect lazily via `ConnectionMultiplexer.ConnectAsync` under a cancellation-linked timeout (a timed-out connect returns `Unhealthy("Healthcheck timed out")`), cache the multiplexer keyed by the string, and evict + dispose the entry on a probe exception so a broken connection is rebuilt on the next probe. The instance/factory overloads never connect — they read the supplied multiplexer.
- registration policy: the string and instance overloads delegate to the factory overloads; every path adds a `HealthCheckRegistration(name ?? "redis", factory, failureStatus, tags, timeout)`; `failureStatus` null defaults to `HealthStatus.Unhealthy`.

[LOCAL_ADMISSION]:
- The probe is one `HealthContributorRow.Peer` row tagged `Store`, never a parallel cache-health surface — its `Probe` adapts `RedisHealthCheck.CheckHealthAsync` and registers through `HealthSurface.Register`, sharing `DeadlineClass.HealthProbe` and the cadence policy with every other contributor.
- The probe binds the SAME `IConnectionMultiplexer` the L2 cache lane registers (overload [03]/[04]), so the connection, endpoints, and cluster topology are defined once; the connection-string overloads with their static cache are the rejected form here because they create a second, probe-only connection outside the app's pooled multiplexer.
- A ping failure, a cluster node off `cluster_state:ok`, or a connect timeout is a typed `HealthCheckResult` with `FailureStatus`, folded by `HealthReport.Snapshot` into a `HealthSnapshot.Entry` — never a thrown exception crossing the fold.

[STACK]:
- health fold: `HealthContributorRow.Peer(name: "redis", tag: HealthContributorRow.Store, cadence, probe: ct => new ValueTask<HealthCheckResult>(redisCheck.CheckHealthAsync(ctx, ct)))` is the canonical row; `Observability/health#HEALTH_FOLD` `HealthSurface.Register(...)` admits it.
- degradation rail: a `Store`-tagged unhealthy entry drives `Observability/health#DEGRADATION_RAIL` `Rule(HealthContributorRow.Store, HealthStatus.Unhealthy, DegradationLevel.ReadOnly)` — a faulted Redis degrades the host to `ReadOnly` with the existing hysteresis, no probe-local branching.
- multiplexer reuse: the `IConnectionMultiplexer` is the one the `Microsoft.Extensions.Caching.StackExchangeRedis` L2 / `HybridCache` lane (`Runtime/resources`) registers as a singleton; passing it to overload [03]/[04] means the probe and the cache share one connection so a cache outage and a degraded probe are the same fact, and `TenantContext` cache-key partitioning (`Persistence/indexes#L2_CONTRIBUTION`) is unaffected by the probe.
- wire-health projection: the contributor result flows into `Wire/companion#HEALTH_SERVICE` `HealthServiceImpl.SetStatus` through the tag-predicate mapping, so cache reachability reaches the gRPC health service.
- resilience boundary: the probe deadline is `DeadlineClass.HealthProbe`, separate from the cache-access path; a slow probe degrades a row without consuming cache-operation budget.

[RAIL_LAW]:
- Package: `AspNetCore.HealthChecks.Redis`
- Owns: Redis reachability and cluster-state as one `store`-tagged contributor probe
- Accept: a shared `IConnectionMultiplexer`, endpoint ping, and cluster-state assertion
- Reject: a probe-only connection-string multiplexer beside the app's singleton, or a thrown probe failure crossing the health fold
