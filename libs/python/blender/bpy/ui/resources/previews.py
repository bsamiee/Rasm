"""One preview collection per extension holding staged glyphs and generated tiles over the theme transparency checker."""

from collections.abc import Callable, Generator, Hashable
from contextlib import contextmanager
from typing import Final, Self

import bpy
import bpy.utils.previews
from expression import Nothing, Ok, Option
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.identity import Identity
from blender.bpy.extension.reports import redrawn
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderResult
from blender.bpy.ui.resources.theme import Checker, Metrics

# --- [MODELS] ---------------------------------------------------------------------------


class Drawn(msgspec.Struct, frozen=True, kw_only=True):
    """Finished preview request."""

    key: Hashable
    side: int
    checker: Checker

    @classmethod
    def read(cls, key: Hashable, scale: float) -> Self:
        """Returns request at current interface metrics."""
        metrics = Metrics.read()
        return cls(key=key, side=int(scale * metrics.unit), checker=metrics.checker())


# --- [SERVICES] -------------------------------------------------------------------------


class Previews(msgspec.Struct, kw_only=True, eq=False):
    """Preview collection of one extension and the finished request per tile slot, pixelless requests included."""

    collection: bpy.utils.previews.ImagePreviewCollection
    drawn: dict[str, Hashable] = msgspec.field(default_factory=dict)

    # --- [LIFECYCLE]

    @classmethod
    @contextmanager
    def opened(cls, _identity: Identity) -> Generator[Self]:
        """Holds a new preview collection until scope exit.

        Args:
            _identity: Extension holding the collection.

        Yields:
            Previews over the open collection.
        """
        collection = bpy.utils.previews.new()
        try:
            yield cls(collection=collection)
        finally:
            bpy.utils.previews.remove(collection)

    # --- [READING]

    def icon_id(self, slot: str) -> Option[int]:
        """Returns slot icon ID, Nothing while slot holds no pixels."""
        return Option.of_optional(self.collection.get(slot)).map(lambda preview: preview.icon_id)

    def current(self, slot: str, key: Hashable, scale: float) -> bool:
        """Returns whether request finished at current tile side and checker."""
        return self.drawn.get(slot) == Drawn.read(key, scale)

    # --- [WRITING]

    def tile(self, slot: str, key: Hashable, rendered: Callable[[int], BlenderResult[Option[Option[npt.NDArray[np.float32]]]]], scale: float) -> BlenderResult[Option[int]]:
        """Writes rendered pixels over the checker or records pixelless finish, waiting while render returns Nothing."""
        shown = Drawn.read(key, scale)
        return (
            Ok(self.icon_id(slot))
            if self.drawn.get(slot) == shown
            else rendered(shown.side).bind(lambda outcome: outcome.map(lambda plane: self.finished(slot, shown, plane.map(lambda pixels: checkered(pixels, shown.checker)))).default_value(Ok(Nothing)))
        )

    def picture(self, slot: str, key: Hashable, rendered: Callable[[Metrics], BlenderResult[Option[npt.NDArray[np.float32]]]]) -> BlenderResult[Option[int]]:
        """Writes rendered straight RGBA once per key and interface metrics, dropping slot while render returns Nothing."""
        metrics = Metrics.read()
        shown = (key, metrics.scale, metrics.pixel_size)
        return Ok(self.icon_id(slot)) if self.drawn.get(slot) == shown else rendered(metrics).bind(lambda plane: self.finished(slot, shown, plane))

    def finished(self, slot: str, shown: Hashable, plane: Option[npt.NDArray[np.float32]]) -> BlenderResult[Option[int]]:
        """Writes or drops slot pixels, then records the finished request."""
        icon = plane.map(lambda pixels: filled(Option.of_optional(self.collection.get(slot)).default_with(lambda: self.collection.new(slot)), pixels).icon_id)
        redraw = redrawn().map(lambda _: None) if plane.is_some() else self.dropped(slot)
        self.drawn[slot] = shown
        return redraw.map(lambda _: icon)

    def dropped(self, slot: str) -> BlenderResult[None]:
        """Clears slot, redrawing when a preview was removed."""
        self.drawn.pop(slot, None)
        if slot in self.collection:
            del self.collection[slot]
            return redrawn().map(lambda _: None)
        return Ok(None)


# --- [OPERATIONS] -----------------------------------------------------------------------


def checkered(plane: npt.NDArray[np.float32], checker: Checker) -> npt.NDArray[np.float32]:
    """Composites top-first premultiplied RGBA over checker as opaque RGBA."""
    height, width, _ = plane.shape
    rows, columns = np.indices((height, width), sparse=True)
    ground = np.where(((rows // checker.cell + columns // checker.cell) % 2 == 0)[..., None], np.asarray(checker.primary[:3], dtype=np.float32), np.asarray(checker.secondary[:3], dtype=np.float32))
    return np.concatenate((plane[..., :3] + (1.0 - plane[..., 3:]) * ground, np.ones((height, width, 1), np.float32)), axis=-1)


def filled(preview: bpy.types.ImagePreview, plane: npt.NDArray[np.float32]) -> bpy.types.ImagePreview:
    """Writes top-first RGBA into preview image and icon buffers at full size."""
    height, width, _ = plane.shape
    pixels, size = np.ascontiguousarray(plane[::-1]).reshape(-1), (width, height)
    preview.image_size = size  # type: ignore[assignment]
    preview.image_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
    preview.icon_size = size  # type: ignore[assignment]
    preview.icon_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
    return preview


# --- [COMPOSITION] ----------------------------------------------------------------------

PREVIEWS: Final = Scoped[Previews](made=Previews.opened)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PREVIEWS", "Drawn", "Previews", "checkered", "filled"]
