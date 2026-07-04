# [PY_GEOMETRY]

`geometry` is the host-free geometry and IFC/BIM companion of the Python branch. It owns the IfcOpenShell GLB tessellation daemon — the load-bearing cross-boundary two-hop that the C# Bim/Compute rail and the TS viewer consume — along with IFC analysis and buildingSMART validation, point-cloud/3D-scan registration, non-manifold topology, AEC computational geometry, and the out-of-process AGPL Ladybug Tools energy/environmental companion band (climate, HBJSON building-energy, urban-district massing, and thermal comfort) emitting HBJSON/`DataCollection`/result evidence across the wire. It is a peer producer, never a Rasm consumer: it meets C# only at the wire via content-identity plus the GLB tessellation rail over the `ComputeService`/`ArtifactSync` gRPC contract, and graduates evidence through the compute `HandoffAxis` geometry case. This file routes the design pages and registers the external packages the folder uses; `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[INGESTION](.planning/scan/ingestion.md): Raw-scan preprocessing: `pdal` filter-graph (ground classification, outlier removal, decimation) and `laspy` COPC/E57 ingestion into a registration-ready `o3d.t.geometry.PointCloud`.
- [02]-[REGISTRATION](.planning/scan/registration.md): `kiss_matcher` global, multi-scale tensor, colored, VGICP, and multiway point-cloud registration.
- [03]-[DEVIATION](.planning/scan/deviation.md): RANSAC plane/primitive segmentation and scan-vs-model nearest-surface deviation against the IFC-tessellated GLB.
- [04]-[RECONSTRUCTION](.planning/scan/reconstruction.md): Registered-cloud-to-watertight-mesh reconstruction over open3d Poisson/ball-pivoting/alpha-shape and normal estimation, producing the `reconstructed-mesh` graduation subject.
- [05]-[ANALYSIS](.planning/ifc/analysis.md): IFC quantity/Pset/IDS/clash/space-program/BCF analysis over the IfcOpenShell ecosystem.
- [06]-[COSTING](.planning/ifc/costing.md): 5D/4D model-lifecycle owner: `ifc5d` rule-driven quantity take-off and cost-schedule rollup, `ifc4d` construction scheduling, `ifcpatch` recipe transformation, and `ifcdiff` revision comparison.
- [07]-[SELECTOR](.planning/ifc/selector.md): `lark`-validated IFC selector/filter-query grammar that admits a structured query before `util.selector.filter_elements`.
- [08]-[STRUCTURAL](.planning/ifc/structural.md): Closed-form section integrals over `IfcProfileDef`, structural-entity enrichment, and warping/plastic/shear enrichment, producing the `numerical-primitive` graduation subject.
- [09]-[DAEMON](.planning/mesh/daemon.md): IfcOpenShell tessellation daemon: source bytes and tolerance into per-element GLB and a semantic header over the existing gRPC contract, routing CAD source formats to the `mesh/cad.md` STEP hop.
- [10]-[CAD](.planning/mesh/cad.md): ISO 10303 AP242/AP203/AP214+IGES CAD-STEP tessellation hop over `cadquery-ocp`, companion to C# `StepIso10303` codec.
- [11]-[REPAIR](.planning/mesh/repair.md): `MeshOp` robust mesh repair, exact boolean, and mesh-file codec owner over `trimesh`/`manifold3d`/`rhino3dm`/`meshio`.
- [12]-[NONMANIFOLD](.planning/graph/nonmanifold.md): Non-manifold `CellComplex`/`Cell`/`Aperture` topology over `topologicpy`.
- [13]-[ALGEBRA](.planning/graph/algebra.md): `compas` network adjacency, form-finding, numerical primitives, and mesh algebra.
- [14]-[CLIMATE](.planning/energy/climate.md): Polymorphic EPW weather admission, `DataCollection` series algebra, `Sunpath` solar geometry, and the PMV/UTCI/PET point-comfort tier with SolarCal MRT and the spatial comfort-map rows.
- [15]-[MODEL](.planning/energy/model.md): One HBJSON building-model admission — wire-arrived document bytes and the computed IFC BIM-to-BEM derivation — under one `check_all` gate, standards-resolved energy assignment, and the content-keyed HBJSON wire egress.
- [16]-[DISTRICT](.planning/energy/district.md): Dragonfly 2.5-D urban massing — dfjson/GeoJSON/massing admission, ordered auto-zoning, the `to_honeybee` explosion onto the model owner, and the URBANopt/DES/OpenDSS/REopt translation rows.
- [17]-[SIMULATE](.planning/energy/simulate.md): Simulation egress — offloaded OSM/IDF/epJSON/gbXML translation, the runtime recipe binding for `annual-energy-use`, and the `SQLiteResult`/EUI decode into self-describing result frames crossing the data seam.

## [02]-[DOMAIN_PACKAGES]

Every domain library the folder uses, planned or implemented. Versions are centralized in the one Python branch manifest; substrate packages live in `[3]-[SUBSTRATE_PACKAGES]` below. A `(admit)` tag marks a library a forward task still draws on that no provenance yet provides, so the registry never implies a phantom dependency.

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
- `laspy` - COPC/LAS I/O
- `pye57` - E57 I/O
- `pdal` - Scan-ingestion filter-graph engine

[TOPOLOGY]:
- `topologicpy` - Non-manifold cell-complex topology
- `compas` - Network adjacency, form-finding, numerical primitives, and mesh algebra
- `compas_dr` - Dynamic relaxation
- `compas_tna` - Thrust-network analysis

[MESH_CAD]:
- `trimesh` - Mesh operations, proximity, ray, containment, and interchange
- `rhino3dm` - Rhino mesh and 3DM I/O
- `meshio` - Mesh-file interchange
- `manifold3d` - Robust exact-boolean backend
- `cadquery-ocp` - OCCT B-rep kernel and XCAF assembly model for the STEP/IGES hop
- `sectionproperties` - Warping, plastic, and shear enrichment row only
- `gmsh` - Deferred

[ENERGY]:
- `ladybug-geometry` - Pure-Python planar/solid value-object substrate of the Ladybug Tools band
- `ladybug-core` - Climate backbone: EPW/Wea weather, unit registry, analysis periods, Sunpath, SQLite results
- `ladybug-comfort` - Thermal-comfort models (PMV/UTCI/PET/adaptive/SolarCal) and spatial comfort maps
- `honeybee-core` - HBJSON base building-model object graph and `check_all` validation spine
- `honeybee-energy` - Building-energy extension: constructions, loads, schedules, HVAC, EnergyPlus/OpenStudio exchange
- `honeybee-openstudio` - In-process OpenStudio/EnergyPlus translator over the native SDK
- `honeybee-standards` - Baseline default construction, schedule, and program data backend
- `honeybee-energy-standards` - Large ASHRAE 90.1 / DOE-prototype standards extension: climate-zone/vintage construction sets, programs, and schedules resolved by identifier through `honeybee-energy.lib`
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
