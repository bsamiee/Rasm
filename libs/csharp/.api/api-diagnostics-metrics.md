# [RASM_API_DIAGNOSTICS_METRICS]

`System.Diagnostics.Metrics` is the vendor-neutral metric-emission surface every minted meter composes: instruments materialize through `Meter` create calls, writes ride the `params ReadOnlySpan` tag overloads, and every meter reaches a process through `IMeterFactory` so instrument lifetime rides the provider. No manifest row exists — the surface ships in-box on the framework — and no OpenTelemetry type is reachable from an emitting library.

## [01]-[PACKAGE_SURFACE]

- Package: BCL inbox
- License: MIT
- Namespace: `System.Diagnostics.Metrics`
- Asset: `System.Diagnostics.DiagnosticSource.dll` (shared framework)
- Rail: library-tier metric emission behind every `rasm.*` instrument

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]              | [KIND]      | [CAPABILITY]                                             |
| :-----: | :-------------------- | :---------- | :------------------------------------------------------- |
|  [01]   | `IMeterFactory`       | factory     | provider-owned meter mint; `IDisposable` lifetime        |
|  [02]   | `MeterOptions`        | options     | `Name`, `Version`, `Tags`, `Scope`, `TelemetrySchemaUrl` |
|  [03]   | `Meter`               | scope owner | instrument factory for one instrumentation scope         |
|  [04]   | `Instrument`          | base        | name, unit, description identity every create returns    |
|  [05]   | `Counter<T>`          | instrument  | monotonic event counts                                   |
|  [06]   | `Histogram<T>`        | instrument  | distributions under optional bucket advice               |
|  [07]   | `ObservableGauge<T>`  | instrument  | current-level reads pulled at collection cadence         |
|  [08]   | `UpDownCounter<T>`    | instrument  | signed level deltas; observable twin available           |
|  [09]   | `InstrumentAdvice<T>` | advice      | `HistogramBucketBoundaries` (`IReadOnlyList<T>?`)        |
|  [10]   | `Measurement<T>`      | value       | multi-measurement observable callbacks                   |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                                    | [KIND] | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------------------------------- | :----- | :----------------------------------- |
|  [01]   | `IMeterFactory.Create(MeterOptions)`                                         | mint   | the one provider-scoped meter mint   |
|  [02]   | `Meter.CreateCounter<T>(name, unit, description)`                            | create | count-kind instrument bind           |
|  [03]   | `Meter.CreateHistogram<T>(name, unit, description, tags, advice)`            | create | distribution bind with bucket advice |
|  [04]   | `Meter.CreateObservableGauge<T>(name, observeValue, unit, description)`      | create | level bind over a cell reader        |
|  [05]   | `Counter<T>.Add(T, params ReadOnlySpan<KeyValuePair<string, object?>>)`      | write  | tagged count write                   |
|  [06]   | `Histogram<T>.Record(T, params ReadOnlySpan<KeyValuePair<string, object?>>)` | write  | tagged distribution write            |

## [04]-[IMPLEMENTATION_LAW]

[METRICS_TOPOLOGY]:
- Instrument identity de-duplicates by name inside one meter, so a drifted unit or description forks the stream — name, unit, and description are declaration facts the owning instrument row carries once.
- A synchronous instrument records event-shaped facts at the call site; an observable gauge pulls level-shaped facts on the collecting thread — a polled level through a synchronous instrument aliases.
- Provider disposal owns instrument lifetime — never the `Meter` or `Instrument`; every meter reaches the process through `IMeterFactory.Create`.
- `MeterOptions.TelemetrySchemaUrl` carries the scope schema pin where the composition root demands one; version and correlation tags ride `Version` and `Tags` at the mint.

[STACKING]:
- `Rasm` `Domain/telemetry#INSTRUMENT_MECHANISM`: `TelemetryIdentity.Mint` is the one meter mint and `InstrumentRow.Bind` the delegate slot every create call lives in; `Buckets.Advised` composes the advice-bearing histogram create, `InstrumentSet.Count`/`Record` the tagged writes, and `LevelCells.Reader` the scalar and tagged-measurement gauge callbacks.
- `Rasm.AppUi` (`Rasm.AppUi/.api/api-diagnostics-metrics.md`): `InstrumentKind` bind delegates and the `AppUiTelemetry` spine — the folder overlay carries the bindings.
- `Rasm.Persistence` `Store/observability#STORE_INSTRUMENTS`, `Rasm.Compute` `Runtime/receipts#TELEMETRY_PROJECTION`, `Rasm.Fabrication` `Process/telemetry`: each package roster binds through its contributor port over this surface.
- `Microsoft.Extensions.Diagnostics.Testing` (`tests/csharp/.api/diagnostics-testing.md`): `MetricCollector<T>` observes one instrument's measurement stream.

[LOCAL_ADMISSION]:
- Create and write calls live only inside a package's declared telemetry-spine fences; an emitting page declares instrument rows and never spells a create or write call.
- Reject: a `new Meter(...)` construction anywhere — the factory mint is the only path; an inline create call at an emitting site; an `UpDownCounter` where a level cell with its observable gauge states the fact.
