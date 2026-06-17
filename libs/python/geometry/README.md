# [PY_GEOMETRY]

`geometry` is the host-free geometry and IFC/BIM companion of the Python branch: the IfcOpenShell GLB tessellation daemon (the load-bearing cross-boundary two-hop the C# Bim/Compute rail and the TS viewer consume), IFC analysis and buildingSMART validation, point-cloud/3D-scan registration, non-manifold topology, and AEC computational geometry. It is a peer producer, never a Rasm consumer — it meets C# only at the wire (content-identity plus the GLB tessellation rail over the existing `ComputeService`/`ArtifactSync` gRPC contract) and graduates evidence through the compute `HandoffAxis` geometry case. This file routes the design pages and registers the external packages the folder uses; `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [1]-[PAGES]

The design pages under `.planning/`, one sub-domain folder per eventual source sub-tree. The `step-bridge` and `mesh-utility` sub-domains are planned and carry no page yet; `ARCHITECTURE.md` shows them as visible gaps.

- `.planning/tessellation/daemon.md` — the IfcOpenShell tessellation daemon: source bytes + tolerance into per-element GLB and a semantic header over the existing gRPC contract.
- `.planning/ifc-analysis/analysis.md` — IFC quantity/Pset/IDS/clash/space-program/BCF analysis over the IfcOpenShell ecosystem.
- `.planning/scan-registration/registration.md` — global, multi-scale tensor, VGICP, and multiway point-cloud registration.
- `.planning/topology/nonmanifold.md` — non-manifold `CellComplex`/`Cell`/`Aperture` topology over topologicpy.
- `.planning/computational-geometry/algebra.md` — compas network adjacency, form-finding, numerical primitives, and mesh algebra.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as a flat list; versions live in the one branch manifest. A `(admit)` tag marks a library a forward task draws on that the branch manifest does not yet declare — the task that needs it carries the manifest-admission as an integration step, so the registry never implies a phantom dependency.

- IFC and BIM: `ifcopenshell`; `ifctester`, `ifcclash`, `bcf` (admit — the buildingSMART validation triad, separate PyPI distributions, not bundled with the `ifcopenshell` wheel)
- Scan and point cloud: `open3d`, `small-gicp`, `laspy`, `pye57`, `pdal`
- Topology and computational geometry: `topologicpy`, `compas`; `compas_dr`, `compas_tna` (admit — the structural form-finding solvers)
- Mesh and CAD-STEP: `trimesh`, `rhino3dm`, `meshio`; `manifold3d` (admit — the robust exact-boolean backend), `pythonocc-core` (admit — the OCCT B-rep reader for the STEP/IGES hop)
- Numerics and typing: `numpy`, `msgspec`, `expression`, `beartype`
- Wire and runtime: `grpcio`, `grpcio-tools`, `protobuf`
