"""Mask keys beside their node groups: shape, qualifier, wipe, highlight, and track matte keys, their interfaces, edges and bands, working luminance, chained mask coverage, and the blend of a result over its base through mode, opacity, and masks."""

from collections.abc import Callable, Mapping
from enum import Enum
import functools
from math import pi, radians, tau
from types import MappingProxyType
from typing import Annotated, Final, TYPE_CHECKING

import bpy
from expression import Option, Some
import msgspec
import psdtags
import PyOpenColorIO

from blender.bpy.extension.identity import Key
from blender.bpy.nodes import functions
from blender.bpy.nodes.desired import item_key, MenuItem, MenuItems
from blender.bpy.nodes.functions import grouped
from blender.bpy.nodes.graphs import Expr, group_input, math, menu, mixed, Scalar, switched, Term, vector, xyz
from blender.bpy.nodes.interfaces import DesiredItem, Interface, InterfacePanel, NodeGroup, Socket, StockInput
from blender.bpy.render.compositor.colors import COLOR_INPUT, converted, hsy, logarithmic, straight, with_alpha
from blender.bpy.render.compositor.coordinates import FrameSpace, shorter
from blender.bpy.render.compositor.inputs import read
from blender.bpy.render.compositor.shapes import turned
from blender.records.faults import accumulated, Nothing, Ok, Outcome

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class MaskOperation(Enum):
    """Join of a key with the coverage of the masks before it, clamped to unit coverage."""

    ADD = ("MAXIMUM", 0.0)
    SUBTRACT = ("SUBTRACT", 1.0)
    MULTIPLY = ("MULTIPLY", 1.0)

    def __init__(self, operation: "rna_enums.NodeMathItems", identity: float) -> None:
        """Holds the Math operation joining the key onto running coverage and the coverage the first mask joins onto."""
        self.operation: rna_enums.NodeMathItems = operation
        self.identity = identity


# --- [CONSTANTS] ------------------------------------------------------------------------

# --- [KEY_ITEMS]

KEY: Final = Socket(key="key", kind=bpy.types.NodeSocketFloat)
LUMINANCE: Final = Socket(key="luminance", kind=bpy.types.NodeSocketFloat)
OPERATIONS: Final = MenuItems.of(MaskOperation)
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

_OUTLINE: Final = Socket(key="outline", kind=bpy.types.NodeSocketMenu, description="Box or ellipse", default_value=_ELLIPSE.name, items=OUTLINES.items)
_POSITION: Final = Socket(key="position", kind=bpy.types.NodeSocketVector2D, description="Center in frame fractions", default_value=(0.5, 0.5))
_SIZE: Final = Socket(key="size", kind=bpy.types.NodeSocketVectorXYZ2D, description="Half extents over greater frame side", default_value=(0.1, 0.05))
_ROTATION: Final = Socket(key="rotation", kind=bpy.types.NodeSocketFloatAngle, description="Turn about center")
_FEATHER: Final = Socket(key="feather", kind=bpy.types.NodeSocketFloat, description="Edge blur radius over greater frame side", min_value=0.0)
SHAPE: Final = Interface(inputs=(_OUTLINE, _POSITION, _SIZE, _ROTATION, _FEATHER))

# --- [QUALIFIER]

_CENTER: Final = Socket(key="center", kind=bpy.types.NodeSocketFloatAngle, description="HSY hue, magenta at zero and red at one sixth turn", default_value=tau / 6.0)
_WIDTH: Final = Socket(key="width", kind=bpy.types.NodeSocketFloatAngle, description="Hue span of full selection", min_value=0.0, max_value=tau, default_value=radians(30.0))
_EDGE: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloatAngle, description="Ramp width beyond each edge", min_value=0.0, max_value=pi, default_value=radians(10.0))
_LOW: Final = Socket(key="low", kind=bpy.types.NodeSocketFloat, description="Lower bound of full selection")
_HIGH: Final = Socket(key="high", kind=bpy.types.NodeSocketFloat, description="Upper bound of full selection")
_RAMP: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloat, description="Ramp width beyond each bound", min_value=0.0)
_BAND: Final = Interface(inputs=(_LOW, _HIGH, _RAMP))
_HUE: Final = InterfacePanel.of("hue", Interface(inputs=(_CENTER, _WIDTH, _EDGE)), description="HSY hue range", toggle=Some(True))
_SATURATION: Final = InterfacePanel.of("saturation", _BAND.defaulted({_LOW.key: 0.2, _HIGH.key: 1.0, _RAMP.key: 0.1}), description="HSY saturation range", toggle=Some(False))
_LUMINANCE_BAND: Final = InterfacePanel.of(
    "luminance", _BAND.defaulted({_LOW.key: -2.0, _HIGH.key: 2.0, _RAMP.key: 1.0}), description="HSY luminance in LIN-wrapped stops from gray", toggle=Some(False)
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
        Socket(key="threshold", kind=StockInput(node=bpy.types.CompositorNodeGlare, input="Highlights Threshold")),
        Socket(key="smoothness", kind=StockInput(node=bpy.types.CompositorNodeGlare, input="Highlights Smoothness")),
    )
)

# --- [MASK]

_KEY_INTERFACES: Final = MappingProxyType({_SHAPED: SHAPE, _QUALIFIED: QUALIFIER, _TRANSITIONED: TRANSITION})
KEYS: Final = MenuItems(items=tuple(_KEY_INTERFACES))
_CHOICE: Final = Socket(key="key", kind=bpy.types.NodeSocketMenu, description="Shape, qualifier, or transition", default_value=_SHAPED.name, items=KEYS.items)
_OPERATION: Final = Socket(key="operation", kind=bpy.types.NodeSocketMenu, description="Join with the masks before it", default_value=MenuItem.of(MaskOperation.ADD).name, items=OPERATIONS.items)
_INVERT: Final = Socket(key="invert", kind=bpy.types.NodeSocketBool, description="Select key complement")
MASK: Final = Interface(
    inputs=(_CHOICE, _OPERATION, _INVERT),
    panels=tuple(InterfacePanel.of(item_key(_CHOICE.key, item.name.lower()), interface, name=item.name, description=item.description) for item, interface in _KEY_INTERFACES.items()),
)
_CHANNEL: Final = Socket(key="channel", kind=bpy.types.NodeSocketMenu, description="Coverage channel of the layer beneath", default_value=_ALPHA.name, items=CHANNELS.items)
_COMPLEMENT: Final = Socket(key="invert", kind=bpy.types.NodeSocketBool, description="Read channel complement")
TRACK_MATTE: Final = Interface(inputs=(_CHANNEL, _COMPLEMENT))
_COVERAGE: Final = Socket(key="coverage", kind=bpy.types.NodeSocketFloat)
_ASPECT: Final = Socket(key="aspect", kind=bpy.types.NodeSocketFloat)
_SCALE: Final = Socket(key="scale", kind=bpy.types.NodeSocketFloat)
_FRAMED: Final = (COLOR_INPUT, _ASPECT, _SCALE)
_RUNNING: Final = Socket(key="running", kind=bpy.types.NodeSocketFloat)

# --- [BLENDS]

BLEND: Final[Key] = "blend"
_MASKS: Final[Key] = "masks"
FORMULAS: Final[bpy.types.EnumProperty] = bpy.types.ShaderNodeMix.bl_rna.properties["blend_type"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
MODES: Final = MenuItems.of(FORMULAS)
MODE: Final = Socket(key="mode", name="Blend", kind=bpy.types.NodeSocketMenu, description="Mix node formula mixing result with base", default_value=FORMULAS.enum_items["MIX"].name, items=MODES.items)
OPACITY: Final = Socket(key="opacity", kind=bpy.types.NodeSocketFloatFactor, description="Share of result mixed over base", default_value=1.0)
_MIX_BASE: Final = Socket(key="base", kind=bpy.types.NodeSocketColor)
_MIX_RESULT: Final = Socket(key="result", kind=bpy.types.NodeSocketColor)
_MIX_WEIGHT: Final = msgspec.structs.replace(OPACITY, key="weight")

# --- [MODELS] ---------------------------------------------------------------------------


class Blend(msgspec.Struct, frozen=True, kw_only=True):
    """Count of the masks limiting where a result mixes over its base, mode and opacity held by the blend sockets."""

    masks: Annotated[int, msgspec.Meta(ge=0)] = 0

    @property
    def interface(self) -> Interface:
        """Mode and opacity sockets, then one panel per mask under `masks`."""
        return Interface(inputs=(MODE, OPACITY), panels=(InterfacePanel.of(_MASKS, Interface(panels=masked(self.masks))),) if self.masks else ())

    @property
    def panel(self) -> InterfacePanel:
        """Blend panel of an entry group, its items keyed under `BLEND`."""
        return InterfacePanel.of(BLEND, self.interface)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [KEYS]


def luminance(config: PyOpenColorIO.Config, image: Term) -> Outcome[Expr]:
    """Places the working-space Y of an image."""
    return luminance_group(config).map(lambda group: functions.placed(group, {COLOR_INPUT.key: image}, LUMINANCE.key))


def luminance_group(config: PyOpenColorIO.Config) -> Outcome[NodeGroup]:
    """Returns the working-space Y group from scene-linear conversion."""
    return converted("xyz", config, PyOpenColorIO.ROLE_SCENE_LINEAR, PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY, read(COLOR_INPUT)).bind(
        lambda xyz_space: functions.built("working_luminance", Interface(inputs=(COLOR_INPUT,), outputs=(LUMINANCE,)), {LUMINANCE.key: xyz_space["Green"]}, bpy.types.CompositorNodeTree)
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
        inputs=(("Image", source), ("Highlights Threshold", threshold), ("Highlights Smoothness", smoothness), ("Quality", "High")),
        socket="Highlights",
        socket_type=bpy.types.NodeSocketColor,
    )


@functools.cache
def shape() -> Outcome[NodeGroup]:
    """Returns the rotated box and ellipse key group over the displayed frame, rotated and sized in pixel-aspect corrected frame shares no stock mask node takes."""

    def shaped(space: FrameSpace) -> Expr:
        extent = read(_SIZE)
        plane = turned(space, read(_POSITION), read(_ROTATION)) / extent
        folded = abs(plane)
        inside = menu(read(_OUTLINE), {_BOX: math("MAXIMUM", folded["X"], folded["Y"]) < 1.0, _ELLIPSE: vector("DOT_PRODUCT", plane, plane) < 1.0}) * (shorter(extent) > 0.0)
        return Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", inside), ("Size", space.pixel_size(read(_FEATHER)))), socket="Image")

    return _framed("shape", SHAPE, shaped)


def outlined(image: Term, aspect: Term, scale: Term, outer: Callable[[Socket], Term]) -> Outcome[Expr]:
    """Places the shape key group over the displayed frame of an image, each `SHAPE` input read through `outer` (a reader prefixing the key of a panel holding `SHAPE`).

    Args:
        image: Image whose displayed frame the outline sits in.
        aspect: Pixel aspect `pixel_aspect_x / pixel_aspect_y`.
        scale: Camera frame share of each image side.
        outer: Operand of each `SHAPE` socket.

    Returns:
        `Ok` of the outline key. `Error` of the shape group's build faults.
    """
    framed: dict[Key, Term] = {COLOR_INPUT.key: image, _ASPECT.key: aspect, _SCALE.key: scale}
    return shape().map(lambda group: functions.placed(group, {**framed, **{socket.key: outer(socket) for socket in SHAPE.inputs}}, KEY.key))


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
        revealed = switched(softness > 0.0, 1.0 - (travel > completion), soft)
        return switched(completion < 1.0, 1.0, switched(completion > 0.0, 0.0, revealed))

    return _framed("wipe", TRANSITION, traveled)


def qualifier(config: PyOpenColorIO.Config) -> Outcome[NodeGroup]:
    """Returns the qualifier group over HSY hue and saturation under the config's luma weights, and log luminance, each panel's key passing while the panel is off."""
    forward = PyOpenColorIO.TRANSFORM_DIR_FORWARD

    def qualified(color: Expr, logged: Expr) -> Expr:
        center, width, softness = map(read, _HUE.inputs)
        distance = pi - abs(abs(color["Red"] * tau - center) % tau - pi)
        keys = (
            (_HUE, edge(width * 0.5 - distance, softness)),
            (_SATURATION, band(color["Green"], *map(read, _SATURATION.inputs))),
            (_LUMINANCE_BAND, band(logged["Blue"], *map(read, _LUMINANCE_BAND.inputs))),
        )
        hue_gate, saturation_gate, luminance_gate = (switched(group_input(panel.key), 1.0, key) for panel, key in keys)
        return hue_gate * saturation_gate * luminance_gate

    return (
        hsy(config, forward, straight(read(COLOR_INPUT)))
        .bind(lambda color: logarithmic(forward, color).map(lambda logged: qualified(color, logged)))
        .bind(lambda key: functions.built("qualifier", msgspec.structs.replace(QUALIFIER, inputs=(COLOR_INPUT,), outputs=(KEY,)), {KEY.key: key}, bpy.types.CompositorNodeTree))
    )


def matte(luminance_group: NodeGroup) -> Outcome[NodeGroup]:
    """Returns the track matte group keying clamped alpha or associated working-space Y."""
    image = read(COLOR_INPUT)
    selected = menu(read(_CHANNEL), {_ALPHA: image["Alpha"], _LUMINOUS: functions.placed(luminance_group, {COLOR_INPUT.key: image}, LUMINANCE.key)})
    clamped = Expr(kind=bpy.types.ShaderNodeClamp, inputs=(("Value", selected), ("Min", 0.0), ("Max", 1.0)), socket="Result")
    interface = msgspec.structs.replace(TRACK_MATTE, inputs=(COLOR_INPUT, *TRACK_MATTE.inputs), outputs=(KEY,))
    return functions.built("track_matte", interface, {KEY.key: abs(read(_COMPLEMENT) - clamped)}, bpy.types.CompositorNodeTree)


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
        joins = {MenuItem.of(operation): math(operation.operation, read(_RUNNING), key, clamp=True) for operation in MaskOperation}
        return menu(read(_OPERATION), joins)

    interface = msgspec.structs.replace(MASK, inputs=(*heads, *MASK.inputs), outputs=(_COVERAGE,))
    return accumulated(built.values()).map(keyed).bind(lambda key: functions.built("mask", interface, {_COVERAGE.key: key}, bpy.types.CompositorNodeTree))


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
    return tuple(InterfacePanel.of(str(index), MASK, name=f"Mask {index}") for index in range(1, count + 1))


def coverage(count: int, image: Term, aspect: Term, scale: Term, outer: Callable[[Key], Expr], config: PyOpenColorIO.Config) -> Outcome[Option[Expr]]:
    """Returns chained coverage of `count` masks over the displayed frame, each mask's inputs at `outer(item_key(index, key))`, the first joining onto its operation's identity, Nothing at zero count."""
    first = menu(outer(item_key(1, _OPERATION.key)), {MenuItem.of(operation): operation.identity for operation in MaskOperation}) if count else 0.0

    def chained(group: NodeGroup) -> Expr:
        def placed(running: Term, index: int) -> Expr:
            held: dict[Key, Term] = {COLOR_INPUT.key: image, _ASPECT.key: aspect, _SCALE.key: scale, _RUNNING.key: running}
            return functions.placed(group, relayed(lambda key: held[key] if key in held else outer(item_key(index, key)), group.interface), _COVERAGE.key)

        return functools.reduce(placed, range(2, count + 1), placed(first, 1))

    return mask(config).map(lambda group: Some(chained(group))) if count else Ok(Nothing)


def _framed(key: Key, interface: Interface, body: Callable[[FrameSpace], Expr]) -> Outcome[NodeGroup]:
    """Builds a key group over the displayed frame of its image, its inputs after the frame heads."""
    framed = msgspec.structs.replace(interface, inputs=(*_FRAMED, *interface.inputs), outputs=(KEY,))
    return functions.built(key, framed, {KEY.key: body(FrameSpace.of(*(read(socket) for socket in _FRAMED)))}, bpy.types.CompositorNodeTree)


# --- [BLENDS]


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
    base, result = read(_MIX_BASE), read(_MIX_RESULT)
    background, foreground = straight(base), straight(result)
    pure = menu(read(MODE), {item: mixed(1.0, background, foreground, blend=formula.identifier) for formula, item in zip(FORMULAS.enum_items, MODES.items, strict=True)})  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    covered = with_alpha(with_alpha(mixed(base["Alpha"], foreground, pure), 1.0), result["Alpha"], masked=True)
    blended = switched(pure.at(FORMULAS.enum_items["MIX"].name), covered, result)
    interface = Interface(inputs=(_MIX_BASE, _MIX_RESULT, MODE, _MIX_WEIGHT), outputs=(COLOR_INPUT,))
    return functions.built("mixback", interface, {COLOR_INPUT.key: mixed(read(_MIX_WEIGHT), base, blended)}, bpy.types.CompositorNodeTree, name=Some("Mix Back"))


def mixed_back(key: Key, base: Term, result: Term, mode: Term, coverage: Term) -> Outcome[Expr]:
    """Places the shared mix-back group under the stored key `<key>_mixback`."""
    return mixback().map(lambda group: functions.placed(group, {_MIX_BASE.key: base, _MIX_RESULT.key: result, MODE.key: mode, _MIX_WEIGHT.key: coverage}, COLOR_INPUT.key).keyed(f"{key}_mixback"))


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

__all__ = [
    "BLEND",
    "CHANNELS",
    "FORMULAS",
    "HIGHLIGHTS",
    "KEY",
    "KEYS",
    "LUMINANCE",
    "MASK",
    "MODE",
    "MODES",
    "OPACITY",
    "OPERATIONS",
    "OUTLINES",
    "QUALIFIER",
    "SHAPE",
    "TRACK_MATTE",
    "TRANSITION",
    "Blend",
    "MaskOperation",
    "band",
    "blended",
    "coverage",
    "edge",
    "highlight",
    "luminance",
    "luminance_group",
    "mask",
    "masked",
    "matte",
    "mixback",
    "mixed_back",
    "outlined",
    "photoshop",
    "qualifier",
    "relayed",
    "shape",
    "wipe",
]
