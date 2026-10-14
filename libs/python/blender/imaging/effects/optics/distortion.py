"""Circular radial lens geometry and wavelength-dependent aberration."""

from collections.abc import Callable
import math
import sys
from typing import Annotated, ClassVar

from expression import Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.imaging.chain import Effect, Stage
from blender.imaging.effects.optics.defocus import Aperture
from blender.imaging.geometry.stmaps import Extension
from blender.imaging.parameters import Choice, GraphForm, Presentation, Quantity, Rgb

# --- [TYPES] ----------------------------------------------------------------------------


class Direction(Choice):
    """Whether a map distorts a rectilinear image or undistorts a lens image."""

    DISTORT = 1, "Distort"
    UNDISTORT = 2, "Undistort"


# --- [MODELS] ---------------------------------------------------------------------------


class Radial(msgspec.Struct, frozen=True, kw_only=True):
    """Radial distortion and zoom normalized by the displayed frame's greater side."""

    distortion: Annotated[float, msgspec.Meta(ge=-sys.float_info.max, le=sys.float_info.max)]
    scale: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max)]
    aspect: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max)]

    @property
    def extent(self) -> npt.NDArray[np.float64]:
        """Frame sides as fractions of the displayed greater side."""
        return np.array((self.aspect, 1.0)) / max(self.aspect, 1.0)

    def to_rectilinear(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps lens positions to rectilinear samples, retaining only real-branch coverage."""
        offset, offset_power = np.frexp(samples[..., :2] - 0.5)
        extent, extent_power = np.frexp(self.extent)
        scale, scale_power = math.frexp(self.scale)
        strength, strength_power = math.frexp(math.sqrt(abs(self.distortion)))
        radial, radial_power = np.frexp(offset * extent * scale * strength)
        radial_power = radial_power + offset_power + extent_power + scale_power + strength_power + 2
        common = np.maximum(0, np.max(radial_power, axis=-1, keepdims=True, where=radial != 0.0, initial=0))
        radius = np.hypot.reduce(np.ldexp(radial, radial_power - common), axis=-1, keepdims=True)
        if self.distortion < 0.0:
            unit = np.ldexp(np.ones_like(radius), -common)
            denominator = unit + np.hypot(unit, radius)
            position = np.ldexp(2.0 * offset * scale / denominator, offset_power + scale_power - common)
            return np.concatenate((0.5 + position, samples[..., 2:]), axis=-1)
        radius = np.ldexp(radius, common, out=np.zeros_like(radius), where=common <= 1)
        covered = np.logical_and(common <= 1, radius <= 1.0)
        root = np.sqrt((1.0 - radius) * (1.0 + radius), out=np.zeros_like(radius), where=covered)
        position = np.ldexp(2.0 * offset * scale / (1.0 + root), offset_power + scale_power, out=np.zeros_like(offset), where=covered)
        return np.concatenate((0.5 + position, samples[..., 2:] * covered), axis=-1)

    def to_lens(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps rectilinear positions to lens samples on the forward map's inverse branch."""
        offset, offset_power = np.frexp(samples[..., :2] - 0.5)
        extent, extent_power = np.frexp(self.extent)
        strength, strength_power = math.frexp(math.sqrt(abs(self.distortion)))
        radial, radial_power = np.frexp(offset * extent * strength)
        radial_power = radial_power + offset_power + extent_power + strength_power + 1
        common = np.maximum(0, np.max(radial_power, axis=-1, keepdims=True, where=radial != 0.0, initial=0))
        radius = np.hypot.reduce(np.ldexp(radial, radial_power - common), axis=-1, keepdims=True)
        radius = np.ldexp(radius, common, out=np.zeros_like(radius), where=common <= 1)
        covered = np.logical_and(common <= 1, radius < 1.0 if self.distortion < 0.0 else radius <= 1.0)
        scale, scale_power = math.frexp(self.scale)
        position = np.ldexp(np.divide(offset, (1.0 + np.copysign(radius**2, self.distortion)) * scale, out=np.zeros_like(offset), where=covered), offset_power - scale_power)
        return np.concatenate((0.5 + position, samples[..., 2:] * covered), axis=-1)


class Distortion(Effect, frozen=True, kw_only=True, tag=25):
    """Barrel or pincushion lens distortion with edge or corner framing."""

    stage = Stage.DISTORTION
    title = "Lens Distortion"
    pointwise = False
    distortion: Annotated[float, msgspec.Meta(gt=-1.0, le=sys.float_info.max, description="Barrel above 0, pincushion below 0"), Presentation(soft=Some((-0.999, 1.0)))] = 0.05
    fit: Annotated[bool, msgspec.Meta(description="Scale a barrel distortion toward the frame corners")] = False
    direction: Annotated[Direction, msgspec.Meta(description="Distort a rectilinear image or undistort a lens image")] = Direction.DISTORT
    extension: Annotated[Extension, msgspec.Meta(description="Sampling past the frame edge on both axes")] = Extension.CLIP

    def mapping(self, aspect: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max)]) -> Callable[[npt.NDArray[np.float64]], npt.NDArray[np.float64]]:
        """Returns the selected mapping at the displayed width over height."""
        smaller = min(aspect, 1.0) / max(aspect, 1.0)
        reach = 1.0 + smaller**2 if self.fit and self.distortion > 0.0 else 1.0
        radial = Radial(distortion=self.distortion, scale=(1.0 / (1.0 / reach + self.distortion)) / reach, aspect=aspect)
        match self.direction:
            case Direction.DISTORT:
                return radial.to_rectilinear
            case Direction.UNDISTORT:
                return radial.to_lens


class Aberration(Effect, frozen=True, kw_only=True, tag=26):
    """Lateral spectral integration about green followed by red and blue iris defocus."""

    stage = Stage.DISTORTION
    title = "Chromatic Aberration"
    pointwise = False
    defocus: ClassVar[Rgb] = Rgb(red=0.5, green=0.0, blue=1.0)
    dispersion: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Spread of image height from red to blue"), Presentation(soft=Some((0.0, 1.0)))] = 0.05
    samples: Annotated[int, msgspec.Meta(ge=2, description="Wavelengths integrated from red to blue"), GraphForm.SETTING] = 8
    axial: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Blue defocus radius, red at half of it and green in focus"),
        Presentation(soft=Some((0.0, 0.05)), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 0.0
    iris: Annotated[Aperture, msgspec.Meta(description="Iris outline of the red and blue defocus")] = Aperture()
    extension: Annotated[Extension, msgspec.Meta(description="Sampling past the frame edge on both axes")] = Extension.EXTEND

    @property
    def spectrum(self) -> tuple[tuple[float, Rgb], ...]:
        """Distortion coefficients and channel-normalized midpoint wavelength weights."""
        waves = (np.arange(self.samples, dtype=np.float64) + 0.5) / self.samples
        across = 2.0 * waves - 1.0
        weights = np.stack((np.maximum(-across, 0.0), 1.0 - np.abs(across), np.maximum(across, 0.0)), axis=-1)
        weights /= weights.sum(axis=0)
        return tuple((float(-0.25 * position), Rgb(red=float(weight[0]), green=float(weight[1]), blue=float(weight[2]))) for position, weight in zip(across, weights))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Aberration", "Direction", "Distortion", "Radial"]
