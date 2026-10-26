"""Finished renders stored with their settings snapshots."""

from collections.abc import Callable
from contextlib import nullcontext
import datetime
from functools import cached_property
from pathlib import Path
import shutil
from typing import Annotated, Final, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import answered, Call, Confirm, Done, Empty, Operator, Outcome, Properties
from blender.bpy.data.ids.images import loaded, Tag
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import rooted
from blender.bpy.data.rna.edits import Deleted, diff, Edit, Inserted, Moved, Reparametrized
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, Registrable
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.render.effects.formation import managed
from blender.bpy.render.effects.stack import EffectStack
from blender.bpy.render.outputs.exports import combined, Source
from blender.bpy.render.outputs.results import ResultRead
from blender.bpy.render.previews.queue import Job, PreviewQueue, Rank
from blender.bpy.render.previews.tiles import fitted
from blender.bpy.render.review.frames import Before, CompareView, FrameMask, revealed, Tint
from blender.bpy.settings.attachments import Attachment
from blender.bpy.settings.history import Retention
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.drawing.handlers import Refresh, Signal, Watch
from blender.bpy.ui.layout.rows import Caption, Flow, Label, Member, OperatorButton, Property, Row, Rows
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.templates import IconView
from blender.bpy.ui.resources.icons import Glyph
from blender.bpy.ui.resources.previews import Drawn, PREVIEWS
from blender.bpy.ui.resources.theme import color
from blender.bpy.ui.screens.editors import EditorState
from blender.imaging.analysis.comparison import Comparison
from blender.imaging.chain import Domain
from blender.imaging.faults import Access, accumulated, ImagingFileFault, paired
from blender.imaging.files.records import Record
from blender.imaging.hashing import content_hash, ContentHash

# --- [CHOICES] --------------------------------------------------------------------------

entries = Choices(
    lambda identity, _owner, _context: tuple(
        (entry.key.hex(), f"{named(history, entry)}  {entry.moment.astimezone():%H:%M:%S}", entry.value, PREVIEWS.of(identity).icon_id(entry.key.hex()).default_value(0), index)
        for history in identity.reports.reported(held)(identity).to_list()
        for index, entry in enumerate(history.entries, 1)
    )
)


# --- [MODELS] ---------------------------------------------------------------------------


class Entry[T](msgspec.Struct, frozen=True, kw_only=True):
    """Value keyed by content hash with add moment."""

    key: ContentHash
    moment: Annotated[datetime.datetime, msgspec.Meta(tz=True)]
    value: T


class History[T](msgspec.Struct, frozen=True, kw_only=True):
    """Entries newest first with evicted and replaced counts."""

    entries: tuple[Entry[T], ...] = ()
    shed: Annotated[int, msgspec.Meta(ge=0)] = 0
    lost: Annotated[int, msgspec.Meta(ge=0)] = 0

    def add(self, entry: Entry[T], retention: Retention) -> Self:
        """Adds entry unless key is held, replacing newest within merge interval."""
        origin = datetime.datetime.min.replace(tzinfo=datetime.UTC)
        match self.entries:
            case _ if any(present.key == entry.key for present in self.entries):
                return self
            case (head, *rest) if 0 <= ((entry.moment - origin) - (head.moment - origin)).total_seconds() < retention.interval:
                held, lost = (entry, *rest), self.lost + 1
            case _:
                held, lost = (entry, *self.entries), self.lost
        kept = held[: retention.capacity]
        return msgspec.structs.replace(self, entries=kept, shed=self.shed + len(held) - len(kept), lost=lost)

    def remove(self, key: ContentHash) -> Self:
        """Removes key, counts unchanged."""
        return msgspec.structs.replace(self, entries=tuple(entry for entry in self.entries if entry.key != key))

    def step(self, key: ContentHash, offset: int) -> Option[Entry[T]]:
        """Returns entry offset from held key, Nothing past either end."""
        return (
            Option
            .of_optional(next((index for index, entry in enumerate(self.entries) if entry.key == key), None))
            .map(lambda index: index + offset)
            .filter(lambda index: 0 <= index < len(self.entries))
            .map(self.entries.__getitem__)
        )


class Browsing(msgspec.Struct, frozen=True, kw_only=True):
    """History browser session state."""

    source: Annotated[Source, msgspec.Meta(title="Before Frame", description="Stored render, or the shown frame's view layer ahead of scene effects")] = Source.RENDER
    shown: Annotated[str, msgspec.Meta(title="Before", description="Stored render drawn as before frame"), entries] = ""
    difference: Annotated[bool, msgspec.Meta(title="Difference", description="Tint pixels by perceived difference from before frame")] = False


class Noting(msgspec.Struct, frozen=True, kw_only=True):
    """Note on a stored render."""

    note: Annotated[str, msgspec.Meta(title="Note", description="Stored render note")] = ""


class Served(msgspec.Struct, kw_only=True):
    """History an extension last read or wrote."""

    history: Option[History[str]] = Nothing

    def filled(self, history: History[str]) -> History[str]:
        """Holds history as the served one."""
        self.history = Some(history)
        return history


class HistoryCommands(msgspec.Struct, frozen=True, kw_only=True):
    """History operators, one field per section button."""

    restore: Operator[Empty]
    diff: Operator[Empty]
    open: Operator[Empty]
    measure: Operator[Empty]
    note: Operator[Noting]
    remove: Operator[Empty]
    clear: Operator[Empty]
    folder: Operator[Empty]

    @classmethod
    def of(cls, stack: EffectStack, signal: Signal) -> Self:
        """Builds operators binding the calling extension, the stack forming view layer frames and the signal redrawing compares after each history edit."""

        def revised[T: msgspec.Struct](change: Callable[[Call[T], History[str], Entry[str]], History[str]]) -> Callable[[Call[T]], BlenderResult[Done]]:
            return lambda call: chosen(call.identity, call.context).bind(lambda pair: kept(call.identity, pair[0], change(call, *pair))).bind(lambda _: signal.fired()).map(lambda _: Done())

        def restored(call: Call[Empty]) -> BlenderResult[Outcome]:
            return (
                chosen(call.identity, call.context)
                .bind(lambda pair: paired(files(call.identity, pair[1].key)[1].read(), call.root(bpy.types.Scene)))
                .bind(
                    lambda pair: answered(
                        applied := pair[0].apply(pair[1]), "Scene settings match stored render", (("INFO", f"Restored {len(applied.written)} settings, {len(applied.skipped)} skipped"),)
                    )
                )
            )

        def diffed(call: Call[Empty]) -> BlenderResult[Done]:
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
                chosen(call.identity, call.context)
                .bind(lambda pair: paired(files(call.identity, pair[1].key)[1].read(), call.root(bpy.types.Scene).bind(Snapshot.capture)))
                .map(lambda snapshots: Done(notes=tuple(("INFO", noted(edit)) for edit in diff(snapshots[0].tree, snapshots[1].tree)) or (("INFO", "Settings match stored render"),)))
            )

        def opened(call: Call[Empty]) -> BlenderResult[Done]:
            def shown(image: bpy.types.Image) -> BlenderResult[Done]:
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

            return chosen(call.identity, call.context).bind(lambda pair: loaded(str(files(call.identity, pair[1].key)[0]), Tag(view_as_render=Some(True)))).bind(shown)

        def measured(call: Call[Empty]) -> BlenderResult[Done]:
            return compared(call.identity, stack, call.context).map(
                lambda comparison: Done(
                    notes=(*(("INFO", f"{axis.label} {value:.4g}") for axis, value in comparison.measures.items()), ("INFO", f"Verdict {comparison.verdict(Comparison.bars).label}"))
                )
            )

        def cleared(call: Call[Empty]) -> BlenderResult[Done]:
            return held(call.identity).bind(lambda history: kept(call.identity, history, msgspec.structs.replace(history, entries=()))).bind(lambda _: signal.fired()).map(lambda _: Done())

        return cls(
            restore=Operator(key="history_restore", title="Restore Settings", description="Apply stored settings to scene", form=Empty, run=restored),
            diff=Operator(key="history_diff", title="Compare Settings", description="Report settings changed since stored render", form=Empty, run=diffed, undo=False),
            open=Operator(key="history_open", title="Open Render", description="Show stored render", form=Empty, run=opened, undo=False),
            measure=Operator(key="history_measure", title="Measure Difference", description="Grade stored render against shown frame", form=Empty, run=measured, undo=False),
            note=Operator(
                key="history_note",
                title="Edit Note",
                description="Write stored render note",
                form=Noting,
                run=revised(
                    lambda call, history, chosen: msgspec.structs.replace(
                        history, entries=tuple(msgspec.structs.replace(entry, value=call.record.note) if entry.key == chosen.key else entry for entry in history.entries)
                    )
                ),
                invoke=Some(Properties()),
                seed=Some(lambda record, _event, identity: chosen(identity, bpy.context).map(lambda pair: Noting(note=pair[1].value)).default_value(record)),
                undo=False,
            ),
            remove=Operator(
                key="history_remove",
                title="Remove Render",
                description="Delete stored render and files",
                form=Empty,
                run=revised(lambda _call, history, chosen: history.remove(chosen.key)),
                undo=False,
            ),
            clear=Operator(
                key="history_clear",
                title="Clear History",
                description="Delete every stored render",
                form=Empty,
                run=cleared,
                invoke=Some(Confirm(confirm_text=Some("Clear"), icon="WARNING")),
                undo=False,
            ),
            folder=Operator(
                key="history_folder",
                title="Open History Folder",
                description="Open stored render folder",
                form=Empty,
                undo=False,
                run=lambda call: called(bpy.ops.wm.path_open, {"filepath": str(folder(call.identity))}).map(lambda _: Done()),
            ),
        )


class HistoryStore(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """History row with browser, operators, compare, and difference mask."""

    stack: EffectStack
    retention: Callable[[Identity], BlenderResult[Retention]]
    glyph: Glyph
    scale: float
    signal: Signal = msgspec.field(default_factory=Signal)

    @cached_property
    def commands(self) -> HistoryCommands:
        """History operators over the stack, each edit firing the compare signal."""
        return HistoryCommands.of(self.stack, self.signal)

    @cached_property
    def compare(self) -> CompareView:
        """Shown entry over editor frame, redrawn on browser change and each stored render."""
        return CompareView(key="history_compare", glyph=self.glyph, read=lambda identity, _: before(identity, self.stack), kinds=("IMAGE", "RENDER_RESULT"), refreshes=self.refreshes)

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Browser attachment refresh watches bind, and the served history cell."""
        return (browsing, SERVED)

    def refreshes(self, identity: Identity) -> BlenderResult[tuple[Refresh, ...]]:
        """Returns a refresh per browser field and the stored render signal."""
        return browsing.bound(identity).map(lambda attached: (*(Watch(owner=attached.group.struct, identifier=row.name) for row in msgspec.structs.fields(Browsing)), self.signal))

    def section(self) -> Single:
        """Returns Render History section."""
        commands = self.commands
        source, shown, difference = Browsing.__struct_fields__

        def grouped(*rows: OperatorButton) -> Caption:
            return Caption(rows=rows, flow=Flow.ALIGNED)

        def rows(identity: Identity) -> BlenderResult[tuple[Row, ...]]:
            return browsing.bound(identity).map(
                lambda attached: (
                    Property(target=Member(root=bpy.types.WindowManager, path=f"{attached.name}.{source}"), expand=True),
                    IconView(target=Member(root=bpy.types.WindowManager, path=f"{attached.name}.{shown}"), scale=self.scale, show_labels=True),
                    Label(text=lambda context: chosen(identity, context).to_option().map(lambda pair: pair[1].value).filter(bool)),
                    Property(target=Member(root=bpy.types.WindowManager, path=f"{attached.name}.{difference}")),
                    grouped(*(OperatorButton(target=command) for command in (commands.restore, commands.diff, commands.open))),
                    grouped(OperatorButton(target=commands.measure), OperatorButton(target=commands.note)),
                    grouped(OperatorButton(target=commands.remove), OperatorButton(target=commands.clear), OperatorButton(target=commands.folder, text="", icon=Some("FILE_FOLDER"))),
                )
            )

        return Single(key="history", title="Render History", rows=(Rows(rows=rows),))

    def jobs(self, identity: Identity, _context: bpy.types.Context) -> BlenderResult[tuple[Job, ...]]:
        """Returns every stale tile request from one history read, withdrawing omitted requests."""
        previews = PREVIEWS.of(identity)
        return held(identity).map(lambda history: tuple(tiled(identity, entry.key, self.scale) for entry in history.entries if not previews.current(entry.key.hex(), entry.key, self.scale)))

    def recorded(self, identity: Identity, scene: bpy.types.Scene, path: str) -> BlenderResult[Option[str]]:
        """Stores finished render with settings snapshot under retention, then signals the compare."""
        moment = datetime.datetime.now(datetime.UTC)

        def stored(snapshot: Snapshot, plane: npt.NDArray[np.float32], history: History[str], retention: Retention) -> BlenderResult[None]:
            entry = Entry(key=content_hash(Entry, snapshot, arrays=(plane,)), moment=moment, value="")
            added, (frame, values) = history.add(entry, retention), files(identity, entry.key)

            def copied(_: Path) -> BlenderResult[None]:
                return catch(shutil.copyfile, exception=OSError)(path, frame).map(lambda _: None).map_error(lambda error: (ImagingFileFault(access=Access.WRITE, request=frame, error=error),))

            def published() -> BlenderResult[None]:
                return (
                    Result[Path, BlenderFaults]
                    .bind(values.written(snapshot), copied)
                    .bind(lambda _: kept(identity, history, added))
                    .bind(lambda _: PreviewQueue.executed(identity, tiled(identity, entry.key, self.scale, Some(plane)), feed=Some(self.jobs)))
                    .bind(lambda _: self.signal.fired())
                )

            return Ok(None) if added is history else published()

        return (
            paired(Snapshot.capture(scene), combined(path, Source.RENDER).map(lambda found: found[1]), held(identity), self.retention(identity))
            .bind(lambda found: stored(*found))
            .map(lambda _: Nothing)
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns preview and feed resources beside native declarations."""
        return self.refreshes(identity).map(
            lambda refreshes: (
                PREVIEWS,
                (Band.RESOURCES, PreviewQueue.fed(identity, self.jobs)),
                ResultRead(consume=lambda scene, path: self.recorded(identity, scene, path)),
                Timer(
                    body=lambda: (
                        held(identity)
                        .bind(lambda history: accumulated(PreviewQueue.executed(identity, tiled(identity, entry.key, self.scale), feed=Some(self.jobs)) for entry in history.entries))
                        .map(lambda _: Nothing)
                    )
                ),
                *msgspec.structs.astuple(self.commands),
                self.compare,
                FrameMask(read=lambda _: difference(identity, self.stack), kinds=("IMAGE", "RENDER_RESULT"), refreshes=refreshes),
            )
        )


# --- [SERVICES] -------------------------------------------------------------------------

SERVED: Final = Scoped[Served](made=lambda _identity: nullcontext(Served()))

# --- [OPERATIONS] -----------------------------------------------------------------------


def folder(identity: Identity) -> Path:
    """Returns history folder under extension user folder."""
    return identity.folder / "history"


def record(identity: Identity) -> Record[History[str]]:
    """Returns the history file record."""
    return Record(path=folder(identity) / "history.json", form=History[str])


def files(identity: Identity, key: ContentHash) -> tuple[Path, Record[Snapshot]]:
    """Returns stored frame path and settings snapshot record."""
    stem = folder(identity) / key.hex()
    return stem.with_suffix(".exr"), Record(path=stem.with_suffix(".json"), form=Snapshot)


def output(identity: Identity, key: ContentHash) -> BlenderResult[npt.NDArray[np.float32]]:
    """Returns a stored frame's render output."""
    return combined(str(files(identity, key)[0]), Source.RENDER).map(lambda found: found[1])


def held(identity: Identity) -> BlenderResult[History[str]]:
    """Returns served history, read from its file on first use and empty without one."""
    served = SERVED.of(identity)

    def absent(faults: BlenderFaults) -> BlenderResult[History[str]]:
        match faults:
            case (ImagingFileFault(error=FileNotFoundError()),):
                return Ok(History[str]())
            case _:
                return Error(faults)

    return served.history.map(Ok).default_with(lambda: Result[History[str], BlenderFaults].or_else_with(record(identity).read(), absent).map(served.filled))


def kept(identity: Identity, before: History[str], after: History[str]) -> BlenderResult[None]:
    """Writes and serves history, then deletes files and tiles of each dropped entry."""
    dropped = {entry.key for entry in before.entries} - {entry.key for entry in after.entries}

    def unlinked(path: Path) -> BlenderResult[None]:
        return catch(path.unlink, exception=OSError)(missing_ok=True).map_error(lambda error: (ImagingFileFault(access=Access.WRITE, request=path, error=error),))

    def erased(key: ContentHash) -> BlenderResult[None]:
        frame, values = files(identity, key)
        return accumulated(map(unlinked, (frame, values.path))).bind(lambda _: PREVIEWS.of(identity).dropped(key.hex()))

    return Result[Path, BlenderFaults].map(record(identity).written(after), lambda _: SERVED.of(identity).filled(after)).bind(lambda _: accumulated(map(erased, dropped))).map(lambda _: None)


def tiled(identity: Identity, key: ContentHash, scale: float, plane: Option[npt.NDArray[np.float32]] = Nothing) -> Job:
    """Constructs a tile request from a prepared plane or its stored frame."""
    return Job(
        place=(HistoryStore, key),
        key=content_hash(HistoryStore, Drawn.read(key, scale)),
        rank=Rank.SHOWN,
        run=lambda: (
            PREVIEWS
            .of(identity)
            .tile(key.hex(), key, lambda side: plane.map(Ok).default_with(lambda: output(identity, key)).bind(lambda found: fitted(found, side)).map(lambda pixels: Some(Some(pixels))), scale)
            .map(lambda _: None)
        ),
    )


def browsed(identity: Identity, context: bpy.types.Context) -> BlenderResult[Browsing]:
    """Returns the window manager's browser state."""
    return paired(browsing.bound(identity), rooted(bpy.types.WindowManager, context)).bind(lambda pair: pair[0].read(pair[1]))


def chosen(identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[History[str], Entry[str]]]:
    """Returns history and shown entry, absent fault without one."""
    return paired(browsed(identity, context), held(identity)).bind(
        lambda pair: (
            Option
            .of_optional(next((entry for entry in pair[1].entries if entry.key.hex() == pair[0].shown), None))
            .map(lambda entry: (pair[1], entry))
            .to_result_with(lambda: (BlenderAbsentFault(owner=str(record(identity).path), key=pair[0].shown),))
        )
    )


def named(history: History[str], entry: Entry[str]) -> str:
    """Returns a stored render's note, `#n` by its newest-first place for an unnamed one."""
    return entry.value or f"#{history.entries.index(entry) + 1}"


def beneath(identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[History[str], Entry[str]]]:
    """Returns history and the stored render the editor shows, the newest for Render Result."""

    def shown(space: bpy.types.SpaceImageEditor, history: History[str]) -> BlenderResult[tuple[History[str], Entry[str]]]:
        return (
            Option
            .of_optional(space.image)
            .bind(
                lambda image: Option.of_optional(
                    next((entry for entry in history.entries if image.type == "RENDER_RESULT" or files(identity, entry.key)[0] == Path(bpy.path.abspath(image.filepath, library=image.library))), None)
                )
            )
            .map(lambda entry: (history, entry))
            .to_result_with(lambda: (BlenderAbsentFault(owner=str(record(identity).path), key=repr(space)),))
        )

    return paired(rooted(bpy.types.SpaceImageEditor, context), held(identity)).bind(lambda pair: shown(*pair))


def shown(identity: Identity, context: bpy.types.Context) -> BlenderResult[str]:
    """Returns the shown frame's history name, the image's own name for a frame history holds no entry of."""

    def imaged(faults: BlenderFaults) -> BlenderResult[str]:
        match faults:
            case (BlenderAbsentFault(),):
                return rooted(bpy.types.SpaceImageEditor, context).bind(lambda space: Option.of_optional(space.image).map(lambda image: image.name).to_result_with(lambda: faults))
            case _:
                return Error(faults)

    return Result[str, BlenderFaults].or_else_with(beneath(identity, context).map(lambda pair: named(*pair)), imaged)


def side(identity: Identity, stack: EffectStack, context: bpy.types.Context, source: Source) -> BlenderResult[Before]:
    """Returns the chosen stored render's output captioned by its history name, or the shown frame's view layer ahead of effects formed by scene color management."""
    match source:
        case Source.RENDER:
            return paired(chosen(identity, context), shown(identity, context)).bind(
                lambda found: output(identity, found[0][1].key).map(lambda plane: Before(plane=plane, name=named(*found[0]), shown=found[1]))
            )
        case Source.LAYER:
            return paired(beneath(identity, context), rooted(bpy.types.Scene, context), rooted(bpy.types.ViewLayer, context)).bind(
                lambda found: (
                    combined(str(files(identity, found[0][1].key)[0]), source, Some(found[2].name))
                    .bind(lambda layer: managed(identity, stack, found[1], layer[1]))
                    .map(lambda plane: Before(plane=plane, name="Before effects", shown=named(*found[0])))
                )
            )


def before(identity: Identity, stack: EffectStack) -> BlenderResult[Option[Before]]:
    """Returns the before frame of the browser's source, `Nothing` while it holds no frame."""
    return (
        browsed(identity, bpy.context)
        .bind(lambda browser: side(identity, stack, bpy.context, browser.source))
        .map(Some)
        .or_else_with(lambda faults: Ok(Nothing) if all(isinstance(fault, BlenderAbsentFault) for fault in faults) else Error(faults))
    )


def difference(identity: Identity, stack: EffectStack) -> BlenderResult[Option[tuple[Tint, ...]]]:
    """Returns perceptual difference tint against shown frame while difference is on."""
    context = bpy.context
    ink = color(context.preferences.themes[0].info.info_error_text)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    return browsed(identity, context).bind(
        lambda browser: compared(identity, stack, context).map(lambda comparison: Some((Tint(plane=comparison.perceptual[..., 0], color=ink),))) if browser.difference else Ok(Nothing)
    )


def compared(identity: Identity, stack: EffectStack, context: bpy.types.Context) -> BlenderResult[Comparison]:
    """Compares the browser's before frame with the stored output of the frame the editor shows."""
    return (
        browsed(identity, context)
        .bind(lambda browser: paired(side(identity, stack, context, browser.source), beneath(identity, context).bind(lambda pair: output(identity, pair[1].key))))
        .bind(lambda planes: Comparison.of(planes[0].plane, planes[1], Domain.DISPLAY, data_range=1.0))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

browsing = Attachment(key="history", form=Browsing, owner=bpy.types.WindowManager)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["SERVED", "Browsing", "Entry", "History", "HistoryCommands", "HistoryStore", "Noting", "Served", "before", "beneath", "browsed", "browsing", "difference", "entries", "side"]
