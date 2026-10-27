# [SETTINGS]

`BaseSettings.__init__` runs each source, merges their mappings, and validates the merged mapping once. `model_validate` runs the same sources and ignores a call's `extra`, `strict`, and `context`.

## [01]-[SOURCES]

Each source class reads configuration keys of its store (`env_file`, `secrets_dir`, `toml_file` with `toml_table_header`, `json_file`, `yaml_file` with `yaml_config_section`, `pyproject_toml_table_header`), and a source over a library outside pydantic-settings needs the extra naming its store (`yaml`, `azure-key-vault`, `gcp-secret-manager`, `aws-secrets-manager`):
- Default precedence runs init keywords, environment, dotenv, secrets directory, then field defaults
- `InitSettingsSource(settings_cls, init_kwargs=)` hands a host's mapping to the tuple as one source
- `NestedSecretsSettingsSource` reads nested keys from file names split by `secrets_nested_delimiter` or from one folder per model under `secrets_nested_subdir`
- File sources subclass `InitSettingsSource`, read field names or aliases, map nested tables onto nested models, and read a missing file as empty
- Cloud sources take their store address as constructor arguments (vault `url`, `project_id`, `secret_id`, `ssm_path`)
- Cloud sources subclass `EnvSettingsSource`, and prefix, nested delimiter, and JSON decoding apply to secret names
- `AzureKeyVaultSettingsSource` lists vault secrets in its constructor, an unreachable vault fails while `settings_customise_sources` builds its tuple
- Use `python-coding-cyclopts` for command lines

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
- `type Csv = Annotated[list[int], NoDecode, BeforeValidator(split)]` passes `1,2,3` raw to `split`, `enable_decoding=False` covers every field
- `env_ignore_empty=True` leaves a field at its default for an empty variable, an empty string otherwise fails as input
- `env_parse_none_str="null"` reads the literal as `None`, `env_parse_enums=True` reads member names beside values
- `strict=True` accepts environment strings for `int` and `bool`, init keywords stay strict
- `AliasChoices("SVC_URL", "url")` on `validation_alias` reads the first present name, an alias bypasses `env_prefix`
- Validation errors hold a field's `loc` (`("store", "port")`) with no variable name or source

## [04]-[DOTENV]

Dotenv keys pass through `extra`, and an unknown key under `extra="forbid"` fails as `extra_forbidden` with a lowercased `loc`:
- `extra="ignore"` drops unknown keys, `dotenv_filtering="match_prefix"` passes prefixed keys alone
- Unprefixed keys equal to a field name stay unread under `env_prefix` and raise nothing
- `env_file` sequences load in order with later files winning, environment variables win over every dotenv value
- `_env_file=None` on the constructor reads no dotenv for one call

## [05]-[SECRETS]

Secret fields are `SecretStr`, and `secrets_dir` reads one file per field named `<prefix><field>`, stripped of surrounding whitespace:
- Missing `secrets_dir` directories warn `directory "<dir>" does not exist`

## [06]-[LIFETIME]

Each construction builds fresh sources and rereads environment and files:
- `Settings.settings_cached()` keeps one instance per exact class and process, built once under a lock and typed `Self`
- Cached instances keep stale values after an environment change until `settings_clear_cache()` on the class
- Nested models a settings field names precede the settings class, a forward reference warns `IncompleteFieldDefinitionWarning`
- Sources skip decoding a field with an unresolved forward reference, its JSON variable fails validation as a raw string
- `class Scoped[T](BaseSettings)` parametrized as `Scoped[int]()` reads and validates its sources

## [07]-[CUSTOM_SOURCES]

Stores no built-in source reads subclass `PydanticBaseSettingsSource`, implement `get_field_value(field, field_name) -> (value, key, is_complex)` and `__call__() -> dict`, and join the tuple in `settings_customise_sources`:
- `self.current_state` holds the merged raw values of higher sources, environment strings unvalidated
- `self.settings_sources_data` holds each higher source's mapping by source class name
- Values a custom source supplies lose to every higher source supplying the same key
- `EnvSettingsSource` subclasses overriding `prepare_field_value` reuse environment name resolution with their own value parser
