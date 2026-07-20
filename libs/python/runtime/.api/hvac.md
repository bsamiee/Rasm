# [PY_RUNTIME_API_HVAC]

`hvac` supplies the HashiCorp Vault client closing the `SecretTier.cloud` provider family beside the GCP arm: one `hvac.Client(url=, token=, namespace=, verify=)` whose `secrets.kv.v2.read_secret_version(path=, mount_point=)` reads a versioned KV-v2 payload, whose `auth.approle.login`/`auth.kubernetes.login` mint a deployment-portable token without an inline root secret, and whose `exceptions.VaultError` taxonomy maps every Vault HTTP status through `VaultError.from_status`. Vault is the second cloud-tier backend behind the `execution/admission#SETTINGS` `SecretTier.cloud` discrimination, read once through the gated arm and lifted to `SecretStr`, never a bare `str`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `hvac`
- package: `hvac`
- import: `hvac`
- owner: `runtime`
- rail: secrets
- license: Apache-2.0
- namespaces: `hvac`, `hvac.adapters`, `hvac.exceptions`, `hvac.api`, `hvac.constants`
- capability: versioned Vault KV-v1/KV-v2 secret reads, AppRole and Kubernetes token-mint auth legs, token self-lookup and authentication probe, per-request Vault namespace targeting, a pluggable request `Adapter`, and a status-mapped `VaultError` failure taxonomy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client + adapter family
- rail: secrets

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                                           |
| :-----: | :--------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `Client`               | client        | root client; `.secrets`/`.auth`/`.sys` engine and method routers |
|  [02]   | `adapters.Adapter`     | abstract base | request-adapter contract; custom transport extension point       |
|  [03]   | `adapters.JSONAdapter` | impl          | default adapter; JSON-decoded responses                          |
|  [04]   | `adapters.RawAdapter`  | impl          | raw `requests.Response` passthrough adapter                      |

[PUBLIC_TYPE_SCOPE]: secret-engine + auth-method routers
- rail: secrets
- `Client.secrets.kv` fans to the `v1`/`v2` engine twins (`default_kv_version = 2`); `Client.auth` fans to the token-mint legs.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                      |
| :-----: | :----------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `Client.secrets.kv.v2`   | engine        | KV-v2 versioned-secret read arm; the runtime's primary read |
|  [02]   | `Client.secrets.kv.v1`   | engine        | KV-v1 unversioned read arm                                  |
|  [03]   | `Client.auth.approle`    | auth method   | role-id + secret-id token mint, the headless-deployment leg |
|  [04]   | `Client.auth.kubernetes` | auth method   | service-account-JWT token mint, the in-cluster identity leg |
|  [05]   | `Client.auth.token`      | auth method   | `lookup_self`/`renew_self` over an already-mounted token    |

[PUBLIC_TYPE_SCOPE]: exception taxonomy
- rail: secrets
- every arm derives from `exceptions.VaultError`; `VaultError.from_status(status_code)` maps the Vault HTTP status to its arm, defaulting to `UnexpectedError`.

| [INDEX] | [SYMBOL]                         | [STATUS] | [RAIL]                                                      |
| :-----: | :------------------------------- | :------: | :---------------------------------------------------------- |
|  [01]   | `exceptions.VaultError`          |    —     | base; carries `errors`/`method`/`url`/`text`/`json` context |
|  [02]   | `exceptions.InvalidRequest`      |   400    | malformed request                                           |
|  [03]   | `exceptions.Unauthorized`        |   401    | missing or expired token                                    |
|  [04]   | `exceptions.Forbidden`           |   403    | policy denies the path                                      |
|  [05]   | `exceptions.InvalidPath`         |   404    | absent secret or unmounted engine — the MISS arm            |
|  [06]   | `exceptions.RateLimitExceeded`   |   429    | quota rejection — a transient the retry rides               |
|  [07]   | `exceptions.InternalServerError` |   500    | Vault-side fault                                            |
|  [08]   | `exceptions.VaultNotInitialized` |   501    | cluster not initialized                                     |
|  [09]   | `exceptions.BadGateway`          |   502    | upstream gateway fault — a transient                        |
|  [10]   | `exceptions.VaultDown`           |   503    | sealed or unreachable cluster — a transient                 |
|  [11]   | `exceptions.UnexpectedError`     | default  | unmapped status fallthrough                                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction
- rail: secrets

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :------------------------------------------------------ | :------------- | :----------------------------------------------- |
|  [01]   | `Client(url=, token=, namespace=, verify=, timeout=30)` | construct      | Enterprise `namespace=` targeting; TLS `verify=` |
|  [02]   | `Client(..., adapter=adapters.JSONAdapter)`             | construct      | adapter injection; `JSONAdapter` default         |
|  [03]   | `client.is_authenticated() -> bool`                     | probe          | token-mount check before the first read          |

[ENTRYPOINT_SCOPE]: secret read
- rail: secrets
- `read_secret_version` is the one polymorphic KV-v2 entry over `path=`/`version=`; the payload lands at `["data"]["data"]`.

| [INDEX] | [SURFACE]                                                                    | [RAIL]                            |
| :-----: | :--------------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `client.secrets.kv.v2.read_secret_version(path, version=None, mount_point=)` | head version; `["data"]["data"]`  |
|  [02]   | `client.secrets.kv.v2.read_secret(path, mount_point=)`                       | head-version shorthand            |
|  [03]   | `client.secrets.kv.v1.read_secret(path, mount_point=)`                       | KV-v1 read; payload at `["data"]` |

[ENTRYPOINT_SCOPE]: token mint + self-lookup
- rail: secrets
- one leg mints the token the read rides; `use_token=True` sets it on the client in place.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `client.auth.approle.login(role_id, secret_id=, mount_point=)` | token mint     | headless role auth; portable default leg |
|  [02]   | `client.auth.kubernetes.login(role, jwt, mount_point=)`        | token mint     | in-cluster service-account-JWT leg       |
|  [03]   | `client.auth.token.lookup_self(mount_point=)`                  | token query    | resolved token TTL and policy set        |

## [04]-[IMPLEMENTATION_LAW]

[SECRET_TOPOLOGY]:
- consume law: the runtime constructs one `Client`, mints a token through the gated auth leg (`approle.login` for a headless deployment, `kubernetes.login` for an in-cluster identity), reads the KV-v2 payload through `read_secret_version`, and projects its selected value through `SecretStr` into `BasicCredential`; the admin surface (`sys.enable_secrets_engine`, `secrets.kv.v2.create_or_update_secret`, policy CRUD) is never admitted — the runtime reads secrets, never mints or rotates them.
- ladder law: the `execution/admission#SETTINGS` `SecretTier.cloud` discriminant carries the Vault mount/namespace prefix in one `TierRow(SecretTier(cloud=...), Some(Feature.SECRET_MANAGER), RetryClass.SECRET)` beside the GCP row — gated by `Feature.SECRET_MANAGER` and its `Killswitch.DISABLE_SECRET_MANAGER`, retried under the shared `RetryClass.SECRET` `stamina` policy, so a sealed or rate-limited Vault retries inside one derivation span rather than failing the resolve on the first RPC fault.
- miss-vs-fault law: `InvalidPath` (404) is a MISS the ladder walks past to the next tier, matching the GCP `NotFound` arm; `VaultDown`/`BadGateway`/`RateLimitExceeded` are transients the `RetryClass.SECRET` backoff rides; `Forbidden`/`Unauthorized` are hard boundary faults the `guarded` envelope surfaces, never a silently-empty read.
- credential law: token material arrives from the auth leg's mint (AppRole secret-id or Kubernetes JWT resolved at construction), never an inline root token in domain code; the resolved secret crosses as `SecretStr`, and `namespace=` scopes multi-tenant Vault reads so one admitted boundary serves every app shape without a shared mutable client.

[INTEGRATION_STACK]:
- settings leg: the Vault read backs the `SecretTier.cloud` probe exactly as the GCP client backs its own — one client built at admission, the KV read folded into the `SECRET_LADDER` walk, the tier probe reading the admitted field rather than a scattered direct call.
- resilience leg: the cloud-tier probe rides the `reliability/resilience#RESILIENCE` `guarded(RetryClass.SECRET, ...)` retried-traced-railed envelope, offloaded through `anyio.to_thread.run_sync` because `hvac.Client` is a synchronous `requests`-backed client whose blocking read must never stall the loop.
- transport leg: `hvac.Client` manages its own `requests.Session` behind the `adapters.JSONAdapter`; it is distinct from the `.api/httpx.md` transport the runtime owns — the runtime never reaches into the Vault client's session, and the adapter extension point stays unused unless a deployment pins a custom transport.

[LOCAL_ADMISSION]:
- admission admits `Client` construction, one gated auth-leg mint, and the KV read whose payload lifts to `SecretStr`; `read_secret_version` and the auth legs ride the ladder row, never a direct scattered runtime call.
- lazy import defers the `hvac`/`requests` stack to the gated arm's first fire, matching the `lazy from google.cloud.secretmanager import ...` cold-dependency bind on `execution/admission`; the sync client offloads through `anyio.to_thread.run_sync` under `_PROBE_BAND`.
- TLS `verify`, per-request `namespace`, and token TTL arrive settled from the client and the auth leg; this page owns only the read-slice the cloud-tier ladder row consumes.

[RAIL_LAW]:
- Package: `hvac`
- Owns: the HashiCorp Vault read client closing the cloud secret-resolution provider family beside the GCP arm
- Accept: one `Client` (namespace-scoped, TLS-verified) whose token mints through `auth.approle.login`/`auth.kubernetes.login`, the KV-v2 `read_secret_version` read lifting `["data"]["data"]` to `SecretStr`, the `SecretTier.cloud` Vault `TierRow` gated by `Feature.SECRET_MANAGER` and retried under `RetryClass.SECRET`, `InvalidPath` as a ladder MISS and `VaultDown`/`BadGateway`/`RateLimitExceeded` as retried transients, the sync read offloaded through `anyio.to_thread.run_sync`
- Reject: a direct scattered `read_secret_version` call bypassing the ladder row, the admin surface (`sys.enable_secrets_engine`/`create_or_update_secret`/policy CRUD) the runtime does not own, an inline root token beside the auth-leg mint, a bare-`str` resolved secret beside `SecretStr`, a shared mutable process-global `Client` that collides across app tenants, a parallel cloud-secret owner beside the one `SecretTier.cloud` discrimination
