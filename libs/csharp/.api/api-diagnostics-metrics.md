# [RASM_API_DIAGNOSTICS_METRICS]

`System.Diagnostics.Metrics` owns vendor-neutral metric emission: one `Meter` per instrumentation scope mints every instrument, and a measurement reaches the process only through the instrument that mint returned. Aggregation, cardinality policy, and export sit at the composition root, so an emitting library declares instrument rows and writes measurements.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Diagnostics.Metrics`
- package: BCL inbox (MIT, .NET Foundation)
- assembly: `System.Diagnostics.DiagnosticSource.dll` (shared framework)
- namespace: `System.Diagnostics.Metrics`, `System.Diagnostics`
- rail: library-tier metric emission behind every `rasm.*` instrument

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: one meter scope, its instrument families, and the tag and measurement carriers every write composes

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :--------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `IMeterFactory`              | interface     | provider-owned meter mint                    |
|  [02]   | `MeterFactoryExtensions`     | class         | name-shaped mint over an injected factory    |
|  [03]   | `MeterOptions`               | class         | scope identity the mint consumes             |
|  [04]   | `Meter`                      | class         | instrument factory for one scope             |
|  [05]   | `Instrument`                 | class         | identity and enablement every create returns |
|  [06]   | `Instrument<T>`              | class         | typed measurement base carrying advice       |
|  [07]   | `InstrumentAdvice<T>`        | class         | histogram bucket-boundary hint               |
|  [08]   | `Counter<T>`                 | class         | monotonic event counts                       |
|  [09]   | `UpDownCounter<T>`           | class         | signed level deltas                          |
|  [10]   | `Histogram<T>`               | class         | value distributions under advice             |
|  [11]   | `Gauge<T>`                   | class         | call-site last-value writes                  |
|  [12]   | `ObservableInstrument<T>`    | class         | collection-cadence pull base                 |
|  [13]   | `ObservableCounter<T>`       | class         | monotonic totals pulled at collection        |
|  [14]   | `ObservableUpDownCounter<T>` | class         | signed totals pulled at collection           |
|  [15]   | `ObservableGauge<T>`         | class         | current levels pulled at collection          |
|  [16]   | `Measurement<T>`             | struct        | one observed value with its tags             |
|  [17]   | `TagList`                    | struct        | stack-allocated tag set                      |
|  [18]   | `MeasurementCallback<T>`     | delegate      | listener-side measurement receiver           |

[MeterOptions]: `Name` `Version` `Tags` `Scope` `TelemetrySchemaUrl`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: every create leads on `string name`, closes on optional `string? unit`, `string? description`, and an `IEnumerable<KeyValuePair<string, object?>>` tag tail; `CreateHistogram<T>` appends `InstrumentAdvice<T>?`, every observable create takes its callback second. Each synchronous write overloads one-to-three `KeyValuePair<string, object?>` args over `params KeyValuePair<string, object?>[]` and `in TagList`.

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `IMeterFactory.Create(MeterOptions) -> Meter`                                 | instance | the one provider-scoped meter mint        |
|  [02]   | `MeterFactoryExtensions.Create(IMeterFactory, string) -> Meter`               | static   | name-shaped mint over an injected factory |
|  [03]   | `MeterOptions(string)`                                                        | ctor     | scope name the property set completes     |
|  [04]   | `Meter.CreateCounter<T>(string) -> Counter<T>`                                | instance | monotonic count bind                      |
|  [05]   | `Meter.CreateUpDownCounter<T>(string) -> UpDownCounter<T>`                    | instance | signed-delta bind                         |
|  [06]   | `Meter.CreateHistogram<T>(string) -> Histogram<T>`                            | instance | distribution bind under bucket advice     |
|  [07]   | `Meter.CreateGauge<T>(string) -> Gauge<T>`                                    | instance | call-site last-value bind                 |
|  [08]   | `Meter.CreateObservableCounter<T>(string, Func<T>)`                           | instance | monotonic total over a reader             |
|  [09]   | `Meter.CreateObservableUpDownCounter<T>(string, Func<T>)`                     | instance | signed total over a reader                |
|  [10]   | `Meter.CreateObservableGauge<T>(string, Func<T>)`                             | instance | scalar level over a cell reader           |
|  [11]   | `Meter.CreateObservableGauge<T>(string, Func<IEnumerable<Measurement<T>>>)`   | instance | keyed family as tagged measurements       |
|  [12]   | `Counter<T>.Add(T, params ReadOnlySpan<KeyValuePair<string, object?>>)`       | instance | tagged count write                        |
|  [13]   | `UpDownCounter<T>.Add(T, params ReadOnlySpan<KeyValuePair<string, object?>>)` | instance | tagged signed-delta write                 |
|  [14]   | `Histogram<T>.Record(T, params ReadOnlySpan<KeyValuePair<string, object?>>)`  | instance | tagged distribution write                 |
|  [15]   | `Gauge<T>.Record(T, params ReadOnlySpan<KeyValuePair<string, object?>>)`      | instance | tagged last-value write                   |
|  [16]   | `TagList(params ReadOnlySpan<KeyValuePair<string, object?>>)`                 | ctor     | tag set built once on the stack           |
|  [17]   | `TagList.Add(string, object?)`                                                | instance | append one dimension                      |
|  [18]   | `Measurement<T>(T, params ReadOnlySpan<KeyValuePair<string, object?>>)`       | ctor     | one observed value with its tags          |
|  [19]   | `Instrument.Enabled`                                                          | property | listener gate before a tag build          |
|  [20]   | `Instrument<T>.Advice`                                                        | property | the bucket advice the create bound        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Instrument identity de-duplicates by name inside one meter, so a drifted unit or description forks the stream into two series.
- Every meter reaches a process through `IMeterFactory.Create`, so provider disposal owns instrument lifetime.
- `MeterOptions` is the only mint slot carrying `Scope` and `TelemetrySchemaUrl`.
- Synchronous instruments write an event-shaped fact at the call site, `Gauge<T>` a level the caller already holds, and an observable bind reads state on the collecting thread at collection cadence.
- `ObservableInstrument<T>` derives from `Instrument`, never `Instrument<T>`, so an observable bind reaches a measurement collector by meter and name rather than by handle.

[STACKING]:
- `ActivitySource`(`api-diagnostics-activity.md`): the sibling span surface in this assembly, so one scope name and version stamp the `Meter` and `ActivitySource` mints together.
- `Microsoft.Extensions.Diagnostics`(`api-extensions-diagnostics.md`): `AddMetrics` registers the `IMeterFactory` every mint here resolves, and `InstrumentRule` rows gate instrument publication.
- `Microsoft.Extensions.Diagnostics.Testing`(`tests/csharp/.api/diagnostics-testing.md`): `MetricCollector<T>` binds an `Instrument<T>` by handle or by meter-plus-name and drives `RecordObservableInstruments` over the observable binds.
- `Rasm.AppUi`: `TelemetryIdentity.Mint` is the only meter mint and `InstrumentRow.Bind` the delegate slot every create call lives in; `AppUiTelemetry.Mount` freezes contributions with duplicate-name collision at build, and `InstrumentKind` derives each `InstrumentSpec` row into its bind delegate — `Count`, `Distribution` over kernel `Buckets` advice, `Level` over scalar `LevelCells` readers, `Levels` over keyed families projected as tagged `Measurement<long>` batches — so the `Diagnostics/evidence.md` emitting pages declare `InstrumentSpec` rows and never spell a create or write call; `ProofLaw.InstrumentFold` is the `MetricCollector<T>` proof rail.
- `Rasm` telemetry spine: `TelemetryIdentity.Mint` folds the `MeterOptions` mint, `Buckets.Advised` the advice-bearing histogram create, `InstrumentRow.Observable` the gauge callback, `InstrumentSet.Count`/`Record` the span writes, and `LevelCells.Reader` both the scalar and `Func<IEnumerable<Measurement<long>>>` families; every contributing package reaches the surface as a `TelemetryContributorPort` row.

[LOCAL_ADMISSION]:
- Create and write calls live inside a package's declared telemetry-spine fences; an emitting page declares instrument rows.

[RAIL_LAW]:
- Package: `System.Diagnostics.Metrics` (BCL inbox)
- Owns: library-tier instrument declaration and measurement writes behind every `rasm.*` meter
- Accept: a factory-minted `Meter`, instrument rows bound through create delegates, tagged writes over a span or a built `TagList`, observable binds over cell readers
- Reject: `new Meter(...)` at any site, an inline create call at an emitting page, a synchronous instrument polling state an observable bind reads, and an `UpDownCounter` where a level cell with its observable gauge states the fact
