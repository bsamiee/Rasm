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

[PERSISTENCE_BIM_ARTIFACT_INDEX]-[QUEUED]: Persistence lands the content-keyed ArtifactIndexRow joining the Bim IFC semantic graph and its tessellated GLB as two projections of one content-addressed artifact, so the Bim TessellationOutcome cache-hit reads the prior GLB by ArtifactKey and the BimWire snapshot joins the same content key, never re-crossing the companion for an unchanged model.
- Capability: Gives the Bim tessellation cache and wire snapshot a durable content-addressed home: Persistence indexes the IFC artifact and the re-imported GLB by the Compute content-key, so a re-tessellation or re-fetch resolves by reference rather than re-computing.
- Shape: A Persistence ArtifactIndexRow keyed on the Compute InterchangeIdentity content-key carrying the IFC-semantic and GLB-tessellation projections, read by the Bim TessellationRequest.Resolve cache leg, the BimWire content-key join, and the INCREMENTAL_DELTA_REIMPORT prior-snapshot lookup.
- Unlocks: The Bim TESSELLATION_OUTCOME_RECEIPT cache-hit fact and the INCREMENTAL_DELTA_REIMPORT prior-model join resolve against a real durable index, and a federation client re-fetches only changed BimWire element rows by content-key.
- Anchors: Exchange/tessellation#TESSELLATION_BRIDGE ArtifactKey; Exchange/wire#WIRE_PROJECTION ContentKey; Exchange/import#IMPORT_RAIL Reimport prior-model join; Review/diff#MODEL_DIFF ElementFingerprint; csharp:Rasm.Persistence/Query/Cache#ArtifactIndexRow keyed on the Compute `Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity`; ARCHITECTURE.md `Query/cache → Rasm.Bim/Model # [CONTENT_KEY]: ArtifactIndexRow` plus the three `Query ← Rasm.Bim/Exchange # [CONTENT_KEY]: TessellationOutcome ArtifactKey / Reimport prior-BimModel / BimWire snapshot` seams.
- Tension: Bim depends strictly upward and mints no Persistence reference: the artifact index is the Persistence owner's concern read at the seam, the Bim side carrying only the ArtifactKey the index is keyed by.
- Ripple: counterpart of `Rasm.Bim` `[TESSELLATION_OUTCOME_RECEIPT]`/`[INCREMENTAL_DELTA_REIMPORT]`/`[BIM_CONNECTION_IFC_WIRE]` Exchange/Review content-key owners (`tessellation`/`import`/`wire`/`diff`), realizing the Bim `[VERSIONED]` idea.

[RECOVERY_PAGE_AUTHOR]-[QUEUED]: Author `Version/recovery.md` as the durable-recovery owner minting `RecoveryProfile`/`BackupKind`/`RecoveryObjective` and the `RecoveryDrill` fold over `StoreProfile.Replication`.
- Capability: Stands up the home for gap `a` — backup, DR, PITR, and replication verification — by giving the durable-state spine a recovery page that the existing `StoreCeremony.Restore` and `SqliteMaintenance.Backup` owners already gesture toward but no page owns.
- Shape: A new `Version/recovery.md` page declaring the `RecoveryProfile` `SmartEnum` x `BackupKind` `Union` x `RecoveryObjective` axis, with the PG arm verification-only through `ClusterConfig.Verify` on `StoreProfile.Replication`, the sqlite arm driving `SqliteMaintenance.Backup`, the object arm carrying `ObjectResidence`+`LegalHold`, and the `RecoveryDrill` fold stamping measured RTO against the replication state.
- Unlocks: Verified disaster recovery, measured RPO/RTO, and restore-drill evidence over every engine without spawning a second PG backup tool.
- Anchors: `StoreCeremony.Restore`, `StoreProfile.Replication`, `SqliteMaintenance.Backup`, `ClusterConfig.Verify`, `SyncCursor.Lsn`, and the typed `RestoreReceipt`/`TransferReceipt`; `Npgsql` supplies the PG replication/PITR surface read verification-only; the seam adds a `RECOVERY` row to `Rasm.Persistence/README.md` and a `Version/Recovery.cs` node to `Rasm.Persistence/ARCHITECTURE.md`.

[RECOVERY_OBJECTIVE_COLLAPSE_RECOVERYFACT]-[QUEUED]: Fold per-engine restore evidence into one typed `RecoveryFact` stream feeding `StoreFact`, keeping `RestoreReceipt`/`TransferReceipt` typed.
- Capability: Collapses three-plus per-engine recovery-evidence buckets into one fact stream with slot/kind metadata rather than parallel construction sites, so every engine's restore evidence flows through one surface.
- Shape: One `RecoveryFact` stream on `Version/recovery.md` feeding `StoreFact`/`StoreEvidence`, with the sqlite, PG, and object restore arms emitting fact rows discriminated by kind while `RestoreReceipt` and `TransferReceipt` remain typed algorithm receipts.
- Unlocks: A single recovery-evidence surface that downstream observability and drill verification read without re-learning per-engine receipt shapes.
- Anchors: The typed `RestoreReceipt`/`TransferReceipt`, `StoreFact`, and `StoreEvidence`; `Npgsql` carries the PG restore evidence; sequenced after `RECOVERY_PAGE_AUTHOR` mints the page the fold lives on.

[TRANSACTION_PAGE_AUTHOR]-[QUEUED]: Author `Query/transaction.md` owning `TxnScope`/`IsolationPolicy`/`LockMode`/`Savepoint`/2PC as a closed arm-family on `StoreOp` and the SQLSTATE classifier feeding `StoreFault.Concurrency`.
- Capability: Stands up the home for gap `b` — isolation, MVCC locking, savepoints, two-phase commit, and deadlock classification — reaching the dormant `StoreFault.Concurrency` rail that exists but no page drives.
- Shape: A new `Query/transaction.md` page declaring `TxnScope` over an `IsolationPolicy` `SmartEnum` x `LockMode` `Union` plus a `Savepoint` ladder as an arm-family on `StoreOp`/`StoreRail.Run`, projecting `StoreProfile.ConcurrencyToken` into `StoreFault.Concurrency` 7001, classifying SQLSTATE 40001/40P01 into a verdict the existing `EnableRetryOnFailure` owner consumes, and folding `PrepareTransactionAsync` for the 2PC arm.
- Unlocks: Concurrent correctness, `SKIP LOCKED` work queues, nested rollback, and cross-store commit coordination.
- Anchors: `StoreOp` 8-case+`StoreRail.Run`, `StoreFault.Concurrency` 7001+`From(Error)`+`TransactionCommittedAsync`, and `StoreProfile.ConcurrencyToken`+`EnableRetryOnFailure`; `Npgsql` and `Microsoft.Data.Sqlite` supply the engine transaction surfaces; the seam adds a `TRANSACTION` row to `Rasm.Persistence/README.md` and a `Query/Transaction.cs` node to `Rasm.Persistence/ARCHITECTURE.md`.

[CONFLICTOUTCOME_DISCRIMINANT_COLLAPSE]-[QUEUED]: Collapse the Sync `ConflictOutcome` 4-case `Union` into one `ConflictResult`+discriminant, reusing the verdict in the transaction classifier.
- Capability: Densifies a 4-case union into one record carrying a verdict discriminant, and shares that verdict with the SQLSTATE classifier `TRANSACTION_PAGE_AUTHOR` stands up so merge resolution and deadlock classification speak one verdict vocabulary.
- Shape: One `ConflictResult` (`Verdict`, `Receipt`) on `Sync/collaboration.md` replacing the `ConflictOutcome` 4-case `Union`+`SyncMerge.Apply` arms, with the typed `ConflictReceipt` preserved.
- Unlocks: A denser merge surface plus a shared verdict reused across Sync conflict resolution and transaction deadlock classification.
- Anchors: `ConflictOutcome` 4-case `Union`+`SyncMerge.Apply` and the typed `ConflictReceipt`; `Thinktecture.Runtime.Extensions` supplies the `[Union]`/discriminant collapse surface; the wire `ConflictOutcomeKind` consumed by `typescript:interchange/codec` stays unchanged.
- Tension: The wire `ConflictOutcomeKind` carried to the TypeScript interchange codec must stay unchanged; only the in-process union collapses, not the serialized discriminant.

[ENCRYPTION_PAGE_AUTHOR]-[QUEUED]: Author `Store/encryption.md` owning `KeyEnvelope`/`KmsProvider`/`EnvelopeScope`/`RotationPolicy` and the per-engine keying arms, relocating SQLCipher out of Research.
- Capability: Stands up the home for gap `j` — envelope DEK/KEK encryption, cloud-KMS custody, and rotation across sqlite/PG/object-store — and retires the research-held SQLCipher stub by giving it a production keying home.
- Shape: A new `Store/encryption.md` page declaring `KeyEnvelope` over a `KmsProvider` `SmartEnum` x `EnvelopeScope` `Union` plus `RotationPolicy`, with the sqlite arm driving `EncryptionGate.Sqlcipher` from a KMS-unwrapped DEK, the PG arm verifying TDE GUC read-only via `ClusterConfig.Verify`, the object arm carrying SSE-KMS, and Personal-only `DemandsEncryption` read as mandatory.
- Unlocks: At-rest encryption, cloud-KMS custody, per-tenant key isolation, and rotation; retires the SQLCipher research row.
- Anchors: `EncryptionGate.Sqlcipher` (relocated out of Research), Personal-only `DemandsEncryption`, `ObjectResidence` SSE, and `TenancyModel.Rls`+`ClusterConfig.Verify`; the admitted `AWSSDK.KeyManagementService` (`.api/api-aws-kms.md`), `Azure.Security.KeyVault.Keys` (`.api/api-azure-keyvault.md`), and `Google.Cloud.Kms.V1` (`.api/api-google-kms.md`) supply cloud key custody; the seam adds an `ENCRYPTION` row to `Rasm.Persistence/README.md` and a `Store/Encryption.cs` node plus the authored `Store/encryption ⇄ Rasm.AppHost/Runtime # [PORT]: KMS-unwrap port` seam to `Rasm.Persistence/ARCHITECTURE.md`.

[SQLCIPHER_RESEARCH_PROMOTE]-[QUEUED]: Promote `EncryptionGate.Sqlcipher` out of Research by binding its PRAGMA key/rekey ceremony to a KMS-unwrapped DEK.
- Capability: Gives the SQLCipher gate a real key source by wiring its PRAGMA key/rekey ceremony to the `EnvelopeKeyring` unwrap delegate, turning a research stub into at-rest sqlite encryption.
- Shape: Deepens the `SQLITE_KEYING` arm on `Store/encryption.md`, binding the `EncryptionGate.Sqlcipher` ceremony rows to a KMS-unwrapped DEK supplied through the `EnvelopeKeyring` unwrap delegate.
- Unlocks: At-rest sqlite encryption for Personal content, keyed from the envelope owner rather than a static passphrase.
- Anchors: The `EncryptionGate.Sqlcipher` ceremony rows and the `EnvelopeKeyring` unwrap delegate; the admitted `AWSSDK.KeyManagementService` (`.api/api-aws-kms.md`) supplies the DEK unwrap; sequenced after `ENCRYPTION_PAGE_AUTHOR` relocates the gate (`KMS_PACKAGE_ADMISSION` already landed the unwrap members), holding sqlite-embedded with no new engine.

[QUALITY_PAGE_AUTHOR]-[QUEUED]: Author `Store/quality.md` owning the `QualityRule` `Union` and the `QualityPlan` lowering fold to the cheapest server-side site.
- Capability: Stands up the home for gap `h` — declarative validation rules, cross-document referential integrity, and anomaly/near-duplicate detection — as one closed rule axis that lowers each rule to the cheapest enforcement site.
- Shape: A new `Store/quality.md` page declaring a `QualityRule` `Union` (`NotNullSet`/`UniqueAcross`/`ReferentialEdge`/`RangeBound`/`RegexShape`/`JsonSchemaShape`/`StatisticalAnomaly`/`NearDuplicate`) lowered by `QualityPlan` to `CHECK`/`EXCLUDE`/`FK`, `jsonb_matches_schema`, a changefeed-fold, or a DuckDB pass, riding `CrossDocLink`/`StalePins` and emitting `ElementSet`.
- Unlocks: Enforced integrity, quality gates, and cross-document referential consistency over the federated store.
- Anchors: `SetExpr`/`ElementSet`/`RulePlan`/`CrossDocLink`, `jsonb_matches_schema`, `ContentChunk.ShortTag`, and `btree_gist` `EXCLUDE`; the admitted `pg_jsonschema` and `DuckDB.NET.Data.Full` (`.api/api-duckdb.md`) supply schema-shape and anomaly enforcement; the seam adds a `QUALITY` row to `Rasm.Persistence/README.md`, a `Store/Quality.cs` node to `Rasm.Persistence/ARCHITECTURE.md`, authors the BIM quality-rule catalog at `Rasm.Bim/Model` feeding IFC validation rules into `QualityRule` rows through the authored `Store/quality ← Rasm.Bim/Model` seam, and reads `Rasm.Compute` geometry-derived anomaly rule sources through the `Store/quality ← Rasm.Compute` seam.

[PIPELINE_PAGE_AUTHOR]-[QUEUED]: Author `Query/pipeline.md` owning the `PipelineStage` `Union` and the `BulkPipeline` back-pressured fold over `ArrowChunk`.
- Capability: Stands up the home for gap `i` — large import/export/transform orchestration across Arrow/Parquet/CSV/IFC/Speckle with staging, in-flight validation, idempotency, dead-letter, and back-pressure — collecting the partial primitives that exist without an orchestrator.
- Shape: A new `Query/pipeline.md` page declaring a `PipelineStage` `Union` (`Extract`/`Stage`/`Validate`/`Transform`/`Load`/`Reconcile`) driven by `BulkPipeline` into a back-pressured fold over `ArrowChunk`, made idempotent through chunk index plus the idempotency-dedup `ArtifactClassRow`, with a typed `PipelineReject` dead-letter and `ArrowEgress`/`BulkRoute` op-log terminators.
- Unlocks: Resumable staging, in-flight validation, idempotent re-runs, dead-letter capture, and back-pressure.
- Anchors: `ArrowChunk`/`ArrowPlane`/`ArrowEgress`, `TabularExportSpec`/`LakehouseCatalog`, `ContentChunker`+idempotency-dedup `ArtifactClassRow`, and `BulkRoute`/`BulkShed`; the admitted `Apache.Arrow.Flight`/`Apache.Arrow.Adbc` (`.api/api-arrow.md`) and `DuckDB.NET.Data.Full` (`.api/api-duckdb.md`) supply streaming transform and lakehouse legs and `Sep` (`.api/api-sep.md`) the CSV leg; idempotency reuses `ArtifactClassRow` and termination rides the one `BulkRoute` op-log; the seam adds a `PIPELINE` row to `Rasm.Persistence/README.md`, a `Query/Pipeline.cs` node to `Rasm.Persistence/ARCHITECTURE.md`, and confirms `Rasm.Compute/Exchange` parse-to-canonical-bytes for the `Extract` stage through the authored `Sync/pipeline ⇄ Rasm.Compute/Exchange # [PORT]: parse-to-canonical-bytes (Extract)` seam.

[EGRESS_PAGE_AUTHOR]-[QUEUED]: Author `Sync/egress.md` owning the `EgressSink` axis, the `EgressPump` op-log-drain fold with at-least-once delivery and dead-letter, and the CloudEvents envelope.
- Capability: Stands up the home for gap `g` — fanning the op-log changefeed to external sinks with delivery guarantees, redaction scoping, dead-letter, and cursor replay — without a second changefeed, since `LineageCdc.Feed` already filters internally but pushes nowhere.
- Shape: A new `Sync/egress.md` page declaring an `EgressSink` `SmartEnum` (`webhook`/`nats`/`kafka`/`rabbitmq`/`arrow-flight-push`/`grpc-stream`) x `DeliveryGuarantee` x `SinkScope`, with `EgressPump` draining the op-log past a durable `SyncCursor`, projecting via `LineageCdc.Feed`, advancing on durable ack, and emitting a typed `EgressDeadLetter`; the envelope reuses `CdcEnvelopeWire` (its `TraceContext` realizing `ONE_DISTRIBUTED_TRACE`) in CloudEvents.
- Unlocks: Real-time propagation, delivery guarantees, redaction scoping, dead-letter capture, and replay.
- Anchors: `LineageCdc.Feed`/`CdcEnvelope`, `OpLog`/`SyncCursor`/`OpLogEntryWire`, `ArrowEgress`/`ArrowChunk`, AppHost `OutboundHop`, and `CdcEnvelopeWire`; the admitted `Confluent.Kafka` (`.api/api-kafka.md`) and `CloudNative.CloudEvents` (`.api/api-cloudevents.md`) supply broker delivery and the standard envelope; egress rides the one op-log, registers as a keyed AppHost `OutboundHop` through the authored `Sync/egress ⇄ Rasm.AppHost/Runtime # [PORT]: keyed OutboundHop egress` seam, and reuses `LineageCdc`/`ExportProof` redaction; the seam adds an `EGRESS` row to `Rasm.Persistence/README.md`, a `Sync/Egress.cs` node to `Rasm.Persistence/ARCHITECTURE.md`, and authors the CloudEvents egress-consumption leg at `python:runtime/transport`.

[SCHEMADDL_SQL_COLLAPSE]-[QUEUED]: Collapse `TimescaleProvisioning.Provision` and `SearchProvisioning.Provision`, both folding a `Union` into `MigrationBuilder.Sql`, into one `SchemaDdl.Sql` fold.
- Capability: Densifies two near-identical server-tier raw-SQL provisioning folds into one surface, the brief near-term collapse, while preserving the `Fin` reject arm.
- Shape: One `ProvisioningStep` `Union` (4 `TimescaleStep` + 2 `IndexSpec`) on `Store/server.md` and `Schema/ddl.md`, replacing the twin `TimescaleProvisioning.Provision` and `SearchProvisioning.Provision` folds with one `SchemaDdl.Sql` fold over `MigrationBuilder.Sql`, the `Fin` reject arm surviving.
- Unlocks: One server-tier raw-SQL provisioning surface.
- Anchors: `TimescaleProvisioning.Provision`, `SearchProvisioning.Provision`+`IndexSpec`+`Fin` reject, and `SchemaDdl`; `Microsoft.EntityFrameworkCore.Design` supplies the `MigrationBuilder.Sql` surface; the `Bm25Predicate` pdb projection stays its own owner and the collapse is build/verify-only.
- Tension: The `Bm25Predicate` pdb builder axis must not fold in; only the `IndexSpec` build-DDL fold collapses.

[STORE_SERVER_SPLIT]-[QUEUED]: Split the `Store/server` 5-owner god-page into `Store/provisioning.md` and `Store/tenancy.md`.
- Capability: Decomposes a 5-owner page into focused owners — provisioning (`SchemaDdl.Sql` fold + `ClusterConfig` verify + `MigrationBundle`) and tenancy (`TenancyModel`/`TenantProvision`/`TenantQuota`) — conserving capability while sharpening ownership.
- Shape: `Store/server.md` splits into `Store/provisioning.md` carrying the `SchemaDdl.Sql` fold, `ClusterConfig.Verify`, and `MigrationBundle`, and `Store/tenancy.md` carrying `TenancyModel`/`TenantProvision`/`TenantQuota`.
- Unlocks: Focused `ClusterConfig.Verify` and `TenancyModel` owners freed from the god-page.
- Anchors: `TimescaleProvisioning`/`SearchProvisioning`/`ClusterConfig`/`MigrationBundle` and `TenancyModel`/`TenantProvision`/`TenantQuota`; `Npgsql` supplies the cluster and RLS surface; the split is verify-only; the seam replaces `SERVER` with `PROVISIONING`+`tenancy` in `Rasm.Persistence/README.md` and splits `Store/Server.cs` into `Provisioning.cs`+`Tenancy.cs` in `Rasm.Persistence/ARCHITECTURE.md`.
- Tension: `ClusterConfig.Preload` and the RLS `current_setting` `rasm.tenant` binding must stay intact; validate before splitting.

[ANNOTATION_RELOCATE_TO_BIM]-[QUEUED]: Move BCF/coordination domain semantics out of `Sync/annotation` into Rasm.Bim coordination, leaving Persistence the generic durable-annotation plus op-log/CDE sync owner.
- Capability: Enforces the boundary that Bim owns BCF/coordination semantics while Persistence owns durable annotation anchoring and CDE/op-log sync, removing the domain BCF model from the app-platform store so the AEC-domain owner holds it.
- Shape: The BCF topic/component/comment domain semantics relocate from `Sync/annotation` into the `Rasm.Bim/coordination` page; Persistence `Sync/annotation` retains generic durable-annotation anchoring plus the op-log/CDE sync round-trip, becoming a host-neutral annotation store rather than a BCF model.
- Unlocks: A clean openBIM coordination boundary where the federated cockpit composes one Bim-owned BCF model and one Persistence-owned annotation/sync surface, with no second BCF schema in the store.
- Anchors: `Sync/annotation` anchor algebra and CDE-sync op-log round-trip, `Rasm.Bim/coordination` BCF topic/component/comment semantics, and the `FEDERATED_COORDINATION_COCKPIT` branch idea whose boundary this enforces; the authored `Sync/annotation ⇄ Rasm.Bim/coordination # [WIRE]: BCF/coordination domain` seam retires the stale `Sync/annotation -> Rasm.AppUi/Editing` domain leg, re-pointing `Rasm.AppUi` annotation consumption to Bim; the two join on the `GlobalId` content-key the durable annotation row carries; strata-legal as app-platform consuming aec-domain.
- Ripple: counterpart of `Rasm.Bim` `[BIM_COORDINATION_ANNOTATION]` (BCF/coordination domain relocated into `Rasm.Bim/coordination`, Persistence keeps the generic durable-annotation + op-log/CDE-sync store).

[SCHEDULE_RELOCATE_TO_BIM]-[QUEUED]: Move P6/MS-Project plus 4D construction-state domain semantics out of `Sync/schedule` into a Rasm.Bim schedule sub-domain, leaving Persistence the durable schedule rows plus sync owner.
- Capability: Enforces the same boundary for scheduling — Bim owns the P6/MS-Project format parse and 4D construction-state semantics while Persistence owns durable schedule rows and their sync — removing the construction-domain model from the app-platform store.
- Shape: The P6/MS-Project parse and 4D construction-state domain semantics relocate from `Sync/schedule` into a new `Rasm.Bim/schedule` sub-domain; Persistence `Sync/schedule` retains durable schedule rows plus the sync round-trip, holding no construction-state model.
- Unlocks: A clean schedule boundary where Bim owns construction sequencing semantics and Persistence owns durable schedule storage and sync, with no construction-domain model in the store.
- Anchors: `Sync/schedule` durable rows and `ScheduleImport.ReadXer` sync, the new `Rasm.Bim/schedule` 4D construction-state plus CPM owner, and the `FEDERATED_COORDINATION_COCKPIT` branch idea whose boundary this enforces; the authored `Sync/schedule ⇄ Rasm.Bim/schedule # [WIRE]: P6/MS-Project + 4D construction domain` seam splits the source lanes — Persistence owns the durable P6/MS-Project store plus its `CpmPass` over the external `TaskRelation` DAG, Bim owns the host-neutral CPM/4D fold over the IFC-projected `SequenceRel` network — per the `[SCHEDULE_NETWORK_DEPTH]` boundary note; strata-legal as app-platform consuming aec-domain.
- Ripple: counterpart of `Rasm.Bim` `[BIM_SCHEDULE]` (4D construction-state + CPM domain relocated into `Rasm.Bim/schedule`, Persistence keeps the durable external-scheduler store).

[ARTIFACT_CONTENT_KEY_FEDERATION]-[QUEUED]: Author the Persistence-side wire consumer for the artifacts provenance content-key binding, decoding the C#-owned XxHash128 content-key seed the artifacts C2PA hard-binding stamps so signed artifacts federate into the durable identity store Persistence solely owns.
- Capability: A content-key wire consumer accepting the artifacts `ContentIdentity` binding over the XxHash128 seed, so a C2PA-signed artifact federates into the durable content-hash identity store; the Python session lane holds no durable store and never re-mints the identity.
- Shape: A `[CONTENT_KEY]` seam on Rasm.Persistence mirroring the `artifacts provenance/credential -> csharp:Rasm.Persistence` `[CONTENT_KEY]` edge, the durable consumer of the wire-projected content-key binding.
- Unlocks: Signed, content-addressed artifacts resolved against the durable Persistence identity store from the one shared XxHash128 seed, the durable end of the artifacts provenance hard-binding.
- Anchors: `libs/csharp/Rasm.Persistence` content-hash identity/federation owner (architecture.md `[04]`), the XxHash128 content-key seed, `artifacts provenance/credential#PROVENANCE`.
- Ripple: `artifacts` `[PROVENANCE]` — Persistence consumer for the C2PA content-key binding over the XxHash128 seed.
- Atomic: content-key wire consumer for the artifacts provenance binding.

[ICECHUNK_ASOF_CONTENT_KEY]-[QUEUED]: Mirror the data icechunk as-of snapshot identity on the Persistence `Version/Snapshots` owner, reproducing the as-of snapshot content-key from the one XxHash128 seed so a data virtual-cube as-of read federates against the durable Version owner.
- Capability: `Version/Snapshots` reproduces the icechunk as-of snapshot content-key from the C#-owned XxHash128 seed, so a data virtual-cube as-of read resolves against the durable content-addressed snapshot spine rather than a second snapshot identity.
- Shape: A `Version/Snapshots` seam row consuming `python:data/gridded/virtual` `[CONTENT_KEY]`, the version-control surface being the Persistence Version concern read at the wire.
- Unlocks: A data virtual-cube as-of read federated against the durable Version owner off the shared seed, mirroring the `gridded/virtual -> Rasm.Persistence` `[CONTENT_KEY]` edge.
- Anchors: `Rasm.Persistence/Version/Snapshots.cs` content-addressed snapshot spine, the XxHash128 seed, icechunk `set_virtual_ref`.
- Ripple: `data` `[TENSOR_SPLIT]` — mirror the icechunk as-of snapshot identity on the `Version/Snapshots` owner.
- Atomic: `Version/Snapshots` as-of content-key seam row.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[KMS_PACKAGE_ADMISSION]-[COMPLETE]: `AWSSDK.KeyManagementService` 4.0.12.8, `Azure.Security.KeyVault.Keys` 4.10.0, and `Google.Cloud.Kms.V1` 3.24.0 pinned + referenced; README `[ENCRYPTION_KMS]` group and `.api/api-aws-kms.md`, `.api/api-azure-keyvault.md`, `.api/api-google-kms.md` authored.
[ARROW_FLIGHT_PACKAGE_ADMISSION]-[COMPLETE]: `Apache.Arrow.Flight` 23.0.0 already admitted (manifest, csproj, README `[COLUMNAR_ARROW]`, `.api/api-arrow.md` covers Flight); the streaming and Flight-egress activation rides `BULK_ETL_INTERCHANGE_PIPELINE`.
[KAFKA_CLOUDEVENTS_PACKAGE_ADMISSION]-[COMPLETE]: `Confluent.Kafka` 2.14.2 and `CloudNative.CloudEvents`/`.Kafka`/`.SystemTextJson` 2.8.0 pinned + referenced; README `[STREAMING_EGRESS]` group and `.api/api-kafka.md`, `.api/api-cloudevents.md` authored.
