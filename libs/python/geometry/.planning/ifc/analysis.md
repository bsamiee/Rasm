# [PY_GEOMETRY_IFC_ANALYSIS]

IFC property, quantity, and relationship analysis and standards-conformant validation — AEC verbs the tessellation hop drops: quantity takeoff, Pset queries, IDS model-checking, clash detection, space-program validation, and BCF issue authoring over `ifcopenshell.util`, `ifctester`, `ifcclash`, `ifc5d.qto`, and `bcf`, every provider output folded into one `AnalysisRow` algebra. C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the analysis verbs and buildingSMART validation output that projection never produces.

Every selecting verb admits its query through `IfcSelector` (`ifc/selector#SELECTOR`), so a malformed selector faults typed at the boundary, never a silent empty `filter_elements` match three arms deep, and the `SelectorMatch` it hands back carries the canonical `filter_string` every receipt keys its evidence on. `run` threads the graduation `evidence_run` weave under `EvidenceScope.IFC_ANALYSIS` and carries no head decorator — `@beartype(conf=FAULT_CONF)` on `_dispatch` is the one innermost seam this capsule fences, as its two peers hold. OCC clash trees are this owner's one transient-native boundary, riding the runtime-pinned `guarded_sync(RetryClass.OCC_NATIVE)` row, never a bare `stamina.retry` mint; the authored `.bcfzip` rides home as archive BYTES on the receipt and the durable write stays `python:data/spatial`'s. Evidence graduates under `GeometrySubject.BIM_COMPLIANCE`, the differentiated member the IDS/clash/BCF verdict class owns, distinct from the section-integral and lifecycle members their owners bind.

## [01]-[INDEX]

- [02]-[ANALYSIS]: analysis verbs over one `AnalysisRow` algebra, the `IfcSelector` gate, the `OCC_NATIVE` retry row, the columnar row egress, the `run_async` twin recording the BCF arm's operational trail, and kind-specific graduation evidence under `BIM_COMPLIANCE`.

## [02]-[ANALYSIS]

- Owner: `IfcAnalysis` dispatches verbs through one rail-returning `_dispatch` fold; `AnalysisRow` is the one carrier whose case IS the shape — one algebra, never a per-verb row dialect; graduation subject rides the `ANALYSIS_SUBJECT` module constant, never a per-receipt `subject: str` field racing the discriminant.
- Entry: `run` takes the model, the kind, the free-form `query`, and the `composition` custody key. `query` meaning is kind-fixed through the `QUERY_SPLIT` table — pure selector, `a#b` side pair, or spec path/JSON table — read ONCE at the `_dispatch` head so every row governs the arm it keys, the four empty-delimiter rows included and the `BCF` row governing the BCF arm, where a hardcoded `CLASH` key leaves five of six rows unread. `CLASH`/`BCF` sides arrive pre-split and validate under one polymorphic batch parse aborting on the first malformed member, an empty query defaulting both sides to whole-model mode. `BCF` is the composition apex, re-running the clash leg and stacking overlaps into `bcf` topics with viewpoints AND the OCC-rendered `get_viewpoint_snapshot` image bound per topic, never a same-string round-trip.
- Auto: IDS reads BOTH verdict depths off one validation pass — per-spec entity ratio and the `Json(ids).report()` roll-up, `percent_checks_pass` on the `Results` ROOT and `total_applicable_*` totals on the `ResultsSpecification` rows, never conflated — excluding a `status is None` not-applicable spec whose `0.0` row poisons the evidence mean. Space-program validation measures through the package that owns measurement: `ifc5d.qto.quantify` folds the shared `RuleSet` base-quantity table over the space set and the grade reads the `Qto_SpaceBaseQuantities.NetFloorArea` cell off the returned `ResultsDict`, so no local `get_psets(qtos_only=True)` key fold and no unit-scale square survive — the take-off already converts to the model's declared project units, the ONE regime the program table is read in. Two exclusions are distinct and both named: a space absent from the program table carries no program signal and is excluded from grading, while a space the take-off could not measure is a TYPED REFUSAL on its own `unquantified` axis, never a `0.0` area grading as total non-compliance.
- Receipt: receipts carry the census, frames carry the rows. `contribute` emits one row per run — row count, subject count, archive extent where one exists, and the residual ledger — because a whole-model take-off is one fact key per element per quantity and a flattened row stream turns the runtime receipt into a hundred-thousand-key dict per run; `frame` projects those rows as ONE columnar `EvidenceFrame` through the graduation port, columns off the first row's `facts` keys exactly as the sibling lifecycle rollup does. `evidence` ledger is kind-specific, never a row count — IDS keys per-entity AND per-check failing fractions, `SPACE_PROGRAM` the per-space failing fraction beside its unquantified count, `CLASH` the unresolved-cluster count, takeoff/BCF the empty-result fraction — so a model breaching its ceiling fails the carrier's `admitted` verdict rather than crossing clean. `graduates` and `frame` derive their own `ContentKey` from the receipt's `spec` — kind plus the validated query projection — through the spine's `evidence_key`, so no caller mints a key for evidence it did not produce. `_distributed` records the `non-compliant` fraction as the `rasm.geometry.compliance.noncompliant` charter measure at the producing fold; the cluster-count and empty-fraction keys name no charter row and record nothing, a count or boolean gate earning no histogram.
- Law: the BCF arm alone lands durable evidence on the `python:runtime/observability/journal#LEDGER` plane — one `OPERATIONAL` `AuditFact` keyed on the run's `spec`, its topic GUIDs the subject index — because that arm AUTHORS an archive the data seam persists while every other kind reads the model and produces a verdict nothing keeps. `run_async` is its seat, the awaitable twin this caller-floor owner mints over the band hop, since recording suspends and `_distributed` is a synchronous charter projection. No meter rides the leg: the archive crosses as receipt payload and the data seam charges the write it performs, so a byte count at both tiers bills one artifact twice.
- Packages: `ifcopenshell`, `ifctester`, `ifcclash`, `bcf-client`, and `ifc5d` (`qto.quantify` the space base-quantity take-off, its `rules` key arriving as the sibling lifecycle owner's `RuleSet` vocabulary rather than a second transcription) per the fence imports; `IfcSelector` is the only `filter_elements` caller.
- Growth: a new verb is one `AnalysisKind` row, one `of_*` constructor and one `facts` arm, one dispatch arm, one `QUERY_SPLIT` row, and one `evidence` key — the frame column set following from `facts` with no edit here, and a durable trail only where the verb WRITES something, as one `_evidence` arm; a new selection axis is one `IfcSelector` grammar alternative, never a local query-parse fold; a new quantity axis on the space grade is one `SPACE_AREA` cell row; IDS document AUTHORING (`Ids(...)`/facet family/`to_xml`) is the named next verb once a consumer supplies a rule vocabulary — one row, never a second engine.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy; no ledger, custody, or retention window minted here, the plane arriving bound at the composition root and this owner declaring a `Retain` class alone; no durable store — topic rows and the archive bytes are the wire carry, the durable `.bcfzip`, IDS-report, and frame writes the data seam's; no Rhino/GH mutation; no raw `query` string threaded past admission into `filter_elements`; no hand-rolled quantity-key fold where `ifc5d.qto` owns measurement; no per-row fact map on the receipt stream where the frame is the columnar carrier.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from enum import StrEnum
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import TYPE_CHECKING, Final, Literal, NotRequired, TypedDict, assert_never

from beartype import beartype
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.json import decode

from rasm.geometry.graduation import (
    EVIDENCE_DOMAIN,
    EvidenceFrame,
    EvidenceScope,
    GeometryHandoff,
    GeometrySubject,
    charter_record,
    evidence_key,
    evidence_run,
)
from rasm.geometry.ifc.costing import RuleSet
from rasm.geometry.ifc.selector import IfcSelector
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.journal import Actor, Assigned, AuditFact, Fact, Journal, Party, Retain
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.resilience import RetryClass, guarded_sync

if TYPE_CHECKING:  # every runtime provider use is a function-local boundary import, so the module loads clean; the
    # type-only names here are what make the `Clasher`/`ClashSet`/`BcfXml` string annotations below resolvable at all
    import bcf.v3.bcfxml
    import ifcopenshell
    from ifcclash.ifcclash import Clasher, ClashSet

# --- [TYPES] ---------------------------------------------------------------------------


class AnalysisKind(StrEnum):
    QUANTITY = "quantity"
    PSET = "pset"
    IDS = "ids"
    CLASH = "clash"
    SPACE_PROGRAM = "space-program"
    BCF = "bcf"


class ClashRow(TypedDict):
    # `ifcclash.ClashResult` shape and the derived spatial-cluster index the grouper writes.
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

# this owner's one name, serving the receipt label and the durable audit actor alike, so a rename cannot leave a
# receipt stream and an evidence-plane actor column under two spellings.
OWNER: Final[str] = "rasm.geometry.ifc.analysis"

# One delimiter row per kind, read ONCE at the `_dispatch` head so every row governs the arm it keys — the four
# empty-delimiter rows the no-split path and the `BCF` row the BCF arm, where a hardcoded `CLASH` key leaves five of six
# rows spelling nothing. Mirrors the sibling `PHASE_DELIMITER` fold, never a `.get` default that drops a kind.
QUERY_SPLIT: Final[Map[AnalysisKind, str]] = Map.of_seq([
    (AnalysisKind.QUANTITY, ""),
    (AnalysisKind.PSET, ""),
    (AnalysisKind.IDS, ""),
    (AnalysisKind.SPACE_PROGRAM, ""),
    (AnalysisKind.CLASH, "#"),
    (AnalysisKind.BCF, "#"),
])

# The `ifc5d.qto` cell the space-program grade reads off the `ResultsDict` (`element -> qto -> quantity -> float`);
# a miss on this pair IS the unquantified case, never a zero area. One row, so a second quantity axis is one row.
SPACE_AREA: Final[tuple[str, str]] = ("Qto_SpaceBaseQuantities", "NetFloorArea")

# --- [MODELS] --------------------------------------------------------------------------


class AnalysisResult(Struct, frozen=True, gc=False):
    kind: AnalysisKind
    # run identity — the kind plus the validated query projection (canonical `filter_string`, spec path, program table,
    # or side pair) — from which `graduates`/`frame` derive their own `ContentKey` through the spine.
    spec: str
    subjects: tuple[str, ...]
    rows: tuple[AnalysisRow, ...]
    checks_pass: float | None = None  # IDS arm's Json-report root percent_checks_pass roll-up; None elsewhere
    # SPACE_PROGRAM spaces the take-off measured NO area cell for: their own axis against their own ceiling, because a
    # 0.0 area grades as total non-compliance and spells a measurement no producer took.
    unquantified: tuple[str, ...] = ()
    # BCF arm's authored `.bcfzip` archive bytes, the wire carry the data seam persists; empty on every other kind.
    product: bytes = b""

    def evidence(self) -> dict[str, float]:
        match self.kind:
            case AnalysisKind.IDS:
                ratios = tuple(r.compliance[1] for r in self.rows if r.tag == "compliance")
                entity_fail = 1.0 - sum(ratios) / len(ratios) if ratios else 0.0
                check_fail = 1.0 - self.checks_pass / 100.0 if self.checks_pass is not None else 0.0
                return {"non-compliant": entity_fail, "check-fail": check_fail}
            case AnalysisKind.SPACE_PROGRAM:
                # an empty `ratios` is "no compliance signal" keyed 0.0 — never a fabricated total-non-compliance mean —
                # and the unmeasured set rides its OWN key, so a model before take-off reads "N spaces unquantified"
                # rather than "0% compliant" against a mean nothing measured.
                ratios = tuple(r.compliance[1] for r in self.rows if r.tag == "compliance")
                return {"non-compliant": 1.0 - sum(ratios) / len(ratios) if ratios else 0.0, "unquantified": float(len(self.unquantified))}
            case AnalysisKind.CLASH:
                clusters = {r.clash[3] for r in self.rows if r.tag == "clash"}
                return {"clash-clusters": float(len(clusters))}
            case AnalysisKind.QUANTITY | AnalysisKind.PSET | AnalysisKind.BCF:
                return {"empty": 0.0 if self.rows else 1.0}
            case unreachable:
                assert_never(unreachable)

    def contribute(self) -> Iterable[Receipt]:
        # census, never the rows: a whole-model QUANTITY take-off carries one fact key per element per quantity, which
        # makes the runtime receipt a hundred-thousand-key dict per run. Per-row evidence crosses as `frame()`; the
        # receipt keeps the counts and the residual ledger the ceiling gates on, and the archive extent only where one
        # exists, so a kind that authored nothing publishes no zero-length claim.
        yield Receipt.of(
            OWNER,
            (
                "emitted",
                self.kind.value,
                {"rows": len(self.rows), "subjects": len(self.subjects)}
                | ({"product": len(self.product)} if self.product else {})
                | self.evidence(),
            ),
        )

    def graduates(self, ceiling: dict[str, float]) -> GeometryHandoff:
        return GeometryHandoff.of(ANALYSIS_SUBJECT, evidence_key(ANALYSIS_SUBJECT, self.spec), self.evidence(), ceiling)

    def frame(self) -> "RuntimeRail[EvidenceFrame]":
        # rows are kind-homogeneous, so the first row's fact keys ARE the column set; the folder's largest row sets —
        # one quantity or pset row per element — cross the geometry-to-data seam as one columnar frame per run instead
        # of a flattened fact map. An empty result frames zero rows rather than faulting, and a row set that is NOT
        # homogeneous rails on the port's own width check at this producer rather than raising past its consumer.
        names = tuple(self.rows[0].facts) if self.rows else ()
        table: dict[str, list[object]] = {
            "kind": [self.kind.value] * len(self.rows),
            **{name: [row.facts[name] for row in self.rows] for name in names},
        }
        return EvidenceFrame.of(ANALYSIS_SUBJECT, evidence_key(ANALYSIS_SUBJECT, self.spec), table)


# --- [OPERATIONS] ----------------------------------------------------------------------


def _archived(document: "bcf.v3.bcfxml.BcfXml") -> bytes:
    # `BcfXml.save(filename)` writes the zip and returns None, so the authored archive rides home as bytes through one
    # scoped temp path — the sibling lifecycle owner's SPF `write`-then-read shape. Without this carry the document is
    # dropped at function exit and the `.bcfzip` the page promises the data seam never crosses at all.
    with TemporaryDirectory(prefix="ifc-bcf-") as work:
        path = Path(work, "topics.bcfzip")
        document.save(path)
        return path.read_bytes()


def _distributed(result: AnalysisResult, composition: ScopeKey) -> AnalysisResult:
    # BIM_COMPLIANCE charter projection at the producing fold, off the one `evidence()` ledger the receipt, the frame,
    # and the handoff already read — the IDS and space-program `non-compliant` fraction IS the
    # `rasm.geometry.compliance.noncompliant` measure, so the spelling derives from the charter row and no producing
    # arm picks one. The cluster-count and empty-fraction keys name no charter row, so the projection skips them: a
    # count or boolean-shaped gate earns no histogram. Every verb runs caller-floor, so this is already the parent side.
    charter_record(ANALYSIS_SUBJECT, result.evidence(), composition=composition)
    return result


def _evidence(result: AnalysisResult) -> "Block[Fact]":
    # `_distributed`'s durable twin, and the BCF arm alone reaches it: that arm AUTHORS a `.bcfzip` the data seam
    # persists, so the issue set it opened against a model is a record an audit reads back, where every other kind
    # reads the model and produces a verdict nothing keeps. `OPERATIONAL` is the class — an issue log is the routine
    # project trail, not the disposal evidence a mutation leaves. Topic GUIDs are the subject index, this producer's
    # one honest source. No meter rides here: the archive crosses as receipt payload and the data seam charges the
    # write it actually performs, so a byte count on both tiers bills one artifact twice.
    if result.kind is not AnalysisKind.BCF:
        return Block.empty()
    return Block.singleton(
        AuditFact(
            action=f"{EVIDENCE_DOMAIN}.{result.kind.value}",
            actor=Party(kind=Actor.SERVICE, key=OWNER),
            target=Party(kind="model", key=result.spec),
            retention=Retain.OPERATIONAL,
            change=(Assigned(path="/topics", next=str(len(result.rows))), Assigned(path="/archive", next=str(len(result.product)))),
            subjects=result.subjects,
        )
    )


class IfcAnalysis:
    @staticmethod
    def run(
        model: "ifcopenshell.file", kind: AnalysisKind, query: str, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[AnalysisResult]":
        # weave flatten absorbs the rail-returning `_dispatch`, so a selector parse fault meets the converted provider
        # fault on one carrier; graduation stays the caller's own step on the returned receipt. The head carries no
        # decorator — `_dispatch` is the one innermost untrusted seam the capsule fences, as its two peers hold.
        return evidence_run(
            EvidenceScope.IFC_ANALYSIS,
            f"run.{kind}",
            lambda: IfcAnalysis._dispatch(model, kind, query).map(lambda result: _distributed(result, composition)),
            composition=composition,
        )

    @staticmethod
    async def run_async(
        model: "ifcopenshell.file", kind: AnalysisKind, query: str, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[AnalysisResult]":
        # the awaitable twin over the band hop: every verb runs caller-floor on a live pybind11 handle, so `run` is
        # synchronous whole and recording — which SUSPENDS — cannot land inside it or inside `_distributed`, the sync
        # charter projection this leg pairs with. The twin runs the same fold and records the BCF arm's authored
        # issue set past it; every other kind mints an empty block and this leg costs one map read. The record rail
        # BINDS into the verdict, so an armed plane refusing an issue-log fact reaches the caller that owns it.
        match IfcAnalysis.run(model, kind, query, composition=composition):
            case Result(tag="ok", ok=result):
                return (await Journal.record(_evidence(result), scope=composition)).map(lambda _landed: result)
            case refused:
                return Error(refused.error)

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _dispatch(model: "ifcopenshell.file", kind: AnalysisKind, query: str) -> "RuntimeRail[AnalysisResult]":
        # ONE `QUERY_SPLIT` read at the head partitions the query for every kind — the empty-delimiter rows passing the
        # whole query through — so each row governs by being read and the `CLASH`/`BCF` legs consume an already-split
        # pair rather than reaching for another kind's key. Each arm derives its own `spec` from the VALIDATED
        # projection it holds, so two spellings of one query key one piece of evidence.
        delimiter = QUERY_SPLIT[kind]
        head, _, tail = query.partition(delimiter) if delimiter else (query, "", "")
        match kind:
            case AnalysisKind.QUANTITY | AnalysisKind.PSET:
                quantities = kind is AnalysisKind.QUANTITY
                return IfcSelector.filter(model, head).map(
                    lambda matched: AnalysisResult(
                        kind,
                        f"{kind.value}|{matched.query.filter_string}",
                        tuple(e.GlobalId for e in matched.elements),
                        tuple(IfcAnalysis._takeoff(e, quantities) for e in matched.elements),
                    )
                )
            case AnalysisKind.SPACE_PROGRAM:
                return Ok(IfcAnalysis._space(model, head, f"{kind.value}|{head}"))
            case AnalysisKind.IDS:
                return Ok(IfcAnalysis._validate(model, head, f"{kind.value}|{head}"))
            case AnalysisKind.CLASH:
                return IfcAnalysis._clash_sides(head, tail).bind(lambda sides: IfcAnalysis._clash(model, sides, f"{kind.value}|{sides[0]}#{sides[1]}"))
            case AnalysisKind.BCF:
                return IfcAnalysis._clash_sides(head, tail).bind(lambda sides: IfcAnalysis._author(model, sides, f"{kind.value}|{sides[0]}#{sides[1]}"))
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _space(model: "ifcopenshell.file", table: str, spec: str) -> AnalysisResult:
        import ifc5d.qto  # ruff:ignore[import-outside-top-level]

        # Program-table decode runs under the weave fence, so a malformed JSON table is a BoundaryFault classified at
        # the seam, never an inline try/except in domain logic. Measurement is the PACKAGE'S — `qto.quantify` folds the
        # shared `RuleSet` base-quantity table over the space set and the grade reads its `SPACE_AREA` cell off the
        # returned `ResultsDict` — so no `get_psets(qtos_only=True)` key fold survives here and no unit scale either:
        # the take-off already converts SI to the model's declared project units, the ONE regime the table is read in.
        program = decode(table.encode(), type=dict[str, float])
        spaces = model.by_type("IfcSpace")
        qto, quantity = SPACE_AREA
        results = ifc5d.qto.quantify(model, set(spaces), ifc5d.qto.rules[RuleSet.IFC4.value])
        measured = {
            space: area
            for space in spaces
            for area in (results.get(space, {}).get(qto, {}).get(quantity),)
            if isinstance(area, (int, float))
        }
        # two exclusions, both named: a space absent from the program table carries no program signal and never grades,
        # while a targeted space the take-off could not measure lands on the `unquantified` axis — a `0.0` area
        # fallback spells total non-compliance for a measurement no producer took.
        targeted = tuple((space, program[name]) for space in spaces for name in (space.LongName or space.Name or "",) if program.get(name, 0.0) > 0.0)
        graded = tuple((space, target, measured[space]) for space, target in targeted if space in measured)
        rows = tuple(AnalysisRow.of_compliance(s.GlobalId, area / target, 0 if area >= target else 1) for s, target, area in graded)
        return AnalysisResult(
            AnalysisKind.SPACE_PROGRAM,
            spec,
            tuple(s.GlobalId for s, _, _ in graded),
            rows,
            unquantified=tuple(space.GlobalId for space, _ in targeted if space not in measured),
        )

    @staticmethod
    def _takeoff(element: "ifcopenshell.entity_instance", quantities: bool) -> AnalysisRow:
        import ifcopenshell.util.element  # ruff:ignore[import-outside-top-level]

        psets = ifcopenshell.util.element.get_psets(element, qtos_only=quantities)
        merged = {f"{name}.{key}": value for name, body in psets.items() for key, value in body.items()}
        if quantities:
            return AnalysisRow.of_quantity(element.GlobalId, {k: float(v) for k, v in merged.items() if isinstance(v, int | float)})
        return AnalysisRow.of_pset(element.GlobalId, merged)

    @staticmethod
    def _validate(model: "ifcopenshell.file", spec_path: str, spec: str) -> AnalysisResult:
        import ifctester.ids  # ruff:ignore[import-outside-top-level]
        import ifctester.reporter  # ruff:ignore[import-outside-top-level]

        document = ifctester.ids.open(spec_path)
        document.validate(model)
        rows = tuple(
            AnalysisRow.of_compliance(
                row.name, len(row.passed_entities) / max(len(row.passed_entities) + len(row.failed_entities), 1), len(row.failed_entities)
            )
            for row in document.specifications
            if row.status is not None
        )
        applicable = tuple(row.name for row in document.specifications if row.status is not None)
        results = ifctester.reporter.Json(document).report()
        return AnalysisResult(AnalysisKind.IDS, spec, applicable, rows, checks_pass=float(results["percent_checks_pass"]))

    @staticmethod
    def _clash_sides(head: str, tail: str) -> "RuntimeRail[tuple[str, str]]":
        # the pair arrives PRE-SPLIT from the one head read, so this leg neither re-partitions nor reaches for another
        # kind's delimiter row; an empty query defaults both sides to whole-model mode and a bare `a` side clashes
        # against itself. One polymorphic batch parse validates both and aborts on the first malformed member.
        return (
            Ok(("", ""))
            if not head and not tail
            else IfcSelector.parse((head, tail or head)).map(lambda sides: (sides[0].filter_string, sides[1].filter_string))
        )

    @staticmethod
    def _clash(model: "ifcopenshell.file", sides: tuple[str, str], spec: str) -> "RuntimeRail[AnalysisResult]":
        return IfcAnalysis._run_clash(model, sides).map(
            lambda run: AnalysisResult(
                AnalysisKind.CLASH,
                spec,
                tuple(c["a_global_id"] for c in run[1]),
                tuple(AnalysisRow.of_clash(c["a_global_id"], c["b_global_id"], c["distance"], c.get("cluster", 0)) for c in run[1]),
            )
        )

    @staticmethod
    def _author(model: "ifcopenshell.file", sides: tuple[str, str], spec: str) -> "RuntimeRail[AnalysisResult]":
        from bcf.v3.bcfxml import BcfXml  # ruff:ignore[import-outside-top-level]

        # this leg authors the archive, reads its topic GUIDs back, AND carries the archive bytes on the receipt, so the
        # durable `.bcfzip` write is the data seam's own step over a payload that actually crossed.
        def compose(run: "tuple[Clasher, tuple[ClashRow, ...], ClashSet]") -> AnalysisResult:
            clasher, clashes, clash_set = run
            document = BcfXml.create_new("rasm.ifc.analysis")
            rows = tuple(IfcAnalysis._topic(document, clasher, clash_set, c) for c in clashes)
            return AnalysisResult(AnalysisKind.BCF, spec, tuple(r.topic[0] for r in rows), rows, product=_archived(document))

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
        from ifcclash.ifcclash import Clasher, ClashSettings, ClashSet, ClashSource  # ruff:ignore[import-outside-top-level]

        def source(selector: str) -> ClashSource:
            # `file` is a required ClashSource key; the pre-loaded `ifc` model makes the empty path inert. Mode
            # 'e' consumes the validated `filter_string`, mode 'a'-all the whole-model default.
            base: ClashSource = {"file": "", "ifc": model}
            return {**base, "selector": selector, "mode": "e"} if selector else {**base, "mode": "a"}

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
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
