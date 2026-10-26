"""Spatial and temporal removal of render noise from scene-linear light."""

from typing import Annotated

from expression import Some
import msgspec

from blender.imaging.chain import Effect, Stage
from blender.imaging.compositing.recombination import Terms
from blender.imaging.parameters import Case, Choice, Gated, NonNegative, Presentation

# --- [TYPES] ----------------------------------------------------------------------------


class Guides(Choice):
    """Render passes guiding a spatial filter."""

    NONE = 1
    ALBEDO = 2
    ALBEDO_NORMAL = 3, "Albedo and Normal"


class Prefilter(Choice):
    """Guide denoising before color filtering."""

    NONE = 1
    FAST = 2
    ACCURATE = 3


class Quality(Choice):
    """Spatial filter quality or scene-controlled quality."""

    FOLLOW_SCENE = 1
    HIGH = 2
    BALANCED = 3
    FAST = 4


# --- [MODELS] ---------------------------------------------------------------------------


class Filter(msgspec.Struct, frozen=True, kw_only=True):
    """Open Image Denoise filter over color input."""

    guides: Annotated[Guides, msgspec.Meta(description="Passes guiding filter")] = Guides.ALBEDO_NORMAL
    prefilter: Annotated[Prefilter, msgspec.Meta(description="Guide denoising before filtering")] = Prefilter.ACCURATE
    quality: Annotated[Quality, msgspec.Meta(description="Quality against run time")] = Quality.FOLLOW_SCENE


class Frame(Case, frozen=True, kw_only=True, tag=1):
    """Incoming frame denoised as one image."""

    filter: Annotated[Filter, msgspec.Meta(description="Filter over frame")] = Filter()


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
    occlusion: Annotated[Gated[Filter], msgspec.Meta(title="Ambient Occlusion", description="Filter over occlusion pass")] = Gated(enabled=False, value=Filter())


class Groups(Case, frozen=True, kw_only=True, tag=3):
    """Light-group passes filtered before recombination."""

    title = "Light Groups"
    filter: Annotated[Filter, msgspec.Meta(description="Filter per light group pass")] = Filter()


class SpatialDenoise(Effect, frozen=True, kw_only=True, tag=19):
    """Guided spatial denoising after optional firefly suppression."""

    stage = Stage.DENOISE
    title = "Denoise"
    pointwise = False
    fireflies: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=1.0, le=10.0)]], msgspec.Meta(title="Firefly Threshold", description="Luminance over fourth brightest ring neighbor"), Presentation(precision=Some(2))
    ] = Gated(enabled=True, value=2.0)
    target: Annotated[Frame | Passes | Groups, msgspec.Meta(description="Frame or light terms to denoise")] = Frame()


class TemporalDenoise(Effect, frozen=True, kw_only=True, tag=20):
    """Motion-warped neighboring frames blended by color similarity."""

    stage = Stage.DENOISE
    pointwise = False
    hue: Annotated[NonNegative, msgspec.Meta(title="Hue Threshold", description="Hue distance zeroing neighbor weight")] = 0.1
    saturation: Annotated[NonNegative, msgspec.Meta(title="Saturation Threshold", description="Chroma distance zeroing neighbor weight")] = 0.05
    value: Annotated[NonNegative, msgspec.Meta(title="Value Threshold", description="Value distance zeroing neighbor weight")] = 0.05
    max_weight: Annotated[float, msgspec.Meta(ge=0.0, le=1 / 3, description="Largest share per neighbor frame")] = 0.3
    tolerance: Annotated[NonNegative, msgspec.Meta(title="Motion Tolerance", description="Allowed reverse-motion error in pixels")] = 1.0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Filter", "Frame", "Groups", "Guides", "Passes", "Prefilter", "Quality", "SpatialDenoise", "TemporalDenoise"]
