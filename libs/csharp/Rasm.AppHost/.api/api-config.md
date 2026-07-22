# [RASM_APPHOST_API_CONFIG]

`Microsoft.Extensions.Configuration` owns the AppHost configuration tree: ordered `IConfigurationSource` inputs merge into one `IConfigurationRoot` of colon-delimited string keys with provider precedence by source order, and every reload propagates through a change token. Its boundary is bootstrap — sources mount and the root builds at composition, and runtime policy binds typed projections off the binder rail, never a raw section read.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Configuration`
- package: `Microsoft.Extensions.Configuration`
- assembly: `Microsoft.Extensions.Configuration`
- contract_assembly: `Microsoft.Extensions.Configuration.Abstractions`
- namespace: `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Configuration.Memory`
- rail: configuration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: configuration contracts

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :----------------------- | :------------ | :------------------------------ |
|  [01]   | `IConfiguration`         | interface     | read view over the merged tree  |
|  [02]   | `IConfigurationRoot`     | interface     | provider root with reload       |
|  [03]   | `IConfigurationSection`  | interface     | one path, value, and children   |
|  [04]   | `IConfigurationBuilder`  | interface     | ordered source composition      |
|  [05]   | `IConfigurationManager`  | interface     | builder and root in one surface |
|  [06]   | `IConfigurationProvider` | interface     | key/value provider contract     |
|  [07]   | `IConfigurationSource`   | interface     | provider factory                |

[PUBLIC_TYPE_SCOPE]: configuration implementations

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :---------------------------- | :------------ | :------------------------------ |
|  [01]   | `ConfigurationBuilder`        | class         | source list building a root     |
|  [02]   | `ConfigurationManager`        | class         | mutable builder-and-root        |
|  [03]   | `ConfigurationProvider`       | class         | key/value provider base         |
|  [04]   | `ConfigurationRoot`           | class         | merged provider tree            |
|  [05]   | `ConfigurationSection`        | class         | path and value view             |
|  [06]   | `ConfigurationReloadToken`    | class         | reload change-token signal      |
|  [07]   | `ConfigurationKeyComparer`    | class         | numeric-aware path segment sort |
|  [08]   | `ConfigurationPath`           | class         | key path split and combine      |
|  [09]   | `ConfigurationExtensions`     | class         | value and key projection        |
|  [10]   | `ConfigurationRootExtensions` | class         | root reload diagnostics         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: source composition, section access, and reload

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `IConfigurationBuilder.Add(IConfigurationSource)`            | instance | mounts a provider source            |
|  [02]   | `IConfigurationBuilder.AddConfiguration(IConfiguration)`     | static   | chains an existing tree             |
|  [03]   | `IConfigurationBuilder.AddInMemoryCollection(IEnumerable)`   | static   | mounts in-memory values             |
|  [04]   | `IConfigurationBuilder.Build() -> IConfigurationRoot`        | instance | merges providers into a root        |
|  [05]   | `IConfiguration.GetSection(string) -> IConfigurationSection` | instance | resolves a section path             |
|  [06]   | `IConfiguration.GetChildren() -> IEnumerable`                | instance | reads child sections                |
|  [07]   | `IConfiguration.GetReloadToken() -> IChangeToken`            | instance | hands out the reload token          |
|  [08]   | `IConfiguration.AsEnumerable() -> IEnumerable`               | static   | flattens to key/value pairs         |
|  [09]   | `IConfigurationRoot.Reload()`                                | instance | reloads every provider              |
|  [10]   | `ConfigurationReloadToken.OnReload()`                        | instance | fires the reload signal             |
|  [11]   | `IConfigurationSection.Exists() -> bool`                     | static   | true when value or children present |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `IConfigurationSource` builds one `IConfigurationProvider`, which answers `TryGet` and `GetChildKeys` reads and owns `Load`, `Set`, and `GetReloadToken` lifecycle.
- `IConfigurationRoot` merges the provider tree in source order, a later provider overriding an earlier key, and folds every provider reload into one change token.
- Keys are colon-delimited path segments ordered by `ConfigurationKeyComparer` across numeric and string segments.
- `ConfigurationManager` is builder and root at once, rebuilding live as a source mounts.
- `MemoryConfigurationSource` and `StreamConfigurationSource` carry in-memory and stream inputs; `AddConfiguration` mounts an existing tree as a chained source.

[STACKING]:
- `api-config-providers`(`.api/api-config-providers.md`): each provider package contributes one `IConfigurationSource` mounted through `IConfigurationBuilder.Add`, and `OptionsBuilder.BindConfiguration` threads this root's reload token onward.
- `api-binder`(`.api/api-binder.md`): `ConfigurationBinder.Get<T>`, `Bind`, and `GetValue<T>` consume an `IConfiguration` or `IConfigurationSection` from this surface and produce the typed policy record.
- `api-options`(`.api/api-options.md`): `OptionsBuilder.Bind(IConfiguration)` and `BindConfiguration(path)` project a section onto a named options value and feed `GetReloadToken` into `IOptionsMonitor`.
- within-lib: AppHost bootstrap folds ranked sources onto one `ConfigurationManager`, builds the root once, and binds typed projections into Runtime policy records so no runtime consumer reads a raw section.

[LOCAL_ADMISSION]:
- Configuration sources enter bootstrap composition as ordered inputs in explicit precedence.
- Runtime policy consumes typed projections, never a raw `IConfigurationSection`.
- A reload replaces policy only through an owned state transition, never an ambient re-read.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Configuration`
- Owns: the merged runtime configuration tree and its reload propagation
- Accept: ordered sources building one root, bound to typed policy records
- Reject: stringly local `IConfiguration.GetValue` lookups past bootstrap
