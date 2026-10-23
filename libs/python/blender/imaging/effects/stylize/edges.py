"""Ink lines from rendered discontinuities over optional edge-preserving paint."""

import math
from typing import Annotated

from expression import Some
import msgspec

from blender.imaging.chain import Effect, Stage
from blender.imaging.parameters import Case, Choice, Finite, FrameLength, Gated, NonNegative, Positive, Presentation, Quantity, Ramp, Rgb, Unit

# --- [TYPES] ----------------------------------------------------------------------------


class IdSource(Choice):
    """Passes object and material terms compare."""

    CRYPTOMATTE = 1
    INDEX = 2, "Pass Index"


class Placement(Choice):
    """Contour side an outer line covers."""

    CENTER = 1
    INSIDE = 2
    OUTSIDE = 3


type Paints = Anisotropic | Classic
type Widths = FrameWidth | WorldWidth

# --- [MODELS] ---------------------------------------------------------------------------


class Paint(Case, frozen=True, kw_only=True):
    """Edge-preserving smoothing flattening texture into painted regions."""

    size: Annotated[FrameLength, msgspec.Meta(description="Filter window radius"), Presentation(soft=Some((0.0, 0.02)))] = 1 / 320


class Anisotropic(Paint, frozen=True, kw_only=True, tag=1):
    """Kuwahara filtering over elliptical sectors along local structure."""

    uniformity: Annotated[FrameLength, msgspec.Meta(description="Stroke direction smoothing radius"), Presentation(soft=Some((0.0, 0.01)))] = 1 / 480
    sharpness: Annotated[Unit, msgspec.Meta(description="Weight toward least varied sectors, sharpest at 1")] = 1.0
    eccentricity: Annotated[float, msgspec.Meta(ge=0.0, le=2.0, description="Sector elongation along edges, longest at 2")] = 1.0


class Classic(Paint, frozen=True, kw_only=True, tag=2):
    """Kuwahara filtering by least varied quadrant mean."""

    precise: Annotated[bool, msgspec.Meta(title="High Precision", description="Direct quadrant sums instead of summed-area tables")] = False


class FrameWidth(Case, frozen=True, kw_only=True, tag=1):
    """Line width as a frame share."""

    title = "Frame"
    size: Annotated[FrameLength, msgspec.Meta(title="Width", description="Line width"), Presentation(soft=Some((0.0, 0.01)))] = 1 / 640


class WorldWidth(Case, frozen=True, kw_only=True, tag=2):
    """Line width as a surface length, narrowing with camera distance."""

    title = "World"
    size: Annotated[NonNegative, msgspec.Meta(title="Width", description="Line width on surface"), Presentation(quantity=Some(Quantity.LENGTH))] = 0.00635
    limit: Annotated[FrameLength, msgspec.Meta(description="Width limit near camera"), Presentation(soft=Some((0.0, 0.02)))] = 1 / 240


class Outer(msgspec.Struct, frozen=True, kw_only=True):
    """Lines at occluding surfaces and object boundaries, drawn above inner lines."""

    depth: Annotated[Gated[Unit], msgspec.Meta(title="Depth Gap", description="Least relative gap (1 - near/far) at occluding surface"), Presentation(soft=Some((0.0, 0.25)))] = Gated(
        enabled=True, value=0.05
    )
    objects: Annotated[bool, msgspec.Meta(title="Object Changes", description="Lines at object boundaries")] = False
    ink: Annotated[Rgb[Finite], msgspec.Meta(description="Outer line display color")] = Rgb(red=0.0, green=0.0, blue=0.0)
    width: Annotated[Widths, msgspec.Meta(description="Outer line width")] = FrameWidth()
    placement: Annotated[Placement, msgspec.Meta(description="Contour side outer lines cover, inside on nearer surface")] = Placement.CENTER


class Inner(msgspec.Struct, frozen=True, kw_only=True):
    """Lines at surface turns, material boundaries, and image contrast."""

    crease: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=0.0, le=math.pi)]],
        msgspec.Meta(title="Crease Angle", description="Least neighboring normal angle drawing a line"),
        Presentation(quantity=Some(Quantity.ANGLE)),
    ] = Gated(enabled=True, value=math.pi - math.radians(134.4))
    materials: Annotated[bool, msgspec.Meta(title="Material Changes", description="Lines at material boundaries")] = False
    contrast: Annotated[Gated[Unit], msgspec.Meta(description="Least luminance-weighted display change, 1 from black to white"), Presentation(soft=Some((0.0, 0.5)))] = Gated(enabled=False, value=0.1)
    ink: Annotated[Rgb[Finite], msgspec.Meta(description="Inner line display color")] = Rgb(red=0.0, green=0.0, blue=0.0)
    width: Annotated[Widths, msgspec.Meta(description="Inner line width")] = FrameWidth(size=1 / 1280)


class Taper(msgspec.Struct, frozen=True, kw_only=True):
    """Width scaling through a ramp over line depth."""

    near: Annotated[Positive, msgspec.Meta(description="Distance at ramp position 1, depth d reading near / d"), Presentation(quantity=Some(Quantity.LENGTH))] = 3.048
    ramp: Annotated[Ramp, msgspec.Meta(description="Width factor over near / depth, read as each color value")] = Ramp()


class Edges(Effect, frozen=True, kw_only=True, tag=42):
    """Ink lines from depth, identity, normal, and image changes over optional paint."""

    stage = Stage.EDGES
    pointwise = False
    paint: Annotated[Gated[Paints], msgspec.Meta(description="Edge-preserving smoothing before lines")] = Gated(enabled=False, value=Anisotropic())
    outer: Annotated[Gated[Outer], msgspec.Meta(title="Outer Lines", description="Lines at occluding surfaces and object boundaries")] = Gated(enabled=True, value=Outer())
    inner: Annotated[Gated[Inner], msgspec.Meta(title="Inner Lines", description="Lines at surface turns, material boundaries, and image contrast")] = Gated(enabled=True, value=Inner())
    source: Annotated[IdSource, msgspec.Meta(title="ID Source", description="Pass identifying objects and materials")] = IdSource.CRYPTOMATTE
    taper: Annotated[Gated[Taper], msgspec.Meta(title="Depth Taper", description="Width scaling with line depth")] = Gated(enabled=False, value=Taper())
    paper: Annotated[Gated[Rgb[Finite]], msgspec.Meta(description="Display color beneath lines, replacing image")] = Gated(enabled=False, value=Rgb(red=1.0, green=1.0, blue=1.0))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Anisotropic", "Classic", "Edges", "FrameWidth", "IdSource", "Inner", "Outer", "Paint", "Paints", "Placement", "Taper", "Widths", "WorldWidth"]
