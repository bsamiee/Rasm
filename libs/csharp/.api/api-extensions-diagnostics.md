# [RASM_API_EXTENSIONS_DIAGNOSTICS]

`Microsoft.Extensions.Diagnostics` is the dependency-injection companion to the in-box metric surface: `AddMetrics` registers the default `IMeterFactory` into an `IServiceCollection`, so every `Meter` a composition mints reaches the process through a provider-owned factory rather than a raw `new Meter(...)`. The factory registration is provider-scoped, so one `ServiceProvider` built per host or per plugin `AssemblyLoadContext` owns one factory and the meters it mints; the contract assembly `Microsoft.Extensions.Diagnostics.Abstractions` carries the instrument-enablement grammar — `IMetricsBuilder`, `IMetricsListener`, `InstrumentRule`, `MeterScope`, `MetricsOptions` — so a listener subscribes and rule rows enable or disable instrument publication without an emitting library ever naming the DI seam.

## [01]-[PACKAGE_SURFACE]

- Package: `Microsoft.Extensions.Diagnostics`
- License: MIT
- Namespace: `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Diagnostics.Metrics`
- Asset: `Microsoft.Extensions.Diagnostics.dll` + the `Microsoft.Extensions.Diagnostics.Abstractions` contract assembly carrying the enablement grammar
- Rail: composition-root `IMeterFactory` mint behind every provider-owned meter

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                       | [KIND]     | [CAPABILITY]                                                           |
| :-----: | :----------------------------- | :--------- | :--------------------------------------------------------------------- |
|  [01]   | `MetricsServiceExtensions`     | static     | `AddMetrics` DI registration of `IMeterFactory` and the listener graph |
|  [02]   | `IMetricsBuilder`              | contract   | `Services` accessor the enablement and listener rows extend            |
|  [03]   | `IMetricsListener`             | contract   | consumes published instruments; `Name` keys rule targeting             |
|  [04]   | `IObservableInstrumentsSource` | contract   | `RecordObservableInstruments()` pull a listener drives                 |
|  [05]   | `InstrumentRule`               | rule       | `(meterName, instrumentName, listenerName, MeterScope, enable)` row    |
|  [06]   | `MeterScope`                   | flags enum | `None`/`Global`/`Local` publication selector on a rule                 |
|  [07]   | `MetricsOptions`               | options    | `Rules` — the ordered `InstrumentRule` list                            |
|  [08]   | `MeasurementHandlers`          | callbacks  | per-numeric-type `MeasurementCallback<T>` slots a listener binds       |
|  [09]   | `ConsoleMetrics`               | constants  | `DebugListenerName` the debug listener registers under                 |

## [03]-[ENTRYPOINTS]

`MetricsBuilderExtensions.EnableMetrics`/`DisableMetrics` take `(IMetricsBuilder, meterName, instrumentName?, listenerName?, MeterScope)`; `MetricsBuilderConfigurationExtensions.AddConfiguration` takes `(IMetricsBuilder, IConfiguration)`; `AddListener` carries a generic `AddListener<T>` beside an `AddListener(IMetricsListener)` overload, and `ClearListeners` resets the listener set.

| [INDEX] | [SURFACE]                                                          | [KIND]    | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------------------------- | :-------- | :---------------------------------------------------- |
|  [01]   | `AddMetrics(IServiceCollection)`                                   | mint      | `TryAddSingleton<IMeterFactory, DefaultMeterFactory>` |
|  [02]   | `AddMetrics(IServiceCollection, Action<IMetricsBuilder>)`          | configure | the mint plus builder-scoped enablement rows          |
|  [03]   | `MetricsBuilderExtensions.EnableMetrics`                           | rule      | appends an enable `InstrumentRule`                    |
|  [04]   | `MetricsBuilderExtensions.DisableMetrics`                          | rule      | appends a disable `InstrumentRule`                    |
|  [05]   | `MetricsBuilderExtensions.AddListener<T>`                          | listener  | registers a listener                                  |
|  [06]   | `MetricsBuilderConfigurationExtensions.AddConfiguration`           | binding   | binds `MetricsOptions.Rules` from a config section    |
|  [07]   | `MetricsBuilderConsoleExtensions.AddDebugConsole(IMetricsBuilder)` | debug     | registers the debug-only console listener             |

## [04]-[IMPLEMENTATION_LAW]

[MINT_TOPOLOGY]:
- `AddMetrics` registers `IMeterFactory` through `TryAddSingleton<IMeterFactory, DefaultMeterFactory>` — the factory is provider-scoped, one instance per built `ServiceProvider`, and idempotent under a repeated call.
- `DefaultMeterFactory` caches minted meters by name in instance state; `Dispose` disposes every cached meter and clears the map, so provider disposal is the authoritative meter release — never a `Meter.Dispose` at an emitting site.
- Instrument enablement rides `InstrumentRule` rows on `MetricsOptions`: `EnableMetrics`/`DisableMetrics` append rows, `MeterScope` flags select `Global`/`Local` publication, and an `IMetricsListener` consumes the instruments a rule set publishes.

[ISOLATION]:
- The mint is per-instance: a distinct `ServiceProvider` built per `AssemblyLoadContext` resolves a distinct `DefaultMeterFactory` with a disjoint meter cache, so two co-resident plugin ALCs minting identically-named meters stay isolated by provider scope.
- The `ServiceProvider` is the per-instance handle and the lifetime owner — disposing it disposes the factory and every meter minted through it; no static or process-global factory participates.

[STACKING]:
- `System.Diagnostics.Metrics` (`api-diagnostics-metrics.md`): this package mints the `IMeterFactory` that catalog names as the sole meter path; `IMeterFactory.Create(MeterOptions)` and the `Meter.Create*` instrument binds ride the provider-owned factory.
- `Rasm.AppHost` `Observability/instruments#PROVIDER_LIFETIME`: `PluginTelemetryHost.Open` builds `new ServiceCollection().AddMetrics().BuildServiceProvider()` per ALC and exposes `Meters => GetRequiredService<IMeterFactory>()`; `AssemblyLoadContext.Unloading` disposes the provider so the factory releases its meters after the final `ForceFlush`.
- `Rasm.Grasshopper` `Shell/telemetry#CUSTODY` and `Rasm.Rhino` `HostUi/shell#TELEMETRY_ROOT`: `GhTelemetry.Of(IMeterFactory, …)` and `ShellTelemetry` consume the injected per-ALC factory; the emitting boundary never registers or constructs it.
- `Microsoft.Extensions.Diagnostics.Testing` (`tests/csharp/.api/diagnostics-testing.md`): `MetricCollector<T>(factory, …)` observes one instrument's stream over a test-scoped factory built the same way.

[LOCAL_ADMISSION]:
- The DI mint is composition-root surface — a host or per-ALC capsule calls `AddMetrics`; a library takes `IMeterFactory` by injection and never registers the graph.
- Reject: a static or process-global meter factory beside the provider-scoped registration; a raw `MeterListener` hand-wired where an `IMetricsListener` rule fits; `AddDebugConsole` in a shipped composition, since it is debug-only by declaration.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Diagnostics` (+ `Microsoft.Extensions.Diagnostics.Abstractions` contracts)
- Owns: the composition-root `IMeterFactory` mint and the instrument-enablement rule and listener grammar
- Accept: a per-provider `AddMetrics` registration, `InstrumentRule` enable/disable rows, an `IMetricsListener` over published instruments
- Reject: a library-level `AddMetrics`; a static factory; a debug console listener in production composition
