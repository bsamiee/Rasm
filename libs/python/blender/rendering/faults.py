"""Rendering faults and result type."""

import datetime
from pathlib import Path
import re
from typing import override

from expression import Option, Result
import httpx2
import msgspec

from blender.imaging.faults import Faults, ImagingFault, Refusal

# --- [TYPES] ----------------------------------------------------------------------------

type RenderingFault = (
    RenderingNameFault | RenderingRootFault | RenderingSendFault | RenderingDependencyFault | RenderingRunFault | RenderingSystemFault | RenderingTimeFault | RenderingSizeFault | ImagingFault
)
type RenderingFaults = Faults[RenderingFault]
type RenderingResult[T] = Result[T, RenderingFaults]

# --- [ERRORS] ---------------------------------------------------------------------------


class RenderingNameFault(Refusal[str, re.Pattern[str] | ValueError], frozen=True):
    """Name text outside its declared grammar, or template outside the output field grammar."""

    @property
    @override
    def message(self) -> str:
        """Report text naming the unmatched pattern or the template refusal."""
        match self.error:
            case re.Pattern(pattern=pattern):
                return f"{self.request} does not match {pattern}"
            case ValueError() as error:
                return f"{self.request} is no output name template. {error}"


class RenderingRootFault(Refusal[Path, Path], frozen=True):
    """Resolved output path outside its root."""

    template = "{request} lies outside {error}"


class RenderingSendFault(Refusal[str | Path, httpx2.HTTPError | httpx2.InvalidURL | msgspec.DecodeError | ValueError | OSError | OverflowError | int], frozen=True):
    """HTTP transport, decode, or status failure."""

    @property
    @override
    def message(self) -> str:
        """Report text naming the answered status or the exchange error."""
        match self.error:
            case int():
                return f"{self.request} answered HTTP {self.error}"
            case _:
                return f"{self.request} failed. {self.error}"


class RenderingDependencyFault(Refusal[type[msgspec.Struct], tuple[int, ...]], frozen=True):
    """Target unable to enforce part prerequisites."""

    template = "{request.__name__} cannot enforce prerequisites for queue parts {error}"


class RenderingRunFault(Refusal[tuple[str, ...], OSError | int], frozen=True):
    """Process launch failure or nonzero exit."""

    @property
    @override
    def message(self) -> str:
        """Report text naming the exit code or the launch error."""
        match self.error:
            case int():
                return f"{' '.join(self.request)} exited with code {self.error}"
            case _:
                return f"{' '.join(self.request)} failed to start. {self.error}"


class RenderingSystemFault(Refusal[tuple[str, int], OSError], frozen=True):
    """Operating system call failure."""

    template = "{request[0]}({request[1]}) failed. {error}"


class RenderingTimeFault(Refusal[datetime.date | tuple[datetime.datetime, datetime.timedelta, int], OverflowError], frozen=True):
    """Date outside datetime range."""

    template = "Calendar calculation for {request} failed. {error}"


class RenderingSizeFault(Refusal[tuple[int, int], Option[tuple[int, int]]], frozen=True):
    """Empty pixel dimensions or dimensions past renderer bounds."""

    @property
    @override
    def message(self) -> str:
        """Report text naming the bounds, or the empty dimensions."""
        return self.error.map(lambda limit: f"Pixel dimensions {self.request} lie outside full-scale overscan bounds {limit}").default_value(f"Pixel dimensions {self.request} are empty")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "RenderingDependencyFault",
    "RenderingFault",
    "RenderingFaults",
    "RenderingNameFault",
    "RenderingResult",
    "RenderingRootFault",
    "RenderingRunFault",
    "RenderingSendFault",
    "RenderingSizeFault",
    "RenderingSystemFault",
    "RenderingTimeFault",
]
