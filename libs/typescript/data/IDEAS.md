# [TS_DATA_IDEAS]

Forward pool of higher-order `data` concepts grounded in the durable-persistence domain; an idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[LAYER_TOPOLOGY_GRAPH_FACTS]-[QUEUED]: Decoded `LayerTopologyFact` rows land as read-side query-store relations for transport and visualization.
- Capability: Wire-carried layer and relation keys decode into `Model.Class` relations — layer identity, layer-path nesting, membership, and per-viewport overrides as decoded rows — so the read side serves host organization to transport and visualization consumers keyed by the one `ContentKey`, with no host handle.
- Shape: A boundary decoder folds the detached fact rows into the read side's projection tables; `SqlSchema` typed reads and `SqlResolver` batched loaders serve layer organization over `Query.table`, the object and journal planes carry the rows across runtimes under the one `ContentKey`, and the decoded relations feed the layer-visualization surface.
- Unlocks: Host-organized read-side queries, cross-runtime layer transport, and a visualization-ready organizational axis every peer reads by content identity.
- Anchors: `read/query.md` `Model.Class`/`SqlSchema`/`SqlResolver`/`Query.table`; the one `ContentKey` content-identity wire; `README.md` durable-persistence plane and the bit-identical content-identity demand across wire peers.
- Tension: Wire schema and codec mint in C#; this plane decodes and never re-mints, and the query-store relations carry only detached fact rows, never a host layer handle.
- Ripple: `libs/.planning` `[LAYER_TOPOLOGY_GRAPH_FACTS]`.

[HOST_OPLOG_CRDT_CONSUMER]-[QUEUED]: Host op-log entries decode, replay, and merge against the journal plane — the TypeScript end of the shared op-log CRDT wire owner.
- Capability: `OperationId`-keyed causal entries decode at the boundary and replay through the journal's one write owner, so cross-runtime sync, collaborative merge, and checkpoint replay land as journal operations keyed by the shared causal identity, with `ContentHash` payloads resolved through the object plane.
- Shape: A boundary decoder admits the C#-minted op-log wire rows; replay folds entries into `Journal.publish` intents under `Occ` arbitration, merge applies the CRDT commutation policy per mutation kind before append, and a checkpoint snapshot bounds replay windows through the journal's windowed `read`.
- Unlocks: Multi-runtime document sync into the durable plane, deterministic replay for audit, and the consumer half that arms the producer's wire.
- Anchors: `journal/append.md` `Journal.publish`/`Occ`/`StreamKey`; `journal/evolve.md` upcast road for entry payload versions; `object/store.md` `ContentKey` payload custody.
- Tension: C# mints the wire schema and codec — this plane decodes and never re-mints; merge policy settles commutation per mutation kind without conflating operation identity with payload identity.
- Ripple: `libs/.planning` `[HOST_OPLOG_CRDT_PRODUCER]`.

[OBJECT_PLANE_INSTRUMENT_PROJECTION]-[QUEUED]: Object-plane receipts gain their lossy instrument projections once `Convention` mints the `rasm.object.*` rows.
- Capability: Dedup rate (`ObjectStore.Receipt.written`), bytes written, GC reclaim, and resumable-upload throughput project from receipts the store and stream pages already mint — receipts stay the truth, instruments the dashboard projection.
- Shape: `Convention` rows land first under its growth law (metric name with instrument metadata), then the object owners emit through the same instrument-row idiom the journal and read pages carry.
- Unlocks: Object-plane health on the estate dashboards with zero new evidence surfaces.
- Anchors: `object/store.md` receipt family; `object/stream.md` `ChunkMark` and Merkle proof receipts; the `journal/fact.md`/`read/fold.md` instrument-row idiom.
- Tension: `Convention` rows are core's mint — this folder emits only after the vocabulary exists, never through a free-string metric name.
- Ripple: `libs/.planning` `[UNIFIED_SIGNAL_FABRIC]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
