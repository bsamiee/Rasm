"""Geometric requirement bodies over runs of a build and its original shaped once each, every body returning the breaches it measures."""

from functools import cache
lazy from collections.abc import Iterable
lazy from functools import reduce
lazy from io import BytesIO
lazy from itertools import combinations, groupby, pairwise
lazy import math
lazy import operator
lazy from statistics import median
lazy import unicodedata

lazy from fontTools.misc.transform import Offset
lazy from fontTools.ttLib import TTFont
lazy from fontTools.ttLib.tables.otTables import LigatureSubst

lazy from typography.faces.glyphs import Joining, Outline, Side
lazy from typography.faces.names import GlyphName
lazy from typography.outlines.measure import column_ink, gap
lazy from typography.outlines.paths import area, bounds, Box, combined, offset, Operation, pieces, transformed, union
lazy from typography.shaping.corpus import Corpus
lazy from typography.shaping.runs import Binary, encoded, marks, outline, Placed, shaped, Shaper

# --- [TYPES] ----------------------------------------------------------------------------

type Breach = tuple[str, float | str, float | str]

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [EXTENTS]


@cache
def _font(binary: Binary, /) -> TTFont:
    """Opens the binary's tables for metric reads."""
    return TTFont(BytesIO(binary.data), lazy=True)


@cache
def _box(ink: Outline, /) -> Box | None:
    """Extent of an outline, read once per outline."""
    return bounds(ink)


@cache
def _grown(ink: Outline, reach: float, /) -> Outline:
    return offset(ink, reach) if reach else ink


def _moved(box: Box, x: float, y: float, /) -> Box:
    return Box(xmin=box.xmin + x, ymin=box.ymin + y, xmax=box.xmax + x, ymax=box.ymax + y)


def _hull(boxes: Iterable[Box], /) -> Box | None:
    """Extent of boxes together, `None` for none."""
    held = tuple(boxes)
    return reduce(operator.or_, held) if held else None


# --- [GLYPHS]


@cache
def _nuqta(binary: Binary, /) -> float:
    """Median area of the pieces apart from the largest across the mapped letters, the dot most of them draw."""
    return median(
        tuple(area(piece) for code, glyph in encoded(binary).items() if unicodedata.category(chr(code)).startswith("L") for piece in sorted(pieces(outline(binary, glyph)), key=area)[:-1]) or (0.0,)
    )


@cache
def _split(binary: Binary, glyph: str, /) -> tuple[Outline, tuple[Outline, ...]]:
    """Body of a glyph and its dots and hamzas, the pieces apart from its largest of at most 3.5 nuqtas, a larger one a split part of the body."""
    body, *rest = sorted(pieces(outline(binary, glyph)), key=area, reverse=True) or [()]
    split = 3.5 * _nuqta(binary)
    return (*body, *(contour for piece in rest if area(piece) > split for contour in piece)), tuple(piece for piece in rest if area(piece) <= split)


@cache
def _inked(binary: Binary, text: str, language: str, /) -> tuple[tuple[Placed, Box], ...]:
    """Glyphs of a run shaped once that hold ink, each with its extent at its placed position."""
    return tuple((placed, _moved(box, placed.x, placed.y)) for placed in shaped(binary, text, language=language).placed if (box := _box(outline(binary, placed.glyph))))


def _letter(binary: Binary, form: str, /) -> str:
    """Inked glyph the letter of a form run shapes to, the joiners before it and the marks after it passed over."""
    cluster = len(form.encode()) - len(form.lstrip("\N{ZERO WIDTH JOINER}").encode())
    return next((placed.glyph for placed, _ in _inked(binary, form, "") if placed.cluster == cluster and placed.glyph not in marks(binary)), binary.order[0])


# --- [MARKS]


def _clashes(binary: Binary, text: str, language: str, floor: float, /) -> tuple[tuple[tuple[str, ...], float, float], ...]:
    """Gaps under their reach in one run, a mark to its own cluster under zero and to another under the floor, a dot or hamza on another cluster's ink under zero, measured only where the piece grown by the reach meets the other glyph."""
    held, inked = marks(binary), _inked(binary, text, language)
    return tuple(
        (tuple(sorted((placed.glyph, other.glyph))), apart / binary.upm, reach / binary.upm)
        for placed, piece, reaches in (
            *((placed, outline(binary, placed.glyph), (floor, 0.0)) for placed, _ in inked if placed.glyph in held),
            *((placed, piece, (0.0, None)) for placed, _ in inked if placed.glyph not in held for piece in _split(binary, placed.glyph)[1]),
        )
        if (box := _box(piece))
        for other, extent in inked
        if other is not placed
        and (reach := reaches[other.cluster == placed.cluster]) is not None
        and _moved(box, placed.x, placed.y).grown(reach) & extent
        and combined(transformed(_grown(piece, reach), shift := Offset(placed.x - other.x, placed.y - other.y)), near := outline(binary, other.glyph), Operation.INTERSECTION)
        and (apart := gap(transformed(piece, shift), near)) < reach
    )


def clearance(binary: Binary, corpus: Corpus, floor: float, /) -> tuple[Breach, ...]:
    """Mark and dot gaps over the matrix, letter pairs bare and between two dual-joining letters, and whole words, under each corpus language.

    Args:
        binary: Build to shape.
        corpus: Runs and languages to shape.
        floor: Least gap in em from a mark to a glyph of another cluster.

    Returns:
        The lowest gap of each glyph pair under its reach, with the run that showed it.
    """
    around = next((chr(code) for code in corpus.letters if Joining.of(code) is Joining.DUAL), "")
    texts = dict.fromkeys((*corpus.matrix, *corpus.pairs(), *corpus.pairs(around), *corpus.word_runs))
    found = sorted((pair, measured, bound, text) for language in corpus.languages for text in texts for pair, measured, bound in _clashes(binary, text, language, floor * binary.upm))
    return tuple((f"{' '.join(pair)} in {text}", measured, bound) for pair, group in groupby(found, key=lambda clash: clash[0]) for _, measured, bound, text in (next(group),))


def mark_line(binary: Binary, corpus: Corpus, tolerance: float, /) -> tuple[Breach, ...]:
    """Spread of the bottoms of the single above marks over each letter form, held within a tolerance in em."""
    above, held = tuple(stack for stack in corpus.vowel_stacks if len(stack) == 1 and Side.of(ord(stack)) is Side.ABOVE), marks(binary)
    return tuple(
        dict.fromkeys(
            (_letter(binary, form), spread / binary.upm, tolerance)
            for code in corpus.letters
            for index, form in enumerate(corpus.forms(code))
            if len(bottoms := tuple(box.ymin for stack in above for placed, box in _inked(binary, corpus.forms(code, stack)[index], "") if placed.glyph in held)) > 1
            and (spread := max(bottoms) - min(bottoms)) > tolerance * binary.upm
        )
    )


# --- [LETTERS]


def ligature_ink(binary: Binary, ratio: float, /) -> tuple[Breach, ...]:
    """Ink of each GSUB ligature against its components' summed ink, and its ink span against its advance, each held at a ratio."""
    font = _font(binary)
    ligatures = dict.fromkeys(
        (rule.LigGlyph, (first, *rule.Component))
        for lookup in (font["GSUB"].table.LookupList.Lookup if "GSUB" in font else ())
        for table in lookup.SubTable
        if isinstance(sub := vars(table).get("ExtSubTable", table), LigatureSubst)
        for first, rules in sub.ligatures.items()
        for rule in rules
    )
    return tuple(
        dict.fromkeys(
            (f"{glyph} {measure}", value, ratio)
            for glyph, parts in ligatures
            for whole, box in ((math.fsum(area(outline(binary, part)) for part in parts), _box(outline(binary, glyph))),)
            for measure, value in (("ink", area(outline(binary, glyph)) / whole if whole else 1.0), ("span", (box.width if box else 0.0) / (font["hmtx"][glyph][0] or 1)))
            if value < ratio
        )
    )


def distinct(binary: Binary, bound: float, /) -> tuple[Breach, ...]:
    """Intersection over union of every two digits of one script, held under a bound."""
    digits = sorted((code - unicodedata.digit(chr(code)), glyph) for code, glyph in encoded(binary).items() if unicodedata.category(chr(code)) == "Nd")
    return tuple(
        (f"{a} {b}", overlap, bound)
        for _, group in groupby(digits, key=lambda digit: digit[0])
        for (_, a), (_, b) in combinations(group, 2)
        if (joined := area(union(ink := outline(binary, a), other := outline(binary, b)))) and (overlap := area(combined(ink, other, Operation.INTERSECTION)) / joined) >= bound
    )


def identity(binary: Binary, original: Binary, corpus: Corpus, /) -> tuple[Breach, ...]:
    """Dots and hamzas of each letter form both fonts draw, every piece of either meeting the other's once the original is scaled by the median body height ratio and centred on the body.

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
            if (glyph := _letter(binary, form)) != binary.order[0] and (former := _letter(original, form)) != original.order[0]
        )
    )
    bodies = tuple((glyph, former, mine, theirs) for glyph, former in pairs if (mine := _box(_split(binary, glyph)[0])) and (theirs := _box(_split(original, former)[0])) and theirs.height)
    factor = median(tuple(mine.height / theirs.height for _, _, mine, theirs in bodies) or (1.0,))
    return tuple(
        (glyph, float(missed), 0.0)
        for glyph, former, mine, theirs in bodies
        for apart, kept in ((_split(binary, glyph)[1], tuple(transformed(piece, (factor, 0, 0, factor, mine.middle[0] - factor * theirs.middle[0], 0)) for piece in _split(original, former)[1])),)
        if (missed := sum(1 for piece in apart if not combined(piece, union(*kept), Operation.INTERSECTION)) + sum(1 for piece in kept if not combined(piece, union(*apart), Operation.INTERSECTION)))
    )


def join_parity(binary: Binary, corpus: Corpus, /) -> tuple[Breach, ...]:
    """Distance between the pen positions of each joined letter pair with `kern` on against off, held at zero."""
    held = marks(binary)

    def steps(text: str, language: str, features: frozendict[str, bool], /) -> tuple[tuple[str, str, float], ...]:
        letters = tuple(placed for placed in shaped(binary, text, language=language, features=features).placed if placed.glyph not in held)
        return tuple((a.glyph, b.glyph, b.x - a.x) for a, b in pairwise(letters) if GlyphName.parse(a.glyph).form.right and GlyphName.parse(b.glyph).form.left)

    return tuple(
        dict.fromkeys(
            (f"{a} {b} in {text}", (on - off) / binary.upm, 0.0)
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
        columns, rows = (max(1, math.ceil(extent * size / binary.upm)) for extent in (box.width, box.height))
        return tuple(
            composited / whole
            for strip in box.strips(columns)
            if len(parts := tuple(part for ink, extent in inks if extent & strip and (part := combined(ink, (strip.contour,), Operation.INTERSECTION)))) > 1
            if (whole := math.fsum(column_ink(union(*parts), strip, rows, "y")))
            for composited in (math.fsum(1 - math.prod(1 - cell for cell in row) for row in zip(*(column_ink(part, strip, rows, "y") for part in parts), strict=True)),)
        )

    return tuple(
        (f"{text} at {size} ppem", measured, limit)
        for text in corpus.seam
        for inks in (tuple((transformed(outline(binary, placed.glyph), Offset(placed.x, placed.y)), extent) for placed, extent in _inked(binary, text, "")),)
        if (box := _hull(extent for _, extent in inks))
        for size in sizes
        for found in (ratios(inks, box, size),)
        for measured, limit, side in ((min(found, default=1.0), band[0], 1), (max(found, default=1.0), band[1], -1))
        if side * (measured - limit) < 0
    )


def _pitch(binary: Binary, /) -> float:
    """Line pitch in em the `hhea` ascender, descender, and line gap state."""
    hhea = vars(_font(binary)["hhea"])
    return float(hhea["ascent"] - hhea["descent"] + hhea["lineGap"]) / binary.upm


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
    upm, line, os2 = binary.upm, _pitch(binary), vars(_font(binary)["OS/2"])
    low, high = pitch or (_pitch(original),) * 2
    extents = (
        *((glyph, box) for glyph in binary.order if (box := _box(outline(binary, glyph)))),
        *(
            (text, box)
            for text in (*corpus.lines, *corpus.matrix)
            for inked in (_inked(binary, text, ""),)
            if all(placed.glyph != binary.order[0] for placed, _ in inked) and (box := _hull(extent for _, extent in inked))
        ),
    )
    top, bottom = max(extents, key=lambda extent: extent[1].ymax, default=None), min(extents, key=lambda extent: extent[1].ymin, default=None)
    return (
        *((("line pitch", line, low if line < low else high),) if not low <= line <= high else ()),
        *(((f"win ascent at {top[0]}", top[1].ymax / upm, os2["usWinAscent"] / upm),) if top and top[1].ymax > os2["usWinAscent"] else ()),
        *(((f"win descent at {bottom[0]}", -bottom[1].ymin / upm, os2["usWinDescent"] / upm),) if bottom and -bottom[1].ymin > os2["usWinDescent"] else ()),
    )


def reachable(binary: Binary, corpus: Corpus, shapers: tuple[Shaper, ...], /) -> tuple[Breach, ...]:
    """Count of glyph 0 in every corpus line under each shaper and language, held at zero."""
    return tuple(
        (f"{text} under {shaper} {language or 'dflt'}", float(count), 0.0)
        for shaper in shapers
        for language in corpus.languages
        for text in corpus.lines
        if (count := shaped(binary, text, language=language, shaper=shaper).names.count(binary.order[0]))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Breach", "clearance", "distinct", "identity", "join_parity", "ligature_ink", "mark_line", "metrics", "reachable", "seam"]
