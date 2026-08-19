# [RASM_API_DIAGNOSTICS_METRICS]

`System.Diagnostics.Metrics` owns vendor-neutral metric emission: one `Meter` per instrumentation scope mints every instrument, and a measurement reaches the process only through the instrument that mint returned. Aggregation, cardinality policy, and export sit at the composition root, so an emitting library declares instrument rows and writes measurements.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Diagnostics.Metrics`
- package: BCL inbox (MIT)
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
|  [19]   | `MeterListener`              | sealed class  | in-process measurement subscription          |

[MeterOptions]: `Name` `Version` `Tags` `Scope` `TelemetrySchemaUrl`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: every create leads on `string name`, closes on optional `string? unit`, `string? description`, and an `IEnumerable<KeyValuePair<string, object?>>` tag tail; `CreateHistogram<T>` appends `InstrumentAdvice<T>?`, every observable create takes its callback second. Each synchronous write overloads one-to-three `KeyValuePair<string, object?>` args over `params KeyValuePair<string, object?>[]` and `in TagList`.

| [INDEX] | [SURFACE]                                                                           | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `IMeterFactory.Create(MeterOptions) -> Meter`                                       | instance | the one provider-scoped meter mint      |
|  [02]   | `MeterFactoryExtensions.Create(IMeterFactory, string) -> Meter`                     | static   | name-shaped mint on an injected factory |
|  [03]   | `MeterOptions(string)`                                                              | ctor     | scope name the property set completes   |
|  [04]   | `Meter.CreateCounter<T>(string) -> Counter<T>`                                      | instance | monotonic count bind                    |
|  [05]   | `Meter.CreateUpDownCounter<T>(string) -> UpDownCounter<T>`                          | instance | signed-delta bind                       |
|  [06]   | `Meter.CreateHistogram<T>(string) -> Histogram<T>`                                  | instance | distribution bind under bucket advice   |
|  [07]   | `Meter.CreateGauge<T>(string) -> Gauge<T>`                                          | instance | call-site last-value bind               |
|  [08]   | `Meter.CreateObservableCounter<T>(string, Func<IEnumerable<Measurement<T>>>)`       | instance | monotonic total; absence spellable      |
|  [09]   | `Meter.CreateObservableUpDownCounter<T>(string, Func<IEnumerable<Measurement<T>>>)` | instance | signed total; absence spellable         |
|  [10]   | `Meter.CreateObservableGauge<T>(string, Func<IEnumerable<Measurement<T>>>)`         | instance | level or keyed family, tagged           |
|  [11]   | `Meter.CreateObservable{Counter,UpDownCounter,Gauge}<T>(string, Func<T>)`           | instance | scalar twin; publishes every read       |
|  [12]   | `Counter<T>.Add(T, params ReadOnlySpan<KeyValuePair<string, object?>>)`             | instance | tagged count write                      |
|  [13]   | `UpDownCounter<T>.Add(T, params ReadOnlySpan<KeyValuePair<string, object?>>)`       | instance | tagged signed-delta write               |
|  [14]   | `Histogram<T>.Record(T, params ReadOnlySpan<KeyValuePair<string, object?>>)`        | instance | tagged distribution write               |
|  [15]   | `Gauge<T>.Record(T, params ReadOnlySpan<KeyValuePair<string, object?>>)`            | instance | tagged last-value write                 |
|  [16]   | `TagList(params ReadOnlySpan<KeyValuePair<string, object?>>)`                       | ctor     | tag set built once on the stack         |
|  [17]   | `TagList.Add(string, object?)`                                                      | instance | append one dimension                    |
|  [18]   | `Measurement<T>(T, params ReadOnlySpan<KeyValuePair<string, object?>>)`             | ctor     | one observed value with its tags        |
|  [19]   | `Instrument.Enabled`                                                                | property | listener gate before a tag build        |
|  [20]   | `Instrument<T>.Advice`                                                              | property | the bucket advice the create bound      |

[ENTRYPOINT_SCOPE]: in-process subscription — the read path an SDK-less consumer binds against a published meter

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :---------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `MeterListener.InstrumentPublished`                                     | property | per-instrument admission callback        |
|  [02]   | `MeterListener.EnableMeasurementEvents(Instrument, object?)`            | instance | subscribe one admitted instrument        |
|  [03]   | `MeterListener.SetMeasurementEventCallback<T>(MeasurementCallback<T>?)` | instance | typed receiver per measurement type      |
|  [04]   | `MeterListener.Start()`                                                 | instance | publish instruments, observe none        |
|  [05]   | `MeterListener.RecordObservableInstruments()`                           | instance | observe every subscribed observable once |
|  [06]   | `MeterListener.MeasurementsCompleted`                                   | property | instrument or meter disposal callback    |
|  [07]   | `MeasurementCallback<T>(Instrument, T, tags span, object?)`             | delegate | four-arg receiver each typed bind takes  |
|  [08]   | `Instrument.Name`                                                       | property | key a listener folds a measurement under |
|  [09]   | `Instrument.IsObservable`                                               | property | pushed-versus-pulled polarity at a call  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `MeterListener.Start()` replays `InstrumentPublished` over already-published instruments and observes nothing; an observable bind delivers only from `RecordObservableInstruments()`, which the caller drives on its own cadence and which aggregates every throwing callback into one `AggregateException`, so a bare started listener over observable instruments reads its cell forever unchanged.
- Instrument identity de-duplicates by name inside one meter, so a drifted unit or description forks the stream into two series.
- Every meter reaches a process through `IMeterFactory.Create`, so provider disposal owns instrument lifetime.
- `MeterOptions` is the only mint slot carrying `Scope` and `TelemetrySchemaUrl`.
- Synchronous instruments write an event-shaped fact at the call site, `Gauge<T>` a level the caller already holds, and an observable bind reads state on the collecting thread at collection cadence.
- `ObservableInstrument<T>` derives from `Instrument`, never `Instrument<T>`, so an observable bind reaches a measurement collector by meter and name rather than by handle.

[STACKING]:
- `ActivitySource`(`api-diagnostics-activity.md`): the sibling span surface in this assembly, so one scope name and version stamp the `Meter` and `ActivitySource` mints together.
- `Microsoft.Extensions.Diagnostics`(`api-extensions-diagnostics.md`): `AddMetrics` registers the `IMeterFactory` every mint here resolves, and `InstrumentRule` rows gate instrument publication.
- `Microsoft.Extensions.Diagnostics.Testing`(`tests/csharp/.api/diagnostics-testing.md`): `MetricCollector<T>` binds an `Instrument<T>` by handle or by meter-plus-name and drives `RecordObservableInstruments` over the observable binds.
- `Rasm.AppUi`: `AppUiTelemetry.Mount` freezes contributions on a rail carrying both the duplicate-name collision at build and any contributed pack's descriptor refusal, and declares `InstrumentSpec` rows through the kernel factories, so a `Diagnostics/evidence.md` emitting page never spells a create or write call; `ProofLaw.InstrumentFold` is the `MetricCollector<T>` proof rail.
- `Rasm.Bim`: `BimTelemetry.Rows` declares the `rasm.bim.*` roster through the kernel factories with `TenantContext.TenantSlot` its leading dimension and `rasm.bim.<dimension>` slots beside it, and `BimTelemetry.Tap` mounts every write as a hook subscription off `BimHooks`, so a codec arm, projector, or review fold reaches the meter through a fired fact and never a create or write call.
- `Rasm.Element`: `ElementInstrument` rows each carry their kernel `InstrumentSpec` (`Rows` derives from `Items`) over `rasm.element.<dimension>` slots, and `GraphInstrument.Project` is the one generated-`Switch` fold where each `ElementFact` case meets an `InstrumentSet.Write` addressed by ROW — the one pulled level is the rail's parked-fault depth, which `GraphInstrument.Depth` binds through `InstrumentSet.Bind` for the composition's own lifetime, so an unprojected fact breaks the tap at compile time rather than dropping a series.
- Both AEC rows band their fault dimension on the kernel `KernelInstrument.CategorySlot` rather than a package const, so one query answers which failure class burns across every emitting package.
- `Rasm.Persistence`: `StoreInstruments.Rows` is a static `InstrumentSpec` roster whose bounds read the kernel `Buckets` policy rows, and `StoreInstruments.Arms` is the slot-keyed table where every receipt wire name meets an `InstrumentSet.Write` or an `InstrumentSet.Level` — both cell families in one page, the scalar for engine hit ratios and the keyed for embedded memory regions beside the per-tenant usage census, whose unpartitioned deployment mounts its own untagged entry on the SAME family through the optional cell key; two slots carrying one receipt shape bind one parameterized arm mint under distinct tag values rather than a second body, and a receipt column family — step tells, memory regions, profile phases, I/O events, egress settlement outcomes — rides one instrument under a `(wire field, tag value)` row table, so the write set stays single-owned while the series separate, declared cardinality equals stamped cardinality, and a settlement table covering every column its receipt's drained count partitions keeps the share's denominator whole; `StoreDescriptors.Pack` rides the same contributor port, so the panels and the settlement, plan-stability, headroom, and drain-latency indicators prove against the roster the mounting root just bound.
- `Rasm` telemetry spine: `TelemetryIdentity.Metered` folds the `MeterOptions` mint (its `Mint` sibling adding the paired `ActivitySource` a composing root admits into a band) and `MeasureForm.Mint` is the one delegate slot every create lives in — the `InstrumentSpec` row names the family through `InstrumentKind` and the `MeasureForm` carrying the slot closes the measurement type, so a single generic body spells `CreateCounter`, `CreateUpDownCounter`, `CreateHistogram` (advised through `Buckets.Advised` or plain for the exponential default), `CreateGauge`, `CreateObservableCounter`, `CreateObservableUpDownCounter`, and both `CreateObservableGauge` overloads exactly once; `InstrumentSet.Write` is the one pushed measurement entry, discriminating `Counter<T>`/`UpDownCounter<T>`/`Histogram<T>`/`Gauge<T>` off the bound handle onto a typed rail, `InstrumentSet.Enabled` the row [19] listener probe an emitting fold reads BEFORE its tag mint so a process subscribing to nothing pays no key render and no `TagList` fold, an unmounted name reading enabled there so `Write`'s own refusal survives the gate, `InstrumentSet.Level` and `InstrumentSet.Bind` its pulled pair — a call-site push carrying one optional key and an owner-lifetime registration — each gating on the mounted row's `InstrumentKind.Pulled` column, and `LevelCells.Reader<T>` serves the scalar and the family shapes through the one `Func<IEnumerable<Measurement<T>>>` shape every observable create also accepts — a cell no producer wrote yields no measurement, where the `Func<T>` twin publishes a level on every collection and has no spelling for absence, and a family entry whose key half is absent binds row [18]'s params-span ctor over the EMPTY span so an unpartitioned composition reports the same series untagged rather than earning a second row; one bound name holds a SET of `LevelProbe` rows, so that same ctor carries each owner's own tags off an array materialized at registration, every probe reads inside its own fence against the cycle-wide `AggregateException` fold, and the returned scope retires exactly one registration; every contributing package reaches the surface as a `TelemetryContributorPort` row carrying its instrument roster beside whatever `BoardPack` those rows declare, and `TelemetryContributorPort.Admit` proves that pack against the set a root just mounted; `InstrumentTally` is the branch's one `MeterListener` composition — it admits by HANDLE identity against a mounted `InstrumentSet` so a same-named foreign instrument never enters the read, registers one typed callback per `MeasureForm` row so a measurement type the mint admits can never be a type the listener drops, discriminates `Instrument.IsObservable` to replace a republished observable value where a pushed one accumulates, and drives `RecordObservableInstruments` inside `Read` under a fence because the runtime folds one cycle's throwing callbacks into a single `AggregateException`.

[LOCAL_ADMISSION]:
- Create and write calls live inside a package's declared telemetry-spine fences; an emitting page declares instrument rows.

[RAIL_LAW]:
- Package: `System.Diagnostics.Metrics` (BCL inbox)
- Owns: library-tier instrument declaration and measurement writes behind every `rasm.*` meter
- Accept: a factory-minted `Meter`, instrument rows bound through create delegates, tagged writes over a span or a built `TagList`, observable binds over cell readers
- Reject: `new Meter(...)` at any site, an inline create call at an emitting page, a synchronous instrument polling state an observable bind reads, and an `UpDownCounter` where a level cell with its observable gauge states the fact
