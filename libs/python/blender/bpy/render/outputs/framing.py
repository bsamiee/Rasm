"""Frame size, render regions, overscan restoration, and the rendered size and duration rows on native panels."""

from fractions import Fraction
from functools import partial
from typing import Annotated, ClassVar, Final, Self

from bl_ui.properties_output import RENDER_PT_format, RENDER_PT_frame_range
import bpy
from bpy_extras.object_utils import world_to_camera_view
from expression import Error, Nothing, Ok, Option, Result, Some
from mathutils import Vector
import msgspec
import numpy as np
import OpenImageIO

from blender.bpy.commands.operators import Answer, answered, Call, Empty, Operator
from blender.bpy.data.rna.conditions import Count, Setting
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.paths import RnaPath, rooted
from blender.bpy.data.rna.restoration import Endpoints
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.events import Handler
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.scene.cameras import render_cameras, widened
from blender.bpy.settings.attachments import Attached, Attachment, Recovery, Stored
from blender.bpy.ui.layout.rows import Label
from blender.bpy.ui.surfaces.menus import Append
from blender.imaging.geometry.frames import Region
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.fields import Case, NonNegative, Positive

# --- [TYPES] ----------------------------------------------------------------------------

type Extents = Whole | Border


# --- [CONSTANTS] ------------------------------------------------------------------------

RESOLUTION_X: Final = Descriptor.declared(bpy.types.RenderSettings, ("resolution_x",), bpy.types.IntProperty)
RESOLUTION_Y: Final = Descriptor.declared(bpy.types.RenderSettings, ("resolution_y",), bpy.types.IntProperty)
SCALE: Final = Descriptor.declared(bpy.types.RenderSettings, ("resolution_percentage",), bpy.types.IntProperty)
ASPECT_X: Final = Descriptor.declared(bpy.types.RenderSettings, ("pixel_aspect_x",), bpy.types.FloatProperty)
ASPECT_Y: Final = Descriptor.declared(bpy.types.RenderSettings, ("pixel_aspect_y",), bpy.types.FloatProperty)
EDGES: Final = (("left", "border_min_x"), ("bottom", "border_min_y"), ("right", "border_max_x"), ("top", "border_max_y"))
DENSITY: Final = Descriptor.declared(bpy.types.RenderSettings, ("ppm_factor",), bpy.types.FloatProperty)
DENSITY_BASE: Final = Descriptor.declared(bpy.types.RenderSettings, ("ppm_base",), bpy.types.FloatProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Whole(Case, frozen=True, kw_only=True):
    """Whole frame grown on each side by overscan."""

    overscan: NonNegative = 0.0


class Border(Case, frozen=True, kw_only=True):
    """Render border rectangle, rendered alone."""

    region: Region = Region()
    crop: bool = False


class Framing(msgspec.Struct, frozen=True, kw_only=True):
    """Frame size, aspect, density, and render extent."""

    native: ClassVar[tuple[str, ...]] = tuple(descriptor.prop.identifier for descriptor in (RESOLUTION_X, RESOLUTION_Y, SCALE, ASPECT_X, ASPECT_Y))
    resolution_x: Annotated[int, RESOLUTION_X.meta] = RESOLUTION_X.prop.default
    resolution_y: Annotated[int, RESOLUTION_Y.meta] = RESOLUTION_Y.prop.default
    resolution_percentage: Annotated[int, SCALE.meta] = SCALE.prop.default
    pixel_aspect_x: Annotated[float, ASPECT_X.meta] = ASPECT_X.prop.default
    pixel_aspect_y: Annotated[float, ASPECT_Y.meta] = ASPECT_Y.prop.default
    density: Positive = DENSITY.prop.default / DENSITY_BASE.prop.default
    extent: Extents = Whole()

    @classmethod
    def of(cls, render: bpy.types.RenderSettings, overscan: float = 0.0) -> Self:
        """Reads native framing, its render border when on, else the whole frame grown by `overscan`."""
        extent = Border(region=Region(**{field: getattr(render, member) for field, member in EDGES}), crop=render.use_crop_to_border) if render.use_border else Whole(overscan=overscan)
        return cls(**{member: getattr(render, member) for member in cls.native}, density=render.ppm_factor / render.ppm_base, extent=extent)

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

    @property
    def bounds(self) -> tuple[tuple[int, int], OpenImageIO.ROI]:
        """Canvas the render writes and the window of it the camera frame's rendered pixels fill, rows top first, borders in Blender's float32 region arithmetic."""
        (width, height), (pad_x, pad_y) = self.size, self.padding
        match self.extent:
            case Whole():
                return (width + 2 * pad_x, height + 2 * pad_y), OpenImageIO.ROI(pad_x, pad_x + width, pad_y, pad_y + height)
            case Border(region=Region(left=left, bottom=bottom, right=right, top=top)):
                across, up = np.float32(width), np.float32(height)
                low, high = int(np.float32(bottom) * up), int(np.float32(top) * up)
                return (width, height), OpenImageIO.ROI(int(np.float32(left) * across), int(np.float32(right) * across), height - high, height - low)

    def pixels(self) -> tuple[int, int]:
        """Returns the pixels a render covers, the canvas of a whole frame and the window of a border."""
        canvas, window = self.bounds
        return (window.width, window.height) if isinstance(self.extent, Border) else canvas

    def rendered(self, bounds: tuple[int, int]) -> Outcome[tuple[Self, tuple[int, int]]]:
        """Validates pixels, folding overscan into resolution within renderer bounds."""

        def validated(pixels: tuple[int, int]) -> Outcome[tuple[Self, tuple[int, int]]]:
            if any(side < 1 for side in pixels):
                return Error(Fault.of(RecordReason.OUTSIDE, type(self).__name__, pixels, member="at least 1 pixel per side"))
            if not any(self.padding):
                return Ok((self, pixels))
            grown_x, grown_y = pixels
            low, high = bounds
            return (
                Ok((msgspec.structs.replace(self, resolution_x=grown_x, resolution_y=grown_y, resolution_percentage=100, extent=Whole()), pixels))
                if low <= grown_x <= high and low <= grown_y <= high
                else Error(Fault.of(RecordReason.OUTSIDE, type(self).__name__, pixels, member=f"{low} to {high} pixels per side with overscan"))
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
        extent: dict[RnaPath, Value]
        match self.extent:
            case Border(region=region, crop=crop):
                extent = {**{(member,): getattr(region, field) for field, member in EDGES}, ("use_crop_to_border",): crop, ("use_border",): True}
            case Whole():
                extent = {("use_border",): False}
        native: dict[RnaPath, Value] = {(member,): getattr(self, member) for member in Framing.native}
        return Snapshot.of(bpy.types.RenderSettings, {**native, ("ppm_factor",): self.density * ppm_base, **extent})


class Grown(msgspec.Struct, frozen=True, kw_only=True):
    """Original framing and native values around overscan writes."""

    base: Framing
    ends: Endpoints


class Overscan(bpy.types.PropertyGroup):
    """Overscan a whole-frame render grows by."""

    overscan: bpy.props.FloatProperty(name="Overscan", description="Share of frame side added past each edge, camera widened to keep view", min=0.0, soft_max=0.25, precision=3)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Framed(msgspec.Struct, frozen=True, kw_only=True):
    """Overscan attachment and stored pre-render state."""

    attached: Attached[Overscan]
    recovery: Recovery[Grown]

    # --- [VALUES]

    def read(self, scene: bpy.types.Scene, extent: Option[Extents] = Nothing) -> Framing:
        """Reads native framing and attached overscan, `extent` in place of the native one when given."""
        return extent.map(lambda held: msgspec.structs.replace(Framing.of(scene.render), extent=held)).default_with(lambda: Framing.of(scene.render, self.attached.struct(scene).overscan))

    def written(self, scene: bpy.types.Scene, framing: Framing) -> Outcome[None]:
        """Writes native and attached framing, keeping density unit."""
        return paired(
            framing.snapshot(scene.render.ppm_base).apply(scene.render).outcome,
            assigned(self.attached.struct(scene), {("overscan",): framing.extent.overscan}) if isinstance(framing.extent, Whole) else Ok(None),
        ).map(lambda _: None)

    # --- [RENDER_SCOPE]

    def base(self, scene: bpy.types.Scene) -> Outcome[Option[Framing]]:
        """Returns original camera framing, Nothing outside a grown render."""
        return self.recovery.stored.read(scene).map(lambda held: held.map(lambda grown: grown.base))

    def grown(self, scene: bpy.types.Scene) -> Outcome[tuple[Framing, tuple[int, int]]]:
        """Grows overscan within capacity, recording the render and camera values it writes."""

        def grow(base: Framing, held: Option[Grown]) -> Outcome[tuple[Framing, tuple[int, int]]]:
            def wrote(formed: tuple[Framing, tuple[int, int]]) -> Outcome[tuple[Framing, tuple[int, int]]]:
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

            bounded: Outcome[tuple[Framing, tuple[int, int]]] = base.rendered((RESOLUTION_X.prop.hard_min, RESOLUTION_X.prop.hard_max))
            return Result[tuple[Framing, tuple[int, int]], Faults].bind(bounded, lambda formed: capacity(scene, formed[1]).bind(lambda _: wrote(formed)))

        return self.recovery.stored.read(scene).bind(lambda held: grow(self.read(scene, Some(Whole()) if held.is_some() else Nothing), held))

    def restored(self, scene: bpy.types.Scene) -> Outcome[None]:
        """Restores values still equal to written ones, keeping later edits."""
        return self.recovery.reverted(scene).map(lambda _: None)

    # --- [REGIONS_AND_READINGS]

    def bordered(self, scene: bpy.types.Scene, region: Region) -> Outcome[Answer]:
        """Writes region with scene's existing crop preference."""
        extent = Border(region=region, crop=scene.render.use_crop_to_border)
        return answered(
            msgspec.structs.replace(self.read(scene), extent=extent).snapshot(scene.render.ppm_base).apply(scene.render), "Render region already covers area", (("INFO", "Render region set"),)
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

        return rooted(bpy.types.Scene, call.context).bind(
            lambda scene: Result[Region, Faults].bind(
                Option.of_optional(scene.camera).bind(lambda camera: projected(camera, scene)).to_result_with(partial(Fault.of, BlenderReason.UNSEEN, "selection")),
                lambda region: self.bordered(scene, region),
            )
        )

    def caption(self, context: bpy.types.Context) -> Option[str]:
        """Returns the pixels a render covers, overscan and border included, which the native size rows leave out."""
        return Option.of_optional(context.scene).map(lambda scene: "{} x {} px rendered".format(*self.read(scene).pixels()))


# --- [OPERATIONS] -----------------------------------------------------------------------


def capacity(scene: bpy.types.Scene, size: tuple[int, int]) -> Outcome[None]:
    """Accepts canvas sizes within native GPU or CPU compositor limit."""
    bound = 8192 if scene.render.compositor_device == "GPU" else 32768
    return (
        Ok(size)
        .filter_with(lambda held: max(held) <= bound, lambda held: Fault.of(RecordReason.OUTSIDE, "Compositor canvas", " x ".join(map(str, held)), member=f"at most {bound} pixels per side"))
        .map(lambda _: None)
    )


def camera_border(call: Call[Empty]) -> Outcome[Answer]:
    """Set render region to camera frame."""
    return paired(rooted(bpy.types.Scene, call.context), FrameSize().bound(call.identity)).bind(lambda pair: pair[1].bordered(pair[0], Region()))


def selection_border(call: Call[Empty]) -> Outcome[Answer]:
    """Set render region to selection bounds in camera view."""
    return FrameSize().bound(call.identity).bind(lambda framed: framed.selected(call))


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
    camera: ClassVar[Setting] = Setting(descriptor=Descriptor.declared(bpy.types.RegionView3D, ("view_perspective",), bpy.types.EnumProperty), accepted=("CAMERA",))
    camera_bordering: ClassVar[Operator[Empty]] = Operator(form=Empty, run=camera_border, condition=Some(camera))
    selection_bordering: ClassVar[Operator[Empty]] = Operator(
        form=Empty, run=selection_border, condition=Some(camera & Count(collection=Descriptor.declared(bpy.types.ViewLayer, ("objects", "selected"), bpy.types.CollectionProperty)))
    )

    def bound(self, identity: Identity) -> Outcome[Framed]:
        """Binds registered overscan and its recovery."""
        return paired(
            self.attachment.bound(identity), Stored.keyed(identity, f"{self.key}_recovery", msgspec.msgpack.Decoder(Grown)).map(lambda stored: Recovery(stored=stored, ends=lambda grown: grown.ends))
        ).map(lambda pair: Framed(attached=pair[0], recovery=pair[1]))

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns declarations with callbacks binding after registration."""
        return Ok((
            self.camera_bordering,
            self.selection_bordering,
            Append(target=RENDER_PT_format, rows=(Label(text=lambda context: self.bound(identity).map(lambda framed: framed.caption(context)).default_with(summarized).to_list()),)),
            Append(target=RENDER_PT_frame_range, rows=(Label(text=lambda context: duration(context).to_list()),)),
            Handler(handlers=bpy.app.handlers.load_post, body=lambda: self.bound(identity).bind(lambda framed: accumulated(framed.restored(scene) for scene in bpy.data.scenes).map(lambda _: None))),
        ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ASPECT_X",
    "ASPECT_Y",
    "DENSITY",
    "DENSITY_BASE",
    "EDGES",
    "RESOLUTION_X",
    "RESOLUTION_Y",
    "SCALE",
    "Border",
    "Extents",
    "FrameSize",
    "Framed",
    "Framing",
    "Grown",
    "Overscan",
    "Whole",
    "camera_border",
    "capacity",
    "duration",
    "selection_border",
    "summarized",
]
