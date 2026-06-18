# [API_CATALOGUE] @dopplerhq/node-sdk

`@dopplerhq/node-sdk` supplies the `DopplerSDK` class — a single-entry facade that bundles typed service clients for every Doppler API surface: secrets, configs, environments, projects, service tokens, dynamic secrets, groups, integrations, and workplace management. Each service is a property of the SDK instance and calls the Doppler REST API over the underlying `HTTPLibrary` client with automatic retry.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@dopplerhq/node-sdk`
- package: `@dopplerhq/node-sdk`
- module: `@dopplerhq/node-sdk`
- asset: `DopplerSDK` facade, per-domain service classes, typed request/response interfaces, HTTP error classes
- rail: secrets

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: SDK entry family — rail: secrets

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]   | [DESCRIPTION]                                      |
| :-----: | :----------- | :-------------- | :------------------------------------------------- |
|   [1]   | `DopplerSDK` | class (default) | facade; properties are service instances           |
|   [2]   | `Config`     | type            | `{ accessToken?: string }` — SDK constructor input |

[PUBLIC_TYPE_SCOPE]: service class family — rail: secrets

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [OPERATIONS]                                                        |
| :-----: | :---------------------------- | :------------ | :------------------------------------------------------------------ |
|   [1]   | `SecretsService`              | service class | list, get, update, delete, download, names                          |
|   [2]   | `ConfigsService`              | service class | get, update, delete, list, create, unlock, clone, lock, trusted IPs |
|   [3]   | `EnvironmentsService`         | service class | list, create, get, rename, delete                                   |
|   [4]   | `ProjectsService`             | service class | list, create, get, update, delete                                   |
|   [5]   | `ServiceTokensService`        | service class | list, create, delete                                                |
|   [6]   | `DynamicSecretsService`       | service class | issueLease, revokeLease                                             |
|   [7]   | `ConfigLogsService`           | service class | get, list, rollback                                                 |
|   [8]   | `ActivityLogsService`         | service class | list, retrieve                                                      |
|   [9]   | `GroupsService`               | service class | get, update, list, create, delete, addMember, deleteMember          |
|  [10]   | `IntegrationsService`         | service class | get, update, delete, list, create                                   |
|  [11]   | `ServiceAccountsService`      | service class | list, create, get, update, delete                                   |
|  [12]   | `ServiceAccountTokensService` | service class | list, create, get, delete                                           |
|  [13]   | `WorkplaceService`            | service class | get, update                                                         |
|  [14]   | `AuthService`                 | service class | revoke, me                                                          |
|  [15]   | `AuditService`                | service class | getUser                                                             |

[PUBLIC_TYPE_SCOPE]: secrets response and request family — rail: secrets

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]    | [DESCRIPTION]                                                                                        |
| :-----: | :---------------------- | :--------------- | :--------------------------------------------------------------------------------------------------- |
|   [1]   | `SecretsListResponse`   | response type    | `{ secrets?: …; page?: number }`                                                                     |
|   [2]   | `SecretsGetResponse`    | response type    | `{ name?, value?: { raw?, computed?, note? } }`                                                      |
|   [3]   | `SecretsUpdateRequest`  | request type     | `{ project, config, secrets: { [name]: { originalName?, value? } } }`                                |
|   [4]   | `SecretsUpdateResponse` | response type    | updated secrets map                                                                                  |
|   [5]   | `DownloadResponse`      | response type    | serialized secrets payload (format-dependent)                                                        |
|   [6]   | `NamesResponse`         | response type    | `{ names?: string[] }`                                                                               |
|   [7]   | `Format`                | string enum type | `'json' \| 'dotnet-json' \| 'env' \| 'yaml' \| 'docker' \| 'env-no-quotes'`                          |
|   [8]   | `NameTransformer`       | string enum type | `'camel' \| 'upper-camel' \| 'lower-snake' \| 'tf-var' \| 'dotnet' \| 'dotnet-env' \| 'lower-kebab'` |

[PUBLIC_TYPE_SCOPE]: HTTP error class family — rail: secrets

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [DESCRIPTION]                   |
| :-----: | :-------------------- | :------------ | :------------------------------ |
|   [1]   | `BaseHTTPError`       | base class    | base for all HTTP status errors |
|   [2]   | `BadRequest`          | error class   | 400                             |
|   [3]   | `Unauthorized`        | error class   | 401                             |
|   [4]   | `Forbidden`           | error class   | 403                             |
|   [5]   | `NotFound`            | error class   | 404                             |
|   [6]   | `Conflict`            | error class   | 409                             |
|   [7]   | `TooManyRequests`     | error class   | 429; carries `retryAfter`       |
|   [8]   | `InternalServerError` | error class   | 500                             |
|   [9]   | `ServiceUnavailable`  | error class   | 503; carries `retryAfter`       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SDK construction and configuration — rail: secrets

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [DESCRIPTION]                                     |
| :-----: | :-------------------------------- | :------------- | :------------------------------------------------ |
|   [1]   | `new DopplerSDK({ accessToken })` | constructor    | creates SDK instance with service token           |
|   [2]   | `sdk.setAccessToken(token)`       | mutator        | updates the access token on the existing instance |
|   [3]   | `sdk.setBaseUrl(url)`             | mutator        | overrides the base URL (default: Doppler API)     |

[ENTRYPOINT_SCOPE]: secrets operations — rail: secrets

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RETURN]                                                                                                                            |
| :-----: | :--------------------------------------------- | :------------- | :---------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `sdk.secrets.list(project, config, opts?)`     | list           | `Promise<SecretsListResponse>`; `opts` includes `includeDynamicSecrets`, `dynamicSecretsTtlSec`, `secrets`, `includeManagedSecrets` |
|   [2]   | `sdk.secrets.get(project, config, name)`       | get one        | `Promise<SecretsGetResponse>`                                                                                                       |
|   [3]   | `sdk.secrets.update(input)`                    | bulk update    | `Promise<SecretsUpdateResponse>`; `input` is `SecretsUpdateRequest`                                                                 |
|   [4]   | `sdk.secrets.delete(project, config, name)`    | delete one     | `Promise<any>`                                                                                                                      |
|   [5]   | `sdk.secrets.download(project, config, opts?)` | bulk download  | `Promise<DownloadResponse>`; `opts.format` selects `Format`                                                                         |
|   [6]   | `sdk.secrets.updateNote(input)`                | set note       | `Promise<UpdateNoteResponse>`                                                                                                       |
|   [7]   | `sdk.secrets.names(project, config, opts?)`    | list names     | `Promise<NamesResponse>`                                                                                                            |

[ENTRYPOINT_SCOPE]: config and environment operations — rail: secrets

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RETURN]                              |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------ |
|   [1]   | `sdk.configs.get(project, config)`              | get            | `Promise<ConfigsGetResponse>`         |
|   [2]   | `sdk.configs.list(project, opts?)`              | list           | `Promise<ConfigsListResponse>`        |
|   [3]   | `sdk.configs.create(input)`                     | create branch  | `Promise<ConfigsCreateResponse>`      |
|   [4]   | `sdk.configs.update(input)`                     | rename         | `Promise<ConfigsUpdateResponse>`      |
|   [5]   | `sdk.configs.delete(input)`                     | delete         | `Promise<DeleteResponse>`             |
|   [6]   | `sdk.configs.clone(input)`                      | clone branch   | `Promise<CloneResponse>`              |
|   [7]   | `sdk.configs.lock(input)`                       | lock           | `Promise<LockResponse>`               |
|   [8]   | `sdk.configs.unlock(input)`                     | unlock         | `Promise<UnlockResponse>`             |
|   [9]   | `sdk.environments.list(project)`                | list           | `Promise<EnvironmentsListResponse>`   |
|  [10]   | `sdk.environments.create(input, project)`       | create         | `Promise<EnvironmentsCreateResponse>` |
|  [11]   | `sdk.environments.get(project, environment)`    | get            | `Promise<EnvironmentsGetResponse>`    |
|  [12]   | `sdk.environments.rename(input, project, env)`  | rename         | `Promise<RenameResponse>`             |
|  [13]   | `sdk.environments.delete(project, environment)` | delete         | `Promise<any>`                        |

## [4]-[IMPLEMENTATION_LAW]

[SDK_TOPOLOGY]:
- `DopplerSDK` is the sole public entry point; service classes (`SecretsService`, `ConfigsService`, etc.) extend `BaseService` and are accessible only as properties of the SDK instance.
- `BaseService` holds `baseUrl` and `httpClient` (`HTTPLibrary`); the HTTP layer retries by default (`retryAttempts`, `retryDelayMs`).
- Authentication uses a Bearer token (`Authorization: Bearer <token>`); the token is a Doppler service token, personal token, or CLI token set at construction or via `setAccessToken`.

[LOCAL_ADMISSION]:
- Service tokens are scoped to a single config; personal tokens and CLI tokens access the full API. Pass the token with the least required privilege.
- `sdk.secrets.download` with `format: 'env'` returns a dotenv-format string in `DownloadResponse`; the field shape depends on the format selected.
- `DynamicSecretsService.issueLease` requires `ttl_sec` and returns a lease `id` plus the resolved secret `value`; `revokeLease` uses the `slug` from the issued lease response.

[RAIL_LAW]:
- Package: `@dopplerhq/node-sdk`
- Owns: Doppler API access (secrets, configs, environments, projects, service tokens, dynamic secrets)
- Accept: `DopplerSDK` facade; `sdk.secrets.list` / `sdk.secrets.get` for secret resolution
- Reject: direct HTTP calls to the Doppler API when this package is admitted
