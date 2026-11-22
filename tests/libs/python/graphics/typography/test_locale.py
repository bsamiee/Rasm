"""Signs the `Locale` rebuild derives from a face's own glyphs, and the madda it seats over an alef stem."""

from collections.abc import Callable
import functools

from rasm.typography import DotMatrix, Face, Locale, pieces, Source, TextNaskh

# --- [CONSTANTS] ------------------------------------------------------------------------

NASKH = Face(family="Probe", style="Regular", genre=TextNaskh())

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_locale_derives_the_six_signs_persian_dimensions_need(
    font: functools.partial[bytes], rebuilt: functools.partial[Source], outers: Callable[[Source, str], tuple[tuple[float, float], ...]]
) -> None:
    """Plus-minus, almost-equal, not-equal, section, prime, and double prime come from the face's own glyphs, plus-minus centred on the plus."""
    glyphs = {
        "plus": (600, ((260, 100, 340, 500), (100, 260, 500, 340))),
        "minus": (600, ((100, 260, 500, 340),)),
        "asciitilde": (600, ((100, 280, 500, 340),)),
        "equal": (600, ((100, 200, 500, 260), (100, 340, 500, 400))),
        "slash": (400, ((150, -100, 250, 700),)),
        "S": (500, ((100, 0, 400, 80), (100, 0, 180, 350), (100, 310, 400, 390), (320, 310, 400, 700), (100, 620, 400, 700))),
        "quotesingle": (200, ((70, 500, 130, 700),)),
    }
    cmap = {0x002B: "plus", 0x2212: "minus", 0x007E: "asciitilde", 0x003D: "equal", 0x002F: "slash", 0x0053: "S", 0x0027: "quotesingle"}
    source = rebuilt(NASKH, font(glyphs, cmap), Locale)
    assert {0x00B1, 0x00A7, 0x2032, 0x2033, 0x2248, 0x2260} <= source.encoded.keys()
    plus, plusminus = (source.bounds(source.encoded[code]) for code in (0x002B, 0x00B1))
    assert plus is not None
    assert plusminus is not None
    assert abs((plus[1] + plus[3]) / 2 - (plusminus[1] + plusminus[3]) / 2) <= 1
    assert len(outers(source, source.encoded[0x2033])) == 2


def test_locale_seats_a_strayed_madda_over_the_alef(font: functools.partial[bytes], rebuilt: functools.partial[Source]) -> None:
    """A madda drawn right of the alef, over the letter after it, is centred on the stem."""
    glyphs = {"uni0627": (300, ((100, 0, 160, 700),)), "uni0622": (600, ((100, 0, 160, 700), (300, 760, 500, 800)))}
    source = rebuilt(Face(family="Probe", style="Regular", genre=DotMatrix()), font(glyphs, {0x0627: "uni0627", 0x0622: "uni0622"}), Locale)
    alef, madda = source.bounds("uni0627"), source.bounds("uni0622")
    assert alef is not None
    assert madda is not None
    assert abs((madda[0] + madda[2]) / 2 - (alef[0] + alef[2]) / 2) <= (alef[2] - alef[0]) / 2


def test_locale_keeps_a_final_alef_madda_over_its_stem(font: functools.partial[bytes], rebuilt: functools.partial[Source]) -> None:
    """A final alef's madda over its stem stays, the stem read at its top, not across the joining stroke, as Pahna's `ـآ` keeps its madda apart."""
    glyphs = {"uni0627": (300, ((100, 0, 160, 700),)), "uni0622": (300, ((100, 0, 160, 700), (60, 760, 200, 800))), "uni0622.fina": (600, ((100, 0, 160, 700), (160, 0, 600, 60), (60, 760, 200, 800)))}
    source = rebuilt(NASKH, font(glyphs, {0x0627: "uni0627", 0x0622: "uni0622"}, "feature fina { sub uni0622 by uni0622.fina; } fina;"), Locale)
    assert len(pieces(source.drawn("uni0622.fina").contours)) == 2
    assert source.bounds("uni0622.fina") == (60.0, 0.0, 600.0, 800.0)
