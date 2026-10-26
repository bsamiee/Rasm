"""Pass names and vector axes per downstream reader."""

from collections.abc import Mapping
from types import MappingProxyType
from typing import Annotated, ClassVar, Literal

from expression import Option
import msgspec

from blender.records.fields import Case, Choice

# --- [TYPES] ----------------------------------------------------------------------------

type Axis = Literal[-3, -2, -1, 1, 2, 3]
type Conventions = Blender | Nuke


class Space(Choice):
    """Space of spatial vector pass."""

    WORLD = 1
    CAMERA = 2


# --- [MODELS] ---------------------------------------------------------------------------


class Convention(Case, frozen=True, kw_only=True):
    """Names and axes of one downstream reader."""

    names: ClassVar[Mapping[str, str]]
    channels: ClassVar[Mapping[str, str]] = MappingProxyType({"Depth": "Z"})
    axes: ClassVar[Mapping[Space, tuple[Axis, Axis, Axis]]]
    motion: ClassVar[tuple[int, int, int, int]]
    normals: Annotated[Space, msgspec.Meta(description="Normal pass space")] = Space.WORLD

    def layer(self, identifier: str) -> str:
        """Returns reader layer name of pass."""
        return self.names.get(identifier, identifier)

    def channel(self, identifier: str) -> Option[str]:
        """Returns reader name of one-channel pass, Nothing to keep Blender name."""
        return Option.of_optional(self.channels.get(identifier))


class Blender(Convention, frozen=True):
    """Blender pass names and axes."""

    names = MappingProxyType({})
    axes = MappingProxyType(dict.fromkeys(Space, (1, 2, 3)))
    motion = (1, 2, 3, 4)


class Nuke(Convention, frozen=True):
    """Nuke pass names and axes."""

    names = MappingProxyType({"Combined": "rgba", "Position": "Pworld", "Depth": "z"})
    axes = MappingProxyType({Space.WORLD: (1, 3, -2), Space.CAMERA: (1, 2, -3)})
    motion = (3, 4, 2, 3)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Axis", "Blender", "Convention", "Conventions", "Nuke", "Space"]
