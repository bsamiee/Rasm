# [CSHARP_TESTING_API_DIAGNOSTICS_TESTING]

`Microsoft.Extensions.Diagnostics.Testing` ships the R9 telemetry doubles: `FakeLogger`/`FakeLogCollector` capture every structured log record as a typed `FakeLogRecord`, and `MetricCollector<T>` captures every measurement an `Instrument<T>` emits with its tags and timestamp. A telemetry obligation — a failure-path log a rail must write, a counter an operation must bump — asserts as one snapshot lookup instead of a provider mock, and both doubles take `TimeProvider`, so captured timestamps ride the same `FakeTimeProvider` clock as the rest of the spec (`timeprovider-testing.md`).

## [01]-[PACKAGE_SURFACE]

- package: `Microsoft.Extensions.Diagnostics.Testing` `10.8.0`
- license: `MIT`
- namespaces: `Microsoft.Extensions.Logging.Testing`, `Microsoft.Extensions.Diagnostics.Metrics.Testing`
- asset: `lib/net10.0/Microsoft.Extensions.Diagnostics.Testing.dll`
- rail: evidence — captured log records and metric measurements as typed snapshots; a suite-owned harness row (`PrivateAssets="all"`), never centrally injected

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                 | [KIND]    | [CAPABILITY]                                                           |
| :-----: | :--------------------------------------- | :-------- | :--------------------------------------------------------------------- |
|  [01]   | `FakeLogger` / `FakeLogger<T>`           | double    | `ILogger` writing into its `Collector`; `ControlLevel` arms IsEnabled  |
|  [02]   | `FakeLogCollector`                       | evidence  | `GetSnapshot(clear)`, `LatestRecord`, `Count`, `Clear`, `GetLogsAsync` |
|  [03]   | `FakeLogCollectorOptions`                | policy    | level/category filters, disabled-level capture, `TimeProvider`, sink   |
|  [04]   | `FakeLogRecord`                          | record    | `Level`, `Message`, `StructuredState`, `Exception`, `Scopes`, stamp    |
|  [05]   | `FakeLoggerProvider`                     | double    | `ILoggerProvider` over one shared collector for DI-built hosts         |
|  [06]   | `MetricCollector<T>`                     | evidence  | one instrument's measurement stream; `WaitForMeasurementsAsync` gate   |
|  [07]   | `CollectedMeasurement<T>`                | record    | value + tags + `TimeProvider` timestamp per measurement                |
|  [08]   | `MeasurementExtensions`                  | assert    | tag-containment folds over collected measurements                      |
|  [09]   | `AddFakeLogging` / `GetFakeLogCollector` | extension | `ILoggingBuilder`/`IServiceCollection` wiring; provider-level capture  |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                    | [KIND]   | [CAPABILITY]                                                   |
| :-----: | :----------------------------------------------------------- | :------- | :------------------------------------------------------------- |
|  [01]   | `new FakeLogger(FakeLogCollector, string?)`                  | ctor     | direct double for a SUT taking `ILogger`                       |
|  [02]   | `FakeLogCollector.Create(FakeLogCollectorOptions)`           | factory  | one collector shared across loggers                            |
|  [03]   | `collector.GetSnapshot(bool clearRecords)`                   | evidence | the immutable record list a spec folds over                    |
|  [04]   | `new MetricCollector<T>(Instrument<T>, TimeProvider?)`       | ctor     | bind to a held instrument; meter+name and observable overloads |
|  [05]   | `collector.GetMeasurementSnapshot(bool clear)`               | evidence | every `CollectedMeasurement<T>` so far                         |
|  [06]   | `collector.RecordObservableInstruments()`                    | control  | force an observable-instrument observation                     |
|  [07]   | `collector.WaitForMeasurementsAsync(int, CancellationToken)` | gate     | bounded wait for asynchronous emission                         |
|  [08]   | `services.AddFakeLogging(...)` / `sp.GetFakeLogCollector()`  | wiring   | host-built SUTs capture without touching their composition     |

```csharp signature
public class FakeLogger : ILogger {
    public FakeLogger(FakeLogCollector? collector = null, string? category = null);
    public FakeLogCollector Collector { get; }
    public FakeLogRecord LatestRecord { get; }
    public void ControlLevel(LogLevel logLevel, bool enabled);
}
public class FakeLogCollector {
    public static FakeLogCollector Create(FakeLogCollectorOptions options);
    public IReadOnlyList<FakeLogRecord> GetSnapshot(bool clearRecords = false);
    public FakeLogRecord LatestRecord { get; }
    public int Count { get; }
    public void Clear();
}
public sealed class MetricCollector<T> : IDisposable where T : struct {
    public MetricCollector(Instrument<T> instrument, TimeProvider? timeProvider = null);
    public MetricCollector(Meter meter, string instrumentName, TimeProvider? timeProvider = null);
    public CollectedMeasurement<T>? LastMeasurement { get; }
    public IReadOnlyList<CollectedMeasurement<T>> GetMeasurementSnapshot(bool clear = false);
    public Task WaitForMeasurementsAsync(int minCount, CancellationToken cancellationToken = default);
}
```

## [04]-[IMPLEMENTATION_LAW]

[EVIDENCE]: proof reads the snapshot, never the double's wiring — a log obligation asserts on `FakeLogRecord.Level`/`StructuredState` case identity, a metric obligation on measurement value and tags; message-substring scraping stays banned.

[STACKING]:
- `timeprovider-testing.md`: `FakeLogCollectorOptions.TimeProvider` and every `MetricCollector<T>` ctor take the spec's `FakeTimeProvider`, so record timestamps are pure functions of the advance sequence.
- `Rasm.TestKit` (`Seams.cs`): the `Timeline` clock is the same injected `TimeProvider` these doubles consume; one clock owns the whole spec.
- `xunit-v3.md`: plain construction inside `[Fact]` bodies; the DI extensions serve only host-built SUTs.

[LOCAL_ADMISSION]:
- A suite proving telemetry obligations adds this package as its own harness row beside its other suite-owned packages; the shared test stack never injects it estate-wide.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Diagnostics.Testing`
- Owns: captured log-record and metric-measurement evidence inside C# specs.
- Accept: collector snapshots folded through kit gates; `WaitForMeasurementsAsync` as the bounded async gate; `ControlLevel` for disabled-level lanes.
- Reject: `Moq`-style `ILogger` mocks, message-substring assertions, hand-rolled `MeterListener` harnesses, or a sleep where the measurement gate exists.
