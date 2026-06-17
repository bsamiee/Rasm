# [PY_COMPUTE_GRADUATION]

The Python-only graduation rail moving offline evidence outward across the graduation-evidence wire. `GraduationReceipt` carries the source package, the handoff axis, subject, evidence key, and residual limits across one handoff. `HandoffAxis` is the Literal-discriminated union of handoff kinds, and the geometry case carries the `GeometrySubject` literals so geometry-package evidence reaches the managed owner system through the one rail. Graduation is a Python-branch-only concept: the receipt describes a handoff against the wire and never names a C# interior owner row, mints a C# receipt, or authorizes product runtime behavior. The concrete C# owner that consumes each axis is confirmed on the graduation task, not encoded in a routing literal that drifts.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]   | [OWNS]                                                          |
| :-----: | :---------- | :------------------------------------------------------------- |
|   [1]   | GRADUATION  | the graduation receipt and the handoff axis                    |
|   [2]   | CROSS_OWNER | the routing rules gating each axis to its managed owner         |

## [2]-[GRADUATION]

- Owner: `GraduationReceipt` — the source-package, axis, subject, evidence-key, and residual-limits record wired through runtime `ReceiptContributor`; `HandoffAxis` is the Literal-discriminated union of handoff kinds. The geometry case carries the `GeometrySubject` literal so geometry-package evidence crosses on the one rail. The axis is the wire vocabulary; the C# owner that consumes it is never named in the receipt.
- Cases: `HandoffAxis` literals `solver`, `symbolic`, `model-asset`, `array-layout`, `unit-law`, `uncertainty-law`, and `geometry`; the geometry axis carries the `GeometrySubject` literals `registration-transform`, `reconstructed-mesh`, `topology-graph`, `network-graph`, and `form-finding`.
- Entry: `GraduationReceipt.of` builds the receipt from a solver, symbolic, model-asset, study, or geometry evidence carrier and returns the handoff record keyed by axis and evidence. The receipt contributes a `Receipt.Planned` row through `ReceiptContributor` because it is a handoff proposal, never an emitted product receipt, and the planned facts carry the axis and the evidence key — never a C# owner-row spelling.
- Packages: `msgspec`, runtime (`ContentKey`, `Receipt`, `ReceiptContributor`).
- Growth: a new handoff kind is one `HandoffAxis` literal; a new geometry subject is one `GeometrySubject` literal; zero new surface, no external-package member beyond the runtime port and msgspec.
- Boundary: no C# `ComputeReceipt`, benchmark claim, source generation, or C# interior owner-row spelling; the receipt names the wire axis it crosses on, not the managed owner that receives it. A handoff record claiming production readiness, a graduation without evidence, a Python-only benchmark conclusion, a hard-coded C# package#owner routing literal, and a C# source-shape claim absent from the C# owner planning are the deleted forms.

```python signature
from typing import Literal

from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.observability.receipts import Receipt


type HandoffAxis = Literal["solver", "symbolic", "model-asset", "array-layout", "unit-law", "uncertainty-law", "geometry"]
type GeometrySubject = Literal["registration-transform", "reconstructed-mesh", "topology-graph", "network-graph", "form-finding"]


class GraduationReceipt(Struct, frozen=True):
    source_package: str
    axis: HandoffAxis
    subject: str
    evidence_key: ContentKey
    residual_limits: dict[str, float]

    def contribute(self) -> Receipt:
        return Receipt.Planned(
            self.source_package,
            self.subject,
            {"axis": self.axis, "evidence_key": self.evidence_key.value, "subject": self.subject},
        )
```

## [3]-[CROSS_OWNER]

Each axis crosses the graduation-evidence wire under its own admission gate. The wire axis is the only cross-language vocabulary the receipt carries; the concrete C# owner that consumes an axis is confirmed against the C# owner planning on the graduation task, never an interior owner-row spelling baked into this owner.

- Solver evidence crosses on the `solver` axis once the `SolverReceipt` convergence verdict from `solvers/receipt.md#RECEIPT` holds.
- Symbolic derivation crosses on the `symbolic` axis only after the derivation is stable and reproducible — the sympy codegen handoff from `symbolics/derivation.md#DERIVATION`.
- Model-asset evidence crosses on the `model-asset` axis only after the `ModelAssetManifest` validation from `models/asset.md#ASSET` passes.
- Unit and uncertainty evidence crosses on the `unit-law` and `uncertainty-law` axes as policy evidence only; the `InferenceReceipt` convergence diagnostics from `inference/bayesian.md#BAYESIAN` gate the uncertainty-law residual-limits check, and a posterior failing the rhat-and-ess bar is an admission rejection.
- Array-layout evidence crosses on the `array-layout` axis once the `ArrayPayload` content key from `arrays/payload.md#PAYLOAD` reproduces bit-identically across backends.
- Geometry evidence crosses on the `geometry` axis, the single rail geometry-package evidence crosses, with each `GeometrySubject` literal mapped to its C# wire-seam owner on the graduation task as a cross-language confirmation.
