"""Ink lines from rendered discontinuities over optional edge-preserving paint."""

import math
import sys
from typing import Annotated

from expression import Some
import msgspec

from blender.imaging.chain import Effect, Stage
from blender.imaging.parameters import Case, Choice, Finite, Gated, Presentation, Quantity, Ramp, Rgb

# --- [TYPES] ----------------------------------------------------------------------------


class IdSource(Choice):
    """Passes the object and material terms compare."""

    CRYPTOMATTE = 1, "Cryptomatte"
    INDEX = 2, "Pass Index"


class Placement(Choice):
    """Side of a contour covered by an outer line."""

    CENTER = 1, "Center"
    INSIDE = 2, "Inside"
    OUTSIDE = 3, "Outside"


type Paints = Anisotropic | Classic
type Widths = FrameWidth | WorldWidth

# --- [MODELS] ---------------------------------------------------------------------------


class Paint(Case, frozen=True, kw_only=True):
    """Edge-preserving smoothing that flattens texture into painted regions."""

    size: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Size", description="Radius of the filter window"), Presentation(soft=Some((0.0, 0.02)), quantity=Some(Quantity.FRAME_GREATER))
    ] = 1 / 320


class Anisotropic(Paint, frozen=True, kw_only=True, tag=1):
    """Kuwahara filtering over elliptical sectors aligned with local image structure."""

    title = "Anisotropic"
    uniformity: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Uniformity", description="Smoothing radius of local stroke directions"),
        Presentation(soft=Some((0.0, 0.01)), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 1 / 480
    sharpness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Sharpness", description="Weight toward least varied sectors, 0 smooth and 1 sharp")] = 1.0
    eccentricity: Annotated[float, msgspec.Meta(ge=0.0, le=2.0, title="Eccentricity", description="Sector elongation along edges, 0 least and 2 most elongated")] = 1.0


class Classic(Paint, frozen=True, kw_only=True, tag=2):
    """Kuwahara filtering by the mean of the least varied square quadrant."""

    title = "Classic"
    precise: Annotated[bool, msgspec.Meta(title="High Precision", description="Direct quadrant sums instead of summed-area tables")] = False


class FrameWidth(Case, frozen=True, kw_only=True, tag=1):
    """Line width as a fraction of the frame's greater side."""

    title = "Frame"
    size: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Width", description="Width of the line"), Presentation(soft=Some((0.0, 0.01)), quantity=Some(Quantity.FRAME_GREATER))] = (
        1 / 640
    )


class WorldWidth(Case, frozen=True, kw_only=True, tag=2):
    """Line width as a surface length, narrowing with distance from the camera."""

    title = "World"
    size: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Width", description="Width of the line on its surface"), Presentation(quantity=Some(Quantity.LENGTH))] = 0.00635
    limit: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Limit", description="Maximum width as the surface nears the camera"),
        Presentation(soft=Some((0.0, 0.02)), quantity=Some(Quantity.FRAME_GREATER)),
    ] = 1 / 240


class Outer(msgspec.Struct, frozen=True, kw_only=True):
    """Lines at occluding surfaces and object boundaries, drawn above inner lines."""

    depth: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]],
        msgspec.Meta(title="Depth Gap", description="Least relative gap, 1 - near/far, at an occluding surface"),
        Presentation(soft=Some((0.0, 0.25))),
    ] = Gated(enabled=True, value=0.05)
    objects: Annotated[bool, msgspec.Meta(title="Object Changes", description="Lines at object boundaries")] = False
    ink: Annotated[Rgb[Finite], msgspec.Meta(title="Ink", description="Display color of outer lines")] = Rgb(red=0.0, green=0.0, blue=0.0)
    width: Annotated[Widths, msgspec.Meta(title="Width", description="Width of outer lines")] = FrameWidth()
    placement: Annotated[Placement, msgspec.Meta(title="Placement", description="Contour side covered by outer lines, inside on the nearer surface")] = Placement.CENTER


class Inner(msgspec.Struct, frozen=True, kw_only=True):
    """Lines at surface turns, material boundaries, and image contrast."""

    crease: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=0.0, le=math.pi)]],
        msgspec.Meta(title="Crease Angle", description="Least angle between neighboring surface normals that draws a line"),
        Presentation(quantity=Some(Quantity.ANGLE)),
    ] = Gated(enabled=True, value=math.pi - math.radians(134.4))
    materials: Annotated[bool, msgspec.Meta(title="Material Changes", description="Lines at material boundaries")] = False
    contrast: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]],
        msgspec.Meta(title="Contrast", description="Least luminance-weighted display-color change, 1 from black to white"),
        Presentation(soft=Some((0.0, 0.5))),
    ] = Gated(enabled=False, value=0.1)
    ink: Annotated[Rgb[Finite], msgspec.Meta(title="Ink", description="Display color of inner lines")] = Rgb(red=0.0, green=0.0, blue=0.0)
    width: Annotated[Widths, msgspec.Meta(title="Width", description="Width of inner lines")] = FrameWidth(size=1 / 1280)


class Taper(msgspec.Struct, frozen=True, kw_only=True):
    """Width scaling through a ramp over line depth."""

    near: Annotated[
        float, msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Near", description="Distance at ramp position 1; depth d reads near / d"), Presentation(quantity=Some(Quantity.LENGTH))
    ] = 3.048
    ramp: Annotated[Ramp, msgspec.Meta(title="Ramp", description="Width factor over near / depth, read as each color's value")] = Ramp()


class Edges(Effect, frozen=True, kw_only=True, tag=42):
    """Ink lines from depth, identity, normal, and image changes over optional paint."""

    stage = Stage.EDGES
    title = "Edges"
    pointwise = False
    paint: Annotated[Gated[Paints], msgspec.Meta(title="Paint", description="Edge-preserving smoothing ahead of lines")] = Gated(enabled=False, value=Anisotropic())
    outer: Annotated[Gated[Outer], msgspec.Meta(title="Outer Lines", description="Lines at occluding surfaces and object boundaries")] = Gated(enabled=True, value=Outer())
    inner: Annotated[Gated[Inner], msgspec.Meta(title="Inner Lines", description="Lines at surface turns, material boundaries, and image contrast")] = Gated(enabled=True, value=Inner())
    source: Annotated[IdSource, msgspec.Meta(title="ID Source", description="Cryptomatte names or hand-set pass indices for object and material terms")] = IdSource.CRYPTOMATTE
    taper: Annotated[Gated[Taper], msgspec.Meta(title="Depth Taper", description="Width scaling with line depth")] = Gated(enabled=False, value=Taper())
    paper: Annotated[Gated[Rgb[Finite]], msgspec.Meta(title="Paper", description="Display color beneath lines, replacing the image")] = Gated(enabled=False, value=Rgb(red=1.0, green=1.0, blue=1.0))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Anisotropic", "Classic", "Edges", "FrameWidth", "IdSource", "Inner", "Outer", "Paint", "Paints", "Placement", "Taper", "Widths", "WorldWidth"]
