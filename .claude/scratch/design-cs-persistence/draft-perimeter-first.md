# [DRAFT — PERIMETER_FIRST] Rasm.Persistence restructure blueprint

LENS: perimeter-first. The 18 fence interiors are world-class; the PACKAGE around them is fiction. This blueprint derives the structure from four perimeter invariants and fits capability inside them: (1) ONE type-enforced fault-band registry where a duplicate integer fails at type-init; (2) a seam ledger where every row composes in a fence and every fence crossing has a row, both directions; (3) catalogs anchored to live pages over a zero-orphan manifest; (4) a card pool whose every claim resolves on disk. The final page-set is the SHADOW these invariants cast — 24 pages, zero one-file folders, every fault banded, every seam rowed, every orphan disposed, every card true.

THESIS: the perimeter breaks are worse than the register states in five places and better in one. Worse: `commits.md` is a THIRD un-banded bare-`Error.New` owner (H1 — the brief's "two live breaches" is FALSE); the version engine mints ~10 raw `XxHash128` keys leg 2 has no mandate to re-anchor (H2 — the "one hasher entry" law is cosmetic across half the durable identities); `ColumnarFault` owns the whole 8350-8356 decade and 837x is a THREE-way collision (E4 undercount); provisioning rails a fourth/fifth bare-`Error.New` at 7701/7702 the E4 census omits; GCS/Minio blobstore rows declare a checksum their legs never supply (E9 half-fix). Better: the V1 federation wire-form premise is OVERTURNED in the owner's favor — `SubstraitDeserializer.Deserialize` ships PUBLIC, collapsing transcription to ~2 lines and killing the `Grpc.Tools` admission. The blueprint disposes all of it.

DISPOSITION SUMMARY: V1 REINTRODUCE `Query/federation.md` (probe FEASIBLE_WITH_GAPS). 24 pages (18 kept/rebuilt + 6 minted: authority, retrieval, federation, coordination, egress, schedule). 19 own fault bands + 6 pinned foreign-mirror neighborhoods in ONE registry. 66 manifest packages disposed (14 PRUNE, 7 COMMIT, 11 SINK-ROW, 7 RIDE-verdict, 27 KEEP) + the csproj AppHost-reference inversion. 30 cards disposed (0 surviving phantom, 0 stale-COMPLETE). All four AppHost PORT contracts homed on `Store/coordination#COORDINATION_OP`. Leg partition acyclic.

---

## [0]-[PERIMETER_TRUTH_BOARD]

The disk state the structure is derived from. Every row re-verified across the five dossiers; drift corrected here is binding.

| Invariant | On-disk truth | Perimeter verdict |
|---|---|---|
| Fault registry | NO type-enforced registry; per-page decade prose; THREE un-banded bare-`Error.New` owners (lane 8360-8363, provisioning 8371-8380 + 7701-7702, **commits 8261/8263/8264** — H1); three collision decades (835x, 836x, 837x); `GraphFault` simple-name ×2 | MINT ONE `[SmartEnum<int>]` `FaultBand` registry on `graph#FAULT_TABLES` (§1) |
| Hasher entry | ~10 version-engine raw `XxHash128` mints bypass `ContentHash.Of` (H2); codec composes the seam correctly; `ContentParityCorpus.Seed=0L` dead | EXTEND the re-anchor enumeration to leg 2 (§1.3); the mints are value-identical, so pure call-path collapse |
| Seam ledger | own ledger: 3 declared-unwired (`:50/:55/:57`), 3 wired-undeclared (codebook⇄Compute, cache-L2⇄AppHost, presence←AppHost), 0 counterparts for 4 AppHost PORTs; 12 stale sibling rows; `ARCH:55` mis-targeted | REWRITE the ledger both directions (§2); home all four PORTs (§6); list 12 counterpart obligations |
| Manifest | ~30 of ~66 packages zero-consumer; csproj:10 `Rasm.AppHost` ProjectReference = strata inversion; block count 66 | PRUNE/RIDE/COMMIT (§3); INVERT the reference (§3.4) |
| Catalogs | 77 package + 31 shared; 21 anchor-less (incl. core marten/npgsql/objectstore); ~35 dead-only; messagepack byte-dup misfiled; 4/77 + 0/31 tagged | re-anchor to live pages + uniform tag + union divergent overlays (§3.5) |
| Cards | 4 open (3 phantom-realization + 1 QUEUED), 7 IDEAS-closed, 19 TASKLOG (18 COMPLETE incl. 6 citing deleted pages, 1 DROPPED) | dispose all 30 (§4) — 0 phantom survives |
| Federation | `SubstraitDeserializer.Deserialize(Substrait.Protobuf.Plan)` PUBLIC (probe); `SqlPlanBuilder`/`RelationVisitor` public; `SubstraitSerializer` internal | REINTRODUCE; transcription ~2 lines; DROP `Grpc.Tools` (§7) |

---

## [1]-[THE_BAND_REGISTRY] — perimeter spine

ONE `[SmartEnum<int>] FaultBand` owner block `[FAULT_TABLES]` on `Element/graph.md` (the store-rail root every rail composes), mirroring `.archive/RASM-COMPONENT-PARADIGM-DECISION.md:141-149` — `sealed partial class FaultBand` with `new(<band>, owner:)` rows whose source-generated key lookup makes a duplicate integer FAIL AT TYPE INITIALIZATION. Every fault union derives `Code => Band + n`; every receipt-carried code resolves through the registry. Per-page decade prose (`topology:166`, `columnar:128`, `cypher:49`) dies for one registry pointer. Lands COMPLETE in leg 1 (every own band + every foreign mirror reserved), so a duplicate integer is unrepresentable from the first leg; each union MINTS in its owning leg.

### [1.1] OWN PERSISTENCE BANDS (19)

| Band | Union | Page | Codes | Motion | Leg minted |
|---|---|---|---|---|---|
| 540x | `RemoteStoreFault` | Store/blobstore | 5401+n | register as-is | 4 |
| 771x | `EmbeddedFault` | Store/provisioning | 7711-7714 | register as-is; **7701/7702 loose Refused rail `EmbeddedFault.Refused` (7714), no new codes** (dossier E4 undercount) | 4 |
| 825x | `SyncFault` | Version/ledger | 8251-8256 | register as-is (the one correct typed band on disk) | 2 |
| 826x | `CommitFault` | Version/commits | 8261 decode-drift, 8263 parity-drift, 8264 owner-mints | **MINT `[Union]:Expected` — H1: on disk these are bare `Error.New`, NOT a typed band; "register as-is" is wrong** | 2 |
| 827x | `EgressFault` | Version/egress | 8271 sink-unreachable, 8272 dead-letter, 8273 cursor-stall | NEW owner (V3); Version-adjacent gap | 4 |
| 828x | `RetentionFault` | Version/retention | 8281-8283 | register as-is | 2 |
| 829x | `RecoveryFault` | Version/recovery | 8291-8293 | register as-is | 2 |
| 830x | `GraphFault` | Element/graph | 8300-8302 | register as-is; **KEEPS the name** (cypher renames) | 1 |
| 831x-833x | `CodecFault` | Element/codec | 8310 / 8320+Rank / 8330 | register as-is (legal multi-decade stride) | 1 |
| 834x | `IdentityFault` | Element/identity | 8341+n | register as-is | 1 |
| 835x | `ColumnarFault` | Query/columnar | **8350-8356 (7 cases)** | KEEPS 835x; **register corrected — the whole decade, not 8350-8352** | 3 |
| 836x | `CypherFault` | Query/cypher | 8360-8363 | **RENAME from `GraphFault`**; keeps 836x; resolves the simple-name ×2 | 3 |
| 837x | `TopologyFault` | Query/topology | 8370-8371 | KEEPS 837x | 3 |
| 838x | `TabularFault` | Ingest/tabular | 8380-8383 | **RE-BAND off 837x** (was 8370-8373, three-way collision) | 3 |
| 839x | `ServerFault` | Store/provisioning | 8390-8392 ServerFault + 8393-8399 the ex-loose FailureRank/readiness/`Admit` sites as typed cases | **RE-BAND off 835x; absorbs all 7 loose provisioning `Error.New` sites** | 4 |
| 840x | `RetrievalFault` | Query/retrieval | 8401-8404 (codebook-train — the ex-lane bare 8360-8363) | NEW owner (V5b); **kills the only un-banded lane codebook** | 3 |
| 841x | `CoordinationFault` | Store/coordination | 8411 LeaseFenced, 8412 CasConflict, 8413 MembershipLapsed, 8414 OutboxDrain | NEW owner (V2) | 4 |
| 842x | `FederationFault` | Query/federation | 8421 SubstraitParse, 8422 UnsupportedRelation, 8423 SourceUnreachable, 8424 WriteRejected | NEW owner (V1) | 3 |
| 843x | `ScheduleFault` | Ingest/schedule | 8431 MpxjDecode, 8432 ScheduleReject | NEW owner (V11) | 3 |

Free after: 84xx from 8440 onward (growth headroom). The re-partition fits the 5xx/77x/82x-84x space entirely — the `[06]` federation-wide renumber escalation is NOT triggered. NOTE: the deleted `Query/transaction` folded `StoreFault.Concurrency` 7001 into `graph#STORE_RAIL` (TASKLOG:37); leg 1 verifies whether a 700x `StoreFault` concurrency case survives on graph (registered sub-band) or folded into `GraphFault` — the register census found no 700x, so the ruled default is folded into `GraphFault`; a surviving concurrency case registers as a graph sub-band row, never a loose integer.

### [1.2] PINNED FOREIGN-MIRROR NEIGHBORHOODS (6) — cross-package disjointness is a row, never prose

| Foreign owner | Reserved band(s) | Source of truth |
|---|---|---|
| AppHost | 1xxx, 4100-4810 | RASM-CS-APPHOST-BRIEF `[V1]` |
| Compute | 2200-2299 | Compute registry |
| Compute Remote `WireFault` | 4520-4532 | wire band |
| AppUi | 6xxx | AppUi registry |
| AEC registry | 2300 Component · 2350 Generation · **2400 Geometry (mirror of kernel `GeometryFault` 2400-2449, `Rasm/Numerics/faults.md`)** · 2450 Material · 2470 Projection · 2500 Element · 2600 Bim · **2700-2710 Fabrication** | `.archive/…DECISION.md:141-149` |
| kernel substrate | 9104 `Fault.UnsupportedCode` (the only coded case on `Rasm/Domain/rails.md` `Fault`) | landed kernel fence |

### [1.3] THE HASHER RE-ANCHOR ENUMERATION (H2 — perimeter law, not cosmetic)

`[SEAM_AND_RAIL_LAW]` says "every Persistence digest mint composes the landed kernel `ContentHash.Of` seed-zero entry." The brief enumerates codec-only targets (leg 1). The dossier proves the VERSION ENGINE mints ~10 raw `XxHash128` keys leg 2 has no mandate to touch. Value-identical (`ContentHash.Of(span) => XxHash128.HashToUInt128(span)` seed-zero, `Rasm/Domain/identity.md:31`), so pure call-path collapse. The DECISION EXTENDS the re-anchor to leg 2:

| Mint | Anchor | Re-anchor |
|---|---|---|
| `OpLogEntry.ContentKey` | ledger.md:153 | `ContentHash.Of(payload.Span)` |
| `CommitNode` content key | commits.md:91 | `ContentHash.Of(canonical.WrittenSpan)` |
| `CrdtWire.ContentKey` | commits.md:354 | `ContentHash.Of(EncodeCompanion(op).Span)` |
| `ParityVector.Pin/Stamped/Contribute` | commits.md:375-376,401 | route through `ContentHash.Of`; fold the dead `ContentParityCorpus.Seed=0L` into the mint or delete |
| `AgentKey` (durable PROV node identity) | provenance.md:291 | `ContentHash.Of(Encoding.UTF8.GetBytes(actor))` |
| edge content keys | timetravel.md:203-204 | `ContentAddress.Of(edge.ToCanonicalBytes(tolerance).Span)` — the **seam** form (H3), matching `merge.md:160`, not a raw instance |

Codec-tier targets stay leg 1 (`SnapshotHeader.Seal`, `ContentChunker`, cache identity, the V1 plan digest). The blobstore multipart/chunk folds whose payloads outgrow a one-shot span compose the identity.md Growth-row streaming member (`XxHash128.Append` + `GetCurrentHashAsUInt128`, seed zero) on landing; the `ContentAddress.Of(UInt128)` wrap of a precomputed digest is the documented interim.

---

## [2]-[THE_SEAM_LEDGER] — every crossing a row, both directions

The corrected `ARCHITECTURE.md#[02]-[SEAMS]` block. KEEP = live+wired; CORRECT = target/half fixed; SPLIT = row bifurcates with the `[V5]` extraction; ADD = wired-undeclared now rowed; NEW = minted-owner seam. One anchor per seam.

### [2.1] OWN LEDGER — corrected

| # | Row (owner → counterpart) | Kind | Motion |
|---|---|---|---|
| 41 | `Element/graph ← Rasm.Element` ElementGraph/GraphDelta SoR | SEAM | KEEP |
| 42 | `Element/codec ← Rasm` seed-zero `XxHash128` entry | CONTENT_KEY | KEEP (the one hasher; §1.3 extends its reach) |
| 43 | `Element/codec → ts:wire` SnapshotHeader canonical-CBOR | WIRE | KEEP |
| 44 | `Version/commits → ts:wire` CrdtOpWire msgpack union | WIRE | KEEP |
| 45 | `Version/commits ⇄ py:runtime/transport` CrdtOp None-companion + parity corpus | WIRE | KEEP |
| 46 | `Version/commits → ts:state` commit/branch/vv/Merkle shapes | SHAPE | KEEP |
| 47 | `Version/merge → ts:wire` JsonPatch RFC 6902 egress | SHAPE | KEEP |
| 48 | `Version/ledger ⇄ py:runtime/transport` OpLogEntry CRDT delta | WIRE | KEEP |
| 49 | `Version/ledger ⇄ AppHost/Runtime` HLC + TraceSlot | PORT | KEEP |
| 50 | `Version/timetravel ← py:data/gridded/virtual` icechunk **`AsOfKey`=`Checkpoint.Hash`** | CONTENT_KEY | **CORRECT** — mint `AsOfKey` on the live page (was declared-unwired; TASKLOG:48 placed it on deleted `Version/snapshots`) |
| 51 | `Version/provenance ← py:artifacts/provenance` signed-artifact content-key | CONTENT_KEY | KEEP (the `[ARTIFACT_CONTENT_KEY_FEDERATION]` card re-anchors HERE, not the phantom federation) |
| 52 | `Version/retention ← Rasm.Compute` content-keyed Assessment blobs | CONTENT_KEY | KEEP |
| 53a | `Element/identity ⇄ AppHost/Runtime` TenantId RLS + KMS SigningKeyring+EnvelopeKeyring unwrap | PORT | **SPLIT** — identity keeps KMS custody + Tenant-RLS (APPHOST:72 unchanged) |
| 53b | `Element/authority ← AppHost/Runtime` `ObjectAcl` identity store | PORT | **SPLIT-NEW** — the `ObjectAcl` store moves to authority (V5a) |
| 54 | `Element/graph ← AppHost/Runtime` ClockPolicy/CorrelationId/TenantContext | PORT | KEEP |
| 55 | `Version/merge ← Rasm/Spatial/reconciliation` adjacency-derived `GeometryHash` | CONTENT_KEY | **CORRECT** — retarget off `Query/topology` (zero reconciliation refs) onto `Version/merge#STRUCTURAL_DIFF` (the named `GraphNode.GeometryHash` consumer); the adjacency-vs-Representations digest-semantics resolution is the geometry-`[V8]` counterpart (§8) |
| 56 | `Query/columnar ← Rasm.Bim/Model` BimOpenSchema FlatTableProjection | PROJECTION | **RULE** — the fence implements it Persistence-side generic (`columnar.md:435-448`), not "Bim-implemented"; rule the Persistence-owned generic branch law and align the row |
| 57a | `Query/lane ⇄ py:data/tabular/query` ElementSet receipt currency | WIRE | **SPLIT** — ElementSet half stays lane |
| 57b | `Query/federation ⇄ py:data/tabular/query` Substrait portable plan | WIRE | **SPLIT-NEW** — the Substrait half re-homes to the V1 owner (§7) |
| 58 | `Query/cache ← Rasm.Compute` artifact/result/benchmark index | INDEX | KEEP |
| 59 | `Store/blobstore ← Rasm.Compute` authored-GLB GeometryHash | CONTENT_KEY | KEEP |
| 60 | `Store/blobstore ← Rasm.Bim/Exchange` imported IFC/BREP | CONTENT_KEY | KEEP |
| 61 | `Ingest/tabular → Rasm.Element` row shape only | WIRE | KEEP |
| 62 | `Query/columnar ⇄ py:data/tabular` Arrow over ADBC | WIRE | KEEP |
| 63 | `Store/provisioning ← AppHost/Observability` **Npgsql-only** HEALTH_PROBE | HEALTH_PROBE | KEEP (AppHost:85 re-points to match — drops Redis/Kafka) |
| 64 | `Version/recovery ← AppHost/Runtime` RPO/RTO objective | PORT | KEEP |

### [2.2] OWN LEDGER — ADD (wired-undeclared + minted-owner seams)

| Row | Kind | Evidence |
|---|---|---|
| `Query/retrieval ⇄ Rasm.Compute/Model/embedding` VECTOR_CODEBOOK | SEAM | wired lane.md:381-388, declared COMPUTE:99, no own row; post-V5b targets retrieval |
| `Query/cache ⇄ AppHost/Runtime` L2 partition + TenantId RLS | CACHE_PORT | wired cache.md:191-232, declared APPHOST:69; distinct from row 58 (Compute INDEX) |
| `Version/ledger ← AppHost/Wire/companion` PeerRoster presence beats | PRESENCE | wired ledger.md:466-472 over `DrainSurface`; producer is AppHost companion roster (Persistence-owned wire row, no AppHost type down) |
| `Store/coordination ⇄ AppHost/Agent/capability` fenced per-tenant Budget debit | PORT (ONE_FENCED_LEASE_STORE) | counterpart of APPHOST:76 (§6) |
| `Store/coordination ⇄ AppHost/Runtime/orchestration` step-state CAS + InFlight read | PORT (ONE_FENCED_LEASE_STORE) | counterpart of APPHOST:77 (§6) |
| `Store/coordination ⇄ AppHost/Wire/outbox` transactional outbox same-tx | PORT (ONE_OUTBOX_EGRESS_SPINE) | counterpart of APPHOST:78 (§6) |
| `Store/coordination ⇄ AppHost/Wire/Coordination` CAS+lease+membership | PORT (ONE_FENCED_LEASE_STORE) | counterpart of APPHOST:79 (§6) |
| `Version/egress ⇄ AppHost/Wire` OutboundHop wire-native delivery-honesty | WIRE | the V3 wire-native sink reads the AppHost `HopPolicy` declared guarantee (RASM-CS-APPHOST-BRIEF `[V9]`) |
| `Version/egress → Store/coordination` outbox cursor read | INTERNAL | the V3 acyclicity edge — egress reads coordination's cursor; NO reverse edge (coordination never reads the pump) |
| `Query/federation → Query/retrieval + Query/columnar` SetExpr + columnar/ADBC lowering | INTERNAL | the two-target lowering (§7); intra-leg (retrieval/columnar rebuild before federation) |
| `Store/blobstore ⇄ Rasm.Compute` `Placement`→GeometrySource egress | CONTENT_KEY | dossier C2 — currently prose only (`blobstore.md:272-289`); rowed explicitly |
| `Ingest/schedule ← Rasm.Bim/schedule` P6/MS-Project schedule wire | WIRE | counterpart of BIM:102 (§8); MPXJ codec + durable rows |
| `Query/columnar ← Rasm.Bim/Semantics/geospatial` GDAL/OGR GeoParquet ingest | TRANSPORT | **homes BIM:81** (was unhomed `/Store` folder); the OGR Arrow C-stream columnar ingest |

### [2.3] SIBLING COUNTERPART OBLIGATIONS (12+2) — corrected targets, sibling interiors out of edit scope

| Sibling row | Dead target | Corrected target | Owning campaign |
|---|---|---|---|
| COMPUTE:111 FastCDC content-key delta | `Rasm.Persistence/Sync` | `Element/codec#CONTENT_CHUNKER` | Compute |
| COMPUTE:115 parse-to-canonical Extract | `Query/pipeline` | `graph#STORE_RAIL` / `Ingest` | Compute |
| COMPUTE:116 anomaly rule source | `Store/quality` | columnar/provisioning verification surface | Compute |
| COMPUTE:119 Protobuf Kafka topics | `Sync` | retire with `[V3]`/`[V7]` streaming | Compute |
| BIM:91 federation AuditEntry log | `Query/federation` | `[V1]` `Query/federation` OR `ledger#CHANGEFEED` | Bim |
| BIM:95 IFC validation rules | `Store/quality` | columnar/provisioning quality surface | Bim |
| BIM:101 durable annotation + CDE | `Sync/annotation` | `ledger#CHANGEFEED` | Bim |
| BIM:102 P6/MS-Project 4D | `Sync/schedule` | `[V11]` `Ingest/schedule.md` | Bim |
| BIM:104 Speckle Base import | `Sync` | `Version/ledger` (Speckle SyncTransport) | Bim |
| APPHOST:71 drain 2PC in-doubt | `Query/transaction` | `graph#STORE_RAIL` (V12 prepared-tx RETIRE) | AppHost |
| APPHOST:73 keyed OutboundHop egress | `Sync/egress` | `[V3]` `Version/egress` | AppHost |
| APPHOST:85 health-probe driver roster | Npgsql/Redis/Kafka | `Store/provisioning`, **Npgsql-only** (matches ARCH:63) | AppHost |
| BIM:81 GDAL/OGR GeoParquet (extra) | `/Store` folder | `Query/columnar` (§2.2) | Bim |
| BIM:94 `CommitGraph.MergeBase` (spelling) | — | align to `commits#MergeBase` (static, no `CommitGraph` type) | Bim |

---

## [3]-[THE_ROSTER_+_CATALOG_PERIMETER] — zero orphans

Central ownership `Directory.Packages.props` `Persistence` block (66 packages, `:252-317`) + `Rasm.Persistence.csproj` (85 references, incl. shared substrate). The manifest motion lands in leg 1 (every prune is zero-page-consumer, safe). INTEGRATION-FIRST: a zero-consumer package is realized as a row before it is a removal candidate; a removal carries a per-package redundancy/charter proof.

### [3.1] PRUNE (12) — each leaves manifest + csproj + README group + `.api` in one motion on its named proof

| Package | Proof |
|---|---|
| `ClickHouse.Driver` | redundant vs in-PG TimescaleDB OLAP; V13 admits a ClickHouse row only where TimescaleDB provably can't own scale — no consumer today |
| `ScyllaDBCSharpDriver` | no named owning axis; wide-column has zero consumer; redundancy proof rules |
| `Qdrant.Client` | redundant vs in-PG pgvector/pgvectorscale ANN; V13 row only over a proven in-PG ceiling |
| `DeltaLake.Net` | redundant vs DuckDB columnar + the recorded DuckLake forward candidate; no Delta-wire consumer |
| `rocksdb` | redundant vs the SQLite embedded floor; V13 EngineOps-tier row only over a proven SQLite ceiling |
| `LightningDB` | same as `rocksdb` |
| `PollinationSDK` | charter misplacement — a domain cloud SDK with no store-class concern; re-admission belongs to whichever package names a consumer |
| `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` | no persisted NTS geometry column; identity.md:19 mis-lists it (dossier Defect C) — H3 rides `pocketken.H3` |
| `Npgsql.NetTopologySuite` | same |
| `NetTopologySuite.IO.GeoJSON4STJ` | same; `api-h3.md:3` NTS-IO composition claim corrects to transitive core-NTS |
| `NetTopologySuite.IO.GeoPackage` | same |
| `Microsoft.EntityFrameworkCore.Sqlite` | the embedded floor is raw ADO by design (`provisioning.md:369-434`) |
| `StackExchange.Redis` (direct ref) | the L2 swap row keeps `Microsoft.Extensions.Caching.StackExchangeRedis`, which transits the driver (`cache.md:185`) |
| `linq2db.EntityFrameworkCore` | redundancy proof: DuckDB `COPY` + Npgsql binary import own bulk-copy; the `tabular.md:5,20` BulkCopy ref is prose-only, adds no fence |

(14 rows — `StackExchange.Redis` and `linq2db` fold into the PRUNE class beside the 12 named.)

### [3.2] COMMIT / MINE (7) — keep + newly compose

| Package | Composition |
|---|---|
| `EFCore.NamingConventions` | V6 `UseSnakeCaseNamingConvention()` on the identity DbContext; ~40 hand `HasColumnName` die |
| `Microsoft.EntityFrameworkCore.Design` | V6 DDL/migration owner (`element_identity`/`node_cell` DDL) on identity's schema section |
| `Thinktecture.Runtime.Extensions.EntityFrameworkCore10` | V6 `UseValueObjectValueConverter()`; the hand `NodeId` converter (`identity.md:120`) dies; the LanguageExt-type converters stay |
| `Pgvector.EntityFrameworkCore` | V6 `<->`/`<=>` LINQ distance as the server-side ANN row on `Query/retrieval` (in-process PQ/ADC keeps the hot-set lane) |
| `NATS.Net` | V3 JetStream durable streams — `Nats-Msg-Id` dedup + Settle-ack cursor advance |
| `CloudNative.CloudEvents` (+`.SystemTextJson`) | V3 the ONE egress envelope — `id`=`ContentKey`, `Sequence`=cursor, `partitionkey`=`EntityKey` |
| `Ara3D.BimOpenSchema` (+`.IO`) | KEEP on the live columnar consumer; **BUMP 1.0.1→1.6.1** (feed truth — 6 releases stale) + re-verify `AssemblyConfiguration("Debug")`; retire the DEBUG-IL caveat if fixed, escalate to in-corpus absorption if it persists |

### [3.3] SINK-ROW DEFAULTS (11) + RIDE-VERDICT (6) — INTEGRATION-FIRST, leave only on per-package proof

SINK-ROW (V3 — one delivery row over the one pump fold; a leaving row takes its serdes satellites + `.api` + README row + AppHost probe row): `Confluent.Kafka`, `Confluent.SchemaRegistry`(+`.Serdes.Avro`/`.Json`/`.Protobuf`), `Chr.Avro`(+`.Binary`/`.Confluent`) as the Kafka row's codec column, `CloudNative.CloudEvents.Kafka`, `RabbitMQ.Client`, `DotPulsar`. Zero consumers never lowers the bar (enterprise-interop egress is standing 5x pressure).

RIDE-VERDICT: `FlowtideDotNet.Substrait` + `Apache.Arrow.Adbc`(+`.Drivers.Apache`/`.BigQuery`) + `Apache.Arrow.Flight` ride V1 — **KEEP** (probe FEASIBLE, §7); `MPXJ.Net` rides V11 — **KEEP** (schedule mints, BIM:102 consumer confirmed); `Npgsql.OpenTelemetry` ruled **KEEP** (the provisioning observability row — tracing+metrics builder subscriptions at the AppHost root).

### [3.4] STRATA INVERSION (csproj:10) — the build-graph fact

`Rasm.Persistence.csproj:10` carries `<ProjectReference Include="../Rasm.AppHost/Rasm.AppHost.csproj" />` — Persistence→AppHost, a PLACEMENT_LAW dependency-reversal at the build graph (AppHost is a PORT peer Persistence contributes rows to, never reverses). Since V2 makes the coordination op-union, fencing tokens, membership rows, and receipts Persistence-owned types AppHost's PORT adapters DECODE, the edge INVERTS: **REMOVE csproj:10**; the reference is AppHost→Persistence. Leg-1 structural fix. The `dotnet restore` acyclicity gate confirms.

### [3.5] CATALOG MOTION (leg 1) — `.api` obligations

- **Anchor 21 anchor-less catalogs** to live pages: `api-marten`→graph/provisioning, `api-npgsql`→provisioning, `api-objectstore`→blobstore (the event-store/driver/blobstore spine — first-class, not filler); the rest per the api-manifests §1 map.
- **Re-anchor ~35 dead-only + 16 mixed** off `Store/{profiles,remote,encryption,quality,server,tenancy}` · `Query/{federation,pipeline,transaction,rail,lanes}` · `Version/snapshots` · `Sync/*` · `Schema/*` onto the live set (`api-duckdb`→`Query/columnar`, `api-cbor`→`Element/codec`, `api-zstd`/`api-lz4`→`Element/codec`, `api-arrow`→`Query/columnar`, etc.).
- **DELETE** the misfiled shared `libs/csharp/.api/api-messagepack.md` (byte-identical dup, shasum `5527490f`, mis-tagged `RASM_PERSISTENCE`) or re-scope `RASM_PERSISTENCE`→neutral `RASM_API` (matching the landed `RASM_API_LANGUAGEEXT` precedent).
- **UNION** the 8 divergent double-cataloged overlays (`highperformance`/`hybrid-cache`/`jsonpatch`/`nodatime-stj`/`nodatime`/`redaction`/`thinktecture-json`/`hashing`) into their shared-tier owners; the package copies collapse to one-line pointers (content UNIONED before delete — they diverge).
- **CORRECT** `api-h3.md:3` NTS-IO claim → transitive core-`NetTopologySuite` 2.6.0; `api-flowtide-substrait.md:151` → the public `SubstraitDeserializer.Deserialize` (§7 G1).
- **ADD** to `api-npgsql.md` the advisory-lock/`LISTEN`/`NOTIFY` rows (catalog gap; V2 fenced lease + V3 pump wake); to `api-objectstore.md` the `CompleteMultipartUploadRequest.ChecksumXXHASH128` row after `assay api` confirms it against AWSSDK.S3 4.0.25.3 (the two enum members `XXHASH128`/`FULL_OBJECT` are already verified).
- **UNIFORM** provenance tag `<!-- catalog:Pkg@ver -->` + `[STACKING]` section across the kept tier (currently 4/77 + 0/31); the tag is the single version-echo reconciled on every roster motion.
- **STUB** the DuckDB `substrait` community-extension row (`ColumnarExtension.Substrait` — extension row, NOT a NuGet; §7 G3); pruned packages' catalogs delete with them.
- `api-languageext.md` obligation is DISCHARGED (already landed, substantive) — verify-only.

---

## [4]-[THE_CARD_POOL] — every claim resolves on disk

Full disposition of all 30 cards (11 IDEAS + 19 TASKLOG). Zero phantom-realization clause survives; every stale `-[COMPLETE]` re-points to its real fold owner or dies.

### [4.1] IDEAS.md (11)

| Card | Status now | Disposition |
|---|---|---|
| `PERSISTENCE_LIBRARY_TABLES` | QUEUED | KEEP OPEN; re-anchor off the (correct) Materials content-key pins; durable schema lands when Materials pins — honest QUEUED |
| `FABRICATION_PROGRAM_DURABLE_ROWS` | QUEUED | KEEP OPEN; **re-anchor off dead `Schema/ddl#IDENTITY`** onto blobstore content rows + cache artifact index + the `[V2]`/`Store` growth axis; carry the forward constraint (durable receipts decode re-banded **2701-2710** Fabrication codes, never 25xx); KIND-AGNOSTIC `ArtifactKind` rows on the ONE artifact index under `ContentHash.Of` |
| `REUSE_WIRE` | BLOCKED | **CORRECT the "is realized" phantom** — re-anchor to the real `ElementSet`/`cache`/`commits` currency + the `[V1]` `Query/federation` owner; honest status: lowering target realized as fences, BLOCKED on the `python:data` producer |
| `SUBSTRAIT_FEDERATION_SEAM` | BLOCKED | **CORRECT the "is realized" phantom** — re-anchor to `[V1]` `Query/federation`; SetExpr subset zero-gap fences, tabular subset scoped (§7); BLOCKED on the Python producer |
| `PERSISTENCE_BIM_SYNC_CRDT` | COMPLETE | KEEP CLOSED (verified — `commits#CRDT_ALGEBRA` `CrdtField`/`Crdt.Apply` real) |
| `DURABILITY_RECOVERY_OBSERVATORY` | COMPLETE | KEEP CLOSED (verified — `recovery.md` real) |
| `TRANSACTION_CONCURRENCY_CONTROL` | COMPLETE | **RE-POINT** off deleted `Query/transaction.md` → `graph#STORE_RAIL` fold (TASKLOG:37) |
| `ENVELOPE_ENCRYPTION_KMS` | COMPLETE | **RE-POINT** off deleted `Store/encryption.md` → `Element/identity#AUTHORITY` KMS custody (settled, TASKLOG:39) |
| `DATA_QUALITY_INTEGRITY_FRAMEWORK` | COMPLETE | **RE-POINT** off deleted `Store/quality.md` → columnar/provisioning verification surfaces |
| `BULK_ETL_INTERCHANGE_PIPELINE` | COMPLETE | **RE-POINT** off deleted `Query/pipeline.md` → `Query/columnar` |
| `CDC_STREAMING_EGRESS` | COMPLETE | **RE-POINT** off deleted `Sync/egress.md` → the `[V3]` `Version/egress.md` owner |

### [4.2] TASKLOG.md (19)

| Card | Status now | Disposition |
|---|---|---|
| `ARTIFACT_CONTENT_KEY_FEDERATION` | BLOCKED | **CORRECT the `Query/federation#ENTITY_GRAPH SourceKind.SignedArtifact` "is realized" phantom** — the signed-artifact content-key binding homes on `Version/provenance` (ARCH:51, live), OR the `[V1]` federation owner; honest BLOCKED on the artifacts producer |
| `PERSISTENCE_BIM_ARTIFACT_INDEX` | COMPLETE | KEEP CLOSED (`Query/cache#ARTIFACT_BLOB_INDEX` real) |
| `RECOVERY_PAGE_AUTHOR` | COMPLETE | KEEP CLOSED (verified) |
| `RECOVERY_OBJECTIVE_COLLAPSE_RECOVERYFACT` | COMPLETE | KEEP CLOSED (verified) |
| `TRANSACTION_PAGE_AUTHOR` | COMPLETE | **RE-POINT** off deleted `Query/transaction.md`/`.cs` → `graph#STORE_RAIL` fold |
| `CONFLICTOUTCOME_DISCRIMINANT_COLLAPSE` | COMPLETE | KEEP CLOSED (`ledger#MERGE_LAW ConflictResult` real) |
| `ENCRYPTION_PAGE_AUTHOR` | COMPLETE | KEEP CLOSED (records the fold into identity#AUTHORITY + blobstore#BLOB_GC) |
| `SQLCIPHER_RESEARCH_PROMOTE` | DROPPED | KEEP CLOSED (verified drop) |
| `QUALITY_PAGE_AUTHOR` | COMPLETE | **RE-POINT** off deleted `Store/quality.md`/`.cs` → columnar/provisioning surfaces |
| `PIPELINE_PAGE_AUTHOR` | COMPLETE | **RE-POINT** off deleted `Query/pipeline.md`/`.cs` → `Query/columnar` |
| `EGRESS_PAGE_AUTHOR` | COMPLETE | **RE-POINT** off deleted `Sync/egress.md`/`.cs` → `[V3]` `Version/egress.md` |
| `SCHEMADDL_SQL_COLLAPSE` | COMPLETE | **RE-POINT** off `SchemaDdl.Sql` (nonexistent — self-contradicts :45) → the `[V6]` identity migration owner |
| `STORE_SERVER_SPLIT` | COMPLETE | KEEP CLOSED (verified — `Store/provisioning.md` real) |
| `ANNOTATION_RELOCATE_TO_BIM` | COMPLETE | **RE-POINT** off deleted `Sync/annotation.md` + phantom `Query/federation#ENTITY_GRAPH` join → `ledger#CHANGEFEED` |
| `SCHEDULE_RELOCATE_TO_BIM` | COMPLETE | **RE-POINT** off deleted `Sync/schedule.md` → the `[V11]` `Ingest/schedule.md` (which now mints) |
| `ICECHUNK_ASOF_CONTENT_KEY` | COMPLETE | **RE-POINT** off deleted `Version/snapshots#SNAPSHOT_PROTOCOL AsOfKey` → the live `Version/timetravel` `AsOfKey`=`Checkpoint.Hash` arm (V12) |
| `KMS_PACKAGE_ADMISSION` | COMPLETE | KEEP CLOSED (verified) |
| `ARROW_FLIGHT_PACKAGE_ADMISSION` | COMPLETE | KEEP CLOSED; Flight rides V1 |
| `KAFKA_CLOUDEVENTS_PACKAGE_ADMISSION` | COMPLETE | KEEP CLOSED; the packages become V3 sink rows |

Perimeter proof: after this motion, grep `FederatedEntity|FederatedPlan|PlanNode|SourceKind|Query/federation` over cards = 0 realized-claims; every `-[COMPLETE]` anchor resolves on disk.

---

## [5]-[THE_PAGE_SET] — 24 pages, the shadow the perimeter casts

The perimeter forces the pages: a fault band with no owner page mints one; a wired-undeclared seam demands a page to home it; an orphan package realized as a row demands the axis page that carries it. 18 kept/rebuilt + 6 minted. Zero deletions (the element rebuild already deleted `Sync/`/`Schema/`/etc.; the two split sources survive as rebuilt remainders). Zero one-file folders.

### [5.1] ENGINE-NATIVE ACTION TABLE — semantic action + rebuild-engine lowering

| Path | Semantic | Engine lowering | Band | Entry signature | Leg |
|---|---|---|---|---|---|
| `Element/graph.md` | KEEP | `improve` | 830x `GraphFault` + **owns `[FAULT_TABLES]` registry** | `GraphStoreOp` [Union] | 1 |
| `Element/codec.md` | KEEP | `improve` | 831x-833x `CodecFault` | `SnapshotCodec`/`ContentAddress` entries | 1 |
| `Element/identity.md` | SPLIT | `rebuild` (remainder) | 834x `IdentityFault` | `IdentityStore`/`SchemaGate` entries | 1 |
| `Element/authority.md` | NEW | `new`, absorb `{into: Element/authority.md, from: Element/identity.md}` | (vocabulary — set-algebra) | `Authority.Admit`/`Effective` (NONE-union; pure algebra) | 1 |
| `Version/ledger.md` | KEEP | `improve` | 825x `SyncFault` | `SyncOp`/changefeed entries | 2 |
| `Version/commits.md` | KEEP | `improve` (MINT `CommitFault`) | 826x `CommitFault` **MINT** | commit/CRDT entries | 2 |
| `Version/timetravel.md` | KEEP | `improve` | (rides Recovery/Graph bands) | `TimeTravelOp` entries | 2 |
| `Version/merge.md` | KEEP | `improve` | (typed conflict classes) | `StructuralMerge.Reconcile` | 2 |
| `Version/provenance.md` | KEEP | `improve` | (ProvFault, existing) | `ProvNode`/`AttestedLedger` | 2 |
| `Version/retention.md` | KEEP | `improve` | 828x `RetentionFault` | `RetentionOp`/sweep | 2 |
| `Version/recovery.md` | KEEP | `improve` | 829x `RecoveryFault` | `RecoveryRoute`/`RestoreStep` | 2 |
| `Version/egress.md` | NEW | `new` | 827x `EgressFault` | `EgressSink` [Union] over one pump | 4 |
| `Query/lane.md` | SPLIT | `rebuild` (remainder) | (routing; RetrievalFault extracted) | `ReadRouter`/`SetExpr` | 3 |
| `Query/retrieval.md` | NEW | `new`, absorb `{into: Query/retrieval.md, from: Query/lane.md}` | 840x `RetrievalFault` | `RetrievalOp` (fusion+PQ codebook) | 3 |
| `Query/topology.md` | KEEP | `improve` (Lca DAG-gate) | 837x `TopologyFault` | `TopologyQuery` [Union] | 3 |
| `Query/columnar.md` | KEEP | `improve` (trust gates) | 835x `ColumnarFault` (8350-8356) | `ColumnarOp`/`ColumnarExtension` | 3 |
| `Query/cypher.md` | KEEP | `improve` (rename) | 836x `CypherFault` **RENAME** | `GraphQuery` [Union] | 3 |
| `Query/cache.md` | KEEP | `improve` | (cache/blob classes) | `CacheOp`/`ArtifactKind` | 3 |
| `Query/federation.md` | NEW | `new` | 842x `FederationFault` | `FederationOp` (Substrait→SetExpr lowering) | 3 |
| `Ingest/tabular.md` | KEEP | `improve` (Sep explicit; `Wire` disposed) | 838x `TabularFault` RE-BAND | `TabularSpec` [Union] | 3 |
| `Ingest/schedule.md` | NEW | `new` | 843x `ScheduleFault` | `ScheduleSpec` (MPXJ codec + durable rows) | 3 |
| `Store/blobstore.md` | KEEP | `improve` (checksum honesty) | 540x `RemoteStoreFault` | `ObjectLeg`/`GraphStoreOp` blob | 4 |
| `Store/provisioning.md` | KEEP | `improve` (loose codes typed) | 771x `EmbeddedFault` + 839x `ServerFault` RE-BAND | `ClusterProvision`/`EngineOps` | 4 |
| `Store/coordination.md` | NEW | `new` | 841x `CoordinationFault` | `CoordinationOp` [Union] (§6) | 4 |

`deletePages`: [] (none — the split sources rebuild; the deleted-page set already happened in the element rebuild). `absorb` pairs: 2 (authority←identity, retrieval←lane). `new`: 6. `rebuild`: 2. `improve`: 16.

### [5.2] CHARTER CARDS — per-page skeleton, folder home, in/out seams

**Element/ (4 — leg 1; folder home: the ElementGraph store-load roundtrip)**

- `graph.md` [KEEP→improve] — unchanged interior (one materializer `GraphDelta.ReplayOnto`, co-txn identity, generated-total `GraphStoreOp.Switch`, one-boundary `Lift`). ADDS the `[FAULT_TABLES]` `[SmartEnum<int>] FaultBand` registry owner block (§1) — the store-rail root every rail composes; `GraphFault` becomes registry-derived (`Band + 830x`). IN: 41 (Rasm.Element), 54 (AppHost ClockPolicy). OUT: — (root). Consumes timetravel `TimeCut` (frozen-contract, V5d).
- `codec.md` [KEEP→improve] — SOUND interior (seam `ContentAddress`, 8-rank RejectTier, atomic seal, CBOR trust boundary, FastCDC). RULES: HashPolicy shrinks to `{Identity, Content}` (Wide/FrameWide/Compute/ByDomainId dead) + states the `HashDomain` forward-compat byte law; deletes the `:175` `ByDomainId` prose drift; records the `[V9]` incremental-`OfGraph` + `[SEAM_AND_RAIL_LAW]` streaming-identity consumer contracts; the codec-tier raw mints re-anchor onto `ContentHash.Of` (§1.3). IN: 42 (kernel hasher), 41. OUT: 43 (ts:wire).
- `identity.md` [SPLIT→rebuild] — slims to the relational tier + `IdentityPolicy` big-endian key axis + KMS custody (signing AND envelope on the one `KmsProvider` axis, settled) + `SchemaVerdict`. EF COMMIT: `UseValueObjectValueConverter()` + `UseSnakeCaseNamingConvention()`; hand `NodeId` converter + ~40 `HasColumnName` die; LanguageExt-type converters stay. MIGRATION OWNER: `element_identity`/`node_cell` DDL authored on the schema section (`Microsoft.EntityFrameworkCore.Design` earns admission); `ServerExtension.CreateSql` rows commit through this one migration owner. Embedded charter STATED: relational identity floor + `EngineOps` checkpoint/snapshot/backup, read-only/offline tier, never the SoR (Marten is PG-only). Packages:19 de-lists the pruned EF-NTS bridge. IN: 42, 53a (KMS+Tenant-RLS PORT), 72 (KMS-unwrap). OUT: — .
- `authority.md` [NEW←identity] — the object-ACL authz set-algebra (`Grant`/`GrantSet`/`AclScope`/`AclEntry`/`ObjectAcl`/`Authority.Admit`+`Effective`+`LapsedFor`, deny-over-allow, superuser-aware) — pure set-algebra, zero `KmsProvider`. Vocabulary page (entry = the algebra, NONE-union). IN: 53b (ObjectAcl store PORT). OUT: consumed by `commits#Movable` ACL gate.

**Version/ (8 — legs 2+4; folder home: the version-control engine projecting from Marten events; stays flat at 8, sub-partition recorded only if >~9)**

- `ledger.md` [KEEP→improve] — SOUND (one-materializer changefeed, `SyncFault` 825x the correct typed band, HLC LWW tie-break). REPAIRS: `SyncOpKind.Truncate`+`WholeRelation` wire-or-die; `OpLogEntry.Codec` collapses to a `Family`-derived accessor; `ProcessEventsAsync` folds the range into ONE `IO` (was per-event `RunAsync`); `OpLogEntry.ContentKey` re-anchors `ContentHash.Of` (§1.3). IN: 48 (py transport), 49 (AppHost HLC), 52 (Compute), presence←AppHost (ADD). OUT: 44/45/46 (ts/py wire).
- `commits.md` [KEEP→improve] — SOUND (content-addressed commit-DAG, six-type CRDT algebra, `CrdtOpWire` flagship). **MINTS `CommitFault`/`CrdtWireFault [Union]:Expected` 826x (H1 — the third un-banded owner)**. REPAIRS: `MergeBase` re-shapes to one reverse-reachability generation-mark pass + dominance filter (near-linear, killing O(V²) — has the named Bim consumer BIM:94); `VectorOrder`/`Order` wire-or-die; the parity mints + `ContentParityCorpus.Seed` re-anchor `ContentHash.Of`; **PINS the `CRDT_OP_SET` `MvRegister`/`opMerge` parity fixture** (kernel `reconciliation.md:129` DESIGN-PIN → REAL). IN: 45. OUT: 44/46, `Movable`←authority.
- `timetravel.md` [KEEP→improve] — SOUND (one materializer, two-axis RangeDiff, monotone bisect). ADDS the `AsOfKey`=`Checkpoint.Hash` arm (V12 — RAISED to load-bearing: ONE digest serving both the icechunk cross-runtime seam and the recovery content-identity oracle). Scrub/Bisect re-shape to fold the incremental `OfGraph(prior,delta)` contract (interim documented); edge keys use the seam `ContentAddress.Of(span)` (H3); `AggregateStreamToLastKnownAsync` composes-or-leaves. IN: 50 (icechunk AsOfKey — CORRECTED live). OUT: `TimeCut` consumed frozen by graph.
- `merge.md` [KEEP→improve] — SOUND (re-ingest `Reconcile` GlobalId alignment, two-axis three-way merge). ADDS the `[V8]`b Type-correlation key row — a stable classification-independent Type natural key so a re-keyed Type diffs as RENAME (coupled to the kernel `[V8]`a seed change, §8); threads `ElementJson.Options` STJ through the JsonPatch egress; `MemberPath` NodeId-typing VERIFIED clean (`api-generator-equals:52-53`). IN: 55 (reconciliation GeometryHash — CORRECTED target). OUT: 47 (ts:wire).
- `provenance.md` [KEEP→improve] — the strongest fence (exact W3C-PROV-O, independent-`digestOf` attested verify). REPAIRS: `ProvJson` reads the agent class off the `Principal` (not unconditional `Person`); the mutation-boundary annotation lands; `AgentKey` re-anchors `ContentHash.Of` (§1.3). IN: 51 (signed-artifact — the corrected home for `ARTIFACT_CONTENT_KEY_FEDERATION`). OUT: — .
- `retention.md` [KEEP→improve] — SOUND (six-row `RetentionClass`, fail-closed ceiling, full-history reachability GC). REPAIRS: `StorageLane.Durable` consume-or-die. Consumes blobstore `StorageTier` + timetravel `TimeCut` as FROZEN contracts (V5c). IN: 52. OUT: → blobstore GC.
- `recovery.md` [KEEP→improve] — SOUND (real `(Timeline,Lsn)` via Npgsql `IdentifySystem`, ranked restore, `ReAttest` content-identity). REPAIRS the two verified RPO bugs: `ObjectReplica` computes real freshest-blob-lag (killing `Duration.FromMinutes(absent.Count)`, :180); the WAL-throughput assumption (:171) lifts into a policy row. The AS-OF verify uses `AsOfKey` (`RecoveryFault.VerifyFailed` fires on reconstructed-vs-checkpoint mismatch). Consumes blobstore `ObjectStore.Head` FROZEN (V5c). IN: 64 (AppHost RPO/RTO). OUT: — .
- `egress.md` [NEW] — 827x `EgressFault`. ONE pump fold draining the `[V2]` outbox cursor past `OpLogEntry` rows into `EgressSink` delivery rows under ONE CloudEvents envelope (`id`=`ContentKey`, `Sequence`=cursor, `partitionkey`=`EntityKey`) — exactly-once EFFECT. Sink rows (seed DATA): `Webhook` (pg_net response-reconciled advance, NEVER fire-and-forget — advances only on `net.request_status`=SUCCESS), `Nats` (JetStream `Nats-Msg-Id` dedup + Settle-ack advance), the wire-native gRPC/Arrow hop (reads AppHost `HopPolicy` declared honesty). Kafka/RabbitMQ/DotPulsar land as sink ROWS by default. Typed dead-letter + replay. IN: OutboundHop honesty (ADD). OUT: → coordination outbox cursor (the acyclicity edge — no reverse).

**Query/ (7 — leg 3; folder home: the read lanes split by consistency demand)**

- `lane.md` [SPLIT→rebuild] — slims to READ_ROUTING + ELEMENT_SET_ALGEBRA (routing + `SetExpr`). SOUND: `StalenessWatermark` sequence-gap, length-framed `ElementSet.Canonical` parity contribution. Routes `WaitForNonStaleProjectionDataAsync` through `IProjectionDaemon`/`IMartenDatabase` (not the TestingExtensions symbol). IN: 57a (ElementSet currency). OUT: → federation (lowering target).
- `retrieval.md` [NEW←lane] — 840x `RetrievalFault` (the ex-lane bare 8360-8363 becomes typed). FUSION_AND_CACHE + VECTOR_CODEBOOK (one coupled ANN owner — fusion reads codebook ADC rows). The pgvector `<->`/`<=>` LINQ server-side ANN row lands HERE beside the in-process PQ/ADC hot-set lane (V6). IN: VECTOR_CODEBOOK⇄Compute (ADD). OUT: federation lowers key-selection here.
- `topology.md` [KEEP→improve] — SOUND (seam-frozen incidence, SCC-once). REPAIRS: `Lca` pre-gates `IsDirectedAcyclicGraph()` and rails `TopologyFault.Cyclic` on a cyclic Spatial view (symmetric with `Order` — api-quikgraph.md:26 flags Tarjan LCA unsound over cycles; the page models containment cycles as real). `TypedEdge.IsContainment`/`Kind` dead-accessors dispose. `TopologyView.Of` OfGraph memo stays (acceptable). IN: — . OUT: — (ARCH:55 re-targeted OFF topology).
- `columnar.md` [KEEP→improve] — SOUND (DuckDB engine owner, posture-anchor session). REPAIRS: typed `Identifier`/`StorePath`/`SecretName` trust gate on every raw-interpolated identifier/path (`Mount`/`Secret`/`Egress {projection}`/`Generation`; `{projection}` becomes a composed typed projection, never a raw SQL fragment); the four `"80%"`/`"90%"` posture literals collapse to one constant. ADDS the `ColumnarExtension.Substrait` row (V1 execution). Rules the Persistence-owned generic `BimOpenSchemaProjection` branch (ARCH:56). Phantom-spellings close: `AdbcStatement.ExecuteQuery` (sync), `frames.ToDataSet().WriteToDuckDB`. IN: 56 (Bim), 62 (py Arrow), BIM:81 GeoParquet (ADD). OUT: → federation execution lane.
- `cypher.md` [KEEP→improve] — SOUND (injection-safe `format('%L')`). RENAME `GraphFault`→`CypherFault` 836x. `AgtypePath.Weight`-always-0.0 dead carrier disposes. Consumes provisioning `ServerExtension.CreateSql` FROZEN (V5c). IN: 3 (provisioning ext). OUT: — .
- `cache.md` [KEEP→improve] — SOUND (folded horizon gate, one-row `ArtifactKind` growth law — load-bearing for Compute/Fabrication consumers). Batching note only. IN: 58 (Compute INDEX), cache-L2⇄AppHost (ADD). OUT: — .
- `federation.md` [NEW] — 842x `FederationFault`. The Substrait plan decode → `SetExpr` lowering fold + columnar/ADBC execution lane (§7). `SourceKind` seed DATA (durable-store/signed-artifact/ADBC-warehouse) with a Substrait-capable capability axis. Cut-pinned content-addressed receipt (`plan digest`=`ContentHash.Of` over wire bytes, cut, watermark). Signature-locked python:data wire. IN: 57b (Substrait plan — CORRECTED home). OUT: → retrieval + columnar (lowering).

**Ingest/ (2 — leg 3; folder home: codec-ingress axis, ends the one-file-folder violation)**

- `tabular.md` [KEEP→improve] — SOUND (`TabularSpec` one discriminant, `TabularFault.Lift` funnel). `TabularFault` RE-BANDS to 838x. Sep made EXPLICIT owner law (the SIMD delimited lane — fenced, not prose). `TabularWire.Wire` dead carrier: compose the `CustomFormatter` into `Policy().DynamicColumns` or delete (STJ `Bind<T>` is the wired path). IN: — . OUT: 61 (Rasm.Element row shape).
- `schedule.md` [NEW] — 843x `ScheduleFault`. The `MPXJ.Net` schedule-file codec (.mpp/XER/PMXML → the record rail) + durable schedule rows — the Persistence half of the relocated Bim schedule domain (the file codec + durable store is ingest-shaped; the app root owns the schedule→element map). MINTS (V11 deciding criterion met — BIM:102 P6/MS-Project consumer confirmed). IN: Bim/schedule wire (ADD, counterpart BIM:102). OUT: durable rows.

**Store/ (3 — leg 4; folder home: geometry object store + server tier + coordination — a real 3-page growth axis)**

- `blobstore.md` [KEEP→improve] — SOUND (nine-delegate `ObjectLeg`, 412-noop seal, WORM defense). REPAIRS: `MultipartTransfer.Upload`+`BlobTransferReceipt` become THE composed receipt path (callers route through) or both die; `BlobResidence.Correlation` populate-or-drop; **GCS/Minio checksum rows read the honest SDK-native stance (Crc64/None), not the decorative `XxHash128`** (E9 half-fix — only `S3Leg.Seal` supplies `Integrity.Wire`). IN: 59 (Compute GLB), 60 (Bim IFC/BREP), Placement→GeometrySource (ADD). OUT: `StorageTier` frozen to retention.
- `provisioning.md` [KEEP→improve] — SOUND (one six-command verification fold, `FailureRank.Absorb`, embedded residency ritual). `ServerFault` RE-BANDS 835x→839x; ALL 7 loose `Error.New` sites become typed 839x cases; **`EmbeddedStore.Refused` 7701/7702 rail `EmbeddedFault.Refused`** (the fourth/fifth breach the E4 census omitted). The `Npgsql.OpenTelemetry` observability row names its tracing+metrics subscriptions. IN: 63 (AppHost health, Npgsql-only). OUT: `ServerExtension` frozen to cypher.
- `coordination.md` [NEW] — 841x `CoordinationFault`. The four AppHost PORT contracts on ONE `CoordinationOp [Union]` + the `ONE_OUTBOX_EGRESS_SPINE` (§6). Composes Marten/Npgsql primitives (`FetchForWriting`, `QueueSqlCommand`, advisory locks, `LISTEN`/`NOTIFY`) — never a second event store, never a distributed-lock sidecar. IN: 76/77/78/79 (four PORT counterparts, ADD). OUT: → egress reads the outbox cursor.

---

## [6]-[APPHOST_PORT_HOMING] — the disqualifying gate, all four walked to page + op-union case

Every PORT homes on a Persistence-owned type AppHost's adapter decodes; NO AppHost interface or type crosses down. ONE `CoordinationOp [Union]` on `Store/coordination.md` discriminates all cases; guarded writes carry the holder's monotonic `FencingToken`; the guarded write REJECTS a token older than the row's lease generation via the SAME PG row-CAS predicate that debits (Kleppmann fencing — a fence a resource never checks is decorative).

| PORT (AppHost) | Owning page#anchor | `CoordinationOp` case(s) | Persistence-owned type |
|---|---|---|---|
| APPHOST:76 fenced per-tenant Budget debit (ONE_FENCED_LEASE_STORE) | `Store/coordination#COORDINATION_OP` | `BudgetDebit(TenantId, FencingToken, CostVector)` | fenced predicated compare-and-decrement, **PER-UNIT VECTOR** — the row carries N `CostUnit` balances; ONE `UPDATE … WHERE token >= held AND balance_i >= debit_i` for every requested unit atomically (AppHost's `CostVector`, `capability.md:62`; a scalar debit is falsified by the consumer) → `Fin<BudgetReceipt>`, `CoordinationFault.LeaseFenced` 8411 on a stale token |
| APPHOST:77 workflow step-state CAS (ONE_FENCED_LEASE_STORE) | `Store/coordination#COORDINATION_OP` | `StepStateCas(WorkflowInstance, WorkflowStep, FencingToken)` (write) + `StepStateInFlight(TenantContext)` + `StepStateLoad` (READS) | honors the AppHost `StepStateSeam.InFlight: Func<TenantContext, Fin<Seq<string>>>` (`orchestration.md:271`) the `CrashResume` flagship reads (:282) — a write-only union strands crash-resume; `CasConflict` 8412 |
| APPHOST:78 transactional outbox same-tx (ONE_OUTBOX_EGRESS_SPINE) | `Store/coordination#OUTBOX_SPINE` | `OutboxAdvance(OutboxCursor)` | NAMES the Marten event stream as the outbox (events commit in the same `IDocumentSession` as state — the same-tx guarantee already holds); mints the durable drain `OutboxCursor` + at-least-once advance; the V3 egress pump consumes this cursor; `OutboxDrain` 8414 |
| APPHOST:79 CAS + fenced-lease + membership (ONE_FENCED_LEASE_STORE) | `Store/coordination#COORDINATION_OP` | `LeaseAcquire(LeaseKey, Holder)` + `LeaseRenew` (fenced, monotonic token) + `MembershipUpsert`/`MembershipScan` (lease-expiry) + the expired-lease scan READ | `Fin<FencingToken>`; `MembershipRow` with lease-expiry semantics; `MembershipLapsed` 8413 |

The op-union carries READ cases beside the guarded writes (`StepStateInFlight`, expired-lease scan, `MembershipScan`) — a write-only union forces the AppHost-side table scan the PORT law forbids. V12 prepared-tx disposition: the single-`IDocumentSession` spine mints no `pg_prepared_xacts` in-doubt set, so the AppHost 2PC drain-row criterion RESOLVES RETIRE (APPHOST:71→`graph#STORE_RAIL`) unless a draft proves a prepared-tx owner on `graph#STORE_RAIL` — none demanded.

---

## [7]-[FEDERATION_DISPOSITION] — probe FEASIBLE_WITH_GAPS → REINTRODUCE

The probe OVERTURNS the ruled-default WIRE-FORM premise in the owner's favor; a draft assuming the default (hand-built transcriber + `Grpc.Tools`) LOSES against the probe. Rulings:

- **WIRE FORM (a) — the boundary transcriber is ALREADY SHIPPED PUBLIC.** `FlowtideDotNet.Substrait@0.15.0` ships `Substrait.Protobuf.Plan : IMessage<Plan>` with `public static MessageParser<Plan> Parser` and `public class SubstraitDeserializer` with `Deserialize(Substrait.Protobuf.Plan)` / `Deserialize(string json)` / `DeserializeFromJson(string)` — all decompile-confirmed public. This FALSIFIES `api-flowtide-substrait.md:151`. Transcription collapses to ~2 lines: `Substrait.Protobuf.Plan.Parser.ParseFrom(wireBytes)` then `new SubstraitDeserializer().Deserialize(protoPlan)`. `SubstraitSerializer` (reverse) is internal — favorable: retain wire bytes, never re-serialize.
- **DROP the `Grpc.Tools` codegen admission** — NOT needed and COUNTERPRODUCTIVE: regenerating substrait `.proto` mints a duplicate CLR `Plan` type `SubstraitDeserializer` rejects by identity. `Google.Protobuf@3.35.1` (already admitted, shared tier) is the ONLY runtime dependency, as the `MessageParser.ParseFrom` runtime. Zero new admission.
- **LOWERING** — one `RelationVisitor` subclass (~150-250 LOC) with a `LoweringTarget` discriminant per the probe [03] routing table (reference: Flowtide's own `SubstraitToDifferentialComputeVisitor`, 139 LOC / 9 kinds). `SetRelation`→Union/Intersect/Difference (3-for-3), `ReadRelation`+Filter→`SetExpr.Predicate`, `VirtualTableReadRelation`→`Literal`, bounded `IterationRelation`→`Closure`, key-semijoin `Join`→`Intersect`. No-SetExpr-target ops (`Project`/`Aggregate`/`Sort`/`TopN`/`Fetch`/window/general join) ROUTE to columnar; `WriteRelation` REJECTS (read-only, `FederationFault.WriteRejected`); `ExchangeRelation` DROPS.
- **ONE-ENGINE TEST — retire criterion NOT triggered.** A total lowering onto `SetExpr` alone is correctly impossible (SetExpr is a key-set algebra), but the two-target partition (SetExpr for key-selection; the EXISTING in-process DuckDB + external-warehouse ADBC lanes for tabular) mints NO new engine — the owner is a router/lowerer over standing lanes.
- **PINNED EXECUTION (b)** — every lowered plan executes against ONE `TimeCut` (lane watermark head) and returns the `ElementSet` receipt + Arrow batch stream; the receipt records the (plan digest = `ContentHash.Of` over the received wire bytes, cut, watermark) triple; replay is one `ArtifactKind` reuse row (`cache.md:20`).
- **G3 (the one real gap) — tabular-subtree Substrait EXECUTION support.** ADD the DuckDB `substrait` community-extension row (`ColumnarExtension.Substrait`, extension row not NuGet) for the in-process lane (`from_substrait(blob)`, MIT v1.2.2) — VERIFY a build exists for the pinned `DuckDB.NET.Data.Full` 1.5.x and FAIL-CLOSED (typed `FederationFault.UnsupportedRelation`) on an unsupported relation. External ADBC: scope committed tabular federation to Substrait-capable sources (in-process DuckDB, Flight-SQL/DataFusion endpoints); stage SQL-only warehouses (the admitted BigQuery Beta driver is SQL-backend) as a growth row via `AdbcStatement.SqlQuery` — the producer emits SQL for that `SourceKind`. No second engine.
- **ROSTER** (§3.3): KEEP `FlowtideDotNet.Substrait` + `Apache.Arrow.Adbc`(.Drivers.{Apache,BigQuery}) + `Apache.Arrow.Flight`. **G1**: correct `api-flowtide-substrait.md:151` (protobuf round-trip IS public). **G5**: state the receipt semantics — the pinned cut is the LOCAL coordinate; external data currency is read-time.
- **CARDS**: the three BLOCKED cards (`REUSE_WIRE`, `SUBSTRAIT_FEDERATION_SEAM`, `ARTIFACT_CONTENT_KEY_FEDERATION`) re-anchor to `Query/federation` with honest status (§4).

---

## [8]-[VERDICT_/_EVIDENCE_/_DELTA_DISPOSITION]

### [8.1] Verdicts (13)

| V | Ruling |
|---|---|
| V1 | REINTRODUCE `Query/federation.md` (§7) — retire NOT triggered |
| V2 | NEW `Store/coordination.md` — one `CoordinationOp` union, token-VALIDATING fenced lease, per-unit-vector Budget CAS, READ cases, named outbox spine (§6) |
| V3 | NEW `Version/egress.md` — one pump, exactly-once-effect CloudEvents envelope, sink rows (§5.2 egress) |
| V4 | ONE `FaultBand` registry on `graph#FAULT_TABLES` — 19 own + 6 mirrors; **CommitFault MINTED (H1), not "register as-is"**; 835x=8350-8356, 837x three-way, 7701/7702 corrected (§1) |
| V5 | (a) authority extracted; (b) retrieval extracted; (c) `retention/recovery/cypher` consume blobstore/provisioning FROZEN vocabulary; (d) graph consumes timetravel `TimeCut` FROZEN — a leg-4 change reopens the consuming leg as hard residual (§5.2) |
| V6 | COMMIT EF read-projection; migration owner on identity; embedded charter stated; pgvector server-side ANN on retrieval (§3.2, §5.2 identity/retrieval) |
| V7 | roster motion as a verdict — 12 PRUNE on named proofs (§3.1); csproj inversion (§3.4) |
| V8 | (a) consumer contract RECORDED for the Materials/Element campaign (exclude Classifications from the Type seed — coupled: `element.md:295` writes Classifications unconditionally today); (b) `merge` Type-correlation key row NOW (§5.2 merge) |
| V9 | incremental `OfGraph(prior,delta)` + parametric-digest + streaming-identity as consumer contracts; the geometry-`[V2]` parametric digest folds INTO `ToCanonicalBytes` (waterfalled, Persistence the demanding consumer) — whole-graph recompute documented interim (§1.3, §5.2 codec/timetravel) |
| V10 | verified-defect set lands with each page's rebuild (§5.2): recovery RPO ×2, MergeBase, ledger Truncate/Codec/batch, merge STJ, blobstore Upload/Correlation/checksum, retention StorageLane, provenance AgentClass, columnar trust gates, codec HashPolicy, timetravel AggregateStreamToLastKnown |
| V11 | `Ingest/` grows to 2 — tabular (Sep explicit) + schedule MINTS (§5.2 tabular/schedule) |
| V12 | governance machine-checkably true — cards (§4), ledger both directions (§2), catalogs live-anchored (§3.5), registry type-enforced (§1), 12 counterpart obligations (§2.3) |
| V13 | store axis parameterized — provider rows on closed axes; SoR-spine seal SINGULAR (one event store/materializer/identity/changefeed); scale-out backends PRUNE on redundancy proofs, re-admissible via V13 axis; DuckLake forward candidate recorded (§3.1) |

### [8.2] Evidence (14)

| E | Disposition |
|---|---|
| E1 | phantom-realization dies (§4) — 3 open cards corrected + 6 stale `-[COMPLETE]` re-pointed; the `:44`↔`:45` contradiction resolves (SchemaDdl.Sql → V6 migration owner); IDEAS:57-61 + TASKLOG:46-47 (un-enumerated extras) disposed |
| E2 | orphan roster disposed (§3.1-3.3); Ara3D bump 1.0.1→1.6.1 (feed-refuted "latest") |
| E3 | catalog drift (§3.5) — 21 anchor-less (not 16), messagepack byte-dup deleted, re-anchor map applied |
| E4 | band collisions (§1) — corrected: 835x=8350-8356, 837x three-way, provisioning anchors, 7701/7702 undercount; `GraphFault`×2 → `CypherFault` |
| E5 | coordination gap (§6) — bilateral, four PORTs homed, per-unit-vector Budget + InFlight READ hard demands met |
| E6 | seam-ledger (§2) — `ARCH:55` retargeted `Version/merge`, `:50` AsOfKey minted, `:57` split; 3 wired-undeclared added; 12 counterparts |
| E7 | EF stack (§3.2, §5.2 identity) — COMMIT + migration owner; NTS bridges PRUNE; embedded charter stated |
| E8 | verified bugs (§5.2) — recovery ×2, MergeBase, ledger batch, merge STJ; MemberPath VERIFIED clean; **commits H1 added** |
| E9 | dead carriers (§5.2) — blobstore Upload/Correlation/checksum, ledger Truncate/Codec, retention StorageLane, timetravel AggregateStreamToLastKnown, codec HashPolicy; **+VectorOrder/Order + ContentParityCorpus.Seed (H2/C4 extras) + cypher AgtypePath.Weight + topology TypedEdge + tabular Wire** |
| E10 | Type-rekey (§5.2 merge, §8.1 V8) — `[V8]`b row + `[V8]`a coupling recorded |
| E11 | OfGraph hot paths (§1.3, §5.2 timetravel) — incremental contract; overload-split REFUTED (both real, intentional id-inclusive/exclusive) |
| E12 | mandate 1+2 clean (§4 Fabrication card) — count-prefix ZERO-structural; 2701-2710 forward constraint; no node-canonical golden pinned |
| E13 | folder/page overload (§5) — identity/lane split; Ingest 2-page; frozen-contract cross-leg edges (§8.1 V5) |
| E14 | parameterization (§5.2 columnar) — trust gates (incl. the register-mislabeled `{name}`/`{into}`/`{projection}`/`{stamp}` sites), posture constant; provenance AgentClass; provisioning/README H3Postgis superset benign |

### [8.3] [03] escalation deltas — all 12 planes carry a page skeleton reaching the target grade (§5.2). New planes: Query/retrieval 9.5, Query/federation 9, Ingest 9, Store/coordination 9.5, Changefeed egress 9, Governance perimeter 9.5.

---

## [9]-[LEG_PARTITION] — proven acyclic

Order 1→2→3→4; within a leg the listed order IS dependency order.

1. **SPINE + PERIMETER** — the `FaultBand` registry COMPLETE (all 19 own bands + 6 mirrors reserved, so a duplicate integer is unrepresentable from leg 1); `Element/` (authority extract, EF commit + migration owner, codec consumer-contracts + kernel-hash re-anchor + HashPolicy); the FULL roster reconciliation + catalog re-anchor/dedup/stub + **csproj inversion**; the own-ledger seam corrections.
2. **VERSION ENGINE** — ledger/commits/timetravel/merge/provenance/retention/recovery repairs; **CommitFault MINT**; `AsOfKey`; `CRDT_OP_SET` pin; `[V8]` Type-correlation; `[V9]` Scrub/Bisect incremental; the **version-engine hasher re-anchor (§1.3)**.
3. **QUERY + INGEST** — retrieval extraction (`RetrievalFault`); columnar trust gates + `ColumnarExtension.Substrait`; the `[V1]` federation owner (lowers onto retrieval/columnar, which rebuild FIRST in-leg); cache/cypher/topology cold passes (`CypherFault` rename, `Lca` DAG-gate); `Ingest/` growth (tabular Sep-explicit + schedule mint).
4. **STORE + COORDINATION + EGRESS** — blobstore repairs; provisioning loose-code typing + `ServerFault` re-band + `EmbeddedStore.Refused` fix; `Store/coordination.md` (lands BEFORE egress in-leg); the egress owner over the leg-2 changefeed + the leg-4 outbox cursor.

ACYCLICITY PROOF: zero upward strata edges (Persistence stays APP-PLATFORM, up-only on `Rasm` + `Rasm.Element`; the csproj AppHost inversion removes the only build-graph reversal). The four backward cross-leg edges — graph(1)←timetravel(2) `TimeCut`, retention(2)←blobstore(4) `StorageTier`, recovery(2)←blobstore(4) `ObjectStore.Head`, cypher(3)←provisioning(4) `ServerExtension.CreateSql` — are FROZEN-VOCABULARY consumptions: the shape is DECISION-fixed (a constant both legs reference), not runtime-produced, so no cycle; a leg-4 rebuild may not mutate a frozen vocabulary (a change reopens the consuming leg as a hard residual). Intra-leg edges are ordered WITHIN the leg (retrieval/columnar before federation in leg 3; coordination before egress in leg 4; the `egress→coordination` cursor edge has no reverse — coordination never reads the pump). The band registry (leg 1) is the universal upstream every later union derives from. Acyclic. ∎

Campaign acceptance (post-leg-4, composed contracts not the ledger): (a) version-engine dry-run — commit → three-way merge across a Type re-key reading as RENAME → AS-OF scrub shaped for the incremental contract → attested provenance verify → retention sweep → verified restore whose terminal proof is `reconstructed OfGraph == checkpoint AsOfKey` (`RecoveryFault.VerifyFailed` unreachable) beside a real RPO lag; (b) coordination dry-run — a fenced Budget debit whose stale-token replay is REJECTED (`LeaseFenced`, proving validated-not-issued), a step-state CAS, an outbox drain through the egress pump to a CloudEvents sink where a redelivered event dedups by `id`=`ContentKey`; (c) federation dry-run — a Substrait plan lowered to `SetExpr` executing on the columnar lane; (d) perimeter audit — every fault code → exactly one registry row, every digest mint → the kernel `ContentHash.Of` entry or the seam `ContentAddress`, every catalog anchor → a live page, every card anchor → disk, `dotnet restore` clean.
