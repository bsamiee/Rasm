"""Distance-field figures, grids, bars, lightning, and gradients painted over frame with their light."""

import math
from typing import Annotated

from expression import Some
import msgspec

from blender.imaging.chain import Effect, Stage
from blender.imaging.effects.generators.noise import Basis, Cut, Emission, Fractal, Interpolation, Octaves, Perlin, Profile, Tint
from blender.imaging.geometry.stmaps import Extension
from blender.imaging.parameters import (
    AtLeastOne,
    Case,
    Choice,
    Finite,
    FrameExtent,
    FrameLength,
    Gated,
    GraphForm,
    Motion,
    NonNegative,
    Position,
    Positive,
    Presentation,
    Quantity,
    Ramp,
    Rgb,
    Seed,
    Unit,
)

# --- [TYPES] ----------------------------------------------------------------------------


class Growth(Choice):
    """Where a bar's fill starts."""

    START = 1, "From Start"
    CENTER = 2, "From Center"
    EDGES = 3, "From Edges"


class Operation(Choice):
    """Join of two figure fields."""

    UNION = 1
    SUBTRACT = 2
    INTERSECT = 3


class CellPattern(Choice):
    """Alternating or seeded colors of lattice cells."""

    CHECKER = 1
    RANDOM = 2


type Primitive = Box | Ellipse | Triangle | Cross | Heart | Segment | Bar | Grid | Polygon | Ring
type Field = Frame | Figure | Boolean | Morph | Lightning | Window
type Geometry = Linear | Radial | Angular | Diamond | Rectangular
type Paint = Solid | Gradient | Corners | Cells


# --- [MODELS] ---------------------------------------------------------------------------


class Box(Case, frozen=True, kw_only=True, tag=1):
    """Rectangle with rounded corners."""

    width: Annotated[FrameLength, msgspec.Meta(description="Full width over greater frame side")] = 0.4
    height: Annotated[FrameLength, msgspec.Meta(description="Full height over greater frame side")] = 0.2
    roundness: Annotated[Unit, msgspec.Meta(description="Corner radius over shorter half side")] = 0.0


class Ellipse(Case, frozen=True, kw_only=True, tag=2):
    """Ellipse inside width and height."""

    width: Annotated[FrameLength, msgspec.Meta(description="Full width over greater frame side")] = 0.4
    height: Annotated[FrameLength, msgspec.Meta(description="Full height over greater frame side")] = 0.2
    exponent: Annotated[AtLeastOne, msgspec.Meta(description="Superellipse power, 1 giving diamond and 2 ellipse"), Presentation(soft=Some((1.0, 8.0)))] = 2.0


class Triangle(Case, frozen=True, kw_only=True, tag=3):
    """Triangle with vertices offset from figure position."""

    a: Annotated[tuple[Finite, Finite], msgspec.Meta(title="Vertex A", description="First vertex offset over greater frame side"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = (0.0, 0.1)
    b: Annotated[tuple[Finite, Finite], msgspec.Meta(title="Vertex B", description="Second vertex offset over greater frame side"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = (
        -0.05 * math.sqrt(3.0),
        -0.05,
    )
    c: Annotated[tuple[Finite, Finite], msgspec.Meta(title="Vertex C", description="Third vertex offset over greater frame side"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = (
        0.05 * math.sqrt(3.0),
        -0.05,
    )
    roundness: Annotated[Unit, msgspec.Meta(description="Vertex rounding over incircle radius")] = 0.0


class Cross(Case, frozen=True, kw_only=True, tag=4):
    """Plus sign of two crossing bars, an X when turned 45 degrees."""

    length: Annotated[FrameLength, msgspec.Meta(description="Arm-to-arm length over greater frame side")] = 0.2
    thickness: Annotated[Unit, msgspec.Meta(description="Bar width over length")] = 0.3
    aspect: Annotated[Positive, msgspec.Meta(description="Horizontal over vertical arm length")] = 1.0
    roundness: Annotated[Unit, msgspec.Meta(description="Corner radius over arm half width")] = 0.0


class Heart(Case, frozen=True, kw_only=True, tag=5):
    """Heart centered on figure position."""

    height: Annotated[FrameLength, msgspec.Meta(description="Tip-to-lobe height over greater frame side")] = 0.2


class Segment(Case, frozen=True, kw_only=True, tag=6):
    """Round-capped line drawn between two shares of its length."""

    length: Annotated[FrameLength, msgspec.Meta(description="Full length over greater frame side")] = 0.4
    thickness: Annotated[FrameLength, msgspec.Meta(description="Line width over greater frame side"), Presentation(soft=Some((0.0, 0.02)))] = 0.005
    start: Annotated[Unit, msgspec.Meta(description="Line start along length")] = 0.0
    end: Annotated[Unit, msgspec.Meta(description="Line end along length")] = 1.0


class Bar(Case, frozen=True, kw_only=True, tag=7):
    """Rounded box filled to a completion share."""

    box: Annotated[Box, msgspec.Meta(description="Bar boundary before completion clip")] = Box(width=0.8, height=0.05)
    completion: Annotated[Unit, msgspec.Meta(description="Filled share of bar")] = 0.5
    growth: Annotated[Growth, msgspec.Meta(description="Fill start")] = Growth.START


class Grid(Case, frozen=True, kw_only=True, tag=8):
    """Lattice lines through figure position, every second course shifted."""

    width: Annotated[FrameExtent, msgspec.Meta(title="Cell Width", description="Horizontal spacing over greater frame side")] = 0.1
    height: Annotated[FrameExtent, msgspec.Meta(title="Cell Height", description="Vertical spacing over greater frame side")] = 0.1
    line: Annotated[FrameLength, msgspec.Meta(description="Line width over greater frame side"), Presentation(soft=Some((0.0, 0.01)))] = 0.002
    offset: Annotated[Unit, msgspec.Meta(description="Course shift over cell width")] = 0.0


class Polygon(Case, frozen=True, kw_only=True, tag=9):
    """Regular polygon or inset star with rounded vertices."""

    radius: Annotated[FrameLength, msgspec.Meta(description="Distance from center to outer vertex")] = 0.2
    sides: Annotated[int, msgspec.Meta(ge=3, description="Outer vertices around center"), GraphForm.SETTING] = 5
    inset: Annotated[Unit, msgspec.Meta(description="Side midpoint pull toward center")] = 0.0
    roundness: Annotated[Unit, msgspec.Meta(description="Vertex rounding radius over outer radius")] = 0.0


class Ring(Case, frozen=True, kw_only=True, tag=10):
    """Band inside a superellipse boundary."""

    ellipse: Annotated[Ellipse, msgspec.Meta(title="Boundary", description="Outer superellipse")] = Ellipse()
    thickness: Annotated[Unit, msgspec.Meta(description="Band width over shorter half extent")] = 0.1


class Figure(Case, frozen=True, kw_only=True, tag=2):
    """Primitive placed and turned on frame."""

    primitive: Annotated[Primitive, msgspec.Meta(description="Figure drawn about position")] = Ellipse()
    position: Annotated[Position, msgspec.Meta(description="Center in frame fractions")] = (0.5, 0.5)
    rotation: Annotated[Finite, msgspec.Meta(description="Counterclockwise turn about position"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    short_side: Annotated[bool, msgspec.Meta(title="Fit Short Side", description="Measures lengths against shorter frame side")] = False


class Frame(Case, frozen=True, kw_only=True, tag=1):
    """Whole frame."""


class Window(Case, frozen=True, kw_only=True, tag=6):
    """Centered aspect-fitted aperture with inset and rounded corners."""

    aspect: Annotated[Gated[Positive], msgspec.Meta(description="Aperture width over height, frame aspect when off")] = Gated(enabled=True, value=2.39)
    inset: Annotated[Unit, msgspec.Meta(description="Inset over fitted shorter half side")] = 0.0
    roundness: Annotated[Unit, msgspec.Meta(description="Corner radius over shorter half side")] = 0.0


class Boolean(Case, frozen=True, kw_only=True, tag=3):
    """Two figures joined with a smooth seam."""

    operation: Annotated[Operation, msgspec.Meta(description="Union, base minus cutter, or overlap")] = Operation.SUBTRACT
    base: Annotated[Figure, msgspec.Meta(description="First figure to combine")] = Figure(primitive=Box())
    cutter: Annotated[Figure, msgspec.Meta(description="Figure joined with base")] = Figure(position=(0.6, 0.5))
    smoothness: Annotated[FrameLength, msgspec.Meta(description="Seam blend radius over greater frame side"), Presentation(soft=Some((0.0, 0.1)))] = 0.0


class Morph(Case, frozen=True, kw_only=True, tag=4):
    """Blend from one figure's field to another's."""

    start: Annotated[Figure, msgspec.Meta(description="Figure shown at progress 0")] = Figure(primitive=Box())
    end: Annotated[Figure, msgspec.Meta(description="Figure shown at progress 1")] = Figure()
    progress: Annotated[Unit, msgspec.Meta(description="Blend from start to end")] = 0.0


class Lightning(Case, frozen=True, kw_only=True, tag=5):
    """Jagged bolt between two frame points, forking from its first subdivision points."""

    start: Annotated[Position, msgspec.Meta(description="Point bolt leaves in frame fractions")] = (0.2, 1.0)
    end: Annotated[Position, msgspec.Meta(description="Point bolt strikes in frame fractions")] = (0.6, 0.4)
    thickness: Annotated[FrameLength, msgspec.Meta(description="Channel width over greater frame side"), Presentation(soft=Some((0.0, 0.02)))] = 0.004
    depth: Annotated[int, msgspec.Meta(ge=1, description="Channel subdivision levels"), GraphForm.SETTING] = 5
    displacement: Annotated[NonNegative, msgspec.Meta(description="Midpoint offset over split segment"), Presentation(soft=Some((0.0, 1.0)))] = 0.5
    branches: Annotated[int, msgspec.Meta(ge=0, description="Forks leaving channel in dyadic level order"), GraphForm.SETTING] = 3
    reach: Annotated[NonNegative, msgspec.Meta(description="Fork length over bolt length"), Presentation(soft=Some((0.0, 1.0)))] = 0.4
    spread: Annotated[float, msgspec.Meta(ge=0.0, le=math.pi / 3.0, description="Fork turn off bolt direction"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.5
    taper: Annotated[Unit, msgspec.Meta(description="Fork root width over channel width")] = 0.5
    motion: Annotated[Motion, msgspec.Meta(description="Clock each bolt redraws over")] = Motion()
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting pattern")] = 0


class Linear(Case, frozen=True, kw_only=True, tag=1):
    """Bands across a direction from an origin, swept back into chevrons."""

    origin: Annotated[Position, msgspec.Meta(description="Point at first stop in frame fractions")] = (0.0, 0.5)
    angle: Annotated[Finite, msgspec.Meta(description="Direction toward last stop, 0 running left to right"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    length: Annotated[FrameExtent, msgspec.Meta(description="Distance from first stop to last over greater frame side")] = 1.0
    sweep: Annotated[NonNegative, msgspec.Meta(description="Chevron slope, 0 straight and 1 at 45 degrees"), Presentation(soft=Some((0.0, 4.0)))] = 0.0


class Radial(Case, frozen=True, kw_only=True, tag=2):
    """Rings about an origin."""

    origin: Annotated[Position, msgspec.Meta(description="Center in frame fractions")] = (0.5, 0.5)
    radius: Annotated[FrameExtent, msgspec.Meta(description="Distance to last stop over greater frame side")] = 0.5
    aspect: Annotated[Positive, msgspec.Meta(description="Horizontal radius over vertical radius")] = 1.0
    angle: Annotated[Finite, msgspec.Meta(description="Ellipse turn"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class Angular(Case, frozen=True, kw_only=True, tag=3):
    """Sweep around an origin."""

    origin: Annotated[Position, msgspec.Meta(description="Center in frame fractions")] = (0.5, 0.5)
    angle: Annotated[Finite, msgspec.Meta(description="First stop direction"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class Diamond(Case, frozen=True, kw_only=True, tag=4):
    """Square rings about an origin."""

    origin: Annotated[Position, msgspec.Meta(description="Center in frame fractions")] = (0.5, 0.5)
    angle: Annotated[Finite, msgspec.Meta(description="Ring turn"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    radius: Annotated[FrameExtent, msgspec.Meta(description="Distance to last stop over greater frame side")] = 0.5


class Rectangular(Case, frozen=True, kw_only=True, tag=5):
    """Rectangular gradient rings with independent half extents."""

    origin: Annotated[Position, msgspec.Meta(description="Ring center")] = (0.5, 0.5)
    angle: Annotated[Finite, msgspec.Meta(description="Ring turn"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    width: Annotated[FrameExtent, msgspec.Meta(description="Horizontal distance to last stop")] = 0.5
    height: Annotated[FrameExtent, msgspec.Meta(description="Vertical distance to last stop")] = 0.5


class Solid(Case, frozen=True, kw_only=True, tag=1):
    """One scene-linear color."""

    color: Annotated[Rgb[Finite], msgspec.Meta(description="Scene-linear fill color")] = Rgb(red=1.0, green=1.0, blue=1.0)


class Gradient(Case, frozen=True, kw_only=True, tag=2):
    """Colors along a geometric parameter with optional periodic motion."""

    geometry: Annotated[Geometry, msgspec.Meta(description="Band shape")] = Linear()
    extension: Annotated[Extension, msgspec.Meta(description="Parameter past ramp ends")] = Extension.EXTEND
    ramp: Annotated[Ramp, msgspec.Meta(description="Scene-linear colors along parameter 0 to 1")] = Ramp()
    profile: Annotated[Gated[Profile], msgspec.Meta(description="Periodic wave over parameter"), GraphForm.SETTING] = Gated(enabled=False, value=Profile.SIN)
    phase: Annotated[Finite, msgspec.Meta(description="Profile phase offset"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    motion: Annotated[Motion, msgspec.Meta(description="Clock advancing profile")] = Motion(speed=0.0)


class Corners(Case, frozen=True, kw_only=True, tag=3):
    """Four scene-linear corner colors interpolated across frame with smootherstep."""

    top_left: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    top_right: Rgb[Finite] = Rgb(red=0.0, green=0.0, blue=0.0)
    bottom_right: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    bottom_left: Rgb[Finite] = Rgb(red=0.0, green=0.0, blue=0.0)


class Cells(Case, frozen=True, kw_only=True, tag=4):
    """Alternating or seeded cell colors with separate mortar color."""

    grid: Annotated[Grid, msgspec.Meta(description="Lattice spacing, row offset, and mortar width")] = Grid()
    pattern: Annotated[CellPattern, msgspec.Meta(description="Alternating or seeded color weights"), GraphForm.SETTING] = CellPattern.CHECKER
    first: Rgb[Finite] = Rgb(red=0.0, green=0.0, blue=0.0)
    second: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    mortar: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    seed: Seed = 0


class Outline(msgspec.Struct, frozen=True, kw_only=True):
    """Ring outside field."""

    width: Annotated[FrameLength, msgspec.Meta(description="Ring width over greater frame side"), Presentation(soft=Some((0.0, 0.05)))] = 0.01
    offset: Annotated[FrameLength, msgspec.Meta(description="Gap between field and ring over greater frame side"), Presentation(soft=Some((0.0, 0.05)))] = 0.0
    color: Annotated[Rgb[Finite], msgspec.Meta(description="Scene-linear ring color")] = Rgb(red=1.0, green=0.0, blue=0.0)


class Shapes(Effect, frozen=True, kw_only=True, tag=34):
    """Paints distance-field shapes and gradients over frame with their light."""

    stage = Stage.GENERATE
    pointwise = False
    field: Annotated[Field, msgspec.Meta(description="Region paint and light fill")] = Figure()
    fill: Annotated[Gated[Paint], msgspec.Meta(description="Color covering field")] = Gated(enabled=True, value=Solid())
    outline: Annotated[Gated[Outline], msgspec.Meta(description="Ring outside field")] = Gated(enabled=False, value=Outline())
    emission: Annotated[Gated[Emission], msgspec.Meta(description="Light the field adds")] = Gated(enabled=False, value=Emission(spectrum=Tint()))
    softness: Annotated[FrameLength, msgspec.Meta(description="Edge ramp past one pixel over greater frame side"), Presentation(soft=Some((0.0, 0.02)))] = 0.0
    roughness: Annotated[FrameLength, msgspec.Meta(description="Edge displacement amplitude"), Presentation(soft=Some((0.0, 0.05)))] = 0.0
    edge_noise: Annotated[Basis, msgspec.Meta(description="Texture displacing field before coverage")] = Perlin(
        size=0.4, stretch=1.0, fractal=Fractal.RIDGED_MULTIFRACTAL, octaves=Octaves(detail=15.0, roughness=0.9), distortion=1.9, gain=74.5, cut=Cut(interpolation=Interpolation.LINEAR)
    )
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting edge noise pattern")] = 0
    invert: Annotated[bool, msgspec.Meta(description="Fills outside field")] = False


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
