# [PY_GEOMETRY_ENERGY_CLIMATE]

`Climate` owns the energy plane's weather substrate and its point-comfort tier — the plane's ONLY solar owner. `Climate.of` admits EPW weather by payload shape, and ONE polymorphic `query` answers every read that document supports: `series` collection algebra, `derive` projections, `solar` sun-geometry, `comfort` point-comfort with SolarCal MRT, the `index` scalar band, the `mrt` SolarCal series, and the spatial `comfort_map` readback — one `ClimateQuery` case in, one `ClimateResult` arm out, one `_dispatch` closing on `assert_never`. `ladybug-core` and `ladybug-comfort` own every weather, psychrometric, comfort, and index kernel; this page composes them into typed, railed results.

Ladybug's AGPL-3.0 network-copyleft band rides the standing companion-lane charter: `ladybug`/`ladybug_comfort` reach the interpreter through `LateBound` — the folder's ONE late-binding owner, holding its module as a STRING and resolving at the call seam — or through a function-local import inside a boundary seam, never a module-scope binding of either form, because a static license audit reads the LEXICAL import graph and a module-scope statement (`lazy` included, the soft keyword being module-scope by design) couples every importer of this page to AGPL; confinement is the point, not deferral. Evidence crosses the wire, never a link into a distributed host binary. Every query threads runtime observation through `evidence_run` under the owner's own `composition` key, and the admitted document keys once over its canonical file-string bytes so re-ingestion dedupes in the persistence ledger.

## [01]-[INDEX]

- [02]-[CLIMATE]: one polymorphic weather owner — EPW admission and one `ClimateQuery`/`ClimateResult` read surface over series, derived documents, sun geometry, comfort, scalar indices, MRT, and spatial maps.

## [02]-[CLIMATE]

- Owner: `Climate` holds the decoded `EPW`, its `Location`, the weather `ContentKey`, and the `composition` custody key every weave and charter record stamps. `ClimateField` members ARE the `EPW` collection-property names across the whole EPW data band, so field access is one `getattr`; `Reduce.method` derives its ladybug spelling from its own fields; `LateBound` carries the module, the member, and the roster of owner attributes its kernel takes, so `COMFORT`, `MAPS`, and `INDEX` share ONE resolution fold and the page holds no second `import_module` site. `EnergyFault` seats beside `LateBound` for the same reachability reason — every band page already imports this owner — so the whole band's refusal vocabulary is ONE closed family rather than four coordinate-string grammars a consumer re-parses per page.
- Entry: `Climate.of` admits `bytes | str | Path | Mapping` by payload shape — modality IS the shape, never a `source_kind=` knob. `query` is the one read surface over `ClimateQuery | Sequence[ClimateQuery]`; a batch accumulates through `traversed(ACCUMULATE)` so a refused member names itself beside its siblings. `comfort` routes the EPW solar fields through SolarCal to the `rad_temperature` MRT input internally; the `mrt` case is the explicit arm only for a caller feeding a non-EPW thermal model, and the `index` band's own MRT slot resolves through that same fold.
- Auto: every fold returns its canonical `ClimateResult` directly. Collection alignment is `compute_function_aligned`'s own precondition over the aligned operand list, so the index band never re-derives it; `Wea`/`DDY` projections serialize through their own `to_file_string`, reaching the recipe boundary as handler inputs, never a second reader; `comfort_map` rows consume the recipe products `energy/simulate`'s `matrix` readback surfaces, so a spatial map reads real addresses rather than a feed no producer emits.
- Output: the comfort result already carries `percent_comfortable`, the only query-specific fact the charter needs; that fold records discomfort at the producing site. Other queries return their own typed result arm without a parallel metadata object.
- Packages: `ladybug-core` and `ladybug-comfort` per the table rows; the spatial `map.*` kernels are the `energy/simulate` readback surface, and a `LateBound` resolution or a boundary-seam function-local import is the seam every one of them crosses.
- Growth: a new climate read is one `ClimateQuery` case, one `_dispatch` arm, and one `ClimateResult` arm; a new field is one `ClimateField` member; a new grain one `Grain` member, method name deriving; a new projection one `Derived` case beside its `DerivedDocument` arm, IP-units over `EPW.convert_to_ip` the named next; a new comfort model one `COMFORT` row; a new scalar index one `INDEX` row over its `IndexInput` roster; a new spatial map one `MAPS` row; a new band refusal is one `EnergyFault` case carrying its own coordinate tuple, minted at the page that raises it and read by every consumer off the tag. `Adaptive` is not weather-drivable — it builds `from_air_and_rad_temp` over model results, so its home is the `energy/simulate` readback, its `AdaptiveParameter` already serializable through this page's parameter discipline; urban-microclimate EPW morphing enters as one more `Derived` case with its own package admission.
- Boundary: `SolarQuery` reads the WEATHER file's own sun and a captured scene descriptor carries angles a peer already solved — `energy/simulate` projects those angles straight onto a sky, since routing them back through `Sunpath` re-derives an ephemeris the descriptor settled and silently substitutes a second almanac's answer.
- Boundary: no diagram furniture — artifacts owns the sun-path diagram and `Sunpath` gains no diagram consumer here; no radiance simulation (the recipe rail owns it), no HBJSON model semantics (`energy/model` owns them), no chart/legend composition (artifacts-plane material), and no re-derived solar vector algebra — `Sunpath` emits `ladybug_geometry` primitives and this owner projects them to arrays.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Mapping, Sequence
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
    GeometrySubject,
    charter_record,
    evidence_run,
)
from rasm.runtime.faults import Disposition, RuntimeRail, traversed
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey

if TYPE_CHECKING:
    from ladybug.datacollection import HourlyContinuousCollection
    from ladybug.ddy import DDY
    from ladybug.designday import DesignDay
    from ladybug.epw import EPW
    from ladybug.location import Location
    from ladybug.wea import Wea

# --- [TYPES] ----------------------------------------------------------------------------

type Window = tuple[int, int, int, int, int, int]


class ClimateField(StrEnum):
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
    PMV = "pmv"
    UTCI = "utci"
    PET = "pet"


class IndexModel(StrEnum):
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
    module: str
    member: str
    leads: tuple[str, ...] = ()

    def resolve(self) -> object:
        return getattr(import_module(self.module), self.member)

    def bound(self, owner: object) -> Mapping[str, object]:
        return {name: getattr(owner, name) for name in self.leads}


class RegimeAction(StrEnum):
    DEGREE_BASE = "degree-base"
    COMFORT_STANDARD = "comfort-standard"
    COMPLIANCE_CEILING = "compliance-ceiling"


@tagged_union(frozen=True)
class RegimeFactor:
    tag: Literal["magnitude", "parameters"] = tag()
    magnitude: float = case()
    parameters: Mapping[str, object] = case()


class EnergyRegime(Struct, frozen=True, gc=False):
    action: RegimeAction
    factor: RegimeFactor
    citation: str
    kernel: Option[LateBound] = Nothing

    @property
    def magnitude(self) -> Option[float]:
        return Some(self.factor.magnitude) if self.factor.tag == "magnitude" else Nothing

    def bar(self) -> float:
        match self.factor:
            case RegimeFactor(tag="magnitude", magnitude=value):
                return value
            case RegimeFactor(tag="parameters", parameters=document):
                raise EnergyFault(regime_factor=(self.action.value, f"non-scalar:{len(document)}"))
            case _ as unreachable:
                assert_never(unreachable)

    def document(self) -> Mapping[str, object]:
        match self.factor:
            case RegimeFactor(tag="parameters", parameters=document):
                return document
            case RegimeFactor(tag="magnitude", magnitude=value):
                raise EnergyFault(regime_factor=(self.action.value, f"non-document:{value:.6g}"))
            case _ as unreachable:
                assert_never(unreachable)


class RegimeKey(StrEnum):
    HEATING_BALANCE = "heating-balance"
    COOLING_BALANCE = "cooling-balance"
    MODEL_VALIDITY = "model-validity"
    DISTRICT_DEFECTS = "district-defects"
    THERMAL_DISCOMFORT = "thermal-discomfort"
    BUILDING_EUI = "building-eui"


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
    tag: Literal["field", "mrt", "constant"] = tag()
    field: ClimateField = case()
    mrt: None = case()
    constant: tuple[str, Option[float]] = case()


@tagged_union(frozen=True)
class SeriesSubject:
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
    tag: Literal[
        "empty_model", "index_constant", "unknown_output", "unresolved_output", "unsupported_target",
        "district_defects", "authored_sun", "shading_fidelity", "shading_census",
        "artifact_integrity", "artifact_admission",
        "map_operands", "regime_factor",
    ] = tag()
    empty_model: tuple[str, int] = case()
    index_constant: tuple[str, str] = case()
    unknown_output: tuple[tuple[str, ...], int] = case()
    unresolved_output: tuple[str, tuple[str, ...]] = case()
    unsupported_target: tuple[str, str] = case()
    district_defects: tuple[int, tuple[tuple[str, int], ...]] = case()
    authored_sun: tuple[str, str] = case()
    shading_fidelity: tuple[str, float, float] = case()
    shading_census: tuple[str, int, int] = case()
    artifact_integrity: str = case()
    artifact_admission: str = case()
    map_operands: tuple[str, int, int] = case()
    regime_factor: tuple[str, str] = case()

    def __str__(self) -> str:
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
    statement: Option[str] = Nothing
    unit: Option[str] = Nothing
    reduce: Option[Reduce] = Nothing


class ComfortSpec(Struct, frozen=True):
    model: ComfortModel
    standard: Option[EnergyRegime] = Nothing
    include_wind: bool = True
    include_sun: bool = True


class IndexSpec(Struct, frozen=True):
    model: IndexModel
    constants: Map[str, float] = Map.empty()
    window: Option[Window] = Nothing


class MapSpec(Struct, frozen=True):
    kind: MapKind
    artifacts: tuple[Path, ...] = ()
    series: "Block[HourlyContinuousCollection]" = Block.empty()


class ComfortRow(Struct, frozen=True, gc=False):
    calc: LateBound
    parameter: LateBound
    parameter_kw: str
    results: tuple[str, ...]


class IndexRow(Struct, frozen=True, gc=False):
    kernel: LateBound
    result: LateBound
    unit: str
    inputs: tuple[IndexInput, ...]


class MapRow(Struct, frozen=True, gc=False):
    kernel: LateBound
    slots: tuple[str, ...]
    bands: tuple[str, ...] = ()


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
    tag: Literal["series", "derive", "solar", "comfort", "comfort_map", "mrt", "index"] = tag()
    series: SeriesSpec = case()
    derive: Derived = case()
    solar: SolarQuery = case()
    comfort: ComfortSpec = case()
    comfort_map: MapSpec = case()
    mrt: "HourlyContinuousCollection" = case()
    index: IndexSpec = case()


@tagged_union(frozen=True)
class ClimateResult:
    tag: Literal["series", "comfort", "solar", "derived", "mapped"] = tag()
    series: SeriesFact = case()
    comfort: ComfortFact = case()
    solar: SolarResult = case()
    derived: DerivedDocument = case()
    mapped: MapResult = case()


# --- [SERVICES] -------------------------------------------------------------------------


class Climate(Struct, frozen=True):
    epw: "EPW"
    location: "Location"
    content_key: ContentKey
    composition: ScopeKey = DEFAULT_SCOPE

    @classmethod
    def of(
        cls, source: "bytes | str | Path | Mapping[str, object]", *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[Self]":
        def admit() -> Self:
            from ladybug.epw import EPW

            match source:
                case bytes() as raw:
                    epw = EPW.from_file_string(raw.decode())
                case Mapping() as data:
                    epw = EPW.from_dict(dict(data))
                case at:
                    epw = EPW(str(at))
            return cls(
                epw=epw,
                location=epw.location,
                content_key=ContentIdentity.key("weather", epw.to_file_string().encode()),
                composition=composition,
            )

        return evidence_run(EvidenceScope.ENERGY_CLIMATE, "admit", admit, composition=composition)

    @overload
    def query(self, q: ClimateQuery) -> "RuntimeRail[ClimateResult]": ...
    @overload
    def query(self, q: Sequence[ClimateQuery]) -> "RuntimeRail[Block[ClimateResult]]": ...
    def query(
        self, q: "ClimateQuery | Sequence[ClimateQuery]"
    ) -> "RuntimeRail[ClimateResult] | RuntimeRail[Block[ClimateResult]]":
        match q:
            case ClimateQuery() as one:
                return self._routed(one)
            case batch:
                return traversed(Block.of_seq([self._routed(one) for one in batch]), by=Disposition.ACCUMULATE)

    def _routed(self, q: ClimateQuery) -> "RuntimeRail[ClimateResult]":
        return evidence_run(EvidenceScope.ENERGY_CLIMATE, f"query.{q.tag}", partial(self._dispatch, q), composition=self.composition)

    def _dispatch(self, q: ClimateQuery) -> ClimateResult:
        match q:
            case ClimateQuery(tag="series", series=spec):
                fact = _series(SeriesSubject(field=spec.field), _reduced(spec, _windowed(getattr(self.epw, spec.field.value), spec.window)))
                return ClimateResult(series=fact)
            case ClimateQuery(tag="index", index=spec):
                fact = _series(SeriesSubject(index=spec.model), _indexed(self, spec))
                return ClimateResult(series=fact)
            case ClimateQuery(tag="mrt", mrt=surfaces):
                fact = _series(SeriesSubject(mrt=None), _mrt(self, surfaces))
                return ClimateResult(series=fact)
            case ClimateQuery(tag="derive", derive=kind):
                return ClimateResult(derived=_derived(self, kind))
            case ClimateQuery(tag="solar", solar=query):
                return ClimateResult(solar=_solar(self.location, query))
            case ClimateQuery(tag="comfort", comfort=spec):
                fact = _comfort(self, spec)
                charter_record(GeometrySubject.THERMAL_COMFORT, {"discomfort": 1.0 - fact.percent_comfortable / 100.0}, composition=self.composition)
                return ClimateResult(comfort=fact)
            case ClimateQuery(tag="comfort_map", comfort_map=spec):
                return ClimateResult(mapped=_mapped(self, spec))
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _windowed(collection: object, window: Option[Window]) -> object:
    from ladybug.analysisperiod import AnalysisPeriod

    return window.map(lambda w: collection.filter_by_analysis_period(AnalysisPeriod(*w))).default_value(collection)


def _reduced(spec: SeriesSpec, windowed: object) -> object:
    filtered = spec.statement.map(windowed.filter_by_conditional_statement).default_value(windowed)
    converted = spec.unit.map(filtered.to_unit).default_value(filtered)
    return spec.reduce.map(
        lambda r: getattr(converted, r.method)(r.percentile[1]) if r.tag == "percentile" else getattr(converted, r.method)()
    ).default_value(converted)


def _series(subject: SeriesSubject, collection: object) -> SeriesFact:
    header = collection.header
    return SeriesFact(
        subject=subject, data_type=str(header.data_type), unit=header.unit, period=str(header.analysis_period), values=tuple(collection.values)
    )


def _derived(climate: Climate, kind: Derived) -> DerivedDocument:
    from ladybug.ddy import DDY
    from ladybug.wea import Wea

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
    from ladybug.sunpath import Sunpath

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
    from ladybug_comfort.collection.solarcal import OutdoorSolarCal

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
            return _windowed(_mrt(climate, climate.epw.dry_bulb_temperature), spec.window)
        case IndexInput(tag="constant", constant=(name, default)):
            return spec.constants.try_find(name).or_else(default).default_with(lambda: _demanded(spec.model, name))
        case _ as unreachable:
            assert_never(unreachable)


def _demanded[T](model: IndexModel, name: str) -> T:
    raise EnergyFault(index_constant=(model.value, name))


def _indexed(climate: Climate, spec: IndexSpec) -> object:
    from ladybug.datacollection import HourlyContinuousCollection

    row = INDEX[spec.model]
    operands = [_operand(climate, slot, spec) for slot in row.inputs]
    return HourlyContinuousCollection.compute_function_aligned(row.kernel.resolve(), operands, row.result.resolve()(), row.unit)


def _floats(row: object) -> tuple[float, ...]:
    return tuple(float(value) for value in getattr(row, "values", row))


def _mapped(climate: Climate, spec: MapSpec) -> MapResult:
    row = MAPS[spec.kind]
    operands = ((list(spec.series),) if not spec.series.is_empty() else ()) + spec.artifacts
    if len(operands) > len(row.slots):
        raise EnergyFault(map_operands=(spec.kind.value, len(operands), len(row.slots)))
    returned = row.kernel.resolve()(**dict(zip(row.slots, operands)), **row.kernel.bound(climate))
    return MapResult(kind=spec.kind, labels=row.bands, values=tuple(_floats(band) for band in returned))


# --- [COMPOSITION] ----------------------------------------------------------------------

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
            parameter_kw="body_parameter",
            results=("physiologic_equivalent_temperature", "core_body_temperature", "skin_temperature"),
        ),
    ),
])

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
