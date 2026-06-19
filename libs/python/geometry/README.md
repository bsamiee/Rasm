# [PY_GEOMETRY]

`geometry` is the host-free geometry and IFC/BIM companion of the Python branch: the IfcOpenShell GLB tessellation daemon (the load-bearing cross-boundary two-hop the C# Bim/Compute rail and the TS viewer consume), IFC analysis and buildingSMART validation, point-cloud/3D-scan registration, non-manifold topology, and AEC computational geometry. It is a peer producer, never a Rasm consumer — it meets C# only at the wire (content-identity plus the GLB tessellation rail over the existing `ComputeService`/`ArtifactSync` gRPC contract) and graduates evidence through the compute `HandoffAxis` geometry case. This file routes the design pages and registers the external packages the folder uses; `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [1]-[ROUTER]

The design pages under `.planning/`, one sub-domain folder per eventual source sub-tree. Every sub-domain carries a page.

- `.planning/tessellation/daemon.md` — the IfcOpenShell tessellation daemon: source bytes + tolerance into per-element GLB and a semantic header over the existing gRPC contract, routing CAD source formats to the step-bridge hop.
- `.planning/ifc-analysis/analysis.md` — IFC quantity/Pset/IDS/clash/space-program/BCF analysis over the IfcOpenShell ecosystem.
- `.planning/ifc-analysis/costing.md` — the 5D/4D model-lifecycle owner: `ifc5d` rule-driven quantity take-off + cost-schedule rollup, `ifc4d` construction scheduling, `ifcpatch` recipe transformation, and `ifcdiff` revision comparison.
- `.planning/ifc-analysis/selector.md` — the `lark`-validated IFC selector/filter-query grammar that admits a structured query before `util.selector.filter_elements`.
- `.planning/scan-registration/ingestion.md` — the raw-scan preprocessing owner: `pdal` filter-graph (ground classification, outlier removal, decimation) + `laspy` COPC/E57 ingestion into a registration-ready `o3d.t.geometry.PointCloud`.
- `.planning/scan-registration/registration.md` — `kiss_matcher` global, multi-scale tensor, colored, VGICP, and multiway point-cloud registration.
- `.planning/scan-registration/deviation.md` — RANSAC plane/primitive segmentation and scan-vs-model nearest-surface deviation against the IFC-tessellated GLB.
- `.planning/scan-registration/reconstruction.md` — the registered-cloud-to-watertight-mesh reconstruction owner over open3d Poisson/ball-pivoting/alpha-shape + normal estimation, producing the `reconstructed-mesh` graduation subject.
- `.planning/topology/nonmanifold.md` — non-manifold `CellComplex`/`Cell`/`Aperture` topology over topologicpy.
- `.planning/computational-geometry/algebra.md` — compas network adjacency, form-finding, numerical primitives, and mesh algebra.
- `.planning/step-bridge/cad.md` — the ISO 10303 AP242/AP203/AP214 + IGES CAD-STEP tessellation hop over cadquery-ocp, the companion the C# `StepIso10303` codec calls.
- `.planning/mesh-utility/repair.md` — the `MeshOp` robust mesh repair, exact boolean, and mesh-file codec owner over trimesh/manifold3d/rhino3dm/meshio.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list; versions live in the one branch manifest. Two provenances carry these rows: a **Forge companion lane** row is provided by the Forge `<'3.13'` companion interpreter (python312, source-building the companion native libs) and is NOT declared in the cp315 `pyproject.toml`; every other row is declared in the Python manifest as cp315-clean core or as the `python_version<'3.15'` gated band. A `(admit)` tag marks a library a forward task still draws on that no provenance yet provides — the task carries the admission as an integration step, so the registry never implies a phantom dependency.

- IFC and BIM: `ifcopenshell` (Forge companion lane); `ifctester`, `ifcclash`, `bcf-client` (the buildingSMART validation triad, separate PyPI distributions; `ifctester`/`ifcclash` Forge companion lane, `bcf-client` imports `bcf` and ships a pure-Python wheel); `ifc5d`, `ifc4d`, `ifcpatch`, `ifcdiff` (the IfcOpenShell 5D-costing/4D-scheduling/recipe-transformation/model-diff ecosystem siblings, `0.8.5` companion-lane distributions over the `ifcopenshell` core); `lark` (manifest, cp315-clean — the IDS/selector parsing-grammar engine)
- Scan and point cloud: `open3d`, `small-gicp` (Forge companion lane); `kiss-matcher` (manifest, gated `<'3.13'` — global initialization-free registration, cp38-cp312 wheels only); `laspy`, `pye57`, `pdal` (manifest — `pye57`/`pdal` gated `<'3.15'`; `pdal` the scan-ingestion filter-graph engine)
- Topology and computational geometry: `topologicpy` (Forge companion lane); `compas`, `compas_dr`, `compas_tna` (manifest, gated `<'3.15'`)
- Mesh and CAD-STEP: `trimesh`, `rhino3dm`, `meshio` (manifest, cp315-clean); `manifold3d` (manifest, gated `<'3.15'` — robust exact-boolean backend); `cadquery-ocp` (manifest, gated `<'3.15'` — the OCCT B-rep kernel and XCAF assembly model for the STEP/IGES hop, binary-wheel `OCP` namespace, cp310-cp314 wheels only)

## [3]-[CROSS_CUTTING]

Branch-wide infrastructure packages this folder consumes; canonical registry lives at `libs/python/.api/`.

- expression
- beartype
- msgspec
- numpy
- grpcio
- grpcio-tools
- protobuf
