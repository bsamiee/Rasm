"""Staged glyphs loaded from SVG into the extension's previews, tinted in the widget text color, and icon values of every glyph kind."""

from expression import Error, Ok

lazy from tests.python.libs.blender.bpy.conftest import Observations

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_glyphs_resolve_their_icon_values(observed: Observations) -> None:
    """The brand resolves at registration and equals its staged glyph's value, a built-in icon reads the host value, unknown and missing glyphs fault, and tool icons path into the staged folder."""
    assert observed["resources.glyphs"] == {
        "brand": True,
        "staged": Ok(True),
        "built in": Ok(True),
        "unknown": Error(("ABSENT",)),
        "missing": Error(("ABSENT",)),
        "tool path": ("icons", "tools", "brush"),
    }


def test_glyphs_tint_and_retint(observed: Observations) -> None:
    """Opaque glyph pixels take the widget text color, a changed tint writes once, and the default tint restores the text color."""
    assert observed["resources.tinted"] == {"opaque": True, "deviation": 0.0, "red": (True, False), "restored": (True, 0.0)}
