# [PY_ARTIFACTS_API_PVLIB]

`pvlib.solarposition` owns the NREL SPA solar-ephemeris surface feeding the artifacts diagram rail: apparent and true solar position, sunrise/sunset/transit times, Earth-Sun distance, hour angle, and the analytical declination and equation-of-time closed forms, all numpy-vectorized over the input `pandas.DatetimeIndex`. It emits ephemeris data alone, so `visualization/diagram/solar#SOLAR` consumes the `azimuth`/`apparent_elevation` columns as source geometry for sun-path arcs, projection, and furniture.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solar-position result frames

`pvlib.solarposition` returns a `pandas.DataFrame`/`Series` on the input `DatetimeIndex`, and a consumer reads these columns by name. Every angle is in degrees; an `apparent_` column is atmospheric-refraction-corrected where the bare column is true geometric, and the `sunrise`/`sunset`/`transit` columns come from `sun_rise_set_transit_spa`.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [CAPABILITY]                                                             |
| :-----: | :------------------- | :-------------- | :----------------------------------------------------------------------- |
|  [01]   | `apparent_zenith`    | position column | refraction-corrected zenith (`90 - apparent_elevation`); altitude source |
|  [02]   | `zenith`             | position column | true geometric zenith                                                    |
|  [03]   | `apparent_elevation` | position column | refraction-corrected altitude above horizon (negative below horizon)     |
|  [04]   | `elevation`          | position column | true geometric altitude                                                  |
|  [05]   | `azimuth`            | position column | clockwise from north (`0`=N,`90`=E,`180`=S,`270`=W); compass-tick source |
|  [06]   | `equation_of_time`   | position column | equation of time, minutes (SPA/`ephemeris` frames); feeds `hour_angle`   |
|  [07]   | `sunrise`            | rise/set frame  | tz-aware first-light `Timestamp` per day                                 |
|  [08]   | `sunset`             | rise/set frame  | tz-aware last-light `Timestamp` per day                                  |
|  [09]   | `transit`            | rise/set frame  | tz-aware solar-noon `Timestamp` per day; date-arc bound                  |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SPA solar position

Every function vectorizes over the input `DatetimeIndex` (`time`/`times`), never a per-timestamp loop; leading positionals are `(time, latitude, longitude)` with `latitude`/`longitude` in scalar degrees (north/east positive) and `altitude` in metres.
- SPA carry: `altitude`, `pressure`, `temperature`, `atmos_refract`, `delta_t` — the refraction-model and TT-UT1 kwargs the `…` tail elides

| [INDEX] | [SURFACE]                                   | [CAPABILITY]                                                               |
| :-----: | :------------------------------------------ | :------------------------------------------------------------------------- |
|  [01]   | `get_solarposition(…, method='nrel_numpy')` | dispatch entry; `method` picks the SPA backend; zenith/azimuth + EoT frame |
|  [02]   | `spa_python(…, atmos_refract=None)`         | direct NREL SPA, pure-Python vectorized; high-accuracy default             |
|  [03]   | `sun_rise_set_transit_spa(…)`               | SPA sunrise/sunset/solar-transit `Timestamp`s per day; date-arc source     |
|  [04]   | `nrel_earthsun_distance(…) -> Series`       | Earth-Sun distance in AU; the apparent-sun-radius scale factor             |
|  [05]   | `ephemeris(…)`                              | pure-Python no-scipy fallback; adds `solar_time`; lower accuracy than SPA  |

[ENTRYPOINT_SCOPE]: analytical closed forms and the Location aggregator

`declination_*`/`equation_of_time_*` closed forms take `(dayofyear) -> numeric` and feed a scipy-free sampling path; `Location` binds latitude/longitude/altitude/tz once and forwards to the module.

| [INDEX] | [SURFACE]                                                          | [CAPABILITY]                                                  |
| :-----: | :----------------------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `declination_spencer71(dayofyear)`                                 | solar declination, radians (Spencer 1971 Fourier series)      |
|  [02]   | `declination_cooper69(dayofyear)`                                  | solar declination, radians (Cooper 1969); equinox-arc source  |
|  [03]   | `equation_of_time_spencer71(dayofyear)`                            | equation of time, minutes (Spencer 1971)                      |
|  [04]   | `equation_of_time_pvcdrom(dayofyear)`                              | equation of time, minutes (PVCDROM)                           |
|  [05]   | `hour_angle(times, longitude, equation_of_time) -> ndarray`        | solar hour angle in degrees from EoT; hour-line arc parameter |
|  [06]   | `sun_rise_set_transit_geometric(…, declination, equation_of_time)` | geometric rise/set/transit from declination + EoT (no SPA)    |
|  [07]   | `Location(...).get_solarposition(times, …)`                        | site-bound aggregator forwarding to the module                |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `get_solarposition(method=)` is the single polymorphic dispatch — `method` discriminates the SPA backend (`'nrel_numpy'`/`'nrel_numba'`/`'pyephem'`/`'ephemeris'`/`'nrel_c'`); a new accuracy tier is a `method` value, never a parallel owner, with `spa_python` the direct high-accuracy backend and `ephemeris` the scipy-free path.
- Every function vectorizes over the input `DatetimeIndex`; one `pandas.date_range` samples the whole solstice/equinox/hour-line grid in a single call and the `azimuth`/`apparent_elevation` columns return as `numpy` arrays.
- `apparent_zenith`/`apparent_elevation`/`azimuth` (refraction-corrected) are the on-sky projection inputs and the bare `zenith`/`elevation` carry the geometric angle; `pressure`/`temperature`/`altitude` tune the refraction model and `delta_t` the TT-UT1 offset.
- pvlib owns the ephemeris alone: `visualization/diagram/solar` generates the sun-path arcs, 2D projection, and furniture over `graphic/vector/path`.
- Import at boundary scope only (`lazy import pvlib.solarposition`); `solarposition` and `pvlib.location` are the sole admitted namespaces, the PV-system/irradiance/module-model surface out of scope.

[STACKING]:
- `numpy`(`.api/numpy.md`): the SPA frame is a `numpy`-vectorized `pandas.DataFrame`/`Series` on the `DatetimeIndex`; the `azimuth`/`apparent_elevation` columns read as dense arrays feeding the arc generator with no per-timestamp loop.
- `visualization/diagram/solar#SOLAR` binds one `Location(latitude, longitude, tz, altitude)` per site and samples `Location.get_solarposition(pandas.date_range(...))` across the three cardinal declination days at `freq='5min'`, feeding the returned columns through its owned `SolarProjection` into the `graphic/vector/path#PATH` arc generator, so a labeled sun-path chart is real ephemeris geometry.
- `sun_rise_set_transit_spa` bounds each date arc at true horizon crossing and `nrel_earthsun_distance` scales the sun disc; arc labels route to `typography/shape#SHAPE` outlines and the furniture projects into `graphic/layer#LAYER` named layers the `visualization/diagram/draw#DRAW` terminal emits.
- `visualization/diagram/layout#LAYOUT`'s `SUN_PATH` kind receives the column-named solar geometry as its coordinate ingress.

[LOCAL_ADMISSION]:
- Bind one `Location(latitude, longitude, tz, altitude)` per site and forward through `Location.get_solarposition`/`get_sun_rise_set_transit`, never bare lat/lon threaded through call sites.
- Pass site `pressure`/`temperature`/`altitude` for a high-altitude or non-standard-atmosphere site, where the sea-level defaults bias the refraction correction.
- Keep pvlib to the ephemeris frame; the arc geometry, projection, furniture, and labels are the solar owner's over `graphic/vector/path`/`typography/shape`, never a pvlib matplotlib plot.
