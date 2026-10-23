"""Imaging faults and result accumulation."""

from collections.abc import Callable, Iterable
from itertools import chain
from pathlib import Path
import struct
from typing import Final, Literal, Protocol

from expression import Error, Ok, Option, Result
from expression.collections import Block
from imagecodecs import Jpeg8Error
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

# --- [TYPES] ----------------------------------------------------------------------------

type Faults[F] = tuple[F, *tuple[F, ...]]
type Numeric = FloatingPointError | OverflowError | MemoryError | ValueError
type Condition = Literal["over", "divide", "invalid"]
type ColorRequest = str | PyOpenColorIO.Config | PyOpenColorIO.Transform | PyOpenColorIO.Baker | msgspec.Struct
type ImagingFault = (
    ImagingColorFault
    | ImagingUnresolvedFault
    | ImagingCoincidentFault
    | ImagingReadFault
    | ImagingWriteFault
    | ImagingBufferFault
    | ImagingMismatchFault
    | ImagingComparisonFault
    | ImagingCalibrationFault
    | ImagingBaseFault
    | ImagingSpectralFault
    | ImagingNumericFault
    | ImagingContrastFault
    | ImagingLutShapeFault
    | ImagingWhiteFault
    | ImagingJpegFault
)
type ImagingFaults = Faults[ImagingFault]
type ImagingResult[T] = Result[T, ImagingFaults]


class Refused[R, F](Protocol):
    """Fault constructor over request and numeric error."""

    def __call__(self, *, request: R, error: Numeric) -> F:
        """Constructs fault."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

RAISED: Final[frozenset[Condition]] = frozenset(("over", "divide", "invalid"))

# --- [MODELS] ---------------------------------------------------------------------------


class Refusal[R, E](msgspec.Struct, frozen=True, kw_only=True):
    """Request with error that refused it."""

    request: R
    error: E


# --- [ERRORS] ---------------------------------------------------------------------------


class ImagingColorFault(Refusal[ColorRequest, PyOpenColorIO.Exception | PyOpenColorIO.ExceptionMissingFile], frozen=True, kw_only=True):
    """Color request OCIO refused."""


class ImagingUnresolvedFault(Refusal[PyOpenColorIO.Config, tuple[str, ...]], frozen=True, kw_only=True):
    """Config with unresolved roles or colorspaces."""


class ImagingCoincidentFault(msgspec.Struct, frozen=True, kw_only=True):
    """Curve with knots sharing an input coordinate."""

    curve: msgspec.Struct


class ImagingReadFault(Refusal[str | Path, str | msgspec.DecodeError | OSError], frozen=True, kw_only=True):
    """File its reader refused."""


class ImagingWriteFault(Refusal[str | Path, str | OSError | ValueError | OverflowError | struct.error], frozen=True, kw_only=True):
    """Destination its writer refused."""


class ImagingBufferFault(msgspec.Struct, frozen=True, kw_only=True):
    """Buffer operation OpenImageIO refused."""

    request: OpenImageIO.ImageBuf
    error: str

    @classmethod
    def checked(cls, image: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
        """Returns buffer, or fault when buffer holds an error."""
        return Error((cls(request=image, error=image.geterror()),)) if image.has_error else Ok(image)

    @classmethod
    def located(cls, values: npt.NDArray[np.float32 | np.uint8], like: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
        """Returns values as buffer with metadata, origin, and full window of `like`."""
        image = OpenImageIO.ImageBuf(values)
        image.copy_metadata(like)
        image.set_origin(like.xbegin, like.ybegin, like.zbegin)
        image.roi_full = like.roi_full
        return cls.checked(image)

    @classmethod
    def pixels(cls, image: OpenImageIO.ImageBuf) -> ImagingResult[npt.NDArray[np.float32]]:
        """Returns float32 pixels."""
        return Option.of_optional(image.get_pixels(OpenImageIO.FLOAT)).to_result_with(lambda: (cls(request=image, error=image.geterror()),))


class ImagingMismatchFault(msgspec.Struct, frozen=True, kw_only=True):
    """Image operands with unequal shapes."""

    reference: tuple[int, int, int]
    test: tuple[int, int, int]


class ImagingComparisonFault(Refusal[tuple[int, int, int], str], frozen=True, kw_only=True):
    """Comparison OpenImageIO refused."""


class ImagingCalibrationFault(msgspec.Struct, frozen=True, kw_only=True):
    """Fitted matrix failing singular value tolerance."""

    singular_values: tuple[float, ...]
    tolerance: float


class ImagingBaseFault(msgspec.Struct, frozen=True, kw_only=True):
    """Colorspace without linear RGB-to-XYZ matrix."""

    space: str
    transform: PyOpenColorIO.Transform


class ImagingSpectralFault(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Matching weights without finite nonzero channel integrals."""

    space: str
    values: npt.NDArray[np.float64]


class ImagingNumericFault(Refusal[msgspec.Struct | npt.NDArray[np.float64], Numeric | TypeError], frozen=True, kw_only=True, eq=False):
    """Request numeric computation refused."""


class ImagingContrastFault(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Endpoints without contrast to normalize."""

    request: msgspec.Struct
    black: npt.NDArray[np.float64]
    white: npt.NDArray[np.float64]


class ImagingLutShapeFault(msgspec.Struct, frozen=True, kw_only=True):
    """Shape no LUT image layout fits."""

    path: str
    shape: tuple[int, ...]


class ImagingWhiteFault(msgspec.Struct, frozen=True, kw_only=True):
    """Scene white outside tone operator domain."""

    white: Option[float]


class ImagingJpegFault(Refusal[msgspec.Struct, Jpeg8Error], frozen=True, kw_only=True):
    """Compression request JPEG codec refused."""


# --- [OPERATIONS] -----------------------------------------------------------------------


def accumulated[T, F](results: Iterable[Result[T, Faults[F]]]) -> Result[tuple[T, ...], Faults[F]]:
    """Combines results, keeping every fault in input order."""
    block: Block[Result[T, Faults[F]]] = Block.of_seq(results)
    faults: tuple[F, ...] = tuple(chain.from_iterable(block.choose(lambda result: result.swap().to_option())))
    return Error((faults[0], *faults[1:])) if faults else Ok(tuple(block.choose(lambda result: result.to_option())))


def paired[A, B, F, G](left: Result[A, Faults[F]], right: Result[B, Faults[G]]) -> Result[tuple[A, B], Faults[F | G]]:
    """Combines two results, keeping faults left to right."""
    match left, right:
        case Result(tag="error", error=first), Result(tag="error", error=second):
            return Error((first[0], *(first[1:] + second)))
        case Result(tag="error", error=first), _:
            return Error(first)
        case _, Result(tag="error", error=second):
            return Error(second)
        case _:
            return Ok((left.ok, right.ok))


def colored[T](request: ColorRequest, build: Callable[[], T]) -> ImagingResult[T]:
    """Runs OCIO call, returning OCIO exceptions as color fault."""
    try:
        return Ok(build())
    except (PyOpenColorIO.ExceptionMissingFile, PyOpenColorIO.Exception) as error:
        return Error((ImagingColorFault(request=request, error=error),))


def guarded[R, T, F](fault: Refused[R, F], request: R, compute: Callable[[], Result[T, Faults[F]]], raised: frozenset[Condition] = RAISED) -> Result[T, Faults[F]]:
    """Runs `compute` raising on `raised` conditions, returning numeric error as `fault`."""
    try:
        with np.errstate(over="raise" if "over" in raised else None, divide="raise" if "divide" in raised else None, invalid="raise" if "invalid" in raised else None):
            return compute()
    except (FloatingPointError, OverflowError, MemoryError, ValueError) as error:
        return Error((fault(request=request, error=error),))


def scoped[R, T, F](acquired: Result[R, Faults[F]], use: Callable[[R], Result[T, Faults[F]]], release: Callable[[R], Result[object, Faults[F]]]) -> Result[T, Faults[F]]:
    """Uses and releases acquired resource on every path, release fault after use fault."""

    def held(resource: R) -> Result[T, Faults[F]]:
        try:
            used = use(resource)
        finally:
            released = release(resource)
        return paired(used, released).map(lambda pair: pair[0])

    return acquired.bind(held)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "RAISED",
    "Condition",
    "Faults",
    "ImagingBaseFault",
    "ImagingBufferFault",
    "ImagingCalibrationFault",
    "ImagingCoincidentFault",
    "ImagingColorFault",
    "ImagingComparisonFault",
    "ImagingContrastFault",
    "ImagingFault",
    "ImagingFaults",
    "ImagingJpegFault",
    "ImagingLutShapeFault",
    "ImagingMismatchFault",
    "ImagingNumericFault",
    "ImagingReadFault",
    "ImagingResult",
    "ImagingSpectralFault",
    "ImagingUnresolvedFault",
    "ImagingWhiteFault",
    "ImagingWriteFault",
    "Numeric",
    "Refusal",
    "Refused",
    "accumulated",
    "colored",
    "guarded",
    "paired",
    "scoped",
]
