"""NREL sun positions, solar days, sun paths, and studies."""

import calendar
import datetime
from enum import auto, Enum
import functools
import math
from typing import Annotated, ClassVar, Final, Self
import zoneinfo

from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
from pvlib import spa  # type: ignore[import-untyped]

from blender.records.faults import Fault, Outcome, Reason
from blender.records.fields import Bearing, Count, Frozen, Inclination

# --- [TYPES] ----------------------------------------------------------------------------


class Polar(Enum):
    """Day sun crosses no horizon."""

    DAY = auto()
    NIGHT = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_DAY: Final = datetime.timedelta(days=1)
_DAY_SECONDS: Final = _DAY.total_seconds()

# --- [MODELS] ---------------------------------------------------------------------------


class Site(msgspec.Struct, frozen=True, kw_only=True):
    """Place on Earth with clock time zone."""

    latitude: Inclination = 0.0
    longitude: Bearing = 0.0
    north: Bearing = 0.0
    zone: str = "UTC"

    def __post_init__(self) -> None:
        """Validates zone as a key the time zone database holds."""
        try:
            zoneinfo.ZoneInfo(self.zone)
        except zoneinfo.ZoneInfoNotFoundError as error:
            raise ValueError(*error.args) from error

    @property
    def clock(self) -> zoneinfo.ZoneInfo:
        """Time zone of site clock."""
        return zoneinfo.ZoneInfo(self.zone)


class SunPosition(Frozen, frozen=True, kw_only=True, eq=False):
    """True azimuth and geometric elevation in radians."""

    # --- [STATE]

    azimuth: npt.NDArray[np.float64]
    elevation: npt.NDArray[np.float64]
    north: float

    # --- [COORDINATES]

    @property
    def rotation(self) -> npt.NDArray[np.float64]:
        """Clockwise angle from scene +Y for sky and sun light."""
        return self.azimuth - self.north

    @property
    def vectors(self) -> npt.NDArray[np.float64]:
        """Unit vectors toward sun in scene axes, shaped (..., 3), +Z up."""
        return SunPosition.toward(self.rotation, self.elevation)

    @staticmethod
    def toward(rotation: npt.NDArray[np.float64], elevation: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Returns unit vectors at clockwise rotation from scene +Y and elevation, shaped (..., 3), +Z up."""
        horizontal = np.cos(elevation)
        return np.stack((np.sin(rotation) * horizontal, np.cos(rotation) * horizontal, np.sin(elevation)), axis=-1)

    @staticmethod
    def equatorial(longitude: float, seconds: npt.NDArray[np.float64]) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
        """Returns NREL declination and hour angle in the input shape."""
        sidereal, ascension, declination = spa.solar_position(seconds.ravel(), 0.0, 0.0, 0.0, 0.0, 0.0, delta_t=0.0, atmos_refract=0.0, sst=True)
        return np.radians(declination).reshape(seconds.shape), np.radians(spa.local_hour_angle(sidereal, math.degrees(longitude), ascension)).reshape(seconds.shape)

    @classmethod
    def of(cls, site: Site, declination: npt.NDArray[np.float64], hour: npt.NDArray[np.float64]) -> Self:
        """Returns pole-safe positions, azimuth unwrapped along time axis."""
        sine, cosine = np.sin(declination), np.cos(declination)
        latitude_sine, latitude_cosine = math.sin(site.latitude), math.cos(site.latitude)
        meridian = cosine * np.cos(hour)
        east = -cosine * np.sin(hour)
        north = latitude_cosine * sine - latitude_sine * meridian
        up = latitude_sine * sine + latitude_cosine * meridian
        return cls(azimuth=np.unwrap(np.arctan2(east, north), axis=-1), elevation=np.arctan2(up, np.hypot(east, north)), north=site.north)

    @classmethod
    def at(cls, site: Site, seconds: npt.NDArray[np.float64]) -> Self:
        """Returns positions shaped as POSIX seconds, time on last axis."""
        return cls.of(site, *cls.equatorial(site.longitude, seconds))

    # --- [REFRACTION]

    def apparent(self) -> Self:
        """Adds NREL standard-atmosphere refraction above the apparent horizon."""
        degrees = np.degrees(self.elevation)
        refraction = 0.5667
        correction = np.piecewise(degrees, [degrees >= -(0.26667 + refraction)], [functools.partial(spa.atmospheric_refraction_correction, 1013.25, 12.0, atmos_refract=refraction), 0.0])
        return msgspec.structs.replace(self, elevation=self.elevation + np.radians(correction))


class Daylight(msgspec.Struct, frozen=True, kw_only=True):
    """Sunrise and sunset."""

    sunrise: datetime.datetime
    sunset: datetime.datetime


class SolarDay(msgspec.Struct, frozen=True, kw_only=True):
    """Solar noon with daylight or polar case."""

    noon: datetime.datetime
    light: Daylight | Polar

    @classmethod
    def of(cls, site: Site, date: datetime.date) -> Outcome[Self]:
        """Solves solar noon and horizon crossings, `FAILED` fault on overflow."""
        zone = site.clock
        middle = datetime.datetime.combine(date, datetime.time(12), tzinfo=zone).timestamp()
        days = (math.floor(middle / _DAY_SECONDS) + np.arange(1, -2, -1, dtype=np.float64)) * _DAY_SECONDS
        transit, sunrise, sunset = spa.transit_sunrise_sunset(days, math.degrees(site.latitude), math.degrees(site.longitude), delta_t=0.0, numthreads=1)
        index = int(np.abs(transit - middle).argmin())
        noon_stamp, sunrise_stamp, sunset_stamp = (float(values[index]) for values in (transit, sunrise, sunset))
        epoch = datetime.datetime(1970, 1, 1, tzinfo=datetime.UTC)

        return catch(
            lambda: cls(
                noon=(epoch + datetime.timedelta(seconds=noon_stamp)).astimezone(zone),
                light=(
                    (Polar.DAY if float(SunPosition.at(site, transit[index : index + 1]).elevation[0]) > math.radians(-0.8333) else Polar.NIGHT)
                    if math.isnan(sunrise_stamp)
                    else Daylight(sunrise=(epoch + datetime.timedelta(seconds=sunrise_stamp)).astimezone(zone), sunset=(epoch + datetime.timedelta(seconds=sunset_stamp)).astimezone(zone))
                ),
            ),
            exception=OverflowError,
        )().map_error(functools.partial(Fault.of, Reason.FAILED, str(date)))


class SunPaths(Frozen, frozen=True, kw_only=True, eq=False):
    """Hourly analemmas and yearly sky band as scene unit vectors."""

    spacing: ClassVar[float] = math.radians(1.0)
    analemmas: npt.NDArray[np.float64]
    sky_band: npt.NDArray[np.float64]

    @property
    def faces(self) -> npt.NDArray[np.int64]:
        """Sky band quads as rows of four corner indices."""
        grid = np.arange(math.prod(self.sky_band.shape[:2]), dtype=np.int64).reshape(self.sky_band.shape[:2])
        return np.stack((grid[:-1, :-1], grid[:-1, 1:], grid[1:, 1:], grid[1:, :-1]), axis=-1).reshape(-1, 4)

    @classmethod
    def of(cls, site: Site, year: int) -> Self:
        """Returns (24, days, 3) mean-time analemmas and (rows, columns, 3) sky band."""
        seconds = (
            datetime.datetime(year, 1, 1, tzinfo=datetime.UTC).timestamp()
            + datetime.timedelta(hours=1).total_seconds() * np.arange(24, dtype=np.float64)[:, None]
            + _DAY_SECONDS * np.arange(365 + calendar.isleap(year), dtype=np.float64)
            - site.longitude * _DAY_SECONDS / math.tau
        )
        declination, hour = SunPosition.equatorial(site.longitude, seconds)
        low = max(float(declination.min()), site.latitude - math.pi / 2.0)
        high = min(float(declination.max()), site.latitude + math.pi / 2.0)
        rows = np.linspace(low, high, math.ceil((high - low) / cls.spacing) + 1)[:, None]
        half_day = np.arccos(np.clip(-math.tan(site.latitude) * np.tan(rows), -1.0, 1.0))
        return cls(analemmas=SunPosition.of(site, declination, hour).vectors, sky_band=SunPosition.of(site, rows, half_day * np.linspace(-1.0, 1.0, round(math.tau / cls.spacing) + 1)).vectors)

    @property
    def arcs(self) -> tuple[tuple[npt.NDArray[np.float64], bool], ...]:
        """Above-horizon runs of each analemma with their cyclic flag, a loop never setting whole and cyclic, else its open pieces of more than one point."""

        def split(loop: npt.NDArray[np.float64]) -> tuple[tuple[npt.NDArray[np.float64], bool], ...]:
            above = loop[:, 2] >= 0.0
            rolled = np.roll(loop, -int(np.argmin(above)), axis=0)
            pieces = np.split(rolled, np.flatnonzero(np.diff(rolled[:, 2] >= 0.0)) + 1)
            return ((loop, True),) if above.all() else tuple((piece, False) for piece in pieces if piece[0, 2] >= 0.0 and len(piece) > 1)

        return tuple(arc for loop in self.analemmas for arc in split(loop))


class Study(msgspec.Struct, frozen=True, kw_only=True):
    """Whole-day clock steps and other elapsed steps from a naive local start."""

    start: Annotated[datetime.datetime, msgspec.Meta(tz=False)]
    step: datetime.timedelta
    frames: Count

    def moments(self, zone: zoneinfo.ZoneInfo) -> Outcome[tuple[datetime.datetime, ...]]:
        """Returns study moments on the zone clock, `FAILED` fault on overflow."""
        first = self.start.replace(tzinfo=zone)

        @catch(exception=OverflowError)
        def stepped() -> tuple[datetime.datetime, ...]:
            origin = first.astimezone(datetime.UTC) if self.step % _DAY else first
            return tuple((origin + frame * self.step).astimezone(zone) for frame in range(self.frames))

        return stepped().map_error(functools.partial(Fault.of, Reason.FAILED, f"{first} + {self.frames} x {self.step}"))

    def positions(self, site: Site, /, *, refraction: bool) -> Outcome[SunPosition]:
        """Returns the sun position at each study moment on the site clock, apparent with `refraction`, or `FAILED` fault."""
        return self.moments(site.clock).map(lambda moments: SunPosition.at(site, np.array([moment.timestamp() for moment in moments]))).map(lambda found: found.apparent() if refraction else found)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Daylight", "Polar", "Site", "SolarDay", "Study", "SunPaths", "SunPosition"]
