"""Scene content: cameras, collections, materials, objects, worlds, and lights."""

from rasm_bpy.scene._collections import active_editable, cloned, created, deleted, duplicated, grouped, instanced, ungrouped
from rasm_bpy.scene._lights.emitters import mesh_light_add, MeshLight
from rasm_bpy.scene._lights.groups import Assignment, Grouping, Kept, light_group_select, light_groups_assign, light_split, light_split_toggle, light_unsplit, PerEmitter, Shared, Split, Ungrouped
from rasm_bpy.scene._lights.linking import light_linking_unlink, Unlinking
from rasm_bpy.scene._lights.lists import Bulk, Emissives, emissives_select, light_bulk, light_solo, light_unsolo, Solo
from rasm_bpy.scene._lights.rigs import (
    GelPick,
    ReferenceDraw,
    ReferencePick,
    rig_arrange,
    rig_pick,
    rig_place,
    rig_reference_apply,
    rig_reference_draw,
    rig_reference_gel,
    rig_reference_pick,
    rig_remove,
    RigPick,
    RigSetup,
)
from rasm_bpy.scene._lights.sun import sun_place, sun_remove, SunSetup
from rasm_bpy.scene._materials import Clay, Emissive, Materials, referenced, stored, texture_setup, Textured, Utility
from rasm_bpy.scene._objects import LevelEdit, subdivision_levels
from rasm_bpy.scene._worlds import Backdrop, Environment, environment_place, environment_restore
from rasm_bpy.scene.cameras import camera_frame, camera_type

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Assignment",
    "Backdrop",
    "Bulk",
    "Clay",
    "Emissive",
    "Emissives",
    "Environment",
    "GelPick",
    "Grouping",
    "Kept",
    "LevelEdit",
    "Materials",
    "MeshLight",
    "PerEmitter",
    "ReferenceDraw",
    "ReferencePick",
    "RigPick",
    "RigSetup",
    "Shared",
    "Solo",
    "Split",
    "SunSetup",
    "Textured",
    "Ungrouped",
    "Unlinking",
    "Utility",
    "active_editable",
    "camera_frame",
    "camera_type",
    "cloned",
    "created",
    "deleted",
    "duplicated",
    "emissives_select",
    "environment_place",
    "environment_restore",
    "grouped",
    "instanced",
    "light_bulk",
    "light_group_select",
    "light_groups_assign",
    "light_linking_unlink",
    "light_solo",
    "light_split",
    "light_split_toggle",
    "light_unsolo",
    "light_unsplit",
    "mesh_light_add",
    "referenced",
    "rig_arrange",
    "rig_pick",
    "rig_place",
    "rig_reference_apply",
    "rig_reference_draw",
    "rig_reference_gel",
    "rig_reference_pick",
    "rig_remove",
    "stored",
    "subdivision_levels",
    "sun_place",
    "sun_remove",
    "texture_setup",
    "ungrouped",
]
