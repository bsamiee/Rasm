# [PY_GEOMETRY_ENERGY_CLIMATE]

`Climate` owns the energy plane's weather substrate and its point-comfort tier — the plane's ONLY solar owner. `Climate.of` admits EPW weather by payload shape, and ONE polymorphic `query` answers every read that document supports: `series` collection algebra, `derive` projections, `solar` sun-geometry, `comfort` point-comfort with SolarCal MRT, the `index` scalar band, the `mrt` SolarCal series, and the spatial `comfort_map` readback — one `ClimateQuery` case in, one `ClimateResult` arm out, one `_dispatch` closing on `assert_never`. `ladybug-core` and `ladybug-comfort` own every weather, psychrometric, comfort, and index kernel; this page composes them into typed, railed, receipted evidence.

Ladybug's AGPL-3.0 network-copyleft band rides the standing companion-lane charter: `ladybug`/`ladybug_comfort` reach the interpreter through `LateBound` — the folder's ONE late-binding owner, holding its module as a STRING and resolving at the call seam — or through a function-local import inside a boundary seam, never a module-scope binding of either form, because a static license audit reads the LEXICAL import graph and a module-scope statement (`lazy` included, the soft keyword being module-scope by design) couples every importer of this page to AGPL; confinement is the point, not deferral. Evidence crosses the wire, never a link into a distributed host binary. Every query threads the `rasm.geometry.graduation` `evidence_run` weave under the owner's own `composition` key, the admitted document keys once over its canonical file-string bytes so re-ingestion dedupes in the persistence ledger, and comfort evidence graduates under `GeometrySubject.THERMAL_COMFORT` against caller ceilings.

## [01]-[INDEX]

- [02]-[CLIMATE]: one polymorphic weather owner — EPW admission and one `ClimateQuery`/`ClimateResult` read surface over series, derived documents, sun geometry, comfort, scalar indices, MRT, and spatial maps — under one `ClimateReceipt`.

## [02]-[CLIMATE]

- Owner: `Climate` holds the decoded `EPW`, its `Location`, the weather `ContentKey`, and the `composition` custody key every weave and charter record stamps. `ClimateField` members ARE the `EPW` collection-property names across the whole EPW data band, so field access is one `getattr`; `Reduce.method` derives its ladybug spelling from its own fields; `LateBound` carries the module, the member, and the roster of owner attributes its kernel takes, so `COMFORT`, `MAPS`, and `INDEX` share ONE resolution fold and the page holds no second `import_module` site. `EnergyFault` seats beside `LateBound` for the same reachability reason — every band page already imports this owner — so the whole band's refusal vocabulary is ONE closed family rather than four coordinate-string grammars a consumer re-parses per page.
- Entry: `Climate.of` admits `bytes | str | Path | Mapping` by payload shape — modality IS the shape, never a `source_kind=` knob. `query` is the one read surface over `ClimateQuery | Sequence[ClimateQuery]`; a batch accumulates through `traversed(ACCUMULATE)` so a refused member names itself beside its siblings. `comfort` routes the EPW solar fields through SolarCal to the `rad_temperature` MRT input internally; the `mrt` case is the explicit arm only for a caller feeding a non-EPW thermal model, and the `index` band's own MRT slot resolves through that same fold.
- Auto: every fold returns `(result, receipt)` built inside the fold off facts it already holds, so the weave's conditional harvest emits without a caller hand-asserting a single count. Collection alignment is `compute_function_aligned`'s own precondition over the aligned operand list, so the index band never re-derives it; `Wea`/`DDY` projections serialize through their own `to_file_string`, reaching the recipe boundary as handler inputs, never a second reader; `comfort_map` rows consume the recipe products `energy/simulate`'s `matrix` readback surfaces, so a spatial map reads real addresses rather than a feed no producer emits.
- Receipt: `ClimateReceipt.spec` is the evidence subject — the admitted weather key beside the query that read it — and `graduates` derives its `ContentKey` from it, so no caller supplies a key. Discomfort is the comfort fold's own measurement: a series, solar, or index read never computed it, so `graduates` OMITS the measure and the spine's `_breached` refuses `unmeasured:discomfort` rather than clearing a ceiling on a fabricated zero. The comfort fold records `rasm.geometry.comfort.discomfort` onto the charter at the producing site.
- Packages: `ladybug-core` and `ladybug-comfort` per the table rows; the spatial `map.*` kernels are the `energy/simulate` readback surface, and a `LateBound` resolution or a boundary-seam function-local import is the seam every one of them crosses.
- Growth: a new climate read is one `ClimateQuery` case, one `_dispatch` arm, and one `ClimateResult` arm; a new field is one `ClimateField` member; a new grain one `Grain` member, method name deriving; a new projection one `Derived` case beside its `DerivedDocument` arm, IP-units over `EPW.convert_to_ip` the named next; a new comfort model one `COMFORT` row; a new scalar index one `INDEX` row over its `IndexInput` roster; a new spatial map one `MAPS` row; a new band refusal is one `EnergyFault` case carrying its own coordinate tuple, minted at the page that raises it and read by every consumer off the tag. `Adaptive` is not weather-drivable — it builds `from_air_and_rad_temp` over model results, so its home is the `energy/simulate` readback, its `AdaptiveParameter` already serializable through this page's parameter discipline; urban-microclimate EPW morphing enters as one more `Derived` case with its own package admission.
- Boundary: `SolarQuery` reads the WEATHER file's own sun and a captured scene descriptor carries angles a peer already solved — `energy/simulate` projects those angles straight onto a sky, since routing them back through `Sunpath` re-derives an ephemeris the descriptor settled and silently substitutes a second almanac's answer.
- Boundary: no diagram furniture — artifacts owns the sun-path diagram and `Sunpath` gains no diagram consumer here; no radiance simulation (the recipe rail owns it), no HBJSON model semantics (`energy/model` owns them), no chart/legend composition (artifacts-plane material), and no re-derived solar vector algebra — `Sunpath` emits `ladybug_geometry` primitives and this owner projects them to arrays.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Mapping, Sequence
from enum import StrEnum
from functools import partial
from importlib import import_module
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never, overload

from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.geometry.graduation import (
    EvidenceScope,
    GeometryHandoff,
    GeometrySubject,
    charter_record,
    evidence_key,
    evidence_run,
)
from rasm.runtime.faults import Disposition, RuntimeRail, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey

if TYPE_CHECKING:  # AGPL band: annotations resolve here and never at runtime; every runtime reach is a LateBound resolution or a boundary-seam function-local import
    from ladybug.datacollection import HourlyContinuousCollection
    from ladybug.ddy import DDY
    from ladybug.designday import DesignDay
    from ladybug.epw import EPW
    from ladybug.location import Location
    from ladybug.wea import Wea

# --- [TYPES] ----------------------------------------------------------------------------

type Window = tuple[int, int, int, int, int, int]  # (st_month, st_day, st_hour, end_month, end_day, end_hour)


class ClimateField(StrEnum):
    # every EPW hourly data field the reader publishes as a collection property, fields 6-34 plus the derived sky
    # temperature — the member value IS the property name, so a field read is one `getattr` and the index rows below
    # name their operands from this one vocabulary rather than a per-kernel string.
    DRY_BULB_TEMPERATURE = "dry_bulb_temperature"
    DEW_POINT_TEMPERATURE = "dew_point_temperature"
    RELATIVE_HUMIDITY = "relative_humidity"
    ATMOSPHERIC_STATION_PRESSURE = "atmospheric_station_pressure"
    EXTRATERRESTRIAL_HORIZONTAL_RADIATION = "extraterrestrial_horizontal_radiation"
    EXTRATERRESTRIAL_DIRECT_NORMAL_RADIATION = "extraterrestrial_direct_normal_radiation"
    HORIZONTAL_INFRARED_RADIATION_INTENSITY = "horizontal_infrared_radiation_intensity"
    GLOBAL_HORIZONTAL_RADIATION = "global_horizontal_radiation"
    DIRECT_NORMAL_RADIATION = "direct_normal_radiation"
    DIFFUSE_HORIZONTAL_RADIATION = "diffuse_horizontal_radiation"
    GLOBAL_HORIZONTAL_ILLUMINANCE = "global_horizontal_illuminance"
    DIRECT_NORMAL_ILLUMINANCE = "direct_normal_illuminance"
    DIFFUSE_HORIZONTAL_ILLUMINANCE = "diffuse_horizontal_illuminance"
    ZENITH_LUMINANCE = "zenith_luminance"
    WIND_DIRECTION = "wind_direction"
    WIND_SPEED = "wind_speed"
    TOTAL_SKY_COVER = "total_sky_cover"
    OPAQUE_SKY_COVER = "opaque_sky_cover"
    VISIBILITY = "visibility"
    CEILING_HEIGHT = "ceiling_height"
    PRESENT_WEATHER_OBSERVATION = "present_weather_observation"
    PRESENT_WEATHER_CODES = "present_weather_codes"
    PRECIPITABLE_WATER = "precipitable_water"
    AEROSOL_OPTICAL_DEPTH = "aerosol_optical_depth"
    SNOW_DEPTH = "snow_depth"
    DAYS_SINCE_LAST_SNOWFALL = "days_since_last_snowfall"
    ALBEDO = "albedo"
    LIQUID_PRECIPITATION_DEPTH = "liquid_precipitation_depth"
    LIQUID_PRECIPITATION_QUANTITY = "liquid_precipitation_quantity"
    SKY_TEMPERATURE = "sky_temperature"


class Grain(StrEnum):
    MONTHLY = "monthly"
    DAILY = "daily"
    MONTHLY_PER_HOUR = "monthly_per_hour"


class ComfortModel(StrEnum):
    # the heat-balance collection tier: a class whose results are collection properties.
    PMV = "pmv"
    UTCI = "utci"
    PET = "pet"


class IndexModel(StrEnum):
    # the scalar tier: a pointwise kernel `compute_function_aligned` folds across aligned collections. A class-shaped
    # comfort model can never be an INDEX row and a scalar kernel can never be a COMFORT row — the two tiers are the
    # discriminant, so neither table's growth clause overpromises the other's.
    APPARENT = "apparent"
    DISCOMFORT = "discomfort"
    HEAT = "heat"
    HUMIDEX = "humidex"
    WBGT = "wbgt"
    WINDCHILL = "windchill"
    SENSATION = "sensation"
    HEATING_DEGREE = "heating-degree"
    COOLING_DEGREE = "cooling-degree"


class MapKind(StrEnum):
    SHORTWAVE_MRT = "shortwave_mrt"
    LONGWAVE_MRT = "longwave_mrt"
    TCP = "tcp"
    AIR = "air"


class LateBound(Struct, frozen=True, gc=False):
    # the folder's ONE late-binding owner: a dotted module, the member inside it, and the roster of OWNER attributes
    # its kernel takes. `leads` names them rather than flagging them, so a row reaches its parameter by keyword at any
    # position and the boolean prefix branch a two-slot tuple forced dies with it. `energy/model` and
    # `energy/simulate` read the same struct, so the AGPL band's whole late-binding surface is one grammar.
    module: str
    member: str
    leads: tuple[str, ...] = ()

    def resolve(self) -> object:
        return getattr(import_module(self.module), self.member)

    def bound(self, owner: object) -> Mapping[str, object]:
        return {name: getattr(owner, name) for name in self.leads}


class RegimeAction(StrEnum):
    # what a regime factor DECIDES. The action class is the discriminant a reader needs before the number means
    # anything: a balance point, a comfort standard, and an admission bar are three different kinds of decision, and
    # a bare float at a call site declares none of them.
    DEGREE_BASE = "degree-base"  # the balance-point temperature a degree-time kernel integrates against
    COMFORT_STANDARD = "comfort-standard"  # the parameter document a comfort kernel scores against
    COMPLIANCE_CEILING = "compliance-ceiling"  # the residual bar a graduating crossing is admitted under


@tagged_union(frozen=True)
class RegimeFactor:
    # a factor is a scalar the kernel consumes directly or the provider parameter DOCUMENT a standard is expressed
    # as — two shapes, one carrier, so a grading site and a comfort fold read the same rows without a second table.
    tag: Literal["magnitude", "parameters"] = tag()
    magnitude: float = case()
    parameters: Mapping[str, object] = case()


class EnergyRegime(Struct, frozen=True, gc=False):
    # ONE row shape for every number and every standard this band decides on. The retired forms carried no axis at
    # all: two degree-day balance points sat as bare literals inside index rows, a comfort standard rode an erased
    # `Mapping[str, object]` no reader could attribute, and four graduating crossings took an anonymous `ceiling:
    # float` from their caller — so nothing on this band could tell a code-mandated bar from a guess, and a
    # compliance verdict asserted a standard nobody named. The three columns close that: `action` names what the
    # factor decides, `citation` names the authority that published it, and `kernel` names the provider entry that
    # consumes it — absent on a bar this band grades itself rather than hands to a provider.
    action: RegimeAction
    factor: RegimeFactor
    citation: str
    kernel: Option[LateBound] = Nothing

    @property
    def magnitude(self) -> Option[float]:
        # the scalar half, shaped for the operand slots that already carry `Option[float]`; a parameters row is
        # absent HERE rather than coerced, so a standard document can never be read as a balance point.
        return Some(self.factor.magnitude) if self.factor.tag == "magnitude" else Nothing

    def bar(self) -> float:
        # a compliance ceiling's factor IS a magnitude by construction, so a parameters row reaching a grading site
        # is a roster defect the band refuses BY NAME rather than grading a crossing against a document.
        match self.factor:
            case RegimeFactor(tag="magnitude", magnitude=value):
                return value
            case RegimeFactor(tag="parameters", parameters=document):
                raise EnergyFault(regime_factor=(self.action.value, f"non-scalar:{len(document)}"))
            case _ as unreachable:
                assert_never(unreachable)

    def document(self) -> Mapping[str, object]:
        # the standard half, handed to a comfort kernel's own `*Parameter.from_dict`; a magnitude row reaching a
        # standard slot is the same roster defect read from the other side.
        match self.factor:
            case RegimeFactor(tag="parameters", parameters=document):
                return document
            case RegimeFactor(tag="magnitude", magnitude=value):
                raise EnergyFault(regime_factor=(self.action.value, f"non-document:{value:.6g}"))
            case _ as unreachable:
                assert_never(unreachable)


class RegimeKey(StrEnum):
    # one member per DECIDED factor this band holds; a new decision is one member and one `ENERGY_REGIMES` row, never
    # a literal re-appearing at a call site.
    HEATING_BALANCE = "heating-balance"
    COOLING_BALANCE = "cooling-balance"
    MODEL_VALIDITY = "model-validity"
    DISTRICT_DEFECTS = "district-defects"
    THERMAL_DISCOMFORT = "thermal-discomfort"
    BUILDING_EUI = "building-eui"


# every DECIDED factor this band holds, in one table under one row shape. Each citation names an ADMITTED CATALOG ROW
# rather than a standard asserted from memory, so a reader follows the authority to the member that publishes it: the
# two balance points quote `.api/ladybug-comfort.md` `[17]`, whose `degreetime` entries document 18 degC and 23 degC
# as common building balance points; the model and district bars quote the `check_all` rows their own providers
# publish; the comfort bar quotes the collection property the comfort fold reads; and the intensity bar quotes the
# EnergyPlus sql parser that measures it. A row whose factor no provider entry consumes carries no kernel — this band
# grades those itself at the graduation crossing, and the Appendix G / LEED rating entries `.api/honeybee-energy.md`
# `[09]`-`[13]` publish need a PROPOSED simulation beside a baseline roster, which no arm on this band runs.
ENERGY_REGIMES: Final[Map[RegimeKey, EnergyRegime]] = Map.of_seq([
    (
        RegimeKey.HEATING_BALANCE,
        EnergyRegime(
            action=RegimeAction.DEGREE_BASE,
            factor=RegimeFactor(magnitude=18.0),
            citation="ladybug-comfort [17] degreetime.heating_degree_time — documented 18 degC balance point",
            kernel=Some(LateBound("ladybug_comfort.degreetime", "heating_degree_time")),
        ),
    ),
    (
        RegimeKey.COOLING_BALANCE,
        EnergyRegime(
            action=RegimeAction.DEGREE_BASE,
            factor=RegimeFactor(magnitude=23.0),
            citation="ladybug-comfort [17] degreetime.cooling_degree_time — documented 23 degC balance point",
            kernel=Some(LateBound("ladybug_comfort.degreetime", "cooling_degree_time")),
        ),
    ),
    (
        RegimeKey.MODEL_VALIDITY,
        EnergyRegime(
            action=RegimeAction.COMPLIANCE_CEILING,
            factor=RegimeFactor(magnitude=0.0),
            citation="honeybee-energy [01] Model.check_all(detailed=True) — error rows over the element census",
        ),
    ),
    (
        RegimeKey.DISTRICT_DEFECTS,
        EnergyRegime(
            action=RegimeAction.COMPLIANCE_CEILING,
            factor=RegimeFactor(magnitude=0.0),
            citation="dragonfly-core [10] Model.check_all — defect rows over the admitted building census",
        ),
    ),
    (
        RegimeKey.THERMAL_DISCOMFORT,
        EnergyRegime(
            action=RegimeAction.COMPLIANCE_CEILING,
            factor=RegimeFactor(magnitude=0.2),
            citation="ladybug-comfort [08] .percent_comfortable — its complement over the read window",
        ),
    ),
    (
        RegimeKey.BUILDING_EUI,
        EnergyRegime(
            action=RegimeAction.COMPLIANCE_CEILING,
            factor=RegimeFactor(magnitude=150.0),
            citation="honeybee-energy [01] result.eui.eui_from_sql — total end-use intensity, kWh/m2-yr",
        ),
    ),
])


@tagged_union(frozen=True)
class Reduce:
    tag: Literal["mean", "total", "percentile"] = tag()
    mean: Grain = case()
    total: Grain = case()
    percentile: tuple[Grain, float] = case()

    @property
    def method(self) -> str:
        # one derived spelling over the ladybug reduction family — never a six-arm ladder.
        match self:
            case Reduce(tag="mean", mean=grain):
                return f"average_{grain}"
            case Reduce(tag="total", total=grain):
                return f"total_{grain}"
            case Reduce(tag="percentile", percentile=(grain, _q)):
                return f"percentile_{grain}"
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class Derived:
    tag: Literal["wea", "ddy", "design_days", "location"] = tag()
    wea: None = case()
    ddy: float = case()
    design_days: None = case()
    location: None = case()


@tagged_union(frozen=True)
class SolarQuery:
    tag: Literal["position", "day", "arc", "analemma"] = tag()
    position: float = case()
    day: tuple[int, int] = case()
    arc: tuple[int, int] = case()
    analemma: int = case()


@tagged_union(frozen=True)
class IndexInput:
    # one operand slot of a scalar index kernel, in the kernel's own positional order: an EPW field, the SolarCal MRT
    # series this owner derives, or a named scalar whose package default rides the slot — a slot carrying `Nothing`
    # demands the caller's value and refuses by name instead of inventing a physical constant.
    tag: Literal["field", "mrt", "constant"] = tag()
    field: ClimateField = case()
    mrt: None = case()
    constant: tuple[str, Option[float]] = case()


@tagged_union(frozen=True)
class SeriesSubject:
    # what a returned series IS — an admitted EPW field, a computed index, or the SolarCal MRT — so one `SeriesFact`
    # carries all three producers rather than three near-identical fact shapes keyed by three vocabularies.
    tag: Literal["field", "index", "mrt"] = tag()
    field: ClimateField = case()
    index: IndexModel = case()
    mrt: None = case()

    @property
    def label(self) -> str:
        match self:
            case SeriesSubject(tag="field", field=member) | SeriesSubject(tag="index", index=member):
                return member.value
            case SeriesSubject(tag="mrt"):
                return "solarcal"
            case _ as unreachable:
                assert_never(unreachable)


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class EnergyFault(Exception):
    # the energy band's ONE structured refusal, seated here beside `LateBound` because every band page already
    # imports this owner: raised INTO the converting fence — `evidence_run`'s `boundary` on the caller-floor folds,
    # the lane's `async_boundary` on the offloaded translate kernel, whose crossing carries the token as
    # `CrossedFault` DATA and re-mints this family's own case parent-side per `execution/workers#CROSSING` — so the
    # coordinate facts survive as kwargs the boundary fault lifts whole. A `raise ValueError(f"...")` flattens those
    # facts into a string every consumer re-parses and forks the refusal vocabulary against the mesh peers.
    tag: Literal[
        "empty_model", "index_constant", "unknown_output", "unresolved_output", "unsupported_target",
        "district_defects", "authored_sun", "shading_fidelity", "shading_census",
        "artifact_integrity", "artifact_admission",
        "map_operands", "regime_factor",
    ] = tag()
    empty_model: tuple[str, int] = case()  # (admission modality, check-row census) — a model with no rooms
    index_constant: tuple[str, str] = case()  # (index model, the demanded constant slot no source answers)
    unknown_output: tuple[tuple[str, ...], int] = case()  # (requested names absent from the SQL census, census size)
    unresolved_output: tuple[str, tuple[str, ...]] = case()  # (recipe, declared outputs its product never resolved)
    unsupported_target: tuple[str, str] = case()  # (translation target, the constraint that refuses it)
    district_defects: tuple[int, tuple[tuple[str, int], ...]] = case()  # (defect rows, the per-code roster)
    authored_sun: tuple[str, str] = case()  # (recipe, the sited coordinate a manual-control sun never carries)
    shading_fidelity: tuple[str, float, float] = case()  # (refused bound, declared value, the ceiling it crossed)
    shading_census: tuple[str, int, int] = case()  # (count coordinate, descriptor value, decoded GLB value)
    # Two cases, not one two-slot case: the artifact arm measures a failed aggregate proof and NO admission phase,
    # while the admission arm measures a refused phase and NO artifact proof. One pair forced each arm to fabricate
    # the half it could not measure, and the two fabrications sat in opposite slots, so slot 0 read as a phase on
    # one arm and as a literal on the other.
    artifact_integrity: str = case()  # the `ArtifactProof` aggregate law that failed
    artifact_admission: str = case()  # the `AdmissionPhase` half Protovalidate refused
    map_operands: tuple[str, int, int] = case()  # (map kind, operands supplied, the slot roster they overflow)
    regime_factor: tuple[str, str] = case()  # (regime action, the factor shape its consuming site cannot read)

    def __str__(self) -> str:
        # `BoundaryFault.of` admits a `Tagged()` token AHEAD of every `CLASSIFY` row, so this family crosses the
        # conversion door WHOLE on the `domain` case and the catch-all's `str(cause)` half never renders it. A
        # worker seam carries it whole too: `execution/workers#CROSSING` lowers the token onto `CrossedFault` DATA
        # at `shipped` and re-mints this family's own case parent-side, so a raise inside a HOSTILE kernel needs no
        # edit here. `__str__` serves the LOG and HOST edge alone — a token surfacing in a worker traceback or a log
        # line before the seam lowers it — where `Exception.__str__` answers the EMPTY string for a kwarg-only
        # union. The law half IS the tag, so no arm re-spells its own case name and a renamed case cannot drift.
        return f"{self.tag}:{self._coordinate()}"

    def _coordinate(self) -> str:
        match self:
            case EnergyFault(tag="empty_model", empty_model=(modality, rows)):
                return f"{modality}[{rows}]"
            case EnergyFault(tag="index_constant", index_constant=(model, slot)):
                return f"{model}[{slot}]"
            case EnergyFault(tag="unknown_output", unknown_output=(names, census)):
                return f"{','.join(names)}[{census}]"
            case EnergyFault(tag="unresolved_output", unresolved_output=(recipe, declared)):
                return f"{recipe}[{','.join(declared)}]"
            case EnergyFault(tag="unsupported_target", unsupported_target=(target, constraint)):
                return f"{target}[{constraint}]"
            case EnergyFault(tag="district_defects", district_defects=(rows, roster)):
                return f"{rows}[{';'.join(f'{code}={count}' for code, count in roster)}]"
            case EnergyFault(tag="authored_sun", authored_sun=(recipe, coordinate)):
                return f"{recipe}[{coordinate}]"
            case EnergyFault(tag="shading_fidelity", shading_fidelity=(bound, declared, ceiling)):
                return f"{bound}[{declared:.6g}>{ceiling:.6g}]"
            case EnergyFault(tag="shading_census", shading_census=(coordinate, declared, decoded)):
                return f"{coordinate}[{declared}!={decoded}]"
            case EnergyFault(tag="artifact_integrity", artifact_integrity=proof):
                return f"integrity[{proof}]"
            case EnergyFault(tag="artifact_admission", artifact_admission=phase):
                return f"admission[{phase}]"
            case EnergyFault(tag="map_operands", map_operands=(kind, supplied, roster)):
                return f"{kind}[{supplied}>{roster}]"
            case EnergyFault(tag="regime_factor", regime_factor=(action, shape)):
                return f"{action}[{shape}]"
            case _ as unreachable:
                assert_never(unreachable)


# --- [MODELS] ---------------------------------------------------------------------------


class SeriesSpec(Struct, frozen=True):
    field: ClimateField
    window: Option[Window] = Nothing
    statement: Option[str] = Nothing  # the ladybug conditional DSL, e.g. "a > 25"
    unit: Option[str] = Nothing
    reduce: Option[Reduce] = Nothing


class ComfortSpec(Struct, frozen=True):
    model: ComfortModel
    # the CITED comfort standard the kernel scores against, never a bare `Mapping[str, object]` a reader cannot
    # attribute: the regime row names the authority, and its `parameters` factor IS the `*Parameter.from_dict`
    # document the row's own `parameter` late-bind consumes.
    standard: Option[EnergyRegime] = Nothing
    include_wind: bool = True
    include_sun: bool = True


class IndexSpec(Struct, frozen=True):
    model: IndexModel
    constants: Map[str, float] = Map.empty()  # per-name overrides of the row's own constant slots
    window: Option[Window] = Nothing  # applied to every field operand, so the aligned fold stays aligned


class MapSpec(Struct, frozen=True):
    # a spatial map's operands in the kernel's own positional order: the aligned longwave collections a shortwave row
    # leads with, then the recipe-produced artifacts `energy/simulate`'s `matrix` readback addresses. The owner's own
    # `location`/`epw` never appear here — the row's `leads` binds them.
    kind: MapKind
    artifacts: tuple[Path, ...] = ()
    series: "Block[HourlyContinuousCollection]" = Block.empty()


class ComfortRow(Struct, frozen=True, gc=False):
    calc: LateBound  # the ComfortCollection class; `leads=("epw",)` binds the admitted weather to `from_epw`
    parameter: LateBound  # its serializable *Parameter class, reached through `from_dict`
    parameter_kw: str
    results: tuple[str, ...]


class IndexRow(Struct, frozen=True, gc=False):
    kernel: LateBound  # the pointwise scalar kernel `compute_function_aligned` maps
    result: LateBound  # the ladybug datatype class the aligned result carries; instantiated at the fold
    unit: str
    inputs: tuple[IndexInput, ...]


class MapRow(Struct, frozen=True, gc=False):
    kernel: LateBound
    # the kernel's OWN parameter names the operands fill in order — series slot first where the row takes one. The
    # call binds operands by KEYWORD because the shortwave kernel's lead (`location`) PRECEDES its operands, so a
    # positional spill lands the series list on the lead slot and collides with the keyword-bound lead; slots the
    # operands never fill fall to the kernel's own defaults.
    slots: tuple[str, ...]
    bands: tuple[str, ...] = ()  # the kernel's own declared return bands; empty where each row is one caller sensor


class SeriesFact(Struct, frozen=True):
    subject: SeriesSubject
    data_type: str
    unit: str
    period: str
    values: tuple[float, ...]


class SunFact(Struct, frozen=True, gc=False):
    altitude: float
    azimuth: float
    vector: tuple[float, float, float]
    is_during_day: bool


class DayFact(Struct, frozen=True, gc=False):
    sunrise_hoy: float
    noon_hoy: float
    sunset_hoy: float


class ComfortFact(Struct, frozen=True):
    model: ComfortModel
    results: Map[str, tuple[float, ...]]
    percent_comfortable: float
    condition_distribution: Map[int, int]
    hours: int


class MapResult(Struct, frozen=True):
    # `labels` names the kernel's own return bands where it declares them (the TCP triple) and is EMPTY where each row
    # is one of the caller's sensors, so a consumer reads the row regime off the value instead of the kind.
    kind: MapKind
    labels: tuple[str, ...]
    values: tuple[tuple[float, ...], ...]

    @property
    def count(self) -> int:
        return sum(len(row) for row in self.values)


@tagged_union(frozen=True)
class SolarResult:
    tag: Literal["position", "day", "arc", "analemma"] = tag()
    position: SunFact = case()
    day: DayFact = case()
    arc: tuple[tuple[float, float, float], ...] = case()
    analemma: tuple[tuple[float, float, float], ...] = case()

    @property
    def count(self) -> int:
        match self:
            case SolarResult(tag="position"):
                return 1
            case SolarResult(tag="day"):
                return 3
            case SolarResult(tag="arc", arc=points) | SolarResult(tag="analemma", analemma=points):
                return len(points)
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class DerivedDocument:
    # the ladybug documents the derive arm projects, each named rather than erased to `object`, so a recipe-boundary
    # consumer matches the arm instead of re-discriminating by `isinstance` outside this owner.
    tag: Literal["wea", "ddy", "design_days", "location"] = tag()
    wea: "Wea" = case()
    ddy: "DDY" = case()
    design_days: "tuple[DesignDay, ...]" = case()
    location: "Location" = case()

    @property
    def count(self) -> int:
        match self:
            case DerivedDocument(tag="design_days", design_days=days):
                return len(days)
            case DerivedDocument(tag="wea") | DerivedDocument(tag="ddy") | DerivedDocument(tag="location"):
                return 1
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class ClimateQuery:
    # the one read request over an admitted EPW: modality is the case, never a method name.
    tag: Literal["series", "derive", "solar", "comfort", "comfort_map", "mrt", "index"] = tag()
    series: SeriesSpec = case()
    derive: Derived = case()
    solar: SolarQuery = case()
    comfort: ComfortSpec = case()
    comfort_map: MapSpec = case()
    mrt: "HourlyContinuousCollection" = case()  # surface temperatures for a caller feeding a non-EPW thermal model
    index: IndexSpec = case()


@tagged_union(frozen=True)
class ClimateResult:
    # mirrors the request: `series`, `index`, and `mrt` all resolve to one aligned collection, so ONE `SeriesFact` arm
    # carries the three and the union stays as narrow as the shapes it actually distinguishes.
    tag: Literal["series", "comfort", "solar", "derived", "mapped"] = tag()
    series: SeriesFact = case()
    comfort: ComfortFact = case()
    solar: SolarResult = case()
    derived: DerivedDocument = case()
    mapped: MapResult = case()


class ClimateReceipt(Struct, frozen=True):
    content_key: ContentKey
    latitude: float
    longitude: float
    elevation: float
    time_zone: float
    is_leap_year: bool
    operation: str
    discriminant: str
    count: int
    comfortable: Option[float] = Nothing

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of(
            "rasm.geometry.energy.climate",
            (
                "emitted",
                f"{self.operation}:{self.discriminant}",
                {
                    "count": self.count,
                    "latitude": self.latitude,
                    "longitude": self.longitude,
                    "content_key": self.content_key.hex,
                    # a read that took no comfort pass OMITS the key: a sentinel percentage is a reading no producer
                    # made, and a dashboard cannot tell one from a real one.
                    **self.comfortable.map(lambda pct: {"comfortable": pct}).default_value({}),
                },
            ),
        )

    def spec(self) -> bytes:
        # the evidence subject IS the admitted weather beside the query that read it, so two reads over one EPW key
        # apart and an identical re-read dedupes in the persistence ledger without a caller-minted key.
        return b"|".join((self.content_key.memory, self.operation.encode(), self.discriminant.encode()))

    def graduates(self, regime: EnergyRegime = ENERGY_REGIMES[RegimeKey.THERMAL_DISCOMFORT]) -> GeometryHandoff:
        # discomfort is the comfort fold's own measurement; every other read OMITS it, so the spine reports
        # `unmeasured:discomfort` and refuses honestly rather than clearing the ceiling on a fabricated zero. The bar
        # arrives as a CITED regime row rather than an anonymous float, so the verdict names the standard it graded
        # against and a caller overriding it declares its own authority instead of a number.
        measured = self.comfortable.map(lambda pct: {"discomfort": 1.0 - pct / 100.0}).default_value({}) | {"count": float(self.count)}
        subject = GeometrySubject.THERMAL_COMFORT
        return GeometryHandoff.of(subject, evidence_key(subject, self.spec()), measured, {"discomfort": regime.bar()})


# --- [SERVICES] -------------------------------------------------------------------------


class Climate(Struct, frozen=True):
    epw: "EPW"
    location: "Location"
    content_key: ContentKey
    composition: ScopeKey = DEFAULT_SCOPE

    @classmethod
    def of(
        cls, source: "bytes | str | Path | Mapping[str, object]", *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[tuple[Self, ClimateReceipt]]":
        def admit() -> tuple[Self, ClimateReceipt]:
            from ladybug.epw import EPW  # ruff:ignore[import-outside-top-level] — AGPL isolation: the lexical coupling stays inside the boundary seam

            match source:
                case bytes() as raw:
                    epw, shape = EPW.from_file_string(raw.decode()), "bytes"
                case Mapping() as data:
                    epw, shape = EPW.from_dict(dict(data)), "mapping"
                case at:
                    epw, shape = EPW(str(at)), "path"
            admitted = cls(
                epw=epw,
                location=epw.location,
                content_key=ContentIdentity.key("weather", epw.to_file_string().encode()),
                composition=composition,
            )
            return admitted, admitted._receipt("admit", shape, len(epw.dry_bulb_temperature.values))

        return evidence_run(EvidenceScope.ENERGY_CLIMATE, "admit", admit, composition=composition)

    @overload
    def query(self, q: ClimateQuery) -> "RuntimeRail[tuple[ClimateResult, ClimateReceipt]]": ...
    @overload
    def query(self, q: Sequence[ClimateQuery]) -> "RuntimeRail[Block[tuple[ClimateResult, ClimateReceipt]]]": ...
    def query(
        self, q: "ClimateQuery | Sequence[ClimateQuery]"
    ) -> "RuntimeRail[tuple[ClimateResult, ClimateReceipt]] | RuntimeRail[Block[tuple[ClimateResult, ClimateReceipt]]]":
        # arity is the argument's own shape; each member keeps its own weave span and its own receipt, so a batch of N
        # reads emits N evidence rows rather than the last one overwriting its siblings.
        match q:
            case ClimateQuery() as one:
                return self._routed(one)
            case batch:
                return traversed(Block.of_seq([self._routed(one) for one in batch]), by=Disposition.ACCUMULATE)

    def _routed(self, q: ClimateQuery) -> "RuntimeRail[tuple[ClimateResult, ClimateReceipt]]":
        return evidence_run(EvidenceScope.ENERGY_CLIMATE, f"query.{q.tag}", partial(self._dispatch, q), composition=self.composition)

    def _dispatch(self, q: ClimateQuery) -> tuple[ClimateResult, ClimateReceipt]:
        # one total fold over the request union; every arm builds its receipt off facts the fold already holds, so no
        # caller ever hand-asserts a count, a coordinate, or a comfort percentage.
        match q:
            case ClimateQuery(tag="series", series=spec):
                fact = _series(SeriesSubject(field=spec.field), _reduced(spec, _windowed(getattr(self.epw, spec.field.value), spec.window)))
                return ClimateResult(series=fact), self._receipt("series", spec.field.value, len(fact.values))
            case ClimateQuery(tag="index", index=spec):
                fact = _series(SeriesSubject(index=spec.model), _indexed(self, spec))
                return ClimateResult(series=fact), self._receipt("index", spec.model.value, len(fact.values))
            case ClimateQuery(tag="mrt", mrt=surfaces):
                fact = _series(SeriesSubject(mrt=None), _mrt(self, surfaces))
                return ClimateResult(series=fact), self._receipt("mrt", fact.subject.label, len(fact.values))
            case ClimateQuery(tag="derive", derive=kind):
                document = _derived(self, kind)
                return ClimateResult(derived=document), self._receipt("derive", kind.tag, document.count)
            case ClimateQuery(tag="solar", solar=query):
                result = _solar(self.location, query)
                return ClimateResult(solar=result), self._receipt("solar", query.tag, result.count)
            case ClimateQuery(tag="comfort", comfort=spec):
                fact = _comfort(self, spec)
                # charter row at the producing fold, spelling derived from THERMAL_COMFORT: the discomfort fraction is
                # the comfortable percentage's complement by definition, so no second parser member is claimed.
                charter_record(GeometrySubject.THERMAL_COMFORT, {"discomfort": 1.0 - fact.percent_comfortable / 100.0}, composition=self.composition)
                return ClimateResult(comfort=fact), self._receipt("comfort", spec.model.value, fact.hours, Some(fact.percent_comfortable))
            case ClimateQuery(tag="comfort_map", comfort_map=spec):
                result = _mapped(self, spec)
                return ClimateResult(mapped=result), self._receipt("comfort_map", spec.kind.value, result.count)
            case _ as unreachable:
                assert_never(unreachable)

    def _receipt(self, operation: str, discriminant: str, count: int, comfortable: Option[float] = Nothing) -> ClimateReceipt:
        return ClimateReceipt(
            content_key=self.content_key,
            latitude=self.location.latitude,
            longitude=self.location.longitude,
            elevation=self.location.elevation,
            time_zone=self.location.time_zone,
            is_leap_year=self.epw.is_leap_year,
            operation=operation,
            discriminant=discriminant,
            count=count,
            comfortable=comfortable,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _windowed(collection: object, window: Option[Window]) -> object:
    from ladybug.analysisperiod import AnalysisPeriod  # ruff:ignore[import-outside-top-level] — AGPL isolation: the lexical coupling stays inside the boundary seam

    return window.map(lambda w: collection.filter_by_analysis_period(AnalysisPeriod(*w))).default_value(collection)


def _reduced(spec: SeriesSpec, windowed: object) -> object:
    filtered = spec.statement.map(windowed.filter_by_conditional_statement).default_value(windowed)
    converted = spec.unit.map(filtered.to_unit).default_value(filtered)
    return spec.reduce.map(
        lambda r: getattr(converted, r.method)(r.percentile[1]) if r.tag == "percentile" else getattr(converted, r.method)()
    ).default_value(converted)


def _series(subject: SeriesSubject, collection: object) -> SeriesFact:
    # the one projection every collection-returning arm folds through: series, index, and MRT all land here, so the
    # header vocabulary is read once and the three arms share one fact shape.
    header = collection.header
    return SeriesFact(
        subject=subject, data_type=str(header.data_type), unit=header.unit, period=str(header.analysis_period), values=tuple(collection.values)
    )


def _derived(climate: Climate, kind: Derived) -> DerivedDocument:
    from ladybug.ddy import DDY  # ruff:ignore[import-outside-top-level] — AGPL isolation: the lexical coupling stays inside the boundary seam
    from ladybug.wea import Wea  # ruff:ignore[import-outside-top-level] — AGPL isolation

    # EPW.to_wea/to_ddy take a file_path and WRITE files; the in-memory projections are the Wea radiation constructor
    # and DDY over the percentile-selected design days.
    match kind:
        case Derived(tag="wea"):
            return DerivedDocument(wea=Wea(climate.location, climate.epw.direct_normal_radiation, climate.epw.diffuse_horizontal_radiation))
        case Derived(tag="ddy", ddy=percentile):
            return DerivedDocument(ddy=DDY(climate.location, climate.epw.best_available_design_days(percentile)))
        case Derived(tag="design_days"):
            return DerivedDocument(design_days=tuple(climate.epw.best_available_design_days()))
        case Derived(tag="location"):
            return DerivedDocument(location=climate.location)
        case _ as unreachable:
            assert_never(unreachable)


def _solar(location: "Location", query: SolarQuery) -> SolarResult:
    from ladybug.sunpath import Sunpath  # ruff:ignore[import-outside-top-level] — AGPL isolation: the lexical coupling stays inside the boundary seam

    path = Sunpath.from_location(location)
    match query:
        case SolarQuery(tag="position", position=hoy):
            sun = path.calculate_sun_from_hoy(hoy)
            return SolarResult(
                position=SunFact(altitude=sun.altitude, azimuth=sun.azimuth, vector=tuple(sun.sun_vector.to_array()), is_during_day=sun.is_during_day)
            )
        case SolarQuery(tag="day", day=(month, day)):
            edges = path.calculate_sunrise_sunset(month, day)
            return SolarResult(day=DayFact(sunrise_hoy=edges["sunrise"].hoy, noon_hoy=edges["noon"].hoy, sunset_hoy=edges["sunset"].hoy))
        case SolarQuery(tag="arc", arc=(month, day)):
            return SolarResult(arc=tuple(path.day_arc3d(month, day).to_polyline(24).to_array()))
        case SolarQuery(tag="analemma", analemma=hour):
            return SolarResult(analemma=tuple(path.hourly_analemma_polyline3d()[hour].to_array()))
        case _ as unreachable:
            assert_never(unreachable)


def _comfort(climate: Climate, spec: ComfortSpec) -> ComfortFact:
    row = COMFORT[spec.model]
    parameter = spec.standard.map(lambda regime: row.parameter.resolve().from_dict(dict(regime.document()))).to_optional()
    calc = row.calc.resolve().from_epw(
        **row.calc.bound(climate), include_wind=spec.include_wind, include_sun=spec.include_sun, **{row.parameter_kw: parameter}
    )
    condition = calc.thermal_condition
    distribution = Block.of_seq(condition.values).fold(lambda acc, value: acc.change(int(value), lambda n: Some(n.default_value(0) + 1)), Map.empty())
    return ComfortFact(
        model=spec.model,
        results=Map.of_seq([(name, tuple(getattr(calc, name).values)) for name in row.results]),
        percent_comfortable=calc.percent_comfortable,
        condition_distribution=distribution,
        hours=len(condition.values),
    )


def _mrt(climate: Climate, surfaces: object) -> object:
    # OutdoorSolarCal over the EPW's own solar fields; `from_epw` internalizes this path for the COMFORT rows, and the
    # WBGT index row reaches it through the same fold rather than a second SolarCal construction.
    from ladybug_comfort.collection.solarcal import OutdoorSolarCal  # ruff:ignore[import-outside-top-level] — AGPL isolation: the lexical coupling stays inside the boundary seam

    return OutdoorSolarCal(
        climate.location,
        climate.epw.direct_normal_radiation,
        climate.epw.diffuse_horizontal_radiation,
        climate.epw.horizontal_infrared_radiation_intensity,
        surfaces,
    ).mean_radiant_temperature


def _operand(climate: Climate, slot: IndexInput, spec: IndexSpec) -> object:
    match slot:
        case IndexInput(tag="field", field=field):
            return _windowed(getattr(climate.epw, field.value), spec.window)
        case IndexInput(tag="mrt"):
            # surface temperature is the EPW dry bulb — the outdoor ground-at-air-temperature assumption SolarCal is
            # posed against — so a WBGT read needs no caller input the weather file already answers.
            return _windowed(_mrt(climate, climate.epw.dry_bulb_temperature), spec.window)
        case IndexInput(tag="constant", constant=(name, default)):
            return spec.constants.try_find(name).or_else(default).default_with(lambda: _demanded(spec.model, name))
        case _ as unreachable:
            assert_never(unreachable)


def _demanded[T](model: IndexModel, name: str) -> T:
    # the slot's refusal names the model and the operand a caller must supply; converted once by the evidence_run fence.
    raise EnergyFault(index_constant=(model.value, name))


def _indexed(climate: Climate, spec: IndexSpec) -> object:
    from ladybug.datacollection import HourlyContinuousCollection  # ruff:ignore[import-outside-top-level] — AGPL isolation: the lexical coupling stays inside the boundary seam

    # `compute_function_aligned` admits a mixed operand list of collections and scalars, proves alignment itself, and
    # rebuilds the result under the row's own datatype and unit — so the index band re-derives no alignment check, no
    # per-hour loop, and no header.
    row = INDEX[spec.model]
    operands = [_operand(climate, slot, spec) for slot in row.inputs]
    return HourlyContinuousCollection.compute_function_aligned(row.kernel.resolve(), operands, row.result.resolve()(), row.unit)


def _floats(row: object) -> tuple[float, ...]:
    # a map kernel hands back either a ladybug collection per sensor (the MRT rows) or a bare scalar sequence (the TCP
    # bands, the air matrix rows); `values` is the collection's own projection, so one fold reads both regimes.
    return tuple(float(value) for value in getattr(row, "values", row))


def _mapped(climate: Climate, spec: MapSpec) -> MapResult:
    row = MAPS[spec.kind]
    operands = ((list(spec.series),) if not spec.series.is_empty() else ()) + spec.artifacts
    if len(operands) > len(row.slots):
        raise EnergyFault(map_operands=(spec.kind.value, len(operands), len(row.slots)))
    returned = row.kernel.resolve()(**dict(zip(row.slots, operands)), **row.kernel.bound(climate))
    return MapResult(kind=spec.kind, labels=row.bands, values=tuple(_floats(band) for band in returned))


# --- [COMPOSITION] ----------------------------------------------------------------------

# spatial comfort-map kernels over the `energy/simulate` readback addresses; `leads` binds the owner argument each
# kernel names — the location the shortwave row takes, the weather the longwave and air rows take.
MAPS: Final[Map[MapKind, MapRow]] = Map.of_seq([
    (
        MapKind.SHORTWAVE_MRT,
        MapRow(
            kernel=LateBound("ladybug_comfort.map.mrt", "shortwave_mrt_map", leads=("location",)),
            slots=("longwave_data", "sun_up_hours", "indirect_ill", "direct_ill", "ref_ill", "contributions", "transmittance_contribs"),
        ),
    ),
    (
        MapKind.LONGWAVE_MRT,
        MapRow(
            kernel=LateBound("ladybug_comfort.map.mrt", "longwave_mrt_map", leads=("epw",)),
            slots=("enclosure_info", "modifiers", "sql", "view_factors"),
        ),
    ),
    (MapKind.TCP, MapRow(kernel=LateBound("ladybug_comfort.map.tcp", "tcp_total"), slots=("condition_csv", "schedule"), bands=("tcp", "hsp", "csp"))),
    (MapKind.AIR, MapRow(kernel=LateBound("ladybug_comfort.map.air", "air_map", leads=("epw",)), slots=("enclosure_info", "sql"))),
])

COMFORT: Final[Map[ComfortModel, ComfortRow]] = Map.of_seq([
    (
        ComfortModel.PMV,
        ComfortRow(
            calc=LateBound("ladybug_comfort.collection.pmv", "PMV", leads=("epw",)),
            parameter=LateBound("ladybug_comfort.parameter.pmv", "PMVParameter"),
            parameter_kw="pmv_parameter",
            results=("predicted_mean_vote", "percentage_people_dissatisfied", "standard_effective_temperature"),
        ),
    ),
    (
        ComfortModel.UTCI,
        ComfortRow(
            calc=LateBound("ladybug_comfort.collection.utci", "UTCI", leads=("epw",)),
            parameter=LateBound("ladybug_comfort.parameter.utci", "UTCIParameter"),
            parameter_kw="utci_parameter",
            results=("universal_thermal_climate_index",),
        ),
    ),
    (
        ComfortModel.PET,
        ComfortRow(
            calc=LateBound("ladybug_comfort.collection.pet", "PET", leads=("epw",)),
            parameter=LateBound("ladybug_comfort.parameter.pet", "PETParameter"),
            parameter_kw="body_parameter",  # PET.from_epw spells its PETParameter slot body_parameter, not pet_parameter
            results=("physiologic_equivalent_temperature", "core_body_temperature", "skin_temperature"),
        ),
    ),
])

# the scalar index band: every operand is an admitted EPW field, the derived SolarCal MRT, or a named constant, and
# every result datatype and unit is the ladybug registry entry the aligned collection carries.
_TEMPERATURE: Final = "ladybug.datatype.temperature"
_DEGREE_TIME: Final = "ladybug.datatype.temperaturetime"

INDEX: Final[Map[IndexModel, IndexRow]] = Map.of_seq([
    (
        IndexModel.APPARENT,
        IndexRow(
            kernel=LateBound("ladybug_comfort.at", "apparent_temperature"),
            result=LateBound(_TEMPERATURE, "Temperature"),
            unit="C",
            inputs=(
                IndexInput(field=ClimateField.DRY_BULB_TEMPERATURE),
                IndexInput(field=ClimateField.RELATIVE_HUMIDITY),
                IndexInput(field=ClimateField.WIND_SPEED),
            ),
        ),
    ),
    (
        IndexModel.DISCOMFORT,
        IndexRow(
            kernel=LateBound("ladybug_comfort.di", "discomfort_index"),
            result=LateBound(_TEMPERATURE, "Temperature"),
            unit="C",
            inputs=(IndexInput(field=ClimateField.DRY_BULB_TEMPERATURE), IndexInput(field=ClimateField.RELATIVE_HUMIDITY)),
        ),
    ),
    (
        IndexModel.HEAT,
        IndexRow(
            kernel=LateBound("ladybug_comfort.hi", "heat_index"),
            result=LateBound(_TEMPERATURE, "HeatIndexTemperature"),
            unit="C",
            inputs=(IndexInput(field=ClimateField.DRY_BULB_TEMPERATURE), IndexInput(field=ClimateField.RELATIVE_HUMIDITY)),
        ),
    ),
    (
        IndexModel.HUMIDEX,
        IndexRow(
            kernel=LateBound("ladybug_comfort.humidex", "humidex"),
            result=LateBound(_TEMPERATURE, "Temperature"),
            unit="C",
            inputs=(IndexInput(field=ClimateField.DRY_BULB_TEMPERATURE), IndexInput(field=ClimateField.DEW_POINT_TEMPERATURE)),
        ),
    ),
    (
        IndexModel.WBGT,
        IndexRow(
            kernel=LateBound("ladybug_comfort.wbgt", "wet_bulb_globe_temperature"),
            result=LateBound(_TEMPERATURE, "WetBulbGlobeTemperature"),
            unit="C",
            inputs=(
                IndexInput(field=ClimateField.DRY_BULB_TEMPERATURE),
                IndexInput(mrt=None),
                IndexInput(field=ClimateField.WIND_SPEED),
                IndexInput(field=ClimateField.RELATIVE_HUMIDITY),
            ),
        ),
    ),
    (
        IndexModel.WINDCHILL,
        IndexRow(
            kernel=LateBound("ladybug_comfort.wc", "windchill_temp"),
            result=LateBound(_TEMPERATURE, "WindChillTemperature"),
            unit="C",
            inputs=(IndexInput(field=ClimateField.DRY_BULB_TEMPERATURE), IndexInput(field=ClimateField.WIND_SPEED)),
        ),
    ),
    (
        IndexModel.SENSATION,
        IndexRow(
            kernel=LateBound("ladybug_comfort.ts", "thermal_sensation"),
            result=LateBound("ladybug.datatype.thermalcondition", "ThermalCondition"),
            unit="condition",
            inputs=(
                IndexInput(field=ClimateField.DRY_BULB_TEMPERATURE),
                IndexInput(field=ClimateField.WIND_SPEED),
                IndexInput(field=ClimateField.RELATIVE_HUMIDITY),
                IndexInput(field=ClimateField.GLOBAL_HORIZONTAL_RADIATION),
                # ground surface temperature has no weather-file source and no package default, so the slot demands
                # the caller's value and refuses by name rather than inventing one.
                IndexInput(constant=("tground", Nothing)),
            ),
        ),
    ),
    (
        IndexModel.HEATING_DEGREE,
        IndexRow(
            kernel=LateBound("ladybug_comfort.degreetime", "heating_degree_time"),
            result=LateBound(_DEGREE_TIME, "HeatingDegreeTime"),
            unit="degC-hours",
            inputs=(
                IndexInput(field=ClimateField.DRY_BULB_TEMPERATURE),
                IndexInput(constant=("t_base", ENERGY_REGIMES[RegimeKey.HEATING_BALANCE].magnitude)),
            ),
        ),
    ),
    (
        IndexModel.COOLING_DEGREE,
        IndexRow(
            kernel=LateBound("ladybug_comfort.degreetime", "cooling_degree_time"),
            result=LateBound(_DEGREE_TIME, "CoolingDegreeTime"),
            unit="degC-hours",
            inputs=(
                IndexInput(field=ClimateField.DRY_BULB_TEMPERATURE),
                IndexInput(constant=("t_base", ENERGY_REGIMES[RegimeKey.COOLING_BALANCE].magnitude)),
            ),
        ),
    ),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
