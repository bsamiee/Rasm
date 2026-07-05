# W3 GROUNDING — Rasm.Persistence batch b1 (Query/cypher, Query/cache, Query/federation[new], Ingest/tabular)

Verified primary extracts only. Every cited member has a `file:line` anchor. No doctrine digest, no removal framing.

## [00]-[INVENTORIES] — real `ls` of both .api tiers + doctrine root

### Doctrine root — `docs/stacks/csharp/` (`ls`, 2026-07-05)
```
algorithms.md  boundaries.md  domain/  language.md  rails-and-effects.md  README.md  shapes.md  surfaces-and-dispatch.md  system-apis.md
```
`docs/stacks/csharp/domain/`:
```
README.md compute.md concurrency.md data-interchange.md diagnostics.md durability.md interaction.md
persistence.md postgres.md resilience.md runtime.md transport.md validation.md visuals.md
```
Atlas order (`docs/stacks/csharp/README.md:11-33`): language -> shapes -> surfaces-and-dispatch -> rails-and-effects -> boundaries -> algorithms -> system-apis -> domain/README -> runtime/concurrency/diagnostics/validation/resilience/transport/persistence/durability/postgres/data-interchange/compute/visuals/interaction. Analyzer law: `dotnet_style_namespace_match_folder = true:error` (namespace ALWAYS equals folder path).

### Shared substrate tier — `libs/csharp/.api/` (31 files)
```
api-csparse api-extensions-ai api-generator-equals api-grpc-{aspnetcore,client-web,client,common,core-api,tools}
api-hashing api-highperformance api-hybrid-cache api-jsonpatch api-languageext api-mapperly
api-mathnet-numerics api-mathnet-providers api-nodatime{,-protobuf,-stj} api-protobuf api-quikgraph
api-redaction api-system-configuration api-tensors api-thinktecture-{json,messagepack,runtime-extensions}
api-unicolour api-unitsnet
```

### Folder tier — `libs/csharp/Rasm.Persistence/.api/` (79 files; cited subset)
```
api-flowtide-substrait api-duckdb api-arrow api-adbc-apache api-adbc-bigquery api-apache-age api-pgrouting
api-scylladb api-redis api-pollination-sdk api-hybrid-cache(stub) api-sep api-linq2db-ef api-miniexcel
api-npgsql api-marten api-objectstore api-h3 api-pg-graphql api-pg-search api-pgvector-ef api-pgvectorscale
```
NOTE: folder `api-hybrid-cache.md` is a 160-byte stub (`libs/csharp/Rasm.Persistence/.api/api-hybrid-cache.md` = 160 bytes) — the real catalog is the shared tier `libs/csharp/.api/api-hybrid-cache.md` (12818 bytes).

## [01]-[DECISION/BRIEF SEAM + RIDER ANCHORS] (these 4 pages)

### cypher (`RASM-CS-PERSISTENCE-DECISION.md:110`)
> `Query/cypher.md` | KEEP · `improve` | ... **`[V4]` `GraphFault`→`CypherFault` RENAME** (disk simple-name ×2: `graph.md:154` `GraphFault` ns `Rasm.Persistence` vs `cypher.md:60` ns `Rasm.Persistence.Query`; graph keeps `GraphFault`). `[V10]` `AgtypePath.Weight`-always-0.0 dead carrier (populate or collapse to `Seq<NodeId>`). Band **Cypher 8360** (8360-8363, keeps 836x). | `← Store/provisioning#ServerExtension.CreateSql` (frozen-vocab install rail, V5c); optional/demoted beneath QuikGraph

Band registry `RASM-CS-PERSISTENCE-DECISION.md:149`: `8360 | CypherFault | Query/cypher | 8360-8363 | KEEP 836x + RENAME from GraphFault`. FaultBand is ONE `[SmartEnum<int>]` on `Element/graph.md#[FAULT_TABLES]`; every union `Code => Band + n` (`RASM-CS-PERSISTENCE-DECISION.md:132`). Frozen vocab contract (`:40`): `ServerExtension.CreateSql` row shape frozen, owner `Store/provisioning` (leg 3), consumer `Query/cypher` (leg 3 ordered earlier) — `cypher.md:3` installs through it.

### cache (`RASM-CS-PERSISTENCE-DECISION.md:111`)
Three integration rows the improve ADDS (all absent on disk):
> **`ArtifactKind.CloudRun` row** ([05] PollinationSDK): `new("cloud-run", RetentionClass.Cache, CacheLane.ModelResult)` — content-keys the run receipt over recipe digest `PackageVersion.Digest` · input-asset content keys · project slug ... SUPERSEDES the SDK's verifiably weak reuse (`Helper.CheckCached` path-existence-only, `Wrapper.LocalDatabase` bare SQLite ledger).
> **Wide-column index-residency axis** ([05] ScyllaDB): provider rows `marten-pg` (default) · `scylla-widecolumn` (scale-out) — partition key `(TenantId, ArtifactKind)`, `InsertIfNotExistsAsync → AppliedInfo<T>` claim-gated admission ... `FetchPageAsync<T>`+`PagingState` sweep scans, `ConsistencyLevel`/retry as named `ExecutionProfiles` rows, `TokenAwarePolicy` shard routing.
> **Redis invalidation-backplane row** beside `#L2_CONTRIBUTION` ([05] Redis): `ISubscriber`/`ChannelMessageQueue` on ONE channel keyed `(storeKey, tenant)` carrying evicted-key beats ... `CacheLane.Store`-gated.

Band registry `RASM-CS-PERSISTENCE-DECISION.md:158`: `8450 | WideColumnFault | Query/cache | 8451-8454 | Unavailable · WriteTimeout · LwtRefused · HostDown` (ONE-boundary `DriverException` conversion). `cache.md` leaves the no-band list (`:162`).

### federation (`RASM-CS-PERSISTENCE-DECISION.md:112`) — NEW, V1/S1
> **`[V1]/S1` REINTRODUCE** ... Substrait protobuf bytes → `Substrait.Protobuf.Plan.Parser.ParseFrom` + `new SubstraitDeserializer().Deserialize` (shipped PUBLIC, ~2 lines — falsifies `api-flowtide-substrait.md:151`) → a `LoweringTarget` `RelationVisitor` (~150-250 LOC) onto `SetExpr` (SetRelation→Union/Intersect/Difference 3-for-3, ReadRelation+Filter→Predicate, VirtualTableRead→Literal, bounded Iteration→Closure, key-semijoin Join→Intersect) ... OR the columnar/ADBC lane (Project/Aggregate/Sort/TopN/Window→columnar; Write→REJECT fail-closed; Exchange→DROP). Plan digest = `ContentHash.Of(wireBytes)`. Executes against ONE `TimeCut` (default `StalenessWatermark.HeadSequence` head, `lane.md:40`); returns `ElementSet` receipt + Arrow batch; `(plan-digest·cut·watermark)` triple content-addresses; replay is one `ArtifactKind` reuse row. `SourceKind` capability axis (Substrait-native durable-store/signed-artifact/ADBC vs SQL-only staged). DuckDB substrait extension row rides `ColumnarExtension.Substrait`; `Google.Protobuf@3.35.1` (shared) sole runtime dep. Band **Federation 8420**.

Seams (`:112`): `← python:data/tabular/query` (Substrait portable-plan half, ARCH:57b SPLIT); `→ Query/lane#SetExpr` + `→ Query/columnar` (intra-leg lowering/execution); `← provenance#SignedArtifact`. Band registry `:155`: `8420 | FederationFault | 8421-8425 | SubstraitParse · UnsupportedRelation · SourceUnreachable · WriteRejected · SourceUncapable`.

### tabular (`RASM-CS-PERSISTENCE-DECISION.md:118`)
> `Ingest/tabular.md` | KEEP · `improve` | One rectangular-data owner: MiniExcel spreadsheet + **`Sep` as EXPLICIT owner law** (a fenced SIMD delimited-lane surface, the `[V11]` charter made real). `[V10]/NEW` `TabularWire.Wire` disposition (compose the `DynamicExcelColumn` `CustomFormatter` into `Policy().DynamicColumns`, OR delete `Wire`). `linq2db.EntityFrameworkCore` COMMITTED ... `LinqToDBForEFTools.BulkCopyAsync<T>(context, BulkCopyOptions, IAsyncEnumerable<T>, ct) → Task<BulkCopyRowsCopied>` over the V6 identity `DbContext` — `ProviderSpecific` lowers to PG binary COPY, mapping derives from the EF model (`EFCoreMetadataReader`) ... `TabularFault.BulkRefused` in-band 8390. `[V4]` `TabularFault` RE-BAND off 837x. Band **Tabular 8390**.

Band registry `:152`: `8390 | TabularFault | Ingest/tabular | re-banded 839x | RE-BAND off 837x (was 8370-8373) + BulkRefused in-band`.

## [02]-[ON-DISK FENCE ANCHORS — the improve baselines]

### cypher.md
- `libs/csharp/Rasm.Persistence/.planning/Query/cypher.md:35` namespace `Rasm.Persistence.Query`.
- `:60` `public abstract partial record GraphFault : Expected, IValidationError<GraphFault>` — the RENAME target (→ `CypherFault`).
- `:67-71` `Code` hardcoded 8360/8361/8362/8363 (NOT `Band + n` off a registry).
- `:198` `public readonly record struct AgtypePath(Seq<NodeId> Vertices, double Weight);`
- `:310` `GraphSession.Decode(...).Map(static id => new AgtypePath(Seq(id), 0.0))` — Weight ALWAYS 0.0 AND Vertices ALWAYS length-1 (single vertex per row, never the full AGE path).
- `:221` `Flow(EdgesSql Edges, H3Cell Source, H3Cell Sink)`; `:397` `SELECT pgr_maxFlow(@edges, @source, @sink)`; `:238` `Flowed(long MaxFlow)` — scalar only; no per-edge flow.
- `:184-192` `CleaveKind` rows Strong/Connected/Articulation/Bridges — no `biconnected`.
- `:220` `Tour(...)`; no `Kth`-style VIA or point-routing verb; no bulk-load.
- `:3` installs through `Store/provisioning#SERVER_EXTENSIONS ServerExtension CreateSql` (matches frozen-vocab V5c).

### cache.md (grade-9 baseline; ZERO of the 3 integration catalogs present)
- `:26-39` `ArtifactKind` five rows (Interchange/EpContext/OnnxProfile/IfcSemantic/ChunkContent) — NO `CloudRun`.
- `:191-218` `CacheL2Store : IBufferDistributedCache` over Marten; `:222-232` `CacheCodecFactory`/`CacheCodec<T>` MessagePack; `:236-242` `CachePartition.Scoped` tenant partition.
- `:184` Packages: Microsoft.Extensions.Caching.Hybrid, Marten, MessagePack, System.IO.Hashing, AppHost, Rasm.Element, LanguageExt — NO ScyllaDBCSharpDriver, NO StackExchange.Redis, NO PollinationSDK.
- `:185` growth: "a redis swap is the `Microsoft.Extensions.Caching.StackExchangeRedis` `RedisCache`" — the L2 SWAP only; NO pub/sub backplane, NO stream, NO wide-column residency.

### tabular.md (grade-8 baseline)
- `:18` Packages: MiniExcel, Microsoft.Extensions.Compliance.Redaction, `Sep (delimited sibling)`, LanguageExt, Thinktecture, NodaTime — Sep listed, but NO fenced Sep surface anywhere in the fence; NO linq2db package.
- `:40-44` `Policy()` returns `OpenXmlConfiguration{...}`/`CsvConfiguration{...}` with NO `DynamicColumns` set.
- `:272-273` `TabularWire.Wire(string column)` builds a `DynamicExcelColumn{ CustomFormatter=... }` — but `Policy()` (`:40`) never assigns it to `DynamicColumns`, so `Wire` is built-but-never-composed (phantom; prose `:5,16,271` claims it "is the cell projection on the dynamic/reader/write legs").
- `:103-107` `TabularFault.Code` hardcoded 8370/8371/8372/8373 (collides `topology.md` 8370-8371; must re-band 839x + add `BulkRefused`).
- `:5,20` linq2db bulk-copy is prose-only ("the linq2db/EF bulk-copy lane ... consumes the typed `IEnumerable<T>`") — no `LinqToDBForEFTools.BulkCopyAsync` fence.
- Sibling parity: `columnar.md:19` `Query<T>` cite pattern; `lane.md:131` `ElementSet` receipt currency the record rail projects into.

## [03]-[VERIFIED .api MEMBER BLOCKS — every member the maps cite]

### api-flowtide-substrait (federation, S1) — `libs/csharp/Rasm.Persistence/.api/api-flowtide-substrait.md`
- `:32` `SubstraitDeserializer` PUBLIC: `Deserialize(Substrait.Protobuf.Plan)` / `Deserialize(string json)` (instance) + static `DeserializeFromJson(string)` → managed `Plan`.
- `:33` `Substrait.Protobuf.Plan` generated `IMessage`; static `Parser` (`MessageParser<Plan>`: `ParseFrom(byte[]/ReadOnlySpan<byte>/CodedInputStream)`); `WriteTo`/`ToByteArray` for retained wire bytes.
- `:34` `SubstraitSerializer` is `internal` — no public managed→protobuf; RETAIN inbound bytes.
- `:47` `RelationVisitor<TReturn, TState>` `Visit` + `VisitReadRelation`/`VisitJoinRelation`/... double-dispatch (base throws `NotImplementedException`, `:153`).
- `:54` `SetRelation` `SetOperation` (union/intersect/except); `:48` `ReadRelation` (`BaseSchema`/`NamedTable`/`Filter?`); `:49` `FilterRelation`; `:50` `ProjectRelation`; `:51` `JoinRelation`/`MergeJoinRelation` (`JoinType`, `PostJoinFilter`, `IsFieldFromLeft`); `:52` `AggregateRelation`; `:53` `SortRelation`/`TopNRelation`/`FetchRelation`; `:55` `WriteRelation` (sink); `:59` `ExchangeRelation`/`TableFunctionRelation`/`VirtualTableReadRelation`/`IterationRelation`; `:58` `ConsistentPartitionWindowRelation`.
- `:35` `Conversion.SubstraitToDifferentialCompute.Convert(Plan, addWriteRelation, tableName, params primaryKeys)` — streaming differential-compute materialized view (admitted, DECISION-unnamed).
- `:89` `Sql.SqlPlanBuilder` `AddTableDefinition`/`AddTableProvider`/`AddPlanAsView`/`Sql(text)`/`GetPlan()`; `:90` `ITableProvider`; `:91` `ISqlFunctionRegister`.
- `:95-98` `FunctionExtensions.Functions*` URI catalogs (function refs, never magic strings); `:70` `ExpressionVisitor<TOutput,TState>` (predicate pushdown); `:36` `Hints.Hint`/`HintOptimizations`.
- `:37` `Exceptions.SubstraitParseException` (→ FederationFault.SubstraitParse).
- version `:9` `0.15.0`, license Apache-2.0.

### api-duckdb (federation execution) — cited via `columnar.md` `ColumnarExtension.Substrait` row (DECISION `:109` "ADD ColumnarExtension.Substrait row (DuckDB from_substrait(blob), V1 lane)"); ADBC bridge `columnar.md:16,284` `AdbcStatement.SqlQuery`→`ExecuteQueryAsync`→`QueryResult.Stream` (`IArrowArrayStream`).

### api-adbc-{apache,bigquery} (federation external-source rows) — `libs/csharp/Rasm.Persistence/.api/api-adbc-*.md` present; the ADBC-warehouse `SourceKind` rows (READ-TIME currency). [folder-tier catalogs confirmed present via ls]

### api-apache-age (cypher) — `libs/csharp/Rasm.Persistence/.api/api-apache-age.md`
- `:74` `ag_catalog.cypher(graph_name, query_string, params agtype) RETURNS SETOF record`; mandatory column-definition list (`:77`).
- `:87` mutate-and-return in one statement REJECTED (split CREATE/SET from trailing MATCH..RETURN).
- `:99-106` agtype operators `->`/`->>`/`#>`/`#>>`/`@>`/`<@`; `:108` casts `expr::vertex|edge|path`, `agtype::int|text|jsonb`.
- `:97` agtype `path` = alternating `[vertex, edge, ...]::path` (the full multi-vertex path the current single-vertex `AgtypePath` decode collapses).
- `:61-66` graph/label lifecycle: `create_graph`/`drop_graph(cascade)`/`create_vlabel`/`create_elabel`/`drop_label`/`alter_graph('RENAME')` — the DDL surface GraphSession has NO owner for.
- `:125-126` `load_labels_from_file`/`load_edges_from_file` (bulk server-side CSV load); `:127` `create_complete_graph`; `:128` `age_vle` (the `*`-range engine `Reach` lowers to).
- `:136` VLE cache coherence: `age_invalidate_graph_cache()` trigger on label backing relations (direct SQL writes stay coherent).
- `:33-35` per-session `LOAD 'age'` + `SET search_path = ag_catalog,"$user",public` (the physical-connection hook `cypher.md:96-111` composes).

### api-pgrouting (cypher) — `libs/csharp/Rasm.Persistence/.api/api-pgrouting.md`
- `:72` `pgr_KSP(Edges SQL, start, end, K[, directed, heap_paths])` MULTI-PATH `path_id=1` cheapest (current `Kth` composes it, `cypher.md:340`).
- `:67` `pgr_dijkstraVia(Edges SQL, via vids[, directed, strict, U_turn_on_edge])` VIA multi-waypoint — output VIA = MULTI-PATH + `route_agg_cost` (`edge=-2` on last node); NOT modeled (distinct from TSP optimal-order).
- `:73` `pgr_withPoints`/`…Cost`/`…CostMatrix`/`…DD`/`…KSP` (off-node Point routing; negative node ⇒ a Point) — NOT modeled.
- `:91` `pgr_TSPeuclidean(Coordinates SQL, start_id, end_id)` — coordinate-based TSP, no cost matrix; NOT modeled.
- `:94` FLOW family `pgr_boykovKolmogorov`/`pgr_pushRelabel`/`pgr_edmondsKarp(Edges SQL, source, sink)` → FLOW `(seq, edge, start_vid, end_vid, flow, residual_capacity)` — per-edge flow assignment. Current `Flow` (`cypher.md:397`) calls ONLY scalar `pgr_maxFlow` → `Flowed(long)` (`:93` `pgr_maxFlow → bigint`), discarding the per-edge assignment.
- `:113` `pgr_biconnectedComponents(Edges SQL)` → `(seq, component, edge)` undirected edge-components — NO `CleaveKind` row (Strong/Connected/Articulation/Bridges only, `cypher.md:185-188`).
- `:116` `pgr_extractVertices(Edges SQL, dryrun)` → vertices-table derivation; `:106` supersedes the removed `pgr_createTopology`/`pgr_nodeNetwork` family.
- `:127` H3 node-space seam: `pocketken.H3 GridPathCells`/`GridDistance` share the H3-cell id node space with in-database `pgr_dijkstra`.

### api-scylladb (cache wide-column) — `libs/csharp/Rasm.Persistence/.api/api-scylladb.md`
- `:81` `AppliedInfo<T>` LWT verdict (`Applied` bool + existing row when `IF` not applied) — the write-once claim gate (matches blobstore 412-noop `cache.md:20` prose).
- `:76,188` `Mapper.InsertIfNotExistsAsync<T>(poco) → Task<AppliedInfo<T>>` (LWT conditional insert); `UpdateIfAsync<T>`.
- `:83,185` `FetchPageAsync<T>(Cql.New(...).WithOptions(o=>o.SetPageSize(n))) → Task<IPage<T>>`; `IPage<T> : ICollection<T>` with `PagingState`/`CurrentPagingState` (`byte[]`) stateless cursor.
- `:80` `Map<T>` `PartitionKey`/`ClusteringKey`/`Column` — partition key `(TenantId, ArtifactKind)`, clustering recency/`ContentAddress`.
- `:110` `TokenAwarePolicy` shard routing; `:116` `IExecutionProfile`/`IExecutionProfileBuilder` named consistency/retry bundles; `:113` retry policy family; `:135` `DriverException` (+ `NoHostAvailableException`/`WriteTimeoutException`/`UnavailableException`/`OperationTimedOutException`) — the ONE-boundary `Fin` fold (→ WideColumnFault Unavailable/WriteTimeout/LwtRefused/HostDown).
- `:127` `CqlVector<T>` ANN column (co-located embedding). assembly `ScyllaDB` (`:25`), namespace `Cassandra.*` (`:26`), netstandard2.0 (`:27`), Apache-2.0.

### api-redis (cache backplane) — `libs/csharp/Rasm.Persistence/.api/api-redis.md`
- `:39,173-174` `ISubscriber.SubscribeAsync(channel) → ChannelMessageQueue`; `ChannelMessageQueue : IAsyncEnumerable<ChannelMessage>` — `await foreach (var m in queue.WithCancellation(token))` backpressure-safe drain (the invalidation backplane keyed `(storeKey,tenant)`).
- `:137-139` `ISubscriber.Subscribe`/`Publish`/`PublishAsync` (the evicted-key beat publish).
- `:206` RESP3 `__redis__:invalidate` broadcast push (server-assisted client-side caching); `:157` `ConfigurationOptions.Protocol = RedisProtocol.Resp3`.
- `:69,90` `RedisCache : IBufferDistributedCache` (the L2 swap row survives the buffer contract — `api-hybrid-cache:90` sniffs it).
- `:56-59,122-128` Stream family `StreamEntry`/`StreamAdd(key,pairs,messageId,...,trimMode)` XADD/`StreamReadGroup`/`StreamAcknowledge`/`StreamTrimMode.Acknowledged` (the egress `EgressSink.RedisStream` sink — routed to `Version/egress`, NOT cache). version `3.0.7`.

### api-pollination-sdk (cache CloudRun) — `libs/csharp/Rasm.Persistence/.api/api-pollination-sdk.md`
- `:112` `ArtifactsApi.CreateArtifactAsync(owner, name, KeyRequest) → Task<S3UploadRequest>` (presigned S3 upload — routes byte transfer to `Store/blobstore` object plane).
- `:113` `ArtifactsApi.DownloadArtifactAsync`/`ListArtifactsAsync → Task<FileMetaList>`; `:71` `FileMeta { Key, FileType, FileName, LastModified, Size }` — carries NO checksum/etag (→ `ObjectChecksum.None` per DECISION blobstore row).
- `:126,128` result-landing seam: `Run` + `RunOutputAsset`s → `Version/provenance` lineage + `Query/cache#ArtifactKind.CloudRun` index.
- `:61` `Wrapper.LocalDatabase` bare `Microsoft.Data.Sqlite` cache; `Wrapper` `CheckLocalJobStatus` — the "verifiably weak reuse" the content-keyed index supersedes.
- netstandard2.0 (`:14`), MIT, vendored `LBT.RestSharp`/`LBT.Newtonsoft.Json` sidecar-isolated (`:118`).

### api-hybrid-cache (cache L2 + backplane invalidation) — `libs/csharp/.api/api-hybrid-cache.md`
- `:39` `RemoveByTagAsync(string tag | IEnumerable<string> tags)` — the tag-invalidation the Redis backplane beat drives (each process tag-invalidates on receipt).
- `:90-92` `IBufferDistributedCache : IDistributedCache` sniffed at construction; `TryGetAsync(IBufferWriter<byte>)`/`SetAsync(ReadOnlySequence<byte>)` zero-copy; `RedisCache` implements it.
- `:43` `HybridCacheEntryFlags` (`DisableLocalCache=3`, etc.) — the L1/L2 lane routing (`cache.md:250`).
- `:82,86` `DistributedCacheServiceKey` selects which keyed L2 backs (the provider-row swap).

### api-sep (tabular delimited owner) — `libs/csharp/Rasm.Persistence/.api/api-sep.md`
- `:61-70` `SepReader` (sync `IEnumerable<Row>` + async `IAsyncEnumerable<Row>`); `SepReader.Row`/`Col`/`Cols` ref-struct projections; `SepReaderHeader`.
- `:101-106` `Sep.Reader(Func<SepReaderOptions,SepReaderOptions>)` functional config; `Sep.Writer(Func<...>)`; `Strict`.
- `:179-182` `Enumerate(RowFunc<T>)`/`Enumerate(RowTryFunc<T>) → IEnumerable<T>` (the record-rail materialization boundary + the bulk source); `EnumerateAsync → IAsyncEnumerable<T>`; `ParallelEnumerate(RowFunc<T>, int degreeOfParallelism)`.
- `:158-159` `Col.Parse<T>()` `where T:ISpanParsable<T>`; `Col.TryParse<T>() → T?`; `:160` `Cols.Parse<T>(Span<T>)` into caller buffer.
- `:163-166` `Header.IndexOf`/`IndicesOf(Span<string>,Span<int>)`/`NamesStartingWith(prefix)`.
- `:226-230` `SepToString` pool family (`PoolPerCol`/`PoolPerColThreadSafe` for `ParallelEnumerate`).
- `:212-214` reader→writer `writer.NewRow(SepReader.Row)`/`readerRow.CopyTo(writerRow)` — the redaction/column-projection egress bridge (per-column redact before `Col.Set`, `:269-271`).
- STACKING (`:261-264`): `Enumerate(RowFunc<T>) → IEnumerable<T>` is the `LinqToDBForEFTools.BulkCopy<T>`/`BulkCopyAsync<T>` source (streams CSV → PG binary COPY without buffering). version `0.15.0`, MIT.

### api-linq2db-ef (tabular bulk-copy) — `libs/csharp/Rasm.Persistence/.api/api-linq2db-ef.md`
- `:77-78` `LinqToDBForEFTools.BulkCopyAsync<T>(context, BulkCopyOptions/int/—, IEnumerable<T>, ct)` AND over `IAsyncEnumerable<T>` → `Task<BulkCopyRowsCopied>` (`where T:class`); `:69` `BulkCopyRowsCopied` typed receipt.
- `:166` `BulkCopyOptions` `BulkCopyType` (`ProviderSpecific` = PG binary COPY, `MultipleRows` = multi-row-INSERT fallback), `MaxBatchSize`, `KeepIdentity`, `ConflictAction`.
- `:95-96` `GetMetadataReader`/`GetMappingSchema` — reuse the EF model's mapping (no second mapping); `:47` `EFCoreMetadataReader`.
- `:101-103` `CreateLinqToDBConnection(DbContext[, IDbContextTransaction?])`; `:110` `CreateLinqToDBConnectionDetached` (change-tracker-free); `:146` `LinqToDBForEFTools.Initialize()` (once at composition).
- `:163` core `MergeWithOutputAsync<TTarget,TSource,TOutput>(...) → IAsyncEnumerable<TOutput>` (RETURNING-streaming upsert). version `10.4.0` / core `linq2db 6.3.0`, MIT.

### api-miniexcel (tabular spreadsheet — underutilized report surface) — `libs/csharp/Rasm.Persistence/.api/api-miniexcel.md`
- `:78` `DynamicExcelColumn : ExcelColumnAttribute` ctor `(string key)` + `CustomFormatter (Func<object,object>)` — supplied through `Configuration.DynamicColumns` (`:63`). The `TabularWire.Wire` fence builds one but `Policy()` never sets `DynamicColumns` (the phantom).
- `:96-102` `OpenXmlStyleOptions`/`OpenXmlHeaderStyle`/`HorizontalCellAlignment`/`MiniExcelPicture` — the report-egress styling surface (`SaveAsByTemplate` alone exploited today).
- `:181-182` `MergeSameCells`/`AddPicture` (report-shaped egress); `:77` `ColumnType.Formula` (write cell as Excel formula).
- `:63` `OpenXmlConfiguration.DynamicSheets` (multi-sheet runtime binding). version `1.45.0`, Apache-2.0.

## [04]-[FOLDER-CONTEXT ANCHORS]
- `lane.md:40` `StalenessWatermark(HeadSequence, ProjectedSequence, Lag)` — federation's default `TimeCut` = watermark head.
- `lane.md:130,193-203` `SetExpr` (`Literal|Predicate|ByRule|Union|Intersect|Difference|Closure`) + `ElementSet` receipt — federation's Substrait→SetExpr lowering target (`SetRelation`→Union/Intersect/Difference 3-for-3).
- `columnar.md:16,109,284` `ColumnarLane.ArrowStream`(ADBC)/`ColumnarExtension` axis — federation's columnar execution lane (Project/Aggregate/Sort→columnar; `ColumnarExtension.Substrait` = DuckDB `from_substrait`).
- `columnar.md:14,48-64` `ColumnarProfile`/`ColumnarExtension` `Lakehouse` profile (`httpfs`/`iceberg`/`delta`/`postgres`) — the ADBC/warehouse execution surface.
- `cache.md:74` `ModelResultIndex` single recency-horizon owner; federation replay lands as one `ArtifactKind` reuse row (`cache.md:20` growth law).
- Doctrine: `domain/postgres.md:186-202` `Lane` `[SmartEnum<string>]` wire-keyed + `[UseDelegateFromConstructor]` narrowing arrow — the pattern cypher's `RouteMode`/`CleaveKind`/federation's `SourceKind` mirror. `domain/persistence.md:30` model-cache keys on context type + design-time flag (the wide-column provider-row axis composes beneath). `shapes.md:308-353` `[GRAPH_FAMILY]` two closed `[Union]` owners — the neutral-verb graph law cypher's `GraphQuery` composes.
