# [PY_DATA_IMPACT]

One material environmental-impact owner — the EPD/LCA normalization plane of `data`. Two external EPD declaration formats (`openepd`, `epdx`) and three life-cycle-assessment compute legs (the Brightway solver, the live openLCA engine, `premise`-shifted prospective backgrounds) fold into ONE `MaterialImpact` carrier: an EN 15804 indicator × life-cycle-stage matrix keyed by `ContentIdentity`, discriminated on the source payload shape, never a provider knob. This owner owns only the normalization to the common carrier, the identity, the receipt, and the tabular egress — `openepd`/`epdx` own EPD wire parsing, `bw2data` owns the project graph as system of record, `bw2calc`/openLCA own the solver, and `MaterialImpact` is never the system of record.

Its self-describing eight-column frame crosses to the C# AEC domain as the seam `Discipline.Environmental` `Assessment` payload routed onto the `Material` node `MaterialPropertySet.Environmental` case — `Rasm.Compute` the assessment-runner owner, `Rasm.Materials` the material-node projection — the physical crossing content-keyed canonical Arrow bytes through the `tabular/columnar` public fold. Its solver cluster is pure-python and 3.15-clean: its function-local imports are optional-provider lazy loading, never a version gate — a run touching only the declaration arms never pays the Brightway/openLCA import — while `premise` alone keeps its manifest `<3.15` marker pending resolver evidence and `epdx` carries its `<1.0` pin on the 1.x wheel break. Transport endpoints arrive from the runtime `TransportResource` at the boundary, never re-minted here.

## [01]-[INDEX]

- [01]-[IMPACT]: the `ImpactSource` provider axis into the normalized `MaterialImpact` carrier, the one `_normalize` fold, the typed `ImpactReceipt`, the eight-column egress frame.

## [02]-[IMPACT]

- Owner: `MaterialImpact` reuses the `openepd` `LCIAMethod` value vocabulary as the canonical method axis, never re-declared; `Spread` gives mined dispersion a typed home on every `ImpactCell` (`rsd` declared, `std` sampled), so uncertainty is a carried slot, never an afterthought.
- Cases: the provider is recovered from the payload shape and the arity from the payload value; the `openepd` method selector `""` picks the deterministic minimum of the available `LCIAMethod`s.
- Entry: `gated(*rules)` composes the `tabular/contract#ADMISSION` gate downward over the eight-column frame and `profiled(profile)` grades the same frame through the `tabular/profile#PROFILE` plan; `wire` composes the consumer-edge crossing over the `tabular/columnar` public Arrow-bytes fold plus the carrier's `ContentKey`.
- Auto: `premise` supplies the future-year background LCI and never an LCIA of its own — the `premise_background` case scores through the same Brightway solve arm, keyed by its scenario tuple so identical prospective builds dedupe; a demand×method sweep rides the `Block` arity with each solve content-keyed, never a second arm.
- Receipt: the receipt keys over the source identity — declaration id+version, solve fingerprint, setup identity, scenario tuple — so re-ingestion or recompute of the same declaration dedupes in the `Rasm.Persistence` reuse ledger rather than recomputing; structured evidence on the one runtime rail, never product LCA state. `contribute` projects the one fixed-unit measure — the GWP A1A3 score — onto the runtime `Metrics.record` arm under `domain="impact"`, keyed by source; mixed-unit indicators stay receipt evidence, never a metric with an incoherent unit. The `_one` normalize span is the solver's only trace surface — embedded LCA engines carry no scrape surface, the `query`-plane law applied here.
- Packages: `bw2io`/`bw-processing` are the cluster's ingestion and matrix-datapackage substrate, composed by the graph owners; `pyarrow` binds function-local per the module-level ban.
- Growth: a new EPD format or compute engine is one `ImpactSource` case plus one `_normalize` arm; a new indicator one `Indicator` member plus one `INDICATOR_UNIT` row, with a provider correspondence row only where its field spelling drifts; a new stage one `Stage` row; a new egress shape one `_lower` arm. Staged rows: the EC3 OMF search stream (`epds.find`) when a consumer names search; `annotated_top_emissions` and the recursive-calculation prints as one contribution-kind row each; the `bw2calc.MultiLCA` shared-factorization batch when a consumer carries sweeps too wide for the per-solve arity; per-stage foreground/background splits beyond the aggregate `A1A3` when a consumer carries staged system boundaries.
- Boundary: never a per-provider `EpdImpact`/`LcaImpact` carrier split, never a second normalization kernel, never a re-implemented solver or sparse-matrix assembly; a frame missing its `source`/`declared_unit`/`content_key` columns is the rejected form — the C# decoder can neither attribute nor dedupe it.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import Nothing, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec import json as msgjson
from opentelemetry import trace

from rasm.data.tabular.columnar import arrow_bytes
from rasm.data.tabular.contract import ContractClaim, FrameAdmission, QualityRule
from rasm.data.tabular.interop import Backend, FieldShape, FrameInterop
from rasm.data.tabular.profile import ProfileReceipt, QualityProfile
from rasm.runtime.faults import Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import TransportResource

if TYPE_CHECKING:
    import pyarrow as pa
    from epdx.pydantic import EPD as IlcdEpd
    from olca_schema import CalculationSetup
    from openepd.model.epd import Epd
    from openepd.model.generic_estimate import GenericEstimate
    from openepd.model.industry_epd import IndustryEpd

# --- [TYPES] ----------------------------------------------------------------------------

_TRACER: Final = trace.get_tracer("rasm.data.impact")


class Stage(StrEnum):  # EN 15804 life-cycle modules (epdx ImpactCategory / openepd ScopeSet stage axis)
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


class Indicator(StrEnum):  # EN 15804+A2 impact + primary-energy + resource + waste/output-flow indicators
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


# --- [CONSTANTS] ------------------------------------------------------------------------

# canonical EN 15804+A2 unit per indicator — ONE table every arm reads; epdx carries no unit
# and openepd's ScopeSet subclass pins its own, so the row is the floor and the provider overrides.
INDICATOR_UNIT: Final[Map[Indicator, str]] = Map.of_seq([
    (Indicator.GWP, "kg CO2 eq"),
    (Indicator.GWP_FOSSIL, "kg CO2 eq"),
    (Indicator.GWP_BIOGENIC, "kg CO2 eq"),
    (Indicator.GWP_LULUC, "kg CO2 eq"),
    (Indicator.ODP, "kg CFC-11 eq"),
    (Indicator.AP, "mol H+ eq"),
    (Indicator.EP, "kg PO4 eq"),
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

# per-provider field correspondences DERIVE from the vocabulary; only drifting members override —
# epdx spells materials-for-recycling `mrf`; openepd spells exported energy `ee`/`eh` and abiotic
# depletion `ADP_mineral`/`ADP_fossil` (an un-overridden `adpe`/`adpf` never reaches a ScopeSet).
EPDX_FIELD: Final[Map[Indicator, str]] = Map.of_seq([(ind, {"mfr": "mrf"}.get(ind.value, ind.value)) for ind in Indicator])
OPENEPD_FIELD: Final[Map[Indicator, str]] = Map.of_seq([
    (ind, {"eee": "ee", "eet": "eh", "adpe": "ADP_mineral", "adpf": "ADP_fossil"}.get(ind.value, ind.value)) for ind in Indicator
])
OPENEPD_STAGE: Final[Map[Stage, str]] = Map.of_seq([(stage, "A1A2A3" if stage is Stage.A1A3 else stage.value.upper()) for stage in Stage])

FRAME_COLUMNS: Final[tuple[str, ...]] = ("source", "method", "indicator", "stage", "amount", "unit", "declared_unit", "content_key")

# structural claim over the fence-pinned wire: amount is the one float64 column, every other
# slot a non-null string — the FieldShape rows the contract gate proves before the wire crosses.
_WIRE_SHAPES: Final[tuple[FieldShape, ...]] = tuple(
    FieldShape(field=name, logical_type="Float64" if name == "amount" else "String", nullable=False) for name in FRAME_COLUMNS
)

_ENCODER: Final = msgjson.Encoder(order="deterministic")

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
    method: tuple[str, ...]
    indicator: Indicator = Indicator.GWP
    label: str = "EN 15978:2011"  # the LCIAMethod value carried as the carrier's method axis
    iterations: int = 0  # >0 samples the UNCERTAINTY_DTYPE rows: one Monte Carlo draw per next(lca)
    contributions: int = 0  # >0 mines bw2analyzer annotated_top_processes to this depth


class PremiseSolve(Struct, frozen=True):
    solve: LcaSolve
    scenario: tuple[str, str, int, str, str]  # (model, pathway, year, ecoinvent version, system_model)


class OlcaSolve(Struct, frozen=True):
    endpoint: str  # supplied by the runtime TransportResource axis at the boundary
    setup: "CalculationSetup"  # caller-authored via olca_schema new_* factories
    categories: Map[str, Indicator] = Map.empty()  # impact-category ref/name -> Indicator; a miss is a vocabulary-gap fault


@tagged_union(frozen=True)
class ImpactSource:
    tag: Literal["openepd", "ilcd_epd", "brightway", "openlca", "premise_background"] = tag()
    openepd: "tuple[Epd | IndustryEpd | GenericEstimate, str]" = case()  # (declaration, method label; "" = first available)
    ilcd_epd: "IlcdEpd" = case()
    brightway: LcaSolve = case()
    openlca: OlcaSolve = case()
    premise_background: PremiseSolve = case()


class ImpactReceipt(Struct, frozen=True):
    source: str
    method: str
    cell_count: int
    sampled: int
    content_key: ContentKey
    gwp_a1a3: float | None = None  # the one fixed-unit (kg CO2 eq) measure the metric spine grades; None when the carrier holds no GWP A1A3 cell

    def contribute(self) -> Iterable[Receipt]:
        # receipts stay truth, instruments stay projections: the GWP score lands on the runtime metric spine under domain="impact".
        if self.gwp_a1a3 is not None:
            Metrics.record({"rasm.impact.score": self.gwp_a1a3}, domain="impact", kind=self.source)
        yield Receipt.of("impact", ("emitted", self.source, {"method": self.method, "cells": self.cell_count, "sampled": self.sampled}))


class MaterialImpact(Struct, frozen=True):
    source: str
    method: str
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
        # ILCD admission projector: convert_ilcd panics as pyo3_runtime.PanicException — a
        # BaseException no Exception fence sees — so the kernel guards by qualname and re-raises
        # every other BaseException; the converted JSON lands the typed epdx model.
        def convert() -> ImpactSource:
            import epdx  # noqa: PLC0415 — banded boundary import
            from epdx.pydantic import EPD  # noqa: PLC0415

            text = document.decode() if isinstance(document, bytes) else document
            try:
                converted = epdx.convert_ilcd(text)
            except BaseException as panic:  # noqa: BLE001 — the PyO3 panic is BaseException-rooted
                if type(panic).__name__ != "PanicException":
                    raise
                raise ValueError("ilcd-parse-panic") from panic
            return ImpactSource(ilcd_epd=EPD.model_validate(msgjson.decode(converted.encode())))

        return boundary("impact.ilcd", convert)

    @staticmethod
    async def fetched(transport: TransportResource, uuid: str, method: str = "") -> "RuntimeRail[ImpactSource]":
        # live EC3 leg over the runtime transport axis — bearer + retry ride the resource; RootDocumentFactory routes the doctype.
        acquired = await transport.acquire(f"epds/{uuid}")
        return acquired.bind(
            lambda body: boundary(
                "impact.fetch",
                lambda: ImpactSource(openepd=(_doctyped(bytes(body)), method)),
            )
        )

    @classmethod
    def _one(cls, payload: "ImpactSource") -> "RuntimeRail[MaterialImpact]":
        # the sparse-matrix solve and declaration folds are heavy in-process legs with no scrape surface — the span IS
        # the engine observability; the boundary fence inside marks the span ERROR + record_exception on a failed leg.
        with _TRACER.start_as_current_span(f"impact.normalize.{payload.tag}", attributes={"rasm.impact.source": payload.tag}):
            return boundary(f"impact.normalize.{payload.tag}", lambda: _normalize(payload))

    def frame(self) -> "RuntimeRail[pa.Table]":
        return boundary("impact.frame", lambda: _lower(self))

    def wire(self) -> "RuntimeRail[tuple[bytes, ContentKey]]":
        # consumer-edge physical crossing: the canonical Arrow-bytes fold over the frame, the carrier key traveling with the bytes.
        return self.frame().map(lambda table: (bytes(arrow_bytes(table)), self.content_key))

    def gated(self, *rules: QualityRule) -> "RuntimeRail[ContractClaim]":
        # eight-column frame proves the `_WIRE_SHAPES` structural rows through the one contract gate — the claim records,
        # never raises, so a breached wire is a caller `match`, not an exception.
        admission = FrameAdmission.of(FrameInterop.of(Backend.PYARROW), _WIRE_SHAPES, *rules)
        return self.frame().bind(lambda table: admission.admit(table).bind(admission.enforce))

    async def profiled(self, profile: QualityProfile) -> "RuntimeRail[ProfileReceipt]":
        # caller-tuned pointblank plan grades the frame above the gate — pointblank's Narwhals engine admits the pa.Table
        # directly. The synchronous Arrow lowering crosses the band-bounded `on_thread` hop so the loop never hosts the
        # materialization; the frame rail short-circuits before the awaited interrogation and the composed signature
        # stays one rail, never a coroutine smuggled through `bind`.
        match await on_thread(self.frame):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=table):
                return await profile.interrogate(table)
            case _ as unreachable:
                assert_never(unreachable)

    def receipt(self) -> ImpactReceipt:
        sampled = max((cell.spread.map(lambda s: s.samples).default_value(0) for cell in self.cells), default=0)
        gwp = next((cell.amount for cell in self.cells if cell.indicator is Indicator.GWP and cell.stage is Stage.A1A3), None)
        return ImpactReceipt(
            source=self.source, method=self.method, cell_count=len(self.cells), sampled=sampled, content_key=self.content_key, gwp_a1a3=gwp
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _normalize(src: "ImpactSource") -> MaterialImpact:
    match src:
        case ImpactSource(tag="openepd", openepd=(decl, method)):
            return _from_openepd(decl, method)
        case ImpactSource(tag="ilcd_epd", ilcd_epd=epd):
            return _from_epdx(epd)
        case ImpactSource(tag="brightway", brightway=solve):
            return _from_score(solve, "brightway", _ENCODER.encode((solve.demand, solve.method)))
        case ImpactSource(tag="premise_background", premise_background=shifted):
            return _from_score(shifted.solve, "premise_background", _ENCODER.encode(shifted.scenario))
        case ImpactSource(tag="openlca", openlca=solve):
            return _from_olca(solve)
        case unreachable:
            assert_never(unreachable)


def _from_openepd(decl: "Epd | IndustryEpd | GenericEstimate", method: str) -> MaterialImpact:
    # ALL THREE ScopesetByNameBase containers fold: Impacts (method-keyed) + ResourceUseSet +
    # OutputFlowSet (direct containers) — an Impacts-only fold strands pere/penre/fw and hwd/nhwd/eee.
    chosen = method or min(str(m) for m in decl.impacts.available_methods())
    impact_set = decl.impacts.get_impact_set(chosen)
    containers = tuple(c for c in (impact_set, decl.resource_uses, decl.output_flows) if c is not None)

    def cells_of(container: object) -> Iterable[ImpactCell]:
        for indicator in Indicator:
            scopeset = container.get_scopeset_by_name(OPENEPD_FIELD[indicator])
            if scopeset is None:
                continue
            for stage in Stage:
                measurement = getattr(scopeset, OPENEPD_STAGE[stage], None)
                if measurement is None or measurement.mean is None:
                    continue
                spread = Some(Spread(kind="rsd", value=measurement.rsd)) if measurement.rsd is not None else Nothing
                yield ImpactCell(indicator=indicator, stage=stage, amount=measurement.mean, unit=measurement.unit or INDICATOR_UNIT[indicator], spread=spread)

    cells = tuple(cell for container in containers for cell in cells_of(container))
    key = ContentIdentity.key("impact", f"{decl.id}:{decl.version}".encode())
    return MaterialImpact(source="openepd", method=str(chosen), declared_unit=str(decl.declared_unit), cells=cells, content_key=key)


def _from_epdx(epd: "IlcdEpd") -> MaterialImpact:
    # `epd.standard` names which subset an A1-vs-A2 declaration populates — the Optional walk skips the rest.
    cells = tuple(
        ImpactCell(indicator=ind, stage=stg, amount=val, unit=INDICATOR_UNIT[ind])
        for ind in Indicator
        if (category := getattr(epd, EPDX_FIELD[ind], None)) is not None
        for stg in Stage
        if (val := getattr(category, stg.value, None)) is not None
    )
    key = ContentIdentity.key("impact", f"{epd.id}:{epd.published_date}:{epd.version}".encode())
    return MaterialImpact(source="ilcd_epd", method=epd.standard.value, declared_unit=epd.declared_unit.value, cells=cells, content_key=key)


def _from_score(solve: LcaSolve, source: str, identity: bytes) -> MaterialImpact:
    # staged bw2calc solve at mined depth: lci/lcia/score -> the aggregate A1A3 cell; Monte
    # Carlo one draw per next(lca); bw2analyzer contribution rows when the request carries depth.
    import bw2calc as bc  # noqa: PLC0415 — banded boundary import
    import bw2data as bd  # noqa: PLC0415

    demand = dict(solve.demand)
    fu, data_objs, remapping = bd.prepare_lca_inputs(demand=demand, method=solve.method)
    lca = bc.LCA(demand=fu, data_objs=data_objs, remapping_dicts=remapping, use_distributions=solve.iterations > 0)
    lca.lci()
    lca.lcia()
    unit = str(bd.Method(solve.method).metadata.get("unit", INDICATOR_UNIT[solve.indicator]))
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
        from bw2analyzer import ContributionAnalysis  # noqa: PLC0415 — banded boundary import

        rows = tuple(
            ContributionRow(score=score, supply=supply, activity=str(activity))
            for score, supply, activity in ContributionAnalysis().annotated_top_processes(lca, limit=solve.contributions)
        )
    cell = ImpactCell(indicator=solve.indicator, stage=Stage.A1A3, amount=float(lca.score), unit=unit, spread=spread)
    key = ContentIdentity.key("impact", identity)
    return MaterialImpact(source=source, method=solve.label, declared_unit=unit, cells=(cell,), content_key=key, contributions=rows)


def _from_olca(solve: OlcaSolve) -> MaterialImpact:
    # documented lifecycle: setup -> calculate -> wait_until_ready -> query -> dispose (finally).
    import olca_ipc as ipc  # noqa: PLC0415 — banded boundary import

    client = ipc.Client(solve.endpoint)
    result = client.calculate(solve.setup)
    try:
        state = result.wait_until_ready()
        if state.error:
            raise ValueError(f"olca-state:{state.error}")
        rows = result.get_total_impacts()
    finally:
        result.dispose()

    def cell_of(row: object) -> ImpactCell:
        name = row.impact_category.name if row.impact_category else "?"
        found = solve.categories.try_find(row.impact_category.id if row.impact_category else "").or_else(solve.categories.try_find(name))
        if found.is_none():
            raise ValueError(f"olca-category-gap:{name}")  # a vocabulary gap surfaces at admission, never a silent skip
        ind = found.value
        return ImpactCell(indicator=ind, stage=Stage.A1A3, amount=float(row.amount or 0.0), unit=INDICATOR_UNIT[ind])

    cells = tuple(cell_of(row) for row in rows)
    target = solve.setup.target.id if solve.setup.target else "?"
    method_ref = solve.setup.impact_method.id if solve.setup.impact_method else "?"
    key = ContentIdentity.key("impact", f"{target}:{method_ref}:{solve.setup.amount}".encode())
    return MaterialImpact(source="openlca", method=method_ref, declared_unit="", cells=cells, content_key=key)


def _doctyped(body: bytes) -> "Epd | IndustryEpd | GenericEstimate":
    from openepd.model.factory import RootDocumentFactory  # noqa: PLC0415 — boundary import (patch_pydantic side effect rides it)

    return RootDocumentFactory.from_dict(msgjson.decode(body))


def _lower(impact: MaterialImpact) -> "pa.Table":
    # eight-column SELF-DESCRIBING floor: a frame the C# Discipline.Environmental Assessment
    # decode attributes (source/method/declared_unit) and dedupes (content_key) with no side channel.
    import pyarrow as pa  # noqa: PLC0415 — module-level import banned; deferred at the egress edge

    key = impact.content_key.hex
    return pa.Table.from_pydict({
        "source": [impact.source] * len(impact.cells),
        "method": [impact.method] * len(impact.cells),
        "indicator": [cell.indicator.value for cell in impact.cells],
        "stage": [cell.stage.value for cell in impact.cells],
        "amount": [cell.amount for cell in impact.cells],
        "unit": [cell.unit for cell in impact.cells],
        "declared_unit": [impact.declared_unit] * len(impact.cells),
        "content_key": [key] * len(impact.cells),
    })
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
