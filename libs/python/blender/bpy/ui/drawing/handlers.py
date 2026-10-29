"""Draw callbacks with cached reads, owned refresh sources, and scoped draw and paint cursor handles."""

from collections.abc import Callable, Generator
from contextlib import AbstractContextManager, contextmanager, ExitStack, nullcontext
from functools import cache, partial
from typing import ClassVar, Final, Protocol, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.rna.conditions import AllOf, Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import definition, Struct
from blender.bpy.extension.events import Change, Handler, Subscription, updated
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.extension.registration import Acquisitions, Band, scoped
from blender.bpy.ui.drawing.canvas import Canvas, DrawType, PROGRAMS
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.screens.areas import AreaQuery
from blender.records.faults import accumulated, Fault, Outcome, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Part[S](Protocol):
    """Scale-dependent read and draw of a part, in the draw phase an overlay places it."""

    draw_type: ClassVar[DrawType]

    def read(self, metrics: Metrics, /) -> Outcome[Option[S]]:
        """Returns draw state, Nothing while none exists."""
        ...

    def drawn(self, canvas: Canvas, state: S, /) -> None:
        """Draws state on canvas."""
        ...


class _Drawn(Protocol):
    """State drawing itself on a canvas."""

    def drawn(self, canvas: Canvas, /) -> None:
        """Draws on canvas."""
        ...


type Drew = Callable[[DrawType, Option[tuple[int, int]]], None]
type Refresh = tuple[type[Struct], str] | Handled | Signal

# --- [MODELS] ---------------------------------------------------------------------------


class Handled(msgspec.Struct, frozen=True, kw_only=True):
    """App handler list invalidating the cached read on each call, on depsgraph updates of `ids` types and `changes` kinds alone while `ids` names a type."""

    handlers: list[Callable[..., None]]
    ids: tuple[type[bpy.types.ID], ...] = ()
    changes: Option[Change] = Nothing


class Reading[S: _Drawn](msgspec.Struct, frozen=True, kw_only=True):
    """Part of a read whose state draws itself, in region pixels."""

    draw_type: ClassVar[DrawType] = "POST_PIXEL"
    read: Callable[[Metrics], Outcome[Option[S]]]

    @staticmethod
    def drawn(canvas: Canvas, state: S, /) -> None:
        """Draws state on canvas through its own draw."""
        state.drawn(canvas)


class Drawing[S](msgspec.Struct, frozen=True, kw_only=True):
    """Part drawn in matching areas."""

    area: AreaQuery
    part: Part[S]
    refreshes: tuple[Refresh, ...] = ()

    def acquired(self, identity: Identity) -> tuple[Acquisitions, Drew]:
        """Builds refresh acquisitions and the draw callback taking its draw phase and pointer, rereading after each refresh, scale change, and file load."""
        reported = identity.reports.reported(self.part.read)

        @cache
        def current(metrics: Metrics, _loads: int) -> Option[S]:
            """Reads state once per interface metrics and file load count."""
            return reported(metrics).bind(lambda state: state)

        def refreshed() -> Outcome[None]:
            """Drops held reads and redraws matching areas."""
            manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            current.cache_clear()
            for located in self.area.located(manager.windows):
                located.area.tag_redraw()
            return Ok(None)

        def drew(phase: DrawType, pointer: Option[tuple[int, int]]) -> None:
            """Draws the read current for the canvas metrics and file load count."""
            match bpy.context.area, bpy.context.region:
                case bpy.types.Area() as area, bpy.types.Region() as region if self.area.matches(area):
                    canvas = Canvas.of(area, region, phase, pointer)
                    for state in current(canvas.metrics, identity.held[_LOADS]).to_list():
                        with canvas.drawn():
                            self.part.drawn(canvas, state)
                case _:
                    pass

        return (_LOADS, PROGRAMS, (Band.DRAW_HANDLERS, refreshing(identity, self.refreshes, refreshed))), drew


class Overlay[S](msgspec.Struct, frozen=True, kw_only=True):
    """Drawing in one region of the space its area query names, in the draw phase its part states."""

    drawing: Drawing[S]
    respect_overlays: bool = True
    region: "rna_enums.RegionTypeItems" = "WINDOW"

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Owns region draw handle, hiding the drawing while the editor's overlays are off."""
        space, draw_type = self.drawing.area.space, self.drawing.part.draw_type
        switch = (
            Descriptor
            .resolve(space, ("overlay", "show_overlays"), bpy.types.BoolProperty)
            .to_option()
            .or_else_with(lambda: Descriptor.resolve(space, ("show_overlays",), bpy.types.BoolProperty).to_option())
            if self.respect_overlays
            else Nothing
        )
        shown = AllOf(conditions=(*self.drawing.area.shows.to_list(), *switch.map(lambda descriptor: Setting(descriptor=descriptor)).to_list()))
        held, drew = msgspec.structs.replace(self.drawing, area=msgspec.structs.replace(self.drawing.area, shows=Some(shown))).acquired(identity)
        return Ok((*held, (Band.DRAW_HANDLERS, hooked(space, partial(drew, draw_type, Nothing), self.region, draw_type))))


class Cursor[S](msgspec.Struct, frozen=True, kw_only=True):
    """Drawing at the pointer in window pixels over regions of one type in one space type, held as a window manager paint cursor outside any tool."""

    drawing: Drawing[S]
    space: "rna_enums.SpaceTypeItems"
    region: "rna_enums.RegionTypeItems" = "WINDOW"

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Owns the paint cursor drawing at each pointer position the window manager passes."""
        held, drew = self.drawing.acquired(identity)
        cursor = scoped(
            f"WindowManager.draw_cursor_add, {self.space}, {self.region}",
            partial(bpy.types.WindowManager.draw_cursor_add, lambda xy: drew("POST_PIXEL", Some(xy)), (), self.space, self.region),
            bpy.types.WindowManager.draw_cursor_remove,
            exception=TypeError,
        )
        return Ok((*held, (Band.DRAW_HANDLERS, cursor)))


# --- [SERVICES] -------------------------------------------------------------------------


class Signal:
    """Main-thread refresh source fired by its state owner."""

    __slots__ = ("refreshes",)

    def __init__(self) -> None:
        """Follows no drawing."""
        self.refreshes: list[Callable[[], Outcome[None]]] = []

    @contextmanager
    def followed(self, refresh: Callable[[], Outcome[None]]) -> Generator[None]:
        """Holds drawing refresh until release."""
        with ExitStack() as stack:
            self.refreshes.append(refresh)
            stack.callback(self.refreshes.remove, refresh)
            yield

    def fired(self) -> Outcome[None]:
        """Refreshes every following drawing, accumulating failures."""
        return accumulated(refresh() for refresh in self.refreshes).map(lambda _: None)


# --- [OPERATIONS] -----------------------------------------------------------------------


def hooked(space: type[bpy.types.Space], callback: Callable[[], None], region: "rna_enums.RegionTypeItems", draw_type: DrawType) -> AbstractContextManager[Outcome[None]]:
    """Holds a draw handle of a space region until scope exit.

    Args:
        space: Space class whose regions draw the callback.
        callback: Draw body taking no arguments.
        region: Region type of the space.
        draw_type: Draw phase.

    Returns:
        Scope yielding Ok(None) while the handle draws, register fault for a region or phase the space refuses.
    """
    return scoped(
        f"{space.__name__}, {region}, {draw_type}", partial(space.draw_handler_add, callback, (), region, draw_type), lambda handle: space.draw_handler_remove(handle, region), exception=TypeError
    )


@contextmanager
def refreshing(identity: Identity, refreshes: tuple[Refresh, ...], refreshed: Callable[[], Outcome[None]]) -> Generator[Outcome[None]]:
    """Holds the refresh sources of a drawing, refreshing on the main thread and deferring the refreshes app handlers request.

    Args:
        identity: Extension owning the sources.
        refreshes: Sources invalidating the drawing.
        refreshed: Invalidation run once per source event.

    Yields:
        Ok(None) while every source holds, absent fault per msgbus key naming a member its type lacks once registered.
    """
    with identity.reports.deferred(lambda: refreshed().map(lambda _: Nothing)) as later, ExitStack() as stack:

        def deferring(*_: object) -> Outcome[None]:
            """Schedules main-thread refresh from a handler."""
            later()
            return Ok(None)

        def held(refresh: Refresh) -> Outcome[AbstractContextManager[None]]:
            """Builds the scope of one source."""
            match refresh:
                case Handled(handlers=handlers, ids=()):
                    return Ok(Handler(handlers=handlers, body=deferring).held(identity))
                case Handled(handlers=handlers, ids=ids, changes=changes):
                    return Ok(Handler(handlers=handlers, body=partial(updated, ids, deferring, changes=changes)).held(identity))
                case Signal() as signal:
                    return Ok(signal.followed(refreshed))
                case (owner, member):
                    return (
                        Option
                        .of_optional(definition(owner).properties.get(member))
                        .map(lambda setting: Subscription(owner=owner, setting=setting, body=refreshed).held(identity))
                        .to_result_with(partial(Fault.of, Reason.ABSENT, owner.__name__, member=member))
                    )

        scopes = accumulated(map(held, refreshes))
        for scope in (scope for entered in scopes.to_option().to_list() for scope in entered):
            stack.enter_context(scope)
        yield scopes.map(lambda _: None)


# --- [COMPOSITION] ----------------------------------------------------------------------

_LOADS: Final = Resource[int](made=lambda _identity: nullcontext(0), renewed=Some(lambda count: count + 1))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Cursor", "Drawing", "Drew", "Handled", "Overlay", "Part", "Reading", "Refresh", "Signal", "hooked", "refreshing"]
