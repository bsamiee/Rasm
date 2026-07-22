# [RASM_API_OTEL_INSTRUMENTATION_CONFLUENTKAFKA]

`OpenTelemetry.Instrumentation.ConfluentKafka` owns messaging-semconv emission on the Kafka wire: instrumented builders subclass the Confluent builders so every client they mint spans and meters its own traffic, publish injects W3C context into Kafka `Headers`, and consume extracts it back. One name — `OpenTelemetry.Instrumentation.ConfluentKafka` — serves both its `ActivitySource` and its `Meter`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.ConfluentKafka`
- package: `OpenTelemetry.Instrumentation.ConfluentKafka`
- assembly: `OpenTelemetry.Instrumentation.ConfluentKafka`
- namespace: `Confluent.Kafka`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`
- rail: transport instrumentation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumented clients, their telemetry options, and the admission holders

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :---------------------------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `InstrumentedProducerBuilder<TKey,TValue>`                  | class         | `ProducerBuilder` subclass; span+meter `IProducer` mint |
|  [02]   | `InstrumentedConsumerBuilder<TKey,TValue>`                  | class         | `ConsumerBuilder` subclass; span+meter `IConsumer` mint |
|  [03]   | `ConfluentKafkaInstrumentedProducerBuilderOptions`          | class         | `EnableTraces`/`EnableMetrics` for a code-built lift    |
|  [04]   | `ConfluentKafkaInstrumentedConsumerBuilderOptions`          | class         | `EnableTraces`/`EnableMetrics` for a code-built lift    |
|  [05]   | `OpenTelemetryConsumeAndProcessMessageHandler<TKey,TValue>` | delegate      | `(ConsumeResult, Activity?, CancellationToken)` async   |
|  [06]   | `OpenTelemetryProducerBuilderExtensions`                    | class         | producer lift off a live `ProducerBuilder`              |
|  [07]   | `OpenTelemetryConsumerBuilderExtensions`                    | class         | consumer lift off a live `ConsumerBuilder`              |
|  [08]   | `OpenTelemetryConsumeResultExtensions`                      | class         | header context join and the process-span wrapper        |
|  [09]   | `TracerProviderBuilderExtensions`                           | class         | Kafka span admission on the tracer provider             |
|  [10]   | `MeterProviderBuilderExtensions`                            | class         | Kafka meter admission on the meter provider             |

[METER_ROSTER]: instruments on the shared meter name, every point tagged `messaging.system`, `messaging.operation.name`, `messaging.operation.type`, and the destination, partition, and consumer-group keys the operation resolves

| [INDEX] | [INSTRUMENT]                          | [UNIT]      | [CAPABILITY]                                       |
| :-----: | :------------------------------------ | :---------- | :------------------------------------------------- |
|  [01]   | `messaging.client.operation.duration` | `s`         | histogram under an `InstrumentAdvice<double>` hint |
|  [02]   | `messaging.client.sent.messages`      | `{message}` | producer send attempts                             |
|  [03]   | `messaging.client.consumed.messages`  | `{message}` | messages delivered to the application              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction, root admission, and the consume seam; every verb closes on the `TKey,TValue` closure

| [INDEX] | [SURFACE]                                                                                | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `InstrumentedProducerBuilder(IEnumerable<KeyValuePair<string,string>>)`                  | ctor     | trim-safe mint from config rows  |
|  [02]   | `InstrumentedConsumerBuilder(IEnumerable<KeyValuePair<string,string>>)`                  | ctor     | trim-safe mint from config rows  |
|  [03]   | `InstrumentedProducerBuilder.Build() -> IProducer`                                       | instance | instrumented producer mint       |
|  [04]   | `InstrumentedConsumerBuilder.Build() -> IConsumer`                                       | instance | instrumented consumer mint       |
|  [05]   | `AsInstrumentedProducerBuilder(ProducerBuilder) -> InstrumentedProducerBuilder`          | static   | reflection lift, live builder    |
|  [06]   | `AsInstrumentedConsumerBuilder(ConsumerBuilder) -> InstrumentedConsumerBuilder`          | static   | reflection lift, live builder    |
|  [07]   | `AddKafkaProducerInstrumentation<TKey,TValue>`                                           | static   | bare, named, and bound overloads |
|  [08]   | `AddKafkaConsumerInstrumentation<TKey,TValue>`                                           | static   | consumer-side overload family    |
|  [09]   | `ConsumeAndProcessMessageAsync(IConsumer, OpenTelemetryConsumeAndProcessMessageHandler)` | static   | process span over one handler    |
|  [10]   | `TryExtractPropagationContext(ConsumeResult, out PropagationContext) -> bool`            | static   | W3C context off message headers  |

- `ConsumeAndProcessMessageAsync`: binds only a consumer `InstrumentedConsumerBuilder.Build()` minted; any other instance faults at the call.
- `AddKafkaProducerInstrumentation`: builder-less overloads resolve the instrumented builder from DI, keyed when the name argument rides, and set the matching flag on it.
- `TryExtractPropagationContext`: a header-free message returns `true` on the default context, so callers discriminate on the extracted `ActivityContext`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- build root: `InstrumentedProducerBuilder` and `InstrumentedConsumerBuilder` are the client mints on this wire, so admission binds the builder instance the root registered or resolves.
- context root: publish injects span context and current `Baggage` into Kafka `Headers` through `Propagators.DefaultTextMapPropagator`; consume extracts through the same propagator and hangs the producer context on the process span as a link, so each leg keeps its own trace id.
- flag root: `EnableTraces` and `EnableMetrics` ride the two options classes; a root-registered admission verb sets its own flag on the builder it binds, and a code-built lift carries the options record.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): each admission verb folds `AddSource`/`AddMeter` on the shared name and registers the instrumentation object; both legs read `Propagators.DefaultTextMapPropagator`, so the root's propagator composite fixes the header format.
- `System.Diagnostics.DiagnosticSource`(`api-diagnostics-activity.md`): the process span carries the extracted producer context as an `ActivityLink`, and a handler fault stamps `Activity.SetStatus(ActivityStatusCode.Error, …)` with an `error.type` tag before the throw resumes.
- `System.Diagnostics.Metrics`(`api-diagnostics-metrics.md`): the duration histogram ships its own `InstrumentAdvice<double>` boundary hint, so a provider `AddView` row re-buckets only where a backend rejects the advised shape.
- within-lib: one closure pair composes ctor -> `Build()` -> `ConsumeAndProcessMessageAsync`, the handler's `Activity` parameter carrying the process span the caller enriches and the `ConsumeResult` return driving the commit; a loop owning its own span joins through `TryExtractPropagationContext`.

[LOCAL_ADMISSION]:
- Composition-root-only, at the AppHost wire root that owns Kafka clients; one closure pair registers per message shape across both providers.
- librdkafka `StatisticsHandler` JSON stays the broker-ops lane on the Confluent config rows; these client instruments carry the app-side leg on the OTel providers.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.ConfluentKafka`
- Owns: Kafka messaging spans, the client instrument roster, and header-borne context on both legs
- Accept: instrumented-builder construction with per-closure admission at the root
- Reject: hand-rolled messaging-semconv spans over plain Confluent builders
