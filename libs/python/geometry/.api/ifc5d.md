# [PY_GEOMETRY_API_IFC5D]

`ifc5d` supplies the IFC 5D costing surface for the geometry ifc-analysis rail: rule-driven quantity take-off (`qto.quantify`/`edit_qtos`) writing `IfcElementQuantity` base quantities, and structured cost-schedule CSV/ODS/XLSX export (`ifc5Dspreadsheet`). It rides the `ifcopenshell` companion lane (same `0.8.5` distribution band, pure-Python over the `ifcopenshell` core), so the analysis owner composes `ifc5d.qto`/`ifc5d.ifc5Dspreadsheet` directly rather than re-deriving quantity-key folds over `util.element.get_psets`; the per-`IfcCostItem` cost rollup is `ifcopenshell.api.cost.calculate_cost_item_resource_value`, not an `ifc5d` member.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifc5d`
- package: `ifc5d`
- import: `import ifc5d`
- owner: `geometry`
- rail: ifc-analysis / 5d-costing
- installed: `0.8.5`, the IfcOpenShell-ecosystem companion-lane band (depends `ifcopenshell`; `typst` only under the `advanced` extra)
- entry points: none (library only)
- capability: rule-driven quantity take-off across the IFC element set (`qto.quantify`/`edit_qtos` over the `qto.rules` `RULE_SET` table) and structured cost-schedule export (`ifc5Dspreadsheet` CSV/ODS/XLSX, typst PDF under the `advanced` extra); the cost-item resource rollup itself is `ifcopenshell.api.cost`, not an `ifc5d` member

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quantity take-off (`ifc5d.qto`)
- rail: 5d-costing

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE]    | [CAPABILITY]                                                                                                                    |
| :-----: | :-------------- | :---------------- | :------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `qto.rules`     | quantity rule set | the `dict[RULE_SET, ...]` base-quantity rule table loaded from bundled JSON                                                     |
|  [02]   | `qto.RULE_SET`  | rule-set literal  | `Literal["IFC4QtoBaseQuantities", "IFC4QtoBaseQuantitiesBlender", "IFC4X3QtoBaseQuantities", "IFC4X3QtoBaseQuantitiesBlender"]` |
|  [03]   | `qto.quantify`  | quantity kernel   | `quantify(file, elements, rules) -> ResultsDict` rule-driven measurement                                                        |
|  [04]   | `qto.edit_qtos` | quantity writer   | `edit_qtos(file, results) -> None` writes `IfcElementQuantity` base quantities                                                  |

[PUBLIC_TYPE_SCOPE]: cost export (`ifc5d.ifc5Dspreadsheet`)
- rail: 5d-costing

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]  | [CAPABILITY]                                                                |
| :-----: | :--------------------------------- | :-------------- | :-------------------------------------------------------------------------- |
|  [01]   | `ifc5Dspreadsheet.Ifc5DCsvWriter`  | report exporter | `(file, output, cost_schedule=None)` then `.write()` to a CSV in `output`   |
|  [02]   | `ifc5Dspreadsheet.Ifc5DOdsWriter`  | report exporter | `(file, output, cost_schedule=None)` then `.write()` to an ODS in `output`  |
|  [03]   | `ifc5Dspreadsheet.Ifc5DXlsxWriter` | report exporter | `(file, output, cost_schedule=None)` then `.write()` to an XLSX in `output` |
|  [04]   | `ifc5Dspreadsheet.Ifc5Dwriter`     | exporter base   | the shared `.write()` writer contract the format writers subclass           |

Cost rollup is NOT an `ifc5d` member — `ifc5d` carries no `cost` module. The per-`IfcCostItem` resource rollup is `ifcopenshell.api.cost.calculate_cost_item_resource_value(file, cost_item)` (sums the construction-resource base costs into the item's value); the rolled value reads back through `IfcCostItem.CostValues` `IfcCostValue.AppliedValue`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: quantity take-off and cost rollup
- rail: 5d-costing

Quantity rows consume an `ifcopenshell.file` plus an element set and a rule set; export rows consume a model, an output directory, and an optional `IfcCostSchedule`.

| [INDEX] | [SURFACE]                                                                    | [CALL_SHAPE]                      | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------------------- | :-------------------------------- | :------------------------------------------- |
|  [01]   | `ifc5d.qto.quantify(file, elements, rules) -> ResultsDict`                   | model, element set, rule-set dict | compute base quantities per element          |
|  [02]   | `ifc5d.qto.edit_qtos(file, results) -> None`                                 | model plus a `ResultsDict`        | write the computed quantities into the model |
|  [03]   | `ifc5d.qto.rules[RULE_SET]`                                                  | a `RULE_SET` literal key          | the bundled base-quantity rule table         |
|  [04]   | `ifcopenshell.api.cost.calculate_cost_item_resource_value(file, cost_item)`  | model plus one `IfcCostItem`      | roll resource costs into the item value      |
|  [05]   | `ifc5d.ifc5Dspreadsheet.Ifc5DCsvWriter(file, output, cost_schedule).write()` | model, dir, schedule              | export the cost schedule to a CSV file       |
|  [06]   | `ifc5d.ifc5Dspreadsheet.Ifc5DOdsWriter(file, output, cost_schedule).write()` | model, dir, schedule              | export the cost schedule to an ODS workbook  |

## [04]-[IMPLEMENTATION_LAW]

[FIVE_D_TOPOLOGY]:
- import: `import ifc5d.qto` / `import ifc5d.ifc5Dspreadsheet` / `import ifcopenshell.api.cost` at boundary scope only; module-level import is banned by the manifest import policy.
- quantity axis: `qto.quantify(file, elements, rules)` computes the base-quantity `ResultsDict` from the geometric measurement kernel keyed by the `qto.rules[RULE_SET]` rule table, and `qto.edit_qtos(file, results)` writes `IfcElementQuantity` back into the model — the rule-driven take-off the hand-rolled `get_psets(qtos_only=True)` fold over a single `NetFloorArea` key replaces. The rule set is the closed quantity vocabulary (`RULE_SET` literal), never a per-class literal.
- cost axis: the per-item resource rollup is `ifcopenshell.api.cost.calculate_cost_item_resource_value(file, cost_item)` (NOT an `ifc5d.cost` member — `ifc5d` has no `cost` module); the structured export is `ifc5d.ifc5Dspreadsheet.Ifc5DCsvWriter`/`Ifc5DOdsWriter`, each `(file, output, cost_schedule).write()` writing the spreadsheet to `output`, the structured 5D output the analysis owner graduates.
- evidence: each take-off captures the element count, the quantity rows derived, and the quantity keys; each cost rollup captures the schedule id and the per-item `IfcCostValue.AppliedValue` rows as a 5d receipt.
- boundary: `ifc5d` owns 5D quantity take-off and cost-schedule export over the `ifcopenshell` model; geometric measurement stays inside the `ifc5d.qto` kernel, never re-derived against a local pset fold; the cost-item resource rollup stays `ifcopenshell.api.cost`; IFC parse and tessellation stay `ifcopenshell`; clash stays `ifcclash`; IDS stays `ifctester`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifc5d`
- Owns: rule-driven IFC quantity take-off (`qto.quantify`/`edit_qtos`) and structured cost-schedule spreadsheet export (`ifc5Dspreadsheet`)
- Accept: an `ifcopenshell.file` plus an element set and a rule set, or a cost schedule plus an output directory, feeding the ifc-analysis 5D owner
- Reject: a hand-rolled quantity-key fold over `get_psets(qtos_only=True)`; a per-IFC-class measurement function family where `qto.rules` carries the rule; a bespoke cost-rollup loop where `ifcopenshell.api.cost.calculate_cost_item_resource_value` owns the per-item rollup

[CAPTURE_GAP]:
- floor: companion interpreter cp313; `ifc5d==0.8.5` rides the `ifcopenshell` companion lane (pure-Python over the native IFC core), so reflection resolves where `ifcopenshell` resolves, and the `>=3.15` project venv carries no `ifcopenshell` core
- members: the `qto.quantify`/`qto.edit_qtos`/`qto.rules` quantity surface and the `ifc5Dspreadsheet.Ifc5DCsvWriter`/`Ifc5DOdsWriter` export surface confirm by introspection against the installed companion distribution before any fence transcribes them; the `qto.rules` `RULE_SET` literal set and the `IfcCostItem.CostValues` `AppliedValue` measure shape are the live-run-confirmed members
