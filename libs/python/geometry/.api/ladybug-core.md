# [PY_GEOMETRY_API_LADYBUG_CORE]

`ladybug-core` (imported as `ladybug`, not `ladybug_core`) owns the energy band's climate substrate: weather-file ingestion, the `Location`/`AnalysisPeriod`/`DateTime` time model, the polymorphic `DataCollection` time-series algebra, the unit-type registry, `Sunpath` solar geometry, the `psychrometrics`/`skymodel` kernels, the `SQLiteResult` EnergyPlus-output reader, and the visualization set. Solar geometry and every chart emit `ladybug-geometry` primitives; `ladybug-comfort` and `honeybee-energy` consume its `DataCollection`s and shared unit registry as the climate feed.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ladybug-core`
- package: `ladybug-core` (AGPL-3.0 network-copyleft)
- module: `ladybug` — `ladybug.{epw,wea,stat,ddy,designday,location,analysisperiod,dt}` weather + time model, `ladybug.{datacollection,datacollectionimmutable,header}` time-series algebra, `ladybug.{datatype,sunpath,sql,psychrometrics,skymodel}` registry/solar/output/numeric, `ladybug.{legend,color,graphic,compass,hourlyplot,monthlychart,psychchart,windrose,windprofile,viewsphere,solarenvelope,climatezone}` visualization
- owner: `geometry`
- rail: energy / climate
- consumer: `.planning/energy/climate.md` (weather/series/solar/psychrometrics) + `.planning/energy/simulate.md` (`SQLiteResult` decode)
- depends: `ladybug-geometry` (`Sunpath`/chart geometry resolves to its primitives), `click` (the `ladybug` CLI)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: weather boundary readers and the time model — `EPW` the canonical source, `Wea` its solar projection, `STAT`/`DDY`/`DesignDay` its design conditions

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]     | [CAPABILITY]                                                 |
| :-----: | :---------------------------------------------- | :---------------- | :----------------------------------------------------------- |
|  [01]   | `epw.EPW`                                       | weather file      | `is_leap_year`/`location`, design-day, `to_wea`/`to_ddy`     |
|  [02]   | `wea.Wea`                                       | solar radiation   | direct/diffuse/global; 8 factories, `directional_irradiance` |
|  [03]   | `stat.STAT` / `ddy.DDY` / `designday.DesignDay` | design conditions | `.stat`/`.ddy` design-day and climate-summary readers        |
|  [04]   | `location.Location`                             | site              | lat/long/elevation/time_zone/meridian; `from_idf`/`to_idf`   |
|  [05]   | `analysisperiod.AnalysisPeriod`                 | time window       | start/end m-d-h + timestep; `hoys`/`datetimes`/`moys`        |
|  [06]   | `dt.DateTime`                                   | instant           | `datetime` subclass; `hoy`/`int_hoy`/`moy` addressing        |

[PUBLIC_TYPE_SCOPE]: the polymorphic `DataCollection` family and its `Header` — resolution is the class, the `*Immutable` mirror the mutability, never a parallel statistics ladder

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :-------------------------------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `HourlyContinuousCollection`                        | hourly dense  | gap-free hourly series aligned to an annual `AnalysisPeriod`   |
|  [02]   | `HourlyDiscontinuousCollection`                     | hourly sparse | filtered hourly series carrying explicit `datetimes`           |
|  [03]   | `MonthlyCollection` / `DailyCollection`             | aggregated    | coarser-resolution series with the matching filter axis        |
|  [04]   | `MonthlyPerHourCollection`                          | aggregated    | month-per-hour aggregated series                               |
|  [05]   | `*Immutable` (5 mirrors, `datacollectionimmutable`) | frozen        | read-only mirrors via `to_immutable()`; `to_mutable()` returns |
|  [06]   | `header.Header`                                     | series schema | `(data_type, unit, analysis_period, metadata)`; `to_tuple`     |

[PUBLIC_TYPE_SCOPE]: the unit registry, solar geometry, EnergyPlus output, and the visualization set

| [INDEX] | [SYMBOL]                                                      | [TYPE_FAMILY]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------ | :------------- | :--------------------------------------------------- |
|  [01]   | `datatype.base.DataTypeBase` (+ `TYPESDICT`/`UNITS`/`TYPES`)  | unit type      | 102-type registry; `to_unit`/`to_ip`/`to_si`         |
|  [02]   | `sunpath.Sunpath` / `sunpath.Sun`                             | solar geometry | sun position/rise/set/analemma -> `ladybug_geometry` |
|  [03]   | `sql.SQLiteResult`                                            | E+ output      | `.sql` reader; `data_collections_by_output_name`     |
|  [04]   | `legend.Legend` / `color.Color`/`Colorset`                    | visualization  | legend + colorset composition                        |
|  [05]   | `graphic.GraphicContainer`                                    | visualization  | graphic-frame over collections                       |
|  [06]   | `compass.Compass` / `hourlyplot.HourlyPlot`                   | chart          | compass + hourly plot geometry                       |
|  [07]   | `monthlychart.MonthlyChart` / `psychchart.PsychrometricChart` | chart          | monthly + psychrometric chart                        |
|  [08]   | `windrose.WindRose` / `windprofile.WindProfile`               | chart          | wind rose + wind profile                             |
|  [09]   | `viewsphere.ViewSphere` / `solarenvelope.SolarEnvelope`       | analysis       | view-factor patches, solar-envelope geometry         |
|  [10]   | `climatezone`                                                 | analysis       | ASHRAE climate-zone classification                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: weather ingestion — `EPW` field properties and the `Wea` solar factories

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `EPW(file_path)` / `EPW.from_file_string(s)` / `EPW.from_dict(d)`      | ctor     | parse a weather file / serialized form     |
|  [02]   | `EPW.dry_bulb_temperature` / `.relative_humidity`                      | property | each field as `HourlyContinuousCollection` |
|  [03]   | `.direct_normal_radiation` / `.wind_speed` / (40+)                     | property | the remaining 40+ climate fields           |
|  [04]   | `EPW.location` / `.monthly_ground_temperature`                         | property | site `Location`, ground temps              |
|  [05]   | `EPW.heating_design_condition_dictionary`                              | property | ASHRAE design conditions                   |
|  [06]   | `EPW.to_wea(...)` / `EPW.to_ddy(percentile)`                           | instance | derive `Wea` / `DDY`                       |
|  [07]   | `EPW.approximate_design_day(...)` / `EPW.best_available_design_days()` | instance | design-day derivation                      |
|  [08]   | `EPW.convert_to_ip()` / `convert_to_si()`                              | instance | IP/SI unit flip                            |
|  [09]   | `EPW.save(file_path)` / `to_file_string()`                             | instance | persist the weather file                   |
|  [10]   | `Wea.from_epw_file(epw_file, timestep=1)`                              | factory  | solar radiation from an `EPW`              |
|  [11]   | `from_ashrae_clear_sky(...)`                                           | factory  | ASHRAE clear-sky radiation                 |
|  [12]   | `from_ashrae_revised_clear_sky(...)` / `from_stat_file(...)`           | factory  | solar radiation by named source            |
|  [13]   | `from_zhang_huang_solar(...)`                                          | factory  | Zhang-Huang solar model                    |
|  [14]   | `Wea.directional_irradiance(altitude, azimuth, ...)`                   | instance | directional irradiance                     |
|  [15]   | `Wea.filter_by_analysis_period(p)`                                     | instance | period filtering                           |
|  [16]   | `Wea.estimate_illuminance_components(...)`                             | instance | illuminance components                     |

[ENTRYPOINT_SCOPE]: the one `DataCollection` with its filter, group/aggregate, statistic, and unit axes; the string-DSL statement (`'a > 25'`) binds `a`/`b`/... to positional collections, and the static ops fold across aligned collections

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `coll.filter_by_analysis_period(period)` / `filter_by_hoys(hoys)`    | instance | window series (-> discontinuous)          |
|  [02]   | `coll.filter_by_moys(moys)`                                          | instance | window by minute-of-year                  |
|  [03]   | `coll.filter_by_conditional_statement(statement)`                    | instance | string-DSL conditional filter             |
|  [04]   | `coll.filter_by_pattern(pattern)` / `filter_by_range(...)`           | instance | mask / range filtering                    |
|  [05]   | `coll.group_by_month()` / `group_by_day()`                           | instance | dict of sub-series keyed by period        |
|  [06]   | `coll.group_by_month_per_hour()`                                     | instance | month-per-hour sub-series dict            |
|  [07]   | `coll.average_monthly()` / `total_monthly()`                         | instance | resolution-aware aggregation              |
|  [08]   | `coll.average_monthly_per_hour()`                                    | instance | month-per-hour aggregation                |
|  [09]   | `coll.percentile(pct)` / `percentile_monthly(pct)`                   | instance | resolution-aware percentile               |
|  [10]   | `coll.to_unit(unit)` / `to_ip()` / `to_si()`                         | instance | unit conversion                           |
|  [11]   | `coll.convert_to_unit(unit)`                                         | instance | in-place unit conversion                  |
|  [12]   | `coll.aggregate_by_area(area, area_unit)`                            | instance | area normalization                        |
|  [13]   | `coll.to_immutable()` / `to_mutable()` / `to_discontinuous()`        | instance | mutability transforms                     |
|  [14]   | `coll.get_aligned_collection(value, data_type, unit)`                | instance | alignment transform                       |
|  [15]   | `Collection.histogram(values, bins, key=None)`                       | static   | numpy-free histogram                      |
|  [16]   | `Collection.linspace(...)` / `arange(...)`                           | static   | numpy-free range helpers                  |
|  [17]   | `Collection.compute_function_aligned(funct, data_collections, ...)`  | static   | element-wise scalar fn over aligned colls |
|  [18]   | `Collection.filter_collections_by_statement(collections, statement)` | static   | cross-collection conditional filter       |
|  [19]   | `Collection.pattern_from_collections_and_statement(...)`             | static   | shared boolean pattern                    |

[ENTRYPOINT_SCOPE]: the unit registry, `Sunpath` solar geometry, and the `SQLiteResult` EnergyPlus reader

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `datatype.TYPESDICT[name]` / `datatype.UNITS[type]`          | static   | 102-entry unit-type registry (lookup)                |
|  [02]   | `datatype.TYPES`                                             | static   | full type roster                                     |
|  [03]   | `DataTypeBase.to_unit(values, unit, from_unit)`              | instance | unit conversion owned by the type                    |
|  [04]   | `to_ip(values, from_unit)` / `to_si(...)`                    | instance | IP/SI conversion                                     |
|  [05]   | `is_unit_acceptable(unit)`                                   | instance | unit validation                                      |
|  [06]   | `Sunpath.from_location(location, north_angle=0, ...)`        | factory  | solar calculator from a `Location`                   |
|  [07]   | `Sunpath(latitude, longitude, ...)`                          | ctor     | solar calculator from coords                         |
|  [08]   | `Sunpath.calculate_sun_from_hoy(hoy, is_solar_time=False)`   | instance | `Sun` `altitude`/`azimuth`/`sun_vector` (`Vector3D`) |
|  [09]   | `calculate_sun(month, day, hour)`                            | instance | `Sun` by date                                        |
|  [10]   | `calculate_sunrise_sunset(...)`                              | instance | sunrise / sunset                                     |
|  [11]   | `Sunpath.day_arc3d(...)` / `hourly_analemma_polyline3d(...)` | instance | analemma/day-path -> `ladybug_geometry`              |
|  [12]   | `Sunpath.analemma_suns(...)`                                 | instance | per-hour analemma suns                               |
|  [13]   | `SQLiteResult(file_path)`                                    | ctor     | open an EnergyPlus `.sql`                            |
|  [14]   | `.data_collections_by_output_name(output_name)`              | instance | outputs -> `DataCollection`s                         |
|  [15]   | `.tabular_data_by_name(...)` / `.available_outputs`          | instance | summary tables + output roster                       |

[ENTRYPOINT_SCOPE]: the `psychrometrics` and `skymodel` scalar kernels — radians/SI in, SI out; the named function is the conversion
- [SHAPE]: static module function

| [INDEX] | [SURFACE]                                                                   | [CAPABILITY]                    |
| :-----: | :-------------------------------------------------------------------------- | :------------------------------ |
|  [01]   | `psychrometrics.humid_ratio_from_db_rh(db_temp, rel_humid, b_press=101325)` | humidity ratio from db/rh       |
|  [02]   | `psychrometrics.enthalpy_from_db_hr(...)` / `wet_bulb_from_db_rh(...)`      | enthalpy / wet-bulb             |
|  [03]   | `dew_point_from_db_rh(...)`                                                 | dew-point from db/rh            |
|  [04]   | `psychrometrics.saturated_vapor_pressure(t_kelvin)`                         | saturation vapor pressure       |
|  [05]   | `skymodel.ashrae_clear_sky(altitudes, month, sky_clearness=1)`              | ASHRAE clear-sky model          |
|  [06]   | `ashrae_revised_clear_sky(...)` / `zhang_huang_solar(...)`                  | revised clear-sky / Zhang-Huang |
|  [07]   | `skymodel.dirint(...)` / `disc(...)`                                        | DNI decomposition               |
|  [08]   | `estimate_illuminance_from_irradiance(...)`                                 | irradiance -> illuminance       |
|  [09]   | `calc_sky_temperature(horiz_ir, ...)`                                       | sky temperature                 |
|  [10]   | `skymodel.get_relative_airmass(altitude)` / `get_extra_radiation(doy)`      | airmass / extra-terrestrial rad |
|  [11]   | `clearness_index(...)`                                                      | clearness index                 |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every type round-trips the symmetric `from_dict`/`to_dict`, and `EPW` serializes as a file string; source kind is the constructor — `Wea`'s eight named factories, the `EPW`-derived `DDY`/design-days — never a parallel reader class.
- `DataCollection` is one polymorphic concept across resolution (`Hourly*`/`Monthly`/`Daily`/`MonthlyPerHour`) and mutability (mutable + `*Immutable`); the filter, group/aggregate, statistic, and unit methods ride the one collection, and the static `compute_function_aligned`/`filter_collections_by_statement`/`histogram` fold across aligned collections.
- `datatype.TYPESDICT`/`UNITS` is the registry keyed by type name that `DataTypeBase.to_unit`/`to_ip`/`to_si` converts against, so a `Header.data_type` is a registry entry rather than a per-collection enum; `Sunpath` emits `ladybug-geometry` primitives; `psychrometrics`/`skymodel` stay pure scalar functions the owner vectorizes with `numpy`; and `SQLiteResult.data_collections_by_output_name` is the single EnergyPlus `.sql` decode path.

[STACKING]:
- `ladybug-geometry`(`geometry/.api/ladybug-geometry.md`): `Sun.sun_vector`/`position_3d()` and `Sunpath.day_arc3d`/`hourly_analemma_polyline3d`/`monthly_day_polyline2d` return `Vector3D`/`Point3D`/`Arc3D`/`Polyline3D`/`Polyline2D`; shading and solar-access rails feed the sun vector into `Face3D.contour_fins_*` and `intersection3d` ray casts, so weather, solar, and model geometry share one primitive vocabulary.
- `ladybug-comfort`(`geometry/.api/ladybug-comfort.md`): `Collection.compute_function_aligned(funct, [t, rh], data_type, unit)` and `filter_collections_by_statement` are the aligned-series primitives `ComfortCollection` maps pointwise comfort functions over, and comfort reads its `DataCollection`s (or an `EPW` via `from_epw`) directly.
- `honeybee-energy`(`geometry/.api/honeybee-energy.md`): shares the `datatype.TYPESDICT`/`UNITS` registry every `Header.data_type` resolves against, and writes the EnergyPlus `.sql` that `SQLiteResult` decodes.
- `lbt-recipes`(`runtime/.api/lbt-recipes.md`): recipe execution emits the `eplusout.sql`, and `SQLiteResult(sql_path).data_collections_by_output_name(name)` reads it back into native `DataCollection`s — closing model -> simulation -> labeled series with `available_outputs` as the routing discriminator.
- `dragonfly-core`(`geometry/.api/dragonfly-core.md`): `Model.to_geojson(location=...)`/`from_geojson` takes this package's `Location` to place a metric massing model on the globe.
- `numpy`/`xarray`(`.api/numpy.md`, `.api/xarray.md`): a `DataCollection` maps onto an `xarray.DataArray` — `values` -> `data`, `datetimes` -> the `time` coordinate, `header` fields -> `attrs` — batch reduction runs in `numpy`, and a computed array rebuilds via `HourlyContinuousCollection(header, array.tolist())` while the package's own `histogram`/`percentile`/`linspace` stay the numpy-free fallback.
- `msgspec`/`pydantic`(`.api/msgspec.md`, `.api/pydantic.md`): the symmetric `from_dict`/`to_dict` on every type is the codec boundary, and `EPW.to_file_string`/`from_file_string` graduates the weather file as an artifact-rail string.

[LOCAL_ADMISSION]:
- Consume the AGPL ladybug/honeybee stack out-of-process as a process-boundary companion: invoke it at the edge, graduate `DataCollection`/EPW evidence across the wire, and keep its code out of any distributed proprietary artifact.

[RAIL_LAW]:
- Package: `ladybug-core`
- Owns: EPW/Wea/STAT/DDY weather ingestion and design-day extraction; the `Location`/`AnalysisPeriod`/`DateTime` time model; the polymorphic `DataCollection` family with its filter/group/statistic/unit algebra; the unit-type registry with IP/SI conversion; `Sunpath` solar geometry; the `psychrometrics`/`skymodel` kernels; the `SQLiteResult` EnergyPlus reader; and the legend/color/graphic/compass/chart/windrose visualization set
- Accept: the climate feed for `ladybug-comfort` `ComfortCollection`s and `honeybee-energy` simulations; the `Location` geo-anchor `dragonfly-core` places a massing model with; `SQLiteResult` as the recipe-output decode; the shared unit registry across the band; solar geometry expressed in `ladybug-geometry` primitives
- Reject: wrapper-renames of `EPW`/`Wea`/`DataCollection`; a hand-rolled EPW parser, time-series statistic, unit conversion, psychrometric/sky model, or EnergyPlus SQL reader this package already owns; a per-resolution statistics ladder over the one `DataCollection` family; solar geometry re-derived outside `ladybug-geometry`; importing as `ladybug_core` (the module is `ladybug`)
