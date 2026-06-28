# [RASM_ELEMENT_IDEAS]

The forward pool of higher-order concepts for the lowest AEC-DOMAIN element seam. `[1]-[OPEN]` holds active ideas as cards; `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition.

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

[WIRE_CODEC]-[QUEUED]: the seam emits a content-keyed wire the Python and TypeScript peers decode without re-minting.
- Capability: a `Mapperly`-generated `ElementGraph`↔proto/DTO codec lowering the `Node`/`Relationship` `[Union]` families through `[MapDerivedType]` onto a `oneof`-backed wire, the `NodeId`/`ContentAddress` content keys and the typed `Material`/`Property`/`Assessment`/`Classification` vocabulary crossing as the canonical wire shape.
- Shape: a `Graph/wire.md` owner — the source-generated mapper, the golden-byte parity corpus (a float-bearing `IfcMaterialLayer`-shaped node), and the `ContentAddress` round-trip the three runtimes agree on.
- Unlocks: a host-neutral element exchange the companion runtimes decode by content key, never re-deriving the graph.
- Anchors: `Riok.Mapperly` `[MapDerivedType]`, the `Projection/address#CONTENT_ADDRESS` one canonical codec, the kernel `XxHash128` seed-zero rail.
- Ripple: `python:geometry/ifc` `[SEAM_WIRE_DECODE]`, `typescript:interchange` `[SEAM_WIRE_DECODE]`.

[DELTA_CRDT]-[QUEUED]: the `GraphDelta` becomes the convergence substrate for offline multi-writer and IFC 3-way merge.
- Capability: a commutative/idempotent `GraphDelta` algebra (an HLC-stamped op-log over the `Graph/delta#GRAPH_DELTA` cases) the Persistence Sync owner replays for offline convergence, the `Generator.Equals` `Inequalities` diff feeding the 3-way `StructuralMerge`.
- Shape: a delta-ordering policy (last-writer-wins per node field, set-union per edge) the Persistence `Version` engine projects from the Marten event stream.
- Unlocks: collaborative editing and IFC model merge without a central lock.
- Anchors: the `GraphDelta` monoid (`Empty`/`Combine`), the `ContentAddress` causal key, the Persistence CRDT/time-travel engine.
- Tension: delta commutativity under concurrent structural edits (a drop racing an attach) needs an explicit conflict case the merge resolves.
- Ripple: `csharp:Rasm.Persistence` `[VERSION_CRDT_OVER_DELTA]`.

[STREAMING_BAKE]-[BLOCKED]: a viewport-scoped partial `Bake` for million-node federated models.
- Capability: an incremental `Bake` that materializes only the reachable subgraph of a requested object set, the incidence index and `QuikGraph` view sharing structure across partial freezes so a million-node model bakes a viewport in O(visible).
- Shape: a `Graph/element#ELEMENT_GRAPH` `BakeScope` that bounds the `Compose` descent and lazily resolves payloads.
- Unlocks: interactive editing of a federated model the frozen-whole snapshot cannot hold in memory.
- Anchors: the HAMT working form, the per-snapshot `Bake` memo, the `QuikGraph` reachability.
- Tension: the spatial-partition stream grain must align the partial bake scope with the Persistence per-model-partition stream so a scope never spans a stream boundary mid-bake.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[SEAM_DECOMPOSITION]-[COMPLETE]: the property-graph collapse of the two parallel unaligned element owners (`Rasm.Bim` `BimElement`/`BimModel` and `Rasm.Materials` `Element`/`MaterialAssignment`) into the one `ElementGraph` whose consumer-facing `Element` is the `Bake` derived fold — the canonical `Node` `[Union]`, the neutral `Relationship` edge algebra, the typed `PropertyValue`/`MeasureValue` value vocabulary, the generic `Classification`/`Discipline` axes, the `Material` composition + property family with the relocated acoustic folds, the generic `Assessment` receipt, the geospatial `Coverage`/`GeoReference`, the one canonical content codec, and the `IElementProjection`/`IGraphConstraint` contracts — authored as the `Rasm.Element` design corpus; the AEC peers now project onto this graph rather than owning parallel element records.
