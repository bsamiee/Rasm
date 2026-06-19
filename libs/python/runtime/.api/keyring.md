# [PY_RUNTIME_API_KEYRING]

`keyring` supplies a platform-agnostic secret storage facade: top-level functions for `get_password`, `set_password`, `delete_password`, and `get_credential` dispatch to the highest-priority available `KeyringBackend` (macOS Keychain, SecretService, Windows Credential Manager, or fallback), with explicit backend selection, pluggable backend extension, and a `Credential` protocol for structured username-plus-password retrieval.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `keyring`
- package: `keyring`
- module: `keyring`
- asset: runtime library
- rail: secrets

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: backend family
- rail: secrets

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :--------------------------- | :------------ | :------------------------------ |
|  [01]   | `backend.KeyringBackend`     | abstract base | backend contract for all stores |
|  [02]   | `backend.Crypter`            | protocol      | encrypt/decrypt extension point |
|  [03]   | `backend.NullCrypter`        | no-op impl    | passthrough crypter             |
|  [04]   | `backend.SchemeSelectable`   | mixin         | scheme-based backend selection  |
|  [05]   | `backend.KeyringBackendMeta` | metaclass     | backend registration metaclass  |

[PUBLIC_TYPE_SCOPE]: credentials family
- rail: secrets

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                                 |
| :-----: | :-------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `credentials.Credential`          | protocol      | username + password access contract    |
|  [02]   | `credentials.SimpleCredential`    | impl          | plain username/password pair           |
|  [03]   | `credentials.EnvironCredential`   | impl          | env-var-backed credential              |
|  [04]   | `credentials.AnonymousCredential` | impl          | password-only credential (no username) |

[PUBLIC_TYPE_SCOPE]: errors family
- rail: secrets

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [RAIL]                        |
| :-----: | :--------------------------- | :------------- | :---------------------------- |
|  [01]   | `errors.KeyringError`        | base exception | all keyring failures          |
|  [02]   | `errors.KeyringLocked`       | exception      | backend locked or unavailable |
|  [03]   | `errors.PasswordSetError`    | exception      | set operation failed          |
|  [04]   | `errors.PasswordDeleteError` | exception      | delete operation failed       |
|  [05]   | `errors.InitError`           | exception      | backend initialization failed |
|  [06]   | `errors.NoKeyringError`      | exception      | no viable backend found       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level facade
- rail: secrets

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]  | [RAIL]                          |
| :-----: | :----------------------------------------------------------- | :-------------- | :------------------------------ |
|  [01]   | `get_password(service_name, username) -> str\|None`          | secret read     | retrieve password or `None`     |
|  [02]   | `set_password(service_name, username, password)`             | secret write    | store password string           |
|  [03]   | `delete_password(service_name, username)`                    | secret delete   | remove stored password          |
|  [04]   | `get_credential(service_name, username) -> Credential\|None` | structured read | retrieve username+password pair |
|  [05]   | `get_keyring() -> KeyringBackend`                            | backend query   | return active backend instance  |
|  [06]   | `set_keyring(keyring)`                                       | backend set     | override active backend         |

[ENTRYPOINT_SCOPE]: backend management
- rail: secrets

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]  | [RAIL]                              |
| :-----: | :---------------------------------- | :-------------- | :---------------------------------- |
|  [01]   | `core.init_backend(limit=None)`     | backend init    | initialize best available backend   |
|  [02]   | `core.disable()`                    | backend disable | disable keyring (sets fail backend) |
|  [03]   | `backend.get_all_keyring() -> list` | backend list    | enumerate all registered backends   |

[ENTRYPOINT_SCOPE]: KeyringBackend abstract interface
- rail: secrets

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]  | [RAIL]                     |
| :-----: | :------------------------------------------------------ | :-------------- | :------------------------- |
|  [01]   | `get_password(service, username) -> str\|None`          | abstract method | read secret from store     |
|  [02]   | `set_password(service, username, password) -> None`     | abstract method | write secret to store      |
|  [03]   | `delete_password(service, username) -> None`            | abstract method | remove secret from store   |
|  [04]   | `get_credential(service, username) -> Credential\|None` | abstract method | read structured credential |

## [04]-[IMPLEMENTATION_LAW]

[SECRETS_TOPOLOGY]:
- top-level functions (`get_password`, `set_password`, etc.) delegate to the active backend via `get_keyring()`
- backend priority: macOS Keychain > SecretService (Linux) > Windows Credential Manager > fail backend
- `PYTHON_KEYRING_BACKEND` environment variable overrides backend selection
- `get_credential` returns `None` when no credential exists; never raises for missing secrets
- `delete_password` raises `PasswordDeleteError` when the entry does not exist
- `AnonymousCredential.username` raises `ValueError`; access only `.password`
- `get_all_keyring()` returns a list sorted by descending priority

[LOCAL_ADMISSION]:
- secret retrieval at process startup uses `get_password` or `get_credential` via the top-level facade; backend is never constructed directly in consuming code
- `set_keyring` is a test-boundary tool for injecting a controlled backend; production code never calls it
- `errors.NoKeyringError` is the expected failure when running in headless or container environments with no native keystore

[RAIL_LAW]:
- Package: `keyring`
- Owns: platform-backed secret storage and retrieval
- Accept: `get_password`/`set_password`/`delete_password`/`get_credential` facade calls; `KeyringBackend` subclassing for custom stores
- Reject: storing raw secrets in environment variables or config files when keyring is available; constructing backends directly in domain code
