# [PY_RUNTIME_API_HVAC]

`hvac` binds the HashiCorp Vault read client on the `SecretTier.cloud` family beside the GCP and Azure arms: one `hvac.Client(url=, token=, namespace=, verify=)` whose `secrets.kv.v2.read_secret_version` reads a versioned KV-v2 payload, whose `auth.approle.login`/`auth.kubernetes.login` mint a deployment-portable token without an inline root secret, and whose `exceptions.VaultError` taxonomy maps each Vault HTTP status through `VaultError.from_status`. `SecretTier.cloud` reads it once through the gated `execution/admission#SETTINGS` leg and lifts the value to `SecretStr`, never a bare `str`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `hvac`
- package: `hvac` (Apache-2.0)
- module: `hvac`
- rail: secrets
- namespaces: `hvac`, `hvac.adapters`, `hvac.exceptions`, `hvac.api`, `hvac.constants`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client + adapter family

- `Client` exposes NO `close`, `__enter__`, or `__exit__` — the only release seam is `client.adapter.close()`, which closes the adapter's own `requests.Session`.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `Client`                   | client        | root client; `.secrets`/`.auth`/`.sys` engine and method routers |
|  [02]   | `Client.adapter`           | property      | settable; the live adapter holding the HTTP session              |
|  [03]   | `adapters.Adapter`         | abstract base | request-adapter contract; custom transport extension point       |
|  [04]   | `adapters.Adapter.close()` | release       | closes the underlying `requests.Session`; the ONLY release seam  |
|  [05]   | `adapters.JSONAdapter`     | impl          | default adapter; JSON-decoded responses                          |
|  [06]   | `adapters.RawAdapter`      | impl          | raw `requests.Response` passthrough adapter                      |

[PUBLIC_TYPE_SCOPE]: secret-engine + auth-method routers
- `Client.secrets.kv` defaults to the v2 engine (`default_kv_version = 2`).

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :----------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `Client.secrets.kv.v2`   | engine        | KV-v2 versioned-secret read arm; the runtime's primary read |
|  [02]   | `Client.secrets.kv.v1`   | engine        | KV-v1 unversioned read arm                                  |
|  [03]   | `Client.auth.approle`    | auth method   | role-id + secret-id token mint, the headless-deployment leg |
|  [04]   | `Client.auth.kubernetes` | auth method   | service-account-JWT token mint, the in-cluster identity leg |
|  [05]   | `Client.auth.token`      | auth method   | `lookup_self`/`renew_self` over an already-mounted token    |

[PUBLIC_TYPE_SCOPE]: exception taxonomy
- every arm derives from `exceptions.VaultError`; `VaultError.from_status(status_code)` maps the Vault HTTP status to its arm, defaulting to `UnexpectedError`.

| [INDEX] | [SYMBOL]                         | [STATUS] | [CAPABILITY]                                                |
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

| [INDEX] | [SURFACE]                                               | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------ | :------ | :----------------------------------------------- |
|  [01]   | `Client(url=, token=, namespace=, verify=, timeout=30)` | ctor    | Enterprise `namespace=` targeting; TLS `verify=` |
|  [02]   | `Client(..., adapter=adapters.JSONAdapter)`             | ctor    | adapter injection; `JSONAdapter` default         |
|  [03]   | `client.is_authenticated() -> bool`                     | probe   | live token-validity check before the first read  |
|  [04]   | `client.adapter.close()`                                | release | the release seam; `Client` has no `close`        |

[ENTRYPOINT_SCOPE]: secret read
- `read_secret_version` is the one polymorphic KV-v2 entry over `path=`/`version=`; the payload lands at `["data"]["data"]`.
- `mount_point` defaults to `'secret'` on every KV arm, so an engine mounted anywhere else reads empty until the row names its mount.

| [INDEX] | [SURFACE]                                                                             | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------------------ | :-------------------------------- |
|  [01]   | `client.secrets.kv.v2.read_secret_version(path, version=None, mount_point='secret')`  | head version; `["data"]["data"]`  |
|  [02]   | `client.secrets.kv.v2.read_secret(path, mount_point='secret')`                        | head-version shorthand            |
|  [03]   | `client.secrets.kv.v1.read_secret(path, mount_point='secret')`                        | KV-v1 read; payload at `["data"]` |

[ENTRYPOINT_SCOPE]: token mint + self-lookup
- one leg mints the token the read rides; `use_token=True` sets it on the client in place.

| [INDEX] | [SURFACE]                                                      | [SHAPE]     | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------- | :---------- | :--------------------------------------- |
|  [01]   | `client.auth.approle.login(role_id, secret_id=, mount_point=)` | token mint  | headless role auth; portable default leg |
|  [02]   | `client.auth.kubernetes.login(role, jwt, mount_point=)`        | token mint  | in-cluster service-account-JWT leg       |
|  [03]   | `client.auth.token.lookup_self(mount_point=)`                  | token query | resolved token TTL and policy set        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- consume law: the runtime builds the `Client` inside `execution/admission#SETTINGS` `CloudVault.read` with the ADMITTED token (`vault_token: SecretStr` settings field — the Vault Agent / mounted-token-file pattern, composing with the `secrets_dir` source), reads the KV-v2 payload through `read_secret_version` at call time, and projects the selected value through `SecretStr` into `BasicCredential`; the rung folds OUT of the ladder when no token is admitted rather than defaulting; the admin surface (`sys.enable_secrets_engine`, `create_or_update_secret`, policy CRUD) stays unadmitted — the runtime reads secrets, never mints or rotates them.
- ladder law: `SecretTier.cloud` carries the Vault mount/namespace prefix in one `execution/admission#SETTINGS` `TierRow(SecretTier.CLOUD, Some(Feature.SECRET_MANAGER))` beside the GCP row, gated by `Feature.SECRET_MANAGER` and `Killswitch.DISABLE_SECRET_MANAGER`; `RetryClass.SECRET` spells once at the dispatch applying it, never as a ladder column carrying one value per rung, so a sealed or throttled Vault retries inside one derivation span.
- miss-vs-fault law: `InvalidPath` (404) is a MISS the ladder walks past, matching the GCP `NotFound` arm; `VaultDown`/`BadGateway`/`InternalServerError`/`RateLimitExceeded` are the four transients `RetryClass.SECRET` rides, each named by dotted spelling because the taxonomy is FLAT — every arm derives straight from `VaultError`, leaving no shared transient base to name instead; `Forbidden`/`Unauthorized` surface as hard boundary faults, never a silently-empty read.
- release law: `Client` carries no `close` and no context manager, so the per-read client releases through `client.adapter.close()` inside the same fence that built it; a `with hvac.Client(...)` is unspellable and a client left for GC strands its `requests.Session` pool across every ladder walk.
- credential law: token material is ADMITTED settings material — a `SecretStr` field the deployment fills, produced out-of-band by the AppRole secret-id or Kubernetes-JWT auth-leg mints (deployment-side origins, never run by this fence) and never an inline root token in domain code; `hvac.Client` at `token=None` falls back to `VAULT_TOKEN`/`~/.vault-token`, which is why an unadmitted token folds the rung out instead of constructing; the resolved secret crosses as `SecretStr`, and `namespace=` scopes multi-tenant reads so one admitted boundary serves every app shape without a shared mutable client.

[STACKING]:
- settings leg: the `Client` builds per read inside `CloudVault.read` with the admitted `vault_token`, its `read_secret_version` result folded into the `SECRET_LADDER` walk — the direct call IS the ladder's read arm, seated at the one fence.
- resilience leg: the cloud-tier probe rides the `reliability/resilience#RESILIENCE` `guarded(RetryClass.SECRET, ...)` envelope, offloaded through `anyio.to_thread.run_sync` because `hvac.Client` is a synchronous `requests`-backed client whose blocking read must never stall the loop.
- transport leg: `hvac.Client` manages its own `requests.Session` behind `adapters.JSONAdapter`, distinct from the `.api/httpx.md` transport the runtime owns; the runtime never reaches into the Vault session, and the `Adapter` extension point stays unbound unless a deployment pins a custom transport.

[LOCAL_ADMISSION]:
- admission admits `Client` construction, one gated auth-leg mint, the KV read whose payload lifts to `SecretStr`, and the `adapter.close()` release that ends it; `read_secret_version` and the auth legs ride the ladder row, never a scattered runtime call.
- lazy import defers the `hvac`/`requests` stack to the gated arm's first fire; the sync client offloads through `anyio.to_thread.run_sync` under `_PROBE_BAND`.
- TLS `verify`, per-request `namespace`, and token TTL arrive settled from the client and auth leg; this page owns only the read-slice the cloud-tier ladder row consumes.

[RAIL_LAW]:
- Package: `hvac`
- Owns: the HashiCorp Vault read client on the cloud secret-resolution provider family beside the GCP and Azure arms
- Accept: one `Client` (namespace-scoped, TLS-verified) whose token mints through `auth.approle.login`/`auth.kubernetes.login`, the KV-v2 `read_secret_version` read naming its `mount_point` against the `'secret'` default and lifting `["data"]["data"]` to `SecretStr`, `client.adapter.close()` as the release, the `SecretTier.cloud` Vault `TierRow` gated by `Feature.SECRET_MANAGER` and retried under `RetryClass.SECRET`, `InvalidPath` as a ladder MISS with `VaultDown`/`BadGateway`/`InternalServerError`/`RateLimitExceeded` as retried transients, the sync read offloaded through `anyio.to_thread.run_sync`
- Reject: a scattered `read_secret_version` call bypassing the ladder row, a `mount_point` left to the `'secret'` default for an engine mounted elsewhere, a `with`-bracketed `Client` the type supports no protocol for, a client left for GC where `adapter.close()` is the release, the admin surface (`sys.enable_secrets_engine`/`create_or_update_secret`/policy CRUD) the runtime does not own, an inline root token beside the auth-leg mint, a bare-`str` resolved secret beside `SecretStr`, a shared mutable process-global `Client` colliding across tenants, a parallel cloud-secret owner beside the one `SecretTier.cloud` discrimination
