# [DOSSIER — API + MANIFESTS LANE] Rasm.Persistence

Lane: `api-manifests`. Scope surveyed COMPLETE: 77 package catalogs (`libs/csharp/Rasm.Persistence/.api/`), 31 shared substrate catalogs (`libs/csharp/.api/` — brief says 30), `Directory.Packages.props` Persistence block (`:249-316`), `Rasm.Persistence.csproj`. Register rows re-verified on disk: E2, E3, every `[ROSTER_RECONCILIATION]` disposition. Phantom-member law proven for every verify-or-die member in this lane. Feed truth pulled via nuget MCP where the brief cites versions.

Stance outcome: the two register rows in this lane are **REAL DEFECTS, CONFIRMED** — the perimeter is exactly as rotten as the verdict claims, and in two places WORSE than the register states. The orphan roster and catalog drift are not merely pointers; disk is more damning than the brief on anchor counts, and one roster freshness claim is now falsified by the live feed.

---

## [1] REGISTER VERDICTS (this lane)

### E2 — Orphaned roster → **DRIFT** (defect real and CONFIRMED; PROPS anchors uniformly -2; one sub-claim REFUTED)

The core claim is DECISIVELY TRUE. Positive-control grep proves the harness (`Marten`=137, `ContentAddress`=176, `ElementGraph`=80 hits across the 18 pages). Against that live harness, **every orphan token returns 0 structural hits** in the 18 live pages:

`Confluent`/`IProducer`/`Kafka`/`SchemaRegistry`/`Chr.Avro`/`CloudEvent`/`CloudNative`/`Nats`/`JetStream`/`RabbitMq`/`Pulsar`/`ClickHouse`/`Scylla|Cassandra|Cql`/`Qdrant`/`DeltaLake`/`RocksDb`/`Lightning|Lmdb`/`Substrait|Flowtide`/`FederatedPlan|FederatedEntity|PlanNode|SourceKind`/`Pollination`/`StackExchange|Redis`/`GeoJson|GeoPackage`/`Mpxj`/`Adbc`/`Arrow…Flight`/`ScyllaDBCSharpDriver` — **ALL 0**. ~30 of ~90 admitted packages have zero live-page consumer. HOLDS.

PROPS anchor DRIFT (manifest shifted -2 since brief authored; corrected anchors):

| Package | Brief E2 anchor | Disk anchor | Version |
|---|---|---|---|
| `ClickHouse.Driver` | PROPS:267 | **PROPS:265** | 1.2.0 |
| `DeltaLake.Net` | PROPS:276 | **PROPS:274** | 0.32.0 |
| `FlowtideDotNet.Substrait` | PROPS:281 | **PROPS:279** | 0.15.0 |
| `LightningDB` | PROPS:285 | **PROPS:283** | 0.21.0 |
| `PollinationSDK` | PROPS:307 | **PROPS:305** | 1.10.0 |
| `Qdrant.Client` | PROPS:308 | **PROPS:306** | 1.18.1 |
| `rocksdb` | PROPS:310 | **PROPS:308** | 10.10.1.1747 |
| `ScyllaDBCSharpDriver` | PROPS:311 | **PROPS:309** | 3.22.0.3 |
| `StackExchange.Redis` | PROPS:314 | **PROPS:312** | 3.0.7 |

Streaming cluster: brief `PROPS:264-277,297,309` → disk `262-275` (Chr.Avro 262-264, CloudEvents 266-268, Confluent 269-273, DotPulsar 275) + NATS **295** + RabbitMQ **307**. Same -2.

E2 sub-anchor **DEBUG-IL claim mis-located**: brief cites `columnar.md:411` for "Ara3D.BimOpenSchema ships DEBUG-built IL". Disk `columnar.md:410` is the live `FlatTableProjection` egress (the consumer). The DEBUG-IL fact lives in the CATALOG: `api-ara3d-bimopenschema.md:17,29,135` — "the published `1.0.1` assembly is a DEBUG build (`AssemblyConfiguration("Debug")`, `DisableOptimizations | EnableEditAndContinue`)". Fact real; anchor wrong.

E2 sub-claim **REFUTED by live feed** (see §5): "Ara3D.BimOpenSchema … 1.0.1 is the latest release" is FALSE — nuget.org latest is **1.6.1** (published 2026-07-03); manifest pins stale DEBUG-built 1.0.1.

### E3 — Catalog drift → **DRIFT** (every cited example HOLDS; disk WORSE — 21 anchor-less not 16; messagepack dup byte-identity CONFIRMED)

Every specific E3 catalog-drift example CONFIRMS on disk (exact):

| Catalog | E3 dead anchor | Disk lines |
|---|---|---|
| `api-duckdb` | `Store/profiles` | :6, :176, :253 |
| `api-arrow` | `Query/rail`+`Sync/egress` | :301 |
| `api-cbor` | `Sync/egress` | :123 (+`Schema/identity`:121) |
| `api-zstd` | `Version/snapshots` | :151 |
| `api-lz4` | `Version/snapshots` | :99 |
| `api-nodatime` | `Version/snapshots` | :174 |
| `api-thinktecture-serialization` | `Version/snapshots` | :116 |
| `api-messagepack-analyzer` | `Version/snapshots` | :4, :102 |
| `api-highperformance` | `Store/remote` | :3, :18, :70, :119-139 |
| `api-h3`/`api-sep`/`api-pgvectorscale`/`api-timescaledb` | `Query/lanes` (plural) | h3:140, sep:11/174, pgvectorscale:30, timescaledb:10/25/97 |
| `api-pg-search` | `Store/server` | :6 |
| `api-apache-age`/`api-pgrouting`/`api-pg-graphql` | `Query/federation` | apache-age:7/21/89/121/138, pgrouting:24/126, pg-graphql:9/21 |

**messagepack byte-identical duplicate CONFIRMED**: `shasum` identical (`5527490f…`) across `libs/csharp/.api/api-messagepack.md` and the package copy; the shared-tier file is the ONLY shared catalog tagged `RASM_PERSISTENCE` (`rg -l RASM_PERSISTENCE ../.api/` → 1 hit). Mis-filed and mis-scoped, exactly as E3 states.

**Anchor-less count DRIFT — brief says 16, disk shows 21** (whole-file, case-insensitive `(Element|Version|Query|Ingest|Store|Sync|Schema)/…` = zero):
`adbc-apache, adbc-bigquery, ara3d-bimopenschema, chr-avro-binary, chr-avro-confluent, chr-avro, ef-naming, hybrid-cache, marten, nodatime-stj, npgsql-ef-nodatime, npgsql-ef, npgsql-nts, npgsql-otel, npgsql, nts-ef, nts-io, objectstore, redis, scylladb, thinktecture-json`. The core kept packages `api-marten`, `api-npgsql`, `api-objectstore` — the event-store, the driver, and the blobstore spine — carry NO page anchor at all; anchoring them (graph/provisioning, provisioning, blobstore) is a first-class leg-1 obligation, not filler.

**Dead-page vocabulary the catalogs still cite** (the complete re-anchor map the DECISION owes): `Store/{profiles,remote,encryption,lifecycle,quality,server,tenancy}` · `Query/{federation,pipeline,transaction,rail,lanes(plural)}` · `Version/snapshots` · whole deleted folders `Sync/{egress,coordination,schedule}` and `Schema/{identity,converters,migration}`. Ruled re-anchor targets, cross-checked against the live pages and the V-verdicts:

| Dead token | Live re-anchor (evidence) |
|---|---|
| `Store/profiles` | `Store/provisioning` (`StoreProfile` `[SmartEnum<string>]` lives there, provisioning.md:13,37) |
| `Store/remote` | `Store/blobstore` |
| `Store/encryption` | `Element/identity` (folded, TASKLOG:39) |
| `Store/quality` | `Query/columnar` + `Store/provisioning` verification folds |
| `Store/server` | `Store/provisioning` |
| `Store/tenancy` | `Element/identity` (Tenant-RLS) |
| `Query/rail` / `Query/lanes` | `Query/lane` (singular) |
| `Query/transaction` | `Element/graph#STORE_RAIL` (fold, TASKLOG:37) |
| `Query/federation` / `Query/pipeline` | `Query/federation.md` (V1 REINTRODUCE) or ElementSet+cache on retire |
| `Version/snapshots` | `Version/timetravel` (icechunk fold, TASKLOG:48) |
| `Sync/egress` | `Version/egress.md` (V3 NEW) |
| `Sync/coordination` | `Store/coordination.md` (V2 NEW) |
| `Sync/schedule` | `Ingest/schedule.md` (V11 NEW) or fold-up |
| `Schema/{identity,converters,migration}` | `Element/{codec,identity}` + the V6 migration owner |

---

## [2] ROSTER_RECONCILIATION DISPOSITIONS (each a row)

| # | Disposition | Verdict | Evidence |
|---|---|---|---|
| R1 | Scale-out tier (ClickHouse/Scylla/Qdrant/DeltaLake/rocksdb/LightningDB) INTEGRATION-FIRST re-dispose under V13 | **HOLD** | 0 page consumers each; sealed two-engine sweep confirmed provisioning.md:29 "a third engine is the deleted form", :13/:37 `StoreProfile` server/embedded |
| R2 | PRUNE `PollinationSDK` (charter misplacement) | **HOLD** | PROPS:305, csproj:119; 0 page hits; catalog `api-pollination-sdk.md`→dead `Store/remote` only |
| R3 | PRUNE 4 NTS bridges (`Npgsql.EFCore.PostgreSQL.NetTopologySuite`, `Npgsql.NetTopologySuite`, `NetTopologySuite.IO.GeoJSON4STJ`, `NetTopologySuite.IO.GeoPackage`) | **HOLD** | csproj:36,37,42,44; `GeoJson\|GeoPackage`=0 page hits; H3 rides `pocketken.H3` (kept, PROPS:140) |
| R4 | PRUNE `Microsoft.EntityFrameworkCore.Sqlite` (embedded floor is raw ADO) | **HOLD** | csproj:35; catalog `api-ef-sqlite`→dead `Store/profiles`; provisioning EMBEDDED_FLOOR is raw-ADO by design |
| R5 | PRUNE `StackExchange.Redis` (direct); KEEP `Microsoft.Extensions.Caching.StackExchangeRedis` | **HOLD** | csproj:68 direct vs csproj:66 wrapper; `CacheL2Store`=`IBufferDistributedCache` over Marten (cache.md:177-179), not raw Redis; direct driver 0 page hits |
| R6 | Streaming/messaging cluster INTEGRATION-FIRST as V3 sink rows | **HOLD** | 14 pkgs, all 0 page hits; catalogs anchor to dead `Sync/egress`/`Sync/coordination` |
| R7 | `FlowtideDotNet.Substrait`+ADBC(Apache/BigQuery)+`Apache.Arrow.Flight` ride V1 | **HOLD** | 0 page hits; `api-flowtide-substrait.md:151` confirms NO public `Serialize()`/`Deserialize()` (V1 wire-form premise true) |
| R8 | COMMIT `EFCore.NamingConventions`/`Microsoft.EntityFrameworkCore.Design`/`Thinktecture.…EntityFrameworkCore10` (V6) | **HOLD** | all present PROPS:277,289,314; catalogs anchor to dead `Schema/migration`/`Schema/converters` — commit target is the V6 migration owner |
| R9 | `MPXJ.Net` rides V11 (`Ingest/schedule.md` or leaves) | **HOLD** | csproj:109; `api-mpxj`→dead `Sync/schedule`; 0 page hits |
| R10 | KEEP `Ara3D.BimOpenSchema`(+`.IO`) on live columnar consumer; DEBUG-IL upstream fix demanded | **DRIFT** | KEEP holds (columnar.md:405-490 live); but "1.0.1 latest" **REFUTED** — feed latest **1.6.1** (2026-07-03), manifest pins DEBUG 1.0.1. The DEBUG-IL caveat (catalog :17/29/135) is pinned to a SUPERSEDED release; DECISION must bump 1.0.1→1.6.1 and re-verify DEBUG status |
| R11 | KEEP `Npgsql.OpenTelemetry` (0 consumers; earns provisioning observability row) | **HOLD** | PROPS:301, csproj:45; catalog `api-npgsql-otel` anchor-less; ruled default KEEP |
| R12 | `linq2db.EntityFrameworkCore` CONDITIONAL (BulkCopy prose ref → composed fence or leaves) | **HOLD** | tabular.md:5,20 prose ref present; catalog anchors to dead `Query/lanes`+`Schema/converters`+`Schema/identity` — prose only, not composed |
| R13 | Delete/re-scope misfiled shared `api-messagepack.md`; correct `api-h3.md:3` NTS-IO claim | **HOLD** | dup byte-identical; `api-h3.md:3` DOES claim composing `NetTopologySuite.IO.GeoJSON4STJ` (pruned) → must correct to transitive core-NTS 2.6.0 |
| R14 | Land the ONE substrate `api-languageext.md` (track-shared, "absent") | **REFUTED** | Already PRESENT: `libs/csharp/.api/api-languageext.md`, 21,804 B, tagged `RASM_API_LANGUAGEEXT`, substantive. Obligation DISCHARGED — later campaigns verify, never re-author |

---

## [3] PHANTOM-MEMBER LAW (verify-or-die members in this lane)

Every SEAM_AND_RAIL_LAW / [04] member reachable from the catalog tier PROVEN present (assay unavailable in-lane; verified against the `.api` catalogs + nuget MCP + upstream, per the fallback rail):

| Member | Brief anchor | Verdict | Disk |
|---|---|---|---|
| `AWSSDK.S3` `ChecksumAlgorithm.XXHASH128` / `ChecksumType.FULL_OBJECT` | api-objectstore:58-59 | **HOLD** | rows [21] `XXHASH128` (+CRC64NVME/SHA256/CRC32/CRC32C), [22] `FULL_OBJECT` vs `COMPOSITE`. `CompleteMultipartUploadRequest.ChecksumXXHASH128` not independently catalogued — assay is the open confirm the brief already names |
| CloudEvents `Sequence` (String) / `Partitioning.partitionkey` | api-cloudevents:62 | **HOLD** | row [10] `Sequence` `sequence`/`sequencetype` (String); row [08] `Partitioning` `partitionkey`+`Set/GetPartitionKey` |
| NATS `Nats-Msg-Id` header / JetStream publish-ack Settle | api-nats:44,94 | **HOLD** | row [07] `NatsHeaders` = "`Nats-Msg-Id` carrier"; JETSTREAM "publish-ack … `DeliveryGuarantee.Settle`" |
| pg_net `net.http_post`→bigint / `net._http_response` UNLOGGED / `net.request_status` | api-pg-net:35,44,50-53 | **HOLD** | row [02] `http_post`→`bigint`; §RESPONSE_MODEL `net._http_response` UNLOGGED; row [03] `request_status` PENDING/SUCCESS/ERROR |
| Flowtide no public `Serialize()`/`Deserialize()`; `SqlPlanBuilder`/`RelationVisitor`/`ExpressionVisitor`/`SubstraitToDifferentialCompute.Convert`/`PlanModifier` | api-flowtide-substrait:151 | **HOLD** | :151 "protobuf round-trip … internal … not a public `Serialize()`/`Deserialize()` call"; visitor base "throws `NotImplementedException` for unhandled node kinds" — the transcription-weight risk the V1 retire-criterion flags is REAL |
| Marten `FetchForWriting` / `QueueSqlCommand` / `Snapshot<T>(SnapshotLifecycle.Inline)` | api-marten:30,70,83-85,103 | **HOLD** | `FetchForWriting`:30/83/85/183; `QueueSqlCommand` present; `SnapshotLifecycle` enum `Inline`/`Async`:103, `Snapshot<T>(SnapshotLifecycle)`:198 |
| Npgsql advisory-lock/LISTEN/NOTIFY catalog gap; `Npgsql.Replication` pgoutput recorded-unconsumed | api-npgsql (gap), :86-95 | **HOLD** | `advisory\|LISTEN\|NOTIFY` = 0 hits in `api-npgsql.md` → CATALOG GAP CONFIRMED (leg-1 must add these rows for V2 fenced lease + V3 pump wake); `Npgsql.Replication`/pgoutput catalogued :83-104, unconsumed |

No phantom member found in-lane: every catalogued member the DECISION leans on exists in the catalog. The one open external confirm (`CompleteMultipartUploadRequest.ChecksumXXHASH128`) routes to `assay api` as the brief already directs — not a phantom, an unresolved confirm.

---

## [4] CROSS-CUTTING PERIMETER DEFECTS (beyond register)

- **Provenance-tag law almost entirely UNMET.** Only **4 of 77** package-tier catalogs carry the `<!-- catalog:Pkg@ver -->` tag (`api-clickhouse`, `api-deltalake`, `api-dotpulsar`, `api-flowtide-substrait` — 3 of which are removal/conditional candidates); **0 of 31** shared-tier. The `[ROSTER_RECONCILIATION]` "uniform provenance tag + STACKING section across the kept tier" is a 73/77 + 31/31 open obligation, not a touch-up. The tag SHOULD carry pinned version so a stale catalog is machine-detectable (see next).
- **Version pins live ONLY in the tag on 4 files; the other 73 embed version in prose** (`- version: 1.5.3`), so a manifest bump silently desyncs every untagged catalog. The uniform tag must be the single version-echo the DECISION reconciles against on every roster motion.
- **9 double-cataloged overlays** (`hashing, highperformance, hybrid-cache, jsonpatch, messagepack, nodatime-stj, nodatime, redaction, thinktecture-json`): 8 DIVERGE between tiers (e.g. `highperformance` pkg=142/shared=199 LOC, `hybrid-cache` pkg=161/shared=91, `nodatime-stj` pkg=200/shared=105) — two independently-authored catalogs for ONE substrate package, a maintenance fork. Only `messagepack` is byte-identical. Reconciliation = the shared tier OWNS the substrate package; the package-tier copy deletes (or thins to a one-line overlay pointer at the shared owner). The DIVERGENCE means content must be UNIONED into the shared owner before deleting the package copy, not blind-deleted.
- **Shared-tier count DRIFT**: brief "30" → disk **31**. Recently-landed shared catalogs (mtime ≤1 day: `api-languageext`, `api-mapperly`, `api-mathnet-numerics`, `api-mathnet-providers`, `api-quikgraph`, `api-thinktecture-runtime-extensions`) postdate the brief; the LanguageExt obligation (R14) is among them and is DISCHARGED.
- **`api-h3.md:3` composes a to-be-pruned bridge**: explicitly names `NetTopologySuite.IO.GeoJSON4STJ` (R3 PRUNE target). After the prune the claim dangles — correct to the transitive core `NetTopologySuite` 2.6.0 crossing (`pocketken.H3` survives, PROPS:140).
- **csproj carries a `Rasm.AppHost` ProjectReference** (`Rasm.Persistence.csproj:10`) — Persistence → AppHost is a DOWN/PEER edge. The PLACEMENT_LAW says AppHost is a PORT peer Persistence contributes rows to and "never reverses the dependency". A hard ProjectReference onto AppHost is exactly that reversal at the build graph. If the four AppHost PORT contracts (V2) are Persistence-owned types AppHost decodes, this reference should INVERT (AppHost→Persistence) or the shared vocabulary lands in the kernel/Element seam. Flag for the strata-acyclicity judge gate — this is a build-graph fact, not prose.

---

## [5] FEED TRUTH (nuget MCP, live)

- `Ara3D.BimOpenSchema`: **latest 1.6.1, published 2026-07-03**; manifest/csproj pin **1.0.1**. Brief's "1.0.1 is the latest release" REFUTED. Six minor releases of upstream motion the roster is blind to — and the entire DEBUG-IL upstream-fix demand (E2, [04]) is predicated on 1.0.1 being current. DECISION action: bump to 1.6.1, re-decompile for `AssemblyConfiguration("Debug")`, and either retire the DEBUG-IL caveat (if fixed) or escalate (if it persists across 6 releases → in-corpus absorption candidate).
- `FastCDC.Net`: **latest 1.0.1, published 2022-12-09**. Brief's "two releases, dormant since 2022-12" CONFIRMED; manifest pin 1.0.1 is current-and-terminal. The recorded in-corpus-absorption escalation stands as the only forward path if the pin ever blocks.

---

## [6] CHARTER-AS-IT-SHOULD-BE (this lane)

- **`Directory.Packages.props` Persistence block**: after R1-R14 the block loses (on named redundancy/charter proofs) the 6 scale-out backends, 4 NTS bridges, EF-Sqlite, direct Redis, PollinationSDK — each with its csproj `PackageReference`, README group row, and `.api` catalog in ONE motion; the streaming cluster + Substrait/ADBC/Flight + MPXJ stay as V3/V1/V11-gated rows (leave only on per-package proof). `Ara3D.BimOpenSchema(.IO)` bumps 1.0.1→1.6.1. The block stays label-grouped, one-line-comment discipline, newest-stable-unpinned except where a resolver floor demands (none new here).
- **`Rasm.Persistence.csproj`**: the AppHost ProjectReference (`:10`) is the one structural anomaly — resolve per the V2 dependency-direction ruling. The label groups (`Store Lanes`, `Embedded KV Engines`, `Streaming And Messaging Protocols`, `Sync Outside-Rhino`) re-emit to match the surviving roster; `Sync Outside-Rhino` loses PollinationSDK (Speckle stays, ledger consumer).
- **Package `.api/` tier (77→post-prune)**: every kept catalog re-anchors to a LIVE page per the §1 map; the 21 anchor-less gain anchors (marten→graph/provisioning, npgsql→provisioning, objectstore→blobstore first); a uniform `<!-- catalog:Pkg@ver -->` tag + `[STACKING]` section lands across the full kept tier (currently 4/77 tagged); the messagepack package-copy deletes.
- **Shared `.api/` tier (31)**: the misfiled `api-messagepack.md` re-scopes `RASM_PERSISTENCE`→neutral `RASM_API` (matching the landed `RASM_API_LANGUAGEEXT` precedent) or deletes; the 8 divergent overlays union into their shared owners and the package copies collapse to pointers. `api-languageext.md` obligation is closed — verify-only.

---

## [7] PER-CATALOG ANCHOR LEDGER (77 package-tier, condensed)

- **LIVE-clean (5)**: `jsonpatch`(Version/ledger), `redaction`(Element/identity), `speckle`(Version/ledger), `sqlite`(Store/provisioning), `sqlitepcl`(Store/provisioning+Element/identity).
- **MIXED (live + dead co-anchor) (16)**: `aws-kms`, `azure-keyvault`, `google-kms`, `ef-sqlite`, `fastcdc`, `h3`, `h3-pg`, `hashing`, `lightningdb`, `minio`, `pg-graphql`, `pg-net`, `pg-search`, `pg-server-bgworkers`, `pgrouting`, `pgvector-ef`, `pgvectorscale`, `timescaledb`, `miniexcel`, `cloudevents`, `deltalake`, `apache-age`, `schemaregistry`(+3 serdes) — each keeps its live anchor(s), sheds the dead co-anchor(s).
- **DEAD-only (drift) (~35)**: `arrow`, `cbor`, `clickhouse`, `dotpulsar`, `duckdb`, `ef-design`, `flowtide-substrait`, `highperformance`, `kafka`, `linq2db-ef`, `lz4`, `messagepack`, `messagepack-analyzer`, `nats`, `nodatime`, `parquetsharp`, `pollination-sdk`, `qdrant`, `rabbitmq`, `rocksdb`, `sep`, `thinktecture-ef`, `thinktecture-serialization`, `zstd`.
- **ANCHOR-LESS (21)**: listed in §1 E3.
