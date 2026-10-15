"""Render lobe and light-group records for rebuilding and relighting beauty."""

from typing import Annotated

from expression import Some
import msgspec

from blender.imaging.parameters import Case, Gated, Presentation, Rgb, Stops, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type Recombination = Lobes | LightGroups

# --- [MODELS] ---------------------------------------------------------------------------


class Terms[V](msgspec.Struct, frozen=True, kw_only=True):
    """One value per summed beauty term, shared by gains and denoising."""

    diffuse: Annotated[V, msgspec.Meta(description="Diffuse light")]
    glossy: Annotated[V, msgspec.Meta(description="Glossy light")]
    transmission: Annotated[V, msgspec.Meta(description="Transmitted light")]
    volume: Annotated[V, msgspec.Meta(description="Volume light")]
    emission: Annotated[V, msgspec.Meta(description="Emitters seen by the camera")]
    environment: Annotated[V, msgspec.Meta(description="Background seen by the camera")]


class Lobes(Case, frozen=True, kw_only=True, tag=1):
    """Light passes rebuilt with independent term gains and ambient occlusion."""

    title = "Light Passes"
    terms: Annotated[Terms[Gated[Stops]], msgspec.Meta(title="Gains", description="Gain in stops per summed term")] = Terms(**dict.fromkeys(Terms.__struct_fields__, Gated(enabled=True, value=0.0)))
    occlusion: Annotated[Gated[Unit], msgspec.Meta(description="Ambient occlusion strength over lobe light"), Presentation(soft=Some((0.0, 1.0)))] = Gated(enabled=False, value=1.0)


class GroupGain(msgspec.Struct, frozen=True, kw_only=True):
    """Gain and tint of one provider-listed light group."""

    name: Annotated[str, msgspec.Meta(title="Light Group", description="Light group as listed by the view layer")]
    gain: Annotated[Gated[Stops], msgspec.Meta(description="Group light in stops")] = Gated(enabled=True, value=0.0)
    tint: Annotated[Rgb, msgspec.Meta(description="Color multiplying group light")] = Rgb(red=1.0, green=1.0, blue=1.0)


class LightGroups(Case, frozen=True, kw_only=True, tag=2):
    """Light-group passes summed under independent gains and tints."""

    title = "Light Groups"
    groups: Annotated[tuple[GroupGain, ...], msgspec.Meta(description="Gain and tint per light group")] = ()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["GroupGain", "LightGroups", "Lobes", "Recombination", "Terms"]
