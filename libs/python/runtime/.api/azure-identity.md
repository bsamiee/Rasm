# [PY_RUNTIME_API_AZURE_IDENTITY]

`azure-identity` mints the `TokenCredential` every Azure data-plane client authenticates through — the credential-chain owner behind the Azure Key Vault `SecretTier.cloud` arm. Runtime composition binds exactly one member on the resolve path: `DefaultAzureCredential`, whose chain resolves ambient workload identity (environment, workload identity federation, managed identity, shared cache, CLI) inside its own construction, which is what lets the admission boundary hold "no rasm code reads `os.environ` after admission" while the SDK's own credential legs read theirs.

## [01]-[CREDENTIALS]

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
|  [08]   | `close()`                                                           | release   | retires each constructed leg's transport session       |
|  [09]   | `__enter__` / `__exit__`                                            | bracket   | `__enter__` yields the inner `ChainedTokenCredential`  |
|  [10]   | `aio` twin `__aenter__` / `await close()`                           | bracket   | async-chain release point                              |

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `TokenCredential` IS the injection value both legs take: a data-plane client (`SecretClient(vault_url, credential)`) and the pydantic-settings `AzureKeyVaultSettingsSource(settings_cls, url=, credential=)` each accept the credential and never a pre-built client, so the credential is the one boundary the two legs share.
- `DefaultAzureCredential` resolves its chain lazily at the first `get_token`, and an exhausted chain raises `CredentialUnavailableError` (a `ClientAuthenticationError` subclass), so one `except ClientAuthenticationError` arm covers both the no-material and the refused-material states at the fault boundary; neither subclasses `OSError` — the retry row that rides the resolve names the Azure transport transients (`ServiceRequestError`/`ServiceResponseError` families) by dotted spelling.
- credential state binds per composition, never process-global — `libs/python/.planning/RULINGS.md` `[05]-[PROCESS]` — so the ladder constructs the credential beside the client per read arm and memoizes neither.
- release law: `DefaultAzureCredential` carries `close()` and the context-manager pair, so per-resolve construction brackets BOTH handles under one `with` — a credential constructed inline as the client's argument has no name to close, and every managed-identity and CLI leg it built keeps its own transport session alive past the read that needed it.

[STACKING]:
- `azure-keyvault-secrets`(`.api/azure-keyvault-secrets.md`): the one consuming client — `execution/admission#SETTINGS` `CloudVault.read`'s Azure arm names the credential and brackets both handles per resolve (`with DefaultAzureCredential() as credential, SecretClient(vault_url, credential) as client:`), so the chain's transport dies with the read that built it; the declared-field twin injects the same credential into `AzureKeyVaultSettingsSource`, where the source's own lifetime holds it.
- `reliability/resilience`(`runtime/.planning/reliability/resilience.md`): the `RetryClass.SECRET` row carries the Azure transport transients by module-qualified spelling at the BASE tier; `CredentialUnavailableError` is NOT transient — absent material never heals inside a retry window.
