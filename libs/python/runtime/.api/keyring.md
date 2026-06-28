# [PY_RUNTIME_API_KEYRING]

`keyring` supplies a platform-agnostic secret-storage facade: top-level `get_password`/`set_password`/`delete_password`/`get_credential` dispatch to the highest-priority viable `KeyringBackend` (macOS Keychain, SecretService/KWallet on Linux, Windows Credential Manager, or the chained/fail fallbacks), with priority-ranked backend discovery, environment- and config-file-driven backend selection, a `Credential` protocol for structured username-plus-password retrieval, and a pluggable backend extension point. It is the runtime secret-admission row: secrets are read once at startup through the facade and minted into the downstream auth surface (httpx `Auth`, pydantic-settings), never stored in env vars or config files when a native keystore is viable.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `keyring`
- package: `keyring`
- import: `keyring`
- owner: `runtime`
- rail: secrets
- version: `25.7.0`
- license: `MIT`
- namespaces: `keyring`, `keyring.core`, `keyring.backend`, `keyring.credentials`, `keyring.errors`, `keyring.backends`
- capability: priority-ranked platform secret storage, structured credential retrieval, environment/config-file backend selection, chained-backend fallback, pluggable `KeyringBackend` extension, full failure taxonomy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: backend family
- rail: secrets

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [RAIL]                                              |
| :-----: | :----------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `backend.KeyringBackend`       | abstract base | backend contract; `priority` ranks viability        |
|  [02]   | `backends.chainer.ChainerBackend` | impl       | tries each viable backend in priority order         |
|  [03]   | `backend.Crypter`              | protocol      | encrypt/decrypt extension point                     |
|  [04]   | `backend.NullCrypter`          | no-op impl    | passthrough crypter                                 |
|  [05]   | `backend.SchemeSelectable`     | mixin         | scheme-based backend selection                      |
|  [06]   | `backend.ExceptionTrap`        | context mgr   | suppresses+records a backend's viability exception   |
|  [07]   | `backend.KeyringBackendMeta`   | metaclass     | registers each subclass into `KeyringBackend._classes` |

[PUBLIC_TYPE_SCOPE]: credentials family
- rail: secrets

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                                 |
| :-----: | :-------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `credentials.Credential`          | protocol      | `username` + `password` access contract |
|  [02]   | `credentials.SimpleCredential`    | impl          | plain username/password pair           |
|  [03]   | `credentials.EnvironCredential`   | impl          | env-var-backed credential              |
|  [04]   | `credentials.AnonymousCredential` | impl          | password-only credential (no username) |

[PUBLIC_TYPE_SCOPE]: errors family
- rail: secrets

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [RAIL]                        |
| :-----: | :--------------------------- | :------------- | :---------------------------- |
|  [01]   | `errors.KeyringError`        | base exception | all keyring failures          |
|  [02]   | `errors.KeyringLocked`       | exception      | backend locked or unavailable |
|  [03]   | `errors.InitError`           | exception      | backend initialization failed |
|  [04]   | `errors.PasswordSetError`    | exception      | set operation failed          |
|  [05]   | `errors.PasswordDeleteError` | exception      | delete operation failed       |
|  [06]   | `errors.NoKeyringError`      | exception      | no viable backend found       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level facade
- rail: secrets

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]  | [RAIL]                          |
| :-----: | :--------------------------------------------------------------------- | :-------------- | :------------------------------ |
|  [01]   | `get_password(service_name, username) -> str \| None`                  | secret read     | retrieve password or `None`     |
|  [02]   | `set_password(service_name, username, password) -> None`               | secret write    | store password string           |
|  [03]   | `delete_password(service_name, username) -> None`                      | secret delete   | remove stored password          |
|  [04]   | `get_credential(service_name, username: str \| None) -> Credential \| None` | structured read | retrieve username+password pair (None username = backend-default user) |
|  [05]   | `get_keyring() -> KeyringBackend`                                      | backend query   | return the active backend instance |
|  [06]   | `set_keyring(keyring) -> None`                                         | backend set     | override the active backend (test boundary) |

[ENTRYPOINT_SCOPE]: backend discovery and selection
- rail: secrets

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]  | [RAIL]                                            |
| :-----: | :------------------------------------------------- | :-------------- | :------------------------------------------------ |
|  [01]   | `core.init_backend(limit=None) -> KeyringBackend`  | backend init    | pick the highest-priority viable backend (optionally filtered by `limit`) |
|  [02]   | `core.load_env() -> KeyringBackend \| None`        | env select      | resolve `PYTHON_KEYRING_BACKEND` to a backend     |
|  [03]   | `core.load_config() -> KeyringBackend \| None`     | config select   | resolve `keyringrc.cfg` to a backend              |
|  [04]   | `core.load_keyring(keyring_name) -> KeyringBackend` | name resolve   | import-load a backend by dotted class name        |
|  [05]   | `core.recommended(backend) -> bool`                | viability       | predicate: backend has positive (recommended) priority |
|  [06]   | `core.disable() -> None`                           | backend disable | write config selecting the fail (null) backend    |
|  [07]   | `backend.get_all_keyring() -> list[KeyringBackend]` | backend list   | enumerate all registered backends                 |
|  [08]   | `backend.by_priority`                              | sort key        | `attrgetter('priority')` ranking key over backends |

[ENTRYPOINT_SCOPE]: `KeyringBackend` abstract/extension interface
- rail: secrets
- defined on `backend.KeyringBackend` (PUBLIC_TYPES [01]); custom stores subclass and implement the abstract reads/writes plus the `priority` ranking.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY]   | [RAIL]                              |
| :-----: | :-------------------------------------------------------------- | :--------------- | :---------------------------------- |
|  [01]   | `get_password(service, username) -> str \| None`                | abstract method  | read secret from store              |
|  [02]   | `set_password(service, username, password) -> None`             | abstract method  | write secret to store               |
|  [03]   | `delete_password(service, username) -> None`                    | abstract method  | remove secret from store            |
|  [04]   | `get_credential(service, username) -> Credential \| None`       | method           | read structured credential          |
|  [05]   | `priority` (class property)                                     | viability rank   | float rank; raises `NotImplementedError` on the base, must be overridden |
|  [06]   | `viable` (class property)                                       | viability gate   | whether the backend's deps/platform are present |
|  [07]   | `get_viable_backends() -> set[type[KeyringBackend]]`            | discovery        | viable subclasses ranked by priority |
|  [08]   | `with_properties(**kwargs)` / `set_properties_from_env()`       | configure        | clone with overrides / hydrate props from env |

## [04]-[IMPLEMENTATION_LAW]

[SECRETS_TOPOLOGY]:
- facade-dispatch law: `get_password`/`set_password`/`delete_password`/`get_credential` delegate to the active backend resolved through `get_keyring()`; consuming code never constructs a backend directly.
- selection-precedence law: backend selection follows `load_env()` (`PYTHON_KEYRING_BACKEND`) -> `load_config()` (`keyringrc.cfg`) -> `init_backend()` (highest-priority viable). `recommended(backend)` (positive priority) gates the auto-selected default; `ChainerBackend` cascades through all viable backends in priority order when configured.
- priority law: each backend declares a float `priority`; the base raises `NotImplementedError`, forcing every concrete store to rank itself. `get_all_keyring()`/`get_viable_backends()` rank by `by_priority` descending — macOS Keychain > SecretService/KWallet (Linux) > Windows Credential Manager > chainer > fail/null.
- viability law: a backend's `viable` property and `ExceptionTrap` gate whether its platform deps are present; a backend that raises during viability probing is trapped and excluded, never crashing discovery.
- missing-vs-error law: `get_password`/`get_credential` return `None` for a missing secret and never raise; `delete_password` raises `PasswordDeleteError` when the entry is absent; `AnonymousCredential.username` raises `ValueError` (access only `.password`).

[LOCAL_ADMISSION]:
- secret retrieval at process startup uses `get_password`/`get_credential` via the top-level facade; the backend is resolved once and never constructed directly in consuming code.
- `set_keyring`/`init_backend(limit=...)` are test-boundary tools for injecting or constraining a controlled backend; production code selects via env/config, not by mutating the active backend in-process.
- `errors.NoKeyringError` is the expected failure in headless/container environments with no native keystore; the secret-admission rail lifts it to a typed boundary fault and falls back to the explicitly-configured source (env/config), never a silent plaintext default.

[STACK_LAW]:
- `get_credential(service, None)` -> `Credential` (`username`/`password`) -> minted once into an `httpx.BasicAuth`/`DigestAuth` (the `.api/httpx.md` `Auth` flow) at client construction: the secret crosses into the HTTP transport once, never re-read per request, never inlined.
- `pydantic-settings` reads the keyring-sourced secret into a `SecretStr` field through a custom settings source backed by `get_password`; the settings model is the single admission point, keyring is the secret store behind it.
- `EnvironCredential` is the explicit env-var fallback when `NoKeyringError` fires in a headless lane — a declared source, not an ad-hoc `os.environ` read scattered through domain code.

[RAIL_LAW]:
- Package: `keyring`
- Owns: platform-backed secret storage/retrieval, priority-ranked backend discovery, env/config backend selection, and the `KeyringBackend` extension point
- Accept: `get_password`/`set_password`/`delete_password`/`get_credential` facade calls; `init_backend`/`load_env`/`load_config`/`load_keyring` selection; `recommended`/`by_priority`/`get_viable_backends` ranking; `KeyringBackend` subclassing with an explicit `priority`; the keyring-sourced `Credential` minted once into the httpx `Auth`/pydantic-settings rail
- Reject: storing raw secrets in env vars or config files when a viable keyring exists; constructing backends directly in domain code; re-reading the secret per request instead of minting it once into the auth surface; mutating the active backend in production via `set_keyring`
