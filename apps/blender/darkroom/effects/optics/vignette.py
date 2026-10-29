"""Frame-edge falloff in scene-linear light: natural chief-ray illumination or a feathered outline pulling toward a color."""

from typing import Final

import bpy
from expression import Some
import msgspec

from blender.bpy.nodes import functions
from blender.bpy.nodes.desired import Interface, InterfacePanel, MenuItem, MenuItems, NodeGroup, Socket, under
from blender.bpy.nodes.graphs import Expr, mixed, switched
from blender.bpy.render.compositor import lenses, masks
from blender.bpy.render.compositor.colors import with_alpha
from blender.bpy.render.compositor.effects import CompositorEffect
from blender.bpy.render.compositor.inputs import FRAME_ASPECT, FRAME_SCALE, Shell
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage

# --- [CONSTANTS] ------------------------------------------------------------------------

_NATURAL: Final = MenuItem(name="Natural", description="Relative illumination as fourth power of chief-ray cosine")
_SHAPED: Final = MenuItem(name="Shaped", description="Light kept inside a feathered outline, falling to a color outside")
_FALLOFF: Final = Socket(key="falloff", kind=bpy.types.NodeSocketMenu, description="Lens falloff or drawn outline", default_value=_NATURAL.name, items=Some(MenuItems(items=(_NATURAL, _SHAPED))))
_SHAPED_COLOR: Final = Socket(key="shaped_color", name="Color", kind=bpy.types.NodeSocketColor, description="Scene-linear color outside shape", default_value=(0.0, 0.0, 0.0, 1.0))
_SHAPED_PANEL: Final = InterfacePanel(
    key="shaped",
    name="Shaped",
    description="Outline keeping frame light and the color outside it",
    inputs=(
        *(
            msgspec.structs.replace(socket, default_value=(0.8, 0.45))
            if socket.key == "shaped_size"
            else msgspec.structs.replace(socket, default_value=0.2)
            if socket.key == "shaped_feather"
            else socket
            for socket in under("shaped", masks.SHAPE).inputs
        ),
        _SHAPED_COLOR,
    ),
)

# --- [MODELS] ---------------------------------------------------------------------------


class Vignette(Effect, frozen=True, kw_only=True):
    """Frame-edge falloff from camera chief rays or a feathered outline."""

    stage = Stage.VIGNETTE
    pointwise = False


# --- [OPERATIONS] -----------------------------------------------------------------------


def vignetted(shell: Shell[Vignette]) -> Outcome[Expr]:
    """Mixes the stage input toward the pull color by the selected falloff weight."""
    source = shell.color
    entering = {"image": source, "aspect": shell.input(FRAME_ASPECT), "scale": shell.input(FRAME_SCALE)}
    shape = {socket.key: socket for socket in masks.SHAPE.inputs}
    pull = shell.menu(_FALLOFF, cases={_NATURAL: (0.0, 0.0, 0.0, 1.0), _SHAPED: shell.input(_SHAPED_COLOR)})

    def built(cosine: Expr, outline: NodeGroup) -> Expr:
        shaped = functions.placed(outline, masks.relayed(lambda key: entering[key] if key in entering else shell.input("shaped", shape[key]), outline.interface), masks.KEY.key)
        weight = switched(msgspec.structs.replace(pull, socket=_SHAPED.name, shape=bpy.types.NodeSocketFloat), cosine**4.0, shaped).named("weight")
        return mixed(weight, with_alpha(pull, source["Alpha"], masked=True), source)

    return masks.shape().map(lambda outline: built(lenses.chief_ray(source), outline))


# --- [COMPOSITION] ----------------------------------------------------------------------

vignette: Final = CompositorEffect(vignetted, record=Vignette, interface=Interface(inputs=(_FALLOFF,), panels=(_SHAPED_PANEL,)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Vignette", "vignette"]
