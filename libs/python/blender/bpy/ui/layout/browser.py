"""Preset browsing over native modal events, GPU drawing, and the shared preview queue."""

from collections.abc import Callable, Hashable, Iterable, Mapping
from contextlib import ExitStack
from enum import auto, Enum
from functools import partial
import math
from pathlib import Path
from typing import Annotated, ClassVar, Literal, Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import gpu
import msgspec
from msgspec.structs import replace
import numpy as np
import numpy.typing as npt

from blender.bpy.commands import operators
from blender.bpy.commands.calls import called
from blender.bpy.commands.modal import Gesture, Modal, Travel
from blender.bpy.data.rna.conditions import AllOf, holds
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, entered
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderOperatorFault, BlenderResult
from blender.bpy.render.previews.queue import Job, PreviewQueue, Rank
from blender.bpy.render.previews.scenes import Chain
from blender.bpy.render.previews.tiles import Tiles
from blender.bpy.render.review.frames import Before, editor, Wipe
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.ui.drawing.canvas import Canvas, Rect
from blender.bpy.ui.drawing.handlers import Drawing, Overlay, Signal
from blender.bpy.ui.layout.rows import Line, OperatorButton, realized, RowDraw
from blender.bpy.ui.layout.sections import Action, Listing, Named, Renamed, Saved, Selected, Verb
from blender.bpy.ui.resources.textures import uploaded
from blender.bpy.ui.resources.theme import Metrics, State, widget
from blender.bpy.ui.screens.areas import AreaQuery, Located
from blender.bpy.ui.screens.regions import RegionView
from blender.imaging.color.processors import unassociated
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Choice, Gated, Presentation

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class BrowserMode(Choice):
    """Persistent thumbnail and compact row presentations."""

    GRID = 0
    LIST = 1


class BrowserControl(Enum):
    """Browser's text entry and thumbnail-size controls."""

    SEARCH = auto()
    SIZE = auto()


type Target = str | Verb | BrowserMode | BrowserControl | Gated[float]
type Cells = tuple[tuple[Target, Rect], ...]
type Reply = Act | Literal["RUNNING_MODAL", "CANCELLED"]
type Observation[T] = tuple[Hashable, Callable[[tuple[T, ...]], BlenderResult[tuple[bool, ...]]]]

# --- [CONSTANTS] ------------------------------------------------------------------------

THUMBNAIL_SIDE = (32.0, 256.0)


# --- [MODELS] ---------------------------------------------------------------------------


class BrowserLayout(msgspec.Struct, frozen=True, kw_only=True):
    """Screen's browser layout, with disabled size fitting the available browser width."""

    mode: BrowserMode = BrowserMode.GRID
    size: Annotated[Gated[Annotated[float, msgspec.Meta(ge=THUMBNAIL_SIDE[0], le=THUMBNAIL_SIDE[1])]], Presentation(decimals=Some(0))] = Gated(enabled=False, value=128.0)


class Demand[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Retained identity of one visible tile or one completed hover dwell."""

    stem: str
    value: BlenderResult[T]
    side: int
    rank: Rank


class Catalog[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Listing rows by stem in listing order, the row the current values match, and the observation revision read."""

    rows: Mapping[str, BlenderResult[T]]
    current: Option[str]
    revision: Hashable


class Act(msgspec.Struct, frozen=True, kw_only=True):
    """Listing action an event requests and the complete record its operator runs over."""

    verb: Verb
    record: msgspec.Struct


class Browsing(msgspec.Struct, frozen=True, kw_only=True):
    """Value each browser event returns: layout, pointer, search, scroll, resize drag, and selection."""

    layout: BrowserLayout
    pointer: Option[tuple[int, int]] = Nothing
    shift: bool = False
    search: str = ""
    searching: bool = False
    resizing: Option[Rect] = Nothing
    offset: int = 0
    lead: Option[str] = Nothing
    anchor: Option[str] = Nothing
    selected: frozenset[str] = frozenset()

    def shown(self, rows: Iterable[str]) -> tuple[str, ...]:
        """Catalog order restricted by the display-name search."""
        return tuple(stem for stem in rows if self.search.casefold() in bpy.path.display_name(stem).casefold())

    def settled[T: msgspec.Struct](self, catalog: Catalog[T]) -> Self:
        """Restricts lead, anchor, and selection to shown rows, the lead falling back to the current row."""
        shown = self.shown(catalog.rows)

        def visible(stem: str) -> bool:
            return stem in shown

        lead = self.lead.filter(visible).or_else(catalog.current.filter(visible))
        return replace(self, lead=lead, anchor=self.anchor.filter(visible).or_else(lead), selected=self.selected.intersection(shown))

    def cells[T: msgspec.Struct](self, canvas: Canvas, catalog: Catalog[T], verbs: Iterable[Verb]) -> Cells:
        """Derives paint and hit rectangles once from the same native region metrics."""
        bounds, unit, gap = canvas.extent, canvas.metrics.unit, canvas.metrics.spacing.template
        width = bounds.width - 2 * gap
        top = bounds.y + bounds.height - gap
        controls = tuple(verb for verb in verbs if verb is not Verb.APPLY)
        columns = max(1, math.floor(width / (unit * 6)))
        button = width / columns
        footer = bounds.y + gap + math.ceil(len(controls) / columns) * unit
        toolbar: Cells = (
            (BrowserControl.SEARCH, Rect(x=bounds.x + gap, y=top - unit, width=width, height=unit)),
            *((mode, Rect(x=bounds.x + gap + index * width / 4, y=top - 2 * unit, width=width / 4, height=unit)) for index, mode in enumerate(BrowserMode)),
            (self.layout.size, Rect(x=bounds.x + gap + width / 2, y=top - 2 * unit, width=width / 4, height=unit)),
            (BrowserControl.SIZE, Rect(x=bounds.x + gap + width * 3 / 4, y=top - 2 * unit, width=width / 4, height=unit)),
        )
        actions = tuple((verb, Rect(x=bounds.x + gap + (index % columns) * button, y=footer - (index // columns + 1) * unit, width=button, height=unit)) for index, verb in enumerate(controls))
        grid = self.layout.mode is BrowserMode.GRID
        size = float(canvas.metrics.scaled(self.layout.size.value)) if self.layout.size.enabled else width / 3 - gap if grid else float(canvas.metrics.scaled(48.0))
        across = (max(1, math.floor(width / (size + gap))) if self.layout.size.enabled else 3) if grid else 1
        height = size + unit if grid else size + 2 * gap
        count = max(0, math.floor((top - 2 * unit - footer - gap) / (height + gap))) * across
        tiles = tuple(
            (stem, Rect(x=bounds.x + gap + (index % across) * (size + gap), y=top - 2 * unit - gap - (index // across + 1) * (height + gap), width=size if grid else width, height=height))
            for index, stem in enumerate(self.shown(catalog.rows)[self.offset : self.offset + count])
        )
        return (*toolbar, *actions, *tiles)

    def act[T: msgspec.Struct](self, verb: Verb, stem: Option[str], catalog: Catalog[T]) -> Reply:
        """Builds the verb's record from the selection, the current row, or the stem, staying modal where the verb lacks its subject."""
        match verb, stem.to_optional():
            case Verb.EXPORT | Verb.DELETE, _:
                chosen = tuple(Named(stem=name) for name in catalog.rows if name in self.selected)
                return Act(verb=verb, record=Selected(items=chosen)) if chosen else "RUNNING_MODAL"
            case Verb.SAVE, _:
                return Act(verb=verb, record=Saved(name=catalog.current.map(bpy.path.display_name).default_value("")))
            case Verb.RENAME, str(name):
                return Act(verb=verb, record=Renamed(stem=name, name=bpy.path.display_name(name)))
            case Verb.RENAME | Verb.REPLACE, None:
                return "RUNNING_MODAL"
            case _, str(name):
                return Act(verb=verb, record=Named(stem=name))
            case _, None:
                return Act(verb=verb, record=operators.Empty())

    def stepped[T: msgspec.Struct](self, event: bpy.types.Event, point: tuple[int, int], target: Option[Target], cells: Cells, catalog: Catalog[T]) -> tuple[Self, Reply]:
        """Answers the value after one event over the cells it hit and the reply the session carries out."""
        shown, chord = self.shown(catalog.rows), event.ctrl or event.oskey
        tiles = tuple(rect for held, rect in cells if isinstance(held, str))
        match self.resizing.to_optional(), event.type, event.value, target.to_optional():
            case Rect() as rect, "MOUSEMOVE", _, _ if rect.x <= point[0] <= rect.x + rect.width:
                low, high = THUMBNAIL_SIDE
                return replace(self, layout=replace(self.layout, size=Gated(enabled=True, value=low + (point[0] - rect.x) / rect.width * (high - low)))), "RUNNING_MODAL"
            case Rect(), "LEFTMOUSE", "RELEASE", _:
                return replace(self, resizing=Nothing), "RUNNING_MODAL"
            case _, _, value, _ if value != "PRESS":
                return self, "RUNNING_MODAL"
            case _, "ESC" | "RIGHTMOUSE", _, _:
                return self, "CANCELLED"
            case _, "LEFTMOUSE", _, None:
                return self, "CANCELLED"
            case _, "LEFTMOUSE", _, str(stem):
                span = sorted({shown.index(stem), *(shown.index(held) for held in self.anchor.to_list() if event.shift)})
                following = replace(
                    self, searching=False, lead=Some(stem), selected=self.selected ^ {stem} if chord else frozenset(shown[span[0] : span[-1] + 1]), anchor=self.anchor if event.shift else Some(stem)
                )
                return following, "RUNNING_MODAL" if event.shift or chord else following.act(Verb.APPLY, Some(stem), catalog)
            case _, "LEFTMOUSE", _, Verb() as verb:
                return replace(self, searching=False), self.act(verb, self.lead if verb in {Verb.DELETE, Verb.EXPORT, Verb.REPLACE, Verb.RENAME} else Nothing, catalog)
            case _, "LEFTMOUSE", _, BrowserMode() as mode:
                return replace(self, searching=False, layout=replace(self.layout, mode=mode)), "RUNNING_MODAL"
            case _, "LEFTMOUSE", _, Gated(enabled=enabled, value=value):
                return replace(self, searching=False, layout=replace(self.layout, size=Gated(enabled=not enabled, value=value))), "RUNNING_MODAL"
            case _, "LEFTMOUSE", _, BrowserControl.SIZE:
                return replace(self, searching=False, resizing=Option.of_optional(next((rect for held, rect in cells if held is BrowserControl.SIZE), None))), "RUNNING_MODAL"
            case _, "LEFTMOUSE", _, BrowserControl.SEARCH:
                return replace(self, searching=True), "RUNNING_MODAL"
            case _, "WHEELUPMOUSE" | "WHEELDOWNMOUSE", _, _:
                return replace(self, offset=min(max(0, len(shown) - len(tiles)), max(0, self.offset + (-1 if event.type == "WHEELUPMOUSE" else 1)))), "RUNNING_MODAL"
            case _, "RET" | "NUMPAD_ENTER", _, _:
                return self, self.lead.map(lambda stem: self.act(Verb.APPLY, Some(stem), catalog)).default_value("RUNNING_MODAL")
            case _, "F2" | "DEL", _, _:
                return self, self.act(Verb.RENAME if event.type == "F2" else Verb.DELETE, self.lead, catalog)
            case _, "BACK_SPACE", _, _ if self.searching:
                return replace(self, search=self.search[:-1], offset=0), "RUNNING_MODAL"
            case _, "A", _, _ if chord:
                return replace(self, selected=frozenset(shown)), "RUNNING_MODAL"
            case _, "LEFT_ARROW" | "RIGHT_ARROW" | "UP_ARROW" | "DOWN_ARROW" | "HOME" | "END", _, _ if shown:
                current = self.lead.map(shown.index).default_value(0)
                across = sum(rect.y == tiles[0].y for rect in tiles) if tiles else 1
                index = {
                    "HOME": 0,
                    "END": len(shown) - 1,
                    "LEFT_ARROW": (current - 1) % len(shown),
                    "RIGHT_ARROW": (current + 1) % len(shown),
                    "UP_ARROW": (current - across) % len(shown),
                    "DOWN_ARROW": (current + across) % len(shown),
                }[event.type]
                span = sorted({index, *(shown.index(held) for held in self.anchor.to_list() if event.shift)})
                return replace(
                    self,
                    lead=Some(shown[index]),
                    selected=frozenset(shown[span[0] : span[-1] + 1]),
                    anchor=self.anchor if event.shift else Some(shown[index]),
                    offset=self.offset if self.offset <= index < self.offset + len(tiles) else index // across * across,
                ), "RUNNING_MODAL"
            case _ if event.unicode.isdecimal() and not self.searching:
                index = int(event.unicode) - 1
                return self, next((self.act(Verb.APPLY, Some(stem), catalog) for stem in tuple(catalog.rows)[index : index + 1] if 0 <= index < 9 and stem in shown), "RUNNING_MODAL")
            case _ if event.unicode and self.searching:
                return replace(self, search=self.search + event.unicode, offset=0), "RUNNING_MODAL"
            case _:
                return self, "RUNNING_MODAL"


class Spot(msgspec.Struct, frozen=True, kw_only=True):
    """Browser region and the region it opened from, valid during one native call."""

    view: RegionView
    origin: Option[bpy.types.Region]

    def placed(self, canvas: Canvas, main: AreaQuery) -> Canvas:
        """Narrows the canvas to its origin sidebar's width and side when the browser opened over the main editor."""
        return next(
            (
                replace(canvas, extent=replace(canvas.extent, width=width, x=canvas.extent.x if origin.alignment == "LEFT" else canvas.extent.x + canvas.extent.width - width))
                for origin in (self.origin.to_list() if main.admits(self.view.located.area) else ())
                for width in (min(origin.width, canvas.extent.width),)
            ),
            canvas,
        )

    def canvas(self, main: AreaQuery) -> Canvas:
        """Browser canvas over the region's visible extent."""
        return self.placed(Canvas.of(self.view.located.area, self.view.region), main)


class Place(msgspec.Struct, frozen=True, kw_only=True):
    """Session pointers of a region, its area and window, and the region the browser opened from."""

    window: int
    area: int
    region: int
    origin: Option[int] = Nothing

    @classmethod
    def of(cls, view: RegionView, origin: Option[bpy.types.Region] = Nothing) -> Self:
        """Holds the pointers of a live region view and origin."""
        return cls(window=view.located.window.as_pointer(), area=view.located.area.as_pointer(), region=view.region.as_pointer(), origin=origin.map(lambda region: region.as_pointer()))

    def within(self, context: bpy.types.Context) -> bool:
        """Whether the context names this exact window, area, and region."""
        return tuple(None if held is None else held.as_pointer() for held in (context.window, context.area, context.region)) == (self.window, self.area, self.region)

    def resolved(self, context: bpy.types.Context) -> BlenderResult[Spot]:
        """Answers the live region and origin, absent once any of them closed."""

        def region(area: bpy.types.Area, pointer: int) -> Option[bpy.types.Region]:
            return Option.of_optional(next((region for region in area.regions if region.as_pointer() == pointer), None))

        return (
            Option
            .of_optional(context.window_manager)
            .bind(lambda manager: Option.of_optional(next((window for window in manager.windows if window.as_pointer() == self.window), None)))
            .bind(lambda window: Located.found(window, self.area))
            .bind(lambda at: region(at.area, self.region).map(lambda found: RegionView(located=at, region=found)))
            .bind(lambda view: self.origin.map(lambda pointer: region(view.located.area, pointer).map(Some)).default_value(Some(Nothing)).map(lambda origin: Spot(view=view, origin=origin)))
            .to_result_with(lambda: (BlenderAbsentFault(owner=repr(context.window_manager), key=self.region),))
        )


class Browsed[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Value a session publishes to its drawings, queue feed, and dwell: view, catalog, hover, main editor, demands, and rendered tiles."""

    view: Browsing
    catalog: Catalog[T]
    hover: tuple[Option[str], bool] = (Nothing, False)
    main: Option[Place] = Nothing
    demands: Mapping[tuple[str, Rank], Demand[T]] = msgspec.field(default_factory=dict)
    outcomes: Mapping[Demand[T], BlenderResult[gpu.types.GPUTexture | Before]] = msgspec.field(default_factory=dict)

    def reloaded(self, catalog: Catalog[T]) -> Self:
        """Replaces the catalog, dropping every demand over its earlier rows."""
        return replace(self, catalog=catalog, demands={}, outcomes={})

    def demanded(self, sides: Mapping[str, int]) -> Self:
        """Holds one demand per painted tile at its side beside the dwelled demand, keeping outcomes of held demands."""
        demands = {(stem, Rank.SHOWN): Demand(stem=stem, value=self.catalog.rows[stem], side=side, rank=Rank.SHOWN) for stem, side in sides.items()} | {
            place: demand for place, demand in self.demands.items() if place[1] is Rank.ACTIVE or sides.get(place[0]) == demand.side
        }
        kept = frozenset(demands.values())
        return replace(self, demands=demands, outcomes={demand: outcome for demand, outcome in self.outcomes.items() if demand in kept})

    def hovered(self, stem: Option[str], *, shifted: bool) -> Self:
        """Marks the hovered preset and Shift state, dropping the dwelled demand."""
        return replace(self, hover=(stem, shifted), demands={place: demand for place, demand in self.demands.items() if place[1] is not Rank.ACTIVE})


class Browser[T: msgspec.Struct](Line, frozen=True, kw_only=True):
    """One existing preset listing with caller-owned recipes and the screen's layout."""

    layouts: ClassVar[Attachment[BrowserLayout]] = Attachment(key="browser_layout", form=BrowserLayout, owner=bpy.types.Screen)
    listing: Listing[T]
    tiles: Tiles
    candidate: Callable[[Identity, bpy.types.Context, T, Option[npt.NDArray[np.float32]]], BlenderResult[Chain]]
    observed: Callable[[bpy.types.Context], BlenderResult[Observation[T]]]
    main: AreaQuery

    @property
    def key(self) -> str:
        """Native operator key derived from the listing it opens."""
        return f"browse_{self.listing.preset.key}"

    @property
    def requirement(self) -> AllOf:
        """Row and listing conditions the browser opens and stays open under."""
        return AllOf(conditions=(*self.condition.to_list(), *self.listing.condition.to_list()))

    def observation(self, identity: Identity, context: bpy.types.Context) -> BlenderResult[Observation[T]]:
        """Joins the caller's observation with the rendered proxy every tile renders from."""
        return self.observed(context).map(lambda held: ((held[0], Tiles.held[identity.package].proxy.map(lambda proxy: proxy.key)), held[1]))

    def catalogued(self, identity: Identity, observation: Observation[T]) -> BlenderResult[Catalog[T]]:
        """Reads the listing and marks the row the observation matches."""
        revision, matches = observation
        rows = {stem: self.listing.preset.read(value) if isinstance(value, Path) else Ok(value) for stem, value in self.listing.preset.listed(identity).items()}
        valid = tuple((stem, held) for stem, value in rows.items() for held in value.to_option().to_list())
        return matches(tuple(held for _, held in valid)).map(
            lambda marks: Catalog(rows=rows, current=Option.of_optional(next((stem for (stem, _), mark in zip(valid, marks) if mark), None)), revision=revision)
        )

    def command(self, identity: Identity) -> operators.Operator[operators.Empty, "Session[T]"]:
        """Builds the native modal operator over this catalog and resource scope."""

        def begin(context: bpy.types.Context, travel: Travel) -> BlenderResult[Session[T]]:
            origin = Option.of_optional(context.region)
            located = Located.current(context).bind(RegionView.of)

            def admitted(view: RegionView) -> BlenderResult[RegionView]:
                area = view.located.area
                sidebar = any(region.type == "UI" for region in origin.to_list())
                return Error((BlenderAbsentFault(owner=repr(area), key="sidebar origin or a distinct browser editor"),)) if self.main.admits(area) and not sidebar else Ok(view)

            def opened(view: RegionView, attached: Attached[BrowserLayout], layout: BrowserLayout, catalog: Catalog[T]) -> BlenderResult[Session[T]]:
                browsing = Browsing(layout=layout, pointer=origin.map(lambda region: (travel.x + region.x, travel.y + region.y)), shift=travel.precise)
                session = Session(
                    browser=self, identity=identity, place=Place.of(view, origin), attached=attached, actions=self.listing.commands(identity).verbs, held=Some(Browsed(view=browsing, catalog=catalog))
                )
                return session.acquired(Spot(view=view, origin=origin)).map(lambda _: session)

            return paired(located.bind(admitted), Browser.layouts.bound(identity)).bind(
                lambda pair: paired(pair[1].read(pair[0].located.window.screen), self.observation(identity, context).bind(lambda seen: self.catalogued(identity, seen))).bind(
                    lambda read: opened(*pair, *read)
                )
            )

        return operators.Operator(
            key=self.key,
            title="Browse Presets",
            description="Browse, preview, and apply this section's presets",
            icon=Some("PRESET"),
            condition=Some(self.requirement),
            form=operators.Empty,
            run=lambda _call: Ok(operators.Done()),
            undo=False,
            invoke=Some(
                Modal(
                    gesture=Gesture.TRACK,
                    begin=begin,
                    preview=lambda _context, _state, _travel: Ok(Nothing),
                    restore=lambda _context, _state: Ok(None),
                    event=Some(lambda context, session, event: session.handled(context, event).map(lambda reply: (session, reply))),
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


class Session[T: msgspec.Struct](msgspec.Struct, kw_only=True, eq=False):
    """One browser's acquisition scope, owning the browsed value it publishes, Nothing once released."""

    browser: Browser[T]
    identity: Identity
    place: Place
    attached: Attached[BrowserLayout]
    actions: Mapping[Verb, Action]
    held: Option[Browsed[T]]
    scope: ExitStack = msgspec.field(default_factory=ExitStack)
    dwell: ExitStack = msgspec.field(default_factory=ExitStack)
    signal: Signal = msgspec.field(default_factory=Signal)

    # --- [LIFECYCLE]

    def published(self, following: Callable[[Browsed[T]], Browsed[T]]) -> None:
        """Replaces the live browsed value with its successor, publishing nothing once released."""
        self.held = self.held.map(following)

    def acquired(self, spot: Spot) -> BlenderResult[None]:
        """Acquires both drawings, the queue feed, and the dwell into the session's scope, then binds the pointer."""
        area = spot.view.located.area
        with ExitStack() as scope:
            scope.enter_context(self.dwell)
            scope.enter_context(PreviewQueue.fed(self.identity, self.jobs))
            space: BlenderResult[bpy.types.Space] = Option.of_optional(area.spaces.active).to_result_with(lambda: (BlenderAbsentFault(owner=repr(area), key="spaces.active"),))
            result = (
                Result[bpy.types.Space, BlenderFaults]
                .bind(
                    space,
                    lambda active: accumulated((
                        Overlay(
                            drawing=Drawing(
                                area=self.browser.main,
                                part=Wipe(identity=self.identity, source=self.before),
                                refreshes=(self.signal,),
                                where=Some(lambda context: any(place.within(context) for state in self.held.to_list() for place in state.main.to_list())),
                            ),
                            space=bpy.types.SpaceImageEditor,
                            scissor_enabled=False,
                            respect_overlays=False,
                        ).acquisitions(self.identity),
                        Overlay(
                            drawing=Drawing(area=AreaQuery(space=area.type), part=self, refreshes=(self.signal,), where=Some(self.place.within)),
                            space=type(active),
                            scissor_enabled=False,
                            respect_overlays=False,
                        ).acquisitions(self.identity),
                    )),
                )
                .bind(lambda rows: entered(scope, tuple(acquisition for row in rows for _, acquisition in row)))
                .bind(lambda _: self.rehovered(spot))
                .bind(lambda _: self.signal.fired())
            )
            if result.is_ok():
                self.scope.push(scope.pop_all())
            return result

    def released(self) -> BlenderResult[None]:
        """Drops the browsed value, refreshes both drawings so they clear, then closes the session's scope."""
        self.held = Nothing
        fired = self.signal.fired()
        self.scope.close()
        return fired

    # --- [DRAWING]

    def read(self, metrics: Metrics) -> BlenderResult[Option[bpy.types.Preferences]]:
        """Reads the preferences the browser takes its theme and text style from."""
        return Ok(Some(metrics.preferences))

    def before(self, _metrics: Metrics) -> BlenderResult[Option[Before]]:
        """Reads the dwelled preset's rendered frame for the main editor's wipe."""
        outcomes = (state.outcomes.get(demand) for state in self.held.to_list() for demand in state.demands.values() if demand.rank is Rank.ACTIVE)
        match next(outcomes, None):
            case Result(tag="ok", ok=Before() as before):
                return Ok(Some(before))
            case _:
                return Ok(Nothing)

    def drawn(self, canvas: Canvas, preferences: bpy.types.Preferences) -> None:
        """Paints the native-themed browser in its exact region."""
        ui, points, inset = preferences.themes[0].user_interface, canvas.metrics.text(preferences.ui_styles[0].widget), canvas.metrics.spacing.template
        shader = gpu.shader.from_builtin("IMAGE_COLOR")
        located = ((state, spot.placed(canvas, self.browser.main)) for state in self.held.to_list() for spot in self.place.resolved(bpy.context).to_option().to_list())
        for state, placed in located:
            view, catalog = state.view, state.catalog
            numbered = {stem: index for index, stem in enumerate(catalog.rows, 1)} if view.layout.mode is BrowserMode.LIST else {}
            with placed.clipped(placed.extent):
                placed.frame(placed.extent, placed.theme.back, placed.theme.back, 0.0)
                if not view.shown(catalog.rows):
                    empty = "No matching presets" if catalog.rows else "Add or import a preset"
                    placed.label(empty, (placed.extent.x + inset, placed.extent.y + placed.extent.height / 2), placed.theme.text, points)
                for target, rect in view.cells(placed, catalog, self.actions):
                    listed = isinstance(target, str) and view.layout.mode is BrowserMode.LIST
                    chosen = target in view.selected if isinstance(target, str) else target is view.layout.mode
                    colors = widget(
                        ui.wcol_list_item if isinstance(target, str) else ui.wcol_tool,
                        State.SELECT if chosen else State.HOVER if isinstance(target, str) and state.hover[0] == Some(target) else State.NONE,
                    )
                    label, picture = captioned(state, target, numbered)
                    image = Rect(
                        x=rect.x + inset if listed else rect.x,
                        y=rect.y + inset if listed else rect.y + placed.metrics.unit,
                        width=rect.height - 2 * inset if listed else rect.width,
                        height=rect.height - 2 * inset if listed else rect.height - placed.metrics.unit,
                    )
                    placed.frame(rect, colors.inner, colors.outline, inset)
                    for texture, factor in ((texture, min(image.width / texture.width, image.height / texture.height)) for texture in picture.to_list()):
                        placed.picture(texture, replace(image, width=texture.width * factor, height=texture.height * factor), shader)
                    with placed.clipped(rect):
                        placed.label(label, (image.x + image.width + inset if listed else rect.x + inset, rect.y + placed.metrics.spacing.button_y), colors.text, points)

    # --- [PREVIEWS]

    def jobs(self, _identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[Job, ...]]:
        """Answers the shown and dwelled demand, releasing the session once its region, main editor, or conditions are gone."""
        live = self.held.bind(lambda state: self.place.resolved(context).to_option().map(lambda spot: (state, spot))).filter(
            lambda pair: all(place.resolved(context).to_option().filter(lambda spot: self.browser.main.admits(spot.view.located.area)).is_some() for place in pair[0].main.to_list())
        )
        match live.to_optional():
            case None:
                return self.released().map(lambda _: ())
            case (Browsed() as state, Spot() as spot):
                with spot.view.override:
                    return (
                        self.browser
                        .observation(self.identity, bpy.context)
                        .bind(
                            lambda seen: (
                                self.rehovered(spot)
                                if seen[0] == state.catalog.revision
                                else self.browser.catalogued(self.identity, seen).bind(lambda catalog: self.reloaded(spot, catalog, shifted=state.hover[1]))
                            )
                        )
                        .map(lambda _: self.requests(spot))
                        if holds(self.browser.requirement, bpy.context)
                        else self.released().map(lambda _: ())
                    )

    def reloaded(self, spot: Spot, catalog: Catalog[T], *, shifted: bool) -> BlenderResult[None]:
        """Publishes the new catalog and rebinds the pointer to the repainted cells."""
        self.published(lambda state: state.reloaded(catalog))
        return self.hovered(Nothing, shifted=shifted).bind(lambda _: self.rehovered(spot)).bind(lambda _: self.signal.fired())

    def requests(self, spot: Spot) -> tuple[Job, ...]:
        """Publishes the painted tiles' demands, answering a job per demand not yet rendered."""
        canvas = spot.canvas(self.browser.main)
        self.published(
            lambda state: state.demanded({stem: max(1, round(min(rect.width, rect.height))) for stem, rect in state.view.cells(canvas, state.catalog, self.actions) if isinstance(stem, str)})
        )
        return tuple(
            Job(place=place, key=demand, rank=demand.rank, run=partial(self.rendered, demand))
            for state in self.held.to_list()
            for place, demand in state.demands.items()
            if demand not in state.outcomes or state.outcomes[demand].is_error()
        )

    def rendered(self, demand: Demand[T]) -> BlenderResult[None]:
        """Renders one demand in the browser's region and publishes its outcome while the session still holds that demand."""

        def published(spot: Spot) -> BlenderResult[None]:
            with spot.view.override:
                plane = demand.value.bind(
                    lambda value: self.browser.tiles.rendered(self.identity, partial(self.browser.candidate, self.identity, bpy.context, value), demand.side, scene=spot.view.located.window.scene)
                )
            if self.held.filter(lambda state: state.demands.get((demand.stem, demand.rank)) is demand).is_none():
                return Ok(None)
            outcome: BlenderResult[gpu.types.GPUTexture | Before] = plane.bind(
                lambda pixels: Ok(Before(plane=pixels, name=bpy.path.display_name(demand.stem))) if demand.rank is Rank.ACTIVE else uploaded(unassociated(pixels))
            )
            self.published(lambda state: replace(state, outcomes={**state.outcomes, demand: outcome}))
            return paired(outcome, self.signal.fired()).map(lambda _: None)

        return self.place.resolved(bpy.context).bind(published)

    def dwelled(self, stem: str) -> BlenderResult[Option[float]]:
        """Demands the dwelled preset at the main editor image's size, then stops its timer."""

        def demanded(view: RegionView) -> BlenderResult[None]:
            with view.override:
                image = editor(bpy.context).bind(
                    lambda space: Option.of_optional(space.image).filter(lambda held: held.has_data).to_result_with(lambda: (BlenderAbsentFault(owner=repr(view.located.area), key="image pixels"),))
                )
            side = image.map(lambda held: int(np.asarray(held.size).max()))
            self.published(
                lambda state: replace(
                    state,
                    main=Some(Place.of(view)),
                    demands=side.map(lambda held: {**state.demands, (stem, Rank.ACTIVE): Demand(stem=stem, value=state.catalog.rows[stem], side=held, rank=Rank.ACTIVE)}).default_value(state.demands),
                )
            )
            return side.map(lambda _: None)

        return self.place.resolved(bpy.context).bind(lambda spot: self.browser.main.first((spot.view.located.window,))).bind(RegionView.of).bind(demanded).map(lambda _: Nothing)

    def hovered(self, target: Option[Target], *, shifted: bool) -> BlenderResult[None]:
        """Restarts the dwell and drops the dwelled demand when the hovered preset or Shift changes."""
        stem = target.bind(lambda held: Some(held) if isinstance(held, str) else Nothing)
        if self.held.filter(lambda state: state.hover != (stem, shifted)).is_none():
            return Ok(None)
        self.dwell.close()
        self.published(lambda state: state.hovered(stem, shifted=shifted))
        armed = (
            stem
            .filter(lambda _: not shifted)
            .map(
                lambda held: (
                    Timer(body=partial(self.dwelled, held), first_interval=0.2).acquisitions(self.identity).bind(lambda rows: entered(self.dwell, tuple(acquisition for _, acquisition in rows)))
                )
            )
            .default_value(Ok(None))
        )
        return paired(armed, self.signal.fired()).map(lambda _: None)

    def rehovered(self, spot: Spot) -> BlenderResult[None]:
        """Rebinds the stationary pointer to the cells now painted."""
        region = spot.view.region
        canvas = spot.canvas(self.browser.main)
        return self.held.map(
            lambda state: self.hovered(
                state.view.pointer.bind(lambda pointer: hit(state.view.cells(canvas, state.catalog, self.actions), (pointer[0] - region.x, pointer[1] - region.y))), shifted=state.view.shift
            )
        ).default_value(Ok(None))

    # --- [EVENTS]

    def acted(self, act: Act, shifted: bool) -> BlenderResult["rna_enums.OperatorReturnItems"]:
        """Invokes the listing's registered operator over the act's record, its CANCELLED closing the browser without a fault."""
        command = self.actions[act.verb]

        def invoked(binding: tuple[str, operators.Writer]) -> BlenderResult["rna_enums.OperatorReturnItems"]:
            module, _, operation = binding[0].partition(".")
            match called(getattr(getattr(bpy.ops, module), operation), msgspec.to_builtins(act.record), context="INVOKE_DEFAULT" if command.invoked else "EXEC_DEFAULT", undo=bool(command.undo)):
                case Result(tag="ok"):
                    return Ok("FINISHED")
                case Result(error=(BlenderOperatorFault(error=answer),)) if answer == frozenset({"CANCELLED"}):
                    return Ok("CANCELLED")
                case Result(error=faults):
                    return Error(faults)

        return self.hovered(Nothing, shifted=shifted).bind(lambda _: command.invocation(self.identity)).bind(invoked)

    def handled(self, context: bpy.types.Context, event: bpy.types.Event) -> BlenderResult["rna_enums.OperatorReturnItems"]:
        """Steps the browsed view over one event, publishes it, then carries out its dwell, layout write, redraw, or listing action."""
        live = self.held.bind(lambda state: self.place.resolved(context).to_option().map(lambda spot: (state, spot))).filter(
            lambda _: event.type != "WINDOW_DEACTIVATE" and holds(self.browser.requirement, context)
        )
        match live.to_optional():
            case None:
                return Ok("CANCELLED")
            case (Browsed() as state, Spot() as spot):
                view = state.view.settled(state.catalog)
                point = (event.mouse_x - spot.view.region.x, event.mouse_y - spot.view.region.y)
                cells = view.cells(spot.canvas(self.browser.main), state.catalog, self.actions)
                target = hit(cells, point)
                following, reply = replace(view, pointer=Some((event.mouse_x, event.mouse_y)), shift=event.shift).stepped(event, point, target, cells, state.catalog)
                self.published(lambda held: replace(held, view=following))
                hovered = self.hovered(target, shifted=event.shift)
                relaid = following.layout != view.layout
                match reply:
                    case Act() as act:
                        return hovered.bind(lambda _: self.acted(act, event.shift))
                    case "CANCELLED":
                        return hovered.map(lambda _: "CANCELLED")
                    case "RUNNING_MODAL":
                        return (
                            hovered
                            .bind(lambda _: self.attached.write(spot.view.located.window.screen, following.layout).bind(lambda _: self.hovered(Nothing, shifted=event.shift)) if relaid else Ok(None))
                            .bind(lambda _: self.rehovered(spot).bind(lambda _: self.signal.fired()) if relaid or event.value == "PRESS" else Ok(None))
                            .map(lambda _: "RUNNING_MODAL")
                        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def captioned[T: msgspec.Struct](state: Browsed[T], target: Target, numbered: Mapping[str, int]) -> tuple[str, Option[gpu.types.GPUTexture]]:
    """Labels a cell, prefixing a preset's read or render failure, and answers its rendered tile."""
    match target:
        case str(stem):
            demand = Option.of_optional(state.demands.get((stem, Rank.SHOWN)))
            outcome = demand.bind(lambda held: Option.of_optional(state.outcomes.get(held)))
            faults = outcome.bind(lambda result: result.swap().to_option()).or_else(demand.bind(lambda held: held.value.swap().to_option()))
            prefix = faults.map(lambda errors: "Missing: " if any(isinstance(error, BlenderAbsentFault) for error in errors) else "Failed: ").default_value("")
            number = f"{numbered[stem]}. " if stem in numbered else ""
            return (
                prefix + number + ("✓ " if state.catalog.current == Some(stem) else "") + bpy.path.display_name(stem),
                outcome.bind(lambda result: result.to_option()).bind(lambda held: Some(held) if isinstance(held, gpu.types.GPUTexture) else Nothing),
            )
        case Verb() as verb:
            return verb.title, Nothing
        case BrowserMode() as mode:
            return mode.label, Nothing
        case Gated(enabled=enabled):
            return "Fixed" if enabled else "Fit", Nothing
        case BrowserControl.SIZE:
            return f"Size {state.view.layout.size.value:g}", Nothing
        case BrowserControl.SEARCH:
            return "Search: " + state.view.search, Nothing


def hit(cells: Cells, point: tuple[int, int]) -> Option[Target]:
    """Answers the cell target under a region point."""
    x, y = point
    return Option.of_optional(next((target for target, rect in cells if rect.x <= x < rect.x + rect.width and rect.y <= y < rect.y + rect.height), None))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Browser", "BrowserLayout", "BrowserMode"]
