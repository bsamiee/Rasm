"""Horizontal metrics of the `Spacing` rebuild, its kerning across ligatures, tatweels, and vowels, and the vertical metrics `Metrics` measures for an ornament face."""

import functools
from io import BytesIO

from fontTools.otlLib.builder import LOOKUP_FLAG_IGNORE_MARKS
from fontTools.ttLib import TTFont
import msgspec

from rasm.testing import settled
from rasm.typography import Anchor, Binary, Corpus, Face, imported, Joining, Metrics, OrnamentFace, Pairs, planned, Source, Spacing, TextNaskh

# --- [CONSTANTS] ------------------------------------------------------------------------

NASKH = Face(family="Probe", style="Regular", genre=TextNaskh())

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_spacing_kerns_a_ligature_ending_a_part_across_its_marks(font: functools.partial[bytes], rebuilt: functools.partial[Source]) -> None:
    """A lam-alef ligature reaching under the next part is kerned the floor clear of it, the pair matching across marks typed between."""
    glyphs = {
        "uni0627": (300, ((100, 0, 200, 700),)),
        "uni0644": (300, ((0, 0, 300, 700),)),
        "uni0644_uni0627": (600, ((-150, -100, 600, 0), (100, 0, 200, 700), (400, 0, 500, 700))),
        "uni0628": (600, ((0, 0, 600, 150),)),
        "uni0628.init": (600, ((0, 0, 600, 150),)),
    }
    features = "feature init { sub uni0628 by uni0628.init; } init;\nfeature rlig { sub uni0644 uni0627 by uni0644_uni0627; } rlig;"
    rule = next(rule for rule in planned(NASKH) if isinstance(rule, Spacing))
    kern = next(lookup for lookup in rebuilt(NASKH, font(glyphs, {0x0627: "uni0627", 0x0628: "uni0628", 0x0644: "uni0644"}, features), Spacing).lookups if isinstance(lookup, Pairs))
    assert kern.flags & LOOKUP_FLAG_IGNORE_MARKS
    assert kern.values["uni0644_uni0627", "uni0628.init"].advance >= 150 + rule.floor * 1000


def test_spacing_never_kerns_a_tatweel(font: functools.partial[bytes], rebuilt: functools.partial[Source]) -> None:
    """A tatweel joins on both sides, so a kashida run stays one stroke and no pair parts it."""
    glyphs = {"uni0627": (300, ((100, 0, 200, 700),)), "uni0628": (600, ((0, 0, 600, 150),)), "uni0640": (300, ((-10, 0, 310, 100),))}
    joinings = frozendict({0x0640: (Joining.CAUSING, "No_Joining_Group"), 0x0628: (Joining.DUAL, "BEH")})
    source = rebuilt(NASKH, font(glyphs, {0x0627: "uni0627", 0x0628: "uni0628", 0x0640: "uni0640"}), Spacing, joinings=joinings)
    assert not any("uni0640" in pair for lookup in source.lookups if isinstance(lookup, Pairs) for pair in lookup.values)


def test_spacing_tightens_no_pair_into_the_vowels_its_letters_carry(font: functools.partial[bytes], corpus: Corpus) -> None:
    """A loose waw before qaf tightens toward the rhythm alef sets only until a kasra under the qaf would meet the waw's descender."""
    glyphs = {"uni0627": (300, ((50, 0, 150, 700),)), "uni0648": (400, ((100, 0, 400, 300), (-200, -300, 400, -200))), "uni0642": (500, ((0, 0, 500, 300),)), "uni0650": (0, ((-60, -40, 60, 0),))}
    data = font(glyphs, {0x0627: "uni0627", 0x0648: "uni0648", 0x0642: "uni0642", 0x0650: "uni0650"})
    rule = next(rule for rule in planned(NASKH) if isinstance(rule, Spacing))
    held = settled(imported(data, NASKH, frozendict({0x0627: (Joining.RIGHT, "ALEF"), 0x0648: (Joining.RIGHT, "WAW"), 0x0642: (Joining.DUAL, "QAF")})))
    anchored = held.replaced(
        msgspec.structs.replace(held.named["uni0642"], anchors=(Anchor(name="bottom", x=450.0, y=-150.0),)),
        msgspec.structs.replace(held.named["uni0650"], anchors=(Anchor(name="_bottom", x=0.0, y=0.0),)),
    )
    bare, voweled = (
        next(lookup for lookup in settled(rule.applied(source, original=Binary(data=data), corpus=corpus)).lookups if isinstance(lookup, Pairs)).values.get(("uni0648", "uni0642"))
        for source in (held, anchored)
    )
    assert bare is not None
    assert bare.advance < 0
    assert voweled is None or voweled.advance > bare.advance


def test_ornament_win_metrics_cover_its_art_alone(font: functools.partial[bytes], corpus: Corpus) -> None:
    """An ornament face's win metrics reach its own art and line, no corpus run shaping to glyph 0 stacking marks past it, as Tazhib's 3732 units over its 906 did."""
    data = font({"uniE000": (1000, ((0, -200, 1000, 900),))}, {0xE000: "uniE000"})
    face = OrnamentFace(family="Probe", style="Regular")
    rule = next(rule for rule in planned(face) if isinstance(rule, Metrics))
    stacked = msgspec.structs.replace(corpus, words=("\N{ZERO WIDTH JOINER}یّٰٓ\N{ZERO WIDTH JOINER}", "ءٍّۭ"))
    vertical = rule.measured(settled(imported(data, face, frozendict())), corpus=stacked, binary=Binary(data=TTFont(BytesIO(data))))
    assert (vertical.win_ascent, vertical.win_descent) == (900, 300)
