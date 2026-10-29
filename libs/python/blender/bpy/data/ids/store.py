"""Session handles, BlendData collections, and collision-checked renames."""

from collections.abc import Iterable, Mapping
from functools import partial
from types import MappingProxyType
from typing import Final, Self

import bpy
from expression import Error, Ok, Option, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.records.faults import accumulated, Fault, Outcome, Reason

# --- [CONSTANTS] ------------------------------------------------------------------------

STORES: Final[Mapping[type, str]] = MappingProxyType({type(prop.fixed_type): prop.identifier for prop in bpy.types.BlendData.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty)})
_TREES: Final[Mapping[type, str]] = MappingProxyType({
    type(fixed): tree.identifier
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
    """Session handle of original ID, an embedded node tree held through the session of the ID embedding it."""

    kind: type[T]
    uid: int
    embedded: bool = False

    @classmethod
    def of(cls, block: T) -> Self:
        """Returns handle of block's original, an embedded tree naming its holder."""
        held: T = block.original  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return cls(kind=type(block), uid=holder(held).default_value(held).session_uid, embedded=held.is_embedded_data)

    def resolve(self) -> Outcome[T]:
        """Returns live original, absent fault once expired."""
        return resolved((self,)).map(lambda found: found[0])


# --- [OPERATIONS] -----------------------------------------------------------------------


def member(kind: type[bpy.types.ID]) -> str:
    """Returns BlendData collection name for kind's nearest MRO type."""
    return Block.of_seq(kind.__mro__).choose(lambda parent: Option.of_optional(STORES.get(parent))).head()


def blocks[T: bpy.types.ID](data: bpy.types.BlendData, kind: type[T]) -> tuple[T, ...]:
    """Returns blocks of kind in collection order."""
    return tuple(block for block in getattr(data, member(kind)) if isinstance(block, kind))


def resolved[T: bpy.types.ID](handles: Iterable[Handle[T]]) -> Outcome[tuple[T, ...]]:
    """Returns live originals in order, accumulating expired-handle faults."""
    live = {block.session_uid: block for block in bpy.data.all_ids}

    def reached(handle: Handle[T]) -> Option[bpy.types.ID]:
        found: Option[bpy.types.ID] = Option.of_optional(live.get(handle.uid))
        return found.bind(embedded) if handle.embedded else found

    return accumulated(
        reached(handle).bind(lambda block: Option.of_optional(try_downcast(handle.kind, block))).to_result_with(partial(Fault.of, Reason.ABSENT, handle.kind.__name__, member=str(handle.uid)))
        for handle in handles
    )


def embedded(block: bpy.types.ID) -> Option[bpy.types.NodeTree]:
    """Returns block's embedded node tree."""
    return Block.of_seq(type(block).__mro__).choose(lambda parent: Option.of_optional(_TREES.get(parent))).try_head().bind(lambda tree: Option.of_optional(getattr(block, tree)))


def holder(block: bpy.types.ID) -> Option[bpy.types.ID]:
    """Returns block itself, or `bpy.data` block embedding it as node tree."""
    return Block.of_seq(bpy.data.all_ids).filter(lambda owner: embedded(owner) == Some(block)).try_head() if block.is_embedded_data else Some(block)


def named[T: bpy.types.ID](block: T, name: str) -> Outcome[T]:
    """Renames block, collision fault when another block holds name, the prior name restored where the host suffixed this one."""
    prior, refused = block.name, partial(Fault.of, Reason.REFUSED, repr(block))
    taken: Outcome[T] = Error(Fault.of(Reason.TAKEN, name, member=member(type(block))))

    def answered(result: str) -> Outcome[T]:
        match result:
            case "RENAMED_COLLISION_ADJUSTED":
                return catch(block.rename, exception=RuntimeError)(prior, mode="NEVER").map_error(refused).bind(lambda _: taken)
            case "UNCHANGED_COLLISION":
                return taken
            case _:
                return Ok(block)

    return catch(block.rename, exception=RuntimeError)(name, mode="NEVER").map_error(refused).bind(answered)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["STORES", "Handle", "IdName", "blocks", "embedded", "holder", "member", "named", "resolved"]
