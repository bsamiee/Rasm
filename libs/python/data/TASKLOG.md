# [PY_DATA_TASKLOG]

Open and closed work for `data`, distilled from `IDEAS.md`. Each task card leads with a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` open; `[COMPLETE]` or `[DROPPED]` closed — and carries the capability or file to build, the external packages to integrate, the integration points and boundaries, and the key considerations. `[1]-[OPEN]` holds live work; `[2]-[CLOSED]` records finished or dropped tasks.

## [1]-[OPEN]

[QUEUED] Author the `laspy` folder `.api` catalogue (host-environmental, not an upstream blocker) — `laspy` 2.7.0 is pure-Python cp315-clean and lock-resolved; the LAS/LAZ surface (`read`/`LasData.{x,y,z,header,write}`/`PointFormat.{id,dimension_names}`/`header.parse_crs` for `spatial/mesh.md#POINTCLOUD` `[LASPY_SURFACE]`) catalogues by reflection once the authoring host carries `laspy` + `assay api`, blocked on host provisioning alone.

[BLOCKED] Author the `pdal` folder `.api` catalogue (genuine upstream blocker) — `pdal` rides the `<'3.15'` gated band, sdist-only with no cp315 wheel; its COPC octree-subset surface (`Reader.copc(path, bounds=)`/`Pipeline.{execute,arrays}` for `spatial/mesh.md#POINTCLOUD` `[PDAL_COPC]`) stays RESEARCH until the `<3.15` companion host installs the dist and reflects the spelling.

[BLOCKED] Confirm the scalar `LanceDataset.version` accessor (genuine catalogue gap) — the `pylance` `.api` lists only `versions()`; the scalar `.version` integer property the `_lance_receipt` reads (`tabular/lakehouse.md` `[LANCE_VERSION]`) stays catalogue-pending until reflection against the live `pylance` distribution captures the `.version` accessor.

## [2]-[CLOSED]

(none)
