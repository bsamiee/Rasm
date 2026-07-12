# [RASM_APPHOST_API_TESTING_SEAMS]

`Microsoft.Extensions.TimeProvider.Testing`, `Microsoft.Extensions.Diagnostics.Testing`, and `NodaTime.Testing` supply deterministic time, log capture, metric capture, and clock/time-zone fakes as tests-only seams over the runtime contracts.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.TimeProvider.Testing`
- package: `Microsoft.Extensions.TimeProvider.Testing`
- assembly: `Microsoft.Extensions.TimeProvider.Testing`
- namespace: `Microsoft.Extensions.Time.Testing`
- asset: test library
- rail: testing seams

[PACKAGE_SURFACE]: `Microsoft.Extensions.Diagnostics.Testing`
- package: `Microsoft.Extensions.Diagnostics.Testing`
- assembly: `Microsoft.Extensions.Diagnostics.Testing`
- namespace: `Microsoft.Extensions.Logging.Testing`
- namespace: `Microsoft.Extensions.Diagnostics.Metrics.Testing`
- namespace: `Microsoft.Extensions.DependencyInjection`
- asset: test library
- rail: testing seams

[PACKAGE_SURFACE]: `NodaTime.Testing`
- package: `NodaTime.Testing`
- assembly: `NodaTime.Testing`
- namespace: `NodaTime.Testing`
- namespace: `NodaTime.Testing.Extensions`
- namespace: `NodaTime.Testing.TimeZones`
- asset: test library
- rail: testing seams

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: deterministic time family
- rail: testing seams

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]      | [CAPABILITY]                              |
| :-----: | :----------------- | :------------------ | :---------------------------------------- |
|  [01]   | `FakeTimeProvider` | `TimeProvider` fake | manual and auto-advanced clock and timers |
|  [02]   | `FakeClock`        | `IClock` fake       | programmable NodaTime instant source      |

[PUBLIC_TYPE_SCOPE]: log capture family
- rail: testing seams

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]    | [CAPABILITY]                     |
| :-----: | :-------------------------------------- | :---------------- | :------------------------------- |
|  [01]   | `FakeLogger`                            | `ILogger` fake    | record capture and level control |
|  [02]   | `FakeLogCollector`                      | record sink       | snapshot and async record stream |
|  [03]   | `FakeLogRecord`                         | record value      | level, state, scopes, timestamp  |
|  [04]   | `FakeLogCollectorOptions`               | option value      | filters, sink, time provider     |
|  [05]   | `FakeLoggerProvider`                    | provider          | logger factory integration       |
|  [06]   | `FakeLoggerServiceCollectionExtensions` | service extension | fake logging registration        |
|  [07]   | `FakeLoggerBuilderExtensions`           | builder extension | logging builder registration     |

[PUBLIC_TYPE_SCOPE]: metric capture family
- rail: testing seams

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]    | [CAPABILITY]                         |
| :-----: | :------------------------ | :---------------- | :----------------------------------- |
|  [01]   | `MetricCollector<T>`      | instrument tap    | measurement capture per instrument   |
|  [02]   | `CollectedMeasurement<T>` | measurement value | value, tags, timestamp               |
|  [03]   | `MeasurementExtensions`   | query extension   | tag filtering and counter evaluation |

[PUBLIC_TYPE_SCOPE]: NodaTime fake zone family
- rail: testing seams

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]     | [CAPABILITY]                    |
| :-----: | :----------------------------- | :----------------- | :------------------------------ |
|  [01]   | `FakeDateTimeZoneSource`       | zone source fake   | builder-defined zone provider   |
|  [02]   | `SingleTransitionDateTimeZone` | zone fake          | one-transition zone             |
|  [03]   | `MultiTransitionDateTimeZone`  | zone fake          | scripted transition sequence    |
|  [04]   | `DurationConstruction`         | value construction | literal duration construction   |
|  [05]   | `LocalDateConstruction`        | value construction | literal local date construction |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: time control
- rail: testing seams

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                   | [CAPABILITY]                           |
| :-----: | :------------------ | :----------------------------- | :------------------------------------- |
|  [01]   | `SetUtcNow`         | `DateTimeOffset` value         | jumps the fake clock                   |
|  [02]   | `Advance`           | `TimeSpan` or `Duration` delta | moves time and fires due timers        |
|  [03]   | `AdjustTime`        | `DateTimeOffset` value         | shifts time without timer side effects |
|  [04]   | `AutoAdvanceAmount` | property on `FakeTimeProvider` | per-read automatic advancement         |
|  [05]   | `SetLocalTimeZone`  | `TimeZoneInfo` value           | controls local zone projection         |
|  [06]   | `FromUtc`           | static `FakeClock` factory     | constructs clock at a UTC instant      |
|  [07]   | `AdvanceSeconds`    | unit-suffixed advance family   | advances by ticks through days         |
|  [08]   | `Reset`             | `Instant` value                | rebases the fake clock                 |

[ENTRYPOINT_SCOPE]: log and metric capture
- rail: testing seams

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                            | [CAPABILITY]                      |
| :-----: | :---------------------------- | :-------------------------------------- | :-------------------------------- |
|  [01]   | `AddFakeLogging`              | service or logging-builder registration | installs fake logger pipeline     |
|  [02]   | `GetSnapshot`                 | optional clear flag                     | reads captured log records        |
|  [03]   | `GetLogsAsync`                | cancellation token                      | streams records asynchronously    |
|  [04]   | `LatestRecord`                | collector property                      | reads the newest record           |
|  [05]   | `ControlLevel`                | level plus enabled flag                 | toggles logger level response     |
|  [06]   | `GetMeasurementSnapshot`      | optional clear flag                     | reads captured measurements       |
|  [07]   | `WaitForMeasurementsAsync`    | min count plus token or timeout         | awaits measurement arrival        |
|  [08]   | `RecordObservableInstruments` | collector command                       | polls observable instruments      |
|  [09]   | `ContainsTags`                | tag filter over measurements            | subset tag filter on measurements |
|  [10]   | `MatchesTags`                 | tag filter over measurements            | exact tag match on measurements   |
|  [11]   | `EvaluateAsCounter<T>`        | measurement aggregation                 | folds deltas into a counter total |

## [04]-[IMPLEMENTATION_LAW]

[SEAM_TOPOLOGY]:
- restore scope: all three packages restore under the AppHost test closure only
- time seam: `FakeTimeProvider` substitutes the runtime `TimeProvider` contract; `FakeClock` substitutes NodaTime `IClock`
- log seam: `FakeLogger` implements `ILogger` and `IBufferedLogger`; the collector owns record retention and filtering
- metric seam: `MetricCollector<T>` taps a single instrument by instance, meter, or meter scope
- zone seam: fake zone sources and transition zones script `DateTimeZone` behavior

[LOCAL_ADMISSION]:
- Testing seams never enter production composition; production code depends on `TimeProvider` and `IClock` contracts.
- Deterministic spring and scheduling specs drive time exclusively through fake advancement.
- Log and metric assertions read captured snapshots, never sink output text.
- Zone-sensitive specs construct scripted zones instead of depending on tzdb contents.

[RAIL_LAW]:
- Packages: `Microsoft.Extensions.TimeProvider.Testing`, `Microsoft.Extensions.Diagnostics.Testing`, `NodaTime.Testing`
- Owns: deterministic test substitutes for time, logging, metrics, and zones
- Accept: contract-shaped fakes injected at test composition
- Reject: real-clock sleeps and sink-scraping assertions
