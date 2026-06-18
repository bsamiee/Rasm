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

Every external library the folder uses, planned or implemented, as a flat list; versions live in the one branch manifest. Two provenances carry these rows: a **Forge companion lane** row is provided by the Forge `<'3.13'` companion interpreter (python312, source-building the companion native libs) and is NOT declared in the cp315 `pyproject.toml`; every other row is declared in the Python manifest as cp315-clean core or as the `python_version<'3.15'` gated band. A `(admit)` tag marks a library a forward task still draws on that no provenance yet provides — the task carries the admission as an integration step, so the registry never implies a phantom dependency.

- IFC and BIM: `ifcopenshell` (Forge companion lane); `ifctester`, `ifcclash`, `bcf` (admit — the buildingSMART validation triad, separate PyPI distributions, Forge companion lane)
- Scan and point cloud: `open3d`, `small-gicp` (Forge companion lane); `laspy`, `pye57`, `pdal` (manifest — `pye57`/`pdal` gated `<'3.15'`)
- Topology and computational geometry: `topologicpy` (Forge companion lane); `compas`, `compas_dr`, `compas_tna` (manifest, gated `<'3.15'`)
- Mesh and CAD-STEP: `trimesh`, `rhino3dm`, `meshio` (manifest, cp315-clean); `manifold3d` (manifest, gated `<'3.15'` — robust exact-boolean backend); `pythonocc-core` (admit — the OCCT B-rep reader for the STEP/IGES hop; no PyPI distribution, conda-only, Forge companion lane or deferred)
- Numerics and typing: `numpy`, `msgspec`, `expression`, `beartype` (manifest, cp315-clean)
- Wire and runtime: `grpcio`, `grpcio-tools`, `protobuf` (Forge companion lane; also present transitively via `specklepy` on cp315)
