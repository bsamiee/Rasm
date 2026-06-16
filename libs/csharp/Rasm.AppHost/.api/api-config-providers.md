# [RASM_APPHOST_API_CONFIG_PROVIDERS]

`Microsoft.Extensions.Configuration.Json`, `EnvironmentVariables`, `CommandLine`, and
`UserSecrets` supply the four sibling configuration provider sources that mount JSON
files, process environment, argument vectors, and developer secret stores onto one
configuration builder chain; `Microsoft.Extensions.Options.ConfigurationExtensions`
binds a configuration section onto an `OptionsBuilder<T>` so options validate-on-start
re-binds from configuration without an interior `IConfiguration` read.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Configuration.Json`
- package: `Microsoft.Extensions.Configuration.Json`
- assembly: `Microsoft.Extensions.Configuration.Json`
- namespace: `Microsoft.Extensions.Configuration`
- namespace: `Microsoft.Extensions.Configuration.Json`
- asset: runtime library
- rail: configuration

[PACKAGE_SURFACE]: `Microsoft.Extensions.Configuration.EnvironmentVariables`
- package: `Microsoft.Extensions.Configuration.EnvironmentVariables`
- assembly: `Microsoft.Extensions.Configuration.EnvironmentVariables`
- namespace: `Microsoft.Extensions.Configuration`
- namespace: `Microsoft.Extensions.Configuration.EnvironmentVariables`
- asset: runtime library
- rail: configuration

[PACKAGE_SURFACE]: `Microsoft.Extensions.Configuration.CommandLine`
- package: `Microsoft.Extensions.Configuration.CommandLine`
- assembly: `Microsoft.Extensions.Configuration.CommandLine`
- namespace: `Microsoft.Extensions.Configuration`
- namespace: `Microsoft.Extensions.Configuration.CommandLine`
- asset: runtime library
- rail: configuration

[PACKAGE_SURFACE]: `Microsoft.Extensions.Configuration.UserSecrets`
- package: `Microsoft.Extensions.Configuration.UserSecrets`
- assembly: `Microsoft.Extensions.Configuration.UserSecrets`
- namespace: `Microsoft.Extensions.Configuration`
- namespace: `Microsoft.Extensions.Configuration.UserSecrets`
- asset: runtime library
- rail: configuration

[PACKAGE_SURFACE]: `Microsoft.Extensions.Options.ConfigurationExtensions`
- package: `Microsoft.Extensions.Options.ConfigurationExtensions`
- assembly: `Microsoft.Extensions.Options.ConfigurationExtensions`
- namespace: `Microsoft.Extensions.DependencyInjection`
- namespace: `Microsoft.Extensions.Options`
- asset: runtime library
- rail: options-binding
- admission: transitive of the hosting closure, central Transitive Floors pin, never a direct project asset

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: file and stream provider family
- rail: configuration

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]  | [CAPABILITY]               |
| :-----: | :-------------------------------- | :-------------- | :------------------------- |
|   [1]   | `JsonConfigurationSource`         | source value    | JSON file source policy    |
|   [2]   | `JsonConfigurationProvider`       | provider        | JSON file load and parse   |
|   [3]   | `JsonStreamConfigurationSource`   | source value    | JSON stream source policy  |
|   [4]   | `JsonStreamConfigurationProvider` | provider        | JSON stream load and parse |
|   [5]   | `JsonConfigurationExtensions`     | builder surface | JSON source registration   |

[PUBLIC_TYPE_SCOPE]: environment and argument provider family
- rail: configuration

| [INDEX] | [SYMBOL]                                    | [PACKAGE_ROLE]  | [CAPABILITY]                  |
| :-----: | :------------------------------------------ | :-------------- | :---------------------------- |
|   [1]   | `EnvironmentVariablesConfigurationSource`   | source value    | prefix-scoped variable source |
|   [2]   | `EnvironmentVariablesConfigurationProvider` | provider        | environment snapshot load     |
|   [3]   | `EnvironmentVariablesExtensions`            | builder surface | environment registration      |
|   [4]   | `CommandLineConfigurationSource`            | source value    | args and switch mappings      |
|   [5]   | `CommandLineConfigurationProvider`          | provider        | argument vector parse         |
|   [6]   | `CommandLineConfigurationExtensions`        | builder surface | command line registration     |

[PUBLIC_TYPE_SCOPE]: user secrets family
- rail: configuration

| [INDEX] | [SYMBOL]                             | [PACKAGE_ROLE]  | [CAPABILITY]                 |
| :-----: | :----------------------------------- | :-------------- | :--------------------------- |
|   [1]   | `UserSecretsConfigurationExtensions` | builder surface | secrets source registration  |
|   [2]   | `UserSecretsIdAttribute`             | assembly marker | secrets id declaration       |
|   [3]   | `PathHelper`                         | path projection | secrets file path resolution |

[PUBLIC_TYPE_SCOPE]: options-binding family
- rail: options-binding

| [INDEX] | [SYMBOL]                                       | [PACKAGE_ROLE]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------------- | :-------------- | :--------------------------------- |
|   [1]   | `OptionsBuilderConfigurationExtensions`        | builder surface | binds a section onto an options builder |
|   [2]   | `OptionsConfigurationServiceCollectionExtensions` | service surface | binds a section onto a service options registration |
|   [3]   | `ConfigurationChangeTokenSource<TOptions>`     | change token    | reloads bound options on section change |
|   [4]   | `NamedConfigureFromConfigurationOptions<TOptions>` | configure action | configures named options from a section |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: source registration
- rail: configuration

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                   | [CAPABILITY]                  |
| :-----: | :------------------------ | :--------------------------------------------- | :---------------------------- |
|   [1]   | `AddJsonFile`             | path, optional, reloadOnChange, file provider  | mounts JSON file source       |
|   [2]   | `AddJsonFile`             | `Action<JsonConfigurationSource>` configurator | mounts configured JSON source |
|   [3]   | `AddJsonStream`           | `Stream` payload                               | mounts JSON stream source     |
|   [4]   | `AddEnvironmentVariables` | no-arg, prefix, or source configurator         | mounts environment source     |
|   [5]   | `AddCommandLine`          | `string[]` args, optional switch mappings      | mounts argument source        |
|   [6]   | `AddCommandLine`          | `Action<CommandLineConfigurationSource>`       | mounts configured args source |
|   [7]   | `AddUserSecrets<T>`       | assembly marker type, optional, reloadOnChange | mounts secrets by marker type |
|   [8]   | `AddUserSecrets`          | `Assembly`, optional, reloadOnChange           | mounts secrets by assembly    |

[ENTRYPOINT_SCOPE]: source policy and resolution
- rail: configuration

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]           | [CAPABILITY]                       |
| :-----: | :---------------------------- | :--------------------- | :--------------------------------- |
|   [1]   | `Prefix`                      | source property        | strips environment variable prefix |
|   [2]   | `Args`                        | source property        | carries argument vector            |
|   [3]   | `SwitchMappings`              | source property        | maps short switches to keys        |
|   [4]   | `GetSecretsPathFromSecretsId` | static path projection | resolves secrets store path        |
|   [5]   | `Build`                       | source-to-provider     | constructs provider from source    |
|   [6]   | `Load`                        | provider stream load   | parses payload into key values     |

[ENTRYPOINT_SCOPE]: options binding
- rail: options-binding

| [INDEX] | [SURFACE]                                                          | [CALL_SHAPE]              | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------------- | :----------------------- | :--------------------------------- |
|   [1]   | `OptionsBuilder<T>.Bind(IConfiguration)`                         | builder bind             | binds a section onto the builder    |
|   [2]   | `OptionsBuilder<T>.Bind(IConfiguration, Action<BinderOptions>)`  | builder bind configured  | binds with binder-options policy    |
|   [3]   | `OptionsBuilder<T>.BindConfiguration(string, Action<BinderOptions>)` | builder bind by path  | binds by section path with reload token |
|   [4]   | `Configure<T>(IServiceCollection, IConfiguration)`              | service configure        | binds a section onto service options |
|   [5]   | `Configure<T>(IServiceCollection, string name, IConfiguration)` | named service configure  | binds a section onto named options  |

## [4]-[IMPLEMENTATION_LAW]

[PROVIDER_TOPOLOGY]:
- shared namespace: extension classes live in `Microsoft.Extensions.Configuration`
- source model: each package contributes one `IConfigurationSource` family per input axis
- JSON axis: file source over `FileConfigurationSource`, stream source over `StreamConfigurationSource`
- environment axis: prefix-filtered process environment snapshot
- argument axis: args vector with optional switch-mapping dictionary
- secrets axis: assembly-declared secrets id resolved to a JSON secrets store path
- options-binding axis: `OptionsBuilder<T>.BindConfiguration` mounts a section onto an options builder and registers a `ConfigurationChangeTokenSource<TOptions>` so a section change reloads the bound options through the monitor

[OPTIONS_BINDING_TOPOLOGY]:
- namespace: `Microsoft.Extensions.DependencyInjection` for the service and builder extensions, `Microsoft.Extensions.Options` for the change-token source
- builder root: `OptionsBuilder<T>.BindConfiguration(section, configureBinder)` is the validate-on-start-compatible bind that re-reads from configuration without an interior `IConfiguration` handle
- change model: `ConfigurationChangeTokenSource<TOptions>` ties the options monitor to the configuration reload token so a transition-class section flip re-binds without restart
- binder policy: the `Action<BinderOptions>` carries `ErrorOnUnknownConfiguration` so an unknown key fails closed at bind

[LOCAL_ADMISSION]:
- Provider sources mount at bootstrap composition in explicit precedence order.
- Environment and argument sources override file sources by source order, not by lookup code.
- User secrets are development-shape inputs; production composition omits the source.
- Switch mappings and prefixes are source policy values, not call-site string handling.

[RAIL_LAW]:
- Packages: `Microsoft.Extensions.Configuration.{Json,EnvironmentVariables,CommandLine,UserSecrets}`, `Microsoft.Extensions.Options.ConfigurationExtensions`
- Owns: configuration input sources per axis and the section-to-options-builder bind
- Accept: ordered source mounting on one builder chain; section bind onto an options builder with fail-closed binder policy
- Reject: per-call-site environment or argument parsing; interior `IConfiguration` reads past bootstrap
