# [PY_GEOMETRY_API_IFCDIFF]

`ifcdiff` owns two-model IFC revision comparison over the `ifcopenshell` model: an `old`/`new` file pair diffs across the closed `RELATIONSHIP_TYPE` axis, folding numeric-tolerant `deepdiff.DeepDiff` over each survivor's attributes, tessellated shape, and property-set map into a GlobalId-keyed `change_register` and disjoint `added_elements`/`deleted_elements` GUID sets. It feeds the geometry ifc-analysis model-diff rail, scoping the compared element set through the shared `IfcSelector` gate.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcdiff`
- package: `ifcdiff` (LGPL-3.0-or-later)
- import: `import ifcdiff`
- owner: `geometry`
- rail: ifc-analysis / model-diff
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: diff owner and change axis

`RELATIONSHIP_TYPE`: `geometry` `attributes` `type` `property` `container` `aggregate` `classification`

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                                                       |
| :-----: | :------------------ | :------------ | :------------------------------------------------------------------------------------------------- |
|  [01]   | `IfcDiff`           | class         | comparison root holding `old`/`new`, the three result surfaces, and `precision`                    |
|  [02]   | `RELATIONSHIP_TYPE` | literal       | closed change-axis vocabulary (values in the scope line)                                           |
|  [03]   | `DiffTerminator`    | class         | `DeepDiff` custom-operator short-circuiting the shallow representation compare on the first change |

`IfcDiff.diff()` populates three disjoint surfaces: `change_register: dict[str, dict[str, Any]]` maps a survivor GlobalId to its `*_changed` marker dict, and `added_elements`/`deleted_elements: set[str]` carry the GlobalIds present only in `new` / only in `old`. Markers `geometry_changed`, `attributes_changed`, `type_changed`, `container_changed`, `aggregate_changed`, and `classification_changed` are `True` flags; `properties_changed` carries the full pset `DeepDiff` result (`True` on a comparison exception).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: diff construction, execution, and export

`IfcDiff(old, new, relationships=None, is_shallow=True, filter_elements=None)` constructs over two `ifcopenshell.file` models; `relationships` defaults to `["geometry"]` and a non-`geometry` subset skips the `geom.iterator` tessellation, while `filter_elements` pre-scopes the compared set.

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                                                         |
| :-----: | :------------------------------------------------ | :------- | :------------------------------------------------------------------- |
|  [01]   | `IfcDiff(old, new, ...)`                          | ctor     | construct the comparison over two `ifcopenshell.file` models         |
|  [02]   | `IfcDiff.diff() -> None`                          | instance | run the comparison, populating the three result surfaces in place    |
|  [03]   | `IfcDiff.export(path) -> None`                    | instance | write JSON `{added, deleted, changed}`                               |
|  [04]   | `IfcDiff.diff_element(old, new)`                  | instance | attribute-list `DeepDiff` feeding `attributes_changed`               |
|  [05]   | `IfcDiff.diff_element_relationships(old, new)`    | instance | per-axis type/property/container/aggregate/classification diff       |
|  [06]   | `IfcDiff.summarise_shapes(ifc, elements) -> dict` | instance | geometry leg: `geom.iterator` vertex-summary tessellation            |
|  [07]   | `IfcDiff.get_precision() -> float`                | instance | model `Precision` (default `1e-4`) feeding `DeepDiff` `math_epsilon` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- model axis: `IfcDiff(old, new).diff()` partitions the GlobalId sets by set difference into `deleted_elements`/`added_elements`, walks the surviving intersection through `diff_element`/`diff_element_relationships`, and writes `change_register` as the survivor map — three disjoint result surfaces, never one conflated map. `relationships` scopes which legs run; `RELATIONSHIP_TYPE` is the closed axis.
- change axis: each relationship leg writes one bounded `*_changed` marker — `attributes` a direct attribute-list `DeepDiff`, `property` a numeric-tolerant `DeepDiff` over `util.element.get_psets` carried under `properties_changed`, `type`/`container`/`aggregate`/`classification` through `util.element`/`util.classification` accessors, and `geometry` through `summarise_shapes` tessellation. Property leg owns the map compare, geometry leg the shape compare.
- selector axis: `filter_elements` scopes the compared set through `util.selector.filter_elements` before the set difference, threaded through the `IfcSelector` gate rather than a raw query.

[STACKING]:
- `ifcopenshell`(`.api/ifcopenshell.md`): `old`/`new` `ifcopenshell.file` pair drives the diff; the geometry leg tessellates through `geom.iterator` and the relationship legs read `util.element.get_psets`/`get_type`/`get_container`, `util.classification` references, and `util.selector.filter_elements` — every model accessor owned there, none re-derived.
- `ifc/costing.md`: `IfcLifecycle`'s `DIFF` phase composes `IfcDiff(model, ifcopenshell.open(revision)).diff()` and folds `change_register` with the `added_elements`/`deleted_elements` sets through `DiffChange.of_register`, whose arms map each `*_changed` marker onto the closed `DiffChange` case set; a non-`geometry` `relationships` subset skips tessellation while still keying the `DIFF` drift-fraction evidence the `LifecycleReceipt` reads.
- `ifc/selector.md`: `filter_elements` binds the validated `SelectorQuery.filter_string` — `IfcSelector.parse` → `filter_string` → `IfcDiff(..., filter_elements=validated)` re-serializes to the exact grammar `util.selector.filter_elements` consumes, scoping a revision diff to a discipline subset without a second selection engine.

[LOCAL_ADMISSION]:
- `geometry` ifc-analysis model-diff owner composes `IfcDiff(...).diff()`/`change_register` directly; quantity, cost, and transformation phases compose their own siblings on the shared lifecycle rail.

[RAIL_LAW]:
- Package: `ifcdiff`
- Owns: two-model IFC comparison — added/deleted GlobalId sets and a per-element `change_register` across the closed `RELATIONSHIP_TYPE` axis, with a JSON export
- Accept: two `ifcopenshell.file` models, an optional `relationships` subset, and an optional `filter_elements` selector threaded through the `IfcSelector` gate
- Reject: a hand-rolled `by_type` attribute-walk diff where `IfcDiff` owns the comparison; a stringified `deepdiff` blob where the bounded `*_changed` markers classify; a conflated single result map where the three surfaces stay disjoint; a raw `filter_elements` query where `IfcSelector` validates
