"""Perspective tilt, affine placement, and seeded camera shake across a scene shutter."""

import math
import sys
from typing import Annotated

from expression import Some
import msgspec

from blender.imaging.chain import Effect, Stage
from blender.imaging.compositing.layers import Affine
from blender.imaging.effects.generators.noise import Octaves
from blender.imaging.geometry.homography import Quad
from blender.imaging.geometry.panoramas import Elevation, PanoramaView, Rectilinear, Turn
from blender.imaging.geometry.stmaps import Extension
from blender.imaging.parameters import Finite, Gated, GraphForm, Motion, NonNegative, Position, Presentation, Quantity, Seed

# --- [TYPES] ----------------------------------------------------------------------------


# --- [MODELS] ---------------------------------------------------------------------------


class Tilt(msgspec.Struct, frozen=True, kw_only=True):
    """Turn of a scene camera viewing its own frame."""

    heading: Annotated[Turn, msgspec.Meta(description="Rightward turn of the camera viewing the frame")] = 0.0
    elevation: Annotated[Elevation, msgspec.Meta(description="Upward turn of the camera viewing the frame")] = 0.0

    def view(self, lens: Rectilinear) -> PanoramaView:
        """Returns inverse and forward tilt mappings through the supplied camera lens."""
        return PanoramaView(source=lens, view=lens, heading=self.heading, elevation=self.elevation)


class Shake(msgspec.Struct, frozen=True, kw_only=True):
    """Seeded camera motion on position, rotation, and scale over time."""

    position: Annotated[tuple[NonNegative, NonNegative], msgspec.Meta(description="Peak offset along each frame axis"), Presentation(soft=Some((0.0, 0.1)), quantity=Some(Quantity.FRAME_GREATER))] = (
        0.01,
        0.01,
    )
    rotation: Annotated[NonNegative, msgspec.Meta(description="Peak turn about the frame center"), Presentation(soft=Some((0.0, math.pi / 18.0)), quantity=Some(Quantity.ANGLE))] = math.radians(5.0)
    scale: Annotated[NonNegative, msgspec.Meta(description="Peak zoom in stops"), Presentation(soft=Some((0.0, 1.0)))] = math.log2(1.1)
    motion: Annotated[Motion, msgspec.Meta(description="Reference clock rate and hold, advancing continuously within exposure")] = Motion(speed=1.0 / 24.0)
    octaves: Annotated[Octaves, msgspec.Meta(description="Fractal detail of the motion")] = Octaves()
    seed: Annotated[Seed, msgspec.Meta(description="Noise variant the motion reads")] = 0


class Transform(Effect, frozen=True, kw_only=True, tag=38):
    """Tilts, places, and shakes a frame with optional motion blur across its scene shutter."""

    stage = Stage.WARP
    title = "Transform"
    pointwise = False
    pin: Annotated[Gated[Quad], msgspec.Meta(title="Corner Pin", description="Project the frame into four independently placed corners before camera tilt and affine placement")] = Gated(
        enabled=False, value=Quad()
    )
    tilt: Annotated[Tilt, msgspec.Meta(title="Perspective", description="Turn of the scene camera viewing the frame")] = Tilt()
    affine: Annotated[Affine, msgspec.Meta(title="Placement", description="Scale, shear, rotation, position, and pivot of the frame")] = Affine()
    travel: Annotated[Position, msgspec.Meta(description="Translation across the exposure as a share of each frame side")] = (0.0, 0.0)
    turn: Annotated[Finite, msgspec.Meta(description="Rotation across the exposure"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    zoom: Annotated[Finite, msgspec.Meta(description="Zoom in stops across the exposure")] = 0.0
    shake: Annotated[Gated[Shake], msgspec.Meta(description="Seeded camera motion over time")] = Gated(enabled=False, value=Shake())
    extension: Annotated[Extension, msgspec.Meta(description="Content sampled past the frame edge")] = Extension.CLIP
    filtername: Annotated[str, msgspec.Meta(description="Reconstruction filter name supplied by the native sampler"), GraphForm.SETTING] = "triangle"
    filterwidth: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, description="Full filter support width in source pixels")] = 2.0
    motion_blur: Annotated[bool, msgspec.Meta(description="Integrate spatial travel and timed shake across the exposure")] = False


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Shake", "Tilt", "Transform"]
