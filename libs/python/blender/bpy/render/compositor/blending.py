"""Blend of a compositor result over its base under a Mix node formula, opacity, and masks, the shared mix-back group, and the Photoshop mode of each formula."""

import functools
from typing import Annotated, Final, Generic, Protocol, TypeVar

import bpy
from expression import Some
import msgspec
import psdtags
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Key
from blender.bpy.nodes import functions
from blender.bpy.nodes.desired import NodeGroup
from blender.bpy.nodes.graphs import Expr, group_input, menu, mixed, Scalar, switched, Term
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItems, Socket, under
from blender.bpy.render.compositor.colors import straight, with_alpha
from blender.bpy.render.compositor.masks import coverage, masked
from blender.records.faults import Outcome
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

_M_co = TypeVar("_M_co", bound=Case, covariant=True)


class _FormulaMixed(Protocol):
    """`mixed` under a `ShaderNodeMix.blend_type` identifier read from the live enum, a string the stubs' literal does not admit."""

    def __call__(self, factor: Scalar, a: Term, b: Term, /, *, blend: str) -> Expr: ...


# --- [CONSTANTS] ------------------------------------------------------------------------

_FORMULA_MIXED: Final[_FormulaMixed] = mixed  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

BLEND: Final[Key] = "blend"
_MASKS: Final[Key] = "masks"
FORMULAS: Final = Descriptor.declared(bpy.types.ShaderNodeMix, ("blend_type",), bpy.types.EnumProperty).prop
MODES: Final = MenuItems.of(FORMULAS)
MODE: Final = Socket(key="mode", name="Blend", kind=bpy.types.NodeSocketMenu, description="Mix node formula mixing result with base", default_value=FORMULAS.enum_items["MIX"].name, items=Some(MODES))
OPACITY: Final = Socket(key="opacity", name="Opacity", kind=bpy.types.NodeSocketFloatFactor, description="Share of result mixed over base", min_value=0.0, max_value=1.0, default_value=1.0)

# --- [MODELS] ---------------------------------------------------------------------------


class Blend(msgspec.Struct, frozen=True, kw_only=True):
    """Count of the masks limiting where a result mixes over its base, mode and opacity held by the blend sockets."""

    masks: Annotated[int, msgspec.Meta(ge=0)] = 0

    @property
    def interface(self) -> Interface:
        """Mode and opacity sockets, then one panel per mask under `masks`."""
        nested = under(_MASKS, Interface(panels=masked(self.masks)))
        return Interface(inputs=(MODE, OPACITY), panels=(InterfacePanel(key=_MASKS, name="Masks", panels=nested.panels),) if self.masks else ())

    @property
    def panel(self) -> InterfacePanel:
        """Blend panel of an entry group, its items keyed under `BLEND`."""
        face = under(BLEND, self.interface)
        return InterfacePanel(key=BLEND, name="Blend", inputs=face.inputs, panels=face.panels)


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
    base, result = group_input("base", bpy.types.NodeSocketColor), group_input("result", bpy.types.NodeSocketColor)
    background, foreground = straight(base), straight(result)
    pure = menu(group_input("blend"), {item: _FORMULA_MIXED(1.0, background, foreground, blend=formula.identifier) for formula, item in zip(FORMULAS.enum_items, MODES.items, strict=True)})
    covered = with_alpha(with_alpha(mixed(base["Alpha"], foreground, pure), 1.0), result["Alpha"], masked=True)
    blended = switched(msgspec.structs.replace(pure, socket=FORMULAS.enum_items["MIX"].name, shape=bpy.types.NodeSocketFloat), covered, result)
    return functions.built(
        "mixback",
        "Mix Back",
        Interface(
            inputs=(
                Socket(key="base", kind=bpy.types.NodeSocketColor),
                Socket(key="result", kind=bpy.types.NodeSocketColor),
                msgspec.structs.replace(MODE, key="blend"),
                msgspec.structs.replace(OPACITY, key="coverage"),
            ),
            outputs=(Socket(key="image", kind=bpy.types.NodeSocketColor),),
        ),
        {"image": mixed(group_input("coverage"), base, blended)},
        bpy.types.CompositorNodeTree,
    )


def mixed_back(key: Key, base: Term, result: Term, mode: Term, coverage: Term) -> Outcome[Expr]:
    """Places the shared mix-back group under the stored key `<key>_mixback`."""
    return mixback().map(lambda group: functions.placed(group, {"base": base, "result": result, "blend": mode, "coverage": coverage}, "image").named(f"{key}_mixback"))


def blended(blend: Blend, base: Term, result: Expr, frame: tuple[Expr, Term, Term], config: PyOpenColorIO.Config) -> Outcome[Expr]:
    """Mixes a result back over its base through the blend panel's mode and opacity, weighted by its masks over the displayed frame.

    Args:
        blend: Mask count of the entry.
        base: Term the result mixes over.
        result: Associated result.
        frame: Image, pixel aspect, and camera share the masks frame.
        config: Configuration the mask keys read.

    Returns:
        Mixed result, or the mask group faults.
    """
    weight = group_input(item_key(BLEND, OPACITY.key))
    return (
        coverage(blend.masks, *frame, lambda key: group_input(item_key(BLEND, _MASKS, key)), config)
        .map(lambda mask: mask.map(lambda held: weight * held).default_value(weight))
        .bind(lambda weighted: mixed_back(BLEND, base, result, group_input(item_key(BLEND, MODE.key)), weighted))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BLEND", "FORMULAS", "MODE", "MODES", "OPACITY", "Blend", "Entry", "blended", "mixback", "mixed_back", "photoshop"]
