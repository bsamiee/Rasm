"""Frame shares, positions, and rectangles measured from the bottom left of a displayed frame."""

from typing import Annotated, Self

from expression import Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.records.fields import Finite, NonNegative, Positive, Presentation, Quantity, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type FrameLength = Annotated[NonNegative, Presentation(soft=Some((0.0, 1.0)), quantity=Some(Quantity.FRAME))]
type FrameExtent = Annotated[Positive, Presentation(soft=Some((0.0, 1.0)), quantity=Some(Quantity.FRAME))]
type Position = Annotated[tuple[Finite, Finite], Presentation(soft=Some((0.0, 1.0)), quantity=Some(Quantity.FRAME))]

# --- [MODELS] ---------------------------------------------------------------------------


class Region(msgspec.Struct, frozen=True, kw_only=True):
    """Frame rectangle in shares from bottom left."""

    left: Annotated[Unit, Presentation(quantity=Some(Quantity.FRAME))] = 0.0
    bottom: Annotated[Unit, Presentation(quantity=Some(Quantity.FRAME))] = 0.0
    right: Annotated[Unit, Presentation(quantity=Some(Quantity.FRAME))] = 1.0
    top: Annotated[Unit, Presentation(quantity=Some(Quantity.FRAME))] = 1.0

    @classmethod
    def bounding(cls, points: npt.NDArray[np.float64]) -> Self:
        """Returns rectangle spanning the first two components of `points`."""
        corners = points[..., :2].reshape(-1, 2)
        (left, bottom), (right, top) = corners.min(axis=0), corners.max(axis=0)
        return cls(left=float(left), bottom=float(bottom), right=float(right), top=float(top))

    def window(self, height: int, width: int) -> tuple["slice[int, int, None]", "slice[int, int, None]"]:
        """Returns row and column slices of a top-first plane."""
        return slice(round((1.0 - self.top) * height), round((1.0 - self.bottom) * height)), slice(round(self.left * width), round(self.right * width))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FrameExtent", "FrameLength", "Position", "Region"]
