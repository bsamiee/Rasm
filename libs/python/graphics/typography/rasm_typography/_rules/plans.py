"""Ordered face plans with rules resolved from face values, genre, and script, and the fold applying a plan to a source."""

lazy from functools import reduce
lazy import unicodedata

import msgspec

from rasm_typography._rules.anchors import _anchored, Anchors
from rasm_typography._rules.digits import _figured, Digits
from rasm_typography._rules.fits import _fitted, Outlines
from rasm_typography._rules.joins import _joined, Joins
from rasm_typography._rules.locale import _localized, Locale
from rasm_typography._rules.marks import _penned, _quranic, _signed, Marks, QuranicMarks, WordSigns
from rasm_typography._rules.metrics import _recorded, Metrics, Records
from rasm_typography._rules.ornaments import _encoded, Ornaments
from rasm_typography._rules.scale import _scaled, Scale
from rasm_typography._rules.spacing import _spaced, Spacing
lazy from rasm_records import Faults, Ok, Outcome
lazy from rasm_typography._faces.declaration import Face, Faces, OrnamentFace, Sheared
lazy from rasm_typography._faces.glyphs import _Source
lazy from rasm_typography._shaping.corpus import _Corpus
lazy from rasm_typography._shaping.runs import _Binary

# --- [TYPES] ----------------------------------------------------------------------------

type _Rules = Ornaments | Locale | Digits | Outlines | Marks | QuranicMarks | Joins | Scale | Anchors | Spacing | WordSigns | Records

# --- [OPERATIONS] -----------------------------------------------------------------------


def _planned(face: Faces, /) -> tuple[_Rules | Metrics, ...]:
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


def _applied(source: _Source, rule: _Rules, /, *, original: _Binary, corpus: _Corpus) -> Outcome[_Source]:
    """Applies one rule to a source, measuring the original where the source cannot shape yet."""
    match rule:
        case Ornaments():
            return Ok(_encoded(source, rule))
        case Locale():
            return Ok(_localized(source, rule))
        case Digits():
            return Ok(_figured(source, rule))
        case Outlines():
            return Ok(_fitted(source, rule))
        case Marks():
            return _penned(source, rule)
        case QuranicMarks():
            return _quranic(source, rule, original=original)
        case Joins():
            return _joined(source, rule)
        case Scale():
            return _scaled(source, rule, original=original, corpus=corpus)
        case Anchors():
            return _anchored(source, rule, original=original, corpus=corpus)
        case Spacing():
            return _spaced(source, rule, original=original, corpus=corpus)
        case WordSigns():
            return _signed(source, rule, original=original)
        case Records():
            return Ok(_recorded(source, rule))


def _named(rule: str, faults: Faults, /) -> Faults:
    """Prefixes each fault's member with the rule that refused."""
    head, *rest = (msgspec.structs.replace(fault, member=f"{rule} {fault.member}".strip()) for fault in faults)
    return (head, *rest)


def _executed(source: _Source, plan: tuple[_Rules | Metrics, ...], /, *, original: _Binary, corpus: _Corpus) -> Outcome[_Source]:
    """Folds drawing rules over a source, leaving native layout metrics to font construction, the first refused rule naming itself on its faults."""
    initial: Outcome[_Source] = Ok(source)
    return reduce(
        lambda held, rule: held.bind(lambda current: _applied(current, rule, original=original, corpus=corpus).map_error(lambda faults: _named(type(rule).__name__.lower(), faults))),
        (rule for rule in plan if not isinstance(rule, Metrics)),
        initial,
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
