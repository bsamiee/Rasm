# [PY_GEOMETRY_IFC_ANALYSIS]

IFC property/quantity/relationship analysis and standards-conformant validation — the AEC verbs the tessellation hop alone drops: quantity takeoff, Pset queries, IDS model-checking, clash detection, space-program validation, and BCF issue authoring over `ifcopenshell.util`, `ifctester`, `ifcclash`, and the `bcf` library, every verb's provider output folded into one `AnalysisRow` row algebra. The C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the analysis verbs and the buildingSMART validation output the managed projection does not produce.

Every selecting verb admits its query through `IfcSelector` from `ifc/selector.md#SELECTOR`, so a malformed selector is a typed fault at the boundary, never a silent empty `filter_elements` match three arms deep. `run` threads the graduation `evidence_run` weave under `EvidenceScope.IFC_ANALYSIS` with `@beartype(conf=FAULT_CONF)` binding the contract fence. The OCC clash tree is this owner's one transient-native boundary and rides the runtime-pinned `guarded_sync(RetryClass.OCC_NATIVE)` row, never a bare `stamina.retry` mint; the durable `.bcfzip`/IDS-report archive WRITE stays the `python:data/spatial` boundary's. Evidence graduates under `GeometrySubject.BIM_COMPLIANCE` — the differentiated member the IDS/clash/BCF verdict class owns, distinct from the section-integral and lifecycle members their owners bind.

## [01]-[INDEX]

- [01]-[ANALYSIS]: six analysis verbs over one `AnalysisRow` algebra, the `IfcSelector` gate, the `OCC_NATIVE` retry row, and kind-specific graduation evidence under `BIM_COMPLIANCE`.

## [02]-[ANALYSIS]

- Owner: `IfcAnalysis` dispatches the verbs through one rail-returning `_dispatch` fold; `AnalysisRow` is the one row carrier whose case IS the shape — six verbs, one algebra, never a per-verb row dialect; the graduation subject rides the `ANALYSIS_SUBJECT` module constant, never a per-receipt `subject: str` field racing the discriminant.
- Entry: the `query` meaning is fixed by the kind through the `QUERY_SPLIT` table — pure selector, `a#b` side pair, or spec path/JSON table — one table-keyed split, never a partition-per-arm string ladder; the `CLASH`/`BCF` sides validate under one polymorphic batch parse aborting on the first malformed member, an empty query defaulting both sides to the whole-model mode. The BCF verb is the composition apex — it re-runs the clash leg and stacks the overlaps into `bcf` topics with viewpoints AND the OCC-rendered `get_viewpoint_snapshot` image bound per topic, never a same-string round-trip.
- Auto: IDS reads BOTH verdict depths off one validation pass — the per-spec entity ratio and the `Json(ids).report()` roll-up, with `percent_checks_pass` on the `Results` ROOT and the `total_applicable_*` totals on the `ResultsSpecification` rows, never conflated — excluding a `status is None` not-applicable spec, whose `0.0` row poisons the evidence mean; space-program validation excludes spaces absent from the program table — a `0.0`-ratio row reads an out-of-program space as total non-compliance — and scales net area through `calculate_unit_scale` squared before the comparison.
- Receipt: the `evidence` ledger is kind-specific, never a row count — IDS keys the per-entity AND per-check failing fractions, `SPACE_PROGRAM` the per-space failing fraction, `CLASH` the unresolved-cluster count, the takeoff/BCF verbs the empty-result fraction — so a model breaching its ceiling fails the carrier's `admitted` verdict rather than crossing clean.
- Packages: `ifcopenshell`, `ifctester`, `ifcclash`, and `bcf-client` per the fence imports; `IfcSelector` is the only `filter_elements` caller.
- Growth: a new verb is one `AnalysisKind` row, one `of_*` constructor plus one `facts` arm, one dispatch arm, one `QUERY_SPLIT` row, and one `evidence` key; a new selection axis is one `IfcSelector` grammar alternative, never a local query-parse fold here; IDS document AUTHORING (the `Ids(...)`/facet family/`to_xml` surface) is the named next verb when a consumer supplies a rule vocabulary — one row, never a second engine.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy; no durable store — the topic rows are the wire carry and the durable `.bcfzip` write is the data seam's; no Rhino/GH mutation; no raw `query` string threaded past admission into `filter_elements`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, NotRequired, TypedDict, assert_never

from beartype import beartype
from expression import Ok, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from msgspec.json import decode

from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_run
from rasm.geometry.ifc.selector import IfcSelector
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.receipts import Receipt
from rasm.runtime.resilience import RetryClass, guarded_sync

if TYPE_CHECKING:  # every runtime ifcopenshell use is a function-local boundary import, so the module loads clean
    import ifcopenshell

# --- [TYPES] ---------------------------------------------------------------------------


class AnalysisKind(StrEnum):
    QUANTITY = "quantity"
    PSET = "pset"
    IDS = "ids"
    CLASH = "clash"
    SPACE_PROGRAM = "space-program"
    BCF = "bcf"


class ClashRow(TypedDict):
    # the `ifcclash.ClashResult` shape plus the derived spatial-cluster index the grouper writes.
    a_global_id: str
    b_global_id: str
    distance: float
    p1: list[float]
    group: NotRequired[str]
    cluster: NotRequired[int]


@tagged_union(frozen=True)
class AnalysisRow:
    tag: Literal["quantity", "pset", "compliance", "clash", "topic"] = tag()
    quantity: tuple[str, dict[str, float]] = case()
    pset: tuple[str, dict[str, object]] = case()
    compliance: tuple[str, float, int] = case()  # subject, passing-ratio, failing-check count
    clash: tuple[str, str, float, int] = case()  # a-guid, b-guid, penetration, cluster index
    topic: tuple[str, str, str, bool] = case()  # guid, title, authored-from finding kind, snapshot bound

    @staticmethod
    def of_quantity(element: str, quantities: dict[str, float]) -> "AnalysisRow":
        return AnalysisRow(quantity=(element, quantities))

    @staticmethod
    def of_pset(element: str, properties: dict[str, object]) -> "AnalysisRow":
        return AnalysisRow(pset=(element, properties))

    @staticmethod
    def of_compliance(subject: str, passing: float, failing: int) -> "AnalysisRow":
        return AnalysisRow(compliance=(subject, passing, failing))

    @staticmethod
    def of_clash(a: str, b: str, penetration: float, cluster: int) -> "AnalysisRow":
        return AnalysisRow(clash=(a, b, penetration, cluster))

    @staticmethod
    def of_topic(guid: str, title: str, finding: str, snapshotted: bool) -> "AnalysisRow":
        return AnalysisRow(topic=(guid, title, finding, snapshotted))

    @property
    def facts(self) -> dict[str, object]:
        match self:
            case AnalysisRow(tag="quantity", quantity=(element, quantities)):
                return {"element": element, "quantities": quantities}
            case AnalysisRow(tag="pset", pset=(element, properties)):
                return {"element": element, "properties": properties}
            case AnalysisRow(tag="compliance", compliance=(subject, passing, failing)):
                return {"subject": subject, "passing": passing, "failing": failing}
            case AnalysisRow(tag="clash", clash=(a, b, penetration, cluster)):
                return {"a": a, "b": b, "penetration": penetration, "cluster": cluster}
            case AnalysisRow(tag="topic", topic=(guid, title, finding, snapshotted)):
                return {"topic": guid, "title": title, "finding": finding, "snapshotted": snapshotted}
            case unreachable:
                assert_never(unreachable)


# --- [CONSTANTS] -----------------------------------------------------------------------

ANALYSIS_SUBJECT: Final[GeometrySubject] = GeometrySubject.BIM_COMPLIANCE

QUERY_SPLIT: Final[Map[AnalysisKind, str]] = Map.of_seq([
    (AnalysisKind.QUANTITY, ""),
    (AnalysisKind.PSET, ""),
    (AnalysisKind.IDS, ""),
    (AnalysisKind.SPACE_PROGRAM, ""),
    (AnalysisKind.CLASH, "#"),
    (AnalysisKind.BCF, "#"),
])

# --- [MODELS] --------------------------------------------------------------------------


class AnalysisResult(Struct, frozen=True, gc=False):
    kind: AnalysisKind
    subjects: tuple[str, ...]
    rows: tuple[AnalysisRow, ...]
    checks_pass: float | None = None  # the IDS arm's Json-report root percent_checks_pass roll-up; None elsewhere

    def evidence(self) -> dict[str, float]:
        match self.kind:
            case AnalysisKind.IDS:
                ratios = tuple(r.compliance[1] for r in self.rows if r.tag == "compliance")
                entity_fail = 1.0 - sum(ratios) / len(ratios) if ratios else 0.0
                check_fail = 1.0 - self.checks_pass / 100.0 if self.checks_pass is not None else 0.0
                return {"non-compliant": entity_fail, "check-fail": check_fail}
            case AnalysisKind.SPACE_PROGRAM:
                # an empty `ratios` is "no compliance signal" keyed 0.0 — never a fabricated total-non-compliance mean.
                ratios = tuple(r.compliance[1] for r in self.rows if r.tag == "compliance")
                return {"non-compliant": 1.0 - sum(ratios) / len(ratios) if ratios else 0.0}
            case AnalysisKind.CLASH:
                clusters = {r.clash[3] for r in self.rows if r.tag == "clash"}
                return {"clash-clusters": float(len(clusters))}
            case AnalysisKind.QUANTITY | AnalysisKind.PSET | AnalysisKind.BCF:
                return {"empty": 0.0 if self.rows else 1.0}
            case unreachable:
                assert_never(unreachable)

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {f"{self.kind}.{i}.{k}": v for i, r in enumerate(self.rows) for k, v in r.facts.items()}
        yield Receipt.of("rasm.geometry.ifc.analysis", ("emitted", self.kind.value, facts | {"subjects": len(self.subjects)} | self.evidence()))

    def graduates(self, evidence_key: ContentKey, ceiling: dict[str, float]) -> GeometryHandoff:
        return GeometryHandoff.of(ANALYSIS_SUBJECT, evidence_key, self.evidence(), ceiling)


# --- [OPERATIONS] ----------------------------------------------------------------------


class IfcAnalysis:
    @staticmethod
    @beartype
    def run(model: "ifcopenshell.file", kind: AnalysisKind, query: str) -> "RuntimeRail[AnalysisResult]":
        # the weave's flatten absorbs the rail-returning `_dispatch`, so a selector parse fault meets the converted
        # provider fault on one carrier; graduation stays the caller's own step on the returned receipt.
        return evidence_run(EvidenceScope.IFC_ANALYSIS, f"run.{kind}", lambda: IfcAnalysis._dispatch(model, kind, query))

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _dispatch(model: "ifcopenshell.file", kind: AnalysisKind, query: str) -> "RuntimeRail[AnalysisResult]":
        match kind:
            case AnalysisKind.QUANTITY | AnalysisKind.PSET:
                quantities = kind is AnalysisKind.QUANTITY
                return IfcSelector.filter(model, query).map(
                    lambda elements: AnalysisResult(
                        kind, tuple(e.GlobalId for e in elements), tuple(IfcAnalysis._takeoff(e, quantities) for e in elements)
                    )
                )
            case AnalysisKind.SPACE_PROGRAM:
                return Ok(IfcAnalysis._space(model, query))
            case AnalysisKind.IDS:
                return Ok(IfcAnalysis._validate(model, query))
            case AnalysisKind.CLASH:
                return IfcAnalysis._clash_sides(query).bind(lambda sides: IfcAnalysis._clash(model, sides))
            case AnalysisKind.BCF:
                return IfcAnalysis._clash_sides(query).bind(lambda sides: IfcAnalysis._author(model, sides))
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _space(model: "ifcopenshell.file", query: str) -> AnalysisResult:
        # The program-table decode runs under the weave fence, so a malformed JSON table is a
        # BoundaryFault classified at the seam rather than an inline try/except in domain logic.
        program = decode(query.encode(), type=dict[str, float])
        scale = IfcAnalysis._unit_scale(model)
        spaces = model.by_type("IfcSpace")
        # a space absent from the program table carries no program signal and is EXCLUDED from grading.
        graded = tuple(
            (s, program[key], IfcAnalysis._net_area(s) * scale)
            for s in spaces
            for key in (s.LongName or s.Name or "",)
            if program.get(key, 0.0) > 0.0
        )
        rows = tuple(AnalysisRow.of_compliance(s.GlobalId, area / target, 0 if area >= target else 1) for s, target, area in graded)
        return AnalysisResult(AnalysisKind.SPACE_PROGRAM, tuple(s.GlobalId for s, _, _ in graded), rows)

    @staticmethod
    def _takeoff(element: "ifcopenshell.entity_instance", quantities: bool) -> AnalysisRow:
        import ifcopenshell.util.element  # noqa: PLC0415

        psets = ifcopenshell.util.element.get_psets(element, qtos_only=quantities)
        merged = {f"{name}.{key}": value for name, body in psets.items() for key, value in body.items()}
        if quantities:
            return AnalysisRow.of_quantity(element.GlobalId, {k: float(v) for k, v in merged.items() if isinstance(v, int | float)})
        return AnalysisRow.of_pset(element.GlobalId, merged)

    @staticmethod
    def _validate(model: "ifcopenshell.file", spec_path: str) -> AnalysisResult:
        import ifctester.ids  # noqa: PLC0415
        import ifctester.reporter  # noqa: PLC0415

        document = ifctester.ids.open(spec_path)
        document.validate(model)
        rows = tuple(
            AnalysisRow.of_compliance(
                spec.name, len(spec.passed_entities) / max(len(spec.passed_entities) + len(spec.failed_entities), 1), len(spec.failed_entities)
            )
            for spec in document.specifications
            if spec.status is not None
        )
        applicable = tuple(spec.name for spec in document.specifications if spec.status is not None)
        results = ifctester.reporter.Json(document).report()
        return AnalysisResult(AnalysisKind.IDS, applicable, rows, checks_pass=float(results["percent_checks_pass"]))

    @staticmethod
    def _clash_sides(query: str) -> "RuntimeRail[tuple[str, str]]":
        if not query:
            return Ok(("", ""))
        a_query, _, b_query = query.partition(QUERY_SPLIT[AnalysisKind.CLASH])
        return IfcSelector.parse((a_query, b_query or a_query)).map(lambda sides: (sides[0].filter_string, sides[1].filter_string))

    @staticmethod
    def _clash(model: "ifcopenshell.file", sides: tuple[str, str]) -> "RuntimeRail[AnalysisResult]":
        return IfcAnalysis._run_clash(model, sides).map(
            lambda run: AnalysisResult(
                AnalysisKind.CLASH,
                tuple(c["a_global_id"] for c in run[1]),
                tuple(AnalysisRow.of_clash(c["a_global_id"], c["b_global_id"], c["distance"], c.get("cluster", 0)) for c in run[1]),
            )
        )

    @staticmethod
    def _author(model: "ifcopenshell.file", sides: tuple[str, str]) -> "RuntimeRail[AnalysisResult]":
        from bcf.v3.bcfxml import BcfXml  # noqa: PLC0415

        # this leg authors the in-memory archive and reads its topic GUIDs back; the durable save is the data seam's.
        def compose(run: "tuple[Clasher, tuple[ClashRow, ...], ClashSet]") -> AnalysisResult:
            clasher, clashes, clash_set = run
            document = BcfXml.create_new("rasm.ifc.analysis")
            rows = tuple(IfcAnalysis._topic(document, clasher, clash_set, c) for c in clashes)
            return AnalysisResult(AnalysisKind.BCF, tuple(r.topic[0] for r in rows), rows)

        return IfcAnalysis._run_clash(model, sides).map(compose)

    @staticmethod
    def _topic(document: "bcf.v3.bcfxml.BcfXml", clasher: "Clasher", clash_set: "ClashSet", collision: ClashRow) -> AnalysisRow:
        handler = document.add_topic(
            f"Clash {collision['a_global_id']} × {collision['b_global_id']}", f"penetration {collision['distance']:.4f}", "rasm", topic_type="Clash"
        )
        viewpoint = handler.add_viewpoint_from_point_and_guids(collision["p1"], collision["a_global_id"], collision["b_global_id"])
        snapshot = clasher.get_viewpoint_snapshot(clash_set, viewpoint)
        return AnalysisRow.of_topic(handler.guid, handler.topic.title, "clash", snapshot is not None)

    @staticmethod
    def _run_clash(model: "ifcopenshell.file", sides: tuple[str, str]) -> "RuntimeRail[tuple[Clasher, tuple[ClashRow, ...], ClashSet]]":
        from ifcclash.ifcclash import Clasher, ClashSettings, ClashSet, ClashSource  # noqa: PLC0415

        def source(selector: str) -> ClashSource:
            # `file` is a required ClashSource key; the pre-loaded `ifc` model makes the empty path inert. The 'e'
            # expression mode consumes the validated `filter_string`, the 'a'-all mode the whole-model default.
            base: ClashSource = {"file": "", "ifc": model}
            return {**base, "selector": selector, "mode": "e"} if selector else {**base, "mode": "a"}

        # the returned rail carries the live Clasher and the clash set so the BCF arm binds snapshots.
        def solve() -> tuple[Clasher, tuple[ClashRow, ...], ClashSet]:
            clasher = Clasher(ClashSettings())
            clash_set: ClashSet = {"name": "ifc.clash", "a": [source(sides[0])], "b": [source(sides[1])], "mode": "intersection", "tolerance": 0.001}
            clasher.clash_sets = [clash_set]
            clasher.clash()
            clasher.smart_group_clashes(clasher.clash_sets, max_clustering_distance=1.0)
            clashes: tuple[ClashRow, ...] = tuple(clash_set.get("clashes", {}).values())
            clusters = {key: index for index, key in enumerate(clash_set.get("clash_groups", {}))}
            return clasher, tuple({**c, "cluster": clusters.get(c.get("group", ""), 0)} for c in clashes), clash_set

        return guarded_sync(RetryClass.OCC_NATIVE, solve, subject="ifc.clash.tree")

    @staticmethod
    def _unit_scale(model: "ifcopenshell.file") -> float:
        import ifcopenshell.util.unit  # noqa: PLC0415

        return float(ifcopenshell.util.unit.calculate_unit_scale(model)) ** 2

    @staticmethod
    def _net_area(space: "ifcopenshell.entity_instance") -> float:
        import ifcopenshell.util.element  # noqa: PLC0415

        qtos = ifcopenshell.util.element.get_psets(space, qtos_only=True)
        return next((float(pset["NetFloorArea"]) for pset in qtos.values() if "NetFloorArea" in pset), 0.0)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
