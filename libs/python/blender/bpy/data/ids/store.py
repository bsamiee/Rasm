"""Session handles, native ID collections, and collision-aware renames."""

from collections.abc import Iterable
from typing import Self

import bpy
from expression import Error, Ok, Option, Result, Some
import msgspec

from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.imaging.faults import accumulated

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
        return resolved((self,)).map(lambda found: found[0])


# --- [OPERATIONS] -----------------------------------------------------------------------


def member(kind: type[bpy.types.ID]) -> str:
    """Returns the BlendData collection with the nearest fixed type in the kind's MRO."""
    stores: dict[type[object], str] = {type(prop.fixed_type): prop.identifier for prop in bpy.types.BlendData.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty)}
    return next(stores[parent] for parent in kind.__mro__ if parent in stores)


def blocks[T: bpy.types.ID](data: bpy.types.BlendData, kind: type[T]) -> tuple[T, ...]:
    """Returns the kind's data-blocks in native collection order from an open or temporary BlendData store."""
    return tuple(block for block in getattr(data, member(kind)) if isinstance(block, kind))


def resolved[T: bpy.types.ID](handles: Iterable[Handle[T]]) -> BlenderResult[tuple[T, ...]]:
    """Returns each handle's live original in order from one session identity map of the stores its kinds name, with an absent fault per expired handle."""
    held = tuple(handles)
    live = {block.session_uid: block for store in dict.fromkeys(map(member, dict.fromkeys(handle.kind for handle in held))) for block in getattr(bpy.data, store)}

    def found(handle: Handle[T]) -> BlenderResult[T]:
        return Option.of_optional(live.get(handle.uid)).filter(lambda block: isinstance(block, handle.kind)).to_result_with(lambda: (BlenderAbsentFault(owner=handle.kind.__name__, key=handle.uid),))

    return accumulated(map(found, held))


def embedded(block: bpy.types.ID) -> Option[bpy.types.NodeTree]:
    """Returns the node tree a data-block embeds, `Nothing` for a kind declaring none or an empty pointer."""
    return _tree_member(block.bl_rna).bind(lambda member: Option.of_optional(getattr(block, member)))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


def holder(block: bpy.types.ID) -> Option[bpy.types.ID]:
    """Returns the `bpy.data` holder of a data-block: itself, or the data-block embedding it as its node tree, `Nothing` for an embedded tree none holds."""
    stores = (prop.identifier for prop in bpy.types.BlendData.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty) and Option.of_optional(prop.fixed_type).bind(_tree_member).is_some())
    return Option.of_optional(next((owner for store in stores for owner in getattr(bpy.data, store) if embedded(owner) == Some(block)), None)) if block.is_embedded_data else Some(block)


def named[T: bpy.types.ID](block: T, name: str) -> BlenderResult[T]:
    """Returns the renamed block, a host edit refusal, or the collision with another block holding the name, refused before any rename."""

    def renamed() -> BlenderResult[T]:
        try:
            block.rename(name, mode="NEVER")
        except (AttributeError, RuntimeError) as error:
            return Error((BlenderEditFault(request=repr(block), error=error),))
        return Ok(block)

    key = (name, Option.of_optional(block.library).map(lambda library: library.filepath).to_optional())
    return (
        Option
        .of_optional(getattr(bpy.data, member(type(block))).get(key))
        .filter(lambda holding: holding != block)
        .map(lambda holding: Result[T, BlenderFaults].Error((BlenderCollisionFault(request=name, error=repr(holding)),)))
        .default_with(renamed)
    )


def _tree_member(struct: bpy.types.Struct) -> Option[str]:
    """Identifier of the embedded node tree pointer a struct declares, a read-only pointer whose fixed type is a node tree."""
    return Option.of_optional(
        next((prop.identifier for prop in struct.properties if isinstance(prop, bpy.types.PointerProperty) and prop.is_readonly and isinstance(prop.fixed_type, bpy.types.NodeTree)), None)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Handle", "blocks", "embedded", "holder", "member", "named", "resolved"]
