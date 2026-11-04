"""Native modifier reconciliation preserves retained entries and pinned order."""

import bpy
from expression import Some
import pytest

from blender.bpy.scene.objects import ObjectState, reconciled, StackEntry

# --- [TESTS] ----------------------------------------------------------------------------


def test_modifier_addition_before_a_pinned_suffix_preserves_native_entries(scene: bpy.types.Scene) -> None:
    """Native insertion and subsequent name-based moves keep retained modifier identity and state."""
    mesh = bpy.data.meshes.new("Pinned source")
    item = bpy.data.objects.new("Pinned source", mesh)
    scene.collection.objects.link(item)
    first: bpy.types.BevelModifier = item.modifiers.new("First", "BEVEL")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    pinned: bpy.types.BevelModifier = item.modifiers.new("Pinned", "BEVEL")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    pinned.use_pin_to_last = True
    first.width, pinned.width = 0.25, 0.75
    state = ObjectState(modifiers=Some({"Inserted": StackEntry(kind="BEVEL"), first.name: StackEntry(kind="BEVEL"), pinned.name: StackEntry(kind="BEVEL")}))
    reconciled(state, item, tuple(scene.view_layers)).default_with(lambda faults: pytest.fail(repr(faults)))
    assert tuple(modifier.name for modifier in item.modifiers) == ("Inserted", first.name, pinned.name)
    assert item.modifiers[first.name] == first
    assert item.modifiers[pinned.name] == pinned
    assert (first.width, pinned.width, pinned.use_pin_to_last) == (0.25, 0.75, True)
