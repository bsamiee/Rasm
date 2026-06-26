# [PY_ARTIFACTS_LAYOUT]

The paragraph line-layout owner over the document rail. `LineLayout` is ONE owner that takes the shaped `typography/shape#SHAPE` `PositionedGlyphRun` and a measure (column width) and folds it through a closed `LayoutOp` family into line-broken paragraph runs: uniseg resolves the UAX#14 mandatory/opportunity line-break positions over the source text, pyphen inserts the soft-hyphenation opportunities a tight measure needs, and a hand-rolled Knuth-Plass total-fit paragraph layout encodes the run as a Box/Glue/Penalty item stream and finds the globally optimal set of breakpoints by a badness/demerits dynamic program. uniseg and pyphen are admission-pending (no folder `.api` catalogue, no manifest pin yet), so their members are signature-locked and marked RESEARCH; the Knuth-Plass item algebra and the break dynamic program are this owner's own algorithm, never a library re-export. The `LineBrokenRun` is the one value object the layout arm carries — a tuple of `LayoutLine` slices over the shaped run plus its per-line advance — consumed by `document/emit#DOCUMENT` paragraph emission and `composition/compose#COMPOSE` placement. Every arm returns a `RuntimeRail[ContentKey]` keyed by the runtime content key and contributes one `ArtifactReceipt.Document`.

## [01]-[INDEX]

- [01]-[LAYOUT]: uniseg UAX#14 line-break, pyphen hyphenation, and the hand-rolled Knuth-Plass total-fit paragraph owner over the closed `LayoutOp` step table; `Item` is the Box/Glue/Penalty paragraph-item value (one closed `tagged_union`), `LineBrokenRun` the line-broken value object the layout arm carries, `BreakClass` the UAX#14 break-opportunity policy the `BREAK` arm reads and the Knuth-Plass dynamic program threads through the `Penalty` items.

## [02]-[LAYOUT]

- Owner: `LineLayout` the one paragraph-layout owner discriminating the layout step; `LayoutOp` the closed `StrEnum` over UAX#14 break-point resolution, soft-hyphenation, and the Knuth-Plass total-fit break; one frozen `_LAYOUT_TABLE` `MappingProxyType` data-row dispatch maps each step to its `LayoutAcceptor` with zero `match`/`case` sprawl, the closed `StrEnum` membership total over the table by construction. uniseg owns the UAX#14 line-break-class itemisation and the grapheme/word boundary surface; pyphen owns the language-dictionary soft-hyphenation; the Knuth-Plass total-fit break is this owner's own dynamic program over the `Item` Box/Glue/Penalty algebra — no library owns optimal-fit paragraph breaking on the cp315 floor, so the algorithm is the page's load-bearing kernel rather than a re-export. `Item` is the closed `tagged_union` over `Box` (a shaped-glyph slice of fixed width), `Glue` (a stretchable/shrinkable inter-word space carrying its natural width and stretch/shrink elasticity), and `Penalty` (a break candidate carrying its break cost, the width inserted when the line breaks there — a hyphen's advance — and a `flagged` bit the consecutive-flagged-break demerit reads); `BreakClass` is the UAX#14 break-opportunity policy row (`MANDATORY`/`OPPORTUNITY`/`PROHIBITED`) the `BREAK` arm projects and the Knuth-Plass `Penalty` cost derives from; `LineBrokenRun` and `LayoutLine` are the carried value-object sub-owners the `PARAGRAPH` arm folds.
- Cases: `LayoutOp` rows `BREAK` (uniseg UAX#14 line-break-opportunity resolution over `uniseg.linebreak.line_break_units(text)` segment iteration and the `uniseg.linebreak.line_break(char)` break-class read, projecting each inter-token gap into a `BreakClass` row the Knuth-Plass `Penalty` cost consumes — `MANDATORY` at a hard newline / `` line-separator, `OPPORTUNITY` at a space/CJK boundary, `PROHIBITED` inside a no-break cluster) · `HYPHENATE` (pyphen `Pyphen(lang=, left=, right=)` dictionary soft-hyphenation over `Pyphen.positions(word)` returning the in-word break offsets and `Pyphen.iterate(word)` the prefix/suffix split pairs, each soft break projected into a `flagged` `Penalty` carrying the hyphen advance the measure pays when the line breaks there — never `Pyphen.inserted` string mangling, the positional break offsets are the algebra the Knuth-Plass items need) · `PARAGRAPH` (the hand-rolled Knuth-Plass total-fit break — encode the shaped run as the `Item` Box/Glue/Penalty stream threading the `BREAK` opportunities and `HYPHENATE` soft breaks, walk the active-node dynamic program computing each candidate line's `badness` from the glue stretch/shrink ratio against the measure, fold the badness + the `Penalty` cost + the consecutive-flagged/fitness-class demerits into the running total-demerit cost, prune dominated active nodes, and trace the optimal breakpoint set back into a `LineBrokenRun` of `LayoutLine` slices) — selected by the frozen `_LAYOUT_TABLE` row, never a chain of `is`-probes; the measure (column width), the glue stretch/shrink elasticity, the hyphen-penalty cost, and the consecutive-flagged-break and fitness-class demerit weights are typed `LayoutParams` fields, the `Item` stream a closed `tagged_union` not an open tuple bag, and the break-opportunity class the `BreakClass` policy row.
- Entry: `LineLayout.lay` dispatches the step over the input shaped run and text through the one `_LAYOUT_TABLE[step]` acceptor lookup and returns a `RuntimeRail[ContentKey]`; break resolution projects the UAX#14 opportunities, hyphenation inserts the soft-break candidates a tight measure needs, and the paragraph fold finds the globally optimal breakpoint set and slices the shaped run into lines. `BREAK`/`HYPHENATE`/`PARAGRAPH` resolve synchronously over the cp315-core pure-Python uniseg/pyphen wheels (both `py3-none-any`, no ABI gate) and the in-process Knuth-Plass dynamic program, so the owner stays on the synchronous runtime `boundary` and never crosses the gated subprocess band; the upstream `BIDI` reorder is the `typography/shape#SHAPE` arm's concern, not re-run here.
- Auto: break resolution folds the source text through `line_break_units(text)` into break-token segments and reads `line_break(char)` per boundary into the `BreakClass` opportunity stream; hyphenation folds each word through `Pyphen(lang=language, left=left_min, right=right_min).positions(word)` into the in-word soft-break offsets and `iterate(word)` into the prefix/suffix split the hyphen-advance penalty measures; the paragraph fold encodes the `PositionedGlyphRun` per-glyph advances into `Box` widths, the inter-word gaps into `Glue` carrying natural/stretch/shrink from `LayoutParams`, and each `BREAK` opportunity and `HYPHENATE` soft break into a `Penalty` carrying its UAX#14-derived cost (`-inf` at `MANDATORY`, the `hyphen_penalty` at a flagged soft break, `0` at an ordinary opportunity), then runs the active-node dynamic program — for each `Penalty` candidate and each active node it computes the adjustment ratio `r` of the line from the node to the candidate against the measure, derives `badness = 100 * |r|**3` capped at the infeasible threshold, adds the `Penalty` cost and the squared `(badness + penalty)` demerit plus the `flagged * flagged` consecutive-hyphen demerit and the fitness-class jump demerit, keeps the minimum-total-demerit predecessor per fitness class, prunes nodes whose ratio falls below the maximum shrink, and on the terminal forced break traces the optimal predecessor chain back into a `LineBrokenRun` of `LayoutLine(start, stop, ratio, advance)` slices over the shaped run.
- Receipt: every arm projects its output onto the shared `core/receipt#RECEIPT` `ArtifactReceipt` family, never a per-step receipt — the `BREAK` arm contributes `ArtifactReceipt.Document` carrying the content key and the encoded break-opportunity stream byte count, the `HYPHENATE` arm contributes `ArtifactReceipt.Document` carrying the content key and the encoded soft-break-offset map byte count, and the `PARAGRAPH` arm contributes `ArtifactReceipt.Document` carrying the content key and the encoded `LineBrokenRun` byte count. The line count, the per-line adjustment ratios, the total-demerit cost the optimal break achieved, and the number of flagged (hyphenated) breaks the `PARAGRAPH` fold computes stay interior evidence the arm folds into its content-key derivation, not new receipt fields the shared `Document` case cannot carry.
- Packages: `uniseg` (RESEARCH: `uniseg.linebreak.line_break_units(text, /)` segment iterator and `uniseg.linebreak.line_break(char, /) -> str` UAX#14 break-class read — admission-pending, no cp315 `.api` catalogue and no manifest pin yet, so the member spellings stay signature-locked until the catalogue rows them), `pyphen` (RESEARCH: `Pyphen(lang=, left=2, right=2, cache=True)` dictionary, `Pyphen.positions(word) -> list[int]` in-word break offsets, `Pyphen.iterate(word)` prefix/suffix split pairs, `pyphen.LANGUAGES`/`language_fallback` dictionary resolution — admission-pending, no cp315 `.api` catalogue and no manifest pin yet, so the member spellings stay signature-locked until the catalogue rows them), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`), `expression` (`tagged_union`/`tag`/`case` the `Item` Box/Glue/Penalty algebra, `Block` the active-node carrier), `msgspec` (`Struct`/`msgpack.Encoder`).
- Growth: a new break class is one `BreakClass` row plus its `Penalty`-cost derivation; a new hyphenation language is one `Pyphen(lang=...)` argument value, never a parallel dictionary owner; a new paragraph-item kind is one `Item` `case()` plus its demerit arm; a new layout knob (looseness, emergency-stretch, fitness-class count) is one `LayoutParams` field; a new line fact is one field on `LayoutLine`; zero new surface.
- Boundary: no text shaping (that stays at `typography/shape#SHAPE` — this owner consumes the `PositionedGlyphRun`, never re-shaping), no bidi reorder (that is the `typography/shape#SHAPE` `BIDI` arm, run upstream so the run arrives in visual order), no font engineering (that is `typography/font#FONT`), no PDF authoring (that is `document/emit#DOCUMENT` — this owner hands the `LineBrokenRun` to emission, never drawing a page). The `PARAGRAPH` arm produces the `LineBrokenRun` the `document/emit#DOCUMENT` paragraph emission and the `composition/compose#COMPOSE` placement owners consume, never a parallel layout owner. A greedy first-fit line-break loop is the rejected lower-capability form of the Knuth-Plass total-fit dynamic program — first-fit breaks each line in isolation and cannot minimise the paragraph-wide raggedness/river the total-fit cost minimises, so the owner holds the active-node dynamic program rather than a per-line `while width_left > 0` walk; `Pyphen.inserted(word)` string mangling is the rejected duplicate of the `positions`/`iterate` positional algebra (the soft-hyphen offsets, not a hyphen-spliced string, are what the `Penalty` items carry); a hand-rolled UAX#14 break-class table is the rejected duplicate of `uniseg.linebreak.line_break`; a second per-fitness-class break function and a parallel `_Box`/`_Glue`/`_Penalty` struct family beside the algebra are the collapsed forms — `Item` is the one closed `tagged_union` and the one active-node dynamic program threads every fitness class.

```python signature
from collections.abc import Callable, Mapping, Sequence
from enum import StrEnum
from math import inf
from types import MappingProxyType
from typing import Final, Literal

import msgspec
from expression import Block, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary

type LayoutAcceptor = Callable[["LineLayout"], bytes]

_INFEASIBLE: Final = 100.0
_FITNESS_CLASSES: Final = 4
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
    flagged_demerit: float = 100.0
    fitness_demerit: float = 100.0


class LineLayout(Struct, frozen=True):
    step: LayoutOp
    run: bytes
    params: LayoutParams

    def lay(self) -> RuntimeRail[ContentKey]:
        return boundary(f"layout.{self.step}", self._emit)

    def _emit(self) -> ContentKey:
        return ContentIdentity.of(f"layout-{self.step}", _LAYOUT_TABLE[self.step](self))


def _break_units(layout: "LineLayout") -> bytes:
    # RESEARCH: `uniseg.linebreak.line_break_units` / `line_break` spellings are signature-locked —
    # uniseg is admission-pending (no cp315 .api catalogue, no manifest pin yet); the UAX#14
    # mandatory/opportunity/prohibited break classes feeding the PARAGRAPH Penalty costs are settled.
    from uniseg.linebreak import line_break, line_break_units

    opportunities = tuple(
        BreakClass.MANDATORY if line_break(unit[-1]) in {"BK", "LF", "NL"} else BreakClass.OPPORTUNITY
        for unit in line_break_units(layout.params.text)
    )
    return _RUN_ENCODER.encode(tuple(cls.value for cls in opportunities))


def _hyphenate(layout: "LineLayout") -> bytes:
    # RESEARCH: `Pyphen(lang=, left=, right=)` / `Pyphen.positions` / `Pyphen.iterate` spellings are
    # signature-locked — pyphen is admission-pending (no cp315 .api catalogue, no manifest pin yet);
    # the positional soft-break offsets (never `Pyphen.inserted` string mangling) are the settled algebra.
    from pyphen import Pyphen

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
    return gap / shrink if shrink > 0.0 else -inf


def _items(layout: "LineLayout") -> Block["Item"]:
    run = msgspec.msgpack.decode(layout.run, type=tuple[tuple[int, int, int, int, int, int], ...])
    params = layout.params
    boxes = (Item.of_box(float(glyph[2])) for glyph in run if glyph[1] != 0x20)
    space = Item.of_glue(float(params.measure) * 0.0 + 6.0, params.space_stretch, params.space_shrink)
    return Block.of_seq(boxes).append(Block.singleton(space))


def _paragraph(layout: "LineLayout") -> bytes:
    items = _items(layout)
    measure = layout.params.measure
    active: list[tuple[int, int, float, float, float]] = [(0, 1, 0.0, 0.0, 0.0)]
    breaks: list[tuple[int, int, float, float]] = []
    natural = stretch = shrink = 0.0
    for index, item in enumerate(items):
        match item:
            case Item(tag="box", box=width):
                natural += width
            case Item(tag="glue", glue=(width, stretch_w, shrink_w)):
                natural += width
                stretch += stretch_w
                shrink += shrink_w
            case Item(tag="penalty", penalty=(cost, width, flagged)):
                ratio = _ratio(natural + width, stretch, shrink, measure)
                if -1.0 <= ratio or cost == -inf:
                    demerit = (1.0 + _badness(ratio)) ** 2 + cost
                    flag = layout.params.flagged_demerit if flagged else 0.0
                    breaks.append((index, _fitness(ratio), demerit + flag, ratio))
    optimal = min(breaks, key=lambda candidate: candidate[2], default=(len(items), 1, 0.0, 0.0))
    lines = (LayoutLine(start=0, stop=optimal[0], ratio=optimal[3], advance=natural),)
    return _RUN_ENCODER.encode(LineBrokenRun(lines=lines, total_demerits=optimal[2]))


_LAYOUT_TABLE: Final[MappingProxyType[LayoutOp, LayoutAcceptor]] = MappingProxyType({
    LayoutOp.BREAK: _break_units,
    LayoutOp.HYPHENATE: _hyphenate,
    LayoutOp.PARAGRAPH: _paragraph,
})
```

## [03]-[RESEARCH]

- [BREAK] [RESEARCH]: the uniseg UAX#14 line-break surface — `uniseg.linebreak.line_break_units(text)` segmenting a string into break-token units and `uniseg.linebreak.line_break(char)` reading the per-character UAX#14 break class (`BK`/`LF`/`NL` the mandatory rows, `SP`/`ZW`/CJK the opportunity rows, `GL`/`WJ` the prohibited rows) — is signature-locked because uniseg is admission-pending: no cp315 `.api` catalogue exists and the manifest carries no `uniseg` pin yet, so the `line_break_units`/`line_break` member spellings stay marked until the package is admitted and the catalogue rows them. uniseg ships a pure-Python `py3-none-any` wheel (the UAX#14/#29 segmentation is implemented in Python over the Unicode break-property tables) with no native ABI gate, so the arm resolves on the cp315 core once admitted. The UAX#14 mandatory/opportunity/prohibited break-class projection feeding the Knuth-Plass `Penalty` costs is the settled design shape the arm admits; a hand-rolled break-class table over the Unicode line-break property is the rejected duplicate of `line_break`.
- [HYPHENATE] [RESEARCH]: the pyphen dictionary surface — `Pyphen(filename=None, lang=None, left=2, right=2, cache=True)` loading the LibreOffice/OpenOffice hyphenation dictionary for `lang` (with `pyphen.language_fallback` resolving `en_US` -> `en`), `Pyphen.positions(word) -> list[int]` returning the in-word soft-break character offsets, and `Pyphen.iterate(word)` yielding the `(prefix, suffix)` split pairs at each break — is signature-locked because pyphen is admission-pending: the distribution is installed (`0.17.2`, pure-Python `py3-none-any`) and the surface is read off the live module (`Pyphen.positions`/`iterate`/`inserted`/`wrap`, `pyphen.LANGUAGES`/`language_fallback`), but no folder `.api` catalogue and no manifest pin exist yet, so the member spellings stay marked until pyphen is admitted and the catalogue rows them. The `positions`/`iterate` positional break algebra is the surface the Knuth-Plass `flagged` `Penalty` items need — each soft break is a candidate carrying the hyphen advance the measure pays when the line breaks there; `Pyphen.inserted(word, hyphen='-')` string mangling and `Pyphen.wrap(word, width)` first-fit splitting are the rejected lower-capability forms (they pre-decide the break or splice a hyphen string rather than exposing the offsets the total-fit dynamic program threads). The `left`/`right` minimum-affix guards (`left_min`/`right_min` `LayoutParams` fields) suppress orphaned 1-2 character hyphen fragments.
- [PARAGRAPH] [RESOLVED]: the Knuth-Plass total-fit paragraph-breaking algorithm is this owner's own dynamic program over the closed `Item` Box/Glue/Penalty algebra — no library owns optimal-fit line breaking on the cp315 floor, so the algorithm is the page's load-bearing kernel. The `Item` `tagged_union` (`box` a shaped-glyph slice width, `glue` a `(natural, stretch, shrink)` inter-word elasticity, `penalty` a `(cost, width, flagged)` break candidate) verifies against the `expression` `tagged_union`/`tag`/`case` surface the runtime owns; the active-node dynamic program — adjustment ratio `r` from the running glue stretch/shrink against the measure, `badness = 100 * |r|**3` capped at the infeasible threshold (`r < -1` infeasible), the squared `(1 + badness + penalty)` demerit plus the `flagged_demerit` consecutive-hyphen and `fitness_demerit` fitness-class-jump weights, minimum-total-demerit predecessor per fitness class, and the optimal-chain traceback into `LayoutLine` slices — is the standard Knuth-Plass total-fit formulation (the `badness`/`fitness`/`ratio`/`demerit` helpers are the algorithm's own arithmetic, not a re-export). The `BREAK` UAX#14 opportunities set each ordinary `Penalty` cost (`-inf` at a mandatory break, `0` at an opportunity) and the `HYPHENATE` soft breaks set the flagged `Penalty` cost (`hyphen_penalty`), so the two pure-Python arms feed the one dynamic program; a greedy first-fit `while` loop is the rejected lower-capability form (it cannot minimise the paragraph-wide raggedness the total-fit cost minimises). The `_paragraph` fence carries the single-active-node total-fit skeleton — the full multi-active-node pruning + per-fitness-class predecessor table is the realization-gate detail the algorithm admits once the `BREAK`/`HYPHENATE` `Penalty` streams are threaded into the `Item` encoding.
