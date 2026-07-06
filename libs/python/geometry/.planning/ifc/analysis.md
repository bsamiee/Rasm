# [PY_GEOMETRY_IFC_ANALYSIS]

IFC property/quantity/relationship analysis and standards-conformant validation — the AEC verbs the tessellation hop alone drops. `IfcAnalysis` runs quantity takeoff, Pset queries, IDS model-checking, clash detection, space-program validation, and BCF issue authoring over `ifcopenshell.util`, `ifctester`, `ifcclash`, and the `bcf` library, folding every verb's provider output into one `AnalysisRow` `@tagged_union` row algebra whose `of_*` smart constructors mint the typed quantity/pset/compliance/clash/topic case and whose one total `facts` `match` projects each case to its addressable `dict[str, object]` — never a stringified `dict[str, str]` bag that erases floats and bools. The cross-cutting concerns compose through the canonical owners: `run` threads the graduation `evidence_run` weave (`EvidenceScope.IFC_ANALYSIS` the seed row — span, fence, and receipt harvest composed once on the spine, no page-local tracer, no hand-authored `_ok`/`_emit` pair), whose flatten absorbs the rail-returning `_dispatch` so a provider exception, a selector parse fault, and a contract violation all meet on one carrier. `@beartype(conf=FAULT_CONF)` on `_dispatch` binds the shared `BeartypeConf(violation_type=BeartypeCallHintViolation)` so a contract violation raises the canonical root the rail folds. Resilience splits by boundary class: the OCC clash tree (`Clasher.clash()` over `ifcopenshell.geom.tree`) is this owner's transient-native-failure boundary and rides the runtime-pinned `guarded_sync(RetryClass.OCC_NATIVE, ...)` row — the exported in-process transient policy (attempts over `RuntimeError`), never a bare `stamina.retry` mint — while the durable `.bcfzip`/IDS-report archive WRITE stays the `python:data/spatial` boundary's columnar product. The receipt graduates as the geometry-minted `GeometrySubject.BIM_COMPLIANCE` member — the differentiated evidence class IDS/clash/BCF verdicts own, distinct from the section-integral and lifecycle members their owners bind — `graduates()` returning the local `rasm.geometry.graduation` `GeometryHandoff` carrier whose `wire()` projection is the compute crossing. The BCF verb is the composition apex — it re-runs the clash leg and stacks `ifcclash` overlaps into `bcf` topics with viewpoints AND the `Clasher.get_viewpoint_snapshot` OCC-rendered image bound per topic, rather than a same-string round-trip. The selecting verbs never touch a raw query string: the free-form selector is validated once at admission through `IfcSelector.filter`/`IfcSelector.parse` from `geometry:ifc/selector.md#SELECTOR`, so a malformed selector is a typed `Error(BoundaryFault)` on the rail at the boundary rather than a silent empty `filter_elements` match three arms deep. The C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the analysis verbs and the buildingSMART validation output the managed projection does not produce.

## [01]-[INDEX]

- [01]-[ANALYSIS]: the quantity, Pset, IDS, clash, space-program, and BCF analysis verbs under one `AnalysisKind`-discriminated owner folding into one `AnalysisRow` `@tagged_union` row algebra with `of_*` smart constructors and a self-projecting `facts` fold, every selecting arm fed by the `IfcSelector` validated gate, woven through the graduation `evidence_run` weave with the `@beartype(conf=FAULT_CONF)` contract fence and the runtime-pinned `OCC_NATIVE` retry row, the IDS arm reading BOTH the per-spec entity ratio and the `Json(ids).report()` `Results` roll-up, the run graduating on kind-specific evidence under `BIM_COMPLIANCE`.

## [02]-[ANALYSIS]

- Owner: `IfcAnalysis` — the `@staticmethod` boundary capsule dispatching the analysis verbs over the IfcOpenShell ecosystem through a thin `_dispatch` rail-fold over per-verb helpers, mirroring the sibling `IfcLifecycle`/`IfcStructural`; `AnalysisKind` the closed `StrEnum` selecting the verb; `AnalysisRow` the `@tagged_union` of per-verb result rows whose `of_*` smart constructors mint the case and whose one `facts` total `match` projects it — `quantity` a GlobalId plus a `dict[str, float]` quantity map, `pset` a GlobalId plus a `dict[str, object]` property map, `compliance` a subject plus a `float` passing ratio and an `int` failing-check count shared by the IDS and space-program verdicts, `clash` an a/b GlobalId pair plus a `float` penetration distance and a cluster index, `topic` a guid plus title, an authored-from finding kind, and the snapshot verdict — so the six verbs share one row carrier whose case IS the shape; `AnalysisResult` the typed receipt carrying the kind, the subject element set, the `tuple[AnalysisRow, ...]` rows, and the IDS arm's `Json`-report `checks_pass` roll-up, conforming structurally to the runtime-checkable `ReceiptContributor` Protocol (`contribute -> Iterable[Receipt]` — the weave's harvest step emits it) and owning the kind-specific `evidence` ledger the graduation carrier folds, the subject the geometry-minted `GeometrySubject.BIM_COMPLIANCE` member carried by the module constant rather than a per-receipt `subject: str` field racing the discriminant.
- Cases: `AnalysisKind` rows `QUANTITY` (quantity takeoff over `util.element`) · `PSET` (property-set queries) · `IDS` (model-checking over `ifctester.ids` plus the `Json` `Results` roll-up) · `CLASH` (clash sets over `ifcclash`) · `SPACE_PROGRAM` (`IfcSpace` area validation against a program table) · `BCF` (issue authoring over the `bcf` library that consumes the clash findings as topics with OCC-rendered snapshots) — matched by `match`/`assert_never`, each dispatching to the ecosystem tool that owns it and folding its provider output into the shared `AnalysisRow` case, never a per-verb row dialect. A new verb is one `AnalysisKind` row, one `AnalysisRow.of_*` constructor, one dispatch arm, and one `evidence` key, breaking every dispatch site at type-check time under `ty`.
- Selector gate: the `query` meaning is fixed by the kind through the `QUERY_SPLIT` data table — `QUANTITY`/`PSET` are pure selectors, `CLASH`/`BCF` carry an `a#b` side pair, `IDS`/`SPACE_PROGRAM` carry a spec path or JSON table — so the spec parse is one table-keyed split rather than a `partition`-per-arm string ladder. The selecting verbs never thread a raw query into `filter_elements`: each opens with `IfcSelector.filter(model, query)` — `IfcSelector.parse(query).map(filter_elements)` — so the `lark` grammar admits the closed selection vocabulary once and lifts an `UnexpectedInput` parse failure into the rail at the boundary, and the validated `SelectorQuery.filter_string` re-serializes the selection to the exact grammar `ifcopenshell` consumes. The `CLASH`/`BCF` arms derive their two `ClashSource.selector` strings through `_clash_sides`, which folds the `a#b` side pair through the one polymorphic `IfcSelector.parse(Iterable[str])` batch — both sides validate under one `traversed(..., by=Disposition.ABORT)` rail short-circuiting on the first malformed member — defaulting both sides to the `'a'`-all mode for a whole-model intersection when the `query` is empty.
- Entry: `IfcAnalysis.run` is the one `@beartype`-fenced entry taking an `ifcopenshell.file`, an `AnalysisKind`, and a `query`, returning `RuntimeRail[AnalysisResult]` by composing `evidence_run(EvidenceScope.IFC_ANALYSIS, f"run.{kind}", lambda: IfcAnalysis._dispatch(model, kind, query))` — the weave opens the seeded span, runs the fence INSIDE it so the faults `_convert` records a provider exception on the live recording span and sets ERROR, FLATTENS the rail-returning `_dispatch` (a selector parse fault arrives already typed on the rail and meets the converted provider fault on one carrier), harvests the structurally-conforming `AnalysisResult.contribute` stream on the cleared `Ok`, and closes OK once — the span-fence-emit-ok weave stated once on the graduation spine, composed here, never re-authored. The dispatch is a rail-returning fold: each arm yields `RuntimeRail[AnalysisResult]`, the selecting arms by `IfcSelector.filter(...).map(...)`, the non-selecting arms lifted through `Ok`. Graduation stays the caller's own step on the returned receipt. Each arm derives its own `subjects` from the verb's true subject set: validated `filter_elements` GlobalIds for the selecting arms, `IfcSpace` GlobalIds for `SPACE_PROGRAM`, spec names for `IDS`, clash a-side GlobalIds for `CLASH`, authored topic guids for `BCF`.
- Auto: quantity takeoff folds `util.element.get_psets(element, qtos_only=True)` over the validated selected set into one merged `quantity` map per element, keeping the numeric quantities as floats; pset queries fold the full `get_psets` map into the `pset` rows preserving native value types. IDS validation runs an `Ids` specification against the model, then reads BOTH verdict depths: the per-spec `Specification.status` tri-state plus `passed_entities`/`failed_entities` entity sets fold a `compliance` row whose passing ratio is `len(passed) / max(len(passed) + len(failed), 1)` — a real per-element fraction, EXCLUDING the `status is None` not-applicable spec (the `if spec.status is not None` guard) rather than folding it as a `0.0`-ratio row that would poison the `evidence` mean — AND the `Json(document).report()` `Results` graph lands the sharper roll-up: the root `percent_checks_pass` rides `AnalysisResult.checks_pass` so the evidence ledger keys the per-check failing fraction beside the per-entity one, two independent compliance bars off one validation pass (`percent_*` lives on the `Results` ROOT, the `total_applicable_*` totals on the `ResultsSpecification` rows — never conflated). Clash detection runs the `ifcclash` clash-set query over the validated `ClashSource.selector` sides at one configurable `ClashSet` mode/tolerance, then `smart_group_clashes` clusters the overlaps so each `clash` row carries its `ClashResult` GlobalId pair, penetration `distance`, and spatial-cluster index in one pass — the grouper writes the per-clash cluster handle into the clash-set internal state, and the fold defaults the index to `0` through the `NotRequired[int]` slot when the handle is absent, never dropping the row. Space-program validation decodes the `query` program table, scales each `IfcSpace` net floor area to project units through `util.unit.calculate_unit_scale` before the comparison, and folds a `compliance` row whose ratio is `actual/target` per PROGRAMMED space only (the `program.get(key) > 0.0` guard). The BCF arm re-runs the clash leg once and composes each overlap INTO a topic through the `_topic` per-collision projection — `BcfXml.create_new` opens the project, `add_topic(..., topic_type="Clash")` mints one topic per clash, `add_viewpoint_from_point_and_guids` binds each topic to the `ClashResult.p1` overlap point and the offending GUID pair, and `Clasher.get_viewpoint_snapshot` renders the OCC viewpoint image the topic binds (the mined exporter row — the snapshot verdict rides the topic row) — the durable `.bcfzip` `BcfXml.save` flush deferred to the `python:data/spatial` boundary. The OCC clash tree is the one transient-native-failure boundary: `_run_clash` folds the `clash()`/`smart_group_clashes` pass through `guarded_sync(RetryClass.OCC_NATIVE, solve, subject="ifc.clash.tree")` — the runtime-pinned in-process transient row (its policy retries `RuntimeError`), composed where the provider is invoked, never a page-local `stamina.retry` mint and never a deferred downstream concern that drops the retry the OCC kernel demands.
- Receipt: `AnalysisResult.contribute` yields one `Receipt.of("rasm.geometry.ifc.analysis", ("emitted", subject, facts))` row — the 2-arg owner-then-`(Phase, subject, facts)` shape — whose facts are the per-row `AnalysisRow.facts` projection joined with the subject count and the resolved `evidence` ledger as native scalars; the weave's harvest step emits it on the cleared `Ok`, so no page-local `@receipted` leg exists. The run graduates through `AnalysisResult.graduates(key, ceiling)` returning the local `GeometryHandoff` carrier under `GeometrySubject.BIM_COMPLIANCE` — the differentiated member the IDS/clash/BCF evidence class owns (algebra keeps `numerical-primitive`; the section-integral and lifecycle owners bind their own members) — over the kind-specific `evidence` residual ledger: the IDS verdict keys the per-entity failing fraction (`1 - mean(passing-ratio)`) AND the per-check `check-fail` fraction off the `Json` roll-up, `SPACE_PROGRAM` the per-space failing fraction, `CLASH` the unresolved-cluster count, and the takeoff/BCF verbs the empty-result fraction, so a model breaching its ceiling fails the carrier's residual-over-ceiling `admitted` verdict rather than crossing clean; the crossing to compute is the carrier's `wire()` data, never an import.
- Packages: `ifcopenshell` (`util.element.get_psets`/`util.unit.calculate_unit_scale`), `ifctester` (`ids.open`/`Ids.validate`/`Ids.specifications`/`Specification.name`/`Specification.status`/`Specification.passed_entities`/`Specification.failed_entities`, `reporter.Json(ids).report() -> Results` — the machine-readable roll-up whose root carries `percent_checks_pass` and whose `ResultsSpecification` rows carry `total_applicable_fail`), `ifcclash` (`Clasher`/`ClashSettings`/the `ClashSet`/`ClashSource` TypedDicts/`Clasher.clash`/`Clasher.smart_group_clashes`/`Clasher.get_viewpoint_snapshot` the OCC-rendered topic snapshot/`ClashResult.a_global_id`/`b_global_id`/`p1`/`distance`), `bcf-client` (`bcf.v3.bcfxml.BcfXml.create_new`/`add_topic`/`TopicHandler.add_viewpoint_from_point_and_guids`/`TopicHandler.guid`/`TopicHandler.topic.title`), `geometry:ifc/selector.md#SELECTOR` (`IfcSelector.filter`/`IfcSelector.parse(Iterable[str])` the validated selection engine, the only `filter_elements` caller), geometry (`evidence_run`/`EvidenceScope`/`GeometryHandoff`/`GeometrySubject` the graduation spine), `beartype` (`@beartype` on `run`, `@beartype(conf=FAULT_CONF)` on `_dispatch`), `expression` (`tagged_union`/`tag`/`case`, `Ok`, `Result.map`/`bind`, `Map.of_seq` the `QUERY_SPLIT` table), runtime (`RuntimeRail`/`FAULT_CONF`, `guarded_sync`/`RetryClass.OCC_NATIVE` the pinned in-process transient row, `ContentKey` from `rasm.runtime.identity`, `Receipt`).
- Growth: a new analysis verb is one `AnalysisKind` row, one `AnalysisRow.of_*` constructor plus one `facts` arm, one dispatch arm, one `QUERY_SPLIT` row, and one `evidence` key; a new selection axis is one `IfcSelector` grammar alternative, never a local query-parse fold here; a new IDS specification is authored through `ifctester`, never a local rule fold; IDS document AUTHORING (the `Ids(...)`/facet family/`to_xml` surface) is the named next verb when a consumer supplies a rule vocabulary — one row, never a second engine; a stricter compliance bar is one tighter ceiling row the caller supplies; zero new surface.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy (projected in-process); no durable store; no Rhino/GH mutation; no raw `query` string threaded past admission into `filter_elements`. The deleted forms: a hand-rolled IFC parser; a second selection engine; a bespoke non-portable IDS rule fold; a binary `1.0`/`0.0` IDS verdict where the entity sets carry the real fraction and the `Json` roll-up the per-check one; a page-local `trace.get_tracer` mint, hand-authored span/`_ok`/`_emit` weave, or `@receipted` leg where `evidence_run` owns the weave; a bare `stamina.retry` mint where the runtime-pinned `guarded_sync(RetryClass.OCC_NATIVE, ...)` row owns the OCC transient; a compute-interior graduation binding or a `GraduationReceipt.graduates` call where the local `rasm.geometry.graduation` owner mints the vocabulary and `graduates()` returns the local `GeometryHandoff`; a `"numerical-primitive"` subject carrying compliance evidence where `BIM_COMPLIANCE` is the differentiated member; an OCCT bounding-overlap clash reimplementation; a stringified `dict[str, str]` row; a same-string BCF round-trip; a topic authored without its OCC snapshot where the mined `get_viewpoint_snapshot` row binds it; a raw-unit space comparison; an erased `dict[str, object]` clash bag blind-subscripted; a hand-rolled `parse(a).bind(parse(b))` per-arm side loop; a `ClashSource` literal omitting the required `file` key; an unflushed `BcfXml` document silently dropped where the topic rows are the wire carry and the durable write is the data seam's; the legacy 4-positional `Receipt.of`; an inlined residual-vs-ceiling comparison; and a parallel per-verb class family.

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

if TYPE_CHECKING:  # worker: every runtime ifcopenshell use is the function-local `import ifcopenshell.<sub>  # noqa: PLC0415` that binds `ifcopenshell` in scope, so the runtime module loads clean and the boundary-scope import policy holds (the selector sibling shape)
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
    # The catalog `ifcclash.ClashResult` shape plus the derived spatial-cluster index the grouper
    # writes — the typed payload the clash leg reads field-by-field, never a `dict[str, object]` bag.
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
        # One total match projecting each case to its addressable fact map; native floats/maps
        # ride the runtime EventDict whose enc_hook=repr renderer serializes them with no str() coerce.
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

# The IFC-analysis evidence crosses on the geometry-minted BIM_COMPLIANCE member — the differentiated
# evidence class IDS/clash/BCF verdicts own; typed as the union member so an unlisted subject fails
# at the boundary under `ty`.
ANALYSIS_SUBJECT: Final[GeometrySubject] = GeometrySubject.BIM_COMPLIANCE

# The query's meaning is fixed by the kind: the side-pair verbs split on `#`, the selecting
# verbs take the whole query as the selector, the spec-bearing verbs take the whole query as a
# path/table. One immutable table key, never a partition-per-arm string ladder.
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
        # The residual ledger is kind-specific, never a row count: the verdict verbs key the mean
        # failing fraction off the real per-spec/space passing ratio (IDS additionally the per-check
        # fraction off the Json roll-up), CLASH the unresolved cluster count, the takeoff/BCF verbs
        # the empty-result fraction.
        match self.kind:
            case AnalysisKind.IDS:
                ratios = tuple(r.compliance[1] for r in self.rows if r.tag == "compliance")
                entity_fail = 1.0 - sum(ratios) / len(ratios) if ratios else 0.0
                check_fail = 1.0 - self.checks_pass / 100.0 if self.checks_pass is not None else 0.0
                return {"non-compliant": entity_fail, "check-fail": check_fail}
            case AnalysisKind.SPACE_PROGRAM:
                # Only the programmed compliance rows reach here, so an empty `ratios` is "no
                # compliance signal" keyed 0.0 — never the fabricated total-non-compliance mean.
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
        # the local carrier's residual-over-ceiling `admitted` verdict gates; `wire()` is the compute crossing.
        return GeometryHandoff.of(ANALYSIS_SUBJECT, evidence_key, self.evidence(), ceiling)


# --- [OPERATIONS] ----------------------------------------------------------------------


class IfcAnalysis:
    @staticmethod
    @beartype
    def run(model: "ifcopenshell.file", kind: AnalysisKind, query: str) -> "RuntimeRail[AnalysisResult]":
        # the graduation weave owns span-fence-emit-ok: the seeded IFC_ANALYSIS span opens, the fence
        # runs INSIDE it (the faults `_convert` records a provider exception on the live span and sets
        # ERROR), the flatten absorbs the rail-returning `_dispatch` so a selector parse fault meets
        # the converted provider fault on one carrier, and the harvest emits the structurally-
        # conforming `AnalysisResult.contribute` stream on the cleared Ok. Graduation stays the
        # caller's own step on the returned receipt, mirroring `IfcStructural.run`.
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
        # A space absent from the program table carries no program signal and is EXCLUDED — admitting
        # a 0.0-ratio row would read an out-of-program space as total non-compliance.
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
        # BOTH verdict depths off one validation pass: the per-spec passed/failed entity sets give the
        # real per-element passing fraction (the `status is None` not-applicable spec EXCLUDED — a skip
        # carries no compliance signal and would poison the evidence mean), and the Json Results graph
        # gives the sharper per-check roll-up — `percent_checks_pass` rides the Results ROOT, while the
        # per-spec `total_applicable_*` totals live on the ResultsSpecification rows, never conflated.
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
        # The two side selectors validate through the ONE polymorphic batch parse the selector page
        # exposes for exactly this caller — never a hand-rolled `parse(a).bind(parse(b))` per-arm loop.
        # An empty `query` is the whole-model default: both sides drop to the 'a'-all mode.
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

        # The authored topic rows ARE this owner's wire carry. The durable `.bcfzip` `BcfXml.save`
        # flush is the `python:data/spatial` boundary's columnar product, so this leg authors the
        # in-memory archive and reads its topic GUIDs back rather than discarding an unflushed document.
        def compose(run: "tuple[Clasher, tuple[ClashRow, ...], ClashSet]") -> AnalysisResult:
            clasher, clashes, clash_set = run
            document = BcfXml.create_new("rasm.ifc.analysis")
            rows = tuple(IfcAnalysis._topic(document, clasher, clash_set, c) for c in clashes)
            return AnalysisResult(AnalysisKind.BCF, tuple(r.topic[0] for r in rows), rows)

        return IfcAnalysis._run_clash(model, sides).map(compose)

    @staticmethod
    def _topic(document: "bcf.v3.bcfxml.BcfXml", clasher: "Clasher", clash_set: "ClashSet", collision: ClashRow) -> AnalysisRow:
        # One overlap -> one topic bound to the ClashResult.p1 point, the offending GUID pair, AND the
        # OCC-rendered viewpoint snapshot the mined `get_viewpoint_snapshot` exporter row captures —
        # the issue-authoring leg stacking ifcclash + bcf, never a same-string round-trip.
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
            # `file` is a required `ClashSource` key; the pre-loaded `ifc` model makes the empty path
            # inert. The 'e' expression mode consumes the validated `filter_string`, the 'a'-all mode
            # the empty whole-model default — never a raw caller query.
            base: ClashSource = {"file": "", "ifc": model}
            return {**base, "selector": selector, "mode": "e"} if selector else {**base, "mode": "a"}

        # The OCC clash tree is the transient-native-failure boundary the runtime-pinned OCC_NATIVE row
        # retries in-process (its policy targets RuntimeError): `clash()`/`smart_group_clashes` run
        # under `guarded_sync` before the rows read back — never a page-local stamina mint. The
        # returned rail carries the live Clasher and the clash set so the BCF arm binds snapshots.
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
