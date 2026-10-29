"""Iris outlines and coverage kernels, radial lens distortion, chief-ray falloff, and thin-lens defocus, written once over any namespace."""

import itertools
import math
from typing import Annotated

from expression import Option, Some
import msgspec
import numpy as np
from numpy.polynomial import Polynomial
import numpy.typing as npt
from scipy.integrate import quad_vec

from blender.imaging.geometry.arithmetic import Elementwise, Namespace, smoothstep
from blender.imaging.geometry.panoramas import Bearing, ImagePoint, shares
from blender.records.fields import Finite, Positive, Unit

# --- [MODELS] ---------------------------------------------------------------------------


class Aperture(msgspec.Struct, frozen=True, kw_only=True):
    """Iris outline of bokeh and diffraction pupils."""

    blades: Annotated[int, msgspec.Meta(ge=3, le=24)] = 5
    rotation: Bearing = 0.0
    roundness: Unit = 1.0
    obstruction: Unit = 0.0

    @property
    def sector(self) -> float:
        """Angle one blade spans."""
        return math.tau / self.blades

    def across[V: Elementwise, B](self, angles: V, ns: Namespace[V, B]) -> V:
        """Returns the sine of each polar angle's offset from its blade's middle."""
        return ns.sin((angles + (self.rotation - math.pi / 2.0)) % self.sector - self.sector / 2.0)

    def radial[V: Elementwise, B](self, angles: V, ns: Namespace[V, B], scale: float = 1.0) -> V:
        """Returns positive iris boundary radius per polar angle, the outline at circumradius `scale`."""
        across = self.across(angles, ns)
        straight, curved = (1.0 - self.roundness) * scale, self.roundness * scale
        denominator = 1.0 - straight * straight * (across * across)
        return (ns.sqrt(curved * curved * (across * across) + denominator * math.cos(self.sector / 2.0) ** 2) * straight + curved) / denominator

    def band[V: Elementwise, B](self, angles: V, ns: Namespace[V, B]) -> tuple[V, V]:
        """Returns outer radius and annular thickness without subtracting close roots."""
        across = self.across(angles, ns)
        outer, inner = self.radial(angles, ns), self.radial(angles, ns, self.obstruction)
        half = math.cos(self.sector / 2.0)
        width = outer / (
            1.0
            - across
            * across
            * (outer + inner)
            * (self.obstruction * (1.0 - self.roundness))
            / (ns.sqrt(outer * outer * (across * across) + half * half) + ns.sqrt(inner * inner * (across * across) + half * half))
        )
        return outer, width * (1.0 - self.obstruction)

    def profile[V: Elementwise, B](self, x: V, y: V, softness: float, ns: Namespace[V, B]) -> V:
        """Returns the continuous inward cubic rim over iris and obstruction at offsets in circumradii, hard edged at zero softness."""
        outer, thickness = self.band(ns.atan2(y, x), ns)
        gap = outer - ns.hypot(x, y)
        if softness == 0.0:
            inside = ns.where(ns.less(gap, thickness), 1.0, 0.0) if self.obstruction > 0.0 else 1.0
            return ns.where(ns.greater(gap, 0.0), inside, 0.0)
        outside = smoothstep(ns.minimum(ns.maximum(gap / softness, 0.0), 1.0))
        return outside * smoothstep(ns.minimum(ns.maximum((thickness - gap) / softness, 0.0), 1.0)) if self.obstruction > 0.0 else outside

    def integral(self, softness: float, *, epsabs: float, epsrel: float) -> tuple[float, float]:
        """Estimates unit-profile area with exact piecewise-polynomial radial integrals."""

        def radial_area(angle: float) -> npt.NDArray[np.float64]:
            outside_radius, band = self.band(np.array((angle,)), np)
            outer, thickness = float(outside_radius[0]), float(band[0])
            if softness == 0.0:
                return np.array((thickness * (outer - thickness / 2.0),))
            if thickness == 0.0:
                return np.zeros(1, dtype=np.float64)
            cuts = sorted({0.0, 1.0, min(1.0, softness / thickness), max(0.0, 1.0 - softness / thickness)})

            def piece(first: float, last: float) -> float:
                span = last - first
                gap = thickness * Polynomial((first, span))
                outside = gap / softness if thickness * (first + last) / 2.0 < softness else Polynomial((1.0,))
                inside = (thickness - gap) / softness if self.obstruction > 0.0 and thickness * (1.0 - (first + last) / 2.0) < softness else Polynomial((1.0,))
                primitive = ((outer - gap) * smoothstep(outside) * smoothstep(inside)).integ()
                return thickness * span * float(primitive(1.0) - primitive(0.0))

            return np.array((sum(itertools.starmap(piece, itertools.pairwise(cuts))),))

        area, error = quad_vec(radial_area, 0.0, math.tau, epsabs=epsabs, epsrel=epsrel, points=math.pi / 2.0 - self.rotation + self.sector * np.arange(self.blades))
        return float(area[0]), error

    def coverage(self, xedges: npt.NDArray[np.float64], yedges: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Estimates pupil area fractions for Cartesian cells in circumradii."""
        sector = self.sector
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
            band: tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]] = self.band(angle, np)
            outer, thickness = band
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
            return np.asarray(span * width * np.clip(far - near, 0.0, None) * (outer - thickness * (far + near) / 2.0), dtype=np.float64)

        integrated, _ = quad_vec(area, 0.0, 1.0)
        result[rows, columns] = np.bincount(owners, weights=integrated, minlength=len(rows)) * (1.0 - self.obstruction) / ((xmax - xmin) * (ymax - ymin))
        return result

    def kernel(self, radius: float, /) -> Option[npt.NDArray[np.float64]]:
        """Returns the coverage kernel at circumradius `radius` pixels, odd sided with unit sum, a single tap at zero radius, absent for a closed iris."""
        edges = (np.arange(2 * math.ceil(radius) + 2, dtype=np.float64) - math.ceil(radius) - 0.5) / (radius or 1.0)
        return (
            Some(self)
            .filter(lambda iris: iris.obstruction < 1.0)
            .map(lambda iris: iris.coverage(edges, -edges) if radius > 0.0 else np.ones((1, 1), dtype=np.float64))
            .map(lambda covered: covered / covered.sum())
        )


class Radial(msgspec.Struct, frozen=True, kw_only=True):
    """Circular radial distortion and zoom normalized by the displayed greater side."""

    distortion: Finite
    scale: Positive
    aspect: Positive

    def to_rectilinear[V: Elementwise, B](self, point: ImagePoint[V], ns: Namespace[V, B]) -> ImagePoint[V]:
        """Maps lens positions to rectilinear samples, covering the real branch alone."""
        s, t, coverage = point
        across, up, (width, height) = s - 0.5, t - 0.5, shares(self.aspect)
        strength = 4.0 * self.scale * math.sqrt(abs(self.distortion))
        radius = ns.hypot(across * (width * strength), up * (height * strength))
        if self.distortion < 0.0:
            shrink = 2.0 * self.scale / (ns.sqrt(radius * radius + 1.0) + 1.0)
            return across * shrink + 0.5, up * shrink + 0.5, coverage
        covered = ns.less_equal(radius, 1.0)
        shrink = ns.where(covered, 2.0 * self.scale / (ns.sqrt(ns.maximum((1.0 - radius) * (radius + 1.0), 0.0)) + 1.0), 0.0)
        return across * shrink + 0.5, up * shrink + 0.5, ns.where(covered, coverage, 0.0)

    def to_lens[V: Elementwise, B](self, point: ImagePoint[V], ns: Namespace[V, B]) -> ImagePoint[V]:
        """Maps rectilinear positions to lens samples on the forward map's inverse branch."""
        s, t, coverage = point
        across, up, (width, height) = s - 0.5, t - 0.5, shares(self.aspect)
        strength = 2.0 * math.sqrt(abs(self.distortion))
        radius = ns.hypot(across * (width * strength), up * (height * strength))
        covered = ns.less(radius, 1.0) if self.distortion < 0.0 else ns.less_equal(radius, 1.0)
        grow = ns.where(covered, 1.0 / ((radius * radius * math.copysign(1.0, self.distortion) + 1.0) * self.scale), 0.0)
        return across * grow + 0.5, up * grow + 0.5, ns.where(covered, coverage, 0.0)


class ThinLens(msgspec.Struct, frozen=True, kw_only=True):
    """Thin lens focal length, f-number, focus distance, and sensor width, lengths in meters."""

    focal_length: Positive
    f_number: Positive
    focus: Positive
    sensor: Positive

    def confusion[V: Elementwise, B](self, depth: V, ns: Namespace[V, B]) -> V:
        """Returns circle of confusion radius over sensor width at scene depths, unbounded when focused at the focal length."""
        gap = abs(self.focus - self.focal_length)
        return ns.abs(self.focus / depth - 1.0) * (self.focal_length**2 / (2.0 * self.f_number * self.sensor * gap) if gap else math.inf)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Aperture", "Radial", "ThinLens"]
