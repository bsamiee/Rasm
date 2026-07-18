# [CROSS_LIBS_IDEAS]

Cross-language ideas span two or more of C# / Python / TypeScript at the wire and the companion/offline seams. A concept coupling packages within one language lives in that branch's idea register. `[1]-[OPEN]` holds the live concert; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated.

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

[LAYER_TOPOLOGY_GRAPH_FACTS]-[QUEUED]: Shared `LayerTopologyFact` wire rows carry host organization into every `ElementGraph` peer.
- Capability: `LayerTopologyFact` projects `LayerStamp` identity, `LayerPath` nesting, membership, and per-viewport overrides as detached entity and containment facts, so each runtime answers layer organization without a host handle.
- Shape: C# owns the canonical wire schema and codec; `Rasm.Rhino` `Layers.Ask` emits rows, `Rasm.Element` folds them through `IElementProjection`, `Rasm.Bim` aligns them with IFC spatial structure, Python decodes them into IFC graph projections, and TypeScript decodes them into query-store relations. `ContentHash` identifies referenced geometry and payloads, while the schema's typed layer and relation keys identify graph facts.
- Unlocks: Host-organized element queries, spatial-structure round-tripping, and one organizational producer for the graph's containment axis.
- Anchors: `Rasm.Rhino` `Document/layers.md` `LayerStamp`/`LayerPath`/`Layers.Ask`; `libs/.planning/architecture.md` shared wire and AEC-domain strata; `Rasm.Element/.planning/Projection/projection.md`; `Rasm.Bim/.planning/Projection/semantic.md`.
- Tension: Detached values cross the wire; host handles remain inside `Rasm.Rhino`, and each peer projects the same canonical fact identity.
- Ripple: `python:data` `[LAYER_TOPOLOGY_GRAPH_FACTS]`; `typescript:data` `[LAYER_TOPOLOGY_GRAPH_FACTS]`.

[DAYLIGHTING_SCENE_DESCRIPTOR]-[QUEUED]: Owned sun astronomy, scene lights, and GLB tessellation compose one daylighting scene descriptor the Python analysis companion consumes for EnergyPlus/OpenStudio-grade solar and daylight studies.
- Capability: A content-keyed, host-free scene descriptor — sun state (`SunSolver` astronomy), photometric light roster (`LightStamp` rows with `Radiance` power and the `PhotometricWeb` distribution payload), shading geometry as GLB tessellation — emitted from the C# host and folded by the Python compute owner into radiation, shading, and daylight-autonomy analyses.
- Shape: A descriptor emitter on the `Rasm.Rhino` Render/Exchange surface stacking `SunState` + `Objects/lights.md` stamps + the GLB rail over the content-keyed wire; a Python consumer in the compute branch driving the machine's `energyplus`/`openstudio` engines; results return as wire receipts keyed by the same content identity.
- Unlocks: Closed-loop environmental analysis from the live model — solar exposure, shading studies, daylight metrics — without a host dependency in the analysis runtime, and a reusable scene-descriptor vocabulary for any future physics consumer.
- Anchors: `Rasm.Rhino` `Render/settings.md` `SunSolver.Solve`/`SunState`; `Objects/lights.md` `LightStamp`/`Radiance`; `Render/kinds.md` `PhotometricWeb`; `libs/.planning/architecture.md` `[04]` geometry-flow law (kernel and Python geometry meet only at the wire: content identity plus the GLB tessellation rail) and `[06]` per-language roles (Python as the host-free science/compute companion).
- Tension: Descriptor schema is a new shared wire owner — C# mints it, Python decodes it, and the schema lands in the wire vocabulary before either end builds; tessellation fidelity policy remains an explicit descriptor axis.
- Ripple: `python:compute` `[DAYLIGHTING_SCENE_DESCRIPTOR]`.

[HOST_OPLOG_CRDT_PRODUCER]-[QUEUED]: Committed host transactions become a replayable, mergeable causal op-log — the host end of the shared op-log CRDT wire owner.
- Capability: Every sealed commit folds into an `OperationId`-keyed causal log, so equal payloads remain distinct operations and cross-runtime sync, collaborative merge, and checkpoint replay become wire operations instead of file exchanges.
- Shape: `OperationId` is the shared `[ComplexValueObject]` over tenant, actor, actor sequence, and causal frontier; its canonical codec sorts the vector-clock rows before encoding. Each op-log entry carries `OperationId Id`, `ContentHash Payload`, and the sealed-commit facts. `Rasm.Rhino` `DocumentStream` and `DocumentCommit.Sealed` emit entries, while every branch decodes the same shared payload owner and replays or merges it against a checkpoint snapshot.
- Unlocks: Multi-runtime document sync, collaborative editing groundwork, deterministic replay for testing and audit, and the first live producer for the wire law's op-log owner.
- Anchors: `Rasm.Rhino` `Document/events.md` `DocumentStream.Observe`; `Document/tables.md` `DocumentCommit.Sealed`/`UndoBracket` (the one commit envelope every mutation walks — the natural single tap point); `libs/.planning/architecture.md` `[07]` shared-owner roster (the op-log CRDT payload and tenant/causal identity are named shared wire owners; C# owns the wire vocabulary, Python and TypeScript decode it).
- Tension: Distinct from the static archive diff (`Exchange/archive.md` `ArchiveDelta`) — the op-log is causal and live, the diff is structural and at-rest; CRDT merge policy settles commutation and conflict per mutation kind without conflating operation identity with payload identity.
- Ripple: `typescript:data` `[HOST_OPLOG_CRDT_CONSUMER]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
