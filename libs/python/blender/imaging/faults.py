"""Imaging refusal reasons."""

from enum import StrEnum

# --- [TYPES] ----------------------------------------------------------------------------


class ImagingReason(StrEnum):
    """Color, image buffer, codec, and array shape refusals, each value the report template of its fault."""

    COLOR = "Color operation on {subject} failed. {cause}"
    UNRESOLVED = "Color configuration {subject} lacks color spaces {member}"
    BUFFER = "Image buffer {subject!r} failed. {cause}"
    CODEC = "JPEG coding of {subject} samples failed. {cause}"
    COMPARISON = "Image comparison at {subject} failed. {cause}"
    MISMATCH = "{subject} shapes {member} and {cause} differ"
    SHAPE = "{subject} has incompatible shape {member}"
    BASIS = "Color space {subject} transform {member} lacks linear RGB geometry"
    SPECTRAL = "{subject} spectral matching cannot normalize RGB integrals {member}"


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ImagingReason"]
