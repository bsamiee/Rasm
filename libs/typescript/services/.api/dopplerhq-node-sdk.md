# [API_CATALOGUE] @dopplerhq/node-sdk

`@dopplerhq/node-sdk` supplies the `DopplerSDK` class — a single-entry facade that bundles typed service clients for every Doppler API surface: secrets, configs, environments, projects, service tokens, dynamic secrets, groups, integrations, and workplace management. Each service is a property of the SDK instance and calls the Doppler REST API over the underlying `HTTPLibrary` client with automatic retry.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@dopplerhq/node-sdk`
- package: `@dopplerhq/node-sdk`
- module: `@dopplerhq/node-sdk`
- asset: `DopplerSDK` facade, per-domain service classes, typed request/response interfaces, HTTP error classes
- rail: secrets

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: SDK entry family — rail: secrets

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]   | [DESCRIPTION]                                      |
| :-----: | :----------- | :-------------- | :------------------------------------------------- |
|  [01]   | `DopplerSDK` | class (default) | facade; properties are service instances           |
|  [02]   | `Config`     | type            | `{ accessToken?: string }` — SDK constructor input |

[PUBLIC_TYPE_SCOPE]: service class family — rail: secrets

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [OPERATIONS]                                                        |
| :-----: | :---------------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `SecretsService`              | service class | list, get, update, delete, download, names                          |
|  [02]   | `ConfigsService`              | service class | get, update, delete, list, create, unlock, clone, lock, trusted IPs |
|  [03]   | `EnvironmentsService`         | service class | list, create, get, rename, delete                                   |
|  [04]   | `ProjectsService`             | service class | list, create, get, update, delete                                   |
|  [05]   | `ServiceTokensService`        | service class | list, create, delete                                                |
|  [06]   | `DynamicSecretsService`       | service class | issueLease, revokeLease                                             |
|  [07]   | `ConfigLogsService`           | service class | get, list, rollback                                                 |
|  [08]   | `ActivityLogsService`         | service class | list, retrieve                                                      |
|  [09]   | `GroupsService`               | service class | get, update, list, create, delete, addMember, deleteMember          |
|  [10]   | `IntegrationsService`         | service class | get, update, delete, list, create                                   |
|  [11]   | `ServiceAccountsService`      | service class | list, create, get, update, delete                                   |
|  [12]   | `ServiceAccountTokensService` | service class | list, create, get, delete                                           |
|  [13]   | `WorkplaceService`            | service class | get, update                                                         |
|  [14]   | `AuthService`                 | service class | revoke, me                                                          |
|  [15]   | `AuditService`                | service class | getUser                                                             |

[PUBLIC_TYPE_SCOPE]: secrets response and request family — rail: secrets

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]    | [DESCRIPTION]                                                                                        |
| :-----: | :---------------------- | :--------------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `SecretsListResponse`   | response type    | `{ secrets?: …; page?: number }`                                                                     |
|  [02]   | `SecretsGetResponse`    | response type    | `{ name?, value?: { raw?, computed?, note? } }`                                                      |
|  [03]   | `SecretsUpdateRequest`  | request type     | `{ project, config, secrets: { [name]: { originalName?, value? } } }`                                |
|  [04]   | `SecretsUpdateResponse` | response type    | updated secrets map                                                                                  |
|  [05]   | `DownloadResponse`      | response type    | serialized secrets payload (format-dependent)                                                        |
|  [06]   | `NamesResponse`         | response type    | `{ names?: string[] }`                                                                               |
|  [07]   | `Format`                | string enum type | `'json' \| 'dotnet-json' \| 'env' \| 'yaml' \| 'docker' \| 'env-no-quotes'`                          |
|  [08]   | `NameTransformer`       | string enum type | `'camel' \| 'upper-camel' \| 'lower-snake' \| 'tf-var' \| 'dotnet' \| 'dotnet-env' \| 'lower-kebab'` |

[PUBLIC_TYPE_SCOPE]: HTTP error class family — rail: secrets

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [DESCRIPTION]                   |
| :-----: | :-------------------- | :------------ | :------------------------------ |
|  [01]   | `BaseHTTPError`       | base class    | base for all HTTP status errors |
|  [02]   | `BadRequest`          | error class   | 400                             |
|  [03]   | `Unauthorized`        | error class   | 401                             |
|  [04]   | `Forbidden`           | error class   | 403                             |
|  [05]   | `NotFound`            | error class   | 404                             |
|  [06]   | `Conflict`            | error class   | 409                             |
|  [07]   | `TooManyRequests`     | error class   | 429; carries `retryAfter`       |
|  [08]   | `InternalServerError` | error class   | 500                             |
|  [09]   | `ServiceUnavailable`  | error class   | 503; carries `retryAfter`       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK construction and configuration — rail: secrets

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [DESCRIPTION]                                     |
| :-----: | :-------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `new DopplerSDK({ accessToken })` | constructor    | creates SDK instance with service token           |
|  [02]   | `sdk.setAccessToken(token)`       | mutator        | updates the access token on the existing instance |
|  [03]   | `sdk.setBaseUrl(url)`             | mutator        | overrides the base URL (default: Doppler API)     |

[ENTRYPOINT_SCOPE]: secrets operations — rail: secrets

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RETURN]                                                                                                                            |
| :-----: | :--------------------------------------------- | :------------- | :---------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `sdk.secrets.list(project, config, opts?)`     | list           | `Promise<SecretsListResponse>`; `opts` includes `includeDynamicSecrets`, `dynamicSecretsTtlSec`, `secrets`, `includeManagedSecrets` |
|  [02]   | `sdk.secrets.get(project, config, name)`       | get one        | `Promise<SecretsGetResponse>`                                                                                                       |
|  [03]   | `sdk.secrets.update(input)`                    | bulk update    | `Promise<SecretsUpdateResponse>`; `input` is `SecretsUpdateRequest`                                                                 |
|  [04]   | `sdk.secrets.delete(project, config, name)`    | delete one     | `Promise<any>`                                                                                                                      |
|  [05]   | `sdk.secrets.download(project, config, opts?)` | bulk download  | `Promise<DownloadResponse>`; `opts.format` selects `Format`                                                                         |
|  [06]   | `sdk.secrets.updateNote(input)`                | set note       | `Promise<UpdateNoteResponse>`                                                                                                       |
|  [07]   | `sdk.secrets.names(project, config, opts?)`    | list names     | `Promise<NamesResponse>`                                                                                                            |

[ENTRYPOINT_SCOPE]: config and environment operations — rail: secrets

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RETURN]                              |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `sdk.configs.get(project, config)`              | get            | `Promise<ConfigsGetResponse>`         |
|  [02]   | `sdk.configs.list(project, opts?)`              | list           | `Promise<ConfigsListResponse>`        |
|  [03]   | `sdk.configs.create(input)`                     | create branch  | `Promise<ConfigsCreateResponse>`      |
|  [04]   | `sdk.configs.update(input)`                     | rename         | `Promise<ConfigsUpdateResponse>`      |
|  [05]   | `sdk.configs.delete(input)`                     | delete         | `Promise<DeleteResponse>`             |
|  [06]   | `sdk.configs.clone(input)`                      | clone branch   | `Promise<CloneResponse>`              |
|  [07]   | `sdk.configs.lock(input)`                       | lock           | `Promise<LockResponse>`               |
|  [08]   | `sdk.configs.unlock(input)`                     | unlock         | `Promise<UnlockResponse>`             |
|  [09]   | `sdk.environments.list(project)`                | list           | `Promise<EnvironmentsListResponse>`   |
|  [10]   | `sdk.environments.create(input, project)`       | create         | `Promise<EnvironmentsCreateResponse>` |
|  [11]   | `sdk.environments.get(project, environment)`    | get            | `Promise<EnvironmentsGetResponse>`    |
|  [12]   | `sdk.environments.rename(input, project, env)`  | rename         | `Promise<RenameResponse>`             |
|  [13]   | `sdk.environments.delete(project, environment)` | delete         | `Promise<any>`                        |

## [04]-[IMPLEMENTATION_LAW]

[SDK_TOPOLOGY]:
- `DopplerSDK` is the sole public entry point; service classes (`SecretsService`, `ConfigsService`, etc.) extend `BaseService` and are accessible only as properties of the SDK instance.
- `BaseService` holds `baseUrl` and `httpClient` (`HTTPLibrary`); the HTTP layer retries by default (`retryAttempts`, `retryDelayMs`).
- Authentication uses a Bearer token (`Authorization: Bearer <token>`); the token is a Doppler service token, personal token, or CLI token set at construction or via `setAccessToken`.

[LOCAL_ADMISSION]:
- Service tokens are scoped to a single config; personal tokens and CLI tokens access the full API. Pass the token with the least required privilege.
- `sdk.secrets.download` with `format: 'env'` returns a dotenv-format string in `DownloadResponse`; the field shape depends on the format selected.
- `DynamicSecretsService.issueLease` requires `ttl_sec` and returns a lease `id` plus the resolved secret `value`; `revokeLease` uses the `slug` from the issued lease response.

[RUNTIME_RESOLUTION]:
- Consumed at RUNTIME by the `security/secret#SECRET_STORE` `SecretStore` `Doppler` arm — `sdk.secrets.get(project, config, name)` resolves one static secret and `sdk.secrets.download(project, config, { format: 'env' })` resolves the whole config as a dotenv block, both carried into `Config.redacted` so the value never enters a span or log.
- A TTL-leased secret rides `DynamicSecretsService.issueLease({ ttl_sec })` → the `SecretStore` `LeasedSecret` cache holds the resolved `value` under the returned lease `id` for `ttl_sec`, and `revokeLease(slug)` releases it; the lease expiry drives the `SubscriptionRef.changes` rotation edge so dependents re-resolve without a restart.
- A rotation invalidates the lease (expiry or explicit `revokeLease`) and the `SecretStore` re-runs `issueLease`/`secrets.get`, never a process restart.

[RAIL_LAW]:
- Package: `@dopplerhq/node-sdk`
- Owns: Doppler API access (secrets, configs, environments, projects, service tokens, dynamic secrets)
- Accept: `DopplerSDK` facade; `sdk.secrets.list` / `sdk.secrets.get` for secret resolution
- Reject: direct HTTP calls to the Doppler API when this package is admitted
