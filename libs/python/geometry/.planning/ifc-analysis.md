# [PY_GEOMETRY_IFC_ANALYSIS]

IFC property/quantity/relationship analysis — the AEC verbs the tessellation hop alone drops. `IfcAnalysis` runs quantity takeoff, clash detection, space-program validation, Pset/schedule queries, and IDS-style model-checking over `ifcopenshell.util.{element,placement,unit,selector}` and `ifcopenshell.api`, emitting a geometry receipt that graduates through the compute `HandoffAxis` geometry case. The C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the analysis verbs the managed projection does not perform.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                  |
| :-----: | :-------- | :------------------------------------------------------ |
|   [1]   | ANALYSIS  | quantity takeoff, clash, space-program, Pset, IDS verbs |

## [2]-[ANALYSIS]

- Owner: `IfcAnalysis` — the static surface dispatching the analysis verbs over `ifcopenshell.util` and `ifcopenshell.api`; `AnalysisKind` the closed `StrEnum` selecting the verb; `AnalysisResult` the typed receipt carrying the kind, the subject element set, and the result rows.
- Cases: `AnalysisKind` rows `QUANTITY` · `CLASH` · `SPACE_PROGRAM` · `PSET` · `IDS` — matched by `match`/`case` over the kind, each dispatching to the verb that owns it.
- Entry: `IfcAnalysis.run` takes an `ifcopenshell.file` and an `AnalysisKind` and returns a `RuntimeRail[AnalysisResult]`; the `selector.filter_elements` query language selects the element set, `util.element.get_psets` reads property/quantity sets, and `util.placement.get_local_placement` resolves geometry for clash.
- Auto: quantity takeoff folds `util.element.get_psets(element, qtos_only=True)` over the selected set; clash runs the OCCT bounding-overlap over `util.placement` transforms; space-program validation joins `IfcSpace` areas against a program table; IDS-style checking folds a rule table over the element psets.
- Receipt: each run contributes a `Receipt.emitted` row through `ReceiptContributor` and produces a geometry `GraduationReceipt` subject so the analysis evidence reaches the C# owner system through the one graduation rail.
- Packages: `ifcopenshell` (`util.element.get_psets`/`util.selector.filter_elements`/`util.placement.get_local_placement`/`util.unit`/`api`), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new analysis verb is one `AnalysisKind` row plus one dispatch arm; a new IDS rule is one row in the rule table; zero new surface.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy (that is projected in-process by GeometryGym); no durable store; no Rhino/GH mutation; a hand-rolled IFC parser, a parallel per-verb class family, and a stringly-typed verb dispatch are the deleted forms. This owner is `SPIKE` on the companion floor.

```python signature
from enum import StrEnum

import ifcopenshell
import ifcopenshell.util.element
import ifcopenshell.util.selector
from msgspec import Struct


class AnalysisKind(StrEnum):
    QUANTITY = "quantity"
    CLASH = "clash"
    SPACE_PROGRAM = "space-program"
    PSET = "pset"
    IDS = "ids"


class AnalysisResult(Struct, frozen=True):
    kind: AnalysisKind
    subjects: tuple[str, ...]
    rows: tuple[dict[str, str], ...]


class IfcAnalysis:
    @staticmethod
    def run(model: "ifcopenshell.file", kind: AnalysisKind, query: str) -> "RuntimeRail[AnalysisResult]":
        return boundary(f"ifc.{kind}", lambda: IfcAnalysis._dispatch(model, kind, query))

    @staticmethod
    def _dispatch(model: "ifcopenshell.file", kind: AnalysisKind, query: str) -> AnalysisResult:
        elements = ifcopenshell.util.selector.filter_elements(model, query)
        match kind:
            case AnalysisKind.QUANTITY:
                rows = tuple(ifcopenshell.util.element.get_psets(e, qtos_only=True) for e in elements)
            case AnalysisKind.PSET:
                rows = tuple(ifcopenshell.util.element.get_psets(e) for e in elements)
            case _:
                rows = ()
        return AnalysisResult(kind, tuple(e.GlobalId for e in elements), rows)
```

## [3]-[RESEARCH]

- [SELECTOR_GRAMMAR]: the `ifcopenshell.util.selector.filter_elements` query grammar and the `get_psets(qtos_only=True)` return shape are verified against `.api/api-ifcopenshell.md`; the clash bounding-overlap over `util.placement.get_local_placement` transforms and the IDS rule-table fold confirm on the cp312 companion interpreter once the floor/lock-scope decision admits the sub-3.13 environment.
