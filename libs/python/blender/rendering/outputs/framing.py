"""Frame size, overscan, render region, and safe areas."""

from fractions import Fraction
from typing import Annotated, Self

from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np

from blender.imaging.faults import guarded
from blender.imaging.parameters import Case, Choice, NonNegative, Positive, Presentation, Quantity, Unit
from blender.rendering.faults import RenderingArithmeticFault, RenderingResult, RenderingSizeFault

# --- [TYPES] ----------------------------------------------------------------------------

type Extents = Whole | Region
type Inset = tuple[Unit, Unit]
type Side = Annotated[int, msgspec.Meta(ge=1)]


class Fit(Choice):
    """Camera axis sensor size spans."""

    AUTO = 1
    HORIZONTAL = 2
    VERTICAL = 3


class Broadcast(Choice):
    """Broadcast standard of safe area preset."""

    WIDE = 1, "16:9", "Clear center-cut safe areas for a full 16:9 frame"
    PROTECTED = 2, "14:9 in 16:9", "Set center-cut safe areas for a 14:9 frame inside 16:9"
    CENTER_CUT = 3, "4:3 in 16:9", "Set center-cut safe areas for a 4:3 frame inside 16:9"


# --- [MODELS] ---------------------------------------------------------------------------


class Whole(Case, frozen=True, kw_only=True, tag=5):
    """Whole frame grown on each side by overscan."""

    title = "Whole Frame"
    overscan: Annotated[NonNegative, msgspec.Meta(description="Share of frame side added past each edge, camera widened to keep view"), Presentation(soft=Some((0.0, 0.25)), decimals=Some(3))] = 0.0


class Region(Case, frozen=True, kw_only=True, tag=6):
    """Frame rectangle from bottom-left, rendered alone."""

    left: Annotated[Unit, msgspec.Meta(description="Share of frame width"), Presentation(quantity=Some(Quantity.FRAME_WIDTH))] = 0.0
    bottom: Annotated[Unit, msgspec.Meta(description="Share of frame height"), Presentation(quantity=Some(Quantity.FRAME_HEIGHT))] = 0.0
    right: Annotated[Unit, msgspec.Meta(description="Share of frame width"), Presentation(quantity=Some(Quantity.FRAME_WIDTH))] = 1.0
    top: Annotated[Unit, msgspec.Meta(description="Share of frame height"), Presentation(quantity=Some(Quantity.FRAME_HEIGHT))] = 1.0
    crop: Annotated[bool, msgspec.Meta(description="Write region pixels alone")] = False


class Framing(msgspec.Struct, frozen=True, kw_only=True):
    """Frame size, aspect, density, and render extent."""

    resolution_x: Annotated[Side, msgspec.Meta(description="Horizontal pixels at full scale")] = 1920
    resolution_y: Annotated[Side, msgspec.Meta(description="Vertical pixels at full scale")] = 1080
    resolution_percentage: Annotated[int, msgspec.Meta(ge=1, title="Resolution Scale", description="Percentage of full size rendered"), Presentation(soft=Some((1.0, 100.0)), step=Some(10.0))] = 100
    pixel_aspect_x: Positive = 1.0
    pixel_aspect_y: Positive = 1.0
    density: Annotated[Positive, msgspec.Meta(title="Pixel Density", description="Pixels per meter in file metadata")] = 72 / 0.0254
    extent: Annotated[Extents, msgspec.Meta(description="Whole frame with overscan, or region rendered alone")] = Whole()

    @property
    def size(self) -> tuple[int, int]:
        """Frame pixels after percentage, each axis floored."""
        return self.resolution_x * self.resolution_percentage // 100, self.resolution_y * self.resolution_percentage // 100

    @property
    def padding(self) -> tuple[int, int]:
        """Overscan pixels beyond each edge per axis."""
        width, height = self.size
        match self.extent:
            case Whole(overscan=overscan):
                fraction = Fraction(overscan)
                return round(width * fraction), round(height * fraction)
            case Region():
                return 0, 0

    def pixels(self) -> RenderingResult[tuple[int, int]]:
        """Returns covered pixels, or arithmetic fault of float32 region bounds."""
        (width, height), (pad_x, pad_y) = self.size, self.padding
        match self.extent:
            case Whole():
                return Ok((width + 2 * pad_x, height + 2 * pad_y))
            case Region(left=left, bottom=bottom, right=right, top=top):

                def counted() -> RenderingResult[tuple[int, int]]:
                    across, up = np.float32(width), np.float32(height)
                    return Ok((int(np.float32(right) * across) - int(np.float32(left) * across), int(np.float32(top) * up) - int(np.float32(bottom) * up)))

                return guarded(RenderingArithmeticFault, (width, height), counted)

    def rendered(self, bounds: tuple[int, int]) -> RenderingResult[tuple[Self, tuple[Side, Side]]]:
        """Validates pixels, folding overscan into resolution within renderer bounds."""

        def validated(pixels: tuple[int, int]) -> RenderingResult[tuple[Self, tuple[Side, Side]]]:
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

        return self.pixels().bind(validated)

    def growth(self, fit: Fit) -> Option[Fraction]:
        """Returns exact camera growth on fit axis, Nothing for zero-pixel axis."""
        (width, height), (pad_x, pad_y) = self.size, self.padding
        pad, side = (pad_x, width) if fit is Fit.HORIZONTAL or (fit is Fit.AUTO and width * Fraction(self.pixel_aspect_x) >= height * Fraction(self.pixel_aspect_y)) else (pad_y, height)
        return Some(Fraction(side + 2 * pad, side)) if side else Nothing


class SafeAreas(msgspec.Struct, frozen=True, kw_only=True):
    """Title and action safe margins of frame and center cut."""

    title: Annotated[Inset, msgspec.Meta(title="Title Safe", description="Share of frame side outside text area, half per edge")] = (0.1, 0.05)
    action: Annotated[Inset, msgspec.Meta(title="Action Safe", description="Share of frame side outside action area, half per edge")] = (0.035, 0.035)
    title_center: Annotated[Inset, msgspec.Meta(title="Center-Cut Title Safe")] = (0.175, 0.05)
    action_center: Annotated[Inset, msgspec.Meta(title="Center-Cut Action Safe")] = (0.15, 0.05)

    @classmethod
    def broadcast(cls, standard: Broadcast) -> Self:
        """Returns safe margins with center-cut margins of standard."""
        match standard:
            case Broadcast.WIDE:
                return cls(title_center=(0.0, 0.0), action_center=(0.0, 0.0))
            case Broadcast.PROTECTED:
                return cls(title_center=(0.15, 0.05), action_center=(0.1, 0.05))
            case Broadcast.CENTER_CUT:
                return cls()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Broadcast", "Extents", "Fit", "Framing", "Inset", "Region", "SafeAreas", "Side", "Whole"]
