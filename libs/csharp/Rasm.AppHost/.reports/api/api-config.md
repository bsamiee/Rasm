# [RASM_APPHOST_API_CONFIG]

`Microsoft.Extensions.Configuration` supplies hierarchical configuration roots, sections, providers, and change tokens.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Configuration`
- package: `Microsoft.Extensions.Configuration`
- assembly: `Microsoft.Extensions.Configuration`
- namespace: `Microsoft.Extensions.Configuration`
- asset: runtime library
- rail: configuration

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: configuration family
- rail: configuration

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]   | [CAPABILITY]                   |
| :-----: | :---------------------- | :--------------- | :----------------------------- |
|   [1]   | `IConfiguration`        | contract surface | defines boundary contract      |
|   [2]   | `IConfigurationRoot`    | contract surface | defines boundary contract      |
|   [3]   | `IConfigurationSection` | contract surface | defines boundary contract      |
|   [4]   | `IConfigurationBuilder` | builder surface  | constructs configured root     |
|   [5]   | `ConfigurationBuilder`  | builder surface  | constructs configured root     |
|   [6]   | `ConfigurationProvider` | provider base    | anchors configuration contract |
|   [7]   | `IConfigurationSource`  | contract surface | defines boundary contract      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: configuration operations
- rail: configuration

| [INDEX] | [SURFACE]        | [CALL_SHAPE]        | [CAPABILITY]              |
| :-----: | :--------------- | :------------------ | :------------------------ |
|   [1]   | `Add`            | DI extension        | admits configured surface |
|   [2]   | `Build`          | factory call        | creates configured handle |
|   [3]   | `GetSection`     | lookup call         | resolves typed value      |
|   [4]   | `GetChildren`    | lookup call         | resolves typed value      |
|   [5]   | `GetReloadToken` | lookup call         | resolves typed value      |
|   [6]   | `AsEnumerable`   | flattening iterator | projects key values       |
|   [7]   | `Reload`         | reload command      | refreshes configuration   |

## [4]-[IMPLEMENTATION_LAW]

[CONFIGURATION_TOPOLOGY]:
- namespaces: `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Configuration.Memory`
- source model: `IConfigurationSource` builds an `IConfigurationProvider`
- provider contract: `TryGet`, `Set`, `Load`, `GetChildKeys`, `GetReloadToken`
- root contract: merged provider tree with reload propagation
- key model: colon-delimited path segments with provider precedence by source order

[LOCAL_ADMISSION]:
- Configuration sources enter bootstrap composition as ordered inputs.
- Runtime policy consumes typed projections, not raw configuration sections.
- Reload tokens trigger policy replacement only through owned state transitions.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.Configuration`
- Owns: runtime configuration trees
- Accept: configuration enters policy records
- Reject: stringly local lookup
