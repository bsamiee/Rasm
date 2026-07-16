# [PY_ARTIFACTS_DRAWING_ANNOTATE]

The ISO 128-2 annotation producer: `Annotate` is ONE owner over a closed `AnnotateOp` union — `Leader`/`TextNote`/`RevisionCloud` — lowering every leader, keynote, flag-note, general-note, and revision-cloud mark onto a named SVG layer AND an `ezdxf` DXF layout, the structural twin of `drawing/symbol#SYMBOL`. The three cases are the deepest collapse the domain admits: `Leader` carries the shared ISO 128-2 leader geometry (targets, landing shoulder, straight/spline reference line, terminator) and discriminates its landing over a closed `LeaderContent` sub-family — `Note` mtext, `Keynote` code-bubble, `Flag` note-flag — so a plain leader, a keynote, and a flag-note are one owner varying only in what sits at the landing, the exact `ezdxf` `add_multileader_mtext`-versus-`add_multileader_block` split; `TextNote` carries a leader-less block whose `NoteBody` is `Prose` (a Knuth-Plass run outlined line-by-line through `ziafont`) or `Math` (a `ziamath` text-and-`$math$` note); `RevisionCloud` carries the scalloped enclosure and its optional delta tag.

`Annotate` composes the drawing plane's one mark-style owner rather than re-declaring it: `drawing/symbol#SYMBOL` `SymbolStyle` carries the palette index, ISO 128 `LineWeight`, ISO 3098 `TextHeight`, `LayerName`, and `Terminator`, all from `drawing/regime#REGIME`'s owned vocabulary — so the plane declares no parallel `AnnotateStyle` and no second terminator family, and the shared `drawsvg.Marker(orient='auto')` defs auto-orient each ISO line-end along its leader. Note text outlines to a font-independent `ziafont` `<path>` and the formula typesets through `ziamath` (never a font-dependent `drawsvg.Text`); the general note consumes `typography/layout#LAYOUT`'s `LineBrokenRun` and `typography/shape#SHAPE`'s `PositionedGlyphRun` rather than re-breaking or re-shaping; a keynote/flag column distributes through one `kiwisolver.Solver`; a filled-band revision cloud composes the landed `graphic/vector/region#REGION` `outline`/`boolean` over the self-contained stroked-scallop default. Every mark palette-indexes to `visualization/chart/spec#CHART`, the render offloads onto the runtime thread lane, and the owner contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Drawing` case and one `core/plan#PLAN` `ArtifactWork` node — minting no IFC (`csharp:Rasm.Bim`) and computing no sheet placement (`composition/sheet#SHEET`).

## [01]-[INDEX]

- [01]-[ANNOTATE]: the `Annotate` owner over the closed `AnnotateOp` (`Leader`/`TextNote`/`RevisionCloud`) with nested `LeaderContent`/`NoteBody`, dual-lowering each mark over `SymbolTarget` into a `drawsvg` named layer or an `ezdxf` multileader layout.

## [02]-[ANNOTATE]

- Owner: `Annotate` holds `marks`, the `graphic/color/derive#DERIVE` `Palette`, and the `SymbolTarget` egress value, discriminating over the closed `AnnotateOp` — never a per-marker `KeynoteMark`/`FlagMark`/`GeneralNote` class family, never a `StrEnum` over an erased `dict`. `LeaderContent` (`Note`/`Keynote`/`Flag`) collapses the leader-geometry sharing: the three carry identical `targets`/`landing`/`LeaderPath`/`terminator` and differ ONLY at the landing — the `ezdxf` mtext-versus-block split, not three parallel payloads. `NoteBody` (`Prose`/`Math`) carries only each mode's own fields, never one permissive bag.
- Cases: each `AnnotateOp` case carries its own typed geometry plus `SymbolStyle` as the last slot, matched by one total `match` over `tag`, the nested `LeaderContent`/`NoteBody` each by their own total `match` — never a per-content special case. `LeaderPath` (`STRAIGHT`/`SPLINE`) and `BubbleShape` (`CIRCLE`/`HEXAGON`/`TRIANGLE`/`DIAMOND`/`RECTANGLE`) are the closed leader-and-bubble axes; an empty `Leader.targets` is the leader-less landing bubble.
- Entry: `Annotate.over` normalizes `AnnotateOp | Iterable[AnnotateOp]` by a structural `match` at the head — never a `batch` knob. `render` returns `RuntimeRail[ArtifactReceipt]` beside the `layered()` `LayerPlan` projection; `_svg_engine` and `_dxf_engine` are the `SymbolTarget`-keyed dual lowering, the SVG arm folding each mark into its `SymbolStyle.layer` `drawsvg.Group` under one `Drawing` carrying the shared terminator defs, the DXF arm through the `ezdxf` multileader/mtext/wipeout/lwpolyline model.
- Auto: `SymbolStyle.fill`/`stroke` are palette indices resolved once per render, so a recolor is a palette swap, not a per-mark literal. Leader content, prose lines, and the math note are MEASURED and baseline-seated via `getyofst()`/`getsize()` so each outline's baseline lands on the leader shoulder / note insert rather than its bounding-box top — the blind translate that hangs the outline below the landing is the trap; `ziafont` outlines to a font-independent `<path>` (never a `drawsvg.Text` that renders wrong without the CAD font) and `ziamath.Text` typesets the mixed note. `_svg_engine` pins `ziafont.config.precision`/`ziamath.config.precision` once inside the offload lane so the emitted `d`-floats and the content key over them are deterministic. `RevisionCloud` folds each edge into `max(1, round(length / (2·radius)))` convex bumps. A keynote/flag column threads one `kiwisolver.Solver` — a `required` anchor, a `required` `_MIN_SEP` HARD no-overlap (never a soft band an overlap survives), and a `weak` equal-gap — and `updateVariables()` writes each solved `value()` into the routed landing the engines pass, actually re-keying rather than leaving the solve unread.
- Packages: `drawsvg` the named-layer container and leader/scallop path builder with the reusable auto-oriented `Marker` defs; `ziafont` the ISO 3098 text-to-`<path>` outline (measured, baseline-seated, precision-pinned); `ziamath` the mixed text-and-math typeset; `ezdxf` the DXF multileader/mtext/wipeout/lwpolyline model with the `MTextEditor` content builder and `bbox.extents` true rendered extent; `kiwisolver` the keynote/flag-column solve; `beartype` the `over` contract; `typography/layout#LAYOUT` `LineBrokenRun` and `typography/shape#SHAPE` `PositionedGlyphRun` the consumed line-break and shaping; `drawing/regime#REGIME` `Terminator`/`LineWeight`/`TextHeight`/`LayerName` and `drawing/symbol#SYMBOL` `SymbolStyle`/`SymbolTarget` the owned ISO vocabulary and mark-style; `graphic/color/derive#DERIVE` `Palette`/`hex_ramp`; `graphic/vector/region#REGION` `outline`/`boolean` for the filled-band variant only.
- Growth: a new leader landing content is one `LeaderContent` case plus one `_content_group`/`_dxf_leader` arm — leader geometry, terminator, and routing untouched; a new note body is one `NoteBody` case plus one arm; a new mark grammar is one `AnnotateOp` case; a new terminator is one `drawing/regime#REGIME` `Terminator` member plus one `_marker` arm; a new bubble is one `BubbleShape` member plus one `_SIDES` row; a new leader reference-line kind is one `LeaderPath` member plus one `Path` arm; a new mark visual axis is one `SymbolStyle` field on `drawing/symbol#SYMBOL`; a new column-distribution rule is one `kiwisolver` constraint at its `strength` band. Zero new surface for a new annotation kind or a new layer.
- Boundary: no dimension, symbol, or sheet-set logic — `drawing/dimension#DIMENSION`, `drawing/symbol#SYMBOL`, `composition/sheet#SHEET`. `drawsvg` owns the SVG container and leader/scallop builders, `ziafont`/`ziamath` the text and math outline, `ezdxf` the DXF model, `graphic/vector/region#REGION` the boolean/offset, `kiwisolver` the solve, `typography/layout#LAYOUT`/`typography/shape#SHAPE` the line-break and shaping, `export/layered#LAYERED` the layer binding, and `csharp:Rasm.Bim` the IFC; identity minting is the runtime's.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import math
import os
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import lru_cache
from itertools import pairwise
from typing import Literal, Self, assert_never

import msgspec
from beartype import beartype
from expression import Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.drawing.regime import LineWeight, Terminator
from artifacts.drawing.symbol import SymbolStyle, SymbolTarget
from artifacts.graphic.layer import LayerContent, LayerIntent, LayerNode, LayerPlan, NamingSchema
from artifacts.typography.layout import LineBrokenRun
from artifacts.typography.shape import PositionedGlyphRun
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp

# each proxy reifies on first render-arm use in the offloaded worker
lazy import drawsvg
lazy import ezdxf
lazy import kiwisolver
lazy import ziafont
lazy import ziamath
lazy from ezdxf import bbox
lazy from ezdxf.enums import MTextEntityAlignment
lazy from ezdxf.gfxattribs import GfxAttribs
lazy from ezdxf.math import Vec2
lazy from ezdxf.render.mleader import ConnectionSide
lazy from ezdxf.tools.text import MTextEditor

# --- [TYPES] ----------------------------------------------------------------------------
type Point = tuple[float, float]
type Box = tuple[float, float, float, float]
type AnnotateTag = Literal["leader", "textnote", "revcloud"]

_SHOULDER: float = 2.0  # landing-shoulder length, in text-height multiples (ISO 128-2 leader landing)
_BUBBLE: float = 1.4  # keynote/flag bubble radius, in text-height multiples
_MIN_SEP: float = 8.0  # keynote/flag-column minimum vertical separation (drawing units)
_LEADER_STYLE: str = "ISO-128-2"  # the annotation convention the receipt style slot carries
_PRECISION: int = 3  # ziafont/ziamath emitted-d-float places — the content-key determinism lever set once per offloaded arm


class LeaderPath(StrEnum):  # ISO 128-2 leader reference-line geometry (ezdxf `render.mleader.LeaderType` twin)
    STRAIGHT = "straight"  # straight polyline — the drawing-office default
    SPLINE = "spline"  # smoothed spline leader through the elbow


class BubbleShape(StrEnum):  # the keynote/flag landing-mark outline
    CIRCLE = "circle"  # keynote circle bubble
    HEXAGON = "hexagon"
    TRIANGLE = "triangle"  # flag-note delta
    DIAMOND = "diamond"
    RECTANGLE = "rectangle"  # boxed keynote


# --- [VOCABULARY] -----------------------------------------------------------------------
@tagged_union(frozen=True)
class LeaderContent:
    # the leader-landing content sub-family — the ezdxf mtext-vs-block split as data.
    tag: Literal["note", "keynote", "flag"] = tag()
    note: str = case()  # MTextEditor-formatted mtext content
    keynote: tuple[str, BubbleShape, str, bool] = case()  # (code, bubble, sheet_ref, mask)
    flag: tuple[str, BubbleShape, bool] = case()  # (number, shape, mask)

    @staticmethod
    def Note(text: str) -> "LeaderContent":
        return LeaderContent(note=text)

    @staticmethod
    def Keynote(code: str, bubble: BubbleShape = BubbleShape.HEXAGON, sheet_ref: str = "", *, mask: bool = True) -> "LeaderContent":
        return LeaderContent(keynote=(code, bubble, sheet_ref, mask))

    @staticmethod
    def Flag(number: str, shape: BubbleShape = BubbleShape.TRIANGLE, *, mask: bool = True) -> "LeaderContent":
        return LeaderContent(flag=(number, shape, mask))


@tagged_union(frozen=True)
class NoteBody:
    # the note-body sub-family — each mode carries only its own fields, never one permissive bag.
    tag: Literal["prose", "math"] = tag()
    prose: tuple[str, bytes, LineBrokenRun] = case()  # (source, PositionedGlyphRun bytes, Knuth-Plass break)
    math: str = case()  # mixed text + $math$ ziamath source

    @staticmethod
    def Prose(source: str, run: bytes, broken: LineBrokenRun) -> "NoteBody":
        return NoteBody(prose=(source, run, broken))

    @staticmethod
    def Math(source: str) -> "NoteBody":
        return NoteBody(math=source)


@tagged_union(frozen=True)
class AnnotateOp:
    tag: AnnotateTag = tag()
    leader: tuple[tuple[Point, ...], Point, LeaderPath, LeaderContent, SymbolStyle] = case()
    textnote: tuple[Point, NoteBody, SymbolStyle] = case()
    revcloud: tuple[tuple[Point, ...], float, str, SymbolStyle] = case()

    @staticmethod
    def Leader(
        targets: tuple[Point, ...], landing: Point, content: LeaderContent, style: SymbolStyle, *, path: LeaderPath = LeaderPath.STRAIGHT
    ) -> "AnnotateOp":
        return AnnotateOp(leader=(targets, landing, path, content, style))

    @staticmethod
    def TextNote(insert: Point, body: NoteBody, style: SymbolStyle) -> "AnnotateOp":
        return AnnotateOp(textnote=(insert, body, style))

    @staticmethod
    def RevisionCloud(region: tuple[Point, ...], radius: float, style: SymbolStyle, *, mark: str = "") -> "AnnotateOp":
        return AnnotateOp(revcloud=(region, radius, mark, style))


# --- [SERVICES] -------------------------------------------------------------------------
class Annotate(Struct, frozen=True):
    marks: tuple[AnnotateOp, ...]
    palette: Palette
    target: SymbolTarget = SymbolTarget.SVG

    @classmethod
    @beartype
    def over(cls, marks: AnnotateOp | Iterable[AnnotateOp], palette: Palette, /, *, target: SymbolTarget = SymbolTarget.SVG) -> Self:
        match marks:  # the one modal-arity head — a lone mark is the singleton, an iterable the multi-mark sheet
            case AnnotateOp():
                return cls(marks=(marks,), palette=palette, target=target)
            case _:
                return cls(marks=tuple(marks), palette=palette, target=target)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.marks)))

    @property
    def _key(self) -> ContentKey:
        # key over the frozen INPUT spec, minted pre-run — never over rendered layer bytes.
        return ContentIdentity.of(f"drawing-annotate-{self.target}", (self.marks, self.palette, self.target), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the render thunk — the receipt threads the pre-run key; the layer payload is the layered() projection.
        return (await async_boundary(f"drawing.annotate.{self.target}", self._crossed)).map(lambda pair: pair[1])

    async def layered(self) -> RuntimeRail[LayerPlan]:
        # the engine rows as one LayerPlan tree — substrate data the layered/sheet consumers compose, not the producer rail.
        return (await async_boundary(f"drawing.annotate.{self.target}", self._crossed)).map(
            lambda pair: LayerPlan(schema=NamingSchema.ISO13567, roots=pair[0])
        )

    async def _crossed(self) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
        # synchronous native fold — crosses the runtime thread lane.
        crossed = await LanePolicy.offload(_ENGINES[self.target].arm, self, modality=Modality.THREAD, retry=RetryClass.OCCT)
        return crossed.default_with(lambda fault: _fold_raise(fault))


def _fold_raise(fault: object) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    # terminal collapse at the render boundary: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))


def _row(*, name: str, source: bytes, bbox: tuple[float, float, float, float] | None = None, group: str | None = None) -> LayerNode:
    # one engine row into the graphic/layer vocabulary — a group name nests as the LayerPlan path prefix, z rides row order.
    return LayerNode(name=name if group is None else f"{group}/{name}", intent=LayerIntent.ANNOTATION, content=Some(LayerContent(fragment=source)))


class AnnotateEngine(Struct, frozen=True):
    arm: Callable[[Annotate], tuple[tuple[LayerNode, ...], ArtifactReceipt]]
    engine: str  # the `ArtifactReceipt.Drawing` engine descriptor


# --- [OPERATIONS] -----------------------------------------------------------------------
def _mm(weight: LineWeight, /) -> float:
    return float(weight.value)  # the ISO 128-20 line-weight group (mm); the StrEnum value keys the drawn width


def _lw(weight: LineWeight, /) -> int:
    return round(float(weight.value) * 100)  # the DXF 1/100 mm integer the `GfxAttribs.lineweight` carries


def _style(mark: AnnotateOp, /) -> SymbolStyle:
    match mark:  # every case's style is its last payload slot; one total projection, never a per-tag getattr
        case (
            AnnotateOp(tag="leader", leader=(*_, style))
            | AnnotateOp(tag="textnote", textnote=(*_, style))
            | AnnotateOp(tag="revcloud", revcloud=(*_, style))
        ):
            return style
        case _ as unreachable:
            assert_never(unreachable)


def _points(mark: AnnotateOp, /) -> tuple[Point, ...]:
    match mark:  # the defining points the extent fold unions over
        case AnnotateOp(tag="leader", leader=(targets, landing, *_)):
            return (*targets, landing)
        case AnnotateOp(tag="textnote", textnote=(insert, *_)):
            return (insert,)
        case AnnotateOp(tag="revcloud", revcloud=(region, *_)):
            return region
        case _ as unreachable:
            assert_never(unreachable)


def _text_span(mark: AnnotateOp, /) -> Box | None:
    # the MEASURED note extent so the canvas sizes against the real run, never clipping a long note the point-hull
    # misses: Prose reads its line advances off the LineBrokenRun (no re-measure), Math reads ziamath getsize().
    match mark:
        case AnnotateOp(tag="textnote", textnote=(insert, NoteBody(tag="prose", prose=(_source, _run, broken)), style)):
            width = max((line.advance for line in broken.lines), default=0.0)
            return (insert[0], insert[1], insert[0] + width, insert[1] + len(broken.lines) * style.text_height.mm * 1.4)
        case AnnotateOp(tag="textnote", textnote=(insert, NoteBody(tag="math", math=source), style)):
            width, height = ziamath.Text(source, size=style.text_height.mm).getsize()
            return (insert[0], insert[1], insert[0] + width, insert[1] + height)
        case _:
            return None


def _bbox(marks: tuple[AnnotateOp, ...], /) -> Box:
    pts = tuple(point for mark in marks for point in _points(mark)) or ((0.0, 0.0),)
    spans = tuple(span for mark in marks if (span := _text_span(mark)) is not None)
    xs = tuple(p[0] for p in pts) + tuple(span[0] for span in spans)
    ys = tuple(p[1] for p in pts) + tuple(span[1] for span in spans)
    hi_x = max((*(x + 1.0 for x in (p[0] for p in pts)), *(span[2] for span in spans)))
    hi_y = max((*(y + 1.0 for y in (p[1] for p in pts)), *(span[3] for span in spans)))
    return (min(xs), min(ys), hi_x, hi_y)


@lru_cache(maxsize=1)
def _drawing_font() -> "ziafont.Font":
    # the ISO 3098 face — the SAME face typography/shape#SHAPE shapes with, so the decoded run's glyph clusters
    # index the source the outlining reads; loaded once inside the worker.
    return ziafont.Font(None)


def _route(marks: Block[AnnotateOp], /) -> frozendict[int, Point]:
    # the keynote/flag column solve: each landing's y is a Variable sharing the column x (the collinear axis is the
    # constant x0, no constraint), a required anchor, a required _MIN_SEP hard no-overlap, and a weak equal-gap.
    column = tuple((index, mark.leader[1]) for index, mark in enumerate(marks) if mark.tag == "leader" and mark.leader[3].tag in ("keynote", "flag"))
    if len(column) < 2:
        return frozendict()
    solver = kiwisolver.Solver()
    ys = tuple(kiwisolver.Variable(f"y{index}") for index, _ in column)
    x0 = sum(landing[0] for _, landing in column) / len(column)
    solver.addConstraint(ys[0] == column[0][1][1])  # required: anchor the first landing at its own y
    for lo, hi in pairwise(ys):  # Exemption: kiwisolver Solver is the stateful native sink; constraints add in place
        solver.addConstraint(hi - lo >= _MIN_SEP)  # required: hard no-overlap between adjacent landings
        solver.addConstraint((hi - lo == ys[1] - ys[0]) | kiwisolver.strength.weak)  # weak: even distribution
    solver.updateVariables()
    return frozendict({index: (x0, var.value()) for (index, _), var in zip(column, ys, strict=True)})


def _marker(term: Terminator, /) -> "drawsvg.Marker":
    # the reusable ISO 129-1/128-2 line-end def the leader marker_start auto-orients along (context-stroke inherits the leader pen).
    marker = drawsvg.Marker(-1.0, -1.0, 1.0, 1.0, orient="auto", id=f"term-{term.value}")
    match term:
        case Terminator.FILLED_ARROW:
            marker.append(drawsvg.Lines(-1.0, -1.0, 1.0, 0.0, -1.0, 1.0, close=True, fill="context-stroke"))
        case Terminator.OPEN_ARROW:
            marker.append(drawsvg.Lines(-1.0, -1.0, 1.0, 0.0, -1.0, 1.0, close=False, fill="none", stroke="context-stroke", stroke_width=0.3))
        case Terminator.OBLIQUE_STROKE:
            marker.append(drawsvg.Line(-1.0, 1.0, 1.0, -1.0, stroke="context-stroke", stroke_width=0.3))
        case Terminator.DOT:
            marker.append(drawsvg.Circle(0.0, 0.0, 0.6, fill="context-stroke"))
        case Terminator.NONE:
            pass
        case _ as unreachable:
            assert_never(unreachable)
    return marker


def _polygon(sides: int, radius: float, /) -> tuple[float, ...]:
    # the flat coord run a `drawsvg.Lines` closed polygon takes, derived from the side count and radius
    return tuple(
        coord
        for i in range(sides)
        for coord in (radius * math.cos(math.tau * i / sides - math.pi / 2.0), radius * math.sin(math.tau * i / sides - math.pi / 2.0))
    )


def _bubble_outline(shape: BubbleShape, radius: float, *, fill: str, stroke: str, width: float) -> "drawsvg.DrawingBasicElement":
    match shape:  # the keynote/flag mark outline; a filled `stroke="none"` backing is the SVG wipeout-mask twin
        case BubbleShape.CIRCLE:
            return drawsvg.Circle(0.0, 0.0, radius, fill=fill, stroke=stroke, stroke_width=width)
        case BubbleShape.RECTANGLE:
            return drawsvg.Rectangle(-radius, -radius, 2.0 * radius, 2.0 * radius, fill=fill, stroke=stroke, stroke_width=width)
        case BubbleShape.HEXAGON | BubbleShape.TRIANGLE | BubbleShape.DIAMOND:
            return drawsvg.Lines(*_polygon(_SIDES[shape], radius), close=True, fill=fill, stroke=stroke, stroke_width=width)
        case _ as unreachable:
            assert_never(unreachable)


def _outlined(text: str, at: Point, style: SymbolStyle, ramp: list[str], *, anchor: str = "left") -> "drawsvg.Group":
    # ISO 3098 note text as a font-independent ziafont `<path>` on a drawsvg.Raw (no f-string splice). MEASURED and
    # baseline-seated: getyofst() is the baseline-to-top offset, so the outline's baseline lands ON `at` (the leader
    # shoulder / bubble centre), not its bounding-box top; getsize() feeds _text_span.
    run = _drawing_font().text(text, size=style.text_height.mm, halign=anchor, color=ramp[style.stroke % len(ramp)])
    return drawsvg.Group(drawsvg.Raw(run.svg()), transform=f"translate({at[0]},{at[1] - run.getyofst()})")


def _bubble_group(label: str, shape: BubbleShape, at: Point, style: SymbolStyle, ramp: list[str], sheet_ref: str, *, mask: bool) -> "drawsvg.Group":
    radius = _BUBBLE * style.text_height.mm
    stroke = ramp[style.stroke % len(ramp)]
    group = drawsvg.Group(transform=f"translate({at[0] + radius},{at[1]})")
    if mask:  # the `add_wipeout` twin — a paper backing so the code reads over drawing geometry
        group.append(_bubble_outline(shape, radius, fill="white", stroke="none", width=0.0))
    group.append(_bubble_outline(shape, radius, fill="none", stroke=stroke, width=_mm(style.weight)))
    group.append(_outlined(label, (0.0, 0.0), style, ramp, anchor="center"))
    if sheet_ref:  # the keynote sheet/legend reference under the bisector
        group.append(_outlined(sheet_ref, (0.0, radius * 0.5), style, ramp, anchor="center"))
    return group


def _content_group(content: LeaderContent, at: Point, style: SymbolStyle, ramp: list[str]) -> "drawsvg.Group":
    match content:  # the leader-landing content; the total sub-match over `LeaderContent`
        case LeaderContent(tag="note", note=text):
            return _outlined(text, at, style, ramp)
        case LeaderContent(tag="keynote", keynote=(code, bubble, sheet_ref, mask)):
            return _bubble_group(code, bubble, at, style, ramp, sheet_ref, mask=mask)
        case LeaderContent(tag="flag", flag=(number, shape, mask)):
            return _bubble_group(number, shape, at, style, ramp, "", mask=mask)
        case _ as unreachable:
            assert_never(unreachable)


def _leader_group(
    targets: tuple[Point, ...], landing: Point, path: LeaderPath, content: LeaderContent, style: SymbolStyle, ramp: list[str]
) -> "drawsvg.Group":
    stroke = ramp[style.stroke % len(ramp)]
    home = (landing[0] + _SHOULDER * style.text_height.mm, landing[1])
    group = drawsvg.Group()
    for target in targets:  # one ISO 128-2 reference line per pointed feature (multileader), terminator at the point
        line = drawsvg.Path(stroke=stroke, stroke_width=_mm(style.weight), fill="none", marker_start=f"url(#term-{style.terminator.value})")
        line.M(target[0], target[1])
        _elbow(line, target, landing, path)
        line.L(home[0], home[1])  # the horizontal landing shoulder
        group.append(line)
    group.append(_content_group(content, home, style, ramp))
    return group


def _elbow(line: "drawsvg.Path", target: Point, landing: Point, path: LeaderPath, /) -> None:
    match path:  # Exemption: drawsvg.Path is the stateful cursor builder; the reference line appends in place
        case LeaderPath.SPLINE:
            line.Q(landing[0], target[1], landing[0], landing[1])
        case LeaderPath.STRAIGHT:
            line.L(landing[0], landing[1])
        case _ as unreachable:
            assert_never(unreachable)


def _note_group(insert: Point, body: NoteBody, style: SymbolStyle, ramp: list[str]) -> "drawsvg.Group":
    match body:  # the standalone note body; the total sub-match over `NoteBody`
        case NoteBody(tag="prose", prose=(source, run_bytes, broken)):
            return _prose_group(source, run_bytes, broken, insert, style, ramp)
        case NoteBody(tag="math", math=source):
            # the mixed text-and-`$math$` note through ziamath.Text — getyofst() seats the baseline on insert, getsize() sizes the canvas.
            run = ziamath.Text(source, size=style.text_height.mm, color=ramp[style.stroke % len(ramp)])
            return drawsvg.Group(drawsvg.Raw(run.svg()), transform=f"translate({insert[0]},{insert[1] - run.getyofst()})")
        case _ as unreachable:
            assert_never(unreachable)


def _prose_group(source: str, run_bytes: bytes, broken: LineBrokenRun, insert: Point, style: SymbolStyle, ramp: list[str]) -> "drawsvg.Group":
    # the Knuth-Plass general note: decode the shaped run, slice source per LineBrokenRun line through the run's
    # glyph clusters, outline each line at its own baseline — the break is the layout owner's, the outlining this owner's.
    run = msgspec.msgpack.decode(run_bytes, type=PositionedGlyphRun).glyphs
    group = drawsvg.Group(transform=f"translate({insert[0]},{insert[1]})")
    for order, line in enumerate(broken.lines):
        lo = run[line.start][1] if line.start < len(run) else 0
        hi = run[line.stop][1] if line.stop < len(run) else len(source)
        group.append(_outlined(source[lo:hi], (0.0, order * style.text_height.mm * 1.4), style, ramp))
    return group


def _revcloud_group(region: tuple[Point, ...], radius: float, mark: str, style: SymbolStyle, ramp: list[str]) -> "drawsvg.Group":
    # the revision enclosure as one self-contained drawsvg.Path scallop chain; a filled-band variant composes region outline/boolean.
    band = drawsvg.Path(stroke=ramp[style.stroke % len(ramp)], stroke_width=_mm(style.weight), fill="none")
    _scallop_path(band, region, radius)
    group = drawsvg.Group()
    group.append(band)
    if mark:  # the revision-delta tag at the first vertex
        group.append(_bubble_group(mark, BubbleShape.TRIANGLE, region[0], style, ramp, "", mask=True))
    return group


def _scallop_path(path: "drawsvg.Path", region: tuple[Point, ...], radius: float, /) -> None:
    # Exemption: drawsvg.Path is the stateful cursor builder; the scallop chain appends convex arc bumps in place
    path.M(*region[0])
    for a, b in zip(region, (*region[1:], region[0]), strict=True):
        bumps = max(1, round(math.dist(a, b) / (2.0 * radius)))
        for i in range(1, bumps + 1):
            end = (a[0] + (b[0] - a[0]) * i / bumps, a[1] + (b[1] - a[1]) * i / bumps)
            path.A(radius, radius, 0, False, True, end[0], end[1])
    path.Z()


# --- [BOUNDARIES] -----------------------------------------------------------------------
def _svg_mark(mark: AnnotateOp, ramp: list[str], landing: Point | None) -> "drawsvg.Group":
    match mark:
        case AnnotateOp(tag="leader", leader=(targets, home, path, content, style)):
            return _leader_group(targets, landing or home, path, content, style, ramp)
        case AnnotateOp(tag="textnote", textnote=(insert, body, style)):
            return _note_group(insert, body, style, ramp)
        case AnnotateOp(tag="revcloud", revcloud=(region, radius, mark_no, style)):
            return _revcloud_group(region, radius, mark_no, style, ramp)
        case _ as unreachable:
            assert_never(unreachable)


def _layer_svg(name: str, groups: tuple["drawsvg.Group", ...], box: Box) -> bytes:
    # one layer's marks into a named Group under one Drawing carrying the shared Marker defs the leader marker_start resolves against.
    canvas = drawsvg.Drawing(box[2] - box[0], box[3] - box[1], origin=(box[0], box[1]))
    for term in Terminator:  # Exemption: drawsvg Drawing is the mutable def/child container; defs and groups append in place
        if term is not Terminator.NONE:
            canvas.append_def(_marker(term))
    layer = drawsvg.Group(id=name)
    for group in groups:
        layer.append(group)
    canvas.append(layer)
    return canvas.as_svg().encode()


def _dxf_leader(
    doc: "ezdxf.document.Drawing", msp: object, targets: tuple[Point, ...], landing: Point, content: LeaderContent, style: SymbolStyle
) -> None:
    match content:
        case LeaderContent(tag="note", note=text):
            builder = msp.add_multileader_mtext("Standard")  # Exemption: the mleader builder is the stateful sink; leader lines add in place
            # the fuller `MTextEditor` surface the flat `.append` discards — the ISO 3098 lettering face inline (the
            # height rides the builder `char_height`); a multi-item keynote legend deepens to `.bullet_list` here.
            builder.set_content(str(MTextEditor().font("isocp").append(text)), char_height=style.text_height.mm)
            for target in targets:
                builder.add_leader_line(_side(target, landing), [Vec2(target), Vec2(landing)])
            builder.build(insert=Vec2(landing))
        case LeaderContent(tag="keynote", keynote=(code, bubble, sheet_ref, mask)):
            _dxf_bubble_leader(doc, msp, targets, landing, code, bubble, style, mask=mask)
        case LeaderContent(tag="flag", flag=(number, shape, mask)):
            _dxf_bubble_leader(doc, msp, targets, landing, number, shape, style, mask=mask)
        case _ as unreachable:
            assert_never(unreachable)


def _dxf_bubble_leader(
    doc: "ezdxf.document.Drawing",
    msp: object,
    targets: tuple[Point, ...],
    landing: Point,
    label: str,
    shape: BubbleShape,
    style: SymbolStyle,
    *,
    mask: bool,
) -> None:
    block = _bubble_block(doc, shape, style)
    builder = msp.add_multileader_block("Standard")  # Exemption: the block mleader builder is the stateful sink; leader lines add in place
    builder.set_content(block, scale=style.text_height.mm)
    builder.set_attribute("LABEL", label)
    for target in targets:
        builder.add_leader_line(_side(target, landing), [Vec2(target), Vec2(landing)])
    builder.build(insert=Vec2(landing))
    if mask:  # the wipeout backing so the code reads over drawing geometry
        msp.add_wipeout(_polygon_at(shape, landing, _BUBBLE * style.text_height.mm))


def _dxf_note(msp: object, insert: Point, body: NoteBody, style: SymbolStyle) -> None:
    attribs = GfxAttribs(layer=style.layer.compose(), lineweight=_lw(style.weight)).asdict()
    match body:  # Exemption: ezdxf MText is the stateful entity; the location/width set in place
        case NoteBody(tag="prose", prose=(source, _run, broken)):
            content = str(MTextEditor().font("isocp").height(style.text_height.mm).append(source))  # ISO 3098 face + height the raw source lacks
            note = msp.add_mtext(content, dxfattribs=attribs)
            note.dxf.width = max((line.advance for line in broken.lines), default=style.text_height.mm)
            note.set_location(Vec2(insert), attachment_point=MTextEntityAlignment.TOP_LEFT)
        case NoteBody(tag="math", math=source):
            msp.add_mtext(source, dxfattribs=attribs).set_location(Vec2(insert), attachment_point=MTextEntityAlignment.TOP_LEFT)
        case _ as unreachable:
            assert_never(unreachable)


def _dxf_revcloud(msp: object, region: tuple[Point, ...], radius: float, style: SymbolStyle) -> None:
    # the scalloped enclosure as one bulged LWPolyline — each convex bump a `(x, y, 0, 0, 1.0)` semicircle-bulge
    # vertex, never a hand-emitted arc series; the `xyseb` format carries the per-vertex bulge.
    points = tuple(
        (a[0] + (b[0] - a[0]) * i / bumps, a[1] + (b[1] - a[1]) * i / bumps, 0.0, 0.0, 1.0)
        for a, b in zip(region, (*region[1:], region[0]), strict=True)
        for bumps in (max(1, round(math.dist(a, b) / (2.0 * radius))),)
        for i in range(1, bumps + 1)
    )
    msp.add_lwpolyline(points, format="xyseb", close=True, dxfattribs=GfxAttribs(layer=style.layer.compose(), lineweight=_lw(style.weight)).asdict())


def _dxf_mark(doc: "ezdxf.document.Drawing", msp: object, mark: AnnotateOp, landing: Point | None) -> None:
    match mark:
        case AnnotateOp(tag="leader", leader=(targets, home, _path, content, style)):
            _dxf_leader(doc, msp, targets, landing or home, content, style)
        case AnnotateOp(tag="textnote", textnote=(insert, body, style)):
            _dxf_note(msp, insert, body, style)
        case AnnotateOp(tag="revcloud", revcloud=(region, radius, _mark_no, style)):
            _dxf_revcloud(msp, region, radius, style)
        case _ as unreachable:
            assert_never(unreachable)


def _bubble_block(doc: "ezdxf.document.Drawing", shape: BubbleShape, style: SymbolStyle) -> str:
    # one reusable keynote/flag block placed by N `add_blockref`, never a per-placement geometry copy
    name = f"ANNOT_{shape.value}"
    if name not in doc.blocks:
        block = doc.blocks.new(name)  # Exemption: ezdxf BlockLayout is the GraphicsFactory sink; the outline + attdef add in place
        block.add_lwpolyline(_polygon_at(shape, (0.0, 0.0), _BUBBLE * style.text_height.mm), close=True)
        block.add_attdef("LABEL", (0.0, 0.0))
    return name


def _side(target: Point, landing: Point, /) -> "ConnectionSide":
    return ConnectionSide.right if target[0] > landing[0] else ConnectionSide.left


def _polygon_at(shape: BubbleShape, at: Point, radius: float, /) -> tuple[tuple[float, float], ...]:
    match shape:  # the closed vertex ring an `add_lwpolyline`/`add_wipeout` takes
        case BubbleShape.CIRCLE:
            return tuple((at[0] + radius * math.cos(math.tau * i / 16), at[1] + radius * math.sin(math.tau * i / 16)) for i in range(16))
        case BubbleShape.RECTANGLE:
            return (
                (at[0] - radius, at[1] - radius),
                (at[0] + radius, at[1] - radius),
                (at[0] + radius, at[1] + radius),
                (at[0] - radius, at[1] + radius),
            )
        case BubbleShape.HEXAGON | BubbleShape.TRIANGLE | BubbleShape.DIAMOND:
            flat = _polygon(_SIDES[shape], radius)
            return tuple((at[0] + flat[2 * i], at[1] + flat[2 * i + 1]) for i in range(_SIDES[shape]))
        case _ as unreachable:
            assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------
_SIDES: frozendict[BubbleShape, int] = frozendict({BubbleShape.TRIANGLE: 3, BubbleShape.DIAMOND: 4, BubbleShape.HEXAGON: 6})


def _svg_engine(annotate: Annotate) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    ziafont.config.precision = ziamath.config.precision = _PRECISION  # once inside the offload lane — same d-floats -> same content key
    ramp = hex_ramp(annotate.palette)
    routed = _route(Block.of_seq(annotate.marks))
    box = _bbox(annotate.marks)
    groups: dict[str, list["drawsvg.Group"]] = {}
    for index, mark in enumerate(
        annotate.marks
    ):  # Exemption: the drawsvg named-layer tree buckets marks by `SymbolStyle.layer` through a mutable dict of group lists
        groups.setdefault(_style(mark).layer.compose(), []).append(_svg_mark(mark, ramp, routed.get(index)))
    layers = tuple(_row(name=name, source=_layer_svg(name, tuple(items), box), bbox=box) for name, items in sorted(groups.items()))
    key = annotate._key
    return layers, ArtifactReceipt.Drawing(
        key,
        "drawing-annotate",
        len(annotate.marks),
        _LEADER_STYLE,
        int(box[2] - box[0]),
        int(box[3] - box[1]),
        sum(len(layer.source) for layer in layers),
    )


def _dxf_extent(msp: object, fallback: Box, /) -> tuple[int, int]:
    # the TRUE rendered extent over the authored entities via ezdxf.bbox.extents — never a point-hull the placed blocks and masks miss.
    box = bbox.extents(msp, fast=True)
    return (round(box.size.x), round(box.size.y)) if box.has_data else (int(fallback[2] - fallback[0]), int(fallback[3] - fallback[1]))


def _dxf_engine(annotate: Annotate) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    doc = ezdxf.new("R2018", setup=True)
    msp = doc.modelspace()
    routed = _route(Block.of_seq(annotate.marks))
    box = _bbox(annotate.marks)
    for index, mark in enumerate(annotate.marks):  # Exemption: ezdxf Modelspace is the GraphicsFactory sink; add_* mutate the layout in place
        _dxf_mark(doc, msp, mark, routed.get(index))
    width, height = _dxf_extent(msp, box)
    stream = io.StringIO()
    doc.write(stream)
    data = stream.getvalue().encode()
    key = annotate._key
    return (_row(name="dxf", source=data, bbox=box),), ArtifactReceipt.Drawing(
        key, "drawing-annotate", len(annotate.marks), _LEADER_STYLE, width, height, len(data)
    )


_ENGINES: frozendict[SymbolTarget, AnnotateEngine] = frozendict({
    SymbolTarget.SVG: AnnotateEngine(arm=_svg_engine, engine="drawsvg"),
    SymbolTarget.DXF: AnnotateEngine(arm=_dxf_engine, engine="ezdxf"),
})


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["Annotate", "AnnotateOp", "BubbleShape", "LeaderContent", "LeaderPath", "NoteBody"]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
