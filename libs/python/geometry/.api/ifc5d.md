# [PY_GEOMETRY_API_IFC5D]

`ifc5d` supplies the IFC 5D costing surface for the geometry ifc-analysis rail: quantity take-off folding `IfcElementQuantity` and base-quantity psets into cost-line rows, cost-schedule (`IfcCostSchedule`/`IfcCostItem`) authoring and value rollup, and structured CSV/ODS/typst export. It rides the `ifcopenshell` companion lane (same `0.8.5` distribution band, pure-Python over the `ifcopenshell` core), so the analysis owner composes `ifc5d.qto`/`ifc5d.cost` directly rather than re-deriving quantity-key folds over `util.element.get_psets`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifc5d`
- package: `ifc5d`
- import: `import ifc5d`
- owner: `geometry`
- rail: ifc-analysis / 5d-costing
- installed: `0.8.5`, the IfcOpenShell-ecosystem companion-lane band (depends `ifcopenshell`; `typst` only under the `advanced` extra)
- entry points: none (library only)
- capability: rule-driven quantity take-off across the IFC element set, `IfcCostSchedule`/`IfcCostItem` cost-tree authoring and value rollup, cost-value/quantity binding to elements, and structured cost-report export (CSV/ODS, typst PDF under the `advanced` extra)

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quantity take-off (`ifc5d.qto`)
- rail: 5d-costing

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]    | [CAPABILITY]                                                   |
| :-----: | :---------------- | :---------------- | :------------------------------------------------------------- |
|   [1]   | `qto.SCHEMA`      | quantity rule set | the base-quantity rule table keyed by IFC class and quantity   |
|   [2]   | `qto.calculate_*` | quantity kernel   | per-quantity geometric measurement (length/area/volume/weight) |
|   [3]   | `qto.edit_qtos`   | quantity writer   | derive and write `IfcElementQuantity` base quantities          |

[PUBLIC_TYPE_SCOPE]: cost schedule (`ifc5d.cost`)
- rail: 5d-costing

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]  | [CAPABILITY]                                          |
| :-----: | :---------------------- | :-------------- | :---------------------------------------------------- |
|   [1]   | `cost.CostSchedule`     | schedule root   | wraps an `IfcCostSchedule`, drives the cost-item tree |
|   [2]   | `cost.calculate_cost`   | rollup kernel   | total-cost rollup over the cost-item hierarchy        |
|   [3]   | `cost.Csv` / `cost.Ods` | report exporter | structured cost-report serialization                  |
|   [4]   | `cost.Reporter`         | report base     | shared `report`/`write` interface over the schedule   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: quantity take-off and cost rollup
- rail: 5d-costing

Quantity rows consume an `ifcopenshell.file` plus an element selection; cost rows consume an `IfcCostSchedule` entity.

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]             | [CAPABILITY]                                |
| :-----: | :-------------------------------- | :----------------------- | :------------------------------------------ |
|   [1]   | `ifc5d.qto.edit_qtos`             | model plus element set   | compute and write base quantities           |
|   [2]   | `ifc5d.qto.SCHEMA`                | none                     | the quantity rule table per IFC class       |
|   [3]   | `ifc5d.cost.calculate_cost`       | model plus cost schedule | roll up cost-item totals over the tree      |
|   [4]   | `ifc5d.cost.Csv(...).report(...)` | model plus schedule      | export the cost schedule to CSV rows        |
|   [5]   | `ifc5d.cost.Ods(...).report(...)` | model plus schedule      | export the cost schedule to an ODS workbook |

## [4]-[IMPLEMENTATION_LAW]

[FIVE_D_TOPOLOGY]:
- import: `import ifc5d.qto` / `import ifc5d.cost` at boundary scope only; module-level import is banned by the manifest import policy.
- quantity axis: `qto.edit_qtos` derives base quantities from the geometric measurement kernel keyed by the `qto.SCHEMA` rule table, writing `IfcElementQuantity` back into the model — the rule-driven take-off the hand-rolled `get_psets(qtos_only=True)` fold over a single `NetFloorArea` key replaces. The schema is the closed quantity vocabulary, never a per-class literal.
- cost axis: `cost.calculate_cost` rolls totals over the `IfcCostItem` tree under one `IfcCostSchedule`; `cost.Csv`/`cost.Ods` are the report rows over the schedule, the structured 5D output the analysis owner graduates.
- evidence: each take-off captures the element count, the quantity rows derived, and the quantity keys; each cost rollup captures the schedule id and the rolled total as a 5d receipt.
- boundary: `ifc5d` owns 5D quantity take-off and cost-schedule rollup over the `ifcopenshell` model; geometric measurement stays inside the `ifc5d.qto` kernel, never re-derived against a local pset fold; IFC parse and tessellation stay `ifcopenshell`; clash stays `ifcclash`; IDS stays `ifctester`.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifc5d`
- Owns: rule-driven IFC quantity take-off, `IfcCostSchedule`/`IfcCostItem` authoring and rollup, and structured cost-report export
- Accept: an `ifcopenshell.file` plus an element selection or a cost schedule entity, feeding the ifc-analysis 5D owner
- Reject: a hand-rolled quantity-key fold over `get_psets(qtos_only=True)`; a per-IFC-class measurement function family where `qto.SCHEMA` carries the rule; a bespoke cost-rollup loop where `cost.calculate_cost` owns the tree fold

[CAPTURE_GAP]:
- floor: companion interpreter cp313; `ifc5d==0.8.5` rides the `ifcopenshell` companion lane (pure-Python over the native IFC core), so reflection resolves where `ifcopenshell` resolves, and the `>=3.15` project venv carries no `ifcopenshell` core
- members: the `qto.edit_qtos`/`qto.SCHEMA` quantity surface and the `cost.calculate_cost`/`cost.Csv`/`cost.Ods` report surface confirm by introspection against the installed companion distribution before any fence transcribes them; the `qto.SCHEMA` rule-table key shape and the `cost.Reporter.report` argument arity are the live-run-confirmed members
