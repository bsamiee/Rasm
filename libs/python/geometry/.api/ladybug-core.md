# [PY_GEOMETRY_API_LADYBUG_CORE]

`ladybug-core` (imported as `ladybug`) is the climate-data backbone of the energy companion band: it owns weather-file ingestion (`EPW`, `Wea`, `STAT`, `DDY`/`DesignDay`), the `Location` and `AnalysisPeriod`/`DateTime` time model, the polymorphic `DataCollection` time-series family with its statistical and conditional-filter algebra, the 102-type unit registry (`datatype.TYPESDICT`/`UNITS` with `to_unit`/`to_ip`/`to_si` conversion), `Sunpath` solar geometry, the `psychrometrics` and `skymodel` numeric kernels, the `SQLiteResult` EnergyPlus-output reader, and the visualization set (`Legend`, `Color`, `GraphicContainer`, `Compass`, `HourlyPlot`/`MonthlyChart`/`PsychrometricChart`/`WindRose`/`WindProfile`, `ViewSphere`, `SolarEnvelope`). It depends on `ladybug-geometry` (`Sunpath` and the charts emit its primitives) and is the climate substrate `ladybug-comfort` and `honeybee-energy` consume. The climate owner composes the `EPW`/`Wea` boundary readers, the `DataCollection` algebra, the unit registry, and `SQLiteResult` into one weather-and-results owner; it never re-implements the EPW parser, the time-series statistics, the unit conversions, the psychrometric/sky models, or the EnergyPlus SQL schema this package already owns, and it never re-derives the solar geometry that resolves to `ladybug-geometry` vectors.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ladybug-core`
- package: `ladybug-core`
- import: `import ladybug`
- owner: `geometry`
- rail: energy / climate
- installed: `0.43.18`
- license: AGPL-3.0 (strong copyleft; runs as the out-of-process energy companion rail graduating `DataCollection`/EPW evidence across the wire)
- entry points: `ladybug` console script (CLI: `translate`, `setconfig`); the rail composes the API, not the CLI
- dependency: `ladybug-geometry==1.33.11` (`Sunpath`/chart geometry resolves to its primitives); `click==8.1.7`
- capability: EPW/Wea/STAT/DDY weather ingestion and design-day extraction; `Location`/`AnalysisPeriod`/`DateTime` (hour-of-year addressing); the hourly/monthly/daily/monthly-per-hour `DataCollection` family (mutable + immutable) with analysis-period/conditional/pattern/range filtering, group/aggregate, percentile/histogram statistics, and `to_unit`/`to_ip`/`to_si` conversion; the 102-type unit registry; `Sunpath` solar position and analemma geometry; `psychrometrics` and `skymodel` numeric kernels; `SQLiteResult` EnergyPlus-output reader; and the legend/color/graphic/compass/chart/windrose visualization set

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: weather, location, and time (`ladybug`)
- rail: energy / climate

The boundary readers and the time model. `EPW` is the canonical weather source; `Wea` is its solar-radiation projection; `STAT`/`DDY` carry design conditions. All carry `from_dict`/`to_dict`.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [CAPABILITY]                                                          |
| :-----: | :---------------- | :-------------- | :------------------------------------------------------------------- |
|  [01]   | `epw.EPW`         | weather file    | full EnergyPlus weather file; 40+ climate fields as `DataCollection`s, design-day extraction, `to_wea`/`to_ddy` |
|  [02]   | `wea.Wea`         | solar radiation | direct/diffuse/global irradiance; 8 factory constructors, analysis-period filtering, `directional_irradiance` |
|  [03]   | `stat.STAT` / `ddy.DDY` / `designday.DesignDay` | design conditions | `.stat`/`.ddy` design-day and climate-summary readers |
|  [04]   | `location.Location` | site            | latitude/longitude/elevation/time_zone/meridian; `from_idf`/`to_idf` |
|  [05]   | `analysisperiod.AnalysisPeriod` | time window | start/end month-day-hour + timestep; `hoys`/`datetimes`/`moys`; the filter selector |
|  [06]   | `dt.DateTime`     | instant         | `datetime` subclass with hour-of-year (`hoy`/`int_hoy`) and `moy` addressing |

[PUBLIC_TYPE_SCOPE]: data-collection family and header (`ladybug.datacollection`)
- rail: energy / climate

One polymorphic time-series concept across resolution and mutability; the resolution is the class, not a parallel statistics ladder. Every collection pairs `values` with a `Header` carrying `data_type`/`unit`/`analysis_period`/`metadata`.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]   | [CAPABILITY]                                              |
| :-----: | :---------------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `HourlyContinuousCollection`              | hourly dense    | gap-free hourly series aligned to an annual `AnalysisPeriod` |
|  [02]   | `HourlyDiscontinuousCollection`           | hourly sparse   | filtered hourly series carrying explicit `datetimes`     |
|  [03]   | `MonthlyCollection` / `DailyCollection` / `MonthlyPerHourCollection` | aggregated | coarser-resolution series with the matching filter axis  |
|  [04]   | `*Immutable` (5 mirrors, `datacollectionimmutable`) | frozen | read-only mirrors via `to_immutable()`; `to_mutable()` returns |
|  [05]   | `header.Header`                           | series schema   | `(data_type, unit, analysis_period, metadata)`; `to_csv_strings`/`to_tuple` |

[PUBLIC_TYPE_SCOPE]: unit registry, solar, output, and visualization
- rail: energy / climate

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]   | [CAPABILITY]                                              |
| :-----: | :-------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `datatype.base.DataTypeBase` (+ `TYPESDICT`/`UNITS`/`TYPES`) | unit type | 102-type registry (`Temperature`, `Energy`, `Power`, ...); `to_unit`/`to_ip`/`to_si`, `is_unit_acceptable` |
|  [02]   | `sunpath.Sunpath` / `sunpath.Sun` | solar geometry  | sun position, sunrise/sunset, analemma; emits `ladybug_geometry` vectors/arcs |
|  [03]   | `sql.SQLiteResult`                | E+ output       | EnergyPlus `.sql` reader; `data_collections_by_output_name`, `tabular_data_by_name`, sizing |
|  [04]   | `legend.Legend` / `color.Color`/`Colorset` / `graphic.GraphicContainer` | visualization | legend/colorset/graphic-frame composition over collections |
|  [05]   | `compass.Compass` / `hourlyplot.HourlyPlot` / `monthlychart.MonthlyChart` / `psychchart.PsychrometricChart` / `windrose.WindRose` / `windprofile.WindProfile` | chart | analysis charts emitting `ladybug_geometry` mesh/polyline geometry |
|  [06]   | `viewsphere.ViewSphere` / `solarenvelope.SolarEnvelope` / `climatezone` | analysis | view-factor patches, solar-envelope geometry, ASHRAE climate-zone classification |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: weather ingestion (`EPW`, `Wea`)
- rail: energy / climate

`EPW` exposes every climate field as a lazily-loaded `DataCollection` property; `Wea` is the solar projection with eight named factory constructors (the constructor is the source kind, never a parallel reader class). Both round-trip through `from_dict`/`to_dict` and serialize to their file strings.

| [INDEX] | [SURFACE]                                                              | [CALL_SHAPE]            | [CAPABILITY]                                                |
| :-----: | :-------------------------------------------------------------------- | :---------------------- | :--------------------------------------------------------- |
|  [01]   | `EPW(file_path)` / `EPW.from_file_string(s)` / `EPW.from_dict(d)`     | path / string / dict    | parse a weather file (or its serialized form)              |
|  [02]   | `EPW.dry_bulb_temperature` / `.relative_humidity` / `.direct_normal_radiation` / `.wind_speed` / (40+) | property | each climate field as an `HourlyContinuousCollection`       |
|  [03]   | `EPW.location` / `EPW.heating_design_condition_dictionary` / `.monthly_ground_temperature` | property | site `Location`, ASHRAE design conditions, ground temps    |
|  [04]   | `EPW.to_wea(...)` / `EPW.to_ddy(percentile)` / `EPW.approximate_design_day(...)` / `EPW.best_available_design_days()` | none/percentile | derive `Wea`, `DDY`, and design days from the weather file |
|  [05]   | `EPW.convert_to_ip()` / `convert_to_si()` / `EPW.save(file_path)` / `to_file_string()` | none / path | unit flip; persist the weather file                        |
|  [06]   | `Wea.from_epw_file(epw_file, timestep=1)` / `from_ashrae_clear_sky(...)` / `from_ashrae_revised_clear_sky(...)` / `from_stat_file(...)` / `from_zhang_huang_solar(...)` | source + timestep | construct solar radiation by named source                  |
|  [07]   | `Wea.directional_irradiance(altitude, azimuth, ...)` / `Wea.filter_by_analysis_period(p)` / `Wea.estimate_illuminance_components(...)` | angles / period | directional irradiance, period filtering, illuminance      |

[ENTRYPOINT_SCOPE]: data-collection algebra (`ladybug.datacollection`)
- rail: energy / climate

One polymorphic time-series with a filter axis, a group/aggregate axis, a statistics axis, and a unit axis. The `filter_by_conditional_statement` argument is a string DSL (`'a > 25'`); the static `compute_function_aligned`/`pattern_from_collections_and_statement` operate across aligned collections. The resolution-specific methods (`group_by_month`, `average_monthly`, `percentile_daily`) live on the matching class.

| [INDEX] | [SURFACE]                                                                       | [CALL_SHAPE]              | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------------------ | :------------------------ | :--------------------------------------------------- |
|  [01]   | `coll.filter_by_analysis_period(period)` / `filter_by_hoys(hoys)` / `filter_by_moys(moys)` | `AnalysisPeriod` / hoys | window the series; hourly returns a discontinuous collection |
|  [02]   | `coll.filter_by_conditional_statement(statement)` / `filter_by_pattern(pattern)` / `filter_by_range(...)` | `'a > 25'` / bool mask | DSL/mask/range filtering                              |
|  [03]   | `coll.group_by_month()` / `group_by_day()` / `group_by_month_per_hour()`         | none                      | dict of sub-series keyed by period                   |
|  [04]   | `coll.average_monthly()` / `total_monthly()` / `average_monthly_per_hour()` / `percentile(pct)` / `percentile_monthly(pct)` | none / percentile | resolution-aware aggregation and percentile          |
|  [05]   | `coll.to_unit(unit)` / `to_ip()` / `to_si()` / `convert_to_unit(unit)` / `aggregate_by_area(area, area_unit)` | unit / none | unit conversion and area normalization               |
|  [06]   | `coll.to_immutable()` / `to_mutable()` / `to_discontinuous()` / `get_aligned_collection(value, data_type, unit)` | none / value | mutability + alignment transforms                    |
|  [07]   | `Collection.histogram(values, bins, key=None)` / `linspace(...)` / `arange(...)` (static) | values + bins | numpy-free statistics helpers shared across the family |
|  [08]   | `Collection.compute_function_aligned(funct, data_collections, data_type, unit)` (static) | fn + aligned colls | apply a scalar function element-wise across aligned collections (the comfort-collection compute primitive) |
|  [09]   | `Collection.filter_collections_by_statement(collections, statement)` / `pattern_from_collections_and_statement(...)` (static) | colls + DSL | cross-collection conditional filter and shared boolean pattern |

[ENTRYPOINT_SCOPE]: unit registry, solar, and EnergyPlus output (`datatype`, `sunpath`, `sql`)
- rail: energy / climate

| [INDEX] | [SURFACE]                                                                       | [CALL_SHAPE]              | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------------------ | :------------------------ | :--------------------------------------------------- |
|  [01]   | `datatype.TYPESDICT[name]` / `datatype.UNITS[type]` / `datatype.TYPES`           | type name / type          | the 102-entry unit-type registry (lookup, not a class ladder) |
|  [02]   | `DataTypeBase.to_unit(values, unit, from_unit)` / `to_ip(values, from_unit)` / `to_si(...)` / `is_unit_acceptable(unit)` | values + units | unit conversion + validation owned by the data type  |
|  [03]   | `Sunpath.from_location(location, north_angle=0, ...)` / `Sunpath(latitude, longitude, ...)` | `Location` / coords | construct the solar calculator                       |
|  [04]   | `Sunpath.calculate_sun_from_hoy(hoy, is_solar_time=False)` / `calculate_sun(month, day, hour)` / `calculate_sunrise_sunset(...)` | time | `Sun` with `altitude`/`azimuth`/`sun_vector` (a `Vector3D`) |
|  [05]   | `Sunpath.day_arc3d(...)` / `hourly_analemma_polyline3d(...)` / `analemma_suns(...)` | time            | analemma/day-path geometry as `ladybug_geometry` arcs/polylines |
|  [06]   | `SQLiteResult(file_path)` / `.data_collections_by_output_name(output_name)` / `.tabular_data_by_name(...)` / `.available_outputs` | E+ `.sql` path | read EnergyPlus results back into `DataCollection`s   |

[ENTRYPOINT_SCOPE]: psychrometric and sky numeric kernels (`psychrometrics`, `skymodel`)
- rail: energy / climate

Pure scalar numeric functions (radians/SI in, SI out) that vectorize cleanly; the comfort and radiation owners compose them. The conversion is the named function, never a stateful object.

| [INDEX] | [SURFACE]                                                                       | [CALL_SHAPE]              | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------------------ | :------------------------ | :--------------------------------------------------- |
|  [01]   | `psychrometrics.humid_ratio_from_db_rh(db_temp, rel_humid, b_press=101325)` / `enthalpy_from_db_hr(...)` / `wet_bulb_from_db_rh(...)` / `dew_point_from_db_rh(...)` | scalar climate | psychrometric conversions among db/rh/hr/wb/dpt/enthalpy |
|  [02]   | `psychrometrics.saturated_vapor_pressure(t_kelvin)`                              | temperature               | saturation vapor pressure                            |
|  [03]   | `skymodel.ashrae_clear_sky(altitudes, month, sky_clearness=1)` / `ashrae_revised_clear_sky(...)` / `zhang_huang_solar(...)` | solar inputs | clear-sky and empirical irradiance models            |
|  [04]   | `skymodel.dirint(...)` / `disc(...)` / `estimate_illuminance_from_irradiance(...)` / `calc_sky_temperature(horiz_ir, ...)` | irradiance | DNI decomposition, illuminance, sky temperature      |
|  [05]   | `skymodel.get_relative_airmass(altitude)` / `get_extra_radiation(doy)` / `clearness_index(...)` | solar geometry | airmass, extraterrestrial radiation, clearness index |

## [04]-[INTEGRATION_PATTERNS]

[STACK_TIMESERIES_XARRAY]: `DataCollection` <-> `numpy`/`xarray`
- A `DataCollection` is `values` plus a `Header(data_type, unit, analysis_period, metadata)`. The climate owner maps it onto an `xarray.DataArray` — `values` -> `data`, `datetimes` -> the `time` coordinate, and `header.data_type`/`unit`/`metadata` -> `attrs` — so annual hourly climate becomes a labeled array the numerics/data tier reduces with `numpy`/`xarray`. The reverse build wraps a computed `numpy` array via `HourlyContinuousCollection(header, values.tolist())`. The package's own `Collection.histogram`/`percentile`/`linspace`/`arange` are numpy-free fallbacks; the owner prefers `numpy` for batch and keeps `DataCollection` as the labeled boundary form.

[STACK_CONDITIONAL_FILTER_DSL]: `filter_by_conditional_statement` <-> aligned-collection algebra
- `filter_by_conditional_statement('a > 25')` and the static `filter_collections_by_statement(collections, 'a > 25 and b < 80')` are a string-DSL filter where `a`/`b`/... bind to the positional collections. `compute_function_aligned(funct, [t, rh], data_type, unit)` applies a scalar function element-wise across aligned series — this is exactly the primitive `ladybug-comfort` `ComfortCollection`s build on to map pointwise comfort functions over time. The owner aligns collections with `are_collections_aligned`/`get_aligned_collection` before any cross-series op, lifting misalignment to a typed precondition.

[STACK_GEOMETRY_SOLAR]: `Sunpath` <-> `ladybug-geometry`
- `Sunpath` is a downstream consumer of the geometry substrate: `Sun.sun_vector`/`sun_vector_reversed` are `Vector3D`, `Sun.position_3d()` is a `Point3D`, and `Sunpath.day_arc3d`/`hourly_analemma_polyline3d`/`monthly_day_polyline2d` return `Arc3D`/`Polyline3D`/`Polyline2D`. Shading-design and solar-access rails compose the sun vector directly into `Face3D.contour_fins_*` orientation and into `intersection3d` ray casts, so weather, solar geometry, and model geometry share one primitive vocabulary.

[STACK_SIMULATION_OUTPUT]: `SQLiteResult` <-> the recipe/honeybee-energy output boundary
- `SQLiteResult(sql_path).data_collections_by_output_name('Zone Mean Air Temperature')` reads the EnergyPlus `.sql` an `lbt-recipes` energy recipe (or a `honeybee-energy` simulation) produced and returns native `DataCollection`s — closing the loop from model -> simulation -> labeled time-series. `available_outputs`/`available_outputs_info` is the discriminator the owner reads to route which outputs to pull; `tabular_data_by_name`/`component_sizes_by_type` pull the summary tables. This is the only EnergyPlus-results decode path; the owner never re-parses the SQL schema.

[STACK_UNIT_REGISTRY]: `datatype` <-> the energy-band unit system
- `datatype.TYPESDICT` (102 entries) and `UNITS` are the shared unit registry honeybee-energy constructions, `ladybug-comfort` outputs, and the `Header` of every `DataCollection` resolve against; `DataTypeBase.to_unit`/`to_ip`/`to_si` own the conversions. The climate owner treats the registry as a lookup table keyed by type name — a `Header`'s `data_type` is a registry entry, not a hand-rolled enum — so unit-aware conversion and IP/SI flips are one registry concern across the whole band.

[STACK_BOUNDARY_MSGSPEC]: `from_dict`/`to_dict` <-> the substrate codecs
- `EPW`, `Wea`, `Location`, `AnalysisPeriod`, `Header`, `DateTime`, and every `DataCollection` carry the symmetric `from_dict`/`to_dict`, so the climate boundary is one `msgspec.Struct`/`pydantic` model family — the same discriminated-dict pattern `ladybug-geometry` uses for primitives and honeybee uses for the model. EPW additionally graduates as a file string (`to_file_string`/`from_file_string`) for the weather-file artifact rail.

## [05]-[IMPLEMENTATION_LAW]

[ENERGY_CLIMATE]:
- import: `import ladybug` and the submodules at boundary scope only; module-level import is banned by the manifest import policy.
- weather axis: `EPW` is the canonical weather source exposing every climate field as a lazily-loaded `HourlyContinuousCollection`; `Wea` is its solar projection constructed by named factory (`from_epw_file`/`from_ashrae_clear_sky`/...); `STAT`/`DDY`/`DesignDay` carry design conditions. The source kind is the constructor, never a parallel reader class; the owner derives `Wea`/`DDY`/design-days from the `EPW` rather than re-reading the file.
- time-series axis: the `DataCollection` family is one polymorphic concept across resolution (`Hourly*`/`Monthly`/`Daily`/`MonthlyPerHour`) and mutability (mutable + `*Immutable`). The filter axis (`filter_by_analysis_period`/`_conditional_statement`/`_pattern`/`_range`), group/aggregate axis (`group_by_*`/`average_*`/`total_*`/`percentile*`), and unit axis (`to_unit`/`to_ip`/`to_si`) are methods on the one collection, never a parallel statistics ladder. The static `compute_function_aligned`/`filter_collections_by_statement`/`histogram` operate across aligned collections.
- unit axis: `datatype.TYPESDICT`/`UNITS` is a 102-entry registry keyed by type name; `DataTypeBase.to_unit`/`to_ip`/`to_si` owns conversion. A `Header.data_type` is a registry entry, so unit awareness is one lookup concern shared with honeybee-energy and ladybug-comfort, not a per-collection enum.
- solar axis: `Sunpath` emits `ladybug_geometry` primitives (`Sun.sun_vector` is a `Vector3D`); the package is a consumer of the geometry substrate, never a re-implementer of vector algebra. Solar-time vs clock-time and DST are flags on the calculate methods.
- output axis: `SQLiteResult.data_collections_by_output_name` is the single EnergyPlus `.sql` decode path returning native `DataCollection`s; `available_outputs` is the routing discriminator. The owner never re-parses the SQL schema.
- numeric axis: `psychrometrics` and `skymodel` are pure scalar functions (the conversion is the named function); the owner vectorizes them with `numpy` for batch and composes them into the comfort and radiation rails.
- evidence: each weather read captures the `Location`, `AnalysisPeriod`, leap-year flag, and field count; each collection op captures the `data_type`/`unit`, value count, and (for filters) the resulting period; each `SQLiteResult` read captures the output name, reporting frequency, and run period as a climate receipt.
- boundary: ladybug-core owns weather ingestion, the time-series algebra, the unit registry, solar geometry, the psychrometric/sky kernels, the EnergyPlus-output reader, and the visualization set. The geometry primitives it emits are owned by `ladybug-geometry`; comfort models that consume its collections are `ladybug-comfort`; energy-model construction and simulation are `honeybee-energy`; recipe execution producing the `.sql` it reads is `lbt-recipes`; the urban geo-anchor consumer is `dragonfly-core`, whose `Model.to_geojson(location=...)`/`from_geojson` takes this package's `Location` to place a metric massing model on the globe (the reciprocal seam in `dragonfly-core.md`); batch numeric reduction routes to `numpy`/`xarray`; persistence of the labeled series to the data tier.

## [06]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ladybug-core`
- Owns: EPW/Wea/STAT/DDY weather ingestion and design-day extraction; `Location`/`AnalysisPeriod`/`DateTime` time model; the polymorphic `DataCollection` family with filter/group/statistic/unit algebra; the 102-type unit registry with IP/SI conversion; `Sunpath` solar geometry; the `psychrometrics`/`skymodel` numeric kernels; the `SQLiteResult` EnergyPlus-output reader; and the legend/color/graphic/compass/chart/windrose visualization set
- Accept: the climate substrate feeding `ladybug-comfort` `ComfortCollection`s and `honeybee-energy` simulations; the `Location` geo-anchor `dragonfly-core` `Model.to_geojson`/`from_geojson` places a massing model with; the EnergyPlus-results boundary `SQLiteResult` decodes from recipe output; the unit registry shared across the band; the solar geometry expressed in `ladybug-geometry` primitives
- Reject: wrapper-renames of `EPW`/`Wea`/`DataCollection`; a hand-rolled EPW parser, time-series statistic, unit conversion, psychrometric/sky model, or EnergyPlus SQL reader where this package already owns it; a per-resolution statistics ladder over the one `DataCollection` family; a re-derivation of solar geometry outside `ladybug-geometry`; identity minting the runtime owns
- Note: AGPL-3.0 copyleft — the climate rail runs out-of-process and graduates `DataCollection`/EPW evidence across the wire, never linked into a distributed host binary

[CAPTURE_GAP]:
- members: verified by `assay api` reflection against the installed `ladybug-core==0.43.18` distribution. Every documented type, factory, property, and static resolves with the signatures shown — no phantom. Confirmed shapes: `EPW(file_path)` / `EPW.from_file_string`; `Wea.from_epw_file(epw_file, timestep=1)`; `HourlyContinuousCollection.filter_by_conditional_statement(statement)` (string DSL) and the static `compute_function_aligned(funct, data_collections, data_type, unit)`; `Collection.histogram(values, bins, key=None)`; `Sunpath.calculate_sun_from_hoy(hoy, is_solar_time=False)` with `Sun.sun_vector -> ladybug_geometry...Vector3D`; `SQLiteResult(file_path).data_collections_by_output_name(output_name)`; `datatype.TYPESDICT` confirmed 102 entries (`TYPES` 102, `UNITS` 27 categories). License confirmed AGPL-3.0; `ladybug-geometry==1.33.11` confirmed as the runtime dependency.
