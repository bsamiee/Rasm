# [RASM_APPUI_API_DIAGNOSTICS_METRICS]

`System.Diagnostics.Metrics` is the vendor-neutral metric-emission surface the AppUi telemetry spine binds: every `rasm.appui.*` instrument materializes through `Meter` create calls carried on `InstrumentKind` bind delegates, writes ride the `params ReadOnlySpan` tag overloads on the mounted instruments, and the meter itself is minted only through `IMeterFactory` so instrument lifetime rides the provider. No manifest row exists — the surface ships in-box on the framework, and no OpenTelemetry type is reachable from this package.

## [01]-[PACKAGE_SURFACE]

- Package: BCL inbox
- License: MIT
- Namespace: `System.Diagnostics.Metrics`
- Asset: `System.Diagnostics.DiagnosticSource.dll` (shared framework)
- Rail: `Meter` create calls inside `InstrumentKind` bind delegates; `InstrumentSet` writes at the mount

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
|  [02]   | `Meter.CreateCounter<T>(name, unit, description)`                            | create | `Count` kind bind                    |
|  [03]   | `Meter.CreateHistogram<T>(name, unit, description, tags, advice)`            | create | `Distribution` kind bind with advice |
|  [04]   | `Meter.CreateObservableGauge<T>(name, observeValue, unit, description)`      | create | `Level` kind bind over a cell reader |
|  [05]   | `Counter<T>.Add(T, params ReadOnlySpan<KeyValuePair<string, object?>>)`      | write  | tagged count write                   |
|  [06]   | `Histogram<T>.Record(T, params ReadOnlySpan<KeyValuePair<string, object?>>)` | write  | tagged distribution write            |

## [04]-[IMPLEMENTATION_LAW]

[METRICS_TOPOLOGY]:
- Instrument identity de-duplicates by name inside one meter, so a drifted unit or description forks the stream — name, unit, and description are declaration facts the `InstrumentSpec` row carries once.
- A synchronous instrument records event-shaped facts at the call site; an observable gauge pulls level-shaped facts on the collecting thread — a polled level through a synchronous instrument aliases.
- Provider disposal owns instrument lifetime — never the `Meter` or `Instrument`; every AppUi meter reaches the process through `IMeterFactory.Create`.
- `MeterOptions.TelemetrySchemaUrl` carries the scope schema pin where the composition root demands one; version and correlation tags ride `Version` and `Tags` at the mint.

[STACKING]:
- `Rasm.AppHost`: `TelemetryIdentity.Mint` is the only AppUi meter mint, and `InstrumentRow.Bind` is the delegate slot every create call lives in.
- `Rasm.AppHost`: `Buckets.Advised` composes the advice-bearing histogram create with explicit `HistogramBucketBoundaries`; `InstrumentSet.Count`/`Record` compose the tagged writes.
- `Microsoft.Extensions.Diagnostics.Testing` (`tests/csharp/.api/diagnostics-testing.md`): `MetricCollector<T>` observes one instrument's measurement stream for the proof lane.

[LOCAL_ADMISSION]:
- Admitted only inside `Diagnostics/evidence.md` telemetry-spine fences — `InstrumentKind` bind delegates, `AppUiTelemetry.Mount`, level-cell readers; an emitting page declares `InstrumentSpec` rows and never spells a create or write call.
- Reject: a `new Meter(...)` construction anywhere (the factory mint is the only path); an inline create call at an emitting site; an `UpDownCounter` where a level cell with its observable gauge states the fact.
