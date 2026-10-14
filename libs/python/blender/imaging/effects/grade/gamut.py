"""Gamut compression toward a colorspace's boundary."""

from typing import Annotated, override

from expression import Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.color.processors import applied, processor
from blender.imaging.faults import ImagingResult, paired
from blender.imaging.parameters import ColorSpace, Presentation

# --- [MODELS] ---------------------------------------------------------------------------


class Channel(msgspec.Struct, frozen=True, kw_only=True):
    """Compression of a channel's distance from the achromatic axis."""

    limit: Annotated[float, msgspec.Meta(ge=1.001, le=65504.0, title="Limit", description="Distance compressed onto the gamut boundary"), Presentation(soft=Some((1.001, 2.0)), decimals=Some(3))]
    threshold: Annotated[float, msgspec.Meta(ge=0.0, le=0.9995, title="Threshold", description="Distance below which values stay unchanged"), Presentation(decimals=Some(3))]


class GamutCompression(Effect, frozen=True, kw_only=True, tag=10):
    """Compresses scene-linear excursions toward a selected gamut boundary."""

    stage = Stage.GAMUT
    title = "Gamut Compression"
    pointwise = True
    space: Annotated[ColorSpace, msgspec.Meta(title="Gamut", description="Colorspace whose boundary values compress toward")] = PyOpenColorIO.ROLE_SCENE_LINEAR
    cyan: Annotated[Channel, msgspec.Meta(title="Cyan", description="Red channel toward cyan")] = Channel(limit=1.147, threshold=0.815)
    magenta: Annotated[Channel, msgspec.Meta(title="Magenta", description="Green channel toward magenta")] = Channel(limit=1.264, threshold=0.803)
    yellow: Annotated[Channel, msgspec.Meta(title="Yellow", description="Blue channel toward yellow")] = Channel(limit=1.312, threshold=0.880)
    power: Annotated[
        float, msgspec.Meta(ge=1.0, le=65504.0, title="Power", description="Sharpness of the curve between threshold and limit"), Presentation(soft=Some((1.0, 2.0)), step=Some(0.01), decimals=Some(3))
    ] = 1.2

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns the exact compression specification for serialization."""
        channels = (self.cyan, self.magenta, self.yellow)
        return Ok(
            Some(
                PyOpenColorIO.FixedFunctionTransform(
                    style=PyOpenColorIO.FIXED_FUNCTION_ACES_GAMUT_COMP_13, params=[*(channel.limit for channel in channels), *(channel.threshold for channel in channels), self.power]
                )
            )
        )

    def compress(self, values: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
        """Returns compressed straight RGB in the declared process space."""
        channels = (self.cyan, self.magenta, self.yellow)
        limit, threshold = np.array([(channel.limit, channel.threshold) for channel in channels], dtype=np.float64).T
        head = 1.0 - threshold
        scale = head / np.power(1.0 - np.power(head / (limit - threshold), self.power), 1.0 / self.power)
        rgb = values.astype(np.float64)
        achromatic = np.max(rgb, axis=-1, keepdims=True)
        magnitude = np.abs(achromatic)
        distance = np.divide(achromatic - rgb, magnitude, out=np.zeros_like(rgb), where=magnitude != 0.0)
        excess = np.maximum(distance - threshold, 0.0)
        lower, upper = np.minimum(excess, scale), np.maximum(excess, scale)
        compressed = threshold + lower / np.power(1.0 + np.power(lower / upper, self.power), 1.0 / self.power)
        return np.where(magnitude == 0.0, 0.0, np.where(excess == 0.0, rgb, achromatic - magnitude * compressed)).astype(np.float32)

    def apply(self, config: PyOpenColorIO.Config, values: npt.NDArray[np.float32]) -> ImagingResult[npt.NDArray[np.float32]]:
        """Returns associated scene-linear RGB or RGBA through stable process-space compression."""

        def converted(conversions: tuple[PyOpenColorIO.Processor, PyOpenColorIO.Processor]) -> npt.NDArray[np.float32]:
            inward, outward = (conversion.getDefaultCPUProcessor() for conversion in conversions)

            def operation(rgb: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
                entering = np.ascontiguousarray(rgb)
                inward.applyRGB(entering)
                compressed = self.compress(entering)
                outward.applyRGB(compressed)
                return compressed

            return applied(operation, values)

        return paired(
            processor(config, PyOpenColorIO.ColorSpaceTransform(src=PyOpenColorIO.ROLE_SCENE_LINEAR, dst=self.space)),
            processor(config, PyOpenColorIO.ColorSpaceTransform(src=self.space, dst=PyOpenColorIO.ROLE_SCENE_LINEAR)),
        ).map(converted)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Channel", "GamutCompression"]
