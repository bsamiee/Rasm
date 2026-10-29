"""Image editor views: the displayed image's kind, the editors showing it, the frame it draws in, pointer picks read as frame regions, the render view revealed where the render display preference places it, overlays over the frame, and frame region handles."""

from collections.abc import Callable
from functools import partial, wraps
import math
from typing import ClassVar, Final, Literal, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
import gpu
from mathutils import Matrix, Vector
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.calls import called
from blender.bpy.commands.invocations import Pick
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import root_in, rooted
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.ui.drawing.canvas import Canvas, DrawType, Rect
from blender.bpy.ui.drawing.handlers import Drawing, Overlay, Part, Refresh
from blender.bpy.ui.resources.previews import uploaded
from blender.bpy.ui.resources.theme import Color, color, Metrics, Shade
from blender.bpy.ui.screens.areas import AreaQuery, Arrangement, EditorState, Located, visibility
from blender.bpy.ui.surfaces.gizmos import GizmoPart, Gizmos, TargetHandler
from blender.bpy.ui.surfaces.panels import Beside, Sidebar
from blender.imaging.geometry.frames import Quad, Region
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

HANDLE: Final = 0.2
_WINDOW: Final = Setting(descriptor=Descriptor.declared(bpy.types.Context, ("region", "type"), bpy.types.EnumProperty), accepted=("WINDOW",))

# --- [MODELS] ---------------------------------------------------------------------------

# --- [VIEWS]


class RenderView(msgspec.Struct, frozen=True, kw_only=True):
    """Render view revealed, its Sidebar opened, then turned on the next host pass to the tab of the panel placement `at`, once the region has drawn its tabs."""

    at: Beside | Sidebar

    @property
    def command(self) -> Operator[Empty]:
        """Command revealing the view at the placement's tab."""
        return Operator(undo=Nothing, form=Empty, run=self.show_render_view)

    def show_render_view(self, call: Call[Empty]) -> Outcome[Answer]:
        """Shows the render view, opening one unless an editor shows Render Result, then queues the Sidebar tab the placement's members name, none for a stock panel declaring no category."""
        tabbed = (
            Option
            .of_optional(try_downcast(str, self.at.members(call.identity).get("bl_category")))
            .map(lambda tab: EditorState.of(bpy.types.Region, {("active_panel_category",): tab}, Some("UI")).map(lambda state: (state,)))
            .default_value(Ok(()))
        )
        return (
            paired(revealed(call.context), paired(visibility(bpy.types.SpaceImageEditor, "UI", visible=True), tabbed))
            .bind(lambda found: call.identity.reports.queued(Arrangement(at=found[0], steps=(found[1][0], *found[1][1]))))
            .map(lambda _: Done())
        )

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the command revealing the view."""
        return Ok((self.command,))


class Picked(bpy.types.PropertyGroup):
    """Pick point and sampled square half side."""

    x: bpy.props.IntProperty(name="X", description="Pick column in region pixels", options={"HIDDEN", "SKIP_SAVE"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    y: bpy.props.IntProperty(name="Y", description="Pick row in region pixels", options={"HIDDEN", "SKIP_SAVE"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    radius: bpy.props.IntProperty(name="Sample Radius", description="Pixels sampled each side of pick", min=0, max=3)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OVERLAYS]


class Tint(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Top-first frame coverage plane with its tint color."""

    plane: npt.NDArray[np.float32]
    color: Color


class Masks(msgspec.Struct, frozen=True, kw_only=True):
    """Part uploading each tint's coverage once per refresh and drawing it over the frame in its color."""

    draw_type: ClassVar[DrawType] = "POST_VIEW"
    source: Callable[[Metrics], Outcome[Option[tuple[Tint, ...]]]]

    def read(self, metrics: Metrics) -> Outcome[Option[tuple[tuple[gpu.types.GPUTexture, Tint], ...]]]:
        """Uploads each coverage as associated white of that alpha, paired with its tint."""
        return self.source(metrics).bind(
            lambda found: found.map(
                lambda tints: accumulated(uploaded(np.repeat(tint.plane[..., None], 4, axis=-1)) for tint in tints).map(lambda textures: Some(tuple(zip(textures, tints, strict=True))))
            ).default_value(Ok(Nothing))
        )

    @staticmethod
    def drawn(canvas: Canvas, state: tuple[tuple[gpu.types.GPUTexture, Tint], ...], /) -> None:
        """Draws each coverage over the unit frame in its tint associated with its alpha, nearest filtering past full zoom."""
        match canvas.space:
            case bpy.types.SpaceImageEditor(zoom=zoom):
                across, _ = np.asarray(zoom)
                for texture, tint in state:
                    red, green, blue, alpha = tint.color
                    texture.filter_mode(bool(across < 1.0))
                    canvas.picture(texture, Rect(x=0.0, y=0.0, width=1.0, height=1.0), blend="ALPHA_PREMULT", color=(red * alpha, green * alpha, blue * alpha, alpha))
            case _:
                pass


class Edges(msgspec.Struct, frozen=True, kw_only=True):
    """Part outlining a frame quad in a theme color."""

    draw_type: ClassVar[DrawType] = "POST_VIEW"
    quad: Callable[[bpy.types.Context], Outcome[Option[Quad]]]
    role: Callable[[bpy.types.ThemeUserInterface], Shade]

    def read(self, _metrics: Metrics, /) -> Outcome[Option[gpu.types.GPUBatch]]:
        """Batches the quad as a closed corner strip in frame fractions."""
        return self.quad(bpy.context).map(
            lambda found: found.map(lambda held: Canvas.batched("POLYLINE_UNIFORM_COLOR", "LINE_STRIP", {"pos": np.ascontiguousarray(held.samples[(0, 1, 2, 3, 0), :2], np.float32)}))
        )

    def drawn(self, canvas: Canvas, state: gpu.types.GPUBatch, /) -> None:
        """Draws the strip in the role's theme color."""
        with canvas.lines(1.0) as lines:
            lines.uniform_float("color", color(self.role(canvas.metrics.preferences.themes[0].user_interface)))
            state.draw(lines)


# --- [CAGES]


class FrameRegion(msgspec.Struct, frozen=True, kw_only=True):
    """Quad over images `shown` accepts in frame fractions, read and written through its owner and edited through one host cage.

    The cage moves, turns, and scales the quad as `transform` allows, Shift scaling uniformly and Escape restoring the press value.
    `corners` adds the quad outline and one move handle per corner, ahead of the cage so a press on a corner takes the corner, and every edit stays inside `bounds`, a pure move shifting the whole quad.
    """

    key: Key
    read: Callable[[bpy.types.Context], Outcome[Option[Quad]]]
    write: Callable[[bpy.types.Context, Quad], Outcome[None]]
    transform: frozenset[Literal["TRANSLATE", "ROTATE", "SCALE", "SCALE_UNIFORM"]] = frozenset({"TRANSLATE", "SCALE"})
    corners: bool = False
    bounds: Option[Region] = Nothing
    shown: Setting = msgspec.field(default_factory=lambda: RESULT)
    refreshes: tuple[Refresh, ...] = ()

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the handle group, then the quad outline when corners are edited."""
        cage = GizmoPart(
            kind="GIZMO_GT_cage_2d", place=self.origin, bind=TargetHandler[tuple[float, ...]](get=self.caging, set=self.caged), properties={"transform": self.transform, "draw_options": frozenset()}
        )
        corners = tuple(
            GizmoPart(
                kind="GIZMO_GT_move_3d",
                place=self.origin,
                bind=TargetHandler[tuple[float, ...]](get=partial(self.corner, index), set=partial(self.cornered, index)),
                properties={"scale_basis": HANDLE, "draw_style": "CROSS_2D"},
            )
            for index in range(len(Quad().corners) if self.corners else 0)
        )
        group = Gizmos(key=self.key, space="IMAGE_EDITOR", parts=(*corners, cage), condition=Some(self.shown))
        outline = overlaid(Edges(quad=self.read, role=lambda ui: ui.gizmo_primary), self.shown, self.refreshes)
        return Ok((group, *((outline,) if self.corners else ())))

    # --- [READING]

    def held(self, context: bpy.types.Context) -> Outcome[Option[tuple[Quad, Rect]]]:
        """Reads the quad with the frame of the region drawing it."""
        return self.read(context).map(lambda found: found.bind(lambda quad: root_in(bpy.types.Region, context).map(lambda region: (quad, framed(region)))))

    def origin(self, context: bpy.types.Context) -> Outcome[Option[Matrix]]:
        """Places a handle at the frame origin while the owner holds a quad."""
        return self.held(context).map(lambda found: found.map(lambda pair: Matrix.Translation((pair[1].x, pair[1].y, 0.0))))

    def caging(self, context: bpy.types.Context) -> tuple[float, ...]:
        """Returns the cage's frame of the quad column by column, the order of the host's float matrix target, identity while the handle is unplaced, its placement holding any read fault."""
        frame = self.held(context).to_option().bind(lambda found: found).map(lambda pair: _frame(_pixels(*pair))).default_with(lambda: Matrix.Identity(4))
        return tuple(np.asarray(frame).ravel(order="F").tolist())

    def corner(self, index: int, context: bpy.types.Context) -> tuple[float, ...]:
        """Returns the corner's offset from the frame origin in region pixels, zero while the handle is unplaced."""
        return self.held(context).to_option().bind(lambda found: found).map(lambda pair: (*_pixels(*pair)[index], 0.0)).default_value((0.0, 0.0, 0.0))

    # --- [WRITING]

    def caged(self, context: bpy.types.Context, values: tuple[float, ...]) -> Outcome[None]:
        """Writes the quad the cage's change of frame carries, a pure move shifting it whole inside the bounds."""

        def carried(quad: Quad, frame: Rect) -> Outcome[None]:
            pixels = _pixels(quad, frame)
            change = Matrix(np.reshape(values, (4, 4)).tolist()).transposed() @ _frame(pixels).inverted_safe()
            moved = np.asarray([(change @ Vector((x, y, 0.0))).to_2d() for x, y in pixels]) / (frame.width, frame.height)
            return self.write(context, Quad.of(self.bounded(moved, shifted=np.allclose(np.asarray(change.to_2x2()), np.eye(2)))))

        return self.pressed(context).bind(lambda pair: carried(*pair))

    def cornered(self, index: int, context: bpy.types.Context, values: tuple[float, ...]) -> Outcome[None]:
        """Writes the quad with one corner at the handle's offset."""

        def carried(quad: Quad, frame: Rect) -> Outcome[None]:
            fractions = quad.samples[:, :2].copy()
            fractions[index] = np.asarray(values[:2]) / (frame.width, frame.height)
            return self.write(context, Quad.of(self.bounded(fractions, shifted=False)))

        return self.pressed(context).bind(lambda pair: carried(*pair))

    def pressed(self, context: bpy.types.Context) -> Outcome[tuple[Quad, Rect]]:
        """Reads the quad and frame under a handle, absent fault while the owner holds none."""
        return self.held(context).bind(lambda found: found.to_result_with(partial(Fault.of, Reason.ABSENT, repr(context.area), member=self.key)))

    def bounded(self, fractions: npt.NDArray[np.float64], *, shifted: bool) -> npt.NDArray[np.float64]:
        """Keeps corners inside the bounds, a shifted quad first moving whole as far as the bounds let it before any corner is clipped."""
        for region in self.bounds.to_list():
            floor, ceiling = np.array((region.left, region.bottom)), np.array((region.right, region.top))
            lower = fractions.min(axis=0)
            offset = np.clip(lower, floor, np.maximum(ceiling - np.ptp(fractions, axis=0), floor)) - lower if shifted else 0.0
            return np.clip(fractions + offset, floor, ceiling)
        return fractions


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [EDITORS]


def image_type(kinds: tuple[str, ...]) -> Setting:
    """Returns the condition that an image editor shows an image of one of `kinds`.

    Args:
        kinds: `Image.type` items accepted.

    Returns:
        Condition over the image editor space.
    """
    return Setting(descriptor=Descriptor.declared(bpy.types.SpaceImageEditor, ("image", "type"), bpy.types.EnumProperty), accepted=kinds)


def _editor(shown: Setting) -> AreaQuery:
    """Returns the query over image editors whose space `shown` accepts."""
    return AreaQuery(space=bpy.types.SpaceImageEditor, ui_type=Some("IMAGE_EDITOR"), shows=Some(shown))


def framed(region: bpy.types.Region) -> Rect:
    """Returns the displayed frame's rectangle in region pixels, the region pixels of image fractions 0 and 1.

    Args:
        region: Image editor main region.

    Returns:
        Frame rectangle in region pixels.
    """
    view = region.view2d
    (left, bottom), (right, top) = np.asarray(view.region_to_view(0.0, 0.0)), np.asarray(view.region_to_view(1.0, 1.0))
    across, up = float(right - left), float(top - bottom)
    return Rect(x=float(-left) / across, y=float(-bottom) / up, width=1.0 / across, height=1.0 / up)


def viewing(context: bpy.types.Context) -> Outcome[Located]:
    """Returns the largest editor showing Render Result in a window of the context scene.

    Args:
        context: Context naming the scene and window manager.

    Returns:
        Located editor, absent fault while none shows Render Result.
    """
    return rooted(bpy.types.WindowManager, context).bind(lambda manager: RENDERED.first(window for window in manager.windows if window.scene == context.scene))


def revealed(context: bpy.types.Context) -> Outcome[Located]:
    """Returns the editor `viewing` finds, first opening the render view where the render display preference places it when none shows Render Result.

    A temporary context window opens nothing, the host toggle lowering that window in place of opening a view.

    Args:
        context: Context the render view opens from.

    Returns:
        Located editor showing Render Result, the `viewing` fault while none shows after the open.
    """
    return viewing(context).or_else_with(
        lambda missed: (
            rooted(bpy.types.Window, context)
            .filter_with(lambda window: not window.screen.is_temporary, lambda _: missed)
            .bind(lambda _: called(bpy.ops.render.view_show, {}, context="INVOKE_DEFAULT"))
            .bind(lambda _: viewing(context))
        )
    )


def overlaid[S](part: Part[S], shown: Setting, refreshes: tuple[Refresh, ...]) -> Overlay[S]:
    """Places a part over the image editor while `shown` holds, in the draw phase the part states.

    Args:
        part: Part drawn over the editor.
        shown: Condition over the image editor space.
        refreshes: Sources invalidating the part's read.

    Returns:
        Overlay drawing over the image editor's main region.
    """
    return Overlay(drawing=Drawing(area=_editor(shown), part=part, refreshes=refreshes))


# --- [PICKS]


def picking[P: Picked](cursor: "rna_enums.WindowCursorItems", form: type[P], taken: Callable[[Call[P], Region], Outcome[Answer]], shown: Option[Setting] = Nothing) -> Operator[P]:
    """Returns an operator reading a pick in the main region while `shown` holds as the frame region `taken` edits, keyed and described by `taken`.

    The button opening the pick draws anywhere, the host deferring the operator until the pointer picks a location, so `shown` tests the editor picked in.

    Args:
        cursor: Cursor the pending pick shows.
        form: Pick properties, `Picked` or a group extending it.
        taken: Edit of the picked frame region.
        shown: Condition over the image editor space, an editor showing Render Result while unstated.

    Returns:
        Operator seeding the pick from the event's region position.
    """

    @wraps(taken)
    def ran(call: Call[P]) -> Outcome[Answer]:
        return (
            (shown.default_value(RESULT) & _WINDOW)
            .check(call.context)
            .bind(lambda _: paired(rooted(bpy.types.SpaceImageEditor, call.context), rooted(bpy.types.Region, call.context)))
            .bind(partial(_regioned, call.properties))
            .bind(lambda region: taken(call, region))
        )

    return Operator(form=form, run=ran, invoke=Some(Pick(cursor=cursor)), seed=Some(lambda _properties, event, _identity: {"x": event.mouse_region_x, "y": event.mouse_region_y}))


def _regioned(record: Picked, located: tuple[bpy.types.SpaceImageEditor, bpy.types.Region]) -> Outcome[Region]:
    """Returns the frame region of the square around the picked image pixel, absent fault for a pick outside the image."""
    space, region = located
    frame, (zoom_x, zoom_y) = framed(region), np.asarray(space.zoom)
    width, height = round(frame.width / zoom_x), round(frame.height / zoom_y)
    across, up = np.asarray(region.view2d.region_to_view(record.x, record.y))
    column, row = math.floor(across * width), math.floor(up * height)
    reach = np.array(((-record.radius, -record.radius), (record.radius + 1, record.radius + 1)))
    return (
        Some((column, row))
        .filter(lambda pixel: 0 <= pixel[0] < width and 0 <= pixel[1] < height)
        .map(lambda pixel: Region.bounding(np.clip((np.array(pixel) + reach) / (width, height), 0.0, 1.0)))
        .to_result_with(partial(Fault.of, Reason.ABSENT, repr(space.image), member=f"pixel {column}, {row}"))
    )


# --- [CAGES]


def _pixels(quad: Quad, frame: Rect) -> npt.NDArray[np.float64]:
    """Returns quad corners in pixels from the frame origin."""
    return quad.samples[:, :2] * (frame.width, frame.height)


def _frame(pixels: npt.NDArray[np.float64]) -> Matrix:
    """Returns the frame the cage edits: the corner mean with the mean edge across and the mean edge up, the exact rectangle of an aligned quad."""
    top_left, top_right, bottom_right, bottom_left = pixels
    (across_x, across_y), (up_x, up_y) = (top_right - top_left + bottom_right - bottom_left) / 2.0, (top_left - bottom_left + top_right - bottom_right) / 2.0
    center_x, center_y = pixels.mean(axis=0)
    return Matrix(((across_x, up_x, 0.0, center_x), (across_y, up_y, 0.0, center_y), (0.0, 0.0, 1.0, 0.0), (0.0, 0.0, 0.0, 1.0)))


# --- [COMPOSITION] ----------------------------------------------------------------------

RESULT: Final[Setting] = image_type(("RENDER_RESULT",))
RENDERED: Final = _editor(RESULT)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["HANDLE", "RENDERED", "RESULT", "Edges", "FrameRegion", "Masks", "Picked", "RenderView", "Tint", "framed", "image_type", "overlaid", "picking", "revealed", "viewing"]
