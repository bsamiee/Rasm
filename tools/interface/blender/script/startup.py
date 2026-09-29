# ty: ignore[invalid-argument-type, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, union-attr"
"""Blender's startup scenes with their data-blocks, render, color, snapping, sky, sun, and camera settings, and the stock objects every task deletes."""

from collections.abc import Callable
from functools import partial
from math import radians
from pathlib import Path, PurePath
from typing import Final

import bpy
from mathutils import Matrix, Vector

from interface.blender.rows import Launch
from interface.blender.script.rna import held, Paint
from interface.render import (
    CAUSTICS,
    DIFFUSE_BOUNCES,
    DPI,
    ELEVATION,
    EXPOSURE,
    FILTER_GLOSSY,
    FRAME_SIZE,
    GLOSSY_BOUNCES,
    INDIRECT_CLAMP,
    LATITUDE,
    LENS,
    LONGITUDE,
    MAX_BOUNCES,
    NOISE_THRESHOLD,
    NORTH,
    SAMPLES,
    SUN_IRRADIANCE,
    TRANSMISSION_BOUNCES,
    TRANSPARENT_BOUNCES,
    VOLUME_BOUNCES,
)
from interface.report import Row, subscript
from interface.roles import Annotation, Ink, Surface
from interface.units import ANGLE_STEP, Length

# --- [CONSTANTS] ------------------------------------------------------------------------

ANALYSIS: Final = "Analysis"

# --- [LENGTHS] --------------------------------------------------------------------------

EYE_HEIGHT: Final = 66 * Length.INCHES
PLAN_DISTANCE: Final = 100 * Length.FEET

# --- [FILES] ----------------------------------------------------------------------------

HEADLAMP: Final = Path(__file__).with_name("Headlamp.sl")

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [DATA]
def sun(scene: bpy.types.Scene) -> bpy.types.Object:
    """Scene's one light object."""
    return next(target for target in scene.objects if target.type == "LIGHT")


def fixed(scene: bpy.types.Scene) -> frozenset[bpy.types.Object]:
    """Objects every task keeps, the scene's light and camera objects."""
    return frozenset(target for target in scene.objects if target.type in {"LIGHT", "CAMERA"})


def world_node(scene: bpy.types.Scene, kind: str) -> bpy.types.Node:
    """World shader node of the node type."""
    return next(node for node in scene.world.node_tree.nodes if node.type == kind)


def prepared_scene(scene: bpy.types.Scene) -> tuple[Row, ...]:
    """Rows of the startup data the RNA paths cannot state: the analysis scene, its world, and the annotation collection, then the site, the sky link, the root objects, and the root collections, whose write unlinks the factory collection the objects sat in."""
    tree, background, label = scene.world.node_tree, world_node(scene, "BACKGROUND"), subscript("scenes", scene.name)
    root, anchored, color = scene.collection, fixed(scene), background.inputs["Color"]

    def annotation() -> bpy.types.Collection:
        return bpy.data.collections[Annotation.NAME]

    def id_property(owner: Callable[[], bpy.types.ID], key: str) -> object:
        return owner().get(key)

    def id_property_written(owner: Callable[[], bpy.types.ID], key: str, value: object) -> None:
        owner()[key] = value

    def linked(names: tuple[str, ...]) -> None:
        present = {child.name for child in root.children}
        for child in [child for child in root.children if child.name not in names]:
            root.children.unlink(child)
        for name in (name for name in names if name not in present):
            root.children.link(bpy.data.collections[name])

    def rooted(names: tuple[str, ...]) -> None:
        for target in (scene.objects[name] for name in names):
            for owner in [owner for owner in target.users_collection if owner != root]:
                owner.objects.unlink(target)
            if root not in target.users_collection:
                root.objects.link(target)

    blocks = (("scenes", bpy.data.scenes, ANALYSIS), ("worlds", bpy.data.worlds, ANALYSIS), ("collections", bpy.data.collections, Annotation.NAME))
    properties = (
        (label, lambda: scene, "lat", LATITUDE),
        (label, lambda: scene, "lon", LONGITUDE),
        (subscript("collections", Annotation.NAME), annotation, "rhid", str(Annotation.ID)),
        (subscript("collections", Annotation.NAME), annotation, "dimensions_collection_role", "DIMENSIONS"),
    )
    return (
        *(Row(label=subscript(kind, name), read=partial(held, owner, name), write=owner.new, target=name) for kind, owner, name in blocks),
        *(Row(label=subscript(path, key), read=partial(id_property, owner, key), write=partial(id_property_written, owner, key), target=value) for path, owner, key, value in properties),
        Row(label=f"{label}.world.node_tree.nodes", read=lambda: next((node.bl_idname for node in tree.nodes if node.type == "TEX_SKY"), None), write=tree.nodes.new, target="ShaderNodeTexSky"),
        Row(
            label=f'{subscript(f"{label}.world.node_tree.nodes", background.name)}.inputs["Color"].links[0].from_node.type',
            read=lambda: next((link.from_node.type for link in tree.links if link.to_socket == color), None),
            write=lambda kind: tree.links.new(world_node(scene, kind).outputs["Color"], color),
            target="TEX_SKY",
        ),
        Row(
            label=f"{label}.collection.objects",
            read=lambda: tuple(sorted(target.name for target in anchored if target.users_collection == (root,))),
            write=rooted,
            target=tuple(sorted(target.name for target in anchored)),
        ),
        Row(label=f"{label}.collection.children", read=lambda: tuple(child.name for child in root.children), write=linked, target=(Annotation.NAME,)),
    )


def cleared_objects(scene: bpy.types.Scene) -> tuple[Row, ...]:
    """Rows deleting every object but the sun and camera from the startup and analysis scenes, orphaned data included."""
    anchored = fixed(scene)

    def stock(owner: bpy.types.Scene) -> tuple[str, ...]:
        return tuple(sorted(target.name for target in owner.objects if target not in anchored))

    def objects(owner: bpy.types.Scene, kept: tuple[str, ...]) -> None:
        bpy.data.batch_remove([target for target in owner.objects if target not in anchored and target.name not in kept])
        bpy.data.orphans_purge(do_recursive=True)

    return tuple(Row(label=f"{subscript('scenes', owner.name)}.objects", read=partial(stock, owner), write=partial(objects, owner), target=()) for owner in (scene, bpy.data.scenes[ANALYSIS]))


# --- [SETTINGS]
def declared_scenes(scene: bpy.types.Scene, launch: Launch) -> tuple[tuple[str, bpy.types.Scene, dict[str, object]], ...]:
    """Label, scene, and declared values by path of each startup scene, each dependent value after the value it depends on, every render output under the renders folder beside the saved file."""
    output = f"//{PurePath(launch.renders).name}/{{blend_name}}/"
    north = Matrix.Rotation(radians(NORTH), 3, "Z") @ Vector((0.0, 1.0, 0.0))
    texture, background = (subscript("world.node_tree.nodes", world_node(scene, kind).name) for kind in ("TEX_SKY", "BACKGROUND"))
    light, collection = subscript("objects", sun(scene).name), subscript("collection.children", Annotation.NAME)
    (width, height), fps, clamp = FRAME_SIZE, 24, INDIRECT_CLAMP / 2**EXPOSURE
    startup = {
        f"{collection}.color_tag": Annotation.TAG.name,
        f"{collection}.lineart_usage": "INCLUDE",
        **dict.fromkeys((f"{collection}.{flag}" for flag in ("hide_viewport", "hide_select", "hide_render")), False),
        "view_layers[0].active_layer_collection": scene.view_layers[0].layer_collection,
        "render.fps": fps,
        "render.fps_base": 1.0,
        "render.use_persistent_data": True,
        "render.filepath": output,
        "render.engine": "CYCLES",
        "render.use_lock_interface": True,
        "render.anisotropic_filter": "FILTER_16",
        "render.resolution_x": width,
        "render.resolution_y": height,
        "render.resolution_percentage": 100,
        "render.ppm_factor": DPI,
        "render.ppm_base": Length.INCHES,
        "render.image_settings.color_depth": "16",
        "render.ffmpeg.format": "MPEG4",
        "render.ffmpeg.codec": "H264",
        "render.ffmpeg.constant_rate_factor": "HIGH",
        "render.ffmpeg.gopsize": fps,
        "render.ffmpeg.audio_codec": "AAC",
        "display_settings.display_device": "sRGB",
        "view_settings.view_transform": "AgX",
        "view_settings.look": "None",
        "view_settings.exposure": EXPOSURE,
        "display.render_aa": "16",
        "display.viewport_aa": "16",
        "display.shading.light": "STUDIO",
        "display.shading.studio_light": HEADLAMP.name,
        "display.shading.color_type": "SINGLE",
        "display.shading.show_specular_highlight": False,
        "display.shading.use_world_space_lighting": False,
        "display.shading.single_color": Paint(Surface.SHADED),
        "display.shading.object_outline_color": Paint(Ink.SCREEN),
        "display.shading.show_cavity": False,
        "display.shading.show_object_outline": True,
        "cycles.device": "GPU",
        "cycles.samples": SAMPLES,
        "cycles.preview_samples": 256,
        "cycles.use_adaptive_sampling": True,
        "cycles.adaptive_threshold": NOISE_THRESHOLD,
        "cycles.use_denoising": True,
        "cycles.denoiser": "OPENIMAGEDENOISE",
        "cycles.denoising_use_gpu": True,
        "cycles.use_preview_denoising": True,
        "cycles.preview_denoising_input_passes": "RGB_ALBEDO_NORMAL",
        "cycles.max_bounces": MAX_BOUNCES,
        "cycles.diffuse_bounces": DIFFUSE_BOUNCES,
        "cycles.glossy_bounces": GLOSSY_BOUNCES,
        "cycles.transmission_bounces": TRANSMISSION_BOUNCES,
        "cycles.volume_bounces": VOLUME_BOUNCES,
        "cycles.transparent_max_bounces": TRANSPARENT_BOUNCES,
        "cycles.sample_clamp_direct": 0.0,
        "cycles.sample_clamp_indirect": clamp,
        "cycles.blur_glossy": FILTER_GLOSSY,
        "cycles.caustics_reflective": CAUSTICS,
        "cycles.caustics_refractive": CAUSTICS,
        "eevee.use_raytracing": True,
        "eevee.taa_render_samples": 128,
        "eevee.clamp_surface_indirect": clamp,
        "eevee.clamp_volume_indirect": clamp,
        "eevee.ray_tracing_options.resolution_scale": "1",
        "eevee.fast_gi_resolution": "1",
        "tool_settings.use_snap": True,
        "tool_settings.snap_target": "CLOSEST",
        "tool_settings.snap_angle_increment_3d": radians(ANGLE_STEP),
        "tool_settings.snap_angle_increment_3d_precision": radians(1),
        "tool_settings.snap_angle_increment_2d": radians(ANGLE_STEP),
        "tool_settings.snap_angle_increment_2d_precision": radians(1),
        "tool_settings.snap_elements": {"VERTEX", "EDGE_MIDPOINT", "EDGE_PERPENDICULAR", "FACE_MIDPOINT"},
        **dict.fromkeys((f"tool_settings.use_snap_{kind}" for kind in ("translate", "rotate", "backface_culling", "self", "edit", "nonedit")), True),
        **dict.fromkeys((f"tool_settings.use_snap_{kind}" for kind in ("scale", "node", "selectable", "align_rotation", "peel_object")), False),
        "tool_settings.use_proportional_edit": False,
        "tool_settings.use_mesh_automerge": False,
        "tool_settings.use_transform_correct_face_attributes": True,
        "tool_settings.use_transform_correct_keep_connected": True,
        "tool_settings.transform_pivot_point": "MEDIAN_POINT",
        "transform_orientation_slots[0].type": "GLOBAL",
        f"{texture}.sky_type": "MULTIPLE_SCATTERING",
        f"{texture}.sun_disc": False,
        f"{texture}.altitude": ELEVATION,
        f'{background}.inputs["Strength"].default_value': 1.0,
        f"{light}.data.type": "SUN",
        f"{light}.data.energy": SUN_IRRADIANCE,
        f"{light}.data.angle": bpy.types.SunLight.bl_rna.properties["angle"].default,
        "camera.location": (Vector((0.0, 0.0, EYE_HEIGHT)) - PLAN_DISTANCE * north)[:],
        "camera.rotation_euler": north.to_track_quat("-Z", "Y").to_euler()[:],
        "camera.data.dof.focus_distance": PLAN_DISTANCE,
        "camera.data.lens": LENS,
        "camera.name": "Camera",
        f"{light}.data.name": "Sun",
        f"{light}.name": "Sun",
    }
    return ((subscript("scenes", scene.name), scene, startup), (subscript("scenes", ANALYSIS), bpy.data.scenes[ANALYSIS], {"world": bpy.data.worlds[ANALYSIS], "render.filepath": output}))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ANALYSIS", "EYE_HEIGHT", "HEADLAMP", "PLAN_DISTANCE", "cleared_objects", "declared_scenes", "prepared_scene", "sun", "world_node"]
