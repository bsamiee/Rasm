"""Persistent callbacks on `bpy.app.handlers` lists."""

from collections.abc import Callable, Generator, Iterator
from contextlib import contextmanager
from enum import auto, Flag
import inspect
from pathlib import Path
from types import MethodType
from typing import Protocol

import bpy
from expression import Nothing, Ok, Option
import msgspec

from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.faults import BlenderResult

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


# --- [MODELS] ---------------------------------------------------------------------------


class SceneHandler(msgspec.Struct, frozen=True, kw_only=True):
    """Scene callback skipping extension's runtime scenes."""

    handlers: Handlers[Callable[[bpy.types.Scene], None]]
    body: Callable[[bpy.types.Scene], BlenderResult[None]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds scene callback acquisition.

        Args:
            identity: Extension holding reports and runtime scene mark.

        Returns:
            Ok of one app-handler acquisition.
        """
        body = identity.reports.reported(self.body)

        def entry(scene: bpy.types.Scene) -> None:
            """Runs body outside extension's runtime scenes."""
            return None if scene.is_runtime_data and identity.manifest.id in scene else body(scene).to_optional()  # type: ignore[operator]

        return Ok(((Band.APP_HANDLERS, appended(identity, self.handlers, entry)),))


class FrameHandler(msgspec.Struct, frozen=True, kw_only=True):
    """Frame callback receiving evaluated depsgraph with no update filter."""

    handlers: Handlers[Callable[[bpy.types.Scene, bpy.types.Depsgraph], None]]
    body: Callable[[bpy.types.Scene, bpy.types.Depsgraph], BlenderResult[None]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds frame callback acquisition.

        Args:
            identity: Extension holding reports and runtime scene mark.

        Returns:
            Ok of one app-handler acquisition.
        """
        body = identity.reports.reported(self.body)

        def entry(scene: bpy.types.Scene, depsgraph: bpy.types.Depsgraph) -> None:
            """Runs body outside extension's runtime scenes."""
            return None if scene.is_runtime_data and identity.manifest.id in scene else body(scene, depsgraph).to_optional()  # type: ignore[operator]

        return Ok(((Band.APP_HANDLERS, appended(identity, self.handlers, entry)),))


class PathHandler(msgspec.Struct, frozen=True, kw_only=True):
    """Load or save callback receiving Nothing for startup file."""

    handlers: Handlers[Callable[[str, None], None]]
    body: Callable[[Option[Path]], BlenderResult[None]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds file callback acquisition.

        Args:
            identity: Extension with reports holding callback faults.

        Returns:
            Ok of one app-handler acquisition.
        """
        body = identity.reports.reported(self.body)

        def entry(path: str, _: None) -> None:
            """Lifts empty startup file path to Nothing."""
            return body(Option.of_optional(path or None).map(Path)).to_optional()

        return Ok(((Band.APP_HANDLERS, appended(identity, self.handlers, entry)),))


class UpdateHandler(msgspec.Struct, frozen=True, kw_only=True):
    """Scene callback receiving updates of its ID types and optional change kinds."""

    handlers: Handlers[Callable[[bpy.types.Scene, bpy.types.Depsgraph], None]]
    ids: tuple[type[bpy.types.ID], ...]
    body: Callable[[bpy.types.Scene, tuple[bpy.types.DepsgraphUpdate, ...]], BlenderResult[None]]
    changes: Option[Change] = Nothing

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds depsgraph callback acquisition.

        Args:
            identity: Extension holding reports and runtime scene mark.

        Returns:
            Ok of one app-handler acquisition.
        """

        def entry(scene: bpy.types.Scene, depsgraph: bpy.types.Depsgraph) -> BlenderResult[None]:
            """Passes matching updates to body during handler call."""
            matched = tuple(item for item in depsgraph.updates if isinstance(item.id, self.ids) and all(changed(item) & kinds for kinds in self.changes.to_list()))
            return self.body(scene, matched) if matched else Ok(None)

        return FrameHandler(handlers=self.handlers, body=entry).acquisitions(identity)


class ImportHandler(msgspec.Struct, frozen=True, kw_only=True):
    """Append or link callback receiving read-only import context."""

    handlers: Handlers[Callable[[bpy.types.BlendImportContext], None]]
    body: Callable[[bpy.types.BlendImportContext], BlenderResult[None]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds import callback acquisition.

        Args:
            identity: Extension with reports holding callback faults.

        Returns:
            Ok of one app-handler acquisition.
        """
        body = identity.reports.reported(self.body)

        def entry(context: bpy.types.BlendImportContext) -> None:
            """Runs import body through reports."""
            return body(context).to_optional()

        return Ok(((Band.APP_HANDLERS, appended(identity, self.handlers, entry)),))


# --- [SERVICES] -------------------------------------------------------------------------


class Dispatcher(msgspec.Struct, frozen=True, kw_only=True):
    """One persistent callback an extension keeps on a handler list, running each held callback with the arguments it reads."""

    package: str
    entries: list[tuple[Callback, int]]

    @classmethod
    def held[E](cls, handlers: Handlers[E], package: str) -> "Dispatcher":
        """Returns the dispatcher of `package` on handler list, appending a new one when absent."""

        def opened() -> Dispatcher:
            dispatcher = cls(package=package, entries=[])
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

__all__ = ["Change", "Dispatcher", "FrameHandler", "Handlers", "ImportHandler", "PathHandler", "SceneHandler", "UpdateHandler", "appended", "changed"]
