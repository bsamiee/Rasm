"""Film grain over a normal-score tile and sensor noise on scene-linear light."""

import functools
import math
import sys
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
from blender.imaging.parameters import Case, GraphForm, Motion, Presentation, Quantity, Seed

# --- [TYPES] ----------------------------------------------------------------------------


type Source = Procedural | Scan
type Medium = Film | Sensor

# --- [MODELS] ---------------------------------------------------------------------------


class Procedural(Case, frozen=True, kw_only=True, tag=1):
    """Gaussian grain drawn from a seeded stream and filtered to its diameter."""

    title = "Procedural"
    diameters: ClassVar[int] = 512
    size: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Size", description="Grain diameter as a share of the frame's greater side"),
        Presentation(soft=Some((0.0002, 0.005)), decimals=Some(5), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 2.0 / 1920.0

    @staticmethod
    def key() -> ContentHash:
        """Returns the content key seeding the draw and naming the packed tile."""
        return content_hash(Procedural, providers=(np,))

    @staticmethod
    @functools.cache
    def tile() -> npt.NDArray[np.float32]:
        """Bakes four normal channels on a seamless tile at one texel deviation."""
        side = 2 * Procedural.diameters
        frequency = np.fft.fftfreq(side)
        response = np.exp(-2.0 * np.pi**2 * (frequency[:, None] ** 2 + frequency**2))
        draw = rng(Procedural.key()).standard_normal((4, side, side), dtype=np.float32)
        return np.ascontiguousarray(np.moveaxis(np.fft.irfft2(np.fft.rfft2(draw) * response[:, : side // 2 + 1]) / np.sqrt(np.mean(response**2)), 0, -1), dtype=np.float32)


class Scan(Case, frozen=True, kw_only=True, tag=2):
    """Scanned grain image tiled over the frame."""

    title = "Scan"
    path: Annotated[str, msgspec.Meta(title="File", description="Grain image in a format OpenImageIO reads"), GraphForm.IMAGE]
    span: Annotated[
        float,
        msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Span", description="Tile width as a share of the frame's greater side"),
        Presentation(soft=Some((0.05, 1.0)), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 1.0

    def tile(self, config: PyOpenColorIO.Config) -> ImagingResult[npt.NDArray[np.float32]]:
        """Reads data samples into four normal-score channels, retaining imaging faults."""
        return read_file(self.path, config, colorspace=Some(PyOpenColorIO.ROLE_DATA)).map(
            lambda image: ranked(np.dstack(((color := image.pixels[..., (0, 1, 2) if image.pixels.shape[2] >= 3 else (0, 0, 0)]), color.mean(axis=2, dtype=np.float64))))
        )


class Film(Case, frozen=True, kw_only=True, tag=1):
    """Grain multiplying light by a lognormal factor, weighted by tone."""

    title = "Film"
    field: Annotated[Source, msgspec.Meta(title="Field", description="Procedural grain or a scanned image")] = Procedural()
    granularity: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Granularity", description="Deviation of the log grain factor")] = 0.0
    shadows: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Shadows", description="Grain weight below the shadow edge")] = 1.0
    midtones: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Midtones", description="Grain weight between the shadow and highlight bands")] = 1.0
    highlights: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Highlights", description="Grain weight above the highlight band")] = 1.0
    shadow_edge: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Shadow Edge", description="Luminance where the shadow weight reaches zero"), Presentation(soft=Some((0.0, 0.5)), decimals=Some(3))
    ] = 0.09
    midtone_span: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Midtone Span", description="Luminance from the shadow edge to the highlight band"),
        Presentation(soft=Some((0.0, 2.0)), decimals=Some(3)),
    ] = 0.5 - 0.09
    highlight_span: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Highlight Span", description="Luminance from the highlight band's start to its full weight"),
        Presentation(soft=Some((0.0, 4.0)), decimals=Some(3)),
    ] = 1.0 - 0.5


class Sensor(Case, frozen=True, kw_only=True, tag=2):
    """Shot and read noise added at the deviation each signal sets."""

    title = "Sensor"
    shot: Annotated[
        float, msgspec.Meta(ge=0.0, le=1.0, title="Shot", description="Noise deviation at scene-linear grey from the photon count"), Presentation(soft=Some((0.0, 0.1)), decimals=Some(4))
    ] = 0.1875 * 0.25 * math.sqrt(GREY)
    read: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Read", description="Noise deviation independent of the signal"), Presentation(soft=Some((0.0, 0.05)), decimals=Some(4))] = 0.0


class Grain(Effect, frozen=True, kw_only=True, tag=22):
    """Film grain multiplying light, or sensor noise at the signal's deviation."""

    stage = Stage.GRAIN
    title = "Grain"
    pointwise = False
    space: ClassVar[str] = PyOpenColorIO.ROLE_SCENE_LINEAR
    medium: Annotated[Medium, msgspec.Meta(title="Medium", description="Film grain or sensor noise")] = Film()
    chroma: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Chroma", description="Share of each channel's own noise against noise every channel shares")] = 0.0
    motion: Annotated[Motion, msgspec.Meta(title="Motion", description="Rate the grain or noise changes over time")] = Motion()
    seed: Annotated[Seed, msgspec.Meta(title="Seed", description="Grain or noise of equal statistics the entry draws")] = 0


# --- [OPERATIONS] -----------------------------------------------------------------------


def ranked(plane: npt.NDArray[np.float32 | np.float64]) -> npt.NDArray[np.float32]:
    """Maps each channel to normal scores with average ranks for tied values."""
    values = np.asarray(plane.reshape(-1, plane.shape[-1]), dtype=np.float64)
    return np.ascontiguousarray(ndtri((rankdata(values, axis=0) - 0.5) / values.shape[0]).reshape(plane.shape), dtype=np.float32)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Film", "Grain", "Medium", "Procedural", "Scan", "Sensor", "Source", "ranked"]
