"""Binary import into a source: names, roles, anchors, lookups, aliases, records, and ornament faces."""

lazy from pathlib import Path

from expression import Result

from typography.faces.coverage import Coverage, Excluded
from typography.faces.declaration import Face, Kufi, OrnamentFace
from typography.faces.faults import Reason
from typography.faces.glyphs import Control, Digit, Glyph, Ligature, Mark, Ornament, role, Side, Sign, Source, Space
from typography.faces.importing import imported
from typography.faces.layout import Ligatures, Single
from typography.faces.names import Form, GlyphName

# --- [CONSTANTS] ------------------------------------------------------------------------

FACE = Face(style="Regular", original="Arabic-Regular.ttf", family="Test", genre=Kufi())

# --- [OPERATIONS] -----------------------------------------------------------------------


def _source(path: Path, face: Face | OrnamentFace = FACE, /) -> Source:
    match imported(path, face):
        case Result(tag="ok", ok=source):
            return source
        case Result(error=faults):
            raise AssertionError(faults)


def test_names_follow_cmap_forms_and_ligatures(arabic: Path) -> None:
    """Glyphs named from nominal code points, GSUB forms, presentation decomposition, and the lam-alef ligature; unreached and Private Use glyphs leave."""
    source = _source(arabic)
    assert [glyph.name.text for glyph in source.glyphs[:13]] == [
        ".notdef",
        "space",
        "uni0628",
        "uni0628.init",
        "uni0628.medi",
        "uni0628.fina",
        "uni0644",
        "uni0644.init",
        "uni0627",
        "uni0627.fina",
        "uni0644_uni0627",
        "uni064E",
        "uni0650",
    ]
    assert source.encoded == frozendict({0x0020: "space", 0x00A0: "uni00A0", 0x0627: "uni0627", 0x0628: "uni0628", 0x0644: "uni0644", 0x064E: "uni064E", 0x0650: "uni0650", 0xFE90: "uniFE90"})


def test_roles_and_categories_derive_from_unicode(arabic: Path) -> None:
    """Letters take joining type and form, marks their side, the ligature its components, and the space stays unassigned."""
    named = _source(arabic).named
    assert (named["uni0628.medi"].role, named["uni0650"].role, named["uni0644_uni0627"].role, named["space"].category, named["uni0644_uni0627"].category) == (
        role(GlyphName(codes=(0x0628,), form=Form.MEDI)),
        Mark(side=Side.BELOW),
        Ligature(components=("uni0644.init", "uni0627.fina")),
        "unassigned",
        "ligature",
    )


def test_anchors_from_mark_attachment(arabic: Path) -> None:
    """Base, mark, and ligature component anchors named by the side their mark class votes for."""
    named = _source(arabic).named
    assert (
        [(anchor.name, anchor.x, anchor.y) for anchor in named["uni0628.init"].anchors],
        [(anchor.name, anchor.y) for anchor in named["uni0650"].anchors],
        [anchor.name for anchor in named["uni0644_uni0627"].anchors],
        named["uni064E"].anchor("_top").map(lambda anchor: anchor.y).default_value(0),
    ) == ([("bottom", 250, -200), ("top", 250, 600)], [("_bottom", -80)], ["bottom_1", "bottom_2", "top_1", "top_2"], 80)


def test_lookups_keep_original_substitutions(arabic: Path) -> None:
    """GSUB singles and ligatures carried under their feature tags in lookup order, glyphs renamed."""
    assert _source(arabic).lookups == (
        Single(feature="init", order=0, mapping=frozendict({"uni0628": "uni0628.init", "uni0644": "uni0644.init"})),
        Single(feature="medi", order=1, mapping=frozendict({"uni0628": "uni0628.medi"})),
        Single(feature="fina", order=2, mapping=frozendict({"uni0628": "uni0628.fina", "uni0627": "uni0627.fina"})),
        Ligatures(feature="rlig", order=3, rules=((("uni0644.init", "uni0627.fina"), "uni0644_uni0627"),)),
    )


def test_secondary_code_points_become_component_aliases(arabic: Path) -> None:
    """Code points sharing a glyph map to aliases referencing it, each with its own role."""
    named = _source(arabic).named
    assert [(glyph.components[0].name, glyph.advance, glyph.role) for glyph in (named["uni00A0"], named["uniFE90"])] == [("space", 500, Space()), ("uni0628.fina", 500, Sign())]


def test_records_and_outlines_read(arabic: Path) -> None:
    """Units per em, records, metrics, and quadratic contours with off-curve controls carried from the binary."""
    source = _source(arabic)
    beh = source.glyph("uni0628").map(lambda glyph: glyph.contours[0]).default_value(())
    assert (source.upm, source.info.vendor, source.info.designer, source.metrics.win_ascent if source.metrics else 0, [point.on for point in beh]) == (
        1000,
        "TEST",
        "Test designer",
        900,
        [True, True, False, True, True],
    )


def test_ornament_faces_keep_original_names(arabic: Path) -> None:
    """Ornament import keeps every inked glyph under its original name, the space encoded, no layout read."""
    source = _source(arabic, OrnamentFace(style="Regular", original="Arabic-Regular.ttf", family="Ornaments"))
    roles = {glyph.name.text: glyph.role for glyph in source.glyphs}
    assert (roles["beh"], roles["space"], roles[".notdef"], source.lookups, source.encoded) == (Ornament(), Space(), Sign(), (), frozendict({0x0020: "space"}))


def test_unreadable_binary_faults(tmp_path: Path) -> None:
    """Bytes no font parser accepts fail with one `UNREADABLE` fault naming the path."""
    (path := tmp_path / "broken.ttf").write_bytes(b"not a font")
    match imported(path, FACE):
        case Result(error=(fault,)):
            assert (fault.reason, fault.subject) == (Reason.UNREADABLE, str(path))
        case outcome:
            raise AssertionError(outcome)


def test_role_covers_digits_and_controls() -> None:
    """Digits take their script from the Unicode name, format controls the control role."""
    assert [role(GlyphName(codes=(code,))) for code in (0x06F4, 0x0664, 0x0034, 0x200C)] == [Digit(script="farsi"), Digit(script="arab"), Digit(script="latn"), Control()]


def test_source_replaces_and_removes_glyphs(arabic: Path) -> None:
    """Replacement keeps glyph order, a new key appends, removal drops by key."""
    source = _source(arabic)
    wider = Glyph(name=GlyphName(codes=(0x0628,)), role=role(GlyphName(codes=(0x0628,))), advance=900)
    added = Glyph(name=GlyphName(codes=(0x06CC,)), role=role(GlyphName(codes=(0x06CC,))), advance=400)
    changed = source.replaced(wider, added).without("uni0650")
    assert (changed.named["uni0628"].advance, changed.glyphs[-1].name.text, "uni0650" in changed.named, len(changed.by_role(Mark))) == (900, "uni06CC", False, 1)


def test_genre_coverage_drops_excluded_codes(arabic: Path) -> None:
    """Code points a declared genre coverage excludes leave the cmap, their glyphs kept where another code reaches them."""
    face = Face(style="Regular", original="Arabic-Regular.ttf", family="Test", genre=Kufi(coverage=Coverage(excluded=(Excluded(codes=frozenset({0x0644}), reason="Test"),))))
    assert (0x0644 in (narrowed := _source(arabic, face)).encoded, "uni0644" in narrowed.named, 0x0644 in _source(arabic).encoded) == (False, False, True)
