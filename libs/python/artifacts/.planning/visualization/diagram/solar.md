# [PY_ARTIFACTS_DIAGRAM_SOLAR]

`solar` owns real sun geometry on the diagram plane. `Site` lowers once to `pvlib.location.Location`; `Location.get_solarposition` supplies refraction-corrected drawn angles and distinct true geometry, `Location.get_sun_rise_set_transit` supplies timezone-aware daylight bounds, and `solarposition.nrel_earthsun_distance` scales the apparent sun disc. `hour_angle`, `declination_spencer71`, and `equation_of_time_spencer71` derive hour lines and hemisphere-neutral March/June/September/December events over the complete civil year. Every sampler is numpy-vectorized over one `pandas.DatetimeIndex`, and `SolarFault` closes invalid domain inputs and provider refusals before geometry reaches layout.

Furniture is owned geometry over the vector plane, generated under one `FurniturePolicy` value: the horizon circle, altitude rings, azimuth grid rays, compass ticks and cardinal labels, labeled solstice/equinox date arcs, hour lines in either `HourConvention`, and the distance-scaled sun discs — each a `SolarProjection` fold from `(azimuth, altitude)` to sheet coordinates, emitted as vector-path fragments through the `graphic/vector/path#PATH` `fragment` serializer. Ring roster, compass division, hour band, sampling cadence, and disc scale are policy seed DATA, so a climate convention or office chart standard is one policy value, never an edited generator body. `visualization/diagram/layout#LAYOUT`'s `SUN_PATH` arm composes this owner for its positioned marks; `visualization/diagram/draw#DRAW` renders the result. Entry is NONE — solar emits geometry values into the layout chain, pure and synchronous; the consuming producer offloads, and the diagram receipt is draw's.

## [01]-[INDEX]

- [01]-[SOLAR]: the ephemeris and furniture owner — `Site` the geodetic-plus-atmosphere anchor lowered once to the pvlib `Location`, `positions()` the SPA-frame-plus-distance sampler, `day_arc()`/`analemma()`/`hour_line()`/`solstices()` the date-geometry solvers, `SolarProjection` the closed hemisphere-projection family with its `project()` fold, and `furniture()` the `FurniturePolicy`-seeded sun-path backdrop as vector fragments — composed by layout's `SUN_PATH` arm, drawn by draw, minting nothing.

## [02]-[SOLAR]

- Owner: one site-bound ephemeris seam, one projection law, and one policy-seeded furniture generator. `Site(latitude, longitude, altitude, tz, pressure, temperature)` anchors every solver; `_located` lowers it once. `positions(site, times)` returns `Result[tuple[SunSample, ...], SolarFault]` with apparent angles, true zenith, and AU distance, never a provider frame. Rise/set/transit remains the second provider operation required to bound arcs and discs; both operations close on the same rail.
- Cases: `SolarProjection` closes the hemisphere-to-sheet family. `STEREOGRAPHIC`, `EQUIDISTANT`, `ORTHOGRAPHIC`, and equal-area `LAMBERT` map the horizon to `radius`; `GNOMONIC` normalizes its low-altitude clamp to the same boundary. `HourConvention.CLOCK` produces analemmas, while `SOLAR` corrects each sample to hour angle `15·(H-12)`. `SolarFault.admission` accumulates independent site, projection, calendar, and furniture-policy defects; `provider` carries a trapped pvlib/pandas refusal.
- Auto: `day_arc`, `analemma`, `hour_line`, `solstices`, and `furniture` stay on the `Result` spine. Polar `NaT` rise/set stamps fall to a civil-day above-horizon filter. `solstices` sweeps `365` or `366` days and returns March/June/September/December identities, so hemisphere semantics remain a consumer decision. `furniture` sequences date arcs, hour lines, and transit discs only after admission and scales each disc by `1 / distance_au`.
- Growth: a new projection is one `SolarProjection` member plus one `_R` row; a new furniture element one `FurnishingKind` member plus one generator arm; a new chart convention one `FurniturePolicy` value — ring roster, azimuth rays, compass division, hour band, cadence, disc scale all seed data; a new hour semantics one `HourConvention` member plus one `hour_line` arm; a new solver one typed fold over `positions`; site vocabularies (climate, obstruction masks) are upstream data — zero new surface here.
- Boundary: no rendering, styling, or layer projection (`visualization/diagram/draw#DRAW`'s); no coordinate assignment of diagram marks (`visualization/diagram/layout#LAYOUT` composes this owner for its `SUN_PATH` arm); no shading/energy analysis (the geometry/compute tracks read the true `zenith` column); no receipt, no entry, no async — the SPA kernel is numpy-vectorized pure computation the consuming producer offloads inside its own seam; no ladybug (AGPL, process boundary, geometry track); only `pvlib.solarposition` and `pvlib.location` are admitted, never the PV-system/irradiance surface. A hardcoded solstice calendar, a per-timestamp scalar provider call, bare lat/lon threaded past the `Location`, and a generator body legislating a chart convention the policy value owns are the rejected forms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from calendar import isleap
from collections.abc import Callable
from datetime import date, datetime
from zoneinfo import ZoneInfo, ZoneInfoNotFoundError
from enum import StrEnum
from functools import lru_cache
from math import isfinite
from typing import Final, Literal, assert_never

import numpy as np
import pandas as pd
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block
from expression.extra.result import sequence
from msgspec import Struct

from builtins import frozendict
from rasm.artifacts.graphic.vector.path import fragment

lazy from pvlib import solarposition
lazy from pvlib.location import Location

# --- [TYPES] ----------------------------------------------------------------------------
class SolarProjection(StrEnum):
    STEREOGRAPHIC = "stereographic"  # r = R·tan((90-alt)/2) — the drafting sun-path chart
    EQUIDISTANT = "equidistant"  # r = R·(90-alt)/90 — uniform altitude rings
    ORTHOGRAPHIC = "orthographic"  # r = R·cos(alt) — the shadow-plan projection
    LAMBERT = "lambert"  # r = R·√2·sin((90-alt)/2) — equal-area hemisphere normalized at the horizon
    GNOMONIC = "gnomonic"  # r = R·tan(90-alt)/tan(90-floor) — finite chart normalization of shadow rays


class FurnishingKind(StrEnum):
    HORIZON = "horizon"
    ALTITUDE_RING = "altitude_ring"
    AZIMUTH_LINE = "azimuth_line"
    COMPASS_TICK = "compass_tick"
    DATE_ARC = "date_arc"
    HOUR_LINE = "hour_line"
    SUN_DISC = "sun_disc"


class HourConvention(StrEnum):
    CLOCK = "clock"  # analemma figure-eight per clock hour
    SOLAR = "solar"  # smooth local-apparent-time curve via the hour-angle correction


# --- [CONSTANTS] ------------------------------------------------------------------------
_GNOMONIC_FLOOR: Final[float] = 5.0  # gnomonic radius diverges at the horizon; altitude clamps here
_R: Final[frozendict[SolarProjection, Callable[[float], float]]] = frozendict({
    SolarProjection.STEREOGRAPHIC: lambda alt: float(np.tan(np.radians((90.0 - alt) / 2.0))),
    SolarProjection.EQUIDISTANT: lambda alt: (90.0 - alt) / 90.0,
    SolarProjection.ORTHOGRAPHIC: lambda alt: float(np.cos(np.radians(alt))),
    SolarProjection.LAMBERT: lambda alt: float(np.sqrt(2.0) * np.sin(np.radians((90.0 - alt) / 2.0))),
    SolarProjection.GNOMONIC: lambda alt: float(
        np.tan(np.radians(90.0 - max(alt, _GNOMONIC_FLOOR))) / np.tan(np.radians(90.0 - _GNOMONIC_FLOOR))
    ),
})


# --- [MODELS] ---------------------------------------------------------------------------
def _tz_known(tz: str, /) -> bool:
    # zoneinfo resolves against the same IANA database pandas and pvlib read, so an identifier admitted here can
    # never raise a lookup error inside a provider call; the empty string and a traversal-shaped key both refuse.
    try:
        return bool(tz) and ZoneInfo(tz) is not None
    except (ZoneInfoNotFoundError, ValueError):
        return False


class Site(Struct, frozen=True):
    latitude: float
    longitude: float
    altitude: float = 0.0
    tz: str = "UTC"
    pressure: float | None = None  # pascals; None derives from altitude inside the SPA refraction model
    temperature: float = 12.0  # °C; the refraction correction's atmosphere input

    def issues(self, /) -> tuple[str, ...]:
        return (
            (() if isfinite(self.latitude) and -90.0 <= self.latitude <= 90.0 else (f"latitude:{self.latitude}",))
            + (() if isfinite(self.longitude) and -180.0 <= self.longitude <= 180.0 else (f"longitude:{self.longitude}",))
            + (() if isfinite(self.altitude) else (f"altitude:{self.altitude}",))
            + (() if _tz_known(self.tz) else (f"timezone:{self.tz or '<empty>'}",))
            + (() if self.pressure is None or isfinite(self.pressure) and self.pressure > 0.0 else (f"pressure:{self.pressure}",))
            + (() if isfinite(self.temperature) else (f"temperature:{self.temperature}",))
        )


class SunSample(Struct, frozen=True):
    at: datetime
    azimuth: float  # degrees clockwise from north (SPA apparent)
    altitude: float  # apparent elevation, refraction-corrected under the site atmosphere
    zenith_true: float  # true geometric zenith — shading math reads this, never the drawn angle
    distance_au: float = 1.0  # Earth-Sun distance; the apparent sun-disc scale is 1/distance_au


class SeasonDates(Struct, frozen=True):
    march_equinox: date
    june_solstice: date
    september_equinox: date
    december_solstice: date


class FurniturePolicy(Struct, frozen=True):
    # one chart convention as seed data; a climate or office standard is a policy value, never an edited generator.
    rings: tuple[int, ...] = (10, 20, 30, 40, 50, 60, 70, 80)
    azimuths: tuple[int, ...] = ()  # azimuth grid rays in degrees; () = compass ticks only
    compass: int = 16  # tick ring divisions
    hours: tuple[int, int] = (5, 20)  # hour-line band [start, stop)
    convention: HourConvention = HourConvention.CLOCK
    hour_freq: str = "7D"  # across-the-year sampling cadence for hour lines
    step_min: int = 10  # day-arc sampling cadence
    disc: float = 0.0  # apparent sun-disc base radius in sheet units at 1 AU; 0 disables the disc family

    def issues(self, /) -> tuple[str, ...]:
        start, stop = self.hours
        return (
            (() if self.rings == tuple(sorted(set(self.rings))) and all(0 < ring < 90 for ring in self.rings) else ("rings",))
            + (() if len(set(self.azimuths)) == len(self.azimuths) and all(0 <= azimuth < 360 for azimuth in self.azimuths) else ("azimuths",))
            + (() if self.compass >= 4 and self.compass % 4 == 0 else (f"compass:{self.compass}",))
            + (() if 0 <= start < stop <= 24 else (f"hours:{start}:{stop}",))
            + (() if self.hour_freq else ("hour_freq:empty",))
            + (() if 0 < self.step_min <= 1_440 else (f"step_min:{self.step_min}",))
            + (() if isfinite(self.disc) and self.disc >= 0.0 else (f"disc:{self.disc}",))
        )


CHART: Final[FurniturePolicy] = FurniturePolicy()


class Furnishing(Struct, frozen=True):
    kind: FurnishingKind
    fragment: str  # vector-path d-string in sheet coordinates
    label: str = ""
    anchor: tuple[float, float] = (0.0, 0.0)  # label seat


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class SolarFault:
    tag: Literal["admission", "provider"] = tag()
    admission: tuple[str, ...] = case()
    provider: str = case()


# --- [OPERATIONS] -----------------------------------------------------------------------
@lru_cache(maxsize=32)
def _located(site: Site, /) -> "Location":
    # ONE Site -> Location lowering; every provider call forwards through it, no bare lat/lon threading.
    return Location(site.latitude, site.longitude, site.tz, site.altitude)


def positions(site: Site, times: pd.DatetimeIndex, /) -> Result[tuple[SunSample, ...], SolarFault]:
    issues = site.issues() + (() if times.tz is not None else ("times:timezone-naive",))
    if issues:
        return Error(SolarFault(admission=issues))
    try:
        frame = _located(site).get_solarposition(times, pressure=site.pressure, temperature=site.temperature)
        distance = solarposition.nrel_earthsun_distance(times)
        return Ok(tuple(
            SunSample(at=at.to_pydatetime(), azimuth=float(az), altitude=float(el), zenith_true=float(zt), distance_au=float(au))
            for at, az, el, zt, au in zip(frame.index, frame["azimuth"], frame["apparent_elevation"], frame["zenith"], distance, strict=True)
        ))
    except (KeyError, TypeError, ValueError) as bad:
        return Error(SolarFault(provider=f"positions:{bad}"))


def project(azimuth: float, altitude: float, kind: SolarProjection, radius: float, /) -> tuple[float, float]:
    # Hemisphere -> sheet; every normalized projection maps the horizon to `radius` and zenith to the origin.
    r = radius * _R[kind](min(max(altitude, 0.0), 90.0))
    theta = np.radians(azimuth % 360.0)
    return (r * np.sin(theta), -r * np.cos(theta))


def day_arc(
    site: Site, on: date, kind: SolarProjection, radius: float, /, step_min: int = 10
) -> Result[tuple[SunSample, ...], SolarFault]:
    issues = site.issues() + (() if isfinite(radius) and radius > 0.0 else (f"radius:{radius}",)) + (() if 0 < step_min <= 1_440 else (f"step_min:{step_min}",))
    if issues:
        return Error(SolarFault(admission=issues))
    try:
        start = pd.Timestamp(on, tz=site.tz)
        rs = _located(site).get_sun_rise_set_transit(pd.DatetimeIndex([start]), method="spa")
        rise, sets = rs["sunrise"].iloc[0], rs["sunset"].iloc[0]
        samples = (
            pd.date_range(rise, sets, freq=f"{step_min}min")
            if not (pd.isna(rise) or pd.isna(sets))
            else pd.date_range(start, start + pd.Timedelta(days=1), freq=f"{step_min}min")
        )
    except (KeyError, TypeError, ValueError) as bad:
        return Error(SolarFault(provider=f"day_arc:{bad}"))
    return positions(site, samples).map(lambda rows: rows if not (pd.isna(rise) or pd.isna(sets)) else tuple(row for row in rows if row.altitude > 0.0))


def analemma(site: Site, hour: int, year: int, /, freq: str = "7D") -> Result[tuple[SunSample, ...], SolarFault]:
    issues = site.issues() + (() if 0 <= hour < 24 else (f"hour:{hour}",)) + (() if 1 <= year <= 9_999 else (f"year:{year}",)) + (() if freq else ("freq:empty",))
    if issues:
        return Error(SolarFault(admission=issues))
    try:
        days = pd.date_range(f"{year}-01-01 {hour:02d}:00", f"{year}-12-31 {hour:02d}:00", freq=freq, tz=site.tz)
    except (KeyError, TypeError, ValueError) as bad:
        return Error(SolarFault(provider=f"analemma:{bad}"))
    return positions(site, days).map(lambda rows: tuple(row for row in rows if row.altitude > 0.0))


def hour_line(
    site: Site, hour: int, year: int, convention: HourConvention, /, freq: str = "7D"
) -> Result[tuple[SunSample, ...], SolarFault]:
    match convention:
        case HourConvention.CLOCK:
            return analemma(site, hour, year, freq)
        case HourConvention.SOLAR:
            issues = site.issues() + (() if 0 <= hour < 24 else (f"hour:{hour}",)) + (() if 1 <= year <= 9_999 else (f"year:{year}",)) + (() if freq else ("freq:empty",))
            if issues:
                return Error(SolarFault(admission=issues))
            try:
                clock = pd.date_range(f"{year}-01-01 {hour:02d}:00", f"{year}-12-31 {hour:02d}:00", freq=freq, tz=site.tz)
                eot = solarposition.equation_of_time_spencer71(clock.dayofyear)
                actual = solarposition.hour_angle(clock, site.longitude, eot)
                corrected = clock + pd.to_timedelta((15.0 * (hour - 12) - actual) / 15.0, unit="h")
            except (KeyError, TypeError, ValueError) as bad:
                return Error(SolarFault(provider=f"hour_line:{bad}"))
            return positions(site, pd.DatetimeIndex(corrected)).map(lambda rows: tuple(row for row in rows if row.altitude > 0.0))
        case _ as unreachable:
            assert_never(unreachable)


def solstices(year: int, /) -> Result[SeasonDates, SolarFault]:
    if not 1 <= year <= 9_999:
        return Error(SolarFault(admission=(f"year:{year}",)))
    doy = np.arange(1, 367 if isleap(year) else 366)
    decl = solarposition.declination_spencer71(doy)
    base = date(year, 1, 1).toordinal() - 1
    crossings = np.where(np.diff(np.sign(decl)) != 0)[0] + 1
    return Ok(SeasonDates(
        march_equinox=date.fromordinal(base + int(doy[crossings[0]])),
        june_solstice=date.fromordinal(base + int(doy[np.argmax(decl)])),
        september_equinox=date.fromordinal(base + int(doy[crossings[-1]])),
        december_solstice=date.fromordinal(base + int(doy[np.argmin(decl)])),
    ))


def furniture(
    site: Site, kind: SolarProjection, radius: float, year: int, /, policy: FurniturePolicy = CHART
) -> Result[tuple[Furnishing, ...], SolarFault]:
    issues = site.issues() + policy.issues() + (() if isfinite(radius) and radius > 0.0 else (f"radius:{radius}",))
    return Error(SolarFault(admission=issues)) if issues else solstices(year).bind(lambda seasons: _furnished(site, kind, radius, year, policy, seasons))


def _furnished(
    site: Site, kind: SolarProjection, radius: float, year: int, policy: FurniturePolicy, seasons: SeasonDates, /
) -> Result[tuple[Furnishing, ...], SolarFault]:
    dates = (
        (seasons.june_solstice, seasons.june_solstice.strftime("%b %d").upper()),
        (seasons.march_equinox, f"{seasons.march_equinox.strftime('%b %d').upper()} / {seasons.september_equinox.strftime('%b %d').upper()}"),
        (seasons.december_solstice, seasons.december_solstice.strftime("%b %d").upper()),
    )
    # every labeled furnishing carries its geometry-derived seat: ring labels sit on their own ring's north
    # crossing, ray and cardinal labels on the horizon at their azimuth, arc and hour labels on their first
    # sample — the layout owner forwards these anchors verbatim and reconstructs no solar geometry.
    rings = tuple(
        Furnishing(FurnishingKind.ALTITUDE_RING, _circle(ring_r), f"{alt}°", anchor=(0.0, -ring_r))
        for alt in policy.rings
        for ring_r in (abs(project(180.0, alt, kind, radius)[1]),)
    )
    rays = tuple(Furnishing(FurnishingKind.AZIMUTH_LINE, _ray(az, kind, radius), f"{az}°", anchor=project(az, 0.0, kind, radius)) for az in policy.azimuths)
    cardinal_step = policy.compass // 4
    ticks = tuple(
        Furnishing(
            FurnishingKind.COMPASS_TICK,
            _tick(azimuth, radius),
            "NESW"[i // cardinal_step] if i % cardinal_step == 0 else "",
            anchor=project(azimuth, 0.0, SolarProjection.EQUIDISTANT, radius),
        )
        for i in range(policy.compass)
        for azimuth in (i * 360.0 / policy.compass,)
    )
    arcs = sequence(Block.of_seq(day_arc(site, on, kind, radius, policy.step_min) for on, _ in dates))
    hours = sequence(Block.of_seq(hour_line(site, hour, year, policy.convention, policy.hour_freq) for hour in range(*policy.hours)))
    discs = _discs(site, tuple(on for on, _ in dates), kind, radius, policy) if policy.disc > 0.0 else Ok(())
    return arcs.bind(lambda arc_rows: hours.bind(lambda hour_rows: discs.map(lambda disc_rows: (
        Furnishing(FurnishingKind.HORIZON, _circle(radius)),
        *rings,
        *rays,
        *ticks,
        *(
            Furnishing(FurnishingKind.DATE_ARC, _polyline(rows, kind, radius), label, anchor=_head(rows, kind, radius))
            for rows, (_, label) in zip(arc_rows, dates, strict=True)
            if rows  # a sampleless arc (polar night) draws nothing — skipping keeps its label off the origin
        ),
        *(
            Furnishing(FurnishingKind.HOUR_LINE, _polyline(rows, kind, radius), f"{hour:02d}", anchor=_head(rows, kind, radius))
            for rows, hour in zip(hour_rows, range(*policy.hours), strict=True)
            if rows  # an hour never above the horizon yields no line, so no origin-anchored label either
        ),
        *disc_rows,
    ))))


def _discs(
    site: Site, dates: tuple[date, ...], kind: SolarProjection, radius: float, policy: FurniturePolicy, /
) -> Result[tuple[Furnishing, ...], SolarFault]:
    try:
        stamps = _located(site).get_sun_rise_set_transit(pd.DatetimeIndex([pd.Timestamp(on, tz=site.tz) for on in dates]), method="spa")
        transits = pd.DatetimeIndex([stamp for stamp in stamps["transit"] if not pd.isna(stamp)])
    except (KeyError, TypeError, ValueError) as bad:
        return Error(SolarFault(provider=f"discs:{bad}"))
    return (
        Ok(())
        if len(transits) == 0
        else positions(site, transits).map(lambda rows: tuple(
            Furnishing(FurnishingKind.SUN_DISC, _disc(project(row.azimuth, row.altitude, kind, radius), policy.disc / row.distance_au))
            for row in rows
            # a below-horizon transit (polar night) stays undrawn: project() clamps a negative altitude onto the
            # horizon ring, so an unfiltered row would mint a phantom horizon disc — the same skip law the
            # sampleless date-arc and hour-line rows already carry.
            if row.altitude > 0.0
        ))
    )


def _circle(radius: float, /) -> str:
    return fragment(f"M {radius} 0 A {radius} {radius} 0 1 1 {-radius} 0 A {radius} {radius} 0 1 1 {radius} 0 Z")


def _disc(center: tuple[float, float], radius: float, /) -> str:
    cx, cy = center
    return fragment(f"M {cx + radius} {cy} A {radius} {radius} 0 1 1 {cx - radius} {cy} A {radius} {radius} 0 1 1 {cx + radius} {cy} Z")


def _ray(azimuth: float, kind: SolarProjection, radius: float, /) -> str:
    (x0, y0), (x1, y1) = project(azimuth, 90.0, kind, radius), project(azimuth, 0.0, kind, radius)
    return fragment(f"M {x0:.2f} {y0:.2f} L {x1:.2f} {y1:.2f}")


def _tick(azimuth: float, radius: float, /) -> str:
    (x0, y0), (x1, y1) = (project(azimuth, 0.0, SolarProjection.EQUIDISTANT, radius * 0.97),
                          project(azimuth, 0.0, SolarProjection.EQUIDISTANT, radius))
    return fragment(f"M {x0} {y0} L {x1} {y1}")


def _polyline(samples: tuple[SunSample, ...], kind: SolarProjection, radius: float, /) -> str:
    pts = tuple(project(s.azimuth, s.altitude, kind, radius) for s in samples)
    return fragment("M " + " L ".join(f"{x:.2f} {y:.2f}" for x, y in pts)) if pts else ""


def _head(samples: tuple[SunSample, ...], kind: SolarProjection, radius: float, /) -> tuple[float, float]:
    # a labeled polyline's seat is its first sample; an empty run keeps the origin default the caller never labels.
    return project(samples[0].azimuth, samples[0].altitude, kind, radius) if samples else (0.0, 0.0)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "CHART",
    "Furnishing",
    "FurnishingKind",
    "FurniturePolicy",
    "HourConvention",
    "SeasonDates",
    "Site",
    "SolarFault",
    "SolarProjection",
    "SunSample",
    "analemma",
    "day_arc",
    "furniture",
    "hour_line",
    "positions",
    "project",
    "solstices",
]
```
