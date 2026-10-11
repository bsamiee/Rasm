"""Generated preview tiles and data-block pixels over the native checker."""

from collections.abc import Callable, Hashable
from typing import ClassVar, Self

import bpy
import bpy.utils.previews
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt

from rasm.blender.extension.identity import Identity
from rasm.blender.extension.reports import redrawn
from rasm.blender.faults import BlenderResult
from rasm.blender.ui.resources.theme import Checker, Metrics

# --- [MODELS] ---------------------------------------------------------------------------


class Drawn(msgspec.Struct, frozen=True, kw_only=True):
    """Request identity and display facts of one completed preview."""

    key: Hashable
    side: int
    checker: Checker

    @classmethod
    def read(cls, key: Hashable, scale: float) -> Self:
        """Returns the request identity and native display facts for a tile scale."""
        metrics = Metrics.read()
        return cls(key=key, side=int(scale * metrics.unit), checker=metrics.checker())


class TileSet(msgspec.Struct, frozen=True, kw_only=True):
    """Native preview items and the facts of completed requests, including absence."""

    collection: bpy.utils.previews.ImagePreviewCollection
    drawn: dict[str, Drawn]


# --- [SERVICES] -------------------------------------------------------------------------


class Previews(msgspec.Struct, frozen=True, kw_only=True):
    """One extension's generated preview collection, acquired once by its scope."""

    identity: Identity
    held: ClassVar[dict[str, TileSet]] = {}

    # --- [LIFECYCLE]

    def __enter__(self) -> None:
        """Creates the extension's native collection."""
        Previews.held[self.identity.package] = TileSet(collection=bpy.utils.previews.new(), drawn={})

    def __exit__(self, *_: object) -> None:
        """Removes the extension's held collection and releases its native items."""
        bpy.utils.previews.remove(Previews.held.pop(self.identity.package).collection)

    # --- [READING]

    def icon_id(self, slot: str) -> Option[int]:
        """Returns a slot's native icon ID, absent before pixels or after completed absence."""
        return Option.of_optional(Previews.held[self.identity.package].collection.get(slot)).map(lambda preview: preview.icon_id)

    def current(self, slot: str, key: Hashable, scale: float) -> bool:
        """Returns whether a request completed at the current display facts, with or without pixels."""
        return Previews.held[self.identity.package].drawn.get(slot) == Drawn.read(key, scale)

    # --- [WRITING]

    def tile(self, slot: str, key: Hashable, rendered: Callable[[int], BlenderResult[Option[Option[npt.NDArray[np.float32]]]]], scale: float) -> BlenderResult[Option[int]]:
        """Publishes admitted pixels or completed absence, leaving withdrawn requests untouched."""
        tiles, shown = Previews.held[self.identity.package], Drawn.read(key, scale)

        def drawn(plane: Option[npt.NDArray[np.float32]]) -> BlenderResult[Option[int]]:
            """Completes the native item change before publishing its request and display facts."""
            match plane.tag:
                case "some":
                    preview = Option.of_optional(tiles.collection.get(slot)).default_with(lambda: tiles.collection.new(slot))
                    icon = Some(written(preview, plane.some, shown.checker).icon_id)
                    redraw = redrawn().map(lambda _: None)
                case "none":
                    icon = Nothing
                    redraw = self.dropped(slot)
            tiles.drawn[slot] = shown
            return redraw.map(lambda _: icon)

        return Ok(self.icon_id(slot)) if tiles.drawn.get(slot) == shown else rendered(shown.side).bind(lambda admitted: admitted.map(drawn).default_value(Ok(Nothing)))

    def dropped(self, slot: str) -> BlenderResult[None]:
        """Releases a slot and its completion, redrawing when a native item was removed."""
        tiles = Previews.held[self.identity.package]
        tiles.drawn.pop(slot, None)
        if slot in tiles.collection:
            del tiles.collection[slot]
            return redrawn().map(lambda _: None)
        return Ok(None)


# --- [OPERATIONS] -----------------------------------------------------------------------


def written(preview: bpy.types.ImagePreview, plane: npt.NDArray[np.float32], checker: Checker) -> bpy.types.ImagePreview:
    """Writes top-first premultiplied display RGBA over the checker into native buffers."""
    height, width, _ = plane.shape
    rows, columns = np.indices((height, width), sparse=True)
    ground = np.where(((rows // checker.cell + columns // checker.cell) % 2 == 0)[..., None], np.asarray(checker.primary[:3], dtype=np.float32), np.asarray(checker.secondary[:3], dtype=np.float32))
    opaque = np.concatenate((plane[..., :3] + (1.0 - plane[..., 3:]) * ground, np.ones((height, width, 1), np.float32)), axis=-1)
    pixels, size = np.ascontiguousarray(opaque[::-1]).reshape(-1), (width, height)
    preview.image_size = size  # type: ignore[assignment]
    preview.image_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
    preview.icon_size = size  # type: ignore[assignment]
    preview.icon_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
    return preview


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Drawn", "Previews", "TileSet", "written"]
