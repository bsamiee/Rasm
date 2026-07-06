# [PY_GEOMETRY]

`geometry` is the host-free geometry and IFC/BIM companion of the Python branch. It owns the IfcOpenShell GLB tessellation daemon — the load-bearing cross-boundary two-hop that the C# Bim/Compute rail and the TS viewer consume — along with IFC analysis and buildingSMART validation, point-cloud/3D-scan registration, non-manifold topology, AEC computational geometry, and the out-of-process AGPL Ladybug Tools energy/environmental companion band (climate, HBJSON building-energy, urban-district massing, and thermal comfort) emitting HBJSON/`DataCollection`/result evidence across the wire. It is a peer producer, never a Rasm consumer: it meets C# only at the wire via content-identity plus the GLB tessellation rail over the `ComputeService`/`ArtifactSync` gRPC contract, and graduates evidence through the geometry-minted `rasm.geometry.graduation` spine — the differentiated `GeometrySubject` union and the `GeometryHandoff` carrier whose `wire()` projection is the content-keyed receipt data compute decodes, never an import. This file routes the design pages and registers the external packages the folder uses; `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[GRADUATION](.planning/graduation.md): The tier-0 evidence spine — the geometry-minted differentiated `GeometrySubject` union, the `GeometryHandoff` carrier with the frozen compute contract pin, and the one `evidence_run` weave (seeded span + fence + receipt harvest) every producer composes.
- [02]-[INGESTION](.planning/scan/ingestion.md): Source-discriminated raw-scan preprocessing: the `pdal` filter-graph (ground classification, outlier removal, decimation, streaming iterator for block-scale clouds) and `pye57` E57 intake with typed `ScanHeader` station provenance into a registration-ready `o3d.t.geometry.PointCloud`; the inbound LAS/LAZ/COPC bytes and the `pyarrow` point-record bridge decode at the data codec seam.
- [03]-[REGISTRATION](.planning/scan/registration.md): `kiss_matcher` global (with the `open3d` FGR fallback arm), multi-scale tensor, colored, VGICP, and multiway point-cloud registration, producing the `registration-transform` graduation subject.
- [04]-[DEVIATION](.planning/scan/deviation.md): RANSAC plane/primitive segmentation and scan-vs-model signed nearest-surface deviation against the IFC-tessellated GLB fetched BY CONTENT KEY, the watertight gate the `mesh/quality` closure fold, producing the `scan-deviation` graduation subject.
- [05]-[RECONSTRUCTION](.planning/scan/reconstruction.md): Registered-cloud-to-watertight-mesh reconstruction over open3d Poisson/ball-pivoting/alpha-shape and normal estimation, closure-graded through the `mesh/quality` fold, producing the `reconstructed-mesh` graduation subject.
- [06]-[ANALYSIS](.planning/ifc/analysis.md): IFC quantity/Pset/IDS/clash/space-program/BCF analysis over the IfcOpenShell ecosystem, producing the `bim-compliance` graduation subject.
- [07]-[COSTING](.planning/ifc/costing.md): 5D/4D model-lifecycle owner: `ifc5d` rule-driven quantity take-off and cost-schedule rollup, `ifc4d` construction scheduling, `ifcpatch` recipe transformation, and `ifcdiff` revision comparison, producing the `bim-lifecycle` graduation subject.
- [08]-[SELECTOR](.planning/ifc/selector.md): `lark`-validated IFC selector/filter-query grammar that admits a structured query before `util.selector.filter_elements`.
- [09]-[STRUCTURAL](.planning/ifc/structural.md): Closed-form section integrals over `IfcProfileDef`, structural-entity enrichment, and the realized warping/plastic/shear FE tier, producing the `section-property` graduation subject.
- [10]-[AUTHORING](.planning/ifc/authoring.md): `IfcAuthoring` IfcOpenShell spatial/element/geometry transactions over the GUID and OwnerHistory ownership-history rail — the verb-table authoring owner, graduation-free by design.
- [11]-[DAEMON](.planning/mesh/daemon.md): IfcOpenShell tessellation daemon: source bytes and the geometry-owned `TessellationPolicy` into a RETURNED `RuntimeRail[Block[TessellationResult]]` of per-element GLB plus semantic header, cache-keyed at the source, routing CAD source formats to the `mesh/cad.md` STEP hop.
- [12]-[SERVE](.planning/mesh/serve.md): The geometry-side wire owner — the tessellation servicer registered in the runtime `ServerHost` lifecycle, decoding the C#-minted `TessellationRequest`, answering the `TessellationReceipt` field floor, and streaming the GLB over `ArtifactSync` as 64 KiB Crc32-framed rows with the seed-zero XxHash128 artifact hash.
- [13]-[CAD](.planning/mesh/cad.md): ISO 10303 AP242/AP203/AP214+IGES CAD-STEP tessellation hop over `cadquery-ocp`, the mint of the geometry-owned `TessellationPolicy` mesher knobs, companion to C# `StepIso10303` codec.
- [14]-[REPAIR](.planning/mesh/repair.md): `MeshRepairOp` robust mesh repair, winding/normal fix, and exact `manifold3d` boolean over in-memory `trimesh`, owning the PUBLIC `to_manifold` uint32-ceiling kernel; mesh-file decode/encode is deferred to the data `MeshPayload` seam.
- [15]-[BREP](.planning/mesh/brep.md): `BrepOp` `cadquery-ocp` B-rep evaluation — primitives, BOPAlgo booleans with fuzzy/history evidence, sew/NURBS conditioning, offset/loft/sweep over the `manifold3d.CrossSection` 2D leg — producing the `mesh-algebra` subject.
- [16]-[SPATIAL](.planning/mesh/spatial.md): `MeshSpatial` `trimesh`+`numpy` proximity/ray/containment query with the vectorized `rtree` Bounds/Nearest arms and the direct `python-fcl` signed-clearance / `manifold3d.min_gap` exact-clearance backends over in-memory triangulation.
- [17]-[QUALITY](.planning/mesh/quality.md): `MeshQuality` `trimesh`+`numpy` aspect-ratio/skewness/manifold-edge/genus mesh-quality metric receipts and the PUBLIC exact watertight/euler/volume/components `closure_fold` the scan consumers compose.
- [18]-[ANALYTIC](.planning/graph/analytic.md): The tier-0 graph-analytics substrate — the `AnalyticValue` reducer-return union with its dual projections, the polymorphic `ranked` board fold, and the census projections both graph-analytics producers (`graph/features`, `graph/nonmanifold`) compose.
- [19]-[NONMANIFOLD](.planning/graph/nonmanifold.md): Non-manifold `CellComplex`/`Cell`/`Aperture` topology over `topologicpy` behind cached function-local AGPL gates, producing the `topology-graph` graduation subject.
- [20]-[ALGEBRA](.planning/graph/algebra.md): `compas` network adjacency, form-finding, numerical primitives, and mesh algebra, producing the `network-graph`/`form-finding`/`numerical-primitive`/`mesh-algebra` graduation subjects.
- [21]-[FEATURES](.planning/graph/features.md): `Features` `networkx` centrality/community/cycle/connectivity analytics over the mesh-feature network-graph projection, producing the `network-graph` graduation subject.
- [22]-[CLIMATE](.planning/energy/climate.md): Polymorphic EPW weather admission, `DataCollection` series algebra, `Sunpath` solar geometry, and the PMV/UTCI/PET point-comfort tier with SolarCal MRT and the spatial comfort-map rows.
- [23]-[MODEL](.planning/energy/model.md): One HBJSON building-model admission — wire-arrived document bytes and the computed IFC BIM-to-BEM derivation — under one `check_all` gate, standards-resolved energy assignment, and the content-keyed HBJSON wire egress.
- [24]-[DISTRICT](.planning/energy/district.md): Dragonfly 2.5-D urban massing — dfjson/GeoJSON/massing admission, ordered auto-zoning, the `to_honeybee` explosion onto the model owner, and the URBANopt/DES/OpenDSS/REopt translation rows.
- [25]-[SIMULATE](.planning/energy/simulate.md): Simulation egress — offloaded OSM/IDF/epJSON/gbXML translation, the runtime recipe binding for `annual-energy-use`, and the `SQLiteResult`/EUI decode into self-describing result frames crossing the data seam.

## [02]-[DOMAIN_PACKAGES]

Every domain library the folder uses, planned or implemented; substrate packages live in `[03]-[SUBSTRATE_PACKAGES]` below. Admission owner is per-group, never one manifest: worker rows carry a root `pyproject.toml` entry (`pye57`/`cadquery-ocp`/`kiss-matcher`/`python-fcl` on the `python_version<'3.15'` band, the rest unmarkered), the AGPL companion band (`ifcopenshell` and the `ifctester`/`ifcclash`/`ifc5d`/`ifc4d`/`ifcpatch`/`ifcdiff` utility ring, `open3d`, `small-gicp`, `topologicpy`) carries NO manifest row by design and provisions through the companion lane over the Forge scientific environment, and the recipe trio (`queenbee`/`lbt-recipes`/`pollination-handlers`) is a `[RUNTIME]`-owned schema binding. A `Deferred` tag marks a library a forward task draws on that no admission yet provides; a package with neither a manifest row nor a companion-lane home is a phantom.

[IFC_BIM]:
- `ifcopenshell` - IFC model, geometry, and tessellation spine
- `ifctester` - BuildingSMART IDS validation
- `ifcclash` - BCF-producing clash detection
- `bcf-client` - BuildingSMART BCF I/O
- `ifc5d` - Rule-driven quantity take-off and cost-schedule rollup
- `ifc4d` - Construction scheduling
- `ifcpatch` - Recipe transformation
- `ifcdiff` - Model revision comparison
- `lark` - IDS and selector parsing-grammar engine

[SCAN_POINT_CLOUD]:
- `open3d` - Point-cloud geometry, registration, and reconstruction
- `small-gicp` - Fine point-cloud registration
- `kiss-matcher` - Global initialization-free registration
- `pye57` - E57 I/O
- `pdal` - Scan-ingestion filter-graph engine

[TOPOLOGY]:
- `topologicpy` - Non-manifold cell-complex topology
- `compas` - Network adjacency, form-finding, numerical primitives, and mesh algebra
- `compas_dr` - Dynamic relaxation
- `compas_tna` - Thrust-network analysis

[MESH_CAD]:
- `trimesh` - Mesh operations, proximity, ray, and containment over in-memory triangulation
- `manifold3d` - Robust exact-boolean backend and `min_gap` clearance fall-through
- `cadquery-ocp` - OCCT B-rep kernel and XCAF assembly model for the STEP/IGES hop
- `sectionproperties` - Warping, plastic, and shear enrichment row only
- `rtree` - libspatialindex R-tree bounding-box index for the `mesh/spatial` Bounds arm
- `python-fcl` - FCL narrow-phase collision/distance for the `mesh/spatial` CORE-clearance arm
- `gmsh` - Deferred

[ENERGY]:
- `ladybug-geometry` - Pure-Python planar/solid value-object substrate of the Ladybug Tools band
- `ladybug-core` - Climate backbone: EPW/Wea weather, unit registry, analysis periods, Sunpath, SQLite results
- `ladybug-comfort` - Thermal-comfort models (PMV/UTCI/PET/adaptive/SolarCal) and spatial comfort maps
- `honeybee-core` - HBJSON base building-model object graph and `check_all` validation spine
- `honeybee-energy` - Building-energy extension: constructions, loads, schedules, HVAC, EnergyPlus/OpenStudio exchange
- `honeybee-openstudio` - In-process OpenStudio/EnergyPlus translator over the native SDK
- `honeybee-standards` - Baseline default construction, schedule, and program data backend
- `honeybee-energy-standards` - ASHRAE 90.1/DOE-prototype standards: climate-zone/vintage construction sets, programs, schedules via `honeybee-energy.lib`
- `dragonfly-core` - District/urban 2.5-D massing object model exploding to Honeybee
- `dragonfly-energy` - District-energy translation (URBANopt/DES/OpenDSS/REopt)
- `queenbee` - Recipe/workflow schema contract; geometry binds `RecipeInterface`/`Job` while `runtime` owns execution
- `lbt-recipes` - Recipe binding and input coercion; `runtime` owns the `queenbee-local` Luigi DAG execution
- `pollination-handlers` - Model-to-recipe-input handler adapters; `runtime` owns execution-time invocation

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
