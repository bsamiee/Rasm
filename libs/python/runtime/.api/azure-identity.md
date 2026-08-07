# [PY_RUNTIME_API_AZURE_IDENTITY]

`azure-identity` mints the `TokenCredential` every Azure data-plane client authenticates through — the credential-chain owner behind the Azure Key Vault `SecretTier.cloud` arm. The runtime composes exactly one member on the resolve path: `DefaultAzureCredential`, whose chain resolves ambient workload identity (environment, workload identity federation, managed identity, shared cache, CLI) inside its own construction, which is what lets the admission boundary hold "no rasm code reads `os.environ` after admission" while the SDK's own credential legs read theirs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `azure-identity`
- package: `azure-identity` (MIT)
- module: `azure.identity`
- owner: `runtime`
- rail: settings/secrets — the `TokenCredential` leg of the `SECRET_LADDER` cloud rung's Azure arm
- depends: `azure-core` (the `TokenCredential`/`AccessToken` protocol shapes), `msal`/`msal-extensions` (the token broker underneath)
- capability: ambient credential-chain resolution (`DefaultAzureCredential`), single-leg credentials (managed identity, workload identity federation, environment, CLI, certificate/secret service principals), an explicit `ChainedTokenCredential` composer, and the typed unavailability family

## [02]-[CREDENTIALS]

[ENTRYPOINT_SCOPE]: credential construction and the token read

| [INDEX] | [MEMBER]                                                            | [KIND]    | [ROLE]                                                 |
| :-----: | :------------------------------------------------------------------ | :-------- | :----------------------------------------------------- |
|  [01]   | `DefaultAzureCredential(**kwargs)`                                  | ctor      | ordered ambient chain; `exclude_*` kwargs prune legs   |
|  [02]   | `ManagedIdentityCredential(client_id, identity_config)`             | ctor      | in-cluster or VM identity leg alone                    |
|  [03]   | `WorkloadIdentityCredential(tenant_id, client_id, token_file_path)` | ctor      | federated workload identity (k8s token file)           |
|  [04]   | `ChainedTokenCredential(*credentials)`                              | ctor      | explicit ordered chain overriding the default order    |
|  [05]   | `get_token(*scopes, claims, tenant_id, enable_cae)`                 | protocol  | `azure-core` `TokenCredential` read every client calls |
|  [06]   | `CredentialUnavailableError`                                        | exception | empty leg; subclasses `ClientAuthenticationError`      |
|  [07]   | `ClientAuthenticationError` (`azure.core.exceptions`)               | exception | material present, authentication refused               |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- the credential IS the injection value: a data-plane client (`SecretClient(vault_url, credential)`) and the pydantic-settings `AzureKeyVaultSettingsSource(settings_cls, url=, credential=)` both take the `TokenCredential`, never a pre-built client — the credential is the one seam the two legs share.
- `DefaultAzureCredential` resolves its chain lazily at the first `get_token`, and an exhausted chain raises `CredentialUnavailableError` (a `ClientAuthenticationError` subclass), so one `except ClientAuthenticationError` arm covers both the no-material and the refused-material states at the fault seam; neither subclasses `OSError` — the retry row that rides the resolve names the Azure transport transients (`ServiceRequestError`/`ServiceResponseError` families) by dotted spelling.
- credential state binds per composition, never process-global — `libs/python/.planning/RULINGS.md` `[05]-[PROCESS]` — so the ladder constructs the credential beside the client per read arm and memoizes neither.

[STACKING]:
- `azure-keyvault-secrets`(`.api/azure-keyvault-secrets.md`): the one consuming client — `execution/admission#ADMISSION` `CloudVault.read`'s Azure arm constructs `SecretClient(vault_url, DefaultAzureCredential())` per resolve; the declared-field twin injects the same credential into `AzureKeyVaultSettingsSource`.
- `reliability/resilience`(`runtime/.planning/reliability/resilience.md`): the `RetryClass.SECRET` row carries the Azure transport transients by module-qualified spelling at the BASE tier; `CredentialUnavailableError` is NOT transient — absent material never heals inside a retry window.

[RAIL_LAW]:
- Package: `azure-identity`
- Owns: Azure credential-chain resolution, the single-leg credential ctors, the explicit chain composer, and the typed unavailability family
- Accept: `DefaultAzureCredential` as the ladder's ambient chain; a single-leg ctor where a deployment pins one identity; the credential handed to `SecretClient`/`AzureKeyVaultSettingsSource` as the shared seam value
- Reject: a process-global memoized credential (per-composition binding rules it), a hand-rolled token acquisition over `msal` where the chain owns it, `CredentialUnavailableError` in a retry target, and a pre-built `SecretClient` injected where the settings source takes the credential
