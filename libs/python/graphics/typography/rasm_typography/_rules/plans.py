"""Ordered face plans with rules resolved from face values, genre, and script."""

lazy import unicodedata

from rasm_typography._rules.anchors import Anchors
from rasm_typography._rules.digits import Digits
from rasm_typography._rules.fits import Outlines
from rasm_typography._rules.joins import Joins
from rasm_typography._rules.locale import Locale
from rasm_typography._rules.marks import Marks, QuranicMarks, WordSigns
from rasm_typography._rules.metrics import Metrics, Records
from rasm_typography._rules.ornaments import Ornaments
from rasm_typography._rules.scale import Scale
from rasm_typography._rules.spacing import Spacing
lazy from rasm_typography._faces.declaration import Advance, Face, Faces, OrnamentFace, Sheared

# --- [TYPES] ----------------------------------------------------------------------------

type Rules = Ornaments | Locale | Digits | Outlines | Marks | QuranicMarks | Joins | Scale | Anchors | Spacing | WordSigns | Records

# --- [OPERATIONS] -----------------------------------------------------------------------


def planned(face: Faces, /) -> tuple[Rules | Metrics, ...]:
    """Returns a face's plan, each value the face's override or its genre's, each code point its script's, its slant from its shear."""
    match face:
        case Face(genre=genre, values=values, treatments=treatments):
            script, stack = genre.script, values.stack_gap if values.stack_gap is not None else genre.stack_gap
            return (
                Locale(script=script, coverage=genre.coverage),
                Digits(script=script, zero=values.zero, advance=values.digit_advance or Advance.TABULAR, bearing=values.digit_bearing),
                Outlines(floor=values.nuqta_floor if values.nuqta_floor is not None else genre.nuqta_floor, spurs=genre.spurs, opened=values.counter, letters=script.counters, dot=script.dotted),
                Marks(script=script, band=genre.mark, ratio=values.mark, floor=genre.mark_floor, drawn=values.marks_drawn, angle=values.fatha_angle, length=values.fatha_length, stack=stack),
                QuranicMarks(codes=frozenset(code for code in genre.coverage.required if unicodedata.category(chr(code)) == "Mn" and code not in script.recipes), script=script),
                Joins(overlap=genre.join_overlap, kashida=script.kashida),
                Scale(script=script, height=values.alif if values.alif is not None else genre.alif, cap=genre.pitch[1]),
                Anchors(script=script, stack=stack, clearance=genre.clearance),
                Spacing(script=script, ratio=values.word_ratio or genre.word_ratio, band=genre.word_band),
                WordSigns(script=script, stack=stack),
                Metrics(pitch=genre.pitch, stack=stack),
                Records(lowest=values.lowest_ppem, letters=script.counters, stem=script.stem, slant=next((treatment.angle for treatment in treatments if isinstance(treatment, Sheared)), 0.0)),
            )
        case OrnamentFace(first=first):
            return (Ornaments(first=first), Outlines(sliver=0.001, snap=0.006), Metrics(pitch=None, stack=0.0), Records())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Rules", "planned"]
