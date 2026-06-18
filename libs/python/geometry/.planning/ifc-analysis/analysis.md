# [PY_GEOMETRY_IFC_ANALYSIS]

IFC property/quantity/relationship analysis and standards-conformant validation — the AEC verbs the tessellation hop alone drops. `IfcAnalysis` runs quantity takeoff, Pset/schedule queries, IDS model-checking, clash detection, space-program validation, and BCF issue round-trip over `ifcopenshell.util`, `ifctester`, `ifcclash`, and the `bcf` library, emitting a geometry receipt that graduates through the compute `HandoffAxis` geometry case. The C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the analysis verbs and the buildingSMART validation output the managed projection does not produce.

## [1]-[INDEX]

[CLUSTERS]:
- `[2]-[ANALYSIS]`: the quantity, Pset, IDS, clash, space-program, and BCF analysis verbs under one `AnalysisKind`-discriminated owner.

## [2]-[ANALYSIS]

- Owner: `IfcAnalysis` — the static surface dispatching the analysis verbs over the IfcOpenShell ecosystem; `AnalysisKind` the closed `StrEnum` selecting the verb; `AnalysisResult` the typed receipt carrying the kind, the subject element set, and the BCF-serializable result rows.
- Cases: `AnalysisKind` rows `QUANTITY` (quantity takeoff over `util.element`) · `PSET` (property-set queries) · `IDS` (model-checking over `ifctester.ids`) · `CLASH` (clash sets over `ifcclash`) · `SPACE_PROGRAM` (`IfcSpace` area validation against a program table) · `BCF` (issue authoring/round-trip over the `bcf` library) — matched by `match`/`case`, each dispatching to the ecosystem tool that owns it.
- Entry: `IfcAnalysis.run` takes an `ifcopenshell.file`, an `AnalysisKind`, and a `query` whose meaning is fixed by the kind — an element selector for `QUANTITY`/`PSET`, an IDS spec path for `IDS`, a JSON program table keyed by space long-name for `SPACE_PROGRAM`, a BCF title for `BCF`, ignored for `CLASH` — and returns a `RuntimeRail[AnalysisResult]`. Each arm derives its own `subjects` from the verb's true subject set: `filter_elements` GlobalIds for the selecting arms, `IfcSpace` GlobalIds for `SPACE_PROGRAM`, spec names for `IDS`, clash a-side GlobalIds for `CLASH`, topic guids for `BCF` — so the subject field never carries a meaningless selector run for a non-selecting verb.
- Auto: quantity takeoff folds `util.element.get_psets(element, qtos_only=True)` over the selected set; IDS validation runs an `Ids` specification against the model and exports a BCF report; clash detection runs the `ifcclash` clash-set query and returns overlap pairs; space-program validation decodes the `query` program table and joins each `IfcSpace` net floor area from its QTO pset against the target, emitting a per-space target/actual/compliant row; BCF round-trip authors and reads conformant issue markup over the `bcf` library.
- Receipt: each run contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` and produces a geometry `GraduationReceipt` subject, so IDS results, clash sets, and BCF findings reach the C# owner system through the one graduation rail as standards-conformant output the toolchain consumes directly.
- Packages: `ifcopenshell` (`util.element.get_psets`/`util.selector.filter_elements`/`util.placement.get_local_placement`/`util.unit`/`api`), `ifctester` (`ids.Ids`/`ids.open`/`reporter`), `ifcclash` (`ifcclash.Clasher`/clash-set query), `bcf` (issue read/write), runtime (`RuntimeRail`/`ReceiptContributor`).
- Growth: a new analysis verb is one `AnalysisKind` row plus one dispatch arm; a new IDS specification is authored through `ifctester`, never a local rule fold; zero new surface.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy (projected in-process); no durable store; no Rhino/GH mutation; a hand-rolled IFC parser, a bespoke non-portable IDS rule fold, an OCCT bounding-overlap clash reimplementation, and a parallel per-verb class family are the deleted forms — IDS, clash, and BCF compose the provider tools end-to-end.

```python signature
import ifcopenshell
import ifcopenshell.util.element
import ifcopenshell.util.selector
import ifctester.ids
from bcf.v3.bcfxml import BcfXml
from ifcclash.ifcclash import Clasher
from enum import StrEnum
from typing import assert_never
from msgspec import Struct
from msgspec.json import decode

from rasm.runtime.receipts import ReceiptContributor
from rasm.runtime.faults import RuntimeRail, boundary


class AnalysisKind(StrEnum):
    QUANTITY = "quantity"
    PSET = "pset"
    IDS = "ids"
    CLASH = "clash"
    SPACE_PROGRAM = "space-program"
    BCF = "bcf"


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
        match kind:
            case AnalysisKind.QUANTITY | AnalysisKind.PSET:
                qtos = kind is AnalysisKind.QUANTITY
                elements = ifcopenshell.util.selector.filter_elements(model, query)
                rows = tuple(ifcopenshell.util.element.get_psets(e, qtos_only=qtos) for e in elements)
                return AnalysisResult(kind, tuple(e.GlobalId for e in elements), rows)
            case AnalysisKind.SPACE_PROGRAM:
                program = decode(query.encode(), type=dict[str, float])
                spaces = model.by_type("IfcSpace")
                rows = tuple(
                    {
                        "space": s.GlobalId,
                        "target": str(target),
                        "actual": str(area),
                        "compliant": str(area >= target),
                    }
                    for s in spaces
                    for target in (program.get(s.LongName or s.Name or "", 0.0),)
                    for area in (IfcAnalysis._net_area(s),)
                )
                return AnalysisResult(kind, tuple(s.GlobalId for s in spaces), rows)
            case AnalysisKind.IDS:
                spec = ifctester.ids.open(query)
                spec.validate(model)
                rows = tuple({"spec": s.name, "status": str(s.status)} for s in spec.specifications)
                return AnalysisResult(kind, tuple(r["spec"] for r in rows), rows)
            case AnalysisKind.CLASH:
                clasher = Clasher({"name": "ifc.clash"})
                clasher.clash_sets = [{"a": [{"file": model}], "b": [{"file": model}], "mode": "intersection"}]
                clasher.clash()
                rows = tuple({"a": c["a_global_id"], "b": c["b_global_id"]} for s in clasher.clash_sets for c in s["clashes"].values())
                return AnalysisResult(kind, tuple(r["a"] for r in rows), rows)
            case AnalysisKind.BCF:
                bcf = BcfXml.create_new(query)
                rows = tuple({"topic": t.title, "guid": t.guid} for t in bcf.topics.values())
                return AnalysisResult(kind, tuple(r["guid"] for r in rows), rows)
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _net_area(space: "ifcopenshell.entity_instance") -> float:
        qtos = ifcopenshell.util.element.get_psets(space, qtos_only=True)
        for pset in qtos.values():
            if "NetFloorArea" in pset:
                return float(pset["NetFloorArea"])
        return 0.0
```

## [3]-[RESEARCH]

- [CLASH_SET_SHAPE]: the `ifcclash.ifcclash.Clasher` constructor arity, the `clash_sets` element dict shape (`a`/`b` file selectors and `mode`), and the per-clash overlap-pair return (`a_global_id`/`b_global_id`) confirm against the branch `ifcclash` catalogue on the companion interpreter; the clash arm composes the verified `Clasher` class until the internal dict shape is catalogue-confirmed.
- [BCF_TOPIC_ROUNDTRIP]: the `bcf.v3.bcfxml.BcfXml` create/load entrypoints, the `topics` accessor, and the topic `title`/`guid` fields confirm against the branch `bcf` catalogue; the BCF arm composes the verified `BcfXml` class until the topic-collection shape is catalogue-confirmed.
- [IDS_SELECTOR_GRAMMAR]: the `ifctester.ids.open`/`Ids.validate`/`specifications[].status` surface, the `util.selector.filter_elements` query grammar, and the `get_psets(qtos_only=True)` return shape confirm against the branch `ifctester`/`ifcopenshell` catalogues.
- [SPACE_QUANTITY_KEY]: the `Qto_SpaceBaseQuantities` `NetFloorArea` quantity key the `_net_area` fold reads from the `get_psets(qtos_only=True)` return, and the `IfcSpace` `LongName`/`Name` program-table join key, confirm against the branch `ifcopenshell` catalogue.
