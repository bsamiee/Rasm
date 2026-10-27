"""Frame size, render regions, overscan restoration, and safe areas with their native rows."""

from enum import auto, Enum
from fractions import Fraction
from typing import Annotated, ClassVar, Final, Self

from bl_ui.properties_output import RENDER_PT_format, RENDER_PT_frame_range
import bpy
from bpy_extras.object_utils import world_to_camera_view
from expression import Error, Nothing, Ok, Option, Result, Some
from mathutils import Vector
import msgspec
import numpy as np

from blender.bpy.commands.operators import Answer, answered, Call, Empty, Operator
from blender.bpy.data.rna.conditions import AllOf, Count, Setting
from blender.bpy.data.rna.descriptors import assigned, Descriptor
from blender.bpy.data.rna.snapshots import Endpoints, Snapshot
from blender.bpy.extension.events import Handler
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.render.outputs.formats import struct_listing
from blender.bpy.scene.cameras import render_cameras, widened
from blender.bpy.settings.attachments import Attached, Attachment, Recovery, Stored
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import Label
from blender.bpy.ui.surfaces.appends import Append
from blender.imaging.geometry.frames import Region
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.fields import Case, NonNegative, Positive, Side

# --- [TYPES] ----------------------------------------------------------------------------

type Extents = Whole | Border


class Broadcast(Enum):
    """Broadcast standard of safe area preset."""

    WIDE = auto()
    PROTECTED = auto()
    CENTER_CUT = auto()

    @property
    def safe(self) -> dict[str, tuple[float, ...]]:
        """Title and action margins at their host defaults with the standard's center-cut margins."""
        defaults = {name: defaulted(descriptor) for name, descriptor in SAFE_AREAS.items()}
        match self:
            case Broadcast.WIDE:
                return defaults | {"title_center": (0.0, 0.0), "action_center": (0.0, 0.0)}
            case Broadcast.PROTECTED:
                return defaults | {"title_center": (0.15, 0.05), "action_center": (0.1, 0.05)}
            case Broadcast.CENTER_CUT:
                return defaults


# --- [CONSTANTS] ------------------------------------------------------------------------

RESOLUTION_X: Final = Descriptor.declared(bpy.types.RenderSettings, "resolution_x", bpy.types.IntProperty)
RESOLUTION_Y: Final = Descriptor.declared(bpy.types.RenderSettings, "resolution_y", bpy.types.IntProperty)
SCALE: Final = Descriptor.declared(bpy.types.RenderSettings, "resolution_percentage", bpy.types.IntProperty)
ASPECT_X: Final = Descriptor.declared(bpy.types.RenderSettings, "pixel_aspect_x", bpy.types.FloatProperty)
ASPECT_Y: Final = Descriptor.declared(bpy.types.RenderSettings, "pixel_aspect_y", bpy.types.FloatProperty)
SAFE_AREAS: Final = {name: Descriptor.declared(bpy.types.DisplaySafeAreas, name, bpy.types.FloatProperty) for name in ("title", "action", "title_center", "action_center")}
EDGES: Final = (("left", "border_min_x"), ("bottom", "border_min_y"), ("right", "border_max_x"), ("top", "border_max_y"))
STANDARDS: Final = (
    (Broadcast.WIDE.name, "16:9", "Clear center-cut safe areas for a full 16:9 frame"),
    (Broadcast.PROTECTED.name, "14:9 in 16:9", "Set center-cut safe areas for a 14:9 frame inside 16:9"),
    (Broadcast.CENTER_CUT.name, "4:3 in 16:9", "Set center-cut safe areas for a 4:3 frame inside 16:9"),
)

# --- [MODELS] ---------------------------------------------------------------------------


class Whole(Case, frozen=True, kw_only=True):
    """Whole frame grown on each side by overscan."""

    overscan: Annotated[NonNegative, msgspec.Meta(description="Share of frame side added past each edge, camera widened to keep view")] = 0.0


class Border(Case, frozen=True, kw_only=True):
    """Render border rectangle, rendered alone."""

    region: Annotated[Region, msgspec.Meta(description="Frame rectangle from bottom-left")] = Region()
    crop: Annotated[bool, msgspec.Meta(description="Write region pixels alone")] = False


class Framing(msgspec.Struct, frozen=True, kw_only=True):
    """Frame size, aspect, density, and render extent."""

    native: ClassVar[tuple[str, ...]] = tuple(descriptor.prop.identifier for descriptor in (RESOLUTION_X, RESOLUTION_Y, SCALE, ASPECT_X, ASPECT_Y))
    resolution_x: Annotated[int, RESOLUTION_X.meta] = RESOLUTION_X.prop.default
    resolution_y: Annotated[int, RESOLUTION_Y.meta] = RESOLUTION_Y.prop.default
    resolution_percentage: Annotated[int, SCALE.meta] = SCALE.prop.default
    pixel_aspect_x: Annotated[float, ASPECT_X.meta] = ASPECT_X.prop.default
    pixel_aspect_y: Annotated[float, ASPECT_Y.meta] = ASPECT_Y.prop.default
    density: Annotated[Positive, msgspec.Meta(title="Pixel Density", description="Pixels per meter in file metadata")] = 72 / 0.0254
    extent: Annotated[Extents, msgspec.Meta(description="Whole frame with overscan, or region rendered alone")] = Whole()

    @property
    def size(self) -> tuple[int, int]:
        """Frame pixels after percentage, each axis floored."""
        return self.resolution_x * self.resolution_percentage // 100, self.resolution_y * self.resolution_percentage // 100

    @property
    def padding(self) -> tuple[int, int]:
        """Overscan pixels beyond each edge per axis."""
        width, height = self.size
        match self.extent:
            case Whole(overscan=overscan):
                fraction = Fraction(overscan)
                return round(width * fraction), round(height * fraction)
            case Border():
                return 0, 0

    def pixels(self) -> tuple[int, int]:
        """Returns covered pixels in Blender's float32 region arithmetic."""
        (width, height), (pad_x, pad_y) = self.size, self.padding
        match self.extent:
            case Whole():
                return width + 2 * pad_x, height + 2 * pad_y
            case Border(region=Region(left=left, bottom=bottom, right=right, top=top)):
                across, up = np.float32(width), np.float32(height)
                return int(np.float32(right) * across) - int(np.float32(left) * across), int(np.float32(top) * up) - int(np.float32(bottom) * up)

    def rendered(self, bounds: tuple[int, int]) -> Outcome[tuple[Self, tuple[Side, Side]]]:
        """Validates pixels, folding overscan into resolution within renderer bounds."""

        def validated(pixels: tuple[int, int]) -> Outcome[tuple[Self, tuple[Side, Side]]]:
            if any(side < 1 for side in pixels):
                return Error((Fault(reason=RecordReason.OUTSIDE, subject=type(self).__name__, member="at least 1 pixel per side", cause=str(pixels)),))
            if not any(self.padding):
                return Ok((self, pixels))
            grown_x, grown_y = pixels
            low, high = bounds
            return (
                Ok((msgspec.structs.replace(self, resolution_x=grown_x, resolution_y=grown_y, resolution_percentage=100, extent=Whole()), pixels))
                if low <= grown_x <= high and low <= grown_y <= high
                else Error((Fault(reason=RecordReason.OUTSIDE, subject=type(self).__name__, member=f"{low} to {high} pixels per side with overscan", cause=str(pixels)),))
            )

        return validated(self.pixels())

    def growth(self, camera: bpy.types.Camera) -> Option[Fraction]:
        """Returns exact growth of the camera's fit axis, Nothing for zero-pixel axis."""
        (width, height), (pad_x, pad_y) = self.size, self.padding
        horizontal = camera.sensor_fit == "HORIZONTAL" or (camera.sensor_fit == "AUTO" and width * Fraction(self.pixel_aspect_x) >= height * Fraction(self.pixel_aspect_y))
        pad, side = (pad_x, width) if horizontal else (pad_y, height)
        return Some(Fraction(side + 2 * pad, side)) if side else Nothing

    def snapshot(self, ppm_base: float) -> Snapshot:
        """Returns native framing settings keeping user's density unit."""
        match self.extent:
            case Border(region=region, crop=crop):
                extent = {member: getattr(region, field) for field, member in EDGES} | {"use_crop_to_border": crop, "use_border": True}
            case Whole():
                extent = {"use_border": False}
        return Snapshot.of(bpy.types.RenderSettings, {member: getattr(self, member) for member in Framing.native} | {"ppm_factor": self.density * ppm_base} | extent)


class Grown(msgspec.Struct, frozen=True, kw_only=True):
    """Original framing and native values around overscan writes."""

    base: Framing
    ends: Endpoints


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


class Overscan(bpy.types.PropertyGroup):
    """Overscan a whole-frame render grows by."""

    overscan: bpy.props.FloatProperty(name="Overscan", description="Share of frame side added past each edge, camera widened to keep view", min=0.0, soft_max=0.25, precision=3)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Standard(bpy.types.PropertyGroup):
    """Safe-area action's broadcast standard."""

    standard: bpy.props.EnumProperty(name="Standard", description="Standard setting safe areas", items=STANDARDS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Framed(msgspec.Struct, frozen=True, kw_only=True):
    """Overscan attachment and stored pre-render state."""

    attached: Attached[Overscan]
    recovery: Recovery[Grown]

    # --- [VALUES]

    def read(self, scene: bpy.types.Scene, extent: Option[Extents] = Nothing) -> Outcome[Framing]:
        """Reads native framing and attached overscan."""
        render = scene.render
        area: Outcome[Extents] = extent.map(Ok).default_with(
            lambda: (
                Ok(Border(region=Region(**{field: getattr(render, member) for field, member in EDGES}), crop=render.use_crop_to_border))
                if render.use_border
                else Ok(Whole(overscan=self.attached.struct(scene).overscan))
            )
        )
        return area.map(lambda held: Framing(**{member: getattr(render, member) for member in Framing.native}, density=render.ppm_factor / render.ppm_base, extent=held))

    def written(self, scene: bpy.types.Scene, framing: Framing) -> Outcome[None]:
        """Writes native and attached framing, keeping density unit."""
        return paired(
            framing.snapshot(scene.render.ppm_base).apply(scene.render).outcome,
            assigned(self.attached.struct(scene), {"overscan": framing.extent.overscan}) if isinstance(framing.extent, Whole) else Ok(None),
        ).map(lambda _: None)

    # --- [RENDER_SCOPE]

    def base(self, scene: bpy.types.Scene) -> Outcome[Option[Framing]]:
        """Returns original camera framing, Nothing outside a grown render."""
        return self.recovery.stored.read(scene).map(lambda held: held.map(lambda grown: grown.base))

    def grown(self, scene: bpy.types.Scene) -> Outcome[tuple[Framing, tuple[Side, Side]]]:
        """Grows overscan within capacity, recording the render and camera values it writes."""

        def grow(base: Framing, held: Option[Grown]) -> Outcome[tuple[Framing, tuple[Side, Side]]]:
            def wrote(formed: tuple[Framing, tuple[Side, Side]]) -> Outcome[tuple[Framing, tuple[Side, Side]]]:
                bigger, covered = formed
                result = (bigger, bigger.size if isinstance(bigger.extent, Border) and not bigger.extent.crop else covered)
                if held.is_some() or not any(base.padding):
                    return capacity(scene, result[1]).map(lambda _: result)
                writes = (
                    (scene.render, bigger.snapshot(scene.render.ppm_base)),
                    *(
                        (camera, snapshot)
                        for camera in dict.fromkeys(render_cameras(scene).values())
                        for factor in base.growth(camera).to_list()
                        for snapshot in widened(camera, float(factor)).to_list()
                    ),
                )
                return self.recovery.recorded(scene, writes, lambda ends: Grown(base=base, ends=ends)).map(lambda _: result)

            bounded: Outcome[tuple[Framing, tuple[Side, Side]]] = base.rendered((RESOLUTION_X.prop.hard_min, RESOLUTION_X.prop.hard_max))
            return Result[tuple[Framing, tuple[Side, Side]], Faults].bind(bounded, lambda formed: capacity(scene, formed[1]).bind(lambda _: wrote(formed)))

        return self.recovery.stored.read(scene).bind(lambda held: self.read(scene, Some(Whole()) if held.is_some() else Nothing).bind(lambda base: grow(base, held)))

    def restored(self, scene: bpy.types.Scene) -> Outcome[None]:
        """Restores values still equal to written ones, keeping later edits."""
        return self.recovery.reverted(scene).map(lambda _: None)

    # --- [REGIONS_AND_READINGS]

    def bordered(self, scene: bpy.types.Scene, region: Region) -> Outcome[Answer]:
        """Writes region with scene's existing crop preference."""
        extent = Border(region=region, crop=scene.render.use_crop_to_border)
        return self.read(scene).bind(
            lambda framing: answered(
                msgspec.structs.replace(framing, extent=extent).snapshot(scene.render.ppm_base).apply(scene.render), "Render region already covers area", (("INFO", "Render region set"),)
            )
        )

    def selected(self, call: Call[Empty]) -> Outcome[Answer]:
        """Projects selected bounds into camera frame."""

        def projected(camera: bpy.types.Object, scene: bpy.types.Scene) -> Option[Region]:
            points = tuple(
                point.to_tuple()
                for item in call.context.selected_objects
                for corner in item.bound_box
                for point in (world_to_camera_view(scene, camera, item.matrix_world @ Vector(corner)),)
                if point.z > 0
            )
            return Some(Region.bounding(np.clip(np.asarray(points), 0.0, 1.0))) if points else Nothing

        return call.root(bpy.types.Scene).bind(
            lambda scene: Result[Region, Faults].bind(
                Option.of_optional(scene.camera).bind(lambda camera: projected(camera, scene)).to_result_with(lambda: (Fault(reason=BlenderReason.UNMET, subject=Unseen().message),)),
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
                *(("cropped",) if isinstance(framing.extent, Border) and framing.extent.crop else ()),
            ))

        return Option.of_optional(context.scene).bind(lambda scene: self.read(scene).map(lambda framing: text(framing, framing.pixels())).map(Some).default_with(summarized))


# --- [OPERATIONS] -----------------------------------------------------------------------


def defaulted(descriptor: Descriptor[bpy.types.FloatProperty]) -> tuple[float, ...]:
    """Returns a float array member's RNA default."""
    held = [descriptor.prop.default] * descriptor.prop.array_length
    descriptor.prop.default_array.foreach_get(held)
    return tuple(held)


def capacity(scene: bpy.types.Scene, size: tuple[Side, Side]) -> Outcome[None]:
    """Accepts canvas sizes within native GPU or CPU compositor limit."""
    width, height = size
    bound = 8192 if scene.render.compositor_device == "GPU" else 32768
    return Error((Fault(reason=BlenderReason.UNMET, subject=Oversize(width=width, height=height, bound=bound).message),)) if max(size) > bound else Ok(None)


def camera_border(call: Call[Empty]) -> Outcome[Answer]:
    """Set render region to camera frame."""
    return paired(call.root(bpy.types.Scene), FrameSize().bound(call.identity)).bind(lambda pair: pair[1].bordered(pair[0], Region()))


def selection_border(call: Call[Empty]) -> Outcome[Answer]:
    """Set render region to selection bounds in camera view."""
    return FrameSize().bound(call.identity).bind(lambda framed: framed.selected(call))


def broadcast_safe_areas(call: Call[Standard]) -> Outcome[Answer]:
    """Set safe areas for a broadcast standard."""
    standard = Broadcast[call.properties.standard]
    label = bpy.types.UILayout.enum_item_name(call.properties, "standard", standard.name)
    return call.root(bpy.types.Scene).bind(
        lambda scene: answered(Snapshot.of(bpy.types.DisplaySafeAreas, standard.safe).apply(scene.safe_areas), f"Safe areas already hold {label}", (("INFO", f"Safe areas set for {label}"),))
    )


def summarized(faults: Faults) -> Option[str]:
    """Joins rendered fault reports for a label."""
    return Some(". ".join(fault.message for fault in faults))


def duration(context: bpy.types.Context) -> Option[str]:
    """Returns total and preview inclusive-frame durations as timecode."""

    def spans(scene: bpy.types.Scene) -> str:
        render = scene.render
        held = (("Total", scene.frame_start, scene.frame_end), *((("Preview", scene.frame_preview_start, scene.frame_preview_end),) if scene.use_preview_range else ()))
        return ", ".join(f"{label} {bpy.utils.smpte_from_frame(end - start + 1, fps=render.fps, fps_base=render.fps_base)}, {end - start + 1} frames" for label, start, end in held)

    return Option.of_optional(context.scene).map(spans)


# --- [COMPOSITION] ----------------------------------------------------------------------


class FrameSize(msgspec.Struct, frozen=True, kw_only=True):
    """Framing attachment, border actions, native-panel readings, and load restoration."""

    key: ClassVar[Key] = "framing"
    attachment: ClassVar[Attachment[Overscan]] = Attachment(key=key, form=Overscan, owner=bpy.types.Scene)
    prerequisites: ClassVar[tuple[Registrable, ...]] = (attachment,)
    camera: ClassVar[Setting] = Setting(descriptor=Descriptor.declared(bpy.types.RegionView3D, "view_perspective", bpy.types.EnumProperty), accepted=("CAMERA",))
    camera_bordering: ClassVar[Operator[Empty]] = Operator(form=Empty, run=camera_border, condition=Some(camera))
    selection_bordering: ClassVar[Operator[Empty]] = Operator(
        form=Empty, run=selection_border, condition=Some(AllOf(conditions=(camera, Count(collection=Descriptor.declared(bpy.types.ViewLayer, "objects.selected", bpy.types.CollectionProperty)))))
    )

    def bound(self, identity: Identity) -> Outcome[Framed]:
        """Binds registered overscan and its recovery."""
        return paired(
            self.attachment.bound(identity),
            identity.identifier(f"{self.key}_recovery").map(lambda name: Recovery(stored=Stored(name=name, decoder=msgspec.msgpack.Decoder(Grown)), ends=lambda grown: grown.ends)),
        ).map(lambda pair: Framed(attached=pair[0], recovery=pair[1]))

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns declarations with callbacks binding after registration."""
        return Ok((
            self.camera_bordering,
            self.selection_bordering,
            safe_broadcast,
            Append(target=RENDER_PT_format, rows=(Label(text=lambda context: self.bound(identity).map(lambda framed: framed.caption(context)).default_with(summarized)),)),
            Append(target=RENDER_PT_frame_range, rows=(Label(text=duration),)),
            Handler(handlers=bpy.app.handlers.load_post, body=lambda: self.bound(identity).bind(lambda framed: accumulated(framed.restored(scene) for scene in bpy.data.scenes).map(lambda _: None))),
        ))


safe_broadcast = Operator(form=Standard, run=broadcast_safe_areas)
safe_presets = Preset(key="safe_areas", form=Snapshot)
safe_listing = struct_listing(safe_presets, lambda scene: scene.safe_areas, "Safe areas already hold preset")

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ASPECT_X",
    "ASPECT_Y",
    "EDGES",
    "RESOLUTION_X",
    "RESOLUTION_Y",
    "SAFE_AREAS",
    "SCALE",
    "STANDARDS",
    "Border",
    "Broadcast",
    "Extents",
    "FrameSize",
    "Framed",
    "Framing",
    "Grown",
    "Overscan",
    "Oversize",
    "Standard",
    "Unseen",
    "Whole",
    "broadcast_safe_areas",
    "camera_border",
    "capacity",
    "defaulted",
    "duration",
    "safe_broadcast",
    "safe_listing",
    "safe_presets",
    "selection_border",
    "summarized",
]
