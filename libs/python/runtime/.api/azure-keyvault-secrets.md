# [PY_RUNTIME_API_AZURE_KEYVAULT_SECRETS]

`azure-keyvault-secrets` supplies the Azure Key Vault client closing the `SecretTier.cloud` provider family beside the GCP and Vault arms: one `SecretClient(vault_url=, credential=)` (with its `azure.keyvault.secrets.aio.SecretClient` asyncio twin) whose `get_secret(name, version=None)` reads a versioned `KeyVaultSecret` carrying `.value` and a `SecretProperties` metadata graph, a `KeyVaultSecretIdentifier` that splits a secret id URL into its `vault_url`/`name`/`version` parts, and an `azure.core.exceptions` failure taxonomy rooted at `AzureError` mapping every Key Vault HTTP status through `HttpResponseError.status_code`. It is the client backing the branch-catalogued `AzureKeyVaultSettingsSource` (`libs/python/runtime/.api/pydantic-settings.md` PUBLIC_TYPES [12]), the third cloud-tier backend behind the `execution/admission#SETTINGS` `SecretTier.cloud` discrimination, read once through the gated arm and lifted to `SecretStr`, never a bare `str`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `azure-keyvault-secrets`
- package: `azure-keyvault-secrets`
- import: `azure.keyvault.secrets`
- owner: `runtime`
- rail: secrets
- license: `MIT`
- namespaces: `azure.keyvault.secrets`, `azure.keyvault.secrets.aio`
- capability: versioned Key Vault secret reads over a `TokenCredential`-authenticated client, secret-id URL parsing into vault/name/version parts, the `KeyVaultSecret`/`SecretProperties` metadata graph, a sync + asyncio client twin, and the `azure.core.exceptions` status-mapped `AzureError` failure taxonomy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client family
- rail: secrets

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                                                  |
| :-----: | :----------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `SecretClient`     | client        | sync client; `AzureKeyVaultSettingsSource` accepts as `credential=` leg |
|  [02]   | `aio.SecretClient` | client        | asyncio client twin for a native-async read leg                         |

[PUBLIC_TYPE_SCOPE]: secret + metadata model graph
- rail: secrets
- Read leg needs only `KeyVaultSecret` and its `SecretProperties`; `DeletedSecret`, `ContentType`, and `ApiVersion` sit beside the admin/soft-delete surface the runtime never mints.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                                      |
| :-----: | :------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `KeyVaultSecret`           | payload       | carries `.value: str`, `.id`, `.name`, `.properties`        |
|  [02]   | `SecretProperties`         | metadata      | `version`, `content_type`, `enabled`, `expires_on`, `tags`  |
|  [03]   | `KeyVaultSecretIdentifier` | parser        | splits a secret id URL into `vault_url`/`name`/`version`    |
|  [04]   | `ApiVersion`               | enum          | service API-version selector, pinned at client construction |

[PUBLIC_TYPE_SCOPE]: credential contract
- rail: secrets
- `SecretClient` accepts any `azure.core.credentials.TokenCredential`; the concrete managed-identity and workload-identity implementations (`ManagedIdentityCredential`, `DefaultAzureCredential`) live in the companion `azure-identity` package, resolved by the deployment exactly as ADC backs the GCP arm.

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY] | [RAIL]                                                      |
| :-----: | :--------------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `azure.core.credentials.TokenCredential` | protocol      | accepted `credential=` contract; `get_token` token provider |

[PUBLIC_TYPE_SCOPE]: exception taxonomy
- rail: secrets
- every arm derives from `azure.core.exceptions.AzureError`; `HttpResponseError.status_code` carries the Key Vault HTTP status, and `ResourceNotFoundError` is the typed 404 MISS arm.

| [INDEX] | [SYMBOL]                      | [STATUS] | [RAIL]                                                     |
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
- rail: secrets

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                                                   |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `SecretClient(vault_url, credential, **kwargs)` | construct      | `TokenCredential`-authenticated; `api_version=` pinnable |
|  [02]   | `aio.SecretClient(vault_url, credential)`       | construct      | asyncio twin for a native-async resolve leg              |
|  [03]   | `KeyVaultSecretIdentifier(source_id)`           | parse          | secret id URL → `vault_url`/`name`/`version` parts       |

[ENTRYPOINT_SCOPE]: secret read
- rail: secrets
- `get_secret` is the one polymorphic entry over `name=`/`version=`; the value lands at `.value`, the metadata at `.properties`.

| [INDEX] | [SURFACE]                                       | [RAIL]                                               |
| :-----: | :---------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `client.get_secret(name, version=None)`         | latest when `version=None`; returns `KeyVaultSecret` |
|  [02]   | `secret.value`                                  | the resolved secret string lifted to `SecretStr`     |
|  [03]   | `await async_client.get_secret(name, version=)` | asyncio twin for a native-async resolve leg          |

## [04]-[IMPLEMENTATION_LAW]

[SECRET_TOPOLOGY]:
- consume law: the runtime does NOT call `get_secret` directly — it constructs one `SecretClient` (a deployment-resolved `TokenCredential`, managed identity by default) and injects it as the `credential=` leg into `AzureKeyVaultSettingsSource`, which folds each `RASM_PY_`-prefixed field's Key Vault secret into the settings source chain exactly as the GCP arm injects `secret_client=`. Admin/soft-delete surface (`set_secret`/`begin_delete_secret`/`begin_recover_deleted_secret`/`purge_deleted_secret`/`update_secret_properties`/`backup_secret`/`restore_secret_backup`) is never admitted — the runtime reads secrets, never mints, rotates, or deletes them.
- ladder law: the `execution/admission#SETTINGS` `SecretTier.cloud` case carries the Key Vault `vault_url` in one `TierRow(SecretTier(cloud=...), Some(Feature.SECRET_MANAGER), RetryClass.SECRET)` beside the GCP and Vault rows — gated by `Feature.SECRET_MANAGER` and its `Killswitch.DISABLE_SECRET_MANAGER`, retried under the shared `RetryClass.SECRET` `stamina` policy, so a transiently-unreachable vault retries inside one derivation span rather than failing the resolve on the first RPC fault.
- miss-vs-fault law: `ResourceNotFoundError` (404) is a MISS the ladder walks past to the next tier, matching the GCP `NotFound` and Vault `InvalidPath` arms; `ServiceRequestError`/`ServiceResponseError`/`ServiceRequestTimeoutError`/`ServiceResponseTimeoutError` are transients the `RetryClass.SECRET` backoff rides; `ClientAuthenticationError` is a hard boundary fault the `guarded` envelope surfaces, never a silently-empty read.
- credential law: authentication is a `TokenCredential` resolved by the deployment (managed identity, workload identity, or a `DefaultAzureCredential` chain from the companion `azure-identity`), never inline secret material in domain code; the resolved secret crosses as `SecretStr`, and a per-tenant `vault_url` scopes multi-tenant reads so one admitted boundary serves every app shape without a shared mutable client.

[INTEGRATION_STACK]:
- settings leg: `AzureKeyVaultSettingsSource(settings_cls, url=, credential=)` (`.api/pydantic-settings.md` PUBLIC_TYPES [12]) is the one consuming fence — the runtime builds the client's credential, the source folds it into `settings_customise_sources`, and the cloud tier probe reads the admitted settings field rather than a scattered direct call.
- resilience leg: the cloud-tier probe rides the `reliability/resilience#RESILIENCE` `guarded(RetryClass.SECRET, ...)` retried-traced-railed envelope exactly as the GCP and Vault tiers do, offloaded through `anyio.to_thread.run_sync` because the sync `SecretClient` is a blocking `azure-core` pipeline whose read must never stall the loop; the `aio.SecretClient` twin is the alternative native-async leg.
- transport leg: the client's own `azure-core` HTTP pipeline is the channel it manages internally; it is distinct from the `.api/httpx.md` transport the runtime owns — the runtime never reaches into the Key Vault client's pipeline.

[LOCAL_ADMISSION]:
- admission admits `SecretClient` construction and its `credential=` injection into `AzureKeyVaultSettingsSource`; `get_secret` and the async twin ride the source, never a direct scattered runtime call.
- lazy import defers the `azure.keyvault.secrets`/`azure-core` stack to the gated arm's first fire, matching the `hvac` and `google-cloud-secret-manager` cold-dependency binds on `execution/admission`; the sync client offloads through `anyio.to_thread.run_sync` under `_PROBE_BAND`.
- `TokenCredential` resolution, `api_version` pinning, and pipeline retry/timeout defaults arrive settled from the client and its credential; this page owns only the read-slice the cloud-tier ladder row consumes.

[RAIL_LAW]:
- Package: `azure-keyvault-secrets`
- Owns: the Azure Key Vault read client closing the cloud secret-resolution provider family beside the GCP and Vault arms
- Accept: one `SecretClient` (`vault_url`-scoped, `TokenCredential`-authenticated) whose `get_secret` read lifts `.value` to `SecretStr`, its `credential=` injection into `AzureKeyVaultSettingsSource`, the `SecretTier.cloud` Key Vault `TierRow` gated by `Feature.SECRET_MANAGER` and retried under `RetryClass.SECRET`, `ResourceNotFoundError` as a ladder MISS and the `ServiceRequestError`/`ServiceResponseError`/timeout arms as retried transients, the sync read offloaded through `anyio.to_thread.run_sync`
- Reject: a direct scattered `get_secret` call bypassing the settings-source fence, the admin/soft-delete surface (`set_secret`/`begin_delete_secret`/`purge_deleted_secret`/`update_secret_properties`/`backup_secret`) the runtime does not own, inline credential material beside the `TokenCredential` resolution, a bare-`str` resolved secret beside `SecretStr`, a shared mutable process-global `SecretClient` that collides across app tenants, a parallel cloud-secret owner beside the one `SecretTier.cloud` discrimination
