"""Preview pixels of one extension: a preview collection holding staged glyphs and generated tiles over the theme transparency checker, and half-float GPU preview textures."""

from collections import OrderedDict
from collections.abc import Generator, Hashable
from contextlib import closing, contextmanager, nullcontext
from functools import partial
from typing import Final, Self

import bpy
import bpy.utils.previews
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import gpu
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.identity import Resource
from blender.bpy.extension.reports import redrawn
from blender.bpy.ui.resources.theme import Checker, Metrics
from blender.records.faults import Fault, Outcome, Reason

# --- [CONSTANTS] ------------------------------------------------------------------------

_FORMAT: Final = "RGBA16F"
_COMPONENT: Final = np.dtype(np.float16)

# --- [MODELS] ---------------------------------------------------------------------------


class PreviewKey(msgspec.Struct, frozen=True, kw_only=True):
    """Key, tile side in pixels, and checker of a drawn tile."""

    key: Hashable
    side: int
    checker: Checker

    @classmethod
    def read(cls, key: Hashable, side: int = bpy.app.render_preview_size) -> Self:
        """Returns request at the current checker, Blender's preview render size by default."""
        return cls(key=key, side=side, checker=Metrics.read().checker())

    def checkered(self, plane: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
        """Composites top-first associated RGBA over the checker as opaque RGBA."""
        height, width, _ = plane.shape
        rows, columns = np.indices((height, width), sparse=True)
        cell = self.checker.cell
        ground = np.where(((rows // cell + columns // cell) % 2 == 0)[..., None], np.asarray(self.checker.primary[:3], np.float32), np.asarray(self.checker.secondary[:3], np.float32))
        return np.concatenate((plane[..., :3] + (1.0 - plane[..., 3:]) * ground, np.ones((height, width, 1), np.float32)), axis=-1)


# --- [SERVICES] -------------------------------------------------------------------------


class Previews:
    """Preview collection of one extension and the finished request per slot, pixelless requests included."""

    __slots__ = ("collection", "drawn")

    def __init__(self, collection: bpy.utils.previews.ImagePreviewCollection) -> None:
        """Holds the collection with no finished request."""
        self.collection = collection
        self.drawn: dict[str, Hashable] = {}

    @classmethod
    @contextmanager
    def opened(cls) -> Generator[Self]:
        """Holds a new preview collection, closed at scope exit.

        Yields:
            Previews over the open collection.
        """
        with closing(bpy.utils.previews.new()) as collection:
            yield cls(collection)

    # --- [READING]

    def icon_id(self, slot: str) -> Option[int]:
        """Returns slot icon ID, Nothing while slot holds no pixels."""
        return Option.of_optional(self.collection.get(slot)).map(lambda preview: preview.icon_id)

    def current(self, slot: str, key: Hashable) -> bool:
        """Returns whether the slot finished the request."""
        return self.drawn.get(slot) == key

    # --- [WRITING]

    def written(self, slot: str, key: Hashable, plane: Option[npt.NDArray[np.float32]]) -> Outcome[Option[int]]:
        """Writes top-first RGBA into the slot's image and icon buffers at full size, or drops the slot for no pixels, then records the finished request."""
        match plane:
            case Option(tag="some", some=pixels):
                height, width, _ = pixels.shape
                flat, size = np.ascontiguousarray(pixels[::-1]).reshape(-1), (width, height)
                preview = Option.of_optional(self.collection.get(slot)).default_with(lambda: self.collection.new(slot))
                preview.image_size = size  # type: ignore[assignment]
                preview.image_pixels_float.foreach_set(flat)  # type: ignore[arg-type]
                preview.icon_size = size  # type: ignore[assignment]
                preview.icon_pixels_float.foreach_set(flat)  # type: ignore[arg-type]
                self.drawn[slot] = key
                return redrawn().map(lambda _: Some(preview.icon_id))
            case _:
                dropped = self.dropped(slot)
                self.drawn[slot] = key
                return dropped.map(lambda _: Nothing)

    def dropped(self, slot: str) -> Outcome[None]:
        """Clears slot and its finished request, redrawing when a preview was removed."""
        self.drawn.pop(slot, None)
        if slot in self.collection:
            del self.collection[slot]
            return redrawn().map(lambda _: None)
        return Ok(None)


class Textures:
    """Preview textures of one extension, least recently shown first."""

    __slots__ = ("held",)

    def __init__(self) -> None:
        """Holds no texture."""
        self.held: OrderedDict[Hashable, gpu.types.GPUTexture] = OrderedDict()

    @property
    def memory(self) -> int:
        """Estimated base-level pixel bytes in the upload format."""
        return sum(texture.width * texture.height for texture in self.held.values()) * 4 * _COMPONENT.itemsize

    def cleared(self) -> int:
        """Releases every held texture, answering how many it held."""
        count = len(self.held)
        self.held.clear()
        return count

    def shown(self, key: Hashable) -> Option[gpu.types.GPUTexture]:
        """Reads texture by content digest or request key, marking it most recent."""
        if key in self.held:
            self.held.move_to_end(key)
        return Option.of_optional(self.held.get(key))

    def stored(self, key: Hashable, plane: npt.NDArray[np.float32], limit: int, kept: frozenset[Hashable]) -> Outcome[gpu.types.GPUTexture]:
        """Uploads associated display colors drawn under `ALPHA_PREMULT` blending, evicting oldest unpinned textures past limit.

        Args:
            key: Content digest with color transfer, or render request key.
            plane: Top-first RGBA with associated display colors.
            limit: Texture count past which eviction starts.
            kept: Keys open editors pin.

        Returns:
            Uploaded texture, even when evicted, or GPU fault.
        """

        def published(texture: gpu.types.GPUTexture) -> gpu.types.GPUTexture:
            """Caches upload, evicting entries past limit."""
            self.held[key] = texture
            self.held.move_to_end(key)
            for victim in [held for held in self.held if held not in kept][: max(0, len(self.held) - limit)]:
                del self.held[victim]
            return texture

        return uploaded(plane).map(published)


# --- [OPERATIONS] -----------------------------------------------------------------------


def uploaded(plane: npt.NDArray[np.float32]) -> Outcome[gpu.types.GPUTexture]:
    """Uploads top-first RGBA into a half-float RGBA texture with bottom-first rows."""
    pixels = np.ascontiguousarray(plane[::-1])
    return (
        catch(gpu.init, exception=SystemError)()
        .map_error(partial(Fault.of, Reason.REFUSED, "gpu.init"))
        .bind(
            lambda _: catch(gpu.types.GPUTexture, exception=RuntimeError)((pixels.shape[1], pixels.shape[0]), format=_FORMAT, data=gpu.types.Buffer("FLOAT", pixels.shape, pixels)).map_error(  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                partial(Fault.of, Reason.REFUSED, "gpu.types.GPUTexture")
            )
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

PREVIEWS: Final = Resource[Previews](made=lambda _identity: Previews.opened())
TEXTURES: Final = Resource[Textures](made=lambda _identity: nullcontext(Textures()))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PREVIEWS", "TEXTURES", "PreviewKey", "Previews", "Textures", "uploaded"]
