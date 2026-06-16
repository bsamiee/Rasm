# [PY_GEOMETRY_PLANNING]

`geometry` is the first-class home of geometry + IFC/BIM interchange and the load-bearing cross-boundary package of the branch. It has zero consumers today; implementation is full-capability. These pages are decision-complete blueprints an implementation agent transcribes. The package owns the IfcOpenShell tessellation companion daemon (IFC to mesh/GLB + semantic XML/JSON), IFC property/quantity/relationship analysis, point-cloud/3D-scan registration and reconstruction, non-manifold topological modeling, and AEC computational geometry — consuming the runtime `ServerHost`, `ContentIdentity`, rails, lanes, and `ReceiptContributor` as settled vocabulary. The package is pinned under a SEPARATE companion interpreter floor (`python_version<'3.13'`) divorced from the `>=3.15` runtime floor.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                  | [OWNS]                                                             | [STATE]   |
| :-----: | :-------------------------------------- | :----------------------------------------------------------------- | :-------- |
|   [1]   | [ifc-companion](ifc-companion.md)       | the IfcOpenShell tessellation daemon over the C# gRPC contract     | finalized |
|   [2]   | [ifc-analysis](ifc-analysis.md)         | IFC property/quantity/relationship analysis (QTO/clash/rule-check) | finalized |
|   [3]   | [scan-processing](scan-processing.md)   | point-cloud/3D-scan registration, reconstruction                   | finalized |
|   [4]   | [geometry-algebra](geometry-algebra.md) | non-manifold topology + AEC computational geometry                 | finalized |

## [2]-[CATALOGUE_PENDING]

- All five distributions (`ifcopenshell`, `open3d`, `small-gicp`, `topologicpy`, `compas`) ride the `python_version<'3.13'` companion floor; the project `>=3.15` venv carries no wheel. Catalogue members are verified on a cp312 companion interpreter; first-class in-place `assay api query` lands when the floor/lock-scope decision admits the sub-3.13 environment (suite TASKLOG).

## [3]-[DENSITY_BAR]

Implementation collapses to one owner per axis. The four owners are re-derived on altitude, not the noun: `IfcCompanion` (the deep flagship daemon), `IfcAnalysis` (the AEC QTO/clash/rule-check verbs the tessellation hop drops), `ScanProcessing` (registration discriminating by mode row), and `GeometryAlgebra` (topologicpy + compas folded into ONE algebra owner discriminating by algebra-kind row). `[STATE]` carries `SPIKE` where the fence is complete but its proof carries a residual companion-floor or live-server probe named in the page RESEARCH cluster.

| [INDEX] | [AXIS/CONCERN]      | [OWNER]           | [KIND]                  | [CASES]                                 | [STATE] |
| :-----: | :------------------ | :---------------- | :---------------------- | :-------------------------------------- | :-----: |
|   [1]   | Tessellation daemon | `IfcCompanion`    | boundary capsule        | `tessellate`/`semantic`/`warm`          |  SPIKE  |
|   [2]   | IFC analysis        | `IfcAnalysis`     | static surface          | quantity/clash/space/pset/ids verbs     |  SPIKE  |
|   [3]   | Scan registration   | `ScanProcessing`  | frozen owner + mode row | icp/colored-icp/generalized/vgicp       |  SPIKE  |
|   [4]   | Geometry algebra    | `GeometryAlgebra` | tagged union            | topology/network/form-finding/numerical |  SPIKE  |

## [4]-[BUILD_ORDER]

| [INDEX] | [FILE]         | [TRANSCRIBES]                | [GATE]         |
| :-----: | :------------- | :--------------------------- | :------------- |
|   [1]   | `companion.py` | ifc-companion#DAEMON         | floor + bridge |
|   [2]   | `analysis.py`  | ifc-analysis#ANALYSIS        | floor          |
|   [3]   | `scan.py`      | scan-processing#REGISTRATION | floor          |
|   [4]   | `algebra.py`   | geometry-algebra#ALGEBRA     | floor          |

## [5]-[PROOF_GATES]

| [GATE] | [RAIL]           | [EVIDENCE]                                                                                                                                                     |
| :----: | :--------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [G1]  | companion floor  | all five pins reflect on the cp312 companion interpreter                                                                                                       |
|  [G2]  | `.api` catalogue | every fence member resolves to an `.api` row                                                                                                                   |
|  [G3]  | bridge           | the `IfcCompanion` daemon serves the C# `ComputeService`/`ArtifactSync` contract; blocked on `PY_FLOOR_001` admitting the sub-3.13 companion environment first |

## [6]-[PROHIBITIONS]

- [NEVER] mint a transport, channel, or second wire vocabulary; the companion speaks the EXISTING C# gRPC contract through the runtime `ServerHost`.
- [NEVER] re-mint content identity; the companion GLB key is one runtime `ContentIdentity` key with deflection/tolerance folded in.
- [NEVER] re-derive the BIM space-graph the C# `IfcSemanticModel` already projects in-process; `topologicpy` is admitted only for non-manifold cell/aperture analysis the managed side does not extract.
- [NEVER] author glTF or IFC semantic in-process; C# owns those (SharpGLTF, GeometryGym). The companion is purely the tessellation hop the managed surface cannot do.
- [NEVER] fold `topologicpy` and `compas` into two thin 1:1 wrappers; they collapse into one `GeometryAlgebra` owner discriminating by algebra-kind row.
- [NEVER] touch C# interiors, host lifecycles, durable stores, bridge lifecycle code, or TypeScript UI state.

## [7]-[ADMISSIONS_RECORD]

| [INDEX] | [PACKAGE]    | [PAGE]                      | [CATALOGUE]         | [STATUS]          |
| :-----: | :----------- | :-------------------------- | :------------------ | :---------------- |
|   [1]   | ifcopenshell | ifc-companion, ifc-analysis | api-ifcopenshell.md | catalogue-pending |
|   [2]   | open3d       | scan-processing             | api-open3d.md       | catalogue-pending |
|   [3]   | small-gicp   | scan-processing             | api-small_gicp.md   | catalogue-pending |
|   [4]   | topologicpy  | geometry-algebra            | api-topologicpy.md  | catalogue-pending |
|   [5]   | compas       | geometry-algebra            | api-compas.md       | catalogue-pending |

## [8]-[REFINEMENT_HORIZON]

Entry for the next deepening session: `libs/python/.planning/campaign-method.md`, then the suite `TASKLOG.md`, then this charter. Every owner is `SPIKE` until the floor/lock-scope decision admits the sub-3.13 companion environment and the daemon serves the C# contract end-to-end; the `IfcCompanion` warm-pool cold-start amortization (200-400ms Python+OCCT) and the 64 KiB ArtifactSync FrameEdge framing with per-frame Crc32 + whole-artifact XxHash128 are proven against the C# `ArtifactSync` descriptors. The bar: the C# managed surface tessellates any IFC through this companion over the existing wire, content-addressed for cache-hit-by-reference, with the AEC analysis verbs the tessellation hop alone drops fully owned.
