# [RASM_API_OPENTELEMETRY_EXTENSIONS]

`OpenTelemetry.Extensions` owns the span and log processors the SDK core omits: predicate-scoped `Baggage` promotion onto spans and log records, a per-second head cap on recorded traces, and log-record attachment as activity events.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `OpenTelemetry.Extensions`
- package: `OpenTelemetry.Extensions`
- assembly: `OpenTelemetry.Extensions`
- contract assembly: `OpenTelemetry.Api`
- namespace: `OpenTelemetry`, `OpenTelemetry.Trace`, `OpenTelemetry.Logs`, `Microsoft.Extensions.Logging`
- rail: telemetry composition

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: baggage promotion, head sampling, and log-to-event conversion

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :------------------------------------ | :------------ | :------------------------------------------------------ |
|  [01]   | `TracerProviderBuilderExtensions`     | static class  | span-side registration verbs                            |
|  [02]   | `OpenTelemetryLoggingExtensions`      | static class  | log-side registration verbs                             |
|  [03]   | `BaggageActivityProcessor`            | sealed class  | `BaseProcessor<Activity>` stamping baggage as span tags |
|  [04]   | `RateLimitingSampler`                 | class         | `Sampler` capping recorded traces per second            |
|  [05]   | `LogToActivityEventConversionOptions` | class         | delegate seats shaping the log-to-event projection      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: span-side registration and the seats it takes

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `AddBaggageActivityProcessor(Predicate<string>)`           | static   | seat baggage-to-span promotion                      |
|  [02]   | `AddAutoFlushActivityProcessor(Func<Activity, bool>, int)` | static   | flush the provider on a matched activity            |
|  [03]   | `BaggageActivityProcessor.AllowAllBaggageKeys`             | property | promote-everything key predicate                    |
|  [04]   | `RateLimitingSampler(int)`                                 | ctor     | mint the per-second head cap                        |
|  [05]   | `RateLimitingSampler.ShouldSample(in SamplingParameters)`  | instance | verdict carrying `sampler.type` and `sampler.param` |

- `AddAutoFlushActivityProcessor`: registers after every exporter-bound processor.
- Predicate seats read a throw as false: the baggage key drops and the flush is skipped.

[ENTRYPOINT_SCOPE]: log-side registration and the conversion seats it configures

| [INDEX] | [SURFACE]                                                                                        | [SHAPE]  | [CAPABILITY]              |
| :-----: | :----------------------------------------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `AttachLogsToActivityEvent(Action<LogToActivityEventConversionOptions>)`                         | static   | attach records as events  |
|  [02]   | `AddBaggageProcessor(Predicate<string>)`                                                         | static   | copy baggage onto records |
|  [03]   | `StateConverter -> Action<ActivityTagsCollection, IReadOnlyList<KeyValuePair<string, object?>>>` | property | record state onto tags    |
|  [04]   | `ScopeConverter -> Action<ActivityTagsCollection, int, LogRecordScope>`                          | property | each scope onto tags      |
|  [05]   | `Filter -> Func<LogRecord, bool>?`                                                               | property | admit a record            |

- `AddBaggageProcessor`: overloads ride `OpenTelemetryLoggerOptions` and `LoggerProviderBuilder`; each predicate-free overload copies every entry.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Promotion is opt-in per key: `AllowAllBaggageKeys` admits every entry, a narrower `Predicate<string>` admits the propagated tenant and correlation keys alone, and an unmatched key reaches neither span nor log record.
- `RateLimitingSampler` caps head volume before any processor runs, and a `ParentBasedSampler` root carries its verdict to every child span.

[STACKING]:
- `OpenTelemetry`(`api-opentelemetry.md`): supplies `BaseProcessor<Activity>`, `Sampler`, `SamplingParameters`, and the `TracerProviderBuilder`/`LoggerProviderBuilder` these verbs extend; `OpenTelemetry.Api` carries the `Baggage` both processors read.
- `OpenTelemetry.Extensions.Hosting`(`api-opentelemetry-hosting.md`): the `WithTracing`/`WithLogging` delegates receive the builders these verbs register against.
- `SignalGovernance.PromotedBaggage`: `AddBaggageActivityProcessor` chains it inside `WithTracing` ahead of the batch export processor, `AddBaggageProcessor` seats it on the log leg, `SetSampler` nests `RateLimitingSampler` under a `ParentBasedSampler` root, and `AttachLogsToActivityEvent` shapes the projection through the three conversion seats.

[LOCAL_ADMISSION]:
- One `Predicate<string>` serves both promotion legs, so span tags and log attributes never carry divergent allowlists.
- `AttachLogsToActivityEvent` projects each admitted record onto the ambient span, so its `Filter` seat narrows attachment to the records a trace view reads.

[RAIL_LAW]:
- Package: `OpenTelemetry.Extensions`
- Owns: predicate-scoped baggage promotion onto spans and log records, rate-limiting head sampling, and log-to-activity-event attachment
- Accept: promotion, head sampling, and attachment seated through the registration verbs on the provider builders
- Reject: a hand-rolled `BaseProcessor<Activity>` re-reading `Baggage.Current` at span start; per-call-site tag writes standing in for promotion
