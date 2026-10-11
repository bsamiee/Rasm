"""Session handles, native ID collections, and collision-aware renames."""

from typing import Self

import bpy
from expression import Error, Ok, Option
import msgspec

from rasm.blender.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderResult

# --- [MODELS] ---------------------------------------------------------------------------


class Handle[T: bpy.types.ID](msgspec.Struct, frozen=True, kw_only=True):
    """Original data-block's type and session identity."""

    kind: type[T]
    uid: int

    @classmethod
    def of(cls, block: T) -> Self:
        """Returns a handle naming the original of evaluated data."""
        return cls(kind=type(block), uid=block.original.session_uid)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

    def resolve(self) -> BlenderResult[T]:
        """Returns the live original, or an absent fault for an expired session identity."""
        return Option.of_optional(next((block for block in blocks(bpy.data, self.kind) if block.session_uid == self.uid), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=self.kind.__name__, key=self.uid),)
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def member(kind: type[bpy.types.ID]) -> str:
    """Returns the BlendData collection with the nearest fixed type in the kind's MRO."""
    stores: dict[type[object], str] = {type(prop.fixed_type): prop.identifier for prop in bpy.types.BlendData.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty)}
    return next(stores[parent] for parent in kind.__mro__ if parent in stores)


def blocks[T: bpy.types.ID](data: bpy.types.BlendData, kind: type[T]) -> tuple[T, ...]:
    """Returns the kind's data-blocks in native collection order from an open or temporary BlendData store."""
    return tuple(block for block in getattr(data, member(kind)) if isinstance(block, kind))


def named[T: bpy.types.ID](block: T, name: str) -> BlenderResult[T]:
    """Returns the renamed block, a host edit refusal, or a name collision."""
    try:
        outcome = block.rename(name, mode="NEVER")
    except (AttributeError, RuntimeError) as error:
        return Error((BlenderEditFault(request=repr(block), error=error),))
    return Ok(block) if outcome in {"UNCHANGED", "RENAMED_NO_COLLISION"} else Error((BlenderCollisionFault(request=name, error=repr(block)),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Handle", "blocks", "member", "named"]
