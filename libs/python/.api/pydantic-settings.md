# [PY_RUNTIME_API_PYDANTIC_SETTINGS]

`pydantic-settings` supplies layered settings admission: a `BaseSettings` base, a customisable source priority chain (init, env, dotenv, TOML/YAML/JSON, secrets dirs, cloud secret managers), and a CLI source. It is the runtime owner for caller-provided configuration admitted as one validated settings model.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pydantic-settings`
- package: `pydantic-settings`
- import: `pydantic_settings`
- owner: `runtime`
- rail: validation
- namespaces: `pydantic_settings`, `pydantic_settings.sources`, `pydantic_settings.sources.providers`
- installed: `2.14.2`
- capability: layered settings model, source-priority customisation, env/dotenv/file/secret/nested-secret sources, cloud secret managers, a full CLI source with argparse-grade flag/subcommand/positional annotations and a `CliApp` runner

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: settings base family
- rail: validation

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :--------------------------- | :------------ | :------------------------------- |
|  [01]   | `BaseSettings`               | settings base | layered validated settings model |
|  [02]   | `SettingsConfigDict`         | config        | settings behavior/source knobs   |
|  [03]   | `PydanticBaseSettingsSource` | source base   | custom settings source contract  |
|  [04]   | `CliApp`                     | CLI runner    | settings-backed CLI entry        |

[PUBLIC_TYPE_SCOPE]: source family
- rail: validation

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :---------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `InitSettingsSource`                | source        | constructor-kwarg layer                                 |
|  [02]   | `EnvSettingsSource`                 | source        | environment-variable layer                              |
|  [03]   | `DotEnvSettingsSource`              | source        | `.env` file layer                                       |
|  [04]   | `SecretsSettingsSource`             | source        | secrets-directory layer (flat files)                    |
|  [05]   | `NestedSecretsSettingsSource`       | source        | nested secrets-directory layer (subdir-per-model)       |
|  [06]   | `TomlConfigSettingsSource`          | source        | TOML file layer (`toml` extra)                          |
|  [07]   | `YamlConfigSettingsSource`          | source        | YAML file layer (`yaml` extra)                          |
|  [08]   | `JsonConfigSettingsSource`          | source        | JSON file layer                                         |
|  [09]   | `PyprojectTomlConfigSettingsSource` | source        | `pyproject.toml` table layer                            |
|  [10]   | `CliSettingsSource`                 | source        | command-line layer (argparse-backed)                    |
|  [11]   | `AWSSecretsManagerSettingsSource`   | source        | AWS Secrets Manager layer (`aws-secrets-manager` extra) |
|  [12]   | `AzureKeyVaultSettingsSource`       | source        | Azure Key Vault layer (`azure-key-vault` extra)         |
|  [13]   | `GoogleSecretManagerSettingsSource` | source        | GCP Secret Manager layer (`gcp-secret-manager` extra)   |

[PUBLIC_TYPE_SCOPE]: CLI annotation and fault family
- rail: validation

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [RAIL]                                      |
| :-----: | :----------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `CliSubCommand`                | CLI annotation | subcommand field marker                     |
|  [02]   | `CliPositionalArg`             | CLI annotation | positional argument marker                  |
|  [03]   | `CliImplicitFlag`              | CLI annotation | implicit boolean flag (`--x`/`--no-x` pair) |
|  [04]   | `CliExplicitFlag`              | CLI annotation | explicit boolean flag (value required)      |
|  [05]   | `CliToggleFlag`                | CLI annotation | single-toggle boolean flag                  |
|  [06]   | `CliDualFlag`                  | CLI annotation | dual positive/negative flag pair            |
|  [07]   | `CliMutuallyExclusiveGroup`    | CLI annotation | exclusive-option group                      |
|  [08]   | `CliSuppress` / `CLI_SUPPRESS` | CLI annotation | hide a field from CLI help/parsing          |
|  [09]   | `CliUnknownArgs`               | CLI annotation | capture unparsed trailing args              |
|  [10]   | `NoDecode`                     | decode marker  | suppress complex decode                     |
|  [11]   | `ForceDecode`                  | decode marker  | force complex decode                        |
|  [12]   | `SettingsError`                | fault          | settings-load failure                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: settings construction and source order
- rail: validation
- `BaseSettings()` runs the full source chain at instantiation; the per-instance override kwargs let a caller redirect a source without subclassing.

| [INDEX] | [SURFACE]                                                                                                                                                     | [ENTRY_FAMILY] | [RAIL]                                                   |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------- | :------------------------------------------------------- |
|  [01]   | `BaseSettings(_env_file=, _env_prefix=, _env_nested_delimiter=, _secrets_dir=, _cli_parse_args=, **values)`                                                   | construct      | instantiate + run the resolved source chain              |
|  [02]   | `settings_customise_sources(cls, settings_cls, init_settings, env_settings, dotenv_settings, file_secret_settings) -> tuple[PydanticBaseSettingsSource, ...]` | source order   | classmethod returning the highest-to-lowest source tuple |
|  [03]   | `PydanticBaseSettingsSource.__call__() -> dict[str, Any]` / `get_field_value(field, field_name)`                                                              | source impl    | a custom source returns its field mapping                |

[ENTRYPOINT_SCOPE]: settings config knobs (`SettingsConfigDict`)
- rail: validation
- the `model_config = SettingsConfigDict(...)` that the source chain reads; one config row per behavior, never scattered constructor args.

| [INDEX] | [KNOB]                                                                                                                      | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `env_prefix` / `env_prefix_target` / `case_sensitive`                                                                       | env binding    | environment variable name mapping          |
|  [02]   | `env_nested_delimiter` / `env_parse_none_str` / `env_parse_enums`                                                           | env nesting    | nested-model env key splitting and parsing |
|  [03]   | `env_file` / `env_file_encoding`                                                                                            | dotenv         | `.env` file path(s) and encoding           |
|  [04]   | `secrets_dir`                                                                                                               | secrets        | secrets directory path(s)                  |
|  [05]   | `toml_file` / `yaml_file` / `json_file` (+ `_encoding`)                                                                     | file source    | declarative file-source paths              |
|  [06]   | `cli_parse_args` / `cli_prog_name` / `cli_kebab_case` / `cli_implicit_flags` / `cli_enforce_required` / `cli_exit_on_error` | cli            | CLI parsing behavior                       |
|  [07]   | `extra` / `nested_model_default_partial_update`                                                                             | merge policy   | unknown-key + nested-merge behavior        |

[ENTRYPOINT_SCOPE]: CLI runner
- rail: validation

| [INDEX] | [SURFACE]                                                                                                                                       | [ENTRY_FAMILY] | [RAIL]                                                                                                         |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `CliApp.run(model_cls, cli_args=None, cli_settings_source=None, cli_exit_on_error=None, cli_cmd_method_name='cli_cmd', **model_init_data) -> T` | CLI run        | build the model from CLI args and invoke its `cli_cmd` (awaits if async, thread-isolated under a running loop) |
|  [02]   | `CliApp.run_subcommand(model, cli_exit_on_error=None, cli_cmd_method_name='cli_cmd') -> PydanticModel`                                          | CLI dispatch   | run the selected subcommand on an already-built model                                                          |
|  [03]   | `get_subcommand(model, is_required=True, cli_exit_on_error=True)`                                                                               | CLI            | extract the selected `CliSubCommand` instance                                                                  |

## [04]-[IMPLEMENTATION_LAW]

[SETTINGS_TOPOLOGY]:
- settings law: caller configuration is one `BaseSettings` subclass; the source priority chain is declared once in `settings_customise_sources`, which returns a `tuple[PydanticBaseSettingsSource, ...]` ordered highest-to-lowest precedence (earlier tuple element wins), never resolved with scattered `os.environ` reads.
- source law: every configuration origin is a source in the priority tuple — init kwargs, env, dotenv, the chosen file format, secrets dir, cloud secret manager, CLI; a new origin is one additional source class implementing `PydanticBaseSettingsSource.__call__`, never a parallel loader. The default order is `init_settings` > `env_settings` > `dotenv_settings` > `file_secret_settings`; reorder by returning a permuted tuple.
- nested law: structured settings nest validated models with `env_nested_delimiter` splitting flat keys into the model tree; complex env values use the `NoDecode`/`ForceDecode` markers (and `nested_model_default_partial_update` for partial overrides), never manual JSON parsing. `SecretsSettingsSource` reads flat per-key files; `NestedSecretsSettingsSource` reads a subdirectory-per-model tree (Docker/K8s secret mounts).
- cli law: the CLI source is `argparse`-backed; flag arity is declared by the annotation family (`CliImplicitFlag`/`CliExplicitFlag`/`CliToggleFlag`/`CliDualFlag`), subcommands by `CliSubCommand` + `get_subcommand`, positionals by `CliPositionalArg`, hidden fields by `CliSuppress`/`CLI_SUPPRESS`, exclusivity by `CliMutuallyExclusiveGroup`. `CliApp.run` is the entry that builds the model and dispatches to a `cli_cmd` method, awaiting async commands.
- extras law: TOML/YAML/cloud sources lazily import their backing dependency (`tomli`/`pyyaml`/`boto3`/`azure-*`/`google-cloud-secret-manager`) and raise `ImportError`/`SettingsError` at construction when the extra is absent; admit the matching extra in the owning manifest before placing that source on the chain.
- boundary law: a settings-load failure surfaces as `SettingsError`/`ValidationError` lifted into `Error(BoundaryFault(...))` at admission; the runtime owns no global settings singleton.

[LOCAL_ADMISSION]:
- The context-admission surface receives the validated settings model as a caller-owned value; the runtime resolves no host profile or global clock from it.
- Model and validator semantics arrive settled from the `pydantic` dependency (`BaseModel`, `Field`, validators, `model_config`); this page owns only the `pydantic-settings`-specific source-chain, source classes, decode markers, and CLI surface.
- Cloud secret-manager sources are admitted rows on the priority tuple, never separate credential fetchers.

[INTEGRATION_STACK]:
- pydantic leg: a `BaseSettings` subclass is a `pydantic.BaseModel`, so its fields use `pydantic` `Field`/validators/`SecretStr` and `Annotated[...]` types; the `NoDecode`/`ForceDecode` markers compose with pydantic `Annotated` metadata on a single field, and validation faults are pydantic `ValidationError`.
- secrets-mount leg: an S3/cloud-mounted secrets tree from `.api/s3fs.md` or a K8s secret volume is read by `NestedSecretsSettingsSource(secrets_dir=...)`; the cloud secret-manager sources (`AWSSecretsManagerSettingsSource` etc.) are the credential-store origin whose values then flow into the same merged model.
- credential-handoff leg: the validated settings model is the caller-owned carrier of `storage_options` (key/secret/token/endpoint) that the `.api/s3fs.md` / `.api/fsspec.md` dispatch consumes and of the `tracer_provider` config the observability rail reads — one validated model, never per-consumer env reads.
- single rail: one `BaseSettings` model declares the full source tuple (init > CLI > env > dotenv > nested secrets > cloud secret manager) in `settings_customise_sources`, `CliApp.run` is the binary entry, and the resulting immutable model is threaded into every downstream resource/observability owner — never a second config object.

[RAIL_LAW]:
- Package: `pydantic-settings`
- Owns: layered settings admission, source-priority customisation, env/dotenv/file/secret/nested-secret sources, cloud secret managers, and the CLI source + `CliApp` runner
- Accept: one `BaseSettings` model, an explicit `settings_customise_sources` tuple, `SettingsConfigDict` knobs, typed nested settings, decode markers, the CLI annotation family + `CliApp.run`
- Reject: scattered `os.environ` reads, a global settings singleton, manual JSON env decoding, parallel config loaders, a cloud/file source placed on the chain without its admitting extra
