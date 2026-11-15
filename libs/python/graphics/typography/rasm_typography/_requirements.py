"""Requirements a build meets per genre and treatment, each measuring its own breaches over runs of a build and its original shaped once each, and the findings they give over a build, its original, a corpus, and approved shaping."""

import sys
from typing import Annotated
lazy from collections.abc import Iterable
lazy from functools import cache, partial, reduce
lazy from itertools import combinations, groupby, pairwise, zip_longest
lazy import math
lazy from pathlib import Path
lazy from statistics import median
lazy import unicodedata

import msgspec
lazy from expression import Option
lazy from expression.extra.result import catch
lazy from fontTools.misc.transform import Offset
lazy from fontTools.ttLib.tables.otTables import LigatureSubst
lazy from pathops import PathOp, PathOpsError

from rasm_typography._faces.declaration import Band, Em, Face, Faces, OrnamentFace, Ratio, Sheared
lazy from rasm_records import Fault, Outcome, Reason
lazy from rasm_typography._faces import coverage
lazy from rasm_typography._faces.glyphs import fetched, Joining, Joinings, Point, Side
lazy from rasm_typography._faces.importing import subtables
lazy from rasm_typography._faces.names import GlyphName
lazy from rasm_typography._outlines import measure, paths
lazy from rasm_typography._rules import measures
lazy from rasm_typography._rules.fits import Outlines
lazy from rasm_typography._rules.plans import planned
lazy from rasm_typography._shaping.corpus import Corpus
lazy from rasm_typography._shaping.expectations import Expected
lazy from rasm_typography._shaping.runs import Binary

# --- [CONSTANTS] ------------------------------------------------------------------------

_DOTS = (0.25, 3.5)

# --- [MODELS] ---------------------------------------------------------------------------

# --- [REQUIREMENTS]


class _Requirement(msgspec.Struct, frozen=True, kw_only=True, tag_field="kind", tag=str.lower):
    """Property a build holds, its tag the finding's requirement name."""


class Expectations(_Requirement, frozen=True, kw_only=True):
    """An approval exists, every approved test shapes to its expectation, and no test shapes to a forbidden glyph."""

    @staticmethod
    def measured(binary: Binary, /, *, approved: Option[Expected], **_: object) -> tuple[tuple[str, float | str, float | str], ...]:
        """Every approved test shaping apart from its expectation, an absent approval its own breach."""
        return approved.map(lambda expected: expected.regressed(binary)).default_value((("shaping", "absent", "approved"),))


class Reachable(_Requirement, frozen=True, kw_only=True):
    """No corpus line shapes to glyph 0 under any shaper and language."""

    shapers: tuple[str, ...] = ("ot", *(("coretext",) if sys.platform == "darwin" else ()))

    def measured(self, binary: Binary, /, *, corpus: Corpus, **_: object) -> tuple[tuple[str, float | str, float | str], ...]:
        """Counts glyph 0 under every shaper and language, with native backend failures reported independently."""

        def measured(text: str, language: str | None, shaper: str, /) -> tuple[tuple[str, float | str, float | str], ...]:
            subject = f"{text} under {shaper} {'dflt' if language is None else language}"
            return (
                catch(binary.shaped, exception=RuntimeError)(text, language=language, shapers=(shaper,))
                .map(lambda run: ((subject, float(count), 0.0),) if (count := run.names.count(binary.order[0])) else ())
                .map_error(lambda error: ((subject, str(error), "available shaper"),))
                .merge()
            )

        return tuple(*measured(text, language, shaper) for shaper in self.shapers for language in corpus.languages for text in corpus.lines)


class Covered(_Requirement, frozen=True, kw_only=True, tag="coverage"):
    """Every required code point mapped and every mapped one required, excluded, or inside a kept block."""

    @staticmethod
    def measured(binary: Binary, /, *, coverage: coverage.Coverage, **_: object) -> tuple[tuple[str, float | str, float | str], ...]:
        """Every required code point the build leaves unmapped and every mapped one the coverage holds no record of."""
        return (
            *((f"U+{code:04X}", "unmapped", "required") for code in coverage.uncovered(binary.encoded)),
            *((f"U+{code:04X}", "mapped", "required, excluded, or kept") for code in coverage.unrecorded(binary.encoded)),
        )


class MarkClearance(_Requirement, frozen=True, kw_only=True):
    """Marks clear their own cluster and stand a gap in em from every other, and where the plan reads letters as carrying dots apart from their drawing, dots and hamzas clear other letters."""

    gap: Em
    dotted: bool

    def measured(self, binary: Binary, /, *, corpus: Corpus, joinings: Joinings, **_: object) -> tuple[tuple[str, float | str, float | str], ...]:
        """Mark and dot gaps over the matrix, letter pairs unwrapped and between two dual-joining letters, and whole words, under each corpus language.

        Args:
            binary: Build to shape.
            corpus: Runs and languages to shape.
            joinings: Unicode joining type and group of each code point.

        Returns:
            The lowest gap of each glyph pair under its reach, with the run that showed it.
        """
        nuqta, boxes, minimum = _nuqta(binary) if self.dotted else 0.0, cache(paths.bounds), self.gap * binary.upem

        @cache
        def grown(outline: tuple[tuple[Point, ...], ...], reach: float, /) -> tuple[tuple[Point, ...], ...]:
            return paths.offset(outline, reach) if reach else outline

        def clashes(text: str, language: str | None, /) -> tuple[tuple[tuple[str, ...], float, float], ...]:
            extents, held = binary.placed_extents(text, language), binary.marks
            return tuple(
                (tuple(sorted((placed.glyph, other.glyph))), apart / binary.upem, reach / binary.upem)
                for placed, piece, reaches in (
                    *((placed, binary.outline(placed.glyph), (minimum, 0.0)) for placed, _ in extents if placed.glyph in held),
                    *((placed, piece, (0.0, None)) for placed, _ in extents if placed.glyph not in held for piece in _split(binary, nuqta, placed.glyph)[1]),
                )
                if (box := boxes(piece))
                for other, extent in extents
                if other is not placed
                and (reach := reaches[other.cluster == placed.cluster]) is not None
                and box.shifted(placed.x, placed.y).grown(reach) & extent
                and paths.combined(paths.transformed(grown(piece, reach), shift := Offset(placed.x - other.x, placed.y - other.y)), near := binary.outline(other.glyph), PathOp.INTERSECTION)
                and (apart := measure.gap(paths.transformed(piece, shift), near)) < reach
            )

        around = next((chr(code) for code in corpus.letters if Joining.of(joinings, code) is Joining.DUAL), None)
        texts = dict.fromkeys((*corpus.matrix, *corpus.pairs(), *corpus.pairs(around), *corpus.word_runs))
        found = sorted((pair, measured, bound, text) for language in corpus.languages for text in texts for pair, measured, bound in clashes(text, language))
        return tuple((f"{' '.join(pair)} in {text}", measured, bound) for pair, group in groupby(found, key=lambda clash: clash[0]) for _, measured, bound, text in (next(group),))


class PartClearance(_Requirement, frozen=True, kw_only=True):
    """Neighbouring glyphs that do not join, parted by a non-joining letter, ZWNJ, or space, keep a gap in em between their bodies, a body the whole drawing where the plan reads no dots apart."""

    gap: Em
    dotted: bool

    def measured(self, binary: Binary, /, *, corpus: Corpus, joinings: Joinings, **_: object) -> tuple[tuple[str, float | str, float | str], ...]:
        """Least gap between the bodies of each two neighbouring unjoined glyphs over the prose, marked, word, and ZWNJ lines under each corpus language, held at the gap."""
        held, split = binary.marks, partial(_split, binary, _nuqta(binary) if self.dotted else 0.0)

        @cache
        def apart(left: str, right: str, dx: float, dy: float, /) -> float:
            return measure.gap(split(left)[0], paths.transformed(split(right)[0], Offset(dx, dy)))

        found = sorted(
            ((a.glyph, b.glyph), distance, text)
            for text in dict.fromkeys((*corpus.plain, *corpus.marked, *corpus.words, *corpus.zwnj))
            for language in corpus.languages
            for a, b in pairwise(placed for placed, _ in binary.placed_extents(text, language) if placed.glyph not in held)
            if not _joined(joinings, a.glyph, b.glyph) and (distance := apart(a.glyph, b.glyph, b.x - a.x, b.y - a.y)) < self.gap * binary.upem
        )
        return tuple((f"{' '.join(pair)} in {text}", distance / binary.upem, self.gap) for pair, group in groupby(found, key=lambda row: row[0]) for _, distance, text in (next(group),))


class MarkSide(_Requirement, frozen=True, kw_only=True):
    """Every below mark sits wholly under the baseline, on each letter form and ligature component it attaches to."""

    @staticmethod
    def measured(binary: Binary, /, *, corpus: Corpus, **_: object) -> tuple[tuple[str, float | str, float | str], ...]:
        """Top of each below mark over the matrix and ligature lines under each corpus language, held at the baseline."""
        held = binary.marks
        found = sorted(
            (placed.glyph, box.ymax / binary.upem, text)
            for text in dict.fromkeys((*corpus.matrix, *corpus.ligatures))
            for language in corpus.languages
            for placed, box in binary.placed_extents(text, language)
            if placed.glyph in held and box.ymax > 0 and (codes := GlyphName.parse(placed.glyph).codes) and Side.of(codes[0]) is Side.BELOW
        )
        return tuple((f"{glyph} in {text}", top, 0.0) for glyph, group in groupby(found, key=lambda row: row[0]) for _, top, text in (next(group),))


class MarkLine(_Requirement, frozen=True, kw_only=True):
    """Single above marks over one letter form share one bottom line within a tolerance in em."""

    tolerance: Em = 0.04

    def measured(self, binary: Binary, /, *, corpus: Corpus, **_: object) -> tuple[tuple[str, float | str, float | str], ...]:
        """Spread of the bottoms of the single above marks over each letter form, held within a tolerance in em."""
        above, held = tuple(stack for stack in corpus.vowel_stacks if len(stack) == 1 and Side.of(ord(stack)) is Side.ABOVE), binary.marks
        return tuple(
            dict.fromkeys(
                (_letter(binary, form, language), spread / binary.upem, self.tolerance)
                for code in corpus.letters
                for language in corpus.languages
                for index, form in enumerate(corpus.forms(code))
                if len(bottoms := tuple(box.ymin for stack in above for placed, box in binary.placed_extents(corpus.forms(code, stack)[index], language) if placed.glyph in held)) > 1
                and (spread := max(bottoms) - min(bottoms)) > self.tolerance * binary.upem
            )
        )


class LigatureArea(_Requirement, frozen=True, kw_only=True):
    """Each ligature holds a ratio of its components' filled area and spans that ratio of its advance."""

    ratio: Ratio = 0.5

    def measured(self, binary: Binary, /, **_: object) -> tuple[tuple[str, float | str, float | str], ...]:
        """Filled area of each GSUB ligature against its components' summed area and its span against its advance, zero references meeting either bound."""
        font = binary.tables
        ligatures = dict.fromkeys(
            (rule.LigGlyph, (first, *rule.Component))
            for lookup in (font["GSUB"].table.LookupList.Lookup if "GSUB" in font else ())
            for sub in subtables(lookup)
            if isinstance(sub, LigatureSubst)
            for first, rules in sub.ligatures.items()
            for rule in rules
        )
        return tuple(
            dict.fromkeys(
                (f"{target} {aspect}", value, self.ratio)
                for glyph, parts in ligatures
                for target in (binary.order[font.getGlyphID(glyph)],)
                for whole, box in ((math.fsum(paths.area(binary.outline(binary.order[font.getGlyphID(part)])) for part in parts), binary.extent(target)),)
                for aspect, measured, reference in (("area", paths.area(binary.outline(target)), whole), ("span", box.width if box else 0.0, float(font["hmtx"][glyph][0])))
                if measured < self.ratio * reference
                for value in (measured / reference,)
            )
        )


class JoinParity(_Requirement, frozen=True, kw_only=True):
    """Joined letter pairs keep their distance with `kern` on and off."""

    @staticmethod
    def measured(binary: Binary, /, *, corpus: Corpus, joinings: Joinings, **_: object) -> tuple[tuple[str, float | str, float | str], ...]:
        """Distance between the pen positions of each joined letter pair with `kern` on against off, held at zero."""
        held = binary.marks

        def steps(text: str, language: str | None, features: frozendict[str, bool], /) -> tuple[tuple[str, str, float], ...]:
            letters = tuple(placed for placed in binary.shaped(text, language=language, features=features).placed if placed.glyph not in held)
            return tuple((a.glyph, b.glyph, b.x - a.x) for a, b in pairwise(letters) if _joined(joinings, a.glyph, b.glyph))

        return tuple(
            dict.fromkeys(
                (f"{a} {b} in {text}", (on - off) / binary.upem, 0.0)
                for text in (*corpus.plain, *corpus.marked, *corpus.zwnj, *corpus.words)
                for language in corpus.languages
                for (a, b, on), (_, _, off) in zip(steps(text, language, frozendict()), steps(text, language, frozendict(kern=False)), strict=False)
                if on != off
            )
        )


class Seam(_Requirement, frozen=True, kw_only=True):
    """Seam columns composited glyph by glyph stay inside a band of the merged outline's filled area at each pixel size, abutting edges compositing short under it and overlaps compositing twice over it."""

    band: tuple[Ratio, Ratio] = (0.87, 1.58)
    sizes: tuple[Annotated[int, msgspec.Meta(ge=1)], ...] = (12, 16, 24, 96)

    @property
    def overlap(self) -> float:
        """Least overlap in em each of two joined glyphs reaches into the other, two equal partial covers of one pixel at the smallest size, the worst seam cell, compositing at the band's floor."""
        return (1 - 2 * math.sqrt(1 - self.band[0])) / (2 * min(self.sizes))

    def measured(self, binary: Binary, /, *, corpus: Corpus, **_: object) -> tuple[tuple[str, float | str, float | str], ...]:
        """Filled area of each seam column composited glyph by glyph against the merged outline, per pixel cell at each size, held inside a band.

        Args:
            binary: Build to shape.
            corpus: Seam lines to shape.

        Returns:
            The lowest and highest column ratio of each line and size outside the band.
        """

        def ratios(outlines: tuple[tuple[tuple[tuple[Point, ...], ...], paths.Box], ...], box: paths.Box, size: int, /) -> tuple[float, ...]:
            unit = binary.upem / size
            bottom, top = math.floor(box.ymin / unit), math.ceil(box.ymax / unit)
            rows = top - bottom
            return tuple(
                composited / whole
                for column in range(math.floor(box.xmin / unit), math.ceil(box.xmax / unit))
                for strip in (paths.Box(xmin=column * unit, ymin=bottom * unit, xmax=(column + 1) * unit, ymax=top * unit),)
                if len(parts := tuple(part for outline, extent in outlines if extent & strip and (part := paths.combined(outline, (strip.contour,), PathOp.INTERSECTION)))) > 1
                if (whole := math.fsum(measure.column_coverage(paths.union(*parts), strip, rows, "y")))
                for composited in (math.fsum(1 - math.prod(1 - cell for cell in row) for row in zip(*(measure.column_coverage(part, strip, rows, "y") for part in parts), strict=False)),)
            )

        return tuple(
            (f"{text} at {size} ppem", measured, limit)
            for text in corpus.seam
            for runs in (
                tuple(
                    (outlines, box)
                    for language in corpus.languages
                    for outlines in (tuple((paths.transformed(binary.outline(placed.glyph), Offset(placed.x, placed.y)), extent) for placed, extent in binary.placed_extents(text, language)),)
                    for box in (_hull(extent for _, extent in outlines),)
                    if box is not None
                ),
            )
            for size in self.sizes
            for found in (tuple(*ratios(outlines, box, size) for outlines, box in runs),)
            for measured, limit, side in ((min(found, default=1.0), self.band[0], 1), (max(found, default=1.0), self.band[1], -1))
            if side * (measured - limit) < 0
        )


class Distinct(_Requirement, frozen=True, kw_only=True):
    """No two digits of one script reach an intersection over union bound."""

    bound: Ratio = 0.9

    def measured(self, binary: Binary, /, **_: object) -> tuple[tuple[str, float | str, float | str], ...]:
        """Intersection over union of every two digits of one script, held under a bound."""
        digits = sorted((code - unicodedata.digit(chr(code)), glyph) for code, glyph in binary.encoded.items() if unicodedata.category(chr(code)) == "Nd")
        return tuple(
            (f"{a} {b}", overlap, self.bound)
            for _, group in groupby(digits, key=lambda digit: digit[0])
            for (_, a), (_, b) in combinations(group, 2)
            if (overlap := paths.iou(binary.outline(a), binary.outline(b))) >= self.bound
        )


class LineMetrics(_Requirement, frozen=True, kw_only=True, tag="metrics"):
    """Line pitch in em inside its band, the original's pitch where none is stated, and win metrics over every glyph and corpus run."""

    pitch: Band | None = None

    def measured(self, binary: Binary, /, *, original: Binary, corpus: Corpus, **_: object) -> tuple[tuple[str, float | str, float | str], ...]:
        """Line pitch in em inside its band, and win ascent and descent over every glyph and every corpus and matrix run the face reaches.

        Args:
            binary: Build to measure.
            original: Font the build was imported from.
            corpus: Lines and matrix runs the win metrics cover.

        Returns:
            The line pitch outside its band and each win metric short of the outline extent.
        """
        upm, line, os2 = binary.upem, _pitch(binary), binary.header.os2
        low, high = self.pitch or (_pitch(original),) * 2
        glyphs = tuple((glyph, box) for glyph in binary.order if (box := binary.extent(glyph)))
        extents = (
            *glyphs,
            *(
                (text, box)
                for text in (*corpus.lines, *corpus.matrix)
                for language in corpus.languages
                for extents in (binary.placed_extents(text, language),)
                if all(placed.glyph != binary.order[0] for placed, _ in extents) and (box := _hull(extent for _, extent in extents))
            ),
        )
        top, bottom = max(extents, key=lambda extent: extent[1].ymax, default=None), min(extents, key=lambda extent: extent[1].ymin, default=None)
        return (
            *((("line pitch", line, low if line < low else high),) if not low <= line <= high else ()),
            *(((f"win ascent at {top[0]}", top[1].ymax / upm, os2.us_win_ascent / upm),) if top and top[1].ymax > os2.us_win_ascent else ()),
            *(((f"win descent at {bottom[0]}", -bottom[1].ymin / upm, os2.us_win_descent / upm),) if bottom and -bottom[1].ymin > os2.us_win_descent else ()),
        )


class Identity(_Requirement, frozen=True, kw_only=True):
    """Each letter form keeps the original's dots and hamzas, piece for piece, each piece's elongation within a ratio of the original's, wherever the build moves them."""

    shape: Ratio = 1.5

    def measured(self, binary: Binary, /, *, original: Binary, corpus: Corpus, **_: object) -> tuple[tuple[str, float | str, float | str], ...]:
        """Pieces apart from the body of each letter form both fonts draw, paired in size order, held where either of a pair is a dot or hamza.

        Args:
            binary: Build to measure.
            original: Font the build's source was imported from.
            corpus: Letters whose forms are measured.

        Returns:
            Each letter form with a dot or hamza the other font lacks, partnered by a piece outside the dot window widened by the elongation ratio, or parting past the elongation ratio, as both fonts' dot elongations.
        """
        pairs = tuple(
            dict.fromkeys(
                (glyph, former)
                for code in corpus.letters
                for form in corpus.forms(code)
                for language in corpus.languages
                if (glyph := _letter(binary, form, language)) != binary.order[0] and (former := _letter(original, form, language)) != original.order[0]
            )
        )
        low, high = _DOTS[0] / self.shape, _DOTS[1] * self.shape

        def shapes(font: Binary, nuqta: float, glyph: str, /) -> tuple[tuple[float, float, bool], ...]:
            held, dots = font.pieces(glyph), _split(font, nuqta, glyph)[1]
            links = tuple((one, two) for (one, (_, first)), (two, (_, second)) in combinations(enumerate(held), 2) if measure.gap(first, second) < 1)
            body = reduce(lambda joined, _: joined | {index for link in links if joined & {*link} for index in link}, held, frozenset({0}))
            return tuple((size / nuqta if nuqta else math.inf, measure.moments(piece).elongation, piece in dots and index not in body) for index, (size, piece) in enumerate(held) if index)

        built, previous = (cache(partial(shapes, font, _nuqta(font))) for font in (binary, original))
        return tuple(
            (glyph, " ".join(f"{own:.2f}" for _, own, dot in mine if dot) or "none", " ".join(f"{old:.2f}" for _, old, dot in theirs if dot) or "none")
            for glyph, former in pairs
            for mine, theirs in ((built(glyph), previous(former)),)
            if any(
                own is None or old is None or not (low <= own[0] <= high and low <= old[0] <= high) or max(own[1], old[1]) >= self.shape * min(own[1], old[1])
                for own, old in zip_longest(mine, theirs)
                if (own is not None and own[2]) or (old is not None and old[2])
            )
        )


type Requirements = Expectations | Reachable | Covered | MarkClearance | PartClearance | MarkSide | MarkLine | LigatureArea | JoinParity | Seam | Distinct | LineMetrics | Identity

# --- [FINDINGS]


class Finding(msgspec.Struct, frozen=True, kw_only=True):
    """Requirement a build misses on one subject, with the measured value and its bound."""

    requirement: str
    subject: str
    measured: float | str
    bound: float | str


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [MEASURES]


def _hull(boxes: Iterable[paths.Box], /) -> paths.Box | None:
    """Extent of boxes together, `None` for none."""
    held = iter(boxes)
    return reduce(paths.Box.__or__, held, first) if (first := next(held, None)) is not None else None


def _nuqta(binary: Binary, /) -> float:
    """Median area of the pieces apart from the largest across the mapped letters, the dot most of them draw."""
    return median(tuple(size for code, glyph in binary.encoded.items() if unicodedata.category(chr(code)).startswith("L") for size, _ in binary.pieces(glyph)[1:]) or (0.0,))


def _split(binary: Binary, nuqta: float, glyph: str, /) -> tuple[tuple[tuple[Point, ...], ...], tuple[tuple[tuple[Point, ...], ...], ...]]:
    """Body of a glyph and its dots and hamzas, the pieces apart from its largest of a quarter to 3.5 nuqtas off the baseline at its joining edges, a larger one a split body part, a smaller one a fragment, one crossing the baseline at a joining edge its stroke, all body at zero nuqta."""
    form, advance, held = GlyphName.parse(glyph).form, binary.tables["hmtx"][glyph][0], binary.pieces(glyph) if nuqta else ((0.0, binary.outline(glyph)),)
    loose = frozenset(index for index, (size, piece) in enumerate(held) if index and _DOTS[0] * nuqta <= size <= _DOTS[1] * nuqta and not measures.joining(form, advance, paths.bounds(piece)))
    return (tuple(contour for index, (_, piece) in enumerate(held) if index not in loose for contour in piece), tuple(piece for index, (_, piece) in enumerate(held) if index in loose))


def _joined(joinings: Joinings, left: str, right: str, /) -> bool:
    """Whether two glyphs side by side join, the left one joining the letter before it and the right one the letter after it, a join-causing glyph on both sides."""
    (left_name, left_causing), (right_name, right_causing) = ((name, any(Joining.of(joinings, code) is Joining.CAUSING for code in name.codes)) for name in map(GlyphName.parse, (left, right)))
    return (left_name.form.right or left_causing) and (right_name.form.left or right_causing)


def _letter(binary: Binary, form: str, language: str | None, /) -> str:
    """Drawn glyph the letter of a form run shapes to, the joiners before it and the marks after it passed over."""
    cluster = len(form.encode()) - len(form.lstrip("\N{ZERO WIDTH JOINER}").encode())
    return next((placed.glyph for placed, _ in binary.placed_extents(form, language) if placed.cluster == cluster and placed.glyph not in binary.marks), binary.order[0])


def _pitch(binary: Binary, /) -> float:
    """Line pitch in em the `hhea` ascender, descender, and line gap state."""
    hhea = binary.header.hhea
    return (hhea.ascent - hhea.descent + hhea.line_gap) / binary.upem


# --- [VERIFICATION]


def requirements(face: Faces, /) -> tuple[Requirements, ...]:
    """Derives the requirements of a face from its genre, its treatments, and whether its plan reads letters as carrying dots apart, an ornament face holding the ones art without letters meets.

    Args:
        face: Design record of the face.

    Returns:
        The requirements in measure order.
    """
    match face:
        case Face(genre=genre, treatments=treatments):
            filled, dotted = all(treatment.filled for treatment in treatments), any(isinstance(rule, Outlines) and rule.dot is not None for rule in planned(face))
            return (
                Expectations(),
                Reachable(),
                Covered(),
                MarkClearance(gap=genre.clearance, dotted=dotted),
                PartClearance(gap=genre.clearance, dotted=dotted),
                MarkSide(),
                MarkLine(),
                LigatureArea(),
                JoinParity(),
                *((Seam(),) if filled else ()),
                Distinct(),
                LineMetrics(pitch=genre.pitch),
                *((Identity(),) if filled and dotted and not any(isinstance(treatment, Sheared) for treatment in treatments) else ()),
            )
        case OrnamentFace():
            return (Covered(), LineMetrics())


def verified(binary: Binary, face: Faces, /, *, coverage: coverage.Coverage, original: Binary, corpus: Corpus, approved: Option[Expected], cache: Path) -> Outcome[tuple[Finding, ...]]:
    """Measures every requirement of a face's build independently and accumulates their findings, an empty tuple passing.

    Args:
        binary: Built font.
        face: Design record whose requirements the build meets.
        coverage: Code points the build answers for, a text face's genre coverage or the mappings an ornament face's source states.
        original: Font the source was imported from.
        corpus: Runs every shaping measure reads.
        approved: Approved shaping of the face, `Nothing` before its first approval.
        cache: Folder holding the pinned Unicode joining table, downloaded where it lacks a copy.

    Returns:
        Ok(findings) in requirement order, `FAILED` for a measure the native boolean refuses, and the faults of holding the pinned Unicode joining table.
    """
    return fetched(cache).bind(
        lambda joinings: catch(
            lambda: tuple(
                Finding(requirement=str(type(requirement).__struct_config__.tag), subject=subject, measured=measured, bound=bound)
                for requirement in requirements(face)
                for subject, measured, bound in requirement.measured(binary, coverage=coverage, original=original, corpus=corpus, approved=approved, joinings=joinings)
            ),
            exception=PathOpsError,
        )().map_error(partial(Fault.of, Reason.FAILED, face.postscript))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Covered",
    "Distinct",
    "Expectations",
    "Finding",
    "Identity",
    "JoinParity",
    "LigatureArea",
    "LineMetrics",
    "MarkClearance",
    "MarkLine",
    "MarkSide",
    "PartClearance",
    "Reachable",
    "Requirements",
    "Seam",
    "requirements",
    "verified",
]
