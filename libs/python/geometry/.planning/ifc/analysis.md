# [PY_GEOMETRY_IFC_ANALYSIS]

IFC property, quantity, and relationship analysis and standards-conformant validation — AEC verbs the tessellation hop drops: quantity takeoff, Pset queries, IDS model-checking, clash detection, space-program validation, and BCF issue authoring over `ifcopenshell.util`, `ifctester`, `ifcclash`, `ifc5d.qto`, and `bcf`, every provider output folded into one `AnalysisRow` algebra. The model's own map georeference decodes here beside them as the branch's ONE model CRS source, crossing as a `GeoreferenceFact` wire shape rather than an analysis row, because a coordinate frame carries no verdict to grade. C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the analysis verbs and buildingSMART validation output that projection never produces.

Every selecting verb admits its query through `IfcSelector`; a malformed selector faults at the boundary, and `SelectorMatch` carries the canonical `filter_string`. `run` delegates observation through `evidence_run`. OCC clash trees stay behind the runtime-pinned retry row, BCF archive bytes remain on `AnalysisResult`, and durable writes stay with `python:data/spatial`.

## [01]-[INDEX]

- [02]-[ANALYSIS]: analysis verbs over one `AnalysisRow` algebra, the `IfcSelector` gate, the IDS parse/resolve/evaluate split gating the grade on `IdsResolved`, the `OCC_NATIVE` retry row, the columnar row egress, the `run_async` twin recording the BCF arm's operational trail, kind-specific graduation evidence under `BIM_COMPLIANCE`, and the `GeoreferenceFact` decode crossing beside the dispatch as the model CRS seam.

## [02]-[ANALYSIS]

- Owner: `IfcAnalysis` dispatches verbs through one rail-returning `_dispatch` fold; `AnalysisRow` is the one carrier whose case IS the shape — one algebra, never a per-verb row dialect; graduation subject rides the `ANALYSIS_SUBJECT` module constant, never a per-result `subject: str` field racing the discriminant.
- Entry: `run` takes the model, the kind, the free-form `query`, and the `composition` custody key. `query` meaning is kind-fixed through the `QUERY_SPLIT` table — pure selector, `a#b` side pair, or spec path/JSON table — read ONCE at the `_dispatch` head so every row governs the arm it keys, the four empty-delimiter rows included and the `BCF` row governing the BCF arm, where a hardcoded `CLASH` key leaves five of six rows unread. `IDS`'s spec path is admitted at PARSE and never reaches the grade as a path: `run` and `run_async` both take an `Option[IdsResolved]` a caller who resolved against a registry hands in, and an unresolved URI-bearing document refuses at the sync entry rather than grading. `CLASH`/`BCF` sides arrive pre-split and validate under one polymorphic batch parse aborting on the first malformed member, an empty query defaulting both sides to whole-model mode. `BCF` is the composition apex, re-running the clash leg and stacking overlaps into `bcf` topics with viewpoints AND the OCC-rendered `get_viewpoint_snapshot` image bound per topic, never a same-string round-trip.
- Auto: IDS runs as three NAMED steps — `parse` mints the document beside its typed `pending` roster off bytes that reached no network, `resolve` is the one async leg expanding those URIs over a built `TransportResource` and minting `IdsResolved`, and `_validate` accepts that type alone — so a `Classification`/`Property`/`Material` facet carrying a bSDD `uri` and an unexpanded value can no longer grade a narrower applicable set than its author declared and report the narrower verdict clean. `ifctester` depends on `ifcopenshell` and `xmlschema` alone and ships no HTTP client, which is exactly why the expansion is this owner's step and not the package's. IDS reads BOTH verdict depths off one validation pass — per-spec entity ratio and the `Json(ids).report()` roll-up, `percent_checks_pass` on the `Results` ROOT and `total_applicable_*` totals on the `ResultsSpecification` rows, never conflated — excluding a `status is None` not-applicable spec whose `0.0` row poisons the evidence mean. Space-program validation measures through the package that owns measurement: `ifc5d.qto.quantify` folds the shared `RuleSet` base-quantity table over the space set and the grade reads the `Qto_SpaceBaseQuantities.NetFloorArea` cell off the returned `ResultsDict`, so no local `get_psets(qtos_only=True)` key fold and no unit-scale square survive — the take-off already converts to the model's declared project units, the ONE regime the program table is read in. Two exclusions are distinct and both named: a space absent from the program table carries no program signal and is excluded from grading, while a space the take-off could not measure is a TYPED REFUSAL on its own `unquantified` axis, never a `0.0` area grading as total non-compliance.
- Output: `AnalysisResult` carries the kind-specific census and typed rows; `frame` projects those rows through `EvidenceFrame` using a specification-derived key. `_distributed` records the `non-compliant` fraction at the producing fold.
- Law: the BCF arm alone lands durable evidence on the `python:runtime/observability/journal#LEDGER` plane — one `OPERATIONAL` `AuditFact` keyed on the run's `spec`, its topic GUIDs the subject index — because that arm AUTHORS an archive the data seam persists while every other kind reads the model and produces a verdict nothing keeps. `run_async` is its seat, the awaitable twin this caller-floor owner mints over the band hop, since recording suspends and `_distributed` is a synchronous charter projection. No meter rides the leg: the archive crosses as the result's `product` and the data seam charges the write it performs, so a byte count at both tiers bills one artifact twice.
- Law: `GeoreferenceFact` decodes at this band as the branch's one model CRS source, while a dataset's own file CRS remains data-owned. The decode sits beside the dispatch: it produces no `AnalysisRow` or charter measure because a coordinate frame is not a compliance verdict. The fact crosses one-way to `python:data/spatial`, whose eight-field decoder mirrors this producer's roster arm-for-arm. Typed absence answers an ungeoreferenced model.
- Packages: `ifcopenshell` (`util.element` the pset and quantity reads, `util.geolocation` the ONE georeference extraction seam whose `get_helmert_transformation_parameters` collapses `IfcMapConversion`, `IfcMapConversionScaled`, `IfcRigidOperation`, and the IFC2X3 ePSet fallback onto one nine-field transform, so no consumer branches on coordinate-operation subtype), `ifctester`, `ifcclash`, `bcf-client`, and `ifc5d` (`qto.quantify` the space base-quantity take-off, its `rules` key arriving as the sibling lifecycle owner's `RuleSet` vocabulary rather than a second transcription) per the fence imports; runtime transport (`TransportResource.acquire` the ONE registry acquisition, arriving BUILT so this page mints no client, no cache posture, and no egress policy) and runtime rails (`FaultRow`/`RAISES` the raise coordinates, `boundary` under a named `catch` set, `guarded_sync` bound to the clash row); `IfcSelector` is the only `filter_elements` caller.
- Growth: a new verb is one `AnalysisKind` row, one `of_*` constructor and one `facts` arm, one dispatch arm, one `QUERY_SPLIT` row, and one `evidence` key — the frame column set following from `facts` with no edit here, and a durable trail only where the verb WRITES something, as one `_evidence` arm; a new selection axis is one `IfcSelector` grammar alternative, never a local query-parse fold; a new quantity axis on the space grade is one `SPACE_AREA` cell row; a newly decoded georeference axis is one `GeoreferenceFact` field landed at BOTH ends of the seam in the same pass, a producer-only field being a wire key the data decoder rejects; a newly resolvable registry axis is one `uri`-bearing facet the `parse` census already sees and one `RegistryTerms` shape the composition's endpoint answers, zero page edits; IDS document AUTHORING (`Ids(...)`/facet family/`to_xml`) is the named next verb once a consumer supplies a rule vocabulary — one row, never a second engine.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy; no ledger, custody, or retention window minted here, the plane arriving bound at the composition root and this owner declaring a `Retain` class alone; no durable store — topic rows and the archive bytes are the wire carry, the durable `.bcfzip`, IDS-report, and frame writes the data seam's; no Rhino/GH mutation; no raw `query` string threaded past admission into `filter_elements`; no hand-rolled quantity-key fold where `ifc5d.qto` owns measurement; no per-row fact map on the result where the frame is the columnar carrier; no second model CRS decode and no CRS guess anywhere, the georeference reading through `util.geolocation` alone and the map transform composing on the data side's `reproject` prelude, never here; no HTTP client, cache posture, or egress policy minted for the registry leg, the `TransportResource` arriving built from the composition exactly as the sibling lifecycle owner's lane does.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from enum import StrEnum

from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Final, Literal, NotRequired, TypedDict, assert_never

from beartype import beartype
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.json import decode

lazy import ifc5d.qto
lazy import ifcopenshell
lazy from bcf.v3.bcfxml import BcfXml
lazy from ifcclash.ifcclash import Clasher, ClashSet, ClashSettings, ClashSource
lazy from ifcopenshell.util.element import get_psets
lazy from ifcopenshell.util.geolocation import HelmertTransformation, get_crs, get_helmert_transformation_parameters, get_true_north
lazy from ifctester.facet import Facet, Restriction
lazy from ifctester.ids import Ids, IdsXmlValidationError
lazy from ifctester.ids import open as open_ids
lazy from ifctester.reporter import Json

from rasm.geometry.graduation import (
    EvidenceFrame,
    EvidenceScope,
    GeometryLeg,
    GeometrySubject,
    charter_record,
    evidence_key,
    evidence_run,
)
from rasm.geometry.ifc.costing import RuleSet
from rasm.geometry.ifc.selector import GeoDrop, IfcFault, IfcSelector
from rasm.runtime.faults import (
    FAULT_CONF,
    PACKAGE,
    TERMINAL,
    TRANSIENT,
    BoundaryFault,
    Catch,
    Disposition,
    FaultRow,
    RuntimeRail,
    boundary,
    rostered,
    traversed,
)
from rasm.runtime.journal import Actor, Assigned, AuditFact, Fact, Journal, Party, Retain
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.resilience import RetryClass, guarded_sync
from rasm.runtime.roots import Acquired, TransportResource

# --- [TYPES] ----------------------------------------------------------------------------


class AnalysisKind(StrEnum):
    QUANTITY = "quantity"
    PSET = "pset"
    IDS = "ids"
    CLASH = "clash"
    SPACE_PROGRAM = "space-program"
    BCF = "bcf"


class ClashRow(TypedDict):
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
    compliance: tuple[str, float, int] = case()
    clash: tuple[str, str, float, int] = case()
    topic: tuple[str, str, str, bool] = case()

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


# --- [CONSTANTS] ------------------------------------------------------------------------

ANALYSIS_SUBJECT: Final[GeometrySubject] = GeometrySubject.BIM_COMPLIANCE

OWNER: Final[str] = f"{PACKAGE}.{GeometryLeg.ANALYSIS.value}"

QUERY_SPLIT: Final[Map[AnalysisKind, str]] = Map.of_seq([
    (AnalysisKind.QUANTITY, ""),
    (AnalysisKind.PSET, ""),
    (AnalysisKind.IDS, ""),
    (AnalysisKind.SPACE_PROGRAM, ""),
    (AnalysisKind.CLASH, "#"),
    (AnalysisKind.BCF, "#"),
])

SPACE_AREA: Final[tuple[str, str]] = ("Qto_SpaceBaseQuantities", "NetFloorArea")

# --- [MODELS] ---------------------------------------------------------------------------


class AnalysisResult(Struct, frozen=True, gc=False):
    kind: AnalysisKind
    spec: str
    subjects: tuple[str, ...]
    rows: tuple[AnalysisRow, ...]
    checks_pass: float | None = None
    resolved: int = 0
    unquantified: tuple[str, ...] = ()
    product: bytes = b""

    def evidence(self) -> dict[str, float]:
        match self.kind:
            case AnalysisKind.IDS:
                ratios = tuple(r.compliance[1] for r in self.rows if r.tag == "compliance")
                entity_fail = 1.0 - sum(ratios) / len(ratios) if ratios else 0.0
                check_fail = 1.0 - self.checks_pass / 100.0 if self.checks_pass is not None else 0.0
                return {"non-compliant": entity_fail, "check-fail": check_fail}
            case AnalysisKind.SPACE_PROGRAM:
                ratios = tuple(r.compliance[1] for r in self.rows if r.tag == "compliance")
                return {"non-compliant": 1.0 - sum(ratios) / len(ratios) if ratios else 0.0, "unquantified": float(len(self.unquantified))}
            case AnalysisKind.CLASH:
                clusters = {r.clash[3] for r in self.rows if r.tag == "clash"}
                return {"clash-clusters": float(len(clusters))}
            case AnalysisKind.QUANTITY | AnalysisKind.PSET | AnalysisKind.BCF:
                return {"empty": 0.0 if self.rows else 1.0}
            case unreachable:
                assert_never(unreachable)

    def frame(self) -> "RuntimeRail[EvidenceFrame]":
        names = tuple(self.rows[0].facts) if self.rows else ()
        table: dict[str, list[object]] = {
            "kind": [self.kind.value] * len(self.rows),
            **{name: [row.facts[name] for row in self.rows] for name in names},
        }
        return EvidenceFrame.of(ANALYSIS_SUBJECT, evidence_key(ANALYSIS_SUBJECT, self.spec), table)


class PendingFacet(Struct, frozen=True, gc=False):
    specification: str
    clause: Literal["applicability", "requirement"]
    facet: str
    uri: str


class RegistryTerms(Struct, frozen=True, gc=False):
    values: tuple[str, ...] = ()


class IdsDocument(Struct, frozen=True, gc=False):
    document: "Ids"
    pending: "Block[PendingFacet]"

    @property
    def settled(self) -> "Option[IdsResolved]":
        return Some(IdsResolved(document=self.document, resolved=0)) if self.pending.is_empty() else Nothing


class IdsResolved(Struct, frozen=True, gc=False):
    document: "Ids"
    resolved: int


class GeoreferenceFact(Struct, frozen=True):
    crs: str
    eastings: float
    northings: float
    orthogonal_height: float
    x_axis_abscissa: float
    x_axis_ordinate: float
    scale: float
    true_north: float | None


# --- [ERRORS] ---------------------------------------------------------------------------

ANALYSIS_REFUSED: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.ANALYSIS, point="analysis", arm="boundary", defect="analysis-refused", retriability=TERMINAL
)
IDS_PARSE: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.ANALYSIS, point="ids.parse", arm="boundary", defect="ids-unparsed", retriability=TERMINAL
)
IDS_REGISTRY: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.ANALYSIS, point="ids.registry", arm="resource", defect="terms-undecoded", retriability=TRANSIENT
)
GEOREFERENCE: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.ANALYSIS, point="georeference", arm="boundary", defect="georeference-unread", retriability=TERMINAL
)
CLASH_TREE: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.ANALYSIS, point="clash.tree", arm="boundary", defect="occ-tree", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([ANALYSIS_REFUSED, IDS_PARSE, IDS_REGISTRY, GEOREFERENCE, CLASH_TREE]))

_GEO_RAISES: Final[Catch] = (AttributeError, IndexError, KeyError, TypeError, ValueError)


def _domain(fault: IfcFault) -> BoundaryFault:
    return BoundaryFault.of(ANALYSIS_REFUSED, fault)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _archived(document: "BcfXml") -> bytes:
    with TemporaryDirectory(prefix="ifc-bcf-") as work:
        path = Path(work, "topics.bcfzip")
        document.save(path)
        return path.read_bytes()


def _distributed(result: AnalysisResult, composition: ScopeKey) -> AnalysisResult:
    charter_record(ANALYSIS_SUBJECT, result.evidence(), composition=composition)
    return result


def _evidence(result: AnalysisResult) -> "Block[Fact]":
    if result.kind is not AnalysisKind.BCF:
        return Block.empty()
    return Block.singleton(
        AuditFact(
            action=f"geometry.{result.kind.value}",
            actor=Party(kind=Actor.SERVICE, key=OWNER),
            target=Party(kind="model", key=result.spec),
            retention=Retain.OPERATIONAL,
            change=(Assigned(path="/topics", next=str(len(result.rows))), Assigned(path="/archive", next=str(len(result.product)))),
            subjects=result.subjects,
        )
    )


def _restricted(parsed: IdsDocument, payloads: "Block[Acquired]") -> "RuntimeRail[IdsResolved]":
    def written() -> IdsResolved:
        seated: "Map[tuple[str, str], Facet]" = Map.of_seq(
            ((spec.name, getattr(facet, "uri", None) or ""), facet)
            for spec in parsed.document.specifications
            for facet in (*spec.applicability, *spec.requirements)
        )
        for pending, payload in zip(parsed.pending, payloads, strict=True):
            terms = decode(bytes(payload), type=RegistryTerms)
            seated[(pending.specification, pending.uri)].value = Restriction(options=list(terms.values), base="string")
        return IdsResolved(document=parsed.document, resolved=len(payloads))

    return boundary(IDS_REGISTRY, written, catch=(AttributeError, KeyError, TypeError, ValueError))


def _georeferenced(helmert: "HelmertTransformation | None", crs: str, true_north: float | None) -> "RuntimeRail[Option[GeoreferenceFact]]":
    if helmert is None:
        return Ok(Nothing)
    factors = (helmert.factor_x, helmert.factor_y, helmert.factor_z)
    unwirable = (
        *(((GeoDrop.NON_UNIFORM_FACTORS, ",".join(map(str, factors))),) if len(set(factors)) > 1 else ()),
        *(((GeoDrop.UNNAMED_CRS, ""),) if not crs else ()),
    )
    return (
        Error(_domain(IfcFault(unwirable_georeference=unwirable)))
        if unwirable
        else Ok(
            Some(
                GeoreferenceFact(
                    crs=crs,
                    eastings=helmert.e,
                    northings=helmert.n,
                    orthogonal_height=helmert.h,
                    x_axis_abscissa=helmert.xaa,
                    x_axis_ordinate=helmert.xao,
                    scale=helmert.scale * factors[0],
                    true_north=true_north,
                )
            )
        )
    )


class IfcAnalysis:
    @staticmethod
    def run(
        model: "ifcopenshell.file",
        kind: AnalysisKind,
        query: str,
        *,
        resolved: "Option[IdsResolved]" = Nothing,
        composition: ScopeKey = DEFAULT_SCOPE,
    ) -> "RuntimeRail[AnalysisResult]":
        return evidence_run(
            EvidenceScope.IFC_ANALYSIS,
            f"run.{kind}",
            lambda: IfcAnalysis._dispatch(model, kind, query, resolved).map(lambda result: _distributed(result, composition)),
            composition=composition,
        )

    @staticmethod
    async def run_async(
        model: "ifcopenshell.file",
        kind: AnalysisKind,
        query: str,
        *,
        resolved: "Option[IdsResolved]" = Nothing,
        composition: ScopeKey = DEFAULT_SCOPE,
    ) -> "RuntimeRail[AnalysisResult]":
        match IfcAnalysis.run(model, kind, query, resolved=resolved, composition=composition):
            case Result(tag="ok", ok=result):
                return (await Journal.record(_evidence(result), scope=composition)).map(lambda _landed: result)
            case refused:
                return Error(refused.error)

    @staticmethod
    def parse(spec_path: str) -> "RuntimeRail[IdsDocument]":
        def read() -> IdsDocument:
            document = open_ids(spec_path, validate=True)
            pending = Block.of_seq(
                PendingFacet(specification=spec.name, clause=clause, facet=type(facet).__name__, uri=uri)
                for spec in document.specifications
                for clause, facets in (("applicability", spec.applicability), ("requirement", spec.requirements))
                for facet in facets
                for uri in (getattr(facet, "uri", None) or "",)
                if uri
            )
            return IdsDocument(document=document, pending=pending)

        return boundary(IDS_PARSE, read, catch=(IdsXmlValidationError, OSError, AttributeError, TypeError, ValueError))

    @staticmethod
    async def resolve(spec_path: str, registry: "TransportResource") -> "RuntimeRail[IdsResolved]":
        match IfcAnalysis.parse(spec_path):
            case Result(tag="ok", ok=parsed):
                acquired = Block.of_seq([await registry.acquire(facet.uri) for facet in parsed.pending])
                return traversed(acquired, by=Disposition.ACCUMULATE).bind(
                    lambda payloads: _restricted(parsed, payloads)
                )
            case refused:
                return Error(refused.error)

    @staticmethod
    def georeference(model: "ifcopenshell.file") -> "RuntimeRail[Option[GeoreferenceFact]]":
        return boundary(
            GEOREFERENCE,
            lambda: (
                get_helmert_transformation_parameters(model),
                str((get_crs(model) or {}).get("Name", "")),
                float(get_true_north(model))
                if any(context.TrueNorth for context in model.by_type("IfcGeometricRepresentationContext", include_subtypes=False))
                else None,
            ),
            catch=_GEO_RAISES,
        ).bind(lambda read: _georeferenced(*read))

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _dispatch(
        model: "ifcopenshell.file", kind: AnalysisKind, query: str, resolved: "Option[IdsResolved]" = Nothing
    ) -> "RuntimeRail[AnalysisResult]":
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
                return (
                    resolved.map(Ok)
                    .default_with(
                        lambda: IfcAnalysis._parse_settled(head, f"{kind.value}|{head}")
                    )
                    .map(lambda ready: IfcAnalysis._validate(model, ready, f"{kind.value}|{head}"))
                )
            case AnalysisKind.CLASH:
                return IfcAnalysis._clash_sides(head, tail).bind(lambda sides: IfcAnalysis._clash(model, sides, f"{kind.value}|{sides[0]}#{sides[1]}"))
            case AnalysisKind.BCF:
                return IfcAnalysis._clash_sides(head, tail).bind(lambda sides: IfcAnalysis._author(model, sides, f"{kind.value}|{sides[0]}#{sides[1]}"))
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _space(model: "ifcopenshell.file", table: str, spec: str) -> AnalysisResult:
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
        psets = get_psets(element, qtos_only=quantities)
        merged = {f"{name}.{key}": value for name, body in psets.items() for key, value in body.items()}
        if quantities:
            return AnalysisRow.of_quantity(element.GlobalId, {k: float(v) for k, v in merged.items() if isinstance(v, int | float)})
        return AnalysisRow.of_pset(element.GlobalId, merged)

    @staticmethod
    def _parse_settled(spec_path: str, subject: str) -> "RuntimeRail[IdsResolved]":
        return IfcAnalysis.parse(spec_path).bind(
            lambda parsed: parsed.settled.to_result(
                _domain(IfcFault(unresolved_slots=(subject, tuple(facet.uri for facet in parsed.pending))))
            )
        )

    @staticmethod
    def _validate(model: "ifcopenshell.file", ready: IdsResolved, spec: str) -> AnalysisResult:
        document = ready.document
        document.validate(model)
        rows = tuple(
            AnalysisRow.of_compliance(
                row.name, len(row.passed_entities) / max(len(row.passed_entities) + len(row.failed_entities), 1), len(row.failed_entities)
            )
            for row in document.specifications
            if row.status is not None
        )
        applicable = tuple(row.name for row in document.specifications if row.status is not None)
        results = Json(document).report()
        return AnalysisResult(
            AnalysisKind.IDS, spec, applicable, rows, checks_pass=float(results["percent_checks_pass"]), resolved=ready.resolved
        )

    @staticmethod
    def _clash_sides(head: str, tail: str) -> "RuntimeRail[tuple[str, str]]":
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
        def compose(run: "tuple[Clasher, tuple[ClashRow, ...], ClashSet]") -> AnalysisResult:
            clasher, clashes, clash_set = run
            document = BcfXml.create_new("rasm.ifc.analysis")
            rows = tuple(IfcAnalysis._topic(document, clasher, clash_set, c) for c in clashes)
            return AnalysisResult(AnalysisKind.BCF, spec, tuple(r.topic[0] for r in rows), rows, product=_archived(document))

        return IfcAnalysis._run_clash(model, sides).map(compose)

    @staticmethod
    def _topic(document: "BcfXml", clasher: "Clasher", clash_set: "ClashSet", collision: ClashRow) -> AnalysisRow:
        handler = document.add_topic(
            f"Clash {collision['a_global_id']} × {collision['b_global_id']}", f"penetration {collision['distance']:.4f}", "rasm", topic_type="Clash"
        )
        viewpoint = handler.add_viewpoint_from_point_and_guids(collision["p1"], collision["a_global_id"], collision["b_global_id"])
        snapshot = clasher.get_viewpoint_snapshot(clash_set, viewpoint)
        return AnalysisRow.of_topic(handler.guid, handler.topic.title, "clash", snapshot is not None)

    @staticmethod
    def _run_clash(model: "ifcopenshell.file", sides: tuple[str, str]) -> "RuntimeRail[tuple[Clasher, tuple[ClashRow, ...], ClashSet]]":
        def source(selector: str) -> ClashSource:
            base: ClashSource = {"file": "", "ifc": model}
            return {**base, "selector": selector, "mode": "e"} if selector else {**base, "mode": "a"}

        def solve() -> tuple[Clasher, tuple[ClashRow, ...], ClashSet]:
            clasher = Clasher(ClashSettings())
            clash_set: ClashSet = {"name": "ifc.clash", "a": [source(sides[0])], "b": [source(sides[1])], "mode": "intersection", "tolerance": 0.001}
            clasher.clash_sets = [clash_set]
            clasher.clash()
            clasher.smart_group_clashes(clasher.clash_sets, max_clustering_distance=1.0)
            clashes: tuple[ClashRow, ...] = tuple(clash_set.get("clashes", {}).values())
            clusters = {key: index for index, key in enumerate(clash_set.get("clash_groups", {}))}
            return clasher, tuple({**c, "cluster": clusters.get(c.get("group", ""), 0)} for c in clashes), clash_set

        return guarded_sync(RetryClass.OCC_NATIVE, solve, at=CLASH_TREE)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
