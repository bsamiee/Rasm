"""GPU texture uploads and the least-recently-shown texture cache."""

from expression import Ok

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_planes_upload_as_half_float_textures(observed: Observations) -> None:
    """A top-first plane uploads at its width and height in RGBA16F."""
    assert observed["resources.textured", "uploaded"] == Ok((5, 3, "RGBA16F"))


def test_cache_evicts_oldest_unpinned_past_its_limit(observed: Observations) -> None:
    """A third texture past a limit of two evicts the oldest unpinned one, showing a texture moves it last, and memory counts half-float RGBA bytes."""
    assert observed["resources.textured", "stored"] == (Ok(("first",)), Ok(("first", "second")), Ok(("first", "third")))
    assert (observed["resources.textured", "shown"], observed["resources.textured", "order"]) == (True, ("third", "first"))
    assert observed["resources.textured", "memory"] == 2 * 5 * 3 * 4 * 2
