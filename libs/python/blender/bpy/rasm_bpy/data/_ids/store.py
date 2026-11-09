"""Session handles, BlendData collections, and collision-checked renames."""

from collections.abc import Iterable, Mapping, MutableMapping
from functools import partial
from types import MappingProxyType
from typing import Final, Self

import bpy
from expression import Option, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from rasm_records import accumulated, Error, Fault, Nothing, Ok, Outcome, Reason

# --- [CONSTANTS] ------------------------------------------------------------------------

_STORES: Final[Mapping[type, str]] = MappingProxyType({type(prop.fixed_type): prop.identifier for prop in bpy.types.BlendData.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty)})
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
        return Option.of_optional(try_downcast(kind, getattr(bpy.data, _member(kind)).get((self.name, self.library))))


class Handle[T: bpy.types.ID](msgspec.Struct, frozen=True, kw_only=True):
    """Session handle of original ID, an embedded node tree held through the session of the ID embedding it."""

    kind: type[T]
    uid: int
    embedded: bool = False

    @classmethod
    def of(cls, block: T) -> Self:
        """Returns handle of block's original, an embedded tree naming its holder."""
        held: T = block.original  # ty: ignore[invalid-assignment]
        return cls(kind=type(block), uid=holder(held).default_value(held).session_uid, embedded=held.is_embedded_data)

    def resolve(self: "Handle[T]") -> Outcome[T]:
        """Returns live original, absent fault once expired."""
        return _resolved((self,)).map(lambda found: found[0])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _member(kind: type[bpy.types.ID]) -> str:
    """Returns BlendData collection name for kind's nearest MRO type."""
    return Block.of_seq(kind.__mro__).choose(lambda parent: Option.of_optional(_STORES.get(parent))).head()


def _blocks[T: bpy.types.ID](kind: type[T]) -> tuple[T, ...]:
    """Returns blocks of kind in collection order."""
    return tuple(block for block in getattr(bpy.data, _member(kind)) if isinstance(block, kind))


def _resolved[T: bpy.types.ID](handles: Iterable[Handle[T]]) -> Outcome[tuple[T, ...]]:
    """Returns live originals in order, accumulating expired-handle faults."""
    held = tuple(handles)
    live = _joined(held, bpy.data.all_ids)
    return accumulated(Option.of_optional(live.get(handle)).to_result_with(partial(Fault.of, Reason.ABSENT, handle.kind.__name__, member=str(handle.uid))) for handle in held)


def retained[T: bpy.types.ID, V](values: MutableMapping[Handle[T], V], originals: Iterable[bpy.types.ID], /) -> dict[Handle[T], T]:
    """Removes absent entries and returns their live originals from one native ID join."""
    live = _joined(values, originals)
    for handle in values.keys() - live.keys():
        del values[handle]
    return live


def _embedded(block: bpy.types.ID) -> Option[bpy.types.NodeTree]:
    """Returns block's embedded node tree."""
    for member in Block.of_seq(type(block).__mro__).choose(lambda parent: Option.of_optional(_TREES.get(parent))).try_head().to_list():
        tree: bpy.types.NodeTree | None = getattr(block, member)  # ty: ignore[unsound-assignment]
        return Option.of_optional(tree)
    return Nothing


def holder(block: bpy.types.ID) -> Option[bpy.types.ID]:
    """Returns block itself, or `bpy.data` block embedding it as node tree."""
    return Block.of_seq(bpy.data.all_ids).filter(lambda owner: _embedded(owner) == Some(block)).try_head() if block.is_embedded_data else Some(block)


def _named[T: bpy.types.ID](block: T, name: str) -> Outcome[T]:
    """Renames block, taken fault when another block holds name, the prior name restored where the host suffixed this one."""
    prior, refused = block.name, partial(Fault.of, Reason.REFUSED, repr(block))
    taken: Outcome[T] = Error(Fault.of(Reason.TAKEN, name, member=_member(type(block))))

    def answered(result: str) -> Outcome[T]:
        match result:
            case "RENAMED_COLLISION_ADJUSTED":
                return catch(block.rename, exception=RuntimeError)(prior, mode="NEVER").map_error(refused).bind(lambda _: taken)
            case "UNCHANGED_COLLISION":
                return taken
            case _:
                return Ok(block)

    return catch(block.rename, exception=RuntimeError)(name, mode="NEVER").map_error(refused).bind(answered)


def _joined[T: bpy.types.ID](handles: Iterable[Handle[T]], originals: Iterable[bpy.types.ID]) -> dict[Handle[T], T]:
    """Joins handles to current originals through one native ID index."""
    held = tuple(handles)
    live = {block.session_uid: block for block in originals} if held else {}

    def reached(handle: Handle[T]) -> Option[bpy.types.ID]:
        found: Option[bpy.types.ID] = Option.of_optional(live.get(handle.uid))
        return found.bind(_embedded) if handle.embedded else found

    return {handle: block for handle in held for block in reached(handle).bind(lambda block: Option.of_optional(try_downcast(handle.kind, block))).to_list()}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Handle", "IdName", "holder", "retained"]
