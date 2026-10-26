"""Film grain over a normal-score tile and sensor noise on scene-linear light."""

import functools
import math
from typing import Annotated, ClassVar

from expression import Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
from scipy.special import ndtri
from scipy.stats import rankdata

from blender.imaging.chain import Effect, GREY, Stage
from blender.imaging.faults import ImagingResult
from blender.imaging.files.images import read_file
from blender.imaging.hashing import content_hash, ContentHash, rng
from blender.imaging.parameters import Case, FrameExtent, GraphForm, Motion, NonNegative, Presentation, Seed, Unit

# --- [TYPES] ----------------------------------------------------------------------------


type Source = Procedural | Scan
type Medium = Film | Sensor
type Deviation = Annotated[float, msgspec.Meta(ge=0.0, le=float(np.sqrt(np.finfo(np.float32).max)))]

# --- [MODELS] ---------------------------------------------------------------------------


class Procedural(Case, frozen=True, kw_only=True, tag=1):
    """Seeded Gaussian grain filtered to its diameter."""

    diameters: ClassVar[int] = 512
    side: ClassVar[int] = 2 * diameters
    size: Annotated[FrameExtent, msgspec.Meta(description="Grain diameter over greater frame side"), Presentation(soft=Some((0.0002, 0.005)), precision=Some(5))] = 2.0 / 1920.0

    @staticmethod
    def key() -> ContentHash:
        """Returns content key seeding draw."""
        return content_hash(Procedural, providers=(np,))

    @staticmethod
    @functools.cache
    def packed() -> ContentHash:
        """Returns baked tile content key naming packed image."""
        return content_hash(Procedural, arrays=(Procedural.tile(),))

    @staticmethod
    @functools.cache
    def tile() -> npt.NDArray[np.float32]:
        """Bakes seamless tile of four standardized filtered normal channels."""
        frequency = np.fft.fftfreq(Procedural.side)
        response = np.exp(-2.0 * np.pi**2 * (frequency[:, None] ** 2 + frequency**2))
        draw = rng(Procedural.key()).standard_normal((4, Procedural.side, Procedural.side), dtype=np.float32)
        return standardized(np.moveaxis(np.fft.irfft2(np.fft.rfft2(draw) * response[:, : Procedural.side // 2 + 1]), 0, -1))

    @staticmethod
    @functools.cache
    def correlation() -> npt.NDArray[np.float64]:
        """Returns baked tile's neighbor correlations."""
        return correlated(Procedural.tile())


class Scan(Case, frozen=True, kw_only=True, tag=2):
    """Scanned grain image tiled over frame."""

    path: Annotated[str, msgspec.Meta(title="File", description="Image OpenImageIO reads"), GraphForm.IMAGE]
    span: Annotated[FrameExtent, msgspec.Meta(description="Tile width over greater frame side"), Presentation(soft=Some((0.05, 1.0)))] = 1.0

    def tile(self, config: PyOpenColorIO.Config) -> ImagingResult[npt.NDArray[np.float32]]:
        """Reads data samples into four normal-score channels."""
        return read_file(self.path, config, colorspace=Some(PyOpenColorIO.ROLE_DATA)).map(
            lambda image: ranked(np.dstack(((color := image.pixels[..., (0, 1, 2) if image.pixels.shape[2] >= 3 else (0, 0, 0)]), color.mean(axis=2, dtype=np.float64))))
        )


class Film(Case, frozen=True, kw_only=True, tag=1):
    """Grain multiplying light by a lognormal factor, weighted by tone."""

    field: Annotated[Source, msgspec.Meta(description="Procedural grain or scanned image")] = Procedural()
    granularity: Annotated[Deviation, msgspec.Meta(description="Deviation of log grain factor"), Presentation(soft=Some((0.0, 0.5)))] = 0.0
    shadows: Annotated[Unit, msgspec.Meta(description="Grain weight below shadow edge")] = 1.0
    midtones: Annotated[Unit, msgspec.Meta(description="Grain weight between shadow and highlight bands")] = 1.0
    highlights: Annotated[Unit, msgspec.Meta(description="Grain weight above highlight band")] = 1.0
    shadow_edge: Annotated[NonNegative, msgspec.Meta(description="Luminance zeroing shadow weight"), Presentation(soft=Some((0.0, 0.5)), precision=Some(3))] = 0.09
    midtone_span: Annotated[NonNegative, msgspec.Meta(description="Luminance from shadow edge to highlight band"), Presentation(soft=Some((0.0, 2.0)), precision=Some(3))] = 0.5 - 0.09
    highlight_span: Annotated[NonNegative, msgspec.Meta(description="Luminance from highlight start to full weight"), Presentation(soft=Some((0.0, 4.0)), precision=Some(3))] = 1.0 - 0.5


class Sensor(Case, frozen=True, kw_only=True, tag=2):
    """Shot noise scaling with signal and constant read noise."""

    shot: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=float(np.float32(math.sqrt(float(np.finfo(np.float32).max) * GREY))), description="Photon noise deviation at scene-linear gray"),
        Presentation(soft=Some((0.0, 0.1)), precision=Some(4)),
    ] = 0.1875 * 0.25 * math.sqrt(GREY)
    read: Annotated[Deviation, msgspec.Meta(description="Signal-independent deviation"), Presentation(soft=Some((0.0, 0.05)), precision=Some(4))] = 0.0


class Grain(Effect, frozen=True, kw_only=True, tag=22):
    """Multiplicative film grain or additive sensor noise."""

    stage = Stage.GRAIN
    pointwise = False
    medium: Annotated[Medium, msgspec.Meta(description="Film grain or sensor noise")] = Film()
    chroma: Annotated[Unit, msgspec.Meta(description="Per-channel noise share against shared noise")] = 0.0
    motion: Annotated[Motion, msgspec.Meta(description="Pattern change clock")] = Motion()
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting pattern")] = 0


# --- [OPERATIONS] -----------------------------------------------------------------------


def standardized(plane: npt.NDArray[np.float32 | np.float64]) -> npt.NDArray[np.float32]:
    """Shifts and scales each channel to zero mean and unit second moment."""
    centered = np.asarray(plane, dtype=np.float64) - np.mean(plane, axis=(0, 1), dtype=np.float64)
    return np.ascontiguousarray(centered / np.sqrt(np.mean(centered**2, axis=(0, 1))), dtype=np.float32)


def ranked(plane: npt.NDArray[np.float32 | np.float64]) -> npt.NDArray[np.float32]:
    """Maps each channel to standardized normal scores, averaging tied ranks."""
    values = np.asarray(plane.reshape(-1, plane.shape[-1]), dtype=np.float64)
    return standardized(ndtri((rankdata(values, axis=0) - 0.5) / values.shape[0]).reshape(plane.shape))


def correlated(plane: npt.NDArray[np.float32]) -> npt.NDArray[np.float64]:
    """Returns per-channel mean products of bilinear neighbor pairs on wrapped tile.

    Args:
        plane: Tile rows by columns by channels.

    Returns:
        Rows for width, height, diagonal, and antidiagonal neighbors, one column per channel.
    """
    values = np.asarray(plane, dtype=np.float64)
    right, down = np.roll(values, -1, axis=1), np.roll(values, -1, axis=0)
    return np.stack([np.mean(pair, axis=(0, 1)) for pair in (values * right, values * down, values * np.roll(right, -1, axis=0), right * down)])


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Deviation", "Film", "Grain", "Medium", "Procedural", "Scan", "Sensor", "Source", "correlated", "ranked", "standardized"]
