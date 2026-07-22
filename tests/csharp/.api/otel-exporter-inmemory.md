# [CSHARP_TESTING_API_OTEL_EXPORTER_INMEMORY]

`OpenTelemetry.Exporter.InMemory` captures a full-SDK telemetry pipeline into a caller-owned `ICollection<T>`: `AddInMemoryExporter` registers an `InMemoryExporter<T>` on a `TracerProviderBuilder`, `MeterProviderBuilder`, `LoggerProviderBuilder`, or `OpenTelemetryLoggerOptions`, and every `Activity`, `Metric`, or `LogRecord` the SDK emits — through its real resource, views, exemplars, and processors — lands in the collection for a spec to fold.

Trace and log export assertions have no other in-process rail; the metrics lane complements `MetricCollector<T>` (`diagnostics-testing.md`), which reads one instrument's raw measurement stream without spinning up an SDK pipeline.

## [01]-[PACKAGE_SURFACE]

- package: `OpenTelemetry.Exporter.InMemory`
- license: `Apache-2.0`
- namespaces: `OpenTelemetry.Exporter`, `OpenTelemetry.Trace`, `OpenTelemetry.Metrics`, `OpenTelemetry.Logs`
- asset: `lib/net10.0/OpenTelemetry.Exporter.InMemory.dll`
- rail: evidence — full-SDK trace/metric/log export captured into a caller `ICollection<T>`; a suite-owned harness row (`PrivateAssets="all"`), never centrally injected

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                            | [KIND]    | [CAPABILITY]                                                                           |
| :-----: | :---------------------------------- | :-------- | :------------------------------------------------------------------------------------- |
|  [01]   | `InMemoryExporter<T>`               | exporter  | `: BaseExporter<T>`; writes each exported batch item into the bound `ICollection<T>`   |
|  [02]   | `InMemoryExporter<T>.ExportFunc`    | delegate  | `delegate(in Batch<T>)` export callback the second ctor overload takes                 |
|  [03]   | `MetricSnapshot`                    | record    | point-in-time copy of a `Metric`; `Name`, `MetricType`, `MetricPoints`                 |
|  [04]   | `InMemoryExporterHelperExtensions`  | extension | `AddInMemoryExporter` over `TracerProviderBuilder`                                     |
|  [05]   | `InMemoryExporterMetricsExtensions` | extension | `AddInMemoryExporter` over `MeterProviderBuilder`, `Metric` and `MetricSnapshot` items |
|  [06]   | `InMemoryExporterLoggingExtensions` | extension | `AddInMemoryExporter` over `LoggerProviderBuilder` and `OpenTelemetryLoggerOptions`    |

## [03]-[ENTRYPOINTS]

Every overload binds a caller `ICollection<T>`; fence carries the full `AddInMemoryExporter` family and the `MetricSnapshot` shape.

| [INDEX] | [SURFACE]                                                          | [KIND] | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------------------------- | :----- | :---------------------------------------------- |
|  [01]   | `AddInMemoryExporter(ICollection<Activity>)`                       | trace  | on `TracerProviderBuilder`; spans as `Activity` |
|  [02]   | `AddInMemoryExporter(ICollection<Metric>, name?, reader?)`         | metric | on `MeterProviderBuilder`; live `Metric`        |
|  [03]   | `AddInMemoryExporter(ICollection<MetricSnapshot>, name?, reader?)` | metric | on `MeterProviderBuilder`; frozen snapshots     |
|  [04]   | `AddInMemoryExporter(ICollection<LogRecord>)`                      | log    | on `OpenTelemetryLoggerOptions`                 |
|  [05]   | `AddInMemoryExporter(ICollection<LogRecord>)`                      | log    | on `LoggerProviderBuilder`                      |

```csharp signature
public class InMemoryExporter<T> : BaseExporter<T> where T : class {
    public InMemoryExporter(ICollection<T> exportedItems);
    public InMemoryExporter(ExportFunc exportFunc);
    public delegate ExportResult ExportFunc(in Batch<T> batch);
}
public static class InMemoryExporterHelperExtensions {
    public static TracerProviderBuilder AddInMemoryExporter(this TracerProviderBuilder builder, ICollection<Activity> exportedItems);
}
public static class InMemoryExporterMetricsExtensions {
    public static MeterProviderBuilder AddInMemoryExporter(this MeterProviderBuilder builder, ICollection<Metric> exportedItems);
    public static MeterProviderBuilder AddInMemoryExporter(this MeterProviderBuilder builder, ICollection<Metric> exportedItems, Action<MetricReaderOptions> configureMetricReader);
    public static MeterProviderBuilder AddInMemoryExporter(this MeterProviderBuilder builder, string? name, ICollection<Metric> exportedItems, Action<MetricReaderOptions>? configureMetricReader);
    public static MeterProviderBuilder AddInMemoryExporter(this MeterProviderBuilder builder, ICollection<MetricSnapshot> exportedItems);
    public static MeterProviderBuilder AddInMemoryExporter(this MeterProviderBuilder builder, ICollection<MetricSnapshot> exportedItems, Action<MetricReaderOptions> configureMetricReader);
    public static MeterProviderBuilder AddInMemoryExporter(this MeterProviderBuilder builder, string? name, ICollection<MetricSnapshot> exportedItems, Action<MetricReaderOptions>? configureMetricReader);
}
public static class InMemoryExporterLoggingExtensions {
    public static OpenTelemetryLoggerOptions AddInMemoryExporter(this OpenTelemetryLoggerOptions loggerOptions, ICollection<LogRecord> exportedItems);
    public static LoggerProviderBuilder AddInMemoryExporter(this LoggerProviderBuilder loggerProviderBuilder, ICollection<LogRecord> exportedItems);
}
public class MetricSnapshot {
    public MetricSnapshot(Metric metric);
    public string Name { get; }
    public MetricType MetricType { get; }
    public IReadOnlyList<MetricPoint> MetricPoints { get; }
}
```

## [04]-[IMPLEMENTATION_LAW]

[EVIDENCE]: proof reads the captured collection after `ForceFlush`, never the exporter's wiring — a trace obligation asserts on `Activity` display name, tags, and status; a metric obligation on `MetricPoint` values and dimensions; a log obligation on `LogRecord` state; message-substring scraping stays banned.

[STACKING]:
- `diagnostics-testing.md`: `MetricCollector<T>` reads one `Instrument<T>`'s raw measurement stream WITHOUT an SDK, so it proves what the instrument emits; this exporter proves the assembled pipeline — resource attributes, view renames and aggregation, exemplars, processors — where a view or exemplar obligation lives that `MetricCollector<T>` cannot see. Trace and log export have no `MetricCollector<T>` counterpart; `InMemoryExporter<T>` is the only in-process rail for either.
- `MetricSnapshot` vs `Metric`: the live `Metric`/`MetricPoint` the SDK exports is reused and mutated across collection cycles, so a multi-cycle metric assertion binds `ICollection<MetricSnapshot>` for frozen values; a single-flush read binds `ICollection<Metric>`.
- `xunit-v3.md`: plain construction and `Sdk.CreateTracerProviderBuilder()`/`Sdk.CreateMeterProviderBuilder()` inside `[Fact]` bodies; the logging overload wires through `OpenTelemetryLoggerOptions` on a host-built SUT.

[LOCAL_ADMISSION]:
- A suite proving full-SDK export obligations adds this package as its own harness row beside its other suite-owned packages; the shared test stack never injects it estate-wide.

[RAIL_LAW]:
- Package: `OpenTelemetry.Exporter.InMemory`
- Owns: captured full-SDK trace, metric, and log export inside C# specs.
- Accept: `AddInMemoryExporter` binding a caller `ICollection<T>` on the real provider builder; `MetricSnapshot` items for multi-cycle metric values; `ForceFlush` before the snapshot fold.
- Reject: asserting instrument streams the lighter `MetricCollector<T>` already owns; a raw `Metric` collection where cross-cycle mutation aliases the values; message-substring assertions over exported items.
