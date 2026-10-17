"""Light shafts and lens flares generated about a frame or scene source."""

from typing import Annotated

from expression import Some
import msgspec

from blender.imaging.chain import Effect, Stage
from blender.imaging.effects.generators.noise import Basis, Cut, Emission, Gradient, Interpolation, Octaves, Perlin, Tint
from blender.imaging.effects.optics.defocus import Aperture
from blender.imaging.effects.optics.glare import Highlights
from blender.imaging.parameters import AtLeastOne, Case, Choice, Finite, FrameLength, Gated, GraphForm, Motion, NonNegative, Position, Positive, Presentation, Quantity, Ramp, Rgb, Seed, Stop, Unit

# --- [TYPES] ----------------------------------------------------------------------------


class Reading(Choice):
    """Object location or direction toward a light at infinity."""

    POSITION = 1
    DIRECTION = 2


class Grading(Choice):
    """Ghost color sampled per aperture image or continuously along the axis."""

    INSTANCE = 1, "Per Ghost"
    AXIS = 2, "Along Axis"


type Source = FramePosition | BoundObject

# --- [MODELS] ---------------------------------------------------------------------------


class FramePosition(Case, frozen=True, kw_only=True, tag=1):
    """Source at a frame position without a camera or depth dependency."""

    position: Annotated[Position, msgspec.Meta(description="Source as a share of each frame side")] = (0.3, 0.6)


class BoundObject(Case, frozen=True, kw_only=True, tag=2):
    """Bound scene object projected as a location or along its local positive Z axis."""

    title = "Object"
    reading: Annotated[Reading, msgspec.Meta(description="Object location or its local positive Z direction toward a distant light")] = Reading.POSITION
    occlusion: Annotated[bool, msgspec.Meta(description="Surfaces nearer than the source hide its light")] = True


class LightRays(Effect, frozen=True, kw_only=True, tag=35):
    """Highlight shafts marched toward a source through gaps in nearer surfaces."""

    stage = Stage.GENERATE
    pointwise = False
    source: Annotated[Source, msgspec.Meta(description="Point the shafts converge on")] = FramePosition()
    highlights: Annotated[Highlights, msgspec.Meta(description="Light that streams toward the source")] = Highlights()
    length: Annotated[FrameLength, msgspec.Meta(description="Longest march as a share of the frame's greater side"), Presentation(soft=Some((0.0, 1.0)))] = 0.5
    jitter: Annotated[Unit, msgspec.Meta(description="Share of march steps skipped at hashed shifts")] = 0.0
    color: Annotated[Rgb[Finite], msgspec.Meta(description="Scene-linear tint and strength of the shafts")] = Rgb(red=1.0, green=1.0, blue=1.0)


class Hotspot(msgspec.Struct, frozen=True, kw_only=True):
    """Source disc with light graded from its center to its rim."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from the disc's center to its rim")] = Emission(
        spectrum=Tint(start=Rgb(red=1.0, green=0.9, blue=0.8), end=Rgb(red=1.0, green=0.9, blue=0.8)), intensity=3.0
    )
    softness: Annotated[FrameLength, msgspec.Meta(description="Edge ramp beyond one pixel as a share of the frame's greater side"), Presentation(soft=Some((0.0, 0.05)))] = 0.01


class Starburst(msgspec.Struct, frozen=True, kw_only=True):
    """Radial streaks with periodic angular noise and quadratic length falloff."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from each streak's root to its tip")] = Emission(
        spectrum=Tint(start=Rgb(red=1.0, green=0.9, blue=0.8), end=Rgb(red=1.0, green=0.9, blue=0.8)), intensity=0.0
    )
    basis: Annotated[Basis, msgspec.Meta(title="Noise", description="Texture sampled around the source to form its streaks")] = Perlin(
        size=1.0, stretch=1.0, octaves=Octaves(roughness=0.563), distortion=0.0, cut=Cut(interpolation=Interpolation.LINEAR)
    )
    length: Annotated[FrameLength, msgspec.Meta(description="Streak reach as a share of the frame's greater side")] = 0.25
    density: Annotated[NonNegative, msgspec.Meta(description="Noise cells around the source"), Presentation(soft=Some((0.0, 64.0)))] = 24.0
    sharpness: Annotated[AtLeastOne, msgspec.Meta(description="Exponent narrowing each streak"), Presentation(soft=Some((1.0, 8.0)))] = 3.0
    motion: Annotated[Motion, msgspec.Meta(description="Clock advancing the streak pattern through noise cells")] = Motion(speed=0.0)


class Anamorphic(msgspec.Struct, frozen=True, kw_only=True):
    """Bar through the source along a cylindrical lens element's squeeze axis."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from the bar's center to its ends")] = Emission(
        spectrum=Tint(start=Rgb(red=0.04, green=0.34, blue=1.0), end=Rgb(red=0.04, green=0.34, blue=1.0)), intensity=0.0
    )
    length: Annotated[FrameLength, msgspec.Meta(description="Bar half length as a share of the frame's greater side")] = 0.6
    thickness: Annotated[FrameLength, msgspec.Meta(description="Bar width as a share of the frame's greater side"), Presentation(soft=Some((0.0, 0.02)))] = 0.004
    angle: Annotated[Finite, msgspec.Meta(description="Counter-clockwise rotation of the squeeze axis"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class Halo(msgspec.Struct, frozen=True, kw_only=True):
    """Ring centered along the axis from the frame center through the source."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from the ring's inner edge to its outer edge")] = Emission(
        spectrum=Tint(start=Rgb(red=0.124, green=0.009, blue=0.194), end=Rgb(red=0.124, green=0.009, blue=0.194)), intensity=0.0
    )
    magnification: Annotated[Finite, msgspec.Meta(description="Ring center: 1 the source, 0 the frame center, -1 its mirror"), Presentation(soft=Some((-2.0, 2.0)))] = -0.4
    radius: Annotated[FrameLength, msgspec.Meta(description="Ring radius as a share of the frame's greater side")] = 0.2
    width: Annotated[FrameLength, msgspec.Meta(description="Ring width as a share of the frame's greater side"), Presentation(soft=Some((0.0, 0.1)))] = 0.02
    softness: Annotated[FrameLength, msgspec.Meta(description="Edge ramp beyond one pixel as a share of the frame's greater side"), Presentation(soft=Some((0.0, 0.1)))] = 0.02


class Ghosts(msgspec.Struct, frozen=True, kw_only=True):
    """Aperture images along the source axis with seeded spacing and conserved area gain."""

    emission: Annotated[Emission, msgspec.Meta(description="Light along the chain from its first ghost to its last")] = Emission(
        spectrum=Gradient(
            ramp=Ramp(
                stops=(
                    Stop(position=0.0, color=Rgb(red=0.12, green=0.02, blue=0.2)),
                    Stop(position=0.5, color=Rgb(red=0.02, green=0.1, blue=0.08)),
                    Stop(position=1.0, color=Rgb(red=0.2, green=0.08, blue=0.02)),
                )
            )
        ),
        intensity=0.0,
    )
    iris: Annotated[Aperture, msgspec.Meta(description="Aperture stop imaged by each ghost")] = Aperture()
    grading: Annotated[Grading, msgspec.Meta(description="Spectrum sampled at each ghost's slot or along its axis"), GraphForm.SETTING] = Grading.INSTANCE
    squeeze: Annotated[Positive, msgspec.Meta(description="Horizontal compression of each aperture image"), Presentation(soft=Some((1.0, 2.0)))] = 1.0
    fringe: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, description="Channel radius shift, toward blue above zero")] = 0.0
    count: Annotated[int, msgspec.Meta(ge=1, description="Aperture images along the axis"), Presentation(soft=Some((1.0, 8.0))), GraphForm.SETTING] = 5
    first: Annotated[Finite, msgspec.Meta(description="Magnification at the start of the ghost chain"), Presentation(soft=Some((-2.0, 2.0)))] = 0.6
    last: Annotated[Finite, msgspec.Meta(description="Magnification at the end of the ghost chain"), Presentation(soft=Some((-2.0, 2.0)))] = -1.0
    radius: Annotated[FrameLength, msgspec.Meta(description="Largest ghost radius as a share of the frame's greater side"), Presentation(soft=Some((0.0, 0.2)))] = 0.03
    variation: Annotated[float, msgspec.Meta(ge=0.0, lt=1.0, description="Variation of ghost sizes and positions within their slots")] = 0.5
    softness: Annotated[Unit, msgspec.Meta(description="Edge ramp as a share of each ghost's radius")] = 0.3


class LensFlare(Effect, frozen=True, kw_only=True, tag=36):
    """Source disc, radial streaks, anamorphic bar, halo, and ghosts generated as light."""

    stage = Stage.GENERATE
    pointwise = False
    source: Annotated[Source, msgspec.Meta(description="Light the lens images")] = FramePosition()
    radius: Annotated[FrameLength, msgspec.Meta(description="Source disc radius as a share of the frame's greater side"), Presentation(soft=Some((0.0, 0.05)))] = 0.01
    hotspot: Annotated[Gated[Hotspot], msgspec.Meta(description="Source disc")] = Gated(enabled=True, value=Hotspot())
    starburst: Annotated[Gated[Starburst], msgspec.Meta(description="Radial streaks about the source")] = Gated(enabled=True, value=Starburst())
    anamorphic: Annotated[Gated[Anamorphic], msgspec.Meta(title="Anamorphic Bar", description="Line through the source along the squeeze axis")] = Gated(enabled=False, value=Anamorphic())
    halo: Annotated[Gated[Halo], msgspec.Meta(description="Ring centered along the source axis")] = Gated(enabled=True, value=Halo())
    ghosts: Annotated[Gated[Ghosts], msgspec.Meta(description="Aperture images along the source axis")] = Gated(enabled=True, value=Ghosts())
    seed: Annotated[Seed, msgspec.Meta(description="Streak and ghost pattern of equal statistics")] = 0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Anamorphic", "BoundObject", "FramePosition", "Ghosts", "Grading", "Halo", "Hotspot", "LensFlare", "LightRays", "Reading", "Source", "Starburst"]
