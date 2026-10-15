"""Ownership marks, data-block lifetimes, reuse, replacement, and import merging."""

from collections.abc import Callable, Iterable
from contextlib import ExitStack
from enum import auto, Enum
from types import MappingProxyType
from typing import Literal, overload, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import idprop
import msgspec

from blender.bpy.data.ids.store import blocks, named
from blender.bpy.extension.events.handlers import ImportHandler
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderResult
from blender.imaging.hashing import ContentHash

# --- [TYPES] ----------------------------------------------------------------------------


class Saved(Enum):
    """Lifetime of owned data in a saved blend file."""

    NEVER = auto()
    USED = auto()
    ALWAYS = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Mark(msgspec.Struct, frozen=True, kw_only=True):
    """Owned purpose, optional source data-block, and optional content identity."""

    key: Key
    of: Option[bpy.types.ID] = Nothing
    content: Option[ContentHash] = Nothing

    @classmethod
    def read(cls, identity: Identity, holder: bpy.types.ID | bpy.types.Node) -> Option[Self]:
        """Returns the admitted mark, or Nothing when a user removed or retyped its key."""
        match holder.get(identity.manifest.id):
            case idprop.types.IDPropertyGroup() as group:
                return (
                    catch(lambda: msgspec.convert(group.get("key"), Key), exception=msgspec.ValidationError)()
                    .to_option()
                    .map(
                        lambda key: cls(
                            key=key,
                            of=Option.of_optional(group.get("of")).filter(lambda value: isinstance(value, bpy.types.ID)),
                            content=catch(lambda: msgspec.convert(group.get("content"), ContentHash), exception=msgspec.ValidationError)().to_option(),
                        )
                    )
                )
            case _:
                return Nothing

    def write(self, identity: Identity, holder: bpy.types.ID | bpy.types.Node) -> BlenderResult[None]:
        """Writes the mark's present fields under its extension ID, preserving native property refusals."""
        try:
            holder[identity.manifest.id] = {name: value for name, held in msgspec.structs.asdict(self).items() for value in (held.to_list() if isinstance(held, Option) else (held,))}
        except (AttributeError, KeyError, TypeError, ValueError) as error:
            return Error((BlenderEditFault(request=identity.manifest.id, error=error),))
        return Ok(None)


class Ownership[T: bpy.types.ID](msgspec.Struct, frozen=True, kw_only=True):
    """Owned data kind, purpose, saved lifetime, and per-mark multiplicity."""

    kind: type[T]
    key: Key
    saved: Saved
    single: bool = True

    # --- [HOLDERS]

    def held(self, identity: Identity) -> tuple[tuple[T, Mark], ...]:
        """Returns matching data-blocks and their admitted marks in native collection order."""
        return tuple((block, mark) for block in blocks(bpy.data, self.kind) for mark in self.marked(identity, block).to_list())

    def marked(self, identity: Identity, holder: bpy.types.ID | bpy.types.Node) -> Option[Mark]:
        """Returns the holder's admitted mark when it carries this purpose."""
        return Mark.read(identity, holder).filter(lambda mark: mark.key == self.key)

    def owned(self, identity: Identity, block: T, mark: Mark, filled: Callable[[T], BlenderResult[T]] = Ok) -> BlenderResult[T]:
        """Sets a new block's mark and lifetime, then fills it, removing the block on any refusal."""
        with ExitStack() as rollback:
            rollback.callback(bpy.data.batch_remove, ids=(block,))

            def configured() -> T:
                """Sets native saved state."""
                block.is_runtime_data = self.saved is Saved.NEVER
                block.use_fake_user = self.saved is Saved.ALWAYS
                return block

            result = (
                mark.write(identity, block).bind(lambda _: catch(configured, exception=AttributeError)().map_error(lambda error: (BlenderEditFault(request=repr(block), error=error),))).bind(filled)
            )
            for _ in result.to_option().to_list():
                rollback.pop_all()
            return result

    @overload
    def made(
        self,
        identity: Identity,
        build: Callable[[], BlenderResult[T]],
        *,
        of: Option[bpy.types.ID] = Nothing,
        content: Option[ContentHash] = Nothing,
        optional: Literal[False] = False,
        filled: Callable[[T], BlenderResult[T]] = Ok,
    ) -> BlenderResult[T]: ...

    @overload
    def made(
        self,
        identity: Identity,
        build: Callable[[], BlenderResult[Option[T]]],
        *,
        of: Option[bpy.types.ID] = Nothing,
        content: Option[ContentHash] = Nothing,
        optional: Literal[True],
        filled: Callable[[T], BlenderResult[T]] = Ok,
    ) -> BlenderResult[Option[T]]: ...

    def made(
        self,
        identity: Identity,
        build: Callable[[], BlenderResult[T | Option[T]]],
        *,
        of: Option[bpy.types.ID] = Nothing,
        content: Option[ContentHash] = Nothing,
        optional: bool = False,
        filled: Callable[[T], BlenderResult[T]] = Ok,
    ) -> BlenderResult[T | Option[T]]:
        """Reuses equal content before building, fills the reused or built block, removes a built block its fill refuses, and retains optional absence without an ID."""
        mark = Mark(key=self.key, of=of, content=content)

        def shaped(block: T) -> T | Option[T]:
            return Some(block) if optional else block

        return (
            (Option.of_optional(earliest(self.held(identity)).get(mark)) if self.single else Nothing)
            .map(lambda block: filled(block).map(shaped))
            .default_with(
                lambda: build().bind(
                    lambda block: (block if isinstance(block, Option) else Some(block)).map(lambda block: self.owned(identity, block, mark, filled).map(shaped)).default_value(Ok(Nothing))
                )
            )
        )

    def rebuilt(self, identity: Identity, held: T, build: Callable[[], BlenderResult[T]]) -> BlenderResult[T]:
        """Returns the replacement, or an absent mark, creation, edit, or collision fault."""

        def swapped(block: T) -> BlenderResult[T]:
            """Remaps native users before removing the predecessor and restoring its name."""
            name = held.name
            held.user_remap(block)
            bpy.data.batch_remove(ids=(held,))
            return named(block, name)

        match Mark.read(identity, held):
            case Option(tag="some", some=mark):
                return build().bind(lambda block: self.owned(identity, block, mark)).bind(swapped)
            case Option():
                return Error((BlenderAbsentFault(owner=repr(held), key=identity.manifest.id),))

    def removed(self, identity: Identity) -> int:
        """Returns the number of unused USED holders removed, or zero for other lifetimes."""
        match self.saved:
            case Saved.USED:
                if unused := tuple(block for block, _ in self.held(identity) if block.users == 0):
                    bpy.data.batch_remove(ids=unused)
                return len(unused)
            case Saved.NEVER | Saved.ALWAYS:
                return 0

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Returns the import merger acquisition for a saved single row, or no acquisition."""

        def merged(context: bpy.types.BlendImportContext) -> BlenderResult[None]:
            """Remaps imported local duplicates to the earliest local equal holder."""
            imported = frozenset(item.id for item in context.import_items if isinstance(item.id, self.kind) and item.id.library is None)
            local = tuple((block, mark) for block, mark in self.held(identity) if block.library is None)
            keepers = earliest(local)
            twins = tuple((block, kept) for block, mark in local if block in imported and block != (kept := keepers[mark]))
            for block, kept in twins:
                block.user_remap(kept)
            if twins:
                bpy.data.batch_remove(ids=tuple(block for block, _ in twins))
            return Ok(None)

        return (
            ImportHandler(
                handlers=bpy.app.handlers.blend_import_post,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                body=merged,
            ).acquisitions(identity)
            if self.single and self.saved is not Saved.NEVER
            else Ok(())
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def earliest[T: bpy.types.ID](pairs: Iterable[tuple[T, Mark]]) -> MappingProxyType[Mark, T]:
    """Returns each equal mark's holder with the least session UID in one pass."""
    held: dict[Mark, T] = {}
    for block, mark in pairs:
        held[mark] = min(held.get(mark, block), block, key=lambda value: value.session_uid)
    return MappingProxyType(held)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Mark", "Ownership", "Saved", "earliest"]
