# [PY_COMPUTE_HANDOFF]

Compute graduation is the tier-0 hub every compute evidence producer composes. It sends admitted evidence outward to the C# managed owner. Geometry retains its canonical results and observes them through its own runtime seam; this hub imports no `rasm.geometry` symbol and authors no geometry vocabulary. `Graduation` names the Python wire axis, while the graduation task confirms its concrete .NET consumer.

`Graduation.graduates` is the one admission gate: the sibling rejection clauses every evidence owner declares collapse to one residual-over-ceiling fold parameterized by the axis owner's ledger, never inlined per-site comparisons. `evidence_run` is this hub's binding of the runtime `measured` weave — the compute `EvidenceScope` vocabulary applied once — so span, fault fence, rail flatten, and status close stay the `runtime/observability/observe#OBSERVE` owner's mechanics and compute authors no second instrumentation shape.

## [01]-[INDEX]

- [02]-[GRADUATION]: `Graduation` carries the `HandoffAxis` union through admission and durable recording.
- [03]-[EVIDENCE_WEAVE]: `evidence_run` binds shared runtime observation and the optional `StageTap` slot.
- [04]-[CROSS_OWNER]: Cross-owner rules gate each axis to its managed owner.

## [02]-[GRADUATION]

- Owner: `Graduation` carries source package, axis, evidence key, and residual ledger. `ComputeLeg` and `RAISES` sit here because every compute producer imports this hub downward. Moving either roster to a producer inverts the folder strata. Its axis case supplies the subject, so no parallel `subject: str` field can race the discriminant. Rejection returns `Error`; admission alone mints the value.
- Cases: the `HandoffAxis` roster extends by sibling campaign, never by silent admit; the `convex_program` case carries the dual-certificate optimality proof distinct from the `solver` case's first-order convergence verdict; the `unit_law`/`uncertainty_law` cases cross as policy evidence only.
- Producers: one self-wired `graduates()` producer per live case, each importing this hub downward — a case with no producer is dead vocabulary wearing a rail. `solver`: the `solvers/solve#SOLVE` `graduate` projection every solve owner feeds with its `Solve` or prepared ledger, its family ceiling row, and its key; `convex_program`: `optimization/convex#CONVEX`; `symbolic`: `analysis/symbolic#DERIVATION` under its own stability law; `array_layout`: `numerics/array#PAYLOAD` over the cross-backend bit-identity proof; `unit_law`: `numerics/quantity#QUANTITY`; `uncertainty_law`: `experiments/inference#BAYESIAN`; `model_asset`: `experiments/model#ASSET`; `artifact`: the artifacts graduating producer (sibling-owned). `numerics/statistics#STATISTICS` stays deliberately graduation-free by its own charter and `solvers/sensitivity#SENSITIVITY` stays disjoint from study DGSM — preserved boundaries, never gaps; composing the evidence weave is an observability import that breaches neither.
- Law: every graduation admission reaches the `python:runtime/observability/journal#LEDGER` plane, and `graduates_async` is its ONE seat — the awaitable twin this pure fold mints over the band hop, since recording suspends and `graduates` opens no loop. BOTH verdicts record through one `_evidence` fold: `REGULATORY` at the admitted tee, because a cleared crossing is the record a C# consumer acts on years later, and `OPERATIONAL` at the refused tee, because a bar that held is incident-window evidence and never a seven-year hold. Recording both verdicts distinguishes absence from refusal. Admitted and refused rails differ by arm: admission BINDS, while refusal rides BESIDE the fault so a journal failure cannot hide the domain verdict. Subjects stay empty — an evidence key names a computation, never a data subject — and no meter rides the leg, the crossing's cpu being the resource band's one charge. `EVIDENCE_DOMAIN` derives off this page's own scope spelling and is the one domain segment every compute audit verb carries.
- Auto: every graduating family's DEFAULT ceiling is a governed policy row on that family's own carrier beside its route table, the hub's caller-supplied tighter row the override — an ad-hoc ceiling literal at a `graduates()` call site has no owner. Three failure concerns stay distinct on three fences: a refinement breach is an exception the `_admit` fence converts, a ceiling rejection is a pure domain `Error` and never a raise, and an emit-time raise is the weave's emit fence to convert.
- Output: an admitted handoff is a `planned` wire proposal, never an emitted product; `_witnessed` stamps the admitting span with the value's `attributes` and the residual ledger namespaced under `residual.`, so a ledger metric can never shadow the `axis`/`subject`/`evidence_key`/`residual_count` floor a board filters on, and the evidence key renders through the canonical `ContentKey.hex` form the C# `InterchangeIdentity.Key` contract reads.
- Growth: a new handoff kind is one `HandoffAxis` case, one `_subject` match arm, and its sibling-campaign producer, its audit verb deriving with it; a new compute module is one `ComputeLeg` member beside its `EvidenceScope` row; a new refusal is one `FaultRow` anchor in `RAISES`, its coordinates its declared `slots`; a newly instrumented long fold is one producer-owned stage roster and the optional `evidence_run` slot, never a weave edit; a newly audited admission column is one `_evidence` `Change` row; a stricter admission bar is one tighter ceiling row the caller supplies; a new evidence owner is one `EvidenceScope` row; a new embedded composition is one `ScopeKey` the caller threads, never a sibling registry.
- Boundary: no handoff record claims production readiness, a Python-only benchmark conclusion, or a C# source-shape claim absent from the .NET owner planning. No ledger, custody, or retention window is minted here — the plane arrives bound at the composition root and this owner declares a `Retain` class alone. Geometry stays outside the compute graduation axis and retains the canonical result each operation produces.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Mapping
from enum import StrEnum
from math import isfinite
from queue import Queue
from typing import Annotated, Final, Literal, assert_never

from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from expression import Error, Nothing, Ok, Option, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace

from rasm.runtime.faults import FAULT_CONF, TERMINAL, BoundaryFault, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.identity import ContentKey
from rasm.runtime.journal import Actor, Assigned, AuditFact, Change, Cleared, Fact, Journal, Party, Retain
from rasm.runtime.lanes import PulseFact, pulsed
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey, measured

from rasm.runtime.hooks import HookId, StageMark

lazy from rasm.compute.graduation.observability import ComputePoint, GraduationAdmitted, GraduationRejected, fired, ledgered, stage_point

# --- [TYPES] ----------------------------------------------------------------------------


class EvidenceScope(StrEnum):
    ARRAY = "rasm.compute.array"
    CODEGEN = "rasm.compute.codegen"
    CONVEX = "rasm.compute.convex"
    DESIGN = "rasm.compute.design"
    DIFFERENTIAL = "rasm.compute.differential"
    FIELD = "rasm.compute.field"
    HANDOFF = "rasm.compute.handoff"
    HISTORY = "rasm.compute.history"
    INFERENCE = "rasm.compute.inference"
    INTERVAL = "rasm.compute.interval"
    JIT = "rasm.compute.jit"
    LINEAR = "rasm.compute.linear"
    MESH = "rasm.compute.mesh"
    MODEL = "rasm.compute.model"
    NONLINEAR = "rasm.compute.nonlinear"
    PROGRAM = "rasm.compute.program"
    QUADRATURE = "rasm.compute.quadrature"
    QUANTITY = "rasm.compute.quantity"
    SOLVE = "rasm.compute.solve"
    SENSITIVITY = "rasm.compute.sensitivity"
    SIGNAL = "rasm.compute.signal"
    SPATIAL = "rasm.compute.spatial"
    STATISTICS = "rasm.compute.statistics"
    STUDY = "rasm.compute.study"
    SYMBOLIC = "rasm.compute.symbolic"
    TRANSFORM = "rasm.compute.transform"


class ComputeLeg(StrEnum):
    SOLVE = "compute.solvers.solve"
    LINEAR = "compute.solvers.linear"
    NONLINEAR = "compute.solvers.nonlinear"
    QUADRATURE = "compute.solvers.quadrature"
    DIFFERENTIAL = "compute.solvers.differential"
    SENSITIVITY = "compute.solvers.sensitivity"
    MESH = "compute.solvers.mesh"
    FIELD = "compute.solvers.field"
    DESIGN = "compute.optimization.design"
    PROGRAM = "compute.optimization.program"
    CONVEX = "compute.optimization.convex"
    STUDY = "compute.experiments.study"
    HISTORY = "compute.experiments.history"
    INFERENCE = "compute.experiments.inference"
    MODEL = "compute.experiments.model"
    ARRAY = "compute.numerics.array"
    JIT = "compute.numerics.jit"
    INTERVAL = "compute.numerics.interval"
    QUANTITY = "compute.numerics.quantity"
    STATISTICS = "compute.numerics.statistics"
    SIGNAL = "compute.analysis.signal"
    TRANSFORM = "compute.analysis.transform"
    SYMBOLIC = "compute.analysis.symbolic"
    SPATIAL = "compute.analysis.spatial"
    HANDOFF = "compute.graduation.handoff"
    CODEGEN = "compute.graduation.codegen"
    OBSERVABILITY = "compute.graduation.observability"


type Ledger = Annotated[dict[str, float], Is[lambda m: all(isfinite(v) for v in m.values())]]
type Ceiling = Annotated[dict[str, float], Is[lambda m: all(isfinite(v) for v in m.values())]]
type SpanFacts = Mapping[str, str | int | float | bool]


@tagged_union(frozen=True)
class HandoffAxis:
    tag: Literal["solver", "symbolic", "model_asset", "array_layout", "unit_law", "uncertainty_law", "convex_program", "artifact"] = tag()
    solver: str = case()
    symbolic: str = case()
    model_asset: str = case()
    array_layout: str = case()
    unit_law: str = case()
    uncertainty_law: str = case()
    convex_program: str = case()
    artifact: str = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

EVIDENCE_DOMAIN: Final[str] = EvidenceScope.HANDOFF.value.split(".", 2)[1]

_COORDINATES: Final[frozenset[str]] = frozenset({"tag", "subject", "leg"})

# --- [TABLES] ---------------------------------------------------------------------------

ADMIT_LEDGER: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.HANDOFF, point="admit", arm="api", defect="non-finite-ledger", retriability=TERMINAL
)
CEILING: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.HANDOFF, point="clear", arm="boundary", defect="residual-ceiling", retriability=TERMINAL, slots=("axis",)
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([ADMIT_LEDGER, CEILING]))

# --- [MODELS] ---------------------------------------------------------------------------


class StageTap(Struct, frozen=True, gc=False):
    point: HookId
    tap: Queue[PulseFact | None]
    total: Option[int] = Nothing

    @staticmethod
    def of(scope: EvidenceScope, tap: Queue[PulseFact | None], total: Option[int] = Nothing) -> "StageTap":
        return StageTap(point=stage_point(scope), tap=tap, total=total)

    def beat[S: StrEnum](self, stage: S, done: int) -> None:
        pulsed(self.tap, self.point, StageMark(stage=stage.value, done=done, total=self.total))

    def facts(self) -> SpanFacts:
        return self.total.map(lambda extent: {"stage_total": extent}).default_value({})


class Graduation(Struct, frozen=True):
    source_package: str
    axis: HandoffAxis
    evidence_key: ContentKey
    residuals: dict[str, float]

    @staticmethod
    def graduates(
        source_package: str, axis: HandoffAxis, evidence_key: ContentKey, measured: dict[str, float], ceiling: dict[str, float],
        composition: ScopeKey = DEFAULT_SCOPE,
    ) -> RuntimeRail[Graduation]:
        def rail() -> RuntimeRail[Graduation]:
            return (
                boundary(ADMIT_LEDGER, lambda: Graduation._admit(measured, ceiling), catch=BeartypeCallHintViolation)
                .bind(lambda validated: Graduation._clear(source_package, axis, evidence_key, validated))
                .map(lambda cleared: Graduation._witnessed(cleared, composition))
                .map_error(lambda fault: Graduation._refused(fault, composition))
            )

        floor: SpanFacts = {"axis": axis.tag, "evidence_key": evidence_key.hex, "residual_count": len(measured)}
        return evidence_run(EvidenceScope.HANDOFF, f"graduate.{axis.tag}", rail, facts=floor, composition=composition)

    @staticmethod
    async def graduates_async(
        source_package: str, axis: HandoffAxis, evidence_key: ContentKey, measured: dict[str, float], ceiling: dict[str, float],
        composition: ScopeKey = DEFAULT_SCOPE,
    ) -> RuntimeRail[Graduation]:
        match Graduation.graduates(source_package, axis, evidence_key, measured, ceiling, composition):
            case Result(tag="ok") as cleared:
                return (await Journal.record(_evidence(source_package, axis, cleared), scope=composition)).bind(lambda _landed: cleared)
            case refused:
                await Journal.record(_evidence(source_package, axis, refused), scope=composition)
                return refused

    @property
    def subject(self) -> str:
        return Graduation._subject(self.axis)

    @property
    def attributes(self) -> dict[str, str | int | float]:
        return {
            "axis": self.axis.tag,
            "subject": self.subject,
            "evidence_key": self.evidence_key.hex,
            "residual_count": len(self.residuals),
            **{f"residual.{name}": value for name, value in self.residuals.items()},
        }

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _admit(measured: Ledger, ceiling: Ceiling) -> tuple[Ledger, Ceiling]:
        return (measured, ceiling)

    @staticmethod
    def _clear(source_package: str, axis: HandoffAxis, evidence_key: ContentKey, validated: tuple[Ledger, Ceiling]) -> RuntimeRail[Graduation]:
        measured, ceiling = validated
        cleared = measured.keys() >= ceiling.keys() and all(measured[k] <= cap for k, cap in ceiling.items())
        return (
            Ok(Graduation(source_package=source_package, axis=axis, evidence_key=evidence_key, residuals=measured))
            if cleared
            else Error(CEILING.raised(axis.tag))
        )

    @staticmethod
    def _witnessed(cleared: "Graduation", composition: ScopeKey) -> "Graduation":
        trace.get_current_span().set_attributes(cleared.attributes)
        fired(ComputePoint.ADMITTED, GraduationAdmitted(axis=cleared.axis.tag, subject=cleared.subject, evidence_key=cleared.evidence_key.hex, residual_count=len(cleared.residuals)), composition)
        return cleared

    @staticmethod
    def _refused(fault: BoundaryFault, composition: ScopeKey) -> BoundaryFault:
        fired(ComputePoint.REJECTED, GraduationRejected(subject=fault.subject, tag=fault.tag, detail=_detail(fault)), composition)
        return fault

    @staticmethod
    def _subject(axis: HandoffAxis) -> str:
        match axis:
            case (
                HandoffAxis(tag="solver", solver=s)
                | HandoffAxis(tag="symbolic", symbolic=s)
                | HandoffAxis(tag="model_asset", model_asset=s)
                | HandoffAxis(tag="array_layout", array_layout=s)
                | HandoffAxis(tag="unit_law", unit_law=s)
                | HandoffAxis(tag="uncertainty_law", uncertainty_law=s)
                | HandoffAxis(tag="convex_program", convex_program=s)
                | HandoffAxis(tag="artifact", artifact=s)
            ):
                return s
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _evidence(source_package: str, axis: HandoffAxis, settled: RuntimeRail[Graduation]) -> Block[Fact]:
    match settled:
        case Result(tag="ok", ok=cleared):
            row: tuple[Retain, str, tuple[Change, ...]] = (
                Retain.REGULATORY,
                cleared.subject,
                (
                    Assigned(path="/evidence_key", next=cleared.evidence_key.hex),
                    Assigned(path="/residual_count", next=str(len(cleared.residuals))),
                ),
            )
        case Result(tag="error", error=fault):
            row = (Retain.OPERATIONAL, Graduation._subject(axis), (Cleared(path="/admitted", prior=_detail(fault)),))
        case _ as unreachable:
            assert_never(unreachable)
    retention, subject, change = row
    return Block.singleton(
        AuditFact(
            action=f"{EVIDENCE_DOMAIN}.{axis.tag}",
            actor=Party(kind=Actor.SERVICE, key=source_package),
            target=Party(kind="axis", key=subject),
            retention=retention,
            change=change,
        )
    )


def _detail(fault: BoundaryFault) -> str:
    return ";".join(f"{name}={value}" for name, value in fault.facts().items() if name not in _COORDINATES)


def evidence_run[T](
    scope: EvidenceScope, subject: str, dispatch: Callable[[], T] | Callable[[], Awaitable[T]], facts: SpanFacts = Map.empty(),
    composition: ScopeKey = DEFAULT_SCOPE, stage: Option[StageTap] = Nothing,
) -> RuntimeRail[T] | Awaitable[RuntimeRail[T]]:
    return measured(
        scope.value, subject, ledgered(scope, subject, dispatch, composition=composition, stage=stage),
        {**facts, **stage.map(StageTap.facts).default_value({})},
    )
```

## [03]-[EVIDENCE_WEAVE]

- Owner: `evidence_run` binds the runtime `measured` weave to compute policy — the `EvidenceScope` seed table names the span scope — and every producer composes this binding, so a page-local tracer mint or an inline span open beside it has no owner. Span, fence, rail flatten, and the two-sided status close are the `runtime/observability/observe#OBSERVE` owner's mechanics, composed here, never re-authored; a producer's RESULT facts land on that same span through the result type's own `attributes` stamp at its factory, so the trace filters on what the value carries.
- Spelling: every member's value is `rasm.compute.<leaf>` and the member NAME is the only handle a producer spells, so a scope reaches a producer as `EvidenceScope.<X>` and its value only where the weave stamps the tracer or the `Graduation.source_package`. Reverse `EvidenceScope(f"...{tag}")` lookup reconstructs a spelling the enum already owns and re-breaks on the next root change, so a tag-keyed consumer carries a `Map[str, EvidenceScope]` row instead. Estate rooting keeps every compute span in the namespace shared by sibling branches, observability points, and instruments.
- Cases: every `EvidenceScope` member holds at least one composed consumer — a span emitter through this weave, a `source_package` spelling through `.value`, or both; a member with neither is deleted, so the seed table can never carry dead vocabulary.
- Entry: one entry discriminating modality on the dispatch shape, never an `evidence_run_async` sibling; `facts` threads each producer's call-time discriminants — problem size, route, backend, precision — onto the recording span at open, and the result's `attributes` land on it at the mint, so a trace filters on the same evidence the value carries; the dispatch fence at the runtime owner is the no-escape guarantee the hub's admission rail demands, granted to every producer. That guarantee survives the narrowed interior `catch=` sets exactly because it lives HERE: the weave fence is the outermost catch-all a producer plane may hold, and every seam beneath it names the provider classes it reaches.
- Stage: `StageTap` is the optional mid-operation slot, and the mark carrier is the hook registry's composed `StageMark`; a compute clone duplicates one conduit payload. Each long fold owns its closed milestone `StrEnum`, erased at the conduit so every fold shares one point payload type. `Option` carries the census, letting an indeterminate extent remain absent instead of publishing a false zero.
- Ledger: the binding weaves the `graduation/observability.md` `ledgered` leg around every dispatch — enter fact, resource band off the runtime `Cost` bracket, exit fact on both the settled and raised arms — so the point rail and the resource ledger reach every producer through the one binding; point rows, payload family, measure mapping, and taps are that page's, composed here, never re-authored. `composition` threads from the caller through this binding into `ledgered` and through `graduates` into both admission fires, so an embedded second composition's lifecycle and admission facts reach the points IT registered; the key defaults `DEFAULT_SCOPE`, so the root call shape stays scope-free.

## [04]-[CROSS_OWNER]

Each axis crosses under the one admission gate, and no `Graduation` value exists for a crossing that did not clear its ceiling. C# crossing stays outward-only: compute graduates `→` `dotnet:Rasm.Compute`; C# never imports back.

- `solver`: rides the ONE `solvers/solve#SOLVE` `graduate` projection — the solve routes' `Solve` values, the design/program optima through the shared `Optimum.graduates`, and the interval certificates feed it with their own ledgers and family ceiling rows; a stationary-point or `OptimizeResult` verdict is a convergence verdict, never a separate case.
- `convex_program`: carries the `optimization/convex#CONVEX` KKT-gap certificate — a global-optimality proof distinct from the `solver` convergence verdict, so a returned point whose gap exceeds tolerance is an admission rejection.
- `symbolic`: `analysis/symbolic#DERIVATION` under its own stability law, that bar the admission ceiling.
- `model_asset`: crosses only after the `experiments/model#ASSET` manifest validation passes.
- `unit_law`/`uncertainty_law`: policy evidence only — the pint dimensional-consistency subject and the posterior-diagnostics subject gated on the rhat-and-ess residual check.
- `array_layout`: crosses once the `numerics/array#PAYLOAD` content key reproduces bit-identically across backends.
- `artifact`: stays the artifacts-side producer, never a compute-side obligation.
- Geometry retains the actual result each operation produces and uses geometry's `evidence_run` binding for runtime observation. It neither enters `HandoffAxis` nor routes through this hub.

## [05]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
