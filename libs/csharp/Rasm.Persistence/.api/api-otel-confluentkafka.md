# [RASM_PERSISTENCE_API_OTEL_CONFLUENTKAFKA]

`OpenTelemetry.Instrumentation.ConfluentKafka` wraps the Confluent producer and consumer builders in instrumented twins that emit messaging spans and client meters, with propagation-context extraction off a consumed result. Instrumented builders replace the plain builders at the Kafka egress-sink construction seam; trace and meter admission ride the provider builders at the AppHost composition root.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.ConfluentKafka`
- package: `OpenTelemetry.Instrumentation.ConfluentKafka`
- assembly: `OpenTelemetry.Instrumentation.ConfluentKafka`
- namespace: `Confluent.Kafka` (builder twins and extensions)
- namespace: `OpenTelemetry.Trace`
- namespace: `OpenTelemetry.Metrics`
- driver package: `Confluent.Kafka` (declares `ProducerBuilder`/`ConsumerBuilder`/`ConsumeResult` the twins wrap)
- asset: runtime library
- rail: telemetry

## [02]-[PUBLIC_TYPES]

[TELEMETRY_TYPES]: instrumented builder family (namespace `Confluent.Kafka`)
- rail: telemetry

| [INDEX] | [SYMBOL]                                        | [PACKAGE_ROLE]      | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------- | :------------------ | :------------------------------------------------ |
|  [01]   | `InstrumentedProducerBuilder<TKey,TValue>`      | builder twin        | produce spans and client meters at build          |
|  [02]   | `InstrumentedConsumerBuilder<TKey,TValue>`      | builder twin        | consume spans and client meters at build          |
|  [03]   | `OpenTelemetryProducerBuilderExtensions`        | adapter extension   | `AsInstrumentedProducerBuilder` conversion        |
|  [04]   | `OpenTelemetryConsumerBuilderExtensions`        | adapter extension   | `AsInstrumentedConsumerBuilder` conversion        |
|  [05]   | `OpenTelemetryConsumeResultExtensions`          | consume extension   | process-loop span + propagation-context extract   |
|  [06]   | `ConfluentKafkaInstrumentedProducerBuilderOptions` | options value    | `EnableTraces`/`EnableMetrics` toggles            |
|  [07]   | `ConfluentKafkaInstrumentedConsumerBuilderOptions` | options value    | `EnableTraces`/`EnableMetrics` toggles            |
|  [08]   | `MeterProviderBuilderExtensions`                | builder extension   | Kafka client meter admission                      |
|  [09]   | `TracerProviderBuilderExtensions`               | builder extension   | Kafka producer/consumer trace admission           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder conversion
- rail: telemetry

- Surface: `AsInstrumentedProducerBuilder<TKey,TValue>(ProducerBuilder<TKey,TValue>)` / overload taking `ConfluentKafkaInstrumentedProducerBuilderOptions` — converts the sink's plain builder into the instrumented twin at construction.
- Surface: `AsInstrumentedConsumerBuilder<TKey,TValue>(ConsumerBuilder<TKey,TValue>)` / options overload — the consumer-side conversion.
- Surface: `InstrumentedProducerBuilder<TKey,TValue>(IEnumerable<KeyValuePair<string,string>>)` / consumer twin — direct construction from config when no plain builder pre-exists; `Build()` yields the instrumented client.

[ENTRYPOINT_SCOPE]: provider registration
- rail: telemetry

- Surface: `AddKafkaProducerInstrumentation<TKey,TValue>(TracerProviderBuilder)` / overloads taking a name, an `InstrumentedProducerBuilder<TKey,TValue>`, or both — trace admission per instrumented builder.
- Surface: `AddKafkaConsumerInstrumentation<TKey,TValue>(TracerProviderBuilder)` / same overload set — consumer trace admission.
- Surface: `AddKafkaProducerInstrumentation<TKey,TValue>(MeterProviderBuilder)` / `AddKafkaConsumerInstrumentation<TKey,TValue>(MeterProviderBuilder)` with the same overload set — meter admission.

[ENTRYPOINT_SCOPE]: consume-loop processing
- rail: telemetry

- Surface: `ConsumeAndProcessMessageAsync<TKey,TValue>(IConsumer<TKey,TValue>, OpenTelemetryConsumeAndProcessMessageHandler<TKey,TValue>[, CancellationToken])` — wraps one consume-plus-handle round in a processing span.
- Surface: `TryExtractPropagationContext<TKey,TValue>(ConsumeResult<TKey,TValue>, out PropagationContext)` — reads the W3C context off the consumed headers for a hand-driven loop.

## [04]-[IMPLEMENTATION_LAW]

[TELEMETRY_PROFILE]:
- builder twins: `InstrumentedProducerBuilder`/`InstrumentedConsumerBuilder` in namespace `Confluent.Kafka`, converted via `AsInstrumented*Builder` or constructed from config
- options: `EnableTraces`/`EnableMetrics` on the two options values gate emission per builder outside DI registration
- tracer/meter roots: the generic `AddKafka{Producer,Consumer}Instrumentation` extensions on both provider builders, name-keyed overloads for multiple sinks

[STACKING]:
- `EgressSink.Kafka` constructs its producer through the instrumented builder, so delivery spans continue the `traceparent` the CloudEvents envelope already carries as an extension attribute — one trace from op-log drain to broker ack.
- `TryExtractPropagationContext` is the consumer-side counterpart for any future inbound Kafka leg, aligning with the estate law that Kafka context rides message `Headers`.
- Trace and meter admission compose at the AppHost composition root beside the `Npgsql` rows; the sink page owns builder construction, never provider registration.

[LOCAL_ADMISSION]:
- One instrumented builder replaces the plain builder at the sink-construction seam; a second plain producer beside it forks the telemetry surface.
- Span and meter names are package facts, not Persistence vocabulary; dashboards bind the messaging semconv streams the package emits.
- Provider registration cannot leak into store profiles or the egress pump fold.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.ConfluentKafka`
- Owns: Kafka producer/consumer span and meter emission over the Confluent client
- Accept: instrumented-builder construction at the sink seam; provider registration rows at the composition root
- Reject: hand-rolled messaging-semconv spans over the plain builder; header-context injection code beside the instrumented producer
