# [PY_GEOMETRY_IFC_COSTING]

The 5D/4D model-lifecycle owner — the construction-economics and model-management verbs the analysis hop drops: rule-driven quantity take-off, cost-schedule rollup, construction scheduling, recipe-driven model transformation, and two-model revision comparison. `IfcLifecycle` dispatches these five phases over the `ifc5d`/`ifc4d`/`ifcpatch`/`ifcdiff` IfcOpenShell-ecosystem siblings, emitting a `LifecycleReceipt` whose rows are the typed `LifecycleRow` union. The C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the lifecycle dimension the managed projection does not produce.

Every selecting phase admits its query through `IfcSelector` from `ifc/selector.md#SELECTOR`, so a malformed selector is a typed `BoundaryFault` at admission, never a silent empty `filter_elements` match feeding `quantify`. `run` threads the graduation `evidence_run` weave under `EvidenceScope.IFC_LIFECYCLE` with `@beartype(conf=FAULT_CONF)` on `_dispatch` binding the contract fence. Evidence graduates under `GeometrySubject.BIM_LIFECYCLE` — the differentiated 5D/4D member distinct from the section-integral and compliance members their owners bind — and crosses to the C# owner system through the one `graduates()`/`GeometryHandoff.wire()` rail. The durable cost-spreadsheet, `ifcpatch.write`, and diff-export writes defer to `python:data/spatial` as the token or product carried on the receipt.

## [01]-[INDEX]

- [01]-[LIFECYCLE]: the quantity, cost, schedule, patch, and diff phases under one `LifecyclePhase`-discriminated owner folding the four IfcOpenShell ecosystem siblings, the `IfcSelector` gate, and kind-specific graduation evidence under `BIM_LIFECYCLE`.

## [02]-[LIFECYCLE]

- Owner: `IfcLifecycle` — the `@staticmethod` boundary capsule mirroring `IfcAnalysis`, dispatching the five phases through one rail-returning `_dispatch` fold over per-phase helpers (`_takeoff`/`_cost`/`_schedule`/`_patch`/`_diff`), never five fat in-arm bodies. `LifecyclePhase`, `ScheduleFormat`, `CostReport`, `RuleSet`, and `DiffChange` are closed `StrEnum` discriminants parsed through the one generic `_token[E: StrEnum]` rail — each phase, `ifc4d` parser, `ifc5Dspreadsheet` writer family, `qto.rules` base-quantity set, and `ifcdiff` change class is a rail-validated row, never a raw string fed to `StrEnum(token)`/`rules[str]` that escapes a `ValueError`/`KeyError`. `LifecycleRow` is the `@tagged_union` result row carrying one typed case per phase, never a stringly `dict[str, str]` the toolchain must re-parse.
- Cases: `QUANTITY` (rule-driven take-off over `ifc5d.qto`), `COST` (`ifcopenshell.api.cost` rollup over each `IfcCostItem`), `SCHEDULE` (`ifc4d` `<Format>2Ifc` parser populating `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence`), `PATCH` (`ifcpatch.execute` named recipe over the `recipes` namespace), `DIFF` (`ifcdiff` revision comparison over `deepdiff`) — matched by `match`/`assert_never`, each dispatching to the ecosystem sibling that owns it.
- Entry: `IfcLifecycle.run` takes a model, a `LifecyclePhase`, and a `spec` whose meaning is phase-fixed — validated selector for `QUANTITY`, cost-schedule GlobalId plus report token for `COST`, `<format>:<path>` for `SCHEDULE`, `<recipe>:<json-args>` for `PATCH`, revision path for `DIFF` — returning `RuntimeRail[LifecycleReceipt]` through the `evidence_run` weave. `_dispatch` partitions the `spec` once on the `PHASE_DELIMITER` table keyed by every phase including `DIFF`'s empty-delimiter row (the whole `spec` as revision path, no `partition("")` fault), never a `.get` default that silently drops a phase off the table. The `QUANTITY` arm binds the `#<rule-set>` token AND the validated selector monadically, so both fault before `quantify` runs. Each arm derives its own `subjects` from the phase's true subject set; `DIFF`'s `population` field separately carries the full compared element count the drift fraction divides against.
- Auto: the `QUANTITY` arm's `ifc5d.qto.quantify`/`edit_qtos` answers the whole base-quantity schedule keyed by the `qto.rules` table and writes it back as `IfcElementQuantity`, superseding the `get_psets(qtos_only=True)`/`NetFloorArea` single-key fold the sibling owner shed. The `COST`, `PATCH`, and `DIFF` arms each carry the phase's product as a typed token on the receipt subject — the `CostReport` writer key, the patch product type, the diff change class — so the durable write stays the data boundary's. No phase carries an `if/else` value ladder or mints a per-phase class: one fold arm and one helper per row, the owning package bound directly.
- Receipt: the kind-specific `evidence` ledger keys the empty-row fraction for `QUANTITY`/`COST`/`SCHEDULE`/`PATCH` (a phase producing no rows for a non-empty subject set is a degenerate run keyed `1.0`) and the changed-over-`population` drift fraction for `DIFF` (never changed-over-changed, which clears every ceiling), so a model breaching the caller's ceiling fails the carrier's `admitted` verdict rather than crossing clean. `graduates()` returns `GeometryHandoff.of(BIM_LIFECYCLE, …)` against the per-key ceiling; the typed `LifecycleRow` is the carry and its per-field `facts` the lossless projection.
- Packages: `ifc5d` (`qto.rules`/`quantify`/`edit_qtos` take-off surface only — the `ifc5Dspreadsheet` writer family is the data boundary's), `ifcopenshell` (`api.cost` rollup and in-process model access; selector filtering is the validated gate, never a direct `util.selector.filter_elements` call here), `ifc4d` (`<Format>2Ifc` named parsers), `ifcpatch` (`execute` over the `recipes` namespace; the durable `write` is the data boundary's), `ifcdiff` (`IfcDiff`/`change_register`/`added_elements`/`deleted_elements`; the `export` JSON is the data boundary's), and `geometry`/`expression`/`beartype`/`runtime` per the fence imports; `IfcSelector` is the only `filter_elements` caller.
- Growth: a new quantity rule set is one `RuleSet` row over the upstream `qto.rules` key; a new cost format one `CostReport` row the data boundary binds to its `ifc5Dspreadsheet` writer subclass; a new schedule format one `ScheduleFormat` row binding its `<Format>2Ifc` parser; a new model transformation one `recipe` name in the `ifcpatch.execute` directive; a new diff classification one `DiffChange` row plus one `of_register` arm — zero new surface, no parallel per-phase class family.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy; no durable store — the cost spreadsheet, `ifcpatch.write` serialization, and diff `export` JSON all defer to `python:data/spatial` as the token or product carried on the receipt; no Rhino/GH mutation. The four ecosystem siblings import function-local under `# noqa: PLC0415` at boundary scope per the manifest import policy, and the `spec` selector crosses the `IfcSelector.filter` validated gate, never a raw `util.selector.filter_elements` passthrough.

```python signature
from collections.abc import Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from beartype import beartype
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from msgspec.json import decode

from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_run
from rasm.geometry.ifc.selector import IfcSelector
from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:  # every runtime ifcopenshell use is a function-local boundary import, so the module loads clean
    import ifcopenshell

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


class RuleSet(StrEnum):
    # `ifc5d.qto.rules` table keys; the `*Blender` variants are Blender-host-only and never cross this non-Blender lane.
    IFC4 = "IFC4QtoBaseQuantities"
    IFC4X3 = "IFC4X3QtoBaseQuantities"


class DiffChange(StrEnum):
    ADDED = "added"
    DELETED = "deleted"
    GEOMETRY = "geometry"
    ATTRIBUTE = "attribute"
    PSET = "pset"
    RELATIONSHIP = "relationship"

    @staticmethod
    def of_register(markers: object) -> "DiffChange":
        # The `change_register[guid]` marker dict carries one or more `*_changed` flags at once, so the arm
        # order IS the change-priority collapse to one row. `attributes_changed` is matched by intent, so the
        # `_` fallback is the closed-enum floor absorbing an unrecognized future marker as ATTRIBUTE. Element
        # presence rides the disjoint `added_elements`/`deleted_elements` sets `_diff` classifies directly.
        match markers:
            case {"geometry_changed": True}:
                return DiffChange.GEOMETRY
            case {"properties_changed": object()}:
                return DiffChange.PSET
            case {"type_changed": True} | {"container_changed": True} | {"aggregate_changed": True} | {"classification_changed": True}:
                return DiffChange.RELATIONSHIP
            case {"attributes_changed": True} | _:
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
    def of_diff(element: str, change: DiffChange | dict[str, object]) -> "LifecycleRow":
        # A bare `DiffChange` is the already-classified presence row (the added/deleted GUID sets);
        # a `change_register` marker dict folds through `of_register` — one constructor, both sources.
        return LifecycleRow(diff=(element, change if isinstance(change, DiffChange) else DiffChange.of_register(change)))

    @property
    def facts(self) -> dict[str, object]:
        # Native float measures ride the runtime EventDict; pre-stringifying the measure is the receipts-owner deleted form.
        match self:
            case LifecycleRow(tag="quantity", quantity=(element, qto, name, value)):
                return {"element": element, "quantity": f"{qto}.{name}", "value": value}
            case LifecycleRow(tag="cost", cost=(item, name, applied)):
                return {"item": item, "name": name, "value": applied}
            case LifecycleRow(tag="task", task=(guid, name)):
                return {"task": guid, "name": name}
            case LifecycleRow(tag="patch", patch=(recipe, product)):
                return {"recipe": recipe, "product": product}
            case LifecycleRow(tag="diff", diff=(element, change)):
                return {"element": element, "change": change.value}
            case unreachable:
                assert_never(unreachable)


# --- [CONSTANTS] -----------------------------------------------------------------------

# The lifecycle output crosses on BIM_LIFECYCLE; an unlisted subject fails at the boundary under `ty`.
LIFECYCLE_SUBJECT: Final[GeometrySubject] = GeometrySubject.BIM_LIFECYCLE

# The delimiter is the partition vocabulary keyed for every phase, never a parse-per-phase ladder and never
# a `.get` default that drops a phase; `DIFF`'s empty-delimiter row passes the whole spec as the revision path.
PHASE_DELIMITER: Map[LifecyclePhase, str] = Map.of_seq([
    (LifecyclePhase.QUANTITY, "#"),
    (LifecyclePhase.COST, ":"),
    (LifecyclePhase.SCHEDULE, ":"),
    (LifecyclePhase.PATCH, ":"),
    (LifecyclePhase.DIFF, ""),
])

# The full ifcdiff relationship axis the audit scopes over, not the ctor's `["geometry"]` default;
# the `"geometry"` leg drives the costly tessellation, the rest fold markers off the model.
DIFF_AXIS: tuple[str, ...] = ("geometry", "attributes", "type", "property", "container", "aggregate", "classification")

# --- [MODELS] --------------------------------------------------------------------------


class LifecycleReceipt(Struct, frozen=True, gc=False):
    phase: LifecyclePhase
    subjects: tuple[str, ...]
    rows: tuple[LifecycleRow, ...]
    # The compared population the DIFF drift fraction divides against, NOT the changed-subject count
    # `subjects` carries (changed-over-changed is the always-1.0 ledger). The other phases ignore it.
    population: int = 0

    def evidence(self) -> dict[str, float]:
        # The residual ledger is phase-specific, never a row/subject count that clears against any ceiling.
        match self.phase:
            case LifecyclePhase.QUANTITY | LifecyclePhase.COST | LifecyclePhase.SCHEDULE:
                produced = max(len(self.subjects), 1)
                return {"empty": 1.0 - min(len(self.rows), produced) / produced}
            case LifecyclePhase.PATCH:
                return {"empty": 0.0 if self.rows else 1.0}
            case LifecyclePhase.DIFF:
                return {"drift": len(self.subjects) / max(self.population, 1)}
            case unreachable:
                assert_never(unreachable)

    def graduates(self, evidence_key: ContentKey, ceiling: dict[str, float]) -> GeometryHandoff:
        # the local carrier's residual-over-ceiling `admitted` verdict gates; `wire()` is the compute crossing.
        return GeometryHandoff.of(LIFECYCLE_SUBJECT, evidence_key, self.evidence(), ceiling)

    def contribute(self) -> "Iterable[Receipt]":
        facts = {f"{self.phase}.{i}.{k}": v for i, row in enumerate(self.rows) for k, v in row.facts.items()}
        yield Receipt.of("rasm.geometry.ifc.costing", ("emitted", self.phase.value, facts | {"subjects": len(self.subjects)} | self.evidence()))


# --- [OPERATIONS] ----------------------------------------------------------------------


def _token[E: StrEnum](vocabulary: type[E], raw: str) -> "RuntimeRail[E]":
    # The one generic closed-vocabulary parse for the report, format, AND rule-set tokens: an unknown token
    # is a typed `wire` fault, never a raw `StrEnum(raw)`/`rules[str]` escape. The `raw in vocabulary`
    # value-membership test is the public 3.12+ EnumType contract, no private map.
    return Ok(vocabulary(raw)) if raw in vocabulary else Error(BoundaryFault(wire=(f"lifecycle.{vocabulary.__name__}.{raw}", 0)))


class IfcLifecycle:
    @staticmethod
    def run(model: "ifcopenshell.file", phase: LifecyclePhase, spec: str) -> "RuntimeRail[LifecycleReceipt]":
        # the weave's flatten absorbs the rail-returning `_dispatch`, so a selector/token fault meets the
        # converted provider fault on one carrier and the harvest emits the receipt stream on the cleared Ok.
        return evidence_run(EvidenceScope.IFC_LIFECYCLE, f"run.{phase}", lambda: IfcLifecycle._dispatch(model, phase, spec))

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _dispatch(model: "ifcopenshell.file", phase: LifecyclePhase, spec: str) -> "RuntimeRail[LifecycleReceipt]":
        delimiter = PHASE_DELIMITER[phase]
        head, _, tail = spec.partition(delimiter) if delimiter else (spec, "", "")
        match phase:
            case LifecyclePhase.QUANTITY:
                # The selector rail AND the rule-set token both validate before `quantify`: a typo'd
                # `#<rule-set>` is a typed `wire` fault, never a raw `rules[str]` KeyError past the fence.
                return _token(RuleSet, tail or RuleSet.IFC4.value).bind(
                    lambda rule_set: IfcSelector.filter(model, head).map(lambda elements: IfcLifecycle._takeoff(model, elements, rule_set))
                )
            case LifecyclePhase.COST:
                return _token(CostReport, tail or "csv").map(lambda report: IfcLifecycle._cost(model, head, report))
            case LifecyclePhase.SCHEDULE:
                return _token(ScheduleFormat, head).map(lambda fmt: IfcLifecycle._schedule(model, fmt, tail))
            case LifecyclePhase.PATCH:
                return Ok(IfcLifecycle._patch(model, head, tail))
            case LifecyclePhase.DIFF:
                return Ok(IfcLifecycle._diff(model, head))
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _takeoff(model: "ifcopenshell.file", elements: tuple["ifcopenshell.entity_instance", ...], rule_set: RuleSet) -> LifecycleReceipt:
        import ifc5d.qto  # noqa: PLC0415

        results = ifc5d.qto.quantify(model, set(elements), ifc5d.qto.rules[rule_set.value])
        ifc5d.qto.edit_qtos(model, results)
        rows = tuple(
            LifecycleRow.of_quantity(element.GlobalId, qto, name, float(value))
            for element, qtos in results.items()
            for qto, quantities in qtos.items()
            for name, value in quantities.items()
        )
        return LifecycleReceipt(LifecyclePhase.QUANTITY, tuple(e.GlobalId for e in results), rows)

    @staticmethod
    def _cost(model: "ifcopenshell.file", schedule_guid: str, report: CostReport) -> LifecycleReceipt:
        import ifcopenshell.api.cost  # noqa: PLC0415

        schedule = model.by_guid(schedule_guid)
        items = model.by_type("IfcCostItem")
        for item in items:
            ifcopenshell.api.cost.calculate_cost_item_resource_value(model, cost_item=item)
        # The subject carries the closed `CostReport` token, not a leaky `Ifc5DCsvWriter` class name, so the
        # data boundary re-keys its `ifc5Dspreadsheet` writer table on it, never a throwaway temp-dir write.
        rows = tuple(
            LifecycleRow.of_cost(item.GlobalId, item.Name or "", float(getattr(value, "AppliedValue", 0.0) or 0.0))
            for item in items
            for value in (item.CostValues or ())
        )
        return LifecycleReceipt(LifecyclePhase.COST, (schedule.GlobalId, report.value), rows)

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
        import ifcopenshell  # noqa: PLC0415  boundary-scope: the `ifcopenshell.file()` output match needs the name bound
        import ifcpatch  # noqa: PLC0415

        # `execute` returns `ifcopenshell.file | str | None` — patched model, non-IFC product, or in-place.
        # The product TYPE is the wire carry the data boundary keys `ifcpatch.write` on; no throwaway write here.
        output = ifcpatch.execute({
            "input": "",
            "file": model,
            "recipe": recipe,
            "arguments": decode(args.encode(), type=list[object]) if args else [],
        })
        match output:
            case ifcopenshell.file():
                product = output.schema
            case None:
                product = "in-place"
            case _:
                product = type(output).__name__
        rows = (LifecycleRow.of_patch(recipe, product),)
        return LifecycleReceipt(LifecyclePhase.PATCH, (recipe, product), rows)

    @staticmethod
    def _diff(model: "ifcopenshell.file", revision_path: str) -> LifecycleReceipt:
        import ifcopenshell  # noqa: PLC0415  boundary-scope: the `ifcopenshell.open(revision_path)` revision load needs the name bound
        import ifcdiff  # noqa: PLC0415

        # `change_register` carries only the surviving-element marker map; the disjoint
        # `added_elements`/`deleted_elements` sets carry the presence rows the register never holds
        # — three result surfaces folded into one typed diff row stream.
        revision = ifcopenshell.open(revision_path)
        differ = ifcdiff.IfcDiff(model, revision, relationships=list(DIFF_AXIS))
        differ.diff()
        rows = (
            *(LifecycleRow.of_diff(guid, markers) for guid, markers in differ.change_register.items()),
            *(LifecycleRow.of_diff(guid, DiffChange.ADDED) for guid in differ.added_elements),
            *(LifecycleRow.of_diff(guid, DiffChange.DELETED) for guid in differ.deleted_elements),
        )
        subjects = (*differ.change_register, *differ.added_elements, *differ.deleted_elements)
        # `model` is the OLD revision, already holding survivors plus deleted, so the union adds only
        # the new-only `added_elements`: the drift denominator is `len(old IfcRoot) + len(added)`.
        population = len(model.by_type("IfcRoot")) + len(differ.added_elements)
        return LifecycleReceipt(LifecyclePhase.DIFF, subjects, rows, population=population)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
