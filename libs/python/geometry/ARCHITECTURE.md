# [PY_GEOMETRY_ARCHITECTURE]

The domain map of `geometry` — the host-free geometry and IFC/BIM companion and load-bearing cross-boundary owner. The `graduation` evidence spine, the `mesh` IfcOpenShell GLB tessellation daemon and its `serve` wire owner, plus the `scan`, `ifc`, `graph`, and `energy` domains.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
geometry/
├── graduation.py             # GeometrySubject union + GeometryHandoff carrier + evidence_run weave: the tier-0 spine every producer composes
├── scan/                     # Point-cloud and 3D-scan ingestion, registration, deviation, and reconstruction
│   ├── ingestion.py          # ScanIngestion: pdal filter-graph (ground/outlier/decimate, streaming iterator) + pye57 E57 station provenance; LAS/COPC decode at the data seam
│   ├── registration.py       # ScanRegistration: kiss_matcher global (open3d FGR fallback), tensor multi_scale_icp, colored, VGICP, multiway pose-graph
│   ├── deviation.py          # ScanDeviation: RANSAC plane/primitive segmentation + signed nearest-surface deviation against the content-keyed IFC GLB, quality-gated watertight
│   └── reconstruction.py     # ScanReconstruction: open3d Poisson/ball-pivoting/alpha-shape registered-cloud-to-watertight-mesh, closure-graded through mesh/quality
├── ifc/                      # IFC property/quantity/relationship analysis + buildingSMART validation + 5D/4D lifecycle
│   ├── analysis.py           # IfcAnalysis: Pset, IDS (entity + Json-report roll-up), clash, space-program, BCF with OCC snapshots; bim-compliance subject
│   ├── costing.py            # IfcLifecycle: ifc5d 5D quantity take-off + cost rollup, ifc4d scheduling, ifcpatch recipes, ifcdiff revision diff; bim-lifecycle subject
│   ├── selector.py           # IfcSelector: lark-validated selector/filter-query grammar admitting a structured query before filter_elements
│   ├── authoring.py          # IfcAuthoring: ifcopenshell spatial/element/geometry transactions, GUID + ownership-history rail (companion, ifcopenshell)
│   └── structural.py         # IfcStructural: Section integrals (A/I/centroid/torsion) over IfcProfileDef + the realized warping/plastic/shear FE tier; section-property subject
├── mesh/                     # IfcOpenShell GLB tessellation daemon (X-boundary owner), the serve wire owner, CAD-STEP hop, mesh algebra, B-rep evaluation, spatial query
│   ├── daemon.py             # TessellationDaemon: source bytes + TessellationPolicy → RETURNED RuntimeRail[Block[TessellationResult]] per-element GLB + semantic header + replay phase
│   ├── serve.py              # GeometryServe: the servicer registered in the runtime ServerHost — TessellationRequest decode, receipt field floor, 64 KiB Crc32-framed ArtifactSync GLB stream
│   ├── cad.py                # StepBridge: STEP/IGES B-rep bytes → GLB over OCCT XCAF; the TessellationPolicy mesher-knob mint the daemon/brep import downward
│   ├── repair.py             # MeshRepairOp: watertight repair, winding/normal fix, manifold3d boolean; the PUBLIC to_manifold uint32-ceiling kernel; mesh-file IO is the data MeshPayload seam
│   ├── brep.py               # BrepOp: cadquery-ocp B-rep evaluation — BOPAlgo booleans (fuzzy/history), sew/NURBS conditioning, CrossSection offset legs; mesh-algebra subject
│   ├── spatial.py            # MeshSpatial: trimesh+numpy proximity/ray/contains + vectorized rtree Bounds/Nearest + direct fcl signed clearance / manifold3d.min_gap exact gap
│   └── quality.py            # MeshQuality: aspect-ratio/skewness/manifold-edge/genus receipts + the PUBLIC closure_fold (watertight/euler/volume/components) the scan consumers compose
├── graph/                    # Non-manifold topology over topologicpy, AEC computational geometry over compas, network analytics over networkx
│   ├── analytic.py           # AnalyticValue reducer-return union + ranked board fold + census projections: the tier-0 substrate both analytics producers (features, nonmanifold) compose
│   ├── nonmanifold.py        # run/TopologyOp: CellComplex/Cell/Aperture construction, decomposition, adjacency, dual-graph behind cached function-local AGPL gates; topology-graph subject
│   ├── algebra.py            # ComputationalGeometry: network adjacency, form-finding, numerical primitives, mesh algebra; four graduation subjects
│   └── features.py           # Features: networkx centrality/community/cycle/connectivity analytics over the network-graph projection
└── energy/                   # Out-of-process AGPL Ladybug Tools building-physics band: climate, HBJSON model, urban district, simulation egress
    ├── climate.py            # Climate: polymorphic EPW admission, DataCollection series algebra, Sunpath solar, PMV/UTCI/PET comfort + map rows
    ├── model.py              # BuildingModel: HBJSON/BIM-to-BEM admission under one check_all gate, standards-resolved energy assignment, content-keyed wire
    ├── district.py           # District: dragonfly dfjson/GeoJSON/massing admission, auto-zoning, to_honeybee explosion, URBANopt/DES/OpenDSS/REopt rows
    └── simulate.py           # Simulation: offloaded OSM/IDF/epJSON/gbXML translation, runtime recipe binding, SQLiteResult/EUI decode into result frames
```

## [02]-[SEAMS]

```text seams
graduation          →  python:compute/graduation    # [GRADUATION]: GeometryHandoff.wire() content-keyed receipt data — the ONE compute crossing, decode-only, no import either direction
scan/registration   →  graduation                   # [GRADUATION]: registration-transform GeometryHandoff
scan/deviation      →  graduation                   # [GRADUATION]: scan-deviation GeometryHandoff keyed to the IFC GlobalId
scan/reconstruction →  graduation                   # [GRADUATION]: reconstructed-mesh GeometryHandoff
graph/algebra       →  graduation                   # [GRADUATION]: network-graph / form-finding / numerical-primitive / mesh-algebra GeometryHandoff
graph/features      →  graduation                   # [GRADUATION]: network-graph GeometryHandoff
graph/nonmanifold   →  graduation                   # [GRADUATION]: topology-graph GeometryHandoff
ifc/analysis        →  graduation                   # [GRADUATION]: bim-compliance GeometryHandoff (IDS/clash/BCF evidence)
ifc/costing         →  graduation                   # [GRADUATION]: bim-lifecycle GeometryHandoff (5D/4D evidence)
ifc/structural      →  graduation                   # [GRADUATION]: section-property GeometryHandoff (ring-closure + fe-area residuals)
mesh/repair         →  graduation                   # [GRADUATION]: reconstructed-mesh / mesh-algebra subjects on the repair receipts
mesh/brep           →  graduation                   # [GRADUATION]: mesh-algebra subject on the brep receipts
energy/climate      →  graduation                   # [GRADUATION]: thermal-comfort GeometryHandoff
energy/model        →  graduation                   # [GRADUATION]: building-energy GeometryHandoff
energy/district     →  graduation                   # [GRADUATION]: building-energy GeometryHandoff
energy/simulate     →  graduation                   # [GRADUATION]: building-energy GeometryHandoff (EUI vs caller ceiling)
mesh/serve          →  mesh/daemon                  # [PORT]: the servicer drives the daemon's returned RuntimeRail[Block[TessellationResult]]; daemon never frames, serve never tessellates
mesh/daemon         →  mesh/cad                     # [PORT]: cad case delegates to StepBridge.tessellate; TessellationPolicy imports downward from the cad mint
mesh/brep           →  mesh/cad                     # [PORT]: TessellationPolicy mesher knobs imported downward
mesh/serve          →  mesh/cad                     # [PORT]: BridgeFormat/TessellationPolicy/CANONICAL_TESSELLATION the request-echo decode composes
mesh/quality        →  mesh/repair                  # [PORT]: the public to_manifold uint32-ceiling kernel composed by the exact-topology tier
mesh/spatial        →  mesh/repair                  # [PORT]: the public to_manifold kernel composed by the exact-clearance arm
scan/deviation      →  mesh/quality                 # [PORT]: closure_fold the watertight gate composes
scan/reconstruction →  mesh/quality                 # [PORT]: closure_fold the receipt/graduation ledger projects from
graph/features      →  graph/analytic               # [PORT]: AnalyticValue/ranked/census projections
graph/nonmanifold   →  graph/analytic               # [PORT]: AnalyticValue/ranked/peak_of projections
ifc/analysis        →  ifc/selector                 # [PORT]: IfcSelector.filter/parse the validated selection gate
ifc/costing         →  ifc/selector                 # [PORT]: IfcSelector.filter the QUANTITY selector gate
ifc/structural      →  ifc/selector                 # [PORT]: IfcSelector.filter/parse the profile selector gate
energy/district     →  energy/model                 # [PORT]: to_honeybee explosion crossing every emitted model through BuildingModel.of
energy/simulate     →  energy/model                 # [PORT]: BuildingModel/hbjson the translation and recipe folds consume
mesh                ⇄  csharp:Rasm.Bim/Exchange     # [TESSELLATION]: GLB tessellation rail / TessellationRequest
mesh/serve          ⇄  csharp:Rasm.Compute/Runtime  # [WIRE]: ComputeService/ArtifactSync gRPC GLB tessellation
mesh/serve          →  csharp:Rasm.Compute/Runtime  # [TRANSPORT]: ServerHost-registered servicer streaming GLB + semantic header, 64 KiB Crc32 frames
mesh/serve          →  csharp:Rasm.Compute/Runtime  # [PROJECTION]: IFC tessellation bridge via IfcOpenShell decoded by Codecs
mesh/serve          →  csharp:Rasm.Compute/Runtime/codecs # [SHAPE]: SharpGLTF GLB import per-element tessellation staged into ResidencyPayload rows — AppUi decodes ONLY the Compute payload owner, never this rail directly
mesh/daemon         ⇄  csharp:Rasm.Compute/Runtime  # [CONTENT_KEY]: ContentIdentity XxHash128 source bytes + TessellationPolicy.spec seed re-tessellation cache parity
mesh/daemon         ⇄  csharp:Rasm.Element/Graph    # [WIRE]: imported-IFC GLB seed-zero XxHash128 == seam RepresentationContentHash entry; decode the seam key
mesh/serve          ⇄  python:runtime/transport     # [WIRE]: TessellationRequest/TessellationReceipt/ArtifactFrame registry rows bound by symbol; Route rows into ServerHost.register
mesh/daemon         →  python:runtime/identity      # [CONTENT_KEY]: ContentIdentity.of source+TessellationPolicy.spec cache key; seed-zero (Some(0)) over GLB bytes = wire key
ifc                 ←  csharp:Rasm.Bim/Exchange     # [PROJECTION]: BimWire model vocabulary IFC ingest
ifc                 ←  csharp:Rasm.Bim/Exchange     # [WIRE]: IfcWire bytes; ifcopenshell parity via ContentAddress.OfGraph, never byte-equality
ifc                 ⇄  csharp:Rasm.Element/Graph    # [WIRE]: the SAME rasm.element.v1 contract the codec decodes via grpcio-tools; keys verbatim
ifc                 →  csharp:Rasm.Bim/Review       # [BOUNDARY]: IDS validation evidence via ifctester
scan/deviation      ←  csharp:Rasm.Bim/Model        # [SHAPE]: reference GLB + element identity fetched BY CONTENT KEY; scan never re-tessellates
scan                →  python:data/spatial          # [SHAPE]: Arrow point-record columnar bridge x/y/z
mesh                ←  python:data/spatial          # [SHAPE]: MeshPayload cell-block topology
mesh/repair         →  python:data/spatial          # [BOUNDARY]: repair/brep/spatial/quality return in-memory Trimesh; mesh-file decode/encode + GLB preview are data's
mesh                ⇄  python:artifacts/figures     # [BOUNDARY]: visualization-scene/USD/GLTF/OBJ export is artifacts figures/scene, mesh-file codec is data
scan/ingestion      ←  python:data/spatial          # [SHAPE]: COPC arm decode leaves the pdal filter-graph owner unchanged
energy/model        ⇄  csharp:Rasm.Bim/Exchange     # [WIRE]: content-keyed canonical HBJSON document bytes; Energy peers at document bytes, 1 XxHash128 derivation
energy/model        ←  csharp:Rasm.Bim/Exchange     # [SHAPE]: IFC SPF source bytes for the BIM-to-BEM derivation modality
energy/simulate     →  python:runtime/execution     # [PORT]: RecipeExecution/RecipeSpec — geometry binds Job/RecipeInterface schema, runtime owns execution
energy/simulate     →  python:data/tabular          # [SHAPE]: self-describing result frames (output/unit/period/zone/step/value/content_key) over arrow_bytes fold
```

## [03]-[COMPANION_LANES]

Every sub-domain rides the companion engine selection the branch manifest owns, with compiled geometry/IFC cores and copyleft packages isolated at the process boundary.

The runtime lane carries `numpy`, `trimesh`, and `networkx` for the `mesh/spatial`, `mesh/quality`, and `graph/features` spine owners. Worker lanes carry compiled enrichment rows: `manifold3d`, `cadquery-ocp`, `compas`, `compas_dr`, `compas_tna`, `open3d`, `small-gicp`, `kiss-matcher`, `pye57`, `sectionproperties`, `rtree`, and `python-fcl`; `ifcopenshell` remains the worker IFC package behind `ifc/authoring`, `ifc/structural`, and the `energy/model` BIM-to-BEM derivation arm. The `find_spec` probe band is governed, never implicit: `rtree` (the `mesh/spatial` Bounds/Nearest offload probe), `fcl` (the CORE-clearance probe), `manifold3d` (the exact-gap and exact-topology tier probe), `embreex` (a probe-only transparent trimesh ray accel, unadmitted, no catalog), and `openstudio` (the `energy/simulate` in-process translator probe) — each probe selects an offload or tier row, never a silent import failure. The AGPL companion band (`ifcopenshell`, the `ifctester`/`ifcclash`/`ifc5d`/`ifc4d`/`ifcpatch`/`ifcdiff` utility ring, `open3d`, `small-gicp`, `topologicpy`) carries no root-manifest row and provisions through the companion-lane owner — `uv run python -m tools.assay provision` over the Forge cp312 scientific environment.

The AGPL Ladybug Tools band (`ladybug-core`/`ladybug-geometry`/`ladybug-comfort`, `honeybee-core`/`honeybee-energy`/`honeybee-openstudio` + the two standards data backends, `dragonfly-core`/`dragonfly-energy`) rides the `energy/` owners with strictly function-local boundary imports and process-boundary evidence exchange — HBJSON/dfjson/EPW document bytes and result frames across the wire, never a distributed link. The simulation engines (Radiance/OpenStudio/EnergyPlus behind the runtime recipe rail; URBANopt/Modelica/RNM/REopt behind the district translation rows) are external process-boundary services.
