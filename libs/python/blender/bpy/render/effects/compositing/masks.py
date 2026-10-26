"""Displayed-frame mask keys, working luminance, and composited coverage."""

from collections.abc import Callable, Mapping
import functools
from math import pi, tau
from typing import Final, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, group_input, grouped, math, menu, NodeGroup, switched, Term, vector, xyz
from blender.bpy.nodes.interface import DesiredItem, Interface, InterfacePanel, item_key, MenuItems, Socket
from blender.bpy.render.color.spaces import COLOR_INPUT, COLOR_RESULT, converted, hsy, logarithmic
from blender.bpy.render.effects.coordinates import framed, FrameSpace, pixels, shorter
from blender.imaging.compositing.masks import Band, HueRange, Mask, MaskOperation, MatteChannel, Outline, Qualifier, Shape, TrackMatte, Transition
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, Gated, heading

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

KEY: Final = Socket(key="key", name="Key", kind=bpy.types.NodeSocketFloat)
LUMINANCE: Final = Socket(key="luminance", name="Luminance", kind=bpy.types.NodeSocketFloat)
_COVERAGE: Final = Socket(key="coverage", name="Coverage", kind=bpy.types.NodeSocketFloat)
_FRAMED: Final = (COLOR_INPUT, Socket(key="aspect", name="Aspect", kind=bpy.types.NodeSocketFloat), Socket(key="scale", name="Scale", kind=bpy.types.NodeSocketFloat))
_RUNNING: Final = Socket(key="running", name="Running", kind=bpy.types.NodeSocketFloat)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GROUPS]


def luminance(config: PyOpenColorIO.Config) -> BlenderResult[NodeGroup]:
    """Returns working-space Y group from scene-linear conversion."""
    return converted("xyz", config, PyOpenColorIO.ROLE_SCENE_LINEAR, PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY, group_input(COLOR_INPUT.key, "RGBA")).bind(
        lambda xyz: _built(
            "working_luminance",
            Interface(inputs=(COLOR_INPUT,), outputs=(LUMINANCE,)),
            Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", xyz),), members=(("mode", "RGB"),), socket="Green"),
        )
    )


@grouped("soft_edge", heading("soft_edge"))
def edge(distance: Expr, softness: Expr) -> Mapping[Key, Expr]:
    """Smoothstep edge over a signed distance, an inclusive step at zero softness."""
    soft = Expr(
        kind=bpy.types.ShaderNodeMapRange,
        inputs=(("Value", distance), ("From Min", -softness), ("From Max", 0.0), ("To Min", 0.0), ("To Max", 1.0)),
        members=(("interpolation_type", "SMOOTHSTEP"), ("clamp", True)),
        socket="Result",
    )
    return {KEY.key: switched(softness > 0.0, 1.0 - (distance < 0.0), soft)}


@grouped("band", heading("band"))
def band(value: Expr, low: Expr, high: Expr, softness: Expr) -> Mapping[Key, Expr]:
    """Feathered interval key, empty when high is below low."""
    return {KEY.key: edge(value - low, softness) * edge(high - value, softness) * (1.0 - (high < low))}


@functools.cache
def shape() -> BlenderResult[NodeGroup]:
    """Returns rotated box and ellipse key group over displayed frame."""
    outline, position, size, rotation, feather = (field.name for field in msgspec.structs.fields(Shape))

    def shaped(space: FrameSpace) -> Expr:
        extent, angle = group_input(size, "VECTOR"), group_input(rotation)
        offset = space.point - group_input(position, "VECTOR") * space.extent
        cosine, sine = math("COSINE", angle), math("SINE", angle)
        plane = xyz(offset["X"] * cosine + offset["Y"] * sine, offset["Y"] * cosine - offset["X"] * sine) / extent
        folded = abs(plane)
        keys = {Outline.BOX.label: math("MAXIMUM", folded["X"], folded["Y"]) < 1.0, Outline.ELLIPSE.label: vector("DOT_PRODUCT", plane, plane) < 1.0}
        inside = menu(group_input(outline), MenuItems.of(Outline), keys, kind="FLOAT") * (shorter(extent) > 0.0)
        radius = pixels(space, xyz(group_input(feather), group_input(feather)), "VECTOR")
        return Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", inside), ("Size", radius), ("Type", "Fast Gaussian")), socket="Image")

    return _framed("shape", Shape, shaped)


@functools.cache
def wipe() -> BlenderResult[NodeGroup]:
    """Returns corner-normalized wipe group, exact at both ends."""
    completion, angle, softness = (group_input(name) for name in Transition.__struct_fields__)

    def traveled(space: FrameSpace) -> Expr:
        direction = xyz(math("COSINE", angle), math("SINE", angle))
        corner = space.extent * direction
        low, high = (math(operation, corner["X"], 0.0) + math(operation, corner["Y"], 0.0) for operation in ("MINIMUM", "MAXIMUM"))
        travel = (vector("DOT_PRODUCT", space.point, direction) - low) / (high - low)
        soft = Expr(
            kind=bpy.types.ShaderNodeMapRange,
            inputs=(("Value", completion * (softness + 1.0) - travel), ("From Min", 0.0), ("From Max", softness), ("To Min", 0.0), ("To Max", 1.0)),
            members=(("interpolation_type", "LINEAR"), ("clamp", True)),
            socket="Result",
        )
        interior = switched(softness > 0.0, 1.0 - (travel > completion), soft)
        return switched(completion < 1.0, 1.0, switched(completion > 0.0, 0.0, interior))

    return _framed("wipe", Transition, traveled)


@functools.cache
def qualifier() -> BlenderResult[NodeGroup]:
    """Returns qualifier group over HSY hue, saturation, and log luminance."""
    forward = PyOpenColorIO.TRANSFORM_DIR_FORWARD
    hue_field, saturation_field, luminance_field = Qualifier.__struct_fields__
    _, value = Gated.__struct_fields__
    center, width, softness = HueRange.__struct_fields__

    def qualified(groups: tuple[NodeGroup, NodeGroup]) -> Expr:
        coordinates, logarithm = groups
        straight = Expr(kind=bpy.types.CompositorNodePremulKey, inputs=(("Image", group_input(COLOR_INPUT.key, "RGBA")), ("Type", "To Straight")), socket="Image", shape="RGBA")
        color = coordinates.placed({COLOR_INPUT.key: straight}, COLOR_RESULT.key)
        stops = logarithm.placed({COLOR_INPUT.key: color}, COLOR_RESULT.key)["Blue"]
        hue = group_input(item_key(hue_field, value, center))
        distance = pi - abs(abs(color["Red"] * tau - hue) % tau - pi)
        keys = (
            edge(group_input(item_key(hue_field, value, width)) * 0.5 - distance, group_input(item_key(hue_field, value, softness))),
            *(
                band(level, **{field.name: group_input(item_key(name, value, field.name)) for field in msgspec.structs.fields(Band)})
                for name, level in ((saturation_field, color["Green"]), (luminance_field, stops))
            ),
        )
        hue_gate, saturation_gate, luminance_gate = (switched(group_input(name), 1.0, key) for name, key in zip((hue_field, saturation_field, luminance_field), keys))
        return hue_gate * saturation_gate * luminance_gate

    return paired(Interface.of(Qualifier), paired(hsy(forward), logarithmic(forward)).map(qualified)).bind(
        lambda parts: _built("qualifier", msgspec.structs.replace(parts[0], inputs=(COLOR_INPUT, *parts[0].inputs), outputs=(KEY,)), parts[1])
    )


def matte(working: NodeGroup) -> BlenderResult[NodeGroup]:
    """Returns track matte group keying clamped alpha or associated working-space Y."""
    image = group_input(COLOR_INPUT.key, "RGBA")
    channel, invert = (field.name for field in msgspec.structs.fields(TrackMatte))
    selected = menu(
        group_input(channel),
        MenuItems.of(MatteChannel),
        {MatteChannel.ALPHA.label: image["Alpha"], MatteChannel.LUMINANCE.label: working.placed({COLOR_INPUT.key: image}, LUMINANCE.key)},
        kind="FLOAT",
    )
    clamped = Expr(kind=bpy.types.ShaderNodeClamp, inputs=(("Value", selected), ("Min", 0.0), ("Max", 1.0)), socket="Result")
    return Interface.of(TrackMatte).bind(lambda face: _built("track_matte", msgspec.structs.replace(face, inputs=(COLOR_INPUT, *face.inputs), outputs=(KEY,)), abs(group_input(invert) - clamped)))


@functools.cache
def joined(*, opening: bool) -> BlenderResult[NodeGroup]:
    """Returns mask join group, opening mask joining onto operation identity."""
    key_mask, operation_mask, invert = (field.name for field in msgspec.structs.fields(Mask))
    heads = (*_FRAMED, *(() if opening else (_RUNNING,)))
    shared = frozenset(socket.key for socket in heads)
    cases: tuple[type[Case], ...] = (Shape, Qualifier, Transition)
    joins: Mapping[MaskOperation, tuple[rna_enums.NodeMathItems, bool, float]] = {
        MaskOperation.ADD: ("MAXIMUM", False, 0.0),
        MaskOperation.SUBTRACT: ("SUBTRACT", True, 1.0),
        MaskOperation.MULTIPLY: ("MULTIPLY", False, 1.0),
    }

    def placed(case: type[Case], group: NodeGroup) -> Expr:
        return group.placed(relayed(lambda key: group_input(key if key in shared else item_key(key_mask, case, key)), group.interface), KEY.key)

    def keyed(groups: tuple[NodeGroup, ...]) -> Expr:
        keys = {case.title: placed(case, group) for case, group in zip(cases, groups)}
        key = abs(group_input(invert) - menu(group_input(key_mask), MenuItems.of(cases), keys, kind="FLOAT"))
        return menu(
            group_input(operation_mask),
            MenuItems.of(MaskOperation),
            {operation.label: math(name, start if opening else group_input(_RUNNING.key), key, clamp=clamp) for operation, (name, clamp, start) in joins.items()},
            kind="FLOAT",
        )

    return paired(Interface.of(Mask), accumulated((shape(), qualifier(), wipe())).map(keyed)).bind(
        lambda parts: _built("first_mask" if opening else "mask", msgspec.structs.replace(parts[0], inputs=(*heads, *parts[0].inputs), outputs=(_COVERAGE,)), parts[1])
    )


# --- [_COVERAGE]


def relayed(outer: Callable[[Key], Term], interface: Interface) -> dict[Key, Term]:
    """Returns operands for group input sockets and panel toggles."""

    def entering(item: DesiredItem) -> bool:
        match item:
            case InterfacePanel(toggle=toggle):
                return toggle.is_some()
            case (_, side):
                return side == "INPUT"

    return {key: outer(key) for key, item, _, _ in interface.walked() if entering(item)}


def coverage(count: int, image: Term, aspect: Term, scale: Term, outer: Callable[[Key], Term]) -> BlenderResult[Option[Expr]]:
    """Returns chained coverage of `count` masks over the displayed frame, each mask's inputs at `outer(item_key(index, key))`, Nothing at zero count."""

    def chained(groups: tuple[NodeGroup, NodeGroup]) -> Expr:
        def placed(index: int, group: NodeGroup, running: Option[Expr]) -> Expr:
            heads: dict[Key, Term] = dict(zip((socket.key for socket in (*_FRAMED, _RUNNING)), (image, aspect, scale, *running.to_list())))
            return group.placed(relayed(lambda key: heads[key] if key in heads else outer(item_key(index, key)), group.interface), _COVERAGE.key)

        first, rest = groups
        return functools.reduce(lambda running, index: placed(index, rest, Some(running)), range(2, count + 1), placed(1, first, Nothing))

    return paired(joined(opening=True), joined(opening=False)).map(lambda groups: Some(chained(groups))) if count else Ok(Nothing)


def _framed(key: Key, form: type[msgspec.Struct], body: Callable[[FrameSpace], Expr]) -> BlenderResult[NodeGroup]:
    """Builds key group over the displayed frame of its image, record inputs after the frame heads."""
    image, aspect, scale = (group_input(socket.key, "RGBA" if socket is COLOR_INPUT else "FLOAT") for socket in _FRAMED)
    return Interface.of(form).bind(lambda face: _built(key, msgspec.structs.replace(face, inputs=(*_FRAMED, *face.inputs), outputs=(KEY,)), body(framed(image, aspect, scale))))


def _built(key: Key, interface: Interface, result: Term) -> BlenderResult[NodeGroup]:
    """Builds a one-output group named after its key."""
    return NodeGroup.built(key, heading(key), interface, {interface.outputs[0].key: result})


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["KEY", "LUMINANCE", "band", "coverage", "edge", "joined", "luminance", "matte", "qualifier", "relayed", "shape", "wipe"]
