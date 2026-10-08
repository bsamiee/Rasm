# ty: ignore[unresolved-attribute]
# mypy: disable-error-code="attr-defined, union-attr"
"""Blender's startup scenes with their data-blocks, render, pass, color, snapping, sky, sun, ground, and camera settings, and the stock objects every task deletes."""

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
    ADAPTIVE_MIN_SAMPLES,
    CAUSTICS,
    DIFFUSE_BOUNCES,
    DIRECT_CLAMP,
    DPI,
    ELEVATION,
    EXPOSURE,
    FILTER_GLOSSY,
    FRAME_SIZE,
    GLOSSY_BOUNCES,
    GROUND_ALBEDO,
    INDIRECT_CLAMP,
    LATITUDE,
    LENS,
    LIGHT_TREE,
    LONGITUDE,
    MAX_BOUNCES,
    NOISE_THRESHOLD,
    NORTH,
    Pass,
    SAMPLES,
    SUN_IRRADIANCE,
    TRANSMISSION_BOUNCES,
    TRANSPARENT_BOUNCES,
    VOLUME_BOUNCES,
)
from interface.report import Row, subscript
from interface.roles import Annotation, Ink, Surface
from interface.units import ANGLE_STEP, Length, Units

# --- [CONSTANTS] ------------------------------------------------------------------------

ANALYSIS: Final = "Analysis"
GROUND: Final = "Ground"

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
    """Objects every task keeps, the scene's light and camera objects and the ground."""
    return frozenset(target for target in scene.objects if target.type in {"LIGHT", "CAMERA"} or target.name == GROUND)


def typed_node(nodes: bpy.types.Nodes, kind: str) -> bpy.types.Node:
    """Node of the node type among the tree's nodes."""
    return next(node for node in nodes if node.type == kind)


def prepared_scene(scene: bpy.types.Scene) -> tuple[Row, ...]:
    """Rows of the startup data the RNA paths cannot state: the analysis scene and world, annotation collection, and ground material, then the site, sky link, ground plane and its diffuse surface, root objects, and root collections, whose write unlinks the factory collection."""
    tree, label = scene.world.node_tree, subscript("scenes", scene.name)
    background = typed_node(tree.nodes, "BACKGROUND")
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

    def grounded(name: str) -> None:
        mesh, corners = bpy.data.meshes.new(name), ((-1.0, -1.0, 0.0), (1.0, -1.0, 0.0), (1.0, 1.0, 0.0), (-1.0, 1.0, 0.0))
        mesh.from_pydata(corners, (), (range(len(corners)),))
        mesh.materials.append(bpy.data.materials[name])
        root.objects.link(bpy.data.objects.new(name, mesh))

    def surfaced(kinds: tuple[str, str]) -> None:
        shader = bpy.data.materials[GROUND].node_tree
        shader.nodes.clear()
        source, sink = map(shader.nodes.new, kinds)
        shader.links.new(source.outputs["BSDF"], sink.inputs["Surface"])

    def surface() -> tuple[str, ...]:
        return tuple(node.bl_idname for link in bpy.data.materials[GROUND].node_tree.links for node in (link.from_node, link.to_node))

    blocks = (("scenes", bpy.data.scenes, ANALYSIS), ("worlds", bpy.data.worlds, ANALYSIS), ("collections", bpy.data.collections, Annotation.NAME), ("materials", bpy.data.materials, GROUND))
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
            write=lambda kind: tree.links.new(typed_node(tree.nodes, kind).outputs["Color"], color),
            target="TEX_SKY",
        ),
        Row(label=subscript("objects", GROUND), read=partial(held, bpy.data.objects, GROUND), write=grounded, target=GROUND),
        Row(label=f"{subscript('materials', GROUND)}.node_tree.links", read=surface, write=surfaced, target=("ShaderNodeBsdfDiffuse", "ShaderNodeOutputMaterial")),
        Row(
            label=f"{label}.collection.objects",
            read=lambda: tuple(sorted(target.name for target in anchored if target.users_collection == (root,))),
            write=rooted,
            target=tuple(sorted(target.name for target in anchored)),
        ),
        Row(label=f"{label}.collection.children", read=lambda: tuple(child.name for child in root.children), write=linked, target=(Annotation.NAME,)),
    )


def cleared_objects(scene: bpy.types.Scene) -> tuple[Row, ...]:
    """Rows deleting every object but the sun, camera, and ground from the startup and analysis scenes, orphaned data included."""
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
    texture, background = (subscript("world.node_tree.nodes", typed_node(scene.world.node_tree.nodes, kind).name) for kind in ("TEX_SKY", "BACKGROUND"))
    light, collection = subscript("objects", sun(scene).name), subscript("collection.children", Annotation.NAME)
    ground, far = subscript("objects", GROUND), max(units.far for units in Units)
    diffuse = subscript(f"{ground}.active_material.node_tree.nodes", typed_node(bpy.data.materials[GROUND].node_tree.nodes, "BSDF_DIFFUSE").name)
    (width, height), fps, video_bitrate = FRAME_SIZE, 24, 6000
    dither_lsb, lsb_per_intensity, light_threshold = 1.0, 255 * 0.0033, 0.05
    direct_clamp, indirect_clamp = (clamp / 2**EXPOSURE for clamp in (DIRECT_CLAMP, INDIRECT_CLAMP))

    def pass_member(kind: Pass) -> str:
        """View layer member enabling the render pass."""
        match kind:
            case Pass.DEPTH:
                return "use_pass_z"
            case Pass.NORMAL:
                return "use_pass_normal"
            case Pass.ALBEDO:
                return "use_pass_diffuse_color"
            case Pass.MATERIAL_INDEX:
                return "use_pass_material_index"
            case Pass.OBJECT_INDEX:
                return "use_pass_object_index"

    startup = {
        f"{collection}.color_tag": Annotation.TAG.name,
        f"{collection}.lineart_usage": "INCLUDE",
        **dict.fromkeys((f"{collection}.{flag}" for flag in ("hide_viewport", "hide_select", "hide_render")), False),
        "view_layers[0].active_layer_collection": scene.view_layers[0].layer_collection,
        **dict.fromkeys((f"view_layers[0].{pass_member(kind)}" for kind in Pass), True),
        "render.fps": fps,
        "render.use_persistent_data": True,
        "render.filepath": output,
        "render.engine": "CYCLES",
        "render.use_lock_interface": True,
        "render.anisotropic_filter": "FILTER_16",
        "render.resolution_x": width,
        "render.resolution_y": height,
        "render.ppm_factor": DPI,
        "render.ppm_base": Length.INCHES,
        "render.dither_intensity": dither_lsb / lsb_per_intensity,
        "render.image_settings.color_depth": "16",
        "render.ffmpeg.video_bitrate": video_bitrate,
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
        "display.shading.studio_light": HEADLAMP.name,
        "display.shading.color_type": "SINGLE",
        "display.shading.show_specular_highlight": False,
        "display.shading.single_color": Paint(Surface.SHADED),
        "display.shading.object_outline_color": Paint(Ink.SCREEN),
        "display.shading.show_object_outline": True,
        "cycles.device": "GPU",
        "cycles.samples": SAMPLES,
        "cycles.preview_samples": 256,
        "cycles.adaptive_threshold": NOISE_THRESHOLD,
        "cycles.adaptive_min_samples": ADAPTIVE_MIN_SAMPLES,
        "cycles.denoising_use_gpu": True,
        "cycles.use_preview_denoising": True,
        "cycles.preview_denoising_input_passes": "RGB_ALBEDO_NORMAL",
        "cycles.max_bounces": MAX_BOUNCES,
        "cycles.diffuse_bounces": DIFFUSE_BOUNCES,
        "cycles.glossy_bounces": GLOSSY_BOUNCES,
        "cycles.transmission_bounces": TRANSMISSION_BOUNCES,
        "cycles.volume_bounces": VOLUME_BOUNCES,
        "cycles.transparent_max_bounces": TRANSPARENT_BOUNCES,
        "cycles.sample_clamp_direct": direct_clamp,
        "cycles.sample_clamp_indirect": indirect_clamp,
        "cycles.blur_glossy": FILTER_GLOSSY,
        "cycles.caustics_reflective": CAUSTICS,
        "cycles.caustics_refractive": CAUSTICS,
        "cycles.use_light_tree": LIGHT_TREE,
        "cycles.light_sampling_threshold": light_threshold,
        "eevee.use_raytracing": True,
        "eevee.taa_render_samples": 128,
        "eevee.clamp_surface_indirect": indirect_clamp,
        "eevee.clamp_volume_indirect": indirect_clamp,
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
        f"{ground}.scale": (far, far, 1.0),
        f"{ground}.hide_select": True,
        f"{ground}.display_type": "WIRE",
        f"{ground}.lineart.usage": "NO_INTERSECTION",
        f'{diffuse}.inputs["Color"].default_value': (GROUND_ALBEDO, GROUND_ALBEDO, GROUND_ALBEDO, 1.0),
        "camera.location": (Vector((0.0, 0.0, EYE_HEIGHT)) - PLAN_DISTANCE * north)[:],
        "camera.rotation_euler": north.to_track_quat("-Z", "Y").to_euler()[:],
        "camera.data.dof.focus_distance": PLAN_DISTANCE,
        "camera.data.lens": LENS,
        "camera.data.sensor_fit": "VERTICAL",
        "camera.name": "Camera",
        f"{light}.data.name": "Sun",
        f"{light}.name": "Sun",
    }
    return ((subscript("scenes", scene.name), scene, startup), (subscript("scenes", ANALYSIS), bpy.data.scenes[ANALYSIS], {"world": bpy.data.worlds[ANALYSIS], "render.filepath": output}))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ANALYSIS", "EYE_HEIGHT", "HEADLAMP", "PLAN_DISTANCE", "cleared_objects", "declared_scenes", "prepared_scene", "sun", "typed_node"]
