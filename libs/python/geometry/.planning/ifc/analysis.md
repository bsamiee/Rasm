# [PY_GEOMETRY_IFC_ANALYSIS]

IFC property/quantity/relationship analysis and standards-conformant validation — the AEC verbs the tessellation hop alone drops. `IfcAnalysis` runs quantity takeoff, Pset queries, IDS model-checking, clash detection, space-program validation, and BCF issue authoring over `ifcopenshell.util`, `ifctester`, `ifcclash`, and the `bcf` library, folding every verb's provider output into one `AnalysisRow` tagged-union row algebra (typed quantity/pset/compliance/clash/topic cases, never a stringified `dict[str, str]` bag), contributing an emitted-phase `Receipt`, and emitting a geometry `GraduationReceipt` that crosses the compute `HandoffAxis` geometry case under the one residual-ceiling admission keyed by the kind-specific compliance evidence. The BCF verb is the composition apex — it re-runs the clash leg and stacks `ifcclash` overlaps into `bcf` topics with viewpoints, rather than a same-string round-trip. The selecting verbs never touch a raw query string: the free-form selector is validated once at admission through `IfcSelector.parse` from `geometry:ifc/selector.md#SELECTOR`, so a malformed selector is a typed `Error(BoundaryFault)` on the rail at the boundary rather than a silent empty `filter_elements` match three arms deep, and the validated `SelectorQuery` re-serializes to the exact grammar `ifcopenshell` consumes. The C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the analysis verbs and the buildingSMART validation output the managed projection does not produce.

## [01]-[INDEX]

- [01]-[ANALYSIS]: the quantity, Pset, IDS, clash, space-program, and BCF analysis verbs under one `AnalysisKind`-discriminated owner folding into one `AnalysisRow` tagged-union row algebra, every selecting arm fed by the `IfcSelector` validated entry gate, every run contributing a typed receipt and graduating on kind-specific compliance evidence.

## [02]-[ANALYSIS]

- Owner: `IfcAnalysis` — the static surface dispatching the analysis verbs over the IfcOpenShell ecosystem; `AnalysisKind` the closed `StrEnum` selecting the verb; `AnalysisRow` the `@tagged_union` of per-verb result rows — one discriminated row algebra carrying typed payloads (`quantity` a GlobalId plus a `dict[str, float]` quantity map, `pset` a GlobalId plus a `dict[str, object]` property map, `compliance` a subject plus a `float` ratio and an `int` failure count shared by the IDS and space-program verdicts, `clash` an a/b GlobalId pair plus a `float` penetration distance and a cluster index, `topic` a guid plus title and an authored-from finding kind) — so the six verbs share one row carrier whose case IS the shape, never six parallel `dict[str, str]` bags that stringify floats and bools; `AnalysisResult` the typed receipt carrying the kind, the `GeometrySubject` graduation case, the subject element set, and the `tuple[AnalysisRow, ...]` rows, owning both the emitted-phase `contribute` and the kind-specific `evidence` ledger the graduation leg folds.
- Cases: `AnalysisKind` rows `QUANTITY` (quantity takeoff over `util.element`) · `PSET` (property-set queries) · `IDS` (model-checking over `ifctester.ids`) · `CLASH` (clash sets over `ifcclash`) · `SPACE_PROGRAM` (`IfcSpace` area validation against a program table) · `BCF` (issue authoring over the `bcf` library that consumes the clash and IDS findings as topics) — matched by `match`/`assert_never`, each dispatching to the ecosystem tool that owns it and folding its provider output into the shared `AnalysisRow` case the verb's evidence reads from, never a per-verb row dialect.
- Selector gate: `QUANTITY` and `PSET` are the selecting verbs; their `query` is never threaded raw into `filter_elements`. Each arm opens with `IfcSelector.filter(model, query)` — `IfcSelector.parse(query).map(filter_elements)` — so the `lark` grammar admits the closed selection vocabulary once and lifts an `UnexpectedInput` parse failure into the rail at the boundary, and the validated `SelectorQuery.filter_string` re-serializes the selection to the `ifcopenshell` grammar; the selector is the only selection engine, never a parallel filter. The `CLASH` arm derives its two `ClashSource.selector` strings the same validated way when the `query` carries an `a#b` side pair, defaulting both sides to the `'a'`-all mode for a whole-model intersection when the `query` is empty.
- Entry: `IfcAnalysis.run` takes an `ifcopenshell.file`, an `AnalysisKind`, and a `query` whose meaning is fixed by the kind — a validated element selector for `QUANTITY`/`PSET`, an IDS spec path for `IDS`, a JSON program table keyed by space long-name for `SPACE_PROGRAM`, a BCF project title plus an `a#b` clash selector pair the BCF arm re-runs for `BCF`, an optional `a#b` selector pair for `CLASH` — and returns a `RuntimeRail[AnalysisResult]` through one `boundary(f"ifc.{kind}", ...)` admission whose thunk yields the per-arm rail; the boundary flattens the nested rail through `expression.Result.bind(identity)` so a provider exception converts to a `BoundaryFault` exactly once at the seam while a selector parse fault arrives already typed on the rail, the two fault sources meeting on one carrier. The dispatch is a rail-returning fold: each arm yields `RuntimeRail[AnalysisResult]`, the selecting arms by `IfcSelector.filter(...).map(...)`, the non-selecting arms lifted through `Ok`, so the whole verb composes monadically rather than a bare value wrapped once. Each arm derives its own `subjects` from the verb's true subject set: validated `filter_elements` GlobalIds for the selecting arms, `IfcSpace` GlobalIds for `SPACE_PROGRAM`, spec names for `IDS`, clash a-side GlobalIds for `CLASH`, authored topic guids for `BCF` — so the subject field never carries a meaningless selector run for a non-selecting verb.
- Auto: quantity takeoff folds `util.element.get_psets(element, qtos_only=True)` over the validated selected set into one merged `quantity` map per element, keeping the numeric quantities as floats; pset queries fold the full `get_psets` map into the `pset` rows preserving native value types; IDS validation runs an `Ids` specification against the model and reads each `Specification.status` confirmed boolean verdict, folding each spec into a `compliance` row whose ratio is the `1.0`/`0.0` pass verdict and whose failure count is `0` or `1` — the binary spec verdict is the confirmed accessor, the per-requirement passing fraction a residual the live `Specification` member-set sharpens, never a fabricated `requirements` walk in the fold body; clash detection runs the `ifcclash` clash-set query over the validated `ClashSource.selector` sides at one configurable `ClashSet` mode/tolerance, then `smart_group_clashes` clusters the overlaps so each `clash` row carries its `ClashResult` GlobalId pair, penetration `distance`, and spatial-cluster index in one pass; space-program validation decodes the `query` program table, scales each `IfcSpace` net floor area to project units through `util.unit.calculate_unit_scale` before the comparison, and folds a `compliance` row per space whose ratio is `actual/target`; the BCF arm re-runs the clash leg once and composes each overlap INTO a topic — `BcfXml.create_new` opens the project, `add_topic` mints one topic per clash, and `add_viewpoint_from_point_and_guids` binds each topic to the `ClashResult.p1` overlap point and the offending `a_global_id`/`b_global_id` GUIDs, so the BCF output is the issue-authoring leg that stacks `ifcclash` + `bcf` into one in-memory archive rather than a same-string round-trip.
- Receipt: each run contributes an emitted-phase `Receipt.of("emitted", "rasm.geometry.ifc.analysis", subject, facts)` row through `ReceiptContributor.contribute` carrying the kind tag and the kind-specific facts (selected-element count for the takeoff verbs, passing/failing spec counts for IDS, clash-cluster count for CLASH, compliant-space fraction for SPACE_PROGRAM, authored-topic count for BCF), then graduates the `GeometrySubject` case `numerical-primitive` — the IFC-analysis evidence the C# owner system consumes, the same `GeometrySubject` literal the `geometry:ifc/structural.md#STRUCTURAL` section-integral owner crosses on — annotated as the canonical union from `python:compute/graduation/handoff.md#GRADUATION` (`GraduationReceipt.graduates(source_package, HandoffAxis(geometry=subject), evidence_key, measured, ceiling)`), never a bare `str`, so an unlisted literal fails at the type boundary. The residual ledger is the kind-specific `AnalysisResult.evidence` fold rather than a meaningless row count: the IDS/space-program verdicts key the failing fraction (`1 - passing/total`), CLASH keys the unresolved-cluster count, and the takeoff verbs key the empty-result fraction, so a model whose compliance falls below its ceiling is an `Error(BoundaryFault)` on the rail rather than a graduated handoff, and IDS results, clash sets, and BCF findings reach the C# owner system through the one graduation rail as standards-conformant output the toolchain consumes directly.
- Packages: `ifcopenshell` (`util.element.get_psets`/`util.unit.calculate_unit_scale`), `ifctester` (`ids.open`/`Ids.validate`/`Ids.specifications`/`Specification.name`/`Specification.status`), `ifcclash` (`ifcclash.Clasher`/`ifcclash.ClashSettings`/`ClashSet`/`ClashSource.selector`/`ClashSource.mode`/`Clasher.clash`/`Clasher.smart_group_clashes`/`ClashResult.a_global_id`/`b_global_id`/`p1`/`distance`), `bcf-client` (import `bcf`; `bcf.v3.bcfxml.BcfXml.create_new`/`add_topic`/`get_topics`/`TopicHandler.add_viewpoint_from_point_and_guids`/`TopicHandler.guid`), `geometry:ifc/selector.md#SELECTOR` (`IfcSelector.filter`/`IfcSelector.parse` — the validated selection engine, the only `filter_elements` caller), `python:compute/graduation/handoff.md#GRADUATION` (`GeometrySubject`/`HandoffAxis`/`GraduationReceipt`), runtime (`RuntimeRail`/`boundary`/`ContentKey`/`Receipt`/`ReceiptContributor`).
- Growth: a new analysis verb is one `AnalysisKind` row, one `AnalysisRow` case, one dispatch arm, and one `evidence` ledger key; a new selection axis is one `IfcSelector` grammar alternative, never a local query-parse fold here; a new IDS specification is authored through `ifctester`, never a local rule fold; zero new surface.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy (projected in-process); no durable store; no Rhino/GH mutation; no raw `query` string threaded past admission into `filter_elements` (the deleted stringly-typed passthrough — the selecting arms enter through `IfcSelector` and the raw query never reaches `ifcopenshell.util.selector` unvalidated); a hand-rolled IFC parser, a second selection engine where `IfcSelector` re-serializes the validated query to the `ifcopenshell` grammar, a bespoke non-portable IDS rule fold, an OCCT bounding-overlap clash reimplementation, a stringified `dict[str, str]` row that erases floats and bools where `AnalysisRow` discriminates the typed payload, a same-string BCF round-trip where the arm composes clash/IDS findings into topics, a raw-unit space comparison where `util.unit.calculate_unit_scale` normalizes, an inlined residual-vs-ceiling comparison where `GraduationReceipt.graduates` owns the admission, and a parallel per-verb class family are the deleted forms — the selector grammar, IDS, clash, BCF, the unit scale, and the graduation rail compose the provider tools end-to-end.

```python signature
from enum import StrEnum
from typing import Literal, assert_never

import ifcopenshell
from expression import Ok, case, tag, tagged_union
from msgspec import Struct
from msgspec.json import decode

from rasm.compute.graduation.handoff import GeometrySubject, GraduationReceipt, HandoffAxis
from rasm.geometry.ifc.selector import IfcSelector
from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, ReceiptContributor

# --- [TYPES] ---------------------------------------------------------------------------


class AnalysisKind(StrEnum):
    QUANTITY = "quantity"
    PSET = "pset"
    IDS = "ids"
    CLASH = "clash"
    SPACE_PROGRAM = "space-program"
    BCF = "bcf"


@tagged_union(frozen=True)
class AnalysisRow:
    tag: Literal["quantity", "pset", "compliance", "clash", "topic"] = tag()
    quantity: tuple[str, dict[str, float]] = case()
    pset: tuple[str, dict[str, object]] = case()
    compliance: tuple[str, float, int] = case()
    clash: tuple[str, str, float, int] = case()
    topic: tuple[str, str, str] = case()


# --- [CONSTANTS] -----------------------------------------------------------------------

# The IFC-analysis evidence crosses the geometry graduation case as a numerical primitive,
# the same GeometrySubject literal the section-integral owner crosses on.
ANALYSIS_SUBJECT: GeometrySubject = "numerical-primitive"

# --- [MODELS] --------------------------------------------------------------------------


class AnalysisResult(Struct, frozen=True):
    kind: AnalysisKind
    subject: GeometrySubject
    subjects: tuple[str, ...]
    rows: tuple[AnalysisRow, ...]

    def evidence(self) -> dict[str, float]:
        match self.kind:
            case AnalysisKind.IDS | AnalysisKind.SPACE_PROGRAM:
                ratios = tuple(r.compliance[1] for r in self.rows if r.tag == "compliance")
                passing = sum(1.0 for ratio in ratios if ratio >= 1.0)
                return {"non-compliant": 1.0 - passing / max(len(ratios), 1)}
            case AnalysisKind.CLASH:
                clusters = {r.clash[3] for r in self.rows if r.tag == "clash"}
                return {"clash-clusters": float(len(clusters))}
            case AnalysisKind.QUANTITY | AnalysisKind.PSET | AnalysisKind.BCF:
                return {"empty": 0.0 if self.rows else 1.0}
            case unreachable:
                assert_never(unreachable)

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted",
            "rasm.geometry.ifc.analysis",
            self.kind.value,
            {"subjects": str(len(self.subjects)), "rows": str(len(self.rows))}
            | {k: repr(v) for k, v in self.evidence().items()},
        )

    def graduates(self, evidence_key: ContentKey, ceiling: dict[str, float]) -> "RuntimeRail[GraduationReceipt]":
        return GraduationReceipt.graduates(
            "rasm.geometry.ifc.analysis",
            HandoffAxis(geometry=self.subject),
            evidence_key,
            self.evidence(),
            ceiling,
        )


# --- [OPERATIONS] ----------------------------------------------------------------------


class IfcAnalysis:
    @staticmethod
    def run(model: "ifcopenshell.file", kind: AnalysisKind, query: str) -> "RuntimeRail[AnalysisResult]":
        return boundary(f"ifc.{kind}", lambda: IfcAnalysis._dispatch(model, kind, query)).bind(lambda rail: rail)

    @staticmethod
    def _dispatch(model: "ifcopenshell.file", kind: AnalysisKind, query: str) -> "RuntimeRail[AnalysisResult]":
        match kind:
            case AnalysisKind.QUANTITY | AnalysisKind.PSET:
                quantities = kind is AnalysisKind.QUANTITY
                return IfcSelector.filter(model, query).map(
                    lambda elements: IfcAnalysis._result(
                        kind,
                        tuple(e.GlobalId for e in elements),
                        tuple(IfcAnalysis._takeoff(e, quantities) for e in elements),
                    )
                )
            case AnalysisKind.SPACE_PROGRAM:
                program = decode(query.encode(), type=dict[str, float])
                scale = IfcAnalysis._unit_scale(model)
                spaces = model.by_type("IfcSpace")
                rows = tuple(
                    AnalysisRow(compliance=(s.GlobalId, area / target if target else 0.0, 0 if area >= target else 1))
                    for s in spaces
                    for target in (program.get(s.LongName or s.Name or "", 0.0),)
                    for area in (IfcAnalysis._net_area(s) * scale,)
                )
                return Ok(IfcAnalysis._result(kind, tuple(s.GlobalId for s in spaces), rows))
            case AnalysisKind.IDS:
                return Ok(IfcAnalysis._validate(model, query))
            case AnalysisKind.CLASH:
                return IfcAnalysis._clash_sides(query).map(lambda sides: IfcAnalysis._clash(model, sides))
            case AnalysisKind.BCF:
                return IfcAnalysis._clash_sides(query).map(lambda sides: IfcAnalysis._author(model, sides))
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _takeoff(element: "ifcopenshell.entity_instance", quantities: bool) -> AnalysisRow:
        import ifcopenshell.util.element  # noqa: PLC0415

        psets = ifcopenshell.util.element.get_psets(element, qtos_only=quantities)
        merged = {f"{name}.{key}": value for name, body in psets.items() for key, value in body.items()}
        if quantities:
            return AnalysisRow(quantity=(element.GlobalId, {k: float(v) for k, v in merged.items() if isinstance(v, int | float)}))
        return AnalysisRow(pset=(element.GlobalId, merged))

    @staticmethod
    def _validate(model: "ifcopenshell.file", spec_path: str) -> AnalysisResult:
        import ifctester.ids  # noqa: PLC0415

        document = ifctester.ids.open(spec_path)
        document.validate(model)
        rows = tuple(
            AnalysisRow(compliance=(spec.name, 1.0 if spec.status else 0.0, 0 if spec.status else 1))
            for spec in document.specifications
        )
        return IfcAnalysis._result(AnalysisKind.IDS, tuple(spec.name for spec in document.specifications), rows)

    @staticmethod
    def _clash_sides(query: str) -> "RuntimeRail[tuple[str, str]]":
        a_query, _, b_query = query.partition("#")
        if not a_query:
            return Ok(("", ""))
        return IfcSelector.parse(a_query).bind(
            lambda a: IfcSelector.parse(b_query or a_query).map(lambda b: (a.filter_string, b.filter_string))
        )

    @staticmethod
    def _clash(model: "ifcopenshell.file", sides: tuple[str, str]) -> AnalysisResult:
        results = IfcAnalysis._run_clash(model, sides)
        rows = tuple(
            AnalysisRow(clash=(c["a_global_id"], c["b_global_id"], float(c["distance"]), c.get("cluster", 0)))
            for c in results
        )
        return IfcAnalysis._result(AnalysisKind.CLASH, tuple(r.clash[0] for r in rows), rows)

    @staticmethod
    def _author(model: "ifcopenshell.file", sides: tuple[str, str]) -> AnalysisResult:
        from bcf.v3.bcfxml import BcfXml  # noqa: PLC0415

        document = BcfXml.create_new("rasm.ifc.analysis")
        rows: list[AnalysisRow] = []
        for collision in IfcAnalysis._run_clash(model, sides):
            handler = document.add_topic(
                f"Clash {collision['a_global_id']} × {collision['b_global_id']}",
                f"penetration {collision['distance']:.4f}",
                "rasm",
                topic_type="Clash",
            )
            handler.add_viewpoint_from_point_and_guids(
                collision["p1"], collision["a_global_id"], collision["b_global_id"]
            )
            rows.append(AnalysisRow(topic=(handler.guid, handler.topic.title, "clash")))
        return IfcAnalysis._result(AnalysisKind.BCF, tuple(r.topic[0] for r in rows), tuple(rows))

    @staticmethod
    def _run_clash(model: "ifcopenshell.file", sides: tuple[str, str]) -> tuple[dict[str, object], ...]:
        from ifcclash.ifcclash import Clasher, ClashSettings  # noqa: PLC0415

        def source(selector: str) -> dict[str, object]:
            return {"ifc": model, "selector": selector, "mode": "e"} if selector else {"ifc": model, "mode": "a"}

        clasher = Clasher(ClashSettings())
        clash_set: dict[str, object] = {
            "name": "ifc.clash",
            "a": [source(sides[0])],
            "b": [source(sides[1])],
            "mode": "intersection",
            "tolerance": 0.001,
        }
        clasher.clash_sets = [clash_set]
        clasher.clash()
        clasher.smart_group_clashes(clasher.clash_sets, max_clustering_distance=1.0)
        clashes = tuple(clash_set.get("clashes", {}).values())
        clusters = {key: index for index, key in enumerate(clash_set.get("clash_groups", {}))}
        return tuple({**c, "cluster": clusters.get(c.get("group", ""), 0)} for c in clashes)

    @staticmethod
    def _result(kind: AnalysisKind, subjects: tuple[str, ...], rows: tuple[AnalysisRow, ...]) -> AnalysisResult:
        return AnalysisResult(kind, ANALYSIS_SUBJECT, subjects, rows)

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

- [CLASH_SET_INTERNAL]: the branch `ifcclash` catalogue confirms `Clasher(ClashSettings())`, the `ClashSet` TypedDict (`name`/`a`/`b`/`mode`/`tolerance`/`clearance`/`clashes`), the `ClashSource` TypedDict (`ifc`/`mode` `'a'|'e'|'i'`/`selector`), the in-place `clashes: dict[str, ClashResult]` accumulation, the `ClashResult.a_global_id`/`b_global_id`/`p1`/`distance` fields, and `Clasher.smart_group_clashes(clash_sets, max_clustering_distance)` (`ifcclash.md#45`/`#100`/`#105`). The `_run_clash` fold writes one `ClashSet` at the `'intersection'` mode plus a `tolerance` policy, calls `clash()` then `smart_group_clashes` in one pass, and reads back the `clashes` map and the spatial-cluster grouping; the exact spelling the grouper writes the per-clash cluster handle under — whether `clash_set["clash_groups"]` keyed by group id with each `ClashResult` carrying a `group` back-reference, or a flat re-keyed `clashes` map — is the remaining internal-shape detail the live run confirms before the `cluster` index binds (`_run_clash` defaults the index to `0` until the grouper handle is confirmed, never dropping the row).
- [BCF_ISSUE_AUTHORING]: the branch `bcf-client` catalogue confirms `bcf.v3.bcfxml.BcfXml.create_new(project_name)`, `BcfXml.add_topic(title, description, author, topic_type='', topic_status='') -> TopicHandler` (`bcf-client.md#87`), `TopicHandler.add_viewpoint_from_point_and_guids(position, *guids) -> VisualizationInfoHandler` (`#103`), and `TopicHandler.topic`/`TopicHandler.guid` (`#104`); the `TopicHandler.topic.title` markup field spelling (the `v3.model.markup.Topic.title` attribute) is the remaining field-name detail the live run confirms. The `BCF` arm is the issue-authoring leg, not a same-string round-trip: it re-runs the clash leg through `_run_clash`, mints one topic per overlap with `add_topic(..., topic_type="Clash")`, and binds each topic to the `ClashResult.p1` overlap point and the offending `a_global_id`/`b_global_id` through `add_viewpoint_from_point_and_guids`, so `ifcclash` + `bcf` stack into one archive.
- [IDS_STRUCTURED_VERDICT]: the branch `ifctester` catalogue confirms `ids.open(filepath, validate=False) -> Ids`, `Ids.validate(ifc_file)`, `Ids.specifications`, `Specification.name`, and `Specification.status` (`ifctester.md#74`/`#86`/`#20`). The `_validate` fold reads the confirmed `Specification.status` boolean as the per-spec verdict and folds it into the `1.0`/`0.0` `compliance` row; the per-requirement passing fraction that would sharpen the ratio — whether the result set spells `Specification.requirements` or `Specification.requirement_facets`, and whether each requirement carries a `status` boolean or a `FacetFailure` count (`ifctester.md#51`) — is an unconfirmed member-spelling residual that does NOT appear in the fold body (no fabricated `requirements` walk); the binary verdict graduates today and the fraction is a row-sharpen the live `Specification` member-set unlocks. The `reporter.Bcf` IDS-side archive is dropped from this arm: the IDS evidence is the `compliance` rows the rail ships in-memory, and `report()` collects rather than writes, so a dangling `reporter.Bcf(document).report()` was a dead side-effect — BCF authoring is the `bcf-client` `_author` leg alone, not an `ifctester` reporter file. The selecting arms no longer call `util.selector.filter_elements` directly — the validated `IfcSelector.filter`/`IfcSelector.parse` gate from `geometry:ifc/selector.md#SELECTOR` is the sole `filter_elements` caller, so a malformed selector is an `UnexpectedInput`-derived `Error(BoundaryFault)` on the rail at admission. The `CLASH`/`BCF` arms thread the same gate per `ClashSource.selector` side, the `'e'` IfcClass-expression mode against the validated query and the `'a'`-all mode for the empty whole-model default (`ifcclash.md#73`/`#98`).
- [GRADUATION_SUBJECT_TYPE]: the `AnalysisResult.subject` field is the canonical `GeometrySubject` literal union from `python:compute/graduation/handoff.md#GRADUATION` (`numerical-primitive`, the IFC-analysis evidence case the C# owner consumes, matching the `geometry:ifc/structural.md#STRUCTURAL` section-integral subject), never a bare `str`, so an unlisted literal fails the `HandoffAxis(geometry=...)` type boundary; `AnalysisResult.graduates` folds the kind-specific `evidence` ledger (the failing-compliance fraction for IDS/space-program, the unresolved-cluster count for CLASH, the empty-result fraction for the takeoff/BCF verbs) through the one `GraduationReceipt.graduates(source_package, HandoffAxis(geometry=subject), evidence_key, measured, ceiling)` admission (handoff.md `#15`/`#34`-`#52`), never a row count or an inlined comparison. `AnalysisResult.contribute` implements the runtime `ReceiptContributor` Protocol (`receipts.md#47`/`#52`) returning one emitted-phase `Receipt.of` row, mirroring `GraduationReceipt.contribute`.
- [SPACE_QUANTITY_KEY]: the `Qto_SpaceBaseQuantities` `NetFloorArea` quantity key the `_net_area` fold reads from the `get_psets(qtos_only=True)` return, the `IfcSpace` `LongName`/`Name` program-table join key, and `util.unit.calculate_unit_scale(model)` (`ifcopenshell.md#118`) — the project-to-SI length scale the `_unit_scale` fold squares for the area comparison so the program table joins in consistent units — confirm against the branch `ifcopenshell` catalogue.
