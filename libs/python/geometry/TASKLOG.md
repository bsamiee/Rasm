# [PY_GEOMETRY_TASKLOG]

Open and closed work for `geometry`, distilled from `IDEAS.md`. Each task card leads with `[ID]-[STATUS]: thesis` and carries `Capability`, `Shape`, `Unlocks`, `Anchors`, and optional `Tension` fields. `[1]-[OPEN]` holds live work; `[2]-[CLOSED]` records finished or dropped tasks.

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

[IFC_BYTES_CONSTRUCTOR]-[BLOCKED]: add the IFC-bytes non-manifold topology constructor.
- Capability: `TopologyAlgebra` admits IFC bytes into the existing non-manifold `TopologyOp.CONSTRUCT` case without minting a second topology owner.
- Shape: `ifcopenshell.file.from_string` opens the SPF bytes in memory; `Topology.ByIFCFile(file)` ingests the model object and returns a per-product topology `list[Any]` folded via `Cluster.ByTopologies` into one non-manifold handle the `_CONSTRUCT` table lifts as the locked `SourceKind.IFC` row.
- Unlocks: IFC-derived cell, aperture, adjacency, and dual-graph analysis reaches the geometry graduation rail while the C# BIM space-graph remains the in-process semantic owner.
- Anchors: `graph/nonmanifold.md#3-IFC_BYTES_CONSTRUCTOR`, `.api/topologicpy.md`, `Topology.ByIFCFile`, `Topology.ByIFCPath`, `Cluster.ByTopologies`, `ifcopenshell.file.from_string`, and the opt-in worker lane.
- Tension: the constructor arity is source-confirmed — `Topology.ByIFCFile` takes the `ifcopenshell.file` object (docstring `file : ifcopenshell.file`), `Topology.ByIFCPath` takes the path string — so the `_CONSTRUCT` row is signature-locked; the temp-file detour stays forbidden. The card stays BLOCKED on the orthogonal dual gate: `topologicpy` requires an explicit AGPL-accepting companion row (cross-ref `[GEO_TOPOLOGICPY_LICENSE_REPLACE]` / `nonmanifold#4-DUAL_GATE_EXCLUSION`).

[GEOMETRY_CPU_OFFLOAD]-[QUEUED]: hand geometry CPU kernels to the settled runtime offload lane.
- Capability: heavy geometry kernels run through the settled runtime `LanePolicy.offload` per-subinterpreter lane instead of a geometry-local concurrency surface.
- Shape: `registration.py` ICP/global loops, `mesh/daemon.py` OCCT iteration, `ingestion.py` `pdal` filters, `reconstruction.py` open3d reconstruction, and `repair.py` `manifold3d` boolean work hand caller-supplied kernels to `LanePolicy.offload(kernel, *args)` under the lane's one `CapacityLimiter` and one `DrainReceipt`, the active OTel context stitched across the subinterpreter hop by the lane.
- Unlocks: the companion event loop stays responsive during multi-second geometry work, and every geometry owner adopts the same one-call handoff over the already-realized lane.
- Anchors: `runtime/.planning/execution/lanes.md#LANES` `LanePolicy.offload` / `anyio.to_interpreter.run_sync` (the no-pickle subinterpreter path the lane owns — there is no process-pool fallback, a CPU kernel a subinterpreter isolates never pays a serialization tax), `daemon.md#DAEMON`, `registration.md#REGISTRATION`, `repair.md#MESH`, `ingestion.md#OFFLOAD_LANE`, `reconstruction.md#OFFLOAD_LANE`, and the `scan/deviation.md#DEVIATION` per-point proximity/segmentation hand-off already authored against the seam.
- Tension: runtime owns `LanePolicy` and geometry only consumes it through the one-call `offload`; the lane never imports the kernel, the kernel carries only the optional `LanePolicy` field, and no kernel is forced picklable — the subinterpreter path runs the caller `Callable` in place with no pickle round-trip, so the deleted process-pool form is never reintroduced as a geometry-local fallback. This task absorbs the former `IDEAS.md` `[COMPANION_CPU_OFFLOAD]` idea, which is dropped to one task rather than a standing idea.

[MESH_CODEC_BOUNDARY]-[QUEUED]: record the scene-export versus mesh-file-codec boundary so a later pass never collapses the artifacts figures/scene USD/USDZ export into the geometry mesh-file codec.
- Capability: a `[BOUNDARY]` seam on geometry/mesh recording that artifacts figures/scene owns visualization-scene export (camera, lights, PBR) while geometry owns the mesh-file codec, sharing no owner and admitting no collapse.
- Shape: one `[BOUNDARY]` seam row in geometry `ARCHITECTURE.md` mirroring the artifacts figures/scene to `python:geometry/mesh` `[BOUNDARY]` edge.
- Anchors: geometry `mesh.md`, artifacts `figures/scene#SCENE` `SCENE_USD_EXPORT`, `vtkUSDExporter`.
- Ripple: `artifacts` `[SCENE_USD_EXPORT]` — scene export and mesh-file codec stay distinct owners; the seam records the boundary so neither side collapses the other.
- Atomic: one `[BOUNDARY]` seam row.

[SCAN_COPC_PARTIAL]-[QUEUED]: confirm pdal removal is partial — the data COPC arm rebinds to `laspy.copc` while geometry scan/ingestion keeps its full pdal filter-graph owner.
- Shape: a confirmation that geometry retains pdal and its scan/ingestion filter-graph catalogue; the data-to-geometry Arrow point-record bridge seam is unchanged in shape, only the data-side decode engine changes.
- Ripple: `data` `[REMOTE_POINTCLOUD_LASPY]` — geometry keeps its full pdal filter-graph; data leaves pdal only for COPC decode, and the decoded `pyarrow.Table` bridge stays untouched.
- Atomic: one filter-graph retention confirmation.

[MESH_TOPOLOGY_SHAPE]-[QUEUED]: confirm the data spatial/mesh `MeshPayload` deepening meets the geometry mesh owner at cell-block topology through the existing mirrored seam.
- Capability: geometry/mesh consumes the deepened `MeshPayload` cell-block topology (`rhino3dm`/`trimesh`/`meshio` file exchange) at the existing mirrored seam, where the data deepening adds type-keyed `cell_data`/`point_data` and a `.3dm` row.
- Shape: a confirmation of the existing spatial/mesh to geometry/mesh `[SHAPE]` seam under the deepened `MeshPayload`.
- Anchors: geometry/mesh owner, the data `MeshPayload` cell-block topology, the existing mirrored seam.
- Ripple: `data` `[MESH_DATA_PRESERVE]` — the deepened `MeshPayload` cell-block topology crosses the existing `[SHAPE]` seam unchanged in shape.
- Atomic: one seam confirmation.

[STRUCTURAL_SECTION_PROPS]-[BLOCKED]: land the `sectionproperties` warping/plastic/shear enrichment tier in the `ifc/structural` owner.
- Shape: the `WARPING` fold arm builds `pre.Geometry.from_points(points, facets, control_points, holes)` from the `ProfileRings` (one closed facet loop per outer/void ring with a per-ring index offset, each void's guaranteed-interior `_interior_point` through `holes`), runs `create_mesh(mesh_sizes)`, binds `analysis.Section`, runs `calculate_geometric_properties` then `calculate_warping_properties` then `calculate_plastic_properties` in the one prerequisite order, and folds `get_j`/`get_sc`/`get_as`/`get_s` onto the warping slots plus `get_area` back-checked against the numpy spine area as the `fe-area` convergence residual — the FE torsion landing on the distinct `fe_torsion_constant` slot, never overwriting the closed-form spine torsion.

[KISS_MATCHER_FALLBACK_FGR]-[BLOCKED]: select the `open3d` Fast Global Registration bootstrap when `kiss-matcher` is unavailable in the active worker lane.
- Capability: `ScanRegistration._engine` resolves the `BootstrapEngine` per engine selection — `KISS_MATCHER` when available, `OPEN3D_FGR` otherwise — so the `GLOBAL` arm mints the initialization-free coarse pose on every worker lane the registration owner runs on, the FGR fallback seeding `small-gicp` fine refinement exactly as the `kiss-matcher` primary does.
- Shape: the `OPEN3D_FGR` bootstrap downsamples and estimates normals on both legacy clouds, computes `compute_fpfh_feature` descriptors, runs `registration_fgr_based_on_feature_matching` reading `.transformation`/`.fitness`/`.inlier_rmse`, and folds the correspondence-set length into the inlier count — the dispatch row body authored in `registration.md#REGISTRATION`, the per-interpreter `_engine` selection it serves landing with this task.
- Anchors: `scan/registration.md#REGISTRATION` `BootstrapEngine.OPEN3D_FGR` arm / `_engine` availability read, `.api/kiss-matcher.md`, `.api/open3d.md` (`registration.registration_fgr_based_on_feature_matching`/`compute_fpfh_feature`/`estimate_normals`/`KDTreeSearchParamHybrid`), the `small-gicp` fine-refinement seed.

[GMSH_BUILD_EXCLUSION_MANDATE]-[BLOCKED]: record the `gmsh` GPL default-build-exclusion plus singleton subprocess-isolation mandate as a `[BOUNDARY]` decision on the `mesh/quality` consumer, gating any later FE/volumetric mesh-generation row.
- Shape: one `[03]` mandate paragraph in geometry `ARCHITECTURE.md` mirroring `[TOPOLOGICPY_EXCLUSION]` — the GPL default-build exclusion, the Forge-worker opt-in, and the singleton subprocess-isolation rule — so when a finite-element/volumetric mesh-generation row lands on the `mesh/quality` aspect-ratio/skewness consumer it folds Delaunay/frontal/recombination 1D/2D/3D element generation and physical-group tagging into a `MeshPayload` through the existing data seam under a pre-settled exclusion, never a fresh admission decision at code time; no `gmsh` manifest row and no `.api/gmsh.md` catalogue land until that consuming fence is authored.
- Anchors: geometry `ARCHITECTURE.md#[03]-[COMPANION_LANES]` `[TOPOLOGICPY_EXCLUSION]` (the exclusion-mandate shape to mirror), the `mesh/quality.md#QUALITY` aspect-ratio/skewness receipt the eventual generated mesh feeds, the data `MeshPayload` cell-block seam, the `runtime/.planning/execution/lanes.md#LANES` offload subprocess hop, the README `gmsh` "Deferred (not admitted by any provenance)" row, the `mesh-algebra` `GeometrySubject` case the future row graduates.
- Atomic: one `[03]` build-exclusion mandate row mirroring `[TOPOLOGICPY_EXCLUSION]`.
- Tension: `gmsh` carries no `.api/gmsh.md` catalogue today, so the FE generation row's `gmsh` member signatures stay uncited and out of scope until the catalogue lands; this task records ONLY the exclusion-and-isolation mandate (an in-scope `ARCHITECTURE` refinement), never a manifest admission and never a code fence, [BLOCKED] on the same Forge GPL-accepting lane decision that strands `topologicpy`, with the FE generation row itself deferred until a real `mesh/quality` consumer and the `gmsh` catalogue both land.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
