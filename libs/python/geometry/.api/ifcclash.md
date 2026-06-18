# [PY_GEOMETRY_API_IFCCLASH]

`ifcclash` supplies IFC geometry clash detection: it loads IFC models into OpenCASCADE, runs intersection/collision/clearance tests across `ClashSet` pairs defined by `ClashSource` selectors, and emits `ClashResult` records that can be grouped spatially and exported as JSON or BCF issues.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcclash`
- package: `ifcclash`
- module: `ifcclash.ifcclash`
- asset: runtime library
- rail: ifc-companion / clash-detection

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: clash configuration family
- rail: clash-detection

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]    | [RAIL]                                      |
| :-----: | :-------------- | :--------------- | :------------------------------------------ |
|   [1]   | `Clasher`       | detection engine | runs clash pipeline against configured sets |
|   [2]   | `ClashSettings` | settings carrier | `logger` and `output` path configuration    |
|   [3]   | `ClashSet`      | TypedDict        | named pair of `ClashSource` lists with mode |
|   [4]   | `ClashSource`   | TypedDict        | `file` path + optional `selector` + `mode`  |

[PUBLIC_TYPE_SCOPE]: result family
- rail: clash-detection output

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                                                    |
| :-----: | :------------ | :------------ | :-------------------------------------------------------- |
|   [1]   | `ClashResult` | TypedDict     | pair of GUIDs, IFC classes, names, type, points, distance |
|   [2]   | `ClashGroup`  | TypedDict     | spatially clustered `dict[str, entity_instance]`          |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Clasher lifecycle
- rail: clash-detection

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :------------------------------------------------------------------------ | :------------- | :-------------------------------------- |
|   [1]   | `Clasher(settings: ClashSettings)`                                        | constructor    | bind engine to settings                 |
|   [2]   | `Clasher.load_ifc(path: str) -> ifcopenshell.file`                        | loader         | open and cache IFC file                 |
|   [3]   | `Clasher.add_collision_objects(...)`                                      | setup          | register OpenCASCADE collision objects  |
|   [4]   | `Clasher.clash() -> None`                                                 | runner         | execute all configured clash sets       |
|   [5]   | `Clasher.process_clash_set(clash_set: ClashSet) -> None`                  | runner         | execute one clash set                   |
|   [6]   | `Clasher.smart_group_clashes(clash_sets, max_clustering_distance: float)` | grouper        | spatial cluster clashes                 |
|   [7]   | `Clasher.create_group(...)`                                               | grouper        | create a `ClashGroup`                   |
|   [8]   | `Clasher.export() -> None`                                                | exporter       | write results to configured output path |
|   [9]   | `Clasher.export_json() -> None`                                           | exporter       | write JSON clash report                 |
|  [10]   | `Clasher.export_bcfxml() -> None`                                         | exporter       | write BCF issue archive                 |
|  [11]   | `Clasher.get_viewpoint_snapshot(...)`                                     | exporter       | capture viewpoint image for BCF issue   |

[ENTRYPOINT_SCOPE]: ClashSet schema
- rail: clash-detection configuration

| [INDEX] | [FIELD]          | [TYPE]                                         | [REQUIRED] |
| :-----: | :--------------- | :--------------------------------------------- | :--------: |
|   [1]   | `name`           | `str`                                          |    [Y]     |
|   [2]   | `a`              | `list[ClashSource]`                            |    [Y]     |
|   [3]   | `b`              | `list[ClashSource]`                            |    [N]     |
|   [4]   | `mode`           | `'intersection' \| 'collision' \| 'clearance'` |    [Y]     |
|   [5]   | `tolerance`      | `float`                                        |    [N]     |
|   [6]   | `clearance`      | `float`                                        |    [N]     |
|   [7]   | `check_all`      | `bool`                                         |    [N]     |
|   [8]   | `allow_touching` | `bool`                                         |    [N]     |
|   [9]   | `clashes`        | `dict[str, ClashResult]`                       |    [N]     |

[ENTRYPOINT_SCOPE]: ClashSource schema
- rail: clash-detection configuration

| [INDEX] | [FIELD]    | [TYPE]              | [REQUIRED] |
| :-----: | :--------- | :------------------ | :--------: |
|   [1]   | `file`     | `str`               |    [Y]     |
|   [2]   | `mode`     | `'a' \| 'e' \| 'i'` |    [N]     |
|   [3]   | `selector` | `str`               |    [N]     |
|   [4]   | `ifc`      | `ifcopenshell.file` |    [N]     |

[ENTRYPOINT_SCOPE]: ClashResult schema
- rail: clash-detection output

| [INDEX] | [FIELD]       | [TYPE]                             |
| :-----: | :------------ | :--------------------------------- |
|   [1]   | `a_global_id` | `str`                              |
|   [2]   | `b_global_id` | `str`                              |
|   [3]   | `a_ifc_class` | `str`                              |
|   [4]   | `b_ifc_class` | `str`                              |
|   [5]   | `a_name`      | `str`                              |
|   [6]   | `b_name`      | `str`                              |
|   [7]   | `type`        | `ifcopenshell.geom.main.ClashType` |
|   [8]   | `p1`          | `list[float]`                      |
|   [9]   | `p2`          | `list[float]`                      |
|  [10]   | `distance`    | `float`                            |

## [4]-[IMPLEMENTATION_LAW]

[CLASH_TOPOLOGY]:
- single production module: `ifcclash.ifcclash`
- detection modes: `intersection` (geometry overlap), `collision` (touching/penetrating), `clearance` (within clearance distance)
- `ClashSource.mode` selector: `'a'` = all, `'e'` = by IfcClass expression, `'i'` = by IfcGuid
- `ClashSettings.output` defaults to `'clashes.json'`; set `logger` to a Python `logging.Logger`
- `smart_group_clashes` clusters `ClashResult` points spatially using `max_clustering_distance`

[LOCAL_ADMISSION]:
- Import from `ifcclash.ifcclash`: `Clasher`, `ClashSettings`, `ClashSet`, `ClashSource`, `ClashResult`, `ClashGroup`
- Construct a `ClashSettings` instance, set `output` and `logger`, then pass to `Clasher`
- Populate `Clasher` with `ClashSet` dicts before calling `clash()`; results accumulate in `ClashSet['clashes']`
- `export_bcfxml` depends on `bcf` and `ifcopenshell` being importable; fails silently if absent

[RAIL_LAW]:
- Package: `ifcclash`
- Owns: IFC geometry intersection/collision/clearance detection and result export
- Accept: `ifcopenshell`-loaded or path-based IFC sources; `ClashSet` dicts for configuration
- Reject: custom geometry intersection kernels or duplicate BCF export outside this package
