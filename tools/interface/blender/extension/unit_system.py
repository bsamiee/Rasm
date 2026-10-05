# ty: ignore[invalid-argument-type, invalid-return-type, invalid-type-form, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, no-any-return, return-value, union-attr, untyped-decorator, valid-type"
# ruff: file-ignore[invalid-class-name, mutable-class-default, relative-imports]
"""Scene unit systems with every setting that follows them, the RNA forms converges compare in, the switch operator, and the file-load handler."""

from collections.abc import Callable, Iterable, Iterator, Mapping
from functools import cache, reduce
from importlib import import_module
from math import ceil, sqrt
from operator import setitem
import sys
from typing import Final, override, TYPE_CHECKING

import attrs
import blf
import bpy
from bpy.app.handlers import persistent
from bpy.props import EnumProperty

from .report import single, subscript
from .units import Length, Pen, Units

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import OperatorReturnItems

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Group:
    """Values under one struct by member path, the report label of the struct, and the assignment that writes a member."""

    label: str
    struct: "bpy.types.bpy_struct[object]"
    values: Mapping[str, object]
    assign: "Callable[[bpy.types.bpy_struct[object], str, object], None]" = setattr


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [READS]
def scene_units(scene: bpy.types.Scene) -> Units | None:
    """Unit system the scene displays, None for a scene without one."""
    return Units.__members__.get(scene.unit_settings.system)


def struct_at(root: "bpy.types.bpy_struct[object]", path: str) -> "bpy.types.bpy_struct[object] | None":
    """Struct at the dotted path under the root, None when a step is unregistered or unset."""

    def step(struct: "bpy.types.bpy_struct[object] | None", name: str) -> "bpy.types.bpy_struct[object] | None":
        return getattr(struct, name) if struct is not None and name in struct.bl_rna.properties else None

    return reduce(step, path.split("."), root)


@cache
def cap(path: str) -> float:
    """Cap height of the font file in ems."""
    font = blf.load(path)
    try:
        blf.size(font, 1000)
        return blf.dimensions(font, "H")[1] / 1000
    finally:
        blf.unload(path)


def preset(camera: bpy.types.PropertyGroup, sheet: int) -> str:
    """Bonsai drawing scale preset at the sheet scale from its preset list for the context scene's unit system."""
    return next(identifier for identifier, *_ in sys.modules[type(camera).__module__].get_diagram_scales(camera, bpy.context) if identifier.endswith(f"|1/{sheet}"))


# --- [LENGTHS]
def snapped(units: Units, value: float) -> float:
    """Length as a grid multiple from one grid step up and a snap multiple below, one snap at least for a positive length."""
    step = units.grid if value >= units.grid else units.snap
    return step * max(round(value / step), 1 if value > 0 else 0)


def profile(units: Units, settings: bpy.types.PropertyGroup) -> dict[str, float]:
    """Curve Profile Creator lengths by member over the add-on defaults, each arc depth the quarter-circle sagitta of its chord."""
    lengths = {
        name: value if units is Units.IMPERIAL else settings.bl_rna.properties[name].default
        for members, value in (
            (("arch_width", "arch_height"), 5 * Length.INCHES / 4),
            (("arch_fillet_a", "arch_fillet_b"), Length.INCHES / 4),
            (("arch_fascia", "arch_secondary_width", "arch_secondary_height", "arch_tread1", "arch_tread2"), 3 * Length.INCHES / 4),
            (("arch_rise1", "arch_rise2"), Length.INCHES / 2),
            (("parametric_width",), 2 * Length.INCHES),
            (("parametric_height",), Length.INCHES),
        )
        for name in members
    }
    geometry = import_module(".primitive_geometry", sys.modules[type(settings).__module__].__package__)
    chords = (("arch_arc_depth", "arch_width"), ("arch_secondary_arc_depth", "arch_secondary_width"), ("parametric_arc_depth", "parametric_width"))
    return {**lengths, **{depth: geometry.default_arc_depth(lengths[chord]) for depth, chord in chords}}


def extent(units: Units, scene: bpy.types.Scene) -> dict[str, float]:
    """Blosm import extent one modeling extent each way around the scene's georeference, with its snapped level height."""
    mercator = import_module(f"{type(scene.blosm).__module__.partition('.')[0]}.util.transverse_mercator").TransverseMercator(lat=scene["lat"], lon=scene["lon"])
    (south, west), (north, east) = (mercator.toGeographic(corner, corner) for corner in (-units.extent, units.extent))
    return {"minLat": south, "minLon": west, "maxLat": north, "maxLon": east, "levelHeight": snapped(units, scene.blosm.bl_rna.properties["levelHeight"].default)}


# --- [SETTINGS]
def declared(units: Units, scene: bpy.types.Scene, preferences: bpy.types.Preferences) -> Iterator[Group]:
    """Scene settings of the unit system by registered struct, the system first since its write resets the unit tokens, each group read after the one before it is written."""
    by_units: Mapping[Units, Mapping[str, Mapping[str, object]]] = {
        Units.IMPERIAL: {
            "unit_settings": {"use_separate": True, "mass_unit": "POUNDS", "temperature_unit": "FAHRENHEIT"},
            "scale_interactive_settings": {"system_unit": "IMPERIAL_IN"},
            "BIMProperties": {"area_unit": "square foot", "volume_unit": "cubic foot", "mass_unit": "pound"},
        },
        Units.METRIC: {
            "unit_settings": {"use_separate": False, "mass_unit": "KILOGRAMS", "temperature_unit": "CELSIUS"},
            "scale_interactive_settings": {"system_unit": "METRIC", "metric_unit": "MM"},
            "BIMProperties": {"area_unit": "SQUARE_METRE", "volume_unit": "CUBIC_METRE", "mass_unit": "KILO/GRAM"},
        },
    }
    tokens, sheet, em = by_units[units], units.sheet_scale, units.text / Length.POINTS / cap(preferences.view.font_path_ui)
    groups = (
        ("unit_settings", lambda: {"system": units.name, "scale_length": 1.0, "length_unit": units.length.name, **tokens["unit_settings"], "time_unit": "SECONDS", "system_rotation": "DEGREES"}),
        ("tool_settings", lambda: {"double_threshold": units.tolerance, "proportional_distance": units.grid}),
        ("eevee", lambda: {"volumetric_end": units.far}),
        ("camera.data", lambda: {"clip_start": units.snap, "clip_end": units.far, "display_size": units.grid, "ortho_scale": units.paper[0] * sheet}),
        ("camera.data.BIMCameraProperties", lambda: {"diagram_scale": preset(scene.camera.data.BIMCameraProperties, sheet), "width": units.paper[0] * sheet, "height": units.paper[1] * sheet}),
        ("cpc_settings", lambda: {"merge_tolerance": units.tolerance, **profile(units, scene.cpc_settings)}),
        ("scale_interactive_settings", lambda: {**tokens["scale_interactive_settings"], "decimal_precision": str(units.places(units.page, 10))}),
        *((("blosm", lambda: extent(units, scene)),) if {"lat", "lon"} <= set(scene.keys()) else ()),
        ("BIMProperties", lambda: {**tokens["BIMProperties"], "time_unit": "SECOND"}),
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
    yield from (Group(f"{subscript('scenes', scene.name)}.{path}", struct, rows()) for path, rows in groups if (struct := struct_at(scene, path)) is not None)


def parameters(units: Units, bonsai: bpy.types.AddonPreferences) -> dict[str, object]:
    """Bonsai's default element parameters, declared openings over snapped defaults under IMPERIAL and the defaults under METRIC."""
    defaults = {
        f"{group.identifier}.{prop.identifier}": tuple(prop.default_array) if prop.array_length else prop.default
        for group in bonsai.bl_rna.properties["default_parameters"].fixed_type.properties
        if isinstance(group, bpy.types.PointerProperty)
        for prop in group.fixed_type.properties
        if (isinstance(prop, bpy.types.FloatProperty) and prop.unit == "LENGTH" and prop.identifier != "total_length_target") or prop.identifier == "number_of_treads"
    }
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
    lengths = {
        path: tuple(snapped(Units.IMPERIAL, part) for part in default) if isinstance(default, tuple) else snapped(Units.IMPERIAL, default) for path, default in defaults.items() if path not in imperial
    }
    return {f"default_parameters.{path}": value for path, value in (defaults | lengths | imperial if units is Units.IMPERIAL else defaults).items()}


def preferred(units: Units, preferences: bpy.types.Preferences) -> Iterator[Group]:
    """Application-wide lengths of the unit system, by enabled add-on holding preferences, Bonsai's parameters written by item."""
    addons = {addon.module.rpartition(".")[2]: addon for addon in preferences.addons}
    groups = (
        ("dimensions", lambda _: {"default_offset_distance": units.first_offset * units.sheet_scale, "empty_display_size": 2 * units.snap}, setattr),
        ("cad2cube", lambda _: {"default_scale": units.page}, setattr),
        ("univ", lambda _: {"texel_unit": "ft" if units is Units.IMPERIAL else "m"}, setattr),
        ("bonsai", lambda bonsai: parameters(units, bonsai), setitem),
    )
    yield Group("preferences.edit", preferences.edit, {"collection_instance_empty_size": units.grid})
    yield from (
        Group(f"{subscript('preferences.addons', addon.module)}.preferences", addon.preferences, rows(addon.preferences), assign)
        for name, rows, assign in groups
        if (addon := addons.get(name)) is not None and addon.preferences is not None
    )


def resolved(units: Units, scenes: Iterable[bpy.types.Scene], preferences: bpy.types.Preferences) -> Iterator[Group]:
    """Settings of the unit system in each scene, each read and written with that scene as the context scene, then the application-wide ones."""
    for scene in scenes:
        with bpy.context.temp_override(scene=scene):
            yield from declared(units, scene, preferences)
    yield from preferred(units, preferences)


def dialogs(units: Units, window_manager: bpy.types.WindowManager, preferences: bpy.types.Preferences) -> Iterator[Group]:
    """BlenderGIS OSM import dialog values while BlenderGIS is enabled, a 13 ft level of US mixed-use floors snapped to the system, an untagged building one level tall, and one collection per tag."""
    level = snapped(units, 13 * Length.FEET)
    yield from (
        Group(subscript("operators", idname), window_manager.operator_properties_last(idname), {"levelHeight": level, "defaultHeight": level, "separate": True})
        for idname in (("importgis.osm_query", "importgis.osm_file") if "BlenderGIS" in preferences.addons else ())
    )


# --- [FORMS]
def stored(value: object) -> object:
    """Value in the form RNA stores it, a float cast to single precision and an array element by element."""
    match value:
        case float():
            return single(value)
        case tuple():
            return tuple(map(stored, value))
        case _:
            return value


def member(struct: "bpy.types.bpy_struct[object]", path: str) -> "tuple[bpy.types.bpy_struct[object], str]":
    """Owning struct and member name of the dotted path under the struct."""
    parent, _, name = path.rpartition(".")
    return struct.path_resolve(parent) if parent else struct, name


def current(owner: "bpy.types.bpy_struct[object]", name: str) -> object:
    """Member value on the struct in the form stored values compare against, an array as a tuple."""
    value = getattr(owner, name)
    match prop := owner.bl_rna.properties[name]:
        case bpy.types.FloatProperty() | bpy.types.IntProperty() | bpy.types.BoolProperty() if prop.array_length:
            return tuple(value)
        case _:
            return value


def write(groups: Iterable[Group]) -> None:
    """Write every member whose current value differs from the stored form of its value, through its group's assignment."""
    for group in groups:
        for path, value in group.values.items():
            owner, name = member(group.struct, path)
            if current(owner, name) != stored(value):
                group.assign(owner, name, value)


# --- [HANDLERS]
def set_dialogs() -> None:
    """Set the OSM import dialogs to the context scene's values from a timer, after a file load clears the operators' last properties."""
    if (units := scene_units(bpy.context.scene)) is not None:
        write(dialogs(units, bpy.context.window_manager, bpy.context.preferences))


@persistent
def match_loaded_scene(_file: str) -> None:
    """Set the application-wide lengths to the loaded scene's unit system and queue the OSM dialog values."""
    if (units := scene_units(bpy.context.scene)) is not None:
        write(preferred(units, bpy.context.preferences))
        if not bpy.app.timers.is_registered(set_dialogs):
            bpy.app.timers.register(set_dialogs, first_interval=0.0)


# --- [SWITCH]
def switch(self: bpy.types.Panel, _context: bpy.types.Context) -> None:
    """Unit switch at the head of the scene's Units panel."""
    self.layout.operator_menu_enum(INTERFACE_OT_units.bl_idname, "system", text="Switch Units")


class INTERFACE_OT_units(bpy.types.Operator):
    """Switch every scene of the file and the application-wide lengths between unit systems with every setting that follows the system."""

    bl_idname = "interface.units"
    bl_label = "Units"
    bl_options = {"REGISTER", "UNDO"}

    system: EnumProperty(name="System", items=[(member.name, member.name.title(), "") for member in Units])

    @override
    def invoke(self, context: bpy.types.Context | None, event: bpy.types.Event | None) -> "set[OperatorReturnItems]":
        return self.execute(context) if self.properties.is_property_set("system") else context.window_manager.invoke_props_dialog(self)

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        write(resolved(Units[self.system], bpy.data.scenes, context.preferences))
        return {"FINISHED"}


# --- [COMPOSITION] ----------------------------------------------------------------------

CLASSES: Final = (INTERFACE_OT_units,)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CLASSES", "Group", "cap", "current", "match_loaded_scene", "member", "resolved", "scene_units", "set_dialogs", "snapped", "stored", "switch", "write"]
