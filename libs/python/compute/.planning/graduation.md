# [PY_COMPUTE_GRADUATION]

The C# graduation receipt. `GraduationReceipt` moves useful Python evidence into the C# owner system: source-pkg/target-C#-owner/subject/evidence-bundle/residual-limits/decision-route. `HandoffAxis` is the Literal-discriminated union of handoff kinds; the geometry case carries registration-transform/reconstructed-mesh/topology-graph/form-finding subjects so geometry-package evidence reaches the C# owner system through the one graduation rail. Graduation is a Python-branch-only concept; no C# graduation page exists, so the geometry case is added freely. The receipt describes handoff; it never mints C# receipts or authorizes product runtime behavior.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]   | [OWNS]                                                          |
| :-----: | :---------- | :------------------------------------------------------------- |
|   [1]   | GRADUATION  | the graduation receipt, the handoff axis, cross-owner rules    |

## [2]-[GRADUATION]

- Owner: `GraduationReceipt` — the source-pkg/target-C#-owner/subject/evidence-bundle/residual-limits/decision-route record; `HandoffAxis` the Literal-discriminated union of handoff kinds; wired through runtime `ReceiptContributor`.
- Cases: `HandoffAxis` literals `solver` · `symbolic` · `model-asset` · `array-layout` · `unit-law` · `uncertainty-law` · `geometry` — the geometry case carries the `GeometrySubject` literal `registration-transform` · `reconstructed-mesh` · `topology-graph` · `form-finding`.
- Entry: `GraduationReceipt.of` builds the receipt from a study receipt, artifact bundle, data exchange bundle, model-asset manifest, or geometry evidence and returns the C# handoff record; `GraduationReceipt.route` resolves the target C# owner from the axis.
- Auto: solver/symbolic evidence routes to `Rasm.Compute`; model-asset evidence routes to the C# model lane only after the manifest validation passes; geometry evidence routes to the C# owner system carrying its `GeometrySubject`; the residual-limits field gates the decision route.
- Receipt: the graduation contributes a `Receipt.planned` row through `ReceiptContributor` (it is a handoff proposal, never an emitted product receipt).
- Packages: `msgspec`, runtime (`ReceiptContributor`/`ContentKey`).
- Growth: a new handoff kind is one `HandoffAxis` literal; a new geometry subject is one `GeometrySubject` literal; zero new surface.
- Boundary: no C# `ComputeReceipt`, benchmark claim, or source generation; a handoff record claiming production readiness, a graduation without data/artifact evidence, a Python-only benchmark conclusion, and a C# source-shape claim absent from the C# owner planning pages are the deleted forms; this owner is FINALIZED (no external-package member, only the runtime port and msgspec).

```python signature
from typing import Literal

from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.observability import Receipt


type HandoffAxis = Literal["solver", "symbolic", "model-asset", "array-layout", "unit-law", "uncertainty-law", "geometry"]
type GeometrySubject = Literal["registration-transform", "reconstructed-mesh", "topology-graph", "form-finding"]


class GraduationReceipt(Struct, frozen=True):
    source_package: str
    target_csharp_owner: str
    axis: HandoffAxis
    subject: str
    evidence_key: ContentKey
    residual_limits: dict[str, float]
    decision_route: str

    def contribute(self) -> Receipt:
        return Receipt.Planned(self.source_package, self.target_csharp_owner, {"axis": self.axis})

    @staticmethod
    def route(axis: HandoffAxis) -> str:
        match axis:
            case "geometry":
                return "Rasm.Compute#interchange"
            case "model-asset":
                return "Rasm.Compute#model-lane"
            case _:
                return "Rasm.Compute"
```

## [3]-[CROSS_OWNER_RULES]

- Solver evidence routes to `Rasm.Compute` unless it belongs in the RhinoCommon-aware kernel.
- Symbolic derivation routes to C# source owners only after the derivation is stable and reproducible (the sympy codegen handoff).
- Model-asset evidence routes to the C# model lane only after the `ModelAssetManifest` validation passes.
- Unit and uncertainty evidence routes to the C# quantity/unit owners as policy rows only.
- Geometry evidence (registration transform, reconstructed mesh, topology graph, form-finding) routes to the C# owner system through the geometry `HandoffAxis` case, the single rail geometry-package evidence crosses.
