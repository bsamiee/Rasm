"""Scatter of scene light by lens bloom, diffraction, internal reflections, and film halation."""

from collections.abc import Callable
from fractions import Fraction
import math
import sys
from typing import Annotated

from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from scipy.fft import next_fast_len
from scipy.signal import ZoomFFT

from rasm.imaging.chain import Effect, Stage
from rasm.imaging.color import observer
from rasm.imaging.color.processors import matching
from rasm.imaging.effects.optics.defocus import Aperture
from rasm.imaging.faults import ImagingDiffractionFault, ImagingResult, ImagingSpecksFault
from rasm.imaging.hashing import content_hash, rng
from rasm.imaging.parameters import Gated, GraphForm, Presentation, Quantity, Rgb, Seed

# --- [MODELS] -----------------------------------------------------------------------------


class Highlights(msgspec.Struct, frozen=True, kw_only=True):
    """Continuous threshold key for internal reflections and film re-exposure."""

    threshold: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Scene-linear threshold entering the key")] = 1.0
    smoothness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Knee easing light into the key")] = 0.1


class Pupil(msgspec.Struct, frozen=True, kw_only=True):
    """Bladed iris whose spectral diffraction spreads every transmitted ray."""

    iris: Annotated[Aperture, msgspec.Meta(description="Blade outline and central obstruction")] = Aperture(blades=6, rotation=math.pi / 6.0, roundness=0.0)
    squeeze: Annotated[float, msgspec.Meta(ge=1.0, le=2.0, description="Horizontal pupil compression producing anamorphic diffraction")] = 1.0
    core: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Airy radius at 550 nm as a share of the frame's greater side"), Presentation(quantity=Some(Quantity.FRAME_GREATER))
    ] = 0.002

    @property
    def kernel(self) -> Option[Callable[[PyOpenColorIO.Config, tuple[int, int], tuple[int, int], float], ImagingResult[npt.NDArray[np.float32]]]]:
        """Optional diffraction computation, absent at zero core or a closed iris."""
        return Nothing if self.core == 0.0 or self.iris.obstruction == 1.0 else Some(self._kernel)

    def _kernel(
        self, config: PyOpenColorIO.Config, size: tuple[int, int], frame: tuple[int, int], pixel_aspect: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max)]
    ) -> ImagingResult[npt.NDArray[np.float32]]:
        """Returns spectral diffraction weights over every frame displacement.

        Args:
            config: Config providing an admitted linear scene-RGB spectral basis.
            size: Positive stored image extent as (height, width).
            frame: Positive camera frame extent at the same pixel scale.
            pixel_aspect: Positive finite displayed pixel width over height.

        Returns:
            Ok with RGB weights of shape (2 * height - 1, 2 * width - 1, 3)
            and mathematically unit channel sums.
            Error retains native color, numeric capacity, or allocation refusals.
        """
        try:
            shape = (2 * size[0] - 1, 2 * size[1] - 1, 3)
            rows = observer.table()
            side = 2 * next_fast_len(max(frame))
            aspect = Fraction(pixel_aspect)
            greater = max(Fraction(frame[0]), frame[1] * aspect)
            sampling = Fraction(1.22 * 550.0) / (Fraction(self.core) * greater * side)
            edges = np.linspace(-1.0, 1.0, side + 1)
            pupil = self.iris.coverage(self.squeeze * edges, -edges)

            def axis(extent: int, frequency: Fraction) -> tuple[ZoomFFT, npt.NDArray[np.float64]]:
                count = 2 * extent - 1
                step = float(frequency - round(frequency))
                first = float((-(extent - 1) * frequency) % 1)
                transform = ZoomFFT(side, (first, first + count * step), m=count, fs=1.0)
                offsets = np.arange(-(extent - 1), extent, dtype=np.float64)
                phase = np.remainder(offsets * step + 0.5, 1.0) - 0.5
                footprint = (
                    np.sinc(offsets * float(frequency)) if frequency <= 1 else np.divide(np.sinc(phase) * phase * float(1 / frequency), offsets, out=np.ones_like(offsets), where=offsets != 0.0)
                )
                return transform, footprint

            def spread(wavelength: np.float64) -> npt.NDArray[np.float64]:
                frequency = sampling / int(wavelength)
                horizontal, xfootprint = axis(size[1], frequency * aspect)
                vertical, yfootprint = axis(size[0], frequency)
                power = abs(vertical(horizontal(pupil, axis=1), axis=0) * yfootprint[:, None] * xfootprint[None, :]) ** 2
                return power / power.sum()

            def integrated(weights: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float32]]:
                with np.errstate(over="raise", invalid="raise", divide="raise"):
                    return Ok(sum((np.multiply.outer(spread(wavelength), weight) for wavelength, weight in zip(rows[:, 0].flat, weights)), start=np.zeros(shape, dtype=np.float64)).astype(np.float32))

            return matching(config, PyOpenColorIO.ROLE_SCENE_LINEAR, rows[:, 1:]).bind(integrated)
        except (FloatingPointError, MemoryError, OverflowError, ValueError) as error:
            return Error((ImagingDiffractionFault(request=self, size=size, frame=frame, pixel_aspect=pixel_aspect, error=error),))


class Specks(msgspec.Struct, frozen=True, kw_only=True):
    """Seeded dirt coverage on a front lens element."""

    coverage: Annotated[float, msgspec.Meta(ge=0.01, le=1.0, description="Share of the lens covered by dirt")] = 0.2
    scale: Annotated[float, msgspec.Meta(ge=0.001, le=0.5, description="Largest speck as a share of the frame's greater side"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = 0.05
    seed: Annotated[Seed, msgspec.Meta(description="Dirt pattern")] = 0

    def field(self, size: tuple[int, int]) -> ImagingResult[npt.NDArray[np.float32]]:
        """Returns lens dirt at the consuming frame's extent.

        Args:
            size: Positive frame extent as (height, width).

        Returns:
            Ok with float32 coverage, with zero denoting clean lens area.
            Error retains the extent and native allocation or dimension refusal.
        """
        try:
            height, width = (max(2, extent) for extent in size)
            frequency = np.hypot(np.fft.fftfreq(height)[:, None], np.fft.rfftfreq(width)[None, :])
            noise = rng(content_hash(Specks), self.seed).standard_normal((height, width), dtype=np.float32)
            smudge = np.fft.irfft2(np.fft.rfft2(noise) / np.hypot(frequency, 1.0 / (self.scale * max(size))), s=(height, width), axes=(-2, -1))
            floor = np.quantile(smudge, 1.0 - self.coverage)
            return Ok(np.clip((smudge - floor) / (smudge.max() - floor), 0.0, 1.0)[: size[0], : size[1]].astype(np.float32))
        except (MemoryError, OverflowError, ValueError) as error:
            return Error((ImagingSpecksFault(request=self, size=size, error=error),))


class Dirt(msgspec.Struct, frozen=True, kw_only=True):
    """Front-element dirt scattering the bloom that reaches it."""

    strength: Annotated[float, msgspec.Meta(ge=0.0, le=5.0, description="Dirt light as a multiple of the bloom reaching it"), Presentation(soft=Some((0.0, 1.0)))] = 1.0
    specks: Annotated[Specks, msgspec.Meta(description="Lens dirt coverage"), GraphForm.IMAGE] = Specks()


class Bloom(Effect, frozen=True, kw_only=True, tag=27):
    """Whole-frame scatter toward a mip-chain mean, with dirt illuminated by that bloom."""

    stage = Stage.SCATTER
    title = "Bloom"
    pointwise = False
    scatter: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Share of scene light spread into bloom"), Presentation(soft=Some((0.0, 0.25)))] = 0.7 * 0.15**1.5
    size: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Widest bloom level as a share of the frame's greater side"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = 0.28125
    dirt: Annotated[Gated[Dirt], msgspec.Meta(title="Lens Dirt", description="Front-element dirt illuminated by bloom")] = Gated(enabled=False, value=Dirt())


class Glare(Effect, frozen=True, kw_only=True, tag=28):
    """Whole-frame point spread from photopic veiling glare and aperture diffraction."""

    stage = Stage.SCATTER
    title = "Glare"
    pointwise = False
    veil: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Size", description="Spread from a frame spanning 180 to 10 degrees")]], msgspec.Meta(description="Spencer photopic veiling glare")
    ] = Gated(enabled=True, value=0.5)
    aperture: Annotated[Gated[Annotated[Pupil, GraphForm.IMAGE]], msgspec.Meta(description="Spectral diffraction of the iris")] = Gated(enabled=False, value=Pupil())


class Flares(Effect, frozen=True, kw_only=True, tag=29):
    """Internal-reflection ghosts of highlights at signed scales through the frame center."""

    stage = Stage.SCATTER
    title = "Flares"
    pointwise = False
    highlights: Annotated[Highlights, msgspec.Meta(description="Light forming internal reflections")] = Highlights()
    strength: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Ghost light as a multiple of highlights"), Presentation(soft=Some((0.0, 1.0)))] = 1.0
    iterations: Annotated[int, msgspec.Meta(ge=2, le=5, description="Two base ghosts and four per subsequent iteration")] = 3
    color_modulation: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Spectral modulation between ghost channels")] = 0.25


class Halation(Effect, frozen=True, kw_only=True, tag=30):
    """Red-weighted highlight re-exposure reflected by a film base."""

    stage = Stage.HALATION
    title = "Halation"
    pointwise = False
    highlights: Annotated[Highlights, msgspec.Meta(description="Light reaching the film base")] = Highlights(threshold=2.0, smoothness=1.0)
    radius: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Halo blur radius as a share of the frame's greater side"),
        Presentation(soft=Some((0.0, 0.03)), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 64.0 * 0.2 / 1920.0
    color: Annotated[Rgb, msgspec.Meta(description="Film re-exposure per channel")] = Rgb(red=1.0, green=0.1, blue=0.0)


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = ["Bloom", "Dirt", "Flares", "Glare", "Halation", "Highlights", "Pupil", "Specks"]
