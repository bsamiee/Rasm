"""Darkroom exposure metering: the scene meter written into the earliest Exposure entry from its input frame, viewport following, and the meter region picked or dragged in the render view."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from functools import cached_property
from pathlib import Path
from typing import ClassVar, Final

from bl_ui.properties_render import RENDER_PT_color_management
import bpy
from bpy_extras.view3d_utils import location_3d_to_region_2d
from expression import Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.keymaps import Button, Chord
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Job, Setting
from blender.bpy.data.rna.descriptors import assigned, Descriptor, root_in
from blender.bpy.extension.events import Timer
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.render.compositor.effects import Enabled, entered
from blender.bpy.render.compositor.viewers import READ, ViewerRead
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.drawing.canvas import Canvas, Rect
from blender.bpy.ui.drawing.handlers import Drawing, Overlay, Signal, Updated
from blender.bpy.ui.drawing.offscreen import captured
from blender.bpy.ui.images.cages import Access, Aligned, FrameRegion
from blender.bpy.ui.images.picks import Picked, picking
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.screens.areas import AreaQuery
from blender.bpy.ui.surfaces.panels import Child, Panel
from blender.bpy.ui.surfaces.tools import Appended, ImageEditor, Tool, Tools
from blender.imaging.analysis.metering import Meter
from blender.imaging.color.processors import processor
from blender.imaging.files.buffers import applied
from blender.imaging.files.images import ImageFile
from blender.imaging.geometry.frames import Quad, Region
from blender.records.faults import Fault, Faults, Outcome

from ..catalog import chain
from ..effects.grade.exposure import exposure

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
    centre: bpy.props.FloatProperty(name="Centre", description="Weight taken from samples outside the center", subtype="FACTOR", min=0.0, max=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_region: bpy.props.BoolProperty(name="Region", description="Meter the region alone, the whole frame while off")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    region: bpy.props.FloatVectorProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Region", description="Metered frame share from bottom left as left, bottom, right, and top", size=4, min=0.0, max=1.0, default=(0.0, 0.0, 1.0, 1.0)
    )
    bias: bpy.props.FloatProperty(name="EV Bias", description="Stops added to each reading", min=-3.0, max=3.0, soft_min=-2.0, soft_max=2.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_follow: bpy.props.BoolProperty(name="Follow Viewport", description="Step the chain gain toward viewport readings")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    follow: bpy.props.PointerProperty(type=Follow, name="Follow Viewport")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @property
    def meter(self) -> Meter:
        """Meter of the trims, center weight, and region, the whole frame while the region is off."""
        left, bottom, right, top = self.region
        return Meter(low=self.low, high=self.high, centre=self.centre, region=Region(left=left, bottom=bottom, right=right, top=top) if self.use_region else Region())


class Following(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Enabled viewport meter with its bias and agreement, and the inverse of the preview chain."""

    scene: Handle[bpy.types.Scene]
    meter: Meter
    bias: float
    tolerance: float
    config: PyOpenColorIO.Config
    inverse: Option[PyOpenColorIO.CPUProcessor]


# --- [SERVICES] -------------------------------------------------------------------------


class Follower:
    """Viewport part metering the shown camera frame, stepping toward targets consecutive readings agree on."""

    __slots__ = ("identity", "previous", "step")

    def __init__(self, identity: Identity, step: Callable[[Handle[bpy.types.Scene], float], None]) -> None:
        """Holds the identity and deferred step with no previous reading."""
        self.identity, self.step = identity, step
        self.previous: Option[tuple[Handle[bpy.types.Scene], float]] = Nothing

    def read(self, _metrics: Metrics) -> Outcome[Option[Following]]:
        """Returns enabled metering with the preview chain's inverse."""
        config = PyOpenColorIO.GetCurrentConfig()

        def followed(scene: bpy.types.Scene, metering: Metering) -> Outcome[Following]:
            inverse = chain.forming(self.identity, scene, Enabled.PREVIEW).bind(
                lambda formed: formed.map(
                    lambda transform: processor(config, PyOpenColorIO.GroupTransform([transform], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE)).map(
                        lambda built: Some(built.getDefaultCPUProcessor())
                    )
                ).default_value(Ok(Nothing))
            )
            return inverse.map(lambda held: Following(scene=Handle.of(scene), meter=metering.meter, bias=metering.bias, tolerance=metering.follow.tolerance, config=config, inverse=held))

        return (
            root_in(bpy.types.Scene, bpy.context)
            .map(lambda scene: AutoExposure.attachment.read(self.identity, scene).bind(lambda metering: followed(scene, metering).map(Some) if metering.use_follow else Ok(Nothing)))
            .default_value(Ok(Nothing))
        )

    def drawn(self, canvas: Canvas, state: Following) -> None:
        """Meters the visible camera frame, through the inverse chain while the viewport composites, and defers targets two readings agree on."""
        reports = self.identity.reports

        def framed(scene: bpy.types.Scene, region: bpy.types.Region, view: bpy.types.RegionView3D, camera: bpy.types.Object, lens: bpy.types.Camera) -> Option[Rect]:
            corners = tuple(Option.of_optional(location_3d_to_region_2d(region, view, camera.matrix_world @ corner)) for corner in lens.view_frame(scene=scene))
            if any(corner.is_none() for corner in corners):
                return Nothing
            points = np.asarray(tuple(point.to_tuple() for corner in corners for point in corner.to_list()))
            low = np.maximum(points.min(axis=0), (canvas.extent.x, canvas.extent.y))
            high = np.minimum(points.max(axis=0), (canvas.extent.x + canvas.extent.width, canvas.extent.y + canvas.extent.height))
            return Some(Rect(x=float(low[0]), y=float(low[1]), width=float(high[0] - low[0]), height=float(high[1] - low[1]))).filter(lambda rect: rect.width > 0.0 and rect.height > 0.0)

        def metered(plane: npt.NDArray[np.float32]) -> Option[float]:
            return reports.reported(lambda: state.meter.stops(state.config, plane))().bind(lambda value: value).map(lambda value: value + state.bias)

        match bpy.context.scene, bpy.context.region, canvas.space.to_optional():
            case bpy.types.Scene() as scene, bpy.types.Region() as region, bpy.types.SpaceView3D(region_3d=bpy.types.RegionView3D() as view) as space if Handle.of(scene) == state.scene:
                viewing = view.view_perspective == "CAMERA"
                composited = space.shading.use_compositor == "ALWAYS" or (space.shading.use_compositor == "CAMERA" and viewing)
                match viewing, scene.camera:
                    case True, bpy.types.Object(data=bpy.types.Camera() as lens) as camera:
                        rect = framed(scene, region, view, camera, lens)
                    case _:
                        rect = Some(canvas.extent)
                reading = rect.bind(reports.reported(captured)).bind(lambda plane: state.inverse.map(lambda inverse: applied(inverse, plane)) if composited else Some(plane)).bind(metered)
                for target in reading.filter(lambda found: self.previous.filter(lambda previous: previous[0] == state.scene and abs(found - previous[1]) <= state.tolerance).is_some()).to_list():
                    self.step(state.scene, target)
                self.previous = reading.map(lambda value: (state.scene, value))
            case _:
                self.previous = Nothing


class AutoExposure(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Scene meter of the earliest Exposure entry's input frame, writing absolute stops into that entry, and the viewport following it."""

    reading: ViewerRead = READ
    attachment: ClassVar[Attachment[Metering]] = Attachment(key="auto_exposure", form=Metering, owner=bpy.types.Scene)
    prerequisites: ClassVar[tuple[Registrable, ...]] = (attachment,)

    # --- [EXPOSURE]

    @staticmethod
    def entry(identity: Identity, scene: bpy.types.Scene) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Returns the earliest Exposure entry, absent fault without one."""
        return Option.of_optional(next(iter(exposure.entries(identity, scene)), None)).to_result_with(
            lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(scene.compositor_effects), member=exposure.kind),)
        )

    @staticmethod
    def written(identity: Identity, scene: bpy.types.Scene, stops: float) -> Outcome[bool]:
        """Writes the earliest Exposure entry's stops input, answering whether it changed."""
        return AutoExposure.entry(identity, scene).bind(lambda held: entered(identity, held, {"stops": stops}))

    @staticmethod
    def consumed(identity: Identity, scene: bpy.types.Scene, _entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> Outcome[Option[str]]:
        """Writes the metered stops plus bias, naming the undo step from the command on a change."""

        def kept(metering: Metering, measured: Option[float]) -> Outcome[Option[str]]:
            return measured.map(
                lambda value: AutoExposure.written(identity, scene, value + metering.bias).map(
                    lambda changed: Some(bpy.path.display_name(AutoExposure.meter_exposure.__name__)) if changed else Nothing
                )
            ).default_value(Ok(Nothing))

        return AutoExposure.attachment.read(identity, scene).bind(
            lambda metering: Result[Option[float], Faults].bind(metering.meter.stops(PyOpenColorIO.GetCurrentConfig(), file.pixels), lambda measured: kept(metering, measured))
        )

    @staticmethod
    def stepped(identity: Identity, handle: Handle[bpy.types.Scene], target: float) -> Outcome[Option[str]]:
        """Moves the chain gain toward a target beyond the dead band, outside undo."""

        def moved(scene: bpy.types.Scene, follow: Follow, current: float) -> Outcome[Option[str]]:
            return AutoExposure.written(identity, scene, current + follow.speed * (target - current)).map(lambda _: Nothing) if abs(target - current) > follow.band else Ok(Nothing)

        return handle.resolve().bind(
            lambda scene: AutoExposure.attachment.read(identity, scene).bind(lambda metering: chain.gain(identity, scene).bind(lambda current: moved(scene, metering.follow, current)))
        )

    # --- [COMMANDS]

    def meter_exposure(self, call: Call[Empty]) -> Outcome[Answer]:
        """Meter the Exposure entry input and write absolute stops."""
        return (
            call
            .root(bpy.types.Scene)
            .bind(lambda scene: self.entry(call.identity, scene).bind(lambda held: self.reading.entering(call.identity, scene, held, self.consumed)))
            .map(lambda _: Done(notes=(("INFO", "Metering exposure entry input"),)))
        )

    @cached_property
    def metering(self) -> Operator[Empty]:
        """Command arming the viewer read on the earliest Exposure entry's input."""
        return Operator(form=Empty, run=self.meter_exposure, condition=Some(Job(kind="RENDER")), undo=False)

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Holds the command, viewer read, viewport following with its deferred step, and aligned handles over the meter region while it is on."""
        refresh, interval = Signal(), 0.12

        def access(context: bpy.types.Context) -> Outcome[Option[Access]]:
            return (
                root_in(bpy.types.Scene, context)
                .map(
                    lambda scene: self.attachment.read(identity, scene).map(
                        lambda metering: (
                            Some(metering.meter.region)
                            .filter(lambda _: metering.use_region)
                            .map(lambda region: Access(quad=Quad.of_region(region), write=lambda quad: metered(identity, scene, Region.bounding(quad.samples))))
                        )
                    )
                )
                .default_value(Ok(Nothing))
            )

        with identity.reports.deferred(lambda handle, target: self.stepped(identity, handle, target)) as step:
            yield Ok((
                self.metering,
                self.reading,
                Timer(body=lambda: refresh.fired().map(lambda _: Some(interval))),
                Overlay[Following](
                    drawing=Drawing(
                        area=AreaQuery(
                            space="VIEW_3D", shows=Some(Setting(descriptor=Descriptor.declared(bpy.types.SpaceView3D, "shading.type", bpy.types.EnumProperty), accepted=("MATERIAL", "RENDERED")))
                        ),
                        part=Follower(identity, step),
                        refreshes=(refresh, Updated(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene,))),
                    ),
                    space=bpy.types.SpaceView3D,
                    draw_type="PRE_VIEW",
                    respect_overlays=False,
                ),
                FrameRegion(key="meter_region", access=access, shape=Aligned(bounds=Some(Region()))),
            ))


# --- [OPERATIONS] -----------------------------------------------------------------------


def metered(identity: Identity, scene: bpy.types.Scene, region: Region) -> Outcome[None]:
    """Writes a region into the scene meter with the region on."""
    return AutoExposure.attachment.read(identity, scene).bind(lambda metering: assigned(metering, {"use_region": True, "region": (region.left, region.bottom, region.right, region.top)}))


def pick_meter_region(call: Call[Picked], region: Region) -> Outcome[Answer]:
    """Meter the next frame over the picked square."""
    return call.root(bpy.types.Scene).bind(lambda scene: metered(call.identity, scene, region)).map(lambda _: Done(notes=(("INFO", "Metering picked region"),)))


# --- [COMPOSITION] ----------------------------------------------------------------------

auto_exposure: Final = AutoExposure()
meter_pick: Final = picking(pick_meter_region.__name__, "CROSSHAIR", Picked, pick_meter_region)
rows: Final = (
    auto_exposure,
    meter_pick,
    Tools(
        toolbar=ImageEditor(mode="VIEW"),
        tools=(Tool(key="meter", title=meter_pick.title, operator=meter_pick, icon=Path("region.svg"), bindings={Chord(type=Button.SELECT): meter_pick}),),
        placement=Appended(separator=True),
    ),
    Panel(
        section=Single(
            key=AutoExposure.attachment.key,
            title=bpy.path.display_name(AutoExposure.attachment.key),
            rows=(OperatorButton(target=meter_pick), OperatorButton(target=auto_exposure.metering)),
            store=Some(Hosted(store=Store(attachment=AutoExposure.attachment))),
        ),
        at=Child(parent=RENDER_PT_color_management),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AutoExposure", "Follow", "Metering", "meter_pick", "rows"]
