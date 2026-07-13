# [PY_RUNTIME_API_GOOGLE_CLOUD_SECRET_MANAGER]

`google-cloud-secret-manager` supplies the GCP Secret Manager client: a `SecretManagerServiceClient` (and its `SecretManagerServiceAsyncClient` twin) whose `access_secret_version(name=)` reads a versioned secret payload, a `secret_version_path(project, secret, version)` resource-name builder, and the `AccessSecretVersionResponse`/`SecretPayload` message graph carrying the `data` bytes and a `data_crc32c` integrity field, over gRPC (`SecretManagerServiceGrpcTransport`) with ADC- or service-account-file credentials. It is the client backing the branch-catalogued `GoogleSecretManagerSettingsSource` (`libs/python/.api/pydantic-settings.md` PUBLIC_TYPES [13]), which the `execution/admission#SETTINGS` `SecretTier.cloud` arm graduates from the deferred `Ok(Nothing)` placeholder to a real `SECRET_LADDER` cloud-tier row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `google-cloud-secret-manager`
- package: `google-cloud-secret-manager`
- import: `google.cloud.secretmanager`
- owner: `runtime`
- rail: secrets
- version: `2.29.0`
- license: Apache-2.0
- namespaces: `google.cloud.secretmanager`, `google.cloud.secretmanager_v1`
- installed: `2.29.0`
- capability: versioned secret-payload access over gRPC, secret/version resource-name path builders, ADC and service-account-file credential loading, the `AccessSecretVersionResponse`/`SecretPayload` message graph with `data_crc32c` integrity, and a sync + asyncio client twin

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client family
- rail: secrets

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [RAIL]                                                                            |
| :-----: | :-------------------------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `SecretManagerServiceClient`      | client        | sync gRPC client; `GoogleSecretManagerSettingsSource` accepts as `secret_client=` |
|  [02]   | `SecretManagerServiceAsyncClient` | client        | asyncio client twin for a native-async read leg                                   |

[PUBLIC_TYPE_SCOPE]: request + payload message graph
- rail: secrets
- proto-plus messages; the read leg needs only `AccessSecretVersionRequest`/`Response` and `SecretPayload`, the remaining CRUD request messages are the admin surface the runtime never mints.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [RAIL]                                             |
| :-----: | :---------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `AccessSecretVersionRequest`  | request       | `name`-addressed version read                      |
|  [02]   | `AccessSecretVersionResponse` | response      | carries `name` + `payload: SecretPayload`          |
|  [03]   | `SecretPayload`               | payload       | `data: bytes` + `data_crc32c: int` integrity check |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction
- rail: secrets

Every surface hangs off `SecretManagerServiceClient`; the bare `(...)` row is the constructor, the rest classmethods.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :-------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `(credentials=None, transport=None, client_options=None)` | construct      | ADC when `credentials=None`; gRPC transport default |
|  [02]   | `from_service_account_file(path)`                         | construct      | explicit service-account JSON credential            |
|  [03]   | `secret_version_path(project, secret, version)`           | path builder   | `projects/*/secrets/*/versions/*` resource name     |

[ENTRYPOINT_SCOPE]: secret read
- rail: secrets

Every row is a `read`; `access_secret_version` is the one polymorphic entry over `name=` or `request=`.

| [INDEX] | [SURFACE]                                                               | [RAIL]                                                       |
| :-----: | :---------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `client.access_secret_version(name=)`                                   | returns `AccessSecretVersionResponse`; `.payload.data` bytes |
|  [02]   | `client.access_secret_version(request=AccessSecretVersionRequest(...))` | request-object form, one polymorphic entrypoint              |
|  [03]   | `await async_client.access_secret_version(name=)`                       | asyncio twin for a native-async resolve leg                  |

## [04]-[IMPLEMENTATION_LAW]

[SECRET_TOPOLOGY]:
- consume law: the runtime does NOT call `access_secret_version` directly — it constructs one `SecretManagerServiceClient` (ADC by default, `from_service_account_file` when a key path is admitted) and injects it as `secret_client=` into `GoogleSecretManagerSettingsSource`, which folds each `RASM_PY_`-prefixed field's secret version into the settings source chain. The admin CRUD surface (`create_secret`/`add_secret_version`/`destroy_secret_version`) is never admitted: the runtime reads secrets, never mints or rotates them.
- ladder law: the `execution/admission#SETTINGS` `SecretTier.cloud` case is one `TierRow(SecretTier(cloud=...), Some(Feature.SECRET_MANAGER), RetryClass.SECRET)` above the `file` fallback — gated by `Feature.SECRET_MANAGER` and its `Killswitch.DISABLE_SECRET_MANAGER` row, retried under the same `RetryClass.SECRET` `stamina` policy the keystore/file tiers share, so a transiently-unreachable Secret Manager retries under one backoff inside one derivation span rather than failing the resolve on the first RPC fault.
- credential law: authentication is ADC (workload-identity, metadata server, or `GOOGLE_APPLICATION_CREDENTIALS`) resolved by the client at construction, or an explicit service-account file when the deployment pins one; the runtime passes no secret material inline and the resolved secret crosses as `pydantic` `SecretStr`, never a bare `str`.
- integrity law: `SecretPayload.data_crc32c` verifies the payload survived transport; a mismatch is a boundary fault the `guarded` envelope surfaces, not a silently-trusted read.

[INTEGRATION_STACK]:
- settings leg: `GoogleSecretManagerSettingsSource(settings_cls, credentials=, project_id=, secret_client=)` (`.api/pydantic-settings.md`) is the one consuming fence — the runtime builds the client, the source folds it into `settings_customise_sources`, and the cloud tier probe reads the admitted settings field rather than a bare client call.
- resilience leg: the cloud-tier probe rides the `reliability/resilience#RESILIENCE` `guarded(RetryClass.SECRET, ...)` retried-traced-railed envelope exactly as the keystore/file tiers do, offloaded through `anyio.to_thread.run_sync` when the sync client is used so the blocking gRPC read never stalls the loop.
- transport leg: the client's own `SecretManagerServiceGrpcTransport` is the gRPC channel it manages internally; it is distinct from the `.api/grpcio.md` serve/dial channels the runtime owns — the runtime never reaches into the client's transport.

[LOCAL_ADMISSION]:
- the admission owner admits `SecretManagerServiceClient` construction and its injection as `secret_client=` into `GoogleSecretManagerSettingsSource`; `access_secret_version` and the async twin ride the source, never a direct runtime call.
- ADC/service-account credential resolution, gRPC transport, and retry/timeout defaults arrive settled from the client; this page owns only the read-slice the cloud-tier ladder row consumes.

[RAIL_LAW]:
- Package: `google-cloud-secret-manager`
- Owns: the GCP Secret Manager read client backing the cloud secret-resolution tier
- Accept: one `SecretManagerServiceClient` (ADC or service-account-file) injected as `secret_client=` into `GoogleSecretManagerSettingsSource`, the `SecretTier.cloud` `TierRow` gated by `Feature.SECRET_MANAGER` and retried under `RetryClass.SECRET`, `SecretPayload.data_crc32c` integrity verification, the resolved secret lifted to `SecretStr`
- Reject: a direct `access_secret_version` call bypassing the settings-source fence, the admin CRUD surface (`create_secret`/`add_secret_version`/rotation) the runtime does not own, inline credential material beside ADC/service-account resolution, a bare-`str` resolved secret beside `SecretStr`, a parallel cloud-secret owner beside the one `SecretTier.cloud` ladder row
