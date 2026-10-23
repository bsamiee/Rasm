"""Native frame size, render regions, overscan restoration, and safe-area rows."""

from typing import Annotated, ClassVar

from bl_ui.properties_output import RENDER_PT_format, RENDER_PT_frame_range
import bpy
from bpy_extras.object_utils import world_to_camera_view
from expression import Error, Nothing, Ok, Option, Result, Some
from mathutils import Vector
import msgspec

from blender.bpy.commands.operators import answered, Call, Empty, Operator, Outcome
from blender.bpy.data.rna.conditions import AllOf, Count, Setting
from blender.bpy.data.rna.descriptors import Descriptor, rooted
from blender.bpy.data.rna.snapshots import Persisted, Snapshot
from blender.bpy.extension.events.handlers import PathHandler
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderRenderingFault, BlenderResult, BlenderUnmetFault
from blender.bpy.scene.cameras import render_cameras, widened
from blender.bpy.settings.attachments import Attached, Attachment, Stored
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import Label
from blender.bpy.ui.layout.sections import Listing
from blender.bpy.ui.surfaces.appends import Append
from blender.imaging.faults import accumulated, paired
from blender.rendering.edits import transfer, Tree
from blender.rendering.outputs.framing import Broadcast, Extents, Fit, Framing, Region, SafeAreas, Side, Whole

# --- [MODELS] ---------------------------------------------------------------------------


class Growing(msgspec.Struct, frozen=True, kw_only=True, tag=1):
    """Original framing and native values before overscan writes."""

    base: Framing
    before: Persisted


class Grown(Growing, frozen=True, kw_only=True, tag=2):
    """Original framing and native values around completed overscan writes."""

    after: Persisted


class Oversize(msgspec.Struct, frozen=True, kw_only=True):
    """Frame past compositor capacity."""

    width: int
    height: int
    bound: int

    @property
    def message(self) -> str:
        """Capacity and requested dimensions."""
        return f"Requires at most {self.bound} pixels per side, got {self.width} x {self.height}"


class Unseen(msgspec.Struct, frozen=True, kw_only=True):
    """Selection behind scene camera."""

    @property
    def message(self) -> str:
        """Selection requirement."""
        return "Requires selection in front of scene camera"


class Standard(msgspec.Struct, frozen=True, kw_only=True):
    """Safe-area action's broadcast standard."""

    standard: Annotated[Broadcast, msgspec.Meta(description="Standard setting safe areas")] = Broadcast.WIDE


class Framed(msgspec.Struct, frozen=True, kw_only=True):
    """Overscan attachment and stored pre-render state."""

    attached: Attached[Whole]
    held: Stored[Growing | Grown]
    resolution: Descriptor[bpy.types.IntProperty]

    # --- [VALUES]

    def read(self, scene: bpy.types.Scene, extent: Option[Extents] = Nothing) -> BlenderResult[Framing]:
        """Reads native framing and attached overscan."""
        render = scene.render
        area: BlenderResult[Extents] = extent.map(Ok).default_with(
            lambda: Ok(Region(**{field: getattr(render, member) for field, member in FrameSize.edges}, crop=render.use_crop_to_border)) if render.use_border else self.attached.read(scene)
        )
        return area.map(lambda held: Framing(**{member: getattr(render, member) for member in FrameSize.members}, density=render.ppm_factor / render.ppm_base, extent=held))

    def written(self, scene: bpy.types.Scene, framing: Framing) -> BlenderResult[None]:
        """Writes native and attached framing, keeping density unit."""
        return paired(settings(framing, scene.render.ppm_base).apply(scene.render).outcome, self.attached.write(scene, framing.extent) if isinstance(framing.extent, Whole) else Ok(None)).map(
            lambda _: None
        )

    # --- [RENDER_SCOPE]

    def base(self, scene: bpy.types.Scene) -> BlenderResult[Option[Framing]]:
        """Returns original camera framing, Nothing outside a grown render."""
        return self.held.read(scene).map(lambda held: held.map(lambda state: state.base))

    def grown(self, scene: bpy.types.Scene) -> BlenderResult[tuple[Framing, tuple[Side, Side]]]:
        """Grows overscan within capacity, storing native restore state."""

        def grow(base: Framing, held: Option[Growing | Grown]) -> BlenderResult[tuple[Framing, tuple[Side, Side]]]:
            bounds = self.resolution.prop

            def wrote(formed: tuple[Framing, tuple[Side, Side]]) -> BlenderResult[tuple[Framing, tuple[Side, Side]]]:
                bigger, covered = formed
                stored = bigger.size if isinstance(bigger.extent, Region) and not bigger.extent.crop else covered
                result = (bigger, stored)
                match held:
                    case Option(tag="some", some=Grown()):
                        return capacity(scene, stored).map(lambda _: result)
                    case Option(tag="some", some=Growing()):
                        return Error((BlenderAbsentFault(owner=repr(scene), key=f"{self.held.name}.after"),))
                    case Option():
                        if not any(base.padding):
                            return capacity(scene, stored).map(lambda _: result)
                cameras = tuple(
                    (camera, snapshot)
                    for camera in dict.fromkeys(render_cameras(scene).values())
                    for factor in base.growth(Fit[camera.sensor_fit]).to_list()
                    for snapshot in widened(camera, float(factor)).to_list()
                )
                owners = (scene.render, *(camera for camera, _ in cameras))

                def changed(before: Persisted) -> BlenderResult[tuple[Framing, tuple[Side, Side]]]:
                    outcomes = (settings(bigger, scene.render.ppm_base).apply(scene.render), *(snapshot.apply(camera) for camera, snapshot in cameras))
                    final = tuple(Snapshot.capture(owner, native=True) for owner in owners)
                    trees = tuple(tree for answer in final for tree in answer.to_option().to_list())
                    after = Tree.joined(trees)
                    completed = Snapshot.persisted(after).bind(lambda projected: self.held.write(scene, Grown(base=base, before=before, after=projected)))
                    return paired(accumulated(outcome.outcome for outcome in outcomes), paired(accumulated(final), completed)).map(lambda _: result)

                return (
                    accumulated(Snapshot.capture(owner, native=True) for owner in owners)
                    .map(Tree.joined)
                    .bind(Snapshot.persisted)
                    .bind(lambda before: self.held.write(scene, Growing(base=base, before=before)).bind(lambda _: changed(before)))
                )

            bounded: BlenderResult[tuple[Framing, tuple[Side, Side]]] = base.rendered((bounds.hard_min, bounds.hard_max)).map_error(BlenderRenderingFault.lifted)
            return Result[tuple[Framing, tuple[Side, Side]], BlenderFaults].bind(bounded, lambda formed: capacity(scene, formed[1]).bind(lambda _: wrote(formed)))

        return self.held.read(scene).bind(lambda held: self.read(scene, Some(Whole()) if held.is_some() else Nothing).bind(lambda base: grow(base, held)))

    def restored(self, scene: bpy.types.Scene) -> BlenderResult[None]:
        """Restores values still equal to written ones, keeping later edits."""

        def back(kept: Growing | Grown) -> BlenderResult[None]:
            match kept:
                case Grown():
                    return Snapshot.reverted(kept.before, kept.after).bind(lambda _: self.held.removed(scene)).map(lambda _: None)
                case Growing():
                    return Error((BlenderAbsentFault(owner=repr(scene), key=f"{self.held.name}.after"),))

        return self.held.read(scene).bind(lambda held: held.map(back).default_value(Ok(None)))

    # --- [REGIONS_AND_READINGS]

    def bordered(self, scene: bpy.types.Scene, region: Region) -> BlenderResult[Outcome]:
        """Writes region with scene's existing crop preference."""
        extent = msgspec.structs.replace(region, crop=scene.render.use_crop_to_border)
        return self.read(scene).bind(
            lambda framing: answered(
                settings(msgspec.structs.replace(framing, extent=extent), scene.render.ppm_base).apply(scene.render), "Render region already covers area", (("INFO", "Render region set"),)
            )
        )

    def selected(self, call: Call[Empty]) -> BlenderResult[Outcome]:
        """Projects selected bounds into camera frame."""

        def projected(camera: bpy.types.Object, scene: bpy.types.Scene) -> Option[Region]:
            points = tuple(
                point for item in call.context.selected_objects for corner in item.bound_box for point in (world_to_camera_view(scene, camera, item.matrix_world @ Vector(corner)),) if point.z > 0
            )
            return (
                Some(
                    Region(
                        left=min(1.0, max(0.0, min(point.x for point in points))),
                        bottom=min(1.0, max(0.0, min(point.y for point in points))),
                        right=min(1.0, max(0.0, *(point.x for point in points))),
                        top=min(1.0, max(0.0, *(point.y for point in points))),
                    )
                )
                if points
                else Nothing
            )

        return call.root(bpy.types.Scene).bind(
            lambda scene: Result[Region, BlenderFaults].bind(
                Option.of_optional(scene.camera).bind(lambda camera: projected(camera, scene)).to_result_with(lambda: (BlenderUnmetFault(requirement=Unseen()),)),
                lambda region: self.bordered(scene, region),
            )
        )

    def caption(self, context: bpy.types.Context) -> Option[str]:
        """Returns native size, covered pixels, pixel aspect, and cropped-region readings."""

        def text(framing: Framing, shown: tuple[int, int]) -> str:
            width, height = framing.size
            shown_width, shown_height = shown
            aspect_x, aspect_y = framing.pixel_aspect_x, framing.pixel_aspect_y
            return ", ".join((
                f"{width} x {height} px",
                *((f"{shown_width} x {shown_height} rendered",) if shown != (width, height) else ()),
                *((f"pixel aspect {aspect_x:g}:{aspect_y:g}",) if aspect_x != aspect_y else ()),
                *(("cropped",) if isinstance(framing.extent, Region) and framing.extent.crop else ()),
            ))

        if (scene := (context.scene)) is None:
            return Nothing
        return self.read(scene).bind(lambda framing: framing.pixels().map_error(BlenderRenderingFault.lifted).map(lambda shown: text(framing, shown))).map(Some).default_with(summarized)


class FrameSize(msgspec.Struct, frozen=True, kw_only=True):
    """Framing attachment, border actions, native-panel readings, and load restoration."""

    key: ClassVar[Key] = "framing"
    camera_border: ClassVar[Key] = "camera_border"
    selection_border: ClassVar[Key] = "selection_border"
    members: ClassVar[tuple[str, ...]] = tuple(field.name for field in msgspec.structs.fields(Framing) if field.name not in {"density", "extent"})
    edges: ClassVar[tuple[tuple[str, str], ...]] = (("left", "border_min_x"), ("bottom", "border_min_y"), ("right", "border_max_x"), ("top", "border_max_y"))
    attachment: ClassVar[Attachment[Whole]] = Attachment(key=key, form=Whole, owner=bpy.types.Scene)

    @property
    def page_rows(self) -> tuple[Attachment[Whole]]:
        """Attachment declaration for preferences page."""
        return (self.attachment,)

    def bound(self, identity: Identity) -> BlenderResult[Framed]:
        """Binds registered overscan and its stored-state name."""
        return paired(
            paired(self.attachment.bound(identity), identity.name(Name.ATTACHMENT, f"{self.key}_grown").map(lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(Growing | Grown)))),
            Descriptor.resolve(bpy.types.RenderSettings, "resolution_x", bpy.types.IntProperty),
        ).map(lambda pair: Framed(attached=pair[0][0], held=pair[0][1], resolution=pair[1]))

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns declarations with callbacks binding after registration."""
        camera = Setting(descriptor=Descriptor(root=bpy.types.RegionView3D, owner="", prop=bpy.types.RegionView3D.bl_rna.properties["view_perspective"]), accepted=("CAMERA",))
        return Descriptor.resolve(bpy.types.ViewLayer, "objects.selected", bpy.types.CollectionProperty).map(
            lambda selection: (
                self.attachment,
                Operator(
                    key=self.camera_border,
                    title="Render Region from Camera",
                    description="Sets render region to camera frame",
                    form=Empty,
                    run=lambda call: Result[bpy.types.Scene, BlenderFaults].bind(
                        call.root(bpy.types.Scene), lambda scene: self.bound(call.identity).bind(lambda framed: framed.bordered(scene, Region()))
                    ),
                    condition=Some(camera),
                ),
                Operator(
                    key=self.selection_border,
                    title="Render Region from Selection",
                    description="Sets render region to selection bounds in camera view",
                    form=Empty,
                    run=lambda call: self.bound(call.identity).bind(lambda framed: framed.selected(call)),
                    condition=Some(AllOf(conditions=(camera, Count(collection=selection)))),
                ),
                safe_broadcast,
                Append(
                    target=RENDER_PT_format,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                    rows=(Label(text=lambda context: self.bound(identity).map(lambda framed: framed.caption(context)).default_with(summarized)),),
                ),
                Append(target=RENDER_PT_frame_range, rows=(Label(text=duration),)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                PathHandler(
                    handlers=bpy.app.handlers.load_post,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                    body=lambda _: self.bound(identity).bind(lambda framed: accumulated(framed.restored(scene) for scene in bpy.data.scenes).map(lambda _: None)),
                ),
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def capacity(scene: bpy.types.Scene, size: tuple[Side, Side]) -> BlenderResult[None]:
    """Accepts canvas sizes within native GPU or CPU compositor limit."""
    width, height = size
    bound = 8192 if scene.render.compositor_device == "GPU" else 32768
    return Error((BlenderUnmetFault(requirement=Oversize(width=width, height=height, bound=bound)),)) if max(size) > bound else Ok(None)


def settings(framing: Framing, ppm_base: float) -> Snapshot:
    """Returns framing settings keeping user's density unit."""
    match framing.extent:
        case Region() as region:
            extent = {member: getattr(region, field) for field, member in FrameSize.edges} | {"use_crop_to_border": region.crop, "use_border": True}
        case Whole():
            extent = {"use_border": False}
    return Snapshot.of(bpy.types.RenderSettings, {member: getattr(framing, member) for member in FrameSize.members} | {"ppm_factor": framing.density * ppm_base} | extent)


def broadcasted(call: Call[Standard]) -> BlenderResult[Outcome]:
    """Writes selected broadcast margins."""
    label = call.record.standard.label
    return call.root(bpy.types.Scene).bind(
        lambda scene: answered(
            Snapshot.of(bpy.types.DisplaySafeAreas, msgspec.structs.asdict(SafeAreas.broadcast(call.record.standard))).apply(scene.safe_areas),
            f"Safe areas already hold {label}",
            (("INFO", f"Safe areas set for {label}"),),
        )
    )


def summarized(faults: BlenderFaults) -> Option[str]:
    """Joins rendered fault reports for a label."""
    return Some(". ".join(message for _, message in rendered(faults)))


def duration(context: bpy.types.Context) -> Option[str]:
    """Returns total and preview inclusive-frame durations as timecode."""
    if (scene := (context.scene)) is None:
        return Nothing
    render = scene.render
    spans = (("Total", scene.frame_start, scene.frame_end), *(((("Preview", scene.frame_preview_start, scene.frame_preview_end),)) if scene.use_preview_range else ()))
    return Some(", ".join(f"{label} {bpy.utils.smpte_from_frame(end - start + 1, fps=render.fps, fps_base=render.fps_base)}, {end - start + 1} frames" for label, start, end in spans))


# --- [COMPOSITION] ----------------------------------------------------------------------


safe_broadcast = Operator(key="safe_broadcast", title="Broadcast Safe Areas", description="Sets safe areas for a broadcast standard", form=Standard, run=broadcasted)
safe_presets = Preset(key="safe_areas", form=Snapshot)
safe_listing = Listing(
    preset=safe_presets,
    current=lambda context: rooted(bpy.types.Scene, context).bind(lambda scene: Snapshot.capture(scene.safe_areas)),
    matches=lambda context, values: (
        rooted(bpy.types.Scene, context).bind(lambda scene: Snapshot.capture(scene.safe_areas)).map(lambda held: tuple(not transfer(held.tree, value.tree) for value in values))
    ),
    applied=lambda context, value: rooted(bpy.types.Scene, context).bind(lambda scene: answered(value.apply(scene.safe_areas), "Safe areas already hold preset")),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "FrameSize",
    "Framed",
    "Growing",
    "Grown",
    "Oversize",
    "Standard",
    "Unseen",
    "broadcasted",
    "capacity",
    "duration",
    "safe_broadcast",
    "safe_listing",
    "safe_presets",
    "settings",
    "summarized",
]
