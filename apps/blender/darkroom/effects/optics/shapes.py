"""Distance-field figures, booleans, morphs, lightning, and windows painted over the frame in solid, gradient, corner, or cell colors with their light."""

from collections.abc import Callable
from enum import auto, Enum
from functools import cache, partial, reduce
from math import sqrt, tau
from typing import Annotated, Final, get_args

import bpy
from expression import Nothing, Some
import msgspec

from blender.bpy.extension.identity import Key
from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import Expr, Group, grouped, math, mixed, Scalar, switched, vector, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket, under
from blender.bpy.nodes.ramps import Ramp
from blender.bpy.render.compositor.coordinates import Border, BORDERS, clocked, FrameSpace, origin, shorter
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, precise, Requests, Shell
from blender.bpy.render.compositor.shapes import (
    angular,
    Bounded,
    box,
    coverage,
    cross,
    diamond,
    ellipse,
    grid,
    heart,
    lattice,
    line,
    linear,
    polygon,
    radial,
    rectangular,
    rotated,
    stroke,
    triangle,
    turned,
)
from blender.records.faults import accumulated, Outcome, paired
from blender.records.fields import Seed

from ...chain.stages import Effect, Stage
from .procedural import ANGLE, bases, Basis, clock, Emission, emission, emitted, FACTOR, Perlin, PLANAR, profiled, textured, Tint, WaveProfile

# --- [TYPES] ----------------------------------------------------------------------------

type Point = Annotated[Expr, bpy.types.NodeSocketVector]
type Geometry = Callable[[Expr, Callable[[str], Expr]], Expr]


class CellPattern(Enum):
    """Alternating or seeded colors of lattice cells."""

    CHECKER = auto()
    RANDOM = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_SEED: Final = get_args(Seed.__value__)[1]
_GRID: Final = (
    ("cell_width", "Horizontal spacing over greater frame side", {"min_value": 0.0, "default_value": 0.1}),
    ("cell_height", "Vertical spacing over greater frame side", {"min_value": 0.0, "default_value": 0.1}),
    ("line", "Line width over greater frame side", {"min_value": 0.0, "default_value": 0.002}),
    ("offset", "Course shift over cell width", FACTOR),
)
_FROM_START: Final = MenuItem(name="From Start")
_FROM_CENTER: Final = MenuItem(name="From Center")
_FROM_EDGES: Final = MenuItem(name="From Edges")
_GROWTHS: Final = MenuItems(items=(_FROM_START, _FROM_CENTER, _FROM_EDGES))
_UNION: Final = MenuItem(name="Union")
_SUBTRACT: Final = MenuItem(name="Subtract")
_INTERSECT: Final = MenuItem(name="Intersect")
_OPERATIONS: Final = MenuItems(items=(_UNION, _SUBTRACT, _INTERSECT))
_FRAME: Final = MenuItem(name="Frame", description="Whole frame")
_FIGURE: Final = MenuItem(name="Figure", description="Primitive placed and turned on frame")
_BOOLEAN: Final = MenuItem(name="Boolean", description="Two figures joined with a smooth seam")
_MORPH: Final = MenuItem(name="Morph", description="Blend from one figure's field to another's")
_BOX: Final = InterfacePanel(
    key="box",
    name="Box",
    description="Rectangle with rounded corners",
    inputs=tuple(
        Socket(key=item_key("box", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members)
        for field, text, members in (
            ("width", "Full width over greater frame side", {"min_value": 0.0, "default_value": 0.4}),
            ("height", "Full height over greater frame side", {"min_value": 0.0, "default_value": 0.2}),
            ("roundness", "Corner radius over shorter half side", FACTOR),
        )
    ),
)
_ELLIPSE: Final = InterfacePanel(
    key="ellipse",
    name="Ellipse",
    description="Ellipse inside width and height",
    inputs=tuple(
        Socket(key=item_key("ellipse", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members)
        for field, text, members in (
            ("width", "Full width over greater frame side", {"min_value": 0.0, "default_value": 0.4}),
            ("height", "Full height over greater frame side", {"min_value": 0.0, "default_value": 0.2}),
            ("exponent", "Superellipse power, 1 giving diamond and 2 ellipse", {"min_value": 1.0, "default_value": 2.0}),
        )
    ),
)
_TRIANGLE: Final = InterfacePanel(
    key="triangle",
    name="Triangle",
    description="Triangle with vertices offset from figure position",
    inputs=tuple(
        Socket(key=item_key("triangle", field), name=bpy.path.display_name(field), kind=kind, description=text, members=members)
        for field, kind, text, members in (
            ("vertex_a", bpy.types.NodeSocketVector, "First vertex offset over greater frame side", {**PLANAR, "default_value": (0.0, 0.1)}),
            ("vertex_b", bpy.types.NodeSocketVector, "Second vertex offset over greater frame side", {**PLANAR, "default_value": (-0.05 * sqrt(3.0), -0.05)}),
            ("vertex_c", bpy.types.NodeSocketVector, "Third vertex offset over greater frame side", {**PLANAR, "default_value": (0.05 * sqrt(3.0), -0.05)}),
            ("roundness", bpy.types.NodeSocketFloat, "Vertex rounding over incircle radius", FACTOR),
        )
    ),
)
_CROSS: Final = InterfacePanel(
    key="cross",
    name="Cross",
    description="Plus sign of two crossing bars, an X when turned 45 degrees",
    inputs=tuple(
        Socket(key=item_key("cross", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members)
        for field, text, members in (
            ("length", "Arm-to-arm length over greater frame side", {"min_value": 0.0, "default_value": 0.2}),
            ("thickness", "Bar width over length", {**FACTOR, "default_value": 0.3}),
            ("aspect", "Horizontal over vertical arm length", {"min_value": 0.0, "default_value": 1.0}),
            ("roundness", "Corner radius over arm half width", FACTOR),
        )
    ),
)
_HEART: Final = InterfacePanel(
    key="heart",
    name="Heart",
    description="Heart centered on figure position",
    inputs=(Socket(key="heart_height", name="Height", kind=bpy.types.NodeSocketFloat, description="Tip-to-lobe height over greater frame side", members={"min_value": 0.0, "default_value": 0.2}),),
)
_SEGMENT: Final = InterfacePanel(
    key="segment",
    name="Segment",
    description="Round-capped line drawn between two shares of its length",
    inputs=tuple(
        Socket(key=item_key("segment", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members)
        for field, text, members in (
            ("length", "Full length over greater frame side", {"min_value": 0.0, "default_value": 0.4}),
            ("thickness", "Line width over greater frame side", {"min_value": 0.0, "default_value": 0.005}),
            ("start", "Line start along length", FACTOR),
            ("end", "Line end along length", {**FACTOR, "default_value": 1.0}),
        )
    ),
)
_BAR: Final = InterfacePanel(
    key="bar",
    name="Bar",
    description="Rounded box filled to a completion share",
    inputs=(
        *(
            Socket(key=item_key("bar", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members)
            for field, text, members in (
                ("width", "Full width over greater frame side", {"min_value": 0.0, "default_value": 0.8}),
                ("height", "Full height over greater frame side", {"min_value": 0.0, "default_value": 0.05}),
                ("roundness", "Corner radius over shorter half side", FACTOR),
                ("completion", "Filled share of bar", {**FACTOR, "default_value": 0.5}),
            )
        ),
        Socket(key="bar_growth", name="Growth", kind=bpy.types.NodeSocketMenu, description="Fill start", members={"default_value": _FROM_START.name}, items=Some(_GROWTHS)),
    ),
)
_LATTICE: Final = InterfacePanel(
    key="grid",
    name="Grid",
    description="Lattice lines through figure position, every second course shifted",
    inputs=tuple(Socket(key=item_key("grid", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members) for field, text, members in _GRID),
)
_POLYGON: Final = InterfacePanel(
    key="polygon",
    name="Polygon",
    description="Regular polygon or inset star with rounded vertices",
    inputs=tuple(
        Socket(key=item_key("polygon", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members)
        for field, text, members in (
            ("radius", "Distance from center to outer vertex", {"min_value": 0.0, "default_value": 0.2}),
            ("inset", "Side midpoint pull toward center", FACTOR),
            ("roundness", "Vertex rounding radius over outer radius", FACTOR),
        )
    ),
)
_RING: Final = InterfacePanel(
    key="ring",
    name="Ring",
    description="Band inside a superellipse boundary",
    inputs=tuple(
        Socket(key=item_key("ring", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members)
        for field, text, members in (
            ("width", "Full boundary width over greater frame side", {"min_value": 0.0, "default_value": 0.4}),
            ("height", "Full boundary height over greater frame side", {"min_value": 0.0, "default_value": 0.2}),
            ("exponent", "Boundary superellipse power", {"min_value": 1.0, "default_value": 2.0}),
            ("thickness", "Band width over shorter half extent", {**FACTOR, "default_value": 0.1}),
        )
    ),
)
_PRIMITIVES: Final = Interface(panels=(_BOX, _ELLIPSE, _TRIANGLE, _CROSS, _HEART, _SEGMENT, _BAR, _LATTICE, _POLYGON, _RING))
_LINEAR: Final = InterfacePanel(
    key="linear",
    name="Linear",
    description="Bands across a direction from an origin, swept back into chevrons",
    inputs=(
        Socket(key="linear_origin", name="Origin", kind=bpy.types.NodeSocketVector, description="Point at first stop in frame fractions", members={**PLANAR, "default_value": (0.0, 0.5)}),
        Socket(key="linear_angle", name="Angle", kind=bpy.types.NodeSocketFloat, description="Direction toward last stop, 0 running left to right", members=ANGLE),
        Socket(
            key="linear_length", name="Length", kind=bpy.types.NodeSocketFloat, description="Distance from first stop to last over greater frame side", members={"min_value": 0.0, "default_value": 1.0}
        ),
        Socket(key="linear_sweep", name="Sweep", kind=bpy.types.NodeSocketFloat, description="Chevron slope, 0 straight and 1 at 45 degrees", members={"min_value": 0.0, "default_value": 0.0}),
    ),
)
_RADIAL: Final = InterfacePanel(
    key="radial",
    name="Radial",
    description="Rings about an origin",
    inputs=(
        Socket(key="radial_origin", name="Origin", kind=bpy.types.NodeSocketVector, description="Center in frame fractions", members={**PLANAR, "default_value": (0.5, 0.5)}),
        Socket(key="radial_angle", name="Angle", kind=bpy.types.NodeSocketFloat, description="Ellipse turn", members=ANGLE),
        Socket(key="radial_radius", name="Radius", kind=bpy.types.NodeSocketFloat, description="Distance to last stop over greater frame side", members={"min_value": 0.0, "default_value": 0.5}),
        Socket(key="radial_aspect", name="Aspect", kind=bpy.types.NodeSocketFloat, description="Horizontal radius over vertical radius", members={"min_value": 0.0, "default_value": 1.0}),
    ),
)
_ANGULAR: Final = InterfacePanel(
    key="angular",
    name="Angular",
    description="Sweep around an origin",
    inputs=(
        Socket(key="angular_origin", name="Origin", kind=bpy.types.NodeSocketVector, description="Center in frame fractions", members={**PLANAR, "default_value": (0.5, 0.5)}),
        Socket(key="angular_angle", name="Angle", kind=bpy.types.NodeSocketFloat, description="First stop direction", members=ANGLE),
    ),
)
_DIAMOND: Final = InterfacePanel(
    key="diamond",
    name="Diamond",
    description="Square rings about an origin",
    inputs=(
        Socket(key="diamond_origin", name="Origin", kind=bpy.types.NodeSocketVector, description="Center in frame fractions", members={**PLANAR, "default_value": (0.5, 0.5)}),
        Socket(key="diamond_angle", name="Angle", kind=bpy.types.NodeSocketFloat, description="Ring turn", members=ANGLE),
        Socket(key="diamond_radius", name="Radius", kind=bpy.types.NodeSocketFloat, description="Distance to last stop over greater frame side", members={"min_value": 0.0, "default_value": 0.5}),
    ),
)
_RECTANGULAR: Final = InterfacePanel(
    key="rectangular",
    name="Rectangular",
    description="Rectangular gradient rings with independent half extents",
    inputs=(
        Socket(key="rectangular_origin", name="Origin", kind=bpy.types.NodeSocketVector, description="Ring center", members={**PLANAR, "default_value": (0.5, 0.5)}),
        Socket(key="rectangular_angle", name="Angle", kind=bpy.types.NodeSocketFloat, description="Ring turn", members=ANGLE),
        Socket(key="rectangular_width", name="Width", kind=bpy.types.NodeSocketFloat, description="Horizontal distance to last stop", members={"min_value": 0.0, "default_value": 0.5}),
        Socket(key="rectangular_height", name="Height", kind=bpy.types.NodeSocketFloat, description="Vertical distance to last stop", members={"min_value": 0.0, "default_value": 0.5}),
    ),
)
_GEOMETRIES: Final = Interface(panels=(_LINEAR, _RADIAL, _ANGULAR, _DIAMOND, _RECTANGULAR))
_SOLID: Final = InterfacePanel(
    key="solid",
    name="Solid",
    description="One scene-linear color",
    inputs=(Socket(key="solid_color", name="Color", kind=bpy.types.NodeSocketColor, description="Scene-linear fill color", members={"default_value": (1.0, 1.0, 1.0, 1.0)}),),
)
_GRADIENT: Final = InterfacePanel(
    key="gradient",
    name="Gradient",
    description="Colors along a geometric parameter with optional periodic motion",
    inputs=(
        Socket(
            key="gradient_geometry",
            name="Geometry",
            kind=bpy.types.NodeSocketMenu,
            description="Band shape",
            members={"default_value": _LINEAR.name},
            items=Some(MenuItems(items=tuple(MenuItem(name=panel.name, description=panel.description) for panel in _GEOMETRIES.panels))),
        ),
        Socket(
            key="gradient_extension", name="Extension", kind=bpy.types.NodeSocketMenu, description="Parameter past ramp ends", members={"default_value": Border.CLAMP.item.name}, items=Some(BORDERS)
        ),
        Socket(key="gradient_phase", name="Phase", kind=bpy.types.NodeSocketFloat, description="Profile phase offset", members=ANGLE),
    ),
    panels=(*under("gradient", _GEOMETRIES).panels, clock("gradient_clock", speed=0.0)),
)
_CORNERS: Final = InterfacePanel(
    key="corners",
    name="Corners",
    description="Four scene-linear corner colors interpolated across frame with smootherstep",
    inputs=tuple(
        Socket(
            key=item_key("corners", corner),
            name=bpy.path.display_name(corner),
            kind=bpy.types.NodeSocketColor,
            description=f"Scene-linear {bpy.path.display_name(corner).lower()} color",
            members={"default_value": color},
        )
        for corner, color in (("top_left", (1.0, 1.0, 1.0, 1.0)), ("top_right", (0.0, 0.0, 0.0, 1.0)), ("bottom_right", (1.0, 1.0, 1.0, 1.0)), ("bottom_left", (0.0, 0.0, 0.0, 1.0)))
    ),
)
_CELLS: Final = InterfacePanel(
    key="cells",
    name="Cells",
    description="Alternating or seeded cell colors with separate mortar color",
    inputs=(
        *(Socket(key=item_key("cells", "grid", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members) for field, text, members in _GRID),
        *(
            Socket(key=item_key("cells", field), name=bpy.path.display_name(field), kind=kind, description=text, members=members)
            for field, kind, text, members in (
                ("first", bpy.types.NodeSocketColor, "Scene-linear first cell color", {"default_value": (0.0, 0.0, 0.0, 1.0)}),
                ("second", bpy.types.NodeSocketColor, "Scene-linear second cell color", {"default_value": (1.0, 1.0, 1.0, 1.0)}),
                ("mortar_color", bpy.types.NodeSocketColor, "Scene-linear mortar color", {"default_value": (1.0, 1.0, 1.0, 1.0)}),
                ("seed", bpy.types.NodeSocketInt, "Number selecting seeded cell colors", {"min_value": _SEED.ge, "max_value": _SEED.le}),
            )
        ),
    ),
)
_PAINTS: Final = Interface(panels=(_SOLID, _GRADIENT, _CORNERS, _CELLS))
_LIGHTNING: Final = InterfacePanel(
    key="lightning",
    name="Lightning",
    description="Jagged bolt between two frame points, forking from its first subdivision points",
    inputs=tuple(
        Socket(key=item_key("lightning", field), name=bpy.path.display_name(field), kind=kind, description=text, members=members)
        for field, kind, text, members in (
            ("start", bpy.types.NodeSocketVector, "Point bolt leaves in frame fractions", {**PLANAR, "default_value": (0.2, 1.0)}),
            ("end", bpy.types.NodeSocketVector, "Point bolt strikes in frame fractions", {**PLANAR, "default_value": (0.6, 0.4)}),
            ("thickness", bpy.types.NodeSocketFloat, "Channel width over greater frame side", {"min_value": 0.0, "default_value": 0.004}),
            ("displacement", bpy.types.NodeSocketFloat, "Midpoint offset over split segment", {"min_value": 0.0, "default_value": 0.5}),
            ("reach", bpy.types.NodeSocketFloat, "Fork length over bolt length", {"min_value": 0.0, "default_value": 0.4}),
            ("spread", bpy.types.NodeSocketFloat, "Fork turn off bolt direction", {**ANGLE, "min_value": 0.0, "max_value": tau / 6.0, "default_value": 0.5}),
            ("taper", bpy.types.NodeSocketFloat, "Fork root width over channel width", {**FACTOR, "default_value": 0.5}),
            ("seed", bpy.types.NodeSocketInt, "Number selecting pattern", {"min_value": _SEED.ge, "max_value": _SEED.le}),
        )
    ),
    panels=(clock("lightning_clock"),),
)
_WINDOW: Final = InterfacePanel(
    key="window",
    name="Window",
    description="Centered aspect-fitted aperture with inset and rounded corners",
    inputs=(
        Socket(key="window_inset", name="Inset", kind=bpy.types.NodeSocketFloat, description="Inset over fitted shorter half side", members=FACTOR),
        Socket(key="window_roundness", name="Roundness", kind=bpy.types.NodeSocketFloat, description="Corner radius over shorter half side", members=FACTOR),
    ),
    panels=(
        InterfacePanel(
            key="window_aspect",
            name="Aspect",
            description="Aperture width over height, frame aspect when off",
            toggle=Some(True),
            inputs=(Socket(key="window_aspect_value", name="Value", kind=bpy.types.NodeSocketFloat, description="Aperture width over height", members={"min_value": 0.0, "default_value": 2.39}),),
        ),
    ),
)
_FIELDS: Final = MenuItems(items=(_FRAME, _FIGURE, _BOOLEAN, _MORPH, MenuItem(name=_LIGHTNING.name, description=_LIGHTNING.description), MenuItem(name=_WINDOW.name, description=_WINDOW.description)))

# --- [MODELS] ---------------------------------------------------------------------------


class Shapes(Effect, frozen=True, kw_only=True):
    """Paints distance-field shapes and gradients over frame with their light, holding the polygon sides, the bolt depth and forks, the gradient ramp and wave, the cell pattern, the edge noise, and the emitted spectrum."""

    stage = Stage.GENERATE
    pointwise = False
    sides: Annotated[int, msgspec.Meta(ge=3)] = 5
    depth: Annotated[int, msgspec.Meta(ge=1)] = 5
    branches: Annotated[int, msgspec.Meta(ge=0)] = 3
    ramp: Ramp = Ramp()
    profile: WaveProfile | None = None
    pattern: CellPattern = CellPattern.CHECKER
    edge_noise: Basis = Perlin(texture=textures.Noise(noise_dimensions="4D", noise_type="RIDGED_MULTIFRACTAL", scale=2.5, detail=15.0, roughness=0.9, gain=74.5, distortion=1.9))
    emission: Emission = Emission(spectrum=Tint())


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [INTERFACES]


def figure(parts: tuple[Key, ...], primitive: InterfacePanel, position: tuple[float, float], description: str) -> InterfacePanel:
    """Figure panel keyed by `parts` and named by its last part: its primitive menu defaulting to `primitive`, placement, and one panel per primitive."""
    key = item_key(*parts)
    return InterfacePanel(
        key=key,
        name=bpy.path.display_name(parts[-1]),
        description=description,
        inputs=(
            Socket(
                key=item_key(key, "primitive"),
                name="Primitive",
                kind=bpy.types.NodeSocketMenu,
                description="Figure drawn about position",
                members={"default_value": primitive.name},
                items=Some(MenuItems(items=tuple(MenuItem(name=panel.name, description=panel.description) for panel in _PRIMITIVES.panels))),
            ),
            Socket(key=item_key(key, "position"), name="Position", kind=bpy.types.NodeSocketVector, description="Center in frame fractions", members={**PLANAR, "default_value": position}),
            Socket(key=item_key(key, "rotation"), name="Rotation", kind=bpy.types.NodeSocketFloat, description="Counterclockwise turn about position", members=ANGLE),
            Socket(key=item_key(key, "fit_short_side"), name="Fit Short Side", kind=bpy.types.NodeSocketBool, description="Measure lengths against shorter frame side"),
        ),
        panels=under(key, _PRIMITIVES).panels,
    )


# --- [LIGHTNING]


def drawn(family: Scalar, time: Scalar, seed: Scalar) -> Expr:
    """Draws one White Noise value per draw family, held time, and seed."""
    return textures.White(noise_dimensions="4D").node(vector("FLOOR", origin(seed, family)), math("FLOOR", time))


@grouped("shapes_lightning_midpoint", "Lightning Midpoint", bpy.types.CompositorNodeTree)
def bisection(start: Point, end: Point, family: Scalar, time: Scalar, seed: Scalar, displacement: Scalar) -> Expr:
    """Midpoint of a bolt segment pushed along its normal by a signed draw."""
    return mixed(0.5, start, end) + rotated(end - start, tau / 4.0) * (math("MULTIPLY_ADD", drawn(family, time, seed), 2.0, -1.0) * displacement)


@cache
def channel(depth: int) -> Group[[Point, Point, Point, Point, Scalar, Scalar, Scalar, Scalar]]:
    """Bolt channel group of `depth` levels, each splitting at a drawn midpoint between end radii in `radii` X and Y, its field packed."""

    def body(point: Point, start: Point, end: Point, radii: Point, family: Scalar, time: Scalar, seed: Scalar, displacement: Scalar) -> Expr:
        match depth:
            case 0:
                return stroke(point, start, end, radii["X"], radii["Y"]).packed
            case _:
                half = float(2 ** (depth - 1))
                split = bisection(start, end, math("ADD", family, half), time, seed, displacement)
                middle = mixed(0.5, radii["X"], radii["Y"])
                lower = channel(depth - 1)
                left = Bounded.of(lower(point, start, split, xyz(radii["X"], middle), family, time, seed, displacement))
                return left.nearer(Bounded.of(lower(point, split, end, xyz(middle, radii["Y"]), math("ADD", family, half), time, seed, displacement))).packed

    return grouped(item_key("shapes", "lightning", "channel", depth), f"Lightning Channel {depth}", bpy.types.CompositorNodeTree)(body)


# --- [REGIONS]


def fielded(shell: Shell[Shapes], frame: FrameSpace, growths: tuple[Scalar, ...] = (0.0,)) -> Outcome[tuple[Expr, ...]]:
    """Returns grown coverages of the selected region under one noisy boundary, the whole frame covered under `Frame`."""
    inlet, record = shell.input, shell.record

    def figured(key: Key) -> Outcome[Bounded]:
        inlet = partial(shell.input, key)
        scale = switched(inlet("fit_short_side"), 1.0, shorter(frame.extent))
        point = turned(frame, inlet("position"), inlet("rotation")) / scale
        width, height, completion = inlet(_BAR.key, "width"), inlet(_BAR.key, "height"), inlet(_BAR.key, "completion")
        clips = {
            _FROM_START.name: point["X"] - math("MULTIPLY_ADD", completion, 2.0, -1.0) * width * 0.5,
            _FROM_CENTER.name: abs(point["X"]) - completion * width * 0.5,
            _FROM_EDGES.name: (1.0 - completion) * width * 0.5 - abs(point["X"]),
        }

        def filled(clip: Expr) -> Bounded:
            length = width * completion
            span = math("MINIMUM", switched(msgspec.structs.replace(clip, socket=_FROM_EDGES.name, shape="FLOAT"), length, length * 0.5), height)
            return Bounded(distance=math("MAXIMUM", box(point, width, height, inlet(_BAR.key, "roundness")).distance, clip), width=span)

        def selected(bar: Bounded) -> Outcome[Bounded]:
            cases = {
                _BOX.name: box(point, inlet(_BOX.key, "width"), inlet(_BOX.key, "height"), inlet(_BOX.key, "roundness")),
                _ELLIPSE.name: ellipse(point, inlet(_ELLIPSE.key, "width"), inlet(_ELLIPSE.key, "height"), inlet(_ELLIPSE.key, "exponent")),
                _TRIANGLE.name: triangle(point, inlet(_TRIANGLE.key, "vertex_a"), inlet(_TRIANGLE.key, "vertex_b"), inlet(_TRIANGLE.key, "vertex_c"), inlet(_TRIANGLE.key, "roundness")),
                _CROSS.name: cross(point, inlet(_CROSS.key, "length"), inlet(_CROSS.key, "thickness"), inlet(_CROSS.key, "aspect"), inlet(_CROSS.key, "roundness")),
                _HEART.name: heart(point, inlet(_HEART.key, "height")),
                _SEGMENT.name: line(point, inlet(_SEGMENT.key, "length"), inlet(_SEGMENT.key, "thickness"), inlet(_SEGMENT.key, "start"), inlet(_SEGMENT.key, "end")),
                _BAR.name: bar,
                _LATTICE.name: grid(point, inlet(_LATTICE.key, "cell_width"), inlet(_LATTICE.key, "cell_height"), inlet(_LATTICE.key, "line"), inlet(_LATTICE.key, "offset")),
                _POLYGON.name: polygon(point, inlet(_POLYGON.key, "radius"), shell.record.sides, inlet(_POLYGON.key, "inset"), inlet(_POLYGON.key, "roundness")),
                _RING.name: ellipse(point, inlet(_RING.key, "width"), inlet(_RING.key, "height"), inlet(_RING.key, "exponent")).ring(inlet(_RING.key, "thickness")),
            }
            return shell.menu(key, "primitive", cases={name: case.packed for name, case in cases.items()}, kind="VECTOR").map(lambda field: Bounded.of(field * scale))

        return shell.menu(key, _BAR.key, "growth", cases=clips, kind="FLOAT").map(filled).bind(selected)

    def lightning() -> Bounded:
        bolt = partial(inlet, _LIGHTNING.key)
        time = clocked(bolt("clock", "speed"), bolt("clock", "hold")) * 2.0
        radius = bolt("thickness") * 0.5
        start, end = bolt("start") * frame.extent, bolt("end") * frame.extent
        seed, displacement = bolt("seed"), bolt("displacement")
        span = 2**record.depth
        family = textures.stream(Shapes, "lightning")

        def subdivided(first: Expr, last: Expr, radii: Expr, offset: int) -> Bounded:
            return Bounded.of(channel(record.depth)(frame.point, first, last, radii, float(family + offset), time, seed, displacement))

        @cache
        def vertex(index: int) -> Expr:
            low = index & -index
            return start if index == 0 else end if index == span else bisection(vertex(index - low), vertex(index + low), float(family + index), time, seed, displacement)

        def fork(index: int) -> Bounded:
            level = (index + 1).bit_length()
            cell, remainder = divmod((2 * (index + 1 - 2 ** (level - 1)) + 1) * span, 2**level)
            offset = span + index * (span + 2)
            root = mixed(remainder / 2**level, vertex(cell), vertex(cell + 1))
            angle = (drawn(float(family + offset), time, seed) + 0.5) * bolt("spread") * (-1.0 if index % 2 else 1.0)
            tip = root + vector("SCALE", rotated(end - start, angle), (drawn(float(family + offset + 1), time, seed) + 0.5) * bolt("reach"))
            return subdivided(root, tip, xyz(radius * bolt("taper")), offset + 2)

        return reduce(Bounded.nearer, (fork(index) for index in range(record.branches)), subdivided(start, end, xyz(radius, radius), 0))

    def window() -> Bounded:
        extent = frame.extent
        aspect = switched(inlet(_WINDOW.key, "aspect"), extent["X"] / extent["Y"], inlet(_WINDOW.key, "aspect", "value"))
        width = math("MINIMUM", extent["Y"] * aspect, extent["X"])
        height = width / aspect
        margin = math("MINIMUM", width, height) * inlet(_WINDOW.key, "inset")
        return box(frame.point - extent * 0.5, width - margin, height - margin, inlet(_WINDOW.key, "roundness"))

    def regions(figures: tuple[Bounded, ...]) -> Outcome[Expr]:
        single, base, cutter, start, end = figures
        seam = inlet("boolean", "smoothness")
        joins = {_UNION.name: base.union(cutter, seam), _SUBTRACT.name: base.difference(cutter, seam), _INTERSECT.name: base.intersection(cutter, seam)}
        return shell.menu("boolean", "operation", cases={name: join.packed for name, join in joins.items()}, kind="VECTOR").bind(
            lambda joined: shell.menu(
                "field",
                cases={
                    _FIGURE.name: single.packed,
                    _BOOLEAN.name: joined,
                    _MORPH.name: start.morphed(end, inlet("morph", "progress")).packed,
                    _LIGHTNING.name: lightning().packed,
                    _WINDOW.name: window().packed,
                },
                kind="VECTOR",
            )
        )

    def covered(selection: Expr) -> tuple[Expr, ...]:
        region = Bounded.of(selection)
        invert, softness, pixel = inlet("invert"), inlet("softness"), 1.0 / frame.greater
        ((noise, _),) = textured(shell, "edge_noise", record.edge_noise, frame.point, 0.0, inlet("seed"), streams=(textures.stream(Shapes, "edge_noise"),))
        displaced = region.distance - math("MULTIPLY_ADD", noise, 2.0, -1.0) * inlet("roughness")
        signed = switched(invert, displaced, -displaced)
        whole = msgspec.structs.replace(selection, socket=_FRAME.name, shape="FLOAT")
        return tuple(
            switched(whole, switched(invert, coverage(signed, Some(region.width), softness, pixel, growth), coverage(signed, Nothing, softness, pixel, growth)), 1.0 - invert) for growth in growths
        )

    return accumulated(figured(key) for key in ("figure", "boolean_base", "boolean_cutter", "morph_start", "morph_end")).bind(regions).map(covered)


# --- [PAINT]


def painted(shell: Shell[Shapes], frame: FrameSpace) -> Outcome[tuple[Expr, Expr]]:
    """Returns the frame-fixed fill color and its alpha coverage, the gradient's coverage zero past black ramp ends."""
    inlet, record = partial(shell.input, "fill"), shell.record

    def parameter(panel: InterfacePanel, geometry: Geometry) -> Expr:
        at = partial(inlet, _GRADIENT.key, panel.key)
        return geometry(turned(frame, at("origin"), at("angle")), at)

    def graded(coordinate: Expr) -> Outcome[tuple[Expr, Expr]]:
        cycles = coordinate + (inlet(_GRADIENT.key, "phase") / tau - clocked(inlet(_GRADIENT.key, "clock", "speed"), inlet(_GRADIENT.key, "clock", "hold")))
        held = coordinate if record.profile is None else profiled(record.profile, cycles, Expr)
        clamped = Expr(kind=bpy.types.ShaderNodeClamp, inputs=(("Value", held),), socket="Result")
        folds = {Border.BLACK: clamped, Border.CLAMP: clamped, Border.PERIODIC: math("FRACT", held), Border.MIRROR: math("PINGPONG", held, 1.0)}
        inside = (1.0 - (held < 0.0)) * (1.0 - (held > 1.0))

        def tinted(extended: Expr) -> tuple[Expr, Expr]:
            tint = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", extended),), content=Some(record.ramp), socket="Color", shape="RGBA").named("fill_gradient_ramp")
            return tint, msgspec.structs.replace(tint, socket="Alpha", shape="FLOAT") * switched(msgspec.structs.replace(extended, socket=Border.BLACK.item.name), 1.0, inside)

        return shell.menu("fill", _GRADIENT.key, "extension", cases={member.item.name: folds[member] for member in Border}, kind="FLOAT").map(tinted)

    def selected(gradient: tuple[Expr, Expr]) -> Outcome[tuple[Expr, Expr]]:
        across, up = (textures.MapRange(interpolation_type="SMOOTHERSTEP").node(frame.fraction[axis]) for axis in ("X", "Y"))
        cells = partial(inlet, _CELLS.key)
        thickness = cells("grid", "line")
        distance, cell = lattice(frame.point, cells("grid", "cell_width"), cells("grid", "cell_height"), cells("grid", "offset"), thickness)
        match record.pattern:
            case CellPattern.CHECKER:
                weight = (cell["X"] + cell["Y"]) % 2.0
            case CellPattern.RANDOM:
                weight = textures.White().node(vector("FLOOR", cell + origin(cells("seed"), float(textures.stream(Shapes, "cells")))))
        corners = partial(inlet, _CORNERS.key)
        paints = {
            _SOLID.name: inlet(_SOLID.key, "color"),
            _GRADIENT.name: gradient[0],
            _CORNERS.name: mixed(up, mixed(across, corners("bottom_left"), corners("bottom_right")), mixed(across, corners("top_left"), corners("top_right"))),
            _CELLS.name: mixed(coverage(distance, Some(thickness), 0.0, 1.0 / frame.greater), mixed(weight, cells("first"), cells("second")), cells("mortar_color")),
        }
        return shell.menu("fill", "paint", cases=paints, kind="RGBA").map(lambda tint: (tint, switched(msgspec.structs.replace(tint, socket=_GRADIENT.name, shape="FLOAT"), 1.0, gradient[1])))

    geometries = {
        _LINEAR.name: parameter(_LINEAR, lambda local, at: linear(local, at("length"), at("sweep"))),
        _RADIAL.name: parameter(_RADIAL, lambda local, at: radial(local, at("radius"), at("aspect"))),
        _ANGULAR.name: parameter(_ANGULAR, lambda local, _at: angular(local)),
        _DIAMOND.name: parameter(_DIAMOND, lambda local, at: diamond(local, at("radius"))),
        _RECTANGULAR.name: parameter(_RECTANGULAR, lambda local, at: rectangular(local, at("width"), at("height"))),
    }
    return shell.menu("fill", _GRADIENT.key, "geometry", cases=geometries, kind="FLOAT").bind(graded).bind(selected)


# --- [LIGHT]


def shaped(shell: Shell[Shapes]) -> Outcome[Expr]:
    """Returns the region's fill, outline, and emitted light with their coverage as alpha."""
    ring = partial(shell.input, "outline")

    def lit(parts: tuple[tuple[Expr, ...], tuple[Expr, Expr]]) -> Outcome[Expr]:
        (region, inner, outer), (tint, opacity) = parts
        field = region.named("shape_field")
        filled = switched(shell.input("fill"), 0.0, field * opacity)
        outlined = switched(shell.input("outline"), 0.0, outer - inner)

        def summed(lights: tuple[Expr, ...]) -> Expr:
            light = vector("SCALE", tint, filled) + vector("SCALE", ring("color"), outlined) + switched(shell.input("emission"), (0.0, 0.0, 0.0), vector("SCALE", lights[0], field))
            return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", light), ("Alpha", filled + outlined), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")

        return emitted(shell, "emission", shell.record.emission, (field,)).map(summed)

    return paired(fielded(shell, shell.space, (0.0, ring("offset"), ring("offset") + ring("width"))), painted(shell, shell.space)).bind(lit)


# --- [COMPOSITION] ----------------------------------------------------------------------

shapes = CompositorEffect(
    record=Shapes,
    graph=shaped,
    interface=Interface(
        inputs=(
            Socket(key="field", name="Field", kind=bpy.types.NodeSocketMenu, description="Region paint and light fill", members={"default_value": _FIGURE.name}, items=Some(_FIELDS)),
            *(
                Socket(key=key, name=bpy.path.display_name(key), kind=kind, description=text, members=members)
                for key, kind, text, members in (
                    ("softness", bpy.types.NodeSocketFloat, "Edge ramp past one pixel over greater frame side", {"min_value": 0.0}),
                    ("roughness", bpy.types.NodeSocketFloat, "Edge displacement amplitude", {"min_value": 0.0}),
                    ("seed", bpy.types.NodeSocketInt, "Number selecting edge noise pattern", {"min_value": _SEED.ge, "max_value": _SEED.le}),
                    ("invert", bpy.types.NodeSocketBool, "Fill outside field", {}),
                )
            ),
        ),
        panels=(
            figure(("figure",), _ELLIPSE, (0.5, 0.5), _FIGURE.description),
            InterfacePanel(
                key="boolean",
                name=_BOOLEAN.name,
                description=_BOOLEAN.description,
                inputs=(
                    Socket(
                        key="boolean_operation",
                        name="Operation",
                        kind=bpy.types.NodeSocketMenu,
                        description="Union, base minus cutter, or overlap",
                        members={"default_value": _SUBTRACT.name},
                        items=Some(_OPERATIONS),
                    ),
                    Socket(key="boolean_smoothness", name="Smoothness", kind=bpy.types.NodeSocketFloat, description="Seam blend radius over greater frame side", members={"min_value": 0.0}),
                ),
                panels=(figure(("boolean", "base"), _BOX, (0.5, 0.5), "First figure to combine"), figure(("boolean", "cutter"), _ELLIPSE, (0.6, 0.5), "Figure joined with base")),
            ),
            InterfacePanel(
                key="morph",
                name=_MORPH.name,
                description=_MORPH.description,
                inputs=(Socket(key="morph_progress", name="Progress", kind=bpy.types.NodeSocketFloat, description="Blend from start to end", members=FACTOR),),
                panels=(figure(("morph", "start"), _BOX, (0.5, 0.5), "Figure shown at progress 0"), figure(("morph", "end"), _ELLIPSE, (0.5, 0.5), "Figure shown at progress 1")),
            ),
            _LIGHTNING,
            _WINDOW,
            InterfacePanel(
                key="fill",
                name="Fill",
                description="Color covering field",
                toggle=Some(True),
                inputs=(
                    Socket(
                        key="fill_paint",
                        name="Paint",
                        kind=bpy.types.NodeSocketMenu,
                        description="Fill paint",
                        members={"default_value": _SOLID.name},
                        items=Some(MenuItems(items=tuple(MenuItem(name=panel.name, description=panel.description) for panel in _PAINTS.panels))),
                    ),
                ),
                panels=under("fill", _PAINTS).panels,
            ),
            InterfacePanel(
                key="outline",
                name="Outline",
                description="Ring outside field",
                toggle=Some(False),
                inputs=tuple(
                    Socket(key=item_key("outline", field), name=bpy.path.display_name(field), kind=kind, description=text, members=members)
                    for field, kind, text, members in (
                        ("width", bpy.types.NodeSocketFloat, "Ring width over greater frame side", {"min_value": 0.0, "default_value": 0.01}),
                        ("offset", bpy.types.NodeSocketFloat, "Gap between region and ring over greater frame side", {"min_value": 0.0}),
                        ("color", bpy.types.NodeSocketColor, "Scene-linear ring color", {"default_value": (1.0, 0.0, 0.0, 1.0)}),
                    )
                ),
            ),
            emission("emission", Shapes().emission, "Light the field adds", Some(False)),
            bases("edge_noise", Shapes().edge_noise, "Texture displacing field before coverage", "Edge Noise"),
        ),
    ),
    alpha=Alpha.RESULT,
    parts=(Requests[Shapes](request=precise),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CellPattern", "Shapes", "shapes"]
