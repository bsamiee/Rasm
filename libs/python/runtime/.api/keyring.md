# [PY_RUNTIME_API_KEYRING]

`keyring` owns platform-agnostic secret retrieval: a top-level facade routes every read, write, and delete to the highest-priority viable `KeyringBackend`, discovered by priority rank and selected from environment or config, with a `Credential` protocol for structured username-and-password pairs and a subclass extension point for a custom store. It is the runtime secret-admission row — a secret is read once at startup and minted into the downstream auth surface, never held in an env var or config file while a native keystore is viable.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `keyring`
- package: `keyring` (`MIT`)
- module: `keyring`
- namespaces: `keyring`, `keyring.core`, `keyring.backend`, `keyring.credentials`, `keyring.errors`, `keyring.backends`
- rail: secrets

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: backend family

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :-------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `backend.KeyringBackend`          | abstract base | backend contract; `priority` ranks viability           |
|  [02]   | `backends.chainer.ChainerBackend` | impl          | tries each viable backend in priority order            |
|  [03]   | `backend.Crypter`                 | protocol      | encrypt/decrypt extension point                        |
|  [04]   | `backend.NullCrypter`             | no-op impl    | passthrough crypter                                    |
|  [05]   | `backend.SchemeSelectable`        | mixin         | scheme-based backend selection                         |
|  [06]   | `backend.ExceptionTrap`           | context mgr   | suppresses+records a backend's viability exception     |
|  [07]   | `backend.KeyringBackendMeta`      | metaclass     | registers each subclass into `KeyringBackend._classes` |

[PUBLIC_TYPE_SCOPE]: credentials family

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :-------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `credentials.Credential`          | protocol      | `username` + `password` access contract |
|  [02]   | `credentials.SimpleCredential`    | impl          | plain username/password pair            |
|  [03]   | `credentials.EnvironCredential`   | impl          | env-var-backed credential               |
|  [04]   | `credentials.AnonymousCredential` | impl          | password-only credential (no username)  |

[PUBLIC_TYPE_SCOPE]: errors family

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                  |
| :-----: | :--------------------------- | :------------- | :---------------------------- |
|  [01]   | `errors.KeyringError`        | base exception | all keyring failures          |
|  [02]   | `errors.KeyringLocked`       | exception      | backend locked or unavailable |
|  [03]   | `errors.InitError`           | exception      | backend initialization failed |
|  [04]   | `errors.PasswordSetError`    | exception      | set operation failed          |
|  [05]   | `errors.PasswordDeleteError` | exception      | delete operation failed       |
|  [06]   | `errors.NoKeyringError`      | exception      | no viable backend found       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level facade

| [INDEX] | [SURFACE]                                                                   | [SHAPE]         | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------------------- | :-------------- | :---------------------------------- |
|  [01]   | `get_password(service_name, username) -> str \| None`                       | secret read     | retrieve password or `None`         |
|  [02]   | `set_password(service_name, username, password) -> None`                    | secret write    | store password string               |
|  [03]   | `delete_password(service_name, username) -> None`                           | secret delete   | remove stored password              |
|  [04]   | `get_credential(service_name, username: str \| None) -> Credential \| None` | structured read | pair; `None` user = backend default |
|  [05]   | `get_keyring() -> KeyringBackend`                                           | backend query   | return the active backend instance  |
|  [06]   | `set_keyring(keyring) -> None`                                              | backend set     | override the active backend         |

[ENTRYPOINT_SCOPE]: backend discovery and selection

| [INDEX] | [SURFACE]                                           | [SHAPE]         | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------- | :-------------- | :---------------------------------------------------- |
|  [01]   | `core.init_backend(limit=None) -> KeyringBackend`   | backend init    | pick highest-priority viable backend; `limit` filters |
|  [02]   | `core.load_env() -> KeyringBackend \| None`         | env select      | resolve `PYTHON_KEYRING_BACKEND` to a backend         |
|  [03]   | `core.load_config() -> KeyringBackend \| None`      | config select   | resolve `keyringrc.cfg` to a backend                  |
|  [04]   | `core.load_keyring(keyring_name) -> KeyringBackend` | name resolve    | import-load a backend by dotted class name            |
|  [05]   | `core.recommended(backend) -> bool`                 | viability       | predicate: backend priority is positive               |
|  [06]   | `core.disable() -> None`                            | backend disable | write config selecting the fail (null) backend        |
|  [07]   | `backend.get_all_keyring() -> list[KeyringBackend]` | backend list    | enumerate all registered backends                     |
|  [08]   | `backend.by_priority`                               | sort key        | `attrgetter('priority')` ranking key over backends    |

[ENTRYPOINT_SCOPE]: `KeyringBackend` abstract/extension interface
- Custom stores subclass `backend.KeyringBackend`, implementing the abstract reads/writes and declaring a float `priority`.

| [INDEX] | [SURFACE]                                                 | [SHAPE]         | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------------- | :-------------- | :---------------------------------------------- |
|  [01]   | `get_password(service, username) -> str \| None`          | abstract method | read secret from store                          |
|  [02]   | `set_password(service, username, password) -> None`       | abstract method | write secret to store                           |
|  [03]   | `delete_password(service, username) -> None`              | abstract method | remove secret from store                        |
|  [04]   | `get_credential(service, username) -> Credential \| None` | instance        | read structured credential                      |
|  [05]   | `priority` (class property)                               | viability rank  | float rank; base raises `NotImplementedError`   |
|  [06]   | `viable` (class property)                                 | viability gate  | whether the backend's deps/platform are present |
|  [07]   | `get_viable_backends() -> set[type[KeyringBackend]]`      | discovery       | viable subclasses ranked by priority            |
|  [08]   | `with_properties(**kwargs)` / `set_properties_from_env()` | configure       | clone with overrides / hydrate props from env   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- facade-dispatch law: `get_password`/`set_password`/`delete_password`/`get_credential` delegate to the active backend from `get_keyring()`; consuming code never constructs a backend.
- selection-precedence law: backend selection resolves `load_env()` (`PYTHON_KEYRING_BACKEND`), then `load_config()` (`keyringrc.cfg`), then `init_backend()` (highest-priority viable); `recommended` (positive priority) gates the auto-default, and `ChainerBackend` cascades every viable backend in priority order.
- priority law: each backend declares a float `priority`, the base raising `NotImplementedError` to force every concrete store to rank itself; `get_all_keyring`/`get_viable_backends` rank `by_priority` descending — macOS Keychain, SecretService/KWallet, Windows Credential Manager, chainer, fail/null.
- viability law: a backend's `viable` property under `ExceptionTrap` gates whether its platform deps are present; a backend raising during the probe is trapped and excluded, never crashing discovery.
- missing-vs-error law: `get_password`/`get_credential` return `None` for a missing secret and never raise; `delete_password` raises `PasswordDeleteError` on an absent entry; `AnonymousCredential.username` raises `ValueError`.

[STACKING]:
- `.api/httpx.md`: `get_credential(service, None)` yields a `Credential` (`username`/`password`) lifted to the admission `BasicCredential` and minted once into `httpx.BasicAuth` at client construction — the secret crosses into transport once, never re-read per request.
- `.api/pydantic-settings.md`: a custom `PydanticBaseSettingsSource` backed by `get_password` reads the keyring secret into a `SecretStr` field, so the settings model is the single admission point.
- within-lib: `EnvironCredential` is the declared env-var source when `NoKeyringError` fires in a headless lane, never a scattered `os.environ` read.

[LOCAL_ADMISSION]:
- `set_keyring`/`init_backend(limit=...)` inject or constrain a controlled backend at the test boundary; production selects via env/config, never by mutating the active backend in-process.
- `errors.NoKeyringError` is the expected failure in a headless/container lane with no native keystore; the secret-admission rail lifts it to a typed boundary fault and falls back to the explicitly-configured env/config source, never a silent plaintext default.

[RAIL_LAW]:
- Package: `keyring`
- Owns: platform-backed secret storage/retrieval, priority-ranked backend discovery, env/config backend selection, and the `KeyringBackend` extension point
- Accept: `get_password`/`set_password`/`delete_password`/`get_credential` facade calls; `init_backend`/`load_env`/`load_config`/`load_keyring` selection; `recommended`/`by_priority`/`get_viable_backends` ranking; `KeyringBackend` subclassing with an explicit `priority`; the keyring-sourced `Credential` minted once into the httpx `Auth`/pydantic-settings rail
- Reject: raw secrets in env vars or config files while a viable keyring exists; backends constructed directly in domain code; the secret re-read per request instead of minted once into the auth surface; the active backend mutated in production via `set_keyring`
