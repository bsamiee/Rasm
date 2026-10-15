"""Lens blur from depth, an elliptical sharp region, or a tilted band."""

import itertools
import math
import sys
from typing import Annotated

from expression import Some
import msgspec
import numpy as np
from numpy.polynomial import Polynomial
import numpy.typing as npt
from scipy.integrate import quad_vec

from blender.imaging.chain import Effect, Stage
from blender.imaging.parameters import Case, Position, Positive, Presentation, Quantity

# --- [MODELS] ---------------------------------------------------------------------------


class Aperture(msgspec.Struct, frozen=True, kw_only=True):
    """Iris outline shared by bokeh and diffraction pupils."""

    blades: Annotated[int, msgspec.Meta(ge=3, le=24, description="Iris blades outlining the bokeh")] = 5
    rotation: Annotated[float, msgspec.Meta(ge=-math.pi, le=math.pi, description="Clockwise turn of a blade vertex from vertical"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    roundness: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Blade curvature, 1 drawing a circular iris")] = 1.0
    obstruction: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Catadioptric", description="Central mirror as a fraction of the iris outline")] = 0.0

    def radial(self, angles: npt.NDArray[np.float64], scale: float = 1.0) -> npt.NDArray[np.float64]:
        """Returns the positive native iris boundary root at each polar angle."""
        sector = math.tau / self.blades
        across = np.sin((angles + self.rotation - math.pi / 2.0) % sector - sector / 2.0)
        straight = (1.0 - self.roundness) * scale
        curved = self.roundness * scale
        denominator = 1.0 - np.square(straight * across)
        return (curved + straight * np.sqrt(np.square(curved * across) + denominator * math.cos(sector / 2.0) ** 2)) / denominator

    def profile(self, points: npt.NDArray[np.float64], softness: float) -> npt.NDArray[np.float64]:
        """Returns a continuous inward cubic rim over the native iris and obstruction."""
        radius = np.linalg.norm(points, axis=-1)
        angles = np.arctan2(points[..., 1], points[..., 0])
        outer, thickness = self.band(angles)
        gap = outer - radius
        if softness == 0.0:
            return np.asarray((gap > 0.0) & ((gap < thickness) | (self.obstruction == 0.0)), dtype=np.float64)
        outside = np.clip(gap / softness, 0.0, 1.0)
        inside = np.clip((thickness - gap) / softness, 0.0, 1.0) if self.obstruction > 0.0 else np.ones_like(radius)
        return outside * outside * (3.0 - 2.0 * outside) * inside * inside * (3.0 - 2.0 * inside)

    def band(self, angles: npt.NDArray[np.float64]) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
        """Returns outer radius and annular thickness without subtracting close roots."""
        sector = math.tau / self.blades
        across = np.sin((angles + self.rotation - math.pi / 2.0) % sector - sector / 2.0)
        outer, inner = self.radial(angles), self.radial(angles, self.obstruction)
        width = outer / (
            1.0 - self.obstruction * (1.0 - self.roundness) * across**2 * (outer + inner) / (np.hypot(math.cos(sector / 2.0), outer * across) + np.hypot(math.cos(sector / 2.0), inner * across))
        )
        return outer, (1.0 - self.obstruction) * width

    def integral(self, softness: float, *, epsabs: float, epsrel: float) -> tuple[float, float]:
        """Estimates unit-profile area with exact piecewise-polynomial radial integrals."""

        def radial_area(angle: float) -> npt.NDArray[np.float64]:
            angles = np.array((angle,))
            outside_radius, band = self.band(angles)
            outer, thickness = float(outside_radius[0]), float(band[0])
            if softness == 0.0:
                return np.array((thickness * (outer - thickness / 2.0),))
            if thickness == 0.0:
                return np.zeros(1, dtype=np.float64)
            cuts = sorted({0.0, 1.0, min(1.0, softness / thickness), max(0.0, 1.0 - softness / thickness)})
            area = 0.0
            for first, last in itertools.pairwise(cuts):
                span = last - first
                gap = thickness * Polynomial((first, span))
                radius = outer - gap
                outside = gap / softness if thickness * (first + last) / 2.0 < softness else Polynomial((1.0,))
                inside = (thickness - gap) / softness if self.obstruction > 0.0 and thickness * (1.0 - (first + last) / 2.0) < softness else Polynomial((1.0,))
                integrand = radius * outside**2 * (3.0 - 2.0 * outside) * inside**2 * (3.0 - 2.0 * inside)
                primitive = integrand.integ()
                area += thickness * span * float(primitive(1.0) - primitive(0.0))
            return np.array((area,))

        area, error = quad_vec(radial_area, 0.0, math.tau, epsabs=epsabs, epsrel=epsrel, points=math.pi / 2.0 - self.rotation + math.tau / self.blades * np.arange(self.blades))
        return float(area[0]), error

    def coverage(self, xedges: npt.NDArray[np.float64], yedges: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Estimates pupil area fractions for Cartesian cells in circumradii."""
        sector = math.tau / self.blades
        x = np.stack((xedges[:-1], xedges[1:]))
        y = np.stack((yedges[:-1], yedges[1:]))
        if self.obstruction == 1.0:
            return np.zeros((y.shape[1], x.shape[1]), dtype=np.float64)
        half_edge = math.cos(sector / 2.0)
        polygon = 1.0 - self.roundness
        xc = x.mean(axis=0)[None, :]
        yc = y.mean(axis=0)[:, None]
        radius = np.hypot(xc, yc)
        local = np.remainder(np.arctan2(yc, xc) + self.rotation - math.pi / 2.0, sector) - sector / 2.0
        bound = polygon * np.hypot(half_edge, radius * np.sin(local)) + self.roundness
        outer_distance = radius - bound
        inner_distance = radius - self.obstruction * bound
        half_diagonal = np.hypot(np.diff(xedges)[None, :], np.diff(yedges)[:, None]) / 2.0
        rows, columns = np.nonzero((np.abs(outer_distance) <= (1.0 + polygon) * half_diagonal) | (np.abs(inner_distance) <= (1.0 + self.obstruction * polygon) * half_diagonal))
        result = np.asarray((outer_distance < 0.0) & (inner_distance > 0.0), dtype=np.float64)
        if not len(rows):
            return result
        xmin = x.min(axis=0)[columns]
        xmax = x.max(axis=0)[columns]
        ymin = y.min(axis=0)[rows]
        ymax = y.max(axis=0)[rows]
        center = np.arctan2(yc[rows, 0], xc[0, columns])
        corners = np.arctan2(np.stack((ymin, ymin, ymax, ymax), axis=1), np.stack((xmin, xmax, xmin, xmax), axis=1))
        offsets = (corners - center[:, None] + math.pi) % math.tau - math.pi
        enclosing_origin = (xmin <= 0.0) & (xmax >= 0.0) & (ymin <= 0.0) & (ymax >= 0.0)
        start = np.where(enclosing_origin, -math.pi, offsets.min(axis=1))
        stop = np.where(enclosing_origin, math.pi, offsets.max(axis=1))
        divisions = np.concatenate((math.pi / 2.0 - self.rotation + sector * np.arange(self.blades), math.pi / 2.0 * np.arange(4)))
        divisions = (divisions[None, :] - center[:, None] + math.pi) % math.tau - math.pi
        cuts: npt.NDArray[np.float64] = np.sort(np.clip(np.concatenate((offsets, divisions, start[:, None], stop[:, None]), axis=1), start[:, None], stop[:, None]), axis=1)
        owners, intervals = np.nonzero(np.diff(cuts, axis=1) > 0.0)
        first = center[owners] + cuts[owners, intervals]
        span = cuts[owners, intervals + 1] - cuts[owners, intervals]
        limits = np.stack((np.stack((xmin[owners], ymin[owners])), np.stack((xmax[owners], ymax[owners]))))
        parallel_inside = (limits[0] <= 0.0) & (limits[1] >= 0.0)

        def area(position: float) -> npt.NDArray[np.float64]:
            angle = first + span * position
            outer, thickness = self.band(angle)
            width = thickness / (1.0 - self.obstruction)
            direction = np.stack((np.cos(angle), np.sin(angle)))
            transverse = direction != 0.0
            entering = np.full(direction.shape, np.inf, dtype=np.float64)
            entering[parallel_inside] = -np.inf
            leaving = -entering
            np.divide(np.where(direction >= 0.0, limits[0], limits[1]), direction, out=entering, where=transverse)
            np.divide(np.where(direction >= 0.0, limits[1], limits[0]), direction, out=leaving, where=transverse)
            lower = entering.max(axis=0)
            upper = leaving.min(axis=0)
            near = np.clip((outer - upper) / thickness, 0.0, 1.0)
            far = np.clip((outer - lower) / thickness, 0.0, 1.0)
            return span * width * np.clip(far - near, 0.0, None) * (outer - thickness * (far + near) / 2.0)

        integrated, _ = quad_vec(area, 0.0, 1.0)
        result[rows, columns] = np.bincount(owners, weights=integrated, minlength=len(rows)) * (1.0 - self.obstruction) / ((xmax - xmin) * (ymax - ymin))
        return result


class DepthOfField(Case, frozen=True, kw_only=True, tag=1):
    """Circle of confusion from a camera's thin lens and depth pass."""

    title = "Depth of Field"
    f_stop: Annotated[
        float, msgspec.Meta(gt=0.0, le=sys.float_info.max, title="F-Stop", description="Focal length over aperture diameter"), Presentation(soft=Some((0.1, 128.0)), decimals=Some(1))
    ] = 2.8


class FieldCurvature(Case, frozen=True, kw_only=True, tag=2):
    """Defocus growing quadratically beyond a sharp ellipse."""

    title = "Radial"
    center: Annotated[Position, msgspec.Meta(description="Center of the sharp ellipse")] = (0.5, 0.5)
    angle: Annotated[float, msgspec.Meta(ge=-math.pi / 2.0, le=math.pi / 2.0, description="Turn of the ellipse from horizontal"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    size: Annotated[tuple[Positive, Positive], msgspec.Meta(description="Positive ellipse half extents along its rotated axes"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = (0.4, 0.4)
    feather: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, description="Outward distance where defocus reaches full size"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = 0.2


class TiltShift(Case, frozen=True, kw_only=True, tag=3):
    """Defocus growing linearly beyond a tilted sharp band."""

    title = "Tilt-Shift"
    center: Annotated[Position, msgspec.Meta(description="Point the sharp band passes through")] = (0.5, 0.5)
    angle: Annotated[float, msgspec.Meta(ge=-math.pi / 2.0, le=math.pi / 2.0, description="Turn of the sharp band from horizontal"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    width: Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Width of the sharp band"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = 0.16875
    feather: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, description="Outward distance where defocus reaches full size"), Presentation(quantity=Some(Quantity.FRAME_GREATER))] = (
        0.140625
    )


class Defocus(Effect, frozen=True, kw_only=True, tag=24):
    """Variable-radius lens blur over the shared iris."""

    stage = Stage.DEFOCUS
    title = "Defocus"
    pointwise = False
    focus: Annotated[DepthOfField | FieldCurvature | TiltShift, msgspec.Meta(description="Depth, sharp ellipse, or tilted band sizing the bokeh")] = DepthOfField()
    aperture: Annotated[Aperture, msgspec.Meta(description="Iris outline of the bokeh")] = Aperture()
    radius: Annotated[
        float, msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Max Blur", description="Largest bokeh radius"), Presentation(soft=Some((0.0, 0.05)), quantity=Some(Quantity.FRAME_GREATER))
    ] = 0.02
    fringe: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, description="Bokeh rim color shift, toward blue above 0")] = 0.0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Aperture", "Defocus", "DepthOfField", "FieldCurvature", "TiltShift"]
