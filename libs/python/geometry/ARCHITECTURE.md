# [PY_GEOMETRY_ARCHITECTURE]

The professional-domain folder-map for `geometry`: the host-free geometry and IFC/BIM companion. The map is the full sub-domain structure, each named by its real domain concept with a one-line charter. The tessellation companion is the load-bearing cross-boundary owner; the analysis, scan, topology, computational-geometry, step-bridge, and mesh-utility domains ride alongside it, each carrying a design page. Dependency direction across the Python packages lives once in the branch `ARCHITECTURE.md`; the wires each sub-domain crosses live on the tasks that build them, never in a folder seam ledger.

## [1]-[DOMAIN_MAP]

The sub-domain layout mirrors the eventual source tree, one folder per professional concept.

```text codemap
geometry/
├── tessellation/             # IfcOpenShell GLB tessellation daemon — the load-bearing cross-boundary owner
│   └── daemon.py             # TessellationDaemon: source bytes + tolerance → per-element GLB + semantic header, CAD arms route step-bridge
├── ifc-analysis/             # IFC property/quantity/relationship analysis + buildingSMART validation + 5D/4D lifecycle
│   ├── analysis.py           # IfcAnalysis: Pset, IDS, clash, space-program, BCF over the IfcOpenShell ecosystem
│   ├── costing.py            # IfcLifecycle: ifc5d 5D quantity take-off + cost rollup, ifc4d scheduling, ifcpatch recipes, ifcdiff revision diff
│   └── selector.py           # IfcSelector: the lark-validated selector/filter-query grammar admitting a structured query before filter_elements
├── scan-registration/        # point-cloud and 3D-scan ingestion, registration, deviation, and reconstruction
│   ├── ingestion.py          # ScanIngestion: pdal filter-graph (ground/outlier/decimate) + laspy COPC/E57 into a registration-ready PointCloud
│   ├── registration.py       # ScanRegistration: kiss_matcher global, tensor multi_scale_icp, colored, VGICP, multiway pose-graph
│   ├── deviation.py          # ScanDeviation: RANSAC plane/primitive segmentation + nearest-surface deviation against the IFC GLB
│   └── reconstruction.py     # ScanReconstruction: open3d Poisson/ball-pivoting/alpha-shape registered-cloud-to-watertight-mesh, reconstructed-mesh subject
├── topology/                 # non-manifold topological modeling over topologicpy
│   └── nonmanifold.py        # TopologyAlgebra: CellComplex/Cell/Aperture construction, decomposition, adjacency, dual-graph
├── computational-geometry/   # AEC computational and numerical geometry over compas
│   └── algebra.py            # ComputationalGeometry: network adjacency, form-finding, numerical primitives, mesh algebra
├── step-bridge/              # ISO 10303 AP242/AP203/AP214 + IGES CAD-STEP tessellation hop over cadquery-ocp
│   └── cad.py                # StepBridge: STEP/IGES B-rep bytes + tolerance → GLB over OCCT XCAF, the companion the C# StepIso10303 codec calls
└── mesh-utility/             # robust mesh algebra shared by tessellation, scan, and step hops
    └── repair.py             # MeshOp: watertight repair, winding/normal fix, manifold3d boolean, mesh-file codec
```

## [2]-[CHARTERS]

- `tessellation`: the persistent IfcOpenShell tessellation companion daemon — source bytes plus a deflection/tolerance policy into per-element GLB and a lightweight semantic header through the `geom.iterator` and the native `geom.serializers.gltf`, served through the runtime `ServerHost` over the existing C# `ComputeService`/`ArtifactSync` contract and content-addressed via runtime `ContentIdentity`.
- `ifc-analysis`: IFC property/relationship analysis, standards-conformant validation, and the 5D/4D model lifecycle — Pset queries, IDS model-checking, clash detection, space-program validation, and BCF round-trip over `ifcopenshell.util`, `ifctester`, `ifcclash`, and `bcf` (`analysis.py`); rule-driven 5D quantity take-off and cost-schedule rollup over `ifc5d`, 4D construction scheduling over `ifc4d`, recipe-driven model transformation over `ifcpatch`, and two-model revision comparison over `ifcdiff` (`costing.py`); and the `lark`-validated selector/filter-query grammar that admits a structured query before `util.selector.filter_elements` (`selector.py`). Every verb emits evidence that graduates through the compute `HandoffAxis` geometry case.
- `scan-registration`: raw-scan ingestion, point-cloud registration, scan-vs-model deviation, and surface reconstruction — `ingestion.py` runs the `pdal` filter-graph (SMRF/PMF ground classification, statistical/radius outlier removal, voxel/decimation downsampling) plus `laspy` COPC and `pye57` E57 ingestion into a registration-ready `o3d.t.geometry.PointCloud`, consuming the data-branch columnar point-record bridge; `registration.py` runs the `kiss_matcher` initialization-free global bootstrap, coarse-to-fine `multi_scale_icp` over the open3d tensor backend with robust kernels, the `small_gicp` VGICP speed path, and multiway pose-graph optimization; `deviation.py` extracts planar primitives over `segment_plane` and computes nearest-surface deviation against the IFC-tessellated GLB over `trimesh.proximity`; `reconstruction.py` reconstructs a watertight mesh from the registered cloud over open3d Poisson/ball-pivoting/alpha-shape with normal estimation, producing the `reconstructed-mesh` graduation subject the `mesh-utility` repair owner then conditions. Transforms, deviation rows, and reconstructed meshes graduate through the geometry case.
- `topology`: non-manifold topological modeling over `topologicpy` — `CellComplex`/`Cell`/`Aperture` construction, decomposition, and adjacency/dual-graph extraction the C# `IfcSemanticModel` spatial projection does not perform.
- `computational-geometry`: AEC computational and numerical geometry over `compas` — graph/network adjacency, form-finding over `compas_dr`/`compas_tna`, best-fit/boolean/bbox numerical primitives, and mesh datastructure algebra.
- `step-bridge`: the ISO 10303 AP242/AP203/AP214 and IGES CAD-STEP tessellation hop riding the same two-hop companion shape as IFC — STEP/IGES B-rep bytes plus tolerance into GLB over the `cadquery-ocp` OCCT XCAF reader (`STEPCAFControl_Reader`/`IGESCAFControl_Reader` → `BRepMesh_IncrementalMesh` → `RWGltf_CafWriter`), returning raw GLB the daemon keys, aligned at the wire to the C# `StepIso10303` codec which requests CAD tessellation from this companion rather than re-implementing a managed reader.
- `mesh-utility`: robust mesh algebra the tessellation, scan, and step hops consume downstream — watertight repair, winding/normal fixing, exact boolean over the `trimesh` `manifold3d` backend, and `rhino3dm`/`meshio` mesh-file decode/encode at the data seam, as one `MeshOp` tagged union.

## [3]-[INTERPRETER_FLOOR]

Every sub-domain rides the companion interpreter floor the branch manifest owns — the sanctioned divergence from the Python core floor, forced by the compiled geometry/IFC cores and isolating the copyleft IFC wheel at the process boundary. The folder consumes this floor as settled and never re-decides it; it surfaces here only because the whole map sits below it.
