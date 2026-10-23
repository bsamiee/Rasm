"""Rendering faults and result type."""

import datetime
from enum import auto, Enum
from pathlib import Path
import re

from expression import Option, Result
import msgspec

from blender.imaging.faults import Faults, ImagingFaults, Numeric, Refusal

# --- [TYPES] ----------------------------------------------------------------------------

type RenderingFault = (
    RenderingNameFault
    | RenderingRootFault
    | RenderingReadFault
    | RenderingWriteFault
    | RenderingSendFault
    | RenderingDependencyFault
    | RenderingRunFault
    | RenderingWaitFault
    | RenderingSystemFault
    | RenderingTimeFault
    | RenderingSizeFault
    | RenderingArithmeticFault
    | RenderingImagingFault
)
type RenderingFaults = Faults[RenderingFault]
type RenderingResult[T] = Result[T, RenderingFaults]


class Cause(Enum):
    """HTTP exchange failure with no response status."""

    INVALID_URL = auto()
    UNREACHABLE = auto()
    TIMED_OUT = auto()
    BROKEN = auto()
    UNREADABLE = auto()


class Call(Enum):
    """System call outside launched process."""

    KILL_GROUP = auto()
    EXECUTION_STATE = auto()


# --- [ERRORS] ---------------------------------------------------------------------------


class RenderingNameFault(Refusal[str, re.Pattern[str]], frozen=True):
    """Name text outside its declared grammar."""


class RenderingRootFault(Refusal[Path, Path], frozen=True):
    """Resolved output path outside its root."""


class RenderingReadFault(Refusal[str | Path, msgspec.DecodeError | OSError], frozen=True):
    """Unreadable or undecodable record or file."""


class RenderingWriteFault(Refusal[Path, OSError], frozen=True):
    """Failed write, copy, touch, or removal."""


class RenderingSendFault(Refusal[str | Path, Cause | int | OverflowError], frozen=True):
    """HTTP transport or status failure."""


class RenderingDependencyFault(Refusal[type[msgspec.Struct], tuple[int, ...]], frozen=True):
    """Target unable to enforce part prerequisites."""


class RenderingRunFault(Refusal[tuple[str, ...], OSError | int], frozen=True):
    """Process launch failure or nonzero exit."""


class RenderingWaitFault(Refusal[float, int], frozen=True):
    """Wait interval reaching Windows INFINITE."""


class RenderingSystemFault(Refusal[tuple[Call, int], OSError], frozen=True):
    """Operating system call failure."""


class RenderingTimeFault(Refusal[datetime.date | tuple[datetime.datetime, datetime.timedelta, int], OverflowError], frozen=True):
    """Date outside datetime range."""


class RenderingSizeFault(Refusal[tuple[int, int], Option[tuple[int, int]]], frozen=True):
    """Empty pixel dimensions or dimensions past renderer bounds."""


class RenderingArithmeticFault(Refusal[msgspec.Struct | tuple[float | msgspec.Struct, ...], Numeric], frozen=True):
    """Operands failing arithmetic, allocation, or native representation."""


class RenderingImagingFault(msgspec.Struct, frozen=True, kw_only=True):
    """Imaging faults as one rendering fault."""

    faults: ImagingFaults

    @classmethod
    def lifted(cls, faults: ImagingFaults) -> RenderingFaults:
        """Wraps imaging faults as one rendering fault."""
        return (cls(faults=faults),)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Call",
    "Cause",
    "RenderingArithmeticFault",
    "RenderingDependencyFault",
    "RenderingFault",
    "RenderingFaults",
    "RenderingImagingFault",
    "RenderingNameFault",
    "RenderingReadFault",
    "RenderingResult",
    "RenderingRootFault",
    "RenderingRunFault",
    "RenderingSendFault",
    "RenderingSizeFault",
    "RenderingSystemFault",
    "RenderingTimeFault",
    "RenderingWaitFault",
    "RenderingWriteFault",
]
