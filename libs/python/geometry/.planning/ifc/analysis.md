# [PY_GEOMETRY_IFC_ANALYSIS]

IFC property, quantity, and relationship analysis and standards-conformant validation — AEC verbs the tessellation hop drops: quantity takeoff, Pset queries, IDS model-checking, clash detection, space-program validation, and BCF issue authoring over `ifcopenshell.util`, `ifctester`, `ifcclash`, `ifc5d.qto`, and `bcf`, every provider output folded into one `AnalysisRow` algebra. The model's own map georeference decodes here beside them as the branch's ONE model CRS source, crossing as a `GeoreferenceFact` wire shape rather than an analysis row, because a coordinate frame carries no verdict to grade. C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the analysis verbs and buildingSMART validation output that projection never produces.

Every selecting verb admits its query through `IfcSelector` (`ifc/selector#SELECTOR`), so a malformed selector faults typed at the boundary, never a silent empty `filter_elements` match three arms deep, and the `SelectorMatch` it hands back carries the canonical `filter_string` every receipt keys its evidence on. `run` threads the graduation `evidence_run` weave under `EvidenceScope.IFC_ANALYSIS` and carries no head decorator — `@beartype(conf=FAULT_CONF)` on `_dispatch` is the one innermost seam this capsule fences, as its two peers hold. OCC clash trees are this owner's one transient-native boundary, riding the runtime-pinned `guarded_sync(RetryClass.OCC_NATIVE)` row, never a bare `stamina.retry` mint; the authored `.bcfzip` rides home as archive BYTES on the receipt and the durable write stays `python:data/spatial`'s. Evidence graduates under `GeometrySubject.BIM_COMPLIANCE`, the differentiated member the IDS/clash/BCF verdict class owns, distinct from the section-integral and lifecycle members their owners bind.

## [01]-[INDEX]

- [02]-[ANALYSIS]: analysis verbs over one `AnalysisRow` algebra, the `IfcSelector` gate, the IDS parse/resolve/evaluate split gating the grade on `IdsResolved`, the `OCC_NATIVE` retry row, the columnar row egress, the `run_async` twin recording the BCF arm's operational trail, kind-specific graduation evidence under `BIM_COMPLIANCE`, and the `GeoreferenceFact` decode crossing beside the dispatch as the model CRS seam.

## [02]-[ANALYSIS]

- Owner: `IfcAnalysis` dispatches verbs through one rail-returning `_dispatch` fold; `AnalysisRow` is the one carrier whose case IS the shape — one algebra, never a per-verb row dialect; graduation subject rides the `ANALYSIS_SUBJECT` module constant, never a per-receipt `subject: str` field racing the discriminant.
- Entry: `run` takes the model, the kind, the free-form `query`, and the `composition` custody key. `query` meaning is kind-fixed through the `QUERY_SPLIT` table — pure selector, `a#b` side pair, or spec path/JSON table — read ONCE at the `_dispatch` head so every row governs the arm it keys, the four empty-delimiter rows included and the `BCF` row governing the BCF arm, where a hardcoded `CLASH` key leaves five of six rows unread. `IDS`'s spec path is admitted at PARSE and never reaches the grade as a path: `run` and `run_async` both take an `Option[IdsResolved]` a caller who resolved against a registry hands in, and an unresolved URI-bearing document refuses at the sync entry rather than grading. `CLASH`/`BCF` sides arrive pre-split and validate under one polymorphic batch parse aborting on the first malformed member, an empty query defaulting both sides to whole-model mode. `BCF` is the composition apex, re-running the clash leg and stacking overlaps into `bcf` topics with viewpoints AND the OCC-rendered `get_viewpoint_snapshot` image bound per topic, never a same-string round-trip.
- Auto: IDS runs as three NAMED steps — `parse` mints the document beside its typed `pending` roster off bytes that reached no network, `resolve` is the one async leg expanding those URIs over a built `TransportResource` and minting `IdsResolved`, and `_validate` accepts that type alone — so a `Classification`/`Property`/`Material` facet carrying a bSDD `uri` and an unexpanded value can no longer grade a narrower applicable set than its author declared and report the narrower verdict clean. `ifctester` depends on `ifcopenshell` and `xmlschema` alone and ships no HTTP client, which is exactly why the expansion is this owner's step and not the package's. IDS reads BOTH verdict depths off one validation pass — per-spec entity ratio and the `Json(ids).report()` roll-up, `percent_checks_pass` on the `Results` ROOT and `total_applicable_*` totals on the `ResultsSpecification` rows, never conflated — excluding a `status is None` not-applicable spec whose `0.0` row poisons the evidence mean. Space-program validation measures through the package that owns measurement: `ifc5d.qto.quantify` folds the shared `RuleSet` base-quantity table over the space set and the grade reads the `Qto_SpaceBaseQuantities.NetFloorArea` cell off the returned `ResultsDict`, so no local `get_psets(qtos_only=True)` key fold and no unit-scale square survive — the take-off already converts to the model's declared project units, the ONE regime the program table is read in. Two exclusions are distinct and both named: a space absent from the program table carries no program signal and is excluded from grading, while a space the take-off could not measure is a TYPED REFUSAL on its own `unquantified` axis, never a `0.0` area grading as total non-compliance.
- Receipt: receipts carry the census, frames carry the rows. `contribute` emits one row per run — row count, subject count, archive extent where one exists, and the residual ledger — because a whole-model take-off is one fact key per element per quantity and a flattened row stream turns the runtime receipt into a hundred-thousand-key dict per run; `frame` projects those rows as ONE columnar `EvidenceFrame` through the graduation port, columns off the first row's `facts` keys exactly as the sibling lifecycle rollup does. `evidence` ledger is kind-specific, never a row count — IDS keys per-entity AND per-check failing fractions, `SPACE_PROGRAM` the per-space failing fraction beside its unquantified count, `CLASH` the unresolved-cluster count, takeoff/BCF the empty-result fraction — so a model breaching its ceiling fails the carrier's `admitted` verdict rather than crossing clean. `graduates` and `frame` derive their own `ContentKey` from the receipt's `spec` — kind plus the validated query projection — through the spine's `evidence_key`, so no caller mints a key for evidence it did not produce. `_distributed` records the `non-compliant` fraction as the `rasm.geometry.compliance.noncompliant` charter measure at the producing fold; the cluster-count and empty-fraction keys name no charter row and record nothing, a count or boolean gate earning no histogram.
- Law: the BCF arm alone lands durable evidence on the `python:runtime/observability/journal#LEDGER` plane — one `OPERATIONAL` `AuditFact` keyed on the run's `spec`, its topic GUIDs the subject index — because that arm AUTHORS an archive the data seam persists while every other kind reads the model and produces a verdict nothing keeps. `run_async` is its seat, the awaitable twin this caller-floor owner mints over the band hop, since recording suspends and `_distributed` is a synchronous charter projection. No meter rides the leg: the archive crosses as receipt payload and the data seam charges the write it performs, so a byte count at both tiers bills one artifact twice.
- Law: `GeoreferenceFact` decodes at this band as the branch's ONE model CRS source, a dataset's own file CRS staying a data-owned origin beside it that this fact never overrides. The decode sits BESIDE the dispatch rather than inside it: it produces no `AnalysisRow`, grades no ceiling, graduates nothing, and records no charter measure — `BIM_COMPLIANCE` is the verdict class the IDS/clash/BCF arms own and a coordinate frame is not a verdict — so an `AnalysisKind` row would seat it in an algebra with nothing to say about it. The decoded fact IS the evidence: it crosses one-way to `python:data/spatial`'s geospatial plane as the `[SHAPE]: GeoreferenceFact` seam, whose eight-field roster this producer DECLARES and the data-side decoder mirrors arm-for-arm, so a second decode anywhere in the estate is the deleted form. All eight fields are REQUIRED — an identity abscissa/ordinate pair, a unit scale, and a `None` north are the fabricated values a partial decode would publish as read facts, and typed absence already answers the ungeoreferenced model.
- Packages: `ifcopenshell` (`util.element` the pset and quantity reads, `util.geolocation` the ONE georeference extraction seam whose `get_helmert_transformation_parameters` collapses `IfcMapConversion`, `IfcMapConversionScaled`, `IfcRigidOperation`, and the IFC2X3 ePSet fallback onto one nine-field transform, so no consumer branches on coordinate-operation subtype), `ifctester`, `ifcclash`, `bcf-client`, and `ifc5d` (`qto.quantify` the space base-quantity take-off, its `rules` key arriving as the sibling lifecycle owner's `RuleSet` vocabulary rather than a second transcription) per the fence imports; runtime transport (`TransportResource.acquire` the ONE registry acquisition, arriving BUILT so this page mints no client, no cache posture, and no egress policy) and runtime rails (`FaultRow`/`RAISES` the raise coordinates, `boundary` under a named `catch` set, `guarded_sync` bound to the clash row); `IfcSelector` is the only `filter_elements` caller.
- Growth: a new verb is one `AnalysisKind` row, one `of_*` constructor and one `facts` arm, one dispatch arm, one `QUERY_SPLIT` row, and one `evidence` key — the frame column set following from `facts` with no edit here, and a durable trail only where the verb WRITES something, as one `_evidence` arm; a new selection axis is one `IfcSelector` grammar alternative, never a local query-parse fold; a new quantity axis on the space grade is one `SPACE_AREA` cell row; a newly decoded georeference axis is one `GeoreferenceFact` field landed at BOTH ends of the seam in the same pass, a producer-only field being a wire key the data decoder rejects; a newly resolvable registry axis is one `uri`-bearing facet the `parse` census already sees and one `RegistryTerms` shape the composition's endpoint answers, zero page edits; IDS document AUTHORING (`Ids(...)`/facet family/`to_xml`) is the named next verb once a consumer supplies a rule vocabulary — one row, never a second engine.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy; no ledger, custody, or retention window minted here, the plane arriving bound at the composition root and this owner declaring a `Retain` class alone; no durable store — topic rows and the archive bytes are the wire carry, the durable `.bcfzip`, IDS-report, and frame writes the data seam's; no Rhino/GH mutation; no raw `query` string threaded past admission into `filter_elements`; no hand-rolled quantity-key fold where `ifc5d.qto` owns measurement; no per-row fact map on the receipt stream where the frame is the columnar carrier; no second model CRS decode and no CRS guess anywhere, the georeference reading through `util.geolocation` alone and the map transform composing on the data side's `reproject` prelude, never here; no HTTP client, cache posture, or egress policy minted for the registry leg, the `TransportResource` arriving built from the composition exactly as the sibling lifecycle owner's lane does.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from enum import StrEnum
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Final, Literal, NotRequired, TypedDict, assert_never

from beartype import beartype
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.json import decode

# Provider band, deferred at module scope: the manifest roster bans the EAGER form alone, and every dereference sits
# inside a verb body, so no constant or table row reifies a proxy at import. The `ifcopenshell.util` members and the
# `ifctester` reporters ride `lazy from`, binding each consumed name directly — sibling `lazy import <pkg>.<mod>`
# lines reify independently off `sys.lazy_modules`; `open as open_ids` keeps the builtin unshadowed.
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
    EVIDENCE_DOMAIN,
    EvidenceFrame,
    EvidenceScope,
    GeometryHandoff,
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
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.resilience import RetryClass, guarded_sync
from rasm.runtime.roots import Acquired, TransportResource

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

# this owner's one name, DERIVED off the leg roster member every raise on this page seats under, so the receipt
# stream, the durable audit actor, and the fault subject cannot drift apart under three transcribed spellings.
OWNER: Final[str] = f"{PACKAGE}.{GeometryLeg.ANALYSIS.value}"

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
    # IDS facets a registry expanded before grading: an audit that resolved NONE against a URI-bearing spec graded a
    # narrower applicable set than its author declared, and this is the column that makes that visible.
    resolved: int = 0
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
                | ({"resolved": self.resolved} if self.resolved else {})
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


class PendingFacet(Struct, frozen=True, gc=False):
    # ONE unresolved registry axis with every anchor an operator acts on: which specification carries it, which of the
    # two clauses it sits in, which facet class declares it, and the URI nothing dereferenced. `ifctester` depends on
    # `ifcopenshell` and `xmlschema` alone and ships no HTTP client, so a `Classification`/`Property`/`Material` facet
    # carrying a `uri` and an unexpanded value is a real axis the parse can see and the validation cannot close.
    specification: str
    clause: Literal["applicability", "requirement"]
    facet: str
    uri: str


class RegistryTerms(Struct, frozen=True, gc=False):
    # THIS owner's declared contract for a registry destination: an expansion answers the enumerated terms its URI
    # names, and the composition that admits the endpoint is what maps a dictionary's own payload onto this shape.
    # Declaring it here keeps the decode a closed local type rather than a dict the resolve leg re-parses per facet.
    values: tuple[str, ...] = ()


class IdsDocument(Struct, frozen=True, gc=False):
    # what PARSE mints, synchronously, off bytes that reached no network: the `ifctester` handle beside the TYPED
    # unresolved axis. An empty `pending` is the already-resolved document, so the resolved case is a VALUE `settled`
    # projects rather than a flag a caller remembers to test.
    document: "Ids"
    pending: "Block[PendingFacet]"

    @property
    def settled(self) -> "Option[IdsResolved]":
        return Some(IdsResolved(document=self.document, resolved=0)) if self.pending.is_empty() else Nothing


class IdsResolved(Struct, frozen=True, gc=False):
    # the distinct NOMINAL type one `resolve` mints, never a copy of `IdsDocument` with a cleared roster: a separate
    # type is what makes an unresolved evaluation unrepresentable rather than caller-disciplined, and `resolved`
    # carries how many facets a registry actually expanded so the receipt can say the audit ran against live terms.
    document: "Ids"
    resolved: int


class GeoreferenceFact(Struct, frozen=True):
    # ONE wire fact DECLARES the model's map georeference here, mirrored arm-for-arm by the data geospatial decoder:
    # `crs` off the projected-CRS name, the six transform fields off the one `HelmertTransformation` — `scale`
    # carrying the provider's `scale * factor` product, the lossless single-scale spelling of a uniform per-axis
    # triple — and `true_north` off the context's own declaration. Transform fields are the similarity the data seam
    # composes — the abscissa/ordinate pair is a DIRECTION its consumer normalizes, so an authoring tool's
    # unnormalized magnitude crosses unedited rather than being pre-scaled into a second producer's job.
    # `true_north` rides as DECLARED evidence and never enters that transform, the map conversion having already
    # oriented the eastings axis.
    # No field carries a default: an identity abscissa/ordinate pair, a unit scale, and a `None` north are exactly
    # the fabricated values a partial decode would publish as read facts, and this producer already answers typed
    # ABSENCE for a model carrying no coordinate operation at all — so every one of the eight is REQUIRED and the
    # decoder that mirrors this roster cannot admit a wire missing one under a value nobody measured.
    crs: str
    eastings: float
    northings: float
    orthogonal_height: float
    x_axis_abscissa: float
    x_axis_ordinate: float
    scale: float
    true_north: float | None


# --- [ERRORS] --------------------------------------------------------------------------

# Every domain refusal this module mints is an `IfcFault` CASE, so these rows spend ONE coordinate per raise POINT and
# no fence spells a subject string. `IDS_PARSE` and `GEOREFERENCE` are TERMINAL — an XSD failure and a malformed
# coordinate operation refuse identically on every re-read — while `IDS_REGISTRY` and `CLASH_TREE` declare TRANSIENT:
# a dictionary host that did not answer and an OCC tree that died mid-build are both dependencies a re-issue may
# clear, and `CLASH_TREE` is the row the runtime-pinned `OCC_NATIVE` envelope re-offers against.
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

# `util.geolocation` is pure Python over the coordinate-operation entities, so its raise set is the builtin one those
# attribute walks surface; `open_ids` parses through `xmlschema` and wraps its failure in the package's own error,
# which leads the set because it is the precise class. Neither admits a bare `Exception`.
_GEO_RAISES: Final[Catch] = (AttributeError, IndexError, KeyError, TypeError, ValueError)


def _domain(fault: IfcFault) -> BoundaryFault:
    # ONE door for every domain refusal this module mints, and the ONE site binding the raise row. The band's typed
    # token rides the runtime's own `domain` case WHOLE — `BoundaryFault.of` admits a `Tagged` token ahead of every
    # `CLASSIFY` row — so case and coordinate cross the funnel as structured evidence rather than a rendered cause a
    # consumer re-parses; the render the wire edge reads stays `IfcFault.__str__` at the family owner.
    return BoundaryFault.of(ANALYSIS_REFUSED, fault)


# --- [OPERATIONS] ----------------------------------------------------------------------


def _archived(document: "BcfXml") -> bytes:
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


def _restricted(parsed: IdsDocument, payloads: "Block[Acquired]") -> "RuntimeRail[IdsResolved]":
    # the ONE write-back: each acquisition decodes to this owner's declared `RegistryTerms` and becomes the facet's
    # own enumeration restriction, so the applicable set the grade runs against is the one the spec's author declared
    # rather than the narrower one an unexpanded `uri` left behind. The whole-delivery arm answers a chunk, never an
    # iterator, so the decode reads bytes directly; the roster and the payload block are index-aligned by the fold
    # that produced them, and a payload the destination shaped differently refuses HERE with its own row.
    def written() -> IdsResolved:
        # the facets index ONCE on the same `(specification, uri)` key the pending roster carries, so the write-back
        # is a keyed lookup per payload rather than a re-walk of every clause per facet.
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
    # ABSENCE, never a fabricated identity: `get_helmert_transformation_parameters` answers `None` for a model
    # carrying no coordinate operation at all, and every `auto_*` sibling folds that `None` into the identity
    # transform — which reports site-local engineering coordinates AS map coordinates the instant the data seam's
    # `reproject` prelude composes the fact. Typed absence crosses instead, and a refusal is equally the deleted
    # form: a model that is merely un-georeferenced is not a malformed one, and its analysis rows still stand.
    if helmert is None:
        return Ok(Nothing)
    # one refusal names EVERY fact the eight-field wire cannot carry. `IfcMapConversionScaled` publishes THREE
    # per-axis factors against the wire's single `scale`; the provider's own algebra multiplies `scale * factor_*`
    # on every axis — `xyz2enh`'s eastings/northings legs, the `z2e` height leg, and `local2global`'s diagonal
    # alike — so a UNIFORM triple folds losslessly into the one wire scale as `scale * factor`, and only a
    # non-uniform triple refuses BY NAME rather than crossing as a uniform similarity with two axes silently
    # dropped — the [DISCARDED_DISCRIMINANT] scar exactly, and one no downstream `pyproj` call can detect. The
    # factors decode straight off the coordinate operation's literal attributes, so uniformity is exact equality,
    # never an epsilon a near-uniform triple would silently collapse through. A coordinate operation whose target
    # CRS names nothing fills no `crs` a consumer resolves, so it refuses on the same roster rather than crossing
    # as an empty sentinel.
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
        # weave flatten absorbs the rail-returning `_dispatch`, so a selector parse fault meets the converted provider
        # fault on one carrier; graduation stays the caller's own step on the returned receipt. The head carries no
        # decorator — `_dispatch` is the one innermost untrusted seam the capsule fences, as its two peers hold.
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
        # the awaitable twin over the band hop: every verb runs caller-floor on a live pybind11 handle, so `run` is
        # synchronous whole and recording — which SUSPENDS — cannot land inside it or inside `_distributed`, the sync
        # charter projection this leg pairs with. The twin runs the same fold and records the BCF arm's authored
        # issue set past it; every other kind mints an empty block and this leg costs one map read. The record rail
        # BINDS into the verdict, so an armed plane refusing an issue-log fact reaches the caller that owns it.
        match IfcAnalysis.run(model, kind, query, resolved=resolved, composition=composition):
            case Result(tag="ok", ok=result):
                return (await Journal.record(_evidence(result), scope=composition)).map(lambda _landed: result)
            case refused:
                return Error(refused.error)

    @staticmethod
    def parse(spec_path: str) -> "RuntimeRail[IdsDocument]":
        # PARSE is synchronous and reaches no network by construction — `ifctester` depends on `ifcopenshell` and
        # `xmlschema` alone — so this step answers the document beside the TYPED roster of facets whose `uri` nothing
        # dereferenced. `validate=True` is what makes the XSD failure the package's own `IdsXmlValidationError`
        # instead of a schema error surfacing three frames deeper, and the census walks both clauses of every
        # specification, so the clause a facet sits in reaches the operator rather than being inferred from position.
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
        # RESOLVE is this owner's ONE network step and takes a BUILT `TransportResource`, so no page here mints a
        # handle — the same law the sibling lifecycle owner holds taking a built `LanePolicy`. Every pending facet
        # acquires under `ACCUMULATE`, so ONE refusal names every unresolvable URI rather than costing one run per
        # registry miss, and the expanded terms land on the facet's own `Restriction(options=..., base=...)` slot
        # through the provider's own constructor rather than a value this page formats. A spec carrying no `uri`
        # parses to an empty roster and this leg is a TOTAL no-op issuing not one request, so the common case pays
        # nothing for the gate that makes an unresolved evaluation unrepresentable.
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
        # `georeference` is the branch's ONE model CRS source, and a SHORT PURE DECODE: `util.geolocation` is pure Python over the
        # coordinate-operation entities, so this needs neither the native wrapper nor a band hop and stays
        # caller-floor beside the dispatch. It grades nothing and graduates nothing — the decoded fact IS the
        # evidence, crossing one-way to the data geospatial plane — so it takes no kind, no `spec`, and no weave.
        # It IS reachable outside `run`, so the three provider reads ride the `boundary` fence and this projection
        # returns the rail rather than raising past a caller no weave covered; every wire decision is the pure fold
        # above, and `get_crs` is read for its `Name` alone, the one attribute a `pyproj` consumer resolves —
        # it answers `None` outright for a model declaring no CRS, which folds onto the same named `crs:unnamed`
        # refusal rather than surfacing as an attribute fault the fence can only classify blind. `get_true_north`
        # is TOTAL — an undeclared `TrueNorth` answers `0`, its bare-except floor included — so declaredness reads
        # off the same context roster the provider itself walks: a declared north converts to float, an undeclared
        # one crosses as `None` onto the wire's own absence arm, never a fabricated due-north `0.0` a solar
        # consumer would read as the model's declaration.
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
                # EVALUATE accepts `IdsResolved` ALONE, so an audit graded against an unexpanded registry axis is
                # unrepresentable rather than caller-disciplined. A caller holding a resolved document hands it in; a
                # caller handing a path parses here, and `settled` mints the resolved value for free when the document
                # carries no `uri` at all — the common case, which issues no request and pays nothing for the gate. A
                # URI-bearing spec reaching this SYNC entry unresolved refuses BY NAME, naming every pending URI,
                # instead of grading a narrower applicable set than its author declared and reporting it clean.
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
        psets = get_psets(element, qtos_only=quantities)
        merged = {f"{name}.{key}": value for name, body in psets.items() for key, value in body.items()}
        if quantities:
            return AnalysisRow.of_quantity(element.GlobalId, {k: float(v) for k, v in merged.items() if isinstance(v, int | float)})
        return AnalysisRow.of_pset(element.GlobalId, merged)

    @staticmethod
    def _parse_settled(spec_path: str, subject: str) -> "RuntimeRail[IdsResolved]":
        # the sync path's whole admission: parse, then demand the document already carry nothing pending. The refusal
        # names every unresolvable anchor at once, so an operator supplies one registry rather than re-running per URI.
        return IfcAnalysis.parse(spec_path).bind(
            lambda parsed: parsed.settled.to_result(
                _domain(IfcFault(unresolved_slots=(subject, tuple(facet.uri for facet in parsed.pending))))
            )
        )

    @staticmethod
    def _validate(model: "ifcopenshell.file", ready: IdsResolved, spec: str) -> AnalysisResult:
        # the signature IS the gate: this leg never opens a path, so PARSE and EVALUATE cannot fuse back together and
        # no caller can hand it a document a registry never expanded.
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
        # this leg authors the archive, reads its topic GUIDs back, AND carries the archive bytes on the receipt, so the
        # durable `.bcfzip` write is the data seam's own step over a payload that actually crossed.
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

        return guarded_sync(RetryClass.OCC_NATIVE, solve, at=CLASH_TREE)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
