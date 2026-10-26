"""Iris coverage kernels, spectral diffraction point spreads, and seeded 1/f lens dirt."""

from fractions import Fraction
import math
from typing import Annotated

from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from scipy.fft import next_fast_len
from scipy.signal import ZoomFFT

from blender.imaging.color import observer
from blender.imaging.color.processors import matching
from blender.imaging.geometry.lenses import Aperture
from blender.records.faults import Outcome
from blender.records.fields import NonNegative, Positive, Seed
from blender.records.identity import content_hash, rng

# --- [MODELS] ---------------------------------------------------------------------------


class Pupil(msgspec.Struct, frozen=True, kw_only=True):
    """Bladed iris with spectral diffraction of every ray."""

    iris: Aperture = Aperture(blades=6, rotation=math.pi / 6.0, roundness=0.0)
    squeeze: Annotated[float, msgspec.Meta(ge=1.0, le=2.0)] = 1.0
    core: NonNegative = 0.002

    def kernel(self, config: PyOpenColorIO.Config, size: tuple[int, int], frame: tuple[int, int], pixel_aspect: Positive) -> Outcome[Option[npt.NDArray[np.float32]]]:
        """Returns spectral diffraction weights over every frame displacement, absent at zero core or a closed iris.

        Args:
            config: Config holding the scene-linear RGB spectral basis.
            size: Stored frame extent as (height, width).
            frame: Camera frame extent as (height, width) at the stored pixel scale.
            pixel_aspect: Displayed pixel width over height.

        Returns:
            RGB weights of shape (2 * height - 1, 2 * width - 1, 3) with unit channel sums, or color and numeric refusals.
        """
        if self.core == 0.0 or self.iris.obstruction == 1.0:
            return Ok(Nothing)
        shape = (2 * size[0] - 1, 2 * size[1] - 1, 3)
        rows = observer.table()
        side = 2 * next_fast_len(max(frame))
        aspect = Fraction(pixel_aspect)
        sampling = Fraction(1.22 * 550.0) / (Fraction(self.core) * max(Fraction(frame[0]), frame[1] * aspect) * side)
        edges = np.linspace(-1.0, 1.0, side + 1)

        def computed(weights: npt.NDArray[np.float64]) -> npt.NDArray[np.float32]:
            pupil = self.iris.coverage(self.squeeze * edges, -edges)

            def axis(extent: int, frequency: Fraction) -> tuple[ZoomFFT, npt.NDArray[np.float64]]:
                count = 2 * extent - 1
                step = float(frequency - round(frequency))
                first = float((-(extent - 1) * frequency) % 1)
                offsets = np.arange(-(extent - 1), extent, dtype=np.float64)
                phase = np.remainder(offsets * step + 0.5, 1.0) - 0.5
                footprint = (
                    np.sinc(offsets * float(frequency)) if frequency <= 1 else np.divide(np.sinc(phase) * phase * float(1 / frequency), offsets, out=np.ones_like(offsets), where=offsets != 0.0)
                )
                return ZoomFFT(side, (first, first + count * step), m=count, fs=1.0), footprint

            def spread(wavelength: np.float64) -> npt.NDArray[np.float64]:
                frequency = sampling / int(wavelength)
                horizontal, xfootprint = axis(size[1], frequency * aspect)
                vertical, yfootprint = axis(size[0], frequency)
                power = abs(vertical(horizontal(pupil, axis=1), axis=0) * yfootprint[:, None] * xfootprint[None, :]) ** 2
                return power / power.sum()

            return sum((np.multiply.outer(spread(wavelength), weight) for wavelength, weight in zip(rows[:, 0].flat, weights)), start=np.zeros(shape, dtype=np.float64)).astype(np.float32)

        return matching(config, PyOpenColorIO.ROLE_SCENE_LINEAR, rows[:, 1:]).map(lambda weights: Some(computed(weights)))


class Specks(msgspec.Struct, frozen=True, kw_only=True):
    """Seeded front-element dirt coverage with a 1/f spectrum."""

    coverage: Annotated[float, msgspec.Meta(ge=0.01, le=1.0)] = 0.2
    scale: Annotated[float, msgspec.Meta(ge=0.001, le=0.5)] = 0.05
    seed: Seed = 0

    def field(self, size: tuple[int, int]) -> npt.NDArray[np.float32]:
        """Returns float32 dirt coverage, zero on clean lens, at positive frame extent (height, width)."""
        height, width = (max(2, extent) for extent in size)
        frequency = np.hypot(np.fft.fftfreq(height)[:, None], np.fft.rfftfreq(width)[None, :])
        noise = rng(content_hash(Specks), self.seed).standard_normal((height, width), dtype=np.float32)
        smudge = np.fft.irfft2(np.fft.rfft2(noise) / np.hypot(frequency, 1.0 / (self.scale * max(size))), s=(height, width), axes=(-2, -1))
        floor = np.quantile(smudge, 1.0 - self.coverage)
        return np.clip((smudge - floor) / (smudge.max() - floor), 0.0, 1.0)[: size[0], : size[1]].astype(np.float32)


# --- [OPERATIONS] -----------------------------------------------------------------------


def bokeh(iris: Aperture, radius: float) -> Option[npt.NDArray[np.float64]]:
    """Returns the iris coverage kernel at circumradius `radius` pixels, odd sided with unit sum, a single tap at zero radius, absent for a closed iris."""
    side = 2 * math.ceil(radius) + 1
    edges = (np.arange(side + 1, dtype=np.float64) - side / 2.0) / (radius or 1.0)
    covered = iris.coverage(edges, -edges) if radius > 0.0 else np.ones((1, 1), dtype=np.float64)
    return Some(covered / covered.sum()) if iris.obstruction < 1.0 else Nothing


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Pupil", "Specks", "bokeh"]
