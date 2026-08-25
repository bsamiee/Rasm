# [PY_GEOMETRY_IFC_COSTING]

5D/4D model-lifecycle owner — construction-economics and model-management verbs the analysis hop drops: rule-driven quantity take-off, cost-schedule rollup, construction scheduling, recipe-driven model transformation, two-model revision comparison, and the spreadsheet exchange pair an estimator reviews and edits back. `IfcLifecycle` dispatches these phases over the `ifc5d`/`ifc4d`/`ifcpatch`/`ifcdiff`/`ifccsv` IfcOpenShell-ecosystem siblings, returning a `LifecycleResult` whose rows are the typed `LifecycleRow` union. C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the lifecycle dimension that projection never produces.

Every selecting phase admits its query through `IfcSelector` (`ifc/selector#SELECTOR`), so a malformed selector is a typed `BoundaryFault` at admission, never a silent empty `filter_elements` match feeding `quantify`, and the `SelectorMatch` it returns carries the canonical `filter_string` the result keys its evidence on. Full-model 5D walks are a genuinely long native phase, so `run` is `async` and the whole dispatch crosses as `Kernel.of(_lifecycle_kernel, KernelTrait.HOSTILE, idempotent=False)` on `LanePolicy.offload` — SPF source bytes in, mutated-model bytes and result out, the live `ifcopenshell.file` rebuilt worker-side because a pybind11 handle never meets the pickle seam, `idempotent=False` dropping the trait's `WORKER` retry so a mutating transaction never re-applies on worker death. That crossing carries the observability its own prose claims: the lane conduit's pickled `tap` rides as the trailing offload arg and the kernel beats `GeometryPulse.LIFECYCLE` once per phase entry, while `bench` measures the whole entry seam through the graduation `bench_seam` fold keyed per phase. `run` threads the graduation `evidence_run` weave under `EvidenceScope.IFC_LIFECYCLE`, `@beartype(conf=FAULT_CONF)` on `_dispatch` binding the contract fence. `LifecycleResult` retains the phase census, rows, fidelity log, and provider output directly. `IMPORT` is the folder's second model-mutating arm beside `ifc/authoring#AUTHORING`, so it re-applies an edited table inside the same `begin_transaction`/`undo`/`end_transaction` fence that page legislates and a table faulting midway unwinds whole. Durable cost-spreadsheet, exchange-table, `ifcpatch.write`, and diff-export writes defer to `python:data/spatial` as the token or product carried on the result.

## [01]-[INDEX]

- [02]-[LIFECYCLE]: the quantity, cost, schedule, patch, diff, and spreadsheet-exchange phases under one `LifecyclePhase`-discriminated owner folding the five IfcOpenShell ecosystem siblings, the `IfcSelector` gate, the transaction fence over the re-import arm, the pulse and bench observability legs, the parent-side regulatory audit and storage meter, and kind-specific graduation evidence under `BIM_LIFECYCLE`.

## [02]-[LIFECYCLE]

- Owner: `IfcLifecycle` — `@staticmethod` boundary capsule mirroring `IfcAnalysis`, dispatching the phases through one rail-returning `_dispatch` fold over per-phase helpers (`_takeoff`/`_cost`/`_schedule`/`_patch`/`_diff`/`_export`/`_import`), never fat in-arm bodies. `LifecyclePhase`, `ScheduleFormat`, `CostReport`, `TableFormat`, and `RuleSet` are closed `StrEnum` discriminants parsed through the one generic `_token[E: StrEnum]` rail — each phase, `ifc4d` parser, `ifc5Dspreadsheet` writer family, `ifccsv` writer-and-reader key, and `qto.rules` base-quantity set is a rail-validated row, never a raw string fed to `StrEnum(token)`/`rules[str]` that escapes a `ValueError`/`KeyError`. `DiffAxis` is the same law over `ifcdiff.RELATIONSHIP_TYPE`: its member VALUE is the foreign axis name, its `marker` column derives, and the `relationships` argument derives too, so the axis roster, the marker vocabulary, and the change classification are ONE declaration where three divergent transcriptions stood — `DiffPresence` staying separate because presence and change axis answer different questions. `DropLaw` closes the exchange pair's loss vocabulary the same way. `CostReport` and `TableFormat` spell three tokens alike and stay disjoint on their providers' writer tables: only `ifccsv` publishes the in-memory `pd` row and only `ifc5d` the `ifc5Dspreadsheet` subclasses, so one merged vocabulary would mint a writer neither owner holds. `LifecycleRow` is the `@tagged_union` result row carrying one typed case per phase — the exchange pair sharing the one `exchange` case, its direction recoverable from the result's own phase — never a stringly `dict[str, str]` the toolchain must re-parse.
- Cases: `QUANTITY` (rule-driven take-off over `ifc5d.qto`), `COST` (`ifcopenshell.api.cost` rollup over each `IfcCostItem`), `SCHEDULE` (`ifc4d` `<Format>2Ifc` parser populating `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence`), `PATCH` (`ifcpatch.execute` named recipe over the `recipes` namespace), `DIFF` (`ifcdiff` revision comparison over `deepdiff`), `EXPORT` (`ifccsv` selector-scoped column resolve for estimator review), `IMPORT` (`ifccsv` re-application of an edited table's attribute and Pset cells) — matched by `match`/`assert_never`, each dispatching to the ecosystem sibling that owns it. The exchange pair stays two rows rather than one direction-flagged phase because the two arms diverge on all four discriminants a collapse would have to erase: admission shape, mutation posture, transaction fence, and residual source.
- Entry: `IfcLifecycle.run` takes SPF source bytes, a `LifecyclePhase`, a `spec` whose meaning is phase-fixed — validated selector for `QUANTITY`, cost-schedule GlobalId and report token for `COST`, `<format>:<path>` for `SCHEDULE`, `<recipe>:<json-args>` for `PATCH`, revision path for `DIFF`, `<selector>#<format>:<columns>` for `EXPORT`, table path for `IMPORT` — the lane, and the `composition` custody key, returning `RuntimeRail[tuple[bytes, LifecycleResult]]` through the `evidence_run` weave over the `HOSTILE` kernel crossing: mutating phases ride home as the successor model's SPF bytes (`PATCH` serializes the file `ifcpatch.execute` minted, never the pre-patch input), the `READONLY` roster — `DIFF` and `EXPORT`, the two phases that read the model and write nothing into it — rides `b""`, and a kernel-side `_dispatch` fault crosses home as the typed `BoundaryFault` on the kernel's own rail — the caller flattens the nested rail once, so tag, subject, and fields survive the seam whole. The SPF source digest mints PARENT-side through the one content-addressing owner and threads in as the result's identity prefix, so the evidence key names the exact model the phase ran against rather than the caller's spec text alone. `_dispatch` partitions the `spec` once on the `PHASE_DELIMITER` table keyed by every phase including `DIFF`'s empty-delimiter row (whole `spec` as revision path, no `partition("")` fault), never a `.get` default that silently drops a phase. `QUANTITY` binds the `#<rule-set>` token AND the validated selector monadically, so both fault before `quantify` runs, and `EXPORT` binds the writer token, the column vocabulary, and the selector the same way, so a typo'd format and an empty column list each name themselves before a single cell resolves. `IMPORT` admits its reader off the table's own suffix against the `SUFFIXED` roster, because `Import` derives that reader itself and returns silently on a suffix it does not know — an unadmitted table would otherwise settle as a clean zero-row run — and keys its run identity on the table's CONTENT digest beside the format, the path riding the result's `subjects` as display metadata alone, so two edits of one table at one path key distinct evidence exactly as the SPF digest names the exact model. Each arm derives its own `subjects` from the phase's true subject set; `DIFF`'s `population` field separately carries the full compared element count the drift fraction divides against.
- Auto: `QUANTITY`'s `ifc5d.qto.quantify`/`edit_qtos` answers the whole base-quantity schedule keyed by the `qto.rules` table and writes it back as `IfcElementQuantity`; the `RuleSet` vocabulary over those keys is this owner's and the sibling analysis space-program grade composes it rather than transcribing a second copy. `COST`, `PATCH`, and `DIFF` each carry the phase's product as a typed token on the result subject — the `CostReport` writer key, the patch product type, the diff change class — so the durable write stays the data boundary's. `_cost` keeps only values whose `AppliedValue` actually resolves and counts the rest on `unpriced`: an unpriced `IfcCostValue` is missing a price, not priced at zero, and an `or 0.0` fold erases a genuine zero-cost item and an unpriced one onto one indistinguishable row. `EXPORT` drives `export` with `format=None`/`output=None`, so the resolve half alone populates the stateful object's grid and resolved header row and no writer opens a handle — the `TableFormat` token on the subject is what the data boundary re-keys its writer on, the same deferral `COST` makes — and it hands `export` a FRESH list, the member inserting the `include_global_id` key column into the caller's own roster. Wildcard columns expand at this owner because `export` expands none itself: `get_wildcard_attributes` reads the object's `ifc_file`, which `export` binds only on entry, so the model binds first and a `<pset>.*` column reaches the grid as the real property roster that pset carries. `IMPORT` reads its census off the provider's OWN per-row dispatch rather than a second read of the table, `Import` publishing no telemetry and printing its misses to stdout; one `process_row` override classifies each row against a model-guid roster — a membership test where the provider's own miss path is a bare `except` around a raising lookup — and delegates the write untouched, so rows applied, rows skipped, and cells written are exact rather than inferred. That override threads ONE slot holding a frozen `FidelityLog` through the pair's monoid, so the provider-driven loop accumulates without a list mutating beside the return. No phase carries an `if/else` value ladder or mints a per-phase class: one fold arm and one helper per row, the owning package bound directly.
- Law: durable evidence lands on the `python:runtime/observability/journal#LEDGER` plane at the async `run` parent — one `REGULATORY` `AuditFact` per run beside a `STORAGE` `MeterFact` over the successor bytes the phase wrote, so read-only `DIFF` records its audit line alone. The seat is the parent by two laws at once: recording suspends, and the HOSTILE kernel's whole observability reach is the pickled tap queue, so a worker binds no plane and runs no loop to reach one. The facts mint off the SETTLED result, so a refused phase names no run that produced nothing, and the record rail binds into the verdict rather than riding beside it.
- Law: the crossing carries its declared cost — `_lifecycle_kernel` takes the lane conduit's pickled `tap` as its trailing offload arg and beats `GeometryPulse.LIFECYCLE` once per phase entry on the runtime `StageMark` payload under this page's own closed `IfcLifecycleStage` roster, with `total=Some(1)`, because each phase is ONE opaque provider call with no per-element hook and a fabricated denominator would state an extent the provider never publishes; delivery is the lane's lossy law and the worker reaches only the queue proxy. `bench` rides the graduation `bench_seam` fold over the whole `run` crossing — offload, worker rebuild, provider phase, serialization, weave — keyed `rasm.geometry.ifc.costing.<phase>`, so a latency row compares like-for-like across the five phases one dispatch serves, with `bench_terminal` the process-terminal wrap.
- Law: `IMPORT` drives `IfcCsv().Import` INSIDE `ifcopenshell`'s own `begin_transaction`/`undo`/`end_transaction` fence — the single transaction law `ifc/authoring#AUTHORING` legislates over the folder's exactly two mutating arms. `IfcCsv().Import` re-applies cell after cell holding no rollback of its own, so a table faulting midway persists half an estimator's edit; `boundary` converts a provider raise into the typed rail INSIDE the fence, one `is_error()` test unwinds a raise and a typed refusal alike before the close, only a clean `Ok` commits, and the undo and close each cross their own `boundary` trap so the fence reaches its terminal state on every exit with no raise escaping it — a torn rollback or a refusing close accumulates onto the primary fault through the runtime's `BoundaryFault.combine` monoid, the combined rail propagating only after the close, so no secondary fault replaces the cause and no cause shadows the tear. Durable records seat past the fence at the async `run` parent under that same law, never between the two calls.
- Law: the pair's substitution vocabulary is one agreement rather than two spellings — `NULL_CELL`/`EMPTY_CELL` bind at both members and `bool_true`/`bool_false`/`concat` ride the provider's own matching defaults on `export` and `Import` alike, never re-spelled here, because a token spelled at one end alone reads a null back as its own literal string and the package CLI's divergent `--null`/`--empty` defaults are exactly that failure shipped. `BLIND_KEYS` names the one WRITE asymmetry the pair cannot close — `process_row` drops any column whose key carries `count` or `material`, so such a column exports cleanly and never writes back — and it is the first of five `DropLaw` rows the pair's ONE `_dropped` classifier NAMES per cell, beside the two substitution spellings, the GlobalId the model does not hold, and the table row the provider's per-index write truncates. Both legs read that one classifier, on the export grid and the import table alike, so each side's loss carries its law, its subject, and its column rather than an integer three structurally different losses were indistinguishable inside.
- Output: the result carries the census and frames carry the rows — `frame()` re-projects the typed `LifecycleRow` facts as one phase-homogeneous `EvidenceFrame` and the drop occurrences cross as the `fidelity_frame()` family beside it, because a whole-model take-off is three fact keys per quantity per element and a flattened row stream is a hundred-thousand-key map per run. Phase-specific `evidence` keys the subject-relative empty fraction for `QUANTITY`/`SCHEDULE` (a phase producing no rows for a non-empty subject set is a degenerate run keyed `1.0`), the bare no-row fraction plus the `unpriced` count for `COST` (whose `subjects` is the schedule guid and report token, not a produced population), the bare no-row fraction for `PATCH`, the changed-over-`population` drift fraction for `DIFF` (never changed-over-changed, which clears every ceiling), and ONE FRACTION PER `DropLaw` for `EXPORT`/`IMPORT` over the cells the exchange actually touched, so a caller ceiling gates the specific loss it can act on rather than the merged fraction that graded a badly-mapped column set and a model missing half its subjects alike. That ledger rides the result as the `FidelityLog` the phase's own fold RETURNED — its census derived off the occurrence stream, so a count and its evidence cannot disagree.
- Packages: `ifc5d` (`qto.rules`/`quantify`/`edit_qtos` take-off surface only — the `ifc5Dspreadsheet` writer family is the data boundary's), `ifcopenshell` (`api.cost` rollup and in-process model access; selector filtering is the validated gate, never a direct `util.selector.filter_elements` call here), `ifc4d` (`<Format>2Ifc` named parsers), `ifcpatch` (`execute` over the `recipes` namespace; the durable `write` is the data boundary's), `ifcdiff` (`IfcDiff`/`change_register`/`added_elements`/`deleted_elements`, its `RELATIONSHIP_TYPE` axis the `DiffAxis` roster keys on; the `export` JSON is the data boundary's), `ifccsv` (`IfcCsv().export` run for its resolve half, `get_wildcard_attributes` for the column expansion, `IfcCsv().Import` for the re-application and `process_row` for the census seam; the `export_*`/`import_*` writer-reader family and the durable spreadsheet are the data boundary's), and `geometry`/`expression`/`beartype`/`runtime` (`ContentIdentity` the one content-addressing owner, `LanePolicy`/`pulsed` the conduit, `Bench` the measurement tier, `Journal` with the `AuditFact`/`MeterFact` vocabulary the run's durable evidence records through) per the fence imports; `IfcSelector` is the only `filter_elements` caller.
- Growth: a new quantity rule set is one `RuleSet` row over the upstream `qto.rules` key; a new cost format one `CostReport` row the data boundary binds to its `ifc5Dspreadsheet` writer subclass; a new schedule format one `ScheduleFormat` row binding its `<Format>2Ifc` parser; a new model transformation one `recipe` name in the `ifcpatch.execute` directive; a new diff axis one `DiffAxis` row whose marker and `relationships` entry both derive from it; a new exchange drop law one `DropLaw` row and one `_law` arm reaching the census, the ledger, and the fidelity frame together; a new exchange format one `TableFormat` row the data boundary binds to its `export_*` writer, plus one `SUFFIXED` member where that format names a file suffix; a new exported column one selector-path string in the caller's own `spec`, zero page edits; a new mid-phase fact is one `pulsed` call inside the phase helper that can see it, zero conduit edits; a newly audited run column is one `_evidence` `Change` row, the verb and the meter deriving off the phase and the successor already in hand — zero new surface, no parallel per-phase class family.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy; no ledger, custody, or retention window minted here, the plane arriving bound at the composition root and this owner declaring a `Retain` class alone; no durable store — cost spreadsheet, exchange table, `ifcpatch.write` serialization, and diff `export` JSON all defer to `python:data/spatial` as the token or product carried on the result, the exchange arm binding the writer key without holding a file handle across the seam; no Rhino/GH mutation. No hand-rolled `csv` fold over `util.element.get_psets` where `export` owns column resolution, and no bespoke `by_guid`-keyed property-set mutation where `process_row` routes every cell through `util.selector.set_element_value`. Ecosystem siblings bind one module-scope `lazy import`/`lazy from` each, the proxy reifying worker-side on the first phase that reaches it — the manifest roster bans the EAGER module-level form alone, and a function-local import earns nothing the module binding has not already deferred; the parser and writer rosters those siblings populate stay inside their phase bodies, a module-scope cell over a deferred band being the reification the deferral exists to prevent. The `spec` selector crosses the `IfcSelector.filter` validated gate, never a raw `util.selector.filter_elements` passthrough.

```python
from collections.abc import Iterable
from enum import StrEnum
from functools import partial
from pathlib import Path
from queue import Queue
from tempfile import TemporaryDirectory
from typing import Final, Literal, assert_never

from beartype import beartype
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.json import decode

lazy import ifc5d.qto
lazy import ifccsv
lazy import ifcdiff
lazy import ifcopenshell
lazy import ifcpatch
lazy from ifc4d.asta2ifc import Asta2Ifc
lazy from ifc4d.msproject2ifc import MSProject2Ifc
lazy from ifc4d.p62ifc import P62Ifc
lazy from ifcopenshell.api.cost import calculate_cost_item_resource_value

from rasm.geometry.graduation import (
    EvidenceFrame,
    EvidenceScope,
    GeometryLeg,
    GeometryPulse,
    GeometrySubject,
    bench_seam,
    bench_subject,
    evidence_key,
    evidence_run,
)
from rasm.geometry.ifc.selector import IfcFault, IfcRoster, IfcSelector
from rasm.runtime.faults import (
    FAULT_CONF,
    PACKAGE,
    TERMINAL,
    TRANSIENT,
    BoundaryFault,
    Catch,
    Disposition,
    FaultRow,
    RuntimeRail,
    boundary,
    rostered,
    traversed,
)
from rasm.runtime.hooks import StageMark
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.journal import Actor, Assigned, AuditFact, Fact, Journal, MeterFact, Party, Resource, Retain
from rasm.runtime.lanes import LanePolicy, PulseFact, pulsed
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.profiles import Benchmark
from rasm.runtime.workers import Kernel, KernelTrait

# --- [TYPES] ----------------------------------------------------------------------------


class IfcLifecycleStage(StrEnum):
    PHASE = "phase"


class LifecyclePhase(StrEnum):
    QUANTITY = "quantity"
    COST = "cost"
    SCHEDULE = "schedule"
    PATCH = "patch"
    DIFF = "diff"
    EXPORT = "export"
    IMPORT = "import"


class ScheduleFormat(StrEnum):
    MSPROJECT = "msproject"
    P6 = "p6"
    ASTA = "asta"


class CostReport(StrEnum):
    CSV = "csv"
    ODS = "ods"
    XLSX = "xlsx"


class TableFormat(StrEnum):
    CSV = "csv"
    ODS = "ods"
    XLSX = "xlsx"
    PD = "pd"


class RuleSet(StrEnum):
    IFC4 = "IFC4QtoBaseQuantities"
    IFC4X3 = "IFC4X3QtoBaseQuantities"


class DiffPresence(StrEnum):
    ADDED = "added"
    DELETED = "deleted"
    SURVIVING = "surviving"


class DiffAxis(StrEnum):
    GEOMETRY = "geometry"
    ATTRIBUTES = "attributes"
    TYPE = "type"
    PROPERTY = "property"
    CONTAINER = "container"
    AGGREGATE = "aggregate"
    CLASSIFICATION = "classification"

    @property
    def marker(self) -> str:
        return "properties_changed" if self is DiffAxis.PROPERTY else f"{self.value}_changed"

    @staticmethod
    def of_markers(markers: "dict[str, object]") -> "RuntimeRail[frozenset[DiffAxis]]":
        seat = Map.of_seq((axis.marker, axis) for axis in DiffAxis)
        unknown = Block.of_seq(sorted(key for key in markers if key not in seat))
        return (
            Ok(frozenset(seat[key] for key, moved in markers.items() if moved))
            if unknown.is_empty()
            else Error(_domain(IfcFault(unrostered=(DiffAxis.__name__, ",".join(unknown)))))
        )


class DropLaw(StrEnum):
    BLIND_COLUMN = "blind-column"
    SUBSTITUTED_NULL = "substituted-null"
    SUBSTITUTED_EMPTY = "substituted-empty"
    ABSENT_SUBJECT = "absent-subject"
    SHORT_ROW = "short-row"


@tagged_union(frozen=True)
class LifecycleRow:
    tag: Literal["quantity", "cost", "task", "patch", "diff", "exchange"] = tag()
    quantity: tuple[str, str, str, float] = case()
    cost: tuple[str, str, float] = case()
    task: tuple[str, str] = case()
    patch: tuple[str, str] = case()
    diff: tuple[str, DiffPresence, frozenset[DiffAxis]] = case()
    exchange: tuple[str, int, int] = case()

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
    def of_diff(element: str, presence: DiffPresence, axes: frozenset[DiffAxis] = frozenset()) -> "LifecycleRow":
        return LifecycleRow(diff=(element, presence, axes))

    @staticmethod
    def of_exchange(element: str, carried: int, dropped: int) -> "LifecycleRow":
        return LifecycleRow(exchange=(element, carried, dropped))

    @property
    def facts(self) -> dict[str, object]:
        match self:
            case LifecycleRow(tag="quantity", quantity=(element, qto, name, value)):
                return {"element": element, "quantity": f"{qto}.{name}", "value": value}
            case LifecycleRow(tag="cost", cost=(item, name, applied)):
                return {"item": item, "name": name, "value": applied}
            case LifecycleRow(tag="task", task=(guid, name)):
                return {"task": guid, "name": name}
            case LifecycleRow(tag="patch", patch=(recipe, product)):
                return {"recipe": recipe, "product": product}
            case LifecycleRow(tag="diff", diff=(element, presence, axes)):
                return {"element": element, "presence": presence.value, "axes": ",".join(sorted(axis.value for axis in axes))}
            case LifecycleRow(tag="exchange", exchange=(element, carried, dropped)):
                return {"element": element, "cells": carried, "dropped": dropped}
            case unreachable:
                assert_never(unreachable)


# --- [CONSTANTS] ------------------------------------------------------------------------

LIFECYCLE_SUBJECT: Final[GeometrySubject] = GeometrySubject.BIM_LIFECYCLE

PHASE_DELIMITER: Final[Map[LifecyclePhase, str]] = Map.of_seq([
    (LifecyclePhase.QUANTITY, "#"),
    (LifecyclePhase.COST, ":"),
    (LifecyclePhase.SCHEDULE, ":"),
    (LifecyclePhase.PATCH, ":"),
    (LifecyclePhase.DIFF, ""),
    (LifecyclePhase.EXPORT, "#"),
    (LifecyclePhase.IMPORT, ""),
])

READONLY: Final[frozenset[LifecyclePhase]] = frozenset({LifecyclePhase.DIFF, LifecyclePhase.EXPORT})

NULL_CELL: Final[str] = "-"
EMPTY_CELL: Final[str] = ""

BLIND_KEYS: Final[frozenset[str]] = frozenset({"count", "material"})

SUFFIXED: Final[frozenset[TableFormat]] = frozenset({TableFormat.CSV, TableFormat.ODS, TableFormat.XLSX})

OWNER: Final[str] = f"{PACKAGE}.{GeometryLeg.COSTING.value}"

# --- [MODELS] ---------------------------------------------------------------------------


class DropFact(Struct, frozen=True, gc=False):
    law: DropLaw
    subject: str
    column: str = ""
    value: str = ""


class FidelityLog(Struct, frozen=True, gc=False):
    drops: "Block[DropFact]" = Block.empty()
    carried: int = 0

    @staticmethod
    def combined(left: "FidelityLog", right: "FidelityLog") -> "FidelityLog":
        return FidelityLog(drops=left.drops.append(right.drops), carried=left.carried + right.carried)

    @property
    def census(self) -> "Map[DropLaw, int]":
        return self.drops.fold(lambda seat, fact: seat.add(fact.law, seat.try_find(fact.law).default_value(0) + 1), Map.empty())


class LifecycleResult(Struct, frozen=True, gc=False):
    phase: LifecyclePhase
    spec: str
    subjects: tuple[str, ...]
    rows: tuple[LifecycleRow, ...]
    population: int = 0
    unpriced: int = 0
    fidelity: FidelityLog = FidelityLog()

    def evidence(self) -> dict[str, float]:
        match self.phase:
            case LifecyclePhase.QUANTITY | LifecyclePhase.SCHEDULE:
                produced = max(len(self.subjects), 1)
                return {"empty": 1.0 - min(len(self.rows), produced) / produced}
            case LifecyclePhase.COST:
                return {"empty": 0.0 if self.rows else 1.0, "unpriced": float(self.unpriced)}
            case LifecyclePhase.PATCH:
                return {"empty": 0.0 if self.rows else 1.0}
            case LifecyclePhase.DIFF:
                return {"drift": len(self.subjects) / max(self.population, 1)}
            case LifecyclePhase.EXPORT | LifecyclePhase.IMPORT:
                census = self.fidelity.census
                touched = max(self.fidelity.carried + sum(count for _, count in census.items()), 1)
                return {f"drop.{law.value}": census.try_find(law).default_value(0) / touched for law in DropLaw}
            case unreachable:
                assert_never(unreachable)

    def fidelity_frame(self) -> "RuntimeRail[EvidenceFrame]":
        drops = self.fidelity.drops
        table: dict[str, list[object]] = {
            "law": [fact.law.value for fact in drops],
            "subject": [fact.subject for fact in drops],
            "column": [fact.column for fact in drops],
            "value": [fact.value for fact in drops],
        }
        return EvidenceFrame.of(LIFECYCLE_SUBJECT, evidence_key(LIFECYCLE_SUBJECT, f"{self.spec}#fidelity"), table)

    def frame(self) -> "RuntimeRail[EvidenceFrame]":
        names = tuple(self.rows[0].facts) if self.rows else ()
        table: dict[str, list[object]] = {
            "phase": [self.phase.value] * len(self.rows),
            **{name: [row.facts[name] for row in self.rows] for name in names},
        }
        return EvidenceFrame.of(LIFECYCLE_SUBJECT, evidence_key(LIFECYCLE_SUBJECT, self.spec), table)


# --- [ERRORS] ---------------------------------------------------------------------------

PHASE_REFUSED: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.COSTING, point="phase", arm="boundary", defect="phase-refused", retriability=TERMINAL
)
TABLE_READ: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.COSTING, point="import.table", arm="resource", defect="table-read", retriability=TRANSIENT
)
IMPORT_APPLY: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.COSTING, point="import.apply", arm="boundary", defect="cells-refused", retriability=TERMINAL
)
IMPORT_UNDO: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.COSTING, point="import.undo", arm="boundary", defect="rollback-torn", retriability=TERMINAL
)
IMPORT_CLOSE: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.COSTING, point="import.close", arm="boundary", defect="close-refused", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([PHASE_REFUSED, TABLE_READ, IMPORT_APPLY, IMPORT_UNDO, IMPORT_CLOSE]))


def _domain(fault: IfcFault) -> BoundaryFault:
    return BoundaryFault.of(PHASE_REFUSED, fault)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _evidence(successor: bytes, result: LifecycleResult) -> "Block[Fact]":
    audited = AuditFact(
        action=f"geometry.{result.phase.value}",
        actor=Party(kind=Actor.SERVICE, key=OWNER),
        target=Party(kind="model", key=result.spec),
        retention=Retain.REGULATORY,
        change=(Assigned(path="/rows", next=str(len(result.rows))), Assigned(path="/subjects", next=str(len(result.subjects)))),
    )
    metered = MeterFact(resource=Resource.STORAGE, quantity=len(successor), surface=result.spec)
    return Block.of_seq((audited, metered) if successor else (audited,))


def _token[E: StrEnum](vocabulary: type[E], raw: str) -> "RuntimeRail[E]":
    return Ok(vocabulary(raw)) if raw in vocabulary else Error(_domain(IfcFault(unrostered=(vocabulary.__name__, raw))))


def _columns(raw: str) -> "RuntimeRail[tuple[str, ...]]":
    columns = tuple(stripped for name in raw.split(",") if (stripped := name.strip()))
    return Ok(columns) if columns else Error(_domain(IfcFault(empty_roster=("lifecycle.export", IfcRoster.EXPORT_COLUMN))))


def _reader(table: str) -> "RuntimeRail[TableFormat]":
    return _token(TableFormat, Path(table).suffix.removeprefix(".").lower()).bind(
        lambda fmt: Ok(fmt) if fmt in SUFFIXED else Error(_domain(IfcFault(unserved=("lifecycle.import", fmt.value))))
    )


def _law(key: str, value: object) -> "Option[DropLaw]":
    return (
        Some(DropLaw.BLIND_COLUMN)
        if any(blind in key.lower() for blind in BLIND_KEYS)
        else Some(DropLaw.SUBSTITUTED_NULL)
        if value == NULL_CELL
        else Some(DropLaw.SUBSTITUTED_EMPTY)
        if value == EMPTY_CELL
        else Nothing
    )


def _dropped(subject: str, values: "Iterable[object]", keys: "Iterable[str]") -> FidelityLog:
    row, header = tuple(values), tuple(keys)
    graded = Block.of_seq((str(key), value) for index, (value, key) in enumerate(zip(row, header)) if index).map(
        lambda cell: (cell[0], cell[1], _law(cell[0], cell[1]))
    )
    truncated = Block.of_seq(DropFact(law=DropLaw.SHORT_ROW, subject=subject, column=str(key)) for key in header[len(row) :])
    return FidelityLog(
        drops=truncated.append(
            graded.choose(lambda cell: cell[2].map(lambda law: DropFact(law=law, subject=subject, column=cell[0], value=str(cell[1]))))
        ),
        carried=sum(1 for _, _, law in graded if law.is_none()),
    )


class IfcLifecycle:
    @staticmethod
    async def run(
        source: bytes, phase: LifecyclePhase, spec: str, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[tuple[bytes, LifecycleResult]]":
        source_key = ContentIdentity.key("ifc.spf", source).project("wire")
        rail = await evidence_run(
            EvidenceScope.IFC_LIFECYCLE,
            f"run.{phase}",
            partial(
                lane.offload,
                Kernel.of(_lifecycle_kernel, KernelTrait.HOSTILE, idempotent=False),
                source,
                phase,
                spec,
                source_key,
                lane.pulses.tap,
            ),
            composition=composition,
        )
        match rail.bind(lambda inner: inner):
            case Result(tag="ok", ok=(successor, result)):
                return (await Journal.record(_evidence(successor, result), scope=composition)).map(lambda _landed: (successor, result))
            case refused:
                return Error(refused.error)

    @staticmethod
    def bench(
        source: bytes,
        phase: LifecyclePhase,
        spec: str,
        lane: LanePolicy,
        *,
        rounds: int = 32,
        warmup: int = 4,
        composition: ScopeKey = DEFAULT_SCOPE,
    ) -> "RuntimeRail[Benchmark]":
        return bench_seam(
            bench_subject(EvidenceScope.IFC_LIFECYCLE, phase.value),
            partial(IfcLifecycle.run, source, phase, spec, lane, composition=composition),
            rounds=rounds,
            warmup=warmup,
        )

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _dispatch(
        model: "ifcopenshell.file", phase: LifecyclePhase, spec: str, source_key: str
    ) -> "RuntimeRail[tuple[LifecycleResult, ifcopenshell.file]]":
        delimiter = PHASE_DELIMITER[phase]
        head, _, tail = spec.partition(delimiter) if delimiter else (spec, "", "")
        base = f"{phase.value}|{source_key}"
        match phase:
            case LifecyclePhase.QUANTITY:
                return _token(RuleSet, tail or RuleSet.IFC4.value).bind(
                    lambda rule_set: IfcSelector.filter(model, head).map(
                        lambda matched: (
                            IfcLifecycle._takeoff(model, matched.elements, rule_set, f"{base}|{matched.query.filter_string}#{rule_set.value}"),
                            model,
                        )
                    )
                )
            case LifecyclePhase.COST:
                return _token(CostReport, tail or "csv").map(
                    lambda report: (IfcLifecycle._cost(model, head, report, f"{base}|{head}#{report.value}"), model)
                )
            case LifecyclePhase.SCHEDULE:
                return _token(ScheduleFormat, head).map(lambda fmt: (IfcLifecycle._schedule(model, fmt, tail, f"{base}|{fmt.value}#{tail}"), model))
            case LifecyclePhase.PATCH:
                return Ok(IfcLifecycle._patch(model, head, tail, f"{base}|{head}#{tail}"))
            case LifecyclePhase.DIFF:
                return IfcLifecycle._diff(model, head, f"{base}|{head}").map(lambda result: (result, model))
            case LifecyclePhase.EXPORT:
                token, _, columns = tail.partition(":")
                return _token(TableFormat, token or TableFormat.CSV.value).bind(
                    lambda fmt: _columns(columns).bind(
                        lambda vocabulary: IfcSelector.filter(model, head).map(
                            lambda matched: (
                                IfcLifecycle._export(
                                    model,
                                    matched.elements,
                                    vocabulary,
                                    fmt,
                                    f"{base}|{matched.query.filter_string}#{fmt.value}:{','.join(vocabulary)}",
                                ),
                                model,
                            )
                        )
                    )
                )
            case LifecyclePhase.IMPORT:
                return _reader(head).bind(
                    lambda fmt: boundary(TABLE_READ, Path(head).read_bytes, catch=(OSError,)).bind(
                        lambda octets: IfcLifecycle._import(
                            model, head, fmt, f"{base}|{ContentIdentity.key('ifc.table', octets).project('wire')}#{fmt.value}"
                        ).map(lambda result: (result, model))
                    )
                )
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _takeoff(
        model: "ifcopenshell.file", elements: tuple["ifcopenshell.entity_instance", ...], rule_set: RuleSet, spec: str
    ) -> LifecycleResult:
        results = ifc5d.qto.quantify(model, set(elements), ifc5d.qto.rules[rule_set.value])
        ifc5d.qto.edit_qtos(model, results)
        rows = tuple(
            LifecycleRow.of_quantity(element.GlobalId, qto, name, float(value))
            for element, qtos in results.items()
            for qto, quantities in qtos.items()
            for name, value in quantities.items()
        )
        return LifecycleResult(LifecyclePhase.QUANTITY, spec, tuple(e.GlobalId for e in results), rows)

    @staticmethod
    def _cost(model: "ifcopenshell.file", schedule_guid: str, report: CostReport, spec: str) -> LifecycleResult:
        schedule = model.by_guid(schedule_guid)
        items = model.by_type("IfcCostItem")
        for item in items:
            calculate_cost_item_resource_value(model, cost_item=item)
        valued = tuple(
            (item, getattr(value, "AppliedValue", None)) for item in items for value in (item.CostValues or ())
        )
        rows = tuple(
            LifecycleRow.of_cost(item.GlobalId, item.Name or "", float(applied)) for item, applied in valued if isinstance(applied, (int, float))
        )
        return LifecycleResult(
            LifecyclePhase.COST,
            spec,
            (schedule.GlobalId, report.value),
            rows,
            unpriced=sum(1 for _, applied in valued if not isinstance(applied, (int, float))),
        )

    @staticmethod
    def _schedule(model: "ifcopenshell.file", fmt: ScheduleFormat, source: str, spec: str) -> LifecycleResult:
        parser = {
            ScheduleFormat.MSPROJECT: MSProject2Ifc,
            ScheduleFormat.P6: P62Ifc,
            ScheduleFormat.ASTA: Asta2Ifc,
        }[fmt]()
        parser.file = model
        parser.xml = source
        plans = model.by_type("IfcWorkPlan")
        parser.work_plan = plans[0] if plans else None
        parser.execute()
        tasks = model.by_type("IfcTask")
        rows = tuple(LifecycleRow.of_task(t.GlobalId, t.Name or "") for t in tasks)
        return LifecycleResult(LifecyclePhase.SCHEDULE, spec, tuple(t.GlobalId for t in tasks), rows)

    @staticmethod
    def _patch(model: "ifcopenshell.file", recipe: str, args: str, spec: str) -> tuple[LifecycleResult, "ifcopenshell.file"]:
        output = ifcpatch.execute({
            "input": "",
            "file": model,
            "recipe": recipe,
            "arguments": decode(args.encode(), type=list[object]) if args else [],
        })
        match output:
            case ifcopenshell.file() as patched:
                product, successor = output.schema, patched
            case None:
                product, successor = "in-place", model
            case _:
                product, successor = type(output).__name__, model
        rows = (LifecycleRow.of_patch(recipe, product),)
        return LifecycleResult(LifecyclePhase.PATCH, spec, (recipe, product), rows), successor

    @staticmethod
    def _diff(model: "ifcopenshell.file", revision_path: str, spec: str) -> "RuntimeRail[LifecycleResult]":
        revision = ifcopenshell.open(revision_path)
        differ = ifcdiff.IfcDiff(model, revision, relationships=[axis.value for axis in DiffAxis])
        differ.diff()
        subjects = (*differ.change_register, *differ.added_elements, *differ.deleted_elements)
        population = len(model.by_type("IfcRoot")) + len(differ.added_elements)
        return traversed(
            Block.of_seq(differ.change_register.items()).map(
                lambda entry: DiffAxis.of_markers(entry[1]).map(lambda axes: LifecycleRow.of_diff(entry[0], DiffPresence.SURVIVING, axes))
            ),
            by=Disposition.ACCUMULATE,
        ).map(
            lambda changed: LifecycleResult(
                LifecyclePhase.DIFF,
                spec,
                subjects,
                (
                    *changed,
                    *(LifecycleRow.of_diff(guid, DiffPresence.ADDED) for guid in differ.added_elements),
                    *(LifecycleRow.of_diff(guid, DiffPresence.DELETED) for guid in differ.deleted_elements),
                ),
                population=population,
            )
        )

    @staticmethod
    def _export(
        model: "ifcopenshell.file",
        elements: tuple["ifcopenshell.entity_instance", ...],
        columns: tuple[str, ...],
        fmt: TableFormat,
        spec: str,
    ) -> LifecycleResult:
        exporter = ifccsv.IfcCsv()
        exporter.ifc_file = model
        resolved = tuple(
            name for column in columns for name in (exporter.get_wildcard_attributes(column) if column.endswith(".*") else (column,))
        )
        exporter.export(
            model,
            elements,
            list(resolved),
            output=None,
            format=None,
            include_global_id=True,
            null=NULL_CELL,
            empty=EMPTY_CELL,
            sort=[{"name": "GlobalId", "order": "ASC"}],
        )
        def step(state: "tuple[Block[LifecycleRow], FidelityLog]", cells: "tuple[object, ...]") -> "tuple[Block[LifecycleRow], FidelityLog]":
            held, log = state
            subject = str(cells[0])
            fidelity = _dropped(subject, cells, exporter.headers)
            return (
                held.append(Block.singleton(LifecycleRow.of_exchange(subject, fidelity.carried, len(fidelity.drops)))),
                FidelityLog.combined(log, fidelity),
            )

        rows, fidelity = Block.of_seq(exporter.results).fold(step, (Block.empty(), FidelityLog()))
        return LifecycleResult(LifecyclePhase.EXPORT, spec, (fmt.value, *exporter.headers), tuple(rows), fidelity=fidelity)

    @staticmethod
    def _import(model: "ifcopenshell.file", table: str, fmt: TableFormat, spec: str) -> "RuntimeRail[LifecycleResult]":
        roster = frozenset(root.GlobalId for root in model.by_type("IfcRoot"))
        held: list[tuple[Block[LifecycleRow], FidelityLog]] = [(Block.empty(), FidelityLog())]

        class _Census(ifccsv.IfcCsv):
            def process_row(
                self,
                ifc_file: "ifcopenshell.file",
                row: list[str],
                headers: list[str],
                attributes: list[str | None],
                null: str,
                empty: str,
                bool_true: str,
                bool_false: str,
                concat: str,
            ) -> None:
                guid = str(row[0])
                rows, log = held[0]
                if guid not in roster:
                    absent = FidelityLog(drops=Block.singleton(DropFact(law=DropLaw.ABSENT_SUBJECT, subject=guid)))
                    held[0] = (rows.append(Block.singleton(LifecycleRow.of_exchange(guid, 0, 1))), FidelityLog.combined(log, absent))
                    return
                keys = tuple(str(attribute or header) for attribute, header in zip(attributes, headers, strict=True))
                fidelity = _dropped(guid, row, keys)
                held[0] = (
                    rows.append(Block.singleton(LifecycleRow.of_exchange(guid, fidelity.carried, len(fidelity.drops)))),
                    FidelityLog.combined(log, fidelity),
                )
                super().process_row(ifc_file, row, headers, attributes, null, empty, bool_true, bool_false, concat)

        reading: Catch = (ifcopenshell.Error, KeyError, IndexError, TypeError, ValueError, OSError)
        staging: Catch = (ifcopenshell.Error, RuntimeError)
        model.begin_transaction()
        applied = boundary(IMPORT_APPLY, lambda: _Census().Import(model, table, null=NULL_CELL, empty=EMPTY_CELL), catch=reading)
        unwound = boundary(IMPORT_UNDO, model.undo, catch=staging) if applied.is_error() else Ok(None)
        closed = boundary(IMPORT_CLOSE, model.end_transaction, catch=staging)
        faults = Block.of_seq((applied, unwound, closed)).choose(lambda rail: rail.swap().to_option())
        rows, fidelity = held[0]
        return (
            Ok(LifecycleResult(LifecyclePhase.IMPORT, spec, (fmt.value, table), tuple(rows), fidelity=fidelity))
            if faults.is_empty()
            else Error(faults.reduce(BoundaryFault.combine))
        )


def _serialized(model: "ifcopenshell.file", phase: LifecyclePhase) -> bytes:
    if phase in READONLY:
        return b""
    with TemporaryDirectory(prefix="ifc-lifecycle-") as work:
        path = Path(work, "model.ifc")
        model.write(str(path))
        return path.read_bytes()


def _lifecycle_kernel(
    source: bytes, phase: LifecyclePhase, spec: str, source_key: str, tap: "Queue[PulseFact | None]"
) -> "RuntimeRail[tuple[bytes, LifecycleResult]]":
    pulsed(tap, GeometryPulse.LIFECYCLE, StageMark(stage=IfcLifecycleStage.PHASE.value, done=0, total=Some(1)))

    model = ifcopenshell.file.from_string(source.decode())
    return IfcLifecycle._dispatch(model, phase, spec, source_key).map(lambda pair: (_serialized(pair[1], phase), pair[0]))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
