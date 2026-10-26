"""Blend of a compositor result over its base under a Mix node formula, opacity, and masks, the shared mix-back group, and the Photoshop mode of each formula."""

import functools
import typing
from typing import Annotated, Final, Generic, Literal, TYPE_CHECKING, TypeVar

import bpy
from expression import Some
import msgspec
import psdtags

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.graphs import Expr, group_input, menu, mixed, NodeGroup, switched, Term
from blender.bpy.nodes.interface import Interface, InterfacePanel, MenuItems, Socket, under
from blender.bpy.render.compositor.masks import masked
from blender.records.faults import Outcome
from blender.records.fields import Case

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import RampBlendItems
else:
    RampBlendItems = Literal[tuple(item.identifier for item in bpy.types.ShaderNodeMix.bl_rna.properties["blend_type"].enum_items)]

# --- [TYPES] ----------------------------------------------------------------------------

_M_co = TypeVar("_M_co", bound=Case, covariant=True)

# --- [CONSTANTS] ------------------------------------------------------------------------

FORMULAS: Final = Descriptor.declared(bpy.types.ShaderNodeMix, "blend_type", bpy.types.EnumProperty).prop
MODES: Final = MenuItems.of(FORMULAS)
MODE: Final = Socket(
    key="mode", name="Blend", kind=bpy.types.NodeSocketMenu, description="Mix node formula mixing result with base", members={"default_value": FORMULAS.enum_items["MIX"].name}, items=Some(MODES)
)
OPACITY: Final = Socket(
    key="opacity",
    name="Opacity",
    kind=bpy.types.NodeSocketFloat,
    description="Share of result mixed over base",
    members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0, "default_value": 1.0},
)

# --- [MODELS] ---------------------------------------------------------------------------


class Blend(msgspec.Struct, frozen=True, kw_only=True):
    """Count of the masks limiting where a result mixes over its base, mode and opacity held by the blend sockets."""

    masks: Annotated[int, msgspec.Meta(ge=0)] = 0

    @property
    def interface(self) -> Interface:
        """Mode and opacity sockets, then one panel per mask under `masks`."""
        nested = under("masks", Interface(panels=masked(self.masks)))
        return Interface(inputs=(MODE, OPACITY), panels=(InterfacePanel(key="masks", name="Masks", panels=nested.panels),) if self.masks else ())


class Entry(msgspec.Struct, Generic[_M_co], frozen=True, kw_only=True):
    """Compositor entry record with the blend of its result over its input."""

    record: _M_co
    blend: Blend = Blend()


# --- [OPERATIONS] -----------------------------------------------------------------------


def photoshop(name: str) -> psdtags.PsdBlendMode:
    """Photoshop mode of a Mix formula's menu item, the mode of the same name where both hosts share it."""
    match next(item.identifier for item in FORMULAS.enum_items if item.name == name):
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
        case identifier:
            return psdtags.PsdBlendMode[identifier]


@functools.cache
def mixback() -> Outcome[NodeGroup]:
    """Returns the shared HDR blend group: straight colors mixed per formula, re-covered by result alpha, mixed over base by coverage, the plain formula passing the associated result."""
    base, result = group_input("base", "RGBA"), group_input("result", "RGBA")
    background, foreground = (Expr(kind=bpy.types.CompositorNodePremulKey, inputs=(("Image", color), ("Type", "To Straight")), socket="Image", shape="RGBA") for color in (base, result))
    pure = menu(group_input("blend"), MODES, {FORMULAS.enum_items[formula].name: mixed(1.0, background, foreground, blend=formula) for formula in typing.get_args(RampBlendItems)}, kind="RGBA")
    opaque = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", mixed(base["Alpha"], foreground, pure)), ("Alpha", 1.0), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")
    covered = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", opaque), ("Alpha", result["Alpha"]), ("Type", "Apply Mask")), socket="Image", shape="RGBA")
    blended = switched(msgspec.structs.replace(pure, socket=FORMULAS.enum_items["MIX"].name, shape="FLOAT"), covered, result)
    return NodeGroup.built(
        "mixback",
        "Mix Back",
        Interface(
            inputs=(
                Socket(key="base", name="Base", kind=bpy.types.NodeSocketColor),
                Socket(key="result", name="Result", kind=bpy.types.NodeSocketColor),
                msgspec.structs.replace(MODE, key="blend"),
                msgspec.structs.replace(OPACITY, key="coverage"),
            ),
            outputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),),
        ),
        {"image": mixed(group_input("coverage"), base, blended)},
        bpy.types.CompositorNodeTree,
    )


def mixed_back(key: Key, base: Term, result: Term, mode: Term, coverage: Term) -> Outcome[Expr]:
    """Places the shared mix-back group under the stored key `<key>_mixback`."""
    return mixback().map(lambda group: group.placed({"base": base, "result": result, "blend": mode, "coverage": coverage}, "image").named(f"{key}_mixback"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FORMULAS", "MODE", "MODES", "OPACITY", "Blend", "Entry", "mixback", "mixed_back", "photoshop"]
