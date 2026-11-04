"""Ordered face plans with rules resolved from face values, genre, and script, and the fold applying a plan to a source."""

lazy from functools import reduce
lazy import unicodedata

import msgspec

from typography.rules.anchors import Anchors
from typography.rules.digits import Digits
from typography.rules.fits import Outlines
from typography.rules.joins import Joins
from typography.rules.locale import Locale
from typography.rules.marks import Marks, QuranicMarks, WordSigns
from typography.rules.metrics import Metrics, Records
from typography.rules.ornaments import Ornaments
from typography.rules.scale import Scale
from typography.rules.spacing import Spacing
lazy from typography.faces.declaration import Face, Faces, OrnamentFace, Sheared
lazy from typography.faces.faults import Faults, Ok, Outcome
lazy from typography.faces.glyphs import Source
lazy from typography.rules import anchors, digits, fits, joins, locale, marks, metrics, ornaments, scale, spacing
lazy from typography.shaping.corpus import Corpus
lazy from typography.shaping.runs import Binary

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
                Digits(script=script, zero=values.zero, advance=values.digit_advance or "tabular", bearing=values.digit_bearing),
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


def applied(source: Source, rule: Rules, /, *, original: Binary, corpus: Corpus) -> Outcome[Source]:
    """Applies one rule to a source, measuring the original where the source cannot shape yet."""
    match rule:
        case Ornaments():
            return Ok(ornaments.encoded(source, rule))
        case Locale():
            return Ok(locale.localized(source, rule))
        case Digits():
            return Ok(digits.figured(source, rule))
        case Outlines():
            return Ok(fits.fitted(source, rule))
        case Marks():
            return marks.penned(source, rule)
        case QuranicMarks():
            return marks.quranic(source, rule, original=original)
        case Joins():
            return joins.joined(source, rule)
        case Scale():
            return scale.scaled(source, rule, original=original, corpus=corpus)
        case Anchors():
            return anchors.anchored(source, rule, original=original, corpus=corpus)
        case Spacing():
            return spacing.spaced(source, rule, original=original, corpus=corpus)
        case WordSigns():
            return marks.signed(source, rule, original=original)
        case Records():
            return Ok(metrics.recorded(source, rule))


def _named(rule: str, faults: Faults, /) -> Faults:
    """Prefixes each fault's member with the rule that refused."""
    head, *rest = (msgspec.structs.replace(fault, member=f"{rule} {fault.member}".strip()) for fault in faults)
    return (head, *rest)


def executed(source: Source, plan: tuple[Rules | Metrics, ...], /, *, original: Binary, corpus: Corpus) -> Outcome[Source]:
    """Folds drawing rules over a source, leaving native layout metrics to font construction, the first refused rule naming itself on its faults."""
    initial: Outcome[Source] = Ok(source)
    return reduce(
        lambda held, rule: held.bind(lambda current: applied(current, rule, original=original, corpus=corpus).map_error(lambda faults: _named(type(rule).__name__.lower(), faults))),
        (rule for rule in plan if not isinstance(rule, Metrics)),
        initial,
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Rules", "applied", "executed", "planned"]
