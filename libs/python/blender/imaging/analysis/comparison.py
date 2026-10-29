"""Signal, structural, and perceptual comparison of written image planes."""

from collections.abc import Callable, Iterable, Mapping
from enum import auto, IntEnum
import math
import operator
from types import MappingProxyType
from typing import ClassVar, Final

from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
from skimage.metrics import normalized_root_mse, structural_similarity

from blender.imaging.color.processors import Domain
from blender.imaging.faults import ImagingReason
from blender.imaging.files.buffers import checked, pixels
from blender.records.faults import Fault, Outcome, paired, RecordReason
from blender.records.fields import Frozen, NonNegative

# --- [TYPES] ----------------------------------------------------------------------------


class Severity(IntEnum):
    """Comparison grade in severity order."""

    PASS = 0
    ADVISORY = auto()
    UNMEASURED = auto()
    REFUSE = auto()


class Metric(IntEnum):
    """Measured axis."""

    MEAN_ERROR = 1
    RMS_ERROR = 2
    MAX_ERROR = 3
    PSNR = 4
    NRMSE = 5
    FLIP = 6
    SSIM = 7


# --- [TABLES] ---------------------------------------------------------------------------

_HOLDS: Final[Mapping[Metric, Callable[[float, float], bool]]] = MappingProxyType({
    Metric.MEAN_ERROR: operator.le,
    Metric.RMS_ERROR: operator.le,
    Metric.MAX_ERROR: operator.le,
    Metric.PSNR: operator.ge,
    Metric.NRMSE: operator.le,
    Metric.FLIP: operator.le,
    Metric.SSIM: operator.ge,
})

# --- [MODELS] ---------------------------------------------------------------------------


class Bar(msgspec.Struct, frozen=True, kw_only=True):
    """Advisory and refusal thresholds of an axis."""

    metric: Metric
    advisory: NonNegative
    refuse: NonNegative

    def graded(self, measures: Mapping[Metric, float]) -> Severity:
        """Grades axis measure, unmeasured when absent."""
        holds = _HOLDS[self.metric]
        return (
            Option
            .of_optional(measures.get(self.metric))
            .map(lambda value: Severity.REFUSE if not holds(value, self.refuse) else Severity.ADVISORY if not holds(value, self.advisory) else Severity.PASS)
            .default_value(Severity.UNMEASURED)
        )


class Comparison(Frozen, frozen=True, kw_only=True, eq=False):
    """Metric measures, difference planes, and SSIM map."""

    structural_window: ClassVar[int] = 7
    measures: Mapping[Metric, float]
    difference: npt.NDArray[np.float32]
    perceptual: npt.NDArray[np.float32]
    structural: Option[npt.NDArray[np.float64]]

    @classmethod
    def of(cls, reference: npt.NDArray[np.float32], test: npt.NDArray[np.float32], domain: Domain, *, data_range: float) -> Outcome["Comparison"]:
        """Compares equal-shape planes, `data_range` as SSIM input range."""
        if reference.shape != test.shape:
            return Error(Fault.of(RecordReason.OUTSIDE, cls.__name__, str(test.shape), member=f"test planes of the reference shape {reference.shape}"))
        first, second = OpenImageIO.ImageBuf(reference[..., :3]), OpenImageIO.ImageBuf(test[..., :3])

        @np.errstate(divide="ignore", invalid="ignore")
        def measured(parts: tuple[OpenImageIO.CompareResults, npt.NDArray[np.float32], npt.NDArray[np.float32]]) -> Comparison:
            signal, difference, flip = parts
            reference_color, test_color = reference[..., :3].astype(np.float64), test[..., :3].astype(np.float64)
            normalized = float(normalized_root_mse(reference_color, test_color, normalization="euclidean"))
            side = min(cls.structural_window, *reference.shape[:2])
            window = side - 1 + side % 2
            structural = Some(structural_similarity(reference_color, test_color, data_range=data_range, channel_axis=-1, win_size=window, full=True)) if window >= 3 else Nothing
            measures = {
                Metric.MEAN_ERROR: signal.meanerror,
                Metric.RMS_ERROR: signal.rms_error,
                Metric.MAX_ERROR: signal.maxerror,
                Metric.PSNR: signal.PSNR,
                **({Metric.NRMSE: normalized} if math.isfinite(normalized) else {}),
                Metric.FLIP: float(flip.mean(dtype=np.float64)),
                **dict(structural.map(lambda answer: (Metric.SSIM, float(answer[0]))).to_list()),
            }
            return cls(measures=MappingProxyType(measures), difference=difference, perceptual=flip, structural=structural.map(lambda answer: answer[1]))

        return paired(
            Ok(OpenImageIO.ImageBufAlgo.compare(first, second, math.inf, math.inf)).filter_with(
                lambda result: not result.error, lambda _: Fault.of(ImagingReason.BUFFER, f"comparison of {reference.shape} planes", OpenImageIO.geterror())
            ),
            checked(OpenImageIO.ImageBufAlgo.absdiff(first, second)).bind(pixels),
            checked(OpenImageIO.ImageBufAlgo.FLIP_diff(first, second, hdr=int(domain is Domain.SCENE_LINEAR))).bind(pixels),
        ).map(measured)

    def severity(self, bars: Iterable[Bar]) -> Severity:
        """Returns worst grade across thresholds."""
        return max((bar.graded(self.measures) for bar in bars), default=Severity.PASS)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bar", "Comparison", "Metric", "Severity"]
