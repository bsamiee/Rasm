"""Session handles, BlendData collections, collision-checked renames, and vacant labels."""

from collections.abc import Iterable
import itertools
from typing import Self

import bpy
from expression import Error, Ok, Option, Result, Some
import msgspec

from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.imaging.faults import accumulated

# --- [MODELS] ---------------------------------------------------------------------------


class Handle[T: bpy.types.ID](msgspec.Struct, frozen=True, kw_only=True):
    """Session handle of original ID."""

    kind: type[T]
    uid: int

    @classmethod
    def of(cls, block: T) -> Self:
        """Returns handle of block's original."""
        return cls(kind=type(block), uid=block.original.session_uid)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

    def resolve(self) -> BlenderResult[T]:
        """Returns live original, absent fault once expired."""
        return resolved((self,)).map(lambda found: found[0])


# --- [OPERATIONS] -----------------------------------------------------------------------


def member(kind: type[bpy.types.ID]) -> str:
    """Returns BlendData collection name for kind's nearest MRO type."""
    stores: dict[type[object], str] = {type(prop.fixed_type): prop.identifier for prop in bpy.types.BlendData.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty)}
    return next(stores[parent] for parent in kind.__mro__ if parent in stores)


def blocks[T: bpy.types.ID](data: bpy.types.BlendData, kind: type[T]) -> tuple[T, ...]:
    """Returns blocks of kind in collection order."""
    return tuple(block for block in getattr(data, member(kind)) if isinstance(block, kind))


def resolved[T: bpy.types.ID](handles: Iterable[Handle[T]]) -> BlenderResult[tuple[T, ...]]:
    """Returns live originals in order, accumulating expired-handle faults."""
    held = tuple(handles)
    live = {block.session_uid: block for store in dict.fromkeys(map(member, dict.fromkeys(handle.kind for handle in held))) for block in getattr(bpy.data, store)}

    def found(handle: Handle[T]) -> BlenderResult[T]:
        return Option.of_optional(live.get(handle.uid)).filter(lambda block: isinstance(block, handle.kind)).to_result_with(lambda: (BlenderAbsentFault(owner=handle.kind.__name__, key=handle.uid),))

    return accumulated(map(found, held))


def embedded(block: bpy.types.ID) -> Option[bpy.types.NodeTree]:
    """Returns block's embedded node tree."""
    return _tree_member(block.bl_rna).bind(lambda member: Option.of_optional(getattr(block, member)))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


def holder(block: bpy.types.ID) -> Option[bpy.types.ID]:
    """Returns block itself, or `bpy.data` block embedding it as node tree."""
    stores = (prop.identifier for prop in bpy.types.BlendData.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty) and Option.of_optional(prop.fixed_type).bind(_tree_member).is_some())
    return Option.of_optional(next((owner for store in stores for owner in getattr(bpy.data, store) if embedded(owner) == Some(block)), None)) if block.is_embedded_data else Some(block)


def named[T: bpy.types.ID](block: T, name: str) -> BlenderResult[T]:
    """Renames block, collision fault when another block holds name."""

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


def vacant(title: str, taken: Iterable[str]) -> str:
    """Returns title, else its first numbered label no name in `taken` holds, so the host never suffixes a name it dedups."""
    held = frozenset(taken)
    return next(label for label in itertools.chain((title,), (f"{title} {ordinal}" for ordinal in itertools.count(2))) if label not in held)


def _tree_member(struct: bpy.types.Struct) -> Option[str]:
    """Returns struct's read-only node tree pointer identifier."""
    return Option.of_optional(
        next((prop.identifier for prop in struct.properties if isinstance(prop, bpy.types.PointerProperty) and prop.is_readonly and isinstance(prop.fixed_type, bpy.types.NodeTree)), None)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Handle", "blocks", "embedded", "holder", "member", "named", "resolved", "vacant"]
