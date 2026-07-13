# [PY_GEOMETRY]

`geometry` owns the host-free geometry and IFC/BIM band of the Python branch — the IfcOpenShell GLB tessellation daemon the C# Bim/Compute rail and the TS viewer consume, IFC analysis and buildingSMART validation, point-cloud registration and reconstruction, mesh algebra and non-manifold topology, and the out-of-process Ladybug Tools energy companion. It is a peer producer, never a Rasm consumer: it meets C# only at the wire through content identity and the GLB tessellation rail over the `ComputeService`/`ArtifactSync` gRPC contract, and graduates evidence through the geometry-minted `rasm.geometry.graduation` spine, never an import.

## [01]-[ROUTER]

[GRADUATION]:
- [01]-[GRADUATION](.planning/graduation.md): Tier-0 evidence spine every producer composes into a content-keyed graduation receipt.

[SCAN]:
- [02]-[INGESTION](.planning/scan/ingestion.md): Source-discriminated raw-scan preprocessing into a registration-ready point cloud.
- [03]-[REGISTRATION](.planning/scan/registration.md): Global-then-multi-scale point-cloud registration minting the `registration-transform` subject.
- [04]-[DEVIATION](.planning/scan/deviation.md): Signed nearest-surface scan-vs-model deviation against the content-keyed reference GLB.
- [05]-[RECONSTRUCTION](.planning/scan/reconstruction.md): Registered-cloud-to-watertight-mesh reconstruction, closure-graded through `mesh/quality`.

[IFC]:
- [06]-[ANALYSIS](.planning/ifc/analysis.md): IFC quantity, Pset, IDS, clash, and BCF analysis minting the `bim-compliance` subject.
- [07]-[COSTING](.planning/ifc/costing.md): 5D/4D model-lifecycle owner — quantity take-off, cost rollup, scheduling, and revision diff.
- [08]-[SELECTOR](.planning/ifc/selector.md): Validated selector grammar admitting a structured query before element filtering.
- [09]-[AUTHORING](.planning/ifc/authoring.md): IFC spatial, element, and geometry authoring transactions over the ownership-history rail.
- [10]-[STRUCTURAL](.planning/ifc/structural.md): Section-property integrals over `IfcProfileDef` and the warping/plastic/shear FE tier.

[MESH]:
- [11]-[DAEMON](.planning/mesh/daemon.md): IfcOpenShell tessellation daemon folding source bytes and policy into per-element GLB, source-keyed.
- [12]-[SERVE](.planning/mesh/serve.md): Geometry-side wire owner streaming GLB over `ArtifactSync` to the C#-minted request.
- [13]-[CAD](.planning/mesh/cad.md): CAD-STEP tessellation hop over `cadquery-ocp`, companion to the C# `StepIso10303` codec.
- [14]-[REPAIR](.planning/mesh/repair.md): Robust mesh repair, winding and normal fix, and the public exact-boolean `to_manifold` kernel.
- [15]-[BREP](.planning/mesh/brep.md): `cadquery-ocp` B-rep evaluation minting the `mesh-algebra` subject.
- [16]-[SPATIAL](.planning/mesh/spatial.md): Proximity, ray, containment, and signed-clearance query over in-memory triangulation.
- [17]-[QUALITY](.planning/mesh/quality.md): Mesh-quality metric receipts and the public exact `closure_fold` the scan consumers compose.

[GRAPH]:
- [18]-[ANALYTIC](.planning/graph/analytic.md): Tier-0 graph-analytics substrate both graph producers compose.
- [19]-[NONMANIFOLD](.planning/graph/nonmanifold.md): Non-manifold cell-complex topology minting the `topology-graph` subject.
- [20]-[ALGEBRA](.planning/graph/algebra.md): `compas` network adjacency, form-finding, and mesh algebra.
- [21]-[FEATURES](.planning/graph/features.md): `networkx` centrality, community, cycle, and connectivity analytics over the network graph.

[ENERGY]:
- [22]-[CLIMATE](.planning/energy/climate.md): EPW weather admission, series algebra, solar geometry, and the point-comfort tier.
- [23]-[MODEL](.planning/energy/model.md): One HBJSON building-model admission under one `check_all` gate with standards-resolved assignment.
- [24]-[DISTRICT](.planning/energy/district.md): Dragonfly 2.5-D urban massing exploding onto the model owner.
- [25]-[SIMULATE](.planning/energy/simulate.md): Simulation egress — offloaded translation, recipe binding, and self-describing result decode.

## [02]-[DOMAIN_PACKAGES]

Domain libraries the folder admits; substrate lives in `[03]-[SUBSTRATE_PACKAGES]`. Admission is per-group, never one manifest — worker rows carry a root `pyproject.toml` entry, the AGPL companion band carries no manifest row and provisions through the Forge scientific-environment companion lane, and the recipe schema binding is `[RUNTIME]`-owned. Every admitted package carries either a manifest row or a companion-lane home; a `Deferred` tag marks a library a forward task draws on that no admission yet backs.

[IFC_BIM]:
- `ifcopenshell` - IFC model, geometry, and tessellation spine
- `ifctester` - buildingSMART IDS validation
- `ifcclash` - BCF-producing clash detection
- `bcf-client` - buildingSMART BCF I/O
- `ifc5d` - Quantity take-off and cost-schedule rollup
- `ifc4d` - Construction scheduling
- `ifcpatch` - Recipe transformation
- `ifcdiff` - Model revision comparison
- `lark` - IDS and selector grammar engine

[SCAN]:
- `open3d` - Point-cloud registration and reconstruction
- `small-gicp` - Fine point-cloud registration
- `kiss-matcher` - Global initialization-free registration
- `pye57` - E57 I/O
- `pdal` - Scan-ingestion filter graph

[TOPOLOGY_GRAPH]:
- `topologicpy` - Non-manifold cell-complex topology
- `compas` - Network adjacency, form-finding, and mesh algebra
- `compas_dr` - Dynamic relaxation
- `compas_tna` - Thrust-network analysis

[MESH_CAD]:
- `trimesh` - In-memory mesh operations, proximity, ray, and containment
- `manifold3d` - Exact-boolean and clearance backend
- `cadquery-ocp` - OCCT B-rep kernel and XCAF assembly for the STEP/IGES hop
- `sectionproperties` - Warping, plastic, and shear enrichment
- `rtree` - R-tree bounding-box index for the spatial bounds arm
- `python-fcl` - Narrow-phase collision and signed-distance for the clearance arm
- `gmsh` - Deferred

[ENERGY]:
- `ladybug-geometry` - Planar and solid value-object substrate of the Ladybug band
- `ladybug-core` - Climate backbone: weather, unit registry, Sunpath, and results
- `ladybug-comfort` - Thermal-comfort models and spatial comfort maps
- `honeybee-core` - HBJSON building-model object graph and the `check_all` spine
- `honeybee-energy` - Building-energy constructions, loads, schedules, and HVAC exchange
- `honeybee-openstudio` - In-process OpenStudio/EnergyPlus translator
- `honeybee-standards` - Baseline construction, schedule, and program data
- `honeybee-energy-standards` - ASHRAE 90.1 and DOE-prototype construction sets
- `dragonfly-core` - District 2.5-D massing model exploding to Honeybee
- `dragonfly-energy` - District-energy translation
- `queenbee` - Recipe and workflow schema contract
- `lbt-recipes` - Recipe binding and input coercion
- `pollination-handlers` - Model-to-recipe-input handler adapters

## [03]-[SUBSTRATE_PACKAGES]

Branch-wide substrate this folder consumes; the canonical registry and API evidence live in `libs/python/.planning/README.md` and the adjacent `libs/python/.api/`.

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
