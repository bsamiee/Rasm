# [PY_ARTIFACTS_LAYOUT]

The paragraph line-layout owner over the document rail. `LineLayout` is ONE owner that takes the shaped `typography/shape#SHAPE` `PositionedGlyphRun` and a measure (column width) and folds it through a closed `LayoutOp` family into line-broken paragraph runs: uniseg resolves the UAX#14 mandatory/opportunity line-break positions over the source text, pyphen inserts the soft-hyphenation opportunities a tight measure needs, and a hand-rolled Knuth-Plass total-fit paragraph layout encodes the run as a Box/Glue/Penalty item stream and finds the globally optimal set of breakpoints by a badness/demerits dynamic program. uniseg and pyphen are admission-pending (no folder `.api` catalogue, no manifest pin yet), so their members are signature-locked and marked RESEARCH; the Knuth-Plass item algebra and the break dynamic program are this owner's own algorithm, never a library re-export. The `LineBrokenRun` is the one value object the layout arm carries — a tuple of `LayoutLine` slices over the shaped run plus its per-line advance — consumed by `document/emit#DOCUMENT` paragraph emission and `composition/compose#COMPOSE` placement. Every arm returns a `RuntimeRail[ContentKey]` keyed by the runtime content key and contributes one `ArtifactReceipt.Document`.

## [01]-[INDEX]

- [01]-[LAYOUT]: uniseg UAX#14 line-break, pyphen hyphenation, and the hand-rolled Knuth-Plass total-fit paragraph owner over the closed `LayoutOp` step table; `Item` is the Box/Glue/Penalty paragraph-item value (one closed `tagged_union`), `LineBrokenRun` the line-broken value object the layout arm carries, `BreakClass` the UAX#14 break-opportunity policy the `BREAK` arm reads and the Knuth-Plass dynamic program threads through the `Penalty` items.

## [02]-[LAYOUT]

- Cases: `LayoutOp` rows `BREAK` uniseg UAX#14 line-break-opportunity resolution over `uniseg.linebreak.line_break_units(text)` segment iteration and the `uniseg.linebreak.line_break(char)` break-class read, projecting each inter-token gap into a `BreakClass` row the Knuth-Plass `Penalty` cost consumes — `MANDATORY` at a hard newline / `` line-separator, `OPPORTUNITY` at a space/CJK boundary, `PROHIBITED` inside a no-break cluster) · `HYPHENATE` (pyphen `Pyphen(lang=, left=, right=)` dictionary soft-hyphenation over `Pyphen.positions(word)` returning the in-word break offsets and `Pyphen.iterate(word)` the prefix/suffix split pairs, each soft break projected into a `flagged` `Penalty` carrying the hyphen advance the measure pays when the line breaks there — never `Pyphen.inserted` string mangling, the positional break offsets are the algebra the Knuth-Plass items need) · `PARAGRAPH` (the hand-rolled Knuth-Plass total-fit break — encode the shaped run as the `Item` Box/Glue/Penalty stream threading the `BREAK` opportunities and `HYPHENATE` soft breaks, walk the active-node dynamic program computing each candidate line's `badness` from the glue stretch/shrink ratio against the measure, fold the badness + the `Penalty` cost + the consecutive-flagged/fitness-class demerits into the running total-demerit cost, prune dominated active nodes, and trace the optimal breakpoint set back into a `LineBrokenRun` of `LayoutLine` slices) — selected by the frozen `_LAYOUT_TABLE` row, never a chain of `is`-probes; the measure (column width), the glue stretch/shrink elasticity, the hyphen-penalty cost, and the consecutive-flagged-break and fitness-class demerit weights are typed `LayoutParams` fields, the `Item` stream a closed `tagged_union` not an open tuple bag, and the break-opportunity class the `BreakClass` policy row.
- Auto: break resolution folds the source text through `line_break_units(text)` into break-token segments and reads `line_break(char)` per boundary into the `BreakClass` opportunity stream; hyphenation folds each word through `Pyphen(lang=language, left=left_min, right=right_min).positions(word)` into the in-word soft-break offsets and `iterate(word)` into the prefix/suffix split the hyphen-advance penalty measures; the paragraph fold (`_stream`) encodes the `PositionedGlyphRun` per-glyph advances into `Box` widths, each inter-word space into a breakable `Glue` carrying natural/stretch/shrink from `LayoutParams`, each `MANDATORY` break into a forced `Penalty(-inf)`, and each `HYPHENATE` soft break into a flagged `Penalty(hyphen_penalty, hyphen_advance)`, then closes the stream with a finishing `Glue(0, inf, 0)` and a forced terminal `Penalty(-inf)` so the last line absorbs its slack; `_paragraph` precomputes the cumulative width/stretch/shrink prefix sums and runs the active-node dynamic program — for each legal breakpoint (a `Penalty`, or a `Glue` following a `Box`) and each active node it computes the adjustment ratio `r` of the line from the node's line-start to the candidate against the measure, derives `badness = 100 * |r|**3` (infinite below `r = -1`), folds the line-penalty + badness + the squared `Penalty`-cost demerit plus the consecutive-flagged and fitness-class-jump demerits into the running total, deactivates a node whose `r < -1` (or on a forced break), and keeps the minimum-total-demerit candidate per fitness class — a forced break (mandatory newline, terminal) admits a candidate at any badness so the break is never silently dropped, an optional break only within `tolerance` — and on the terminal forced break `_trace` walks the optimal predecessor chain back into a `LineBrokenRun` of `LayoutLine(start, stop, ratio, advance)` slices indexed into the shaped run through the parallel glyph-origin column.
- Receipt: every arm projects its output onto the shared `core/receipt#RECEIPT` `ArtifactReceipt` family, never a per-step receipt — the `BREAK` arm contributes `ArtifactReceipt.Document` carrying the content key and the encoded break-opportunity stream byte count, the `HYPHENATE` arm contributes `ArtifactReceipt.Document` carrying the content key and the encoded soft-break-offset map byte count, and the `PARAGRAPH` arm contributes `ArtifactReceipt.Document` carrying the content key and the encoded `LineBrokenRun` byte count. The line count, the per-line adjustment ratios, the total-demerit cost the optimal break achieved, and the number of flagged (hyphenated) breaks the `PARAGRAPH` fold computes stay interior evidence the arm folds into its content-key derivation, not new receipt fields the shared `Document` case cannot carry.
- Growth: a new break class is one `BreakClass` row plus its `Penalty`-cost derivation; a new hyphenation language is one `Pyphen(lang=...)` argument value, never a parallel dictionary owner; a new paragraph-item kind is one `Item` `case()` plus its demerit arm; a new layout knob (looseness, emergency-stretch, fitness-class count) is one `LayoutParams` field; a new line fact is one field on `LayoutLine`; zero new surface.
- Boundary: no text shaping (that stays at `typography/shape#SHAPE` — this owner consumes the `PositionedGlyphRun`, never re-shaping), no bidi reorder (that is the `typography/shape#SHAPE` `BIDI` arm, run upstream so the run arrives in visual order), no font engineering (that is `typography/font#FONT`), no PDF authoring (that is `document/emit#DOCUMENT` — this owner hands the `LineBrokenRun` to emission, never drawing a page). The `PARAGRAPH` arm produces the `LineBrokenRun` the `document/emit#DOCUMENT` paragraph emission and the `composition/compose#COMPOSE` placement owners consume, never a parallel layout owner. A greedy first-fit line-break loop is the rejected lower-capability form of the Knuth-Plass total-fit dynamic program — first-fit breaks each line in isolation and cannot minimise the paragraph-wide raggedness/river the total-fit cost minimises, so the owner holds the active-node dynamic program rather than a per-line `while width_left > 0` walk; `Pyphen.inserted(word)` string mangling is the rejected duplicate of the `positions`/`iterate` positional algebra (the soft-hyphen offsets, not a hyphen-spliced string, are what the `Penalty` items carry); a hand-rolled UAX#14 break-class table is the rejected duplicate of `uniseg.linebreak.line_break`; a second per-fitness-class break function and a parallel `_Box`/`_Glue`/`_Penalty` struct family beside the algebra are the collapsed forms — `Item` is the one closed `tagged_union` and the one active-node dynamic program threads every fitness class.

```python signature
from collections.abc import Callable, Iterator
from enum import StrEnum
from math import inf
from types import MappingProxyType
from typing import Final, Literal, assert_never

import msgspec
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary

# RESEARCH: pyphen is installed (`Pyphen(lang=, left=, right=)` / `Pyphen.positions`); uniseg is
# admission-pending, so `uniseg.linebreak.line_break`/`line_break_units` stay signature-locked. The
# positional soft-break offsets (never `Pyphen.inserted` string mangling) feed the Knuth-Plass `Penalty` items.
lazy from pyphen import Pyphen
lazy from uniseg.linebreak import line_break, line_break_units

lazy from artifacts.typography.shape import PositionedGlyphRun

type LayoutAcceptor = Callable[["LineLayout"], bytes]

_INFEASIBLE: Final = 100.0
_FITNESS_CLASSES: Final = 4
_MANDATORY: Final[frozenset[str]] = frozenset({"BK", "CR", "LF", "NL"})
_RUN_ENCODER: Final = msgspec.msgpack.Encoder()


class LayoutOp(StrEnum):
    BREAK = "break"
    HYPHENATE = "hyphenate"
    PARAGRAPH = "paragraph"


class BreakClass(StrEnum):
    MANDATORY = "mandatory"
    OPPORTUNITY = "opportunity"
    PROHIBITED = "prohibited"


@tagged_union(frozen=True)
class Item:
    tag: Literal["box", "glue", "penalty"] = tag()
    box: float = case()
    glue: tuple[float, float, float] = case()
    penalty: tuple[float, float, bool] = case()

    @staticmethod
    def of_box(width: float) -> "Item":
        return Item(box=width)

    @staticmethod
    def of_glue(natural: float, stretch: float, shrink: float) -> "Item":
        return Item(glue=(natural, stretch, shrink))

    @staticmethod
    def of_penalty(cost: float, width: float, flagged: bool) -> "Item":
        return Item(penalty=(cost, width, flagged))


class LayoutLine(Struct, frozen=True):
    start: int
    stop: int
    ratio: float
    advance: float


class LineBrokenRun(Struct, frozen=True):
    lines: tuple[LayoutLine, ...]
    total_demerits: float

    @property
    def line_count(self) -> int:
        return len(self.lines)


class LayoutParams(Struct, frozen=True, kw_only=True):
    text: str = ""
    measure: float = 0.0
    language: str = "en_US"
    left_min: int = 2
    right_min: int = 2
    space_stretch: float = 6.0
    space_shrink: float = 3.0
    hyphen_penalty: float = 50.0
    hyphen_advance: float = 6.0
    flagged_demerit: float = 100.0
    fitness_demerit: float = 100.0
    line_penalty: float = 10.0
    tolerance: float = 10.0


class LineLayout(Struct, frozen=True):
    step: LayoutOp
    run: bytes
    params: LayoutParams

    def lay(self) -> RuntimeRail[ContentKey]:
        return boundary(f"layout.{self.step}", self._emit)

    def _emit(self) -> ContentKey:
        return ContentIdentity.of(f"layout-{self.step}", _LAYOUT_TABLE[self.step](self))


def _break_units(layout: "LineLayout") -> bytes:
    opportunities = tuple(
        BreakClass.MANDATORY if line_break(unit[-1]) in _MANDATORY else BreakClass.OPPORTUNITY
        for unit in line_break_units(layout.params.text)
    )
    return _RUN_ENCODER.encode(tuple(cls.value for cls in opportunities))


def _hyphenate(layout: "LineLayout") -> bytes:
    dictionary = Pyphen(lang=layout.params.language, left=layout.params.left_min, right=layout.params.right_min)
    offsets: dict[str, tuple[int, ...]] = {
        word: tuple(dictionary.positions(word)) for word in layout.params.text.split()
    }
    return _RUN_ENCODER.encode(offsets)


def _badness(ratio: float) -> float:
    return inf if ratio < -1.0 else _INFEASIBLE * abs(ratio) ** 3


def _fitness(ratio: float) -> int:
    if ratio < -0.5:
        return 0
    if ratio <= 0.5:
        return 1
    return 2 if ratio <= 1.0 else 3


def _ratio(natural: float, stretch: float, shrink: float, measure: float) -> float:
    gap = measure - natural
    if gap > 0.0:
        return gap / stretch if stretch > 0.0 else inf
    if gap < 0.0:
        return gap / shrink if shrink > 0.0 else -inf
    return 0.0  # an exactly-fitting line needs no adjustment, even with zero stretch/shrink available


def _classified(char: str) -> BreakClass:
    if char and line_break(char) in _MANDATORY:
        return BreakClass.MANDATORY
    return BreakClass.OPPORTUNITY if char.isspace() else BreakClass.PROHIBITED


def _tokens(text: str) -> Iterator[tuple[int, str]]:
    cursor = 0
    for token in text.split(" "):
        yield cursor, token
        cursor += len(token) + 1


def _soft_breaks(text: str, params: "LayoutParams") -> frozenset[int]:
    dictionary = Pyphen(lang=params.language, left=params.left_min, right=params.right_min)
    return frozenset(start + position for start, token in _tokens(text) for position in dictionary.positions(token))


def _stream(layout: "LineLayout") -> Iterator[tuple[Item, int]]:
    run = msgspec.msgpack.decode(layout.run, type=PositionedGlyphRun).glyphs  # the shape owner encodes the PositionedGlyphRun Struct (a msgpack map); read its `.glyphs` six-tuples, never a bare-tuple decode
    params, text = layout.params, layout.params.text
    soft = _soft_breaks(text, params)
    for index, glyph in enumerate(run):
        cluster, advance = glyph[1], float(glyph[2])
        char = text[cluster] if 0 <= cluster < len(text) else ""
        if cluster in soft:  # flagged soft break carrying the hyphen advance the line pays when it breaks here
            yield Item.of_penalty(params.hyphen_penalty, params.hyphen_advance, True), index
        match _classified(char):
            case BreakClass.MANDATORY:
                yield Item.of_box(advance), index
                yield Item.of_penalty(-inf, 0.0, False), index
            case BreakClass.OPPORTUNITY:  # the inter-word glue IS the breakable opportunity (glue after a box)
                yield Item.of_glue(advance, params.space_stretch, params.space_shrink), index
            case BreakClass.PROHIBITED:
                yield Item.of_box(advance), index
            case _ as unreachable:
                assert_never(unreachable)
    yield Item.of_glue(0.0, inf, 0.0), len(run)        # finishing glue absorbs the last line's slack
    yield Item.of_penalty(-inf, 0.0, False), len(run)  # forced terminal break


def _items(layout: "LineLayout") -> tuple[tuple[Item, ...], tuple[int, ...]]:
    items, origins = zip(*_stream(layout), strict=True)  # parallel item/glyph-origin columns stay in lockstep
    return tuple(items), tuple(origins)


class _Node(Struct, frozen=True):
    position: int   # breakpoint item index; -1 marks the paragraph start
    line: int
    fitness: int
    width: float    # cumulative box/glue extent at this node's line-start
    stretch: float
    shrink: float
    demerits: float
    flagged: bool   # this break landed on a flagged (hyphen) penalty
    ratio: float
    previous: "_Node | None"


def _legal(items: tuple[Item, ...], index: int) -> bool:
    item = items[index]
    if item.tag == "penalty":
        return item.penalty[0] < inf
    return item.tag == "glue" and index > 0 and items[index - 1].tag == "box"


def _line_start(items: tuple[Item, ...], after: int) -> int:
    start = after + 1
    while start < len(items) and items[start].tag == "glue":  # a line never opens on a discarded breakable glue
        start += 1
    return start


def _demerits(ratio: float, item: Item, node: _Node, params: "LayoutParams") -> float:
    badness = _badness(ratio)
    cost = item.penalty[0] if item.tag == "penalty" else 0.0
    flagged = item.tag == "penalty" and item.penalty[2]
    base = (
        (params.line_penalty + badness + cost) ** 2 if cost >= 0.0
        else (params.line_penalty + badness) ** 2 - cost * cost if cost > -inf
        else (params.line_penalty + badness) ** 2
    )
    paired = params.flagged_demerit if flagged and node.flagged else 0.0
    jump = params.fitness_demerit if abs(_fitness(ratio) - node.fitness) > 1 else 0.0
    return base + paired + jump


def _trace(chosen: _Node, items: tuple[Item, ...], origins: tuple[int, ...], width: list[float]) -> tuple[LayoutLine, ...]:
    chain: list[_Node] = []
    node: _Node | None = chosen
    while node is not None and node.position >= 0:
        chain.append(node)
        node = node.previous
    chain.reverse()
    lines: list[LayoutLine] = []
    opener = -1
    for broken in chain:  # Exemption: predecessor-chain walk over the immutable node links the DP threaded
        start = _line_start(items, opener)
        lines.append(LayoutLine(start=origins[start], stop=origins[broken.position], ratio=broken.ratio, advance=width[broken.position] - width[start]))
        opener = broken.position
    return tuple(lines)


def _paragraph(layout: "LineLayout") -> bytes:
    items, origins = _items(layout)
    params, measure, count = layout.params, layout.params.measure, len(items)
    width, stretch, shrink = [0.0] * (count + 1), [0.0] * (count + 1), [0.0] * (count + 1)
    for i, item in enumerate(items):  # Exemption: Knuth-Plass prefix sums — cumulative box/glue extent before each item
        natural, flex, squeeze = item.glue if item.tag == "glue" else (item.box, 0.0, 0.0) if item.tag == "box" else (0.0, 0.0, 0.0)
        width[i + 1], stretch[i + 1], shrink[i + 1] = width[i] + natural, stretch[i] + flex, shrink[i] + squeeze
    opened = _line_start(items, -1)
    active: list[_Node] = [_Node(position=-1, line=0, fitness=1, width=width[opened], stretch=stretch[opened], shrink=shrink[opened], demerits=0.0, flagged=False, ratio=0.0, previous=None)]
    for index, item in enumerate(items):  # Exemption: Knuth-Plass total-fit frontier mutates the active list in place
        if not _legal(items, index):
            continue
        forced = item.tag == "penalty" and item.penalty[0] == -inf
        extra = item.penalty[1] if item.tag == "penalty" else 0.0
        opened = _line_start(items, index)
        best: dict[int, _Node] = {}
        survivors: list[_Node] = []
        for node in active:
            ratio = _ratio(width[index] - node.width + extra, stretch[index] - node.stretch, shrink[index] - node.shrink, measure)
            if ratio >= -1.0 and not forced:
                survivors.append(node)
            if forced or -1.0 <= ratio <= params.tolerance:  # a forced break ends its line at any badness; an optional break qualifies only within tolerance
                fit, total = _fitness(ratio), node.demerits + _demerits(ratio, item, node, params)
                if fit not in best or total < best[fit].demerits:
                    best[fit] = _Node(position=index, line=node.line + 1, fitness=fit, width=width[opened], stretch=stretch[opened], shrink=shrink[opened], demerits=total, flagged=item.tag == "penalty" and item.penalty[2], ratio=ratio, previous=node)
        active = [*survivors, *best.values()] or active
    chosen = min(active, key=lambda node: node.demerits)
    return _RUN_ENCODER.encode(LineBrokenRun(lines=_trace(chosen, items, origins, width), total_demerits=chosen.demerits))


_LAYOUT_TABLE: Final[MappingProxyType[LayoutOp, LayoutAcceptor]] = MappingProxyType({
    LayoutOp.BREAK: _break_units,
    LayoutOp.HYPHENATE: _hyphenate,
    LayoutOp.PARAGRAPH: _paragraph,
})
```

## [03]-[RESEARCH]

- [HYPHENATE] [RESEARCH]: the pyphen dictionary surface — `Pyphen(filename=None, lang=None, left=2, right=2, cache=True)` (with `pyphen.language_fallback` resolving `en_US` -> `en`), `Pyphen.positions(word) -> list[int]` returning the in-word soft-break character offsets, and `Pyphen.iterate(word)` yielding the `(prefix, suffix)` split pairs — is verified live (`0.17.2`, pure-Python; the module exposes `Pyphen.positions`/`iterate`/`inserted`/`wrap` and `pyphen.LANGUAGES`/`language_fallback`) but stays signature-locked because pyphen is admission-pending (no manifest pin, no folder `.api` catalogue). `_soft_breaks` folds each `_tokens` word offset plus its `positions` into the absolute cluster offsets the `_stream` flagged `Penalty(hyphen_penalty, hyphen_advance)` items mark; `Pyphen.inserted(word, hyphen='-')` string mangling and `Pyphen.wrap(word, width)` first-fit splitting are the rejected lower-capability forms (they pre-decide the break or splice a hyphen string rather than exposing the offsets the total-fit DP threads). The `left`/`right` minimum-affix guards (`left_min`/`right_min` `LayoutParams` fields) suppress orphaned 1-2 character hyphen fragments.
- [BREAK] [RESEARCH]: `uniseg.linebreak.line_break(char)` (the per-character UAX#14 break-class string) and `line_break_units(text)` (the break-unit iterator) are fully signature-locked — uniseg is admission-pending and not installed, so the spellings stay marked until uniseg is admitted and the catalogue rows them. `_classified` reads `line_break(char) in _MANDATORY` to mark a hard newline (`BK`/`CR`/`LF`/`NL`) as a forced break, a whitespace cluster as an `OPPORTUNITY` glue, and every other glyph as a `PROHIBITED` in-word box; `_break_units` reuses `line_break_units` for the standalone `BREAK` arm.
- [PARAGRAPH] [RESOLVED]: the Knuth-Plass total-fit line-break is this owner's own algorithm, never a library re-export — `_stream` builds the `Item` Box/Glue/Penalty stream, `_paragraph` threads the `_Node` active frontier over the cumulative width/stretch/shrink prefix sums computing the adjustment ratio, badness, and per-line demerits, deactivating an over-shrunk node (`r < -1`) and keeping the minimum-demerit candidate per fitness class, and `_trace` walks the predecessor chain into multiple `LayoutLine` slices keyed off the parallel glyph-origin column. The `tolerance`/`line_penalty`/`flagged_demerit`/`fitness_demerit`/`hyphen_advance` knobs are typed `LayoutParams` fields; the `msgspec.msgpack` decode reads the upstream `typography/shape#SHAPE` `PositionedGlyphRun` Struct (`type=PositionedGlyphRun`, then its `.glyphs` `(gid, cluster, x_advance, y_advance, x_offset, y_offset)` six-tuples) the shape owner emits — never a bare-tuple decode, which raises `ValidationError` against the map-encoded Struct.
