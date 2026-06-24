# [PERSISTENCE_TASKLOG]

The open and closed work for the durable-state spine, distilled from `IDEAS.md`. Each task carries a status marker, thesis, capability, shape, unlocks, anchors, and optional tension; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[ARTIFACT_CONTENT_KEY_FEDERATION]-[BLOCKED]: Author the Persistence-side wire consumer for the artifacts provenance content-key binding, decoding the C#-owned XxHash128 content-key seed the artifacts C2PA hard-binding stamps so signed artifacts federate into the durable identity store Persistence solely owns.
- Capability: A content-key wire consumer accepting the artifacts `ContentIdentity` binding over the XxHash128 seed, so a C2PA-signed artifact federates into the durable content-hash identity store; the Python session lane holds no durable store and never re-mints the identity.
- Shape: A `[CONTENT_KEY]` seam on Rasm.Persistence mirroring the `artifacts provenance/credential -> csharp:Rasm.Persistence` `[CONTENT_KEY]` edge, the durable consumer of the wire-projected content-key binding.
- Unlocks: Signed, content-addressed artifacts resolved against the durable Persistence identity store from the one shared XxHash128 seed, the durable end of the artifacts provenance hard-binding.
- Anchors: `libs/csharp/Rasm.Persistence` content-hash identity/federation owner (architecture.md `[04]`), the XxHash128 content-key seed, `artifacts provenance/credential#PROVENANCE`. The Persistence half — `Query/federation#ENTITY_GRAPH` `SourceKind.SignedArtifact` decoding the wire-projected content-key binding — is realized; the `artifacts [PROVENANCE]` counterpart that stamps the `ContentIdentity` binding has not landed, so the cross-language wire-consumer seam stays open until the Python artifacts leg projects the binding.
- Ripple: `artifacts` `[PROVENANCE]` — Persistence consumer for the C2PA content-key binding over the XxHash128 seed.
- Atomic: content-key wire consumer for the artifacts provenance binding.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[PERSISTENCE_BIM_ARTIFACT_INDEX]-[COMPLETE]: `Query/cache#ARTIFACT_BLOB_INDEX` `ArtifactIndexRow` carries the `SourceKey`/`Project` join over `IfcSemantic`/`Interchange`/wire-snapshot projections content-keyed on the Compute `InterchangeIdentity`, read by the Bim tessellation cache-hit, the BimWire snapshot join, and the incremental-delta reimport; Ripple: `Rasm.Bim` `[TESSELLATION_OUTCOME_RECEIPT]`/`[INCREMENTAL_DELTA_REIMPORT]`/`[BIM_CONNECTION_IFC_WIRE]`.
[RECOVERY_PAGE_AUTHOR]-[COMPLETE]: `Version/recovery.md` mints `RecoveryProfile`/`BackupKind`/`RecoveryObjective` over `StoreProfile.Replication`, sqlite-paged/PG-verify-only/object/file-snapshot arms, `RecoveryDrill` stamping measured RTO; README `RECOVERY` row and `Version/Recovery.cs` node landed.
[RECOVERY_OBJECTIVE_COLLAPSE_RECOVERYFACT]-[COMPLETE]: one `RecoveryFact`/`RecoveryFactKind` stream feeds `StoreFact`/`StoreEvidence` with `RestoreReceipt`/`TransferReceipt` kept typed; every per-engine recovery bucket folds into the one stream.
[TRANSACTION_PAGE_AUTHOR]-[COMPLETE]: `Query/transaction.md` owns `TxnScope`/`IsolationPolicy`/`LockMode`/`Savepoint`/2PC as an arm-family on `StoreOp` through `StoreRail.Run`, the SQLSTATE 40001/40P01 classifier feeding `StoreFault.Concurrency` 7001 and `EnableRetryOnFailure`, and `TwoPhase` `PREPARE/COMMIT PREPARED` draining the in-doubt set through AppHost; README `TRANSACTION` row and `Query/Transaction.cs` node landed.
[CONFLICTOUTCOME_DISCRIMINANT_COLLAPSE]-[COMPLETE]: `Sync/collaboration#MERGE_LAW` collapsed the `ConflictOutcome` 4-case `Union` into one `ConflictResult(Verdict, Receipt)` reusing the shared `ConflictVerdict` in the transaction SQLSTATE classifier; the wire `ConflictOutcomeKind` stays unchanged.
[ENCRYPTION_PAGE_AUTHOR]-[COMPLETE]: `Store/encryption.md` owns `KeyEnvelope`/`KmsProvider`/`EnvelopeScope`/`RotationPolicy` and the `EnvelopeKeyring` wrap/unwrap, sqlite-DEK/PG-TDE-verify/object-SSE arms, Personal-only `DemandsEncryption`; README `ENCRYPTION` row, `Store/Encryption.cs` node, and the KMS-unwrap-port seam landed.
[SQLCIPHER_RESEARCH_PROMOTE]-[COMPLETE]: `Store/encryption#SQLITE_KEYING` promoted `EncryptionGate.Sqlcipher` out of Research, binding its `PRAGMA key`/`rekey` ceremony to a KMS-unwrapped DEK supplied through the `EnvelopeKeyring` unwrap delegate.
[QUALITY_PAGE_AUTHOR]-[COMPLETE]: `Store/quality.md` owns the `QualityRule` 8-case `Union` and the `QualityPlan` lowering fold to CHECK/EXCLUDE/FK/`jsonb_matches_schema`/changefeed-fold/DuckDB pass/pgvector probe, emitting an `ElementSet`; README `QUALITY` row, `Store/Quality.cs` node, and the Bim/Compute rule-source seams landed.
[PIPELINE_PAGE_AUTHOR]-[COMPLETE]: `Query/pipeline.md` owns the `PipelineStage` 6-case `Union` and the `BulkPipeline` back-pressured fold over `ArrowChunk` with `ArtifactClassRow` idempotency, typed `PipelineReject` dead-letter, and `BulkRoute`/`ArrowEgress` terminators; README `PIPELINE` row, `Query/Pipeline.cs` node, and the parse-to-canonical-bytes Extract seam landed.
[EGRESS_PAGE_AUTHOR]-[COMPLETE]: `Sync/egress.md` owns the `EgressSink` axis x `DeliveryGuarantee` x `SinkScope`, the `EgressPump` op-log-drain past a durable `SyncCursor` with at-least-once advance and typed `EgressDeadLetter`, and the CloudEvents envelope; README `EGRESS` row, `Sync/Egress.cs` node, and the keyed-OutboundHop seam landed.
[SCHEMADDL_SQL_COLLAPSE]-[COMPLETE]: `Schema/ddl#EXTENSION_DDL` folded the four timescale + two index provisioning cases into the one `SchemaDdl.Sql` traverse over `MigrationBuilder.Sql`, retiring the twin `TimescaleProvisioning.Provision`/`SearchProvisioning.Provision` surfaces with the `Fin` `DiskAnn` reject arm surviving; `Bm25Predicate` stays its own search-lane query owner.
[STORE_SERVER_SPLIT]-[COMPLETE]: `Store/server` split into `Store/provisioning.md` (`SchemaDdl.Sql` fold + `ClusterConfig.Verify`/`Preload` + `MigrationBundle`) and `Store/tenancy.md` (`TenancyModel`/`TenantProvision`/`TenantQuota`), the RLS `current_setting('rasm.tenant')` binding intact; README and `ARCHITECTURE.md` codemap updated.
[ANNOTATION_RELOCATE_TO_BIM]-[COMPLETE]: `Sync/annotation.md` retains the generic durable-annotation `Anchor` algebra + CDE/op-log sync, the BCF/coordination domain relocated to `Rasm.Bim/coordination`, joining on the `Query/federation#ENTITY_GRAPH` `GlobalId` key; Ripple: `Rasm.Bim` `[BIM_COORDINATION_ANNOTATION]`.
[SCHEDULE_RELOCATE_TO_BIM]-[COMPLETE]: `Sync/schedule.md` retains the durable P6/MS-Project store + `CpmPass` over the external `TaskRelation` DAG + sync, the 4D/CPM-IFC construction domain relocated to `Rasm.Bim/schedule`; Ripple: `Rasm.Bim` `[BIM_SCHEDULE]`.
[ICECHUNK_ASOF_CONTENT_KEY]-[COMPLETE]: `Version/snapshots#SNAPSHOT_PROTOCOL` `AsOfKey` reproduces the icechunk as-of snapshot content-key from the one C#-owned XxHash128 seed so a data virtual-cube as-of read federates against the durable Version owner; Ripple: `data` `[TENSOR_SPLIT]`.
[KMS_PACKAGE_ADMISSION]-[COMPLETE]: `AWSSDK.KeyManagementService` 4.0.12.8, `Azure.Security.KeyVault.Keys` 4.10.0, and `Google.Cloud.Kms.V1` 3.24.0 pinned + referenced; README `[ENCRYPTION_KMS]` group and `.api/api-aws-kms.md`, `.api/api-azure-keyvault.md`, `.api/api-google-kms.md` authored.
[ARROW_FLIGHT_PACKAGE_ADMISSION]-[COMPLETE]: `Apache.Arrow.Flight` 23.0.0 already admitted (manifest, csproj, README `[COLUMNAR_ARROW]`, `.api/api-arrow.md` covers Flight); the streaming and Flight-egress activation rides `BULK_ETL_INTERCHANGE_PIPELINE`.
[KAFKA_CLOUDEVENTS_PACKAGE_ADMISSION]-[COMPLETE]: `Confluent.Kafka` 2.14.2 and `CloudNative.CloudEvents`/`.Kafka`/`.SystemTextJson` 2.8.0 pinned + referenced; README `[STREAMING_EGRESS]` group and `.api/api-kafka.md`, `.api/api-cloudevents.md` authored.
