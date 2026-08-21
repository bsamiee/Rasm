# [PY_RUNTIME_API_AZURE_KEYVAULT_SECRETS]

`azure-keyvault-secrets` closes the `SecretTier.cloud` provider family beside the GCP and Vault arms with the Azure Key Vault read client: a `TokenCredential`-authenticated `SecretClient` (sync and `aio` twin) whose versioned `get_secret` read lifts to `SecretStr`, over the `azure.core.exceptions` `AzureError` taxonomy mapping every Key Vault HTTP status onto the resolution ladder. Runtime code consumes it on two legs off one credential: declared model fields through the branch-catalogued `AzureKeyVaultSettingsSource`, per-service ladder credentials through `CloudVault.read`'s direct call-time `get_secret` — the one fence a direct call is seated at.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `azure-keyvault-secrets`
- package: `azure-keyvault-secrets` (MIT)
- module: `azure.keyvault.secrets`
- namespaces: `azure.keyvault.secrets`, `azure.keyvault.secrets.aio`
- rail: secrets

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client family + credential contract
- `SecretClient` accepts any `TokenCredential`; the concrete `ManagedIdentityCredential`/`DefaultAzureCredential` impls live in the companion `azure-identity`, deployment-resolved as ADC backs the GCP arm.

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :--------------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `SecretClient`                           | client        | sync client; the ladder's direct read arm                   |
|  [02]   | `aio.SecretClient`                       | client        | asyncio twin for a native-async read leg                    |
|  [03]   | `azure.core.credentials.TokenCredential` | protocol      | accepted `credential=` contract; `get_token` token provider |

[PUBLIC_TYPE_SCOPE]: secret + metadata model graph
- Read leg consumes `KeyVaultSecret`/`SecretProperties`; `DeletedSecret` and `ContentType` sit on the admin/soft-delete surface the runtime never mints.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `KeyVaultSecret`           | payload       | carries `.value: str`, `.id`, `.name`, `.properties`        |
|  [02]   | `SecretProperties`         | metadata      | `version`, `content_type`, `enabled`, `expires_on`, `tags`  |
|  [03]   | `KeyVaultSecretIdentifier` | parser        | splits a secret-id URL into `vault_url`/`name`/`version`    |
|  [04]   | `ApiVersion`               | enum          | service API-version selector, pinned at client construction |

[PUBLIC_TYPE_SCOPE]: exception taxonomy
- every arm derives from `azure.core.exceptions.AzureError`; `HttpResponseError.status_code` carries the Key Vault HTTP status, `ResourceNotFoundError` the typed 404 MISS arm.
- `ServiceRequestTimeoutError` subclasses `ServiceRequestError` and `ServiceResponseTimeoutError` subclasses `ServiceResponseError`, so the retry row's two dotted spellings already cover all four arms through the MRO and a timeout row duplicates one of them.
- Key Vault spells a 429 throttle with NO named class — it arrives as a bare `HttpResponseError` carrying the code on `status_code` — so a name-matched retry target never reaches it and it surfaces as a hard boundary fault, unlike the Vault `RateLimitExceeded` and GCP `TooManyRequests` arms.

| [INDEX] | [SYMBOL]                      | [STATUS] | [CAPABILITY]                                               |
| :-----: | :---------------------------- | :------: | :--------------------------------------------------------- |
|  [01]   | `AzureError`                  |    —     | base; carries `message`/`response`/`inner_exception`       |
|  [02]   | `HttpResponseError`           | 4xx/5xx  | HTTP fault carrier; `status_code` maps the arm             |
|  [03]   | `ResourceNotFoundError`       |   404    | absent secret or version — the MISS arm                    |
|  [04]   | `ClientAuthenticationError`   | 401/403  | credential rejected or policy denies — hard boundary fault |
|  [05]   | `ServiceRequestError`         |    —     | request never reached the service — a transient            |
|  [06]   | `ServiceResponseError`        |    —     | response lost after the request landed — a transient       |
|  [07]   | `ServiceRequestTimeoutError`  |    —     | request-side deadline — a transient the retry rides        |
|  [08]   | `ServiceResponseTimeoutError` |    —     | response-side deadline — a transient                       |
|  [09]   | `TooManyRedirectsError`       |    —     | redirect ceiling exceeded                                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction

| [INDEX] | [SURFACE]                                       | [SHAPE] | [CAPABILITY]                                             |
| :-----: | :---------------------------------------------- | :------ | :------------------------------------------------------- |
|  [01]   | `SecretClient(vault_url, credential, **kwargs)` | ctor    | `TokenCredential`-authenticated; `api_version=` pinnable |
|  [02]   | `aio.SecretClient(vault_url, credential)`       | ctor    | asyncio twin for a native-async resolve leg              |
|  [03]   | `KeyVaultSecretIdentifier(source_id)`           | ctor    | secret-id URL → `vault_url`/`name`/`version` parts       |

[ENTRYPOINT_SCOPE]: secret read
- `get_secret` is the one polymorphic entry over `name=`/`version=`; the value lands at `.value`, the metadata at `.properties`.

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :---------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `client.get_secret(name, version=None)`         | instance | latest when `version=None`; returns `KeyVaultSecret` |
|  [02]   | `secret.value`                                  | property | the resolved secret string lifted to `SecretStr`     |
|  [03]   | `await async_client.get_secret(name, version=)` | instance | asyncio twin for a native-async resolve leg          |

[ENTRYPOINT_SCOPE]: client release
- unlike the GCP client, `SecretClient` carries BOTH a bare `close()` and the context-manager pair, so the bracket is the spelling and the bare call is the escape a non-lexical lifetime needs.

| [INDEX] | [SURFACE]                  | [SHAPE] | [CAPABILITY]                                       |
| :-----: | :------------------------- | :------ | :------------------------------------------------- |
|  [01]   | `client.close()`           | release | retires the `azure-core` pipeline; returns `None`  |
|  [02]   | `__enter__` / `__exit__`   | bracket | the `with` form the per-read arm takes             |
|  [03]   | `__aenter__` / `__aexit__` | bracket | `aio.SecretClient` twin, closed by `await close()` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- consume law: TWO legs off one credential. Declared model fields fold through `AzureKeyVaultSettingsSource(settings_cls, url=, credential=)`, which takes the `TokenCredential` itself and never a pre-built `SecretClient`; per-service `(service, username)` ladder credentials resolve at call time through `CloudVault.read`'s direct `SecretClient(vault_url, credential).get_secret(...)`, a read no construction-time source serves. This boundary reads secrets and never mints, rotates, or deletes them.
- ladder law: `SecretTier.cloud` carries `vault_url` in one `execution/admission#SETTINGS` `TierRow(SecretTier.CLOUD, Some(Feature.SECRET_MANAGER))` beside the GCP and Vault rows, gated by `Feature.SECRET_MANAGER`/`Killswitch.DISABLE_SECRET_MANAGER`; `RetryClass.SECRET` spells once at the dispatch applying it, never as a ladder column repeating one value per rung, so a transiently-unreachable vault retries inside one derivation span.
- miss-vs-fault law: `ResourceNotFoundError` (404) is a MISS the ladder walks to the next tier, matching the GCP `NotFound` and Vault `InvalidPath` arms; `ServiceRequestError`/`ServiceResponseError` are the two transients the `RetryClass.SECRET` backoff names, carrying both timeout subclasses through the MRO; `ClientAuthenticationError` and the unnamed 429 are hard boundary faults the `guarded` envelope surfaces, never a silent empty read.
- release law: the per-read arm brackets `with SecretClient(vault_url, credential) as client:` so the `azure-core` pipeline retires on every exit path, and the credential brackets with it under the same `with` — a client or credential left for GC strands its connection pool across every ladder walk, and the async twin closes through `await client.close()` under `async with`.
- credential law: authentication is a deployment-resolved `TokenCredential` (managed or workload identity, or a `DefaultAzureCredential` chain from `azure-identity`), never inline secret material; the resolved secret crosses as `SecretStr`, and a per-tenant `vault_url` scopes multi-tenant reads so one admitted boundary serves every app shape without a shared mutable client.

[STACKING]:
- `pydantic-settings`(`.api/pydantic-settings.md` PUBLIC_TYPES [12]): `AzureKeyVaultSettingsSource(settings_cls, url=, credential=)` is the declared-field fence — the `TokenCredential` crosses as `credential=` (probed live: the source takes the credential, not a built client), the source folds into `settings_customise_sources`; the per-service ladder read stays `CloudVault.read`'s.
- resilience leg: the cloud-tier probe rides the `reliability/resilience` `guarded(RetryClass.SECRET, ...)` retried-traced-railed envelope as the GCP and Vault tiers do, the sync `SecretClient` offloaded through `anyio.to_thread.run_sync` because its blocking `azure-core` pipeline must never stall the loop; the `aio.SecretClient` twin is the native-async alternative.
- transport leg: the client's own `azure-core` HTTP pipeline is internal, distinct from the `.api/httpx.md` transport the runtime owns — the runtime never reaches into it.

[LOCAL_ADMISSION]:
- admits `SecretClient` construction, its `credential=` injection into `AzureKeyVaultSettingsSource`, and the `with` bracket retiring both handles on the read arm; `get_secret` and the async twin ride the source, never a scattered direct call.
- lazy import defers the `azure.keyvault.secrets`/`azure-core` stack to the gated arm's first fire, matching the `hvac` and `google-cloud-secret-manager` cold binds; the sync client offloads through `anyio.to_thread.run_sync` under `_PROBE_BAND`.
- `TokenCredential` resolution, `api_version` pinning, and pipeline retry/timeout defaults arrive settled from the client; this page owns only the read-slice the cloud-tier row consumes.

[RAIL_LAW]:
- Package: `azure-keyvault-secrets`
- Owns: the Azure Key Vault read client closing the cloud secret-resolution provider family beside the GCP and Vault arms
- Accept: one `vault_url`-scoped, `TokenCredential`-authenticated `SecretClient` whose `get_secret` lifts `.value` to `SecretStr`, the `with` bracket retiring client and credential together, its `credential=` injection into `AzureKeyVaultSettingsSource`, the `SecretTier.cloud` `TierRow` gated by `Feature.SECRET_MANAGER` and retried under `RetryClass.SECRET`, `ResourceNotFoundError` as a ladder MISS with `ServiceRequestError`/`ServiceResponseError` carrying their timeout subclasses as retried transients, the sync read offloaded through `anyio.to_thread.run_sync`
- Reject: a direct `get_secret` placed OUTSIDE the `CloudVault.read` arm or the settings source, an unbracketed client or credential left for GC where `close()` retires the pipeline, a named 429 arm the taxonomy does not define, the admin/soft-delete surface (`set_secret`/`begin_delete_secret`/`purge_deleted_secret`/`update_secret_properties`/`backup_secret`) the runtime does not own, inline credential material beside the `TokenCredential` resolution, a bare-`str` resolved secret beside `SecretStr`, a shared mutable process-global `SecretClient` colliding across tenants, a parallel cloud-secret owner beside the one `SecretTier.cloud` discrimination
