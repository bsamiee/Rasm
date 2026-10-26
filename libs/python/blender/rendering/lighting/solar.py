"""NOAA sun positions, solar days, sun paths, and studies."""

import calendar
import datetime
from enum import auto, Enum
import functools
import math
from typing import Annotated, ClassVar, Final, Self
import zoneinfo

from expression import Error, Ok
import msgspec
import numpy as np
import numpy.typing as npt

from blender.records.fields import Case, Elevation, Turn
from blender.rendering.faults import RenderingResult, RenderingTimeFault

# --- [TYPES] ----------------------------------------------------------------------------

type SunStudy = DayStudy | SeasonStudy


class Polar(Enum):
    """Day sun crosses no horizon."""

    DAY = auto()
    NIGHT = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

DAY_SECONDS: Final = datetime.timedelta(days=1).total_seconds()


# --- [MODELS] ---------------------------------------------------------------------------


class Site(msgspec.Struct, frozen=True, kw_only=True):
    """Place on Earth with clock time zone."""

    latitude: Annotated[Elevation, msgspec.Meta(description="Angle north of equator")] = 0.0
    longitude: Annotated[Turn, msgspec.Meta(description="Angle east of Greenwich")] = 0.0
    north: Annotated[Turn, msgspec.Meta(description="True north counterclockwise from +Y")] = 0.0
    zone: Annotated[str, msgspec.Meta(title="Time Zone", description="IANA time zone of site clock")] = "UTC"

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


class LocalTime(msgspec.Struct, frozen=True, kw_only=True):
    """Calendar date and time on site clock."""

    year: Annotated[int, msgspec.Meta(ge=datetime.MINYEAR, le=datetime.MAXYEAR)] = 2000
    month: Annotated[int, msgspec.Meta(ge=1, le=12)] = 1
    day: Annotated[int, msgspec.Meta(ge=1, le=31, description="Day of month, rolling into next month")] = 1
    hour: Annotated[int, msgspec.Meta(ge=0, le=23, description="Hour on site clock")] = 12
    minute: Annotated[int, msgspec.Meta(ge=0, le=59)] = 0

    @property
    def date(self) -> datetime.date:
        """Calendar date, days past month end rolling forward."""
        return datetime.date(self.year, self.month, 1) + datetime.timedelta(days=self.day - 1)

    def moment(self, zone: zoneinfo.ZoneInfo) -> datetime.datetime:
        """Returns aware wall time in zone."""
        return datetime.datetime.combine(self.date, datetime.time(self.hour, self.minute), tzinfo=zone)


class SunPosition(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """True azimuth and geometric elevation in radians."""

    azimuth: npt.NDArray[np.float64]
    elevation: npt.NDArray[np.float64]
    north: float

    # --- [COORDINATES]

    @property
    def rotation(self) -> npt.NDArray[np.float64]:
        """Clockwise angle from scene +Y for sky and lamp."""
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
        """Returns NOAA declination and hour angle."""
        days = seconds / DAY_SECONDS
        t = (days - 10957.5) / 36525.0
        mean = np.radians(np.mod(280.46646 + t * (36000.76983 + t * 0.0003032), 360.0))
        anomaly = np.radians(357.52911 + t * (35999.05029 - t * 0.0001537))
        anomaly_sine, double_anomaly_sine = np.sin(anomaly), np.sin(2.0 * anomaly)
        eccentricity = 0.016708634 - t * (0.000042037 + t * 0.0000001267)
        omega = np.radians(125.04 - 1934.136 * t)
        apparent = mean + np.radians(
            anomaly_sine * (1.914602 - t * (0.004817 + t * 0.000014)) + double_anomaly_sine * (0.019993 - t * 0.000101) + np.sin(3.0 * anomaly) * 0.000289 - 0.00569 - 0.00478 * np.sin(omega)
        )
        obliquity = np.radians(23.0 + (26.0 + (21.448 - t * (46.815 + t * (0.00059 - t * 0.001813))) / 60.0) / 60.0 + 0.00256 * np.cos(omega))
        y = np.tan(obliquity / 2.0) ** 2
        equation = (
            y * np.sin(2.0 * mean)
            - 2.0 * eccentricity * anomaly_sine
            + 4.0 * eccentricity * y * anomaly_sine * np.cos(2.0 * mean)
            - 0.5 * y**2 * np.sin(4.0 * mean)
            - 1.25 * eccentricity**2 * double_anomaly_sine
        )
        return np.arcsin(np.sin(obliquity) * np.sin(apparent)), math.tau * np.mod(days, 1.0) - math.pi + longitude + equation

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
        """Adds NOAA atmospheric refraction to elevation."""
        degrees = np.degrees(self.elevation)
        band = np.digitize(degrees, (-0.575, 5.0, 85.0), right=True)
        arcseconds = np.piecewise(
            degrees,
            [band == index for index in range(3)],
            [
                lambda e: -20.774 / np.tan(np.radians(e)),
                lambda e: 1735.0 + e * (-518.2 + e * (103.4 + e * (-12.79 + e * 0.711))),
                lambda e: (cot := 1.0 / np.tan(np.radians(e))) * (58.1 + cot**2 * (-0.07 + cot**2 * 0.000086)),
                0.0,
            ],
        )
        return msgspec.structs.replace(self, elevation=self.elevation + np.radians(arcseconds / 3600.0))


class Daylight(msgspec.Struct, frozen=True, kw_only=True):
    """Sunrise and sunset."""

    sunrise: datetime.datetime
    sunset: datetime.datetime


class SolarDay(msgspec.Struct, frozen=True, kw_only=True):
    """Solar noon with daylight or polar case."""

    noon: datetime.datetime
    light: Daylight | Polar

    @classmethod
    def of(cls, site: Site, date: datetime.date, zone: zoneinfo.ZoneInfo) -> RenderingResult[Self]:
        """Solves solar noon and horizon crossings, time fault on overflow."""
        side = np.array([0.0, -1.0, 1.0], dtype=np.float64)
        latitude_sine, latitude_cosine = math.sin(site.latitude), math.cos(site.latitude)
        horizon = math.cos(math.radians(90.833))

        def reach(seconds: npt.NDArray[np.float64]) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
            declination, hour = SunPosition.equatorial(site.longitude, seconds)
            return hour, (horizon - latitude_sine * np.sin(declination)) / (latitude_cosine * np.cos(declination))

        def step(seconds: npt.NDArray[np.float64], _: int) -> npt.NDArray[np.float64]:
            hour, ratio = reach(seconds)
            offset = hour - side * np.arccos(np.clip(ratio, -1.0, 1.0))
            return seconds - np.arctan2(np.sin(offset), np.cos(offset)) * DAY_SECONDS / math.tau

        solved = functools.reduce(step, range(3), np.full(side.shape, datetime.datetime.combine(date, datetime.time(12), tzinfo=zone).timestamp(), dtype=np.float64))
        epoch = datetime.datetime(1970, 1, 1, tzinfo=datetime.UTC)
        _, ratios = reach(solved)
        ratio = float(ratios[0])
        try:
            noon, *crossings = ((epoch + datetime.timedelta(seconds=float(moment))).astimezone(zone) for moment in solved[: 3 if -1.0 < ratio < 1.0 else 1])
        except OverflowError as error:
            return Error((RenderingTimeFault(request=date, error=error),))
        match crossings:
            case [sunrise, sunset]:
                return Ok(cls(noon=noon, light=Daylight(sunrise=sunrise, sunset=sunset)))
            case _:
                return Ok(cls(noon=noon, light=Polar.DAY if ratio <= -1.0 else Polar.NIGHT))


class SunPaths(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Hourly analemmas and yearly sky band as scene unit vectors."""

    spacing: ClassVar[float] = math.radians(1.0)
    analemmas: npt.NDArray[np.float64]
    surface: npt.NDArray[np.float64]

    @property
    def faces(self) -> npt.NDArray[np.int64]:
        """Sky band quads as rows of four corner indices."""
        grid = np.arange(math.prod(self.surface.shape[:2]), dtype=np.int64).reshape(self.surface.shape[:2])
        return np.stack((grid[:-1, :-1], grid[:-1, 1:], grid[1:, 1:], grid[1:, :-1]), axis=-1).reshape(-1, 4)

    @classmethod
    def of(cls, site: Site, year: int) -> Self:
        """Returns (24, days, 3) mean-time analemmas and (rows, columns, 3) sky band."""
        seconds = (
            datetime.datetime(year, 1, 1, tzinfo=datetime.UTC).timestamp()
            + datetime.timedelta(hours=1).total_seconds() * np.arange(24, dtype=np.float64)[:, None]
            + DAY_SECONDS * np.arange(365 + calendar.isleap(year), dtype=np.float64)
            - site.longitude * DAY_SECONDS / math.tau
        )
        declination, hour = SunPosition.equatorial(site.longitude, seconds)
        low = max(float(declination.min()), site.latitude - math.pi / 2.0)
        high = min(float(declination.max()), site.latitude + math.pi / 2.0)
        rows = np.linspace(low, high, math.ceil((high - low) / cls.spacing) + 1)[:, None]
        half_day = np.arccos(np.clip(-math.tan(site.latitude) * np.tan(rows), -1.0, 1.0))
        return cls(analemmas=SunPosition.of(site, declination, hour).vectors, surface=SunPosition.of(site, rows, half_day * np.linspace(-1.0, 1.0, round(math.tau / cls.spacing) + 1)).vectors)


class DayStudy(Case, frozen=True, kw_only=True):
    """Moments stepped by elapsed minutes."""

    title = "Day"
    start: Annotated[LocalTime, msgspec.Meta(description="Date and time of first frame")] = LocalTime(hour=6)
    minutes: Annotated[int, msgspec.Meta(ge=1, le=1440, title="Step", description="Minutes between frames")] = 30
    frames: Annotated[int, msgspec.Meta(ge=1, description="Frames the study keys")] = 25


class SeasonStudy(Case, frozen=True, kw_only=True):
    """Moments stepped by days at one clock time."""

    title = "Season"
    start: Annotated[LocalTime, msgspec.Meta(description="First date and clock time of every frame")] = LocalTime(hour=6)
    days: Annotated[int, msgspec.Meta(ge=1, le=366, title="Step", description="Days between frames")] = 30
    frames: Annotated[int, msgspec.Meta(ge=1, description="Frames the study keys")] = 25


# --- [OPERATIONS] -----------------------------------------------------------------------


def moments(study: SunStudy, zone: zoneinfo.ZoneInfo) -> RenderingResult[tuple[datetime.datetime, ...]]:
    """Returns study moments, time fault on overflow."""
    first = study.start.moment(zone)
    match study:
        case DayStudy(minutes=minutes):
            step = datetime.timedelta(minutes=minutes)
        case SeasonStudy(days=days):
            step = datetime.timedelta(days=days)
    try:
        origin = first.astimezone(datetime.UTC) if isinstance(study, DayStudy) else first
        return Ok(tuple((origin + frame * step).astimezone(zone) for frame in range(study.frames)))
    except OverflowError as error:
        return Error((RenderingTimeFault(request=(first, step, study.frames), error=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DayStudy", "Daylight", "LocalTime", "Polar", "SeasonStudy", "Site", "SolarDay", "SunPaths", "SunPosition", "SunStudy", "moments"]
