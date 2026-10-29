"""Asset shelves over extension asset marks."""

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_shelf_registers_its_options_and_filters(observed: Observations) -> None:
    """A visible shelf without drag registers in its editor with the host flags those declarations name and its ID type filters set."""
    read = ("bl_space_type", "filter_mesh", "bl_options")
    assert tuple(observed["surfaces.shelved", "some", member] for member in read) == ("VIEW_3D", True, ("DEFAULT_VISIBLE", "NO_ASSET_DRAG", "STORE_ENABLED_CATALOGS_IN_PREFERENCES"))


def test_unknown_filter_fails_registration(released: Observations) -> None:
    """A filter the host's shelf type does not declare fails registration."""
    assert released["surfaces.refusals", "unknown filter"] == "RuntimeError"
