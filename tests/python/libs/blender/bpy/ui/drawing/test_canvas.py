"""Canvas text: measured extents, wrapping, and labels drawn offscreen."""

from expression import Ok

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_text_measures_grow_with_length_and_wrap(observed: Observations) -> None:
    """Text measures a positive extent, longer text measures wider, and wrapping it narrower than its line measures taller."""
    width, height = observed.of(tuple, ("drawing.written", "single"))
    assert (width > 0, height > 0) == (True, True)
    assert (observed["drawing.written", "longer"], observed["drawing.written", "wrapped"]) == (True, True)


def test_label_draws_opaque_text(observed: Observations) -> None:
    """A white label drawn offscreen leaves fully opaque pixels."""
    assert observed["drawing.written", "label"] == Ok(1.0)
