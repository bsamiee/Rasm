# [RASM_PERSISTENCE_API_OTEL_INSTRUMENTATION_AWS]

`OpenTelemetry.Instrumentation.AWS` spans the two AWSSDK client legs Persistence holds — the `AWSSDK.S3` object-store transfers at `Store/blobstore#OBJECT_STORE` and the `AWSSDK.KeyManagementService` custody calls at `Element/identity#KMS_CUSTODY` — so every S3 multipart hop and KMS wrap/unwrap/sign joins the one trace the sibling data legs already carry. Substrate canonical members live at `libs/csharp/.api/api-otel-instrumentation-aws.md`; this overlay carries only the Persistence delta — the single-registration seam over both clients and the process-global-registry ruling.

## [01]-[SUBSTRATE_CANONICAL]

[SUBSTRATE_CANONICAL]: `libs/csharp/.api/api-otel-instrumentation-aws.md`
- options/enum type roster, the `AddAWSInstrumentation` tracer/meter admission tables, the `AWSSDK.*` wildcard source/meter facts, and package/asset facts live on the substrate catalog — this overlay never re-states them
- rail: cloud-sdk instrumentation

## [02]-[PERSISTENCE_BINDINGS]

- `AddAWSInstrumentation` on the tracer and meter builders at the AppHost root covers BOTH held AWSSDK clients at once — the customizer hooks the shared `AWSSDK.Core` pipeline, so the `IAmazonS3` object-store client and the `IAmazonKeyManagementService` custody client emit spans off one registration, never a per-client bind.
- `Store/blobstore#OBJECT_STORE` `ObjectClient.S3` (`IAmazonS3`) multipart and `TransferUtility` hops carry `Client`-kind spans continuing the parent commit-drain activity; the `ObjectEncryption` SSE-KMS write stance and `ObjectLock` WORM edge ride the same span, never a hand-tagged twin.
- `Element/identity#KMS_CUSTODY` `SigningKeyring` and `Store/blobstore#BLOB_GC` `ObjectEncryption` envelope calls (`GenerateDataKey`/`Encrypt`/`Decrypt`/`ReEncrypt`, `Sign`/`Verify`) span through the same customizer — the KMS client needs no separate instrumentation row.

## [03]-[IMPLEMENTATION_LAW]

[SINGLE_REGISTRATION]:
- one host-root `AddAWSInstrumentation` tracer admission covers every AWSSDK service client in the process, so the S3 and KMS legs share it
- `RuntimePipelineCustomizerRegistry.Instance` ignores later registrations with the same customizer name, so the first admitted options govern every app and SDK client in the host
- `SuppressDownstreamInstrumentation` stays set where the AppHost also admits `OpenTelemetry.Instrumentation.Http`, collapsing each S3/KMS hop's inner `HttpClient` span into the single AWS-service span

[HOST_WIDE_SCOPE]:
- customizer registration lands on the process-global `RuntimePipelineCustomizerRegistry.Instance`, so one host composition root owns admission for every app in the process
- app-scoped instrumentation options have no isolated registry slot; co-hosted apps share the first admitted customizer configuration

[RAIL_LAW]:
- Package: `OpenTelemetry.Instrumentation.AWS`
- Owns: the Persistence tracer/meter registration row covering the S3 object-store and KMS custody legs
- Accept: composition-root `AddAWSInstrumentation` on the tracer and meter builders; `SuppressDownstreamInstrumentation` where HTTP instrumentation co-admits
- Reject: per-app or per-client customizer admission; hand-rolled aws-semconv spans over the transfer or custody bodies; customizer registration inside a store or keyring fold
