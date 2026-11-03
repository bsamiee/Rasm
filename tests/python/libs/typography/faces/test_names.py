"""Glyph name rendering, parsing, and joining forms."""

import pytest

from typography.faces.names import Form, GlyphName

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "name, text",
    [
        (GlyphName(codes=(0x0644, 0x0627), form=Form.FINA), "uni0644_uni0627.fina"),
        (GlyphName(codes=(0x06F4,), variant="ss01"), "uni06F4.ss01"),
        (GlyphName(codes=(0x0020,)), "space"),
        (GlyphName(codes=(0x0041,)), "A"),
        (GlyphName(base=".notdef"), ".notdef"),
        (GlyphName(base="ARB_114", form=Form.INIT), "ARB_114.init"),
    ],
)
def test_text_round_trips(name: GlyphName, text: str) -> None:
    """Rendered text names the glyph and parses back to the same name."""
    assert (name.text, str(name), GlyphName.parse(text)) == (text, text, name)


def test_name_refuses_codes_beside_base() -> None:
    """Name holding both code points and a base name, refused at construction."""
    with pytest.raises(ValueError, match="code points and a base name"):
        GlyphName(codes=(0x0628,), base="beh")


@pytest.mark.parametrize("right, left, form", [(False, False, Form.ISOL), (False, True, Form.INIT), (True, True, Form.MEDI), (True, False, Form.FINA)])
def test_joined_form_derives_from_sides(*, right: bool, left: bool, form: Form) -> None:
    """Form joining on the stated sides, its own side properties restating them."""
    assert (Form.joined(right=right, left=left), form.right, form.left) == (form, right, left)
