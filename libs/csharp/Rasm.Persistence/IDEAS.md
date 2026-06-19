# [PERSISTENCE_IDEAS]

The forward pool of higher-order concepts for the durable-state spine, each grounded in the folder's domain and current platform capability. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

## [1]-[OPEN]

## [2]-[CLOSED]

[TRACE_CONTEXT_CHANGEFEED]: realized — `sync/collaboration#OPLOG_CHANGEFEED` (the `TraceContext` `[ComplexValueObject]` W3C carrier + `OpLogEntry.Trace` envelope slot + `OpLog.Stamp` ambient-span capture), `sync/collaboration#TRANSPORT_AXIS` (`ReplicationPump.Resumed` extract-and-continue), `provenance#LINEAGE_CDC` (`CdcEnvelope.Trace` threaded through `Feed`), and `stores-remote/object-store#ARTIFACT_SYNC_FEED` (`Announce` producer span) own the `ONE_DISTRIBUTED_TRACE` op-log anchor — trace bytes ride the existing envelope, the AppHost `CORRELATION_SPINE` populates them, the wire slot is append-only optional.

[GRADUATION_LEDGER_IMPORT]: realized — `provenance#ATTESTED_LEDGER` (the `AttestedEntry.ProviderFingerprint` dedicated `Option<UInt128>` + the digest tuple extended in symmetric `Append`/`Recompute` over a `stackalloc byte[80]` as a forward-only format epoch) and `cache/indexes#ARTIFACT_BLOB_INDEX` (the `GraduationAsset` kind const) own the `ONE_GRADUATION_EVIDENCE` inward seam — the content-keyed ONNX surrogate chains into the tamper-evident ledger and rides the existing object-store residence + `ClosureGc` sweep, the fingerprint never the live `RedactedPayloadProof` slot.

[REDIS_LIVE_FABRIC]: realized — `cache/indexes#L2_CONTRIBUTION` (the `CacheContribution` capsule + the `CacheResidence.Redis` `Register` arm carrying the `RedisFabric` delegate row + the `CacheIndexFact.Invalidate` kind) owns the live distributed-coordination fabric — RESP3 `__redis__:invalidate` push, the keyspace-notification stream, and the atomic Lua single-flight/lease, all gated so a Redis-absent profile is bit-identical to the TTL baseline; the `.api/api-redis.md` decompile fold catalogues the RESP3/keyspace/`ScriptEvaluate`/`FUNCTION` members the fence pins.

[CONTENT_PARITY_CORPUS]: realized — `versioning/version-control#CRDT_WIRE` (the `ContentParityCorpus` `ParityVector` table: `hlc.zero`, `commit.genesis`, `crdt.set.seed`, `elementset.empty`, `embedding.seed`) owns the Persistence leg of `ONE_WIRE_FIXTURE_CORPUS` — the byte SHAPE, field order, and seed-zero convention authored now (`Hlc.WriteTo` Int64LE-ticks-then-UInt64LE-logical VERIFIED; cross-page `federation#ELEMENT_SET_ALGEBRA` and `data-lanes#SEARCH_LANES` shapes cited by reference), the literal `XxHash128` digests host-frozen on the validation pass.

[SCHEDULE_BASELINE_CASCADE]: realized — `schedule/schedule-interchange#SCHEDULE_STORE` (the `CpmPass` forward/backward float pass replacing the longest-task stub, `ScheduleFloat` typed delta, `ScheduleBaseline` content-addressed snapshot, and the `ResourceId`/`ResourceUnits`/`BudgetedCost` loading columns on `ScheduleTask`) owns the real CPM algebra — the critical path is the total-float-zero set, the four `RelationKind` arms dispatch through the generated total `Switch` with lag, and one DAG algebra serves CPM, lineage, and the commit-DAG.

[DUCKLAKE_LAKEHOUSE]: [CAPTURED] — `data-lanes#ANALYTICAL_LANE` parquet export promoted to a DuckLake v1.0 SQL-catalog lakehouse (`ATTACH`-per-backend, HLC-drained ACID snapshots through the existing `DuckDBOpLogMap` appender, `AT (VERSION => n)` time-travel); catalog on the PG tier, data on object-store under the one content key, parquet-export the `Available`-false fallback.

[IDS_SPECIFICATION_MODEL]: [CAPTURED] — buildingSMART IDS 1.0 folded into `federation#RULE_PLAN` as data (`Parse`/`Scope`/`Lower` projecting facets and cardinalities into the settled `RuleAst.Requires`/`Forbids`), an unsupported facet a typed rejection, the `IdsConformance` receipt aligned to `annotation#BCF_PROTOCOL`.

[TRANSPARENCY_PROOF]: [CAPTURED] — `provenance#ATTESTED_LEDGER` lifted to an RFC 9162 Merkle transparency log (O(log n) inclusion/consistency proofs over the `AttestedEntry` leaves, `ECDsa` head-seal the sole crypto surface, redaction-preserving leaf, export through `redaction-retention#EXPORT_PROOF`).

[SHORT_TAG_CONSUMER]: [CAPTURED] — `snapshots#COMPRESSION_HASHING` `HashPolicy.Content` (64-bit `XxHash3`) bound as the `#CONTENT_CHUNKING` `ContentChunk.ShortTag` bloom pre-filter ahead of the authoritative 128-bit `ContentKey` compare; dedup identity stays `HashPolicy.Identity`, and the no-use-site boundary prose is corrected.

[ARROW_FLIGHT_EGRESS]: [CAPTURED] — the in-process Arrow plane bound to a zero-copy Flight SQL + ADBC egress at `query-rail#ARROW_EGRESS` (one `#ARROW_PLANE` carrier schema as `RecordBatch`, `FlightServer` serve, `AdbcStatement`/`QueryResult` read-back, standing-query batches over one `DoExchange`); the server-Flight/ADBC member fold is the `[ARROW_FLIGHT_SERVE]` API-pass leg.

[CDC_DEDUP_STORAGE]: [CAPTURED] — fixed-window framing replaced by FastCDC content-defined chunking at `snapshots#CONTENT_CHUNKING` keyed by `XxHash128`, the `cache/indexes#ARTIFACT_BLOB_INDEX` deduping chunks across snapshots and peers; the boundary survives an interior insertion, aligned to `Compute/interchange#GEOMETRY_DELTA` at the rolling-hash level only.

[LAYERED_COMPOSITION]: [DROPPED] — product-anchored (USD) and draft-standard-anchored (IFC5); minted no capability the `versioning#COMMIT_DAG` named branches plus `versioning#CRDT_ALGEBRA` `MvRegister` concurrent-keep and `TimeTravel.BranchFromPast` do not already own. (DO-NOT-RE-ADD.)
