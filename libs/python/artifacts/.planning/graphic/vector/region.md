# [PY_ARTIFACTS_GRAPHIC_VECTOR_REGION]

The boolean/offset/serialization owner of the vector plane — the s1 surface that turns parsed geometry into set-operated, stroked, warped, wound, hit-tested, documented, and rasterized regions. `Region` is ONE modal owner over the closed `RegionOp` family, normalizing `RegionOp | Iterable[RegionOp]` at the head so a lone boolean and a mixed batch are the same entrypoint, traversing into one `RuntimeRail[Block[RegionResult]]` whose every outcome is the typed `RegionResult` (`document`/`facts`/`hits`/`raster`), never an erased `bytes` a consumer re-parses. Beside the rail it exposes the composable surface — `boolean`/`outline`/`warp`/`wind`/`contains`/`facts`/`clip`/`text_path`/`transform`/`fit`/`document`/`rasterize` plus the `RenderPolicy` raster-policy owner and the `PaintSpec` paint rows — that `graphic/vector/pattern#PATTERN`, `drawing/annotate#ANNOTATE`, and `drawing/symbol#SYMBOL` import one hop. Every fallible arm rails its provider raise into the closed `RegionFault` `@tagged_union`; the interior is total over `Result[RegionResult, RegionFault]`, never a railless body trusting a boundary capsule to swallow a `pathops.PathOpsError` or a `resvg` `ValueError`.

`skia-pathops` (the abi3 binding of Skia's `SkPathOps`/`SkStroke`) owns the boolean/offset/outline algebra `svgelements` cannot express — N-ary planar set operations (`op`/`OpBuilder`), self-intersection removal and winding repair (`simplify`), stroke-to-outline offset (`Path.stroke` with cap/join/miter/dash), the projective 3x3 `Path.transform` warp, the settable `Path.clockwise` normalization, the `area`/`bounds`/`controlPointBounds`/`isConvex` query family, and the fill-rule `Path.contains` hit test — over ONE mutable accumulator the `graphic/vector/path#PATH` segment stream draws into through the FontTools pen protocol and draws back out of into a `fonttools` `SVGPathPen`, so geometry never re-parses a `d` string between ops. `drawsvg` owns EVERY document emission: `Drawing`/`Group`/`Path(d=)`/`Raw` assemble fragments onto a viewBox-framed canvas, the `LinearGradient`/`RadialGradient` def-tier owners carry the `PaintSpec` gradient rows, and `as_svg().encode()` is the one egress — a hand-emitted f-string tag is the rejected form. `resvg_py.svg_to_bytes` rasterizes a placed document to PNG bytes in-process under the one `RenderPolicy`. TEXT-ON-PATH lands here as the `on_path` baseline threading: `typography/shape#SHAPE`'s `PositionedGlyphRun.on_path()` per-glyph outlines lay along a baseline at METRIC arc-length positions composing path's `point_at`, never a local re-shape (the shape-once law). Because the pathops algebra, the drawsvg assembly, and the resvg render are synchronous native/CPU work, the modal rail crosses the whole batch through the runtime lane's `offload(modality=Modality.PROCESS)` — zero folder-minted limiters or retry.

## [01]-[INDEX]

- [01]-[REGION]: the boolean/offset/outline/serialization owner over the closed `RegionOp` family — the `pathops` boolean/stroke/warp/wind/facts/contains spine, the metric TEXT-ON-PATH threading, the drawsvg `document` assembly with `PaintSpec` paint, and the `resvg_py` raster floor under one `RenderPolicy` — the composable surface pattern/annotate/symbol import one hop, on the `Region.over`/`of` modal rail over `Block[RegionResult]`.

## [02]-[REGION]

- Owner: `Region` the one boolean/offset/serialization owner holding `ops: tuple[RegionOp, ...]` and discriminating over the closed `RegionOp` `expression.tagged_union`; projecting one closed `RegionResult` family; and railing every provider raise into `RegionFault`. The `pathops.Path` accumulator is the boolean working surface, `drawsvg.Drawing` the one document canvas, `resvg_py.svg_to_bytes` the in-process raster floor. Parse, drawable narrowing, combined outlines, affine matrices, and metric positions arrive from `graphic/vector/path#PATH` — this owner never re-parses, never re-derives an affine, and never re-shapes text.
- Cases: `RegionOp` cases split by outcome — `Boolean` (N-ary `OpBuilder` fold keyed by `BooleanOp`, `simplify` winding, `fillType` from `WindingRule` — the set-op algebra svgelements has no member for), `Outline` (`Path.stroke` centerline into a closed filled offset keyed by `CapStyle`/`JoinStyle` — the thick-connector/offset-boundary substrate and the scallop band `drawing/annotate#ANNOTATE` composes), `Warp` (the full 3x3 `Path.transform` affine/PERSPECTIVE the 6-tuple `Matrix` cannot express), `Wind` (`simplify` then set `clockwise` so a disagreeing contour reverses), `Contains` (`simplify` then per-point `Path.contains` under the resolved `FillType`), `Facts` (`abs(area)`/`bounds`/contour-count/`isConvex`/`controlPointBounds`), `Clip` (per-shape geometric crop — a straddling shape is really severed, separate shapes stay separate fragments), `TextPath` (thread each glyph outline along the baseline at its METRIC mid-advance arc-length position via path's `point_at`, place by the tangent-following `Matrix`, then `OpBuilder`-union the placed glyphs), `Transform`/`Fit` (re-emit through path's `fragment`/`fit_matrix`), `Serialize` (assemble pre-built `d` fragments under the optional `PaintSpec`), `Rasterize` (`resvg_py.svg_to_bytes` under the one `RenderPolicy`) — matched by one total `match`. `RegionResult` cases are `document`/`facts`/`hits`/`raster`.
- Entry: `Region.over` normalizes `RegionOp | Iterable[RegionOp]` into the `ops` tuple by a structural `match` at the head — a lone boolean the one-element case, a mixed sheet the multi-element case, never a `batch: bool` and never a per-op sibling.
- Auto: `_ingest` replays the svgelements segment stream into a `pathops.Path` through the FontTools `getPen()` under one total `match`; `_admitted` pre-flattens arcs to cubics under `TOLERANCE.flatten` (the pen speaks move/line/cubic/quad/close, never an arc/conic verb); `_drawn` runs `convertConicsToQuads(TOLERANCE.conic)` then draws into a `fonttools` `SVGPathPen`; `document` builds a `drawsvg.Drawing` sized to the full `xmin ymin width height` viewBox so non-origin geometry is framed rather than clipped to a `0 0 w h` box, registers `PaintSpec` gradient defs once via `append_def`, and appends one `draw.Path(d=)` per fragment; `_resolved` is the shared thunk-trap naming `OpenPathError` BEFORE its `PathOpsError` base so the precise open-path cause is not shadowed; `text_path` reads the glyph mid-advance distances as one vector, resolves every `(position, tangent)` row through ONE `point_at` call, and unions the placed glyphs so tight-curve overlaps merge into one outline.
- Receipt: `Region` is a geometry substrate — its results are keyed by the consuming producer into its own `ContentIdentity.of`; this owner mints no content key and adds no receipt case.
- Growth: a new boolean kind is one `BooleanOp` member (its `.name` resolving the `pathops.PathOp` member by `getattr`); a new stroke cap/join one `CapStyle`/`JoinStyle` member; a winding-policy shift one `WindingRule` member; a winding target one `WindingDir` member; a new paint row one `PaintSpec` case lowered to its drawsvg def; a knockout glyph boolean composes the SAME `getPen`/`draw` spine onto the typography glyph producers with no serialization hop; a new resvg knob one `RenderPolicy` field carried into the one `svg_to_bytes` spread, never a second rasterizer; a new fault cause one `RegionFault` case; zero new surface.
- Packages: `skia-pathops` (`Path`/`getPen`/`draw`/`stroke`/`transform`/`clockwise`/`simplify`/`convertConicsToQuads`/`area`/`bounds`/`controlPointBounds`/`isConvex`/`contains`, `op`/`OpBuilder.add`+`resolve`, `PathOp`/`FillType`/`LineCap`/`LineJoin`, `OpenPathError`/`PathOpsError` leaves; `reverse_difference` is NOT re-exported at top level, so the `op(one, two, PathOp.REVERSE_DIFFERENCE)` binary form and the `OpBuilder` N-way form are the top-level spellings); `drawsvg` (`Drawing`/`append`/`append_def`/`as_svg`, `Path(d=)`/`Group`/`Raw`, `LinearGradient`/`RadialGradient` + `add_stop`; save_png/video extras absent on core, raster stays resvg); `resvg_py` (`svg_to_bytes`, `ValueError` the one raise); `fonttools` (`SVGPathPen`); `svgelements` (segment types the pen replay matches); `expression`/`msgspec`/`beartype`; runtime `lanes`/`faults`; `typography/shape#SHAPE` (`PositionedGlyphRun.on_path()`/`to_svg_path` — glyph outlines arrive shaped, never re-shaped here).
- Boundary: the fault rail carries a composed `graphic/vector/path#PATH` `PathFault` whole (`geometry`) rather than re-classifying it, mints `render`/`empty`/`contract`/`open_path`/`degenerate` for its own raises, and never trusts a boundary capsule to swallow an unclassified provider raise. No parse/measure/sample/affine re-derivation (path's, one hop); no repeating fill geometry (`graphic/vector/pattern#PATTERN` composes THIS page's clip); no chart-origin SVG rasterization (`vl-convert`'s bundled resvg owns it); no text shaping (`typography/shape#SHAPE` shapes; this page places outlines); no receipt or identity minting; no folder-minted limiter — the native seam is the runtime lane's `offload`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Mapping
from enum import StrEnum
from functools import wraps
from typing import TYPE_CHECKING, Literal, Self, assert_never

from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block
from expression.extra.result import traverse
from msgspec import Struct
from msgspec.structs import asdict

from rasm.artifacts.graphic.vector.path import TOLERANCE, Bounds, PathFault, Point2, Tolerance, combined, fit_matrix, fragment, point_at, scene
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy, Modality

lazy import drawsvg as draw
lazy import pathops
lazy import resvg_py
lazy from fontTools.pens.svgPathPen import SVGPathPen
lazy from svgelements import Close, CubicBezier, Matrix, Move, QuadraticBezier
lazy from svgelements import Path as SvgPath

if TYPE_CHECKING:
    import pathops
    from svgelements import Matrix, Shape
    from svgelements import Path as SvgPath

    from rasm.artifacts.typography.shape import PositionedGlyphRun

# --- [TYPES] ----------------------------------------------------------------------------
type RegionFacts = tuple[float, Bounds, int, bool, Bounds]  # (absolute area, tight bounds, contour count, convex, control-hull bounds)
type Glyphs = tuple[tuple[str, float], ...]  # per-glyph (origin-drawn SVG d-string, x-advance) — PositionedGlyphRun.on_path()
type Stroked = tuple[str, str, float]  # a per-fragment styled path: (d-string, stroke color, stroke width) — document strokes it, fill="none"
type Fragment = str | Stroked  # a document fragment: a bare d-string under the uniform paint, or a Stroked path rendering its own stroke/width
type Stops = tuple[tuple[float, str], ...]  # gradient (offset 0..1, resolved color value) rows — colors arrive resolved, never literal here
type RenderKwargs = dict[str, str | int | float | bool | list[str] | None]
type ShapeRendering = Literal["optimize_speed", "crisp_edges", "geometric_precision"]
type TextRendering = Literal["optimize_speed", "optimize_legibility", "geometric_precision"]
type ImageRendering = Literal["optimize_quality", "optimize_speed"]
type RegionOpTag = Literal[
    "boolean", "outline", "warp", "wind", "contains", "facts", "clip", "text_path", "transform", "fit", "serialize", "rasterize"
]
type RegionResultTag = Literal["document", "facts", "hits", "raster"]
type RegionFaultTag = Literal["geometry", "render", "empty", "contract", "open_path", "degenerate"]
type PaintTag = Literal["flat", "linear", "radial"]


# the pathops selectors — each member NAME mirrors the pathops.PathOp/LineCap/LineJoin/FillType member
# resolved through getattr, one derivation, never a parallel map.
class BooleanOp(StrEnum):
    UNION = "union"
    DIFFERENCE = "difference"
    INTERSECTION = "intersection"
    XOR = "xor"
    REVERSE_DIFFERENCE = "reverse-difference"


class CapStyle(StrEnum):
    BUTT_CAP = "butt-cap"
    ROUND_CAP = "round-cap"
    SQUARE_CAP = "square-cap"


class JoinStyle(StrEnum):
    MITER_JOIN = "miter-join"
    ROUND_JOIN = "round-join"
    BEVEL_JOIN = "bevel-join"


class WindingRule(StrEnum):
    WINDING = "winding"
    EVEN_ODD = "even-odd"
    INVERSE_WINDING = "inverse-winding"
    INVERSE_EVEN_ODD = "inverse-even-odd"


class WindingDir(StrEnum):  # target contour winding — maps onto the settable pathops.Path.clockwise
    CW = "cw"
    CCW = "ccw"


# --- [MODELS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class PaintSpec:
    # the fill paint a serialized document carries: flat color, or a drawsvg def-tier gradient; color
    # VALUES arrive resolved from graphic/color/derive, never literal.
    tag: PaintTag = tag()
    flat: str = case()
    linear: tuple[Stops, tuple[float, float, float, float]] = case()  # stops + (x1, y1, x2, y2) userSpaceOnUse
    radial: tuple[Stops, tuple[float, float, float]] = case()  # stops + (cx, cy, r)


class RenderPolicy(Struct, frozen=True):
    width: int | None = None
    height: int | None = None
    zoom: float | None = None
    dpi: float = 0.0
    background: str | None = None
    style_sheet: str | None = None
    resources_dir: str | None = None
    languages: tuple[str, ...] = ()
    skip_system_fonts: bool = False
    font_size: float = 16.0
    font_files: tuple[str, ...] = ()
    font_dirs: tuple[str, ...] = ()
    font_family: str | None = None
    serif_family: str | None = None
    sans_serif_family: str | None = None
    cursive_family: str | None = None
    fantasy_family: str | None = None
    monospace_family: str | None = None
    shape_rendering: ShapeRendering = "geometric_precision"
    text_rendering: TextRendering = "optimize_legibility"
    image_rendering: ImageRendering = "optimize_quality"
    log_information: bool = False

    def kwargs(self, source: Mapping[str, str]) -> RenderKwargs:
        # parameterized over the source-keyword mapping ({svg_string} or {svg_path}) the consumer projects;
        # each ()-default tuple coerces to list(value) or None per the engine's shape.
        rows = {key: (list(value) or None) if isinstance(value, tuple) else value for key, value in asdict(self).items()}
        return {**source, **rows}


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class RegionFault:
    tag: RegionFaultTag = tag()
    geometry: PathFault = case()  # the composed path substrate's fault carried whole, never re-classified
    render: str = case()
    empty: None = case()
    contract: str = case()
    open_path: None = case()  # a pathops boolean/stroke met an unclosed contour (OpenPathError)
    degenerate: str = case()  # a pathops PathOpsError leaf (NumberOfPointsError/UnsupportedVerbError/root)


# --- [OPERATIONS] -----------------------------------------------------------------------
# ONE geometry spine: path segments -> pathops.Path -> boolean/simplify/stroke -> drawsvg document,
# never a re-parsed d between ops, never an f-string tag at egress.
def _ingest(outline: "SvgPath", target: "pathops.Path", /) -> None:
    pen = target.getPen()  # the FontTools PathPen the svgelements segment stream draws into
    for segment in outline.segments():
        match segment:
            case Move():
                pen.moveTo((float(segment.end.x), float(segment.end.y)))
            case Close():
                pen.closePath()
            case CubicBezier():
                pen.curveTo(
                    (float(segment.control1.x), float(segment.control1.y)),
                    (float(segment.control2.x), float(segment.control2.y)),
                    (float(segment.end.x), float(segment.end.y)),
                )
            case QuadraticBezier():
                pen.qCurveTo((float(segment.control.x), float(segment.control.y)), (float(segment.end.x), float(segment.end.y)))
            case _:  # Line and the arcs pre-flattened to cubics upstream
                pen.lineTo((float(segment.end.x), float(segment.end.y)))


def _admitted(outline: "SvgPath", tolerance: Tolerance = TOLERANCE, /) -> "pathops.Path":
    outline.approximate_arcs_with_cubics(tolerance.flatten)  # the pen speaks move/line/cubic/quad/close, never an arc/conic verb
    target = pathops.Path()
    _ingest(outline, target)
    return target


def _to_pathops(source: bytes, /) -> Result["pathops.Path", RegionFault]:
    return combined(source).map_error(lambda fault: RegionFault(geometry=fault)).map(_admitted)


def _drawn(result: "pathops.Path", tolerance: Tolerance = TOLERANCE, /) -> str:
    result.convertConicsToQuads(tolerance.conic)  # SVG has no conic verb; round caps/joins emit conics
    pen = SVGPathPen(None)
    result.draw(pen)
    return pen.getCommands()


def _resolved[T](work: Callable[[], T], /) -> Result[T, RegionFault]:
    try:
        return Ok(work())
    except pathops.OpenPathError:
        return Error(RegionFault(open_path=None))  # named BEFORE the PathOpsError base so the precise cause is not shadowed
    except pathops.PathOpsError as fault:
        return Error(RegionFault(degenerate=type(fault).__name__))


def _paint_defs(canvas: "draw.Drawing", paint: PaintSpec | None, /) -> str | None:
    # register the PaintSpec def ONCE and return the fill; flat returns the color, gradients the
    # registered def the paths reference — reusable paint, never inline duplication.
    match paint:
        case None:
            return None
        case PaintSpec(tag="flat", flat=color):
            return color
        case PaintSpec(tag="linear", linear=(stops, (x1, y1, x2, y2))):
            grad = draw.LinearGradient(x1, y1, x2, y2, gradientUnits="userSpaceOnUse")
            for offset, color in stops:
                grad.add_stop(offset, color)
            canvas.append_def(grad)
            return grad
        case PaintSpec(tag="radial", radial=(stops, (cx, cy, r))):
            grad = draw.RadialGradient(cx, cy, r, gradientUnits="userSpaceOnUse")
            for offset, color in stops:
                grad.add_stop(offset, color)
            canvas.append_def(grad)
            return grad
        case _ as unreachable:
            assert_never(unreachable)


def document(fragments: Iterable[Fragment], viewbox: Bounds, paint: PaintSpec | None = None) -> bytes:
    # the ONE document assembly: a drawsvg canvas framed to the full extent (non-origin geometry framed,
    # never clipped to 0 0 w h), one draw.Path per fragment; gradient defs registered once.
    xmin, ymin, xmax, ymax = viewbox
    canvas = draw.Drawing(xmax - xmin, ymax - ymin, origin=(xmin, ymin))
    fill = _paint_defs(canvas, paint)
    for frag in fragments:
        match frag:
            case str() as d:
                canvas.append(draw.Path(d=d) if fill is None else draw.Path(d=d, fill=fill))
            case (d, stroke, width):
                canvas.append(draw.Path(d=d, stroke=stroke, stroke_width=width, fill="none"))
    return canvas.as_svg().encode()


def boolean(sources: tuple[bytes, ...], op: BooleanOp = BooleanOp.UNION, fill: WindingRule = WindingRule.WINDING) -> Result[bytes, RegionFault]:
    def _fold(paths: Block["pathops.Path"], /) -> Result[bytes, RegionFault]:
        def _run() -> "pathops.Path":
            builder, member = pathops.OpBuilder(fix_winding=True, keep_starting_points=True), getattr(pathops.PathOp, op.name)
            for operand in paths:  # the first add seeds the base, each later add applies `op` against the accumulator
                builder.add(operand, member)
            result = builder.resolve()
            result.simplify(fix_winding=True)
            result.fillType = getattr(pathops.FillType, fill.name)
            return result

        return _resolved(_run).bind(_framed)

    return traverse(_to_pathops, Block.of_seq(sources)).bind(_fold)


def _framed(result: "pathops.Path", /) -> Result[bytes, RegionFault]:
    if not len(result):  # len is the contour count; an empty boolean/stroke rails empty rather than an empty bounds read
        return Error(RegionFault(empty=None))
    box = result.bounds
    return Ok(document((_drawn(result),), (float(box[0]), float(box[1]), float(box[2]), float(box[3]))))


def outline(
    source: bytes,
    width: float = 1.0,
    cap: CapStyle = CapStyle.BUTT_CAP,
    join: JoinStyle = JoinStyle.MITER_JOIN,
    miter: float = 4.0,
    dash: tuple[float, ...] | None = None,
) -> Result[bytes, RegionFault]:
    def _stroke(centerline: "pathops.Path", /) -> Result[bytes, RegionFault]:
        def _run() -> "pathops.Path":
            centerline.stroke(width, getattr(pathops.LineCap, cap.name), getattr(pathops.LineJoin, join.name), miter, dash)
            centerline.simplify(fix_winding=True)
            return centerline

        return _resolved(_run).bind(_framed)

    return _to_pathops(source).bind(_stroke)


def warp(source: bytes, coeffs: tuple[float, float, float, float, float, float, float, float, float]) -> Result[bytes, RegionFault]:
    # the full 3x3 affine/PERSPECTIVE placement via pathops.Path.transform in place — the keystone dewarp the 6-tuple Matrix lacks.
    def _apply(shape: "pathops.Path", /) -> Result[bytes, RegionFault]:
        def _run() -> "pathops.Path":
            shape.transform(*coeffs)
            return shape

        return _resolved(_run).bind(_framed)

    return _to_pathops(source).bind(_apply)


def wind(source: bytes, direction: WindingDir = WindingDir.CW) -> Result[bytes, RegionFault]:
    def _orient(shape: "pathops.Path", /) -> Result[bytes, RegionFault]:
        def _run() -> "pathops.Path":
            shape.simplify(fix_winding=True)
            shape.clockwise = direction is WindingDir.CW  # settable dominant-winding policy; reverses disagreeing contours
            return shape

        return _resolved(_run).bind(_framed)

    return _to_pathops(source).bind(_orient)


def contains(source: bytes, points: tuple[Point2, ...]) -> Result[tuple[bool, ...], RegionFault]:
    def _hit(shape: "pathops.Path", /) -> Result[tuple[bool, ...], RegionFault]:
        def _run() -> tuple[bool, ...]:
            shape.simplify(fix_winding=True)  # canonicalize winding first so the FillType point-in-path test is well-defined
            return tuple(bool(shape.contains((float(x), float(y)))) for x, y in points)

        return _resolved(_run)

    return _to_pathops(source).bind(_hit)


def facts(source: bytes) -> Result[RegionFacts, RegionFault]:
    def _read(shape: "pathops.Path", /) -> Result[RegionFacts, RegionFault]:
        def _run() -> RegionFacts:
            shape.simplify(fix_winding=True)
            if not len(shape):
                return (0.0, (0.0, 0.0, 0.0, 0.0), 0, True, (0.0, 0.0, 0.0, 0.0))
            box, hull = shape.bounds, shape.controlPointBounds  # tight extent + the control-hull extent a layout/collision consumer keys
            return (
                abs(float(shape.area)),
                (float(box[0]), float(box[1]), float(box[2]), float(box[3])),
                len(shape),
                bool(shape.isConvex),
                (float(hull[0]), float(hull[1]), float(hull[2]), float(hull[3])),
            )

        return _resolved(_run).bind(lambda read: Ok(read) if read[2] else Error(RegionFault(empty=None)))

    return _to_pathops(source).bind(_read)


def clip(source: bytes, rect: Bounds) -> Result[bytes, RegionFault]:
    # per-shape geometric crop: a straddling shape is really severed, not masked, and separate shapes
    # stay separate fragments, never one merged boolean.
    def _window() -> "pathops.Path":
        x0, y0, x1, y1 = rect
        window = pathops.Path()
        pen = window.getPen()
        pen.moveTo((x0, y0))
        pen.lineTo((x1, y0))
        pen.lineTo((x1, y1))
        pen.lineTo((x0, y1))
        pen.closePath()
        return window

    def _cut(shapes: list["Shape"], /) -> Result[bytes, RegionFault]:
        def _run() -> bytes:
            window = _window()
            kept = tuple(
                _drawn(clipped)
                for shape in shapes
                if len(clipped := pathops.op(_admitted(SvgPath(*SvgPath(shape).segments())), window, pathops.PathOp.INTERSECTION))
            )
            return document(kept, rect)

        return _resolved(_run)

    return scene(source).map_error(lambda fault: RegionFault(geometry=fault)).bind(_cut)


def text_path(glyphs: "PositionedGlyphRun | Glyphs", baseline: bytes, offset: float = 0.0) -> Result[bytes, RegionFault]:
    # METRIC text-on-path: shape's glyph outlines lay at mid-advance arc-length distances via ONE
    # point_at call; the tangent-following Matrix rotates each glyph onto the baseline.
    rows: Glyphs = glyphs.on_path() if hasattr(glyphs, "on_path") else tuple(glyphs)
    if not rows:
        return Error(RegionFault(empty=None))
    cursors = tuple(offset + sum(advance for _, advance in rows[:index]) + rows[index][1] * 0.5 for index in range(len(rows)))

    def _thread(oriented: tuple[tuple[Point2, Point2], ...], /) -> Result[bytes, RegionFault]:
        def _run() -> "pathops.Path":
            builder = pathops.OpBuilder(fix_winding=True, keep_starting_points=True)
            for (d, _), ((px_, py_), (tx, ty)) in zip(rows, oriented, strict=True):
                if d:  # each placed glyph is one UNION operand so tight-curve overlaps merge into one outline
                    builder.add(_admitted(SvgPath(d) * Matrix(tx, ty, -ty, tx, px_, py_)), pathops.PathOp.UNION)
            result = builder.resolve()
            result.simplify(fix_winding=True)
            return result

        return _resolved(_run).bind(_framed)

    return point_at(baseline, cursors).map_error(lambda fault: RegionFault(geometry=fault)).bind(_thread)


def transform(source: bytes, matrix: "Matrix | None" = None) -> Result[bytes, RegionFault]:
    def _emit(shapes: list["Shape"], /) -> Result[bytes, RegionFault]:
        boxes = [box for shape in shapes if (box := shape.bbox()) is not None]
        if not boxes:
            return Error(RegionFault(empty=None))
        extent = (min(b[0] for b in boxes), min(b[1] for b in boxes), max(b[2] for b in boxes), max(b[3] for b in boxes))
        return Ok(document(tuple(fragment(shape, matrix) for shape in shapes), extent))

    return scene(source).map_error(lambda fault: RegionFault(geometry=fault)).bind(_emit)


def fit(source: bytes, target: Bounds) -> Result[bytes, RegionFault]:
    def _place(matrix: "Matrix", /) -> Result[bytes, RegionFault]:
        return (
            scene(source)
            .map_error(lambda fault: RegionFault(geometry=fault))
            .map(lambda shapes: document(tuple(fragment(shape, matrix) for shape in shapes), target))
        )

    return fit_matrix(source, target).map_error(lambda fault: RegionFault(geometry=fault)).bind(_place)


def rasterize(source: bytes, render: RenderPolicy = RenderPolicy()) -> Result[bytes, RegionFault]:
    try:
        return Ok(resvg_py.svg_to_bytes(**render.kwargs({"svg_string": source.decode()})))
    except ValueError as fault:
        return Error(RegionFault(render=str(fault)))


# --- [COMPOSITION] ----------------------------------------------------------------------
@tagged_union(frozen=True)
class RegionOp:
    tag: RegionOpTag = tag()
    boolean: tuple[tuple[bytes, ...], BooleanOp, WindingRule] = case()
    outline: tuple[bytes, float, CapStyle, JoinStyle, float, tuple[float, ...] | None] = case()
    warp: tuple[bytes, tuple[float, float, float, float, float, float, float, float, float]] = case()
    wind: tuple[bytes, WindingDir] = case()
    contains: tuple[bytes, tuple[Point2, ...]] = case()
    facts: bytes = case()
    clip: tuple[bytes, Bounds] = case()
    text_path: tuple[Glyphs, bytes, float] = case()  # (per-glyph (d, advance), baseline SVG, along-path offset)
    transform: tuple[bytes, "Matrix | None"] = case()
    fit: tuple[bytes, Bounds] = case()
    serialize: tuple[tuple[str, ...], Bounds, PaintSpec | None] = case()
    rasterize: tuple[bytes, RenderPolicy] = case()

    @staticmethod
    def Boolean(sources: Iterable[bytes], op: BooleanOp = BooleanOp.UNION, fill: WindingRule = WindingRule.WINDING) -> "RegionOp":
        return RegionOp(boolean=(tuple(sources), op, fill))

    @staticmethod
    def Outline(
        source: bytes,
        width: float = 1.0,
        cap: CapStyle = CapStyle.BUTT_CAP,
        join: JoinStyle = JoinStyle.MITER_JOIN,
        miter: float = 4.0,
        dash: tuple[float, ...] | None = None,
    ) -> "RegionOp":
        return RegionOp(outline=(source, width, cap, join, miter, dash))

    @staticmethod
    def Warp(source: bytes, coeffs: tuple[float, float, float, float, float, float, float, float, float]) -> "RegionOp":
        return RegionOp(warp=(source, coeffs))

    @staticmethod
    def Wind(source: bytes, direction: WindingDir = WindingDir.CW) -> "RegionOp":
        return RegionOp(wind=(source, direction))

    @staticmethod
    def Contains(source: bytes, points: Iterable[Point2]) -> "RegionOp":
        return RegionOp(contains=(source, tuple(points)))

    @staticmethod
    def Facts(source: bytes) -> "RegionOp":
        return RegionOp(facts=source)

    @staticmethod
    def Clip(source: bytes, rect: Bounds) -> "RegionOp":
        return RegionOp(clip=(source, rect))

    @staticmethod
    def TextPath(glyphs: "PositionedGlyphRun | Iterable[tuple[str, float]]", baseline: bytes, offset: float = 0.0) -> "RegionOp":
        rows = glyphs.on_path() if hasattr(glyphs, "on_path") else tuple(glyphs)
        return RegionOp(text_path=(rows, baseline, offset))

    @staticmethod
    def Transform(source: bytes, matrix: "Matrix | None" = None) -> "RegionOp":
        return RegionOp(transform=(source, matrix))

    @staticmethod
    def Fit(source: bytes, target: Bounds) -> "RegionOp":
        return RegionOp(fit=(source, target))

    @staticmethod
    def Serialize(fragments: tuple[str, ...], viewbox: Bounds, paint: PaintSpec | None = None) -> "RegionOp":
        return RegionOp(serialize=(fragments, viewbox, paint))

    @staticmethod
    def Rasterize(source: bytes, render: RenderPolicy = RenderPolicy()) -> "RegionOp":
        return RegionOp(rasterize=(source, render))


@tagged_union(frozen=True)
class RegionResult:
    tag: RegionResultTag = tag()
    document: bytes = case()
    facts: RegionFacts = case()
    hits: tuple[bool, ...] = case()
    raster: bytes = case()


_CONTRACT = BeartypeConf(is_pep484_tower=True)


def _contracted(operation: Callable[[RegionOp], Result[RegionResult, RegionFault]], /) -> Callable[[RegionOp], Result[RegionResult, RegionFault]]:
    guarded = beartype(conf=_CONTRACT)(operation)

    @wraps(operation)
    def call(op: RegionOp, /) -> Result[RegionResult, RegionFault]:
        try:
            return guarded(op)
        except BeartypeCallHintViolation as violation:
            return Error(RegionFault(contract=type(violation).__name__))

    return call


@_contracted
def _dispatch(op: RegionOp, /) -> Result[RegionResult, RegionFault]:
    match op:
        case RegionOp(tag="boolean", boolean=(sources, kind, fill)):
            return boolean(sources, kind, fill).map(lambda emitted: RegionResult(document=emitted))
        case RegionOp(tag="outline", outline=(source, width, cap, join, miter, dash)):
            return outline(source, width, cap, join, miter, dash).map(lambda emitted: RegionResult(document=emitted))
        case RegionOp(tag="warp", warp=(source, coeffs)):
            return warp(source, coeffs).map(lambda emitted: RegionResult(document=emitted))
        case RegionOp(tag="wind", wind=(source, direction)):
            return wind(source, direction).map(lambda emitted: RegionResult(document=emitted))
        case RegionOp(tag="contains", contains=(source, points)):
            return contains(source, points).map(lambda flags: RegionResult(hits=flags))
        case RegionOp(tag="facts", facts=source):
            return facts(source).map(lambda read: RegionResult(facts=read))
        case RegionOp(tag="clip", clip=(source, rect)):
            return clip(source, rect).map(lambda emitted: RegionResult(document=emitted))
        case RegionOp(tag="text_path", text_path=(glyphs, baseline, offset)):
            return text_path(glyphs, baseline, offset).map(lambda emitted: RegionResult(document=emitted))
        case RegionOp(tag="transform", transform=(source, matrix)):
            return transform(source, matrix).map(lambda emitted: RegionResult(document=emitted))
        case RegionOp(tag="fit", fit=(source, target)):
            return fit(source, target).map(lambda emitted: RegionResult(document=emitted))
        case RegionOp(tag="serialize", serialize=(fragments, viewbox, paint)):
            return Ok(RegionResult(document=document(fragments, viewbox, paint)))
        case RegionOp(tag="rasterize", rasterize=(source, render)):
            return rasterize(source, render).map(lambda png: RegionResult(raster=png))
        case _ as unreachable:
            assert_never(unreachable)


def _worked(ops: tuple[RegionOp, ...], /) -> Result[Block[RegionResult], RegionFault]:
    return traverse(_dispatch, Block.of_seq(ops))


class Region(Struct, frozen=True):
    ops: tuple[RegionOp, ...]

    @classmethod
    def over(cls, ops: RegionOp | Iterable[RegionOp], /) -> Self:
        match ops:
            case RegionOp():
                return cls(ops=(ops,))
            case _:
                return cls(ops=tuple(ops))

    async def of(self, lane: LanePolicy, /) -> RuntimeRail[Block[RegionResult]]:
        # pathops/drawsvg/resvg are synchronous native/CPU work: the whole batch crosses the runtime
        # lane's PROCESS offload — zero folder-minted limiters.
        return await lane.offload(_worked, self.ops, modality=Modality.PROCESS)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "BooleanOp",
    "CapStyle",
    "Fragment",
    "Glyphs",
    "JoinStyle",
    "PaintSpec",
    "Region",
    "RegionFacts",
    "RegionFault",
    "RegionOp",
    "RegionResult",
    "RenderPolicy",
    "Stops",
    "Stroked",
    "WindingDir",
    "WindingRule",
    "boolean",
    "clip",
    "contains",
    "document",
    "facts",
    "fit",
    "outline",
    "rasterize",
    "text_path",
    "transform",
    "warp",
    "wind",
]
```

```mermaid
flowchart LR
    Over["Region.over (RegionOp | Iterable)"] --> Of["Region.of(lane) -> offload PROCESS"]
    Of --> Disp["_dispatch (@_contracted) match per op"]
    Disp -->|boolean| Bl["OpBuilder + simplify + fillType -> document"]
    Disp -->|outline| Ol["Path.stroke + simplify -> document"]
    Disp -->|warp| Wp["Path.transform 3x3 -> document"]
    Disp -->|wind| Wd["simplify + clockwise -> document"]
    Disp -->|contains| Ct["simplify + Path.contains -> hits"]
    Disp -->|facts| Fc["area/bounds/hull/convex -> facts"]
    Disp -->|clip| Cl["per-shape op INTERSECTION -> document"]
    Disp -->|text_path| Tp["point_at metric rows + tangent Matrix + union -> document"]
    Disp -->|transform/fit| Tf["path.fragment/fit_matrix -> document"]
    Disp -->|serialize| Sr["document(fragments, viewbox, PaintSpec)"]
    Disp -->|rasterize| Rz["resvg svg_to_bytes(RenderPolicy) -> raster"]
    Spine["path segments -> _admitted/_ingest -> pathops.Path -> _drawn(SVGPathPen)"] -.->|one geometry spine| Bl
    Spine -.-> Ol
    Spine -.-> Tp
    Doc["document: drawsvg Drawing + Path(d=) + PaintSpec defs"] -.->|one egress| Sr
    Bl --> Result["Result[RegionResult, RegionFault]"]
    Ol --> Result
    Ct --> Result
    Fc --> Result
    Tp --> Result
    Rz --> Result
    Result -->|traverse fold| Block["Block[RegionResult]"]
    Block -.->|awaited rail| Consumers["pattern / annotate scallop-band / symbol rasterize / compose one hop"]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
