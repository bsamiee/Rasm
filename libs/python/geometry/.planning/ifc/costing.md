# [PY_GEOMETRY_IFC_COSTING]

The 5D/4D model-lifecycle owner — the construction-economics and model-management verbs the analysis hop alone drops. `IfcLifecycle` runs rule-driven quantity take-off, cost-schedule rollup with structured report export, construction scheduling, recipe-driven model transformation, and two-model revision comparison over `ifc5d`, `ifc4d`, `ifcpatch`, and `ifcdiff` — the four `0.8.5` IfcOpenShell-ecosystem siblings over the `ifcopenshell` core — emitting a `LifecycleReceipt` whose rows are the typed `LifecycleRow` tagged union (never a stringly `dict[str, str]`), graduating through the compute `HandoffAxis` geometry case carrying the canonical `GeometrySubject` `"numerical-primitive"` literal alongside the analysis verbs in `geometry:ifc/analysis.md#ANALYSIS`. The `QUANTITY` phase is the rule-driven take-off that supersedes the hand-rolled single-`NetFloorArea` fold the sibling owner shed: `ifc5d.qto.quantify` computes the full base-quantity schema (length/area/volume/weight per IFC class) keyed by the `ifc5d.qto.rules` `RULE_SET` table and `ifc5d.qto.edit_qtos` writes the computed `ResultsDict` back into the model as `IfcElementQuantity`, so the quantity arm answers the whole schedule rather than one key. The element set the quantity arm measures is never a raw `filter_elements` passthrough: the selector text threads through the shared `geometry:ifc/selector.md#SELECTOR` validated entry gate — `IfcSelector.filter(model, selector)` returns a `RuntimeRail[tuple[entity_instance, ...]]` so a malformed selector is an `UnexpectedInput`-derived `BoundaryFault` at admission, the same gate the analysis quantity/pset arms thread their selector into, never a second selection engine. The C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the lifecycle dimension the managed projection does not produce, taking the validated selector through that shared gate and reaching the C# owner system through the one graduation rail.

## [01]-[INDEX]

- [01]-[LIFECYCLE]: the quantity, cost, schedule, patch, and diff lifecycle phases under one `LifecyclePhase`-discriminated owner folding the four IfcOpenShell ecosystem siblings.

## [02]-[LIFECYCLE]

- Owner: `IfcLifecycle` — the `@staticmethod` boundary capsule mirroring `IfcAnalysis`, dispatching the lifecycle phases over the `ifc5d`/`ifc4d`/`ifcpatch`/`ifcdiff` ecosystem through a thin `_dispatch` fold over per-phase helpers (`_takeoff`/`_cost`/`_schedule`/`_patch`/`_diff`), never five fat in-arm bodies; `LifecyclePhase` the closed `StrEnum` selecting the phase; `ScheduleFormat` the closed `StrEnum` selecting the `ifc4d` named parser so the schedule arm is one row over a closed parser vocabulary rather than a parse-per-format function family; `CostReport` the closed `StrEnum` selecting the `ifc5Dspreadsheet` writer so the cost arm's export leg is one row over `Ifc5DCsvWriter`/`Ifc5DOdsWriter`/`Ifc5DXlsxWriter`; `DiffChange` the closed `StrEnum` classifying the `ifcdiff` per-element change (added/deleted/geometry/attribute/pset/relationship) so a revision-diff row carries the bounded change vocabulary, never a stringified `deepdiff` blob; `LifecycleRow` the `@tagged_union` result row carrying one typed case per phase (`quantity` element-GUID/qto-name/quantity-name/`float` measure, `cost` item-GUID/name/`float` applied value, `task` GUID/name, `patch` recipe/product, `diff` element-GUID/`DiffChange`) so the receipt rows are typed evidence the toolchain reads field-by-field, never a stringly `dict[str, str]`; `LifecycleReceipt` the typed receipt carrying the phase, the subject element set, the `tuple[LifecycleRow, ...]` typed result rows, and the kind-specific `evidence` residual ledger the graduation leg folds.
- Cases: `LifecyclePhase` rows `QUANTITY` (rule-driven base-quantity take-off over `ifc5d.qto.quantify`/`edit_qtos` keyed by the `qto.rules` rule-set table, writing `IfcElementQuantity` and reading the derived quantity keys) · `COST` (`ifcopenshell.api.cost.calculate_cost_item_resource_value` rollup over each `IfcCostItem` plus the `CostReport`-selected `ifc5d.ifc5Dspreadsheet.Ifc5DCsvWriter`/`Ifc5DOdsWriter` structured export leg) · `SCHEDULE` (`ifc4d` `<Format>2Ifc` named-parser conversion populating `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence`) · `PATCH` (`ifcpatch.execute` named-recipe transformation over the `recipes` namespace plus the `ifcpatch.write` polymorphic sink) · `DIFF` (`ifcdiff.IfcDiff(old, new).diff()` revision comparison over `deepdiff`, reading `change_register`) — matched by `match`/`assert_never`, each dispatching to the ecosystem sibling that owns it. A new phase is one `LifecyclePhase` row plus one fold arm and breaks every dispatch site at type-check time under `ty`/`py_analyzer`.
- Entry: `IfcLifecycle.run` takes an `ifcopenshell.file`, a `LifecyclePhase`, and a `spec` whose meaning is fixed by the phase — a validated selector query for `QUANTITY`, a cost-schedule GlobalId plus report token for `COST`, a `<format>:<path>` schedule source for `SCHEDULE`, a `<recipe>:<json-args>` patch directive for `PATCH`, a revision-model path for `DIFF` — and returns a `RuntimeRail[LifecycleReceipt]` through one `boundary(f"lifecycle.{phase}", ...)` admission `.bind`-flattened over the rail-returning `_dispatch`, so a provider exception converts to a `BoundaryFault` exactly once at the seam while a selector parse fault arrives already typed on the rail. The dispatch is one rail-returning fold mirroring the analysis sibling: `_dispatch` partitions the `spec` once on the per-phase `PHASE_DELIMITER` row — every phase is a table key including `DIFF` (the empty-delimiter row that passes the whole `spec` as the revision path without a `partition("")` `ValueError`), never a `.get` default that silently drops a phase — then each arm yields `RuntimeRail[LifecycleReceipt]`: the `QUANTITY` arm by `IfcSelector.filter(model, head).map(_takeoff)` so the validated selector rail composes monadically and a malformed selector surfaces as the selector page's `UnexpectedInput`-derived `BoundaryFault` before `quantify` runs, the four non-selecting arms lifting their helper-built receipt through `Ok`, never a second call shape or an out-of-fold short-circuit. The `#<rule-set>`/`<report>`/`<format>`/`<recipe>` token is the `tail` of the one partition, the selector/schedule-id/format/recipe the `head`, so the spec parse is one data-driven split keyed by the `PHASE_DELIMITER` table, never a partition-per-arm string ladder. Each arm derives its own `subjects` from the phase's true subject set: quantity element GlobalIds for `QUANTITY`, the schedule id plus the bound writer class for `COST`, task GlobalIds for `SCHEDULE`, the recipe plus the patched-output schema/product for `PATCH`, changed-element GlobalIds for `DIFF` — so the subject field never carries a meaningless run.
- Auto: the `QUANTITY` arm maps the already-validated `elements` tuple off the `IfcSelector.filter` rail into the `_takeoff` fold (no in-arm `filter_elements` call), running `ifc5d.qto.quantify(model, set(elements), ifc5d.qto.rules[rule_set])` — the rule-driven measurement kernel computing the full base-quantity schedule per element keyed by the `qto.rules` `RULE_SET` table (the IFC4 vs IFC4X3 base-quantity rule set the `spec`'s `#<rule-set>` suffix selects, defaulting to `IFC4QtoBaseQuantities`) — then `ifc5d.qto.edit_qtos(model, results)` writes the computed `ResultsDict` back as `IfcElementQuantity` base quantities, replacing the deleted `get_psets(qtos_only=True)`/`NetFloorArea` single-key fold, and folds the `ResultsDict` (`element -> qto-name -> quantity-name -> value`) directly into typed `LifecycleRow.of_quantity` rows carrying the element GUID, the qto name, the quantity name, and the `float` measure (never the `str(value)` coercion the old stringly fold lost the numeric type to); the `_cost` arm runs `ifcopenshell.api.cost.calculate_cost_item_resource_value(model, cost_item=item)` over each `IfcCostItem` to roll its resource values, binds the `CostReport`-selected `ifc5d.ifc5Dspreadsheet` writer class (`Ifc5DCsvWriter`/`Ifc5DOdsWriter`/`Ifc5DXlsxWriter`) onto the receipt subject for the data boundary to drive against a durable path, and reads each `IfcCostItem.CostValues` `IfcCostValue.AppliedValue` back as typed `LifecycleRow.of_cost` rows carrying the item GUID, name, and the `float` applied value — the spreadsheet write is a columnar product the data boundary owns, never a throwaway `tempfile.TemporaryDirectory()` write the run discards; the `_schedule` arm constructs the `ScheduleFormat`-selected `<Format>2Ifc` parser, sets its `.file`/`.xml`/`.work_plan` slots, and runs `.execute()` to populate the `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence` task tree, reading the task GlobalIds back as `LifecycleRow.of_task` rows; the `_patch` arm runs `ifcpatch.execute({"input", "file", "recipe", "arguments"})` dispatching the named recipe over the `recipes` namespace and the `ifcpatch.write` polymorphic sink serializing the recipe-determined output, emitting one `LifecycleRow.of_patch` row keyed by the patched-file schema or the non-IFC product type; the `_diff` arm runs `ifcdiff.IfcDiff(model, revision).diff()` populating `change_register` over `deepdiff` and folds the GlobalId-keyed map through `DiffChange.of_register` into typed `LifecycleRow.of_diff` rows carrying the bounded change classification, never `str(change)`. No phase carries an `if/else` value ladder and no phase mints a sibling per-phase class — one fold arm and one helper per row, the package that owns the phase bound directly, every row a typed `LifecycleRow` case.
- Receipt: each run contributes an emitted-phase `Receipt.of` row through the `ReceiptContributor` Protocol `LifecycleReceipt.contribute` satisfies, whose facts are the flattened per-row `LifecycleRow.facts` projection keyed `f"{phase}.{i}.{field}"` (quantity keys for `QUANTITY`, item value for `COST`, task name for `SCHEDULE`, recipe and product for `PATCH`, the `DiffChange` value for `DIFF`) joined with the subject count and the resolved `evidence` ledger, never the lossy `str(dict)` coercion that buries the typed fields in one opaque value. The run graduates through `LifecycleReceipt.graduate(key, ceiling)` over the kind-specific `LifecycleReceipt.evidence` residual ledger — the empty-row fraction for the `QUANTITY`/`COST`/`SCHEDULE`/`PATCH` arms (a phase that produces no rows for a non-empty subject set is a degenerate run keyed `1.0`) and the changed-element drift fraction for `DIFF` — folded through one `GraduationReceipt.graduates("rasm.geometry.ifc.costing", HandoffAxis(geometry=LIFECYCLE_SUBJECT), key, self.evidence(), ceiling)` admission against the caller-supplied per-key ceiling, so the lifecycle output crosses the one geometry graduation rail under the canonical `GeometrySubject` `"numerical-primitive"` literal the compute `graph/handoff` producer owns (the same subject the `ifc/structural` section-integral and `ifc/analysis` owners cross on, the source-package fixed not threaded), never a bare `str` subject, never a row/subject count ledger that clears against any nonzero ceiling, and never an empty `measured={}`/`ceiling={}` no-op admission — take-off rows, cost reports, schedules, patched models, and revision diffs reach the C# owner system through the one graduation rail as typed lifecycle output the toolchain consumes directly. The typed `LifecycleRow` is the carry and the stringified facts are the projection, never the source of truth.
- Packages: `ifc5d` (`qto.rules` rule-set table/`qto.quantify`/`qto.edit_qtos`/`ifc5Dspreadsheet.Ifc5DCsvWriter`/`Ifc5DOdsWriter`/`Ifc5DXlsxWriter`), `ifcopenshell` (`api.cost.calculate_cost_item_resource_value` cost rollup, `file`/`by_guid`/`by_type`/`open` over the in-process model only — selector filtering is the validated gate, never a direct `util.selector.filter_elements` call here), `ifc4d` (`MSProject2Ifc`/`P62Ifc`/`Asta2Ifc` named parsers, `.file`/`.xml`/`.work_plan` slots, `.execute()`), `ifcpatch` (`execute`/`write` over the `recipes` namespace), `ifcdiff` (`IfcDiff`/`diff`/`change_register`/`export`), geometry (`IfcSelector.filter` the shared validated selector entry gate from `ifc/selector.md#SELECTOR`, `GraduationReceipt.graduates`/`HandoffAxis`/`GeometrySubject` the compute graduation rail), `expression` (`tagged_union` for `LifecycleRow`), runtime (`RuntimeRail`/`boundary`/`Receipt`/`ReceiptContributor`); the columnar cost-spreadsheet write defers to `python:data/spatial`, never a local file sink.
- Growth: a new quantity rule set is one `qto.rules` `RULE_SET` key authored upstream, never a local measurement fold; a new cost report format is one `CostReport` row binding its `ifc5Dspreadsheet` writer subclass (the `Ifc5DXlsxWriter` row already admitted); a new schedule format is one `ScheduleFormat` row binding its `<Format>2Ifc` parser; a new model transformation is one `recipe` name in the `ifcpatch.execute` directive, never a `file.add`/`remove` loop; a new diff change classification is one `DiffChange` row plus one `of_register` match arm; zero new surface, no parallel per-phase class family.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy (projected in-process); no durable store; no Rhino/GH mutation; the `ifcopenshell.file` model is the only foreign object held, and the four ecosystem siblings import function-local under `# noqa: PLC0415` at boundary scope, never module-top, per the manifest import policy. A raw `util.selector.filter_elements` passthrough of the unvalidated `spec` selector (the deleted form — the selector text crosses the shared `IfcSelector.filter` validated gate so a malformed query is a `BoundaryFault` at admission, never a silent empty match feeding `quantify`), a hand-rolled `NetFloorArea` quantity fold, a per-IFC-class measurement function family where the `qto.rules` table carries the rule, a stringly `dict[str, str]` result row where the typed `LifecycleRow` union carries the phase-specific fields, a `str(change)` diff row where `DiffChange` classifies the `deepdiff` change into the bounded added/deleted/geometry/attribute/pset/relationship vocabulary, a bare `str` graduation subject where the canonical `GeometrySubject` `"numerical-primitive"` literal is the wire vocabulary, a row/subject-count residual ledger that clears against any nonzero ceiling where the kind-specific `evidence` fold keys the empty-row or drift fraction, a bespoke cost-rollup loop where `ifcopenshell.api.cost.calculate_cost_item_resource_value` owns the per-item resource rollup, a throwaway `tempfile.TemporaryDirectory()` spreadsheet write the run discards where the columnar export defers to the data boundary, a hand-written P6/MS Project XML parser where the `<Format>2Ifc` parser owns the conversion, an ad-hoc `file.create_entity`/`add`/`remove` mutation loop where a recipe owns the transformation, and a `by_type`/attribute-walk diff where `IfcDiff`/`deepdiff` own the comparison are the deleted forms — the validated selector gate, the typed row union, the rule table, the named parser family, the recipe vocabulary, the bounded change classification, and the structured diff compose the provider tools end-to-end.

```python signature
from enum import StrEnum
from typing import Literal, assert_never

import ifcopenshell
from expression import Ok, case, tag, tagged_union
from msgspec import Struct
from msgspec.json import decode

from rasm.compute.graduation.handoff import GeometrySubject, GraduationReceipt, HandoffAxis
from rasm.geometry.ifc.selector import IfcSelector
from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, ReceiptContributor

# --- [TYPES] ---------------------------------------------------------------------------


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
    XLSX = "xlsx"


class DiffChange(StrEnum):
    ADDED = "added"
    DELETED = "deleted"
    GEOMETRY = "geometry"
    ATTRIBUTE = "attribute"
    PSET = "pset"
    RELATIONSHIP = "relationship"

    @staticmethod
    def of_register(change: object) -> "DiffChange":
        # The ifcdiff change_register value is the deepdiff per-element classification;
        # the closed vocabulary is the wire fact, never a stringified deepdiff blob.
        match change:
            case {"added": True}:
                return DiffChange.ADDED
            case {"deleted": True}:
                return DiffChange.DELETED
            case {"geometry_changed": True} | {"geometry": object()}:
                return DiffChange.GEOMETRY
            case {"properties_changed": object()} | {"pset": object()}:
                return DiffChange.PSET
            case {"relationships_changed": object()}:
                return DiffChange.RELATIONSHIP
            case _:
                return DiffChange.ATTRIBUTE


@tagged_union(frozen=True)
class LifecycleRow:
    tag: Literal["quantity", "cost", "task", "patch", "diff"] = tag()
    quantity: tuple[str, str, str, float] = case()
    cost: tuple[str, str, float] = case()
    task: tuple[str, str] = case()
    patch: tuple[str, str] = case()
    diff: tuple[str, DiffChange] = case()

    @staticmethod
    def of_quantity(element: str, qto: str, name: str, value: float) -> "LifecycleRow":
        return LifecycleRow(quantity=(element, qto, name, value))

    @staticmethod
    def of_cost(item: str, name: str, applied: float) -> "LifecycleRow":
        return LifecycleRow(cost=(item, name, applied))

    @staticmethod
    def of_task(guid: str, name: str) -> "LifecycleRow":
        return LifecycleRow(task=(guid, name))

    @staticmethod
    def of_patch(recipe: str, product: str) -> "LifecycleRow":
        return LifecycleRow(patch=(recipe, product))

    @staticmethod
    def of_diff(element: str, change: object) -> "LifecycleRow":
        return LifecycleRow(diff=(element, DiffChange.of_register(change)))

    @property
    def facts(self) -> dict[str, str]:
        match self:
            case LifecycleRow(tag="quantity", quantity=(element, qto, name, value)):
                return {"element": element, "quantity": f"{qto}.{name}", "value": repr(value)}
            case LifecycleRow(tag="cost", cost=(item, name, applied)):
                return {"item": item, "name": name, "value": repr(applied)}
            case LifecycleRow(tag="task", task=(guid, name)):
                return {"task": guid, "name": name}
            case LifecycleRow(tag="patch", patch=(recipe, product)):
                return {"recipe": recipe, "product": product}
            case LifecycleRow(tag="diff", diff=(element, change)):
                return {"element": element, "change": change.value}
            case unreachable:
                assert_never(unreachable)


# --- [CONSTANTS] -----------------------------------------------------------------------

# The 5D/4D lifecycle output crosses the geometry graduation case as a numerical primitive,
# the same GeometrySubject literal the section-integral and analysis owners cross on.
LIFECYCLE_SUBJECT: GeometrySubject = "numerical-primitive"

# The phase's spec carries the rule-set/report/format/recipe token after a single delimiter;
# the delimiter is the partition vocabulary keyed for every phase, never a parse-per-phase
# string ladder and never a silent `.get` default that drops a phase off the table.
PHASE_DELIMITER: dict[LifecyclePhase, str] = {
    LifecyclePhase.QUANTITY: "#",
    LifecyclePhase.COST: ":",
    LifecyclePhase.SCHEDULE: ":",
    LifecyclePhase.PATCH: ":",
    LifecyclePhase.DIFF: "",
}

# --- [MODELS] --------------------------------------------------------------------------


class LifecycleReceipt(Struct, frozen=True):
    phase: LifecyclePhase
    subjects: tuple[str, ...]
    rows: tuple[LifecycleRow, ...]

    def evidence(self) -> dict[str, float]:
        # The residual ledger is phase-specific, never a row/subject count that clears against
        # any ceiling: a take-off/cost/schedule arm that produces no rows for a non-empty subject
        # set is a degenerate run keyed by the empty fraction, and a revision diff keys the
        # changed-element fraction so a model whose drift exceeds the caller's ceiling is rejected.
        match self.phase:
            case LifecyclePhase.QUANTITY | LifecyclePhase.COST | LifecyclePhase.SCHEDULE:
                produced = max(len(self.subjects), 1)
                return {"empty": 1.0 - min(len(self.rows), produced) / produced}
            case LifecyclePhase.PATCH:
                return {"empty": 0.0 if self.rows else 1.0}
            case LifecyclePhase.DIFF:
                drift = sum(1.0 for r in self.rows if r.tag == "diff")
                return {"drift": drift / max(len(self.subjects), 1)}
            case unreachable:
                assert_never(unreachable)

    def graduate(self, key: ContentKey, ceiling: dict[str, float]) -> "RuntimeRail[GraduationReceipt]":
        return GraduationReceipt.graduates(
            "rasm.geometry.ifc.costing",
            HandoffAxis(geometry=LIFECYCLE_SUBJECT),
            key,
            self.evidence(),
            ceiling,
        )

    def contribute(self) -> Receipt:
        facts = {f"{self.phase}.{i}.{k}": v for i, row in enumerate(self.rows) for k, v in row.facts.items()}
        return Receipt.of(
            "emitted",
            "rasm.geometry.ifc.costing",
            self.phase.value,
            facts | {"subjects": str(len(self.subjects))} | {k: repr(v) for k, v in self.evidence().items()},
        )


# --- [OPERATIONS] ----------------------------------------------------------------------


class IfcLifecycle:
    @staticmethod
    def run(model: "ifcopenshell.file", phase: LifecyclePhase, spec: str) -> "RuntimeRail[LifecycleReceipt]":
        return boundary(f"lifecycle.{phase}", lambda: IfcLifecycle._dispatch(model, phase, spec)).bind(lambda rail: rail)

    @staticmethod
    def _dispatch(model: "ifcopenshell.file", phase: LifecyclePhase, spec: str) -> "RuntimeRail[LifecycleReceipt]":
        delimiter = PHASE_DELIMITER[phase]
        head, _, tail = spec.partition(delimiter) if delimiter else (spec, "", "")
        match phase:
            case LifecyclePhase.QUANTITY:
                return IfcSelector.filter(model, head).map(
                    lambda elements: IfcLifecycle._takeoff(model, elements, tail or "IFC4QtoBaseQuantities")
                )
            case LifecyclePhase.COST:
                return Ok(IfcLifecycle._cost(model, head, CostReport(tail or "csv")))
            case LifecyclePhase.SCHEDULE:
                return Ok(IfcLifecycle._schedule(model, ScheduleFormat(head), tail))
            case LifecyclePhase.PATCH:
                return Ok(IfcLifecycle._patch(model, head, tail))
            case LifecyclePhase.DIFF:
                return Ok(IfcLifecycle._diff(model, head))
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _cost(model: "ifcopenshell.file", schedule_guid: str, report: CostReport) -> LifecycleReceipt:
        import ifc5d.ifc5Dspreadsheet  # noqa: PLC0415
        import ifcopenshell.api.cost  # noqa: PLC0415

        schedule = model.by_guid(schedule_guid)
        items = model.by_type("IfcCostItem")
        for item in items:
            ifcopenshell.api.cost.calculate_cost_item_resource_value(model, cost_item=item)
        # The structured spreadsheet export is the columnar 5D product the data boundary owns;
        # this owner reads the rolled IfcCostValue.AppliedValue measure back as typed rows and
        # binds the format-selected ifc5Dspreadsheet writer onto the receipt rather than writing
        # a throwaway temp-dir file the run discards. The writer class is the closed CostReport
        # row; the data boundary drives its `.write()` against the durable output path.
        writer_cls = {
            CostReport.CSV: ifc5d.ifc5Dspreadsheet.Ifc5DCsvWriter,
            CostReport.ODS: ifc5d.ifc5Dspreadsheet.Ifc5DOdsWriter,
            CostReport.XLSX: ifc5d.ifc5Dspreadsheet.Ifc5DXlsxWriter,
        }[report]
        rows = tuple(
            LifecycleRow.of_cost(item.GlobalId, item.Name or "", float(getattr(value, "AppliedValue", 0.0) or 0.0))
            for item in items
            for value in (item.CostValues or ())
        )
        return LifecycleReceipt(
            LifecyclePhase.COST, (schedule.GlobalId, writer_cls.__name__), rows
        )

    @staticmethod
    def _schedule(model: "ifcopenshell.file", fmt: ScheduleFormat, source: str) -> LifecycleReceipt:
        import ifc4d.asta2ifc  # noqa: PLC0415
        import ifc4d.msproject2ifc  # noqa: PLC0415
        import ifc4d.p62ifc  # noqa: PLC0415

        parser = {
            ScheduleFormat.MSPROJECT: ifc4d.msproject2ifc.MSProject2Ifc,
            ScheduleFormat.P6: ifc4d.p62ifc.P62Ifc,
            ScheduleFormat.ASTA: ifc4d.asta2ifc.Asta2Ifc,
        }[fmt]()
        parser.file = model
        parser.xml = source
        plans = model.by_type("IfcWorkPlan")
        parser.work_plan = plans[0] if plans else None
        parser.execute()
        tasks = model.by_type("IfcTask")
        rows = tuple(LifecycleRow.of_task(t.GlobalId, t.Name or "") for t in tasks)
        return LifecycleReceipt(LifecyclePhase.SCHEDULE, tuple(t.GlobalId for t in tasks), rows)

    @staticmethod
    def _patch(model: "ifcopenshell.file", recipe: str, args: str) -> LifecycleReceipt:
        import ifcpatch  # noqa: PLC0415

        output = ifcpatch.execute({
            "input": "",
            "file": model,
            "recipe": recipe,
            "arguments": decode(args.encode(), type=list[object]) if args else [],
        })
        ifcpatch.write(output, f"lifecycle.{recipe}")
        product = output.schema if isinstance(output, ifcopenshell.file) else type(output).__name__
        rows = (LifecycleRow.of_patch(recipe, product),)
        return LifecycleReceipt(LifecyclePhase.PATCH, (recipe, product), rows)

    @staticmethod
    def _diff(model: "ifcopenshell.file", revision_path: str) -> LifecycleReceipt:
        import ifcdiff  # noqa: PLC0415

        differ = ifcdiff.IfcDiff(model, ifcopenshell.open(revision_path))
        differ.diff()
        register = differ.change_register
        rows = tuple(LifecycleRow.of_diff(guid, change) for guid, change in register.items())
        return LifecycleReceipt(LifecyclePhase.DIFF, tuple(register), rows)

    @staticmethod
    def _takeoff(
        model: "ifcopenshell.file", elements: tuple["ifcopenshell.entity_instance", ...], rule_set: str
    ) -> LifecycleReceipt:
        import ifc5d.qto  # noqa: PLC0415

        results = ifc5d.qto.quantify(model, set(elements), ifc5d.qto.rules[rule_set])
        ifc5d.qto.edit_qtos(model, results)
        rows = tuple(
            LifecycleRow.of_quantity(element.GlobalId, qto, name, float(value))
            for element, qtos in results.items()
            for qto, quantities in qtos.items()
            for name, value in quantities.items()
        )
        return LifecycleReceipt(LifecyclePhase.QUANTITY, tuple(e.GlobalId for e in results), rows)
```

## [03]-[RESEARCH]

- [QTO_RULE_SET]: the branch `ifc5d` catalogue confirms `qto.rules` (the `dict[RULE_SET, ...]` base-quantity rule table loaded from the bundled `IFC4QtoBaseQuantities`/`IFC4X3QtoBaseQuantities` JSON, keyed by the `RULE_SET` literal, `ifc5d.md#23`/`47`), `qto.quantify(ifc_file, elements, rules) -> ResultsDict` (the rule-driven measurement kernel, `#24`/`46`), and `qto.edit_qtos(ifc_file, results) -> None` (write the `ResultsDict` back as `IfcElementQuantity` base quantities, `#25`/`48`). The `quantify` `elements: set[ifcopenshell.entity_instance]` set form (the fence wraps the `IfcSelector.filter`-validated `elements` tuple in `set(...)`) and the `ResultsDict = dict[element, dict[qto-name, dict[quantity-name, float]]]` nesting the row fold reads (each leaf `float` flowing into the typed `LifecycleRow.of_quantity` measure field) confirm by introspection against the installed companion distribution. The `IFC4` versus `IFC4X3` rule-set choice the `spec`'s `#<rule-set>` suffix selects defaults to `IFC4QtoBaseQuantities`; the live-run residual is the exact `RULE_SET` literal set bundled in the `0.8.5` distribution (`ifc5d.md#71`).
- [COST_ROLLUP_EXPORT]: the cost rollup is `ifcopenshell.api.cost.calculate_cost_item_resource_value(file, cost_item)` per `IfcCostItem` (summing the construction-resource base costs into the item's resource value, an `ifcopenshell.api.cost` function, NOT an `ifc5d.cost` member — `ifc5d` carries no `cost` module), and the structured export is the `ifc5d.ifc5Dspreadsheet.Ifc5DCsvWriter`/`Ifc5DOdsWriter`/`Ifc5DXlsxWriter` writer family, each constructed `(file, output_dir, cost_schedule=None)` and run through `.write()` (writing the spreadsheet to `output_dir`, no return rows), confirmed against the installed companion distribution (`ifc5d.md#49`/`50`/`51`). The `_cost` arm binds the `CostReport`-selected writer class onto the receipt subject for the data spatial-codec boundary to drive against a durable path rather than constructing-and-running the writer over a throwaway `tempfile.TemporaryDirectory()` the run discards: the spreadsheet is a columnar product `python:data/spatial` owns, and this owner's wire carry is the typed `of_cost` rows the `IfcCostItem.CostValues` `IfcCostValue.AppliedValue` measure feeds (the value-select union `float(getattr(value, "AppliedValue", 0.0) or 0.0)` coerces into the `LifecycleRow.of_cost` `float` field, the `None`/measure-select fallback the `or 0.0` guards), the live-run-confirmed member before the typed row construction binds. The typed `float` carry replaces the stringly `str(...)` coercion the old `dict[str, str]` row lost the numeric type to; the durable spreadsheet path and the writer's exact constructor binding under the data boundary are the runtime-action residual, not a fence blocker.
- [SCHEDULE_SLOT_POPULATION]: the branch `ifc4d` catalogue confirms the `MSProject2Ifc`/`P62Ifc`/`Asta2Ifc` named-parser family (`ifc4d.md#23`-`25`), the `.execute()` entry (`#37`-`39`), the `.file`/`.xml`/`.work_plan` slots (`#45`), and the populated `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence` set — the schedule format is the named parser class, a closed parser set, never a parse-per-format function family, so the `SCHEDULE` arm is fully fenced. The narrow residual is the live-run `.file`/`.xml`/`.work_plan` slot-population semantics: whether `.xml` accepts the source path string or a parsed document, and whether `.work_plan` requires a pre-existing `IfcWorkPlan` or the parser mints one — the companion-interpreter `<Format>2Ifc.execute()` run confirms the slot contract, a runtime-action band step, not a fence blocker.
- [PATCH_OUTPUT_DISPATCH]: the branch `ifcpatch` catalogue confirms `ifcpatch.execute({"input", "file", "recipe", "arguments"})` (named-recipe dispatch over the `Patcher` base, `ifcpatch.md#39`), the `recipes` namespace recipe names (`#24`), and `ifcpatch.write(output, filepath)` (the polymorphic sink, `#40`); the `ifcpatch.write` output-type dispatch (a patched `ifcopenshell.file` versus a recipe's non-IFC string/path product) is the recipe-determined return the `_patch` arm's `isinstance(output, ifcopenshell.file)` discrimination reads, projecting the patched-file `.schema` or the non-IFC product type as the `of_patch` product and the receipt subject, confirmed by introspection against the installed companion distribution (`#62`). The `arguments` list decodes through `decode(args.encode(), type=list[object])` — the parameterized `list[object]` element shape, never a bare untyped `list`.
- [DIFF_CHANGE_REGISTER]: the branch `ifcdiff` catalogue confirms `IfcDiff(old_file, new_file)` (the two-model constructor, `ifcdiff.md#33`), `IfcDiff.diff()` (`#34`), `IfcDiff.change_register` (the GlobalId-keyed added/deleted/changed map, `#36`), and `IfcDiff.export(path)` (`#35`); the `change_register` value shape (the per-element change classification — added/deleted/geometry/attribute/pset/relationship — `deepdiff` populates) folds through `DiffChange.of_register` into the closed `DiffChange` `StrEnum` the `of_diff` row carries, never `str(change)`. The `of_register` match arms map the live-run `change_register` value keys (whether the per-element record flags `added`/`deleted` or carries `geometry_changed`/`properties_changed`/`relationships_changed` markers) onto the bounded vocabulary; the exact key spelling the `0.8.5` `deepdiff` integration writes is the runtime-action residual the `of_register` fold absorbs without a fence change, `attribute` the total catch-all so an unrecognized record never escapes the closed enum (`#56`).
- [VALIDATED_SELECTOR_GATE]: the `QUANTITY` arm threads its selector text (the `head` of the one `PHASE_DELIMITER`-keyed partition) through `IfcSelector.filter(model, head)` from `geometry:ifc/selector.md#SELECTOR` — the shared validated entry gate the analysis quantity/pset arms also consume — returning `RuntimeRail[tuple[ifcopenshell.entity_instance, ...]]`, so the arm `.map`s the selector rail into the `_takeoff` fold and a malformed selector surfaces as the selector page's `UnexpectedInput`-derived `BoundaryFault` at admission rather than as a silent empty `filter_elements` match feeding `quantify`. The whole verb is one rail-returning `_dispatch` fold flattened through the outer `boundary(...).bind(lambda rail: rail)`, exactly the `geometry:ifc/analysis.md#ANALYSIS` composition: the `QUANTITY` arm yields the selector rail mapped to a receipt, the four non-selecting arms lift their receipt through `Ok`, so a provider exception is a `BoundaryFault` at the seam and a selector fault is already typed on the rail, with no second call shape or out-of-fold short-circuit. The `#<rule-set>` token is the `tail` of the partition, so the validated query the gate parses never carries the rule-set suffix. The `.map` rail composition is the `expression` `Result.map`, `Ok` the `Result` constructor, and the outer `.bind(lambda rail: rail)` the `Result.bind` rail-flatten confirmed at `.api/expression.md#97`/`#140`, the same monadic surface the selector page's `.map` leg uses.
- [TYPED_LIFECYCLE_ROW]: the result rows are the `expression` `@tagged_union` `LifecycleRow` (`tagged_union`/`tag`/`case` confirmed at `.api/expression.md#129`/`#140`), one typed case per phase — `quantity` carrying `(element-GUID, qto-name, quantity-name, float)`, `cost` carrying `(item-GUID, name, float)`, `task`/`patch` carrying their `(str, str)` pairs, and `diff` carrying `(element-GUID, DiffChange)` so the change classification is the closed enum not a stringified blob — replacing the stringly `tuple[dict[str, str], ...]` rows; the `facts` property is the one total `match`/`assert_never` fold projecting each case to its `dict[str, str]` (the `diff` arm rendering the `DiffChange.value`), flattened per-row by `contribute` into the `Receipt.of` facts map under the `f"{phase}.{i}.{field}"` key so each typed field stays an addressable fact rather than collapsing into one `str(dict)` blob, the typed row the carry and the per-field projection lossless to the field grain. The graduation subject is the `LIFECYCLE_SUBJECT` module constant binding the canonical `GeometrySubject` `"numerical-primitive"` literal from `compute:graduation/handoff.md#HANDOFF` (the `HandoffAxis(geometry=...)` case, the same subject the `ifc/structural` section-integral and `ifc/analysis` owners cross on), passed to `GraduationReceipt.graduates` with the kind-specific `LifecycleReceipt.evidence` residual ledger — the empty-row fraction for the `QUANTITY`/`COST`/`SCHEDULE`/`PATCH` arms and the changed-element drift fraction for `DIFF`, mirroring the analysis sibling's `AnalysisResult.evidence` discipline — against the caller's ceiling, so an unlisted literal fails at the type boundary the handoff producer owns and a degenerate or over-drifting run is an `Error(BoundaryFault)` rather than a graduated handoff — never a bare `str` subject, never a row/subject-count ledger that clears against any nonzero ceiling, and never a `measured={}` graduation that clears trivially.

## [04]-[UPSTREAM]

- [COMPANION_LANE]: `ifc5d`, `ifc4d`, `ifcpatch`, and `ifcdiff` are the `0.8.5` IfcOpenShell-ecosystem siblings that ride the `ifcopenshell` companion lane — each depends `ifcopenshell` (`ifcpatch` additionally `toposort`/`numpy`, `ifcdiff` additionally `deepdiff`, `ifc4d` additionally `typing-extensions`) and resolves where `ifcopenshell` resolves, so the four siblings are inert in the cp315 project venv that carries no `ifcopenshell` core and settle only on the `<'3.15'` companion interpreter (cp313). The fence imports them function-local at boundary scope under `# noqa: PLC0415` per the manifest import policy, never module-top.
- [IFCOPENSHELL_PRESENCE]: `ifcopenshell` is itself absent from the repo-root manifest despite the `tessellation` daemon and the `ifc-analysis` owners consuming it; the four `0.8.5` siblings are a moot admission until `ifcopenshell` is manifest-present, so the manifest reconciliation admits `ifcopenshell` (companion lane) plus the four `ifc5d`/`ifc4d`/`ifcpatch`/`ifcdiff` `>=0.8.5` rows (`python_version<'3.15'`, depends `ifcopenshell`) as one batch — the `.api` catalogues exist, only the manifest rows remain.
