# [PY_DATA_IMPACT]

The material environmental-impact owner — the EPD/LCA normalization plane of `data`, and the composing owner of the `openepd`/`epdx`/`bw2data`/`bw2calc`/`bw2io`/`bw-processing`/`bw2analyzer`/`olca-ipc`/`premise` catalogs. It folds two external EPD declaration formats (`openepd` OpenEPD/EC3 payloads, `epdx` ILCD+EPD documents) and three life-cycle-assessment compute legs (the Brightway solver `bw2calc` over the `bw2data` graph and `bw-processing` matrix datapackages, the live openLCA engine `olca-ipc`, and `premise`-shifted prospective backgrounds) into ONE `MaterialImpact` carrier: an EN 15804 indicator × life-cycle-stage matrix keyed by `ContentIdentity`. The owner is the single normalization seam — it discriminates on the source payload shape (never a provider knob), maps each provider's native impact shape onto one `(indicator, stage) -> amount + unit + spread` cell stream, contributes a typed `ImpactReceipt` on the runtime receipt rail, and lowers to the SELF-DESCRIBING eight-column frame (`source`/`method`/`indicator`/`stage`/`amount`/`unit`/`declared_unit`/`content_key`) the `tabular/contract` and `tabular/profile` rails consume and the C# AEC domain ingests as the seam `Discipline.Environmental` `Assessment` payload routed onto the `Material` node `MaterialPropertySet.Environmental` case — `Rasm.Compute` the assessment-runner owner, `Rasm.Materials` the material-node projection — decoded against the seam vocabulary, never a re-minted impact shape, the physical crossing content-keyed canonical Arrow bytes through the `tabular/columnar` public fold composed at the consumer edge.

The unit law is row data: `epdx`'s `ImpactCategory` carries NO unit while `openepd`'s per-indicator `ScopeSet` subclass pins its own, so the `Indicator` vocabulary carries the canonical EN 15804+A2 unit as one `INDICATOR_UNIT` table every arm reads — covering the core, resource-use, and output-flow families end to end, the GWP fossil/biogenic/luluc split included — and a provider reporting an indicator the table lacks surfaces as a vocabulary gap at admission, never a silent empty unit. Uncertainty is a carried slot, not an afterthought: an `ImpactCell` holds an `Option[Spread]` — the openepd `Measurement.rsd` relative deviation on a declared cell, the Monte Carlo sample deviation on a Brightway cell — so mined dispersion has a typed home. It never re-implements the LCA solver (`bw2calc`/openLCA own it), never re-mints the project graph (`bw2data` is the system of record), never authors EPD wire parsing (`openepd`/`epdx` own it), and never holds the model as the system of record — it owns the normalization to the common carrier, the identity, the receipt, and the tabular egress. The heavy solver cluster (bw2calc/bw2data/bw2io/bw2analyzer, olca-ipc) is pure-python and 3.15-clean — its function-local imports are OPTIONAL-PROVIDER lazy loading, never a version gate: a run that touches only the declaration arms never pays the Brightway/openLCA import, while `premise` alone keeps its manifest `<3.15` marker pending resolver evidence and loads function-local so the cp315 lane stays clean.

## [01]-[INDEX]

- [01]-[IMPACT]: the material environmental-impact owner — the `ImpactSource` provider axis, the normalized EN 15804 indicator × stage `MaterialImpact` carrier with the unit-row and spread laws, the one `_normalize` fold with four realized arms, the typed `ImpactReceipt`, and the eight-column self-describing egress frame.

## [02]-[IMPACT]

- Owner: `MaterialImpact` — the normalized EN 15804 indicator × life-cycle-stage carrier every provider folds into; it carries the recovered source tag, the `method` label (the `openepd` `LCIAMethod` value vocabulary reused as the canonical method axis, never re-declared), the declared unit, the `(indicator, stage) -> amount + unit + spread` `ImpactCell` stream, the `Option`-carried contribution rows a mined Brightway leg attaches, and the `ContentKey`. `ImpactSource` is the closed provider tagged union; `Indicator`/`Stage` are the closed EN 15804 vocabularies with `INDICATOR_UNIT` the canonical unit row every arm reads; `Spread` the typed uncertainty evidence (`rsd` declared, `std` sampled); `ImpactReceipt` the typed `ReceiptContributor` evidence keyed by `ContentIdentity`. A new EPD format or compute engine is one `ImpactSource` case plus one `_normalize` arm, a new indicator one `Indicator` member plus one `INDICATOR_UNIT` row, a new egress one `_lower` arm — never a per-provider `EpdImpact`/`LcaImpact` carrier split, never a `from_openepd`/`from_brightway` ingest family on the public surface, never a second normalization kernel.
- Cases: `ImpactSource` is the one provider axis closed by `assert_never` — `openepd` (a declaration plus its method selector; `""` picks the deterministic minimum of the available `LCIAMethod`s), `ilcd_epd` (an `epdx.pydantic.EPD`), `brightway` (an `LcaSolve` request the arm SOLVES — demand pairs, the `bw2data` method tuple, the target `Indicator`, the Monte Carlo iteration count, the contribution depth), `openlca` (an `OlcaSolve` request — the server endpoint the runtime `TransportResource` axis supplied, the caller-authored `olca_schema` `CalculationSetup`, and the category->`Indicator` correspondence as request data), `premise_background` (a `PremiseSolve` — the same `LcaSolve` over a premise-shifted database already written to the Brightway project, keyed by its `(model, pathway, year, ecoinvent version, system_model)` scenario). The provider is recovered from the payload shape, never a `provider=` knob; arity (lone vs `Block`) is recovered from the payload value, never an `of_many` sibling.
- Entry: `MaterialImpact.of` is the one modality-polymorphic ingest — it absorbs a lone `ImpactSource` or a `Block[ImpactSource]` over one `match` head, threads the private `_normalize` fold through the `boundary` fence, and returns `RuntimeRail[MaterialImpact]` (lone) or the `Disposition`-keyed batch through `traversed` (`ABORT`/`ACCUMULATE`/`PARTITION` exactly as the `graph/graph#GRAPH` `analyze` and `gridded/store#STORE` `write_region` arms), because the content-key seam and every solve leg are fallible. `MaterialImpact.ilcd` is the ILCD admission projector: `epdx.convert_ilcd(json)` (the ONE Rust-core entry — no `as_type`, no typed parse exception in the `0.3.0` line) converts inside a qualname-guarded kernel — the Rust core panics as `pyo3_runtime.PanicException`, a `BaseException` subclass no `Exception` fence sees, so the guard re-raises any non-`PanicException` `BaseException` and maps the panic to the parse fault — then `epdx.pydantic.EPD.model_validate` lands the typed model as an `ilcd_epd` source. `MaterialImpact.fetched` is the live EC3 leg: the runtime `TransportResource.acquire(f"epds/{uuid}")` moves the declaration bytes (bearer credential and retry class on the transport axis — the `impact ← python:runtime [PORT]` seam composed, not claimed), and `RootDocumentFactory.from_dict` routes the doctype (`Epd`/`IndustryEpd`/`GenericEstimate`) through the one factory — the OMF search stream (`OpenEpdApiClientSync(...).epds.find`) stays a growth row until a consumer names search. `frame` lowers the carrier to the eight-column `pa.Table`; `wire` composes the consumer-edge crossing — the `tabular/columnar` public Arrow-bytes fold over the frame plus the carrier's `ContentKey`; `receipt` mints the typed `ImpactReceipt`. `MaterialImpact.gated(*rules)` composes the `tabular/contract#ADMISSION` gate downward — the `_WIRE_SHAPES` structural rows proved over the eight-column frame, the recorded `ContractClaim` the outcome — and `MaterialImpact.profiled(profile)` grades the same frame through the `tabular/profile#PROFILE` plan; both are the formerly-declared gate/quality hand-offs realized as fence imports now that contract and profile sit strictly earlier in the module order.
- Auto: `_normalize` is ONE fold — each arm maps the provider's native shape onto the cell stream at the depth the catalogs verify. `_from_openepd` folds all THREE `ScopesetByNameBase` containers — `Impacts` (`RootModel[dict[LCIAMethod, ImpactSet]]`: `available_methods()` enumerates, `get_impact_set(method)` selects, `get_scopeset_by_name(field)` yields the per-indicator `ScopeSet`) PLUS `ResourceUseSet` (`decl.resource_uses`) and `OutputFlowSet` (`decl.output_flows`), the two direct containers whose pere/penre/fw and hwd/nhwd/eee families the `Indicator` vocabulary claims — an `Impacts`-only fold leaves them unreachable; the stage axis reads the UPPERCASE `ScopeSet` fields through the `OPENEPD_STAGE` correspondence (`A1A3 -> "A1A2A3"`), each populated `Measurement` lands `(mean, unit-or-row, rsd->Spread)`, and the key is `(decl.id, decl.version)` — the `open_xpd_uuid` identity rides the `id` field. `_from_epdx` walks the indicator × stage matrix through the `EPDX_FIELD` correspondence (identity except `MFR -> "mrf"`, the one epdx field drift) with the REAL unit per cell from `INDICATOR_UNIT` (the `unit=""` form is dead), reading `epd.standard` for the method label and keying `(id, published_date, version)`. `_from_score` REALIZES the Brightway leg: `bw2data.prepare_lca_inputs(demand, method)` bridges the graph, the staged `LCA(demand=fu, data_objs=..., remapping_dicts=...)` runs `lci()`/`lcia()`/`.score` into the aggregate `A1A3` cell for the solve's target `Indicator` with the method's own unit read from `bw2data.Method(method).metadata`, a non-zero `iterations` re-samples through the Monte Carlo iteration protocol (`use_distributions=True`, `keep_first_iteration()`, one sample per `next(lca)`) folding the sample deviation into the cell's `Spread`, and a non-zero `contributions` mines `bw2analyzer.ContributionAnalysis().annotated_top_processes(lca, limit=n)` into typed `(score, supply, activity)` rows on the carrier — a single aggregate cell from a five-package cluster is the coverage floor this depth replaces; a demand×method sweep rides the `Block[ImpactSource]` arity (each solve content-keyed so the reuse ledger elides repeats), never a second arm — `bw2calc.MultiLCA`'s shared-factorization batch is the named growth row for wide sweeps. `_from_olca` REALIZES the documented lifecycle — `Client(endpoint)` -> `calculate(setup)` -> `wait_until_ready()` (a `ResultState.error` is the typed fault) -> `get_total_impacts()` -> `dispose()` in the `finally` (a skipped dispose leaks server memory) — each `ImpactValue` row resolving its `Indicator` through the solve's category correspondence, an unmapped category a vocabulary-gap fault naming the category, the stage the aggregate `A1A3`, the key the setup identity (target ref + method ref + amount). The `premise_background` case scores the premise-shifted database through the SAME solve arm — `premise` supplies the future-year background LCI (`NewDatabase(...).update().write_db_to_brightway(name)` is its offline build motion, self-parallelizing, never wrapped in an outer pool), never an LCIA of its own — keyed by the scenario tuple so identical prospective builds dedupe.
- Receipt: `ImpactReceipt.contribute` emits one `emitted`-phase `Receipt.of("impact", ("emitted", source, {...}))` keyed by `ContentIdentity` over the source identity — `openepd` `id`+version, `epdx` `id`+`published_date`+`version`, a `bw2calc` demand/method fingerprint, an openLCA setup identity, a `premise` scenario tuple — so re-ingestion or recompute of the same declaration or setup dedupes in the `Rasm.Persistence` reuse ledger rather than recomputing. The receipt is structured evidence on the one runtime receipt rail, never product LCA state.
- Packages: `openepd` (`RootDocumentFactory.from_dict` the one doctype parse; `Impacts.available_methods`/`get_impact_set`, `ScopesetByNameBase.get_scopeset_by_name` over `ImpactSet`/`ResourceUseSet`/`OutputFlowSet`; the per-indicator `ScopeSet` subclasses with UPPERCASE stage fields (`A1A2A3`..`D`) of `Measurement(mean, unit, rsd, dist)`; `LCIAMethod` the canonical method vocabulary; the `import openepd` `patch_pydantic` side effect is a shared-`pydantic`-process fact the boundary import order accounts for), `epdx` (`convert_ilcd(json) -> str` the sole Rust-core export — panics `pyo3_runtime.PanicException` on malformed input, a `BaseException` outside the `Exception` fence; `epdx.pydantic.EPD`/`ImpactCategory` with the `mrf` field spelling; `<1.0` pin on the 1.x wheel break), `bw2data` (`projects.set_current`, `prepare_lca_inputs`, `Method(...).metadata` the unit source), `bw2calc` (`LCA` staged `lci`/`lcia`/`score`, `use_distributions`/`keep_first_iteration` with one draw per `next(lca)` — the LCA exposes `__next__` alone), `bw2analyzer` (`ContributionAnalysis.annotated_top_processes`/`annotated_top_emissions` the contribution mine; BSD-3-Clause), `bw2io`/`bw-processing` (ingestion and the COO datapackage substrate — composed by the graph owners, named here as the cluster's system of record), `olca-ipc` (`Client`/`ProtoClient`, `calculate`/`wait_until_ready`/`get_total_impacts`/`dispose`; `olca_schema` `CalculationSetup`/`ImpactValue`/`Ref` authored offline by the caller), `premise` (`NewDatabase(...).update().write_db_to_brightway`, `IncrementalDatabase`, `PathwaysDataPackage` — the offline prospective build; the one row still manifest-marked `<3.15` pending resolver evidence), runtime (`ContentIdentity`/`ContentKey` via `rasm.runtime.identity`, `RuntimeRail`/`boundary`/`traversed`/`Disposition`, `TransportResource` the EC3/openLCA endpoint axis, `Receipt`/`ReceiptContributor`), data (`tabular/columnar` public Arrow-bytes fold — the one canonical crossing serialization), `pyarrow` (`Table.from_pydict` the frame lift, banned at module level), `expression` (`Block`/`Map`/`Option`/`tagged_union` — the `Map` rail owns every dispatch/correspondence table; the importable frozen-mapping import is the deleted contrast).
- Growth: a new EPD format is one `ImpactSource` case + one `_normalize` arm; a new compute engine the same; a new indicator one `Indicator` member + one `INDICATOR_UNIT` row (+ a provider correspondence row only where its field spelling drifts); a new stage one `Stage` row; a new egress shape one `_lower` arm; the EC3 OMF search stream (`epds.find` over `MaterialFilterDefinition`) is one fetch arm when a consumer names search; `annotated_top_emissions` and the `bw2analyzer` recursive-calculation prints are one contribution-kind row each; the `bw2calc.MultiLCA` shared-factorization batch (with `bw2data.get_multilca_data_objs` bridging) is one solve-request row when a consumer carries demand×method sweeps too wide for the per-solve arity; per-stage foreground/background splits beyond the aggregate `A1A3` land as solve-request rows when a consumer carries staged system boundaries — never a per-provider carrier, never a second normalization kernel, never a `provider=` knob where the payload shape recovers it.
- Boundary: `openepd`/`epdx` own EPD wire parsing; `bw2data` owns the project graph (system of record); `bw2calc`/openLCA own the solver; `bw-processing` owns the matrix datapackage; `bw2io` owns ingestion; `premise` owns the prospective transform; this owner owns ONLY the normalization to the common carrier, the identity, the receipt, and the tabular egress. The transport endpoints (EC3 for `openepd`, the openLCA server for `olca-ipc`) are supplied by the runtime `TransportResource` at the boundary, never re-minted here. The deleted forms: a per-provider `EpdImpact`/`LcaImpact` model split; a hand-rolled ILCD/OpenEPD parser where the catalogs own it; a re-implemented LCA solver or sparse-matrix assembly where `bw2calc`/`scipy` own it; a `provider=`/`mode=` knob where the payload shape recovers the case; treating `MaterialImpact` as the system of record; an `Impacts`-only openepd fold that strands the resource-use and output-flow families; an empty-unit cell where `INDICATOR_UNIT` carries the row; a mined spread with no carrier slot; an `Exception`-typed fence around `convert_ilcd` that the PyO3 panic sails past; a skipped `Result.dispose()`; wrapping `premise.update()` in an outer process pool where it self-parallelizes; a frozen-mapping dispatch import where the `expression` `Map` rows own the fold; and a frame missing its `source`/`declared_unit`/`content_key` columns — the C# decoder can neither attribute nor dedupe it.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec import json as msgjson

from rasm.data.tabular.columnar import arrow_bytes
from rasm.data.tabular.contract import ContractClaim, FrameAdmission, QualityRule
from rasm.data.tabular.interop import Backend, FieldShape, FrameInterop
from rasm.data.tabular.profile import ProfileReceipt, QualityProfile
from rasm.runtime.faults import Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey
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

# the canonical EN 15804+A2 unit per indicator — ONE table every arm reads; epdx carries no unit
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

# the structural claim over the fence-pinned wire: amount is the one float64 column, every other
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

    def contribute(self) -> Iterable[Receipt]:
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
        # one modality-polymorphic ingest: arity recovers from the payload shape, the provider from
        # the ImpactSource case — never a from_openepd/from_brightway family, never a provider= knob.
        match payload:
            case Block() as many:
                return traversed(many.map(cls._one), by=by)
            case lone:
                return cls._one(lone)

    @staticmethod
    def ilcd(document: "str | bytes") -> "RuntimeRail[ImpactSource]":
        # the ILCD admission projector: convert_ilcd panics as pyo3_runtime.PanicException — a
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
        # the live EC3 leg over the runtime transport axis — bearer + retry ride the resource,
        # RootDocumentFactory routes the doctype; search (epds.find over OMF) stays a growth row.
        acquired = await transport.acquire(f"epds/{uuid}")
        return acquired.bind(
            lambda body: boundary(
                "impact.fetch",
                lambda: ImpactSource(openepd=(_doctyped(bytes(body)), method)),
            )
        )

    @classmethod
    def _one(cls, payload: "ImpactSource") -> "RuntimeRail[MaterialImpact]":
        return boundary(f"impact.normalize.{payload.tag}", lambda: _normalize(payload))

    def frame(self) -> "RuntimeRail[pa.Table]":
        return boundary("impact.frame", lambda: _lower(self))

    def wire(self) -> "RuntimeRail[tuple[bytes, ContentKey]]":
        # the consumer-edge physical crossing: the one canonical Arrow-bytes fold over the frame,
        # the carrier key traveling WITH the bytes — the C# Assessment decode attributes and dedupes.
        return self.frame().map(lambda table: (bytes(arrow_bytes(table)), self.content_key))

    def gated(self, *rules: QualityRule) -> "RuntimeRail[ContractClaim]":
        # the declared contract-gate hand-off, composed downward now that contract is strictly
        # earlier in the module order: the eight-column frame proves the `_WIRE_SHAPES` structural
        # rows (plus any caller `QualityRule` rows) through the ONE contract gate — the claim
        # records, never raises, so a breached wire is a caller `match`, not an exception.
        admission = FrameAdmission.of(FrameInterop.of(Backend.PYARROW), _WIRE_SHAPES, *rules)
        return self.frame().bind(lambda table: admission.admit(table).bind(admission.enforce))

    def profiled(self, profile: QualityProfile) -> "RuntimeRail[ProfileReceipt]":
        # the declared profile-frame hand-off: the caller-tuned pointblank plan grades the live
        # assessment frame above the gate — pointblank's Narwhals engine admits the pa.Table directly.
        return self.frame().bind(profile.interrogate)

    def receipt(self) -> ImpactReceipt:
        sampled = max((cell.spread.map(lambda s: s.samples).default_value(0) for cell in self.cells), default=0)
        return ImpactReceipt(source=self.source, method=self.method, cell_count=len(self.cells), sampled=sampled, content_key=self.content_key)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _normalize(src: "ImpactSource") -> MaterialImpact:
    # ONE fold: each provider's native impact shape maps onto the (indicator, stage) cell stream;
    # the per-provider extraction is owned by the cited catalogs — each arm adapts, never re-parses.
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
    # indicator × stage walk through the EPDX_FIELD correspondence; the epdx model carries NO unit,
    # so every cell reads the INDICATOR_UNIT row; epd.standard names which subset an A1-vs-A2
    # declaration populates — the Optional walk skips the rest.
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
    # the staged bw2calc solve at mined depth: lci/lcia/score -> the aggregate A1A3 cell; Monte
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
    # the documented lifecycle: setup -> calculate -> wait_until_ready -> query -> dispose (finally);
    # an unmapped impact category is a vocabulary-gap fault at admission, never a silent skip.
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
    # the eight-column SELF-DESCRIBING floor: a frame the C# Discipline.Environmental Assessment
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
