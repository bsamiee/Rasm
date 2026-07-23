# [PY_GEOMETRY_API_IFC5D]

`ifc5d` owns the IFC 5D costing surface over the `ifcopenshell` model: rule-driven quantity take-off through `qto.quantify`/`qto.edit_qtos` writing `IfcElementQuantity` base quantities, and structured cost-schedule export through the `ifc5Dspreadsheet` writer family to CSV, ODS, XLSX, and typst PDF. It feeds the geometry ifc-analysis 5D rail, folding the `qto.rules` base-quantity table over an element set the shared `IfcSelector` gate validates.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifc5d`
- package: `ifc5d` (LGPL-3.0-or-later)
- import: `import ifc5d.qto` / `import ifc5d.ifc5Dspreadsheet`
- owner: `geometry`
- rail: ifc-analysis / 5d-costing
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quantity take-off (`ifc5d.qto`)

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :---------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `qto.RULE_SET`    | enum          | closed rule-set literal, 4 IFC4/IFC4X3 base-quantity variants            |
|  [02]   | `qto.rules`       | table         | `dict[RULE_SET, dict]` base-quantity rule table loaded from bundled JSON |
|  [03]   | `qto.ResultsDict` | type          | `element -> qto-name -> quantity-name -> float` measurement result map   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: quantity take-off and cost-schedule export

Each `ifc5Dspreadsheet` writer constructs `(file, output, cost_schedule=None)` then `.write()` renders the selected `IfcCostSchedule` into the `output` directory; `Ifc5DPdfWriter` adds `options` and `force_schedule_type` ctor args.

| [INDEX] | [SURFACE]                                                | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `qto.quantify(ifc_file, elements, rules) -> ResultsDict` | static  | compute base quantities per element          |
|  [02]   | `qto.edit_qtos(ifc_file, results) -> None`               | static  | write `IfcElementQuantity` sets into model   |
|  [03]   | `ifc5Dspreadsheet.Ifc5DCsvWriter`                        | ctor    | export the cost schedule to CSV              |
|  [04]   | `ifc5Dspreadsheet.Ifc5DOdsWriter`                        | ctor    | export the cost schedule to an ODS workbook  |
|  [05]   | `ifc5Dspreadsheet.Ifc5DXlsxWriter`                       | ctor    | export the cost schedule to an XLSX workbook |
|  [06]   | `ifc5Dspreadsheet.Ifc5DPdfWriter`                        | ctor    | export the cost schedule to a typst PDF      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `qto.quantify(ifc_file, elements, rules)` folds the `qto.rules[RULE_SET]` base-quantity table over an element set into a `ResultsDict`, and `qto.edit_qtos` writes it back as `IfcElementQuantity` base quantities; geometric measurement stays inside the `qto` kernel, never a local `get_psets(qtos_only=True)` fold over a single quantity key.
- Cost export binds one `ifc5Dspreadsheet` writer subclass per output format, `.write()` rendering the `IfcCostSchedule` to the `output` directory; a new rule set is one `qto.rules` `RULE_SET` key and a new format one writer subclass.

[STACKING]:
- `ifcopenshell`(`.api/ifcopenshell.md`): `qto.quantify` consumes an `ifcopenshell.file` and its `entity_instance` element set; the per-`IfcCostItem` resource rollup preceding export is `ifcopenshell.api.cost.calculate_cost_item_resource_value(file, cost_item)`, owned there at `[03]` usecase [15], its result read back through `IfcCostItem.CostValues` `IfcCostValue.AppliedValue`.
- `ifc/costing.md`: gates the `quantify` selector through the shared `IfcSelector` grammar, folds the `ResultsDict` into typed lifecycle rows, and binds a `CostReport`-selected `ifc5Dspreadsheet` writer subclass onto the receipt subject, deferring the columnar `.write()` to the `python:data/spatial` boundary.

[LOCAL_ADMISSION]:
- Geometry's ifc-analysis 5D owner composes `ifc5d.qto` and `ifc5d.ifc5Dspreadsheet` directly; quantity measurement and cost-schedule rendering never re-derive against a local pset fold.

[RAIL_LAW]:
- Package: `ifc5d`
- Owns: rule-driven IFC quantity take-off (`qto.quantify`/`qto.edit_qtos`) and structured cost-schedule export (`ifc5Dspreadsheet` writer family)
- Accept: an `ifcopenshell.file` with an element set and a `RULE_SET` key, or an `IfcCostSchedule` with an output directory
- Reject: a hand-rolled quantity-key fold over `get_psets(qtos_only=True)`; a per-IFC-class measurement family where `qto.rules` owns the rule; a bespoke cost-rollup loop where `ifcopenshell.api.cost.calculate_cost_item_resource_value` owns the per-item rollup
