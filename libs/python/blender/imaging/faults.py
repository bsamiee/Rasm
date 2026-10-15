"""Imaging failures and accumulation of independent results."""

from collections.abc import Iterable
from itertools import chain
from pathlib import Path
import struct
from typing import TYPE_CHECKING

from expression import Error, Ok, Option, Result
from expression.collections import Block
from imagecodecs import Jpeg8Error
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

if TYPE_CHECKING:
    from blender.imaging.effects.optics.glare import Pupil, Specks

# --- [TYPES] ----------------------------------------------------------------------------

type Faults[F] = tuple[F, *tuple[F, ...]]
type ImagingFault = (
    ImagingConfigFault
    | ImagingUnresolvedFault
    | ImagingProcessorFault
    | ImagingCurveFault
    | ImagingCoincidentFault
    | ImagingLutFault
    | ImagingReadFault
    | ImagingWriteFault
    | ImagingBufferFault
    | ImagingMismatchFault
    | ImagingComparisonFault
    | ImagingCalibrationFault
    | ImagingBaseFault
    | ImagingSpectralFault
    | ImagingDiffractionFault
    | ImagingSpecksFault
    | ImagingMatrixFault
    | ImagingContrastFault
    | ImagingLutShapeFault
    | ImagingWhiteFault
    | ImagingJpegFault
    | ImagingGeneratorFault
    | ImagingRepresentationFault
)
type ImagingFaults = Faults[ImagingFault]
type ImagingResult[T] = Result[T, ImagingFaults]

# --- [MODELS] ---------------------------------------------------------------------------


class Refusal[R, E](msgspec.Struct, frozen=True, kw_only=True):
    """Request and the provider answer that refused it."""

    request: R
    error: E


# --- [ERRORS] ---------------------------------------------------------------------------


class ImagingConfigFault(Refusal[str | PyOpenColorIO.Config, PyOpenColorIO.Exception | PyOpenColorIO.ExceptionMissingFile], frozen=True, kw_only=True):
    """Color configuration its provider refused."""


class ImagingUnresolvedFault(Refusal[PyOpenColorIO.Config, tuple[str, ...]], frozen=True, kw_only=True):
    """Configuration with unresolved roles or color spaces."""


class ImagingProcessorFault(Refusal[PyOpenColorIO.Transform, PyOpenColorIO.Exception], frozen=True, kw_only=True):
    """Transform its processor refused."""


class ImagingCurveFault(Refusal[msgspec.Struct, PyOpenColorIO.Exception], frozen=True, kw_only=True):
    """Curve its grading provider refused."""


class ImagingCoincidentFault(msgspec.Struct, frozen=True, kw_only=True):
    """Curve with knots sharing an input coordinate."""

    curve: msgspec.Struct


class ImagingLutFault(Refusal[PyOpenColorIO.Transform | PyOpenColorIO.Baker | str, PyOpenColorIO.ExceptionMissingFile | PyOpenColorIO.Exception], frozen=True, kw_only=True):
    """Lookup source or operation its provider refused."""


class ImagingReadFault(Refusal[str | Path, str | msgspec.DecodeError | OSError], frozen=True, kw_only=True):
    """File source its reader refused."""


class ImagingWriteFault(Refusal[str | Path, str | OSError | ValueError | OverflowError | struct.error], frozen=True, kw_only=True):
    """Image destination its writer refused."""


class ImagingBufferFault(msgspec.Struct, frozen=True, kw_only=True):
    """Image buffer operation its provider refused."""

    request: OpenImageIO.ImageBuf
    error: str

    @classmethod
    def checked(cls, image: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
        """Answers the buffer, or its provider's refusal when the buffer holds an error."""
        return Error((cls(request=image, error=image.geterror()),)) if image.has_error else Ok(image)


class ImagingMismatchFault(msgspec.Struct, frozen=True, kw_only=True):
    """Image operands with unequal shapes."""

    reference: tuple[int, int, int]
    test: tuple[int, int, int]


class ImagingComparisonFault(Refusal[tuple[int, int, int], str], frozen=True, kw_only=True):
    """Image comparison its provider refused."""


class ImagingCalibrationFault(msgspec.Struct, frozen=True, kw_only=True):
    """Fitted matrix whose singular values failed its tolerance."""

    singular_values: tuple[float, ...]
    tolerance: float


class ImagingBaseFault(msgspec.Struct, frozen=True, kw_only=True):
    """Native color conversion without the linear RGB geometry its operation needs."""

    space: str
    transform: PyOpenColorIO.Transform


class ImagingSpectralFault(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Native RGB matching rows without finite nonzero channel integrals."""

    space: str
    values: npt.NDArray[np.float64]


class ImagingDiffractionFault(msgspec.Struct, frozen=True, kw_only=True):
    """Pupil exceeding its native arithmetic or image publication domain."""

    request: "Pupil"
    size: tuple[int, int]
    frame: tuple[int, int]
    pixel_aspect: float
    error: FloatingPointError | MemoryError | OverflowError | ValueError


class ImagingSpecksFault(msgspec.Struct, frozen=True, kw_only=True):
    """Lens dirt extent its native array provider refused."""

    request: "Specks"
    error: MemoryError | OverflowError | ValueError
    size: tuple[int, int]


class ImagingMatrixFault(Refusal[npt.NDArray[np.float64], np.linalg.LinAlgError], frozen=True, kw_only=True, eq=False):
    """Coefficient matrix its native linear solver refused."""


class ImagingContrastFault(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Black and white endpoints with no contrast for normalization."""

    request: msgspec.Struct
    black: npt.NDArray[np.float64]
    white: npt.NDArray[np.float64]


class ImagingLutShapeFault(msgspec.Struct, frozen=True, kw_only=True):
    """Image or cube shape incompatible with its LUT image layout."""

    path: str
    shape: tuple[int, ...]


class ImagingWhiteFault(msgspec.Struct, frozen=True, kw_only=True):
    """Measured scene white outside the tone operator's exposure domain."""

    white: Option[float]


class ImagingJpegFault(Refusal[msgspec.Struct, Jpeg8Error], frozen=True, kw_only=True):
    """Compression request its native JPEG codec refused."""


class ImagingGeneratorFault(Refusal[msgspec.Struct, FloatingPointError | MemoryError | OverflowError | ValueError], frozen=True, kw_only=True):
    """Procedural field request its numerical provider refused."""


class ImagingRepresentationFault(Refusal[msgspec.Struct, TypeError | OverflowError | MemoryError], frozen=True, kw_only=True):
    """Image representation its native dimension or allocation provider refused."""


# --- [OPERATIONS] -----------------------------------------------------------------------


def accumulated[T, F](results: Iterable[Result[T, Faults[F]]]) -> Result[tuple[T, ...], Faults[F]]:
    """Combines independent results, retaining every failure in input order."""
    block: Block[Result[T, Faults[F]]] = Block.of_seq(results)
    faults: tuple[F, ...] = tuple(chain.from_iterable(block.choose(lambda result: result.swap().to_option())))
    return Error((faults[0], *faults[1:])) if faults else Ok(tuple(block.choose(lambda result: result.to_option())))


def paired[A, B, F, G](left: Result[A, Faults[F]], right: Result[B, Faults[G]]) -> Result[tuple[A, B], Faults[F | G]]:
    """Combines independent success and fault types, retaining failures left to right."""
    match left, right:
        case Result(tag="error", error=first), Result(tag="error", error=second):
            return Error((first[0], *(first[1:] + second)))
        case Result(tag="error", error=first), _:
            return Error(first)
        case _, Result(tag="error", error=second):
            return Error(second)
        case _:
            return Ok((left.ok, right.ok))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Faults",
    "ImagingBaseFault",
    "ImagingBufferFault",
    "ImagingCalibrationFault",
    "ImagingCoincidentFault",
    "ImagingComparisonFault",
    "ImagingConfigFault",
    "ImagingContrastFault",
    "ImagingCurveFault",
    "ImagingDiffractionFault",
    "ImagingFault",
    "ImagingFaults",
    "ImagingGeneratorFault",
    "ImagingJpegFault",
    "ImagingLutFault",
    "ImagingLutShapeFault",
    "ImagingMatrixFault",
    "ImagingMismatchFault",
    "ImagingProcessorFault",
    "ImagingReadFault",
    "ImagingRepresentationFault",
    "ImagingResult",
    "ImagingSpecksFault",
    "ImagingSpectralFault",
    "ImagingUnresolvedFault",
    "ImagingWhiteFault",
    "ImagingWriteFault",
    "Refusal",
    "accumulated",
    "paired",
]
