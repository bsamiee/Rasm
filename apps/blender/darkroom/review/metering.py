"""Darkroom exposure metering: the scene meter written into the earliest Exposure entry from its input frame, viewport following, and the meter region picked or dragged in the render view."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from functools import cached_property
from pathlib import Path
from typing import Annotated, ClassVar, Final

from bl_ui.properties_render import RENDER_PT_color_management
import bpy
from bpy_extras.view3d_utils import location_3d_to_region_2d
from expression import Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.keymaps import Button, Chord
from blender.bpy.commands.operators import Call, Done, Empty, Operator
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Job, Setting
from blender.bpy.data.rna.descriptors import Descriptor, root_in
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.compositor.effects import Enabled, revised
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
from blender.imaging.files.images import applied, ImageFile
from blender.imaging.geometry.homography import Quad
from blender.imaging.geometry.regions import Region
from blender.records.faults import paired
from blender.records.fields import Gated, heading, Presentation

from ..catalog import chain
from ..effects.grade.exposure import exposure

# --- [MODELS] ---------------------------------------------------------------------------


class Follow(msgspec.Struct, frozen=True, kw_only=True):
    """Viewport response speed, dead band, and agreement between readings."""

    speed: Annotated[float, msgspec.Meta(ge=0.02, le=1.0, description="Share of remaining step per update")] = 0.5
    band: Annotated[float, msgspec.Meta(ge=0.0, le=2.0, title="Dead Band"), Presentation(soft=Some((0.0, 1.0)), step=Some(0.01), precision=Some(2))] = 0.1
    tolerance: Annotated[float, msgspec.Meta(ge=0.0, le=2.0, description="Stops between agreeing readings"), Presentation(soft=Some((0.0, 1.0)), step=Some(0.01), precision=Some(2))] = 0.1


class Metering(msgspec.Struct, frozen=True, kw_only=True):
    """One scene's meter shared by rendered-frame and viewport exposure."""

    meter: Meter = Meter()
    bias: Annotated[float, msgspec.Meta(ge=-3.0, le=3.0, title="EV Bias"), Presentation(soft=Some((-2.0, 2.0)))] = 0.0
    follow: Annotated[Gated[Follow], msgspec.Meta(title="Follow Viewport")] = Gated(enabled=False, value=Follow())


class Following(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Enabled viewport meter with the inverse of the preview chain."""

    scene: Handle[bpy.types.Scene]
    metering: Metering
    config: PyOpenColorIO.Config
    inverse: Option[PyOpenColorIO.CPUProcessor]


# --- [SERVICES] -------------------------------------------------------------------------


class Follower(msgspec.Struct, kw_only=True):
    """Viewport part metering the shown camera frame, stepping toward targets consecutive readings agree on."""

    identity: Identity
    step: Callable[[Handle[bpy.types.Scene], float], None]
    previous: Option[tuple[Handle[bpy.types.Scene], float]] = Nothing

    def read(self, _metrics: Metrics) -> BlenderResult[Option[Following]]:
        """Returns enabled metering with the preview chain's inverse."""

        def followed(scene: bpy.types.Scene, config: PyOpenColorIO.Config, metering: Metering) -> BlenderResult[Following]:
            inverse = chain.forming(self.identity, scene, Enabled.PREVIEW).bind(
                lambda formed: formed.map(
                    lambda transform: processor(config, PyOpenColorIO.GroupTransform([transform], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE)).map(
                        lambda built: Some(built.getDefaultCPUProcessor())
                    )
                ).default_value(Ok(Nothing))
            )
            return inverse.map(lambda held: Following(scene=Handle.of(scene), metering=metering, config=config, inverse=held))

        return (
            root_in(bpy.types.Scene, bpy.context)
            .map(lambda scene: paired(config_of(), AutoExposure.attachment.read(self.identity, scene)).bind(lambda pair: followed(scene, *pair).map(Some) if pair[1].follow.enabled else Ok(Nothing)))
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
            return reports.reported(lambda: state.metering.meter.stops(state.config, plane))().bind(lambda value: value).map(lambda value: value + state.metering.bias)

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
                for target in reading.filter(
                    lambda found: self.previous.filter(lambda previous: previous[0] == state.scene and abs(found - previous[1]) <= state.metering.follow.value.tolerance).is_some()
                ).to_list():
                    self.step(state.scene, target)
                self.previous = reading.map(lambda value: (state.scene, value))
            case _:
                self.previous = Nothing


class AutoExposure(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Scene meter of the earliest Exposure entry's input frame, writing absolute stops into that entry, and the viewport following it."""

    reading: ViewerRead = READ
    title: ClassVar[str] = heading("meter_exposure")
    attachment: ClassVar[Attachment[Metering]] = Attachment(key="auto_exposure", form=Metering, owner=bpy.types.Scene)
    prerequisites: ClassVar[tuple[Registrable, ...]] = (attachment,)

    # --- [EXPOSURE]

    @staticmethod
    def entry(identity: Identity, scene: bpy.types.Scene) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Returns the earliest Exposure entry, absent fault without one."""
        return Option.of_optional(next(iter(exposure.entries(identity, scene)), None)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=exposure.kind),))

    @staticmethod
    def written(identity: Identity, scene: bpy.types.Scene, stops: float) -> BlenderResult[bool]:
        """Writes the earliest Exposure entry's stops, answering whether its inputs changed."""
        return AutoExposure.entry(identity, scene).bind(lambda held: revised(exposure, identity, scene, held, lambda record: Ok(msgspec.structs.replace(record, stops=stops))))

    @staticmethod
    def consumed(identity: Identity, scene: bpy.types.Scene, _entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> BlenderResult[Option[str]]:
        """Writes the metered stops plus bias, naming the undo step from the command on a change."""

        def kept(metering: Metering, measured: Option[float]) -> BlenderResult[Option[str]]:
            return measured.map(lambda value: AutoExposure.written(identity, scene, value + metering.bias).map(lambda changed: Some(AutoExposure.title) if changed else Nothing)).default_value(
                Ok(Nothing)
            )

        return paired(config_of(), AutoExposure.attachment.read(identity, scene)).bind(
            lambda pair: Result[Option[float], BlenderFaults].bind(pair[1].meter.stops(pair[0], file.pixels), lambda measured: kept(pair[1], measured))
        )

    @staticmethod
    def stepped(identity: Identity, handle: Handle[bpy.types.Scene], target: float) -> BlenderResult[Option[str]]:
        """Moves the chain gain toward a target beyond the dead band, outside undo."""

        def moved(scene: bpy.types.Scene, metering: Metering, current: float) -> BlenderResult[Option[str]]:
            follow = metering.follow.value
            return AutoExposure.written(identity, scene, current + follow.speed * (target - current)).map(lambda _: Nothing) if abs(target - current) > follow.band else Ok(Nothing)

        return handle.resolve().bind(lambda scene: paired(AutoExposure.attachment.read(identity, scene), chain.gain(identity, scene)).bind(lambda pair: moved(scene, *pair)))

    # --- [COMMANDS]

    @cached_property
    def metering(self) -> Operator[Empty]:
        """Command arming the viewer read on the earliest Exposure entry's input."""

        def run(call: Call[Empty]) -> BlenderResult[Done]:
            return (
                call
                .root(bpy.types.Scene)
                .bind(lambda scene: self.entry(call.identity, scene).bind(lambda held: self.reading.entering(call.identity, scene, held, self.consumed)))
                .map(lambda _: Done(notes=(("INFO", "Metering exposure entry input"),)))
            )

        return Operator(
            key="meter_exposure", title=AutoExposure.title, description="Meter the Exposure entry input and write absolute stops", form=Empty, run=run, condition=Some(Job(kind="RENDER")), undo=False
        )

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds the command, viewer read, and viewport following with its deferred step."""
        refresh, interval = Signal(), 0.12
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
                        part=Follower(identity=identity, step=step),
                        refreshes=(refresh, Updated(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene,))),
                    ),
                    space=bpy.types.SpaceView3D,
                    draw_type="PRE_VIEW",
                    respect_overlays=False,
                ),
            ))


class MeterRegion(msgspec.Struct, frozen=True, kw_only=True):
    """Aligned frame handles over the scene's gated meter region, held inside the frame."""

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the region handles."""
        return Ok((FrameRegion(key="meter_region", access=lambda context: region_access(identity, context), shape=Aligned(bounds=Some(Region()))),))


# --- [OPERATIONS] -----------------------------------------------------------------------


def metered(identity: Identity, scene: bpy.types.Scene, region: Region) -> BlenderResult[None]:
    """Writes a region into the scene meter with its gate on."""
    return AutoExposure.attachment.revised(identity, scene, lambda held: Ok(msgspec.structs.replace(held, meter=msgspec.structs.replace(held.meter, region=Gated(enabled=True, value=region))))).map(
        lambda _: None
    )


def region_access(identity: Identity, context: bpy.types.Context) -> BlenderResult[Option[Access]]:
    """Reads the scene's gated meter region as a quad writing back through `metered`."""

    def read(scene: bpy.types.Scene) -> BlenderResult[Option[Access]]:
        return AutoExposure.attachment.read(identity, scene).map(
            lambda held: (
                Some(held.meter.region)
                .filter(lambda gated: gated.enabled)
                .map(lambda gated: Access(quad=Quad.of_region(gated.value), write=lambda quad: metered(identity, scene, Region.bounding(quad.samples))))
            )
        )

    return root_in(bpy.types.Scene, context).map(read).default_value(Ok(Nothing))


# --- [COMPOSITION] ----------------------------------------------------------------------

auto_exposure: Final = AutoExposure()
meter_pick: Final = picking(
    "pick_meter",
    "Pick Meter Region",
    "Meter the next frame over the picked square",
    "CROSSHAIR",
    Picked,
    lambda call, region: call.root(bpy.types.Scene).bind(lambda scene: metered(call.identity, scene, region)).map(lambda _: Done(notes=(("INFO", "Metering picked region"),))),
)
rows: Final = (
    auto_exposure,
    MeterRegion(),
    meter_pick,
    Tools(
        toolbar=ImageEditor(mode="VIEW"),
        tools=(Tool(key="meter", title=heading("meter_region"), operator=meter_pick, icon=Path("region.svg"), bindings={Chord(type=Button.SELECT): meter_pick}),),
        placement=Appended(separator=True),
    ),
    Panel(
        section=Single(
            key=AutoExposure.attachment.key,
            title=heading(AutoExposure.attachment.key),
            rows=(OperatorButton(target=meter_pick), OperatorButton(target=auto_exposure.metering)),
            store=Some(Hosted(store=Store(attachment=AutoExposure.attachment))),
        ),
        at=Child(parent=RENDER_PT_color_management),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AutoExposure", "Follow", "Metering", "meter_pick", "rows"]
