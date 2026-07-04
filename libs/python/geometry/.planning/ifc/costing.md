# [PY_GEOMETRY_IFC_COSTING]

The 5D/4D model-lifecycle owner — the construction-economics and model-management verbs the analysis hop alone drops. `IfcLifecycle` runs rule-driven quantity take-off, cost-schedule rollup with structured report export, construction scheduling, recipe-driven model transformation, and two-model revision comparison over `ifc5d`, `ifc4d`, `ifcpatch`, and `ifcdiff` — the four `0.8.5` IfcOpenShell-ecosystem siblings over the `ifcopenshell` core — emitting a `LifecycleReceipt` whose rows are the typed `LifecycleRow` tagged union (never a stringly `dict[str, str]`), graduating through the compute `HandoffAxis` geometry case carrying the canonical `GeometrySubject` `"numerical-primitive"` literal alongside the analysis verbs in `geometry:ifc/analysis.md#ANALYSIS`. The cross-cutting concerns are the canonical runtime aspects, never inline re-wiring: `run` owns one `content.lifecycle` OTel span the `boundary(f"lifecycle.{phase}", ...)` fence runs INSIDE — the identity/graduation/structural/analysis span-owning shape — so the runtime `_convert` records a provider exception on a LIVE recording span and sets ERROR (a span-less `boundary` would no-op the `is_recording()` trace-egress and drop the fault from the trace), the fence converting the exception to a `BoundaryFault` exactly once and the `_ok` close-out setting the clean-exit OK status; this owner mints the one `content.lifecycle` `Tracer` exactly as the analysis sibling mints `content.analysis`, but re-wires no `structlog`/SDK chain — the trace-egress weave behind the fence stays the faults owner's. `@beartype(conf=FAULT_CONF)` on `_dispatch` raises the canonical `BeartypeCallHintViolation` the `CLASSIFY` `api` row folds onto the rail rather than an inline `is_bearable` tree, and the `@receipted(LIFECYCLE_REDACTION)` `_emit` step harvests the `ReceiptContributor` stream onto the egress sink as a decorator rail rather than an inline `Signals.emit` — exactly the AOP `geometry:ifc/authoring.md#AUTHORING` composes for its mutation receipt. The `QUANTITY` phase is the rule-driven take-off that supersedes the hand-rolled single-`NetFloorArea` fold the sibling owner shed: `ifc5d.qto.quantify` computes the full base-quantity schema (length/area/volume/weight per IFC class) keyed by the `ifc5d.qto.rules` `RULE_SET` table and `ifc5d.qto.edit_qtos` writes the computed `ResultsDict` back into the model as `IfcElementQuantity`, so the quantity arm answers the whole schedule rather than one key. The element set the quantity arm measures is never a raw `filter_elements` passthrough: the selector text threads through the shared `geometry:ifc/selector.md#SELECTOR` validated entry gate — `IfcSelector.filter(model, selector)` returns a `RuntimeRail[tuple[entity_instance, ...]]` so a malformed selector is an `UnexpectedInput`-derived `BoundaryFault` at admission, the same gate the analysis quantity/pset arms thread their selector into, never a second selection engine. The C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the lifecycle dimension the managed projection does not produce, taking the validated selector through that shared gate and reaching the C# owner system through the one graduation rail.

## [01]-[INDEX]

- [01]-[LIFECYCLE]: the quantity, cost, schedule, patch, and diff lifecycle phases under one `LifecyclePhase`-discriminated owner folding the four IfcOpenShell ecosystem siblings.

## [02]-[LIFECYCLE]

- Owner: `IfcLifecycle` — the `@staticmethod` boundary capsule mirroring `IfcAnalysis`, dispatching the lifecycle phases over the `ifc5d`/`ifc4d`/`ifcpatch`/`ifcdiff` ecosystem through a thin `@beartype(conf=FAULT_CONF)`-fenced `_dispatch` fold over per-phase helpers (`_takeoff`/`_cost`/`_schedule`/`_patch`/`_diff`), never five fat in-arm bodies, with the `@receipted(LIFECYCLE_REDACTION)` `_emit` step the egress aspect harvests on the cleared `Ok`; `LifecyclePhase` the closed `StrEnum` selecting the phase; `ScheduleFormat` the closed `StrEnum` selecting the `ifc4d` named parser so the schedule arm is one row over a closed parser vocabulary rather than a parse-per-format function family; `CostReport` the closed `StrEnum` naming the `ifc5Dspreadsheet` writer family (`Ifc5DCsvWriter`/`Ifc5DOdsWriter`/`Ifc5DXlsxWriter`) the data boundary re-keys on; `RuleSet` the closed `StrEnum` over the headless `qto.RULE_SET` base-quantity vocabulary (`IFC4QtoBaseQuantities`/`IFC4X3QtoBaseQuantities`) so the take-off rule-set is a rail-validated discriminant rather than a raw string fed to `rules[str]`; all three of the report, format, and rule-set tokens parse through the one generic `_token[E: StrEnum]` rail-returning fold so an unknown token is a typed `wire` `BoundaryFault` naming the offending value, never a raw `StrEnum(token)`/`rules[str]` that escapes a `ValueError`/`KeyError` past the fence; `DiffChange` the closed `StrEnum` classifying the `ifcdiff` per-element change (added/deleted/geometry/attribute/pset/relationship) so a revision-diff row carries the bounded change vocabulary, never a stringified `deepdiff` blob; `LifecycleRow` the `@tagged_union` result row carrying one typed case per phase (`quantity` element-GUID/qto-name/quantity-name/`float` measure, `cost` item-GUID/name/`float` applied value, `task` GUID/name, `patch` recipe/product, `diff` element-GUID/`DiffChange`) so the receipt rows are typed evidence the toolchain reads field-by-field, never a stringly `dict[str, str]`; `LifecycleReceipt` the typed receipt carrying the phase, the subject element set, the `tuple[LifecycleRow, ...]` typed result rows, and the kind-specific `evidence` residual ledger the graduation leg folds, itself the `ReceiptContributor` whose `contribute` yields the `Iterable[Receipt]` stream.
- Cases: `LifecyclePhase` rows `QUANTITY` (rule-driven base-quantity take-off over `ifc5d.qto.quantify`/`edit_qtos` keyed by the `qto.rules` rule-set table, writing `IfcElementQuantity` and reading the derived quantity keys) · `COST` (`ifcopenshell.api.cost.calculate_cost_item_resource_value` rollup over each `IfcCostItem` carrying the closed `CostReport` token onto the subject for the `python:data/spatial` boundary to drive the `ifc5d.ifc5Dspreadsheet.Ifc5DCsvWriter`/`Ifc5DOdsWriter`/`Ifc5DXlsxWriter` export leg) · `SCHEDULE` (`ifc4d` `<Format>2Ifc` named-parser conversion populating `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence`) · `PATCH` (`ifcpatch.execute` named-recipe transformation over the `recipes` namespace, the recipe-determined output type — IFC schema, non-IFC product, or in-place — carried as evidence for the `python:data/spatial` boundary's durable `ifcpatch.write` sink) · `DIFF` (`ifcdiff.IfcDiff(old, new, relationships=DIFF_AXIS).diff()` revision comparison over `deepdiff`, reading the three disjoint result surfaces — the `change_register` survivor-marker map plus the `added_elements`/`deleted_elements` GUID sets the register never carries) — matched by `match`/`assert_never`, each dispatching to the ecosystem sibling that owns it. A new phase is one `LifecyclePhase` row plus one fold arm and breaks every dispatch site at type-check time under `ty`.
- Entry: `IfcLifecycle.run` takes an `ifcopenshell.file`, a `LifecyclePhase`, and a `spec` whose meaning is fixed by the phase — a validated selector query for `QUANTITY`, a cost-schedule GlobalId plus report token for `COST`, a `<format>:<path>` schedule source for `SCHEDULE`, a `<recipe>:<json-args>` patch directive for `PATCH`, a revision-model path for `DIFF` — and returns a `RuntimeRail[LifecycleReceipt]` through one `boundary(f"lifecycle.{phase}", ...)` admission `.bind`-flattened over the rail-returning `_dispatch` and `.map`-threaded through the `@receipted` `_emit` harvest, so a provider exception converts to a `BoundaryFault` exactly once at the seam, a selector/token parse fault arrives already typed on the rail, and the receipt streams onto the egress sink only on the cleared `Ok`. The dispatch is one rail-returning fold mirroring the analysis sibling: `_dispatch` partitions the `spec` once on the per-phase `PHASE_DELIMITER` row — every phase is a table key including `DIFF` (the empty-delimiter row that passes the whole `spec` as the revision path without a `partition("")` `ValueError`), never a `.get` default that silently drops a phase — then each arm yields `RuntimeRail[LifecycleReceipt]`: the `QUANTITY` arm by `_token(RuleSet, tail or …).bind(rule_set -> IfcSelector.filter(model, head).map(_takeoff))` so BOTH the rule-set token and the validated selector compose monadically — a typo'd `#<rule-set>` is the same typed `wire` fault and a malformed selector the selector page's `UnexpectedInput`-derived `BoundaryFault`, both before `quantify` runs, the `COST`/`SCHEDULE` arms by `_token(CostReport, …)`/`_token(ScheduleFormat, head)` `.map`ping the parsed-token rail into their helper so an unknown report/format token is a typed `wire` fault rather than a raw `StrEnum` `ValueError`, the `PATCH`/`DIFF` arms lifting their helper-built receipt through `Ok`, never a second call shape or an out-of-fold short-circuit. The `#<rule-set>`/`<report>`/`<format>`/`<recipe>` token is the `tail` of the one partition, the selector/schedule-id/format/recipe the `head`, so the spec parse is one data-driven split keyed by the `PHASE_DELIMITER` table, never a partition-per-arm string ladder. Each arm derives its own `subjects` from the phase's true subject set: quantity element GlobalIds for `QUANTITY`, the schedule id plus the bound writer class for `COST`, task GlobalIds for `SCHEDULE`, the recipe plus the patched-output schema/product for `PATCH`, the changed-plus-added-plus-deleted GlobalId union for `DIFF` (the `population` field separately carrying the full compared element count the drift fraction divides against) — so the subject field never carries a meaningless run.
- Auto: the `QUANTITY` arm rail-validates the `#<rule-set>` token through `_token(RuleSet, tail or RuleSet.IFC4.value)` then `.bind`s the already-validated `elements` tuple off the `IfcSelector.filter` rail into the `_takeoff` fold (no in-arm `filter_elements` call), running `ifc5d.qto.quantify(model, set(elements), ifc5d.qto.rules[rule_set.value])` — the rule-driven measurement kernel computing the full base-quantity schedule per element keyed by the `qto.rules` `RULE_SET` table (the closed `RuleSet` `IFC4`/`IFC4X3` vocabulary the `spec`'s `#<rule-set>` suffix selects, defaulting to `RuleSet.IFC4`) — then `ifc5d.qto.edit_qtos(model, results)` writes the computed `ResultsDict` back as `IfcElementQuantity` base quantities, replacing the deleted `get_psets(qtos_only=True)`/`NetFloorArea` single-key fold, and folds the `ResultsDict` (`element -> qto-name -> quantity-name -> value`) directly into typed `LifecycleRow.of_quantity` rows carrying the element GUID, the qto name, the quantity name, and the `float` measure (never the `str(value)` coercion the old stringly fold lost the numeric type to); the `_cost` arm runs `ifcopenshell.api.cost.calculate_cost_item_resource_value(model, cost_item=item)` over each `IfcCostItem` to roll its resource values, carries the closed `CostReport` token (`report.value`, the rail-validated discriminant naming the `Ifc5DCsvWriter`/`Ifc5DOdsWriter`/`Ifc5DXlsxWriter` writer family) onto the receipt subject for the data boundary to re-key its own writer table on and drive against a durable path, and reads each `IfcCostItem.CostValues` `IfcCostValue.AppliedValue` back as typed `LifecycleRow.of_cost` rows carrying the item GUID, name, and the `float` applied value — the spreadsheet write is a columnar product the data boundary owns, never a throwaway `tempfile.TemporaryDirectory()` write the run discards and never a leaky implementation class name on the wire; the `_schedule` arm constructs the `ScheduleFormat`-selected `<Format>2Ifc` parser, sets its `.file`/`.xml`/`.work_plan` slots, and runs `.execute()` to populate the `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence` task tree, reading the task GlobalIds back as `LifecycleRow.of_task` rows; the `_patch` arm runs `ifcpatch.execute({"input", "file", "recipe", "arguments"})` dispatching the named recipe over the `recipes` namespace, then `match`es the `ifcopenshell.file | str | None` recipe-determined output to one `LifecycleRow.of_patch` row keyed by the patched-file schema, the non-IFC product type, or the `"in-place"` marker (the `None` return of a `PurgeData`-style in-place recipe), deferring the durable `ifcpatch.write` serialization to the data boundary rather than a throwaway run-local path the run discards; the `_diff` arm runs `ifcdiff.IfcDiff(model, revision, relationships=list(DIFF_AXIS)).diff()` over the full `RELATIONSHIP_TYPE` axis and folds the three disjoint result surfaces into typed `LifecycleRow.of_diff` rows: the `change_register` survivor-marker map through `DiffChange.of_register` (the bounded `geometry_changed`/`properties_changed`/`type_changed`/`container_changed`/`aggregate_changed`/`classification_changed` markers onto `GEOMETRY`/`PSET`/`RELATIONSHIP`, the named `attributes_changed` marker onto `ATTRIBUTE` by an explicit arm with `ATTRIBUTE` doubling as the closed-enum floor for a genuinely-unrecognized future marker), and the disjoint `added_elements`/`deleted_elements` GUID sets — which `change_register` never carries — as already-classified `DiffChange.ADDED`/`DELETED` presence rows, never `str(change)`, with `population = len(model.by_type("IfcRoot")) + len(added)` carried (the old-revision `model` already holds survivors plus deleted, so the GlobalId union adds only the new-only added set) so the drift fraction divides changed against the real compared population rather than the always-1.0 changed-against-changed degenerate. No phase carries an `if/else` value ladder and no phase mints a sibling per-phase class — one fold arm and one helper per row, the package that owns the phase bound directly, every row a typed `LifecycleRow` case.
- Receipt: `LifecycleReceipt` is the `ReceiptContributor` whose `contribute` yields the `Iterable[Receipt]` stream the `@receipted` aspect harvests — never a single forced `Receipt` — minting the one emitted-phase row through the runtime two-arg `Receipt.of("rasm.geometry.ifc.costing", ("emitted", phase, facts))` contract, the `(phase, subject, facts)` evidence triple the factory discriminates, never a four-positional `Receipt.of("emitted", owner, subject, facts)` call. The facts are the flattened per-row `LifecycleRow.facts` projection keyed `f"{phase}.{i}.{field}"` (quantity keys for `QUANTITY`, item value for `COST`, task name for `SCHEDULE`, recipe and product for `PATCH`, the `DiffChange` value for `DIFF`) joined with the subject count and the resolved `evidence` ledger as native `int`/`float` scalars the runtime `EventDict` (`dict[str, object]`) carries through its `Encoder(enc_hook=repr, order="deterministic")` renderer — never the lossy `str(dict)`/`repr(v)` coercion the receipts owner's renderer is built to avoid. The run graduates through `LifecycleReceipt.graduates(evidence_key, ceiling)` (the canonical name `GraduationReceipt.graduates`/`AnalysisResult.graduates`/`SectionReceipt.graduates` share) over the kind-specific `LifecycleReceipt.evidence` residual ledger — the empty-row fraction for the `QUANTITY`/`COST`/`SCHEDULE`/`PATCH` arms (a phase that produces no rows for a non-empty subject set is a degenerate run keyed `1.0`) and the changed-over-population drift fraction for `DIFF` (the changed-plus-added-plus-deleted count over the full compared `population`, never changed-over-changed which clears every ceiling) — folded through one `GraduationReceipt.graduates("rasm.geometry.ifc.costing", HandoffAxis(geometry=LIFECYCLE_SUBJECT), evidence_key, self.evidence(), ceiling)` admission against the caller-supplied per-key ceiling, so the lifecycle output crosses the one geometry graduation rail under the canonical `GeometrySubject` `"numerical-primitive"` literal the compute `graph/handoff` producer owns (the same subject the `ifc/structural` section-integral and `ifc/analysis` owners cross on, the source-package fixed not threaded), never a bare `str` subject, never a row/subject count ledger that clears against any nonzero ceiling, and never an empty `measured={}`/`ceiling={}` no-op admission — take-off rows, cost reports, schedules, patched models, and revision diffs reach the C# owner system through the one graduation rail as typed lifecycle output the toolchain consumes directly. The typed `LifecycleRow` is the carry and the per-field facts are the lossless projection, never a stringified source of truth.
- Packages: `ifc5d` (`qto.rules` rule-set table/`qto.quantify`/`qto.edit_qtos` the quantity take-off surface only — the `ifc5Dspreadsheet.Ifc5DCsvWriter`/`Ifc5DOdsWriter`/`Ifc5DXlsxWriter` writer family the closed `CostReport` token names is the `python:data/spatial` boundary's, not imported here), `ifcopenshell` (`api.cost.calculate_cost_item_resource_value` cost rollup, `file`/`by_guid`/`by_type`/`open` over the in-process model only — selector filtering is the validated gate, never a direct `util.selector.filter_elements` call here), `ifc4d` (`MSProject2Ifc`/`P62Ifc`/`Asta2Ifc` named parsers, `.file`/`.xml`/`.work_plan` slots, `.execute()`), `ifcpatch` (`execute` over the `recipes` namespace returning the `ifcopenshell.file | str | None` product the arm classifies — the durable `write` sink is the data boundary's, not run here), `ifcdiff` (`IfcDiff`/`diff`/`change_register`/`added_elements`/`deleted_elements` — the `export` JSON sink is the data boundary's, never run here), geometry (`IfcSelector.filter` the shared validated selector entry gate from `ifc/selector.md#SELECTOR`, `GraduationReceipt.graduates`/`HandoffAxis`/`GeometrySubject` the compute graduation rail), `expression` (`tagged_union`/`tag`/`case` for `LifecycleRow`, `Ok`/`Error`/`Result.map`/`Result.bind` the rail-threading fold, `Map.of_seq`/`Map.__contains__`/`Map.empty` for the `PHASE_DELIMITER` and `LIFECYCLE_REDACTION` tables), `beartype` (`@beartype(conf=FAULT_CONF)` on `_dispatch` binding the one shared domain conf so a contract violation folds through the `CLASSIFY` `api` row), runtime (`RuntimeRail`/`boundary`/`BoundaryFault` the rail and the typed `wire` token fault, `FAULT_CONF` the shared contract conf, `Receipt.of(owner, evidence)`/`ReceiptContributor.contribute` the `Iterable[Receipt]` stream, `Redaction`/`@receipted` the egress aspect the `_emit` step rides); the columnar cost-spreadsheet write, the patched-model `ifcpatch.write` serialization, and the diff `export` JSON all defer to `python:data/spatial` as the typed token/product carried on the receipt, never a local file sink. `opentelemetry-api` (`trace.get_tracer`/`start_as_current_span`/`Span.is_recording`/`Span.set_attributes`/`Span.set_status`/`Status`/`StatusCode`) for the one self-owned `content.lifecycle` span the fence runs inside, mirroring the analysis sibling — the runtime faults owner owns the trace-egress WEAVE behind `boundary` (the `_convert` `record_exception`/ERROR), so this owner mints the one `content.lifecycle` tracer but re-wires no `structlog`/SDK chain and no `stamina` inline.
- Growth: a new quantity rule set is one `RuleSet` row over the upstream-authored `qto.rules` `RULE_SET` key, never a local measurement fold; a new cost report format is one `CostReport` row the data boundary binds to its `ifc5Dspreadsheet` writer subclass (the `Ifc5DXlsxWriter` row already admitted); a new schedule format is one `ScheduleFormat` row binding its `<Format>2Ifc` parser; a new model transformation is one `recipe` name in the `ifcpatch.execute` directive, never a `file.add`/`remove` loop; a new diff change classification is one `DiffChange` row plus one `of_register` match arm; zero new surface, no parallel per-phase class family.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy (projected in-process); no durable store; no Rhino/GH mutation; the `ifcopenshell.file` model is the only foreign object held, and the four ecosystem siblings import function-local under `# noqa: PLC0415` at boundary scope, never module-top, per the manifest import policy. A raw `util.selector.filter_elements` passthrough of the unvalidated `spec` selector (the deleted form — the selector text crosses the shared `IfcSelector.filter` validated gate so a malformed query is a `BoundaryFault` at admission, never a silent empty match feeding `quantify`), a hand-rolled `NetFloorArea` quantity fold, a per-IFC-class measurement function family where the `qto.rules` table carries the rule, a stringly `dict[str, str]` result row where the typed `LifecycleRow` union carries the phase-specific fields, a `str(change)` diff row where `DiffChange` classifies the `change_register` markers into the bounded geometry/attribute/pset/relationship vocabulary and the disjoint `added_elements`/`deleted_elements` sets supply the added/deleted presence rows, a `change_register`-only diff fold that drops the added/deleted GUID sets the register never carries, a changed-over-changed drift ledger that clears every nonzero ceiling where the `population`-divided fraction measures real model drift, a bare `str` graduation subject where the canonical `GeometrySubject` `"numerical-primitive"` literal is the wire vocabulary, a row/subject-count residual ledger that clears against any nonzero ceiling where the kind-specific `evidence` fold keys the empty-row or drift fraction, a bespoke cost-rollup loop where `ifcopenshell.api.cost.calculate_cost_item_resource_value` owns the per-item resource rollup, a throwaway `tempfile.TemporaryDirectory()` spreadsheet write or a run-local `ifcpatch.write(output, f"lifecycle.{recipe}")` the run discards where both the columnar cost export and the patched-model serialization defer to the data boundary as the product token/type carried on the receipt, a leaky `Ifc5DCsvWriter` implementation class name on the wire where the closed `CostReport.value` token is the carry the data boundary re-keys its writer table on, a hand-written P6/MS Project XML parser where the `<Format>2Ifc` parser owns the conversion, an ad-hoc `file.create_entity`/`add`/`remove` mutation loop where a recipe owns the transformation, a `by_type`/attribute-walk diff where `IfcDiff`/`deepdiff` own the comparison, a raw `StrEnum(token)` report/format construction that escapes a `ValueError` past the fence where the `_token` rail folds an unknown token to a typed `wire` `BoundaryFault`, a four-positional `Receipt.of("emitted", owner, subject, facts)` call against the runtime two-arg `of(owner, evidence)` contract, a `contribute` returning a single `Receipt` against the `Iterable[Receipt]` port, a `repr(v)`/`str(dict)`-coerced fact map where the `dict[str, object]` `EventDict` carries the native `float` measure and residual through its `enc_hook=repr` renderer, an inline `Signals.emit` threaded through the run body where the `@receipted(LIFECYCLE_REDACTION)` `_emit` aspect owns egress, an inline `is_bearable` contract tree where `@beartype(conf=FAULT_CONF)` on `_dispatch` folds the violation through the `CLASSIFY` `api` row, and a locally minted `Tracer`/`Span` or re-wired `structlog`/`opentelemetry`/`stamina` where the runtime `boundary` fence owns the trace-egress weave and resilience is the `reliability/resilience` owner's rail are the deleted forms — the validated selector gate, the typed row union, the rule table, the named parser family, the recipe vocabulary, the bounded change classification, the structured diff, the rail-parsed token vocabulary, and the canonical runtime aspects compose the provider tools end-to-end.

```python signature
from collections.abc import Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from beartype import beartype
from expression import Error, Ok, case, identity, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from msgspec.json import decode
from opentelemetry import trace
from opentelemetry.trace import Span, Status, StatusCode

from rasm.compute.graduation.handoff import GeometrySubject, GraduationReceipt, HandoffAxis
from rasm.geometry.ifc.selector import IfcSelector
from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, ReceiptContributor, Redaction, receipted

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

# The 5D/4D lifecycle output crosses the geometry graduation case as a numerical primitive,
# the same GeometrySubject literal the section-integral and analysis owners cross on.
LIFECYCLE_SUBJECT: GeometrySubject = "numerical-primitive"

# The lifecycle facts carry no secret field, so the @receipted egress aspect rides the keep-all
# Redaction the runtime owner exposes — the same empty-table policy the graduation owner binds.
LIFECYCLE_REDACTION: Redaction = Redaction(classified=Map.empty())

# The one self-owned span the `boundary` fence runs INSIDE so the faults `_convert` records a provider
# exception on a LIVE recording span (the identity/graduation/structural/analysis span-owning shape); a
# span-less `boundary` would no-op the `is_recording()` trace-egress and drop the fault from the trace.
# This owner mints the `content.lifecycle` tracer like the analysis sibling mints `content.analysis`, but
# re-wires no `structlog`/SDK chain — the trace-egress weave behind the fence stays the faults owner's.
_TRACER: Final[trace.Tracer] = trace.get_tracer("geometry.ifc.costing")

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

    def graduates(self, evidence_key: ContentKey, ceiling: dict[str, float]) -> "RuntimeRail[GraduationReceipt]":
        return GraduationReceipt.graduates(
            "rasm.geometry.ifc.costing", HandoffAxis(geometry=LIFECYCLE_SUBJECT), evidence_key, self.evidence(), ceiling
        )

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
        # One woven rail INSIDE one self-owned `content.lifecycle` span — the identity/graduation/
        # structural/analysis span-owning shape: the `boundary` fence runs the rail-returning `_dispatch`
        # ON the live recording span so the runtime `_convert` records a provider exception on it and sets
        # ERROR (a bare span-less `boundary` would no-op the `is_recording()` egress and drop the fault from
        # the trace), the identity `.bind` flattens the rail-returning `_dispatch`, the @receipted `_emit`
        # harvests the contributor stream onto `Signals.emit` on the cleared `Ok` only, and the `_ok` `.map`
        # close-out sets `Status(StatusCode.OK)` once on the clean exit the conversion never re-annotates —
        # receipt emission a decorator rail, never an inline `Signals.emit` in the body.
        with _TRACER.start_as_current_span("content.lifecycle") as span:
            if span.is_recording():
                span.set_attributes({"phase": phase.value, "subject": LIFECYCLE_SUBJECT})
            return (
                boundary(f"lifecycle.{phase}", lambda: IfcLifecycle._dispatch(model, phase, spec))
                .bind(identity)
                .map(IfcLifecycle._emit)
                .map(lambda receipt: IfcLifecycle._ok(span, receipt))
            )

    @staticmethod
    def _ok(span: "Span", receipt: LifecycleReceipt) -> LifecycleReceipt:
        # the clean-exit close-out: the measured operation owns the OK status on its own span, the ERROR
        # side staying the fence `_convert`'s — the analysis `content.analysis`/structural `content.section`
        # `_ok` shape.
        span.set_status(Status(StatusCode.OK))
        return receipt

    @staticmethod
    @receipted(LIFECYCLE_REDACTION)
    def _emit(receipt: LifecycleReceipt) -> LifecycleReceipt:
        return receipt

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

## [03]-[RESEARCH]

- [QTO_RULE_SET]: the branch `ifc5d` catalogue confirms `qto.rules` (the `dict[RULE_SET, ...]` base-quantity rule table loaded from the bundled `IFC4QtoBaseQuantities`/`IFC4X3QtoBaseQuantities` JSON, keyed by the `RULE_SET` literal, `ifc5d.md#23`/`47`), `qto.quantify(ifc_file, elements, rules) -> ResultsDict` (the rule-driven measurement kernel, `#24`/`46`), and `qto.edit_qtos(ifc_file, results) -> None` (write the `ResultsDict` back as `IfcElementQuantity` base quantities, `#25`/`48`). The `quantify` `elements: set[ifcopenshell.entity_instance]` set form (the fence wraps the `IfcSelector.filter`-validated `elements` tuple in `set(...)`) and the `ResultsDict = dict[element, dict[qto-name, dict[quantity-name, float]]]` nesting the row fold reads (each leaf `float` flowing into the typed `LifecycleRow.of_quantity` measure field) confirm by introspection against the installed companion distribution. The `IFC4` versus `IFC4X3` rule-set choice the `spec`'s `#<rule-set>` suffix selects is the closed `RuleSet` `StrEnum` (the two headless `qto.RULE_SET` members; the `*Blender` variants are Blender-host-only and excluded from this lane), rail-validated through `_token(RuleSet, …)` exactly as the `CostReport`/`ScheduleFormat` tokens are so a typo'd suffix is a typed `wire` fault naming the value rather than a raw `rules[str]` `KeyError`, and defaults to `RuleSet.IFC4`; the live-run residual is confirming the exact `RULE_SET` literal set bundled in the `0.8.5` distribution still matches the two headless `RuleSet` members (`ifc5d.md#71`).
- [COST_ROLLUP_EXPORT]: the cost rollup is `ifcopenshell.api.cost.calculate_cost_item_resource_value(file, cost_item)` per `IfcCostItem` (summing the construction-resource base costs into the item's resource value, an `ifcopenshell.api.cost` function, NOT an `ifc5d.cost` member — `ifc5d` carries no `cost` module), and the structured export is the `ifc5d.ifc5Dspreadsheet.Ifc5DCsvWriter`/`Ifc5DOdsWriter`/`Ifc5DXlsxWriter` writer family, each constructed `(file, output_dir, cost_schedule=None)` and run through `.write()` (writing the spreadsheet to `output_dir`, no return rows), confirmed against the installed companion distribution (`ifc5d.md#49`/`50`/`51`). The `_cost` arm carries the closed `CostReport` token (`report.value`) onto the receipt subject for the data spatial-codec boundary to re-key its own writer table on and drive against a durable path rather than constructing-and-running the writer over a throwaway `tempfile.TemporaryDirectory()` the run discards: the spreadsheet is a columnar product `python:data/spatial` owns, the writer class spelling stays the data boundary's so this owner imports no `ifc5Dspreadsheet` and leaks no `Ifc5DCsvWriter` class name on the wire, and this owner's wire carry is the typed `of_cost` rows the `IfcCostItem.CostValues` `IfcCostValue.AppliedValue` measure feeds (the value-select union `float(getattr(value, "AppliedValue", 0.0) or 0.0)` coerces into the `LifecycleRow.of_cost` `float` field, the `None`/measure-select fallback the `or 0.0` guards), the live-run-confirmed member before the typed row construction binds. The `report` token is the upstream `_token(CostReport, …)` rail-validated discriminant, so a phantom format is a typed `wire` fault at the dispatch seam and the data boundary always receives a real writer key. The typed `float` carry replaces the stringly `str(...)` coercion the old `dict[str, str]` row lost the numeric type to; the durable spreadsheet path and the writer's exact constructor binding stay the data boundary's runtime-action residual, not a fence concern here.
- [SCHEDULE_SLOT_POPULATION]: the branch `ifc4d` catalogue confirms the `MSProject2Ifc`/`P62Ifc`/`Asta2Ifc` named-parser family (`ifc4d.md#23`-`25`), the `.execute()` entry (`#37`-`39`), the `.file`/`.xml`/`.work_plan` slots (`#45`), and the populated `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence` set — the schedule format is the named parser class, a closed parser set, never a parse-per-format function family, so the `SCHEDULE` arm is fully fenced. The narrow residual is the live-run `.file`/`.xml`/`.work_plan` slot-population semantics: whether `.xml` accepts the source path string or a parsed document, and whether `.work_plan` requires a pre-existing `IfcWorkPlan` or the parser mints one — the companion-interpreter `<Format>2Ifc.execute()` run confirms the slot contract, a runtime-action band step, not a fence blocker.
- [PATCH_OUTPUT_DISPATCH]: the branch `ifcpatch` catalogue confirms `ifcpatch.execute(args: ArgumentsDict)` (single-arg named-recipe dispatch constructing the recipe's `BasePatcher` subclass — NOT a `Patcher` base — `ifcpatch.md#41`/`#53`), the `recipes` namespace recipe names (`#30`), and `ifcpatch.write(output, filepath)` (the polymorphic sink discriminated on `isinstance(output, ifcopenshell.file)`, `#42`/`#54`); the `execute` return is `ifcopenshell.file | str | None` (`#41`) — a patched model, a recipe's non-IFC string/path product, or `None` for an in-place mutation recipe whose `get_output()` returns nothing (`#24`) — which the `_patch` arm folds through one total `match`/`case` over those three shapes, projecting the patched-file `.schema`, the non-IFC product type, or the `"in-place"` marker as the `of_patch` product and the receipt subject, source-confirmed against the IfcOpenShell `0.8.x` `src/ifcpatch` (`#60`/`#73`). The durable `ifcpatch.write(output, path)` sink (`#42`/`#54`) is the `python:data/spatial` boundary's against a real path, never a throwaway `f"lifecycle.{recipe}"` run-local write this owner discards — the same temp-sink anti-pattern the COST arm sheds (`ifcpatch.md#62`); this owner carries the product type as the wire evidence, not the serialized bytes. The `arguments` list decodes through `decode(args.encode(), type=list[object])` — the parameterized `list[object]` element shape, never a bare untyped `list`.
- [DIFF_CHANGE_REGISTER]: the branch `ifcdiff` catalogue confirms `IfcDiff(old, new, relationships=None, is_shallow=True, filter_elements=None)` (the two-model constructor whose `relationships` defaults to `["geometry"]`, `ifcdiff.md#39`), `IfcDiff.diff()` (`#40`), and the THREE disjoint result surfaces a consumer reads — the GlobalId-keyed `change_register` survivor-marker map (`#23`/`#25`), the disjoint `added_elements`/`deleted_elements` GUID sets (`#26`/`#27`), and the `export(path)` JSON sink (`#41`) — never one conflated map. The `change_register[guid]` value is the bounded `*_changed` marker dict the catalogue's `[CHANGE_MARKER_KEYS]` row spells exactly: `geometry_changed`/`type_changed`/`container_changed`/`aggregate_changed`/`classification_changed` boolean flags plus `properties_changed` carrying the full `DeepDiff` pset result (a dict, or `True` on a comparison exception), one element accumulating multiple markers. `DiffChange.of_register` folds those markers onto `GEOMETRY` (`geometry_changed`), `PSET` (`properties_changed`), `RELATIONSHIP` (`type`/`container`/`aggregate`/`classification` markers), and `ATTRIBUTE` (the named `attributes_changed` marker `diff_element` writes, `ifcdiff.md#30`/`#42`/`#60`) — each named marker matched by an explicit arm so the real attribute change is classified by intent, with the `_` fallback also resolving to `ATTRIBUTE` as the closed-enum floor so a genuinely-unrecognized future marker never escapes the enum yet stays distinct in intent from the flagged attribute change. The `ADDED`/`DELETED` rows are NOT register markers — the `_diff` arm reads them off the disjoint `added_elements`/`deleted_elements` sets as already-classified `DiffChange.ADDED`/`DELETED` presence rows the one `of_diff` constructor lifts without re-folding (`#60`). The `lifecycle` audit scopes the full `RELATIONSHIP_TYPE` axis (`DIFF_AXIS`) rather than the ctor's `["geometry"]` default, so the attribute/property/type/container/aggregate/classification legs run; the drift evidence divides the changed count by the full `population = len(model.by_type("IfcRoot")) + len(added)` (the old-revision `model` already counting survivors plus deleted, the added set new-only, per the catalogue's `added`/`deleted` set semantics at `#26`/`#27`), the changed-over-changed ledger the always-1.0 deleted form (`#62`).
- [VALIDATED_SELECTOR_GATE]: the `QUANTITY` arm threads its selector text (the `head` of the one `PHASE_DELIMITER`-keyed partition) through `IfcSelector.filter(model, head)` from `geometry:ifc/selector.md#SELECTOR` — the shared validated entry gate the analysis quantity/pset arms also consume — returning `RuntimeRail[tuple[ifcopenshell.entity_instance, ...]]`, so the arm `.map`s the selector rail into the `_takeoff` fold and a malformed selector surfaces as the selector page's `UnexpectedInput`-derived `BoundaryFault` at admission rather than as a silent empty `filter_elements` match feeding `quantify`. The whole verb is one rail-returning `_dispatch` fold flattened through the outer `boundary(...).bind(identity)` (the `expression`-exported `identity` rail-flatten the structural/authoring siblings hold, never a `lambda rail: rail`), exactly the `geometry:ifc/analysis.md#ANALYSIS` composition: the `QUANTITY` arm `.bind`s the rail-validated `RuleSet` token into the selector rail mapped to a receipt, the `COST`/`SCHEDULE` arms `.map` their `_token` rail, the `PATCH`/`DIFF` arms lift their receipt through `Ok`, so a provider exception is a `BoundaryFault` at the seam and a selector or token fault is already typed on the rail, with no second call shape or out-of-fold short-circuit. The `#<rule-set>` token is the `tail` of the partition (rail-parsed through `_token(RuleSet, …)`), so the validated query the gate parses is the `head` and never carries the rule-set suffix. The `.map` rail composition is the `expression` `Result.map`, `Ok` the `Result` constructor, and the outer `.bind(identity)` the `Result.bind` rail-flatten confirmed at `.api/expression.md#97`/`#140` (the `expression`-exported `identity`, never a `lambda rail: rail`), the same monadic surface the selector page's `.map` leg uses. The `geometry:ifc/selector.md#SELECTOR` gate is no longer silent: `SelectorQuery` is itself the `ReceiptContributor` and the page's `@receipted(_REDACTION)` `_emit` leg emits an `emitted`-phase `SelectorQuery.contribute` fact (the validated `filter_string`, the parsed axes, the facet-group count) on the `Ok` exit of every `parse`, so the `QUANTITY` arm's parse-once admission is observable on the shared receipt chain — the same admission fact the analysis quantity/pset arms see through the one selector gate, never an unobserved filter.
- [TYPED_LIFECYCLE_ROW]: the result rows are the `expression` `@tagged_union` `LifecycleRow` (`tagged_union`/`tag`/`case` confirmed at `.api/expression.md#129`/`#140`), one typed case per phase — `quantity` carrying `(element-GUID, qto-name, quantity-name, float)`, `cost` carrying `(item-GUID, name, float)`, `task`/`patch` carrying their `(str, str)` pairs, and `diff` carrying `(element-GUID, DiffChange)` so the change classification is the closed enum not a stringified blob — replacing the stringly `tuple[dict[str, str], ...]` rows; the `facts` property is the one total `match`/`assert_never` fold projecting each case to its `dict[str, object]` carrying the native `float` measure (the `diff` arm rendering the `DiffChange.value`), flattened per-row by `contribute` into the `Receipt.of` facts map under the `f"{phase}.{i}.{field}"` key so each typed field — including the numeric measure — stays an addressable native fact the runtime `EventDict` (`dict[str, object]`) carries through its `Encoder(enc_hook=repr, order="deterministic")` renderer rather than collapsing into one `str(dict)` blob or a `repr`-coerced scalar, the typed row the carry and the per-field projection lossless to the field grain. The graduation subject is the `LIFECYCLE_SUBJECT` module constant binding the canonical `GeometrySubject` `"numerical-primitive"` literal from `compute:graduation/handoff.md#HANDOFF` (the `HandoffAxis(geometry=...)` case, the same subject the `ifc/structural` section-integral and `ifc/analysis` owners cross on), passed to `GraduationReceipt.graduates` with the kind-specific `LifecycleReceipt.evidence` residual ledger — the empty-row fraction for the `QUANTITY`/`COST`/`SCHEDULE`/`PATCH` arms and the changed-over-`population` drift fraction for `DIFF`, mirroring the analysis sibling's `AnalysisResult.evidence` discipline — against the caller's ceiling, so an unlisted literal fails at the type boundary the handoff producer owns and a degenerate or over-drifting run is an `Error(BoundaryFault)` rather than a graduated handoff — never a bare `str` subject, never a row/subject-count ledger that clears against any nonzero ceiling (the `DIFF` drift divides the changed count by the full compared population, not the changed-subject count), and never a `measured={}` graduation that clears trivially.

- [RUNTIME_ASPECT_STACK]: the cross-cutting concerns compose through the canonical runtime owners, never inline re-wiring that would trample them. `run` owns the one `content.lifecycle` OTel span the `boundary` fence runs INSIDE (`trace.get_tracer`/`start_as_current_span`, the bounded `phase`/`subject` attributes behind the `is_recording()` gate, `Status(StatusCode.OK)` on the `Ok` arm through the `_ok` close-out) so the `boundary` (`reliability/faults#FAULT`) `_convert` records the caught exception on the LIVE recording span (`record_exception` plus `Status(StatusCode.ERROR)`) and folds it through the `CLASSIFY` table — a bare span-less `boundary` would no-op the `is_recording()` egress and drop the fault from the trace, the identity/graduation/structural/analysis span-owning shape avoiding it; this owner mints the one `content.lifecycle` `Tracer` exactly as the analysis sibling mints `content.analysis`, but re-wires no `structlog`/SDK chain — the trace-egress weave is the faults owner's, the receipt-egress chain the `observability/receipts#RECEIPT` owner's. `@beartype(conf=FAULT_CONF)` on `_dispatch` binds the one shared `BeartypeConf(violation_type=BeartypeCallHintViolation)` (`faults.md` `FAULT_CONF`) so a contract violation raises the canonical root the `CLASSIFY` `api` row folds onto the rail, never an inline `door.is_bearable` tree. `@receipted(LIFECYCLE_REDACTION)` on the `_emit` step is the `observability/receipts#RECEIPT` egress aspect harvesting the `ReceiptContributor` stream onto `Signals.emit` on exit, the keep-all `Redaction(classified=Map.empty())` policy mirroring the graduation owner's `_REDACTION` since lifecycle facts carry no secret field — receipt emission a decorator rail, never an inline `Signals.emit` in the run body. Resilience is the distinct `reliability/resilience#RESILIENCE` owner's `@retry`/`retry_context` rail composed at the lane/transport seam (`stamina` depends on faults, not the reverse); the `_diff` arm's `ifcopenshell.open(revision_path)` file read is the transient-I/O boundary that owner's retry policy wraps where the lifecycle verb is invoked under a lane, never a hand-rolled retry loop here. `Receipt.of` is the runtime two-arg `of(owner, evidence)` factory (`receipts.md` `#21`) discriminating the `(phase, subject, facts)` triple — `("emitted", phase, facts)` for the emitted lifecycle row — and `contribute` yields the `Iterable[Receipt]` port (`receipts.md` `#24`/`#147`), the same shape the `geometry:ifc/authoring.md#AUTHORING` `AuthorReceipt` and the compute `GraduationReceipt` satisfy; the four-positional `Receipt.of("emitted", owner, subject, facts)` call and the single-`Receipt` `contribute` are the forms `compute:graduation/handoff.md#GRADUATION` names deleted.
- [TOKEN_VOCABULARY_RAIL]: the `CostReport`/`ScheduleFormat`/`RuleSet` tokens ALL parse through the one generic `_token[E: StrEnum](vocabulary, raw) -> RuntimeRail[E]` fold testing `raw in vocabulary` (the public 3.12+ `EnumType.__contains__` value-membership contract, not the private `_value2member_map_`) so a known token lifts through `Ok(vocabulary(raw))` and an unknown token is `Error(BoundaryFault(wire=(subject, 0)))` carrying the offending value — the `wire` case the `reliability/faults#FAULT` owner reserves for explicit code-carrying construction (`#20`), the `0` code the "no numeric protocol code in hand" floor a token miss reads. A raw `StrEnum(token)` constructor raising a bare `ValueError`, or the `QUANTITY` rule-set fed raw to `ifc5d.qto.rules[str]` raising a `KeyError`, past the fence are the deleted forms — modeling the closed `qto.RULE_SET` literal as the `RuleSet` `StrEnum` routed through `_token` is the symmetric input-parameterization that keeps every phase's vocabulary one rail-validated discriminant; the rail-parsed token mirrors the malformed-selector path so the selector, the three closed-vocabulary tokens, and a future closed input all arrive as typed faults on the one rail, never a provider exception the fence re-catches.

## [04]-[UPSTREAM]
