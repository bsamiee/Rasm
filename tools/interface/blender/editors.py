# ty: ignore[invalid-argument-type, invalid-assignment, invalid-context-manager, redundant-condition-strict, unresolved-attribute]
# mypy: disable-error-code="arg-type, assignment, attr-defined, union-attr"
"""Blender's editor settings per workspace, from add-on filters and tools to views, studio lights, sidebars, and Properties tab columns."""

from collections.abc import Iterator, Mapping
from contextlib import contextmanager
from enum import StrEnum
from typing import Final

import bpy
from mathutils import Vector

from interface.blender.screens import Below, CONSOLE_SIZE, Layout, MARGIN_COLUMN, names, override, TEXT_SIZE, toggle, until
from interface.blender.startup import PLAN_DISTANCE
from interface.blender.theme import Encoding, Paint
from interface.render import LENS, LOOK_DEVELOPMENT
from interface.report import ABSENT, Kind, line
from interface.roles import Alpha, Ink, Line, Surface
from interface.units import GRID_THICK_EVERY, Units

# --- [TYPES] ----------------------------------------------------------------------------


class Region(StrEnum):
    """Space member that shows or hides one editor region."""

    HEADER = "show_region_header"
    TOOL_HEADER = "show_region_tool_header"
    TOOLBAR = "show_region_toolbar"
    UI = "show_region_ui"
    ASSET_SHELF = "show_region_asset_shelf"
    FOOTER = "show_region_footer"


# --- [CONSTANTS] ------------------------------------------------------------------------

MODES: Final = ("OBJECT", "EDIT_MESH")

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [VIEWS]
def top_area(screen: bpy.types.Screen, editor: str) -> bpy.types.Area:
    """Screen's top area of the editor's ui_type."""
    return max((area for area in screen.areas if area.ui_type == editor), key=lambda area: area.y)


def viewpoint(screen: bpy.types.Screen, area: bpy.types.Area, layout: Layout) -> dict[str, object]:
    """Declared view of a 3D Viewport by path, the layout's view in the top one and the camera below it."""
    distance_per_span = LENS / bpy.types.Camera.bl_rna.properties["sensor_width"].default
    match layout.view if area == top_area(screen, "VIEW_3D") else "CAMERA":
        case None:
            projection, placed = (
                "PERSP",
                {
                    "region_3d.view_rotation": Vector((1.0, -1.0, 1.0)).to_track_quat("Z", "Y")[:],
                    "region_3d.view_location": (0.0, 0.0, 0.0),
                    "region_3d.view_distance": 2 * GRID_THICK_EVERY * Units.IMPERIAL.grid * distance_per_span,
                },
            )
        case "CAMERA":
            projection, placed = "CAMERA", {}
        case _:
            projection, placed = ("ORTHO", {"region_3d.view_location": (0.0, 0.0, 0.0), "region_3d.view_distance": PLAN_DISTANCE if layout.reach is None else 2 * layout.reach * distance_per_span})
    return {"region_3d.view_perspective": projection, **placed}


def slots(workspace: bpy.types.WorkSpace) -> Iterator[tuple[str, bpy.types.View3DShading, str, str, str]]:
    """Studio light slot of each workspace 3D Viewport with its label, shading, selecting type and light kind, and declared light."""
    labels = names(workspace.screens[0])
    lights = (("SOLID", "MATCAP", "check_reflection_horizontal.exr"), ("SOLID", "STUDIO", "Headlamp.sl"), ("MATERIAL", "STUDIO", LOOK_DEVELOPMENT.name))
    yield from (
        (f"{workspace.name}.{labels[area.as_pointer()]}.shading[{kind}.{light}]", view.shading, kind, light, name)
        for area in workspace.screens[0].areas
        if isinstance(view := area.spaces[0], bpy.types.SpaceView3D)
        for kind, light, name in lights
    )


@contextmanager
def studio_slot(shading: bpy.types.View3DShading, kind: str, light: str) -> Iterator[None]:
    """Select the shading type and light kind for the block and restore the shading's own after it."""
    held = shading.type, shading.light
    shading.type, shading.light = kind, light
    try:
        yield
    finally:
        shading.type, shading.light = held


def align_view(window: bpy.types.Window, layout: Layout) -> Iterator[str]:
    """Release the rotation lock of every 3D view on screen and turn the top one to the layout's axis view."""
    area, axis = top_area(window.screen, "VIEW_3D"), layout.view
    for space in [each.spaces.active for each in window.screen.areas if each.type == "VIEW_3D"]:
        space.region_3d.lock_rotation = False
    if axis is None or axis == "CAMERA" or area.spaces.active.region_3d.is_orthographic_side_view:
        return
    with override(window, area, next(region for region in area.regions if region.type == "WINDOW")):
        bpy.ops.view3d.view_axis(type=axis)
    yield line(Kind.CHANGE, f"{window.workspace.name}.{names(window.screen)[area.as_pointer()]}.region_3d.is_orthographic_side_view", "False", "True")


def fitted_cameras(window: bpy.types.Window) -> Iterator[tuple[str, bpy.types.bpy_struct, Mapping[str, object]]]:
    """Each camera view's space on screen by label with the zoom and offset Frame Camera Bounds gives it, the view restored after the read."""
    labels = names(window.screen)
    for area in [area for area in window.screen.areas if area.type == "VIEW_3D" and area.spaces[0].region_3d.view_perspective == "CAMERA"]:
        space, view = area.spaces[0], area.spaces[0].region_3d
        held = view.view_camera_zoom, tuple(view.view_camera_offset), view.lock_rotation
        view.lock_rotation = False
        with override(window, area, next(region for region in area.regions if region.type == "WINDOW")):
            bpy.ops.view3d.view_center_camera()
        fitted = {"region_3d.view_camera_zoom": view.view_camera_zoom, "region_3d.view_camera_offset": tuple(view.view_camera_offset)}
        view.view_camera_zoom, view.view_camera_offset, view.lock_rotation = held
        yield f"{window.workspace.name}.{labels[area.as_pointer()]}", space, fitted


# --- [WORKSPACE]
def filter_workspace(workspace: bpy.types.WorkSpace, placement: Mapping[str, tuple[str, ...]], modules: Mapping[str, str]) -> Iterator[str]:
    """Create the workspace's missing 3D Viewport tool records and filter it to every enabled add-on its package row places here or nowhere."""
    for mode in [mode for mode in MODES if workspace.tools.from_space_view3d_mode(mode, create=False) is None]:
        workspace.tools.from_space_view3d_mode(mode, create=True)
        yield line(Kind.CHANGE, f"{workspace.name}.tools.{mode}", ABSENT, "created")
    owners = sorted(set(bpy.context.preferences.addons.keys()) - {module for name, module in modules.items() if placement.get(name) and workspace.name not in placement[name]})
    if (before := sorted(owner.name for owner in workspace.owner_ids)) == owners:
        return
    workspace.owner_ids.clear()
    for module in owners:
        workspace.owner_ids.new(module)
    yield line(Kind.CHANGE, f"{workspace.name}.owner_ids", repr(before), repr(owners))


def declared_workspace(workspace: bpy.types.WorkSpace, layout: Layout, modules: Mapping[str, str]) -> Iterator[tuple[str, bpy.types.bpy_struct, Mapping[str, object]]]:
    """Declared values of the workspace, its screen, tools, areas, spaces, and Bonsai tabs, by report label, owner, and path."""
    screen, labels = workspace.screens[0], names(workspace.screens[0])
    shown, top = None if layout.sidebar is None else top_area(screen, layout.sidebar[0]), top_area(screen, "VIEW_3D")
    yield workspace.name, workspace, {"use_pin_scene": False, "use_filter_by_owner": True, "screens[0].name": workspace.name, "screens[0].show_statusbar": True}
    select = {"OBJECT": "object_tool.select_box_xray", "EDIT_MESH": "mesh_tool.select_box_xray"} if "xray_selection_tools" in modules else dict.fromkeys(MODES, "builtin.select_box")
    yield from ((f"{workspace.name}.tools.{mode}", workspace.tools.from_space_view3d_mode(mode, create=False), {"idname": select[mode]}) for mode in MODES)
    for index, area in enumerate(screen.areas):
        label, space = f"{workspace.name}.{labels[area.as_pointer()]}", area.spaces[0]
        yield label, area, {"show_menus": True}
        yield (
            label,
            space,
            settings(
                space,
                layout,
                view=viewpoint(screen, area, layout),
                sidebar=area == shown,
                top=area == top,
                shelf={Region.ASSET_SHELF: area.ui_type in layout.shelf} if Region.ASSET_SHELF in space.bl_rna.properties and not space.is_property_readonly(Region.ASSET_SHELF) else {},
            ),
        )
        if area.type == "PROPERTIES" and "bonsai" in modules:
            yield label, screen, {f"BIMAreaProperties[{index}].tab": layout.tab}


def settings(
    space: bpy.types.SpaceView3D
    | bpy.types.SpaceImageEditor
    | bpy.types.SpaceNodeEditor
    | bpy.types.SpaceSpreadsheet
    | bpy.types.SpaceConsole
    | bpy.types.SpaceInfo
    | bpy.types.SpaceTextEditor
    | bpy.types.SpaceProperties
    | bpy.types.SpaceOutliner,
    layout: Layout,
    *,
    view: dict[str, object],
    sidebar: bool,
    top: bool,
    shelf: dict[str, object],
) -> dict[str, object]:
    """Declared values of one editor's space by path."""
    match space:
        case bpy.types.SpaceView3D():
            return {
                "lens": 2 * LENS,
                "clip_start": Units.IMPERIAL.snap,
                "clip_end": Units.IMPERIAL.far,
                Region.TOOL_HEADER: False,
                Region.TOOLBAR: top,
                Region.UI: sidebar,
                **shelf,
                Region.HEADER: True,
                "use_local_collections": any(Below.SHEET in row for row in layout.rows[1:]),
                **dict.fromkeys(
                    (
                        f"show_gizmo_{kind}"
                        for kind in ("empty_image", "empty_force_field", "light_size", "light_look_at", "camera_lens", "camera_dof_distance", "object_translate", "object_rotate", "object_scale")
                    ),
                    True,
                ),
                "show_reconstruction": False,
                "shading.type": layout.shading,
                "shading.light": "STUDIO",
                "shading.background_type": "THEME",
                "shading.color_type": layout.color_type,
                "shading.wireframe_color_type": "THEME",
                "shading.show_cavity": False,
                "shading.single_color": Paint(Surface.SHADED, encoding=Encoding.LINEAR),
                "shading.object_outline_color": Paint(Ink.SCREEN, encoding=Encoding.LINEAR),
                "shading.background_color": Paint(Surface.CANVAS, encoding=Encoding.LINEAR),
                "overlay.gpencil_grid_color": Paint(Line.GRID, encoding=Encoding.LINEAR),
                **dict.fromkeys((f"shading.{name}" for name in ("show_object_outline", "use_scene_world_render", "use_scene_lights_render")), True),
                **dict.fromkeys((f"shading.{name}" for name in ("use_world_space_lighting", "show_specular_highlight", "show_shadows", "show_xray", "show_backface_culling")), False),
                "shading.studiolight_background_alpha": 0.0,
                "shading.studiolight_background_blur": 0.5,
                "shading.xray_alpha_wireframe": 0.0,
                **dict.fromkeys(
                    (
                        f"overlay.show_{name}"
                        for name in (
                            "text",
                            "floor",
                            "ortho_grid",
                            "outline_selected",
                            "wireframes",
                            "axis_x",
                            "axis_y",
                            "extras",
                            "cursor",
                            "object_origins",
                            "bones",
                            "motion_paths",
                            "annotation",
                            "relationship_lines",
                            "edge_seams",
                            "edge_sharp",
                            "edge_crease",
                            "edge_bevel_weight",
                            "freestyle_edge_marks",
                            "freestyle_face_marks",
                            "viewer_attribute",
                            "camera_guides",
                            "camera_passepartout",
                        )
                    ),
                    True,
                ),
                **dict.fromkeys(
                    (
                        f"overlay.show_{name}"
                        for name in (
                            "axis_z",
                            "look_dev",
                            "face_orientation",
                            "stats",
                            "object_origins_all",
                            "fade_inactive",
                            "face_center",
                            "statvis",
                            "light_colors",
                            "extra_edge_length",
                            "extra_edge_angle",
                            "extra_face_angle",
                            "extra_face_area",
                            "extra_indices",
                        )
                    ),
                    False,
                ),
                "overlay.wireframe_threshold": 0.0,
                "overlay.wireframe_opacity": 1.0,
                "overlay.normals_length": 0.1,
                "overlay.xray_alpha_bone": 0.0,
                "overlay.gpencil_grid_opacity": Alpha.GRID_MINOR,
                "overlay.gpencil_fade_layer": 0.5,
                "overlay.grid_lines": Units.IMPERIAL.grid_lines,
                **view,
            }
        case bpy.types.SpaceImageEditor():
            return {
                **dict.fromkeys((Region.TOOL_HEADER, Region.UI, "use_image_pin"), False),
                **dict.fromkeys((Region.HEADER, Region.TOOLBAR, "show_gizmo", "show_annotation", "overlay.show_overlays"), True),
                **shelf,
                **(
                    {"uv_editor.edge_display_type": "WHITE"}
                    if space.mode == "UV"
                    else {"image": next(image for image in bpy.data.images if image.type == "RENDER_RESULT"), "ui_mode": "VIEW", "display_channels": "COLOR_ALPHA"}
                ),
            }
        case bpy.types.SpaceNodeEditor():
            trees = {"GeometryNodeTree": {"node_tree_sub_type": "MODIFIER"}, "ShaderNodeTree": {"shader_type": "OBJECT"}, "CompositorNodeTree": {"node_tree_sub_type": "SCENE"}}
            return {
                **dict.fromkeys(("pin", "overlay.show_timing", "overlay.show_wire_color"), False),
                **dict.fromkeys(
                    (
                        Region.HEADER,
                        Region.TOOLBAR,
                        "show_annotation",
                        "overlay.show_overlays",
                        "overlay.show_reroute_auto_labels",
                        "overlay.show_context_path",
                        "overlay.show_named_attributes",
                        "overlay.show_previews",
                    ),
                    True,
                ),
                Region.UI: sidebar,
                **shelf,
                **trees[space.tree_type],
            }
        case bpy.types.SpaceSpreadsheet():
            return {Region.HEADER: True, Region.TOOLBAR: False, Region.FOOTER: False, Region.UI: False, "show_internal_attributes": False, "use_filter": True, "show_only_selected": False}
        case bpy.types.SpaceConsole():
            return {"font_size": CONSOLE_SIZE}
        case bpy.types.SpaceInfo():
            return {Region.HEADER: True}
        case bpy.types.SpaceTextEditor():
            return {
                **dict.fromkeys((Region.FOOTER, Region.UI, "show_word_wrap", "show_line_highlight", "show_margin", "use_live_edit", "use_match_case", "use_find_all"), False),
                **dict.fromkeys((Region.HEADER, "show_line_numbers", "show_syntax_highlight", "use_find_wrap"), True),
                "font_size": TEXT_SIZE,
                "tab_width": 4,
                "margin_column": MARGIN_COLUMN,
            }
        case bpy.types.SpaceProperties():
            return {
                "context": layout.context,
                Region.HEADER: True,
                "use_pin_id": False,
                "outliner_sync": "AUTO",
                **dict.fromkeys((f"show_properties_{kind}" for kind in ("particles", "physics", "effects", "strip", "strip_modifier")), False),
            }
        case bpy.types.SpaceOutliner():
            return {
                "display_mode": "VIEW_LAYER",
                "filter_text": "",
                "filter_state": "ALL",
                **dict.fromkeys(
                    (
                        "use_filter_complete",
                        "use_filter_case_sensitive",
                        "filter_invert",
                        "use_filter_view_layers",
                        "show_restrict_column_holdout",
                        "show_restrict_column_indirect_only",
                        "use_filter_object_content",
                    ),
                    False,
                ),
                **dict.fromkeys(
                    (
                        "use_sort_alpha",
                        "use_sync_select",
                        "show_mode_column",
                        "use_filter_children",
                        "scroll_to_active",
                        *(f"show_restrict_column_{kind}" for kind in ("enable", "select", "hide", "viewport", "render")),
                        "use_filter_collection",
                        "use_filter_object",
                        *(f"use_filter_object_{kind}" for kind in ("mesh", "armature", "empty", "light", "camera", "grease_pencil", "others")),
                    ),
                    True,
                ),
            }


# --- [REGIONS]
def sidebars(window: bpy.types.Window, layout: Layout) -> Iterator[tuple[str, bpy.types.bpy_struct, Mapping[str, object]] | float | str]:
    """Layout's sidebar region with its label and declared tab once drawn, or a skip line when no panel draws in the tab."""
    match layout.sidebar:
        case editor, tab:
            area = top_area(window.screen, editor)
            region, label = next(region for region in area.regions if region.type == "UI"), f"{window.workspace.name}.{names(window.screen)[area.as_pointer()]}.UI"
            yield from until(lambda: region.active_panel_category != "UNSUPPORTED", f"{label}.draw")
            yield (label, region, {"active_panel_category": tab}) if bpy.types.UILayout.enum_item_name(region, "active_panel_category", tab) else line(Kind.SKIP, f"{label}.{tab}")
        case None:
            pass


def reveal(window: bpy.types.Window) -> Iterator[float | str]:
    """Show each Properties editor's tab column on the window's outer edge and clear the typed console history."""

    def typed(space: bpy.types.SpaceConsole) -> bool:
        """Whether the console history holds a typed line."""
        return any(entry.body for entry in space.history)

    labels, right = names(window.screen), max(area.x + area.width for area in window.screen.areas)
    regions = [(area, region, f"{window.workspace.name}.{labels[area.as_pointer()]}.{region.type}") for area in window.screen.areas for region in area.regions]
    for area, bar, label in [(area, region, label) for area, region, label in regions if area.type == "PROPERTIES" and region.type == "NAVIGATION_BAR"]:
        outer = "RIGHT" if area.x + area.width == right else "LEFT"
        if bar.width <= 1:
            yield from toggle(window, area, bar, label)
            yield line(Kind.CHANGE, label, "hidden", "shown")
        if (before := bar.alignment) != outer:
            with override(window, area, bar):
                bpy.ops.screen.region_flip()
            yield line(Kind.CHANGE, f"{label}.alignment", before, outer)
    for area, region, label in [(area, region, label) for area, region, label in regions if area.type == "CONSOLE" and region.type == "WINDOW" and typed(area.spaces.active)]:
        with override(window, area, region):
            bpy.ops.console.clear(scrollback=False, history=True)
        yield line(Kind.CHANGE, f"{label}.history", "typed", "cleared")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["align_view", "declared_workspace", "filter_workspace", "fitted_cameras", "reveal", "sidebars", "slots", "studio_slot"]
