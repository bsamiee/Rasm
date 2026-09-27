# ty: ignore[unresolved-attribute]
# mypy: disable-error-code="attr-defined, union-attr"
"""Blender's startup scenes with their render, color, snapping, sky, sun, and camera settings and the stock data every task deletes."""

from functools import partial
from math import radians
from typing import Final

import bpy
from mathutils import Matrix, Vector

from interface.blender.theme import Encoding, Paint
from interface.render import (
    CAUSTICS,
    DIFFUSE_BOUNCES,
    DPI,
    ELEVATION,
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
    TRANSMISSION_BOUNCES,
    TRANSPARENT_BOUNCES,
    VOLUME_BOUNCES,
)
from interface.report import Row
from interface.roles import Annotation, Ink, Surface
from interface.units import ANGLE_STEP, FOOT, INCH

# --- [CONSTANTS] ------------------------------------------------------------------------

EYE_HEIGHT: Final = 66 * INCH
PLAN_DISTANCE: Final = 100 * FOOT
ANALYSIS: Final = "Analysis"
LIBRARY: Final = "Library"

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [DATA]
def sun(scene: bpy.types.Scene) -> bpy.types.Object:
    """Scene's one light object."""
    return next(target for target in scene.objects if target.type == "LIGHT")


def world_node(scene: bpy.types.Scene, kind: str) -> bpy.types.Node:
    """World shader node of the node type."""
    return next(node for node in scene.world.node_tree.nodes if node.type == kind)


def prepared_scene(scene: bpy.types.Scene) -> list[Row]:
    """Rows of the startup data the RNA paths cannot state, from the analysis and library scenes to the sky link, annotation collection, and root objects."""
    tree, color = scene.world.node_tree, world_node(scene, "BACKGROUND").inputs["Color"]
    root, held = scene.collection, (sun(scene), scene.camera)

    def annotation() -> bpy.types.Collection:
        return bpy.data.collections.get(Annotation.NAME) or bpy.data.collections.new(Annotation.NAME)

    def stamp(key: str) -> object:
        child = root.children.get(Annotation.NAME)
        return None if child is None else child.get(key)

    def stamped(key: str, value: object) -> None:
        annotation()[key] = value

    def children(_target: object) -> None:
        for child in [child for child in root.children if child.name != Annotation.NAME]:
            root.children.unlink(child)
        if root.children.get(Annotation.NAME) is None:
            root.children.link(annotation())

    def rooted(_target: object) -> None:
        for target in held:
            for owner in [owner for owner in target.users_collection if owner != root]:
                owner.objects.unlink(target)
            if root not in target.users_collection:
                root.objects.link(target)

    def analyzed() -> str:
        return next((each.world.name for each in bpy.data.scenes if each.name == ANALYSIS and each.world is not None), "")

    def analysis(_target: object) -> None:
        (bpy.data.scenes.get(ANALYSIS) or bpy.data.scenes.new(ANALYSIS)).world = bpy.data.worlds.get(ANALYSIS) or bpy.data.worlds.new(ANALYSIS)

    return [
        Row(label=f'scenes["{ANALYSIS}"].world.name', read=analyzed, write=analysis, target=ANALYSIS),
        Row(label=f'scenes["{LIBRARY}"]', read=lambda: LIBRARY in bpy.data.scenes, write=lambda _target: bpy.data.scenes.new(LIBRARY), target=True),
        *(Row(label=f'scene["{key}"]', read=partial(scene.get, key), write=partial(scene.__setitem__, key), target=value) for key, value in (("lat", LATITUDE), ("lon", LONGITUDE))),
        Row(label="scene.world.node_tree.nodes[TEX_SKY]", read=lambda: any(node.type == "TEX_SKY" for node in tree.nodes), write=lambda _target: tree.nodes.new("ShaderNodeTexSky"), target=True),
        Row(
            label='scene.world.node_tree.nodes["Background"].inputs["Color"].is_linked',
            read=lambda: color.is_linked,
            write=lambda _target: tree.links.new(world_node(scene, "TEX_SKY").outputs["Color"], color),
            target=True,
        ),
        Row(label="scene.collection.children", read=lambda: tuple(child.name for child in root.children), write=children, target=(Annotation.NAME,)),
        *(
            Row(label=f'scene.collection.children["{Annotation.NAME}"]["{key}"]', read=partial(stamp, key), write=partial(stamped, key), target=value)
            for key, value in (("rhid", str(Annotation.ID)), ("dimensions_collection_role", "DIMENSIONS"))
        ),
        Row(
            label="scene.collection.objects",
            read=lambda: tuple(sorted(target.name for target in held if target.users_collection == (root,))),
            write=rooted,
            target=tuple(sorted(target.name for target in held)),
        ),
    ]


def cleared(scene: bpy.types.Scene) -> list[Row]:
    """Rows deleting every object but the sun and camera from the startup and analysis scenes, orphaned data included."""
    held = {sun(scene), scene.camera}

    def stock(owner: bpy.types.Scene) -> tuple[str, ...]:
        return tuple(sorted(target.name for target in owner.objects if target not in held))

    def objects(owner: bpy.types.Scene, _target: object) -> None:
        bpy.data.batch_remove([target for target in owner.objects if target not in held])
        bpy.data.orphans_purge(do_recursive=True)

    return [Row(label=f'scenes["{owner.name}"].objects', read=partial(stock, owner), write=partial(objects, owner), target=()) for owner in (scene, bpy.data.scenes[ANALYSIS])]


# --- [SETTINGS]
def declared_scene(scene: bpy.types.Scene, preferences: bpy.types.Preferences) -> dict[str, object]:
    """Declared startup scene values by path, each dependent value after the value it depends on."""
    exposure = -5.3
    north = Matrix.Rotation(radians(NORTH), 3, "Z") @ Vector((0.0, 1.0, 0.0))
    texture, background = (f'world.node_tree.nodes["{world_node(scene, kind).name}"]' for kind in ("TEX_SKY", "BACKGROUND"))
    light, collection = f'objects["{sun(scene).name}"]', f'collection.children["{Annotation.NAME}"]'
    (width, height), fps, clamp = FRAME_SIZE, 24, INDIRECT_CLAMP / 2**exposure
    return {
        f"{collection}.color_tag": Annotation.TAG.name,
        f"{collection}.lineart_usage": "INCLUDE",
        **dict.fromkeys((f"{collection}.{flag}" for flag in ("hide_viewport", "hide_select", "hide_render")), False),
        "view_layers[0].active_layer_collection": scene.view_layers[0].layer_collection,
        "render.fps": fps,
        "render.fps_base": 1.0,
        "render.use_persistent_data": True,
        "render.filepath": preferences.filepaths.render_output_directory,
        "render.engine": "CYCLES",
        "render.use_lock_interface": True,
        "render.anisotropic_filter": "FILTER_16",
        "render.resolution_x": width,
        "render.resolution_y": height,
        "render.resolution_percentage": 100,
        "render.ppm_factor": DPI,
        "render.ppm_base": INCH,
        "render.image_settings.color_depth": "16",
        "render.ffmpeg.format": "MPEG4",
        "render.ffmpeg.codec": "H264",
        "render.ffmpeg.constant_rate_factor": "HIGH",
        "render.ffmpeg.gopsize": fps,
        "render.ffmpeg.audio_codec": "AAC",
        "display_settings.display_device": "sRGB",
        "view_settings.view_transform": "AgX",
        "view_settings.look": "None",
        "view_settings.exposure": exposure,
        "display.render_aa": "16",
        "display.viewport_aa": "16",
        "display.shading.light": "STUDIO",
        "display.shading.studio_light": "Headlamp.sl",
        "display.shading.color_type": "SINGLE",
        "display.shading.show_specular_highlight": False,
        "display.shading.use_world_space_lighting": False,
        "display.shading.single_color": Paint(Surface.SHADED, encoding=Encoding.LINEAR),
        "display.shading.object_outline_color": Paint(Ink.SCREEN, encoding=Encoding.LINEAR),
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
        f"{light}.data.energy": 137.0,
        f"{light}.data.angle": bpy.types.SunLight.bl_rna.properties["angle"].default,
        "camera.location": (Vector((0.0, 0.0, EYE_HEIGHT)) - PLAN_DISTANCE * north)[:],
        "camera.rotation_euler": north.to_track_quat("-Z", "Y").to_euler()[:],
        "camera.data.dof.focus_distance": PLAN_DISTANCE,
        "camera.data.lens": LENS,
        "camera.name": "Camera",
        f"{light}.data.name": "Sun",
        f"{light}.name": "Sun",
    }


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ANALYSIS", "EYE_HEIGHT", "LIBRARY", "PLAN_DISTANCE", "cleared", "declared_scene", "prepared_scene", "sun", "world_node"]
