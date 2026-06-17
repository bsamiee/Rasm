# [PY_GEOMETRY_FEATURES]

The realized capability list for geometry and IFC/BIM interchange. Every feature is a row or case on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`.

## [1]-[TESSELLATION_AND_ANALYSIS]

The persistent tessellation companion daemon and the IFC analysis verbs the tessellation hop alone drops.

| [INDEX] | [FEATURE]                                                                  | [PAGE#CLUSTER]        |
| :-----: | :------------------------------------------------------------------------ | :-------------------- |
|   [1]   | Persistent IfcOpenShell tessellation daemon over the inbound gRPC contract | ifc-companion#DAEMON  |
|   [2]   | IFC bytes plus deflection/tolerance into GLB and semantic XML/JSON        | ifc-companion#DAEMON  |
|   [3]   | Content-addressed output keyed by deflection/tolerance for cache-by-reference | ifc-companion#DAEMON  |
|   [4]   | Warm-pool daemon amortizing cold-start across requests                    | ifc-companion#DAEMON  |
|   [5]   | Quantity takeoff, clash detection, space-program validation              | ifc-analysis#ANALYSIS |
|   [6]   | Pset/schedule queries and IDS-style model-checking verbs                  | ifc-analysis#ANALYSIS |

## [2]-[SCAN_AND_ALGEBRA]

Point-cloud registration and reconstruction, and the one non-manifold-topology plus AEC computational-geometry owner.

| [INDEX] | [FEATURE]                                                                  | [PAGE#CLUSTER]               |
| :-----: | :------------------------------------------------------------------------ | :--------------------------- |
|   [7]   | One registration owner discriminating ICP/colored-ICP/generalized/VGICP   | scan-processing#REGISTRATION |
|   [8]   | Normal/feature estimation, voxel downsampling, surface/mesh reconstruction | scan-processing#REGISTRATION |
|   [9]   | Non-manifold cell/aperture topology over topologicpy                      | geometry-algebra#ALGEBRA     |
|  [10]   | AEC computational geometry, networks, assemblies, form-finding over compas | geometry-algebra#ALGEBRA     |
