"""Render history browser: the stored renders picked per window manager from their preview tiles, the pick drawn as the before frame of a compare, measured, tinted by difference, and restored or diffed against the scene."""

from collections.abc import Callable, Mapping
from contextlib import nullcontext
from enum import auto, Enum
from functools import cached_property, partial
from typing import ClassVar

import bpy
from expression import Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm_bpy.commands.invocations import Popup
from rasm_bpy.commands.operators import Answer, answered, Call, called, Done, Empty, Operator
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import Acquisitions, Declaration
from rasm_bpy.data._ids.images import _loaded, Tag
from rasm_bpy.data._ids.store import IdName
from rasm_bpy.data._rna.descriptors import Value
from rasm_bpy.data._rna.paths import _rna, RnaPath, rooted
from rasm_bpy.data._rna.properties import _offered, annotated, Callbacks, choices, enum_choices, EnumItem, IconedItem
from rasm_bpy.data._rna.snapshots import Snapshot
from rasm_bpy.render._color.buffers import _projection
from rasm_bpy.render._color.spaces import viewed
from rasm_bpy.render._outputs.results import CompletedRender
from rasm_bpy.render._review.history import RenderHistory, Renders
from rasm_bpy.settings._attachments import Attachment
from rasm_bpy.ui._drawing.handlers import HandlerRefresh, Refresh
from rasm_bpy.ui._images.compare import CompareView
from rasm_bpy.ui._images.views import coverage, Masks, overlaid, revealed
from rasm_bpy.ui._layout.members import Member
from rasm_bpy.ui._layout.rows import _grouped, _Label, even, Layout, OperatorButton, Property
from rasm_bpy.ui._layout.sections import Section
from rasm_bpy.ui._layout.templates import Gallery
from rasm_bpy.ui._resources.icons import Icon
from rasm_bpy.ui._resources.previews import _PREVIEWS, _uploaded, Texture
from rasm_bpy.ui._resources.theme import _color, Color, Metrics
from rasm_bpy.ui._screens.areas import _EditorState
from rasm_imaging import Comparison, converted_image, DisplayView, ImageFile, Metric, rgba, Severity, Threshold
from rasm_records import ContentHash, Deleted, diff, Edit, Error, Fault, Inserted, Moved, Nothing, Ok, Outcome, paired, Reason, Reparametrized

# --- [TYPES] ----------------------------------------------------------------------------


class Baseline(Enum):
    """Before frame source: the browsed stored render, or the shown render's view layer ahead of the scene's compositor effects."""

    STORED_RENDER = auto()
    VIEW_LAYER = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Browsing(bpy.types.PropertyGroup):
    """History browser of one window manager: the before frame source, the stored render drawn as before frame by its key and its gallery, and the difference tint."""

    baseline: bpy.props.EnumProperty(name="Before Frame", description="Stored render, or the shown render's view layer ahead of scene effects", items=enum_choices(Baseline))  # ty: ignore[invalid-type-form]
    picked: bpy.props.StringProperty(name="Stored Render", description="Key of the stored render drawn as before frame", options={"HIDDEN"})  # ty: ignore[invalid-type-form]
    shown: bpy.props.EnumProperty(name="Before", description="Stored render drawn as before frame", items=())  # ty: ignore[invalid-type-form]
    difference: bpy.props.BoolProperty(name="Difference", description="Tint pixels by perceived difference from the before frame")  # ty: ignore[invalid-type-form]


class Noting(bpy.types.PropertyGroup):
    """Note on a stored render."""

    note: bpy.props.StringProperty(name="Note", description="Stored render note")  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class HistoryBrowser(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render history browsed per window manager, its picked render compared as a before frame under `icon`, measured against `gates`, and restored or diffed against the scene.

    Stored and shown buffers compare through the scene's OCIO viewing pipeline; the view layer ahead of effects passes through `view`, its exposure holding any gain the effects apply.
    """

    # --- [STATE]

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
            callbacks=Some(lambda identity: nullcontext({"shown": Callbacks[Browsing](items=partial(self.listed, identity), get=partial(self.pick, identity), set=partial(self.picking, identity))})),
        )

    def listed(self, identity: Identity, _owner: Browsing, _context: bpy.types.Context | None) -> tuple[IconedItem | EnumItem, *tuple[IconedItem | EnumItem, ...]]:
        """Item rows of the stored renders newest first under their preview tiles, each value fixed per stored render key, the `NONE` row while none is stored."""
        previews = _PREVIEWS[identity]
        return _offered(
            tuple(
                (identifier, name, description, previews.get(identifier).map(lambda preview: preview.icon_id).default_value(0), value)
                for renders in identity.reports.reported(self.history.held)(identity).to_list()
                for names in (renders.names,)
                for identifier, name, description, _, value in choices(
                    (entry.value.hex(), f"{names[entry.value]}  {entry.moment.astimezone():%H:%M:%S}", renders.notes.get(entry.value.hex(), "")) for entry in renders.history.past
                )
            )
        )

    def pick(self, identity: Identity, owner: Browsing) -> int:
        """Returns the gallery value of the stored render the browser draws as before frame, the first row's while none is stored."""
        rows = self.listed(identity, owner, None)
        chosen = identity.reports.reported(self.history.held)(identity).bind(lambda renders: renders.before(owner.picked))
        return chosen.map(lambda key: next(row[4] for row in rows if row[0] == key.hex())).default_value(rows[0][4])

    def picking(self, identity: Identity, owner: Browsing, value: int) -> None:
        """Picks the stored render of a gallery value."""
        owner.picked = next(row[0] for row in self.listed(identity, owner, None) if row[4] == value)

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

    def caption(self, identity: Identity, context: bpy.types.Context) -> Outcome[str]:
        """Returns the shown frame's history name, the image's own name for a frame the history holds no entry of."""
        image = rooted(bpy.types.SpaceImageEditor, context).bind(lambda space: Option.of_optional(space.image).to_result_with(partial(Fault.of, Reason.ABSENT, repr(space), member="image")))
        return paired(image, self.history.held(identity)).map(
            lambda pair: self.history.stored.marked(identity, pair[0]).bind(lambda mark: mark.content).bind(lambda key: Option.of_optional(pair[1].names.get(key))).default_value(pair[0].name)
        )

    def note(self, identity: Identity, context: bpy.types.Context) -> tuple[str, ...]:
        """Returns the picked stored render's note, none while it holds none."""
        return tuple(self.chosen(identity, context).to_option().bind(lambda pair: Option.of_optional(pair[0].notes.get(pair[1].hex()))).filter(bool).to_list())

    # --- [FRAMES]

    def read(self, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[bpy.types.Image, ImageFile, CompletedRender]]:
        """Reads the explicitly opened history image from its owned immutable file and frozen encoding."""
        config = PyOpenColorIO.GetCurrentConfig()
        return paired(rooted(bpy.types.SpaceImageEditor, context), self.history.selected(identity, context)).bind(
            lambda found: (
                paired(found[1].read(), found[1].encoding)
                .bind(lambda source: converted_image(config, PyOpenColorIO.ROLE_SCENE_LINEAR, source[0], source=source[1]))
                .bind(lambda file: Option.of_optional(found[0].image).map(lambda image: (image, file, found[1])).to_result_with(partial(Fault.of, Reason.ABSENT, repr(found[0]), member="image")))
            )
        )

    def side(
        self, identity: Identity, context: bpy.types.Context, baseline: Baseline, shown: tuple[bpy.types.Image, ImageFile, CompletedRender]
    ) -> Outcome[tuple[npt.NDArray[np.float32], str, str, DisplayView]]:
        """Returns the projected baseline and the selected OCIO view forming the shown buffer."""
        config = PyOpenColorIO.GetCurrentConfig()
        view = Some(shown[1].header.get_string_attribute("view")).filter(bool)
        projection = _projection(context, shown[0], config)

        match baseline:
            case Baseline.STORED_RENDER:
                return paired(self.chosen(identity, context), self.caption(identity, context), projection).bind(
                    lambda found: self.history.output(identity, found[0][1], config, view=view).bind(
                        lambda file: found[2].form(config, rgba(file.pixels), file.colorspace).map(lambda plane: (plane, found[0][0].names[found[0][1]], found[1], found[2]))
                    )
                )
            case Baseline.VIEW_LAYER:
                formation = rooted(bpy.types.Scene, context).bind(partial(self.view, identity))
                return paired(shown[2].source(view=view), shown[2].source_encoding, formation, self.caption(identity, context), projection).bind(
                    lambda found: (
                        converted_image(config, PyOpenColorIO.ROLE_SCENE_LINEAR, found[0], source=found[1])
                        .bind(lambda file: found[2].form(config, rgba(file.pixels), file.colorspace))
                        .map(lambda plane: (plane, "Before effects", found[3], found[4]))
                    )
                )

    def before(self, identity: Identity) -> Outcome[Option[Texture[tuple[str, str]]]]:
        """Returns the before frame of the browser's baseline, `Nothing` while it holds no frame."""
        return (
            self
            .read(identity, bpy.context)
            .bind(
                lambda shown: (
                    self
                    .browsed(identity, bpy.context)
                    .bind(lambda browser: self.side(identity, bpy.context, Baseline[browser.baseline], shown))
                    .bind(lambda side: _uploaded(side[0]).map(lambda texture: (texture, (side[1], side[2]))))
                    .map(Some)
                )
            )
            .or_else_with(lambda faults: Ok(Nothing) if all(fault.reason is Reason.ABSENT for fault in faults) else Error(faults))
        )

    def compared(self, identity: Identity, context: bpy.types.Context) -> Outcome[Comparison]:
        """Compares the browser's before frame with the shown buffer through the selected OCIO viewing pipeline."""
        return paired(self.browsed(identity, context), self.read(identity, context)).bind(lambda found: self.measured(identity, context, Baseline[found[0].baseline], found[1]))

    def measured(self, identity: Identity, context: bpy.types.Context, baseline: Baseline, shown: tuple[bpy.types.Image, ImageFile, CompletedRender]) -> Outcome[Comparison]:
        """Compares an actual selected buffer with the browser's baseline in full-frame OCIO RGB projection."""
        config = PyOpenColorIO.GetCurrentConfig()
        return self.side(identity, context, baseline, shown).bind(
            lambda side: side[3].form(config, rgba(shown[1].pixels), shown[1].colorspace).bind(lambda after: Comparison.of(side[0], after, PyOpenColorIO.REFERENCE_SPACE_DISPLAY, data_range=1.0))
        )

    def difference(self, identity: Identity, metrics: Metrics) -> Outcome[Option[tuple[Texture[Color], ...]]]:
        """Returns the perceptual difference tint in the theme's error text color while difference shows."""
        tint = _color(metrics.preferences.themes[0].info.info_error_text)
        return self.browsed(identity, bpy.context).bind(
            lambda browser: (
                self.read(identity, bpy.context).bind(
                    lambda shown: (
                        self
                        .measured(identity, bpy.context, Baseline[browser.baseline], shown)
                        .bind(lambda comparison: coverage(comparison.perceptual[..., 0]))
                        .map(lambda texture: Some(((texture, tint),)))
                    )
                )
                if browser.difference
                else Ok(Nothing)
            )
        )

    @cached_property
    def compare(self) -> CompareView:
        """Before frame over the shown image, redrawn on browser change and each stored render."""
        return CompareView(key=f"{self.history.key}_compare", icon=self.icon, read=lambda identity, _metrics: self.before(identity), kinds=("IMAGE",), refreshes=self.refreshes)

    @staticmethod
    def refreshes(identity: Identity) -> tuple[Refresh, ...]:
        """Returns a refresh per browser field and the stored render signal."""
        return (  # ty: ignore[unsound-return-statement]
            *((Browsing, name) for name in annotated(Browsing)),
            RenderHistory.signal[identity],
            HandlerRefresh(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene, bpy.types.Image)),
        )

    # --- [COMMANDS]

    def revised(self, call: Call[Noting] | Call[Empty], change: Callable[[Renders, ContentHash], Renders]) -> Outcome[Answer]:
        """Writes the picked render's change to the stored renders, then fires the compare signal."""
        identity = call.identity
        return self.chosen(identity, call.context).bind(lambda pair: self.history.kept(identity, pair[0], change(*pair))).map(lambda _: Done())

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
                    return f"{_rna(key)} changed {', '.join(sorted(map(_rna, before.keys() | after.keys())))}"
                case Inserted(key=key):
                    return f"{_rna(key)} added"
                case Deleted(key=key):
                    return f"{_rna(key)} removed"
                case Moved(key=key):
                    return f"{_rna(key)} moved"

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
        config = PyOpenColorIO.GetCurrentConfig()

        def opened(found: tuple[Renders, ContentHash]) -> Outcome[bpy.types.Image]:
            renders, key = found
            space = Option.of_optional(renders.origins.get(key.hex())).map(lambda origin: origin[0])
            tag = Tag(
                space=space,
                view_as_render=Some(
                    space
                    .bind(lambda name: Option.of_optional(config.getColorSpace(name)))
                    .filter(lambda color: not color.isData() and color.getReferenceSpaceType() == PyOpenColorIO.REFERENCE_SPACE_SCENE)
                    .is_some()
                ),
            )
            return self.history.stored.made(
                call.identity, lambda: _loaded(str(self.history.frame(call.identity, key)), Tag()), filled=lambda image: tag.applied(image).map(lambda _: image), content=Some(key)
            )

        def shown(image: bpy.types.Image) -> Outcome[Answer]:
            return revealed(call.context).bind(
                lambda located: (
                    _EditorState
                    .of(bpy.types.SpaceImageEditor, {("image",): IdName.of(image)})
                    .bind(lambda state: state.applied(located))
                    .map(lambda _: Done(notes=(("INFO", f"Showing {image.name}"),)))
                )
            )

        return self.chosen(call.identity, call.context).bind(opened).bind(shown)

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
        return self.history.held(identity).bind(lambda renders: self.history.kept(identity, renders, Renders())).map(lambda _: Done())

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
        return Operator.of(Empty, options=frozenset())(self.compare_settings)

    @cached_property
    def opening(self) -> Operator[Empty]:
        """Command showing the picked render."""
        return Operator.of(Empty, options=frozenset())(self.open_render)

    @cached_property
    def measuring(self) -> Operator[Empty]:
        """Command grading the before frame against the shown frame."""
        return Operator.of(Empty, options=frozenset())(self.measure_difference)

    @cached_property
    def noting(self) -> Operator[Noting]:
        """Command writing the picked render's note through a dialog seeded with it."""
        return Operator.of(Noting, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))), seed=Some(self.noted), options=frozenset())(self.edit_note)

    @cached_property
    def removing(self) -> Operator[Empty]:
        """Command deleting the picked render."""
        return Operator.of(Empty, options=frozenset())(self.remove_render)

    @cached_property
    def clearing(self) -> Operator[Empty]:
        """Command deleting every stored render after a confirmation."""
        return Operator.of(Empty, invoke=Some(Popup[Empty](partial(bpy.types.WindowManager.invoke_confirm, confirm_text="Clear", icon="WARNING"))), options=frozenset())(self.clear_history)

    @cached_property
    def revealing(self) -> Operator[Empty]:
        """Command opening the history folder."""
        return Operator.of(Empty, options=frozenset())(self.open_history_folder)

    # --- [SECTIONS]

    def section(self) -> Section:
        """Returns the Render History section: the before frame source, the gallery, the picked render's note, the difference tint, and the command groups."""
        return Section(
            key="render_history",
            rows=(
                Layout(rows=(Property(target=Member(root=self.browsing, path=("baseline",)), keywords={"expand": True}),), container=Some(even)),
                Gallery(target=Member(root=self.browsing, path=("shown",))),
                Layout(builder=Some(lambda identity: Ok((_Label(text=partial(self.note, identity)),)))),
                Property(target=Member(root=self.browsing, path=("difference",))),
                Layout(rows=tuple(OperatorButton(target=command) for command in (self.restoring, self.diffing, self.opening)), container=Some(_grouped)),
                Layout(rows=(OperatorButton(target=self.measuring), OperatorButton(target=self.noting)), container=Some(_grouped)),
                Layout(rows=(OperatorButton(target=self.removing), OperatorButton(target=self.clearing), OperatorButton(target=self.revealing, icon=Some("FILE_FOLDER"))), container=Some(_grouped)),
            ),
        )

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Render history the browser reads."""
        return (self.history,)

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns the browser, the commands, the compare, and the difference mask."""
        return (
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
            overlaid(Masks(read=partial(self.difference, identity)), self.compare.shown, self.refreshes(identity)),
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Baseline", "Browsing", "HistoryBrowser", "Noting"]
