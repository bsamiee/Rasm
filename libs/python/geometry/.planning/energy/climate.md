# [PY_GEOMETRY_ENERGY_CLIMATE]

The climate and thermal-comfort owner — the weather substrate of the energy plane and its point-comfort tier. `Climate.of` is the one polymorphic weather admission: EPW document bytes ARRIVE over the wire and decode through `EPW.from_file_string`, a mounted path opens through `EPW(file_path)`, and a serialized dict reconstructs through `EPW.from_dict` — the modality is the payload shape, never a `source_kind=` knob — and every admitted weather document keys once through the `rasm.runtime.identity` owner over its canonical file-string bytes so re-ingestion of the same climate dedupes in the persistence ledger. The admitted owner exposes four operation families over the one `ladybug` substrate: `series` runs the `DataCollection` algebra (analysis-period windowing, the `'a > 25'` conditional-statement DSL, unit conversion through the 102-type `datatype` registry, and the grain-keyed reduction table) and returns labeled series evidence; `derive` projects the weather document into its owned derivations (`Wea` solar radiation over the EPW's own radiation collections, `DDY` design days by percentile) — the source kind is the constructor, never a re-read of the file; `solar` answers the `Sunpath` geometry family (sun position by hour-of-year, sunrise/sunset, day arcs, analemma polylines) as typed coordinate facts — this page is the energy plane's ONLY solar owner, and the artifacts diagram sun-path furniture stays its own closed-form kernel with no `Sunpath` consumer here; `comfort` runs the weather-drivable point-comfort models (`PMV`/`UTCI`/`PET` through their one-call `from_epw` constructors, SolarCal MRT through `OutdoorSolarCal` feeding the `rad_temperature` slot) with the serializable `Parameter` config carried as data into the constructor's `comfort_parameter` slot, never a threshold branch.

Every operation is a producer on the folder's evidence spine: the fold runs through the `rasm.geometry.graduation` `evidence_run` weave (runtime `boundary` + one span + `@receipted` composed once, never an inline `_TRACER` mint), contributes a typed `ClimateReceipt`, and graduates through the geometry-minted `GeometrySubject` `"thermal-comfort"` member with comfort-percentage evidence measured against caller ceilings. The whole Ladybug band is AGPL-3.0 network copyleft riding the standing companion-lane charter: `ladybug`/`ladybug_comfort` import function-local at the boundary kernels, the companion process exchanges EPW/`DataCollection` evidence across the wire, and nothing links into a distributed host binary. This owner re-implements no EPW parser, no time-series statistic, no unit conversion, no psychrometric or sky model, no PMV/UTCI/PET heat balance, and no MRT map — `ladybug-core` and `ladybug-comfort` own those kernels; it composes them into typed, railed, receipted evidence.

## [01]-[INDEX]

- [01]-[CLIMATE]: the one polymorphic `Climate` weather owner — shape-discriminated EPW admission, the `SeriesSpec` collection algebra, the `Derived` projection family, the `SolarQuery` sun-geometry family, the `ComfortModel` point-comfort tier with SolarCal MRT, the `ClimateReceipt` evidence, and the `thermal-comfort` graduation.

## [02]-[CLIMATE]

- Owner: `Climate` — the admitted weather capsule holding the decoded `EPW` handle, its `Location`, and the weather `ContentKey`; `ClimateField` the closed vocabulary over the EPW climate-field surface (each member IS the `EPW` property name, so field access is one `getattr(epw, field)` derivation, never a per-field accessor family); `SeriesSpec` the one series request — field, `Option` analysis-period window, `Option` conditional statement, `Option` target unit, `Option[Reduce]` reduction — so the collection algebra is request data over one fold; `Reduce` the closed grain×statistic reduction shape whose method name DERIVES as `f"{stat}_{grain}"` from its own fields (the `average_monthly`/`total_daily`/`average_monthly_per_hour` family is one derived spelling, never six arms) with the percentile case carrying its quantile; `Derived` the closed projection union (`wea` by timestep, `ddy` by percentile, `location`, `design_days`); `SolarQuery` the closed sun-geometry union (`position` by hoy, `day` sunrise/sunset by month-day, `arc` day path, `analemma` by hour); `ComfortModel` the closed weather-drivable comfort vocabulary (`PMV`/`UTCI`/`PET`) keying the `COMFORT` table row (module, class, parameter keyword, result properties) so a comfort run is one row-resolved `from_epw` call; `ComfortSpec` the request carrying the model, the `Option` serialized parameter dict (the ladybug `*Parameter.from_dict` config — thresholds and standard selection as data), and the wind/sun inclusion flags the `from_epw` constructors own; `SeriesFact`/`SunFact`/`ComfortFact` the typed egress carriers (values + header facts, positions + vectors, result series + comfort percentages); `ClimateReceipt` the `ReceiptContributor` evidence keyed by the weather `ContentKey`.
- Cases: `ClimateField` rows `DRY_BULB_TEMPERATURE` · `DEW_POINT_TEMPERATURE` · `RELATIVE_HUMIDITY` · `ATMOSPHERIC_STATION_PRESSURE` · `DIRECT_NORMAL_RADIATION` · `DIFFUSE_HORIZONTAL_RADIATION` · `GLOBAL_HORIZONTAL_RADIATION` · `HORIZONTAL_INFRARED_RADIATION_INTENSITY` · `DIRECT_NORMAL_ILLUMINANCE` · `DIFFUSE_HORIZONTAL_ILLUMINANCE` · `WIND_SPEED` · `WIND_DIRECTION` · `TOTAL_SKY_COVER` · `OPAQUE_SKY_COVER` · `VISIBILITY` · `PRECIPITABLE_WATER` — the EPW format's own bounded field set, one member per consumed property, growth one member; `Reduce` cases `mean(Grain)` · `total(Grain)` · `percentile(tuple[Grain, float])` over `Grain` rows `MONTHLY` · `DAILY` · `MONTHLY_PER_HOUR`; `Derived` cases `wea(None)` (the in-memory `Wea(location, direct_normal_radiation, diffuse_horizontal_radiation)` constructor over the EPW's own hourly collections — `EPW.to_wea`/`to_ddy` take a `file_path` and WRITE files, so the in-memory projection is the constructor, and the `Wea` reaches the recipe boundary as a handler input, never a second reader) · `ddy(float percentile)` (`DDY(location, best_available_design_days(percentile))`) · `design_days(None)` (`best_available_design_days`) · `location(None)`; `SolarQuery` cases `position(float)` · `day(tuple[int, int])` · `arc(tuple[int, int])` · `analemma(int)`.
- Entry: `Climate.of` admits `bytes | str | Path | Mapping[str, object]` over one `match` head inside the graduation `evidence_run` weave — bytes decode `EPW.from_file_string(raw.decode())`, a path opens `EPW(str(at))`, a mapping reconstructs `EPW.from_dict(dict(data))` — then keys `ContentIdentity.key("weather", epw.to_file_string().encode())` and returns `RuntimeRail[Climate]`; a malformed document is a typed `BoundaryFault` at the fence, never a raw `ValueError`. `series(spec)` windows through `filter_by_analysis_period(AnalysisPeriod(*window))` when the spec carries one, filters through `filter_by_conditional_statement(statement)` when it carries one, converts through `to_unit(unit)` when it carries one, and reduces through the ONE derived-name dispatch — `getattr(collection, spec.reduce.method)()` for the grain statistics, `percentile_monthly(q)`/`percentile(q)` for the quantile case — returning a `SeriesFact` carrying the values tuple, the `Header` facts (data type, unit, period), and the count. `derive(kind)` runs the projection case (the `Wea` radiation constructor, `DDY(location, best_available_design_days(percentile))`, `best_available_design_days()`, `location`) and returns the typed projection — the `Wea`/`DDY` products serialize through their own `to_file_string` for the recipe-input handoff. `solar(query)` builds one `Sunpath.from_location(self.location)` and answers the case: `position` folds `calculate_sun_from_hoy(hoy)` into a `SunFact` (altitude, azimuth, the `sun_vector` xyz triple, `is_during_day`); `day` folds `calculate_sunrise_sunset(month, day)`; `arc`/`analemma` fold `day_arc3d`/`hourly_analemma_polyline3d` into coordinate-tuple polylines through the primitives' own `to_array` — geometry leaves as arrays, never live `ladybug_geometry` handles. `comfort(spec)` resolves the `COMFORT` row, reconstructs the `Option` parameter through the row's `*Parameter.from_dict`, runs `<Model>.from_epw(epw, include_wind=..., include_sun=..., <parameter_kw>=parameter)` — the one-call weather-to-comfort path that routes the EPW solar fields through SolarCal to the `rad_temperature` MRT input internally — and folds the row's result properties plus `percent_comfortable`/`thermal_condition` distribution into a `ComfortFact`; `mrt()` is the explicit SolarCal arm building `OutdoorSolarCal(location, direct_normal, diffuse_horizontal, horizontal_infrared, surface_temperatures)` from the EPW's own fields for a caller that feeds a non-EPW thermal model.
- Auto: every entry threads `evidence_run(EvidenceScope.ENERGY_CLIMATE, <operation>, dispatch)` — the graduation-owned weave composing the runtime `boundary` fence, one span, and the `@receipted` harvest, so the AGPL import fault, a malformed EPW, an unaligned collection, and a bad conditional statement all convert exactly once; the fact carriers ride the rail bare, and `receipt(operation, discriminant, count, comfortable)` is the one `ClimateReceipt` construction site (the sibling `model`/`district` `receipt()` shape) whose `contribute` stream the weave harvests when the caller threads it — the COMFORT tier passing `Some(percent_comfortable)` so `graduates` stays reachable; collection alignment is a precondition the fold lifts (`are_collections_aligned` before any cross-series op), never a silent ladybug raise; the psychrometric and sky kernels (`humid_ratio_from_db_rh`, `dew_point_from_db_rh`, `ashrae_clear_sky`, `dirint`) reach consumers as `series`-adjacent scalar folds over admitted fields — named functions composed inside the kernels, never re-derived; batch vectorization over sensor grids stays `numpy` inside the pointwise `ladybug_comfort` kernels; the spatial tier is the `comfort_map` arm — the `MAPS` row set addressing `map.mrt.shortwave_mrt_map`/`longwave_mrt_map`, `map.tcp.tcp_total`, and `map.air.air_map` over the simulation matrices the `energy/simulate` recipe products feed back, the MRT rows prepending this owner's admitted `Location` — so the map kernels are rows on this owner, never a per-kernel method family.
- Receipt: `ClimateReceipt` carries the weather `ContentKey`, the `Location` facts (latitude, longitude, elevation, time zone), the leap-year flag, the operation, the field/model discriminant, the value count, and the `Option[float]` percent-comfortable evidence; `contribute` yields one emitted-phase `Receipt.of("rasm.geometry.energy.climate", ("emitted", subject, facts))` triple — the receipts owner's `Evidence` shape; `graduates(key, ceiling)` folds the comfort evidence into the geometry-minted `GeometryHandoff` under `GeometrySubject.THERMAL_COMFORT` — the residual is the discomfort fraction (`1 - percent_comfortable/100`) measured against the caller ceiling, so a comfort run crossing to compute carries real measured evidence, never a count that clears any ceiling.
- Packages: `ladybug-core` (`epw.EPW` — `from_file_string`/`from_dict`/property fields/`best_available_design_days(percentile)`/`to_file_string`; `wea.Wea(location, direct_normal_irradiance, diffuse_horizontal_irradiance)` and `ddy.DDY(location, design_days)` the in-memory projection owners (the `EPW.to_wea`/`to_ddy` file writers stay rejected — they demand a `file_path`); `analysisperiod.AnalysisPeriod`; `datacollection` filter/reduce/unit surface + `are_collections_aligned`; `sunpath.Sunpath.from_location`/`calculate_sun_from_hoy`/`calculate_sunrise_sunset`/`day_arc3d`/`hourly_analemma_polyline3d`; `psychrometrics`/`skymodel` scalar kernels — all function-local at boundary scope, AGPL-3.0 companion posture), `ladybug-comfort` (`collection.pmv.PMV`/`collection.utci.UTCI`/`collection.pet.PET` `from_epw` constructors, `collection.solarcal.OutdoorSolarCal`, `parameter.*Parameter.from_dict` config carriers — same posture; the spatial `map.*` kernels are the `energy/simulate` readback surface), `expression` (`Block`/`Map`/`Option`/`Ok`/`Error`/`tagged_union`/`case`/`tag`), `msgspec` (`Struct` carriers), geometry (`evidence_run`/`EvidenceScope`/`GeometryHandoff`/`GeometrySubject` the graduation spine), runtime (`ContentIdentity`/`ContentKey`, `RuntimeRail`/`BoundaryFault`, `Receipt`/`ReceiptContributor`).
- Growth: a new climate field is one `ClimateField` member; a new reduction grain is one `Grain` member (the method name derives); a new weather projection is one `Derived` case (an IP-units projection over `EPW.convert_to_ip` is the named next case); a new sun-geometry answer is one `SolarQuery` case; a new weather-drivable comfort model is one `COMFORT` row; the adaptive model joins when a consumer carries indoor operative temperatures (it is not weather-drivable — `Adaptive` builds `from_air_and_rad_temp` over model results, so its home is the `energy/simulate` readback, its `AdaptiveParameter` config already serializable through this page's parameter discipline); urban-microclimate EPW morphing stays admission-gated growth per the campaign charter, entering as one more `Derived` case with its own package admission.
- Boundary: no diagram furniture (the artifacts sun-path diagram is its own closed-form kernel; `Sunpath` gains no diagram consumer here), no radiance simulation (the recipe rail owns it), no HBJSON model semantics (`energy/model` owns them), no chart/legend/visualization composition (`ladybug` chart objects are artifacts-plane material), and no re-derived solar vector algebra (`Sunpath` emits `ladybug_geometry` primitives; this owner projects them to arrays). The deleted forms: a per-field accessor family where `ClimateField` + one `getattr` derivation reads any field; a `reduce_monthly`/`reduce_daily` method ladder where the `Reduce` shape derives the method name; a second EPW reader beside the three admission arms; a re-read of the weather file where `derive` projects the admitted document; an inline `trace.get_tracer` mint where `evidence_run` owns the weave; a bare `percent_comfortable` receipt with no graduation residual; a module-top `import ladybug` that loads the AGPL band into every companion start; and a hand-rolled psychrometric/sky/heat-balance formula where the named `ladybug` kernel exists.

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
        # the spatial tier: the ladybug_comfort.map kernels over recipe-produced radiance/thermal
        # matrices (energy/simulate feeds them back) — row-addressed, the MRT rows prepending the
        # admitted Location; per-sensor vectorization stays the kernels' own numpy interior.
        def fold() -> object:
            from importlib import import_module  # noqa: PLC0415 — row-resolved AGPL boundary import

            module, function, wants_location = MAPS[kind]
            kernel = getattr(import_module(module), function)
            return kernel(self.location, *matrices) if wants_location else kernel(*matrices)

        return evidence_run(EvidenceScope.ENERGY_CLIMATE, f"comfort_map.{kind}", fold)

    def receipt(self, operation: str, discriminant: str, count: int, comfortable: Option[float] = Nothing) -> ClimateReceipt:
        # the one ClimateReceipt construction site (the sibling model/district `receipt()` shape):
        # location facts fold from the admitted capsule, the comfort fraction rides Option so only
        # a COMFORT-tier operation carries the thermal-comfort graduation residual.
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
        # the explicit SolarCal arm for a caller feeding a non-EPW thermal model: OutdoorSolarCal
        # over the EPW's own solar fields; from_epw internalizes this path for the COMFORT rows.
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

# spatial comfort-map kernels: (module, function, prepend-location) — the lbt-recipes comfort-map
# recipes drive these over simulation matrices; energy/simulate feeds them back through this row set.
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
