# [PY_COMPUTE_HANDOFF]

The Python-only graduation rail moving offline evidence outward across the graduation-evidence wire. `GraduationReceipt` carries the source package, the handoff axis, the evidence key, and the residual ledger across one handoff; `HandoffAxis` is the tagged union of handoff kinds, each case carrying the subject its kind names — the `geometry` case carrying the `GeometrySubject` literal, the policy cases carrying the unit-and-uncertainty law subject, and the numeric cases carrying their evidence subject. The axis case IS the subject: there is no parallel `subject: str` field to drift against the discriminant, because the subject lives inside the case the axis selects. The admission gate is the owner's reason to exist: `GraduationReceipt.graduates` folds the measured residual ledger against the per-key residual ceiling and returns `RuntimeRail[GraduationReceipt]`, so the four sibling rejection clauses every evidence owner declares — a solver residual above tolerance, a convex duality gap above tolerance, a posterior failing the rhat-and-ess bar, an array layout that fails bit-identical reproduction — are the one residual-over-ceiling fold here, never four ad-hoc inlined comparisons at each call site. A handoff that proposes crossing without clearing its ceiling is an `Error(BoundaryFault)` on the rail, never an emitted receipt. Graduation is a Python-branch-only concept: the receipt describes a handoff against the wire and never names a C# interior owner row, mints a C# receipt, or authorizes product runtime behavior. The concrete C# owner that consumes each axis is confirmed on the graduation task, not encoded in a routing literal that drifts.

## [01]-[INDEX]

- [01]-[GRADUATION]: the graduation receipt, the handoff-axis union, the residual-ceiling admission fold, and the `graduates` rail
- [02]-[CROSS_OWNER]: the routing rules gating each axis to its managed owner

## [02]-[GRADUATION]

- Owner: `GraduationReceipt` — the source-package, axis, evidence-key, and residual-ledger record wired through runtime `ReceiptContributor`; `HandoffAxis` is the `@tagged_union` of handoff kinds and each case carries the subject its kind discriminates. The `geometry` case carries the `GeometrySubject` literal so geometry-package evidence crosses on the one rail, the `unit_law`/`uncertainty_law` cases carry the policy-law subject, and the numeric cases carry their evidence subject. The axis case is the wire vocabulary; the C# owner that consumes it is never named in the receipt.
- Cases: `HandoffAxis` cases `solver`, `symbolic`, `model_asset`, `array_layout`, `unit_law`, `uncertainty_law`, `geometry`, and `convex_program`. The `geometry` case carries the `GeometrySubject` literals `registration-transform`, `reconstructed-mesh`, `topology-graph`, `network-graph`, `form-finding`, `numerical-primitive`, `mesh-algebra`, and `scan-deviation`; every other case carries its `str` evidence subject. The `convex_program` case carries the dual-certificate optimality proof distinct from the `solver` case's first-order convergence verdict, and the `unit_law`/`uncertainty_law` cases carry the policy-law subject crossing as policy evidence only.
- Scan-deviation producer half: `scan-deviation` is the PRODUCER literal compute emits — compute mints the `GeometrySubject` case the geometry `scan/deviation.md#DEVIATION` owner consumes; this owner emits the subject the deviation owner reads and never implements the scan-vs-model deviation kernel. The literal joins `registration-transform`, `reconstructed-mesh`, `topology-graph`, `network-graph`, `form-finding`, `numerical-primitive`, and `mesh-algebra` on the one `GeometrySubject` union so the deviation handoff crosses the single geometry rail.
- Entry: `GraduationReceipt.graduates(source_package, axis, evidence_key, measured, ceiling)` is the ONE admission rail — it folds the measured residual ledger against the per-key ceiling through `_admit` and returns `RuntimeRail[GraduationReceipt]`: `Ok(GraduationReceipt(...))` when every measured residual clears its ceiling, `Error(BoundaryFault(boundary=("graduation.<tag>", "residual-ceiling")))` when any key exceeds it or a ceiling key is unmeasured, so a handoff proposal that has not cleared its evidence bar never materializes a receipt. The `axis` argument IS the subject carrier — a `solver`/`symbolic`/`model_asset`/`array_layout`/`unit_law`/`uncertainty_law`/`geometry`/`convex_program` case — so a caller graduating geometry evidence passes `HandoffAxis(geometry="scan-deviation")` and the rail reads the subject off the case rather than a parallel field. The admitted residual ledger rides into the receipt as the cleared `residuals` map, so the receipt that exists is the one whose evidence passed — there is no admitted-but-unchecked state. The sibling owners (`solvers/receipt.md#RECEIPT`, `optimization/convex.md#CONVEX`, `experiments/inference.md#BAYESIAN`, `numerics/array.md#PAYLOAD`) feed their per-axis residual ledger through this one rail rather than each inlining its own admission comparison.
- Admission fold: `_admit(tag, measured, ceiling)` is the one total fold gating the handoff — `Ok(measured)` exactly when `measured.keys() >= ceiling.keys()` and `measured[k] <= ceiling[k]` for every ceiling key, else `Error(BoundaryFault(boundary=(f"graduation.{tag}", "residual-ceiling")))`. The per-axis rejection semantics every evidence owner declares — solver residual, convex duality gap, posterior rhat/ess deficit, array reproduction delta — are one residual-over-ceiling predicate parameterized by the ledger the axis owner supplies, never four parallel admission bodies; a stricter axis is a tighter ceiling row the caller passes, not a new gate.
- Axis fold: `_subject` is the one total `match` over the `HandoffAxis` union resolving each case to its `str` subject — the `geometry` case yields its `GeometrySubject` literal, every other case yields its carried subject — closed by `assert_never` so a new handoff kind is a compile-surfaced gap, never a silent default. The fold is the single place the union is read; `subject` and `contribute` both route through it and never probe the union by `getattr`.
- Receipt: `GraduationReceipt.contribute` emits one `Receipt.of("planned", ...)` row through `ReceiptContributor` because an admitted handoff is a wire proposal, never an emitted product receipt; the planned facts carry the axis tag, the resolved subject, the evidence key rendered through the canonical `ContentKey.hex` form the C# `ArtifactKey` contract reads, and the cleared residual ledger — never a C# owner-row spelling. The receipt carries no `bool` admitted flag because its existence IS the admission: a rejected handoff is an `Error` that never reaches `contribute`.
- Packages: `expression` (`tagged_union`/`case`/`tag`, `Ok`/`Error`), `msgspec` (`Struct`), runtime (`ContentKey`, `RuntimeRail`/`BoundaryFault`, `Receipt`/`ReceiptContributor`).
- Growth: a new handoff kind is one `HandoffAxis` case plus one `_subject` match arm; a new geometry subject is one `GeometrySubject` literal; a stricter admission bar is one tighter ceiling row the caller supplies; zero new surface, no external-package member beyond the runtime port, `expression`, and msgspec, no per-axis admission body.
- Boundary: no C# `ComputeReceipt`, benchmark claim, source generation, or C# interior owner-row spelling; the receipt names the wire axis it crosses on, not the managed owner that receives it. compute emits the `scan-deviation` `GeometrySubject` and never implements geometry — the deviation kernel, the registration transform, the reconstruction mesh, and the topology/network graphs are geometry-branch owners that consume these literals. A handoff record claiming production readiness, a graduation that materializes a receipt without clearing its residual ceiling, a Python-only benchmark conclusion, a hard-coded C# package#owner routing literal, a parallel `subject: str` field racing the discriminant, a per-axis admission body duplicating the `_admit` fold, and a C# source-shape claim absent from the C# owner planning are the deleted forms.

```python signature
from typing import Literal, assert_never

from expression import Error, Ok, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.receipts import Receipt


type GeometrySubject = Literal[
    "registration-transform", "reconstructed-mesh", "topology-graph", "network-graph",
    "form-finding", "numerical-primitive", "mesh-algebra", "scan-deviation",
]


@tagged_union(frozen=True)
class HandoffAxis:
    tag: Literal[
        "solver", "symbolic", "model_asset", "array_layout", "unit_law", "uncertainty_law", "geometry", "convex_program"
    ] = tag()
    solver: str = case()
    symbolic: str = case()
    model_asset: str = case()
    array_layout: str = case()
    unit_law: str = case()
    uncertainty_law: str = case()
    geometry: GeometrySubject = case()
    convex_program: str = case()


class GraduationReceipt(Struct, frozen=True):
    source_package: str
    axis: HandoffAxis
    evidence_key: ContentKey
    residuals: dict[str, float]

    @staticmethod
    def graduates(
        source_package: str,
        axis: HandoffAxis,
        evidence_key: ContentKey,
        measured: dict[str, float],
        ceiling: dict[str, float],
    ) -> RuntimeRail[GraduationReceipt]:
        return GraduationReceipt._admit(axis.tag, measured, ceiling).map(
            lambda cleared: GraduationReceipt(
                source_package=source_package, axis=axis, evidence_key=evidence_key, residuals=cleared
            )
        )

    @property
    def subject(self) -> str:
        return GraduationReceipt._subject(self.axis)

    def contribute(self) -> Receipt:
        subject = GraduationReceipt._subject(self.axis)
        facts = {"axis": self.axis.tag, "evidence_key": self.evidence_key.hex, "subject": subject}
        return Receipt.of("planned", self.source_package, subject, facts | {k: repr(v) for k, v in self.residuals.items()})

    @staticmethod
    def _admit(tag: str, measured: dict[str, float], ceiling: dict[str, float]) -> RuntimeRail[dict[str, float]]:
        cleared = measured.keys() >= ceiling.keys() and all(measured[k] <= cap for k, cap in ceiling.items())
        return Ok(measured) if cleared else Error(BoundaryFault(boundary=(f"graduation.{tag}", "residual-ceiling")))

    @staticmethod
    def _subject(axis: HandoffAxis) -> str:
        match axis:
            case HandoffAxis(tag="solver", solver=s):
                return s
            case HandoffAxis(tag="symbolic", symbolic=s):
                return s
            case HandoffAxis(tag="model_asset", model_asset=s):
                return s
            case HandoffAxis(tag="array_layout", array_layout=s):
                return s
            case HandoffAxis(tag="unit_law", unit_law=s):
                return s
            case HandoffAxis(tag="uncertainty_law", uncertainty_law=s):
                return s
            case HandoffAxis(tag="geometry", geometry=s):
                return s
            case HandoffAxis(tag="convex_program", convex_program=s):
                return s
            case _:
                assert_never(axis)
```

## [03]-[CROSS_OWNER]

Each axis crosses the graduation-evidence wire under one admission gate: the evidence owner supplies its measured residual ledger and the ceiling that axis must clear to `GraduationReceipt.graduates`, and a ledger key above its ceiling is an `Error(BoundaryFault)` on the rail rather than a graduated handoff. The wire axis case is the only cross-language vocabulary the receipt carries; the concrete C# owner that consumes an axis is confirmed against the C# owner planning on the graduation task, never an interior owner-row spelling baked into this owner.

- Solver evidence crosses on the `solver` case once the `SolverReceipt` convergence verdict from `solvers/receipt.md#RECEIPT` holds; the design and program optima from `optimization/design.md#DESIGN` and `optimization/program.md#PROGRAM` reuse this case (a stationary-point or `OptimizeResult` verdict is a convergence verdict, never a separate case).
- Convex-program evidence crosses on the `convex_program` case once the `ConvexReceipt` KKT-gap certificate from `optimization/convex.md#CONVEX` holds — the dual multipliers and the complementary-slackness KKT gap are the global-optimality proof object, a distinct admission from the first-order convergence verdict the `solver` case carries, so a returned point whose gap exceeds the tolerance is an admission rejection.
- Symbolic derivation crosses on the `symbolic` case only after the derivation is stable and reproducible — the sympy codegen handoff from `analysis/symbolic.md#DERIVATION`.
- Model-asset evidence crosses on the `model_asset` case only after the `ModelAssetManifest` validation from `experiments/model.md#ASSET` passes.
- Unit-law and uncertainty-law evidence crosses on the `unit_law` and `uncertainty_law` cases as policy evidence only: the `unit_law` case carries the `numerics/quantity.md#QUANTITY` pint unit-algebra dimensional-consistency subject (a unit-bearing quantity whose dimensional reduction is consistent under the unit algebra), and the `uncertainty_law` case carries the posterior-diagnostics subject the `InferenceReceipt` convergence diagnostics from `experiments/inference.md#BAYESIAN` gate against the rhat-and-ess residual-limits check, so a quantity failing dimensional consistency or a posterior failing the rhat-and-ess bar is an admission rejection.
- Array-layout evidence crosses on the `array_layout` case once the `ArrayPayload` content key from `numerics/array.md#PAYLOAD` reproduces bit-identically across backends.
- Geometry evidence crosses on the `geometry` case, the single rail geometry-package evidence crosses, with each `GeometrySubject` literal mapped to its C# wire-seam owner on the graduation task as a cross-language confirmation. compute is the PRODUCER of the `scan-deviation` literal consumed by geometry `scan/deviation.md#DEVIATION`, the `reconstructed-mesh` literal consumed by geometry `scan/reconstruction.md#RECONSTRUCTION` and `mesh/repair.md#MESH` (and the distinct non-mesh boundary the compute `analysis/spatial.md#SPATIAL` owner crosses on the same case aligned to the scan companion), and the `registration-transform`/`topology-graph`/`network-graph`/`form-finding`/`numerical-primitive`/`mesh-algebra` literals their geometry-branch owners consume (the `network-graph`, `form-finding`, `numerical-primitive`, and `mesh-algebra` subjects crossing from geometry `graph/algebra.md#ALGEBRA` keyed per `compas` dispatch case, the `topology-graph` subject from geometry `graph/nonmanifold.md`); this owner emits each literal and implements none of the geometry kernels.
