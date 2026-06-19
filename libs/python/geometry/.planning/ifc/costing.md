# [PY_GEOMETRY_IFC_COSTING]

The 5D/4D model-lifecycle owner — the construction-economics and model-management verbs the analysis hop alone drops. `IfcLifecycle` runs rule-driven quantity take-off, cost-schedule rollup with structured report export, construction scheduling, recipe-driven model transformation, and two-model revision comparison over `ifc5d`, `ifc4d`, `ifcpatch`, and `ifcdiff` — the four `0.8.5` IfcOpenShell-ecosystem siblings over the `ifcopenshell` core — emitting a `LifecycleReceipt` that graduates through the compute `HandoffAxis` geometry case alongside the analysis verbs in `geometry:ifc/analysis.md#ANALYSIS`. The `QUANTITY` phase is the rule-driven take-off that supersedes the hand-rolled single-`NetFloorArea` fold the sibling owner shed: `ifc5d.qto.quantify` computes the full base-quantity schema (length/area/volume/weight per IFC class) keyed by the `ifc5d.qto.rules` `RULE_SET` table and `ifc5d.qto.edit_qtos` writes the computed `ResultsDict` back into the model as `IfcElementQuantity`, so the quantity arm answers the whole schedule rather than one key. The C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the lifecycle dimension the managed projection does not produce, taking the validated selector from `geometry:ifc/selector.md#SELECTOR` and reaching the C# owner system through the one graduation rail.

## [01]-[INDEX]

- [01]-[LIFECYCLE]: the quantity, cost, schedule, patch, and diff lifecycle phases under one `LifecyclePhase`-discriminated owner folding the four IfcOpenShell ecosystem siblings.

## [02]-[LIFECYCLE]

- Owner: `IfcLifecycle` — the `@staticmethod` boundary capsule mirroring `IfcAnalysis`, dispatching the lifecycle phases over the `ifc5d`/`ifc4d`/`ifcpatch`/`ifcdiff` ecosystem; `LifecyclePhase` the closed `StrEnum` selecting the phase; `ScheduleFormat` the closed `StrEnum` selecting the `ifc4d` named parser so the schedule arm is one row over a closed parser vocabulary rather than a parse-per-format function family; `CostReport` the closed `StrEnum` selecting the `ifc5Dspreadsheet` writer so the cost arm's export leg is one row over `Ifc5DCsvWriter`/`Ifc5DOdsWriter`; `LifecycleReceipt` the typed receipt carrying the phase, the subject element set, and the structured result rows.
- Cases: `LifecyclePhase` rows `QUANTITY` (rule-driven base-quantity take-off over `ifc5d.qto.quantify`/`edit_qtos` keyed by the `qto.rules` rule-set table, writing `IfcElementQuantity` and reading the derived quantity keys) · `COST` (`ifcopenshell.api.cost.calculate_cost_item_resource_value` rollup over each `IfcCostItem` plus the `CostReport`-selected `ifc5d.ifc5Dspreadsheet.Ifc5DCsvWriter`/`Ifc5DOdsWriter` structured export leg) · `SCHEDULE` (`ifc4d` `<Format>2Ifc` named-parser conversion populating `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence`) · `PATCH` (`ifcpatch.execute` named-recipe transformation over the `recipes` namespace plus the `ifcpatch.write` polymorphic sink) · `DIFF` (`ifcdiff.IfcDiff(old, new).diff()` revision comparison over `deepdiff`, reading `change_register`) — matched by `match`/`assert_never`, each dispatching to the ecosystem sibling that owns it. A new phase is one `LifecyclePhase` row plus one fold arm and breaks every dispatch site at type-check time under `ty`/`py_analyzer`.
- Entry: `IfcLifecycle.run` takes an `ifcopenshell.file`, a `LifecyclePhase`, and a `spec` whose meaning is fixed by the phase — a validated selector query for `QUANTITY`, a cost-schedule GlobalId plus report token for `COST`, a `<format>:<path>` schedule source for `SCHEDULE`, a `<recipe>:<json-args>` patch directive for `PATCH`, a revision-model path for `DIFF` — and returns a `RuntimeRail[LifecycleReceipt]` through one `boundary(f"lifecycle.{phase}", ...)` admission, so a provider exception converts to a `BoundaryFault` exactly once at the seam. Each arm derives its own `subjects` from the phase's true subject set: quantity element GlobalIds for `QUANTITY`, the schedule id for `COST`, task GlobalIds for `SCHEDULE`, the patched-output identity for `PATCH`, changed-element GlobalIds for `DIFF` — so the subject field never carries a meaningless run.
- Auto: the `QUANTITY` arm runs `ifc5d.qto.quantify(model, elements, ifc5d.qto.rules[rule_set])` — the rule-driven measurement kernel computing the full base-quantity schedule per element keyed by the `qto.rules` `RULE_SET` table (the IFC4 vs IFC4X3 base-quantity rule set the `spec`'s `#<rule-set>` suffix selects, defaulting to `IFC4QtoBaseQuantities`) — then `ifc5d.qto.edit_qtos(model, results)` writes the computed `ResultsDict` back as `IfcElementQuantity` base quantities, replacing the deleted `get_psets(qtos_only=True)`/`NetFloorArea` single-key fold, and folds the `ResultsDict` (`element -> qto-name -> quantity-name -> value`) directly into the rows; the `COST` arm runs `ifcopenshell.api.cost.calculate_cost_item_resource_value(model, cost_item=item)` over each `IfcCostItem` to roll its resource values, then routes the `CostReport`-selected `ifc5d.ifc5Dspreadsheet` writer (`Ifc5DCsvWriter` or `Ifc5DOdsWriter`, constructed `(model, output_dir, cost_schedule=schedule)`) whose `.write()` emits the structured spreadsheet export, and reads each `IfcCostItem.CostValues` `IfcCostValue.AppliedValue` back as the rows; the `SCHEDULE` arm constructs the `ScheduleFormat`-selected `<Format>2Ifc` parser, sets its `.file`/`.xml`/`.work_plan` slots, and runs `.execute()` to populate the `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence` task tree, reading the task GlobalIds back; the `PATCH` arm runs `ifcpatch.execute({"input", "file", "recipe", "arguments"})` dispatching the named recipe over the `recipes` namespace and the `ifcpatch.write` polymorphic sink serializing the recipe-determined output; the `DIFF` arm runs `ifcdiff.IfcDiff(model, revision).diff()` populating `change_register` over `deepdiff` and folds the GlobalId-keyed added/deleted/changed map into rows. No phase carries an `if/else` value ladder and no phase mints a sibling per-phase class — one fold arm per row, the package that owns the phase bound directly.
- Receipt: each run contributes an emitted-phase `Receipt.of` row through `ReceiptContributor.contribute` carrying the phase tag and the phase-specific facts (element count and quantity keys for `QUANTITY`, schedule id and rolled total for `COST`, source format and task count for `SCHEDULE`, recipe name and patched-model identity for `PATCH`, added/deleted/changed counts for `DIFF`), and produces a geometry `GraduationReceipt` subject so take-off rows, cost reports, schedules, patched models, and revision diffs reach the C# owner system through the one graduation rail as structured lifecycle output the toolchain consumes directly.
- Packages: `ifc5d` (`qto.rules` rule-set table/`qto.quantify`/`qto.edit_qtos`/`ifc5Dspreadsheet.Ifc5DCsvWriter`/`Ifc5DOdsWriter`), `ifcopenshell` (`api.cost.calculate_cost_item_resource_value` cost rollup, `util.selector.filter_elements`, `file`/`by_guid`/`by_type`/`open` over the in-process model only), `ifc4d` (`MSProject2Ifc`/`P62Ifc`/`Asta2Ifc` named parsers, `.file`/`.xml`/`.work_plan` slots, `.execute()`), `ifcpatch` (`execute`/`write` over the `recipes` namespace), `ifcdiff` (`IfcDiff`/`diff`/`change_register`/`export`), runtime (`RuntimeRail`/`boundary`/`ReceiptContributor`).
- Growth: a new quantity rule set is one `qto.rules` `RULE_SET` key authored upstream, never a local measurement fold; a new cost report format is one `CostReport` row binding its `ifc5Dspreadsheet` writer subclass; a new schedule format is one `ScheduleFormat` row binding its `<Format>2Ifc` parser; a new model transformation is one `recipe` name in the `ifcpatch.execute` directive, never a `file.add`/`remove` loop; zero new surface, no parallel per-phase class family.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy (projected in-process); no durable store; no Rhino/GH mutation; the `ifcopenshell.file` model is the only foreign object held, and the four ecosystem siblings import function-local under `# noqa: PLC0415` at boundary scope, never module-top, per the manifest import policy. A hand-rolled `NetFloorArea` quantity fold, a per-IFC-class measurement function family where the `qto.rules` table carries the rule, a bespoke cost-rollup loop where `ifcopenshell.api.cost.calculate_cost_item_resource_value` owns the per-item resource rollup and `ifc5Dspreadsheet` owns the structured export, a hand-written P6/MS Project XML parser where the `<Format>2Ifc` parser owns the conversion, an ad-hoc `file.create_entity`/`add`/`remove` mutation loop where a recipe owns the transformation, and a `by_type`/attribute-walk diff where `IfcDiff`/`deepdiff` own the comparison are the deleted forms — the rule table, the named parser family, the recipe vocabulary, and the structured diff compose the provider tools end-to-end.

```python signature
from enum import StrEnum
from typing import assert_never

import ifcopenshell
from msgspec import Struct
from msgspec.json import decode

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import ReceiptContributor


class LifecyclePhase(StrEnum):
    QUANTITY = "quantity"
    COST = "cost"
    SCHEDULE = "schedule"
    PATCH = "patch"
    DIFF = "diff"


class ScheduleFormat(StrEnum):
    MSPROJECT = "msproject"
    P6 = "p6"
    ASTA = "asta"


class CostReport(StrEnum):
    CSV = "csv"
    ODS = "ods"


class LifecycleReceipt(Struct, frozen=True):
    phase: LifecyclePhase
    subjects: tuple[str, ...]
    rows: tuple[dict[str, str], ...]


class IfcLifecycle:
    @staticmethod
    def run(model: "ifcopenshell.file", phase: LifecyclePhase, spec: str) -> "RuntimeRail[LifecycleReceipt]":
        return boundary(f"lifecycle.{phase}", lambda: IfcLifecycle._dispatch(model, phase, spec))

    @staticmethod
    def _dispatch(model: "ifcopenshell.file", phase: LifecyclePhase, spec: str) -> LifecycleReceipt:
        match phase:
            case LifecyclePhase.QUANTITY:
                import ifc5d.qto  # noqa: PLC0415
                import ifcopenshell.util.selector  # noqa: PLC0415

                selector, _, rule_set = spec.partition("#")
                elements = set(ifcopenshell.util.selector.filter_elements(model, selector))
                results = ifc5d.qto.quantify(model, elements, ifc5d.qto.rules[rule_set or "IFC4QtoBaseQuantities"])
                ifc5d.qto.edit_qtos(model, results)
                rows = tuple(
                    {"element": element.GlobalId, "quantity": f"{qto}.{name}", "value": str(value)}
                    for element, qtos in results.items()
                    for qto, quantities in qtos.items()
                    for name, value in quantities.items()
                )
                return LifecycleReceipt(phase, tuple(e.GlobalId for e in results), rows)
            case LifecyclePhase.COST:
                import tempfile  # noqa: PLC0415

                import ifc5d.ifc5Dspreadsheet  # noqa: PLC0415
                import ifcopenshell.api.cost  # noqa: PLC0415

                schedule_id, _, report_token = spec.partition(":")
                schedule = model.by_guid(schedule_id)
                for item in model.by_type("IfcCostItem"):
                    ifcopenshell.api.cost.calculate_cost_item_resource_value(model, cost_item=item)
                writer_cls = {CostReport.CSV: ifc5d.ifc5Dspreadsheet.Ifc5DCsvWriter, CostReport.ODS: ifc5d.ifc5Dspreadsheet.Ifc5DOdsWriter}[
                    CostReport(report_token or "csv")
                ]
                with tempfile.TemporaryDirectory() as out:
                    writer_cls(model, out, cost_schedule=schedule).write()
                rows = tuple(
                    {"item": item.GlobalId, "name": item.Name or "", "value": str(getattr(value, "AppliedValue", "") or "")}
                    for item in model.by_type("IfcCostItem")
                    for value in (item.CostValues or ())
                )
                return LifecycleReceipt(phase, (schedule.GlobalId,), rows)
            case LifecyclePhase.SCHEDULE:
                import ifc4d.asta2ifc  # noqa: PLC0415
                import ifc4d.msproject2ifc  # noqa: PLC0415
                import ifc4d.p62ifc  # noqa: PLC0415

                fmt, _, source = spec.partition(":")
                parser = {
                    ScheduleFormat.MSPROJECT: ifc4d.msproject2ifc.MSProject2Ifc,
                    ScheduleFormat.P6: ifc4d.p62ifc.P62Ifc,
                    ScheduleFormat.ASTA: ifc4d.asta2ifc.Asta2Ifc,
                }[ScheduleFormat(fmt)]()
                parser.file = model
                parser.xml = source
                parser.work_plan = model.by_type("IfcWorkPlan")[0] if model.by_type("IfcWorkPlan") else None
                parser.execute()
                tasks = model.by_type("IfcTask")
                rows = tuple({"task": t.GlobalId, "name": t.Name or ""} for t in tasks)
                return LifecycleReceipt(phase, tuple(t.GlobalId for t in tasks), rows)
            case LifecyclePhase.PATCH:
                import ifcpatch  # noqa: PLC0415

                recipe, _, arguments = spec.partition(":")
                output = ifcpatch.execute({
                    "input": "",
                    "file": model,
                    "recipe": recipe,
                    "arguments": decode(arguments.encode(), type=list) if arguments else [],
                })
                ifcpatch.write(output, f"lifecycle.{recipe}")
                patched = output if isinstance(output, ifcopenshell.file) else model
                rows = ({"recipe": recipe, "product": type(output).__name__},)
                return LifecycleReceipt(phase, (patched.schema,), rows)
            case LifecyclePhase.DIFF:
                import ifcdiff  # noqa: PLC0415

                revision = ifcopenshell.open(spec)
                differ = ifcdiff.IfcDiff(model, revision)
                differ.diff()
                rows = tuple({"element": guid, "change": str(change)} for guid, change in differ.change_register.items())
                return LifecycleReceipt(phase, tuple(r["element"] for r in rows), rows)
            case unreachable:
                assert_never(unreachable)
```

## [03]-[RESEARCH]

- [QTO_RULE_SET]: the branch `ifc5d` catalogue confirms `qto.rules` (the `dict[RULE_SET, ...]` base-quantity rule table loaded from the bundled `IFC4QtoBaseQuantities`/`IFC4X3QtoBaseQuantities` JSON, keyed by the `RULE_SET` literal, `ifc5d.md#23`/`47`), `qto.quantify(ifc_file, elements, rules) -> ResultsDict` (the rule-driven measurement kernel, `#24`/`46`), and `qto.edit_qtos(ifc_file, results) -> None` (write the `ResultsDict` back as `IfcElementQuantity` base quantities, `#25`/`48`). The `quantify` `elements: set[ifcopenshell.entity_instance]` set form (the fence wraps `filter_elements` in `set(...)`) and the `ResultsDict = dict[element, dict[qto-name, dict[quantity-name, float]]]` nesting the row fold reads confirm by introspection against the installed companion distribution. The `IFC4` versus `IFC4X3` rule-set choice the `spec`'s `#<rule-set>` suffix selects defaults to `IFC4QtoBaseQuantities`; the live-run residual is the exact `RULE_SET` literal set bundled in the `0.8.5` distribution (`ifc5d.md#71`).
- [COST_ROLLUP_EXPORT]: the cost rollup is `ifcopenshell.api.cost.calculate_cost_item_resource_value(file, cost_item)` per `IfcCostItem` (summing the construction-resource base costs into the item's resource value, an `ifcopenshell.api.cost` function, NOT an `ifc5d.cost` member — `ifc5d` carries no `cost` module), and the structured export is `ifc5d.ifc5Dspreadsheet.Ifc5DCsvWriter`/`Ifc5DOdsWriter`, each constructed `(file, output_dir, cost_schedule=None)` and run through `.write()` (writing the spreadsheet to `output_dir`, no return rows), confirmed against the installed companion distribution (`ifc5d.md#49`/`50`). The `IfcCostItem.CostValues` `IfcCostValue.AppliedValue` measure shape the row fold reads back (the value-select union `getattr(value, "AppliedValue", "")` folds) is the live-run-confirmed member before the row construction binds.
- [SCHEDULE_SLOT_POPULATION]: the branch `ifc4d` catalogue confirms the `MSProject2Ifc`/`P62Ifc`/`Asta2Ifc` named-parser family (`ifc4d.md#23`-`25`), the `.execute()` entry (`#37`-`39`), the `.file`/`.xml`/`.work_plan` slots (`#45`), and the populated `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence` set — the schedule format is the named parser class, a closed parser set, never a parse-per-format function family, so the `SCHEDULE` arm is fully fenced. The narrow residual is the live-run `.file`/`.xml`/`.work_plan` slot-population semantics: whether `.xml` accepts the source path string or a parsed document, and whether `.work_plan` requires a pre-existing `IfcWorkPlan` or the parser mints one — the companion-interpreter `<Format>2Ifc.execute()` run confirms the slot contract, a runtime-action band step, not a fence blocker.
- [PATCH_OUTPUT_DISPATCH]: the branch `ifcpatch` catalogue confirms `ifcpatch.execute({"input", "file", "recipe", "arguments"})` (named-recipe dispatch over the `Patcher` base, `ifcpatch.md#39`), the `recipes` namespace recipe names (`#24`), and `ifcpatch.write(output, filepath)` (the polymorphic sink, `#40`); the `ifcpatch.write` output-type dispatch (a patched `ifcopenshell.file` versus a recipe's non-IFC string/path product) is the recipe-determined return the `PATCH` arm's `isinstance(output, ifcopenshell.file)` discrimination reads, confirmed by introspection against the installed companion distribution (`#62`).
- [DIFF_CHANGE_REGISTER]: the branch `ifcdiff` catalogue confirms `IfcDiff(old_file, new_file)` (the two-model constructor, `ifcdiff.md#33`), `IfcDiff.diff()` (`#34`), `IfcDiff.change_register` (the GlobalId-keyed added/deleted/changed map, `#36`), and `IfcDiff.export(path)` (`#35`); the `change_register` value shape (the per-element change classification — geometry/attribute/pset/relationship — `deepdiff` populates) is the live-run-confirmed member the `DIFF` arm's row fold reads (`#56`).

## [04]-[UPSTREAM]

- [COMPANION_LANE]: `ifc5d`, `ifc4d`, `ifcpatch`, and `ifcdiff` are the `0.8.5` IfcOpenShell-ecosystem siblings that ride the `ifcopenshell` companion lane — each depends `ifcopenshell` (`ifcpatch` additionally `toposort`/`numpy`, `ifcdiff` additionally `deepdiff`, `ifc4d` additionally `typing-extensions`) and resolves where `ifcopenshell` resolves, so the four siblings are inert in the cp315 project venv that carries no `ifcopenshell` core and settle only on the `<'3.15'` companion interpreter (cp313). The fence imports them function-local at boundary scope under `# noqa: PLC0415` per the manifest import policy, never module-top.
- [IFCOPENSHELL_PRESENCE]: `ifcopenshell` is itself absent from the repo-root manifest despite the `tessellation` daemon and the `ifc-analysis` owners consuming it; the four `0.8.5` siblings are a moot admission until `ifcopenshell` is manifest-present, so the manifest reconciliation admits `ifcopenshell` (companion lane) plus the four `ifc5d`/`ifc4d`/`ifcpatch`/`ifcdiff` `>=0.8.5` rows (`python_version<'3.15'`, depends `ifcopenshell`) as one batch — the `.api` catalogues exist, only the manifest rows remain.
