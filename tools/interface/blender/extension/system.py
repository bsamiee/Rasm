# ty: ignore[invalid-argument-type, invalid-context-manager, invalid-type-form, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, no-any-return, union-attr, untyped-decorator, valid-type"
# ruff: file-ignore[invalid-class-name, mutable-class-default, relative-imports]
"""Scene unit systems with every setting that follows them, the switch operator, and the file-load handler."""

from array import array
from collections.abc import Callable, Iterator, Mapping
from functools import cache, reduce
from importlib import import_module
from math import ceil, sqrt
from operator import setitem
import sys
from typing import Final, override, TYPE_CHECKING

import blf
import bpy
from bpy.app.handlers import persistent
from bpy.props import EnumProperty

from .units import Length, Pen, Units

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import OperatorReturnItems

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [READS]
def struct_at(root: bpy.types.bpy_struct, path: str) -> bpy.types.bpy_struct | None:
    """Struct at the dotted path under the root, None when a step is unregistered or unset."""

    def step(struct: bpy.types.bpy_struct | None, name: str) -> bpy.types.bpy_struct | None:
        return getattr(struct, name) if struct is not None and name in struct.bl_rna.properties else None

    return reduce(step, path.split("."), root)


@cache
def cap(path: str) -> float:
    """Cap height of the font file in ems."""
    font = blf.load(path)
    blf.size(font, 1000)
    return blf.dimensions(font, "H")[1] / 1000


def preset(camera: bpy.types.PropertyGroup, sheet: int) -> str:
    """Bonsai drawing scale preset at the sheet scale from its preset list rebuilt for the scene's unit system."""
    drawing = sys.modules[type(camera).__module__]
    drawing.purge()
    return next(identifier for identifier, *_ in drawing.get_diagram_scales(camera, bpy.context) if identifier.endswith(f"|1/{sheet}"))


# --- [LENGTHS]
def snapped(units: Units, value: float) -> float:
    """Length as a grid multiple from one grid step up and a snap multiple below, one snap at least for a positive length."""
    step = units.grid if value >= units.grid else units.snap
    return step * max(round(value / step), 1 if value > 0 else 0)


def profile(units: Units, settings: bpy.types.PropertyGroup) -> dict[str, float]:
    """Curve Profile Creator lengths by member, inch values under IMPERIAL and defaults under METRIC, each arc depth the quarter-circle sagitta of its chord."""
    imperial = {
        member: value
        for members, value in (
            (("arch_width", "arch_height"), 5 * Length.INCHES / 4),
            (("arch_fillet_a", "arch_fillet_b"), Length.INCHES / 4),
            (("arch_fascia", "arch_secondary_width", "arch_secondary_height", "arch_tread1", "arch_tread2"), 3 * Length.INCHES / 4),
            (("arch_rise1", "arch_rise2"), Length.INCHES / 2),
            (("parametric_width",), 2 * Length.INCHES),
            (("parametric_height",), Length.INCHES),
        )
        for member in members
    }
    lengths = imperial if units is Units.IMPERIAL else {member: settings.bl_rna.properties[member].default for member in imperial}
    geometry = import_module(".primitive_geometry", sys.modules[type(settings).__module__].__package__)
    chords = (("arch_arc_depth", "arch_width"), ("arch_secondary_arc_depth", "arch_secondary_width"), ("parametric_arc_depth", "parametric_width"))
    return {**lengths, **{depth: geometry.default_arc_depth(lengths[chord]) for depth, chord in chords}}


def extent(units: Units, scene: bpy.types.Scene) -> dict[str, float]:
    """Blosm import extent one modeling extent each way around the scene's georeference, with its snapped level height."""
    mercator = import_module(f"{type(scene.blosm).__module__.partition('.')[0]}.util.transverse_mercator").TransverseMercator(lat=scene["lat"], lon=scene["lon"])
    (south, west), (north, east) = (mercator.toGeographic(corner, corner) for corner in (-units.extent, units.extent))
    return {"minLat": south, "minLon": west, "maxLat": north, "maxLon": east, "levelHeight": snapped(units, scene.blosm.bl_rna.properties["levelHeight"].default)}


# --- [SETTINGS]
def declared(units: Units, scene: bpy.types.Scene) -> Iterator[tuple[str, bpy.types.bpy_struct, Mapping[str, object]]]:
    """Scene settings of the unit system by struct path, struct, and rows, the system first since its write resets the unit tokens."""
    sheet, (area, volume, mass) = units.sheet_scale, ("square foot", "cubic foot", "pound") if units is Units.IMPERIAL else ("SQUARE_METRE", "CUBIC_METRE", "KILO/GRAM")
    em = units.text / Length.POINTS / cap(bpy.context.preferences.view.font_path_ui)
    groups = (
        (
            "unit_settings",
            lambda: {
                "system": units.name,
                "scale_length": 1.0,
                "use_separate": units.separate,
                "length_unit": units.length.name,
                "mass_unit": units.mass,
                "temperature_unit": units.temperature,
                "time_unit": "SECONDS",
                "system_rotation": "DEGREES",
            },
        ),
        ("tool_settings", lambda: {"double_threshold": units.tolerance, "proportional_distance": units.grid}),
        ("eevee", lambda: {"volumetric_end": units.far}),
        ("camera.data", lambda: {"clip_start": units.snap, "clip_end": units.far, "display_size": units.grid, "ortho_scale": units.paper[0] * sheet}),
        ("camera.data.BIMCameraProperties", lambda: {"diagram_scale": preset(scene.camera.data.BIMCameraProperties, sheet), "width": units.paper[0] * sheet, "height": units.paper[1] * sheet}),
        ("cpc_settings", lambda: {"merge_tolerance": units.tolerance, **profile(units, scene.cpc_settings)}),
        (
            "scale_interactive_settings",
            lambda: {**({"system_unit": "IMPERIAL_IN"} if units is Units.IMPERIAL else {"system_unit": "METRIC", "metric_unit": "MM"}), "decimal_precision": str(units.places(units.page, 10))},
        ),
        *((("blosm", lambda: extent(units, scene)),) if {"lat", "lon"} <= set(scene.keys()) else ()),
        ("BIMProperties", lambda: {"area_unit": area, "volume_unit": volume, "mass_unit": mass, "time_unit": "SECOND"}),
        (
            "dimensions_settings",
            lambda: {
                "imperial_unit_style": "FEET_INCHES",
                "metric_unit_style": Units.METRIC.length.name,
                "output_sizing_mode": "WORLD",
                "output_world_text_height": units.text * sheet,
                "output_world_arrow_size": units.text * sheet,
                "output_world_line_width": Pen.THIN * sheet,
            },
        ),
        ("MeasureItArchProps", lambda: {"default_scale": sheet}),
        (
            "StyleGenerator",
            lambda: {
                **{f"annotations[{index}].{name}": value for index, _ in enumerate(scene.StyleGenerator.annotations) for name, value in (("fontSize", em), ("lineWeight", Pen.THIN / Length.POINTS))},
                **{
                    f"alignedDimensions[{index}].{name}": value
                    for index, _ in enumerate(scene.StyleGenerator.alignedDimensions)
                    for name, value in (
                        ("fontSize", em),
                        ("lineWeight", Pen.THIN / Length.POINTS),
                        ("dimOffset", units.first_offset * sheet),
                        ("dimLeaderOffset", units.offset * sheet),
                        ("endcapSize", units.text / Length.POINTS / (0.8 * sqrt(2))),
                    )
                },
            },
        ),
    )
    yield from ((path, struct, rows()) for path, rows in groups if (struct := struct_at(scene, path)) is not None)


def preferred(units: Units, preferences: bpy.types.Preferences) -> Iterator[tuple[str, bpy.types.bpy_struct, Mapping[str, object]]]:
    """Application-wide lengths a scene reads in its own system by struct path, struct, and rows, disabled add-ons skipped."""
    addons = {addon.module.rpartition(".")[2]: addon.preferences for addon in preferences.addons}
    groups = (
        ("edit", preferences.edit, lambda: {"collection_instance_empty_size": units.grid}),
        ("dimensions", addons.get("dimensions"), lambda: {"default_offset_distance": units.first_offset * units.sheet_scale, "empty_display_size": 2 * units.snap}),
        ("cad2cube", addons.get("cad2cube"), lambda: {"default_scale": units.page}),
        ("univ", addons.get("univ"), lambda: {"texel_unit": {Length.FEET: "ft", Length.MILLIMETERS: "m"}[units.length]}),
    )
    yield from ((path, struct, rows()) for path, struct, rows in groups if struct is not None)


def parameters(units: Units, preferences: bpy.types.Preferences) -> Iterator[tuple[str, bpy.types.bpy_struct, Mapping[str, object]]]:
    """Bonsai's default element parameters by struct path, struct, and rows while Bonsai is enabled, declared and snapped lengths under IMPERIAL and defaults under METRIC."""
    openings = {
        "door.overall_width": 3 * Length.FEET,
        "door.overall_height": 7 * Length.FEET,
        "window.overall_width": 3 * Length.FEET,
        "window.overall_height": 4 * Length.FEET,
        "stair.width": 3 * Length.FEET + 8 * Length.INCHES,
        "stair.height": 9 * Length.FEET,
        "stair.tread_run": 11 * Length.INCHES,
        "railing.height": 3 * Length.FEET + 6 * Length.INCHES,
    }
    imperial = (
        openings
        | {
            f"window.{order}_{kind}_offset": openings[f"window.overall_{dimension}"] * share
            for order, share in (("first", 1 / 3), ("second", 2 / 3))
            for kind, dimension in (("mullion", "width"), ("transom", "height"))
        }
        | {"stair.number_of_treads": ceil(openings["stair.height"] / (7 * Length.INCHES)) - 1}
    )
    for bonsai in [addon.preferences for addon in preferences.addons if addon.module.rpartition(".")[2] == "bonsai" and addon.preferences is not None]:
        defaults = {
            f"{group.identifier}.{prop.identifier}": tuple(prop.default_array) if prop.array_length else prop.default
            for group in bonsai.bl_rna.properties["default_parameters"].fixed_type.properties
            if isinstance(group, bpy.types.PointerProperty)
            for prop in group.fixed_type.properties
            if (isinstance(prop, bpy.types.FloatProperty) and prop.unit == "LENGTH" and prop.identifier != "total_length_target") or prop.identifier == "number_of_treads"
        }
        lengths = {path: tuple(snapped(units, part) for part in default) if isinstance(default, tuple) else snapped(units, default) for path, default in defaults.items() if path not in imperial}
        yield "bonsai", bonsai, {f"default_parameters.{path}": value for path, value in (defaults if units is Units.METRIC else defaults | lengths | imperial).items()}


def dialogs(window_manager: bpy.types.WindowManager, preferences: bpy.types.Preferences) -> Iterator[tuple[str, bpy.types.bpy_struct, Mapping[str, object]]]:
    """BlenderGIS OSM import dialog values by operator while BlenderGIS is enabled, a 13 ft level of US mixed-use floors with one collection per tag."""
    level = 13 * Length.FEET
    yield from (
        (idname, window_manager.operator_properties_last(idname), {"levelHeight": level, "defaultHeight": float(round(level)), "separate": True})
        for idname in (("importgis.osm_query", "importgis.osm_file") if "BlenderGIS" in preferences.addons else ())
    )


# --- [WRITES]
def stored(value: object) -> object:
    """Value in the form writes compare, a float at single precision to four places, where a byte-stored color and its written fraction meet."""
    match value:
        case float():
            return round(array("f", (value,))[0], 4)
        case tuple():
            return tuple(map(stored, value))
        case _:
            return value


def current(owner: bpy.types.bpy_struct, name: str) -> object:
    """Member value on the struct, an array as a tuple."""
    value = getattr(owner, name)
    match prop := owner.bl_rna.properties[name]:
        case bpy.types.FloatProperty() | bpy.types.IntProperty() | bpy.types.BoolProperty() if prop.array_length:
            return tuple(value)
        case _:
            return value


def write(rows: Iterator[tuple[str, bpy.types.bpy_struct, Mapping[str, object]]], assign: Callable[[bpy.types.bpy_struct, str, object], None] = setattr) -> None:
    """Write every row whose value differs through the assignment, Bonsai's parameters by item, which skips the update callbacks that rebuild the active object."""
    for _, struct, group in rows:
        for path, value in group.items():
            parent, _, name = path.rpartition(".")
            owner = struct.path_resolve(parent) if parent else struct
            if stored(current(owner, name)) != stored(value):
                assign(owner, name, value)


def set_dialogs() -> None:
    """Set the OSM import dialogs to their declared values from a timer, after a file load clears the operators' last properties."""
    write(dialogs(bpy.context.window_manager, bpy.context.preferences))


@persistent
def match_loaded_scene(_file: str) -> None:
    """Set the application-wide lengths to the loaded scene's unit system and queue the OSM dialog values."""
    units = Units[bpy.context.scene.unit_settings.system]
    write(preferred(units, bpy.context.preferences))
    write(parameters(units, bpy.context.preferences), setitem)
    if not bpy.app.timers.is_registered(set_dialogs):
        bpy.app.timers.register(set_dialogs, first_interval=0.0)


# --- [SWITCH]
def switch(self: bpy.types.Panel, _context: bpy.types.Context) -> None:
    """Unit switch at the head of the scene's Units panel."""
    self.layout.operator_menu_enum(CONTROL_OT_units.bl_idname, "system", text="Switch Units")


class CONTROL_OT_units(bpy.types.Operator):
    """Switch the scene and the library scene between unit systems with every setting that follows the system."""

    bl_idname = "control.units"
    bl_label = "Units"
    bl_options = {"REGISTER", "UNDO"}

    system: EnumProperty(name="System", items=[(member.name, member.name.title(), "") for member in Units])

    @override
    def invoke(self, context: bpy.types.Context | None, event: bpy.types.Event | None) -> "set[OperatorReturnItems]":
        return self.execute(context) if self.properties.is_property_set("system") else context.window_manager.invoke_props_dialog(self)

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        units = Units[self.system]
        for scene in dict.fromkeys(scene for scene in (context.scene, bpy.data.scenes.get("Library")) if scene is not None):
            with context.temp_override(scene=scene):
                write(declared(units, scene))
        write(preferred(units, context.preferences))
        write(parameters(units, context.preferences), setitem)
        return {"FINISHED"}


# --- [COMPOSITION] ----------------------------------------------------------------------

CLASSES: Final = (CONTROL_OT_units,)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CLASSES", "cap", "current", "declared", "match_loaded_scene", "parameters", "preferred", "set_dialogs", "snapped", "stored", "switch"]
