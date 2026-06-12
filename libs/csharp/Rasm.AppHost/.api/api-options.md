# [RASM_APPHOST_API_OPTIONS]

`Microsoft.Extensions.Options` supplies named options, configure and post-configure
pipelines, factories, caches, monitors, change tokens, validation hooks, and startup
policy validation.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Options`
- package: `Microsoft.Extensions.Options`
- assembly: `Microsoft.Extensions.Options`
- namespace: `Microsoft.Extensions.Options`
- asset: runtime library
- rail: options

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: options family
- rail: options

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]       | [RAIL]                 |
| :-----: | :------------------------------------ | :------------------ | :--------------------- |
|   [1]   | `IOptions<TOptions>`                  | singleton access    | default policy value   |
|   [2]   | `IOptionsSnapshot<TOptions>`          | scoped access       | scoped named policy    |
|   [3]   | `IOptionsMonitor<TOptions>`           | monitor access      | named live policy      |
|   [4]   | `IOptionsFactory<TOptions>`           | factory contract    | create policy value    |
|   [5]   | `IOptionsMonitorCache<TOptions>`      | monitor cache       | named value cache      |
|   [6]   | `IOptionsChangeTokenSource<TOptions>` | change token        | reload signal          |
|   [7]   | `IConfigureOptions<TOptions>`         | configure contract  | initial policy setup   |
|   [8]   | `IConfigureNamedOptions<TOptions>`    | named configure     | named policy setup     |
|   [9]   | `IPostConfigureOptions<TOptions>`     | post-configure      | derived policy setup   |
|  [10]   | `IValidateOptions<TOptions>`          | validation contract | policy validation      |
|  [11]   | `IStartupValidator`                   | startup validator   | startup proof          |
|  [12]   | `OptionsBuilder<TOptions>`            | options builder     | fluent policy pipeline |

[PUBLIC_TYPE_SCOPE]: implementation and validation family
- rail: options

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]          | [RAIL]                |
| :-----: | :--------------------------------- | :--------------------- | :-------------------- |
|   [1]   | `OptionsFactory<TOptions>`         | factory implementation | configure pipeline    |
|   [2]   | `OptionsManager<TOptions>`         | singleton manager      | default value access  |
|   [3]   | `OptionsMonitor<TOptions>`         | monitor implementation | live value access     |
|   [4]   | `OptionsCache<TOptions>`           | cache implementation   | named value cache     |
|   [5]   | `OptionsWrapper<TOptions>`         | fixed wrapper          | literal value wrapper |
|   [6]   | `Options`                          | static factory         | wrapper construction  |
|   [7]   | `ConfigureNamedOptions<T1..T6>`    | configure action       | dependency configure  |
|   [8]   | `PostConfigureOptions<T1..T6>`     | post-configure action  | dependency derivation |
|   [9]   | `ValidateOptions<T1..T6>`          | validation action      | dependency validation |
|  [10]   | `ValidateOptionsResult`            | validation result      | success/failure state |
|  [11]   | `ValidateOptionsResultBuilder`     | result builder         | accumulated failures  |
|  [12]   | `OptionsValidationException`       | validation exception   | startup failure       |
|  [13]   | `OptionsValidatorAttribute`        | generator attribute    | generated validator   |
|  [14]   | `ValidateObjectMembersAttribute`   | recursive validation   | member validation     |
|  [15]   | `ValidateEnumeratedItemsAttribute` | item validation        | collection validation |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: options operations
- rail: options

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]           | [RAIL]                  |
| :-----: | :------------------------------ | :----------------------- | :---------------------- |
|   [1]   | `AddOptions`                    | service registration     | admits options services |
|   [2]   | `AddOptions<TOptions>`          | builder registration     | typed policy builder    |
|   [3]   | `AddOptions(name)`              | named registration       | named policy builder    |
|   [4]   | `AddOptionsWithValidateOnStart` | startup registration     | startup validation      |
|   [5]   | `Configure`                     | configure action         | default policy setup    |
|   [6]   | `Configure(name)`               | named configure          | named policy setup      |
|   [7]   | `ConfigureAll`                  | wildcard configure       | all-names setup         |
|   [8]   | `PostConfigure`                 | post-configure action    | derived policy setup    |
|   [9]   | `PostConfigureAll`              | wildcard post-configure  | all-names derivation    |
|  [10]   | `ConfigureOptions`              | type/object registration | custom pipeline hook    |
|  [11]   | `Validate`                      | validation predicate     | policy validation       |
|  [12]   | `ValidateOnStart`               | startup hook             | eager validation        |
|  [13]   | `IStartupValidator.Validate`    | startup operation        | throws startup failures |
|  [14]   | `IOptionsMonitor.OnChange`      | change callback          | live policy transition  |
|  [15]   | `IOptionsMonitor.Get`           | named lookup             | current named value     |
|  [16]   | `IOptionsMonitorCache.GetOrAdd` | cache lookup             | cached named value      |
|  [17]   | `TryRemove`                     | cache invalidation       | named value refresh     |
|  [18]   | `Clear`                         | cache invalidation       | all value refresh       |

## [4]-[IMPLEMENTATION_LAW]

[OPTIONS_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Options`, `Microsoft.Extensions.DependencyInjection`
- access modes: singleton options, scoped snapshot, monitor with named lookup
- construction pipeline: configure, post-configure, validate, cache
- named policy: default name, explicit name, configure-all, post-configure-all
- cache policy: get-or-add, try-add, try-remove, clear
- validation contracts: `IValidateOptions<T>`, result builder, validation exception, startup validator
- generated validation: options validator attribute, object-member validation, enumerated-item validation
- generator assets: options source-generation analyzer assets

[LOCAL_ADMISSION]:
- Policy values enter AppHost through named options where variants are bounded.
- Startup validation is mandatory for runtime-critical policy records.
- Cache invalidation becomes a typed runtime transition.
- Configure and post-configure actions stay at composition boundaries.
- Options monitor changes become explicit state transitions, not mutable ambient settings.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Options`
- Owns: runtime policy values
- Accept: options are validated inputs
- Reject: mutable global settings
