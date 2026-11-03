"""The ordered plan of a face, its rules resolved from the face's values, its genre, and its script, and the fold that applies it to a source."""

from typing import get_args
lazy from functools import reduce
lazy from itertools import pairwise
lazy import unicodedata

import msgspec
lazy from expression import Ok

from typography.faces.declaration import Genres, Treatments
from typography.rules.anchors import Anchors
from typography.rules.digits import Digits
from typography.rules.fits import Outlines
from typography.rules.joins import Joins
from typography.rules.locale import Locale
from typography.rules.marks import Marks, QuranicMarks, WordSigns
from typography.rules.metrics import Metrics, Records
from typography.rules.ornaments import Ornaments
from typography.rules.scale import Scale
from typography.rules.scripts import Rule
from typography.rules.spacing import Spacing
lazy from typography.faces.declaration import Face, Faces, OrnamentFace, Sheared
lazy from typography.faces.faults import Faults, Outcome
lazy from typography.faces.glyphs import Source
lazy from typography.rules import anchors, digits, fits, joins, locale, marks, metrics, ornaments, scale, spacing
lazy from typography.rules.scripts import ARABIC
lazy from typography.shaping.corpus import Corpus
lazy from typography.shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Hinting(Rule, frozen=True, kw_only=True):
    """TrueType hinting the build runs."""


type Rules = Ornaments | Locale | Digits | Outlines | Marks | QuranicMarks | Joins | Scale | Anchors | Spacing | WordSigns | Metrics | Records | Hinting


class Plan(msgspec.Struct, frozen=True, kw_only=True):
    """Rules of a face in the order each measures what earlier ones wrote, and the treatments the face is drawn with."""

    genre: Genres | None
    treatments: tuple[Treatments, ...]
    rules: tuple[Rules, ...]

    def __post_init__(self) -> None:
        """Refuses rules out of the `Rules` union order or repeated, so anchors follow marks and scale, and spacing follows anchors.

        Raises:
            ValueError: If a rule precedes or repeats one the order places before it.
        """
        ranks = tuple(get_args(Rules.__value__).index(type(rule)) for rule in self.rules)
        if any(later <= earlier for earlier, later in pairwise(ranks)):
            raise ValueError(f"rules out of order: {', '.join(type(rule).__name__ for rule in self.rules)}")


# --- [OPERATIONS] -----------------------------------------------------------------------


def planned(face: Faces, /) -> Plan:
    """Returns a face's plan, each value the face's override or its genre's, each code point its script's, its slant from its shear."""
    script = ARABIC
    match face:
        case Face(genre=genre, values=values, treatments=treatments):
            stack = values.stack_gap or genre.stack_gap
            return Plan(
                genre=genre,
                treatments=treatments,
                rules=(
                    Locale(script=script, coverage=genre.coverage),
                    Digits(script=script, zero=values.zero, advance=values.digit_advance or "tabular", bearing=values.digit_bearing),
                    Outlines(floor=values.nuqta_floor or genre.nuqta_floor, spurs=genre.spurs, opened=values.counter, letters=script.counters, dot=script.dotted),
                    Marks(script=script, band=genre.mark, ratio=values.mark, floor=genre.mark_floor, drawn=bool(values.marks_drawn), angle=values.fatha_angle, length=values.fatha_length, stack=stack),
                    QuranicMarks(codes=frozenset(code for code in genre.coverage.required if unicodedata.category(chr(code)) == "Mn" and code not in script.recipes), script=script),
                    Joins(overlap=genre.join_overlap, kashida=script.kashida),
                    Scale(script=script, height=values.alif or genre.alif, cap=genre.pitch[1]),
                    Anchors(script=script, stack=stack, clearance=genre.clearance),
                    Spacing(script=script, ratio=values.word_ratio or genre.word_ratio, band=genre.word_band),
                    WordSigns(script=script, stack=stack),
                    Metrics(pitch=genre.pitch, stack=stack),
                    Records(
                        lowest=values.lowest_ppem,
                        letters=script.counters,
                        heights=script.heights,
                        stem=script.stem,
                        slant=next((treatment.angle for treatment in treatments if isinstance(treatment, Sheared)), 0.0),
                    ),
                    *((Hinting(),) if genre.hinting else ()),
                ),
            )
        case OrnamentFace(first=first):
            return Plan(genre=None, treatments=(), rules=(Ornaments(first=first, script=script), Outlines(sliver=0.001, snap=0.006), Metrics(pitch=None, stack=0.0), Records(heights=script.heights)))


def applied(source: Source, rule: Rules, /, *, original: Binary, corpus: Corpus) -> Outcome[Source]:
    """Applies one rule to a source, measuring the original where the source cannot shape yet."""
    match rule:
        case Ornaments():
            return Ok(ornaments.encoded(source, rule, original=original))
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
            return anchors.seated(source, rule, original=original, corpus=corpus)
        case Spacing():
            return spacing.spaced(source, rule, original=original, corpus=corpus)
        case WordSigns():
            return marks.signed(source, rule, original=original)
        case Metrics():
            return Ok(metrics.measured(source, rule, corpus=corpus))
        case Records():
            return Ok(metrics.recorded(source, rule))
        case Hinting():
            return Ok(source)


def _named(rule: str, faults: Faults, /) -> Faults:
    """Prefixes each fault's member with the rule that refused."""
    head, *rest = (msgspec.structs.replace(fault, member=f"{rule} {fault.member}".strip()) for fault in faults)
    return (head, *rest)


def executed(source: Source, plan: Plan, /, *, original: Binary, corpus: Corpus) -> Outcome[Source]:
    """Folds every rule of a plan over a source, the first refused rule naming itself on its faults."""
    initial: Outcome[Source] = Ok(source)
    return reduce(
        lambda held, rule: held.bind(lambda current: applied(current, rule, original=original, corpus=corpus).map_error(lambda faults: _named(type(rule).__name__.lower(), faults))),
        plan.rules,
        initial,
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Hinting", "Plan", "Rules", "applied", "executed", "planned"]
