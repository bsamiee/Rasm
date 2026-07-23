# [RASM_APPHOST_API_TESTING_SEAMS]

Test-only substitution surfaces fold the AppHost runtime's time, clock, logging, and metric contracts into deterministic fakes injected at the test composition seam, so a spec holds determinism without wall-clock timing or sink scraping.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.TimeProvider.Testing`
- package: `Microsoft.Extensions.TimeProvider.Testing` (MIT)
- assembly: `Microsoft.Extensions.TimeProvider.Testing`
- namespace: `Microsoft.Extensions.Time.Testing`
- rail: testing seams

[PACKAGE_SURFACE]: `Microsoft.Extensions.Diagnostics.Testing`
- package: `Microsoft.Extensions.Diagnostics.Testing` (MIT)
- assembly: `Microsoft.Extensions.Diagnostics.Testing`
- namespace: `Microsoft.Extensions.Logging.Testing`, `Microsoft.Extensions.Diagnostics.Metrics.Testing`, `Microsoft.Extensions.Logging`, `Microsoft.Extensions.DependencyInjection`
- rail: testing seams

[PACKAGE_SURFACE]: `NodaTime.Testing`
- package: `NodaTime.Testing` (Apache-2.0)
- assembly: `NodaTime.Testing`
- namespace: `NodaTime.Testing`, `NodaTime.Testing.Extensions`, `NodaTime.Testing.TimeZones`
- rail: testing seams

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: deterministic time family

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :----------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `FakeTimeProvider` | class         | `TimeProvider` fake — manual/auto-advanced clock, timers |
|  [02]   | `FakeClock`        | class         | `IClock` fake — programmable NodaTime instant source     |

[PUBLIC_TYPE_SCOPE]: log capture family

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :-------------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `FakeLogger`                            | class         | `ILogger`/`IBufferedLogger` fake; level control |
|  [02]   | `FakeLogCollector`                      | class         | record sink; snapshot and async record stream   |
|  [03]   | `FakeLogRecord`                         | class         | one record — level, state, scopes, timestamp    |
|  [04]   | `FakeLogCollectorOptions`               | class         | filters, sink, time provider                    |
|  [05]   | `FakeLoggerProvider`                    | class         | `ILoggerProvider` fake; logger factory          |
|  [06]   | `FakeLoggerServiceCollectionExtensions` | class         | registration on `IServiceCollection`            |
|  [07]   | `FakeLoggerBuilderExtensions`           | class         | registration on `ILoggingBuilder`               |

[PUBLIC_TYPE_SCOPE]: metric capture family

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :------------------------ | :------------ | :--------------------------------------- |
|  [01]   | `MetricCollector<T>`      | class         | single-instrument measurement tap        |
|  [02]   | `CollectedMeasurement<T>` | class         | one measurement — value, tags, timestamp |
|  [03]   | `MeasurementExtensions`   | class         | tag filtering and counter evaluation     |

[PUBLIC_TYPE_SCOPE]: NodaTime fake zone family

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :----------------------------- | :------------ | :------------------------------ |
|  [01]   | `FakeDateTimeZoneSource`       | class         | builder-defined zone provider   |
|  [02]   | `SingleTransitionDateTimeZone` | class         | one-transition zone             |
|  [03]   | `MultiTransitionDateTimeZone`  | class         | scripted transition sequence    |
|  [04]   | `DurationConstruction`         | class         | literal duration construction   |
|  [05]   | `LocalDateConstruction`        | class         | literal local-date construction |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: time control

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `FakeTimeProvider.SetUtcNow(DateTimeOffset)`      | instance | jumps the fake clock                  |
|  [02]   | `FakeTimeProvider.Advance(TimeSpan)`              | instance | moves time and fires due timers       |
|  [03]   | `FakeTimeProvider.AdjustTime(DateTimeOffset)`     | instance | shifts time without firing timers     |
|  [04]   | `FakeTimeProvider.AutoAdvanceAmount`              | property | per-read automatic advancement        |
|  [05]   | `FakeTimeProvider.SetLocalTimeZone(TimeZoneInfo)` | instance | controls local-zone projection        |
|  [06]   | `FakeClock.FromUtc(int, ...)`                     | static   | constructs the clock at a UTC instant |
|  [07]   | `FakeClock.Advance(Duration)`                     | instance | duration or unit-suffixed advance     |
|  [08]   | `FakeClock.Reset(Instant)`                        | instance | rebases the fake clock                |

[ENTRYPOINT_SCOPE]: log and metric capture

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :-------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `AddFakeLogging(IServiceCollection\|ILoggingBuilder)`                 | static   | installs the fake logger pipeline |
|  [02]   | `FakeLogCollector.GetSnapshot(bool)`                                  | instance | reads captured log records        |
|  [03]   | `FakeLogCollector.GetLogsAsync(CancellationToken)`                    | instance | streams records asynchronously    |
|  [04]   | `FakeLogCollector.LatestRecord`                                       | property | the newest record                 |
|  [05]   | `FakeLogger.ControlLevel(LogLevel, bool)`                             | instance | toggles logger level response     |
|  [06]   | `MetricCollector<T>.GetMeasurementSnapshot(bool)`                     | instance | reads captured measurements       |
|  [07]   | `MetricCollector<T>.WaitForMeasurementsAsync(int, CancellationToken)` | instance | awaits measurement arrival        |
|  [08]   | `MetricCollector<T>.RecordObservableInstruments()`                    | instance | polls observable instruments      |
|  [09]   | `MeasurementExtensions.ContainsTags<T>(...)`                          | static   | subset tag filter                 |
|  [10]   | `MeasurementExtensions.MatchesTags<T>(...)`                           | static   | exact tag match                   |
|  [11]   | `MeasurementExtensions.EvaluateAsCounter<T>()`                        | fold     | folds deltas into a counter total |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each fake substitutes one runtime contract at the injection seam, so a spec composes the fake where production composes the contract and reads captured state in place of side effects.

[STACKING]:
- `TimeProvider` (BCL runtime contract): `FakeTimeProvider` derives it and overrides `GetUtcNow`/`GetTimestamp`/`CreateTimer`, so any port taking `TimeProvider` binds the fake and its `ITimer` scheduling under test.
- `NodaTime`(`.api/api-nodatime.md`): `FakeClock` implements `IClock.GetCurrentInstant()`, substituting `SystemClock.Instance`; `FromUtc`/`Advance`/`Reset` drive the `Instant` a `ZonedClock` reads.
- `System.Diagnostics.Metrics`(`.api/api-diagnostics-metrics.md`): `MetricCollector<T>` binds an `Instrument<T>`, `ObservableInstrument<T>`, or `Meter`+name and captures its `Measurement<T>` writes as `CollectedMeasurement<T>`.
- `Microsoft.Extensions.Logging`(`.api/api-logging.md`): `FakeLogger` implements `ILogger`/`IBufferedLogger`, folding every `ILogger.Log<TState>` emission into `FakeLogCollector` records.

[LOCAL_ADMISSION]:
- Testing seams restore and compose only inside the AppHost test closure; production depends on the `TimeProvider` and `IClock` contracts.
- Time-driven specs drive advancement through a fake exclusively.
- Log and metric assertions read captured snapshots, never sink output text.
- Zone-sensitive specs resolve against scripted zones, never tzdb contents.

[RAIL_LAW]:
- Packages: `Microsoft.Extensions.TimeProvider.Testing`, `Microsoft.Extensions.Diagnostics.Testing`, `NodaTime.Testing`
- Owns: deterministic test substitutes for time, logging, metrics, and zones
- Accept: contract-shaped fakes injected at test composition
- Reject: real-clock sleeps and sink-scraping assertions
