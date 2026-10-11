"""Spatial and temporal removal of render noise from scene-linear light."""

import sys
from typing import Annotated

from expression import Some
import msgspec

from rasm.imaging.chain import Effect, Stage
from rasm.imaging.compositing.recombination import Terms
from rasm.imaging.parameters import Case, Choice, Gated, Presentation

# --- [TYPES] ----------------------------------------------------------------------------


class Guides(Choice):
    """Render passes guiding a spatial filter."""

    NONE = 1, "None"
    ALBEDO = 2, "Albedo"
    ALBEDO_NORMAL = 3, "Albedo and Normal"


class Prefilter(Choice):
    """Treatment of guides before filtering color."""

    NONE = 1, "None"
    FAST = 2, "Fast"
    ACCURATE = 3, "Accurate"


class Quality(Choice):
    """Spatial filter quality or scene-controlled quality."""

    FOLLOW_SCENE = 1, "Follow Scene"
    HIGH = 2, "High"
    BALANCED = 3, "Balanced"
    FAST = 4, "Fast"


# --- [MODELS] ---------------------------------------------------------------------------


class Filter(msgspec.Struct, frozen=True, kw_only=True):
    """One Open Image Denoise filter over a color input."""

    guides: Annotated[Guides, msgspec.Meta(description="Render passes guiding the filter")] = Guides.ALBEDO_NORMAL
    prefilter: Annotated[Prefilter, msgspec.Meta(description="Treatment of guides before filtering color")] = Prefilter.ACCURATE
    quality: Annotated[Quality, msgspec.Meta(description="Filter quality against its run time")] = Quality.FOLLOW_SCENE


class Frame(Case, frozen=True, kw_only=True, tag=1):
    """Incoming frame denoised as one image."""

    title = "Frame"
    filter: Annotated[Filter, msgspec.Meta(description="Filter over the frame")] = Filter()


class Passes(Case, frozen=True, kw_only=True, tag=2):
    """Light terms filtered before recombination."""

    title = "Light Passes"
    terms: Annotated[Terms[Gated[Filter]], msgspec.Meta(description="Filter per summed term")] = Terms(
        diffuse=Gated(enabled=True, value=Filter()),
        glossy=Gated(enabled=True, value=Filter()),
        transmission=Gated(enabled=True, value=Filter()),
        volume=Gated(enabled=True, value=Filter()),
        emission=Gated(enabled=False, value=Filter()),
        environment=Gated(enabled=False, value=Filter()),
    )
    occlusion: Annotated[Gated[Filter], msgspec.Meta(title="Ambient Occlusion", description="Filter over the occlusion pass")] = Gated(enabled=False, value=Filter())


class Groups(Case, frozen=True, kw_only=True, tag=3):
    """Light-group passes filtered before recombination."""

    title = "Light Groups"
    filter: Annotated[Filter, msgspec.Meta(description="Filter over each light group pass")] = Filter()


class SpatialDenoise(Effect, frozen=True, kw_only=True, tag=19):
    """Guided spatial denoising after optional firefly suppression."""

    stage = Stage.DENOISE
    title = "Denoise"
    pointwise = False
    fireflies: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=1.0, le=10.0)]],
        msgspec.Meta(title="Firefly Threshold", description="Luminance ratio over the fourth brightest ring neighbor marking a firefly"),
        Presentation(decimals=Some(2)),
    ] = Gated(enabled=True, value=2.0)
    target: Annotated[Frame | Passes | Groups, msgspec.Meta(description="Frame or recombined terms to denoise")] = Frame()


class TemporalDenoise(Effect, frozen=True, kw_only=True, tag=20):
    """Motion-warped neighboring frames blended by color similarity."""

    stage = Stage.DENOISE
    title = "Temporal Denoise"
    pointwise = False
    hue: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Hue Threshold", description="Circular hue distance at which a neighbor's weight reaches zero")] = 0.1
    saturation: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Saturation Threshold", description="Chroma distance at which a neighbor's weight reaches zero")] = 0.05
    value: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Value Threshold", description="Value distance at which a neighbor's weight reaches zero")] = 0.05
    max_weight: Annotated[float, msgspec.Meta(ge=0.0, le=1 / 3, description="Largest share contributed by each neighboring frame")] = 0.3
    tolerance: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Motion Tolerance", description="Allowed reverse-motion disagreement in pixels")] = 1.0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Filter", "Frame", "Groups", "Guides", "Passes", "Prefilter", "Quality", "SpatialDenoise", "TemporalDenoise"]
