"""Seeded draws, seamless noise tiles, and lens kernels: draw families, stable uniforms per key, filtered Gaussian tiles, centered rank tiles of provider images, tile statistics, seeded 1/f lens dirt, and spectral diffraction point spreads."""

from fractions import Fraction
import math
from typing import Annotated

from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO
from scipy import ndimage
from scipy.fft import next_fast_len
from scipy.signal import ZoomFFT
from scipy.special import ndtri
from scipy.stats import rankdata

from blender.imaging.color.colorimetry import matching, table
from blender.imaging.files.buffers import Depth, pixels
from blender.imaging.geometry.lenses import Aperture
from blender.records.faults import Outcome
from blender.records.fields import NonNegative, Seed
from blender.records.stores import content_hash, rng

# --- [MODELS] ---------------------------------------------------------------------------


class Pupil(msgspec.Struct, frozen=True, kw_only=True):
    """Bladed iris with spectral diffraction of every ray."""

    iris: Aperture = Aperture(blades=6, rotation=math.pi / 6.0, roundness=0.0)
    squeeze: Annotated[float, msgspec.Meta(ge=1.0, le=2.0)] = 1.0
    core: NonNegative = 0.002

    def kernel(self, config: PyOpenColorIO.Config, size: tuple[int, int], frame: tuple[int, int], pixel_aspect: float) -> Outcome[Option[npt.NDArray[np.float32]]]:
        """Returns spectral diffraction weights over every displacement of the stored `size` frame, `frame` the camera extent at stored pixel scale.

        Returns:
            RGB weights of shape (2 * height - 1, 2 * width - 1, 3) with unit channel sums, absent at zero core or a closed iris, or color and numeric refusals.
        """
        if self.core == 0.0 or self.iris.obstruction == 1.0:
            return Ok(Nothing)
        shape = (2 * size[0] - 1, 2 * size[1] - 1, 3)
        rows = table()
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
                footprint = np.asarray(
                    np.sinc(offsets * float(frequency)) if frequency <= 1 else np.divide(np.sinc(phase) * phase * float(1 / frequency), offsets, out=np.ones_like(offsets), where=offsets != 0.0),
                    dtype=np.float64,
                )
                return ZoomFFT(side, (first, first + count * step), m=count, fs=1.0), footprint

            def spread(wavelength: np.float64) -> npt.NDArray[np.float64]:
                frequency = sampling / int(wavelength)
                horizontal, xfootprint = axis(size[1], frequency * aspect)
                vertical, yfootprint = axis(size[0], frequency)
                power = abs(vertical(horizontal(pupil, axis=1), axis=0) * yfootprint[:, None] * xfootprint[None, :]) ** 2
                return np.asarray(power / power.sum(), dtype=np.float64)

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
        return np.asarray(np.clip((smudge - floor) / (smudge.max() - floor), 0.0, 1.0)[: size[0], : size[1]], dtype=np.float32)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DRAWS]


def stream(kind: type[object], *key: str | int) -> int:
    """Returns the draw family of a record kind and key, below 2**24 so a float32 node coordinate holds it exactly."""
    return int.from_bytes(content_hash(kind, msgspec.msgpack.encode(key))[:3], "little")


def uniforms(keys: npt.NDArray[np.int64], family: int, count: int) -> npt.NDArray[np.float64]:
    """Returns `count` stable uniforms per integer key row, each row seeding its own `SeedSequence` spawned from the draw family."""
    rows = np.ascontiguousarray(keys, dtype=np.int64).reshape(len(keys), -1).view(np.uint64)
    return np.array([np.random.default_rng(np.random.SeedSequence(family, spawn_key=tuple(row))).random(count) for row in rows.tolist()], dtype=np.float64).reshape(len(keys), count)


# --- [TILES]


def gaussian(generator: np.random.Generator, side: int, channels: int, deviation: float = 1.0) -> npt.NDArray[np.float32]:
    """Returns a seamless `(side, side, channels)` tile of standardized normal draws low-pass filtered by a Gaussian of `deviation` pixels."""
    draw = generator.standard_normal((channels, side, side), dtype=np.float32)
    return _standardized(np.moveaxis(np.fft.irfft2(ndimage.fourier_gaussian(np.fft.rfft2(draw), sigma=(0.0, deviation, deviation), n=side), s=(side, side)), 0, -1))


def ranks(image: OpenImageIO.ImageBuf) -> Outcome[npt.NDArray[np.float32]]:
    """Returns the first channel of a provider rank image, such as `ImageBufAlgo.bluenoise_image`, as centered unit ranks with rows bottom first."""
    return pixels(image, Depth.UINT16).map(lambda codes: (codes[::-1, :, :1].astype(np.float32) + 0.5) / float(codes[..., :1].size))


# --- [STATISTICS]


def _standardized(plane: npt.NDArray[np.float32 | np.float64]) -> npt.NDArray[np.float32]:
    """Shifts and scales each channel to zero mean and unit second moment."""
    centered = np.asarray(plane, dtype=np.float64) - np.mean(plane, axis=(0, 1), dtype=np.float64)
    return np.ascontiguousarray(centered / np.sqrt(np.mean(centered**2, axis=(0, 1))), dtype=np.float32)


def ranked(plane: npt.NDArray[np.float32 | np.float64]) -> npt.NDArray[np.float32]:
    """Maps each channel to standardized normal scores, averaging tied ranks."""
    values = np.asarray(plane.reshape(-1, plane.shape[-1]), dtype=np.float64)
    return _standardized(ndtri((rankdata(values, axis=0) - 0.5) / values.shape[0]).reshape(plane.shape))


def correlated(plane: npt.NDArray[np.float32]) -> npt.NDArray[np.float64]:
    """Returns per-channel mean products of width, height, diagonal, and antidiagonal neighbor pairs on a wrapped tile, one row per pair."""
    values = np.asarray(plane, dtype=np.float64)
    right, down = np.roll(values, -1, axis=1), np.roll(values, -1, axis=0)
    return np.stack([np.mean(pair, axis=(0, 1)) for pair in (values * right, values * down, values * np.roll(right, -1, axis=0), right * down)])


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Pupil", "Specks", "correlated", "gaussian", "ranked", "ranks", "stream", "uniforms"]
