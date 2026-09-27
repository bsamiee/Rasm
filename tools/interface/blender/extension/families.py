# ty: ignore[invalid-argument-type, invalid-assignment, invalid-type-form, not-iterable, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, union-attr, unreachable, valid-type"
# ruff: file-ignore[invalid-class-name, mutable-class-default, relative-imports]
"""Command aliases per editor, the operators they add, and the leader that runs an alias typed in the status bar."""

from collections.abc import Callable, Mapping
from math import pi
from types import MappingProxyType
from typing import Final, override, TYPE_CHECKING

import attrs
import bpy
from bpy.props import EnumProperty, StringProperty

from .aliases import COMMAND_ALIASES, FAMILIES
from .navigation import KEYMAPS
from .system import cap, snapped
from .units import Units

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import OperatorReturnItems

# --- [CONSTANTS] ------------------------------------------------------------------------

LAST: Final = StringProperty(name="Last Alias", description="Alias the leader ran last, which Enter on an empty prompt repeats")

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen(init=False)
class Alias:
    """Alias label, the operator it runs with its properties, and the state it acts in."""

    label: str
    operator: str
    enabled: Callable[[bpy.types.Context], bool]
    properties: Mapping[str, object]

    def __init__(self, label: str, operator: str, /, *, enabled: Callable[[bpy.types.Context], bool] = lambda _context: True, **properties: object) -> None:
        """Alias whose keywords past `enabled` are the operator's properties."""
        self.__attrs_init__(label, operator, enabled, MappingProxyType(properties))


# --- [TABLES] ---------------------------------------------------------------------------

ALIASES: Final = MappingProxyType({
    "VIEW_3D": MappingProxyType({
        "Q": Alias("Line", "view3d.slvs_add_line2d", continuous_draw=False),
        "Q1": Alias("Extend Curve", "curve.extrude_move"),
        "Q2": Alias("Connect", "view3d.slvs_add_coincident"),
        "QQ": Alias("Polyline", "view3d.slvs_add_line2d"),
        "QW": Alias("Arc", "view3d.slvs_add_arc2d"),
        "QE": Alias("Interpolated Curve", "curve.primitive_bezier_curve_add"),
        "QA": Alias("Point", "view3d.slvs_add_point2d"),
        "QS": Alias("Points", "wm.tool_set_by_id", name="sketcher.slvs_add_point2d"),
        "QF": Alias("Divide by Segments", "curve.subdivide"),
        "QZ": Alias("Point Cloud", "object.convert", target="POINTCLOUD", keep_original=True),
        "W1": Alias("Rectangle Corner to Corner", "view3d.slvs_add_rectangle"),
        "WQ": Alias("Polygon", "mesh.primitive_circle_add", fill_type="NGON"),
        "WW": Alias("Box", "control.primitive", kind="CUBE"),
        "WE": Alias("Tube", "control.primitive", enabled=lambda context: context.mode == "OBJECT", kind="TUBE"),
        "WR": Alias("Cone", "control.primitive", kind="CONE"),
        "WA": Alias("Pyramid", "mesh.primitive_cone_add", vertices=4),
        "E": Alias("Circle", "view3d.slvs_add_circle2d"),
        "EV": Alias("Circle Vertical", "curve.primitive_bezier_circle_add", rotation=(pi / 2, 0.0, 0.0)),
        "EQ": Alias("Sphere", "control.primitive", kind="UVSPHERE"),
        "EW": Alias("Cylinder", "control.primitive", kind="CYLINDER"),
        "ER": Alias("Torus", "control.primitive", kind="TORUS"),
        "R": Alias("Rotate", "transform.rotate"),
        "R1": Alias("Symmetry", "object.modifier_add", type="MIRROR"),
        "R2": Alias("Mirror", "transform.mirror"),
        "RE": Alias("Orient on Surface", "transform.translate", snap=True, snap_elements={"FACE_PROJECT"}, snap_align=True),
        "RL": Alias("Rotate View Left", "control.view_orbit", type="ORBITLEFT", angle=pi / 2),
        "RLL": Alias("Rotate View Left Sideways", "control.view_orbit", type="ORBITLEFT", angle=pi),
        "RR": Alias("Rotate View Right", "control.view_orbit", type="ORBITRIGHT", angle=pi / 2),
        "RRR": Alias("Rotate View Right Sideways", "control.view_orbit", type="ORBITRIGHT", angle=pi),
        "RU": Alias("Rotate View Up", "control.view_orbit", type="ORBITUP", angle=pi),
        "RD": Alias("Rotate View Down", "control.view_orbit", type="ORBITDOWN", angle=pi),
        "T": Alias("Text", "control.text"),
        "TT": Alias("Text Object", "object.convert", target="MESH"),
        "T1": Alias("Scale 1-D", "transform.resize", constraint_axis=(True, False, False)),
        "T2": Alias("Scale 2-D", "transform.resize", constraint_axis=(True, True, False)),
        "T3": Alias("Scale 3-D", "transform.resize"),
        "TQ": Alias("Stretch", "control.deform", method="STRETCH"),
        "TW": Alias("Shear", "transform.shear"),
        "TE": Alias("Taper", "control.deform", method="TAPER"),
        "TA": Alias("Bend", "control.deform", method="BEND"),
        "TS": Alias("Twist", "control.deform", method="TWIST"),
        "A": Alias("Offset", "view3d.slvs_offset"),
        "A2": Alias("Offset Surface", "object.modifier_add", type="SOLIDIFY"),
        "A3": Alias("Inset", "mesh.inset"),
        "AQ": Alias("Curve Boolean", "view3d.slvs_node_boolean"),
        "AW": Alias("Boolean Union", "object.boolean_auto_union"),
        "AE": Alias("Boolean Difference", "object.boolean_auto_difference"),
        "AR": Alias("Boolean Intersection", "object.boolean_auto_intersect"),
        "AT": Alias("Boolean Split", "object.boolean_auto_slice"),
        "AA": Alias("Array", "object.modifier_add", type="ARRAY"),
        "AF": Alias("Flow", "object.modifier_add", type="CURVE"),
        "AZ": Alias("Array Linear", "view3d.slvs_node_array_linear"),
        "AX": Alias("Array Polar", "mesh.spin", dupli=True),
        "S": Alias("Plane", "mesh.primitive_plane_add"),
        "S1": Alias("Extend Surface", "mesh.extrude_edges_move"),
        "S4": Alias("Surface from Planar Curves", "mesh.edge_face_add"),
        "S5": Alias("Surface from Edge Curves", "mesh.fill_grid"),
        "SV": Alias("Plane Vertical", "mesh.primitive_plane_add", rotation=(pi / 2, 0.0, 0.0)),
        "SQ": Alias("Sweep 1", "cpc.apply_profile_to_curve"),
        "SE": Alias("Loft", "mesh.bridge_edge_loops"),
        "SR": Alias("Revolve", "view3d.slvs_node_revolve"),
        "ST": Alias("Project", "mesh.knife_project"),
        "SA": Alias("Cap", "mesh.fill_holes"),
        "SS": Alias("Shell", "object.modifier_add", type="SOLIDIFY"),
        "D": Alias("Distance", "dimensions.measure"),
        "DL": Alias("Length", "wm.context_toggle", data_path="space_data.overlay.show_extra_edge_length"),
        "DA": Alias("Area", "dimensions.create_area"),
        "DV": Alias("Volume", "wm.context_toggle", data_path="scene.dimensions_settings.show_selected_object_overlay"),
        "DQ": Alias("Dimension Aligned", "dimensions.create_dimension"),
        "DW": Alias("Dimension Angle", "dimensions.create_angle"),
        "DE": Alias("Dimension Diameter", "view3d.slvs_add_diameter"),
        "DR": Alias("Dimension Radius", "view3d.slvs_add_diameter", setting=True),
        "DS": Alias("Leader", "measureit_arch.addannotationbutton"),
        "F": Alias("Trim", "view3d.slvs_trim"),
        "FF": Alias("Join", "object.join"),
        "F1": Alias("Fillet", "view3d.slvs_bevel"),
        "F2": Alias("Chamfer", "mesh.bevel", segments=1),
        "FQ": Alias("Split", "mesh.split"),
        "FW": Alias("Divide", "mesh.subdivide"),
        "FE": Alias("Explode", "mesh.separate", type="LOOSE"),
        "FA": Alias("Infinite Plane", "mesh.bisect"),
        "FS": Alias("Cutting Plane", "bim.add_section_plane"),
        "FD": Alias("Wire Cut", "object.carve_polyline"),
        "G": Alias("Group", "collection.create"),
        "GU": Alias("Ungroup", "collection.objects_remove"),
        "GH": Alias("Hide", "object.hide_view_set"),
        "GJ": Alias("Show", "object.hide_view_clear"),
        "GI": Alias("Isolate", "view3d.localview", enabled=lambda context: context.space_data.local_view is None),
        "GO": Alias("Unisolate", "view3d.localview", enabled=lambda context: context.space_data.local_view is not None),
        "GL": Alias("Lock", "control.lock"),
        "GP": Alias("Unlock", "control.unlock"),
        "GW": Alias("Add Guides", "dimensions.create_guide"),
        "GE": Alias("Remove Guides", "dimensions.clear_guides"),
        "Z": Alias("Zoom Window", "view3d.zoom_border"),
        "ZZ": Alias("Zoom Target", "view3d.view_center_pick"),
        "ZE": Alias("Zoom Extents", "view3d.view_all"),
        "ZS": Alias("Zoom Selected", "view3d.view_selected"),
        "ZF": Alias("Front", "control.view_axis", type="FRONT"),
        "ZB": Alias("Back", "control.view_axis", type="BACK"),
        "ZT": Alias("Top", "control.view_axis", type="TOP"),
        "ZL": Alias("Left", "control.view_axis", type="LEFT"),
        "ZR": Alias("Right", "control.view_axis", type="RIGHT"),
        "ZC": Alias("Sketch Plane", "view3d.slvs_align_view"),
        "ZP": Alias("Perspective", "control.view_persportho", enabled=lambda context: context.region_data.view_perspective != "PERSP"),
        "X": Alias("Push Pull", "mesh.extrude_region_shrink_fatten"),
        "XQ": Alias("Extrude Curve", "view3d.slvs_node_extrude"),
        "XW": Alias("Extrude Surface", "mesh.extrude_region_move"),
        "XE": Alias("Extract Surface", "mesh.separate", type="SELECTED"),
        "XR": Alias("Project to Plane", "transform.resize", value=(1.0, 1.0, 0.0), center_override=(0.0, 0.0, 0.0)),
        "C": Alias("Copy", "object.duplicate_move"),
        "CC": Alias("Plane 3 Points", "transform.create_orientation", enabled=lambda context: context.mode == "EDIT_MESH", use=True),
        "CT": Alias("World Top", "view3d.slvs_set_active_sketch", sketch_name=""),
        "CS": Alias("Plane to Surface", "wm.tool_set_by_id", name="sketcher.slvs_add_sketch"),
        "CO": Alias("Plane to Object", "transform.create_orientation", enabled=lambda context: context.mode == "OBJECT", use=True),
        "CW": Alias("Clipping Plane", "bim.create_clipping_plane"),
        "CE": Alias("Clipping Box", "view3d.clip_border", enabled=lambda context: not context.region_data.use_clip_planes),
        "CD": Alias("Clipping Off", "view3d.clip_border", enabled=lambda context: context.region_data.use_clip_planes),
        "V": Alias("Move", "transform.translate"),
        "VA": Alias("Align", "object.align"),
        "VK": Alias("Set Point", "view3d.snap_selected_to_cursor"),
        "VV": Alias("Brush Select", "view3d.select_circle"),
        "VD": Alias("Select Dimensions", "wm.tool_set_by_id", name="dimensions.annotation_selection"),
        "VB": Alias("Select Instances", "control.select_instances"),
        "VO": Alias("Select All", "object.select_all", action="SELECT"),
        "VI": Alias("Invert Selection", "object.select_all", action="INVERT"),
        "VE": Alias("Select Collection", "object.select_grouped", type="COLLECTION"),
        "B": Alias("Block", "object.library_instance"),
        "BN": Alias("Rename", "wm.batch_rename"),
        "BQ": Alias("Create Unique Block", "object.make_single_user_library_instance"),
        "BW": Alias("Add Objects to Block", "object.add_to_library_instance"),
        "BE": Alias("Block Manager", "wm.call_panel", name="VIEW3D_PT_library_instance_menu"),
        "BA": Alias("Block Edit", "object.edit_library_instance_skip_testing"),
        "BS": Alias("Replace Block", "object.make_links_data", type="DUPLICOLLECTION"),
        "BD": Alias("Explode Block", "object.library_instance_ungroup"),
        "BF": Alias("Reset Scale", "object.scale_clear"),
        "MF": Alias("Merge Faces", "mesh.dissolve_limited"),
        "MS": Alias("Merge Surfaces", "mesh.dissolve_faces"),
        "MC": Alias("Merge Curves", "curve.make_segment"),
        "ME": Alias("Merge Edges", "mesh.dissolve_edges"),
        "ML": Alias("Match Collection", "object.make_links_data", type="GROUPS"),
        "MP": Alias("Match Material", "object.make_links_data", type="MATERIAL"),
        "MM": Alias("Match Mapping", "object.join_uvs"),
        "LG": Alias("New Layout", "measureit_arch.addviewbutton"),
        "IM": Alias("Import", "wm.call_menu", name="TOPBAR_MT_file_import"),
        "IN": Alias("Insert", "object.collection_instance_add"),
        "PQ": Alias("Purge", "outliner.orphans_purge"),
        "PW": Alias("Merge by Distance", "mesh.remove_doubles"),
    }),
    "NODE_EDITOR": MappingProxyType({
        "F": Alias("Trim", "node.links_cut"),
        "FQ": Alias("Split", "node.links_detach"),
        "G": Alias("Frame", "node.join"),
        "GU": Alias("Detach", "node.detach"),
        "GH": Alias("Hide", "node.hide_toggle"),
        "Z": Alias("Zoom Window", "view2d.zoom_border"),
        "ZE": Alias("Zoom Extents", "node.view_all"),
        "ZS": Alias("Zoom Selected", "node.view_selected"),
        "C": Alias("Copy", "node.duplicate_move"),
        "V": Alias("Move", "node.translate_attach"),
        "VA": Alias("Align", "node.nw_align_nodes"),
        "VO": Alias("Select All", "node.select_all", action="SELECT"),
        "VI": Alias("Invert Selection", "node.select_all", action="INVERT"),
        "VE": Alias("Select Grouped", "node.select_grouped"),
        "B": Alias("Group", "node.group_make"),
        "BA": Alias("Edit Group", "node.group_edit"),
        "BS": Alias("Replace Group", "wm.call_menu", name="NODE_MT_group_swap"),
        "BD": Alias("Ungroup", "node.group_ungroup"),
        "BW": Alias("Add to Group", "node.group_insert"),
        "IN": Alias("Insert", "wm.call_menu", name="NODE_MT_group_add"),
        "PQ": Alias("Purge", "outliner.orphans_purge"),
    }),
})
PRIMITIVES: Final = MappingProxyType({
    "CUBE": ("Cube", (bpy.ops.mesh.primitive_cube_add, MappingProxyType({"size": 1.0}))),
    "TUBE": ("Tube", None),
    "CONE": ("Cone", (bpy.ops.mesh.primitive_cone_add, MappingProxyType({"radius1": 2.0, "radius2": 1.0, "depth": 1.0}))),
    "UVSPHERE": ("UV Sphere", (bpy.ops.mesh.primitive_uv_sphere_add, MappingProxyType({"radius": 1.0}))),
    "CYLINDER": ("Cylinder", (bpy.ops.mesh.primitive_cylinder_add, MappingProxyType({"radius": 1.0, "depth": 1.0}))),
    "TORUS": ("Torus", (bpy.ops.mesh.primitive_torus_add, MappingProxyType({"major_radius": 1.0, "minor_radius": 0.5}))),
})

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [OPERATORS]
class AliasOperator(bpy.types.Operator):
    """Base of the alias operators with a redo panel and an undo step."""

    bl_options = {"REGISTER", "UNDO"}


class CONTROL_OT_deform(AliasOperator):
    """Add a Simple Deform modifier with the method to the active object."""

    bl_idname = "control.deform"
    bl_label = "Simple Deform"

    method: EnumProperty(name="Method", items=[(item.identifier, item.name, item.description) for item in bpy.types.SimpleDeformModifier.bl_rna.properties["deform_method"].enum_items])

    @classmethod
    @override
    def poll(cls, context: bpy.types.Context | None) -> bool:
        return context.object is not None

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        context.object.modifiers.new(self.bl_label, "SIMPLE_DEFORM").deform_method = self.method
        return {"FINISHED"}


class CONTROL_OT_lock(AliasOperator):
    """Lock the selected objects against selection."""

    bl_idname = "control.lock"
    bl_label = "Lock"

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        for target in context.selected_objects:
            target.hide_select = True
        return {"FINISHED"}


class CONTROL_OT_unlock(AliasOperator):
    """Unlock every object of the view layer."""

    bl_idname = "control.unlock"
    bl_label = "Unlock"

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        for target in context.view_layer.objects:
            target.hide_select = False
        return {"FINISHED"}


class CONTROL_OT_text(AliasOperator):
    """Add annotation text at the 3D cursor at the sheet's text height."""

    bl_idname = "control.text"
    bl_label = "Text"

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        units, face = Units[context.scene.unit_settings.system], context.preferences.view.font_path_ui
        curve = bpy.data.curves.new(self.bl_label, "FONT")
        curve.font, curve.size, curve.align_x = bpy.data.fonts.load(face, check_existing=True), units.text * units.sheet_scale / cap(face), "LEFT"
        target = bpy.data.objects.new(self.bl_label, curve)
        target.location = context.scene.cursor.location
        next((child for child in context.scene.collection.children_recursive if child.get("dimensions_collection_role") == "DIMENSIONS"), context.collection).objects.link(target)
        for other in context.selected_objects:
            other.select_set(state=False)
        target.select_set(state=True)
        context.view_layer.objects.active = target
        return {"FINISHED"}


class CONTROL_OT_select_instances(AliasOperator):
    """Add every selectable collection instance of the view layer to the selection."""

    bl_idname = "control.select_instances"
    bl_label = "Select Instances"

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        for target in [target for target in context.selectable_objects if target.instance_type == "COLLECTION"]:
            target.select_set(state=True)
        return {"FINISHED"}


class CONTROL_OT_primitive(AliasOperator):
    """Add a Modern Primitive at grid and snap lengths of the scene's unit system, or the stock primitive in Edit Mode."""

    bl_idname = "control.primitive"
    bl_label = "Primitive"

    kind: EnumProperty(name="Kind", items=[(kind, label, "") for kind, (label, _) in PRIMITIVES.items()])

    @classmethod
    @override
    def poll(cls, context: bpy.types.Context | None) -> bool:
        return context.mode == "EDIT_MESH" or (context.mode == "OBJECT" and {f"mpr_make_{kind.lower()}" for kind in PRIMITIVES} <= set(dir(bpy.ops.mesh)))

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        units, (label, stock) = Units[context.scene.unit_settings.system], PRIMITIVES[self.kind]
        match context.mode, stock:
            case "EDIT_MESH", None:
                self.report({"ERROR"}, f"{label} has no Edit Mode form")
                return {"CANCELLED"}
            case "EDIT_MESH", (operator, lengths):
                return operator(**{name: snapped(units, length) for name, length in lengths.items()})
            case _:
                getattr(bpy.ops.mesh, f"mpr_make_{self.kind.lower()}")()
        target = context.view_layer.objects.active
        modifier = target.modifiers[0]
        for item in modifier.node_group.interface.items_tree:
            increment = item.name.startswith("Snap ")
            match item:
                case bpy.types.NodeTreeInterfaceSocketVector(in_out="INPUT", subtype="TRANSLATION"):
                    socket = getattr(modifier.properties.inputs, item.identifier)
                    socket.value = [units.snap if increment else snapped(units, part) for part in socket.value]
                case bpy.types.NodeTreeInterfaceSocketFloat(in_out="INPUT", subtype="DISTANCE"):
                    socket = getattr(modifier.properties.inputs, item.identifier)
                    socket.value = units.snap if increment else snapped(units, socket.value)
                case _:
                    pass
        for block in (target, target.data):
            group = block.bl_system_properties_get(do_create=True)
            for key in set(group.keys()) - set(block.bl_rna.properties.keys()):
                del group[key]
        target.update_tag()
        return {"FINISHED"}


# --- [LEADER]
class CONTROL_OT_alias(bpy.types.Operator):
    """Type an alias in the status bar and run it with Enter or Space, the last alias on an empty prompt."""

    bl_idname = "control.alias"
    bl_label = "Alias"

    @override
    def invoke(self, context: bpy.types.Context | None, event: bpy.types.Event | None) -> "set[OperatorReturnItems]":
        self.present = {
            name: (alias, getattr(submodule, member))
            for name, alias in ALIASES[context.area.type].items()
            for category, _, member in (alias.operator.partition("."),)
            if name in COMMAND_ALIASES and member in dir(submodule := getattr(bpy.ops, category))
        }
        self.acting = {name: alias.enabled(context) and operator.poll() for name, (alias, operator) in self.present.items()}
        self.typed = ""
        context.workspace.status_text_set(self.status())
        context.window_manager.modal_handler_add(self)
        return {"RUNNING_MODAL"}

    @override
    def modal(self, context: bpy.types.Context | None, event: bpy.types.Event | None) -> "set[OperatorReturnItems]":
        match event.type, event.value:
            case "ESC" | "RIGHTMOUSE", "PRESS":
                self.cancel(context)
                return {"CANCELLED"}
            case "RET" | "NUMPAD_ENTER" | "SPACE", "PRESS":
                return self.run(context, self.typed or context.window_manager.control_alias)
            case "BACK_SPACE", "PRESS":
                self.typed = self.typed[:-1]
            case _, "PRESS" if not event.is_repeat and event.unicode.isalnum() and any(name.startswith(self.typed + event.unicode.upper()) for name in self.present):
                self.typed += event.unicode.upper()
            case _, "PRESS":
                return {"RUNNING_MODAL"}
            case _:
                return {"PASS_THROUGH"}
        context.workspace.status_text_set(self.status())
        return {"RUNNING_MODAL"}

    @override
    def cancel(self, context: bpy.types.Context | None) -> None:
        context.workspace.status_text_set(None)

    def run(self, context: bpy.types.Context, name: str) -> "set[OperatorReturnItems]":
        """Run and remember the named alias when it acts in this state, and keep waiting otherwise."""
        if not self.acting.get(name, False):
            return {"RUNNING_MODAL"}
        alias, operator = self.present[name]
        context.window_manager.control_alias = name
        self.cancel(context)
        operator("INVOKE_DEFAULT", **alias.properties)
        return {"FINISHED"}

    def status(self) -> Callable[[bpy.types.Header, bpy.types.Context], None]:
        """Status bar draw function listing each family by key and first word, or the typed prefix and each matching alias grayed where it does not act."""

        def draw(header: bpy.types.Header, _context: bpy.types.Context) -> None:
            layout = header.layout
            if not self.typed:
                for key in [key for key in FAMILIES if any(name.startswith(key) for name in self.present)]:
                    layout.label(text=FAMILIES[key].split()[0].rstrip(","), icon=f"EVENT_{key}")
                return
            layout.label(text=self.typed)
            for name in [name for name in self.present if name.startswith(self.typed)]:
                row = layout.row()
                row.enabled = self.acting[name]
                row.label(text=f"{name} {self.present[name][0].label}")

        return draw


def bind(keyconfigs: bpy.types.KeyConfigurations) -> list[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem]]:
    """Add-on keymap items binding the leader to Alt and the grave accent in every aliased editor."""
    return [
        (keymap, keymap.keymap_items.new(CONTROL_OT_alias.bl_idname, "ACCENT_GRAVE", "PRESS", alt=True))
        for keymap in (keyconfigs.addon.keymaps.new(name=KEYMAPS[space], space_type=space, region_type="WINDOW") for space in ALIASES)
    ]


# --- [COMPOSITION] ----------------------------------------------------------------------

CLASSES: Final = (CONTROL_OT_deform, CONTROL_OT_lock, CONTROL_OT_unlock, CONTROL_OT_text, CONTROL_OT_select_instances, CONTROL_OT_primitive, CONTROL_OT_alias)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["bind", "CLASSES", "LAST"]
