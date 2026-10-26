"""Signal, structural, and perceptual comparison of written image planes."""

from collections.abc import Callable, Iterable, Mapping
from enum import auto
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

from blender.imaging.color.processors import Domain
from blender.imaging.faults import ImagingImageFault, ImagingResult, ImagingShapeFault
from blender.imaging.files.images import checked, pixels
from blender.records.faults import paired
from blender.records.fields import Choice, Frozen

# --- [TYPES] ----------------------------------------------------------------------------


class Verdict(Choice):
    """Comparison grade in severity order."""

    PASS = 0
    ADVISORY = auto()
    UNMEASURED = auto()
    REFUSE = auto()


class Axis(Choice):
    """Measured axis with threshold direction."""

    holds: Callable[[float, float], bool]

    def __init__(self, value: int, holds: Callable[[float, float], bool], label: str | None = None) -> None:
        """Sets threshold comparison and label."""
        super().__init__(value, label)
        self.holds = holds

    MEAN_ERROR = 1, operator.le
    RMS_ERROR = 2, operator.le, "RMS Error"
    MAX_ERROR = 3, operator.le
    PSNR = 4, operator.ge, "PSNR"
    NRMSE = 5, operator.le, "NRMSE"
    FLIP = 6, operator.le, "FLIP"
    SSIM = 7, operator.ge, "SSIM"


# --- [MODELS] ---------------------------------------------------------------------------


class Bar(msgspec.Struct, frozen=True, kw_only=True):
    """Advisory and refusal thresholds of an axis."""

    axis: Axis
    advisory: Annotated[float, msgspec.Meta(ge=0.0)]
    refuse: Annotated[float, msgspec.Meta(ge=0.0)]

    def graded(self, measures: Mapping[Axis, float]) -> Verdict:
        """Grades axis measure, unmeasured when absent."""
        return (
            Option
            .of_optional(measures.get(self.axis))
            .map(lambda value: Verdict.REFUSE if not self.axis.holds(value, self.refuse) else Verdict.ADVISORY if not self.axis.holds(value, self.advisory) else Verdict.PASS)
            .default_value(Verdict.UNMEASURED)
        )


class Comparison(Frozen, frozen=True, kw_only=True, eq=False):
    """Axis measures, difference planes, and SSIM map."""

    structural_window: ClassVar[int] = 7
    measures: Mapping[Axis, float]
    difference: npt.NDArray[np.float32]
    perceptual: npt.NDArray[np.float32]
    structural: Option[npt.NDArray[np.float64]]

    @classmethod
    def of(cls, reference: npt.NDArray[np.float32], test: npt.NDArray[np.float32], domain: Domain, *, data_range: Annotated[float, msgspec.Meta(gt=0.0)]) -> ImagingResult[Self]:
        """Compares equal-shape planes, `data_range` as SSIM input range."""
        if reference.shape != test.shape:
            return Error((ImagingShapeFault(subject="Image", shape=test.shape, expected=Some(reference.shape)),))
        first, second = OpenImageIO.ImageBuf(reference[..., :3]), OpenImageIO.ImageBuf(test[..., :3])
        result = OpenImageIO.ImageBufAlgo.compare(first, second, math.inf, math.inf)
        compared: ImagingResult[OpenImageIO.CompareResults] = Error((ImagingImageFault(request=reference.shape, error=OpenImageIO.geterror()),)) if result.error else Ok(result)

        def measured(parts: tuple[OpenImageIO.CompareResults, npt.NDArray[np.float32], npt.NDArray[np.float32]]) -> ImagingResult[Self]:
            signal, difference, flip = parts
            reference_color, test_color = reference[..., :3].astype(np.float64), test[..., :3].astype(np.float64)
            scale = math.sqrt(float(np.square(reference_color).mean(dtype=np.float64)))
            side = min(cls.structural_window, *reference.shape[:2])
            window = side - 1 + side % 2
            structural = Some(structural_similarity(reference_color, test_color, data_range=data_range, channel_axis=-1, win_size=window, full=True)) if window >= 3 else Nothing
            measures = {
                Axis.MEAN_ERROR: signal.meanerror,
                Axis.RMS_ERROR: signal.rms_error,
                Axis.MAX_ERROR: signal.maxerror,
                Axis.PSNR: signal.PSNR,
                **({Axis.NRMSE: signal.rms_error / scale} if scale > 0.0 else {}),
                Axis.FLIP: float(flip.mean(dtype=np.float64)),
                **dict(structural.map(lambda answer: (Axis.SSIM, float(answer[0]))).to_list()),
            }
            return Ok(cls(measures=MappingProxyType(measures), difference=difference, perceptual=flip, structural=structural.map(lambda answer: answer[1])))

        return paired(
            compared,
            checked(OpenImageIO.ImageBufAlgo.absdiff(first, second)).bind(pixels),
            checked(OpenImageIO.ImageBufAlgo.FLIP_diff(first, second, hdr=int(domain is Domain.SCENE_LINEAR))).bind(pixels),
        ).bind(measured)

    def verdict(self, bars: Iterable[Bar]) -> Verdict:
        """Returns worst grade across thresholds."""
        return max((bar.graded(self.measures) for bar in bars), default=Verdict.PASS)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Axis", "Bar", "Comparison", "Verdict"]
