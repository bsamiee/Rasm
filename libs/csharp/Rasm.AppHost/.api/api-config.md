# [RASM_APPHOST_API_CONFIG]

`Microsoft.Extensions.Configuration` supplies hierarchical configuration roots, mutable configuration managers, sections, provider chains, in-memory sources, stream sources, key comparison, and reload tokens.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Configuration`

- package: `Microsoft.Extensions.Configuration`
- assembly: `Microsoft.Extensions.Configuration`
- contract_assembly: `Microsoft.Extensions.Configuration.Abstractions`
- namespace: `Microsoft.Extensions.Configuration`
- asset: runtime library
- rail: configuration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: configuration contracts

- rail: configuration

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [RAIL]                  |
| :-----: | :----------------------- | :---------------- | :---------------------- |
|  [01]   | `IConfiguration`         | read contract     | configuration tree read |
|  [02]   | `IConfigurationRoot`     | root contract     | provider root           |
|  [03]   | `IConfigurationSection`  | section contract  | section path/value      |
|  [04]   | `IConfigurationBuilder`  | builder contract  | source composition      |
|  [05]   | `IConfigurationManager`  | mutable contract  | builder plus root       |
|  [06]   | `IConfigurationProvider` | provider contract | key/value provider      |
|  [07]   | `IConfigurationSource`   | source contract   | provider factory        |

[PUBLIC_TYPE_SCOPE]: configuration implementation family

- rail: configuration

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]       | [RAIL]               |
| :-----: | :---------------------------- | :------------------ | :------------------- |
|  [01]   | `ConfigurationBuilder`        | builder surface     | source composition   |
|  [02]   | `ConfigurationManager`        | mutable root        | build and read root  |
|  [03]   | `ConfigurationProvider`       | provider base       | key/value provider   |
|  [04]   | `ConfigurationRoot`           | root implementation | provider merge       |
|  [05]   | `ConfigurationSection`        | section value       | path/value view      |
|  [06]   | `ConfigurationReloadToken`    | reload token        | reload signal        |
|  [07]   | `ConfigurationKeyComparer`    | key comparer        | path segment order   |
|  [08]   | `ConfigurationPath`           | path helper         | key/path operations  |
|  [09]   | `ConfigurationExtensions`     | read extensions     | value/key projection |
|  [10]   | `ConfigurationRootExtensions` | root extensions     | root diagnostics     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: configuration operations

- rail: configuration

| [INDEX] | [SURFACE]               | [CALL_SHAPE]        | [CAPABILITY]               |
| :-----: | :---------------------- | :------------------ | :------------------------- |
|  [01]   | `Add`                   | source registration | admits provider source     |
|  [02]   | `AddConfiguration`      | chained source      | chains configuration root  |
|  [03]   | `AddInMemoryCollection` | memory source       | admits memory values       |
|  [04]   | `Build`                 | root factory        | creates configuration root |
|  [05]   | `GetSection`            | section lookup      | resolves section path      |
|  [06]   | `GetChildren`           | child enumeration   | reads child sections       |
|  [07]   | `GetReloadToken`        | reload token lookup | resolves reload token      |
|  [08]   | `AsEnumerable`          | flattening iterator | projects key values        |
|  [09]   | `Reload`                | reload command      | refreshes configuration    |
|  [10]   | `OnReload`              | provider signal     | triggers reload token      |
|  [11]   | `Exists`                | section predicate   | tests section presence     |

[EXISTS_PREDICATE]:

- surface: `ConfigurationExtensions.Exists`
- truth: section exists with a value or children

## [04]-[IMPLEMENTATION_LAW]

[CONFIGURATION_TOPOLOGY]:

- namespaces: `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Configuration.Memory`
- source model: `IConfigurationSource` builds an `IConfigurationProvider`
- provider contract: `TryGet`, `Set`, `Load`, `GetChildKeys`, `GetReloadToken`
- root contract: merged provider tree with reload propagation
- key model: colon-delimited path segments with provider precedence by source order

[INPUT_TOPOLOGY]:

- chained source: `AddConfiguration` mounts an existing configuration tree
- memory source: `MemoryConfigurationSource` and `MemoryConfigurationProvider`
- stream source: `StreamConfigurationSource` and `StreamConfigurationProvider`
- mutable root: `ConfigurationManager` is both builder and root
- comparison rail: `ConfigurationKeyComparer` sorts numeric and string path segments

[LOCAL_ADMISSION]:

- Configuration sources enter bootstrap composition as ordered inputs.
- Runtime policy consumes typed projections, not raw configuration sections.
- Reload tokens trigger policy replacement only through owned state transitions.
- Mutable configuration is bootstrap material; runtime mutation enters through state transitions.

[RAIL_LAW]:

- Package: `Microsoft.Extensions.Configuration`
- Owns: runtime configuration trees
- Accept: configuration enters policy records
- Reject: stringly local lookup
