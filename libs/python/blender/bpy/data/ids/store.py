"""Session handles, BlendData collections, and collision-checked renames."""

from collections.abc import Iterable, Mapping
from functools import partial
from types import MappingProxyType
from typing import Final, Self

import bpy
from expression import Error, Option, Some, try_downcast
from expression.collections import Block
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
        return Option.of_optional(try_downcast(kind, getattr(bpy.data, member(kind)).get((self.name, self.library))))


class Handle[T: bpy.types.ID](msgspec.Struct, frozen=True, kw_only=True):
    """Session handle of original ID."""

    kind: type[T]
    uid: int

    @classmethod
    def of(cls, block: T) -> Self:
        """Returns handle of block's original."""
        return cls(kind=type(block), uid=original(block).session_uid)

    def resolve(self) -> Outcome[T]:
        """Returns live original, absent fault once expired."""
        return resolved((self,)).map(lambda found: found[0])


# --- [OPERATIONS] -----------------------------------------------------------------------


def original[T: bpy.types.ID](block: T) -> T:
    """Returns the `bpy.data` block an evaluated copy came from, an original answering itself."""
    held: T = block.original  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return held


def member(kind: type[bpy.types.ID]) -> str:
    """Returns BlendData collection name for kind's nearest MRO type."""
    return Block.of_seq(kind.__mro__).choose(lambda parent: Option.of_optional(STORES.get(parent))).head()


def blocks[T: bpy.types.ID](data: bpy.types.BlendData, kind: type[T]) -> tuple[T, ...]:
    """Returns blocks of kind in collection order."""
    return tuple(block for block in getattr(data, member(kind)) if isinstance(block, kind))


def resolved[T: bpy.types.ID](handles: Iterable[Handle[T]]) -> Outcome[tuple[T, ...]]:
    """Returns live originals in order, accumulating expired-handle faults."""
    held = tuple(handles)
    kinds = tuple(dict.fromkeys(handle.kind for handle in held))
    live = {block.session_uid: block for block in bpy.data.all_ids if isinstance(block, kinds)}
    return accumulated(
        Option.of_optional(try_downcast(handle.kind, live.get(handle.uid))).to_result_with(partial(Fault.of, BlenderReason.ABSENT, handle.kind.__name__, member=str(handle.uid))) for handle in held
    )


def embedded(block: bpy.types.ID) -> Option[bpy.types.NodeTree]:
    """Returns block's embedded node tree."""
    return Option.of_optional(_TREES.get(member(type(block)))).bind(lambda tree: Option.of_optional(getattr(block, tree)))


def holder(block: bpy.types.ID) -> Option[bpy.types.ID]:
    """Returns block itself, or `bpy.data` block embedding it as node tree."""
    return Block.of_seq(bpy.data.all_ids).filter(lambda owner: embedded(owner) == Some(block)).try_head() if block.is_embedded_data else Some(block)


def named[T: bpy.types.ID](block: T, name: str) -> Outcome[T]:
    """Renames block, collision fault when another block holds name."""
    return (
        IdName(name=name, library=IdName.of(block).library)
        .resolve(type(block))
        .filter(lambda holding: holding != block)
        .map(lambda holding: Error(Fault.of(BlenderReason.TAKEN, name, member=repr(holding))))
        .default_with(lambda: catch(block.rename, exception=RuntimeError)(name, mode="NEVER").map(lambda _: block).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(block))))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["STORES", "Handle", "IdName", "blocks", "embedded", "holder", "member", "named", "original", "resolved"]
