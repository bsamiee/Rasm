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
        fields = {name: value for name, held in msgspec.structs.asdict(self).items() for value in (held.to_list() if isinstance(held, Option) else (held,))}
        return BlenderEditFault.attempted(identity.manifest.id, lambda: holder.__setitem__(identity.manifest.id, fields))


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

    def found(self, identity: Identity, *, of: Option[bpy.types.ID] = Nothing, content: Option[ContentHash] = Nothing) -> Option[T]:
        """Returns the earliest holder of this purpose for a source and content, absent when none holds it."""
        return Option.of_optional(_earliest(self.held(identity)).get(Mark(key=self.key, of=of, content=content)))

    def serving(self, identity: Identity, source: bpy.types.ID) -> tuple[T, ...]:
        """Returns the holders whose mark names a source, in native collection order."""
        return tuple(block for block, mark in self.held(identity) if mark.of == Some(source))

    def owned(
        self,
        identity: Identity,
        block: T,
        *,
        of: Option[bpy.types.ID] = Nothing,
        content: Option[ContentHash] = Nothing,
        filled: Callable[[T], BlenderResult[T]] = Ok,
        scope: Option[ExitStack] = Nothing,
    ) -> BlenderResult[T]:
        """Sets the mark and lifetime of a new block, then fills it, the block removed as `kept` removes it."""

        def configured() -> T:
            """Sets native saved state."""
            block.is_runtime_data = self.saved is Saved.NEVER
            block.use_fake_user = self.saved is Saved.ALWAYS
            return block

        return kept(Ok(block), lambda block: Mark(key=self.key, of=of, content=content).write(identity, block).bind(lambda _: BlenderEditFault.attempted(repr(block), configured)).bind(filled), scope)

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
        scope: Option[ExitStack] = Nothing,
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
        scope: Option[ExitStack] = Nothing,
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
        scope: Option[ExitStack] = Nothing,
    ) -> BlenderResult[T | Option[T]]:
        """Reuses equal content before building, fills the reused or built block, and retains optional absence, a built block removed as `owned` removes it."""

        def shaped(block: T) -> T | Option[T]:
            return Some(block) if optional else block

        return (
            (self.found(identity, of=of, content=content) if self.single else Nothing)
            .map(lambda block: filled(block).map(shaped))
            .default_with(
                lambda: build().bind(
                    lambda block: (
                        (block if isinstance(block, Option) else Some(block))
                        .map(lambda block: self.owned(identity, block, of=of, content=content, filled=filled, scope=scope).map(shaped))
                        .default_value(Ok(Nothing))
                    )
                )
            )
        )

    def rebuilt(self, identity: Identity, held: T, build: Callable[[], BlenderResult[T]]) -> BlenderResult[T]:
        """Returns the replacement, or an absent mark, creation, edit, or collision fault."""

        def swapped(block: T) -> BlenderResult[T]:
            """Remaps native users before removing the predecessor and restoring its name."""
            name = held.name
            held.user_remap(block)
            return released((held,)).bind(lambda _: named(block, name))

        match Mark.read(identity, held):
            case Option(tag="some", some=mark):
                return build().bind(lambda block: self.owned(identity, block, of=mark.of, content=mark.content)).bind(swapped)
            case Option():
                return Error((BlenderAbsentFault(owner=repr(held), key=identity.manifest.id),))

    def removed(self, identity: Identity) -> BlenderResult[int]:
        """Removes the unused holders of a USED row, answering their count or the refused removal, zero for other lifetimes."""
        return released(tuple(block for block, _ in self.held(identity) if block.users == 0)) if self.saved is Saved.USED else Ok(0)

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Returns the import merger acquisition for a saved single row, or no acquisition."""

        def merged(context: bpy.types.BlendImportContext) -> BlenderResult[None]:
            """Remaps imported local duplicates to the earliest local equal holder."""
            imported = frozenset(item.id for item in context.import_items if isinstance(item.id, self.kind) and item.id.library is None)
            local = tuple((block, mark) for block, mark in self.held(identity) if block.library is None)
            keepers = _earliest(local)
            twins = tuple((block, keeper) for block, mark in local if block in imported and block != (keeper := keepers[mark]))
            for block, keeper in twins:
                block.user_remap(keeper)
            return released(tuple(block for block, _ in twins)).map(lambda _: None)

        return (
            ImportHandler(
                handlers=bpy.app.handlers.blend_import_post,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                body=merged,
            ).acquisitions(identity)
            if self.single and self.saved is not Saved.NEVER
            else Ok(())
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def committed[T](result: BlenderResult[T], rollback: ExitStack, scope: Option[ExitStack] = Nothing) -> BlenderResult[T]:
    """Answers a result unchanged, keeping the acquisitions its rollback holds once it succeeds, for good or until a given outer scope unwinds."""
    for _ in result.to_option().to_list():
        held = rollback.pop_all()
        for outer in scope.to_list():
            outer.enter_context(held)
    return result


def kept[B: bpy.types.ID, T](made: BlenderResult[B], fill: Callable[[B], BlenderResult[T]], scope: Option[ExitStack] = Nothing) -> BlenderResult[T]:
    """Fills a new data-block, removing it when the fill refuses or, given a scope, when that scope unwinds."""

    def filled(block: B) -> BlenderResult[T]:
        with ExitStack() as rollback:
            rollback.callback(bpy.data.batch_remove, ids=(block,))
            return committed(fill(block), rollback, scope)

    return made.bind(filled)


def released(blocks: tuple[bpy.types.ID, ...]) -> BlenderResult[int]:
    """Removes data-blocks in one `batch_remove`, answering their count or the refused removal."""
    return BlenderEditFault.attempted(repr(blocks), lambda: bpy.data.batch_remove(ids=blocks) if blocks else None).map(lambda _: len(blocks))


def _earliest[T: bpy.types.ID](pairs: Iterable[tuple[T, Mark]]) -> MappingProxyType[Mark, T]:
    """Returns each equal mark's holder with the least session UID in one pass."""
    held: dict[Mark, T] = {}
    for block, mark in pairs:
        held[mark] = min(held.get(mark, block), block, key=lambda value: value.session_uid)
    return MappingProxyType(held)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Mark", "Ownership", "Saved", "committed", "kept", "released"]
