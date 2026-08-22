# [PY_RUNTIME_API_GOOGLE_CLOUD_SECRET_MANAGER]

`google-cloud-secret-manager` owns the GCP Secret Manager read client backing the `execution/admission#SETTINGS` `SecretTier.cloud` arm: one `SecretManagerServiceClient` (ADC or `from_service_account_file`) injects as `secret_client=` into `GoogleSecretManagerSettingsSource`, serving the `SECRET_LADDER` cloud-tier row beside the Vault and Azure backends. Declared model fields resolve through the settings source; per-service ladder credentials resolve through `CloudVault.read`'s direct `access_secret_version` — the runtime reads versioned secrets, never minting or rotating them.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `google-cloud-secret-manager`
- package: `google-cloud-secret-manager` (Apache-2.0)
- module: `google.cloud.secretmanager`
- namespaces: `google.cloud.secretmanager`, `google.cloud.secretmanager_v1`
- rail: secrets

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client family + read message graph
- proto-plus messages; the read leg mints only the request/response/payload messages, the remaining CRUD request messages being the unadmitted admin surface.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `SecretManagerServiceClient`      | client        | sync gRPC client injected as `secret_client=`      |
|  [02]   | `SecretManagerServiceAsyncClient` | client        | asyncio twin for a native-async read leg           |
|  [03]   | `AccessSecretVersionRequest`      | request       | `name`-addressed version read                      |
|  [04]   | `AccessSecretVersionResponse`     | response      | carries `name` + `payload: SecretPayload`          |
|  [05]   | `SecretPayload`                   | payload       | `data: bytes` + `data_crc32c: int` integrity field |

[PUBLIC_TYPE_SCOPE]: exception taxonomy
- every arm derives from `google.api_core.exceptions.GoogleAPICallError`, whose `.code` carries the HTTP status; the sibling Vault and Key Vault catalogs name a GCP MISS arm this roster is the source for.
- `ServerError` is the 5xx base and is deliberately NOT the retry target — it also roots `MethodNotImplemented` (501) and `DataLoss`, neither of which a retry window clears — so each transient arm is named at its own spelling.

| [INDEX] | [SYMBOL]              | [STATUS] | [CAPABILITY]                                             |
| :-----: | :-------------------- | :------: | :------------------------------------------------------- |
|  [01]   | `GoogleAPICallError`  |    —     | base; carries `.code`, `.message`, `.details`            |
|  [02]   | `NotFound`            |   404    | absent secret or version — the MISS arm                  |
|  [03]   | `PermissionDenied`    |   403    | IAM denies the resource — hard boundary fault            |
|  [04]   | `Unauthenticated`     |   401    | credential rejected — hard boundary fault                |
|  [05]   | `TooManyRequests`     |   429    | quota rejection; roots `ResourceExhausted` — a transient |
|  [06]   | `InternalServerError` |   500    | service-side fault — a transient                         |
|  [07]   | `ServiceUnavailable`  |   503    | backend unreachable — a transient                        |
|  [08]   | `DeadlineExceeded`    |   504    | RPC deadline through `GatewayTimeout` — a transient      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction + secret read

Every surface hangs off `SecretManagerServiceClient`: the bare `(...)` row constructs it, two classmethods build the file-credential client and the resource path, and `access_secret_version` is the one polymorphic read over `name=` or `request=` with an asyncio twin.

- release is the CONTEXT MANAGER alone — the client exposes `__enter__`/`__exit__` and NO `close` member, so a bare `client.close()` is an `AttributeError` and the `with` block is the only spelling that retires the gRPC transport; the async twin mirrors it with `__aenter__`/`__aexit__`.

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------ | :------- | :-------------------------------------------------- |
|  [01]   | `(credentials=, transport=, client_options=)`     | ctor     | ADC when `credentials=None`; gRPC transport default |
|  [02]   | `from_service_account_file(path)`                 | factory  | explicit service-account JSON credential            |
|  [03]   | `secret_version_path(project, secret, version)`   | static   | `projects/*/secrets/*/versions/*` resource name     |
|  [04]   | `access_secret_version(name= \| request=)`        | instance | one polymorphic read; `.payload.data` bytes         |
|  [05]   | `await async_client.access_secret_version(name=)` | instance | asyncio twin for a native-async resolve leg         |
|  [06]   | `__enter__` / `__exit__`                          | bracket  | the ONLY release seam; no `close` member exists     |
|  [07]   | `__aenter__` / `__aexit__`                        | bracket  | async-twin release seam                             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- consume law: TWO legs off one client family. Declared `RASM_PY_`-prefixed model fields read through the injected `GoogleSecretManagerSettingsSource` (ADC by default, `from_service_account_file` when a key path is admitted); per-service `(service, username)` ladder credentials resolve at call time through `execution/admission#SETTINGS` `CloudVault.read`'s direct `access_secret_version` — a construction-time settings source structurally cannot serve the call-time read.
- ladder law: the `SecretTier.cloud` case is one `TierRow(SecretTier.CLOUD, Some(Feature.SECRET_MANAGER))` above the `file` fallback, gated by `Feature.SECRET_MANAGER`/`Killswitch.DISABLE_SECRET_MANAGER`; the retry class spells once at the dispatch that applies it rather than as a ladder column repeating `RetryClass.SECRET` per rung, so a transiently-unreachable manager retries inside one derivation span.
- miss-vs-fault law: `NotFound` (404) is the MISS the ladder walks past, the arm Vault `InvalidPath` and Key Vault `ResourceNotFoundError` match; `TooManyRequests`/`InternalServerError`/`ServiceUnavailable`/`DeadlineExceeded` are the transients `RetryClass.SECRET` rides, each spelled at its own arm rather than at the `ServerError` base a 501 also roots; `PermissionDenied`/`Unauthenticated` surface as hard boundary faults, never a silently-empty read.
- credential law: authentication resolves at construction as ADC (workload-identity, metadata server, or `GOOGLE_APPLICATION_CREDENTIALS`) or an explicit service-account file when the deployment pins one, and the resolved secret crosses as `pydantic` `SecretStr`.
- release law: the per-read client brackets through `with SecretManagerServiceClient(...)`, because the type carries no `close` and a client left for GC strands its gRPC channel across every ladder walk; the settings-source leg holds its injected client for the source's own lifetime instead.

[STACKING]:
- `pydantic-settings`(`.api/pydantic-settings.md`): the constructed client becomes `secret_client=` on `GoogleSecretManagerSettingsSource(settings_cls, credentials=, project_id=, secret_client=)`, folded into `settings_customise_sources` so the cloud tier reads an admitted settings field rather than a bare client call.
- `google-crc32c`(`.api/google-crc32c.md`): `value(payload.data) -> int` verifies `SecretPayload.data_crc32c` inside the `CloudVault.read` fence, a mismatch surfacing as the retryable `RetryClass.SECRET` transport fault rather than a silently-trusted payload.
- `reliability/resilience#RESILIENCE`: `guarded(RetryClass.SECRET, ...)` wraps the cloud-tier probe, offloaded through `anyio.to_thread.run_sync` so the blocking gRPC read never stalls the loop.

[LOCAL_ADMISSION]:
- admission admits `SecretManagerServiceClient` construction, its `secret_client=` injection, and the `with` bracket that retires it; ADC/service-account resolution, retry/timeout defaults, and the internal `SecretManagerServiceGrpcTransport` — a client-private channel no runtime serve or dial seam shares — all arrive settled from the client, and this page owns only the read slice the cloud-tier row consumes.

[RAIL_LAW]:
- Package: `google-cloud-secret-manager`
- Owns: the GCP Secret Manager read client backing the cloud secret-resolution tier
- Accept: one `SecretManagerServiceClient` (ADC or service-account-file) injected as `secret_client=` into `GoogleSecretManagerSettingsSource`, its `with` bracket as the read leg's release, the `SecretTier.cloud` `TierRow` gated by `Feature.SECRET_MANAGER` and retried under `RetryClass.SECRET`, `NotFound` as the ladder MISS with the `TooManyRequests`/5xx arms as retried transients, `SecretPayload.data_crc32c` verified through `google-crc32c`, the resolved secret lifted to `SecretStr`
- Reject: a direct `access_secret_version` placed OUTSIDE the `CloudVault.read` arm or the settings source, a `client.close()` the type never defines, a client left for GC where the `with` bracket retires the channel, `ServerError` as the retry target where a 501 rides it, the admin CRUD surface (`create_secret`/`add_secret_version`/`destroy_secret_version`) the runtime does not own, inline credential material beside ADC/service-account resolution, a bare-`str` resolved secret, a parallel cloud-secret owner beside the one `SecretTier.cloud` row
