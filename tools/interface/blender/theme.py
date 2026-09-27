"""Blender's declared theme, each member's color role or value by path."""

from collections.abc import Iterator, Mapping
from enum import auto, Enum
from itertools import chain, repeat
from math import sqrt
from typing import Final

from attrs import asdict, evolve, frozen
import bpy
from mathutils import Color

from interface.roles import Accent, Alpha, Axis, blend, Field, Guide, Ink, Line, Modality, Node, POINT_WIDTH, Rgb, Selection, Status, Surface, Tag, Text, Wire, Zone

# --- [TYPES] ----------------------------------------------------------------------------

type Target = Paint | float | Mapping[str, Target] | tuple[Mapping[str, Target], ...]


class Encoding(Enum):
    """Channel encoding a color member's draw path reads."""

    DISPLAY = auto()
    LINEAR = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

ROUNDNESS: Final = 0.4

# --- [MODELS] ---------------------------------------------------------------------------


@frozen
class Paint:
    """Color role at an alpha in the encoding its draw path reads."""

    rgb: Rgb
    alpha: float = 1.0
    encoding: Encoding = Encoding.DISPLAY

    def stored(self, prop: bpy.types.FloatProperty) -> tuple[float, ...]:
        """Channels Blender stores for the property's array length, rounded as Blender rounds them."""
        display = tuple(round(channel / 255, 4) for channel in self.rgb)
        channels = display if self.encoding is Encoding.DISPLAY else tuple(round(channel, 4) for channel in Color(display).from_srgb_to_scene_linear()[:])
        return (*channels, round(round(self.alpha * 255) / 255, 4))[: prop.array_length]


@frozen(kw_only=True)
class Widget:
    """Colors and shape of one widget class by its `ThemeWidgetColors` members."""

    outline: Paint
    outline_sel: Paint
    inner: Paint
    inner_sel: Paint
    item: Paint
    text: Paint = Paint(Text.PRIMARY)
    text_sel: Paint = Paint(Text.PRIMARY)
    roundness: float = ROUNDNESS
    show_shaded: bool = False


@frozen(kw_only=True)
class Space:
    """Text and header colors `ThemeSpaceGeneric` and `ThemeSpaceGradient` share."""

    title: Paint = Paint(Text.PRIMARY)
    text: Paint = Paint(Text.PRIMARY)
    text_hi: Paint = Paint(Text.PRIMARY)
    header: Paint = Paint(Surface.PANEL)
    header_text: Paint = Paint(Text.PRIMARY)
    header_text_hi: Paint = Paint(Text.PRIMARY)


@frozen(kw_only=True)
class Generic(Space):
    """Colors of a `ThemeSpaceGeneric` space over its flat back."""

    back: Paint


@frozen(kw_only=True)
class Gradient(Space):
    """Colors of a `ThemeSpaceGradient` space over its `ThemeGradientColors` back."""

    gradients: Mapping[str, Target]


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [GRIDS]
def lowered(rgb: Rgb, shade: int) -> Rgb:
    """Theme color the 3D view draws as `rgb` after adding `shade` and lifting the sum by 255 x srgb_to_linear(b / 255)^(1 / 2.2), each channel at its nearest byte."""
    red, green, blue = (max(0, round(channel * 255) - shade) for channel in Color(tuple((channel / 255) ** 2.2 for channel in rgb)).from_scene_linear_to_srgb()[:])
    return (red, green, blue)


def emphasized(minor: Rgb, major: Rgb, ground: Rgb) -> tuple[Rgb, float]:
    """Image editor grid color and byte alpha whose minor line, lifted 10 over the ground, and emphasized line, lifted 20 over the minor line, draw nearest the two roles."""

    def drawn(grid: float, alpha: float, base: int) -> tuple[float, float]:
        low = base + alpha * (grid + 10 - base)
        return low, low + alpha * (grid + 20 - low)

    def fitted(alpha: float) -> tuple[float, Rgb, float]:
        slopes = (alpha, alpha * (2 - alpha))
        red, green, blue = (
            min(255, max(0, round(sum(slope * (target - start) for slope, target, start in zip(slopes, wanted, drawn(0, alpha, base), strict=True)) / sum(slope**2 for slope in slopes))))
            for *wanted, base in zip(minor, major, ground, strict=True)
        )
        error = sum((low - lower) ** 2 + (high - upper) ** 2 for grid, lower, upper, base in zip((red, green, blue), minor, major, ground, strict=True) for low, high in (drawn(grid, alpha, base),))
        return error, (red, green, blue), alpha

    _, grid, alpha = min(fitted(step / 255) for step in range(1, 256))
    return grid, alpha


def brightened(grid: Rgb, axes: tuple[Rgb, ...]) -> float:
    """Floor axis brightness whose one byte offset, added to each axis the 3D view floors from 0.85 of the axis over 0.15 of the grid, draws the axes nearest their roles by least squares."""
    offset = round(sum(role - int(0.15 * ground + 0.85 * role) for axis in axes for role, ground in zip(axis, grid, strict=True)) / (3 * len(axes)))
    return 0.5 + (offset + 0.5) / 510


# --- [THEME]
def targets(prefix: str, members: Mapping[str, Target]) -> Iterator[tuple[str, Target]]:
    """Declared members by dotted path, each collection item by its index."""
    for key, entry in members.items():
        match entry:
            case Mapping():
                yield from targets(f"{prefix}{key}.", entry)
            case tuple():
                yield from chain.from_iterable(targets(f"{prefix}{key}[{index}].", table) for index, table in enumerate(entry))
            case _:
                yield f"{prefix}{key}", entry


def declared_theme(theme: bpy.types.Theme, system: bpy.types.PreferencesSystem) -> dict[str, Target]:
    """Theme targets by path."""
    minor, major = (blend(Line.GRID, Surface.CANVAS, alpha) for alpha in (Alpha.GRID_MINOR, Alpha.GRID_MAJOR))
    image, image_alpha = emphasized(minor, major, Surface.CANVAS)
    grid = lowered(minor, 10)
    text, _, _ = Text.PRIMARY
    hover, _, _ = Surface.HOVER
    menu = next(byte for byte in range(256) if int(0.8 * byte + 0.2 * text) == hover)
    pixel, scale = system.pixel_size, system.ui_scale
    field, pressed, frame, well, border = Paint(Surface.FIELD), Paint(Accent.CONTROL_PRESSED), Paint(Surface.FRAME), Paint(Surface.WELL), Paint(Line.BORDER)
    screen, tab = Paint(Ink.SCREEN), Paint(Accent.TAB_ACTIVE)
    toggle = Widget(outline=field, outline_sel=pressed, inner=field, inner_sel=pressed, item=frame)
    number = Widget(outline=border, outline_sel=Paint(Accent.FOCUS), inner=field, inner_sel=Paint(Field.EDITING), item=Paint(Accent.INDICATOR))
    widgets = {
        "wcol_regular": Widget(outline=field, outline_sel=pressed, inner=field, inner_sel=pressed, item=Paint(Surface.FRAME, Alpha.HALF)),
        "wcol_tool": Widget(outline=field, outline_sel=pressed, inner=field, inner_sel=pressed, item=Paint(Text.PRIMARY)),
        "wcol_toolbar_item": Widget(outline=Paint(Surface.PANEL), outline_sel=pressed, inner=Paint(Surface.PANEL), inner_sel=pressed, item=Paint(Text.PRIMARY, Alpha.RECEDED)),
        "wcol_radio": toggle,
        "wcol_text": Widget(outline=border, outline_sel=Paint(Accent.FOCUS), inner=field, inner_sel=Paint(Field.EDITING), item=Paint(Accent.TEXT_SELECTED)),
        "wcol_option": Widget(outline=border, outline_sel=Paint(Accent.FOCUS), inner=field, inner_sel=Paint(Accent.CHECKBOX_CHECKED), item=Paint(Text.PRIMARY)),
        "wcol_toggle": toggle,
        "wcol_num": number,
        "wcol_numslider": evolve(number, item=Paint(Field.SLIDER)),
        "wcol_box": Widget(outline=border, outline_sel=border, inner=well, inner_sel=field, item=frame),
        "wcol_curve": Widget(outline=border, outline_sel=Paint(Text.SECONDARY), inner=frame, inner_sel=frame, item=border),
        "wcol_menu": Widget(outline=field, outline_sel=pressed, inner=field, inner_sel=pressed, item=Paint(Text.SECONDARY)),
        "wcol_pulldown": Widget(outline=Paint(Line.BORDER, 0.0), outline_sel=Paint(Accent.FOCUS, 0.0), inner=Paint(Surface.FRAME, 0.0), inner_sel=pressed, item=Paint(Text.SECONDARY)),
        "wcol_menu_back": Widget(outline=border, outline_sel=border, inner=frame, inner_sel=Paint(Accent.ROW_ACTIVE), item=Paint(Text.SECONDARY), text=Paint(Text.SECONDARY)),
        "wcol_pie_menu": Widget(outline=border, outline_sel=Paint(Accent.ROW_ACTIVE), inner=frame, inner_sel=Paint(Accent.ROW_ACTIVE), item=Paint(Surface.HOVER)),
        "wcol_tooltip": Widget(outline=border, outline_sel=border, inner=frame, inner_sel=Paint(Accent.ROW_ACTIVE), item=Paint(Text.SECONDARY)),
        "wcol_menu_item": Widget(
            outline=Paint(Line.BORDER, 0.0), outline_sel=Paint(Accent.FOCUS, 0.0), inner=Paint((menu, menu, menu), 0.0), inner_sel=Paint(Accent.ROW_ACTIVE), item=Paint(Text.SECONDARY)
        ),
        "wcol_scroll": Widget(outline=border, outline_sel=border, inner=Paint(Surface.FRAME, 0.0), inner_sel=Paint(Surface.HOVER), item=field),
        "wcol_progress": Widget(outline=border, outline_sel=border, inner=frame, inner_sel=Paint(Accent.INDICATOR), item=Paint(Accent.INDICATOR)),
        "wcol_list_item": Widget(
            outline=Paint(blend(Accent.ROW_SELECTED, Accent.ROW_ACTIVE, 2), 0.0),
            outline_sel=border,
            inner=Paint(Surface.WELL, 0.0),
            inner_sel=Paint(Accent.ROW_ACTIVE),
            item=Paint(Text.PRIMARY, Alpha.ROW_ITEM),
        ),
        "wcol_tab": Widget(outline=frame, outline_sel=frame, inner=well, inner_sel=tab, item=frame, text=Paint(Text.SECONDARY)),
    }

    def points(vertex: Paint, size: int) -> dict[str, Target]:
        """Vertex color, selected color, and size members of an editor."""
        return {"vertex": vertex, "vertex_select": Paint(Selection.ITEM), "vertex_size": size}

    panel_space, well_space, canvas_space = Generic(back=Paint(Surface.PANEL)), Generic(back=well), Generic(back=Paint(Surface.CANVAS))
    timeline_space = Generic(back=Paint(Surface.PANEL), text=Paint(Text.SECONDARY))
    ruled = {"grid": Paint(Line.GRID_PANEL)}
    keyed = {**ruled, **dict.fromkeys(("keyframe_border", "keyframe_border_selected"), Paint(Surface.SHADOW))}
    metadata = {"metadatabg": frame, "metadatatext": Paint(Text.PRIMARY)}
    alternating = {"row_alternate": Paint(Text.PRIMARY, 0.0)}
    matched = {"match": Paint(Accent.INDICATOR)}
    editors: dict[str, tuple[Space, Mapping[str, Target]]] = {
        "view_3d": (
            Gradient(gradients=dict.fromkeys(("high_gradient", "gradient"), Paint(lowered(Surface.CANVAS, 0)))),
            {
                "grid": Paint(grid),
                "grid_major": Paint(lowered(major, 20), Alpha.GRID_MAJOR),
                "grid_axis_brightness": brightened(grid, (Axis.X, Axis.Y)),
                "clipping_border_3d": Paint(Surface.SECTION),
                **points(screen, round(POINT_WIDTH / (sqrt(2) * pixel))),
                **dict.fromkeys(("wire", "wire_edit", "gp_vertex", "empty", "vertex_unreferenced"), screen),
                "camera_passepartout": Paint(Surface.SHADOW),
                "edge_width": 1,
                "gp_wire_edit": Paint(Ink.SCREEN, Alpha.HALF),
                "gp_vertex_size": round(POINT_WIDTH / (2 * pixel)),
                "text_grease_pencil": Paint(Text.SECONDARY),
                **dict.fromkeys(("gp_vertex_select", "object_selected", "edge_select", "edge_mode_select", "nurb_sel_uline", "nurb_sel_vline", "bone_pose"), Paint(Selection.ITEM)),
                **dict.fromkeys(("object_active", "bone_pose_active"), Paint(Selection.ACTIVE)),
                "face": Paint(Text.PRIMARY, Alpha.EDIT_FACE),
                **dict.fromkeys(("face_select", "face_mode_select"), Paint(Selection.ITEM, Alpha.FACE_FILL)),
                "facedot_size": round(POINT_WIDTH / pixel),
                "face_back": Paint(Status.ERROR, Alpha.RECEDED),
                "face_front": Paint(Ink.SCREEN, 0.0),
                "bevel": Paint(Line.BEVEL),
                "seam": Paint(Line.SEAM),
                "sharp": Paint(Line.SHARP),
                "crease": Paint(Line.CREASE),
                **dict.fromkeys(("freestyle", "nurb_uline", "nurb_vline", "view_overlay", "camera", "light", "speaker", "camera_path"), Paint(Guide.HANDLE)),
                **dict.fromkeys(("extra_edge_len", "extra_edge_angle", "extra_face_angle", "extra_face_area"), Paint(Text.PRIMARY)),
                "editmesh_active": Paint(Selection.ACTIVE, Alpha.FACE_FILL),
                **dict.fromkeys(("normal", "vertex_normal", "split_normal", "skin_root"), Paint(Guide.CONSTRUCTION)),
                "face_retopology": Paint(Surface.SHADED, Alpha.FACE_FILL),
                **dict.fromkeys(("bone_solid", "bundle_solid"), Paint(Surface.SHADED)),
                "bone_locked_weight": Paint(Line.LOCKED, Alpha.HALF),
                "before_current_frame": Paint(Line.BEFORE_FRAME),
                "after_current_frame": Paint(Line.AFTER_FRAME),
                "transform": Paint(Guide.TRACKING),
                "outline_width": 1,
                "object_origin_size": round((POINT_WIDTH + 1) / pixel - 1),
            },
        ),
        "graph_editor": (timeline_space, {**ruled, **points(Paint(Text.SECONDARY), round((POINT_WIDTH + 1) / scale)), "vertex_active": Paint(Selection.ACTIVE)}),
        "file_browser": (well_space, {"selected_file": Paint(Accent.ROW_SELECTED), **alternating}),
        "nla_editor": (
            timeline_space,
            {
                **keyed,
                "active_action": Paint(Accent.INDICATOR, Alpha.TINT),
                "active_action_unset": Paint(Line.LOCKED, Alpha.UNSET),
                **dict.fromkeys(("strips", "meta_strips", "sound_strips"), field),
                "transition_strips": Paint(Surface.BOX),
                **dict.fromkeys(("strips_selected", "transition_strips_selected", "meta_strips_selected", "sound_strips_selected"), Paint(Selection.BODY)),
                "tweak": Paint(Guide.TENTATIVE),
                "tweak_duplicate": Paint(Status.WARNING),
            },
        ),
        "dopesheet_editor": (
            timeline_space,
            {
                **keyed,
                "keyframe_scale_factor": 1.0,
                "summary": Paint(Surface.BOX),
                "anim_interpolation_linear": Paint(Text.SECONDARY, Alpha.INTERPOLATION),
                "anim_interpolation_constant": Paint(Text.DISABLED, Alpha.INTERPOLATION),
                "anim_interpolation_other": Paint(Line.LOCKED, Alpha.RECEDED),
                "simulated_frames": Paint(Accent.INDICATOR),
            },
        ),
        "image_editor": (
            canvas_space,
            {
                "grid": Paint(image, image_alpha),
                **points(screen, round(((POINT_WIDTH + 2.5) / sqrt(2) - 1.5) / scale)),
                "wire_edit": screen,
                "edge_select": Paint(Selection.ITEM),
                "face": Paint(Text.PRIMARY, Alpha.UV_FACE),
                "face_select": Paint(Selection.ITEM, Alpha.FACE_FILL),
                "face_mode_select": Paint(Selection.ITEM, 0.0),
                "facedot_size": round(POINT_WIDTH / scale),
                "editmesh_active": Paint(Selection.ACTIVE, Alpha.FACE_FILL),
                "edge_width": 1,
                "scope_back": Paint(Surface.PANEL),
                **dict.fromkeys(("preview_stitch_face", "preview_stitch_edge", "preview_stitch_vert"), Paint(Guide.TRACKING, Alpha.FACE_FILL)),
                "preview_stitch_stitchable": Paint(Guide.TRACKING),
                "preview_stitch_unstitchable": Paint(Status.ERROR),
                "preview_stitch_active": Paint(Guide.TRACKING, Alpha.SELECTION_FILL),
                "uv_shadow": Paint(Line.LOCKED),
                **metadata,
            },
        ),
        "sequence_editor": (
            timeline_space,
            {
                **keyed,
                **dict.fromkeys(
                    ("movie_strip", "movieclip_strip", "image_strip", "scene_strip", "audio_strip", "effect_strip", "transition_strip", "color_strip", "meta_strip", "mask_strip", "text_strip"), field
                ),
                "active_strip": Paint(Selection.ACTIVE),
                **dict.fromkeys(("selected_strip", "text_strip_cursor"), Paint(Selection.ITEM)),
                "preview_back": Paint(Surface.CANVAS),
                **metadata,
                **alternating,
                "selected_text": Paint(Accent.TEXT_SELECTED),
            },
        ),
        "properties": (panel_space, matched),
        "text_editor": (
            well_space,
            {
                "line_numbers": Paint(Text.SECONDARY),
                "line_numbers_background": frame,
                "selected_text": Paint(Accent.TEXT_SELECTED),
                "cursor": Paint(Accent.ITEM_SELECTED),
                "syntax_builtin": Paint(Text.KEYWORD),
                "syntax_symbols": Paint(Text.PRIMARY),
                "syntax_special": Paint(Text.FUNCTION),
                "syntax_preprocessor": Paint(Text.DECORATOR),
                "syntax_reserved": Paint(Text.ERROR),
                "syntax_comment": Paint(Text.COMMENT),
                "syntax_string": Paint(Text.STRING),
                "syntax_numbers": Paint(Text.NUMBER),
            },
        ),
        "node_editor": (
            canvas_space,
            {
                "grid": Paint(Line.GRID),
                "node_outline": Paint(Node.BORDER),
                "wire": Paint(Wire.CASING),
                "wire_inner": Paint(Wire.CORE),
                "wire_select": Paint(Wire.SELECTED, Alpha.WIRE_SELECTED),
                "node_selected": Paint(Node.SELECTED_BORDER),
                "node_active": Paint(Node.ACTIVE_BORDER),
                "node_backdrop": Paint(Surface.PANEL),
                **{
                    f"{kind}_node": Paint(modality.header)
                    for kind, modality in (
                        ("input", Modality.INPUT),
                        ("output", Modality.OUTPUT),
                        ("geometry", Modality.GEOMETRY),
                        ("vector", Modality.VECTOR),
                        ("converter", Modality.SCALAR),
                        ("color", Modality.COLOR),
                        ("filter", Modality.COLOR),
                        ("texture", Modality.ANALYSIS),
                        ("shader", Modality.DISPLAY),
                        ("distor", Modality.TRANSFORM),
                        ("matte", Modality.TRANSFORM),
                        ("attribute", Modality.DATA),
                        ("script", Modality.DATA),
                        ("group", Modality.DATA),
                        ("group_socket", Modality.DATA),
                    )
                },
                "frame_node": Paint(Surface.RECESS),
                **{
                    f"{kind}_zone": Paint(zone.border, Alpha.ZONE_FILL)
                    for kind, zone in (("simulation", Zone.SIMULATION), ("repeat", Zone.REPEAT), ("foreach_geometry_element", Zone.FOR_EACH), ("closure", Zone.CLOSURE))
                },
                "noodle_curving": 4,
                "grid_levels": 3,
                "dash_alpha": 0.5,
            },
        ),
        "outliner": (
            well_space,
            {
                **matched,
                "selected_highlight": Paint(Accent.ROW_SELECTED),
                "active": Paint(Accent.ROW_ACTIVE),
                "selected_object": Paint(Accent.ITEM_SELECTED),
                "active_object": Paint(Accent.ITEM_ACTIVE),
                "edited_object": Paint(Accent.INDICATOR, Alpha.TINT),
                **alternating,
            },
        ),
        "info": (
            well_space,
            {
                "info_selected": Paint(Accent.ROW_SELECTED),
                **dict.fromkeys(("info_selected_text", "info_error_text", "info_info_text", "info_debug_text", "info_property_text", "info_operator_text"), Paint(Text.PRIMARY)),
                "info_warning_text": Paint(Text.ON_SOLID),
                **dict.fromkeys(("info_debug", "info_property", "info_operator"), Paint(Status.INFO)),
            },
        ),
        "preferences": (panel_space, matched),
        "console": (
            well_space,
            {
                "line_output": Paint(Text.PRIMARY),
                **dict.fromkeys(("line_input", "line_info"), Paint(Text.SECONDARY)),
                "line_error": Paint(Text.ERROR),
                "cursor": Paint(Accent.ITEM_SELECTED),
                "select": Paint(Accent.TEXT_SELECTED),
            },
        ),
        "clip_editor": (
            Generic(back=Paint(Surface.CANVAS), text=Paint(Text.SECONDARY)),
            {
                "grid": Paint(Line.GRID, 0.0),
                "marker_outline": Paint(Surface.SHADOW),
                "marker": Paint(Text.SECONDARY),
                "active_marker": Paint(Selection.ACTIVE),
                "selected_marker": Paint(Selection.ITEM),
                "disabled_marker": Paint(Text.DISABLED),
                "locked_marker": Paint(Line.LOCKED),
                **dict.fromkeys(("path_before", "path_keyframe_before"), Paint(Line.BEFORE_FRAME)),
                **dict.fromkeys(("path_after", "path_keyframe_after"), Paint(Line.AFTER_FRAME)),
                **metadata,
            },
        ),
        "topbar": (Generic(back=frame, header=frame), {}),
        "statusbar": (Generic(back=frame, header=frame, text=Paint(Text.SECONDARY), header_text=Paint(Text.SECONDARY)), {}),
        "spreadsheet": (well_space, alternating),
    }
    members: dict[str, Target] = {
        "user_interface": {
            **{name: asdict(widget, recurse=False) for name, widget in widgets.items()},
            "wcol_state": {
                "error": Paint(Status.ERROR),
                "warning": Paint(Status.WARNING),
                "info": Paint(Status.INFO),
                "success": Paint(Status.SUCCESS),
                **dict.fromkeys(("inner_anim", "inner_anim_sel"), Paint(Field.ANIMATED)),
                **dict.fromkeys(("inner_key", "inner_key_sel"), Paint(Field.KEYED)),
                **dict.fromkeys(("inner_driven", "inner_driven_sel"), Paint(Field.DRIVEN)),
                **dict.fromkeys(("inner_overridden", "inner_overridden_sel"), Paint(Field.OVERRIDDEN)),
                **dict.fromkeys(("inner_changed", "inner_changed_sel"), Paint(Field.CHANGED)),
                "blend": 1.0,
            },
            "menu_shadow_width": 0,
            "icon_alpha": 1.0,
            "icon_saturation": 0.0,
            "widget_emboss": Paint(Surface.SHADOW, 0.0),
            "link": Paint(Accent.LINK),
            **dict.fromkeys(("editor_border", "editor_outline", "editor_outline_active"), frame),
            "widget_text_cursor": Paint(Accent.ITEM_SELECTED),
            "panel_roundness": ROUNDNESS,
            "panel_header": Paint(Surface.BOX),
            **dict.fromkeys(("panel_title", "panel_text"), Paint(Text.PRIMARY)),
            "panel_back": Paint(Surface.PANEL),
            "panel_sub_back": Paint(Surface.SHADOW, Alpha.VEIL),
            "panel_outline": Paint(Text.PRIMARY, Alpha.HAIRLINE),
            "panel_active": Paint(Accent.FOCUS),
            "transparent_checker_primary": Paint(Surface.BOX),
            "transparent_checker_secondary": Paint(Surface.PANEL),
            "transparent_checker_size": 10,
            "axis_x": Paint(Axis.X),
            "axis_y": Paint(Axis.Y),
            "axis_z": Paint(Axis.Z),
            "axis_w": Paint(Text.SECONDARY),
            "gizmo_hi": Paint(Selection.GIZMO_HOVER),
            **dict.fromkeys(("gizmo_primary", "gizmo_a"), Paint(Selection.GIZMO)),
            **dict.fromkeys(("gizmo_secondary", "gizmo_b"), Paint(Selection.GIZMO_SECONDARY)),
            "gizmo_view_align": Paint(Guide.HANDLE),
            **{
                f"icon_{kind}": Paint(modality.token)
                for kind, modality in (
                    ("scene", Modality.DATA),
                    ("folder", Modality.DATA),
                    ("object", Modality.TRANSFORM),
                    ("object_data", Modality.GEOMETRY),
                    ("modifier", Modality.VECTOR),
                    ("shading", Modality.DISPLAY),
                )
            },
            "icon_collection": Paint(Text.ICON),
            "icon_autokey": Paint(Accent.CHECKBOX_CHECKED),
            "icon_border_intensity": 0.0,
        },
        "regions": {
            "asset_shelf": {"back": Paint(Surface.PANEL), "header_back": frame},
            "channels": {"back": well, "text": Paint(Text.PRIMARY), "text_selected": Paint(Accent.ITEM_SELECTED)},
            "scrubbing": {"back": frame, "text": Paint(Text.SECONDARY), "time_marker": Paint(Text.SECONDARY), "time_marker_selected": Paint(Accent.ITEM_SELECTED)},
            "sidebars": {"back": Paint(Surface.PANEL), "tab_back": frame},
        },
        "common": {
            "anim": {
                "playhead": Paint(Accent.INDICATOR),
                "preview_range": Paint(Guide.CONSTRUCTION, Alpha.TINT),
                "scene_strip_range": Paint(Surface.SHADOW, Alpha.HALF),
                "channels": Paint(Surface.BOX),
                **dict.fromkeys(("channels_sub", "channel_group"), Paint(Surface.PANEL)),
                "channel_group_active": Paint(Accent.ROW_ACTIVE),
                "channel": well,
                "channel_selected": Paint(Accent.ROW_SELECTED),
                "keyframe": Paint(Text.SECONDARY),
                **dict.fromkeys(("keyframe_selected", "keyframe_extreme_selected", "keyframe_breakdown_selected", "keyframe_jitter_selected"), Paint(Selection.ITEM)),
                "keyframe_extreme": Paint(Line.KEY_EXTREME),
                "keyframe_breakdown": Paint(Line.KEY_BREAKDOWN),
                "keyframe_jitter": Paint(Line.KEY_JITTER),
                "keyframe_moving_hold": Paint(Line.LOCKED),
                **dict.fromkeys(("keyframe_moving_hold_selected", "keyframe_generated_selected"), Paint(Selection.INACTIVE)),
                "keyframe_generated": Paint(Text.DISABLED),
                "long_key": Paint(Text.PRIMARY, Alpha.VEIL),
                "long_key_selected": Paint(Selection.ITEM, Alpha.HOLD),
            },
            "curves": {
                **dict.fromkeys(("handle_free", "handle_auto", "handle_vect", "handle_align", "handle_auto_clamped"), Paint(Guide.HANDLE)),
                **dict.fromkeys(("handle_sel_free", "handle_sel_auto", "handle_sel_vect", "handle_sel_align", "handle_sel_auto_clamped"), Paint(Selection.ITEM)),
                "handle_vertex": screen,
                "handle_vertex_select": Paint(Selection.ITEM),
                "handle_vertex_size": round((POINT_WIDTH + 1) / (1.4 * scale)),
            },
        },
        **{editor: {"space": asdict(space, recurse=False), **rest} for editor, (space, rest) in editors.items()},
        "bone_color_sets": tuple(
            {"normal": Paint(tag.value), "select": Paint(Selection.ITEM), "active": Paint(Selection.ACTIVE), "show_colored_constraints": False}
            for _, tag in zip(theme.bone_color_sets, chain(Tag, repeat(Tag.NEUTRAL)), strict=False)
        ),
        **{name: tuple({"color": Paint(tag.value)} for _, tag in zip(getattr(theme, name), chain(Tag, repeat(Tag.NEUTRAL)), strict=False)) for name in ("collection_color", "strip_color")},
    }
    return dict(targets("", members))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Encoding", "Paint", "Target", "declared_theme"]
