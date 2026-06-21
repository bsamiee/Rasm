# [PY_GEOMETRY]

`geometry` is the host-free geometry and IFC/BIM companion of the Python branch. It owns the IfcOpenShell GLB tessellation daemon — the load-bearing cross-boundary two-hop that the C# Bim/Compute rail and the TS viewer consume — along with IFC analysis and buildingSMART validation, point-cloud/3D-scan registration, non-manifold topology, and AEC computational geometry. It is a peer producer, never a Rasm consumer: it meets C# only at the wire via content-identity plus the GLB tessellation rail over the `ComputeService`/`ArtifactSync` gRPC contract, and graduates evidence through the compute `HandoffAxis` geometry case. This file routes the design pages and registers the external packages the folder uses; `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[INGESTION](.planning/scan/ingestion.md): Raw-scan preprocessing: `pdal` filter-graph (ground classification, outlier removal, decimation) and `laspy` COPC/E57 ingestion into a registration-ready `o3d.t.geometry.PointCloud`.
- [02]-[REGISTRATION](.planning/scan/registration.md): `kiss_matcher` global, multi-scale tensor, colored, VGICP, and multiway point-cloud registration.
- [03]-[DEVIATION](.planning/scan/deviation.md): RANSAC plane/primitive segmentation and scan-vs-model nearest-surface deviation against the IFC-tessellated GLB.
- [04]-[RECONSTRUCTION](.planning/scan/reconstruction.md): Registered-cloud-to-watertight-mesh reconstruction over open3d Poisson/ball-pivoting/alpha-shape and normal estimation, producing the `reconstructed-mesh` graduation subject.
- [05]-[ANALYSIS](.planning/ifc/analysis.md): IFC quantity/Pset/IDS/clash/space-program/BCF analysis over the IfcOpenShell ecosystem.
- [06]-[COSTING](.planning/ifc/costing.md): 5D/4D model-lifecycle owner: `ifc5d` rule-driven quantity take-off and cost-schedule rollup, `ifc4d` construction scheduling, `ifcpatch` recipe transformation, and `ifcdiff` revision comparison.
- [07]-[SELECTOR](.planning/ifc/selector.md): `lark`-validated IFC selector/filter-query grammar that admits a structured query before `util.selector.filter_elements`.
- [08]-[STRUCTURAL](.planning/ifc/structural.md): numpy closed-form section integrals (area, centroid, second moments, principal axes, thin-walled torsion) over `IfcProfileDef` as the cp315-clean spine, tiered with the `ifcopenshell` structural-entity companion layer and the gated `sectionproperties` warping/plastic enrichment, producing the `numerical-primitive` graduation subject.
- [09]-[DAEMON](.planning/mesh/daemon.md): IfcOpenShell tessellation daemon: source bytes and tolerance into per-element GLB and a semantic header over the existing gRPC contract, routing CAD source formats to the `mesh/cad.md` STEP hop.
- [10]-[CAD](.planning/mesh/cad.md): ISO 10303 AP242/AP203/AP214+IGES CAD-STEP tessellation hop over `cadquery-ocp`, companion to C# `StepIso10303` codec.
- [11]-[REPAIR](.planning/mesh/repair.md): `MeshOp` robust mesh repair, exact boolean, and mesh-file codec owner over `trimesh`/`manifold3d`/`rhino3dm`/`meshio`.
- [12]-[NONMANIFOLD](.planning/graph/nonmanifold.md): Non-manifold `CellComplex`/`Cell`/`Aperture` topology over `topologicpy`.
- [13]-[ALGEBRA](.planning/graph/algebra.md): `compas` network adjacency, form-finding, numerical primitives, and mesh algebra.

## [02]-[DOMAIN_PACKAGES]

Every domain library the folder uses, planned or implemented. Versions are centralized in the one Python branch manifest and never pinned here; substrate packages live in `[3]-[SUBSTRATE_PACKAGES]` below. Two provenances carry these rows: a Forge companion lane row is provided by the Forge `<'3.13'` companion interpreter (python312, source-building the companion native libs) and is NOT declared in the cp315 `pyproject.toml`; every other row is declared in the Python manifest as cp315-clean core or as the `python_version<'3.15'` gated band. A `(admit)` tag marks a library a forward task still draws on that no provenance yet provides — the task carries the admission as an integration step, so the registry never implies a phantom dependency.

[IFC_BIM]:
- `ifcopenshell` - Forge companion lane — `0.8.5` py313 spine
- `ifctester` - BuildingSMART IDS validation, Forge companion lane
- `ifcclash` - BCF-producing clash detection, Forge companion lane
- `bcf-client` - BuildingSMART BCF I/O; pure-Python wheel, imports `bcf`
- `ifc5d` - Rule-driven quantity take-off and cost-schedule rollup; `0.8.5` companion lane over `ifcopenshell`
- `ifc4d` - Construction scheduling; companion lane
- `ifcpatch` - Recipe transformation; companion lane
- `ifcdiff` - Model revision comparison; companion lane
- `lark` - Manifest, cp315-clean — IDS/selector parsing-grammar engine

[SCAN_POINT_CLOUD]:
- `open3d` - Forge companion lane — `cp312`-max companion floor
- `small-gicp` - Forge companion lane, gated `<'3.15'` — no cp315 wheel
- `kiss-matcher` - Manifest, gated `<'3.13'` — global initialization-free registration, cp38-cp312 wheels only, FGR fallback below floor
- `laspy` - Manifest — COPC/LAS I/O
- `pye57` - Manifest, gated `<'3.15'` — E57 I/O
- `pdal` - Manifest, gated `<'3.15'` — scan-ingestion filter-graph engine

[TOPOLOGY]:
- `topologicpy` - Forge companion lane — AGPL-3.0 network-copyleft, excluded from the default server build
- `compas` - Manifest, gated `<'3.15'`
- `compas_dr` - Manifest, gated `<'3.15'`
- `compas_tna` - Manifest, gated `<'3.15'`

[MESH_CAD]:
- `trimesh` - Manifest, cp315-clean
- `rhino3dm` - Manifest, cp315-clean
- `meshio` - Manifest, cp315-clean
- `manifold3d` - Manifest, gated `<'3.15'` — robust exact-boolean backend
- `cadquery-ocp` - Manifest, gated `<'3.15'` — OCCT B-rep kernel and XCAF assembly model for the STEP/IGES hop, binary-wheel `OCP` namespace, cp310-cp314 wheels only
- `sectionproperties` - Manifest, gated `<'3.15'` — `ifc/structural` warping/plastic/shear enrichment row only (`cytriangle` LGPLv3 native mesh backend), never the section-integral spine
- `gmsh` - Deferred (not admitted by any provenance)

## [03]-[SUBSTRATE_PACKAGES]

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
