# [PY_GEOMETRY_API_IFCCLASH]

`ifcclash` owns IFC geometry clash detection for the geometry ifc-analysis rail: it loads IFC models into the OpenCASCADE spatial index through `ifcopenshell.geom.tree`, runs intersection, collision, and clearance tests across `ClashSet` pairs selected by `ClashSource`, and emits `ClashResult` records that `smart_group_clashes` clusters spatially and `export` writes as JSON or a BCF archive. `ifcopenshell` owns the overlap tree as its OCC geometry, and `ifc/analysis.md` composes the engine into its `CLASH` and `BCF` verbs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcclash`
- package: `ifcclash` (LGPL-3.0-or-later)
- module: `ifcclash.ifcclash`
- rail: ifc-analysis / clash-detection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: clash configuration family

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :-------------- | :------------ | :---------------------------------------------- |
|  [01]   | `Clasher`       | class         | runs the clash pipeline against configured sets |
|  [02]   | `ClashSettings` | class         | carries `logger` and the `output` path          |
|  [03]   | `ClashSet`      | TypedDict     | named pair of `ClashSource` lists with a mode   |
|  [04]   | `ClashSource`   | TypedDict     | `file` path plus optional `selector` and `mode` |

[PUBLIC_TYPE_SCOPE]: result family

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------ | :------------ | :---------------------------------------------------- |
|  [01]   | `ClashResult` | TypedDict     | GUID pair, IFC classes, names, type, points, distance |
|  [02]   | `ClashGroup`  | TypedDict     | spatially clustered `dict[str, entity_instance]`      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Clasher lifecycle

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Clasher(settings)`                                              | ctor     | bind the engine to `ClashSettings`        |
|  [02]   | `clash_sets`                                                     | property | the `ClashSet` list `clash()` iterates    |
|  [03]   | `load_ifc(path) -> file`                                         | instance | open and cache an IFC file                |
|  [04]   | `add_collision_objects(name, ifc_file, source)`                  | instance | register OCC collision objects            |
|  [05]   | `clash()`                                                        | instance | run every `clash_sets` into `clashes`     |
|  [06]   | `process_clash_set(clash_set)`                                   | instance | run one clash set                         |
|  [07]   | `smart_group_clashes(clash_sets, max_distance)`                  | instance | cluster clashes; write per-clash handles  |
|  [08]   | `create_group(name)`                                             | instance | create a `ClashGroup`                     |
|  [09]   | `export()`                                                       | instance | write results to `ClashSettings.output`   |
|  [10]   | `export_json()`                                                  | instance | write the JSON clash report               |
|  [11]   | `export_bcfxml()`                                                | instance | write the BCF archive                     |
|  [12]   | `get_viewpoint_snapshot(viewpoint) -> tuple[str, bytes] \| None` | instance | capture a viewpoint image for a BCF issue |

[ENTRYPOINT_SCOPE]: ClashSet schema

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

| [INDEX] | [FIELD]    | [TYPE]              | [REQUIRED] |
| :-----: | :--------- | :------------------ | :--------: |
|  [01]   | `file`     | `str`               |    [Y]     |
|  [02]   | `mode`     | `'a' \| 'e' \| 'i'` |    [N]     |
|  [03]   | `selector` | `str`               |    [N]     |
|  [04]   | `ifc`      | `ifcopenshell.file` |    [N]     |

[ENTRYPOINT_SCOPE]: ClashResult schema

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

[TOPOLOGY]:
- `clash()` folds every `ClashSet` into its own `clashes: dict[str, ClashResult]` map in place; mode selection is the closed `ClashSet` literal, never a per-mode runner family.
- `ClashSet.mode` selects `intersection` (geometry overlap under `tolerance`), `collision` (touching or penetrating, gated by `allow_touching`), or `clearance` (within `ClashSet.clearance`).
- `ClashSource.mode` selects `'a'` (whole model), `'e'` (exclude the `selector` set), or `'i'` (only the `selector` set); `'e'` and `'i'` consume the `ClashSource.selector` string an `IfcSelector` gate validated.
- `smart_group_clashes` clusters overlaps through `sklearn.cluster.OPTICS` at `max_clustering_distance`, keyed by `ClashResult` centroid geometry.
- `ClashSettings.output` defaults to `'clashes.json'`; `export()` dispatches to `export_bcfxml()` when `output` ends `.bcf`, else `export_json()`.
- Import `ifcclash.ifcclash` function-local at boundary scope under `# ruff:ignore[import-outside-top-level]`; the manifest import policy bans the module-level form.

[STACKING]:
- `ifcopenshell`(`.api/ifcopenshell.md`): `Clasher` opens files through `ifcopenshell.open`, indexes elements in `ifcopenshell.geom.tree` via `geom.iterator`, resolves selector sides through `ifcopenshell.util.selector.filter_elements`, and keys every `ClashResult` by `entity_instance.GlobalId` — the overlap tree is the OCC geometry itself.
- `bcf-client`(`.api/bcf-client.md`): each `ClashResult` mints one BCF issue — `BcfXml.create_new` opens the archive, `add_topic` records the offending pair, and `add_viewpoint_from_point_and_guids(p1, a_global_id, b_global_id)` binds the viewpoint to the overlap point and GUIDs; `ifcclash` and `bcf` compose into one in-memory archive.
- `ifc/analysis.md`: its `CLASH` verb derives both `ClashSource.selector` sides from the shared `IfcSelector` gate, runs one `ClashSet` at a configured mode and tolerance, clusters with `smart_group_clashes`, and folds each overlap into `AnalysisRow.clash`; its `BCF` verb re-runs the clash leg and authors through `bcf-client` directly.

[LOCAL_ADMISSION]:
- Import `Clasher`, `ClashSettings`, `ClashSet`, `ClashSource`, `ClashResult`, `ClashGroup` from `ifcclash.ifcclash`.
- Construct `ClashSettings`, set `output` and `logger`, pass it to `Clasher`, assign `Clasher.clash_sets` before `clash()`, and read results from each `ClashSet['clashes']`.
- Pass `ClashSource.ifc` a pre-loaded `ifcopenshell.file` to skip `load_ifc`; derive `ClashSource.selector` only through the `IfcSelector` gate.
- `export_bcfxml` imports `bcf.v2.bcfxml` lazily on call.

[RAIL_LAW]:
- Package: `ifcclash`
- Owns: IFC geometry intersection, collision, and clearance detection, spatial clustering, and result export
- Accept: `ifcopenshell`-loaded or path-based IFC sources, `ClashSet` dicts, and `IfcSelector`-validated `ClashSource.selector` sides
- Reject: a custom intersection kernel where the OCC clash tree owns the overlap; a raw caller query threaded past the `IfcSelector` gate; a duplicate BCF export where the `ifc/analysis` `BCF` verb composes `ifcclash` and `bcf`
