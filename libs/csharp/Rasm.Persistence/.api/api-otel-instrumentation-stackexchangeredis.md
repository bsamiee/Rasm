# [RASM_PERSISTENCE_API_OTEL_INSTRUMENTATION_STACKEXCHANGEREDIS]

`OpenTelemetry.Instrumentation.StackExchangeRedis` hooks the held cache-backplane multiplexer's command profiler so Redis command spans join the one trace the Npgsql and Kafka legs already carry. Substrate canonical members live at `libs/csharp/.api/api-otel-instrumentation-stackexchangeredis.md`; this overlay carries only the Persistence delta — the connection-binding seam, the multi-connection posture across cache and egress, and the tracer-only (no-meter) ruling that parts it from the two-verb Npgsql row.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-otel-instrumentation-stackexchangeredis.md`
- instrumentation-handle/options/context type roster, the `AddRedisInstrumentation`/`ConfigureRedisInstrumentation` admission table, span shape, and package/asset facts live on the substrate catalog — this overlay never re-states them
- rail: storage instrumentation

## [02]-[PERSISTENCE_BINDINGS]

- `Query/cache#L2_CONTRIBUTION` `CacheBackplane(IConnectionMultiplexer connection, …)` holds the L2 multiplexer; `AddRedisInstrumentation(connection)` on the tracer builder at the AppHost root binds it, the one instrumented-connection seam for the cache leg.
- `Version/egress#EGRESS_SINK` `EgressSink.RedisStream` may own a second multiplexer distinct from the cache backplane; `ConfigureRedisInstrumentation` captures the `StackExchangeRedisInstrumentation` handle so `AddConnection` binds both the cache and egress multiplexers under one subscription instead of two builder admissions.
- Span depth binds at the options seam — `FlushInterval` conversion cadence, `SetVerboseDatabaseStatements` for key/script text, `Filter`/`Enrich` over `RedisInstrumentationContext` — set on the `Store/observability` composition row, never inside a cache-op or drain body.

## [03]-[IMPLEMENTATION_LAW]

[TRACER_ONLY]:
- one verb, tracer only — the package publishes no `Meter`, so the Npgsql two-altitude row (tracer `AddNpgsql` + meter `AddNpgsqlInstrumentation`) has no meter twin here; the cache hit-ratio gauge stays the `Store/observability#STORE_INSTRUMENTS` span-derived concern, never a Redis-meter subscription row
- only calls on a bound multiplexer emit spans; an unregistered connection is dark until `AddConnection` binds it, so every held Redis multiplexer joins the instrumentation or its hops leave the trace

[LOCAL_ADMISSION]:
- Composition-root-only at the AppHost root that owns the Redis clients; `Filter`/`Enrich` pins every profiled command in memory until the parent activity completes, so both stay unset on the hot high-fanout cache path unless the drop or tag earns the buffering cost.
- Span and meter-absence are package facts, never Persistence vocabulary; the profiler hook never leaks into the `CacheBackplane` or `EgressSink` bodies.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.StackExchangeRedis`
- Owns: the Persistence tracer registration row and the multi-connection binding across the cache and egress multiplexers
- Accept: composition-root `AddRedisInstrumentation(connection)`; `ConfigureRedisInstrumentation` + `AddConnection` for the second multiplexer
- Reject: a meter subscription row where the package ships no meter; hand-rolled db-semconv spans over the profiled cache commands; profiler-hook code inside the backplane or drain fold
