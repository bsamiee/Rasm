# [PY_DATA_DECLARATION]

The declaration-registry ingest owner — the product-declaration plane of `data`, sibling to the impact normalization plane and minting what that plane structurally cannot: a DATED, IDENTITY-BEARING record per verified product declaration. `impact.md`'s eight-column frame stays impact-only by design — per-(indicator, stage) amounts with no issuer, no dates, no coverage census — and this owner carries the declaration NOUN whole: material identity, issuer + registration, declared unit, issue and expiry dates, and the per-indicator per-module cell map whose KEY PRESENCE is the coverage census. The record is the corpus `tests/contracts/` `declaration-record` DOMAIN contract; this page is its one producer, `csharp:Rasm.Materials` assessment its committed decoder, and the schema definition owns every roster — this page transcribes, never re-declares.

Sources ride one payload-shape axis exactly as the impact plane's: the Ökobaudat/soda4LCA ILCD+EPD document parses through `epdx.convert_ilcd`, the EC3/OpenEPD typed model through `openepd`, and each arm folds onto ONE `DeclarationRecord` under a caller-supplied KEYING row binding the registry product to the estate material identity — registry curation is provenance-bound caller data, never an inferred join. Transport endpoints arrive from the runtime `TransportResource` at the boundary; parser imports bind function-local per the module-level ban, so a run touching no registry pays no parser import.

## [01]-[INDEX]

- [02]-[DECLARATION]: the `DeclarationIngress` source axis, the `Keying` row, the `DeclarationRecord` wire struct with its frozen rosters, the one `_normalize` fold, the canonical-JSON `wire` crossing, and the `DeclarationReceipt`.

## [02]-[DECLARATION]

- Owner: `DeclarationRecord` transcribes the corpus `declaration-record` schema field for field — struct declaration order IS the canonical key order, roster-ordered cell maps, absent cells omitted (`omit_defaults`), so the encoder emits canonical JSON by construction and no second canonicalization pass exists; `Registry`/`DeclaredUnit`/`Standard`/`Subtype` mirror the schema's closed vocabularies as total `StrEnum` mirrors; `Keying` binds one registry identity to one estate `material_key`.
- Cases: `DeclarationIngress` discriminates the payload shape — `ilcd` (an Ökobaudat/soda4LCA ILCD+EPD document, `str | bytes`) and `openepd` (a typed EC3 declaration) — never a provider knob; the registry provenance row derives from the arm and the payload's own identity fields.
- Entry: `MaterialDeclaration.of(payload, keying, *, by=Disposition.ABORT)` is the one normalization entry, polymorphic over one ingress or a `Block` exactly as the impact plane's `of`; `MaterialDeclaration.fetched(transport, uuid)` is the live Ökobaudat leg acquiring the ILCD+EPD document off the runtime transport; `wire()` composes the consumer-edge crossing — canonical JSON bytes beside the `ContentIdentity`-minted `ContentKey`.
- Auto: coverage is NEVER fabricated — a parser field holding `None` writes no cell, an indicator the parser's typed model cannot carry writes no row, and the census a consumer reads is exactly the key set; the epdx model carries the A1-shaped single `ep` and no `wdp`, so the Ökobaudat arm fills the six correspondence-proven core indicators and the C# decoder's carbon-vector arm (not its full-matrix arm) carries the crossing — the honest projection of the parser's own capability bound. An `UNKNOWN` declared unit refuses typed; a fraction outside the unit interval refuses typed; a record missing issue or expiry dates refuses typed, because a dateless declaration cannot enter a resolution law that gates on expiry.
- Receipt: `DeclarationReceipt` keys on the registry identity — registry, uuid, version, material key, declared-cell census — so re-ingestion of one registration dedupes in the reuse ledger rather than re-fetching; structured evidence on the one runtime rail, no new metric family minted — declaration ingest is evidence-plane traffic and the impact plane's `domain="impact"` measure stays the branch's one environmental metric row.
- Packages: `epdx` parses the ILCD+EPD wire (`convert_ilcd` + the `epdx.pydantic` EN 15804 model — its `pyo3` panic on malformed input guarded at the `boundary`), `openepd` the EC3 typed model (`RootDocumentFactory` doctype routing, `Impacts`/`ImpactSet` LCIA reads), `msgspec` the wire struct and canonical encoder; runtime rails, identity, lanes, and transport compose as everywhere. No Brightway, no openLCA — a declaration is a published record, never a solve.
- Growth: a new registry is one `DeclarationIngress` case with its `_normalize` arm and one `Registry` row (the offline `bundle` reader over `openepd`'s `DefaultBundleReader` is the staged instance, its `Registry` token already frozen in the contract); a new correspondence is one proven row on the owning table — the openepd scopeset-name widening to the full +A2 roster lands row by row as each name proves on the member rail; a schema field is a CONTRACT change first — the corpus definition moves, then this struct transcribes, in that order.
- Boundary: this page mints no `MaterialImpact` and the impact plane mints no declaration — two nouns, two owners, one shared source ecosystem; the record schema is the corpus definition's and a local field, roster, or canonical-order divergence is a fork the contract digest catches; registry licence custody stays with the consuming estate's settled clearance (Ökobaudat's redistribution licence, recorded at the Materials counterpart) and this page adds no catalogue store; deleted forms — a per-registry record class, a provider knob on `of`, a zero written into an undeclared cell, a fabricated date, a keying inferred from name matching.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import datetime as dt
from enum import StrEnum
from typing import TYPE_CHECKING, Final

from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec import json as msgjson

from rasm.runtime.faults import Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.roots import TransportResource

if TYPE_CHECKING:
    from epdx.pydantic import EPD as IlcdEpd
    from openepd.model.epd import Epd

# --- [TYPES] ----------------------------------------------------------------------------


class Registry(StrEnum):  # contract source.registry roster — frozen at the corpus definition
    OKOBAUDAT = "okobaudat"
    EC3 = "ec3"
    EPD_NORGE = "epd-norge"
    BUNDLE = "bundle"


class DeclaredUnit(StrEnum):  # contract declared_unit roster; epdx Unit UNKNOWN refuses, never defaults
    KG = "kg"
    T = "t"
    M = "m"
    M2 = "m2"
    M3 = "m3"
    L = "l"
    PCS = "pcs"
    M2R1 = "m2r1"


class Standard(StrEnum):
    EN15804A1 = "en15804a1"
    EN15804A2 = "en15804a2"


class Subtype(StrEnum):
    GENERIC = "generic"
    SPECIFIC = "specific"
    INDUSTRY = "industry"
    REPRESENTATIVE = "representative"


# --- [CONSTANTS] ------------------------------------------------------------------------

# contract rosters, frozen in schema order — cell maps build in THIS order so canonical JSON
# is the construction order, never a sort pass.
INDICATORS: Final[tuple[str, ...]] = (
    "gwp-total", "gwp-fossil", "gwp-biogenic", "gwp-luluc", "odp", "ap",
    "ep-freshwater", "ep-marine", "ep-terrestrial", "pocp", "adp-minerals", "adp-fossil", "wdp")
MODULES: Final[tuple[str, ...]] = (
    "a1-a3", "a4", "a5", "b1", "b2", "b3", "b4", "b5", "b6", "b7", "c1", "c2", "c3", "c4", "d")

# epdx EPD field -> contract indicator token: the correspondence-proven rows the typed model
# carries; the A1-shaped single `ep` and the absent `wdp` map to NOTHING — undeclared absence,
# the parser's own capability bound projected honestly.
EPDX_INDICATOR: Final[Map[str, str]] = Map.of_seq([
    ("gwp", "gwp-total"), ("odp", "odp"), ("ap", "ap"),
    ("pocp", "pocp"), ("adpe", "adp-minerals"), ("adpf", "adp-fossil")])

# epdx ImpactCategory per-stage field -> contract module token, 1:1 across the fifteen modules.
EPDX_MODULE: Final[Map[str, str]] = Map.of_seq([
    ("a1a3", "a1-a3"), ("a4", "a4"), ("a5", "a5"),
    ("b1", "b1"), ("b2", "b2"), ("b3", "b3"), ("b4", "b4"), ("b5", "b5"), ("b6", "b6"), ("b7", "b7"),
    ("c1", "c1"), ("c2", "c2"), ("c3", "c3"), ("c4", "c4"), ("d", "d")])

# epdx Unit member -> contract declared-unit token; UNKNOWN carries no row, so its read refuses.
EPDX_UNIT: Final[Map[str, str]] = Map.of_seq([
    ("KG", "kg"), ("TONES", "t"), ("M", "m"), ("M2", "m2"), ("M3", "m3"),
    ("L", "l"), ("PCS", "pcs"), ("M2R1", "m2r1")])

# openepd scopeset name -> contract indicator token: proven rows only; the +A2 widening lands
# row by row as each scopeset name proves on the member rail.
OPENEPD_INDICATOR: Final[Map[str, str]] = Map.of_seq([
    ("gwp", "gwp-total"), ("odp", "odp"), ("ap", "ap"), ("pocp", "pocp")])


# --- [MODELS] ---------------------------------------------------------------------------

# The ingest keying row — registry curation binding one registry identity to one estate
# material identity; caller data with provenance, never an inferred name match.
class Keying(Struct, frozen=True):
    material_key: str


class RecordSource(Struct, frozen=True, omit_defaults=True):
    registry: str
    uuid: str
    version: str | None = None


# The corpus `declaration-record` transcription: struct declaration order IS the canonical
# key order the contract states; absent optional fields omit, never null.
class DeclarationRecord(Struct, frozen=True, omit_defaults=True):
    material_key: str
    product: str
    issuer: str
    registration: str
    declared_unit: str
    standard: str
    subtype: str
    issued: dt.date
    valid_until: dt.date
    indicators: dict[str, dict[str, float]]
    source: RecordSource
    recycled_content: float | None = None
    end_of_life_recovery: float | None = None

    def wire(self) -> "RuntimeRail[tuple[bytes, ContentKey]]":
        # consumer-edge crossing: canonical JSON bytes (construction order is canonical order)
        # beside the content key the consumer dedupes on.
        return boundary("declaration.wire", lambda: msgjson.encode(self)).bind(
            lambda body: ContentIdentity.of(body).map(lambda key: (body, key)))

    def receipt(self) -> "DeclarationReceipt":
        return DeclarationReceipt(
            registry=self.source.registry, uuid=self.source.uuid, version=self.source.version,
            material_key=self.material_key,
            cells=sum(len(row) for row in self.indicators.values()))


class DeclarationReceipt(Struct, frozen=True, omit_defaults=True):
    registry: str
    uuid: str
    material_key: str
    cells: int
    version: str | None = None


@tagged_union
class DeclarationIngress:
    tag: str = tag()
    ilcd: "str | bytes" = case()          # an Ökobaudat/soda4LCA ILCD+EPD document
    openepd: "Epd" = case()               # a typed EC3 declaration


# --- [OPERATIONS] -----------------------------------------------------------------------


class MaterialDeclaration:
    @classmethod
    def of(
        cls, payload: "DeclarationIngress | Block[DeclarationIngress]", keying: Keying,
        *, by: Disposition = Disposition.ABORT,
    ) -> "RuntimeRail[DeclarationRecord] | RuntimeRail[Block[DeclarationRecord]]":
        match payload:
            case Block() as block:
                return traversed(block.map(lambda one: cls._one(one, keying)), by=by)
            case one:
                return cls._one(one, keying)

    @staticmethod
    async def fetched(transport: TransportResource, uuid: str) -> "RuntimeRail[DeclarationIngress]":
        # live Ökobaudat leg over the runtime transport axis — the soda4LCA process resource in
        # ILCD+EPD JSON; bearer + retry ride the resource, the document routes to the ilcd arm.
        return (await transport.acquire(f"processes/{uuid}")).map(DeclarationIngress.Ilcd)

    @classmethod
    def _one(cls, payload: "DeclarationIngress", keying: Keying) -> "RuntimeRail[DeclarationRecord]":
        match payload:
            case DeclarationIngress(tag="ilcd", ilcd=document):
                return cls._from_ilcd(document, keying)
            case DeclarationIngress(tag="openepd", openepd=decl):
                return cls._from_openepd(decl, keying)

    @staticmethod
    def _from_ilcd(document: "str | bytes", keying: Keying) -> "RuntimeRail[DeclarationRecord]":
        # convert_ilcd unwraps a Rust parse Result — malformed ILCD surfaces as a PanicException,
        # so the boundary fence is the guard the parser does not carry.
        import json as _json

        from epdx import convert_ilcd
        from epdx.pydantic import EPD

        return boundary("declaration.ilcd", lambda: EPD(**_json.loads(convert_ilcd(document)))).bind(
            lambda epd: _record(epd, keying))

    @staticmethod
    def _from_openepd(decl: "Epd", keying: Keying) -> "RuntimeRail[DeclarationRecord]":
        # the EC3 arm: identity off open_xpd_uuid + version, dates off the declaration's own
        # validity, cells off the deterministic-minimum LCIA method through OPENEPD_INDICATOR —
        # proven correspondence rows only, unproven names write nothing.
        return boundary("declaration.openepd", lambda: _openepd_record(decl, keying))


# ILCD/epdx lowering: every cell is a declared value — a None per-stage field writes no key,
# so the census is the key set; dateless or UNKNOWN-unit declarations refuse typed.
def _record(epd: "IlcdEpd", keying: Keying) -> "RuntimeRail[DeclarationRecord]":
    def cells(epd: "IlcdEpd") -> dict[str, dict[str, float]]:
        rows: dict[str, dict[str, float]] = {}
        for field, token in EPDX_INDICATOR.to_seq():
            category = getattr(epd, field)
            if category is None:
                continue
            row = {
                EPDX_MODULE[stage]: value
                for stage, _ in EPDX_MODULE.to_seq()
                if (value := getattr(category, stage)) is not None
            }
            if row:
                rows[token] = row
        return rows

    return boundary("declaration.record", lambda: DeclarationRecord(
        material_key=keying.material_key,
        product=epd.name,
        issuer=epd.source.name if epd.source is not None else "",
        registration=epd.id,
        declared_unit=EPDX_UNIT[epd.declared_unit.name],   # missing row raises -> typed refusal at the boundary
        standard=Standard.EN15804A2 if epd.standard.name == "EN15804A2" else Standard.EN15804A1,
        subtype=epd.subtype.name.lower(),
        issued=dt.date.fromisoformat(str(epd.published_date)),
        valid_until=dt.date.fromisoformat(str(epd.valid_until)),
        indicators=cells(epd),
        source=RecordSource(registry=Registry.OKOBAUDAT, uuid=epd.id, version=epd.version)))


# EC3 lowering: identity off the canonical open_xpd_uuid, cells off the deterministic-minimum
# available LCIA method (Impacts.get_impact_set -> get_scopeset_by_name per OPENEPD_INDICATOR
# proven row, per-module ScopeSet fields writing keys only where declared). The validity-date
# member spellings prove on the member rail before this body lands.
def _openepd_record(decl: "Epd", keying: Keying) -> DeclarationRecord: ...
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
