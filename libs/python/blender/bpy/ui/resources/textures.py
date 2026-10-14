"""Native RGBA texture uploads and package-scoped preview caches."""

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
    """Package preview textures acquired once by their registration scope."""

    identity: Identity
    held: ClassVar[dict[str, OrderedDict[Hashable, gpu.types.GPUTexture]]] = {}

    # --- [LIFECYCLE]

    def __enter__(self) -> None:
        """Creates the package's cache for the registration lifetime."""
        TextureCache.held[self.identity.package] = OrderedDict()

    def __exit__(self, *_: object) -> None:
        """Drops the package's owned texture references at release."""
        del TextureCache.held[self.identity.package]

    # --- [CACHING]

    @property
    def memory(self) -> int:
        """Estimated base-level RGBA16F pixel bytes, excluding mipmaps and driver storage."""
        return sum(texture.width * texture.height for texture in TextureCache.held[self.identity.package].values()) * 4 * np.dtype(np.float16).itemsize

    def shown(self, key: Hashable) -> Option[gpu.types.GPUTexture]:
        """Reads a texture and marks it most recently shown.

        Args:
            key: Content digest or retained request identity of the preview.

        Returns:
            Some of the held native texture, or Nothing when absent.
        """
        if (texture := (textures := (TextureCache.held[self.identity.package])).get(key)) is not None:
            textures.move_to_end(key)
        return Option.of_optional(texture)

    def stored(self, key: Hashable, plane: npt.NDArray[np.float32], limit: int, kept: frozenset[Hashable]) -> BlenderResult[gpu.types.GPUTexture]:
        """Uploads straight display colors and evicts the oldest unpinned previews past the limit.

        Args:
            key: Content digest including color transfer, or retained render demand.
            plane: Top-first float32 RGBA pixels with associated display colors.
            limit: Caller-owned cache preference.
            kept: Preview keys retained by open editors.

        Returns:
            Ok of the uploaded texture, including when eviction removed its cache reference,
            or Error of the native GPU initialization or allocation refusal.
        """

        def published(texture: gpu.types.GPUTexture) -> gpu.types.GPUTexture:
            """Publishes a completed upload and releases excess cache references."""
            textures = TextureCache.held[self.identity.package]
            textures[key] = texture
            textures.move_to_end(key)
            for victim in [held for held in textures if held not in kept][: max(0, len(textures) - limit)]:
                del textures[victim]
            return texture

        return uploaded(unassociated(plane)).map(published)


# --- [OPERATIONS] -----------------------------------------------------------------------


def uploaded(plane: npt.NDArray[np.float32]) -> BlenderResult[gpu.types.GPUTexture]:
    """Uploads RGBA pixels into a native half-float texture.

    Args:
        plane: Top-first float32 RGBA pixels in the caller's color space.

    Returns:
        Ok of an RGBA16F texture with bottom-first rows and the supplied alpha,
        or Error of the native GPU initialization or allocation refusal.
    """
    pixels = np.ascontiguousarray(plane[::-1])
    try:
        gpu.init()
        return Ok(gpu.types.GPUTexture((pixels.shape[1], pixels.shape[0]), format="RGBA16F", data=gpu.types.Buffer("FLOAT", pixels.shape, pixels)))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    except (SystemError, RuntimeError) as error:
        return Error((BlenderEditFault(request="gpu.types.GPUTexture", error=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TextureCache", "uploaded"]
