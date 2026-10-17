"""Attenuation and inscattering through a uniform medium over camera-ray lengths."""

from typing import Annotated

from expression import Some
import msgspec

from blender.imaging.chain import Effect, GREY, Stage
from blender.imaging.effects.generators.noise import Basis, Perlin
from blender.imaging.parameters import Case, FrameLength, Gated, Motion, Presentation, Quantity, Rgb, Seed, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type Inscattering = Ambient | Diffused

# --- [MODELS] ---------------------------------------------------------------------------


class Ambient(Case, frozen=True, kw_only=True, tag=1):
    """Light of one color scattered toward the camera by a uniformly lit medium."""

    color: Annotated[Rgb, msgspec.Meta(description="Scene-linear light the medium scatters toward the camera")] = Rgb(**dict.fromkeys(Rgb.__struct_fields__, (217 / 255) ** 2.2))


class Diffused(Case, frozen=True, kw_only=True, tag=2):
    """Frame's own light spread through the medium and tinted."""

    tint: Annotated[Rgb, msgspec.Meta(description="Scene-linear factor on the spread light")] = Rgb(**dict.fromkeys(Rgb.__struct_fields__, GREY))
    radius: Annotated[FrameLength, msgspec.Meta(description="Gaussian size of the spread light")] = 0.75


class Fluctuation(msgspec.Struct, frozen=True, kw_only=True):
    """Seeded procedural variation of the medium's opacity."""

    variation: Annotated[Unit, msgspec.Meta(description="Maximum share of the medium's opacity removed by the field")] = 1.0
    basis: Annotated[Basis, msgspec.Meta(description="Procedural field varying the medium's opacity")] = Perlin()
    motion: Annotated[Motion, msgspec.Meta(description="Clock evolving the field in feature lengths")] = Motion(speed=0.0)
    seed: Annotated[Seed, msgspec.Meta(description="Field of equal statistics the medium samples")] = 0


class Atmosphere(Effect, frozen=True, kw_only=True, tag=23):
    """Light lost to and scattered from a uniform medium along each camera ray."""

    stage = Stage.ATMOSPHERE
    pointwise = False
    inscatter: Annotated[Inscattering, msgspec.Meta(title="Inscattering", description="Light the medium scatters toward the camera")] = Ambient()
    noise: Annotated[Gated[Fluctuation], msgspec.Meta(description="Seeded procedural reduction of the medium's opacity")] = Gated(enabled=False, value=Fluctuation())
    attenuation: Annotated[
        float, msgspec.Meta(gt=0.0, title="Attenuation Length", description="Distance through the medium that passes 1/e of the light behind it"), Presentation(quantity=Some(Quantity.LENGTH))
    ] = 30.48
    start: Annotated[float, msgspec.Meta(ge=0.0, description="Distance from the camera where the medium begins"), Presentation(quantity=Some(Quantity.LENGTH))] = 0.0
    cutoff: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=0.0)]], msgspec.Meta(description="Ray distance past which supplied depth samples keep their light"), Presentation(quantity=Some(Quantity.LENGTH))
    ] = Gated(enabled=False, value=914.4)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Ambient", "Atmosphere", "Diffused", "Fluctuation", "Inscattering"]
