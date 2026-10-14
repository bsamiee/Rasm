"""Signal, structural, and perceptual comparison of written image planes."""

from collections.abc import Callable, Iterable, Mapping
from enum import auto, IntEnum
import math
import operator
from types import MappingProxyType
from typing import Annotated, ClassVar, Self

from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
from skimage.metrics import structural_similarity

from blender.imaging.chain import Domain
from blender.imaging.faults import ImagingComparisonFault, ImagingMismatchFault, ImagingResult, paired
from blender.imaging.parameters import Choice

# --- [TYPES] ----------------------------------------------------------------------------


class Verdict(IntEnum):
    """Comparison grade in severity order."""

    PASS = 0
    ADVISORY = auto()
    UNMEASURED = auto()
    REFUSE = auto()

    @property
    def label(self) -> str:
        """Display label derived from the grade name."""
        return self.name.title()


class Axis(Choice):
    """Persistent measured axis and its threshold ordering."""

    holds: Callable[[float, float], bool]

    def __new__(cls, value: int, label: str, holds: Callable[[float, float], bool]) -> Self:
        """Constructs an axis with the comparison direction it grades."""
        member = int.__new__(cls, value)
        member._value_, member.label, member.holds = value, label, holds
        return member

    MEAN_ERROR = 1, "Mean Error", operator.le
    RMS_ERROR = 2, "RMS Error", operator.le
    MAX_ERROR = 3, "Max Error", operator.le
    PSNR = 4, "PSNR", operator.ge
    NRMSE = 5, "NRMSE", operator.le
    FLIP = 6, "FLIP", operator.le
    SSIM = 7, "SSIM", operator.ge


# --- [MODELS] ---------------------------------------------------------------------------


class Bar(msgspec.Struct, frozen=True, kw_only=True):
    """Advisory and refusal thresholds for one measured axis."""

    axis: Axis
    advisory: Annotated[float, msgspec.Meta(ge=0.0)]
    refuse: Annotated[float, msgspec.Meta(ge=0.0)]

    def graded(self, measures: Mapping[Axis, float]) -> Verdict:
        """Grades a measured axis or returns unmeasured for its absence."""
        return (
            Option
            .of_optional(measures.get(self.axis))
            .map(lambda value: Verdict.REFUSE if not self.axis.holds(value, self.refuse) else Verdict.ADVISORY if not self.axis.holds(value, self.advisory) else Verdict.PASS)
            .default_value(Verdict.UNMEASURED)
        )


class Comparison(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Measured axes, absolute and perceptual differences, and optional SSIM map."""

    bars: ClassVar[tuple[Bar, ...]] = (Bar(axis=Axis.PSNR, advisory=35.0, refuse=30.0), Bar(axis=Axis.NRMSE, advisory=0.05, refuse=0.10), Bar(axis=Axis.SSIM, advisory=0.98, refuse=0.95))
    structural_window: ClassVar[int] = 7
    measures: Mapping[Axis, float]
    difference: npt.NDArray[np.float32]
    perceptual: npt.NDArray[np.float32]
    structural: Option[npt.NDArray[np.float64]]

    def verdict(self, bars: Iterable[Bar]) -> Verdict:
        """Returns the worst grade under the caller's current bars."""
        return max((bar.graded(self.measures) for bar in bars), default=Verdict.PASS)


def comparison_of(reference: npt.NDArray[np.float32], test: npt.NDArray[np.float32], domain: Domain, *, data_range: Annotated[float, msgspec.Meta(gt=0.0)]) -> ImagingResult[Comparison]:
    """Compares equal planes with an explicit possible input range for structural similarity."""
    if reference.shape != test.shape:
        return Error((ImagingMismatchFault(reference=reference.shape, test=test.shape),))
    first, second = OpenImageIO.ImageBuf(reference[..., :3]), OpenImageIO.ImageBuf(test[..., :3])
    result = OpenImageIO.ImageBufAlgo.compare(first, second, math.inf, math.inf)
    compared: ImagingResult[OpenImageIO.CompareResults] = Error((ImagingComparisonFault(request=reference.shape, error=OpenImageIO.geterror()),)) if result.error else Ok(result)

    def pixels(image: OpenImageIO.ImageBuf) -> ImagingResult[npt.NDArray[np.float32]]:
        if image.has_error:
            return Error((ImagingComparisonFault(request=reference.shape, error=image.geterror()),))
        match image.get_pixels(OpenImageIO.FLOAT):
            case None:
                return Error((ImagingComparisonFault(request=reference.shape, error=image.geterror()),))
            case values:
                values.setflags(write=False)
                return Ok(values)

    difference = pixels(OpenImageIO.ImageBufAlgo.absdiff(first, second))
    perceptual = pixels(OpenImageIO.ImageBufAlgo.FLIP_diff(first, second, hdr=int(domain is Domain.SCENE_LINEAR)))

    def measured(pair: tuple[OpenImageIO.CompareResults, tuple[npt.NDArray[np.float32], npt.NDArray[np.float32]]]) -> Comparison:
        signal, (difference, flip) = pair
        reference_color, test_color = reference[..., :3].astype(np.float64), test[..., :3].astype(np.float64)
        scale = math.sqrt(float(np.square(reference_color).mean(dtype=np.float64)))
        side = min(Comparison.structural_window, *reference.shape[:2])
        window = side - 1 + side % 2
        structural = Some(structural_similarity(reference_color, test_color, data_range=data_range, channel_axis=-1, win_size=window, full=True)) if window >= 3 else Nothing  # type: ignore[no-untyped-call]
        measures = {
            Axis.MEAN_ERROR: float(signal.meanerror),
            Axis.RMS_ERROR: float(signal.rms_error),
            Axis.MAX_ERROR: float(signal.maxerror),
            Axis.PSNR: float(signal.PSNR),
            **({Axis.NRMSE: float(signal.rms_error / scale)} if scale > 0.0 else {}),
            Axis.FLIP: float(flip.mean(dtype=np.float64)),
            **dict(structural.map(lambda answer: (Axis.SSIM, float(answer[0]))).to_list()),
        }
        for answer in structural.to_list():
            answer[1].setflags(write=False)
        return Comparison(measures=MappingProxyType(measures), difference=difference, perceptual=flip, structural=structural.map(lambda answer: answer[1]))

    return paired(compared, paired(difference, perceptual)).map(measured)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Axis", "Bar", "Comparison", "Verdict", "comparison_of"]
