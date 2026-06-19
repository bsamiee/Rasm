# [PY_GEOMETRY]

`geometry` is the host-free geometry and IFC/BIM companion of the Python branch. It owns the IfcOpenShell GLB tessellation daemon — the load-bearing cross-boundary two-hop that the C# Bim/Compute rail and the TS viewer consume — along with IFC analysis and buildingSMART validation, point-cloud/3D-scan registration, non-manifold topology, and AEC computational geometry. It is a peer producer, never a Rasm consumer: it meets C# only at the wire via content-identity plus the GLB tessellation rail over the `ComputeService`/`ArtifactSync` gRPC contract, and graduates evidence through the compute `HandoffAxis` geometry case. This file routes the design pages and registers the external packages the folder uses; `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [1]-[ROUTER]

- [1]-[INGESTION](.planning/scan/ingestion.md): Raw-scan preprocessing: `pdal` filter-graph (ground classification, outlier removal, decimation) and `laspy` COPC/E57 ingestion into a registration-ready `o3d.t.geometry.PointCloud`.
- [2]-[REGISTRATION](.planning/scan/registration.md): `kiss_matcher` global, multi-scale tensor, colored, VGICP, and multiway point-cloud registration.
- [3]-[DEVIATION](.planning/scan/deviation.md): RANSAC plane/primitive segmentation and scan-vs-model nearest-surface deviation against the IFC-tessellated GLB.
- [4]-[RECONSTRUCTION](.planning/scan/reconstruction.md): Registered-cloud-to-watertight-mesh reconstruction over open3d Poisson/ball-pivoting/alpha-shape and normal estimation, producing the `reconstructed-mesh` graduation subject.
- [5]-[ANALYSIS](.planning/ifc/analysis.md): IFC quantity/Pset/IDS/clash/space-program/BCF analysis over the IfcOpenShell ecosystem.
- [6]-[COSTING](.planning/ifc/costing.md): 5D/4D model-lifecycle owner: `ifc5d` rule-driven quantity take-off and cost-schedule rollup, `ifc4d` construction scheduling, `ifcpatch` recipe transformation, and `ifcdiff` revision comparison.
- [7]-[SELECTOR](.planning/ifc/selector.md): `lark`-validated IFC selector/filter-query grammar that admits a structured query before `util.selector.filter_elements`.
- [8]-[DAEMON](.planning/mesh/daemon.md): IfcOpenShell tessellation daemon: source bytes and tolerance into per-element GLB and a semantic header over the existing gRPC contract, routing CAD source formats to the `mesh/cad.md` STEP hop.
- [9]-[CAD](.planning/mesh/cad.md): ISO 10303 AP242/AP203/AP214 and IGES CAD-STEP tessellation hop over `cadquery-ocp`, the companion the C# `StepIso10303` codec calls.
- [10]-[REPAIR](.planning/mesh/repair.md): `MeshOp` robust mesh repair, exact boolean, and mesh-file codec owner over `trimesh`/`manifold3d`/`rhino3dm`/`meshio`.
- [11]-[NONMANIFOLD](.planning/graph/nonmanifold.md): Non-manifold `CellComplex`/`Cell`/`Aperture` topology over `topologicpy`.
- [12]-[ALGEBRA](.planning/graph/algebra.md): `compas` network adjacency, form-finding, numerical primitives, and mesh algebra.

## [2]-[DOMAIN_PACKAGES]

Every domain library the folder uses, planned or implemented. Versions are centralized in the one Python branch manifest and never pinned here; substrate packages live in `[3]-[SUBSTRATE_PACKAGES]` below. Two provenances carry these rows: a Forge companion lane row is provided by the Forge `<'3.13'` companion interpreter (python312, source-building the companion native libs) and is NOT declared in the cp315 `pyproject.toml`; every other row is declared in the Python manifest as cp315-clean core or as the `python_version<'3.15'` gated band. A `(admit)` tag marks a library a forward task still draws on that no provenance yet provides — the task carries the admission as an integration step, so the registry never implies a phantom dependency.

[IFC_BIM]:
- `ifcopenshell` (Forge companion lane)
- `ifctester` (buildingSMART IDS validation, Forge companion lane)
- `ifcclash` (BCF-producing clash detection, Forge companion lane)
- `bcf-client` (buildingSMART BCF I/O; pure-Python wheel, imports `bcf`)
- `ifc5d` (rule-driven quantity take-off and cost-schedule rollup; `0.8.5` companion lane over `ifcopenshell`)
- `ifc4d` (construction scheduling; companion lane)
- `ifcpatch` (recipe transformation; companion lane)
- `ifcdiff` (model revision comparison; companion lane)
- `lark` (manifest, cp315-clean — IDS/selector parsing-grammar engine)

[SCAN_POINT_CLOUD]:
- `open3d` (Forge companion lane)
- `small-gicp` (Forge companion lane)
- `kiss-matcher` (manifest, gated `<'3.13'` — global initialization-free registration, cp38-cp312 wheels only)
- `laspy` (manifest — COPC/LAS I/O)
- `pye57` (manifest, gated `<'3.15'` — E57 I/O)
- `pdal` (manifest, gated `<'3.15'` — scan-ingestion filter-graph engine)

[TOPOLOGY]:
- `topologicpy` (Forge companion lane)
- `compas` (manifest, gated `<'3.15'`)
- `compas_dr` (manifest, gated `<'3.15'`)
- `compas_tna` (manifest, gated `<'3.15'`)

[MESH_CAD]:
- `trimesh` (manifest, cp315-clean)
- `rhino3dm` (manifest, cp315-clean)
- `meshio` (manifest, cp315-clean)
- `manifold3d` (manifest, gated `<'3.15'` — robust exact-boolean backend)
- `cadquery-ocp` (manifest, gated `<'3.15'` — OCCT B-rep kernel and XCAF assembly model for the STEP/IGES hop, binary-wheel `OCP` namespace, cp310-cp314 wheels only)

## [3]-[SUBSTRATE_PACKAGES]

Branch-wide substrate libraries this folder consumes; canonical registry and API evidence live in `libs/python/.planning/README.md` and the adjacent `libs/python/.api/`.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`
- `pydantic`

[CONCURRENCY]:
- `anyio`

[NUMERIC_SUBSTRATE]:
- `numpy`

[WIRE_CODEGEN]:
- `grpcio`
- `grpcio-tools`
- `protobuf`
