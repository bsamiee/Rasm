"""Imaging refusal reasons."""

from enum import StrEnum

# --- [TYPES] ----------------------------------------------------------------------------


class ImagingReason(StrEnum):
    """Color, image buffer, and RGB geometry refusals, each value the report template of its fault."""

    COLOR = "Color operation on {subject} failed. {cause}"
    UNRESOLVED = "Color configuration {subject} lacks color spaces {member}"
    BUFFER = "Image buffer {subject} failed. {cause}"
    BASIS = "Color space {subject} transform {member} lacks linear RGB geometry"
    SPECTRAL = "{subject} spectral matching cannot normalize RGB integrals {member}"


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ImagingReason"]
