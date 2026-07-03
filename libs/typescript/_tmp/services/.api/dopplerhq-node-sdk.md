# [API_CATALOGUE] @dopplerhq/node-sdk

`@dopplerhq/node-sdk` is one default-exported facade — `DopplerSDK` — bundling 24 typed REST service clients as instance properties, each a `BaseService` calling the Doppler API over one shared `HTTPLibrary` (built-in retry). A Bearer access token (service, personal, or CLI token) authenticates every call. In the `security/secret#SECRET_STORE` design only the `secrets` and `dynamicSecrets` arms are load-bearing (static resolution + TTL leases); the remaining 22 services are the full config/access/identity/delivery surface a provisioning or governance path composes.

- package: `@dopplerhq/node-sdk`
- version: `1.3.0`
- license: `MIT`
- tier: `node` — server-side Bearer-token REST client; the browser owns no Doppler key and never reaches this surface.
- rail: secrets

## [01]-[PACKAGE_SURFACE]

`DopplerSDK` is the default export (`import DopplerSDK from "@dopplerhq/node-sdk"`); every service is a property, never separately importable. `Config` (`{ accessToken?: string }`) is the constructor input.

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [DESCRIPTION]                                              |
| :-----: | :----------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `new DopplerSDK({ accessToken })`    | constructor    | builds the facade + all 24 services from a Bearer token   |
|  [02]   | `sdk.setAccessToken(accessToken)`    | mutator        | rotates the token on the live instance (`void`)           |
|  [03]   | `sdk.setBaseUrl(url)`                | mutator        | overrides the API base URL (`void`; default Doppler API)  |

[SERVICE_MAP]: the 24 facade properties (exact property name → service class → operations), verified from `index.d.ts`:

| [INDEX] | [PROPERTY]              | [SERVICE_CLASS]               | [OPERATIONS]                                                                             |
| :-----: | :---------------------- | :---------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `secrets`               | `SecretsService`              | `list`, `update`, `get`, `delete`, `download`, `updateNote`, `names`                     |
|  [02]   | `dynamicSecrets`        | `DynamicSecretsService`       | `issueLease`, `revokeLease`                                                               |
|  [03]   | `configs`               | `ConfigsService`              | `get`, `update`, `delete`, `list`, `create`, `unlock`, `clone`, `lock`, `listTrustedIps`, `addTrustedIp`, `deleteTrustedIp` |
|  [04]   | `configLogs`            | `ConfigLogsService`           | `get`, `list`, `rollback`                                                                 |
|  [05]   | `environments`          | `EnvironmentsService`         | `list`, `create`, `get`, `rename`, `delete`                                               |
|  [06]   | `projects`              | `ProjectsService`             | `list`, `create`, `get`, `update`, `delete`                                               |
|  [07]   | `serviceTokens`         | `ServiceTokensService`        | `delete`, `list`, `create`                                                                |
|  [08]   | `serviceAccounts`       | `ServiceAccountsService`      | `get`, `update`, `delete`, `list`, `create`                                               |
|  [09]   | `serviceAccountTokens`  | `ServiceAccountTokensService` | `list`, `create`, `get`, `delete`                                                         |
|  [10]   | `auth`                  | `AuthService`                 | `revoke`, `me`                                                                            |
|  [11]   | `users`                 | `UsersService`                | `list`, `get`                                                                             |
|  [12]   | `invites`               | `InvitesService`              | `list`                                                                                    |
|  [13]   | `groups`                | `GroupsService`               | `get`, `update`, `delete`, `list`, `create`, `deleteMember`, `addMember`                  |
|  [14]   | `projectMembers`        | `ProjectMembersService`       | `list`, `add`, `get`, `update`, `delete`                                                  |
|  [15]   | `projectRoles`          | `ProjectRolesService`         | `get`, `update`, `delete`, `list`, `create`, `listPermissions`                            |
|  [16]   | `workplace`             | `WorkplaceService`            | `get`, `update`                                                                           |
|  [17]   | `workplaceRoles`        | `WorkplaceRolesService`       | `list`, `create`, `get`, `update`, `delete`, `listPermissions`                            |
|  [18]   | `integrations`          | `IntegrationsService`         | `get`, `update`, `delete`, `list`, `create`                                               |
|  [19]   | `syncs`                 | `SyncsService`                | `create`, `get`, `delete`                                                                 |
|  [20]   | `webhooks`              | `WebhooksService`             | `list`, `add`, `get`, `update`, `delete`, `enable`, `disable`                             |
|  [21]   | `get`                   | `GetService`                  | `options(integration)` — integration option catalogue                                    |
|  [22]   | `retrieve`              | `RetrieveService`             | `member(groupSlug, memberType, memberSlug)` — group-member lookup                         |
|  [23]   | `audit`                 | `AuditService`                | `getUser(workplaceUserId, { settings? })`                                                 |
|  [24]   | `activityLogs`          | `ActivityLogsService`         | `list`, `retrieve`                                                                        |

Each service also re-exports its type namespace (`SecretsModels`, `DynamicSecretsModels`, `ConfigsModels`, … via `index$N as <Name>Models`) plus flat request/response interfaces; catalog the flat names the design composes below.

## [02]-[SECRETS_AND_LEASE_SURFACE]

[SECRETS]: `sdk.secrets` — the static-resolution rail the `SecretStore` `Doppler` arm reads.

```ts
list(project: string, config: string, opts?: { accepts?: string; includeDynamicSecrets?: boolean; dynamicSecretsTtlSec?: number; secrets?: string; includeManagedSecrets?: boolean }): Promise<SecretsListResponse>
get(project: string, config: string, name: string): Promise<SecretsGetResponse>
update(input: SecretsUpdateRequest): Promise<SecretsUpdateResponse>
delete(project: string, config: string, name: string): Promise<any>
download(project: string, config: string, opts?: { format?: Format; nameTransformer?: NameTransformer; includeDynamicSecrets?: boolean; dynamicSecretsTtlSec?: number; secrets?: string }): Promise<DownloadResponse>
updateNote(input: UpdateNoteRequest): Promise<UpdateNoteResponse>
names(project: string, config: string, opts?: { includeDynamicSecrets?: boolean; includeManagedSecrets?: boolean }): Promise<NamesResponse>
```

- `SecretsGetResponse` = `{ name?: string; value?: { raw?: string; computed?: string; note?: string } }` — the `Doppler` arm reads `value.computed` (fallback `value.raw`).
- `SecretsListResponse` = `{ secrets?: Secrets$2 }` where `Secrets$2` is codegen example-shaped (`{ STRIPE?, ALGOLIA?, DATABASE?, USER? }`, each `{ raw?, computed?, note?, rawVisibility?, computedVisibility? }`). There is NO `page` field — treat the value map as a `Record<string, { raw?; computed?; note? }>` at the boundary.
- `DownloadResponse` is likewise codegen example-shaped (`{ STRIPE?: string; … }`); with `{ format: 'env' }` the wire body is a dotenv string parsed into `ConfigProvider.fromMap`, not the typed example record.
- `Format` = `'json' | 'dotnet-json' | 'env' | 'yaml' | 'docker' | 'env-no-quotes'`.
- `NameTransformer` = `'camel' | 'upper-camel' | 'lower-snake' | 'tf-var' | 'dotnet' | 'dotnet-env' | 'lower-kebab'`.

[DYNAMIC_LEASE]: `sdk.dynamicSecrets` — the TTL-lease rail. Both methods take ONE request object; the input dynamic-secret field is `dynamic_secret`, not `name`:

```ts
issueLease(input: IssueLeaseRequest): Promise<IssueLeaseResponse>
  // IssueLeaseRequest  = { project: string; config: string; dynamic_secret: string; ttl_sec: number }
  // IssueLeaseResponse = { success?: boolean; id?: string; expires_at?: string; value?: {} }
revokeLease(input: RevokeLeaseRequest): Promise<RevokeLeaseResponse>
  // RevokeLeaseRequest  = { project: string; config: string; dynamic_secret: string; slug: string }
  // RevokeLeaseResponse = { success?: boolean }
```

- The lease HANDLE for revocation is the caller-known `slug`, distinct from the response `id` — `IssueLeaseResponse` carries `id`/`expires_at`, NOT `slug`. `RevokeLeaseRequest.slug` is supplied by the caller's lease bookkeeping (the response `id`/`expires_at` drive the TTL sweep; the `slug` addresses the specific lease to revoke).
- `IssueLeaseResponse.value` is codegen-typed as an empty `Value$1 = {}`; the resolved credential arrives at runtime but is untyped, so read it as `Record<string, unknown>` / `unknown` at the `Effect.tryPromise` boundary and wrap in `Redacted.make` before it can widen.

## [03]-[ERROR_FAMILY]

The SDK does not expose a hand-picked error subset — it exports the COMPLETE RFC 7807 (`type`/`title`/`detail`/`instance`/`statusCode`) HTTP status-code hierarchy as one parameterized family: `BaseHTTPError extends Error` plus 39 `<StatusName> extends BaseHTTPError` subclasses (28 4xx + 11 5xx) spanning every code. Discriminate by `statusCode` (or `instanceof` the base), never by enumerating a fixed roster.

- Base: `BaseHTTPError` = `{ type?: string; title: string; detail?: string; instance?: string; statusCode: number }`. Every subclass sets `statusCode`/`title` and takes `constructor(detail?, …)`.
- Retry-bearing arms carry `retryAfter: number | null` — exactly `PayloadTooLarge` (413), `TooManyRequests` (429), `ServiceUnavailable` (503). Feed `retryAfter` into the backoff schedule.
- Challenge/allow-bearing arms: `Unauthorized.wwwAuthenticate` (401), `ProxyAuthenticationRequired.proxyAuthenticate` (407), `MethodNotAllowed.allow` (405).
- Full 4xx roster: `BadRequest` 400, `Unauthorized` 401, `PaymentRequired` 402, `Forbidden` 403, `NotFound` 404, `MethodNotAllowed` 405, `NotAcceptable` 406, `ProxyAuthenticationRequired` 407, `RequestTimeout` 408, `Conflict` 409, `Gone` 410, `LengthRequired` 411, `PreconditionFailed` 412, `PayloadTooLarge` 413, `UriTooLong` 414, `UnsupportedMediaType` 415, `RangeNotSatisfiable` 416, `ExpectationFailed` 417, `MisdirectedRequest` 421, `UnprocessableEntity` 422, `Locked` 423, `FailedDependency` 424, `TooEarly` 425, `UpgradeRequired` 426, `PreconditionRequired` 428, `TooManyRequests` 429, `RequestHeaderFieldsTooLarge` 431, `UnavailableForLegalReasons` 451.
- Full 5xx roster: `InternalServerError` 500, `NotImplemented` 501, `BadGateway` 502, `ServiceUnavailable` 503, `GatewayTimeout` 504, `HttpVersionNotSupported` 505, `VariantAlsoNegotiates` 506, `UnsufficientStorage` 507, `LoopDetected` 508, `NotExtended` 510, `NetworkAuthenticationRequired` 511.

## [04]-[IMPLEMENTATION_LAW]

[SDK_TOPOLOGY]:
- `DopplerSDK` is the sole entry; each service extends `BaseService` (`baseUrl`, `httpClient: HTTPLibrary`, private `accessToken`) and is reachable only as a facade property.
- `HTTPLibrary` retries by default (`retryAttempts`, `retryDelayMs` readonly; internal `retry(retries, callbackFn, delay?)`), so a single `sdk.*` call already absorbs transient 429/503 within the client — the outer Effect retry is the second, policy-owned tier.
- Scope the token to least privilege: a service token is bound to one config; personal/CLI tokens reach the full API.

[RUNTIME_RESOLUTION]: how the `security/secret#SECRET_STORE` `Doppler` arm stacks this into one Effect rail.
- Construction: the `DopplerSDK` is built once at the `SecretStore` layer from `new DopplerSDK({ accessToken: Redacted.value(dopplerToken) })`, the `dopplerToken` itself resolved through the ambient `ConfigProvider` (`Config.redacted("DOPPLER_TOKEN")`, the `Static` arm), never a bare `process.env`.
- Static secret: `sdk.secrets.get(project, config, name)` reads one secret's `value.computed`, wrapped as `Effect.tryPromise({ try, catch: (cause) => new SecretFault({ reason: "doppler_resolve", … }) })` and lifted into `Redacted.make` so the value never widens to a loggable string; `sdk.secrets.download(project, config, { format: 'env' })` bulk-loads the whole config as a dotenv payload parsed into `ConfigProvider.fromMap`.
- TTL lease: the dynamic sub-arm calls `sdk.dynamicSecrets.issueLease({ project, config, dynamic_secret, ttl_sec })` where `ttl_sec = Duration.toSeconds(LeasePolicy.Doppler.ttl)` — the `LeasePolicy` row IS the lease request. The resolved value seeds a `SubscriptionRef`-backed `LeasedSecret` under `expires_at`; the TTL-expiry sweep (`execution/backplane` `ScheduledWork.singleton`) calls `sdk.dynamicSecrets.revokeLease({ project, config, dynamic_secret, slug })` on the caller-tracked lease slug, and `rotate` re-runs `issueLease`/`secrets.get` pushing a new value through `SubscriptionRef.changes` so dependents re-resolve without a process restart.
- Error collapse: the whole `BaseHTTPError` hierarchy a `sdk.*` promise throws collapses at the one `Effect.tryPromise.catch` boundary into a single `SecretFault` reason; the caught `err.statusCode`/`err.retryAfter` are the fields the retry decision reads, so no per-status error class is caught individually.
- Every resolution runs inside the `persistence/tenancy#TENANCY` `withTenant` GUC scope and emits one `AuditReceipt`, so a Doppler read is RLS-scoped to `app.current_tenant` and recorded.

[SIBLING_STACK]:
- `@pulumi/esc-sdk` (`pulumi-esc-sdk.md`) is the peer `EscEnv` arm of the same `SecretRef` `Match.tagsExhaustive` fold; both resolve into one `Redacted.Redacted` and meet the deploy-time `provisioning/contract#PROVISIONING` `SecretResolver` at the one `ConfigProvider` boundary — this SDK is not a parallel secret scheme.
- `effect` owns the `Config.redacted`/`Redacted`/`ConfigProvider.fromMap` resolution carrier, the `SubscriptionRef` lease cell + `changes` rotation `Stream`, and the `Effect.tryPromise` boundary this SDK's promises cross.

[RAIL_LAW]:
- Package: `@dopplerhq/node-sdk`
- Owns: Doppler REST access — secrets, dynamic leases, configs, environments, projects, tokens, accounts, groups, integrations, syncs, webhooks, audit
- Accept: `DopplerSDK` facade built from a `Redacted`-wrapped token; `sdk.secrets.get`/`sdk.secrets.download` for static resolution and `sdk.dynamicSecrets.issueLease`/`revokeLease` for TTL leases, each wrapped in `Effect.tryPromise` → `SecretFault` and lifted into `Redacted`
- Reject: direct HTTP calls to the Doppler API; catching individual `BaseHTTPError` subclasses instead of collapsing them at the `tryPromise` boundary; reading a resolved value as a plain `string` instead of `Redacted.Redacted`; issuing a lease without tracking its `slug` for `revokeLease`
