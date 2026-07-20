# [PY_GEOMETRY_GRADUATION]

`graduation` is the folder's tier-0 evidence spine every producer composes and no page precedes. It mints the graduation vocabulary — `GeometrySubject`, the geometry-owned union of what the folder produces — and the typed `GeometryHandoff` carrier every producer's `graduates()` returns; the crossing to compute is content-keyed receipt DATA (`GeometryHandoff.wire()`), never an import. It also owns the folder's evidence weave, geometry's faults-analogue tier, so a producer writes only its domain fold.

Compute is the named demanding consumer: its `HandoffAxis` hub and `GraduationReceipt` fold decode `wire()` at the seam and re-shape nothing without a geometry ripple that a union change ships, never a silent admit; no geometry prelude names a compute symbol. Evidence weave composes the runtime `boundary`/`async_boundary` fence inside a span seeded from `EvidenceScope`, then the `@receipted` harvest and OK close-out, so producers thread no page-local instrumentation.

## [01]-[INDEX]

- [01]-[GRADUATION]: the `GeometrySubject` union, the `EvidenceScope` scope table, the `GeometryHandoff` carrier with its residual-over-ceiling admission and `wire()` crossing, and the one `evidence_run` weave.

## [02]-[GRADUATION]

- Owner: `GeometrySubject` — the closed union the folder graduates, each member a frozen wire literal; `EvidenceScope` — the closed tracer-scope table, one member per producing page; `GeometryHandoff` — the frozen crossing carrier owning the `admitted` verdict, the `wire()` projection that IS the compute crossing, and a `contribute` receipt stream; `evidence_run` — the one weave every producer threads, span-fence-emit-ok once here and composed everywhere.
- Cases: the union is DIFFERENTIATED, one member per evidence class, each owner binding its own. Scan produces `REGISTRATION_TRANSFORM`, `SCAN_DEVIATION`, and (with mesh/repair) `RECONSTRUCTED_MESH`; graph produces `TOPOLOGY_GRAPH`, `NETWORK_GRAPH` (features and algebra co-produce it), `FORM_FINDING`, and `NUMERICAL_PRIMITIVE` (the retained compas-numerics member); mesh/brep, mesh/repair, and graph/algebra co-produce `MESH_ALGEBRA`; ifc produces `BIM_COMPLIANCE`, `BIM_LIFECYCLE`, `SECTION_PROPERTY`; energy produces `BUILDING_ENERGY`, `THERMAL_COMFORT`. Two or three producers of one subject stay one member, never parallel vocabularies; a page earns its `EvidenceScope` member the moment its rail returns through `evidence_run`, and an engine or gate page whose receipts an enclosing weave harvests holds none.
- Entry: `evidence_run(scope, operation, dispatch)` is the one polymorphic weave, modality discriminated on the dispatch shape (`iscoroutinefunction`), never an `evidence_run_async` sibling — a coroutine rides `async_boundary` for the awaitable rail, a sync callable fences inline. Both arms FLATTEN through `_flat`, so a `lane.offload` or rail-returning fold absorbs un-nested and a bare value lifts `Ok`, and both arms time the dispatch onto the runtime `rasm.geometry.evidence.duration` row under `domain="geometry"` keyed by scope — the serve leg composes the weave under `EvidenceScope.MESH_SERVE`, so serve latency is one weave composition and pool depth stays the runtime lane spine's own gauges. `GeometryHandoff.of` mints, `admitted` folds residual-over-ceiling, `wire()` projects the dict the receipt stream and compute seam consume.
- Receipt: `GeometryHandoff.contribute` yields one `Receipt.of("rasm.geometry.graduation", (phase, subject, wire))` row — `phase="emitted"` for an admitted crossing, `"admitted"` for a breaching one whose caveat the receipt flags rather than asserts — so every graduation is visible before compute decodes it. Producer receipts stay on the producing pages; this owner receipts only the crossing.
- Packages: `expression` (`Map`), `msgspec` (`Struct` carrier), `opentelemetry-api` — the ONE page that names the OTel tracer/span API, every sibling composing `evidence_run` — and runtime (`RuntimeRail`/`boundary`/`async_boundary`, `ContentKey`, `Receipt`/`receipted`, `Metrics.record` as the one instrument projection port).
- Growth: a new evidence class is one `GeometrySubject` member and its producing binding, shipping the compute-side decode ripple; a new producing page is one `EvidenceScope` member; a new cross-cutting concern on the weave is one composition inside `evidence_run`, never a per-page re-weave; a producer whose result carries `span_facts` gains span-visible evidence with zero weave edit; the crossing is one-way data, no reverse direction.
- Boundary: no compute import anywhere in the folder, the crossing being `wire()` data; no per-page tracer mint, `_ok`/`_emit` pair, or inline `start_as_current_span` beside this owner; every weave span opens INTERNAL and nests beneath the live seam parent — the runtime `ServerHost` interceptor owns the SERVER span at the gRPC seam; no second subject vocabulary, bare-`str` subject, or per-receipt `subject` field racing the union. Mid-operation facts stay worker-side receipt evidence — the offloaded `KernelTrait.HOSTILE` kernels cross a process seam no in-process hook registry or live span reaches. A `"numerical-primitive"` literal carrying compliance, lifecycle, or section evidence breaches the differentiated members; the compute graduation interior is compute's to own — this owner only mints and projects.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterable, Mapping
from enum import StrEnum
from functools import cache
from inspect import iscoroutinefunction
from time import perf_counter
from typing import Final, Protocol, runtime_checkable

from expression import Ok, Result
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import Span, Status, StatusCode

from rasm.runtime.faults import RuntimeRail, async_boundary, boundary
from rasm.runtime.identity import ContentKey
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt, Redaction, receipted

# --- [TYPES] ----------------------------------------------------------------------------


class GeometrySubject(StrEnum):
    # each member a frozen wire literal compute inherits as decode-only data.
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
    # one member per weave-returning rail; value the rasm.-prefixed instrumentation scope the weave's tracer mints from —
    # one spelling law with the runtime SCOPES vocabulary and this owner's own receipt scopes.
    MESH_SERVE = "rasm.geometry.mesh.serve"
    MESH_BREP = "rasm.geometry.mesh.brep"
    MESH_REPAIR = "rasm.geometry.mesh.repair"
    IFC_ANALYSIS = "rasm.geometry.ifc.analysis"
    IFC_LIFECYCLE = "rasm.geometry.ifc.costing"
    IFC_SECTION = "rasm.geometry.ifc.structural"
    SCAN_INGESTION = "rasm.geometry.scan.ingestion"
    SCAN_REGISTRATION = "rasm.geometry.scan.registration"
    SCAN_DEVIATION = "rasm.geometry.scan.deviation"
    SCAN_RECONSTRUCTION = "rasm.geometry.scan.reconstruction"
    GRAPH_ALGEBRA = "rasm.geometry.graph.algebra"
    GRAPH_FEATURES = "rasm.geometry.graph.features"
    GRAPH_TOPOLOGY = "rasm.geometry.graph.nonmanifold"
    ENERGY_CLIMATE = "rasm.geometry.energy.climate"
    ENERGY_MODEL = "rasm.geometry.energy.model"
    ENERGY_DISTRICT = "rasm.geometry.energy.district"
    ENERGY_SIMULATE = "rasm.geometry.energy.simulate"


@runtime_checkable
class SpanFacts(Protocol):
    # structural close-out conformance the weave reads like the receipt harvest; flat primitives only — the span attribute API refuses a nested mapping.
    @property
    def span_facts(self) -> Mapping[str, object]: ...


# --- [CONSTANTS] ------------------------------------------------------------------------

# frozen tuple compute inherits as decode-only wire data; a union change lands as a compute ripple, never a silent admit.
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
        # residual-over-ceiling: every ceiling row demands its measured fact clearing the limit; an unmeasured row breaches.
        return all(name in self.measured and self.measured[name] <= limit for name, limit in self.ceilings.items())

    @property
    def span_facts(self) -> Mapping[str, object]:
        # dotted-key flattening: measured/ceilings are nested dicts the span attribute API refuses whole.
        return {
            "subject": self.subject.value,
            "key": self.key.hex,
            "admitted": self.admitted,
            **{f"measured.{name}": value for name, value in self.measured.items()},
            **{f"ceilings.{name}": value for name, value in self.ceilings.items()},
        }

    def wire(self) -> dict[str, object]:
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
    # OK on the measured span exactly once; ERROR stays the fence _convert's. A conforming value annotates the span with its crossing evidence.
    if span.is_recording() and isinstance(result, SpanFacts):
        span.set_attributes(dict(result.span_facts))
    span.set_status(Status(StatusCode.OK))
    return result


@receipted(_REDACTION)
def _emit[T](result: T) -> T:
    # harvests contribute() when the cleared value conforms structurally (runtime-checkable Protocol, no subclassing); a plain value passes.
    return result


def _flat[T](value: "T | RuntimeRail[T]") -> "RuntimeRail[T]":
    # a lane-offloaded or rail-returning dispatch already carries a fenced rail; flatten so an offload composes un-nested.
    return value if isinstance(value, Result) else Ok(value)


def _recorded[T](scope: EvidenceScope, started: float, result: T) -> T:
    # serve latency lands as the geometry domain row on the runtime spine; pool depth stays the lane spine's own gauges.
    Metrics.record({"rasm.geometry.evidence.duration": (perf_counter() - started) * 1000.0}, domain="geometry", kind=scope.value)
    return result


def evidence_run[T](
    scope: EvidenceScope, operation: str, dispatch: Callable[[], T] | Callable[[], Awaitable[T]]
) -> RuntimeRail[T] | Awaitable[RuntimeRail[T]]:
    # Exemption: boundary kernel — the span manager and modality probe are the platform-forced seam producers compose, not re-author.
    started = perf_counter()
    if iscoroutinefunction(dispatch):

        async def woven() -> RuntimeRail[T]:
            with _tracer(scope).start_as_current_span(operation) as span:
                if span.is_recording():
                    span.set_attributes({"scope": scope.value, "operation": operation})
                rail = (await async_boundary(f"{scope}.{operation}", dispatch)).bind(_flat)
                return rail.map(_emit).map(lambda result: _recorded(scope, started, _close(span, result)))

        return woven()
    with _tracer(scope).start_as_current_span(operation) as span:
        if span.is_recording():
            span.set_attributes({"scope": scope.value, "operation": operation})
        return boundary(f"{scope}.{operation}", dispatch).bind(_flat).map(_emit).map(lambda result: _recorded(scope, started, _close(span, result)))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
