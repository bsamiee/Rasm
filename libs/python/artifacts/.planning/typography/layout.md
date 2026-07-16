# [PY_ARTIFACTS_LAYOUT]

`LineLayout` is the locale-aware paragraph line-layout owner over the document rail — one owner folding a shaped `typography/shape#SHAPE` `PositionedGlyphRun` and a measure through a closed `LayoutOp` family into line-broken paragraph runs, cell-width measures, and locale-collated indices. Break resolution is engine-polymorphic: `SegmentEngine.UNISEG` runs uniseg's locale-free UAX#14 default-table break, `SegmentEngine.ICU` runs PyICU's CLDR-tailored dictionary-backed `BreakIterator` so a spaceless Thai/Lao/Khmer/Burmese/CJK run breaks at lexical boundaries the default table cannot find. pyphen inserts soft-hyphenation opportunities, every break/hyphen offset reconciled onto the UAX#29 grapheme boundaries so no break lands mid-cluster, and a hand-rolled Knuth-Plass total-fit program encodes the run as a Box/Glue/Penalty item stream — reading the shaped run's HarfBuzz break-safety column so an `unsafe_to_break` cluster is never a legal breakpoint and a `tatweel_points` cluster carries elastic kashida — finding the globally optimal breakpoint set by a badness/demerits program.

`pyphen`, `uniseg`, and `PyICU` are admitted manifest rows; the Knuth-Plass item algebra is this owner's own algorithm, never a library re-export. `LineBrokenRun` is the value object the paragraph arm carries, surfaced by the public `LineLayout.broken()` projection beside `lay()` so `drawing/annotate#ANNOTATE` `TextNote.Prose`, `document/emit#DOCUMENT` paragraph emission, and `composition/compose#COMPOSE` placement drive the total-fit break from a shaped run rather than a pre-computed payload; `COLLATE`'s sort keys and index buckets are the locale-correct order `drawing/schedule`, `specification/classify`, and `visualization/table` read. Native ICU4C arms ride the gated worker seam; the pure-Python uniseg/pyphen segmentation and the bounded per-paragraph Knuth-Plass DP run inline.

## [01]-[INDEX]

- [01]-[LAYOUT]: the uniseg/PyICU line-break, pyphen hyphenation, east-asian measure, PyICU collation, and hand-rolled Knuth-Plass total-fit paragraph owner over the closed `LayoutOp` step table — break resolution the `SegmentEngine` discriminant and the paragraph item stream one closed `Item` `tagged_union`.

## [02]-[LAYOUT]

- Owner: `LineLayout` folds `(step, run, params)` — the shaped `PositionedGlyphRun` and a `LayoutParams` measure — through the `LayoutOp` step table; `SegmentEngine` discriminates the break engine, the `_native` gate crossing the ICU arms onto the gated worker while the pure arms run inline; `Item` is the one closed Box/Glue/Penalty `tagged_union`, and `LineBrokenRun` the line-broken value object the public `broken()` projection returns beside `lay()`.
- Cases: `LayoutOp` rows — `BREAK` (engine-polymorphic UAX#14 opportunity resolution, both engines reconciled onto `grapheme_cluster_boundaries` so a break never lands mid-cluster), `HYPHENATE` (pyphen soft-hyphenation over uniseg word tokens, each `positions(word)` `DataInt` carrying the offset plus the `.data` `(change, index, cut)` orthographic-mutation channel — never `Pyphen.inserted` string mangling), `PARAGRAPH` (the Knuth-Plass total-fit break, `_stream` encoding the shaped run's break-safety and justification classes into the `Item` stream and the active-node program tracing the optimal breakpoint set into a `LineBrokenRun`), `MEASURE` (the cumulative east-asian cell-width prefix a CJK-correct or monospace column needs, distinct from the advance-based proportional measure), `COLLATE` (`Collator` sort keys + `AlphabeticIndex` bucketing, the collation seam schedule/classify/table read). Measure, elasticity, penalty, and demerit weights are typed `LayoutParams` fields.
- Entry: `emit()` returns the one `ArtifactWork` keyed pre-run over `(step ⊕ run ⊕ params)`, the run payload the primary bytes-producing input; `_emit` maps the laid bytes onto `ArtifactReceipt.Document`; the `_native` gate offloads the ICU arms to the process lane while the uniseg/pyphen/DP arms fold inline, both inside one span.
- Auto: `_dictionary`/`_soft_breaks` guard the unbundled locale as a rail fault, never a bare `KeyError`; `_broken` keeps the minimum-total-demerit candidate per fitness class, and `_trace` indexes each `LayoutLine` slice through the parallel glyph-origin column so a line's `start`/`stop` name shaped-run glyphs, not `Item` indices.
- Receipt: every arm contributes `ArtifactReceipt.Document` carrying the content key and encoded output byte count (the opportunity stream, soft-break map, `LineBrokenRun`, cell-width prefix, or sort-key/bucket set). Line count, per-line adjustment ratios, total-demerit cost, engine, ICU/Unicode data version, and bucket count stay interior evidence in the content-key derivation, never new `Document` fields.
- Packages: `pyphen` (`Pyphen`/`positions`/`iterate`/`language_fallback`), `uniseg` (`line_break_units`/`words`/`grapheme_cluster_boundaries`/`tt_text_extents`), `PyICU` (`BreakIterator`/`Collator`/`AlphabeticIndex`/`Bidi`), `core/receipt#RECEIPT` (`ArtifactReceipt.Document`, composed never re-declared); the Knuth-Plass item algebra is this owner's own.
- Growth: a new break engine is one `SegmentEngine` member plus one `_break`/`_paragraph` branch; a new locale tailor one `_TAILOR_TABLE` row threaded by language; a new hyphenation language one `Pyphen(lang=)` value `language_fallback` resolves; a new paragraph-item kind one `Item` `case()` plus its demerit arm; a new layout knob one `LayoutParams` field; a new break-safety read one `PositionedGlyphRun` column `_stream` reads; a new line fact one `LayoutLine` field; a new collation axis one `COLLATE` line.
- Boundary: no text shaping or bidi reorder (`typography/shape#SHAPE` — this owner consumes the `PositionedGlyphRun` in visual order, never re-shaping), no font engineering (`typography/font#FONT`), no PDF authoring (`document/emit#DOCUMENT`). A greedy first-fit line-break, `uniseg.wrap.tt_wrap` (the monospace-terminal form), `Pyphen.inserted`/`wrap` string mangling, a hand-rolled UAX#14 break-class table, a re-implemented UCA collation or CLDR segmentation, and `text.split()` tokenization are each rejected against the library op or the total-fit program that owns them; a break landing mid-cluster is deleted by the `grapheme_cluster_boundaries` reconciliation. `Item` is the one closed `tagged_union` and one active-node program threads every fitness class, never a parallel `_Box`/`_Glue`/`_Penalty` family or a per-fitness-class break function.

```python signature
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import os
from collections.abc import Callable, Iterator, Mapping
from enum import StrEnum
from math import inf
from typing import Final, Literal, assert_never

import msgspec
import structlog
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

# pyphen/uniseg are pure-Python and run inline; PyICU native ICU4C is reified only inside the gated worker.
lazy from pyphen import Pyphen, language_fallback
lazy from uniseg.graphemecluster import grapheme_cluster_boundaries
lazy from uniseg.linebreak import line_break, line_break_units
lazy from uniseg.unicodedatawrapper import east_asian_width
lazy from uniseg.wordbreak import words
lazy from uniseg.wrap import tt_text_extents
lazy from icu import AlphabeticIndex, BreakIterator, Collator, Locale

lazy from artifacts.typography.shape import PositionedGlyphRun

# --- [TYPES] ---------------------------------------------------------------------------

type LayoutAcceptor = Callable[["LineLayout"], bytes]
type TailorFunction = Callable[[str, object], object]
type Opportunity = tuple[int, "BreakClass"]

# --- [CONSTANTS] -----------------------------------------------------------------------

_INFEASIBLE: Final = 100.0
_FITNESS_CLASSES: Final = 4
_ICU_LINE_HARD: Final = 100  # ICU getRuleStatus UBRK_LINE_HARD floor: >= is a mandatory break
_MANDATORY: Final[frozenset[str]] = frozenset({"BK", "CR", "LF", "NL"})
_WIDE: Final[frozenset[str]] = frozenset({"W", "F"})  # East_Asian_Width wide/fullwidth -> a CJK ideograph boundary
_RUN_ENCODER: Final = msgspec.msgpack.Encoder()
_LOG: Final = structlog.get_logger()
_TRACER: Final = trace.get_tracer(__name__)
# locale break tailors land as rows, threaded into the uniseg `tailor=` hook by language.
_TAILOR_TABLE: Final[Map[str, TailorFunction]] = Map.empty()

# --- [MODELS] --------------------------------------------------------------------------


class LayoutOp(StrEnum):
    BREAK = "break"
    HYPHENATE = "hyphenate"
    PARAGRAPH = "paragraph"
    MEASURE = "measure"
    COLLATE = "collate"


class SegmentEngine(StrEnum):
    UNISEG = "uniseg"  # locale-free UAX#14 default table
    ICU = "icu"  # CLDR-tailored dictionary-backed ICU4C


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
    engine: SegmentEngine = SegmentEngine.UNISEG
    left_min: int = 2
    right_min: int = 2
    space_stretch: float = 6.0
    space_shrink: float = 3.0
    hyphen_penalty: float = 50.0
    hyphen_advance: float = 6.0
    ideograph_penalty: float = 25.0  # a CJK-ideograph break weighted vs a Latin-space break
    kashida_stretch: float = 12.0  # the elastic elongation a `SAFE_TO_INSERT_TATWEEL` cluster admits for Arabic kashida justification
    flagged_demerit: float = 100.0
    fitness_demerit: float = 100.0
    line_penalty: float = 10.0
    tolerance: float = 10.0
    ambiguous_wide: bool = False  # MEASURE: resolve East-Asian ambiguous width as wide


class LineLayout(Struct, frozen=True):
    step: LayoutOp
    run: bytes
    params: LayoutParams

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key over (step ⊕ run ⊕ params), the run payload the primary bytes-producing input.
        return ContentIdentity.of(f"layout-{self.step}", (self.step, self.run, self.params), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        return (await async_boundary(f"layout.{self.step}", self._laid)).map(
            lambda data: ArtifactReceipt.Document(self._key, len(data))
        )

    async def _laid(self) -> bytes:
        acceptor = _LAYOUT_TABLE[self.step]
        with _TRACER.start_as_current_span(f"layout.{self.step}") as span:
            span.set_attributes({"step": self.step, "engine": self.params.engine, "native": self._native})
            data = (
                (await LanePolicy.offload(acceptor, self, modality=Modality.PROCESS, retry=RetryClass.OCCT)).default_with(_layout_raise)
                if self._native
                else acceptor(self)
            )
        _LOG.info("layout.emit", step=self.step, engine=self.params.engine, bytes=len(data))
        return data

    @property
    def _native(self) -> bool:
        return self.step is LayoutOp.COLLATE or (self.step in (LayoutOp.BREAK, LayoutOp.PARAGRAPH) and self.params.engine is SegmentEngine.ICU)

    def broken(self) -> LineBrokenRun:
        # the public Knuth-Plass projection beside `lay()`: annotate/emit/compose drive the total-fit break from a shaped run (inline pure-Python DP).
        return _broken(self)


class _Node(Struct, frozen=True):
    position: int  # breakpoint item index; -1 marks the paragraph start
    line: int
    fitness: int
    width: float  # cumulative box/glue extent at this node's line-start
    stretch: float
    shrink: float
    demerits: float
    flagged: bool  # this break landed on a flagged (hyphen) penalty
    ratio: float
    previous: "_Node | None"


# --- [OPERATIONS] ----------------------------------------------------------------------


def _uniseg_breaks(params: "LayoutParams") -> tuple[Opportunity, ...]:
    text, tailor = params.text, _TAILOR_TABLE.try_find(params.language).default_value(None)
    boundaries, position, out = frozenset(grapheme_cluster_boundaries(text)), 0, []
    for unit in line_break_units(text, tailor=tailor):  # the tailorable UAX#14 segment stream, never text.split()
        position += len(unit)
        cls = BreakClass.MANDATORY if line_break(unit[-1]) in _MANDATORY else BreakClass.OPPORTUNITY
        if position in boundaries:  # a break never lands mid-cluster
            out.append((position, cls))
    return tuple(out)


def _icu_breaks(params: "LayoutParams") -> tuple[Opportunity, ...]:
    iterator = BreakIterator.createLineInstance(Locale(params.language))
    iterator.setText(params.text)
    boundaries, out = frozenset(grapheme_cluster_boundaries(params.text)), []
    iterator.first()
    while (position := iterator.next()) != BreakIterator.DONE:  # Exemption: the ICU iterator is a stateful native cursor
        cls = BreakClass.MANDATORY if iterator.getRuleStatus() >= _ICU_LINE_HARD else BreakClass.OPPORTUNITY
        if position in boundaries:
            out.append((position, cls))
    return tuple(out)


def _breaks(params: "LayoutParams") -> tuple[Opportunity, ...]:
    return _icu_breaks(params) if params.engine is SegmentEngine.ICU else _uniseg_breaks(params)


def _layout_raise(fault: object) -> bytes:
    raise ValueError(str(fault))


def _break(layout: "LineLayout") -> bytes:
    return _RUN_ENCODER.encode(tuple((position, cls.value) for position, cls in _breaks(layout.params)))


def _dictionary(params: "LayoutParams") -> object:
    resolved = language_fallback(params.language)  # en_Latn_US -> en; None on an unbundled locale
    if resolved is None:
        raise KeyError(f"no bundled hyphenation dictionary for {params.language!r}")
    return Pyphen(lang=resolved, left=params.left_min, right=params.right_min)


def _hyphenate(layout: "LineLayout") -> bytes:
    dictionary, result = _dictionary(layout.params), {}
    for word in words(layout.params.text):  # uniseg word tokens, never text.split()
        if positions := dictionary.positions(word):
            result[word] = (
                tuple(int(offset) for offset in positions),  # the Knuth-Plass Penalty positions
                tuple(offset.data for offset in positions),  # the DataInt (change, index, cut) orthographic channel
                tuple(dictionary.iterate(word)),  # the (head, tail) split the hyphen advance measures
            )
    return _RUN_ENCODER.encode(result)


def _soft_breaks(params: "LayoutParams") -> frozenset[int]:
    dictionary, boundaries = _dictionary(params), frozenset(grapheme_cluster_boundaries(params.text))
    cursor, breaks = 0, set()
    for word in words(params.text):
        breaks.update(cursor + int(offset) for offset in dictionary.positions(word) if cursor + int(offset) in boundaries)
        cursor += len(word)
    return frozenset(breaks)


def _ideographic(char: str) -> bool:
    return bool(char) and east_asian_width(char) in _WIDE


def _stream(layout: "LineLayout", opportunities: Mapping[int, BreakClass], soft: frozenset[int]) -> Iterator[tuple[Item, int]]:
    positioned = msgspec.msgpack.decode(layout.run, type=PositionedGlyphRun)  # the shape owner's PositionedGlyphRun, never a bare-tuple decode
    run, unsafe, tatweel = positioned.glyphs, positioned.unsafe_to_break, frozenset(positioned.tatweel_points)
    # unsafe = UNSAFE_TO_BREAK clusters the DP refuses a break inside; tatweel = SAFE_TO_INSERT_TATWEEL kashida-stretch points.
    params, text = layout.params, layout.params.text
    for index, glyph in enumerate(run):
        cluster, advance = glyph[1], float(glyph[2])
        char = text[cluster] if 0 <= cluster < len(text) else ""
        if cluster in soft:  # flagged soft break, already reconciled onto a grapheme boundary
            yield Item.of_penalty(params.hyphen_penalty, params.hyphen_advance, True), index
        match opportunities.get(cluster, BreakClass.PROHIBITED):
            case BreakClass.MANDATORY:
                yield Item.of_box(advance), index
                yield Item.of_penalty(-inf, 0.0, False), index
            case BreakClass.OPPORTUNITY if cluster in unsafe:  # HarfBuzz refuses a safe break inside this cluster: keep it an unbreakable box
                yield Item.of_box(advance), index
            case BreakClass.OPPORTUNITY if _ideographic(char):  # CJK boundary: weighted zero-width penalty, no inter-ideograph glue
                yield Item.of_box(advance), index
                yield Item.of_penalty(params.ideograph_penalty, 0.0, False), index
            case BreakClass.OPPORTUNITY:  # the inter-word glue IS the breakable opportunity
                yield Item.of_glue(advance, params.space_stretch, params.space_shrink), index
            case BreakClass.PROHIBITED if (
                cluster in tatweel
            ):  # kashida: an inf-penalty forbids a break so the trailing glue stretches without opening a line
                yield Item.of_box(advance), index
                yield Item.of_penalty(inf, 0.0, False), index
                yield Item.of_glue(0.0, params.kashida_stretch, 0.0), index
            case BreakClass.PROHIBITED:
                yield Item.of_box(advance), index
            case _ as unreachable:
                assert_never(unreachable)
    yield Item.of_glue(0.0, inf, 0.0), len(run)  # finishing glue absorbs the last line's slack
    yield Item.of_penalty(-inf, 0.0, False), len(run)  # forced terminal break


def _items(layout: "LineLayout", opportunities: Mapping[int, BreakClass], soft: frozenset[int]) -> tuple[tuple[Item, ...], tuple[int, ...]]:
    items, origins = zip(*_stream(layout, opportunities, soft), strict=True)  # parallel item/glyph-origin columns stay in lockstep
    return tuple(items), tuple(origins)


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
        (params.line_penalty + badness + cost) ** 2
        if cost >= 0.0
        else (params.line_penalty + badness) ** 2 - cost * cost
        if cost > -inf
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
        lines.append(
            LayoutLine(start=origins[start], stop=origins[broken.position], ratio=broken.ratio, advance=width[broken.position] - width[start])
        )
        opener = broken.position
    return tuple(lines)


def _broken(layout: "LineLayout") -> LineBrokenRun:
    params = layout.params
    opportunities = {position: cls for position, cls in _breaks(params)}
    items, origins = _items(layout, opportunities, _soft_breaks(params))
    measure, count = params.measure, len(items)
    width, stretch, shrink = [0.0] * (count + 1), [0.0] * (count + 1), [0.0] * (count + 1)
    for i, item in enumerate(items):  # Exemption: Knuth-Plass prefix sums — cumulative box/glue extent before each item
        natural, flex, squeeze = item.glue if item.tag == "glue" else (item.box, 0.0, 0.0) if item.tag == "box" else (0.0, 0.0, 0.0)
        width[i + 1], stretch[i + 1], shrink[i + 1] = width[i] + natural, stretch[i] + flex, shrink[i] + squeeze
    opened = _line_start(items, -1)
    active: list[_Node] = [
        _Node(
            position=-1,
            line=0,
            fitness=1,
            width=width[opened],
            stretch=stretch[opened],
            shrink=shrink[opened],
            demerits=0.0,
            flagged=False,
            ratio=0.0,
            previous=None,
        )
    ]
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
            if forced or -1.0 <= ratio <= params.tolerance:  # a forced break ends its line at any badness; an optional break only within tolerance
                fit, total = _fitness(ratio), node.demerits + _demerits(ratio, item, node, params)
                if fit not in best or total < best[fit].demerits:
                    best[fit] = _Node(
                        position=index,
                        line=node.line + 1,
                        fitness=fit,
                        width=width[opened],
                        stretch=stretch[opened],
                        shrink=shrink[opened],
                        demerits=total,
                        flagged=item.tag == "penalty" and item.penalty[2],
                        ratio=ratio,
                        previous=node,
                    )
        active = [*survivors, *best.values()] or active
    chosen = min(active, key=lambda node: node.demerits)
    return LineBrokenRun(lines=_trace(chosen, items, origins, width), total_demerits=chosen.demerits)


def _paragraph(layout: "LineLayout") -> bytes:
    return _RUN_ENCODER.encode(_broken(layout))


def _measure(layout: "LineLayout") -> bytes:
    return _RUN_ENCODER.encode(tuple(tt_text_extents(layout.params.text, ambiguous_as_wide=layout.params.ambiguous_wide)))


def _collate(layout: "LineLayout") -> bytes:
    locale = Locale(layout.params.language)
    collator = Collator.createInstance(locale)
    items = tuple(line for line in layout.params.text.splitlines() if line)
    ordered = sorted(items, key=collator.getSortKey)  # the stable locale-correct sort key, not compare
    index, buckets = AlphabeticIndex(locale), []
    for item in ordered:
        index.addRecord(item, None)  # Exemption: the ICU AlphabeticIndex is a stateful native bucketer
    while index.nextBucket():
        buckets.append(index.getBucketLabel())
    return _RUN_ENCODER.encode({"order": ordered, "keys": tuple(collator.getSortKey(item).hex() for item in ordered), "buckets": tuple(buckets)})


_LAYOUT_TABLE: Final[Map[LayoutOp, LayoutAcceptor]] = Map.of_seq([
    (LayoutOp.BREAK, _break),
    (LayoutOp.HYPHENATE, _hyphenate),
    (LayoutOp.PARAGRAPH, _paragraph),
    (LayoutOp.MEASURE, _measure),
    (LayoutOp.COLLATE, _collate),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
