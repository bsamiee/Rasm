# [PY_DATA_DECLARATION]

The declaration-registry ingest owner — the product-declaration plane of `data`, sibling to the impact normalization plane and minting what that plane structurally cannot: a DATED, IDENTITY-BEARING record per verified product declaration. `impact.md`'s eight-column frame stays impact-only by design — per-(indicator, stage) amounts with no issuer, no dates, no coverage census — and this owner carries the declaration NOUN whole: material identity, issuer + registration, declared unit, issue and expiry dates, and the per-indicator per-module cell map whose KEY PRESENCE is the coverage census. The record is the `libs/contracts/manifest.json` `declaration-record` DOMAIN contract; this page is its one producer, `dotnet:Rasm.Materials` assessment its committed decoder, and the schema definition owns every roster — this page transcribes, never re-declares.

Registries are POLICY VALUES, never arms. `IngestPolicy` carries one row per contract `Registry` token — the token it stamps, the payload case that registry publishes, and the transport resource its live leg reads — so the two ILCD registries reach the epdx reader whole and differ in data alone.

`DeclarationIngress` keeps the one axis a body genuinely turns on, the payload SHAPE: an Ökobaudat/soda4LCA ILCD+EPD document parses through the shared `ilcd_document` guard over `epdx.convert_ilcd`, an EC3/OpenEPD typed model through `openepd`. Every contract slot then elects on its own rail into `Admitted`, whose field roster IS the census roster, and the census folds under `Disposition.ACCUMULATE` so one malformed declaration names every offending column at once.

Transport endpoints arrive from the runtime `TransportResource` at the boundary; parser imports bind function-local per the module-level ban, so a run touching no registry pays no parser import.

## [01]-[INDEX]

- [02]-[DECLARATION]: the corpus `ImpactCategory`/`Module` rosters and the reader reach they derive, the `IngestPolicy` registry rows, the `DeclarationIngress` payload axis, the `Keying` row, the generated `DeclarationRecord` mint, the `Admitted` slot census, the proto-binary `wire` crossing, and the `DeclarationReceipt` coverage census.

## [02]-[DECLARATION]

- Owner: the record IS the generated `rasm.contracts.declaration.DeclarationRecord` — every closed vocabulary the corpus declares TYPES its field as the generated enum (`Registry`, `DeclaredUnit`, `Standard`, `Subtype`, `ImpactCategory`, `Module`), the cell matrix is `repeated ImpactCell{category, stage, value}` whose PRESENCE is the coverage census, the dates are `google.type.Date` cells, and each enum's declaration ORDER is the corpus's frozen order, so the generated enum is the ordering authority the cell fold walks rather than a roster standing beside it; this page mints no struct, no schema, and no JSON twin. `Keying` binds one registry identity to one estate `material_key`.
- Cases: `DeclarationIngress` discriminates the payload shape alone — `ilcd` (an Ökobaudat/soda4LCA ILCD+EPD document, `str | bytes`) and `openepd` (a typed EC3 declaration) — closed on `assert_never`, never a provider knob; WHICH registry a payload came from is the `IngestPolicy` value the caller hands beside it, so a new registry on an existing payload shape costs one row and no body.
- Entry: `MaterialDeclaration.of(payload, policy, keying, *, by=Disposition.ABORT)` is the one normalization entry, polymorphic over one ingress or a `Block` exactly as the impact plane's `of`; `MaterialDeclaration.fetched(transport, policy, uuid)` is the live leg reading the registry's own resource row off the runtime transport, refusing typed for a registry whose policy declares no live leg; `wire(record)` composes the consumer-edge crossing — the record's proto binary beside the `ContentKey` minted over its ProtoJSON projection, the declaration-ordered, map-free text every branch renders identically, never over the non-canonical binary.
- Auto: coverage is NEVER fabricated — a parser field holding `None` writes no cell, and the census a consumer reads is exactly the key set. What key presence alone CANNOT say is why a cell is missing, so each reader's capability bound is a DECLARED row: `EPDX_REACH` names the six contract indicators the A1-shaped epdx model can carry and thereby names the split GWP/EP rows and `wdp` UNREACHABLE for that reader, openepd's `ImpactSet` answers the +A2 core roster whole so its bound is the vocabulary entire and its census reports no unreachable slot, `READERS` keys those bounds by payload case, and `DeclarationReceipt` reports declared, undeclared, and unreachable slots apart. Admission refuses rather than defaults: an `UNKNOWN` declared unit or standard refuses because the roster carries no member for it, an absent or blank issuer refuses because the contract's `minLength: 1` admits no blank half of the duplicate-check pair, a record with no declared cell refuses on the contract's `minProperties: 1`, and a record missing issue or expiry dates refuses because a dateless declaration cannot enter a resolution law that gates on expiry. Every one of those elects on its OWN rail, so a declaration with a bad unit and two bad dates names three columns in one aggregate fault rather than the first to trip.
- Receipt: `DeclarationReceipt` keys on the registry identity — registry, uuid, version, material key, and the three-way cell census — so re-ingestion of one registration dedupes in the reuse ledger rather than re-fetching; structured evidence on the one runtime rail, no new metric family minted — declaration ingest is evidence-plane traffic and the impact plane's `domain="impact"` measure stays the branch's one environmental metric row.
- Packages: `epdx` parses the ILCD+EPD wire (`convert_ilcd` + the `epdx.pydantic` EN 15804 model), `openepd` the EC3 typed model, `rasm.contracts` the generated record, its enums, and the `Date` cell, `protobuf-py` the `to_binary`/`to_json` pair whose encode-time validation is the one raise surface the wire fence names; runtime rails, identity, lanes, and transport compose as everywhere, and the `pyo3` panic on malformed input crosses `impact/impact#IMPACT`'s `ilcd_document` — the package's one qualname guard, composed rather than restated, because the panic class is unnameable and a `catch` set alone never held it. No Brightway, no openLCA — a declaration is a published record, never a solve.
- Growth: a new registry is one `IngestPolicy` row naming its token, its payload case, and its live resource — never a new body; a new refusal law is one `FaultRow` row on this module's `RAISES` table, and a slot whose law an existing parameterized row already holds costs no row at all, only its subject at the raise; a new PAYLOAD SHAPE is one `DeclarationIngress` case with one reader and one `READERS` reach row, and the total `match` breaks loudly until it lands; a new correspondence is one DIVERGENCE — the reader's reach set states which indicators it reaches and the provider spelling derives from the contract token, so only a drifting spelling costs a row; a schema field is a CONTRACT change first — the corpus definition moves, then this struct transcribes, in that order.
- Boundary: this page mints no `MaterialImpact` and the impact plane mints no declaration — two nouns, two owners, one shared source ecosystem — and the one thing it does reach across for is `ImpactRegime`'s characterization-method preference order, because which method carries an edition's core roster is edition policy the impact plane already owns and a second order here would be the fork that ruling forecloses; the record shape is the corpus message's — the `protovalidate` rules on it (`min_len`, `finite`, the `[0, 1]` fractions, `enum.defined_only`) are declared at the corpus and this page's admission rails enforce the same laws on the rail before construction, since the Python runtime validates at encode alone; registry licence custody stays with the consuming estate's settled clearance (Ökobaudat's redistribution licence, recorded at the Materials counterpart) and this page adds no catalogue store. Three contract slots have no like-for-like `openepd` member and each ELECTS on a stated ground the catalog's `[DECLARATION_FIELD_SCOPE]` rows carry: the contract's issuer is the programme operator, so `program_operator` answers it and `manufacturer` never does; representativeness has no field at all in the distribution, so the product `doctype` answers it and every other doctype refuses rather than being stamped specific; and the standard has no enum, so a free `compliance` spelling folds to the roster token or refuses naming every spelling it saw. Deleted forms — a per-registry record class or normalize arm, a provider knob on `of`, a second mapping table beside a roster whose member names already agree, an `if/else` electing a standard for a declaration that declares none, a zero written into an undeclared cell, a blank written into a required identity half, a fabricated date, a keying inferred from name matching, a date rendered to text and re-parsed when the reader already hands a `datetime`, a declared unit admitted without its quantity, an impact set folded without its characterization method pinned, split `A1`/`A2`/`A3` summed into the production cell, and a provenance identity aliased off the registration on a rail where the two genuinely part.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import datetime as dt
from collections.abc import Generator
from dataclasses import dataclass, fields
from enum import Enum
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from rasm.contracts.google.type.date_pb import Date
from rasm.contracts.rasm.contracts.declaration.declaration_pb import (
    DeclarationRecord, DeclaredUnit, ImpactCategory, ImpactCell, Module, Registry, Source, Standard, Subtype,
)

from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, TRANSIENT, Catch, Disposition, FaultRow, RuntimeRail, boundary, railed, rostered, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.roots import Delivery, TransportResource

if TYPE_CHECKING:
    from epdx.pydantic import EPD as IlcdEpd
    from openepd.model.common import Amount
    from openepd.model.epd import Epd
    from openepd.model.standard import Standard as Compliance

# --- [TYPES] ----------------------------------------------------------------------------

type IngressTag = Literal["ilcd", "openepd"]

_CATEGORIES: Final[tuple[ImpactCategory, ...]] = tuple(member for member in ImpactCategory if member is not ImpactCategory.UNSPECIFIED)
_STAGES: Final[tuple[Module, ...]] = tuple(member for member in Module if member is not Module.UNSPECIFIED)


# --- [CONSTANTS] ------------------------------------------------------------------------

EPDX_REACH: Final[frozenset[ImpactCategory]] = frozenset({
    ImpactCategory.GWP_TOTAL, ImpactCategory.ODP, ImpactCategory.AP,
    ImpactCategory.POCP, ImpactCategory.ADP_MINERALS, ImpactCategory.ADP_FOSSIL})

OPENEPD_REACH: Final[frozenset[ImpactCategory]] = frozenset(_CATEGORIES)

READERS: Final[Map[IngressTag, frozenset[ImpactCategory]]] = Map.of_seq([("ilcd", EPDX_REACH), ("openepd", OPENEPD_REACH)])


def _token(member: Enum) -> str:
    return member.name.lower().replace("_", "-")


EPDX_INDICATOR: Final[Map[ImpactCategory, str]] = Map.of_seq(
    (indicator, {"gwp-total": "gwp", "adp-minerals": "adpe", "adp-fossil": "adpf"}.get(_token(indicator), _token(indicator)))
    for indicator in _CATEGORIES
    if indicator in EPDX_REACH)

EPDX_MODULE: Final[Map[Module, str]] = Map.of_seq((module, _token(module).replace("-", "")) for module in _STAGES)

_EPDX_NAMES: Final[Map[str, str]] = Map.of_seq([("TONES", "T")])

OPENEPD_INDICATOR: Final[Map[ImpactCategory, str]] = Map.of_seq(
    (indicator, {"gwp-total": "gwp", "ep-freshwater": "ep-fresh", "ep-terrestrial": "ep-terr",
                 "adp-minerals": "ADP-mineral", "adp-fossil": "ADP-fossil", "wdp": "WDP"}.get(_token(indicator), _token(indicator)))
    for indicator in _CATEGORIES)

OPENEPD_MODULE: Final[Map[Module, str]] = Map.of_seq(
    (module, "A1A2A3" if module is Module.A1_A3 else module.name) for module in _STAGES)

UNIT_TOKEN: Final[Map[str, DeclaredUnit]] = Map.of_seq((_token(unit), unit) for unit in DeclaredUnit if unit is not DeclaredUnit.UNSPECIFIED)
STANDARD_TOKEN: Final[Map[str, Standard]] = Map.of_seq((_token(standard), standard) for standard in Standard if standard is not Standard.UNSPECIFIED)

OPENEPD_DOCTYPE: Final[str] = "openEPD"

_ENCODE_RAISES: Final[Catch] = (TypeError, ValueError, OverflowError)

_ILCD_RAISES: Final[Catch] = (TypeError, ValueError)

_ADMIT_RAISES: Final[Catch] = (TypeError, ValueError)

DECLARATION_WIRE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.DECLARATION, point="wire", arm="boundary", defect="canonical-encode", retriability=TERMINAL
)
DECLARATION_FETCH: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.DECLARATION, point="fetch", arm="boundary", defect="ingress-admit", retriability=TRANSIENT
)
DECLARATION_UNROUTED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.DECLARATION, point="fetch.route", arm="config", defect="no-live-leg", retriability=TERMINAL, slots=("registry",)
)
DECLARATION_ILCD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.DECLARATION, point="ilcd", arm="boundary", defect="ilcd-parse", retriability=TERMINAL
)
DECLARATION_MISMATCHED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.DECLARATION, point="ingress", arm="config", defect="mismatched", retriability=TERMINAL, slots=("payload", "registry")
)
DECLARATION_BLANK: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.DECLARATION, point="slot", arm="config", defect="blank", retriability=TERMINAL, slots=("slot",)
)
DECLARATION_UNROSTERED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.DECLARATION, point="slot.roster", arm="config", defect="unrostered", retriability=TERMINAL, slots=("slot", "member")
)
DECLARATION_UNDATED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.DECLARATION, point="slot.date", arm="config", defect="absent", retriability=TERMINAL, slots=("slot",)
)
DECLARATION_AMOUNT: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.DECLARATION, point="declared_unit", arm="config", defect="unadmitted-amount", retriability=TERMINAL, slots=("amount",)
)
DECLARATION_STANDARD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.DECLARATION, point="standard", arm="config", defect="unrostered-standard", retriability=TERMINAL, slots=("spellings",)
)
DECLARATION_DOCTYPE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.DECLARATION, point="subtype", arm="config", defect="unadmitted-doctype", retriability=TERMINAL, slots=("doctype",)
)
DECLARATION_CELLLESS: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.DECLARATION, point="indicators", arm="config", defect="no-declared-cell", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    DECLARATION_WIRE,
    DECLARATION_FETCH,
    DECLARATION_UNROUTED,
    DECLARATION_ILCD,
    DECLARATION_MISMATCHED,
    DECLARATION_BLANK,
    DECLARATION_UNROSTERED,
    DECLARATION_UNDATED,
    DECLARATION_AMOUNT,
    DECLARATION_STANDARD,
    DECLARATION_DOCTYPE,
    DECLARATION_CELLLESS,
]))


# --- [MODELS] ---------------------------------------------------------------------------

class Keying(Struct, frozen=True):
    material_key: str


@dataclass(frozen=True, slots=True, kw_only=True)
class IngestPolicy:
    registry: Registry
    ingress: IngressTag
    resource: Option[str]


class DeclarationReceipt(Struct, frozen=True, omit_defaults=True):
    registry: Registry
    uuid: str
    material_key: str
    declared: int
    undeclared: int
    unreachable: int
    version: str | None = None


@tagged_union(frozen=True)
class DeclarationIngress:
    tag: IngressTag = tag()
    ilcd: "str | bytes" = case()
    openepd: "Epd" = case()


@dataclass(frozen=True, slots=True, kw_only=True)
class Admitted:
    material_key: RuntimeRail[str]
    product: RuntimeRail[str]
    issuer: RuntimeRail[str]
    registration: RuntimeRail[str]
    declared_unit: RuntimeRail[DeclaredUnit]
    standard: RuntimeRail[Standard]
    subtype: RuntimeRail[Subtype]
    issued: RuntimeRail[dt.date]
    valid_until: RuntimeRail[dt.date]
    cells: RuntimeRail[list[ImpactCell]]
    uuid: RuntimeRail[str]

    def record(self, registry: Registry, version: Option[str]) -> "RuntimeRail[DeclarationRecord]":
        return self._censused().bind(lambda _: self._transcribed(registry, version))

    def _censused(self) -> "RuntimeRail[Block[str]]":
        return traversed(
            Block.of_seq(fields(self)).map(lambda slot: getattr(self, slot.name).map(lambda _: slot.name)),
            by=Disposition.ACCUMULATE)

    def _transcribed(self, registry: Registry, version: Option[str]) -> "RuntimeRail[DeclarationRecord]":
        @railed
        def built() -> Generator[Any, Any, DeclarationRecord]:
            material_key = yield from self.material_key
            product = yield from self.product
            issuer = yield from self.issuer
            registration = yield from self.registration
            declared_unit = yield from self.declared_unit
            standard = yield from self.standard
            subtype = yield from self.subtype
            issued = yield from self.issued
            valid_until = yield from self.valid_until
            cells = yield from self.cells
            uuid = yield from self.uuid
            return DeclarationRecord(
                material_key=material_key, product=product, issuer=issuer, registration=registration,
                declared_unit=declared_unit, standard=standard, subtype=subtype, issued=issued,
                valid_until=valid_until, cells=cells,
                source=Source(registry=registry, uuid=uuid, version=version.to_optional()))

        return built()


# --- [POLICIES] -------------------------------------------------------------------------

POLICIES: Final[Map[Registry, IngestPolicy]] = Map.of_seq(
    (policy.registry, policy)
    for policy in (
        IngestPolicy(registry=Registry.OKOBAUDAT, ingress="ilcd", resource=Some("processes/{uuid}")),
        IngestPolicy(registry=Registry.EPD_NORGE, ingress="ilcd", resource=Some("processes/{uuid}")),
        IngestPolicy(registry=Registry.EC3, ingress="openepd", resource=Nothing),
        IngestPolicy(registry=Registry.BUNDLE, ingress="openepd", resource=Nothing)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def wire(record: DeclarationRecord) -> "RuntimeRail[tuple[bytes, ContentKey]]":
    return boundary(DECLARATION_WIRE, lambda: (record.to_binary(), record.to_json().encode()), catch=_ENCODE_RAISES).bind(
        lambda rendered: ContentIdentity.of("declaration", rendered[1]).map(lambda key: (rendered[0], key)))


def receipt(record: DeclarationRecord) -> DeclarationReceipt:
    reach = READERS[POLICIES[record.source.registry].ingress]
    return DeclarationReceipt(
        registry=record.source.registry, uuid=record.source.uuid,
        version=record.source.version if record.source.has_field("version") else None,
        material_key=record.material_key, declared=len(record.cells),
        undeclared=len(reach) * len(_STAGES) - len(record.cells),
        unreachable=(len(_CATEGORIES) - len(reach)) * len(_STAGES))


class MaterialDeclaration:
    @classmethod
    def of(
        cls, payload: "DeclarationIngress | Block[DeclarationIngress]", policy: IngestPolicy, keying: Keying,
        *, by: Disposition = Disposition.ABORT,
    ) -> "RuntimeRail[DeclarationRecord] | RuntimeRail[Block[DeclarationRecord]]":
        match payload:
            case Block() as block:
                return traversed(block.map(lambda one: cls._one(one, policy, keying)), by=by)
            case one:
                return cls._one(one, policy, keying)

    @staticmethod
    async def fetched(
        transport: TransportResource, policy: IngestPolicy, uuid: str
    ) -> "RuntimeRail[DeclarationIngress]":
        match policy.resource:
            case Option(tag="some", some=template):
                acquired = await transport.acquire(template.format(uuid=uuid), Delivery.WHOLE)
                return acquired.bind(
                    lambda chunk: boundary(DECLARATION_FETCH, lambda: DeclarationIngress(ilcd=bytes(chunk)), catch=_ADMIT_RAISES))
            case _:
                return Error(DECLARATION_UNROUTED.raised(policy.registry.name))

    @classmethod
    def _one(
        cls, payload: "DeclarationIngress", policy: IngestPolicy, keying: Keying
    ) -> "RuntimeRail[DeclarationRecord]":
        if payload.tag != policy.ingress:
            return Error(DECLARATION_MISMATCHED.raised(payload.tag, policy.registry.name))
        match payload:
            case DeclarationIngress(tag="ilcd", ilcd=document):
                return _ilcd(document, policy, keying)
            case DeclarationIngress(tag="openepd", openepd=declaration):
                return _openepd(declaration, policy, keying)
            case _ as unreachable:
                assert_never(unreachable)


def _ilcd(document: "str | bytes", policy: IngestPolicy, keying: Keying) -> "RuntimeRail[DeclarationRecord]":
    import json as _json

    from epdx.pydantic import EPD

    from rasm.data.impact.impact import ilcd_document

    return boundary(DECLARATION_ILCD, lambda: EPD(**_json.loads(ilcd_document(document))), catch=_ILCD_RAISES).bind(
        lambda epd: _ilcd_admitted(epd, keying).record(policy.registry, Option.of_obj(epd.version)))


def _ilcd_admitted(epd: "IlcdEpd", keying: Keying) -> Admitted:
    return Admitted(
        material_key=_present("material_key", keying.material_key),
        product=_present("product", epd.name),
        issuer=_present("issuer", Option.of_obj(epd.source).map(lambda source: source.name).to_optional()),
        registration=_present("registration", epd.id),
        declared_unit=_rostered("declared_unit", DeclaredUnit, epd.declared_unit),
        standard=_rostered("standard", Standard, epd.standard),
        subtype=_rostered("subtype", Subtype, epd.subtype),
        issued=_dated("issued", epd.published_date),
        valid_until=_dated("valid_until", epd.valid_until),
        cells=_declared(_ilcd_cells(epd)),
        uuid=_present("uuid", epd.id))


def _openepd(declaration: "Epd", policy: IngestPolicy, keying: Keying) -> "RuntimeRail[DeclarationRecord]":
    return _openepd_admitted(declaration, keying).record(
        policy.registry, Option.of_obj(declaration.version).map(str))


def _openepd_admitted(declaration: "Epd", keying: Keying) -> Admitted:
    standard = _complied(declaration.compliance)
    return Admitted(
        material_key=_present("material_key", keying.material_key),
        product=_present("product", declaration.product_name),
        issuer=_present("issuer", Option.of_obj(declaration.program_operator).map(lambda org: org.name).to_optional()),
        registration=_present("registration", declaration.program_operator_doc_id),
        declared_unit=_united(declaration.declared_unit),
        standard=standard,
        subtype=_represented(declaration.doctype),
        issued=_dated("issued", declaration.date_of_issue),
        valid_until=_dated("valid_until", declaration.valid_until),
        cells=standard.bind(lambda edition: _declared(_openepd_cells(declaration, edition))),
        uuid=_present("uuid", declaration.id))


def _present(slot: str, text: "str | None") -> "RuntimeRail[str]":
    return Option.of_obj(text).filter(lambda value: value != "").to_result_with(lambda: DECLARATION_BLANK.raised(slot))


def _rostered[E: Enum](slot: str, roster: type[E], member: "Enum | None") -> "RuntimeRail[E]":
    return Option.of_obj(member).bind(
        lambda foreign: Option.of_obj(roster.__members__.get(_EPDX_NAMES.try_find(foreign.name.upper()).default_value(foreign.name.upper())))
    ).filter(lambda elected: elected.name != "UNSPECIFIED").to_result_with(lambda: DECLARATION_UNROSTERED.raised(slot, str(member)))


def _united(declared: "Amount | None") -> "RuntimeRail[DeclaredUnit]":
    return Option.of_obj(declared).filter(lambda amount: amount.qty == 1.0).bind(
        lambda amount: UNIT_TOKEN.try_find(str(amount.unit))
    ).to_result_with(lambda: DECLARATION_AMOUNT.raised(str(declared)))


def _complied(compliance: "list[Compliance]") -> "RuntimeRail[Standard]":
    return Block.of_seq(compliance).collect(
        lambda claimed: Block.of_seq((claimed.short_name, claimed.name))
    ).choose(lambda spelling: STANDARD_TOKEN.try_find(_folded(spelling))).try_head().to_result_with(
        lambda: DECLARATION_STANDARD.raised(",".join(str(one.short_name) for one in compliance)))


def _folded(spelling: "str | None") -> str:
    return "".join(character for character in (spelling or "") if character.isalnum()).lower()


def _represented(doctype: str) -> "RuntimeRail[Subtype]":
    return Ok(Subtype.SPECIFIC) if doctype == OPENEPD_DOCTYPE else Error(DECLARATION_DOCTYPE.raised(doctype))


def _dated(slot: str, moment: "dt.datetime | None") -> "RuntimeRail[Date]":
    return Option.of_obj(moment).map(lambda instant: Date(year=instant.year, month=instant.month, day=instant.day)).to_result_with(
        lambda: DECLARATION_UNDATED.raised(slot))


def _declared(cells: "list[ImpactCell]") -> "RuntimeRail[list[ImpactCell]]":
    return Ok(cells) if cells else Error(DECLARATION_CELLLESS.raised())


def _ilcd_cells(epd: "IlcdEpd") -> "list[ImpactCell]":
    return [
        ImpactCell(category=indicator, stage=module, value=value)
        for indicator in _CATEGORIES
        for field in EPDX_INDICATOR.try_find(indicator).to_list()
        for category in Option.of_obj(getattr(epd, field)).to_list()
        for module in _STAGES
        for value in Option.of_obj(getattr(category, EPDX_MODULE[module])).to_list()
    ]


def _openepd_cells(declaration: "Epd", edition: Standard) -> "list[ImpactCell]":
    from rasm.data.impact.impact import REGIMES, Regime

    return [
        ImpactCell(category=indicator, stage=module, value=measured.mean)
        for impacts in Option.of_obj(declaration.impacts).to_list()
        for method in REGIMES[Regime[edition.name]].elected(impacts.available_methods()).to_list()
        for chosen in Option.of_obj(impacts.get_impact_set(method)).to_list()
        for indicator in _CATEGORIES
        for scopeset in Option.of_obj(chosen.get_scopeset_by_name(OPENEPD_INDICATOR[indicator])).to_list()
        for module in _STAGES
        for measured in Option.of_obj(getattr(scopeset, OPENEPD_MODULE[module])).to_list()
    ]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
