# [PY_GEOMETRY_IFC_COSTING]

5D/4D model-lifecycle owner — construction-economics and model-management verbs the analysis hop drops: rule-driven quantity take-off, cost-schedule rollup, construction scheduling, recipe-driven model transformation, and two-model revision comparison. `IfcLifecycle` dispatches these phases over the `ifc5d`/`ifc4d`/`ifcpatch`/`ifcdiff` IfcOpenShell-ecosystem siblings, emitting a `LifecycleReceipt` whose rows are the typed `LifecycleRow` union. C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the lifecycle dimension that projection never produces.

Every selecting phase admits its query through `IfcSelector` (`ifc/selector#SELECTOR`), so a malformed selector is a typed `BoundaryFault` at admission, never a silent empty `filter_elements` match feeding `quantify`, and the `SelectorMatch` it returns carries the canonical `filter_string` the receipt keys its evidence on. Full-model 5D walks are a genuinely long native phase, so `run` is `async` and the whole dispatch crosses as `Kernel.of(_lifecycle_kernel, KernelTrait.HOSTILE, idempotent=False)` on `LanePolicy.offload` — SPF source bytes in, mutated-model bytes and receipt out, the live `ifcopenshell.file` rebuilt worker-side because a pybind11 handle never meets the pickle seam, `idempotent=False` dropping the trait's `WORKER` retry so a mutating transaction never re-applies on worker death. That crossing carries the observability its own prose claims: the lane conduit's pickled `tap` rides as the trailing offload arg and the kernel beats `GeometryPulse.LIFECYCLE` once per phase entry, while `bench` measures the whole entry seam through the graduation `bench_seam` fold keyed per phase. `run` threads the graduation `evidence_run` weave under `EvidenceScope.IFC_LIFECYCLE`, `@beartype(conf=FAULT_CONF)` on `_dispatch` binding the contract fence. Evidence graduates under `GeometrySubject.BIM_LIFECYCLE` — the differentiated 5D/4D member distinct from the section-integral and compliance members their owners bind — and crosses to the C# owner system through the one `graduates()`/`GeometryHandoff.wire()` rail. Durable cost-spreadsheet, `ifcpatch.write`, and diff-export writes defer to `python:data/spatial` as the token or product carried on the receipt.

## [01]-[INDEX]

- [02]-[LIFECYCLE]: the quantity, cost, schedule, patch, and diff phases under one `LifecyclePhase`-discriminated owner folding the four IfcOpenShell ecosystem siblings, the `IfcSelector` gate, the pulse and bench observability legs, the parent-side regulatory audit and storage meter, and kind-specific graduation evidence under `BIM_LIFECYCLE`.

## [02]-[LIFECYCLE]

- Owner: `IfcLifecycle` — `@staticmethod` boundary capsule mirroring `IfcAnalysis`, dispatching the phases through one rail-returning `_dispatch` fold over per-phase helpers (`_takeoff`/`_cost`/`_schedule`/`_patch`/`_diff`), never fat in-arm bodies. `LifecyclePhase`, `ScheduleFormat`, `CostReport`, `RuleSet`, and `DiffChange` are closed `StrEnum` discriminants parsed through the one generic `_token[E: StrEnum]` rail — each phase, `ifc4d` parser, `ifc5Dspreadsheet` writer family, `qto.rules` base-quantity set, and `ifcdiff` change class is a rail-validated row, never a raw string fed to `StrEnum(token)`/`rules[str]` that escapes a `ValueError`/`KeyError`. `LifecycleRow` is the `@tagged_union` result row carrying one typed case per phase, never a stringly `dict[str, str]` the toolchain must re-parse.
- Cases: `QUANTITY` (rule-driven take-off over `ifc5d.qto`), `COST` (`ifcopenshell.api.cost` rollup over each `IfcCostItem`), `SCHEDULE` (`ifc4d` `<Format>2Ifc` parser populating `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence`), `PATCH` (`ifcpatch.execute` named recipe over the `recipes` namespace), `DIFF` (`ifcdiff` revision comparison over `deepdiff`) — matched by `match`/`assert_never`, each dispatching to the ecosystem sibling that owns it.
- Entry: `IfcLifecycle.run` takes SPF source bytes, a `LifecyclePhase`, a `spec` whose meaning is phase-fixed — validated selector for `QUANTITY`, cost-schedule GlobalId and report token for `COST`, `<format>:<path>` for `SCHEDULE`, `<recipe>:<json-args>` for `PATCH`, revision path for `DIFF` — the lane, and the `composition` custody key, returning `RuntimeRail[tuple[bytes, LifecycleReceipt]]` through the `evidence_run` weave over the `HOSTILE` kernel crossing: mutating phases ride home as the successor model's SPF bytes (`PATCH` serializes the file `ifcpatch.execute` minted, never the pre-patch input), `DIFF` (read-only) rides `b""`, and a kernel-side `_dispatch` fault crosses home as the typed `BoundaryFault` on the kernel's own rail — the caller flattens the nested rail once, so tag, subject, and fields survive the seam whole. The SPF source digest mints PARENT-side through the one content-addressing owner and threads in as the receipt's identity prefix, so the evidence key names the exact model the phase ran against rather than the caller's spec text alone. `_dispatch` partitions the `spec` once on the `PHASE_DELIMITER` table keyed by every phase including `DIFF`'s empty-delimiter row (whole `spec` as revision path, no `partition("")` fault), never a `.get` default that silently drops a phase. `QUANTITY` binds the `#<rule-set>` token AND the validated selector monadically, so both fault before `quantify` runs. Each arm derives its own `subjects` from the phase's true subject set; `DIFF`'s `population` field separately carries the full compared element count the drift fraction divides against.
- Auto: `QUANTITY`'s `ifc5d.qto.quantify`/`edit_qtos` answers the whole base-quantity schedule keyed by the `qto.rules` table and writes it back as `IfcElementQuantity`; the `RuleSet` vocabulary over those keys is this owner's and the sibling analysis space-program grade composes it rather than transcribing a second copy. `COST`, `PATCH`, and `DIFF` each carry the phase's product as a typed token on the receipt subject — the `CostReport` writer key, the patch product type, the diff change class — so the durable write stays the data boundary's. `_cost` keeps only values whose `AppliedValue` actually resolves and counts the rest on `unpriced`: an unpriced `IfcCostValue` is missing a price, not priced at zero, and an `or 0.0` fold erases a genuine zero-cost item and an unpriced one onto one indistinguishable row. No phase carries an `if/else` value ladder or mints a per-phase class: one fold arm and one helper per row, the owning package bound directly.
- Law: durable evidence lands on the `python:runtime/observability/journal#LEDGER` plane at the async `run` parent beside the receipt emit — one `REGULATORY` `AuditFact` per run beside a `STORAGE` `MeterFact` over the successor bytes the phase wrote, so read-only `DIFF` records its audit line alone. The seat is the parent by two laws at once: recording suspends, and the HOSTILE kernel's whole observability reach is the pickled tap queue, so a worker binds no plane and runs no loop to reach one. The facts mint off the SETTLED receipt, so a refused phase names no run that produced nothing, and the record rail binds into the verdict rather than riding beside it.
- Law: the crossing carries its declared cost — `_lifecycle_kernel` takes the lane conduit's pickled `tap` as its trailing offload arg and beats `GeometryPulse.LIFECYCLE` once per phase entry with `total=1`, because each phase is ONE opaque provider call with no per-element hook and a fabricated denominator would state an extent the provider never publishes; delivery is the lane's lossy law and the worker reaches only the queue proxy. `bench` rides the graduation `bench_seam` fold over the whole `run` crossing — offload, worker rebuild, provider phase, serialization, weave — keyed `rasm.geometry.ifc.costing.<phase>`, so a latency row compares like-for-like across the five phases one dispatch serves, with `bench_terminal` the process-terminal wrap.
- Receipt: receipts carry the census, frames carry the rows — `contribute` emits row and subject counts, the compared population where a phase has one, and the residual ledger, because a whole-model take-off is three fact keys per quantity per element and a flattened row stream turns the runtime receipt into a hundred-thousand-key dict per run. Phase-specific `evidence` keys the subject-relative empty fraction for `QUANTITY`/`SCHEDULE` (a phase producing no rows for a non-empty subject set is a degenerate run keyed `1.0`), the bare no-row fraction plus the `unpriced` count for `COST` (whose `subjects` is the schedule guid and report token, not a produced population), the bare no-row fraction for `PATCH`, and the changed-over-`population` drift fraction for `DIFF` (never changed-over-changed, which clears every ceiling), so a model breaching the caller's ceiling fails the carrier's `admitted` verdict rather than crossing clean. `graduates()` returns `GeometryHandoff.of(BIM_LIFECYCLE, …)` against the per-key ceiling and `frame()` re-projects the typed `LifecycleRow` facts as one phase-homogeneous `EvidenceFrame`, both deriving their own `ContentKey` from the receipt's `spec` through the spine's `evidence_key`, so no caller mints a key for evidence it did not produce.
- Packages: `ifc5d` (`qto.rules`/`quantify`/`edit_qtos` take-off surface only — the `ifc5Dspreadsheet` writer family is the data boundary's), `ifcopenshell` (`api.cost` rollup and in-process model access; selector filtering is the validated gate, never a direct `util.selector.filter_elements` call here), `ifc4d` (`<Format>2Ifc` named parsers), `ifcpatch` (`execute` over the `recipes` namespace; the durable `write` is the data boundary's), `ifcdiff` (`IfcDiff`/`change_register`/`added_elements`/`deleted_elements`; the `export` JSON is the data boundary's), and `geometry`/`expression`/`beartype`/`runtime` (`ContentIdentity` the one content-addressing owner, `LanePolicy`/`pulsed` the conduit, `Bench` the measurement tier, `Journal` with the `AuditFact`/`MeterFact` vocabulary the run's durable evidence records through) per the fence imports; `IfcSelector` is the only `filter_elements` caller.
- Growth: a new quantity rule set is one `RuleSet` row over the upstream `qto.rules` key; a new cost format one `CostReport` row the data boundary binds to its `ifc5Dspreadsheet` writer subclass; a new schedule format one `ScheduleFormat` row binding its `<Format>2Ifc` parser; a new model transformation one `recipe` name in the `ifcpatch.execute` directive; a new diff classification one `DiffChange` row and one `of_register` arm; a new mid-phase fact is one `pulsed` call inside the phase helper that can see it, zero conduit edits; a newly audited run column is one `_evidence` `Change` row, the verb and the meter deriving off the phase and the successor already in hand — zero new surface, no parallel per-phase class family.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy; no ledger, custody, or retention window minted here, the plane arriving bound at the composition root and this owner declaring a `Retain` class alone; no durable store — cost spreadsheet, `ifcpatch.write` serialization, and diff `export` JSON all defer to `python:data/spatial` as the token or product carried on the receipt; no Rhino/GH mutation. Ecosystem siblings import function-local under `# ruff:ignore[import-outside-top-level]` at boundary scope per the manifest import policy, and the `spec` selector crosses the `IfcSelector.filter` validated gate, never a raw `util.selector.filter_elements` passthrough.

```python signature
from collections.abc import Iterable
from enum import StrEnum
from functools import partial
from pathlib import Path
from queue import Queue
from tempfile import TemporaryDirectory
from typing import TYPE_CHECKING, Final, Literal, assert_never

from beartype import beartype
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.json import decode

from rasm.geometry.graduation import (
    EVIDENCE_DOMAIN,
    EvidenceFrame,
    EvidenceScope,
    GeometryHandoff,
    GeometryPulse,
    GeometrySubject,
    PulseBeat,
    bench_seam,
    bench_subject,
    evidence_key,
    evidence_run,
)
from rasm.geometry.ifc.selector import IfcSelector
from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.journal import Actor, Assigned, AuditFact, Fact, Journal, MeterFact, Party, Resource, Retain
from rasm.runtime.lanes import LanePolicy, PulseFact, pulsed
from rasm.runtime.profiles import BenchmarkReceipt
from rasm.runtime.receipts import DEFAULT_SCOPE, OPEN, Receipt, ScopeKey, receipted
from rasm.runtime.workers import Kernel, KernelTrait

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
        # `change_register[guid]` marker dict carries one or more `*_changed` flags at once, so the arm
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

# Lifecycle output crosses on BIM_LIFECYCLE; an unlisted subject fails at the boundary under `ty`.
LIFECYCLE_SUBJECT: Final[GeometrySubject] = GeometrySubject.BIM_LIFECYCLE

# this owner's one name, serving the receipt label and the durable audit actor alike, so a rename cannot leave a
# receipt stream and an evidence-plane actor column under two spellings.
OWNER: Final[str] = "rasm.geometry.ifc.costing"

# One delimiter row is the partition vocabulary key per phase, never a parse-per-phase ladder and never
# a `.get` default that drops a phase; `DIFF`'s empty-delimiter row passes the whole spec as the revision path.
PHASE_DELIMITER: Final[Map[LifecyclePhase, str]] = Map.of_seq([
    (LifecyclePhase.QUANTITY, "#"),
    (LifecyclePhase.COST, ":"),
    (LifecyclePhase.SCHEDULE, ":"),
    (LifecyclePhase.PATCH, ":"),
    (LifecyclePhase.DIFF, ""),
])

# Full ifcdiff relationship axis the audit scopes over, not the ctor's `["geometry"]` default;
# its `"geometry"` leg drives the costly tessellation, the rest fold markers off the model.
DIFF_AXIS: Final[tuple[str, ...]] = ("geometry", "attributes", "type", "property", "container", "aggregate", "classification")

# --- [MODELS] --------------------------------------------------------------------------


class LifecycleReceipt(Struct, frozen=True, gc=False):
    phase: LifecyclePhase
    # run identity — the phase, the parent-minted SPF source digest, and the phase's validated projection — from which
    # `graduates`/`frame` derive their own `ContentKey` through the spine.
    spec: str
    subjects: tuple[str, ...]
    rows: tuple[LifecycleRow, ...]
    # Compared population the DIFF drift fraction divides against, NOT the changed-subject count
    # `subjects` carries (changed-over-changed is the always-1.0 ledger). The other phases ignore it.
    population: int = 0
    # COST values carrying no resolvable AppliedValue: an unpriced item, never a zero-cost one, so it rides its own
    # key against its own ceiling rather than a `0.0` row diluting the schedule rollup.
    unpriced: int = 0

    def evidence(self) -> dict[str, float]:
        # Residual ledger is phase-specific, never a row/subject count that clears against any ceiling.
        match self.phase:
            case LifecyclePhase.QUANTITY | LifecyclePhase.SCHEDULE:
                produced = max(len(self.subjects), 1)
                return {"empty": 1.0 - min(len(self.rows), produced) / produced}
            case LifecyclePhase.COST:
                # COST's `subjects` is the schedule guid and the report token, not a produced population, so a
                # subject-relative fraction grades every schedule with two rows as complete; the honest signals are
                # "no priced row at all" and the unpriced count, each against its own ceiling.
                return {"empty": 0.0 if self.rows else 1.0, "unpriced": float(self.unpriced)}
            case LifecyclePhase.PATCH:
                return {"empty": 0.0 if self.rows else 1.0}
            case LifecyclePhase.DIFF:
                return {"drift": len(self.subjects) / max(self.population, 1)}
            case unreachable:
                assert_never(unreachable)

    def graduates(self, ceiling: dict[str, float]) -> GeometryHandoff:
        # local carrier residual-over-ceiling `admitted` verdict gates; `wire()` is the compute crossing.
        return GeometryHandoff.of(LIFECYCLE_SUBJECT, evidence_key(LIFECYCLE_SUBJECT, self.spec), self.evidence(), ceiling)

    def frame(self) -> "RuntimeRail[EvidenceFrame]":
        # phase rows are homogeneous, so the first row's fact keys ARE the column set; the rollup crosses the
        # geometry-to-data seam as one columnar frame per run — an empty phase frames zero rows, never a fault, and a
        # row set that is NOT homogeneous rails on the port's own width check rather than raising past this producer.
        names = tuple(self.rows[0].facts) if self.rows else ()
        table: dict[str, list[object]] = {
            "phase": [self.phase.value] * len(self.rows),
            **{name: [row.facts[name] for row in self.rows] for name in names},
        }
        return EvidenceFrame.of(LIFECYCLE_SUBJECT, evidence_key(LIFECYCLE_SUBJECT, self.spec), table)

    def contribute(self) -> "Iterable[Receipt]":
        # census, never the rows: a whole-model take-off carries three fact keys per quantity per element, which makes
        # the runtime receipt a hundred-thousand-key dict per run. Per-row evidence crosses as `frame()`; the receipt
        # keeps the counts, the compared population where a phase has one, and the residual ledger the ceiling reads.
        yield Receipt.of(
            OWNER,
            (
                "emitted",
                self.phase.value,
                {"rows": len(self.rows), "subjects": len(self.subjects)}
                | ({"population": float(self.population)} if self.population else {})
                | self.evidence(),
            ),
        )

    @staticmethod
    @receipted(OPEN)  # lifecycle facts carry no secret field, so the runtime keep-all policy binds
    def _emit(receipt: "LifecycleReceipt") -> "LifecycleReceipt":
        # explicit harvest point: the kernel's cleared value is a (bytes, receipt) tuple the weave's own harvest
        # passes through plain, so the receipt slot threads this aspect on the Ok path — the reconstruction convention.
        return receipt


# --- [OPERATIONS] ----------------------------------------------------------------------


def _evidence(successor: bytes, receipt: LifecycleReceipt) -> "Block[Fact]":
    # the durable half of a lifecycle run, minted PARENT-side off the settled receipt: the HOSTILE kernel's whole
    # observability reach is the pickled tap queue by page law, so a record inside it would need a plane no worker
    # binds and a loop no worker runs. Retention is REGULATORY because a take-off, a cost rollup, and a revision
    # diff are the construction record a project reads back years later. The meter carries the SUCCESSOR bytes this
    # run actually wrote, so read-only `DIFF` — which rides home as `b""` — lands its audit line alone rather than
    # charging storage for a comparison that moved nothing. The verb carries the folder's one domain segment beside
    # the phase its own dispatch names, and the target is the run's own `spec` — phase, parent-minted source digest,
    # and validated projection — the same identity `graduates`/`frame` key their evidence on.
    audited = AuditFact(
        action=f"{EVIDENCE_DOMAIN}.{receipt.phase.value}",
        actor=Party(kind=Actor.SERVICE, key=OWNER),
        target=Party(kind="model", key=receipt.spec),
        retention=Retain.REGULATORY,
        change=(Assigned(path="/rows", next=str(len(receipt.rows))), Assigned(path="/subjects", next=str(len(receipt.subjects)))),
    )
    metered = MeterFact(resource=Resource.STORAGE, quantity=len(successor), surface=receipt.spec)
    return Block.of_seq((audited, metered) if successor else (audited,))


def _token[E: StrEnum](vocabulary: type[E], raw: str) -> "RuntimeRail[E]":
    # One generic closed-vocabulary parse for the report, format, AND rule-set tokens: an unknown token
    # is a typed `wire` fault, never a raw `StrEnum(raw)`/`rules[str]` escape. The `raw in vocabulary`
    # value-membership test is the public 3.12+ EnumType contract, no private map.
    return Ok(vocabulary(raw)) if raw in vocabulary else Error(BoundaryFault(wire=(f"lifecycle.{vocabulary.__name__}.{raw}", 0)))


class IfcLifecycle:
    @staticmethod
    async def run(
        source: bytes, phase: LifecyclePhase, spec: str, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[tuple[bytes, LifecycleReceipt]]":
        # 5D walks are the genuinely long native phase, so the whole dispatch crosses HOSTILE with picklable args;
        # idempotent=False drops the trait's WORKER retry — a mutating transaction never re-applies on worker death.
        # The SPF digest mints PARENT-side through the one content-addressing owner so the receipt's identity names
        # the exact model the phase ran against, and the trailing `tap` is the lane conduit's pickled pulse proxy.
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
        # offload nests the kernel's own rail over the crossing rail; flatten ONCE here, then thread the receipt emit.
        # This parent fold is the durable seat too — the nearest async owner of the fact, one hop above a worker that
        # binds no plane. The record rail BINDS into the verdict, so an armed plane refusing a lifecycle fact reaches
        # the caller that owns the governance failure, while an unjournalled composition folds to the lawful no-op.
        settled = rail.bind(lambda inner: inner).map(lambda pair: (pair[0], LifecycleReceipt._emit(pair[1])))
        match settled:
            case Result(tag="ok", ok=(successor, receipt)):
                return (await Journal.record(_evidence(successor, receipt), scope=composition)).map(lambda _landed: (successor, receipt))
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
    ) -> "RuntimeRail[BenchmarkReceipt]":
        # phase-keyed macro-bench over the WHOLE entry crossing — digest, offload, worker rebuild, provider phase,
        # serialization, weave — never an in-kernel probe, which is the pulse's boundary. The subject keys the exact
        # phase row, so a latency row compares like-for-like across the five phases one dispatch serves.
        return bench_seam(
            bench_subject(EvidenceScope.IFC_LIFECYCLE, phase.value),
            partial(IfcLifecycle.run, source, phase, spec, lane, composition=composition),
            rounds=rounds,
            warmup=warmup,
            composition=composition,
        )

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _dispatch(
        model: "ifcopenshell.file", phase: LifecyclePhase, spec: str, source_key: str
    ) -> "RuntimeRail[tuple[LifecycleReceipt, ifcopenshell.file]]":
        # every arm returns (receipt, successor model): the in-place phases and the read-only DIFF thread `model`
        # through, while PATCH threads the file `ifcpatch.execute` minted — the kernel serializes the pair's file.
        # `base` is the run identity every arm extends with its OWN validated projection, so two spellings of one
        # query over one model key one piece of evidence.
        delimiter = PHASE_DELIMITER[phase]
        head, _, tail = spec.partition(delimiter) if delimiter else (spec, "", "")
        base = f"{phase.value}|{source_key}"
        match phase:
            case LifecyclePhase.QUANTITY:
                # Selector rail AND rule-set token both validate before `quantify`: a typo'd
                # `#<rule-set>` is a typed `wire` fault, never a raw `rules[str]` KeyError past the fence.
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
                return Ok((IfcLifecycle._diff(model, head, f"{base}|{head}"), model))
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _takeoff(
        model: "ifcopenshell.file", elements: tuple["ifcopenshell.entity_instance", ...], rule_set: RuleSet, spec: str
    ) -> LifecycleReceipt:
        import ifc5d.qto  # ruff:ignore[import-outside-top-level]

        results = ifc5d.qto.quantify(model, set(elements), ifc5d.qto.rules[rule_set.value])
        ifc5d.qto.edit_qtos(model, results)
        rows = tuple(
            LifecycleRow.of_quantity(element.GlobalId, qto, name, float(value))
            for element, qtos in results.items()
            for qto, quantities in qtos.items()
            for name, value in quantities.items()
        )
        return LifecycleReceipt(LifecyclePhase.QUANTITY, spec, tuple(e.GlobalId for e in results), rows)

    @staticmethod
    def _cost(model: "ifcopenshell.file", schedule_guid: str, report: CostReport, spec: str) -> LifecycleReceipt:
        import ifcopenshell.api.cost  # ruff:ignore[import-outside-top-level]

        schedule = model.by_guid(schedule_guid)
        items = model.by_type("IfcCostItem")
        for item in items:
            ifcopenshell.api.cost.calculate_cost_item_resource_value(model, cost_item=item)
        # Subject carries the closed `CostReport` token, not a leaky `Ifc5DCsvWriter` class name, so the
        # data boundary re-keys its `ifc5Dspreadsheet` writer table on it, never a throwaway temp-dir write.
        # An IfcCostValue whose AppliedValue does not resolve is UNPRICED, so its row drops and the count rides its
        # own receipt field: an `or 0.0` fold publishes an unpriced item and a genuine zero-cost one as one
        # indistinguishable row, and the rollup reads the schedule as complete.
        valued = tuple(
            (item, getattr(value, "AppliedValue", None)) for item in items for value in (item.CostValues or ())
        )
        rows = tuple(
            LifecycleRow.of_cost(item.GlobalId, item.Name or "", float(applied)) for item, applied in valued if isinstance(applied, (int, float))
        )
        return LifecycleReceipt(
            LifecyclePhase.COST,
            spec,
            (schedule.GlobalId, report.value),
            rows,
            unpriced=sum(1 for _, applied in valued if not isinstance(applied, (int, float))),
        )

    @staticmethod
    def _schedule(model: "ifcopenshell.file", fmt: ScheduleFormat, source: str, spec: str) -> LifecycleReceipt:
        import ifc4d.asta2ifc  # ruff:ignore[import-outside-top-level]
        import ifc4d.msproject2ifc  # ruff:ignore[import-outside-top-level]
        import ifc4d.p62ifc  # ruff:ignore[import-outside-top-level]

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
        return LifecycleReceipt(LifecyclePhase.SCHEDULE, spec, tuple(t.GlobalId for t in tasks), rows)

    @staticmethod
    def _patch(model: "ifcopenshell.file", recipe: str, args: str, spec: str) -> tuple[LifecycleReceipt, "ifcopenshell.file"]:
        import ifcopenshell  # ruff:ignore[import-outside-top-level]  boundary-scope: the `ifcopenshell.file()` output match needs the name bound
        import ifcpatch  # ruff:ignore[import-outside-top-level]

        # `execute` returns `ifcopenshell.file | str | None` — patched model, non-IFC product, or in-place.
        # Product TYPE is the wire carry the data boundary keys `ifcpatch.write` on; no throwaway write here.
        output = ifcpatch.execute({
            "input": "",
            "file": model,
            "recipe": recipe,
            "arguments": decode(args.encode(), type=list[object]) if args else [],
        })
        # a file-producing recipe's output IS the successor the kernel serializes; the in-place and non-IFC
        # products thread the mutated input model through.
        match output:
            case ifcopenshell.file() as patched:
                product, successor = output.schema, patched
            case None:
                product, successor = "in-place", model
            case _:
                product, successor = type(output).__name__, model
        rows = (LifecycleRow.of_patch(recipe, product),)
        return LifecycleReceipt(LifecyclePhase.PATCH, spec, (recipe, product), rows), successor

    @staticmethod
    def _diff(model: "ifcopenshell.file", revision_path: str, spec: str) -> LifecycleReceipt:
        import ifcopenshell  # ruff:ignore[import-outside-top-level]  boundary-scope: the `ifcopenshell.open(revision_path)` revision load needs the name bound
        import ifcdiff  # ruff:ignore[import-outside-top-level]

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
        # `model` is the OLD revision, already holding survivors and deleted, so the union adds only the
        # new-only `added_elements`: the drift denominator is `len(old IfcRoot) + len(added)`.
        population = len(model.by_type("IfcRoot")) + len(differ.added_elements)
        return LifecycleReceipt(LifecyclePhase.DIFF, spec, subjects, rows, population=population)


def _serialized(model: "ifcopenshell.file", phase: LifecyclePhase) -> bytes:
    # DIFF reads only; every other phase mutated the worker-local rebuild, so the successor model rides home as SPF
    # bytes through the path-based `file.write`, the one serialization member, under one scoped temp cleanup.
    if phase is LifecyclePhase.DIFF:
        return b""
    with TemporaryDirectory(prefix="ifc-lifecycle-") as work:
        path = Path(work, "model.ifc")
        model.write(str(path))
        return path.read_bytes()


def _lifecycle_kernel(
    source: bytes, phase: LifecyclePhase, spec: str, source_key: str, tap: "Queue[PulseFact | None]"
) -> "RuntimeRail[tuple[bytes, LifecycleReceipt]]":
    # module-level HOSTILE kernel: SPF bytes in, the RAILED (successor bytes, receipt) out — the live ifcopenshell.file
    # rebuilds worker-side and never meets the pickle seam. The dispatch pair carries the phase's successor model, so a
    # PATCH recipe that mints a new file serializes THAT file, never the pre-patch original; a `_dispatch` fault crosses
    # home as the typed BoundaryFault on the kernel's own rail — tag, subject, and fields survive the seam whole, and the
    # caller flattens the nested rail exactly once, never a RuntimeError flattening the fault to text. One beat per
    # phase entry states the extent honestly: each phase is ONE opaque provider call with no per-element hook, so a
    # fabricated denominator would publish progress the provider never reports, and the kernel's whole observability
    # reach stays this pickled queue proxy — `Hooks.fire` runs parent-side in the lane drain, delivery lossy by lane law.
    import ifcopenshell  # ruff:ignore[import-outside-top-level]

    pulsed(tap, GeometryPulse.LIFECYCLE, PulseBeat(stage=phase.value, done=0, total=1))
    model = ifcopenshell.file.from_string(source.decode())
    return IfcLifecycle._dispatch(model, phase, spec, source_key).map(lambda pair: (_serialized(pair[1], phase), pair[0]))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
