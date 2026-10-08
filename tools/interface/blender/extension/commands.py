# ty: ignore[invalid-argument-type, invalid-assignment, invalid-type-form, unresolved-attribute, unresolved-import]
# mypy: disable-error-code="arg-type, attr-defined, explicit-any, no-any-return, union-attr, unreachable, valid-type"
# ruff: file-ignore[import-private-name, invalid-class-name, mutable-class-default]
"""Command aliases per editor, the operators they add, and the leader that runs an alias typed in the status bar."""

from collections.abc import Callable, Mapping
from math import pi
from types import MappingProxyType
from typing import Final, override, TYPE_CHECKING

from _bpy import ops
import attrs
import bpy
from bpy.props import EnumProperty, StringProperty

from .aliases import Alias, families
from .navigation import KEYMAPS
from .unit_system import cap, scene_units, snapped
from .units import Units

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import OperatorReturnItems, SpaceTypeItems

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Binding:
    """Operator an alias runs, its properties, and the state it acts in."""

    idname: str
    properties: Mapping[str, object]
    enabled: Callable[[bpy.types.Context], bool]


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [BINDINGS]
def binding(idname: str, /, *, enabled: Callable[[bpy.types.Context], bool] = lambda _context: True, **properties: object) -> Binding:
    """Binding whose keywords past `enabled` are the operator's properties."""
    return Binding(idname, MappingProxyType(properties), enabled)


def operator(idname: str) -> Callable[..., "set[OperatorReturnItems]"]:
    """Operator function of the Python idname."""
    return ops.create_function(*idname.split("."))


# --- [OPERATORS]
class AliasOperator(bpy.types.Operator):
    """Base of the alias operators with a redo panel and an undo step."""

    bl_options = {"REGISTER", "UNDO"}


class INTERFACE_OT_deform(AliasOperator):
    """Add a Simple Deform modifier with the method to the active object."""

    bl_idname = "interface.deform"
    bl_label = "Simple Deform"

    method: EnumProperty(name="Method", items=[(item.identifier, item.name, item.description) for item in bpy.types.SimpleDeformModifier.bl_rna.properties["deform_method"].enum_items])

    @classmethod
    @override
    def poll(cls, context: bpy.types.Context) -> bool:
        return context.object is not None

    @override
    def execute(self, context: bpy.types.Context) -> "set[OperatorReturnItems]":
        context.object.modifiers.new(self.bl_label, "SIMPLE_DEFORM").deform_method = self.method
        return {"FINISHED"}


class INTERFACE_OT_text(AliasOperator):
    """Add annotation text at the 3D cursor at the sheet's text height."""

    bl_idname = "interface.text"
    bl_label = "Text"

    @classmethod
    @override
    def poll(cls, context: bpy.types.Context) -> bool:
        return scene_units(context.scene) is not None

    @override
    def execute(self, context: bpy.types.Context) -> "set[OperatorReturnItems]":
        units, face = scene_units(context.scene), context.preferences.view.font_path_ui
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


class INTERFACE_OT_select_instances(AliasOperator):
    """Add every selectable collection instance of the view layer to the selection."""

    bl_idname = "interface.select_instances"
    bl_label = "Select Instances"

    @override
    def execute(self, context: bpy.types.Context) -> "set[OperatorReturnItems]":
        for target in [target for target in context.selectable_objects if target.instance_type == "COLLECTION"]:
            target.select_set(state=True)
        return {"FINISHED"}


def fit(target: bpy.types.Object, units: Units) -> None:
    """Set each length input of the primitive's node modifier to a grid or snap multiple, each snap step input to the snap length."""
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
    target.update_tag()


class INTERFACE_OT_primitive(AliasOperator):
    """Add a Modern Primitive at grid and snap lengths of the scene's unit system, or the stock primitive in Edit Mode."""

    bl_idname = "interface.primitive"
    bl_label = "Primitive"

    kind: EnumProperty(name="Kind", items=[(kind, kind, "") for kind in ("Cube", "Tube", "Cone", "UVSphere", "Cylinder", "Torus")])

    @classmethod
    @override
    def poll(cls, context: bpy.types.Context) -> bool:
        return context.mode in {"OBJECT", "EDIT_MESH"} and scene_units(context.scene) is not None

    @override
    def execute(self, context: bpy.types.Context) -> "set[OperatorReturnItems]":
        stock = {
            "Cube": ("mesh.primitive_cube_add", {"size": 1.0}),
            "Cone": ("mesh.primitive_cone_add", {"radius1": 2.0, "radius2": 1.0, "depth": 1.0}),
            "UVSphere": ("mesh.primitive_uv_sphere_add", {"radius": 1.0}),
            "Cylinder": ("mesh.primitive_cylinder_add", {"radius": 1.0, "depth": 1.0}),
            "Torus": ("mesh.primitive_torus_add", {"major_radius": 1.0, "minor_radius": 0.5}),
        }
        units = scene_units(context.scene)
        match context.mode:
            case "EDIT_MESH" if self.kind in stock:
                idname, lengths = stock[self.kind]
                return operator(idname)(**{name: snapped(units, length) for name, length in lengths.items()})
            case "EDIT_MESH":
                self.report({"ERROR"}, f"{self.kind} has no stock primitive to add in Edit Mode")
                return {"CANCELLED"}
            case _:
                operator(f"mesh.mpr_make_{self.kind.lower()}")()
                fit(context.view_layer.objects.active, units)
                return {"FINISHED"}


# --- [LEADER]
class INTERFACE_OT_alias(bpy.types.Operator):
    """Type an alias in the status bar and run it with Enter or Space, the last alias on an empty prompt."""

    bl_idname = "interface.alias"
    bl_label = "Alias"

    @classmethod
    @override
    def poll(cls, context: bpy.types.Context) -> bool:
        return context.area is not None and context.area.type in BINDINGS

    @override
    def invoke(self, context: bpy.types.Context, event: bpy.types.Event) -> "set[OperatorReturnItems]":
        registered = frozenset(ops.dir())
        self.present = {alias: (row, function) for alias, row in BINDINGS[context.area.type].items() if (function := operator(row.idname)).idname() in registered}
        self.acting = {alias: row.enabled(context) and function.poll() for alias, (row, function) in self.present.items()}
        self.typed = ""
        context.workspace.status_text_set(self.status())
        context.window_manager.modal_handler_add(self)
        return {"RUNNING_MODAL"}

    @override
    def modal(self, context: bpy.types.Context, event: bpy.types.Event) -> "set[OperatorReturnItems]":
        match event.type, event.value:
            case "ESC" | "RIGHTMOUSE", "PRESS":
                self.cancel(context)
                return {"CANCELLED"}
            case "RET" | "NUMPAD_ENTER" | "SPACE", "PRESS":
                return self.run(context, Alias.__members__.get(self.typed or context.window_manager.interface_alias))
            case "BACK_SPACE", "PRESS":
                self.typed = self.typed[:-1]
            case _, "PRESS" if not event.is_repeat and event.unicode.isalnum() and any(alias.name.startswith(self.typed + event.unicode.upper()) for alias in self.present):
                self.typed += event.unicode.upper()
            case _, "PRESS":
                return {"RUNNING_MODAL"}
            case _:
                return {"PASS_THROUGH"}
        context.workspace.status_text_set(self.status())
        return {"RUNNING_MODAL"}

    @override
    def cancel(self, context: bpy.types.Context) -> None:
        context.workspace.status_text_set(None)

    def run(self, context: bpy.types.Context, alias: Alias | None) -> "set[OperatorReturnItems]":
        """Run and remember the alias when it acts in this state, and keep waiting otherwise."""
        match alias:
            case Alias() if self.acting.get(alias, False):
                row, function = self.present[alias]
                context.window_manager.interface_alias = alias.name
                self.cancel(context)
                function("INVOKE_DEFAULT", **row.properties)
                return {"FINISHED"}
            case _:
                return {"RUNNING_MODAL"}

    def status(self) -> Callable[[bpy.types.Header, bpy.types.Context], None]:
        """Status bar draw function listing each family by key and name, or the typed prefix and each matching alias grayed where it does not act."""

        def draw(header: bpy.types.Header, _context: bpy.types.Context) -> None:
            layout = header.layout
            if not self.typed:
                for family in families(self.present):
                    layout.label(text=family.name.title(), icon=f"EVENT_{family.value}")
                return
            layout.label(text=self.typed)
            for alias in [alias for alias in self.present if alias.name.startswith(self.typed)]:
                row = layout.row()
                row.enabled = self.acting[alias]
                row.label(text=f"{alias.name} {alias.value}")

        return draw


def bind(keyconfigs: bpy.types.KeyConfigurations) -> list[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem]]:
    """Add-on keymap items binding the leader to Alt and the grave accent in every aliased editor."""
    return [
        (keymap, keymap.keymap_items.new(INTERFACE_OT_alias.bl_idname, "ACCENT_GRAVE", "PRESS", alt=True))
        for keymap in (keyconfigs.addon.keymaps.new(name=KEYMAPS[space], space_type=space) for space in BINDINGS)
    ]


# --- [ALIASES] --------------------------------------------------------------------------

BINDINGS: Final[Mapping["SpaceTypeItems", Mapping[Alias, Binding]]] = MappingProxyType({
    "VIEW_3D": MappingProxyType({
        Alias.Q: binding("view3d.slvs_add_line2d", continuous_draw=False),
        Alias.Q1: binding("curve.extrude_move"),
        Alias.Q2: binding("view3d.slvs_add_coincident"),
        Alias.QQ: binding("view3d.slvs_add_line2d"),
        Alias.QW: binding("view3d.slvs_add_arc2d"),
        Alias.QE: binding("curve.primitive_bezier_curve_add"),
        Alias.QA: binding("view3d.slvs_add_point2d"),
        Alias.QS: binding("wm.tool_set_by_id", name="sketcher.slvs_add_point2d"),
        Alias.QF: binding("curve.subdivide"),
        Alias.QZ: binding("object.convert", target="POINTCLOUD", keep_original=True),
        Alias.W1: binding("view3d.slvs_add_rectangle"),
        Alias.WQ: binding("mesh.primitive_circle_add", fill_type="NGON"),
        Alias.WW: binding(INTERFACE_OT_primitive.bl_idname, kind="Cube"),
        Alias.WE: binding(INTERFACE_OT_primitive.bl_idname, enabled=lambda context: context.mode == "OBJECT", kind="Tube"),
        Alias.WR: binding(INTERFACE_OT_primitive.bl_idname, kind="Cone"),
        Alias.WA: binding("mesh.primitive_cone_add", vertices=4),
        Alias.E: binding("view3d.slvs_add_circle2d"),
        Alias.EV: binding("curve.primitive_bezier_circle_add", rotation=(pi / 2, 0.0, 0.0)),
        Alias.EQ: binding(INTERFACE_OT_primitive.bl_idname, kind="UVSphere"),
        Alias.EW: binding(INTERFACE_OT_primitive.bl_idname, kind="Cylinder"),
        Alias.ER: binding(INTERFACE_OT_primitive.bl_idname, kind="Torus"),
        Alias.R: binding("transform.rotate"),
        Alias.R1: binding("object.modifier_add", type="MIRROR"),
        Alias.R2: binding("transform.mirror"),
        Alias.RE: binding("transform.translate", snap=True, snap_elements={"FACE_PROJECT"}, snap_align=True),
        Alias.RL: binding("view3d.view_orbit", type="ORBITLEFT", angle=pi / 2),
        Alias.RLL: binding("view3d.view_orbit", type="ORBITLEFT", angle=pi),
        Alias.RR: binding("view3d.view_orbit", type="ORBITRIGHT", angle=pi / 2),
        Alias.RRR: binding("view3d.view_orbit", type="ORBITRIGHT", angle=pi),
        Alias.RU: binding("view3d.view_orbit", type="ORBITUP", angle=pi),
        Alias.RD: binding("view3d.view_orbit", type="ORBITDOWN", angle=pi),
        Alias.T: binding(INTERFACE_OT_text.bl_idname),
        Alias.TT: binding("object.convert", target="MESH"),
        Alias.T1: binding("transform.resize", constraint_axis=(True, False, False)),
        Alias.T2: binding("transform.resize", constraint_axis=(True, True, False)),
        Alias.T3: binding("transform.resize"),
        Alias.TQ: binding(INTERFACE_OT_deform.bl_idname, method="STRETCH"),
        Alias.TW: binding("transform.shear"),
        Alias.TE: binding(INTERFACE_OT_deform.bl_idname, method="TAPER"),
        Alias.TA: binding(INTERFACE_OT_deform.bl_idname, method="BEND"),
        Alias.TS: binding(INTERFACE_OT_deform.bl_idname, method="TWIST"),
        Alias.A: binding("view3d.slvs_offset"),
        Alias.A2: binding("object.modifier_add", type="SOLIDIFY"),
        Alias.A3: binding("mesh.inset"),
        Alias.AQ: binding("view3d.slvs_node_boolean"),
        Alias.AW: binding("object.boolean_auto_union"),
        Alias.AE: binding("object.boolean_auto_difference"),
        Alias.AR: binding("object.boolean_auto_intersect"),
        Alias.AT: binding("object.boolean_auto_slice"),
        Alias.AA: binding("object.modifier_add", type="ARRAY"),
        Alias.AF: binding("object.modifier_add", type="CURVE"),
        Alias.AZ: binding("view3d.slvs_node_array_linear"),
        Alias.AX: binding("mesh.spin", dupli=True),
        Alias.S: binding("mesh.primitive_plane_add"),
        Alias.S1: binding("mesh.extrude_edges_move"),
        Alias.S4: binding("mesh.edge_face_add"),
        Alias.S5: binding("mesh.fill_grid"),
        Alias.SV: binding("mesh.primitive_plane_add", rotation=(pi / 2, 0.0, 0.0)),
        Alias.SQ: binding("cpc.apply_profile_to_curve"),
        Alias.SE: binding("mesh.bridge_edge_loops"),
        Alias.SR: binding("view3d.slvs_node_revolve"),
        Alias.ST: binding("mesh.knife_project"),
        Alias.SA: binding("mesh.fill_holes"),
        Alias.SS: binding("object.modifier_add", type="SOLIDIFY"),
        Alias.D: binding("dimensions.measure"),
        Alias.DL: binding("wm.context_toggle", data_path="space_data.overlay.show_extra_edge_length"),
        Alias.DA: binding("dimensions.create_area"),
        Alias.DV: binding("wm.context_toggle", data_path="scene.dimensions_settings.show_selected_object_overlay"),
        Alias.DQ: binding("dimensions.create_dimension"),
        Alias.DW: binding("dimensions.create_angle"),
        Alias.DE: binding("view3d.slvs_add_diameter"),
        Alias.DR: binding("view3d.slvs_add_diameter", setting=True),
        Alias.DS: binding("measureit_arch.addannotationbutton"),
        Alias.F: binding("view3d.slvs_trim"),
        Alias.FF: binding("object.join"),
        Alias.F1: binding("view3d.slvs_bevel"),
        Alias.F2: binding("mesh.bevel", segments=1),
        Alias.FQ: binding("mesh.split"),
        Alias.FW: binding("mesh.subdivide"),
        Alias.FE: binding("mesh.separate", type="LOOSE"),
        Alias.FA: binding("mesh.bisect"),
        Alias.FS: binding("bim.add_section_plane"),
        Alias.FD: binding("object.carve_polyline"),
        Alias.G: binding("collection.create"),
        Alias.GU: binding("collection.objects_remove"),
        Alias.GH: binding("object.hide_view_set"),
        Alias.GJ: binding("object.hide_view_clear"),
        Alias.GI: binding("view3d.localview", enabled=lambda context: context.space_data.local_view is None),
        Alias.GO: binding("view3d.localview", enabled=lambda context: context.space_data.local_view is not None),
        Alias.GL: binding("wm.context_collection_boolean_set", data_path_iter="selected_objects", data_path_item="hide_select", type="ENABLE"),
        Alias.GP: binding("wm.context_collection_boolean_set", data_path_iter="visible_objects", data_path_item="hide_select", type="DISABLE"),
        Alias.GW: binding("dimensions.create_guide"),
        Alias.GE: binding("dimensions.clear_guides"),
        Alias.Z: binding("view3d.zoom_border"),
        Alias.ZZ: binding("view3d.view_center_pick"),
        Alias.ZE: binding("view3d.view_all"),
        Alias.ZS: binding("view3d.view_selected"),
        Alias.ZF: binding("view3d.view_axis", type="FRONT"),
        Alias.ZB: binding("view3d.view_axis", type="BACK"),
        Alias.ZT: binding("view3d.view_axis", type="TOP"),
        Alias.ZL: binding("view3d.view_axis", type="LEFT"),
        Alias.ZR: binding("view3d.view_axis", type="RIGHT"),
        Alias.ZC: binding("view3d.slvs_align_view"),
        Alias.ZP: binding("view3d.view_persportho", enabled=lambda context: context.region_data.view_perspective != "PERSP"),
        Alias.X: binding("mesh.extrude_region_shrink_fatten"),
        Alias.XQ: binding("view3d.slvs_node_extrude"),
        Alias.XW: binding("mesh.extrude_region_move"),
        Alias.XE: binding("mesh.separate", type="SELECTED"),
        Alias.XR: binding("transform.resize", value=(1.0, 1.0, 0.0), center_override=(0.0, 0.0, 0.0)),
        Alias.C: binding("object.duplicate_move"),
        Alias.CC: binding("transform.create_orientation", enabled=lambda context: context.mode == "EDIT_MESH", use=True),
        Alias.CT: binding("view3d.slvs_set_active_sketch", sketch_name=""),
        Alias.CS: binding("wm.tool_set_by_id", name="sketcher.slvs_add_sketch"),
        Alias.CO: binding("transform.create_orientation", enabled=lambda context: context.mode == "OBJECT", use=True),
        Alias.CW: binding("bim.create_clipping_plane"),
        Alias.CE: binding("view3d.clip_border", enabled=lambda context: not context.region_data.use_clip_planes),
        Alias.CD: binding("view3d.clip_border", enabled=lambda context: context.region_data.use_clip_planes),
        Alias.V: binding("transform.translate"),
        Alias.VA: binding("object.align"),
        Alias.VK: binding("view3d.snap_selected_to_cursor"),
        Alias.VV: binding("view3d.select_circle"),
        Alias.VD: binding("wm.tool_set_by_id", name="dimensions.annotation_selection"),
        Alias.VB: binding(INTERFACE_OT_select_instances.bl_idname),
        Alias.VO: binding("object.select_all", action="SELECT"),
        Alias.VI: binding("object.select_all", action="INVERT"),
        Alias.VE: binding("object.select_grouped", type="COLLECTION"),
        Alias.B: binding("object.library_instance"),
        Alias.BN: binding("wm.batch_rename"),
        Alias.BQ: binding("object.make_single_user_library_instance"),
        Alias.BW: binding("object.add_to_library_instance"),
        Alias.BE: binding("wm.call_panel", name="VIEW3D_PT_library_instance_menu"),
        Alias.BA: binding("object.edit_library_instance_skip_testing"),
        Alias.BS: binding("object.make_links_data", type="DUPLICOLLECTION"),
        Alias.BD: binding("object.library_instance_ungroup"),
        Alias.BF: binding("object.scale_clear"),
        Alias.MF: binding("mesh.dissolve_limited"),
        Alias.MS: binding("mesh.dissolve_faces"),
        Alias.MC: binding("curve.make_segment"),
        Alias.ME: binding("mesh.dissolve_edges"),
        Alias.ML: binding("object.make_links_data", type="GROUPS"),
        Alias.MP: binding("object.make_links_data", type="MATERIAL"),
        Alias.MM: binding("object.join_uvs"),
        Alias.LG: binding("measureit_arch.addviewbutton"),
        Alias.IM: binding("wm.call_menu", name="TOPBAR_MT_file_import"),
        Alias.IN: binding("object.collection_instance_add"),
        Alias.PQ: binding("outliner.orphans_purge"),
        Alias.PW: binding("mesh.remove_doubles"),
    }),
    "NODE_EDITOR": MappingProxyType({
        Alias.F: binding("node.links_cut"),
        Alias.FQ: binding("node.links_detach"),
        Alias.G: binding("node.join"),
        Alias.GU: binding("node.detach"),
        Alias.GH: binding("node.hide_toggle"),
        Alias.Z: binding("view2d.zoom_border"),
        Alias.ZE: binding("node.view_all"),
        Alias.ZS: binding("node.view_selected"),
        Alias.C: binding("node.duplicate_move"),
        Alias.V: binding("node.translate_attach"),
        Alias.VA: binding("node.nw_align_nodes"),
        Alias.VO: binding("node.select_all", action="SELECT"),
        Alias.VI: binding("node.select_all", action="INVERT"),
        Alias.VE: binding("node.select_grouped"),
        Alias.B: binding("node.group_make"),
        Alias.BA: binding("node.group_edit"),
        Alias.BS: binding("wm.call_menu", name="NODE_MT_group_swap"),
        Alias.BD: binding("node.group_ungroup"),
        Alias.BW: binding("node.group_insert"),
        Alias.IN: binding("wm.call_menu", name="NODE_MT_group_add"),
        Alias.PQ: binding("outliner.orphans_purge"),
    }),
})

# --- [COMPOSITION] ----------------------------------------------------------------------

LAST: Final = StringProperty(name="Last Alias", description="Alias the leader ran last, which Enter on an empty prompt repeats")
CLASSES: Final = (INTERFACE_OT_deform, INTERFACE_OT_text, INTERFACE_OT_select_instances, INTERFACE_OT_primitive, INTERFACE_OT_alias)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CLASSES", "LAST", "bind"]
