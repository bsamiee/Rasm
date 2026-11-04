"""Geometric requirement bodies over runs of a build and its original shaped once each, every body returning the breaches it measures."""

lazy from collections.abc import Iterable
lazy from functools import cache, partial, reduce
lazy from itertools import combinations, groupby, pairwise
lazy import math
lazy from statistics import median
lazy import unicodedata

lazy from expression.extra.result import catch
lazy from fontTools.misc.transform import Offset
lazy from fontTools.ttLib.tables.otTables import ExtensionSubst, LigatureSubst  # type: ignore[attr-defined] # ty: ignore[unresolved-import]
lazy from pathops import PathOp

lazy from typography.faces.glyphs import Joining, Outline, Side
lazy from typography.faces.names import GlyphName
lazy from typography.outlines.measure import column_ink, gap
lazy from typography.outlines.paths import area, bounds, Box, combined, iou, offset, pieces, transformed, union
lazy from typography.shaping.corpus import Corpus
lazy from typography.shaping.runs import Binary

# --- [TYPES] ----------------------------------------------------------------------------

type Breach = tuple[str, float | str, float | str]

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [EXTENTS]


def _hull(boxes: Iterable[Box], /) -> Box | None:
    """Extent of boxes together, `None` for none."""
    held = iter(boxes)
    return reduce(Box.__or__, held, first) if (first := next(held, None)) is not None else None


# --- [GLYPHS]


def _nuqta(binary: Binary, /) -> float:
    """Median area of the pieces apart from the largest across the mapped letters, the dot most of them draw."""
    letters = tuple(glyph for code, glyph in binary.encoded.items() if unicodedata.category(chr(code)).startswith("L"))
    sizes = {glyph: sorted(map(area, pieces(binary.outline(glyph))))[:-1] for glyph in dict.fromkeys(letters)}
    return median(tuple(*sizes[glyph] for glyph in letters) or (0.0,))


def _split(binary: Binary, nuqta: float, glyph: str, /) -> tuple[Outline, tuple[Outline, ...]]:
    """Body of a glyph and its dots and hamzas, the pieces apart from its largest of at most 3.5 nuqtas, a larger one a split part of the body."""
    limit = 3.5 * nuqta
    held = sorted(((area(piece), piece) for piece in pieces(binary.outline(glyph))), key=lambda held: held[0], reverse=True)
    return tuple(contour for index, (size, piece) in enumerate(held) if index == 0 or size > limit for contour in piece), tuple(piece for size, piece in held[1:] if size <= limit)


def _letter(binary: Binary, form: str, language: str, /) -> str:
    """Inked glyph the letter of a form run shapes to, the joiners before it and the marks after it passed over."""
    cluster = len(form.encode()) - len(form.lstrip("\N{ZERO WIDTH JOINER}").encode())
    return next((placed.glyph for placed, _ in binary.inked(form, language) if placed.cluster == cluster and placed.glyph not in binary.marks), binary.order[0])


# --- [MARKS]


def clearance(binary: Binary, corpus: Corpus, floor: float, /) -> tuple[Breach, ...]:
    """Mark and dot gaps over the matrix, letter pairs unwrapped and between two dual-joining letters, and whole words, under each corpus language.

    Args:
        binary: Build to shape.
        corpus: Runs and languages to shape.
        floor: Least gap in em from a mark to a glyph of another cluster.

    Returns:
        The lowest gap of each glyph pair under its reach, with the run that showed it.
    """
    split, boxes, minimum = cache(partial(_split, binary, _nuqta(binary))), cache(bounds), floor * binary.face.upem

    @cache
    def grown(ink: Outline, reach: float, /) -> Outline:
        return offset(ink, reach) if reach else ink

    def clashes(text: str, language: str, /) -> tuple[tuple[tuple[str, ...], float, float], ...]:
        inked, held = binary.inked(text, language), binary.marks
        return tuple(  # ty: ignore[unsound-return-statement]
            (tuple(sorted((placed.glyph, other.glyph))), apart / binary.face.upem, reach / binary.face.upem)
            for placed, piece, reaches in (
                *((placed, binary.outline(placed.glyph), (minimum, 0.0)) for placed, _ in inked if placed.glyph in held),
                *((placed, piece, (0.0, None)) for placed, _ in inked if placed.glyph not in held for piece in split(placed.glyph)[1]),
            )
            if (box := boxes(piece))
            for other, extent in inked
            if other is not placed
            and (reach := reaches[other.cluster == placed.cluster]) is not None
            and box.shifted(placed.x, placed.y).grown(reach) & extent
            and combined(transformed(grown(piece, reach), shift := Offset(placed.x - other.x, placed.y - other.y)), near := binary.outline(other.glyph), PathOp.INTERSECTION)
            and (apart := gap(transformed(piece, shift), near)) < reach
        )

    around = next((chr(code) for code in corpus.letters if Joining.of(code) is Joining.DUAL), "")
    texts = dict.fromkeys((*corpus.matrix, *corpus.pairs(), *corpus.pairs(around), *corpus.word_runs))
    found = sorted((pair, measured, bound, text) for language in corpus.languages for text in texts for pair, measured, bound in clashes(text, language))
    return tuple((f"{' '.join(pair)} in {text}", measured, bound) for pair, group in groupby(found, key=lambda clash: clash[0]) for _, measured, bound, text in (next(group),))


def mark_line(binary: Binary, corpus: Corpus, tolerance: float, /) -> tuple[Breach, ...]:
    """Spread of the bottoms of the single above marks over each letter form, held within a tolerance in em."""
    above, held = tuple(stack for stack in corpus.vowel_stacks if len(stack) == 1 and Side.of(ord(stack)) is Side.ABOVE), binary.marks
    return tuple(  # ty: ignore[unsound-return-statement]
        dict.fromkeys(
            (_letter(binary, form, language), spread / binary.face.upem, tolerance)
            for code in corpus.letters
            for language in corpus.languages
            for index, form in enumerate(corpus.forms(code))
            if len(bottoms := tuple(box.ymin for stack in above for placed, box in binary.inked(corpus.forms(code, stack)[index], language) if placed.glyph in held)) > 1
            and (spread := max(bottoms) - min(bottoms)) > tolerance * binary.face.upem
        )
    )


# --- [LETTERS]


def ligature_ink(binary: Binary, ratio: float, /) -> tuple[Breach, ...]:
    """Ink of each GSUB ligature against its components' summed ink and its span against its advance, zero references meeting either bound."""
    font = binary.tables
    ligatures = dict.fromkeys(
        (rule.LigGlyph, (first, *rule.Component))
        for lookup in (font["GSUB"].table.LookupList.Lookup if "GSUB" in font else ())
        for table in lookup.SubTable
        if isinstance(sub := table.ExtSubTable if isinstance(table, ExtensionSubst) else table, LigatureSubst)
        for first, rules in sub.ligatures.items()
        for rule in rules
    )
    return tuple(  # ty: ignore[unsound-return-statement]
        dict.fromkeys(
            (f"{target} {measure}", value, ratio)
            for glyph, parts in ligatures
            for target in (binary.order[font.getGlyphID(glyph)],)
            for whole, box in ((math.fsum(area(binary.outline(binary.order[font.getGlyphID(part)])) for part in parts), binary.extent(target)),)
            for measure, measured, reference in (("ink", area(binary.outline(target)), whole), ("span", box.width if box else 0.0, font["hmtx"][glyph][0]))
            if measured < ratio * reference
            for value in (measured / reference,)
        )
    )


def distinct(binary: Binary, bound: float, /) -> tuple[Breach, ...]:
    """Intersection over union of every two digits of one script, held under a bound."""
    digits = sorted((code - unicodedata.digit(chr(code)), glyph) for code, glyph in binary.encoded.items() if unicodedata.category(chr(code)) == "Nd")
    return tuple(
        (f"{a} {b}", overlap, bound)
        for _, group in groupby(digits, key=lambda digit: digit[0])
        for (_, a), (_, b) in combinations(group, 2)
        if (overlap := iou(binary.outline(a), binary.outline(b))) >= bound
    )


def identity(binary: Binary, original: Binary, corpus: Corpus, /) -> tuple[Breach, ...]:
    """Dots and hamzas of each letter form both fonts draw, every piece of either meeting the other's once the original is scaled by the median body height ratio and centered on the body.

    Args:
        binary: Build to measure.
        original: Font the build's source was imported from.
        corpus: Letters whose forms are measured.

    Returns:
        Each letter form with the count of its pieces and the original's that meet nothing, held at zero.
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
    mine, theirs = (cache(partial(_split, font, _nuqta(font))) for font in (binary, original))
    boxes = cache(bounds)
    bodies = tuple((glyph, former, own, old) for glyph, former in pairs if (own := boxes(mine(glyph)[0])) and (old := boxes(theirs(former)[0])) and old.height)
    factor = median(tuple(own.height / old.height for _, _, own, old in bodies) or (1.0,))
    return tuple(
        (glyph, float(missed), 0.0)
        for glyph, former, own, old in bodies
        for apart, kept in ((mine(glyph)[1], tuple(transformed(piece, (factor, 0, 0, factor, own.middle[0] - factor * old.middle[0], 0)) for piece in theirs(former)[1])),)
        for held, previous in ((union(*apart), union(*kept)),)
        if (missed := sum(1 for piece in apart if not combined(piece, previous, PathOp.INTERSECTION)) + sum(1 for piece in kept if not combined(piece, held, PathOp.INTERSECTION)))
    )


def join_parity(binary: Binary, corpus: Corpus, /) -> tuple[Breach, ...]:
    """Distance between the pen positions of each joined letter pair with `kern` on against off, held at zero."""
    held = binary.marks

    def steps(text: str, language: str, features: frozendict[str, bool], /) -> tuple[tuple[str, str, float], ...]:
        letters = tuple(placed for placed in binary.shaped(text, language=language, features=features).placed if placed.glyph not in held)
        return tuple((a.glyph, b.glyph, b.x - a.x) for a, b in pairwise(letters) if GlyphName.parse(a.glyph).form.right and GlyphName.parse(b.glyph).form.left)

    return tuple(  # ty: ignore[unsound-return-statement]
        dict.fromkeys(
            (f"{a} {b} in {text}", (on - off) / binary.face.upem, 0.0)
            for text in (*corpus.plain, *corpus.marked, *corpus.zwnj, *corpus.words)
            for language in corpus.languages
            for (a, b, on), (_, _, off) in zip(steps(text, language, frozendict()), steps(text, language, frozendict(kern=False)), strict=False)
            if on != off
        )
    )


# --- [RUNS]


def seam(binary: Binary, corpus: Corpus, band: tuple[float, float], sizes: tuple[int, ...], /) -> tuple[Breach, ...]:
    """Ink of each seam column composited glyph by glyph against the merged outline, per pixel cell at each size, held inside a band.

    Args:
        binary: Build to shape.
        corpus: Seam lines to shape.
        band: Least and greatest column ratio, a dip where abutting edges composite short and a doubling where overlaps composite twice.
        sizes: Pixels per em of the cell grid.

    Returns:
        The lowest and highest column ratio of each line and size outside the band.
    """

    def ratios(inks: tuple[tuple[Outline, Box], ...], box: Box, size: int, /) -> tuple[float, ...]:
        unit = binary.face.upem / size
        bottom, top = math.floor(box.ymin / unit), math.ceil(box.ymax / unit)
        rows = top - bottom
        return tuple(
            composited / whole
            for column in range(math.floor(box.xmin / unit), math.ceil(box.xmax / unit))
            for strip in (Box(xmin=column * unit, ymin=bottom * unit, xmax=(column + 1) * unit, ymax=top * unit),)
            if len(parts := tuple(part for ink, extent in inks if extent & strip and (part := combined(ink, (strip.contour,), PathOp.INTERSECTION)))) > 1
            if (whole := math.fsum(column_ink(union(*parts), strip, rows, "y")))
            for composited in (math.fsum(1 - math.prod(1 - cell for cell in row) for row in zip(*(column_ink(part, strip, rows, "y") for part in parts), strict=False)),)
        )

    return tuple(
        (f"{text} at {size} ppem", measured, limit)
        for text in corpus.seam
        for runs in (
            tuple(
                (inks, box)
                for language in corpus.languages
                for inks in (tuple((transformed(binary.outline(placed.glyph), Offset(placed.x, placed.y)), extent) for placed, extent in binary.inked(text, language)),)
                for box in (_hull(extent for _, extent in inks),)
                if box is not None
            ),
        )
        for size in sizes
        for found in (tuple(*ratios(inks, box, size) for inks, box in runs),)
        for measured, limit, side in ((min(found, default=1.0), band[0], 1), (max(found, default=1.0), band[1], -1))
        if side * (measured - limit) < 0
    )


def _pitch(binary: Binary, /) -> float:
    """Line pitch in em the `hhea` ascender, descender, and line gap state."""
    hhea = binary.tables["hhea"]
    return (hhea.ascent - hhea.descent + hhea.lineGap) / binary.face.upem  # type: ignore[attr-defined, no-any-return] # ty: ignore[unresolved-attribute, unsound-return-statement]


def metrics(binary: Binary, original: Binary, corpus: Corpus, pitch: tuple[float, float] | None, /) -> tuple[Breach, ...]:
    """Line pitch in em inside its band, and win ascent and descent over every glyph and every corpus and matrix run the face reaches.

    Args:
        binary: Build to measure.
        original: Font the build was imported from.
        corpus: Lines and matrix runs the win metrics cover.
        pitch: Least and greatest line pitch in em, `None` for the original's pitch.

    Returns:
        The line pitch outside its band and each win metric short of the ink.
    """
    upm, line, os2 = binary.face.upem, _pitch(binary), binary.tables["OS/2"]
    low, high = pitch or (_pitch(original),) * 2
    glyphs = tuple((glyph, box) for glyph in binary.order if (box := binary.extent(glyph)))
    extents = (
        *glyphs,
        *(
            (text, box)
            for text in (*corpus.lines, *corpus.matrix)
            for language in corpus.languages
            for inked in (binary.inked(text, language),)
            if all(placed.glyph != binary.order[0] for placed, _ in inked) and (box := _hull(extent for _, extent in inked))
        ),
    )
    top, bottom = max(extents, key=lambda extent: extent[1].ymax, default=None), min(extents, key=lambda extent: extent[1].ymin, default=None)
    return (  # ty: ignore[unsound-return-statement]
        *((("line pitch", line, low if line < low else high),) if not low <= line <= high else ()),
        *(((f"win ascent at {top[0]}", top[1].ymax / upm, os2.usWinAscent / upm),) if top and top[1].ymax > os2.usWinAscent else ()),  # type: ignore[attr-defined] # ty: ignore[unresolved-attribute]
        *(((f"win descent at {bottom[0]}", -bottom[1].ymin / upm, os2.usWinDescent / upm),) if bottom and -bottom[1].ymin > os2.usWinDescent else ()),  # type: ignore[attr-defined] # ty: ignore[unresolved-attribute]
    )


def reachable(binary: Binary, corpus: Corpus, shapers: tuple[str, ...], /) -> tuple[Breach, ...]:
    """Counts glyph 0 under every shaper and language, with native backend failures reported independently."""

    def measured(text: str, language: str, shaper: str, /) -> tuple[Breach, ...]:
        subject = f"{text} under {shaper} {language or 'dflt'}"
        return (
            catch(binary.shaped, exception=RuntimeError)(text, language=language, shapers=(shaper,))
            .map(lambda run: ((subject, float(count), 0.0),) if (count := run.names.count(binary.order[0])) else ())
            .map_error(lambda error: ((subject, str(error), "available shaper"),))
            .merge()
        )

    return tuple(*measured(text, language, shaper) for shaper in shapers for language in corpus.languages for text in corpus.lines)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Breach", "clearance", "distinct", "identity", "join_parity", "ligature_ink", "mark_line", "metrics", "reachable", "seam"]
