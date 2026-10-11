"""Preset browsing over native modal events, GPU drawing, and the shared preview queue."""

from collections.abc import Callable, Hashable
from contextlib import ExitStack
from enum import auto, Enum
from functools import partial
import math
from pathlib import Path
from typing import Annotated, ClassVar, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import gpu
import msgspec
import numpy as np
import numpy.typing as npt

from rasm.blender.commands import operators
from rasm.blender.commands.calls import called
from rasm.blender.commands.modal import Gesture, Modal, Travel
from rasm.blender.data.rna.conditions import AllOf, holds
from rasm.blender.extension.identity import Identity
from rasm.blender.extension.registration import Acquisitions, entered
from rasm.blender.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from rasm.blender.render.previews.queue import Job, PreviewQueue, Rank
from rasm.blender.render.previews.scenes import Chain
from rasm.blender.render.previews.tiles import Tiles
from rasm.blender.render.review.frames import Before, editor, Wipe
from rasm.blender.settings.attachments import Attached, Attachment
from rasm.blender.ui.drawing.canvas import Canvas, Rect
from rasm.blender.ui.drawing.handlers import Drawing, Overlay, Signal
from rasm.blender.ui.layout.rows import Line, OperatorButton, realized, RowDraw
from rasm.blender.ui.layout.sections import Listing, Named, Renamed, Saved, Selected, Verb
from rasm.blender.ui.resources.textures import uploaded
from rasm.blender.ui.resources.theme import Metrics, State, widget
from rasm.blender.ui.screens.areas import AreaQuery, Located
from rasm.blender.ui.screens.regions import RegionView
from rasm.imaging.color.processors import unassociated
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.parameters import Choice, Gated

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class BrowserMode(Choice):
    """Persistent thumbnail and compact row presentations."""

    GRID = (0, "Grid")
    LIST = (1, "List")


class BrowserControl(Enum):
    """Browser's text entry and thumbnail-size controls."""

    SEARCH = auto()
    SIZE = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

THUMBNAIL_SIDE = (32.0, 256.0)


# --- [MODELS] ---------------------------------------------------------------------------


class BrowserLayout(msgspec.Struct, frozen=True, kw_only=True):
    """Screen's browser layout, with disabled size fitting the available browser width."""

    mode: BrowserMode = BrowserMode.GRID
    size: Gated[Annotated[float, msgspec.Meta(ge=THUMBNAIL_SIDE[0], le=THUMBNAIL_SIDE[1])]] = Gated(enabled=False, value=128.0)


class Demand[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Retained identity of one visible tile or one completed hover dwell."""

    stem: str
    value: BlenderResult[T]
    side: int
    rank: Rank


class Browser[T: msgspec.Struct](Line, frozen=True, kw_only=True):
    """One existing preset listing with caller-owned recipes and the screen's layout."""

    layouts: ClassVar[Attachment[BrowserLayout]] = Attachment(key="browser_layout", form=BrowserLayout, owner=bpy.types.Screen)
    listing: Listing[T]
    tiles: Tiles
    candidate: Callable[[Identity, bpy.types.Context, T, Option[npt.NDArray[np.float32]]], BlenderResult[Chain]]
    observed: Callable[[bpy.types.Context], BlenderResult[tuple[Hashable, Callable[[tuple[T, ...]], BlenderResult[tuple[bool, ...]]]]]]
    main: AreaQuery

    @property
    def key(self) -> str:
        """Native operator key derived from the listing it opens."""
        return f"browse_{self.listing.preset.key}"

    def command(self, identity: Identity) -> operators.Operator[operators.Empty, "Session[T]"]:
        """Builds the native modal operator over this catalog and resource scope."""

        def begin(context: bpy.types.Context, travel: Travel) -> BlenderResult[Session[T]]:
            origin = paired(
                Option.of_optional(context.window).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key="window"),)),
                Option.of_optional(context.area).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key="area"),)),
            )
            at = Result[tuple[bpy.types.Window, bpy.types.Area], BlenderFaults].bind(origin, lambda pair: RegionView.of(Located(window=pair[0], area=pair[1])))

            def acquired(view: RegionView, held: Attached[BrowserLayout], layout: BrowserLayout) -> BlenderResult[Session[T]]:
                session = Session(self, identity, view, held, layout, Option.of_optional(context.region))
                session.pointer = Option.of_optional(context.region).map(lambda region: (travel.x + region.x, travel.y + region.y))
                session.shift = travel.precise
                with ExitStack() as scope:
                    if (result := (session.acquired(scope).bind(lambda _: session.reloaded(context)).map(lambda _: session))).is_ok():
                        session.scope = scope.pop_all()
                    return result

            return paired(at, Browser.layouts.bound(identity)).bind(lambda pair: pair[1].read(pair[0].located.window.screen).bind(lambda layout: acquired(*pair, layout)))

        return operators.Operator(
            key=self.key,
            title="Browse Presets",
            description="Browse, preview, and apply this section's presets",
            icon=Some("PRESET"),
            condition=Some(AllOf(conditions=(*self.condition.to_list(), *self.listing.condition.to_list()))),
            form=operators.Empty,
            run=lambda _call: Ok(operators.Done()),
            undo=False,
            invoke=Some(
                Modal(
                    gesture=Gesture.TRACK,
                    begin=begin,
                    preview=lambda _context, _session, _travel: Ok(Nothing),
                    restore=lambda _context, _session: Ok(None),
                    event=Some(lambda context, session, event: session.handled(context, event).map(lambda outcome: (session, outcome))),
                    release=Some(lambda _context, session: session.released()),
                )
            ),
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Retains the renderer and listing before registering the native browser."""
        return Ok((self.tiles, self.listing, self.command(identity)))

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Places the acquired browser operator through the existing native row policy."""
        return realized(OperatorButton(target=operators.Acquired(key=self.key), context="INVOKE_DEFAULT"), identity)


# --- [SERVICES] -------------------------------------------------------------------------


class Session[T: msgspec.Struct]:
    """One modal browser's native drawing, dwell, requests, and file-action lifetime."""

    def __init__(self, browser: Browser[T], identity: Identity, view: RegionView, held: Attached[BrowserLayout], layout: BrowserLayout, origin: Option[bpy.types.Region]) -> None:
        """Captures the native origin and initializes an unacquired session."""
        self.browser, self.identity, self.view, self.held, self.layout = browser, identity, view, held, layout
        self.origin = origin
        self.main: Option[RegionView] = Nothing
        self.area = view.located.area.as_pointer()
        self.region = view.region.as_pointer()
        self.scope = ExitStack()
        self.signal = Signal()
        self.active = True
        self.rows: dict[str, BlenderResult[T]] = {}
        self.current: Option[str] = Nothing
        self.lead: Option[str] = Nothing
        self.anchor: Option[str] = Nothing
        self.selected: frozenset[str] = frozenset()
        self.hover: Option[str] = Nothing
        self.pointer: Option[tuple[int, int]] = Nothing
        self.shift = False
        self.search = ""
        self.searching = False
        self.resizing: Option[Rect] = Nothing
        self.offset = 0
        self.revision: Option[Hashable] = Nothing
        self.desired: dict[tuple[str, Rank], Demand[T]] = {}
        self.planes: dict[Demand[T], npt.NDArray[np.float32]] = {}
        self.textures: dict[Demand[T], gpu.types.GPUTexture] = {}
        self.failures: dict[Demand[T], BlenderFaults] = {}
        self.before: Option[Before] = Nothing
        self.dwell: Option[Callable[[], None]] = Nothing
        self.actions = browser.listing.commands(identity).verbs

    # --- [LIFECYCLE]

    def acquired(self, scope: ExitStack) -> BlenderResult[None]:
        """Acquires the native draw handles and complete-demand queue feed together."""
        scope.callback(self.cancelled)
        area = self.view.located.area
        if self.browser.main.admits(area) and not any(region.type == "UI" for region in self.origin.to_list()):
            return Error((BlenderAbsentFault(owner=repr(area), key="sidebar origin or a distinct browser editor"),))
        frame = Overlay(
            drawing=Drawing(
                area=self.browser.main,
                part=Wipe(identity=self.identity, source=lambda _metrics: Ok(self.before)),
                refreshes=(self.signal,),
                where=Some(lambda context: any(context.window == view.located.window and context.area == view.located.area and context.region == view.region for view in self.main.to_list())),
            ),
            space=bpy.types.SpaceImageEditor,
            scissor_enabled=False,
            respect_overlays=False,
        )
        scope.enter_context(PreviewQueue.fed(self.identity, self.jobs))
        space = Option.of_optional(area.spaces.active).to_result_with(lambda: (BlenderAbsentFault(owner=repr(area), key="spaces.active"),))
        return Result[bpy.types.Space, BlenderFaults].bind(
            space,
            lambda active: accumulated(
                row.acquisitions(self.identity)
                for row in (
                    frame,
                    Overlay(
                        drawing=Drawing(
                            area=AreaQuery(space=area.type),
                            part=self,
                            refreshes=(self.signal,),
                            where=Some(lambda context: context.window == self.view.located.window and context.area == area and context.region == self.view.region),
                        ),
                        space=type(active),
                        scissor_enabled=False,
                        respect_overlays=False,
                    ),
                )
            ).bind(lambda rows: entered(scope, tuple(acquisition for row in rows for _, acquisition in row))),
        )

    def released(self) -> BlenderResult[None]:
        """Withdraws demand and removes the transient drawing before releasing handles."""
        self.active = False
        self.cancelled()
        self.desired.clear()
        self.before = Nothing
        result = self.signal.fired()
        self.scope.close()
        self.textures.clear()
        self.planes.clear()
        self.failures.clear()
        return result

    def cancelled(self) -> None:
        """Cancels the held one-shot native dwell timer."""
        for callback in self.dwell.to_list():
            bpy.app.timers.unregister(callback)
        self.dwell = Nothing

    def observed(self, context: bpy.types.Context) -> BlenderResult[tuple[Hashable, Callable[[tuple[T, ...]], BlenderResult[tuple[bool, ...]]]]]:
        """Joins the caller's single observation with the current rendered proxy identity."""
        return self.browser.observed(context).map(lambda held: ((held[0], Tiles.held[self.identity.package].proxy.map(lambda proxy: proxy.key)), held[1]))

    def reloaded(self, context: bpy.types.Context, observation: Option[tuple[Hashable, Callable[[tuple[T, ...]], BlenderResult[tuple[bool, ...]]]]] = Nothing) -> BlenderResult[None]:
        """Reads the same catalog and derives current from locked application equality."""
        self.cancelled()
        self.desired.clear()
        self.planes.clear()
        self.textures.clear()
        self.failures.clear()
        self.before, self.hover = Nothing, Nothing
        self.rows = {stem: self.browser.listing.preset.read(value) if isinstance(value, Path) else Ok(value) for stem, value in self.browser.listing.preset.listed(self.identity).items()}
        valid = tuple((stem, held) for stem, value in self.rows.items() for held in value.to_option().to_list())

        def current(values: tuple[tuple[bool, ...], Hashable]) -> BlenderResult[None]:
            matched, revision = values
            self.current = Option.of_optional(next((stem for (stem, _), mark in zip(valid, matched) if mark), None))
            self.lead = self.lead.filter(lambda stem: stem in self.shown).or_else(self.current.filter(lambda stem: stem in self.shown))
            self.anchor = self.anchor.filter(lambda stem: stem in self.shown).or_else(self.lead)
            self.selected = self.selected.intersection(self.shown)
            self.revision = Some(revision)
            return self.rehovered().bind(lambda _: self.signal.fired())

        return (
            observation
            .map(Ok)
            .default_with(lambda: self.observed(context))
            .bind(lambda observed: observed[1](tuple(value for _, value in valid)).map(lambda matched: (matched, observed[0])))
            .bind(current)
        )

    def rehovered(self) -> BlenderResult[None]:
        """Rebinds stationary input to the newly painted cells after a real reload."""
        cells = self.cells(self.canvas)
        stem = self.pointer.bind(
            lambda pointer: Option.of_optional(
                next(
                    (
                        target
                        for target, rect in cells
                        if isinstance(target, str) and rect.x <= pointer[0] - self.view.region.x < rect.x + rect.width and rect.y <= pointer[1] - self.view.region.y < rect.y + rect.height
                    ),
                    None,
                )
            )
        )
        return self.hovered(stem, shifted=self.shift)

    # --- [GEOMETRY]

    @property
    def canvas(self) -> Canvas:
        """Derives a sidebar-sized browser beside the native main-image viewport."""
        return self.placed(Canvas.of(self.view.located.area, self.view.region))

    def placed(self, canvas: Canvas) -> Canvas:
        """Places browser bounds within the supplied native drawing canvas."""
        for origin in self.origin.to_list() if self.browser.main.admits(self.view.located.area) else ():
            width = min(origin.width, canvas.extent.width)
            bounds = msgspec.structs.replace(canvas.extent, width=width, x=canvas.extent.x if origin.alignment == "LEFT" else canvas.extent.x + canvas.extent.width - width)
            return msgspec.structs.replace(canvas, extent=bounds)
        return canvas

    @property
    def shown(self) -> tuple[str, ...]:
        """Catalog order restricted by the current display-name search."""
        return tuple(stem for stem in self.rows if self.search.casefold() in bpy.path.display_name(stem).casefold())

    def cells(self, canvas: Canvas) -> tuple[tuple[str | Verb | BrowserMode | BrowserControl | Gated[float], Rect], ...]:
        """Derives paint and hit rectangles once from the same native region metrics."""
        bounds, unit, gap = canvas.extent, canvas.metrics.unit, canvas.metrics.spacing.template
        width = bounds.width - 2 * gap
        top = bounds.y + bounds.height - gap
        controls = tuple(verb for verb in self.actions if verb is not Verb.APPLY)
        columns = max(1, math.floor(width / (unit * 6)))
        button = width / columns
        rows = math.ceil(len(controls) / columns)
        footer = bounds.y + gap + rows * unit
        toolbar: tuple[tuple[str | Verb | BrowserMode | BrowserControl | Gated[float], Rect], ...] = (
            (BrowserControl.SEARCH, Rect(x=bounds.x + gap, y=top - unit, width=width, height=unit)),
            *((mode, Rect(x=bounds.x + gap + index * width / 4, y=top - 2 * unit, width=width / 4, height=unit)) for index, mode in enumerate(BrowserMode)),
            (self.layout.size, Rect(x=bounds.x + gap + width / 2, y=top - 2 * unit, width=width / 4, height=unit)),
            (BrowserControl.SIZE, Rect(x=bounds.x + gap + width * 3 / 4, y=top - 2 * unit, width=width / 4, height=unit)),
        )
        actions = tuple((verb, Rect(x=bounds.x + gap + (index % columns) * button, y=footer - (index // columns + 1) * unit, width=button, height=unit)) for index, verb in enumerate(controls))
        names = self.shown
        size = float(canvas.metrics.scaled(self.layout.size.value)) if self.layout.size.enabled else width / 3 - gap if self.layout.mode is BrowserMode.GRID else float(canvas.metrics.scaled(48.0))
        across = (max(1, math.floor(width / (size + gap))) if self.layout.size.enabled else 3) if self.layout.mode is BrowserMode.GRID else 1
        height = size + unit if self.layout.mode is BrowserMode.GRID else size + 2 * gap
        count = max(0, math.floor((top - 2 * unit - footer - gap) / (height + gap))) * across
        tiles = tuple(
            (
                stem,
                Rect(
                    x=bounds.x + gap + (index % across) * (size + gap),
                    y=top - 2 * unit - gap - (index // across + 1) * (height + gap),
                    width=size if self.layout.mode is BrowserMode.GRID else width,
                    height=height,
                ),
            )
            for index, stem in enumerate(names[self.offset : self.offset + count])
        )
        return (*toolbar, *actions, *tiles)

    def read(self, _metrics: Metrics) -> BlenderResult[Option[bpy.types.Preferences]]:
        """Reads native preferences within this session's acquired lifetime."""
        return Ok(Some(bpy.context.preferences) if self.active else Nothing)  # type: ignore[arg-type]  # ty: ignore[invalid-return-type]

    def drawn(self, canvas: Canvas, preferences: bpy.types.Preferences) -> None:
        """Paints the native-themed browser only in its exact selected WINDOW region."""
        canvas = self.placed(canvas)
        ui = preferences.themes[0].user_interface
        points = canvas.metrics.text(preferences.ui_styles[0].widget)
        numbered = {stem: index for index, stem in enumerate(self.rows, 1)} if self.layout.mode is BrowserMode.LIST else {}
        with canvas.clipped(canvas.extent):
            canvas.frame(canvas.extent, canvas.theme.back, canvas.theme.back, 0.0)
            if not self.shown:
                canvas.label(
                    "No matching presets" if self.rows else "Add or import a preset",
                    (canvas.extent.x + canvas.metrics.spacing.template, canvas.extent.y + canvas.extent.height / 2),
                    canvas.theme.text,
                    points,
                )
            for target, rect in self.cells(canvas):
                chosen = target in self.selected if isinstance(target, str) else target is self.layout.mode
                colors = widget(
                    ui.wcol_list_item if isinstance(target, str) else ui.wcol_tool, State.SELECT if chosen else State.HOVER if isinstance(target, str) and self.hover == Some(target) else State.NONE
                )
                canvas.frame(rect, colors.inner, colors.outline, canvas.metrics.spacing.template)
                picture: Option[gpu.types.GPUTexture] = Nothing
                match target:
                    case str() as stem:
                        label = ("✓ " if self.current == Some(stem) else "") + bpy.path.display_name(stem)
                        if self.layout.mode is BrowserMode.LIST:
                            label = f"{numbered[stem]}. {label}"
                        demand = Option.of_optional(self.desired.get((stem, Rank.SHOWN)))
                        faults = demand.bind(lambda held: Option.of_optional(self.failures.get(held)).or_else(held.value.swap().to_option()))
                        label = faults.map(lambda errors: "Missing: " if any(isinstance(error, BlenderAbsentFault) for error in errors) else "Failed: ").default_value("") + label
                        picture = demand.bind(lambda held: Option.of_optional(self.textures.get(held)))
                    case Verb() as verb:
                        label = verb.title
                    case BrowserMode() as mode:
                        label = mode.label
                    case Gated(enabled=enabled):
                        label = "Fixed" if enabled else "Fit"
                    case BrowserControl.SIZE:
                        label = f"Size {self.layout.size.value:g}"
                    case BrowserControl.SEARCH:
                        label = "Search: " + self.search
                listed = isinstance(target, str) and self.layout.mode is BrowserMode.LIST
                inset = canvas.metrics.spacing.template
                image = Rect(
                    x=rect.x + inset if listed else rect.x,
                    y=rect.y + inset if listed else rect.y + canvas.metrics.unit,
                    width=rect.height - 2 * inset if listed else rect.width,
                    height=rect.height - 2 * inset if listed else rect.height - canvas.metrics.unit,
                )
                for texture in picture.to_list():
                    factor = min(image.width / texture.width, image.height / texture.height)
                    canvas.picture(texture, msgspec.structs.replace(image, width=texture.width * factor, height=texture.height * factor), gpu.shader.from_builtin("IMAGE_COLOR"))
                with canvas.clipped(rect):
                    canvas.label(label, (image.x + image.width + inset if listed else rect.x + inset, rect.y + canvas.metrics.spacing.button_y), colors.text, points)

    # --- [PREVIEWS]

    def jobs(self, _identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[Job, ...]]:
        """Owns the complete visible and dwelled demand set for the shared queue."""
        if not self.active:
            return Ok(())
        if Option.of_optional(context.window_manager).filter(lambda manager: self.view.located.window in manager.windows).is_none():
            return self.released().map(lambda _: ())
        if (live := (Located.found(self.view.located.window, self.area))).is_none():
            return self.released().map(lambda _: ())
        if live.bind(lambda at: Option.of_optional(next((region for region in at.area.regions if region.as_pointer() == self.region), None))).is_none() or any(
            origin not in self.view.located.area.regions for origin in self.origin.to_list()
        ):
            return self.released().map(lambda _: ())
        if any(
            view.located.area not in self.view.located.window.screen.areas or not self.browser.main.admits(view.located.area) or view.region not in view.located.area.regions
            for view in self.main.to_list()
        ):
            return self.released().map(lambda _: ())
        with self.view.override:
            if not all(holds(condition, bpy.context) for condition in (*self.browser.condition.to_list(), *self.browser.listing.condition.to_list())):
                return self.released().map(lambda _: ())
            return (
                self.observed(bpy.context).bind(lambda observed: self.reloaded(bpy.context, Some(observed)) if self.revision != Some(observed[0]) else self.rehovered()).map(lambda _: self.requests())
            )

    def requests(self) -> tuple[Job, ...]:
        """Retains requests across ticks and withdraws every no-longer-visible tile."""
        canvas = self.canvas
        shown = {stem: max(1, round(min(rect.width, rect.height))) for stem, rect in self.cells(canvas) if isinstance(stem, str)}
        self.desired = {place: demand for place, demand in self.desired.items() if place[1] is Rank.ACTIVE or (place[0] in shown and demand.side == shown[place[0]])}
        self.desired |= {(stem, Rank.SHOWN): Demand(stem=stem, value=self.rows[stem], side=side, rank=Rank.SHOWN) for stem, side in shown.items() if (stem, Rank.SHOWN) not in self.desired}
        kept = frozenset(self.desired.values())
        self.planes = {demand: plane for demand, plane in self.planes.items() if demand in kept}
        self.textures = {demand: texture for demand, texture in self.textures.items() if demand in kept}
        self.failures = {demand: faults for demand, faults in self.failures.items() if demand in kept}
        return tuple(Job(place=(self, *place), key=demand, rank=demand.rank, run=partial(self.rendered, demand)) for place, demand in self.desired.items() if demand not in self.planes)

    def rendered(self, demand: Demand[T]) -> BlenderResult[None]:
        """Publishes only the exact request still held after the renderer returns."""
        with self.view.override:
            context, scene = bpy.context, self.view.located.window.scene
            result = demand.value.bind(lambda value: self.browser.tiles.rendered(self.identity, partial(self.browser.candidate, self.identity, context, value), demand.side, scene=scene))
        if not self.active or self.desired.get((demand.stem, demand.rank)) is not demand:
            return Ok(None)

        def published(plane: npt.NDArray[np.float32]) -> BlenderResult[None]:
            self.planes[demand] = plane
            if demand.rank is Rank.ACTIVE:
                self.before = Some(Before(plane=plane, name=bpy.path.display_name(demand.stem)))
                return self.signal.fired()
            return uploaded(unassociated(plane)).map(lambda texture: self.textures.__setitem__(demand, texture)).bind(lambda _: self.signal.fired())

        def failed(faults: BlenderFaults) -> BlenderFaults:
            self.failures[demand] = faults
            _ = self.identity.reports.reported(self.signal.fired)()
            return faults

        return result.bind(published).map_error(failed)

    def hovered(self, stem: Option[str], *, shifted: bool) -> BlenderResult[None]:
        """Clears transient drawing on hover or Shift changes and starts one native dwell."""
        if (stem, shifted) == (self.hover, self.shift):
            return Ok(None)
        self.cancelled()
        self.hover, self.shift, self.before = stem, shifted, Nothing
        self.desired = {place: demand for place, demand in self.desired.items() if place[1] is not Rank.ACTIVE}

        def requested() -> BlenderResult[None]:
            self.dwell = Nothing

            def main(view: RegionView) -> BlenderResult[None]:
                self.main = Some(view)
                with view.override:
                    image = editor(bpy.context).bind(
                        lambda space: (
                            Option.of_optional(space.image).filter(lambda image: image.has_data).to_result_with(lambda: (BlenderAbsentFault(owner=repr(view.located.area), key="image pixels"),))
                        )
                    )
                    return image.map(
                        lambda image: self.desired.update(((name, Rank.ACTIVE), Demand(stem=name, value=self.rows[name], side=max(image.size), rank=Rank.ACTIVE)) for name in self.hover.to_list())  # type: ignore[call-overload]
                    )

            return self.browser.main.first((self.view.located.window,)).bind(RegionView.of).bind(main)

        def dwelled() -> None:
            _ = self.identity.reports.reported(requested)()

        if stem.is_some() and not shifted:
            self.dwell = Some(dwelled)
            bpy.app.timers.register(dwelled, first_interval=0.2)
        return self.signal.fired()

    # --- [EVENTS]

    def acted(self, verb: Verb, stem: Option[str] = Nothing) -> BlenderResult["rna_enums.OperatorReturnItems"]:
        """Invokes the listing's registered native action with its typed properties."""
        if verb in {Verb.EXPORT, Verb.DELETE} and not self.selected:
            return Ok("RUNNING_MODAL")
        if verb in {Verb.RENAME, Verb.REPLACE} and stem.is_none():
            return Ok("RUNNING_MODAL")
        command = self.actions[verb]
        record = (
            Selected(items=tuple(Named(stem=name) for name in self.rows if name in self.selected))
            if verb in {Verb.EXPORT, Verb.DELETE}
            else Saved(name=self.current.map(bpy.path.display_name).default_value(""))
            if verb is Verb.SAVE
            else stem.map(lambda name: Renamed(stem=name, name=bpy.path.display_name(name)) if verb is Verb.RENAME else Named(stem=name)).default_value(operators.Empty())
        )

        def invoked(binding: tuple[str, operators.Writer]) -> BlenderResult["rna_enums.OperatorReturnItems"]:
            module, _, operation = binding[0].partition(".")
            return called(getattr(getattr(bpy.ops, module), operation), msgspec.to_builtins(record), context="INVOKE_DEFAULT" if command.invoked else "EXEC_DEFAULT", undo=bool(command.undo)).map(
                lambda _: "FINISHED"
            )

        return self.hovered(Nothing, shifted=self.shift).bind(lambda _: command.invocation(self.identity)).bind(invoked)

    def handled(self, context: bpy.types.Context, event: bpy.types.Event) -> BlenderResult["rna_enums.OperatorReturnItems"]:
        """Reduces native input; Shift is observed on every event without polling."""
        if event.type == "WINDOW_DEACTIVATE" or not self.active:
            return Ok("CANCELLED")
        if not all(holds(condition, context) for condition in (*self.browser.condition.to_list(), *self.browser.listing.condition.to_list())):
            return Ok("CANCELLED")
        canvas = self.canvas
        self.pointer = Some((event.mouse_x, event.mouse_y))
        x, y = event.mouse_x - self.view.region.x, event.mouse_y - self.view.region.y
        target: Option[str | Verb | BrowserMode | BrowserControl | Gated[float]] = Option.of_optional(
            next((target for target, rect in self.cells(canvas) if rect.x <= x < rect.x + rect.width and rect.y <= y < rect.y + rect.height), None)
        )
        hovering = Option.of_optional(next((held for held in target.to_list() if isinstance(held, str)), None))
        result = self.hovered(hovering, shifted=event.shift)
        for rect in self.resizing.to_list():
            if event.type == "MOUSEMOVE" and rect.x <= x <= rect.x + rect.width:
                self.layout = msgspec.structs.replace(self.layout, size=Gated(enabled=True, value=THUMBNAIL_SIDE[0] + (x - rect.x) / rect.width * (THUMBNAIL_SIDE[1] - THUMBNAIL_SIDE[0])))
                return (
                    result
                    .bind(lambda _: self.held.write(self.view.located.window.screen, self.layout))
                    .bind(lambda _: self.hovered(Nothing, shifted=event.shift))
                    .bind(lambda _: self.rehovered())
                    .bind(lambda _: self.signal.fired())
                    .map(lambda _: "RUNNING_MODAL")
                )
            if event.type == "LEFTMOUSE" and event.value == "RELEASE":
                self.resizing = Nothing
        if event.value != "PRESS":
            return result.map(lambda _: "RUNNING_MODAL")
        match event.type:
            case "ESC" | "RIGHTMOUSE":
                return result.map(lambda _: "CANCELLED")
            case "LEFTMOUSE":
                self.searching = False
                match target.to_optional():
                    case str() as stem:
                        shown = self.shown
                        first, last = (
                            sorted((shown.index(self.anchor.some), shown.index(stem)))
                            if event.shift and self.anchor.is_some() and self.anchor.some in shown
                            else (shown.index(stem), shown.index(stem))
                        )
                        self.lead = Some(stem)
                        self.selected = self.selected ^ {stem} if event.ctrl or event.oskey else frozenset(shown[first : last + 1])
                        self.anchor = self.anchor if event.shift else Some(stem)
                        return (
                            result.bind(lambda _: self.acted(Verb.APPLY, Some(stem)))
                            if not (event.shift or event.ctrl or event.oskey)
                            else result.bind(lambda _: self.signal.fired()).map(lambda _: "RUNNING_MODAL")
                        )
                    case Verb() as verb:
                        chosen = self.lead if verb in {Verb.DELETE, Verb.EXPORT, Verb.REPLACE, Verb.RENAME} else Nothing
                        return result.bind(lambda _: self.acted(verb, chosen))
                    case BrowserMode() as mode:
                        self.layout = msgspec.structs.replace(self.layout, mode=mode)
                    case Gated(enabled=enabled, value=value):
                        self.layout = msgspec.structs.replace(self.layout, size=Gated(enabled=not enabled, value=value))
                    case BrowserControl.SIZE:
                        self.resizing = Some(next(rect for held, rect in self.cells(canvas) if held is BrowserControl.SIZE))
                    case BrowserControl.SEARCH:
                        self.searching = True
                    case None:
                        return result.map(lambda _: "CANCELLED")
                return (
                    result
                    .bind(lambda _: self.held.write(self.view.located.window.screen, self.layout))
                    .bind(lambda _: self.hovered(Nothing, shifted=event.shift))
                    .bind(lambda _: self.rehovered())
                    .bind(lambda _: self.signal.fired())
                    .map(lambda _: "RUNNING_MODAL")
                )
            case "WHEELUPMOUSE" | "WHEELDOWNMOUSE":
                tiles = tuple(rect for target, rect in self.cells(canvas) if isinstance(target, str))
                self.offset = min(max(0, len(self.shown) - len(tiles)), max(0, self.offset + (-1 if event.type == "WHEELUPMOUSE" else 1)))
            case "RET" | "NUMPAD_ENTER":
                return result.bind(lambda _: self.lead.map(lambda stem: self.acted(Verb.APPLY, Some(stem))).default_value(Ok("RUNNING_MODAL")))
            case "F2" | "DEL":
                return result.bind(lambda _: self.acted(Verb.RENAME if event.type == "F2" else Verb.DELETE, self.lead))
            case "BACK_SPACE" if self.searching:
                self.search = self.search[:-1]
                self.offset = 0
            case "A" if event.ctrl or event.oskey:
                self.selected = frozenset(self.shown)
            case "LEFT_ARROW" | "RIGHT_ARROW" | "UP_ARROW" | "DOWN_ARROW" | "HOME" | "END":
                if visible := (self.shown):
                    current = self.lead.filter(lambda stem: stem in visible).map(visible.index).default_value(0)
                    tiles = tuple(rect for target, rect in self.cells(canvas) if isinstance(target, str))
                    across = sum(rect.y == tiles[0].y for rect in tiles) if tiles else 1
                    index = {
                        "HOME": 0,
                        "END": len(visible) - 1,
                        "LEFT_ARROW": (current - 1) % len(visible),
                        "RIGHT_ARROW": (current + 1) % len(visible),
                        "UP_ARROW": (current - across) % len(visible),
                        "DOWN_ARROW": (current + across) % len(visible),
                    }[event.type]
                    self.lead = Some(visible[index])
                    first, last = sorted((self.anchor.filter(lambda stem: event.shift and stem in visible).map(visible.index).default_value(index), index))
                    self.selected = frozenset(visible[first : last + 1])
                    self.anchor = self.anchor if event.shift else self.lead
                    if index < self.offset or index >= self.offset + len(tiles):
                        self.offset = index // across * across
            case _ if event.unicode and event.unicode.isdecimal() and not self.searching:
                index = int(event.unicode) - 1
                for stem in tuple(self.rows)[index : index + 1] if 0 <= index < 9 else ():
                    if self.search.casefold() in bpy.path.display_name(stem).casefold():
                        return result.bind(lambda _: self.acted(Verb.APPLY, Some(stem)))
            case _ if event.unicode and self.searching:
                self.search += event.unicode
                self.offset = 0
        self.selected = self.selected.intersection(self.shown)
        self.lead = self.lead.filter(lambda stem: stem in self.shown)
        self.anchor = self.anchor.filter(lambda stem: stem in self.shown)
        return result.bind(lambda _: self.rehovered()).bind(lambda _: self.signal.fired()).map(lambda _: "RUNNING_MODAL")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Browser", "BrowserLayout", "BrowserMode"]
