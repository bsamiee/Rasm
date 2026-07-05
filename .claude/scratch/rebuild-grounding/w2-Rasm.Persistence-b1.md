# GROUNDING DOSSIER — w2 Rasm.Persistence b1 (Version/{provenance,retention,recovery})

Verified primary extracts only. Every claim carries a `file:line` anchor. Members
confirmed via `uv run python -m tools.assay api query` (assay LIVE, ilspy over restored
net10.0 assemblies) or quoted directly from the on-disk `.api` catalogs.

## [00]-[INVENTORIES] — real `ls`

### Doctrine root — `docs/stacks/csharp/`
```
README.md algorithms.md boundaries.md language.md rails-and-effects.md shapes.md
surfaces-and-dispatch.md system-apis.md  + domain/
```
### Doctrine domain — `docs/stacks/csharp/domain/`
```
README.md compute.md concurrency.md data-interchange.md diagnostics.md durability.md
interaction.md persistence.md postgres.md resilience.md runtime.md transport.md
validation.md visuals.md
```
### Shared substrate tier — `libs/csharp/.api/` (relevant to these pages)
```
api-languageext.md api-thinktecture-runtime-extensions.md api-thinktecture-json.md
api-hashing.md api-nodatime.md api-nodatime-stj.md api-highperformance.md
api-generator-equals.md api-mapperly.md api-quikgraph.md ...
```
### Folder tier — `libs/csharp/Rasm.Persistence/.api/` (relevant)
```
api-marten.md api-npgsql.md api-npgsql-otel.md api-hashing.md api-objectstore.md
api-minio.md api-pollination-sdk.md api-cloudevents.md api-thinktecture-serialization.md
api-nodatime.md ... (84 catalogs total)
```
### Version folder — `libs/csharp/Rasm.Persistence/.planning/Version/`
```
commits.md ledger.md merge.md provenance.md recovery.md retention.md timetravel.md
```
Note: NO `RASM-CS-PERSISTENCE-DECISION.md` inside the package; the DECISION lives at
REPO ROOT `/Users/bardiasamiee/Documents/99.Github/Rasm/RASM-CS-PERSISTENCE-DECISION.md`.

## [01]-[ASSAY-VERIFIED MEMBERS] (live decompile, net10.0)

### Marten 9.12.0 — `EventStoreStatistics` (assay query, primary_assembly marten/9.12.0/lib/net10.0/Marten.dll)
```
public class EventStoreStatistics {
    public long EventCount { get; set; }        // unique events
    public long StreamCount { get; set; }       // unique streams
    public long EventSequenceNumber { get; set; } // CURRENT GLOBAL event-sequence value
}
```
XMLDoc on `EventSequenceNumber`: "Current value of the event sequence. This may be higher
than the number of events if events have been archived or if there were failures while
appending events." => it is the GLOBAL sequence high-water, NOT a per-stream version.

### Npgsql 10.0.3 — `ReplicationSystemIdentification` (assay query)
`public class ReplicationSystemIdentification` with members `SystemId`, `Timeline`, `XLogPos`
(confirmed present). Returned by `IdentifySystem`.

### LanguageExt.Core — `LanguageExt.IO` static surface (assay query, --full)
Confirmed members present: `lift`, `liftAsync`, `pure`, `fail`, `Map`, `Bind`, `catch`.
=> `recovery.md` `IO.liftAsync`/`IO.lift`/`IO.pure`/`IO.fail`/`@catch<IO,T>` all real.

## [02]-[.API CATALOG EXTRACTS] (on-disk, folder + shared tiers)

### `libs/csharp/Rasm.Persistence/.api/api-marten.md`
- :84  `IQueryEventStore` — "the read contract: `FetchStreamAsync`, `AggregateStreamAsync<T>` (AS-OF), `AggregateStreamToLastKnownAsync`, `FetchStreamStateAsync`, event `LoadAsync`"
- :90  `StreamState` — "`FetchStreamStateAsync` result: `Id`/`Key`, `Version`, `AggregateType`, `LastTimestamp`, `Created`, `IsArchived`"
- :106 `IProjectionDaemon` — "`StartAllAsync`, `RebuildProjectionAsync`, `WaitForNonStaleData`, agent/shard control"
- :109 `EventStoreStatistics` — "`FetchEventStoreStatistics` result (event/stream counts, sequence + projection high-water marks)"
- :172 `Events.AggregateStreamAsync<T>(Guid streamId, long version = 0, DateTimeOffset? timestamp = null, T? state = null, long fromVersion = 0, CancellationToken token = default)` — "fold a stream into `T`; AS-OF by `version` or `timestamp`"
- :175 `Events.FetchStreamStateAsync(Guid streamId, [CancellationToken])` — "current `StreamState` (version/timestamps/archive) without folding"
- :205 `daemon.StartAllAsync()` / `RebuildProjectionAsync<TView>(...)` / `WaitForNonStaleData(TimeSpan)` / `StopAllAsync()`
- :206 `store.Advanced.RebuildSingleStreamAsync<T>(Guid id, [CancellationToken])`

### `libs/csharp/Rasm.Persistence/.api/api-npgsql.md`
- :93  `LogicalReplicationConnection`: `StartReplication`, `SetReplicationStatus`, `SendStatusUpdate`, `IdentifySystem`, `CreatePgOutputReplicationSlot`
- :94  `NpgsqlLogSequenceNumber`: `Parse`, `TryParse`, comparison operators, `Larger`, `Invalid`
- :100 `ReplicationSystemIdentification` (`SystemId`, `Timeline`, `XLogPos`, `DbName`) returned by `IdentifySystem`
- :225 `IdentifySystem` — "returns `ReplicationSystemIdentification` (`XLogPos`/`Timeline`/`SystemId`)"
- :246 `[REPLICATION_UNCONSUMED]` — "`Npgsql.Replication` ... is RECORDED-UNCONSUMED: the changefeed is Marten's async daemon over the event stream, not raw-WAL logical decoding. Logical replication is the NAMED escalation path — admitted only if a raw-WAL CDC consumer ever lands." => recovery's `IdentifySystem`-only use is the SANCTIONED restraint, not a gap.

### `libs/csharp/.api/api-hashing.md` (shared System.IO.Hashing 10.0.9)
- `[03]` entrypoints: `XxHash128.HashToUInt128(ReadOnlySpan<byte>, long seed = 0)` static UInt128; incremental `Append` + `GetCurrentHashAsUInt128`.
  => provenance.md `XxHash128`/`AgentKey` real; retention.md cites this package but composes NONE of it (see [04]).

### `libs/csharp/Rasm.Persistence/.api/api-pollination-sdk.md`
- :28  `Configuration` — "`Default` static, `BasePath`, `AccessToken => TokenRepo?.GetToken()`, `DefaultHeader`, `AddApi...`"
- :31  `TokenRepo` — "holds + refreshes the Pollination access token (`GetToken()`)"
- :43  `RunsApi` — "`GetRun`/`GetAllRunSteps`/`GetRunOutput`/`GetRunStepLogs`/`DownloadRunArtifact`/`CancelRun`"
- :45  `ArtifactsApi` — "`CreateArtifact` (-> `S3UploadRequest`) / `DownloadArtifact` / `ListArtifacts` / `DeleteArtifact`"
- :46  `RecipesApi` / `RegistriesApi` / `PluginsApi` — "recipe/registry/plugin catalog transport (the run definition supply side)"
- :69  `Run` / `StepStatus` / `JobStatusEnum` / `RunStatusEnum` — "run state, per-step status, the job/run status discriminants"
- :83  `config.AccessToken` — "`virtual string AccessToken => TokenRepo?.GetToken()` — the bearer token"
- :109 `RunsApi.GetRunAsync` — "`Task<Run> GetRunAsync(string owner, string name, string runId, …)`"
- :122 "The Pollination artifact plane IS S3: `ArtifactsApi.CreateArtifactAsync` returns an `S3UploadRequest`"
- :126 compute-route seam: "`PollinationSDK`'s job-submission/watch half ROUTES to `Rasm.Compute`" => provenance CONSUMES completed-run metadata as PROV rows; it never submits jobs.

## [03]-[DECISION SEAM/RIDER ANCHORS] (repo-root RASM-CS-PERSISTENCE-DECISION.md)

- :67  `[B]` HASHER_RE_ANCHOR row: "`AgentKey` (durable PROV node identity) | `provenance.md:291` `XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(actor))` | `ContentHash.Of(Encoding.UTF8.GetBytes(actor))` | Leg 2" — re-anchor obligation NOT yet realized on disk.
- :70  `[B]` DEFENSIBLE-LOCAL: "the `provenance` internal Merkle rolling digests (`provenance.md:229,321,349,418`) ... are NOT re-anchored" — the attested-ledger Merkle hasher stays raw XxHash128 by ruling.
- :97  Row 9 `Version/provenance.md` IMPROVE: "`[V10]` Principal-derived agent class (read the class off `Principal`, not unconditional `Person` `:257`). [B] `AgentKey` re-anchor. **Cloud-run attribution rows** ([05] PollinationSDK): a completed cloud run is a W3C-PROV `Activity` — Agent = the service principal behind `Configuration.AccessToken` (`TokenRepo`), Used = input-asset content keys, Generated = output-asset content keys, the recipe reference (`owner/name:tag` + `PackageVersion.Digest`) the plan."
- :98  Row 10 `Version/retention.md` IMPROVE: "`[V10]` `StorageLane.Durable` consume-in-a-guard-or-delete (`:36`). `[V5]c` FROZEN imports: `StorageTier` (blobstore-owned) + `TimeCut` (timetravel-owned)."
- :99  Row 11 `Version/recovery.md` IMPROVE: "`[V10]` the two verified RPO bugs: `ObjectReplica` real freshest-replicated-blob lag (killing `Duration.FromMinutes(absent.Count)` `:180`); `PgPitr` WAL-throughput → explicit policy row (killing 16-MiB-segment-as-rate `:171`). `[V12]/S3` terminal proof: reconstructed `OfGraph` == checkpoint `AsOfKey`. `[V5]c` FROZEN import `ObjectStore.Head`."
- :38-39 `[A.3]` FROZEN VOCAB: `StorageTier` rows (blobstore→retention `retention.md:100,121-124`); `ObjectStore.Head` delegate shape (blobstore→recovery `recovery.md:175,179,204`).

## [04]-[TARGET-PAGE DEFECT ANCHORS] (on-disk fences)

### provenance.md
- :257 `agents[Iri(authorityKey)] = new Dictionary<string, object?> { ["prov:type"] = AgentClass.Person.ClassIri, ["rasm:id"] = bundle.Authority.Subject, ["rasm:signed"] = false };` — bundle-authority agent class HARDCODED `Person`, `signed` HARDCODED false. DECISION:97 `[V10]` flags derive-from-Principal.
- :291 `internal static UInt128 AgentKey(string actor) => XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(actor));` — raw hasher; DECISION:67 `[B]` re-anchor to `ContentHash.Of` unrealized.
- NO PollinationSDK integration anywhere in the page (cloud-run Activity rows absent). DECISION:97 `[05]` names it.

### retention.md
- :17  Packages line: "... System.IO.Hashing, BCL inbox." and :177 "... System.IO.Hashing, BCL inbox." — PHANTOM CITATION: `rg 'XxHash|HashToUInt|System.IO.Hashing|\.Append\('` over the fences returns ZERO hashing calls. `Admit` (:152) takes an already-hashed `ContentAddress key`; `RetentionCeiling` uses `FrozenDictionary`; nothing composes System.IO.Hashing.
- :36  `public bool Durable { get; }` on `StorageLane` — captured; `rg 'Durable'` in fence bodies shows it is NEVER READ by any sweep/executor/admit body. DECISION:98 `[V10]` "consume-in-a-guard-or-delete."

### recovery.md
- :56  `public static RecoveryPoint Of(ReplicationSystemIdentification id, long streamVersion, Instant at) => Create(id.Timeline, (ulong)id.XLogPos, Some(streamVersion), at);`
- :170-171 `PgPitr`: `ulong lagBytes = system.XLogPos >= ctx.ArchiveFlushed ? (ulong)system.XLogPos - (ulong)ctx.ArchiveFlushed : 0UL; return (RecoveryPoint.Of(system, stats.EventSequenceNumber, clocks.Now), Duration.FromSeconds(lagBytes / (16d * 1024 * 1024)));`
  - GLOBAL `stats.EventSequenceNumber` bound as per-model `streamVersion` (version-space mismatch — see [01] assay XMLDoc).
  - `16d * 1024 * 1024` HARDCODED as WAL-bytes-per-second rate. DECISION:99 `[V10]` bug #2.
- :180 `ObjectReplica`: `absent.IsEmpty ? Duration.Zero : Duration.FromMinutes(absent.Count)` — RPO fabricated as `minutes = count-of-absent-blobs` (a count is not a time lag). DECISION:99 `[V10]` bug #1.
- :336-338 `ReAttest`: `restore.Target.StreamVersion.Match(Some: version => query.Events.AggregateStreamAsync<GraphProjection>(restore.Model.Value, version: version), None: () => ...timestamp: restore.Target.At...)` — consumes the global-sequence-as-version; comment "the same event the LSN replay reached" is false (LSN is a global WAL axis, per-stream version is a different axis).

## [05]-[FOLDER-CONTEXT ANCHORS] (siblings composed by the three targets)

- provenance composes: `Element/identity#AUTHORITY` `Authority.Verify`/`AuthDecision`/`SigningKeyring`/`SignedAuthorship`/`OpDigest`/`Principal` (provenance.md:5); `Version/commits` `Hlc`/`CommitNode`/`CommitNode.IsMerge`/`Parents`/`OpKeys` (provenance.md:5,177,187,198); `Version/ledger` `OpLogEntry` changefeed (provenance.md:5, ledger.md:107 record shape); `Version/timetravel#TIME_TRAVEL` `Checkpoint.Hash` non-crypto chain deferral + `BlameRow` (provenance.md:21,310); `Element/codec#CODEC_AXIS` `ElementJson.Options` (provenance.md:19,266).
- retention composes: AppHost `DataClassification` (retention.md:3,103-106 seam-local rank over 9 rows); `Element/codec` `ContentAddress`/`SnapshotCatalogRow.StoredLength`/`ChunkManifest.Length` (retention.md:3,15); `Store/blobstore` `StorageTier` (FROZEN, retention.md:100,121-124) + `BlobResidence.Length` (:15) + `#BLOB_GC` `BlobCatalogRow` (:19) + `RemoteStoreFault.Locked` WORM (:179); `Version/timetravel` `TimeCut` cut set (FROZEN, retention.md:3,241).
- recovery composes: AppHost `ResolvedProfile.Recovery`/`RecoveryObjective` (recovery.md:3,12,14,132); `Element/codec` `Snapshots.Verify`/`ContentAddress`/`ContentAddress.OfGraph`/`SnapshotCatalogRow` (recovery.md:3,188,339); `Element/graph` `GraphProjection`/`ModelId` (recovery.md:3,310,338); `Version/provenance#ATTESTED_LEDGER` `AttestedLedger.Verify`/`AttestVerdict`/`SigningKeyring`/`OpDigest`/`SignedAuthorship`/`AttestedEntry` (recovery.md:3,328); `Version/timetravel` `TimeCut`/`RecoveryPoint.AsCut()` (recovery.md:3,65); `Store/blobstore` `ObjectStore`/`ObjectClient`/`ObjectStore.Head` (FROZEN, recovery.md:3,178).
- Cross-page seam CONSISTENCY confirmed: recovery `ReAttest` (recovery.md:328) composes provenance `AttestedLedger.Verify(chain, KeyringFor, DigestOf)` (provenance.md:364) with the identical independent-`digestOf` discipline (provenance.md:309,363; recovery.md:250-256). The `Unauthored`-reachable law is one owner, correctly consumed.
- Non-target sibling cross-checks: ledger.md:107 `OpLogEntry(... ColumnFamily Family, ... UInt128 ContentKey, ... Seq<UInt128> Closure ...)` — provenance `Derive` (provenance.md:170) reads `entry.ContentKey`/`entry.Actor`/`entry.Stamp`/`entry.Kind.Tombstone`; `SyncOpKind.Tombstone` (ledger.md:33) is the real flag provenance's `WasInvalidatedBy` arm reads (provenance.md:195). Consistent.
