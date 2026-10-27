"""Session handles, BlendData collections, and collision-checked renames."""

from collections.abc import Iterable, Mapping
from types import MappingProxyType
from typing import Final, Self

import bpy
from expression import Error, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Outcome

# --- [CONSTANTS] ------------------------------------------------------------------------

STORES: Final[Mapping[type, str]] = MappingProxyType({type(prop.fixed_type): prop.identifier for prop in bpy.types.BlendData.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty)})
_TREES: Final = MappingProxyType({
    prop.identifier: tree.identifier
    for prop in bpy.types.BlendData.bl_rna.properties
    if isinstance(prop, bpy.types.CollectionProperty)
    for fixed in Option.of_optional(prop.fixed_type).to_list()
    for tree in fixed.properties
    if isinstance(tree, bpy.types.PointerProperty) and tree.is_readonly and isinstance(tree.fixed_type, bpy.types.NodeTree)
})

# --- [MODELS] ---------------------------------------------------------------------------


class IdName(msgspec.Struct, frozen=True, kw_only=True):
    """ID name in local data or linked library."""

    name: str
    library: str | None = None

    @classmethod
    def of(cls, block: bpy.types.ID) -> Self:
        """Returns block name with library path when linked."""
        return cls(name=block.name, library=Option.of_optional(block.library).map(lambda library: library.filepath).to_optional())

    def resolve[T: bpy.types.ID](self, kind: type[T]) -> Option[T]:
        """Returns the block of kind under this name and library through the collection's key lookup.

        Args:
            kind: ID class whose `bpy.data` collection holds the block.

        Returns:
            Block of kind, Nothing when absent or of another class.
        """
        return Option.of_optional(getattr(bpy.data, member(kind)).get((self.name, self.library))).filter(lambda block: isinstance(block, kind))


class Handle[T: bpy.types.ID](msgspec.Struct, frozen=True, kw_only=True):
    """Session handle of original ID."""

    kind: type[T]
    uid: int

    @classmethod
    def of(cls, block: T) -> Self:
        """Returns handle of block's original."""
        original: bpy.types.ID = block.original  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return cls(kind=type(block), uid=original.session_uid)

    def resolve(self) -> Outcome[T]:
        """Returns live original, absent fault once expired."""
        return resolved((self,)).map(lambda found: found[0])


# --- [OPERATIONS] -----------------------------------------------------------------------


def member(kind: type[bpy.types.ID]) -> str:
    """Returns BlendData collection name for kind's nearest MRO type."""
    return next(STORES[parent] for parent in kind.__mro__ if parent in STORES)


def blocks[T: bpy.types.ID](data: bpy.types.BlendData, kind: type[T]) -> tuple[T, ...]:
    """Returns blocks of kind in collection order."""
    return tuple(block for block in getattr(data, member(kind)) if isinstance(block, kind))


def resolved[T: bpy.types.ID](handles: Iterable[Handle[T]]) -> Outcome[tuple[T, ...]]:
    """Returns live originals in order, accumulating expired-handle faults."""
    held = tuple(handles)
    live = {block.session_uid: block for store in dict.fromkeys(member(handle.kind) for handle in held) for block in getattr(bpy.data, store)}
    return accumulated(
        Option
        .of_optional(live.get(handle.uid))
        .filter(lambda block: isinstance(block, handle.kind))
        .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=handle.kind.__name__, member=str(handle.uid)),))
        for handle in held
    )


def embedded(block: bpy.types.ID) -> Option[bpy.types.NodeTree]:
    """Returns block's embedded node tree."""
    return Option.of_optional(_TREES.get(member(type(block)))).bind(lambda tree: Option.of_optional(getattr(block, tree)))


def holder(block: bpy.types.ID) -> Option[bpy.types.ID]:
    """Returns block itself, or `bpy.data` block embedding it as node tree."""
    return Option.of_optional(next((owner for store, tree in _TREES.items() for owner in getattr(bpy.data, store) if getattr(owner, tree) == block), None)) if block.is_embedded_data else Some(block)


def named[T: bpy.types.ID](block: T, name: str) -> Outcome[T]:
    """Renames block, collision fault when another block holds name."""
    return (
        IdName(name=name, library=IdName.of(block).library)
        .resolve(type(block))
        .filter(lambda holding: holding != block)
        .map(lambda holding: Error((Fault(reason=BlenderReason.TAKEN, subject=name, member=repr(holding)),)))
        .default_with(
            lambda: (
                catch(block.rename, exception=RuntimeError)(name, mode="NEVER").map(lambda _: block).map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(block), cause=error),))
            )
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["STORES", "Handle", "IdName", "blocks", "embedded", "holder", "member", "named", "resolved"]
