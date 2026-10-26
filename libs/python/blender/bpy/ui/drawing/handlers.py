"""Draw callbacks with cached reads, owned refreshes, and scoped draw handles."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, nullcontext
from itertools import chain
from typing import Final, Literal, Protocol, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Result, Some
import gpu
import msgspec

from blender.bpy.data.rna.conditions import AllOf, Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.events.handlers import Change, SceneHandler, UpdateHandler
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderFaults, BlenderRegisterFault, BlenderResult
from blender.bpy.ui.drawing.canvas import Canvas, PROGRAMS
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.screens.areas import AreaQuery
from blender.imaging.faults import accumulated

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Part[S](Protocol):
    """Scale-dependent read and draw of a part."""

    def read(self, metrics: Metrics, /) -> BlenderResult[Option[S]]:
        """Returns draw state, Nothing while none exists."""
        ...

    def drawn(self, canvas: Canvas, state: S, /) -> None:
        """Draws state on canvas."""
        ...


type DrawType = Literal["PRE_VIEW", "POST_VIEW", "POST_PIXEL", "BACKDROP"]
type Drew = Callable[[Option[tuple[int, int]]], None]
type Refresh = Watch | Updated | Signal | list[Callable[[bpy.types.Scene], None]]

# --- [MODELS] ---------------------------------------------------------------------------


class Watch(msgspec.Struct, frozen=True, kw_only=True):
    """Property invalidating cached read on each change."""

    owner: "type[bpy.types.bpy_struct[object]]"
    identifier: str


class Updated(msgspec.Struct, frozen=True, kw_only=True):
    """Depsgraph updates filtered by ID type and change."""

    handlers: list[Callable[[bpy.types.Scene, bpy.types.Depsgraph], None]]
    ids: tuple[type[bpy.types.ID], ...]
    changes: Option[Change] = Nothing


class DrawState[S](msgspec.Struct, frozen=True, kw_only=True):
    """Completed read at its interface metrics and file load count."""

    metrics: Metrics
    loads: int
    state: Option[S]


class Drawing[S](msgspec.Struct, frozen=True, kw_only=True):
    """Part drawn in matching areas."""

    area: AreaQuery
    part: Part[S]
    refreshes: tuple[Refresh, ...] = ()

    def acquired(self, identity: Identity) -> BlenderResult[tuple[Acquisitions, Drew]]:
        """Builds refresh acquisitions and the draw callback rereading after each refresh, scale change, and file load."""
        held: Option[DrawState[S]] = Nothing
        read = identity.reports.reported(self.part.read)

        def refreshed() -> BlenderResult[None]:
            """Invalidates held read and redraws matching areas."""
            nonlocal held
            held = Nothing
            for located in (located for manager in Option.of_optional(bpy.context.window_manager).to_list() for located in self.area.located(manager.windows)):
                located.area.tag_redraw()
            return Ok(None)

        def drew(pointer: Option[tuple[int, int]]) -> None:
            """Rereads stale state, then draws."""
            nonlocal held
            match bpy.context.area, bpy.context.region:
                case bpy.types.Area() as area, bpy.types.Region() as region if self.area.matches(area):
                    canvas, loads = Canvas.of(area, region, pointer), LOADS.of(identity)
                    current = held.filter(lambda state: (state.metrics, state.loads) == (canvas.metrics, loads)).default_with(
                        lambda: DrawState(metrics=canvas.metrics, loads=loads, state=read(canvas.metrics).bind(lambda state: state))
                    )
                    held = Some(current)
                    for state in current.state.to_list():
                        with canvas.drawn():
                            self.part.drawn(canvas, state)
                case _:
                    pass

        return refreshing(identity, self.refreshes, refreshed).map(lambda rows: ((LOADS, PROGRAMS, *rows), drew))


class Overlay[S](msgspec.Struct, frozen=True, kw_only=True):
    """Drawing in one space region and draw phase."""

    drawing: Drawing[S]
    space: type[bpy.types.Space]
    respect_overlays: bool = True
    region: "rna_enums.RegionTypeItems" = "WINDOW"
    draw_type: DrawType = "POST_PIXEL"

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Owns region draw handle, hiding the drawing while the editor's overlays are off."""
        switch = (
            Descriptor
            .resolve(self.space, "overlay.show_overlays", bpy.types.BoolProperty)
            .to_option()
            .or_else_with(lambda: Descriptor.resolve(self.space, "show_overlays", bpy.types.BoolProperty).to_option())
            if self.respect_overlays
            else Nothing
        )
        shown = AllOf(conditions=(*self.drawing.area.shows.to_list(), *switch.map(lambda descriptor: Setting(descriptor=descriptor)).to_list()))

        def clipped(drew: Drew) -> Callable[[], None]:
            """Draws under the scissor test, disabling it before returning."""

            def drawn() -> None:
                gpu.state.scissor_test_set(True)
                try:
                    drew(Nothing)
                finally:
                    gpu.state.scissor_test_set(False)

            return drawn

        return (
            msgspec.structs
            .replace(self.drawing, area=msgspec.structs.replace(self.drawing.area, shows=Some(shown)))
            .acquired(identity)
            .map(lambda pair: (*pair[0], (Band.DRAW_HANDLERS, hooked(self.space, clipped(pair[1]), self.region, self.draw_type))))
        )


# --- [SERVICES] -------------------------------------------------------------------------


class Signal(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Main-thread refresh source fired by its state owner."""

    refreshes: list[Callable[[], BlenderResult[None]]] = msgspec.field(default_factory=list)

    @contextmanager
    def followed(self, refresh: Callable[[], BlenderResult[None]]) -> Generator[None]:
        """Holds drawing refresh until release."""
        self.refreshes.append(refresh)
        try:
            yield None
        finally:
            self.refreshes.remove(refresh)

    def fired(self) -> BlenderResult[None]:
        """Refreshes every following drawing, accumulating failures."""
        return accumulated(refresh() for refresh in self.refreshes).map(lambda _: None)


LOADS: Final = Scoped[int](made=lambda _identity: nullcontext(0), renewed=Some(lambda count: count + 1))

# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def hooked(space: type[bpy.types.Space], callback: Callable[[], None], region: "rna_enums.RegionTypeItems", draw_type: DrawType) -> Generator[BlenderResult[None]]:
    """Holds a draw handle of a space region until scope exit.

    Args:
        space: Space class whose regions draw the callback.
        callback: Draw body taking no arguments.
        region: Region type of the space.
        draw_type: Draw phase.

    Yields:
        Ok(None) while the handle draws, register fault for a region or phase the space refuses.
    """
    try:
        handle = space.draw_handler_add(callback, (), region, draw_type)
    except TypeError as error:
        yield Result[None, BlenderFaults].Error((BlenderRegisterFault(request=(space.__name__, region, draw_type), error=error),))
    else:
        try:
            yield Result[None, BlenderFaults].Ok(None)
        finally:
            space.draw_handler_remove(handle, region)


def refreshing(identity: Identity, refreshes: tuple[Refresh, ...], refreshed: Callable[[], BlenderResult[None]]) -> BlenderResult[Acquisitions]:
    """Builds the refresh sources of a drawing, refreshing on the main thread and deferring refreshes handlers request.

    Args:
        identity: Extension owning the sources.
        refreshes: Sources invalidating the drawing.
        refreshed: Invalidation run once per source event.

    Returns:
        Ok of the deferred refresh scope and one row per source, faults of unresolved watched properties.
    """
    later: Callable[[], None]

    @contextmanager
    def deferred() -> Generator[None]:
        """Owns refreshes handlers defer to the main thread."""
        nonlocal later
        with identity.reports.deferred(lambda: refreshed().map(lambda _: Nothing)) as later:
            yield None

    def deferring(*_: object) -> BlenderResult[None]:
        """Schedules main-thread refresh from a handler."""
        later()
        return Ok(None)

    def followed(refresh: Refresh) -> BlenderResult[Acquisitions]:
        """Builds the rows of one source."""
        match refresh:
            case Watch(owner=owner, identifier=identifier):
                return Descriptor.resolve(owner, identifier, bpy.types.Property).map(lambda found: (Subscription(owner=owner, setting=found.prop, body=refreshed),))
            case Updated(handlers=handlers, ids=ids, changes=changes):
                return Ok((UpdateHandler(handlers=handlers, ids=ids, body=deferring, changes=changes),))
            case Signal() as signal:
                return Ok(((Band.DRAW_HANDLERS, signal.followed(refreshed)),))
            case list():
                return Ok((SceneHandler(handlers=refresh, body=deferring),))

    return accumulated(map(followed, refreshes)).map(lambda rows: ((Band.DRAW_HANDLERS, deferred()), *chain.from_iterable(rows)))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["LOADS", "DrawState", "DrawType", "Drawing", "Drew", "Overlay", "Part", "Refresh", "Signal", "Updated", "Watch", "hooked", "refreshing"]
