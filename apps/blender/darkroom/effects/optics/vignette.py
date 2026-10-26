"""Frame-edge falloff in scene-linear light: natural chief-ray illumination or a feathered outline pulling toward a color."""

import bpy
from expression import Some
import msgspec

from blender.bpy.nodes.graphs import Expr, mixed, NodeGroup, switched
from blender.bpy.nodes.interface import Interface, InterfacePanel, MenuItem, MenuItems, Socket, under
from blender.bpy.render.compositor import lenses, masks
from blender.bpy.render.compositor.effects import CompositorEffect, Shell
from blender.records.faults import Outcome, paired

from ...chain.stages import Effect, Stage

# --- [MODELS] ---------------------------------------------------------------------------


class Vignette(Effect, frozen=True, kw_only=True):
    """Frame-edge falloff from camera chief rays or a feathered outline."""

    stage = Stage.VIGNETTE
    pointwise = False


# --- [OPERATIONS] -----------------------------------------------------------------------


def vignetted(shell: Shell[Vignette]) -> Outcome[Expr]:
    """Mixes the stage input toward the pull color by the selected falloff weight."""
    source = shell.color
    entering = {"image": source, "aspect": shell.input(Shell.frame, "aspect"), "scale": shell.input(Shell.frame, "scale")}

    def built(cosine: Expr, outline: NodeGroup, pull: Expr) -> Expr:
        shaped = outline.placed(masks.relayed(lambda key: entering[key] if key in entering else shell.input("shaped", key), outline.interface), masks.KEY.key)
        weight = switched(msgspec.structs.replace(pull, socket="Shaped", shape="FLOAT"), cosine**4.0, shaped).named("weight")
        covered = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", pull), ("Alpha", source["Alpha"]), ("Type", "Apply Mask")), socket="Image", shape="RGBA")
        return mixed(weight, covered, source)

    return paired(paired(lenses.chief_ray(source), masks.shape()), shell.menu("falloff", cases={"Natural": (0.0, 0.0, 0.0, 1.0), "Shaped": shell.input("shaped", "color")}, kind="RGBA")).map(
        lambda parts: built(*parts[0], parts[1])
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

vignette = CompositorEffect(
    record=Vignette,
    graph=vignetted,
    interface=Interface(
        inputs=(
            Socket(
                key="falloff",
                name="Falloff",
                kind=bpy.types.NodeSocketMenu,
                description="Lens law or drawn outline",
                members={"default_value": "Natural"},
                items=Some(
                    MenuItems(
                        items=(
                            MenuItem(name="Natural", description="Relative illumination as fourth power of chief-ray cosine"),
                            MenuItem(name="Shaped", description="Light kept inside a feathered outline, falling to a color outside"),
                        )
                    )
                ),
            ),
        ),
        panels=(
            InterfacePanel(
                key="shaped",
                name="Shaped",
                description="Outline keeping frame light and the color outside it",
                inputs=(
                    *(
                        msgspec.structs.replace(socket, members={**socket.members, "default_value": (0.8, 0.45)})
                        if socket.key == "shaped_size"
                        else msgspec.structs.replace(socket, members={**socket.members, "default_value": 0.2})
                        if socket.key == "shaped_feather"
                        else socket
                        for socket in under("shaped", masks.SHAPE).inputs
                    ),
                    Socket(key="shaped_color", name="Color", kind=bpy.types.NodeSocketColor, description="Scene-linear color outside shape", members={"default_value": (0.0, 0.0, 0.0, 1.0)}),
                ),
            ),
        ),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Vignette", "vignette"]
