# [PY_COMPUTE_HANDOFF]

Multi-domain graduation HUB of the Python branch — the tier-0 page every evidence producer composes and no compute page precedes. Two crossings meet here in one direction each: compute-own evidence EGRESSES outward across the graduation-evidence wire to the C# managed owner, and geometry-minted evidence ARRIVES as `GeometryHandoff` wire data this hub DECODES — compute authors no geometry vocabulary, imports no `rasm.geometry` symbol, and re-shapes nothing without a geometry ripple. Graduation is a Python-branch-only concept: the receipt names the wire axis it crosses on and never a C# interior owner row, a C# receipt mint, or a product-runtime authorization — the concrete C# owner consuming each axis is confirmed on the graduation task, never a routing literal that drifts.

`GraduationReceipt.graduates` is the one admission gate: the sibling rejection clauses every evidence owner declares collapse to one residual-over-ceiling fold parameterized by the axis owner's ledger, never inlined per-site comparisons. `evidence_run` is this hub's binding of the runtime `measured` weave — the compute `EvidenceScope` vocabulary and the hub `REDACTION` row applied once — so span, fault fence, rail flatten, and fenced harvest stay the `runtime/observability/receipts#RECEIPT` owner's mechanics and compute authors no second instrumentation shape.

## [01]-[INDEX]

- [02]-[GRADUATION]: the receipt, the `HandoffAxis` union with the inherited geometry contract block, the two-stage `_admit`/`_clear` admission rail with its `graduates_async` durable trail over both verdicts, and the producer registry.
- [03]-[EVIDENCE_WEAVE]: the shared `evidence_run` fold every compute evidence owner composes in place of page-local tracers and inline span opens.
- [04]-[CROSS_OWNER]: the routing rules gating each axis to its managed owner.

## [02]-[GRADUATION]

- Owner: `GraduationReceipt` — the source-package, axis, evidence-key, and residual-ledger carrier. Its axis case IS the subject — no parallel `subject: str` field races the discriminant; the `geometry` case carries its subject as DECODED WIRE DATA typed against the inherited `GEOMETRY_SUBJECTS` contract block, never a compute-authored type racing the geometry mint and never a `rasm.geometry` import — a geometry union change is a geometry ripple landing here as one row. No `bool` admitted flag rides the receipt because its existence IS the admission: a rejected handoff is an `Error` that never reaches `contribute`.
- Cases: the `HandoffAxis` roster extends by sibling campaign, never by silent admit; the `convex_program` case carries the dual-certificate optimality proof distinct from the `solver` case's first-order convergence verdict; the `unit_law`/`uncertainty_law` cases cross as policy evidence only.
- Producers: one self-wired `graduates()` producer per live case, each importing this hub downward — a case with no producer is dead vocabulary wearing a rail. `solver`: the `solvers/receipt#RECEIPT` `graduate` projection every solve owner feeds with its receipt or prepared ledger, its family ceiling row, and its key; `convex_program`: `optimization/convex#CONVEX`; `symbolic`: `analysis/symbolic#DERIVATION` under its own stability law; `array_layout`: `numerics/array#PAYLOAD` over the cross-backend bit-identity proof; `unit_law`: `numerics/quantity#QUANTITY`; `uncertainty_law`: `experiments/inference#BAYESIAN`; `model_asset`: `experiments/model#ASSET`; `artifact`: artifacts `core/receipt.md` (sibling-owned); `geometry`: decode-only. `numerics/statistics#STATISTICS` stays deliberately graduation-free by its own charter and `solvers/sensitivity#SENSITIVITY` stays disjoint from study DGSM — preserved boundaries, never gaps; composing the evidence weave is an observability import that breaches neither.
- Law: every graduation admission reaches the `python:runtime/observability/journal#LEDGER` plane, and `graduates_async` is its ONE seat — the awaitable twin this pure fold mints over the band hop, since recording suspends and `graduates` opens no loop. BOTH verdicts record through one `_evidence` fold: `REGULATORY` at the admitted tee, because a cleared crossing is the record a C# consumer acts on years later, and `OPERATIONAL` at the refused tee, because a bar that held is incident-window evidence and never a seven-year hold. A plane carrying only the crossings that cleared answers "did anything try" with silence. The rails differ by arm and the difference is law: the admitted rail BINDS, so a crossing the plane refused never reads as graduated, while the refused rail rides BESIDE the fault, since replacing a caller's ceiling rejection with a plane fault hides the domain verdict it came for. Subjects stay empty — an evidence key names a computation, never a data subject — and no meter rides the leg, the crossing's cpu being the resource band's one charge. `EVIDENCE_DOMAIN` derives off this page's own scope spelling and is the one domain segment every compute audit verb carries.
- Auto: every graduating family's DEFAULT ceiling is a governed policy row on that family's own carrier beside its route table, the hub's caller-supplied tighter row the override — an ad-hoc ceiling literal at a `graduates()` call site has no owner. Three failure concerns stay distinct on three fences: a refinement breach is an exception the `_admit` fence converts, a ceiling rejection is a pure domain `Error` and never a raise, and an emit-time raise is the weave's emit fence to convert.
- Receipt: an admitted handoff is a `planned` wire proposal, never an emitted product receipt. Its fact floor is FENCE-PINNED SELF-DESCRIBING as the C# graduation gate's decode vocabulary — `FACT_FLOOR` and the residual ledger namespaced under `residual.`, so a ledger metric can never shadow a floor name — and the gate attributes and dedupes every crossing without free-form-map guessing; the evidence key renders through the canonical `ContentKey.hex` form the C# `InterchangeIdentity.Key` contract reads.
- Growth: a new handoff kind is one `HandoffAxis` case, one `_subject` match arm, and its sibling-campaign producer, its audit verb deriving with it; a newly audited admission column is one `_evidence` `Change` row; a new geometry subject is a geometry ripple landing one `GEOMETRY_SUBJECTS` row, re-proved by the `mirror_aligned` boot gate a composing root runs with geometry's own `SUBJECTS`/`WIRE_FIELDS`/`LINK_KIND` exports; a stricter admission bar is one tighter ceiling row the caller supplies; a new evidence owner is one `EvidenceScope` row; a new embedded composition is one `ScopeKey` the caller threads, never a sibling registry.
- Boundary: no handoff record claims production readiness, a Python-only benchmark conclusion, or a C# source-shape claim absent from the C# owner planning. No ledger, custody, or retention window is minted here — the plane arrives bound at the composition root and this owner declares a `Retain` class alone. Compute-emitted geometry subjects do not exist — a second graduation direction is geometry's own closed ruling, so a compute re-graduation on the geometry axis requires a named consumer and a compute-owned axis case, never the geometry case.

```python signature
# --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
import re
from collections.abc import Awaitable, Callable, Iterable, Mapping
from enum import StrEnum
from math import isfinite
from typing import Annotated, Final, Literal, assert_never

from beartype import beartype
from beartype.vale import Is
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, convert, json
from opentelemetry import propagate, trace

from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail, boundary
from rasm.runtime.identity import ContentKey
from rasm.runtime.journal import Actor, Assigned, AuditFact, Change, Cleared, Fact, Journal, Party, Retain
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, Redaction, ScopeKey, measured

lazy from rasm.compute.graduation.observability import ADMITTED, REJECTED, GraduationAdmitted, GraduationRejected, fired, ledgered  # lazy breaks the hub<->observability stratum cycle; proxies reify at first dispatch

# --- [TYPES] --------------------------------------------------------------------------------


class EvidenceScope(StrEnum):
    # scope seed table: one member per compute module leaf, value `rasm.compute.<leaf>`, owning BOTH the tracer scope and the
    # receipt `source_package` spelling — producers compose `EvidenceScope.<X>.value`, never a bare literal, so a drifted
    # spelling has no owner. The `rasm.`-rooted spelling is the branch grammar the runtime `SCOPES` vocabulary and the
    # geometry peer's own scope enum hold, and it is the same root the observability point ids and instrument names carry, so
    # a backend joining a compute span to its metric and its receipt reads one namespace rather than three.
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
    RECEIPT = "rasm.compute.receipt"
    SENSITIVITY = "rasm.compute.sensitivity"
    SIGNAL = "rasm.compute.signal"
    SPATIAL = "rasm.compute.spatial"
    STATISTICS = "rasm.compute.statistics"
    STUDY = "rasm.compute.study"
    SYMBOLIC = "rasm.compute.symbolic"
    TRANSFORM = "rasm.compute.transform"


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

# shared trace-join spelling both ends stamp on `rasm.link.kind`; geometry exports its own `LINK_KIND` and
# `mirror_aligned` proves the two byte-equal at boot.
LINK_KIND: Final[str] = "geometry-graduation"

# fence-pinned SELF-DESCRIBING fact floor of every `planned` receipt — the C# graduation gate's
# decode vocabulary; the cleared residual ledger rides beside the floor under `residual.`-prefixed
# keys, so a metric name can never collide with a floor slot, and `phase` rides the triple.
FACT_FLOOR: Final[tuple[str, ...]] = ("axis", "subject", "evidence_key", "residual_count")

REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # hub-exported: compute evidence facts carry no secret field

# the one domain segment every compute audit verb carries — the same `<domain>` the metric projection and the point
# ids already record under — DERIVED off this page's own scope spelling rather than transcribed, so a root rename
# cannot strand a durable verb under a segment no series answers, and every producer page reads this one export.
EVIDENCE_DOMAIN: Final[str] = EvidenceScope.HANDOFF.value.split(".", 2)[1]

# --- [MODELS] ---------------------------------------------------------------------------


class _GeometryWire(Struct, frozen=True, gc=False, forbid_unknown_fields=True):
    # frozen `GeometryHandoff.wire()` projection — decode-only mirror of the geometry mint;
    # field names are wire law, re-shaped only by a geometry ripple, and `forbid_unknown_fields`
    # is the widen tripwire: a geometry band shipped without its compute row rails a typed
    # ValidationError on both decode arms instead of admitting silently. The optional W3C carrier
    # defaults None and keeps trace context plus baggage under one wire field. The three mapping
    # slots read `dict` because that IS the crossing shape: the owner holds `measured`, `ceilings`,
    # and `trace` as persistent maps for its own frozen custody and `wire()` projects each to a
    # plain dict, so a mirror widened to the owner's interior carrier decodes nothing it sends.
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
        upstream: Mapping[str, str] | None = None, composition: ScopeKey = DEFAULT_SCOPE,
    ) -> RuntimeRail[GraduationReceipt]:
        # two-stage rail: `boundary(_admit)` mints exactly one `RuntimeRail` over the refinement check, `.bind(_clear)`
        # threads the pure ceiling fold — admission, ceiling rejection, and emission stay one rail with no escape path;
        # both arms fire their `graduation/observability.md` admission fact under the caller's composition, taps
        # projecting it without touching the rail.
        def rail() -> RuntimeRail[GraduationReceipt]:
            _linked(upstream)  # producer-chain join at the rail head — the weave's live graduate span carries the Link on both arms
            return (
                boundary(f"graduation.{axis.tag}", lambda: GraduationReceipt._admit(measured, ceiling))
                .bind(lambda validated: GraduationReceipt._clear(source_package, axis, evidence_key, validated))
                .map(lambda cleared: GraduationReceipt._witnessed(cleared, composition))
                .map_error(lambda fault: GraduationReceipt._refused(fault, composition))
            )

        floor: SpanFacts = {"axis": axis.tag, "evidence_key": evidence_key.hex, "residual_count": len(measured)}
        return evidence_run(EvidenceScope.HANDOFF, f"graduate.{axis.tag}", rail, facts=floor, composition=composition)

    @staticmethod
    async def graduates_async(
        source_package: str, axis: HandoffAxis, evidence_key: ContentKey, measured: dict[str, float], ceiling: dict[str, float],
        upstream: Mapping[str, str] | None = None, composition: ScopeKey = DEFAULT_SCOPE,
    ) -> RuntimeRail[GraduationReceipt]:
        # the awaitable twin over the band hop, and the ONE seat where an admission reaches the durable plane.
        # `graduates` is a pure fold over an already-measured ledger — it opens no loop and awaits nothing — so the
        # trail cannot land inside it, recording being a suspending act. BOTH verdicts record: an admission plane
        # carrying only the crossings that cleared answers "did anything try" with silence, and a refused graduation
        # is exactly the crossing an audit reconstructs. The rails differ by arm and that difference is load-bearing
        # — the admitted rail BINDS, because a crossing the plane could not record must not read as graduated, while
        # the refused one rides BESIDE the fault, since replacing a caller's own ceiling rejection with a plane fault
        # hides the domain verdict it came for.
        match GraduationReceipt.graduates(source_package, axis, evidence_key, measured, ceiling, upstream, composition):
            case Result(tag="ok") as cleared:
                return (await Journal.record(_evidence(source_package, axis, cleared), scope=composition)).bind(lambda _landed: cleared)
            case refused:
                await Journal.record(_evidence(source_package, axis, refused), scope=composition)
                return refused

    @staticmethod
    def geometry(source_package: str, payload: bytes | Mapping[str, object], composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[GraduationReceipt]:
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
                    source_package, HandoffAxis(geometry=wire.subject), key, wire.measured, wire.ceilings, upstream=wire.trace,
                    composition=composition,
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
    def _witnessed(receipt: "GraduationReceipt", composition: ScopeKey) -> "GraduationReceipt":
        # admission fact: one tee fire off the cleared receipt under the caller's composition — the observability taps
        # project it onto metrics and receipts, and the rail passes untouched.
        fired(ADMITTED, GraduationAdmitted(axis=receipt.axis.tag, subject=receipt.subject, evidence_key=receipt.evidence_key.hex, residual_count=len(receipt.residuals)), composition)
        return receipt

    @staticmethod
    def _refused(fault: BoundaryFault, composition: ScopeKey) -> BoundaryFault:
        # rejection fact on the replay ring: a late diagnostic subscriber drains the recent refusals
        # on attach; refinement breach and ceiling rejection both land, discriminated by the boundary pair.
        fired(REJECTED, GraduationRejected(boundary=fault.boundary[0], reason=fault.boundary[1]), composition)
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


def _evidence(source_package: str, axis: HandoffAxis, settled: RuntimeRail[GraduationReceipt]) -> Block[Fact]:
    # ONE admission-trail fold over BOTH verdicts, so the two arms cannot drift into two vocabularies for one
    # crossing: the verb, the actor, and the axis target are shared and only the retention, the subject source, and
    # the diff differ. Retention is where the arms genuinely diverge — an ADMITTED crossing is the record a C#
    # consumer acts on years later, so it holds REGULATORY, while a REFUSED one is the operational trail of a bar
    # that held, worth reading back through an incident window and never worth a seven-year hold. The verb carries
    # the package's one domain segment beside the axis its own union names, so a durable row greps against the
    # series the taps already record. Subjects stay EMPTY by law: an evidence key names a computation, never a data
    # subject, so indexing one would pull solve evidence into every portability export and every erasure sweep. No
    # meter rides here — the crossing's cpu is the resource band's one COMPUTE charge.
    match settled:
        case Result(tag="ok", ok=receipt):
            row: tuple[Retain, str, tuple[Change, ...]] = (
                Retain.REGULATORY,
                receipt.subject,
                (
                    Assigned(path="/evidence_key", next=receipt.evidence_key.hex),
                    Assigned(path="/residual_count", next=str(len(receipt.residuals))),
                ),
            )
        case Result(tag="error", error=fault):
            row = (Retain.OPERATIONAL, GraduationReceipt._subject(axis), (Cleared(path="/admitted", prior=fault.boundary[1]),))
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


def _linked(carrier: Mapping[str, str] | None) -> None:
    # consumer half of the co-shipped trace carrier: the installed global composite decodes trace context and baggage,
    # and the live consumer span folds its SpanContext as a Link — cross-producer click-through without a second
    # trace or a wire re-shape. A malformed carrier extracts an invalid context and folds nothing, so trace metadata
    # never rails the crossing; the telemetry SPAN_LIMITS max_links row bounds the fan a hostile payload could stamp.
    if carrier is None:
        return
    linked = trace.get_current_span(propagate.extract(carrier)).get_span_context()
    if linked.is_valid:
        trace.get_current_span().add_link(linked, {"rasm.link.kind": LINK_KIND})


# S4 boot gate over the hand-copied geometry mirror: a composition root composing BOTH branches feeds geometry's own
# exports (`SUBJECTS`, `WIRE_FIELDS`, `LINK_KIND`) and compute imports nothing — the branch descriptor-drift-gate
# pattern seated at the one tier where both ends are importable. Set equality catches the removed-or-renamed subject
# `forbid_unknown_fields` never trips, the field census closes the rename-and-drop hole on `_GeometryWire`, and the
# link spelling proves the trace join stamps one kind at both ends. Empty roster reads aligned; a root refuses boot
# on any row, naming the drift instead of decoding past it.
def mirror_aligned(subjects: Iterable[str], wire_fields: Iterable[str], link_kind: str) -> Block[str]:
    theirs, fields = frozenset(subjects), frozenset(wire_fields)
    mirror = frozenset(_GeometryWire.__struct_fields__)
    return Block.of_seq((
        *(f"subject-missing:{name}" for name in sorted(theirs - GEOMETRY_SUBJECTS)),
        *(f"subject-retired:{name}" for name in sorted(GEOMETRY_SUBJECTS - theirs)),
        *(f"field-missing:{name}" for name in sorted(fields - mirror)),
        *(f"field-orphaned:{name}" for name in sorted(mirror - fields)),
        *((f"link-kind:{link_kind}!={LINK_KIND}",) if link_kind != LINK_KIND else ()),
    ))


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
    scope: EvidenceScope, subject: str, dispatch: Callable[[], T] | Callable[[], Awaitable[T]], facts: SpanFacts = Map.empty(),
    composition: ScopeKey = DEFAULT_SCOPE,
) -> RuntimeRail[T] | Awaitable[RuntimeRail[T]]:
    # hub's one policy binding of the runtime measured weave: compute's scope vocabulary, the hub REDACTION row, and the caller's
    # call-time discriminating facts stamped on the recording span beside {scope, subject} — span and receipt carry parallel evidence.
    # `ledgered` wraps the dispatch, so every producer's kernel fires its domain lifecycle points and the two-block resource band
    # with zero producer edits, and the composition key threads through it AND onto the weave itself so an embedded second
    # composition's lifecycle facts, its harvested weave line, and its recorded series all key to it rather than the root's:
    # `scope.value` is the instrumentation scope the tracer opens under and never doubles as the custody key.
    return measured(scope.value, subject, REDACTION, ledgered(scope, subject, dispatch, composition=composition), facts, composition=composition)
```

## [03]-[EVIDENCE_WEAVE]

- Owner: `evidence_run` binds the runtime `measured` weave to compute policy — the `EvidenceScope` seed table names the span scope, `REDACTION` the emit policy — and every producer composes this binding, so a page-local tracer mint, a page-local redaction declaration, or an inline span open beside it has no owner. Span, fence, rail flatten, fenced harvest, and OK close are the `runtime/observability/receipts#RECEIPT` owner's mechanics, composed here, never re-authored.
- Spelling: every member's value is `rasm.compute.<leaf>` and the member NAME is the only handle a producer spells, so a scope reaches a producer as `EvidenceScope.<X>` and its value only where the weave stamps the tracer or the receipt `source_package`. A reverse `EvidenceScope(f"...{tag}")` value lookup reconstructs a spelling the enum already owns and re-breaks on the next root change, so a tag-keyed consumer carries a `Map[str, EvidenceScope]` row instead. The estate root is not decoration: an unrooted value puts every compute span and receipt outside the namespace its sibling branches, the observability point ids, and the instrument roster all share, so a backend cannot join one crossing's three signals.
- Cases: every `EvidenceScope` member holds at least one composed consumer — a span emitter through this weave, a receipt `source_package` spelling through `.value`, or both; a member with neither is deleted, so the seed table can never carry dead vocabulary.
- Entry: one entry discriminating modality on the dispatch shape, never an `evidence_run_async` sibling; `facts` threads each producer's call-time discriminants — problem size, route, backend, precision — onto the recording span, so a trace filters on the same evidence the receipt carries; emission binds through its own fence at the runtime owner — the no-escape guarantee the hub's admission rail demands, granted to every producer.
- Ledger: the binding weaves the `graduation/observability.md` `ledgered` leg around every dispatch — enter fact, resource band off the runtime `Cost` bracket, exit fact on both the settled and raised arms — so the point rail and the resource ledger reach every producer through the one binding; point rows, payload family, measure mapping, and taps are that page's, composed here, never re-authored. `composition` threads from the caller through this binding into `ledgered` and through `graduates` into both admission fires, so an embedded second composition's lifecycle and admission facts reach the points IT registered; the key defaults `DEFAULT_SCOPE`, so the root call shape stays scope-free.

## [04]-[CROSS_OWNER]

Each axis crosses under the one admission gate, and no `planned` receipt is emitted for a crossing that did not clear its ceiling. C# crossing stays outward-only: compute graduates `→` `csharp:Rasm.Compute`; C# never imports back.

- `solver`: rides the ONE `solvers/receipt#RECEIPT` `graduate` projection — the solve routes' receipts, the design/program optima through the shared `OutcomeReceipt.graduates`, and the interval certificates feed it with their own ledgers and family ceiling rows; a stationary-point or `OptimizeResult` verdict is a convergence verdict, never a separate case.
- `convex_program`: carries the `optimization/convex#CONVEX` KKT-gap certificate — a global-optimality proof distinct from the `solver` convergence verdict, so a returned point whose gap exceeds tolerance is an admission rejection.
- `symbolic`: `analysis/symbolic#DERIVATION` under its own stability law, that bar the admission ceiling.
- `model_asset`: crosses only after the `experiments/model#ASSET` manifest validation passes.
- `unit_law`/`uncertainty_law`: policy evidence only — the pint dimensional-consistency subject and the posterior-diagnostics subject gated on the rhat-and-ess residual check.
- `array_layout`: crosses once the `numerics/array#PAYLOAD` content key reproduces bit-identically across backends.
- `artifact`: stays the artifacts-side producer, never a compute-side obligation.
- `geometry`: ARRIVES as `GeometryHandoff.wire()` data through the one carrier-decode ingress; compute decodes every literal off the geometry-minted union and implements none of the geometry kernels — the producing geometry owners are geometry's own ledger rows, read there, never mirrored here. Wire field `trace` carries optional `traceparent`, `tracestate`, and baggage through `_linked`, which folds a `Link` on the live consumer span; the carrier is geometry-minted law — absent means no link, and a carrier re-shape is a geometry ripple landing on `_GeometryWire`.

## [05]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
