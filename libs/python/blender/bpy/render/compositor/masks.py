"""Mask keys beside their node groups: shape, qualifier, wipe, highlight, and track matte keys, their edges and bands, working luminance, and chained mask coverage."""

from collections.abc import Callable, Mapping
import functools
from math import pi, radians, tau
import typing
from typing import Annotated, Final, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, group_input, grouped, math, menu, NodeGroup, Scalar, switched, Term, vector, xyz
from blender.bpy.nodes.interface import DesiredItem, Interface, InterfacePanel, item_key, MenuItems, Socket
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT, converted, hsy, logarithmic
from blender.bpy.render.compositor.coordinates import framed, FrameSpace, pixels, shorter
from blender.imaging.geometry.regions import FrameLength, Position
from blender.records.faults import accumulated, paired
from blender.records.fields import Case, Choice, Gated, heading, NonNegative, Presentation, Quantity, Unit

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Outline(Choice):
    """Inside test of a shape key."""

    BOX = 1
    ELLIPSE = 2


class MaskOperation(Choice):
    """Join of a key with the coverage of the masks before it."""

    ADD = 1
    SUBTRACT = 2
    MULTIPLY = 3

    @property
    def join(self) -> tuple["rna_enums.NodeMathItems", bool, float]:
        """Math operation joining the key onto running coverage, whether it clamps, and the coverage the first mask joins onto."""
        match self:
            case MaskOperation.ADD:
                return "MAXIMUM", False, 0.0
            case MaskOperation.SUBTRACT:
                return "SUBTRACT", True, 1.0
            case MaskOperation.MULTIPLY:
                return "MULTIPLY", False, 1.0


class MatteChannel(Choice):
    """Coverage channel of the layer beneath a track matte."""

    ALPHA = 1
    LUMINANCE = 2


type MaskKey = Shape | Qualifier | Transition

# --- [CONSTANTS] ------------------------------------------------------------------------

KEY: Final = Socket(key="key", name="Key", kind=bpy.types.NodeSocketFloat)
LUMINANCE: Final = Socket(key="luminance", name="Luminance", kind=bpy.types.NodeSocketFloat)
_COVERAGE: Final = Socket(key="coverage", name="Coverage", kind=bpy.types.NodeSocketFloat)
_FRAMED: Final = (COLOR_INPUT, Socket(key="aspect", name="Aspect", kind=bpy.types.NodeSocketFloat), Socket(key="scale", name="Scale", kind=bpy.types.NodeSocketFloat))
_RUNNING: Final = Socket(key="running", name="Running", kind=bpy.types.NodeSocketFloat)

# --- [MODELS] ---------------------------------------------------------------------------


class Shape(Case, frozen=True, kw_only=True):
    """Box or ellipse with feathered edge."""

    outline: Annotated[Outline, msgspec.Meta(description="Box or ellipse")] = Outline.ELLIPSE
    position: Annotated[Position, msgspec.Meta(description="Center in frame fractions")] = (0.5, 0.5)
    size: Annotated[tuple[float, float], msgspec.Meta(description="Half extents over greater frame side"), Presentation(quantity=Some(Quantity.FRAME))] = (0.1, 0.05)
    rotation: Annotated[float, msgspec.Meta(description="Turn about center"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    feather: Annotated[FrameLength, msgspec.Meta(description="Edge blur radius over greater frame side"), Presentation(soft=Some((0.0, 0.1)))] = 0.0


class Band(msgspec.Struct, frozen=True, kw_only=True):
    """Selected interval with smoothstep feather beyond each bound."""

    low: Annotated[float, msgspec.Meta(description="Lower bound of full selection")]
    high: Annotated[float, msgspec.Meta(description="Upper bound of full selection")]
    softness: Annotated[float, msgspec.Meta(ge=0.0, description="Ramp width beyond each bound")]


class HueRange(msgspec.Struct, frozen=True, kw_only=True):
    """Circular HSY hue interval with smoothstep feather beyond each edge."""

    center: Annotated[float, msgspec.Meta(description="HSY hue in radians, magenta at zero and red at one sixth turn"), Presentation(quantity=Some(Quantity.ANGLE))] = tau / 6.0
    width: Annotated[float, msgspec.Meta(ge=0.0, le=tau, description="Hue span of full selection"), Presentation(quantity=Some(Quantity.ANGLE))] = radians(30.0)
    softness: Annotated[float, msgspec.Meta(ge=0.0, le=pi, description="Ramp width beyond each edge"), Presentation(quantity=Some(Quantity.ANGLE))] = radians(10.0)


class Qualifier(Case, frozen=True, kw_only=True):
    """Pixels selected by HSY hue, saturation, and luminance."""

    hue: Annotated[Gated[HueRange], msgspec.Meta(description="HSY hue range")] = Gated(enabled=True, value=HueRange())
    saturation: Annotated[Gated[Band], msgspec.Meta(description="HSY saturation range")] = Gated(enabled=False, value=Band(low=0.2, high=1.0, softness=0.1))
    luminance: Annotated[Gated[Band], msgspec.Meta(description="HSY luminance in LIN-wrapped stops from gray")] = Gated(enabled=False, value=Band(low=-2.0, high=2.0, softness=1.0))


class Transition(Case, frozen=True, kw_only=True):
    """Spatial wipe revealing a layer as completion rises."""

    completion: Annotated[Unit, msgspec.Meta(description="Travel share, 0 hidden and 1 shown"), Presentation(soft=Some((0.0, 1.0)))] = 0.5
    angle: Annotated[float, msgspec.Meta(description="Travel direction, 0 running left to right"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    softness: Annotated[Unit, msgspec.Meta(description="Edge ramp width over travel"), Presentation(soft=Some((0.0, 1.0)))] = 0.1


class Highlights(msgspec.Struct, frozen=True, kw_only=True):
    """Scene-linear highlight key over a threshold with a soft knee."""

    threshold: Annotated[NonNegative, msgspec.Meta(description="Scene-linear key threshold")] = 1.0
    smoothness: Annotated[Unit, msgspec.Meta(description="Key knee softness")] = 0.1


class Mask(msgspec.Struct, frozen=True, kw_only=True):
    """Key joined with the masks before it, optionally inverted."""

    key: Annotated[MaskKey, msgspec.Meta(description="Shape, qualifier, or transition")] = Shape()
    operation: Annotated[MaskOperation, msgspec.Meta(description="Join with the masks before it")] = MaskOperation.ADD
    invert: Annotated[bool, msgspec.Meta(description="Select key complement")] = False


class TrackMatte(msgspec.Struct, frozen=True, kw_only=True):
    """Coverage read from the layer beneath, optionally inverted."""

    channel: Annotated[MatteChannel, msgspec.Meta(description="Coverage channel of the layer beneath")] = MatteChannel.ALPHA
    invert: Annotated[bool, msgspec.Meta(description="Read channel complement")] = False


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [KEYS]


def luminance(config: PyOpenColorIO.Config) -> BlenderResult[NodeGroup]:
    """Returns the working-space Y group from scene-linear conversion."""
    return converted("xyz", config, PyOpenColorIO.ROLE_SCENE_LINEAR, PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY, group_input(COLOR_INPUT.key, "RGBA")).bind(
        lambda converted_xyz: _built("working_luminance", Interface(inputs=(COLOR_INPUT,), outputs=(LUMINANCE,)), converted_xyz["Green"])
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


def highlight(source: Scalar, threshold: Scalar, smoothness: Scalar) -> Expr:
    """Unclamped scene-linear highlights of a source at full quality, read at the Glare node's `Highlights`."""
    return Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(("Image", source), ("Highlights Threshold", threshold), ("Highlights Smoothness", smoothness), ("Quality", "High"), ("Clamp Highlights", False)),
        socket="Highlights",
        shape="RGBA",
    )


@functools.cache
def shape() -> BlenderResult[NodeGroup]:
    """Returns the rotated box and ellipse key group over the displayed frame."""
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
    """Returns the corner-normalized wipe group, exact at both ends."""
    completion, angle, softness = (group_input(field.name) for field in msgspec.structs.fields(Transition))

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


def qualifier(config: PyOpenColorIO.Config) -> BlenderResult[NodeGroup]:
    """Returns the qualifier group over HSY hue and saturation under the config's luma weights, and log luminance."""
    forward = PyOpenColorIO.TRANSFORM_DIR_FORWARD

    def qualified(groups: tuple[NodeGroup, NodeGroup]) -> Expr:
        coordinates, logarithm = groups
        straight = Expr(kind=bpy.types.CompositorNodePremulKey, inputs=(("Image", group_input(COLOR_INPUT.key, "RGBA")), ("Type", "To Straight")), socket="Image", shape="RGBA")
        color = coordinates.placed({COLOR_INPUT.key: straight}, COLOR_RESULT.key)
        stops = logarithm.placed({COLOR_INPUT.key: color}, COLOR_RESULT.key)["Blue"]
        distance = pi - abs(abs(color["Red"] * tau - group_input(item_key("hue", "value", "center"))) % tau - pi)
        keys: dict[Key, Expr] = {
            "hue": edge(group_input(item_key("hue", "value", "width")) * 0.5 - distance, group_input(item_key("hue", "value", "softness"))),
            **{
                gate: band(level, **{field.name: group_input(item_key(gate, "value", field.name)) for field in msgspec.structs.fields(Band)})
                for gate, level in (("saturation", color["Green"]), ("luminance", stops))
            },
        }
        hue_gate, saturation_gate, luminance_gate = (switched(group_input(item_key(gate)), 1.0, key) for gate, key in keys.items())
        return hue_gate * saturation_gate * luminance_gate

    return paired(Interface.of(Qualifier), paired(hsy(config, forward), logarithmic(forward)).map(qualified)).bind(
        lambda parts: _built("qualifier", msgspec.structs.replace(parts[0], inputs=(COLOR_INPUT, *parts[0].inputs), outputs=(KEY,)), parts[1])
    )


def matte(working: NodeGroup) -> BlenderResult[NodeGroup]:
    """Returns the track matte group keying clamped alpha or associated working-space Y."""
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


# --- [COVERAGE]


def mask(config: PyOpenColorIO.Config) -> BlenderResult[NodeGroup]:
    """Returns the group joining one mask's key onto the running coverage of the masks before it."""
    heads = (*_FRAMED, _RUNNING)
    shared = frozenset(socket.key for socket in heads)
    cases: tuple[type[Case], ...] = typing.get_args(MaskKey.__value__)

    def placed(case: type[Case], group: NodeGroup) -> Expr:
        return group.placed(relayed(lambda key: group_input(key if key in shared else item_key("key", case, key)), group.interface), KEY.key)

    def keyed(groups: tuple[NodeGroup, ...]) -> Expr:
        keys = {case.title: placed(case, group) for case, group in zip(cases, groups)}
        key = abs(group_input("invert") - menu(group_input("key"), MenuItems.of(cases), keys, kind="FLOAT"))
        joins = {operation.label: math(operation.join[0], group_input(_RUNNING.key), key, clamp=operation.join[1]) for operation in MaskOperation}
        return menu(group_input("operation"), MenuItems.of(MaskOperation), joins, kind="FLOAT")

    return paired(Interface.of(Mask), accumulated((shape(), qualifier(config), wipe())).map(keyed)).bind(
        lambda parts: _built("mask", msgspec.structs.replace(parts[0], inputs=(*heads, *parts[0].inputs), outputs=(_COVERAGE,)), parts[1])
    )


def relayed(outer: Callable[[Key], Term], interface: Interface) -> dict[Key, Term]:
    """Returns operands for a group's input sockets and panel toggles."""

    def entering(item: DesiredItem) -> bool:
        match item:
            case InterfacePanel(toggle=toggle):
                return toggle.is_some()
            case (_, side):
                return side == "INPUT"

    return {key: outer(key) for key, item, _, _ in interface.walked() if entering(item)}


def coverage(count: int, image: Term, aspect: Term, scale: Term, outer: Callable[[Key], Expr], config: PyOpenColorIO.Config) -> BlenderResult[Option[Expr]]:
    """Returns chained coverage of `count` masks over the displayed frame, each mask's inputs at `outer(item_key(index, key))`, the first joining onto its operation's identity, Nothing at zero count."""
    first = menu(outer(item_key(1, "operation")), MenuItems.of(MaskOperation), {member.label: member.join[2] for member in MaskOperation}, kind="FLOAT") if count else 0.0

    def chained(group: NodeGroup) -> Expr:
        def placed(running: Term, index: int) -> Expr:
            heads: dict[Key, Term] = dict(zip((socket.key for socket in (*_FRAMED, _RUNNING)), (image, aspect, scale, running)))
            return group.placed(relayed(lambda key: heads[key] if key in heads else outer(item_key(index, key)), group.interface), _COVERAGE.key)

        return functools.reduce(placed, range(2, count + 1), placed(first, 1))

    return mask(config).map(lambda group: Some(chained(group))) if count else Ok(Nothing)


def _framed(key: Key, form: type[msgspec.Struct], body: Callable[[FrameSpace], Expr]) -> BlenderResult[NodeGroup]:
    """Builds a key group over the displayed frame of its image, record inputs after the frame heads."""
    image, aspect, scale = (group_input(socket.key, "RGBA" if socket is COLOR_INPUT else "FLOAT") for socket in _FRAMED)
    return Interface.of(form).bind(lambda face: _built(key, msgspec.structs.replace(face, inputs=(*_FRAMED, *face.inputs), outputs=(KEY,)), body(framed(image, aspect, scale))))


def _built(key: Key, interface: Interface, result: Term) -> BlenderResult[NodeGroup]:
    """Builds a one-output group named after its key."""
    return NodeGroup.built(key, heading(key), interface, {interface.outputs[0].key: result})


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "KEY",
    "LUMINANCE",
    "Band",
    "Highlights",
    "HueRange",
    "Mask",
    "MaskKey",
    "MaskOperation",
    "MatteChannel",
    "Outline",
    "Qualifier",
    "Shape",
    "TrackMatte",
    "Transition",
    "band",
    "coverage",
    "edge",
    "highlight",
    "luminance",
    "mask",
    "matte",
    "qualifier",
    "relayed",
    "shape",
    "wipe",
]
