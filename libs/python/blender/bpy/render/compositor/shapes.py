"""Signed distance fields of planar primitives over node expressions, their smooth booleans and morphs, distance-ramp coverage, and gradient parameters."""

from functools import reduce
from itertools import starmap
from math import cos, pi, sin, tau
from typing import Literal, Self

import bpy
from expression import Option
import msgspec

from blender.bpy.nodes.graphs import equal, Expr, math, mixed, Scalar, switched, vector, where, xyz
from blender.bpy.render.compositor.coordinates import FrameSpace

# --- [MODELS] ---------------------------------------------------------------------------


class Bounded(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Signed region distance in greater-side frame shares, negative inside, and the region's local narrowest width."""

    distance: Expr
    width: Expr

    @classmethod
    def of(cls, packed: Expr) -> Self:
        """Reads the pair a `packed` vector carries."""
        return cls(distance=packed["X"], width=packed["Y"])

    @property
    def packed(self) -> Expr:
        """Distance in X and width in Y of one vector, for one Menu Switch or group output to carry."""
        return xyz(self.distance, self.width)

    def nearer(self, other: Self) -> Self:
        """Keeps per pixel whichever field lies nearer, with its width."""
        closer = other.distance < self.distance
        return type(self)(distance=where(closer, other.distance, self.distance), width=where(closer, other.width, self.width))

    def union(self, other: Self, seam: Expr) -> Self:
        """Joins both regions, blending their boundaries over `seam`."""
        return type(self)(distance=self._joined(other, seam, 1.0, 1.0), width=math("MAXIMUM", self.width, other.width))

    def difference(self, other: Self, seam: Expr) -> Self:
        """Cuts the other region out of this one, blending the cut over `seam`."""
        return type(self)(distance=self._joined(other, seam, -1.0, 1.0), width=self.width)

    def intersection(self, other: Self, seam: Expr) -> Self:
        """Keeps the overlap of both regions, blending its boundary over `seam`."""
        return type(self)(distance=self._joined(other, seam, -1.0, -1.0), width=math("MINIMUM", self.width, other.width))

    def morphed(self, other: Self, share: Scalar) -> Self:
        """Blends this field toward the other by `share`."""
        return type(self)(distance=mixed(share, self.distance, other.distance), width=mixed(share, self.width, other.width))

    def ring(self, thickness: Expr) -> Self:
        """Band inside the boundary `thickness` times a quarter of the narrowest width deep."""
        half = self.width * thickness * 0.25
        return type(self)(distance=abs(self.distance + half) - half, width=half * 2.0)

    def _joined(self, other: Self, seam: Expr, outer: float, inner: float) -> Expr:
        """Quadratic smooth minimum of the signed fields, both signs flipped to give difference and intersection."""
        left, right = self.distance * outer, other.distance * inner
        overlap = math("MAXIMUM", seam - abs(left - right), 0.0)
        return (math("MINIMUM", left, right) - overlap * overlap / (seam * 4.0)) * outer


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PLACEMENT]


def rotated(direction: Expr, angle: Scalar, *, axis: Literal["X_AXIS", "Y_AXIS", "Z_AXIS"] = "Z_AXIS", inverted: bool = False) -> Expr:
    """Turns a vector about one axis through Vector Rotate, clockwise when inverted."""
    return Expr(
        kind=bpy.types.ShaderNodeVectorRotate, inputs=(("Vector", direction), ("Angle", angle)), members={"rotation_type": axis, "invert": inverted}, socket="Vector", shape=bpy.types.NodeSocketVector
    )


def turned(frame: FrameSpace, origin: Expr, angle: Scalar) -> Expr:
    """Returns the frame position about an origin in frame fractions, turned back through angle."""
    return rotated(frame.point - origin * frame.extent, angle, inverted=True)


# --- [PRIMITIVES]


def box(point: Expr, width: Expr, height: Expr, roundness: Expr) -> Bounded:
    """Rounded box of full sides, corner radius over the shorter half side."""
    narrow = math("MINIMUM", width, height)
    radius = narrow * roundness * 0.5
    corner = abs(point) - (xyz(width, height) * 0.5 - radius * (1.0, 1.0, 0.0))
    inside = math("MINIMUM", math("MAXIMUM", corner["X"], corner["Y"]), 0.0)
    return Bounded(distance=vector("LENGTH", vector("MAXIMUM", corner, (0.0, 0.0, 0.0))) + inside - radius, width=narrow)


def stroke(point: Expr, start: Expr, end: Expr, first: Scalar, last: Scalar) -> Bounded:
    """Segment from start to end with its radius interpolated from `first` to `last`."""
    chord = end - start
    fraction = math("DIVIDE", vector("DOT_PRODUCT", point - start, chord), vector("DOT_PRODUCT", chord, chord), clamp=True)
    radius = mixed(fraction, first, last)
    return Bounded(distance=vector("LENGTH", point - (start + vector("SCALE", chord, fraction))) - radius, width=radius * 2.0)


def ellipse(point: Expr, width: Expr, height: Expr, exponent: Expr) -> Bounded:
    """Superellipse inside full sides, a diamond at exponent 1 and an ellipse at 2."""
    half = xyz(width, height) * 0.5
    normalized = abs(point / half)
    powers = normalized**exponent
    level = (powers["X"] + powers["Y"]) ** (1.0 / exponent)
    slope = vector("LENGTH", normalized ** (exponent - 1.0) / half * level ** (1.0 - exponent))
    narrow = math("MINIMUM", width, height)
    centered = equal(vector("LENGTH", point), 0.0)
    return Bounded(distance=where(centered, narrow * -0.5, (level - 1.0) / slope), width=narrow)


def cross(point: Expr, length: Expr, thickness: Expr, aspect: Expr, roundness: Expr) -> Bounded:
    """Plus sign of two rounded bars, horizontal arms `aspect` times the vertical."""
    across, up = box(point, length * aspect, length * thickness, roundness), box(point, length * thickness, length, roundness)
    return Bounded(distance=math("MINIMUM", across.distance, up.distance), width=math("MAXIMUM", across.width, up.width))


def polygonal(point: Expr, vertices: tuple[Expr, *tuple[Expr, ...]], rounding: Scalar) -> Expr:
    """Rounded boundary distance of an ordered polygon, inside by even-odd edge crossings."""
    edges = tuple(zip(vertices, (*vertices[1:], vertices[0])))

    def crossing(start: Expr, end: Expr) -> Expr:
        straddled = abs((start["Y"] > point["Y"]) - (end["Y"] > point["Y"]))
        return straddled * (point["X"] < math("MULTIPLY_ADD", (point["Y"] - start["Y"]) / (end["Y"] - start["Y"]), end["X"] - start["X"], start["X"]))

    nearest = reduce(lambda held, distance: math("MINIMUM", held, distance), (stroke(point, start, end, 0.0, 0.0).distance for start, end in edges))
    first, *rest = starmap(crossing, edges)
    return where(sum(rest, first) % 2.0, -nearest, nearest) - rounding


def triangle(point: Expr, a: Expr, b: Expr, c: Expr, roundness: Expr) -> Bounded:
    """Triangle of three vertices rounded toward its incenter by a share of the incircle radius."""
    corners = (a, b, c)
    lengths = tuple(vector("DISTANCE", corners[(index + 1) % 3], corners[(index + 2) % 3]) for index in range(3))
    area = vector("LENGTH", vector("CROSS_PRODUCT", b - a, c - a))
    perimeter = sum(lengths[1:], lengths[0])
    weighted = tuple(vector("SCALE", corner, length) for corner, length in zip(corners, lengths))
    center = sum(weighted[1:], weighted[0]) / perimeter
    share = switched(area > 0.0, 0.0, roundness)
    incircle = area / perimeter
    first, second, third = (mixed(share, corner, center) for corner in corners)
    return Bounded(distance=polygonal(point, (first, second, third), incircle * roundness), width=incircle * 2.0)


def polygon(point: Expr, radius: Expr, sides: int, inset: Expr, roundness: Expr) -> Bounded:
    """Regular polygon of `sides` outer vertices, side midpoints pulled toward the center into a star by `inset`."""
    rounding = radius * roundness
    outer = radius - rounding
    inner = outer * ((1.0 - inset) * cos(pi / sides))
    first, *rest = (vector("SCALE", (cos(pi / 2.0 + pi * index / sides), sin(pi / 2.0 + pi * index / sides), 0.0), inner if index % 2 else outer) for index in range(2 * sides))
    return Bounded(distance=polygonal(point, (first, *rest), rounding), width=(inner + rounding) * 2.0)


def heart(point: Expr, height: Expr) -> Bounded:
    """Heart of a tip-to-lobe height centered on the point's origin."""
    lobe = 2.0**0.5 / 4.0
    span = 0.75 + lobe
    scale = height / span
    located = point / scale + (0.0, span / 2.0, 0.0)
    folded = xyz(abs(located["X"]), located["Y"])
    total = folded["X"] + folded["Y"]
    tip = vector("LENGTH", folded - math("MAXIMUM", total, 0.0) * 0.5 * (1.0, 1.0, 0.0))
    lower = math("MINIMUM", vector("LENGTH", folded - (0.0, 1.0, 0.0)), tip) * math("SIGN", folded["X"] - folded["Y"])
    return Bounded(distance=where(total > 1.0, vector("LENGTH", folded - (0.25, 0.75, 0.0)) - lobe, lower) * scale, width=height)


def line(point: Expr, length: Expr, thickness: Expr, start: Expr, end: Expr) -> Bounded:
    """Round-capped horizontal line drawn between two shares of a centered length."""
    first, last = xyz((math("MINIMUM", start, end) - 0.5) * length), xyz((math("MAXIMUM", start, end) - 0.5) * length)
    return stroke(point, first, last, thickness * 0.5, thickness * 0.5)


def lattice(point: Expr, width: Expr, height: Expr, offset: Expr, thickness: Expr) -> tuple[Expr, Expr]:
    """Returns mortar distance and integer cell coordinate of a lattice, every second course shifted by `offset` cells."""
    course = math("FLOOR", point["Y"] / height) % 2.0
    parameter = xyz((point["X"] - course * offset * width) / width, point["Y"] / height)
    fraction = vector("FRACTION", parameter)
    distances = vector("MINIMUM", fraction, (1.0, 1.0, 0.0) - fraction) * xyz(width, height)
    return math("MULTIPLY_ADD", thickness, -0.5, math("MINIMUM", distances["X"], distances["Y"])), vector("FLOOR", parameter)


def grid(point: Expr, width: Expr, height: Expr, thickness: Expr, offset: Expr) -> Bounded:
    """Lattice lines of a line thickness through the point's origin."""
    return Bounded(distance=lattice(point, width, height, offset, thickness)[0], width=thickness)


# --- [COVERAGE]


def coverage(distance: Scalar, width: Option[Scalar], softness: Scalar, pixel: Scalar, growth: Scalar = 0.0) -> Expr:
    """Builds grown distance-ramp coverage, width-capped when bounded.

    Args:
        distance: Signed region distance in greater-side frame shares.
        width: Local narrowest width, absent for an unbounded region.
        softness: Authored ramp width in distance units.
        pixel: One displayed pixel in distance units.
        growth: Outward region growth in distance units.

    Returns:
        Approximate covered fraction.
    """
    span = math("ADD", softness, pixel)
    ramp = math("SUBTRACT", 0.5, math("SUBTRACT", distance, growth) / span, clamp=True)
    return width.map(lambda bound: math("MINIMUM", ramp, math("MULTIPLY_ADD", growth, 2.0, bound) / span)).default_value(ramp)


# --- [GRADIENTS]


def linear(local: Expr, length: Expr, sweep: Expr) -> Expr:
    """Parameter across X over `length`, swept back into chevrons along Y by `sweep`."""
    return (local["X"] - abs(local["Y"]) * sweep) / length


def radial(local: Expr, radius: Expr, aspect: Expr) -> Expr:
    """Parameter of elliptic rings reaching 1 at `radius`, horizontal radius `aspect` times the vertical."""
    return vector("LENGTH", local / xyz(aspect, 1.0)) / radius


def angular(local: Expr) -> Expr:
    """Parameter of one counterclockwise turn from +X."""
    return math("ARCTAN2", local["Y"], local["X"]) / tau


def diamond(local: Expr, radius: Expr) -> Expr:
    """Parameter of square rings standing on a corner, reaching 1 at `radius`."""
    folded = abs(local)
    return (folded["X"] + folded["Y"]) / radius


def rectangular(local: Expr, width: Expr, height: Expr) -> Expr:
    """Parameter of rectangular rings reaching 1 at half extents `width` and `height`."""
    folded = abs(local / xyz(width, height))
    return math("MAXIMUM", folded["X"], folded["Y"])


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Bounded",
    "angular",
    "box",
    "coverage",
    "cross",
    "diamond",
    "ellipse",
    "grid",
    "heart",
    "lattice",
    "line",
    "linear",
    "polygon",
    "polygonal",
    "radial",
    "rectangular",
    "rotated",
    "stroke",
    "triangle",
    "turned",
]
