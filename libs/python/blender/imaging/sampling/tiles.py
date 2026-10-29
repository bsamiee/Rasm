"""Seeded draws and seamless noise tiles: draw families, stable uniforms per key, filtered Gaussian tiles, centered rank tiles of provider images, and tile statistics."""

import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
from scipy.special import ndtri
from scipy.stats import rankdata

from blender.imaging.files.buffers import Depth, pixels
from blender.records.faults import Outcome
from blender.records.identity import content_hash

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
    frequency = np.fft.fftfreq(side)
    response = np.exp(-2.0 * np.pi**2 * deviation**2 * (frequency[:, None] ** 2 + frequency**2))
    draw = generator.standard_normal((channels, side, side), dtype=np.float32)
    return _standardized(np.moveaxis(np.fft.irfft2(np.fft.rfft2(draw) * response[:, : side // 2 + 1], s=(side, side)), 0, -1))


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

__all__ = ["correlated", "gaussian", "ranked", "ranks", "stream", "uniforms"]
