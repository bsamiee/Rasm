# [PY_RUNTIME_API_GOOGLE_CLOUD_SECRET_MANAGER]

`google-cloud-secret-manager` owns the GCP Secret Manager read client backing the `execution/admission#SETTINGS` `SecretTier.cloud` arm: one `SecretManagerServiceClient` (ADC or `from_service_account_file`) injects as `secret_client=` into `GoogleSecretManagerSettingsSource`, graduating that arm's deferred `Ok(Nothing)` placeholder to a live `SECRET_LADDER` cloud-tier row. Payloads resolve through the settings source alone — the runtime reads versioned secrets, never minting or rotating them and never calling the client directly.

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

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction + secret read

Every surface hangs off `SecretManagerServiceClient`: the bare `(...)` row constructs it, two classmethods build the file-credential client and the resource path, and `access_secret_version` is the one polymorphic read over `name=` or `request=` with an asyncio twin.

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------ | :------- | :-------------------------------------------------- |
|  [01]   | `(credentials=, transport=, client_options=)`     | ctor     | ADC when `credentials=None`; gRPC transport default |
|  [02]   | `from_service_account_file(path)`                 | factory  | explicit service-account JSON credential            |
|  [03]   | `secret_version_path(project, secret, version)`   | static   | `projects/*/secrets/*/versions/*` resource name     |
|  [04]   | `access_secret_version(name= \| request=)`        | instance | one polymorphic read; `.payload.data` bytes         |
|  [05]   | `await async_client.access_secret_version(name=)` | instance | asyncio twin for a native-async resolve leg         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- consume law: the runtime constructs one `SecretManagerServiceClient` — ADC by default, `from_service_account_file` when a key path is admitted — and reads each `RASM_PY_`-prefixed field's secret version through the injected `GoogleSecretManagerSettingsSource`, never a direct `access_secret_version` call.
- ladder law: the `SecretTier.cloud` case is one `TierRow(SecretTier(cloud=...), Some(Feature.SECRET_MANAGER), RetryClass.SECRET)` above the `file` fallback, gated by `Feature.SECRET_MANAGER`/`Killswitch.DISABLE_SECRET_MANAGER` and retried under the one `RetryClass.SECRET` `stamina` policy the keystore/file tiers share, so a transiently-unreachable manager retries inside one derivation span.
- credential law: authentication resolves at construction as ADC (workload-identity, metadata server, or `GOOGLE_APPLICATION_CREDENTIALS`) or an explicit service-account file when the deployment pins one, and the resolved secret crosses as `pydantic` `SecretStr`.

[STACKING]:
- `pydantic-settings`(`.api/pydantic-settings.md`): the constructed client becomes `secret_client=` on `GoogleSecretManagerSettingsSource(settings_cls, credentials=, project_id=, secret_client=)`, folded into `settings_customise_sources` so the cloud tier reads an admitted settings field rather than a bare client call.
- `google-crc32c`(`.api/google-crc32c.md`): `value(payload.data) -> int` verifies `SecretPayload.data_crc32c` inside the `cloud_read` fence, a mismatch surfacing as the retryable `RetryClass.SECRET` transport fault rather than a silently-trusted payload.
- `reliability/resilience#RESILIENCE`: `guarded(RetryClass.SECRET, ...)` wraps the cloud-tier probe, offloaded through `anyio.to_thread.run_sync` so the blocking gRPC read never stalls the loop.

[LOCAL_ADMISSION]:
- admission admits `SecretManagerServiceClient` construction and its `secret_client=` injection; ADC/service-account resolution, retry/timeout defaults, and the internal `SecretManagerServiceGrpcTransport` — distinct from the `.api/grpcio.md` serve/dial channels the runtime owns — all arrive settled from the client, and this page owns only the read slice the cloud-tier row consumes.

[RAIL_LAW]:
- Package: `google-cloud-secret-manager`
- Owns: the GCP Secret Manager read client backing the cloud secret-resolution tier
- Accept: one `SecretManagerServiceClient` (ADC or service-account-file) injected as `secret_client=` into `GoogleSecretManagerSettingsSource`, the `SecretTier.cloud` `TierRow` gated by `Feature.SECRET_MANAGER` and retried under `RetryClass.SECRET`, `SecretPayload.data_crc32c` verified through `google-crc32c`, the resolved secret lifted to `SecretStr`
- Reject: a direct `access_secret_version` bypassing the settings-source fence, the admin CRUD surface (`create_secret`/`add_secret_version`/`destroy_secret_version`) the runtime does not own, inline credential material beside ADC/service-account resolution, a bare-`str` resolved secret, a parallel cloud-secret owner beside the one `SecretTier.cloud` row
