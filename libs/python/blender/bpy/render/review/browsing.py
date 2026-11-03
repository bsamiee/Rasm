"""Render history browser: the stored renders picked per window manager from their preview tiles, the pick drawn as the before frame of a compare, measured, tinted by difference, and restored or diffed against the scene."""

from collections.abc import Callable, Mapping
from contextlib import nullcontext
from enum import auto, Enum
from functools import cached_property, partial
from typing import ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.invocations import Popup
from blender.bpy.commands.operators import Answer, answered, Call, called, Done, Empty, NoUndo, Operator
from blender.bpy.data.ids.images import loaded, Tag
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.data.rna.paths import rna, RnaPath, rooted, Struct
from blender.bpy.data.rna.properties import annotated, Callbacks, choices, enum_choices, EnumItem, held_choices, IconedItem, offered
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.review.history import RenderHistory, Renders
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.drawing.handlers import Refresh
from blender.bpy.ui.images.compare import Before, CompareView
from blender.bpy.ui.images.views import Masks, overlaid, revealed, Tint
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import even, grouped, Label, Layout, OperatorButton, Property
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.templates import Gallery
from blender.bpy.ui.resources.icons import Icon
from blender.bpy.ui.resources.previews import PREVIEWS
from blender.bpy.ui.resources.theme import color, Metrics
from blender.bpy.ui.screens.areas import EditorState
from blender.imaging.analysis.comparison import Comparison, Metric, Severity, Threshold
from blender.imaging.color.processors import DisplayView
from blender.records.faults import Fault, Outcome, paired, Reason
from blender.records.stores import ContentHash
from blender.records.trees import Deleted, diff, Edit, Inserted, Moved, Reparametrized

# --- [TYPES] ----------------------------------------------------------------------------

type _Row = IconedItem | EnumItem


class Baseline(Enum):
    """Before frame source: the browsed stored render, or the shown render's view layer ahead of the scene's compositor effects."""

    STORED_RENDER = auto()
    VIEW_LAYER = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Browsing(bpy.types.PropertyGroup):
    """History browser of one window manager: the before frame source, the stored render drawn as before frame by its key and its gallery, and the difference tint."""

    baseline: bpy.props.EnumProperty(name="Before Frame", description="Stored render, or the shown render's view layer ahead of scene effects", items=enum_choices(Baseline))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    picked: bpy.props.StringProperty(name="Stored Render", description="Key of the stored render drawn as before frame", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    shown: bpy.props.EnumProperty(name="Before", description="Stored render drawn as before frame", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    difference: bpy.props.BoolProperty(name="Difference", description="Tint pixels by perceived difference from the before frame")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Noting(bpy.types.PropertyGroup):
    """Note on a stored render."""

    note: bpy.props.StringProperty(name="Note", description="Stored render note")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class HistoryBrowser(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render history browsed per window manager, its picked render compared as a before frame under `icon`, measured against `gates`, and restored or diffed against the scene.

    Stored renders and the shown frame draw through the scene's view, as the editor shows them, and the view layer ahead of effects through `view`, its exposure holding any gain the effects apply.
    """

    icon: Icon
    view: Callable[[Identity, bpy.types.Scene], Outcome[DisplayView]] = lambda _identity, scene: Ok(viewed(scene))
    gates: tuple[Threshold, ...] = (
        Threshold(metric=Metric.PSNR, advisory=35.0, refuse=30.0),
        Threshold(metric=Metric.NRMSE, advisory=0.05, refuse=0.10),
        Threshold(metric=Metric.SSIM, advisory=0.98, refuse=0.95),
    )
    history: ClassVar[RenderHistory] = RenderHistory()

    # --- [BROWSING]

    @cached_property
    def browsing(self) -> Attachment[Browsing]:
        """Window manager browser, its gallery items the stored renders and its pick read and written through the stored render key."""
        return Attachment(
            key=self.history.key,
            form=Browsing,
            owner=bpy.types.WindowManager,
            callbacks=Some(lambda identity: nullcontext({"shown": Callbacks(items=partial(self.listed, identity), get=partial(self.pick, identity), set=partial(self.picking, identity))})),
        )

    def listed(self, identity: Identity, _owner: Struct, _context: bpy.types.Context | None) -> tuple[_Row, *tuple[_Row, ...]]:
        """Item rows of the stored renders newest first under their preview tiles, each value fixed per stored render key, the `NONE` row while none is stored."""
        previews = identity.resources[PREVIEWS]
        return offered(
            held_choices(
                tuple(
                    (identifier, name, description, previews.icon_id(identifier).default_value(0), value)
                    for renders in identity.reports.reported(self.history.held)(identity).to_list()
                    for identifier, name, description, _, value in choices(
                        (entry.value.hex(), f"{renders.named(entry.value)}  {entry.moment.astimezone():%H:%M:%S}", renders.notes.get(entry.value.hex(), "")) for entry in renders.history.past
                    )
                )
            )
        )

    def pick(self, identity: Identity, owner: Struct) -> int:
        """Returns the gallery value of the stored render the browser draws as before frame, the first row's while none is stored."""
        rows = self.listed(identity, owner, None)
        chosen = Option.of_optional(try_downcast(Browsing, owner)).bind(lambda browser: identity.reports.reported(self.history.held)(identity).bind(lambda renders: renders.before(browser.picked)))
        return Block.of_seq(rows).filter(lambda row: chosen.filter(lambda key: key.hex() == row[0]).is_some()).try_head().default_value(rows[0])[4]

    def picking(self, identity: Identity, owner: Struct, value: int) -> None:
        """Picks the stored render of a gallery value."""
        rows = self.listed(identity, owner, None)
        for browser, row in ((browser, row) for browser in Option.of_optional(try_downcast(Browsing, owner)).to_list() for row in rows if row[4] == value):
            browser.picked = row[0]

    def browsed(self, identity: Identity, context: bpy.types.Context) -> Outcome[Browsing]:
        """Returns the window manager's browser."""
        return paired(self.browsing.bound(identity), rooted(bpy.types.WindowManager, context)).map(lambda pair: pair[0].struct(pair[1]))

    def chosen(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Renders, ContentHash]]:
        """Returns renders and the stored render the browser draws as before frame, absent fault while none is stored."""
        return paired(self.browsed(identity, context), self.history.held(identity)).bind(
            lambda pair: (
                pair[1].before(pair[0].picked).map(lambda key: (pair[1], key)).to_result_with(partial(Fault.of, Reason.ABSENT, str(self.history.record(identity).path), member="stored render"))
            )
        )

    def shown_render(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Renders, ContentHash]]:
        """Returns renders and the stored render the editor shows: the newest for Render Result, the one a stored image's ownership mark names for a stored image."""

        def shown(space: bpy.types.SpaceImageEditor, renders: Renders) -> Outcome[tuple[Renders, ContentHash]]:
            keys = frozenset(entry.value for entry in renders.history.past)
            return (
                Option
                .of_optional(space.image)
                .bind(
                    lambda image: (
                        Block.of_seq(renders.history.past).try_head().map(lambda entry: entry.value)
                        if image.type == "RENDER_RESULT"
                        else self.history.stored.marked(identity, image).bind(lambda mark: mark.content).filter(lambda key: key in keys)
                    )
                )
                .map(lambda key: (renders, key))
                .to_result_with(partial(Fault.of, Reason.ABSENT, str(self.history.record(identity).path), member=repr(space.image)))
            )

        return paired(rooted(bpy.types.SpaceImageEditor, context), self.history.held(identity)).bind(lambda pair: shown(*pair))

    def caption(self, identity: Identity, context: bpy.types.Context) -> Outcome[str]:
        """Returns the shown frame's history name, the image's own name for a frame the history holds no entry of."""

        def imaged(faults: tuple[Fault, *tuple[Fault, ...]]) -> Outcome[str]:
            match faults:
                case (Fault(reason=Reason.ABSENT),):
                    return rooted(bpy.types.SpaceImageEditor, context).bind(lambda space: Option.of_optional(space.image).map(lambda image: image.name).to_result_with(lambda: faults))
                case _:
                    return Error(faults)

        return self.shown_render(identity, context).map(lambda pair: pair[0].named(pair[1])).or_else_with(imaged)

    def note(self, identity: Identity, context: bpy.types.Context) -> tuple[str, ...]:
        """Returns the picked stored render's note, none while it holds none."""
        return tuple(self.chosen(identity, context).to_option().bind(lambda pair: Option.of_optional(pair[0].notes.get(pair[1].hex()))).filter(bool).to_list())

    # --- [FRAMES]

    def side(self, identity: Identity, context: bpy.types.Context, baseline: Baseline) -> Outcome[Before]:
        """Returns the picked stored render captioned by its history name, or the shown render's view layer ahead of effects formed through the view hook."""
        config = PyOpenColorIO.GetCurrentConfig()

        def layered(scene: bpy.types.Scene, plane: npt.NDArray[np.float32]) -> Outcome[npt.NDArray[np.float32]]:
            return self.view(identity, scene).bind(lambda view: view.form(config, plane))

        match baseline:
            case Baseline.STORED_RENDER:
                return paired(self.chosen(identity, context), self.caption(identity, context), rooted(bpy.types.Scene, context)).bind(
                    lambda found: self.history.formed(identity, found[2], found[0][1]).map(lambda plane: Before(plane=plane, name=found[0][0].named(found[0][1]), shown=found[1]))
                )
            case Baseline.VIEW_LAYER:
                return paired(self.shown_render(identity, context), rooted(bpy.types.Scene, context), rooted(bpy.types.ViewLayer, context)).bind(
                    lambda found: (
                        self.history
                        .output(identity, found[0][1], Some(found[2].name))
                        .bind(lambda plane: layered(found[1], plane))
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
            .or_else_with(lambda faults: Ok(Nothing) if all(fault.reason is Reason.ABSENT for fault in faults) else Error(faults))
        )

    def compared(self, identity: Identity, context: bpy.types.Context) -> Outcome[Comparison]:
        """Compares the browser's before frame with the frame the editor shows, both in the colors the editor shows them."""
        shown = paired(self.shown_render(identity, context), rooted(bpy.types.Scene, context)).bind(lambda found: self.history.formed(identity, found[1], found[0][1]))
        return (
            self
            .browsed(identity, context)
            .bind(lambda browser: paired(self.side(identity, context, Baseline[browser.baseline]), shown))
            .bind(lambda planes: Comparison.of(planes[0].plane, planes[1], PyOpenColorIO.REFERENCE_SPACE_DISPLAY, data_range=1.0))
        )

    def difference(self, identity: Identity, metrics: Metrics) -> Outcome[Option[tuple[Tint, ...]]]:
        """Returns the perceptual difference tint in the theme's error text color while difference shows."""
        tint = color(metrics.preferences.themes[0].info.info_error_text)
        return self.browsed(identity, bpy.context).bind(
            lambda browser: self.compared(identity, bpy.context).map(lambda comparison: Some((Tint(plane=comparison.perceptual[..., 0], color=tint),))) if browser.difference else Ok(Nothing)
        )

    @cached_property
    def compare(self) -> CompareView:
        """Before frame over the shown image, redrawn on browser change and each stored render."""
        return CompareView(key=f"{self.history.key}_compare", icon=self.icon, read=lambda identity, _metrics: self.before(identity), kinds=("IMAGE", "RENDER_RESULT"), refreshes=self.refreshes)

    @staticmethod
    def refreshes(identity: Identity) -> Outcome[tuple[Refresh, ...]]:
        """Returns a refresh per browser field and the stored render signal."""
        return Ok((*((Browsing, name) for name in annotated(Browsing)), identity.resources[RenderHistory.signal]))

    # --- [COMMANDS]

    def revised(self, call: Call[Noting] | Call[Empty], change: Callable[[Renders, ContentHash], Renders]) -> Outcome[Answer]:
        """Writes the picked render's change to the stored renders, then fires the compare signal."""
        identity = call.identity
        return (
            self
            .chosen(identity, call.context)
            .bind(lambda pair: self.history.kept(identity, pair[0], change(*pair)))
            .bind(lambda _: identity.resources[RenderHistory.signal].fired())
            .map(lambda _: Done())
        )

    def restore_settings(self, call: Call[Empty]) -> Outcome[Answer]:
        """Apply stored settings to scene."""

        def restored(snapshot: Snapshot, scene: bpy.types.Scene) -> Outcome[Answer]:
            applied = snapshot.apply(scene)
            return answered(applied, "Scene settings match stored render", (("INFO", f"Restored {len(applied.written)} settings, {len(applied.skipped)} skipped"),))

        return (
            self
            .chosen(call.identity, call.context)
            .bind(lambda pair: paired(self.history.snapshot(call.identity, pair[1]).read(), rooted(bpy.types.Scene, call.context)))
            .bind(lambda pair: restored(*pair))
        )

    def compare_settings(self, call: Call[Empty]) -> Outcome[Answer]:
        """Report settings changed since stored render."""

        def noted[V](edit: Edit[V, RnaPath, RnaPath]) -> str:
            match edit:
                case Reparametrized(key=key, before=before, after=after):
                    return f"{rna(key)} changed {', '.join(sorted(map(rna, before.keys() | after.keys())))}"
                case Inserted(key=key):
                    return f"{rna(key)} added"
                case Deleted(key=key):
                    return f"{rna(key)} removed"
                case Moved(key=key):
                    return f"{rna(key)} moved"

        return (
            self
            .chosen(call.identity, call.context)
            .bind(
                lambda pair: paired(self.history.snapshot(call.identity, pair[1]).read(), rooted(bpy.types.Scene, call.context).bind(lambda scene: Snapshot.capture(scene, owned=call.identity.owns)))
            )
            .map(lambda snapshots: Done(notes=tuple(("INFO", noted(edit)) for edit in diff(snapshots[0].tree, snapshots[1].tree)) or (("INFO", "Settings match stored render"),)))
        )

    def open_render(self, call: Call[Empty]) -> Outcome[Answer]:
        """Show stored render."""

        def shown(image: bpy.types.Image) -> Outcome[Answer]:
            return revealed(call.context).bind(
                lambda located: (
                    EditorState
                    .of(bpy.types.SpaceImageEditor, {("image",): IdName.of(image)})
                    .bind(lambda state: state.applied(located.area))
                    .map(lambda _: Done(notes=(("INFO", f"Showing {image.name}"),)))
                )
            )

        return (
            self
            .chosen(call.identity, call.context)
            .bind(lambda pair: self.history.stored.made(call.identity, lambda: loaded(str(self.history.frame(call.identity, pair[1])), Tag(view_as_render=Some(True))), content=Some(pair[1])))
            .bind(shown)
        )

    def measure_difference(self, call: Call[Empty]) -> Outcome[Answer]:
        """Grade stored render against shown frame."""
        return self.compared(call.identity, call.context).map(
            lambda comparison: Done(
                notes=(*(("INFO", f"{metric.name} {value:.4g}") for metric, value in comparison.measures.items()), ("INFO", f"{Severity.__name__} {comparison.severity(self.gates).name.lower()}"))
            )
        )

    def edit_note(self, call: Call[Noting]) -> Outcome[Answer]:
        """Write stored render note."""
        return self.revised(call, lambda renders, key: msgspec.structs.replace(renders, notes={**renders.notes, key.hex(): call.properties.note}))

    def noted(self, _properties: Noting, _event: bpy.types.Event, identity: Identity) -> Mapping[str, Value]:
        """Seeds the note popup with the picked stored render's note."""
        return dict(zip(("note",), self.note(identity, bpy.context), strict=False))

    def remove_render(self, call: Call[Empty]) -> Outcome[Answer]:
        """Delete stored render and files."""
        return self.revised(call, lambda renders, key: msgspec.structs.replace(renders, history=renders.history.remove(key)))

    def clear_history(self, call: Call[Empty]) -> Outcome[Answer]:
        """Delete every stored render."""
        identity = call.identity
        return self.history.held(identity).bind(lambda renders: self.history.kept(identity, renders, Renders())).bind(lambda _: identity.resources[RenderHistory.signal].fired()).map(lambda _: Done())

    def open_history_folder(self, call: Call[Empty]) -> Outcome[Answer]:
        """Open stored render folder."""
        return called(bpy.ops.wm.path_open, {"filepath": str(self.history.folder(call.identity))}).map(lambda _: Done())

    @cached_property
    def restoring(self) -> Operator[Empty]:
        """Command applying the picked render's settings."""
        return Operator.of(Empty)(self.restore_settings)

    @cached_property
    def diffing(self) -> Operator[Empty]:
        """Command reporting the settings changed since the picked render."""
        return Operator.of(Empty, undo=NoUndo())(self.compare_settings)

    @cached_property
    def opening(self) -> Operator[Empty]:
        """Command showing the picked render."""
        return Operator.of(Empty, undo=NoUndo())(self.open_render)

    @cached_property
    def measuring(self) -> Operator[Empty]:
        """Command grading the before frame against the shown frame."""
        return Operator.of(Empty, undo=NoUndo())(self.measure_difference)

    @cached_property
    def noting(self) -> Operator[Noting]:
        """Command writing the picked render's note through a dialog seeded with it."""
        return Operator.of(Noting, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))), seed=Some(self.noted), undo=NoUndo())(self.edit_note)

    @cached_property
    def removing(self) -> Operator[Empty]:
        """Command deleting the picked render."""
        return Operator.of(Empty, undo=NoUndo())(self.remove_render)

    @cached_property
    def clearing(self) -> Operator[Empty]:
        """Command deleting every stored render after a confirmation."""
        return Operator.of(Empty, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_confirm, confirm_text="Clear", icon="WARNING"))), undo=NoUndo())(self.clear_history)

    @cached_property
    def revealing(self) -> Operator[Empty]:
        """Command opening the history folder."""
        return Operator.of(Empty, undo=NoUndo())(self.open_history_folder)

    # --- [ROWS]

    def section(self) -> Section:
        """Returns the Render History section: the before frame source, the gallery, the picked render's note, the difference tint, and the command groups."""
        return Section(
            key="render_history",
            rows=(
                Layout(rows=(Property(target=Member(root=self.browsing, path=("baseline",)), keywords={"expand": True}),), container=Some(even)),
                Gallery(target=Member(root=self.browsing, path=("shown",))),
                Layout(rows=lambda identity: Ok((Label(text=partial(self.note, identity)),))),
                Property(target=Member(root=self.browsing, path=("difference",))),
                Layout(rows=tuple(OperatorButton(target=command) for command in (self.restoring, self.diffing, self.opening)), container=Some(grouped)),
                Layout(rows=(OperatorButton(target=self.measuring), OperatorButton(target=self.noting)), container=Some(grouped)),
                Layout(rows=(OperatorButton(target=self.removing), OperatorButton(target=self.clearing), OperatorButton(target=self.revealing, icon=Some("FILE_FOLDER"))), container=Some(grouped)),
            ),
        )

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Render history the browser reads."""
        return (self.history,)

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the browser, the commands, the compare, and the difference mask."""
        return self.refreshes(identity).map(
            lambda refreshes: (
                self.browsing,
                self.restoring,
                self.diffing,
                self.opening,
                self.measuring,
                self.noting,
                self.removing,
                self.clearing,
                self.revealing,
                self.compare,
                overlaid(Masks(source=partial(self.difference, identity)), self.compare.shown, refreshes),
            )
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Baseline", "Browsing", "HistoryBrowser", "Noting"]
