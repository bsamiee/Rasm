# w3 Rasm.Persistence b2 — grounding dossier (VERIFIED PRIMARY EXTRACTS)

Batch: `Ingest/schedule.md` [new] · `Ingest/geospatial.md` [new] · `Store/blobstore.md` [improve] · `Store/provisioning.md` [improve].
Every anchor below is `file:line`. Assay `api --member` faulted (verb is `resolve|query|show|status`, not `--member`); `.api` catalogs are the authoritative fallback and carry the member blocks quoted here.

## [00]-[LS_INVENTORIES]

### doctrine root — `docs/stacks/csharp/` (real `ls`)
`algorithms.md · boundaries.md · domain/ · language.md · rails-and-effects.md · README.md · shapes.md · surfaces-and-dispatch.md · system-apis.md`. Domain shards: `docs/stacks/csharp/domain/` = `compute.md · concurrency.md · data-interchange.md · diagnostics.md · durability.md · interaction.md · persistence.md · postgres.md · README.md · resilience.md · runtime.md · transport.md · validation.md · visuals.md`.

### shared substrate `.api` — `libs/csharp/.api/` (30 files)
`api-csparse · api-extensions-ai · api-generator-equals · api-grpc-* · api-hashing · api-highperformance · api-hybrid-cache · api-jsonpatch · api-languageext · api-mapperly · api-mathnet-numerics · api-mathnet-providers · api-nodatime · api-nodatime-protobuf · api-nodatime-stj · api-protobuf · api-quikgraph · api-redaction · api-system-configuration · api-tensors · api-thinktecture-json · api-thinktecture-messagepack · api-thinktecture-runtime-extensions · api-unicolour · api-unitsnet`.

### folder `.api` — `libs/csharp/Rasm.Persistence/.api/` (79 files; batch-relevant)
`api-pollination-sdk` (blobstore Presigned) · `api-objectstore` (blobstore S3/checksum/SSE/lock) · `api-minio` · `api-mpxj` (schedule) · `api-nts-io` (geospatial GeoJSON/GeoPackage/WKB/WKT) · `api-h3` (geospatial cell) · `api-npgsql-nts` (provisioning ADO codec) · `api-npgsql-otel` (provisioning telemetry) · `api-ef-sqlite` (provisioning embedded EF) · `api-npgsql` · `api-npgsql-ef` · `api-nts-ef` · `api-sqlite` · `api-sqlitepcl` · `api-highperformance` (shim→shared) · `api-hashing` (shim→shared).
Note: `api-hashing.md`, `api-highperformance.md`, `api-hybrid-cache.md`, `api-jsonpatch.md`, `api-nodatime*.md`, `api-redaction.md`, `api-thinktecture-json.md` in the folder tier are 130-170-byte POINTER shims to the shared tier (verified: `wc -c` ~137-169).

## [01]-[BRIEF SEAM/RIDER ANCHORS] — `RASM-CS-PERSISTENCE-DECISION.md`

### schedule (row 21, NEW/new) — `RASM-CS-PERSISTENCE-DECISION.md:119`
> `[V11]` schedule-file codec (`MPXJ.Net` .mpp/XER/PMXML → the record rail) + durable schedule rows — the persisted `TaskRelation` predecessor/successor/lag activity-network DAG … The WRITE half chartered: `UniversalProjectWriter(FileFormat).Write` over the seven writable members (`XER`/`PMXML` P6 round-trip, `MSPDI`/`MPX`, `JSON`) is the schedule EGRESS row on the SAME `ScheduleSource` owner … `Relation.Builder`/`Duration.GetInstance` the synthesis members. `MPXJ.Net` re-verified at the pinned 16.5.0. Band **Schedule 8400** (`ScheduleFault` = `CodecReject`/`UnknownDialect`). Entry `ScheduleSource.Run(ScheduleSpec)` (`[Union]`) + the `TaskRelation` durable-DAG rows. Seams `← Rasm.Bim/schedule` (BIM:102 counterpart, widened read→round-trip); `→ Rasm.Element` (schedule→element row-shape). Leg 3.

Band registry — `RASM-CS-PERSISTENCE-DECISION.md:153`: `8400 | ScheduleFault | Ingest/schedule | 8401-8402 | NEW — CodecReject (bad .mpp/XER/PMXML) · UnknownDialect`.
Counterpart obligation — `:207`: `BIM:102 P6/MS-Project 4D | Sync/schedule | Ingest/schedule.md (V11 NEW) | Bim`. Egress rider — `:222`: `BIM:102's counterpart widens read→round-trip (the UniversalProjectWriter XER/PMXML egress row)`.

### geospatial (row 22, NEW/new) — `RASM-CS-PERSISTENCE-DECISION.md:120`
> The `[A.4]` Ingest growth law fires: ONE `GeoSource.Run(GeoSpec)` (`[Union]`) owner over three format rows — `GeoPackage` (`GeoPackageGeoReader`/`GeoPackageGeoWriter` GPB header+WKB body over the ALREADY-ADMITTED `Microsoft.Data.Sqlite`), `GeoJson` (feature collections through the codec's `GeoJsonProjection` factory; feature `properties` reify via `IPartiallyDeserializedAttributesTable.TryDeserializeJsonObject<T>` under the SAME converter graph), `Wkb`/`Wkt` (core-NTS IO, transitive) — projecting features into the record rail exactly as tabular law states (row shape only) … H3 derivation at ingest composes `identity#SPATIAL_CELL` (`pocketken.H3`, in-process/in-PG cell-parity law). Band **GeoIngest 8440** (`GeoIngestFault` = `CodecReject`/`CrsUnsupported`/`GeometryInvalid`). Entry `GeoSource.Run(GeoSpec)` (`[Union]`). Seams `→ Rasm.Element` (row-shape only, ARCH:61); `← Element/codec#GeoJsonProjection`; `→ Element/identity#SPATIAL_CELL` (H3 cell derivation, leg-3→leg-1 downward); `← Rasm.Bim/Semantics/geospatial`. Leg 3.

Band registry — `:157`: `8440 | GeoIngestFault | Ingest/geospatial | 8441-8443 | NEW — CodecReject · CrsUnsupported · GeometryInvalid`.
Codec seam — `:85` (`Element/codec.md` improve): `The GeoJsonProjection row (:53,134-136, live) KEEPS and additionally serves Ingest/geospatial feature reification — feature properties reify through IPartiallyDeserializedAttributesTable.TryDeserializeJsonObject<T> under the SAME converter graph`. Roster integrate — `:240,241`: GeoJSON4STJ + GeoPackage packages routed here.

### blobstore (row 23, KEEP/improve) — `RASM-CS-PERSISTENCE-DECISION.md:126`
> Nine-delegate `ObjectLeg` (four providers fill once), write-once 412-noop seal, `RemoteStoreFault` structural `Lift`, write-blob-first + full-history GC. `[V10]` `MultipartTransfer.Upload`+`BlobTransferReceipt` become THE composed receipt path (or both die). **E9 checksum honesty** (`ObjectChecksum` axis has `XxHash128`/`Crc64`/`None` rows :32-37; GCS/Minio rows read the SDK-native `Crc64`/`None` stance :42-43, NOT the decorative `XxHash128` only `S3Leg.Seal` supplies via `Integrity.Wire`). `[V10]` `BlobResidence.Correlation` thread-from-write-op-or-drop. **`ObjectStore.Presigned` row** ([05] PollinationSDK integration): the `ObjectClient` case carries a `GrantMinter` delegate `Func<GrantRequest, IO<ObjectGrant>>` … `ObjectGrant` a two-case `[Union]`: `FormPost(Uri Url, HashMap<string,string> Fields)` (the upstream `S3UploadRequest` executed as ONE `multipart/form-data` POST — repo-verified `Helper.UploadArtifactAsync`) and `SignedUrl(Uri)` for GET; Pollination the SEED minter (`ArtifactsApi.CreateArtifact(owner, name, KeyRequest{Key}) → S3UploadRequest` mints writes, `DownloadArtifact`/`JobsApi.DownloadJobArtifact` mint reads, `ListArtifacts → FileMetaList` fills `Head`/`Stat` on `FileMeta { Key, FileType, FileName, LastModified, Size }`); honesty rows: `FileMeta` carries NO checksum/etag → `ObjectChecksum.None`, no multipart/resumable upstream → single-shot `ChunkPolicy` + `conditionalWrite: false`; `RemoteStoreFault.GrantExpired(ContentAddress, Instant)` — ONE new case in-band 5400. `StorageTier` FROZEN vocabulary (V5c). Band **RemoteStore 5400**. Entry `BlobStore.Put/Get/Seal` over `ObjectStore` `[SmartEnum]` (5 rows: 4 credentialed providers + presigned-grant). Seams `← Rasm.Compute` (GLB by RepresentationContentHash, ARCH:59); `← Rasm.Bim/Exchange` (ARCH:60); `→ Rasm.Compute GeometrySource` (`Placement` egress, C2); `← Rasm.AppUi/Collab/sync`. Leg 3.

Verify rider — `RASM-CS-PERSISTENCE-DECISION.md:317`: `verify | api-objectstore.md | CompleteMultipartUploadRequest.ChecksumXXHASH128 | assay api over restored AWSSDK.S3 (pinned 4.0.100.2) before the blobstore batch closes | page Store/blobstore.md | wave 3`.
Acceptance — `:331`: `spatial-cloud-dryrun | Ingest/geospatial.md#GeoSource.Run · Store/blobstore.md#ObjectStore.Presigned · Query/cache.md#ArtifactKind.CloudRun`.
Frozen-vocab inversions — `:337`: `retention(2)←blobstore(3) StorageTier · recovery(2)←blobstore(3) ObjectStore.Head` (earlier-leg consumers hold the frozen shape; any owner change reopens the consuming leg).

### provisioning (row 24, KEEP/improve) — `RASM-CS-PERSISTENCE-DECISION.md:127`
> Verification-first six-command `NpgsqlBatch` fold, `FailureRank.Absorb` absence policy, `ExtensionAdmission` install preconditions, `EngineOps` `Handle`-bridge capsule, embedded residency-split ritual. **`[V4]` `ServerFault` RE-BAND off 835x** (Columnar keeps 835x; ServerFault→838x, absorbs the loose `Error.New` 8371-8375/8379/8380 :72-74,292-293,320,325 as typed cases). **`EmbeddedStore.Refused` 7701/7702 → `EmbeddedFault.Refused`** (disk :433/:440, the fourth breach the E4 census omitted, in-banded to Embedded 771x). `[V6]` embedded charter HONEST (SQLite carries NO ElementGraph event-sourcing path — relational identity floor + `EngineOps` checkpoint/snapshot/backup + read-only/offline tier, never SoR). `ServerExtension.CreateSql` FROZEN install rail (V5c). `Npgsql.OpenTelemetry` observability row. **`NpgsqlDataSourceBuilder.UseNetTopologySuite` ADO codec admission row** ([05] Npgsql.NetTopologySuite): a DATA-SOURCE row on the `StoreProfile` data-source build so RAW Npgsql lanes read/write geometry … `geographyAsDefault`/precision/ordinates are profile POLICY values. **`StoreProfile.Embedded` EF admission clause** ([05] EF-Sqlite): `UseSqlite` over the connection the `EmbeddedStore.Open` ritual already dialed. The `[V13]` axis-map re-emit carries four new axes. Bands **Server 8380** + **Embedded 7710**. Entry `ClusterProvision.Verify/Admit/Reload` + `EmbeddedStore.Open`. Leg 3.

Frozen intra-leg — `:337`: `cypher(3)←provisioning(3) ServerExtension.CreateSql` (frozen contract; cypher ordered before provisioning).

## [02]-[FOLDER CONTEXT ANCHORS]

- Folder page set — `RASM-CS-PERSISTENCE-DECISION.md:114`: `Ingest/ — 3 pages (the one-file-folder violation ends; a real codec-ingress axis)`; `:122`: `Store/ — 3 pages (durable-home + coordination substrate)`.
- Sibling `Ingest/tabular.md` (KEEP/improve, row 20 `:118`) is the pattern law for both new Ingest pages: `TabularSource.Run(TabularSpec)` (`[Union]`), row-shape only, app owns the element map (`tabular.md:3,20`), one STJ wire factory for typed cell minting (`tabular.md:16`, `TabularWire.Bind<T>` `tabular.md:264`), row-boundary `Validation<TabularFault,…>` fold on both legs (`tabular.md:20,225-228`), `TabularFault : Rasm.Domain.Expected, IValidationError<TabularFault>` (`tabular.md:94`), `TabularFact` kind-discriminated receipt stream `store.tabular.*` (`tabular.md:17,133`).
- blobstore current entries: `ObjectStore` `[SmartEnum<string>]` 4 rows (`blobstore.md:236-240`: S3/AzureBlob/Gcs/Minio); `ObjectClient` `[Union]` 4 cases (`blobstore.md:149-154`: S3/Azure/Gcs/Minio); `RemoteStoreFault : Expected, IValidationError` 10 cases 5400-5408 (`blobstore.md:180-231`); `MultipartTransfer.Upload` defined `blobstore.md:362-365`; `BlobResidence` with `CorrelationId Correlation` `blobstore.md:160-162`.
- provisioning current: `ServerFault` Code 8350/8351/8352 (`provisioning.md:218-221`); loose `Error.New` — FailureRank `8371/8372/8373` (`provisioning.md:72-74`), Fold readiness `8374/8375` (`provisioning.md:292-293`), Admit `8379/8380` (`provisioning.md:320,325`); `EmbeddedStore.Refused(store, 7701…)`/`7702` bare `Error.New` (`provisioning.md:433,440,455-456`); `EmbeddedFault : Expected` 7711-7714 (`provisioning.md:519-531`); `EngineOps` `Handle`-bridge (`provisioning.md:560-620`); `StoreProfile` `[SmartEnum<string>]` Server/Embedded (`provisioning.md:37-43`).

## [03]-[VERIFIED .api MEMBER BLOCKS]

### blobstore — `api-objectstore.md`
- `api-objectstore.md:125`: `CompleteMultipartUploadRequest.ChecksumXXHASH128` (string, base64) — precomputed whole-object digest; also `.ChecksumCRC64NVME`/`.ChecksumSHA256`/`.ChecksumCRC32`/`.ChecksumCRC32C`. `InitiateMultipartUploadRequest.ChecksumAlgorithm` (`ChecksumAlgorithm.XXHASH128`) + `.ChecksumType` (`ChecksumType.FULL_OBJECT`). [confirms verify-rider `:317`]
- `api-objectstore.md:62`: `ChecksumType` `ConstantClass` — `FULL_OBJECT` vs `COMPOSITE`.
- `api-objectstore.md:125`: SSE on INITIATE — `ServerSideEncryptionMethod`/`ServerSideEncryptionKeyManagementServiceKeyId`, `ServerSideEncryptionCustomerMethod`/`…CustomerProvidedKey`/`…CustomerProvidedKeyMD5`. WORM on INITIATE — `ObjectLockMode.Governance/.Compliance` + `ObjectLockRetainUntilDate` (`DateTime?`) + `ObjectLockLegalHoldStatus`. NOTE: `ObjectLockLegalHoldStatus` is an admitted-but-unexploited member (blobstore models Governance/Compliance only, no legal-hold case).
- `api-objectstore.md:189`: content-key object name IS the `Element/codec#CONTENT_ADDRESS` `XxHash128`, supplied AS whole-object checksum on S3, precalculated CRC64 on Azure/GCS — the E9 honesty precedent the DECISION cites.

### blobstore Presigned — `api-pollination-sdk.md` (v1.10.0, netstandard2.0, sidecar-isolated)
- `api-pollination-sdk.md:71` (DTO index [04]): `S3UploadRequest` / `KeyRequest` / `FileMeta` / `FileMetaList` — presigned-S3 upload request, key request, artifact metadata.
- `api-pollination-sdk.md:112`: `ArtifactsApi.CreateArtifactAsync(string owner, string name, KeyRequest keyRequest, …) → Task<S3UploadRequest>` (the presigned upload seam).
- `api-pollination-sdk.md:113`: `ArtifactsApi.DownloadArtifactAsync(owner, name, path…) → Task<object>`; `ListArtifactsAsync(owner, name, path…) → Task<FileMetaList>`.
- `api-pollination-sdk.md:42`: `JobsApi.DownloadJobArtifact` (read-mint leg).
- `api-pollination-sdk.md:122` [ARTIFACT_S3_SEAM]: the presigned URL uploads to/fetches from the SAME object plane `Store/blobstore` drives (`AWSSDK.S3`/`Minio`) — one transfer rail, not a second uploader. `RunAsset` lands content-keyed `XxHash128` via the `AsStream` body bridge.
- NOT in catalog: `Helper.UploadArtifactAsync` and the `multipart/form-data` field enumeration — DECISION `:126` calls these "repo-verified"; downstream implement must confirm against the restored PollinationSDK assembly (catalog carries `S3UploadRequest` as a DTO but not its field members).

### schedule — `api-mpxj.md` (v16.5.0, LGPL-2.1, `lib/net6.0`, IKVM-translated, osx-arm64-clean)
- `api-mpxj.md:64`: `new UniversalProjectReader().Read(string fileName|Stream) → ProjectFile` (format-sniff auto-dispatch — the ONE ingress).
- `api-mpxj.md:65`: `UniversalProjectReader.ReadAll(string|Stream) → IList<ProjectFile>` (multi-project XER containers).
- `api-mpxj.md:68`: `new UniversalProjectWriter(FileFormat).Write(ProjectFile, string|Stream)` (egress); `Write(IList<ProjectFile>,…)`.
- `api-mpxj.md:70`: `FileFormat` enum — writable set `JSON`/`MPX`/`MSPDI`/`PLANNER`/`PMXML`/`XER`/`SDEF` (SEVEN; no MPP writer, `:83`).
- `api-mpxj.md:71`: `Relation.Builder(ProjectFile).PredecessorTask(t).SuccessorTask(t).Type(rt).Lag(d).Build()` (synthesis).
- `api-mpxj.md:72`: `Duration.GetInstance(double|int magnitude, TimeUnit type)` (unit-tagged mint).
- `api-mpxj.md:38-40`: `ProjectFile` (`Tasks`/`Resources`/`Relations`/`Calendars`/`ResourceAssignments`/`ProjectProperties`/`ChildTasks`); `Task` (`Predecessors`/`Successors` `IList<Relation>`, `ChildTasks`, `TotalSlack`/`FreeSlack` `Duration`, `ConstraintType?`/`ConstraintDate DateTime?`); `Relation` (`PredecessorTask`/`SuccessorTask`, `Type RelationType?`, `Lag Duration`).
- `api-mpxj.md:41`: `RelationType` enum — `FinishStart`/`StartStart`/`FinishFinish`/`StartFinish`.
- `api-mpxj.md:48`: `TimeUnit` enum — `Minutes`…`Years`/`Percent` + `Elapsed*`.
- `api-mpxj.md:98-99` [IKVM_BOUNDARY]: proxy types carry a `JavaObject` handle behind `IHasJavaObject`/`IJavaObjectProxy<T>`; map `ProjectFile` at ONE seam, the handle never threads into canonical code.
- `api-mpxj.md:104` [INTEGRATION_STACK]: the `Task`/`Relation` network is the `SequenceRel` DAG `Rasm.Bim`'s `QuikGraph` runs CPM over — MPXJ owns parse/serialize only, `:113` no CPM in the codec.

### geospatial — `api-nts-io.md` (GeoJSON4STJ 4.0.0, GeoPackage 2.0.0, NTS core 2.6.0; all BSD-3)
- `api-nts-io.md:74-75`: `GeoPackageGeoReader.Read(byte[]|Stream) → Geometry`; `GeoPackageGeoWriter.Write(Geometry) → GeoPackage blob`.
- `api-nts-io.md:76-78`: `GeoPackageBinaryHeader` (magic `GP`, `Version`, `Flags`, `Ordinates`, `SrsId`, `Envelope Extent`, `Interval ZRange`/`MRange`); static `Read(BinaryReader)`/`Write(BinaryWriter)`.
- `api-nts-io.md:63,67,132-133`: `IPartiallyDeserializedAttributesTable : IAttributesTable`; `TryDeserializeJsonObject<T>(JsonSerializerOptions, out T)` (whole table→typed CLR); `TryGetJsonObjectPropertyValue<T>(name, options, out T)` — SAME options graph carrying `GeoJsonConverterFactory`.
- `api-nts-io.md:51,56,122-123`: `GeoJsonConverterFactory` (10 ctor overloads incl. `ringOrientationOption`, `allowModifyingAttributesTables`); admitted via `JsonSerializerOptions.Converters.Add`.
- `api-nts-io.md:85-89`: `WKBReader.Read(byte[]|Stream)`, `WKBReader.HexToBytes(string)`; `WKBWriter.Write(Geometry)→byte[]`/`(Geometry,Stream)`, `WKBWriter.ToHex(byte[])`, ctor `(ByteOrder, handleSRID, emitZ, emitM)`, `EncodingType`/`HandleSRID`.
- `api-nts-io.md:94-100`: `WKTReader.Read(string|Stream|TextReader)`; `WKTWriter.Write(Geometry)→string`/`WriteFormatted`, `OutputOrdinates`/`PrecisionModel`.
- `api-nts-io.md:196-199` [INTEGRATION_LAW]: four codecs share ONE `NtsGeometryServices`/`GeometryFactory` precision+SRID; WKB is canonical interchange keyed by `XxHash128.HashToUInt128` for `Element/codec#CONTENT_ADDRESS` content-stable identity; GeoJSON stacks the SAME `JsonSerializerOptions` as the rest of Persistence.
- UNEXPLOITED (concept-admitted): `RingOrientationOption.EnforceRfc9746` (`:55,175`), `GeoPackageGeoWriter.HandleOrdinates`/reader `HandleSRID`/`RepairRings` (`:144-145`), `WKBWriter` EWKB SRID embedding (`:188`) — geospatial CrsUnsupported/GeometryInvalid handling should compose these.

### geospatial H3 — `api-h3.md` (pocketken.H3 4.0.0, Apache-2.0, `lib/net6.0`, managed, composes NTS 2.6.0)
- `api-h3.md:47`: `H3Index.FromPoint(Point point, int resolution)` — NTS `Point` (SRID 4326) → cell (the PostGIS-geometry bridge).
- `api-h3.md:46`: `H3Index.FromLatLng(LatLng, int resolution)` (v4 `latLngToCell`).
- `api-h3.md:51,89`: `H3Index.ToPoint(GeometryFactory? = null)` (centroid Point); `H3Index.GetCellBoundary(GeometryFactory? = null) → Polygon`.
- `api-h3.md:86`: `Geometry.Fill(this Geometry, int res, VertexTestMode = Center) → IEnumerable<H3Index>` (v4 `polygonToCells`).
- `api-h3.md:37`: implicit `H3Index↔ulong`; `(ulong)index` the durable form; mutable class — store the ulong, never share a live instance.
- `api-h3.md:26,134`: `H3Index.Invalid` (zero ulong) → `Option<H3Index>.None` at boundary; never a stored `0` cell.
- `api-h3.md:33`: `H3.H3IndexJsonConverter : JsonConverter<H3Index>` (STJ hex-string).
- `api-h3.md:139` [STACKING]: `H3Index.FromPoint(point, res)` at ingest computes bit-identical to `h3-pg` `h3_latlng_to_cell` server-side — the cell-parity law (`Store/provisioning` `h3`/`h3_postgis` rows, `Element/identity#SPATIAL_CELL`).

### provisioning — `api-npgsql-nts.md` (Npgsql.NetTopologySuite ADO codec)
- `api-npgsql-nts.md:37`: `NpgsqlDataSourceBuilder.UseNetTopologySuite(...)` — registers geometry codecs on the store-profile data source.
- `api-npgsql-nts.md:31`: overloads accept `CoordinateSequenceFactory?`, `PrecisionModel?`, `Ordinates handleOrdinates = Ordinates.None`, `bool geographyAsDefault = false`.
- `api-npgsql-nts.md:42-43`: admit on the data source BEFORE any connection opens; `geographyAsDefault`/precision/coord-seq/ordinate are profile policy values, not call-site literals. NOTE `:3` — EF plugin catalogue is `api-nts-ef.md`; this is the RAW-ADO codec (the EF plugin does not place the codec on raw connections).

### provisioning — `api-npgsql-otel.md` (Npgsql.OpenTelemetry 10.0.3, `net8.0`)
- `api-npgsql-otel.md:34`: `TracerProviderBuilderExtensions.AddNpgsql(TracerProviderBuilder) : TracerProviderBuilder` (subscribes the Npgsql `ActivitySource`).
- `api-npgsql-otel.md:35`: `MeterProviderBuilderExtensions.AddNpgsqlInstrumentation(MeterProviderBuilder, Action<NpgsqlMetricsOptions>? = default) : MeterProviderBuilder`.
- `api-npgsql-otel.md:37,72`: `NpgsqlMetricsOptions` parameterless ctor, NO settable knobs at 10.0.3 — bucketing rides OTel meter-view config. `:76` two builder-registration rows, never op-body code.

### provisioning — `api-ef-sqlite.md` (Microsoft.EntityFrameworkCore.Sqlite)
- `api-ef-sqlite.md:96`: `UseSqlite(DbConnection, [bool contextOwnsConnection,] Action<…>?)` — admits SQLite over a pre-built `SqliteConnection` (the connection `EmbeddedStore.Dialed`/`Open` already dialed).
- `api-ef-sqlite.md:95,126`: `UseSqlite(connectionString?, Action<SqliteDbContextOptionsBuilder>?)`; 8 overloads = `{connectionString|DbConnection|DbConnection+contextOwnsConnection|parameterless}` × `{builder|builder<TContext>}`.
- `api-ef-sqlite.md:50`: provider `Internal` types resolve through the EF service provider via `UseSqlite`; a consumer never references them by type.

## [04]-[ASSAY STATUS]
`uv run python -m tools.assay api --member ChecksumXXHASH128` FAULTED — `parse: Unknown command "--member". Available commands: resolve, query, show, status.` (exit 2). All member claims above are fallback-verified against the `.api` catalogs (authoritative per prompt fallback law); the load-bearing verify-rider member `CompleteMultipartUploadRequest.ChecksumXXHASH128` is confirmed at `api-objectstore.md:125` and remains a wave-3 assay re-verify obligation against the restored AWSSDK.S3 4.0.100.2 assembly.
