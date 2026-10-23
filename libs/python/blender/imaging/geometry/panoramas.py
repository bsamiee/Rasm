"""Image projections and oriented perspective views into panoramas."""

import math
import sys
from typing import Annotated, ClassVar, Self

from expression import Nothing, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
from scipy.spatial.transform import Rotation

from blender.imaging.geometry.homography import Quad
from blender.imaging.geometry.stmaps import Extension
from blender.imaging.parameters import Case, Elevation, Finite, NonNegative, Positive, Presentation, Quantity, Turn, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type FieldOfView = Annotated[float, msgspec.Meta(gt=0.0, lt=math.pi), Presentation(quantity=Some(Quantity.ANGLE))]
type SensorDimension = Annotated[float, msgspec.Meta(ge=0.001, le=sys.float_info.max)]

# --- [MODELS] ---------------------------------------------------------------------------


class Panorama(Case, frozen=True, kw_only=True):
    """Projection between camera-space rays and image positions."""

    extension: ClassVar[tuple[Extension, Extension]] = (Extension.CLIP, Extension.CLIP)
    upright: ClassVar[bool] = False
    centered: ClassVar[bool] = False


class Fisheye(Panorama, frozen=True, kw_only=True):
    """Projection onto centered image circle."""

    field_of_view: Annotated[float, msgspec.Meta(ge=0.1745, le=10.0 * math.pi, title="Field of View", description="Angle the image circle spans"), Presentation(quantity=Some(Quantity.ANGLE))] = (
        math.pi
    )


class Rectilinear(Panorama, frozen=True, kw_only=True, tag=1):
    """Pinhole projection of a perspective camera."""

    aspect: Annotated[Positive, msgspec.Meta(description="Frame width over height")] = 4.0 / 3.0
    field_of_view: Annotated[FieldOfView, msgspec.Meta(title="Field of View", description="Horizontal angle the frame spans")] = math.radians(60.0)
    shift: Annotated[tuple[Finite, Finite], Presentation(quantity=Some(Quantity.FRAME))] = (0.0, 0.0)

    @property
    def scale(self) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.int32]]:
        """Binary mantissas and exponents of frame sides at unit depth."""
        mantissa, exponent = math.frexp(self.field_of_view * float(np.sinc(self.field_of_view / (2.0 * math.pi))) / math.cos(self.field_of_view / 2.0))
        sides, powers = np.frexp(np.array((1.0, self.aspect)))
        return mantissa / sides, exponent - powers

    @property
    def displayed(self) -> npt.NDArray[np.float64]:
        """Frame sides as shares of greater side."""
        return np.array((self.aspect, 1.0)) / max(self.aspect, 1.0)

    def to_image(self, rays: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Projects rays to frame, zero coverage at and behind camera."""
        depth = -rays[..., 2:3]
        numerator, power = np.frexp(rays[..., :2])
        denominator, depth_power = np.frexp(depth)
        scale, scale_power = self.scale
        planar = np.ldexp(np.divide(numerator, denominator * scale, out=np.zeros_like(numerator), where=depth > 0.0), power - depth_power - scale_power)
        return np.concatenate((planar + 0.5 - self.shift, rays[..., 3:] * (depth > 0.0)), axis=-1)

    def to_ray(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Lifts frame positions to unit rays."""
        mantissa, exponent = np.frexp(samples[..., :2] - 0.5 + self.shift)
        scale, power = self.scale
        mantissa = np.concatenate((mantissa * scale, -np.ones_like(samples[..., :1])), axis=-1)
        exponent = np.concatenate((exponent + power, np.zeros_like(samples[..., :1], dtype=np.int32)), axis=-1)
        common = np.max(exponent, axis=-1, keepdims=True, where=mantissa != 0.0, initial=0)
        length = np.hypot.reduce(np.ldexp(mantissa, exponent - common), axis=-1, keepdims=True)
        return np.concatenate((np.ldexp(mantissa / length, exponent - common), samples[..., 2:]), axis=-1)


class Equirectangular(Panorama, frozen=True, kw_only=True, tag=2):
    """Latitude and longitude over full sphere."""

    title = "Lat-Long"
    extension = (Extension.REPEAT, Extension.EXTEND)
    upright = True

    @staticmethod
    def to_image(rays: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps rays to longitude and latitude."""
        longitude = np.arctan2(rays[..., 0], -rays[..., 2])
        latitude = np.arctan2(rays[..., 1], np.hypot(rays[..., 0], rays[..., 2]))
        return np.stack((longitude / (2.0 * math.pi) + 0.5, latitude / math.pi + 0.5, rays[..., 3]), axis=-1)

    @staticmethod
    def to_ray(samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps longitude and latitude to unit rays."""
        longitude, latitude = (samples[..., 0] - 0.5) * 2.0 * math.pi, (samples[..., 1] - 0.5) * math.pi
        return np.stack((np.cos(latitude) * np.sin(longitude), np.sin(latitude), -np.cos(latitude) * np.cos(longitude), samples[..., 2]), axis=-1)


class CentralCylindrical(Panorama, frozen=True, kw_only=True, tag=3):
    """Projection from cylinder center about vertical axis."""

    title = "Cylindrical"
    extension = (Extension.REPEAT, Extension.CLIP)
    upright = True
    height_min: Annotated[float, msgspec.Meta(ge=-10.0, le=10.0, title="Height Minimum", description="Lowest height over radius")] = -1.0
    height_max: Annotated[float, msgspec.Meta(ge=-10.0, le=10.0, title="Height Maximum", description="Highest height over radius")] = 1.0

    def to_image(self, rays: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Projects rays, zero coverage on axis or for zero-height cylinder."""
        radial = np.hypot(rays[..., 0], rays[..., 2])
        span = self.height_max - self.height_min
        defined = (radial > 0.0) & (span != 0.0)
        height = np.divide(rays[..., 1], radial, out=np.zeros_like(radial), where=radial > 0.0)
        vertical = np.divide(height - self.height_min, span, out=np.zeros_like(height), where=defined)
        return np.stack((np.arctan2(rays[..., 0], -rays[..., 2]) / (2.0 * math.pi) + 0.5, vertical, rays[..., 3] * defined), axis=-1)

    def to_ray(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Lifts cylinder positions to rays, reversed or equal heights included."""
        longitude = (samples[..., 0] - 0.5) * 2.0 * math.pi
        local = np.stack((np.sin(longitude), self.height_min + samples[..., 1] * (self.height_max - self.height_min), -np.cos(longitude)), axis=-1)
        return np.concatenate((local / np.hypot.reduce(local, axis=-1, keepdims=True), samples[..., 2:]), axis=-1)


class MirrorBall(Panorama, frozen=True, kw_only=True, tag=4):
    """Mirrored sphere seen along view axis."""

    @staticmethod
    def to_image(rays: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps rays to ball circle, zero coverage straight behind."""
        normal = rays[..., :3] + (0.0, 0.0, -1.0)
        length = np.linalg.norm(normal, axis=-1, keepdims=True)
        planar = np.divide(normal[..., :2], length, out=np.zeros_like(normal[..., :2]), where=length > 0.0)
        return np.concatenate((0.5 + 0.5 * planar, rays[..., 3:] * (length > 0.0)), axis=-1)

    @staticmethod
    def to_ray(samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Reflects positions into unit rays, zero coverage outside ball."""
        planar = 2.0 * samples[..., :2] - 1.0
        rest = 1.0 - np.sum(planar**2, axis=-1, keepdims=True)
        normal = np.concatenate((planar, -np.sqrt(rest, out=np.zeros_like(rest), where=rest >= 0.0)), axis=-1)
        return np.concatenate((-2.0 * normal[..., 2:] * normal + (0.0, 0.0, 1.0), samples[..., 2:] * (rest >= 0.0)), axis=-1)


class FisheyeEquidistant(Fisheye, frozen=True, kw_only=True, tag=5):
    """Fisheye with radius proportional to view axis angle."""

    def to_image(self, rays: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps rays to image circle, zero coverage past half field of view."""
        radial = np.hypot(rays[..., :1], rays[..., 1:2])
        angle = np.arctan2(radial, -rays[..., 2:3])
        planar = np.divide(rays[..., :2], radial, out=np.zeros_like(rays[..., :2]), where=radial > 0.0)
        defined = (angle <= self.field_of_view / 2.0) & ((radial > 0.0) | (rays[..., 2:3] < 0.0))
        return np.concatenate((0.5 + angle / self.field_of_view * planar, rays[..., 3:] * defined), axis=-1)

    def to_ray(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Lifts positions to unit rays, zero coverage outside image circle."""
        planar = 2.0 * samples[..., :2] - 1.0
        radial = np.linalg.norm(planar, axis=-1, keepdims=True)
        angle = radial * self.field_of_view / 2.0
        unit = np.divide(planar, radial, out=np.zeros_like(planar), where=radial > 0.0)
        return np.concatenate((np.sin(angle) * unit, -np.cos(angle), samples[..., 2:] * (radial <= 1.0)), axis=-1)


class FisheyeEquisolid(Fisheye, frozen=True, kw_only=True, tag=6):
    """Fisheye with equisolid image radius on physical sensor."""

    lens: Annotated[float, msgspec.Meta(ge=0.00001, le=0.1, description="Focal length"), Presentation(quantity=Some(Quantity.CAMERA_LENGTH))] = 0.0105
    sensor: Annotated[tuple[SensorDimension, SensorDimension], Presentation(quantity=Some(Quantity.CAMERA_LENGTH))] = (0.036, 0.024)

    @property
    def radius(self) -> float:
        """Image circle radius on sensor at field of view."""
        return 2.0 * self.lens * math.sin(self.field_of_view / 4.0)

    def to_image(self, rays: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps rays to sensor, zero coverage outside image circle."""
        radial = np.hypot(rays[..., :1], rays[..., 1:2])
        angle = np.arctan2(radial, -rays[..., 2:3])
        planar = np.divide(rays[..., :2], radial, out=np.zeros_like(rays[..., :2]), where=radial > 0.0)
        radius = 2.0 * self.lens * np.sin(angle / 2.0)
        defined = (radius <= self.radius) & ((radial > 0.0) | (rays[..., 2:3] < 0.0))
        return np.concatenate((0.5 + radius * planar / self.sensor, rays[..., 3:] * defined), axis=-1)

    def to_ray(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Lifts sensor positions to rays, zero coverage outside image circle."""
        planar = (samples[..., :2] - 0.5) * self.sensor
        radial = np.hypot(planar[..., :1], planar[..., 1:2])
        inside = radial <= self.radius
        angle = 2.0 * np.arcsin(radial / (2.0 * self.lens), out=np.zeros_like(radial), where=inside)
        unit = np.divide(planar, radial, out=np.zeros_like(planar), where=inside & (radial > 0.0))
        return np.concatenate((np.sin(angle) * unit, -np.cos(angle), samples[..., 2:] * inside), axis=-1)


class Stereographic(Panorama, frozen=True, kw_only=True, tag=7):
    """Conformal projection of sphere from point behind viewer."""

    centered = True
    field_of_view: Annotated[float, msgspec.Meta(gt=0.0, lt=2.0 * math.pi, title="Field of View", description="Angle the frame's greater side spans"), Presentation(quantity=Some(Quantity.ANGLE))] = (
        math.pi
    )

    @property
    def scale(self) -> float:
        """Twice greater-side extent on projection plane."""
        return self.field_of_view * float(np.sinc(self.field_of_view / (4.0 * math.pi))) / math.cos(self.field_of_view / 4.0)

    def to_image(self, rays: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Projects rays to centered positions, zero coverage straight behind."""
        pole = 1.0 - rays[..., 2:3]
        numerator, power = np.frexp(rays[..., :2])
        denominator, pole_power = np.frexp(pole)
        scale, scale_power = math.frexp(self.scale)
        planar = np.ldexp(np.divide(numerator, denominator * scale, out=np.zeros_like(numerator), where=pole > 0.0), power - pole_power - scale_power + 1)
        return np.concatenate((planar, rays[..., 3:] * (pole > 0.0)), axis=-1)

    def to_ray(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Lifts centered positions to covered unit rays."""
        mantissa, exponent = np.frexp(samples[..., :2])
        scale, power = math.frexp(self.scale)
        mantissa, exponent = mantissa * scale, exponent + power - 1
        common = np.max(exponent, axis=-1, keepdims=True, where=mantissa != 0.0, initial=0)
        planar = np.ldexp(mantissa, exponent - common)
        pole = np.ldexp(np.ones_like(samples[..., :1]), -common)
        denominator = np.hypot(np.hypot(planar[..., :1], planar[..., 1:2]), pole) ** 2
        return np.concatenate((np.ldexp(2.0 * mantissa / denominator, exponent - 2 * common), 1.0 - 2.0 * pole**2 / denominator, samples[..., 2:]), axis=-1)


class Panini(Panorama, frozen=True, kw_only=True, tag=8):
    """Cylinder projection from behind axis, keeping verticals straight."""

    upright = True
    centered = True
    field_of_view: Annotated[FieldOfView, msgspec.Meta(title="Field of View", description="Angle the frame's greater side spans")] = math.radians(60.0)
    distance: Annotated[NonNegative, msgspec.Meta(description="Projection center behind sphere center, rectilinear at 0"), Presentation(soft=Some((0.0, 1.0)))] = 1.0
    compression: Annotated[Unit, msgspec.Meta(title="Vertical Compression", description="Blend from rectilinear to compressed heights")] = 0.0

    @property
    def weights(self) -> tuple[float, float]:
        """Perspective and cylindrical weights at projection distance."""
        total = self.distance + 1.0
        return 1.0 / total, self.distance / total

    @property
    def span(self) -> float:
        """Greater-side extent on projection plane."""
        half = self.field_of_view / 2.0
        perspective, cylindrical = self.weights
        return self.field_of_view * float(np.sinc(self.field_of_view / (2.0 * math.pi))) / (cylindrical + perspective * math.cos(half))

    def to_image(self, rays: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Projects rays, zero coverage at and behind camera."""
        depth, horizontal = -rays[..., 2:3], np.hypot(rays[..., :1], rays[..., 2:3])
        perspective, cylindrical = self.weights
        ratio = np.divide(depth, horizontal, out=np.zeros_like(depth), where=depth > 0.0)
        projection, projection_power = np.frexp(cylindrical + perspective * ratio)
        vertical = (1.0 - self.compression) * ratio + self.compression
        numerator, power = np.frexp(rays[..., :2])
        weight, weight_power = np.frexp(np.concatenate((np.ones_like(depth), vertical), axis=-1))
        denominator, denominator_power = np.frexp(np.concatenate((horizontal, depth), axis=-1))
        span, span_power = math.frexp(self.span)
        planar = np.ldexp(
            np.divide(numerator * weight, denominator * projection * span, out=np.zeros_like(numerator), where=depth > 0.0), power + weight_power - denominator_power - projection_power - span_power
        )
        return np.concatenate((planar, rays[..., 3:] * (depth > 0.0)), axis=-1)

    def to_ray(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Lifts centered positions to rays, zero coverage without forward ray."""
        planar = samples[..., :2] * self.span
        horizontal = planar[..., :1]
        perspective, cylindrical = self.weights
        length = np.hypot(1.0, perspective * horizontal)
        tangent, distant = perspective * horizontal / length, cylindrical * horizontal / length
        rest = (1.0 - distant) * (1.0 + distant)
        root = np.sqrt(rest, out=np.zeros_like(rest), where=rest >= 0.0)
        cosine = root / length - tangent * distant
        sine = (horizontal / length) * (perspective * root + cylindrical / length)
        ahead = (rest >= 0.0) & (cosine > 0.0)
        vertical = np.divide(cosine, (1.0 - self.compression) * cosine + self.compression, out=np.zeros_like(cosine), where=ahead)
        local = np.concatenate((sine, planar[..., 1:] * (cylindrical + perspective * cosine) * vertical, -cosine), axis=-1)
        return np.concatenate((np.divide(local, np.hypot.reduce(local, axis=-1, keepdims=True), out=np.zeros_like(local), where=ahead), samples[..., 2:] * ahead), axis=-1)


type Panoramas = Rectilinear | Equirectangular | CentralCylindrical | MirrorBall | FisheyeEquidistant | FisheyeEquisolid | Stereographic | Panini


class PanoramaView(msgspec.Struct, frozen=True, kw_only=True):
    """Perspective view into panorama."""

    source: Panoramas = Equirectangular()
    view: Rectilinear = Rectilinear()
    heading: Annotated[Turn, msgspec.Meta(description="Rightward turn from panorama center")] = 0.0
    elevation: Annotated[Elevation, msgspec.Meta(description="Upward tilt from horizon")] = 0.0
    roll: Annotated[Turn, msgspec.Meta(description="Counterclockwise turn about view axis")] = 0.0

    @property
    def rotation(self) -> Rotation[tuple[()]]:
        """Rotation taking view rays to panorama rays."""
        return Rotation.from_euler("YXZ", (-self.heading, self.elevation, self.roll))

    @property
    def frame(self) -> tuple[float, npt.NDArray[np.float64]]:
        """Origin and scale from view frame to source coordinates."""
        return (0.5, self.view.displayed) if self.source.centered else (0.0, np.ones(2))

    def to_panorama(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps view positions to source, keeping coverage."""
        (origin, scale), rays = self.frame, self.view.to_ray(samples)
        positions = self.source.to_image(np.concatenate((self.rotation.apply(rays[..., :3]), rays[..., 3:]), axis=-1))
        return np.concatenate((positions[..., :2] / scale + origin, positions[..., 2:]), axis=-1)

    def to_view(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps source positions into view, zero coverage behind view."""
        origin, scale = self.frame
        rays = self.source.to_ray(np.concatenate(((samples[..., :2] - origin) * scale, samples[..., 2:]), axis=-1))
        return self.view.to_image(np.concatenate((self.rotation.apply(rays[..., :3], inverse=True), rays[..., 3:]), axis=-1))

    def framing(self, quad: Quad, share: Annotated[float, msgspec.Meta(gt=0.0)]) -> Option[Self]:
        """Aims view at quad filling `share` of frame, `Nothing` for undefined, rearward, or zero-span corner rays."""
        origin, scale = self.frame
        samples = quad.samples
        rays = self.source.to_ray(np.concatenate(((samples[:, :2] - origin) * scale, samples[:, 2:]), axis=-1))
        sides = rays[[0, 1], :3] + rays[[3, 2], :3]
        forward = sides.sum(axis=0)
        level = msgspec.structs.replace(self, heading=math.atan2(forward[0], -forward[2]), elevation=math.atan2(forward[1], math.hypot(forward[0], forward[2])), roll=0.0)
        leveled_sides = level.rotation.apply(sides, inverse=True)
        across = np.diff(leveled_sides, axis=0)[0]
        aimed = msgspec.structs.replace(level, roll=math.atan2(across[1], across[0]))
        rotation = aimed.rotation
        local = rotation.apply(rays[:, :3], inverse=True)
        planar = np.divide(local[:, :2], -local[:, 2:], out=np.zeros_like(local[:, :2]), where=local[:, 2:] < 0.0)
        spread = max(float(np.ptp(planar[:, 0])), float(np.ptp(planar[:, 1])) * self.view.aspect)
        return (
            Some(msgspec.structs.replace(aimed, view=msgspec.structs.replace(self.view, field_of_view=2.0 * math.atan(spread / (2.0 * share)))))
            if bool(spread > 0.0 and np.all(rays[:, 3] > 0.0) and np.all(local[:, 2] < 0.0) and np.all(leveled_sides[:, 2] < 0.0))
            else Nothing
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CentralCylindrical",
    "Equirectangular",
    "FieldOfView",
    "Fisheye",
    "FisheyeEquidistant",
    "FisheyeEquisolid",
    "MirrorBall",
    "Panini",
    "Panorama",
    "PanoramaView",
    "Panoramas",
    "Rectilinear",
    "SensorDimension",
    "Stereographic",
]
