"""GPU texture uploads and preview texture caches."""

from collections import OrderedDict
from collections.abc import Hashable
from contextlib import nullcontext
from typing import Final

from expression import Option
import gpu
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderEditFault, BlenderResult
from blender.imaging.color.processors import unassociated

# --- [SERVICES] -------------------------------------------------------------------------


class Textures(msgspec.Struct, kw_only=True, eq=False):
    """Preview textures of one extension, least recently shown first."""

    held: OrderedDict[Hashable, gpu.types.GPUTexture] = msgspec.field(default_factory=OrderedDict)

    @property
    def memory(self) -> int:
        """Estimated RGBA16F base-level pixel bytes."""
        return sum(texture.width * texture.height for texture in self.held.values()) * 4 * np.dtype(np.float16).itemsize

    def shown(self, key: Hashable) -> Option[gpu.types.GPUTexture]:
        """Reads texture by content digest or request key, marking it most recent."""
        if key in self.held:
            self.held.move_to_end(key)
        return Option.of_optional(self.held.get(key))

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
            self.held[key] = texture
            self.held.move_to_end(key)
            for victim in [held for held in self.held if held not in kept][: max(0, len(self.held) - limit)]:
                del self.held[victim]
            return texture

        return uploaded(unassociated(plane)).map(published)


# --- [OPERATIONS] -----------------------------------------------------------------------


def uploaded(plane: npt.NDArray[np.float32]) -> BlenderResult[gpu.types.GPUTexture]:
    """Uploads top-first RGBA into RGBA16F texture with bottom-first rows."""
    pixels = np.ascontiguousarray(plane[::-1])
    return BlenderEditFault.attempted("gpu.init", gpu.init).bind(
        lambda _: BlenderEditFault.attempted(
            "gpu.types.GPUTexture",
            lambda: gpu.types.GPUTexture((pixels.shape[1], pixels.shape[0]), format="RGBA16F", data=gpu.types.Buffer("FLOAT", pixels.shape, pixels)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

TEXTURES: Final = Scoped[Textures](made=lambda _identity: nullcontext(Textures()))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TEXTURES", "Textures", "uploaded"]
