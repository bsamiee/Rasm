# [PERSISTENCE_IDEAS]

The forward pool of higher-order concepts for the durable-state spine, each grounded in the folder's domain and current platform capability. Open ideas drive the tasks in `TASKLOG.md`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition so it is never re-litigated.

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

[PERSISTENCE_LIBRARY_TABLES]-[QUEUED]: Rasm.Persistence owns the durable content-hash identity and federation tables backing the MaterialLibrary catalogue, the ProfileCatalogue/ConnectionCatalogue registered rows, and the MaterialPropertyLibrary property sets so a material/profile/connection/property row persists with a content key and federates across the workspace, never an in-memory FrozenDictionary the runtime rebuilds.
- Capability: The persistence owner giving the MaterialId-keyed MaterialParameters rows, the ProfileId/ConnectionId-keyed catalogue rows, and the MaterialId-keyed MaterialPropertySet rows a durable content-hash identity and a federation table so the catalogue is a persisted, content-addressed library a workspace shares rather than a compile-time seed.
- Shape: A Rasm.Persistence library/property durable table set keyed by the Materials content keys (MaterialId/ProfileId/ConnectionId), the durable backing the in-memory FrozenDictionary catalogues seed from, the producer end of the durable-library federation concern.
- Unlocks: A material/profile/connection/property library row persists and federates with a content hash, a user-authored material admits into the durable catalogue, and the workspace shares one content-addressed library across host/web/companion consumers.
- Anchors: graph.md MaterialLibrary.Rows (the FrozenDictionary catalogue seed); profile.md/connection.md the ProfileCatalogue/ConnectionCatalogue registered-row tables (connection.md lines 92-101); properties.md MaterialPropertyLibrary.Rows (properties.md lines 176-192); ARCHITECTURE.md Rasm.Persistence content-hash identity/federation; the CLAUDE manifest Persistence-owns-durable-identity law.
- Ripple: counterpart of `Rasm.Materials` catalogue owners behind `[STEEL_DESIGN_OBJECT]`/`[SUSTAINABILITY_PROPERTY_DISCIPLINE]` (durable `MaterialLibrary`/`ProfileCatalogue`/`ConnectionCatalogue` rows).

[PERSISTENCE_BIM_SYNC_CRDT]-[QUEUED]: Persistence introduces a CRDT sync layer over the Bim BimWire OpLog face so concurrent multi-party edits to one federated BimModel converge through the typed ElementChange op-stream, the Bim Versioned commit-DAG providing the content-addressed commit objects the CRDT merges against.
- Capability: A convergent multi-party BIM editing substrate: the OpLogWire ElementChange op-rows are the CRDT operations, the content-key fingerprint is the element identity, and the Versioned merge fold reconciles divergent branches, collaborative BIM the pairwise diff cannot express.
- Shape: A Persistence Sync owner folding the Bim OpLogWire op-stream into a CRDT over GlobalId-keyed ElementChange operations, converging concurrent revisions against the Versioned commit-DAG common ancestor.
- Unlocks: Real-time collaborative model editing with convergent merge, the multi-author platform the federation diff and the Versioned DAG together set up.
- Anchors: Exchange/wire#WIRE_PROJECTION OpLogWire op-log face (ONE_MODEL_THREE_FACES confirmed); Review/diff#MODEL_DIFF ElementChange; csharp:Rasm.Persistence/Sync collaboration; Rasm.Bim Versioned commit-DAG; realizes the `VERSIONED` idea.
- Tension: The CRDT is the Persistence Sync owner's concern consuming the Bim OpLog face and the Versioned commit objects: Bim produces the typed op-stream and the commit-DAG, Persistence owns convergence, so the merge algebra split stays clean between the two owners.
- Ripple: counterpart of `Rasm.Bim` `[VERSIONED]` idea (CRDT sync over the `OpLogWire` op-log face).

[FABRICATION_PROGRAM_DURABLE_ROWS]-[QUEUED]: A Rasm.Persistence fabrication-program durable schema persists the toolpath/nesting/workholding/post-dialect receipts as content-addressed rows so a posted cut-program, a nest layout, and a fixture setup survive as queryable durable records, closing the persistence gap the host-local Fabrication receipts have no durable owner for.
- Capability: Every Fabrication kernel produces a host-local receipt (Motion joint stream, Placement transforms + utilization, CutProgram GWord AST, Fixture exclusion set) that crosses only the in-process seam and dead-ends — no durable persistence owner, so a posted program or nest layout is recomputed every session with no provenance, diff, or version. This ripple-idea authors the Persistence-side durable schema: content-addressed fabrication-program rows (keyed by the same XxHash128 content identity the nesting Remnant already mints) carrying the toolpath/nesting/post-dialect/workholding receipts as queryable durable records the Persistence Schema/Store owners hold.
- Shape: A new Rasm.Persistence Schema/Store fabrication-program durable-row owner: content-addressed rows for the CutProgram AST, the nest Placement, the Fixture setup, and the resolved (Process, Machine, PostDialect) job axis, keyed by the XxHash128 content digest, composing the existing Persistence DDL/converters/identity surfaces — the durable owner the host-local Fabrication receipts project into at the wire boundary, never a Fabrication-side persistence reference.
- Unlocks: Durable, queryable, content-versioned fabrication programs (a posted G-code program diffable across revisions, a nest layout recalled by content key, a fixture setup persisted), and a second real consumer of the XxHash128 content identity beyond the in-folder nesting memo.
- Anchors: csharp:Rasm.Persistence/Schema#IDENTITY and Store the DDL/converters/identity/engine owners; Fabrication/Posting/program#CUT_PROGRAM CutProgram, Nesting/nfp#NESTING Placement/Remnant XxHash128 (nfp.md lines 42-52), Nesting/workholding#WORKHOLDING Fixture; the host-local receipt wire boundary
- Tension: The Fabrication receipts are host-local types the page law says never sit between wire and rail — the durable rows must consume a WIRE PROJECTION of the receipts (a serialized program/nest/fixture shape), not the in-process records, so the persistence owner reads the projected wire form and Fabrication never references Persistence.
- Ripple: counterpart of `Rasm.Fabrication` Posting `program` (`CutProgram`) + `Nesting`/`workholding` owners (durable program/nesting rows).

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
