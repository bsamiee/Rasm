# [RASM_API_OTEL_INSTRUMENTATION_STACKEXCHANGEREDIS]

`OpenTelemetry.Instrumentation.StackExchangeRedis` is the db-semconv span owner for the Redis wire: it hooks StackExchange.Redis's command profiler on a registered `IConnectionMultiplexer` and converts each `IProfiledCommand` into a `Client`-kind span named `OpenTelemetry.Instrumentation.StackExchangeRedis.Execute`. Its single `ActivitySource` shares the package name, admitted at the root like every foreign source. This is a trace-only library — no `Meter` ships, so cache-hit ratios stay a downstream span-aggregation concern, never an instrument roster this package carries.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.StackExchangeRedis`
- package: `OpenTelemetry.Instrumentation.StackExchangeRedis`
- assembly: `OpenTelemetry.Instrumentation.StackExchangeRedis`
- namespace: `OpenTelemetry.Trace`, `OpenTelemetry.Instrumentation.StackExchangeRedis`
- driver package: `StackExchange.Redis` — supplies the `IConnectionMultiplexer` profiler hook and the `IProfiledCommand` entry the converter reads
- asset: runtime library
- rail: storage instrumentation

## [02]-[PUBLIC_TYPES]

[INSTRUMENTATION_TYPES]: instrumentation handle, options, and enrichment carrier
- rail: storage instrumentation

| [INDEX] | [SYMBOL]                                   | [KIND]                      | [CAPABILITY]                                    |
| :-----: | :----------------------------------------- | :-------------------------- | :---------------------------------------------- |
|  [01]   | `StackExchangeRedisInstrumentation`        | sealed `IDisposable` handle | connection registry; add/remove any time        |
|  [02]   | `StackExchangeRedisInstrumentationOptions` | options carrier             | flush, verbosity, filter, enrich, timing knobs  |
|  [03]   | `RedisInstrumentationContext`              | readonly record struct      | `ParentActivity` + `ProfiledCommand` for enrich |

`StackExchangeRedisInstrumentation` carries `IDisposable AddConnection(IConnectionMultiplexer)` and its keyed twin `AddConnection(string name, IConnectionMultiplexer)`; each returns the registration handle whose disposal unhooks the connection, so a multiplexer joins or leaves the instrumented set at runtime.

`StackExchangeRedisInstrumentationOptions` exposes `TimeSpan FlushInterval` (10 s default — the profiled-command conversion cadence), `bool SetVerboseDatabaseStatements` (captures the key/script text into `db.statement`), `Func<RedisInstrumentationContext, bool>? Filter`, `Action<Activity, RedisInstrumentationContext>? Enrich`, and `bool EnrichActivityWithTimingEvents` (true default — queue/network span events off the profiled timings).

`RedisInstrumentationContext` is `readonly record struct RedisInstrumentationContext(Activity? ParentActivity, IProfiledCommand ProfiledCommand)` — the filter reads it to drop a command before the span mints, the enrich callback reads its `ProfiledCommand` to tag the live span.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: admission and connection binding (`TracerProviderBuilderExtensions`)
- rail: storage instrumentation

| [INDEX] | [SURFACE]                       | [KIND]          | [CAPABILITY]                                                                   |
| :-----: | :------------------------------ | :-------------- | :----------------------------------------------------------------------------- |
|  [01]   | `AddRedisInstrumentation`       | trace admission | source subscribe + optional connection/options/service-key bind                |
|  [02]   | `ConfigureRedisInstrumentation` | handle exposure | hands the `StackExchangeRedisInstrumentation` back for runtime `AddConnection` |

`AddRedisInstrumentation(TracerProviderBuilder)` subscribes the source alone; overloads bind an `IConnectionMultiplexer`, an `Action<StackExchangeRedisInstrumentationOptions>`, both together, or a DI `serviceKey`/`name` selector — `(TracerProviderBuilder, string? name, IConnectionMultiplexer?, object? serviceKey, Action<StackExchangeRedisInstrumentationOptions>?)` is the full-control overload. A bare admission with no connection resolves the multiplexer from the application `IServiceProvider`.

`ConfigureRedisInstrumentation(TracerProviderBuilder, Action<StackExchangeRedisInstrumentation>)` and its `Action<IServiceProvider, StackExchangeRedisInstrumentation>` twin capture the instrumentation handle for `AddConnection` control beyond the builder — the multi-connection and late-binding path.

## [04]-[IMPLEMENTATION_LAW]

[REDIS_TOPOLOGY]:
- subscription root: `AddRedisInstrumentation` registers at the AppHost composition root; the `StackExchange.Redis` dependency stays with the connection owner, and the root reaches only the `OpenTelemetry.Instrumentation.StackExchangeRedis` source name
- connection binding: instrumentation hooks the profiler on a specific `IConnectionMultiplexer` — only calls on that instance emit spans; a second unregistered multiplexer is dark until `AddConnection` binds it
- span shape: `Client`-kind, name `OpenTelemetry.Instrumentation.StackExchangeRedis.Execute`, tags `db.system.name`/`db.operation.name`/`db.namespace`, backdated to the profiled `CommandCreated`

[STACKING]:
- `StackExchange.Redis`: instrumentation subscribes the client's own profiling session factory — connection configuration stays driver-owned rows, and the hook adds no client vocabulary.
- `OpenTelemetry`(`api-opentelemetry.md`): `AddRedisInstrumentation` is the source subscription with a profiler hook; `AddSource("OpenTelemetry.Instrumentation.StackExchangeRedis")` alone subscribes the source but never installs the hook, so the convenience verb is the only complete path — never a bare `AddSource` shim.
- trace-only: no `Meter` ships, so cache-op counts and hit ratios derive from span aggregation at the backend, disjoint from the driver-native instrument lanes the relational rows carry.

[LOCAL_ADMISSION]:
- Composition-root-only, at the AppHost root that owns Redis clients; `ConfigureRedisInstrumentation` carries the handle where connections bind after build.
- `Filter` or `Enrich` forces every profiled command to buffer until the parent activity completes — a long-lived or high-volume parent thus pins memory, so both stay unset on hot high-fanout paths unless the drop/enrich earns the cost.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.StackExchangeRedis`
- Owns: Redis command spans and the per-connection profiler hook at the composition root
- Accept: `AddRedisInstrumentation` admission with connection binding; `ConfigureRedisInstrumentation` for runtime `AddConnection`
- Reject: hand-rolled db-semconv spans over raw `IProfiledCommand`; a bare `AddSource` that subscribes the name without the profiler hook
