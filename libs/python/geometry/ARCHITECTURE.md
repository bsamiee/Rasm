# [PY_GEOMETRY_ARCHITECTURE]

The domain map of `geometry` — the host-free geometry and IFC/BIM companion and load-bearing cross-boundary owner. The `mesh` IfcOpenShell GLB tessellation daemon plus the `scan`, `ifc`, and `graph` domains.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

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
│   ├── selector.py           # IfcSelector: lark-validated selector/filter-query grammar admitting a structured query before filter_elements
│   ├── authoring.py          # IfcAuthoring: ifcopenshell spatial/element/geometry transactions, GUID + ownership-history rail (companion, ifcopenshell)
│   └── structural.py         # IfcStructural: Section integrals (A/I/centroid/torsion) over IfcProfileDef, sectionproperties enrichment row, companion layer
├── mesh/                     # IfcOpenShell GLB tessellation daemon (X-boundary owner), CAD-STEP hop, mesh algebra, B-rep evaluation, spatial query, etc...
│   ├── daemon.py             # TessellationDaemon: source bytes + tolerance → per-element GLB + semantic header, CAD arms route to cad.py
│   ├── cad.py                # StepBridge: STEP/IGES B-rep bytes + tolerance → GLB over OCCT XCAF, companion the C# StepIso10303 codec calls
│   ├── repair.py             # MeshRepairOp: watertight repair, winding/normal fix, manifold3d boolean; in-memory Trimesh in/out, mesh-file IO deferred to data MeshPayload
│   ├── brep.py               # BrepOp: cadquery-ocp B-rep evaluation, manifold3d solid algebra, mesh-brep subject
│   ├── spatial.py            # MeshSpatial: trimesh + numpy proximity/ray/contains/AABB-tree + clearance spatial query over in-memory triangulation (runtime spine; FCL CollisionManager clearance + manifold3d.min_gap enrichment worker, offloaded by find_spec)
│   └── quality.py            # MeshQuality: trimesh + numpy aspect-ratio/skewness/manifold-edge/genus mesh-quality metric receipts
├── graph/                    # Non-manifold topology over topologicpy and AEC computational geometry over compas, network analytics over networkx
│   ├── nonmanifold.py        # TopologyAlgebra: CellComplex/Cell/Aperture construction, decomposition, adjacency, dual-graph
│   ├── algebra.py            # ComputationalGeometry: network adjacency, form-finding, numerical primitives, mesh algebra
│   └── features.py           # GraphFeatures: networkx centrality/community/shortest-path/connectivity analytics over the network-graph projection
└── energy/                   # Out-of-process AGPL Ladybug Tools building-physics band: climate, HBJSON model, urban district, simulation egress
    ├── climate.py            # Climate: polymorphic EPW admission, DataCollection series algebra, Sunpath solar, PMV/UTCI/PET comfort + map rows
    ├── model.py              # BuildingModel: one HBJSON/BIM-to-BEM admission under one check_all gate, standards-resolved energy assignment, content-keyed HBJSON wire
    ├── district.py           # District: dragonfly dfjson/GeoJSON/massing admission, ordered auto-zoning, to_honeybee explosion, URBANopt/DES/OpenDSS/REopt translation rows
    └── simulate.py           # Simulation: offloaded OSM/IDF/epJSON/gbXML translation, runtime recipe binding, SQLiteResult/EUI decode into self-describing frames
```

## [02]-[SEAMS]

```text seams
*              →  csharp:Rasm.Compute          # [GRADUATION]: HandoffAxis geometry case IDS/clash/BCF
mesh           ⇄  csharp:Rasm.Bim/Exchange     # [TESSELLATION]: GLB tessellation rail / TessellationRequest
mesh/daemon    ⇄  csharp:Rasm.Compute/Runtime  # [CONTENT_KEY]: ContentIdentity XxHash128 source+deflection/tolerance policy-seed re-tessellation cache parity
mesh/daemon    ⇄  csharp:Rasm.Element/Graph    # [WIRE]: imported-IFC GLB seed-zero XxHash128 == seam RepresentationContentHash entry; decode the seam key.
mesh/daemon    ⇄  csharp:Rasm.Compute/Runtime  # [WIRE]: ComputeService/ArtifactSync gRPC GLB tessellation
mesh/daemon    →  csharp:Rasm.Compute/Runtime  # [TRANSPORT]: ServerHost ComputeService/ArtifactSync GLB + semantic header
mesh/daemon    →  csharp:Rasm.Compute/Runtime  # [PROJECTION]: IFC tessellation bridge via IfcOpenShell decoded by Codecs
ifc            ←  csharp:Rasm.Bim/Exchange     # [PROJECTION]: BimWire model vocabulary IFC ingest
mesh/daemon    →  csharp:Rasm.AppUi/Render     # [SHAPE]: SharpGLTF GLB import per-element tessellation
mesh/daemon    ←  csharp:Rasm.Bim/Model        # [SHAPE]: IFC GLB tessellation reference for deviation
ifc            →  csharp:Rasm.Bim/Review       # [BOUNDARY]: IDS validation evidence via ifctester
mesh/cad       →  python:runtime/evidence      # [CONTENT_KEY]: ContentIdentity.of keyed GLB bytes
mesh/daemon    →  python:runtime/evidence      # [CONTENT_KEY]: ContentIdentity.of source+policy-seed cache key; seed-zero (Some(0)) over the GLB bytes = wire representation key
graph/algebra  ⇄  python:compute/graduation    # [GRADUATION]: HandoffAxis geometry case
graph/features ⇄  python:compute/graduation    # [GRADUATION]: HandoffAxis geometry network-graph subject
ifc/analysis   →  python:compute/graduation    # [GRADUATION]: geometry HandoffAxis case IDS/clash/BCF
ifc/structural →  python:compute/graduation    # [GRADUATION]: geometry HandoffAxis numerical-primitive subject section integrals
mesh/daemon    ⇄  python:runtime/transport     # [WIRE]: ComputeService/ArtifactSync gRPC GLB tessellation
scan           →  python:data/spatial          # [SHAPE]: Arrow point-record columnar bridge x/y/z
mesh           ←  python:data/spatial          # [SHAPE]: MeshPayload cell-block topology
mesh/repair    →  python:data/spatial          # [BOUNDARY]: repair/brep/spatial/quality return in-memory Trimesh; mesh-file decode/encode + GLB preview,
mesh           ⇄  python:artifacts/figures     # [BOUNDARY]: visualization-scene/USD/GLTF/OBJ export is artifacts figures/scene, mesh-file codec is data
scan/ingestion ←  python:data/spatial          # [SHAPE]: COPC arm decode leaves the pdal filter-graph owner unchanged
energy/model   ⇄  csharp:Rasm.Bim/Exchange     # [WIRE]: content-keyed canonical HBJSON document bytes — the Energy exchange peers at document bytes, one XxHash128 derivation
energy/model   ←  csharp:Rasm.Bim/Exchange     # [SHAPE]: IFC SPF source bytes for the BIM-to-BEM derivation modality
energy/simulate →  python:runtime/recipe       # [PORT]: RecipeExecution/RecipeSpec — geometry binds Job/RecipeInterface schema, runtime owns execution
energy/simulate →  python:data/tabular         # [SHAPE]: self-describing result frames (output/unit/period/zone/step/value/content_key) over the columnar arrow_bytes fold
energy/*       →  graduation                   # [GRADUATION]: building-energy / thermal-comfort GeometryHandoff evidence
```

## [03]-[COMPANION_LANES]

Every sub-domain rides the companion engine selection the branch manifest owns, with compiled geometry/IFC cores and copyleft packages isolated at the process boundary.

The runtime lane carries `numpy`, `trimesh`, `rhino3dm`, `laspy`, and `networkx` for the `mesh/spatial`, `mesh/quality`, `graph/features`, and mesh/spatial spine owners. Worker lanes carry compiled enrichment rows such as `manifold3d`, `cadquery-ocp`, `compas`, `compas_dr`, `compas_tna`, `open3d`, `small-gicp`, `kiss-matcher`, `pye57`, and `sectionproperties`; `ifcopenshell` remains the worker IFC package behind `ifc/authoring`, `ifc/structural`, and the `energy/model` BIM-to-BEM derivation arm.

The AGPL Ladybug Tools band (`ladybug-core`/`ladybug-geometry`/`ladybug-comfort`, `honeybee-core`/`honeybee-energy`/`honeybee-openstudio` + the two standards data backends, `dragonfly-core`/`dragonfly-energy`) rides the `energy/` owners with strictly function-local boundary imports and process-boundary evidence exchange — HBJSON/dfjson/EPW document bytes and result frames across the wire, never a distributed link. The simulation engines (Radiance/OpenStudio/EnergyPlus behind the runtime recipe rail; URBANopt/Modelica/RNM/REopt behind the district translation rows) are external process-boundary services.
