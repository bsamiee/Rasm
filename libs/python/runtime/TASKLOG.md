# [PY_RUNTIME_TASKLOG]

Open and closed work for `runtime`, distilled from `IDEAS.md`. Each task card carries a status marker on its leader — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and three to four bullets: the design page or `.api/` catalogue to build, the external packages to integrate, the integration points and boundaries/wires, and the key considerations. One idea spawns one or more tasks. A design-complete idea closes here; the downstream source-transcription mode is outside the planning task pool.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[CRDT_OPLOG_LZ4]-[BLOCKED]: decode the compressed CRDT op-log envelope.
- Capability: the cp315 core decodes the C# `MessagePackCompression.Lz4BlockArray` op-log envelope at `transport/serve#CRDT_DECODE`, with the settled `msgspec.msgpack.Decoder(CrdtArm)` uncompressed-delta leg remaining the canonical MessagePack union decode.
- Shape: `CrdtOpDecode.decode(decompress)` keeps the consumer-side seam as one injected callable: either identity over a producer-owned `MessagePackCompression.None` companion lane, or the published `Lz4BlockArray` reader over `lz4.block`; raw `lz4.frame`, raw block bytes, protobuf, JSON, and a second op vocabulary stay outside the owner.
- Unlocks: collaborative CRDT op decode runs end to end on the cp315 core instead of relying on the current `python_version<'3.15'` companion band for compressed builds.
- Anchors: `csharp:Rasm.Persistence/Version/commits#CRDT_WIRE`, `CrdtWire.Encode`, `MessagePackCompression.Lz4BlockArray`, `CRDT_OPLOG_WIRE_AMENDMENT`, `transport/serve#CRDT_DECODE` `[CRDT_DECODE_LZ4]`, `msgspec.msgpack.Decoder(CrdtArm)`, and `lz4`.
- Tension: blocked on both a cp315/abi3 `lz4` wheel and the producer publication decision: expose a `MessagePackCompression.None` companion lane or publish the shared MessagePack-csharp `Lz4BlockArray` ext-envelope spec.

[ARTIFACT_PIPELINE_KEYED_CONSUMER]-[QUEUED]: confirm the session lane is the elision substrate the artifacts pipeline produces Keyed `(ContentKey, Work)` pairs for.
- Capability: a confirmation that the artifacts `ARTIFACT_PIPELINE` feeds the `runtime/execution/lanes` `Keyed[T]` port and the `StagePlan` `graphlib` DAG, never a second cache, store, scheduler, or DAG; durable federation stays C# Persistence.
- Shape: a `[RECEIPT]` seam on `runtime/execution` mirroring the `artifacts pipeline/plan <- python:runtime/execution [RECEIPT]` edge; no new runtime surface, since the `Keyed[T]` port and `graphlib` DAG already exist and the pipeline folds the existing `LanePolicy.cached` session port (`Map[ContentKey, T]` in-memory, never durable).
- Anchors: `runtime/execution/lanes.md` `LanePolicy.cached` / `Keyed[T]` / `StagePlan.execute`, `artifacts pipeline/plan#PIPELINE`.
- Ripple: `artifacts` `[ARTIFACT_PIPELINE]` — the session lane is the elision substrate the pipeline produces Keyed `(ContentKey, Work)` pairs for; the pipeline owns no durable store.
- Atomic: confirm the Keyed session port consumption, no new owner.

[DATA_LINEAGE_RECEIPT]-[QUEUED]: contribute the data query owner's column-level `lineage_edges` as a Receipt projection.
- Capability: `runtime/observability` accepts the data `QueryReceipt.lineage_edges` contribution through the existing `ReceiptContributor`; the durable provenance ledger stays C# Persistence.
- Shape: a `ReceiptContributor` row accepting the `lineage_edges` fact from `python:data/tabular/query`, mirroring the `tabular/query -> runtime/observability [RECEIPT]` edge; no durable Python store.
- Anchors: `runtime/observability` `ReceiptContributor`, the data `QueryReceipt.lineage_edges` field.
- Ripple: `data` `[QUERY_PLAN_PROVENANCE]` — contribute `lineage_edges` through `ReceiptContributor`; no durable store.
- Atomic: one `ReceiptContributor` row accepting the lineage fact.

[DATA_TRANSPORT_DSN]-[QUEUED]: resolve every data-side remote DSN through the one runtime `TransportResource` owner.
- Capability: `runtime/roots` `TransportResource` resolves the DSN, credential, and runner-address for the data `REMOTE_PARTITION_DEEPEN` (flightsql `grpc+tls`), `DAFT_ELASTICITY` (Ray runner), `DUCKDB_ICEBERG_PROMOTE` (DuckDB iceberg `SECRET`/`ENDPOINT`), and `LAKEHOUSE_DUCKLAKE_FORMAT` (ducklake catalog DSN) arms, and `runtime/transport` `ResourceRef` resolves the `TENSORSTORE_ADMIT` cloud kvstore path; no data card mints a second credential owner.
- Shape: `TransportResource` / `ResourceRef` rows consuming the `python:data/tabular/query` + `tabular/lakehouse` + `gridded/store` transport edges, mirroring the four `[TRANSPORT]`/`[PORT]` edges on both endpoints through `fsspec` path resolution.
- Anchors: `runtime/roots` `TransportResource`, `runtime/transport` `ResourceRef`/`fsspec` path resolution, the data query/lakehouse/store DSN arms.
- Ripple: `data` `[REMOTE_PARTITION_DEEPEN]` (and `[DAFT_ELASTICITY]`, `[DUCKDB_ICEBERG_PROMOTE]`, `[LAKEHOUSE_DUCKLAKE_FORMAT]`, `[TENSORSTORE_ADMIT]`) — resolve every remote DSN through the one `TransportResource` owner.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
