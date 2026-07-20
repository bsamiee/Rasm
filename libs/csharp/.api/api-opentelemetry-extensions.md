# [RASM_API_OPENTELEMETRY_EXTENSIONS]

`OpenTelemetry.Extensions` carries the contrib processors and sampler that the SDK core omits: `BaggageActivityProcessor` promotes selected `Baggage` entries onto every span as tags at start, the canonical owner of tenant/cost baggage-to-span promotion the wire law names; `RateLimitingSampler` caps recorded traces per second as a head sampler; the logging bridge attaches log records onto the ambient activity as events and copies baggage onto log records. Provider builders alone reference this assembly; a library never does, and each processor's lifetime is the provider that admits it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Extensions`
- package: `OpenTelemetry.Extensions`
- assembly: `OpenTelemetry.Extensions`
- contract assembly: `OpenTelemetry.Api` — `Baggage`, the propagation surface the baggage processors read
- namespace: `OpenTelemetry.Trace`, `OpenTelemetry`, `OpenTelemetry.Logs`, `Microsoft.Extensions.Logging`
- asset: runtime library
- rail: telemetry composition

## [02]-[PUBLIC_TYPES]

[PROCESSOR_TYPES]: span-side baggage promotion and flush
- rail: telemetry composition

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]     | [CAPABILITY]                                                           |
| :-----: | :-------------------------------- | :----------------- | :--------------------------------------------------------------------- |
|  [01]   | `TracerProviderBuilderExtensions` | trace registration | `AddBaggageActivityProcessor` / `AddAutoFlushActivityProcessor` verbs  |
|  [02]   | `BaggageActivityProcessor`        | span processor     | copies predicate-selected baggage onto each span at `OnStart`          |
|  [03]   | `RateLimitingSampler`             | head sampler       | per-second recorded-trace cap emitting a rate-limited `SamplingResult` |

`BaggageActivityProcessor` is a sealed `BaseProcessor<Activity>` exposing the static `AllowAllBaggageKeys` `Predicate<string>` seat and overriding `OnStart(Activity)` — construction rides `AddBaggageActivityProcessor`, never a direct new. `RateLimitingSampler` derives `Sampler`, constructs from `RateLimitingSampler(int maxTracesPerSecond)`, and overrides `ShouldSample(in SamplingParameters)`. `AddAutoFlushActivityProcessor` backs an internal auto-flush processor; that registration verb is its only surface.

[LOG_CORRELATION_TYPES]: log-to-span attachment and log-side baggage
- rail: telemetry composition

| [INDEX] | [SYMBOL]                              | [PACKAGE_ROLE]   | [CAPABILITY]                                                           |
| :-----: | :------------------------------------ | :--------------- | :--------------------------------------------------------------------- |
|  [01]   | `OpenTelemetryLoggingExtensions`      | log registration | `AttachLogsToActivityEvent` / `AddBaggageProcessor` verbs              |
|  [02]   | `LogToActivityEventConversionOptions` | conversion knobs | state, scope, and filter delegates shaping the log-to-event projection |

`LogToActivityEventConversionOptions` carries the mutable `StateConverter` (`Action<ActivityTagsCollection, IReadOnlyList<KeyValuePair<string, object?>>>`), `ScopeConverter` (`Action<ActivityTagsCollection, int, LogRecordScope>`), and optional `Filter` (`Func<LogRecord, bool>?`) that govern which log records attach and how their state projects onto the activity event.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: builder registration verbs
- rail: telemetry composition

| [INDEX] | [SURFACE]                       | [KIND]           | [CAPABILITY]                                                                     |
| :-----: | :------------------------------ | :--------------- | :------------------------------------------------------------------------------- |
|  [01]   | `AddBaggageActivityProcessor`   | trace processor  | `(TracerProviderBuilder, Predicate<string>)` seats baggage-to-span promotion     |
|  [02]   | `AddAutoFlushActivityProcessor` | trace processor  | `(TracerProviderBuilder, Func<Activity, bool>, int timeoutMilliseconds = 10000)` |
|  [03]   | `AddBaggageProcessor`           | log processor    | `OpenTelemetryLoggerOptions` and `LoggerProviderBuilder` overloads, key-filtered |
|  [04]   | `AttachLogsToActivityEvent`     | log-event bridge | `(OpenTelemetryLoggerOptions, Action<LogToActivityEventConversionOptions>?)`     |

`AllowAllBaggageKeys` is the promote-everything predicate; a narrower `Predicate<string>` scopes promotion to the tenant and cost keys the wire law propagates, keeping unrelated baggage off spans.

## [04]-[IMPLEMENTATION_LAW]

[PROMOTION_TOPOLOGY]:
- span: `AddBaggageActivityProcessor` runs inside the `WithTracing` delegate; the processor reads ambient `Baggage` at each span start and stamps predicate-selected entries as span tags — the tenant/cost attribution path off one propagated context
- predicate: promotion is opt-in per key; `AllowAllBaggageKeys` promotes all, a custom `Predicate<string>` narrows to the propagated tenant and cost keys, and an unmatched key never reaches a span
- sampler: `RateLimitingSampler` seats as the `TracerProviderBuilder` sampler root or a `ParentBasedSampler` delegate, capping head volume before processors run

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): supplies `BaseProcessor<Activity>`, `Sampler`, `SamplingParameters`, and the `TracerProviderBuilder`/`LoggerProviderBuilder` these verbs extend; the contract assembly `OpenTelemetry.Api` supplies the `Baggage` the processors read.
- `OpenTelemetry.Extensions.Hosting`(`api-opentelemetry-hosting.md`): the `WithTracing`/`WithLogging` delegates receive the builders these verbs register against.

[LOCAL_ADMISSION]:
- Baggage promotion is composition-root-only; no library references this assembly, and the key predicate names the tenant and cost keys explicitly rather than promoting all.
- `RateLimitingSampler` is a head cap, not a delivery guarantee — pair it with the parent-based composite when child spans must follow the root verdict.

[RAIL_LAW]:
- Package: `OpenTelemetry.Extensions`
- Owns: baggage-to-span and baggage-to-log promotion, rate-limiting head sampling, and log-to-activity-event attachment
- Accept: predicate-scoped baggage promotion and a per-second trace cap seated at the provider builders
- Reject: direct processor construction bypassing the registration verbs; unfiltered promotion leaking non-tenant baggage onto spans
