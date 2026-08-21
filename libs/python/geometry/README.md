# [PY_GEOMETRY]

`geometry` is the Python branch's standalone host-free geometry and IFC/BIM platform, every crossing content-keyed, receipted, and produced off the event loop. Peer branches meet it through content identity and the GLB tessellation rail over the `ComputeService`/`ArtifactSyncService` gRPC contract, and it graduates evidence through its own `rasm.geometry.graduation` spine, never an import.

## [01]-[ROUTER]

[GRADUATION]:
- [01]-[GRADUATION](.planning/graduation.md): S0 evidence spine and raise-leg roster every producer composes into a content-keyed receipt.

[SCAN]:
- [02]-[INGESTION](.planning/scan/ingestion.md): Source-discriminated raw-scan preprocessing into a registration-ready point cloud.
- [03]-[REGISTRATION](.planning/scan/registration.md): `ScanRegistration` N-cloud session registration discriminated by `RegistrationMode`.
- [04]-[DEVIATION](.planning/scan/deviation.md): Signed nearest-surface scan-vs-model deviation against the content-keyed reference GLB.
- [05]-[RECONSTRUCTION](.planning/scan/reconstruction.md): Registered-cloud-to-watertight-mesh reconstruction, closure-graded through `mesh/quality`.

[IFC]:
- [06]-[ANALYSIS](.planning/ifc/analysis.md): IFC quantity, Pset, IDS, clash, and BCF analysis minting the `bim-compliance` subject.
- [07]-[COSTING](.planning/ifc/costing.md): 5D/4D model-lifecycle owner — quantity take-off, cost rollup, scheduling, and revision diff.
- [08]-[SELECTOR](.planning/ifc/selector.md): Validated selector grammar admitting a structured query, seating the band-wide `IfcFault` family.
- [09]-[AUTHORING](.planning/ifc/authoring.md): IFC spatial, element, and geometry authoring under the `@transactional`/`@stamped` fold.
- [10]-[STRUCTURAL](.planning/ifc/structural.md): Section-property integrals over `IfcProfileDef` and the warping/plastic/shear FE tier.

[MESH]:
- [11]-[DAEMON](.planning/mesh/daemon.md): IfcOpenShell tessellation daemon folding source bytes and policy into per-element GLB, source-keyed.
- [12]-[SERVE](.planning/mesh/serve.md): Geometry wire owner streaming GLB over contract-conforming `ArtifactSyncService`.
- [13]-[CAD](.planning/mesh/cad.md): ISO 10303 STEP and IGES tessellation hop over `cadquery-ocp`, companion to the C# `StepIso10303` codec.
- [14]-[REPAIR](.planning/mesh/repair.md): Robust mesh repair, winding and normal fix, and the public exact-boolean `to_manifold` kernel.
- [15]-[BREP](.planning/mesh/brep.md): B-rep evaluation over `cadquery-ocp`, minting the `mesh-algebra` subject.
- [16]-[SPATIAL](.planning/mesh/spatial.md): Proximity, ray, containment, and signed-clearance query over in-memory triangulation.
- [17]-[QUALITY](.planning/mesh/quality.md): Mesh-topology conditioning and metrology — decimate, subdivide, smooth, and the exact `closure_fold`.

[GRAPH]:
- [18]-[ANALYTIC](.planning/graph/analytic.md): `AnalyticValue` reducer substrate the nonmanifold and features producers compose.
- [19]-[NONMANIFOLD](.planning/graph/nonmanifold.md): Non-manifold cell-complex topology minting the `topology-graph` subject.
- [20]-[ALGEBRA](.planning/graph/algebra.md): `compas` network adjacency, form-finding, and mesh algebra beside the `NumericalOp` table.
- [21]-[FEATURES](.planning/graph/features.md): Mesh-feature detection projected onto the `networkx` analytic graph, the `network-graph` producer.

[ENERGY]:
- [22]-[CLIMATE](.planning/energy/climate.md): EPW admission and one `ClimateQuery` read surface over series, solar, comfort, indices, MRT, and maps.
- [23]-[MODEL](.planning/energy/model.md): `BuildingModel` HBJSON and BIM-to-BEM admission under `check_all`, energy-assigned at the host fold.
- [24]-[DISTRICT](.planning/energy/district.md): Dragonfly 2.5-D urban massing exploding onto the model owner under one shared explosion policy.
- [25]-[SIMULATE](.planning/energy/simulate.md): Simulation egress — parent-woven translation, recipe-parameterized binding, columnar result decode.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in the root `pyproject.toml` and corroborate against this folder's `.api/`.

[IFC_BIM]:
- `ifcopenshell` — IFC model, geometry, and tessellation spine.
- `ifctester` — buildingSMART IDS validation.
- `ifcclash` — BCF-producing clash detection.
- `bcf-client` — buildingSMART BCF I/O.
- `ifc5d` — Quantity take-off and cost-schedule rollup.
- `ifccsv` — IFC to CSV/ODS/XLSX/Pandas schedule round-trip.
- `ifc4d` — Construction scheduling.
- `ifcpatch` — Recipe transformation.
- `ifcdiff` — Model revision comparison.
- `lark` — IDS and selector grammar engine; runtime's transport filter compiles CESQL over the same substrate.

[SCAN]:
- `open3d` — Point-cloud registration and reconstruction.
- `small-gicp` — Fine point-cloud registration.
- `kiss-matcher` — Global initialization-free registration.
- `probreg` — Probabilistic CPD/FilterReg/SVR non-rigid registration.
- `pye57` — E57 I/O.
- `pdal` — Scan-ingestion filter graph.
- `pillow` — WebP plane decode for the SOG gaussian-splat container.

[TOPOLOGY_GRAPH]:
- `topologicpy` — Non-manifold cell-complex topology.
- `compas` — Network adjacency, form-finding, and mesh algebra.
- `compas_dr` — Dynamic relaxation.
- `compas_tna` — Thrust-network analysis.

[MESH_CAD]:
- `trimesh` — In-memory mesh operations, proximity, ray, and containment.
- `manifold3d` — Exact-boolean and clearance backend.
- `cadquery-ocp` — OCCT B-rep kernel and XCAF assembly for the STEP/IGES hop.
- `sectionproperties` — Warping, plastic, and shear enrichment.
- `rtree` — R-tree bounding-box index for the spatial bounds arm.
- `python-fcl` — Narrow-phase collision and signed-distance for the clearance arm.
- `gmsh` — Unstructured mesh generation the compute `MeshExchange` arm owns at the branch; this folder consumes the meshes, never the kernel.

[ENERGY]:
- `ladybug-geometry` — Planar and solid value-object substrate of the Ladybug band.
- `ladybug-core` — Climate backbone: weather, unit registry, Sunpath, and results.
- `ladybug-comfort` — Thermal-comfort models and spatial comfort maps.
- `honeybee-core` — HBJSON building-model object graph and the `check_all` spine.
- `honeybee-energy` — Building-energy constructions, loads, schedules, and HVAC exchange.
- `honeybee-openstudio` — In-process OpenStudio/EnergyPlus translator.
- `honeybee-standards` — Baseline construction, schedule, and program data.
- `honeybee-energy-standards` — ASHRAE 90.1 and DOE-prototype construction sets.
- `dragonfly-core` — District 2.5-D massing model exploding to Honeybee.
- `dragonfly-energy` — District-energy translation.
- `queenbee` — Recipe and workflow schema contract; the schema binding homes to `runtime`.
- `lbt-recipes` — Recipe binding and input coercion, consumed through the runtime recipe rail.
- `pollination-handlers` — Model-to-recipe-input handler adapters the runtime recipe rail consumes.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Python registry, whose charters own the full contracts; `libs/python/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`
- `pydantic`

[CONCURRENCY]:
- `anyio`

[OBSERVABILITY]:
- `opentelemetry-api` — Graduation-spine tracer surface.

[NUMERIC_SUBSTRATE]:
- `numpy`

[GRAPH_SUBSTRATE]:
- `networkx`

[WIRE_CODEGEN]:
- `grpcio`
- `grpcio-tools`
- `protobuf`
