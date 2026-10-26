"""Camera-ray attenuation and inscattering through a uniform medium."""

from typing import Annotated

import colour
from expression import Some
import msgspec

from blender.imaging.chain import Effect, GREY, Stage
from blender.imaging.effects.generators.noise import Basis, Perlin
from blender.imaging.parameters import Case, FrameLength, Gated, Motion, Presentation, Quantity, Rgb, Seed, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type Inscattering = Ambient | Diffused

# --- [MODELS] ---------------------------------------------------------------------------


class Ambient(Case, frozen=True, kw_only=True, tag=1):
    """Uniformly lit medium scattering one color toward camera."""

    color: Annotated[Rgb, msgspec.Meta(description="Scene-linear scattered light")] = Rgb(**dict.fromkeys(Rgb.__struct_fields__, float(colour.cctf_decoding(217 / 255, function="sRGB"))))


class Diffused(Case, frozen=True, kw_only=True, tag=2):
    """Frame light spread through medium and tinted."""

    tint: Annotated[Rgb, msgspec.Meta(description="Scene-linear spread light factor")] = Rgb(**dict.fromkeys(Rgb.__struct_fields__, GREY))
    radius: Annotated[FrameLength, msgspec.Meta(description="Gaussian spread size")] = 0.75


class Fluctuation(msgspec.Struct, frozen=True, kw_only=True):
    """Seeded medium opacity variation."""

    variation: Annotated[Unit, msgspec.Meta(description="Largest opacity share field removes")] = 1.0
    basis: Annotated[Basis, msgspec.Meta(description="Procedural opacity field")] = Perlin()
    motion: Annotated[Motion, msgspec.Meta(description="Field clock in feature lengths")] = Motion(speed=0.0)
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting pattern")] = 0


class Atmosphere(Effect, frozen=True, kw_only=True, tag=23):
    """Uniform medium absorbing and scattering light along camera rays."""

    stage = Stage.ATMOSPHERE
    pointwise = False
    inscatter: Annotated[Inscattering, msgspec.Meta(title="Inscattering", description="Scattered light source")] = Ambient()
    noise: Annotated[Gated[Fluctuation], msgspec.Meta(description="Seeded opacity reduction")] = Gated(enabled=False, value=Fluctuation())
    attenuation: Annotated[float, msgspec.Meta(gt=0.0, title="Attenuation Length", description="Medium depth passing 1/e of light"), Presentation(quantity=Some(Quantity.LENGTH))] = 30.48
    start: Annotated[float, msgspec.Meta(ge=0.0, description="Camera distance where medium starts"), Presentation(quantity=Some(Quantity.LENGTH))] = 0.0
    cutoff: Annotated[Gated[Annotated[float, msgspec.Meta(ge=0.0)]], msgspec.Meta(description="Ray distance past which samples keep light"), Presentation(quantity=Some(Quantity.LENGTH))] = Gated(
        enabled=False, value=914.4
    )


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Ambient", "Atmosphere", "Diffused", "Fluctuation", "Inscattering"]
