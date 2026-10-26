"""Signal, structural, and perceptual comparison of written image planes."""

from collections.abc import Callable, Iterable, Mapping
from enum import auto, IntEnum
import math
import operator
from types import MappingProxyType
from typing import Annotated, ClassVar, Final, Self

from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
from skimage.metrics import structural_similarity

from blender.imaging.color.processors import Domain
from blender.imaging.faults import ImagingReason
from blender.imaging.files.images import checked, pixels
from blender.records.faults import Fault, Outcome, paired
from blender.records.fields import Frozen

# --- [TYPES] ----------------------------------------------------------------------------


class Verdict(IntEnum):
    """Comparison grade in severity order."""

    PASS = 0
    ADVISORY = auto()
    UNMEASURED = auto()
    REFUSE = auto()


class Axis(IntEnum):
    """Measured axis."""

    MEAN_ERROR = 1
    RMS_ERROR = 2
    MAX_ERROR = 3
    PSNR = 4
    NRMSE = 5
    FLIP = 6
    SSIM = 7


# --- [TABLES] ---------------------------------------------------------------------------

HOLDS: Final[Mapping[Axis, Callable[[float, float], bool]]] = MappingProxyType({
    Axis.MEAN_ERROR: operator.le,
    Axis.RMS_ERROR: operator.le,
    Axis.MAX_ERROR: operator.le,
    Axis.PSNR: operator.ge,
    Axis.NRMSE: operator.le,
    Axis.FLIP: operator.le,
    Axis.SSIM: operator.ge,
})

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
            .map(lambda value: Verdict.REFUSE if not HOLDS[self.axis](value, self.refuse) else Verdict.ADVISORY if not HOLDS[self.axis](value, self.advisory) else Verdict.PASS)
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
    def of(cls, reference: npt.NDArray[np.float32], test: npt.NDArray[np.float32], domain: Domain, *, data_range: Annotated[float, msgspec.Meta(gt=0.0)]) -> Outcome[Self]:
        """Compares equal-shape planes, `data_range` as SSIM input range."""
        if reference.shape != test.shape:
            return Error((Fault(reason=ImagingReason.MISMATCH, subject="Image", member=str(reference.shape), cause=str(test.shape)),))
        first, second = OpenImageIO.ImageBuf(reference[..., :3]), OpenImageIO.ImageBuf(test[..., :3])
        result = OpenImageIO.ImageBufAlgo.compare(first, second, math.inf, math.inf)
        compared: Outcome[OpenImageIO.CompareResults] = Error((Fault(reason=ImagingReason.COMPARISON, subject=str(reference.shape), cause=OpenImageIO.geterror()),)) if result.error else Ok(result)

        def measured(parts: tuple[OpenImageIO.CompareResults, npt.NDArray[np.float32], npt.NDArray[np.float32]]) -> Outcome[Self]:
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

__all__ = ["HOLDS", "Axis", "Bar", "Comparison", "Verdict"]
