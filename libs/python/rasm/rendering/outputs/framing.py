"""Frame size, overscan, render region, and safe areas."""

from fractions import Fraction
import sys
from typing import Annotated, Self

from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np

from rasm.imaging.parameters import Case, Choice, Presentation, Quantity
from rasm.rendering.faults import RenderingResult, RenderingSizeFault

# --- [TYPES] ----------------------------------------------------------------------------

type Extents = Whole | Region
type Inset = tuple[Annotated[float, msgspec.Meta(ge=0.0, le=1.0)], Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]]
type Side = Annotated[int, msgspec.Meta(ge=1)]
type Aspect = Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max)]


class Fit(Choice):
    """Camera axis the sensor size spans."""

    AUTO = 1, "Auto"
    HORIZONTAL = 2, "Horizontal"
    VERTICAL = 3, "Vertical"


class Broadcast(Choice):
    """Broadcast standard of a safe area preset."""

    WIDE = 1, "16:9"
    PROTECTED = 2, "14:9 in 16:9"
    CENTER_CUT = 3, "4:3 in 16:9"


# --- [MODELS] ---------------------------------------------------------------------------


class Whole(Case, frozen=True, kw_only=True, tag=5):
    """Whole frame, grown on each side by the overscan."""

    title = "Whole Frame"
    overscan: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, description="Fraction of each frame side added beyond each edge, the camera grown to keep the frame's view"),
        Presentation(soft=Some((0.0, 0.25)), decimals=Some(3)),
    ] = 0.0


class Region(Case, frozen=True, kw_only=True, tag=6):
    """Rectangle of the frame measured from its bottom-left corner, rendered alone."""

    title = "Region"
    left: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Left edge as a fraction of the frame width"), Presentation(quantity=Some(Quantity.FRAME_WIDTH))] = 0.0
    bottom: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Bottom edge as a fraction of the frame height"), Presentation(quantity=Some(Quantity.FRAME_HEIGHT))] = 0.0
    right: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Right edge as a fraction of the frame width"), Presentation(quantity=Some(Quantity.FRAME_WIDTH))] = 1.0
    top: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Top edge as a fraction of the frame height"), Presentation(quantity=Some(Quantity.FRAME_HEIGHT))] = 1.0
    crop: Annotated[bool, msgspec.Meta(description="Write the region's pixels alone in place of the whole frame")] = False


class Framing(msgspec.Struct, frozen=True, kw_only=True):
    """Frame size, pixel aspect, pixel density, and the extent a render covers."""

    resolution_x: Annotated[Side, msgspec.Meta(description="Horizontal pixels at full scale")] = 1920
    resolution_y: Annotated[Side, msgspec.Meta(description="Vertical pixels at full scale")] = 1080
    resolution_percentage: Annotated[int, msgspec.Meta(ge=1, title="Resolution Scale", description="Percentage of the full size rendered"), Presentation(soft=Some((1.0, 100.0)), step=Some(10.0))] = (
        100
    )
    pixel_aspect_x: Annotated[Aspect, msgspec.Meta(description="Horizontal pixel aspect")] = 1.0
    pixel_aspect_y: Annotated[Aspect, msgspec.Meta(description="Vertical pixel aspect")] = 1.0
    density: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max, title="Pixel Density", description="Pixels per meter written into files that store a density")] = 72 / 0.0254
    extent: Annotated[Extents, msgspec.Meta(description="Whole frame with its overscan, or a region rendered alone")] = Whole()

    @property
    def size(self) -> tuple[int, int]:
        """Frame pixels after the percentage, each axis floored."""
        return self.resolution_x * self.resolution_percentage // 100, self.resolution_y * self.resolution_percentage // 100

    @property
    def padding(self) -> tuple[int, int]:
        """Pixels the overscan adds beyond each edge per axis."""
        width, height = self.size
        match self.extent:
            case Whole(overscan=overscan):
                fraction = Fraction(overscan)
                return round(width * fraction), round(height * fraction)
            case Region():
                return 0, 0

    def pixels(self) -> RenderingResult[tuple[int, int]]:
        """Answers covered pixels or the native region representation refusal."""
        (width, height), (pad_x, pad_y) = self.size, self.padding
        match self.extent:
            case Whole():
                return Ok((width + 2 * pad_x, height + 2 * pad_y))
            case Region(left=left, bottom=bottom, right=right, top=top):
                try:
                    with np.errstate(over="raise", invalid="raise", under="ignore"):
                        across, up = np.float32(width), np.float32(height)
                        return Ok((int(np.float32(right) * across) - int(np.float32(left) * across), int(np.float32(top) * up) - int(np.float32(bottom) * up)))
                except (OverflowError, FloatingPointError) as error:
                    return Error((RenderingSizeFault(request=(width, height), error=error),))

    def rendered(self, bounds: tuple[int, int]) -> RenderingResult[tuple[Self, tuple[Side, Side]]]:
        """Admits covered pixels and fits overscan within the receiving renderer's bounds."""

        def admitted(pixels: tuple[int, int]) -> RenderingResult[tuple[Self, tuple[Side, Side]]]:
            if any(side < 1 for side in pixels):
                return Error((RenderingSizeFault(request=pixels, error=Nothing),))
            pad_x, pad_y = self.padding
            if not (pad_x or pad_y):
                return Ok((self, pixels))
            grown_x, grown_y = pixels
            low, high = bounds
            return (
                Ok((msgspec.structs.replace(self, resolution_x=grown_x, resolution_y=grown_y, resolution_percentage=100, extent=Whole()), pixels))
                if low <= grown_x <= high and low <= grown_y <= high
                else Error((RenderingSizeFault(request=pixels, error=Some(bounds)),))
            )

        return self.pixels().bind(admitted)

    def growth(self, fit: Fit) -> Option[Fraction]:
        """Returns exact camera growth on the fit axis, absent for a zero-pixel axis."""
        (width, height), (pad_x, pad_y) = self.size, self.padding
        pad, side = (pad_x, width) if fit is Fit.HORIZONTAL or (fit is Fit.AUTO and width * Fraction(self.pixel_aspect_x) >= height * Fraction(self.pixel_aspect_y)) else (pad_y, height)
        return Some(Fraction(side + 2 * pad, side)) if side else Nothing


class SafeAreas(msgspec.Struct, frozen=True, kw_only=True):
    """Title and action safe margins of the frame and its center cut."""

    title: Annotated[Inset, msgspec.Meta(title="Title Safe", description="Fraction of each frame side outside the area for text and graphics, half beyond each edge")] = (0.1, 0.05)
    action: Annotated[Inset, msgspec.Meta(title="Action Safe", description="Fraction of each frame side outside the area for general elements, half beyond each edge")] = (0.035, 0.035)
    title_center: Annotated[Inset, msgspec.Meta(title="Center-Cut Title Safe", description="Title safe margins of the center-cut frame")] = (0.175, 0.05)
    action_center: Annotated[Inset, msgspec.Meta(title="Center-Cut Action Safe", description="Action safe margins of the center-cut frame")] = (0.15, 0.05)

    @classmethod
    def broadcast(cls, standard: Broadcast) -> Self:
        """Returns factory safe margins with the broadcast standard's center-cut margins."""
        match standard:
            case Broadcast.WIDE:
                return cls(title_center=(0.0, 0.0), action_center=(0.0, 0.0))
            case Broadcast.PROTECTED:
                return cls(title_center=(0.15, 0.05), action_center=(0.1, 0.05))
            case Broadcast.CENTER_CUT:
                return cls()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Aspect", "Broadcast", "Extents", "Fit", "Framing", "Inset", "Region", "SafeAreas", "Side", "Whole"]
