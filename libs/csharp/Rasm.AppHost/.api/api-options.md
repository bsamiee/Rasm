# [RASM_APPHOST_API_OPTIONS]

`Microsoft.Extensions.Options` binds every runtime policy record to a named, validated value resolved through DI, folding configure, post-configure, and validation stages behind one `OptionsBuilder` pipeline that runs once per `(type, name)` and caches. Its boundary is composition: policy enters as bounded named options at the composition root and reaches runtime as an immutable typed value or an explicit `IOptionsMonitor` change transition, feeding the options rail every AppHost policy consumer binds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Options`
- package: `Microsoft.Extensions.Options`
- assembly: `Microsoft.Extensions.Options`
- namespace: `Microsoft.Extensions.Options`, `Microsoft.Extensions.DependencyInjection`
- rail: options

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: contracts, access, and the builder

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------------------------------ | :------------ | :----------------------------------- |
|  [01]   | `IOptions<TOptions>`                  | interface     | eager singleton value                |
|  [02]   | `IOptionsSnapshot<TOptions>`          | interface     | per-scope recomputed named value     |
|  [03]   | `IOptionsMonitor<TOptions>`           | interface     | live value with change notification  |
|  [04]   | `IOptionsFactory<TOptions>`           | interface     | runs the configure/validate pipeline |
|  [05]   | `IOptionsMonitorCache<TOptions>`      | interface     | named cache behind the monitor       |
|  [06]   | `IOptionsChangeTokenSource<TOptions>` | interface     | reload trigger feeding the monitor   |
|  [07]   | `IConfigureOptions<TOptions>`         | interface     | default-name configure stage         |
|  [08]   | `IConfigureNamedOptions<TOptions>`    | interface     | per-name configure stage             |
|  [09]   | `IPostConfigureOptions<TOptions>`     | interface     | post-configure stage                 |
|  [10]   | `IValidateOptions<TOptions>`          | interface     | pipeline validation stage            |
|  [11]   | `IStartupValidator`                   | interface     | eager validation at host start       |
|  [12]   | `OptionsBuilder<TOptions>`            | class         | fluent configure/validate pipeline   |

[PUBLIC_TYPE_SCOPE]: implementations and validation

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :--------------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `OptionsFactory<TOptions>`         | class         | default pipeline runner                      |
|  [02]   | `OptionsManager<TOptions>`         | class         | `IOptions`/`IOptionsSnapshot` implementation |
|  [03]   | `OptionsMonitor<TOptions>`         | class         | `IOptionsMonitor` implementation             |
|  [04]   | `OptionsCache<TOptions>`           | class         | thread-safe named value cache                |
|  [05]   | `OptionsWrapper<TOptions>`         | class         | wraps a literal value                        |
|  [06]   | `Options`                          | class         | static name and wrapper helpers              |
|  [07]   | `ConfigureNamedOptions<T1..T6>`    | class         | configure with injected dependencies         |
|  [08]   | `PostConfigureOptions<T1..T6>`     | class         | post-configure with injected dependencies    |
|  [09]   | `ValidateOptions<T1..T6>`          | class         | validate with injected dependencies          |
|  [10]   | `ValidateOptionsResult`            | class         | pass/fail/skip outcome                       |
|  [11]   | `ValidateOptionsResultBuilder`     | class         | accumulates failures across validators       |
|  [12]   | `OptionsValidationException`       | class         | thrown on failed validation                  |
|  [13]   | `OptionsValidatorAttribute`        | class         | source-generates an `IValidateOptions`       |
|  [14]   | `ValidateObjectMembersAttribute`   | class         | recurses validation into a member            |
|  [15]   | `ValidateEnumeratedItemsAttribute` | class         | validates each collection item               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration and access operations

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------ | :------- | :----------------------------------------- |
|  [01]   | `AddOptions()`                                          | static   | registers options infrastructure           |
|  [02]   | `AddOptions<TOptions>() -> OptionsBuilder`              | static   | opens the typed pipeline builder           |
|  [03]   | `AddOptions<TOptions>(string) -> OptionsBuilder`        | static   | opens a named pipeline builder             |
|  [04]   | `AddOptionsWithValidateOnStart<TOptions>()`             | static   | builder validated at host start            |
|  [05]   | `Configure<TOptions>(Action<TOptions>)`                 | static   | default-name configure stage               |
|  [06]   | `Configure<TOptions>(string, Action<TOptions>)`         | static   | named configure stage                      |
|  [07]   | `ConfigureAll<TOptions>(Action<TOptions>)`              | static   | configure every name                       |
|  [08]   | `PostConfigure<TOptions>(Action<TOptions>)`             | static   | post-configure stage                       |
|  [09]   | `PostConfigureAll<TOptions>(Action<TOptions>)`          | static   | post-configure every name                  |
|  [10]   | `ConfigureOptions(Type)`                                | static   | registers a custom pipeline implementation |
|  [11]   | `OptionsBuilder.Validate(Func<TOptions,bool>)`          | instance | inline validation predicate                |
|  [12]   | `OptionsBuilder.ValidateOnStart()`                      | instance | promotes validation to host start          |
|  [13]   | `IStartupValidator.Validate()`                          | instance | runs every startup validator               |
|  [14]   | `IOptionsMonitor.OnChange(Action) -> IDisposable`       | instance | subscribes to value reloads                |
|  [15]   | `IOptionsMonitor.Get(string) -> TOptions`               | instance | resolves current named value               |
|  [16]   | `IOptionsMonitorCache.GetOrAdd(string, Func)`           | instance | caches or computes named value             |
|  [17]   | `IOptionsMonitorCache.TryAdd(string, TOptions) -> bool` | instance | seeds a named value if absent              |
|  [18]   | `IOptionsMonitorCache.TryRemove(string) -> bool`        | instance | evicts one named value                     |
|  [19]   | `IOptionsMonitorCache.Clear()`                          | instance | evicts every cached value                  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every value is keyed by a name; the default name is the empty string, and one `(type, name)` pair runs the configure → post-configure → validate pipeline once, then caches.
- Access scope is the contract: `IOptions` computes one singleton, `IOptionsSnapshot` recomputes per DI scope, and `IOptionsMonitor` recomputes on a change-token signal, serving the cache between signals.
- Validation is a pipeline stage deferred to first access, promoted to host start by `ValidateOnStart`; a failed `IValidateOptions` surfaces as `OptionsValidationException`.
- `OptionsValidatorAttribute` source-generates the `IValidateOptions` implementation, `ValidateObjectMembersAttribute` recursing into a member and `ValidateEnumeratedItemsAttribute` into each collection item.

[STACKING]:
- `api-di`(`.api/api-di.md`): every `AddOptions`/`Configure`/`PostConfigure` surface is an `IServiceCollection` extension registering `IConfigureOptions`/`IPostConfigureOptions`/`IValidateOptions` descriptors, and `IOptions`/`IOptionsSnapshot`/`IOptionsMonitor` resolve as singleton, scoped, and singleton services.
- `api-config`(`.api/api-config.md`) + `api-config-providers`(`.api/api-config-providers.md`): `OptionsBuilder.Bind(IConfiguration)` and `BindConfiguration(path)` project a config section onto the value and thread the provider reload token into `IOptionsMonitor`.
- `api-validation`(`.api/api-validation.md`) + `api-validation-di`(`.api/api-validation-di.md`): a FluentValidation `IValidator<TOptions>` runs inside the validation stage as an `IValidateOptions<TOptions>`, the source-generated `OptionsValidatorAttribute` being the reflection-free alternative on the same stage.
- within-lib: AppHost folds each Runtime policy record through one named `OptionsBuilder` bound from the ranked config chain and validated on start, so a runtime consumer reads an immutable `IOptions<TPolicy>` and every reload arrives as an explicit `IOptionsMonitor.OnChange` transition.

[LOCAL_ADMISSION]:
- Policy enters AppHost only as a bounded named `OptionsBuilder`; an open-ended setting is a typed record, never an options name.
- Runtime-critical policy binds `ValidateOnStart`; deferred first-access validation is rejected.
- A reload is an explicit `IOptionsMonitor.OnChange` transition; ambient re-read of a mutable value is rejected.
- Configure and post-configure actions run at composition roots.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Options`
- Owns: named, validated runtime policy values
- Accept: policy bound from the config chain and validated at host start
- Reject: static config singletons and scattered `IConfiguration.GetValue` reads
