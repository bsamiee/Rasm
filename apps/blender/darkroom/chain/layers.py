"""Layer stacks over the chain: layers take the effects ranked before formation, offer them by phase, and form documents through the formation entry's view."""

from collections.abc import Callable, Mapping
from types import MappingProxyType

import bpy
from expression import Nothing, Option, Some

from blender.bpy.data.rna.conditions import Condition
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.render.compositor.layers import LayerStack, Source, TermFiltering
from blender.bpy.render.compositor.stacking import LayerStacks, Valued
from blender.records.faults import Outcome

from .stack import Chain
from .stages import Effect, Stage

# --- [OPERATIONS] -----------------------------------------------------------------------


def stacks(
    chain: Chain,
    *,
    filtering: TermFiltering[Effect],
    conditions: Option[Callable[[Identity, bpy.types.SceneCompositorEffect], Outcome[dict[Key, Condition]]]] = Nothing,
    looks: Mapping[str, Valued[LayerStack[Source, Effect]]] = MappingProxyType({}),
) -> LayerStacks[Effect]:
    """Layer stacks over the chain's rows, a layer accepting effects ranked before formation, its add menu one submenu per phase, the given term filter, head conditions, and stock looks."""
    return LayerStacks(
        rows=chain.rows,
        accepts=lambda row: row.record.stage < Stage.FORMATION,
        groups=Some(lambda row: row.record.stage.phase),
        looks=looks,
        filtering=filtering,
        view=chain.view,
        conditions=conditions,
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["stacks"]
