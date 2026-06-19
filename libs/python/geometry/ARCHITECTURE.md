# [PY_GEOMETRY_ARCHITECTURE]

The domain map of `geometry` — the host-free geometry and IFC/BIM companion and load-bearing cross-boundary owner. The `mesh` IfcOpenShell GLB tessellation daemon plus the `scan`, `ifc`, and `graph` domains.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
geometry/
├── scan/                     # Point-cloud and 3D-scan ingestion, registration, deviation, and reconstruction
│   ├── ingestion.py          # ScanIngestion: pdal filter-graph (ground/outlier/decimate) + laspy COPC/E57 into a registration-ready PointCloud
│   ├── registration.py       # ScanRegistration: kiss_matcher global, tensor multi_scale_icp, colored, VGICP, multiway pose-graph
│   ├── deviation.py          # ScanDeviation: RANSAC plane/primitive segmentation + nearest-surface deviation against IFC GLB
│   └── reconstruction.py     # ScanReconstruction: open3d Poisson/ball-pivoting/alpha-shape registered-cloud-to-watertight-mesh, reconstructed-mesh subject
├── ifc/                      # IFC property/quantity/relationship analysis + buildingSMART validation + 5D/4D lifecycle
│   ├── analysis.py           # IfcAnalysis: Pset, IDS, clash, space-program, BCF over IfcOpenShell ecosystem
│   ├── costing.py            # IfcLifecycle: ifc5d 5D quantity take-off + cost rollup, ifc4d scheduling, ifcpatch recipes, ifcdiff revision diff
│   └── selector.py           # IfcSelector: lark-validated selector/filter-query grammar admitting a structured query before filter_elements
├── mesh/                     # IfcOpenShell GLB tessellation daemon ( load-bearing cross-boundary owner), CAD-STEP hop, and robust mesh algebra
│   ├── daemon.py             # TessellationDaemon: source bytes + tolerance → per-element GLB + semantic header, CAD arms route to cad.py
│   ├── cad.py                # StepBridge: STEP/IGES B-rep bytes + tolerance → GLB over OCCT XCAF, companion the C# StepIso10303 codec calls
│   └── repair.py             # MeshOp: watertight repair, winding/normal fix, manifold3d boolean, mesh-file codec
└── graph/                    # Non-manifold topology over topologicpy and AEC computational geometry over compas
    ├── nonmanifold.py        # TopologyAlgebra: CellComplex/Cell/Aperture construction, decomposition, adjacency, dual-graph
    └── algebra.py            # ComputationalGeometry: network adjacency, form-finding, numerical primitives, mesh algebra
```

## [2]-[SEAMS]

```text seams
*              →  csharp:Rasm.Compute          # [GRADUATION]: HandoffAxis geometry case IDS/clash/BCF
mesh           ⇄  csharp:Rasm.Bim/Exchange     # [TESSELLATION]: GLB tessellation rail / TessellationRequest
mesh/daemon    ⇄  csharp:Rasm.Compute/Runtime  # [WIRE]: ComputeService/ArtifactSync gRPC GLB tessellation
mesh/daemon    →  csharp:Rasm.Compute/Runtime  # [TRANSPORT]: ServerHost ComputeService/ArtifactSync GLB + semantic header
ifc            ←  csharp:Rasm.Bim/Exchange     # [PROJECTION]: BimWire model vocabulary IFC ingest
mesh/daemon    →  csharp:Rasm.AppUi/Render     # [SHAPE]: SharpGLTF GLB import per-element tessellation
mesh/daemon    ←  csharp:Rasm.Bim/Model        # [SHAPE]: IFC GLB tessellation reference for deviation
ifc            →  csharp:Rasm.Bim/Review       # [BOUNDARY]: IDS validation evidence via ifctester
mesh/cad       →  python:runtime/evidence      # [CONTENT_KEY]: ContentIdentity.of keyed GLB bytes
mesh/daemon    →  python:runtime/evidence      # [CONTENT_KEY]: ContentIdentity.of keyed GLB bytes with policy seed
graph/algebra  ⇄  python:compute/graduation    # [GRADUATION]: HandoffAxis geometry case
ifc/analysis   →  python:compute/graduation    # [GRADUATION]: geometry HandoffAxis case IDS/clash/BCF
mesh/daemon    ⇄  python:runtime/transport     # [WIRE]: ComputeService/ArtifactSync gRPC GLB tessellation
scan           →  python:data/spatial          # [SHAPE]: Arrow point-record columnar bridge x/y/z
mesh           ←  python:data/spatial          # [SHAPE]: MeshPayload cell-block topology
```

## [3]-[INTERPRETER_FLOOR]

Every sub-domain rides the companion interpreter floor the branch manifest owns — the sanctioned divergence from the Python core floor, forced by the compiled geometry/IFC cores and isolating the copyleft IFC wheel at the process boundary. The folder consumes this floor as settled and never re-decides it; it surfaces here only because the whole map sits below it.
