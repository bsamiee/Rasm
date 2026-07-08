# [TS_SECURITY_API_DOPPLERHQ_NODE_SDK]

`@dopplerhq/node-sdk` is the Doppler REST client the `security/secret/doppler` design composes for leased-secret custody — a large auto-generated surface (`DopplerSDK` fronting ~23 sub-services) of which the design admits exactly ONE axis: `secrets` fetch (`list`/`download`/`get`/`names`) with dynamic-secret TTL leasing via the `dynamicSecretsTtlSec` parameter (Doppler default 1800s), plus the `dynamicSecrets.issueLease`/`revokeLease` lifecycle. The vast CRUD roster (projects, configs, environments, integrations, syncs, workplace, users, groups, invites, audit) is admitted-but-unused; a design that reached for it is re-implementing Doppler administration inside a runtime folder. The full HTTP-status error surface collapses to ONE `BaseHTTPError` problem-detail carrier (RFC 9457 `type`/`title`/`detail`/`instance`/`statusCode`) with 39 status subclasses as seed data — the design maps `statusCode`, never 39 members. Every fetched value is `Redacted` from the first decode; the access token enters as a `Config.redacted`; the lease TTL is Doppler-side, so the design refreshes the whole set on a `Schedule` slightly under the lease and revokes on teardown.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@dopplerhq/node-sdk`
- package: `@dopplerhq/node-sdk` (MIT, © DopplerSDK)
- module format: dual ESM/CJS (`type: module`, `main: dist/index.cjs`, `exports` map with `import`/`require`); the `DopplerSDK` client is the default and named export, services reached as `sdk.<service>` properties — no deep subpaths needed
- runtime target: node-only — the built-in `HTTPLibrary` transport uses node HTTP with its own retry (`retryAttempts`/`retryDelayMs`); no browser build, zero runtime dependencies (the client is self-contained), which sits inside `secret/`'s node-only custody boundary
- asset: pure-TypeScript runtime library (`.js` + `.d.ts`); every response interface is fully optional (`name?`/`value?`), so a `Schema` decode is the real gate before a fetched secret is trusted
- rail: `security/secret` — the leased-secret provider (admitted in `secret/` only; catalogued at the folder tier)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client, the admitted fetch/lease services, and their payloads
- rail: surfaces-and-dispatch

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:----------------------------------------------------------------- |:---------------- |:----------------------------------------------------------------- |
| [01] | `DopplerSDK` (default export) — ctor `{ accessToken }: Config`; `.setBaseUrl` / `.setAccessToken` | client | `secret/doppler` — the one client built once behind a `Context.Tag`; ~23 `.<service>` handles, of which only `secrets`/`dynamicSecrets`/`auth` are admitted |
| [02] | `SecretsService` (`sdk.secrets`) | fetch service | `secret/doppler` — the admitted axis: `list`/`download`/`get`/`names` reads; `update`/`delete` present but unused (secrets are provisioned in Doppler, not by the runtime) |
| [03] | `DynamicSecretsService` (`sdk.dynamicSecrets`) | lease service | `secret/doppler` — `issueLease`/`revokeLease` for the explicit dynamic-lease lifecycle when a fetch is not the leasing trigger |
| [04] | `SecretsListResponse` / `DownloadResponse` / `SecretsGetResponse` / `NamesResponse` | fetch payload | `secret/doppler` → `secret/material` — all-optional shapes decoded by `Schema` into `Redacted` values before crossing the seam |
| [05] | `Format` / `NameTransformer` | download-shape enum | `secret/doppler` — `download` output format + key-case transform; pinned to the design's env-injection shape |

[PUBLIC_TYPE_SCOPE]: the collapsed HTTP fault carrier (39 status subclasses = seed data)
- rail: rails-and-effects

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:----------------------------------------------------------------- |:---------------- |:----------------------------------------------------------------- |
| [01] | `BaseHTTPError` (`type`/`title`/`detail`/`instance`/`statusCode`) | problem-detail carrier | `secret/doppler` — the one RFC 9457 fault; the design folds on `statusCode`, mapping the whole family through `Match` |
| [02] | `Unauthorized` / `Forbidden` / `NotFound` / `TooManyRequests` / … (39 subclasses of `BaseHTTPError`) | status row | `secret/doppler` — seed data over the carrier; `401/403` → credential-fault arm, `429` → backoff arm, `5xx` → retry arm — never 39 documented members |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construct and authenticate the client
- rail: boundaries

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:--------------------------------------------------------------------------------------- |:-------------- |:----------------------------------------------------------------- |
| [01] | `new DopplerSDK({ accessToken })` / `.setAccessToken(token)` / `.setBaseUrl(url)` | construct | `secret/doppler` — built once at `Layer` construction; `accessToken` sourced from `Config.redacted`, `Redacted.value` unwrapped only here |
| [02] | `sdk.auth.me()` / `sdk.auth.revoke(input)` | token probe | `secret/doppler` — startup liveness check that the service token is valid; `revoke` on credential rotation |

[ENTRYPOINT_SCOPE]: the leased-fetch axis — the one surface the design admits
- rail: system-apis

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:--------------------------------------------------------------------------------------- |:-------------- |:----------------------------------------------------------------- |
| [01] | `sdk.secrets.list(project, config, { includeDynamicSecrets, dynamicSecretsTtlSec, secrets, includeManagedSecrets })` → `Promise<SecretsListResponse>` | leased fetch | `secret/doppler` — the primary read; `includeDynamicSecrets` + `dynamicSecretsTtlSec` issue TTL leases inline (default 1800s) |
| [02] | `sdk.secrets.download(project, config, { format, nameTransformer, includeDynamicSecrets, dynamicSecretsTtlSec, secrets })` → `Promise<DownloadResponse>` | bulk env dump | `secret/doppler` — full config as a name→value map for env injection; also lease-issuing under the same TTL param |
| [03] | `sdk.secrets.get(project, config, name)` / `sdk.secrets.names(project, config, opts?)` | single / names | `secret/doppler` — targeted single-secret read and name enumeration for a partial refresh |
| [04] | `sdk.dynamicSecrets.issueLease(IssueLeaseRequest)` / `.revokeLease(RevokeLeaseRequest)` | lease lifecycle | `secret/doppler` — explicit lease issue/revoke; `revokeLease` is the `Scope` finalizer that returns credentials on teardown |

## [04]-[IMPLEMENTATION_LAW]

[DOPPLER_TOPOLOGY]:
- `DopplerSDK` is a facade over ~23 services; the design admits `secrets` (read), `dynamicSecrets` (lease), and `auth` (probe) and treats the rest as out of scope — a runtime folder that called `projects`/`configs`/`environments`/`integrations` is re-implementing Doppler administration, which belongs in provisioning, not custody.
- Every service method returns a `Promise` that resolves the (all-optional) response or rejects with a `BaseHTTPError` subclass. The 39 status subclasses are one problem-detail carrier discriminated by `statusCode`; the design folds them through `Match` into a small tagged set — `401/403` credential-fault, `404` missing-config, `429` rate-limit-backoff, `5xx` transient-retry — never an instance-of ladder over 39 classes.
- TTL leasing is Doppler-side: `dynamicSecretsTtlSec` on `list`/`download` issues leases that expire server-side (default 1800s). The design owns rotation as a `Schedule` that refetches slightly under the lease window and republishes, and holds a `Scope` finalizer that calls `dynamicSecrets.revokeLease` so leases are returned on teardown rather than left to expire.
- Responses are fully optional and untyped-at-the-edge; the design `Schema`-decodes each payload and wraps every value in `Redacted` at the first read, so a secret never exists as a bare string in the interior. The `download` name→value map is the env-injection shape, `nameTransformer` pinned to the design's key convention.
- The client's built-in `HTTPLibrary` retry (`retryAttempts`/`retryDelayMs`) covers transport hiccups; the design's `Schedule` covers lease-window refresh — the two are orthogonal and both retained.

[STACKS_WITH]:
- `effect` (`.api/effect.md`): `Effect.tryPromise({ try: () => sdk.secrets.list(...), catch })` lifts each fetch; `Match.value(err)` over `BaseHTTPError.statusCode` collapses the 39-class family to a tagged fault; `Config.redacted` sources `DOPPLER_TOKEN`; `Redacted.make` wraps every value at decode and `Schema.decodeUnknown` brands the payload; `Schedule.fixed`/`Schedule.exponential` drives the sub-lease refresh; `SubscriptionRef` publishes the current secret set so consumers observe rotation via `Stream.changes`; `Layer.scoped` owns the client + the `revokeLease` finalizer; `Cache` de-dupes concurrent refetches of the same `(project, config)`.
- `@effect/platform` (`.api/effect-platform.md`): the SDK owns its own node HTTP transport, so Doppler is not routed through `HttpClient` — the boundary is the `Effect.tryPromise` seam, not the transport. Where the design needs shared net policy/tracing over Doppler it re-implements the two admitted reads over `HttpClient.retryTransient` against the Doppler REST API rather than bending the SDK's opaque client; the SDK is admitted whole only for the leased-fetch convenience.
- `security/secret/material` (in-folder): the fetched values become key material — PEM/JWK strings, HMAC keys, the `CredentialPemWire` redacted carrier — decoded once here and handed to `sign` as `Redacted` `CryptoKey`s, never re-fetched per use.
- `security/sign` (in-folder consumer): JWT signing keys, webhook HMAC secrets, and the argon2 pepper (`secret`) are sourced from Doppler through this axis and injected into the `sign` layers at construction, so `sign` never talks to Doppler directly.

[LOCAL_ADMISSION]:
- Use only `secrets` (read), `dynamicSecrets` (lease), and `auth` (probe); never the projects/configs/environments/integrations/syncs administration roster from a runtime folder.
- Wrap every fetch in `Effect.tryPromise` and fold `BaseHTTPError` by `statusCode`; never an instance-of ladder over the 39 status subclasses, and never a bare `Promise` reject in domain logic.
- Lease with `includeDynamicSecrets` + `dynamicSecretsTtlSec`, refresh on a `Schedule` under the lease, and `revokeLease` in a `Scope` finalizer; never let leases silently expire or refetch without a refresh window.
- Source `accessToken` from `Config.redacted` and wrap every returned value in `Redacted` at decode; never a plaintext secret in the interior and never a fetched value in a log or error message.
- Publish the secret set through a `SubscriptionRef`; never re-fetch per consumer read.

[RAIL_LAW]:
- Package: `@dopplerhq/node-sdk`
- Owns: the Doppler secret-fetch axis (`DopplerSDK.secrets` list/download/get/names), the dynamic-lease lifecycle (`dynamicSecrets.issueLease`/`revokeLease`), the auth probe (`auth.me`/`revoke`), the response payload shapes, and the collapsed `BaseHTTPError` problem-detail fault family
- Accept: a single client behind a `Tag` with `Config.redacted` token, `Effect.tryPromise` + `statusCode`-folded faults, TTL leasing via `dynamicSecretsTtlSec` with a `Schedule` refresh + `revokeLease` finalizer, `Schema` decode into `Redacted` values, `SubscriptionRef` publication of the set, `Layer.scoped` lifetime
- Reject: the administration service roster from a runtime folder, an instance-of ladder over the status subclasses, a leased fetch without a refresh window or teardown revoke, a plaintext secret in the interior or a log, a per-consumer refetch, an `accessToken` outside `Redacted`
