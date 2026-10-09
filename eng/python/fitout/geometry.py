# mypy: disable-error-code="union-attr"
# ty: ignore[unresolved-attribute]

"""Export cubic contours through Blender's native geometry icon codec."""

from collections import defaultdict
from contextlib import ExitStack
from pathlib import Path
import runpy
from typing import TYPE_CHECKING

import bpy

if TYPE_CHECKING:
    from eng.python.fitout.icons import Glyphs

# --- [OPERATIONS] -----------------------------------------------------------------------


def packed(glyphs: "Glyphs", destination: Path, exporter: Path) -> None:
    """Write glyphs inside Blender using its unmodified upstream exporter."""
    write_mesh = runpy.run_path(str(exporter))["write_mesh_to_py"]
    destination.mkdir(parents=True, exist_ok=True)
    with ExitStack() as resources:
        scene = bpy.data.scenes.new("Geometry Icons")
        resources.callback(bpy.data.scenes.remove, scene)
        objects: defaultdict[str, list[bpy.types.Object]] = defaultdict(list)
        materials = {}
        for alpha in {alpha for groups in glyphs.values() for alpha, _ in groups}:
            material = bpy.data.materials.new(str(alpha))
            resources.callback(bpy.data.materials.remove, material)
            material.node_tree.nodes.new("ShaderNodeRGB").outputs[0].default_value = (1, 1, 1, alpha)
            materials[alpha] = material
        for stem, alpha, contours in ((stem, alpha, contours) for stem, groups in glyphs.items() for alpha, contours in groups):
            curve = bpy.data.curves.new(stem, "CURVE")
            resources.callback(bpy.data.curves.remove, curve)
            curve.dimensions = "2D"
            curve.fill_mode = "FRONT"
            curve.materials.append(materials[alpha])
            for contour in contours:
                spline = curve.splines.new("BEZIER")
                spline.use_cyclic_u = True
                spline.bezier_points.add(len(contour) - 1)
                spline.bezier_points.foreach_set("co", tuple(component for position, _, _ in contour for component in (*position, 0)))
                spline.bezier_points.foreach_set("handle_left", tuple(component for _, left, _ in contour for component in (*left, 0)))
                spline.bezier_points.foreach_set("handle_right", tuple(component for _, _, right in contour for component in (*right, 0)))
            obj = bpy.data.objects.new(stem, curve)
            resources.callback(bpy.data.objects.remove, obj, do_unlink=True)
            scene.collection.objects.link(obj)
            objects[stem].append(obj)
        with bpy.context.temp_override(scene=scene, view_layer=scene.view_layers[0]):
            depsgraph = bpy.context.evaluated_depsgraph_get()
        for stem, group in objects.items():
            evaluated = tuple(obj.evaluated_get(depsgraph) for obj in group)
            with (destination / f"{stem}.dat").open("wb") as output:
                write_mesh(output, evaluated[0], evaluated[1:])


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["packed"]
