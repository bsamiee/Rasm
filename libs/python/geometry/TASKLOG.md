# [PY_GEOMETRY_TASKLOG]

Open and closed work for `geometry`, distilled from `IDEAS.md`. Each task card leads with a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` open; `[COMPLETE]` or `[DROPPED]` closed — and carries the capability or file to build, the external packages to integrate, the integration points and boundaries, and the key considerations. `[1]-[OPEN]` holds live work; `[2]-[CLOSED]` records finished or dropped tasks.

## [1]-[OPEN]

[BLOCKED] Build the `step-bridge/cad.py` `StepBridge` owner — the CAD-STEP tessellation hop. Blocked on the `pythonocc-core` distribution gap (no PyPI artifact, conda-only) or confirming the `ifcopenshell`-bundled OCCT exposes the reader/tesselator surface on the Forge companion lane.
- Capability: STEP/IGES B-rep bytes plus tolerance into per-element GLB, riding the same `SourceFormat`-discriminated `TessellationRequest` the IFC daemon already carries, so one daemon serves IFC and CAD-STEP through one content-addressed hop.
- Packages: `pythonocc-core` (`STEPControl_Reader`/`IGESControl_Reader`/`BRepMesh`/`ShapeTesselator`) admitted to the branch `pyproject.toml` companion floor, or the OCCT the `ifcopenshell` wheel already bundles; the GLB assembly reuses the `tessellation/daemon.md` serializer path.
- Integration: internal — `tessellation/daemon.md` already routes the STEP/IGES `SourceFormat` rows to `cad#BRIDGE` and composes `StepBridge.model`, so this task supplies that reader returning an iterable B-rep shape the shared GLB assembly drains; aligned cross-language to the C# `StepIso10303` codec at the wire (the codec calls this companion for CAD tessellation), never coupled to a managed interior; content-addressed via the same runtime `ContentIdentity` key and served over the existing gRPC contract.
- Considerations: there is no pure-Python AP242 B-rep tessellator, so this is a companion-floor OCCT hop; OCP tessellation drops normals and ties no vertices, so `pythonocc-core` `ShapeTesselator` (or the wrapped OCCT) is the reader to verify before admitting.

[QUEUED] Deepen `scan-registration/registration.py` to the full modern stack.
- Capability: the `GLOBAL` RANSAC+FPFH bootstrap, the `MULTISCALE` tensor `multi_scale_icp` with robust kernels, the `VGICP` speed path, and the `MULTIWAY` pose-graph optimization under the one `RegistrationMode`-discriminated owner.
- Packages: `open3d` (`t.pipelines.registration.multi_scale_icp`/`TransformationEstimationPointToPlane`/`robust_kernel`/`registration_ransac_based_on_feature_matching`/`compute_fpfh_feature`/`PoseGraph`/`global_optimization`), `small_gicp` (`align`/`GaussianVoxelMap`).
- Integration: internal to the sub-domain; transforms graduate through the compute `HandoffAxis` geometry case; bounded fan-out over multi-station clouds rides the runtime `LanePolicy`.
- Considerations: the tensor backend accessor shapes (`point.positions.numpy()`) and the `multi_scale_icp` schedule arity confirm against the live `open3d` catalogue; the `small_gicp.align` keyword arity and error-to-fitness mapping confirm against the live `small_gicp` catalogue.

[QUEUED] Add scan-vs-model deviation and primitive extraction to `scan-registration`.
- Capability: RANSAC plane/primitive segmentation to extract walls/slabs/columns from a registered cloud, then a nearest-surface deviation query (max/mean/std) against the IFC-tessellated GLB, emitting a per-element deviation receipt.
- Packages: `open3d` (`segment_plane`/region-growing oversegmentation/nearest-surface query), `trimesh` (point-to-mesh distance against the companion GLB).
- Integration: composes the `tessellation/daemon.md` GLB output as the reference surface and the registered transform from this sub-domain; deviation rows graduate through the geometry case keyed to the IFC element GlobalId.
- Considerations: depends on the deepened registration stack (the registered pose is the precondition); learned semantic segmentation is out of host-free CPU scope and informs only the segmentation heuristics.

[QUEUED] Build the `mesh-utility/repair.py` `MeshOp` owner — robust repair and boolean. `manifold3d` is manifest-declared (gated `python_version<'3.15'`), the robust boolean backend `trimesh.boolean` dispatches to.
- Capability: watertight detection plus hole-fill plus winding/normal repair, exact union/difference/intersection boolean, and mesh-file decode/encode, as one polymorphic `MeshOp` the tessellation, scan, and step hops compose.
- Packages: `trimesh` (`boolean` over the `manifold3d` backend, `repair` winding/normal/hole-fill), `rhino3dm`, `meshio` (mesh-file codecs) — all admitted; `manifold3d` admitted to the branch `pyproject.toml` as the boolean engine.
- Integration: internal owner consumed downstream by `tessellation`, `scan-registration` reconstruction, and `step-bridge`; mesh-file decode aligns to the branch mesh-file-exchange seam (`data/mesh-exchange`), never coupled to a managed interior; clash-volume requests from `ifc-analysis` compose this owner's boolean op.
- Considerations: the `manifold3d` backend is the robust default and the legacy mesh-boolean path is slow/unreliable, so the backend is fenced explicitly; clash-volume computation for `ifc-analysis` composes the boolean op rather than re-implementing it.

[BLOCKED] Replace the hand-rolled IDS/clash fold in `ifc-analysis/analysis.py` with the native ecosystem triad. Blocked on the `ifctester`, `ifcclash`, and `bcf` distributions landing on the Forge companion lane — three separate IfcOpenShell/Bonsai-ecosystem distributions, not bundled with the `ifcopenshell` wheel; the page's imports do not resolve until all three are present on the lane.
- Capability: IDS model-checking, filtered clash sets, and BCF issue round-trip as `AnalysisKind` rows producing BCF-serializable findings, in place of the bespoke rule-table and OCCT-overlap reimplementations.
- Packages: `ifctester` (`ids.Ids`/`ids.open`/`reporter`), `ifcclash` (`Clasher`/clash-set query), `bcf` (issue read/write), each provided by the Forge companion lane alongside `ifcopenshell`.
- Integration: internal to the sub-domain; findings graduate through the compute `HandoffAxis` geometry case as standards-conformant output the C# owner system and the wider toolchain consume at the wire.
- Considerations: the `ifctester`/`ifcclash`/`bcf` entrypoints confirm against the folder `.api` catalogues once admitted; the standard mandates the native provider APIs end-to-end, so no local IDS rule fold survives.

## [2]-[CLOSED]

None.
