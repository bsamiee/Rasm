"""Native camera projection, orthographic floors, and multiview layer synchronization."""

from typing import Literal

import bpy
from bpy_extras.object_utils import world_to_camera_view
from mathutils import Euler, Matrix, Vector
import numpy as np
import pytest

from blender.bpy.scene.cameras import framed, view_cameras
from blender.records.faults import Ok

# --- [TESTS] ----------------------------------------------------------------------------


@pytest.mark.parametrize("kind", ["PERSP", "ORTHO"])
@pytest.mark.parametrize("translation", [(0.0, 0.0, 0.0), (1e8, 2e8, -3e8)])
def test_framing_clip_planes_enclose_native_projection_depths(scene: bpy.types.Scene, kind: Literal["PERSP", "ORTHO"], translation: tuple[float, float, float]) -> None:
    """Clip planes enclose native float32 projection depths after scaled, rotated, and translated camera fits."""
    camera = bpy.data.cameras.new("Projected bounds")
    camera.type = kind
    camera.shift_x, camera.shift_y = 0.2, -0.1
    item = bpy.data.objects.new("Projected bounds", camera)
    scene.collection.objects.link(item)
    scene.camera = item
    item.matrix_world = Matrix.LocRotScale(translation, Euler((0.2, -0.3, 0.1)), (2.0, 3.0, 4.0))
    coordinates = np.asarray(translation, np.float32) + np.asarray(((-32.0, -16.0, -64.0), (32.0, 16.0, -64.0), (-32.0, 16.0, -96.0), (32.0, -16.0, -96.0)), np.float32)
    layer = scene.view_layers[0]
    with bpy.context.temp_override(scene=scene, view_layer=layer):
        layer.update()
        graph = bpy.context.evaluated_depsgraph_get()
        assert framed(item, coordinates, graph) == Ok(None)
        layer.update()
        depths = tuple(world_to_camera_view(scene, item, Vector(point)).z for point in coordinates)
    assert camera.clip_start <= min(depths)
    assert camera.clip_end >= max(depths)


def test_zero_extent_orthographic_fit_keeps_the_native_setter_floor(scene: bpy.types.Scene) -> None:
    """A zero-scale native fit retains Blender's positive orthographic setter floor."""
    camera = bpy.data.cameras.new("Zero extent")
    reference = bpy.data.cameras.new("Native zero extent")
    camera.type = reference.type = "ORTHO"
    item = bpy.data.objects.new("Zero extent", camera)
    scene.collection.objects.link(item)
    coordinates = np.full((2, 3), 0.0, np.float32)
    layer = scene.view_layers[0]
    with bpy.context.temp_override(scene=scene, view_layer=layer):
        layer.update()
        graph = bpy.context.evaluated_depsgraph_get()
        _, scale = item.camera_fit_coords(graph, tuple(map(float, coordinates.ravel())))
        reference.ortho_scale = scale
        assert framed(item, coordinates, graph) == Ok(None)
    assert camera.ortho_scale == reference.ortho_scale


def test_multiview_camera_lookup_synchronizes_after_a_native_link(scene: bpy.types.Scene) -> None:
    """A camera linked after layer evaluation is resolved without a caller-owned update."""
    scene.render.use_multiview = True
    scene.render.views_format = "MULTIVIEW"
    for view in scene.render.views:
        view.use = False
    left = scene.render.views.new("Source view")
    left.camera_suffix, left.use = "_L", False
    right = scene.render.views.new("Resolved view")
    right.camera_suffix, right.use = "_R", True
    camera = bpy.data.cameras.new("Multiview source")
    base = bpy.data.objects.new("Multiview_L", camera)
    scene.collection.objects.link(base)
    scene.camera = base
    scene.view_layers[0].update()
    alternate = bpy.data.objects.new("Multiview_R", camera)
    scene.collection.objects.link(alternate)
    found = view_cameras(scene, (base,)).default_with(lambda faults: pytest.fail(repr(faults)))
    assert found == {(base, right): alternate}
