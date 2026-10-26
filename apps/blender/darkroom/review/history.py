"""Darkroom render history in the render view's Sidebar tab: renders through the chain into slots, each finished render stored with its settings snapshot, browsed as a before frame, measured, and tinted by difference."""

from collections.abc import Callable, Mapping
from contextlib import nullcontext
import datetime
from enum import auto, Enum
from functools import cached_property, partial, update_wrapper
from pathlib import Path
import shutil
from typing import ClassVar, Final

from bl_ui.space_image import IMAGE_HT_header, IMAGE_PT_render_slots
import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Answer, answered, Call, Confirm, Done, Empty, Operator, Properties
from blender.bpy.data.ids.images import loaded, Tag
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import rooted, Struct
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, Registrable
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderReason
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.outputs.exports import combined
from blender.bpy.render.outputs.results import ResultRead
from blender.bpy.render.previews.queue import Job, PreviewQueue, Rank
from blender.bpy.render.sessions.renders import RenderSlots
from blender.bpy.settings.attachments import annotated, Attachment
from blender.bpy.ui.drawing.handlers import Refresh, Signal, Watch
from blender.bpy.ui.images.overlays import FrameMask, Tint
from blender.bpy.ui.images.views import ImageWindow, revealed
from blender.bpy.ui.layout.rows import Caption, Flow, Label, Member, OperatorButton, Property, Row, Rows
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.templates import IconView
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.previews import PreviewKey, PREVIEWS
from blender.bpy.ui.resources.theme import color, Metrics
from blender.bpy.ui.screens.editors import EditorState
from blender.bpy.ui.surfaces.appends import Append
from blender.bpy.ui.surfaces.panels import Joined, Panel
from blender.imaging.analysis.comparison import Axis, Bar, Comparison
from blender.imaging.color.processors import DisplayView, Domain
from blender.imaging.files.buffers import fitted
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.identity import content_hash, ContentHash
from blender.records.stores import RecordFile
from blender.records.trees import Dated, Deleted, diff, Edit, History, Inserted, Moved, Reparametrized, Retention

from ..catalog import chain
from ..preferences import reviewing
from .compare import Before, CompareView

# --- [TYPES] ----------------------------------------------------------------------------


type _Row = tuple[str, str, str, int, int]


class Baseline(Enum):
    """Before frame source: the browsed stored render, or the shown render's view layer ahead of scene effects."""

    STORED_RENDER = auto()
    VIEW_LAYER = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

BASELINES: Final = tuple((member.name, bpy.path.display_name(member.name.lower()), "") for member in Baseline)

# --- [MODELS] ---------------------------------------------------------------------------


class Renders(msgspec.Struct, frozen=True, kw_only=True):
    """Stored render hashes newest first under retention, with the note written on each, keyed by its file stem."""

    history: History[ContentHash] = History()
    notes: Mapping[str, str] = msgspec.field(default_factory=dict)

    def named(self, key: ContentHash) -> str:
        """Returns a stored render's note, `#n` by its newest-first place for an unnoted one."""
        return self.notes.get(key.hex()) or f"#{next(index for index, entry in enumerate(self.history.past, 1) if entry.value == key)}"


class _Browsing(bpy.types.PropertyGroup):
    """History browser state of one window manager, its stored render rows bound at registration."""

    baseline: bpy.props.EnumProperty(name="Before Frame", description="Stored render, or the shown render's view layer ahead of scene effects", items=BASELINES)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    shown: bpy.props.EnumProperty(name="Before", description="Stored render drawn as before frame", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    difference: bpy.props.BoolProperty(name="Difference", description="Tint pixels by perceived difference from the before frame")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class _Noting(bpy.types.PropertyGroup):
    """Note on a stored render."""

    note: bpy.props.StringProperty(name="Note", description="Stored render note")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class _Served(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Renders the extension last read or wrote, and the browser rows Blender last read, replaced whole."""

    renders: Option[Renders] = Nothing
    rows: tuple[_Row, ...] = ()


# --- [SERVICES] -------------------------------------------------------------------------

_SERVED: Final = Scoped[_Served](made=lambda _identity: nullcontext(_Served()))


class HistoryStore(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Finished renders stored under retention in the extension folder, browsed per window manager, compared as a before frame, and graded against `gates`."""

    retention: Callable[[Identity], Outcome[Retention]]
    glyph: Glyph
    gates: tuple[Bar, ...] = (Bar(axis=Axis.PSNR, advisory=35.0, refuse=30.0), Bar(axis=Axis.NRMSE, advisory=0.05, refuse=0.10), Bar(axis=Axis.SSIM, advisory=0.98, refuse=0.95))
    signal: Signal = msgspec.field(default_factory=Signal)
    key: ClassVar[str] = "history"

    # --- [STORAGE]

    @staticmethod
    def folder(identity: Identity) -> Path:
        """Returns the history folder under the extension user folder."""
        return identity.folder / HistoryStore.key

    @staticmethod
    def record(identity: Identity) -> RecordFile[Renders]:
        """Returns the history file."""
        return RecordFile(path=HistoryStore.folder(identity) / f"{HistoryStore.key}.json", form=Renders)

    @staticmethod
    def files(identity: Identity, key: ContentHash) -> tuple[Path, RecordFile[Snapshot]]:
        """Returns a stored render's frame path and settings snapshot file."""
        stem = HistoryStore.folder(identity) / key.hex()
        return stem.with_suffix(".exr"), RecordFile(path=stem.with_suffix(".json"), form=Snapshot)

    @staticmethod
    def output(identity: Identity, key: ContentHash) -> Outcome[npt.NDArray[np.float32]]:
        """Returns a stored render's output plane."""
        return combined(str(HistoryStore.files(identity, key)[0])).map(lambda found: found[1])

    @staticmethod
    def held(identity: Identity) -> Outcome[Renders]:
        """Returns served renders, read from the history file on first use and empty without one."""
        served = _SERVED.of(identity)

        def absent(faults: Faults) -> Outcome[Renders]:
            match faults:
                case (Fault(reason=RecordReason.UNREADABLE, cause=FileNotFoundError()),):
                    return Ok(Renders())
                case _:
                    return Error(faults)

        def filled(renders: Renders) -> Renders:
            _SERVED.held[identity.package] = msgspec.structs.replace(_SERVED.of(identity), renders=Some(renders))
            return renders

        return served.renders.map(Ok).default_with(lambda: Result[Renders, Faults].or_else_with(HistoryStore.record(identity).read(), absent).map(filled))

    @staticmethod
    def kept(identity: Identity, before: Renders, after: Renders) -> Outcome[None]:
        """Writes and serves renders without the notes of dropped entries, then deletes each dropped entry's files and tile."""
        live = {entry.value for entry in after.history.past}
        dropped = {entry.value for entry in before.history.past} - live
        current = msgspec.structs.replace(after, notes={stem: note for stem, note in after.notes.items() if bytes.fromhex(stem) in live})

        def unlinked(path: Path) -> Outcome[None]:
            return catch(path.unlink, exception=OSError)(missing_ok=True).map_error(lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=str(path), cause=error),))

        def erased(key: ContentHash) -> Outcome[None]:
            frame, values = HistoryStore.files(identity, key)
            return accumulated(map(unlinked, (frame, values.path))).bind(lambda _: PREVIEWS.of(identity).dropped(key.hex()))

        def served(_: Path) -> None:
            _SERVED.held[identity.package] = msgspec.structs.replace(_SERVED.of(identity), renders=Some(current))

        return Result[Path, Faults].map(HistoryStore.record(identity).written(current), served).bind(lambda _: accumulated(map(erased, dropped))).map(lambda _: None)

    # --- [BROWSING]

    @cached_property
    def browsing(self) -> Attachment[_Browsing]:
        """Window manager browser state."""
        return Attachment(key=self.key, form=_Browsing, owner=bpy.types.WindowManager)

    def browsed(self, identity: Identity, context: bpy.types.Context) -> Outcome[_Browsing]:
        """Returns the window manager's browser state."""
        return paired(self.browsing.bound(identity), rooted(bpy.types.WindowManager, context)).map(lambda pair: pair[0].struct(pair[1]))

    def chosen(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Renders, ContentHash]]:
        """Returns renders and the browsed stored render, absent fault without one."""
        return paired(self.browsed(identity, context), self.held(identity)).bind(
            lambda pair: (
                Option
                .of_optional(next((entry.value for entry in pair[1].history.past if entry.value.hex() == pair[0].shown), None))
                .map(lambda key: (pair[1], key))
                .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=str(self.record(identity).path), member=pair[0].shown),))
            )
        )

    def beneath(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Renders, ContentHash]]:
        """Returns renders and the stored render the editor shows, the newest for Render Result."""

        def shown(space: bpy.types.SpaceImageEditor, renders: Renders) -> Outcome[tuple[Renders, ContentHash]]:
            return (
                Option
                .of_optional(space.image)
                .bind(
                    lambda image: Option.of_optional(
                        next(
                            (
                                entry.value
                                for entry in renders.history.past
                                if image.type == "RENDER_RESULT" or self.files(identity, entry.value)[0] == Path(bpy.path.abspath(image.filepath, library=image.library))
                            ),
                            None,
                        )
                    )
                )
                .map(lambda key: (renders, key))
                .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=str(self.record(identity).path), member=repr(space)),))
            )

        return paired(rooted(bpy.types.SpaceImageEditor, context), self.held(identity)).bind(lambda pair: shown(*pair))

    def shown(self, identity: Identity, context: bpy.types.Context) -> Outcome[str]:
        """Returns the shown frame's history name, the image's own name for a frame the history holds no entry of."""

        def imaged(faults: Faults) -> Outcome[str]:
            match faults:
                case (Fault(reason=BlenderReason.ABSENT),):
                    return rooted(bpy.types.SpaceImageEditor, context).bind(lambda space: Option.of_optional(space.image).map(lambda image: image.name).to_result_with(lambda: faults))
                case _:
                    return Error(faults)

        return Result[str, Faults].or_else_with(self.beneath(identity, context).map(lambda pair: pair[0].named(pair[1])), imaged)

    # --- [FRAMES]

    def side(self, identity: Identity, context: bpy.types.Context, baseline: Baseline) -> Outcome[Before]:
        """Returns the browsed stored render captioned by its history name, or the shown render's view layer ahead of effects formed through the chain view and gain."""

        def formed(scene: bpy.types.Scene, plane: npt.NDArray[np.float32]) -> Outcome[npt.NDArray[np.float32]]:
            def exposed(view: Option[DisplayView], gain: float, config: PyOpenColorIO.Config) -> Outcome[npt.NDArray[np.float32]]:
                held = view.default_with(lambda: viewed(scene))
                return msgspec.structs.replace(held, exposure=held.exposure + gain).form(config, plane)

            return paired(chain.view(identity, scene), chain.gain(identity, scene)).bind(lambda parts: exposed(*parts, PyOpenColorIO.GetCurrentConfig()))

        match baseline:
            case Baseline.STORED_RENDER:
                return paired(self.chosen(identity, context), self.shown(identity, context)).bind(
                    lambda found: self.output(identity, found[0][1]).map(lambda plane: Before(plane=plane, name=found[0][0].named(found[0][1]), shown=found[1]))
                )
            case Baseline.VIEW_LAYER:
                return paired(self.beneath(identity, context), rooted(bpy.types.Scene, context), rooted(bpy.types.ViewLayer, context)).bind(
                    lambda found: (
                        combined(str(self.files(identity, found[0][1])[0]), Some(found[2].name))
                        .bind(lambda layer: formed(found[1], layer[1]))
                        .map(lambda plane: Before(plane=plane, name="Before effects", shown=found[0][0].named(found[0][1])))
                    )
                )

    def before(self, identity: Identity) -> Outcome[Option[Before]]:
        """Returns the before frame of the browser's baseline, `Nothing` while it holds no frame."""
        return (
            self
            .browsed(identity, bpy.context)
            .bind(lambda browser: self.side(identity, bpy.context, Baseline[browser.baseline]))
            .map(Some)
            .or_else_with(lambda faults: Ok(Nothing) if all(fault.reason is BlenderReason.ABSENT for fault in faults) else Error(faults))
        )

    def compared(self, identity: Identity, context: bpy.types.Context) -> Outcome[Comparison]:
        """Compares the browser's before frame with the stored output of the frame the editor shows."""
        return (
            self
            .browsed(identity, context)
            .bind(lambda browser: paired(self.side(identity, context, Baseline[browser.baseline]), self.beneath(identity, context).bind(lambda pair: self.output(identity, pair[1]))))
            .bind(lambda planes: Comparison.of(planes[0].plane, planes[1], Domain.DISPLAY, data_range=1.0))
        )

    def difference(self, identity: Identity, metrics: Metrics) -> Outcome[Option[tuple[Tint, ...]]]:
        """Returns the perceptual difference tint in the theme's error ink while difference shows."""
        ink = color(metrics.preferences.themes[0].info.info_error_text)
        return self.browsed(identity, bpy.context).bind(
            lambda browser: self.compared(identity, bpy.context).map(lambda comparison: Some((Tint(plane=comparison.perceptual[..., 0], color=ink),))) if browser.difference else Ok(Nothing)
        )

    @cached_property
    def compare(self) -> CompareView:
        """Before frame over the shown image, redrawn on browser change and each stored render."""
        return CompareView(key=f"{self.key}_compare", glyph=self.glyph, read=lambda identity, _metrics: self.before(identity), kinds=("IMAGE", "RENDER_RESULT"), refreshes=self.refreshes)

    def refreshes(self, _identity: Identity) -> Outcome[tuple[Refresh, ...]]:
        """Returns a refresh per browser field and the stored render signal."""
        return Ok((*(Watch(owner=_Browsing, identifier=name) for name in annotated(_Browsing)), self.signal))

    # --- [STORING]

    def tiled(self, identity: Identity, key: ContentHash, plane: Option[npt.NDArray[np.float32]] = Nothing) -> Job:
        """Constructs a stored render's tile request from a prepared plane or its stored frame at the host preview size."""
        scale = bpy.app.render_preview_size / Metrics.read().unit
        return Job(
            place=(HistoryStore, key),
            key=content_hash(HistoryStore, PreviewKey.read(key, scale)),
            rank=Rank.SHOWN,
            run=lambda: (
                PREVIEWS
                .of(identity)
                .tile(key.hex(), key, lambda side: plane.map(Ok).default_with(lambda: self.output(identity, key)).bind(lambda found: fitted(found, side)).map(lambda pixels: Some(Some(pixels))), scale)
                .map(lambda _: None)
            ),
        )

    def jobs(self, identity: Identity, _context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
        """Returns the tile request of every stale stored render from one history read."""
        previews, scale = PREVIEWS.of(identity), bpy.app.render_preview_size / Metrics.read().unit
        return self.held(identity).map(lambda renders: tuple(self.tiled(identity, entry.value) for entry in renders.history.past if not previews.current(entry.value.hex(), entry.value, scale)))

    def recorded(self, identity: Identity, scene: bpy.types.Scene, path: str) -> Outcome[Option[str]]:
        """Stores a finished render with its settings snapshot under retention, then signals the compare."""
        moment = datetime.datetime.now(datetime.UTC)

        def stored(snapshot: Snapshot, plane: npt.NDArray[np.float32], renders: Renders, retention: Retention) -> Outcome[None]:
            key = content_hash(Renders, snapshot, arrays=(plane,))
            added = msgspec.structs.replace(renders, history=renders.history.add(Dated(moment=moment, value=key), retention))
            frame, values = self.files(identity, key)

            def copied(_: Path) -> Outcome[None]:
                return catch(shutil.copyfile, exception=OSError)(path, frame).map(lambda _: None).map_error(lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=str(frame), cause=error),))

            def published() -> Outcome[None]:
                written = Ok(None) if any(entry.value == key for entry in renders.history.past) else Result[Path, Faults].bind(values.written(snapshot), copied)
                return (
                    written
                    .bind(lambda _: self.kept(identity, renders, added))
                    .bind(lambda _: PreviewQueue.executed(identity, self.tiled(identity, key, Some(plane)), feed=Some(self.jobs)))
                    .bind(lambda _: self.signal.fired())
                )

            return published() if any(entry.value == key for entry in added.history.past) else Ok(None)

        return paired(Snapshot.capture(scene), combined(path).map(lambda found: found[1]), self.held(identity), self.retention(identity)).bind(lambda found: stored(*found)).map(lambda _: Nothing)

    # --- [COMMANDS]

    def revised[T: bpy.types.PropertyGroup](self, change: Callable[[Call[T], Renders, ContentHash], Renders], call: Call[T]) -> Outcome[Answer]:
        """Writes the browsed render's change to the stored renders, then fires the compare signal."""
        return self.chosen(call.identity, call.context).bind(lambda pair: self.kept(call.identity, pair[0], change(call, *pair))).bind(lambda _: self.signal.fired()).map(lambda _: Done())

    @cached_property
    def commands(self) -> tuple[Operator[Empty], Operator[Empty], Operator[Empty], Operator[Empty], Operator[_Noting], Operator[Empty], Operator[Empty], Operator[Empty]]:
        """History commands in section order, each edit firing the compare signal."""

        def restore_settings(call: Call[Empty]) -> Outcome[Answer]:
            """Apply stored settings to scene."""
            return (
                self
                .chosen(call.identity, call.context)
                .bind(lambda pair: paired(self.files(call.identity, pair[1])[1].read(), call.root(bpy.types.Scene)))
                .bind(
                    lambda pair: answered(
                        applied := pair[0].apply(pair[1]), "Scene settings match stored render", (("INFO", f"Restored {len(applied.written)} settings, {len(applied.skipped)} skipped"),)
                    )
                )
            )

        def compare_settings(call: Call[Empty]) -> Outcome[Answer]:
            """Report settings changed since stored render."""

            def noted[V](edit: Edit[V]) -> str:
                match edit:
                    case Reparametrized(key=key, before=before, after=after):
                        return f"{key} changed {', '.join(sorted(before.keys() | after.keys()))}"
                    case Inserted(key=key):
                        return f"{key} added"
                    case Deleted(key=key):
                        return f"{key} removed"
                    case Moved(key=key):
                        return f"{key} moved"

            return (
                self
                .chosen(call.identity, call.context)
                .bind(lambda pair: paired(self.files(call.identity, pair[1])[1].read(), call.root(bpy.types.Scene).bind(Snapshot.capture)))
                .map(lambda snapshots: Done(notes=tuple(("INFO", noted(edit)) for edit in diff(snapshots[0].tree, snapshots[1].tree)) or (("INFO", "Settings match stored render"),)))
            )

        def open_render(call: Call[Empty]) -> Outcome[Answer]:
            """Show stored render."""

            def shown(image: bpy.types.Image) -> Outcome[Answer]:
                return revealed(call.context).bind(
                    lambda found: found.map(
                        lambda located: (
                            EditorState
                            .of(bpy.types.SpaceImageEditor, {"image": IdName.of(image)})
                            .bind(lambda state: state.applied(located.area))
                            .map(lambda _: Done(notes=(("INFO", f"Showing {image.name}"),)))
                        )
                    ).default_value(Ok(Done(notes=(("INFO", "Opened render view, open entry again to show it"),))))
                )

            return self.chosen(call.identity, call.context).bind(lambda pair: loaded(str(self.files(call.identity, pair[1])[0]), Tag(view_as_render=Some(True)))).bind(shown)

        def measure_difference(call: Call[Empty]) -> Outcome[Answer]:
            """Grade stored render against shown frame."""
            return self.compared(call.identity, call.context).map(
                lambda comparison: Done(
                    notes=(*(("INFO", f"{axis.name} {value:.4g}") for axis, value in comparison.measures.items()), ("INFO", f"Verdict {comparison.verdict(self.gates).name.lower()}"))
                )
            )

        def clear_history(call: Call[Empty]) -> Outcome[Answer]:
            """Delete every stored render."""
            return self.held(call.identity).bind(lambda renders: self.kept(call.identity, renders, Renders())).bind(lambda _: self.signal.fired()).map(lambda _: Done())

        def edit_note(call: Call[_Noting], renders: Renders, key: ContentHash) -> Renders:
            """Write stored render note."""
            return msgspec.structs.replace(renders, notes={**renders.notes, key.hex(): call.properties.note})

        def remove_render(_call: Call[Empty], renders: Renders, key: ContentHash) -> Renders:
            """Delete stored render and files."""
            return msgspec.structs.replace(renders, history=renders.history.remove(key))

        def open_history_folder(call: Call[Empty]) -> Outcome[Answer]:
            """Open stored render folder."""
            return called(bpy.ops.wm.path_open, {"filepath": str(self.folder(call.identity))}).map(lambda _: Done())

        return (
            Operator(form=Empty, run=restore_settings),
            Operator(form=Empty, run=compare_settings, undo=False),
            Operator(form=Empty, run=open_render, undo=False),
            Operator(form=Empty, run=measure_difference, undo=False),
            Operator(
                form=_Noting,
                run=update_wrapper(partial(self.revised, edit_note), edit_note),
                invoke=Some(Properties()),
                seed=Some(
                    lambda _properties, _event, identity: (
                        self.chosen(identity, bpy.context).to_option().bind(lambda pair: Option.of_optional(pair[0].notes.get(pair[1].hex()))).map(lambda note: {"note": note}).default_value({})
                    )
                ),
                undo=False,
            ),
            Operator(form=Empty, run=update_wrapper(partial(self.revised, remove_render), remove_render), undo=False),
            Operator(form=Empty, run=clear_history, invoke=Some(Confirm(confirm_text=Some("Clear"), icon="WARNING")), undo=False),
            Operator(form=Empty, undo=False, run=open_history_folder),
        )

    # --- [ROWS]

    def section(self) -> Single:
        """Returns the Render History section."""
        restore, compared, shown, measure, note, remove, clear, folder = self.commands

        def rows(identity: Identity) -> Outcome[tuple[Row, ...]]:
            return self.browsing.bound(identity).map(
                lambda attached: (
                    Property(target=Member(root=bpy.types.WindowManager, path=f"{attached.name}.baseline"), expand=True),
                    IconView(target=Member(root=bpy.types.WindowManager, path=f"{attached.name}.shown"), show_labels=True),
                    Label(text=lambda context: self.chosen(identity, context).to_option().bind(lambda pair: Option.of_optional(pair[0].notes.get(pair[1].hex()))).filter(bool)),
                    Property(target=Member(root=bpy.types.WindowManager, path=f"{attached.name}.difference")),
                    Caption(rows=tuple(OperatorButton(target=command) for command in (restore, compared, shown)), flow=Flow.ALIGNED),
                    Caption(rows=(OperatorButton(target=measure), OperatorButton(target=note)), flow=Flow.ALIGNED),
                    Caption(rows=(OperatorButton(target=remove), OperatorButton(target=clear), OperatorButton(target=folder, text="", icon=Some("FILE_FOLDER"))), flow=Flow.ALIGNED),
                )
            )

        return Single(key="render_history", title=bpy.path.display_name("render_history"), rows=(Rows(rows=rows),))

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Served renders cell."""
        return (_SERVED,)

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Binds the stored render rows of the browser, then returns it with previews, the tile feed and startup tiling, the render read, commands, the compare, and the difference mask."""

        def listed(_owner: Struct, _context: bpy.types.Context | None) -> tuple[_Row, ...]:
            previews = PREVIEWS.of(identity)
            rows = tuple(
                (
                    entry.value.hex(),
                    f"{renders.named(entry.value)}  {entry.moment.astimezone():%H:%M:%S}",
                    renders.notes.get(entry.value.hex(), ""),
                    previews.icon_id(entry.value.hex()).default_value(0),
                    index,
                )
                for renders in identity.reports.reported(self.held)(identity).to_list()
                for index, entry in enumerate(renders.history.past, 1)
            )
            _SERVED.held[identity.package] = msgspec.structs.replace(_SERVED.of(identity), rows=rows)
            return rows

        _Browsing.__annotations__["shown"].keywords["items"] = listed
        return self.refreshes(identity).map(
            lambda refreshes: (
                self.browsing,
                PREVIEWS,
                (Band.RESOURCES, PreviewQueue.fed(identity, self.jobs)),
                ResultRead(consume=lambda scene, path: self.recorded(identity, scene, path)),
                Timer(
                    body=lambda: (
                        self
                        .held(identity)
                        .bind(lambda renders: accumulated(PreviewQueue.executed(identity, self.tiled(identity, entry.value), feed=Some(self.jobs)) for entry in renders.history.past))
                        .map(lambda _: Nothing)
                    )
                ),
                *self.commands,
                self.compare,
                FrameMask(read=lambda metrics: self.difference(identity, metrics), kinds=self.compare.kinds, refreshes=refreshes),
            )
        )


# --- [COMPOSITION] ----------------------------------------------------------------------

tab: Final = Joined.beside(IMAGE_PT_render_slots)
history: Final = HistoryStore(retention=reviewing, glyph=Path("vertical-wipe.svg"))
slots: Final = RenderSlots(render=chain.rendered)
window: Final = ImageWindow(tab=tab.category)
rows: Final = (window, slots, history, Append(target=IMAGE_HT_header, rows=history.compare.header), Panel(section=slots.section(), at=tab), Panel(section=history.section(), at=tab))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Baseline", "HistoryStore", "Renders", "history", "rows", "window"]
