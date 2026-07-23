# [RASM_API_OTEL_INSTRUMENTATION_STACKEXCHANGEREDIS]

`OpenTelemetry.Instrumentation.StackExchangeRedis` mints the Redis wire's db-semconv span: it registers a profiling-session factory on each bound `IConnectionMultiplexer` and folds every drained `IProfiledCommand` into a `Client`-kind span named for the Redis verb, backdated to the profiled command's creation. Its `ActivitySource` name matches the package on every semantic-convention arm, and no `Meter` ships — cache-op counts and hit ratios stay backend span aggregation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Instrumentation.StackExchangeRedis`
- package: `OpenTelemetry.Instrumentation.StackExchangeRedis` (Apache-2.0)
- assembly: `OpenTelemetry.Instrumentation.StackExchangeRedis`
- namespace: `OpenTelemetry.Trace`, `OpenTelemetry.Instrumentation.StackExchangeRedis`
- depends: `StackExchange.Redis` — the `RegisterProfiler` hook every bound multiplexer takes
- rail: storage instrumentation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentation handle, its per-name options carrier, and the callback payload

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :----------------------------------------- | :------------ | :----------------------------- |
|  [01]   | `StackExchangeRedisInstrumentation`        | class         | disposable connection registry |
|  [02]   | `StackExchangeRedisInstrumentationOptions` | class         | per-name span policy           |
|  [03]   | `RedisInstrumentationContext`              | struct        | filter and enrich payload      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder admission, connection binding, and the options slots each registration name carries; every shorthand folds to `AddRedisInstrumentation(string?, IConnectionMultiplexer?, object?, Action<StackExchangeRedisInstrumentationOptions>?)`, which resolves an absent connection from the application `IServiceProvider` and keys that lookup on `serviceKey`

| [INDEX] | [SURFACE]                                                                                           | [SHAPE]  | [CAPABILITY]           |
| :-----: | :-------------------------------------------------------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `AddRedisInstrumentation()`                                                                         | static   | resolved from DI       |
|  [02]   | `AddRedisInstrumentation(IConnectionMultiplexer)`                                                   | static   | one held multiplexer   |
|  [03]   | `AddRedisInstrumentation(object)`                                                                   | static   | keyed DI multiplexer   |
|  [04]   | `AddRedisInstrumentation(Action<StackExchangeRedisInstrumentationOptions>)`                         | static   | options, default name  |
|  [05]   | `AddRedisInstrumentation(IConnectionMultiplexer, Action<StackExchangeRedisInstrumentationOptions>)` | static   | held with own options  |
|  [06]   | `AddRedisInstrumentation(string?, object, Action<StackExchangeRedisInstrumentationOptions>)`        | static   | named options over key |
|  [07]   | `ConfigureRedisInstrumentation(Action<StackExchangeRedisInstrumentation>)`                          | static   | handle for late bind   |
|  [08]   | `ConfigureRedisInstrumentation(Action<IServiceProvider, StackExchangeRedisInstrumentation>)`        | static   | same handle, provider  |
|  [09]   | `StackExchangeRedisInstrumentation.AddConnection(IConnectionMultiplexer)`                           | instance | bind, default name     |
|  [10]   | `StackExchangeRedisInstrumentation.AddConnection(string, IConnectionMultiplexer)`                   | instance | bind under named slot  |
|  [11]   | `StackExchangeRedisInstrumentationOptions.FlushInterval`                                            | property | profiled-drain cadence |
|  [12]   | `StackExchangeRedisInstrumentationOptions.SetVerboseDatabaseStatements`                             | property | verbose command text   |
|  [13]   | `StackExchangeRedisInstrumentationOptions.Filter`                                                   | property | drop before the span   |
|  [14]   | `StackExchangeRedisInstrumentationOptions.Enrich`                                                   | property | tag the live span      |
|  [15]   | `StackExchangeRedisInstrumentationOptions.EnrichActivityWithTimingEvents`                           | property | queue and wire events  |

- `StackExchangeRedisInstrumentationOptions`: `FlushInterval` defaults to ten seconds and `EnrichActivityWithTimingEvents` to on; every other slot arrives unset.
- `StackExchangeRedisInstrumentation.AddConnection`: disposing the returned registration unhooks that one connection, and disposing the instrumentation handle unhooks every bound multiplexer.
- `ConfigureRedisInstrumentation`: a builder outside `IDeferredTracerProviderBuilder` throws `NotSupportedException`.
- `StackExchangeRedisInstrumentationOptions.Filter`: a `false` return and a thrown exception both drop the command.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- profiler seat: each bound connection takes `RegisterProfiler` with a session factory keyed on `Activity.Current`'s trace and span ids; a null or non-W3C current activity routes its commands to that connection's default session, and those spans mint parentless.
- drain loop: one background thread per bound connection wakes every `FlushInterval` and converts each session's finished commands; a cached entry survives until its parent activity's duration goes nonzero, and disposal drains once more before unhooking.
- span shape: `Client` kind named after `IProfiledCommand.Command`, falling back to the source name suffixed `.Execute` for an empty verb, started at `CommandCreated` and ended at `CommandCreated + ElapsedTime`.
- semconv posture: `OTEL_SEMCONV_STABILITY_OPT_IN` selects both the tag family and the `ActivitySource` instance stamping its schema URL — unset emits `db.system`, `db.redis.database_index`, and `db.statement`; `database` emits `db.system.name`, `db.operation.name`, `db.namespace`, and `db.query.text`; `database/dup` emits both families off a source stamping no schema URL.
- statement shaping: `SetVerboseDatabaseStatements` reflects the driver's private command-and-key text and a script-eval body into `db.statement` on the old arm and `db.query.text` on the new; unset, both carry the bare verb.
- endpoint tags: every arm tags `server.address` and `server.port` off `IProfiledCommand.EndPoint`, adding the `network.peer.address` and `network.peer.port` pair for an IP endpoint and `network.peer.address` alone for a Unix socket.
- timing events: `EnrichActivityWithTimingEvents` appends `Enqueued`, `Sent`, and `ResponseReceived` events at the profiled creation, enqueue, and send offsets.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): the admission verb calls `AddSource` on the package name and registers the handle through `AddInstrumentation<T>`, so provider disposal unhooks every bound connection after a final drain; that source name alone subscribes without installing the profiler.
- `System.Diagnostics`(`api-diagnostics-activity.md`): tagging, timing events, and `Enrich` all run inside `Activity.IsAllDataRequested`, so a sampler-dropped command costs no tag build, and each timing entry is an ordinary `ActivityEvent` row.
- `StackExchange.Redis`(`Rasm.Persistence/.api/api-redis.md`): `RegisterProfiler` is the driver's own hook, so endpoint, retry, and protocol policy stay driver-owned rows, and a `LuaScript` body reaches the span only under verbose statements.
- `Microsoft.Extensions.Caching.Hybrid`(`api-hybrid-cache.md`): its L2 `IDistributedCache` reaches Redis through `RedisCacheOptions.ConnectionMultiplexerFactory`, so cache-tier hops emit spans exactly when that multiplexer instance is the one bound here.
- Within-lib: `IOptionsMonitor` scoping makes the registration name the policy key, so one `ConfigureRedisInstrumentation` handle binds cache, backplane, and egress multiplexers under distinct cadence, verbosity, and `Filter` rows on a single subscription.
- `Rasm.Persistence`: `Query/cache#L2_CONTRIBUTION` `CacheBackplane(IConnectionMultiplexer, …)` holds the L2 multiplexer `AddRedisInstrumentation(connection)` binds at the AppHost root, `Version/egress#EGRESS_SINK` `EgressSink.RedisStream` may own a second multiplexer `ConfigureRedisInstrumentation` with `AddConnection` folds under the same subscription, and span depth — `FlushInterval`, `SetVerboseDatabaseStatements`, `Filter`/`Enrich` — sets on the `Store/observability` composition row, never inside a cache-op or drain body; the package publishes no `Meter`, so the cache hit-ratio gauge stays the `Store/observability#STORE_INSTRUMENTS` span-derived concern, never a Redis-meter subscription row.

[LOCAL_ADMISSION]:
- Registration binds at the AppHost root owning the Redis clients; that root reaches the source name alone, never the `StackExchange.Redis` assembly Persistence carries.
- Setting `Filter` or `Enrich` holds every profiled command until its parent activity completes, so both stay unset on high-fanout cache paths unless the drop or tag earns the retained memory.
- Every held multiplexer joins through `AddConnection`, or its hops leave the trace.

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.StackExchangeRedis`
- Owns: Redis command spans and the per-connection profiler hook at the composition root
- Accept: `AddRedisInstrumentation` with its held or DI-keyed connection; `ConfigureRedisInstrumentation` for late `AddConnection` across several multiplexers
- Reject: hand-rolled db-semconv spans over raw `IProfiledCommand`
