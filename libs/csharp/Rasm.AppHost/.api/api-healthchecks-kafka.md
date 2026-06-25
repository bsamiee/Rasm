# [RASM_APPHOST_API_HEALTHCHECKS_KAFKA]

`AspNetCore.HealthChecks.Kafka` (Xabaril) is a single concrete `IHealthCheck` that proves Kafka broker write-readiness by producing a probe message to a topic through an admitted `Confluent.Kafka` `IProducer<string, string>`. It is one `remote`-tagged probe row feeding the AppHost capability-health fold, not a transport — the producer it builds is the same `ProducerConfig` shape the CloudEvents-over-Kafka topics rail already binds, so broker degradation routes through the existing `ReducedRemote` degradation rule.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AspNetCore.HealthChecks.Kafka`
- package: `AspNetCore.HealthChecks.Kafka`
- version: `9.0.0`
- license: `Apache-2.0`
- assembly: `HealthChecks.Kafka`
- namespace: `HealthChecks.Kafka`
- namespace: `Microsoft.Extensions.DependencyInjection`
- target: `net8.0` (also `netstandard2.0`); the `net10.0` consumer binds the `net8.0` asset, `RefSafetyRules(11)` nullable-annotated
- dependency floor: `Microsoft.Extensions.Diagnostics.HealthChecks` (abstractions for `IHealthCheck`/`HealthCheckResult`/`HealthCheckRegistration`), `Confluent.Kafka` (`ProducerConfig`, `ProducerBuilder<,>`, `IProducer<,>`, `Message<,>`) — both admitted in this folder at higher pins
- asset: runtime library
- rail: health

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: probe and options family
- rail: health

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]        | [RAIL]                                       |
| :-----: | :------------------------ | :------------------- | :------------------------------------------- |
|  [01]   | `KafkaHealthCheck`        | `IHealthCheck` probe | broker write-readiness, `IDisposable`        |
|  [02]   | `KafkaHealthCheckOptions` | probe options        | producer config + topic + builder + message  |

[PUBLIC_MEMBER_SCOPE]: `KafkaHealthCheckOptions`
- rail: health

| [INDEX] | [MEMBER]                                                       | [SHAPE]                | [NOTE]                                                       |
| :-----: | :------------------------------------------------------------ | :--------------------- | :---------------------------------------------------------- |
|  [01]   | `ProducerConfig Configuration { get; set; }`                 | required config        | the Kafka connection config; null at probe build throws     |
|  [02]   | `string Topic { get; set; }`                                 | target topic           | defaults `"healthchecks-topic"`                             |
|  [03]   | `Action<ProducerBuilder<string,string>>? Configure { get; }` | builder hook           | applied once before `Build()` — serializers, logger, OAuth  |
|  [04]   | `Func<KafkaHealthCheckOptions, Message<string,string>> MessageBuilder` | probe message factory | defaults key `healthcheck-key`, timestamped value           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations (`KafkaHealthCheckBuilderExtensions`, default name `"kafka"`)
- rail: health

| [INDEX] | [SURFACE]                                                                                                                          | [ENTRY_FAMILY]   | [RAIL]                                       |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------- | :--------------- | :------------------------------------------- |
|  [01]   | `AddKafka(this IHealthChecksBuilder, ProducerConfig config, string topic = "healthchecks-topic", string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout)` | config admission | registers a `KafkaHealthCheck` singleton     |
|  [02]   | `AddKafka(this IHealthChecksBuilder, Action<ProducerConfig> setup, string topic = "healthchecks-topic", string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout)` | setup admission  | builds `ProducerConfig` via delegate         |
|  [03]   | `AddKafka(this IHealthChecksBuilder, KafkaHealthCheckOptions options, string? name, HealthStatus? failureStatus, IEnumerable<string>? tags, TimeSpan? timeout)` | options admission | full options including `Configure`/`MessageBuilder` |

[ENTRYPOINT_SCOPE]: probe operation
- rail: health

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [RAIL]                                                |
| :-----: | :-------------------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `KafkaHealthCheck.CheckHealthAsync(HealthCheckContext, CancellationToken)` | probe          | `ProduceAsync` to topic; a `NotPersisted` delivery status maps to `FailureStatus` |
|  [02]   | `KafkaHealthCheck.Dispose()`                                          | resource       | disposes the lazily-built producer                    |

## [04]-[IMPLEMENTATION_LAW]

[KAFKA_TOPOLOGY]:
- one type: `KafkaHealthCheck : IHealthCheck, IDisposable`; the public API surface is `KafkaHealthCheck`, `KafkaHealthCheckOptions`, and the three `AddKafka` extension overloads — no async mirror, no health-detail data dictionary, no per-partition probe.
- probe mechanics: `CheckHealthAsync` lazily builds one `IProducer<string,string>` from `Configuration` (caching it on the instance), invokes `MessageBuilder(options)`, then `await producer.ProduceAsync(Topic ?? "healthchecks-topic", message, ct)`. A `PersistenceStatus.NotPersisted` delivery (enum value 0) returns `new HealthCheckResult(context.Registration.FailureStatus, "Message is not persisted ...")`; `PossiblyPersisted` and `Persisted` pass; any exception returns `new HealthCheckResult(FailureStatus, null, exception)`; the green path returns `HealthCheckResult.Healthy()`.
- write semantics: this probe PRODUCES a real record to the configured topic on every evaluation — it is a write-path liveness check, not a passive metadata read. The topic must be one the broker accepts (auto-create or a pre-provisioned probe topic) and the cadence must be bounded so the probe does not flood a partition.
- builder hook: `Configure` runs against the `ProducerBuilder<string,string>` before `Build()`, the seam for `SetKeySerializer`/`SetValueSerializer`, statistics/log/error handlers, and OAuth bearer-token refresh; serializers default to UTF-8 string serializers because the probe message is `<string,string>`.
- registration policy: overloads [01]/[02] register the `KafkaHealthCheck` as a singleton and add a `HealthCheckRegistration(name ?? "kafka", sp => sp.GetRequiredService<KafkaHealthCheck>(), failureStatus, tags, timeout)`; `failureStatus` null defaults to `HealthStatus.Unhealthy`; `tags`/`timeout` flow straight into the registration.

[LOCAL_ADMISSION]:
- The probe is one `HealthContributorRow.Peer` row tagged `Remote`, never a parallel registration surface — its `Probe` adapts `KafkaHealthCheck.CheckHealthAsync` to `Func<CancellationToken, ValueTask<HealthCheckResult>>` and registers through `HealthSurface.Register`, sharing `DeadlineClass.HealthProbe` and the cadence-as-`Delay`/`Period` policy with every other contributor.
- The `ProducerConfig` the probe reads is the SAME broker/SASL/SSL configuration the CloudEvents-over-Kafka topics rail builds its `IProducer` from; the probe does not invent a second connection vocabulary, it re-binds the admitted config so a broker outage degrades the publish path and the probe in lockstep.
- A non-`Persisted` delivery or a connect/auth exception is a typed `HealthCheckResult` with `FailureStatus`, folded by the `HealthReport.Snapshot` projection into a `HealthSnapshot.Entry` — never a thrown exception crossing the fold.
- The probe topic is an explicit operations decision (a dedicated `healthchecks` topic), not the production CloudEvents topics, so health writes never pollute the durable event log the outbox replays.

[STACK]:
- health fold: `HealthContributorRow.Peer(name: "kafka", tag: HealthContributorRow.Remote, cadence, probe: ct => new ValueTask<HealthCheckResult>(kafkaCheck.CheckHealthAsync(ctx, ct)))` is the canonical row; `Observability/health#HEALTH_FOLD` `HealthSurface.Register(...)` admits it and `HealthReport.Snapshot` projects its result.
- degradation rail: a `Remote`-tagged unhealthy entry drives `Observability/health#DEGRADATION_RAIL` `Rule(HealthContributorRow.Remote, …, DegradationLevel.ReducedRemote)` — a faulted Kafka broker degrades the host to `ReducedRemote` with the existing escalation-immediate/recovery-hysteresis semantics, no probe-local branching.
- producer reuse: the `ProducerConfig` is the admitted `Confluent.Kafka` config the `Wire/topics`/`Wire/outbox` CloudEvents-over-Kafka rail composes (`CloudNative.CloudEvents.Kafka` envelope + `Confluent.SchemaRegistry` serdes ride the same broker); the probe `Configure` hook can install the same `SetValueSerializer` the production producer uses so the probe exercises the real serializer path.
- wire-health projection: the contributor result flows into `Wire/companion#HEALTH_SERVICE` `Grpc.HealthCheck.HealthServiceImpl.SetStatus` through the tag-predicate mapping (`ServingStatus.Serving` on healthy/degraded, `NotServing` on unhealthy), so broker readiness reaches the standard gRPC health service without a second status path.
- resilience boundary: the probe deadline is `DeadlineClass.HealthProbe`, distinct from the `Polly.Core` outbound publish pipeline (`Wire/outbound`); the health probe never shares the publish retry budget, so a slow probe degrades a row without consuming production-publish permits.

[RAIL_LAW]:
- Package: `AspNetCore.HealthChecks.Kafka`
- Owns: Kafka broker write-readiness as one `remote`-tagged contributor probe
- Accept: a shared `ProducerConfig`, a dedicated probe topic, and a bounded probe cadence
- Reject: a parallel Kafka connection vocabulary, probing the production CloudEvents topics, or a thrown probe failure crossing the health fold
