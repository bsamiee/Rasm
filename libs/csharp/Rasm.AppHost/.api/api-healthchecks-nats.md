# [RASM_APPHOST_API_HEALTHCHECKS_NATS]

`AspNetCore.HealthChecks.Nats` (Xabaril) is a single concrete `IHealthCheck` that proves NATS broker reachability by opening the connection through an admitted `NATS.Net` `INatsConnection` — the pooled connection the app already registers, resolved from DI unless a factory overrides it. It is one `remote`-tagged probe row feeding the AppHost capability-health fold, not a transport: the connection it exercises is the SAME `NatsConnection` the durable-drain/topics rail binds, so broker degradation routes through the existing `ReducedRemote` degradation rule. This is the `[V6]` broker-anchor probe — the NATS row lands as seed DATA tracking the Persistence egress sink roster, never a fixed enum drifting beside it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AspNetCore.HealthChecks.Nats`
- package: `AspNetCore.HealthChecks.Nats`
- license: `Apache-2.0`
- assembly: `HealthChecks.Nats`
- namespace: `HealthChecks.Nats`
- namespace: `Microsoft.Extensions.DependencyInjection`
- target: `net8.0` (also `netstandard2.0`); the `net10.0` consumer binds the `net8.0` asset, nullable-annotated
- dependency floor: `Microsoft.Extensions.Diagnostics.HealthChecks` (`IHealthCheck`/`HealthCheckResult`/`HealthCheckRegistration`) admitted in this folder; `NATS.Net` (`INatsConnection`/`INatsClient`/`NatsConnection`/`NatsOpts`) arrives transitively with this probe package at the central pin — the folder holds no direct `NATS.Net` reference, and the pooled connection it probes is the Persistence-owned egress connection resolved from DI
- asset: runtime library
- rail: health

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: probe family
- rail: health

[NATS_HEALTH_CHECK]:
- Symbol: `NatsHealthCheck`
- Type family: `IHealthCheck` probe
- Rail: health
- Reachability: NATS connection through an injected `INatsConnection`

The probe carries NO options type, NO probe-message factory, and NO result-detail dictionary — the connection health IS the reachability signal (contrast the Kafka probe's `KafkaHealthCheckOptions`/`MessageBuilder`). The registration extension `NatsHealthCheckBuilderExtensions` is the only other public type.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operation (`NatsHealthCheckBuilderExtensions`, default name `"nats"`)
- rail: health

[ADD_NATS]:
- Surface: `AddNats(this IHealthChecksBuilder, Func<IServiceProvider, INatsConnection>? clientFactory = null, string? name = "nats", HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)`
- Entry family: connection admission
- Resolution: A null factory resolves `NatsConnection` or `INatsConnection` from DI and shares the pooled connection.

[ENTRYPOINT_SCOPE]: probe operation
- rail: health

[CHECK_HEALTH_ASYNC]:
- Surface: `NatsHealthCheck.CheckHealthAsync(HealthCheckContext, CancellationToken)`
- Entry family: probe
- Connect: `((INatsClient)connection).ConnectAsync()`
- Result: Success returns `Healthy()`, and any exception returns `Unhealthy()`.

## [04]-[IMPLEMENTATION_LAW]

[NATS_TOPOLOGY]:
- one type: `NatsHealthCheck(INatsConnection connection) : IHealthCheck` (sealed, primary-constructor injected); the public surface is `NatsHealthCheck` plus the single `AddNats` extension. No options overload set, no sync mirror, no JetStream/consumer assertion.
- probe mechanics: `CheckHealthAsync` calls the private `TryConnectAsync(connection)` which `await`s `((INatsClient)natsConnection).ConnectAsync().ConfigureAwait(false)` and returns `HealthCheckResult.Healthy()`; ANY caught `Exception` returns `HealthCheckResult.Unhealthy()` with no message or exception payload. `ConnectAsync` is idempotent on an already-open `NatsConnection`, so the probe is a cheap liveness ping, not a reconnect.
- connection resolution: `AddNats` registers a `HealthCheckRegistration(name ?? "nats", sp => Factory(clientFactory, sp), failureStatus, tags, timeout)`; the factory invokes `clientFactory?.Invoke(sp)`, else resolves `sp.GetService<NatsConnection>()` and falls back to `sp.GetRequiredService<INatsConnection>()` — the concrete `NatsConnection` is preferred so the probe shares the app's pooled connection and its `NatsOpts` (URL, TLS, auth, ping cadence).
- registration policy: `failureStatus` null defaults to `HealthStatus.Unhealthy`; `tags`/`timeout` flow straight into the `HealthCheckRegistration`; the default check name is the literal `"nats"`.

[LOCAL_ADMISSION]:
- The probe is one `HealthContributorRow.Peer` row tagged `Remote`, never a parallel registration surface — its `Probe` adapts `NatsHealthCheck.CheckHealthAsync` to `Func<CancellationToken, ValueTask<HealthCheckResult>>` and registers through `HealthSurface.Register`, sharing `DeadlineClass.HealthProbe` and the cadence-as-`Delay`/`Period` policy with every other contributor.
- The `INatsConnection` the probe binds is the SAME pooled `NatsConnection` the durable-drain/broker rail composes (exactly as the npgsql row binds the shared `NpgsqlDataSource`); the probe re-uses the admitted connection and its `NatsOpts`, never inventing a second connection vocabulary, so a broker outage degrades the drain path and the probe in lockstep.
- A connect failure is a typed `HealthCheckResult` with `FailureStatus` (the probe returns `Unhealthy()` on any exception), folded by the `HealthReport.Snapshot` projection into a `HealthSnapshot.Entry` — never a thrown exception crossing the fold. The probe's message-less result is enriched at the row (name/tag) since the package attaches no detail.

[STACK]:
- health fold: `HealthContributorRow.Peer(name: "nats", tag: HealthContributorRow.Remote, cadence, probe: ct => new ValueTask<HealthCheckResult>(natsCheck.CheckHealthAsync(ctx, ct)))` is the canonical row; `HealthSurface.Register(...)` admits it, and `HealthReport.Snapshot` projects its result.
- roster tracking: this is the `[V6]` NATS anchor row — the `Broker` contributor set is seed DATA that TRACKS the Persistence `[V3]` egress sink roster, so the NATS spine sink lands one probe row here by construction; a sink admitted Persistence-side lands as one more `Remote`-tagged row, never a re-cut `DriverProbe` enum.
- degradation rail: a `Remote`-tagged unhealthy entry drives `Rule(HealthContributorRow.Remote, HealthStatus.Unhealthy, DegradationLevel.ReducedRemote)`; a faulted NATS broker degrades the host to `ReducedRemote` with the existing escalation-immediate and recovery-hysteresis semantics and no probe-local branching.
- connection reuse: the `NatsConnection` is the one the app registers for the CloudEvents/durable-drain publish path; the probe's `ConnectAsync` exercises the real pooled connection, so a broker partition surfaces as a degraded `nats` row rather than only failing production publishes silently.
- wire-health projection: the contributor result flows into `HealthServiceImpl.SetStatus` through the tag-predicate mapping (`ServingStatus.Serving` on healthy or degraded, `NotServing` on unhealthy), so broker readiness reaches the standard gRPC health service without a second status path.
- resilience boundary: the probe deadline is `DeadlineClass.HealthProbe`, distinct from the `Polly.Core` outbound publish pipeline (`Wire/outbound`); the health probe never shares the publish retry budget.

[RAIL_LAW]:
- Package: `AspNetCore.HealthChecks.Nats`
- Owns: NATS broker reachability as one `remote`-tagged contributor probe over the shared `INatsConnection`
- Accept: the pooled `NatsConnection` resolved from DI (or a factory), and a bounded probe cadence
- Reject: a second NATS connection vocabulary, a JetStream/consumer assertion in the probe, a fixed `DriverProbe` enum drifting beside the sink roster, or a thrown probe failure crossing the health fold
