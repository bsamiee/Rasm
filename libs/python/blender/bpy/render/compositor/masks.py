"""Mask keys beside their node groups: shape, qualifier, wipe, highlight, and track matte keys, their interfaces, edges and bands, working luminance, and chained mask coverage."""

from collections.abc import Callable, Mapping
from enum import auto, Enum
import functools
from math import pi, radians, tau
from typing import Final, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.extension.identity import Key
from blender.bpy.nodes.graphs import Expr, group_input, grouped, math, menu, NodeGroup, Scalar, switched, Term, vector, xyz
from blender.bpy.nodes.interface import DesiredItem, Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket, under
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT, converted, hsy, logarithmic
from blender.bpy.render.compositor.coordinates import framed, FrameSpace, pixels, shorter
from blender.records.faults import accumulated, Outcome, paired

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

KEY: Final = Socket(key="key", name="Key", kind=bpy.types.NodeSocketFloat)
LUMINANCE: Final = Socket(key="luminance", name="Luminance", kind=bpy.types.NodeSocketFloat)
OPERATIONS: Final = MenuItems(items=tuple(operation.item for operation in MaskOperation))
OUTLINES: Final = MenuItems(items=(MenuItem(name="Box"), MenuItem(name="Ellipse")))
CHANNELS: Final = MenuItems(items=(MenuItem(name="Alpha"), MenuItem(name="Luminance")))
KEYS: Final = MenuItems(
    items=(
        MenuItem(name="Shape", description="Box or ellipse with feathered edge"),
        MenuItem(name="Qualifier", description="Pixels selected by HSY hue, saturation, and luminance"),
        MenuItem(name="Transition", description="Spatial wipe revealing a layer as completion rises"),
    )
)
SHAPE: Final = Interface(
    inputs=(
        Socket(key="outline", name="Outline", kind=bpy.types.NodeSocketMenu, description="Box or ellipse", members={"default_value": "Ellipse"}, items=Some(OUTLINES)),
        Socket(key="position", name="Position", kind=bpy.types.NodeSocketVector, description="Center in frame fractions", members={"dimensions": 2, "default_value": (0.5, 0.5)}),
        Socket(key="size", name="Size", kind=bpy.types.NodeSocketVector, description="Half extents over greater frame side", members={"dimensions": 2, "subtype": "XYZ", "default_value": (0.1, 0.05)}),
        Socket(key="rotation", name="Rotation", kind=bpy.types.NodeSocketFloat, description="Turn about center", members={"subtype": "ANGLE"}),
        Socket(key="feather", name="Feather", kind=bpy.types.NodeSocketFloat, description="Edge blur radius over greater frame side", members={"min_value": 0.0}),
    )
)
HUE: Final = InterfacePanel(
    key="hue",
    name="Hue",
    description="HSY hue range",
    toggle=Some(True),
    inputs=(
        Socket(
            key="hue_center", name="Center", kind=bpy.types.NodeSocketFloat, description="HSY hue, magenta at zero and red at one sixth turn", members={"subtype": "ANGLE", "default_value": tau / 6.0}
        ),
        Socket(
            key="hue_width",
            name="Width",
            kind=bpy.types.NodeSocketFloat,
            description="Hue span of full selection",
            members={"subtype": "ANGLE", "min_value": 0.0, "max_value": tau, "default_value": radians(30.0)},
        ),
        Socket(
            key="hue_softness",
            name="Softness",
            kind=bpy.types.NodeSocketFloat,
            description="Ramp width beyond each edge",
            members={"subtype": "ANGLE", "min_value": 0.0, "max_value": pi, "default_value": radians(10.0)},
        ),
    ),
)
QUALIFIER: Final = Interface(
    panels=(
        HUE,
        *(
            InterfacePanel(
                key=key,
                name=bpy.path.display_name(key),
                description=description,
                toggle=Some(False),
                inputs=(
                    Socket(key=f"{key}_low", name="Low", kind=bpy.types.NodeSocketFloat, description="Lower bound of full selection", members={"default_value": low}),
                    Socket(key=f"{key}_high", name="High", kind=bpy.types.NodeSocketFloat, description="Upper bound of full selection", members={"default_value": high}),
                    Socket(key=f"{key}_softness", name="Softness", kind=bpy.types.NodeSocketFloat, description="Ramp width beyond each bound", members={"min_value": 0.0, "default_value": softness}),
                ),
            )
            for key, description, low, high, softness in (("saturation", "HSY saturation range", 0.2, 1.0, 0.1), ("luminance", "HSY luminance in LIN-wrapped stops from gray", -2.0, 2.0, 1.0))
        ),
    )
)
TRANSITION: Final = Interface(
    inputs=(
        Socket(
            key="completion",
            name="Completion",
            kind=bpy.types.NodeSocketFloat,
            description="Travel share, 0 hidden and 1 shown",
            members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0, "default_value": 0.5},
        ),
        Socket(key="angle", name="Angle", kind=bpy.types.NodeSocketFloat, description="Travel direction, 0 running left to right", members={"subtype": "ANGLE"}),
        Socket(
            key="softness",
            name="Softness",
            kind=bpy.types.NodeSocketFloat,
            description="Edge ramp width over travel",
            members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0, "default_value": 0.1},
        ),
    )
)
HIGHLIGHTS: Final = Interface(
    inputs=(
        Socket(key="threshold", name="Threshold", kind=bpy.types.NodeSocketFloat, description="Scene-linear key threshold", members={"min_value": 0.0, "default_value": 1.0}),
        Socket(
            key="smoothness",
            name="Smoothness",
            kind=bpy.types.NodeSocketFloat,
            description="Key knee softness",
            members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0, "default_value": 0.1},
        ),
    )
)
MASK: Final = Interface(
    inputs=(
        Socket(key="key", name="Key", kind=bpy.types.NodeSocketMenu, description="Shape, qualifier, or transition", members={"default_value": "Shape"}, items=Some(KEYS)),
        Socket(key="operation", name="Operation", kind=bpy.types.NodeSocketMenu, description="Join with the masks before it", members={"default_value": "Add"}, items=Some(OPERATIONS)),
        Socket(key="invert", name="Invert", kind=bpy.types.NodeSocketBool, description="Select key complement"),
    ),
    panels=tuple(
        InterfacePanel(key=prefix, name=item.name, description=item.description, inputs=keyed.inputs, panels=keyed.panels)
        for item, face in zip(KEYS.items, (SHAPE, QUALIFIER, TRANSITION))
        for prefix in (item_key("key", item.name.lower()),)
        for keyed in (under(prefix, face),)
    ),
)
TRACK_MATTE: Final = Interface(
    inputs=(
        Socket(key="channel", name="Channel", kind=bpy.types.NodeSocketMenu, description="Coverage channel of the layer beneath", members={"default_value": "Alpha"}, items=Some(CHANNELS)),
        Socket(key="invert", name="Invert", kind=bpy.types.NodeSocketBool, description="Read channel complement"),
    )
)
_COVERAGE: Final = Socket(key="coverage", name="Coverage", kind=bpy.types.NodeSocketFloat)
_FRAMED: Final = (COLOR_INPUT, Socket(key="aspect", name="Aspect", kind=bpy.types.NodeSocketFloat), Socket(key="scale", name="Scale", kind=bpy.types.NodeSocketFloat))
_RUNNING: Final = Socket(key="running", name="Running", kind=bpy.types.NodeSocketFloat)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [KEYS]


def luminance(config: PyOpenColorIO.Config) -> Outcome[NodeGroup]:
    """Returns the working-space Y group from scene-linear conversion."""
    return converted("xyz", config, PyOpenColorIO.ROLE_SCENE_LINEAR, PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY, group_input(COLOR_INPUT.key, "RGBA")).bind(
        lambda converted_xyz: _built("working_luminance", Interface(inputs=(COLOR_INPUT,), outputs=(LUMINANCE,)), converted_xyz["Green"])
    )


@grouped("soft_edge", "Soft Edge", bpy.types.CompositorNodeTree)
def edge(distance: Expr, softness: Expr) -> Mapping[Key, Expr]:
    """Smoothstep edge over a signed distance, an inclusive step at zero softness."""
    soft = Expr(
        kind=bpy.types.ShaderNodeMapRange,
        inputs=(("Value", distance), ("From Min", -softness), ("From Max", 0.0), ("To Min", 0.0), ("To Max", 1.0)),
        members=(("interpolation_type", "SMOOTHSTEP"), ("clamp", True)),
        socket="Result",
    )
    return {KEY.key: switched(softness > 0.0, 1.0 - (distance < 0.0), soft)}


@grouped("band", "Band", bpy.types.CompositorNodeTree)
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
def shape() -> Outcome[NodeGroup]:
    """Returns the rotated box and ellipse key group over the displayed frame."""

    def shaped(space: FrameSpace) -> Expr:
        extent, angle = group_input("size", "VECTOR"), group_input("rotation")
        offset = space.point - group_input("position", "VECTOR") * space.extent
        cosine, sine = math("COSINE", angle), math("SINE", angle)
        plane = xyz(offset["X"] * cosine + offset["Y"] * sine, offset["Y"] * cosine - offset["X"] * sine) / extent
        folded = abs(plane)
        keys = {"Box": math("MAXIMUM", folded["X"], folded["Y"]) < 1.0, "Ellipse": vector("DOT_PRODUCT", plane, plane) < 1.0}
        inside = menu(group_input("outline"), OUTLINES, keys, kind="FLOAT") * (shorter(extent) > 0.0)
        radius = pixels(space, xyz(group_input("feather"), group_input("feather")), "VECTOR")
        return Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", inside), ("Size", radius), ("Type", "Fast Gaussian")), socket="Image")

    return _framed("shape", SHAPE, shaped)


@functools.cache
def wipe() -> Outcome[NodeGroup]:
    """Returns the corner-normalized wipe group, exact at both ends."""
    completion, angle, softness = (group_input(socket.key) for socket in TRANSITION.inputs)

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

    return _framed("wipe", TRANSITION, traveled)


def qualifier(config: PyOpenColorIO.Config) -> Outcome[NodeGroup]:
    """Returns the qualifier group over HSY hue and saturation under the config's luma weights, and log luminance, each panel's key passing while the panel is off."""
    forward = PyOpenColorIO.TRANSFORM_DIR_FORWARD

    def qualified(groups: tuple[NodeGroup, NodeGroup]) -> Expr:
        coordinates, logarithm = groups
        straight = Expr(kind=bpy.types.CompositorNodePremulKey, inputs=(("Image", group_input(COLOR_INPUT.key, "RGBA")), ("Type", "To Straight")), socket="Image", shape="RGBA")
        color = coordinates.placed({COLOR_INPUT.key: straight}, COLOR_RESULT.key)
        stops = logarithm.placed({COLOR_INPUT.key: color}, COLOR_RESULT.key)["Blue"]
        distance = pi - abs(abs(color["Red"] * tau - group_input("hue_center")) % tau - pi)
        keys: dict[Key, Expr] = {
            "hue": edge(group_input("hue_width") * 0.5 - distance, group_input("hue_softness")),
            **{gate: band(level, group_input(f"{gate}_low"), group_input(f"{gate}_high"), group_input(f"{gate}_softness")) for gate, level in (("saturation", color["Green"]), ("luminance", stops))},
        }
        hue_gate, saturation_gate, luminance_gate = (switched(group_input(gate), 1.0, key) for gate, key in keys.items())
        return hue_gate * saturation_gate * luminance_gate

    return paired(hsy(config, forward), logarithmic(forward)).map(qualified).bind(lambda key: _built("qualifier", msgspec.structs.replace(QUALIFIER, inputs=(COLOR_INPUT,), outputs=(KEY,)), key))


def matte(working: NodeGroup) -> Outcome[NodeGroup]:
    """Returns the track matte group keying clamped alpha or associated working-space Y."""
    image = group_input(COLOR_INPUT.key, "RGBA")
    selected = menu(group_input("channel"), CHANNELS, {"Alpha": image["Alpha"], "Luminance": working.placed({COLOR_INPUT.key: image}, LUMINANCE.key)}, kind="FLOAT")
    clamped = Expr(kind=bpy.types.ShaderNodeClamp, inputs=(("Value", selected), ("Min", 0.0), ("Max", 1.0)), socket="Result")
    return _built("track_matte", msgspec.structs.replace(TRACK_MATTE, inputs=(COLOR_INPUT, *TRACK_MATTE.inputs), outputs=(KEY,)), abs(group_input("invert") - clamped))


# --- [COVERAGE]


def mask(config: PyOpenColorIO.Config) -> Outcome[NodeGroup]:
    """Returns the group joining one mask's key onto the running coverage of the masks before it."""
    heads = (*_FRAMED, _RUNNING)
    shared = frozenset(socket.key for socket in heads)

    def placed(item: MenuItem, group: NodeGroup) -> Expr:
        return group.placed(relayed(lambda key: group_input(key if key in shared else item_key("key", item.name.lower(), key)), group.interface), KEY.key)

    def keyed(groups: tuple[NodeGroup, ...]) -> Expr:
        keys = {item.name: placed(item, group) for item, group in zip(KEYS.items, groups)}
        key = abs(group_input("invert") - menu(group_input("key"), KEYS, keys, kind="FLOAT"))
        joins = {operation.item.name: math(operation.join[0], group_input(_RUNNING.key), key, clamp=operation.join[1]) for operation in MaskOperation}
        return menu(group_input("operation"), OPERATIONS, joins, kind="FLOAT")

    return accumulated((shape(), qualifier(config), wipe())).map(keyed).bind(lambda key: _built("mask", msgspec.structs.replace(MASK, inputs=(*heads, *MASK.inputs), outputs=(_COVERAGE,)), key))


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
    first = menu(outer(item_key(1, "operation")), OPERATIONS, {operation.item.name: operation.join[2] for operation in MaskOperation}, kind="FLOAT") if count else 0.0

    def chained(group: NodeGroup) -> Expr:
        def placed(running: Term, index: int) -> Expr:
            heads: dict[Key, Term] = dict(zip((socket.key for socket in (*_FRAMED, _RUNNING)), (image, aspect, scale, running)))
            return group.placed(relayed(lambda key: heads[key] if key in heads else outer(item_key(index, key)), group.interface), _COVERAGE.key)

        return functools.reduce(placed, range(2, count + 1), placed(first, 1))

    return mask(config).map(lambda group: Some(chained(group))) if count else Ok(Nothing)


def _framed(key: Key, face: Interface, body: Callable[[FrameSpace], Expr]) -> Outcome[NodeGroup]:
    """Builds a key group over the displayed frame of its image, its inputs after the frame heads."""
    image, aspect, scale = (group_input(socket.key, "RGBA" if socket is COLOR_INPUT else "FLOAT") for socket in _FRAMED)
    return _built(key, msgspec.structs.replace(face, inputs=(*_FRAMED, *face.inputs), outputs=(KEY,)), body(framed(image, aspect, scale)))


def _built(key: Key, interface: Interface, result: Term) -> Outcome[NodeGroup]:
    """Builds a one-output group named after its key."""
    return NodeGroup.built(key, bpy.path.display_name(key), interface, {interface.outputs[0].key: result}, bpy.types.CompositorNodeTree)


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
]
