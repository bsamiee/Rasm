# [PY_GEOMETRY_ARCHITECTURE]

`geometry` is one geometry-and-interchange surface: the tessellation companion daemon is the load-bearing cross-boundary owner, every analysis verb folds onto the daemon's geometry receipt, and every result graduates through the compute `HandoffAxis` geometry case. Mechanics live in the finalized `.planning/` pages; this page is the atlas — the implementation source tree, the owner registry (the one owner-state surface), dependency direction, cross-folder seams, the package boundaries, and the prohibitions.

## [1]-[SOURCE_TREE]

The planned module layout IS the build order: the tessellation daemon first as the load-bearing cross-boundary owner, then the analysis verbs that ride alongside it, then the scan and algebra owners. Each leaf is annotated with the owners it transcribes and the owning page#cluster.

```text codemap
geometry/
├── companion.py         # IfcCompanion — ifc-companion#DAEMON
├── analysis.py          # IfcAnalysis — ifc-analysis#ANALYSIS
├── scan.py              # ScanProcessing — scan-processing#REGISTRATION
└── algebra.py           # GeometryAlgebra — geometry-algebra#ALGEBRA
```

`companion.py` lands first: `IfcCompanion` drives IFC bytes plus deflection/tolerance into GLB and semantic XML/JSON through `IfcConvert` and the `ifcopenshell.geom.iterator`, hosted by the runtime `ServerHost` over the inbound gRPC contract, content-addressed via runtime `ContentIdentity`. `analysis.py` follows with `IfcAnalysis` running quantity takeoff, clash detection, space-program validation, Pset/schedule queries, and IDS-style model-checking over `ifcopenshell.util` and `ifcopenshell.api`, emitting a geometry receipt. `scan.py` holds `ScanProcessing`, one registration owner discriminating by mode row (the broad ICP family over open3d, the GICP/VGICP speed path over small_gicp) plus estimation, downsampling, and reconstruction. `algebra.py` lands last with `GeometryAlgebra` folding non-manifold cell/aperture topology over topologicpy and AEC computational geometry over compas into one algebra-kind-discriminated owner.

## [2]-[OWNER_REGISTRY]

The single owner-state surface for the package. Implementation collapses to one owner per axis and one entrypoint family per rail; density means no parallel rails, no near-duplicate shapes, no re-derived logic — a file is as large as its owner's concern requires, never trimmed to a line count. A new feature is a row or case, never a new surface; a public type outside these owner regions is the named defect. `[STATE]` is `FINALIZED` where the owner is a transcription-complete fence with no open gate, `SPIKE` where the owner is fence-complete but its proof carries a residual floor/bridge probe named in the page RESEARCH cluster. This is the ONLY place owner state lives.

| [INDEX] | [AXIS/RAIL]         | [OWNER]           | [KIND]                  | [CASES]                                 | [PAGE#CLUSTER]               | [STATE] |
| :-----: | :------------------ | :---------------- | :---------------------- | :-------------------------------------- | :--------------------------- | :-----: |
|   [1]   | tessellation daemon | `IfcCompanion`    | boundary capsule        | `tessellate`/`semantic`/`warm`          | ifc-companion#DAEMON         |  SPIKE  |
|   [2]   | IFC analysis        | `IfcAnalysis`     | static surface          | quantity/clash/space/pset/ids verbs     | ifc-analysis#ANALYSIS        |  SPIKE  |
|   [3]   | scan registration   | `ScanProcessing`  | frozen owner + mode row | icp/colored-icp/generalized/vgicp       | scan-processing#REGISTRATION |  SPIKE  |
|   [4]   | geometry algebra    | `GeometryAlgebra` | tagged union            | topology/network/form-finding/numerical | geometry-algebra#ALGEBRA     |  SPIKE  |

The companion GLB output keys by one runtime `ContentIdentity` key with deflection/tolerance folded into the cache key; topologicpy and compas collapse into one `GeometryAlgebra` owner discriminating by algebra-kind row, never two thin 1:1 wrappers. Registration transforms, reconstructed meshes, and form-found results graduate through the compute `HandoffAxis` geometry case.

## [3]-[DEPENDENCY_DIRECTION]

| [INDEX] | [PACKAGE]   | [MAY_REFERENCE_GEOMETRY] | [GEOMETRY_MAY_REFERENCE] | [BOUNDARY]                                       |
| :-----: | :---------- | :----------------------: | :----------------------: | :----------------------------------------------- |
|   [1]   | `runtime`   |            no            |           yes            | serves through `ServerHost`; content key consumed inward |
|   [2]   | `compute`   |            no            |           yes            | geometry evidence graduates through `HandoffAxis` |
|   [3]   | `data`      |            no            |            no            | mesh-file exchange stays at `data`               |
|   [4]   | `artifacts` |            no            |            no            | 3D scientific visualization stays at `artifacts` |

`geometry` consumes runtime `ServerHost`, `ContentIdentity`, rails, lanes, and `ReceiptContributor` and never re-mints them, and graduates evidence through the compute `HandoffAxis` geometry case. The companion speaks the existing inbound gRPC contract and mints no transport and no second wire vocabulary; the cross-language wire contract, the in-process semantic/glTF ownership, and the content-key seed parity ride the Tier-0 `region-map/seam-splits.md`.

## [4]-[SEAMS]

Every two-folder fact splits by altitude: mechanics live at the named geometry cluster, consequences land at the consumer. Intra-Python seams ride `pkg/page#CLUSTER`; cross-language consequences ride the Tier-0 `region-map/seam-splits.md` and are referenced as a Tier-0 seam, never restated here.

| [INDEX] | [SEAM]              | [MECHANICS_AT]                | [CONSEQUENCE_AT]                                                          |
| :-----: | :------------------ | :---------------------------- | :----------------------------------------------------------------------- |
|   [1]   | companion serve     | runtime/server-host#SERVE     | ifc-companion#DAEMON hosts the tessellation daemon through `ServerHost`   |
|   [2]   | content identity    | runtime/content-identity#IDENTITY | ifc-companion#DAEMON GLB output keys by one `ContentIdentity` with deflection/tolerance folded in |
|   [3]   | geometry receipt    | ifc-analysis#ANALYSIS         | runtime/observability#RECEIPT wires the geometry receipt through `ReceiptContributor` |
|   [4]   | geometry graduation | scan-processing#REGISTRATION  | compute/graduation#GRADUATION accepts registration/mesh/topology/form-finding evidence through the `HandoffAxis` geometry case |
|   [5]   | algebra graduation  | geometry-algebra#ALGEBRA      | compute/graduation#GRADUATION accepts form-found and topology results through the `HandoffAxis` geometry case |

## [5]-[BOUNDARIES]

- `geometry` is not a host package, durable store, bridge-lifecycle owner, or UI-state owner; the companion is purely the tessellation hop the managed surface cannot perform.
- The companion speaks the existing inbound gRPC contract through the runtime `ServerHost`; it mints no transport and no second wire vocabulary.
- In-process IFC semantic projection and in-process glTF authoring are owned at the managed boundary; this package adds the tessellation hop and the analysis verbs that hop alone drops. The cross-language ownership split rides the Tier-0 ledger.
- Mesh-file exchange and remote AEC streams stay in `data`/`runtime`; 3D scientific visualization stays in `artifacts`.
- The copyleft companion-floor license is satisfied at the process boundary by the isolated companion environment keeping the copyleft wheel out of the permissive library lock.
- Statement carve-outs are named per fence: the `IfcCompanion` daemon verbs (`tessellate`/`semantic`/`warm`) are the boundary capsule; every other member stays expression-shaped.

## [6]-[PROHIBITIONS]

The closed NEVER list — the deleted patterns the owner registry forecloses.

- NEVER mint a transport, channel, or second wire vocabulary; the companion speaks the existing inbound gRPC contract through the runtime `ServerHost`.
- NEVER re-mint content identity; the companion GLB key is one runtime `ContentIdentity` key with deflection/tolerance folded in.
- NEVER re-derive the BIM space-graph the managed semantic model already projects in-process; topologicpy is admitted only for non-manifold cell/aperture analysis the managed side does not extract.
- NEVER author glTF or IFC semantic in-process; the managed boundary owns those. The companion is purely the tessellation hop the managed surface cannot do.
- NEVER fold topologicpy and compas into two thin 1:1 wrappers; they collapse into one `GeometryAlgebra` owner discriminating by algebra-kind row.
- NEVER touch managed interiors, host lifecycles, durable stores, bridge lifecycle code, or UI state.
