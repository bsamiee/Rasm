"""Render history store: each finished render kept under retention in the extension folder with its settings snapshot and its preview tile, the stored images loaded from it, and the signal each change fires."""

from collections.abc import Mapping
from contextlib import nullcontext
import datetime
from functools import partial
from pathlib import Path
from typing import ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.ownership import Ownership, released, Saved
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.outputs.results import combined, ResultRead
from blender.bpy.render.previews.queue import Job, PreviewFeed, PreviewQueue, Rank
from blender.bpy.render.sessions.renders import RENDER_HISTORY
from blender.bpy.ui.drawing.handlers import Signal
from blender.bpy.ui.resources.previews import PreviewKey, PREVIEWS
from blender.imaging.files.buffers import fitted
from blender.records.faults import accumulated, Outcome, paired
from blender.records.fields import Dated, History, Retention
from blender.records.stores import content_hash, ContentHash, files_copied, RecordFile, removed

# --- [MODELS] ---------------------------------------------------------------------------


class Renders(msgspec.Struct, frozen=True, kw_only=True):
    """Stored render hashes newest first under retention, with the note written on each, keyed by its file stem."""

    history: History[ContentHash] = History()
    notes: Mapping[str, str] = msgspec.field(default_factory=dict)

    def named(self, key: ContentHash) -> str:
        """Returns a stored render's note, `#n` by its newest-first place for an unnoted one."""
        return self.notes.get(key.hex()) or f"#{next(index for index, entry in enumerate(self.history.past, 1) if entry.value == key)}"

    def before(self, picked: str) -> Option[ContentHash]:
        """Returns the stored render a pick names by its key, else the one stored before the newest, else the newest: the frame a compare draws as before."""
        keys = tuple(entry.value for entry in self.history.past)
        return Block.of_seq(keys).filter(lambda key: key.hex() == picked).try_head().or_else_with(lambda: Block.of_seq(keys[1:2] or keys[:1]).try_head())


# --- [SERVICES] -------------------------------------------------------------------------

_SERVED: Final = Resource[dict[Path, Renders]](made=lambda _identity: nullcontext({}))


class RenderHistory(msgspec.Struct, frozen=True, kw_only=True):
    """Finished renders stored under the retention of the `render_history` preference in the extension folder, each tiled for the preview collection, `signal` firing after each change."""

    key: ClassVar[Key] = "history"
    stored: ClassVar[Ownership[bpy.types.Image]] = Ownership(kind=bpy.types.Image, key="stored_render", saved=Saved.USED)
    signal: ClassVar[Resource[Signal]] = Resource[Signal](made=lambda _identity: nullcontext(Signal()))

    # --- [STORAGE]

    @staticmethod
    def folder(identity: Identity) -> Path:
        """Returns the history folder under the extension user folder."""
        return identity.folder / RenderHistory.key

    @staticmethod
    def record(identity: Identity) -> RecordFile[Renders]:
        """Returns the history file."""
        return RecordFile(path=RenderHistory.folder(identity) / f"{RenderHistory.key}.json", form=Renders)

    @staticmethod
    def frame(identity: Identity, key: ContentHash) -> Path:
        """Returns a stored render's frame path."""
        return RenderHistory.folder(identity) / f"{key.hex()}.exr"

    @staticmethod
    def snapshot(identity: Identity, key: ContentHash) -> RecordFile[Snapshot]:
        """Returns a stored render's settings snapshot file."""
        return RecordFile(path=RenderHistory.folder(identity) / f"{key.hex()}.json", form=Snapshot)

    @staticmethod
    def output(identity: Identity, key: ContentHash, layer: Option[str] = Nothing) -> Outcome[npt.NDArray[np.float32]]:
        """Returns a stored render's output plane, a named view layer's Combined pass in place of the composite."""
        return combined(str(RenderHistory.frame(identity, key)), layer)

    @staticmethod
    def formed(identity: Identity, scene: bpy.types.Scene, key: ContentHash) -> Outcome[npt.NDArray[np.float32]]:
        """Returns a stored render's output formed through the scene's view, the colors the editor shows it in."""
        return RenderHistory.output(identity, key).bind(lambda plane: viewed(scene).form(PyOpenColorIO.GetCurrentConfig(), plane))

    @staticmethod
    def served(identity: Identity, renders: Renders) -> Renders:
        """Holds renders as the history file's read, returning them."""
        identity.resources[_SERVED][RenderHistory.record(identity).path] = renders
        return renders

    @staticmethod
    def held(identity: Identity) -> Outcome[Renders]:
        """Returns the served renders, read from the history file on first use and empty without one."""
        record = RenderHistory.record(identity)
        return (
            Option.of_optional(identity.resources[_SERVED].get(record.path)).map(Ok).default_with(lambda: record.found().map(lambda found: RenderHistory.served(identity, found.default_with(Renders))))
        )

    def kept(self, identity: Identity, before: Renders, after: Renders) -> Outcome[None]:
        """Writes and serves renders without the notes of dropped entries, then deletes each dropped entry's files, tile, and stored image."""
        live = {entry.value for entry in after.history.past}
        current = msgspec.structs.replace(after, notes={stem: note for stem, note in after.notes.items() if bytes.fromhex(stem) in live})

        def erased(key: ContentHash) -> Outcome[None]:
            return (
                accumulated(map(removed, (self.frame(identity, key), self.snapshot(identity, key).path)))
                .bind(lambda _: identity.resources[PREVIEWS].dropped(key.hex()))
                .bind(lambda _: released(tuple(self.stored.found(identity, content=Some(key)).to_list())))
                .map(lambda _: None)
            )

        return (
            self
            .record(identity)
            .written(current)
            .map(lambda _: self.served(identity, current))
            .bind(lambda _: accumulated(erased(entry.value) for entry in before.history.past if entry.value not in live))
            .map(lambda _: None)
        )

    # --- [STORING]

    def tiled(self, identity: Identity, key: ContentHash, plane: Option[npt.NDArray[np.float32]] = Nothing) -> Job:
        """Constructs a stored render's tile request from a formed plane, or its stored frame formed through the context scene's view, at the host preview size."""
        shown = PreviewKey.read(key)
        return Job(
            place=(RenderHistory, key),
            key=content_hash(RenderHistory, shown),
            rank=Rank.SHOWN,
            run=lambda: (
                plane
                .map(Ok)
                .default_with(lambda: rooted(bpy.types.Scene, bpy.context).bind(lambda scene: self.formed(identity, scene, key)))
                .bind(lambda found: fitted(found, (shown.side, shown.side)))
                .bind(lambda pixels: identity.resources[PREVIEWS].written(key.hex(), shown, Some(shown.checkered(pixels))))
                .map(lambda _: None)
            ),
        )

    def jobs(self, identity: Identity, _context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
        """Returns the tile request of every stale stored render from one history read."""
        previews = identity.resources[PREVIEWS]
        return self.held(identity).map(
            lambda renders: tuple(self.tiled(identity, entry.value) for entry in renders.history.past if not previews.current(entry.value.hex(), PreviewKey.read(entry.value)))
        )

    def recorded(self, identity: Identity, scene: bpy.types.Scene, path: str) -> Outcome[Option[str]]:
        """Stores a finished render with its settings snapshot under retention, tiles it, then signals the compare."""
        moment, config = datetime.datetime.now(datetime.UTC), PyOpenColorIO.GetCurrentConfig()

        def stored(snapshot: Snapshot, plane: npt.NDArray[np.float32], renders: Renders, retention: Retention) -> Outcome[None]:
            key = content_hash(Renders, snapshot, arrays=(plane,))
            added = msgspec.structs.replace(renders, history=renders.history.add(Dated(moment=moment, value=key), retention))

            def written() -> Outcome[None]:
                return (
                    Ok(None)
                    if any(entry.value == key for entry in renders.history.past)
                    else files_copied(((Path(path), self.frame(identity, key)),)).bind(lambda _: self.snapshot(identity, key).written(snapshot)).map(lambda _: None)
                )

            return (
                written()
                .bind(lambda _: self.kept(identity, renders, added))
                .bind(lambda _: viewed(scene).form(config, plane))
                .bind(lambda shown: PreviewQueue.executed(identity, self.tiled(identity, key, Some(shown)), feed=Some(self.jobs)))
                .bind(lambda _: identity.resources[self.signal].fired())
                if any(entry.value == key for entry in added.history.past)
                else Ok(None)
            )

        retention = RENDER_HISTORY.read(identity).map(lambda held: held.retention)
        return paired(Snapshot.capture(scene, owned=identity.owns), combined(path), self.held(identity), retention).bind(lambda found: stored(*found)).map(lambda _: Nothing)

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Served renders, the change signal, and the preview collection."""
        return (_SERVED, self.signal, PREVIEWS)

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the retention preference, the stored images, the tile feed, and the render read."""
        return Ok((RENDER_HISTORY, self.stored, PreviewFeed(feed=self.jobs), ResultRead(consume=partial(self.recorded, identity))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RenderHistory", "Renders"]
