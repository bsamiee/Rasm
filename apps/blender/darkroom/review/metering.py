"""Darkroom metering: the scene meter written into the earliest Exposure entry, a meter region picked or dragged over the image, and viewport following through the inverse of the shown forming."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, nullcontext
from functools import partial
import math
from typing import ClassVar, Final

from bl_ui.properties_render import RENDER_PT_color_management
import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.calls import Placement
from blender.bpy.commands.keymaps import Button, Chord
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator
from blender.bpy.data.ids.images import fingerprinted
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Job, Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import root_in, rooted
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.events import Timer
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.inputs import entered, path_key
from blender.bpy.render.compositor.viewers import READ
from blender.bpy.render.previews import queue
from blender.bpy.render.previews.queue import Latest, PreviewFeed, Rank
from blender.bpy.render.previews.scenes import PreviewScene
from blender.bpy.scene.cameras import camera_frame
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.drawing.canvas import Canvas, captured, DrawType
from blender.bpy.ui.drawing.handlers import Drawing, Handled, Overlay, Signal
from blender.bpy.ui.images.views import FrameRegion, Picked, picking
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Store
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.screens.areas import AreaQuery
from blender.bpy.ui.surfaces.panels import Child, Panel
from blender.bpy.ui.surfaces.tools import Tool, Tools
from blender.imaging.analysis.metering import Meter
from blender.imaging.color.luts import Lattice, Logarithmic
from blender.imaging.color.processors import DisplayView, processor
from blender.imaging.files.buffers import applied
from blender.imaging.files.images import ImageFile
from blender.imaging.geometry.frames import Quad, Region
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.stores import content_hash, ContentHash

from ..catalog import chain
from ..effects.grade.exposure import exposure, gain, STOPS

# --- [CONSTANTS] ------------------------------------------------------------------------

_LATTICE: Final = Lattice(spacing=Logarithmic())
_INTERVAL: Final = 0.12
_VIEWPORTS: Final = AreaQuery(
    space=bpy.types.SpaceView3D, shows=Some(Setting(descriptor=Descriptor.declared(bpy.types.SpaceView3D, ("shading", "type"), bpy.types.EnumProperty), accepted=("MATERIAL", "RENDERED")))
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
    centre: bpy.props.FloatProperty(name="Center", description="Weight taken from samples outside the center", subtype="FACTOR", min=0.0, max=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
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
        return Meter(low=self.low, high=self.high, centre=self.centre, region=Region(left=left, bottom=bottom, right=right, top=top) if self.use_region else Region())


class Following(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Enabled viewport meter with its bias and agreement, the inverse of the scene view, the inverse of the composited forming, `Nothing` until its lattice is sampled, and the Exposure gain in stops."""

    scene: Handle[bpy.types.Scene]
    meter: Meter
    bias: float
    tolerance: float
    config: PyOpenColorIO.Config
    viewing: PyOpenColorIO.CPUProcessor
    composited: Option[PyOpenColorIO.CPUProcessor]
    gain: float


# --- [SERVICES] -------------------------------------------------------------------------

_INVERSES: Final = Resource[Latest[ContentHash, PyOpenColorIO.CPUProcessor]](made=lambda _identity: nullcontext(Latest()))
_SIGNAL: Final = Resource[Signal](made=lambda _identity: nullcontext(Signal()))


class Follower:
    """Viewport part metering the shown camera frame through the inverse of its forming, stepping toward targets consecutive readings agree on."""

    __slots__ = ("identity", "previous", "step")
    draw_type: ClassVar[DrawType] = "PRE_VIEW"

    def __init__(self, identity: Identity, step: Callable[[Handle[bpy.types.Scene], float], None]) -> None:
        """Holds the identity and deferred step with no previous reading."""
        self.identity, self.step = identity, step
        self.previous: Option[tuple[Handle[bpy.types.Scene], float]] = Nothing

    def read(self, _metrics: Metrics) -> Outcome[Option[Following]]:
        """Returns enabled metering with the inverses of the scene view and of the held composited forming."""
        config, identity = PyOpenColorIO.GetCurrentConfig(), self.identity

        def followed(scene: bpy.types.Scene, metering: Metering, view: DisplayView) -> Outcome[Following]:
            def held(viewing: PyOpenColorIO.CPUProcessor) -> Outcome[Following]:
                return paired(AutoExposure.composited(identity, scene, view, viewing), gain(identity, scene)).map(
                    lambda found: Following(
                        scene=Handle.of(scene), meter=metering.meter, bias=metering.bias, tolerance=metering.follow.tolerance, config=config, viewing=viewing, composited=found[0], gain=found[1]
                    )
                )

            return (
                view
                .transform(config)
                .bind(lambda built: processor(config, PyOpenColorIO.GroupTransform([built.createGroupTransform()], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE)))
                .bind(lambda built: held(built.getDefaultCPUProcessor()))
            )

        return (
            root_in(bpy.types.Scene, bpy.context)
            .map(lambda scene: AutoExposure.attachment.read(identity, scene).bind(lambda metering: followed(scene, metering, viewed(scene)).map(Some) if metering.use_follow else Ok(Nothing)))
            .default_value(Ok(Nothing))
        )

    def drawn(self, canvas: Canvas, state: Following) -> None:
        """Meters the scene ahead of the chain in the visible camera frame, through the composited forming's inverse with the Exposure gain divided out while the viewport composites and the view's inverse otherwise, and defers targets two readings agree on."""
        reports = self.identity.reports

        def metered(plane: npt.NDArray[np.float32]) -> Option[float]:
            return reports.reported(lambda: state.meter.stops(state.config, plane))().bind(lambda value: value).map(lambda value: value + state.bias)

        match bpy.context.scene, canvas.region, canvas.space:
            case bpy.types.Scene() as scene, bpy.types.Region() as region, bpy.types.SpaceView3D(region_3d=bpy.types.RegionView3D() as view) as space if Handle.of(scene) == state.scene:
                viewing = view.view_perspective == "CAMERA"
                composited = space.shading.use_compositor == "ALWAYS" or (space.shading.use_compositor == "CAMERA" and viewing)
                inverse = state.composited.map(lambda held: (held, math.exp2(-state.gain))) if composited else Some((state.viewing, 1.0))
                rect = camera_frame(scene, region, view, canvas.extent) if viewing else Some(canvas.extent)
                reading = inverse.bind(lambda held: rect.bind(reports.reported(captured)).map(lambda plane: applied(held[0], plane) * np.array((held[1], held[1], held[1], 1.0), np.float32))).bind(
                    metered
                )
                for target in reading.filter(lambda found: self.previous.filter(lambda previous: previous[0] == state.scene and abs(found - previous[1]) <= state.tolerance).is_some()).to_list():
                    self.step(state.scene, target)
                self.previous = reading.map(lambda value: (state.scene, value))
            case _:
                self.previous = Nothing


class AutoExposure(msgspec.Struct, frozen=True, kw_only=True):
    """Scene meter of the earliest Exposure entry's input frame, writing absolute stops into that entry, and the viewport following it through OCIO's inverse of a lattice sampled through the composited forming."""

    attachment: ClassVar[Attachment[Metering]] = Attachment(key="auto_exposure", form=Metering, owner=bpy.types.Scene)
    prerequisites: ClassVar[tuple[Declaration, ...]] = (attachment, _INVERSES, _SIGNAL)

    # --- [EXPOSURE]

    @staticmethod
    def entry(identity: Identity, scene: bpy.types.Scene) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Returns the earliest Exposure entry, absent fault without one."""
        return Block.of_seq(exposure.entries(identity, scene)).try_head().to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.compositor_effects), member=exposure.kind))

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
    def stepped(identity: Identity, handle: Handle[bpy.types.Scene], target: float) -> Outcome[Option[str]]:
        """Moves the chain gain toward a target beyond the dead band, outside undo."""

        def moved(scene: bpy.types.Scene, follow: Follow, current: float) -> Outcome[Option[str]]:
            return AutoExposure.written(identity, scene, current + follow.speed * (target - current)).map(lambda _: Nothing) if abs(target - current) > follow.band else Ok(Nothing)

        return handle.resolve().bind(
            lambda scene: AutoExposure.attachment.read(identity, scene).bind(lambda metering: gain(identity, scene).bind(lambda current: moved(scene, metering.follow, current)))
        )

    # --- [FORMING]

    @staticmethod
    def forming(identity: Identity, scene: bpy.types.Scene) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns the preview-enabled entries whose effects act per pixel other than the Exposure gains, the forming of the composited viewport a lattice samples, the gains it divides out."""
        return tuple(entry for entry in scene.compositor_effects if entry.enable_for_preview and chain.row(identity, entry).filter(lambda row: row.pointwise and row is not exposure).is_some())

    @staticmethod
    def keyed(config: PyOpenColorIO.Config, entries: tuple[bpy.types.SceneCompositorEffect, ...], view: DisplayView) -> Outcome[ContentHash]:
        """Returns the content key of the forming through entries and the view under the config."""
        return accumulated(map(fingerprinted, entries)).map(lambda keys: content_hash(AutoExposure, view, config.getCacheID().encode(), *(key for found in keys for key in found.to_list())))

    @staticmethod
    def composited(identity: Identity, scene: bpy.types.Scene, view: DisplayView, viewing: PyOpenColorIO.CPUProcessor) -> Outcome[Option[PyOpenColorIO.CPUProcessor]]:
        """Returns the held inverse of the composited forming, the view's inverse `viewing` without a per-pixel entry, `Nothing` until its lattice is sampled."""
        entries, held = AutoExposure.forming(identity, scene), identity.held[_INVERSES]
        return (
            AutoExposure.keyed(PyOpenColorIO.GetCurrentConfig(), entries, view).map(lambda key: held.wanted.filter(lambda wanted: wanted == key).bind(lambda _: held.value))
            if entries
            else Ok(Some(viewing))
        )

    @staticmethod
    def inverted(identity: Identity, scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], view: DisplayView, key: ContentHash) -> Outcome[None]:
        """Samples the lattice through the entries and the view, publishes the processor of OCIO's inverse of its table while its key stays wanted, then refreshes the following."""
        config = PyOpenColorIO.GetCurrentConfig()
        return (
            _LATTICE
            .samples()
            .bind(lambda samples: PreviewScene().sampled(identity, scene, entries, samples))
            .bind(lambda plane: view.form(config, plane))
            .bind(lambda formed: processor(config, PyOpenColorIO.GroupTransform([_LATTICE.transform(formed)], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE)))
            .map(lambda built: identity.held[_INVERSES].published(key, built.getDefaultCPUProcessor()))
            .bind(lambda _: identity.held[_SIGNAL].fired())
        )

    @staticmethod
    def jobs(identity: Identity, context: bpy.types.Context) -> Outcome[tuple[queue.Job, ...]]:
        """Returns the lattice sampling of the context scene's composited forming while the scene follows and holds no inverse of it."""
        held = identity.held[_INVERSES]

        def wanted(scene: bpy.types.Scene, metering: Metering) -> Outcome[tuple[queue.Job, ...]]:
            entries, view = AutoExposure.forming(identity, scene), viewed(scene)
            return AutoExposure.keyed(PyOpenColorIO.GetCurrentConfig(), entries, view).map(
                lambda key: (
                    (queue.Job(place=AutoExposure, key=key, rank=Rank.SHOWN, run=partial(AutoExposure.inverted, identity, scene, entries, view, key)),)
                    if metering.use_follow and entries and (held.wanting(key) or held.value.is_none())
                    else ()
                )
            )

        return root_in(bpy.types.Scene, context).map(lambda scene: AutoExposure.attachment.read(identity, scene).bind(partial(wanted, scene))).default_value(Ok(()))

    # --- [REGISTRATION]

    @staticmethod
    def ticked(identity: Identity) -> Outcome[Option[float]]:
        """Redraws the viewports while the context scene follows, keeping their read, each interval."""
        manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        following = root_in(bpy.types.Scene, bpy.context).map(lambda scene: AutoExposure.attachment.read(identity, scene).map(lambda metering: metering.use_follow)).default_value(Ok(False))
        for located in _VIEWPORTS.located(manager.windows) if following.default_value(False) else ():
            located.area.tag_redraw()
        return following.map(lambda _: Some(_INTERVAL))

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

        with identity.reports.deferred(lambda handle, target: self.stepped(identity, handle, target)) as step:
            yield Ok((
                meter_exposure,
                READ,
                PreviewScene(),
                PreviewFeed(feed=self.jobs),
                Timer(body=partial(self.ticked, identity)),
                Overlay[Following](
                    drawing=Drawing(
                        area=_VIEWPORTS, part=Follower(identity, step), refreshes=(identity.held[_SIGNAL], Handled(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene,)))
                    ),
                    respect_overlays=False,
                ),
                FrameRegion(key="meter_region", read=read, write=write, bounds=Some(Region())),
            ))


# --- [OPERATIONS] -----------------------------------------------------------------------


@Operator.of(Empty, condition=Some(Job(kind="RENDER")), undo=Nothing)
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
