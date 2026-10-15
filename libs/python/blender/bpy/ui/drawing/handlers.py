"""Native drawing callbacks with cached reads and owned refresh lifetimes."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from enum import auto, Enum
from itertools import chain
from typing import Literal, Protocol, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Result, Some
import gpu
import msgspec

from blender.bpy.data.rna.conditions import AllOf, Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.events.handlers import Change, PathHandler, SceneHandler, UpdateHandler
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.faults import BlenderFaults, BlenderRegisterFault, BlenderResult
from blender.bpy.ui.drawing.canvas import Canvas, Programs
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.screens.areas import AreaQuery
from blender.imaging.faults import accumulated

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Part[S](Protocol):
    """Scale-dependent read and native draw of a part."""

    def read(self, metrics: Metrics, /) -> BlenderResult[Option[S]]:
        """Returns the current draw state, or Nothing while there is none."""
        ...

    def drawn(self, canvas: Canvas, state: S, /) -> None:
        """Draws the state on the supplied canvas."""
        ...


class _Phase(Enum):
    """Unread and released phases outside a drawing's cached state."""

    UNREAD = auto()
    RELEASED = auto()


type Drew = Callable[[Option[tuple[int, int]]], None]
type Refresh = Watch | Updated | Signal | list[Callable[[bpy.types.Scene], None]]

# --- [MODELS] ---------------------------------------------------------------------------


class Watch(msgspec.Struct, frozen=True, kw_only=True):
    """Native property whose notifications invalidate a drawing's read."""

    owner: "type[bpy.types.bpy_struct[object]]"
    identifier: str


class Updated(msgspec.Struct, frozen=True, kw_only=True):
    """Dependency-graph updates filtered by ID types and change kinds."""

    handlers: list[Callable[[bpy.types.Scene, bpy.types.Depsgraph], None]]
    ids: tuple[type[bpy.types.ID], ...]
    changes: Option[Change] = Nothing


class DrawState[S](msgspec.Struct, frozen=True, kw_only=True):
    """Completed read and the interface metrics it was read at."""

    metrics: Metrics
    state: Option[S]


class Drawing[S](msgspec.Struct, frozen=True, kw_only=True):
    """Drawn part, its admitted areas, and its refresh sources."""

    area: AreaQuery
    part: Part[S]
    refreshes: tuple[Refresh, ...] = ()
    where: Option[Callable[[bpy.types.Context], bool]] = Nothing

    def acquired(self, identity: Identity) -> BlenderResult[tuple[tuple[tuple[Band, Acquisition], ...], Drew]]:
        """Builds owned refresh acquisitions and one region or pointer draw callback."""
        held: DrawState[S] | _Phase = _Phase.UNREAD
        read = identity.reports.reported(self.part.read)

        def refreshed() -> BlenderResult[None]:
            """Invalidates the held read and tags every admitted live area."""
            nonlocal held
            held = _Phase.UNREAD
            for manager in Option.of_optional(bpy.context.window_manager).to_list():
                for located in self.area.located(manager.windows):
                    located.area.tag_redraw()
            return Ok(None)

        def deferred_refresh() -> BlenderResult[Option[str]]:
            """Refreshes after native write restrictions end without an undo step."""
            return refreshed().map(lambda _: Nothing)

        later: Callable[[], None]

        def deferring(*_: object) -> BlenderResult[None]:
            """Schedules one refresh for handlers that can run outside the main thread."""
            later()
            return Ok(None)

        def followed(refresh: Refresh) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
            """Builds the acquisition owned by one refresh source."""
            match refresh:
                case Watch(owner=owner, identifier=identifier):
                    return Descriptor.resolve(owner, identifier, bpy.types.Property).bind(lambda found: Subscription(owner=owner, setting=found.prop, body=refreshed).acquisitions(identity))
                case Updated(handlers=handlers, ids=ids, changes=changes):
                    return UpdateHandler(handlers=handlers, ids=ids, body=deferring, changes=changes).acquisitions(identity)
                case Signal() as signal:
                    return Ok(((Band.DRAW_HANDLERS, signal.followed(refreshed)),))
                case list():
                    return SceneHandler(handlers=refresh, body=deferring).acquisitions(identity)

        def drew(pointer: Option[tuple[int, int]]) -> None:
            """Reads on invalidation or scale changes and draws in admitted native regions."""
            nonlocal held
            if held is _Phase.RELEASED:
                return
            area, region = bpy.context.area, bpy.context.region
            if area is None or region is None or not self.area.admits(area) or not all(admit(bpy.context) for admit in self.where.to_list()):
                return
            canvas = Canvas.of(area, region, pointer)
            if isinstance(held, _Phase) or held.metrics != canvas.metrics:
                held = DrawState(metrics=canvas.metrics, state=read(canvas.metrics).bind(lambda state: state))
            for state in held.state.to_list():
                with canvas.drawn():
                    self.part.drawn(canvas, state)

        @contextmanager
        def cleared() -> Generator[None]:
            """Owns refresh admission and retires callbacks Blender may still retain."""
            nonlocal held, later
            try:
                with identity.reports.deferred(deferred_refresh) as later:
                    yield None
            finally:
                held = _Phase.RELEASED

        followers = (
            PathHandler(handlers=bpy.app.handlers.load_post, body=lambda _: refreshed()).acquisitions(identity),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            *(followed(refresh) for refresh in self.refreshes),
        )
        return accumulated(followers).map(lambda rows: (((Band.RESOURCES, Programs()), (Band.DRAW_HANDLERS, cleared()), *chain.from_iterable(rows)), drew))


class Overlay[S](msgspec.Struct, frozen=True, kw_only=True):
    """Drawing placed in one native space's region and draw phase."""

    drawing: Drawing[S]
    space: type[bpy.types.Space]
    scissor_enabled: bool
    respect_overlays: bool = True
    region: "rna_enums.RegionTypeItems" = "WINDOW"
    draw_type: Literal["PRE_VIEW", "POST_VIEW", "POST_PIXEL", "BACKDROP"] = "POST_PIXEL"

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Owns the region draw handle under the caller's overlay visibility policy."""
        switch = (
            Descriptor
            .resolve(self.space, "overlay.show_overlays", bpy.types.BoolProperty)
            .to_option()
            .or_else_with(lambda: Descriptor.resolve(self.space, "show_overlays", bpy.types.BoolProperty).to_option())
            if self.respect_overlays
            else Nothing
        )
        shown = AllOf(conditions=(*self.drawing.area.shows.to_list(), *switch.map(lambda descriptor: Setting(descriptor=descriptor)).to_list()))
        area = msgspec.structs.replace(self.drawing.area, shows=Some(shown))

        @contextmanager
        def added(drew: Drew) -> Generator[BlenderResult[None] | None]:
            """Holds the native draw handle or returns its registration refusal."""

            def drawn() -> None:
                """Enables clipping and restores the native pass's supplied scissor state."""
                gpu.state.scissor_test_set(True)
                try:
                    drew(Nothing)
                finally:
                    gpu.state.scissor_test_set(self.scissor_enabled)

            try:
                handle = self.space.draw_handler_add(drawn, (), self.region, self.draw_type)
            except TypeError as error:
                yield Result[None, BlenderFaults].Error((BlenderRegisterFault(request=(self.space.__name__, self.region, self.draw_type), error=error),))
            else:
                try:
                    yield None
                finally:
                    self.space.draw_handler_remove(handle, self.region)

        return msgspec.structs.replace(self.drawing, area=area).acquired(identity).map(lambda pair: (*pair[0], (Band.DRAW_HANDLERS, added(pair[1]))))


# --- [SERVICES] -------------------------------------------------------------------------


class Signal(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Main-thread refresh source fired by its state owner."""

    refreshes: list[Callable[[], BlenderResult[None]]] = msgspec.field(default_factory=list)

    @contextmanager
    def followed(self, refresh: Callable[[], BlenderResult[None]]) -> Generator[None]:
        """Holds one drawing's refresh until its acquisition is released."""
        self.refreshes.append(refresh)
        try:
            yield None
        finally:
            self.refreshes.remove(refresh)

    def fired(self) -> BlenderResult[None]:
        """Refreshes every following drawing and accumulates their failures."""
        return accumulated(refresh() for refresh in self.refreshes).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["DrawState", "Drawing", "Drew", "Overlay", "Part", "Refresh", "Signal", "Updated", "Watch"]
