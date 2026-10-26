"""Nuke layer names, one-channel names, and vector axes for files a downstream Nuke reads."""

from collections.abc import Mapping
from enum import IntEnum
from types import MappingProxyType
from typing import ClassVar, Literal

from expression import Option
import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Axis = Literal[-3, -2, -1, 1, 2, 3]


class Space(IntEnum):
    """Space of spatial vector pass."""

    WORLD = 1
    CAMERA = 2


# --- [MODELS] ---------------------------------------------------------------------------


class Nuke(msgspec.Struct, frozen=True, kw_only=True):
    """Nuke layer and one-channel names by the source pass name the writer holds, its vector axes, and its motion channel order."""

    names: Mapping[str, str]
    channels: Mapping[str, str]
    axes: ClassVar[Mapping[Space, tuple[Axis, Axis, Axis]]] = MappingProxyType({Space.WORLD: (1, 3, -2), Space.CAMERA: (1, 2, -3)})
    motion: ClassVar[tuple[int, int, int, int]] = (3, 4, 2, 3)

    def layer(self, identifier: str) -> str:
        """Returns the Nuke layer name of a source pass."""
        return self.names.get(identifier, identifier)

    def channel(self, identifier: str) -> Option[str]:
        """Returns the Nuke channel name of a one-channel source pass, Nothing to keep the source name."""
        return Option.of_optional(self.channels.get(identifier))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Axis", "Nuke", "Space"]
