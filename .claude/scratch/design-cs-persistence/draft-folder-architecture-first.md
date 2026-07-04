# [DRAFT] Rasm.Persistence page-set blueprint — lens: FOLDER-ARCHITECTURE-FIRST

The structure is derived from the folder law FIRST, then capability is fitted into it. The five sub-domain folders are the load-bearing axes; every folder is proven a genuine growth axis with ≥2 pages and a stated "next concern lands as a page" law; the ruled-default 24-page map (Element 4 · Version 8 · Query 7 · Ingest 2 · Store 3) is honored exactly, with zero one-file folders. Every V-verdict, E-row, [03] delta, roster admission, card, seam, and AppHost PORT contract is disposed against that structure. Draft is DECISION-complete: no disposition is left open.

Thesis: the 18 fence interiors survive; the perimeter (bands, ledgers, catalogs, roster, cards) plus a bounded in-fence defect set is rebuilt; and the two one-file/overloaded-folder violations (Ingest at 1, the two 4-concern pages) are resolved by GROWTH, never dilution — Ingest becomes a real codec-ingress axis, identity and lane shed one coherent concern each, and the four missing owners (coordination, egress, federation, retrieval) land as the folders' next axis rows, not as flat sprawl.

---

## [00]-[FOLDER_DERIVATION] — the structure the capability fits into

The lens rule: no one-file folders; every folder a growth axis where the NEXT concern lands as a page; no flat sprawl. Each folder is a coherent durable-spine concern with a closed growth law. The 24-page map is the ruled default; this draft matches it exactly (no departure carries stronger disk proof, so none is taken).

| Folder | Axis (the one thing the folder owns) | Pages now | Pages post | Growth law — the next concern is a page here | One-file risk |
|---|---|---|---|---|---|
| `Element/` | the ElementGraph store-load roundtrip + its content identity + its access tier | 3 | **4** | the next graph-persistence concern (a new content-key domain, a new identity axis, a new authz scope) is a page on `Element/` | resolved — 4 pages |
| `Version/` | the version-control engine PROJECTING from Marten events (history, convergence, provenance, durability, egress) | 7 | **8** | the next event-derived durable concern lands here; stays FLAT at 8 (the core/read/governance sub-partition is the split axis only past ~9) | none |
| `Query/` | the read lanes split by consistency demand + the reuse index | 5 | **7** | the next read lane (a new consistency stance, a new engine surface, a new plan-lowering) is a page on `Query/` | none |
| `Ingest/` | the file-codec ingress axis (rectangular data, schedule files, the next foreign file format) | **1 (VIOLATION)** | **2** | the next foreign-file codec into the record rail is a page on `Ingest/` | RESOLVED — the standing one-file violation ends |
| `Store/` | the durable-home + coordination substrate (object bytes, server tier, cluster coordination) | 2 | **3** | the next durable-home / coordination concern (a new provider axis, the coordination spine) is a page on `Store/` | resolved — 3 pages |

The 24-page map is exact: `Element` 4 (+authority), `Version` 8 (+egress), `Query` 7 (+retrieval, +federation), `Ingest` 2 (+schedule), `Store` 3 (+coordination). Six new owners land as the folders' next axis rows; the two overloaded pages (identity 653 LOC/4 concerns, lane 520 LOC/4 concerns) each shed ONE coherent concern to a sibling page on the SAME folder axis — authz to `Element/authority`, the ANN subsystem to `Query/retrieval` — so growth reduces per-page concern-count without fragmenting the axis.

Folder-law checks that gate the design:
- **No one-file folder**: `Ingest/` grows to `{tabular, schedule}` (the BIM:102 P6/MS-Project relocation supplies the durable-schedule consumer, so `schedule.md` mints; the fold-up alternative is NOT taken because the consumer exists). `Store/` grows to `{blobstore, provisioning, coordination}`.
- **No flat sprawl**: `Version/` stays flat at 8; egress joins beside the changefeed it drains (a `Sync/` revival is refused — it would need a second sync-shaped sibling to clear the folder law, and none exists).
- **Every folder ≥2 concerns of one axis**: no folder holds unrelated concerns; each page is the axis's next row.

---

## [01]-[PAGE_SET] — 24 rows (path · action+lowering · owner charter/home/skeleton/band · entry · seams in⇄out · leg)

`kind` is the rebuild-engine lowering. `absorb {into,from}` carries the extraction. `deletePages` = 0 (the ~30→18 collapse already removed the dead pages; this campaign ADDS and REBUILDS, deletes nothing). Band = the `[V4]` registry row. Seams list one anchor per crossing, both ledger directions.

### `Element/` — 4 pages (the ElementGraph store-load roundtrip + identity + authz)

| # | path | action · lowering | owner charter · home · band | entry signature | seams (in ← / out →) | leg |
|---|---|---|---|---|---|---|
| 1 | `Element/graph.md` | **KEEP** · `kind:improve` | Store-rail root: one-materializer event sourcing (`GraphDelta.ReplayOnto`), co-txn identity commit, one-boundary fault conversion. **HOSTS the `[V4]` `[FAULT_TABLES]` `FaultBand` registry owner block** (the store-rail root every rail composes). Band **Graph 8300**. `Element/` | `GraphStoreOp` (6-case store rail over generated `Switch`) + `FaultBand` `[SmartEnum<int>]` | `← Rasm.Element` (ElementGraph SoR, ARCH:41); `← Rasm.AppHost/Runtime` (ClockPolicy/CorrelationId/TenantContext PORT, ARCH:54); `← graph#STORE_RAIL ← APPHOST:71` (prepared-tx RETIRE counterpart); `← timetravel#TimeCut` (frozen-vocab, V5d); `→ Compute:115` (parse-to-canonical Extract counterpart) | 1 |
| 2 | `Element/codec.md` | **KEEP** · `kind:improve` | Seam-composed `ContentAddress` (kernel `ContentHash.Of` seed-zero, no second hasher), CBOR trust boundary, single-pass atomic seal, FastCDC chunker. **`[V10]` HashPolicy ruling** (shrink to `{Identity,Content}` + state `HashDomain` forward-compat, OR route Seal/Verify through the policy) + kill the `:175` `ByDomainId` prose drift. **THE kernel-hash re-anchor root** (all raw mints compose here). Band **Codec 8310** (831x/832x/833x legal stride). `Element/` | `SnapshotCodec` `[SmartEnum]` axis + `ContentAddress.Of/OfGraph/Of(UInt128)` seam-compose | `← Rasm` (kernel seed-zero XxHash128, ARCH:42 — the ONE hasher entry); `→ typescript:wire` (SnapshotHeader canonical-CBOR, ARCH:43); `← Compute:111` (FastCDC content-key delta counterpart → `#CONTENT_CHUNKER`) | 1 |
| 3 | `Element/identity.md` | **SPLIT** · `kind:rebuild` (absorb-out) | Relational identity tier (one txn owner as a Marten doc), `IdentityPolicy` key axis, **KMS custody (SigningKeyring + EnvelopeKeyring — the `KmsProvider` axis STAYS here per V5a; authz-vs-crypto split, not signing-vs-envelope)**, `SchemaVerdict` boot fold. **`[V6]` EF commit** (`UseValueObjectValueConverter()`+`UseSnakeCaseNamingConvention()`; hand `NodeId` converter + ~13 `HasColumnName` die; LanguageExt-type converters stay). **`[V6]` DDL/migration owner** (`element_identity`/`node_cell` DDL on the schema section; `ServerExtension.CreateSql` commits through this one migration rail; `EF.Design` earns admission). Slims 653→~410 LOC. Band **Identity 8340**. `Element/` | `IdentityPolicy` key axis + `SchemaVerdict` boot fold + `AuthDecision` (KMS custody) | `⇄ Rasm.AppHost/Runtime` (TenantId RLS + KMS SigningKeyring/EnvelopeKeyring unwrap PORT — the ARCH:53 row SPLITS, KMS/RLS stay here; APPHOST:72 unwrap target unchanged) | 1 |
| 4 | `Element/authority.md` | **NEW** · `kind:new` · `absorb {into: Element/authority.md, from: Element/identity.md}` | The object-ACL authz set-algebra extracted from identity `:287-341,465-484`: `Grant`/`GrantSet`/`AclScope`/`AclEntry`/`ObjectAcl`/`Authority.Admit`+`Effective`+`LapsedFor` — pure deny-over-allow set-algebra, zero `KmsProvider`, total (no fault rail). Band **NONE** (total algebra). `Element/` | `Authority` (Admit/Effective/LapsedFor) over `ObjectAcl` — vocabulary page, NONE op-union | `⇄ Rasm.AppHost/Runtime` (ObjectAcl identity-store PORT — the ARCH:53 `ObjectAcl` half moves HERE); consumed by `Version/commits#Movable` (ACL gate on GrantSet) | 1 |

### `Version/` — 8 pages (the engine projecting from Marten events; stays flat at 8)

| # | path | action · lowering | owner charter · home · band | entry signature | seams (in ← / out →) | leg |
|---|---|---|---|---|---|---|
| 5 | `Version/ledger.md` | **KEEP** · `kind:improve` | Changefeed projection off Marten `SubscriptionBase`, HLC `(Hlc,OriginStoreId)` LWW, presence/awareness drain. **`[V10]` dead carriers**: `SyncOpKind.Truncate`+`WholeRelation` wire-or-delete; `OpLogEntry.Codec` collapse to a `Family`-derived accessor; `ProcessEventsAsync` batches the range into ONE fold (`:119`). Band **Sync 8250** (`SyncFault`, the one correct typed band). `Version/` | `SyncTransport` + `ChangefeedSubscription` (op-log projection) + `OpLogEntry` | `⇄ python:runtime/transport` (OpLogEntry CRDT delta, ARCH:48); `⇄ Rasm.AppHost/Runtime` (HLC + TraceSlot PORT, ARCH:49); `← Rasm.AppHost/Wire/companion` (**ADD**: PRESENCE PeerRoster beats over DrainSurface, ledger.md:468); `→ ledger#CHANGEFEED ← BIM:101` (durable-annotation CDE counterpart); `→ ledger#CHANGEFEED ← AppUi Collab/Editing` (edit-intent projection + replay-window read); `← BIM:104` (Speckle Base import counterpart) | 2 |
| 6 | `Version/commits.md` | **REBUILD** · `kind:rebuild` | Content-addressed commit-DAG, six-type CRDT algebra, `CrdtOpWire` `[MessagePack.Union]` flagship. **`[V4]/H1` MINT `CommitFault : Expected`** (8261 decode-drift, 8263 parity-drift, 8264 owner-mints — the THIRD bare-`Error.New` owner the census missed; NOT "register as-is"). **`[V10]` near-linear `MergeBase`** (one reverse-reachability generation-mark pass + dominance filter, killing the per-candidate `Rank` re-run `:108-113`). **`[SEAM]` raw-hash re-anchor** (`CommitNode` key `:91`, `CrdtWire.ContentKey` `:354`, parity corpus `:375-376,401` compose `ContentHash.Of`). **`CRDT_OP_SET` parity fixture PIN** (the `MvRegister`/`opMerge` op-set converging byte-identically under the one kernel seed — turns the kernel DESIGN-PIN `reconciliation.md:129` REAL). **`[V10]` dead carriers**: `VectorOrder`/`Order` wire-or-delete; `ContentParityCorpus.Seed=0L` fold into the mint or delete. Band **Commit 8260**. `Version/` | `CrdtOp`/`CrdtField` algebra + `CommitGraph.MergeBase` (commit-DAG) + `ContentParityCorpus` | `→ typescript:wire` (CrdtOpWire msgpack union, ARCH:44); `⇄ python:runtime/transport` (CrdtOp None-companion + parity corpus, ARCH:45); `→ typescript:state` (commit/branch/VV/Merkle, ARCH:46); `← BIM:92/94` (BimCommit/MergeBase — align spelling to `commits#MergeBase`) | 2 |
| 7 | `Version/timetravel.md` | **KEEP** · `kind:improve` | ONE materializer AS-OF (`GraphDelta.ReplayOnto` via `AggregateStreamAsync`), two-axis RangeDiff, monotone-predicate bisect, non-cryptographic checkpoint chain. **`[V12]` `AsOfKey` arm** (`Checkpoint.Hash` IS the `AsOfKey` — ONE digest serving the icechunk cross-runtime seam AND the recovery content-identity oracle; `recovery.md:65` `RecoveryPoint.AsCut()` resolves to it). **`[SEAM]/H3` edge keys** compose `ContentAddress.Of(edge.ToCanonicalBytes.Span)` (`:203-204`, not raw `XxHash128`). **`[V9]` Scrub/Bisect** re-shape to the incremental `OfGraph(prior,delta)` composition (interim whole-graph recompute documented). **`[V10]` dead carrier** `AggregateStreamToLastKnownAsync` drop-or-compose. Band NONE (routes reconstruction; no Expected fault). `Version/` | `TimeLog` (Reconstruct/Diff/Blame/Scrub/Bisect/Branch) reconstruction ops | `← python:data/gridded/virtual` (icechunk as-of AsOfKey over the kernel seed, ARCH:50 — target LIVE, capability now realized); consumed by `recovery#ReAttest` (AsOfKey verify) | 2 |
| 8 | `Version/merge.md` | **KEEP** · `kind:improve` | Re-ingest `Reconcile` GlobalId alignment before diff, two-axis three-way merge, `always-succeeds-with-annotations` `MergeOutcome`. **`[V8]b` Type-correlation key row** (a classification-independent Type natural key for `ObjectKind.Type` nodes mirroring `ExternalKey`, so a re-keyed Type diffs as RENAME; COUPLED to V8a — waits on the kernel seed change or ships its own classification-excluded seed). **`[V10]` STJ options** (thread `ElementJson.Options` through the `JsonPatchDocument` insert arm `:390`). **`[SEAM]` MemberPath typing VERIFIED CLEAN** (`seg.Value is NodeId`, no degradation — dossier-confirmed). Band NONE (`MergeConflict` is a conflict class, not Expected). `Version/` | `StructuralMerge.Reconcile` + `EditOp`/`EntityEdit` (three-way merge) + `ExternalKey`(+Type key) | `→ typescript:wire` (JsonPatch RFC-6902 egress, ARCH:47); `← Rasm/Spatial/reconciliation` (**RETARGETED** GeometryHash — ARCH:55 moves OFF topology ONTO `Version/merge#STRUCTURAL_DIFF`; `GraphNode.GeometryHash` is the named consumer; the adjacency-Encode-vs-Representations-digest semantics gap surfaced to the geometry `[V8]` counterpart) | 2 |
| 9 | `Version/provenance.md` | **KEEP** · `kind:improve` | Exact W3C-PROV-O typing, independent-`digestOf` attested verify (makes `Unauthored` reachable), Merkle transparency-log proofs — the strongest fence in the lane (federation-wide verify template). **`[V10]` Principal-derived agent class** (read the authority's class off `Principal`, not unconditional `AgentClass.Person` `:257`). **`[SEAM]` `AgentKey` re-anchor** (durable PROV-node identity composes `ContentHash.Of` `:291`; `Bundle`/`Append`/`Pair` internal Merkle digests defensible-local). Band NONE (`AttestVerdict.Unauthored` is a verdict, not Expected). `Version/` | `ProvNode` + `AttestedLedger` (Append/Verify) — causal DAG + attested ledger | `← python:artifacts/provenance` (signed-artifact content-key binding + attested-ledger authority, ARCH:51 — the `[ARTIFACT_CONTENT_KEY_FEDERATION]` card's real home) | 2 |
| 10 | `Version/retention.md` | **KEEP** · `kind:improve` | Six-row `RetentionClass`, fail-closed `RetentionCeiling`, full-history reachability `Mark` over EVERY `TimeCut`, ONE receipted executor. **`[V10]` dead carrier** `StorageLane.Durable` consume-in-a-guard-or-delete `:36`. **`[V5]c` frozen-vocab imports**: `StorageTier` (blobstore-owned, FROZEN) + `TimeCut` (timetravel-owned, FROZEN) consumed as contracts. Band **Retention 8280** (`RetentionFault`, correct typed band). `Version/` | `RetentionClass` `[SmartEnum]` + `SweepVerdict` (holds-first sweep) | `← Rasm.Compute` (content-keyed Assessment.Result blobs in the blob retention class, ARCH:52); `← Store/blobstore#StorageTier` (frozen-vocab, V5c); `← Version/timetravel#TimeCut` (frozen-vocab) | 2 |
| 11 | `Version/recovery.md` | **REBUILD** · `kind:rebuild` | Real `(Timeline,Lsn)` `RecoveryPoint` via Npgsql `IdentifySystem`, six-step ranked restore, `ReAttest` content-identity commit-point. **`[V10]` the two verified RPO bugs**: `ObjectReplica` computes REAL lag (freshest-replicated-blob age vs newest-event instant, killing `Duration.FromMinutes(absent.Count)` `:180`); `PgPitr` WAL-throughput lifts into an explicit policy row (killing the 16-MiB-segment-as-rate `:171`). **`[V12]` content-identity terminal proof**: the restored coordinate's reconstructed `OfGraph` equals the checkpoint `AsOfKey`, `RecoveryFault.VerifyFailed` unreachable on a clean restore. **`[V5]c` frozen import** `ObjectStore.Head` (blobstore-owned, FROZEN). Band **Recovery 8290** (`RecoveryFault`). `Version/` | `RecoveryRoute` `[SmartEnum]` + `RestoreStep` (verified PITR choreography) | `← Rasm.AppHost/Runtime` (RPO/RTO objective inputs PORT, ARCH:64); `← Store/blobstore#ObjectStore.Head` (frozen-vocab, V5c); `← Version/timetravel#AsOfKey` (content-identity oracle) | 2 |
| 12 | `Version/egress.md` | **NEW** · `kind:new` | **`[V3]` CDC egress owner** (ruled default site: beside the changefeed it drains; a `Sync/` revival is refused for lack of a second sync sibling). ONE pump fold draining the `[V2]` outbox cursor past `OpLogEntry` rows into `EgressSink` delivery rows under ONE CloudEvents envelope. **Exactly-once EFFECT**: `id`=`OpLogEntry.ContentKey` (never a minted UUID), `Sequence`=outbox `long Sequence`, `Partitioning.partitionkey`=`EntityKey`. Sink dedup is composition: `Nats` maps `id`→`Nats-Msg-Id` (JetStream dedup) + Settle-ack advances the cursor; `Webhook` (`pg_net`) NEVER advances on post — only on `net.request_status`=SUCCESS reconciliation, PENDING holds, ERROR/timeout dead-letters; the wire-native gRPC/Arrow hop READS AppHost `HopPolicy` delivery-honesty axis. Sink rows: webhook/nats/kafka(+serdes)/rabbitmq/pulsar/wire-native as seed DATA over one pump. Typed dead-letter + replay. Band **Egress 8270**. `Version/` | `EgressPump` (one drain fold) + `EgressSink` `[Union]` + CloudEvents envelope | `← Store/coordination#OUTBOX_CURSOR` (drains the cursor — internal, no reverse edge); `← Version/ledger#CHANGEFEED` (OpLogEntry rows); `→ Rasm.AppHost/Wire/outbox ← APPHOST:73` (keyed OutboundHop egress counterpart); reads AppHost `HopPolicy` delivery-honesty (RASM-CS-APPHOST-BRIEF `[V9]`) | 4 |

### `Query/` — 7 pages (read lanes split by consistency demand + the reuse index)

| # | path | action · lowering | owner charter · home · band | entry signature | seams (in ← / out →) | leg |
|---|---|---|---|---|---|---|
| 13 | `Query/lane.md` | **SPLIT** · `kind:rebuild` (absorb-out) | Drops to routing + set-algebra: `ReadRouter` (sync authoritative vs async analytical), `StalenessWatermark` sequence-gap, `ElementSet`/`SetExpr`/`ElementSetAlgebra.Evaluate`. The `FUSION_AND_CACHE`+`VECTOR_CODEBOOK` ANN subsystem leaves to `retrieval`. Band NONE (set-algebra is total; the codebook faults leave with retrieval). `Query/` | `ReadRouter` + `ElementSet.Evaluate(SetExpr, SetResolve)` | `⇄ python:data/tabular/query` (**SPLIT**: the ElementSet receipt-currency half STAYS here, ARCH:57a); `→ retrieval#VECTOR_CODEBOOK` (absorb-out); seam-honesty: route `WaitForNonStaleProjectionDataAsync` through `IProjectionDaemon`/`IMartenDatabase`, not `TestingExtensions` | 3 |
| 14 | `Query/retrieval.md` | **NEW** · `kind:new` · `absorb {into: Query/retrieval.md, from: Query/lane.md}` | The coupled ANN subsystem extracted from lane `:267-520`: `FUSION_AND_CACHE` (weighted n-ary reciprocal-rank fusion, pgvector SQL-binding axes) + `VECTOR_CODEBOOK` (`ProductCodebook` PQ k-means training, coarse→fine ADC scan). **`[V4]/V5b` MINT `RetrievalFault : Expected`** (kills the lane codebook's bare `Error.New(8360..8363)` — the only un-banded owner in the lane). **`[V6]` pgvector server-side ANN row** (`<->`/`<=>` LINQ distance path as the corpus-scale ANN row beside the in-process PQ/ADC hot-set lane; residency is row data on the one retrieval axis). Band **Retrieval 8410**. `Query/` | `FusionRank.Fuse` + `VectorCodebook.Train`/`AdcScan` (ANN fusion + PQ codebook) | `⇄ Rasm.Compute/Model/embedding` (**ADD**: VECTOR_CODEBOOK — VectorRow.ContentKey ↔ EmbeddingVector.ContentKey, ProductCodebook trained here, Compute imports by project reference; COMPUTE:99 declares, no own row today; post-split targets `retrieval`) | 3 |
| 15 | `Query/topology.md` | **KEEP** · `kind:improve` | In-process QuikGraph view, frozen incidence, the DEFAULT synchronous topology owner. **`[V10]/NEW` `Lca` DAG-gate** (`Lca`/`Ancestor` pre-gate `tree.IsDirectedAcyclicGraph()` and rail `TopologyFault.Cyclic`, symmetric with the already-gated `Order` — `OfflineLeastCommonAncestor` is catalog-flagged unsound over cyclic input, and topology models containment cycles as real). **`[V10]` dead carriers** `TypedEdge.IsContainment`/`Kind` (unread post-build; drop unless an external consumer reads them). Band **Topology 8370** (`TopologyFault` 8370-8371, keeps 837x). `Query/` | `TopologyQuery` (traversal/path/components/topo-sort) | `→ Version/merge` (**ARCH:55 RETARGETED OFF topology** — topology keeps only the `ContentAddress.OfGraph` per-snapshot memo; the reconciliation GeometryHash consumer is merge, not topology) | 3 |
| 16 | `Query/columnar.md` | **KEEP** · `kind:improve` | ONE in-process DuckDB engine, co-transactional `BimOpenSchemaProjection`, ParquetSharp, ADBC. **`[V10]/E14` typed trust gate** (a validated `Identifier`/`StorePath`/`SecretName` value-object family gates `Mount`{store}/{alias} `:255`, `Secret`{name}/{into} `:271`, `Egress`{projection}=raw-SQL-fragment/{destination}/{stamp} `:356-360`, `Generation`{root} `:390`; `{projection}` becomes a composed typed projection, never a raw string). **`[V10]` posture constant** (the four `"80%"`/`"90%"` literals `:80-83` collapse to one). **`[V10]` phantom-spellings**: `AdbcStatement.ExecuteQueryAsync`→confirm-or-`ExecuteQuery`; `BimData.WriteDuckDB`→`frames.ToDataSet().WriteToDuckDB(path)`. **ARCH:56 RE-SCOPE** (the realized fence is Persistence-owned generic `BimOpenSchemaProjection : FlatTableProjection` `:435-448`, NOT Bim-implemented; Bim supplies the typed schema seed only). Band **Columnar 8350** (`ColumnarFault` 8350-8356 — the WHOLE decade, keeps 835x). `Query/` | `ColumnarSession.Open(ColumnarProfile)` + `Mount`/`Query`/`Egress`/`Generation` + `ColumnarExtension` `[SmartEnum]` | `← Rasm.Bim/Model` (BimOpenSchema seed — re-scoped Persistence-implemented, ARCH:56); `⇄ python:data/tabular` (Arrow over ADBC, ARCH:62); `← BIM:81` (**HOME**: GDAL/OGR GeoParquet columnar ingest → `Query/columnar`, the 13th sibling row); `← federation` (tabular-subtree execution lane) | 3 |
| 17 | `Query/cypher.md` | **KEEP** · `kind:improve` | Injection-safe server-side `format('%L')` composition, three-space result discipline (AGE→ElementSet, pgrouting NODE→H3Cell, pgr_bridges→raw long). **`[V4]` `GraphFault`→`CypherFault` RENAME** (resolves the simple-name ×2 collision; graph keeps `GraphFault`). **`[V10]` dead carrier** `AgtypePath.Weight` (always 0.0 — populate or collapse to `Seq<NodeId>`). Band **Cypher 8360** (`CypherFault` 8360-8363, keeps 836x). `Query/` | `GraphQuery` (openCypher/pgrouting) | `← Store/provisioning#ServerExtension.CreateSql` (frozen-vocab install rail, V5c); optional/demoted beneath QuikGraph | 3 |
| 18 | `Query/cache.md` | **KEEP** · `kind:improve` | The compute-result reuse index: `ArtifactKind` blob index + `ModelResultIndex` recency horizon + `BenchmarkRow` claim gate, sync-over-async `IBufferDistributedCache` L2. The `cache.md:20` one-`ArtifactKind`-row growth law is load-bearing for Compute/Fabrication/Materials durable rows. Band NONE (storage leg). `Query/` | `ArtifactKind` `[SmartEnum]` + `CacheL2Store` (IBufferDistributedCache) + `ModelResultIndex` | `← Rasm.Compute` (ArtifactIndexRow/ModelResultIndex/BenchmarkRow INDEX, ARCH:58); `⇄ Rasm.AppHost/Runtime` (**ADD**: L2 partition CACHE_PORT — TenantId RLS + IBufferDistributedCache, APPHOST:69 declares, no own row today) | 3 |
| 19 | `Query/federation.md` | **NEW** · `kind:new` | **`[V1]` REINTRODUCE** (probe FEASIBLE_WITH_GAPS — the retire alternative is NOT triggered; no second engine, a router/lowerer over standing lanes). Substrait plan arrives as protobuf bytes → **`SubstraitDeserializer.Deserialize(Substrait.Protobuf.Plan)` (shipped PUBLIC in `FlowtideDotNet.Substrait`, ~2-line transcription — NOT hand-built)** → managed `Plan` → a `LoweringTarget` `RelationVisitor` (~150-250 LOC) onto `SetExpr` (SetRelation→Union/Intersect/Difference 3-for-3, ReadRelation+Filter→Predicate, VirtualTableRead→Literal, bounded Iteration→Closure, key-semijoin Join→Intersect) OR the columnar/ADBC lane (Project/Aggregate/Sort/TopN/Window → columnar; Write→REJECT; Exchange→DROP). Plan digest = `ContentHash.Of` over received wire bytes (never a local `XxHash128`). Executes against ONE `TimeCut` (lane watermark head), returns the `ElementSet` receipt + Arrow batch; the `(plan-digest·cut·watermark)` triple content-addresses the result; replay is one `ArtifactKind` reuse row. `SourceKind` capability axis (Substrait-native durable-store/signed-artifact/ADBC vs SQL-only staged). Band **Federation 8420**. `Query/` | `FederationPlan.Lower` (Substrait→SetExpr LoweringTarget visitor) + `SourceKind` axis | `← python:data/tabular/query` (**SPLIT from ARCH:57**: the Substrait portable-plan half homes HERE, signature-locked; ElementSet currency stays lane); `→ Query/lane#SetExpr` (lowering target, internal); `→ Query/columnar` (tabular-subtree execution, internal); `← BIM:91` (federation AuditEntry log counterpart) | 3 |

### `Ingest/` — 2 pages (the one-file-folder violation ends; a real codec-ingress axis)

| # | path | action · lowering | owner charter · home · band | entry signature | seams (in ← / out →) | leg |
|---|---|---|---|---|---|---|
| 20 | `Ingest/tabular.md` | **KEEP** · `kind:improve` | One rectangular-data owner: MiniExcel spreadsheet + **`Sep` as EXPLICIT owner law** (a fenced delimited-lane surface, not sibling prose — the `[V11]` charter made real). **`[V10]/NEW` `TabularWire.Wire` disposition** (compose the `DynamicExcelColumn` `CustomFormatter` into `Policy().DynamicColumns` on the dynamic/reader/write legs, OR delete `Wire` and drop the CustomFormatter claim — the STJ `Bind<T>` is the single wired path). **CONDITIONAL `linq2db.EntityFrameworkCore` COMPOSED** (the BulkCopy bulk-load egress leg over the typed `IEnumerable<T>` becomes a fenced surface → KEEP; else leaves). **`[V4]` `TabularFault` RE-BAND off 837x**. Band **Tabular 8390**. `Ingest/` | `TabularSpec` (typed/dynamic/reader/probe/egress) + `Origin` | `→ Rasm.Element` (row-shape only; per-app root maps tabular→ElementGraph, ARCH:61) | 3 |
| 21 | `Ingest/schedule.md` | **NEW** · `kind:new` | **`[V11]` schedule-file codec** (`MPXJ.Net` .mpp/XER/PMXML → the record rail) + the durable schedule rows (the Persistence half of the relocated Bim schedule domain — TASKLOG:47 moved the CPM/4D DOMAIN to `Rasm.Bim`, the file codec + durable store is ingest-shaped). Mints because BIM:102 supplies the durable-schedule consumer (the fold-up alternative is NOT taken). Band **Schedule 8400** (`ScheduleFault`). `Ingest/` | `ScheduleSpec` (MPXJ codec) + the `TaskRelation` durable rows | `← Rasm.Bim/schedule` (**HOME**: P6/MS-Project 4D → `Ingest/schedule`, BIM:102 counterpart; the durable P6 store + external TaskRelation DAG); `→ Rasm.Element` (schedule→element row-shape) | 3 |

### `Store/` — 3 pages (durable-home + coordination substrate)

| # | path | action · lowering | owner charter · home · band | entry signature | seams (in ← / out →) | leg |
|---|---|---|---|---|---|---|
| 22 | `Store/blobstore.md` | **KEEP** · `kind:improve` | Nine-delegate `ObjectLeg` (four providers fill once), write-once 412-noop seal, `RemoteStoreFault` structural `Lift`, write-blob-first + full-history GC. **`[V10]` receipt path** (`MultipartTransfer.Upload`+`BlobTransferReceipt` become THE composed receipt path all callers route through, OR both die). **`[V10]` checksum honesty** (GCS/Minio rows read the SDK-native `Crc64`/`None` stance, NOT a decorative `XxHash128` only `S3Leg.Seal` supplies). **`[V10]` `BlobResidence.Correlation`** thread-from-write-op-or-drop. **`StorageTier` is the FROZEN vocabulary** retention/recovery consume across legs (V5c — blobstore-owned; the `Store/tier` extraction is NOT taken, no leg-4 amend-mid-campaign proof exists). Band **RemoteStore 5400** (`RemoteStoreFault`). `Store/` | `ObjectLeg.For(ObjectClient)` + `ObjectIo`/`store.Put` + `RetentionSweep` + `ObjectClient` `[Union]` (4 providers) | `← Rasm.Compute` (GLB by RepresentationContentHash, content-keyed write-first, ARCH:59); `← Rasm.Bim/Exchange` (IFC/BREP IfcRepHash GLB, ARCH:60); `→ Rasm.Compute GeometrySource` (**ADD explicit**: `Placement`⇄Compute egress row, currently prose only) | 4 |
| 23 | `Store/provisioning.md` | **REBUILD** · `kind:rebuild` | Verification-first six-command `NpgsqlBatch` fold, `FailureRank.Absorb` absence policy, `ExtensionAdmission` install preconditions, `EngineOps` `Handle`-bridge capsule, embedded residency-split ritual. **`[V4]` `ServerFault` RE-BAND off 835x** (Columnar keeps 835x; ServerFault→8380). **`[V4]/E4` the SEVEN loose `Error.New` typed**: `FailureRank` 8371-8375, `Admit` 8379/8380 → `ServerFault` cases in the re-banded decade; **`EmbeddedStore.Refused` 7701/7702 → `EmbeddedFault.Refused`** (the census-missed THIRD bare-`Error.New` cluster, in-banded to Embedded 771x). **`[V6]` embedded charter HONEST** (Marten is PG-only, so `StoreProfile.Embedded` (SQLite) carries NO ElementGraph event-sourcing path — the real charter is relational identity floor + `EngineOps` checkpoint/snapshot/backup + a read-only/offline tier, never the SoR — stated as page law). **`ServerExtension.CreateSql` is the FROZEN install rail** cypher consumes (V5c). Bands **Server 8380** (`ServerFault`, re-banded) + **Embedded 7710** (`EmbeddedFault`, absorbs 7701/7702). `Store/` | `ClusterProvision.Verify/Admit/Reload` + `StoreProfile`/`ServerExtension`×`ExtensionAdmission` + `EngineOps` | `← Rasm.AppHost/Observability` (Npgsql-ONLY HEALTH_PROBE, ARCH:63; `← APPHOST:85` drops Redis/Kafka, the stale side); hosts the `[V13]` embedded/KV-floor + SoR-spine axes | 4 |
| 24 | `Store/coordination.md` | **NEW** · `kind:new` | **`[V2]` the four AppHost PORT contracts** — ONE token-VALIDATING fenced-lease store (Kleppmann fencing: the guarded write REJECTS a token older than the row's lease generation via the SAME PG row-CAS predicate that debits; a stale resumed holder is a typed `LeaseFenced` fault). **Budget debit = fenced predicated compare-and-decrement, PER-UNIT VECTOR** (the one `UPDATE ... WHERE token >= held AND balance_i >= debit_i FOR EVERY requested unit RETURNING` — N unit balances mirroring AppHost's `CostVector`, so no double-spend across a fenced handoff and no client-side multi-statement span). **READ cases beside guarded writes** (`StepStateInFlight` for CrashResume, `ExpiredLeaseScan` for orphan-reclaim — a write-only union strands the flagship). **`ONE_OUTBOX_EGRESS_SPINE` NAMED** (the Marten event stream IS the outbox — same-`IDocumentSession` guarantee holds; the owner mints the durable drain cursor + at-least-once advance; `[V3]` egress consumes it). Composes Marten `FetchForWriting`/`QueueSqlCommand` + Npgsql advisory locks + LISTEN/NOTIFY wake — never a second event store, never a distributed-lock sidecar. All types Persistence-owned; AppHost's PORT adapters decode them (no AppHost type crosses down). Band **Coordination 8430**. `Store/` | `CoordinationOp` `[Union]` (BudgetDebit·StepStateCas·LeaseAcquire/Renew/Release·MembershipUpsert·StepStateInFlight·ExpiredLeaseScan·MembershipView) + `OutboxCursor` spine | `⇄ Rasm.AppHost/Agent/capability` (fenced per-tenant Budget debit PORT, APPHOST:76); `⇄ Rasm.AppHost/Runtime/orchestration` (step-state CAS + InFlight PORT, APPHOST:77); `⇄ Rasm.AppHost/Wire/outbox` (transactional outbox same-tx, APPHOST:78); `⇄ Rasm.AppHost/Wire/Coordination` (CAS+lease+membership, APPHOST:79); `→ Version/egress#OUTBOX_CURSOR` (the pump drains it, no reverse edge) | 4 |

---

## [02]-[BAND_REGISTRY] — `[V4]` the re-partitioned 83xx map (duplicate fails at type-init)

ONE `FaultBand` `[SmartEnum<int>]` sited on `Element/graph.md` `[FAULT_TABLES]` (the store-rail root). Each fault union derives `Code => Band + n`; a duplicate band integer fails at type initialization (mirrors `.archive/…DECISION.md:141-149`). Every loose receipt code is a typed case or a registered sub-band. Per-page band-neighborhood prose dies for the one registry pointer.

### Own bands (domain-ordered; the three collisions + Commit re-partition resolved)

| Band | Union | Owning page | Codes | Disposition |
|---|---|---|---|---|
| 5400 | `RemoteStoreFault` | `Store/blobstore` | 540x | KEEP (disjoint) |
| 7710 | `EmbeddedFault` | `Store/provisioning` | 7711-7714 **+ Refused (7701/7702 absorbed)** | KEEP + absorb the census-missed loose 770x cluster |
| 8250 | `SyncFault` | `Version/ledger` | 8251-8256 | KEEP (the one correct typed band) |
| 8260 | `CommitFault` | `Version/commits` | 8261,8263,8264 | **MINT typed** (H1 — NOT "register as-is"; the bare `Error.New` becomes a `[Union]`) |
| 8270 | `EgressFault` | `Version/egress` | 827x | **NEW** (fills the free decade between Commit and Retention) |
| 8280 | `RetentionFault` | `Version/retention` | 8281-8283 | KEEP (disjoint) |
| 8290 | `RecoveryFault` | `Version/recovery` | 829x | KEEP (disjoint) |
| 8300 | `GraphFault` | `Element/graph` | 8300-8302 | KEEP (keeps the simple name; cypher renames) |
| 8310 | `CodecFault` | `Element/codec` | 8310/8320/8330 | KEEP (legal 831x-833x multi-decade stride; NOT a collision) |
| 8340 | `IdentityFault` | `Element/identity` | 834x | KEEP (disjoint; 8320/8330 are Codec's stride, so 8340 is the next free) |
| 8350 | `ColumnarFault` | `Query/columnar` | 8350-8356 | KEEP 835x (the WHOLE decade, 7 cases) — collision winner |
| 8360 | `CypherFault` | `Query/cypher` | 8360-8363 | KEEP 836x + **RENAME from `GraphFault`** — collision winner + simple-name ×2 resolved |
| 8370 | `TopologyFault` | `Query/topology` | 8370-8371 | KEEP 837x — collision winner |
| 8380 | `ServerFault` | `Store/provisioning` | re-banded 838x | **RE-BAND off 835x** + the loose `FailureRank`/`Admit` codes become typed cases here |
| 8390 | `TabularFault` | `Ingest/tabular` | re-banded 839x | **RE-BAND off 837x** |
| 8400 | `ScheduleFault` | `Ingest/schedule` | 840x | **NEW** |
| 8410 | `RetrievalFault` | `Query/retrieval` | 841x | **MINT** (kills the lane codebook bare `Error.New(8360..8363)` — the only un-banded owner in the query lane) |
| 8420 | `FederationFault` | `Query/federation` | 842x | **NEW** (`SubstraitParseException`-class + unsupported-relation fail-closed) |
| 8430 | `CoordinationFault` | `Store/coordination` | 843x | **NEW** (incl. `LeaseFenced`) |

Pages with NO fault band (total algebras / verdict cases / storage legs): `Element/authority` (deny-over-allow total), `Version/timetravel` (reconstruction), `Version/merge` (`MergeConflict` is a conflict class), `Version/provenance` (`AttestVerdict.Unauthored` is a verdict), `Query/lane` (set-algebra total post-split), `Query/cache` (storage leg).

### PINNED MIRROR rows (foreign neighborhoods — cross-package disjointness is a row, never prose)

| Package | Band(s) | Note |
|---|---|---|
| AppHost | 1xxx / 4100-4810 | mirror per RASM-CS-APPHOST-BRIEF `[V1]` reciprocal pin |
| Compute | 2200-2299 + Remote `WireFault` 4520-4532 | the wire band |
| AppUi | 6xxx | |
| AEC registry | 2300-2799 (Component 2300 · Generation 2350 · **Geometry 2400 = pinned mirror of kernel `GeometryFault` 2400-2449** · Material 2450 · Projection 2470 · Element 2500 · Bim 2600 · Fabrication 2700→**2701-2710**) | mirrors `.archive/…DECISION.md:141-149` |
| Kernel substrate | `Fault.UnsupportedCode` 9104 | the only coded case on `Rasm/.planning/Domain/rails.md` `Fault` |

---

## [03]-[SEAM_LEDGER] — corrected own rows + counterpart obligations

### Own rows (ARCH:41-64) — corrections

| Row | Correction |
|---|---|
| ARCH:50 icechunk AsOfKey | KEEP target (LIVE `Version/timetravel`); **capability now realized** — `AsOfKey`=`Checkpoint.Hash` (V12); the phantom was the member, not the page |
| ARCH:53 identity KMS PORT | **SPLIT** — `ObjectAcl` store → `Element/authority`; TenantId RLS + KMS SigningKeyring/EnvelopeKeyring unwrap → `Element/identity` (APPHOST:72 unchanged) |
| ARCH:55 reconciliation GeometryHash | **RETARGET** `Query/topology` → `Version/merge#STRUCTURAL_DIFF` (topology has zero reconciliation refs; merge's `GraphNode.GeometryHash` is the consumer); the adjacency-`Encode`-vs-Representations-digest semantics gap surfaced to the geometry `[V8]` counterpart (`Rasm/ARCHITECTURE.md:79` re-points to the same home, that campaign's obligation) |
| ARCH:56 columnar BimOpenSchema | **RE-SCOPE** "Bim-implemented" → Persistence-implemented generic `BimOpenSchemaProjection : FlatTableProjection`; Bim supplies the typed schema seed only |
| ARCH:57 lane ElementSet + Substrait | **SPLIT** — ElementSet receipt currency stays `Query/lane`; the Substrait portable-plan half homes on `Query/federation` (V1 reintroduce) |
| ARCH:63 provisioning HEALTH_PROBE | KEEP Npgsql-only; the stale side is APPHOST:85 (drops Redis/Kafka) |

### ADD (wired-undeclared own rows)

- `Query/retrieval ⇄ Rasm.Compute/Model/embedding` [SEAM] VECTOR_CODEBOOK (post-V5b; COMPUTE:99 declares)
- `Query/cache ⇄ Rasm.AppHost/Runtime` [CACHE_PORT] L2 partition IBufferDistributedCache (APPHOST:69 declares)
- `Version/ledger ← Rasm.AppHost/Wire/companion` [PRESENCE] PeerRoster beats over DrainSurface (ledger.md:468; the beat producer is the roster, DrainSurface only the transport)
- `Version/ledger ← Rasm.AppUi/Collab/Editing` [CHANGEFEED] edit-intent projection + per-document replay-window READ (one windowed-read case parameterized by origin/entity/window serves AppUi replay AND the AppHost determinism rehydrate — RASM-CS-APPUI-BRIEF `[V2]` / RASM-CS-APPHOST-BRIEF `[V10]`; the text-container contingency rides the EXISTING `CrdtOpWire` `RgaSequence` — zero new wire row)
- `Store/blobstore ← Rasm.AppUi/Collab/sync` [CONTENT_KEY] snapshot-accelerator rows (content-keyed, derivable-class retention, never SoR)
- The four `Store/coordination` PORT counterparts (APPHOST:76-79 — see page 24)
- `Version/egress → Store/coordination#OUTBOX_CURSOR` (internal cursor edge; no reverse)
- `Query/federation → Query/lane#SetExpr` + `→ Query/columnar` (internal lowering/execution edges)

### The twelve stale sibling → Persistence rows (counterpart obligations; sibling interiors OUT of edit scope)

| Sibling row | Dead target | Corrected target | Campaign |
|---|---|---|---|
| COMPUTE:111 FastCDC content-key delta | `Sync` | `Element/codec#CONTENT_CHUNKER` | Compute |
| COMPUTE:115 parse-to-canonical Extract | `Query/pipeline` | `Element/graph#STORE_RAIL` / `Ingest` | Compute |
| COMPUTE:116 anomaly rule source | `Store/quality` | `Query/columnar` + `Store/provisioning` verification surfaces | Compute |
| COMPUTE:119 Protobuf Kafka topics | `Sync` | retire with the `[V3]`/`[V7]` streaming disposition | Compute |
| BIM:91 federation AuditEntry log | `Query/federation` | `Query/federation` (V1 REINTRODUCED — the owner exists) | Bim |
| BIM:95 IFC validation rules | `Store/quality` | `Query/columnar` + `Store/provisioning` quality surfaces | Bim |
| BIM:101 durable annotation + CDE | `Sync/annotation` | `Version/ledger#CHANGEFEED` | Bim |
| BIM:102 P6/MS-Project 4D | `Sync/schedule` | `Ingest/schedule.md` (V11 NEW) | Bim |
| BIM:104 Speckle Base import | `Sync` | `Version/ledger` (Speckle SyncTransport) | Bim |
| BIM:81 GDAL/OGR GeoParquet (13th, un-enumerated) | `/Store` folder | `Query/columnar` (homed, not retired) | Bim |
| BIM:94 `CommitGraph.MergeBase` (spelling) | — | align to `commits#MergeBase` | Bim |
| APPHOST:71 drain 2PC in-doubt | `Query/transaction` | `Element/graph#STORE_RAIL` — **prepared-tx RETIRE** (the single-`IDocumentSession` spine mints no `pg_prepared_xacts` in-doubt set; RASM-CS-APPHOST-BRIEF `[V11]` the waiting consumer) | AppHost |
| APPHOST:73 keyed OutboundHop egress | `Sync/egress` | `Version/egress.md` (V3 NEW) | AppHost |
| APPHOST:85 health-probe driver roster | Npgsql/Redis/Kafka | `Store/provisioning`, Npgsql-only (matches ARCH:63) | AppHost |

---

## [04]-[ROSTER_DELTA] — INTEGRATION-FIRST; `.api` obligations in the same motion

Central owner `Directory.Packages.props` Persistence block (label-grouped) + `Rasm.Persistence.csproj`. Manifest motion lands in **leg 1** (every ruled prune is zero-page-consumer). License gate: OSS/free-for-OSS-commercial admissible; pay-tiered REJECTED. A leaving row takes its `.api` catalog + README group row + csproj `PackageReference` + AppHost probe row in ONE motion.

### PRUNE (each on a per-package charter/redundancy proof — never a bare zero-consumer default)

| Package | Proof |
|---|---|
| `PollinationSDK` | charter misplacement — a domain cloud SDK with no store-class concern (re-admission belongs to whichever package names a consumer) |
| `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`, `Npgsql.NetTopologySuite`, `NetTopologySuite.IO.GeoJSON4STJ`, `NetTopologySuite.IO.GeoPackage` | no persisted NTS geometry column exists; H3 derivation rides `pocketken.H3` on the transitive core-NTS crossing |
| `Microsoft.EntityFrameworkCore.Sqlite` | the embedded floor is raw ADO by design (`provisioning.md:369-434`) |
| `StackExchange.Redis` (direct) | the L2 swap row keeps `Microsoft.Extensions.Caching.StackExchangeRedis` (an `IBufferDistributedCache`), which transits the driver |
| `ScyllaDBCSharpDriver` | **the single scale-out prune**: `[V13]` wide-column "only with a named owning axis — absent one, the redundancy proof rules"; the store perimeter's closed axes carry NO wide-column owner and no consumer names one (SoR spine + columnar + vector + embedded/KV + object + egress cover every store class) |

### KEEP as `[V13]` axis rows (INTEGRATION-FIRST — realized on the owning axis, not pruned)

The `[V13]` STORE_AXIS_MAP is first-class content; every store-class concern names its axis, its provider rows (deployment/policy DATA), and its selection policy. Each provider row carries its provisioning/health/recovery posture through the verification-first fold, so a row is operable, not merely linkable.

| Axis | Owning page | Provider rows (seed DATA) | Selection | Scale-out admission |
|---|---|---|---|---|
| Object store | `Store/blobstore` | s3 · azure-blob · gcs · minio | `ObjectStore` `[SmartEnum]` | (landed) |
| Egress sink | `Version/egress` | webhook · nats · kafka · rabbitmq · pulsar · wire-native | `EgressSink` `[Union]` | `[V3]` |
| Read-lane/analytic engine | `Query/columnar` | duckdb-in-process · pg_duckdb-in-PG · **clickhouse-scaleout** | `ColumnarEngine` axis | **`ClickHouse.Driver` KEEP** — the scale-out billion-row lane where in-PG TimescaleDB provably cannot own the scale |
| Lakehouse interchange | `Query/columnar` | ducklake(extension, recorded forward) · **delta** | format row | **`DeltaLake.Net` KEEP** — the Delta wire where the columnar plane demands external-warehouse interop |
| Vector search | `Query/retrieval` | pgvector-in-PG · pgvectorscale-diskann · pq-adc-in-process · **qdrant-scaleout** | `VectorBackend` axis | **`Qdrant.Client` KEEP** — the billion-scale ANN row over a proven in-PG ceiling |
| Embedded/KV floor | `Store/provisioning` | sqlite(raw-ADO) · **rocksdb-lsm** · **lmdb** | `EngineOps`-tier row | **`rocksdb`+`LightningDB` KEEP** — write-optimized LSM + read-optimized MVCC EngineOps-tier rows over a proven SQLite-floor ceiling |
| Relational SoR spine | `Store/provisioning` + `Element/graph` | postgres-18 (**SINGULAR**) | SEALED | the ONE event store · ONE materializer · ONE identity · ONE changefeed — unchallengeable; `ARCHITECTURE.md:115` re-scopes to exactly this spine boundary |

### `[V3]` streaming/messaging sink rows (INTEGRATION-FIRST — each leaves ONLY on a per-package redundancy proof)

KEEP-COMMIT the spine anchors: `NATS.Net`, `CloudNative.CloudEvents`(+`.SystemTextJson`). KEEP as sink ROWS (default): `Confluent.Kafka` + `Confluent.SchemaRegistry`(+`.Serdes.Avro`/`.Json`/`.Protobuf`) with `Chr.Avro`(+`.Binary`+`.Confluent`) as the row's codec column, `CloudNative.CloudEvents.Kafka`, `RabbitMQ.Client`, `DotPulsar`. `pg_net` (extension row) — the server-side webhook sink. A broker row leaves only on proven redundancy against an admitted sink of the same delivery semantics or feed-verified abandonment.

### `[V1]` federation rows (KEEP — probe FEASIBLE)

`FlowtideDotNet.Substrait@0.15.0` (the shipped-public `SubstraitDeserializer.Deserialize` is the transcription), `Apache.Arrow.Adbc@0.23.0` + `.Drivers.{Apache,BigQuery}` (source rows), `Apache.Arrow.Flight@23.0.0` (bulk-result hop). **ADD** a DuckDB `substrait` community-extension row (`ColumnarExtension.Substrait`, an extension row, NOT a NuGet — the in-process tabular execution lane via `from_substrait(blob)`; verify a build exists for the pinned DuckDB 1.5.x runtime; fail-closed on unsupported relations). **DROP** the ruled-default `Grpc.Tools` codegen assumption — unnecessary AND counterproductive (regenerating the substrait `.proto` mints a duplicate CLR `Plan` the shipped deserializer rejects by identity); `Google.Protobuf@3.35.1` (already admitted) is the only runtime dependency.

### `[V6]`/`[V11]`/CONDITIONAL/observability

- COMMIT (`[V6]`): `EFCore.NamingConventions`, `Microsoft.EntityFrameworkCore.Design` (earns admission at the identity DDL/migration owner), `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`.
- `MPXJ.Net` (`[V11]`): KEEP — `Ingest/schedule.md` mints (BIM:102 supplies the consumer).
- `linq2db.EntityFrameworkCore` (CONDITIONAL): COMPOSE the BulkCopy bulk-load egress leg in `Ingest/tabular` → KEEP (else leaves).
- `Npgsql.OpenTelemetry` (ruled default KEEP): earns the provisioning observability row (tracing+metrics builder subscriptions at the AppHost composition root, as the health rows are).
- `Ara3D.BimOpenSchema`(+`.IO`): KEEP on the live columnar consumer + **BUMP 1.0.1 → 1.6.1** (feed truth: 1.6.1 published 2026-07-03; the DEBUG-IL caveat is pinned to the superseded 1.0.1 — re-decompile on the bump: retire the caveat if `AssemblyConfiguration("Debug")` is gone, else escalate to in-corpus absorption).

### `.api` catalog obligations (leg-1 motion)

- Re-anchor all 77 package catalogs to LIVE pages (the dead-page map: `Store/{profiles,remote,encryption,quality,server,tenancy}`, `Query/{federation,pipeline,transaction,rail,lanes}`, `Version/snapshots`, `Sync/*`, `Schema/*` → the live re-anchor table). Core spine catalogs `api-marten`→graph/provisioning, `api-npgsql`→provisioning, `api-objectstore`→blobstore are among the **21 anchor-less** (not 16 — disk undercount corrected) that gain anchors.
- Npgsql CATALOG GAP: add advisory-lock (`pg_advisory_xact_lock`) + LISTEN/NOTIFY rows (V2 fenced lease + V3 pump wake); the already-catalogued `Npgsql.Replication` pgoutput stays recorded-unconsumed (the changefeed is Marten's daemon; logical replication the named escalation).
- `CompleteMultipartUploadRequest.ChecksumXXHASH128`: confirm via `assay api` (AWSSDK.S3 registered as a source) or nuget MCP before the blobstore leg closes; add the catalog row (the two enum members `XXHASH128`/`FULL_OBJECT` are already verified).
- `api-flowtide-substrait.md:151` CORRECT: the protobuf round-trip IS public via `SubstraitDeserializer.Deserialize(Substrait.Protobuf.Plan)`/`Deserialize(string json)`/`DeserializeFromJson`; `Substrait.Protobuf.Plan`(+`MessageParser Parser`) is public in-assembly; `SubstraitSerializer` is internal (retain wire bytes, never re-serialize). Re-anchor to `Query/federation`.
- Delete the misfiled shared `api-messagepack.md` package-copy (byte-identical dup, `shasum 5527490f`); re-scope its `RASM_PERSISTENCE` tag → neutral `RASM_API` (matching the landed `RASM_API_LANGUAGEEXT` precedent) or delete.
- The 9 double-cataloged overlays (`hashing, highperformance, hybrid-cache, jsonpatch, messagepack, nodatime-stj, nodatime, redaction, thinktecture-json`): the shared tier OWNS the substrate package; UNION the 8 divergent contents into the shared owner FIRST, then collapse the package copy to a one-line pointer (never blind-delete — content diverges).
- `api-h3.md:3`: correct the NTS-IO bridge claim → the transitive core-`NetTopologySuite` 2.6.0 crossing (`pocketken.H3` survives the prune).
- Uniform `<!-- catalog:Pkg@ver -->` provenance tag + `[STACKING]` section across the kept tier (currently 4/77 tagged, 0/31 shared — a 73/77 + 31/31 open obligation, the version-echo the DECISION reconciles against on every roster motion).
- `api-languageext.md` obligation DISCHARGED (already landed, substantive) — later campaigns verify, never re-author.
- Admissions carry `.api` stubs in the same motion (the DuckDB substrait extension row overlays `api-duckdb`).

### csproj dependency-reversal (PLACEMENT_LAW / strata-acyclicity gate)

`Rasm.Persistence.csproj:10` carries a `Rasm.AppHost` `ProjectReference` — a DOWN/PEER edge that REVERSES the strata (AppHost is a PORT peer Persistence contributes rows to). **RULING: REMOVE it.** The `[V2]` coordination op-union, fencing tokens, membership rows, budget vector, and receipts are Persistence-OWNED types AppHost's PORT adapters decode (no AppHost type crosses down); the `ClockPolicy`/`CorrelationId`/`TenantContext` PORT ingredients cross as Persistence-owned port-input types AppHost's composition root SUPPLIES (injection, not a reference). The correct edge inverts to AppHost → Persistence (an APP consumer of an APP-PLATFORM store). This clears the strata-acyclicity gate.

---

## [05]-[VERDICT_DISPOSITION] — V1-V13 (every ruling resolved, never hedged)

| V | Ruling |
|---|---|
| **V1** FEDERATION | **REINTRODUCE** `Query/federation.md` (probe FEASIBLE_WITH_GAPS; retire NOT triggered — no second engine, a router/lowerer over standing lanes). Wire = Substrait protobuf bytes → shipped-public `SubstraitDeserializer.Deserialize` (~2 lines) → `LoweringTarget` visitor (~150-250 LOC) onto `SetExpr`\|columnar. Plan digest = `ContentHash.Of` over wire bytes. `SourceKind` capability axis (Substrait-native vs SQL-only staged). DROP `Grpc.Tools`; ADD DuckDB substrait extension row. Cards honest-BLOCKED. |
| **V2** COORDINATION | **NEW** `Store/coordination.md` — token-VALIDATING fenced-lease store; per-unit-vector Budget compare-and-decrement; `CoordinationOp` `[Union]` with READ cases (InFlight, ExpiredLeaseScan); NAMED `ONE_OUTBOX_EGRESS_SPINE` (Marten stream + drain cursor). All four PORT rows homed. Band 8430. |
| **V3** EGRESS | **NEW** `Version/egress.md` — one pump; exactly-once-EFFECT envelope (`id`=`ContentKey`, `Sequence`=cursor, `partitionkey`=`EntityKey`); NATS `Nats-Msg-Id` dedup + Settle-ack advance; pg_net response-reconciled advance (never fire-and-forget); wire-native reads AppHost `HopPolicy` honesty. Sink rows + typed dead-letter + replay. Band 8270. |
| **V4** FAULT REGISTRY | **MINT** `FaultBand` `[SmartEnum<int>]` on `Element/graph` `[FAULT_TABLES]`; re-partition (Columnar 835x, Cypher 836x, Topology 837x keep; ServerFault→838x, Tabular→839x, Retrieval→841x); **CommitFault MINT** (H1 — not register-as-is); `GraphFault`→`CypherFault`; PINNED MIRROR foreign rows. |
| **V5** SPLITS+CYCLES | (a) `identity`→`authority` (authz split, KMS stays identity); (b) `lane`→`retrieval` (ANN split); (c/d) FROZEN-VOCAB contracts: `StorageTier` (blobstore, NOT extracted to `Store/tier` — no amend-mid-campaign proof), `ObjectStore.Head`, `ServerExtension`, `TimeCut` frozen; a leg-4 change reopens the consuming leg as a hard residual. |
| **V6** EF COMMIT | COMMIT `UseValueObjectValueConverter()`+`UseSnakeCaseNamingConvention()`; hand converter + ~13 `HasColumnName` die; LanguageExt-type converters stay; DDL/migration owner on identity's schema section; `EF.Design` admitted there; embedded charter HONEST (no ElementGraph event-sourcing path — relational floor + EngineOps + read-only tier); pgvector LINQ → server-side ANN row on `retrieval`. |
| **V7** SCALE-OUT+PRUNE | 5 scale-out KEEP as `[V13]` axis rows (ClickHouse/DeltaLake/Qdrant/rocksdb/LightningDB); **ScyllaDB PRUNE** (no wide-column axis owner); PollinationSDK/4 NTS bridges/EF-Sqlite/direct-Redis PRUNE; catalog motion in leg 1. |
| **V8** TYPE-REKEY | (a) consumer contract recorded (exclude `Classifications` from the Type seed — Element/Materials campaign owns; Persistence demanding consumer); (b) `merge` Type-correlation key row NOW (classification-independent Type natural key mirroring `ExternalKey` → RENAME), COUPLED to V8a (waits on the kernel seed OR ships its own classification-excluded seed); migration case named onto `IdentityPolicy` expand/flip/contract. |
| **V9** INCREMENTAL OFGRAPH | consumer contract recorded (`OfGraph(prior,delta)` is `Rasm.Element/Projection/address`'s; Scrub/Bisect re-shape to the incremental fold; whole-graph recompute documented interim). Geometry `[V2]` parametric-digest-INTO-`ToCanonicalBytes` waterfalled upstream (Persistence named demanding consumer; else `OfGraph` is blind to a parametric-body edit and the `AsOfKey`/recovery proofs assert on an incomplete digest — a false green). |
| **V10** VERIFIED DEFECTS | all repairs land with the owning page's rebuild (per-page table above): recovery RPO lag + WAL policy row; near-linear MergeBase; ledger Truncate/Codec/batch; merge STJ options; blobstore receipt-path + checksum honesty + Correlation; retention StorageLane.Durable; provenance Principal agent class; columnar trust gates + posture constant + phantom-spellings; codec HashPolicy; timetravel dead carrier; **PLUS dossier extras**: commits VectorOrder/Order + ContentParityCorpus.Seed; cypher AgtypePath.Weight; topology TypedEdge.IsContainment/Kind + **Lca DAG-gate**; tabular TabularWire.Wire. |
| **V11** INGEST GROWTH | `tabular` KEEP (Sep explicit owner law; linq2db BulkCopy composed); **NEW `schedule.md`** (MPXJ + durable schedule rows; BIM:102 counterpart supplies the consumer — the fold-up alternative NOT taken). Folder law satisfied (2 pages). |
| **V12** GOVERNANCE | all 30 card dispositions (§09); own-ledger corrections (§03); catalog/manifest close; prepared-tx RETIRE (APPHOST:71→graph#STORE_RAIL); the Fabrication forward constraint (durable receipts decode 2701-2710, never 25xx). |
| **V13** STORE AXIS | the STORE_AXIS_MAP (§04) first-class; SoR spine SINGULAR (one event store/materializer/identity/changefeed — unchallengeable seal); `ARCHITECTURE.md:115` re-scopes to that spine boundary (a perimeter-axis engine row reaching capability the in-PG/in-process owner cannot is a legal DECISION-level admission, never "a new relational engine row"). |

---

## [06]-[EVIDENCE_DISPOSITION] — E1-E14 (re-verified on disk; drifts corrected)

| E | Disposition | Corrections carried from the dossiers |
|---|---|---|
| E1 phantom realization | V1/V12 | HOLD; +un-enumerated IDEAS:57-61 (5 CLOSED cite deleted pages), TASKLOG:46-47, TASKLOG:24-vs-ARCH:51 contradiction |
| E2 orphaned roster | V7/V13 | DRIFT — PROPS anchors uniformly -2; Ara3D "1.0.1 latest" REFUTED (1.6.1); DEBUG-IL at `api-ara3d…:17` not `columnar.md:411`; ~30/90 zero-consumer confirmed |
| E3 catalog drift | V7/ROSTER | DRIFT — anchor-less is **21 not 16**; messagepack byte-identical dup confirmed; every dead anchor confirmed exact |
| E4 band collisions | V4 | DRIFT — Columnar is **8350-8356** (whole decade); 837x is a **THREE-way** collision; graph anchor `:154` not `:155`; **+commits H1 (826x bare `Error.New`, a THIRD un-banded owner)**; +provisioning 7701/7702 (a FOURTH loose cluster) |
| E5 coordination gap | V2 | HOLD — bilateral, fully specified AppHost-side (`orchestration.md:257-282` StepStateSeam; `capability.md:62` CostVector) |
| E6 seam-ledger drift | V12 | HOLD — one internal target DRIFT (ARCH:55 → Version/merge); all 12 sibling rows HOLD; +C1 GeometryHash digest-semantics gap |
| E7 EF stack | V6 | HOLD — identity `:82`/`:120` mapping conflict; MigrationBuilder=0 (no DDL owner); embedded no ElementGraph path |
| E8 verified logic bugs | V10 | DRIFT anchors — recovery `:180`/`:171` (not `:181`/`:172`), MeetsRpo `:145`/`:150`; MergeBase `:108-114` HOLD; MemberPath VERIFIED CLEAN (no degradation) |
| E9 dead carriers | V10 | HOLD — +VectorOrder/Order + ContentParityCorpus.Seed (commits, register-missed); +AgtypePath.Weight, TypedEdge.Kind, TabularWire.Wire (query lane) |
| E10 type-rekey | V8 | HOLD — sharper at `element.md:295` (`WriteObject` writes `Classifications` UNCONDITIONALLY → Type seed IS classification-dependent today; V8a↔V8b coupling) |
| E11 OfGraph hot paths | V9 | HOLD — Scrub 2×/frame, Bisect per probe; the Of(Node,Tol)-vs-span "overload split" REFUTED as a defect (both real, distinct id-inclusive/exclusive semantics); the genuine issue is H3 (timetravel EDGE keys raw) |
| E12 mandate clean | V12 | HOLD — count-prefix ZERO-structural; re-band gate; Fabrication 2701-2710 forward constraint on IDEAS:26-32 |
| E13 folder+page overload | V5/V11 | DRIFT — Ingest one-file CONFIRMED; lane is **520** LOC (retrieval span `:267-520`); Sep prose at `:5,18,20,297` |
| E14 parameterization | V10 | DRIFT — columnar surface WIDER than register (`{projection}` raw SQL fragment, `{stamp}`, `:271` mis-labeled bound); posture literals `:80-83` confirmed |

---

## [07]-[ESCALATION_DELTA] — [03] plane targets (the Phase-2 acceptance bar)

| Plane | Now→Target | Carried by |
|---|---|---|
| Element (graph/codec/identity+authority) | 9→9.5 | V5a, V6, V9, V10, V4-siting, SEAM hash re-anchor |
| Version core (ledger/commits/timetravel) | 9→9.5 | V10 (MergeBase, dead vocab, batch), V12 (AsOfKey), SEAM (CRDT_OP_SET pin), V9 (Scrub) |
| Version merge | 8.5→9.5 | V8 (Type-correlation), SEAM (MemberPath), V10 (STJ) |
| Version governance (provenance/retention/recovery) | 9→9.5 | V10 (RPO lag, WAL policy, Principal class, StorageLane) |
| Query read lanes (lane/topology/columnar/cypher/cache) | 8.5→9.5 | V5b, V4 (CypherFault), V10 (trust gates, Lca gate), V6 (pgvector) |
| Query retrieval (split) | —→9.5 | V5b (fusion+PQ+RetrievalFault), V12 (codebook seam declared) |
| Query federation (new) | 0→9 | V1 (Substrait→SetExpr, wire ingress, source rows, cut-pinned receipt, signature-lock, honest cards) |
| Ingest (tabular+schedule) | 6.5→9 | V11 (folder law, Sep explicit, MPXJ codec + durable rows) |
| Store (blobstore/provisioning) | 9→9.5 | V10 (receipt path, checksum honesty, loose codes), README bridge row |
| Store coordination (new) | 0→9.5 | V2 (token-VALIDATING lease, per-unit debit, CAS/membership, outbox spine, four PORT rows) |
| Changefeed egress (new) | 0→9 | V3 (one pump, exactly-once envelope, sink dedup, dead-letter, replay) |
| Governance perimeter | 3→9.5 | V12 (zero phantoms, ledger complete, 77 catalogs live, zero orphans, band registry type-enforced) |

---

## [08]-[FEDERATION_PROBE_DISPOSITION] — the probe verdict is honored (a draft assuming otherwise loses at judge)

Probe = **FEASIBLE_WITH_GAPS → REINTRODUCE**. This draft rules V1 REINTRODUCE, matching the probe. The probe overturns the ruled-default WIRE-FORM premise in the owner's favor and this draft carries every correction:
- **Wire transcription is shipped-public, ~2 lines** (`SubstraitDeserializer.Deserialize` + `Substrait.Protobuf.Plan.Parser.ParseFrom`), NOT a hand-built boundary transcriber. `api-flowtide-substrait.md:151` FALSIFIED → corrected (§04 catalog obligation).
- **`Grpc.Tools` codegen DROPPED** — unnecessary (types ship compiled) AND counterproductive (a regenerated `Plan` is rejected by identity). Only `Google.Protobuf@3.35.1` (already admitted) is used.
- **The transcription-heavier-than-lowering retire trigger runs OPPOSITE** — transcription ≈2 lines, lowering ≈150-250 LOC (the reference `SubstraitToDifferentialComputeVisitor` is 139 LOC / 9 kinds). Retire NOT triggered.
- **One-engine test PASSES** — a total lowering onto `SetExpr` alone is correctly impossible (SetExpr is a key-set algebra), but the two-target partition (SetExpr for key-selection; existing DuckDB + ADBC for tabular) mints NO new engine.
- **G3 tabular execution** closed as ONE `ColumnarExtension.Substrait` row (in-process DuckDB `from_substrait`, MIT, DuckDB-version-coupled — verify against pinned 1.5.x, fail-closed on unsupported rels) + a `SourceKind` capability axis (Substrait-native committed; SQL-only warehouses staged through the existing `AdbcStatement.SqlQuery` path — the admitted BigQuery Beta driver does NOT guarantee Substrait ingress).
- **G5 external-source honesty** — the pinned `TimeCut` is the LOCAL coordinate; external data currency is read-time, recorded as such on the receipt.
- Roster: KEEP FlowtideDotNet.Substrait + ADBC(Apache/BigQuery) + Arrow.Flight (ride V1); ADD the DuckDB substrait extension row. The three BLOCKED cards re-anchor honest (SetExpr subset zero-gap fences; tabular subset scoped; BLOCKED on the python:data producer).

---

## [09]-[CARD_DISPOSITION] — all 30 cards (every phantom corrected, every stale-COMPLETE re-pointed)

### IDEAS (11)

| Card | Status | Disposition |
|---|---|---|
| :19 `[PERSISTENCE_LIBRARY_TABLES]` | QUEUED | KEEP QUEUED; re-anchor off Materials-side anchors onto the real durable owners (blobstore content rows + cache `ArtifactKind` artifact index + the Store growth axis); lands when the Materials catalogue owners pin content keys |
| :26 `[FABRICATION_PROGRAM_DURABLE_ROWS]` | QUEUED | KEEP QUEUED; **re-anchor off dead `Schema/ddl#IDENTITY`** onto blobstore content rows + cache artifact index + the `[V2]`/Store growth axis; KIND-AGNOSTIC `ArtifactKind` rows (programs/.cli/3MF/NC1/stock-state/travelers) under one `ContentHash.Of` fold; forward constraint: durable receipts decode **2701-2710**, never 25xx |
| :34 `[REUSE_WIRE]` | BLOCKED | KEEP BLOCKED (python:data-gated); **correct the phantom** ("FederatedEntity/EntityGraph is realized" → honest: the SetExpr/ElementSet substrate is realized as fences on `Query/federation`; the plan half BLOCKED on the producer); re-anchor `Query/federation` (V1) |
| :41 `[SUBSTRAIT_FEDERATION_SEAM]` | BLOCKED | KEEP BLOCKED; **correct** ("FederatedPlan is realized" → honest: the SetExpr lowering target realized as fences; the Substrait decode BLOCKED on python:data); re-anchor `Query/federation` (V1) |
| :55 `[PERSISTENCE_BIM_SYNC_CRDT]` | COMPLETE | LEGIT — KEEP CLOSED (`commits#CRDT_ALGEBRA` real); align the BIM:94 `CommitGraph.MergeBase` spelling → `commits#MergeBase` |
| :56 `[DURABILITY_RECOVERY_OBSERVATORY]` | COMPLETE | LEGIT — KEEP CLOSED; the V10 RPO repairs + V12 AsOfKey verify strengthen the "re-establishes CONTENT IDENTITY" claim into an assertable proof |
| :57 `TRANSACTION_CONCURRENCY_CONTROL` | COMPLETE (STALE) | RE-POINT off deleted `Query/transaction.md` → `Element/graph#STORE_RAIL` (fold); prepared-tx RETIRE |
| :58 `ENVELOPE_ENCRYPTION_KMS` | COMPLETE (STALE) | RE-POINT off deleted `Store/encryption.md` → `Element/identity` (KMS custody, post-V5a) + `Store/blobstore#BLOB_GC` (object-SSE) |
| :59 `DATA_QUALITY_INTEGRITY_FRAMEWORK` | COMPLETE (STALE) | RE-POINT off deleted `Store/quality.md` → `Query/columnar` + `Store/provisioning` verification surfaces |
| :60 `BULK_ETL_INTERCHANGE_PIPELINE` | COMPLETE (STALE) | RE-POINT off deleted `Query/pipeline.md` → `Query/columnar` (bulk ingest/egress over ArrowChunk) |
| :61 `CDC_STREAMING_EGRESS` | COMPLETE (STALE) | RE-POINT off deleted `Sync/egress.md` → `Version/egress.md` (V3 NEW owner; the concern is re-owned + re-realized in leg 4) |

### TASKLOG (19)

| Card | Status | Disposition |
|---|---|---|
| :20 `[ARTIFACT_CONTENT_KEY_FEDERATION]` | BLOCKED | KEEP BLOCKED (python:artifacts-gated); **correct the phantom** (`Query/federation#ENTITY_GRAPH SourceKind.SignedArtifact is realized`); re-anchor to `Version/provenance#ATTESTED_LEDGER` (the signed-artifact binding authority, ARCH:51 — the live home TASKLOG:24 contradicted) + the V1 `Query/federation` `SourceKind.SignedArtifact` source row |
| :34 `[PERSISTENCE_BIM_ARTIFACT_INDEX]` | COMPLETE | LEGIT — KEEP CLOSED (`cache#ARTIFACT_BLOB_INDEX` real) |
| :35 `[RECOVERY_PAGE_AUTHOR]` | COMPLETE | LEGIT — KEEP CLOSED (recovery real; V10 RPO repairs land with the rebuild) |
| :36 `[RECOVERY_OBJECTIVE_COLLAPSE_RECOVERYFACT]` | COMPLETE | LEGIT — KEEP CLOSED |
| :37 `[TRANSACTION_PAGE_AUTHOR]` | COMPLETE (STALE) | RE-POINT off deleted `Query/transaction.md`/`.cs` → record the fold into `graph#STORE_RAIL` (TxnScope/IsolationPolicy/2PC through `GraphStoreOp`); prepared-tx RETIRE |
| :38 `[CONFLICTOUTCOME_DISCRIMINANT_COLLAPSE]` | COMPLETE | LEGIT — KEEP CLOSED (`ledger#MERGE_LAW ConflictResult` real) |
| :39 `[ENCRYPTION_PAGE_AUTHOR]` | COMPLETE | LEGIT — KEEP CLOSED; correct the anchor: KMS folds into `Element/identity` (the KmsProvider axis stays identity per V5a), object-SSE into `Store/blobstore` |
| :40 `[SQLCIPHER_RESEARCH_PROMOTE]` | DROPPED | LEGIT DROPPED — KEEP |
| :41 `[QUALITY_PAGE_AUTHOR]` | COMPLETE (STALE) | RE-POINT off deleted `Store/quality.md`/`.cs` → `Query/columnar` + `Store/provisioning` verification surfaces |
| :42 `[PIPELINE_PAGE_AUTHOR]` | COMPLETE (STALE) | RE-POINT off deleted `Query/pipeline.md`/`.cs` → `Query/columnar` |
| :43 `[EGRESS_PAGE_AUTHOR]` | COMPLETE (STALE) | RE-POINT off deleted `Sync/egress.md`/`.cs` → `Version/egress.md` (V3); the COMPLETE was FALSE (owner deleted) → flip to the V3 re-authoring pointer (re-realized leg 4) |
| :44 `[SCHEMADDL_SQL_COLLAPSE]` | COMPLETE (STALE + self-contradicting) | RE-POINT off `SchemaDdl.Sql` (absent, contradicts :45) → the DDL is `ServerExtension.CreateSql` + `ClusterProvision.Admit` (provisioning) + the V6 identity migration owner; NO `SchemaDdl.Sql` owner |
| :45 `[STORE_SERVER_SPLIT]` | COMPLETE | LEGIT — KEEP CLOSED (the truth-source contradicting :44; provisioning + tenancy-into-identity) |
| :46 `[ANNOTATION_RELOCATE_TO_BIM]` | COMPLETE (STALE) | RE-POINT off deleted `Sync/annotation.md` + phantom `Query/federation#ENTITY_GRAPH` join key → `Version/ledger#CHANGEFEED` (BIM:101); join on real GlobalId/ExternalId |
| :47 `[SCHEDULE_RELOCATE_TO_BIM]` | COMPLETE (STALE) | RE-POINT off deleted `Sync/schedule.md` → `Ingest/schedule.md` (V11 NEW; MPXJ codec + durable rows); 4D/CPM to Rasm.Bim/schedule (BIM:102) |
| :48 `[ICECHUNK_ASOF_CONTENT_KEY]` | COMPLETE (STALE + phantom) | RE-POINT off deleted `Version/snapshots#SNAPSHOT_PROTOCOL AsOfKey` → `Version/timetravel` (`AsOfKey`=`Checkpoint.Hash`, V12; re-realized leg 2) |
| :49 `[KMS_PACKAGE_ADMISSION]` | COMPLETE | LEGIT — KEEP CLOSED |
| :50 `[ARROW_FLIGHT_PACKAGE_ADMISSION]` | COMPLETE | LEGIT — KEEP CLOSED; correct the ripple ("rides BULK_ETL_INTERCHANGE_PIPELINE", deleted) → rides V3 `Version/egress` + V1 `Query/federation` |
| :51 `[KAFKA_CLOUDEVENTS_PACKAGE_ADMISSION]` | COMPLETE | LEGIT — KEEP CLOSED; the Kafka sink rides the V3 sink rows |

Card totals: 30 total · 5 open (3 BLOCKED + 2 QUEUED, corrected + held) · 25 closed (9 LEGIT-KEEP + 1 LEGIT-DROPPED + **15 stale re-pointed**: IDEAS:57-61, TASKLOG:37,41,42,43,44,46,47,48,50).

---

## [10]-[LEG_PARTITION] — proven acyclic against the corrected seam graph

Within a leg the listed order IS dependency order. The four on-disk intra-package inversions resolve per V5 (frozen vocabulary contracts consumed across legs). The DECISION freezes `StorageTier`/`ObjectStore.Head`/`ServerExtension`/`TimeCut` shapes at decision time, so an earlier leg consumes the frozen CONTRACT (a DECISION artifact), never the later leg's rebuilt page — the leg graph is acyclic on the contract graph.

1. **SPINE + PERIMETER** — the `[V4]` band registry landing COMPLETE (every row incl. the new-owner bands Egress/Retrieval/Federation/Schedule/Coordination + the re-banded Server/Tabular, so a duplicate integer is unrepresentable from leg 1); `Element/` (authority extraction V5a, EF commit + migration owner V6, codec consumer-contracts V9 + kernel-hash re-anchor + HashPolicy V10); the FULL roster reconciliation + catalog re-anchor/dedup/stub motion (V7, ROSTER — gates every later page's imports); the own-ledger seam corrections (V12 half); the csproj AppHost-reference removal. Pages: graph, codec, identity, authority.
2. **VERSION ENGINE** — ledger/commits/timetravel/merge/provenance/retention/recovery V10 repairs; the `AsOfKey` arm; the `CRDT_OP_SET` parity-fixture pin (commits); the V8 Type-correlation path + named migration case; the V9 Scrub/Bisect incremental re-shape (interim documented). Pages: ledger, commits, timetravel, merge, provenance, retention, recovery.
3. **QUERY + INGEST** — retrieval extraction with its typed band (V5b, extracted BEFORE the federation owner that lowers onto it); columnar trust gates + posture constant; the V1 federation owner; cache/cypher/topology cold passes; Ingest growth (V11). Pages: lane, retrieval, topology, columnar, cypher, cache, federation, tabular, schedule.
4. **STORE + COORDINATION + EGRESS** — blobstore V10 repairs; provisioning loose-code typing onto the registry; `Store/coordination.md` (V2, landed BEFORE the egress pump that drains its cursor); the egress owner (V3) over the leg-2 changefeed and the leg-4 outbox spine. Pages: blobstore, provisioning, coordination, egress.

Acyclicity edges verified: `graph(1)←timetravel(2)` TimeCut, `retention(2)←blobstore(4)` StorageTier, `recovery(2)←blobstore(4)` ObjectStore.Head, `cypher(3)←provisioning(4)` ServerExtension — all FROZEN-CONTRACT (earlier leg reads the DECISION-frozen shape, not the later rebuild). `egress(4)→coordination(4)` OUTBOX_CURSOR + `egress(4)←ledger(2)` CHANGEFEED — within/up-leg, no reverse (coordination never reads the pump). `federation(3)→lane(3)`+`→columnar(3)` — within-leg, ordered (retrieval/columnar before federation). `coordination(4)` read only by AppHost (external up-consumer; the csproj reversal removed) — no down edge. Graph acyclic.

Campaign acceptance (post-leg-4 composed dry-runs): (a) version-engine — commit → three-way merge across a simulated Type re-key reading as RENAME → AS-OF scrub incremental-shaped → attested verify → retention sweep → verified restore whose terminal proof is `reconstructed OfGraph == checkpoint AsOfKey` (`RecoveryFault.VerifyFailed` unreachable) beside a real RPO lag; (b) coordination — a fenced Budget debit whose stale-token replay is REJECTED (`LeaseFenced`), a step-state CAS, an outbox drain to a CloudEvents sink dedup'd by `id`=`ContentKey`; (c) federation — a Substrait plan lowered to `SetExpr` executing on the columnar lane; (d) perimeter audit — every fault code resolves to exactly one registry row, every digest mint resolves through `ContentHash.Of`/`ContentAddress`, every catalog anchor + card anchor resolves on disk, `dotnet restore` clean.

---

## [11]-[SELF_REPORT] — exact counts

- **Page rows**: 24 (Element 4 · Version 8 · Query 7 · Ingest 2 · Store 3). Lowering: 6 `new` (authority, egress, retrieval, federation, schedule, coordination) · 5 `rebuild` (identity, commits, recovery, lane, provisioning) · 13 `improve` (graph, codec, ledger, timetravel, merge, provenance, retention, topology, columnar, cypher, cache, tabular, blobstore) · 2 absorb pairs (authority←identity, retrieval←lane) · 0 deletePages.
- **Verdicts disposed**: 13 (V1-V13, each a ruling).
- **Evidence disposed**: 14 (E1-E14, each with the dossier drift correction carried).
- **Cards disposed**: 30 (11 IDEAS + 19 TASKLOG; 5 open corrected-and-held, 15 stale-COMPLETE re-pointed, 10 legit KEEP/DROP).
- **Packages disposed**: 5 PRUNE (PollinationSDK, 4 NTS bridges — counted as the 4-bridge cluster + PollinationSDK + EF-Sqlite + direct-Redis + ScyllaDB = 8 individual references across 5 charter proofs) → reported as 8 individual package references pruned; 5 scale-out KEEP as `[V13]` axis rows; the `[V3]` streaming cluster + `[V1]` federation trio KEEP as gated rows; 3 `[V6]` COMMIT; Ara3D BUMP 1.0.1→1.6.1; DuckDB substrait extension row ADD; Grpc.Tools assumption DROP.
- **Bands**: 19 own (5400-8430; three collisions + Commit re-partitioned, all unique) + 5 pinned-mirror foreign neighborhoods.
- **Seam rows**: 24 own (ARCH:41-64, 6 corrected) + 8 ADD (wired-undeclared + coordination + internal) + 14 sibling counterpart obligations.
- **AppHost PORT contracts homed**: 4/4 on `Store/coordination#COORDINATION_OP` + `#OUTBOX_SPINE` (APPHOST:76 BudgetDebit · :77 StepStateCas+StepStateInFlight · :78 OutboxCursor · :79 LeaseAcquire/Renew/Release+MembershipUpsert/View).
- **Federation probe**: honored — V1 REINTRODUCE (FEASIBLE_WITH_GAPS); Grpc.Tools DROPPED, DuckDB substrait extension ADDED, `api-flowtide-substrait.md:151` corrected.
- **Leg partition**: 4 legs, acyclic on the frozen-contract graph.
