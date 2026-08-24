# [RASM_APPHOST_API_CONFIG_PROVIDERS]

These configuration-provider packages each mint one `IConfigurationSource` family per input axis and register it onto one `IConfigurationBuilder` chain; `Microsoft.Extensions.Options.ConfigurationExtensions` binds a configuration section onto an `OptionsBuilder<T>`, so validate-on-start options re-bind from configuration with no interior `IConfiguration` read. Bootstrap composition bounds the surface: sources mount in explicit precedence order on the builder and reach runtime as bound, monitored policy values.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.Extensions.Configuration.Json`
- package: `Microsoft.Extensions.Configuration.Json`
- assembly: `Microsoft.Extensions.Configuration.Json`
- namespace: `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Configuration.Json`
- rail: configuration

[PACKAGE_SURFACE]: `Microsoft.Extensions.Configuration.EnvironmentVariables`
- package: `Microsoft.Extensions.Configuration.EnvironmentVariables`
- assembly: `Microsoft.Extensions.Configuration.EnvironmentVariables`
- namespace: `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Configuration.EnvironmentVariables`
- rail: configuration

[PACKAGE_SURFACE]: `Microsoft.Extensions.Configuration.CommandLine`
- package: `Microsoft.Extensions.Configuration.CommandLine`
- assembly: `Microsoft.Extensions.Configuration.CommandLine`
- namespace: `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Configuration.CommandLine`
- rail: configuration

[PACKAGE_SURFACE]: `Microsoft.Extensions.Configuration.UserSecrets`
- package: `Microsoft.Extensions.Configuration.UserSecrets`
- assembly: `Microsoft.Extensions.Configuration.UserSecrets`
- namespace: `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Configuration.UserSecrets`
- rail: configuration

[PACKAGE_SURFACE]: `Microsoft.Extensions.Options.ConfigurationExtensions`
- package: `Microsoft.Extensions.Options.ConfigurationExtensions`
- assembly: `Microsoft.Extensions.Options.ConfigurationExtensions`
- namespace: `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Options`
- rail: options-binding

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: file and stream provider family

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]               |
| :-----: | :-------------------------------- | :------------ | :------------------------- |
|  [01]   | `JsonConfigurationSource`         | class         | JSON file source policy    |
|  [02]   | `JsonConfigurationProvider`       | class         | JSON file load and parse   |
|  [03]   | `JsonStreamConfigurationSource`   | class         | JSON stream source policy  |
|  [04]   | `JsonStreamConfigurationProvider` | class         | JSON stream load and parse |
|  [05]   | `JsonConfigurationExtensions`     | class         | JSON source registration   |

[PUBLIC_TYPE_SCOPE]: environment and argument provider family

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :------------------------------------------ | :------------ | :---------------------------- |
|  [01]   | `EnvironmentVariablesConfigurationSource`   | class         | prefix-scoped variable source |
|  [02]   | `EnvironmentVariablesConfigurationProvider` | class         | environment snapshot load     |
|  [03]   | `EnvironmentVariablesExtensions`            | class         | environment registration      |
|  [04]   | `CommandLineConfigurationSource`            | class         | args and switch mappings      |
|  [05]   | `CommandLineConfigurationProvider`          | class         | argument vector parse         |
|  [06]   | `CommandLineConfigurationExtensions`        | class         | command line registration     |

[PUBLIC_TYPE_SCOPE]: user secrets family

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :----------------------------------- | :------------ | :--------------------------- |
|  [01]   | `UserSecretsConfigurationExtensions` | class         | secrets source registration  |
|  [02]   | `UserSecretsIdAttribute`             | class         | secrets id declaration       |
|  [03]   | `PathHelper`                         | class         | secrets file path resolution |

[PUBLIC_TYPE_SCOPE]: options-binding family

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :------------------------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `OptionsBuilderConfigurationExtensions`            | class         | binds a section onto an options builder |
|  [02]   | `OptionsConfigurationServiceCollectionExtensions`  | class         | binds a section onto service options    |
|  [03]   | `ConfigurationChangeTokenSource<TOptions>`         | class         | reloads bound options on section change |
|  [04]   | `NamedConfigureFromConfigurationOptions<TOptions>` | class         | configures named options from a section |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: source registration onto the builder chain

| [INDEX] | [SURFACE]                                                | [SHAPE] | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `AddJsonFile(string, bool, bool)`                        | static  | mounts a JSON file source           |
|  [02]   | `AddJsonFile(Action<JsonConfigurationSource>)`           | static  | mounts a configured JSON source     |
|  [03]   | `AddJsonStream(Stream)`                                  | static  | mounts a JSON stream source         |
|  [04]   | `AddEnvironmentVariables(string)`                        | static  | mounts a prefix-scoped env source   |
|  [05]   | `AddEnvironmentVariables(Action<...Source>)`             | static  | mounts a configured env source      |
|  [06]   | `AddCommandLine(string[], IDictionary<string,string>)`   | static  | mounts an args source with mappings |
|  [07]   | `AddCommandLine(Action<CommandLineConfigurationSource>)` | static  | mounts a configured args source     |
|  [08]   | `AddUserSecrets<T>(bool, bool)`                          | static  | mounts secrets by marker type       |
|  [09]   | `AddUserSecrets(Assembly, bool, bool)`                   | static  | mounts secrets by assembly          |

[ENTRYPOINT_SCOPE]: source policy set through the configurator

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :----------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `EnvironmentVariablesConfigurationSource.Prefix` | property | strips the env variable prefix  |
|  [02]   | `CommandLineConfigurationSource.Args`            | property | carries the argument vector     |
|  [03]   | `CommandLineConfigurationSource.SwitchMappings`  | property | maps short switches to keys     |
|  [04]   | `PathHelper.GetSecretsPathFromSecretsId(string)` | static   | resolves the secrets store path |

[ENTRYPOINT_SCOPE]: section-to-options binding

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `OptionsBuilder<T>.Bind(IConfiguration)`                             | instance | binds a section onto the builder        |
|  [02]   | `OptionsBuilder<T>.Bind(IConfiguration, Action<BinderOptions>)`      | instance | binds with binder-options policy        |
|  [03]   | `OptionsBuilder<T>.BindConfiguration(string, Action<BinderOptions>)` | instance | binds by section path with reload token |
|  [04]   | `Configure<T>(IServiceCollection, IConfiguration)`                   | static   | binds a section onto service options    |
|  [05]   | `Configure<T>(IServiceCollection, string, IConfiguration)`           | static   | binds a section onto named options      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each package mints one `IConfigurationSource` family per input axis; the source builds its `IConfigurationProvider`, and provider precedence follows source-registration order on the builder chain.
- Configuration extension classes share `Microsoft.Extensions.Configuration`; the options-binding extensions live in `Microsoft.Extensions.DependencyInjection` and the change-token source in `Microsoft.Extensions.Options`.
- `OptionsBuilder<T>.BindConfiguration` is the validate-on-start-compatible bind: it re-reads from configuration with no interior `IConfiguration` handle and registers a `ConfigurationChangeTokenSource<TOptions>`, so a section change reloads the bound value through the monitor.

[STACKING]:
- `api-config`(`.api/api-config.md`): every `Add*` extension registers one `IConfigurationSource` on the `IConfigurationBuilder`, and the built `IConfigurationRoot` merges providers by source order with reload-token propagation.
- `api-binder`(`.api/api-binder.md`): `Bind`, `BindConfiguration`, and `Configure<T>` drive `ConfigurationBinder`, and the `Action<BinderOptions>` carries `ErrorOnUnknownConfiguration` fail-closed policy.
- `api-options`(`.api/api-options.md`): `BindConfiguration` feeds the options pipeline, and its `ConfigurationChangeTokenSource<TOptions>` drives `IOptionsMonitor` reloads.
- `api-di`(`.api/api-di.md`): `Configure<T>(IServiceCollection, IConfiguration)` and `OptionsConfigurationServiceCollectionExtensions` register `IConfigureOptions` and `IOptionsChangeTokenSource` descriptors on the service collection.
- `api-hosting`(`.api/api-hosting.md`): the Generic Host default builder mounts the JSON, environment, args, and secrets sources in fixed precedence before app configuration.
- within-lib: `ConfigSource.Compose` (Runtime/config.md) folds the ranked source chain onto one builder and binds each Runtime policy record through `BindConfiguration` validated on start.

[LOCAL_ADMISSION]:
- Provider sources mount at bootstrap composition in explicit precedence order.
- Environment and argument sources override file sources by source order, never by lookup code.
- User secrets are development-shape inputs; production composition omits the source.
- Switch mappings and prefixes are source-policy values, never call-site string handling.

[RAIL_LAW]:
- Packages: `Microsoft.Extensions.Configuration.{Json,EnvironmentVariables,CommandLine,UserSecrets}`, `Microsoft.Extensions.Options.ConfigurationExtensions`
- Owns: configuration input sources per axis and the section-to-options-builder bind
- Accept: ordered source mounting on one builder chain; a section bound onto an options builder with fail-closed binder policy
- Reject: per-call-site environment or argument parsing; interior `IConfiguration` reads past bootstrap
