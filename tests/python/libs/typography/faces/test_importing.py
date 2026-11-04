"""Native imports preserve naming, aliases, font geometry, shaping, and source updates."""

# mypy: disable-error-code="no-untyped-call"

from io import BytesIO
from pathlib import Path
import unicodedata

from expression import Result
from fontTools.feaLib import ast
from fontTools.otlLib.builder import LOOKUP_FLAG_IGNORE_MARKS
from fontTools.ttLib import TTFont, TTLibError
from hypothesis import given, strategies as st
import msgspec
from pathops import PathOp
import pytest
lazy from fontTools.fontBuilder import FontBuilder
lazy from fontTools.pens.areaPen import AreaPen
lazy from fontTools.pens.cu2quPen import Cu2QuPen
lazy from fontTools.pens.t2CharStringPen import T2CharStringPen
lazy import pathops

from tests.python.libs.typography.fonts import encoded, rectangles
from typography.builds.pipeline import compiled, constructed
from typography.faces.coverage import Coverage, Excluded
from typography.faces.declaration import Face, Kufi, OrnamentFace
from typography.faces.faults import Ok, Reason
from typography.faces.glyphs import Control, Digit, Joining, Letter, Ligature, Mark, Ornament, role, Side, Sign, Source, Space
from typography.faces.importing import imported
from typography.faces.names import Form, GlyphName
from typography.outlines.measure import stroke
from typography.outlines.paths import area, Box, combined
from typography.rules.locale import Locale, localized
from typography.rules.marks import Marks, penned
from typography.rules.measures import inked
from typography.rules.metrics import Metrics
from typography.rules.plans import planned
from typography.rules.scale import Scale, scaled
from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_shared_cmap_and_glyph_order_follow_native_inputs(arabic: Path, face: Face, source: Source) -> None:
    """Shared native IDs keep every retained code point without cloning glyphs, while excluded mappings and unreached ink leave."""
    assert tuple(source.named) == (
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
    )
    assert source.encoded == frozendict({0x20: "space", 0xA0: "space", 0x0627: "uni0627", 0x0628: "uni0628", 0x0644: "uni0644", 0x064E: "uni064E", 0x0650: "uni0650", 0xFE90: "uni0628.fina"})
    assert source.named["space"].codes == frozenset({0x20, 0xA0})
    assert source.named["uni0628.fina"].codes == frozenset({0xFE90})
    assert imported(arabic.read_bytes(), face) == Ok(source)


def test_imported_domain_facts_keep_native_roles_anchors_and_records(source: Source) -> None:
    """Native inputs retain the domain classifications, attachment names and records consumed by subsequent rules."""
    named = source.named
    assert (named["uni0628.medi"].role, named["uni0650"].role, named["uni0644_uni0627"].role) == (
        Letter(joining=Joining.DUAL, form=Form.MEDI),
        Mark(side=Side.BELOW),
        Ligature(components=("uni0644.init", "uni0627.fina")),
    )
    assert (named["space"].category, named["uni0644_uni0627"].category) == ("unassigned", "ligature")
    assert tuple((anchor.name, anchor.x, anchor.y) for anchor in named["uni0628.init"].anchors) == (("bottom", 250, -200), ("top", 250, 600))
    assert tuple((anchor.name, anchor.y) for anchor in named["uni0650"].anchors) == (("_bottom", -80),)
    assert tuple((anchor.name, anchor.y) for anchor in named["uni064E"].anchors) == (("_top", 80),)
    assert tuple(anchor.name for anchor in named["uni0644_uni0627"].anchors) == ("bottom_1", "bottom_2", "top_1", "top_2")
    assert (source.upm, source.info.vendor, source.info.designer) == (1000, "TEST", "Test designer")
    assert source.metrics.win_ascent == 900
    assert tuple(point.on for point in named["uni0628"].contours[0]) == (True, True, False, True, True)


def test_imported_layout_recompiles_to_equivalent_native_placements(arabic: Path, source: Source, sample: Corpus) -> None:
    """Renamed forms, ligatures, and mark anchors retain their outlines and pen positions after compilation."""
    before = Binary.read(arabic).default_with(lambda faults: pytest.fail(str(faults)))
    after = Binary(data=constructed(source, Metrics(pitch=None, stack=0), sample).bind(lambda product: compiled(product[0], product[2])).default_with(lambda faults: pytest.fail(str(faults))))
    assert after.encoded[0x20] == after.encoded[0xA0]
    assert after.encoded[0xFE90] == after.shaped("بب").names[0]
    for text in ("ببب", "بَ", "بِ", "لا", "لَا"):
        original, rebuilt = before.shaped(text), after.shaped(text)
        assert tuple((glyph.x, glyph.y, glyph.advance) for glyph in rebuilt.placed) == tuple((glyph.x, glyph.y, glyph.advance) for glyph in original.placed)
        for a, b in zip(before.drawn(original), after.drawn(rebuilt), strict=True):
            assert area(combined(a, b, PathOp.XOR)) == pytest.approx(0, abs=0.01)


def test_imported_lookup_stages_preserve_native_substitution_order(face: Face, sample: Corpus) -> None:
    """Two ordered lookups in one feature both execute after the imported source is compiled."""
    data = encoded(
        dict.fromkeys((".notdef", "beh", "alef", "lam"), (500, rectangles((0, 0, 100, 100)))),
        {0x0628: "beh", 0x0627: "alef", 0x0644: "lam"},
        "languagesystem arab dflt; lookup First { sub beh by alef; } First; lookup Second { sub alef by lam; } Second; feature ss01 { lookup First; lookup Second; } ss01;",
    )
    original = Binary(data=data)
    source = imported(data, face).default_with(lambda faults: pytest.fail(str(faults)))
    rebuilt = Binary(data=constructed(source, Metrics(pitch=None, stack=0), sample).bind(lambda product: compiled(product[0], product[2])).default_with(lambda faults: pytest.fail(str(faults))))
    assert original.shaped("ب", features={"ss01": True}).names == (original.encoded[0x0644],)
    assert rebuilt.shaped("ب", features={"ss01": True}).names == (rebuilt.encoded[0x0644],)


@pytest.mark.parametrize("tail", ["A B C", "A B"])
def test_native_ligature_priority_survives_prefix_and_duplicate_inputs(face: Face, sample: Corpus, tail: str) -> None:
    """A first matching native subtable wins over a longer sequence or a duplicate input in a later subtable."""
    data = encoded(
        {**dict.fromkeys((".notdef", "A", "B", "C"), (500, rectangles((0, 0, 100, 100)))), "short": (400, rectangles((0, 0, 200, 200))), "long": (700, rectangles((0, 0, 300, 300)))},
        {ord(character): character for character in "ABC"},
        f"languagesystem latn dflt; lookup First {{ sub A B by short; }} First; lookup Later {{ sub {tail} by long; }} Later; feature ss01 {{ lookup First; }} ss01; feature ss02 {{ lookup Later; }} ss02;",
    )
    with TTFont(BytesIO(data)) as font:
        lookups = font["GSUB"].table.LookupList.Lookup
        lookups[0].SubTable.extend(lookups[1].SubTable)
        stream = BytesIO()
        font.save(stream)
    original = Binary(data=stream.getvalue())
    source = imported(original.data, face).default_with(lambda faults: pytest.fail(str(faults)))
    rebuilt = Binary(data=constructed(source, Metrics(pitch=None, stack=0), sample).bind(lambda product: compiled(product[0], product[2])).default_with(lambda faults: pytest.fail(str(faults))))
    before, after = (binary.shaped("ABC", features={"ss01": True}) for binary in (original, rebuilt))
    assert before.width == after.width == 900
    assert len(before.placed) == len(after.placed) == 2
    assert tuple(glyph.advance for glyph in before.placed) == tuple(glyph.advance for glyph in after.placed) == (400, 500)


def test_shared_native_lookup_applies_once_through_locale(face: Face, sample: Corpus) -> None:
    """Two enabled features referring to one native lookup execute it once after Locale preserves its associations."""
    data = encoded(
        dict.fromkeys((".notdef", "A", "B", "C"), (500, rectangles((0, 0, 100, 100)))),
        {ord(character): character for character in "ABC"},
        "languagesystem latn dflt; lookup Shared { sub A by B; sub B by C; } Shared; feature ss01 { lookup Shared; } ss01; feature ss02 { lookup Shared; } ss02;",
    )
    original = Binary(data=data)
    source = imported(data, face).default_with(lambda faults: pytest.fail(str(faults)))
    source = localized(
        source, Locale(script=msgspec.structs.replace(face.genre.script, derived=(), locl=frozendict(), rlig=frozendict()), coverage=Coverage(required=frozenset(), excluded=(), blocks=frozenset()))
    )
    rebuilt = Binary(data=constructed(source, Metrics(pitch=None, stack=0), sample).bind(lambda product: compiled(product[0], product[2])).default_with(lambda faults: pytest.fail(str(faults))))
    for binary in (original, rebuilt):
        assert binary.shaped("A", features={"ss01": True, "ss02": True}).names == (binary.encoded[ord("B")],)


@pytest.mark.parametrize("parts", [("A",), ("A", "B")])
def test_native_language_scopes_and_required_features_survive_import(face: Face, sample: Corpus, parts: tuple[str, ...]) -> None:
    """Default and Persian feature records keep separate lookups, and a native required feature remains active when disabled."""
    text, inputs = "".join(parts), " ".join(parts)
    required_parts = ("E", *parts[1:])
    data = encoded(
        dict.fromkeys((".notdef", *"ABCDE"), (500, rectangles((0, 0, 100, 100)))),
        {ord(character): character for character in "ABCDE"},
        "languagesystem latn dflt; languagesystem latn FAR; "
        f"lookup Default {{ sub {inputs} by C; }} Default; lookup Persian {{ sub {inputs} by D; }} Persian; "
        f"feature ss01 {{ script latn; language dflt; lookup Default; {ast.LanguageStatement('FAR ', include_default=False).asFea()} lookup Persian; }} ss01; "
        f"feature ss02 {{ script latn; {ast.LanguageStatement('FAR ', include_default=False, required=True).asFea()} sub {' '.join(required_parts)} by B; }} ss02;",
    )
    original = Binary(data=data)
    source = imported(data, face).default_with(lambda faults: pytest.fail(str(faults)))
    rebuilt = Binary(data=constructed(source, Metrics(pitch=None, stack=0), sample).bind(lambda product: compiled(product[0], product[2])).default_with(lambda faults: pytest.fail(str(faults))))
    for binary in (original, rebuilt):
        assert binary.shaped(text, language="en", features={"ss01": True, "ss02": False}).names == (binary.encoded[ord("C")],)
        assert binary.shaped(text, language="fa", features={"ss01": True, "ss02": False}).names == (binary.encoded[ord("D")],)
        assert binary.shaped("".join(required_parts), language="fa", features={"ss01": False, "ss02": False}).names == (binary.encoded[ord("B")],)
        assert binary.shaped("".join(required_parts), language="en", features={"ss01": False, "ss02": False}).names == tuple(binary.encoded[ord(part)] for part in required_parts)


@pytest.mark.parametrize(
    "qualifier",
    [
        ast.LookupFlagStatement(LOOKUP_FLAG_IGNORE_MARKS),
        ast.LookupFlagStatement(markAttachment=ast.GlyphClass([ast.GlyphName("fatha")])),
        ast.LookupFlagStatement(markFilteringSet=ast.GlyphClass([ast.GlyphName("fatha")])),
    ],
)
def test_native_mark_flags_retain_attachment_and_filtering_membership(face: Face, sample: Corpus, qualifier: ast.LookupFlagStatement) -> None:
    """Native ignore, attachment-class, and filtering-set qualifiers retain their distinct behavior over Arabic marks."""
    data = encoded(
        {**dict.fromkeys((".notdef", "lam", "alef", "lig"), (500, rectangles((0, 0, 100, 100)))), **dict.fromkeys(("fatha", "kasra"), (0, rectangles((0, 120, 50, 140))))},
        {0x0644: "lam", 0x0627: "alef", 0x064E: "fatha", 0x0650: "kasra"},
        f"languagesystem arab dflt; feature rlig {{ {qualifier.asFea()} sub lam alef by lig; }} rlig; table GDEF {{ GlyphClassDef [lam alef], [lig], [fatha kasra], ; }} GDEF;",
    )
    original = Binary(data=data)
    source = imported(data, face).default_with(lambda faults: pytest.fail(str(faults)))
    rebuilt = Binary(data=constructed(source, Metrics(pitch=None, stack=0), sample).bind(lambda product: compiled(product[0], product[2])).default_with(lambda faults: pytest.fail(str(faults))))
    for mark in ("َ", "ِ"):
        before, after = (binary.shaped(f"\N{ARABIC LETTER LAM}{mark}\N{ARABIC LETTER ALEF}") for binary in (original, rebuilt))
        assert len(before.placed) == len(after.placed) == (2 if mark == "ِ" or qualifier.value == LOOKUP_FLAG_IGNORE_MARKS else 3)
        assert tuple(glyph.advance for glyph in before.placed) == tuple(glyph.advance for glyph in after.placed)


def test_unencoded_stylistic_alternate_retains_native_selection_and_ink(face: Face, sample: Corpus) -> None:
    """An unencoded ss01 target remains reachable and keeps its distinct drawing after import and native compilation."""
    data = encoded(
        {".notdef": (500, rectangles()), "beh": (500, rectangles((0, 0, 100, 100))), "beh.alt": (500, rectangles((0, 0, 200, 300)))},
        {0x0628: "beh"},
        "languagesystem arab dflt; feature ss01 { sub beh by beh.alt; } ss01;",
    )
    original = Binary(data=data)
    source = imported(data, face).default_with(lambda faults: pytest.fail(str(faults)))
    rebuilt = Binary(data=constructed(source, Metrics(pitch=None, stack=0), sample).bind(lambda product: compiled(product[0], product[2])).default_with(lambda faults: pytest.fail(str(faults))))
    normal, alternate = rebuilt.shaped("ب", features={"ss01": False}), rebuilt.shaped("ب", features={"ss01": True})
    assert alternate.names[0] not in rebuilt.encoded.values()
    assert area(tuple(*ink for ink in rebuilt.drawn(normal))) == 10000
    assert area(tuple(*ink for ink in rebuilt.drawn(alternate))) == 60000
    assert area(tuple(*ink for ink in original.drawn(original.shaped("ب", features={"ss01": True})))) == 60000


def test_shared_latin_and_arabic_cmap_keeps_one_native_glyph_through_rules(face: Face, sample: Corpus) -> None:
    """Shared native IDs retain both cmap entries, Arabic semantic identity, mark measurements and substitutions through scaling."""
    data = encoded(
        {
            ".notdef": (500, rectangles()),
            "shared": (500, rectangles((0, 0, 100, 100))),
            "lam": (500, rectangles((0, 0, 100, 100))),
            "fatha": (0, rectangles((0, 0, 80, 20))),
            "kasra": (0, rectangles((0, -20, 80, 0))),
        },
        {0x41: "shared", 0x0627: "shared", 0x0644: "lam", 0x064E: "fatha", 0x0650: "kasra"},
        "languagesystem DFLT dflt; languagesystem latn dflt; languagesystem arab dflt; feature ss01 { sub shared by lam; } ss01;",
    )
    original = Binary(data=data)
    source = imported(data, face).default_with(lambda faults: pytest.fail(str(faults)))
    stem, box = inked(source, 0x0627).default_with(lambda faults: pytest.fail(str(faults)))
    latin, latin_box = inked(source, 0x41).default_with(lambda faults: pytest.fail(str(faults)))
    assert latin is stem
    assert latin_box == box == Box(xmin=0, ymin=0, xmax=100, ymax=100)
    assert stem.codes == frozenset({0x41, 0x0627})
    assert stem.name.codes == (0x0627,)
    mark_rule = next(rule for rule in planned(face) if isinstance(rule, Marks))
    mark_rule = msgspec.structs.replace(mark_rule, ratio=mark_rule.band[1], script=msgspec.structs.replace(mark_rule.script, recipes=frozendict({0x064E: face.genre.script.recipes[0x064E]})))
    marked = penned(source, mark_rule).default_with(lambda faults: pytest.fail(str(faults)))
    assert stroke(marked.drawn(marked.key(0x064E)).contours) > stroke(source.drawn(source.key(0x064E)).contours)
    resized = scaled(marked, Scale(script=face.genre.script, height=0.05, cap=4), original=original, corpus=sample).default_with(lambda faults: pytest.fail(str(faults)))
    assert resized.bounds(resized.key(0x0627)) == pytest.approx((0, 0, 102.4, 102.4), rel=1e-9, abs=0)
    rebuilt = Binary(data=constructed(resized, Metrics(pitch=None, stack=0), sample).bind(lambda product: compiled(product[0], product[2])).default_with(lambda faults: pytest.fail(str(faults))))
    assert rebuilt.encoded[0x41] == rebuilt.encoded[0x0627]
    for code in stem.codes:
        text = chr(code)
        assert original.shaped(text, features={"ss01": True}).names == (original.encoded[0x0644],)
        assert rebuilt.shaped(text, features={"ss01": True}).names == (rebuilt.encoded[0x0644],)


@pytest.mark.parametrize("presentation,nominal", [(0xFE8F, True), (0xFE90, False)])
def test_presentation_only_cmap_encodes_an_existing_isolated_glyph(face: Face, sample: Corpus, presentation: int, *, nominal: bool) -> None:
    """An isolated presentation mapping gains its nominal code on one native glyph, while a final form keeps only its presentation code."""
    data = encoded({".notdef": (500, rectangles()), "beh": (640, rectangles((0, 0, 120, 220)))}, {presentation: "beh"}, "")
    original = Binary(data=data)
    source = imported(data, face).default_with(lambda faults: pytest.fail(str(faults)))
    glyph = source.named[source.encoded[presentation]]
    localized_source = localized(
        source,
        Locale(
            script=msgspec.structs.replace(face.genre.script, derived=(), locl=frozendict(), rlig=frozendict()),
            coverage=Coverage(required=frozenset({0x0628, presentation}), excluded=(), blocks=frozenset()),
        ),
    )
    assert localized_source.named.keys() == source.named.keys()
    assert localized_source.named[glyph.name.text].codes == (glyph.codes | {0x0628} if nominal else glyph.codes)
    assert localized_source.named[glyph.name.text].contours == glyph.contours
    assert localized_source.named[glyph.name.text].advance == glyph.advance
    rebuilt = Binary(data=constructed(localized_source, Metrics(pitch=None, stack=0), sample).default_with(lambda faults: pytest.fail(str(faults)))[2])
    assert len(rebuilt.order) == len(original.order)
    assert (0x0628 in rebuilt.encoded) is nominal
    assert area(combined(original.outline(original.encoded[presentation]), rebuilt.outline(rebuilt.encoded[presentation]), PathOp.XOR)) == pytest.approx(0, abs=0.01)
    if nominal:
        assert rebuilt.encoded[0x0628] == rebuilt.encoded[presentation]
        assert rebuilt.shaped("ب").names == (rebuilt.encoded[presentation],)


def test_ornament_import_keeps_original_names_and_unencoded_ink(arabic: Path) -> None:
    """Art imports retain every native name and drawing while spacing aliases choose the lowest code point."""
    source = imported(arabic, OrnamentFace(style="Regular", original=arabic.name, family="Ornaments")).default_with(lambda faults: pytest.fail(str(faults)))
    with TTFont(arabic) as native:
        assert source.named.keys() == set(native.getGlyphOrder())
        assert source.encoded == native.getBestCmap()  # type: ignore[comparison-overlap]
    assert source.named["orphan"].role == Ornament()
    assert source.named["orphan"].contours
    assert (source.named["space"].role, source.named[".notdef"].role, source.lookups) == (Space(), Sign(), ())


@pytest.mark.parametrize("data", [b"not a font", None])
def test_native_input_failures_preserve_path_and_typed_cause(tmp_path: Path, data: bytes | None) -> None:
    """Unreadable files and rejected font bytes keep their original native exceptions."""
    path = tmp_path / "broken.ttf"
    if data is not None:
        path.write_bytes(data)
    match imported(path, Face(style="Regular", original=path.name)):
        case Result(error=(fault,)):
            assert (fault.reason, fault.subject) == (Reason.UNREADABLE, str(path))
            assert isinstance(fault.cause, OSError if data is None else TTLibError)
        case outcome:
            pytest.fail(str(outcome))


def test_cff_outlines_use_the_native_glyph_set(tmp_path: Path) -> None:
    """Cubic CFF outlines import as quadratic contours through native pens."""
    builder = FontBuilder(1000, isTTF=False)
    builder.setupGlyphOrder([".notdef", "A"])
    builder.setupCharacterMap({0x41: "A"})
    pens = {name: T2CharStringPen(500, None) for name in (".notdef", "A")}
    pen = pens["A"]
    pen.moveTo((0, 0))
    pen.curveTo((0, 200), (200, 200), (200, 0))
    pen.closePath()
    builder.setupCFF("Test-Regular", {"FullName": "Test Regular", "FamilyName": "Test", "Weight": "Regular"}, {name: held.getCharString() for name, held in pens.items()}, {})
    builder.setupHorizontalMetrics(dict.fromkeys(pens, (500, 0)))
    builder.setupHorizontalHeader(ascent=800, descent=-200)
    builder.setupNameTable({"familyName": "Test", "styleName": "Regular"})
    builder.setupOS2()
    builder.setupPost()
    builder.save(path := tmp_path / "Test-Regular.otf")
    source = imported(path, Face(style="Regular", original=path.name)).default_with(lambda faults: pytest.fail(str(faults)))
    assert any(not point.on for contour in source.named["A"].contours for point in contour)
    with TTFont(path) as native:
        glyphs = native.getGlyphSet()
        cubic, expected = AreaPen(glyphs), pathops.Path()
        glyphs["A"].draw(cubic)
        glyphs["A"].draw(Cu2QuPen(expected.getPen(), max_err=1.0, all_quadratic=True))
    assert abs(cubic.value) == 24000
    assert area(source.named["A"].contours) == pytest.approx(pathops.simplify(expected, fix_winding=True, keep_starting_points=False, clockwise=False).area, rel=1e-9, abs=0)


@given(character=st.characters(categories=("Nd", "Cf", "Cc")))
def test_unicode_categories_keep_control_and_decimal_roles(character: str) -> None:
    """Native categories select controls, and every decimal script retains its own zero."""
    match role(GlyphName(stem=(ord(character),))):
        case Digit(zero=zero):
            assert unicodedata.category(character) == "Nd"
            assert unicodedata.decimal(chr(zero)) == 0
            assert ord(character) - zero == unicodedata.decimal(character)
        case Control():
            assert unicodedata.category(character) in {"Cf", "Cc"}
        case found:
            pytest.fail(str(found))


def test_source_updates_compose_last_write_wins_and_removal(source: Source) -> None:
    """Replacement composition preserves order, appends new names, and removes selected glyphs."""
    wider = msgspec.structs.replace(source.named["uni0628"], advance=900)
    alternate = msgspec.structs.replace(wider, name=GlyphName(stem=(0x0628,), variant="ss01"), codes=frozenset(), advance=400)
    changed = source.replaced(wider, alternate)
    assert changed == source.replaced(wider).replaced(alternate)
    assert changed.replaced(wider, source.named["uni0628"]).named["uni0628"].advance == source.named["uni0628"].advance
    assert tuple(changed.named) == (*source.named, alternate.name.text)
    assert changed.without(alternate.name.text).named.keys() == source.named.keys()
    without = changed.without("uni0650")
    assert without.glyph("uni0650").is_none()
    assert without.by_role(Mark) == (changed.named["uni064E"],)


@pytest.mark.parametrize("code,glyph,retained", [(0xFE90, "uni0628.fina", True), (0x0644, "uni0644", False)])
def test_custom_coverage_removes_only_the_excluded_mapping(arabic: Path, source: Source, code: int, glyph: str, *, retained: bool) -> None:
    """Genre coverage changes cmap selection rather than erasing a glyph reached by another retained code."""
    face = Face(style="Regular", original=arabic.name, genre=Kufi(coverage=Coverage(excluded=(Excluded(codes=frozenset({code}), reason="Test exclusion"),))))
    narrowed = imported(arabic, face).default_with(lambda faults: pytest.fail(str(faults)))
    assert code in source.encoded
    assert code not in narrowed.encoded
    assert (glyph in narrowed.named) is retained
