"""Blend of a compositor result over its base under a Mix node formula, opacity, and masks, the shared mix-back group, and the Photoshop mode of each formula."""

import functools
import typing
from typing import Annotated, Generic, Literal, TYPE_CHECKING, TypeVar

import bpy
from expression import Some
import msgspec
import psdtags

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, group_input, menu, mixed, NodeGroup, switched, Term
from blender.bpy.nodes.interface import Interface, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor.masks import Mask
from blender.bpy.settings.shapes import Literals
from blender.records.fields import Case, Presentation, Unit

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import RampBlendItems
else:
    RampBlendItems = Literal[tuple(item.identifier for item in bpy.types.ShaderNodeMix.bl_rna.properties["blend_type"].enum_items)]

# --- [TYPES] ----------------------------------------------------------------------------

_M_co = TypeVar("_M_co", bound=Case, covariant=True)

# --- [MODELS] ---------------------------------------------------------------------------


class Blend(msgspec.Struct, frozen=True, kw_only=True):
    """Mix node formula, opacity, and masks mixing a result over its base."""

    mode: Annotated[RampBlendItems, msgspec.Meta(title="Blend", description="Mix node formula mixing result with base")] = "MIX"
    opacity: Annotated[Unit, msgspec.Meta(description="Share of result mixed over base"), Presentation(soft=Some((0.0, 1.0)))] = 1.0
    masks: Annotated[tuple[Mask, ...], msgspec.Meta(description="Keys limiting where result mixes")] = ()

    @property
    def photoshop(self) -> psdtags.PsdBlendMode:
        """Photoshop mode of the formula, the mode of the same name where both hosts share it."""
        match self.mode:
            case "MIX":
                return psdtags.PsdBlendMode.NORMAL
            case "ADD":
                return psdtags.PsdBlendMode.LINEAR_DODGE
            case "BURN":
                return psdtags.PsdBlendMode.COLOR_BURN
            case "DODGE":
                return psdtags.PsdBlendMode.COLOR_DODGE
            case "VALUE":
                return psdtags.PsdBlendMode.LUMINOSITY
            case named:
                return psdtags.PsdBlendMode[named]


class Entry(msgspec.Struct, Generic[_M_co], frozen=True, kw_only=True):
    """Compositor entry record with the blend of its result over its input."""

    record: _M_co
    blend: Annotated[Blend, msgspec.Meta(description="Mix of result over input")] = Blend()


# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def mixback() -> BlenderResult[NodeGroup]:
    """Returns the shared HDR blend group: straight colors mixed per formula, re-covered by result alpha, mixed over base by coverage, the plain formula passing the associated result."""
    mode, opacity, _ = msgspec.structs.fields(Blend)
    formulas: tuple[RampBlendItems, ...] = typing.get_args(RampBlendItems)
    labels = Literals(values=formulas).labels
    base, result = group_input("base", "RGBA"), group_input("result", "RGBA")
    background, foreground = (Expr(kind=bpy.types.CompositorNodePremulKey, inputs=(("Image", color), ("Type", "To Straight")), socket="Image", shape="RGBA") for color in (base, result))
    items = MenuItems(items=tuple(MenuItem(name=label) for label in labels.values()))
    pure = menu(group_input("blend"), items, {labels[formula]: mixed(1.0, background, foreground, blend=formula) for formula in formulas}, kind="RGBA")
    opaque = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", mixed(base["Alpha"], foreground, pure)), ("Alpha", 1.0), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")
    covered = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", opaque), ("Alpha", result["Alpha"]), ("Type", "Apply Mask")), socket="Image", shape="RGBA")
    blended = switched(msgspec.structs.replace(pure, socket=labels[mode.default], shape="FLOAT"), covered, result)
    return Interface.of(Blend).bind(
        lambda face: NodeGroup.built(
            "mixback",
            "Mix Back",
            Interface(
                inputs=(
                    Socket(key="base", name="Base", kind=bpy.types.NodeSocketColor),
                    Socket(key="result", name="Result", kind=bpy.types.NodeSocketColor),
                    *(msgspec.structs.replace(socket, key={mode.name: "blend", opacity.name: "coverage"}[socket.key]) for socket in face.inputs),
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

__all__ = ["Blend", "Entry", "mixback", "mixed_back"]
