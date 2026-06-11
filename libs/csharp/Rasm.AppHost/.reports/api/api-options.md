# [RASM_APPHOST_API_OPTIONS]

`Microsoft.Extensions.Options` supplies named options, monitor state, validation hooks, and startup policy validation.

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

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]  | [CAPABILITY]          |
| :-----: | :-------------------------------- | :-------------- | :-------------------- |
|   [1]   | `IOptions<TOptions>`              | policy object   | carries policy input  |
|   [2]   | `IOptionsSnapshot<TOptions>`      | policy object   | carries policy input  |
|   [3]   | `IOptionsMonitor<TOptions>`       | policy object   | carries policy input  |
|   [4]   | `OptionsBuilder<TOptions>`        | options builder | builds policy binding |
|   [5]   | `IConfigureOptions<TOptions>`     | policy object   | carries policy input  |
|   [6]   | `IPostConfigureOptions<TOptions>` | policy object   | carries policy input  |
|   [7]   | `IValidateOptions<TOptions>`      | policy object   | carries policy input  |
|   [8]   | `ValidateOptionsResult`           | policy object   | carries policy input  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: options operations
- rail: options

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]           | [CAPABILITY]              |
| :-----: | :------------------------------ | :--------------------- | :------------------------ |
|   [1]   | `AddOptions<TOptions>`          | DI extension           | admits configured surface |
|   [2]   | `Configure`                     | configuration delegate | applies policy value      |
|   [3]   | `PostConfigure`                 | configuration delegate | applies policy value      |
|   [4]   | `Validate`                      | validation method      | returns validation result |
|   [5]   | `ValidateOnStart`               | startup validator      | fails invalid startup     |
|   [6]   | `OnChange`                      | change callback        | observes option changes   |
|   [7]   | `Get`                           | lookup call            | resolves typed value      |
|   [8]   | `AddOptionsWithValidateOnStart` | DI extension           | admits validated options  |

## [4]-[IMPLEMENTATION_LAW]

[OPTIONS_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Options`, `Microsoft.Extensions.DependencyInjection`
- access modes: singleton options, scoped snapshot, monitor with named lookup
- construction pipeline: configure, post-configure, validate, cache
- validation contracts: `IValidateOptions<T>`, `ValidateOptionsResult`, startup validator
- generator assets: options source-generation analyzer assets

[LOCAL_ADMISSION]:
- Policy values enter AppHost through named options where variants are bounded.
- Startup validation is mandatory for runtime-critical policy records.
- Options monitor changes become explicit state transitions, not mutable ambient settings.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Options`
- Owns: runtime policy values
- Accept: options are validated inputs
- Reject: mutable global settings

