# [PY_DATA_IMPACT]

One material environmental-impact owner — the EPD/LCA normalization plane of `data`. Two external EPD declaration formats (`openepd`, `epdx`) and three life-cycle-assessment compute legs (the Brightway solver, the live openLCA engine, `premise`-shifted prospective backgrounds) fold into ONE `MaterialImpact` carrier: an EN 15804 indicator × life-cycle-stage matrix keyed by `ContentIdentity`, discriminated on the source payload shape, never a provider knob. This owner owns only the normalization to the common carrier, the identity, the receipt, and the tabular egress — `openepd`/`epdx` own EPD wire parsing, `bw2data` owns the project graph as system of record (filled by `impact/inventory#INVENTORY`), `bw2calc`/openLCA own the solver (`impact/solve#SOLVE` the batch arity), `impact/scenario#SCENARIO` drives the prospective background build the composition runs out of band, and `MaterialImpact` is never the system of record.

Its self-describing eight-column frame crosses to the C# AEC domain as the seam `Discipline.Environmental` `Assessment` payload routed onto the `Material` node `MaterialPropertySet.Environmental` case — `Rasm.Compute` the assessment-runner owner, `Rasm.Materials` the material-node projection — the physical crossing content-keyed canonical Arrow bytes through the `tabular/columnar` public fold. Its solver cluster is pure-python: every provider binds once at module scope through its own `lazy import`, so the deferral is optional-provider loading and never a version gate — a run touching only the declaration arms never pays the Brightway/openLCA import — and every interpreter marker or pin lives in the root manifest alone. Two openepd surfaces stand outside that deferral for stated reasons: `openepd.model.factory`, whose import runs the `patch_pydantic` registration, so a side-effect module is never `lazy`-deferred and `_doctyped` binds it at the call seam that must fire the patch; and `openepd.model.lcia`'s `LCIAMethod`, the carrier's own method VOCABULARY typed on `MaterialImpact` and valued on every `ImpactRegime` row, which a vocabulary cannot defer past the module that types its fields. Transport endpoints arrive from the runtime `TransportResource` at the boundary, never re-minted here.

## [01]-[INDEX]

- [02]-[IMPACT]: the `ImpactSource` provider axis into the normalized `MaterialImpact` carrier, the `ImpactRegime` policy rows, the one `_normalize` fold, the typed `ImpactReceipt`, the eight-column egress frame.

## [02]-[IMPACT]

- Owner: `MaterialImpact` reuses the `openepd` `LCIAMethod` value vocabulary as the canonical method axis, never re-declared, and carries it as `Posture[LCIAMethod]` so a method the declaration itself DECLARED never reads the same as one the regime row SUPPLIED; `Spread` gives mined dispersion a typed home on every `ImpactCell` (`rsd` declared, `std` sampled), so uncertainty is a carried slot, never an afterthought. `Indicator` and `Stage` are the PROVIDER-FIELD key space — `Stage.value` IS the `epdx` `ImpactCategory` field name the walk reads and the stem `OPENEPD_STAGE` upper-cases, `Indicator.value` the `epdx` `EPD` field name — never the `declaration-record` contract's module and indicator tokens that `impact/declaration#DECLARATION` mints; the two key spaces index apart under the `data/RULINGS.md` key-space row and a cross-page join on spelling is the deleted form.
- Regime: `ImpactRegime` is the EN 15804 edition as a POLICY ROW, not a comment — one frozen row per `Regime` carrying that edition's declared per-indicator unit axis and its characterization-method preference ORDER, with `iso21930` the next ROW and never a new arm. The regime owns the unit: an indicator the edition does not define has no row, so an amount it cannot place refuses by name instead of crossing to the C# decoder wearing a foreign edition's unit. Election is DECLARED per arm, never inferred — `EPDX_REGIME` keys the epdx `Standard` member NAME onto the row so `UNKNOWN` refuses instead of coercing to A1, and `OPENEPD_REGIME` names the +A2 row because openepd's LCIA roster IS the +A2 shape (split GWP, split EP, `WDP`, `ADP_mineral`/`ADP_fossil`). `ImpactRegime.rostered` censuses the whole table at IMPORT under the ACCUMULATE posture — every regime rostered, every indicator placeable under at least one edition, every row carrying a method order — so roster-versus-contract parity is a fold a reader can run, never a sentence a reader must trust.
- Cases: the provider is recovered from the payload shape and the arity from the payload value; the `openepd` method selector is `Option[LCIAMethod]`, and `Nothing` reads the regime's declared preference order intersected with the declaration's own `available_methods()`, first declared row winning.
- Entry: `ilcd`, `fetched`, and `olca` are the three admission projectors, one per foreign edge, each minting an `ImpactSource` on the rail — `ilcd` crossing the shared `ilcd_document` guard and the openepd fold admitting the optional `Impacts` container on the rail before it elects a method, so a declaration carrying resource and output flows alone refuses by name instead of answering `AttributeError`; `gated(*rules)` composes the `tabular/contract#ADMISSION` gate downward over the eight-column frame and `profiled(profile)` grades the same frame through the `tabular/profile#PROFILE` plan and hands back the whole `Interrogation` so a downstream report reuses the graded plan; `wire` composes the consumer-edge crossing over the `tabular/columnar` public Arrow-bytes fold with the carrier's `ContentKey`.
- Auto: `premise` builds the future-year background LCI OUT OF BAND and computes no LCIA of its own, so the `premise_background` case names the written database, proves it registered in the open `bw2data` project, and scores it through the same Brightway solve arm — a scenario tuple with no registered background is a refusal, never a present-day score wearing a future year. Identity joins that tuple with the database's own `bd.databases.version`, so identical prospective builds dedupe in the reuse ledger while a rebuilt background re-keys; a demand×method sweep rides the `Block` arity with each solve content-keyed, never a second arm.
- Receipt: source identity keys the receipt — declaration id+version, solve fingerprint, setup identity, scenario tuple — so re-ingestion or recompute of the same declaration dedupes in the `Rasm.Persistence` reuse ledger rather than recomputing; structured evidence on the one runtime rail, never product LCA state. `contribute` projects the one fixed-unit measure — the GWP A1A3 score — onto the runtime `Metrics.record` arm under `domain="impact"`, keyed by source, and publishes the method's own posture beside it so a reader tells a declared characterization from a regime-supplied one; mixed-unit indicators stay receipt evidence, never a metric with an incoherent unit. `_one`'s normalize span is the solver's only trace surface — embedded LCA engines carry no scrape surface, the `query`-plane law applied here.
- Packages: `bw2data` resolves the demand keys and answers the prospective-background registry, `bw2calc` solves, `bw2analyzer` mines the contribution rows, `olca-ipc` drives the live openLCA engine over `requests`, and `openepd`/`epdx` parse the two declaration wires; `ilcd_document` is the package's ONE PyO3 panic guard and the sibling declaration ingress composes it rather than restating the qualname probe. `bw2io` LCI ingestion and `bw-processing` matrix-datapackage custody stand OUTSIDE this normalization fold — the demand keys arrive already resolved in the open project, which the `impact/inventory#INVENTORY` custodian fills — so this owner composes neither and claims no depth it does not reach; `pyarrow` binds at module scope through `lazy import`, deferring the egress load to first lowering.
- Growth: a new EPD format or compute engine is one `ImpactSource` case with its `_normalize` arm; a new regulatory edition is one `Regime` member and one `ImpactRegime` row stating only its divergences from the +A2 base, never a second unit table and never an arm; a new indicator one `Indicator` member and its unit row on every regime that defines it, with a provider correspondence row only where its field spelling drifts; a new stage one `Stage` row; a new characterization edition one `LCIAMethod` member on the owning regime's preference order; a new egress shape one `FrameColumn` row, from which `FRAME_COLUMNS`, `_WIRE_SHAPES`, and `_lower` all derive. The batch and driver-mining depth live at their sibling owners — `impact/solve#SOLVE` the `MultiLCA` shared-factorization sweep, `impact/solve#CONTRIBUTION` the top-emissions and recursive-walk kinds beside this fold's own `annotated_top_processes` mining — and the prospective producer at `impact/scenario#SCENARIO`. Staged rows: the EC3 OMF search stream (`epds.find`) when a consumer names search; per-stage foreground/background splits beyond the aggregate `A1A3` when a consumer carries staged system boundaries.
- Boundary: never a per-provider `EpdImpact`/`LcaImpact` carrier split, never a second normalization kernel, never a re-implemented solver or sparse-matrix assembly, and never a `NewDatabase(...).update()` build inside the fold — a licensed multi-hour sector transform is a composition step, not a normalization one. Deleted forms: a `premise_background` case whose scenario tuple keys an identity while the solve reads whatever background the project happens to hold; a frame missing its `source`/`declared_unit`/`content_key` columns — the C# decoder can neither attribute nor dedupe it; a display-name second chance behind a missed openLCA category ref, and the one map fusing refs with display names that made it spellable; one global unit table every arm reads regardless of edition; a characterization method elected by the lexicographic minimum of method spellings; an openLCA method UUID, a Brightway label, and an openepd method key sharing one column; `or 0.0`, `else ""`, and `else "?"` standing in for a measurement, a unit, or an identity component the source never supplied; `str(Amount)` lowering a pydantic repr into the decoder's declared-unit column, and the `Amount.unit` read that drops its `qty` and rescales every cell at the consumer.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, NoReturn, assert_never

from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import DecodeError, Struct
from msgspec import json as msgjson
from openepd.model.lcia import LCIAMethod
from opentelemetry import trace
from requests.exceptions import RequestException

lazy import bw2calc as bc
lazy import bw2data as bd
lazy import epdx
lazy import olca_ipc as ipc
lazy import pyarrow as pa
lazy from bw2analyzer import ContributionAnalysis
lazy from epdx.pydantic import EPD

from rasm.data.tabular.contract import ContractClaim, FrameAdmission, QualityRule
from rasm.data.tabular.interop import Backend, DataLeg, FieldShape, FrameInterop, ShapeSource, arrow_bytes
from rasm.data.tabular.profile import Interrogation, QualityProfile
from rasm.runtime.faults import (
    TERMINAL,
    TRANSIENT,
    Catch,
    Disposition,
    FaultRow,
    Posture,
    RuntimeRail,
    boundary,
    rostered,
    scoped,
    traversed,
)
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import TransportResource

if TYPE_CHECKING:
    from olca_schema import CalculationSetup, ImpactValue
    from openepd.model.epd import Epd
    from openepd.model.generic_estimate import GenericEstimate
    from openepd.model.industry_epd import IndustryEpd

# --- [TYPES] ----------------------------------------------------------------------------

# one spelling of the provider axis: the union's own tag roster IS the carrier's source column and the receipt's
# key, so the three sites can never drift and no free string reaches the frame the C# decoder attributes by.
type SourceTag = Literal["openepd", "ilcd_epd", "brightway", "openlca", "premise_background"]

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.impact")


class Stage(StrEnum):  # epdx ImpactCategory field names — the PROVIDER key space, never the contract module tokens
    A1A3 = "a1a3"
    A4 = "a4"
    A5 = "a5"
    B1 = "b1"
    B2 = "b2"
    B3 = "b3"
    B4 = "b4"
    B5 = "b5"
    B6 = "b6"
    B7 = "b7"
    C1 = "c1"
    C2 = "c2"
    C3 = "c3"
    C4 = "c4"
    D = "d"


class Indicator(StrEnum):  # epdx EPD field names — the PROVIDER key space; openepd's drift rides OPENEPD_FIELD
    GWP = "gwp"
    GWP_FOSSIL = "gwp_fossil"
    GWP_BIOGENIC = "gwp_biogenic"
    GWP_LULUC = "gwp_luluc"
    ODP = "odp"
    AP = "ap"
    EP = "ep"
    POCP = "pocp"
    ADPE = "adpe"
    ADPF = "adpf"
    PENRE = "penre"
    PENRM = "penrm"
    PENRT = "penrt"
    PERE = "pere"
    PERM = "perm"
    PERT = "pert"
    SM = "sm"
    RSF = "rsf"
    NRSF = "nrsf"
    FW = "fw"
    HWD = "hwd"
    NHWD = "nhwd"
    RWD = "rwd"
    CRU = "cru"
    MFR = "mfr"
    MER = "mer"
    EEE = "eee"
    EET = "eet"


# the regulatory edition an amount was characterized under. Member NAME matches the epdx `Standard` member so the
# election is key identity and never a compare ladder; member VALUE matches the declaration-record `standard` token
# so the two planes agree on the edition's spelling without either importing the other.
class Regime(StrEnum):
    EN15804A1 = "en15804a1"
    EN15804A2 = "en15804a2"


# --- [CONSTANTS] ------------------------------------------------------------------------

# per-provider field correspondences DERIVE from the vocabulary; only drifting members override —
# epdx spells materials-for-recycling `mrf`; openepd spells exported energy `ee`/`eh` and abiotic
# depletion `ADP_mineral`/`ADP_fossil` (an un-overridden `adpe`/`adpf` never reaches a ScopeSet).
EPDX_FIELD: Final[Map[Indicator, str]] = Map.of_seq([(ind, {"mfr": "mrf"}.get(ind.value, ind.value)) for ind in Indicator])
OPENEPD_FIELD: Final[Map[Indicator, str]] = Map.of_seq([
    (ind, {"eee": "ee", "eet": "eh", "adpe": "ADP_mineral", "adpf": "ADP_fossil"}.get(ind.value, ind.value)) for ind in Indicator
])
OPENEPD_STAGE: Final[Map[Stage, str]] = Map.of_seq([(stage, "A1A2A3" if stage is Stage.A1A3 else stage.value.upper()) for stage in Stage])

# the edition an arm scores under is DECLARED, never inferred. epdx carries the token on the document and keys by
# member NAME, so its `UNKNOWN` member has no row and refuses instead of coercing to A1. openepd carries no edition
# token at all and its LCIA roster IS the +A2 shape — split GWP, split EP, `WDP`, `ADP_mineral`/`ADP_fossil`
# (`.api/openepd.md` [IMPACTSET_MEMBER_SCOPE]) — so the EC3 arm names the A2 row once here rather than guessing.
EPDX_REGIME: Final[Map[str, Regime]] = Map.of_seq([(regime.name, regime) for regime in Regime])
OPENEPD_REGIME: Final[Regime] = Regime.EN15804A2

# the functional quantity the eight-column frame can carry: it holds a declared-unit TOKEN and no quantity axis, so a
# source declaring its cells per any other quantity refuses rather than crossing with the scale factor dropped.
_UNIT_QUANTITY: Final[float] = 1.0

_ENCODER: Final = msgjson.Encoder(order="deterministic")

# the PyO3 panic type is UNNAMEABLE: `pyo3_runtime` is a synthetic module the interpreter materializes only as a
# panic is raised — it resolves nowhere before that and imports nowhere after — so no `catch` tuple can hold the
# class and `ilcd_document` recognizes it by qualname instead. `.api/epdx.md:44` states the same absence.
_PANIC: Final[str] = "PanicException"


def _ilcd_raises() -> Catch:
    # `ilcd_document` converts the panic into a `ValueError` and `EPD.model_validate` answers pydantic's
    # `ValidationError`, itself a `ValueError` (`.api/epdx.md:45`), so one row covers both parse halves; the
    # decode of the converted document answers `msgspec.MsgspecError`.
    return (DecodeError, TypeError, ValueError)


def _solver_raises() -> Catch:
    # the Brightway legs bind `lazy`, so the set resolves at the CALL. `BW2CalcError` roots the solver family,
    # `BW2Exception` the store's, and a demand key or method tuple the project never registered answers `KeyError`.
    return (bc.errors.BW2CalcError, bd.errors.BW2Exception, KeyError, TypeError, ValueError, OSError)


def _olca_raises() -> Catch:
    # `olca_ipc` drives the engine over `requests` and raises `RuntimeError` itself for a protocol fault, so the
    # transport root (`RequestException`, an `OSError`) and that row are the whole reachable set — probed against
    # the installed distribution, which publishes no exception module of its own.
    return (RequestException, RuntimeError, KeyError, TypeError, ValueError)


# this module's raise roster under its one `DataLeg` member. The two live legs — the EC3 fetch and the openLCA
# round trip — declare TRANSIENT because a remote hop may clear on a re-issue; every other row is TERMINAL, since a
# malformed declaration, an unresolvable slot, and a deterministic solve over one project all refuse identically on
# every re-read. `impact.olca`'s absent-slot census is this owner's OWN refusal and carries its column list.
IMPACT_ILCD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.IMPACT, point="ilcd", arm="boundary", defect="ilcd-parse", retriability=TERMINAL
)
IMPACT_FETCH: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.IMPACT, point="fetch", arm="boundary", defect="ec3-document", retriability=TRANSIENT
)
IMPACT_SETUP: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.IMPACT, point="olca.setup", arm="config", defect="setup-absent", retriability=TERMINAL, slots=("slots",)
)
IMPACT_NORMALIZE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.IMPACT, point="normalize", arm="boundary", defect="normalization", retriability=TERMINAL
)
IMPACT_FRAME: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.IMPACT, point="frame", arm="boundary", defect="frame-lowering", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    IMPACT_ILCD,
    IMPACT_FETCH,
    IMPACT_SETUP,
    IMPACT_NORMALIZE,
    IMPACT_FRAME,
]))

# --- [MODELS] ---------------------------------------------------------------------------


class Spread(Struct, frozen=True, gc=False):
    kind: Literal["rsd", "std"]
    value: float
    samples: int = 0


class ImpactCell(Struct, frozen=True):
    indicator: Indicator
    stage: Stage
    amount: float
    unit: str
    spread: Option[Spread] = Nothing


class ContributionRow(Struct, frozen=True, gc=False):
    score: float
    supply: float
    activity: str


class LcaSolve(Struct, frozen=True):
    demand: tuple[tuple[object, float], ...]  # (activity key or id, amount) pairs prepare_lca_inputs resolves
    method: tuple[str, ...]  # the bw2data method REGISTRY key — its own key space, never the carrier's method column
    indicator: Indicator = Indicator.GWP
    label: LCIAMethod = LCIAMethod.EN_15978_2011  # the vocabulary member the carrier's method axis declares
    iterations: int = 0  # >0 samples the UNCERTAINTY_DTYPE rows: one Monte Carlo draw per next(lca)
    contributions: int = 0  # >0 mines bw2analyzer annotated_top_processes to this depth


class PremiseSolve(Struct, frozen=True):
    solve: LcaSolve
    # the premise-written prospective database THIS solve scores against. `premise` builds it out of band —
    # `NewDatabase(scenarios, source_type='brightway', key=…, system_model=…).update().write_db_to_brightway(name)`
    # is an ecoinvent-licensed, self-parallelizing, multi-hour sector transform, never a step a normalization fold
    # runs inline — so the registered name is what makes this leg prospective at all. Without it a scenario tuple is
    # a bare cache key: the solve reads whatever background the open project happens to hold, scores the PRESENT
    # day, and labels the result with a future year, the one failure a prospective carrier cannot survive.
    background: str
    scenario: tuple[str, str, int, str, str]  # (model, pathway, year, ecoinvent version, system_model)


# every `olca_schema` field is `| None`, so the setup's coded identity admits ONCE at `MaterialImpact.olca` and lands
# here total. Two solves can then never collide in the reuse ledger on a shared placeholder, because no placeholder is
# representable: a keyless setup refuses before the first IPC round trip rather than after a server-side calculation.
class OlcaIdentity(Struct, frozen=True):
    target: str  # the setup's coded product-system or process ref
    method_ref: str  # the openLCA impact-method ref — its own registry key space, carried beside the vocabulary member
    method: LCIAMethod  # the engine method's spelling in the one method vocabulary the frame's column admits
    amount: float  # the functional quantity the setup declares
    declared_unit: str  # the functional unit the setup declares — the frame's non-nullable declared_unit column

    def preimage(self) -> bytes:
        return f"{self.target}:{self.method_ref}:{self.amount}".encode()


class OlcaSolve(Struct, frozen=True):
    endpoint: str  # supplied by the runtime TransportResource axis at the boundary
    setup: "CalculationSetup"  # caller-authored via olca_schema new_* factories
    refs: Map[str, Indicator]  # openLCA impact-category REF -> Indicator; the coded key space ALONE, a miss refuses
    identity: OlcaIdentity


@tagged_union(frozen=True)
class ImpactSource:
    tag: SourceTag = tag()
    openepd: "tuple[Epd | IndustryEpd | GenericEstimate, Option[LCIAMethod]]" = case()  # Nothing elects the regime order
    ilcd_epd: "EPD" = case()
    brightway: LcaSolve = case()
    openlca: OlcaSolve = case()
    premise_background: PremiseSolve = case()


class ImpactReceipt(Struct, frozen=True):
    source: SourceTag
    method: Posture[LCIAMethod]
    cell_count: int
    sampled: int
    content_key: ContentKey
    gwp_a1a3: Option[float] = Nothing  # the one fixed-unit (kg CO2 eq) measure the metric spine grades

    def contribute(self) -> Iterable[Receipt]:
        # receipts stay truth, instruments stay projections: the GWP score lands on the runtime metric spine under domain="impact".
        match self.gwp_a1a3:
            case Option(tag="some", some=score):
                Metrics.record({"rasm.impact.score": score}, domain="impact", kind=self.source)
            case Option(tag="none"):
                pass
        # the method's posture rides beside its value, so a reader tells the declaration's OWN characterization key
        # from one the regime row supplied; `source` names the supplier and the tag answers where none exists.
        evidence = {
            "method": self.method.option().map(lambda named: named.value).default_value(self.method.tag),
            "method_source": self.method.source.default_value(self.method.tag),
            "cells": self.cell_count,
            "sampled": self.sampled,
        }
        yield Receipt.of("impact", ("emitted", self.source, evidence))


class MaterialImpact(Struct, frozen=True):
    source: SourceTag
    method: Posture[LCIAMethod]
    declared_unit: str
    cells: tuple[ImpactCell, ...]
    content_key: ContentKey
    contributions: tuple[ContributionRow, ...] = ()

    @classmethod
    def of(
        cls, payload: "ImpactSource | Block[ImpactSource]", *, by: Disposition = Disposition.ABORT
    ) -> "RuntimeRail[MaterialImpact] | RuntimeRail[Block[MaterialImpact]]":
        match payload:
            case Block() as many:
                return traversed(many.map(cls._one), by=by)
            case lone:
                return cls._one(lone)

    @staticmethod
    def ilcd(document: "str | bytes") -> "RuntimeRail[ImpactSource]":
        # ILCD admission projector over the ONE panic guard this package holds; the converted JSON lands the typed
        # epdx model, and the sibling declaration ingress composes the same `ilcd_document` rather than restating it.
        return boundary(
            IMPACT_ILCD, lambda: ImpactSource(ilcd_epd=EPD.model_validate(msgjson.decode(ilcd_document(document).encode()))),
            catch=_ilcd_raises(),
        )

    @staticmethod
    async def fetched(transport: TransportResource, uuid: str, method: "Option[LCIAMethod]" = Nothing) -> "RuntimeRail[ImpactSource]":
        # live EC3 leg over the runtime transport axis — bearer + retry ride the resource; RootDocumentFactory routes the doctype.
        acquired = await transport.acquire(f"epds/{uuid}")
        return acquired.bind(
            lambda body: boundary(
                IMPACT_FETCH,
                lambda: ImpactSource(openepd=(_doctyped(bytes(body)), method)),
                catch=(DecodeError, TypeError, ValueError),
            )
        )

    @staticmethod
    def olca(endpoint: str, setup: "CalculationSetup", refs: "Map[str, Indicator]") -> "RuntimeRail[ImpactSource]":
        # every `olca_schema` slot is optional, so the WHOLE setup admits HERE, once, ACCUMULATING: a half-authored
        # setup names every empty slot in one refusal — `boundaries.md` `[PROBE_SWEEP]` fixes the disposition at the
        # seam the probes run — and the refusal lands BEFORE `calculate`, so no server-side work is spent on a solve
        # that could never be keyed. The census below is the totality proof for the five reads after it; each read
        # still names its own subject, so a slot added to the identity and not to the census refuses rather than
        # silently typing as an `Option`. A caller-repairable construction refusal is the `config` case by law.
        target = Option.of_optional(setup.target).bind(lambda ref: Option.of_optional(ref.id))
        keyed = Option.of_optional(setup.impact_method)
        method_ref = keyed.bind(lambda ref: Option.of_optional(ref.id))
        spelled = keyed.bind(lambda ref: Option.of_optional(ref.name)).bind(_lcia)
        amount = Option.of_optional(setup.amount)
        declared = Option.of_optional(setup.unit).bind(lambda ref: Option.of_optional(ref.name))
        absent = Block.of_seq([
            name
            for name, slot in (
                ("target", target),
                ("impact_method", method_ref),
                ("method-vocabulary", spelled),
                ("amount", amount),
                ("unit", declared),
            )
            if slot.is_none()
        ])
        if not absent.is_empty():
            return Error(IMPACT_SETUP.raised(",".join(absent)))
        identity = OlcaIdentity(
            target=_present("olca.target", target),
            method_ref=_present("olca.method-ref", method_ref),
            method=_present("olca.method-vocabulary", spelled),
            amount=_present("olca.amount", amount),
            declared_unit=_present("olca.unit", declared),
        )
        return Ok(ImpactSource(openlca=OlcaSolve(endpoint=endpoint, setup=setup, refs=refs, identity=identity)))

    @classmethod
    def _one(cls, payload: "ImpactSource") -> "RuntimeRail[MaterialImpact]":
        # sparse-matrix solve and declaration folds are heavy in-process legs with no scrape surface — the span IS the
        # engine observability; the boundary fence inside marks the span ERROR + record_exception on a failed leg.
        with _TRACER.start_as_current_span(f"impact.normalize.{payload.tag}", attributes={"rasm.impact.source": payload.tag}):
            # ONE fence over five arms whose provider sets are disjoint, so the catch is the UNION its own arms
            # reach: the two declaration folds refuse through `_present`'s `ValueError`, the Brightway arms through
            # the solver family, and the openLCA arm through its transport. The span attribute names which arm ran.
            return boundary(
                IMPACT_NORMALIZE, lambda: _normalize(payload), catch=(*_ilcd_raises(), *_solver_raises(), *_olca_raises())
            )

    def frame(self) -> "RuntimeRail[pa.Table]":
        return boundary(IMPACT_FRAME, lambda: _lower(self), catch=(pa.ArrowException, TypeError, ValueError))

    def wire(self) -> "RuntimeRail[tuple[bytes, ContentKey]]":
        # consumer-edge physical crossing: the canonical Arrow-bytes fold over the frame, the carrier key traveling with the bytes.
        return self.frame().map(lambda table: (bytes(arrow_bytes(table)), self.content_key))

    def gated(self, *rules: QualityRule) -> "RuntimeRail[ContractClaim]":
        # eight-column frame proves the `_WIRE_SHAPES` structural rows through the one contract gate — the claim records,
        # never raises, so a breached wire is a caller `match`, not an exception.
        admission = FrameAdmission.of(FrameInterop.of(Backend.PYARROW), _WIRE_SHAPES, *rules)
        return self.frame().bind(lambda table: admission.admit(table).bind(admission.enforce))

    async def profiled(self, profile: QualityProfile) -> "RuntimeRail[Interrogation]":
        # caller-tuned pointblank plan grades the frame above the gate — pointblank's Narwhals engine admits the pa.Table
        # directly. The synchronous Arrow lowering crosses the band-bounded `on_thread` hop so the loop never hosts the
        # materialization; the frame rail short-circuits before the awaited interrogation and the composed signature
        # stays one rail, never a coroutine smuggled through `bind`. The whole `Interrogation` rides out, not its receipt
        # alone, so a caller driving a report threads the graded plan back in rather than re-interrogating the frame.
        match await on_thread(self.frame):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=table):
                return await profile.interrogate(table)
            case _ as unreachable:
                assert_never(unreachable)

    def receipt(self) -> ImpactReceipt:
        sampled = max((cell.spread.map(lambda s: s.samples).default_value(0) for cell in self.cells), default=0)
        gwp = Block.of_seq(self.cells).choose(
            lambda cell: Some(cell.amount) if cell.indicator is Indicator.GWP and cell.stage is Stage.A1A3 else Nothing
        )
        return ImpactReceipt(
            source=self.source,
            method=self.method,
            cell_count=len(self.cells),
            sampled=sampled,
            content_key=self.content_key,
            gwp_a1a3=gwp.try_head(),
        )


# --- [TABLES] ---------------------------------------------------------------------------


# the regulatory edition as a POLICY VALUE per `surfaces-and-dispatch.md` `[POLICY_VALUES]`: the fold reads rows and
# never re-derives which unit or which characterization set an edition mandates. `units` is the edition's OWN axis, so
# an indicator it does not define carries no row and refuses; `methods` is the DECLARED preference order the election
# walks, so the characterization set reaching the C# assessment is a stated policy and never an alphabetical accident.
class ImpactRegime(Struct, frozen=True):
    key: Regime
    methods: tuple[LCIAMethod, ...]
    units: Map[Indicator, str]

    def unit_of(self, indicator: Indicator) -> Option[str]:
        return self.units.try_find(indicator)

    def elected(self, available: frozenset[LCIAMethod]) -> Option[LCIAMethod]:
        return Block.of_seq(self.methods).filter(lambda method: method in available).try_head()

    @staticmethod
    def rostered(rows: "Block[ImpactRegime]") -> "Map[Regime, ImpactRegime]":
        # import-time census under the ACCUMULATE posture, the `runtime/execution/admission#ADMISSION` `_FACTS` idiom:
        # every failed invariant reports with the exact subjects that failed it, so a roster edit reads the whole break
        # at once rather than the first row to trip. A table that cannot place an indicator makes the module
        # unimportable — the parity proof is this fold, never a sentence beside the rows.
        broken = _ROSTER_FACTS.choose(
            lambda fact: Nothing if fact.holds(rows) else Some(f"{fact.reason}:{','.join(fact.subjects(rows))}")
        )
        if not broken.is_empty():
            raise ValueError(f"impact-regime-roster:{'|'.join(broken)}")
        return Map.of_seq([(row.key, row) for row in rows])


class RosterFact(Struct, frozen=True):
    reason: str
    holds: Callable[["Block[ImpactRegime]"], bool]
    subjects: Callable[["Block[ImpactRegime]"], tuple[str, ...]]


_ROSTER_FACTS: Final[Block[RosterFact]] = Block.of_seq([
    RosterFact(
        "regime-unrostered",
        lambda rows: frozenset(Regime) == frozenset(row.key for row in rows),
        lambda rows: tuple(sorted(frozenset(Regime) ^ frozenset(row.key for row in rows))),
    ),
    RosterFact(
        "indicator-unplaceable",
        lambda rows: frozenset(Indicator) <= frozenset(ind for row in rows for ind in row.units.keys()),
        lambda rows: tuple(sorted(frozenset(Indicator) - frozenset(ind for row in rows for ind in row.units.keys()))),
    ),
    RosterFact(
        "regime-methodless",
        lambda rows: all(row.methods for row in rows),
        lambda rows: tuple(row.key.value for row in rows if not row.methods),
    ),
])

# EN 15804+A2 is the BASE row — the edition every current provider wire is shaped to. `ep` carries no row here: the
# +A2 edition splits eutrophication into freshwater, marine, and terrestrial indicators this provider-field roster
# spells as one A1-shaped `ep`, so an A2 declaration carrying it refuses by name instead of crossing with the +A1 unit.
_A2_UNIT: Final[Map[Indicator, str]] = Map.of_seq([
    (Indicator.GWP, "kg CO2 eq"),
    (Indicator.GWP_FOSSIL, "kg CO2 eq"),
    (Indicator.GWP_BIOGENIC, "kg CO2 eq"),
    (Indicator.GWP_LULUC, "kg CO2 eq"),
    (Indicator.ODP, "kg CFC-11 eq"),
    (Indicator.AP, "mol H+ eq"),
    (Indicator.POCP, "kg NMVOC eq"),
    (Indicator.ADPE, "kg Sb eq"),
    (Indicator.ADPF, "MJ"),
    (Indicator.PENRE, "MJ"),
    (Indicator.PENRM, "MJ"),
    (Indicator.PENRT, "MJ"),
    (Indicator.PERE, "MJ"),
    (Indicator.PERM, "MJ"),
    (Indicator.PERT, "MJ"),
    (Indicator.SM, "kg"),
    (Indicator.RSF, "MJ"),
    (Indicator.NRSF, "MJ"),
    (Indicator.FW, "m3"),
    (Indicator.HWD, "kg"),
    (Indicator.NHWD, "kg"),
    (Indicator.RWD, "kg"),
    (Indicator.CRU, "kg"),
    (Indicator.MFR, "kg"),
    (Indicator.MER, "kg"),
    (Indicator.EEE, "MJ"),
    (Indicator.EET, "MJ"),
])

# +A1 states only its DIVERGENCES: one aggregate eutrophication indicator, and acidification and photochemical-ozone
# units that predate the +A2 respell. openepd's own `ScopeSetAp` and `ScopeSetPocp` carry BOTH spellings per edition
# (`allowed_units=("kgSO2e", "molHe")` and `("kgO3e", "kgNMVOCe")`, `.api/openepd.md` [LCIA_SCOPE]), the package-side
# proof this axis belongs to the regime row and never to one global table.
_A1_UNIT: Final[Map[Indicator, str]] = Map.of_seq([
    (Indicator.AP, "kg SO2 eq"),
    (Indicator.EP, "kg PO4 eq"),
    (Indicator.POCP, "kg C2H4 eq"),
])

# the +A2 GWP components have no +A1 counterpart, so the A1 row drops them rather than stamping an A2-only indicator.
_A2_SPLIT: Final[frozenset[Indicator]] = frozenset({Indicator.GWP_FOSSIL, Indicator.GWP_BIOGENIC, Indicator.GWP_LULUC})

EN15804A1: Final[ImpactRegime] = ImpactRegime(
    key=Regime.EN15804A1,
    methods=(LCIAMethod.CML_2016,),  # `.api/openepd.md` [METHOD_LAW]: CML 2016 is the +A1 method, single-`ep` eutrophication
    units=Map.of_seq([
        *((ind, unit) for ind, unit in _A2_UNIT.items() if ind not in _A2_SPLIT and ind not in _A1_UNIT),
        *_A1_UNIT.items(),
    ]),
)
EN15804A2: Final[ImpactRegime] = ImpactRegime(
    key=Regime.EN15804A2,
    methods=(LCIAMethod.EF_3_1, LCIAMethod.EF_3_0),  # `.api/openepd.md` [METHOD_LAW]: +A2 characterization is EF 3.1/3.0
    units=_A2_UNIT,
)
REGIMES: Final[Map[Regime, ImpactRegime]] = ImpactRegime.rostered(Block.of_seq([EN15804A1, EN15804A2]))


# ONE authority for the self-describing eight-column frame: the column name, its structural type, and the cell fill
# ride one row, so `FRAME_COLUMNS`, the `_WIRE_SHAPES` the contract gate proves, and `_lower`'s projection all derive
# from it. Three hand-kept spellings of one fact is how a column reaches the wire that no gate declared.
class FrameColumn(Struct, frozen=True):
    name: str
    logical: Literal["Float64", "String"]
    fill: Callable[["MaterialImpact", ImpactCell], float | str]


_FRAME: Final[tuple[FrameColumn, ...]] = (
    FrameColumn(name="source", logical="String", fill=lambda impact, _cell: impact.source),
    FrameColumn(name="method", logical="String", fill=lambda impact, _cell: _present("frame.method", impact.method.option()).value),
    FrameColumn(name="indicator", logical="String", fill=lambda _impact, cell: cell.indicator.value),
    FrameColumn(name="stage", logical="String", fill=lambda _impact, cell: cell.stage.value),
    FrameColumn(name="amount", logical="Float64", fill=lambda _impact, cell: cell.amount),
    FrameColumn(name="unit", logical="String", fill=lambda _impact, cell: cell.unit),
    FrameColumn(name="declared_unit", logical="String", fill=lambda impact, _cell: impact.declared_unit),
    FrameColumn(name="content_key", logical="String", fill=lambda impact, _cell: impact.content_key.hex),
)

FRAME_COLUMNS: Final[tuple[str, ...]] = tuple(column.name for column in _FRAME)

# structural claim over the fence-pinned wire: amount is the one float64 column, every other
# slot a non-null string — the FieldShape rows the contract gate proves before the wire crosses.
_WIRE_SHAPES: Final[tuple[FieldShape, ...]] = tuple(
    # `DECLARED` by construction: a wire CONTRACT states its nullability as policy and observes no null mask, so the
    # posture the gate compares against is the stated one and a breach names a declared-versus-observed pair.
    FieldShape(field=column.name, logical_type=column.logical, nullable=False, source=ShapeSource.DECLARED)
    for column in _FRAME
)


# --- [OPERATIONS] -----------------------------------------------------------------------


def ilcd_document(document: "str | bytes") -> str:
    # THE package's one ILCD panic guard, public because `impact/declaration#DECLARATION` ingests the same wire and
    # a second copy of this body is the fork. `convert_ilcd` unwraps a Rust parse `Result`, so malformed input
    # surfaces as `pyo3_runtime.PanicException` — rooted at `BaseException`, which no `catch` tuple can name because
    # the module holding the class materializes only as the panic is raised. Recognition is therefore by QUALNAME,
    # every other `BaseException` re-raises untouched, and the panic leaves as a `ValueError` a fence set can hold.
    text = document.decode() if isinstance(document, bytes) else document
    try:
        return epdx.convert_ilcd(text)
    except BaseException as panic:  # ruff:ignore[blind-except] — the PyO3 panic is BaseException-rooted and unnameable
        if type(panic).__name__ != _PANIC:
            raise
        raise ValueError("ilcd-parse-panic") from panic


def _refused(subject: str) -> NoReturn:
    raise ValueError(f"impact-absent:{subject}")


def _present[T](subject: str, value: "Option[T]") -> T:
    # THE absence seam of this plane, per `boundaries.md` `[SENTINEL_SITE]`: every foreign `| None` slot and every
    # policy-row miss refuses HERE by subject, so no `or 0.0`, `else ""`, or `else "?"` ever reaches a measurement, a
    # unit, or an identity component. A DECLARATION's missing module is a different fact and stays a SKIP — an
    # undeclared cell is absence the coverage census reads, never a failed read the carrier must refuse.
    return value.default_with(lambda: _refused(subject))


def _lcia(name: str) -> "Option[LCIAMethod]":
    # the engine's own method name crosses into the one method vocabulary through the package's OWN membership
    # predicate: `get_by_name` buckets an unknown spelling into `LCIAMethod.UNKNOWN`, a token the C# assessment roster
    # refuses by name, so membership decides here and that bucket never reaches the frame's method column.
    return Some(LCIAMethod(name)) if LCIAMethod.is_method_supported(name) else Nothing


def _normalize(src: "ImpactSource") -> MaterialImpact:
    match src:
        case ImpactSource(tag="openepd", openepd=(decl, method)):
            return _from_openepd(decl, method)
        case ImpactSource(tag="ilcd_epd", ilcd_epd=epd):
            return _from_epdx(epd)
        case ImpactSource(tag="brightway", brightway=solve):
            return _from_score(solve, "brightway", _ENCODER.encode((solve.demand, solve.method)))
        case ImpactSource(tag="premise_background", premise_background=shifted):
            return _from_prospective(shifted)
        case ImpactSource(tag="openlca", openlca=solve):
            return _from_olca(solve)
        case unreachable:
            assert_never(unreachable)


def _from_openepd(decl: "Epd | IndustryEpd | GenericEstimate", method: "Option[LCIAMethod]") -> MaterialImpact:
    # ALL THREE ScopesetByNameBase containers fold: Impacts (method-keyed) + ResourceUseSet +
    # OutputFlowSet (direct containers) — an Impacts-only fold strands pere/penre/fw and hwd/nhwd/eee.
    # The caller's pinned method must be one the declaration actually carries: an unavailable pin used to select an
    # absent impact set and silently yield a carrier holding resource and output flows alone.
    regime = REGIMES[OPENEPD_REGIME]
    # `Epd.impacts` is `Impacts | None` (`.api/openepd.md` `[DECLARATION_FIELD_SCOPE]` — EVERY field is optional), so
    # the container admits on the rail BEFORE any method is elected: reaching `available_methods()` through the bare
    # attribute answered `AttributeError` on a declaration carrying resource and output flows alone, which is a
    # lawful EC3 document rather than a defect. The sibling declaration ingress admits the same slot the same way.
    impacts = _present("openepd.impacts", Option.of_optional(decl.impacts))
    available = frozenset(impacts.available_methods())
    chosen = _present("openepd.method", method.filter(lambda named: named in available).or_else(regime.elected(available)))
    containers = tuple(c for c in (impacts.get_impact_set(chosen), decl.resource_uses, decl.output_flows) if c is not None)

    def cells_of(container: object) -> Iterable[ImpactCell]:
        for indicator in Indicator:
            scopeset = container.get_scopeset_by_name(OPENEPD_FIELD[indicator])
            if scopeset is None:
                continue
            for stage in Stage:
                measurement = getattr(scopeset, OPENEPD_STAGE[stage], None)
                if measurement is None:  # an undeclared module writes no cell; `Measurement.mean` is non-optional
                    continue
                spread = Some(Spread(kind="rsd", value=measurement.rsd)) if measurement.rsd is not None else Nothing
                # the declaration's OWN unit wins; the regime row is the floor, and an indicator that edition does not
                # define has no floor, so the amount refuses rather than crossing under a foreign edition's unit.
                unit = Option.of_optional(measurement.unit).or_else(regime.unit_of(indicator))
                yield ImpactCell(
                    indicator=indicator,
                    stage=stage,
                    amount=measurement.mean,
                    unit=_present(f"openepd.unit:{indicator.value}", unit),
                    spread=spread,
                )

    # EVERY `Epd` field is optional (`.api/openepd.md` [DECLARATION_FIELD_SCOPE]), so a declaration missing `id` or
    # `version` would key two different products onto one content key; both refuse by name instead.
    identity = Option.of_optional(decl.id).bind(lambda uid: Option.of_optional(decl.version).map(lambda version: f"{uid}:{version}"))
    # `declared_unit` is an `Amount` — a QUANTITY beside a unit — and `.api/openepd.md` names reading the unit without
    # its `qty` as the defect that silently rescales every cell. The frame carries a unit token and no quantity axis,
    # so both halves admit here and a declaration declared per anything but one unit refuses rather than crossing
    # under-specified. `str(Amount)` is the deleted read: it lowers a pydantic repr into the decoder's unit column.
    amount = Option.of_optional(decl.declared_unit)
    quantity = _present("openepd.declared-quantity", amount.bind(lambda declared: Option.of_optional(declared.qty)))
    if quantity != _UNIT_QUANTITY:
        _refused(f"openepd.declared-quantity:{quantity}")
    return MaterialImpact(
        source="openepd",
        method=Posture(declared=chosen),
        declared_unit=_present("openepd.declared-unit", amount.bind(lambda declared: Option.of_optional(declared.unit))),
        cells=tuple(cell for container in containers for cell in cells_of(container)),
        content_key=ContentIdentity.key("impact", _present("openepd.identity", identity).encode()),
    )


def _from_epdx(epd: "EPD") -> MaterialImpact:
    # `epd.standard` IS the regime the fold dispatches on: the edition decides every cell's unit, and epdx carries no
    # unit of its own, so an unelectable standard refuses here rather than lowering amounts under a guessed edition.
    # epdx carries no characterization method either — the edition's first declared method is DEFAULTED, and the
    # `Posture` source names the regime that supplied it so no reader mistakes it for the declaration's own key.
    regime = REGIMES[_present(f"epdx.standard:{epd.standard.name}", EPDX_REGIME.try_find(epd.standard.name))]
    cells = tuple(
        ImpactCell(indicator=ind, stage=stg, amount=val, unit=_present(f"epdx.unit:{ind.value}", regime.unit_of(ind)))
        for ind in Indicator
        if (category := getattr(epd, EPDX_FIELD[ind], None)) is not None
        for stg in Stage
        if (val := getattr(category, stg.value, None)) is not None
    )
    key = ContentIdentity.key("impact", f"{epd.id}:{epd.published_date}:{epd.version}".encode())
    return MaterialImpact(
        source="ilcd_epd",
        method=Posture(defaulted=(regime.methods[0], f"regime:{regime.key.value}")),
        declared_unit=epd.declared_unit.value,
        cells=cells,
        content_key=key,
    )


def _from_score(solve: LcaSolve, source: SourceTag, identity: bytes) -> MaterialImpact:
    # staged bw2calc solve at mined depth: lci/lcia/score -> the aggregate A1A3 cell; Monte
    # Carlo one draw per next(lca); bw2analyzer contribution rows when the request carries depth.
    demand = dict(solve.demand)
    fu, data_objs, remapping = bd.prepare_lca_inputs(demand=demand, method=solve.method)
    lca = bc.LCA(demand=fu, data_objs=data_objs, remapping_dicts=remapping, use_distributions=solve.iterations > 0)
    lca.lci()
    lca.lcia()
    # the registered method's own metadata owns this unit — no EN 15804 edition governs a Brightway method, so a
    # method registering no unit refuses rather than borrowing an indicator floor from a declaration regime.
    declared = Option.of_optional(bd.Method(solve.method).metadata.get("unit"))
    unit = str(_present(f"brightway.unit:{'/'.join(solve.method)}", declared))
    spread: Option[Spread] = Nothing
    if solve.iterations > 0:
        lca.keep_first_iteration()

        def drawn() -> float:
            next(lca)  # the LCA exposes __next__ alone (no __iter__): one Monte Carlo draw per next(lca)
            return float(lca.score)

        samples = tuple(drawn() for _ in range(solve.iterations))
        mean = sum(samples) / len(samples)
        std = (sum((s - mean) ** 2 for s in samples) / len(samples)) ** 0.5
        spread = Some(Spread(kind="std", value=std, samples=len(samples)))
    rows: tuple[ContributionRow, ...] = ()
    if solve.contributions > 0:
        rows = tuple(
            ContributionRow(score=score, supply=supply, activity=str(activity))
            for score, supply, activity in ContributionAnalysis().annotated_top_processes(lca, limit=solve.contributions)
        )
    cell = ImpactCell(indicator=solve.indicator, stage=Stage.A1A3, amount=float(lca.score), unit=unit, spread=spread)
    key = ContentIdentity.key("impact", identity)
    return MaterialImpact(
        source=source,
        method=Posture(declared=solve.label),
        declared_unit=unit,
        cells=(cell,),
        content_key=key,
        contributions=rows,
    )


def _from_prospective(shifted: PremiseSolve) -> MaterialImpact:
    # premise BUILDS the prospective background out of band and this owner SCORES it: the registered database name
    # is the whole proof the leg is prospective, so an absent one refuses at admission rather than returning a
    # present-day score wearing a future year. Its own registry version joins the identity, so a rebuilt background
    # under the same scenario tuple re-keys instead of deduping onto the prior build's score in the reuse ledger.
    if shifted.background not in bd.databases.list:
        raise ValueError(f"premise-background-absent:{shifted.background}")
    identity = _ENCODER.encode((shifted.scenario, shifted.background, bd.databases.version(shifted.background)))
    return _from_score(shifted.solve, "premise_background", identity)


def _from_olca(solve: OlcaSolve) -> MaterialImpact:
    # documented lifecycle: setup -> calculate -> wait_until_ready -> query -> dispose (finally).
    client = ipc.Client(solve.endpoint)
    result = client.calculate(solve.setup)
    try:
        state = result.wait_until_ready()
        if state.error:
            raise ValueError(f"olca-state:{state.error}")
        rows = result.get_total_impacts()
    finally:
        result.dispose()

    def cell_of(row: "ImpactValue") -> ImpactCell:
        # the impact-category REF is the WHOLE key. A display-name second chance binds the first renamed category to
        # the wrong indicator, and `data/RULINGS.md` already indexes two key spaces apart for exactly this reason, so
        # the ref misses loudly and a name never gets a turn. The engine's own reference unit rides each row, so the
        # cell never wears an EN 15804 edition's unit a TRACI or ReCiPe method never characterized against.
        category = _present("olca.impact-category", Option.of_optional(row.impact_category))
        ref = _present("olca.category-ref", Option.of_optional(category.id))
        return ImpactCell(
            indicator=_present(f"olca.category-gap:{ref}", solve.refs.try_find(ref)),
            stage=Stage.A1A3,
            amount=_present(f"olca.amount:{ref}", Option.of_optional(row.amount)),
            unit=_present(f"olca.category-unit:{ref}", Option.of_optional(category.ref_unit)),
        )

    return MaterialImpact(
        source="openlca",
        method=Posture(declared=solve.identity.method),
        declared_unit=solve.identity.declared_unit,
        cells=tuple(cell_of(row) for row in rows),
        content_key=ContentIdentity.key("impact", solve.identity.preimage()),
    )


def _doctyped(body: bytes) -> "Epd | IndustryEpd | GenericEstimate":
    from openepd.model.factory import RootDocumentFactory  # ruff:ignore[import-outside-top-level] — call-seam side effect (patch_pydantic) bans the module-scope binding

    return RootDocumentFactory.from_dict(msgjson.decode(body))


def _lower(impact: MaterialImpact) -> "pa.Table":
    # eight-column SELF-DESCRIBING floor: a frame the C# Discipline.Environmental Assessment
    # decode attributes (source/method/declared_unit) and dedupes (content_key) with no side channel.
    # The `_FRAME` rows are the one authority — a column the gate declares and the projection forgets is unspellable.
    return pa.Table.from_pydict({column.name: [column.fill(impact, cell) for cell in impact.cells] for column in _FRAME})
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
