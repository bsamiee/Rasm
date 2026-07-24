# [TS_SECURITY_API_DOPPLERHQ_NODE_SDK]

`@dopplerhq/node-sdk` fronts the Doppler REST API as one zero-dependency node client, and `security/secret/doppler` admits its leased-fetch axis alone — `secrets` reads, the `dynamicSecrets` lease lifecycle, the `auth` probe.

Generated response types are all-optional and key on sample secret names, so a `Schema` decode gates every payload; each status subclass sets `statusCode` alone, so one fold owns the whole fault family.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@dopplerhq/node-sdk`
- package: `@dopplerhq/node-sdk` (MIT)
- module: dual ESM/CJS behind one `.` export map; `DopplerSDK` is both default and named export, and every service hangs off it as an `sdk.<service>` property
- runtime: node-only — `HTTPLibrary` drives node HTTP under its own `retryAttempts`/`retryDelayMs` transport retry, and the package pulls zero runtime dependencies
- rail: `security/secret` leased-secret provider, admitted in `secret/` alone

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client facade, the three admitted services, the download-shape vocabularies, and the collapsed fault carrier

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :---------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `DopplerSDK`            | class         | client facade every service hangs off as a property |
|  [02]   | `SecretsService`        | class         | `sdk.secrets` — the leased-fetch reads              |
|  [03]   | `DynamicSecretsService` | class         | `sdk.dynamicSecrets` — issue and revoke a lease     |
|  [04]   | `AuthService`           | class         | `sdk.auth` — service-token liveness and surrender   |
|  [05]   | `BaseHTTPError`         | class         | RFC 9457 problem detail carrying `statusCode`       |
|  [06]   | `Format`                | string-union  | `download` body encoding                            |
|  [07]   | `NameTransformer`       | string-union  | `download` key-case transform                       |

[PAYLOADS]: `SecretsListResponse` `DownloadResponse` `SecretsGetResponse` `NamesResponse` `MeResponse` `IssueLeaseResponse` `RevokeLeaseResponse`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct, probe, fetch, lease

Every `secrets` read keys on `(project, config)`; `includeDynamicSecrets` with `dynamicSecretsTtlSec` issues leases inline on `list` and `download`, `includeManagedSecrets` widens `list` and `names`, and `download` alone carries `format` and `nameTransformer`.

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `new DopplerSDK(Config)`                                     | ctor     | mint one client at `Layer` construction      |
|  [02]   | `sdk.setAccessToken(string)`                                 | instance | swap the token across every service in place |
|  [03]   | `sdk.setBaseUrl(string)`                                     | instance | retarget the API host                        |
|  [04]   | `auth.me() -> MeResponse`                                    | instance | probe token liveness at startup              |
|  [05]   | `auth.revoke(RevokeRequest)`                                 | instance | surrender the token on rotation              |
|  [06]   | `secrets.list(string, string, opts) -> SecretsListResponse`  | instance | primary read, leasing inline                 |
|  [07]   | `secrets.download(string, string, opts) -> DownloadResponse` | instance | whole config as a name-to-value env map      |
|  [08]   | `secrets.get(string, string, string) -> SecretsGetResponse`  | instance | one secret with `raw` and `computed` forms   |
|  [09]   | `secrets.names(string, string, opts) -> NamesResponse`       | instance | name enumeration driving a targeted refresh  |
|  [10]   | `dynamicSecrets.issueLease(IssueLeaseRequest)`               | instance | mint a lease over an explicit `ttl_sec`      |
|  [11]   | `dynamicSecrets.revokeLease(RevokeLeaseRequest)`             | instance | return a lease at teardown                   |

- `new DopplerSDK`: `Config.accessToken` is optional in the type, so an unset token constructs a client that faults only at first call.
- `dynamicSecrets.revokeLease`: keys the lease by `slug` while `IssueLeaseResponse` returns `id` and `expires_at`, and an inline `list`/`download` lease surfaces no identifier — the refresh window is the only reclaim path for those.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DopplerSDK` fans every service off one facade; `secrets`, `dynamicSecrets`, and `auth` carry custody, and Doppler administration is provisioning's concern.
- `BaseHTTPError` carries the whole problem detail (`type`/`title`/`detail`/`instance`/`statusCode`) and each status subclass sets `statusCode` alone, so a `statusCode` fold discriminates the family and an instance-of ladder buys nothing.
- Generated payload interfaces are all-optional and their secret keys are literal sample names, so the declared type models no real secret set and a `Schema` decode over the raw body is the trust gate every fetched value crosses.
- Doppler expires a lease server-side, so a refresh inside the TTL window with a teardown revoke owns lease lifetime; `HTTPLibrary` retry covers transport hiccups and nothing about the window.

[STACKING]:
- `effect`(`.api/effect.md`): `Effect.tryPromise` lifts each `Promise` call, `Match.value` folds `BaseHTTPError.statusCode` into the tagged fault set, `Config.redacted` sources the token, `Schema.decodeUnknown` brands each all-optional payload, `Schedule.fixed` drives the sub-lease refresh, `SubscriptionRef` publishes the rotating set, `Cache` de-dupes concurrent `(project, config)` refetches, and `Layer.scoped` binds the client with its `revokeLease` finalizer.
- `@effect/platform`(`.api/effect-platform.md`): the SDK owns its node transport, so the seam is the `Effect.tryPromise` boundary rather than `HttpClient`; a read demanding shared net policy or tracing runs `HttpClient.retryTransient` against the REST endpoint directly.
- `jose`(`.api/jose.md`): a fetched PEM or JWK string imports once through `importPKCS8`/`importSPKI`/`importJWK` into a non-extractable `CryptoKey` held for the `Layer` lifetime and `calculateJwkThumbprintUri` derives its `kid`, so a Doppler refresh re-imports the key where a per-call design re-imports every signature.
- `security/secret/doppler`: one leased fetch feeds `secret/material`, which hands `sign` its JWT keys, webhook HMAC secrets, and argon2 pepper at layer construction, so no downstream surface reaches Doppler itself.

[LOCAL_ADMISSION]:
- Admit `secrets`, `dynamicSecrets`, and `auth`; a Doppler administration call from a runtime folder routes to provisioning instead.
- Source `accessToken` from `Config.redacted` and wrap each decoded value in `Redacted` at first read.
- Lease through `includeDynamicSecrets` with `dynamicSecretsTtlSec`, refresh inside the window, and revoke in a `Scope` finalizer.
- Publish the fetched set through one `SubscriptionRef` every consumer observes.

[RAIL_LAW]:
- Package: `@dopplerhq/node-sdk`
- Owns: the Doppler fetch axis (`secrets` list/download/get/names), the `dynamicSecrets` lease lifecycle, the `auth` token probe, the generated payload shapes, and the collapsed `BaseHTTPError` fault family
- Accept: one `Tag`-held client on a `Config.redacted` token, `Effect.tryPromise` calls with `statusCode`-folded faults, TTL leasing refreshed inside its window and revoked on a `Scope` finalizer, `Schema` decode into `Redacted` values, `SubscriptionRef` publication, `Layer.scoped` lifetime
- Reject: the administration roster from a runtime folder, an instance-of ladder over the status subclasses, a lease with no refresh window or teardown revoke, a per-consumer refetch, trust in a generated payload type without a decode
