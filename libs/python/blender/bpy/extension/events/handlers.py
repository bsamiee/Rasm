"""Persistent callbacks on `bpy.app.handlers` lists."""

from abc import abstractmethod
from collections.abc import Callable, Generator, Iterator
from contextlib import contextmanager
from enum import auto, Flag
import inspect
from pathlib import Path
from types import MethodType
from typing import Final, override, Protocol

import bpy
from expression import Nothing, Ok, Option
import msgspec

from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Band
from blender.records.faults import Outcome

# --- [TYPES] ----------------------------------------------------------------------------


class Change(Flag):
    """Change kinds depsgraph update reports."""

    GEOMETRY = auto()
    TRANSFORM = auto()
    SHADING = auto()


type Callback = Callable[[*tuple[object, ...]], None]


class Handlers[E](Protocol):
    """Handler list of one callback shape, wider callback unions included."""

    def append(self, entry: E, /) -> None:
        """Appends callback."""

    def remove(self, entry: E, /) -> None:
        """Removes callback."""

    def __iter__(self) -> Iterator[object]:
        """Iterates held callbacks."""


# --- [CONSTANTS] ------------------------------------------------------------------------

LOAD_PRE: Final[Handlers[Callable[[str, None], None]]] = bpy.app.handlers.load_pre  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
LOAD_POST: Final[Handlers[Callable[[str, None], None]]] = bpy.app.handlers.load_post  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
SAVE_PRE: Final[Handlers[Callable[[str, None], None]]] = bpy.app.handlers.save_pre  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
SAVE_POST: Final[Handlers[Callable[[str, None], None]]] = bpy.app.handlers.save_post  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

# --- [MODELS] ---------------------------------------------------------------------------


class Handler[E](msgspec.Struct, frozen=True, kw_only=True):
    """Callback row on one handler list, held on the extension's dispatcher."""

    handlers: Handlers[E]

    @abstractmethod
    def entry(self, identity: Identity, /) -> E:
        """Builds the callback the dispatcher holds.

        Args:
            identity: Extension holding reports and runtime scene mark.
        """

    def acquisitions(self, identity: Identity, /) -> Outcome[tuple[tuple[Band, Acquisition], ...]]:
        """Builds the app-handler acquisition.

        Args:
            identity: Extension owning the dispatcher.

        Returns:
            Ok of one app-handler acquisition.
        """
        return Ok(((Band.APP_HANDLERS, appended(identity, self.handlers, self.entry(identity))),))


class SceneHandler(Handler[Callable[[bpy.types.Scene], None]], frozen=True, kw_only=True):
    """Scene callback skipping extension's runtime scenes."""

    body: Callable[[bpy.types.Scene], Outcome[None]]

    @override
    def entry(self, identity: Identity, /) -> Callable[[bpy.types.Scene], None]:
        body = identity.reports.reported(self.body)
        return lambda scene: None if staged(identity, scene) else body(scene).to_optional()


class FrameHandler(Handler[Callable[[bpy.types.Scene, bpy.types.Depsgraph], None]], frozen=True, kw_only=True):
    """Frame callback receiving evaluated depsgraph with no update filter, skipping extension's runtime scenes."""

    body: Callable[[bpy.types.Scene, bpy.types.Depsgraph], Outcome[None]]

    @override
    def entry(self, identity: Identity, /) -> Callable[[bpy.types.Scene, bpy.types.Depsgraph], None]:
        body = identity.reports.reported(self.body)
        return lambda scene, depsgraph: None if staged(identity, scene) else body(scene, depsgraph).to_optional()


class PathHandler(Handler[Callable[[str, None], None]], frozen=True, kw_only=True):
    """Load or save callback receiving Nothing for startup file."""

    body: Callable[[Option[Path]], Outcome[None]]

    @override
    def entry(self, identity: Identity, /) -> Callable[[str, None], None]:
        body = identity.reports.reported(self.body)
        return lambda path, _: body(Option.of_optional(path or None).map(Path)).to_optional()


class UpdateHandler(Handler[Callable[[bpy.types.Scene, bpy.types.Depsgraph], None]], frozen=True, kw_only=True):
    """Scene callback receiving updates of its ID types and optional change kinds."""

    ids: tuple[type[bpy.types.ID], ...]
    body: Callable[[bpy.types.Scene, tuple[bpy.types.DepsgraphUpdate, ...]], Outcome[None]]
    changes: Option[Change] = Nothing

    @override
    def entry(self, identity: Identity, /) -> Callable[[bpy.types.Scene, bpy.types.Depsgraph], None]:
        return FrameHandler(handlers=self.handlers, body=self.matched).entry(identity)

    def matched(self, scene: bpy.types.Scene, depsgraph: bpy.types.Depsgraph) -> Outcome[None]:
        """Passes matching updates to body during handler call.

        Args:
            scene: Scene the handler received.
            depsgraph: Evaluated depsgraph holding this pass's updates.

        Returns:
            Body result, Ok(None) when no update matches.
        """
        updates = tuple(item for item in depsgraph.updates if isinstance(item.id, self.ids) and self.changes.map(lambda kinds: bool(changed(item) & kinds)).default_value(True))
        return self.body(scene, updates) if updates else Ok(None)


class ImportHandler(Handler[Callable[[bpy.types.BlendImportContext], None]], frozen=True, kw_only=True):
    """Append or link callback receiving read-only import context."""

    body: Callable[[bpy.types.BlendImportContext], Outcome[None]]

    @override
    def entry(self, identity: Identity, /) -> Callable[[bpy.types.BlendImportContext], None]:
        body = identity.reports.reported(self.body)
        return lambda context: body(context).to_optional()


# --- [SERVICES] -------------------------------------------------------------------------


class Dispatcher:
    """One persistent callback an extension keeps on a handler list, running each held callback with the arguments it reads."""

    __slots__ = ("entries", "package")

    def __init__(self, package: str) -> None:
        """Holds no callback of `package`."""
        self.package = package
        self.entries: list[tuple[Callback, int]] = []

    @classmethod
    def held[E](cls, handlers: Handlers[E], package: str) -> "Dispatcher":
        """Returns the dispatcher of `package` on handler list, appending a new one when absent."""

        def opened() -> Dispatcher:
            dispatcher = cls(package)
            handlers.append(dispatcher.dispatched)
            return dispatcher

        return Option.of_optional(
            next((handler.__self__ for handler in handlers if isinstance(handler, MethodType) and isinstance(handler.__self__, cls) and handler.__self__.package == package), None)
        ).default_with(opened)

    @bpy.app.handlers.persistent  # type: ignore[untyped-decorator]  # ty: ignore[dynamic-function-decorator-return]
    def dispatched(self, *arguments: object) -> None:
        """Runs every held callback with the arguments it reads."""
        for held, count in tuple(self.entries):
            held(*arguments[:count])


# --- [OPERATIONS] -----------------------------------------------------------------------


def staged(identity: Identity, scene: bpy.types.Scene) -> bool:
    """Answers whether scene is one of extension's runtime scenes.

    Args:
        identity: Extension whose manifest ID marks its runtime scenes.
        scene: Scene a callback received.

    Returns:
        True for a runtime scene holding the extension's mark.
    """
    return scene.is_runtime_data and identity.manifest.id in scene  # type: ignore[operator]


def changed(update: bpy.types.DepsgraphUpdate) -> Change:
    """Reads change flags of update during its handler call.

    Args:
        update: Update holding geometry, transform, and shading flags.

    Returns:
        Reported change kinds, `Change(0)` when none.
    """
    return (
        (Change.GEOMETRY if update.is_updated_geometry else Change(0)) | (Change.TRANSFORM if update.is_updated_transform else Change(0)) | (Change.SHADING if update.is_updated_shading else Change(0))
    )


@contextmanager
def appended[E](identity: Identity, handlers: Handlers[E], entry: E) -> Generator[None]:
    """Holds callback on the one persistent dispatcher an extension keeps on a handler list.

    Args:
        identity: Extension owning the dispatcher.
        handlers: Handler list accepting callback argument shape.
        entry: Plain function with the positional parameters it reads from the padded arguments.

    Yields:
        None while the dispatcher holds callback.
    """
    dispatcher = Dispatcher.held(handlers, identity.package)
    member: tuple[Callback, int] = (entry, len(inspect.signature(entry).parameters))  # type: ignore[assignment, arg-type]  # ty: ignore[invalid-assignment, invalid-argument-type]
    dispatcher.entries.append(member)
    try:
        yield
    finally:
        dispatcher.entries.remove(member)
        if not dispatcher.entries:
            handlers.remove(dispatcher.dispatched)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "LOAD_POST",
    "LOAD_PRE",
    "SAVE_POST",
    "SAVE_PRE",
    "Change",
    "Dispatcher",
    "FrameHandler",
    "Handler",
    "Handlers",
    "ImportHandler",
    "PathHandler",
    "SceneHandler",
    "UpdateHandler",
    "appended",
    "changed",
    "staged",
]
