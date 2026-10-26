"""Imaging faults and result accumulation."""

from abc import abstractmethod
from collections.abc import Callable, Iterable
from enum import auto, Enum
from importlib.resources.abc import Traversable
from itertools import chain
from pathlib import Path
import struct
from typing import ClassVar, Literal, overload, override, Protocol

from expression import Error, Nothing, Ok, Option, Result
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
    | ImagingFileFault
    | ImagingBufferFault
    | ImagingShapeFault
    | ImagingNumericFault
    | ImagingCoincidentFault
    | ImagingCalibrationFault
    | ImagingBaseFault
    | ImagingSpectralFault
    | ImagingContrastFault
    | ImagingWhiteFault
)
type ImagingFaults = Faults[ImagingFault]
type ImagingResult[T] = Result[T, ImagingFaults]


class Refused[R, F](Protocol):
    """Fault constructor over request and numeric error."""

    def __call__(self, *, request: R, error: Numeric) -> F:
        """Constructs fault."""
        ...


class Access(Enum):
    """Direction of a file operation."""

    READ = auto()
    WRITE = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Fault(msgspec.Struct, frozen=True, kw_only=True):
    """Refused or unmet operation with its report text."""

    @property
    @abstractmethod
    def message(self) -> str:
        """Report text of the fault."""


class Refusal[R, E](Fault, frozen=True, kw_only=True):
    """Request with error that refused it."""

    template: ClassVar[str]
    request: R
    error: E

    @property
    @override
    def message(self) -> str:
        """Report text of the refusal."""
        return self.template.format(request=self.request, error=self.error)


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


class ImagingFileFault(
    Refusal[str | Path | Traversable, OSError | ValueError | TypeError | KeyError | OverflowError | MemoryError | NotImplementedError | struct.error | msgspec.DecodeError | str],
    frozen=True,
    kw_only=True,
):
    """File or stored form its reader or writer refused."""

    access: Access

    @property
    @override
    def message(self) -> str:
        """Report text naming the refused direction."""
        match self.access:
            case Access.READ:
                return f"{self.request} is unreadable. {self.error}"
            case Access.WRITE:
                return f"{self.request} is unwritable. {self.error}"


class ImagingBufferFault(Refusal[OpenImageIO.ImageBuf | tuple[int, int, int], str], frozen=True, kw_only=True):
    """Buffer operation or comparison OpenImageIO refused."""

    @property
    @override
    def message(self) -> str:
        """Report text naming a comparison by its reference shape, else a buffer operation."""
        match self.request:
            case tuple():
                return f"Image comparison failed. {self.error}"
            case _:
                return f"Image buffer operation failed. {self.error}"

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


class ImagingNumericFault(Refusal[msgspec.Struct | npt.NDArray[np.float64] | tuple[float | msgspec.Struct, ...], Numeric | TypeError | Jpeg8Error], frozen=True, kw_only=True, eq=False):
    """Request numeric computation, allocation, native representation, or codec refused."""

    @property
    @override
    def message(self) -> str:
        """Report text naming the codec or the numeric error type."""
        match self.error:
            case Jpeg8Error():
                return f"JPEG compression for {self.request} failed. {self.error}"
            case _:
                return f"Numeric operation for {self.request} failed ({type(self.error).__name__}). {self.error}"


class ImagingCoincidentFault(Fault, frozen=True, kw_only=True):
    """Curve with knots sharing an input coordinate."""

    curve: msgspec.Struct

    @property
    @override
    def message(self) -> str:
        """Report text naming the curve type."""
        return f"{type(self.curve).__name__} holds two knots at one input"


class ImagingCalibrationFault(Fault, frozen=True, kw_only=True):
    """Fitted matrix failing singular value tolerance."""

    singular_values: tuple[float, ...]
    tolerance: float

    @property
    @override
    def message(self) -> str:
        """Report text naming values and tolerance."""
        return f"Calibration matrix singular values {self.singular_values} fail tolerance {self.tolerance:.3g}"


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


class ImagingContrastFault(Fault, frozen=True, kw_only=True, eq=False):
    """Endpoints without contrast to normalize."""

    request: msgspec.Struct
    black: npt.NDArray[np.float64]
    white: npt.NDArray[np.float64]

    @property
    @override
    def message(self) -> str:
        """Report text naming request and endpoints."""
        return f"{self.request} has no contrast between black {self.black} and white {self.white}"


class ImagingWhiteFault(Fault, frozen=True, kw_only=True):
    """Scene white outside tone operator domain."""

    white: Option[float]

    @property
    @override
    def message(self) -> str:
        """Report text naming the white, or the empty metering."""
        return self.white.map(lambda value: f"Scene white {value} lies outside tone operator exposure domain").default_value("White metering covers no finite luminance sample")


# --- [OPERATIONS] -----------------------------------------------------------------------


def accumulated[T, F](results: Iterable[Result[T, Faults[F]]]) -> Result[tuple[T, ...], Faults[F]]:
    """Combines results, keeping every fault in input order."""
    block: Block[Result[T, Faults[F]]] = Block.of_seq(results)
    faults: tuple[F, ...] = tuple(chain.from_iterable(block.choose(lambda result: result.swap().to_option())))
    return Error((faults[0], *faults[1:])) if faults else Ok(tuple(block.choose(lambda result: result.to_option())))


@overload
def paired[A, B, FA, FB](a: Result[A, Faults[FA]], b: Result[B, Faults[FB]], /) -> Result[tuple[A, B], Faults[FA | FB]]: ...
@overload
def paired[A, B, C, FA, FB, FC](a: Result[A, Faults[FA]], b: Result[B, Faults[FB]], c: Result[C, Faults[FC]], /) -> Result[tuple[A, B, C], Faults[FA | FB | FC]]: ...
@overload
def paired[A, B, C, D, FA, FB, FC, FD](
    a: Result[A, Faults[FA]], b: Result[B, Faults[FB]], c: Result[C, Faults[FC]], d: Result[D, Faults[FD]], /
) -> Result[tuple[A, B, C, D], Faults[FA | FB | FC | FD]]: ...
@overload
def paired[A, B, C, D, E, FA, FB, FC, FD, FE](
    a: Result[A, Faults[FA]], b: Result[B, Faults[FB]], c: Result[C, Faults[FC]], d: Result[D, Faults[FD]], e: Result[E, Faults[FE]], /
) -> Result[tuple[A, B, C, D, E], Faults[FA | FB | FC | FD | FE]]: ...
@overload
def paired[A, B, C, D, E, G, FA, FB, FC, FD, FE, FG](
    a: Result[A, Faults[FA]], b: Result[B, Faults[FB]], c: Result[C, Faults[FC]], d: Result[D, Faults[FD]], e: Result[E, Faults[FE]], g: Result[G, Faults[FG]], /
) -> Result[tuple[A, B, C, D, E, G], Faults[FA | FB | FC | FD | FE | FG]]: ...
def paired[F](*results: Result[object, Faults[F]]) -> Result[tuple[object, ...], Faults[F]]:
    """Combines two to six results into one tuple, keeping every fault in input order."""
    return accumulated(results)


def colored[T](request: ColorRequest, build: Callable[[], T]) -> ImagingResult[T]:
    """Runs OCIO call, returning OCIO exceptions as color fault."""
    try:
        return Ok(build())
    except (PyOpenColorIO.ExceptionMissingFile, PyOpenColorIO.Exception) as error:
        return Error((ImagingColorFault(request=request, error=error),))


def guarded[R, T, F](fault: Refused[R, F], request: R, compute: Callable[[], Result[T, Faults[F]]], raised: frozenset[Condition] = frozenset(("over", "divide", "invalid"))) -> Result[T, Faults[F]]:
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
    "Access",
    "Condition",
    "Fault",
    "Faults",
    "ImagingBaseFault",
    "ImagingBufferFault",
    "ImagingCalibrationFault",
    "ImagingCoincidentFault",
    "ImagingColorFault",
    "ImagingContrastFault",
    "ImagingFault",
    "ImagingFaults",
    "ImagingFileFault",
    "ImagingNumericFault",
    "ImagingResult",
    "ImagingShapeFault",
    "ImagingSpectralFault",
    "ImagingUnresolvedFault",
    "ImagingWhiteFault",
    "Numeric",
    "Refusal",
    "Refused",
    "accumulated",
    "colored",
    "guarded",
    "paired",
    "scoped",
]
