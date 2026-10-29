"""Mask keys beside their node groups: shape, qualifier, wipe, highlight, and track matte keys, their interfaces, edges and bands, working luminance, and chained mask coverage."""

from collections.abc import Callable, Mapping
from enum import auto, Enum
import functools
from math import pi, radians, tau
from types import MappingProxyType
from typing import Final, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.extension.identity import Key
from blender.bpy.nodes import functions
from blender.bpy.nodes.desired import NodeGroup
from blender.bpy.nodes.functions import grouped
from blender.bpy.nodes.graphs import Expr, group_input, math, menu, Scalar, switched, Term, vector, xyz
from blender.bpy.nodes.interface import DesiredItem, Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket, under
from blender.bpy.render.compositor.colors import COLOR_INPUT, converted, hsy, logarithmic, straight
from blender.bpy.render.compositor.coordinates import framed, FrameSpace, pixels, shorter
from blender.bpy.render.compositor.shells import read
from blender.records.faults import accumulated, Outcome

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class MaskOperation(Enum):
    """Join of a key with the coverage of the masks before it."""

    ADD = auto()
    SUBTRACT = auto()
    MULTIPLY = auto()

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

    @property
    def item(self) -> MenuItem:
        """Menu item of the operation."""
        return MenuItem(name=bpy.path.display_name(self.name.lower()))


# --- [CONSTANTS] ------------------------------------------------------------------------

# --- [KEY_ITEMS]

KEY: Final = Socket(key="key", kind=bpy.types.NodeSocketFloat)
LUMINANCE: Final = Socket(key="luminance", kind=bpy.types.NodeSocketFloat)
OPERATIONS: Final = MenuItems(items=tuple(operation.item for operation in MaskOperation))
_BOX: Final = MenuItem(name="Box")
_ELLIPSE: Final = MenuItem(name="Ellipse")
OUTLINES: Final = MenuItems(items=(_BOX, _ELLIPSE))
_ALPHA: Final = MenuItem(name="Alpha")
_LUMINOUS: Final = MenuItem(name="Luminance")
CHANNELS: Final = MenuItems(items=(_ALPHA, _LUMINOUS))
_SHAPED: Final = MenuItem(name="Shape", description="Box or ellipse with feathered edge")
_QUALIFIED: Final = MenuItem(name="Qualifier", description="Pixels selected by HSY hue, saturation, and luminance")
_TRANSITIONED: Final = MenuItem(name="Transition", description="Spatial wipe revealing a layer as completion rises")

# --- [SHAPE]

_OUTLINE: Final = Socket(key="outline", kind=bpy.types.NodeSocketMenu, description="Box or ellipse", default_value=_ELLIPSE.name, items=Some(OUTLINES))
_POSITION: Final = Socket(key="position", kind=bpy.types.NodeSocketVector2D, description="Center in frame fractions", default_value=(0.5, 0.5))
_SIZE: Final = Socket(key="size", kind=bpy.types.NodeSocketVectorXYZ2D, description="Half extents over greater frame side", default_value=(0.1, 0.05))
_ROTATION: Final = Socket(key="rotation", kind=bpy.types.NodeSocketFloatAngle, description="Turn about center")
_FEATHER: Final = Socket(key="feather", kind=bpy.types.NodeSocketFloat, description="Edge blur radius over greater frame side", min_value=0.0)
SHAPE: Final = Interface(inputs=(_OUTLINE, _POSITION, _SIZE, _ROTATION, _FEATHER))

# --- [QUALIFIER]

_HUE_CENTER: Final = Socket(key="hue_center", name="Center", kind=bpy.types.NodeSocketFloatAngle, description="HSY hue, magenta at zero and red at one sixth turn", default_value=tau / 6.0)
_HUE_WIDTH: Final = Socket(key="hue_width", name="Width", kind=bpy.types.NodeSocketFloatAngle, description="Hue span of full selection", min_value=0.0, max_value=tau, default_value=radians(30.0))
_HUE_SOFTNESS: Final = Socket(
    key="hue_softness", name="Softness", kind=bpy.types.NodeSocketFloatAngle, description="Ramp width beyond each edge", min_value=0.0, max_value=pi, default_value=radians(10.0)
)
_SATURATION_LOW: Final = Socket(key="saturation_low", name="Low", kind=bpy.types.NodeSocketFloat, description="Lower bound of full selection", default_value=0.2)
_SATURATION_HIGH: Final = Socket(key="saturation_high", name="High", kind=bpy.types.NodeSocketFloat, description="Upper bound of full selection", default_value=1.0)
_SATURATION_SOFTNESS: Final = Socket(key="saturation_softness", name="Softness", kind=bpy.types.NodeSocketFloat, description="Ramp width beyond each bound", min_value=0.0, default_value=0.1)
_LUMINANCE_LOW: Final = Socket(key="luminance_low", name="Low", kind=bpy.types.NodeSocketFloat, description="Lower bound of full selection", default_value=-2.0)
_LUMINANCE_HIGH: Final = Socket(key="luminance_high", name="High", kind=bpy.types.NodeSocketFloat, description="Upper bound of full selection", default_value=2.0)
_LUMINANCE_SOFTNESS: Final = Socket(key="luminance_softness", name="Softness", kind=bpy.types.NodeSocketFloat, description="Ramp width beyond each bound", min_value=0.0, default_value=1.0)
_HUE: Final = InterfacePanel(key="hue", name="Hue", description="HSY hue range", toggle=Some(True), inputs=(_HUE_CENTER, _HUE_WIDTH, _HUE_SOFTNESS))
_SATURATION: Final = InterfacePanel(key="saturation", name="Saturation", description="HSY saturation range", toggle=Some(False), inputs=(_SATURATION_LOW, _SATURATION_HIGH, _SATURATION_SOFTNESS))
_LUMINANCE_BAND: Final = InterfacePanel(
    key="luminance", name="Luminance", description="HSY luminance in LIN-wrapped stops from gray", toggle=Some(False), inputs=(_LUMINANCE_LOW, _LUMINANCE_HIGH, _LUMINANCE_SOFTNESS)
)
QUALIFIER: Final = Interface(panels=(_HUE, _SATURATION, _LUMINANCE_BAND))

# --- [TRANSITION]

_COMPLETION: Final = Socket(key="completion", kind=bpy.types.NodeSocketFloatFactor, description="Travel share, 0 hidden and 1 shown", default_value=0.5)
_ANGLE: Final = Socket(key="angle", kind=bpy.types.NodeSocketFloatAngle, description="Travel direction, 0 running left to right")
_SOFTNESS: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloatFactor, description="Edge ramp width over travel", default_value=0.1)
TRANSITION: Final = Interface(inputs=(_COMPLETION, _ANGLE, _SOFTNESS))

# --- [HIGHLIGHTS]

HIGHLIGHTS: Final = Interface(
    inputs=(
        Socket(key="threshold", kind=bpy.types.NodeSocketFloat, description="Scene-linear key threshold", min_value=0.0, default_value=1.0),
        Socket(key="smoothness", kind=bpy.types.NodeSocketFloatFactor, description="Key knee softness", default_value=0.1),
    )
)

# --- [MASK]

_FACES: Final = MappingProxyType({_SHAPED: SHAPE, _QUALIFIED: QUALIFIER, _TRANSITIONED: TRANSITION})
KEYS: Final = MenuItems(items=tuple(_FACES))
_CHOICE: Final = Socket(key="key", kind=bpy.types.NodeSocketMenu, description="Shape, qualifier, or transition", default_value=_SHAPED.name, items=Some(KEYS))
_OPERATION: Final = Socket(key="operation", kind=bpy.types.NodeSocketMenu, description="Join with the masks before it", default_value=MaskOperation.ADD.item.name, items=Some(OPERATIONS))
_INVERT: Final = Socket(key="invert", kind=bpy.types.NodeSocketBool, description="Select key complement")
MASK: Final = Interface(
    inputs=(_CHOICE, _OPERATION, _INVERT),
    panels=tuple(
        InterfacePanel(key=prefix, name=item.name, description=item.description, inputs=keyed.inputs, panels=keyed.panels)
        for item, face in _FACES.items()
        for prefix in (item_key(_CHOICE.key, item.name.lower()),)
        for keyed in (under(prefix, face),)
    ),
)
_CHANNEL: Final = Socket(key="channel", kind=bpy.types.NodeSocketMenu, description="Coverage channel of the layer beneath", default_value=_ALPHA.name, items=Some(CHANNELS))
_COMPLEMENT: Final = Socket(key="invert", kind=bpy.types.NodeSocketBool, description="Read channel complement")
TRACK_MATTE: Final = Interface(inputs=(_CHANNEL, _COMPLEMENT))
_COVERAGE: Final = Socket(key="coverage", kind=bpy.types.NodeSocketFloat)
_ASPECT: Final = Socket(key="aspect", kind=bpy.types.NodeSocketFloat)
_SCALE: Final = Socket(key="scale", kind=bpy.types.NodeSocketFloat)
_FRAMED: Final = (COLOR_INPUT, _ASPECT, _SCALE)
_RUNNING: Final = Socket(key="running", kind=bpy.types.NodeSocketFloat)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [KEYS]


def luminance(config: PyOpenColorIO.Config, image: Term) -> Outcome[Expr]:
    """Places the working-space Y of an image."""
    return working(config).map(lambda group: functions.placed(group, {COLOR_INPUT.key: image}, LUMINANCE.key))


def working(config: PyOpenColorIO.Config) -> Outcome[NodeGroup]:
    """Returns the working-space Y group from scene-linear conversion."""
    return converted("xyz", config, PyOpenColorIO.ROLE_SCENE_LINEAR, PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY, read(COLOR_INPUT)).bind(
        lambda converted_xyz: _built("working_luminance", Interface(inputs=(COLOR_INPUT,), outputs=(LUMINANCE,)), converted_xyz["Green"])
    )


@grouped(kind=bpy.types.CompositorNodeTree)
def edge(distance: Expr, softness: Expr) -> Mapping[Key, Expr]:
    """Smoothstep edge over a signed distance, an inclusive step at zero softness."""
    soft = Expr(
        kind=bpy.types.ShaderNodeMapRange,
        inputs=(("Value", distance), ("From Min", -softness), ("From Max", 0.0), ("To Min", 0.0), ("To Max", 1.0)),
        members={"interpolation_type": "SMOOTHSTEP", "clamp": True},
        socket="Result",
    )
    return {KEY.key: switched(softness > 0.0, 1.0 - (distance < 0.0), soft)}


@grouped(kind=bpy.types.CompositorNodeTree)
def band(value: Expr, low: Expr, high: Expr, softness: Expr) -> Mapping[Key, Expr]:
    """Feathered interval key, empty when high is below low."""
    return {KEY.key: edge(value - low, softness) * edge(high - value, softness) * (1.0 - (high < low))}


def highlight(source: Scalar, threshold: Scalar, smoothness: Scalar) -> Expr:
    """Unclamped scene-linear highlights of a source at full quality, read at the Glare node's `Highlights`."""
    return Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(("Image", source), ("Highlights Threshold", threshold), ("Highlights Smoothness", smoothness), ("Quality", "High"), ("Clamp Highlights", False)),
        socket="Highlights",
        shape=bpy.types.NodeSocketColor,
    )


@functools.cache
def shape() -> Outcome[NodeGroup]:
    """Returns the rotated box and ellipse key group over the displayed frame, rotated and sized in pixel-aspect corrected frame shares no stock mask node takes."""

    def shaped(space: FrameSpace) -> Expr:
        extent, angle = read(_SIZE), read(_ROTATION)
        offset = space.point - read(_POSITION) * space.extent
        cosine, sine = math("COSINE", angle), math("SINE", angle)
        plane = xyz(offset["X"] * cosine + offset["Y"] * sine, offset["Y"] * cosine - offset["X"] * sine) / extent
        folded = abs(plane)
        inside = menu(read(_OUTLINE), {_BOX: math("MAXIMUM", folded["X"], folded["Y"]) < 1.0, _ELLIPSE: vector("DOT_PRODUCT", plane, plane) < 1.0}) * (shorter(extent) > 0.0)
        radius = pixels(space, xyz(read(_FEATHER), read(_FEATHER)), bpy.types.NodeSocketVector)
        return Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", inside), ("Size", radius), ("Type", "Fast Gaussian")), socket="Image")

    return _framed("shape", SHAPE, shaped)


@functools.cache
def wipe() -> Outcome[NodeGroup]:
    """Returns the corner-normalized wipe group, exact at both ends."""
    completion, angle, softness = read(_COMPLETION), read(_ANGLE), read(_SOFTNESS)

    def traveled(space: FrameSpace) -> Expr:
        direction = xyz(math("COSINE", angle), math("SINE", angle))
        corner = space.extent * direction
        low, high = (math(operation, corner["X"], 0.0) + math(operation, corner["Y"], 0.0) for operation in ("MINIMUM", "MAXIMUM"))
        travel = (vector("DOT_PRODUCT", space.point, direction) - low) / (high - low)
        soft = Expr(
            kind=bpy.types.ShaderNodeMapRange,
            inputs=(("Value", completion * (softness + 1.0) - travel), ("From Min", 0.0), ("From Max", softness), ("To Min", 0.0), ("To Max", 1.0)),
            members={"interpolation_type": "LINEAR", "clamp": True},
            socket="Result",
        )
        interior = switched(softness > 0.0, 1.0 - (travel > completion), soft)
        return switched(completion < 1.0, 1.0, switched(completion > 0.0, 0.0, interior))

    return _framed("wipe", TRANSITION, traveled)


def qualifier(config: PyOpenColorIO.Config) -> Outcome[NodeGroup]:
    """Returns the qualifier group over HSY hue and saturation under the config's luma weights, and log luminance, each panel's key passing while the panel is off."""
    forward = PyOpenColorIO.TRANSFORM_DIR_FORWARD

    def qualified(color: Expr, logged: Expr) -> Expr:
        stops = logged["Blue"]
        distance = pi - abs(abs(color["Red"] * tau - read(_HUE_CENTER)) % tau - pi)
        keys = (
            (_HUE, edge(read(_HUE_WIDTH) * 0.5 - distance, read(_HUE_SOFTNESS))),
            (_SATURATION, band(color["Green"], read(_SATURATION_LOW), read(_SATURATION_HIGH), read(_SATURATION_SOFTNESS))),
            (_LUMINANCE_BAND, band(stops, read(_LUMINANCE_LOW), read(_LUMINANCE_HIGH), read(_LUMINANCE_SOFTNESS))),
        )
        hue_gate, saturation_gate, luminance_gate = (switched(group_input(panel.key), 1.0, key) for panel, key in keys)
        return hue_gate * saturation_gate * luminance_gate

    return (
        hsy(config, forward, straight(read(COLOR_INPUT)))
        .bind(lambda color: logarithmic(forward, color).map(lambda logged: qualified(color, logged)))
        .bind(lambda key: _built("qualifier", msgspec.structs.replace(QUALIFIER, inputs=(COLOR_INPUT,), outputs=(KEY,)), key))
    )


def matte(working: NodeGroup) -> Outcome[NodeGroup]:
    """Returns the track matte group keying clamped alpha or associated working-space Y."""
    image = read(COLOR_INPUT)
    selected = menu(read(_CHANNEL), {_ALPHA: image["Alpha"], _LUMINOUS: functions.placed(working, {COLOR_INPUT.key: image}, LUMINANCE.key)})
    clamped = Expr(kind=bpy.types.ShaderNodeClamp, inputs=(("Value", selected), ("Min", 0.0), ("Max", 1.0)), socket="Result")
    return _built("track_matte", msgspec.structs.replace(TRACK_MATTE, inputs=(COLOR_INPUT, *TRACK_MATTE.inputs), outputs=(KEY,)), abs(read(_COMPLEMENT) - clamped))


# --- [COVERAGE]


def mask(config: PyOpenColorIO.Config) -> Outcome[NodeGroup]:
    """Returns the group joining one mask's key onto the running coverage of the masks before it."""
    heads = (*_FRAMED, _RUNNING)
    shared = frozenset(socket.key for socket in heads)
    built: Mapping[MenuItem, Outcome[NodeGroup]] = {_SHAPED: shape(), _QUALIFIED: qualifier(config), _TRANSITIONED: wipe()}

    def placed(item: MenuItem, group: NodeGroup) -> Expr:
        return functions.placed(group, relayed(lambda key: group_input(key if key in shared else item_key(_CHOICE.key, item.name.lower(), key)), group.interface), KEY.key)

    def keyed(groups: tuple[NodeGroup, ...]) -> Expr:
        key = abs(read(_INVERT) - menu(read(_CHOICE), {item: placed(item, group) for item, group in zip(built, groups, strict=True)}))
        joins = {operation.item: math(operation.join[0], read(_RUNNING), key, clamp=operation.join[1]) for operation in MaskOperation}
        return menu(read(_OPERATION), joins)

    return accumulated(built.values()).map(keyed).bind(lambda key: _built("mask", msgspec.structs.replace(MASK, inputs=(*heads, *MASK.inputs), outputs=(_COVERAGE,)), key))


def relayed(outer: Callable[[Key], Term], interface: Interface) -> dict[Key, Term]:
    """Returns operands for a group's input sockets and panel toggles."""

    def entering(item: DesiredItem) -> bool:
        match item:
            case InterfacePanel(toggle=toggle):
                return toggle.is_some()
            case (_, side):
                return side == "INPUT"

    return {key: outer(key) for key, item, _, _ in interface.walked() if entering(item)}


def masked(count: int) -> tuple[InterfacePanel, ...]:
    """Mask panels of a blend over `count` masks, each mask's items under `<index>`."""
    return tuple(InterfacePanel(key=str(index), name=f"Mask {index}", inputs=face.inputs, panels=face.panels) for index in range(1, count + 1) for face in (under(str(index), MASK),))


def coverage(count: int, image: Term, aspect: Term, scale: Term, outer: Callable[[Key], Expr], config: PyOpenColorIO.Config) -> Outcome[Option[Expr]]:
    """Returns chained coverage of `count` masks over the displayed frame, each mask's inputs at `outer(item_key(index, key))`, the first joining onto its operation's identity, Nothing at zero count."""
    first = menu(outer(item_key(1, _OPERATION.key)), {operation.item: operation.join[2] for operation in MaskOperation}) if count else 0.0
    heads = (COLOR_INPUT.key, _ASPECT.key, _SCALE.key, _RUNNING.key)

    def chained(group: NodeGroup) -> Expr:
        def placed(running: Term, index: int) -> Expr:
            held: dict[Key, Term] = {COLOR_INPUT.key: image, _ASPECT.key: aspect, _SCALE.key: scale, _RUNNING.key: running}
            return functions.placed(group, relayed(lambda key: held[key] if key in heads else outer(item_key(index, key)), group.interface), _COVERAGE.key)

        return functools.reduce(placed, range(2, count + 1), placed(first, 1))

    return mask(config).map(lambda group: Some(chained(group))) if count else Ok(Nothing)


def _framed(key: Key, face: Interface, body: Callable[[FrameSpace], Expr]) -> Outcome[NodeGroup]:
    """Builds a key group over the displayed frame of its image, its inputs after the frame heads."""
    return _built(key, msgspec.structs.replace(face, inputs=(*_FRAMED, *face.inputs), outputs=(KEY,)), body(framed(*(read(socket) for socket in _FRAMED))))


def _built(key: Key, interface: Interface, result: Term) -> Outcome[NodeGroup]:
    """Builds a one-output group named after its key."""
    return functions.built(key, bpy.path.display_name(key), interface, {interface.outputs[0].key: result}, bpy.types.CompositorNodeTree)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CHANNELS",
    "HIGHLIGHTS",
    "KEY",
    "KEYS",
    "LUMINANCE",
    "MASK",
    "OPERATIONS",
    "OUTLINES",
    "QUALIFIER",
    "SHAPE",
    "TRACK_MATTE",
    "TRANSITION",
    "MaskOperation",
    "band",
    "coverage",
    "edge",
    "highlight",
    "luminance",
    "mask",
    "masked",
    "matte",
    "qualifier",
    "relayed",
    "shape",
    "wipe",
    "working",
]
