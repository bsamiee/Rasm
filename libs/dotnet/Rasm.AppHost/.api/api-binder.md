# [RASM_APPHOST_API_BINDER]

`ConfigurationBinder` binds the merged `IConfiguration` tree onto typed AppHost policy records — scalars, object graphs, and constructor-bound records — through one reflection engine the package source generator supplants at every call site. Its boundary is bootstrap: a policy value materializes once and validates before runtime start, feeding the configuration surface every AppHost policy consumer reads.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the static bind surface and its policy object

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :-------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `ConfigurationBinder` | class         | static `Get`/`Bind`/`GetValue` extension face |
|  [02]   | `BinderOptions`       | class         | binding policy carried into every bind        |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: binding operations and the two policy knobs

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Get<T>(IConfiguration)`                              | static   | allocate and bind a typed instance        |
|  [02]   | `Get<T>(IConfiguration, Action<BinderOptions>)`       | static   | typed bind under a policy configurator    |
|  [03]   | `Get(IConfiguration, Type)`                           | static   | allocate and bind by runtime type         |
|  [04]   | `Bind(IConfiguration, object)`                        | static   | populate an existing instance             |
|  [05]   | `Bind(IConfiguration, object, Action<BinderOptions>)` | static   | bind existing under a policy configurator |
|  [06]   | `Bind(IConfiguration, string, object)`                | static   | bind a named section onto an instance     |
|  [07]   | `GetValue<T>(IConfiguration, string)`                 | static   | convert one scalar key                    |
|  [08]   | `GetValue<T>(IConfiguration, string, T)`              | static   | convert one scalar with a fallback        |
|  [09]   | `GetValue(IConfiguration, Type, string)`              | static   | convert one scalar by runtime type        |
|  [10]   | `BinderOptions.BindNonPublicProperties`               | property | widen binding to non-public setters       |
|  [11]   | `BinderOptions.ErrorOnUnknownConfiguration`           | property | throw on a key mapping to no member       |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Binding walks the colon-delimited `IConfiguration` tree onto scalars, nullables, enums, arrays, collections, dictionaries, object graphs, and constructor-bound records; an unmapped key leaves the target default unless `ErrorOnUnknownConfiguration` fails the bind.
- One result rides two engines: reflection binding is the default and the package source generator supplants it, `EnableConfigurationBindingGenerator` intercepting every `Get`/`Bind`/`GetValue` call site with trim- and AOT-safe generated code.

[STACKING]:
- `api-config`(`.api/api-config.md`): `ConfigurationBinder` extends `IConfiguration` and `IConfigurationSection`; every `Get`/`Bind`/`GetValue` reads the merged provider tree and materializes one typed policy value.
- `api-options`(`.api/api-options.md`): the binder is the engine under `OptionsBuilder.Bind(IConfiguration)` and `BindConfiguration(path)`, folding `ConfigurationBinder.Bind` onto the named policy value inside the options configure stage.
- within-lib: AppHost binds each Runtime policy record once at bootstrap through `Get<TPolicy>` with `ErrorOnUnknownConfiguration` closed and the binding generator on, so runtime code reads a validated typed instance and never a raw section.

[LOCAL_ADMISSION]:
- Policy records bind once at bootstrap and validate before runtime start.
- `ErrorOnUnknownConfiguration` fails a closed policy surface on any unmapped key.
- Non-public property binding admits only where a package-owned policy type sets `BindNonPublicProperties`.
