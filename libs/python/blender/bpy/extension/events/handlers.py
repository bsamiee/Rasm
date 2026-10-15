"""Persistent callbacks over Blender's native handler lists."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from enum import auto, Flag
from pathlib import Path
from typing import Protocol

import bpy
from expression import Nothing, Ok, Option
import msgspec

from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.faults import BlenderResult

# --- [TYPES] ----------------------------------------------------------------------------


class Change(Flag):
    """Kinds of change reported by a native dependency-graph update."""

    GEOMETRY = auto()
    TRANSFORM = auto()
    SHADING = auto()


class Handlers[E](Protocol):
    """Native handler list accepting callbacks of one argument shape, lists of wider callback unions included."""

    def append(self, entry: E, /) -> None:
        """Holds the callback on the host list."""

    def remove(self, entry: E, /) -> None:
        """Drops the callback from the host list."""


# --- [MODELS] ---------------------------------------------------------------------------


class SceneHandler(msgspec.Struct, frozen=True, kw_only=True):
    """Scene callback skipping the extension's owned runtime preview scenes."""

    handlers: Handlers[Callable[[bpy.types.Scene], None]]
    body: Callable[[bpy.types.Scene], BlenderResult[None]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds the persistent scene callback acquisition.

        Args:
            identity: Extension whose reports and runtime scene mark the callback consumes.

        Returns:
            Ok of one app-handler acquisition.
        """
        body = identity.reports.reported(self.body)

        def entry(scene: bpy.types.Scene) -> None:
            """Reports the body for saved scenes and other extensions' scenes."""
            return None if scene.is_runtime_data and identity.manifest.id in scene else body(scene).to_optional()  # type: ignore[operator]

        return Ok(((Band.APP_HANDLERS, appended(self.handlers, entry)),))


class FrameHandler(msgspec.Struct, frozen=True, kw_only=True):
    """Frame callback receiving the evaluated dependency graph without update filtering."""

    handlers: Handlers[Callable[[bpy.types.Scene, bpy.types.Depsgraph], None]]
    body: Callable[[bpy.types.Scene, bpy.types.Depsgraph], BlenderResult[None]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds the persistent frame callback acquisition.

        Args:
            identity: Extension whose reports and runtime scene mark the callback consumes.

        Returns:
            Ok of one app-handler acquisition.
        """
        body = identity.reports.reported(self.body)

        def entry(scene: bpy.types.Scene, depsgraph: bpy.types.Depsgraph) -> None:
            """Reports the body with the native scene and dependency graph."""
            return None if scene.is_runtime_data and identity.manifest.id in scene else body(scene, depsgraph).to_optional()  # type: ignore[operator]

        return Ok(((Band.APP_HANDLERS, appended(self.handlers, entry)),))


class PathHandler(msgspec.Struct, frozen=True, kw_only=True):
    """Load or save callback receiving an absent path for the startup file."""

    handlers: Handlers[Callable[[str, None], None]]
    body: Callable[[Option[Path]], BlenderResult[None]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds the persistent file callback acquisition.

        Args:
            identity: Extension whose reports hold callback failures.

        Returns:
            Ok of one app-handler acquisition.
        """
        body = identity.reports.reported(self.body)

        def entry(path: str, _: None) -> None:
            """Lifts the native startup-file path into absence at the boundary, Blender passing `None` after it."""
            return body(Option.of_optional(path or None).map(Path)).to_optional()

        return Ok(((Band.APP_HANDLERS, appended(self.handlers, entry)),))


class UpdateHandler(msgspec.Struct, frozen=True, kw_only=True):
    """Scene callback receiving updates of its ID types and optional change kinds."""

    handlers: Handlers[Callable[[bpy.types.Scene, bpy.types.Depsgraph], None]]
    ids: tuple[type[bpy.types.ID], ...]
    body: Callable[[bpy.types.Scene, tuple[bpy.types.DepsgraphUpdate, ...]], BlenderResult[None]]
    changes: Option[Change] = Nothing

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds the persistent dependency-graph callback acquisition.

        Args:
            identity: Extension whose reports and runtime scene mark the callback consumes.

        Returns:
            Ok of one app-handler acquisition.
        """

        def entry(scene: bpy.types.Scene, depsgraph: bpy.types.Depsgraph) -> BlenderResult[None]:
            """Delivers matching native updates while their handles remain valid."""
            matched = tuple(item for item in depsgraph.updates if isinstance(item.id, self.ids) and all(changed(item) & kinds for kinds in self.changes.to_list()))
            return self.body(scene, matched) if matched else Ok(None)

        return FrameHandler(handlers=self.handlers, body=entry).acquisitions(identity)


class ImportHandler(msgspec.Struct, frozen=True, kw_only=True):
    """Append or link callback receiving the native read-only import context."""

    handlers: Handlers[Callable[[bpy.types.BlendImportContext], None]]
    body: Callable[[bpy.types.BlendImportContext], BlenderResult[None]]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds the persistent import callback acquisition.

        Args:
            identity: Extension whose reports hold callback failures.

        Returns:
            Ok of one app-handler acquisition.
        """
        body = identity.reports.reported(self.body)

        def entry(context: bpy.types.BlendImportContext) -> None:
            """Reports the import body and answers the native callback result."""
            return body(context).to_optional()

        return Ok(((Band.APP_HANDLERS, appended(self.handlers, entry)),))


# --- [OPERATIONS] -----------------------------------------------------------------------


def changed(update: bpy.types.DepsgraphUpdate) -> Change:
    """Reads the native change flags of an update valid during its handler call.

    Args:
        update: Native update carrying geometry, transform, and shading facts.

    Returns:
        Every reported change kind, or the empty flag for a flagless update.
    """
    return (
        (Change.GEOMETRY if update.is_updated_geometry else Change(0)) | (Change.TRANSFORM if update.is_updated_transform else Change(0)) | (Change.SHADING if update.is_updated_shading else Change(0))
    )


@contextmanager
def appended[E](handlers: Handlers[E], entry: E) -> Generator[None]:
    """Holds one persistent callback on its native handler list.

    Args:
        handlers: Host list accepting the callback's argument shape.
        entry: Plain function whose positional parameters Blender inspects.

    Yields:
        None while the host holds the callback.
    """
    handlers.append(bpy.app.handlers.persistent(entry))
    try:
        yield
    finally:
        handlers.remove(entry)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Change", "FrameHandler", "Handlers", "ImportHandler", "PathHandler", "SceneHandler", "UpdateHandler", "appended", "changed"]
