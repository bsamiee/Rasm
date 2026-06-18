# [PY_RUNTIME_API_KEYRING]

`keyring` supplies a platform-agnostic secret storage facade: top-level functions for `get_password`, `set_password`, `delete_password`, and `get_credential` dispatch to the highest-priority available `KeyringBackend` (macOS Keychain, SecretService, Windows Credential Manager, or fallback), with explicit backend selection, pluggable backend extension, and a `Credential` protocol for structured username-plus-password retrieval.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `keyring`
- package: `keyring`
- module: `keyring`
- asset: runtime library
- rail: secrets

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: backend family
- rail: secrets

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :--------------------------- | :------------ | :------------------------------ |
|   [1]   | `backend.KeyringBackend`     | abstract base | backend contract for all stores |
|   [2]   | `backend.Crypter`            | protocol      | encrypt/decrypt extension point |
|   [3]   | `backend.NullCrypter`        | no-op impl    | passthrough crypter             |
|   [4]   | `backend.SchemeSelectable`   | mixin         | scheme-based backend selection  |
|   [5]   | `backend.KeyringBackendMeta` | metaclass     | backend registration metaclass  |

[PUBLIC_TYPE_SCOPE]: credentials family
- rail: secrets

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                                 |
| :-----: | :-------------------------------- | :------------ | :------------------------------------- |
|   [1]   | `credentials.Credential`          | protocol      | username + password access contract    |
|   [2]   | `credentials.SimpleCredential`    | impl          | plain username/password pair           |
|   [3]   | `credentials.EnvironCredential`   | impl          | env-var-backed credential              |
|   [4]   | `credentials.AnonymousCredential` | impl          | password-only credential (no username) |

[PUBLIC_TYPE_SCOPE]: errors family
- rail: secrets

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [RAIL]                        |
| :-----: | :--------------------------- | :------------- | :---------------------------- |
|   [1]   | `errors.KeyringError`        | base exception | all keyring failures          |
|   [2]   | `errors.KeyringLocked`       | exception      | backend locked or unavailable |
|   [3]   | `errors.PasswordSetError`    | exception      | set operation failed          |
|   [4]   | `errors.PasswordDeleteError` | exception      | delete operation failed       |
|   [5]   | `errors.InitError`           | exception      | backend initialization failed |
|   [6]   | `errors.NoKeyringError`      | exception      | no viable backend found       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level facade
- rail: secrets

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]  | [RAIL]                          |
| :-----: | :----------------------------------------------------------- | :-------------- | :------------------------------ |
|   [1]   | `get_password(service_name, username) -> str\|None`          | secret read     | retrieve password or `None`     |
|   [2]   | `set_password(service_name, username, password)`             | secret write    | store password string           |
|   [3]   | `delete_password(service_name, username)`                    | secret delete   | remove stored password          |
|   [4]   | `get_credential(service_name, username) -> Credential\|None` | structured read | retrieve username+password pair |
|   [5]   | `get_keyring() -> KeyringBackend`                            | backend query   | return active backend instance  |
|   [6]   | `set_keyring(keyring)`                                       | backend set     | override active backend         |

[ENTRYPOINT_SCOPE]: backend management
- rail: secrets

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]  | [RAIL]                              |
| :-----: | :---------------------------------- | :-------------- | :---------------------------------- |
|   [1]   | `core.init_backend(limit=None)`     | backend init    | initialize best available backend   |
|   [2]   | `core.disable()`                    | backend disable | disable keyring (sets fail backend) |
|   [3]   | `backend.get_all_keyring() -> list` | backend list    | enumerate all registered backends   |

[ENTRYPOINT_SCOPE]: KeyringBackend abstract interface
- rail: secrets

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]  | [RAIL]                     |
| :-----: | :------------------------------------------------------ | :-------------- | :------------------------- |
|   [1]   | `get_password(service, username) -> str\|None`          | abstract method | read secret from store     |
|   [2]   | `set_password(service, username, password) -> None`     | abstract method | write secret to store      |
|   [3]   | `delete_password(service, username) -> None`            | abstract method | remove secret from store   |
|   [4]   | `get_credential(service, username) -> Credential\|None` | abstract method | read structured credential |

## [4]-[IMPLEMENTATION_LAW]

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
