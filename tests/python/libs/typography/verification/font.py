"""A native font with independent geometric perturbations for verification tests."""

from tests.python.libs.typography.fonts import encoded, rectangles
from typography.shaping.runs import Binary

# --- [OPERATIONS] -----------------------------------------------------------------------


def built(
    *,
    overlap: int = 20,
    fatha: int = 200,
    two: tuple[tuple[int, int, int, int], ...] = ((100, 0, 160, 500), (100, 440, 350, 500)),
    ligature: bool = True,
    dot: tuple[int, int, int, int] = (180, -150, 240, -90),
    damma: int = 0,
    damma_far: int = 0,
    stack: int = 100,
    kern: int = 0,
    win: int = 1200,
) -> Binary:
    """Builds a font with each parameter changing one independent geometric fact."""
    advance = 450
    glyphs = {
        ".notdef": (advance, rectangles((50, 0, 400, 700))),
        "space": (advance, rectangles()),
        **dict.fromkeys(("uni0628", "uni0628.init", "uni0628.medi", "uni0628.fina"), (advance, rectangles((-overlap, 0, advance + overlap, 100), dot))),
        **dict.fromkeys(("uni0627", "uni0627.fina", "uni0644", "uni0644.init"), (advance, rectangles((200, 0, 260, 700)))),
        "uni0644_uni0627.fina": (advance, rectangles(*(((100, 0, 160, 700), (300, 0, 360, 700)) if ligature else ()))),
        "uni064E": (0, rectangles((-100, 0, 100, 40))),
        "uni064F": (0, rectangles((-80, damma, 80, damma + 40))),
        "uni064F.far": (0, rectangles((-80, damma_far, 80, damma_far + 40))),
        "uni06F1": (advance, rectangles((100, 0, 160, 500))),
        "uni06F2": (advance, rectangles(*two)),
    }
    features = f"""
    languagesystem DFLT dflt;
    languagesystem arab dflt;
    languagesystem arab FAR;
    markClass [uni064E uni064F uni064F.far] <anchor 0 0> @TOP;
    feature locl {{ script arab; language FAR; sub uni064F by uni064F.far; }} locl;
    feature init {{ sub uni0628 by uni0628.init; sub uni0644 by uni0644.init; }} init;
    feature medi {{ sub uni0628 by uni0628.medi; }} medi;
    feature fina {{ sub uni0628 by uni0628.fina; sub uni0627 by uni0627.fina; }} fina;
    feature rlig {{ sub uni0644.init uni0627.fina by uni0644_uni0627.fina; }} rlig;
    feature kern {{ pos uni0628.init uni0628.medi {kern}; pos uni0628.medi uni0628.fina {kern}; }} kern;
    feature mark {{
        pos base [uni0628 uni0628.init uni0628.medi uni0628.fina] <anchor 225 {fatha}> mark @TOP;
        pos base [uni0627 uni0627.fina uni0644 uni0644.init] <anchor 230 800> mark @TOP;
    }} mark;
    feature mkmk {{ pos mark [uni064E uni064F uni064F.far] <anchor 0 {stack}> mark @TOP; }} mkmk;
    table GDEF {{ GlyphClassDef [uni0628 uni0628.init uni0628.medi uni0628.fina uni0627 uni0627.fina uni0644 uni0644.init uni06F1 uni06F2], [uni0644_uni0627.fina], [uni064E uni064F uni064F.far], ; }} GDEF;
    """
    return Binary(
        data=encoded(
            glyphs,
            {0x20: "space", 0x0627: "uni0627", 0x0628: "uni0628", 0x0644: "uni0644", 0x064E: "uni064E", 0x064F: "uni064F", 0x06F1: "uni06F1", 0x06F2: "uni06F2"},
            features,
            family="Verification Test",
            ascent=1100,
            descent=-800,
            win_ascent=win,
            win_descent=900,
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["built"]
