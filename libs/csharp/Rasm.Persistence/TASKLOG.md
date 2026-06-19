# [PERSISTENCE_TASKLOG]

The open and closed work for the durable-state spine, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup.

## [1]-[OPEN]

## [2]-[CLOSED]

[COMPLETE] T-TRACE-CHANGEFEED -- `Sync/collaboration#OPLOG_CHANGEFEED` + `#TRANSPORT_AXIS`, `Version/provenance#LINEAGE_CDC`, `Store/remote#ARTIFACT_SYNC_FEED` own the W3C trace-context envelope slot: the `TraceContext` `[ComplexValueObject]` carrier + `OpLogEntry.Trace`/`CdcEnvelope.Trace` slots, `OpLog.Stamp` captures `Activity.Current` once, `ReplicationPump.Resumed` extract-and-continues the parent span, `Announce` brackets a producer span, and the wire slot is append-only optional — realizing `ONE_DISTRIBUTED_TRACE` with the AppHost `CORRELATION_SPINE` populating the bytes, no second correlation owner.

[COMPLETE] T-GRADUATION-IMPORT -- `Version/provenance#ATTESTED_LEDGER` + `Query/cache#ARTIFACT_BLOB_INDEX` own the `ONE_GRADUATION_EVIDENCE` inward seam: the `GraduationAsset` kind const content-addresses the ONNX surrogate, and the `AttestedEntry.ProviderFingerprint` dedicated `Option<UInt128>` binds the `(checksum, OrtEpDevice)` term into the digest tuple extended symmetrically in `Append`/`Recompute` over a `stackalloc byte[80]` as a forward-only format epoch — never the live `RedactedPayloadProof` slot, riding the existing object-store residence + `ClosureGc` sweep.

[COMPLETE] T-REDIS-FABRIC -- `Query/cache#L2_CONTRIBUTION` owns the live distributed-coordination fabric: the `RedisFabric` delegate row on the `CacheResidence.Redis` `Register` arm carries the RESP3 `__redis__:invalidate` push, the keyspace-notification stream feeding the op-log HLC cursor, and the atomic Lua single-flight/writer-lease, plus the `CacheIndexFact.Invalidate` kind — all gated so a Redis-absent profile is bit-identical to the TTL baseline; the `.api/api-redis.md` decompile fold catalogued the RESP3/keyspace/`ScriptEvaluate`/`FUNCTION` members before the fence pinned.

[COMPLETE] T-PARITY-CORPUS -- `Version/commits#CRDT_WIRE` owns the Persistence leg of `ONE_WIRE_FIXTURE_CORPUS`: the `ContentParityCorpus` `ParityVector` table (`hlc.zero`, `commit.genesis`, `crdt.set.seed`, `elementset.empty`, `embedding.seed`) with the VERIFIED `Hlc.WriteTo` Int64LE-ticks-then-UInt64LE-logical shape, the `CommitGraph.Commit` canonical preimage, the seed-zero convention, and the cross-page `Query/federation#ELEMENT_SET_ALGEBRA`/`Query/lanes#SEARCH_LANES` byte shapes cited by reference — the literal `XxHash128` digests host-frozen on the validation pass; a fixture-byte fence, never a `.cs` source file.

[COMPLETE] T-SCHEDULE-CPM -- `Sync/schedule#SCHEDULE_STORE` owns the real CPM/baseline/resource algebra: `CpmPass` runs the forward/backward float pass (early/late start-finish + total/free float, the four `RelationKind` arms through the generated total `Switch` with lag) replacing the longest-task stub, `CriticalPath` is the total-float-zero set, `ScheduleFloat` is the typed slip delta, `ScheduleBaseline.Of` is the content-addressed multi-baseline snapshot, and the `ResourceId`/`ResourceUnits`/`BudgetedCost` loading columns ride `ScheduleTask` — one DAG algebra serving CPM, lineage, and the commit-DAG.

[T-SERVER-EXT-API] [COMPLETE]: Coverage-audit remediation — authored the four missing PG18 server-extension `.api/` catalogues (`pgvectorscale`/`pg-search`/`timescaledb`/`pg-server-bgworkers`) verifying the provisioning/search/document/audit fences that cited them, and added the `[SERVER_EXTENSIONS]` README group; live apply stays `[SERVER_PROVISIONING_PROBE]`.

[T-CRDT-PRESENCE] [COMPLETE]: Landed the sixth replicated type `CrdtField.EphemeralMap` + the `CrdtOp.Beat`/`Leave` delta arms on `Version/commits#CRDT_ALGEBRA` (per-origin LWW-by-HLC `Merge`, add-wins `Apply`, quiescence-horizon `Compact`); the `beat`(8)/`leave`(9) tags ride the one `CrdtOpWire` mint, the divergent `Sync/collaboration` re-mint collapsed into it.

[T-DUCKLAKE] [COMPLETE]: Added `LakehouseCatalog`/`LakehouseKind`/`Lakehouse` to `Query/lanes#ANALYTICAL_LANE` promoting the parquet export to a DuckLake v1.0 SQL-catalog lakehouse (per-backend `ATTACH`, `Snapshot`/`AsOfSql` time-travel via raw `DuckDBCommand` SQL, content-keyed object-store data files); catalog layout holds under `[DUCKLAKE_CATALOG]`.

[T-IDS-RULES] [COMPLETE]: Added the `IdsSpecification`/`IdsFacet`/`IdsImport` importer to `Query/federation#RULE_PLAN` — `Parse` over `System.Xml.Linq`, `Scope` folding the six facets into one `SetExpr`, `Lower` projecting each requirement onto `RuleAst.Requires`/`Forbids` so a published IDS check runs the settled `RulePlan.Evaluate`; grammar holds under `[IDS_FACET_SCHEMA]`.

[T-TRANSPARENCY-PROOF] [COMPLETE]: Added the `TransparencyProof`/`InclusionProof`/`ConsistencyProof`/`HeadSeal` RFC 9162 Merkle owner to `Version/provenance#ATTESTED_LEDGER` (O(log n) inclusion/consistency proofs over `XxHash128` nodes, `ECDsa` head seal, redaction-preserving `LeafDigest`); construction holds under `[MERKLE_PROOF_CONSTRUCTION]`.

[T-UNION-DISPATCH] [COMPLETE]: Converted the lossy-`_`-arm `[Union]` folds (`RuleAst`/`SetExpr`/`PlanNode`/`CrdtOp`) to the generated total `Switch` with `SwitchMethods` so a new case breaks the build; the tuple-`switch` merges and foreign `CrdtOpWire.Map` stay language `switch` per the product/foreign-type exemption.

[T-SPECKLE-BIND] [COMPLETE]: Full-bound the `SyncTransport.SpeckleLikeDiff` case to the `Speckle.Sdk` instance `IOperations.Send`/`Receive` on `Sync/collaboration#TRANSPORT_AXIS` (`rootObjId` onto the one `ContentKey`, the inline `Closure` fork deleted, the inbound graph mapped to op-log entries at the seam); the heavy Speckle closure runs companion/outside-Rhino, faults lift once as `Error.New(8251)`, members verify against `.api/api-speckle.md`.

[T-ARROW-FLIGHT] [COMPLETE]: Bound the Arrow plane to an ADBC + Flight SQL zero-copy egress on `Query/rail#ARROW_EGRESS` (the one carrier schema into a `RecordBatch`, `StoreFlightServer` `DoGet`/`DoExchange`, `AdbcEgress` over `IArrowArrayStream`); the server-side Flight/ADBC member surface is the `[ARROW_FLIGHT_SERVE]` decompile leg, live round-trip external-blocked.

[T-FASTCDC-DEDUP] [COMPLETE]: Replaced fixed-window framing with FastCDC content-defined chunking on `Version/snapshots#CONTENT_CHUNKING` (gear-hash boundary, per-chunk `XxHash128` key, `Novel` projecting the chunks the blob index lacks so identical chunks dedup); the under-churn dedup ratio is the `[CDC_DEDUP_RATIO]` gate.

[T-LAYER-COMPOSE] [DROPPED]: USD/IFC5 layer opinion-composition dropped with its `[LAYERED_COMPOSITION]` idea — duplicated `Version/commits#COMMIT_DAG` branches + `CRDT_ALGEBRA` `MvRegister` + `TimeTravel.BranchFromPast`, a second composition surface breaking the closed-CRDT prohibition.

[T-DOC-MIGRATE] [COMPLETE]: Re-homed the design pages into the single `.planning/<sub-domain>/<page>.md` tree, rebuilt `ARCHITECTURE.md`/`README.md`, authored `IDEAS.md`/`TASKLOG.md`.
