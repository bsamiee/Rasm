# [PY_RUNTIME_API_PYDANTIC_SETTINGS]

`pydantic-settings` supplies layered settings admission: a `BaseSettings` base, a customisable source priority chain (init, env, dotenv, TOML/YAML/JSON, secrets dirs, cloud secret managers), and a CLI source. It is the runtime owner for caller-provided configuration admitted as one validated settings model.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pydantic-settings`
- package: `pydantic-settings`
- import: `pydantic_settings`
- owner: `runtime`
- rail: validation
- namespaces: `pydantic_settings`, `pydantic_settings.sources`
- capability: layered settings model, source-priority customisation, env/dotenv/file/secret sources, cloud secret managers, CLI source

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: settings base family
- rail: validation

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :--------------------------- | :------------ | :------------------------------- |
|   [1]   | `BaseSettings`               | settings base | layered validated settings model |
|   [2]   | `SettingsConfigDict`         | config        | settings behavior/source knobs   |
|   [3]   | `PydanticBaseSettingsSource` | source base   | custom settings source contract  |
|   [4]   | `CliApp`                     | CLI runner    | settings-backed CLI entry        |

[PUBLIC_TYPE_SCOPE]: source family
- rail: validation

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :---------------------------------- | :------------ | :--------------------------- |
|   [1]   | `InitSettingsSource`                | source        | constructor-kwarg layer      |
|   [2]   | `EnvSettingsSource`                 | source        | environment-variable layer   |
|   [3]   | `DotEnvSettingsSource`              | source        | `.env` file layer            |
|   [4]   | `SecretsSettingsSource`             | source        | secrets-directory layer      |
|   [5]   | `TomlConfigSettingsSource`          | source        | TOML file layer              |
|   [6]   | `YamlConfigSettingsSource`          | source        | YAML file layer              |
|   [7]   | `JsonConfigSettingsSource`          | source        | JSON file layer              |
|   [8]   | `PyprojectTomlConfigSettingsSource` | source        | `pyproject.toml` table layer |
|   [9]   | `CliSettingsSource`                 | source        | command-line layer           |
|  [10]   | `AWSSecretsManagerSettingsSource`   | source        | AWS Secrets Manager layer    |
|  [11]   | `AzureKeyVaultSettingsSource`       | source        | Azure Key Vault layer        |
|  [12]   | `GoogleSecretManagerSettingsSource` | source        | GCP Secret Manager layer     |

[PUBLIC_TYPE_SCOPE]: CLI annotation and fault family
- rail: validation

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [RAIL]                     |
| :-----: | :-------------------------- | :------------- | :------------------------- |
|   [1]   | `CliSubCommand`             | CLI annotation | subcommand field marker    |
|   [2]   | `CliPositionalArg`          | CLI annotation | positional argument marker |
|   [3]   | `CliImplicitFlag`           | CLI annotation | implicit boolean flag      |
|   [4]   | `CliMutuallyExclusiveGroup` | CLI annotation | exclusive-option group     |
|   [5]   | `NoDecode`                  | decode marker  | suppress complex decode    |
|   [6]   | `ForceDecode`               | decode marker  | force complex decode       |
|   [7]   | `SettingsError`             | fault          | settings-load failure      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: settings operations
- rail: validation

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :---------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `BaseSettings.settings_customise_sources` | source order   | declare source priority chain |
|   [2]   | `BaseSettings.model_validate`             | parse          | validate merged settings      |
|   [3]   | `get_subcommand`                          | CLI            | extract selected subcommand   |
|   [4]   | `CliApp.run`                              | CLI            | run settings-backed CLI app   |

## [4]-[IMPLEMENTATION_LAW]

[SETTINGS_TOPOLOGY]:
- settings law: caller configuration is one `BaseSettings` subclass; the source priority chain is declared once in `settings_customise_sources`, never resolved with scattered `os.environ` reads.
- source law: every configuration origin is a source in the priority list — init kwargs, env, dotenv, the chosen file format, secrets dir; a new origin is one additional source, never a parallel loader.
- nested law: structured settings nest validated models; complex env values use the `NoDecode`/`ForceDecode` markers, never manual JSON parsing.
- boundary law: a settings-load failure surfaces as `SettingsError`/`ValidationError` lifted into `Error(BoundaryFault(...))` at admission; the runtime owns no global settings singleton.

[LOCAL_ADMISSION]:
- The context-admission surface receives the validated settings model as a caller-owned value; the runtime resolves no host profile or global clock from it.
- Model and validator semantics arrive settled from `.api/pydantic.md`; this page owns only the source-chain surface.
- Cloud secret-manager sources are admitted rows on the priority chain, never separate credential fetchers.

[RAIL_LAW]:
- Package: `pydantic-settings`
- Owns: layered settings admission, source-priority customisation, env/dotenv/file/secret sources, and the CLI settings source
- Accept: one `BaseSettings` model, an explicit source chain, typed nested settings, decode markers
- Reject: scattered `os.environ` reads, a global settings singleton, manual JSON env decoding, parallel config loaders
