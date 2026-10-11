"""Attenuation and inscattering through a uniform medium over camera-ray lengths."""

from typing import Annotated, ClassVar

from expression import Some
import msgspec
import PyOpenColorIO

from rasm.imaging.chain import Effect, GREY, Stage
from rasm.imaging.effects.generators.noise import Basis, Perlin
from rasm.imaging.parameters import Case, Gated, Motion, Presentation, Quantity, Rgb, Seed

# --- [TYPES] ----------------------------------------------------------------------------

type Inscattering = Ambient | Diffused

# --- [MODELS] ---------------------------------------------------------------------------


class Ambient(Case, frozen=True, kw_only=True, tag=1):
    """Light of one color scattered toward the camera by a uniformly lit medium."""

    title = "Ambient"
    color: Annotated[Rgb, msgspec.Meta(title="Color", description="Scene-linear light the medium scatters toward the camera")] = Rgb(**dict.fromkeys(Rgb.__struct_fields__, (217 / 255) ** 2.2))


class Diffused(Case, frozen=True, kw_only=True, tag=2):
    """Frame's own light spread through the medium and tinted."""

    title = "Diffused"
    tint: Annotated[Rgb, msgspec.Meta(title="Tint", description="Scene-linear factor on the spread light")] = Rgb(**dict.fromkeys(Rgb.__struct_fields__, GREY))
    radius: Annotated[float, msgspec.Meta(ge=0.0, title="Radius", description="Gaussian size of the spread light"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = 0.75


class Fluctuation(msgspec.Struct, frozen=True, kw_only=True):
    """Seeded procedural variation of the medium's opacity."""

    variation: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Variation", description="Maximum share of the medium's opacity removed by the field")] = 1.0
    basis: Annotated[Basis, msgspec.Meta(title="Basis", description="Procedural field varying the medium's opacity")] = Perlin()
    motion: Annotated[Motion, msgspec.Meta(title="Motion", description="Clock evolving the field in feature lengths")] = Motion(speed=0.0)
    seed: Annotated[Seed, msgspec.Meta(title="Seed", description="Field of equal statistics the medium samples")] = 0


class Atmosphere(Effect, frozen=True, kw_only=True, tag=23):
    """Light lost to and scattered from a uniform medium along each camera ray."""

    stage = Stage.ATMOSPHERE
    title = "Atmosphere"
    pointwise = False
    space: ClassVar[str] = PyOpenColorIO.ROLE_SCENE_LINEAR
    inscatter: Annotated[Inscattering, msgspec.Meta(title="Inscattering", description="Light the medium scatters toward the camera")] = Ambient()
    noise: Annotated[Gated[Fluctuation], msgspec.Meta(title="Noise", description="Seeded procedural reduction of the medium's opacity")] = Gated(enabled=False, value=Fluctuation())
    attenuation: Annotated[
        float, msgspec.Meta(gt=0.0, title="Attenuation Length", description="Distance through the medium that passes 1/e of the light behind it"), Presentation(quantity=Some(Quantity.LENGTH))
    ] = 30.48
    start: Annotated[float, msgspec.Meta(ge=0.0, title="Start", description="Distance from the camera where the medium begins"), Presentation(quantity=Some(Quantity.LENGTH))] = 0.0
    cutoff: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=0.0)]],
        msgspec.Meta(title="Cutoff", description="Ray distance past which supplied depth samples keep their light"),
        Presentation(quantity=Some(Quantity.LENGTH)),
    ] = Gated(enabled=False, value=914.4)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Ambient", "Atmosphere", "Diffused", "Fluctuation", "Inscattering"]
