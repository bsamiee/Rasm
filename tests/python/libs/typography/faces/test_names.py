"""Glyph names preserve code points, forms, suffixes, and opaque original names."""

from hypothesis import given, strategies as st
import msgspec
import pytest

from typography.faces.names import Form, GlyphName

# --- [OPERATIONS] -----------------------------------------------------------------------


@given(codes=st.lists(st.characters(codec="utf-8").map(ord), min_size=1, max_size=3), form=st.sampled_from(Form), variant=st.sampled_from(("", "ss01", "alt")))
def test_coded_names_round_trip_without_losing_components(codes: list[int], form: Form, variant: str) -> None:
    """Rendered Unicode components and optional joining and variant suffixes parse back to their source."""
    name = GlyphName(stem=tuple(codes), form=form, variant=variant)
    assert GlyphName.parse(name.text) == name
    assert str(name) == name.text


@pytest.mark.parametrize(
    "name,text",
    [
        (GlyphName(stem=".notdef"), ".notdef"),
        (GlyphName(stem="ARB_114", form=Form.INIT), "ARB_114.init"),
        (GlyphName(stem=(0x20,)), "space"),
        (GlyphName(stem=(0x41,)), "A"),
        (GlyphName(stem=(0x0644, 0x0627), form=Form.FINA), "uni0644_uni0627.fina"),
        (GlyphName(stem=(0x06F4,), variant="ss01"), "uni06F4.ss01"),
    ],
)
def test_host_name_spellings_remain_canonical(name: GlyphName, text: str) -> None:
    """Opaque original names and AGL spellings keep the native text consumed by font tables."""
    assert name.text == text
    assert GlyphName.parse(text) == name
    assert msgspec.json.decode(msgspec.json.encode(name), type=GlyphName) == name


def test_external_empty_code_stems_fail_at_the_codec_boundary() -> None:
    """Native record decoding refuses an empty encoded glyph stem."""
    with pytest.raises(msgspec.ValidationError, match=r"\$\.stem"):
        msgspec.json.decode(b'{"stem": []}', type=GlyphName)


@pytest.mark.parametrize("right,left,form", [(False, False, Form.ISOL), (False, True, Form.INIT), (True, True, Form.MEDI), (True, False, Form.FINA)])
def test_joining_sides_select_their_named_opentype_form(*, right: bool, left: bool, form: Form) -> None:
    """Each named OpenType form keeps its actual joining-side semantics."""
    assert (Form.joined(right=right, left=left), form.right, form.left) == (form, right, left)
