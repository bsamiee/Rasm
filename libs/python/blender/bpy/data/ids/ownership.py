"""Ownership marks, data-block lifetimes, reuse, replacement, and import merging."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from enum import auto, Enum
from functools import partial
from typing import Final, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some, try_downcast
from expression.extra.result import catch
import idprop
import msgspec

from blender.bpy.data.ids.store import blocks, named
from blender.bpy.extension.events import Handler
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Outcome
from blender.records.identity import ContentHash

# --- [TYPES] ----------------------------------------------------------------------------


class Saved(Enum):
    """Lifetime of owned data in saved blend file."""

    NEVER = auto()
    USED = auto()
    ALWAYS = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Mark(msgspec.Struct, frozen=True, kw_only=True):
    """Ownership mark stored on holder."""

    key: Key
    of: Option[bpy.types.ID] = Nothing
    content: Option[ContentHash] = Nothing

    @classmethod
    def read(cls, identity: Identity, holder: bpy.types.ID | bpy.types.Node) -> Option[Self]:
        """Returns validated mark, Nothing when missing or invalid."""
        match holder.get(identity.manifest.id):
            case idprop.types.IDPropertyGroup() as group:
                return (
                    catch(msgspec.convert, exception=msgspec.ValidationError)(group.get("key"), Key)
                    .to_option()
                    .map(
                        lambda key: cls(
                            key=key,
                            of=Option.of_optional(try_downcast(bpy.types.ID, group.get("of"))),
                            content=catch(msgspec.convert, exception=msgspec.ValidationError)(group.get("content"), ContentHash).to_option(),
                        )
                    )
                )
            case _:
                return Nothing

    def write(self, identity: Identity, holder: bpy.types.ID | bpy.types.Node) -> Outcome[None]:
        """Writes present fields under manifest ID."""
        fields = dict((("key", self.key), *(("of", block) for block in self.of.to_list()), *(("content", content) for content in self.content.to_list())))
        return catch(holder.__setitem__, exception=TypeError)(identity.manifest.id, fields).map_error(partial(Fault.of, BlenderReason.REFUSED, identity.manifest.id))


class Ownership[T: bpy.types.ID](msgspec.Struct, frozen=True, kw_only=True):
    """Owned ID kind for one purpose."""

    kind: type[T]
    key: Key
    saved: Saved
    single: bool = True

    # --- [HOLDERS]

    def held(self, identity: Identity) -> tuple[tuple[T, Mark], ...]:
        """Returns local holders and marks in collection order, linked library blocks staying their library's."""
        return _held(identity, self.kind, frozenset((self.key,)))

    def marked(self, identity: Identity, holder: bpy.types.ID | bpy.types.Node) -> Option[Mark]:
        """Returns holder's mark, Nothing for another purpose."""
        return Mark.read(identity, holder).filter(lambda mark: mark.key == self.key)

    def found(self, identity: Identity, *, of: Option[bpy.types.ID] = Nothing, content: Option[ContentHash] = Nothing) -> Option[T]:
        """Returns earliest holder of source and content."""
        return Option.of_optional(_earliest(self.held(identity)).get(Mark(key=self.key, of=of, content=content)))

    def serving(self, identity: Identity, source: bpy.types.ID) -> tuple[T, ...]:
        """Returns holders marked for source."""
        return tuple(block for block, mark in self.held(identity) if mark.of == Some(source))

    def owned(
        self, identity: Identity, block: T, *, of: Option[bpy.types.ID] = Nothing, content: Option[ContentHash] = Nothing, filled: Callable[[T], Outcome[T]] = Ok, scope: Option[ExitStack] = Nothing
    ) -> Outcome[T]:
        """Marks and fills new block under `kept` rollback."""

        def configured() -> T:
            block.is_runtime_data = self.saved is Saved.NEVER
            block.use_fake_user = self.saved is Saved.ALWAYS
            return block

        return kept(Ok(block), lambda block: Mark(key=self.key, of=of, content=content).write(identity, block).map(lambda _: configured()).bind(filled), scope)

    def made(
        self,
        identity: Identity,
        build: Callable[[], Outcome[T]],
        *,
        of: Option[bpy.types.ID] = Nothing,
        content: Option[ContentHash] = Nothing,
        filled: Callable[[T], Outcome[T]] = Ok,
        scope: Option[ExitStack] = Nothing,
    ) -> Outcome[T]:
        """Reuses equal-mark holder of single row or owns built block, then fills it."""
        return (
            (self.found(identity, of=of, content=content) if self.single else Nothing)
            .map(filled)
            .default_with(lambda: build().bind(lambda block: self.owned(identity, block, of=of, content=content, filled=filled, scope=scope)))
        )

    def rebuilt(self, identity: Identity, held: T, build: Callable[[], Outcome[T]]) -> Outcome[T]:
        """Replaces marked block with built one under its name."""

        def swapped(block: T) -> Outcome[T]:
            name = held.name
            held.user_remap(block)
            return released((held,)).bind(lambda _: named(block, name))

        match Mark.read(identity, held):
            case Option(tag="some", some=mark):
                return build().bind(lambda block: self.owned(identity, block, of=mark.of, content=mark.content)).bind(swapped)
            case Option():
                return Error(Fault.of(BlenderReason.ABSENT, repr(held), member=identity.manifest.id))

    def removed(self, identity: Identity) -> Outcome[int]:
        """Removes zero-user holders of `USED` row, returning their count."""
        return released(tuple(block for block, _ in self.held(identity) if block.users == 0)) if self.saved is Saved.USED else Ok(0)

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the saved single row's place on its extension's import merge."""
        return Ok((_MERGES, (Band.APP_HANDLERS, _Merges.holding(identity, (self.kind, self.key)))) if self.single and self.saved is not Saved.NEVER else ())


# --- [SERVICES] -------------------------------------------------------------------------


class _Merges:
    """Saved single rows of one extension, imported duplicates of each remapped to its earliest local holder by one import handler."""

    __slots__ = ("rows",)

    def __init__(self) -> None:
        """Holds no row."""
        self.rows: list[tuple[type[bpy.types.ID], Key]] = []

    @classmethod
    @contextmanager
    def opened(cls, identity: Identity) -> Generator[Self]:
        """Holds a new merge with its import handler until scope exit."""
        merges = cls()
        with Handler(handlers=bpy.app.handlers.blend_import_post, body=partial(merges.merged, identity)).held(identity):
            yield merges

    @staticmethod
    @contextmanager
    def holding(identity: Identity, row: tuple[type[bpy.types.ID], Key]) -> Generator[None]:
        """Holds row on the extension's merge until scope exit."""
        merges = identity.held[_MERGES]
        with ExitStack() as stack:
            merges.rows.append(row)
            stack.callback(merges.rows.remove, row)
            yield

    def merged(self, identity: Identity, context: bpy.types.BlendImportContext) -> Outcome[None]:
        """Scans each held kind once, remapping imported duplicates and removing them in one batch."""
        imported = frozenset(item.id for item in context.import_items if item.id is not None and item.id.library is None)
        kinds = {kind: frozenset(key for held, key in self.rows if held is kind) for kind, _ in self.rows}
        duplicates = tuple(
            (block, keeper)
            for kind, keys in kinds.items()
            if any(isinstance(block, kind) for block in imported)
            for local in (_held(identity, kind, keys),)
            for keepers in (_earliest(local),)
            for block, mark in local
            if block in imported and block != (keeper := keepers[mark])
        )
        for block, keeper in duplicates:
            block.user_remap(keeper)
        return released(tuple(block for block, _ in duplicates)).map(lambda _: None)


_MERGES: Final = Resource[_Merges](made=_Merges.opened)

# --- [OPERATIONS] -----------------------------------------------------------------------


def retained[T](result: Outcome[T], rollback: ExitStack, scope: Option[ExitStack] = Nothing) -> Outcome[T]:
    """Moves rollback callbacks into scope on success, dropping them without scope."""
    for _ in result.to_option().to_list():
        held = rollback.pop_all()
        for outer in scope.to_list():
            outer.enter_context(held)
    return result


def kept[B: bpy.types.ID, T](made: Outcome[B], fill: Callable[[B], Outcome[T]], scope: Option[ExitStack] = Nothing) -> Outcome[T]:
    """Fills new data-block, removing it when fill fails or when given scope unwinds."""

    def filled(block: B) -> Outcome[T]:
        with ExitStack() as rollback:
            rollback.callback(bpy.data.batch_remove, ids=(block,))
            return retained(fill(block), rollback, scope)

    return made.bind(filled)


def released(blocks: tuple[bpy.types.ID, ...]) -> Outcome[int]:
    """Removes blocks in one `batch_remove`, returning their count."""
    return (catch(bpy.data.batch_remove, exception=RuntimeError)(ids=blocks) if blocks else Ok(None)).map(lambda _: len(blocks)).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(blocks)))


def _held[T: bpy.types.ID](identity: Identity, kind: type[T], keys: frozenset[Key]) -> tuple[tuple[T, Mark], ...]:
    """Returns local blocks of kind with their marks under keys, in collection order."""
    return tuple((block, mark) for block in blocks(bpy.data, kind) if block.library is None for mark in Mark.read(identity, block).filter(lambda mark: mark.key in keys).to_list())


def _earliest[T: bpy.types.ID](pairs: Iterable[tuple[T, Mark]]) -> Mapping[Mark, T]:
    """Returns holder with least session UID per mark."""
    return {mark: block for block, mark in sorted(pairs, key=lambda pair: pair[0].session_uid, reverse=True)}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Mark", "Ownership", "Saved", "kept", "released", "retained"]
