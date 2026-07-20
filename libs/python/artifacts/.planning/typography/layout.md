# [PY_ARTIFACTS_LAYOUT]

`LineLayout` owns locale-aware paragraph layout over the document rail. One closed `LayoutRequest` family gives each arm only its consumed state, `SegmentEngine` selects uniseg's locale-free UAX #14 table or PyICU's CLDR-tailored dictionary segmentation, and `CollationPolicy` projects the UCA strength, case, normalization, numeric, and alternate-handling axes onto one ICU collator. pyphen contributes every legal soft break with its realized `(head, tail)` orthographic substitution. A Knuth-Plass total-fit program lowers HarfBuzz cluster groups into one `Item` stream, rejects every `unsafe_to_break` edge, admits `tatweel_points` as elastic kashida, and minimizes total demerits across the paragraph.

Break boundaries and HarfBuzz clusters normalize onto code-point indices. `_icu_breaks` converts ICU's UTF-16 offsets before grapheme reconciliation, `_clusters` preserves each contiguous glyph cluster as one indivisible span, RTL reverses cluster groups rather than glyphs, and `_stream` reads break evidence at each group's trailing edge. `LayoutLine` carries both the shaped-run glyph span and source-text span, while `LineEnding.hyphenated` carries the selected pyphen substitution without forcing downstream reconstruction. `broken(spec)` exposes the same total-fit value used by `emit()`. Native ICU4C arms cross the process lane; the pure uniseg/pyphen folds — the input-scaled Knuth-Plass fit included — cross the own-GIL interpreter lane, so no synchronous fold ever runs on the event loop.

## [01]-[INDEX]

- [01]-[LAYOUT]: uniseg/PyICU line breaking, pyphen hyphenation, east-asian measure, PyICU collation, and Knuth-Plass total fit over one closed `LayoutRequest` family.

## [02]-[LAYOUT]

- Owner: `LineLayout` folds one `LayoutRequest` through the `_LAYOUT_TABLE`; `SegmentEngine` discriminates the break engine, `_trait` keying the ICU arms onto the gated process worker as `HOSTILE` kernels and every pure arm onto the own-GIL interpreter substrate as `PURE`; `Item` is the one closed Box/Glue/Penalty `tagged_union`, and `LineBrokenRun` the line-broken value object the public `broken(spec)` projection returns.
- Cases: `breaks` resolves grapheme-safe UAX #14 opportunities; `hyphenate` returns absolute `HyphenBreak` rows with realized standard or orthographic splits; `paragraph` fits one shaped run under `FitPolicy`; `measure` returns the east-asian cell-width prefix; `collate` returns sort keys and locale buckets under `CollationPolicy`.
- Entry: `emit()` mints one request key, captures it into `_emit`, and returns one `ArtifactWork`; every arm crosses `self.lane.offload` as a `Kernel` on its `_trait` row, the trait row's worker-death retry the only re-run — the fold is deterministic, so a raise is a defect the lane converts once.
- Auto: `_dictionary` raises `LookupError` for an unbundled locale; `_clusters` groups every equal HarfBuzz cluster before RTL reversal; `_stream` lowers each group once; `_trace` derives glyph and source spans from `_Cluster`; `broken` retains the minimum-demerit node per fitness class.
- Receipt: every arm contributes `ArtifactReceipt.Document` with the content key and encoded byte count. `_data_versions` adds only the provider data releases the selected arm reads: bundled uniseg segmentation data, interpreter width data, pyphen dictionaries, or linked ICU/Unicode data.
- Packages: `pyphen` (`Pyphen`/`positions`/`iterate`/`language_fallback`/`VERSION`), `uniseg` (`line_break_units`/`line_break`/`words`/`grapheme_cluster_boundaries`/`tt_text_extents`/`east_asian_width`/`unidata_version`), `PyICU` (`BreakIterator`/`Collator`/the full `UCollAttribute` matrix/`AlphabeticIndex`/`ICU_VERSION`/`UNICODE_VERSION`), and `core/receipt#RECEIPT` (`ArtifactReceipt.Document`).
- Growth: break engines extend `SegmentEngine`; locale tailors extend `_TAILOR_TABLE`; item kinds extend `Item`; fit decisions extend `FitPolicy`; UCA decisions extend `CollationPolicy`; line evidence extends `LayoutLine`.
- Boundary: shaping and bidi resolution stay in `typography/shape#SHAPE`; font engineering stays in `typography/font#FONT`; authoring stays in `document/emit#DOCUMENT`. Greedy first fit, `tt_wrap` for proportional text, `Pyphen.inserted`, `Pyphen.wrap`, local Unicode or UCA tables, scalar-glyph breaking, and `text.split()` tokenization are rejected forms.

```python signature
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from collections.abc import Callable, Iterator, Mapping
from builtins import frozendict
from enum import StrEnum
from functools import partial
from itertools import groupby
from math import inf
from typing import Final, Literal, assert_never

import msgspec
import structlog
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

lazy from pyphen import VERSION as PYPHEN_VERSION, Pyphen, language_fallback
lazy from uniseg import unidata_version as SEGMENT_UNICODE_VERSION
lazy from uniseg.breaking import TailorFunction
lazy from uniseg.graphemecluster import grapheme_cluster_boundaries
lazy from uniseg.linebreak import line_break, line_break_units
lazy from uniseg.unicodedatawrapper import east_asian_width, unidata_version as WIDTH_UNICODE_VERSION
lazy from uniseg.wordbreak import words
lazy from uniseg.wrap import tt_text_extents
lazy from icu import ICU_VERSION, UNICODE_VERSION, AlphabeticIndex, BreakIterator, Collator, Locale, UCollAttribute, UCollAttributeValue

lazy from rasm.artifacts.typography.shape import PositionedGlyphRun

# --- [TYPES] ---------------------------------------------------------------------------

type LayoutAcceptor = Callable[["LayoutRequest"], bytes]
type Opportunity = tuple[int, "BreakClass"]
type LayoutTag = Literal["breaks", "hyphenate", "paragraph", "measure", "collate"]

# --- [CONSTANTS] -----------------------------------------------------------------------

_INFEASIBLE: Final = 100.0
_ICU_LINE_HARD: Final = 100  # ICU getRuleStatus UBRK_LINE_HARD floor: >= is a mandatory break
_MANDATORY: Final[frozenset[str]] = frozenset({"BK", "CR", "LF", "NL"})
_WIDE: Final[frozenset[str]] = frozenset({"W", "F"})
_CANON: Final = msgspec.msgpack.Encoder(order="deterministic")  # the stable preimage encoding the bare `ContentIdentity.key` mint addresses
_RUN_ENCODER: Final = msgspec.msgpack.Encoder()
_LOG: Final = structlog.get_logger()
_TRACER: Final = trace.get_tracer(__name__)
_TAILOR_TABLE: Final[Map[str, TailorFunction]] = Map.empty()

# --- [MODELS] --------------------------------------------------------------------------


class LayoutOp(StrEnum):
    # values are the LayoutRequest tag strings, so `LayoutOp(request.tag)` is the whole projection — no parallel table.
    BREAK = "breaks"
    HYPHENATE = "hyphenate"
    PARAGRAPH = "paragraph"
    MEASURE = "measure"
    COLLATE = "collate"


class SegmentEngine(StrEnum):
    UNISEG = "uniseg"
    ICU = "icu"


class BreakClass(StrEnum):
    MANDATORY = "mandatory"
    OPPORTUNITY = "opportunity"
    PROHIBITED = "prohibited"


class CollationStrength(StrEnum):
    PRIMARY = "primary"
    SECONDARY = "secondary"
    TERTIARY = "tertiary"
    QUATERNARY = "quaternary"
    IDENTICAL = "identical"


class CaseFirst(StrEnum):
    OFF = "off"
    LOWER = "lower"
    UPPER = "upper"


# CollationStrength/AlternateHandling member names equal their UCollAttributeValue spellings; only the case-first axis renames.
_UCA_CASE: Final[Map[CaseFirst, str]] = Map.of_seq([(CaseFirst.OFF, "OFF"), (CaseFirst.LOWER, "LOWER_FIRST"), (CaseFirst.UPPER, "UPPER_FIRST")])


class AlternateHandling(StrEnum):
    NON_IGNORABLE = "non_ignorable"
    SHIFTED = "shifted"


class HyphenBreak(Struct, frozen=True):
    position: int
    source_start: int
    source_stop: int
    head: str
    tail: str


@tagged_union(frozen=True)
class LineEnding:
    tag: Literal["natural", "hyphenated"] = tag()
    natural: int = case()
    hyphenated: HyphenBreak = case()


@tagged_union(frozen=True)
class Item:
    tag: Literal["box", "glue", "penalty"] = tag()
    box: float = case()
    glue: tuple[float, float, float] = case()
    penalty: tuple[float, float, bool, LineEnding] = case()

    @staticmethod
    def of_box(width: float) -> "Item":
        return Item(box=width)

    @staticmethod
    def of_glue(natural: float, stretch: float, shrink: float) -> "Item":
        return Item(glue=(natural, stretch, shrink))

    @staticmethod
    def of_penalty(cost: float, width: float, flagged: bool, ending: LineEnding) -> "Item":
        return Item(penalty=(cost, width, flagged, ending))


class BreakSpec(Struct, frozen=True, kw_only=True):
    text: str
    language: str = "en_US"
    engine: SegmentEngine = SegmentEngine.UNISEG


class HyphenSpec(Struct, frozen=True, kw_only=True):
    text: str
    language: str = "en_US"
    left_min: int = 2
    right_min: int = 2


class FitPolicy(Struct, frozen=True, kw_only=True):
    space_stretch: float = 6.0
    space_shrink: float = 3.0
    hyphen_penalty: float = 50.0
    hyphen_advance: float = 6.0
    ideograph_penalty: float = 25.0
    kashida_stretch: float = 12.0
    flagged_demerit: float = 100.0
    fitness_demerit: float = 100.0
    line_penalty: float = 10.0
    tolerance: float = 10.0


class CollationPolicy(Struct, frozen=True, kw_only=True):
    strength: CollationStrength = CollationStrength.TERTIARY
    case_first: CaseFirst = CaseFirst.OFF
    alternate: AlternateHandling = AlternateHandling.NON_IGNORABLE
    normalization: bool = True
    numeric: bool = False


class ParagraphSpec(Struct, frozen=True, kw_only=True):
    run: bytes
    measure: float
    language: str = "en_US"
    engine: SegmentEngine = SegmentEngine.UNISEG
    left_min: int = 2
    right_min: int = 2
    fit: FitPolicy = FitPolicy()


class MeasureSpec(Struct, frozen=True, kw_only=True):
    text: str
    ambiguous_wide: bool = False


class CollateSpec(Struct, frozen=True, kw_only=True):
    items: tuple[str, ...]
    language: str = "en_US"
    policy: CollationPolicy = CollationPolicy()


@tagged_union(frozen=True)
class LayoutRequest:
    tag: LayoutTag = tag()
    breaks: BreakSpec = case()
    hyphenate: HyphenSpec = case()
    paragraph: ParagraphSpec = case()
    measure: MeasureSpec = case()
    collate: CollateSpec = case()

    @property
    def op(self) -> LayoutOp:
        return LayoutOp(self.tag)


class LayoutLine(Struct, frozen=True):
    start: int
    stop: int
    source_start: int
    source_stop: int
    ratio: float
    advance: float
    ending: LineEnding


class LineBrokenRun(Struct, frozen=True):
    lines: tuple[LayoutLine, ...]
    total_demerits: float

    @property
    def line_count(self) -> int:
        return len(self.lines)


class LineLayout(Struct, frozen=True):
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    request: LayoutRequest
    lane: LanePolicy

    def emit(self, /) -> ArtifactWork:
        key = self._key
        return ArtifactWork(key=key, work=partial(self._emit, key), parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # `ContentIdentity.key` is the bare mint (`of` returns the railed `RuntimeRail[ContentKey]`).
        return ContentIdentity.key(f"layout-{self.request.tag}", _CANON.encode((self.request, self._data_versions)))

    @property
    def _data_versions(self) -> tuple[str, ...]:
        match self.request:
            case LayoutRequest(tag="breaks", breaks=spec):
                return (ICU_VERSION, UNICODE_VERSION) if spec.engine is SegmentEngine.ICU else (SEGMENT_UNICODE_VERSION,)
            case LayoutRequest(tag="hyphenate"):
                return (SEGMENT_UNICODE_VERSION, PYPHEN_VERSION)
            case LayoutRequest(tag="paragraph", paragraph=spec):
                segment = (ICU_VERSION, UNICODE_VERSION) if spec.engine is SegmentEngine.ICU else (SEGMENT_UNICODE_VERSION,)
                return (*segment, PYPHEN_VERSION)
            case LayoutRequest(tag="measure"):
                return (WIDTH_UNICODE_VERSION,)
            case LayoutRequest(tag="collate"):
                return (ICU_VERSION, UNICODE_VERSION)
            case _ as unreachable:
                assert_never(unreachable)

    async def _emit(self, key: ContentKey, /) -> RuntimeRail[ArtifactReceipt]:
        acceptor, trait = _LAYOUT_TABLE[self.request.op], self._trait
        with _TRACER.start_as_current_span(f"layout.{self.request.tag}") as span:
            span.set_attributes({"step": self.request.tag, "trait": trait.value})
            crossed = await self.lane.offload(Kernel.of(acceptor, trait), self.request)
            # egress fold closes inside the span scope: the Error arm marks ERROR and logs correlated, the Ok path stays silent.
            return crossed.map(lambda data: ArtifactReceipt.Document(key, len(data))).map_error(partial(_faulted, span, self.request.tag))

    @property
    def _trait(self) -> KernelTrait:
        # ICU4C arms cross the gated process worker as HOSTILE; the pure uniseg/pyphen folds — the input-scaled
        # Knuth-Plass paragraph fit included — take the own-GIL interpreter substrate as PURE, so no synchronous
        # fold ever runs on the loop.
        match self.request:
            case LayoutRequest(tag="collate"):
                return KernelTrait.HOSTILE
            case (
                LayoutRequest(tag="breaks", breaks=spec) | LayoutRequest(tag="paragraph", paragraph=spec)
            ) if spec.engine is SegmentEngine.ICU:
                return KernelTrait.HOSTILE
            case _:
                return KernelTrait.PURE


class _Node(Struct, frozen=True):
    position: int
    line: int
    fitness: int
    width: float
    stretch: float
    shrink: float
    demerits: float
    flagged: bool
    ratio: float
    ending: LineEnding
    previous: "_Node | None"


class _Cluster(Struct, frozen=True):
    glyph_start: int
    glyph_stop: int
    source_start: int
    source_stop: int
    advance: float


# --- [OPERATIONS] ----------------------------------------------------------------------


def _faulted(span: trace.Span, step: str, fault: BoundaryFault, /) -> BoundaryFault:
    span.set_status(Status(StatusCode.ERROR, fault.tag))
    _LOG.error("layout.emit", step=step, **fault.facts())
    return fault


def _uniseg_breaks(text: str, language: str, /) -> tuple[Opportunity, ...]:
    tailor = _TAILOR_TABLE.try_find(language).default_value(None)
    boundaries, position, out = frozenset(grapheme_cluster_boundaries(text)), 0, []
    for unit in line_break_units(text, tailor=tailor):
        position += len(unit)
        cls = BreakClass.MANDATORY if line_break(unit[-1]) in _MANDATORY else BreakClass.OPPORTUNITY
        if position in boundaries:
            out.append((position, cls))
    return tuple(out)


def _icu_breaks(text: str, language: str, /) -> tuple[Opportunity, ...]:
    iterator = BreakIterator.createLineInstance(Locale(language))
    iterator.setText(text)
    boundaries = frozenset(grapheme_cluster_boundaries(text))
    units, offsets = 0, {0: 0}
    for index, char in enumerate(text, start=1):
        units += 2 if ord(char) > 0xFFFF else 1
        offsets[units] = index
    out = []
    iterator.first()
    while (utf16_position := iterator.next()) != BreakIterator.DONE:  # Exemption: the ICU iterator is a stateful native cursor
        position = offsets[utf16_position]
        cls = BreakClass.MANDATORY if iterator.getRuleStatus() >= _ICU_LINE_HARD else BreakClass.OPPORTUNITY
        if position in boundaries:
            out.append((position, cls))
    return tuple(out)


def _breaks(text: str, language: str, engine: SegmentEngine, /) -> tuple[Opportunity, ...]:
    return _icu_breaks(text, language) if engine is SegmentEngine.ICU else _uniseg_breaks(text, language)


def _break(request: LayoutRequest) -> bytes:
    spec = request.breaks
    return _RUN_ENCODER.encode(tuple((position, cls.value) for position, cls in _breaks(spec.text, spec.language, spec.engine)))


def _dictionary(language: str, left: int, right: int, /) -> object:
    resolved = language_fallback(language)
    if resolved is None:
        raise LookupError(f"no bundled hyphenation dictionary for {language!r}")
    return Pyphen(lang=resolved, left=left, right=right)


def _hyphenate(request: LayoutRequest) -> bytes:
    spec = request.hyphenate
    return _RUN_ENCODER.encode(_hyphen_breaks(spec.text, spec.language, spec.left_min, spec.right_min))


def _hyphen_breaks(text: str, language: str, left: int, right: int, /) -> tuple[HyphenBreak, ...]:
    dictionary, boundaries = _dictionary(language, left, right), frozenset(grapheme_cluster_boundaries(text))
    cursor, breaks = 0, []
    for word in words(text):
        positions = dictionary.positions(word)
        splits = tuple(reversed(tuple(dictionary.iterate(word))))
        breaks.extend(
            HyphenBreak(
                position=cursor + int(offset),
                source_start=cursor,
                source_stop=cursor + len(word),
                head=head,
                tail=tail,
            )
            for offset, (head, tail) in zip(positions, splits, strict=True)
            if cursor + int(offset) in boundaries
        )
        cursor += len(word)
    return tuple(breaks)


def _soft_breaks(text: str, language: str, left: int, right: int, /) -> frozendict[int, HyphenBreak]:
    return frozendict((row.position, row) for row in _hyphen_breaks(text, language, left, right))


def _ideographic(char: str) -> bool:
    return bool(char) and east_asian_width(char[0]) in _WIDE


def _clusters(positioned: PositionedGlyphRun, /) -> tuple[_Cluster, ...]:
    grouped = tuple(
        (cluster, tuple(entries))
        for cluster, entries in groupby(enumerate(positioned.glyphs), key=lambda entry: entry[1][1])
    )
    # source spans derive in SOURCE order — each cluster's stop is the next higher cluster value — so the RTL visual
    # reversal below never inverts a span into stop < start; only the cluster SEQUENCE reverses for reading order.
    ordered = sorted(cluster for cluster, _entries in grouped)
    stops = frozendict(zip(ordered, (*ordered[1:], len(positioned.source)), strict=True))
    logical = grouped if positioned.direction != "rtl" else tuple(reversed(grouped))
    return tuple(
        _Cluster(
            glyph_start=min(index for index, _ in entries),
            glyph_stop=max(index for index, _ in entries) + 1,
            source_start=cluster,
            source_stop=stops[cluster],
            advance=sum(float(glyph[2]) for _, glyph in entries),
        )
        for cluster, entries in logical
    )


def _stream(
    spec: ParagraphSpec,
    positioned: PositionedGlyphRun,
    opportunities: Mapping[int, BreakClass],
    soft: Mapping[int, HyphenBreak],
    /,
) -> Iterator[tuple[Item, _Cluster]]:
    logical = _clusters(positioned)
    unsafe, tatweel = positioned.unsafe_to_break, frozenset(positioned.tatweel_points)
    fit, text = spec.fit, positioned.source
    for cluster in logical:
        edge, advance = cluster.source_stop, cluster.advance
        char = text[cluster.source_start : cluster.source_stop]
        origin = cluster
        breakable = edge not in unsafe
        match opportunities.get(edge, BreakClass.PROHIBITED):
            case BreakClass.MANDATORY:
                yield Item.of_box(advance), origin
                yield Item.of_penalty(-inf, 0.0, False, LineEnding(natural=edge)), origin
            case BreakClass.OPPORTUNITY if breakable and char.isspace():
                yield Item.of_glue(advance, fit.space_stretch, fit.space_shrink), origin
            case BreakClass.OPPORTUNITY if breakable and _ideographic(char):
                yield Item.of_box(advance), origin
                yield Item.of_penalty(fit.ideograph_penalty, 0.0, False, LineEnding(natural=edge)), origin
            case BreakClass.OPPORTUNITY if breakable:
                yield Item.of_box(advance), origin
                yield Item.of_penalty(0.0, 0.0, False, LineEnding(natural=edge)), origin
            case _ if breakable and (hyphen := soft.get(edge)) is not None:
                yield Item.of_box(advance), origin
                yield Item.of_penalty(fit.hyphen_penalty, fit.hyphen_advance, True, LineEnding(hyphenated=hyphen)), origin
            case _ if edge in tatweel:
                yield Item.of_box(advance), origin
                yield Item.of_penalty(inf, 0.0, False, LineEnding(natural=edge)), origin
                yield Item.of_glue(0.0, fit.kashida_stretch, 0.0), origin
            case _:
                yield Item.of_box(advance), origin
    terminal = _Cluster(len(positioned.glyphs), len(positioned.glyphs), len(text), len(text), 0.0)
    yield Item.of_glue(0.0, inf, 0.0), terminal
    yield Item.of_penalty(-inf, 0.0, False, LineEnding(natural=len(text))), terminal


def _items(
    spec: ParagraphSpec,
    positioned: PositionedGlyphRun,
    opportunities: Mapping[int, BreakClass],
    soft: Mapping[int, HyphenBreak],
    /,
) -> tuple[tuple[Item, ...], tuple[_Cluster, ...]]:
    items, origins = zip(*_stream(spec, positioned, opportunities, soft), strict=True)
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
    return 0.0


def _legal(items: tuple[Item, ...], index: int) -> bool:
    item = items[index]
    if item.tag == "penalty":
        return item.penalty[0] < inf
    return item.tag == "glue" and index > 0 and items[index - 1].tag == "box"


def _line_start(items: tuple[Item, ...], after: int) -> int:
    start = after + 1
    while start < len(items) and items[start].tag == "glue":
        start += 1
    return start


def _demerits(ratio: float, item: Item, node: _Node, fit: FitPolicy) -> float:
    badness = _badness(ratio)
    cost = item.penalty[0] if item.tag == "penalty" else 0.0
    flagged = item.tag == "penalty" and item.penalty[2]
    base = (
        (fit.line_penalty + badness + cost) ** 2
        if cost >= 0.0
        else (fit.line_penalty + badness) ** 2 - cost * cost
        if cost > -inf
        else (fit.line_penalty + badness) ** 2
    )
    paired = fit.flagged_demerit if flagged and node.flagged else 0.0
    jump = fit.fitness_demerit if abs(_fitness(ratio) - node.fitness) > 1 else 0.0
    return base + paired + jump


def _trace(chosen: _Node, items: tuple[Item, ...], origins: tuple[_Cluster, ...], width: list[float]) -> tuple[LayoutLine, ...]:
    chain: list[_Node] = []
    node: _Node | None = chosen
    while node is not None and node.position >= 0:
        chain.append(node)
        node = node.previous
    chain.reverse()
    lines: list[LayoutLine] = []
    opener = -1
    for broken_at in chain:  # Exemption: predecessor-chain walk over immutable DP links
        start = _line_start(items, opener)
        stop = broken_at.position if items[broken_at.position].tag == "glue" else broken_at.position + 1
        covered = origins[start:stop] or origins[broken_at.position : broken_at.position + 1]
        glyphs = tuple(origin for origin in covered if origin.glyph_start < origin.glyph_stop)
        evidence = glyphs or covered
        extra = items[broken_at.position].penalty[1] if items[broken_at.position].tag == "penalty" else 0.0
        lines.append(
            LayoutLine(
                start=min(origin.glyph_start for origin in evidence),
                stop=max(origin.glyph_stop for origin in evidence),
                source_start=min(origin.source_start for origin in evidence),
                source_stop=broken_at.ending.hyphenated.position
                if broken_at.ending.tag == "hyphenated"
                else broken_at.ending.natural,
                ratio=broken_at.ratio,
                advance=width[broken_at.position] - width[start] + extra,
                ending=broken_at.ending,
            )
        )
        opener = broken_at.position
    return tuple(lines)


def broken(spec: ParagraphSpec, /) -> LineBrokenRun:
    fit = spec.fit
    positioned = msgspec.msgpack.decode(spec.run, type=PositionedGlyphRun)
    text = positioned.source
    opportunities = {position: cls for position, cls in _breaks(text, spec.language, spec.engine)}
    items, origins = _items(spec, positioned, opportunities, _soft_breaks(text, spec.language, spec.left_min, spec.right_min))
    measure, count = spec.measure, len(items)
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
            ending=LineEnding(natural=0),
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
            if forced or -1.0 <= ratio <= fit.tolerance:
                fitted, total = _fitness(ratio), node.demerits + _demerits(ratio, item, node, fit)
                if fitted not in best or total < best[fitted].demerits:
                    best[fitted] = _Node(
                        position=index,
                        line=node.line + 1,
                        fitness=fitted,
                        width=width[opened],
                        stretch=stretch[opened],
                        shrink=shrink[opened],
                        demerits=total,
                        flagged=item.tag == "penalty" and item.penalty[2],
                        ratio=ratio,
                        ending=item.penalty[3] if item.tag == "penalty" else LineEnding(natural=origins[index].source_start),
                        previous=node,
                    )
        active = [*survivors, *best.values()] or active
    chosen = min(active, key=lambda node: node.demerits)
    return LineBrokenRun(lines=_trace(chosen, items, origins, width), total_demerits=chosen.demerits)


def _paragraph(request: LayoutRequest) -> bytes:
    return _RUN_ENCODER.encode(broken(request.paragraph))


def _measure(request: LayoutRequest) -> bytes:
    spec = request.measure
    return _RUN_ENCODER.encode(tuple(tt_text_extents(spec.text, ambiguous_as_wide=spec.ambiguous_wide)))


def _collate(request: LayoutRequest) -> bytes:
    spec = request.collate
    locale, policy = Locale(spec.language), spec.policy
    collator = Collator.createInstance(locale)
    toggled = lambda live: "ON" if live else "OFF"
    rows = (
        (UCollAttribute.STRENGTH, policy.strength.name),
        (UCollAttribute.CASE_FIRST, _UCA_CASE[policy.case_first]),
        (UCollAttribute.ALTERNATE_HANDLING, policy.alternate.name),
        (UCollAttribute.NORMALIZATION_MODE, toggled(policy.normalization)),
        (UCollAttribute.NUMERIC_COLLATION, toggled(policy.numeric)),
    )
    for attribute, name in rows:  # Exemption: the ICU collator is a stateful native object configured in place
        collator.setAttribute(attribute, getattr(UCollAttributeValue, name))
    ordered = sorted(spec.items, key=collator.getSortKey)
    index = AlphabeticIndex(locale)
    for item in ordered:  # Exemption: ICU AlphabeticIndex is a stateful native bucketer
        index.addRecord(item, item)
    buckets: list[tuple[str, tuple[str, ...]]] = []
    while index.nextBucket():  # Exemption: native bucket cursor
        members: list[str] = []
        while index.nextRecord():
            members.append(index.recordData)
        buckets.append((index.getBucketLabel(), tuple(members)))
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

[PYICU]-[BLOCKED]: `PyICU; python_version<'3.15'` excludes the live interpreter, and `UV_CACHE_DIR=.cache/uv uv run --frozen python -m tools.assay api resolve PyICU` reports the gate; `BreakIterator`, the full `Collator.setAttribute` matrix, version constants, and `AlphabeticIndex` therefore ride the catalog-verified surface.
