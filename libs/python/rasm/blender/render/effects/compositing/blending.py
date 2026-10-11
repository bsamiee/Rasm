"""Native blend formulas and premultiplied coverage for effects and layers."""

import functools
from typing import overload

import bpy
from expression import Ok
from msgspec import structs

from rasm.blender.extension.identity import Key
from rasm.blender.faults import BlenderResult
from rasm.blender.nodes.graphs import described, End, ended, Fragment, INPUTS, Interface, math, menu, MenuItems, mixed, nested, node, NodeGroup, Operand, Socket
from rasm.imaging.compositing.blending import Blend, BlendMode
from rasm.imaging.faults import accumulated, paired

# --- [OPERATIONS] -----------------------------------------------------------------------


def blend_type(mode: BlendMode) -> str:
    """Returns the native Mix identifier, deriving every regular spelling."""
    match mode:
        case BlendMode.NORMAL:
            return "MIX"
        case BlendMode.COLOR_BURN:
            return "BURN"
        case BlendMode.COLOR_DODGE:
            return "DODGE"
        case BlendMode.LINEAR_DODGE:
            return "ADD"
        case _:
            return mode.name


@functools.cache
def mixback() -> BlenderResult[NodeGroup]:
    """Returns the shared HDR blend group or its native construction faults."""
    base, result = End(node=INPUTS, socket="base"), End(node=INPUTS, socket="result")
    mode_name = Blend.mode.__name__  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    opacity_name = Blend.opacity.__name__  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    mode = next(field for field in structs.fields(Blend) if field.name == mode_name)
    items = MenuItems.of(BlendMode, described(mode.type))

    def blended(straight: tuple[tuple[Fragment, End], ...]) -> BlenderResult[tuple[Fragment, End]]:
        background, foreground = straight
        return accumulated(Ok(foreground) if choice is BlendMode.NORMAL else mixed(f"pure_{choice.value}", "RGBA", 1.0, background, foreground, blend_type(choice)) for choice in BlendMode).bind(
            lambda choices: menu("pure", "RGBA", End(node=INPUTS, socket="blend"), items, dict(zip((choice.label for choice in BlendMode), choices)))
        )

    pure = accumulated(
        node(f"straight_{name}", bpy.types.CompositorNodePremulKey, {"Image": color, "Type": "To Straight"}, {}).map(ended(f"straight_{name}", "Image"))
        for name, color in (("base", base), ("result", result))
    ).bind(blended)
    alphas = accumulated(
        node(f"{name}_alpha", bpy.types.CompositorNodeSeparateColor, {"Image": color}, {"mode": "RGB"}).map(ended(f"{name}_alpha", "Alpha")) for name, color in (("base", base), ("result", result))
    )

    def covered(parts: tuple[tuple[Fragment, End], tuple[tuple[Fragment, End], ...]]) -> BlenderResult[tuple[Fragment, End]]:
        chosen, (base_alpha, result_alpha) = parts
        return (
            mixed("toward", "RGBA", base_alpha, End(node="straight_result", socket="Image"), chosen)
            .bind(lambda toward: node("opaque", bpy.types.CompositorNodeSetAlpha, {"Image": toward, "Alpha": 1.0, "Type": "Replace Alpha"}, {}))
            .bind(lambda opaque: node("premultiplied", bpy.types.CompositorNodeSetAlpha, {"Image": ended("opaque", "Image")(opaque), "Alpha": result_alpha, "Type": "Apply Mask"}, {}))
            .bind(
                lambda premultiplied: node(
                    "foreground", bpy.types.CompositorNodeSwitch, {"Switch": End(node="pure", socket=BlendMode.NORMAL.label), "Off": ended("premultiplied", "Image")(premultiplied), "On": result}, {}
                )
            )
            .bind(
                lambda foreground: node("weight", bpy.types.ShaderNodeClamp, {"Value": End(node=INPUTS, socket="coverage"), "Min": 0.0, "Max": 1.0}, {}).bind(
                    lambda weight: node(
                        "over",
                        bpy.types.CompositorNodeAlphaOver,
                        {"Background": base, "Foreground": ended("foreground", "Image")(foreground), "Fac": ended("weight", "Result")(weight), "Type": "Over", "Straight Alpha": False},
                        {},
                    )
                )
            )
            .map(ended("over", "Image"))
        )

    return paired(paired(pure, alphas).bind(covered), Interface.of(Blend)).bind(
        lambda parts: NodeGroup.built(
            "mixback",
            "Mix Back",
            Interface(
                inputs=(
                    Socket(key="base", name="Base", kind=bpy.types.NodeSocketColor),
                    Socket(key="result", name="Result", kind=bpy.types.NodeSocketColor),
                    *(structs.replace(socket, key={mode_name: "blend", opacity_name: "coverage"}[socket.key]) for socket in parts[1].inputs),
                ),
                outputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),),
            ),
            {"image": parts[0]},
        )
    )


@overload
def product(key: Key, factors: tuple[tuple[Fragment, End], *tuple[tuple[Fragment, End], ...]]) -> BlenderResult[tuple[Fragment, End]]: ...


@overload
def product(key: Key, factors: tuple[Operand, *tuple[Operand, ...]]) -> BlenderResult[Operand]: ...


def product(key: Key, factors: tuple[Operand, *tuple[Operand, ...]]) -> BlenderResult[Operand]:
    """Returns the product while carrying every operand's upstream graph."""
    first, *rest = factors
    return functools.reduce(lambda done, item: done.bind(lambda held: math(f"{key}_{item[0]}", "MULTIPLY", held, item[1])), enumerate(rest, 1), Ok(first))


def mixed_back(key: Key, base: Operand, result: Operand, mode: Operand, coverage: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Returns one shared mix-back instance or its native construction faults."""
    return mixback().bind(lambda group: nested(f"{key}_mixback", group, {"base": base, "result": result, "blend": mode, "coverage": coverage}, "image"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["blend_type", "mixback", "mixed_back", "product"]
