# [RASM_APPHOST_API_HEALTHCHECKS_KAFKA]

`AspNetCore.HealthChecks.Kafka` (Xabaril) is one concrete `IHealthCheck` proving Kafka broker write-readiness by producing a probe message to a topic through an admitted `Confluent.Kafka` `IProducer<string, string>`. Its producer binds the same `ProducerConfig` the CloudEvents-over-Kafka topics rail composes, so it enters the AppHost capability-health fold as one `remote`-tagged probe row, never a second transport.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AspNetCore.HealthChecks.Kafka`
- package: `AspNetCore.HealthChecks.Kafka` (`Apache-2.0`, Xabaril)
- assembly: `HealthChecks.Kafka`
- namespace: `HealthChecks.Kafka`, `Microsoft.Extensions.DependencyInjection`
- target: `net8.0` (also `netstandard2.0`); the `net10.0` consumer binds the `net8.0` asset, `RefSafetyRules(11)` nullable-annotated
- depends: `Microsoft.Extensions.Diagnostics.HealthChecks` (`IHealthCheck`, `HealthCheckResult`, `HealthCheckRegistration`), `Confluent.Kafka` (`ProducerConfig`, `ProducerBuilder<,>`, `IProducer<,>`, `Message<,>`)
- asset: runtime library
- rail: health

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: probe and options family

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]        | [CAPABILITY]                             |
| :-----: | :------------------------ | :------------------- | :--------------------------------------- |
|  [01]   | `KafkaHealthCheck`        | `IHealthCheck` probe | broker write-readiness, `IDisposable`    |
|  [02]   | `KafkaHealthCheckOptions` | probe options        | producer config, topic, builder, factory |

[PUBLIC_MEMBER_SCOPE]: `KafkaHealthCheckOptions`

| [INDEX] | [MEMBER]         | [TYPE]                                                   | [CAPABILITY]                                 |
| :-----: | :--------------- | :------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `Configuration`  | `ProducerConfig`                                         | required config; null at build throws        |
|  [02]   | `Topic`          | `string`                                                 | target topic, default `"healthchecks-topic"` |
|  [03]   | `Configure`      | `Action<ProducerBuilder<string, string>>?`               | builder hook run once before `Build()`       |
|  [04]   | `MessageBuilder` | `Func<KafkaHealthCheckOptions, Message<string, string>>` | probe factory, default key `healthcheck-key` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration operations (`KafkaHealthCheckBuilderExtensions`, default name `"kafka"`)

Every `AddKafka` overload extends `IHealthChecksBuilder` and closes with `string? name`, `HealthStatus? failureStatus`, `IEnumerable<string>? tags`, `TimeSpan? timeout`; the config and setup forms carry `string topic = "healthchecks-topic"` before that suffix.

| [INDEX] | [SURFACE]                                  | [CAPABILITY]                                                     |
| :-----: | :----------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `AddKafka(ProducerConfig, string)`         | register the `KafkaHealthCheck` singleton from a config          |
|  [02]   | `AddKafka(Action<ProducerConfig>, string)` | build the config through a delegate                              |
|  [03]   | `AddKafka(KafkaHealthCheckOptions)`        | admit the full options, binding `Configure` and `MessageBuilder` |

[ENTRYPOINT_SCOPE]: `KafkaHealthCheck` probe operations

| [INDEX] | [SURFACE]                                                 | [CAPABILITY]                                                 |
| :-----: | :-------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `CheckHealthAsync(HealthCheckContext, CancellationToken)` | produce to the topic; `NotPersisted` maps to `FailureStatus` |
|  [02]   | `Dispose()`                                               | dispose the lazily built producer                            |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `KafkaHealthCheck : IHealthCheck, IDisposable`, `KafkaHealthCheckOptions`, and the three `AddKafka` overloads are the entire public surface.
- probe mechanics: `CheckHealthAsync` lazily builds one `IProducer<string, string>` from `Configuration` and caches it on the instance, invokes `MessageBuilder(options)`, then `ProduceAsync(Topic ?? "healthchecks-topic", message, ct)`; a `PersistenceStatus.NotPersisted` delivery returns `HealthCheckResult(FailureStatus, "Message is not persisted…")`, `PossiblyPersisted` and `Persisted` pass, any exception returns `HealthCheckResult(FailureStatus, null, exception)`, and the green path returns `Healthy()`.
- write semantics: the probe produces a real record to the configured topic on every evaluation — a write-path liveness check, not a passive metadata read — so the topic is broker-accepted (auto-create or a pre-provisioned probe topic) and the cadence is bounded so the probe never floods a partition.
- builder hook: `Configure` runs against `ProducerBuilder<string, string>` before `Build()`, the seam for `SetKeySerializer`/`SetValueSerializer`, statistics/log/error handlers, and OAuth bearer-token refresh; serializers default to UTF-8 string serializers because the probe message is `<string, string>`.
- registration policy: overloads [01] and [02] register `KafkaHealthCheck` as a singleton and add `HealthCheckRegistration(name ?? "kafka", sp => sp.GetRequiredService<KafkaHealthCheck>(), failureStatus, tags, timeout)`; `failureStatus` null defaults to `HealthStatus.Unhealthy`, and `tags`/`timeout` flow into the registration.

[STACKING]:
- `Confluent.Kafka`: `ProducerConfig` is the admitted broker config the `Wire/topics`/`Wire/outbox` CloudEvents-over-Kafka rail composes, so `Configure` installs the same `SetValueSerializer` the production producer binds and the probe exercises the real serializer path.
- `Observability/health.md`: `HealthContributorRow.Driver(DriverProbe.Kafka, cadence, kafkaCheck)` adapts `CheckHealthAsync` into one deploy-gated `remote`-tagged contributor row that `HealthSurface.Register` admits and `HealthReport.Snapshot` projects, and a faulted broker drives that owner's `ReducedRemote` degradation.

[LOCAL_ADMISSION]:
- `ProducerConfig` carries the same broker/SASL/SSL configuration the CloudEvents-over-Kafka topics rail builds its `IProducer` from; the probe re-binds the admitted config rather than minting a second connection vocabulary, so a broker outage degrades the publish path and the probe in lockstep.
- A non-`Persisted` delivery or a connect/auth exception is a typed `HealthCheckResult` carrying `FailureStatus`, folded into a `HealthSnapshot.Entry` rather than thrown across the fold.
- Health writes target a dedicated `healthchecks` topic, never the production CloudEvents topics, so they never pollute the durable event log the outbox replays.

[RAIL_LAW]:
- Package: `AspNetCore.HealthChecks.Kafka`
- Owns: Kafka broker write-readiness as one `remote`-tagged contributor probe
- Accept: a shared `ProducerConfig`, a dedicated probe topic, and a bounded probe cadence
- Reject: a parallel Kafka connection vocabulary, probing the production CloudEvents topics, or a thrown probe failure crossing the health fold
