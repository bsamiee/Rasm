# [CSHARP_TESTING_API_DIAGNOSTICS_TESTING]

`Microsoft.Extensions.Diagnostics.Testing` ships the R9 telemetry doubles: `FakeLogger`/`FakeLogCollector` capture every structured log record as a typed `FakeLogRecord`, and `MetricCollector<T>` captures every measurement an `Instrument<T>` emits with its tags and timestamp. Telemetry obligations — a failure-path log a rail writes, a counter an operation bumps — assert as one snapshot lookup instead of a provider mock, and both doubles take `TimeProvider`, so captured timestamps ride the same `FakeTimeProvider` clock as the rest of the spec (`timeprovider-testing.md`).

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

| [INDEX] | [SURFACE]                                                    | [KIND]   | [CAPABILITY]                                               |
| :-----: | :----------------------------------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `new FakeLogger(FakeLogCollector, string?)`                  | ctor     | direct double for a SUT taking `ILogger`                   |
|  [02]   | `FakeLogCollector.Create(FakeLogCollectorOptions)`           | factory  | one collector shared across loggers                        |
|  [03]   | `collector.GetSnapshot(bool clearRecords)`                   | evidence | the immutable record list a spec folds over                |
|  [04]   | `new MetricCollector<T>(Instrument<T>, TimeProvider?)`       | ctor     | bind a held pushed instrument                              |
|  [05]   | `new MetricCollector<T>(ObservableInstrument<T>, ...)`       | ctor     | bind a held pulled instrument                              |
|  [06]   | `new MetricCollector<T>(Meter, string, TimeProvider?)`       | ctor     | resolve by name on a factory-minted meter                  |
|  [07]   | `new MetricCollector<T>(object?, string, string, ...)`       | ctor     | resolve by meter scope + meter name + instrument name      |
|  [08]   | `collector.GetMeasurementSnapshot(bool clear)`               | evidence | every `CollectedMeasurement<T>` so far                     |
|  [09]   | `collector.LastMeasurement` / `collector.Instrument`         | evidence | latest measurement; the bound instrument once published    |
|  [10]   | `collector.RecordObservableInstruments()`                    | control  | force an observable-instrument observation                 |
|  [11]   | `collector.Clear()`                                          | control  | drop captured measurements without re-binding              |
|  [12]   | `collector.WaitForMeasurementsAsync(int, CancellationToken)` | gate     | bounded wait for asynchronous emission                     |
|  [13]   | `collector.WaitForMeasurementsAsync(int, TimeSpan)`          | gate     | the same wait under a wall timeout                         |
|  [14]   | `services.AddFakeLogging(...)` / `sp.GetFakeLogCollector()`  | wiring   | host-built SUTs capture without touching their composition |

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
    public MetricCollector(ObservableInstrument<T> instrument, TimeProvider? timeProvider = null);
    public MetricCollector(object? meterScope, string meterName, string instrumentName, TimeProvider? timeProvider = null);
    public MetricCollector(Meter meter, string instrumentName, TimeProvider? timeProvider = null);
    public Instrument? Instrument { get; }
    public CollectedMeasurement<T>? LastMeasurement { get; }
    public void Clear();
    public IReadOnlyList<CollectedMeasurement<T>> GetMeasurementSnapshot(bool clear = false);
    public void RecordObservableInstruments();
    public Task WaitForMeasurementsAsync(int minCount, CancellationToken cancellationToken = default);
    public Task WaitForMeasurementsAsync(int minCount, TimeSpan timeout);
}
```

## [04]-[IMPLEMENTATION_LAW]

[EVIDENCE]: proof reads the snapshot, never the double's wiring — a log obligation asserts on `FakeLogRecord.Level`/`StructuredState` case identity, a metric obligation on measurement value and tags; message-substring scraping stays banned.

[STACKING]:
- `timeprovider-testing.md`: `FakeLogCollectorOptions.TimeProvider` and every `MetricCollector<T>` ctor carry the spec's `FakeTimeProvider` as their trailing optional argument, so record timestamps are pure functions of the advance sequence.
- `Rasm.TestKit` (`Seams.cs`): the `Timeline` clock is the same injected `TimeProvider` these doubles consume; one clock owns the whole spec.
- `xunit-v3.md`: plain construction inside `[Fact]` bodies; the DI extensions serve only host-built SUTs.

[LOCAL_ADMISSION]:
- Suites proving telemetry obligations carry this package as their own harness row beside their other suite-owned packages; the shared test stack never injects it estate-wide.

[MEASUREMENT_DOMAIN]: `MetricCollector<T>` admits `int`, `byte`, `short`, `long`, `float`, `double`, and `decimal` alone, and any other `T` throws at construction rather than capturing nothing — the two measurement forms an estate `InstrumentSpec` row binds both sit inside that set, so a spec chooses `T` off the row's own form and never off the value it asserts.

[BINDING_SHAPE]: four constructors resolve one instrument and the SUT's binding shape picks among them — a held `Instrument<T>` or `ObservableInstrument<T>` binds directly, a meter-plus-name pair resolves against a factory-minted meter (parallel-safe, since the factory scopes that meter to its own provider), and the scope-plus-names form resolves a meter the spec never holds. No overload takes an `IMeterFactory`: that factory's product IS the `Meter`, and the scope overload's `object?` carries that meter's own `Scope`.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Diagnostics.Testing`
- Owns: captured log-record and metric-measurement evidence inside C# specs.
- Accept: collector snapshots folded through kit gates; `WaitForMeasurementsAsync` as the bounded async gate under a token or a wall timeout; `RecordObservableInstruments` before every pulled-row read; `ControlLevel` for disabled-level lanes.
- Reject: `Moq`-style `ILogger` mocks, message-substring assertions, hand-rolled `MeterListener` harnesses, a null-scope global-meter binding inside a parallel lane, or a sleep where the measurement gate exists.
