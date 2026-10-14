"""Render history: each completed render stored with its settings, browsed, restored, diffed, compared, and removed."""

from collections.abc import Callable
import datetime
from pathlib import Path
import shutil
from typing import Annotated

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import answered, Call, Confirm, Done, Empty, Operator, Outcome, Properties
from blender.bpy.data.ids.images import Bounded, loaded, ResultRead, Tag
from blender.bpy.data.rna.descriptors import root_in
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderImagingFault, BlenderReadFault, BlenderResult, BlenderWriteFault
from blender.bpy.render.previews.queue import Feed, Job, PreviewQueue, Rank
from blender.bpy.render.review.frames import Before, Bound, CompareView, editor, FrameMask, revealed, showing, Tint
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.properties import Choices, Definition
from blender.bpy.ui.drawing.handlers import Refresh, Watch
from blender.bpy.ui.layout.rows import Caption, Flow, Label, Member, OperatorButton, Property, Row
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.templates import IconView
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.previews import Drawn, Previews
from blender.bpy.ui.resources.theme import color
from blender.imaging.analysis.comparison import Comparison, comparison_of
from blender.imaging.chain import Domain
from blender.imaging.faults import accumulated, ImagingReadFault, paired
from blender.imaging.files.multilayer import Multilayer
from blender.imaging.hashing import content_hash, ContentHash
from blender.rendering.edits import Deleted, diff, Edit, Inserted, Moved, Reparametrized
from blender.rendering.history import Entry, History, Retention

# --- [CHOICES] --------------------------------------------------------------------------

entries = Choices(
    lambda identity, _owner, _context: tuple(
        (entry.key.hex(), f"#{index}  {entry.moment.astimezone():%H:%M:%S}", entry.value, Previews(identity=identity).icon_id(entry.key.hex()).default_value(0), index)
        for history in identity.reports.reported(held)(identity).to_list()
        for index, entry in enumerate(history.entries, 1)
    )
)


# --- [MODELS] ---------------------------------------------------------------------------


class Reviewing(msgspec.Struct, frozen=True, kw_only=True):
    """Render history retention."""

    retention: Annotated[Retention, msgspec.Meta(title="Render History", description="Completed renders kept, newest first")] = Retention(interval=0.0)


class Browsing(msgspec.Struct, frozen=True, kw_only=True):
    """Session view state of the history browser."""

    shown: Annotated[str, msgspec.Meta(title="Before", description="Stored render the compare draws as the before frame"), entries] = ""
    difference: Annotated[bool, msgspec.Meta(title="Difference", description="Tint each pixel by its perceived difference from the before frame")] = False
    newest: Annotated[str, msgspec.Meta(title="Newest", description="Key of the newest stored render"), Definition(options=frozenset({"HIDDEN"}))] = ""


class Noting(msgspec.Struct, frozen=True, kw_only=True):
    """Note on a stored render."""

    note: Annotated[str, msgspec.Meta(title="Note", description="Note on the stored render")] = ""


class HistoryCommands(msgspec.Struct, frozen=True, kw_only=True):
    """Operator rows of the render history, one field per row a section places."""

    restore: Operator[Empty]
    diff: Operator[Empty]
    open: Operator[Empty]
    measure: Operator[Empty]
    note: Operator[Noting]
    remove: Operator[Empty]
    clear: Operator[Empty]
    folder: Operator[Empty]


class HistoryStore(msgspec.Struct, frozen=True, kw_only=True):
    """Row keeping each completed render with its settings, its browser state, operators, compare, and difference mask."""

    settings: Callable[[Identity], BlenderResult[Reviewing]]
    glyph: Glyph
    scale: float

    @property
    def compare(self) -> CompareView:
        """Compare of the shown entry over the editor's frame, refreshed on each browser field."""
        return CompareView(key="history_compare", glyph=self.glyph, read=lambda identity, _: before(identity), kinds=("IMAGE", "RENDER_RESULT"), refreshes=watched)

    @property
    def page_rows(self) -> tuple[Registrable, ...]:
        """Browser state and the compare state, which the preferences page acquires."""
        return (browsing, self.compare.state)

    def section(self) -> Single:
        """Render History section: the Before icon view, the shown entry's note, the difference toggle, and the operator buttons."""

        def rows(identity: Identity) -> BlenderResult[tuple[Row, ...]]:
            commands = self.operators(identity)
            shown = next(field for field in msgspec.structs.fields(Browsing) if field.name == "shown")
            difference = next(field for field in msgspec.structs.fields(Browsing) if field.name == "difference")

            def noted(context: bpy.types.Context) -> Option[str]:
                return chosen(identity, context).to_option().map(lambda pair: pair[1].value).filter(bool)

            def grouped(*rows: OperatorButton) -> Caption:
                return Caption(rows=rows, flow=Flow.ALIGNED)

            return browsing.bound(identity).map(
                lambda attached: (
                    IconView(choice=Member(root=bpy.types.WindowManager, path=f"{attached.name}.{shown.name}"), scale=self.scale, show_labels=True),
                    Label(text=noted),
                    Property(target=Member(root=bpy.types.WindowManager, path=f"{attached.name}.{difference.name}")),
                    grouped(*(OperatorButton(target=command) for command in (commands.restore, commands.diff, commands.open))),
                    grouped(OperatorButton(target=commands.measure), OperatorButton(target=commands.note)),
                    grouped(OperatorButton(target=commands.remove), OperatorButton(target=commands.clear), OperatorButton(target=commands.folder, text=Some(""), icon=Some("FILE_FOLDER"))),
                )
            )

        return Single(key="history", title="Render History", rows=(Bound(rows=rows),))

    def operators(self, identity: Identity) -> HistoryCommands:
        """Builds the history's operator rows over one identity."""

        def restored(call: Call[Empty]) -> BlenderResult[Outcome]:
            return (
                chosen(identity, call.context)
                .bind(lambda pair: paired(decoded(files(identity, pair[1].key)[1], Snapshot), call.root(bpy.types.Scene)))
                .bind(
                    lambda pair: answered(
                        applied := pair[0].apply(pair[1]), "Scene settings already match the stored render", (("INFO", f"Restored {len(applied.written)} settings, {len(applied.skipped)} skipped"),)
                    )
                )
            )

        def diffed(call: Call[Empty]) -> BlenderResult[Done]:
            def noted[V](edit: Edit[V]) -> str:
                match edit:
                    case Reparametrized(key=key, before=before, after=after):
                        return f"{key}: {', '.join(sorted(before.keys() | after.keys()))}"
                    case Inserted(key=key):
                        return f"{key}: added"
                    case Deleted(key=key):
                        return f"{key}: removed"
                    case Moved(key=key):
                        return f"{key}: moved"

            return (
                chosen(identity, call.context)
                .bind(lambda pair: paired(decoded(files(identity, pair[1].key)[1], Snapshot), call.root(bpy.types.Scene).bind(Snapshot.capture)))
                .map(lambda snapshots: Done(notes=tuple(("INFO", noted(edit)) for edit in diff(snapshots[0].tree, snapshots[1].tree)) or (("INFO", "Settings match the stored render"),)))
            )

        def opened(call: Call[Empty]) -> BlenderResult[Done]:
            def shown(image: bpy.types.Image) -> BlenderResult[Done]:
                return revealed(call.context).bind(
                    lambda found: found.map(
                        lambda located: showing(image).bind(lambda state: state.applied(located.area)).map(lambda _: Done(notes=(("INFO", f"Showing {image.name}"),)))
                    ).default_value(Ok(Done(notes=(("INFO", "Opened the render view; open the entry again to show it there"),))))
                )

            return chosen(identity, call.context).bind(lambda pair: loaded(str(files(identity, pair[1].key)[0]), Tag(view_as_render=Some(True)))).bind(shown)

        def measured(call: Call[Empty]) -> BlenderResult[Done]:
            return compared(identity, call.context).map(
                lambda comparison: Done(
                    notes=(*(("INFO", f"{axis.label} {value:.4g}") for axis, value in comparison.measures.items()), ("INFO", f"Verdict {comparison.verdict(Comparison.bars).label}"))
                )
            )

        def noting(call: Call[Noting]) -> BlenderResult[Done]:
            return (
                chosen(identity, call.context)
                .bind(
                    lambda pair: kept(
                        identity,
                        pair[0],
                        msgspec.structs.replace(pair[0], entries=tuple(msgspec.structs.replace(entry, value=call.record.note) if entry.key == pair[1].key else entry for entry in pair[0].entries)),
                    )
                )
                .map(lambda _: Done())
            )

        def removed(call: Call[Empty]) -> BlenderResult[Done]:
            return chosen(identity, call.context).bind(lambda pair: kept(identity, pair[0], pair[0].remove(pair[1].key))).map(lambda _: Done())

        def cleared(_call: Call[Empty]) -> BlenderResult[Done]:
            return held(identity).bind(lambda history: kept(identity, history, msgspec.structs.replace(history, entries=()))).map(lambda _: Done())

        return HistoryCommands(
            restore=Operator(key="history_restore", title="Restore Settings", description="Apply the stored render's settings to the scene", form=Empty, run=restored),
            diff=Operator(key="history_diff", title="Compare Settings", description="Report the settings the scene changed since the stored render", form=Empty, run=diffed, undo=False),
            open=Operator(key="history_open", title="Open Render", description="Show the stored render in the render view", form=Empty, run=opened, undo=False),
            measure=Operator(key="history_measure", title="Measure Difference", description="Grade the stored render against the shown frame", form=Empty, run=measured, undo=False),
            note=Operator(
                key="history_note",
                title="Edit Note",
                description="Write a note on the stored render",
                form=Noting,
                run=noting,
                invoke=Some(Properties()),
                seed=Some(lambda record, _event: chosen(identity, bpy.context).map(lambda pair: Noting(note=pair[1].value)).default_value(record)),
                undo=False,
            ),
            remove=Operator(key="history_remove", title="Remove Render", description="Delete the stored render and its files", form=Empty, run=removed, undo=False),
            clear=Operator(
                key="history_clear",
                title="Clear History",
                description="Delete every stored render and its files",
                form=Empty,
                run=cleared,
                invoke=Some(Confirm(confirm_text=Some("Clear"), icon="WARNING")),
                undo=False,
            ),
            folder=Operator(
                key="history_folder",
                title="Open History Folder",
                description="Open the folder holding the stored renders",
                form=Empty,
                undo=False,
                run=lambda _: called(bpy.ops.wm.path_open, {"filepath": str(folder(identity))}).map(lambda _: Done()),
            ),
        )

    def jobs(self, identity: Identity, _context: bpy.types.Context) -> BlenderResult[tuple[Job, ...]]:
        """Returns every stale tile request from one history read, withdrawing omitted requests."""
        previews = Previews(identity=identity)
        return held(identity).map(lambda history: tuple(tiled(identity, entry.key, self.scale) for entry in history.entries if not previews.current(entry.key.hex(), entry.key, self.scale)))

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Answers preview and feed resources beside the retained native declarations."""
        return watched(identity).map(
            lambda refreshes: (
                (Band.RESOURCES, Previews(identity=identity)),
                (Band.RESOURCES, PreviewQueue.fed(identity, self.jobs)),
                ResultRead(consume=lambda scene, path: recorded(identity, self.settings, self.scale, self.jobs, scene, path)),
                Timer(
                    body=lambda: (
                        held(identity)
                        .bind(lambda history: accumulated(PreviewQueue.executed(identity, tiled(identity, entry.key, self.scale), feed=Some(self.jobs)) for entry in history.entries))
                        .map(lambda _: Nothing)
                    )
                ),
                *msgspec.structs.astuple(self.operators(identity)),
                self.compare,
                FrameMask(read=lambda _: difference(identity), kinds=("IMAGE", "RENDER_RESULT"), refreshes=refreshes),
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def folder(identity: Identity) -> Path:
    """Answers the history folder under the extension's user folder."""
    return identity.folder / "history"


def files(identity: Identity, key: ContentHash) -> tuple[Path, Path]:
    """Answers an entry's frame and snapshot paths."""
    stem = folder(identity) / key.hex()
    return stem.with_suffix(".exr"), stem.with_suffix(".json")


def decoded[T](path: Path, form: type[T]) -> BlenderResult[T]:
    """Decodes a JSON file into a record."""
    try:
        value = msgspec.json.decode(path.read_bytes(), type=form, strict=True)
    except (msgspec.DecodeError, OSError) as error:
        return Error((BlenderReadFault(request=str(path), error=error),))
    return Ok(value)


def held(identity: Identity) -> BlenderResult[History[str]]:
    """Reads the history, empty where no file stands."""
    path = folder(identity) / "history.json"
    return decoded(path, History[str]).or_else_with(
        lambda faults: Ok(History[str]()) if all(isinstance(fault, BlenderReadFault) and isinstance(fault.error, FileNotFoundError) for fault in faults) else Error(faults)
    )


def kept(identity: Identity, before: History[str], after: History[str]) -> BlenderResult[None]:
    """Writes a history, then deletes the files and tiles of each entry it dropped."""
    path, dropped = folder(identity) / "history.json", {entry.key for entry in before.entries} - {entry.key for entry in after.entries}

    def written() -> None:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(msgspec.json.encode(after))
        for key in dropped:
            for file in files(identity, key):
                file.unlink(missing_ok=True)

    published: BlenderResult[None] = catch(written, exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=str(path), error=error),))
    return Result[None, BlenderFaults].bind(published, lambda _: accumulated(Previews(identity=identity).dropped(key.hex()) for key in dropped).map(lambda _: None))


def composite(path: Path) -> BlenderResult[npt.NDArray[np.float32]]:
    """Reads the plane a stored frame displays: Blender's first Combined layer, the composite ahead of the view layers, or the sequencer's unnamed plane."""
    name = "Combined"

    def selected(file: Multilayer) -> BlenderResult[npt.NDArray[np.float32]]:
        match Block.of_seq(file.layers).filter(lambda layer: layer.name.rpartition(".")[2] in {name, ""}).try_head():
            case Option(tag="some", some=layer):
                return file.plane(layer, layer.channels).map_error(lambda faults: (BlenderImagingFault(faults=faults),))
            case Option():
                return Error((BlenderAbsentFault(owner=str(path), key=name),))

    file: BlenderResult[Multilayer] = Multilayer.read(str(path)).map_error(lambda faults: (BlenderImagingFault(faults=faults),))
    return Result[Multilayer, BlenderFaults].bind(file, selected)


def tiled(identity: Identity, key: ContentHash, scale: float, plane: Option[npt.NDArray[np.float32]] = Nothing) -> Job:
    """Constructs a tile request from a prepared plane or its stored frame."""

    def fitted(side: int) -> BlenderResult[Option[Option[npt.NDArray[np.float32]]]]:
        def resized(found: npt.NDArray[np.float32]) -> BlenderResult[npt.NDArray[np.float32]]:
            def pixels(buffer: OpenImageIO.ImageBuf) -> BlenderResult[npt.NDArray[np.float32]]:
                return (
                    Error((BlenderImagingFault(faults=(ImagingReadFault(request=str(files(identity, key)[0]), error=buffer.geterror()),)),))
                    if buffer.has_error
                    else Option.of_optional(buffer.get_pixels(OpenImageIO.FLOAT)).to_result_with(
                        lambda: (BlenderImagingFault(faults=(ImagingReadFault(request=str(files(identity, key)[0]), error=buffer.geterror()),)),)
                    )
                )

            return (
                Bounded(pixels=side)
                .fitted(found.shape[1], found.shape[0])
                .map(
                    lambda size: pixels(
                        OpenImageIO.ImageBufAlgo.fit(OpenImageIO.ImageBuf(found), filtername="", filterwidth=0.0, fillmode="letterbox", exact=True, roi=OpenImageIO.ROI(0, size[0], 0, size[1]))
                    )
                )
                .default_value(Ok(found))
            )

        return plane.map(Ok).default_with(lambda: composite(files(identity, key)[0])).bind(resized).map(lambda pixels: Some(Some(pixels)))

    return Job(
        place=(HistoryStore, key),
        key=content_hash(HistoryStore, Drawn.read(key, scale)),
        rank=Rank.SHOWN,
        run=lambda: Previews(identity=identity).tile(key.hex(), key, fitted, scale).map(lambda _: None),
    )


def recorded(identity: Identity, settings: Callable[[Identity], BlenderResult[Reviewing]], scale: float, feed: Feed, scene: bpy.types.Scene, path: str) -> BlenderResult[Option[str]]:
    """Stores a completed render with its settings snapshot under the retention policy."""
    moment = datetime.datetime.now(datetime.UTC)

    def stored(manager: bpy.types.WindowManager, snapshot: Snapshot, plane: npt.NDArray[np.float32], history: History[str], reviewing: Reviewing, attached: Attached[Browsing]) -> BlenderResult[None]:
        entry = Entry(key=content_hash(Entry, snapshot, arrays=(plane,)), moment=moment, value="")
        added, (frame, values) = history.add(entry, reviewing.retention), files(identity, entry.key)

        def copied() -> None:
            frame.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(path, frame)
            values.write_bytes(msgspec.json.encode(snapshot))

        if added is history:
            return Ok(None)
        copied_frame: BlenderResult[None] = catch(copied, exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=str(frame), error=error),))
        return (
            Result[None, BlenderFaults]
            .bind(copied_frame, lambda _: kept(identity, history, added))
            .bind(lambda _: PreviewQueue.executed(identity, tiled(identity, entry.key, scale, Some(plane)), feed=Some(feed)))
            .bind(lambda _: attached.read(manager).bind(lambda browsed: attached.write(manager, msgspec.structs.replace(browsed, newest=entry.key.hex()))))
        )

    return (
        paired(
            root_in(bpy.types.WindowManager, bpy.context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(bpy.context), key="window_manager"),)),
            paired(Snapshot.capture(scene), paired(paired(composite(Path(path)), held(identity)), paired(settings(identity), browsing.bound(identity)))),
        )
        .bind(lambda values: stored(values[0], values[1][0], *values[1][1][0], *values[1][1][1]))
        .map(lambda _: Nothing)
    )


def chosen(identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[History[str], Entry[str]]]:
    """Answers the history and the entry the browser shows, `absent` where the history holds no shown entry."""
    return paired(
        paired(browsing.bound(identity), root_in(bpy.types.WindowManager, context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="window_manager"),))).bind(
            lambda pair: pair[0].read(pair[1])
        ),
        held(identity),
    ).bind(
        lambda pair: (
            Block
            .of_seq(pair[1].entries)
            .filter(lambda entry: entry.key.hex() == pair[0].shown)
            .try_head()
            .map(lambda entry: (pair[1], entry))
            .to_result_with(lambda: (BlenderAbsentFault(owner=str(folder(identity) / "history.json"), key=pair[0].shown),))
        )
    )


def watched(identity: Identity) -> BlenderResult[tuple[Refresh, ...]]:
    """Answers a refresh on each browser field of the identity's attachment."""
    return browsing.bound(identity).map(lambda attached: tuple(Watch(owner=attached.group.struct, identifier=row.name) for row in msgspec.structs.fields(Browsing)))


def before(identity: Identity) -> BlenderResult[Option[Before]]:
    """Answers the shown entry's composited plane captioned with its number, `Nothing` while no entry shows."""
    return (
        chosen(identity, bpy.context)
        .map(Some)
        .or_else_with(lambda faults: Ok(Nothing) if all(isinstance(fault, BlenderAbsentFault) for fault in faults) else Error(faults))
        .bind(
            lambda found: found.map(lambda pair: composite(files(identity, pair[1].key)[0]).map(lambda plane: Some(Before(plane=plane, name=f"#{pair[0].entries.index(pair[1]) + 1}")))).default_value(
                Ok(Nothing)
            )
        )
    )


def difference(identity: Identity) -> BlenderResult[Option[tuple[Tint, ...]]]:
    """Answers the perceptual difference tint of the shown entry against the shown frame while the browser's difference toggle holds."""
    context = bpy.context
    ink = color(context.preferences.themes[0].info.info_error_text)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    return (
        browsing
        .bound(identity)
        .bind(
            lambda attached: Result[bpy.types.WindowManager, BlenderFaults].bind(
                root_in(bpy.types.WindowManager, context).to_result_with(lambda: (BlenderAbsentFault(owner=repr(context), key="window_manager"),)), attached.read
            )
        )
        .bind(lambda browsed: compared(identity, context).map(lambda comparison: Some((Tint(plane=comparison.perceptual[..., 0], color=ink),))) if browsed.difference else Ok(Nothing))
    )


def compared(identity: Identity, context: bpy.types.Context) -> BlenderResult[Comparison]:
    """Compares the shown entry's frame with the frame the editor shows."""

    def beneath(history: History[str]) -> BlenderResult[Entry[str]]:
        def shown(space: bpy.types.SpaceImageEditor) -> BlenderResult[Entry[str]]:
            return (
                Option
                .of_optional(space.image)
                .bind(
                    lambda image: (
                        Block.of_seq(history.entries).try_head()
                        if image.type == "RENDER_RESULT"
                        else Block.of_seq(history.entries).filter(lambda entry: files(identity, entry.key)[0] == Path(bpy.path.abspath(image.filepath, library=image.library))).try_head()
                    )
                )
                .to_result_with(lambda: (BlenderAbsentFault(owner=str(folder(identity) / "history.json"), key=repr(space)),))
            )

        return editor(context).bind(shown)

    return (
        chosen(identity, context)
        .bind(lambda pair: beneath(pair[0]).map(lambda below: (pair[1], below)))
        .bind(lambda entries: paired(composite(files(identity, entries[0].key)[0]), composite(files(identity, entries[1].key)[0])))
        .bind(lambda planes: comparison_of(planes[0], planes[1], Domain.DISPLAY, data_range=1.0).map_error(lambda faults: (BlenderImagingFault(faults=faults),)))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

browsing = Attachment(key="history", form=Browsing, owner=bpy.types.WindowManager)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Browsing", "HistoryCommands", "HistoryStore", "Noting", "Reviewing", "before", "browsing", "difference", "entries", "recorded", "watched"]
