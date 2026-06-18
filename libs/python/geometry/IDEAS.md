# [PY_GEOMETRY_IDEAS]

The forward pool of higher-order concepts for `geometry`, grounded in the host-free companion role. Each idea is a card — slug leader plus the capability, what it unlocks, and the gap or modern technique it draws on — and spawns one or more tasks in `TASKLOG.md`. `[1]-[OPEN]` holds live concepts; `[2]-[CLOSED]` records dispositions so an idea is never re-litigated.

## [1]-[OPEN]

[STEP_AP242_COMPANION_HOP]: the `step-bridge` sub-domain owning the ISO 10303 AP242/AP203/AP214 + IGES CAD-STEP tessellation hop.
- Rides the exact two-hop companion shape the IFC daemon already uses: STEP/IGES bytes + tolerance → OCCT B-rep → GLB, content-addressed via the same runtime `ContentIdentity` key and served over the same gRPC contract; IFC and STEP collapse into one polymorphic `SourceFormat`-discriminated tessellation request rather than two parallel daemons.
- Unlocks closing the C# `StepIso10303` codec gap end-to-end, gives the TS viewer GLB for native CAD-STEP files, and lets one tessellation daemon serve both AEC (IFC) and mechanical (STEP/IGES) geometry through one content-addressed hop.
- Draws on the gap that the C# Bim/Exchange interchange declares `StepIso10303` with a pending CAD-reader slot and `tessellationRequiresCompanion=true` for every STEP/IGES row, with no Python owner; technique is OCCT `STEPControl_Reader`/`IGESControl_Reader` plus `BRepMesh` tessellation over `pythonocc-core` (admit to the branch manifest, or reuse the OCCT the `ifcopenshell` wheel already bundles), preferred over OCP whose tessellation drops normals.

[TENSOR_MULTIWAY_REGISTRATION]: deepen `scan-registration` to own the full modern registration stack under one mode-discriminated owner.
- The open3d tensor backend (`multi_scale_icp` with Tukey/Huber robust kernels, point-to-plane and colored estimators) for coarse-to-fine alignment, RANSAC+FPFH global registration for the initial pose, the `small_gicp` VGICP parallel speed path, and multiway pose-graph optimization for multi-station sessions.
- Unlocks production-grade as-built scan alignment from raw multi-station clouds with no manual initial pose, outlier-robust convergence, and a fitness/RMSE receipt graduating through the geometry case — the precondition for scan-vs-model deviation.
- Draws on the gap that pairwise legacy ICP alone leaves a multi-station session with no global bootstrap and no multiway optimization; technique is open3d `t.pipelines.registration.multi_scale_icp`, `registration_ransac_based_on_feature_matching` over FPFH, and the pose-graph `global_optimization` pattern.

[SCAN_TO_BIM_DEVIATION]: scan-vs-model deviation and primitive-extraction on top of registration.
- RANSAC plane/primitive segmentation (`segment_plane` plus region-growing oversegmentation) to extract walls/slabs/columns from a registered cloud, then nearest-surface deviation (max/mean/std distance) between the cloud and the IFC-tessellated GLB, emitting a per-element deviation receipt.
- Unlocks construction-verification and as-built QA: the companion answers whether the built geometry matches the IFC design within tolerance, feeding deviation evidence back through graduation to the C# owner system and the TS viewer as a colored overlay.
- Draws on the gap that the package tessellates IFC and registers scans but never compares them — the central AEC value of a host-free scan companion; technique is open3d `segment_plane` with supervoxel/region-growing oversegmentation and a point-to-mesh nearest-surface query against the companion's own GLB, producing element-keyed deviation rows.

[ROBUST_MESH_ALGEBRA]: the `mesh-utility` sub-domain owning robust mesh repair and boolean as a shared downstream primitive.
- Watertight detection plus hole-fill plus winding/normal repair, and exact boolean (union/difference/intersection) over the `trimesh` `manifold3d` backend, plus `rhino3dm`/`meshio` decode at the data seam — one polymorphic `MeshOp` owner the tessellation, scan-reconstruction, and step hops all compose.
- Unlocks a guaranteed-watertight, boolean-capable mesh substrate every other sub-domain reuses instead of re-implementing repair — clash-volume computation for `ifc-analysis`, solid cleanup for scan reconstruction, and valid GLB for the viewer.
- Draws on the gap that `trimesh`, `rhino3dm`, and `meshio` are admitted in the branch manifest but no page owns them, so reconstruction output is emitted raw with no watertight guarantee and boolean has no owner; technique is `trimesh.boolean` over the robust `manifold3d` backend (admit to the branch manifest; not the slow legacy path) and `trimesh.repair` winding/normal/hole-fill.

[IDS_BCF_MODEL_CHECK]: promote `ifc-analysis` to own the full buildingSMART validation triad via the IfcOpenShell ecosystem packages.
- IDS model-checking through `ifctester` (author plus validate IDS specifications with BCF report export), clash detection through `ifcclash` (filtered clash sets over the IFC query syntax), and BCF issue authoring/round-trip through the `bcf` library — one `AnalysisKind` owner whose rows produce BCF-serializable findings.
- Unlocks standards-conformant validation output (IDS results, BCF issues) the rest of the AEC toolchain and the C# owner system consume directly, instead of a bespoke non-portable rule fold, and folds clash/IDS/BCF into the same receipt-graduation rail.
- Draws on the gap that a local IDS rule-table fold and an OCCT bounding-overlap clash reimplement what `ifctester`/`ifcclash`/`bcf` already own natively against the buildingSMART IDS 1.0 and BCF 3.0 standards; technique is `ifctester.ids` for IDS authoring/validation, `ifcclash` for clash sets, and the `bcf` package for conformant issue exchange. The triad is three separate PyPI distributions of the IfcOpenShell/Bonsai ecosystem, not bundled with the `ifcopenshell` wheel, so all three admit to the branch manifest before the page's imports resolve.

## [2]-[CLOSED]

None.
