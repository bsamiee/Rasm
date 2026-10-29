"""Image projections, mirror folds, polar maps, perspective cameras, and oriented views into panoramas, written once over any namespace."""

import math
import sys
from typing import Annotated, ClassVar, Self

from expression import Nothing, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
from scipy.spatial.transform import RigidTransform, Rotation

from blender.imaging.geometry.arithmetic import Elementwise, Namespace
from blender.imaging.geometry.frames import Quad
from blender.records.fields import Case, Finite, NonNegative, Positive, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type Bearing = Annotated[float, msgspec.Meta(ge=-math.pi, le=math.pi)]
type _FieldOfView = Annotated[float, msgspec.Meta(ge=sys.float_info.min, lt=math.pi)]
type _Ray[V] = tuple[V, V, V, V]
type ImagePoint[V] = tuple[V, V, V]

# --- [MODELS] ---------------------------------------------------------------------------


class Panorama(Case, frozen=True, kw_only=True):
    """Projection between camera-space rays (x, y, z, coverage) and image points (s, t, coverage)."""

    extension: ClassVar[tuple[OpenImageIO.Wrap, OpenImageIO.Wrap]] = (OpenImageIO.Wrap.Black, OpenImageIO.Wrap.Black)
    upright: ClassVar[bool] = False
    centered: ClassVar[bool] = False


class Fisheye(Panorama, frozen=True, kw_only=True):
    """Projection onto centered image circle."""

    field_of_view: Positive = math.pi


class Rectilinear(Panorama, frozen=True, kw_only=True):
    """Pinhole projection of a perspective camera."""

    aspect: Positive = 4.0 / 3.0
    field_of_view: _FieldOfView = math.radians(60.0)
    shift: tuple[Finite, Finite] = (0.0, 0.0)

    @property
    def sides(self) -> tuple[float, float]:
        """Frame width and height at unit depth."""
        width = 2.0 * math.tan(self.field_of_view / 2.0)
        return width, width / self.aspect

    def to_image[V: Elementwise, B](self, ray: _Ray[V], ns: Namespace[V, B]) -> ImagePoint[V]:
        """Projects rays to frame, zero coverage at and behind camera."""
        x, y, z, coverage = ray
        (width, height), depth = self.sides, ns.negative(z)
        ahead = ns.greater(depth, 0.0)
        return _over(ns, x, depth * width, ahead) + (0.5 - self.shift[0]), _over(ns, y, depth * height, ahead) + (0.5 - self.shift[1]), ns.where(ahead, coverage, 0.0)

    def to_ray[V: Elementwise, B](self, point: ImagePoint[V], ns: Namespace[V, B]) -> _Ray[V]:
        """Lifts frame positions to unit rays."""
        s, t, coverage = point
        width, height = self.sides
        return (*_normalized(ns, (s + (self.shift[0] - 0.5)) * width, (t + (self.shift[1] - 0.5)) * height, -1.0), coverage)


class Equirectangular(Panorama, frozen=True, kw_only=True):
    """Latitude and longitude over full sphere."""

    extension = (OpenImageIO.Wrap.Periodic, OpenImageIO.Wrap.Clamp)
    upright = True

    @staticmethod
    def to_image[V: Elementwise, B](ray: _Ray[V], ns: Namespace[V, B]) -> ImagePoint[V]:
        """Maps rays to longitude and latitude."""
        x, y, z, coverage = ray
        return ns.atan2(x, ns.negative(z)) / (2.0 * math.pi) + 0.5, ns.atan2(y, ns.hypot(x, z)) / math.pi + 0.5, coverage

    @staticmethod
    def to_ray[V: Elementwise, B](point: ImagePoint[V], ns: Namespace[V, B]) -> _Ray[V]:
        """Maps longitude and latitude to unit rays."""
        s, t, coverage = point
        longitude, latitude = (s - 0.5) * (2.0 * math.pi), (t - 0.5) * math.pi
        return ns.cos(latitude) * ns.sin(longitude), ns.sin(latitude), ns.negative(ns.cos(latitude) * ns.cos(longitude)), coverage


class CentralCylindrical(Panorama, frozen=True, kw_only=True):
    """Projection from cylinder center about vertical axis."""

    extension = (OpenImageIO.Wrap.Periodic, OpenImageIO.Wrap.Black)
    upright = True
    height_min: Finite = -1.0
    height_max: Finite = 1.0

    def to_image[V: Elementwise, B](self, ray: _Ray[V], ns: Namespace[V, B]) -> ImagePoint[V]:
        """Projects rays, zero coverage on axis or for zero-height cylinder."""
        x, y, z, coverage = ray
        radial, span = ns.hypot(x, z), self.height_max - self.height_min
        around = ns.greater(radial, 0.0)
        vertical = (_over(ns, y, radial, around) - self.height_min) / (span or 1.0)
        return ns.atan2(x, ns.negative(z)) / (2.0 * math.pi) + 0.5, vertical, ns.where(around, coverage, 0.0) * float(span != 0.0)

    def to_ray[V: Elementwise, B](self, point: ImagePoint[V], ns: Namespace[V, B]) -> _Ray[V]:
        """Lifts cylinder positions to rays, reversed or equal heights included."""
        s, t, coverage = point
        longitude = (s - 0.5) * (2.0 * math.pi)
        return (*_normalized(ns, ns.sin(longitude), t * (self.height_max - self.height_min) + self.height_min, ns.negative(ns.cos(longitude))), coverage)


class MirrorBall(Panorama, frozen=True, kw_only=True):
    """Mirrored sphere seen along view axis."""

    @staticmethod
    def to_image[V: Elementwise, B](ray: _Ray[V], ns: Namespace[V, B]) -> ImagePoint[V]:
        """Maps rays to ball circle, zero coverage straight behind."""
        x, y, z, coverage = ray
        length = ns.sqrt(x * x + y * y + (z - 1.0) * (z - 1.0))
        defined = ns.greater(length, 0.0)
        return _over(ns, x, length, defined) * 0.5 + 0.5, _over(ns, y, length, defined) * 0.5 + 0.5, ns.where(defined, coverage, 0.0)

    @staticmethod
    def to_ray[V: Elementwise, B](point: ImagePoint[V], ns: Namespace[V, B]) -> _Ray[V]:
        """Reflects positions into unit rays, zero coverage outside ball."""
        s, t, coverage = point
        across, up = s * 2.0 - 1.0, t * 2.0 - 1.0
        rest = 1.0 - across * across - up * up
        normal = ns.negative(ns.sqrt(ns.maximum(rest, 0.0)))
        return normal * across * -2.0, normal * up * -2.0, normal * normal * -2.0 + 1.0, ns.where(ns.greater_equal(rest, 0.0), coverage, 0.0)


class FisheyeEquidistant(Fisheye, frozen=True, kw_only=True):
    """Fisheye with radius proportional to view axis angle."""

    def to_image[V: Elementwise, B](self, ray: _Ray[V], ns: Namespace[V, B]) -> ImagePoint[V]:
        """Maps rays to image circle, zero coverage past half field of view."""
        x, y, z, coverage = ray
        radial = ns.hypot(x, y)
        angle, off = ns.atan2(radial, ns.negative(z)), ns.greater(radial, 0.0)
        reach = angle / self.field_of_view
        held = ns.where(ns.logical_and(ns.less_equal(angle, self.field_of_view / 2.0), _forward(ns, off, z)), coverage, 0.0)
        return _over(ns, x, radial, off) * reach + 0.5, _over(ns, y, radial, off) * reach + 0.5, held

    def to_ray[V: Elementwise, B](self, point: ImagePoint[V], ns: Namespace[V, B]) -> _Ray[V]:
        """Lifts positions to unit rays, zero coverage outside image circle."""
        s, t, coverage = point
        across, up = s * 2.0 - 1.0, t * 2.0 - 1.0
        radial = ns.hypot(across, up)
        angle, off = radial * (self.field_of_view / 2.0), ns.greater(radial, 0.0)
        return ns.sin(angle) * _over(ns, across, radial, off), ns.sin(angle) * _over(ns, up, radial, off), ns.negative(ns.cos(angle)), ns.where(ns.less_equal(radial, 1.0), coverage, 0.0)


class FisheyeEquisolid(Fisheye, frozen=True, kw_only=True):
    """Fisheye with equisolid image radius on physical sensor."""

    lens: Positive = 0.0105
    sensor: tuple[Positive, Positive] = (0.036, 0.024)

    @property
    def radius(self) -> float:
        """Image circle radius on sensor at field of view."""
        return 2.0 * self.lens * math.sin(self.field_of_view / 4.0)

    def to_image[V: Elementwise, B](self, ray: _Ray[V], ns: Namespace[V, B]) -> ImagePoint[V]:
        """Maps rays to sensor, zero coverage outside image circle."""
        x, y, z, coverage = ray
        radial = ns.hypot(x, y)
        off = ns.greater(radial, 0.0)
        reach = ns.sin(ns.atan2(radial, ns.negative(z)) / 2.0) * (2.0 * self.lens)
        held = ns.where(ns.logical_and(ns.less_equal(reach, self.radius), _forward(ns, off, z)), coverage, 0.0)
        return reach * _over(ns, x, radial, off) / self.sensor[0] + 0.5, reach * _over(ns, y, radial, off) / self.sensor[1] + 0.5, held

    def to_ray[V: Elementwise, B](self, point: ImagePoint[V], ns: Namespace[V, B]) -> _Ray[V]:
        """Lifts sensor positions to rays, zero coverage outside image circle."""
        s, t, coverage = point
        across, up = (s - 0.5) * self.sensor[0], (t - 0.5) * self.sensor[1]
        radial = ns.hypot(across, up)
        outside = ns.greater(radial, self.radius)
        angle = ns.where(outside, 0.0, ns.asin(ns.minimum(radial / (2.0 * self.lens), 1.0)) * 2.0)
        off = ns.greater(radial, 0.0)
        return ns.sin(angle) * _over(ns, across, radial, off), ns.sin(angle) * _over(ns, up, radial, off), ns.negative(ns.cos(angle)), ns.where(outside, 0.0, coverage)


class Stereographic(Panorama, frozen=True, kw_only=True):
    """Conformal projection of sphere from point behind viewer."""

    centered = True
    field_of_view: Annotated[float, msgspec.Meta(ge=sys.float_info.min, lt=2.0 * math.pi)] = math.pi

    @property
    def scale(self) -> float:
        """Twice greater-side extent on projection plane."""
        return 4.0 * math.tan(self.field_of_view / 4.0)

    def to_image[V: Elementwise, B](self, ray: _Ray[V], ns: Namespace[V, B]) -> ImagePoint[V]:
        """Projects rays to centered positions, zero coverage straight behind."""
        x, y, z, coverage = ray
        pole = 1.0 - z
        ahead = ns.greater(pole, 0.0)
        return _over(ns, x * 2.0, pole * self.scale, ahead), _over(ns, y * 2.0, pole * self.scale, ahead), ns.where(ahead, coverage, 0.0)

    def to_ray[V: Elementwise, B](self, point: ImagePoint[V], _ns: Namespace[V, B]) -> _Ray[V]:
        """Lifts centered positions to covered unit rays."""
        s, t, coverage = point
        across, up = s * (self.scale / 2.0), t * (self.scale / 2.0)
        denominator = across * across + up * up + 1.0
        return across * 2.0 / denominator, up * 2.0 / denominator, 1.0 - 2.0 / denominator, coverage


class Panini(Panorama, frozen=True, kw_only=True):
    """Cylinder projection from behind axis, keeping verticals straight."""

    upright = True
    centered = True
    field_of_view: _FieldOfView = math.radians(60.0)
    distance: NonNegative = 1.0
    compression: Unit = 0.0

    @property
    def weights(self) -> tuple[float, float]:
        """Perspective and cylindrical weights at projection distance."""
        total = self.distance + 1.0
        return 1.0 / total, self.distance / total

    @property
    def span(self) -> float:
        """Greater-side extent on projection plane."""
        perspective, cylindrical = self.weights
        return 2.0 * math.sin(self.field_of_view / 2.0) / (cylindrical + perspective * math.cos(self.field_of_view / 2.0))

    def to_image[V: Elementwise, B](self, ray: _Ray[V], ns: Namespace[V, B]) -> ImagePoint[V]:
        """Projects rays, zero coverage at and behind camera."""
        x, y, z, coverage = ray
        depth, horizontal = ns.negative(z), ns.hypot(x, z)
        ahead = ns.greater(depth, 0.0)
        perspective, cylindrical = self.weights
        ratio = _over(ns, depth, horizontal, ahead)
        projection = ratio * perspective + cylindrical
        vertical = ratio * (1.0 - self.compression) + self.compression
        return _over(ns, x, horizontal * projection * self.span, ahead), _over(ns, y * vertical, depth * projection * self.span, ahead), ns.where(ahead, coverage, 0.0)

    def to_ray[V: Elementwise, B](self, point: ImagePoint[V], ns: Namespace[V, B]) -> _Ray[V]:
        """Lifts centered positions to rays, zero coverage without forward ray."""
        s, t, coverage = point
        across, up = s * self.span, t * self.span
        perspective, cylindrical = self.weights
        length = ns.sqrt(across * across * (perspective * perspective) + 1.0)
        tangent, distant = across * perspective / length, across * cylindrical / length
        rest = (1.0 - distant) * (distant + 1.0)
        root = ns.sqrt(ns.maximum(rest, 0.0))
        cosine = root / length - tangent * distant
        sine = across / length * (root * perspective + cylindrical / length)
        ahead = ns.logical_and(ns.greater(cosine, 0.0), ns.greater_equal(rest, 0.0))
        vertical = _over(ns, cosine, cosine * (1.0 - self.compression) + self.compression, ahead)
        x, y, z = _normalized(ns, sine, up * (cosine * perspective + cylindrical) * vertical, ns.negative(cosine))
        return ns.where(ahead, x, 0.0), ns.where(ahead, y, 0.0), ns.where(ahead, z, 0.0), ns.where(ahead, coverage, 0.0)


type Panoramas = Rectilinear | Equirectangular | CentralCylindrical | MirrorBall | FisheyeEquidistant | FisheyeEquisolid | Stereographic | Panini


class Camera(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Perspective lens with rigid camera-to-world placement."""

    placement: "RigidTransform[tuple[()]]"
    lens: Rectilinear

    def to_screen(self, points: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Returns s, t, camera-space depth, and coverage, zero coverage at and behind camera."""
        local = self.placement.apply(points, inverse=True)
        s, t, coverage = self.lens.to_image((local[..., 0], local[..., 1], local[..., 2], np.ones_like(local[..., 0])), np)
        return np.stack((s, t, -local[..., 2], coverage), axis=-1)

    def to_world(self, screen: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Lifts s, t, and camera-space depth to world points in meters."""
        (width, height), depth = self.lens.sides, screen[..., 2]
        across, up = (screen[..., 0] + (self.lens.shift[0] - 0.5)) * width * depth, (screen[..., 1] + (self.lens.shift[1] - 0.5)) * height * depth
        return self.placement.apply(np.stack((across, up, -depth), axis=-1))


class PanoramaView(msgspec.Struct, frozen=True, kw_only=True):
    """Perspective view into panorama."""

    source: Panoramas = Equirectangular()
    view: Rectilinear = Rectilinear()
    heading: Bearing = 0.0
    elevation: Annotated[float, msgspec.Meta(ge=-math.pi / 2.0, le=math.pi / 2.0)] = 0.0
    roll: Bearing = 0.0

    @property
    def rotation(self) -> Rotation[tuple[()]]:
        """Rotation taking view rays to panorama rays."""
        return Rotation.from_euler("YXZ", (-self.heading, self.elevation, self.roll))

    @property
    def frame(self) -> tuple[float, npt.NDArray[np.float64]]:
        """Origin and scale from view frame to source coordinates."""
        return (0.5, np.array(shares(self.view.aspect))) if self.source.centered else (0.0, np.ones(2))

    def to_panorama(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps view positions to source, keeping coverage."""
        origin, scale = self.frame
        *ray, coverage = self.view.to_ray((samples[..., 0], samples[..., 1], samples[..., 2]), np)
        turned = self.rotation.apply(np.stack(ray, axis=-1))
        s, t, held = self.source.to_image((turned[..., 0], turned[..., 1], turned[..., 2], coverage), np)
        return np.stack((s / scale[0] + origin, t / scale[1] + origin, held), axis=-1)

    def to_view(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps source positions into view, zero coverage behind view."""
        *ray, coverage = self.source.to_ray(self._source(samples), np)
        local = self.rotation.apply(np.stack(ray, axis=-1), inverse=True)
        return np.stack(self.view.to_image((local[..., 0], local[..., 1], local[..., 2], coverage), np), axis=-1)

    def framing(self, quad: Quad, share: float) -> Option[Self]:
        """Aims view at quad filling `share` of frame, `Nothing` for undefined, rearward, or zero-span corner rays."""
        *ray, coverage = self.source.to_ray(self._source(quad.samples), np)
        rays = np.stack(ray, axis=-1)
        sides = rays[[0, 1]] + rays[[3, 2]]
        forward = sides.sum(axis=0)
        level = msgspec.structs.replace(self, heading=math.atan2(forward[0], -forward[2]), elevation=math.atan2(forward[1], math.hypot(forward[0], forward[2])), roll=0.0)
        leveled_sides = level.rotation.apply(sides, inverse=True)
        across = np.diff(leveled_sides, axis=0)[0]
        aimed = msgspec.structs.replace(level, roll=math.atan2(across[1], across[0]))
        local = aimed.rotation.apply(rays, inverse=True)
        planar = np.divide(local[:, :2], -local[:, 2:], out=np.zeros_like(local[:, :2]), where=local[:, 2:] < 0.0)
        spread = max(float(np.ptp(planar[:, 0])), float(np.ptp(planar[:, 1])) * self.view.aspect)
        return (
            Some(msgspec.structs.replace(aimed, view=msgspec.structs.replace(self.view, field_of_view=2.0 * math.atan(spread / (2.0 * share)))))
            if bool(spread > 0.0 and np.all(coverage > 0.0) and np.all(local[:, 2] < 0.0) and np.all(leveled_sides[:, 2] < 0.0))
            else Nothing
        )

    def _source(self, samples: npt.NDArray[np.float64]) -> ImagePoint[npt.NDArray[np.float64]]:
        origin, scale = self.frame
        return (samples[..., 0] - origin) * scale[0], (samples[..., 1] - origin) * scale[1], samples[..., 2]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [FRAMES]


def shares(aspect: float) -> tuple[float, float]:
    """Returns frame width and height of `aspect` as shares of the greater side."""
    greater = max(aspect, 1.0)
    return aspect / greater, 1.0 / greater


# --- [FOLDS]


def folded[V: Elementwise, B](x: V, y: V, *, mirrors: int, angle: float, zoom: float, ns: Namespace[V, B]) -> tuple[V, V]:
    """Reflects centered offsets into the first of `mirrors` wedges from `angle`, magnified by `zoom`."""
    wedge = math.pi / mirrors
    bearing = ns.abs(ns.remainder(ns.atan2(y, x) + (wedge - angle), 2.0 * wedge) - wedge) + angle
    radius = ns.hypot(x, y) / zoom
    return radius * ns.cos(bearing), radius * ns.sin(bearing)


def to_polar[V: Elementwise, B](point: ImagePoint[V], span: tuple[float, float], ns: Namespace[V, B]) -> ImagePoint[V]:
    """Wraps a full rectangular turn into the frame's inscribed disc, seam above center, for frame sides `span` over the greater side."""
    s, t, coverage = point
    across, up, short = (s - 0.5) * span[0], (t - 0.5) * span[1], min(span)
    return ns.remainder(ns.atan2(across, up), 2.0 * math.pi) / (2.0 * math.pi), 1.0 - ns.hypot(across, up) * (2.0 / short), coverage


def from_polar[V: Elementwise, B](point: ImagePoint[V], span: tuple[float, float], ns: Namespace[V, B]) -> ImagePoint[V]:
    """Unwraps the frame's inscribed disc into a full rectangular turn, inverse of `to_polar`."""
    s, t, coverage = point
    bearing, radius = s * (2.0 * math.pi), (1.0 - t) * (min(span) / 2.0)
    return radius * ns.sin(bearing) / span[0] + 0.5, radius * ns.cos(bearing) / span[1] + 0.5, coverage


# --- [ARITHMETIC]


def _over[V: Elementwise, B](ns: Namespace[V, B], numerator: V, denominator: V, defined: B) -> V:
    return ns.where(defined, numerator / ns.where(defined, denominator, 1.0), 0.0)


def _normalized[V: Elementwise, B](ns: Namespace[V, B], x: V, y: V, z: V | float) -> tuple[V, V, V]:
    length = ns.sqrt(x * x + y * y + z * z)
    return x / length, y / length, z / length


def _forward[V: Elementwise, B](ns: Namespace[V, B], off: B, z: V) -> B:
    return ns.logical_or(off, ns.less(z, 0.0))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Bearing",
    "Camera",
    "CentralCylindrical",
    "Equirectangular",
    "Fisheye",
    "FisheyeEquidistant",
    "FisheyeEquisolid",
    "ImagePoint",
    "MirrorBall",
    "Panini",
    "Panorama",
    "PanoramaView",
    "Panoramas",
    "Rectilinear",
    "Stereographic",
    "folded",
    "from_polar",
    "shares",
    "to_polar",
]
