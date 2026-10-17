"""Typed layer stacks with effects, affine placement, blending, and track mattes."""

from typing import Annotated

from expression import Nothing, Option, Some
import msgspec

from blender.imaging.chain import Effect
from blender.imaging.compositing.blending import Blend, Entry
from blender.imaging.compositing.masks import TrackMatte
from blender.imaging.parameters import Case, Finite, Gated, Position, Presentation, Quantity

# --- [MODELS] ---------------------------------------------------------------------------


class Adjustment(Case, frozen=True, tag=1):
    """Composite beneath, processed by the layer's effects."""


class Affine(msgspec.Struct, frozen=True, kw_only=True):
    """Scale, shear, and rotation about a pivot, followed by translation."""

    position: Annotated[Position, msgspec.Meta(description="Offset as a share of each frame side")] = (0.0, 0.0)
    rotation: Annotated[Finite, msgspec.Meta(description="Turn about the pivot"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    scale: Annotated[tuple[Finite, Finite], msgspec.Meta(description="Signed factor along each frame axis")] = (1.0, 1.0)
    skew: Annotated[tuple[Finite, Finite], msgspec.Meta(description="Shear angles along the horizontal and vertical axes"), Presentation(quantity=Some(Quantity.ANGLE))] = (0.0, 0.0)
    pivot: Annotated[Position, msgspec.Meta(description="Fixed point of scale, shear, and rotation")] = (0.5, 0.5)


class Layer[S: Case, E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Source through effects and placement, blended over preceding layers."""

    key: Annotated[str, msgspec.Meta(description="Identity within the stack")]
    name: Annotated[str, msgspec.Meta(description="Text shown in lists and documents")]
    source: S
    effects: Annotated[Gated[tuple[Gated[Entry[E]], ...]], msgspec.Meta(description="Effects processing the source")] = Gated(enabled=True, value=())
    affine: Annotated[Affine, msgspec.Meta(title="Transform", description="Scale, rotation, and position of the content")] = Affine()
    blend: Annotated[Blend, msgspec.Meta(description="Mode, opacity, and masks over preceding layers")] = Blend()
    matte: Annotated[Gated[TrackMatte], msgspec.Meta(title="Track Matte", description="Coverage read from the layer beneath")] = Gated(enabled=False, value=TrackMatte())
    visible: Annotated[bool, msgspec.Meta(description="Composite the layer")] = True
    solo: Annotated[bool, msgspec.Meta(description="Composite soloed layers alone")] = False


class LayerStack[S: Case, E: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Layers ordered bottom first over a transparent frame."""

    layers: Annotated[tuple[Layer[S, E], ...], msgspec.Meta(description="Layers bottom first")] = ()

    def composited(self) -> tuple[tuple[Layer[S, E], Option[Layer[S, E]]], ...]:
        """Selects visible compositing layers under solo and matte ownership.

        Returns:
            Layers in stack order paired with their immediate predecessor, absent at the bottom.
        """
        soloed = any(layer.solo for layer in self.layers)
        beneath: tuple[Option[Layer[S, E]], ...] = (Nothing, *map(Some, self.layers))
        consumed = (*(layer.matte.enabled for layer in self.layers[1:]), False)
        return tuple((layer, below) for layer, below, matte in zip(self.layers, beneath, consumed, strict=False) if layer.visible and not matte and (layer.solo or not soloed))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Adjustment", "Affine", "Layer", "LayerStack"]
