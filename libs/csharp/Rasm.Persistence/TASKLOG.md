# [PERSISTENCE_TASKLOG]

The open and closed work for the durable-state spine, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup.

## [1]-[OPEN]

[T-DUCKLAKE] [QUEUED]: Promote the analytical lane to a DuckLake lakehouse over object-store Parquet.
- Capability: add a `LakehouseCatalog` owner block to `modalities/data-lanes.md` `#ANALYTICAL_LANE` that mounts DuckLake with its metadata catalog in the PostgreSQL server tier and its data files in the object store, materializing the changefeed as catalog-tracked Parquet snapshots; the catalog reuses the artifact-blob content-key addressing rather than minting a manifest.
- Packages: DuckDB.NET.Data.Full, DuckLake extension, Apache.Arrow.
- Integration: the catalog store rides `stores-server/provisioning.md` PostgreSQL as a settled tier and the data files ride `stores-remote/object-store.md` BlobRemote residence, both internal to the folder; the rollup feed aligns with `provisioning#TIMESCALE_PROVISIONING` continuous aggregates as a sibling analytical tier, never a second engine.
- Considerations: DuckLake v1.0 catalog-table layout and the snapshot/time-travel SQL surface confirm against the live extension before the catalog DDL pins; parquet-export-only stays the fallback where the extension is absent; the catalog metadata is a server-tier table set, never a JSON manifest beside the data files.

[T-ARROW-FLIGHT] [QUEUED]: Bind the Arrow plane to an ADBC + Flight SQL zero-copy egress.
- Capability: extend the `query-rail.md` `#ARROW_PLANE` carrier with a Flight SQL server surface and an ADBC consumer contract that streams the existing `ArrowCarrier` schema as columnar batches, and push incremental standing-query batches over one Flight channel.
- Packages: Apache.Arrow, Apache.Arrow.Flight, Apache.Arrow.Adbc.
- Integration: the Flight stream reuses the in-process `ArrowCarrier` schema internal to the folder and the standing-query window at `query-rail#STANDING_QUERY` as the batch source; the Flight transport endpoint binds at the app root that hosts the gRPC listener, never an interior dependency, and outbound resilience rides AppHost, never a second retry owner.
- Considerations: confirm the Arrow.NET Flight SQL producer member surface and the ADBC statement/result contract against the folder `.api/` catalogue before the egress fence pins; the columnar batch schema is the one `ArrowCarrier` projection, never a parallel result shape; the zero-copy lifetime is bounded to one chunk so peak managed memory stays one batch wide.

[T-FASTCDC-DEDUP] [QUEUED]: Replace fixed 64-KiB snapshot framing with FastCDC content-defined chunking.
- Capability: swap the fixed-window framing in `snapshots/codecs.md` `#SNAPSHOT_PROTOCOL` and the `stores-remote/object-store.md` `#MULTIPART_TRANSFER` window for FastCDC content-defined chunk boundaries, keying each chunk by its `XxHash128` content address so the artifact-blob index dedups identical chunks across snapshots and peers.
- Packages: System.IO.Hashing, FastCDC.Net.
- Integration: the opaque-byte FastCDC chunker lands in `snapshots/codecs.md` as a snapshot-frame owner with internal coupling to `cache/indexes.md` `#ARTIFACT_BLOB_INDEX` for the chunk content-key dedup; it aligns with `Compute/interchange#GEOMETRY_DELTA` at the technique level only — Compute owns the geometry-structural delta and this owns the opaque-byte snapshot/blob frame, neither importing the other's chunker; the multipart transfer windows whole chunks rather than fixed frames, never re-declaring a frame width.
- Considerations: the content-defined boundary parameters (min/avg/max chunk size and the gear-hash mask) trace to one policy table, never free literals; the dedup ratio under edit churn and the cross-snapshot chunk-reuse rate confirm against a real model-history corpus before the chunker replaces the fixed window; the existing 64-KiB Crc32-per-frame integrity stays the in-transfer check.

[T-IDS-RULES] [QUEUED]: Lift the buildingSMART IDS document model into the federation rule engine.
- Capability: add an `IdsSpecification` importer to `federation/federation.md` `#RULE_PLAN` that parses an IDS 1.0 document's facets (entity/attribute/classification/property/material/partOf) into `RuleAst` rows and emits IDS-conformant audit receipts from the existing rule planner run against the federated entity graph.
- Packages: System.Xml.Linq (BCL inbox) for the IDS XSD-validated document parse, LanguageExt.Core.
- Integration: the importer projects onto the existing `federation#RULE_PLAN` `RuleAst` union internal to the folder and runs through the settled `RulePlan` lowering; the conformance receipt aligns with `annotation/annotation.md` `#BCF_PROTOCOL` as an exportable audit artifact the CDE consumes, never a parallel checker.
- Considerations: confirm the IDS 1.0 facet schema and the conformance-receipt shape against the published buildingSMART specification before the importer fence pins; an unsupported facet maps to one `RuleAst` case or a typed unsupported-facet rejection, never a second checker; the specification is data authored alongside the descriptor catalog, never C# predicate code.

[T-TRANSPARENCY-PROOF] [QUEUED]: Promote the attested ledger to a Merkle transparency log with detached-signed head seals.
- Capability: add a `TransparencyProof` owner to `provenance/provenance.md` `#ATTESTED_LEDGER` that builds a Merkle tree over the `AttestedEntry` chain, emits an `InclusionProof` for any one entry and a `ConsistencyProof` between two `LedgerHead` seals, and binds a detached signature over each periodic content-addressed head seal so an external auditor verifies one entry in O(log n) without replaying the chain.
- Packages: System.IO.Hashing for the Merkle node digests, System.Security.Cryptography (BCL inbox) for the detached head-seal signature.
- Integration: the proof tree is a projection over the existing `AttestedEntry` rows and the periodic seal internal to the folder, riding the `Snapshots.ContentAddress` head-seal anchor; the inclusion/consistency proof exports through the `retention/redaction-retention.md` hash-proved support-bundle as a self-verifying artifact, never a second ledger store.
- Considerations: the Merkle node hash stays `XxHash128` for tamper-evidence while the head-seal signature is the only cryptographic surface so the non-repudiation claim attaches to the signed seal, not the tree; confirm the RFC 9162 inclusion-and-consistency-proof construction before the proof fence pins; the redaction-preserving entry keeps its pre-redaction leaf digest so a proof verifies after redaction, matching the existing chain invariant.

## [2]-[CLOSED]

[T-LAYER-COMPOSE] [DROPPED]: USD/IFC5 non-destructive layer opinion-composition.
- Dropped with its `[LAYERED_COMPOSITION]` idea: the `LayerStack` over named op-log partitions duplicated `versioning#COMMIT_DAG` named branches, the `versioning#CRDT_ALGEBRA` `MvRegister` concurrent-keep, and `TimeTravel.BranchFromPast`, which already deliver non-destructive parallel opinions, concurrent-value retention, and per-branch revert over the one op-log; minting a second composition surface broke the closed-CRDT prohibition.

[T-DOC-MIGRATE] [COMPLETE]: Re-homed the 17 design pages into the single `.planning/<sub-domain>/<page>.md` tree, rebuilt `ARCHITECTURE.md` as a codemap, `README.md` as router-plus-package-registry, and authored `IDEAS.md`/`TASKLOG.md`. The per-folder `.api/` stays at the package root.
