# [PY_COMPUTE_HANDOFF]

The Python-only graduation rail moving offline evidence outward across the graduation-evidence wire under one telemetry-and-fault-fenced admission. `GraduationReceipt` carries the source package, the `HandoffAxis`, the `ContentKey` evidence key, and the residual ledger across one handoff.

`HandoffAxis` is the `@tagged_union` of handoff kinds, each case carrying the subject its kind names: the `geometry` case its `GeometrySubject` literal, the `unit_law`/`uncertainty_law` cases the policy-law subject, the numeric cases their `str` evidence subject. The axis case IS the subject — no parallel `subject: str` field drifts against the discriminant because the subject lives inside the selected case.

`GraduationReceipt.graduates` is the one admission gate, folding the measured `Ledger` against the per-key `Ceiling` to `RuntimeRail[GraduationReceipt]`. The four sibling rejection clauses every evidence owner declares collapse to the one residual-over-ceiling fold parameterized by the axis owner's ledger, never four inlined per-site comparisons (the `[ADMISSION_FOLD]` research row enumerates the clauses and the finiteness-versus-clearance split). The fold runs inside one `content.graduate` OTel span weaving `beartype`, `msgspec`, OTel, the runtime `boundary` fault fence, and the `@receipted(_REDACTION)` egress aspect into a single fenced rail. Unlike `experiments/inference.md#BAYESIAN`, `experiments/model.md#ASSET`, and `experiments/study.md#STUDY` — which fit a bare receipt and compose emission inside the one fence as `boundary(lambda: _emit(_fit(...)))` because they carry no clearance stage — graduation owns a genuine two-stage rail: `boundary(_admit).bind(_clear).bind(boundary(_emit))` admits the finite ledger, folds the ceiling as a pure domain predicate, and runs emission inside a SECOND fence on the cleared `Ok` only. A handoff proposing a crossing without clearing its ceiling is an `Error(BoundaryFault)` the `.bind(_clear)` fold returns and the `_convert` weave records on the span, never an emitted `planned` receipt; the ceiling breach stays a returned domain `Error`, never a raise, while a refinement breach and an emit-time raise each fold onto the rail through their own fence. Emission is the decorator rail `observability/receipts.md#RECEIPT` declares, never an inline `Signals.emit` threaded through the body.

Graduation is a Python-branch-only concept. The receipt names the wire axis it crosses on and never names a C# interior owner row, mints a C# receipt, or authorizes product runtime behavior; the concrete C# owner consuming each axis is confirmed on the graduation task, not encoded in a routing literal that drifts.

## [01]-[INDEX]

- [01]-[GRADUATION]: the graduation receipt, the handoff-axis union, the `Ledger`/`Ceiling` `beartype.vale.Is` refinements, the `@beartype`-fenced `_admit` refinement gate `.bind`-threaded through the pure `_clear` ceiling fold and `.bind`-threaded through a second `boundary` over the `@receipted(_REDACTION)` `_emit` egress aspect, all woven under one `content.graduate` span with the `boundary` fault fence, and the `graduates` rail
- [02]-[CROSS_OWNER]: the routing rules gating each axis to its managed owner
- [03]-[RESEARCH]: the four-rejection-clause collapse onto the one residual-over-ceiling fold, and the woven `beartype`/`msgspec`/OTel/`boundary`/`@receipted` admission rail

## [02]-[GRADUATION]

- Owner: `GraduationReceipt` — the source-package, axis, evidence-key, and residual-ledger `Struct` wired through the runtime `ReceiptContributor` port; `HandoffAxis` the `@tagged_union` of handoff kinds, each case carrying the subject its kind discriminates. The `geometry` case carries the `GeometrySubject` literal so geometry-package evidence crosses on the one rail, the `unit_law`/`uncertainty_law` cases carry the policy-law subject, and the numeric cases carry their evidence subject. The axis case is the wire vocabulary; the C# owner that consumes it is never named in the receipt.
- Cases: `HandoffAxis` cases `solver`, `symbolic`, `model_asset`, `array_layout`, `unit_law`, `uncertainty_law`, `geometry`, `convex_program`, and `artifact`. The `geometry` case carries the thirteen geometry-minted `GeometrySubject` literals — `registration-transform`, `reconstructed-mesh`, `topology-graph`, `network-graph`, `form-finding`, `numerical-primitive`, `mesh-algebra`, `scan-deviation`, plus the differentiated `bim-compliance`/`bim-lifecycle`/`section-property` evidence classes and the energy-plane `building-energy`/`thermal-comfort` subjects; every other case carries its `str` evidence subject. The `convex_program` case carries the dual-certificate optimality proof distinct from the `solver` case's first-order convergence verdict, and the `unit_law`/`uncertainty_law` cases carry the policy-law subject crossing as policy evidence only.
- Scan-deviation decode half: `scan-deviation` is geometry-minted like every sibling — geometry `scan/deviation.md#DEVIATION` produces the deviation evidence and its literal, and this owner DECODES the subject on the `geometry` case, never implementing the scan-vs-model deviation kernel and never re-minting the vocabulary. The literal joins its twelve geometry-minted siblings — `registration-transform`, `reconstructed-mesh`, `topology-graph`, `network-graph`, `form-finding`, `numerical-primitive`, `mesh-algebra`, `bim-compliance`, `bim-lifecycle`, `section-property`, `building-energy`, `thermal-comfort` — on the one `GeometrySubject` union so the deviation handoff crosses the single geometry rail.
- Refinements: `Ledger` and `Ceiling` are the `Annotated[dict[str, float], Is[lambda m: all(isfinite(v) for v in m.values())]]` finiteness aliases the `@beartype(conf=FAULT_CONF)` fence on the inner `_admit` thunk checks, so a `NaN`/`±inf` entry raises the canonical `BeartypeCallHintViolation` INSIDE the `boundary` fence and the `CLASSIFY` `api` row folds it onto the rail rather than slipping past the `measured[k] <= cap` comparison. The sign is deliberately unconstrained so a negated-floor deficit (`experiments/inference.md#BAYESIAN` `neg_min_ess_bulk = -min(ess)`) admits; the `[ADMISSION_FOLD]` research row carries the breach semantics. The refinement is the input contract; the ceiling clearance is the admission predicate.
- Entry: `GraduationReceipt.graduates(source_package, axis, evidence_key, measured, ceiling)` is the ONE admission rail, woven under one `content.graduate` span as `boundary(f"graduation.{axis.tag}", _admit).bind(_clear).bind(lambda r: boundary(f"graduation.{axis.tag}", lambda: _emit(r)))`. The first `boundary` fences the `@beartype(conf=FAULT_CONF)`-fenced `_admit` thunk and mints exactly one `RuntimeRail[GraduationReceipt]`; `.bind(_clear)` threads the railed validated ledger through the pure ceiling fold — `Ok(GraduationReceipt(...))` when every measured residual clears its ceiling or `Error(BoundaryFault(boundary=(f"graduation.{tag}", "residual-ceiling")))` when any key exceeds it or a ceiling key is unmeasured; `.bind`-of-a-second-`boundary` runs the `@receipted(_REDACTION)` egress aspect INSIDE its own fence over the cleared `Ok` only, so emission rides the decorator rail and an emit-time raise (a render or sink failure) folds onto the rail rather than escaping `graduates` past a bare `.map(_emit)` — the sibling owners compose `_emit` inside the one fence as `boundary(lambda: _emit(_fit(...)))` for the same reason; graduation fences emit separately because its cleared `Ok` arrives only after the pure `_clear` bind the siblings lack. Three failure concerns stay distinct on three fences: the refinement breach is an exception the `_admit` fence converts (the `@beartype` raise lands as `Error` through the `CLASSIFY` `api` row), the ceiling rejection is a pure domain predicate the `.bind(_clear)` threads as a returned `Error` and never a raise, and the emit-time raise is the second fence's to convert — never a thunk returning `RuntimeRail[GraduationReceipt]` the fence re-wraps to `RuntimeRail[RuntimeRail[GraduationReceipt]]` then unwraps through an identity `.bind`.
- Span egress: the `Ok` arm sets `Status(StatusCode.OK)` and widens the span with `receipt.span_facts` behind the `is_recording()` gate, while the `@receipted` aspect the second `boundary` fences has already streamed the `planned` receipt through the canonical `Signals.emit` fold; the `Error` arm is `pass` because the fence's `_convert` already `record_exception`d the cause and set `Status(StatusCode.ERROR, fault.tag)` on the active span — for a refinement breach, a ceiling rejection, or an emit-time raise alike, since every fault arm on this rail routes through a `boundary` `_convert`. The axis/subject pair both arms need is the one pre-boundary `set_attributes` write behind the same `is_recording()` gate the `evidence/identity#IDENTITY` `derived` aspect and the sibling `graduation/codegen.md#STUB_CODEGEN` owner hold, so a no-op span pays no attribute build and the `Ok`-arm `span_facts` widens the recording span with the `evidence_key.hex`/`residual_count` facts the rejected handoff has no receipt to carry.
- Subject carrier: the `axis` argument IS the subject carrier — a `solver`/`symbolic`/`model_asset`/`array_layout`/`unit_law`/`uncertainty_law`/`geometry`/`convex_program`/`artifact` case — so a caller graduating geometry evidence passes `HandoffAxis(geometry="scan-deviation")` and the rail reads the subject off the case rather than a parallel field. The admitted ledger rides into the receipt as the cleared `residuals` map, so the receipt that exists is the one whose evidence passed — there is no admitted-but-unchecked state. The sibling owners (`solvers/receipt.md#RECEIPT`, `optimization/convex.md#CONVEX`, `experiments/inference.md#BAYESIAN`, `numerics/array.md#PAYLOAD`) feed their per-axis residual ledger through this one rail rather than each inlining its own admission comparison; `graduates` owns egress through its own fenced `_emit` `@receipted` weave, so a consumer reads the returned `RuntimeRail[GraduationReceipt]` and never re-wraps it under a second `@receipted` that would double-stream the receipt.
- Admission split: `_admit(measured, ceiling)` is the `@beartype(conf=FAULT_CONF)`-fenced refinement gate returning the validated `(measured, ceiling)` pair, and `_clear(source_package, axis, evidence_key, validated)` is the one pure total ceiling fold — `Ok(GraduationReceipt(...))` exactly when `measured.keys() >= ceiling.keys()` and `measured[k] <= cap` for every `(k, cap)` ceiling row, else `Error(BoundaryFault(boundary=(f"graduation.{axis.tag}", "residual-ceiling")))`. The per-axis rejection semantics every evidence owner declares — solver residual, convex duality gap, posterior rhat/ess deficit, array reproduction delta — are one residual-over-ceiling predicate parameterized by the ledger the axis owner supplies, never four parallel admission bodies; a stricter axis is a tighter ceiling row the caller passes, not a new gate. The fence sits on `_admit`, not `graduates`, so `_clear` folds only the finite ledger the refinement (Refinements row) already admitted.
- Axis fold: `_subject` is the one total `match` over the `HandoffAxis` union resolving every case to its `str` subject through a single structural or-pattern — the `geometry` case yields its `GeometrySubject` literal, every other case its carried subject, all nine tags binding one `s` capture rather than nine identical `return s` arms — closed by `assert_never` so a new handoff kind is a compile-surfaced gap, never a silent default. The fold is the single place the union is read; `subject`, `span_facts`, and `contribute` all route through it and never probe the union by `getattr(axis, axis.tag)`.
- Receipt: `GraduationReceipt.contribute` returns the one-element `tuple[Receipt, ...]` the `ReceiptContributor` port streams — `Receipt.of(self.source_package, ("planned", subject, facts))` because an admitted handoff is a wire proposal, never an emitted product receipt. The `planned` facts carry the axis tag, the resolved subject, the evidence key rendered through the canonical `ContentKey.hex` `{value:032x}:{fmt}` form the C# `InterchangeIdentity.Key` contract reads, and the cleared residual ledger as native `float` slots the `observability/receipts#RECEIPT` `dict[str, object]` `EventDict` and its `Encoder(enc_hook=repr, order="deterministic")` renderer serialize without a `str()` coerce — never a C# owner-row spelling and never a pre-`repr`-formatted ledger. `span_facts` is the one bounded `str | int` scalar source both the span and `contribute` read (the axis tag, the resolved subject, the `evidence_key.hex`, and the `residual_count`, exactly the set `Span.set_attributes` admits); the full residual ledger rides the receipt facts, never the span. The receipt carries no `bool` admitted flag because its existence IS the admission: a rejected handoff is an `Error` that never reaches `contribute`.
- Packages: `beartype` (`@beartype(conf=FAULT_CONF)` on `_admit` binding the one shared domain conf, `beartype.vale.Is` the `Ledger`/`Ceiling` finiteness refinement), `expression` (`tagged_union`/`case`/`tag`, `Ok`/`Error`, `Result.bind` the admission rail thread chaining the refinement fence, the pure `_clear` ceiling fold, and the fenced `_emit`, `Map.empty` the empty `Redaction.classified` table), `msgspec` (`Struct`), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span`/`Span.is_recording`/`Span.set_attributes`/`Span.set_status`/`Status`/`StatusCode` for the one `content.graduate` span), stdlib `math.isfinite` (the refinement predicate), runtime (`ContentKey`/`ContentKey.hex`, `RuntimeRail`/`BoundaryFault`/`boundary`/`FAULT_CONF`, `Receipt`/`ReceiptContributor`/`Redaction`/`@receipted` the egress aspect the second `boundary` over `_emit` fences).
- Growth: a new handoff kind is one `HandoffAxis` case plus one `_subject` match arm; a new geometry subject is one `GeometrySubject` literal; a stricter admission bar is one tighter ceiling row the caller supplies; zero new surface, no external-package member beyond the runtime port, the woven fault/contract/telemetry libraries, `expression`, and msgspec, no per-axis admission body, and no second egress beyond the one fenced `_emit` `@receipted` weave.
- Boundary: the receipt names the wire axis it crosses on, never the managed owner that receives it — no C# `ComputeReceipt`, benchmark claim, source generation, or C# interior owner-row spelling. compute emits the `scan-deviation` `GeometrySubject` and implements no geometry: the deviation kernel, the registration transform, the reconstruction mesh, and the topology/network graphs are geometry-branch owners that consume these literals. The deleted forms:
  - a handoff record claiming production readiness, a Python-only benchmark conclusion, or a C# source-shape claim absent from the C# owner planning.
  - a hard-coded C# `package#owner` routing literal where the wire axis case is the only cross-language vocabulary.
  - a parallel `subject: str` field racing the discriminant, where the axis case carries the subject.
  - a per-axis admission body duplicating the `_clear` fold, where one residual-over-ceiling predicate is parameterized by the axis owner's ledger.
  - a thunk returning `RuntimeRail[GraduationReceipt]` the `boundary` re-wraps to `RuntimeRail[RuntimeRail[GraduationReceipt]]` then unwraps through an identity `.bind`, where `_admit` returns the bare validated ledger and the ceiling fold is the one `.bind`.
  - a `Receipt.of("planned", owner, subject, facts)` four-positional call against the runtime two-argument `of(owner, evidence)` contract; a `contribute` returning a single `Receipt` against the `Iterable[Receipt]` port.
  - a `-inf`/`NaN` ledger entry reaching `measured[k] <= cap` where the `Is` finiteness refinement rails it first; a `>= 0.0` sign clause rejecting a legitimate negated-floor deficit (`experiments/inference.md#BAYESIAN` `neg_min_ess_bulk`).
  - a graduation materializing a receipt without clearing its residual ceiling; an untraced `_admit` running outside the one `content.graduate` span; a second `set_status(StatusCode.ERROR)` on the `Error` arm re-annotating the status the `boundary` fence's `_convert` already owns.
  - an inline `Signals.emit` threaded through the `graduates` body where the fenced `_emit` `@receipted(_REDACTION)` aspect owns egress as the decorator rail every sibling evidence owner rides (`experiments/study.md#STUDY` names the inline form deleted); an inline `structlog` line where the canonical `Signals.emit` fold the aspect drives owns receipt egress; a raw residual-ledger dump onto the span where only the bounded `span_facts` scalars are admissible.
  - a `.map(_emit)` running the `@receipted` egress aspect OUTSIDE the fault fence where an emit-time render or sink raise escapes `graduates` as an unconverted exception — the sibling owners compose `_emit` inside the one `boundary` thunk and graduation binds a second `boundary` over `_emit` for the same no-escape guarantee, so every fault arm on the rail routes through a `_convert`.

```python signature
from collections.abc import Iterable
from math import isfinite
from typing import Annotated, Final, Literal, assert_never

from beartype import beartype
from beartype.vale import Is
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, Redaction, receipted


# the geometry-minted DIFFERENTIATED 13-literal union, inherited byte-identical as wire data from
# `rasm.geometry.graduation.GeometrySubject`; a union change ships as a geometry ripple, never a local re-shape.
type GeometrySubject = Literal[
    "registration-transform",
    "reconstructed-mesh",
    "topology-graph",
    "network-graph",
    "form-finding",
    "numerical-primitive",
    "mesh-algebra",
    "scan-deviation",
    "bim-compliance",
    "bim-lifecycle",
    "section-property",
    "building-energy",
    "thermal-comfort",
]
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
    geometry: GeometrySubject = case()
    convex_program: str = case()
    artifact: str = case()


# --- [CONSTANTS] ------------------------------------------------------------------------

_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # graduation facts carry no secret field
_TRACER: Final[trace.Tracer] = trace.get_tracer("compute.graduation")

# --- [MODELS] ---------------------------------------------------------------------------


class GraduationReceipt(Struct, frozen=True):
    source_package: str
    axis: HandoffAxis
    evidence_key: ContentKey
    residuals: dict[str, float]

    @staticmethod
    def graduates(
        source_package: str, axis: HandoffAxis, evidence_key: ContentKey, measured: dict[str, float], ceiling: dict[str, float]
    ) -> RuntimeRail[GraduationReceipt]:
        # one fenced rail: `boundary(_admit)` mints exactly one `RuntimeRail` over the refinement check,
        # `.bind(_clear)` threads the pure ceiling fold, and `.bind(boundary(_emit))` runs the `@receipted`
        # egress aspect INSIDE a second fence on the cleared `Ok` only — so an emit-time raise folds onto
        # the rail exactly as the sibling `_emit(_fit(...))`-inside-`boundary` weave holds, never escaping
        # `graduates` past a bare `.map`; admission, ceiling rejection, and emission stay one rail.
        with _TRACER.start_as_current_span("content.graduate") as span:
            if span.is_recording():
                span.set_attributes({"axis": axis.tag, "subject": GraduationReceipt._subject(axis)})
            rail = (
                boundary(f"graduation.{axis.tag}", lambda: GraduationReceipt._admit(measured, ceiling))
                .bind(lambda validated: GraduationReceipt._clear(source_package, axis, evidence_key, validated))
                .bind(lambda receipt: boundary(f"graduation.{axis.tag}", lambda: GraduationReceipt._emit(receipt)))
            )
            match rail:
                case Ok(receipt):
                    if span.is_recording():
                        span.set_attributes(receipt.span_facts)  # the rejected handoff has no receipt to widen with
                    span.set_status(Status(StatusCode.OK))  # the ERROR side is the `boundary` fence's `_convert`, never re-set here
                case Error(_):
                    pass
            return rail

    @staticmethod
    @receipted(_REDACTION)
    def _emit(receipt: GraduationReceipt) -> GraduationReceipt:
        return receipt

    @property
    def subject(self) -> str:
        return GraduationReceipt._subject(self.axis)

    @property
    def span_facts(self) -> dict[str, str | int]:
        # the four bounded `str | int` scalars `Span.set_attributes` admits; the full `residuals`
        # ledger (a `dict[str, float]`) is not an attribute value and rides the receipt facts only.
        return {"axis": self.axis.tag, "subject": self.subject, "evidence_key": self.evidence_key.hex, "residual_count": len(self.residuals)}

    def contribute(self) -> Iterable[Receipt]:
        # the runtime `Receipt.of(owner, evidence)` two-argument contract: the `(Phase, subject, facts)`
        # triple mints the `fact` case at `planned`. Native `float` residuals ride the `EventDict`
        # `dict[str, object]` slots the `enc_hook=repr` renderer serializes without a `str()` coerce.
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
```

## [03]-[CROSS_OWNER]

Each axis crosses the graduation-evidence wire under one admission gate: the evidence owner supplies its measured `Ledger` and the `Ceiling` that axis must clear to `GraduationReceipt.graduates`, and a ledger key above its ceiling is an `Error(BoundaryFault)` the `_convert` weave records on the span and propagates on the rail rather than a graduated handoff — no `planned` receipt is emitted for a crossing that did not clear. The wire axis case is the only cross-language vocabulary the receipt carries; the concrete C# owner that consumes an axis is confirmed against the C# owner planning on the graduation task, never an interior owner-row spelling baked into this owner.

- Solver evidence crosses on the `solver` case once the `SolverReceipt` convergence verdict from `solvers/receipt.md#RECEIPT` holds; the design and program optima from `optimization/design.md#DESIGN` and `optimization/program.md#PROGRAM` reuse this case (a stationary-point or `OptimizeResult` verdict is a convergence verdict, never a separate case).
- Convex-program evidence crosses on the `convex_program` case once the `ConvexReceipt` KKT-gap certificate from `optimization/convex.md#CONVEX` holds — the dual multipliers and the complementary-slackness KKT gap are the global-optimality proof object, a distinct admission from the first-order convergence verdict the `solver` case carries, so a returned point whose gap exceeds the tolerance is an admission rejection.
- Symbolic derivation crosses on the `symbolic` case only after the derivation is stable and reproducible — the sympy codegen handoff from `analysis/symbolic.md#DERIVATION`.
- Model-asset evidence crosses on the `model_asset` case only after the `ModelAssetManifest` validation from `experiments/model.md#ASSET` passes.
- Unit-law and uncertainty-law evidence crosses on the `unit_law` and `uncertainty_law` cases as policy evidence only: the `unit_law` case carries the `numerics/quantity.md#QUANTITY` pint unit-algebra dimensional-consistency subject (a unit-bearing quantity whose dimensional reduction is consistent under the unit algebra), and the `uncertainty_law` case carries the posterior-diagnostics subject the `InferenceReceipt` convergence diagnostics from `experiments/inference.md#BAYESIAN` gate against the rhat-and-ess residual-limits check, so a quantity failing dimensional consistency or a posterior failing the rhat-and-ess bar is an admission rejection.
- Array-layout evidence crosses on the `array_layout` case once the `ArrayPayload` content key from `numerics/array.md#PAYLOAD` reproduces bit-identically across backends.
- Geometry evidence crosses on the `geometry` case, the single rail geometry-package evidence crosses, with each `GeometrySubject` literal mapped to its C# wire-seam owner on the graduation task as a cross-language confirmation. compute DECODES every literal off the geometry-minted union: the `scan-deviation` literal produced by geometry `scan/deviation.md#DEVIATION`, the `reconstructed-mesh` literal produced by geometry `scan/reconstruction.md#RECONSTRUCTION` and `mesh/repair.md#MESH` (and the distinct non-mesh boundary the compute `analysis/spatial.md#SPATIAL` owner crosses on the same case aligned to the scan companion), and the `registration-transform`/`topology-graph`/`network-graph`/`form-finding`/`numerical-primitive`/`mesh-algebra` literals their geometry-branch owners produce (the `form-finding`, `numerical-primitive`, and `mesh-algebra` subjects crossing from geometry `graph/algebra.md#ALGEBRA` keyed per `compas` dispatch case, the `network-graph` subject crossing from BOTH geometry `graph/algebra.md#ALGEBRA` — the `compas`-adjacency producer — AND geometry `graph/features.md#FEATURES` — the distinct mesh-feature-projection producer over `trimesh`/`networkx`, two co-producers of the one literal on the single geometry rail the dual-producer arrangement both sibling pages declare, never folded into one owner — the `topology-graph` subject from geometry `graph/nonmanifold.md`; the differentiated `bim-compliance` literal from geometry `ifc/analysis.md` — IDS/clash/BCF verdict evidence, `bim-lifecycle` from geometry `ifc/costing.md` — 5D/4D lifecycle evidence, `section-property` from geometry `ifc/structural.md` — section-integral evidence, and the energy-plane `building-energy` literal from geometry `energy/model.md`/`energy/district.md`/`energy/simulate.md` beside `thermal-comfort` from geometry `energy/climate.md`); this owner decodes each literal and implements none of the geometry kernels.

## [04]-[RESEARCH]

- [ADMISSION_FOLD]: the four sibling rejection clauses every evidence owner declares — `solvers/receipt.md#RECEIPT` solver residual above tolerance, `optimization/convex.md#CONVEX` KKT duality gap above tolerance, `experiments/inference.md#BAYESIAN` posterior failing the rhat-and-ess bar, `numerics/array.md#PAYLOAD` array layout failing bit-identical reproduction — collapse onto the one `_clear` residual-over-ceiling fold (`measured.keys() >= ceiling.keys() and all(measured[k] <= cap for k, cap in ceiling.items())`), never four inlined per-site comparisons. The axis owner's choice of bar is data, not a code arm: a stricter axis is a tighter `Ceiling` row the caller passes, so `_clear` is total over every axis by construction and a new axis adds zero admission body. The clause set is two orthogonal concerns kept distinct — the `Ledger`/`Ceiling` = `Annotated[dict[str, float], Is[lambda m: all(isfinite(v) for v in m.values())]]` finiteness refinement is the INPUT contract the `@beartype(conf=FAULT_CONF)` fence on `_admit` checks (`.api/beartype.md` `vale.Is` ENTRYPOINTS, the O(1) `Annotated` boundary check), and the ceiling clearance is the admission PREDICATE the pure `_clear` `.bind` threads. The sign is deliberately unconstrained because `experiments/inference.md#BAYESIAN` feeds `neg_min_ess_bulk = -min(ess)` so the one `measured <= ceiling` fold reads an ess floor as a max-deficit; a `>= 0.0` clause would reject that legitimate negated-floor deficit at the fence. A `-inf` residual silently clearing any ceiling and a `NaN` silently rejecting every ceiling are the two breaches the finiteness `Is` rails before either reaches `measured[k] <= cap`, the violation raising the canonical `BeartypeCallHintViolation` (`.api/beartype.md` `violation_type` redirect to the shared `FAULT_CONF`) the `reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail.
- [WOVEN_RAIL]: the admission is one integrated rail composing the admitted libraries as a single flow rather than flat per-library uses — `boundary(f"graduation.{axis.tag}", _admit).bind(_clear).bind(lambda r: boundary(f"graduation.{axis.tag}", lambda: _emit(r)))`. `beartype.vale.Is` refines the `Ledger`/`Ceiling` inputs on the shared `Annotated` aliases the `@beartype(conf=FAULT_CONF)` boundary checks on the inner `_admit` thunk (NOT on `graduates`, so a refinement breach raises inside the `boundary` fence and folds onto the rail, never escaping the owner); the runtime `boundary` (`reliability/faults#FAULT`) fences that thunk and `Result.bind`s the bare validated ledger through the pure `_clear` ceiling fold so the rail stays one `RuntimeRail[GraduationReceipt]` rather than the double-wrapped `RuntimeRail[RuntimeRail[...]]` a thunk returning a rail would force; a second `.bind` over a `boundary` wrapping `_emit` then weaves the `observability/receipts#RECEIPT` `@receipted(_REDACTION)` egress aspect over the cleared `Ok` only INSIDE its own fence, so the canonical `Signals.emit` fold streams the `planned` receipt as the decorator rail rather than an inline body call AND an emit-time raise folds onto the rail rather than escaping the owner. The siblings `experiments/inference.md#BAYESIAN`/`experiments/model.md#ASSET`/`experiments/study.md#STUDY` compose emission inside the one fence as `boundary(lambda: _emit(_fit(...)))` because they fit a bare receipt with no clearance stage; graduation cannot fold `_emit` into the first thunk because `_clear` is a pure rail-returning domain predicate that must precede emission, so it fences emission in a second `boundary` to keep the same no-escape guarantee — a bare `.map(_emit)` running emit outside any fence is the deleted form. `msgspec` carries the `GraduationReceipt` `Struct` and the native-`float` residual ledger the `observability/receipts#RECEIPT` `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()` coerce; the OTel API `trace.get_tracer`/`start_as_current_span` opens the one `content.graduate` span carrying ONLY the bounded `span_facts` `str | int` scalar map behind the `is_recording()` gate (`.api/opentelemetry-api.md` `set_attributes` admits `str | bool | int | float | Sequence[...]`, so the full residual `dict[str, float]` rides the receipt facts, never the span) with `Status(StatusCode.OK)` set on the `Ok` arm only — the success annotation the `boundary` fence's `_convert` does not own, the `Status(StatusCode.ERROR, fault.tag)` egress left to the conversion that `record_exception`s the cause on the same active span (the `evidence/identity#IDENTITY` `derived` span-then-fence discipline, never a second `set_status` trample), the `Error` arm a bare `pass`. The `Ok`-arm `span_facts` write widens the recording span with the `evidence_key.hex`/`residual_count` facts the pre-boundary `axis`/`subject` write does not carry, so a no-op span pays no attribute build and the returned `RuntimeRail[GraduationReceipt]` is the consumer's, never re-wrapped under a second `@receipted` that would double-stream the receipt.
