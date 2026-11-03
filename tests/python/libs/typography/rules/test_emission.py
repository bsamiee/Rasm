"""Feature emission: every lookup kind written once as parseable feature text, lifts ahead of the writers' marker, and kern pairs as class groups."""

from io import StringIO

from fontTools.feaLib.parser import Parser
import msgspec

from typography.faces.glyphs import Source
from typography.faces.layout import Adjustment, Context, Contextual, Lift, Lifts, Ligatures, Pairs, Positions, Single
from typography.rules.emission import emitted

# --- [OPERATIONS] -----------------------------------------------------------------------


def _parsed(text: str, source: Source, /) -> None:
    Parser(StringIO(text), glyphNames=[glyph.name.text for glyph in source.glyphs]).parse()


def test_every_lookup_kind_parses(source: Source) -> None:
    """Each lookup record becomes a lookup its feature references, the language system registered, and the text parses."""
    lookups = (
        Single(feature="locl", languages=("arab/FAR ",), order=-1, mapping=frozendict({"uni0628": "uni0631"})),
        Single(feature="init", mapping=frozendict({"uni0628": "uni0628.init"})),
        Ligatures(feature="rlig", order=1, rules=((("uni0644.init", "uni0627.fina"), "uni0644_uni0627"),)),
        Contextual(feature="calt", order=2, rules=(Context(input=(("uni0628.init",),), lookahead=(("uni0631.fina",),), replacement=("uni0628",)),)),
        Positions(feature="kern", order=3, values=frozendict({"uni0631": Adjustment(advance=20)})),
        Lifts(feature="mark", order=4, rules=(Lift(input=(("uni0628.init",), ("uni064E",)), lookahead=(("uni0628.medi",),), adjustments=(None, Adjustment(dy=40))),)),
    )
    text = emitted(msgspec.structs.replace(source, lookups=lookups)).features
    _parsed(text, source)
    assert "languagesystem arab FAR;" in text
    assert text.index("feature mark") < text.index("# Automatic Code") > text.index("lookup mark")
    assert all(f"feature {lookup.feature}" in text for lookup in lookups)


def test_kern_pairs_become_class_groups(source: Source) -> None:
    """Firsts sharing one value row and seconds sharing one column form `public.kern1` and `public.kern2` groups with one value per class pair."""
    pairs = Pairs(feature="kern", values=frozendict({(first, second): Adjustment(dx=-30, advance=-30) for first in ("uni0631", "uni0631.fina") for second in ("uni0628", "uni0628.init")}))
    done = emitted(msgspec.structs.replace(source, lookups=(pairs,)))
    _parsed(done.features, source)
    assert done.groups == frozendict({"public.kern1.uni0631": ("uni0631", "uni0631.fina"), "public.kern2.uni0628": ("uni0628", "uni0628.init")})
    assert done.kerning == frozendict({("public.kern1.uni0631", "public.kern2.uni0628"): -30})
