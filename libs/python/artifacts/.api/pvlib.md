# [PY_ARTIFACTS_API_PVLIB]

`pvlib` supplies the NREL-grade solar-ephemeris surface for the artifacts diagram rail: the `pvlib.solarposition` module owns the SPA (Solar Position Algorithm) family that turns a timezone-aware date/time index plus a latitude/longitude/altitude into refraction-corrected apparent azimuth and altitude, the sunrise/sunset/transit solvers, the analytical declination and equation-of-time closed forms, and the Earth-Sun distance — every result a numpy-vectorized `pandas` frame over the input `DatetimeIndex`. The `visualization/diagram/solar#SOLAR` owner composes `get_solarposition`/`sun_rise_set_transit_spa`/`nrel_earthsun_distance` as the ephemeris source under its owned sun-path FURNITURE generator (horizon circle, altitude rings, compass ticks, labeled solstice/equinox/hour-line date arcs projected over `SolarProjection` composing `graphic/vector/path#PATH`), replacing the parameterless caller-supplied azimuth/altitude rows; it never re-derives the SPA the native NREL algorithm already owns, and the owned closed-form NOAA/SPA-grade kernel stands recorded only as the declined-admission fallback. Distinct from geometry's AGPL ladybug `Sunpath` (process boundary, geometry track) — pvlib is BSD-3, in-process, interpreter-agnostic.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pvlib`
- package: `pvlib`
- import: `from pvlib import solarposition` / `from pvlib.location import Location`
- owner: `artifacts`
- rail: diagram
- installed: `0.15.2`
- license: `BSD-3-Clause` (NREL-backed; the in-process, copyleft-clean solar surface distinct from AGPL ladybug `Sunpath`)
- asset: pure-Python wheel (interpreter-agnostic, no native extension, no cp-gate); `numpy`/`pandas` are the numeric substrate, `scipy` an optional accelerator for the vectorized SPA `how='numpy'` path
- entry points: none (library only)
- capability: NREL SPA apparent/true solar zenith and azimuth from a date/latitude/longitude/altitude (`get_solarposition`/`spa_python`), sunrise/sunset/solar-transit times (`sun_rise_set_transit_spa`), Earth-Sun distance in AU (`nrel_earthsun_distance`), hour angle and analytical declination/equation-of-time closed forms, all numpy-vectorized over a `pandas.DatetimeIndex`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: solar-position result frames
- rail: diagram — `pvlib.solarposition` returns a `pandas.DataFrame`/`Series` indexed by the input `DatetimeIndex`; the columns are the closed vocabulary a consumer reads by name, never re-derived. Every angle column is in degrees. Apparent angles are atmospheric-refraction-corrected (the true, geometric angles carry no `apparent_` prefix).

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]  | [CAPABILITY]                                                                     |
| :-----: | :--------------------------- | :-------------- | :------------------------------------------------------------------------------- |
|  [01]   | `apparent_zenith`            | position column | refraction-corrected zenith (`90 - apparent_elevation`); altitude source         |
|  [02]   | `zenith`                     | position column | true geometric zenith                                                            |
|  [03]   | `apparent_elevation`         | position column | refraction-corrected altitude above horizon (negative below horizon)             |
|  [04]   | `elevation`                  | position column | true geometric altitude                                                          |
|  [05]   | `azimuth`                    | position column | azimuth clockwise from north (`0`=N,`90`=E,`180`=S,`270`=W); compass-tick source |
|  [06]   | `equation_of_time`           | position column | equation of time in minutes (SPA/`ephemeris` frames); feeds `hour_angle`         |
|  [07]   | `sunrise`/`sunset`/`transit` | rise/set frame  | `sun_rise_set_transit_spa` cols; tz-aware sunrise/sunset/transit per day         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SPA solar position
- rail: diagram — `pvlib.solarposition`; every function is numpy-vectorized over a `pandas.DatetimeIndex` (`time`/`times`), never a per-timestamp Python loop; `latitude`/`longitude` are scalar degrees (north/east positive), `altitude` metres. Each row's leading positional args are `(time|times, latitude, longitude)` and it returns a `DataFrame` except `nrel_earthsun_distance` (`Series`) and `hour_angle` (`ndarray`); the SPA backends share `how='numpy', delta_t=67.0, numthreads=4`; `altitude`/`pressure`/`temperature`/`atmos_refract` tune the refraction model and `delta_t` the TT-UT1 offset. The `…` tail is that shared kwarg set.

| [INDEX] | [CALL_SHAPE]                                     | [CAPABILITY]                                                               |
| :-----: | :----------------------------------------------- | :------------------------------------------------------------------------- |
|  [01]   | `get_solarposition(…, method='nrel_numpy')`      | dispatch entry; `method` picks the SPA backend; zenith/azimuth + EoT frame |
|  [02]   | `spa_python(…, atmos_refract=None)`              | direct NREL SPA (pure-Python, vectorized); the high-accuracy default       |
|  [03]   | `sun_rise_set_transit_spa(…)`                    | SPA sunrise/sunset/solar-transit `Timestamp`s per day; date-arc source     |
|  [04]   | `nrel_earthsun_distance(…)`                      | Earth-Sun distance in AU; the apparent-sun-radius scale factor             |
|  [05]   | `hour_angle(times, longitude, equation_of_time)` | solar hour angle from the EoT column; hour-line arc parameter              |
|  [06]   | `ephemeris(…)`                                   | pure-Python no-scipy fallback; adds `solar_time`; lower accuracy than SPA  |

[ENTRYPOINT_SCOPE]: analytical closed forms and the Location aggregator
- rail: diagram — the day-of-year closed forms feed a scipy-free sampling path; `Location` is the convenience owner binding latitude/longitude/altitude/tz once. The four `declination_*`/`equation_of_time_*` closed forms take `(dayofyear) -> numeric`.

| [INDEX] | [CALL_SHAPE]                                                              | [CAPABILITY]                                                 |
| :-----: | :------------------------------------------------------------------------ | :----------------------------------------------------------- |
|  [01]   | `declination_spencer71(dayofyear)`                                        | solar declination (Spencer 1971 Fourier series), radians     |
|  [02]   | `declination_cooper69(dayofyear)`                                         | solar declination (Cooper 1969), radians; equinox-arc source |
|  [03]   | `equation_of_time_spencer71(dayofyear)`                                   | equation of time in minutes (Spencer 1971)                   |
|  [04]   | `equation_of_time_pvcdrom(dayofyear)`                                     | equation of time in minutes (PVCDROM)                        |
|  [05]   | `hour_angle(times, longitude, equation_of_time) -> ndarray`               | hour angle in degrees (shared with the SPA path)             |
|  [06]   | `sun_rise_set_transit_geometric(…, declination, equation_of_time)`        | geometric rise/set/transit from declination + EoT (no SPA)   |
|  [07]   | `Location(latitude, longitude, tz, altitude).get_solarposition(times, …)` | site-bound aggregator forwarding to the module               |

## [04]-[IMPLEMENTATION_LAW]

[SOLAR_EPHEMERIS]:
- import: `from pvlib import solarposition` at boundary scope only (the `visualization/diagram/solar` page declares `lazy import pvlib.solarposition`); module-level import is banned by the manifest import policy.
- dispatch axis: `get_solarposition(method=)` is the single polymorphic entry — `method` discriminates the SPA backend (`'nrel_numpy'` default / `'nrel_numba'` JIT / `'pyephem'` / `'ephemeris'` scipy-free / `'nrel_c'` C-extension), never a parallel per-backend call in consuming code; `spa_python` is the direct high-accuracy default the solar owner binds, `ephemeris` the no-scipy fallback. A new accuracy tier is a `method` value, never a new owner.
- vectorization axis: every function is numpy/pandas vectorized over the input `DatetimeIndex` — sample the whole solstice/equinox/hour-line date grid as ONE `pandas.date_range` (e.g. `freq='5min'` across a solstice day, or the three cardinal declination days) and read the returned `azimuth`/`apparent_elevation` columns as numpy arrays; never a per-timestamp Python loop, and never a scalar call per arc point.
- angle axis: `apparent_zenith`/`apparent_elevation`/`azimuth` (refraction-corrected) are the sun-path projection inputs — altitude from `apparent_elevation`, compass bearing from `azimuth` (clockwise from north). The true `zenith`/`elevation` columns carry the geometric angle where refraction must be excluded. `pressure`/`temperature`/`atmos_refract` tune the refraction model; `delta_t` the TT-UT1 offset — pass the site rows, never assume sea-level defaults for a high-altitude site.
- furniture axis: the solar owner is the geometry generator — `nrel_earthsun_distance` scales the apparent solar radius, `sun_rise_set_transit_spa` bounds the labeled date arcs at true horizon crossing, `hour_angle` parameterizes the hour-line family, and the `declination_*` closed forms select the three cardinal-day arcs (summer solstice / equinox / winter solstice); the horizon circle, altitude rings, and compass ticks are owned generated geometry over `SolarProjection` composing `graphic/vector/path#PATH`, not a pvlib plot.
- boundary: pvlib owns the solar ephemeris ONLY — azimuth/altitude/rise-set/distance/declination. Sun-path arc geometry, projection to 2D, and the furniture rings/ticks/labels are `visualization/diagram/solar`'s owned generation over `graphic/vector/path`; label glyphs route through `typography/shape`; the diagram render is `visualization/diagram/draw`. pvlib emits no SVG, no plot, and no receipt — the ephemeris frame is data the solar owner consumes. Never admit its PV-system/irradiance/module-model surface: only `solarposition` is in scope.

[STACKING]:
- `visualization/diagram/solar#SOLAR` binds one `pvlib.location.Location(latitude, longitude, tz, altitude)` per site and samples `Location.get_solarposition(pandas.date_range(...))` across the three cardinal declination days at `freq='5min'`; the returned `azimuth`/`apparent_elevation` columns feed the owned `SolarProjection` (stereographic/equidistant/orthographic) → `graphic/vector/path#PATH` arc generator, so a labeled sun-path chart is real ephemeris geometry, never caller-supplied azimuth/altitude rows.
- `sun_rise_set_transit_spa` bounds each date arc at true horizon crossing and `nrel_earthsun_distance` scales the apparent sun disc, so the furniture (horizon circle, altitude rings, compass ticks, solstice/equinox/hour-line arcs) is dimensionally correct; the arc labels route to `typography/shape#SHAPE` outlines and the whole furniture set projects into `graphic/layer#LAYERED` named layers the `visualization/diagram/draw#DRAW` terminal emits.
- the solar frame is a `data/tabular`-shaped result — a `numpy`-vectorized sample the solar owner reads by column name (`azimuth`, `apparent_elevation`), so the `visualization/diagram/layout#LAYOUT` `SUN_PATH` kind receives real solar geometry as its coordinate ingress, and a node-link render wearing a sun-path label fails the V15 review.

[LOCAL_ADMISSION]:
- Sample the whole date/hour grid as one `pandas.date_range` and call the SPA function once over the `DatetimeIndex`; never a per-timestamp scalar call or a Python loop over arc points.
- Bind a `Location(latitude, longitude, tz, altitude)` once per site and forward through `get_solarposition`/`get_sun_rise_set_transit`; do not thread bare lat/lon through every call site.
- Read `apparent_zenith`/`apparent_elevation`/`azimuth` for on-sky projection (refraction-corrected); read the true `zenith`/`elevation` only where geometric angle is required.
- Select the accuracy tier through `get_solarposition(method=)` (`'nrel_numpy'` default, `'ephemeris'` when scipy is absent); never fork a parallel per-backend path.
- Pass site `pressure`/`temperature`/`altitude` for a high-altitude or non-standard-atmosphere site; the sea-level defaults bias the refraction correction otherwise.
- Keep pvlib to the ephemeris: arc geometry, projection, furniture, and labels are the solar owner's over `graphic/vector/path`/`typography/shape`, never a pvlib matplotlib plot; identity and receipts the runtime/owner mint.

[RAIL_LAW]:
- Package: `pvlib`
- Owns: NREL SPA solar position (apparent/true zenith, elevation, azimuth), sunrise/sunset/solar-transit times, Earth-Sun distance, hour angle, and the analytical declination/equation-of-time closed forms — all numpy-vectorized over a `pandas.DatetimeIndex`
- Accept: the solar-ephemeris source feeding `visualization/diagram/solar#SOLAR`'s owned sun-path furniture generator over `SolarProjection`/`graphic/vector/path`, one `Location` per site, sampled once over a whole date grid
- Reject: a per-timestamp scalar call or Python loop where the function vectorizes over a `DatetimeIndex`; a parallel per-backend path where `get_solarposition(method=)` dispatches; re-deriving the SPA the native NREL algorithm owns; admitting the PV-system/irradiance/module-model surface where only `solarposition` is in scope; a pvlib matplotlib plot where the solar owner generates the arc geometry; sea-level refraction defaults on a high-altitude site; the AGPL ladybug `Sunpath` (process boundary, geometry track) where this BSD-3 in-process surface serves
