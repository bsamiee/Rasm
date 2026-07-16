# [PY_ARTIFACTS_GRAPHIC_VECTOR_PATH]

The s1 parse/query/affine/measure/sample substrate of the vector plane — the one `svgelements` owner every geometry consumer composes one hop, minting no receipt and emitting no document. `Path` is ONE modal owner over the closed `PathOp` family, normalizing `PathOp | Iterable[PathOp]` at the head so a lone query and a mixed batch are the same entrypoint, traversing the ops into one `RuntimeRail[Block[PathResult]]` whose every outcome is the typed `PathResult`, never an erased `bytes` a consumer re-parses. Beside the rail it exposes the composable geometry surface — `scene`/`combined`/`bounds`/`measure`/`sample`/`point_at`/`decimate`/`centroid`/`flatten`/`subpaths`/`project`/`fit_matrix`/`compose`/`reflect`/`polar`/`px`/`in_units`/`fragment` — that `graphic/vector/region#REGION`, `graphic/vector/pattern#PATTERN`, the drawing producers, the placement plane, `export/dxf#DXF`, `visualization/diagram/solar#SOLAR`, and `graphic/marks/encode#ENCODE` import in-process. Every fallible arm rails its provider raise into the closed `PathFault` `@tagged_union`; the interior is total over `Result[PathResult, PathFault]`.

`svgelements` (pure-Python, host-free) parses an SVG document into a typed `Shape` tree, resolves bounds through `Shape.bbox(with_stroke=)`, transforms through `SvgPath(geometry) * Matrix`, fits a source extent into a viewport through the `Viewbox` preserve-aspect `Matrix`, measures and vectorized-samples over the combined outline through `SvgPath.npoint`, and flattens curves for a polyline/toolpath consumer. The expensive `SVG.parse(reify=True)` ingestion is memoized on the source `bytes` by the `@lru_cache` `_parsed` core, so a consumer that queries `bounds`, then `measure`, then `point_at` parses once and a malformed source rails once. Three owned metric kernels close what the parametric surface cannot answer over one numpy cumulative-chord sweep: `point_at` resolves points AND unit tangents at metric arc-length distances (a `t ∈ [0,1]` sample is NOT proportional to distance across mixed segment lengths, the rejected form), `decimate` reduces a sampled polyline under a max-deviation tolerance (Ramer-Douglas-Peucker), and `centroid` folds per-contour shoelace areas into the area-weighted document centroid (the vertex-mean stand-in is the rejected form). Every tolerance and density anchor lives on the one `Tolerance` policy row, never an inline float. Because the sweep is synchronous CPU work, the modal rail crosses the whole batch through the runtime lane's `offload(modality=Modality.PROCESS)`, while the composable functions stay synchronous for consumers owning their own lane crossing. This page owns ONLY the parse/query/affine substrate — boolean/offset/outline algebra, serialization, and rasterization are `graphic/vector/region#REGION`'s, and repeating fill is `graphic/vector/pattern#PATTERN`'s.

## [01]-[INDEX]

- [01]-[PATH]: the SVG parse/query/affine/measure/sample substrate over the closed `PathOp` family — the memoized `_parsed` core, the `Matrix` affine, the `Viewbox` fit, the metric arc-length kernels (`point_at`/`decimate`/`centroid` over one numpy sweep), and the `Length` unit egress — the composable surface region/pattern/drawing/placement/dxf/solar/marks import one hop, on the `Path.over`/`of` modal rail over `Block[PathResult]`.

## [02]-[PATH]

- Owner: `Path` the one parse/query/affine substrate owner holding `ops: tuple[PathOp, ...]` and discriminating over the closed `PathOp` `expression.tagged_union` whose every case carries its own typed payload, never a `StrEnum` keyed against a shared erased `dict`; projecting one closed `PathResult` family; and railing every provider raise into `PathFault`, never `None`-as-failure. This owner reads `bbox` over the `Shape`-narrowed `elements(conditional=)` sweep, folds the document shapes into one combined `SvgPath` for the measure/sample/metric/flatten/subpaths queries, and serializes geometry ONLY as `d` strings through the one `fragment` egress — document assembly is `graphic/vector/region#REGION`'s drawsvg surface, never an f-string here.
- Cases: `PathOp` cases split by return kind — `Bounds` (the union extent over `Shape.bbox(with_stroke=)` keyed by `BoundsKind`, never a `with_stroke: bool` knob), `Measure`/`Sample` (arc length and parametric `npoint` points), `PointAt` (the METRIC kernel: points + unit tangents at arc-length distances the tick-spacing law keys and text-on-path threads), `Decimate` (RDP polyline reduction), `Centroid` (area-weighted, holes subtracting by signed area), `Flatten`/`Subpaths` (`FlattenKind`-keyed `d` string, per-contour `d` strings), `Project` (`Point.matrix_transform` or `Matrix.transform_vector` keyed by `ProjectKind`, inverse through a `determinant`-guarded `Matrix(matrix)` copy because `Matrix.inverse()` mutates its receiver), `FitMatrix` (the `Viewbox` preserve-aspect fit `Matrix` `composition/compose#COMPOSE` delegates) — matched by one total `match`, never a per-source parse sibling or per-shape transform method. `PathResult` cases are structurally addressable (`extent`/`measure`/`sampled`/`oriented`/`reduced`/`anchor`/`contours`/`fragment`/`placed`), never `bytes` discriminated by length.
- Entry: `Path.over` normalizes `PathOp | Iterable[PathOp]` into the `ops` tuple by a structural `match` at the head, so a lone query is the one-element case and a mixed batch the multi-element case under the identical surface — never a `batch: bool`, never a per-op sibling.
- Auto: `_parsed` is the one `@lru_cache` ingestion core keyed on the source `bytes`, `reify=True` resolving transforms so every `bbox()`/`SvgPath` read returns absolute coordinates and the cache collapsing the repeated parse a multi-query consumer otherwise pays per op; `scene` narrows through `isinstance(element, Shape)` to exclude the non-drawable root and `Group`/`Use` containers (the root carries a `bbox` attribute, so an attribute post-filter admits it and then crashes every outline fold — the rejected form); `combined` folds every shape's segments into one outline; `_polyline` is the one metric kernel core (`npoint` over `tolerance.samples`, cumulative chord lengths via one `np.cumsum`) that `point_at`/`decimate`/`centroid` share, one vectorized sweep for three queries; `in_units` converts through the catalogued `to_mm`/`to_cm`/`to_inch` rows and strips the target unit's own emitted token, so the strip is total, never a general string parse.
- Receipt: `Path` is a geometry substrate — its rail returns one `Block[PathResult]` and its composable functions return geometry values the consuming producer keys into its own `ContentIdentity.of`; this substrate mints no content key and adds no receipt case.
- Growth: a new geometry query is one `PathOp` case plus one composable function over the existing `svgelements` surface (a curvature query rides the `_polyline` sweep plus a second difference), never a re-implemented engine; a new flatten target is one `FlattenKind` member plus one `_FLATTEN` row; a new projection mode one `ProjectKind` member plus one `_PROJECT` row; a new unit egress one `Unit` member (its suffix the strip token); a new tolerance anchor one `Tolerance` field; a new fault cause one `PathFault` case; a new outcome shape one `PathResult` case; zero new surface.
- Packages: `svgelements` (`SVG.parse(reify=True)`/`elements(conditional=)`, `SvgPath.d`/`bbox(with_stroke=)`/`length`/`npoint`/`segments`/`as_subpaths`/`approximate_arcs_with_cubics`/`approximate_arcs_with_quads`/`approximate_bezier_with_circular_arcs`, `Matrix` factories + `pre_*`/`post_*` + `determinant`/`inverse` + `transform_point`/`transform_vector`, `Viewbox(...).transform(...)`, `Length.value`/`to_mm`/`to_cm`/`to_inch`, `Point.distance_to`/`angle_to`/`polar_to`/`reflected_across`/`matrix_transform`); `numpy` (the `npoint` sweep, `cumsum`/`searchsorted`/`linalg.norm` kernels); `expression` (`tagged_union`, `Result`, `Block`, `Map.of_seq`, `traverse`); `msgspec` (`Struct`); `beartype` (the `_contracted` weave); runtime `lanes`/`faults`.
- Boundary: no boolean/offset/stroke/winding algebra and no `pathops` import (that is `graphic/vector/region#REGION`); no document assembly, `<svg>`/`<path>` emission, paint, or raster (region's drawsvg/resvg surface); no repeating fill geometry (`graphic/vector/pattern#PATTERN`); no receipt or identity minting (the consuming producer's); no folder-minted limiter or retry — the one native seam is the runtime lane's `offload`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import lru_cache, wraps
from io import BytesIO
from itertools import chain
from typing import TYPE_CHECKING, Final, Literal, Protocol, Self, assert_never
from xml.etree.ElementTree import ParseError

import numpy as np
from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from expression.extra.result import traverse
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy, Modality

lazy from svgelements import SVG, Close, Color, CubicBezier, Length, Line, Matrix, Move, Point, QuadraticBezier, Shape, Viewbox
lazy from svgelements import Path as SvgPath

if TYPE_CHECKING:
    from svgelements import SVG, Matrix, Point, Shape
    from svgelements import Path as SvgPath

# --- [TYPES] ----------------------------------------------------------------------------
type Bounds = tuple[float, float, float, float]
type Point2 = tuple[float, float]
type Oriented = tuple[Point2, Point2]  # (position, unit tangent) — one metric point-at-distance row
type Span = str | float
type PathOpTag = Literal["bounds", "measure", "sample", "point_at", "decimate", "centroid", "flatten", "subpaths", "project", "fit_matrix"]
type PathResultTag = Literal["extent", "measure", "sampled", "oriented", "reduced", "anchor", "contours", "fragment", "placed"]
type PathFaultTag = Literal["parse", "singular", "empty", "contract"]


class FlattenKind(StrEnum):
    CUBICS = "cubics"
    QUADS = "quads"
    ARCS = "arcs"


class ProjectKind(StrEnum):
    POINT = "point"
    VECTOR = "vector"


class BoundsKind(StrEnum):
    GEOMETRIC = "geometric"  # tight path extent
    INK = "ink"  # stroke-inclusive visual extent (bbox with_stroke=True)


# each member NAME mirrors the svgelements Matrix.<order>_<step> compose method, so getattr is ONE derivation, never a parallel map.
class ComposeStep(StrEnum):
    SCALE = "scale"
    TRANSLATE = "translate"
    ROTATE = "rotate"
    SKEW = "skew"


class ComposeOrder(StrEnum):
    PRE = "pre"  # left-compose (the new step applies BEFORE the accumulated transform)
    POST = "post"  # right-compose (the new step applies AFTER the accumulated transform)


class Unit(StrEnum):
    MM = "mm"
    CM = "cm"
    INCH = "in"

    @property
    def converter(self) -> str:
        return {"mm": "to_mm", "cm": "to_cm", "in": "to_inch"}[self.value]


class Element(Protocol):
    def bbox(self) -> Bounds | None: ...


# --- [MODELS] ---------------------------------------------------------------------------
class Tolerance(Struct, frozen=True):
    # the ONE tolerance/density policy row — every arc-flatten error, conic tolerance, resolution,
    # tangent step, and sweep density reads here, never an inline float.
    flatten: float = 0.1  # arc->cubic max deviation (user units)
    conic: float = 0.25  # conic->quad tolerance the region draw-back composes
    ppi: float = 96.0  # CSS px resolution anchor for Length.value
    tangent: float = 1e-3  # forward-step fraction for the chord tangent read
    samples: int = 512  # metric-kernel polyline density (the one npoint sweep)


TOLERANCE: Final[Tolerance] = Tolerance()


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class PathFault:  # the closed provider-raise vocabulary; Color(value) is lenient (a malformed color resolves), so no color fault case
    tag: PathFaultTag = tag()
    parse: str = case()  # ParseError/ValueError/TypeError from SVG.parse over malformed markup
    singular: None = case()  # a project inverse against a determinant==0 matrix, guarded before the 1/det raise
    empty: None = case()  # no drawable shape, an outline with no segment, or a zero-length metric sweep
    contract: str = case()  # a BeartypeCallHintViolation the _contracted weave lifts onto _dispatch's rail


# --- [OPERATIONS] -----------------------------------------------------------------------
@lru_cache(maxsize=128)
def _parsed(source: bytes) -> Result["SVG", PathFault]:
    try:
        return Ok(SVG.parse(BytesIO(source), reify=True))
    except (ParseError, ValueError, TypeError) as fault:
        return Error(PathFault(parse=str(fault)))


def scene(source: bytes) -> Result[list["Shape"], PathFault]:
    # the drawable sweep: isinstance(Shape) excludes the SVG root and Group/Use containers the outline fold would crash on.
    return _parsed(source).map(lambda document: list(document.elements(conditional=lambda element: isinstance(element, Shape))))


def elements(source: bytes) -> list[Element]:
    return scene(source).default_value([])


def combined(source: bytes) -> Result["SvgPath", PathFault]:
    def _fold(shapes: list["Shape"], /) -> Result["SvgPath", PathFault]:
        outline = SvgPath(*chain.from_iterable(SvgPath(shape).segments() for shape in shapes))
        return Ok(outline) if len(outline) else Error(PathFault(empty=None))

    return scene(source).bind(_fold)


def _boxes(shapes: list["Shape"], kind: BoundsKind = BoundsKind.GEOMETRIC, /) -> Result[Bounds, PathFault]:
    boxes = [box for shape in shapes if (box := shape.bbox(with_stroke=kind is BoundsKind.INK)) is not None]
    return (
        Ok((min(b[0] for b in boxes), min(b[1] for b in boxes), max(b[2] for b in boxes), max(b[3] for b in boxes)))
        if boxes
        else Error(PathFault(empty=None))
    )


def bounds(source: bytes, kind: BoundsKind = BoundsKind.GEOMETRIC) -> Result[Bounds, PathFault]:
    return scene(source).bind(lambda shapes: _boxes(shapes, kind))


def measure(source: bytes) -> Result[float, PathFault]:
    return combined(source).map(lambda outline: outline.length())


def sample(source: bytes, positions: tuple[float, ...]) -> Result[tuple[Point2, ...], PathFault]:
    def _points(xy: object, /) -> Result[tuple[Point2, ...], PathFault]:
        return Error(PathFault(empty=None)) if xy is None else Ok(tuple((float(x), float(y)) for x, y in xy))

    return combined(source).bind(lambda outline: _points(outline.npoint(np.asarray(positions, dtype=float))))


def _polyline(outline: "SvgPath", tolerance: Tolerance, /) -> Result[tuple[np.ndarray, np.ndarray], PathFault]:
    # the ONE metric kernel core: a dense npoint sweep plus cumulative chord lengths; point_at/decimate/centroid
    # interpolate over this pair, one pass, three queries.
    xy = outline.npoint(np.linspace(0.0, 1.0, tolerance.samples))
    if xy is None or len(xy) < 2:
        return Error(PathFault(empty=None))
    lengths = np.concatenate(([0.0], np.cumsum(np.linalg.norm(np.diff(xy, axis=0), axis=1))))
    return Ok((np.asarray(xy, dtype=float), lengths)) if lengths[-1] > 0.0 else Error(PathFault(empty=None))


def point_at(source: bytes, distances: tuple[float, ...], tolerance: Tolerance = TOLERANCE) -> Result[tuple[Oriented, ...], PathFault]:
    # METRIC point-at-distance: searchsorted onto the cumulative chord lengths, linear blend, unit tangent
    # off the local chord — parametric t is NOT proportional to distance (the rejected form).
    def _rows(sweep: tuple[np.ndarray, np.ndarray], /) -> tuple[Oriented, ...]:
        xy, lengths = sweep
        total = float(lengths[-1])
        where = np.clip(np.asarray(distances, dtype=float), 0.0, total)
        ahead = np.clip(where + tolerance.tangent * total, 0.0, total)

        def _blend(marks: np.ndarray, /) -> np.ndarray:
            index = np.clip(np.searchsorted(lengths, marks, side="right") - 1, 0, len(lengths) - 2)
            span = lengths[index + 1] - lengths[index]
            frac = np.where(span > 0.0, (marks - lengths[index]) / np.where(span > 0.0, span, 1.0), 0.0)
            return xy[index] + (xy[index + 1] - xy[index]) * frac[:, None]

        here, front = _blend(where), _blend(ahead)
        delta = front - here
        norms = np.linalg.norm(delta, axis=1)
        unit = np.where(norms[:, None] > 0.0, delta / np.where(norms[:, None] > 0.0, norms[:, None], 1.0), np.array([1.0, 0.0]))
        return tuple(((float(p[0]), float(p[1])), (float(t[0]), float(t[1]))) for p, t in zip(here, unit, strict=True))

    return combined(source).bind(lambda outline: _polyline(outline, tolerance)).map(_rows)


def decimate(source: bytes, epsilon: float | None = None, tolerance: Tolerance = TOLERANCE) -> Result[tuple[Point2, ...], PathFault]:
    # RDP over the metric sweep: keep the farthest-deviating vertex while it exceeds epsilon (default the
    # flatten row) — the plot-weight/toolpath reducer, iterative, never recursive.
    def _reduced(sweep: tuple[np.ndarray, np.ndarray], /) -> tuple[Point2, ...]:
        xy, _ = sweep
        limit = tolerance.flatten if epsilon is None else epsilon
        keep = np.zeros(len(xy), dtype=bool)
        keep[[0, len(xy) - 1]] = True
        stack = [(0, len(xy) - 1)]
        while stack:
            lo, hi = stack.pop()
            if hi - lo < 2:
                continue
            chord = xy[hi] - xy[lo]
            norm = float(np.linalg.norm(chord)) or 1.0
            offsets = xy[lo + 1 : hi] - xy[lo]
            deviation = np.abs(chord[0] * offsets[:, 1] - chord[1] * offsets[:, 0]) / norm  # 2D cross z-component; np.cross 2D is deprecated
            peak = int(np.argmax(deviation))
            if float(deviation[peak]) > limit:
                keep[lo + 1 + peak] = True
                stack.extend(((lo, lo + 1 + peak), (lo + 1 + peak, hi)))
        return tuple((float(x), float(y)) for x, y in xy[keep])

    return combined(source).bind(lambda outline: _polyline(outline, tolerance)).map(_reduced)


def centroid(source: bytes, tolerance: Tolerance = TOLERANCE) -> Result[Point2, PathFault]:
    # area-weighted centroid: per-contour shoelace area weights the per-contour centroid, holes
    # subtracting by signed area; a zero-area document rails empty, never a NaN.
    def _weighted(outline: "SvgPath", /) -> Result[Point2, PathFault]:
        total_area, moment = 0.0, np.zeros(2)
        for contour in outline.as_subpaths():
            match _polyline(SvgPath(*contour.segments()), tolerance):
                case Ok((xy, _)):
                    pass
                case _:  # a degenerate contour contributes no area; the document-level empty rail closes below
                    continue
            x, y = xy[:, 0], xy[:, 1]
            cross = x * np.roll(y, -1) - np.roll(x, -1) * y
            area = float(np.sum(cross)) / 2.0
            if area == 0.0:
                continue
            cx = float(np.sum((x + np.roll(x, -1)) * cross)) / (6.0 * area)
            cy = float(np.sum((y + np.roll(y, -1)) * cross)) / (6.0 * area)
            total_area += area
            moment += area * np.array([cx, cy])
        return Ok((float(moment[0] / total_area), float(moment[1] / total_area))) if total_area != 0.0 else Error(PathFault(empty=None))

    return combined(source).bind(_weighted)


_FLATTEN: Final[Map[FlattenKind, Callable[["SvgPath", float], object]]] = Map.of_seq([
    (FlattenKind.CUBICS, lambda outline, error: outline.approximate_arcs_with_cubics(error)),
    (FlattenKind.QUADS, lambda outline, error: outline.approximate_arcs_with_quads(error)),
    (FlattenKind.ARCS, lambda outline, error: outline.approximate_bezier_with_circular_arcs(error)),
])
_PROJECT: Final[Map[ProjectKind, Callable[["Matrix", "Point"], "Point"]]] = Map.of_seq([
    (ProjectKind.POINT, lambda active, point: point.matrix_transform(active)),
    (ProjectKind.VECTOR, lambda active, point: active.transform_vector(point)),
])


def flatten(source: bytes, kind: FlattenKind = FlattenKind.CUBICS, error: float | None = None, tolerance: Tolerance = TOLERANCE) -> Result[str, PathFault]:
    def _emit(outline: "SvgPath", /) -> str:
        _FLATTEN[kind](outline, tolerance.flatten if error is None else error)
        return outline.d()

    return combined(source).map(_emit)


def subpaths(source: bytes) -> Result[tuple[str, ...], PathFault]:
    return combined(source).map(lambda outline: tuple(contour.d() for contour in outline.as_subpaths()))


def project(
    points: Iterable[Point2], matrix: "Matrix", kind: ProjectKind = ProjectKind.POINT, inverse: bool = False
) -> Result[tuple[Point2, ...], PathFault]:
    if inverse and matrix.determinant == 0:
        return Error(PathFault(singular=None))
    active, apply = (Matrix(matrix).inverse() if inverse else matrix), _PROJECT[kind]  # inverse mutates: copy, never the caller's matrix
    return Ok(tuple((float(r.x), float(r.y)) for r in (apply(active, Point(*pt)) for pt in points)))


def fit_matrix(source: bytes, target: Bounds) -> Result["Matrix", PathFault]:
    def _fit(src: Bounds, /) -> "Matrix":
        content = f"{src[0]} {src[1]} {src[2] - src[0]} {src[3] - src[1]}"
        viewport = f"{target[0]} {target[1]} {target[2] - target[0]} {target[3] - target[1]}"
        return Matrix(Viewbox(content, preserve_aspect_ratio="xMidYMid meet").transform(Viewbox(viewport)))

    return bounds(source).map(_fit)


def compose(steps: tuple[tuple[ComposeStep, float, float], ...], order: ComposeOrder = ComposeOrder.POST) -> "Matrix":
    # ONE ordered affine through Matrix.pre_*/post_*: rotate takes the lone angle, scale/translate/skew
    # the pair — a declared sequence, never a hand-built 6-tuple.
    matrix = Matrix()
    for step, a, b in steps:
        composed = getattr(matrix, f"{order.value}_{step.value}")
        composed(a) if step is ComposeStep.ROTATE else composed(a, b)
    return matrix


def reflect(points: Iterable[Point2], across: Point2) -> tuple[Point2, ...]:
    pivot = Point(*across)
    return tuple((float(r.x), float(r.y)) for r in (Point(*pt).reflected_across(pivot) for pt in points))


def polar(origin: Point2, angle: float, distance: float) -> Point2:
    placed = Point(*origin).polar_to(angle, distance)
    return (float(placed.x), float(placed.y))


def px(length: Span, viewbox: object = None, tolerance: Tolerance = TOLERANCE) -> float:
    return Length(length).value(ppi=tolerance.ppi, viewbox=viewbox)


def in_units(length: Span, unit: Unit = Unit.MM) -> float:
    # the unit egress: the catalogued to_mm/to_cm/to_inch conversion emits exactly its unit token, so the
    # suffix strip is total — never a general string parse.
    converted = str(getattr(Length(length), unit.converter)())
    return float(converted.removesuffix(unit.value))


def fragment(geometry: object, matrix: "Matrix | None" = None) -> str:
    # the d-string egress through ONE owner; the <path> element, paint, and framing are region's drawsvg surface, never emitted here.
    return (SvgPath(geometry) if matrix is None else SvgPath(geometry) * matrix).d()


# --- [COMPOSITION] ----------------------------------------------------------------------
@tagged_union(frozen=True)
class PathOp:
    tag: PathOpTag = tag()
    bounds: tuple[bytes, BoundsKind] = case()
    measure: bytes = case()
    sample: tuple[bytes, tuple[float, ...]] = case()
    point_at: tuple[bytes, tuple[float, ...]] = case()
    decimate: tuple[bytes, float | None] = case()
    centroid: bytes = case()
    flatten: tuple[bytes, FlattenKind, float | None] = case()
    subpaths: bytes = case()
    project: tuple[tuple[Point2, ...], "Matrix", ProjectKind, bool] = case()
    fit_matrix: tuple[bytes, Bounds] = case()

    @staticmethod
    def Bounds(source: bytes, kind: BoundsKind = BoundsKind.GEOMETRIC) -> "PathOp":
        return PathOp(bounds=(source, kind))

    @staticmethod
    def Measure(source: bytes) -> "PathOp":
        return PathOp(measure=source)

    @staticmethod
    def Sample(source: bytes, positions: float | Iterable[float]) -> "PathOp":
        return PathOp(sample=(source, tuple(positions) if isinstance(positions, Iterable) else (positions,)))

    @staticmethod
    def PointAt(source: bytes, distances: float | Iterable[float]) -> "PathOp":
        return PathOp(point_at=(source, tuple(distances) if isinstance(distances, Iterable) else (distances,)))

    @staticmethod
    def Decimate(source: bytes, epsilon: float | None = None) -> "PathOp":
        return PathOp(decimate=(source, epsilon))

    @staticmethod
    def Centroid(source: bytes) -> "PathOp":
        return PathOp(centroid=source)

    @staticmethod
    def Flatten(source: bytes, kind: FlattenKind = FlattenKind.CUBICS, error: float | None = None) -> "PathOp":
        return PathOp(flatten=(source, kind, error))

    @staticmethod
    def Subpaths(source: bytes) -> "PathOp":
        return PathOp(subpaths=source)

    @staticmethod
    def Project(points: Iterable[Point2], matrix: "Matrix", kind: ProjectKind = ProjectKind.POINT, inverse: bool = False) -> "PathOp":
        return PathOp(project=(tuple(points), matrix, kind, inverse))

    @staticmethod
    def FitMatrix(source: bytes, target: Bounds) -> "PathOp":
        return PathOp(fit_matrix=(source, target))


@tagged_union(frozen=True)
class PathResult:
    tag: PathResultTag = tag()
    extent: Bounds = case()
    measure: float = case()
    sampled: tuple[Point2, ...] = case()
    oriented: tuple[Oriented, ...] = case()
    reduced: tuple[Point2, ...] = case()
    anchor: Point2 = case()
    contours: tuple[str, ...] = case()
    fragment: str = case()
    placed: "Matrix" = case()


_CONTRACT = BeartypeConf(is_pep484_tower=True)


def _contracted(operation: Callable[[PathOp], Result[PathResult, PathFault]], /) -> Callable[[PathOp], Result[PathResult, PathFault]]:
    guarded = beartype(conf=_CONTRACT)(operation)

    @wraps(operation)
    def call(op: PathOp, /) -> Result[PathResult, PathFault]:
        try:
            return guarded(op)
        except BeartypeCallHintViolation as violation:
            return Error(PathFault(contract=type(violation).__name__))

    return call


@_contracted
def _dispatch(op: PathOp, /) -> Result[PathResult, PathFault]:
    match op:
        case PathOp(tag="bounds", bounds=(source, kind)):
            return bounds(source, kind).map(lambda extent: PathResult(extent=extent))
        case PathOp(tag="measure", measure=source):
            return measure(source).map(lambda length: PathResult(measure=length))
        case PathOp(tag="sample", sample=(source, positions)):
            return sample(source, positions).map(lambda points: PathResult(sampled=points))
        case PathOp(tag="point_at", point_at=(source, distances)):
            return point_at(source, distances).map(lambda rows: PathResult(oriented=rows))
        case PathOp(tag="decimate", decimate=(source, epsilon)):
            return decimate(source, epsilon).map(lambda points: PathResult(reduced=points))
        case PathOp(tag="centroid", centroid=source):
            return centroid(source).map(lambda point: PathResult(anchor=point))
        case PathOp(tag="flatten", flatten=(source, kind, error)):
            return flatten(source, kind, error).map(lambda d: PathResult(fragment=d))
        case PathOp(tag="subpaths", subpaths=source):
            return subpaths(source).map(lambda contours: PathResult(contours=contours))
        case PathOp(tag="project", project=(points, matrix, kind, inverse)):
            return project(points, matrix, kind, inverse).map(lambda projected: PathResult(sampled=projected))
        case PathOp(tag="fit_matrix", fit_matrix=(source, target)):
            return fit_matrix(source, target).map(lambda placed: PathResult(placed=placed))
        case _ as unreachable:
            assert_never(unreachable)


def _worked(ops: tuple[PathOp, ...], /) -> Result[Block[PathResult], PathFault]:
    return traverse(_dispatch, Block.of_seq(ops))


class Path(Struct, frozen=True):
    ops: tuple[PathOp, ...]

    @classmethod
    def over(cls, ops: PathOp | Iterable[PathOp], /) -> Self:
        match ops:
            case PathOp():
                return cls(ops=(ops,))
            case _:
                return cls(ops=tuple(ops))

    async def of(self, lane: LanePolicy, /) -> RuntimeRail[Block[PathResult]]:
        # the sweep is synchronous CPU work: the whole batch crosses the runtime lane's PROCESS offload
        # under the runtime-owned worker bound — zero folder-minted limiters.
        return await lane.offload(_worked, self.ops, modality=Modality.PROCESS)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "Bounds",
    "BoundsKind",
    "ComposeOrder",
    "ComposeStep",
    "Element",
    "FlattenKind",
    "Oriented",
    "Path",
    "PathFault",
    "PathOp",
    "PathResult",
    "Point2",
    "ProjectKind",
    "Span",
    "TOLERANCE",
    "Tolerance",
    "Unit",
    "bounds",
    "centroid",
    "combined",
    "compose",
    "decimate",
    "elements",
    "fit_matrix",
    "flatten",
    "fragment",
    "in_units",
    "measure",
    "point_at",
    "polar",
    "project",
    "px",
    "reflect",
    "sample",
    "scene",
    "subpaths",
]
```

```mermaid
flowchart LR
    Over["Path.over (PathOp | Iterable)"] --> Of["Path.of(lane) -> offload PROCESS"]
    Of --> Disp["_dispatch (@_contracted) match per op"]
    Disp -->|bounds| Bd["bounds(source, BoundsKind) -> extent"]
    Disp -->|measure| Ms["measure(source) -> SvgPath.length"]
    Disp -->|sample| Sm["sample(source, t...) -> SvgPath.npoint"]
    Disp -->|point_at| Pa["point_at(source, distances) -> metric (point, tangent) rows"]
    Disp -->|decimate| Dc["decimate(source, epsilon) -> RDP polyline"]
    Disp -->|centroid| Cn["centroid(source) -> area-weighted anchor"]
    Disp -->|flatten| Fl["flatten(source, kind, error) -> d string"]
    Disp -->|subpaths| Sp["subpaths(source) -> per-contour d strings"]
    Disp -->|project| Pj["project(points, matrix, kind, inverse)"]
    Disp -->|fit_matrix| Ft["fit_matrix(source, target) -> Viewbox Matrix"]
    Sweep["_polyline: npoint sweep + cumsum chords"] -.->|one metric core| Pa
    Sweep -.-> Dc
    Sweep -.-> Cn
    Parse["_parsed @lru_cache(SVG.parse reify=True)"] -.->|memoized ingest| Disp
    Bd --> Result["Result[PathResult, PathFault]"]
    Ms --> Result
    Sm --> Result
    Pa --> Result
    Dc --> Result
    Cn --> Result
    Fl --> Result
    Sp --> Result
    Pj --> Result
    Ft --> Result
    Result -->|traverse fold| Block["Block[PathResult]"]
    Block -.->|awaited rail| Consumers["region / pattern / drawing symbol+dimension+detail / compose / sheet / dxf / solar / marks-encode"]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
