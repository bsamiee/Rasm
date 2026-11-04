"""Native hierarchy copies remap arbitrary-depth ID properties."""

import sys

import bpy
from idprop.types import IDPropertyGroup
import pytest

from blender.bpy.scene.collections import duplicated

# --- [TESTS] ----------------------------------------------------------------------------


def test_duplicate_remaps_deep_groups_and_heterogeneous_native_lists(scene: bpy.types.Scene) -> None:
    """Copied ID references change through deep groups and list ancestors while source references remain intact."""
    source = bpy.data.collections.new("Deep source")
    scene.collection.children.link(source)
    mesh = bpy.data.meshes.new("Deep source mesh")
    item = bpy.data.objects.new("Deep source object", mesh)
    source.objects.link(item)
    item["mixed"] = ["retained", mesh, {"mesh": mesh}]
    item["deep"] = {}
    group: IDPropertyGroup = item["deep"]  # ty: ignore[unsound-assignment]
    depth = sys.getrecursionlimit() + 1
    for _ in range(depth):
        group.update({"next": {}})
        group = group.get("next")  # ty: ignore[unsound-assignment]
    group.update({"mesh": mesh})
    copied = duplicated(source, scene.collection, frozenset({bpy.types.Mesh})).default_with(lambda faults: pytest.fail(repr(faults)))
    (copy,) = copied.objects
    copied_mesh: bpy.types.Mesh = copy.data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    assert copied_mesh != mesh
    assert copy["mixed"][0] == "retained"
    assert copy["mixed"][1] == copied_mesh
    assert copy["mixed"][2]["mesh"] == copied_mesh
    assert item["mixed"][1] == mesh
    group = copy["deep"]  # ty: ignore[unsound-assignment]
    for _ in range(depth):
        group = group.get("next")  # ty: ignore[unsound-assignment]
    assert group.get("mesh") == copied_mesh
