# [PYTHON_LIBRARIES]

`libs/python` is the campaign root for the Python sibling library branch: an agnostic, universal, professional multi-package library held to the same bar as the C# suite, aligned hand-in-glove only where a shared concern crosses the boundary. The single root `pyproject.toml` is the only Python manifest; package folders carry no package-local manifest. Branch pages are decision-complete transcription blueprints carrying real Python signature fences, not narratives.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                 | [OWNS]                  |
| :-----: | :----------------------------------------------------- | :---------------------- |
|   [1]   | [campaign method](campaign-method.md)                  | branch cadence          |
|   [2]   | [architecture](architecture.md)                        | package topology        |
|   [3]   | [API catalogues](api-catalogues.md)                    | evidence protocol       |
|   [4]   | [features](FEATURES.md)                                | capability atlas        |
|   [5]   | [tasklog](TASKLOG.md)                                  | open work               |
|   [6]   | [runtime planning](../runtime/.planning/README.md)     | runtime package plan    |
|   [7]   | [data planning](../data/.planning/README.md)           | data package plan       |
|   [8]   | [geometry planning](../geometry/.planning/README.md)   | geometry package plan   |
|   [9]   | [compute planning](../compute/.planning/README.md)     | compute package plan    |
|  [10]   | [artifacts planning](../artifacts/.planning/README.md) | artifacts package plan  |
|  [11]   | [page regions](region-map/page-regions.md)             | planning regions        |
|  [12]   | [owner symbols](region-map/owner-symbols.md)           | symbol ledger           |
|  [13]   | [seam splits](region-map/seam-splits.md)               | cross-owner seams       |
|  [14]   | [API owners](region-map/api-owners.md)                 | package evidence owners |

## [2]-[OWNER_RECORDS]

[RUNTIME]:
- Package: `libs/python/runtime`
- Owns: caller-owned context/settings admission, the single boundary-fault + Result/Option rail, the one resilience policy, resource roots, bounded concurrency lanes, the ONE content-identity owner, local receipts + observability signals, external-API + structural-parsing evidence, the inbound companion server-runtime + credential owner, and the daemon/CLI entrypoint grammar.
- Boundary: product host lifecycle, global health, product telemetry export, support capture, service-root composition, and the global clock stay in `Rasm.AppHost`; the server host owns the companion's inbound serve only.
- Planning: [runtime planning](../runtime/.planning/README.md).

[DATA]:
- Package: `libs/python/data`
- Owns: typed dataset refs, columnar lazy/streaming scan + egress, cross-engine query plans, schema claims + data-contract validation gate, the vector AND raster geospatial axes, graph payloads, and mesh-file exchange.
- Boundary: durable stores, schema migrations, product repositories, and query rails stay in `Rasm.Persistence`; IFC tessellation, registration, topology, and AEC geometry belong to `geometry`; the numeric trio and labelled-array compute belong to `compute`; remote-stream transport is a runtime `TransportResource` row.
- Planning: [data planning](../data/.planning/README.md).

[GEOMETRY]:
- Package: `libs/python/geometry`
- Owns: the IfcOpenShell tessellation companion daemon (IFC to mesh/GLB + semantic XML/JSON), IFC property/quantity/relationship analysis, point-cloud/3D-scan registration and reconstruction, non-manifold topological modeling, and AEC computational geometry — pinned under a separate companion interpreter floor (`python_version<'3.13'`).
- Boundary: C# owns IFC semantic in-process (GeometryGym) and glTF in-process (SharpGLTF); the companion is purely the tessellation hop the managed surface cannot perform and speaks the EXISTING C# gRPC contract — it mints no transport. LGPL-3.0-or-later is satisfied at the process boundary by the isolated companion environment.
- Planning: [geometry planning](../geometry/.planning/README.md).

[COMPUTE]:
- Package: `libs/python/compute`
- Owns: offline array admission, one polymorphic numeric-intent solver/symbolic dispatch with accelerator rows, units + uncertainty claims, study + experiment-run orchestration, model-asset validation, and the C# graduation receipt with a geometry handoff case.
- Boundary: production compute runtime, benchmark authority, substrate selection, tensor sessions, and product `ComputeReceipt`s stay in `Rasm.Compute`; columnar/labelled-array ownership stays in `data`; geometry evidence graduates through the compute geometry handoff case.
- Planning: [compute planning](../compute/.planning/README.md).

[ARTIFACTS]:
- Package: `libs/python/artifacts`
- Owns: one polymorphic document/PDF/Office/structured-text plan, one VisualSpec to ExportPlan axis spanning 2D charts and 3D scientific visualization, one report-templating composition owner, one preview owner, and one compression owner.
- Boundary: live UI controls, dashboard runtime, browser state, product artifact stores, and AppUi evidence timelines stay outside; IFC tessellation/GLB belongs to `geometry`; geospatial/mesh-file/columnar interchange belongs to `data`.
- Planning: [artifacts planning](../artifacts/.planning/README.md).

## [3]-[BUILD_SEQUENCE]

1. Author `runtime` first; every sibling consumes its `RuntimeContext`, `BoundaryFault`/`RuntimeRail`, `Retry`, `ContentIdentity`, `ReceiptContributor`, `ResourceRoot`, `LanePolicy`, and `ServerHost`/`Credential` ports as settled vocabulary.
2. Author `data`, `geometry`, `compute`, and `artifacts` against the runtime ports and the ledger's pinned seam contracts; within a package, position equals the charter PAGE_INDEX order.
3. Admit dependencies only through the root `pyproject.toml`; capture package-owned API evidence under `libs/python/<package>/.api/api-<distribution>.md` before a fence names a member.
4. Refine package-local pages until every planned symbol has one owner, one boundary, and one API evidence route.
5. Start source transcription only after the package page names the owner cluster, its signature fence, and its `.api` evidence route.

## [4]-[FILE_PROCESS]

[ROOT_PLANNING]:
- Path: `libs/python/.planning`
- Role: branch charter, topology, API-evidence protocol, region maps, capability atlas, and cross-owner law.

[PACKAGE_PLANNING]:
- Path: `libs/python/<package>/.planning`
- Role: package-local owner pages modeled after the C# package planning pattern, carrying transcription-complete signature fences.

[API_EVIDENCE]:
- Path: `libs/python/<package>/.api/api-<distribution>.md`
- Role: external package capability evidence verified before a fence names a member.

[PACKAGE_ROOT]:
- Path: `libs/python/<package>`
- Role: package README, `.api`, `.planning`, and future source root. No package-local `pyproject.toml`.

[MANIFEST]:
- Path: `pyproject.toml`
- Role: the only Python dependency and tool manifest for this branch; the geometry-companion group carries the one sanctioned interpreter-floor divergence (`python_version<'3.13'`).

## [5]-[PROOF_GATES]

| [GATE] | [RAIL]                          | [EVIDENCE]                                                     |
| :----: | :------------------------------ | :------------------------------------------------------------ |
|  [G1]  | layout                          | `libs/python` carries no `.py` source, no package manifest    |
|  [G2]  | whitespace                      | `git diff --check -- libs/python pyproject.toml` is clean     |
|  [G3]  | manifest                        | `uv lock --check` resolves clean against the root manifest    |
|  [G4]  | catalogue                       | every fence member resolves to an `.api` evidence row         |
|  [G5]  | anchor                          | every `page#cluster` reference resolves to a live heading     |
|  [G6]  | companion floor                 | geometry/companion-server pins reflect on the cp312 floor     |

## [6]-[PROHIBITIONS]

- Package-local `pyproject.toml` files, nested source-root layout, empty `__init__.py` markers.
- Python source before the package planning + API-evidence pass admits it.
- Barrel exports, wildcard imports, facade-only modules, one-hop re-export files, thin rename wrappers over an external API.
- A second content-identity owner, a second receipt rail, a second retry owner, or a second wire vocabulary; `runtime` owns each of these once and every package consumes it.
- New public CLI commands; `tools/assay` owns public commands and the runtime `Entrypoint` owner is the companion's PRIVATE entry only.
- Product host lifecycle, durable stores, Rhino/GH mutation, live UI, browser dashboard runtime, production compute receipts, or C# wire ownership inside Python packages.
- Release columns, universal state columns (owner state lives only on each package charter DENSITY_BAR), provenance tails, prompt narration, checklist history, and dependency facts copied away from the root manifest or `.api` evidence.

## [7]-[REFINEMENT_HORIZON]

The next deepening pass mines each `.api` catalogue end-to-end into transcription-complete fences, drives every package DENSITY_BAR `SPIKE` row to `FINALIZED` against the installed distributions, and rehearses the companion gRPC handshake end-to-end against the C# `ComputeService`/`ArtifactSync` descriptors. The forward-emitted categorical frontier is the object-graph DAG diff concern (Speckle-class), surfaced for its own owned page when a named consumer and the C# Persistence diff algebra land. The bar: any world-class scientific, data, artifact, geometry, or AEC application is buildable from the package pages alone, with receipts proving every hop and the companion seam consumed, never re-minted.
