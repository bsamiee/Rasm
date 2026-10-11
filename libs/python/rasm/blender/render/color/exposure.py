"""Exposure metering at an effect input and scoped viewport following."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from typing import Annotated, ClassVar, Protocol

import bpy
from bpy_extras.view3d_utils import location_3d_to_region_2d
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm.blender.commands.operators import Call, Done, Empty, Operator
from rasm.blender.data.ids.store import Handle
from rasm.blender.data.rna.conditions import Job, Setting
from rasm.blender.data.rna.descriptors import Descriptor
from rasm.blender.extension.events.timers import Timer
from rasm.blender.extension.identity import Identity, Key
from rasm.blender.extension.registration import Acquisitions, Registrable
from rasm.blender.faults import BlenderAbsentFault, BlenderFaults, BlenderImagingFault, BlenderResult
from rasm.blender.render.color.luts import Enabled, Forming
from rasm.blender.settings.attachments import Attached, Attachment
from rasm.blender.ui.drawing.canvas import Canvas, Rect
from rasm.blender.ui.drawing.handlers import Drawing, Overlay, Signal, Updated
from rasm.blender.ui.drawing.offscreen import captured
from rasm.blender.ui.resources.theme import Metrics
from rasm.blender.ui.screens.areas import AreaQuery
from rasm.imaging.analysis.metering import Meter
from rasm.imaging.chain import Effect
from rasm.imaging.color.processors import applied, processor
from rasm.imaging.faults import ImagingResult, paired
from rasm.imaging.files.images import ImageFile
from rasm.imaging.parameters import Gated, Presentation

# --- [TYPES] ----------------------------------------------------------------------------

type Consume = Callable[[Identity, bpy.types.SceneCompositorEffect, ImageFile[np.float32]], BlenderResult[Option[str]]]


class Gain(Protocol):
    """Owned exposure effect's absolute stops and native write operation."""

    @property
    def record(self) -> type[Effect]:
        """Record identifying exposure entries in the stack."""
        ...

    def stops(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[float]:
        """Returns the earliest owned exposure entry's absolute stops."""
        ...

    def written(self, identity: Identity, scene: bpy.types.Scene, stops: float) -> BlenderResult[None]:
        """Writes absolute stops into the earliest owned entry."""
        ...


class Read(Registrable, Protocol):
    """Scoped viewer read armed on one effect's entering plane."""

    def entering(self, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[None]:
        """Arms the entry's input and retires its viewer after consumption."""
        ...


class Reading(Protocol):
    """Construction of a viewer read with its deferred plane consumer."""

    def __call__(self, *, consume: Consume) -> Read:
        """Returns the original viewer declaration for central registration."""
        ...


# --- [MODELS] ---------------------------------------------------------------------------


class Follow(msgspec.Struct, frozen=True, kw_only=True):
    """Viewport response speed, dead band, and agreement between readings."""

    speed: Annotated[float, msgspec.Meta(ge=0.02, le=1.0, description="Share of the remaining step taken per update")] = 0.5
    band: Annotated[float, msgspec.Meta(ge=0.0, le=2.0, title="Dead Band"), Presentation(soft=Some((0.0, 1.0)), step=Some(0.01), decimals=Some(2))] = 0.1
    tolerance: Annotated[float, msgspec.Meta(ge=0.0, le=2.0, description="Agreement in stops before following"), Presentation(soft=Some((0.0, 1.0)), step=Some(0.01), decimals=Some(2))] = 0.1


class Metering(msgspec.Struct, frozen=True, kw_only=True):
    """One scene's meter shared by rendered-frame and viewport exposure."""

    meter: Meter = Meter()
    bias: Annotated[float, msgspec.Meta(ge=-3.0, le=3.0, title="EV Bias"), Presentation(soft=Some((-2.0, 2.0)))] = 0.0
    follow: Annotated[Gated[Follow], msgspec.Meta(title="Follow Viewport")] = Gated(enabled=False, value=Follow())


class Following(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Admitted viewport meter and optional inverse of its preview chain."""

    scene: Handle[bpy.types.Scene]
    meter: Meter
    bias: float
    tolerance: float
    config: PyOpenColorIO.Config
    inverse: Option[PyOpenColorIO.CPUProcessor]


class Exposing(msgspec.Struct, frozen=True, kw_only=True):
    """Bound metering storage, stack, and exposure destination for every reading."""

    identity: Identity
    attached: Attached[Metering]
    stack: Forming
    gain: Gain
    title: ClassVar[str] = "Meter Exposure"

    def consumed(self, identity: Identity, entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> BlenderResult[Option[str]]:
        """Writes absolute metered stops and names one undo step when samples exist."""
        scene: bpy.types.Scene = entry.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        match paired(self.stack.config(), self.attached.read(scene)):
            case Result(tag="ok", ok=(config, metering)):
                pass
            case Result(error=faults):
                return Error(faults)
        return Result[Option[float], BlenderFaults].bind(
            metering.meter.stops(config, file.pixels).map_error(lambda faults: (BlenderImagingFault(faults=faults),)),
            lambda measured: measured.map(lambda value: self.gain.written(identity, scene, value + metering.bias).map(lambda _: Some(self.title))).default_with(lambda: Ok(Nothing)),
        )

    def stepped(self, handle: Handle[bpy.types.Scene], target: float) -> BlenderResult[Option[str]]:
        """Moves enabled following toward a target beyond its dead band without undo."""
        return handle.resolve().bind(
            lambda scene: paired(self.attached.read(scene), self.gain.stops(self.identity, scene)).bind(
                lambda pair: (
                    self.gain.written(self.identity, scene, pair[1] + pair[0].follow.value.speed * (target - pair[1])).map(lambda _: Nothing)
                    if pair[0].follow.enabled and abs(target - pair[1]) > pair[0].follow.value.band
                    else Ok(Nothing)
                )
            )
        )


class Follower(msgspec.Struct, kw_only=True):
    """Viewport part measuring pixels and scheduling agreed exposure targets."""

    exposing: Exposing
    step: Callable[[Handle[bpy.types.Scene], float], None]
    previous: Option[tuple[Handle[bpy.types.Scene], float]] = Nothing

    def read(self, _metrics: Metrics) -> BlenderResult[Option[Following]]:
        """Returns enabled metering with the current preview inverse or its refusal."""
        if (scene := bpy.context.scene) is None:
            return Ok(Nothing)

        def followed(config: PyOpenColorIO.Config, metering: Metering) -> BlenderResult[Following]:
            match self.exposing.stack.forming(self.exposing.identity, scene, config, Enabled.PREVIEW):
                case Result(tag="ok", ok=chain):
                    inverse: ImagingResult[Option[PyOpenColorIO.CPUProcessor]] = chain.map(
                        lambda transform: (
                            processor(config, PyOpenColorIO.GroupTransform([transform], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE)).map(PyOpenColorIO.Processor.getDefaultCPUProcessor).map(Some)
                        )
                    ).default_value(Ok(Nothing))
                case Result(error=faults):
                    return Error(faults)
            return inverse.map_error(lambda faults: (BlenderImagingFault(faults=faults),)).map(
                lambda inverse: Following(scene=Handle.of(scene), meter=metering.meter, bias=metering.bias, tolerance=metering.follow.value.tolerance, config=config, inverse=inverse)
            )

        return paired(self.exposing.stack.config(), self.exposing.attached.read(scene)).bind(lambda pair: followed(*pair).map(Some) if pair[1].follow.enabled else Ok(Nothing))

    def drawn(self, canvas: Canvas, state: Following) -> None:
        """Meters the visible camera frame and defers targets agreeing in one scene."""
        scene = bpy.context.scene

        def framed(region: bpy.types.Region, view: bpy.types.RegionView3D, camera: bpy.types.Object, lens: bpy.types.Camera) -> Option[Rect]:
            corners = tuple(Option.of_optional(location_3d_to_region_2d(region, view, camera.matrix_world @ corner)) for corner in lens.view_frame(scene=scene))
            if any(corner.is_none() for corner in corners):
                return Nothing
            points = np.asarray(tuple(point.to_tuple() for corner in corners for point in corner.to_list()))
            low = np.maximum(points.min(axis=0), (canvas.extent.x, canvas.extent.y))
            high = np.minimum(points.max(axis=0), (canvas.extent.x + canvas.extent.width, canvas.extent.y + canvas.extent.height))
            return Some(Rect(x=float(low[0]), y=float(low[1]), width=float(high[0] - low[0]), height=float(high[1] - low[1]))).filter(lambda rect: rect.width > 0.0 and rect.height > 0.0)

        match scene, bpy.context.region, canvas.space.to_optional():
            case bpy.types.Scene() as scene, bpy.types.Region() as region, bpy.types.SpaceView3D(region_3d=bpy.types.RegionView3D() as view) as space:
                viewing = view.view_perspective == "CAMERA"
                composited = space.shading.use_compositor == "ALWAYS" or (space.shading.use_compositor == "CAMERA" and viewing)
                match viewing, scene.camera:
                    case True, bpy.types.Object(data=bpy.types.Camera() as lens) as camera:
                        rect = framed(region, view, camera, lens)
                    case _:
                        rect = Some(canvas.extent)

                def metered(plane: npt.NDArray[np.float32]) -> Option[float]:
                    return (
                        self.exposing.identity.reports
                        .reported(lambda: state.meter.stops(state.config, plane).map_error(lambda faults: (BlenderImagingFault(faults=faults),)))()
                        .bind(lambda value: value)
                        .map(lambda value: value + state.bias)
                    )

                reading = (
                    Some(scene)
                    .filter(lambda shown: Handle.of(shown) == state.scene)
                    .bind(lambda _: rect.bind(self.exposing.identity.reports.reported(captured)))
                    .bind(lambda plane: state.inverse.map(lambda inverse: applied(inverse, plane)) if composited else Some(plane))
                    .bind(metered)
                )
                for target in reading.filter(lambda found: self.previous.filter(lambda previous: previous[0] == state.scene and abs(found - previous[1]) <= state.tolerance).is_some()).to_list():
                    self.step(state.scene, target)
                self.previous = reading.map(lambda value: (state.scene, value))
            case _:
                self.previous = Nothing


class AutoExposure(msgspec.Struct, frozen=True, kw_only=True):
    """Rendered-frame meter and viewport following over original scoped declarations."""

    stack: Forming
    gain: Gain
    reading: Reading
    key: ClassVar[Key] = "auto_exposure"
    attachment: ClassVar[Attachment[Metering]] = Attachment(key=key, form=Metering, owner=bpy.types.Scene)
    page_rows: ClassVar[tuple[Registrable, ...]] = (attachment,)

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Binds registered storage and scopes deferred work through native teardown."""
        resolved = paired(self.attachment.bound(identity), Descriptor.resolve(bpy.types.SpaceView3D, "shading.type", bpy.types.EnumProperty))
        match resolved:
            case Result(tag="ok", ok=(attached, shading)):
                exposing = Exposing(identity=identity, attached=attached, stack=self.stack, gain=self.gain)
                read = self.reading(consume=exposing.consumed)
                refresh, interval = Signal(), 0.12

                def metered(call: Call[Empty]) -> BlenderResult[Done]:
                    if (scene := call.context.scene) is None:
                        return Error((BlenderAbsentFault(owner=repr(call.context), key="scene"),))
                    entry: BlenderResult[bpy.types.SceneCompositorEffect] = Option.of_optional(next(iter(self.stack.entries(call.identity, scene, self.gain.record)), None)).to_result_with(
                        lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=self.gain.record.__name__),)
                    )
                    return (
                        Result[bpy.types.SceneCompositorEffect, BlenderFaults]
                        .bind(entry, lambda item: read.entering(call.identity, item))
                        .map(lambda _: Done(notes=(("INFO", "Metering the frame entering the exposure entry"),)))
                    )

                with identity.reports.deferred(exposing.stepped) as step:
                    yield Ok((
                        Operator(
                            key=self.key,
                            title=Exposing.title,
                            description="Meters the frame entering exposure and writes absolute stops",
                            form=Empty,
                            run=metered,
                            condition=Some(Job(kind="RENDER")),
                            undo=False,
                        ),
                        read,
                        Timer(body=lambda: refresh.fired().map(lambda _: Some(interval))),
                        Overlay[Following](
                            drawing=Drawing(
                                area=AreaQuery(space="VIEW_3D", shows=Some(Setting(descriptor=shading, accepted=("MATERIAL", "RENDERED")))),
                                part=Follower(exposing=exposing, step=step),
                                refreshes=(refresh, Updated(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene,))),
                            ),
                            space=bpy.types.SpaceView3D,
                            region="WINDOW",
                            draw_type="PRE_VIEW",
                            scissor_enabled=False,
                            respect_overlays=False,
                        ),
                    ))
            case Result(error=faults):
                yield Error(faults)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["AutoExposure", "Consume", "Follow", "Gain", "Metering", "Read", "Reading"]
