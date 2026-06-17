# [PERSISTENCE_IDEAS]

The forward pool of higher-order concepts for the durable-state spine, each grounded in the folder's domain and current platform capability. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

## [1]-[OPEN]

[DUCKLAKE_LAKEHOUSE]: Promote the analytical lane from parquet-export-only to a DuckLake v1.0 SQL-catalog lakehouse over object-store Parquet.
- Replace the one-directional `data-lanes#ANALYTICAL_LANE` parquet export with a DuckLake catalog whose metadata lives in the existing PostgreSQL server tier and whose data files reside in the `stores-remote` object store, so the changefeed materializes as catalog-tracked Parquet snapshots queryable in place rather than re-exported per read.
- Unlocks ACID snapshots, time-travel, and schema evolution over columnar object-store data with the catalog reusing the same content-key addressing the artifact-blob index already mints; the continuous-aggregate rollups on `provisioning#TIMESCALE_PROVISIONING` gain a durable columnar tier without a second analytical engine, and a federated analytical query reads catalog Parquet through the in-process DuckDB lane.
- Draws on DuckLake v1.0 as the 2025-2026 lakehouse format that keeps catalog metadata in a transactional SQL store rather than a JSON manifest, matching the package's existing PostgreSQL-catalog-plus-object-store split exactly.

[ARROW_FLIGHT_EGRESS]: Extend the in-process Arrow plane to a zero-copy ADBC + Flight SQL egress surface.
- The `query-rail#ARROW_PLANE` carrier is in-process only; bind it to ADBC and Flight SQL so an analytical consumer pulls columnar result batches over the wire without a row-to-object materialization, the Flight stream reusing the same `ArrowCarrier` schema the in-process plane already projects.
- Unlocks high-throughput columnar egress to Python/polars and TypeScript analytical consumers that today decode the row wire, removes the parse-materialize-rebuild hop for large result sets, and lets the standing-query window push incremental Arrow batches to a subscribed client over one Flight channel.
- Draws on Arrow.NET 22 + ADBC 20 as the current zero-copy database-connectivity owners, turning the existing in-process Arrow carrier into a cross-runtime egress contract without minting a second result shape.

[CDC_DEDUP_STORAGE]: Replace the fixed 64-KiB snapshot framing with FastCDC content-defined chunking for cross-snapshot dedup over opaque bytes.
- The snapshot protocol and the blob transfer frame on a fixed 64-KiB window; swap the windowing for a FastCDC opaque-byte rolling-hash boundary so a small edit to a large artifact re-stores only the changed chunks, and key each chunk by its `XxHash128` content address so the artifact-blob index dedups identical chunks across snapshots and peers.
- Unlocks order-of-magnitude storage and transfer savings for the snapshot history and the object-store sync feed, where successive snapshots of an evolving model share most of their bytes; the content-defined boundary survives an insertion that would shift every fixed window, so the dedup ratio holds under edit churn.
- Draws on FastCDC content-defined chunking as the standard rolling-hash boundary technique; the `Compute/interchange#GEOMETRY_DELTA` `DeltaCodec` applies the same technique over geometry-structural columns and stays its own owner, so this idea is the opaque-byte chunker for snapshot/blob frames that aligns with the geometry-structural codec at the technique level, never re-deriving the geometry-aware delta and never importing it as a byte chunker.

[IDS_SPECIFICATION_MODEL]: Lift the buildingSMART IDS document model into the federation rule engine as a first-class specification source.
- The `federation#RULE_PLAN` rule AST today carries clash/MVD/QTO rules authored in the package vocabulary; add an IDS specification importer that parses an IDS 1.0 document (entity/attribute/classification/property/material/partOf facets) into `RuleAst` rows and emits IDS-conformant audit receipts, so a published IDS check runs against the federated entity graph through the existing rule planner.
- Unlocks portable, standards-conformant model-checking where a project's IDS specification is the rule source rather than hand-authored predicates, and the conformance receipt is an exportable IDS audit artifact the CDE consumes alongside the BCF coordination protocol on `annotation#BCF_PROTOCOL`.
- Draws on IDS 1.0 as the settled buildingSMART information-delivery-specification standard, folding its facet model into the existing rule AST so a specification is data rather than a parallel checker.

[TRANSPARENCY_PROOF]: Promote the provenance attested ledger from internal-only tamper-evidence to an externally verifiable Merkle transparency log with detached signatures.
- The `provenance#ATTESTED_LEDGER` chain is non-cryptographic `XxHash128` self-consistency whose only external anchor is a periodic content-addressed seal; add a Merkle transparency tree over the attested entries that emits an inclusion proof for any one entry and a consistency proof between two sealed heads, and bind a detached signature over each periodic head seal so an external auditor verifies that a specific lineage row was logged at a specific point without trusting or replaying the server.
- Unlocks third-party non-repudiation the current page explicitly lacks — a regulatory reviewer or a CDE counterparty checks an artifact's lineage with an O(log n) inclusion proof rather than the whole chain, a split-history attack surfaces as a failed consistency proof between two heads, and an exported support bundle carries a self-verifying proof artifact rather than a server-trusted digest.
- Draws on the RFC 9162 Merkle-tree transparency-log construction (inclusion and consistency proofs over an append-only log) and detached signing as the settled verifiable-log technique, riding the existing hash-chained ledger and the periodic content-addressed seal so the proof tree is a projection over settled entries, not a second ledger store.

## [2]-[CLOSED]

[LAYERED_COMPOSITION]: [DROPPED] — USD/IFC5 layer opinion-composition duplicated the existing `versioning#COMMIT_DAG` named branches plus the `versioning#CRDT_ALGEBRA` `MvRegister` concurrent-keep and `TimeTravel.BranchFromPast`, which already give non-destructive parallel opinions, concurrent-value retention, and per-branch revert; the idea was anchored on a named product (USD) and a draft standard (IFC5) and minted no capability the commit-DAG plus CRDT does not already own.
