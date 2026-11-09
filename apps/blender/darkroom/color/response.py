"""Darkroom tone response: the render-enabled tone and formation entries sampled through the compositor at middle gray across the compositing log window, charted over the Viewer image under a header switch."""

from collections.abc import Generator
from contextlib import contextmanager, nullcontext
from functools import partial
import math
from typing import ClassVar, Final

from bl_ui.space_image import IMAGE_HT_header
import bpy
from expression import Option, Some, try_downcast
from expression.collections import Block
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm_bpy.data import Acquisitions, Handle, Identity, Key, Resource, root_in
from rasm_bpy.render import fingerprinted, Job, Latest, PreviewFeed, PreviewScene, Rank, rate
from rasm_bpy.settings import Attachment
from rasm_bpy.ui import Append, AreaQuery, Chart, DrawingPart, GRID, image_type, Label, Layer, Layout, Member, Metrics, overlaid, Property, ruled, Signal, TRACE
from rasm_imaging import applied, DisplayView, GRAY
from rasm_records import accumulated, ContentHash, Fault, Nothing, Ok, Outcome, paired, Reason

from ..catalog import tones
from ..effects.formation.display import encoding, formation
from .luts import closing, selected

# --- [CONSTANTS] ------------------------------------------------------------------------

_VIEWER: Final = image_type(("COMPOSITING",))

# --- [MODELS] ---------------------------------------------------------------------------


class ResponseChart(bpy.types.PropertyGroup):
    """Screen switch charting the tone response over the Viewer image."""

    shown: bpy.props.BoolProperty(name="Tone Response", description="Chart the tone and formation response over the Viewer image")  # ty: ignore[invalid-type-form]


class _Request(msgspec.Struct, frozen=True, kw_only=True):
    """Tone response sampling with scene, frame, plot dimensions, guide levels, OCIO configuration, and each entry's fingerprint."""

    scene: Handle[bpy.types.Scene]
    entries: tuple[str, ...]
    view: Option[DisplayView]
    frame: tuple[float, float]
    columns: int
    window: tuple[float, float]
    codes: tuple[float, ...]
    config: str
    content: tuple[Option[ContentHash], ...]

    @property
    def centers(self) -> npt.NDArray[np.float32]:
        """Plot fractions at the center of each column."""
        return np.linspace(0.5 / self.columns, 1.0 - 0.5 / self.columns, self.columns, dtype=np.float32)


# --- [SERVICES] -------------------------------------------------------------------------

_LATEST: Final = Resource[Latest[tuple[int, int], _Request, Chart]](made=lambda _identity: nullcontext(Latest()))
_COLUMNS: Final = Resource[dict[Handle[bpy.types.Screen], int]](made=lambda _identity: nullcontext({}), renewed=Some(lambda _columns: {}))


class ToneResponse(msgspec.Struct, frozen=True, kw_only=True):
    """Response of the render-enabled entries of `kinds` at middle gray scaled across the compositing log window, closed through the scene view without a formation entry, and guided through the encoding view of the formed display."""

    # --- [STATE]

    kinds: frozenset[Key]
    levels: ClassVar[tuple[float, ...]] = (*(GRAY * 2.0**stop for stop in range(-4, 3)), 1.0)
    key: ClassVar[Key] = "tone_response"

    # --- [READOUT]

    @staticmethod
    def chart(identity: Identity, metrics: Metrics) -> Outcome[Option[Chart]]:
        """Charts the response of the wanted request at the plot columns the interface scale gives, while the context screen shows it."""
        latest, columns = _LATEST[identity], metrics.unit * 10

        def shown(screen: bpy.types.Screen, held: ResponseChart) -> Option[Chart]:
            if held.shown:
                _COLUMNS[identity][Handle.of(screen)] = columns
                return root_in(bpy.types.Scene, bpy.context).bind(lambda scene: latest.shown((scene.session_uid, columns))).map(lambda pair: pair[1])
            return Nothing

        return root_in(bpy.types.Screen, bpy.context).map(lambda screen: CHARTED.read(identity, screen).map(partial(shown, screen))).default_value(Ok(Nothing))

    # --- [DEMAND]

    def fed(self, signal: Signal, identity: Identity, context: bpy.types.Context) -> Outcome[tuple[Job, ...]]:
        """Returns sampling per actual scene and plot size demanded by chart-enabled windows, equal requests keeping their responses."""
        latest, columns, config = _LATEST[identity], _COLUMNS[identity], PyOpenColorIO.GetCurrentConfig()
        config_id = config.getCacheID()
        manager: bpy.types.WindowManager = context.window_manager  # ty: ignore[invalid-assignment]
        visible = tuple(dict.fromkeys(at.window for at in AreaQuery(space=bpy.types.SpaceImageEditor, ui_type=Some("IMAGE_EDITOR"), shows=Some(_VIEWER)).located(manager.windows)))

        def queued(request: _Request) -> Outcome[tuple[Job, ...]]:
            slot = request.scene.uid, request.columns
            changed, token, value = latest.wanting(slot, request, same=lambda prior, wanted: prior == wanted and all(key.is_some() for key in wanted.content))
            return (signal.fired() if changed else Ok(None)).map(
                lambda _: () if value.is_some() else (Job(place=slot, key=token, rank=Rank.SHOWN, run=partial(self.rendered, signal, identity, request, token)),)
            )

        def requested(
            scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], sizes: tuple[int, ...], window: tuple[float, float], guide: PyOpenColorIO.Processor
        ) -> Outcome[tuple[Job, ...]]:
            codes = tuple(map(float, applied(guide.getDefaultCPUProcessor(), np.repeat(np.asarray(self.levels, dtype=np.float32)[:, None], 3, axis=1)).mean(axis=1)))
            source, names, view, frame = Handle.of(scene), tuple(entry.name for entry in entries), closing(identity, scene, entries), (scene.frame_current_final, rate(scene))
            return (
                fingerprinted(entries)
                .bind(
                    lambda content: accumulated(
                        queued(_Request(scene=source, entries=names, view=view, frame=frame, columns=size, window=window, codes=codes, config=config_id, content=content)) for size in sizes
                    )
                )
                .map(lambda batches: tuple(job for jobs in batches for job in jobs))
            )

        def prepared(windows: tuple[bpy.types.Window, ...]) -> Outcome[tuple[Job, ...]]:
            screens = frozenset(Handle.of(window.screen) for window in windows)
            for screen in columns.keys() - screens:
                del columns[screen]
            sources: dict[bpy.types.Scene, dict[int, None]] = {}
            for window in windows:
                for size in Option.of_optional(columns.get(Handle.of(window.screen))).to_list():
                    sources.setdefault(window.scene, {})[size] = None
            active = tuple((scene, entries, tuple(sizes)) for scene, sizes in sources.items() for entries, left in (selected(identity, scene, Some(self.kinds)),) if entries and not left)
            wanted = frozenset((Handle.of(scene), size) for scene, _, sizes in active for size in sizes)
            return (signal.fired() if latest.cleared(lambda request: (request.scene, request.columns) not in wanted) else Ok(None)).bind(
                lambda _: (
                    paired(windowed(config), accumulated(encoding(identity, scene, config).map(lambda found: found[1]) for scene, _, _ in active)).bind(
                        lambda parts: accumulated(requested(scene, entries, sizes, parts[0], guide) for (scene, entries, sizes), guide in zip(active, parts[1])).map(
                            lambda batches: tuple(job for jobs in batches for job in jobs)
                        )
                    )
                    if active
                    else Ok(())
                )
            )

        return accumulated(CHARTED.read(identity, window.screen).map(lambda held: held.shown) for window in visible).bind(
            lambda shown: prepared(tuple(window for window, on in zip(visible, shown) if on))
        )

    @staticmethod
    def rendered(signal: Signal, identity: Identity, request: _Request, token: object) -> Outcome[None]:
        """Samples the request's levels through its entries and publishes the response while the request stays wanted."""
        low, high = request.window
        levels = (GRAY * np.exp2(low + (high - low) * request.centers)).astype(np.float32)

        def plotted(values: npt.NDArray[np.float32]) -> Chart:
            return Chart(
                width=float(request.columns),
                height=request.columns * 0.5,
                layers=(
                    Layer.of("LINES", ruled((), request.codes), GRID),
                    Layer.of("LINE_STRIP", np.column_stack((request.centers, values[0, :, :3].mean(axis=1))), TRACE, blend="ADDITIVE", width=1.5),
                ),
                labels=tuple(Label(text=f"{level:.2%}", x=0.0, y=code) for level, code in zip(ToneResponse.levels, request.codes)),
                caption=(bpy.path.display_name(ToneResponse.key),),
            )

        return (
            PreviewScene()
            .sampled_from(identity, request.scene, request.entries, np.repeat(levels[None, :, None], 3, axis=2))
            .bind(lambda values: request.view.map(lambda view: view.form(PyOpenColorIO.GetCurrentConfig(), values)).default_value(Ok(values)))
            .map(plotted)
            .bind(lambda chart: signal.fired() if _LATEST[identity].published((request.scene.uid, request.columns), token, chart) else Ok(None))
        )

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Holds the request cell, the screen switch, the queue feed, the chart over the Viewer image refreshed by its signal and switch, and the invalidations of requests covering no content."""
        signal = Signal()
        with Latest.invalidating(identity, _LATEST, lambda request, _scene: any(key.is_none() for key in request.content), signal.fired) as lifecycle:
            yield Ok((
                *lifecycle,
                _COLUMNS,
                CHARTED,
                PreviewScene(),
                PreviewFeed(feed=partial(self.fed, signal)),
                overlaid(DrawingPart(read=partial(self.chart, identity), drawn=Chart.drawn), _VIEWER, (signal, (ResponseChart, "shown"))),
            ))


# --- [OPERATIONS] -----------------------------------------------------------------------


def windowed(config: PyOpenColorIO.Config) -> Outcome[tuple[float, float]]:
    """Returns the stops around middle gray the compositing log role's allocation spans."""

    def stops(allocation: PyOpenColorIO.AllocationTransform) -> tuple[float, float]:
        low, high, *_ = allocation.getVars()
        return low - math.log2(GRAY), high - math.log2(GRAY)

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


# --- [COMPOSITION] ----------------------------------------------------------------------

CHARTED: Final = Attachment(key=ToneResponse.key, form=ResponseChart, owner=bpy.types.Screen)
response: Final = ToneResponse(kinds=frozenset(row.kind for row in (*tones, formation)))
rows: Final = (
    response,
    Append(target=IMAGE_HT_header, rows=(Layout(rows=(Property(target=Member(root=CHARTED, path=("shown",)), keywords={"icon_only": True}, icon=Some("GRAPH")),), visible=Some(_VIEWER)),)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CHARTED", "ResponseChart", "ToneResponse", "response", "rows", "windowed"]
