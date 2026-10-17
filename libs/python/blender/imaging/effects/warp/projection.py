"""Mirror folds, polar conversions, and panorama mappings of a rendered frame."""

from collections.abc import Callable
import math
from typing import Annotated

from expression import Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.imaging.chain import Effect, Stage
from blender.imaging.geometry.cameras import Camera
from blender.imaging.geometry.panoramas import CentralCylindrical, Equirectangular, FisheyeEquidistant, FisheyeEquisolid, MirrorBall, Panini, PanoramaView, Stereographic
from blender.imaging.geometry.stmaps import Extension
from blender.imaging.parameters import AtLeastOne, Case, Choice, Position, Presentation, Turn

# --- [TYPES] ----------------------------------------------------------------------------


class PolarConversion(Choice):
    """Directions between a rectangular strip and its inscribed polar disc."""

    RECTANGULAR_TO_POLAR = 1, "Rectangular to Polar"
    POLAR_TO_RECTANGULAR = 2, "Polar to Rectangular"


type PanoramaModes = Equirectangular | CentralCylindrical | MirrorBall | FisheyeEquidistant | FisheyeEquisolid | Stereographic | Panini

# --- [MODELS] ---------------------------------------------------------------------------


class Sampling(Case, frozen=True, kw_only=True):
    """Frame mapping with its editable source-border sampling rule."""

    extension: Annotated[Extension, msgspec.Meta(description="Content sampled past the frame edge")] = Extension.MIRROR


class Fold(Sampling, frozen=True, kw_only=True):
    """Frame positions folded about a common center."""

    center: Annotated[Position, msgspec.Meta(description="Point the fold turns about")] = (0.5, 0.5)


class Kaleidoscope(Fold, frozen=True, kw_only=True, tag=9):
    """Mirror lines meeting at a center with a zoom about that center."""

    mirrors: Annotated[int, msgspec.Meta(ge=1, description="Mirror lines through the center"), Presentation(soft=Some((1.0, 16.0)))] = 3
    angle: Annotated[Turn, msgspec.Meta(description="Turn of the first mirror line from the horizontal")] = math.pi / 2.0
    zoom: Annotated[float, msgspec.Meta(ge=2.0**-15, le=2.0**15, description="Magnification about the fold center"), Presentation(soft=Some((1.0 / 16.0, 16.0)))] = 1.0

    def folded(self, local: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Reflects displayed offsets into the first wedge and applies the centered zoom."""
        wedge = math.pi / self.mirrors
        bearing = self.angle + np.abs(np.mod(np.arctan2(local[..., 1], local[..., 0]) - self.angle + wedge, 2.0 * wedge) - wedge)
        radius = np.hypot(local[..., 0], local[..., 1]) / self.zoom
        return np.stack((radius * np.cos(bearing), radius * np.sin(bearing)), axis=-1, dtype=np.float64)


class Tiling(Fold, frozen=True, kw_only=True, tag=10):
    """Frame copies scaled about a center."""

    count: Annotated[AtLeastOne, msgspec.Meta(description="Frame copies across each side"), Presentation(soft=Some((1.0, 8.0)))] = 3.0


class Polar(Sampling, frozen=True, kw_only=True, tag=11):
    """Full rectangular turn wrapped into a disc or unwrapped from one."""

    title = "Polar Coordinates"
    extension: Annotated[Extension, msgspec.Meta(description="Content sampled past the frame edge")] = Extension.CLIP
    conversion: Annotated[PolarConversion, msgspec.Meta(description="Wrap the rectangular image into a disc or unwrap its disc")] = PolarConversion.RECTANGULAR_TO_POLAR

    def mapped(self, samples: npt.NDArray[np.float64], span: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps covered bottom-left samples using the displayed frame's inscribed circle.

        Args:
            samples: Source positions followed by their geometric coverage.
            span: Displayed width and height as fractions of the greater side.

        Returns:
            Sampling positions with unchanged geometric coverage and the seam above center.
        """
        short = float(np.min(span))
        match self.conversion:
            case PolarConversion.RECTANGULAR_TO_POLAR:
                local = (samples[..., :2] - 0.5) * span
                return np.stack((np.mod(np.arctan2(local[..., 0], local[..., 1]), math.tau) / math.tau, 1.0 - 2.0 * np.hypot(local[..., 0], local[..., 1]) / short, samples[..., 2]), axis=-1)
            case PolarConversion.POLAR_TO_RECTANGULAR:
                bearing = math.tau * samples[..., 0]
                radius = (1.0 - samples[..., 1]) * short / 2.0
                return np.concatenate((0.5 + radius[..., None] * np.stack((np.sin(bearing), np.cos(bearing)), axis=-1) / span, samples[..., 2:]), axis=-1)


class Panoramic(Sampling, frozen=True, kw_only=True, tag=12):
    """Perspective frame sampled into a native panorama projection."""

    title = "Panorama"
    projection: Annotated[PanoramaModes, msgspec.Meta(description="Panorama receiving the perspective frame")] = Equirectangular()


type Modes = Kaleidoscope | Tiling | Polar | Panoramic


class Projection(Effect, frozen=True, kw_only=True, tag=39):
    """Mirror folds, polar conversions, or a rendered frame placed into a panorama."""

    stage = Stage.WARP
    pointwise = False
    mode: Annotated[Modes, msgspec.Meta(description="Fold about a center or panorama receiving the frame")] = Kaleidoscope()

    def mapping(self, camera: Camera) -> Callable[[npt.NDArray[np.float64]], npt.NDArray[np.float64]]:
        """Maps output samples to source positions and carries their projection coverage."""
        match self.mode:
            case Kaleidoscope() as kaleidoscope:
                span = camera.lens.displayed
                return lambda samples: np.concatenate((kaleidoscope.center + kaleidoscope.folded((samples[..., :2] - kaleidoscope.center) * span) / span, samples[..., 2:]), axis=-1)
            case Tiling() as tiling:
                return lambda samples: np.concatenate((tiling.center + tiling.count * (samples[..., :2] - tiling.center), samples[..., 2:]), axis=-1)
            case Polar() as polar:
                span = camera.lens.displayed
                return lambda samples: polar.mapped(samples, span)
            case Panoramic(projection=panorama):
                right, up, back = camera.placement.rotation.apply((0.0, 0.0, 1.0), inverse=True)
                return PanoramaView(
                    source=panorama, view=camera.lens, elevation=math.atan2(-back, math.hypot(right, up)) if panorama.upright else 0.0, roll=math.atan2(right, up) if panorama.upright else 0.0
                ).to_view


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Fold", "Kaleidoscope", "Modes", "PanoramaModes", "Panoramic", "Polar", "PolarConversion", "Projection", "Sampling", "Tiling"]
