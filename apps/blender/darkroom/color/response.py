"""Darkroom tone response: the render-enabled tone and formation entries sampled through the compositor at middle gray across the compositing log window, charted over the Viewer image under a header switch."""

from collections.abc import Generator
from contextlib import contextmanager, ExitStack, nullcontext
from functools import partial
import math
from typing import ClassVar, Final

from bl_ui.space_image import IMAGE_HT_header
import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import fingerprinted
from blender.bpy.data.rna.paths import root_in
from blender.bpy.extension.events import Handler, updated
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.previews.queue import Job, Latest, PreviewFeed, Rank
from blender.bpy.render.previews.scenes import PreviewScene
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.drawing.charts import Chart, GRID, Label, Layer, ruled, TRACE
from blender.bpy.ui.drawing.handlers import Reading, Signal
from blender.bpy.ui.images.views import image_type, overlaid
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import Layout, Property
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.surfaces.menus import Append
from blender.imaging.color.processors import DisplayView, GREY
from blender.imaging.files.buffers import applied
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.stores import content_hash, ContentHash

from ..catalog import tones
from ..effects.formation.display import formation
from .luts import formed, selected

# --- [MODELS] ---------------------------------------------------------------------------


class ResponseChart(bpy.types.PropertyGroup):
    """Screen switch charting the tone response over the Viewer image."""

    shown: bpy.props.BoolProperty(name="Tone Response", description="Chart the tone and formation response over the Viewer image")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class _Request(msgspec.Struct, frozen=True, kw_only=True):
    """Tone response sampling of entries at a scene frame across a stop window in plot columns, with the guide codes of its levels and the content it covers, `Nothing` where an entry's content holds no buffer."""

    scene: bpy.types.Scene
    entries: tuple[bpy.types.SceneCompositorEffect, ...]
    frame: tuple[int, float]
    columns: int
    window: tuple[float, float]
    codes: tuple[float, ...]
    content: Option[ContentHash]

    @property
    def centres(self) -> npt.NDArray[np.float32]:
        """Plot fractions at the centre of each column."""
        return np.linspace(0.5 / self.columns, 1.0 - 0.5 / self.columns, self.columns, dtype=np.float32)


# --- [SERVICES] -------------------------------------------------------------------------

_LATEST: Final = Resource[Latest[_Request, npt.NDArray[np.float32]]](made=lambda _identity: nullcontext(Latest()))


class ToneResponse(msgspec.Struct, frozen=True, kw_only=True):
    """Response of the render-enabled entries of `kinds` at middle gray scaled across the compositing log window, closed through the scene view without a formation entry, and guided through the encoding view of the formed display."""

    kinds: frozenset[Key]
    levels: ClassVar[tuple[float, ...]] = (*(GREY * 2.0**stop for stop in range(-4, 3)), 1.0)
    key: ClassVar[Key] = "tone_response"

    # --- [READOUT]

    @staticmethod
    def columns(metrics: Metrics) -> int:
        """Plot columns, ten widget units at the interface scale."""
        return metrics.unit * 10

    def chart(self, identity: Identity, metrics: Metrics) -> Outcome[Option[Chart]]:
        """Charts the response of the wanted request at the plot columns the interface scale gives, while the context screen shows it."""
        latest, columns = identity.held[_LATEST], self.columns(metrics)

        def plotted(request: _Request, values: npt.NDArray[np.float32]) -> Chart:
            return Chart(
                width=float(columns),
                height=columns * 0.5,
                layers=(
                    Layer.of("LINES", ruled((), request.codes), GRID),
                    Layer.of("LINE_STRIP", np.column_stack((request.centres, values.mean(axis=1))).astype(np.float32), TRACE, blend="ADDITIVE", width=1.5),
                ),
                labels=tuple(Label(text=f"{level:.2%}", x=0.0, y=code) for level, code in zip(self.levels, request.codes)),
                caption=(bpy.path.display_name(self.key),),
            )

        shown = root_in(bpy.types.Screen, bpy.context).map(lambda screen: CHARTED.read(identity, screen).map(lambda held: held.shown)).default_value(Ok(False))
        return shown.map(lambda showing: latest.wanted.filter(lambda request: showing and request.columns == columns).map2(plotted, latest.value))

    # --- [DEMAND]

    def fed(self, signal: Signal, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
        """Returns the sampling job of the current request while a window's screen shows the chart, an equal request keeping its response."""
        latest, config = identity.held[_LATEST], PyOpenColorIO.GetCurrentConfig()
        manager: bpy.types.WindowManager = context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

        def requested(scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], window: tuple[float, float], guide: PyOpenColorIO.Processor) -> Outcome[tuple[Job, ...]]:
            codes = tuple(map(float, applied(guide.getDefaultCPUProcessor(), np.repeat(np.asarray(self.levels, dtype=np.float32)[:, None], 3, axis=1)).mean(axis=1)))
            frame, columns = (scene.frame_current, scene.frame_subframe), self.columns(Metrics.read())

            def keyed(keys: tuple[Option[ContentHash], ...]) -> Option[ContentHash]:
                arrays = (np.asarray((scene.session_uid, *frame, columns, *window, *codes), dtype=np.float64),)
                return (
                    Some(content_hash(ToneResponse, *(key for held in keys for key in held.to_list()), config.getCacheID().encode(), arrays=arrays))
                    if all(held.is_some() for held in keys)
                    else Nothing
                )

            def queued(content: Option[ContentHash]) -> Outcome[tuple[Job, ...]]:
                request = _Request(scene=scene, entries=entries, frame=frame, columns=columns, window=window, codes=codes, content=content)
                changed = latest.wanting(request)
                return (signal.fired() if changed else Ok(None)).map(
                    lambda _: (
                        ()
                        if latest.value.is_some()
                        else (Job(place=(self.key, identity.package), key=content.default_value(request), rank=Rank.SHOWN, run=partial(self.rendered, signal, identity, request)),)
                    )
                )

            return accumulated(map(fingerprinted, entries)).map(keyed).bind(queued)

        def prepared(scene: bpy.types.Scene) -> Outcome[tuple[Job, ...]]:
            entries, left = selected(identity, scene, Some(self.kinds))
            return paired(windowed(config), guided(identity, scene, config)).bind(lambda parts: requested(scene, entries, *parts)) if entries and not left else self.cleared(signal, identity)

        shown = accumulated(CHARTED.read(identity, window.screen).map(lambda held: held.shown) for window in manager.windows).map(any)
        return shown.bind(lambda showing: Option.of_optional(context.scene).filter(lambda _: showing).map(prepared).default_with(lambda: self.cleared(signal, identity)))

    @staticmethod
    def rendered(signal: Signal, identity: Identity, request: _Request) -> Outcome[None]:
        """Samples the request's levels through its entries and publishes the response while the request stays wanted."""
        low, high = request.window
        levels = (GREY * np.exp2(low + (high - low) * request.centres)).astype(np.float32)
        return formed(identity, request.scene, request.entries, np.repeat(levels[None, :, None], 3, axis=2)).bind(
            lambda values: signal.fired() if identity.held[_LATEST].published(request, values[0, :, :3]) else Ok(None)
        )

    # --- [LIFETIME]

    @staticmethod
    def cleared(signal: Signal, identity: Identity) -> Outcome[tuple[Job, ...]]:
        """Clears the wanted request and its response."""
        return (signal.fired() if identity.held[_LATEST].cleared() else Ok(None)).map(lambda _: ())

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Holds the request cell, the screen switch, the queue feed, the chart over the Viewer image refreshed by its signal and switch, and the invalidations of requests covering no content."""
        signal = Signal()
        with ExitStack() as scopes:

            def invalidated() -> Outcome[Option[str]]:
                return self.cleared(signal, identity).map(lambda _: Nothing)

            later = scopes.enter_context(identity.reports.deferred(invalidated))

            def edited(scene: bpy.types.Scene, _updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
                return Ok(later() if identity.held[_LATEST].wanted.filter(lambda request: request.scene == scene and request.content.is_none()).is_some() else None)

            yield Ok((
                _LATEST,
                CHARTED,
                PreviewScene(),
                PreviewFeed(feed=partial(self.fed, signal)),
                overlaid(Reading(read=partial(self.chart, identity)), image_type(("COMPOSITING",)), (signal, (ResponseChart, "shown"))),
                Handler(handlers=bpy.app.handlers.load_post, body=lambda: self.cleared(signal, identity).map(lambda _: None)),
                Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.Scene, bpy.types.NodeTree, bpy.types.Image), edited)),
            ))


# --- [OPERATIONS] -----------------------------------------------------------------------


def windowed(config: PyOpenColorIO.Config) -> Outcome[tuple[float, float]]:
    """Returns the stops around middle gray the compositing log role's allocation spans."""

    def stops(allocation: PyOpenColorIO.AllocationTransform) -> tuple[float, float]:
        low, high, *_ = allocation.getVars()
        return low - math.log2(GREY), high - math.log2(GREY)

    def window(space: PyOpenColorIO.ColorSpace) -> Option[tuple[float, float]]:
        transform = space.getTransform(PyOpenColorIO.COLORSPACE_DIR_FROM_REFERENCE)
        members = tuple(transform) if isinstance(transform, PyOpenColorIO.GroupTransform) else (transform,)
        return Block.of_seq(members).choose(lambda member: Option.of_optional(try_downcast(PyOpenColorIO.AllocationTransform, member))).try_head().map(stops)

    return (
        Option
        .of_optional(config.getColorSpace(PyOpenColorIO.ROLE_COMPOSITING_LOG))
        .bind(window)
        .to_result_with(partial(Fault.of, Reason.ABSENT, PyOpenColorIO.ROLE_COMPOSITING_LOG, member=PyOpenColorIO.AllocationTransform.__name__))
    )


def guided(identity: Identity, scene: bpy.types.Scene, config: PyOpenColorIO.Config) -> Outcome[PyOpenColorIO.Processor]:
    """Returns the encoding view processor of the display the formation forms, the scene's without one."""

    def encoded(held: Option[DisplayView]) -> Outcome[PyOpenColorIO.Processor]:
        display = held.map(lambda view: view.display).default_with(lambda: viewed(scene).display)
        found: Outcome[Option[tuple[DisplayView, PyOpenColorIO.Processor]]] = DisplayView.for_encoding(config, display)
        return found.bind(lambda pair: pair.map(lambda held: held[1]).to_result_with(partial(Fault.of, Reason.ABSENT, display, member=DisplayView.for_encoding.__name__)))

    return formation.current(identity, scene).map(lambda found: found.map(lambda record: record.display_view)).bind(encoded)


# --- [COMPOSITION] ----------------------------------------------------------------------

CHARTED: Final = Attachment(key=ToneResponse.key, form=ResponseChart, owner=bpy.types.Screen)
response: Final = ToneResponse(kinds=frozenset(row.kind for row in (*tones.members, formation)))
rows: Final = (
    response,
    Append(
        target=IMAGE_HT_header,
        rows=(Layout(rows=(Property(target=Member(root=CHARTED, path=("shown",)), keywords={"icon_only": True}, icon=Some("GRAPH")),), shown=Some(image_type(("COMPOSITING",)))),),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CHARTED", "ResponseChart", "ToneResponse", "guided", "response", "rows", "windowed"]
