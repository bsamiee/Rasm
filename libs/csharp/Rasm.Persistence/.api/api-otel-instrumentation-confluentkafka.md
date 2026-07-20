# [RASM_PERSISTENCE_API_OTEL_INSTRUMENTATION_CONFLUENTKAFKA]

`OpenTelemetry.Instrumentation.ConfluentKafka` wraps the egress Kafka leg's producer builder in the instrumented twin, so delivery spans and client meters continue the CloudEvents `traceparent` from op-log drain to broker ack. Substrate canonical members live at `libs/csharp/.api/api-otel-instrumentation-confluentkafka.md`; this overlay carries only the Persistence delta — the sink-construction seam and the egress-only consumer ruling.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-otel-instrumentation-confluentkafka.md`
- builder-twin and options type roster, admission/lift call-shape tables, and package/asset facts live on the substrate catalog — this overlay never re-states them
- rail: transport instrumentation

## [02]-[PERSISTENCE_BINDINGS]

- `Version/egress#EGRESS_SINK` `EgressSink.Kafka` builds its producer through `AsInstrumentedProducerBuilder` under `ConfluentKafkaInstrumentedProducerBuilderOptions` (`EnableTraces`/`EnableMetrics` explicit — the flags default off outside DI), the one instrumented-builder seam at the leg's composition.
- `AddKafkaProducerInstrumentation` registers on the tracer and meter builders at the AppHost root for the egress leg's key/value closure; the sink page owns builder construction, never provider registration.

## [03]-[IMPLEMENTATION_LAW]

[EGRESS_ONLY]:
- consumer twins (`InstrumentedConsumerBuilder`, `AsInstrumentedConsumerBuilder`, `AddKafkaConsumerInstrumentation`, `TryExtractPropagationContext`, `ConsumeAndProcessMessageAsync`) bind at the inbound CDC ingress owner the `IDEAS` `[PERS-V4]` card holds — the sink family is egress-only, so no consumer member binds in this folder today

[LOCAL_ADMISSION]:
- One instrumented builder replaces the plain builder at the sink seam; a second plain producer beside it forks the telemetry surface.
- Span and meter names are package facts, never Persistence vocabulary; provider registration never leaks into the egress pump fold.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.ConfluentKafka`
- Owns: the egress Kafka leg's instrumented-builder construction and its root registration rows
- Accept: producer twin at the `SinkBinding.Leg` composition; `AddKafkaProducerInstrumentation` rows at the AppHost root
- Reject: hand-rolled messaging-semconv spans over the plain builder; header-context injection code beside the instrumented producer
