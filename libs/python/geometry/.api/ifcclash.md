# [PY_GEOMETRY_API_IFCCLASH]

`ifcclash` supplies IFC geometry clash detection for the geometry ifc-analysis rail: it loads IFC models into OpenCASCADE through the `ifcopenshell.geom.tree` spatial index, runs intersection/collision/clearance tests across `ClashSet` pairs defined by `ClashSource` selectors, and emits `ClashResult` records that can be grouped spatially through `smart_group_clashes` and exported as JSON or BCF issues. It rides the `ifcopenshell` companion lane (`0.8.5`, depends `ifcopenshell`; `bcf`/`numpy` for the grouper and BCF export). `ifc/analysis.md#ANALYSIS` is the integration owner: the `CLASH` verb derives its two `ClashSource.selector` sides from the shared `IfcSelector` validated gate (the lark grammar), runs one `ClashSet` at a configurable mode/tolerance, clusters with `smart_group_clashes`, and folds each overlap into the typed `AnalysisRow.clash` case; the `BCF` verb re-runs the clash leg and stacks each `ClashResult` INTO a `bcf-client` topic with a viewpoint, so `ifcclash`+`bcf` compose into one in-memory archive rather than a custom intersection kernel or a same-string round-trip.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcclash`
- package: `ifcclash`
- import: `from ifcclash.ifcclash import Clasher, ClashSettings` (the single production module is `ifcclash.ifcclash`)
- owner: `geometry`
- rail: ifc-analysis / clash-detection
- installed: `0.8.5`, the IfcOpenShell-ecosystem companion-lane band (depends `ifcopenshell`; `bcf`/`numpy` for grouping and BCF export); `assay api resolve ifcclash` resolves no source on the cp315 (`3.15.0b2`) core — resolves only where `ifcopenshell` resolves (cp313 companion), the documented lane gap, not a catalog fault
- license: LGPL-3.0-or-later (the IfcOpenShell-ecosystem license)
- wheel-floor: pure-Python `py3-none-any` wheel for `ifcclash` itself, but inert without the `ifcopenshell` cp313 core it depends on (no cp315 wheel); the OpenCASCADE clash kernel links through `ifcopenshell.geom`; ABI: none for `ifcclash`, OCC native via the `ifcopenshell` spine
- entry points: none (library only)
- capability: load IFC models into the OpenCASCADE clash tree, run intersection/collision/clearance tests across `ClashSet` pairs, spatially cluster overlaps via `smart_group_clashes`, and export results as JSON or BCF issues with viewpoint snapshots

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
|  [02]   | `Clasher.clash_sets: list[ClashSet]`                                      | input slot     | the configured `ClashSet` list `clash()` iterates; results accumulate back into each set's `clashes` map in place |
|  [03]   | `Clasher.load_ifc(path: str) -> ifcopenshell.file`                        | loader         | open and cache IFC file (skipped when `ClashSource.ifc` carries a pre-loaded model) |
|  [04]   | `Clasher.add_collision_objects(...)`                                      | setup          | register OpenCASCADE collision objects via `ifcopenshell.geom.tree` |
|  [05]   | `Clasher.clash() -> None`                                                 | runner         | execute all `clash_sets`, populating each `ClashSet['clashes']` |
|  [06]   | `Clasher.process_clash_set(clash_set: ClashSet) -> None`                  | runner         | execute one clash set                   |
|  [07]   | `Clasher.smart_group_clashes(clash_sets, max_clustering_distance: float)` | grouper        | spatially cluster clashes; writes the per-clash cluster handle into the clash-set internal state |
|  [08]   | `Clasher.create_group(...)`                                               | grouper        | create a `ClashGroup`                   |
|  [09]   | `Clasher.export() -> None`                                                | exporter       | write results to `ClashSettings.output` |
|  [10]   | `Clasher.export_json() -> None`                                           | exporter       | write JSON clash report                 |
|  [11]   | `Clasher.export_bcfxml() -> None`                                         | exporter       | write BCF issue archive (requires `bcf` + `ifcopenshell`) |
|  [12]   | `Clasher.get_viewpoint_snapshot(...)`                                     | exporter       | capture viewpoint image for BCF issue   |

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
- import: `from ifcclash.ifcclash import Clasher, ClashSettings` function-local at boundary scope under `# noqa: PLC0415`; module-level import is banned by the manifest import policy. The single production module is `ifcclash.ifcclash`.
- detection modes: `ClashSet.mode` selects `intersection` (geometry overlap), `collision` (touching/penetrating), `clearance` (within `ClashSet.clearance` distance); the mode is the closed `ClashSet` literal, never a per-mode runner family.
- selector mode: `ClashSource.mode` selects `'a'` = all (whole-model side), `'e'` = by IfcClass selector expression, `'i'` = by IfcGuid; the `'e'` mode consumes the `ClashSource.selector` string the `IfcSelector` gate validated, never a raw caller query.
- settings: `ClashSettings.output` defaults to `'clashes.json'`; set `logger` to a Python `logging.Logger`.
- run-and-cluster: the integration owner writes one `ClashSet` into `Clasher.clash_sets` at the `'intersection'` mode plus a `tolerance` policy, calls `clash()` (populating each set's `clashes: dict[str, ClashResult]` map in place), then `smart_group_clashes(clash_sets, max_clustering_distance)` in one pass, reads back the `clashes` map values, and derives the spatial-cluster index — so a `CLASH` row carries its `ClashResult` GlobalId pair, penetration `distance`, and cluster index in one pass.
- grouper output shape: `smart_group_clashes` clusters `ClashResult` points spatially using `max_clustering_distance` and writes the per-clash cluster handle into the clash-set internal state. The exact spelling the grouper writes the cluster handle under — whether `clash_set["clash_groups"]` keyed by group id with each `ClashResult` carrying a `group` back-reference, or a flat re-keyed `clashes` map — is the one internal-shape detail the live cp313 run confirms; the integration owner defends with `.get(default)` so no row drops and the cluster index defaults to `0` until the grouper handle is confirmed (a non-empty result then reports a single cluster). This is the load-bearing residual the catalog cannot settle without a live run.
- bcf stacking: the `BCF` analysis verb re-runs the clash leg, then mints one `bcf-client` topic per overlap — `BcfXml.create_new` opens the project, `add_topic(..., topic_type="Clash")` mints the topic, and `add_viewpoint_from_point_and_guids(ClashResult.p1, a_global_id, b_global_id)` binds each topic to the overlap point and the offending GUIDs — so the BCF output is the issue-authoring leg that stacks `ifcclash`+`bcf` into one archive, never a same-string round-trip and never the `ifctester` `reporter.Bcf` IDS-side archive.

[LOCAL_ADMISSION]:
- Import from `ifcclash.ifcclash`: `Clasher`, `ClashSettings`, `ClashSet`, `ClashSource`, `ClashResult`, `ClashGroup`
- Construct a `ClashSettings` instance, set `output` and `logger`, then pass to `Clasher`
- Assign `Clasher.clash_sets` a list of `ClashSet` dicts before calling `clash()`; results accumulate in each `ClashSet['clashes']`
- Pass `ClashSource.ifc` a pre-loaded `ifcopenshell.file` to skip the `load_ifc` path; derive the `ClashSource.selector` only through the shared `IfcSelector` validated gate, never a raw caller query string
- `export_bcfxml` depends on `bcf` and `ifcopenshell` being importable; fails silently if absent — the `ifc/analysis.md#ANALYSIS` `BCF` verb authors through `bcf-client` directly instead, treating `ifcclash` as the overlap source

[RAIL_LAW]:
- Package: `ifcclash`
- Owns: IFC geometry intersection/collision/clearance detection, spatial clustering, and result export
- Accept: `ifcopenshell`-loaded or path-based IFC sources; `ClashSet` dicts for configuration; `IfcSelector`-validated `ClashSource.selector` sides
- Reject: a custom geometry intersection kernel where the OCC clash tree owns the overlap; a raw caller query threaded into `ClashSource.selector` past the `IfcSelector` gate; a duplicate BCF export where the `ifc/analysis.md` `BCF` verb stacks `ifcclash`+`bcf`

[CAPTURE_GAP]:
- floor: companion interpreter cp313; `ifcclash==0.8.5` rides the `ifcopenshell` companion lane, so reflection resolves where `ifcopenshell` resolves; the `>=3.15` project venv carries no `ifcopenshell` core and `assay api resolve ifcclash` returns no source on the cp315 core (the documented marker gap, not a catalog fault)
- members: the `Clasher`/`ClashSettings`/`ClashSet`/`ClashSource`/`ClashResult`/`ClashGroup` family, the `clash_sets` input slot, the `clash()`/`process_clash_set()`/`smart_group_clashes()` runners, and the `ClashResult` `a_global_id`/`b_global_id`/`p1`/`distance` fields confirm by introspection against the installed cp313 companion distribution. The per-clash cluster-handle spelling `smart_group_clashes` writes (the `clash_groups` key / `group` back-reference) is the one internal-shape detail the live run confirms before the cluster index binds; the fence defaults it to `0` so no row drops in the interim.
