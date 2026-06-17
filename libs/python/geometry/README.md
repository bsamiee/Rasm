# [PY_GEOMETRY]

`geometry` owns geometry and IFC/BIM interchange and is the load-bearing cross-boundary package of the branch: the IfcOpenShell tessellation companion daemon (IFC to mesh/GLB plus semantic XML/JSON), IFC property/quantity/relationship analysis, point-cloud/3D-scan registration and reconstruction, non-manifold topological modeling, and AEC computational geometry. It has zero consumers today and implementation is full-capability. It consumes the runtime `ServerHost`, `ContentIdentity`, rails, lanes, and `ReceiptContributor`, and graduates geometry evidence through the compute `HandoffAxis` geometry case. The package is pinned under a separate companion interpreter floor (`python_version<'3.13'`) divorced from the `>=3.15` runtime floor. Owner state and the axis registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`. The design pages in `.planning/` are decision-complete blueprints an implementation agent transcribes; the package catalogues in `.api/` carry the external-surface evidence each page consumes.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                        | [OWNS]                                                             |
| :-----: | :-------------------------------------------- | :---------------------------------------------------------------- |
|   [1]   | [ifc-companion](.planning/ifc-companion.md)     | the IfcOpenShell tessellation daemon over the inbound gRPC contract |
|   [2]   | [ifc-analysis](.planning/ifc-analysis.md)       | IFC property/quantity/relationship analysis (QTO/clash/rule-check) |
|   [3]   | [scan-processing](.planning/scan-processing.md) | point-cloud/3D-scan registration, reconstruction                   |
|   [4]   | [geometry-algebra](.planning/geometry-algebra.md) | non-manifold topology + AEC computational geometry               |

## [2]-[ADMISSIONS_RECORD]

The executed admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in the root manifest; this table never carries a pin. `[STATUS]` is `catalogue-pending` for every distribution until the sub-3.13 companion lock scope is admitted; member proof currently rides a cp312 companion venv, not the default project resolver.

| [INDEX] | [PACKAGE]    | [PAGE]                      | [CATALOGUE]         | [STATUS]          |
| :-----: | :----------- | :-------------------------- | :------------------ | :---------------- |
|   [1]   | ifcopenshell | ifc-companion, ifc-analysis | api-ifcopenshell.md | catalogue-pending |
|   [2]   | open3d       | scan-processing             | api-open3d.md       | catalogue-pending |
|   [3]   | small-gicp   | scan-processing             | api-small_gicp.md   | catalogue-pending |
|   [4]   | topologicpy  | geometry-algebra            | api-topologicpy.md  | catalogue-pending |
|   [5]   | compas       | geometry-algebra            | api-compas.md       | catalogue-pending |

## [3]-[PROOF_GATES]

Proof runs at the planned phase gate, not after each edit. `[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]                | [RAIL]      | [EVIDENCE]                                              |
| :-----: | :-------------------- | :---------- | :----------------------------------------------------- |
|  [G1]   | locked restore        | uv          | geometry pins resolve against the root manifest         |
|  [G2]   | companion floor       | uv          | all five pins reflect on the cp312 companion interpreter |
|  [G3]   | API catalogue resolve | assay api   | every fence member resolves to an `.api` row            |
|  [G4]   | type check            | ty          | typed-signature transcription resolves clean            |
|  [G5]   | lint and format       | ruff        | routed closure, zero diagnostics                        |
|  [G6]   | spec law-matrix       | pytest      | geometry law-matrix specs pass                          |
|  [G7]   | daemon bridge         | pytest      | the `IfcCompanion` daemon serves the inbound contract under the companion floor |
|  [G8]   | page diagram render   | mermaid-cli | page diagrams render through the local renderer          |
