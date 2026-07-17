# [PY_GEOMETRY_ENERGY_CLIMATE]

`Climate` owns the energy plane's weather substrate and its point-comfort tier — the plane's ONLY solar owner. `Climate.of` admits EPW weather by payload shape, and the admitted document answers four families on one `ladybug` substrate: `series` collection algebra, `derive` projections, `solar` sun-geometry, and the `comfort` point-comfort tier with SolarCal MRT. `ladybug-core` and `ladybug-comfort` own every weather, psychrometric, and comfort kernel; this page composes them into typed, railed, receipted evidence.

Ladybug's AGPL-3.0 network-copyleft band rides the standing companion-lane charter: `ladybug`/`ladybug_comfort` import function-local at the boundary kernels — a module-top import loads the AGPL band into every companion start — and evidence crosses the wire, never a link into a distributed host binary. Every operation threads the `rasm.geometry.graduation` `evidence_run` weave, the admitted document keys once over its canonical file-string bytes so re-ingestion dedupes in the persistence ledger, and comfort evidence graduates under `GeometrySubject.THERMAL_COMFORT` against caller ceilings.

## [01]-[INDEX]

- [01]-[CLIMATE]: one polymorphic weather owner — EPW admission, series algebra, derived projections, sun geometry, point comfort — under one `ClimateReceipt`.

## [02]-[CLIMATE]

- Owner: `Climate` holds the decoded `EPW`, its `Location`, and the weather `ContentKey`. `ClimateField` members ARE the `EPW` property names, so field access is one `getattr`; `Reduce.method` derives its ladybug spelling from its own fields, one derived name for the reduction family; `ComfortModel` keys the `COMFORT` row (module, class, parameter keyword, result properties), so a comfort run is one row-resolved `from_epw` call carrying the serializable `*Parameter.from_dict` config as data.
- Entry: `Climate.of` admits `bytes | str | Path | Mapping` by payload shape — modality IS the shape, never a `source_kind=` knob. `Wea`/`DDY` projections serialize through their own `to_file_string`, reaching the recipe boundary as handler inputs, never a second reader. `comfort` routes the EPW solar fields through SolarCal to the `rad_temperature` MRT input internally; `mrt()` is the explicit arm only for a caller feeding a non-EPW thermal model.
- Auto: collection alignment is a precondition the fold lifts — `are_collections_aligned` before any cross-series op, never a silent ladybug raise; psychrometric/sky scalar kernels reach consumers as named composed functions, never re-derived; batch vectorization over sensor grids stays `numpy` inside the pointwise `ladybug_comfort` kernels; `comfort_map` rows consume the simulation matrices `energy/simulate` recipe products feed back.
- Receipt: COMFORT tier passes `Some(percent_comfortable)` so `graduates` stays reachable; the graduation residual is the discomfort fraction `1 - percent_comfortable/100` against the caller ceiling, so a comfort crossing to compute carries measured evidence, never a count clearing any ceiling.
- Packages: `ladybug-core` and `ladybug-comfort` per the fence imports; spatial `map.*` kernels are the `energy/simulate` readback surface.
- Growth: a new field is one `ClimateField` member; a new grain one `Grain` member, method name deriving; a new projection one `Derived` case, IP-units over `EPW.convert_to_ip` the named next; a new sun answer one `SolarQuery` case; a new comfort model one `COMFORT` row. `Adaptive` is not weather-drivable — it builds `from_air_and_rad_temp` over model results, so its home is the `energy/simulate` readback, its `AdaptiveParameter` already serializable through this page's parameter discipline; urban-microclimate EPW morphing enters as one more `Derived` case with its own package admission.
- Boundary: no diagram furniture — artifacts owns the sun-path diagram and `Sunpath` gains no diagram consumer here; no radiance simulation (the recipe rail owns it), no HBJSON model semantics (`energy/model` owns them), no chart/legend composition (artifacts-plane material), and no re-derived solar vector algebra — `Sunpath` emits `ladybug_geometry` primitives and this owner projects them to arrays.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Mapping
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_run
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:  # AGPL band: annotations resolve here; every runtime use is a function-local boundary import
    from ladybug.epw import EPW
    from ladybug.location import Location

# --- [TYPES] ----------------------------------------------------------------------------


class ClimateField(StrEnum):
    DRY_BULB_TEMPERATURE = "dry_bulb_temperature"
    DEW_POINT_TEMPERATURE = "dew_point_temperature"
    RELATIVE_HUMIDITY = "relative_humidity"
    ATMOSPHERIC_STATION_PRESSURE = "atmospheric_station_pressure"
    DIRECT_NORMAL_RADIATION = "direct_normal_radiation"
    DIFFUSE_HORIZONTAL_RADIATION = "diffuse_horizontal_radiation"
    GLOBAL_HORIZONTAL_RADIATION = "global_horizontal_radiation"
    HORIZONTAL_INFRARED_RADIATION_INTENSITY = "horizontal_infrared_radiation_intensity"
    DIRECT_NORMAL_ILLUMINANCE = "direct_normal_illuminance"
    DIFFUSE_HORIZONTAL_ILLUMINANCE = "diffuse_horizontal_illuminance"
    WIND_SPEED = "wind_speed"
    WIND_DIRECTION = "wind_direction"
    TOTAL_SKY_COVER = "total_sky_cover"
    OPAQUE_SKY_COVER = "opaque_sky_cover"
    VISIBILITY = "visibility"
    PRECIPITABLE_WATER = "precipitable_water"


class Grain(StrEnum):
    MONTHLY = "monthly"
    DAILY = "daily"
    MONTHLY_PER_HOUR = "monthly_per_hour"


class ComfortModel(StrEnum):
    PMV = "pmv"
    UTCI = "utci"
    PET = "pet"


class MapKind(StrEnum):
    SHORTWAVE_MRT = "shortwave_mrt"
    LONGWAVE_MRT = "longwave_mrt"
    TCP = "tcp"
    AIR = "air"


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


# --- [MODELS] ---------------------------------------------------------------------------

type Window = tuple[int, int, int, int, int, int]  # (st_month, st_day, st_hour, end_month, end_day, end_hour)


class SeriesSpec(Struct, frozen=True):
    field: ClimateField
    window: Option[Window] = Nothing
    statement: Option[str] = Nothing  # the ladybug conditional DSL, e.g. "a > 25"
    unit: Option[str] = Nothing
    reduce: Option[Reduce] = Nothing


class ComfortSpec(Struct, frozen=True):
    model: ComfortModel
    parameter: Option[Mapping[str, object]] = Nothing  # the serialized *Parameter.from_dict config
    include_wind: bool = True
    include_sun: bool = True


class ComfortRow(Struct, frozen=True, gc=False):
    module: str
    cls: str
    parameter_kw: str
    parameter_cls: str
    results: tuple[str, ...]


class SeriesFact(Struct, frozen=True):
    field: ClimateField
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
                    "comfortable": self.comfortable.default_value(-1.0),
                    "content_key": self.content_key.hex,
                },
            ),
        )

    def graduates(self, key: ContentKey, ceiling: float) -> GeometryHandoff:
        residual = self.comfortable.map(lambda pct: 1.0 - pct / 100.0).default_value(0.0)
        return GeometryHandoff.of(GeometrySubject.THERMAL_COMFORT, key, {"discomfort": residual, "count": float(self.count)}, {"discomfort": ceiling})


# --- [SERVICES] -------------------------------------------------------------------------


class Climate(Struct, frozen=True):
    epw: "EPW"
    location: "Location"
    content_key: ContentKey

    @classmethod
    def of(cls, source: "bytes | str | Path | Mapping[str, object]") -> "RuntimeRail[Self]":
        def admit() -> Self:
            from ladybug.epw import EPW  # noqa: PLC0415 — AGPL boundary import

            match source:
                case bytes() as raw:
                    epw = EPW.from_file_string(raw.decode())
                case Mapping() as data:
                    epw = EPW.from_dict(dict(data))
                case at:
                    epw = EPW(str(at))
            return cls(epw=epw, location=epw.location, content_key=ContentIdentity.key("weather", epw.to_file_string().encode()))

        return evidence_run(EvidenceScope.ENERGY_CLIMATE, "admit", admit)

    def series(self, spec: SeriesSpec) -> "RuntimeRail[SeriesFact]":
        def fold() -> SeriesFact:
            from ladybug.analysisperiod import AnalysisPeriod  # noqa: PLC0415 — AGPL boundary import

            collection = getattr(self.epw, spec.field.value)
            windowed = spec.window.map(lambda w: collection.filter_by_analysis_period(AnalysisPeriod(*w))).default_value(collection)
            filtered = spec.statement.map(windowed.filter_by_conditional_statement).default_value(windowed)
            converted = spec.unit.map(filtered.to_unit).default_value(filtered)
            reduced = spec.reduce.map(
                lambda r: getattr(converted, r.method)(r.percentile[1]) if r.tag == "percentile" else getattr(converted, r.method)()
            ).default_value(converted)
            header = reduced.header
            return SeriesFact(
                field=spec.field,
                data_type=str(header.data_type),
                unit=header.unit,
                period=str(header.analysis_period),
                values=tuple(reduced.values),
            )

        return evidence_run(EvidenceScope.ENERGY_CLIMATE, f"series.{spec.field}", fold)

    def derive(self, kind: Derived) -> "RuntimeRail[object]":
        def fold() -> object:
            from ladybug.ddy import DDY  # noqa: PLC0415 — AGPL boundary import
            from ladybug.wea import Wea  # noqa: PLC0415

            # EPW.to_wea/to_ddy take a file_path and WRITE files; the in-memory projections are the
            # Wea radiation constructor and DDY over the percentile-selected design days.
            match kind:
                case Derived(tag="wea"):
                    return Wea(self.location, self.epw.direct_normal_radiation, self.epw.diffuse_horizontal_radiation)
                case Derived(tag="ddy", ddy=percentile):
                    return DDY(self.location, self.epw.best_available_design_days(percentile))
                case Derived(tag="design_days"):
                    return self.epw.best_available_design_days()
                case Derived(tag="location"):
                    return self.location
                case _ as unreachable:
                    assert_never(unreachable)

        return evidence_run(EvidenceScope.ENERGY_CLIMATE, f"derive.{kind.tag}", fold)

    def solar(self, query: SolarQuery) -> "RuntimeRail[SunFact | DayFact | tuple[tuple[float, float, float], ...]]":
        def fold() -> SunFact | DayFact | tuple[tuple[float, float, float], ...]:
            from ladybug.sunpath import Sunpath  # noqa: PLC0415 — AGPL boundary import

            path = Sunpath.from_location(self.location)
            match query:
                case SolarQuery(tag="position", position=hoy):
                    sun = path.calculate_sun_from_hoy(hoy)
                    return SunFact(
                        altitude=sun.altitude, azimuth=sun.azimuth, vector=tuple(sun.sun_vector.to_array()), is_during_day=sun.is_during_day
                    )
                case SolarQuery(tag="day", day=(month, day)):
                    edges = path.calculate_sunrise_sunset(month, day)
                    return DayFact(sunrise_hoy=edges["sunrise"].hoy, noon_hoy=edges["noon"].hoy, sunset_hoy=edges["sunset"].hoy)
                case SolarQuery(tag="arc", arc=(month, day)):
                    return tuple(path.day_arc3d(month, day).to_polyline(24).to_array())
                case SolarQuery(tag="analemma", analemma=hour):
                    return tuple(path.hourly_analemma_polyline3d()[hour].to_array())
                case _ as unreachable:
                    assert_never(unreachable)

        return evidence_run(EvidenceScope.ENERGY_CLIMATE, f"solar.{query.tag}", fold)

    def comfort(self, spec: ComfortSpec) -> "RuntimeRail[ComfortFact]":
        def fold() -> ComfortFact:
            from importlib import import_module  # noqa: PLC0415 — row-resolved AGPL boundary import

            row = COMFORT[spec.model]
            module = import_module(row.module)
            parameter = spec.parameter.map(lambda data: getattr(module, row.parameter_cls).from_dict(dict(data))).to_optional()
            calc = getattr(module, row.cls).from_epw(
                self.epw, include_wind=spec.include_wind, include_sun=spec.include_sun, **{row.parameter_kw: parameter}
            )
            condition = calc.thermal_condition
            distribution = Block.of_seq(condition.values).fold(lambda acc, value: acc.change(int(value), lambda n: Some(n.default_value(0) + 1)), Map.empty())
            return ComfortFact(
                model=spec.model,
                results=Map.of_seq([(name, tuple(getattr(calc, name).values)) for name in row.results]),
                percent_comfortable=calc.percent_comfortable,
                condition_distribution=distribution,
            )

        return evidence_run(EvidenceScope.ENERGY_CLIMATE, f"comfort.{spec.model}", fold)

    def comfort_map(self, kind: MapKind, *matrices: object) -> "RuntimeRail[object]":
        # ladybug_comfort.map kernels over recipe-produced matrices; the MRT rows prepend the admitted Location.
        def fold() -> object:
            from importlib import import_module  # noqa: PLC0415 — row-resolved AGPL boundary import

            module, function, wants_location = MAPS[kind]
            kernel = getattr(import_module(module), function)
            return kernel(self.location, *matrices) if wants_location else kernel(*matrices)

        return evidence_run(EvidenceScope.ENERGY_CLIMATE, f"comfort_map.{kind}", fold)

    def receipt(self, operation: str, discriminant: str, count: int, comfortable: Option[float] = Nothing) -> ClimateReceipt:
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

    def mrt(self, surface_temperatures: "object") -> "RuntimeRail[object]":
        # OutdoorSolarCal over the EPW's own solar fields; from_epw internalizes this path for the COMFORT rows.
        def fold() -> object:
            from ladybug_comfort.collection.solarcal import OutdoorSolarCal  # noqa: PLC0415 — AGPL boundary import

            return OutdoorSolarCal(
                self.location,
                self.epw.direct_normal_radiation,
                self.epw.diffuse_horizontal_radiation,
                self.epw.horizontal_infrared_radiation_intensity,
                surface_temperatures,
            ).mean_radiant_temperature

        return evidence_run(EvidenceScope.ENERGY_CLIMATE, "mrt", fold)


# --- [COMPOSITION] ----------------------------------------------------------------------

# spatial comfort-map kernels: (module, function, prepend-location) rows over the energy/simulate readback matrices.
MAPS: Final[Map[MapKind, tuple[str, str, bool]]] = Map.of_seq([
    (MapKind.SHORTWAVE_MRT, ("ladybug_comfort.map.mrt", "shortwave_mrt_map", True)),
    (MapKind.LONGWAVE_MRT, ("ladybug_comfort.map.mrt", "longwave_mrt_map", False)),
    (MapKind.TCP, ("ladybug_comfort.map.tcp", "tcp_total", False)),
    (MapKind.AIR, ("ladybug_comfort.map.air", "air_map", False)),
])

COMFORT: Final[Map[ComfortModel, ComfortRow]] = Map.of_seq([
    (
        ComfortModel.PMV,
        ComfortRow(
            module="ladybug_comfort.collection.pmv",
            cls="PMV",
            parameter_kw="pmv_parameter",
            parameter_cls="PMVParameter",
            results=("predicted_mean_vote", "percentage_people_dissatisfied", "standard_effective_temperature"),
        ),
    ),
    (
        ComfortModel.UTCI,
        ComfortRow(
            module="ladybug_comfort.collection.utci",
            cls="UTCI",
            parameter_kw="utci_parameter",
            parameter_cls="UTCIParameter",
            results=("universal_thermal_climate_index",),
        ),
    ),
    (
        ComfortModel.PET,
        ComfortRow(
            module="ladybug_comfort.collection.pet",
            cls="PET",
            parameter_kw="body_parameter",  # PET.from_epw spells its PETParameter slot body_parameter, not pet_parameter
            parameter_cls="PETParameter",
            results=("physiologic_equivalent_temperature", "core_body_temperature", "skin_temperature"),
        ),
    ),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
