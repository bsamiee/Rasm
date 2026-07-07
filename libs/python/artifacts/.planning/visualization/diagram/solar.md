# [PY_ARTIFACTS_VISUALIZATION_DIAGRAM_SOLAR]

The solar-ephemeris owner of the diagram plane — real sun geometry from date and site, so a sun-path diagram is computed astronomy, never caller-supplied azimuth/altitude rows alone. `pvlib.solarposition` owns the ephemeris: `get_solarposition(time, latitude, longitude, altitude=)` returns the NREL SPA frame whose refraction-corrected `apparent_zenith`/`apparent_elevation`/`azimuth` columns are the drawn angles (the true geometric columns stand distinct for shading math), `sun_rise_set_transit_spa(times, latitude, longitude)` returns timezone-aware sunrise/sunset/transit stamps, `nrel_earthsun_distance` the orbital radius, and the closed-form tier (`hour_angle`, `declination_spencer71`, `equation_of_time_spencer71`) backs the vectorized solvers — solstice dates derive from the declination extremes over a year's `dayofyear` sweep, never a hardcoded calendar. Every sampler is numpy-vectorized over a `pandas.DatetimeIndex`; an owned closed-form NOAA/SPA-grade kernel stands recorded as the declined-admission fallback, and geometry's AGPL ladybug `Sunpath` stays out on the process boundary.

The page generates the sun-path FURNITURE as owned geometry over the vector plane: the horizon circle, the 10°-step altitude rings, the compass ticks and cardinal labels, the labeled solstice/equinox date arcs, and the analemma hour lines — each a `SolarProjection` fold (stereographic, equidistant, orthographic hemisphere projections) from `(azimuth, altitude)` to sheet coordinates, emitted as vector-path fragments. `visualization/diagram/layout#LAYOUT`'s `SUN_PATH` projection arm composes this owner downward for its positioned marks; `visualization/diagram/draw#DRAW` renders the result. Entry is NONE — solar emits geometry values into the layout chain; the diagram receipt is draw's.

## [01]-[INDEX]

- [01]-[SOLAR]: the ephemeris and furniture owner — `Site` the geodetic anchor, `positions()` the SPA frame sampler, `day_arc()`/`analemma()`/`solstices()` the date-geometry solvers, `SolarProjection` the closed hemisphere-projection family with its `project()` fold, and `furniture()` the generated sun-path furniture (horizon, altitude rings, compass, date arcs, hour lines) as vector fragments — composed by layout's `SUN_PATH` arm and drawn by draw; importing pvlib, numpy/pandas, and the vector path plane; minting nothing.

## [02]-[SOLAR]

- Owner: one ephemeris seam, one projection law, one furniture generator. `Site(latitude, longitude, altitude, tz)` anchors every solver; `positions(site, times)` is the ONE pvlib call the page fans from — a `DatetimeIndex` in, the SPA frame out, `apparent_*` columns drawn and true columns carried; nothing else touches the provider.
- Cases: `SolarProjection` closes the hemisphere-to-sheet family — `STEREOGRAPHIC` (`r = R·tan((90°−alt)/2)`, the drafting-standard sun-path chart), `EQUIDISTANT` (`r = R·(90°−alt)/90°`, uniform altitude rings), `ORTHOGRAPHIC` (`r = R·cos(alt)`, the shadow-plan projection) — `project(az, alt, kind, radius)` folds one `(azimuth, altitude)` pair or a vectorized column pair into sheet `(x, y)` with north up and east right.
- Solvers: `day_arc(site, date, step_min)` samples one date's daylight sweep between the SPA sunrise/sunset stamps; `analemma(site, hour)` samples one clock hour across the year's days (the figure-eight); `solstices(year)` derives the summer/winter solstice and the equinox pair from the `declination_spencer71` extremes and zero-crossings over the `dayofyear` sweep; `rise_set(site, dates)` projects the `sun_rise_set_transit_spa` stamps. Every solver returns typed rows, never a provider frame leaking upward.
- Furniture: `furniture(site, kind, radius)` generates the owned chart furniture — the horizon circle, `ALTITUDE_RINGS` at the 10° cascade with their labels, the 16-point compass tick ring with cardinal text anchors, the three labeled date arcs (summer solstice, equinox, winter solstice) from `day_arc` sweeps, and the analemma hour lines at each daylight clock hour — each row a `Furnishing(kind, fragment, label)` whose `fragment` is a vector-path d-string sampled through the projection fold. Draw styles the furniture through theme rows; solar owns only geometry.
- Growth: a new projection is one `SolarProjection` member plus one `project` arm; a new furniture element is one `FurnishingKind` member plus one generator arm; a new solver is one typed fold over `positions`; site vocabularies (climate, obstruction masks) are upstream data — zero new surface here.
- Boundary: no rendering, styling, or layer projection (`draw`); no coordinate assignment of diagram marks (`layout` composes this owner for its `SUN_PATH` arm); no shading/energy analysis (the geometry/compute tracks); no receipt, no entry, no async — the SPA kernel is numpy-vectorized pure computation the consuming producer offloads inside its own seam; no ladybug (AGPL, process boundary, geometry track). A caller-supplied-angles-only path, a hardcoded solstice calendar, and a per-consumer projection re-derivation are the deleted forms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from datetime import date, datetime
from enum import StrEnum
from typing import Final

import numpy as np
import pandas as pd
from msgspec import Struct

from rasm.artifacts.graphic.vector.path import fragment

lazy from pvlib import solarposition

# --- [TYPES] ----------------------------------------------------------------------------
class SolarProjection(StrEnum):
    STEREOGRAPHIC = "stereographic"  # r = R·tan((90-alt)/2) — the drafting sun-path chart
    EQUIDISTANT = "equidistant"  # r = R·(90-alt)/90 — uniform altitude rings
    ORTHOGRAPHIC = "orthographic"  # r = R·cos(alt) — the shadow-plan projection


class FurnishingKind(StrEnum):
    HORIZON = "horizon"
    ALTITUDE_RING = "altitude_ring"
    COMPASS_TICK = "compass_tick"
    DATE_ARC = "date_arc"
    HOUR_LINE = "hour_line"


# --- [CONSTANTS] ------------------------------------------------------------------------
ALTITUDE_RINGS: Final[tuple[int, ...]] = (10, 20, 30, 40, 50, 60, 70, 80)
_COMPASS: Final[int] = 16  # tick ring divisions; cardinal labels on the 4 principal points


# --- [MODELS] ---------------------------------------------------------------------------
class Site(Struct, frozen=True):
    latitude: float
    longitude: float
    altitude: float = 0.0
    tz: str = "UTC"


class SunSample(Struct, frozen=True):
    at: datetime
    azimuth: float  # degrees clockwise from north (SPA apparent)
    altitude: float  # apparent elevation, refraction-corrected
    zenith_true: float  # true geometric zenith — shading math reads this, never the drawn angle


class SeasonDates(Struct, frozen=True):
    summer_solstice: date
    winter_solstice: date
    equinox_spring: date
    equinox_autumn: date


class Furnishing(Struct, frozen=True):
    kind: FurnishingKind
    fragment: str  # vector-path d-string in sheet coordinates
    label: str = ""
    anchor: tuple[float, float] = (0.0, 0.0)  # label seat


# --- [OPERATIONS] -----------------------------------------------------------------------
def positions(site: Site, times: pd.DatetimeIndex, /) -> tuple[SunSample, ...]:
    # the ONE provider seam: the SPA frame decoded into typed rows; apparent columns drawn, true carried.
    frame = solarposition.get_solarposition(times, site.latitude, site.longitude, altitude=site.altitude)
    return tuple(
        SunSample(at=at.to_pydatetime(), azimuth=float(az), altitude=float(el), zenith_true=float(zt))
        for at, az, el, zt in zip(frame.index, frame["azimuth"], frame["apparent_elevation"], frame["zenith"])
    )


def project(azimuth: float, altitude: float, kind: SolarProjection, radius: float, /) -> tuple[float, float]:
    # hemisphere -> sheet: north up, east right; below-horizon samples clamp to the horizon circle.
    alt = max(altitude, 0.0)
    r = {
        SolarProjection.STEREOGRAPHIC: radius * np.tan(np.radians((90.0 - alt) / 2.0)),
        SolarProjection.EQUIDISTANT: radius * (90.0 - alt) / 90.0,
        SolarProjection.ORTHOGRAPHIC: radius * np.cos(np.radians(alt)),
    }[kind]
    theta = np.radians(azimuth)
    return (r * np.sin(theta), -r * np.cos(theta))


def day_arc(site: Site, on: date, kind: SolarProjection, radius: float, step_min: int = 10, /) -> tuple[SunSample, ...]:
    # one date's daylight sweep between the SPA sunrise/sunset stamps; a polar no-rise/no-set date carries
    # NaT stamps, so the sweep falls to the full civil day filtered above-horizon — polar night yields the
    # empty arc, polar day the full 24h ring.
    start = pd.Timestamp(on, tz=site.tz)
    rs = solarposition.sun_rise_set_transit_spa(pd.DatetimeIndex([start]), site.latitude, site.longitude)
    rise, sets = rs["sunrise"].iloc[0], rs["sunset"].iloc[0]
    return (
        positions(site, pd.date_range(rise, sets, freq=f"{step_min}min"))
        if not (pd.isna(rise) or pd.isna(sets))
        else tuple(s for s in positions(site, pd.date_range(start, start + pd.Timedelta(days=1), freq=f"{step_min}min")) if s.altitude > 0.0)
    )


def analemma(site: Site, hour: int, year: int, /) -> tuple[SunSample, ...]:
    # one clock hour across the year — the figure-eight hour line.
    days = pd.date_range(f"{year}-01-01 {hour:02d}:00", f"{year}-12-31 {hour:02d}:00", freq="7D", tz=site.tz)
    return tuple(s for s in positions(site, days) if s.altitude > 0.0)


def solstices(year: int, /) -> SeasonDates:
    # derived from the declination extremes and zero-crossings — never a hardcoded calendar.
    doy = np.arange(1, 366)
    decl = solarposition.declination_spencer71(doy)
    base = date(year, 1, 1).toordinal() - 1
    crossings = np.where(np.diff(np.sign(decl)) != 0)[0] + 1
    return SeasonDates(
        summer_solstice=date.fromordinal(base + int(doy[np.argmax(decl)])),
        winter_solstice=date.fromordinal(base + int(doy[np.argmin(decl)])),
        equinox_spring=date.fromordinal(base + int(doy[crossings[0]])),
        equinox_autumn=date.fromordinal(base + int(doy[crossings[-1]])),
    )


def furniture(site: Site, kind: SolarProjection, radius: float, year: int, /) -> tuple[Furnishing, ...]:
    # the owned sun-path chart furniture: horizon, altitude rings, compass, labeled date arcs, hour lines.
    seasons = solstices(year)
    rings = tuple(
        Furnishing(FurnishingKind.ALTITUDE_RING, _circle(project(0.0, alt, kind, radius)[1] * -1.0), f"{alt}°")
        for alt in ALTITUDE_RINGS
    )
    ticks = tuple(
        Furnishing(FurnishingKind.COMPASS_TICK, _tick(i * 360.0 / _COMPASS, radius), "NESW"[i // 4] if i % 4 == 0 else "")
        for i in range(_COMPASS)
    )
    arcs = tuple(
        Furnishing(FurnishingKind.DATE_ARC, _polyline(day_arc(site, on, kind, radius), kind, radius), label)
        for on, label in (
            (seasons.summer_solstice, seasons.summer_solstice.strftime("%b %d").upper()),
            (seasons.equinox_spring, f"{seasons.equinox_spring.strftime('%b %d').upper()} / {seasons.equinox_autumn.strftime('%b %d').upper()}"),
            (seasons.winter_solstice, seasons.winter_solstice.strftime("%b %d").upper()),
        )
    )
    hours = tuple(
        Furnishing(FurnishingKind.HOUR_LINE, _polyline(analemma(site, h, year), kind, radius), f"{h:02d}")
        for h in range(5, 20)
    )
    return (Furnishing(FurnishingKind.HORIZON, _circle(radius)), *rings, *ticks, *arcs, *hours)


def _circle(radius: float, /) -> str:
    return fragment(f"M {radius} 0 A {radius} {radius} 0 1 1 {-radius} 0 A {radius} {radius} 0 1 1 {radius} 0 Z")


def _tick(azimuth: float, radius: float, /) -> str:
    (x0, y0), (x1, y1) = (project(azimuth, 0.0, SolarProjection.EQUIDISTANT, radius * 0.97),
                          project(azimuth, 0.0, SolarProjection.EQUIDISTANT, radius))
    return fragment(f"M {x0} {y0} L {x1} {y1}")


def _polyline(samples: tuple[SunSample, ...], kind: SolarProjection, radius: float, /) -> str:
    pts = tuple(project(s.azimuth, s.altitude, kind, radius) for s in samples)
    return fragment("M " + " L ".join(f"{x:.2f} {y:.2f}" for x, y in pts)) if pts else ""


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "ALTITUDE_RINGS",
    "Furnishing",
    "FurnishingKind",
    "SeasonDates",
    "Site",
    "SolarProjection",
    "SunSample",
    "analemma",
    "day_arc",
    "furniture",
    "positions",
    "project",
    "solstices",
]
```

A sun-path diagram assembled from this owner is astronomy at drawing grade: the date arcs are SPA sweeps between real sunrise and sunset stamps for the project site, the hour lines are sampled analemmas, the solstice labels fall on the derived declination extremes, and the whole hemisphere folds to the sheet through one declared projection — the same `project` law positioning the furniture, the layout plane's `SUN_PATH` marks, and any shadow-study overlay, so the chart's rings, ticks, and arcs cannot disagree with its data. Draw styles the furnishings through theme rows and projects them into the layer tree; the true-zenith column rides every sample for downstream shading math; and the single provider seam keeps the ephemeris swappable against the recorded closed-form fallback without touching a consumer.
