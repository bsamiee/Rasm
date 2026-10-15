"""Rendering failures and the result shared by rendering operations."""

import datetime
from enum import auto, Enum
from pathlib import Path
import re
from typing import Self

from expression import Option, Result
import httpx2
import msgspec

from blender.imaging.faults import Faults, ImagingFaults, Refusal

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
    """Failure of an HTTP exchange that answered no status."""

    INVALID_URL = auto()
    UNREACHABLE = auto()
    TIMED_OUT = auto()
    BROKEN = auto()
    UNREADABLE = auto()


class Call(Enum):
    """Operating-system call made outside a launched process."""

    KILL_GROUP = auto()
    EXECUTION_STATE = auto()


# --- [ERRORS] ---------------------------------------------------------------------------


class RenderingNameFault(Refusal[str, re.Pattern[str]], frozen=True):
    """Name text outside its declared grammar."""


class RenderingRootFault(Refusal[Path, Path], frozen=True):
    """Resolved output path outside its root."""


class RenderingReadFault(Refusal[str | Path, msgspec.DecodeError | OSError], frozen=True):
    """Stored record or file its reader refused."""


class RenderingWriteFault(Refusal[Path, OSError], frozen=True):
    """Record write or file copy its writer refused."""


class RenderingSendFault(Refusal[str | httpx2.URL | Path, Cause | int | OverflowError], frozen=True):
    """HTTP request refused by transport or response status."""


class RenderingDependencyFault(Refusal[type[msgspec.Struct], tuple[int, ...]], frozen=True):
    """Dispatch target cannot enforce prerequisites of the requested queue parts."""


class RenderingRunFault(Refusal[tuple[str, ...], OSError | int], frozen=True):
    """Process launch or exit refused."""


class RenderingWaitFault(Refusal[float, int], frozen=True):
    """Timed wait interval reaches the native infinite-wait word."""


class RenderingSystemFault(Refusal[tuple[Call, int], OSError], frozen=True):
    """Operating-system call refused."""


class RenderingTimeFault(Refusal[datetime.date | tuple[datetime.datetime, datetime.timedelta, int], OverflowError], frozen=True):
    """Calendar result outside native datetime arithmetic."""


class RenderingSizeFault(Refusal[tuple[int, int], Option[tuple[int, int]] | OverflowError | FloatingPointError | ValueError | MemoryError], frozen=True):
    """Dimensions or their allocation refused by the receiving host."""


class RenderingArithmeticFault(Refusal[tuple[float, ...], OverflowError | FloatingPointError | msgspec.ValidationError], frozen=True):
    """Numeric operands whose result exceeds the native representation."""

    @classmethod
    def of(cls, request: msgspec.Struct | tuple[object, ...], error: OverflowError | FloatingPointError | msgspec.ValidationError) -> Self:
        """Constructs a numeric refusal from its native structured request."""

        def operands(value: object) -> tuple[float, ...]:
            match value:
                case float():
                    return (value,)
                case msgspec.Struct():
                    return operands(msgspec.structs.astuple(value))
                case Option():
                    return operands(tuple(value.to_list()))
                case tuple():
                    return tuple(number for item in value for number in operands(item))
                case _:
                    return ()

        return cls(request=operands(request), error=error)


class RenderingImagingFault(msgspec.Struct, frozen=True, kw_only=True):
    """Complete fault tuple from an imaging operation."""

    faults: ImagingFaults

    @classmethod
    def lifted(cls, faults: ImagingFaults) -> RenderingFaults:
        """Carries the imaging faults as one rendering fault."""
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
