"""Darkroom metering: the scene meter written into the earliest Exposure entry, a meter region picked or dragged over the image, and viewport following through the inverse of the shown formation."""

# ruff: file-ignore[relative-imports]
from collections.abc import Callable, Generator
from contextlib import contextmanager, nullcontext
from functools import partial
import math
from typing import ClassVar, Final

from bl_ui.properties_render import RENDER_PT_color_management
import bpy
from expression import Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.keymaps import Button, Chord
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Placement
from blender.bpy.data.ids.images import fingerprinted
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Job, Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import root_in, rooted
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Declaration, Handler, Resource, Timer
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.entries import Enabled
from blender.bpy.render.compositor.inputs import entered, path_key
from blender.bpy.render.compositor.motion import rate
from blender.bpy.render.compositor.viewers import READ
from blender.bpy.render.previews import queue
from blender.bpy.render.previews.queue import Latest, PreviewFeed, Rank
from blender.bpy.render.previews.scenes import PreviewScene
from blender.bpy.scene.cameras import camera_frame
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.drawing.canvas import Canvas, captured, DrawType
from blender.bpy.ui.drawing.handlers import Drawing, HandlerRefresh, Overlay, Signal
from blender.bpy.ui.elements.panels import Child, Panel
from blender.bpy.ui.elements.tools import Tool, Tools
from blender.bpy.ui.images.views import FrameRegion, Picked, picking
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Store
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.screens.areas import AreaQuery, Located
from blender.imaging.analysis.metering import Meter
from blender.imaging.color.luts import Lattice, Logarithmic
from blender.imaging.color.processors import DisplayView, processor
from blender.imaging.files.buffers import applied
from blender.imaging.files.images import ImageFile
from blender.imaging.geometry.frames import Quad, Region
from blender.records.faults import accumulated, Fault, Nothing, Ok, Outcome, paired, Reason
from blender.records.stores import ContentHash

from ..catalog import chain
from ..effects.grade.exposure import entry_stops, exposure, gain, STOPS

# --- [CONSTANTS] ------------------------------------------------------------------------

_VIEWPORTS: Final = AreaQuery(
    space=bpy.types.SpaceView3D,
    shows=Some(Setting(descriptor=Descriptor(root=bpy.types.SpaceView3D, owner=("shading",), prop=bpy.types.View3DShading.bl_rna.properties["type"]), accepted=("MATERIAL", "RENDERED"))),
)
_COMPOSITOR: Final = Descriptor(root=bpy.types.SpaceView3D, owner=("shading",), prop=bpy.types.View3DShading.bl_rna.properties["use_compositor"])
_COMPOSITED: Final = Setting(descriptor=_COMPOSITOR, accepted=("ALWAYS",)) | (
    Setting(descriptor=_COMPOSITOR, accepted=("CAMERA",))
    & Setting(descriptor=Descriptor(root=bpy.types.Context, owner=("region_data",), prop=bpy.types.RegionView3D.bl_rna.properties["view_perspective"]), accepted=("CAMERA",))
)

# --- [MODELS] ---------------------------------------------------------------------------


class Follow(bpy.types.PropertyGroup):
    """Viewport response speed, dead band, and agreement between readings."""

    speed: bpy.props.FloatProperty(name="Speed", description="Share of remaining step per update", subtype="FACTOR", min=0.02, max=1.0, default=0.5)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    band: bpy.props.FloatProperty(name="Dead Band", description="Stops of gain change ignored", min=0.0, max=2.0, soft_max=1.0, step=1, precision=2, default=0.1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    tolerance: bpy.props.FloatProperty(name="Tolerance", description="Stops between agreeing readings", min=0.0, max=2.0, soft_max=1.0, step=1, precision=2, default=0.1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Metering(bpy.types.PropertyGroup):
    """One scene's meter shared by rendered-frame and viewport exposure."""

    low: bpy.props.FloatProperty(name="Low", description="Percentile trimmed below", subtype="PERCENTAGE", min=0.0, max=100.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    high: bpy.props.FloatProperty(name="High", description="Percentile trimmed above", subtype="PERCENTAGE", min=0.0, max=100.0, default=100.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    center: bpy.props.FloatProperty(name="Center", description="Weight taken from samples outside the center", subtype="FACTOR", min=0.0, max=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_region: bpy.props.BoolProperty(name="Region", description="Meter the region alone, the whole frame while off")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    region: bpy.props.FloatVectorProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Bounds", description="Metered frame share from bottom left as left, bottom, right, and top", size=4, min=0.0, max=1.0, default=(0.0, 0.0, 1.0, 1.0)
    )
    bias: bpy.props.FloatProperty(name="EV Bias", description="Stops added to each reading", min=-3.0, max=3.0, soft_min=-2.0, soft_max=2.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_follow: bpy.props.BoolProperty(name="Follow Viewport", description="Step the chain gain toward viewport readings")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    follow: bpy.props.PointerProperty(type=Follow, name="Viewport Response")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @property
    def meter(self) -> Meter:
        """Meter of the trims, center weight, and region, the whole frame while the region is off."""
        left, bottom, right, top = self.region
        return Meter(low=self.low, high=self.high, center_weight=self.center, region=Region.bounding(np.array(((left, bottom), (right, top)))) if self.use_region else Region())


class Following(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Enabled viewport meter with its bias, agreement, and inverse pipeline with Exposure compensation, `Nothing` until the composited lattice is sampled."""

    scene: Handle[bpy.types.Scene]
    meter: Meter
    bias: float
    tolerance: float
    config: PyOpenColorIO.Config
    inverse: Option[tuple[PyOpenColorIO.CPUProcessor, float]]


class _Formation(msgspec.Struct, frozen=True, kw_only=True):
    """Viewport formation request with its frame, view, OCIO configuration, and each entry's fingerprint."""

    scene: Handle[bpy.types.Scene]
    entries: tuple[str, ...]
    frame: tuple[float, float]
    view: DisplayView
    config: str
    content: tuple[Option[ContentHash], ...]


# --- [SERVICES] -------------------------------------------------------------------------

_INVERSES: Final = Resource[Latest[int, _Formation, PyOpenColorIO.CPUProcessor]](made=lambda _identity: nullcontext(Latest()))
_READINGS: Final = Resource[dict[Handle[bpy.types.Scene], tuple[object, float]]](made=lambda _identity: nullcontext({}), renewed=Some(lambda _readings: {}))
_SIGNAL: Final = Resource[Signal](made=lambda _identity: nullcontext(Signal()))


class Follower:
    """Viewport part metering the shown camera frame through the inverse of its formation, stepping toward targets consecutive readings agree on."""

    __slots__ = ("identity", "step")
    draw_type: ClassVar[DrawType] = "PRE_VIEW"

    def __init__(self, identity: Identity, step: Callable[[Handle[bpy.types.Scene], object], None]) -> None:
        """Holds the identity and deferred exposure step."""
        self.identity, self.step = identity, step

    def read(self, _metrics: Metrics) -> Outcome[Option[Following]]:
        """Returns enabled metering with the inverse pipeline used by this viewport."""
        config, identity = PyOpenColorIO.GetCurrentConfig(), self.identity

        def followed(scene: bpy.types.Scene, metering: Metering) -> Outcome[Option[Following]]:
            if not metering.use_follow:
                _READINGS[identity].pop(Handle.of(scene), None)
                return Ok(Nothing)
            view, composited = viewed(scene), _COMPOSITED.holds(bpy.context)
            entries = AutoExposure.viewport_entries(identity, scene) if composited else ()
            inverse = (
                AutoExposure.requested(config, scene, entries, view).map(lambda request: _INVERSES[identity].value(scene.session_uid, request))
                if entries
                else view
                .processor(config)
                .bind(lambda built: processor(config, PyOpenColorIO.GroupTransform([built.createGroupTransform()], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE)))
                .map(lambda built: Some(built.getDefaultCPUProcessor()))
            )
            return paired(inverse, gain(identity, scene, enabled=Enabled.PREVIEW) if composited else Ok(0.0)).map(
                lambda found: Some(
                    Following(
                        scene=Handle.of(scene),
                        meter=metering.meter,
                        bias=metering.bias,
                        tolerance=metering.follow.tolerance,
                        config=config,
                        inverse=found[0].map(lambda built: (built, math.exp2(-found[1]))),
                    )
                )
            )

        return root_in(bpy.types.Scene, bpy.context).map(lambda scene: AutoExposure.attachment.read(identity, scene).bind(partial(followed, scene))).default_value(Ok(Nothing))

    def drawn(self, state: Following, canvas: Canvas) -> None:
        """Meters the scene ahead of the chain in the visible camera frame, through the composited formation's inverse with the Exposure gain divided out while the viewport composites and the view's inverse otherwise, and defers targets two readings agree on."""
        reports, previous = self.identity.reports, _READINGS[self.identity]

        def metered(plane: npt.NDArray[np.float32]) -> Option[float]:
            return reports.reported(lambda: state.meter.stops(state.config, plane))().bind(lambda value: value).map(lambda value: value + state.bias)

        match bpy.context.scene, canvas.region, bpy.context.region_data:
            case bpy.types.Scene() as scene, bpy.types.Region() as region, bpy.types.RegionView3D() as view:
                viewing = view.view_perspective == "CAMERA"
                rect = camera_frame(scene, region, view, canvas.extent) if viewing else Some(canvas.extent)
                reading = state.inverse.bind(
                    lambda held: rect.bind(reports.reported(captured)).map(lambda plane: applied(held[0], plane) * np.array((held[1], held[1], held[1], 1.0), np.float32))
                ).bind(metered)
            case _:
                reading = Nothing

        match reading:
            case Option(tag="some", some=value):
                agreed = Option.of_optional(previous.get(state.scene)).filter(lambda prior: abs(value - prior[1]) <= state.tolerance).is_some()
                token = object()
                previous[state.scene] = token, value
                if agreed:
                    self.step(state.scene, token)
            case _:
                previous.pop(state.scene, None)


class AutoExposure(msgspec.Struct, frozen=True, kw_only=True):
    """Scene meter of the earliest Exposure entry's input frame, writing absolute stops into that entry, and the viewport following it through OCIO's inverse of a lattice sampled through the composited formation."""

    attachment: ClassVar[Attachment[Metering]] = Attachment(key="auto_exposure", form=Metering, owner=bpy.types.Scene)
    prerequisites: ClassVar[tuple[Declaration, ...]] = (attachment, _READINGS, _SIGNAL)

    # --- [EXPOSURE]

    @staticmethod
    def entry(identity: Identity, scene: bpy.types.Scene) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Returns the earliest Exposure entry, absent fault without one."""
        return exposure.earliest(identity, scene).to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.compositor_effects), member=exposure.kind))

    @staticmethod
    def written(identity: Identity, scene: bpy.types.Scene, stops: float) -> Outcome[bool]:
        """Writes the earliest Exposure entry's stops input, answering whether it changed."""
        return AutoExposure.entry(identity, scene).bind(lambda held: entered(identity, held, {path_key(STOPS): stops}))

    @staticmethod
    def consumed(identity: Identity, scene: bpy.types.Scene, _entry: bpy.types.SceneCompositorEffect, file: ImageFile) -> Outcome[Option[str]]:
        """Writes the metered stops plus bias, naming the undo step from the command on a change."""

        def kept(metering: Metering, measured: Option[float]) -> Outcome[Option[str]]:
            return measured.map(
                lambda value: AutoExposure.written(identity, scene, value + metering.bias).map(lambda changed: Some(bpy.path.display_name(meter_exposure.key)) if changed else Nothing)
            ).default_value(Ok(Nothing))

        return AutoExposure.attachment.read(identity, scene).bind(lambda metering: metering.meter.stops(PyOpenColorIO.GetCurrentConfig(), file.pixels).bind(lambda measured: kept(metering, measured)))

    @staticmethod
    def stepped(identity: Identity, handle: Handle[bpy.types.Scene], token: object) -> Outcome[Option[str]]:
        """Moves the shown chain gain toward its current agreed target while viewport following remains enabled."""

        def moved(scene: bpy.types.Scene, follow: Follow, current: float, target: float) -> Outcome[Option[str]]:
            step = follow.speed * (target - current)
            return (
                Option
                .of_optional(next((entry for entry in exposure.entries(identity, scene) if entry.enable_for_preview), None))
                .map(lambda held: entry_stops(identity, held).bind(lambda stops: entered(identity, held, {path_key(STOPS): stops + step})).map(lambda _: Nothing))
                .default_value(Ok(Nothing))
                if abs(target - current) > follow.band
                else Ok(Nothing)
            )

        match _READINGS[identity].get(handle):
            case (held, target) if held is token:
                return (
                    handle
                    .resolve()
                    .bind(lambda scene: AutoExposure.attachment.read(identity, scene).map(lambda metering: (scene, metering)))
                    .bind(lambda pair: gain(identity, pair[0], enabled=Enabled.PREVIEW).bind(lambda current: moved(pair[0], pair[1].follow, current, target)) if pair[1].use_follow else Ok(Nothing))
                )
            case _:
                return Ok(Nothing)

    # --- [FORMATION]

    @staticmethod
    def viewport_entries(identity: Identity, scene: bpy.types.Scene) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns preview-enabled pointwise entries other than Exposure, which a lattice samples as the composited viewport's formation."""
        return tuple(entry for entry in scene.compositor_effects if entry.enable_for_preview and chain.row(identity, entry).filter(lambda row: row.pointwise and row is not exposure).is_some())

    @staticmethod
    def requested(config: PyOpenColorIO.Config, scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], view: DisplayView) -> Outcome[_Formation]:
        """Returns the frame and content requested through the entries and view."""
        return fingerprinted(entries).map(
            lambda content: _Formation(
                scene=Handle.of(scene), entries=tuple(entry.name for entry in entries), frame=(scene.frame_current_final, rate(scene)), view=view, config=config.getCacheID(), content=content
            )
        )

    @staticmethod
    def inverted(identity: Identity, request: _Formation, token: object) -> Outcome[None]:
        """Samples the requested formation, publishes its inverse while the request stays wanted, and refreshes viewport following."""
        config, lattice = PyOpenColorIO.GetCurrentConfig(), Lattice(spacing=Logarithmic())
        return (
            lattice
            .samples()
            .bind(lambda samples: PreviewScene().sampled_from(identity, request.scene, request.entries, samples))
            .bind(lambda plane: request.view.form(config, plane))
            .bind(lambda formed: processor(config, PyOpenColorIO.GroupTransform([lattice.transform(formed)], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE)))
            .bind(lambda built: _SIGNAL[identity].fired() if _INVERSES[identity].published(request.scene.uid, token, built.getDefaultCPUProcessor()) else Ok(None))
        )

    @staticmethod
    def followed(identity: Identity, manager: bpy.types.WindowManager) -> Outcome[tuple[Located, ...]]:
        """Returns rendered viewports following exposure, native query order retained and each scene read once."""
        located = _VIEWPORTS.located(manager.windows)
        scenes = tuple(dict.fromkeys(at.window.scene for at in located))
        return (
            accumulated(AutoExposure.attachment.read(identity, scene).map(lambda settings: settings.use_follow) for scene in scenes)
            .map(lambda following: frozenset(scene for scene, on in zip(scenes, following) if on))
            .map(lambda following: tuple(at for at in located if at.window.scene in following))
        )

    @staticmethod
    def jobs(identity: Identity, context: bpy.types.Context) -> Outcome[tuple[queue.Job, ...]]:
        """Returns lattice sampling per followed window scene whose formation inverse is absent."""
        held, config = _INVERSES[identity], PyOpenColorIO.GetCurrentConfig()
        manager: bpy.types.WindowManager = context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

        def queued(request: _Formation) -> tuple[queue.Job, ...]:
            _, token, value = held.wanting(request.scene.uid, request, same=lambda prior, wanted: prior == wanted and all(key.is_some() for key in wanted.content))
            return () if value.is_some() else (queue.Job(place=request.scene.uid, key=token, rank=Rank.SHOWN, run=partial(AutoExposure.inverted, identity, request, token)),)

        def wanted(located: tuple[Located, ...]) -> Outcome[tuple[queue.Job, ...]]:
            sources: dict[bpy.types.Scene, None] = {}
            for at in located:
                for region in at.area.regions:
                    if region.type != "WINDOW":
                        continue
                    with bpy.context.temp_override(window=at.window, area=at.area, region=region):
                        sources.update(dict.fromkeys((at.window.scene,) if _COMPOSITED.holds(bpy.context) else ()))
            active = tuple((scene, entries) for scene in sources for entries in (AutoExposure.viewport_entries(identity, scene),) if entries)
            following, readings = frozenset(Handle.of(at.window.scene) for at in located), _READINGS[identity]
            for handle in readings.keys() - following:
                del readings[handle]
            sampled = frozenset(Handle.of(scene) for scene, _ in active)
            return (_SIGNAL[identity].fired() if held.cleared(lambda request: request.scene not in sampled) else Ok(None)).bind(
                lambda _: accumulated(AutoExposure.requested(config, scene, entries, viewed(scene)).map(queued) for scene, entries in active).map(
                    lambda batches: tuple(job for jobs in batches for job in jobs)
                )
            )

        return AutoExposure.followed(identity, manager).bind(wanted)

    # --- [REGISTRATION]

    @staticmethod
    def ticked(identity: Identity) -> Outcome[Option[float]]:
        """Redraws the viewports of followed window scenes each interval."""
        manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

        def redrawn(located: tuple[Located, ...]) -> Option[float]:
            for at in located:
                at.area.tag_redraw()
            return Some(0.12)

        return AutoExposure.followed(identity, manager).map(redrawn)

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Holds the command, viewer read, lattice sampling, viewport following with its deferred step and redraw timer, and aligned handles over the meter region while it is on."""

        def read(context: bpy.types.Context) -> Outcome[Option[Quad]]:
            return (
                root_in(bpy.types.Scene, context)
                .map(lambda scene: self.attachment.read(identity, scene).map(lambda metering: Some(metering.meter.region).filter(lambda _: metering.use_region).map(Quad.of_region)))
                .default_value(Ok(Nothing))
            )

        def write(context: bpy.types.Context, quad: Quad) -> Outcome[None]:
            return rooted(bpy.types.Scene, context).bind(lambda scene: metered(identity, scene, Region.bounding(quad.samples)))

        def cleared() -> Outcome[None]:
            _READINGS[identity].clear()
            return Ok(None)

        def framed(scene: bpy.types.Scene) -> Outcome[None]:
            _READINGS[identity].pop(Handle.of(scene), None)
            return Ok(None)

        with (
            identity.reports.deferred(partial(self.stepped, identity)) as step,
            Latest.invalidating(identity, _INVERSES, lambda request, _scene: any(key.is_none() for key in request.content), _SIGNAL[identity].fired) as lifecycle,
        ):
            yield Ok((
                *lifecycle,
                meter_exposure,
                READ,
                PreviewScene(),
                PreviewFeed(feed=self.jobs),
                Timer(body=partial(self.ticked, identity)),
                *(Handler(handlers=event, body=cleared) for event in (bpy.app.handlers.undo_post, bpy.app.handlers.redo_post)),
                Handler(handlers=bpy.app.handlers.frame_change_pre, body=framed),
                Overlay[Following](
                    drawing=Drawing(
                        area=_VIEWPORTS, part=Follower(identity, step), refreshes=(_SIGNAL[identity], HandlerRefresh(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene,)))
                    ),
                    respect_overlays=False,
                ),
                FrameRegion(key="meter_region", read=read, write=write, bounds=Some(Region())),
            ))


# --- [OPERATIONS] -----------------------------------------------------------------------


@Operator.of(Empty, condition=Some(Job(kind="RENDER")), options=frozenset())
def meter_exposure(call: Call[Empty]) -> Outcome[Answer]:
    """Meter the Exposure entry input and write absolute stops."""
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: AutoExposure.entry(call.identity, scene).bind(lambda held: READ.entering(call.identity, scene, held, AutoExposure.consumed)))
        .map(lambda _: Done(notes=(("INFO", "Metering exposure entry input"),)))
    )


def metered(identity: Identity, scene: bpy.types.Scene, region: Region) -> Outcome[None]:
    """Writes a region into the scene meter with the region on."""
    return AutoExposure.attachment.read(identity, scene).bind(lambda metering: assigned(metering, {("use_region",): True, ("region",): (region.left, region.bottom, region.right, region.top)}))


def pick_meter_region(call: Call[Picked], region: Region) -> Outcome[Answer]:
    """Meter the next frame over the picked square."""
    return rooted(bpy.types.Scene, call.context).bind(lambda scene: metered(call.identity, scene, region)).map(lambda _: Done(notes=(("INFO", "Metering picked region"),)))


# --- [COMPOSITION] ----------------------------------------------------------------------

auto_exposure: Final = AutoExposure()
meter_pick: Final = picking("CROSSHAIR", Picked, pick_meter_region)
rows: Final = (
    auto_exposure,
    meter_pick,
    Tools(
        space="IMAGE_EDITOR",
        mode=Some("VIEW"),
        tools=(Tool(key="meter", operator=meter_pick, icon="ops.generic.select_box", bindings=((Chord(type=Button.SELECT), Placement(meter_pick)),)),),
        separator=True,
    ),
    Panel(
        section=Section(key=AutoExposure.attachment.key, rows=(OperatorButton(target=meter_pick), OperatorButton(target=meter_exposure)), store=Some(Store(attachment=AutoExposure.attachment))),
        at=Child(parent=RENDER_PT_color_management),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AutoExposure", "Follow", "Following", "Metering", "auto_exposure", "meter_exposure", "meter_pick", "metered", "pick_meter_region", "rows"]
