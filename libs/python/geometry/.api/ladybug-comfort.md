# [PY_GEOMETRY_API_LADYBUG_COMFORT]

`ladybug-comfort` is the thermal-comfort calculation layer stacked directly on `ladybug-core`: it is three coordinated tiers — pointwise numeric model functions (`pmv`, `utci`, `pet`, `adaptive`, `solarcal`, plus the simple indices `at`/`di`/`hi`/`humidex`/`wbgt`/`wc`/`ts`/`degreetime`/`clo`), `DataCollection`-wrapping `ComfortCollection` objects (`PMV`, `UTCI`, `Adaptive`, `PET`, and the SolarCal MRT family) that map those functions over `ladybug-core` time-series, and serializable `Parameter` config objects (`PMVParameter`, `UTCIParameter`, `AdaptiveParameter`, `PETParameter`, `SolarCalParameter`) that carry the comfort thresholds and standard selection. It additionally owns the psychrometric-chart comfort polygons (`chart.polygonpmv`/`polygonutci`/`adaptive`) and the spatial comfort `map` kernels (`map.mrt`/`tcp`/`air`/`utci`/`irr`) that the `lbt-recipes` comfort-map recipes drive. The comfort owner composes the `ComfortCollection` constructors (especially `from_epw`), the `Parameter` config objects, and the `map` kernels into one comfort owner; it never re-implements the PMV/UTCI/PET/adaptive/SolarCal heat-balance math, the comfort-threshold logic, or the spatial MRT mapping this package already owns, and it consumes `ladybug-core` `DataCollection`s rather than re-deriving the time-series algebra.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ladybug-comfort`
- package: `ladybug-comfort`
- import: `import ladybug_comfort`
- owner: `geometry`
- rail: energy / comfort
- consumer: `.planning/energy/climate.md` (PMV/UTCI/PET rows, SolarCal MRT, the `MAPS` comfort-map rows)
- installed: `0.18.61`
- license: AGPL-3.0 (strong copyleft; runs as the out-of-process energy companion rail graduating comfort `DataCollection`/map evidence across the wire)
- entry points: `ladybug-comfort` console script (CLI command groups `datacollection`/`epw`/`map`/`mtx`/`sql`); the rail composes the API
- dependency: `ladybug-core==0.43.18` (consumes its `DataCollection`/`EPW`/`datatype`); `numpy<2.0.0` under the `mapping` extra (the spatial `map` kernels)
- capability: pointwise PMV/SET/PPD, UTCI, PET, ASHRAE-55 and EN-15251 adaptive (with EN-16798 cooling effect), and indoor/outdoor SolarCal MRT model functions; the simple heat indices (apparent temperature, discomfort/heat index, humidex, WBGT, wind chill, thermal sensation, degree-time); `DataCollection`-based `ComfortCollection` calculators with `from_epw` constructors exposing comfort and `thermal_condition` results as collections; serializable `Parameter` config objects with comfort thresholds and standard selection; psychrometric-chart comfort polygons; and the spatial MRT/TCP/air comfort-map kernels

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: comfort collections (`ladybug_comfort.collection`)
- rail: energy / comfort

All subclass `collection.base.ComfortCollection`: they take aligned `ladybug-core` `DataCollection`s (or read an `EPW` via `from_epw`) and expose every comfort result as a matching collection. The comfort model is the class; the result is a property, never a parallel method family.

| [INDEX] | [SYMBOL]                                                                                                  | [TYPE_FAMILY] | [CAPABILITY]                                                                                                                          |
| :-----: | :-------------------------------------------------------------------------------------------------------- | :------------ | :------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `collection.pmv.PMV`                                                                                      | Fanger PMV    | `predicted_mean_vote`, `percentage_people_dissatisfied`, `standard_effective_temperature`, `thermal_condition`, `percent_comfortable` |
|  [02]   | `collection.utci.UTCI`                                                                                    | UTCI          | `universal_thermal_climate_index`, the 2/5/7/9/11-point `thermal_condition_*`, heat/cold-stress percentages                           |
|  [03]   | `collection.adaptive.Adaptive`                                                                            | adaptive      | `neutral_temperature`, `degrees_from_neutral`, `prevailing_outdoor_temperature`, `thermal_condition`                                  |
|  [04]   | `collection.pet.PET`                                                                                      | PET (MEMI)    | `physiologic_equivalent_temperature`, `core_body_temperature`, `skin_temperature`, `pet_category`                                     |
|  [05]   | `collection.solarcal.OutdoorSolarCal` / `IndoorSolarCal` / `HorizontalSolarCal` / `HorizontalRefSolarCal` | SolarCal MRT  | `mean_radiant_temperature`, `mrt_delta`, short/longwave ERF; the MRT source for PMV/UTCI/PET                                          |

[PUBLIC_TYPE_SCOPE]: comfort parameters and visualization (`parameter`, `chart`)
- rail: energy / comfort

`Parameter` objects are the config carriers: serializable thresholds and standard selectors that parameterize a `ComfortCollection`. They carry `from_dict`/`to_dict`/`from_string` and `is_comfortable`/`thermal_condition` predicates.

| [INDEX] | [SYMBOL]                                                                                         | [TYPE_FAMILY]       | [CAPABILITY]                                                                                 |
| :-----: | :----------------------------------------------------------------------------------------------- | :------------------ | :------------------------------------------------------------------------------------------- |
|  [01]   | `parameter.pmv.PMVParameter`                                                                     | PMV config          | `ppd_comfort_thresh`, `humid_ratio` bounds, `still_air_threshold`; `discomfort_reason`       |
|  [02]   | `parameter.utci.UTCIParameter`                                                                   | UTCI config         | the cold/heat stress thresholds; `original_utci_category`, the n-point `thermal_condition_*` |
|  [03]   | `parameter.adaptive.AdaptiveParameter`                                                           | adaptive config     | `ashrae_or_en` standard, `conditioning`, `neutral_offset`, air-speed method                  |
|  [04]   | `parameter.pet.PETParameter` / `parameter.solarcal.SolarCalParameter`                            | PET/SolarCal config | body parameters, posture, reflectance/exposure config                                        |
|  [05]   | `chart.polygonpmv.PolygonPMV` / `chart.polygonutci.PolygonUTCI` / `chart.adaptive.AdaptiveChart` | chart               | comfort polygons over a `ladybug.psychchart.PsychrometricChart`                              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pointwise comfort model functions
- rail: energy / comfort

Pure scalar functions (SI/radians in, comfort scalar out) — the heat-balance kernels the collections vectorize. The model is the named function; the standard variant is an argument or a sibling function, never a class. These vectorize cleanly under `numpy` for the spatial map.

| [INDEX] | [SURFACE]                                                                                                                                                                                                                         | [CALL_SHAPE]           | [CAPABILITY]                                                                                   |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `pmv.predicted_mean_vote(ta, tr, vel, rh, met, clo, wme=0, still_air_threshold=0.1)`                                                                                                                                              | scalar climate + body  | Fanger PMV + PPD + SET dict (the full pierce SET model)                                        |
|  [02]   | `pmv.fanger_pmv(...)` / `pmv.pierce_set(...)` / `pmv.ppd_from_pmv(pmv)` / `pmv.pmv_from_ppd(ppd)`                                                                                                                                 | scalars                | the underlying Fanger/Pierce models and PMV<->PPD map                                          |
|  [03]   | `utci.universal_thermal_climate_index(ta, tr, vel, rh)`                                                                                                                                                                           | scalar climate         | UTCI in degrees C                                                                              |
|  [04]   | `pet.physiologic_equivalent_temperature(ta, tr, vel, rh, met, clo, ...)` / `pet.pet_category(pet)`                                                                                                                                | scalar climate + body  | PET via the MEMI heat-balance model                                                            |
|  [05]   | `adaptive.adaptive_comfort_ashrae55(t_prevail, to)` / `adaptive_comfort_en15251(t_prevail, to)` / `adaptive_comfort_conditioned(t_prevail, to, conditioning, model)` / `cooling_effect_ashrae55`/`_en15251`/`_en16798`            | prevailing + operative | the adaptive comfort models + air-speed cooling effect (EN-16798 via `cooling_effect_en16798`) |
|  [06]   | `solarcal.outdoor_sky_heat_exch(...)` / `indoor_sky_heat_exch(...)` / `erf_from_body_solar_flux(...)` / `mrt_delta_from_erf(...)`                                                                                                 | solar flux             | SolarCal MRT-delta heat exchange                                                               |
|  [07]   | `at.apparent_temperature(ta, rh, ws)` / `di.discomfort_index(ta, rh)` / `hi.heat_index(ta, rh)` / `humidex.humidex(ta, tdp)` / `wbgt.wet_bulb_globe_temperature(...)` / `wc.windchill_temp(ta, ws)` / `ts.thermal_sensation(...)` | scalar climate         | the simple heat/cold indices + warning categories                                              |
|  [08]   | `degreetime.heating_degree_time(...)` / `cooling_degree_time(...)` / `clo.schiavon_clo(adapt_temp, max_clo=1, min_clo=0.46, ...)`                                                                                                 | temperature            | degree-time accumulation; adaptive-temp clothing estimate                                      |

[ENTRYPOINT_SCOPE]: comfort-collection construction and results (`collection`)
- rail: energy / comfort

A `ComfortCollection` takes aligned `ladybug-core` collections (or an `EPW` via `from_epw`) plus an optional `Parameter`, and exposes every result as a `DataCollection`. `from_epw(epw, include_wind=True, include_sun=True, ...)` is the one-call weather-to-comfort path.

| [INDEX] | [SURFACE]                                                                                                                                                                             | [CALL_SHAPE]                       | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :--------------------------------- | :--------------------------------------------------- |
|  [01]   | `PMV(air_temperature, rel_humidity, rad_temperature=None, air_speed=None, met_rate=None, clo_value=None, external_work=None, comfort_parameter=None)`                                 | `DataCollection`s + `PMVParameter` | build the PMV calculator over time-series            |
|  [02]   | `PMV.from_epw(epw, include_wind=True, include_sun=True, met_rate=None, clo_value=None, external_work=None, pmv_parameter=None)`                                                       | `EPW` + flags                      | weather-to-PMV in one call (sun -> MRT via SolarCal) |
|  [03]   | `UTCI(air_temperature, rel_humidity, rad_temperature=None, wind_speed=None, comfort_parameter=None)` / `UTCI.from_epw(epw, include_wind=True, include_sun=True, utci_parameter=None)` | `DataCollection`s / `EPW`          | UTCI calculator                                      |
|  [04]   | `Adaptive(...)` / `Adaptive.from_air_and_rad_temp(...)` / `PET(...)` / `PET.from_epw(...)`                                                                                            | `DataCollection`s / `EPW`          | adaptive and PET calculators                         |
|  [05]   | `pmv.PMV.predicted_mean_vote` / `.percentage_people_dissatisfied` / `.thermal_condition` / `.is_comfortable` / `.percent_comfortable`                                                 | property                           | results as `DataCollection`s (one per metric)        |
|  [06]   | `utci.UTCI.universal_thermal_climate_index` / `.thermal_condition_nine_point` / `.percent_strong_heat_stress`                                                                         | property                           | UTCI index + categorical stress collections          |
|  [07]   | `OutdoorSolarCal(location, direct_normal_solar, diffuse_horizontal_solar, horizontal_infrared, surface_temperatures, ...)` / `.mean_radiant_temperature`                              | `Location` + solar colls           | SolarCal MRT feeding PMV/UTCI/PET `rad_temperature`  |

[ENTRYPOINT_SCOPE]: comfort parameters and spatial maps (`parameter`, `map`)
- rail: energy / comfort

`Parameter` objects round-trip through `from_dict`/`to_dict`/`from_string`. The `map.*` kernels operate over honeybee-radiance MRT/irradiance matrices for the per-sensor comfort map the recipes drive.

| [INDEX] | [SURFACE]                                                                                                                                                              | [CALL_SHAPE]        | [CAPABILITY]                                                                         |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------ | :----------------------------------------------------------------------------------- |
|  [01]   | `PMVParameter(ppd_comfort_thresh=None, humid_ratio_upper=None, humid_ratio_lower=None, still_air_threshold=None)` / `.from_dict(d)` / `.to_dict()` / `.from_string(s)` | thresholds          | PMV comfort config (unset slots resolve to the 10% PPD / 0.1 m/s still-air defaults) |
|  [02]   | `UTCIParameter(...)` / `AdaptiveParameter(ashrae_or_en=None, neutral_offset=None, conditioning=None, ...)` / `PETParameter(...)` / `SolarCalParameter(...)`            | thresholds/standard | the per-model config carriers                                                        |
|  [03]   | `<Parameter>.is_comfortable(value, ...)` / `.thermal_condition(value, ...)`                                                                                            | comfort value       | comfort predicate + categorical condition from config                                |
|  [04]   | `map.mrt.shortwave_mrt_map(location, longwave_data, sun_up_hours, indirect_ill, direct_ill=None, ref_ill=None, ...)` / `longwave_mrt_map(...)`                         | radiance matrices   | per-sensor MRT map from radiance irradiance                                          |
|  [05]   | `map.tcp.tcp_total(comfort_result, ...)` / `tcp_model_schedules(...)` / `map.air.air_map(...)`                                                                         | comfort matrix      | thermal-comfort-percent map and air-temperature map                                  |

## [04]-[INTEGRATION_PATTERNS]

[STACK_COMFORT_OVER_CORE]: `ComfortCollection` <-> `ladybug-core` `DataCollection`
- A `ComfortCollection` is the concrete realization of `ladybug-core`'s `compute_function_aligned` pattern: it takes aligned `DataCollection`s (`air_temperature`, `rel_humidity`, `rad_temperature`, `air_speed`, ...), maps the pointwise model function element-wise across them, and exposes every result as a `DataCollection` with the right `Header` (e.g. `PMV.predicted_mean_vote`, `UTCI.thermal_condition`). The comfort owner builds these from the climate owner's collections directly; `from_epw(epw, include_sun=True)` is the one-call path that internally routes the EPW's solar fields through SolarCal to the `rad_temperature` MRT input. The owner aligns the inputs (`are_collections_aligned`) before construction and lifts misalignment to a typed precondition.

[STACK_PARAMETER_CONFIG]: `Parameter` <-> `msgspec`/`pydantic` config boundary
- `PMVParameter`/`UTCIParameter`/`AdaptiveParameter`/`PETParameter`/`SolarCalParameter` carry `from_dict`/`to_dict`/`from_string`, so the comfort owner maps them onto a `msgspec.Struct`/`pydantic` config model — the comfort thresholds, the `ashrae_or_en` standard switch, and the body parameters are the knobs that parameterize a calculation, serialized once at the boundary and threaded into the collection constructor's `comfort_parameter=` slot. This is the same discriminated-config pattern the rest of the band uses; the parameter is data, not a code branch.

[STACK_POINTWISE_NUMPY]: pointwise functions <-> `numpy` vectorization
- `pmv.predicted_mean_vote`, `utci.universal_thermal_climate_index`, `pet.physiologic_equivalent_temperature`, and the adaptive/index functions are pure scalar kernels; for the spatial comfort map (tens of thousands of sensors x 8760 hours) the owner vectorizes them with `numpy` rather than looping the Python `ComfortCollection`. The `ComfortCollection` is the labeled single-point/time-series boundary form; the `numpy`-vectorized pointwise call is the batch form, and both compute the identical model.

[STACK_SPATIAL_MAP]: `map.*` <-> honeybee-radiance matrices and the recipe boundary
- `map.mrt.shortwave_mrt_map`/`longwave_mrt_map` consume honeybee-radiance irradiance/illuminance matrices (`indirect_ill`/`direct_ill`/`ref_ill`) and the `Location` to produce per-sensor MRT; `map.tcp.tcp_total` folds a comfort result into a thermal-comfort-percent map; `map.air.air_map` produces the air-temperature field. These are exactly the kernels the `lbt-recipes` comfort-map recipes (`pmv_comfort_map`, `utci_comfort_map`, `adaptive_comfort_map`) drive — the recipe runs the radiance simulation, then calls these map functions with the simulation matrices and the serialized `Parameter`. The comfort owner exposes the map kernels; the recipe owner orchestrates the simulation-then-map DAG.

[STACK_THERMAL_CONDITION_DATATYPE]: `thermal_condition` <-> `ladybug.datatype`
- Every collection's categorical output (`thermal_condition`, `thermal_condition_nine_point`, `pet_category`) is a `DataCollection` whose `Header.data_type` is a `ladybug.datatype.thermalcondition`-family registry entry, so the categorical comfort result flows through the same unit-registry and visualization rail as any climate field — the comfort polygons (`chart.PolygonPMV`/`PolygonUTCI`) render directly over a `ladybug.psychchart.PsychrometricChart`, sharing the climate owner's chart geometry.

## [05]-[IMPLEMENTATION_LAW]

[ENERGY_COMFORT]:
- import: `import ladybug_comfort` and the submodules at boundary scope only; module-level import is banned by the manifest import policy.
- model-function axis: `pmv`/`utci`/`pet`/`adaptive`/`solarcal` and the simple indices (`at`/`di`/`hi`/`humidex`/`wbgt`/`wc`/`ts`/`degreetime`/`clo`) are pure scalar functions; the model is the named function and the standard variant is an argument or sibling function (`adaptive_comfort_ashrae55`/`_en15251`/`_conditioned`, with EN-16798 carried by `cooling_effect_en16798`), never a class. The owner vectorizes them with `numpy` for the spatial map.
- collection axis: a `ComfortCollection` (`PMV`/`UTCI`/`Adaptive`/`PET`/SolarCal) takes aligned `ladybug-core` `DataCollection`s (or an `EPW` via `from_epw`) plus an optional `Parameter` and exposes every comfort result as a `DataCollection` property; the comfort model is the class, the result is a property, never a parallel result-method family. SolarCal MRT feeds the `rad_temperature` of the thermal models.
- parameter axis: `PMVParameter`/`UTCIParameter`/`AdaptiveParameter`/`PETParameter`/`SolarCalParameter` carry `from_dict`/`to_dict`/`from_string` and the `is_comfortable`/`thermal_condition` predicates; they are the serializable config threaded into the collection's `comfort_parameter=` slot — the threshold/standard is data, not a code branch.
- map axis: `map.mrt`/`tcp`/`air`/`utci`/`irr` operate over honeybee-radiance matrices for the per-sensor comfort map; the owner exposes the kernels, the recipe owner orchestrates the simulation-then-map DAG. The categorical output `data_type` is a `ladybug.datatype` registry entry shared with the climate visualization rail.
- evidence: each comfort calculation captures the model, the calc length, the `Parameter` config, the comfort percentages (`percent_comfortable`/`percentage_people_dissatisfied`/stress bands), and the `thermal_condition` distribution as a comfort receipt.
- boundary: ladybug-comfort owns the comfort model functions, the `ComfortCollection` calculators, the `Parameter` config objects, the comfort-chart polygons, and the spatial MRT/TCP/air map kernels. The `DataCollection`/`EPW`/`datatype` it consumes are owned by `ladybug-core`; the radiance matrices the map consumes are produced by honeybee-radiance via `lbt-recipes`; recipe orchestration of the comfort-map DAG is `lbt-recipes`; batch numeric vectorization routes to `numpy`.

## [06]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ladybug-comfort`
- Owns: pointwise PMV/SET/PPD, UTCI, PET, ASHRAE-55 and EN-15251 adaptive (with EN-16798 cooling effect), and SolarCal MRT model functions plus the simple heat/cold indices; the `ComfortCollection` calculators with `from_epw` constructors exposing comfort and `thermal_condition` results as `DataCollection`s; the serializable `Parameter` config objects; the psychrometric-chart comfort polygons; and the spatial MRT/TCP/air comfort-map kernels
- Accept: comfort calculation stacked on `ladybug-core` `DataCollection`s/`EPW`; the `Parameter` config serialized into recipe inputs; the `map` kernels driven by the `lbt-recipes` comfort-map recipes; the categorical `thermal_condition` flowing through the shared `ladybug.datatype` registry
- Reject: wrapper-renames of the model functions or `ComfortCollection`; a hand-rolled PMV/UTCI/PET/adaptive/SolarCal heat-balance, comfort-threshold logic, or MRT map where this package already owns it; a per-result method family over the result-property collection; a re-derivation of the `DataCollection` time-series algebra (owned by `ladybug-core`); identity minting the runtime owns
- Note: AGPL-3.0 copyleft — the comfort rail runs out-of-process and graduates comfort `DataCollection`/map evidence across the wire, never linked into a distributed host binary

[CAPTURE_GAP]:
- members: verified by `assay api` reflection against the installed `ladybug-comfort==0.18.61` distribution. Every documented function, collection, parameter, and map kernel resolves with the signatures shown — no phantom. Confirmed shapes: `pmv.predicted_mean_vote(ta, tr, vel, rh, met, clo, wme=0, still_air_threshold=0.1)`; `utci.universal_thermal_climate_index(ta, tr, vel, rh)`; `PMV(air_temperature, rel_humidity, rad_temperature=None, air_speed=None, met_rate=None, clo_value=None, external_work=None, comfort_parameter=None)` with `PMV.from_epw(epw, include_wind=True, include_sun=True, met_rate=None, clo_value=None, external_work=None, pmv_parameter=None)`; `UTCI.from_epw(epw, include_wind=True, include_sun=True, utci_parameter=None)`; `clo.schiavon_clo(adapt_temp, max_clo=1, max_clo_temp=-5, min_clo=0.46, min_clo_temp=26)`; comfort results (`predicted_mean_vote`, `thermal_condition`, `universal_thermal_climate_index`) confirmed as properties returning `DataCollection`s; `map.mrt.shortwave_mrt_map(location, longwave_data, sun_up_hours, indirect_ill, direct_ill=None, ref_ill=None, ...)`. License confirmed AGPL-3.0; `ladybug-core==0.43.18` confirmed as the runtime dependency.
