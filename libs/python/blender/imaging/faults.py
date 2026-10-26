"""Imaging faults and result type."""

from collections.abc import Callable
from typing import override

from expression import Error, Nothing, Ok, Option, Result
from imagecodecs import Jpeg8Error
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.records.faults import Fault, Faults, RecordFault, Refusal

# --- [TYPES] ----------------------------------------------------------------------------

type ColorRequest = str | PyOpenColorIO.Config | PyOpenColorIO.Transform | PyOpenColorIO.Baker | msgspec.Struct
type ImagingFault = ImagingColorFault | ImagingUnresolvedFault | ImagingImageFault | ImagingShapeFault | ImagingBaseFault | ImagingSpectralFault | RecordFault
type ImagingFaults = Faults[ImagingFault]
type ImagingResult[T] = Result[T, ImagingFaults]

# --- [ERRORS] ---------------------------------------------------------------------------


class ImagingColorFault(Refusal[ColorRequest, PyOpenColorIO.Exception | PyOpenColorIO.ExceptionMissingFile], frozen=True, kw_only=True):
    """Color request OCIO refused."""

    @property
    @override
    def message(self) -> str:
        """Report text naming the request or its type."""
        return f"Color operation on {self.request if isinstance(self.request, str) else type(self.request).__name__} failed. {self.error}"


class ImagingUnresolvedFault(Refusal[PyOpenColorIO.Config, tuple[str, ...]], frozen=True, kw_only=True):
    """Config with unresolved roles or colorspaces."""

    @property
    @override
    def message(self) -> str:
        """Report text listing the unresolved names."""
        return f"Color configuration lacks color spaces {', '.join(self.error)}"


class ImagingImageFault(Refusal[OpenImageIO.ImageBuf | tuple[int, ...], str | Jpeg8Error], frozen=True, kw_only=True):
    """Image buffer operation, comparison of planes by reference shape, or JPEG codec refused."""

    @property
    @override
    def message(self) -> str:
        """Report text naming the codec, the compared shape, else the buffer operation."""
        match self.request, self.error:
            case tuple(), Jpeg8Error():
                return f"JPEG coding of {self.request} samples failed. {self.error}"
            case tuple(), _:
                return f"Image comparison at {self.request} failed. {self.error}"
            case _:
                return f"Image buffer operation failed. {self.error}"


class ImagingShapeFault(Fault, frozen=True, kw_only=True):
    """Array shape unequal to its operand or fitting no layout."""

    subject: str
    shape: tuple[int, ...]
    expected: Option[tuple[int, ...]] = Nothing

    @property
    @override
    def message(self) -> str:
        """Report text naming both shapes, or the shape no layout fits."""
        return self.expected.map(lambda expected: f"{self.subject} shapes {expected} and {self.shape} differ").default_value(f"{self.subject} has incompatible shape {self.shape}")


class ImagingBaseFault(Fault, frozen=True, kw_only=True):
    """Colorspace without linear RGB-to-XYZ matrix."""

    space: str
    transform: PyOpenColorIO.Transform

    @property
    @override
    def message(self) -> str:
        """Report text naming space and transform."""
        return f"Color space {self.space} transform {self.transform} lacks linear RGB geometry"


class ImagingSpectralFault(Fault, frozen=True, kw_only=True, eq=False):
    """Matching weights without finite nonzero channel integrals."""

    space: str
    values: npt.NDArray[np.float64]

    @property
    @override
    def message(self) -> str:
        """Report text naming space and integrals."""
        return f"{self.space} spectral matching cannot normalize RGB integrals {self.values}"


# --- [OPERATIONS] -----------------------------------------------------------------------


def colored[T](request: ColorRequest, build: Callable[[], T]) -> ImagingResult[T]:
    """Runs OCIO call, returning OCIO exceptions as color fault."""
    try:
        return Ok(build())
    except (PyOpenColorIO.ExceptionMissingFile, PyOpenColorIO.Exception) as error:
        return Error((ImagingColorFault(request=request, error=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ImagingBaseFault",
    "ImagingColorFault",
    "ImagingFault",
    "ImagingFaults",
    "ImagingImageFault",
    "ImagingResult",
    "ImagingShapeFault",
    "ImagingSpectralFault",
    "ImagingUnresolvedFault",
    "colored",
]
