# [RASM_API_EXTENSIONS_DIAGNOSTICS]

`Microsoft.Extensions.Diagnostics` mints the provider-owned `IMeterFactory` at a composition root and folds instrument publication through rule rows an `IMetricsListener` consumes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Diagnostics`
- package: `Microsoft.Extensions.Diagnostics` (MIT)
- assembly: `Microsoft.Extensions.Diagnostics.dll`
- contract assembly: `Microsoft.Extensions.Diagnostics.Abstractions`
- namespace: `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Diagnostics.Metrics`
- rail: composition-root meter mint and instrument enablement

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: enablement grammar and listener contracts, carried by the contract assembly

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :----------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `IMetricsBuilder`              | interface     | `Services` seat every enablement row extends   |
|  [02]   | `IMetricsListener`             | interface     | named consumer of published instruments        |
|  [03]   | `IObservableInstrumentsSource` | interface     | pull handle a listener drives                  |
|  [04]   | `InstrumentRule`               | class         | one meter/instrument/listener/scope match row  |
|  [05]   | `MetricsOptions`               | class         | ordered `Rules` match list                     |
|  [06]   | `MeasurementHandlers`          | class         | per-numeric-type `MeasurementCallback<T>` slot |
|  [07]   | `MeterScope`                   | flags enum    | ctor-built vs factory-built meter selector     |

[IMetricsListener]: `Name` `Initialize` `InstrumentPublished` `MeasurementsCompleted` `GetMeasurementHandlers`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: composition-root registration and the builder rows it configures

| [INDEX] | [SURFACE]                                                                                      | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :--------------------------------------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `MetricsServiceExtensions.AddMetrics(IServiceCollection)`                                      | static   | provider-scoped factory mint |
|  [02]   | `MetricsServiceExtensions.AddMetrics(IServiceCollection, Action<IMetricsBuilder>)`             | static   | mint with builder rows       |
|  [03]   | `MetricsBuilderExtensions.EnableMetrics(IMetricsBuilder, string, string, string, MeterScope)`  | static   | appends an enable rule row   |
|  [04]   | `MetricsBuilderExtensions.DisableMetrics(IMetricsBuilder, string, string, string, MeterScope)` | static   | appends a disable rule row   |
|  [05]   | `MetricsBuilderExtensions.AddListener<T>(IMetricsBuilder)`                                     | static   | listener type from DI        |
|  [06]   | `MetricsBuilderExtensions.AddListener(IMetricsBuilder, IMetricsListener)`                      | static   | listener instance row        |
|  [07]   | `MetricsBuilderExtensions.ClearListeners(IMetricsBuilder)`                                     | static   | drops every listener row     |
|  [08]   | `MetricsBuilderConfigurationExtensions.AddConfiguration(IMetricsBuilder, IConfiguration)`      | static   | rule rows from configuration |
|  [09]   | `MetricsBuilderConsoleExtensions.AddDebugConsole(IMetricsBuilder)`                             | static   | debug console listener row   |
|  [10]   | `ConsoleMetrics.DebugListenerName`                                                             | property | name a debug rule targets    |

- `EnableMetrics`/`DisableMetrics`: each carries a `MetricsOptions` receiver overload for the options-configure path.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `AddMetrics` registers `IMeterFactory` through `TryAddSingleton`, so one built `ServiceProvider` owns one factory and every meter it mints.
- A factory resolves one meter per name, version, and tag triple, so a repeated mint under one identity returns the held `Meter`.
- A provider built per `AssemblyLoadContext` keeps co-resident plugins naming one meter identically on disjoint instruments.
- `MeterScope.Local` selects factory-minted meters and `Global` ctor-constructed ones, so an `InstrumentRule` targets the dependency-injected population alone.
- Rule match resolves most-specific-first: meter name exact, else longest prefix, then instrument name, listener name, and scope.
- `AddConfiguration` binds `MetricsOptions.Rules` to a configuration section under a change token, so a rule edit re-subscribes every listener live.

[STACKING]:
- `System.Diagnostics.Metrics`(`api-diagnostics-metrics.md`): this mint supplies the `IMeterFactory` that catalog names as the sole meter path, so `Create(MeterOptions)` and every instrument bind ride a provider-owned factory.
- `Microsoft.Extensions.Diagnostics.Testing`(`tests/csharp/.api/diagnostics-testing.md`): `MetricCollector<T>` observes one instrument's stream over a test-scoped factory this mint builds.
- `Rasm.AppHost` `Observability/instruments#PROVIDER_LIFETIME`: one provider per plugin load context carries the mint, and unload disposes it after the final flush.
- Within-lib: one `AddMetrics(IServiceCollection, Action<IMetricsBuilder>)` call folds configuration binding, enable and disable rows, and listener registration onto one `IMetricsBuilder`, so composition binds the whole graph in one pass.

[LOCAL_ADMISSION]:
- A host or per-ALC capsule calls `AddMetrics`; a library takes `IMeterFactory` by injection and composes its rule rows through `IMetricsBuilder`.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Diagnostics`
- Owns: composition-root `IMeterFactory` mint with the instrument-enablement rule and listener grammar
- Accept: one `AddMetrics` per provider, `InstrumentRule` enable and disable rows, an `IMetricsListener` over published instruments
- Reject: a library-level `AddMetrics`, a process-static meter factory, a hand-wired `MeterListener` where a rule row fits
