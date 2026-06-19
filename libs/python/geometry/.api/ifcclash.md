# [PY_GEOMETRY_API_IFCCLASH]

`ifcclash` supplies IFC geometry clash detection: it loads IFC models into OpenCASCADE, runs intersection/collision/clearance tests across `ClashSet` pairs defined by `ClashSource` selectors, and emits `ClashResult` records that can be grouped spatially and exported as JSON or BCF issues.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcclash`
- package: `ifcclash`
- module: `ifcclash.ifcclash`
- asset: runtime library
- rail: ifc-companion / clash-detection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: clash configuration family
- rail: clash-detection

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]    | [RAIL]                                      |
| :-----: | :-------------- | :--------------- | :------------------------------------------ |
|  [01]   | `Clasher`       | detection engine | runs clash pipeline against configured sets |
|  [02]   | `ClashSettings` | settings carrier | `logger` and `output` path configuration    |
|  [03]   | `ClashSet`      | TypedDict        | named pair of `ClashSource` lists with mode |
|  [04]   | `ClashSource`   | TypedDict        | `file` path + optional `selector` + `mode`  |

[PUBLIC_TYPE_SCOPE]: result family
- rail: clash-detection output

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                                                    |
| :-----: | :------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `ClashResult` | TypedDict     | pair of GUIDs, IFC classes, names, type, points, distance |
|  [02]   | `ClashGroup`  | TypedDict     | spatially clustered `dict[str, entity_instance]`          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Clasher lifecycle
- rail: clash-detection

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY] | [RAIL]                                  |
| :-----: | :------------------------------------------------------------------------ | :------------- | :-------------------------------------- |
|  [01]   | `Clasher(settings: ClashSettings)`                                        | constructor    | bind engine to settings                 |
|  [02]   | `Clasher.load_ifc(path: str) -> ifcopenshell.file`                        | loader         | open and cache IFC file                 |
|  [03]   | `Clasher.add_collision_objects(...)`                                      | setup          | register OpenCASCADE collision objects  |
|  [04]   | `Clasher.clash() -> None`                                                 | runner         | execute all configured clash sets       |
|  [05]   | `Clasher.process_clash_set(clash_set: ClashSet) -> None`                  | runner         | execute one clash set                   |
|  [06]   | `Clasher.smart_group_clashes(clash_sets, max_clustering_distance: float)` | grouper        | spatial cluster clashes                 |
|  [07]   | `Clasher.create_group(...)`                                               | grouper        | create a `ClashGroup`                   |
|  [08]   | `Clasher.export() -> None`                                                | exporter       | write results to configured output path |
|  [09]   | `Clasher.export_json() -> None`                                           | exporter       | write JSON clash report                 |
|  [10]   | `Clasher.export_bcfxml() -> None`                                         | exporter       | write BCF issue archive                 |
|  [11]   | `Clasher.get_viewpoint_snapshot(...)`                                     | exporter       | capture viewpoint image for BCF issue   |

[ENTRYPOINT_SCOPE]: ClashSet schema
- rail: clash-detection configuration

| [INDEX] | [FIELD]          | [TYPE]                                         | [REQUIRED] |
| :-----: | :--------------- | :--------------------------------------------- | :--------: |
|  [01]   | `name`           | `str`                                          |    [Y]     |
|  [02]   | `a`              | `list[ClashSource]`                            |    [Y]     |
|  [03]   | `b`              | `list[ClashSource]`                            |    [N]     |
|  [04]   | `mode`           | `'intersection' \| 'collision' \| 'clearance'` |    [Y]     |
|  [05]   | `tolerance`      | `float`                                        |    [N]     |
|  [06]   | `clearance`      | `float`                                        |    [N]     |
|  [07]   | `check_all`      | `bool`                                         |    [N]     |
|  [08]   | `allow_touching` | `bool`                                         |    [N]     |
|  [09]   | `clashes`        | `dict[str, ClashResult]`                       |    [N]     |

[ENTRYPOINT_SCOPE]: ClashSource schema
- rail: clash-detection configuration

| [INDEX] | [FIELD]    | [TYPE]              | [REQUIRED] |
| :-----: | :--------- | :------------------ | :--------: |
|  [01]   | `file`     | `str`               |    [Y]     |
|  [02]   | `mode`     | `'a' \| 'e' \| 'i'` |    [N]     |
|  [03]   | `selector` | `str`               |    [N]     |
|  [04]   | `ifc`      | `ifcopenshell.file` |    [N]     |

[ENTRYPOINT_SCOPE]: ClashResult schema
- rail: clash-detection output

| [INDEX] | [FIELD]       | [TYPE]                             |
| :-----: | :------------ | :--------------------------------- |
|  [01]   | `a_global_id` | `str`                              |
|  [02]   | `b_global_id` | `str`                              |
|  [03]   | `a_ifc_class` | `str`                              |
|  [04]   | `b_ifc_class` | `str`                              |
|  [05]   | `a_name`      | `str`                              |
|  [06]   | `b_name`      | `str`                              |
|  [07]   | `type`        | `ifcopenshell.geom.main.ClashType` |
|  [08]   | `p1`          | `list[float]`                      |
|  [09]   | `p2`          | `list[float]`                      |
|  [10]   | `distance`    | `float`                            |

## [04]-[IMPLEMENTATION_LAW]

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
