# [PY_RUNTIME_API_PYDANTIC_SETTINGS]

`pydantic-settings` owns layered configuration admission: one `BaseSettings` model folds a declared source-priority chain and a CLI source into a single validated settings model, the runtime's boundary for caller-provided configuration.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pydantic-settings`
- package: `pydantic-settings`
- module: `pydantic_settings`
- namespaces: `pydantic_settings`, `pydantic_settings.sources`, `pydantic_settings.sources.providers`
- rail: validation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: settings base family

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :--------------------------- | :------------ | :------------------------------- |
|  [01]   | `BaseSettings`               | settings base | layered validated settings model |
|  [02]   | `SettingsConfigDict`         | config        | settings behavior/source knobs   |
|  [03]   | `PydanticBaseSettingsSource` | source base   | custom settings source contract  |
|  [04]   | `CliApp`                     | CLI runner    | settings-backed CLI entry        |

[PUBLIC_TYPE_SCOPE]: source family

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                            |
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

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [CAPABILITY]                                |
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
- `BaseSettings()` runs the resolved source chain at instantiation; per-instance kwargs `_env_file`, `_env_prefix`, `_env_nested_delimiter`, `_secrets_dir`, `_cli_parse_args` redirect a source without subclassing.

| [INDEX] | [SURFACE]                                                                      | [SHAPE]      | [CAPABILITY]                    |
| :-----: | :----------------------------------------------------------------------------- | :----------- | :------------------------------ |
|  [01]   | `BaseSettings(**values)`                                                       | construct    | runs the resolved source chain  |
|  [02]   | `settings_customise_sources(...)`                                              | source order | reorders the built-in sources   |
|  [03]   | `PydanticBaseSettingsSource.__call__()` / `get_field_value(field, field_name)` | source impl  | a custom source's field mapping |

[ENTRYPOINT_SCOPE]: settings config knobs (`SettingsConfigDict`)

| [INDEX] | [KNOB]                                                              | [SHAPE]      | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------ | :----------- | :----------------------------------------- |
|  [01]   | `env_prefix` / `env_prefix_target` / `case_sensitive`               | env binding  | environment variable name mapping          |
|  [02]   | `env_nested_delimiter` / `env_parse_none_str` / `env_parse_enums`   | env nesting  | nested-model env key splitting and parsing |
|  [03]   | `env_file` / `env_file_encoding`                                    | dotenv       | `.env` file path(s) and encoding           |
|  [04]   | `secrets_dir`                                                       | secrets      | secrets directory path(s)                  |
|  [05]   | `toml_file` / `yaml_file` / `json_file` (+ `_encoding`)             | file source  | declarative file-source paths              |
|  [06]   | `cli_parse_args` / `cli_prog_name` / `cli_kebab_case`               | cli          | CLI program name and case                  |
|  [07]   | `cli_implicit_flags` / `cli_enforce_required` / `cli_exit_on_error` | cli          | CLI flag, required, and error behavior     |
|  [08]   | `extra` / `nested_model_default_partial_update`                     | merge policy | unknown-key + nested-merge behavior        |

[ENTRYPOINT_SCOPE]: CLI runner

| [INDEX] | [SURFACE]                                                         | [SHAPE]      | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------- | :----------- | :----------------------------------------- |
|  [01]   | `CliApp.run(model_cls, ...) -> T`                                 | CLI run      | build the model, invoke its `cli_cmd`      |
|  [02]   | `CliApp.run_subcommand(model) -> PydanticModel`                   | CLI dispatch | run a subcommand on an already-built model |
|  [03]   | `get_subcommand(model, is_required=True, cli_exit_on_error=True)` | CLI          | extract the selected `CliSubCommand`       |

- `CliApp.run` awaits an async `cli_cmd` on a thread isolated under a running loop; `run_subcommand` and `get_subcommand` share `cli_exit_on_error` and `cli_cmd_method_name`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Caller configuration is one `BaseSettings` subclass; `settings_customise_sources` declares the source-priority chain once as a `tuple[PydanticBaseSettingsSource, ...]`, the earlier element winning.
- Every configuration origin is one source on that tuple; a new origin is one source class implementing `PydanticBaseSettingsSource.__call__`, and the default `init_settings` > `env_settings` > `dotenv_settings` > `file_secret_settings` reorders by returning a permuted tuple.
- Structured settings nest validated models, `env_nested_delimiter` splitting flat keys into the model tree and the `NoDecode`/`ForceDecode` markers overriding complex decode; `SecretsSettingsSource` reads flat per-key files, `NestedSecretsSettingsSource` a subdirectory-per-model tree.
- CLI parsing is `argparse`-backed: flag arity, subcommands, positionals, hidden fields, and exclusivity are declared by the CLI annotation family, and `CliApp.run` builds the model and dispatches to its `cli_cmd`.
- A file or cloud source lazily imports its backing dependency and raises `SettingsError` when the admitting extra is absent.
- A settings-load failure surfaces as `SettingsError`/`ValidationError` lifted into `Error(BoundaryFault(...))` at admission; the runtime holds no global settings singleton.

[STACKING]:
- `pydantic`(`.api/pydantic.md`): a `BaseSettings` subclass is a `BaseModel`, so fields carry `pydantic` `Field`/validators/`SecretStr` and the `NoDecode`/`ForceDecode` markers ride `Annotated` metadata on one field; admission faults are `ValidationError`.
- `fsspec`(`.api/fsspec.md`) / `obstore`(`.api/obstore.md`): the validated model carries the `storage_options` (key/secret/token/endpoint) the branch storage dispatch consumes, and a cloud-mounted secrets tree is read by `NestedSecretsSettingsSource(secrets_dir=...)`; the cloud secret-manager sources are the credential-store origin feeding the same merged model.
- runtime rail: one `BaseSettings` model declares the full source tuple in `settings_customise_sources`, `CliApp.run` is the binary entry, and the resulting immutable model threads into every downstream resource and observability owner.

[LOCAL_ADMISSION]:
- Admission receives the validated settings model as a caller-owned value; the runtime resolves no host profile or global clock from it.
- Model and validator semantics arrive settled from `pydantic`; this catalog owns only the `pydantic-settings` source chain, source classes, decode markers, and CLI surface.
- A cloud secret-manager source is an admitted row on the priority tuple, never a separate credential fetcher, and its extra is admitted in the owning manifest before it joins the chain.

[RAIL_LAW]:
- Package: `pydantic-settings`
- Owns: layered settings admission, source-priority customisation, env/dotenv/file/secret/nested-secret sources, cloud secret managers, and the CLI source with its `CliApp` runner
- Accept: one `BaseSettings` model, an explicit `settings_customise_sources` tuple, `SettingsConfigDict` knobs, typed nested settings, decode markers, the CLI annotation family with `CliApp.run`
- Reject: scattered `os.environ` reads, a global settings singleton, manual JSON env decoding, a parallel config loader, a cloud or file source placed on the chain without its admitting extra
