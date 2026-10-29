"""Template rows realized once per extension identity over data-block, format, color, and keymap members."""

from expression import Error, Ok

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_templates_bind_members_of_their_kind(observed: Observations) -> None:
    """Previews of data-block pointers, image format settings, the grading wheel, and key caps realize, and a preview of a plain value or a foreign format keyword faults."""
    assert observed["layout.templated"] == {
        "preview": Ok(True),
        "preview of value": Error(("OUTSIDE",)),
        "format": Ok(True),
        "format keyword": Error(("OUTSIDE",)),
        "wheel": Ok(True),
        "shortcut": Ok(True),
    }
