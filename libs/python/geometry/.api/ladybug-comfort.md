# [PY_GEOMETRY_API_LADYBUG_COMFORT]

`ladybug-comfort` is the thermal-comfort layer over `ladybug-core`: pointwise heat-balance model functions (PMV, UTCI, PET, adaptive, SolarCal, and the simple heat/cold indices), `DataCollection`-wrapping `ComfortCollection` calculators that map those functions across time-series, serializable `Parameter` configs carrying thresholds and standard selection, the psychrometric comfort polygons, and the spatial `map.*` MRT/TCP/air kernels the `lbt-recipes` comfort-map recipes drive. It computes comfort physics over `ladybug-core` collections, driving no simulation in-process.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ladybug-comfort`
- package: `ladybug-comfort` (AGPL-3.0)
- module: `ladybug_comfort` — pure Python; submodule import at boundary scope
- namespaces: `ladybug_comfort.{pmv,utci,pet,adaptive,solarcal,at,di,hi,humidex,wbgt,wc,ts,degreetime,clo}`, `ladybug_comfort.collection.*`, `ladybug_comfort.parameter.*`, `ladybug_comfort.chart.*`, `ladybug_comfort.map.*`
- rail: energy-companion (thermal-comfort calculation)
- owner: `geometry`
- consumer: `.planning/energy/climate.md` (PMV/UTCI/PET, SolarCal MRT, the comfort-map rows)
- depends: `ladybug-core` (consumes `DataCollection`/`EPW`/`datatype`); `numpy` under the `mapping` extra (the spatial `map` kernels)
- entry points: `ladybug-comfort` console script (`datacollection`/`epw`/`map`/`mtx`/`sql` groups); the rail composes the API
- scope-law: owns the comfort model functions, `ComfortCollection` calculators, `Parameter` configs, chart polygons, and spatial map kernels; not the `DataCollection` algebra (`ladybug-core`), the radiance matrices (honeybee-radiance via `lbt-recipes`), or comfort-map orchestration (`lbt-recipes`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: comfort collections (`ladybug_comfort.collection`)

All subclass `collection.base.ComfortCollection`: each takes aligned `ladybug-core` `DataCollection`s (or reads an `EPW` via `from_epw`) and exposes each comfort result as a matching collection. Comfort model is the class, result a property. Symbols elide the `collection.` prefix.

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------ | :------------ | :----------------------------------------------------- |
|  [01]   | `pmv.PMV`                                               | class         | Fanger PMV/PPD/SET; `standard_effective_temperature`   |
|  [02]   | `utci.UTCI`                                             | class         | UTCI; n-point `thermal_condition_*` (2/5/7/9/11-point) |
|  [03]   | `adaptive.Adaptive`                                     | class         | `neutral_temperature`, `degrees_from_neutral`          |
|  [04]   | `adaptive.Adaptive`                                     | class         | `prevailing_outdoor_temperature`, `thermal_condition`  |
|  [05]   | `pet.PET`                                               | class         | `physiologic_equivalent_temperature`, `pet_category`   |
|  [06]   | `pet.PET`                                               | class         | `core_body_temperature`, `skin_temperature`            |
|  [07]   | `solarcal.OutdoorSolarCal` / `IndoorSolarCal`           | class         | SolarCal `mean_radiant_temperature`, `mrt_delta`       |
|  [08]   | `solarcal.HorizontalSolarCal` / `HorizontalRefSolarCal` | class         | short/longwave ERF; MRT for the thermal models         |

[PUBLIC_TYPE_SCOPE]: comfort parameters and visualization (`parameter`, `chart`)

`Parameter` objects are serializable config carriers — thresholds and standard selectors parameterizing a `ComfortCollection` — carrying `from_dict`/`to_dict`/`from_string` and `is_comfortable`/`thermal_condition` predicates. `Parameter` symbols elide the `parameter.` prefix.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :------------------------------------------------ | :------------ | :---------------------------------------------------------- |
|  [01]   | `pmv.PMVParameter`                                | class         | `ppd_comfort_thresh`, `humid_ratio` bounds                  |
|  [02]   | `pmv.PMVParameter`                                | class         | `still_air_threshold`, `discomfort_reason`                  |
|  [03]   | `utci.UTCIParameter`                              | class         | cold/heat stress thresholds; `original_utci_category`       |
|  [04]   | `utci.UTCIParameter`                              | class         | n-point `thermal_condition_*`                               |
|  [05]   | `adaptive.AdaptiveParameter`                      | class         | `ashrae_or_en`, `conditioning`, `neutral_offset`, air-speed |
|  [06]   | `pet.PETParameter` / `solarcal.SolarCalParameter` | class         | body/posture/reflectance/exposure config                    |
|  [07]   | `chart.polygonpmv.PolygonPMV`                     | class         | PMV comfort polygon over a `PsychrometricChart`             |
|  [08]   | `chart.polygonutci.PolygonUTCI`                   | class         | UTCI comfort polygon                                        |
|  [09]   | `chart.adaptive.AdaptiveChart`                    | class         | adaptive comfort chart                                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pointwise comfort model functions

Pure scalar heat-balance kernels (SI/radians in, comfort scalar out) the collections vectorize; the standard variant is an argument or a sibling function, never a class. They vectorize under `numpy` for the spatial map.

| [INDEX] | [SURFACE]                                                             | [SHAPE]                | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------------------------- | :--------------------- | :------------------------------------- |
|  [01]   | `pmv.predicted_mean_vote(ta, tr, vel, rh, met, clo, ...)`             | scalar climate + body  | Fanger PMV/PPD + Pierce SET dict       |
|  [02]   | `pmv.fanger_pmv(...)` / `pmv.pierce_set(...)`                         | scalars                | underlying Fanger / Pierce models      |
|  [03]   | `pmv.ppd_from_pmv(pmv)` / `pmv.pmv_from_ppd(ppd)`                     | scalars                | PMV <-> PPD map                        |
|  [04]   | `utci.universal_thermal_climate_index(ta, tr, vel, rh)`               | scalar climate         | UTCI in degrees C                      |
|  [05]   | `pet.physiologic_equivalent_temperature(ta, tr, vel, rh, ...)`        | scalar climate + body  | PET via the MEMI heat-balance model    |
|  [06]   | `pet.pet_category(pet)`                                               | scalar climate         | PET category band                      |
|  [07]   | `adaptive.adaptive_comfort_ashrae55(t_prevail, to)`                   | prevailing + operative | ASHRAE-55 adaptive                     |
|  [08]   | `adaptive.adaptive_comfort_en15251(t_prevail, to)`                    | prevailing + operative | EN-15251 adaptive                      |
|  [09]   | `adaptive.adaptive_comfort_conditioned(t_prevail, to, ...)`           | prevailing + operative | conditioned adaptive (`conditioning`)  |
|  [10]   | `adaptive.cooling_effect_ashrae55` / `_en15251` / `_en16798`          | prevailing + operative | air-speed cooling effect (EN-16798)    |
|  [11]   | `solarcal.outdoor_sky_heat_exch(...)` / `indoor_sky_heat_exch(...)`   | solar flux             | SolarCal sky heat exchange             |
|  [12]   | `solarcal.erf_from_body_solar_flux(...)` / `mrt_delta_from_erf(...)`  | solar flux             | ERF -> MRT-delta                       |
|  [13]   | `at.apparent_temperature(ta, rh, ws)` / `di.discomfort_index(ta, rh)` | scalar climate         | apparent temp / discomfort index       |
|  [14]   | `hi.heat_index(ta, rh)` / `humidex.humidex(ta, tdp)`                  | scalar climate         | heat index / humidex                   |
|  [15]   | `wbgt.wet_bulb_globe_temperature(...)` / `wc.windchill_temp(ta, ws)`  | scalar climate         | WBGT / wind chill                      |
|  [16]   | `ts.thermal_sensation(...)`                                           | scalar climate         | thermal-sensation index + warning band |
|  [17]   | `degreetime.heating_degree_time(...)` / `cooling_degree_time(...)`    | temperature            | heating / cooling degree-time          |
|  [18]   | `clo.schiavon_clo(adapt_temp, max_clo, min_clo, ...)`                 | temperature            | adaptive-temp clothing estimate        |

[ENTRYPOINT_SCOPE]: comfort-collection construction and results (`collection`)

A `ComfortCollection` takes aligned collections (or an `EPW` via `from_epw`) with an optional `Parameter`, exposing each result as a `DataCollection`. `from_epw` is the one-call weather-to-comfort path, routing the EPW solar fields through SolarCal to the `rad_temperature` MRT input.

| [INDEX] | [SURFACE]                                                         | [SHAPE]         | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------- |
|  [01]   | `PMV(air_temperature, rel_humidity, rad_temperature, ...)`        | ctor            | build PMV over time-series           |
|  [02]   | `PMV.from_epw(epw, include_wind, include_sun, ...)`               | factory         | weather-to-PMV (sun -> MRT)          |
|  [03]   | `UTCI(air_temperature, rel_humidity, ...)`                        | ctor            | build UTCI over time-series          |
|  [04]   | `UTCI.from_epw(epw, include_wind, include_sun, ...)`              | factory         | weather-to-UTCI                      |
|  [05]   | `Adaptive(...)` / `Adaptive.from_air_and_rad_temp(...)`           | ctor / factory  | adaptive calculators                 |
|  [06]   | `PET(...)` / `PET.from_epw(...)`                                  | ctor / factory  | PET calculators                      |
|  [07]   | `PMV.predicted_mean_vote` / `.percentage_people_dissatisfied`     | property        | PMV results as `DataCollection`s     |
|  [08]   | `.thermal_condition` / `.is_comfortable` / `.percent_comfortable` | property        | PMV categorical results              |
|  [09]   | `UTCI.universal_thermal_climate_index`                            | property        | UTCI index collection                |
|  [10]   | `.thermal_condition_nine_point` / `.percent_strong_heat_stress`   | property        | UTCI categorical stress              |
|  [11]   | `OutdoorSolarCal(...)` / `.mean_radiant_temperature`              | ctor / property | SolarCal MRT feeds `rad_temperature` |

[ENTRYPOINT_SCOPE]: comfort parameters and spatial maps (`parameter`, `map`)

`Parameter` objects round-trip through `from_dict`/`to_dict`/`from_string`; unset slots default to the 10% PPD / 0.1 m/s still-air standard. `map.*` kernels operate over honeybee-radiance MRT/irradiance matrices for the per-sensor comfort map the recipes drive.

| [INDEX] | [SURFACE]                                                                   | [SHAPE]      | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------------------------------- | :----------- | :------------------------------------- |
|  [01]   | `PMVParameter(...)` / `.from_dict(d)` / `.to_dict()` / `.from_string(s)`    | ctor / codec | PMV comfort config (unset -> defaults) |
|  [02]   | `UTCIParameter(...)` / `AdaptiveParameter(...)`                             | ctor         | UTCI + adaptive config carriers        |
|  [03]   | `PETParameter(...)` / `SolarCalParameter(...)`                              | ctor         | PET + SolarCal config carriers         |
|  [04]   | `<Parameter>.is_comfortable(value, ...)` / `.thermal_condition(value, ...)` | instance     | comfort predicate + condition          |
|  [05]   | `map.mrt.shortwave_mrt_map(...)` / `longwave_mrt_map(...)`                  | static       | per-sensor MRT map from radiance       |
|  [06]   | `map.tcp.tcp_total(condition_csv, schedule)` / `tcp_model_schedules(...)`   | static       | thermal-comfort-percent map            |
|  [07]   | `map.air.air_map(enclosure_info, sql, epw, ...)`                            | static       | air-temperature map                    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Comfort computes over `ladybug-core` `DataCollection`s and drives no in-process simulation: a `ComfortCollection` maps the pointwise model function element-wise across aligned collections and exposes each result as a `DataCollection` with the right `Header`. SolarCal MRT feeds the `rad_temperature` of the thermal models; every categorical output (`thermal_condition`, `pet_category`) carries a `ladybug.datatype` registry `data_type`, sharing the climate visualization rail.
- Owner aligns inputs with `are_collections_aligned` before construction and lifts misalignment to a typed precondition.

[STACKING]:
- `ladybug-core`(`.api/ladybug-core.md`): a `ComfortCollection` is the concrete `compute_function_aligned` realization — it maps a pointwise comfort function across aligned `DataCollection`s; `from_epw(epw, include_sun=True)` routes the EPW solar fields through SolarCal to the `rad_temperature` input.
- `numpy`(`.api/numpy.md`): vectorize the pointwise kernels (`predicted_mean_vote`/`universal_thermal_climate_index`/`physiologic_equivalent_temperature`) for the spatial map (tens of thousands of sensors x 8760 hours) rather than looping the Python `ComfortCollection`; both forms compute the identical model.
- `msgspec`(`.api/msgspec.md`) / `pydantic`(`.api/pydantic.md`): map a `Parameter`'s `from_dict`/`to_dict`/`from_string` onto a `Struct`/model, serialized once at the boundary and threaded into the collection's `comfort_parameter=` slot — the threshold/standard is data, not a code branch.
- honeybee-radiance / `lbt-recipes`: `map.mrt.shortwave_mrt_map`/`longwave_mrt_map` consume honeybee-radiance `indirect_ill`/`direct_ill`/`ref_ill` matrices, `map.tcp.tcp_total` folds a comfort result into a thermal-comfort-percent map, and `map.air.air_map` produces the air field — the kernels the `pmv_comfort_map`/`utci_comfort_map`/`adaptive_comfort_map` recipes drive after the radiance simulation.
- within-lib: the comfort owner composes the `ComfortCollection` constructors (`from_epw`), the `Parameter` configs, and the `map` kernels into one owner; `chart.PolygonPMV`/`PolygonUTCI` render directly over a `ladybug.psychchart.PsychrometricChart`.

[LOCAL_ADMISSION]:
- Admitted as a process-boundary companion on `ladybug-core`; the honeybee-radiance matrices and recipe orchestration are the design page's provisioning obligation, not a pip dependency of this catalog.

[RAIL_LAW]:
- Package: `ladybug-comfort`
- Owns: the pointwise PMV/SET/PPD, UTCI, PET, ASHRAE-55/EN-15251 adaptive (with EN-16798 cooling effect), SolarCal MRT model functions, and the simple heat/cold indices; the `ComfortCollection` calculators with `from_epw` constructors; the serializable `Parameter` configs; the psychrometric-chart comfort polygons; and the spatial MRT/TCP/air comfort-map kernels
- Accept: comfort calculation stacked on `ladybug-core` `DataCollection`s/`EPW`; the `Parameter` serialized into recipe inputs; the `map` kernels driven by the `lbt-recipes` comfort-map recipes; the categorical `thermal_condition` flowing through the shared `ladybug.datatype` registry
- Reject: wrapper-renames of the model functions or `ComfortCollection`; a hand-rolled PMV/UTCI/PET/adaptive/SolarCal heat-balance, comfort-threshold logic, or MRT map this package owns; a per-result method family over the result-property collection; a re-derivation of the `DataCollection` algebra (owned by `ladybug-core`); identity minting the runtime owns
- Note: AGPL-3.0 copyleft — the comfort rail runs out-of-process and graduates comfort `DataCollection`/map evidence across the wire, never linked into a distributed host binary
