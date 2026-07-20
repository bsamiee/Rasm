# [RASM_APPUI_API_DIAGNOSTICS_METRICS]

`System.Diagnostics.Metrics` binds the AppUi telemetry spine: every `rasm.appui.*` instrument materializes through `Meter` create calls carried on `InstrumentKind` bind delegates, and the meter mints only through the AppHost factory seam. Substrate canonical members live at `libs/csharp/.api/api-diagnostics-metrics.md`; this overlay carries only the AppUi delta — the spine bindings and the folder admission rule.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-diagnostics-metrics.md`
- type roster, entrypoints, and the metrics topology law live on the substrate catalog — this overlay never re-states them
- rail: AppUi metric emission

## [02]-[APPUI_BINDINGS]

- `TelemetryIdentity.Mint` is the only AppUi meter mint, and `InstrumentRow.Bind` is the delegate slot every create call lives in; `AppUiTelemetry.Mount` freezes the contributions with duplicate-name collision at build.
- `InstrumentKind` derives each `InstrumentSpec` roster row into its bind delegate — `Count`, `Distribution` over `UiBuckets` advice, `Level` over scalar `UiLevelCells` readers, `Levels` over keyed `UiLevelCells` families projected as tagged `Measurement<long>` batches through the multi-measurement observe overload — so an emitting page declares `InstrumentSpec` rows and never spells a create or write call.
- `MetricCollector<T>` (`tests/csharp/.api/diagnostics-testing.md`) observes one instrument's measurement stream for the proof lane; `ProofLaw.InstrumentFold` is the collector rail.

## [03]-[IMPLEMENTATION_LAW]

[LOCAL_ADMISSION]:
- Admitted only inside `Diagnostics/evidence.md` telemetry-spine fences — `InstrumentKind` bind delegates, `AppUiTelemetry.Mount`, level-cell readers.
- Reject: a `new Meter(...)` construction anywhere (the factory mint is the only path); an inline create call at an emitting site; an `UpDownCounter` where a level cell with its observable gauge states the fact.
