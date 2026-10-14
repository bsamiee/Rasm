"""Distance-field figures, grids, bars, lightning, and gradients painted over the frame with their light."""

import math
import sys
from typing import Annotated

from expression import Some
import msgspec

from blender.imaging.chain import Effect, Stage
from blender.imaging.effects.generators.noise import Basis, Cut, Emission, Fractal, Interpolation, Octaves, Perlin, Profile, Tint
from blender.imaging.geometry.stmaps import Extension
from blender.imaging.parameters import Case, Choice, Finite, Gated, GraphForm, Motion, Position, Presentation, Quantity, Ramp, Rgb, Seed

# --- [TYPES] ----------------------------------------------------------------------------


class Growth(Choice):
    """Where a bar's fill starts."""

    START = 1, "From Start"
    CENTER = 2, "From Center"
    EDGES = 3, "From Edges"


class Operation(Choice):
    """Join of two figures' fields."""

    UNION = 1, "Union"
    SUBTRACT = 2, "Subtract"
    INTERSECT = 3, "Intersect"


class CellPattern(Choice):
    """Alternating or seeded colors of lattice cells."""

    CHECKER = 1, "Checker"
    RANDOM = 2, "Random"


type Primitive = Box | Ellipse | Triangle | Cross | Heart | Segment | Bar | Grid | Polygon | Ring
type Field = Frame | Figure | Boolean | Morph | Lightning | Window
type Geometry = Linear | Radial | Angular | Diamond | Rectangular
type Paint = Solid | Gradient | Corners | Cells

type _Length = Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max), Presentation(quantity=Some(Quantity.FRAME_GREATER))]
type _Extent = Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max), Presentation(quantity=Some(Quantity.FRAME_GREATER))]


# --- [MODELS] ---------------------------------------------------------------------------


class Box(Case, frozen=True, kw_only=True, tag=1):
    """Rectangle with corners rounded by a share of its shorter half side."""

    title = "Box"
    width: Annotated[_Length, msgspec.Meta(title="Width", description="Full width as a share of the frame's greater side")] = 0.4
    height: Annotated[_Length, msgspec.Meta(title="Height", description="Full height as a share of the frame's greater side")] = 0.2
    roundness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Roundness", description="Corner radius as a share of the shorter half side")] = 0.0


class Ellipse(Case, frozen=True, kw_only=True, tag=2):
    """Ellipse inside the given width and height."""

    title = "Ellipse"
    width: Annotated[_Length, msgspec.Meta(title="Width", description="Full width as a share of the frame's greater side")] = 0.4
    height: Annotated[_Length, msgspec.Meta(title="Height", description="Full height as a share of the frame's greater side")] = 0.2
    exponent: Annotated[float, msgspec.Meta(ge=1.0, le=sys.float_info.max, title="Exponent", description="Superellipse power, 1 a diamond and 2 an ellipse"), Presentation(soft=Some((1.0, 8.0)))] = 2.0


class Triangle(Case, frozen=True, kw_only=True, tag=3):
    """Triangle through three vertices placed about the figure position."""

    title = "Triangle"
    a: Annotated[
        tuple[Finite, Finite], msgspec.Meta(title="Vertex A", description="First vertex offset as a share of the frame's greater side"), Presentation(quantity=Some(Quantity.FRAME_GREATER))
    ] = (0.0, 0.1)
    b: Annotated[
        tuple[Finite, Finite], msgspec.Meta(title="Vertex B", description="Second vertex offset as a share of the frame's greater side"), Presentation(quantity=Some(Quantity.FRAME_GREATER))
    ] = (-0.05 * math.sqrt(3.0), -0.05)
    c: Annotated[
        tuple[Finite, Finite], msgspec.Meta(title="Vertex C", description="Third vertex offset as a share of the frame's greater side"), Presentation(quantity=Some(Quantity.FRAME_GREATER))
    ] = (0.05 * math.sqrt(3.0), -0.05)
    roundness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Roundness", description="Share of the incircle radius rounding the vertices")] = 0.0


class Cross(Case, frozen=True, kw_only=True, tag=4):
    """Plus sign of two crossing bars, an X when turned 45 degrees."""

    title = "Cross"
    length: Annotated[_Length, msgspec.Meta(title="Length", description="Arm-to-arm length as a share of the frame's greater side")] = 0.2
    thickness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Thickness", description="Bar width as a share of the length")] = 0.3
    aspect: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Aspect", description="Horizontal arm length over vertical arm length")] = 1.0
    roundness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Roundness", description="Corner radius as a share of the arm half width")] = 0.0


class Heart(Case, frozen=True, kw_only=True, tag=5):
    """Heart centered on the figure position."""

    title = "Heart"
    height: Annotated[_Length, msgspec.Meta(title="Height", description="Tip-to-lobe height as a share of the frame's greater side")] = 0.2


class Segment(Case, frozen=True, kw_only=True, tag=6):
    """Straight line with round caps, drawn between two shares of its length."""

    title = "Segment"
    length: Annotated[_Length, msgspec.Meta(title="Length", description="Full length as a share of the frame's greater side")] = 0.4
    thickness: Annotated[_Length, msgspec.Meta(title="Thickness", description="Line width as a share of the frame's greater side")] = 0.005
    start: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Start", description="Share of the length where the line begins")] = 0.0
    end: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="End", description="Share of the length where the line ends")] = 1.0


class Bar(Case, frozen=True, kw_only=True, tag=7):
    """Rounded box filled to a completion share."""

    title = "Bar"
    box: Annotated[Box, msgspec.Meta(title="Box", description="Bar boundary before the completion clip")] = Box(width=0.8, height=0.05)
    completion: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Completion", description="Share of the bar filled")] = 0.5
    growth: Annotated[Growth, msgspec.Meta(title="Growth", description="Where the fill starts: one end, the center, or both ends")] = Growth.START


class Grid(Case, frozen=True, kw_only=True, tag=8):
    """Lines of a rectangular lattice through the figure position, every second course shifted."""

    title = "Grid"
    width: Annotated[_Extent, msgspec.Meta(title="Cell Width", description="Line spacing across as a share of the frame's greater side")] = 0.1
    height: Annotated[_Extent, msgspec.Meta(title="Cell Height", description="Line spacing up as a share of the frame's greater side")] = 0.1
    line: Annotated[_Length, msgspec.Meta(title="Line", description="Line width as a share of the frame's greater side")] = 0.002
    offset: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Offset", description="Share of a cell width every second course shifts")] = 0.0


class Polygon(Case, frozen=True, kw_only=True, tag=9):
    """Regular polygon or inset star with rounded vertices."""

    title = "Polygon"
    radius: Annotated[_Length, msgspec.Meta(title="Radius", description="Distance from center to an outer vertex")] = 0.2
    sides: Annotated[int, msgspec.Meta(ge=3, title="Sides", description="Outer vertices around the center"), GraphForm.SETTING] = 5
    inset: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Inset", description="Share each side midpoint moves toward the center")] = 0.0
    roundness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Roundness", description="Radius rounding each vertex as a share of the outer radius")] = 0.0


class Ring(Case, frozen=True, kw_only=True, tag=10):
    """Band inside a superellipse boundary."""

    title = "Ring"
    ellipse: Annotated[Ellipse, msgspec.Meta(title="Boundary", description="Outer superellipse")] = Ellipse()
    thickness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Thickness", description="Band width as a share of the shorter half extent")] = 0.1


class Figure(Case, frozen=True, kw_only=True, tag=2):
    """One primitive placed and turned on the frame."""

    title = "Figure"
    primitive: Annotated[Primitive, msgspec.Meta(title="Primitive", description="Shape drawn about the position")] = Ellipse()
    position: Annotated[Position, msgspec.Meta(title="Position", description="Center as a share of each frame side")] = (0.5, 0.5)
    rotation: Annotated[Finite, msgspec.Meta(title="Rotation", description="Counter-clockwise turn about the position"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    short_side: Annotated[bool, msgspec.Meta(title="Fit Short Side", description="Measure primitive lengths against the shorter frame side")] = False


class Frame(Case, frozen=True, kw_only=True, tag=1):
    """Whole frame."""

    title = "Frame"


class Window(Case, frozen=True, kw_only=True, tag=6):
    """Centered aspect-fitted aperture with an inset and rounded corners."""

    title = "Window"
    aspect: Annotated[
        Gated[Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max)]], msgspec.Meta(title="Aspect", description="Aperture width over height, disabled to use the frame aspect")
    ] = Gated(enabled=True, value=2.39)
    inset: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Inset", description="Distance inside each edge as a share of the fitted shorter half side")] = 0.0
    roundness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Roundness", description="Corner radius as a share of the shorter half side")] = 0.0


class Boolean(Case, frozen=True, kw_only=True, tag=3):
    """Two figures joined by union, subtraction, or intersection with a smooth seam."""

    title = "Boolean"
    operation: Annotated[Operation, msgspec.Meta(title="Operation", description="Union, base minus cutter, or overlap")] = Operation.SUBTRACT
    base: Annotated[Figure, msgspec.Meta(title="Base", description="Figure the operation starts from")] = Figure(primitive=Box())
    cutter: Annotated[Figure, msgspec.Meta(title="Cutter", description="Figure joined with the base")] = Figure(position=(0.6, 0.5))
    smoothness: Annotated[_Length, msgspec.Meta(title="Smoothness", description="Blend radius of the seam as a share of the frame's greater side")] = 0.0


class Morph(Case, frozen=True, kw_only=True, tag=4):
    """Blend from one figure's field to another's."""

    title = "Morph"
    start: Annotated[Figure, msgspec.Meta(title="Start", description="Figure shown at progress 0")] = Figure(primitive=Box())
    end: Annotated[Figure, msgspec.Meta(title="End", description="Figure shown at progress 1")] = Figure()
    progress: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Progress", description="Share of the way from start to end")] = 0.0


class Lightning(Case, frozen=True, kw_only=True, tag=5):
    """Jagged bolt between two frame points, forking from its first subdivision points."""

    title = "Lightning"
    start: Annotated[Position, msgspec.Meta(title="Start", description="Point the bolt leaves as a share of each frame side")] = (0.2, 1.0)
    end: Annotated[Position, msgspec.Meta(title="End", description="Point the bolt strikes as a share of each frame side")] = (0.6, 0.4)
    thickness: Annotated[_Length, msgspec.Meta(title="Thickness", description="Channel width as a share of the frame's greater side")] = 0.004
    depth: Annotated[int, msgspec.Meta(ge=1, title="Depth", description="Subdivision levels of the channel"), GraphForm.SETTING] = 5
    displacement: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Displacement", description="Midpoint offset as a share of the segment it splits"), Presentation(soft=Some((0.0, 1.0)))
    ] = 0.5
    branches: Annotated[int, msgspec.Meta(ge=0, title="Branches", description="Forks leaving the channel in dyadic level order"), GraphForm.SETTING] = 3
    reach: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Reach", description="Fork length as a share of the bolt's length"), Presentation(soft=Some((0.0, 1.0)))] = 0.4
    spread: Annotated[float, msgspec.Meta(ge=0.0, le=math.pi / 3.0, title="Spread", description="Turn of each fork off the bolt's direction"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.5
    taper: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Taper", description="Fork width at its root as a share of the channel width")] = 0.5
    motion: Annotated[Motion, msgspec.Meta(title="Motion", description="Clock each bolt redraws over")] = Motion()
    seed: Annotated[Seed, msgspec.Meta(title="Seed", description="Bolt of equal statistics the entry draws")] = 0


class Linear(Case, frozen=True, kw_only=True, tag=1):
    """Bands across a direction from an origin, swept back into chevrons."""

    title = "Linear"
    origin: Annotated[Position, msgspec.Meta(title="Origin", description="Point at the first stop as a share of each frame side")] = (0.0, 0.5)
    angle: Annotated[Finite, msgspec.Meta(title="Angle", description="Direction toward the last stop, 0 running left to right"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    length: Annotated[_Extent, msgspec.Meta(title="Length", description="Distance from the first stop to the last as a share of the frame's greater side")] = 1.0
    sweep: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Sweep", description="Slope the bands bend back at either side of the axis, 0 straight and 1 at 45 degrees"),
        Presentation(soft=Some((0.0, 4.0))),
    ] = 0.0


class Radial(Case, frozen=True, kw_only=True, tag=2):
    """Rings about an origin."""

    title = "Radial"
    origin: Annotated[Position, msgspec.Meta(title="Origin", description="Center as a share of each frame side")] = (0.5, 0.5)
    radius: Annotated[_Extent, msgspec.Meta(title="Radius", description="Distance to the last stop as a share of the frame's greater side")] = 0.5
    aspect: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Aspect", description="Horizontal radius over vertical radius")] = 1.0
    angle: Annotated[Finite, msgspec.Meta(title="Angle", description="Turn of the ellipse"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class Angular(Case, frozen=True, kw_only=True, tag=3):
    """Sweep around an origin."""

    title = "Angular"
    origin: Annotated[Position, msgspec.Meta(title="Origin", description="Center as a share of each frame side")] = (0.5, 0.5)
    angle: Annotated[Finite, msgspec.Meta(title="Angle", description="Direction of the first stop"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class Diamond(Case, frozen=True, kw_only=True, tag=4):
    """Square rings about an origin."""

    title = "Diamond"
    origin: Annotated[Position, msgspec.Meta(title="Origin", description="Center as a share of each frame side")] = (0.5, 0.5)
    angle: Annotated[Finite, msgspec.Meta(title="Angle", description="Turn of the rings"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    radius: Annotated[_Extent, msgspec.Meta(title="Radius", description="Distance to the last stop as a share of the frame's greater side")] = 0.5


class Rectangular(Case, frozen=True, kw_only=True, tag=5):
    """Rectangular gradient rings with independent half extents."""

    title = "Rectangular"
    origin: Annotated[Position, msgspec.Meta(title="Origin", description="Center of the rings")] = (0.5, 0.5)
    angle: Annotated[Finite, msgspec.Meta(title="Angle", description="Turn of the rings"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    width: Annotated[_Extent, msgspec.Meta(title="Width", description="Horizontal distance to the last stop")] = 0.5
    height: Annotated[_Extent, msgspec.Meta(title="Height", description="Vertical distance to the last stop")] = 0.5


class Solid(Case, frozen=True, kw_only=True, tag=1):
    """One scene-linear color."""

    title = "Solid"
    color: Annotated[Rgb[Finite], msgspec.Meta(title="Color", description="Scene-linear color of the fill")] = Rgb(red=1.0, green=1.0, blue=1.0)


class Gradient(Case, frozen=True, kw_only=True, tag=2):
    """Colors placed along a geometric parameter with optional periodic motion."""

    title = "Gradient"
    geometry: Annotated[Geometry, msgspec.Meta(title="Geometry", description="Shape of the bands")] = Linear()
    extension: Annotated[Extension, msgspec.Meta(title="Extension", description="Parameter past the ramp's ends: none, held, repeated, or mirrored")] = Extension.EXTEND
    ramp: Annotated[Ramp, msgspec.Meta(title="Ramp", description="Scene-linear colors placed along the parameter from 0 to 1")] = Ramp()
    profile: Annotated[Gated[Profile], msgspec.Meta(title="Profile", description="Periodic wave applied to the gradient parameter"), GraphForm.SETTING] = Gated(enabled=False, value=Profile.SIN)
    phase: Annotated[Finite, msgspec.Meta(title="Phase", description="Phase of the periodic profile in radians"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    motion: Annotated[Motion, msgspec.Meta(title="Motion", description="Clock advancing the periodic profile")] = Motion(speed=0.0)


class Corners(Case, frozen=True, kw_only=True, tag=3):
    """Four scene-linear corner colors interpolated across the frame with smootherstep."""

    title = "Corners"
    top_left: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    top_right: Rgb[Finite] = Rgb(red=0.0, green=0.0, blue=0.0)
    bottom_right: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    bottom_left: Rgb[Finite] = Rgb(red=0.0, green=0.0, blue=0.0)


class Cells(Case, frozen=True, kw_only=True, tag=4):
    """Alternating or seeded cell colors with an independent mortar color."""

    title = "Cells"
    grid: Annotated[Grid, msgspec.Meta(title="Grid", description="Lattice spacing, row offset, and mortar width")] = Grid()
    pattern: Annotated[CellPattern, msgspec.Meta(title="Pattern", description="Alternating or seeded color weights"), GraphForm.SETTING] = CellPattern.CHECKER
    first: Rgb[Finite] = Rgb(red=0.0, green=0.0, blue=0.0)
    second: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    mortar: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    seed: Seed = 0


class Outline(msgspec.Struct, frozen=True, kw_only=True):
    """Ring outside the field."""

    width: Annotated[_Length, msgspec.Meta(title="Width", description="Ring width as a share of the frame's greater side")] = 0.01
    offset: Annotated[_Length, msgspec.Meta(title="Offset", description="Gap between the field and the ring as a share of the frame's greater side")] = 0.0
    color: Annotated[Rgb[Finite], msgspec.Meta(title="Color", description="Scene-linear color of the ring")] = Rgb(red=1.0, green=0.0, blue=0.0)


class Shapes(Effect, frozen=True, kw_only=True, tag=34):
    """Paints distance-field figures, grids, bars, lightning, and gradients over the frame and adds their light."""

    stage = Stage.GENERATE
    title = "Shapes"
    pointwise = False
    field: Annotated[Field, msgspec.Meta(title="Field", description="Region filled by the paint and light")] = Figure()
    fill: Annotated[Gated[Paint], msgspec.Meta(title="Fill", description="Color covering the field")] = Gated(enabled=True, value=Solid())
    outline: Annotated[Gated[Outline], msgspec.Meta(title="Outline", description="Ring outside the field")] = Gated(enabled=False, value=Outline())
    emission: Annotated[Gated[Emission], msgspec.Meta(title="Emission", description="Light the field adds")] = Gated(enabled=False, value=Emission(spectrum=Tint()))
    softness: Annotated[_Length, msgspec.Meta(title="Softness", description="Edge ramp width beyond one pixel as a share of the frame's greater side")] = 0.0
    roughness: Annotated[_Length, msgspec.Meta(title="Roughness", description="Noise amplitude displacing the geometric edge")] = 0.0
    edge_noise: Annotated[Basis, msgspec.Meta(title="Edge Noise", description="Texture displacing the field before coverage")] = Perlin(
        size=0.4, stretch=1.0, fractal=Fractal.RIDGED_MULTIFRACTAL, octaves=Octaves(detail=15.0, roughness=0.9), distortion=1.9, gain=74.5, cut=Cut(interpolation=Interpolation.LINEAR)
    )
    seed: Annotated[Seed, msgspec.Meta(title="Seed", description="Pattern of the noisy edge")] = 0
    invert: Annotated[bool, msgspec.Meta(title="Invert", description="Fill outside the field")] = False


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Angular",
    "Bar",
    "Boolean",
    "Box",
    "CellPattern",
    "Cells",
    "Corners",
    "Cross",
    "Diamond",
    "Ellipse",
    "Field",
    "Figure",
    "Frame",
    "Geometry",
    "Gradient",
    "Grid",
    "Growth",
    "Heart",
    "Lightning",
    "Linear",
    "Morph",
    "Operation",
    "Outline",
    "Paint",
    "Polygon",
    "Primitive",
    "Radial",
    "Rectangular",
    "Ring",
    "Segment",
    "Shapes",
    "Solid",
    "Triangle",
    "Window",
]
