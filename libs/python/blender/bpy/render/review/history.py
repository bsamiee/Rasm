"""Render history store: each finished render kept under retention in the extension folder with its settings snapshot and its preview tile, the stored images loaded from it, and the signal each change fires."""

from collections.abc import Mapping
from contextlib import nullcontext
from copy import deepcopy
import datetime
from functools import partial
from pathlib import Path
from typing import ClassVar, Final

import bpy
from expression import Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.data.ids.ownership import Ownership, released, Saved
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Declaration, Resource
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.outputs.results import combined, CompletedRender, RenderSource, ResultRead, source
from blender.bpy.render.previews.queue import Job, PreviewFeed, PreviewQueue, Rank
from blender.bpy.render.sessions.renders import RENDER_HISTORY
from blender.bpy.ui.drawing.handlers import Signal
from blender.bpy.ui.resources.previews import PreviewKey, PREVIEWS
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.buffers import fitted
from blender.imaging.files.images import ImageFile
from blender.imaging.files.multilayer import Multilayer
from blender.records.faults import accumulated, Fault, Nothing, Ok, Outcome, paired, Reason
from blender.records.fields import Dated, History, Retention
from blender.records.stores import content_hash, ContentHash, files_copied, RecordFile, removed

# --- [MODELS] ---------------------------------------------------------------------------


class Renders(msgspec.Struct, frozen=True, kw_only=True):
    """Stored render hashes newest first under retention, with the note written on each, keyed by its file stem."""

    history: History[ContentHash] = History()
    notes: Mapping[str, str] = msgspec.field(default_factory=dict)
    origins: Mapping[str, tuple[str, RenderSource | None]] = msgspec.field(default_factory=dict)

    @property
    def names(self) -> Mapping[ContentHash, str]:
        """Stored render notes or newest-first `#n` labels."""
        return {entry.value: self.notes.get(entry.value.hex()) or f"#{index}" for index, entry in enumerate(self.history.past, 1)}

    def before(self, picked: str) -> Option[ContentHash]:
        """Returns the stored render a pick names by its key, else the one stored before the newest, else the newest: the frame a compare draws as before."""
        return next((Some(entry.value) for entry in self.history.past if entry.value.hex() == picked), self.history.at(2).or_else_with(lambda: self.history.at(1)).map(lambda entry: entry.value))


# --- [SERVICES] -------------------------------------------------------------------------

_SERVED: Final = Resource[dict[Path, Renders]](made=lambda _identity: nullcontext({}))
_COMPLETED: Final = Resource[dict[Path, tuple[str, CompletedRender]]](made=lambda _identity: nullcontext({}), renewed=Some(lambda _held: {}))


class RenderHistory(msgspec.Struct, frozen=True, kw_only=True):
    """Finished renders stored under the retention of the `render_history` preference in the extension folder, each tiled for the preview collection, `signal` firing after each change."""

    # --- [STATE]

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
    def completed(identity: Identity, key: ContentHash, config: PyOpenColorIO.Config) -> Outcome[CompletedRender]:
        """Returns an explicitly selected immutable history file under the current ordinary-file color definitions."""
        path, cache, current = RenderHistory.frame(identity, key), _COMPLETED[identity], config.getCacheID()

        def held(renders: Renders) -> Outcome[CompletedRender]:
            match cache.get(path):
                case (version, frame) if version == current:
                    return Ok(frame)
                case _:
                    origin = renders.origins.get(key.hex())

                    def kept(file: Multilayer) -> CompletedRender:
                        frame = CompletedRender.stored(
                            file, config, space=Option.of_optional(origin).map(lambda found: found[0]), raw=Option.of_optional(origin).bind(lambda found: Option.of_optional(found[1]))
                        )
                        cache[path] = current, frame
                        return frame

                    return Multilayer.read(path).map(kept)

        return RenderHistory.held(identity).bind(held)

    @staticmethod
    def opened(identity: Identity, image: bpy.types.Image, config: PyOpenColorIO.Config) -> Outcome[Option[CompletedRender]]:
        """Returns an owned history image's immutable frame, absent for ordinary images."""
        return RenderHistory.stored.marked(identity, image).bind(lambda mark: mark.content).map(lambda key: RenderHistory.completed(identity, key, config).map(Some)).default_value(Ok(Nothing))

    @staticmethod
    def selected(identity: Identity, context: bpy.types.Context) -> Outcome[CompletedRender]:
        """Returns the current explicitly owned history image's file, without associating native render slots with history."""
        return (
            rooted(bpy.types.SpaceImageEditor, context)
            .bind(lambda space: Option.of_optional(space.image).to_result_with(partial(Fault.of, Reason.ABSENT, repr(space.image), member="explicitly opened history render")))
            .bind(lambda image: RenderHistory.opened(identity, image, PyOpenColorIO.GetCurrentConfig()))
            .bind(lambda found: found.to_result_with(partial(Fault.of, Reason.ABSENT, repr(context.space_data), member="explicitly opened history render")))
        )

    @staticmethod
    def output(identity: Identity, key: ContentHash, config: PyOpenColorIO.Config, layer: Option[str] = Nothing, *, view: Option[str] = Nothing) -> Outcome[ImageFile]:
        """Returns a stored output with its native encoding, selecting a view layer or camera view when supplied."""

        def loaded(renders: Renders) -> Outcome[ImageFile]:
            match layer, renders.origins.get(key.hex()):
                case Option(tag="some"), (_, RenderSource() as raw):
                    return Multilayer.read(RenderHistory.frame(identity, key)).bind(lambda file: source(file, config, raw, layer=layer, view=view))
                case _, origin:
                    return combined(str(RenderHistory.frame(identity, key)), config, layer, view=view).map(
                        lambda file: msgspec.structs.replace(file, colorspace=origin[0]) if origin is not None and layer.is_none() else file
                    )

        return RenderHistory.held(identity).bind(loaded)

    @staticmethod
    def source(identity: Identity, key: ContentHash, config: PyOpenColorIO.Config, *, view: Option[str] = Nothing) -> Outcome[ImageFile]:
        """Returns the captured raw source, absent for an entry without its metadata."""
        return paired(RenderHistory.held(identity), Multilayer.read(RenderHistory.frame(identity, key))).bind(
            lambda found: (
                Option
                .of_optional(found[0].origins.get(key.hex()))
                .bind(lambda origin: Option.of_optional(origin[1]))
                .to_result_with(lambda: Fault.of(Reason.ABSENT, str(RenderHistory.frame(identity, key)), member="recorded raw source"))
                .bind(lambda raw: source(found[1], config, raw, view=view))
            )
        )

    @staticmethod
    def served(identity: Identity, renders: Renders) -> Renders:
        """Holds renders as the history file's read, returning them."""
        _SERVED[identity][RenderHistory.record(identity).path] = renders
        return renders

    @staticmethod
    def held(identity: Identity) -> Outcome[Renders]:
        """Returns the served renders, read from the history file on first use and empty without one."""
        record = RenderHistory.record(identity)
        return Option.of_optional(_SERVED[identity].get(record.path)).map(Ok).default_with(lambda: record.found().map(lambda found: RenderHistory.served(identity, found.default_with(Renders))))

    def kept(self, identity: Identity, before: Renders, after: Renders) -> Outcome[None]:
        """Commits and publishes retained renders, then releases dropped files, tiles, and images while accumulating independent failures."""
        live = {entry.value.hex() for entry in after.history.past}
        current = msgspec.structs.replace(
            after, notes={stem: note for stem, note in after.notes.items() if stem in live}, origins={stem: origin for stem, origin in after.origins.items() if stem in live}
        )

        def erased(key: ContentHash) -> Outcome[None]:
            _COMPLETED[identity].pop(self.frame(identity, key), None)
            return paired(
                accumulated(map(removed, (self.frame(identity, key), self.snapshot(identity, key).path))),
                PREVIEWS[identity].dropped(key.hex()),
                released(tuple(self.stored.found(identity, content=Some(key)).to_list())),
            ).map(lambda _: None)

        return (
            self
            .record(identity)
            .written(current)
            .map(lambda _: self.served(identity, current))
            .bind(lambda _: paired(self.signal[identity].fired(), accumulated(erased(entry.value) for entry in before.history.past if entry.value.hex() not in live)))
            .map(lambda _: None)
        )

    # --- [STORING]

    def tiled(self, identity: Identity, key: ContentHash, view: DisplayView, config: PyOpenColorIO.Config) -> Job:
        """Constructs a tile request from a stored frame through the supplied view and configuration."""
        shown = PreviewKey.read(content_hash(RenderHistory, key, view, config.getCacheID()))
        return Job(
            place=(RenderHistory, key),
            key=shown,
            rank=Rank.SHOWN,
            run=lambda: (
                self
                .output(identity, key, config)
                .bind(lambda file: view.form(config, file.pixels, file.colorspace))
                .bind(lambda found: fitted(found, (shown.side, shown.side)))
                .bind(lambda pixels: PREVIEWS[identity].written(key.hex(), shown, Some(shown.checkered(pixels))))
                .map(lambda _: None)
            ),
        )

    def jobs(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
        """Returns stale tile requests from one history read and one scene view."""
        previews, config = PREVIEWS[identity], deepcopy(PyOpenColorIO.GetCurrentConfig())
        return paired(self.held(identity), rooted(bpy.types.Scene, context).map(viewed)).map(
            lambda pair: tuple(job for entry in pair[0].history.past for job in (self.tiled(identity, entry.value, pair[1], config),) if previews.read(entry.value.hex(), job.key).is_none())
        )

    def recorded(self, identity: Identity, snapshot: Snapshot, moment: datetime.datetime, view: DisplayView, completed: CompletedRender) -> Outcome[Option[str]]:
        """Stores a finished render with its settings snapshot under retention, tiles it, then signals the compare."""
        path, raw, config = completed.file.path, completed.raw.to_optional(), completed.config

        def stored(content: ContentHash, space: str, renders: Renders, retention: Retention) -> Outcome[None]:
            key = content_hash(Renders, snapshot, space, raw, content)
            added = msgspec.structs.replace(renders, history=renders.history.add(Dated(moment=moment, value=key), retention), origins={**renders.origins, key.hex(): (space, raw)})

            def written() -> Outcome[None]:
                return (
                    Ok(None)
                    if any(entry.value == key for entry in renders.history.past)
                    else files_copied(((path, self.frame(identity, key)),)).bind(lambda _: self.snapshot(identity, key).written(snapshot)).map(lambda _: None)
                )

            return (
                written().bind(lambda _: self.kept(identity, renders, added)).bind(lambda _: PreviewQueue.executed(identity, self.tiled(identity, key, view, config), feed=Some(self.jobs)))
                if any(entry.value == key for entry in added.history.past)
                else Ok(None)
            )

        retention = RENDER_HISTORY.read(identity).map(lambda held: held.retention)
        content = completed.file.fingerprint(
            lambda name: (
                name not in {"Date", "RenderTime", "Memory", "Hostname"} and not (name.startswith("cycles.") and name.rpartition(".")[2] in {"total_time", "render_time", "synchronization_time"})
            )
        )
        space = completed.space.to_result_with(partial(Fault.of, Reason.ABSENT, str(path), member="captured output encoding"))
        return paired(content, space, self.held(identity), retention).bind(lambda found: stored(*found)).map(lambda _: Nothing)

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Served renders, the change signal, and the preview collection."""
        return (_SERVED, _COMPLETED, self.signal, PREVIEWS)

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns the retention preference, the stored images, the tile feed, and the render read."""
        return (
            RENDER_HISTORY,
            self.stored,
            PreviewFeed(feed=self.jobs),
            ResultRead(
                capture=lambda scene, _config: Snapshot.capture(scene, owned=identity.owns).map(
                    lambda snapshot: partial(self.recorded, identity, snapshot, datetime.datetime.now(datetime.UTC), viewed(scene))
                )
            ),
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RenderHistory", "Renders"]
