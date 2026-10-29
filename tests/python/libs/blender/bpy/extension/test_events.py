"""Host event callbacks: filtered depsgraph updates, load paths, startup runs, and runtime scene marks."""

from expression import Nothing

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_depsgraph_handler_passes_transform_updates_of_its_types(observed: Observations) -> None:
    """Moving an object passes one object update reporting a transform change to the filtered body."""
    assert observed["extension.depsgraph"] == (("Probe", "TRANSFORM"),)


def test_load_handler_reads_the_startup_file_as_no_path(observed: Observations) -> None:
    """Loading the startup file runs the persistent load body once with no path, and the startup body once, its timer idle in background mode."""
    assert observed["extension.reloaded", "answer"] == ("FINISHED",)
    assert observed["extension.reloaded", "loads"] == (Nothing,)
    assert observed["extension.reloaded", "starts"] == 1


def test_context_scene_is_no_runtime_scene(observed: Observations) -> None:
    """A file scene carries no mark of the extension's runtime scenes."""
    assert observed["extension.scened"] is False
