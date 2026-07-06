# [PY_GEOMETRY_IFC_COSTING]

The 5D/4D model-lifecycle owner — the construction-economics and model-management verbs the analysis hop alone drops. `IfcLifecycle` runs rule-driven quantity take-off, cost-schedule rollup with structured report export, construction scheduling, recipe-driven model transformation, and two-model revision comparison over `ifc5d`, `ifc4d`, `ifcpatch`, and `ifcdiff` — the four `0.8.5` IfcOpenShell-ecosystem siblings over the `ifcopenshell` core — emitting a `LifecycleReceipt` whose rows are the typed `LifecycleRow` tagged union (never a stringly `dict[str, str]`), graduating as the geometry-minted `GeometrySubject.BIM_LIFECYCLE` member — the differentiated 5D/4D lifecycle evidence class, distinct from the compliance and section-integral members its siblings bind — `graduates()` returning the local `rasm.geometry.graduation` `GeometryHandoff` carrier whose `wire()` projection is the compute crossing. The cross-cutting concerns compose through the canonical owners, never inline re-wiring: `run` threads the graduation `evidence_run` weave (`EvidenceScope.IFC_LIFECYCLE` the seed row — span, fence, and receipt harvest composed once on the spine, no page-local tracer, no hand-authored `_ok`/`_emit` pair), whose flatten absorbs the rail-returning `_dispatch` so a provider exception converts on the live span exactly once and a selector/token fault arrives already typed on the same carrier. `@beartype(conf=FAULT_CONF)` on `_dispatch` raises the canonical `BeartypeCallHintViolation` the `CLASSIFY` `api` row folds onto the rail rather than an inline `is_bearable` tree. The `QUANTITY` phase is the rule-driven take-off that supersedes the hand-rolled single-`NetFloorArea` fold the sibling owner shed: `ifc5d.qto.quantify` computes the full base-quantity schema (length/area/volume/weight per IFC class) keyed by the `ifc5d.qto.rules` `RULE_SET` table and `ifc5d.qto.edit_qtos` writes the computed `ResultsDict` back into the model as `IfcElementQuantity`, so the quantity arm answers the whole schedule rather than one key. The element set the quantity arm measures is never a raw `filter_elements` passthrough: the selector text threads through the shared `geometry:ifc/selector.md#SELECTOR` validated entry gate — `IfcSelector.filter(model, selector)` returns a `RuntimeRail[tuple[entity_instance, ...]]` so a malformed selector is an `UnexpectedInput`-derived `BoundaryFault` at admission, the same gate the analysis quantity/pset arms thread their selector into, never a second selection engine. The C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the lifecycle dimension the managed projection does not produce, taking the validated selector through that shared gate and reaching the C# owner system through the one graduation rail.

## [01]-[INDEX]

- [01]-[LIFECYCLE]: the quantity, cost, schedule, patch, and diff lifecycle phases under one `LifecyclePhase`-discriminated owner folding the four IfcOpenShell ecosystem siblings.

## [02]-[LIFECYCLE]

- Owner: `IfcLifecycle` — the `@staticmethod` boundary capsule mirroring `IfcAnalysis`, dispatching the lifecycle phases over the `ifc5d`/`ifc4d`/`ifcpatch`/`ifcdiff` ecosystem through a thin `@beartype(conf=FAULT_CONF)`-fenced `_dispatch` fold over per-phase helpers (`_takeoff`/`_cost`/`_schedule`/`_patch`/`_diff`), never five fat in-arm bodies, the weave's harvest step emitting the structurally-conforming `LifecycleReceipt.contribute` stream on the cleared `Ok`; `LifecyclePhase` the closed `StrEnum` selecting the phase; `ScheduleFormat` the closed `StrEnum` selecting the `ifc4d` named parser so the schedule arm is one row over a closed parser vocabulary rather than a parse-per-format function family; `CostReport` the closed `StrEnum` naming the `ifc5Dspreadsheet` writer family (`Ifc5DCsvWriter`/`Ifc5DOdsWriter`/`Ifc5DXlsxWriter`) the data boundary re-keys on; `RuleSet` the closed `StrEnum` over the headless `qto.RULE_SET` base-quantity vocabulary (`IFC4QtoBaseQuantities`/`IFC4X3QtoBaseQuantities`) so the take-off rule-set is a rail-validated discriminant rather than a raw string fed to `rules[str]`; all three of the report, format, and rule-set tokens parse through the one generic `_token[E: StrEnum]` rail-returning fold so an unknown token is a typed `wire` `BoundaryFault` naming the offending value, never a raw `StrEnum(token)`/`rules[str]` that escapes a `ValueError`/`KeyError` past the fence; `DiffChange` the closed `StrEnum` classifying the `ifcdiff` per-element change (added/deleted/geometry/attribute/pset/relationship) so a revision-diff row carries the bounded change vocabulary, never a stringified `deepdiff` blob; `LifecycleRow` the `@tagged_union` result row carrying one typed case per phase (`quantity` element-GUID/qto-name/quantity-name/`float` measure, `cost` item-GUID/name/`float` applied value, `task` GUID/name, `patch` recipe/product, `diff` element-GUID/`DiffChange`) so the receipt rows are typed evidence the toolchain reads field-by-field, never a stringly `dict[str, str]`; `LifecycleReceipt` the typed receipt carrying the phase, the subject element set, the `tuple[LifecycleRow, ...]` typed result rows, and the kind-specific `evidence` residual ledger the graduation leg folds, conforming structurally to the runtime-checkable `ReceiptContributor` Protocol, its `contribute` yielding the `Iterable[Receipt]` stream the weave harvests.
- Cases: `LifecyclePhase` rows `QUANTITY` (rule-driven base-quantity take-off over `ifc5d.qto.quantify`/`edit_qtos` keyed by the `qto.rules` rule-set table, writing `IfcElementQuantity` and reading the derived quantity keys) · `COST` (`ifcopenshell.api.cost.calculate_cost_item_resource_value` rollup over each `IfcCostItem` carrying the closed `CostReport` token onto the subject for the `python:data/spatial` boundary to drive the `ifc5d.ifc5Dspreadsheet.Ifc5DCsvWriter`/`Ifc5DOdsWriter`/`Ifc5DXlsxWriter` export leg) · `SCHEDULE` (`ifc4d` `<Format>2Ifc` named-parser conversion populating `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence`) · `PATCH` (`ifcpatch.execute` named-recipe transformation over the `recipes` namespace, the recipe-determined output type — IFC schema, non-IFC product, or in-place — carried as evidence for the `python:data/spatial` boundary's durable `ifcpatch.write` sink) · `DIFF` (`ifcdiff.IfcDiff(old, new, relationships=DIFF_AXIS).diff()` revision comparison over `deepdiff`, reading the three disjoint result surfaces — the `change_register` survivor-marker map plus the `added_elements`/`deleted_elements` GUID sets the register never carries) — matched by `match`/`assert_never`, each dispatching to the ecosystem sibling that owns it. A new phase is one `LifecyclePhase` row plus one fold arm and breaks every dispatch site at type-check time under `ty`.
- Entry: `IfcLifecycle.run` takes an `ifcopenshell.file`, a `LifecyclePhase`, and a `spec` whose meaning is fixed by the phase — a validated selector query for `QUANTITY`, a cost-schedule GlobalId plus report token for `COST`, a `<format>:<path>` schedule source for `SCHEDULE`, a `<recipe>:<json-args>` patch directive for `PATCH`, a revision-model path for `DIFF` — and returns a `RuntimeRail[LifecycleReceipt]` by composing `evidence_run(EvidenceScope.IFC_LIFECYCLE, f"run.{phase}", lambda: IfcLifecycle._dispatch(model, phase, spec))` — the weave opens the seeded span, runs the fence inside it, flattens the rail-returning `_dispatch`, harvests the receipt stream on the cleared `Ok`, and closes OK once, so a provider exception converts to a `BoundaryFault` exactly once at the seam and a selector/token parse fault arrives already typed on the rail. The dispatch is one rail-returning fold mirroring the analysis sibling: `_dispatch` partitions the `spec` once on the per-phase `PHASE_DELIMITER` row — every phase is a table key including `DIFF` (the empty-delimiter row that passes the whole `spec` as the revision path without a `partition("")` `ValueError`), never a `.get` default that silently drops a phase — then each arm yields `RuntimeRail[LifecycleReceipt]`: the `QUANTITY` arm by `_token(RuleSet, tail or …).bind(rule_set -> IfcSelector.filter(model, head).map(_takeoff))` so BOTH the rule-set token and the validated selector compose monadically — a typo'd `#<rule-set>` is the same typed `wire` fault and a malformed selector the selector page's `UnexpectedInput`-derived `BoundaryFault`, both before `quantify` runs, the `COST`/`SCHEDULE` arms by `_token(CostReport, …)`/`_token(ScheduleFormat, head)` `.map`ping the parsed-token rail into their helper so an unknown report/format token is a typed `wire` fault rather than a raw `StrEnum` `ValueError`, the `PATCH`/`DIFF` arms lifting their helper-built receipt through `Ok`, never a second call shape or an out-of-fold short-circuit. The `#<rule-set>`/`<report>`/`<format>`/`<recipe>` token is the `tail` of the one partition, the selector/schedule-id/format/recipe the `head`, so the spec parse is one data-driven split keyed by the `PHASE_DELIMITER` table, never a partition-per-arm string ladder. Each arm derives its own `subjects` from the phase's true subject set: quantity element GlobalIds for `QUANTITY`, the schedule id plus the bound writer class for `COST`, task GlobalIds for `SCHEDULE`, the recipe plus the patched-output schema/product for `PATCH`, the changed-plus-added-plus-deleted GlobalId union for `DIFF` (the `population` field separately carrying the full compared element count the drift fraction divides against) — so the subject field never carries a meaningless run.
- Auto: the `QUANTITY` arm rail-validates the `#<rule-set>` token through `_token(RuleSet, tail or RuleSet.IFC4.value)` then `.bind`s the already-validated `elements` tuple off the `IfcSelector.filter` rail into the `_takeoff` fold (no in-arm `filter_elements` call), running `ifc5d.qto.quantify(model, set(elements), ifc5d.qto.rules[rule_set.value])` — the rule-driven measurement kernel computing the full base-quantity schedule per element keyed by the `qto.rules` `RULE_SET` table (the closed `RuleSet` `IFC4`/`IFC4X3` vocabulary the `spec`'s `#<rule-set>` suffix selects, defaulting to `RuleSet.IFC4`) — then `ifc5d.qto.edit_qtos(model, results)` writes the computed `ResultsDict` back as `IfcElementQuantity` base quantities, replacing the deleted `get_psets(qtos_only=True)`/`NetFloorArea` single-key fold, and folds the `ResultsDict` (`element -> qto-name -> quantity-name -> value`) directly into typed `LifecycleRow.of_quantity` rows carrying the element GUID, the qto name, the quantity name, and the `float` measure (never the `str(value)` coercion the old stringly fold lost the numeric type to); the `_cost` arm runs `ifcopenshell.api.cost.calculate_cost_item_resource_value(model, cost_item=item)` over each `IfcCostItem` to roll its resource values, carries the closed `CostReport` token (`report.value`, the rail-validated discriminant naming the `Ifc5DCsvWriter`/`Ifc5DOdsWriter`/`Ifc5DXlsxWriter` writer family) onto the receipt subject for the data boundary to re-key its own writer table on and drive against a durable path, and reads each `IfcCostItem.CostValues` `IfcCostValue.AppliedValue` back as typed `LifecycleRow.of_cost` rows carrying the item GUID, name, and the `float` applied value — the spreadsheet write is a columnar product the data boundary owns, never a throwaway `tempfile.TemporaryDirectory()` write the run discards and never a leaky implementation class name on the wire; the `_schedule` arm constructs the `ScheduleFormat`-selected `<Format>2Ifc` parser, sets its `.file`/`.xml`/`.work_plan` slots, and runs `.execute()` to populate the `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence` task tree, reading the task GlobalIds back as `LifecycleRow.of_task` rows; the `_patch` arm runs `ifcpatch.execute({"input", "file", "recipe", "arguments"})` dispatching the named recipe over the `recipes` namespace, then `match`es the `ifcopenshell.file | str | None` recipe-determined output to one `LifecycleRow.of_patch` row keyed by the patched-file schema, the non-IFC product type, or the `"in-place"` marker (the `None` return of a `PurgeData`-style in-place recipe), deferring the durable `ifcpatch.write` serialization to the data boundary rather than a throwaway run-local path the run discards; the `_diff` arm runs `ifcdiff.IfcDiff(model, revision, relationships=list(DIFF_AXIS)).diff()` over the full `RELATIONSHIP_TYPE` axis and folds the three disjoint result surfaces into typed `LifecycleRow.of_diff` rows: the `change_register` survivor-marker map through `DiffChange.of_register` (the bounded `geometry_changed`/`properties_changed`/`type_changed`/`container_changed`/`aggregate_changed`/`classification_changed` markers onto `GEOMETRY`/`PSET`/`RELATIONSHIP`, the named `attributes_changed` marker onto `ATTRIBUTE` by an explicit arm with `ATTRIBUTE` doubling as the closed-enum floor for a genuinely-unrecognized future marker), and the disjoint `added_elements`/`deleted_elements` GUID sets — which `change_register` never carries — as already-classified `DiffChange.ADDED`/`DELETED` presence rows, never `str(change)`, with `population = len(model.by_type("IfcRoot")) + len(added)` carried (the old-revision `model` already holds survivors plus deleted, so the GlobalId union adds only the new-only added set) so the drift fraction divides changed against the real compared population rather than the always-1.0 changed-against-changed degenerate. No phase carries an `if/else` value ladder and no phase mints a sibling per-phase class — one fold arm and one helper per row, the package that owns the phase bound directly, every row a typed `LifecycleRow` case.
- Receipt: `LifecycleReceipt` conforms structurally to the runtime-checkable `ReceiptContributor` Protocol; `contribute` yields the `Iterable[Receipt]` stream the weave's harvest step emits — never a single forced `Receipt` — minting the one emitted-phase row through the runtime two-arg `Receipt.of("rasm.geometry.ifc.costing", ("emitted", phase, facts))` contract, the `(phase, subject, facts)` evidence triple the factory discriminates, never a four-positional `Receipt.of("emitted", owner, subject, facts)` call. The facts are the flattened per-row `LifecycleRow.facts` projection keyed `f"{phase}.{i}.{field}"` (quantity keys for `QUANTITY`, item value for `COST`, task name for `SCHEDULE`, recipe and product for `PATCH`, the `DiffChange` value for `DIFF`) joined with the subject count and the resolved `evidence` ledger as native `int`/`float` scalars the runtime `EventDict` (`dict[str, object]`) carries through its `Encoder(enc_hook=repr, order="deterministic")` renderer — never the lossy `str(dict)`/`repr(v)` coercion the receipts owner's renderer is built to avoid. The run graduates through `LifecycleReceipt.graduates(evidence_key, ceiling)` (the canonical name the sibling producers share) over the kind-specific `LifecycleReceipt.evidence` residual ledger — the empty-row fraction for the `QUANTITY`/`COST`/`SCHEDULE`/`PATCH` arms (a phase that produces no rows for a non-empty subject set is a degenerate run keyed `1.0`) and the changed-over-population drift fraction for `DIFF` (the changed-plus-added-plus-deleted count over the full compared `population`, never changed-over-changed which clears every ceiling) — returning the local `GeometryHandoff.of(GeometrySubject.BIM_LIFECYCLE, evidence_key, self.evidence(), ceiling)` carrier against the caller-supplied per-key ceiling, so the lifecycle output crosses under the differentiated `bim-lifecycle` member (the section-integral and compliance owners bind their own members; algebra keeps `numerical-primitive`) with the carrier's residual-over-ceiling `admitted` verdict the gate and its `wire()` projection the compute crossing, never a bare `str` subject, never a row/subject count ledger that clears against any nonzero ceiling, and never an empty `measured={}`/`ceiling={}` no-op admission — take-off rows, cost reports, schedules, patched models, and revision diffs reach the C# owner system through the one graduation rail as typed lifecycle output the toolchain consumes directly. The typed `LifecycleRow` is the carry and the per-field facts are the lossless projection, never a stringified source of truth.
- Packages: `ifc5d` (`qto.rules` rule-set table/`qto.quantify`/`qto.edit_qtos` the quantity take-off surface only — the `ifc5Dspreadsheet.Ifc5DCsvWriter`/`Ifc5DOdsWriter`/`Ifc5DXlsxWriter` writer family the closed `CostReport` token names is the `python:data/spatial` boundary's, not imported here), `ifcopenshell` (`api.cost.calculate_cost_item_resource_value` cost rollup, `file`/`by_guid`/`by_type`/`open` over the in-process model only — selector filtering is the validated gate, never a direct `util.selector.filter_elements` call here), `ifc4d` (`MSProject2Ifc`/`P62Ifc`/`Asta2Ifc` named parsers, `.file`/`.xml`/`.work_plan` slots, `.execute()`), `ifcpatch` (`execute` over the `recipes` namespace returning the `ifcopenshell.file | str | None` product the arm classifies — the durable `write` sink is the data boundary's, not run here), `ifcdiff` (`IfcDiff`/`diff`/`change_register`/`added_elements`/`deleted_elements` — the `export` JSON sink is the data boundary's, never run here), geometry (`IfcSelector.filter` the shared validated selector entry gate from `ifc/selector.md#SELECTOR`, `evidence_run`/`EvidenceScope`/`GeometryHandoff`/`GeometrySubject` the graduation spine), `expression` (`tagged_union`/`tag`/`case` for `LifecycleRow`, `Ok`/`Error`/`Result.map`/`Result.bind` the rail-threading fold, `Map.of_seq` the `PHASE_DELIMITER` table), `beartype` (`@beartype(conf=FAULT_CONF)` on `_dispatch` binding the one shared domain conf so a contract violation folds through the `CLASSIFY` `api` row), runtime (`RuntimeRail`/`BoundaryFault` the rail and the typed `wire` token fault, `FAULT_CONF` the shared contract conf, `Receipt.of(owner, evidence)` the `Iterable[Receipt]` stream, `ContentKey` from `rasm.runtime.identity`); the columnar cost-spreadsheet write, the patched-model `ifcpatch.write` serialization, and the diff `export` JSON all defer to `python:data/spatial` as the typed token/product carried on the receipt, never a local file sink.
- Growth: a new quantity rule set is one `RuleSet` row over the upstream-authored `qto.rules` `RULE_SET` key, never a local measurement fold; a new cost report format is one `CostReport` row the data boundary binds to its `ifc5Dspreadsheet` writer subclass (the `Ifc5DXlsxWriter` row already admitted); a new schedule format is one `ScheduleFormat` row binding its `<Format>2Ifc` parser; a new model transformation is one `recipe` name in the `ifcpatch.execute` directive, never a `file.add`/`remove` loop; a new diff change classification is one `DiffChange` row plus one `of_register` match arm; zero new surface, no parallel per-phase class family.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy (projected in-process); no durable store; no Rhino/GH mutation; the `ifcopenshell.file` model is the only foreign object held, and the four ecosystem siblings import function-local under `# noqa: PLC0415` at boundary scope, never module-top, per the manifest import policy. A raw `util.selector.filter_elements` passthrough of the unvalidated `spec` selector (the deleted form — the selector text crosses the shared `IfcSelector.filter` validated gate so a malformed query is a `BoundaryFault` at admission, never a silent empty match feeding `quantify`), a hand-rolled `NetFloorArea` quantity fold, a per-IFC-class measurement function family where the `qto.rules` table carries the rule, a stringly `dict[str, str]` result row where the typed `LifecycleRow` union carries the phase-specific fields, a `str(change)` diff row where `DiffChange` classifies the `change_register` markers into the bounded geometry/attribute/pset/relationship vocabulary and the disjoint `added_elements`/`deleted_elements` sets supply the added/deleted presence rows, a `change_register`-only diff fold that drops the added/deleted GUID sets the register never carries, a changed-over-changed drift ledger that clears every nonzero ceiling where the `population`-divided fraction measures real model drift, a bare `str` graduation subject or a `"numerical-primitive"` literal carrying lifecycle evidence where `GeometrySubject.BIM_LIFECYCLE` is the differentiated member, a row/subject-count residual ledger that clears against any nonzero ceiling where the kind-specific `evidence` fold keys the empty-row or drift fraction, a bespoke cost-rollup loop where `ifcopenshell.api.cost.calculate_cost_item_resource_value` owns the per-item resource rollup, a throwaway `tempfile.TemporaryDirectory()` spreadsheet write or a run-local `ifcpatch.write(output, f"lifecycle.{recipe}")` the run discards where both the columnar cost export and the patched-model serialization defer to the data boundary as the product token/type carried on the receipt, a leaky `Ifc5DCsvWriter` implementation class name on the wire where the closed `CostReport.value` token is the carry the data boundary re-keys its writer table on, a hand-written P6/MS Project XML parser where the `<Format>2Ifc` parser owns the conversion, an ad-hoc `file.create_entity`/`add`/`remove` mutation loop where a recipe owns the transformation, a `by_type`/attribute-walk diff where `IfcDiff`/`deepdiff` own the comparison, a raw `StrEnum(token)` report/format construction that escapes a `ValueError` past the fence where the `_token` rail folds an unknown token to a typed `wire` `BoundaryFault`, a four-positional `Receipt.of("emitted", owner, subject, facts)` call against the runtime two-arg `of(owner, evidence)` contract, a `contribute` returning a single `Receipt` against the `Iterable[Receipt]` port, a `repr(v)`/`str(dict)`-coerced fact map where the `dict[str, object]` `EventDict` carries the native `float` measure and residual through its `enc_hook=repr` renderer, an inline `Signals.emit`, a page-local `trace.get_tracer` mint, or a hand-authored span/`_ok`/`_emit` weave where the graduation `evidence_run` owns span-fence-emit-ok, an inline `is_bearable` contract tree where `@beartype(conf=FAULT_CONF)` on `_dispatch` folds the violation through the `CLASSIFY` `api` row, and a compute-interior graduation binding or a `GraduationReceipt.graduates` call where the local `rasm.geometry.graduation` owner mints the vocabulary and `graduates()` returns the local `GeometryHandoff` are the deleted forms — the validated selector gate, the typed row union, the rule table, the named parser family, the recipe vocabulary, the bounded change classification, the structured diff, the rail-parsed token vocabulary, and the canonical runtime aspects compose the provider tools end-to-end.

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

if TYPE_CHECKING:  # worker: the model annotations resolve here; every runtime ifcopenshell use is a function-local `import ifcopenshell  # noqa: PLC0415` at boundary scope so the runtime module loads clean (the selector sibling shape)
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
    # The headless base-quantity rule sets the `ifc5d.qto.rules` table keys (the `qto.RULE_SET`
    # literal); the `*Blender` variants are Blender-host-only and never cross this non-Blender lane.
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
        # The `change_register[guid]` value is the bounded `*_changed` marker dict ifcdiff accumulates
        # over the surviving-element intersection — an element carries one or more flags at once, so the
        # arm order IS the change-priority collapse to one row: representation shape, then the pset deep-
        # diff, then the structural relationship markers, then the direct attribute compare. Every named
        # marker has its own arm; `attributes_changed` is matched by intent rather than swept by a default,
        # so the `_` fallback is the closed-enum floor for a genuinely-unrecognized record (a future ifcdiff
        # marker) absorbed as ATTRIBUTE without escaping the enum — distinct from a record the diff actually
        # flagged as an attribute change. Element presence (added/deleted) is NOT a register marker — it
        # rides the disjoint `added_elements`/`deleted_elements` GUID sets `_diff` classifies directly.
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
        # Native float measures ride the runtime EventDict (`dict[str, object]`) whose
        # `Encoder(enc_hook=repr, order="deterministic")` renderer serializes them without a
        # `str()`/`repr()` coerce — pre-stringifying the measure is the receipts-owner deleted form.
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

# The 5D/4D lifecycle output crosses on the geometry-minted BIM_LIFECYCLE member — the
# differentiated lifecycle evidence class; an unlisted subject fails at the boundary under `ty`.
LIFECYCLE_SUBJECT: Final[GeometrySubject] = GeometrySubject.BIM_LIFECYCLE

# The phase's spec carries the rule-set/report/format/recipe token after a single delimiter;
# the delimiter is the partition vocabulary keyed for every phase, never a parse-per-phase
# string ladder and never a silent `.get` default that drops a phase off the table. `DIFF`'s
# empty-delimiter row passes the whole spec as the revision path without a `partition("")` fault.
PHASE_DELIMITER: Map[LifecyclePhase, str] = Map.of_seq([
    (LifecyclePhase.QUANTITY, "#"),
    (LifecyclePhase.COST, ":"),
    (LifecyclePhase.SCHEDULE, ":"),
    (LifecyclePhase.PATCH, ":"),
    (LifecyclePhase.DIFF, ""),
])

# The full ifcdiff RELATIONSHIP_TYPE axis the lifecycle audit scopes over, not the ctor's
# `["geometry"]` default; the `"geometry"` leg drives the costly ifcopenshell.geom tessellation,
# the rest fold attribute/type/property/container/aggregate/classification markers off the model.
DIFF_AXIS: tuple[str, ...] = ("geometry", "attributes", "type", "property", "container", "aggregate", "classification")

# --- [MODELS] --------------------------------------------------------------------------


class LifecycleReceipt(Struct, frozen=True, gc=False):
    phase: LifecyclePhase
    subjects: tuple[str, ...]
    rows: tuple[LifecycleRow, ...]
    # The compared population the DIFF drift fraction divides against — the full surviving+added+
    # deleted element count, NOT the changed-subject count `subjects` carries (dividing changed by
    # changed is the always-1.0 ledger that clears any ceiling). The other phases ignore it.
    population: int = 0

    def evidence(self) -> dict[str, float]:
        # The residual ledger is phase-specific, never a row/subject count that clears against
        # any ceiling: a take-off/cost/schedule arm that produces no rows for a non-empty subject
        # set is a degenerate run keyed by the empty fraction, and a revision diff keys the
        # changed-element fraction against the full compared population so a model whose drift
        # exceeds the caller's ceiling is rejected.
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
        # Per-row typed facts flatten under the `{phase}.{i}.{field}` key joined with the subject
        # count and the native-float residual ledger; the runtime two-arg `Receipt.of(owner, evidence)`
        # contract discriminates the `(phase, subject, facts)` triple, never a four-positional call,
        # and the `dict[str, object]` EventDict carries the float measures and residuals natively.
        facts = {f"{self.phase}.{i}.{k}": v for i, row in enumerate(self.rows) for k, v in row.facts.items()}
        yield Receipt.of("rasm.geometry.ifc.costing", ("emitted", self.phase.value, facts | {"subjects": len(self.subjects)} | self.evidence()))


# --- [OPERATIONS] ----------------------------------------------------------------------


def _token[E: StrEnum](vocabulary: type[E], raw: str) -> "RuntimeRail[E]":
    # The one generic closed-vocabulary parse serving the report, format, AND rule-set tokens on the
    # rail, never a raw `StrEnum(raw)`/`rules[str]` that escapes a `ValueError`/`KeyError` past the
    # boundary fence: an unknown token is a typed `wire` fault carrying the offending value, the same
    # shape a malformed selector arrives as. The `raw in vocabulary` value-membership test is the
    # public 3.12+ EnumType contract, no private map.
    return Ok(vocabulary(raw)) if raw in vocabulary else Error(BoundaryFault(wire=(f"lifecycle.{vocabulary.__name__}.{raw}", 0)))


class IfcLifecycle:
    @staticmethod
    def run(model: "ifcopenshell.file", phase: LifecyclePhase, spec: str) -> "RuntimeRail[LifecycleReceipt]":
        # the graduation weave owns span-fence-emit-ok: the seeded IFC_LIFECYCLE span opens, the fence
        # runs INSIDE it (the faults `_convert` records a provider exception on the live span and sets
        # ERROR), the flatten absorbs the rail-returning `_dispatch` so a selector/token fault meets the
        # converted provider fault on one carrier, and the harvest emits the structurally-conforming
        # `LifecycleReceipt.contribute` stream on the cleared Ok — composed once on the spine, never
        # re-authored per page.
        return evidence_run(EvidenceScope.IFC_LIFECYCLE, f"run.{phase}", lambda: IfcLifecycle._dispatch(model, phase, spec))

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _dispatch(model: "ifcopenshell.file", phase: LifecyclePhase, spec: str) -> "RuntimeRail[LifecycleReceipt]":
        delimiter = PHASE_DELIMITER[phase]
        head, _, tail = spec.partition(delimiter) if delimiter else (spec, "", "")
        match phase:
            case LifecyclePhase.QUANTITY:
                # The selector rail AND the rule-set token both validate before `quantify`: a malformed
                # selector is the selector page's `UnexpectedInput`-derived fault, a typo'd `#<rule-set>`
                # is the same typed `wire` fault COST/SCHEDULE tokens get — never a raw `rules[str]`
                # `KeyError` degraded to a bare `boundary` fault that hides the offending rule-set name.
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
        # The structured spreadsheet export is the columnar 5D product `python:data/spatial` owns; this
        # owner reads the rolled IfcCostValue.AppliedValue measure back as typed rows and carries the
        # closed `CostReport` token (`report.value`) onto the receipt subject — not a leaky `Ifc5DCsvWriter`
        # implementation class name — so the data boundary re-keys its own `ifc5Dspreadsheet` writer table
        # on the canonical vocabulary and drives `.write()` against a durable path, never a throwaway
        # temp-dir file the run discards. The `report` token is the upstream `_token(CostReport, …)`
        # rail-validated discriminant, so the format is already a real writer key before this arm runs.
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

        # `execute` returns `ifcopenshell.file | str | None`: a patched model (its `.schema` is the
        # product), a non-IFC string/path product (an `Ifc2Sql`/CSV recipe), or `None` for an in-place
        # mutation recipe (`PurgeData`) whose `get_output()` returns nothing. The product TYPE is the wire
        # carry the data boundary keys its durable `ifcpatch.write` on — this owner runs no throwaway
        # `write(output, "lifecycle.{recipe}")` to a run-local path it discards (the same temp-sink anti-
        # pattern the COST arm sheds); the columnar/IFC serialization defers to `python:data/spatial`.
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

        # The full relationship axis is the lifecycle audit scope, not the ctor's `["geometry"]`
        # default; `change_register` carries only the surviving-element marker map, while the
        # disjoint `added_elements`/`deleted_elements` GUID sets carry the presence rows the
        # register never holds — three result surfaces folded into one typed diff row stream.
        revision = ifcopenshell.open(revision_path)
        differ = ifcdiff.IfcDiff(model, revision, relationships=list(DIFF_AXIS))
        differ.diff()
        rows = (
            *(LifecycleRow.of_diff(guid, markers) for guid, markers in differ.change_register.items()),
            *(LifecycleRow.of_diff(guid, DiffChange.ADDED) for guid in differ.added_elements),
            *(LifecycleRow.of_diff(guid, DiffChange.DELETED) for guid in differ.deleted_elements),
        )
        subjects = (*differ.change_register, *differ.added_elements, *differ.deleted_elements)
        # `model` is the OLD revision, so it already holds the survivors plus the deleted-from-old
        # set; the GlobalId union across both revisions adds only the new-only `added_elements`,
        # so the drift denominator is `len(old IfcRoot) + len(added)` — drift is changed/population.
        population = len(model.by_type("IfcRoot")) + len(differ.added_elements)
        return LifecycleReceipt(LifecyclePhase.DIFF, subjects, rows, population=population)
```
