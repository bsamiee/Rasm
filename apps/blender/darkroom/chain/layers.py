"""Layer stacks over the chain: layers take the effects ranked before formation, offer them by phase, and form documents through the formation entry's view."""

from collections.abc import Mapping
from types import MappingProxyType

from expression import Some

from blender.bpy.render.compositor.layers import LayerStack, Source, TermFiltering
from blender.bpy.render.compositor.recipes import Valued
from blender.bpy.render.compositor.stacks import LayerStacks

from .stack import Chain
from .stages import Effect, Stage

# --- [OPERATIONS] -----------------------------------------------------------------------


def stacks(chain: Chain, *, filtering: TermFiltering[Effect], looks: Mapping[str, Valued[LayerStack[Source, Effect]]] = MappingProxyType({})) -> LayerStacks[Effect]:
    """Layer stacks over the chain's rows, a layer accepting effects ranked before formation, its add menu one submenu per phase, the given term filter, and stock looks."""
    return LayerStacks(rows=chain.rows, accepts=lambda row: row.record.stage < Stage.FORMATION, groups=Some(lambda row: row.record.stage.phase), looks=looks, filtering=filtering, view=chain.view)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["stacks"]
