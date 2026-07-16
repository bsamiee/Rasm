# [PY_COMPUTE_HANDOFF]

The multi-domain graduation HUB of the Python branch — the tier-0 page every evidence producer composes and no compute page precedes. Two crossings meet here in one direction each: compute-own evidence EGRESSES outward across the graduation-evidence wire to the C# managed owner, and geometry-minted evidence ARRIVES as `GeometryHandoff` wire data this hub DECODES — compute authors no geometry vocabulary, imports no `rasm.geometry` symbol, and re-shapes nothing without a geometry ripple. Graduation is a Python-branch-only concept: the receipt names the wire axis it crosses on and never a C# interior owner row, a C# receipt mint, or a product-runtime authorization — the concrete C# owner consuming each axis is confirmed on the graduation task, never a routing literal that drifts.

`GraduationReceipt.graduates` is the one admission gate: the sibling rejection clauses every evidence owner declares collapse to one residual-over-ceiling fold parameterized by the axis owner's ledger, never inlined per-site comparisons. This page also owns the shared `evidence_run` weave — span, fault fence, fenced `@receipted(REDACTION)` harvest — one instrumentation shape for the hub and every producer alike; emission is the decorator rail `observability/receipts.md#RECEIPT` declares, never an inline `Signals.emit`.

## [01]-[INDEX]

- [01]-[GRADUATION]: the receipt, the `HandoffAxis` union with the inherited geometry contract block, the two-stage `_admit`/`_clear` admission rail, and the producer registry.
- [02]-[EVIDENCE_WEAVE]: the shared `evidence_run` fold every compute evidence owner composes in place of page-local tracers and inline span opens.
- [03]-[CROSS_OWNER]: the routing rules gating each axis to its managed owner.

## [02]-[GRADUATION]

- Owner: `GraduationReceipt` — the source-package, axis, evidence-key, and residual-ledger carrier. The axis case IS the subject — no parallel `subject: str` field races the discriminant; the `geometry` case carries its subject as DECODED WIRE DATA typed against the inherited `GEOMETRY_SUBJECTS` contract block, never a compute-authored type racing the geometry mint and never a `rasm.geometry` import — a geometry union change is a geometry ripple landing here as one row. The receipt carries no `bool` admitted flag because its existence IS the admission: a rejected handoff is an `Error` that never reaches `contribute`.
- Cases: the `HandoffAxis` roster extends by sibling campaign, never by silent admit; the `convex_program` case carries the dual-certificate optimality proof distinct from the `solver` case's first-order convergence verdict; the `unit_law`/`uncertainty_law` cases cross as policy evidence only.
- Producers: one self-wired `graduates()` producer per live case, each importing this hub downward — a case with no producer is dead vocabulary wearing a rail. `solver`: the `solvers/receipt.md#RECEIPT` `graduate` projection every solve owner feeds with its own `(ledger, ceiling, key)` triple; `convex_program`: `optimization/convex.md#CONVEX`; `symbolic`: `analysis/symbolic.md#DERIVATION` under its own stability law; `array_layout`: `numerics/array.md#PAYLOAD` over the cross-backend bit-identity proof; `unit_law`: `numerics/quantity.md#QUANTITY`; `uncertainty_law`: `experiments/inference.md#BAYESIAN`; `model_asset`: `experiments/model.md#ASSET`; `artifact`: artifacts `core/receipt.md` (sibling-owned); `geometry`: decode-only. `numerics/statistics.md#STATISTICS` stays deliberately graduation-free by its own charter and `solvers/sensitivity.md#SENSITIVITY` stays disjoint from study DGSM — preserved boundaries, never gaps; composing the evidence weave is an observability import that breaches neither.
- Auto: every graduating family's DEFAULT ceiling is a governed policy row on that family's own carrier beside its route table, the hub's caller-supplied tighter row the override — an ad-hoc ceiling literal at a `graduates()` call site has no owner. Three failure concerns stay distinct on three fences: a refinement breach is an exception the `_admit` fence converts, a ceiling rejection is a pure domain `Error` and never a raise, and an emit-time raise is the weave's emit fence to convert.
- Receipt: an admitted handoff is a `planned` wire proposal, never an emitted product receipt. The fact floor is FENCE-PINNED SELF-DESCRIBING as the C# graduation gate's decode vocabulary — `FACT_FLOOR` plus the spread residual ledger — so the gate attributes and dedupes every crossing without free-form-map guessing; the evidence key renders through the canonical `ContentKey.hex` form the C# `InterchangeIdentity.Key` contract reads.
- Growth: a new handoff kind is one `HandoffAxis` case plus one `_subject` match arm plus its sibling-campaign producer; a new geometry subject is a geometry ripple landing one `GEOMETRY_SUBJECTS` row; a stricter admission bar is one tighter ceiling row the caller supplies; a new evidence owner is one `EvidenceScope` row.
- Boundary: no handoff record claims production readiness, a Python-only benchmark conclusion, or a C# source-shape claim absent from the C# owner planning. Compute-emitted geometry subjects do not exist — a second graduation direction is geometry's own closed ruling, so a compute re-graduation on the geometry axis requires a named consumer plus a compute-owned axis case, never the geometry case.

```python signature
# --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterable, Mapping
from enum import StrEnum
from functools import cache
from inspect import iscoroutinefunction
from math import isfinite
from typing import Annotated, Final, Literal, assert_never

from beartype import beartype
from beartype.vale import Is
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, convert, json
from opentelemetry import trace
from opentelemetry.trace import Span, Status, StatusCode

from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail, async_boundary, boundary
from rasm.runtime.identity import ContentKey
from rasm.runtime.receipts import Receipt, Redaction, receipted

# --- [TYPES] --------------------------------------------------------------------------------


class EvidenceScope(StrEnum):
    # the tracer-scope seed table: one member per compute module leaf, the value `compute.<leaf>`; a scope spelling disagreeing
    # with its owning module name has no owner.
    ARRAY = "compute.array"
    CODEGEN = "compute.codegen"
    CONVEX = "compute.convex"
    DESIGN = "compute.design"
    DIFFERENTIAL = "compute.differential"
    FIELD = "compute.field"
    HANDOFF = "compute.handoff"
    HISTORY = "compute.history"
    INFERENCE = "compute.inference"
    INTERVAL = "compute.interval"
    JIT = "compute.jit"
    LINEAR = "compute.linear"
    MESH = "compute.mesh"
    MODEL = "compute.model"
    NONLINEAR = "compute.nonlinear"
    PROGRAM = "compute.program"
    QUADRATURE = "compute.quadrature"
    QUANTITY = "compute.quantity"
    RECEIPT = "compute.receipt"
    SENSITIVITY = "compute.sensitivity"
    SIGNAL = "compute.signal"
    SPATIAL = "compute.spatial"
    STATISTICS = "compute.statistics"
    STUDY = "compute.study"
    SYMBOLIC = "compute.symbolic"
    TRANSFORM = "compute.transform"


# finiteness-only input refinement the `@beartype(conf=FAULT_CONF)` fence on `_admit` checks; sign
# is unconstrained so a negated-floor deficit (`neg_min_ess_bulk = -min(ess)`) admits.
type Ledger = Annotated[dict[str, float], Is[lambda m: all(isfinite(v) for v in m.values())]]
type Ceiling = Annotated[dict[str, float], Is[lambda m: all(isfinite(v) for v in m.values())]]


@tagged_union(frozen=True)
class HandoffAxis:
    tag: Literal["solver", "symbolic", "model_asset", "array_layout", "unit_law", "uncertainty_law", "geometry", "convex_program", "artifact"] = tag()
    solver: str = case()
    symbolic: str = case()
    model_asset: str = case()
    array_layout: str = case()
    unit_law: str = case()
    uncertainty_law: str = case()
    geometry: str = case()  # decoded wire data typed against the inherited GEOMETRY_SUBJECTS block
    convex_program: str = case()
    artifact: str = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

# INHERITED CONTRACT BLOCK — the frozen geometry-owned union `rasm.geometry.graduation.GeometrySubject` pins (its `SUBJECTS`
# export). Decode-only data: compute declares no type and imports no geometry symbol; a union change lands here as one row.
GEOMETRY_SUBJECTS: Final[frozenset[str]] = frozenset((
    "registration-transform",
    "scan-deviation",
    "reconstructed-mesh",
    "topology-graph",
    "network-graph",
    "form-finding",
    "numerical-primitive",
    "mesh-algebra",
    "bim-compliance",
    "bim-lifecycle",
    "section-property",
    "building-energy",
    "thermal-comfort",
))

# the fence-pinned SELF-DESCRIBING fact floor of every `planned` receipt — the C# graduation gate's
# decode vocabulary; the cleared residual ledger spreads beside the floor and `phase` rides the triple.
FACT_FLOOR: Final[tuple[str, ...]] = ("axis", "subject", "evidence_key", "residual_count")

REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # hub-exported: compute evidence facts carry no secret field

# --- [MODELS] ---------------------------------------------------------------------------


class _GeometryWire(Struct, frozen=True, gc=False):
    # the frozen `GeometryHandoff.wire()` projection — decode-only mirror of the geometry mint;
    # field names are wire law, re-shaped only by a geometry ripple.
    subject: str
    key: str
    measured: dict[str, float]
    ceilings: dict[str, float]
    admitted: bool


_GEOMETRY_DECODER: Final[json.Decoder[_GeometryWire]] = json.Decoder(_GeometryWire)


class GraduationReceipt(Struct, frozen=True):
    source_package: str
    axis: HandoffAxis
    evidence_key: ContentKey
    residuals: dict[str, float]

    @staticmethod
    def graduates(
        source_package: str, axis: HandoffAxis, evidence_key: ContentKey, measured: dict[str, float], ceiling: dict[str, float]
    ) -> RuntimeRail[GraduationReceipt]:
        # the two-stage rail: `boundary(_admit)` mints exactly one `RuntimeRail` over the refinement check, `.bind(_clear)`
        # threads the pure ceiling fold — admission, ceiling rejection, and emission stay one rail with no escape path.
        def rail() -> RuntimeRail[GraduationReceipt]:
            return boundary(f"graduation.{axis.tag}", lambda: GraduationReceipt._admit(measured, ceiling)).bind(
                lambda validated: GraduationReceipt._clear(source_package, axis, evidence_key, validated)
            )

        return evidence_run(EvidenceScope.HANDOFF, f"graduate.{axis.tag}", rail)

    @staticmethod
    def geometry(source_package: str, payload: bytes | Mapping[str, object]) -> RuntimeRail[GraduationReceipt]:
        # the ONE carrier-decode ingress, channel-agnostic: bytes ride the cached decoder, in-process builtins ride `convert`. An
        # out-of-union subject rails typed on the `unknown-subject` band — the drift signal a geometry union extension trips until
        # the compute ripple lands — never an unfenced ValidationError, never a silent admit.
        def decode() -> _GeometryWire:
            return _GEOMETRY_DECODER.decode(payload) if isinstance(payload, bytes) else convert(payload, type=_GeometryWire)

        def admit(wire: _GeometryWire) -> RuntimeRail[GraduationReceipt]:
            if wire.subject not in GEOMETRY_SUBJECTS:
                return Error(BoundaryFault(boundary=("graduation.geometry", "unknown-subject")))
            return GraduationReceipt.graduates(
                source_package, HandoffAxis(geometry=wire.subject), _key(wire.key), wire.measured, wire.ceilings
            )

        return boundary("graduation.geometry", decode).bind(admit)

    @property
    def subject(self) -> str:
        return GraduationReceipt._subject(self.axis)

    @property
    def span_facts(self) -> dict[str, str | int]:
        # exactly the FACT_FLOOR scalars; the full `residuals` ledger rides the receipt facts, never the span.
        return {"axis": self.axis.tag, "subject": self.subject, "evidence_key": self.evidence_key.hex, "residual_count": len(self.residuals)}

    def contribute(self) -> Iterable[Receipt]:
        # the facts map is the pinned floor plus the spread residual ledger as native `float` slots, serialized uncoerced.
        facts: dict[str, object] = {**self.span_facts, **self.residuals}
        return (Receipt.of(self.source_package, ("planned", self.subject, facts)),)

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _admit(measured: Ledger, ceiling: Ceiling) -> tuple[Ledger, Ceiling]:
        # the `Is` finiteness contract fires here inside the `boundary` fence, so `_clear` only ever
        # folds an already-finite ledger; a `NaN`/`±inf` breach rails through the `CLASSIFY` `api` row.
        return (measured, ceiling)

    @staticmethod
    def _clear(source_package: str, axis: HandoffAxis, evidence_key: ContentKey, validated: tuple[Ledger, Ceiling]) -> RuntimeRail[GraduationReceipt]:
        measured, ceiling = validated
        cleared = measured.keys() >= ceiling.keys() and all(measured[k] <= cap for k, cap in ceiling.items())
        return (
            Ok(GraduationReceipt(source_package=source_package, axis=axis, evidence_key=evidence_key, residuals=measured))
            if cleared
            else Error(BoundaryFault(boundary=(f"graduation.{axis.tag}", "residual-ceiling")))
        )

    @staticmethod
    def _subject(axis: HandoffAxis) -> str:
        # one or-pattern binds the carried subject off every case; `assert_never` makes a new handoff kind a compile gap. The fold
        # is the single place the union is read — `subject`, `span_facts`, and `contribute` all route through it.
        match axis:
            case (
                HandoffAxis(tag="solver", solver=s)
                | HandoffAxis(tag="symbolic", symbolic=s)
                | HandoffAxis(tag="model_asset", model_asset=s)
                | HandoffAxis(tag="array_layout", array_layout=s)
                | HandoffAxis(tag="unit_law", unit_law=s)
                | HandoffAxis(tag="uncertainty_law", uncertainty_law=s)
                | HandoffAxis(tag="geometry", geometry=s)
                | HandoffAxis(tag="convex_program", convex_program=s)
                | HandoffAxis(tag="artifact", artifact=s)
            ):
                return s
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _key(render: str) -> ContentKey:
    # the wire crossing identity is the hex render `{value:032x}:{fmt}` (the C# InterchangeIdentity.Key
    # contract); `byte_length` is producer-local and never wire data, so the decoded key carries 0 and
    # every downstream read is the hex render, which round-trips byte-identically.
    digest, _, fmt = render.partition(":")
    return ContentKey(value=int(digest, 16), fmt=fmt, byte_length=0)


@cache
def _tracer(scope: EvidenceScope) -> trace.Tracer:
    return trace.get_tracer(scope.value)


def _close[T](span: Span, result: T) -> T:
    # the clean-exit close-out: OK on the measured span exactly once; ERROR stays the fence _convert's.
    span.set_status(Status(StatusCode.OK))
    return result


@receipted(REDACTION)
def _harvest[T](result: T) -> T:
    # the one receipt-egress step: the aspect streams `contribute()` when the cleared value conforms
    # structurally to the runtime-checkable ReceiptContributor Protocol; a plain value passes through.
    return result


def _flat[T](value: "T | RuntimeRail[T]") -> "RuntimeRail[T]":
    # a lane-offloaded dispatch or a rail-returning fold already carries a fenced rail; the weave
    # flattens it so an offload composes without double-nesting, and a bare value lifts Ok.
    return value if isinstance(value, Result) else Ok(value)


def evidence_run[T](
    scope: EvidenceScope, subject: str, dispatch: Callable[[], T] | Callable[[], Awaitable[T]]
) -> RuntimeRail[T] | Awaitable[RuntimeRail[T]]:
    # Exemption: boundary kernel — the span context manager and the modality probe are the platform-forced seam every compute
    # evidence owner composes instead of re-authoring. Emission runs INSIDE its own fence, so an emit-time render or sink raise
    # folds onto the rail rather than escaping.
    if iscoroutinefunction(dispatch):

        async def woven() -> RuntimeRail[T]:
            with _tracer(scope).start_as_current_span(subject) as span:
                if span.is_recording():
                    span.set_attributes({"scope": scope.value, "subject": subject})
                rail = (await async_boundary(f"{scope}.{subject}", dispatch)).bind(_flat)
                return rail.bind(lambda value: boundary(f"{scope}.emit", lambda: _harvest(value))).map(lambda value: _close(span, value))

        return woven()
    with _tracer(scope).start_as_current_span(subject) as span:
        if span.is_recording():
            span.set_attributes({"scope": scope.value, "subject": subject})
        return (
            boundary(f"{scope}.{subject}", dispatch)
            .bind(_flat)
            .bind(lambda value: boundary(f"{scope}.emit", lambda: _harvest(value)))
            .map(lambda value: _close(span, value))
        )
```

## [03]-[EVIDENCE_WEAVE]

- Owner: `evidence_run` — span from the `EvidenceScope` seed table, the runtime fence INSIDE the live span so a provider exception records on a recording span, the fenced `@receipted(REDACTION)` harvest on the cleared `Ok`, and the OK close-out: span-fence-emit-ok stated once here, composed everywhere else. A page-local tracer mint, a page-local redaction declaration, or an inline span open beside the owned weave has no owner.
- Entry: one entry discriminating modality on the dispatch shape, never an `evidence_run_async` sibling; emission binds through its own `boundary` — the no-escape guarantee the hub's admission rail demands, granted to every producer.

## [04]-[CROSS_OWNER]

Each axis crosses under the one admission gate, and no `planned` receipt is emitted for a crossing that did not clear its ceiling. The C# crossing is outward-only: compute graduates `→` `csharp:Rasm.Compute`; C# never imports back.

- `solver`: the ONE `solvers/receipt.md#RECEIPT` `graduate` projection — the design/program optima and interval certificates feed the same projection with their own triples; a stationary-point or `OptimizeResult` verdict is a convergence verdict, never a separate case.
- `convex_program`: the `optimization/convex.md#CONVEX` KKT-gap certificate — a global-optimality proof distinct from the `solver` convergence verdict, so a returned point whose gap exceeds tolerance is an admission rejection.
- `symbolic`: `analysis/symbolic.md#DERIVATION` under its own stability law, that bar the admission ceiling.
- `model_asset`: crosses only after the `experiments/model.md#ASSET` manifest validation passes.
- `unit_law`/`uncertainty_law`: policy evidence only — the pint dimensional-consistency subject and the posterior-diagnostics subject gated on the rhat-and-ess residual check.
- `array_layout`: crosses once the `numerics/array.md#PAYLOAD` content key reproduces bit-identically across backends.
- `artifact`: the artifacts-side producer, never a compute-side obligation.
- `geometry`: ARRIVES as `GeometryHandoff.wire()` data through the one carrier-decode ingress; compute decodes every literal off the geometry-minted union and implements none of the geometry kernels — the producing geometry owners are geometry's own ledger rows, read there, never mirrored here.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
