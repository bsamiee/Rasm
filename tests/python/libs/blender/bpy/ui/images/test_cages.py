"""Frame region handles: the cage group, its frame read and edit without a region, and edits kept inside the bounds."""

from expression import Error

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_frame_region_registers_its_handle_group(observed: Observations, released: Observations) -> None:
    """The handle group registers in the image editor's main region and leaves with release."""
    assert (observed["images.caged", "group"], released["images.images", "group"]) == (("IMAGE_EDITOR", "WINDOW"), False)


def test_handles_read_neutral_and_refuse_edits_without_a_region(observed: Observations) -> None:
    """With no region in context the cage reads the identity frame, a corner reads no offset, and an edit faults for the absent quad."""
    identity = (1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0)
    assert (observed["images.caged", "cage"], observed["images.caged", "corner"], observed["images.caged", "edit"]) == (identity, (0.0, 0.0, 0.0), Error(("ABSENT",)))


def test_edits_stay_inside_the_bounds(observed: Observations) -> None:
    """A moved quad shifts whole back inside the bounds, a stretched quad clips at them, and unbounded edits pass unchanged."""
    assert observed["images.caged", "moved"] == ((0.6, 0.9), (1.0, 0.9), (1.0, 0.5), (0.6, 0.5))
    assert observed["images.caged", "stretched"] == ((0.0, 1.0), (1.0, 1.0), (1.0, 0.5), (0.0, 0.5))
    assert observed["images.caged", "unbounded"] == ((2.0, 2.0),)
