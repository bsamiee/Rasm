# ty: ignore[invalid-argument-type, invalid-return-type, unresolved-attribute, unresolved-import]
# mypy: disable-error-code="attr-defined, import-not-found, return-value, union-attr"
# ruff: file-ignore[import-private-name]
"""Blender add-on settings by owner, MeasureIt_ARCH styles they name, and operator and camera presets in Blender's preset layout."""

from collections.abc import Callable, Iterator, Mapping
from datetime import timedelta
from functools import partial
from importlib import import_module
from itertools import chain, starmap
from math import radians
from operator import setitem
from pathlib import Path
import sys
from types import ModuleType
from typing import Final, TYPE_CHECKING
from urllib.parse import urlsplit

from _bpy import ops
from bl_operators.presets import AddPresetOperator
import bpy

from interface.blender.rows import JSON, Launch
from interface.blender.script.rna import converge_groups, held, Paint
from interface.blender.script.startup import ANALYSIS, PLAN_DISTANCE, sun, typed_node
from interface.render import DAYLIGHT, DPI, LATITUDE, LONGITUDE, MATERIALS, MOMENT, NORTH, OFFSET
from interface.report import converged, Row, subscript
from interface.roles import Alpha, Annotation, Guide, Ink, Line, Modality, POINT_WIDTH, Selection, Status, Surface, Text
from interface.units import ANGLE_PRECISION, Length, Units

if TYPE_CHECKING:
    from interface.blender.extension.unit_system import Group

# --- [CONSTANTS] ------------------------------------------------------------------------

DIMENSION_STYLE: Final = "Dimension Style 1"
ANNOTATION_STYLE: Final = "Annotation Style 1"

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [SITE]
def site_crs() -> tuple[int, str]:
    """Site's UTM zone and EPSG code of its projected system."""
    zone = int((LONGITUDE + 180) // 6) + 1
    return zone, f"EPSG:{(32600 if LATITUDE >= 0 else 32700) + zone}"


def georeference(scene: bpy.types.Scene, modules: Mapping[str, str]) -> Row:
    """Row of BlenderGIS scene georeference, site system written before origin in degrees that `setOriginGeo` projects into it."""
    geo, (_, crs) = import_module(f"{modules['BlenderGIS']}.geoscene").GeoScene(scene), site_crs()

    def georeferenced(target: tuple[str, float, float]) -> None:
        projection, longitude, latitude = target
        geo.crs = projection
        geo.setOriginGeo(longitude, latitude)

    return Row(label="BlenderGIS.georeference", read=lambda: (geo.crs, geo.lon, geo.lat), write=georeferenced, target=(crs, LONGITUDE, LATITUDE))


# --- [SETTINGS]
def settings(unit_system: ModuleType, preferences: bpy.types.Preferences, scene: bpy.types.Scene, launch: Launch, modules: Mapping[str, str]) -> "tuple[Group, ...]":
    """Groups of every add-on's settings by owner in the order update callbacks need, item-written groups assigned through `setitem` and Sun Position's with the working scene as context scene its updates move."""

    def addon(name: str) -> tuple[str, bpy.types.AddonPreferences]:
        return f"{subscript('preferences.addons', modules[name])}.preferences", preferences.addons[modules[name]].preferences

    def placed(struct: "bpy.types.bpy_struct[object]", name: str, value: object) -> None:
        with bpy.context.temp_override(scene=scene):
            setattr(struct, name, value)

    working, analysis = (subscript("scenes", scene.name), scene), (subscript("scenes", ANALYSIS), bpy.data.scenes[ANALYSIS])
    overpass, grid = "https://overpass-api.de/api/interpreter", 20
    dem = "https://elevation.nationalmap.gov/arcgis/rest/services/3DEPElevation/ImageServer/exportImage?bbox={W},{S},{E},{N}&bboxSR=4326&imageSR=4326&size=1024,1024&format=tiff&pixelType=F32&interpolation=RSP_BilinearInterpolation&f=image"
    services = ("sketchfab_api_key", "polypizza_api_key", "hyper3d_api_key", "hunyuan3d_secret_id", "hunyuan3d_secret_key")
    drawing, cache, host = preferences.view.font_path_ui, Path(bpy.app.cachedir), urlsplit(overpass).hostname
    denominator, places = str(2 ** Units.IMPERIAL.places(Length.INCHES, 2)), Units.METRIC.places(Length.MILLIMETERS, 10)
    size, (zone, crs) = round(preferences.ui_styles[0].widget.points * preferences.system.ui_scale), site_crs()
    standard, daylight = (value / timedelta(hours=1) for value in (OFFSET, DAYLIGHT))
    gis, blosm, active_point, point_factor = import_module(f"{modules['BlenderGIS']}.prefs"), import_module(f"{modules['Blosm']}.app.blender").BlenderApp, 5, 0.75
    doc, font = preferences.addons[modules["bonsai"]].preferences.doc.bl_rna.properties, next(font for font in bpy.data.fonts if font.filepath == drawing)
    return tuple(
        starmap(
            unit_system.Group,
            (
                (
                    *addon("Sverchok"),
                    {
                        **dict.fromkeys(("log_to_buffer", "auto_apply_theme", "apply_theme_on_open"), False),
                        "log_level": "WARNING",
                        "exception_color": Paint(Status.ERROR),
                        "no_data_color": Paint(Status.INFO),
                        **{f"color_{kind}": Paint(Surface.PANEL) for kind in ("viz", "tex", "sce", "lay", "gen")},
                    },
                ),
                (
                    *addon("CAD_Sketcher"),
                    {
                        "imperial_precision": denominator,
                        "decimal_precision": places,
                        "angle_precision": ANGLE_PRECISION,
                        "show_whats_new": False,
                        "entity_scale": POINT_WIDTH / (active_point * point_factor * preferences.system.ui_scale),
                        "text_size": size,
                        **{
                            f"theme_settings.{path}": Paint(rgb, display=True)
                            for path, rgb in (
                                ("entity.default", Ink.SCREEN),
                                ("entity.highlight", Selection.HOVER),
                                ("entity.selected", Selection.ITEM),
                                ("entity.selected_highlight", Selection.HOVER),
                                ("entity.inactive", Line.LOCKED),
                                ("entity.inactive_selected", Selection.INACTIVE),
                                ("entity.fixed", Line.LOCKED),
                                ("constraint.default", Ink.SCREEN),
                                ("constraint.highlight", Selection.HOVER),
                                ("constraint.failed", Text.ERROR),
                                ("constraint.failed_highlight", Selection.HOVER),
                                ("constraint.reference", Guide.CONSTRUCTION),
                                ("constraint.reference_highlight", Selection.HOVER),
                                ("constraint.text", Text.PRIMARY),
                                ("constraint.text_highlight", Selection.HOVER),
                            )
                        },
                    },
                ),
                (*working, {"cad_vis_highlight_color": Paint(Selection.HOVER)}),
                (*addon("bool_tool"), {"solver": "EXACT", "sidebar_category": "Boolean"}),
                (*addon("ambientcg_material_importer"), {"cache_dir": str(MATERIALS)}),
                (*addon("cad2cube"), {"default_recenter": False}),
                (*addon("step_importer"), {"use_assembly_collections": True}),
                (
                    *addon("BlenderGIS"),
                    {
                        "logLevel": "WARNING",
                        "cacheFolder": str(cache / "gis"),
                        "forceTexturedSolid": False,
                        "adjust3Dview": False,
                        **{f"{service}_api_key": "" for service in ("opentopography", "maptiler")},
                        "overpassServerJson": JSON.dumps([(overpass, host, "Overpass API main instance"), *gis.DEFAULT_OVERPASS_SERVER]),
                        "overpassServer": overpass,
                        "predefCrsJson": JSON.dumps([(crs, f"UTM {zone}{'N' if LATITUDE >= 0 else 'S'}", "Site CRS"), *gis.DEFAULT_CRS]),
                        "predefCrs": crs,
                        "demServerJson": JSON.dumps([(dem, "USGS 3DEP", "USGS 3D Elevation Program bare-earth DEM, United States, keyless"), *gis.DEFAULT_DEM_SERVER]),
                        "demServer": dem,
                    },
                ),
                (*addon("Blosm"), {"osmServer": next(label for label, url in blosm.osmServers.items() if urlsplit(url).hostname == host), "dataDir": str(cache / "blosm")}),
                (*working, {"blosm.lodOf3dTiles": "lod5"}),
                (
                    *addon("mcp"),
                    {
                        "port": launch.port,
                        "use_autostart": True,
                        "autostart_delay": 0.0,
                        "timer_interval_active": 0.05,
                        "timer_interval_idle": 0.1,
                        "timer_interval_idle_delay": 60.0,
                        "use_log": False,
                    },
                ),
                (*addon("MCP for Blender"), {"telemetry_consent": False, **dict.fromkeys(services, "")}),
                (*working, {f"blendermcp_{key}": "" for key in services}),
                (
                    *addon("bonsai"),
                    {
                        "activate_workspace": False,
                        "should_setup_workspace": False,
                        "should_use_snap": False,
                        "should_setup_toolbar": True,
                        "doc.imperial_precision": f"1/{denominator}",
                        "doc.drawing_font": drawing,
                        "doc.magic_font_scale": doc["magic_font_scale"].default
                        * unit_system.cap(str(import_module("bonsai.tool").Blender.get_data_dir_path(Path("fonts") / doc["drawing_font"].default)))
                        / unit_system.cap(drawing),
                        "decorations_colour": Paint(Ink.SCREEN, display=True),
                        "decorator_color_selected": Paint(Selection.ITEM, display=True),
                        "decorator_color_unselected": Paint(Ink.SCREEN, display=True),
                        "decorator_color_special": Paint(Guide.CONSTRUCTION, display=True),
                        "decorator_color_error": Paint(Text.ERROR, display=True),
                        "decorator_color_background": Paint(Line.LOCKED, display=True),
                        "svg2pdf_command": "" if launch.inkscape is None else JSON.dumps([[launch.inkscape, "svg", "-o", "pdf"]]),
                        "svg2dxf_command": "",
                    },
                ),
                (
                    *working,
                    {
                        "camera.data.BIMCameraProperties.dpi": DPI,
                        "BIMProperties.section_plane_colour": Paint(Surface.SECTION),
                        "BIMProjectProperties.organisation_name": "",
                        "BIMProjectProperties.geometry_library": "opencascade",
                    },
                ),
                (
                    *working,
                    {
                        "BIMSolarProperties.latitude": LATITUDE,
                        "BIMSolarProperties.longitude": LONGITUDE,
                        "BIMSolarProperties.timezone": str(MOMENT.tzinfo),
                        "BIMSolarProperties.true_north": radians(NORTH),
                        "BIMSolarProperties.year": MOMENT.year,
                        "BIMSolarProperties.month": MOMENT.month,
                        "BIMSolarProperties.day": MOMENT.day,
                        "BIMSolarProperties.hour": MOMENT.hour,
                        "BIMSolarProperties.minute": MOMENT.minute,
                        "BIMSolarProperties.UTC_zone": -(standard + daylight),
                        "BIMSolarProperties.coordinates": import_module(f"{modules['sun_position']}.sun_calc").format_lat_long(LATITUDE, LONGITUDE),
                        "BIMSolarProperties.sun_path_size": PLAN_DISTANCE,
                    },
                    setitem,
                ),
                (
                    *working,
                    {
                        "dimensions_settings.imperial_denominator": denominator,
                        "dimensions_settings.show_selected_object_overlay": False,
                        "dimensions_settings.show_overlay_volume": True,
                        "dimensions_settings.dimension_color": Paint(Annotation.TAG.value, display=True),
                        "dimensions_settings.selected_dimension_color": Paint(Selection.ITEM, display=True),
                        "dimensions_settings.guide_color": Paint(Guide.CONSTRUCTION, display=True),
                        **{f"dimensions_settings.{kind}_line_width": 1.0 for kind in ("dimension", "guide")},
                        "dimensions_settings.dimension_text_size": size,
                        **{
                            f"dimensions_settings.{member}": kind(size * (hud := scene.dimensions_settings.bl_rna.properties)[member].default / hud["dimension_text_size"].default)
                            for member, kind in (("dimension_arrow_size", float), ("hud_padding_horizontal", round), ("hud_padding_vertical", round))
                        },
                        "dimensions_settings.dimension_arrow_end_style": "ARCHITECTURAL_TICK",
                        "dimensions_settings.precision": places,
                    },
                ),
                (*working, {"cpc_settings.imperial_fraction_denominator": denominator}),
                (
                    *working,
                    {
                        "MeasureItArchProps.imperial_precision": denominator,
                        "MeasureItArchProps.metric_precision": places,
                        "MeasureItArchProps.angle_precision": ANGLE_PRECISION,
                        "MeasureItArchProps.area_precision": 2,
                        "MeasureItArchProps.imperial_area_units": "FEET",
                        "MeasureItArchProps.metric_area_units": "METERS",
                        "MeasureItArchProps.use_unit_scale": False,
                        "MeasureItArchProps.default_color": Paint(Ink.SCREEN),
                        "MeasureItArchProps.default_dimension_style": DIMENSION_STYLE,
                        "MeasureItArchProps.default_annotation_style": ANNOTATION_STYLE,
                        **{
                            f"{subscript(f'StyleGenerator.{kind}', name)}.{member}": value
                            for kind, name, ends, endcap in (("alignedDimensions", DIMENSION_STYLE, ("endcapA", "endcapB"), "D"), ("annotations", ANNOTATION_STYLE, ("endcapA",), "T"))
                            for member, value in (("color", Paint(Annotation.TAG.value)), *((end, endcap) for end in ends), ("font", font))
                        },
                    },
                ),
                (*addon("mpfb"), {"mpfb_shelf_label": "MPFB"}),
                (*addon("modern_primitive"), {"show_world_space_value": True}),
                (*addon("univ"), {"overlay_2d_uv_edge_seam_color": Paint(Line.SEAM, Alpha.GLOW, display=True), "color_mode": "MONO", "show_split_toggle_uv_button": False}),
                (*addon("collection_manager"), {"enable_qcd": False}),
                (
                    *addon("xray_selection_tools"),
                    {
                        **{f"keymaps.is_{kind}_keymap_enabled": False for kind in ("mesh_mouse", "object_mouse", "toggles")},
                        **{f"{kind}_tools.group_with_builtins": True for kind in ("mesh", "object")},
                        **{f"object_tools.{kind}_select_behavior": "DIRECTIONAL" for kind in ("box", "lasso")},
                        "object_tools.show_xray": False,
                        **{f"mesh_tools.directional_{kind}_tool": True for kind in ("box", "lasso")},
                        **{f"{subscript('mesh_tools.direction_properties', 'RIGHT_TO_LEFT')}.select_all_{kind}": True for kind in ("edges", "faces")},
                        **{
                            f"{owner}.{frame}": Paint(Selection.ITEM)
                            for owner in ("mesh_tools", *(subscript("mesh_tools.direction_properties", direction) for direction in ("RIGHT_TO_LEFT", "LEFT_TO_RIGHT")))
                            for frame in ("default_color", "select_through_color")
                        },
                    },
                ),
                (*working, {"na_settings.margin": (2 * grid, 2 * grid)}),
                (
                    *addon("improved_node_search"),
                    {"search_in_blidname": True, "highlight_color": Paint(Guide.TRACKING, display=True), "border_attenuation": 1.0, "border_size": 0.0, "text_size": size},
                ),
                (
                    *addon("incremental_auto_save"),
                    {**dict.fromkeys(("relative_to_blend", "save_images", "print_saves"), False), "max_save_files": 10, **dict.fromkeys(("compress_files", "save_before_close"), True)},
                ),
                (*addon("auto_reload"), {"startup_run": True}),
                (
                    *analysis,
                    {
                        "vi_params.latitude": LATITUDE,
                        "vi_params.longitude": LONGITUDE,
                        "vi_params.sp_sd": MOMENT.timetuple().tm_yday,
                        "vi_params.sp_sh": MOMENT.hour + MOMENT.minute / 60 - daylight,
                        **dict.fromkeys(("vi_params.vi_leg_col", "vi_params.vi_scatt_col"), "viridis"),
                        "vi_params.vi_display_rp_sh": True,
                        "vi_params.vi_display_rp_fc": Paint(Text.PRIMARY, display=True),
                        "vi_params.vi_display_rp_fsh": Paint(Surface.SHADOW, display=True),
                        "vi_params.sp_season_main": Paint(Modality.ANALYSIS.mark),
                        "vi_params.sp_hour_main": Paint(Modality.ANALYSIS.token),
                        "vi_params.sp_sun_colour": Paint(Status.WARNING),
                        "vi_params.sp_globe_colour": Paint(Modality.ANALYSIS.mark, Alpha.ZONE_FILL),
                    },
                ),
                (
                    *working,
                    {
                        "pinsolver_settings.text_size": size,
                        "pinsolver_settings.line_width": 1.0,
                        "pinsolver_settings.pin_radius": POINT_WIDTH // 2,
                        "pinsolver_settings.text_color": Paint(Text.PRIMARY, display=True),
                        "pinsolver_settings.text_use_outline": True,
                        "pinsolver_settings.text_outline_color": Paint(Surface.SHADOW, display=True),
                    },
                ),
                (
                    *working,
                    {
                        "sun_pos_properties.sun_object": sun(scene),
                        "sun_pos_properties.sun_distance": PLAN_DISTANCE,
                        "sun_pos_properties.sky_texture": typed_node(scene.world.node_tree.nodes, "TEX_SKY").name,
                        "sun_pos_properties.use_refraction": False,
                        "sun_pos_properties.latitude": LATITUDE,
                        "sun_pos_properties.longitude": LONGITUDE,
                        "sun_pos_properties.UTC_zone": standard,
                        "sun_pos_properties.use_daylight_savings": bool(DAYLIGHT),
                        "sun_pos_properties.north_offset": -radians(NORTH),
                        "sun_pos_properties.year": MOMENT.year,
                        "sun_pos_properties.month": MOMENT.month,
                        "sun_pos_properties.day": MOMENT.day,
                        "sun_pos_properties.time": MOMENT.hour + MOMENT.minute / 60,
                    },
                    placed,
                ),
            ),
        )
    )


def styles(preferences: bpy.types.Preferences, scene: bpy.types.Scene) -> tuple[Row, ...]:
    """Rows loading interface font that styles draw in, keyed by file as `bpy.data.fonts.load` keys it, and adding MeasureIt_ARCH dimension and annotation styles `settings` names through the add-on's style functions."""
    generator, drawing = scene.StyleGenerator, preferences.view.font_path_ui
    module = sys.modules[type(generator).__module__]

    def font_file(path: str) -> str | None:
        return next((font.filepath for font in bpy.data.fonts if font.filepath == path), None)

    def added(create: Callable[[bpy.types.Context, str], object], name: str) -> None:
        with bpy.context.temp_override(scene=scene):
            create(bpy.context, name)

    return (
        Row(label=subscript("fonts", Path(drawing).name), read=partial(font_file, drawing), write=bpy.data.fonts.load, target=drawing),
        *(
            Row(label=subscript(f"{subscript('scenes', scene.name)}.StyleGenerator.{kind}", name), read=partial(held, collection, name), write=partial(added, create), target=name)
            for kind, collection, name, create in (
                ("alignedDimensions", generator.alignedDimensions, DIMENSION_STYLE, module.add_aligned_dimension_style),
                ("annotations", generator.annotations, ANNOTATION_STYLE, module.add_annotation_style),
            )
        ),
    )


def presets(modules: Mapping[str, str]) -> tuple[Row, ...]:
    """Rows of operator and camera presets in Blender's user preset folders, each file the script its add-preset operator writes, with folder, define lines, and member paths read from that operator."""
    package, variable = import_module(modules["per_camera_resolution"]), AddPresetOperator.preset_defines[0].partition(" = ")[0]
    camera, members = package.AddPresetCameraResolution, package.PerCameraResolutionProps.bl_rna.properties
    declared = (
        (
            AddPresetOperator.operator_path(ops.create_function("export_scene", "gltf").idname()),
            "Interchange",
            AddPresetOperator.preset_defines,
            {f"{variable}.export_format": "GLB", f"{variable}.export_hierarchy_full_collections": True},
        ),
        *(
            (
                AddPresetOperator.operator_path(ops.create_function("import_scene", "max").idname()),
                name,
                AddPresetOperator.preset_defines,
                {f"{variable}.scale_objects": scale, f"{variable}.use_collection": True},
            )
            for name, scale in (("Inches", Length.INCHES.value), ("Feet", Length.FEET.value), ("Millimeters", Length.MILLIMETERS.value), ("Centimeters", 10 * Length.MILLIMETERS), ("Meters", 1.0))
        ),
        *(
            (
                camera.preset_subdir,
                f"{paper}_{DPI}_dpi",
                camera.preset_defines,
                {path: sized[member] if member in sized else members[member].default for path in camera.preset_values for member in (path.rpartition(".")[2],)},
            )
            for units, paper in ((Units.IMPERIAL, "ARCH_D"), (Units.METRIC, "A1"))
            for sized in (dict(zip(("resolution_x", "resolution_y"), (round(side / Length.INCHES * DPI) for side in units.paper), strict=True)),)
        ),
    )

    def preset_text(folder: str, name: str) -> str | None:
        try:
            return Path(bpy.utils.user_resource("SCRIPTS", path=f"presets/{folder}"), f"{name}.py").read_text(encoding="utf-8")
        except FileNotFoundError:
            return None

    def written(folder: str, name: str, text: str) -> None:
        Path(bpy.utils.user_resource("SCRIPTS", path=f"presets/{folder}", create=True), f"{name}.py").write_text(text, encoding="utf-8")

    return tuple(
        Row(
            label=subscript("presets", f"{folder}/{name}"),
            read=partial(preset_text, folder, name),
            write=partial(written, folder, name),
            target="".join(("import bpy\n", *(f"{define}\n" for define in defines), "\n", *(f"{path} = {value!r}\n" for path, value in values.items()))),
        )
        for folder, name, defines, values in declared
    )


# --- [STEPS]
def configured_addons(unit_system: ModuleType, preferences: bpy.types.Preferences, scene: bpy.types.Scene, launch: Launch, modules: Mapping[str, str]) -> Iterator[str]:
    """Converge every add-on's declared members under their owner, each labeled by its RNA path from root, then BlenderGIS georeference, MPFB's log level, and presets."""
    yield from converge_groups(unit_system, settings(unit_system, preferences, scene, launch, modules))
    logs = import_module(f"{modules['mpfb']}.services.logservice").LogService
    rows = (georeference(scene, modules), Row(label="mpfb.LogService.default_log_level", read=logs.get_default_log_level, write=logs.set_default_log_level, target=logs.WARN), *presets(modules))
    yield from chain.from_iterable(converged(row) for row in rows)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["configured_addons", "presets", "settings", "styles"]
