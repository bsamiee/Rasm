"""Layer stacks with effects, affine placement, blending, and track mattes."""

from typing import Annotated

from expression import Nothing, Option, Some
import msgspec

from blender.imaging.chain import Effect
from blender.imaging.compositing.blending import Blend, Entry
from blender.imaging.compositing.masks import TrackMatte
from blender.imaging.parameters import Case, Finite, Gated, Position, Presentation, Quantity

# --- [MODELS] ---------------------------------------------------------------------------


class Adjustment(Case, frozen=True, tag=1):
    """Composite beneath through layer effects."""


class Affine(msgspec.Struct, frozen=True, kw_only=True):
    """Scale, shear, and rotation about pivot, then translation."""

    position: Annotated[Position, msgspec.Meta(description="Offset in frame fractions")] = (0.0, 0.0)
    rotation: Annotated[Finite, msgspec.Meta(description="Turn about pivot"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    scale: Annotated[tuple[Finite, Finite], msgspec.Meta(description="Signed factor per frame axis")] = (1.0, 1.0)
    skew: Annotated[tuple[Finite, Finite], msgspec.Meta(description="Horizontal and vertical shear angles"), Presentation(quantity=Some(Quantity.ANGLE))] = (0.0, 0.0)
    pivot: Annotated[Position, msgspec.Meta(description="Fixed point of scale, shear, and rotation")] = (0.5, 0.5)


class Layer[S: Case, E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Source through effects and placement, blended over preceding layers."""

    key: Annotated[str, msgspec.Meta(description="Identity within stack")]
    name: Annotated[str, msgspec.Meta(description="Name in lists and documents")]
    source: S
    effects: Annotated[Gated[tuple[Gated[Entry[E]], ...]], msgspec.Meta(description="Source effects")] = Gated(enabled=True, value=())
    affine: Annotated[Affine, msgspec.Meta(title="Transform", description="Content placement")] = Affine()
    blend: Annotated[Blend, msgspec.Meta(description="Mix over preceding layers")] = Blend()
    matte: Annotated[Gated[TrackMatte], msgspec.Meta(title="Track Matte", description="Coverage read from layer beneath")] = Gated(enabled=False, value=TrackMatte())
    visible: Annotated[bool, msgspec.Meta(description="Composite layer")] = True
    solo: Annotated[bool, msgspec.Meta(description="Composite soloed layers alone")] = False


class LayerStack[S: Case, E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Layers ordered bottom first over a transparent frame."""

    layers: Annotated[tuple[Layer[S, E], ...], msgspec.Meta(description="Layers bottom first")] = ()

    def composited(self) -> tuple[tuple[Layer[S, E], Option[Layer[S, E]]], ...]:
        """Selects visible layers under solo except layers a matte above consumes.

        Returns:
            Layers in stack order, each with layer beneath, absent at bottom.
        """
        soloed = any(layer.solo for layer in self.layers)
        beneath: tuple[Option[Layer[S, E]], ...] = (Nothing, *map(Some, self.layers))
        consumed = (*(layer.matte.enabled for layer in self.layers[1:]), False)
        return tuple((layer, below) for layer, below, matte in zip(self.layers, beneath, consumed) if layer.visible and not matte and (layer.solo or not soloed))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Adjustment", "Affine", "Layer", "LayerStack"]
