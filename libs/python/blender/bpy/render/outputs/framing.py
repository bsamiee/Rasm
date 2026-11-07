"""Frame size, render regions, overscan restoration, and the rendered size and duration rows on native panels."""

from fractions import Fraction
from functools import partial
import math
from typing import ClassVar, Final, Self

from bl_ui.properties_output import RENDER_PT_format, RENDER_PT_frame_range
import bpy
from bpy_extras.object_utils import world_to_camera_view
from expression import Option, Result, Some
from mathutils import Vector
import msgspec
import numpy as np
import OpenImageIO

from blender.bpy.commands.operators import Answer, answered, Call, Empty, Operator
from blender.bpy.data.rna.conditions import Count, Setting
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.paths import RnaPath, rooted
from blender.bpy.data.rna.restoration import BeforeAfter
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Handler, Registrable
from blender.bpy.scene.cameras import render_cameras, widened
from blender.bpy.settings.attachments import Attached, Attachment, Recovery, Stored
from blender.bpy.ui.elements.menus import Append
from blender.bpy.ui.layout.rows import Label
from blender.imaging.geometry.frames import Raster, Region
from blender.records.faults import accumulated, Error, Fault, Faults, Nothing, Ok, Outcome, paired, Reason
from blender.records.fields import Case, NonNegative

# --- [TYPES] ----------------------------------------------------------------------------

type Extents = Whole | Border

# --- [CONSTANTS] ------------------------------------------------------------------------

_PIXELS: Final[bpy.types.IntProperty] = bpy.types.RenderSettings.bl_rna.properties["resolution_x"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_SELECTED: Final[bpy.types.CollectionProperty] = bpy.types.LayerObjects.bl_rna.properties["selected"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
RESOLUTION: Final = Descriptor(root=bpy.types.RenderSettings, prop=_PIXELS)
_EDGES: Final = (("left", "border_min_x"), ("bottom", "border_min_y"), ("right", "border_max_x"), ("top", "border_max_y"))
_CAMERA_VIEW: Final = Setting(descriptor=Descriptor(root=bpy.types.RegionView3D, prop=bpy.types.RegionView3D.bl_rna.properties["view_perspective"]), accepted=("CAMERA",))

# --- [MODELS] ---------------------------------------------------------------------------


class Whole(Case, frozen=True, kw_only=True):
    """Whole frame grown on each side by overscan."""

    overscan: NonNegative = 0.0


class Border(Case, frozen=True, kw_only=True):
    """Render border rectangle, rendered alone."""

    region: Region = Region()
    crop: bool = False


class Framing(msgspec.Struct, frozen=True, kw_only=True):
    """Frame size, aspect, density, and render extent, each size member named as the host names it."""

    native: ClassVar[tuple[str, ...]] = ("resolution_x", "resolution_y", "resolution_percentage", "pixel_aspect_x", "pixel_aspect_y")
    resolution_x: int
    resolution_y: int
    resolution_percentage: int
    pixel_aspect_x: float
    pixel_aspect_y: float
    density: float
    extent: Extents = Whole()

    @classmethod
    def of(cls, render: bpy.types.RenderSettings, overscan: float = 0.0) -> Self:
        """Reads native framing, its render border when on, else the whole frame grown by `overscan`."""
        extent = Border(region=Region(**{field: getattr(render, member) for field, member in _EDGES}), crop=render.use_crop_to_border) if render.use_border else Whole(overscan=overscan)
        return cls(**{member: getattr(render, member) for member in cls.native}, density=render.ppm_factor / render.ppm_base, extent=extent)

    @property
    def size(self) -> tuple[int, int]:
        """Frame pixels after percentage, each axis floored."""
        return self.resolution_x * self.resolution_percentage // 100, self.resolution_y * self.resolution_percentage // 100

    @property
    def pixel_aspect(self) -> float:
        """Displayed width over height of one pixel."""
        return self.pixel_aspect_x / self.pixel_aspect_y

    @property
    def aspect(self) -> Option[float]:
        """Displayed width over height after native percentage rounding, Nothing for a zero-pixel axis."""
        width, height = self.size
        return Some(width * self.pixel_aspect / height) if width > 0 and height > 0 else Nothing

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
            case Border(region=region):
                across, up = np.float32(width), np.float32(height)
                low, high = int(np.float32(region.bottom) * up), int(np.float32(region.top) * up)
                return (width, height), OpenImageIO.ROI(int(np.float32(region.left) * across), int(np.float32(region.right) * across), height - high, height - low)

    def pixels(self) -> tuple[int, int]:
        """Returns the pixels a render covers, the canvas of a whole frame and the window of a border."""
        canvas, window = self.bounds
        return (window.width, window.height) if isinstance(self.extent, Border) else canvas

    def rasters(self, camera: Self) -> tuple[Raster, Raster]:
        """Builds data and original camera rasters from positive native dimensions and pixel windows."""
        (width, height), (camera_width, camera_height) = self.size, camera.size
        aspect, camera_aspect, (canvas, window) = self.pixel_aspect, camera.pixel_aspect, self.bounds
        greater = max(camera_width * camera_aspect, camera_height)
        offset_x, offset_y = (camera_width * camera_aspect - width * aspect) / 2, (camera_height - height) / 2
        original = Raster(bounds=Region(right=camera_width * camera_aspect / greater, top=camera_height / greater), shape=(camera_height, camera_width))

        def data(bounds: tuple[int, int, int, int], shape: tuple[int, int]) -> Raster:
            left, bottom, right, top = bounds
            return Raster(
                bounds=Region(left=(left * aspect + offset_x) / greater, bottom=(bottom + offset_y) / greater, right=(right * aspect + offset_x) / greater, top=(top + offset_y) / greater), shape=shape
            )

        return (
            data((-window.xbegin, window.yend - canvas[1], canvas[0] - window.xbegin, window.yend), (canvas[1], canvas[0]))
            if isinstance(self.extent, Whole)
            else data((window.xbegin, height - window.yend, window.xend, height - window.ybegin), (window.height, window.width))
            if self.extent.crop
            else data((0, 0, width, height), (height, width)),
            original,
        )

    def scaled(self, ratio: Fraction) -> Self:
        """Returns the framing at full percentage with each frame side scaled by `ratio`, rounded up to a whole pixel."""
        width, height = self.size
        return msgspec.structs.replace(self, resolution_x=math.ceil(width * ratio), resolution_y=math.ceil(height * ratio), resolution_percentage=100)

    def rendered(self) -> Outcome[tuple[Self, tuple[int, int]]]:
        """Validates pixels, folding overscan into resolution within the host's resolution bounds."""
        pixels, low, high = self.pixels(), RESOLUTION.prop.hard_min, RESOLUTION.prop.hard_max
        if any(side < 1 for side in pixels):
            return Error(Fault.of(Reason.OUTSIDE, type(self).__name__, pixels, member="at least 1 pixel per side"))
        if not any(self.padding):
            return Ok((self, pixels))
        return (
            Ok((msgspec.structs.replace(self, resolution_x=pixels[0], resolution_y=pixels[1], resolution_percentage=100, extent=Whole()), pixels))
            if all(low <= side <= high for side in pixels)
            else Error(Fault.of(Reason.OUTSIDE, type(self).__name__, pixels, member=f"{low} to {high} pixels per side with overscan"))
        )

    def growth(self, camera: bpy.types.Camera) -> Option[Fraction]:
        """Returns exact native view-factor growth, Nothing for a zero-pixel fit axis."""
        (width, height), (pad_x, pad_y) = self.size, self.padding
        aspect = Fraction(self.pixel_aspect_x) / Fraction(self.pixel_aspect_y)

        def fitted(across: int, up: int) -> Fraction:
            horizontal, vertical = Fraction(across), Fraction(up) / aspect
            return horizontal if camera.sensor_fit == "HORIZONTAL" else vertical if camera.sensor_fit == "VERTICAL" else max(horizontal, vertical)

        base = fitted(width, height)
        return Some(fitted(width + 2 * pad_x, height + 2 * pad_y) / base) if base else Nothing

    def snapshot(self, ppm_base: float) -> Snapshot:
        """Returns native framing settings keeping user's density unit."""
        extent: dict[RnaPath, Value]
        match self.extent:
            case Border(region=region, crop=crop):
                extent = {**{(member,): getattr(region, field) for field, member in _EDGES}, ("use_crop_to_border",): crop, ("use_border",): True}
            case Whole():
                extent = {("use_border",): False}
        native: dict[RnaPath, Value] = {(member,): getattr(self, member) for member in self.native}
        return Snapshot.of(bpy.types.RenderSettings, {**native, ("ppm_factor",): self.density * ppm_base, **extent})


class Grown(msgspec.Struct, frozen=True, kw_only=True):
    """Original framing and native values around overscan writes."""

    base: Framing
    ends: BeforeAfter


class Overscan(bpy.types.PropertyGroup):
    """Overscan a whole-frame render grows by."""

    overscan: bpy.props.FloatProperty(name="Overscan", description="Share of frame side added past each edge, camera widened to keep view", min=0.0, soft_max=0.25, precision=3)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Framed(msgspec.Struct, frozen=True, kw_only=True):
    """Overscan attachment and stored pre-render state."""

    # --- [STATE]

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

    def rasters(self, scene: bpy.types.Scene) -> Outcome[tuple[Raster, Raster]]:
        """Reads native initial render pixels in the original camera domain retained during overscan."""
        current = Framing.of(scene.render)
        return self.recovery.stored.read(scene).bind(lambda held: current.rendered().map(lambda _: current.rasters(held.map(lambda grown: grown.base).default_value(current))))

    def grown(self, scene: bpy.types.Scene) -> Outcome[tuple[Framing, tuple[int, int]]]:
        """Grows overscan, recording render and camera writes."""

        def grow(held: Option[Grown]) -> Outcome[tuple[Framing, tuple[int, int]]]:
            base = self.read(scene, Some(Whole()) if held.is_some() else Nothing)

            def wrote(formed: tuple[Framing, tuple[int, int]]) -> Outcome[tuple[Framing, tuple[int, int]]]:
                bigger, covered = formed
                result = (bigger, bigger.size if isinstance(bigger.extent, Border) and not bigger.extent.crop else covered)
                if held.is_some() or not any(base.padding):
                    return Ok(result)
                return (
                    render_cameras(scene)
                    .bind(
                        lambda cameras: self.recovery.recorded(
                            scene,
                            (
                                (scene.render, bigger.snapshot(scene.render.ppm_base)),
                                *((camera, snapshot) for camera in cameras for factor in base.growth(camera).to_list() for snapshot in widened(camera, float(factor)).to_list()),
                            ),
                            lambda ends: Grown(base=base, ends=ends),
                        )
                    )
                    .map(lambda _: result)
                )

            return base.rendered().bind(wrote)

        return self.recovery.stored.read(scene).bind(grow)

    # --- [REGIONS]

    def bordered(self, scene: bpy.types.Scene, region: Region) -> Outcome[Answer]:
        """Writes region with scene's existing crop preference."""
        extent = Border(region=region, crop=scene.render.use_crop_to_border)
        return answered(msgspec.structs.replace(self.read(scene), extent=extent).snapshot(scene.render.ppm_base).apply(scene.render), "Render region covers area", (("INFO", "Render region set"),))

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
                Option.of_optional(scene.camera).bind(lambda camera: projected(camera, scene)).to_result_with(partial(Fault.of, Reason.REQUIRED, "selection in front of scene camera")),
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
        .filter_with(lambda held: max(held) <= bound, lambda held: Fault.of(Reason.OUTSIDE, "Compositor canvas", " x ".join(map(str, held)), member=f"at most {bound} pixels per side"))
        .map(lambda _: None)
    )


def duration(context: bpy.types.Context) -> Option[str]:
    """Returns total and preview inclusive-frame durations as timecode."""

    def spans(scene: bpy.types.Scene) -> str:
        render = scene.render
        held = (("Total", scene.frame_start, scene.frame_end), *((("Preview", scene.frame_preview_start, scene.frame_preview_end),) if scene.use_preview_range else ()))
        return ", ".join(f"{label} {bpy.utils.smpte_from_frame(end - start + 1, fps=render.fps, fps_base=render.fps_base)}, {end - start + 1} frames" for label, start, end in held)

    return Option.of_optional(context.scene).map(spans)


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Empty, condition=Some(_CAMERA_VIEW))
def camera_border(call: Call[Empty]) -> Outcome[Answer]:
    """Set render region to camera frame."""
    return paired(rooted(bpy.types.Scene, call.context), FrameSize().bound(call.identity)).bind(lambda pair: pair[1].bordered(pair[0], Region()))


@Operator.of(Empty, condition=Some(_CAMERA_VIEW & Count(collection=Descriptor(root=bpy.types.ViewLayer, owner=("objects",), prop=_SELECTED))))
def selection_border(call: Call[Empty]) -> Outcome[Answer]:
    """Set render region to selection bounds in camera view."""
    return FrameSize().bound(call.identity).bind(lambda framed: framed.selected(call))


class FrameSize(msgspec.Struct, frozen=True, kw_only=True):
    """Framing attachment, border commands, native-panel readings, and load restoration."""

    key: ClassVar[Key] = "framing"
    attachment: ClassVar[Attachment[Overscan]] = Attachment(key=key, form=Overscan, owner=bpy.types.Scene)
    prerequisites: ClassVar[tuple[Registrable, ...]] = (attachment,)

    def bound(self, identity: Identity) -> Outcome[Framed]:
        """Binds registered overscan and its recovery."""
        recovery = Recovery(stored=Stored.keyed(identity, f"{self.key}_recovery", msgspec.msgpack.Decoder(Grown)), before_after_of=lambda grown: grown.ends)
        return self.attachment.bound(identity).map(lambda attached: Framed(attached=attached, recovery=recovery))

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns declarations with callbacks binding after registration."""
        return (
            camera_border,
            selection_border,
            Append(target=RENDER_PT_format, rows=(Label(text=lambda context: identity.reports.settled(self.bound(identity)).bind(lambda framed: framed.caption(context)).to_list()),)),
            Append(target=RENDER_PT_frame_range, rows=(Label(text=lambda context: duration(context).to_list()),)),
            Handler(
                handlers=bpy.app.handlers.load_post,
                body=lambda: self.bound(identity).bind(lambda framed: accumulated(framed.recovery.reverted(scene) for scene in bpy.data.scenes).map(lambda _: None)),
            ),
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RESOLUTION", "Border", "Extents", "FrameSize", "Framed", "Framing", "Grown", "Overscan", "Whole", "camera_border", "capacity", "duration", "selection_border"]
