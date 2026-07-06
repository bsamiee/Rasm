# [PY_COMPUTE_HANDOFF]

The multi-domain graduation HUB of the Python branch — the tier-0 page every evidence producer composes and no compute page precedes. Two crossings meet here in one direction each: compute-own evidence (solver, symbolic, model-asset, array-layout, unit-law, uncertainty-law, convex-program, artifact) EGRESSES outward across the graduation-evidence wire to the C# managed owner, and geometry-minted evidence ARRIVES as `GeometryHandoff` wire data this hub DECODES — compute authors no geometry vocabulary, imports no `rasm.geometry` symbol, and re-shapes nothing without a geometry ripple. `GraduationReceipt` carries the source package, the `HandoffAxis`, the `ContentKey` evidence key, and the residual ledger across one handoff.

`HandoffAxis` is the `@tagged_union` of handoff kinds, each case carrying the subject its kind names: the `geometry` case the geometry-minted subject decoded off the carrier, the `unit_law`/`uncertainty_law` cases the policy-law subject, the numeric cases their `str` evidence subject. The axis case IS the subject — no parallel `subject: str` field drifts against the discriminant because the subject lives inside the selected case. Every live case has exactly ONE self-wired producer importing this hub downward — `solvers/receipt.md#RECEIPT` hosts the solver-axis projection, `optimization/convex.md#CONVEX`, `analysis/symbolic.md#DERIVATION`, `numerics/array.md#PAYLOAD`, `numerics/quantity.md#QUANTITY`, `experiments/inference.md#BAYESIAN`, and `experiments/model.md#ASSET` self-wire their cases, the `artifact` case's producer is artifacts-side (`artifacts/.planning/core/receipt.md`), and the `geometry` case is decode-only — a case with no producer is a deleted form, never standing prose.

`GraduationReceipt.graduates` is the one admission gate, folding the measured `Ledger` against the per-key `Ceiling` to `RuntimeRail[GraduationReceipt]`. The sibling rejection clauses every evidence owner declares collapse to the one residual-over-ceiling fold parameterized by the axis owner's ledger, never inlined per-site comparisons. Graduation owns a genuine two-stage rail: `boundary(_admit).bind(_clear)` admits the finite ledger and folds the ceiling as a pure domain predicate; the span, the fault fence, and the `@receipted(REDACTION)` egress ride the shared `evidence_run` weave this page also owns — one instrumentation shape for the hub and every producer alike. A handoff proposing a crossing without clearing its ceiling is an `Error(BoundaryFault)` the `_clear` fold returns and the fence's `_convert` records on the span, never an emitted `planned` receipt; the ceiling breach stays a returned domain `Error`, never a raise. Emission is the decorator rail `observability/receipts.md#RECEIPT` declares, never an inline `Signals.emit` threaded through the body.

Graduation is a Python-branch-only concept. The receipt names the wire axis it crosses on and never names a C# interior owner row, mints a C# receipt, or authorizes product runtime behavior; the concrete C# owner consuming each axis is confirmed on the graduation task, not encoded in a routing literal that drifts.

## [01]-[INDEX]

- [01]-[GRADUATION]: the graduation receipt, the handoff-axis union with the inherited geometry contract block, the fenced `GeometryHandoff` carrier-decode ingress with its unknown-subject fault band, the `Ledger`/`Ceiling` `beartype.vale.Is` refinements, the two-stage `_admit`/`_clear` admission rail, the fence-pinned self-describing `planned` fact floor, and the one producer pattern every live axis case wires
- [02]-[EVIDENCE_WEAVE]: the shared `evidence_run(scope, subject, dispatch)` fold — span from the `EvidenceScope` seed table, runtime `boundary`/`async_boundary` fence inside the live span, fenced `@receipted(REDACTION)` harvest, OK close-out — composed by every compute evidence owner in place of page-local tracers, redaction mints, and inline span opens
- [03]-[CROSS_OWNER]: the routing rules gating each axis to its managed owner

## [02]-[GRADUATION]

- Owner: `GraduationReceipt` — the source-package, axis, evidence-key, and residual-ledger `Struct` wired through the runtime `ReceiptContributor` port; `HandoffAxis` the `@tagged_union` of handoff kinds, each case carrying the subject its kind discriminates. The `geometry` case carries the geometry-minted subject as DECODED WIRE DATA — typed against the frozen geometry-owned `GeometrySubject` StrEnum `rasm.geometry.graduation` pins, stated in the fence as the inherited `GEOMETRY_SUBJECTS` contract block, never a compute-authored `Literal` source of truth and never a `rasm.geometry` import; a geometry union change is a geometry ripple, not a compute edit. The `unit_law`/`uncertainty_law` cases carry the policy-law subject, and the numeric cases carry their evidence subject. The axis case is the wire vocabulary; the C# owner that consumes it is never named in the receipt.
- Cases: `HandoffAxis` cases `solver`, `symbolic`, `model_asset`, `array_layout`, `unit_law`, `uncertainty_law`, `geometry`, `convex_program`, and `artifact` — the landed nine-case roster, closed; the roster extends by sibling campaign (the `artifact` case landed exactly so, its producer artifacts-side), never by silent admit. The `geometry` case admits the thirteen geometry-minted wire literals the contract block inherits; the `convex_program` case carries the dual-certificate optimality proof distinct from the `solver` case's first-order convergence verdict; the `unit_law`/`uncertainty_law` cases carry the policy-law subject crossing as policy evidence only.
- Producers: one self-wired `graduates()` producer per live case, each importing this hub downward — `solver`: the `solvers/receipt.md#RECEIPT` `graduate` projection, parameterized over the `(ledger, ceiling, key)` triple every solve owner (`linear`/`nonlinear`/`differential`/`quadrature` via `SolverReceipt`; `design`/`program` via `OutcomeReceipt`; `interval` via `Certificate`) projects from its own receipt — receipt imports none of those downstream types; `convex_program`: `optimization/convex.md#CONVEX` over the KKT-gap certificate; `symbolic`: `analysis/symbolic.md#DERIVATION` under its stability law (`Precision.CERTIFIED` or reproducibly `HEURISTIC` — the page's bar is the admission ceiling); `array_layout`: `numerics/array.md#PAYLOAD` over the cross-backend bit-identity `ParityReceipt` proof; `unit_law`: `numerics/quantity.md#QUANTITY`; `uncertainty_law`: `experiments/inference.md#BAYESIAN`; `model_asset`: `experiments/model.md#ASSET`; `artifact`: artifacts `core/receipt.md` (sibling-owned); `geometry`: decode-only per the carrier-decode row. `numerics/statistics.md#STATISTICS` stays deliberately graduation-free by its own charter and `solvers/sensitivity.md#SENSITIVITY` stays disjoint from study DGSM — preserved boundaries, never gaps; both compose the evidence weave, an observability import that breaches no graduation charter.
- Ceiling law: every graduating family's DEFAULT ceiling is a governed policy row on that family's own policy carrier beside its route table; the hub's caller-supplied tighter row stays the override. An ad-hoc ceiling literal at a `graduates()` call site is a deleted form — the admission bar is seed data with one owner per family.
- Carrier decode: `GraduationReceipt.geometry(source_package, payload)` is the ONE carrier-decode ingress — a fenced msgspec one-shot admission of the `GeometryHandoff.wire()` payload, channel-agnostic by construction (bytes ride the cached `json.Decoder`, in-process builtins ride `convert`; a future wire leg admits through the same fence, no carrier-transport ruling owed). The decoded subject checks against the inherited `GEOMETRY_SUBJECTS` contract block: an out-of-union subject rails as `Error(BoundaryFault)` on the `("graduation.geometry", "unknown-subject")` band — the drift signal a geometry union extension trips until the compute ripple lands — never an unfenced `ValidationError`, never a silent admit. The decoded `(subject, key, measured, ceilings)` feeds the same two-stage rail every compute-own axis rides; the wire hex render parses back to `ContentKey` through `_key` and re-renders byte-identically on the receipt.
- Refinements: `Ledger` and `Ceiling` are the `Annotated[dict[str, float], Is[lambda m: all(isfinite(v) for v in m.values())]]` finiteness aliases the `@beartype(conf=FAULT_CONF)` fence on the inner `_admit` thunk checks, so a `NaN`/`±inf` entry raises the canonical `BeartypeCallHintViolation` INSIDE the `boundary` fence and the `CLASSIFY` `api` row folds it onto the rail rather than slipping past the `measured[k] <= cap` comparison. The sign is deliberately unconstrained so a negated-floor deficit (`experiments/inference.md#BAYESIAN` `neg_min_ess_bulk = -min(ess)`) admits — a `>= 0.0` clause would reject it at the fence; a `-inf` residual silently clearing any ceiling and a `NaN` silently rejecting every ceiling are the two breaches the finiteness `Is` rails before either reaches the comparison. The refinement is the input contract; the ceiling clearance is the admission predicate.
- Entry: `GraduationReceipt.graduates(source_package, axis, evidence_key, measured, ceiling)` is the ONE admission rail, riding the shared weave as `evidence_run(EvidenceScope.HANDOFF, f"graduate.{axis.tag}", rail)` where `rail` is `boundary(f"graduation.{axis.tag}", _admit).bind(_clear)`: the `boundary` fences the `@beartype(conf=FAULT_CONF)`-fenced `_admit` thunk and mints exactly one `RuntimeRail[GraduationReceipt]`; `.bind(_clear)` threads the railed validated ledger through the pure ceiling fold — `Ok(GraduationReceipt(...))` when every measured residual clears its ceiling or `Error(BoundaryFault(boundary=(f"graduation.{tag}", "residual-ceiling")))` when any key exceeds it or a ceiling key is unmeasured; the weave's fenced `@receipted(REDACTION)` harvest streams the `planned` receipt on the cleared `Ok` only, so an emit-time raise folds onto the rail rather than escaping. Three failure concerns stay distinct on three fences: the refinement breach is an exception the `_admit` fence converts, the ceiling rejection is a pure domain predicate returned as `Error` and never a raise, and the emit-time raise is the weave's emit fence to convert — never a thunk returning `RuntimeRail[GraduationReceipt]` the fence re-wraps to `RuntimeRail[RuntimeRail[GraduationReceipt]]` then unwraps through an identity `.bind`.
- Admission split: `_admit(measured, ceiling)` is the `@beartype(conf=FAULT_CONF)`-fenced refinement gate returning the validated `(measured, ceiling)` pair, and `_clear(source_package, axis, evidence_key, validated)` is the one pure total ceiling fold — `Ok(GraduationReceipt(...))` exactly when `measured.keys() >= ceiling.keys()` and `measured[k] <= cap` for every `(k, cap)` ceiling row, else `Error(BoundaryFault(boundary=(f"graduation.{axis.tag}", "residual-ceiling")))`. The per-axis rejection semantics every evidence owner declares — solver residual, convex duality gap, posterior rhat/ess deficit, array reproduction delta — are one residual-over-ceiling predicate parameterized by the ledger the axis owner supplies, never parallel admission bodies; a stricter axis is a tighter ceiling row the caller passes, not a new gate. The fence sits on `_admit`, not `graduates`, so `_clear` folds only the finite ledger the refinement already admitted.
- Axis fold: `_subject` is the one total `match` over the `HandoffAxis` union resolving every case to its `str` subject through a single structural or-pattern — all nine tags binding one `s` capture rather than nine identical `return s` arms — closed by `assert_never` so a new handoff kind is a compile-surfaced gap, never a silent default. The fold is the single place the union is read; `subject`, `span_facts`, and `contribute` all route through it and never probe the union by `getattr(axis, axis.tag)`.
- Receipt: `GraduationReceipt.contribute` returns the one-element `tuple[Receipt, ...]` the `ReceiptContributor` port streams — `Receipt.of(self.source_package, ("planned", subject, facts))` because an admitted handoff is a wire proposal, never an emitted product receipt. The fact floor is FENCE-PINNED SELF-DESCRIBING as the C# graduation gate's decode vocabulary — `FACT_FLOOR = ("axis", "subject", "evidence_key", "residual_count")` plus the spread residual ledger, the `phase` riding the receipt triple: every `planned` facts map carries exactly the floor keys plus the cleared residuals, so the gate attributes and dedupes every crossing without free-form-map guessing — a facts map the gate can neither attribute nor dedupe is a chain break, never a doc gap. The evidence key renders through the canonical `ContentKey.hex` `{value:032x}:{fmt}` form the C# `InterchangeIdentity.Key` contract reads; the residuals ride as native `float` slots the `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()` coerce. `span_facts` is the one bounded `str | int` scalar source both the span and `contribute` read; the full residual ledger rides the receipt facts, never the span. The receipt carries no `bool` admitted flag because its existence IS the admission: a rejected handoff is an `Error` that never reaches `contribute`.
- Packages: `beartype` (`@beartype(conf=FAULT_CONF)` on `_admit` binding the one shared domain conf, `beartype.vale.Is` the `Ledger`/`Ceiling` finiteness refinement), `expression` (`tagged_union`/`case`/`tag`, `Ok`/`Error`/`Result`, `Result.bind` the admission thread, `Map` the redaction table and every folder dispatch table), `msgspec` (`Struct` the receipt and wire mirror, `json.Decoder` the cached bytes decoder, `convert` the builtins arm — the one-shot carrier admission), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span`/`Span.is_recording`/`Span.set_attributes`/`Span.set_status`/`Status`/`StatusCode` — the ONE compute page that names them; every sibling composes `evidence_run`), stdlib `math.isfinite` (the refinement predicate), runtime (`ContentKey`/`ContentKey.hex`, `RuntimeRail`/`BoundaryFault`/`boundary`/`async_boundary`/`FAULT_CONF`, `Receipt`/`ReceiptContributor`/`Redaction`/`@receipted`).
- Growth: a new handoff kind is one `HandoffAxis` case plus one `_subject` match arm plus its sibling-campaign producer; a new geometry subject is a geometry ripple landing one `GEOMETRY_SUBJECTS` row; a stricter admission bar is one tighter ceiling row the caller supplies; a new evidence owner is one `EvidenceScope` row; zero new surface, no per-axis admission body, no second egress beyond the weave's fenced `@receipted` harvest.
- Boundary: the receipt names the wire axis it crosses on, never the managed owner that receives it — no C# `ComputeReceipt`, benchmark claim, source generation, or C# interior owner-row spelling. Compute DECODES every geometry literal and implements no geometry kernel; compute-emitted geometry subjects do not exist — a second graduation direction is geometry's own closed ruling, so an `analysis/spatial.md#SPATIAL` re-graduation of its alpha-shape boundary requires a named consumer plus a compute-owned axis case, never the geometry case. The deleted forms:
  - a compute-authored `type GeometrySubject` declaration — a second source of truth racing the geometry mint; the inherited contract block is data, not a type.
  - a pre-destructured geometry admission with no carrier-decode fence, where the one msgspec ingress decodes the `GeometryHandoff` payload against the contract block.
  - an unfenced `ValidationError` or silent admit on an out-of-union subject, where the `("graduation.geometry", "unknown-subject")` fault band rails it typed.
  - a handoff record claiming production readiness, a Python-only benchmark conclusion, or a C# source-shape claim absent from the C# owner planning.
  - a hard-coded C# `package#owner` routing literal where the wire axis case is the only cross-language vocabulary.
  - a parallel `subject: str` field racing the discriminant, where the axis case carries the subject.
  - a per-axis admission body duplicating the `_clear` fold; an ad-hoc ceiling literal at a `graduates()` call site where the family policy row governs.
  - a `HandoffAxis` case with no self-wired producer — dead vocabulary wearing a rail.
  - a free-form `planned` facts map missing the pinned floor keys, where `FACT_FLOOR` is the C# gate's decode vocabulary.
  - a `Receipt.of("planned", owner, subject, facts)` four-positional call against the runtime two-argument `of(owner, evidence)` contract; a `contribute` returning a single `Receipt` against the `Iterable[Receipt]` port.
  - a `-inf`/`NaN` ledger entry reaching `measured[k] <= cap` where the `Is` finiteness refinement rails it first; a `>= 0.0` sign clause rejecting a legitimate negated-floor deficit.
  - a graduation materializing a receipt without clearing its residual ceiling; an inline `Signals.emit` or `structlog` line where the weave's `@receipted(REDACTION)` aspect owns egress; a raw residual-ledger dump onto the span where only the bounded `span_facts` scalars are admissible.

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
    # the tracer-scope seed table: one member per compute module leaf, the value `compute.<leaf>` —
    # the per-page `trace.get_tracer` literals collapsed onto one vocabulary; growth is one member,
    # and a scope spelling disagreeing with its owning module name is a deleted form.
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

# INHERITED CONTRACT BLOCK — the frozen geometry-owned union `rasm.geometry.graduation.GeometrySubject`
# pins (its `SUBJECTS` export, thirteen wire literals). Decode-only data: compute declares no type and
# imports no geometry symbol; a union change is a geometry ripple landing here as one row.
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
        # the two-stage admission rail riding the shared weave: `boundary(_admit)` mints exactly one
        # `RuntimeRail` over the refinement check, `.bind(_clear)` threads the pure ceiling fold, and
        # the weave owns span, fence, and the fenced `@receipted(REDACTION)` harvest — admission,
        # ceiling rejection, and emission stay one rail with no escape path.
        def rail() -> RuntimeRail[GraduationReceipt]:
            return boundary(f"graduation.{axis.tag}", lambda: GraduationReceipt._admit(measured, ceiling)).bind(
                lambda validated: GraduationReceipt._clear(source_package, axis, evidence_key, validated)
            )

        return evidence_run(EvidenceScope.HANDOFF, f"graduate.{axis.tag}", rail)

    @staticmethod
    def geometry(source_package: str, payload: bytes | Mapping[str, object]) -> RuntimeRail[GraduationReceipt]:
        # the ONE carrier-decode ingress: channel-agnostic msgspec one-shot admission (bytes ride the
        # cached decoder, in-process builtins ride `convert`) against the inherited contract block;
        # an out-of-union subject rails typed — the drift signal a geometry union extension trips
        # until the compute ripple lands — never an unfenced ValidationError, never a silent admit.
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
        # the four bounded `str | int` scalars `Span.set_attributes` admits — exactly the FACT_FLOOR;
        # the full `residuals` ledger (a `dict[str, float]`) rides the receipt facts only.
        return {"axis": self.axis.tag, "subject": self.subject, "evidence_key": self.evidence_key.hex, "residual_count": len(self.residuals)}

    def contribute(self) -> Iterable[Receipt]:
        # the runtime `Receipt.of(owner, evidence)` two-argument contract: the `(Phase, subject, facts)`
        # triple mints the `fact` case at `planned`; the facts map is the pinned floor plus the spread
        # residual ledger as native `float` slots the `enc_hook=repr` renderer serializes uncoerced.
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
        # one or-pattern binds the carried subject off whichever case the tag selects, so the nine
        # tags collapse to one `s` capture; `assert_never` makes a new handoff kind a compile gap.
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
    # Exemption: boundary kernel — the span context manager and the modality probe are the
    # platform-forced seam every compute evidence owner composes instead of re-authoring. A
    # coroutine-function dispatch rides async_boundary and returns the awaitable rail; a sync
    # callable fences inline. Emission runs INSIDE its own fence — one hardening past the geometry
    # weave — so an emit-time render or sink raise folds onto the rail rather than escaping.
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

- Owner: `evidence_run` — the one weave every compute evidence owner threads: span from the `EvidenceScope` seed table, runtime `boundary`/`async_boundary` fence INSIDE the live span (so the faults owner's `_convert` records a provider exception on a recording span and sets ERROR), the fenced `@receipted(REDACTION)` harvest on the cleared `Ok`, and the OK close-out — span-fence-emit-ok stated once here, composed everywhere else. `EvidenceScope` — the closed tracer-scope vocabulary, one member per compute module leaf, its value `compute.<leaf>`; `REDACTION` — the one hub-exported redaction constant every producer's receipts ride.
- Entry: `evidence_run(scope, subject, dispatch)` discriminates modality on the dispatch shape (`iscoroutinefunction`), never a sibling `evidence_run_async`; BOTH arms FLATTEN a railed return through `_flat` so a `lane.offload` composition or a rail-returning selector-gated fold absorbs un-nested while a bare value lifts `Ok`. Emission binds through its own `boundary` — the no-escape guarantee the hub's admission rail demands, granted to every producer.
- Boundary: composing the weave is an observability import, never a graduation admission — the graduation-free owners (`statistics`) ride it without breaching their charter. The deleted forms: a page-local `trace.get_tracer` mint, a page-local `_REDACTION` declaration, an inline `start_as_current_span` beside the owned weave, a hand-authored span-fence-emit sequence, a scope literal disagreeing with its owning module leaf, an `evidence_run_async` sibling.

## [04]-[CROSS_OWNER]

Each axis crosses the graduation-evidence wire under one admission gate: the evidence owner supplies its measured `Ledger` and the `Ceiling` that axis must clear to `GraduationReceipt.graduates`, and a ledger key above its ceiling is an `Error(BoundaryFault)` the `_convert` weave records on the span and propagates on the rail rather than a graduated handoff — no `planned` receipt is emitted for a crossing that did not clear. The wire axis case is the only cross-language vocabulary the receipt carries; the concrete C# owner that consumes an axis is confirmed against the C# owner planning on the graduation task, never an interior owner-row spelling baked into this owner. The C# crossing is outward-only: compute graduates `→` `csharp:Rasm.Compute`; C# never imports back.

- Solver evidence crosses on the `solver` case through the ONE `solvers/receipt.md#RECEIPT` `graduate` projection once the `SolverReceipt` convergence verdict holds; the design and program optima from `optimization/design.md#DESIGN` and `optimization/program.md#PROGRAM` and the interval certificates from `numerics/interval.md#ENCLOSURE` feed the same projection with their own `(ledger, ceiling, key)` triples (a stationary-point or `OptimizeResult` verdict is a convergence verdict, never a separate case).
- Convex-program evidence crosses on the `convex_program` case once the `ConvexReceipt` KKT-gap certificate from `optimization/convex.md#CONVEX` holds — the dual multipliers and the complementary-slackness KKT gap are the global-optimality proof object, a distinct admission from the first-order convergence verdict the `solver` case carries, so a returned point whose gap exceeds the tolerance is an admission rejection.
- Symbolic derivation crosses on the `symbolic` case only after the derivation is stable and reproducible — `Precision.CERTIFIED` or reproducibly `HEURISTIC` under `analysis/symbolic.md#DERIVATION`'s own stability law, that bar the admission ceiling.
- Model-asset evidence crosses on the `model_asset` case only after the `ModelAssetManifest` validation from `experiments/model.md#ASSET` passes.
- Unit-law and uncertainty-law evidence crosses on the `unit_law` and `uncertainty_law` cases as policy evidence only: the `unit_law` case carries the `numerics/quantity.md#QUANTITY` pint unit-algebra dimensional-consistency subject, and the `uncertainty_law` case carries the posterior-diagnostics subject the `InferenceReceipt` convergence diagnostics from `experiments/inference.md#BAYESIAN` gate against the rhat-and-ess residual-limits check, so a quantity failing dimensional consistency or a posterior failing the rhat-and-ess bar is an admission rejection.
- Array-layout evidence crosses on the `array_layout` case once the `ArrayPayload` content key from `numerics/array.md#PAYLOAD` reproduces bit-identically across backends — the proof riding the runtime reproduction `ParityReceipt` against the corpus-admitted `array-layout` fixture.
- Artifact-origin evidence crosses on the `artifact` case through the artifacts-side producer (`artifacts/.planning/core/receipt.md` `graduates`) — sibling-owned, never a compute-side producer obligation.
- Geometry evidence ARRIVES on the `geometry` case as `GeometryHandoff.wire()` data through the one carrier-decode ingress — the single rail geometry-package evidence crosses, each literal mapped to its C# wire-seam owner on the graduation task as a cross-language confirmation. Compute DECODES every literal off the geometry-minted union — the scan trio, the graph pair, `form-finding`, `numerical-primitive`, `mesh-algebra`, the differentiated `bim-compliance`/`bim-lifecycle`/`section-property` evidence classes, and the energy-plane `building-energy`/`thermal-comfort` subjects — and implements none of the geometry kernels; the producing geometry owners and their dual-producer arrangements are geometry's own ledger rows, read there, never mirrored here.
