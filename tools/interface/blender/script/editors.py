# ty: ignore[invalid-argument-type, invalid-return-type, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined"
"""Blender's editor spaces per workspace, from add-on filters and tools to views, studio lights, sidebars, navigation bars, and console history."""

from collections.abc import Iterator, Mapping
from functools import partial
from itertools import chain

import bpy
from mathutils import Vector

from interface.blender.rows import LOOK_DEVELOPMENT
from interface.blender.script.rna import assigned, Paint
from interface.blender.script.screens import area_label, CONSOLE_SIZE, Layout, leaves, MARGIN_COLUMN, override, paired, region_label, TEXT_SIZE, TICK, View, workspace_label
from interface.blender.script.startup import HEADLAMP, PLAN_DISTANCE
from interface.render import FRAME_SIZE, LENS
from interface.report import Action, changes, converged, Row, subscript
from interface.roles import Alpha, Ink, Line, Surface
from interface.units import GRID_THICK_EVERY, Units

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [VIEWS]
def canvas(workspace: bpy.types.WorkSpace, layout: Layout) -> bpy.types.Area:
    """Area of the layout's first 3D Viewport in tree order, the canvas its view, toolbar, and sidebar belong to."""
    return next(area for editor, area in paired(layout.screen, workspace.screens[0].areas[:]) if editor == "VIEW_3D")


def viewpoint(view: View, *, reach: bool, units: Units) -> dict[str, object]:
    """Declared view of a 3D Viewport by path: a lens framing a render-sized region as the camera frames the render, and the perspective over two thick grid cells, the plan over the modeling extent where it reaches that far, or the camera."""
    camera, (width, height) = bpy.types.Camera.bl_rna.properties, FRAME_SIZE
    distance_per_span = LENS * height / (camera["sensor_height"].default * width)
    match view:
        case View.PERSPECTIVE:
            projection, framed = (
                "PERSP",
                {
                    "region_3d.view_rotation": Vector((1.0, -1.0, 1.0)).to_track_quat("Z", "Y")[:],
                    "region_3d.view_location": (0.0, 0.0, 0.0),
                    "region_3d.view_distance": 2 * GRID_THICK_EVERY * units.grid * distance_per_span,
                },
            )
        case View.TOP:
            projection, framed = ("ORTHO", {"region_3d.view_location": (0.0, 0.0, 0.0), "region_3d.view_distance": 2 * units.extent * distance_per_span if reach else PLAN_DISTANCE})
        case View.CAMERA:
            projection, framed = "CAMERA", {}
    return {"lens": 2 * camera["sensor_width"].default * distance_per_span, "region_3d.view_perspective": projection, **framed}


def axis_view(window: bpy.types.Window, area: bpy.types.Area, axis: View) -> None:
    """Turn the area's view to the axis view, which marks it a side view."""
    with override(window, area, next(region for region in area.regions if region.type == "WINDOW")):
        bpy.ops.view3d.view_axis(type=axis)


def aligned(window: bpy.types.Window, layout: Layout) -> tuple[Row, ...]:
    """Row turning the canvas to the layout's axis view, none for a perspective or camera view."""
    match layout.view:
        case View.TOP:
            area = canvas(window.workspace, layout)
            view = area.spaces[0].region_3d
            return (
                Action(
                    label=f"{area_label(window.workspace, area)}.spaces[0].region_3d.is_orthographic_side_view",
                    read=lambda: view.is_orthographic_side_view,
                    act=partial(axis_view, window, area, layout.view),
                    target=True,
                ),
            )
        case View.PERSPECTIVE | View.CAMERA:
            return ()


def fitted_cameras(window: bpy.types.Window) -> Iterator[tuple[str, bpy.types.Space, Mapping[str, object]]]:
    """Each camera view's space on screen by label with the zoom and offset Frame Camera Bounds gives it, the view held for the read."""
    for area in [area for area in window.screen.areas if area.type == "VIEW_3D" and area.spaces[0].region_3d.view_perspective == "CAMERA"]:
        space, view = area.spaces[0], area.spaces[0].region_3d
        with (
            assigned((view, "view_camera_zoom", view.view_camera_zoom), (view, "view_camera_offset", view.view_camera_offset)),
            override(window, area, next(region for region in area.regions if region.type == "WINDOW")),
        ):
            bpy.ops.view3d.view_center_camera()
            fitted = {"region_3d.view_camera_zoom": view.view_camera_zoom, "region_3d.view_camera_offset": tuple(view.view_camera_offset)}
        yield f"{area_label(window.workspace, area)}.spaces[0]", space, fitted


def slots(workspace: bpy.types.WorkSpace) -> Iterator[tuple[str, bpy.types.View3DShading, str, str, str]]:
    """Studio light slot of each workspace 3D Viewport with its label, shading, selecting type and light kind, and declared light."""
    lights = (("SOLID", "MATCAP", "check_reflection_horizontal.exr"), ("SOLID", "STUDIO", HEADLAMP.name), ("MATERIAL", "STUDIO", LOOK_DEVELOPMENT.name))
    yield from (
        (subscript(f"{area_label(workspace, area)}.spaces[0].shading", kind, light), view.shading, kind, light, name)
        for area in workspace.screens[0].areas
        if isinstance(view := area.spaces[0], bpy.types.SpaceView3D)
        for kind, light, name in lights
    )


# --- [WORKSPACE]
def owned(workspace: bpy.types.WorkSpace, owners: tuple[str, ...]) -> None:
    """Replace the workspace's add-on owners with the modules."""
    workspace.owner_ids.clear()
    for module in owners:
        workspace.owner_ids.new(module)


def filtered(workspace: bpy.types.WorkSpace, placement: Mapping[str, tuple[str, ...]], modules: Mapping[str, str]) -> Row:
    """Row filtering the workspace to every enabled add-on its package row places here or nowhere."""
    placed_elsewhere = {module for name, module in modules.items() if placement.get(name) and workspace.name not in placement[name]}
    return Row(
        label=f"{workspace_label(workspace)}.owner_ids",
        read=lambda: tuple(sorted(owner.name for owner in workspace.owner_ids)),
        write=partial(owned, workspace),
        target=tuple(sorted(set(modules.values()) - placed_elsewhere)),
    )


def shelf(space: bpy.types.SpaceView3D | bpy.types.SpaceImageEditor | bpy.types.SpaceNodeEditor, *, shown: bool) -> dict[str, bool]:
    """Asset shelf row of a space where a shelf type polls, none where Blender holds the flag read-only."""
    return {} if space.is_property_readonly("show_region_asset_shelf") else {"show_region_asset_shelf": shown}


def spaces(
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
    units: Units,
    *,
    editor: str,
    main: bool,
) -> dict[str, object]:
    """Declared values of one editor's space by path, the layout's and unit system's values in place, `main` marking the canvas."""
    match space:
        case bpy.types.SpaceView3D():
            return {
                "clip_start": units.snap,
                "clip_end": units.far,
                "show_region_tool_header": False,
                "show_region_toolbar": main,
                "show_region_ui": main and layout.sidebar is not None,
                **shelf(space, shown=editor in layout.shelf),
                "show_region_header": True,
                "use_local_collections": leaves(layout.screen).count("VIEW_3D") > 1,
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
                "shading.single_color": Paint(Surface.SHADED),
                "shading.object_outline_color": Paint(Ink.SCREEN),
                "overlay.gpencil_grid_color": Paint(Line.GRID),
                **dict.fromkeys((f"shading.{name}" for name in ("show_object_outline", "use_scene_world_render", "use_scene_lights_render")), True),
                **dict.fromkeys(
                    (
                        f"shading.{name}"
                        for name in ("use_world_space_lighting", "use_scene_world", "use_scene_lights", "show_specular_highlight", "show_shadows", "show_xray", "show_backface_culling")
                    ),
                    False,
                ),
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
                "region_3d.lock_rotation": False,
                **viewpoint(layout.view if main else View.CAMERA, reach=layout.reach, units=units),
            }
        case bpy.types.SpaceImageEditor():
            return {
                "show_region_header": True,
                "show_region_tool_header": False,
                "show_region_toolbar": True,
                "show_region_ui": False,
                **shelf(space, shown=editor in layout.shelf),
                "use_image_pin": False,
                **dict.fromkeys(("show_gizmo", "show_annotation", "overlay.show_overlays"), True),
                **({"uv_editor.edge_display_type": "WHITE"} if editor == "UV" else {"ui_mode": "VIEW", "display_channels": "COLOR_ALPHA"}),
            }
        case bpy.types.SpaceNodeEditor():
            trees = {"GeometryNodeTree": {"node_tree_sub_type": "MODIFIER"}, "ShaderNodeTree": {"shader_type": "OBJECT"}, "CompositorNodeTree": {"node_tree_sub_type": "SCENE"}}
            return {
                "show_region_header": True,
                "show_region_toolbar": True,
                "show_region_ui": False,
                **shelf(space, shown=editor in layout.shelf),
                **dict.fromkeys(("pin", "overlay.show_timing", "overlay.show_wire_color"), False),
                **dict.fromkeys(
                    ("show_annotation", "overlay.show_overlays", "overlay.show_reroute_auto_labels", "overlay.show_context_path", "overlay.show_named_attributes", "overlay.show_previews"), True
                ),
                **trees[editor],
            }
        case bpy.types.SpaceSpreadsheet():
            return {
                "show_region_header": True,
                "show_region_toolbar": False,
                "show_region_footer": False,
                "show_region_ui": False,
                "show_internal_attributes": False,
                "use_filter": True,
                "show_only_selected": False,
            }
        case bpy.types.SpaceConsole():
            return {"font_size": CONSOLE_SIZE}
        case bpy.types.SpaceInfo():
            return {"show_region_header": True}
        case bpy.types.SpaceTextEditor():
            return {
                "show_region_header": True,
                "show_region_footer": False,
                "show_region_ui": False,
                **dict.fromkeys(("show_word_wrap", "show_line_highlight", "show_margin", "use_live_edit", "use_match_case", "use_find_all"), False),
                **dict.fromkeys(("show_line_numbers", "show_syntax_highlight", "use_find_wrap"), True),
                "font_size": TEXT_SIZE,
                "tab_width": 4,
                "margin_column": MARGIN_COLUMN,
            }
        case bpy.types.SpaceProperties():
            return {
                "context": layout.context,
                "show_region_header": True,
                "use_pin_id": False,
                "outliner_sync": "AUTO",
                **dict.fromkeys((f"show_properties_{kind}" for kind in ("particles", "physics", "effects", "strip", "strip_modifier")), False),
            }
        case bpy.types.SpaceOutliner():
            return {
                "display_mode": "VIEW_LAYER",
                "filter_text": "",
                "filter_state": "ALL",
                "sort_method": "ALPHA",
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


def declared_workspace(workspace: bpy.types.WorkSpace, layout: Layout, units: Units) -> "tuple[tuple[str, bpy.types.bpy_struct[object], Mapping[str, object]], ...]":
    """Declared values of the workspace, its X-Ray select tools, areas, spaces, and Bonsai tabs, by report label, owner, and path."""
    screen, owner, tools = workspace.screens[0], workspace_label(workspace), {"OBJECT": "object_tool.select_box_xray", "EDIT_MESH": "mesh_tool.select_box_xray"}
    pairs, main = paired(layout.screen, screen.areas[:]), canvas(workspace, layout)
    return (
        (owner, workspace, {"use_pin_scene": False, "use_filter_by_owner": True, "screens[0].name": workspace.name, "screens[0].show_statusbar": True}),
        *((subscript(f"{owner}.tools", mode), workspace.tools.from_space_view3d_mode(mode, create=True), {"idname": tool}) for mode, tool in tools.items()),
        *(
            row
            for editor, area in pairs
            for label in (area_label(workspace, area),)
            for row in ((label, area, {"show_menus": True}), (f"{label}.spaces[0]", area.spaces[0], spaces(area.spaces[0], layout, units, editor=editor, main=area == main)))
        ),
        *((f"{owner}.screens[0]", screen, {f"BIMAreaProperties[{index}].tab": layout.tab}) for index, area in enumerate(screen.areas) if area.type == "PROPERTIES"),
    )


# --- [REGIONS]
def sidebars(window: bpy.types.Window, layout: Layout) -> tuple[tuple[str, bpy.types.Region, Mapping[str, object]], ...]:
    """Canvas sidebar's declared tab by report label, owner, and path, none for a layout without one; it converges one pass after the declared show, once the region drew its categories."""
    match layout.sidebar:
        case str() as tab:
            area = canvas(window.workspace, layout)
            region = next(region for region in area.regions if region.type == "UI")
            return ((region_label(window.workspace, area, region), region, {"active_panel_category": tab}),)
        case None:
            return ()


def flipped(window: bpy.types.Window, area: bpy.types.Area, bar: bpy.types.Region) -> None:
    """Flip the navigation bar to the area's other side."""
    with override(window, area, bar):
        bpy.ops.screen.region_flip()


def navigation_bar(window: bpy.types.Window, area: bpy.types.Area, bar: bpy.types.Region) -> Iterator[float | str]:
    """Show the Properties editor's tab column, hidden while Blender reports it one pixel wide or tall, one pass before converging it onto the right side, the window's outer edge of the right column."""
    label = region_label(window.workspace, area, bar)
    if min(bar.width, bar.height) <= 1:
        with override(window, area):
            bpy.ops.screen.region_toggle(region_type=bar.type)
        yield TICK
        yield from changes(label, "hidden", "shown")
    yield from converged(Action(label=f"{label}.alignment", read=lambda: bar.alignment, act=partial(flipped, window, area, bar), target="RIGHT"))


def navigation_bars(window: bpy.types.Window) -> Iterator[float | str]:
    """Each Properties editor's tab column shown and aligned in turn."""
    return chain.from_iterable([navigation_bar(window, area, bar) for area in window.screen.areas if area.type == "PROPERTIES" for bar in area.regions if bar.type == "NAVIGATION_BAR"])


def typed(space: bpy.types.SpaceConsole) -> tuple[str, ...]:
    """Typed lines of the console history."""
    return tuple(entry.body for entry in space.history if entry.body)


def emptied(window: bpy.types.Window, area: bpy.types.Area) -> None:
    """Clear the console's typed history and keep its scrollback."""
    with override(window, area, next(region for region in area.regions if region.type == "WINDOW")):
        bpy.ops.console.clear(scrollback=False, history=True)


def cleared_history(window: bpy.types.Window) -> tuple[Row, ...]:
    """Row emptying each console's typed history on screen, which the startup save would otherwise store."""
    return tuple(
        Action(label=f"{area_label(window.workspace, area)}.spaces[0].history", read=partial(typed, area.spaces[0]), act=partial(emptied, window, area), target=())
        for area in window.screen.areas
        if area.type == "CONSOLE"
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["aligned", "cleared_history", "declared_workspace", "filtered", "fitted_cameras", "navigation_bars", "sidebars", "slots", "spaces"]
