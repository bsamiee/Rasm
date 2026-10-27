# [SETTINGS]

`BaseSettings.__init__` runs each source, merges their mappings, and validates the merged mapping once, so `Settings()` and `model_validate` both read every source.

## [01]-[SOURCES]

Sources a `settings_customise_sources` tuple can hold, with the configuration keys each reads:

| [INDEX] | [SOURCE]                            | [CONFIGURATION]                                                         |
| :-----: | :---------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `InitSettingsSource`                | Constructor keywords, or `init_kwargs=` a mapping a host hands over     |
|  [02]   | `EnvSettingsSource`                 | `env_prefix`, `env_nested_delimiter`, `env_parse_*`, `env_ignore_empty` |
|  [03]   | `DotEnvSettingsSource`              | `env_file`, `env_file_encoding`, `dotenv_filtering`                     |
|  [04]   | `SecretsSettingsSource`             | `secrets_dir`, one file per field                                       |
|  [05]   | `NestedSecretsSettingsSource`       | `secrets_nested_delimiter`, `secrets_nested_subdir`                     |
|  [06]   | `TomlConfigSettingsSource`          | `toml_file`, `toml_table_header`                                        |
|  [07]   | `PyprojectTomlConfigSettingsSource` | `pyproject_toml_table_header`, `pyproject_toml_depth`                   |
|  [08]   | `JsonConfigSettingsSource`          | `json_file`, `json_file_encoding`                                       |
|  [09]   | `YamlConfigSettingsSource`          | `yaml_file`, `yaml_config_section`, `pydantic-settings[yaml]`           |
|  [10]   | `AzureKeyVaultSettingsSource`       | Vault `url` and `credential`, `pydantic-settings[azure-key-vault]`      |
|  [11]   | `GoogleSecretManagerSettingsSource` | `project_id`, `pydantic-settings[gcp-secret-manager]`                   |
|  [12]   | `AWSSecretsManagerSettingsSource`   | `secret_id`, `pydantic-settings[aws-secrets-manager]`                   |
|  [13]   | `AWSSystemsManagerSettingsSource`   | `ssm_path`, `pydantic-settings[aws-systems-manager]`                    |

- Default precedence runs init keywords, environment, dotenv, secrets directory, then field defaults
- File sources subclass `InitSettingsSource`, read field names or aliases, map nested tables onto nested models, and read a missing file as empty
- Cloud sources subclass `EnvSettingsSource`, so prefix, nested delimiter, and JSON decoding apply to secret names
- `AzureKeyVaultSettingsSource` lists vault secrets in its constructor, an unreachable vault fails as the hook builds the tuple

## [02]-[PRECEDENCE]

Sources merge key by key at every depth, a higher source winning each key it supplies:
- TOML `store.host` survives beside environment `APP_STORE__PORT`, and the nested model validates the merged mapping
- Init keywords as a mapping merge with lower sources, an init keyword holding a model instance replaces its whole field
- `nested_model_default_partial_update=True` lets environment keys patch a model default, without it they patch a model of class defaults
- JSON in `APP_STORE` and delimiter keys (`APP_STORE__PORT`) combine, the delimiter key wins its field
- Hook parameters keep their stock names, pydantic-settings passes them as keywords and a renamed one raises `TypeError` at construction

## [03]-[ENVIRONMENT]

Environment names are field names, case-insensitive, with `env_prefix` before each:
- Variables matching no field go unread under `extra="forbid"`, a misspelled variable leaves its field at the default
- `case_sensitive=True` matches the prefixed name exactly (`APP_name` for field `name`)
- `env_nested_delimiter="__"` splits `APP_STORE__PORT` into `{"store": {"port": ...}}` under a declared field alone
- `list`, `set`, `dict`, `tuple`, and model fields decode their value as JSON, an undecodable value raises `SettingsError`
- `type Csv = Annotated[list[int], NoDecode, BeforeValidator(split)]` passes `1,2,3` raw to `split`, `enable_decoding=False` covers every field
- `env_ignore_empty=True` leaves a field at its default for an empty variable, an empty string otherwise fails as input
- `env_parse_none_str="null"` reads that literal as `None`, `env_parse_enums=True` reads member names beside values
- `strict=True` accepts environment strings for `int` and `bool`, init keywords stay strict
- `AliasChoices("SVC_URL", "url")` on `validation_alias` reads the first present name, an alias bypasses `env_prefix`
- Validation errors hold the field `loc` (`("store", "port")`), the variable name and source stay out of the error

## [04]-[DOTENV]

Dotenv keys pass through `extra`, so an unknown key under the default `extra="forbid"` fails as `extra_forbidden` with a lowercased `loc`:
- `extra="ignore"` drops unknown keys, `dotenv_filtering="match_prefix"` passes prefixed keys alone
- Unprefixed keys equal to a field name stay unread under `env_prefix` and raise nothing
- `env_file` sequences load in order with later files winning, environment variables win over every dotenv value
- `_env_file=None` on the constructor reads no dotenv for that call

## [05]-[SECRETS]

Secret fields are `SecretStr`, masked in `repr` and JSON, and read through `get_secret_value()` at their use:
- `secrets_dir` reads one file per field named `<prefix><field>`, stripped of surrounding whitespace
- Missing `secrets_dir` directories warn `directory "<dir>" does not exist`
- `PYDANTIC_SETTINGS_DEBUG=1` logs each source mapping at `DEBUG` on the `pydantic_settings` logger, secret values included

## [06]-[LIFETIME]

Each construction builds fresh sources and rereads environment and files:
- `Settings.settings_cached()` keeps one instance per exact class and process, built once under a lock and typed `Self`
- Cached instances keep stale values after an environment change until `settings_clear_cache()` on that class
- Nested models a settings field names precede the settings class, a forward reference warns `IncompleteFieldDefinitionWarning`
- Sources skip decoding a field with an unresolved forward reference, its JSON variable fails validation as a raw string
- `class Scoped[T](BaseSettings)` parametrized as `Scoped[int]()` reads and validates its sources

## [07]-[CUSTOM_SOURCES]

Stores no stock source reads subclass `PydanticBaseSettingsSource`, implement `get_field_value(field, field_name) -> (value, key, is_complex)` and `__call__() -> dict`, and join the tuple in `settings_customise_sources`:
- `self.current_state` holds the merged raw values of higher sources, environment strings unvalidated
- `self.settings_sources_data` holds each higher source's mapping by source class name
- Values a custom source supplies lose to every higher source supplying the same key
- `EnvSettingsSource` subclasses overriding `prepare_field_value` reuse environment name resolution with their own value parser
