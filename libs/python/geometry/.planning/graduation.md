# [PY_GEOMETRY_GRADUATION]

The folder's tier-0 evidence spine — the one page every producer composes and no page precedes. It mints the graduation vocabulary the folder produces: `GeometrySubject` is the geometry-owned subject union (the producer owns the vocabulary of what it produces), `GeometryHandoff` is the typed carrier every producer's `graduates()` returns, and the crossing to compute is content-keyed receipt DATA — `GeometryHandoff.wire()` — never an import. Compute's `HandoffAxis` hub and `GraduationReceipt` admission fold decode this carrier at the seam and re-shape nothing without a geometry ripple; no geometry prelude names a compute symbol. The spine additionally owns the shared evidence-run weave — geometry's `faults`-analogue tier: one `evidence_run(scope, operation, dispatch)` fold composing the runtime `boundary` fence, one span minted from the `EvidenceScope` seed table, and the `@receipted` harvest, so a producer page writes only its domain fold and threads no inline `trace.get_tracer` mint, no hand-authored `_ok`/`_emit` pair, and no per-page tracer literal. The `ReceiptContributor` law resolves here as STRUCTURAL-ONLY: the runtime Protocol is `@runtime_checkable`, a value conforms by carrying `contribute() -> Iterable[Receipt]`, the weave's harvest step emits for a conforming value and passes a plain value through, and subclassing the Protocol as a base is the deleted form corpus-wide.

The subject union is DIFFERENTIATED — one member per evidence class, never one literal carrying four semantics: `bim-compliance` (IDS/clash/BCF verdicts), `bim-lifecycle` (5D/4D lifecycle evidence), and `section-property` (section integrals) stand beside `numerical-primitive` (compas numerics, the algebra owner's retained member), the scan trio (`registration-transform`/`scan-deviation`/`reconstructed-mesh`), the graph pair (`topology-graph`/`network-graph`), `form-finding`, `mesh-algebra`, and the energy pair (`building-energy`/`thermal-comfort`). Each graduating owner binds its own member; two producers of one subject (features and algebra on `network-graph`) stay two producers of one member, never two vocabularies.

## [01]-[INDEX]

- [01]-[GRADUATION]: the `GeometrySubject` differentiated union, the `EvidenceScope` tracer-scope seed table, the frozen compute contract pin, the `GeometryHandoff` carrier with its residual-over-ceiling `admitted` fold and `wire()` crossing projection, and the one `evidence_run` weave (boundary + span + receipt harvest) discriminating sync and async dispatch on the callable shape.

## [02]-[GRADUATION]

- Owner: `GeometrySubject` — the closed `StrEnum` of everything this folder graduates, each member the frozen wire literal compute inherits; `EvidenceScope` — the closed tracer-scope vocabulary, one member per producing page, its value the OTel tracer scope the weave mints from (the seed table that replaces every per-page `trace.get_tracer` literal); `GeometryHandoff` — the frozen carrier of one graduation crossing (subject, evidence `ContentKey`, measured facts, caller ceilings) owning the residual-over-ceiling `admitted` verdict, the `wire()` dict projection that IS the compute crossing, and a `contribute` stream so the crossing itself receipts; `evidence_run` — the one weave every producer entry threads: span from the scope seed, runtime `boundary`/`async_boundary` fence INSIDE the live span (so the faults owner's `_convert` records a provider exception on a recording span and sets ERROR), the `@receipted` harvest on the cleared `Ok`, and the OK close-out — span-fence-emit-ok stated once here, composed everywhere else.
- Cases: `GeometrySubject` rows and their binding producers — `REGISTRATION_TRANSFORM` (scan/registration) · `SCAN_DEVIATION` (scan/deviation) · `RECONSTRUCTED_MESH` (scan/reconstruction + mesh/repair, two producers of one member) · `TOPOLOGY_GRAPH` (graph/nonmanifold) · `NETWORK_GRAPH` (graph/features + graph/algebra, two producers of one member) · `FORM_FINDING` (graph/algebra) · `NUMERICAL_PRIMITIVE` (graph/algebra, the retained compas-numerics member) · `MESH_ALGEBRA` (graph/algebra + mesh/brep + mesh/repair, three producers of one member) · `BIM_COMPLIANCE` (ifc/analysis) · `BIM_LIFECYCLE` (ifc/costing) · `SECTION_PROPERTY` (ifc/structural) · `BUILDING_ENERGY` (energy/model, energy/district, energy/simulate) · `THERMAL_COMFORT` (energy/climate). `EvidenceScope` rows one per producing page: `IFC_ANALYSIS` · `IFC_LIFECYCLE` · `IFC_SECTION` · `SCAN_INGESTION` · `SCAN_REGISTRATION` · `SCAN_DEVIATION` · `SCAN_RECONSTRUCTION` · `GRAPH_ALGEBRA` · `GRAPH_FEATURES` · `GRAPH_TOPOLOGY` · `ENERGY_CLIMATE` · `ENERGY_MODEL` · `ENERGY_DISTRICT` · `ENERGY_SIMULATE` — a new producer is one member, never a new tracer mint.
- Entry: `evidence_run(scope, operation, dispatch)` is the one polymorphic weave — a sync callable fences through `boundary(f"{scope}.{operation}", dispatch)` and returns `RuntimeRail[T]`; a coroutine function rides `async_boundary` and returns the awaitable rail — modality discriminated on the dispatch shape (`iscoroutinefunction`), never a sibling `evidence_run_async`. BOTH arms FLATTEN a railed return through `_flat`: a dispatch that is itself a `lane.offload` composition (the scan/graph producers' async shape, `functools.partial(lane.offload, kernel, ...)`) or a rail-returning selector-gated fold (the ifc producers' sync shape) absorbs un-nested while a bare value lifts `Ok` — one weave, rail-or-value on the return shape. The span opens from the cached per-scope tracer, carries the bounded `scope`/`operation` attributes behind `is_recording()`, and the fence runs INSIDE it; the `Ok` arm threads the `@receipted` harvest (a `ReceiptContributor`-conforming value emits its stream, a plain value passes) then the OK close-out. `GeometryHandoff.of(subject, key, measured, ceilings)` mints the carrier; `admitted` folds residual-over-ceiling (every ceiling row demands its measured fact and the fact clears the limit — an unmeasured ceiling row breaches); `wire()` projects the content-keyed dict the receipt stream and the compute seam both consume.
- Contract: the compute campaign inherits this fence's pin as frozen wire data with compute the named demanding consumer — the `SUBJECTS` tuple (derived from the union, thirteen literals), the admission shape `graduates(owner, HandoffAxis(geometry=subject), key, measured, ceiling)` compute's fold decodes the carrier into, and the residual-over-ceiling `_admit` direction (`measured[name] <= ceiling[name]` for every ceiling row). The pin is contract DATA stated once in the fence's contract block; `HandoffAxis` and `GraduationReceipt` appear in no geometry prelude, an out-of-union subject rails typed on the compute side, and a geometry union change ships a compute ripple, never a silent admit.
- Receipt: `GeometryHandoff.contribute` yields one `Receipt.of("rasm.geometry.graduation", (phase, subject, wire))` row — `phase="emitted"` for an admitted crossing, `phase="admitted"` for a breaching one whose caveat the receipt flags rather than asserting — so every graduation is visible on the receipt stream even before compute decodes it. Producer receipts stay on the producing pages; this owner receipts only the crossing.
- Packages: `expression` (`Map` the redaction table and scope folds), `msgspec` (`Struct` the frozen carrier), `opentelemetry-api` (`trace.get_tracer`/`start_as_current_span`/`Span.is_recording`/`set_attributes`/`set_status`/`Status`/`StatusCode` — the ONE page that names them; every sibling composes `evidence_run`), runtime (`RuntimeRail`/`boundary`/`async_boundary` the fault fence, `ContentKey` the evidence key, `Receipt`/`ReceiptContributor`/`Redaction`/`receipted` the receipt rail).
- Growth: a new evidence class is one `GeometrySubject` member plus its producing page's binding (and ships the compute-side decode ripple); a new producing page is one `EvidenceScope` member; a new cross-cutting concern on the weave (a metric instrument, a deadline) is one composition inside `evidence_run`, never a per-page re-weave; a second graduation direction (compute-to-geometry) does not exist — the crossing is one-way data.
- Boundary: no compute import anywhere in this folder (the crossing is `wire()` data); no per-page tracer mint, `_ok`/`_emit` pair, or inline `start_as_current_span` beside this owner; no second subject vocabulary, no bare-`str` subject, no per-receipt `subject: str` field racing the union; no `ReceiptContributor` subclassing (structural conformance only — the Protocol is runtime-checkable and the weave's harvest step reads it structurally); no geometry-typed callable over compute symbols in the contract pin. The deleted forms: a geometry prelude binding the compute graduation interior where this owner is the mint; a `GraduationReceipt.graduates(...)` call where `graduates()` returns the local carrier; a hand-authored six-element span weave where `evidence_run` owns it; a `"numerical-primitive"` literal carrying compliance, lifecycle, or section evidence where the differentiated members bind; an `evidence_run_async` sibling where the one entry discriminates on the dispatch shape.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterable, Mapping
from enum import StrEnum
from functools import cache
from inspect import iscoroutinefunction
from typing import Final

from expression import Ok, Result
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import Span, Status, StatusCode

from rasm.runtime.faults import RuntimeRail, async_boundary, boundary
from rasm.runtime.identity import ContentKey
from rasm.runtime.receipts import Receipt, Redaction, receipted

# --- [TYPES] ----------------------------------------------------------------------------


class GeometrySubject(StrEnum):
    # the geometry-minted graduation vocabulary — each member a frozen wire literal the compute
    # campaign inherits as decode-only data; a union change ships a compute ripple, never a silent admit.
    REGISTRATION_TRANSFORM = "registration-transform"
    SCAN_DEVIATION = "scan-deviation"
    RECONSTRUCTED_MESH = "reconstructed-mesh"
    TOPOLOGY_GRAPH = "topology-graph"
    NETWORK_GRAPH = "network-graph"
    FORM_FINDING = "form-finding"
    NUMERICAL_PRIMITIVE = "numerical-primitive"
    MESH_ALGEBRA = "mesh-algebra"
    BIM_COMPLIANCE = "bim-compliance"
    BIM_LIFECYCLE = "bim-lifecycle"
    SECTION_PROPERTY = "section-property"
    BUILDING_ENERGY = "building-energy"
    THERMAL_COMFORT = "thermal-comfort"


class EvidenceScope(StrEnum):
    # the tracer-scope seed table: one member per producing page, the value the OTel tracer scope —
    # the per-page `trace.get_tracer` literals collapsed onto one vocabulary; growth is one member.
    IFC_ANALYSIS = "geometry.ifc.analysis"
    IFC_LIFECYCLE = "geometry.ifc.costing"
    IFC_SECTION = "geometry.ifc.structural"
    SCAN_INGESTION = "geometry.scan.ingestion"
    SCAN_REGISTRATION = "geometry.scan.registration"
    SCAN_DEVIATION = "geometry.scan.deviation"
    SCAN_RECONSTRUCTION = "geometry.scan.reconstruction"
    GRAPH_ALGEBRA = "geometry.graph.algebra"
    GRAPH_FEATURES = "geometry.graph.features"
    GRAPH_TOPOLOGY = "geometry.graph.nonmanifold"
    ENERGY_CLIMATE = "geometry.energy.climate"
    ENERGY_MODEL = "geometry.energy.model"
    ENERGY_DISTRICT = "geometry.energy.district"
    ENERGY_SIMULATE = "geometry.energy.simulate"


# --- [CONSTANTS] ------------------------------------------------------------------------

# The frozen boundary contract the compute campaign inherits — decode-only wire data, geometry the
# named demanding consumer. Compute's HandoffAxis hub and GraduationReceipt fold decode
# GeometryHandoff.wire() under exactly this pin and re-shape nothing without a geometry ripple:
#   admission  graduates(owner, HandoffAxis(geometry=subject), key, measured, ceiling)
#   direction  residual-over-ceiling — admit iff measured[name] <= ceiling[name] per ceiling row
SUBJECTS: Final[tuple[str, ...]] = tuple(member.value for member in GeometrySubject)
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # graduation facts carry no secret field

# --- [MODELS] ---------------------------------------------------------------------------


class GeometryHandoff(Struct, frozen=True):
    subject: GeometrySubject
    key: ContentKey
    measured: dict[str, float]
    ceilings: dict[str, float]

    @classmethod
    def of(cls, subject: GeometrySubject, key: ContentKey, measured: Mapping[str, float], ceilings: Mapping[str, float]) -> "GeometryHandoff":
        return cls(subject=subject, key=key, measured=dict(measured), ceilings=dict(ceilings))

    @property
    def admitted(self) -> bool:
        # residual-over-ceiling: every ceiling row demands its measured fact and the fact clears the
        # limit; an unmeasured ceiling row breaches. Compute re-runs the same direction at its seam.
        return all(name in self.measured and self.measured[name] <= limit for name, limit in self.ceilings.items())

    def wire(self) -> dict[str, object]:
        # the crossing IS this projection — content-keyed receipt data compute decodes, never an import.
        return {
            "subject": self.subject.value,
            "key": self.key.hex,
            "measured": self.measured,
            "ceilings": self.ceilings,
            "admitted": self.admitted,
        }

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of("rasm.geometry.graduation", ("emitted" if self.admitted else "admitted", self.subject.value, self.wire()))


# --- [OPERATIONS] -----------------------------------------------------------------------


@cache
def _tracer(scope: EvidenceScope) -> trace.Tracer:
    return trace.get_tracer(scope.value)


def _close[T](span: Span, result: T) -> T:
    # the clean-exit close-out: OK on the measured span exactly once; ERROR stays the fence _convert's.
    span.set_status(Status(StatusCode.OK))
    return result


@receipted(_REDACTION)
def _emit[T](result: T) -> T:
    # the one receipt-egress step: the aspect harvests `contribute()` when the cleared value conforms
    # structurally to the runtime-checkable ReceiptContributor Protocol; a plain value passes through.
    return result


def _flat[T](value: "T | RuntimeRail[T]") -> "RuntimeRail[T]":
    # a lane-offloaded dispatch already returns a fenced rail; the weave flattens it so an offload
    # composes without double-nesting, and a bare value lifts Ok — rail-or-value on one arm.
    return value if isinstance(value, Result) else Ok(value)


def evidence_run[T](
    scope: EvidenceScope, operation: str, dispatch: Callable[[], T] | Callable[[], Awaitable[T]]
) -> RuntimeRail[T] | Awaitable[RuntimeRail[T]]:
    # Exemption: boundary kernel — the span context manager and the modality probe are the
    # platform-forced seam every producer composes instead of re-authoring. A coroutine-function
    # dispatch rides async_boundary and returns the awaitable rail; a sync callable fences inline.
    if iscoroutinefunction(dispatch):

        async def woven() -> RuntimeRail[T]:
            with _tracer(scope).start_as_current_span(operation) as span:
                if span.is_recording():
                    span.set_attributes({"scope": scope.value, "operation": operation})
                rail = (await async_boundary(f"{scope}.{operation}", dispatch)).bind(_flat)
                return rail.map(_emit).map(lambda result: _close(span, result))

        return woven()
    with _tracer(scope).start_as_current_span(operation) as span:
        if span.is_recording():
            span.set_attributes({"scope": scope.value, "operation": operation})
        return boundary(f"{scope}.{operation}", dispatch).bind(_flat).map(_emit).map(lambda result: _close(span, result))
```
