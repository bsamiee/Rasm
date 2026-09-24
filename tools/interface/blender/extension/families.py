# ty: ignore[invalid-argument-type, invalid-type-form, not-iterable, unresolved-attribute]
# mypy: disable-error-code="arg-type, union-attr, valid-type"
# ruff: file-ignore[invalid-class-name, mutable-class-default, relative-imports]
"""Rhino's alias families as pies: the aliases per editor and family key, the operators the aliases add, one pie per family key, and the leader key that opens them."""

from collections.abc import Callable, Mapping
from itertools import chain, islice, repeat
from math import pi
from types import MappingProxyType
from typing import override, TYPE_CHECKING

import attrs
import bpy
from bpy.props import EnumProperty

from .navigation import KEYMAPS

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import OperatorReturnItems

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen(init=False)
class Alias:
    """One alias as a pie row: its name, its label, the operator it runs with the properties set, and the state in which it acts."""

    name: str
    label: str
    operator: str | None
    enabled: Callable[[bpy.types.Context], bool]
    properties: Mapping[str, object]

    def __init__(self, name: str, label: str, operator: str | None = None, /, *, enabled: Callable[[bpy.types.Context], bool] = lambda _context: True, **properties: object) -> None:
        """Alias whose keywords past `enabled` are the operator's properties."""
        self.__attrs_init__(name, label, operator, enabled, MappingProxyType(properties))


@attrs.frozen
class Family:
    """The aliases of one family key: the column its single key and digits open, and the series its second keys open."""

    column: tuple[Alias, ...]
    series: tuple[Alias, ...]


# --- [TABLES] ---------------------------------------------------------------------------

FAMILIES: MappingProxyType[str, MappingProxyType[str, Family]] = MappingProxyType({
    "VIEW_3D": MappingProxyType({
        "Q": Family(
            (
                Alias("Q", "Line", "view3d.slvs_add_line2d"),
                Alias("Q1", "Extend Curve", "curve.extrude_move"),
                Alias("Q2", "Connect", "view3d.slvs_add_coincident"),
                Alias("Q5", "Show Curve Ends"),
                Alias("QV", "Line Vertical"),
            ),
            (
                Alias("QQ", "Polyline", "view3d.slvs_add_line2d"),
                Alias("QW", "Arc", "view3d.slvs_add_arc2d"),
                Alias("QE", "Interpolated Curve", "curve.primitive_bezier_curve_add"),
                Alias("QR", "Tween Curve"),
                Alias("QA", "Point", "view3d.slvs_add_point2d"),
                Alias("QS", "Multiple Points"),
                Alias("QD", "Divide by Length"),
                Alias("QF", "Divide by Segments", "curve.subdivide"),
                Alias("QZ", "Point Cloud"),
                Alias("QZZ", "Reduce Point Cloud"),
            ),
        ),
        "W": Family(
            (
                Alias("W", "Rectangle 3 Points"),
                Alias("W1", "Rectangle Corner to Corner", "view3d.slvs_add_rectangle"),
                Alias("W2", "Rectangle Center"),
                Alias("W3", "Rounded Rectangle"),
                Alias("WV", "Rectangle Vertical"),
            ),
            (
                Alias("WQ", "Polygon", "mesh.primitive_circle_add", fill_type="NGON"),
                Alias("WW", "Box", "mesh.primitive_cube_add"),
                Alias("WE", "Tube"),
                Alias("WR", "Cone", "mesh.primitive_cone_add"),
                Alias("WA", "Pyramid", "mesh.primitive_cone_add", vertices=4),
            ),
        ),
        "E": Family(
            (
                Alias("E", "Circle", "view3d.slvs_add_circle2d"),
                Alias("E1", "Circle 3 Points"),
                Alias("E2", "Circle Along 2 Curves"),
                Alias("E3", "Circle Tangent to 3 Curves"),
                Alias("E4", "Circle Around Curve"),
                Alias("EV", "Circle Vertical"),
            ),
            (
                Alias("EQ", "Sphere", "mesh.primitive_uv_sphere_add"),
                Alias("EW", "Cylinder", "mesh.primitive_cylinder_add"),
                Alias("EE", "Ellipsoid"),
                Alias("ER", "Torus", "mesh.primitive_torus_add"),
                Alias("EA", "Paraboloid"),
            ),
        ),
        "R": Family(
            (Alias("R", "Rotate", "transform.rotate"), Alias("R1", "Symmetry", "mesh.symmetrize"), Alias("R2", "Mirror", "transform.mirror")),
            (
                Alias("RQ", "Orient"),
                Alias("RW", "Orient on Curve"),
                Alias("RE", "Orient on Surface"),
                Alias("RL", "Rotate View Left", "interface.view_orbit", type="ORBITLEFT", angle=pi / 2),
                Alias("RLL", "Rotate View Left Sideways", "interface.view_orbit", type="ORBITLEFT", angle=pi),
                Alias("RR", "Rotate View Right", "interface.view_orbit", type="ORBITRIGHT", angle=pi / 2),
                Alias("RRR", "Rotate View Right Sideways", "interface.view_orbit", type="ORBITRIGHT", angle=pi),
                Alias("RD", "Rotate View Down", "interface.view_orbit", type="ORBITDOWN", angle=pi),
                Alias("RU", "Rotate View Up", "interface.view_orbit", type="ORBITUP", angle=pi),
            ),
        ),
        "T": Family(
            (
                Alias("T", "Text", "object.text_add"),
                Alias("T1", "Scale 1-D", "transform.resize", constraint_axis=(True, False, False)),
                Alias("T2", "Scale 2-D", "transform.resize", constraint_axis=(True, True, False)),
                Alias("T3", "Scale 3-D", "transform.resize"),
            ),
            (
                Alias("TT", "Text Object", "object.convert", target="MESH"),
                Alias("TQ", "Stretch", "interface.deform", method="STRETCH"),
                Alias("TW", "Shear", "transform.shear"),
                Alias("TE", "Taper", "interface.deform", method="TAPER"),
                Alias("TA", "Bend", "transform.bend"),
                Alias("TS", "Twist", "interface.deform", method="TWIST"),
            ),
        ),
        "A": Family(
            (Alias("A", "Offset", "view3d.slvs_offset"), Alias("A1", "Offset Multiple"), Alias("A2", "Offset Surface", "object.modifier_add", type="SOLIDIFY"), Alias("A3", "Inset", "mesh.inset")),
            (
                Alias("AQ", "Curve Boolean"),
                Alias("AW", "Boolean Union", "object.boolean_auto_union"),
                Alias("AE", "Boolean Difference", "object.boolean_auto_difference"),
                Alias("AR", "Boolean Intersection", "object.boolean_auto_intersect"),
                Alias("AT", "Boolean Split", "object.boolean_auto_slice"),
                Alias("AA", "Array", "object.modifier_add", type="ARRAY"),
                Alias("AS", "Array Along Curve"),
                Alias("AD", "Array Along Surface"),
                Alias("AF", "Flow", "object.modifier_add", type="CURVE"),
                Alias("AG", "Array Along Curve on Surface"),
                Alias("AZ", "Array Linear", "view3d.slvs_node_array_linear"),
                Alias("AX", "Array Polar"),
            ),
        ),
        "S": Family(
            (
                Alias("S", "Plane", "mesh.primitive_plane_add"),
                Alias("S1", "Extend Surface"),
                Alias("S2", "Connect Surface"),
                Alias("S3", "Plane 3 Points"),
                Alias("S4", "Surface from Planar Curves", "mesh.edge_face_add"),
                Alias("S5", "Surface from Edge Curves", "mesh.fill_grid"),
                Alias("S6", "Duplicate Edge"),
                Alias("SV", "Plane Vertical"),
            ),
            (
                Alias("SQ", "Sweep 1"),
                Alias("SW", "Sweep 2"),
                Alias("SE", "Loft", "mesh.bridge_edge_loops"),
                Alias("SR", "Revolve", "mesh.spin"),
                Alias("ST", "Project", "mesh.knife_project"),
                Alias("SA", "Cap", "mesh.fill_holes"),
                Alias("SS", "Shell", "object.modifier_add", type="SOLIDIFY"),
                Alias("SD", "Slab"),
                Alias("SF", "Pipe"),
                Alias("SZ", "Boss"),
                Alias("SX", "Rib"),
            ),
        ),
        "D": Family(
            (Alias("D", "Distance", "dimensions.measure"), Alias("D1", "Dimension Bounding Box"), Alias("D3", "Circle Center Points"), Alias("D4", "Count Objects")),
            (
                Alias("DL", "Length", "dimensions.measure"),
                Alias("DA", "Area", "dimensions.create_area"),
                Alias("DV", "Volume"),
                Alias("DQ", "Dimension Aligned", "dimensions.create_dimension"),
                Alias("DW", "Dimension Angle", "dimensions.create_angle"),
                Alias("DE", "Dimension Diameter", "view3d.slvs_add_diameter"),
                Alias("DR", "Dimension Radius"),
                Alias("DZ", "Annotation Dot"),
                Alias("DS", "Leader"),
                Alias("DD", "Align Dimensions"),
            ),
        ),
        "F": Family(
            (
                Alias("F", "Trim", "view3d.slvs_trim"),
                Alias("F1", "Fillet", "mesh.bevel", segments=6),
                Alias("F2", "Chamfer", "mesh.bevel", segments=1),
                Alias("F3", "Blend Curve"),
                Alias("F4", "Blend Surface"),
            ),
            (
                Alias("FF", "Join", "object.join"),
                Alias("FQ", "Split", "mesh.separate", type="SELECTED"),
                Alias("FW", "Divide", "mesh.subdivide"),
                Alias("FE", "Explode", "mesh.separate", type="LOOSE"),
                Alias("FR", "Rebuild"),
                Alias("FA", "Infinite Plane"),
                Alias("FS", "Cutting Plane", "sbp.create_plane"),
                Alias("FD", "Wire Cut", "object.carve_polyline"),
                Alias("FB", "Trim Box", "object.carve_box"),
            ),
        ),
        "G": Family(
            (Alias("G", "Group", "object.link_to_collection"),),
            (
                Alias("GU", "Ungroup", "collection.objects_remove"),
                Alias("GH", "Hide", "object.hide_view_set"),
                Alias("GJ", "Show", "object.hide_view_clear"),
                Alias("GI", "Isolate", "view3d.localview", enabled=lambda context: context.space_data.local_view is None),
                Alias("GO", "Unisolate", "view3d.localview", enabled=lambda context: context.space_data.local_view is not None),
                Alias("GL", "Lock", "interface.lock"),
                Alias("GP", "Unlock", "interface.unlock"),
                Alias("GW", "Add Guides", "dimensions.create_guide"),
                Alias("GE", "Remove Guides", "dimensions.clear_guides"),
            ),
        ),
        "Z": Family(
            (Alias("Z", "Zoom Window", "view3d.zoom_border"),),
            (
                Alias("ZZ", "Zoom Target", "view3d.view_center_pick"),
                Alias("ZE", "Zoom Extents", "view3d.view_all"),
                Alias("ZS", "Zoom Selected", "view3d.view_selected"),
                Alias("ZF", "Front", "interface.view_axis", type="FRONT"),
                Alias("ZB", "Back", "interface.view_axis", type="BACK"),
                Alias("ZT", "Top", "interface.view_axis", type="TOP"),
                Alias("ZL", "Left", "interface.view_axis", type="LEFT"),
                Alias("ZR", "Right", "interface.view_axis", type="RIGHT"),
                Alias("ZC", "Sketch Plane", "view3d.slvs_align_view"),
                Alias("ZP", "Perspective", "interface.view_persportho", enabled=lambda context: context.region_data.view_perspective != "PERSP"),
            ),
        ),
        "X": Family(
            (Alias("X", "Push Pull", "mesh.extrude_region_shrink_fatten"),),
            (
                Alias("XQ", "Extrude Curve", "view3d.slvs_node_extrude"),
                Alias("XW", "Extrude Surface", "mesh.extrude_region_move"),
                Alias("XE", "Extract Surface", "mesh.separate", type="SELECTED"),
                Alias("XR", "Project to Plane", "transform.resize", value=(1.0, 1.0, 0.0)),
            ),
        ),
        "C": Family(
            (Alias("C", "Copy", "object.duplicate_move"),),
            (
                Alias("CC", "Sketch 3 Points", "view3d.slvs_add_sketch"),
                Alias("CT", "World Top", "view3d.slvs_set_active_sketch", sketch_name=""),
                Alias("CS", "Plane to Surface"),
                Alias("CO", "Plane to Object", "transform.create_orientation", use=True),
                Alias("CP", "Plane Perpendicular to Curve"),
                Alias("CD", "Clipping Off", "view3d.clip_border", enabled=lambda context: context.region_data.use_clip_planes),
                Alias("CDD", "Clipping On", "view3d.clip_border", enabled=lambda context: not context.region_data.use_clip_planes),
            ),
        ),
        "V": Family(
            (Alias("V", "Move", "transform.translate"),),
            (
                Alias("VA", "Align", "object.align"),
                Alias("VS", "Distribute"),
                Alias("VK", "Set Point", "view3d.snap_selected_to_cursor"),
                Alias("VD", "Select Dimensions"),
                Alias("VB", "Select Instances", "interface.select_instances"),
                Alias("VL", "Select Last"),
                Alias("VP", "Select Previous"),
                Alias("VO", "Select All", "object.select_all", action="SELECT"),
                Alias("VI", "Invert Selection", "object.select_all", action="INVERT"),
                Alias("VE", "Select Collection", "object.select_grouped", type="COLLECTION"),
            ),
        ),
        "B": Family(
            (Alias("B", "Collection", "collection.create"),),
            (
                Alias("BQ", "Unique Collection"),
                Alias("BW", "Add to Collection", "collection.objects_add_active"),
                Alias("BE", "Collection Manager"),
                Alias("BA", "Edit Collection"),
                Alias("BS", "Replace Collection"),
                Alias("BD", "Make Instances Real", "object.duplicates_make_real"),
                Alias("BF", "Reset Scale", "object.scale_clear"),
            ),
        ),
        "M": Family(
            (),
            (
                Alias("MF", "Merge Faces", "mesh.dissolve_limited"),
                Alias("MS", "Merge Surfaces", "mesh.dissolve_faces"),
                Alias("MC", "Merge Curves"),
                Alias("ME", "Merge Edges", "mesh.dissolve_edges"),
                Alias("ML", "Match Collection", "object.move_to_collection"),
                Alias("MP", "Match Material", "object.make_links_data", type="MATERIAL"),
                Alias("MM", "Match Mapping", "object.join_uvs"),
            ),
        ),
        "I": Family(
            (),
            (
                Alias("IM", "Import", "wm.call_menu", name="TOPBAR_MT_file_import"),
                Alias("IN", "Insert", "object.collection_instance_add"),
                Alias("IN", "Append", "wm.append", instance_collections=True),
            ),
        ),
        "P": Family((), (Alias("PQ", "Purge", "outliner.orphans_purge"), Alias("PW", "Merge by Distance", "mesh.remove_doubles"))),
    }),
    "NODE_EDITOR": MappingProxyType({
        "G": Family((Alias("G", "Group", "node.join"),), (Alias("GU", "Ungroup", "node.detach"), Alias("GH", "Hide", "node.hide_toggle"))),
        "Z": Family((), (Alias("ZE", "Zoom Extents", "node.view_all"), Alias("ZS", "Zoom Selected", "node.view_selected"))),
        "V": Family((), (Alias("VO", "Select All", "node.select_all", action="SELECT"), Alias("VI", "Invert Selection", "node.select_all", action="INVERT"))),
        "C": Family((Alias("C", "Copy", "node.duplicate_move"),), ()),
        "B": Family((Alias("B", "Block", "node.group_make"),), (Alias("BA", "Block Edit", "node.group_edit"), Alias("BD", "Explode Block", "node.group_ungroup"))),
    }),
})

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [OPERATORS]
class FamilyOperator(bpy.types.Operator):
    """Operator base of the operators the aliases add, each with a redo panel and an undo step."""

    bl_options = {"REGISTER", "UNDO"}


class INTERFACE_OT_deform(FamilyOperator):
    """Simple Deform modifier on the active object with the deform method set."""

    bl_idname = "interface.deform"
    bl_label = "Simple Deform"

    method: EnumProperty(name="Method", items=[(item.identifier, item.name, item.description) for item in bpy.types.SimpleDeformModifier.bl_rna.properties["deform_method"].enum_items])

    @classmethod
    @override
    def poll(cls, context: bpy.types.Context | None) -> bool:
        return context.object is not None

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        bpy.ops.object.modifier_add(type="SIMPLE_DEFORM")
        context.object.modifiers[-1].deform_method = self.method
        return {"FINISHED"}


class INTERFACE_OT_lock(FamilyOperator):
    """Lock the selected objects against selection, Rhino's Lock."""

    bl_idname = "interface.lock"
    bl_label = "Lock"

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        for target in context.selected_objects:
            target.hide_select = True
        return {"FINISHED"}


class INTERFACE_OT_unlock(FamilyOperator):
    """Unlock every object of the view layer, Rhino's Unlock."""

    bl_idname = "interface.unlock"
    bl_label = "Unlock"

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        for target in context.view_layer.objects:
            target.hide_select = False
        return {"FINISHED"}


class INTERFACE_OT_select_instances(FamilyOperator):
    """Add every selectable collection instance of the view layer to the selection, Rhino's SelBlockInstance."""

    bl_idname = "interface.select_instances"
    bl_label = "Select Instances"

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        for target in [target for target in context.selectable_objects if target.instance_type == "COLLECTION"]:
            target.select_set(state=True)
        return {"FINISHED"}


# --- [PIES]
def registered(operator: str) -> bool:
    """Whether the operator is registered, an add-on operator counting only while its add-on is enabled."""
    category, _, name = operator.partition(".")
    return name in dir(getattr(bpy.ops, category))


def draw_rows(layout: bpy.types.UILayout, aliases: tuple[Alias, ...], context: bpy.types.Context) -> None:
    """One slice: a column of the aliases Blender can run, each grayed outside the state in which it acts, a separator that keeps the slice's place when none can."""
    if not (present := [(alias, operator) for alias in aliases if (operator := alias.operator) is not None and registered(operator)]):
        layout.separator()
        return
    column = layout.column()
    for alias, operator in present:
        row = column.row()
        row.enabled = alias.enabled(context)
        item = row.operator(operator, text=f"{alias.name} {alias.label}")
        for key, value in alias.properties.items():
            setattr(item, key, value)


def pie(key: str) -> type[bpy.types.Menu]:
    """Pie menu of one family key drawn from the table of the editor it opens in: the column at the top, then clockwise one slice per second key in keyboard order, the last slice holding the rest."""

    def draw(self: bpy.types.Menu, context: bpy.types.Context) -> None:
        family = FAMILIES[context.area.type][key]
        slots = [tuple(alias for alias in family.series if alias.name[1] == second) for second in sorted({alias.name[1] for alias in family.series}, key="QWERTYUIOPASDFGHJKLZXCVBNM".index)]
        slices = dict(zip(("N", "NE", "E", "SE", "S", "SW", "W", "NW"), (family.column, *islice(chain(slots, repeat(())), 6), tuple(chain.from_iterable(slots[6:]))), strict=True))
        layout = self.layout.menu_pie()
        for direction in ("W", "E", "S", "N", "NW", "NE", "SW", "SE"):
            draw_rows(layout, slices[direction], context)

    return type(f"INTERFACE_MT_{key.lower()}", (bpy.types.Menu,), {"bl_label": key, "draw": draw})


# --- [LEADER]
class INTERFACE_OT_family(bpy.types.Operator):
    """Leader: the status bar lists the family keys, the next key names a family and opens its pie, any other key cancels."""

    bl_idname = "interface.family"
    bl_label = "Family"

    @override
    def invoke(self, context: bpy.types.Context | None, event: bpy.types.Event | None) -> "set[OperatorReturnItems]":
        context.workspace.status_text_set(f"{self.bl_label}: {' '.join(FAMILIES[context.area.type])}")
        context.window_manager.modal_handler_add(self)
        return {"RUNNING_MODAL"}

    @override
    def modal(self, context: bpy.types.Context | None, event: bpy.types.Event | None) -> "set[OperatorReturnItems]":
        match event.value, event.type:
            case "PRESS", key:
                context.workspace.status_text_set(None)
                if key not in FAMILIES[context.area.type]:
                    return {"CANCELLED"}
                bpy.ops.wm.call_menu_pie("INVOKE_DEFAULT", name=PIES[key].__name__)
                return {"FINISHED"}
            case _:
                return {"RUNNING_MODAL"}


def bind(keyconfigs: bpy.types.KeyConfigurations) -> list[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem]]:
    """Add-on keyconfig items: the leader on Alt with the grave accent in every editor the table covers."""
    return [
        (keymap, keymap.keymap_items.new(INTERFACE_OT_family.bl_idname, "ACCENT_GRAVE", "PRESS", alt=True))
        for keymap in (keyconfigs.addon.keymaps.new(name=KEYMAPS[space], space_type=space, region_type="WINDOW") for space in FAMILIES)
    ]


# --- [COMPOSITION] ----------------------------------------------------------------------

PIES = MappingProxyType({key: pie(key) for key in dict.fromkeys(key for editor in FAMILIES.values() for key in editor)})
CLASSES = (INTERFACE_OT_deform, INTERFACE_OT_lock, INTERFACE_OT_unlock, INTERFACE_OT_select_instances, INTERFACE_OT_family, *PIES.values())

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["bind", "CLASSES"]
