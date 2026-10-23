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
    """Ghost color sampled per aperture image or continuously along axis."""

    INSTANCE = 1, "Per Ghost"
    AXIS = 2, "Along Axis"


type Source = FramePosition | BoundObject

# --- [MODELS] ---------------------------------------------------------------------------


class FramePosition(Case, frozen=True, kw_only=True, tag=1):
    """Source at frame position, needing no camera or depth."""

    position: Annotated[Position, msgspec.Meta(description="Source in frame fractions")] = (0.3, 0.6)


class BoundObject(Case, frozen=True, kw_only=True, tag=2):
    """Bound scene object read as location or local +Z direction."""

    title = "Object"
    reading: Annotated[Reading, msgspec.Meta(description="Location or local +Z toward distant light")] = Reading.POSITION
    occlusion: Annotated[bool, msgspec.Meta(description="Nearer surfaces hide source")] = True


class LightRays(Effect, frozen=True, kw_only=True, tag=35):
    """Highlight shafts marched toward a source through gaps in nearer surfaces."""

    stage = Stage.GENERATE
    pointwise = False
    source: Annotated[Source, msgspec.Meta(description="Point shafts converge on")] = FramePosition()
    highlights: Annotated[Highlights, msgspec.Meta(description="Light streaming toward source")] = Highlights()
    length: Annotated[FrameLength, msgspec.Meta(description="Longest march over greater frame side"), Presentation(soft=Some((0.0, 1.0)))] = 0.5
    jitter: Annotated[Unit, msgspec.Meta(description="Share of march steps skipped at hashed shifts")] = 0.0
    color: Annotated[Rgb[Finite], msgspec.Meta(description="Scene-linear shaft tint and strength")] = Rgb(red=1.0, green=1.0, blue=1.0)


class Hotspot(msgspec.Struct, frozen=True, kw_only=True):
    """Source disc graded from center to rim."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from disc center to rim")] = Emission(
        spectrum=Tint(start=Rgb(red=1.0, green=0.9, blue=0.8), end=Rgb(red=1.0, green=0.9, blue=0.8)), intensity=3.0
    )
    softness: Annotated[FrameLength, msgspec.Meta(description="Edge ramp past one pixel over greater frame side"), Presentation(soft=Some((0.0, 0.05)))] = 0.01


class Starburst(msgspec.Struct, frozen=True, kw_only=True):
    """Radial streaks with periodic angular noise and quadratic length falloff."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from streak root to tip")] = Emission(
        spectrum=Tint(start=Rgb(red=1.0, green=0.9, blue=0.8), end=Rgb(red=1.0, green=0.9, blue=0.8)), intensity=0.0
    )
    basis: Annotated[Basis, msgspec.Meta(title="Noise", description="Texture forming streaks")] = Perlin(
        size=1.0, stretch=1.0, octaves=Octaves(roughness=0.563), distortion=0.0, cut=Cut(interpolation=Interpolation.LINEAR)
    )
    length: Annotated[FrameLength, msgspec.Meta(description="Streak reach over greater frame side")] = 0.25
    density: Annotated[NonNegative, msgspec.Meta(description="Noise cells around source"), Presentation(soft=Some((0.0, 64.0)))] = 24.0
    sharpness: Annotated[AtLeastOne, msgspec.Meta(description="Exponent narrowing each streak"), Presentation(soft=Some((1.0, 8.0)))] = 3.0
    motion: Annotated[Motion, msgspec.Meta(description="Clock advancing streaks through noise cells")] = Motion(speed=0.0)


class Anamorphic(msgspec.Struct, frozen=True, kw_only=True):
    """Bar through source along cylindrical lens squeeze axis."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from bar center to ends")] = Emission(
        spectrum=Tint(start=Rgb(red=0.04, green=0.34, blue=1.0), end=Rgb(red=0.04, green=0.34, blue=1.0)), intensity=0.0
    )
    length: Annotated[FrameLength, msgspec.Meta(description="Bar half length over greater frame side")] = 0.6
    thickness: Annotated[FrameLength, msgspec.Meta(description="Bar width over greater frame side"), Presentation(soft=Some((0.0, 0.02)))] = 0.004
    angle: Annotated[Finite, msgspec.Meta(description="Counterclockwise squeeze axis turn"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class Halo(msgspec.Struct, frozen=True, kw_only=True):
    """Ring centered along axis from frame center through source."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from inner to outer ring edge")] = Emission(
        spectrum=Tint(start=Rgb(red=0.124, green=0.009, blue=0.194), end=Rgb(red=0.124, green=0.009, blue=0.194)), intensity=0.0
    )
    magnification: Annotated[Finite, msgspec.Meta(description="Ring offset from frame center over source offset"), Presentation(soft=Some((-2.0, 2.0)))] = -0.4
    radius: Annotated[FrameLength, msgspec.Meta(description="Ring radius over greater frame side")] = 0.2
    width: Annotated[FrameLength, msgspec.Meta(description="Ring width over greater frame side"), Presentation(soft=Some((0.0, 0.1)))] = 0.02
    softness: Annotated[FrameLength, msgspec.Meta(description="Edge ramp past one pixel over greater frame side"), Presentation(soft=Some((0.0, 0.1)))] = 0.02


class Ghosts(msgspec.Struct, frozen=True, kw_only=True):
    """Aperture images along source axis with seeded spacing and conserved area gain."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from first ghost to last")] = Emission(
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
    grading: Annotated[Grading, msgspec.Meta(description="Spectrum sampling"), GraphForm.SETTING] = Grading.INSTANCE
    squeeze: Annotated[Positive, msgspec.Meta(description="Horizontal aperture image compression"), Presentation(soft=Some((1.0, 2.0)))] = 1.0
    fringe: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, description="Channel radius shift, toward blue above zero")] = 0.0
    count: Annotated[int, msgspec.Meta(ge=1, description="Aperture images along axis"), Presentation(soft=Some((1.0, 8.0))), GraphForm.SETTING] = 5
    first: Annotated[Finite, msgspec.Meta(description="First ghost magnification"), Presentation(soft=Some((-2.0, 2.0)))] = 0.6
    last: Annotated[Finite, msgspec.Meta(description="Last ghost magnification"), Presentation(soft=Some((-2.0, 2.0)))] = -1.0
    radius: Annotated[FrameLength, msgspec.Meta(description="Largest ghost radius over greater frame side"), Presentation(soft=Some((0.0, 0.2)))] = 0.03
    variation: Annotated[float, msgspec.Meta(ge=0.0, lt=1.0, description="Size and position jitter within slots")] = 0.5
    softness: Annotated[Unit, msgspec.Meta(description="Edge ramp over ghost radius")] = 0.3


class LensFlare(Effect, frozen=True, kw_only=True, tag=36):
    """Source disc, radial streaks, anamorphic bar, halo, and ghosts generated as light."""

    stage = Stage.GENERATE
    pointwise = False
    source: Annotated[Source, msgspec.Meta(description="Light the lens images")] = FramePosition()
    radius: Annotated[FrameLength, msgspec.Meta(description="Source disc radius over greater frame side"), Presentation(soft=Some((0.0, 0.05)))] = 0.01
    hotspot: Annotated[Gated[Hotspot], msgspec.Meta(description="Source disc")] = Gated(enabled=True, value=Hotspot())
    starburst: Annotated[Gated[Starburst], msgspec.Meta(description="Radial streaks about source")] = Gated(enabled=True, value=Starburst())
    anamorphic: Annotated[Gated[Anamorphic], msgspec.Meta(title="Anamorphic Bar", description="Line through source along squeeze axis")] = Gated(enabled=False, value=Anamorphic())
    halo: Annotated[Gated[Halo], msgspec.Meta(description="Ring centered along source axis")] = Gated(enabled=True, value=Halo())
    ghosts: Annotated[Gated[Ghosts], msgspec.Meta(description="Aperture images along source axis")] = Gated(enabled=True, value=Ghosts())
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting streak and ghost pattern")] = 0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Anamorphic", "BoundObject", "FramePosition", "Ghosts", "Grading", "Halo", "Hotspot", "LensFlare", "LightRays", "Reading", "Source", "Starburst"]
