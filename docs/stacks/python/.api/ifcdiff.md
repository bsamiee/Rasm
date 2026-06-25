# [PY_GEOMETRY_API_IFCDIFF]

`ifcdiff` supplies the IFC two-model revision-comparison surface for the geometry ifc-analysis rail: the single-file `IfcDiff` owner diffs an `old`/`new` `ifcopenshell.file` pair across a closed `RELATIONSHIP_TYPE` axis (geometry/attributes/type/property/container/aggregate/classification), folding numeric-tolerant `deepdiff.DeepDiff` over each element's attribute graph, representation shape, and property-set map, and projecting the result into a GlobalId-keyed `change_register` plus disjoint `added_elements`/`deleted_elements` GUID sets. It rides the `ifcopenshell` companion lane (`0.8.5`; depends `ifcopenshell`, `deepdiff`, `numpy`, `orderly_set`, and `ifcopenshell.geom` for the representation leg), so the lifecycle owner composes `IfcDiff(...).diff()`/`change_register` directly rather than a `by_type`/attribute-walk diff, and threads its element pre-filter through the `IfcSelector` validated gate the analysis quantity arms share.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcdiff`
- package: `ifcdiff`
- import: `import ifcdiff` (single-module `ifcdiff.py`, not a package tree)
- owner: `geometry`
- rail: ifc-analysis / model-diff
- installed: `0.8.5`, the IfcOpenShell-ecosystem companion-lane band; depends `ifcopenshell`, `deepdiff`, `numpy`, `orderly_set`; license LGPL-3.0-or-later
- entry points: none (library only; the file additionally ships an `argparse` CLI `__main__` guard not consumed here)
- capability: two-model IFC comparison keyed by GlobalId across a closed relationship axis — added/deleted element GUID sets plus a per-element `change_register` flagging geometry, attribute, type, property-set (a nested `DeepDiff`), container, aggregate, and classification change — with a JSON export and a `filter_elements` selector pre-scope, the representation diff driven through `ifcopenshell.geom` tessellation under a numeric precision tolerance

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: diff owner and change axis
- rail: model-diff

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]   | [CAPABILITY]                                                                                                            |
| :-----: | :------------------------ | :--------------- | :-------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IfcDiff`                 | diff root        | drives the two-model comparison; holds `old`/`new`, `change_register`, `added_elements`/`deleted_elements`, `precision` |
|  [02]   | `RELATIONSHIP_TYPE`       | diff-axis literal | `Literal["geometry","attributes","type","property","container","aggregate","classification"]` — the closed change axes  |
|  [03]   | `IfcDiff.change_register` | change map       | `dict[GlobalId(str), dict[str, object]]` — per-element marker dict keyed by the `*_changed` change kind                 |
|  [04]   | `IfcDiff.added_elements`  | added set        | `set[str]` of GlobalIds present in `new` but not `old` (disjoint from `change_register`)                               |
|  [05]   | `IfcDiff.deleted_elements`| deleted set      | `set[str]` of GlobalIds present in `old` but not `new`                                                                  |
|  [06]   | `DiffTerminator`          | walk control     | the recursion-terminator helper bounding the dependent-entity representation walk                                      |

[CHANGE_MARKER_KEYS]: the `change_register[guid]` value-dict keys are the bounded `*_changed` vocabulary, NOT a stringified `deepdiff` blob — `geometry_changed: True`, `attributes_changed: True`, `type_changed: True`, `container_changed: True`, `aggregate_changed: True`, `classification_changed: True` are boolean flags, while `properties_changed` carries the full `DeepDiff` pset-comparison result (a dict, or `True` on a comparison exception). A single element accumulates multiple markers in one dict.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: diff construction, execution, and export
- rail: model-diff

| [INDEX] | [SURFACE]                                                                                  | [CALL_SHAPE]                          | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------------------------------------------------- | :------------------------------------ | :--------------------------------------------------- |
|  [01]   | `IfcDiff(old, new, relationships=None, is_shallow=True, filter_elements=None)`             | two `ifcopenshell.file` plus options  | construct the comparison; `relationships` defaults to `["geometry"]`, `filter_elements` a selector pre-scope |
|  [02]   | `IfcDiff.diff() -> None`                                                                   | none                                  | run the comparison; populates `change_register`/`added_elements`/`deleted_elements` in place |
|  [03]   | `IfcDiff.export(path: str) -> None`                                                        | path                                  | write a JSON report `{"added": [...], "deleted": [...], "changed": change_register}` |
|  [04]   | `IfcDiff.diff_element(old, new)`                                                            | two `entity_instance`                 | per-element attribute diff feeding `attributes_changed` |
|  [05]   | `IfcDiff.diff_element_relationships(old, new) -> bool`                                      | two `entity_instance`                 | per-relationship diff over the `relationships` axis    |
|  [06]   | `IfcDiff.diff_representation(old_rep_id, new_rep_id) -> bool`                               | two representation step ids           | tessellated representation-shape diff via `ifcopenshell.geom` |
|  [07]   | `IfcDiff.get_precision() -> float`                                                         | none                                  | derive the model length-precision feeding `DeepDiff` `math_epsilon` |

The `relationships` argument is the diff-axis selector: passing a subset of `RELATIONSHIP_TYPE` (e.g. `["attributes", "property"]`) skips the costly `ifcopenshell.geom` representation leg the `"geometry"` axis triggers.

## [04]-[IMPLEMENTATION_LAW]

[DIFF_TOPOLOGY]:
- import: `import ifcdiff` at boundary scope only; module-level import is banned by the manifest import policy.
- model axis: `IfcDiff(old, new).diff()` partitions GlobalId sets into `deleted_elements`/`added_elements` (pure set difference) and walks the surviving intersection through `diff_element`/`diff_element_relationships`; `change_register` is the survivor map, the added/deleted sets are disjoint carriers — a consumer reads three result surfaces, never one conflated map. `RELATIONSHIP_TYPE` is the closed diff axis; the `relationships` ctor arg scopes which legs run.
- change axis: each relationship leg writes a bounded `*_changed` marker — `geometry`/`representation` through `diff_representation` over `ifcopenshell.geom` tessellation under the `precision` `math_epsilon`, `attributes` through a direct attribute compare, `type`/`container`/`aggregate`/`classification` through `ifcopenshell.util.element`/`util.classification` accessors, and `property` through a numeric-tolerant `DeepDiff(old_psets, new_psets, math_epsilon=precision, ignore_string_type_changes=True, ignore_numeric_type_changes=True, exclude_regex_paths=[r".*id$"])` over `util.element.get_psets` — so the property leg owns the deep map comparison and the geometry leg owns the BREP-shape comparison, neither a hand-rolled attribute walk.
- selector axis: `filter_elements` (a selector-grammar string) scopes the compared element set through `ifcopenshell.util.selector.filter_elements` before the set difference, so the diff runs over the selected scope only — the consumer threads this through the `IfcSelector` validated gate from `geometry:ifc/selector.md#SELECTOR` rather than a raw query, the same gate the analysis quantity/pset arms share.
- evidence: each diff captures the old/new model identities, the `added`/`deleted` counts, and the per-element change-marker set as a diff receipt; the lifecycle owner folds `change_register` into the bounded `DiffChange` classification keyed by the `*_changed` marker spelling.
- boundary: `ifcdiff` owns IFC model comparison; a hand-rolled `by_type`/attribute-walk diff is the deleted form; model transformation stays `ifcpatch`, 5D cost stays `ifc5d`, IFC parse and tessellation stay `ifcopenshell`, the property-map diff stays `deepdiff`.

[INTEGRATION_STACK]:
- The `geometry:ifc/costing.md#LIFECYCLE` `DIFF` phase composes `IfcDiff(model, ifcopenshell.open(revision)).diff()` then folds `change_register` items through `DiffChange.of_register`: the marker keys map onto the bounded `DiffChange` enum — `geometry_changed` -> `GEOMETRY`, `properties_changed` -> `PSET`, `type_changed`/`container_changed`/`aggregate_changed`/`classification_changed` -> `RELATIONSHIP`, `attributes_changed` -> `ATTRIBUTE`, with the `added_elements`/`deleted_elements` sets supplying the `ADDED`/`DELETED` rows the `change_register` does not carry.
- The `filter_elements` selector arg STACKS with `geometry:ifc/selector.md#SELECTOR`: the validated `SelectorQuery.filter_string` re-serializes to the exact grammar `util.selector.filter_elements` consumes, so a revision diff scopes to a discipline subset (`.map(IfcSelector.parse)` -> `filter_string` -> `IfcDiff(..., filter_elements=validated)`) without a second selection engine.
- The `relationships` axis STACKS with the graduation drift ledger: scoping to `["attributes", "property"]` skips the `ifcopenshell.geom` representation tessellation, so a property-only revision audit avoids the OCC kernel cost while still keying the `drift` evidence fraction the `LifecycleReceipt.evidence` fold reads.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifcdiff`
- Owns: two-model IFC comparison (added/deleted GUID sets plus a per-element `change_register` across the closed `RELATIONSHIP_TYPE` axis) over `deepdiff` and `ifcopenshell.geom`, with a JSON export and a selector pre-scope
- Accept: two `ifcopenshell.file` models, an optional `relationships` subset, and an optional `filter_elements` selector (threaded through the `IfcSelector` gate), feeding the ifc-analysis diff owner
- Reject: a hand-rolled `by_type` attribute-walk diff where `IfcDiff` owns the comparison; a stringified `deepdiff` blob row where the bounded `*_changed` markers classify; a conflated single result map where `change_register`/`added_elements`/`deleted_elements` are three disjoint surfaces; a raw `filter_elements` query where the `IfcSelector` gate validates

[CAPTURE_GAP]:
- floor: companion interpreter cp313; `ifcdiff==0.8.5` rides the `ifcopenshell` companion lane and additionally requires `ifcopenshell.geom` (native tessellation) for the `"geometry"` relationship leg, so reflection resolves where the native `ifcopenshell` core resolves; the `>=3.15` project venv carries no `ifcopenshell` core, and `assay api resolve ifcdiff` returns no source. The surface above is confirmed against the IfcOpenShell `0.8.0`/`0.8.5` `src/ifcdiff/ifcdiff.py` single-module source.
- members: the `IfcDiff(old, new, relationships=None, is_shallow=True, filter_elements=None)` ctor, the `change_register`/`added_elements`/`deleted_elements`/`precision` attributes, the `*_changed` marker-key vocabulary, the `RELATIONSHIP_TYPE` literal, and the `export(path)` JSON shape are source-confirmed; the exact `DeepDiff` result nesting under `properties_changed` is a live-run detail the `DiffChange.of_register` ATTRIBUTE catch-all absorbs without a fence change
