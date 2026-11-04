"""Preview pixels of one extension: its preview collection and GPU preview textures."""

from collections import OrderedDict
from collections.abc import Generator, Hashable
from contextlib import closing, contextmanager
from functools import partial
from itertools import count, islice
from typing import Final, Self

import bpy
import bpy.utils.previews
from expression import Option, Some
from expression.extra.result import catch
import gpu
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.registration import Resource
from blender.bpy.extension.reports import redrawn
from blender.bpy.ui.resources.theme import Checker, Metrics
from blender.records.faults import Fault, Nothing, Ok, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Texture[M] = tuple[gpu.types.GPUTexture, M]

# --- [CONSTANTS] ------------------------------------------------------------------------

_FORMAT: Final = "RGBA16F"
_COMPONENT: Final = np.dtype(np.float16)

# --- [MODELS] ---------------------------------------------------------------------------


class _Slot(msgspec.Struct, frozen=True, kw_only=True):
    serial: Option[int]
    finished: Hashable


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

    __slots__ = ("_serials", "collection", "held")

    def __init__(self, collection: bpy.utils.previews.ImagePreviewCollection) -> None:
        """Holds the collection with no finished request."""
        self.collection = collection
        self.held: dict[Hashable, _Slot] = {}
        self._serials = count()

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

    def get(self, slot: Hashable) -> Option[bpy.types.ImagePreview]:
        """Returns the slot's native preview, Nothing while it holds no pixels."""
        return Option.of_optional(self.held.get(slot)).bind(lambda held: held.serial).map(lambda serial: self.collection[str(serial)])

    def read(self, slot: Hashable, key: Hashable) -> Option[Option[bpy.types.ImagePreview]]:
        """Returns the finished request's preview or pixelless result, Nothing for an absent or stale request."""
        return Some(held.serial.map(lambda serial: self.collection[str(serial)])) if (held := self.held.get(slot)) is not None and held.finished == key else Nothing

    # --- [WRITING]

    def written(self, slot: Hashable, key: Hashable, plane: Option[npt.NDArray[np.float32]]) -> Outcome[Option[int]]:
        """Writes top-first RGBA into the slot's image and icon buffers at full size, or drops the slot for no pixels, then records the finished request."""
        match plane:
            case Option(tag="some", some=pixels):
                height, width, _ = pixels.shape
                serial, preview = (
                    Option
                    .of_optional(self.held.get(slot))
                    .bind(lambda held: held.serial)
                    .map(lambda serial: (serial, self.collection[str(serial)]))
                    .default_with(lambda: ((serial := next(self._serials)), self.collection.new(str(serial))))
                )
                filled(preview, np.ascontiguousarray(pixels[::-1]).reshape(-1), (width, height))
                self.held[slot] = _Slot(serial=Some(serial), finished=key)
                return redrawn().map(lambda _: Some(preview.icon_id))
            case _:
                dropped = self.dropped(slot)
                self.held[slot] = _Slot(serial=Nothing, finished=key)
                return dropped.map(lambda _: Nothing)

    def dropped(self, slot: Hashable) -> Outcome[None]:
        """Clears slot and its finished request, redrawing when a preview was removed."""
        for serial in Option.of_optional(self.held.pop(slot, None)).bind(lambda held: held.serial).to_list():
            del self.collection[str(serial)]
            return redrawn().map(lambda _: None)
        return Ok(None)


class Textures[M]:
    """Preview textures and their metadata, least recently shown first."""

    __slots__ = ("held",)

    def __init__(self) -> None:
        """Holds no texture."""
        self.held: OrderedDict[Hashable, Texture[M]] = OrderedDict()

    @property
    def memory(self) -> int:
        """Estimated base-level pixel bytes in the upload format."""
        return sum(texture.width * texture.height for texture, _ in self.held.values()) * 4 * _COMPONENT.itemsize

    def cleared(self) -> int:
        """Releases every held texture, answering how many it held."""
        count = len(self.held)
        self.held.clear()
        return count

    def shown(self, key: Hashable) -> Option[Texture[M]]:
        """Reads texture and metadata by key, marking them most recent."""
        if (entry := self.held.get(key)) is not None:
            self.held.move_to_end(key)
        return Option.of_optional(entry)

    def stored(self, key: Hashable, plane: npt.NDArray[np.float32], limit: int, kept: frozenset[Hashable], *, metadata: M) -> Outcome[Texture[M]]:
        """Uploads associated display colors drawn under `ALPHA_PREMULT` blending, evicting oldest unpinned textures past limit.

        Args:
            key: Content digest with color transfer, or render request key.
            plane: Top-first RGBA with associated display colors.
            limit: Texture count past which eviction starts.
            kept: Keys open editors pin.
            metadata: Value retained and evicted with the texture.

        Returns:
            Uploaded texture and metadata, even when evicted, or GPU fault.
        """

        def published(texture: gpu.types.GPUTexture) -> Texture[M]:
            """Caches upload, evicting entries past limit."""
            entry = self.held[key] = texture, metadata
            self.held.move_to_end(key)
            for victim in tuple(islice((held for held in self.held if held not in kept), max(0, len(self.held) - limit))):
                del self.held[victim]
            return entry

        return uploaded(plane).map(published)


# --- [OPERATIONS] -----------------------------------------------------------------------


def filled(preview: bpy.types.ImagePreview, pixels: npt.NDArray[np.float32], size: tuple[int, int] | bpy.types.bpy_prop_array) -> None:
    """Writes flat RGBA into a preview's image and icon buffers at one size."""
    preview.image_size = preview.icon_size = size  # type: ignore[assignment]
    preview.image_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
    preview.icon_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]


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

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PREVIEWS", "PreviewKey", "Previews", "Texture", "Textures", "filled", "uploaded"]
