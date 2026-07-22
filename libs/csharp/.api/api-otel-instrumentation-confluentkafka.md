# [RASM_API_OTEL_INSTRUMENTATION_CONFLUENTKAFKA]

`OpenTelemetry.Instrumentation.ConfluentKafka` is the messaging-semconv owner for the Kafka wire: instrumented producer/consumer builders emit publish and receive spans with W3C context riding Kafka `Headers`, and the client meters carry operation-duration histograms and sent/consumed counters. Its `ActivitySource` and `Meter` share one name — `OpenTelemetry.Instrumentation.ConfluentKafka` — admitted at the root like every foreign source.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.ConfluentKafka`
- package: `OpenTelemetry.Instrumentation.ConfluentKafka`
- assembly: `OpenTelemetry.Instrumentation.ConfluentKafka`
- namespace: `Confluent.Kafka`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`
- asset: runtime library
- rail: transport instrumentation

## [02]-[PUBLIC_TYPES]

[BUILDER_TYPES]: instrumented client builders (namespace `Confluent.Kafka`)
- rail: transport instrumentation

| [INDEX] | [SYMBOL]                                                    | [PACKAGE_ROLE]   | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------- | :--------------- | :----------------------------------------------- |
|  [01]   | `InstrumentedProducerBuilder<TKey,TValue>`                  | producer builder | span+meter-emitting `Build()`                    |
|  [02]   | `InstrumentedConsumerBuilder<TKey,TValue>`                  | consumer builder | span+meter-emitting `Build()`                    |
|  [03]   | `ConfluentKafkaInstrumentedProducerBuilderOptions`          | producer options | `EnableTraces` / `EnableMetrics`                 |
|  [04]   | `ConfluentKafkaInstrumentedConsumerBuilderOptions`          | consumer options | `EnableTraces` / `EnableMetrics`                 |
|  [05]   | `OpenTelemetryConsumeResultExtensions`                      | consume seam     | context extraction + processing-span wrapper     |
|  [06]   | `OpenTelemetryConsumeAndProcessMessageHandler<TKey,TValue>` | handler delegate | typed processing callback under the receive span |

`OpenTelemetryProducerBuilderExtensions.AsInstrumentedProducerBuilder<TKey,TValue>()` and `OpenTelemetryConsumerBuilderExtensions.AsInstrumentedConsumerBuilder<TKey,TValue>()` lift an existing `ProducerBuilder`/`ConsumerBuilder`, each with an options overload; outside DI both `Enable*` flags default off, so standalone lifts state them explicitly.

`OpenTelemetryConsumeResultExtensions` supplies `TryExtractPropagationContext(ConsumeResult<TKey,TValue>, out PropagationContext)` for manual join and `ConsumeAndProcessMessageAsync(IConsumer<TKey,TValue>, OpenTelemetryConsumeAndProcessMessageHandler<TKey,TValue>[, CancellationToken])` for the wrapped receive-process span pair.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: admission and lift
- rail: transport instrumentation

| [INDEX] | [SURFACE]                                      | [KIND]                   | [CAPABILITY]                                               |
| :-----: | :--------------------------------------------- | :----------------------- | :--------------------------------------------------------- |
|  [01]   | `AddKafkaProducerInstrumentation<TKey,TValue>` | trace + metric admission | bare, named, and builder-bound overloads on both providers |
|  [02]   | `AddKafkaConsumerInstrumentation<TKey,TValue>` | trace + metric admission | same overload family on the consumer side                  |
|  [03]   | `AsInstrumentedProducerBuilder`                | builder lift             | wraps an existing `ProducerBuilder`, optional options      |
|  [04]   | `AsInstrumentedConsumerBuilder`                | builder lift             | wraps an existing `ConsumerBuilder`, optional options      |
|  [05]   | `ConsumeAndProcessMessageAsync`                | consume wrapper          | receive + process spans over one typed handler             |
|  [06]   | `TryExtractPropagationContext`                 | manual join              | W3C context off `ConsumeResult` headers                    |

## [04]-[IMPLEMENTATION_LAW]

[KAFKA_TOPOLOGY]:
- build root: the wire owner constructs clients through the instrumented builders; the root registers `AddKafkaProducerInstrumentation`/`AddKafkaConsumerInstrumentation` per key/value closure on both providers
- context root: publish injects W3C context into Kafka `Headers`; consume joins through `ConsumeAndProcessMessageAsync` or the manual `TryExtractPropagationContext` seam

[STACKING]:
- `Confluent.Kafka`: instrumented builders wrap the client library's own builders — producer/consumer configuration stays Confluent-owned rows, and instrumentation adds no config vocabulary.
- `OpenTelemetry`(`api-opentelemetry.md`): source and meter admit by the shared instrumentation name; exemplars link Kafka operation histograms to the publishing span under the root's trace-based filter.
- librdkafka `StatisticsHandler` JSON is the broker-ops lane, disjoint from these app-side signals; the two never merge into one instrument roster.

[LOCAL_ADMISSION]:
- Composition-root-only, at the AppHost wire root that owns Kafka clients; a generic-closure pair registers once per message shape.
- Standalone builder lifts spell `EnableTraces`/`EnableMetrics` explicitly — the off-by-default flags make an unconfigured lift a silent no-op.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.ConfluentKafka`
- Owns: Kafka messaging spans, client meters, and header-borne context propagation
- Accept: instrumented-builder construction with root-registered admission per closure
- Reject: hand-rolled messaging-semconv spans over raw Confluent builders; header injection code beside the instrumented publish path
