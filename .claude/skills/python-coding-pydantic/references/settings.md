# [SETTINGS]

`BaseSettings.__init__` and `model_validate` run each source, merge their mappings, and validate the merged mapping once.

## [01]-[SOURCES]

Sources take their store from configuration keys (`env_file`, `secrets_dir`, `toml_file`, `json_file`, `yaml_file`, `pyproject_toml_table_header`). Sources over a library outside pydantic-settings need its extra (`yaml`, `azure-key-vault`, `gcp-secret-manager`, `aws-secrets-manager`):
- Default precedence runs init keywords, environment, dotenv, secrets directory, then field defaults
- `toml_table_header` and `yaml_config_section` select a table inside their file
- `InitSettingsSource(settings_cls, init_kwargs=)` hands a host's mapping to the tuple as one source
- `NestedSecretsSettingsSource` reads nested keys from file names split on `secrets_nested_delimiter` or model folders under `secrets_nested_subdir`
- File sources subclass `InitSettingsSource`, read field names or aliases, map nested tables onto nested models, and read a missing file as empty
- Cloud sources take their store address as constructor arguments (vault `url`, `project_id`, `secret_id`, `ssm_path`)
- Cloud sources subclass `EnvSettingsSource`, and prefix, nested delimiter, and JSON decoding apply to secret names
- `AzureKeyVaultSettingsSource` lists secrets when constructed, an unreachable vault fails `settings_customise_sources`, nested names split on `--`

## [02]-[PRECEDENCE]

Sources merge key by key at every depth, a higher source winning each key it supplies:
- Init keywords as a mapping merge with lower sources, an init keyword holding a model instance replaces its whole field
- `nested_model_default_partial_update=True` lets environment keys patch a model default, without it they patch a model of class defaults
- JSON in `APP_STORE` and delimiter keys (`APP_STORE__PORT`) combine, the delimiter key wins its field
- Hook parameters keep their declared names, pydantic-settings passes them as keywords and a renamed one raises `TypeError` at construction

## [03]-[ENVIRONMENT]

Environment names are field names, case-insensitive, with `env_prefix` before each:
- Variables matching no field go unread under `extra="forbid"`, a misspelled variable leaves its field at the default
- `case_sensitive=True` matches the prefixed name exactly (`APP_name` for field `name`)
- `env_nested_delimiter="__"` splits `APP_STORE__PORT` into `{"store": {"port": ...}}` under a declared field alone
- `list`, `set`, `dict`, `tuple`, and model fields decode their value as JSON, an undecodable value raises `SettingsError`
- `NoDecode` on a `list[int]` hands `1,2,3` to `BeforeValidator(split)` undecoded, `enable_decoding=False` covers all, `ForceDecode` restores one
- `env_ignore_empty=True` leaves a field at its default for an empty variable, otherwise the empty string validates as input
- `env_parse_none_str="null"` reads the literal as `None`, `env_parse_enums=True` reads member names beside values
- `strict=True` accepts environment strings for `int` and `bool`, init keywords stay strict
- `validation_alias=AliasChoices("SVC_URL", "url")` reads first present name, aliases skip `env_prefix` until `env_prefix_target="alias"` or `"all"`
- Validation errors hold a field's `loc` (`("store", "port")`) with no variable name or source

## [04]-[DOTENV]

Dotenv keys pass through `extra`, and an unknown key under `extra="forbid"` fails as `extra_forbidden` with a lowercased `loc`:
- `extra="ignore"` drops unknown keys, `dotenv_filtering="match_prefix"` passes prefixed keys alone, `"only_existing"` passes field keys alone
- Unprefixed keys equal to a field name stay unread under `env_prefix` and raise nothing
- `env_file` sequences load in order with later files winning, environment variables win over every dotenv value
- `_env_file=None` on the constructor reads no dotenv for one call

## [05]-[SECRETS]

Secret fields are `SecretStr`, and `secrets_dir` reads one file per field named `<prefix><field>`, stripped of surrounding whitespace:
- `secrets_dir_missing="error"` on `NestedSecretsSettingsSource(file_secret_settings)` raises `SettingsError` on missing directory, unwrapped it warns
- `secrets_prefix`, `secrets_case_sensitive`, and `secrets_dir_max_size` configure the nested source alone

## [06]-[LIFETIME]

Each construction builds fresh sources and rereads environment and files:
- `settings_cached()` keys its instance by exact class and builds it under a lock
- Cached instances keep stale values after an environment change until `settings_clear_cache()` on the class
- Nested models a settings field names precede the settings class, a forward reference warns `IncompleteFieldDefinitionWarning`
- Sources skip decoding a field with an unresolved forward reference, its JSON variable fails validation as a raw string
- `class Scoped[T](BaseSettings)` parametrized as `Scoped[int]()` reads and validates its sources

## [07]-[CUSTOM_SOURCES]

`PydanticBaseSettingsSource` subclasses implement `get_field_value(field, field_name) -> (value, key, is_complex)` and `__call__() -> dict`. `settings_customise_sources` adds them to its tuple:
- `self.current_state` holds the merged raw values of higher sources, environment strings unvalidated
- `self.settings_sources_data` holds each higher source's mapping by source class name
- Values a custom source supplies lose to every higher source supplying the same key
- `EnvSettingsSource` subclasses overriding `prepare_field_value` reuse environment name resolution with their own value parser
