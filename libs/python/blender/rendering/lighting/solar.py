"""NOAA sun positions, solar days, sun paths, and sun studies of a site."""

import calendar
import datetime
from enum import auto, Enum
import functools
import math
from typing import Annotated, ClassVar, Final, Self
import zoneinfo

from expression import Error, Ok, Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.imaging.parameters import Case, Presentation, Quantity
from blender.rendering.faults import RenderingResult, RenderingTimeFault

# --- [TYPES] ----------------------------------------------------------------------------

type SunStudy = DayStudy | SeasonStudy


class Polar(Enum):
    """Day on which the sun crosses no horizon."""

    DAY = auto()
    NIGHT = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

DAY_SECONDS: Final = datetime.timedelta(days=1).total_seconds()


# --- [MODELS] ---------------------------------------------------------------------------


class Site(msgspec.Struct, frozen=True, kw_only=True):
    """Place on Earth with the time zone of its clock."""

    latitude: Annotated[float, msgspec.Meta(ge=-math.pi / 2.0, le=math.pi / 2.0, description="Angle north of the equator"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    longitude: Annotated[float, msgspec.Meta(ge=-math.pi, le=math.pi, description="Angle east of Greenwich"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    north: Annotated[float, msgspec.Meta(ge=-math.pi, le=math.pi, description="True north counterclockwise from +Y"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    zone: Annotated[str, msgspec.Meta(title="Time Zone", description="IANA time zone of the site's clock")] = "UTC"


class LocalTime(msgspec.Struct, frozen=True, kw_only=True):
    """Calendar date and clock time on a site's clock."""

    year: Annotated[int, msgspec.Meta(ge=datetime.MINYEAR, le=datetime.MAXYEAR, description="Calendar year")] = 2000
    month: Annotated[int, msgspec.Meta(ge=1, le=12, description="Month of the year")] = 1
    day: Annotated[int, msgspec.Meta(ge=1, le=31, description="Day of the month, continuing into the next month past its end")] = 1
    hour: Annotated[int, msgspec.Meta(ge=0, le=23, description="Hour on the site's clock")] = 12
    minute: Annotated[int, msgspec.Meta(ge=0, le=59, description="Minute of the hour")] = 0

    @property
    def date(self) -> datetime.date:
        """Calendar date the fields state."""
        return datetime.date(self.year, self.month, 1) + datetime.timedelta(days=self.day - 1)

    def moment(self, zone: zoneinfo.ZoneInfo) -> datetime.datetime:
        """Forms the aware wall time on the admitted site's clock."""
        return datetime.datetime.combine(self.date, datetime.time(self.hour, self.minute), tzinfo=zone)


class SunPosition(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """True azimuth and geometric elevation at the site's moments, in radians."""

    azimuth: npt.NDArray[np.float64]
    elevation: npt.NDArray[np.float64]
    north: float

    # --- [COORDINATES]

    @property
    def rotation(self) -> npt.NDArray[np.float64]:
        """Clockwise angle from scene +Y for the sky and lamp."""
        return self.azimuth - self.north

    @property
    def vectors(self) -> npt.NDArray[np.float64]:
        """Unit vectors toward the sun in scene axes, shaped (..., 3), +Z up."""
        rotation, horizontal = self.rotation, np.cos(self.elevation)
        return np.stack((np.sin(rotation) * horizontal, np.cos(rotation) * horizontal, np.sin(self.elevation)), axis=-1)

    @staticmethod
    def equatorial(longitude: float, seconds: npt.NDArray[np.float64]) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
        """Returns NOAA declination and hour angle for POSIX seconds and east longitude."""
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
        """Forms pole-safe positions from declination and hour angle, unwrapping time last."""
        sine, cosine = np.sin(declination), np.cos(declination)
        latitude_sine, latitude_cosine = math.sin(site.latitude), math.cos(site.latitude)
        meridian = cosine * np.cos(hour)
        east = -cosine * np.sin(hour)
        north = latitude_cosine * sine - latitude_sine * meridian
        up = latitude_sine * sine + latitude_cosine * meridian
        return cls(azimuth=np.unwrap(np.arctan2(east, north), axis=-1), elevation=np.arctan2(up, np.hypot(east, north)), north=site.north)

    @classmethod
    def at(cls, site: Site, seconds: npt.NDArray[np.float64]) -> Self:
        """Returns positions shaped as the POSIX seconds, with time on the last axis."""
        return cls.of(site, *cls.equatorial(site.longitude, seconds))

    # --- [REFRACTION]

    def apparent(self) -> Self:
        """Returns the position with NOAA atmospheric refraction added to elevation."""
        degrees = np.degrees(self.elevation)
        band = np.digitize(degrees, (-0.575, 5.0, 85.0), right=True)
        return msgspec.structs.replace(
            self,
            elevation=self.elevation
            + np.radians(
                np.piecewise(
                    degrees,
                    [band == index for index in range(3)],
                    [
                        lambda e: -20.774 / np.tan(np.radians(e)),
                        lambda e: 1735.0 + e * (-518.2 + e * (103.4 + e * (-12.79 + e * 0.711))),
                        lambda e: (cot := 1.0 / np.tan(np.radians(e))) * (58.1 + cot**2 * (-0.07 + cot**2 * 0.000086)),
                        0.0,
                    ],
                )
                / 3600.0
            ),
        )


class Daylight(msgspec.Struct, frozen=True, kw_only=True):
    """Sunrise and sunset of a day the sun crosses the horizon."""

    sunrise: datetime.datetime
    sunset: datetime.datetime


class SolarDay(msgspec.Struct, frozen=True, kw_only=True):
    """Solar noon of a local date with its daylight or polar case."""

    noon: datetime.datetime
    light: Daylight | Polar

    @classmethod
    def of(cls, site: Site, date: datetime.date, zone: zoneinfo.ZoneInfo) -> RenderingResult[Self]:
        """Returns local solar noon and horizon crossings, or native calendar overflow."""
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

        try:
            initial = datetime.datetime.combine(date, datetime.time(12), tzinfo=zone).timestamp()
            solved = functools.reduce(step, range(3), np.full(side.shape, initial, dtype=np.float64))
            epoch = datetime.datetime(1970, 1, 1, tzinfo=datetime.UTC)
            noon = (epoch + datetime.timedelta(seconds=float(solved[0]))).astimezone(zone)
            _, ratios = reach(solved)
            ratio = float(ratios[0])
            if -1.0 < ratio < 1.0:
                sunrise, sunset = ((epoch + datetime.timedelta(seconds=float(moment))).astimezone(zone) for moment in solved[1:])
                return Ok(cls(noon=noon, light=Daylight(sunrise=sunrise, sunset=sunset)))
            return Ok(cls(noon=noon, light=Polar.DAY if ratio <= -1.0 else Polar.NIGHT))
        except OverflowError as error:
            return Error((RenderingTimeFault(request=date, error=error),))


class SunPaths(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Hourly analemmas and the year's above-horizon sky band in scene unit vectors."""

    spacing: ClassVar[float] = math.radians(1.0)
    analemmas: npt.NDArray[np.float64]
    surface: npt.NDArray[np.float64]

    @property
    def faces(self) -> npt.NDArray[np.int64]:
        """Surface quads as index rows of four corners."""
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


class DayStudy(Case, frozen=True, kw_only=True, tag=1):
    """Moments stepped in minutes of elapsed time from the start."""

    title = "Day"
    start: Annotated[LocalTime, msgspec.Meta(description="Date and clock time of the first frame")] = LocalTime(hour=6)
    minutes: Annotated[int, msgspec.Meta(ge=1, le=1440, title="Step", description="Minutes between frames")] = 30
    frames: Annotated[int, msgspec.Meta(ge=1, description="Frames the study keys")] = 25


class SeasonStudy(Case, frozen=True, kw_only=True, tag=2):
    """One moment per stepped date at the start's clock time in the site's zone."""

    title = "Season"
    start: Annotated[LocalTime, msgspec.Meta(description="First date and the clock time of every frame")] = LocalTime(hour=6)
    days: Annotated[int, msgspec.Meta(ge=1, le=366, title="Step", description="Days between frames")] = 30
    frames: Annotated[int, msgspec.Meta(ge=1, description="Frames the study keys")] = 25


# --- [OPERATIONS] -----------------------------------------------------------------------


def moments(study: SunStudy, zone: zoneinfo.ZoneInfo) -> RenderingResult[tuple[datetime.datetime, ...]]:
    """Returns every study moment, or the native overflow of its complete sequence."""
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
