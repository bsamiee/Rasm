# [PY_ARTIFACTS_DRAWING_ANNOTATE]

ISO 128-2 annotation lowering lives in `Annotate`, one owner over `AnnotateOp.leader`/`textnote`/`revcloud`. `LeaderContent` carries `note`/`keynote`/`flag` landing payloads behind one leader geometry, `NoteBody` carries prose or math, and `revcloud` carries an `Option[str]` delta tag. `LeaderPath`, `BubbleShape`, and `Masking` are policy values; `Option[str]` distinguishes absent references from present text. SVG and DXF consume every output-bearing case, so spline leaders and delta-tagged clouds remain distinct on both targets.

`SymbolStyle` supplies the drawing-plane palette, `LineWeight`, `TextHeight`, `LayerName`, and `Terminator` axes. `PositionedGlyphRun` and `LineBrokenRun` remain canonical owners inside prose notes, `ziafont` outlines the selected line text, the `typography/math#MATH` `Formula` owner typesets mixed math, and `kiwisolver.Solver` routes keynote columns. `LanePolicy` offloads the synchronous engines, `ArtifactReceipt.Drawing` records the same pre-run identity as `ArtifactWork`, and `LayerMeta.aec` preserves `LayerSchema.ISO13567` naming.

## [01]-[INDEX]

- [01]-[ANNOTATE]: `Annotate` dual-lowers closed `AnnotateOp.leader`/`textnote`/`revcloud` marks over `SymbolTarget` into named SVG layers or an `ezdxf` layout.

## [02]-[ANNOTATE]

- Owner: `Annotate` holds `marks`, `Palette`, `LanePolicy`, and `SymbolTarget`; `AnnotateOp.of`, `LeaderContent.of`, and `NoteBody.of` discriminate on input shape. `LeaderContent` shares leader geometry across note, keynote, and flag landings, while `NoteBody` keeps prose and math payloads disjoint.
- Cases: each `AnnotateOp` case carries typed geometry plus `SymbolStyle`. `LeaderPath` selects straight or spline geometry, `BubbleShape` generates keynote outlines, `Masking` selects paper backing, and `Option[str]` carries sheet references and revision tags without null/default ghosts.
- Entry: `Annotate.over` normalizes `AnnotateOp | Iterable[AnnotateOp]` by a structural `match` at the head — never a `batch` knob. `emit()` is the schedulable `ArtifactWork`; `_emit` maps the receipt half and `layered()` the `LayerPlan` projection off the same `_crossed` hop, `async_boundary` narrowed to the `_FAULTS` engine-raise tuple so a non-engine raise crosses as a defect. `_svg_engine` and `_dxf_engine` are the `SymbolTarget`-keyed dual lowering, the SVG arm folding each mark into its `SymbolStyle.layer` `drawsvg.Group` under one `Drawing` carrying the shared terminator defs, the DXF arm through the `ezdxf` multileader/mtext/wipeout/lwpolyline model with `set_leader_properties(leader_type=)` carrying the `LeaderPath` axis natively.
- Auto: `SymbolStyle.fill`/`stroke` index one render-time palette. `getyofst()` and `getsize()` baseline-seat measured text, while pinned `ziafont.config.precision` stabilizes SVG bytes (math precision is `Formula`-owned config). `revcloud` folds each edge into `max(1, round(length / (2·radius)))` convex bumps on both targets. A keynote column uses `kiwisolver.Solver` with required anchors and `_MIN_SEP`, then `updateVariables()` publishes routed landings. Each SVG row carries `LayerName` through `LayerMeta.aec` for `LayerSchema.ISO13567`.
- Packages: `drawsvg` builds named SVG layers and marker/path geometry; `ziafont` outlines measured text; `typography/math#MATH` `Formula` renders mixed math; `ezdxf` supplies multileaders, mtext, blocks, wipeouts, polylines, `LeaderType`, and `bbox.extents`; `kiwisolver` solves landing columns; `expression` supplies `Option`, `Block`, and `Map` folds.
- Growth: a leader landing adds one `LeaderContent` case and two lowering arms; a note body adds one `NoteBody` case; an annotation grammar adds one `AnnotateOp` case; bubble polygons add one `BubbleShape` member plus one `_SIDES` row. New visual behavior enters through an existing policy owner.
- Boundary: no dimension, symbol, or sheet-set logic — `drawing/dimension#DIMENSION`, `drawing/symbol#SYMBOL`, `composition/sheet#SHEET`. `drawsvg` owns the SVG container and leader/scallop builders, `ziafont` the text outline and `typography/math#MATH` the math typeset, `ezdxf` the DXF model, `graphic/vector/region#REGION` the boolean/offset, `kiwisolver` the solve, `typography/layout#LAYOUT`/`typography/shape#SHAPE` the line-break and shaping, `export/layered#LAYERED` the layer binding, and `csharp:Rasm.Bim` the IFC; identity minting is the runtime's.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import math
from builtins import frozendict
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import lru_cache
from itertools import pairwise
from typing import Final, Literal, NoReturn, Self, assert_never

from expression import Nothing, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, msgpack

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import RuntimeRail, async_boundary

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.drawing.regime import LayerName, LayerSchema, LineWeight, Terminator
from rasm.artifacts.drawing.symbol import SymbolStyle, SymbolTarget
from rasm.artifacts.graphic.layer import LayerContent, LayerIntent, LayerMeta, LayerNode, LayerPlan
from rasm.artifacts.typography.layout import LineBrokenRun, ParagraphSpec, broken
from rasm.artifacts.typography.math import Formula, FormulaSpec, MixedSpec, seat
from rasm.artifacts.typography.shape import PositionedGlyphRun
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp

# each proxy reifies on first render-arm use in the offloaded worker
lazy import drawsvg
lazy import ezdxf
lazy import kiwisolver
lazy import ziafont
lazy from ezdxf import bbox
lazy from ezdxf.enums import MTextEntityAlignment
lazy from ezdxf.gfxattribs import GfxAttribs
lazy from ezdxf.math import Vec2
lazy from ezdxf.render.mleader import ConnectionSide, LeaderType
lazy from ezdxf.tools.text import MTextEditor

# --- [TYPES] ----------------------------------------------------------------------------
type Point = tuple[float, float]
type Box = tuple[float, float, float, float]
type Ramp = tuple[str, ...]
type AnnotateTag = Literal["leader", "textnote", "revcloud"]
type Engine = Callable[[Annotate], tuple[tuple[LayerNode, ...], ArtifactReceipt]]
type LeaderLanding = str | tuple[str, BubbleShape, Option[str], Masking] | tuple[str, BubbleShape, Masking]
type NoteSource = str | ParagraphSpec | tuple[PositionedGlyphRun, LineBrokenRun]  # ParagraphSpec is the canonical prose shape
type AnnotateSpec = (
    tuple[tuple[Point, ...], Point, LeaderPath, LeaderContent, SymbolStyle]
    | tuple[Point, NoteBody, SymbolStyle]
    | tuple[tuple[Point, ...], float, Option[str], SymbolStyle]
)

_SHOULDER: float = 2.0  # landing-shoulder length, in text-height multiples (ISO 128-2 leader landing)
_BUBBLE: float = 1.4  # keynote/flag bubble radius, in text-height multiples
_MIN_SEP: float = 8.0  # keynote/flag-column minimum vertical separation (drawing units)
_LEADER_STYLE: str = "ISO-128-2"  # the annotation convention the receipt style slot carries
_PRECISION: int = 3  # ziafont emitted-d-float places — the content-key determinism lever set once per offloaded arm
# Engine raises cross through exact provider categories; unlisted defects propagate.
_FAULTS: tuple[type[Exception], ...] = (ValueError, OSError)
_CANON: Final = msgpack.Encoder(order="deterministic")  # the stable preimage encoding the bare `ContentIdentity.key` mint addresses


class LeaderPath(StrEnum):  # ISO 128-2 leader reference-line geometry (ezdxf `render.mleader.LeaderType` twin)
    STRAIGHT = "straight"  # straight polyline — the drawing-office default
    SPLINE = "spline"  # smoothed spline leader through the elbow


class BubbleShape(StrEnum):  # the keynote/flag landing-mark outline
    CIRCLE = "circle"  # keynote circle bubble
    HEXAGON = "hexagon"
    TRIANGLE = "triangle"  # flag-note delta
    DIAMOND = "diamond"
    RECTANGLE = "rectangle"  # boxed keynote


class Masking(StrEnum):
    NONE = "none"
    PAPER = "paper"


# --- [VOCABULARY] -----------------------------------------------------------------------
@tagged_union(frozen=True)
class LeaderContent:
    # Landing payload selects the `ezdxf` mtext or block builder.
    tag: Literal["note", "keynote", "flag"] = tag()
    note: str = case()  # MTextEditor-formatted mtext content
    keynote: tuple[str, BubbleShape, Option[str], Masking] = case()
    flag: tuple[str, BubbleShape, Masking] = case()

    def __post_init__(self) -> None:
        match self:
            case LeaderContent(tag="note", note=text) if not text.strip():
                raise ValueError("leader note content must not be empty")
            case LeaderContent(tag="keynote", keynote=(code, _, _, _)) if not code.strip():
                raise ValueError("keynote code must not be empty")
            case LeaderContent(tag="flag", flag=(number, _, _)) if not number.strip():
                raise ValueError("flag number must not be empty")
            case _:
                pass

    @classmethod
    def of(cls, source: LeaderLanding, /) -> "LeaderContent":
        match source:
            case str() as text:
                return cls(note=text)
            case (code, BubbleShape() as bubble, Option() as sheet, Masking() as masking):
                return cls(keynote=(code, bubble, sheet, masking))
            case (number, BubbleShape() as shape, Masking() as masking):
                return cls(flag=(number, shape, masking))
            case unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class NoteBody:
    # Each note mode carries only its own payload.
    tag: Literal["prose", "math"] = tag()
    prose: tuple[PositionedGlyphRun, LineBrokenRun] = case()
    math: str = case()  # mixed text + $math$ source the typography/math Formula owner typesets

    def __post_init__(self) -> None:
        match self:
            case NoteBody(tag="math", math=source) if not source.strip():
                raise ValueError("math note source must not be empty")
            case NoteBody(tag="prose", prose=(run, broken)) if not run.source.strip() or not broken.lines:
                raise ValueError("prose note requires shaped source and line breaks")
            case _:
                pass

    @classmethod
    def of(cls, source: NoteSource, /) -> "NoteBody":
        match source:
            case str() as math:
                return cls(math=math)
            case ParagraphSpec() as spec:
                # CANONICAL prose ingress: layout's public total-fit projection derives the breaks from the
                # spec's own inputs; the precomputed pair below stays admitted only as the already-shaped working set.
                return cls(prose=(msgpack.decode(spec.run, type=PositionedGlyphRun), broken(spec)))
            case (PositionedGlyphRun() as run, LineBrokenRun() as fitted):
                return cls(prose=(run, fitted))
            case unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class AnnotateOp:
    tag: AnnotateTag = tag()
    leader: tuple[tuple[Point, ...], Point, LeaderPath, LeaderContent, SymbolStyle] = case()
    textnote: tuple[Point, NoteBody, SymbolStyle] = case()
    revcloud: tuple[tuple[Point, ...], float, Option[str], SymbolStyle] = case()

    def __post_init__(self) -> None:
        match self:
            case AnnotateOp(tag="leader", leader=(targets, _, _, _, _)) if not targets:
                raise ValueError("leader requires at least one target")
            case AnnotateOp(tag="revcloud", revcloud=(region, radius, _, _)) if len(region) < 3 or radius <= 0.0:
                raise ValueError("revision cloud requires at least three vertices and a positive radius")
            case _:
                pass

    @classmethod
    def of(cls, spec: AnnotateSpec, /) -> "AnnotateOp":
        match spec:
            case (targets, landing, LeaderPath() as path, LeaderContent() as content, SymbolStyle() as style):
                return cls(leader=(targets, landing, path, content, style))
            case (insert, NoteBody() as body, SymbolStyle() as style):
                return cls(textnote=(insert, body, style))
            case (region, radius, Option() as mark, SymbolStyle() as style):
                return cls(revcloud=(region, radius, mark, style))
            case unreachable:
                assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------
class Annotate(Struct, frozen=True):
    marks: tuple[AnnotateOp, ...]
    palette: Palette
    lane: LanePolicy
    target: SymbolTarget = SymbolTarget.SVG

    def __post_init__(self) -> None:
        if not self.marks:
            raise ValueError("annotation set must not be empty")

    @classmethod
    def over(cls, marks: AnnotateOp | Iterable[AnnotateOp], palette: Palette, lane: LanePolicy, /, *, target: SymbolTarget = SymbolTarget.SVG) -> Self:
        match marks:  # the one modal-arity head — a lone mark is the singleton, an iterable the multi-mark sheet
            case AnnotateOp():
                return cls(marks=(marks,), palette=palette, lane=lane, target=target)
            case _:
                return cls(marks=tuple(marks), palette=palette, lane=lane, target=target)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.marks)))

    @property
    def _key(self) -> ContentKey:
        # key over the frozen INPUT spec, minted pre-run through the bare `ContentIdentity.key` (`of` returns the
        # railed `RuntimeRail[ContentKey]`) — never over rendered layer bytes; the lane never enters identity.
        return ContentIdentity.key(f"drawing-annotate-{self.target}", _CANON.encode((self.marks, self.palette, self.target)))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # Receipt and layer projections thread one pre-run key.
        return (await async_boundary(f"drawing.annotate.{self.target}", self._crossed, catch=_FAULTS)).map(lambda pair: pair[1])

    async def layered(self) -> RuntimeRail[LayerPlan]:
        # Engine rows project into one layer tree.
        return (await async_boundary(f"drawing.annotate.{self.target}", self._crossed, catch=_FAULTS)).map(
            lambda pair: LayerPlan(schema=LayerSchema.ISO13567, roots=pair[0])
        )

    async def _crossed(self) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
        # synchronous native fold — crosses the runtime thread lane.
        crossed = await self.lane.offload(Kernel.of(_ENGINES[self.target], KernelTrait.RELEASING), self)
        return crossed.default_with(self._raise)

    @staticmethod
    def _raise(fault: object) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
        raise ValueError(str(fault))


def _row(*, name: str, source: bytes, aec: Option[LayerName]) -> LayerNode:
    return LayerNode.Leaf(LayerMeta(name=name, intent=LayerIntent.ANNOTATION, aec=aec), LayerContent.Fragment(source))


# --- [OPERATIONS] -----------------------------------------------------------------------
def _math_raise(fault: object) -> NoReturn:
    # a Formula typeset refusal re-raises as the engine fault the `_FAULTS` async_boundary folds onto the
    # runtime rail, so the original failure reaches the caller and never degrades into a silent render.
    raise ValueError(f"math note typeset: {fault}")


def _lw(weight: LineWeight, /) -> int:
    return round(weight.mm * 100)  # the DXF 1/100 mm integer the `GfxAttribs.lineweight` carries


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


def _text_span(mark: AnnotateOp, /) -> Option[Box]:
    match mark:
        case AnnotateOp(tag="textnote", textnote=(insert, NoteBody(tag="prose", prose=(_run, broken)), style)):
            width = max((line.advance for line in broken.lines), default=0.0)
            return Some((insert[0], insert[1], insert[0] + width, insert[1] + len(broken.lines) * style.text_height.mm * 1.4))
        case AnnotateOp(tag="textnote", textnote=(insert, NoteBody(tag="math", math=source), style)):
            # mixed math measures through the typography/math Formula owner — never a second ziamath import;
            # a typeset refusal propagates via `_math_raise`, never a zero-dimension span.
            laid = Formula(spec=FormulaSpec(mixed=MixedSpec(source=source, size=style.text_height.mm))).laid()
            width, height = laid.map(lambda frag: (frag.metrics[1], frag.metrics[2])).default_with(_math_raise)
            return Some((insert[0], insert[1], insert[0] + width, insert[1] + height))
        case AnnotateOp(tag="leader") | AnnotateOp(tag="revcloud"):
            return Nothing
        case unreachable:
            assert_never(unreachable)


def _union(boxes: Iterable[Box], /) -> Box:
    lo_x, lo_y, hi_x, hi_y = zip(*boxes, strict=True)
    return (min(lo_x), min(lo_y), max(hi_x), max(hi_y))


def _bubble_box(at: Point, style: SymbolStyle, /) -> Box:
    # `_bubble_group` translates the disc by +radius, so the footprint spans one diameter right of the seat.
    radius = _BUBBLE * style.text_height.mm
    return (at[0], at[1] - radius, at[0] + 2.0 * radius, at[1] + radius)


def _reach(content: LeaderContent, home: Point, style: SymbolStyle, /) -> Box:
    # landing-content footprint past the shoulder: measured note text or the keynote/flag bubble disc.
    match content:
        case LeaderContent(tag="note", note=text):
            width, height = _drawing_font(style.face).text(text, size=style.text_height.mm).getsize()
            return (home[0], home[1] - height, home[0] + width, home[1] + height)
        case LeaderContent(tag="keynote") | LeaderContent(tag="flag"):
            return _bubble_box(home, style)
        case _ as unreachable:
            assert_never(unreachable)


def _extent(mark: AnnotateOp, landing: Option[Point], /) -> Box:
    # one total per-mark extent over the FULL rendered geometry — routed landing, shoulder run, landing content,
    # terminator pad, scallop bulge, and delta tag — never the bare defining points a canvas clip then truncates.
    match mark:
        case AnnotateOp(tag="leader", leader=(targets, home, _, content, style)):
            seat = landing.default_value(home)
            pad = style.weight.mm  # SVG marker units default to strokeWidth, so the terminator box spans one pen weight
            shoulder = (seat[0] + _SHOULDER * style.text_height.mm, seat[1])
            return _union((
                *((p[0] - pad, p[1] - pad, p[0] + pad, p[1] + pad) for p in (*targets, seat)),
                _reach(content, shoulder, style),
            ))
        case AnnotateOp(tag="textnote", textnote=(insert, _, _)):
            return _text_span(mark).default_value((insert[0], insert[1], insert[0] + 1.0, insert[1] + 1.0))
        case AnnotateOp(tag="revcloud", revcloud=(region, radius, mark_no, style)):
            band = _union((p[0] - radius, p[1] - radius, p[0] + radius, p[1] + radius) for p in region)
            return mark_no.map(lambda _tag: _union((band, _bubble_box(region[0], style)))).default_value(band)
        case _ as unreachable:
            assert_never(unreachable)


def _bbox(marks: tuple[AnnotateOp, ...], routed: Map[int, Point], /) -> Box:
    # marks are non-empty by construction (`Annotate.__post_init__`), so the union always has an operand.
    return _union(_extent(mark, routed.try_find(index)) for index, mark in enumerate(marks))


@lru_cache(maxsize=4)
def _drawing_font(face: str = "") -> "ziafont.Font":
    # Style-carried face identity resolves BOTH lowerings — "" selects the bundled default here and isocp on the
    # DXF arm, so the same logical text never diverges on an ambient pick.
    return ziafont.Font(face or None)


def _routable(index: int, mark: AnnotateOp, /) -> Option[tuple[int, Point]]:
    match mark:
        case AnnotateOp(
            tag="leader",
            leader=(_, landing, _, LeaderContent(tag="keynote") | LeaderContent(tag="flag"), _),
        ):
            return Some((index, landing))
        case AnnotateOp(tag="leader", leader=(_, _, _, LeaderContent(tag="note"), _)) | AnnotateOp(tag="textnote") | AnnotateOp(tag="revcloud"):
            return Nothing
        case unreachable:
            assert_never(unreachable)


def _route(marks: Block[AnnotateOp], /) -> Map[int, Point]:
    # Required anchors and `_MIN_SEP` prevent overlap; weak equal gaps regularize the column.
    column = marks.mapi(_routable).choose(lambda held: held)
    if len(column) < 2:
        return Map.empty()
    solver = kiwisolver.Solver()
    ys = tuple(kiwisolver.Variable(f"y{index}") for index, _ in column)
    x0 = sum(landing[0] for _, landing in column) / len(column)
    solver.addConstraint(ys[0] == column[0][1][1])  # required: anchor the first landing at its own y
    for lo, hi in pairwise(ys):  # Exemption: kiwisolver Solver is the stateful native sink; constraints add in place
        solver.addConstraint(hi - lo >= _MIN_SEP)  # required: hard no-overlap between adjacent landings
        solver.addConstraint((hi - lo == ys[1] - ys[0]) | kiwisolver.strength.weak)  # weak: even distribution
    solver.updateVariables()
    return Map.of_seq((index, (x0, var.value())) for (index, _), var in zip(column, ys, strict=True))


def _marker(term: Terminator, /) -> "drawsvg.Marker":
    # `context-stroke` inherits the leader pen while `orient="auto"` follows its tangent.
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
        case Terminator.ORIGIN_INDICATION:
            marker.append(drawsvg.Circle(0.0, 0.0, 0.6, fill="none", stroke="context-stroke", stroke_width=0.3))
        case Terminator.NONE:
            pass
        case _ as unreachable:
            assert_never(unreachable)
    return marker


def _polygon(sides: int, radius: float, /) -> tuple[float, ...]:
    # Side count generates the flat `drawsvg.Lines` coordinate run.
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


def _outlined(text: str, at: Point, style: SymbolStyle, ramp: Ramp, *, anchor: str = "left") -> "drawsvg.Group":
    # `getyofst()` seats the provider-generated outline's baseline on the leader shoulder or bubble center.
    run = _drawing_font(style.face).text(text, size=style.text_height.mm, halign=anchor, color=ramp[style.stroke % len(ramp)])
    return drawsvg.Group(drawsvg.Raw(run.svg()), transform=f"translate({at[0]},{at[1] - run.getyofst()})")


def _bubble_group(
    label: str,
    shape: BubbleShape,
    at: Point,
    style: SymbolStyle,
    ramp: Ramp,
    sheet_ref: Option[str],
    masking: Masking,
) -> "drawsvg.Group":
    radius = _BUBBLE * style.text_height.mm
    stroke = ramp[style.stroke % len(ramp)]
    group = drawsvg.Group(transform=f"translate({at[0] + radius},{at[1]})")
    match masking:
        case Masking.PAPER:
            group.append(_bubble_outline(shape, radius, fill="white", stroke="none", width=0.0))
        case Masking.NONE:
            pass
        case unreachable:
            assert_never(unreachable)
    group.append(_bubble_outline(shape, radius, fill="none", stroke=stroke, width=style.weight.mm))
    group.append(_outlined(label, (0.0, 0.0), style, ramp, anchor="center"))
    match sheet_ref:
        case Option(tag="some", some=reference):
            group.append(_outlined(reference, (0.0, radius * 0.5), style, ramp, anchor="center"))
        case Option(tag="none"):
            pass
        case unreachable:
            assert_never(unreachable)
    return group


def _content_group(content: LeaderContent, at: Point, style: SymbolStyle, ramp: Ramp) -> "drawsvg.Group":
    match content:  # the leader-landing content; the total sub-match over `LeaderContent`
        case LeaderContent(tag="note", note=text):
            return _outlined(text, at, style, ramp)
        case LeaderContent(tag="keynote", keynote=(code, bubble, sheet_ref, masking)):
            return _bubble_group(code, bubble, at, style, ramp, sheet_ref, masking)
        case LeaderContent(tag="flag", flag=(number, shape, masking)):
            return _bubble_group(number, shape, at, style, ramp, Nothing, masking)
        case _ as unreachable:
            assert_never(unreachable)


def _leader_group(
    targets: tuple[Point, ...], landing: Point, path: LeaderPath, content: LeaderContent, style: SymbolStyle, ramp: Ramp
) -> "drawsvg.Group":
    stroke = ramp[style.stroke % len(ramp)]
    home = (landing[0] + _SHOULDER * style.text_height.mm, landing[1])
    group = drawsvg.Group()
    for target in targets:  # one ISO 128-2 reference line per pointed feature (multileader), terminator at the point
        line = drawsvg.Path(stroke=stroke, stroke_width=style.weight.mm, fill="none", marker_start=f"url(#term-{style.terminator.value})")
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


def _note_group(insert: Point, body: NoteBody, style: SymbolStyle, ramp: Ramp) -> "drawsvg.Group":
    match body:  # the standalone note body; the total sub-match over `NoteBody`
        case NoteBody(tag="prose", prose=(run, broken)):
            return _prose_group(run, broken, insert, style, ramp)
        case NoteBody(tag="math", math=source):
            # typography/math's Formula owner typesets and `seat` resolves the baseline origin — never a second ziamath import.
            laid = Formula(spec=FormulaSpec(mixed=MixedSpec(source=source, size=style.text_height.mm, color=ramp[style.stroke % len(ramp)]))).laid()
            match laid:
                case Result(tag="ok", ok=frag):
                    sx, sy = seat(frag, insert[0], insert[1])
                    return drawsvg.Group(drawsvg.Raw(frag.svg), transform=f"translate({sx},{sy})")
                case Result(tag="error", error=fault):
                    _math_raise(fault)  # a typeset refusal propagates onto the rail — never an empty group masking the failure
        case _ as unreachable:
            assert_never(unreachable)


def _prose_group(run: PositionedGlyphRun, broken: LineBrokenRun, insert: Point, style: SymbolStyle, ramp: Ramp) -> "drawsvg.Group":
    group = drawsvg.Group(transform=f"translate({insert[0]},{insert[1]})")
    for order, line in enumerate(broken.lines):
        group.append(_outlined(run.source[line.source_start : line.source_stop], (0.0, order * style.text_height.mm * 1.4), style, ramp))
    return group


def _revcloud_group(region: tuple[Point, ...], radius: float, mark: Option[str], style: SymbolStyle, ramp: Ramp) -> "drawsvg.Group":
    # One `drawsvg.Path` carries the scallop chain.
    band = drawsvg.Path(stroke=ramp[style.stroke % len(ramp)], stroke_width=style.weight.mm, fill="none")
    _scallop_path(band, region, radius)
    group = drawsvg.Group()
    group.append(band)
    match mark:
        case Option(tag="some", some=number):
            group.append(_bubble_group(number, BubbleShape.TRIANGLE, region[0], style, ramp, Nothing, Masking.PAPER))
        case Option(tag="none"):
            pass
        case unreachable:
            assert_never(unreachable)
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
def _svg_mark(mark: AnnotateOp, ramp: Ramp, landing: Option[Point]) -> "drawsvg.Group":
    match mark:
        case AnnotateOp(tag="leader", leader=(targets, home, path, content, style)):
            return _leader_group(targets, landing.default_value(home), path, content, style, ramp)
        case AnnotateOp(tag="textnote", textnote=(insert, body, style)):
            return _note_group(insert, body, style, ramp)
        case AnnotateOp(tag="revcloud", revcloud=(region, radius, mark_no, style)):
            return _revcloud_group(region, radius, mark_no, style, ramp)
        case _ as unreachable:
            assert_never(unreachable)


def _layer_svg(name: str, groups: Block["drawsvg.Group"], box: Box) -> bytes:
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
    doc: "ezdxf.document.Drawing",
    msp: object,
    targets: tuple[Point, ...],
    landing: Point,
    path: LeaderPath,
    content: LeaderContent,
    style: SymbolStyle,
    ramp: Ramp,
) -> None:
    match content:
        case LeaderContent(tag="note", note=text):
            # the composed ISO 13567 layer rides dxfattribs — an attribs-less multileader lands on layer 0 and breaks the schema.
            builder = msp.add_multileader_mtext("Standard", dxfattribs=GfxAttribs(layer=style.layer.compose()).asdict())  # Exemption: the mleader builder is the stateful sink; leader lines add in place
            builder.set_content(str(MTextEditor().font(style.face or "isocp").append(_mtext_safe(text))), char_height=style.text_height.mm)
            # SVG/DXF parity: the palette stroke and the admitted Terminator land natively — leader color through
            # set_leader_properties(color=RGB), the arrowhead through set_arrow_properties over the ARROWS names.
            builder.set_leader_properties(color=_dxf_rgb(ramp[style.stroke % len(ramp)]), lineweight=_lw(style.weight), leader_type=_leader_type(path))
            builder.set_arrow_properties(name=_DXF_ARROW[style.terminator], size=style.text_height.mm)
            for target in targets:
                builder.add_leader_line(_side(target, landing), [Vec2(target), Vec2(landing)])
            builder.build(insert=Vec2(landing))
        case LeaderContent(tag="keynote", keynote=(code, bubble, sheet_ref, masking)):
            _dxf_bubble_leader(doc, msp, targets, landing, path, code, bubble, style, sheet_ref, masking, ramp)
        case LeaderContent(tag="flag", flag=(number, shape, masking)):
            _dxf_bubble_leader(doc, msp, targets, landing, path, number, shape, style, Nothing, masking, ramp)
        case _ as unreachable:
            assert_never(unreachable)


def _dxf_bubble_leader(
    doc: "ezdxf.document.Drawing",
    msp: object,
    targets: tuple[Point, ...],
    landing: Point,
    path: LeaderPath,
    label: str,
    shape: BubbleShape,
    style: SymbolStyle,
    sheet_ref: Option[str],
    masking: Masking,
    ramp: Ramp,
) -> None:
    block = _bubble_block(doc, shape)
    attribs = GfxAttribs(layer=style.layer.compose()).asdict()  # one composed layer serves the builder and its wipeout twin
    builder = msp.add_multileader_block("Standard", dxfattribs=attribs)  # Exemption: the block mleader builder is the stateful sink; leader lines add in place
    builder.set_content(block, scale=style.text_height.mm)  # the ONE scale application over the unit-coordinate block
    builder.set_attribute("LABEL", label)
    match sheet_ref:
        case Option(tag="some", some=reference):
            builder.set_attribute("REF", reference)
        case Option(tag="none"):
            pass
        case unreachable:
            assert_never(unreachable)
    builder.set_leader_properties(color=_dxf_rgb(ramp[style.stroke % len(ramp)]), lineweight=_lw(style.weight), leader_type=_leader_type(path))
    builder.set_arrow_properties(name=_DXF_ARROW[style.terminator], size=style.text_height.mm)
    for target in targets:
        builder.add_leader_line(_side(target, landing), [Vec2(target), Vec2(landing)])
    builder.build(insert=Vec2(landing))
    match masking:
        case Masking.PAPER:
            msp.add_wipeout(_polygon_at(shape, landing, _BUBBLE * style.text_height.mm), dxfattribs=attribs)
        case Masking.NONE:
            pass
        case unreachable:
            assert_never(unreachable)


def _dxf_note(msp: object, insert: Point, body: NoteBody, style: SymbolStyle) -> None:
    attribs = GfxAttribs(layer=style.layer.compose(), lineweight=_lw(style.weight)).asdict()
    match body:  # Exemption: ezdxf MText is the stateful entity; the location/width set in place
        case NoteBody(tag="prose", prose=(run, broken)):
            content = str(MTextEditor().font(style.face or "isocp").height(style.text_height.mm).append(_mtext_safe(run.source)))
            note = msp.add_mtext(content, dxfattribs=attribs)
            note.dxf.width = max((line.advance for line in broken.lines), default=style.text_height.mm)
            note.set_location(Vec2(insert), attachment_point=MTextEntityAlignment.TOP_LEFT)
        case NoteBody(tag="math", math=source):
            msp.add_mtext(source, dxfattribs=attribs).set_location(Vec2(insert), attachment_point=MTextEntityAlignment.TOP_LEFT)
        case _ as unreachable:
            assert_never(unreachable)


def _dxf_revcloud(
    doc: "ezdxf.document.Drawing", msp: object, region: tuple[Point, ...], radius: float, mark: Option[str], style: SymbolStyle
) -> None:
    # `xyseb` carries each semicircle bulge, and `Some` places the delta-tag block at SVG parity.
    points = tuple(
        (a[0] + (b[0] - a[0]) * i / bumps, a[1] + (b[1] - a[1]) * i / bumps, 0.0, 0.0, 1.0)
        for a, b in zip(region, (*region[1:], region[0]), strict=True)
        for bumps in (max(1, round(math.dist(a, b) / (2.0 * radius))),)
        for i in range(1, bumps + 1)
    )
    attribs = GfxAttribs(layer=style.layer.compose(), lineweight=_lw(style.weight)).asdict()
    msp.add_lwpolyline(points, format="xyseb", close=True, dxfattribs=attribs)
    match mark:
        case Option(tag="some", some=number):
            # SVG/DXF parity: the SVG lowering backs the delta tag with PAPER masking, so the wipeout lands first and the
            # unit-coordinate block scales once through the blockref xscale/yscale pair.
            scale = style.text_height.mm
            msp.add_wipeout(_polygon_at(BubbleShape.TRIANGLE, region[0], _BUBBLE * scale), dxfattribs=attribs)
            msp.add_auto_blockref(
                _bubble_block(doc, BubbleShape.TRIANGLE), region[0], {"LABEL": number}, dxfattribs=attribs | {"xscale": scale, "yscale": scale}
            )
        case Option(tag="none"):
            pass
        case unreachable:
            assert_never(unreachable)


def _dxf_mark(doc: "ezdxf.document.Drawing", msp: object, mark: AnnotateOp, landing: Option[Point], ramp: Ramp) -> None:
    match mark:
        case AnnotateOp(tag="leader", leader=(targets, home, path, content, style)):
            _dxf_leader(doc, msp, targets, landing.default_value(home), path, content, style, ramp)
        case AnnotateOp(tag="textnote", textnote=(insert, body, style)):
            _dxf_note(msp, insert, body, style)
        case AnnotateOp(tag="revcloud", revcloud=(region, radius, mark_no, style)):
            _dxf_revcloud(doc, msp, region, radius, mark_no, style)
        case _ as unreachable:
            assert_never(unreachable)


def _bubble_block(doc: "ezdxf.document.Drawing", shape: BubbleShape) -> str:
    # one reusable keynote/flag block placed by N `add_blockref`, never a per-placement geometry copy; geometry is
    # UNIT-coordinate (radius `_BUBBLE`) so one shared per-shape block serves every style and the placement scale
    # (`set_content(scale=)` / blockref `xscale`) applies the text height exactly once.
    name = f"ANNOT_{shape.value}"
    if name not in doc.blocks:
        block = doc.blocks.new(name)  # Exemption: ezdxf BlockLayout is the GraphicsFactory sink; the outline + attdefs add in place
        block.add_lwpolyline(_polygon_at(shape, (0.0, 0.0), _BUBBLE), close=True)
        block.add_attdef("LABEL", (0.0, 0.0))
        block.add_attdef("REF", (0.0, -_BUBBLE * 0.5))  # the keynote sheet/legend caption slot
    return name


def _mtext_safe(text: str, /) -> str:
    # writer-side MTEXT literalizer — backslash first, then the group braces — because ezdxf ships only the read-side
    # `plain_mtext` strip; prose entering inline-code position carries no live control sequences. The math body stays
    # raw by design: its source IS the markup.
    return text.replace("\\", "\\\\").replace("{", "\\{").replace("}", "\\}")


def _side(target: Point, landing: Point, /) -> "ConnectionSide":
    return ConnectionSide.right if target[0] > landing[0] else ConnectionSide.left


def _leader_type(path: LeaderPath, /) -> "LeaderType":
    # LeaderPath -> the multileader's native leader-line kind, the DXF half of the SVG `_elbow` split — resolved
    # inside the worker so the lazy ezdxf proxy never reifies at import.
    match path:
        case LeaderPath.SPLINE:
            return LeaderType.splines
        case LeaderPath.STRAIGHT:
            return LeaderType.straight_lines
        case _ as unreachable:
            assert_never(unreachable)


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
# ISO 129-1 Terminator -> ezdxf.ARROWS block-name constants (closed_filled is the empty-string default).
_DXF_ARROW: frozendict[Terminator, str] = frozendict({
    Terminator.FILLED_ARROW: "",
    Terminator.OPEN_ARROW: "OPEN",
    Terminator.OBLIQUE_STROKE: "OBLIQUE",
    Terminator.DOT: "DOT",
    Terminator.ORIGIN_INDICATION: "ORIGIN",
    Terminator.NONE: "NONE",
})


def _dxf_rgb(ink: str, /) -> tuple[int, int, int]:
    # a hex_ramp entry onto the ezdxf RGB triple `set_leader_properties(color=)` admits.
    value = int(ink.removeprefix("#"), 16)
    return ((value >> 16) & 0xFF, (value >> 8) & 0xFF, value & 0xFF)


def _svg_engine(annotate: Annotate) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    ziafont.config.precision = _PRECISION  # once inside the offload lane — same d-floats -> same content key; math precision is Formula's own config
    ramp = hex_ramp(annotate.palette)
    routed = _route(Block.of_seq(annotate.marks))
    box = _bbox(annotate.marks, routed)

    def bucket(acc: Map[str, tuple[LayerName, Block["drawsvg.Group"]]], indexed: tuple[int, AnnotateOp], /) -> Map[str, tuple[LayerName, Block["drawsvg.Group"]]]:
        # Map is an ordered tree, so the composed name (str, ordered) keys the bucket and the unordered LayerName
        # rides the value; key-sorted Map iteration IS the deterministic layer order.
        layer = _style(indexed[1]).layer
        mark = Block.singleton(_svg_mark(indexed[1], ramp, routed.try_find(indexed[0])))
        return acc.change(layer.compose(), lambda held: Some((layer, held.map(lambda pair: pair[1]).default_value(Block.empty()).append(mark))))

    grouped = Block.of_seq(enumerate(annotate.marks)).fold(bucket, Map.empty())
    composed = tuple((layer, _layer_svg(name, items, box)) for name, (layer, items) in grouped.items())
    layers = tuple(_row(name=layer.compose(), source=source, aec=Some(layer)) for layer, source in composed)
    return layers, ArtifactReceipt.Drawing(
        annotate._key,
        "drawing-annotate",
        len(annotate.marks),
        _LEADER_STYLE,
        int(box[2] - box[0]),
        int(box[3] - box[1]),
        sum(len(source) for _, source in composed),
    )


def _dxf_extent(msp: object, fallback: Box, /) -> tuple[int, int]:
    # `bbox.extents` includes placed blocks and masks that the point hull omits.
    box = bbox.extents(msp, fast=True)
    return (round(box.size.x), round(box.size.y)) if box.has_data else (int(fallback[2] - fallback[0]), int(fallback[3] - fallback[1]))


def _dxf_engine(annotate: Annotate) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    doc = ezdxf.new("R2018", setup=True)
    msp = doc.modelspace()
    ramp = hex_ramp(annotate.palette)  # SVG/DXF parity: both engines resolve the SAME palette
    routed = _route(Block.of_seq(annotate.marks))
    box = _bbox(annotate.marks, routed)
    for index, mark in enumerate(annotate.marks):  # Exemption: ezdxf Modelspace is the GraphicsFactory sink; add_* mutate the layout in place
        _dxf_mark(doc, msp, mark, routed.try_find(index), ramp)
    width, height = _dxf_extent(msp, box)
    stream = io.StringIO()
    doc.write(stream)
    data = stream.getvalue().encode()
    return (_row(name="dxf", source=data, aec=Nothing),), ArtifactReceipt.Drawing(
        annotate._key, "drawing-annotate", len(annotate.marks), _LEADER_STYLE, width, height, len(data)
    )


_ENGINES: frozendict[SymbolTarget, Engine] = frozendict({SymbolTarget.SVG: _svg_engine, SymbolTarget.DXF: _dxf_engine})


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["Annotate", "AnnotateOp", "BubbleShape", "LeaderContent", "LeaderPath", "Masking", "NoteBody"]
```
