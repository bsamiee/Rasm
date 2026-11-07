"""Image editor views of the displayed frame."""

from collections.abc import Callable, Iterable
from functools import partial, wraps
import math
from typing import ClassVar, Final, Literal, TYPE_CHECKING

import bpy
from expression import Option, Some
from expression.extra.result import catch
import gpu
from gpu_extras.batch import batch_for_shader
from mathutils import Matrix, Vector
import msgspec
import numpy as np
import numpy.typing as npt

from rasm_bpy.commands.invocations import Pick
from rasm_bpy.commands.operators import Answer, Call, called, Done, Empty, Operator
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions
from rasm_bpy.data._rna.conditions import Setting
from rasm_bpy.data._rna.descriptors import Descriptor
from rasm_bpy.data._rna.paths import _definition, root_in, rooted
from rasm_bpy.ui._drawing.canvas import _configured, _draw_batch, Canvas, DrawType, Rect
from rasm_bpy.ui._drawing.handlers import Drawing, Overlay, Part, Refresh
from rasm_bpy.ui._elements.gizmos import Gizmo, GizmoGroup, TargetHandler
from rasm_bpy.ui._elements.panels import Beside, Sidebar
from rasm_bpy.ui._resources.previews import _uploaded, Texture
from rasm_bpy.ui._resources.theme import _color, Color, Metrics, Shade
from rasm_bpy.ui._screens.areas import _Arrangement, _EditorState, _region_in, _visibility, AreaQuery, Located
from rasm_imaging import Quad, Region
from rasm_records import Fault, Nothing, Ok, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Frame = tuple[Quad, Rect]

# --- [CONSTANTS] ------------------------------------------------------------------------

_HANDLE: Final = 0.2
_WINDOW: Final = Setting(descriptor=Descriptor(root=bpy.types.Context, owner=("region",), prop=_definition(bpy.types.Region).properties["type"]), accepted=("WINDOW",))

# --- [MODELS] ---------------------------------------------------------------------------

# --- [VIEWS]


class RenderView(msgspec.Struct, frozen=True, kw_only=True):
    """Render view revealed, its Sidebar opened, then turned on the next host pass to the tab of the panel placement `at`, once the region has drawn its tabs."""

    at: Beside | Sidebar

    @property
    def command(self) -> Operator[Empty]:
        """Command revealing the view at the placement's tab."""
        return Operator(options=frozenset(), form=Empty, run=self.show_render_view)

    def show_render_view(self, call: Call[Empty]) -> Outcome[Answer]:
        """Shows the render view with its Sidebar open at the placement's tab."""
        match self.at:
            case Sidebar(category=tab):
                category = Some(tab)
            case Beside(panel=panel):
                declared: type[bpy.types.Panel] = panel  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                category = catch(lambda: declared.bl_category, exception=AttributeError)().to_option()
        tabbed = category.map(lambda tab: _EditorState.of(bpy.types.Region, {("active_panel_category",): tab}, Some("UI")).map(lambda state: (state,))).default_value(Ok(()))

        def queued(at: Located, sidebar: _EditorState, tab: tuple[_EditorState, ...]) -> Outcome[None]:
            return call.identity.reports.queued(_Arrangement(at=at, steps=(sidebar, *tab)))

        return paired(revealed(call.context), _visibility(bpy.types.SpaceImageEditor, "UI", visible=True), tabbed).bind(lambda found: queued(*found)).map(lambda _: Done())

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns the command revealing the view."""
        return (self.command,)


class Picked(bpy.types.PropertyGroup):
    """Pick point and sampled square half side."""

    x: bpy.props.IntProperty(name="X", description="Pick column in region pixels", options={"HIDDEN", "SKIP_SAVE"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    y: bpy.props.IntProperty(name="Y", description="Pick row in region pixels", options={"HIDDEN", "SKIP_SAVE"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    radius: bpy.props.IntProperty(name="Sample Radius", description="Pixels sampled each side of pick", min=0, max=3)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OVERLAYS]


class Masks(msgspec.Struct, frozen=True, kw_only=True):
    """Part reading uploaded tints and drawing them over the frame."""

    draw_type: ClassVar[DrawType] = "POST_VIEW"
    read: Callable[[Metrics], Outcome[Option[tuple[Texture[Color], ...]]]]

    @staticmethod
    def drawn(state: tuple[Texture[Color], ...], canvas: Canvas, /) -> None:
        """Draws each coverage over the unit frame in its tint associated with its alpha, nearest filtering past full zoom."""
        if isinstance(space := canvas.space, bpy.types.SpaceImageEditor):
            zoom: Iterable[float] = space.zoom  # type: ignore[assignment]  # ty: ignore[unsound-assignment]
            filtered = min(zoom) < 1.0
            for texture, (red, green, blue, alpha) in state:
                texture.filter_mode(filtered)
                canvas.picture(texture, Rect(x=0.0, y=0.0, width=1.0, height=1.0), blend="ALPHA_PREMULT", color=(red * alpha, green * alpha, blue * alpha, alpha))


class _Edges(msgspec.Struct, frozen=True, kw_only=True):
    """Part outlining a frame quad in a theme color."""

    draw_type: ClassVar[DrawType] = "POST_VIEW"
    quad: Callable[[bpy.types.Context], Outcome[Option[Quad]]]
    role: Callable[[bpy.types.ThemeUserInterface], Shade]

    def read(self, _metrics: Metrics, /) -> Outcome[Option[gpu.types.GPUBatch]]:
        """Batches the quad as a closed corner strip in frame fractions."""
        return self.quad(bpy.context).map(
            lambda found: found.map(
                lambda held: batch_for_shader(gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR"), "LINE_STRIP", {"pos": np.array((*held.corners, held.corners[0]), np.float32)})  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]
            )
        )

    def drawn(self, state: gpu.types.GPUBatch, canvas: Canvas, /) -> None:
        """Draws the strip in the role's theme color."""
        lines = canvas.lines(1.0)
        _draw_batch(state, _configured(lines, partial(lines[0].uniform_float, "color", _color(self.role(canvas.metrics.preferences.themes[0].user_interface)))))


# --- [CAGES]


class FrameRegion(msgspec.Struct, frozen=True, kw_only=True):
    """Quad over images `shown` accepts in frame fractions, read and written through its owner and edited through one host cage.

    Cage moves, turns, and scales the quad as `transform` allows, Shift scaling uniformly and Escape restoring the press value.
    `corners` adds the quad outline and one move handle per corner, ahead of the cage, a press on a corner taking the corner, and every edit stays inside `bounds`, a pure move shifting the whole quad.
    """

    # --- [STATE]

    key: Key
    read: Callable[[bpy.types.Context], Outcome[Option[Quad]]]
    write: Callable[[bpy.types.Context, Quad], Outcome[None]]
    transform: frozenset[Literal["TRANSLATE", "ROTATE", "SCALE", "SCALE_UNIFORM"]] = frozenset({"TRANSLATE", "SCALE"})
    corners: bool = False
    bounds: Option[Region] = Nothing
    shown: Setting = msgspec.field(default_factory=lambda: RESULT)
    refreshes: tuple[Refresh, ...] = ()

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns the handle group, then the quad outline when corners are edited."""
        cage = Gizmo(
            kind="GIZMO_GT_cage_2d",
            place=self.origin,
            bind=TargetHandler[tuple[float, ...], _Frame](get=self.caging, set=self.caged),
            properties={"transform": self.transform, "draw_options": frozenset()},
        )
        corners = tuple(
            Gizmo(
                kind="GIZMO_GT_move_3d",
                place=self.origin,
                bind=TargetHandler[tuple[float, ...], _Frame](get=partial(self.corner, index), set=partial(self.cornered, index)),
                properties={"scale_basis": _HANDLE, "draw_style": "CROSS_2D"},
            )
            for index in range(len(Quad().corners) if self.corners else 0)
        )
        group = GizmoGroup(key=self.key, space="IMAGE_EDITOR", gizmos=(*corners, cage), read=self.held, condition=Some(self.shown))
        return (group, *((overlaid(_Edges(quad=self.read, role=lambda ui: ui.gizmo_primary), self.shown, self.refreshes),) if self.corners else ()))

    # --- [READS]

    def held(self, context: bpy.types.Context) -> Outcome[Option[_Frame]]:
        """Reads the quad with the frame of the region drawing it."""
        return self.read(context).map(lambda found: found.bind(lambda quad: root_in(bpy.types.Region, context).map(lambda region: (quad, _framed(region)))))

    @staticmethod
    def origin(state: _Frame) -> Outcome[Option[Matrix]]:
        """Places a handle at the frame origin."""
        return Ok(Some(Matrix.Translation((state[1].x, state[1].y, 0.0))))

    @staticmethod
    def caging(state: _Frame) -> tuple[float, ...]:
        """Returns the cage frame in the native target's column order."""
        columns: Iterable[Vector] = _frame(_pixels(*state)).col  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return tuple(value for column in columns for value in column)  # type: ignore[attr-defined]

    @staticmethod
    def corner(index: int, state: _Frame) -> tuple[float, ...]:
        """Returns the corner offset from the frame origin in region pixels."""
        quad, frame = state
        x, y = quad.corners[index]
        return x * frame.width, y * frame.height, 0.0

    # --- [WRITES]

    def caged(self, context: bpy.types.Context, values: tuple[float, ...]) -> Outcome[None]:
        """Writes the cage's transformed quad inside its bounds, refusing a singular source frame."""

        def rewritten(quad: Quad, frame: Rect) -> Outcome[None]:
            pixels = _pixels(quad, frame)
            original, target = _frame(pixels), Matrix(np.reshape(values, (4, 4)).tolist()).transposed()
            shifted = np.array_equal(np.asarray(target.to_2x2()), np.asarray(original.to_2x2()))
            return (
                catch(original.inverted, exception=ValueError)()
                .map_error(partial(Fault.of, Reason.REFUSED, "mathutils.Matrix.inverted"))
                .map(lambda inverse: np.asarray(target @ inverse))
                .bind(lambda change: self.write(context, Quad.of(self.bounded((pixels @ change[:2, :2].T + change[:2, 3]) / (frame.width, frame.height), shifted=shifted))))
            )

        return self.pressed(context).bind(lambda pair: rewritten(*pair))

    def cornered(self, index: int, context: bpy.types.Context, values: tuple[float, ...]) -> Outcome[None]:
        """Writes the quad with one corner at the handle's offset."""

        def rewritten(quad: Quad, frame: Rect) -> Outcome[None]:
            fractions = np.array(quad.corners)
            fractions[index] = values[0] / frame.width, values[1] / frame.height
            return self.write(context, Quad.of(self.bounded(fractions, shifted=False)))

        return self.pressed(context).bind(lambda pair: rewritten(*pair))

    def pressed(self, context: bpy.types.Context) -> Outcome[_Frame]:
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

# --- [UPLOADS]


def coverage(plane: npt.NDArray[np.float32]) -> Outcome[gpu.types.GPUTexture]:
    """Uploads top-first scalar coverage as an associated white RGBA texture."""
    return _uploaded(np.broadcast_to(plane[..., None], (*plane.shape, 4)))


# --- [EDITORS]


def image_type(kinds: tuple[str, ...]) -> Setting:
    """Returns the condition that an image editor shows an image of one of `kinds`.

    Args:
        kinds: `Image.type` items accepted.

    Returns:
        Condition over the image editor space.
    """
    return Setting(descriptor=Descriptor(root=bpy.types.SpaceImageEditor, owner=("image",), prop=_definition(bpy.types.Image).properties["type"]), accepted=kinds)


def _editor(shown: Setting) -> AreaQuery:
    """Returns the query over image editors whose space `shown` accepts."""
    return AreaQuery(space=bpy.types.SpaceImageEditor, ui_type=Some("IMAGE_EDITOR"), shows=Some(shown))


def _framed(region: bpy.types.Region) -> Rect:
    """Returns the displayed frame's rectangle in region pixels, the region pixels of image fractions 0 and 1.

    Args:
        region: Image editor main region.

    Returns:
        Frame rectangle in region pixels.
    """
    view = region.view2d
    origin: tuple[float, float] = view.region_to_view(0.0, 0.0)  # type: ignore[assignment]  # ty: ignore[unsound-assignment]
    unit: tuple[float, float] = view.region_to_view(1.0, 1.0)  # type: ignore[assignment]  # ty: ignore[unsound-assignment]
    left, bottom = origin[0], origin[1]
    across, up = unit[0] - left, unit[1] - bottom
    return Rect(x=-left / across, y=-bottom / up, width=1.0 / across, height=1.0 / up)


def _displayed_aspect(context: bpy.types.Context) -> Outcome[float]:
    """Returns the shown image's aspect from its editor's main-region frame.

    Args:
        context: Context within the image editor, including its Sidebar.

    Returns:
        Displayed width divided by height, or an absent fault outside an editor showing an image.
    """
    return (
        rooted(bpy.types.SpaceImageEditor, context)
        .bind(lambda space: Option.of_optional(space.image).to_result_with(partial(Fault.of, Reason.ABSENT, repr(space), member="image")))
        .bind(lambda _: rooted(bpy.types.Area, context).bind(_region_in))
        .map(_framed)
        .map(lambda frame: frame.width / frame.height)
    )


def _viewing(context: bpy.types.Context) -> Outcome[Located]:
    """Returns the largest editor showing Render Result in a window of the context scene.

    Args:
        context: Context naming the scene and window manager.

    Returns:
        Located editor, absent fault while none shows Render Result.
    """
    return rooted(bpy.types.WindowManager, context).bind(lambda manager: _RENDERED.first(window for window in manager.windows if window.scene == context.scene))


def revealed(context: bpy.types.Context) -> Outcome[Located]:
    """Returns the editor `viewing` finds, first opening the render view where the render display preference places it when none shows Render Result.

    Temporary context window opens nothing, the host toggle lowering that window in place of opening a view.

    Args:
        context: Context the render view opens from.

    Returns:
        Located editor showing Render Result, the `viewing` fault while none shows after the open.
    """
    return _viewing(context).or_else_with(
        lambda missed: (
            rooted(bpy.types.Window, context)
            .filter_with(lambda window: not window.screen.is_temporary, lambda _: missed)
            .bind(lambda _: called(bpy.ops.render.view_show, {}, context="INVOKE_DEFAULT"))
            .bind(lambda _: _viewing(context))
        )
    )


def overlaid[S](part: Part[S], shown: Setting, refreshes: tuple[Refresh, ...]) -> Overlay[S]:
    """Places a part over the image editor while `shown` holds, in the draw phase the part states.

    Args:
        part: Part drawn over the editor.
        shown: Condition over the image editor space.
        refreshes: Sources requesting redraw.

    Returns:
        Overlay drawing over the image editor's main region.
    """
    return Overlay(drawing=Drawing(area=_editor(shown), part=part, refreshes=refreshes))


# --- [PICKS]


def picking[P: Picked](cursor: "rna_enums.WindowCursorItems", form: type[P], taken: Callable[[Call[P], Region], Outcome[Answer]], shown: Option[Setting] = Nothing) -> Operator[P]:
    """Returns an operator reading a pick in the main region while `shown` holds as the frame region `taken` edits, keyed and described by `taken`.

    Button opening the pick draws anywhere, the host deferring the operator until the pointer picks a location, and `shown` tests the editor picked in.

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
    zoom: Iterable[float] = space.zoom  # type: ignore[assignment]  # ty: ignore[unsound-assignment]
    zoom_x, zoom_y = zoom
    frame = _framed(region)
    width, height = round(frame.width / zoom_x), round(frame.height / zoom_y)
    point: tuple[float, float] = region.view2d.region_to_view(record.x, record.y)  # type: ignore[assignment]  # ty: ignore[unsound-assignment]
    column, row = math.floor(point[0] * width), math.floor(point[1] * height)
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
    return np.asarray(quad.corners) * (frame.width, frame.height)


def _frame(pixels: npt.NDArray[np.float64]) -> Matrix:
    """Returns the frame the cage edits: the corner mean with the mean edge across and the mean edge up, the exact rectangle of an aligned quad."""
    top_left, top_right, bottom_right, bottom_left = pixels
    (across_x, across_y), (up_x, up_y) = (top_right - top_left + bottom_right - bottom_left) / 2.0, (top_left - bottom_left + top_right - bottom_right) / 2.0
    center_x, center_y = pixels.mean(axis=0)
    return Matrix(((across_x, up_x, 0.0, center_x), (across_y, up_y, 0.0, center_y), (0.0, 0.0, 1.0, 0.0), (0.0, 0.0, 0.0, 1.0)))


# --- [COMPOSITION] ----------------------------------------------------------------------

RESULT: Final[Setting] = image_type(("RENDER_RESULT",))
_RENDERED: Final = _editor(RESULT)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RESULT", "FrameRegion", "Masks", "Picked", "RenderView", "coverage", "image_type", "overlaid", "picking", "revealed"]
