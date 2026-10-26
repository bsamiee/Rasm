"""GPU texture uploads and preview texture caches."""

from collections import OrderedDict
from collections.abc import Hashable
from contextlib import nullcontext
from typing import Final

from expression import Option
from expression.extra.result import catch
import gpu
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderReason
from blender.imaging.files.images import unassociated
from blender.records.faults import Fault, Outcome

# --- [SERVICES] -------------------------------------------------------------------------


class Textures:
    """Preview textures of one extension, least recently shown first."""

    __slots__ = ("held",)

    def __init__(self) -> None:
        """Holds no texture."""
        self.held: OrderedDict[Hashable, gpu.types.GPUTexture] = OrderedDict()

    @property
    def memory(self) -> int:
        """Estimated RGBA16F base-level pixel bytes."""
        return sum(texture.width * texture.height for texture in self.held.values()) * 4 * np.dtype(np.float16).itemsize

    def shown(self, key: Hashable) -> Option[gpu.types.GPUTexture]:
        """Reads texture by content digest or request key, marking it most recent."""
        if key in self.held:
            self.held.move_to_end(key)
        return Option.of_optional(self.held.get(key))

    def stored(self, key: Hashable, plane: npt.NDArray[np.float32], limit: int, kept: frozenset[Hashable]) -> Outcome[gpu.types.GPUTexture]:
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


def uploaded(plane: npt.NDArray[np.float32]) -> Outcome[gpu.types.GPUTexture]:
    """Uploads top-first RGBA into RGBA16F texture with bottom-first rows."""
    pixels = np.ascontiguousarray(plane[::-1])
    return (
        catch(gpu.init, exception=RuntimeError)()
        .map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject="gpu.init", cause=error),))
        .bind(
            lambda _: catch(gpu.types.GPUTexture, exception=RuntimeError)((pixels.shape[1], pixels.shape[0]), format="RGBA16F", data=gpu.types.Buffer("FLOAT", pixels.shape, pixels)).map_error(  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                lambda error: (Fault(reason=BlenderReason.REFUSED, subject="gpu.types.GPUTexture", cause=error),)
            )
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

TEXTURES: Final = Scoped[Textures](made=lambda _identity: nullcontext(Textures()))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TEXTURES", "Textures", "uploaded"]
