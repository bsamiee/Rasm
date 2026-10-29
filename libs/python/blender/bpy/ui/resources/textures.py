"""GPU texture uploads and preview texture caches."""

from collections import OrderedDict
from collections.abc import Hashable
from contextlib import nullcontext
from functools import partial
from typing import Final

from expression import Option
from expression.extra.result import catch
import gpu
import numpy as np
import numpy.typing as npt

from blender.bpy.extension.identity import Resource
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Outcome

# --- [CONSTANTS] ------------------------------------------------------------------------

_FORMAT: Final = "RGBA16F"
_COMPONENT: Final = np.dtype(np.float16)

# --- [SERVICES] -------------------------------------------------------------------------


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
        .map_error(partial(Fault.of, BlenderReason.REFUSED, "gpu.init"))
        .bind(
            lambda _: catch(gpu.types.GPUTexture, exception=RuntimeError)((pixels.shape[1], pixels.shape[0]), format=_FORMAT, data=gpu.types.Buffer("FLOAT", pixels.shape, pixels)).map_error(  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                partial(Fault.of, BlenderReason.REFUSED, "gpu.types.GPUTexture")
            )
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

TEXTURES: Final = Resource[Textures](made=lambda _identity: nullcontext(Textures()))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TEXTURES", "Textures", "uploaded"]
