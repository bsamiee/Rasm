"""Gizmo groups of stock and custom-shaped gizmos, their keymap and press callbacks, and linking groups that are not persistent."""

from expression import Error, Ok

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_group_registers_in_its_editor_region(observed: Observations) -> None:
    """A group registers in its editor region under its title, with no persistent flag while it is not persistent and the flag while it is."""
    assert tuple(observed["surfaces.gizmos", "group", "some", member] for member in ("bl_space_type", "bl_region_type", "bl_label", "bl_options")) == ("VIEW_3D", "WINDOW", "Main", ())
    assert observed["surfaces.gizmos", "persistent", "some", "bl_options"] == ("PERSISTENT",)


def test_group_holds_keymap_and_press_callbacks(observed: Observations) -> None:
    """A named keymap adds the host keymap setup, and a part that prepares on press adds the press callback."""
    assert observed["surfaces.gizmos", "callbacks"] == ("invoke_prepare", "setup_keymap")


def test_custom_shape_declares_its_targets(observed: Observations) -> None:
    """A custom shape registers a gizmo type declaring its float targets."""
    assert observed["surfaces.gizmos", "shape"] == ("offset",)


def test_ensured_links_only_groups_that_are_not_persistent(observed: Observations) -> None:
    """A group that is not persistent links into its regions for a scope, and a persistent group refuses the link."""
    assert (observed["surfaces.gizmos", "ensured"], observed["surfaces.gizmos", "refused"]) == (Ok(None), Error(("REFUSED",)))


def test_release_removes_group_and_shape(released: Observations) -> None:
    """Neither the group nor the custom gizmo type stays registered after release."""
    assert (released["surfaces.classes", "FIXTURE_GGT_main"], released["surfaces.classes", "FIXTURE_GT_mark"]) == (False, False)
