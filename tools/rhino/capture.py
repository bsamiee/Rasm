# ty: ignore[unresolved-import]
# mypy: disable-error-code="import-not-found, no-any-unimported"
"""Write one view of a Rhino document to `.artifacts/rhino/<name>.jpg`, run inside Rhino through `runpy.run_path`."""

from pathlib import Path

import msgspec
from Rhino import RhinoDoc
from Rhino.Display import DisplayModeDescription
from Rhino.Geometry import BoundingBox, Point3d
from System.Drawing import Size
from System.Drawing.Imaging import ImageFormat

# --- [MODELS] ---------------------------------------------------------------------------

type Corner = tuple[float, float, float]
type Frame = tuple[Corner, Corner]


class Capture(msgspec.Struct, frozen=True):
    """Written file and the world bounding box the view framed."""

    path: Path
    frame: Frame


class UnknownView(msgspec.Struct, frozen=True):
    """View name outside the document's views."""

    name: str
    views: tuple[str, ...]


class UnknownMode(msgspec.Struct, frozen=True):
    """Display mode name outside Rhino's display modes."""

    name: str
    modes: tuple[str, ...]


class PointFrame(msgspec.Struct, frozen=True):
    """Box collapsed to one corner, an empty document's visible box included, with nothing to frame."""

    corner: Corner


# --- [OPERATIONS] -----------------------------------------------------------------------


def capture(
    doc: RhinoDoc, name: str, frame: Frame | None = None, view: str = "Perspective", mode: str = "Shaded", size: tuple[int, int] = (1280, 720)
) -> Capture | UnknownView | UnknownMode | PointFrame:
    """Frame the given world bounding box, or every visible object, in one view and write the view as a JPEG."""
    box = doc.Objects.BoundingBoxVisible if frame is None else BoundingBox(Point3d(*frame[0]), Point3d(*frame[1]))
    lower: Corner = (box.Min.X, box.Min.Y, box.Min.Z)
    upper: Corner = (box.Max.X, box.Max.Y, box.Max.Z)
    if (rhino_view := doc.Views.Find(view, compareCase=True)) is None:
        return UnknownView(view, tuple(known.ActiveViewport.Name for known in doc.Views))
    if (description := DisplayModeDescription.FindByName(mode)) is None:
        return UnknownMode(mode, tuple(known.EnglishName for known in DisplayModeDescription.GetDisplayModes()))
    if lower == upper:
        return PointFrame(lower)
    rhino_view.ActiveViewport.ZoomBoundingBox(box)
    path = Path(__file__).parents[2] / ".artifacts" / "rhino" / f"{name}.jpg"
    path.parent.mkdir(parents=True, exist_ok=True)
    bitmap = rhino_view.CaptureToBitmap(Size(*size), description)
    try:
        bitmap.Save(str(path), ImageFormat.Jpeg)
    finally:
        bitmap.Dispose()
    return Capture(path, (lower, upper))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Capture", "PointFrame", "UnknownMode", "UnknownView", "capture"]
