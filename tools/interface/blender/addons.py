# ty: ignore[invalid-argument-type, invalid-context-manager, not-iterable, possibly-missing-attribute, redundant-condition-strict, unresolved-attribute, unresolved-import]
# mypy: disable-error-code="arg-type, attr-defined, func-returns-value, import-not-found, union-attr"
"""Blender's declared add-on packages, their settings by store, and the files Blender and add-ons read by path."""

from collections.abc import Callable, Iterator, Mapping
from datetime import timedelta
from enum import auto, Enum
from functools import partial, reduce
from importlib import import_module
from importlib.util import cache_from_source
from math import log10, radians
import os
from pathlib import Path
import pwd
import shutil
import sys
import tomllib
from types import ModuleType
from urllib.parse import urlsplit
import zipfile
import zlib

import addon_utils
from bl_pkg.bl_extension_utils import PKG_MANIFEST_FILENAME_TOML
import bpy
from cattrs.preconf.json import make_converter

from interface.blender.catalog import Catalog, Listed, Local
from interface.blender.startup import PLAN_DISTANCE, sun, world_node
from interface.blender.theme import Encoding, Paint
from interface.render import DAYLIGHT, DPI, LATITUDE, LONGITUDE, MATERIALS, MOMENT, NORTH, OFFSET, stocked
from interface.report import digest, Kind, line, Row
from interface.roles import Alpha, Annotation, Guide, Ink, Line, Modality, POINT_WIDTH, Selection, Status, Surface, Text
from interface.units import ANGLE_PRECISION, FOOT, INCH, MILLIMETER, Units

# --- [TYPES] ----------------------------------------------------------------------------

type Files = Mapping[str, tuple[int, int]]


class Store(Enum):
    """Owner of an add-on setting, the Bonsai solar group written as raw items."""

    PREFERENCES = auto()
    SCENE = auto()
    ANALYSIS = auto()
    SOLAR = auto()
    GEOREFERENCE = auto()


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [MODULES]
def identity(module: ModuleType) -> str:
    """Id a `packages.toml` row keys the module on, an extension's manifest id or another add-on's registered name."""
    return module.__name__.rpartition(".")[2] if module.__name__.startswith("bl_ext.") else module.bl_info["name"]


def location(module: ModuleType) -> Path:
    """Folder of a package module, file of a single-file one."""
    return file.parent if (file := Path(module.__file__)).name == "__init__.py" else file


def checksum(path: Path) -> tuple[int, int]:
    """CRC-32 and size of a file, the pair a zip member records."""
    with path.open("rb") as handle:
        return reduce(lambda total, chunk: zlib.crc32(chunk, total), iter(partial(handle.read, 1 << 20), b""), 0), path.stat().st_size


def checksums(folder: Path, names: frozenset[str]) -> Files:
    """CRC-32 and size of each named file the folder holds."""
    return {name: checksum(path) for name in names if (path := folder / name).is_file()}


def listing(files: Files) -> bytes:
    """One line per file with its CRC-32 and size, the bytes whose digest stamps a folder or an archive."""
    return b"".join(f"{name}\0{crc}\0{size}\n".encode() for name, (crc, size) in sorted(files.items()))


def enabled(name: str) -> bool:
    """Whether the add-on is registered and recorded in the preferences."""
    return all(addon_utils.check(name))


def installed(name: str) -> tuple[ModuleType, ...]:
    """Installed modules of the row id."""
    return tuple(module for module in addon_utils.modules(refresh=False) if identity(module) == name)


def addons_core() -> Path:
    """Folder of Blender's bundled add-ons."""
    return Path(bpy.utils.system_resource("SCRIPTS", path="addons_core"))


def user_repository(preferences: bpy.types.Preferences) -> bpy.types.UserExtensionRepo:
    """User repository without a remote, where archive extensions install."""
    return next(repo for repo in preferences.extensions.repos if repo.source == "USER" and not repo.use_remote_url)


def extension(preferences: bpy.types.Preferences, name: str) -> str:
    """Module the extension with the manifest id registers under in the user repository."""
    return f"bl_ext.{user_repository(preferences).module}.{name}"


def loaded() -> dict[str, str]:
    """Module name of every registered add-on by its `packages.toml` id."""
    return {identity(module): module.__name__ for module in addon_utils.modules() if enabled(module.__name__)}


# --- [PACKAGES]
def cataloged(path: str) -> None:
    """Sync every enabled remote repository when online and write the catalog as JSON to the path."""
    repos = bpy.context.preferences.extensions.repos
    if bpy.app.online_access:
        bpy.ops.extensions.repo_sync_all()
    catalog = Catalog(
        bpy.app.version_string,
        bpy.utils.user_resource("CONFIG"),
        {repo.module: repo.directory for repo in repos if repo.enabled and repo.use_remote_url},
        frozenset(identity(module) for module in addon_utils.modules() if location(module).is_relative_to(addons_core())),
        bpy.utils.system_resource("DATAFILES", path="assets"),
        {item.value.to_bytes(2, "little").decode(): item.identifier for item in bpy.types.ID.bl_rna.properties["id_type"].enum_items},
    )
    Path(path).write_text(make_converter().dumps(catalog), encoding="utf-8")


def purge(name: str) -> None:
    """Evict a removed legacy add-on's loaded modules and aliases and delete its data folder."""
    if (removed := sys.modules.get(name)) is not None:
        roots = {key for key, each in sys.modules.items() if each is removed}
        for key in [key for key in sys.modules if any(key == root or key.startswith(f"{root}.") for root in roots)]:
            del sys.modules[key]
    if (folder := Path(bpy.utils.user_resource("DATAFILES"), name)).is_dir():
        shutil.rmtree(folder)


def remove(window: bpy.types.Window, preferences: bpy.types.Preferences, module: ModuleType) -> None:
    """Uninstall an extension, disable a bundled add-on, or delete a legacy add-on with its bytecode, modules, and data folder."""
    match module.__name__.split("."):
        case ["bl_ext", repository, package]:
            bpy.ops.extensions.package_uninstall(repo_index=next(index for index, repo in enumerate(preferences.extensions.repos) if repo.module == repository), pkg_id=package)
        case _ if location(module).is_relative_to(addons_core()):
            addon_utils.disable(module.__name__, default_set=True)
        case _:
            with bpy.context.temp_override(window=window, screen=window.screen, area=window.screen.areas[0]):
                bpy.ops.preferences.addon_remove(module=module.__name__)
            Path(cache_from_source(module.__file__)).unlink(missing_ok=True)
            purge(module.__name__)
    addon_utils.modules_refresh()


def packages(window: bpy.types.Window, preferences: bpy.types.Preferences, declared: tuple[Local | Listed, ...]) -> Iterator[Row]:
    """Row per declared package converging its one module to its stamp and enabled, then a row removing stale add-on records."""
    addon_utils.modules_refresh()

    def version(module: ModuleType) -> str:
        return str(tomllib.loads(location(module).joinpath(PKG_MANIFEST_FILENAME_TOML).read_text(encoding="utf-8"))["version"])

    def release(_module: ModuleType) -> str:
        return bpy.app.version_string

    def contents(names: frozenset[str], module: ModuleType) -> str:
        return digest(listing(checksums(Path(module.__file__).parent, names)))

    def stamped(name: str, stamp: Callable[[ModuleType], str]) -> tuple[str, ...]:
        return tuple(sorted(f"{module.__name__} {stamp(module)} {'enabled' if enabled(module.__name__) else 'disabled'}" for module in installed(name)))

    def install(name: str, module: str, stamp: Callable[[ModuleType], str], wanted: str, installer: Callable[[], object], _target: object) -> None:
        for each in installed(name):
            if each.__name__ != module:
                remove(window, preferences, each)
        if not any(each.__name__ == module and stamp(each) == wanted for each in installed(name)):
            installer()
            addon_utils.modules_refresh()
        if module not in preferences.addons:
            bpy.ops.preferences.addon_enable(module=module)

    def stale() -> tuple[str, ...]:
        return tuple(sorted(record.module for record in preferences.addons if record.module not in {module.__name__ for module in addon_utils.modules(refresh=False)}))

    def remove_stale(_target: object) -> None:
        for name in stale():
            preferences.addons.remove(preferences.addons[name])
            purge(name)

    def package_row(name: str, module: str, stamp: Callable[[ModuleType], str], wanted: str, installer: Callable[[], object]) -> Row:
        return Row(label=f"packages.{name}", read=partial(stamped, name, stamp), write=partial(install, name, module, stamp, wanted, installer), target=(f"{module} {wanted} enabled",))

    def archived(name: str, archive: Path) -> Row:
        with zipfile.ZipFile(archive) as packed:
            members = {info.filename: (info.CRC, info.file_size) for info in packed.infolist() if not info.is_dir()}
        match sorted({member.partition("/")[0] for member in members}):
            case _ if PKG_MANIFEST_FILENAME_TOML in members:
                repo = user_repository(preferences).module
                return package_row(
                    name,
                    f"bl_ext.{repo}.{name}",
                    partial(contents, frozenset(members)),
                    digest(listing(members)),
                    partial(bpy.ops.extensions.package_install_files, filepath=str(archive), repo=repo, enable_on_install=True),
                )
            case [top]:
                files = {member.removeprefix(f"{top}/"): stamp for member, stamp in members.items()}
                return package_row(
                    name,
                    Path(top).stem,
                    partial(contents, frozenset(files)),
                    digest(listing(files)),
                    partial(bpy.ops.preferences.addon_install, filepath=str(archive), overwrite=True, enable_on_install=True),
                )
            case tops:
                raise LookupError(f"packages.{name} archive {archive} holds {tops} and needs one top entry")

    for row in declared:
        match row:
            case Listed(identity=name, archive=archive, repository=repository, version=wanted):
                yield package_row(name, f"bl_ext.{repository}.{name}", version, wanted, partial(bpy.ops.extensions.package_install_files, filepath=archive, repo=repository, enable_on_install=True))
            case Local(identity=name, archive=str() as archive):
                yield archived(name, Path(archive))
            case Local(identity=name):
                match [each for each in installed(name) if location(each).is_relative_to(addons_core())]:
                    case [bundled]:
                        yield package_row(name, bundled.__name__, release, release(bundled), partial(addon_utils.enable, bundled.__name__, default_set=True))
                    case found:
                        raise LookupError(f"packages.{name} names {len(found)} bundled add-ons and needs one")
    yield Row(label="preferences.addons", read=stale, write=remove_stale, target=())


def orphans() -> Iterator[Row]:
    """Row per startup data-block deleting the system properties a disabled or removed add-on left, which no registered RNA property owns."""

    def unowned(block: bpy.types.ID, group: bpy.types.bpy_struct) -> tuple[str, ...]:
        return tuple(sorted(set(group.keys()) - set(block.bl_rna.properties.keys()) - ({"cycles"} if block.id_type == "CAMERA" else set())))

    def delete(group: bpy.types.bpy_struct, keys: Callable[[], tuple[str, ...]], _target: object) -> None:
        for key in keys():
            del group[key]

    for block in bpy.data.all_ids:
        if (group := block.bl_system_properties_get()) is not None:
            read = partial(unowned, block, group)
            yield Row(label=f'startup.{block.id_type}["{block.name}"]', read=read, write=partial(delete, group, read), target=())


# --- [SETTINGS]
def declared_files(modules: Mapping[str, str]) -> Iterator[Row]:
    """Rows of the operator and camera presets in Blender's preset layout, and of MPFB's log levels while it is registered."""

    def file_row(root: str, relative: str, text: str) -> Row:
        path = Path(root, relative)

        def read() -> str:
            return path.read_text(encoding="utf-8") if path.is_file() else ""

        def write(target: str) -> None:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(target, encoding="utf-8")

        return Row(label=relative, read=read, write=write, target=text)

    operator, camera = ("op", "bpy.context.active_operator"), ("camera_res", "bpy.context.object.data.per_camera_resolution")
    presets = (
        ("operator/export_scene.gltf", "Interchange", operator, {"export_format": "GLB", "export_hierarchy_full_collections": True}),
        *(
            ("operator/import_scene.max", name, operator, {"scale_objects": scale, "use_collection": True})
            for name, scale in (("Inches", INCH), ("Feet", FOOT), ("Millimeters", MILLIMETER), ("Centimeters", 10 * MILLIMETER), ("Meters", 1.0))
        ),
        *(
            (
                "per_camera_resolution",
                f"{paper}_{DPI}_dpi",
                camera,
                {
                    **dict(zip(("resolution_x", "resolution_y"), (round(side / INCH * DPI) for side in system.paper), strict=True)),
                    "resolution_percentage": 100,
                    "pixel_aspect_x": 1.0,
                    "pixel_aspect_y": 1.0,
                },
            )
            for system, paper in ((Units.IMPERIAL, "ARCH_D"), (Units.METRIC, "A1"))
        ),
    )
    yield from (
        file_row(
            bpy.utils.user_resource("SCRIPTS"),
            f"presets/{folder}/{name}.py",
            "".join(("import bpy\n", f"{variable} = {source}\n\n", *(f"{variable}.{member} = {value!r}\n" for member, value in values.items()))),
        )
        for folder, name, (variable, source), values in presets
    )
    if mpfb := modules.get("mpfb"):
        levels = {"default": import_module(f"{mpfb}.services.logservice").LogService.WARN}
        yield file_row(bpy.utils.resource_path("USER"), "mpfb/config/log_levels.json", make_converter().dumps(levels))


def options(
    preferences: bpy.types.Preferences, scene: bpy.types.Scene, port: int, modules: Mapping[str, str], system: ModuleType, inkscape: str | None
) -> tuple[dict[tuple[str, Store], dict[str, object]], tuple[str, ...]]:
    """Registered add-on settings by `packages.toml` id and store in the order their update callbacks need, with a skip line per absent add-on or library."""
    endpoint, grid, drawing, converter = "https://overpass-api.de/api/interpreter", 20, preferences.view.font_path_ui, make_converter()
    denominator, standard, library = str(round(INCH / Units.IMPERIAL.resolution)), OFFSET / timedelta(hours=1), stocked(MATERIALS)
    size, places = round(preferences.ui_styles[0].widget.points * preferences.system.ui_scale), round(-log10(Units.METRIC.resolution / MILLIMETER))
    zone = int((LONGITUDE + 180) // 6) + 1
    epsg = (32600 if LATITUDE >= 0 else 32700) + zone
    crs, font, cache, overpass = f"EPSG:{epsg}", bpy.data.fonts.load(drawing, check_existing=True), Path(bpy.app.cachedir), urlsplit(endpoint).hostname
    dem = "https://elevation.nationalmap.gov/arcgis/rest/services/3DEPElevation/ImageServer/exportImage?bbox={W},{S},{E},{N}&bboxSR=4326&imageSR=4326&size=1024,1024&format=tiff&pixelType=F32&interpolation=RSP_BilinearInterpolation&f=image"
    services = ("sketchfab_api_key", "polypizza_api_key", "hyper3d_api_key", "hunyuan3d_secret_id", "hunyuan3d_secret_key")
    builders: dict[str, Callable[[str], dict[Store, dict[str, object]]]] = {
        "Sverchok": lambda _: {
            Store.PREFERENCES: {
                **dict.fromkeys(("log_to_buffer", "auto_apply_theme", "apply_theme_on_open"), False),
                "log_level": "WARNING",
                "exception_color": Paint(Status.ERROR),
                "no_data_color": Paint(Status.INFO),
                **{f"color_{kind}": Paint(Surface.PANEL) for kind in ("viz", "tex", "sce", "lay", "gen")},
            }
        },
        "CAD_Sketcher": lambda _: {
            Store.PREFERENCES: {
                "imperial_precision": denominator,
                "decimal_precision": places,
                "angle_precision": ANGLE_PRECISION,
                "show_whats_new": False,
                "entity_scale": POINT_WIDTH / (3.75 * preferences.system.ui_scale),
                "text_size": size,
                **{
                    f"theme_settings.{path}": Paint(rgb)
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
            }
        },
        "cad_helper": lambda _: {Store.SCENE: {"cad_vis_highlight_color": Paint(Selection.HOVER, encoding=Encoding.LINEAR)}},
        "bool_tool": lambda _: {Store.PREFERENCES: {"solver": "EXACT", "sidebar_category": "Boolean"}},
        "ambientcg_material_importer": lambda _: {Store.PREFERENCES: {"cache_dir": str(MATERIALS)} if library else {}},
        "cad2cube": lambda _: {Store.PREFERENCES: {"default_recenter": False}},
        "step_importer": lambda _: {Store.PREFERENCES: {"use_assembly_collections": True}},
        "BlenderGIS": lambda module: {
            Store.PREFERENCES: {
                "logLevel": "WARNING",
                "cacheFolder": str(cache / "gis"),
                "forceTexturedSolid": False,
                "adjust3Dview": False,
                **{f"{service}_api_key": "" for service in ("opentopography", "maptiler")},
                "overpassServerJson": converter.dumps([(endpoint, overpass, "Overpass API main instance"), *(gis := import_module(f"{module}.prefs")).DEFAULT_OVERPASS_SERVER]),
                "overpassServer": endpoint,
                "predefCrsJson": converter.dumps([(crs, f"UTM {zone}{'N' if LATITUDE >= 0 else 'S'}", "Site CRS"), *gis.DEFAULT_CRS]),
                "predefCrs": crs,
                "demServerJson": converter.dumps([(dem, "USGS 3DEP", "USGS 3D Elevation Program bare-earth DEM, United States, keyless"), *gis.DEFAULT_DEM_SERVER]),
                "demServer": dem,
            },
            Store.GEOREFERENCE: {
                "lon": LONGITUDE,
                "lat": LATITUDE,
                "crs": crs,
                **dict(zip(("crsx", "crsy"), import_module(f"{module}.core.proj.reproj").reprojPt(4326, crs, LONGITUDE, LATITUDE), strict=True)),
            },
        },
        "Blosm": lambda _: {Store.PREFERENCES: {"osmServer": overpass, "dataDir": str(cache / "blosm")}, Store.SCENE: {"blosm.mode": "3Dsimple", "blosm.lodOf3dTiles": "lod5"}},
        "mcp": lambda _: {
            Store.PREFERENCES: {
                "port": port,
                "use_autostart": True,
                "autostart_delay": 0.0,
                "timer_interval_active": 0.05,
                "timer_interval_idle": 0.1,
                "timer_interval_idle_delay": 60.0,
                "use_log": False,
            }
        },
        "MCP for Blender": lambda _: {Store.PREFERENCES: {"telemetry_consent": False, **dict.fromkeys(services, "")}, Store.SCENE: {f"blendermcp_{key}": "" for key in services}},
        "bonsai": lambda module: {
            Store.PREFERENCES: {
                "activate_workspace": False,
                "should_setup_workspace": False,
                "should_use_snap": False,
                "should_setup_toolbar": True,
                "doc.imperial_precision": f"1/{denominator}",
                "doc.drawing_font": drawing,
                "doc.magic_font_scale": (doc := preferences.addons[module].preferences.doc.bl_rna.properties)["magic_font_scale"].default
                * system.cap(str(import_module("bonsai.tool").Blender.get_data_dir_path(Path("fonts") / doc["drawing_font"].default)))
                / system.cap(drawing),
                "decorations_colour": Paint(Ink.SCREEN),
                "decorator_color_selected": Paint(Selection.ITEM),
                "decorator_color_unselected": Paint(Ink.SCREEN),
                "decorator_color_special": Paint(Guide.CONSTRUCTION),
                "decorator_color_error": Paint(Text.ERROR),
                "decorator_color_background": Paint(Line.LOCKED),
                **({f"svg2{kind}_command": converter.dumps([[str(Path(inkscape, "Contents", "MacOS", "inkscape")), "svg", "-o", kind]]) for kind in ("pdf", "dxf")} if inkscape is not None else {}),
            },
            Store.SCENE: {
                "camera.data.BIMCameraProperties.dpi": DPI,
                "BIMProperties.section_plane_colour": Paint(Surface.SECTION, encoding=Encoding.LINEAR),
                "BIMProjectProperties.author_name": pwd.getpwuid(os.getuid()).pw_gecos,
                "BIMProjectProperties.organisation_name": "",
            },
            Store.SOLAR: {
                "latitude": LATITUDE,
                "longitude": LONGITUDE,
                "timezone": str(MOMENT.tzinfo),
                "true_north": radians(NORTH),
                **{field: getattr(MOMENT, field) for field in ("year", "month", "day", "hour", "minute")},
                "UTC_zone": -standard,
                **({"coordinates": import_module(f"{position}.sun_calc").format_lat_long(LATITUDE, LONGITUDE)} if (position := modules.get("sun_position")) else {}),
                "sun_path_size": PLAN_DISTANCE,
            },
        },
        "dimensions": lambda _: {
            Store.SCENE: {
                "dimensions_settings.imperial_denominator": denominator,
                "dimensions_settings.show_selected_object_overlay": False,
                "dimensions_settings.show_overlay_volume": True,
                "dimensions_settings.dimension_color": Paint(Annotation.TAG.value),
                "dimensions_settings.selected_dimension_color": Paint(Selection.ITEM),
                "dimensions_settings.guide_color": Paint(Guide.CONSTRUCTION),
                **{f"dimensions_settings.{kind}_line_width": 1.0 for kind in ("dimension", "guide")},
                "dimensions_settings.dimension_text_size": size,
                **{
                    f"dimensions_settings.{member}": kind(size * (hud := scene.dimensions_settings.bl_rna.properties)[member].default / hud["dimension_text_size"].default)
                    for member, kind in (("dimension_arrow_size", float), ("hud_padding_horizontal", round), ("hud_padding_vertical", round))
                },
                "dimensions_settings.dimension_arrow_end_style": "ARCHITECTURAL_TICK",
                "dimensions_settings.precision": ANGLE_PRECISION,
            },
            Store.PREFERENCES: {"default_precision": ANGLE_PRECISION},
        },
        "curve_profile_creator": lambda _: {Store.SCENE: {"cpc_settings.imperial_fraction_denominator": denominator}},
        "MeasureIt_ARCH": lambda _: {
            Store.SCENE: {
                "MeasureItArchProps.imperial_precision": denominator,
                "MeasureItArchProps.metric_precision": places,
                "MeasureItArchProps.angle_precision": ANGLE_PRECISION,
                "MeasureItArchProps.area_precision": 2,
                "MeasureItArchProps.imperial_area_units": "FEET",
                "MeasureItArchProps.metric_area_units": "METERS",
                "MeasureItArchProps.use_unit_scale": False,
                "MeasureItArchProps.default_color": Paint(Ink.SCREEN, encoding=Encoding.LINEAR),
                "MeasureItArchProps.default_dimension_style": "Dimension Style 1",
                "MeasureItArchProps.default_annotation_style": "Annotation Style 1",
                **{
                    f'StyleGenerator.{kind}["{name}"].{member}': value
                    for kind, name, ends in (("alignedDimensions", "Dimension Style 1", ("endcapA", "endcapB")), ("annotations", "Annotation Style 1", ("endcapA",)))
                    for member, value in (("color", Paint(Annotation.TAG.value, encoding=Encoding.LINEAR)), *((end, "D" if kind == "alignedDimensions" else "T") for end in ends), ("font", font))
                },
            }
        },
        "mpfb": lambda _: {Store.PREFERENCES: {"mpfb_shelf_label": "MPFB"}},
        "modern_primitive": lambda _: {Store.PREFERENCES: {"show_world_space_value": True}},
        "univ": lambda _: {Store.PREFERENCES: {"overlay_2d_uv_edge_seam_color": Paint(Line.SEAM, Alpha.GLOW), "color_mode": "MONO", "show_split_toggle_uv_button": False}},
        "collection_manager": lambda _: {Store.PREFERENCES: {"enable_qcd": False}},
        "xray_selection_tools": lambda _: {
            Store.PREFERENCES: {
                **{f"keymaps.is_{kind}_keymap_enabled": False for kind in ("mesh_mouse", "object_mouse", "toggles")},
                **{f"{kind}_tools.group_with_builtins": True for kind in ("mesh", "object")},
                **{f"object_tools.{kind}_select_behavior": "DIRECTIONAL" for kind in ("box", "lasso")},
                "object_tools.show_xray": False,
                **{f"mesh_tools.directional_{kind}_tool": True for kind in ("box", "lasso")},
                **{f'mesh_tools.direction_properties["RIGHT_TO_LEFT"].select_all_{kind}': True for kind in ("edges", "faces")},
                **{
                    f"mesh_tools.{owner}{frame}": Paint(Selection.ITEM, encoding=Encoding.LINEAR)
                    for owner in ("", 'direction_properties["RIGHT_TO_LEFT"].', 'direction_properties["LEFT_TO_RIGHT"].')
                    for frame in ("default_color", "select_through_color")
                },
            }
        },
        "node_arrange": lambda _: {Store.SCENE: {"na_settings.margin": (2 * grid, 2 * grid)}},
        "improved_node_search": lambda _: {Store.PREFERENCES: {"search_in_blidname": True, "highlight_color": Paint(Guide.TRACKING), "border_attenuation": 1.0, "border_size": 0.0, "text_size": size}},
        "incremental_auto_save": lambda _: {
            Store.PREFERENCES: {
                **dict.fromkeys(("relative_to_blend", "save_images", "print_saves"), False),
                "max_save_files": 10,
                **dict.fromkeys(("compress_files", "save_before_close"), True),
                "save_interval": 1,
            }
        },
        "auto_reload": lambda _: {Store.PREFERENCES: {"startup_run": True}},
        "VI-Suite": lambda _: {
            Store.ANALYSIS: {
                "vi_params.latitude": LATITUDE,
                "vi_params.longitude": LONGITUDE,
                "vi_params.sp_sd": MOMENT.timetuple().tm_yday,
                "vi_params.sp_sh": MOMENT.hour + MOMENT.minute / 60 - DAYLIGHT / timedelta(hours=1),
                **dict.fromkeys(("vi_params.vi_leg_col", "vi_params.vi_scatt_col"), "viridis"),
                "vi_params.vi_display_rp_sh": True,
                "vi_params.vi_display_rp_fc": Paint(Text.PRIMARY),
                "vi_params.vi_display_rp_fsh": Paint(Surface.SHADOW),
                "vi_params.sp_season_main": Paint(Modality.ANALYSIS.mark),
                "vi_params.sp_hour_main": Paint(Modality.ANALYSIS.token),
                "vi_params.sp_sun_colour": Paint(Status.WARNING),
                "vi_params.sp_globe_colour": Paint(Modality.ANALYSIS.mark, Alpha.ZONE_FILL),
            }
        },
        "pin_solver": lambda _: {
            Store.SCENE: {
                "pinsolver_settings.text_size": size,
                "pinsolver_settings.line_width": 1.0,
                "pinsolver_settings.pin_radius": POINT_WIDTH // 2,
                "pinsolver_settings.text_color": Paint(Text.PRIMARY),
                "pinsolver_settings.text_use_outline": True,
                "pinsolver_settings.text_outline_color": Paint(Surface.SHADOW),
            }
        },
        "sun_position": lambda _: {
            Store.SCENE: {
                "sun_pos_properties.sun_object": sun(scene),
                "sun_pos_properties.sun_distance": PLAN_DISTANCE,
                "sun_pos_properties.sky_texture": world_node(scene, "TEX_SKY").name,
                "sun_pos_properties.latitude": LATITUDE,
                "sun_pos_properties.longitude": LONGITUDE,
                "sun_pos_properties.UTC_zone": standard,
                "sun_pos_properties.use_daylight_savings": bool(DAYLIGHT),
                "sun_pos_properties.north_offset": -radians(NORTH),
                **{f"sun_pos_properties.{field}": getattr(MOMENT, field) for field in ("year", "month", "day")},
                "sun_pos_properties.time": MOMENT.hour + MOMENT.minute / 60,
            }
        },
    }
    return {(name, store): rows for name, build in builders.items() if name in modules for store, rows in build(modules[name]).items()}, (
        *(line(Kind.SKIP, name) for name in builders if name not in modules),
        *(() if library else (line(Kind.SKIP, str(MATERIALS)),)),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Store", "cataloged", "declared_files", "extension", "loaded", "options", "orphans", "packages"]
