"""Mix node blend modes and premultiplied coverage."""

from collections.abc import Mapping
import functools
from types import MappingProxyType
from typing import Final, TYPE_CHECKING

import bpy
from msgspec import structs

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, group_input, menu, mixed, NodeGroup, Term
from blender.bpy.nodes.interface import Interface, MenuItems, Socket
from blender.imaging.compositing.blending import Blend, BlendMode

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

BLENDS: Final[Mapping[BlendMode, "rna_enums.RampBlendItems"]] = MappingProxyType({
    BlendMode.NORMAL: "MIX",
    BlendMode.DARKEN: "DARKEN",
    BlendMode.MULTIPLY: "MULTIPLY",
    BlendMode.COLOR_BURN: "BURN",
    BlendMode.LIGHTEN: "LIGHTEN",
    BlendMode.SCREEN: "SCREEN",
    BlendMode.COLOR_DODGE: "DODGE",
    BlendMode.LINEAR_DODGE: "ADD",
    BlendMode.OVERLAY: "OVERLAY",
    BlendMode.SOFT_LIGHT: "SOFT_LIGHT",
    BlendMode.LINEAR_LIGHT: "LINEAR_LIGHT",
    BlendMode.DIFFERENCE: "DIFFERENCE",
    BlendMode.EXCLUSION: "EXCLUSION",
    BlendMode.SUBTRACT: "SUBTRACT",
    BlendMode.DIVIDE: "DIVIDE",
    BlendMode.HUE: "HUE",
    BlendMode.SATURATION: "SATURATION",
    BlendMode.COLOR: "COLOR",
    BlendMode.VALUE: "VALUE",
})

# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def mixback() -> BlenderResult[NodeGroup]:
    """Returns shared HDR blend group: straight colors blended per mode, re-covered by result alpha, mixed over base by coverage."""
    mode, opacity, _ = structs.fields(Blend)
    base, result = group_input("base", "RGBA"), group_input("result", "RGBA")
    background, foreground = (Expr(kind=bpy.types.CompositorNodePremulKey, inputs=(("Image", color), ("Type", "To Straight")), socket="Image", shape="RGBA") for color in (base, result))
    pure = menu(
        group_input("blend"),
        MenuItems.of(BlendMode),
        {choice.label: foreground if choice is BlendMode.NORMAL else mixed(1.0, background, foreground, blend=BLENDS[choice]) for choice in BlendMode},
        kind="RGBA",
    )
    opaque = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", mixed(base["Alpha"], foreground, pure)), ("Alpha", 1.0), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")
    covered = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", opaque), ("Alpha", result["Alpha"]), ("Type", "Apply Mask")), socket="Image", shape="RGBA")
    blended = Expr(
        kind=bpy.types.CompositorNodeSwitch, inputs=(("Switch", structs.replace(pure, socket=BlendMode.NORMAL.label, shape="FLOAT")), ("Off", covered), ("On", result)), socket="Image", shape="RGBA"
    )
    return Interface.of(Blend).bind(
        lambda face: NodeGroup.built(
            "mixback",
            "Mix Back",
            Interface(
                inputs=(
                    Socket(key="base", name="Base", kind=bpy.types.NodeSocketColor),
                    Socket(key="result", name="Result", kind=bpy.types.NodeSocketColor),
                    *(structs.replace(socket, key={mode.name: "blend", opacity.name: "coverage"}[socket.key]) for socket in face.inputs),
                ),
                outputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),),
            ),
            {"image": mixed(group_input("coverage"), base, blended)},
        )
    )


def mixed_back(key: Key, base: Term, result: Term, mode: Term, coverage: Term) -> BlenderResult[Expr]:
    """Places the shared mix-back group under the stored key `<key>_mixback`."""
    return mixback().map(lambda group: group.placed({"base": base, "result": result, "blend": mode, "coverage": coverage}, "image").named(f"{key}_mixback"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BLENDS", "mixback", "mixed_back"]
