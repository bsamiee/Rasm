"""Rigid perspective cameras between world points and frame positions with depth."""

import msgspec
import numpy as np
import numpy.typing as npt
from scipy.spatial.transform import RigidTransform

from rasm.imaging.geometry.panoramas import Rectilinear

# --- [MODELS] ---------------------------------------------------------------------------


class Camera(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Perspective lens with a native rigid camera-to-world placement."""

    placement: "RigidTransform[tuple[()]]"
    lens: Rectilinear

    def to_screen(self, points: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Returns s, t, camera-space depth, and coverage, zero at and behind the camera."""
        local = self.placement.apply(points, inverse=True)
        positions = self.lens.to_image(np.concatenate((local, np.ones_like(local[..., :1])), axis=-1))
        return np.concatenate((positions[..., :2], -local[..., 2:], positions[..., 2:]), axis=-1)

    def to_world(self, screen: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Lifts s, t, and camera-space depth to world points in meters."""
        planar, exponent = np.frexp(screen[..., :2] - 0.5 + self.lens.shift)
        depth, depth_power = np.frexp(screen[..., 2:3])
        scale, power = self.lens.scale
        local = np.concatenate((np.ldexp(planar * scale * depth, exponent + power + depth_power), -screen[..., 2:3]), axis=-1)
        return self.placement.apply(local)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Camera"]
