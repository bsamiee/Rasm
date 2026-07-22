# [PY_COMPUTE_HANDOFF]

Multi-domain graduation HUB of the Python branch — the tier-0 page every evidence producer composes and no compute page precedes. Two crossings meet here in one direction each: compute-own evidence EGRESSES outward across the graduation-evidence wire to the C# managed owner, and geometry-minted evidence ARRIVES as `GeometryHandoff` wire data this hub DECODES — compute authors no geometry vocabulary, imports no `rasm.geometry` symbol, and re-shapes nothing without a geometry ripple. Graduation is a Python-branch-only concept: the receipt names the wire axis it crosses on and never a C# interior owner row, a C# receipt mint, or a product-runtime authorization — the concrete C# owner consuming each axis is confirmed on the graduation task, never a routing literal that drifts.

`GraduationReceipt.graduates` is the one admission gate: the sibling rejection clauses every evidence owner declares collapse to one residual-over-ceiling fold parameterized by the axis owner's ledger, never inlined per-site comparisons. `evidence_run` is this hub's binding of the runtime `measured` weave — the compute `EvidenceScope` vocabulary and the hub `REDACTION` row applied once — so span, fault fence, rail flatten, and fenced harvest stay the `runtime/observability/receipts#RECEIPT` owner's mechanics and compute authors no second instrumentation shape.

## [01]-[INDEX]

- [01]-[GRADUATION]: the receipt, the `HandoffAxis` union with the inherited geometry contract block, the two-stage `_admit`/`_clear` admission rail, and the producer registry.
- [02]-[EVIDENCE_WEAVE]: the shared `evidence_run` fold every compute evidence owner composes in place of page-local tracers and inline span opens.
- [03]-[CROSS_OWNER]: the routing rules gating each axis to its managed owner.

## [02]-[GRADUATION]

- Owner: `GraduationReceipt` — the source-package, axis, evidence-key, and residual-ledger carrier. Its axis case IS the subject — no parallel `subject: str` field races the discriminant; the `geometry` case carries its subject as DECODED WIRE DATA typed against the inherited `GEOMETRY_SUBJECTS` contract block, never a compute-authored type racing the geometry mint and never a `rasm.geometry` import — a geometry union change is a geometry ripple landing here as one row. No `bool` admitted flag rides the receipt because its existence IS the admission: a rejected handoff is an `Error` that never reaches `contribute`.
- Cases: the `HandoffAxis` roster extends by sibling campaign, never by silent admit; the `convex_program` case carries the dual-certificate optimality proof distinct from the `solver` case's first-order convergence verdict; the `unit_law`/`uncertainty_law` cases cross as policy evidence only.
- Producers: one self-wired `graduates()` producer per live case, each importing this hub downward — a case with no producer is dead vocabulary wearing a rail. `solver`: the `solvers/receipt.md#RECEIPT` `graduate` projection every solve owner feeds with its receipt or prepared ledger, its family ceiling row, and its key; `convex_program`: `optimization/convex.md#CONVEX`; `symbolic`: `analysis/symbolic.md#DERIVATION` under its own stability law; `array_layout`: `numerics/array.md#PAYLOAD` over the cross-backend bit-identity proof; `unit_law`: `numerics/quantity.md#QUANTITY`; `uncertainty_law`: `experiments/inference.md#BAYESIAN`; `model_asset`: `experiments/model.md#ASSET`; `artifact`: artifacts `core/receipt.md` (sibling-owned); `geometry`: decode-only. `numerics/statistics.md#STATISTICS` stays deliberately graduation-free by its own charter and `solvers/sensitivity.md#SENSITIVITY` stays disjoint from study DGSM — preserved boundaries, never gaps; composing the evidence weave is an observability import that breaches neither.
- Auto: every graduating family's DEFAULT ceiling is a governed policy row on that family's own carrier beside its route table, the hub's caller-supplied tighter row the override — an ad-hoc ceiling literal at a `graduates()` call site has no owner. Three failure concerns stay distinct on three fences: a refinement breach is an exception the `_admit` fence converts, a ceiling rejection is a pure domain `Error` and never a raise, and an emit-time raise is the weave's emit fence to convert.
- Receipt: an admitted handoff is a `planned` wire proposal, never an emitted product receipt. Its fact floor is FENCE-PINNED SELF-DESCRIBING as the C# graduation gate's decode vocabulary — `FACT_FLOOR` and the residual ledger namespaced under `residual.`, so a ledger metric can never shadow a floor name — and the gate attributes and dedupes every crossing without free-form-map guessing; the evidence key renders through the canonical `ContentKey.hex` form the C# `InterchangeIdentity.Key` contract reads.
- Growth: a new handoff kind is one `HandoffAxis` case, one `_subject` match arm, and its sibling-campaign producer; a new geometry subject is a geometry ripple landing one `GEOMETRY_SUBJECTS` row; a stricter admission bar is one tighter ceiling row the caller supplies; a new evidence owner is one `EvidenceScope` row.
- Boundary: no handoff record claims production readiness, a Python-only benchmark conclusion, or a C# source-shape claim absent from the C# owner planning. Compute-emitted geometry subjects do not exist — a second graduation direction is geometry's own closed ruling, so a compute re-graduation on the geometry axis requires a named consumer and a compute-owned axis case, never the geometry case.

```python signature
# --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
import re
from collections.abc import Awaitable, Callable, Iterable, Mapping
from enum import StrEnum
from math import isfinite
from typing import Annotated, Final, Literal, assert_never

from beartype import beartype
from beartype.vale import Is
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, convert, json
from opentelemetry import propagate, trace

from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail, boundary
from rasm.runtime.identity import ContentKey
from rasm.runtime.receipts import Receipt, Redaction, measured

lazy from rasm.compute.graduation.observability import ADMITTED, REJECTED, GraduationAdmitted, GraduationRejected, fired, ledgered  # lazy breaks the hub<->observability stratum cycle; proxies reify at first dispatch

# --- [TYPES] --------------------------------------------------------------------------------


class EvidenceScope(StrEnum):
    # scope seed table: one member per compute module leaf, value `compute.<leaf>`, owning BOTH the tracer scope and the receipt
    # `source_package` spelling — producers compose `EvidenceScope.<X>.value`, never a bare literal, so a drifted spelling has no owner.
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
type SpanFacts = Mapping[str, str | int | float | bool]


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

# fence-pinned SELF-DESCRIBING fact floor of every `planned` receipt — the C# graduation gate's
# decode vocabulary; the cleared residual ledger rides beside the floor under `residual.`-prefixed
# keys, so a metric name can never collide with a floor slot, and `phase` rides the triple.
FACT_FLOOR: Final[tuple[str, ...]] = ("axis", "subject", "evidence_key", "residual_count")

REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # hub-exported: compute evidence facts carry no secret field

# --- [MODELS] ---------------------------------------------------------------------------


class _GeometryWire(Struct, frozen=True, gc=False, forbid_unknown_fields=True):
    # frozen `GeometryHandoff.wire()` projection — decode-only mirror of the geometry mint;
    # field names are wire law, re-shaped only by a geometry ripple, and `forbid_unknown_fields`
    # is the widen tripwire: a geometry band shipped without its compute row rails a typed
    # ValidationError on both decode arms instead of admitting silently. The optional W3C carrier
    # defaults None and keeps trace context plus baggage under one wire field.
    subject: str
    key: str
    measured: dict[str, float]
    ceilings: dict[str, float]
    admitted: bool
    trace: dict[str, str] | None = None


_GEOMETRY_DECODER: Final[json.Decoder[_GeometryWire]] = json.Decoder(_GeometryWire)


class GraduationReceipt(Struct, frozen=True):
    source_package: str
    axis: HandoffAxis
    evidence_key: ContentKey
    residuals: dict[str, float]

    @staticmethod
    def graduates(
        source_package: str, axis: HandoffAxis, evidence_key: ContentKey, measured: dict[str, float], ceiling: dict[str, float],
        upstream: Mapping[str, str] | None = None,
    ) -> RuntimeRail[GraduationReceipt]:
        # two-stage rail: `boundary(_admit)` mints exactly one `RuntimeRail` over the refinement check, `.bind(_clear)`
        # threads the pure ceiling fold — admission, ceiling rejection, and emission stay one rail with no escape path;
        # both arms fire their `graduation/observability.md` admission fact, taps projecting it without touching the rail.
        def rail() -> RuntimeRail[GraduationReceipt]:
            _linked(upstream)  # producer-chain join at the rail head — the weave's live graduate span carries the Link on both arms
            return (
                boundary(f"graduation.{axis.tag}", lambda: GraduationReceipt._admit(measured, ceiling))
                .bind(lambda validated: GraduationReceipt._clear(source_package, axis, evidence_key, validated))
                .map(GraduationReceipt._witnessed)
                .map_error(GraduationReceipt._refused)
            )

        floor: SpanFacts = {"axis": axis.tag, "evidence_key": evidence_key.hex, "residual_count": len(measured)}
        return evidence_run(EvidenceScope.HANDOFF, f"graduate.{axis.tag}", rail, facts=floor)

    @staticmethod
    def geometry(source_package: str, payload: bytes | Mapping[str, object]) -> RuntimeRail[GraduationReceipt]:
        # ONE carrier-decode ingress, channel-agnostic: bytes ride the cached decoder, in-process builtins ride `convert`. An
        # out-of-union subject rails typed on the `unknown-subject` band — the drift signal a geometry union extension trips until
        # compute's ripple lands — never an unfenced ValidationError, never a silent admit.
        def decode() -> _GeometryWire:
            return _GEOMETRY_DECODER.decode(payload) if isinstance(payload, bytes) else convert(payload, type=_GeometryWire)

        def admit(wire: _GeometryWire) -> RuntimeRail[GraduationReceipt]:
            if wire.subject not in GEOMETRY_SUBJECTS:
                return Error(BoundaryFault(boundary=("graduation.geometry", "unknown-subject")))
            return _key(wire.key).bind(
                lambda key: GraduationReceipt.graduates(
                    source_package, HandoffAxis(geometry=wire.subject), key, wire.measured, wire.ceilings, upstream=wire.trace
                )
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
        # facts map is the pinned floor plus the residual ledger namespaced under `residual.` — the floor names stay
        # authoritative by construction, a ledger metric can never shadow them, and slots stay native `float`.
        facts: dict[str, object] = {**self.span_facts, **{f"residual.{name}": value for name, value in self.residuals.items()}}
        return (Receipt.of(self.source_package, ("planned", self.subject, facts)),)

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _admit(measured: Ledger, ceiling: Ceiling) -> tuple[Ledger, Ceiling]:
        # `Is` finiteness contract fires here inside the `boundary` fence, so `_clear` only ever
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
    def _witnessed(receipt: "GraduationReceipt") -> "GraduationReceipt":
        # admission fact: one tee fire off the cleared receipt — the observability taps project it
        # onto metrics and receipts, and the rail passes untouched.
        fired(ADMITTED, GraduationAdmitted(axis=receipt.axis.tag, subject=receipt.subject, evidence_key=receipt.evidence_key.hex, residual_count=len(receipt.residuals)))
        return receipt

    @staticmethod
    def _refused(fault: BoundaryFault) -> BoundaryFault:
        # rejection fact on the replay ring: a late diagnostic subscriber drains the recent refusals
        # on attach; refinement breach and ceiling rejection both land, discriminated by the boundary pair.
        fired(REJECTED, GraduationRejected(boundary=fault.boundary[0], reason=fault.boundary[1]))
        return fault

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


_WIRE_KEY: Final[re.Pattern[str]] = re.compile(r"\A(?P<digest>[0-9a-f]{32}):(?P<fmt>[^:]+)\Z")


def _linked(carrier: Mapping[str, str] | None) -> None:
    # consumer half of the co-shipped trace carrier: the installed global composite decodes trace context and baggage,
    # and the live consumer span folds its SpanContext as a Link — cross-producer click-through without a second
    # trace or a wire re-shape. A malformed carrier extracts an invalid context and folds nothing, so trace metadata
    # never rails the crossing; the telemetry SPAN_LIMITS max_links row bounds the fan a hostile payload could stamp.
    if carrier is None:
        return
    linked = trace.get_current_span(propagate.extract(carrier)).get_span_context()
    if linked.is_valid:
        trace.get_current_span().add_link(linked, {"rasm.link.kind": "geometry-graduation"})


def _key(render: str) -> RuntimeRail[ContentKey]:
    # wire crossing identity is the hex render `{value:032x}:{fmt}` (the C# InterchangeIdentity.Key contract);
    # admission proves the exact shape — 32 lowercase hex digits, ONE separator, a non-empty separator-free fmt —
    # so a torn, oversized, or non-hex render rails typed at the crossing instead of minting a garbage key or
    # letting a bare int() raise past the fence. `byte_length` is producer-local and never wire data, so the
    # decoded key carries 0 and every downstream read is the hex render, which round-trips byte-identically.
    matched = _WIRE_KEY.fullmatch(render)
    return (
        Ok(ContentKey(value=int(matched["digest"], 16), fmt=matched["fmt"], byte_length=0))
        if matched is not None
        else Error(BoundaryFault(boundary=("graduation.geometry", f"malformed-wire-key:{render[:64]}")))
    )


def evidence_run[T](
    scope: EvidenceScope, subject: str, dispatch: Callable[[], T] | Callable[[], Awaitable[T]], facts: SpanFacts = Map.empty()
) -> RuntimeRail[T] | Awaitable[RuntimeRail[T]]:
    # hub's one policy binding of the runtime measured weave: compute's scope vocabulary, the hub REDACTION row, and the caller's
    # call-time discriminating facts stamped on the recording span beside {scope, subject} — span and receipt carry parallel evidence.
    # `ledgered` wraps the dispatch, so every producer's kernel fires its domain lifecycle points and the two-block resource band
    # with zero producer edits.
    return measured(scope.value, subject, REDACTION, ledgered(scope, subject, dispatch), facts)
```

## [03]-[EVIDENCE_WEAVE]

- Owner: `evidence_run` binds the runtime `measured` weave to compute policy — the `EvidenceScope` seed table names the span scope, `REDACTION` the emit policy — and every producer composes this binding, so a page-local tracer mint, a page-local redaction declaration, or an inline span open beside it has no owner. Span, fence, rail flatten, fenced harvest, and OK close are the `runtime/observability/receipts#RECEIPT` owner's mechanics, composed here, never re-authored.
- Cases: every `EvidenceScope` member holds at least one composed consumer — a span emitter through this weave, a receipt `source_package` spelling through `.value`, or both; a member with neither is deleted, so the seed table can never carry dead vocabulary.
- Entry: one entry discriminating modality on the dispatch shape, never an `evidence_run_async` sibling; `facts` threads each producer's call-time discriminants — problem size, route, backend, precision — onto the recording span, so a trace filters on the same evidence the receipt carries; emission binds through its own fence at the runtime owner — the no-escape guarantee the hub's admission rail demands, granted to every producer.
- Ledger: the binding weaves the `graduation/observability.md` `ledgered` leg around every dispatch — enter fact, two-`oneshot` resource band, exit fact — so the point rail and the resource ledger reach every producer through the one binding; point rows, payload family, measure mapping, and taps are that page's, composed here, never re-authored.

## [04]-[CROSS_OWNER]

Each axis crosses under the one admission gate, and no `planned` receipt is emitted for a crossing that did not clear its ceiling. C# crossing stays outward-only: compute graduates `→` `csharp:Rasm.Compute`; C# never imports back.

- `solver`: rides the ONE `solvers/receipt.md#RECEIPT` `graduate` projection — the solve routes' receipts, the design/program optima through the shared `OutcomeReceipt.graduates`, and the interval certificates feed it with their own ledgers and family ceiling rows; a stationary-point or `OptimizeResult` verdict is a convergence verdict, never a separate case.
- `convex_program`: carries the `optimization/convex.md#CONVEX` KKT-gap certificate — a global-optimality proof distinct from the `solver` convergence verdict, so a returned point whose gap exceeds tolerance is an admission rejection.
- `symbolic`: `analysis/symbolic.md#DERIVATION` under its own stability law, that bar the admission ceiling.
- `model_asset`: crosses only after the `experiments/model.md#ASSET` manifest validation passes.
- `unit_law`/`uncertainty_law`: policy evidence only — the pint dimensional-consistency subject and the posterior-diagnostics subject gated on the rhat-and-ess residual check.
- `array_layout`: crosses once the `numerics/array.md#PAYLOAD` content key reproduces bit-identically across backends.
- `artifact`: stays the artifacts-side producer, never a compute-side obligation.
- `geometry`: ARRIVES as `GeometryHandoff.wire()` data through the one carrier-decode ingress; compute decodes every literal off the geometry-minted union and implements none of the geometry kernels — the producing geometry owners are geometry's own ledger rows, read there, never mirrored here. Wire field `trace` carries optional `traceparent`, `tracestate`, and baggage through `_linked`, which folds a `Link` on the live consumer span; the carrier is geometry-minted law — absent means no link, and a carrier re-shape is a geometry ripple landing on `_GeometryWire`.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
