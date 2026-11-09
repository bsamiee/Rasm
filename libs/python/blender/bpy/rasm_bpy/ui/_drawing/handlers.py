"""Region draw callbacks and the refresh sources their reads follow."""

from collections.abc import Callable, Generator
from contextlib import AbstractContextManager, contextmanager, ExitStack
from functools import partial
from typing import Protocol, TYPE_CHECKING

import bpy
from expression import Option, Some
import msgspec

from rasm_bpy.data._extension.events import _updated, Change
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import _scoped, _Tier, Acquisitions, Handler, Subscription
from rasm_bpy.data._rna.conditions import AllOf, Setting
from rasm_bpy.data._rna.descriptors import Descriptor
from rasm_bpy.data._rna.paths import _definition, Struct
from rasm_bpy.ui._drawing.canvas import _PROGRAMS, Canvas, DrawType
from rasm_bpy.ui._resources.theme import Metrics
from rasm_bpy.ui._screens.areas import AreaQuery
from rasm_records import accumulated, Nothing, Ok, Outcome

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Part[S](Protocol):
    """Scale-dependent read and draw of a part, in the draw phase an overlay places it."""

    @property
    def draw_type(self) -> DrawType:
        """Region draw phase."""
        ...

    def read(self, metrics: Metrics, /) -> Outcome[Option[S]]:
        """Returns draw state, Nothing while none exists."""
        ...

    def drawn(self, state: S, canvas: Canvas, /) -> None:
        """Draws state on canvas."""
        ...


type DrawCallback = Callable[[DrawType, Option[tuple[int, int]]], None]
type Refresh = tuple[type[Struct], str] | HandlerRefresh | Signal

# --- [MODELS] ---------------------------------------------------------------------------


class HandlerRefresh(msgspec.Struct, frozen=True, kw_only=True):  # type: ignore[explicit-any]
    """App handler list requesting redraw on each call or depsgraph changes matching `ids` and `changes`."""

    handlers: list[Callable[..., None]]  # type: ignore[explicit-any]
    ids: tuple[type[bpy.types.ID], ...] = ()
    changes: Option[Change] = Nothing


class DrawingPart[S](msgspec.Struct, frozen=True, kw_only=True):
    """Read and draw callbacks with their region draw phase."""

    read: Callable[[Metrics], Outcome[Option[S]]]
    drawn: Callable[[S, Canvas], None]
    draw_type: DrawType = "POST_PIXEL"


class Drawing[S](msgspec.Struct, frozen=True, kw_only=True):
    """Part drawn in matching areas."""

    area: AreaQuery
    part: Part[S]
    refreshes: tuple[Refresh, ...] = ()

    def acquired(self, identity: Identity) -> tuple[Acquisitions, DrawCallback]:
        """Builds refresh sources and the callback reading state in each native draw context."""
        reported = identity.reports.reported(self.part.read)

        def refreshed() -> Outcome[None]:
            """Redraws matching areas."""
            manager: bpy.types.WindowManager = bpy.context.window_manager  # ty: ignore[invalid-assignment]
            for located in self.area.located(manager.windows):
                located.area.tag_redraw()
            return Ok(None)

        def callback(phase: DrawType, pointer: Option[tuple[int, int]]) -> None:
            """Reads and draws the part in the host's current region."""
            context = bpy.context
            match context.area, context.region:
                case bpy.types.Area() as area, bpy.types.Region() as region if self.area.matches(area):
                    canvas = Canvas.of(area, region, phase, pointer)
                    for state in reported(canvas.metrics).bind(lambda state: state).to_list():
                        with canvas.drawn():
                            self.part.drawn(state, canvas)
                case _:
                    pass

        return (_PROGRAMS, (_Tier.DRAW_HANDLERS, _refreshing(identity, self.refreshes, refreshed))), callback


class Overlay[S](msgspec.Struct, frozen=True, kw_only=True):
    """Drawing in one region of the space its area query names, in the draw phase its part states."""

    drawing: Drawing[S]
    respect_overlays: bool = True
    region: "rna_enums.RegionTypeItems" = "WINDOW"

    def acquisitions(self, identity: Identity) -> Acquisitions:
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
        held, callback = msgspec.structs.replace(self.drawing, area=msgspec.structs.replace(self.drawing.area, shows=Some(shown))).acquired(identity)
        return (*held, (_Tier.DRAW_HANDLERS, draw_handler(space, partial(callback, draw_type, Nothing), self.region, draw_type)))


class Cursor[S](msgspec.Struct, frozen=True, kw_only=True):
    """Drawing at the pointer in window pixels over regions of one type in one space type, held as a window manager paint cursor outside any tool."""

    drawing: Drawing[S]
    space: "rna_enums.SpaceTypeItems"
    region: "rna_enums.RegionTypeItems" = "WINDOW"

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Owns the paint cursor drawing at each pointer position the window manager passes."""
        held, callback = self.drawing.acquired(identity)
        cursor = _scoped(
            f"WindowManager.draw_cursor_add, {self.space}, {self.region}",
            partial(bpy.types.WindowManager.draw_cursor_add, lambda xy: callback("POST_PIXEL", Some(xy)), (), self.space, self.region),
            bpy.types.WindowManager.draw_cursor_remove,
            exception=TypeError,
        )
        return (*held, (_Tier.DRAW_HANDLERS, cursor))


# --- [SERVICES] -------------------------------------------------------------------------


class Signal:
    """Main-thread refresh source fired by its state owner."""

    __slots__ = ("refreshes",)

    def __init__(self) -> None:
        """Follows no drawing."""
        self.refreshes: list[Callable[[], Outcome[None]]] = []

    @contextmanager
    def followed(self, refresh: Callable[[], Outcome[None]]) -> Generator[Outcome[None]]:
        """Holds drawing refresh until release."""
        with ExitStack() as stack:
            self.refreshes.append(refresh)
            stack.callback(self.refreshes.remove, refresh)
            yield Ok(None)

    def fired(self) -> Outcome[None]:
        """Refreshes every following drawing, accumulating failures."""
        return accumulated(refresh() for refresh in self.refreshes).map(lambda _: None)


# --- [OPERATIONS] -----------------------------------------------------------------------


def draw_handler(space: type[bpy.types.Space], callback: Callable[[], None], region: "rna_enums.RegionTypeItems", draw_type: DrawType) -> AbstractContextManager[Outcome[None]]:
    """Holds a draw handle of a space region until scope exit.

    Args:
        space: Space class whose regions draw the callback.
        callback: Draw body taking no arguments.
        region: Region type of the space.
        draw_type: Draw phase.

    Returns:
        Scope yielding Ok(None) while the handle draws, register fault for a region or phase the space refuses.
    """
    return _scoped(
        f"{space.__name__}, {region}, {draw_type}", partial(space.draw_handler_add, callback, (), region, draw_type), lambda handle: space.draw_handler_remove(handle, region), exception=TypeError
    )


@contextmanager
def _refreshing(identity: Identity, refreshes: tuple[Refresh, ...], refreshed: Callable[[], Outcome[None]]) -> Generator[Outcome[None]]:
    """Holds the refresh sources of a drawing, refreshing on the main thread and deferring the refreshes app handlers request.

    Args:
        identity: Extension owning the sources.
        refreshes: Sources requesting redraw.
        refreshed: Redraw request per source event.

    Yields:
        Ok(None) while every source holds, or acquisition faults.
    """
    with identity.reports.deferred(lambda: refreshed().map(lambda _: Nothing)) as later, ExitStack() as stack:

        def deferring(*_: object) -> Outcome[None]:
            """Schedules main-thread refresh from a handler."""
            later()
            return Ok(None)

        def held(refresh: Refresh) -> AbstractContextManager[Outcome[None]]:
            """Builds the scope of one source."""
            match refresh:
                case HandlerRefresh(handlers=handlers, ids=()):
                    return Handler(handlers=handlers, body=deferring).held(identity)
                case HandlerRefresh(handlers=handlers, ids=ids, changes=changes):
                    return Handler(handlers=handlers, body=partial(_updated, ids, deferring, changes=changes)).held(identity)
                case Signal() as signal:
                    return signal.followed(refreshed)
                case (owner, member):
                    return Subscription(owner=owner, setting=_definition(owner).properties[member], body=refreshed).held(identity)

        yield accumulated(stack.enter_context(held(refresh)) for refresh in refreshes).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Cursor", "DrawCallback", "Drawing", "DrawingPart", "HandlerRefresh", "Overlay", "Part", "Refresh", "Signal", "draw_handler"]
