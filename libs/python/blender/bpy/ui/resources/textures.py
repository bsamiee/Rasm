"""GPU texture uploads and preview texture caches."""

from collections import OrderedDict
from collections.abc import Hashable
from typing import ClassVar

from expression import Error, Ok, Option
import gpu
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderEditFault, BlenderResult
from blender.imaging.color.processors import unassociated

# --- [SERVICES] -------------------------------------------------------------------------


class TextureCache(msgspec.Struct, frozen=True, kw_only=True):
    """Preview textures of one package."""

    identity: Identity
    held: ClassVar[dict[str, OrderedDict[Hashable, gpu.types.GPUTexture]]] = {}

    # --- [LIFECYCLE]

    def __enter__(self) -> None:
        """Creates package texture cache."""
        TextureCache.held[self.identity.package] = OrderedDict()

    def __exit__(self, *_: object) -> None:
        """Drops package texture references."""
        del TextureCache.held[self.identity.package]

    # --- [CACHING]

    @property
    def memory(self) -> int:
        """Estimated RGBA16F base-level pixel bytes."""
        return sum(texture.width * texture.height for texture in TextureCache.held[self.identity.package].values()) * 4 * np.dtype(np.float16).itemsize

    def shown(self, key: Hashable) -> Option[gpu.types.GPUTexture]:
        """Reads texture by content digest or request key, marking it most recent."""
        if (texture := (textures := (TextureCache.held[self.identity.package])).get(key)) is not None:
            textures.move_to_end(key)
        return Option.of_optional(texture)

    def stored(self, key: Hashable, plane: npt.NDArray[np.float32], limit: int, kept: frozenset[Hashable]) -> BlenderResult[gpu.types.GPUTexture]:
        """Uploads unassociated display colors, evicting oldest unpinned textures past limit.

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
            textures = TextureCache.held[self.identity.package]
            textures[key] = texture
            textures.move_to_end(key)
            for victim in [held for held in textures if held not in kept][: max(0, len(textures) - limit)]:
                del textures[victim]
            return texture

        return uploaded(unassociated(plane)).map(published)


# --- [OPERATIONS] -----------------------------------------------------------------------


def uploaded(plane: npt.NDArray[np.float32]) -> BlenderResult[gpu.types.GPUTexture]:
    """Uploads top-first RGBA into RGBA16F texture with bottom-first rows."""
    pixels = np.ascontiguousarray(plane[::-1])
    try:
        gpu.init()
        return Ok(gpu.types.GPUTexture((pixels.shape[1], pixels.shape[0]), format="RGBA16F", data=gpu.types.Buffer("FLOAT", pixels.shape, pixels)))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    except (SystemError, RuntimeError) as error:
        return Error((BlenderEditFault(request="gpu.types.GPUTexture", error=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TextureCache", "uploaded"]
