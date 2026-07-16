# [PY_ARTIFACTS_COMPOSE]

`Figure` owns post-render figure placement — turning already-emitted SVG graphics into placed, annotated, color-correct figures. It scales-to-fit a viewport, tiles an n-up sheet, crops, rotates, mattes, merges, overlays registration marks, rasterizes-then-annotates, and projects to a one-page vector PDF, discriminating a closed-payload `FigureOp` `tagged_union` by one total `match` — one typed payload per case, never a `StrEnum` over an erased `dict`. It places and finishes graphics already emitted by the chart/mark/table siblings; it re-renders no chart and holds no vector-geometry primitive, only the placement-specific arm bodies the geometry surface does not own.

`svgelements`'s `Matrix`/`Path`/`Color`/`Angle` algebra lives once in `graphic/vector/path#PATH` (with `bounds`/`elements`/`fragment`/`px`, the `Bounds`/`Span` value objects, and the `PathFault` rail) and the set-op/serialization surface in `graphic/vector/region#REGION` (`clip`/`boolean`/`outline`/`document`, the `RenderPolicy` rasterize-policy owner, the `RegionFault` rail); this owner imports both, composes them one hop, and re-declares none. Vector arms offload the pure-Python `svgelements` fold to a THREAD lane; the `Annotate`/`Metadata` arms offload the whole `resvg_py.svg_to_bytes` rasterize plus `pillow` draw/finish/metadata pass to a PROCESS worker under `RetryClass.OCCT`, the `lazy import resvg_py`/`lazy from PIL` proxies reifying inside the worker; foreign `PathFault`/`RegionFault` cross into `BoundaryFault` at the single `_extent` seam. This owner owns the placed flat single-`<svg>` egress; the editable named-layer egress projects through `Figure.layers -> tuple[Layer, ...]` to `export/layered#LAYERED`, the one-page vector PDF the `composition/sheet#SHEET`/`imposition#IMPOSE` consumers draw is the `Pdf` case's `vl_convert.svg_to_pdf` wrap of that flat document, the full source->dest ICC transform / soft-proof / CMYK separations compose outward to `graphic/color/managed#MANAGED`, and the authoritative cross-format descriptive-metadata seal to `exchange/metadata#METADATA`. Receipts thread `core/receipt#RECEIPT`'s named `ArtifactReceipt.Preview(key, width, height)` / `Pdf(key, bytes, pages)` mints positionally; every figure routes through the `core/plan#PLAN` `ArtifactPipeline` as a producer node.

## [01]-[INDEX]

- [01]-[COMPOSE]: the post-render placement owner discriminating a closed `FigureOp` `tagged_union` — vector placement (`ScaleFit`/`Tile`/`Crop`/`Merge`/`Matte`/`Rotate`/`Overlay`), the `Pdf` SVG-to-PDF projection, and the gated raster `Annotate`/`Metadata` arms — over the imported `graphic/vector/path#PATH` and `graphic/vector/region#REGION` surfaces, railed `RuntimeRail[ArtifactReceipt]` over `async_boundary(catch=_FAULTS)`.

## [02]-[COMPOSE]

- Owner: `Figure` discriminates over the closed `FigureOp` `expression.tagged_union`, one typed payload per case, never a `StrEnum` over a shared erased `dict`. `svgelements`'s algebra and the `bounds`/`elements`/`fragment`/`px`/`clip`/`boolean`/`outline`/`document` functions live once in `graphic/vector/path#PATH` and `graphic/vector/region#REGION`; this owner imports and composes them one hop, holding only the placement arm bodies plus the `pillow` `Image` raster surface on the gated floor. `RenderPolicy` is REGION's rasterize-policy owner carried into `Annotate` so the resvg policy has one edit site across the chart/diagram/figure consumers. `TextStyle` is the one measured-text owner both `Text` and `Caption` compose, so a stroked complex-script variable-font caption and a bare label share one shape. `DrawOp` collapses the rejected `CaptionSpec`/`BoxSpec`/`PostSpec` triple into one pillow draw family folded by one total `match`, so a new annotation primitive is one case, never a parallel spec struct. `RasterSource` collapses `svg_string` markup and an `.svg`/`.svgz` file path into one case whose `keywords()` projects the live `svg_to_bytes` source keyword, so a file source never grows a second render call. Each `MarkKind` member carries its own verified primitive name and float-only arg row resolved through the bound `MarkKind.shape` builder, so the overlay fold dispatches total over the member, never a per-call `dict[MarkKind, Callable]` over an erased shape bag.
- Cases: dispatch is one total `match` over the closed `FigureOp` — never a sibling op per source media type, a parallel mark emitter per primitive, or a parallel figure class. `Crop` is a real `skia-pathops` `PathOp.INTERSECTION` geometry sever (REGION `clip`), not a CSS `<clipPath>` mask a downstream must honor; `Merge` and `Matte` compose REGION's `boolean`/`outline` for the planar set-op and the fixed-width offset keyline — the offset algebra `svgelements` cannot express, `skia-pathops` exposing no `Path.offset` (the stroke IS the offset), the matte framed UNDER the source. `Pdf` is a pure `vl_convert.svg_to_pdf` projection of the placed flat `<svg>`, the post-edit-SVG-then-PDF path the `sheet`/`imposition` consumers draw, never a re-composition. `Annotate`'s trailing `icc` embeds the working-space profile on the PNG egress through `Image.save(icc_profile=)`; the full source->dest transform / soft-proof / CMYK separations ride `graphic/color/managed#MANAGED` outward, never re-owned here. `Metadata` is the figure-egress-local in-worker PNG EXIF/XMP tag convenience — NOT the authoritative cross-format seal `exchange/metadata#METADATA` owns, composed outward when a full descriptive-metadata write is the deliverable.
- Auto: the vector arms offload `_compose_vector` to a THREAD lane so the pure-Python `svgelements` fold never runs on the loop; the `Annotate`/`Metadata` arms offload the whole `resvg_py.svg_to_bytes` rasterize plus `pillow` `DrawOp` fold to a `RetryClass.OCCT`-retried PROCESS worker where every native call resolves at worker scope; the `Pdf` arm offloads the GIL-releasing `vl_convert.svg_to_pdf` to a THREAD lane, a pure projection re-composing nothing. In-process arms derive the content key and the `ArtifactReceipt.Preview`/`Pdf` evidence from the SAME deterministic render within each projection, so key and facts cannot diverge; the frozen owner carries no memo, so `_emit` and `_placed` recompute per projection, matching the sibling `sheet`/`imposition` `Composed` re-entry. Gated annotate/metadata PNG mints only inside the subprocess, so `_placed` returns `Nothing` and `contribute` yields no row — their receipt rides the async `Figure.of` emission outward, never a placeholder `Placed` over empty bytes.
- Receipt: in-process placement contributes `core/receipt#RECEIPT`'s named `ArtifactReceipt.Preview(key, width, height)` mint (empty perceptual `scores` — placement carries no perceptual measurement, that band is `graphic/raster/measure#MEASURE`'s); `Pdf` mints `ArtifactReceipt.Pdf(key, bytes, 1)`. No new kind and no new evidence `Struct` — the phantom `ArtifactReceipt.of(key, PreviewFacts(...))` indirection is the form the receipt owner deleted, its named per-kind mints taking the scalars positionally. `_placed(op)` folds bytes and placed viewport into ONE `Placed` per call so the content key and the receipt read the SAME bytes, and the phase is the constant `"emitted"`; the `admitted`/`planned` lifecycle facts are the `core/plan#PLAN` planner's own `Receipt.of` mints, not figure cases.
- Growth: a further vector layout op is one `FigureOp` case plus one `_compose_vector` arm over the imported surface; a new planar set-op is one `BooleanOp` member the `Merge` arm reads and a new offset cap/join one REGION `CapStyle`/`JoinStyle` at `outline`; an n-up refinement is one field on `Tile`; a registration primitive is one `MarkKind` member through the shared builder; an annotation source mode one `RasterSource` case projecting one `svg_to_bytes` keyword; a raster primitive one `DrawOp` case, a finish filter one `FilterKind`, a blend one `BlendKind`, a text axis one `TextStyle` field. A resvg knob grows the imported `RenderPolicy` at its owner with zero edit here; a PDF-projection knob is one field on `Pdf`; a color-tagged egress is the `Annotate` `icc` field, the full ICC transform an outward `graphic/color/managed#MANAGED` compose; the authoritative cross-format metadata write an outward `exchange/metadata#METADATA` compose. Zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Sequence
from enum import Enum
from io import BytesIO
from typing import TYPE_CHECKING, Annotated, Literal, NoReturn, assert_never

from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from expression import Nothing, Option, Some, case, tag, tagged_union
from msgspec import Struct


from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

# PATH and REGION own the imported vector surface: `clip` the per-shape `PathOp.INTERSECTION` crop, `boolean`
# the N-ary set-op `svgelements` cannot express, `outline` the fixed-width offset (`Path.stroke`; no
# `Path.offset` exists, the stroke IS the offset), `PathFault`/`RegionFault` the rails the `_extent` seam lifts.
from artifacts.graphic.vector.path import Bounds, PathFault, Span, bounds, elements, fragment, px
from artifacts.graphic.vector.region import BooleanOp, RegionFault, RenderPolicy, Stroked, boolean, clip, document, outline
from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.export.layered import Layer

lazy import resvg_py
lazy import svgelements
lazy import vl_convert
lazy from PIL import Image, ImageChops, ImageDraw, ImageEnhance, ImageFilter, ImageFont, ImageOps, features
lazy from svgelements import Angle, Matrix, Point

if TYPE_CHECKING:
    from rasm.runtime.receipts import Receipt


# --- [TYPES] ----------------------------------------------------------------------------
type Corner = Literal["nw", "ne", "sw", "se", "n", "s", "e", "w", "center"]
type Anchor = tuple[float, float]
type Extent = Annotated[float, Is[lambda value: value > 0.0]]
type Columns = Annotated[int, Is[lambda count: count >= 1]]
type MarkArgs = Callable[[float, float, float], tuple[float, ...]]
type FilterKind = Literal[
    "unsharp", "blur", "median", "sharpness", "contrast", "brightness", "saturation", "autocontrast", "equalize", "posterize", "solarize"
]
type ArcKind = Literal["arc", "chord", "pieslice"]
type BlendKind = Literal["multiply", "screen", "overlay", "soft_light", "hard_light", "difference", "add", "subtract", "darker", "lighter"]


class MarkKind(Enum):
    # each member is (svgelements-primitive-name, float-arg builder); the positional constructor
    # arity is verified — Circle(cx, cy, r), Ellipse(cx, cy, rx, ry), Rect(x, y, w, h),
    # SimpleLine(x1, y1, x2, y2), Polyline/Polygon(*points) — so `MarkKind.shape` is settled fence.
    CORNER = ("Polyline", lambda x, y, s: (x, y - s, x, y, x + s, y))
    TICK = ("SimpleLine", lambda x, y, s: (x - s, y, x + s, y))
    TARGET = ("Circle", lambda x, y, s: (x, y, s))
    REGISTRATION = ("Ellipse", lambda x, y, s: (x, y, s, s * 0.5))
    GUTTER = ("Polyline", lambda x, y, s: (x - s, y, x, y - s, x + s, y, x, y + s, x - s, y))
    MITER = ("Polygon", lambda x, y, s: (x - s, y, x, y - s, x + s, y, x, y + s))
    BLEED = ("Rect", lambda x, y, s: (x - s, y - s, 2.0 * s, 2.0 * s))
    CROSS = ("Polyline", lambda x, y, s: (x - s, y, x, y, x, y - s, x, y, x + s, y, x, y, x, y + s))
    STAR = ( "Polyline", lambda x, y, s: (x, y - s, x, y, x + s, y - s, x, y, x + s, y, x, y, x + s, y + s, x,
            y, x, y + s, x, y, x - s, y + s, x, y, x - s, y, x, y, x - s, y - s,),
    )

    def __init__(self, primitive: str, args: MarkArgs) -> None:
        self.primitive = primitive
        self.args = args

    def shape(self, anchor: Anchor, size: float) -> object:
        return getattr(svgelements, self.primitive)(*self.args(*anchor, size))


# --- [CONSTANTS] ------------------------------------------------------------------------
# the engine raise tuple: `ValueError` on invalid/empty SVG or render failure, `OSError` on the `.svgz`
# file arm, `BrokenWorkerProcess` on an exhausted worker-death retry, `BeartypeCallHintViolation` on a
# degenerate source bbox or non-positive column count at the `_fit`/`_rows`/`_extent` seams.
_FAULTS: tuple[type[Exception], ...] = (ValueError, OSError, BrokenWorkerProcess, BeartypeCallHintViolation)

# the `_fit`/`_rows` division seams refine their `Extent`/`Columns` divisor scalars, so a degenerate extent
# or non-positive grid count rails as `BeartypeCallHintViolation` rather than a `ZeroDivisionError` deep in
# the fold — only a direct scalar parameter is deep-checked, never a `Bounds`/case field.
_GUARD = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))


# --- [MODELS] ---------------------------------------------------------------------------
class MarkSpec(Struct, frozen=True):
    kind: MarkKind
    corner: Corner = "nw"
    size: float = 12.0
    inset: float = 6.0
    stroke: str = "black"
    width: float = 0.5


# the measured-text owner `Text` and `Caption` both compose: `_face` reads `font`/`size`/`variation` and
# gates `Layout.RAQM` on `features.check("raqm")`; `direction`/`language`/`features` drive complex-script
# shaping, `variation` a variable-font axis vector, `stroke_width`/`stroke_fill` a legibility halo.
class TextStyle(Struct, frozen=True):
    font: str | None = None
    size: float = 16.0
    fill: str = "black"
    anchor: str | None = None
    align: str = "left"
    spacing: float = 4.0
    stroke_width: int = 0
    stroke_fill: str | None = None
    direction: str | None = None
    language: str | None = None
    features: tuple[str, ...] = ()
    variation: tuple[float, ...] = ()


@tagged_union(frozen=True)
class RasterSource:
    tag: Literal["markup", "file"] = tag()
    markup: bytes = case()
    file: str = case()

    @staticmethod
    def Markup(svg: bytes) -> "RasterSource":
        return RasterSource(markup=svg)

    @staticmethod
    def File(path: str) -> "RasterSource":
        return RasterSource(file=path)

    def keywords(self) -> dict[str, str]:
        match self:
            case RasterSource(tag="markup", markup=svg):
                return {"svg_string": svg.decode()}
            case RasterSource(tag="file", file=path):
                return {"svg_path": path}
            case _:
                assert_never(self)


@tagged_union(frozen=True)
class DrawOp:
    tag: Literal["text", "caption", "box", "round_box", "line", "ellipse", "arc", "polygon", "regular", "stamp", "blend", "grade", "frame"] = tag()
    text: tuple[str, Anchor, TextStyle] = case()
    caption: tuple[str, Anchor, TextStyle, float, float, str | None, str | None] = case()
    box: tuple[Bounds, str, str | None, int] = case()
    round_box: tuple[Bounds, float, str, str | None, int] = case()
    line: tuple[tuple[Anchor, ...], str, int] = case()
    ellipse: tuple[Bounds, str | None, str | None, int] = case()
    arc: tuple[Bounds, float, float, ArcKind, str | None, str | None, int] = case()
    polygon: tuple[tuple[Anchor, ...], str | None, str, int] = case()
    regular: tuple[Anchor, float, int, float, str | None, str, int] = case()
    stamp: tuple[bytes, Anchor, float] = case()
    blend: tuple[bytes, BlendKind] = case()
    grade: tuple[int, tuple[float, ...]] = case()
    frame: tuple[FilterKind | None, float, int, str, tuple[int, int] | None, bool] = case()

    @staticmethod
    def Text(text: str, xy: Anchor, style: TextStyle = TextStyle()) -> "DrawOp":
        return DrawOp(text=(text, xy, style))

    @staticmethod
    def Caption(
        text: str,
        xy: Anchor,
        style: TextStyle = TextStyle(),
        pad: float = 6.0,
        radius: float = 4.0,
        box_fill: str | None = "white",
        box_outline: str | None = "black",
    ) -> "DrawOp":
        return DrawOp(caption=(text, xy, style, pad, radius, box_fill, box_outline))

    @staticmethod
    def Box(box: Bounds, outline: str = "black", fill: str | None = None, width: int = 1) -> "DrawOp":
        return DrawOp(box=(box, outline, fill, width))

    @staticmethod
    def RoundBox(box: Bounds, radius: float = 4.0, outline: str = "black", fill: str | None = None, width: int = 1) -> "DrawOp":
        return DrawOp(round_box=(box, radius, outline, fill, width))

    @staticmethod
    def Line(points: tuple[Anchor, ...], fill: str = "black", width: int = 1) -> "DrawOp":
        return DrawOp(line=(points, fill, width))

    @staticmethod
    def Ellipse(box: Bounds, outline: str | None = "black", fill: str | None = None, width: int = 1) -> "DrawOp":
        return DrawOp(ellipse=(box, outline, fill, width))

    @staticmethod
    def Arc(
        box: Bounds, start: float, end: float, kind: ArcKind = "arc", fill: str | None = "black", outline: str | None = "black", width: int = 1
    ) -> "DrawOp":
        return DrawOp(arc=(box, start, end, kind, fill, outline, width))

    @staticmethod
    def Polygon(points: tuple[Anchor, ...], fill: str | None = None, outline: str = "black", width: int = 1) -> "DrawOp":
        return DrawOp(polygon=(points, fill, outline, width))

    @staticmethod
    def Regular(
        center: Anchor, radius: float, sides: int, rotation: float = 0.0, fill: str | None = None, outline: str = "black", width: int = 1
    ) -> "DrawOp":
        return DrawOp(regular=(center, radius, sides, rotation, fill, outline, width))

    @staticmethod
    def Stamp(image: bytes, xy: Anchor, opacity: float = 1.0) -> "DrawOp":
        return DrawOp(stamp=(image, xy, opacity))

    @staticmethod
    def Blend(image: bytes, mode: BlendKind = "multiply") -> "DrawOp":
        return DrawOp(blend=(image, mode))

    @staticmethod
    def Grade(size: int, table: tuple[float, ...]) -> "DrawOp":
        return DrawOp(grade=(size, table))

    @staticmethod
    def Frame(
        filter: FilterKind | None = None,
        radius: float = 2.0,
        border: int = 0,
        border_fill: str = "white",
        fit: tuple[int, int] | None = None,
        exif_orient: bool = False,
    ) -> "DrawOp":
        return DrawOp(frame=(filter, radius, border, border_fill, fit, exif_orient))


@tagged_union(frozen=True)
class FigureOp:
    tag: Literal["scale_fit", "tile", "crop", "merge", "matte", "rotate", "overlay", "pdf", "annotate", "metadata"] = tag()
    scale_fit: tuple[bytes, Span, Span] = case()
    tile: tuple[tuple[bytes, ...], int, Span, Span, float] = case()
    crop: tuple[bytes, Bounds] = case()
    merge: tuple[tuple[bytes, ...], BooleanOp] = case()  # N-source planar set-op — imported REGION `boolean`
    matte: tuple[bytes, float, str] = case()  # fixed-width offset keyline — imported REGION `outline`
    rotate: tuple[bytes, str, Corner] = case()
    overlay: tuple[bytes, tuple[MarkSpec, ...]] = case()
    pdf: tuple[bytes, float] = case()
    annotate: tuple[RasterSource, RenderPolicy, tuple[DrawOp, ...], bytes | None] = (
        case()
    )  # trailing `icc` embeds the working-space profile on the PNG; the full transform is graphic/color/managed#MANAGED's
    metadata: tuple[bytes, tuple[tuple[int, str], ...], str | None] = case()

    @staticmethod
    def ScaleFit(source: bytes, width: Span, height: Span) -> "FigureOp":
        return FigureOp(scale_fit=(source, width, height))

    @staticmethod
    def Tile(sources: tuple[bytes, ...], columns: int, cell_width: Span, cell_height: Span, gutter: float = 0.0) -> "FigureOp":
        return FigureOp(tile=(sources, columns, cell_width, cell_height, gutter))

    @staticmethod
    def Crop(source: bytes, x: float, y: float, width: float, height: float) -> "FigureOp":
        return FigureOp(crop=(source, (x, y, x + width, y + height)))

    @staticmethod
    def Merge(sources: tuple[bytes, ...], op: BooleanOp = BooleanOp.UNION) -> "FigureOp":
        return FigureOp(merge=(sources, op))

    @staticmethod
    def Matte(source: bytes, width: float = 3.0, stroke: str = "black") -> "FigureOp":
        return FigureOp(matte=(source, width, stroke))

    @staticmethod
    def Rotate(source: bytes, angle: str, corner: Corner = "center") -> "FigureOp":
        return FigureOp(rotate=(source, angle, corner))

    @staticmethod
    def Overlay(source: bytes, marks: tuple[MarkSpec, ...]) -> "FigureOp":
        return FigureOp(overlay=(source, marks))

    @staticmethod
    def Pdf(source: bytes, scale: float = 1.0) -> "FigureOp":
        return FigureOp(pdf=(source, scale))

    @staticmethod
    def Annotate(source: RasterSource, render: RenderPolicy = RenderPolicy(), draws: tuple[DrawOp, ...] = (), icc: bytes | None = None) -> "FigureOp":
        return FigureOp(annotate=(source, render, draws, icc))

    @staticmethod
    def Metadata(source: bytes, exif: tuple[tuple[int, str], ...] = (), xmp: str | None = None) -> "FigureOp":
        return FigureOp(metadata=(source, exif, xmp))


# one `_compose_vector`/`svg_to_pdf` result feeds the content key AND the receipt evidence off the same
# local `data`, so they cannot diverge within a projection; a pure fold each projection re-enters (no memo).
class Placed(Struct, frozen=True):
    key: ContentKey
    data: bytes
    receipt: ArtifactReceipt


# --- [SERVICES] -------------------------------------------------------------------------
class Figure(Struct, frozen=True):
    op: FigureOp

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical op payload minted PRE-RUN — never a key over the placed-figure bytes.
        return ContentIdentity.of(f"figure-{self.op.tag}", self.op, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        railed = await async_boundary(f"figure.{self.op.tag}", self._placed_key, catch=_FAULTS)
        return railed.map(
            lambda _out: _placed(self.op)
            .map(lambda placed: placed.receipt)
            .default_value(ArtifactReceipt.Preview(self._key, 0, 0))
        )

    async def _placed_key(self) -> ContentKey:
        match self.op:
            case FigureOp(tag="annotate", annotate=(source, render, draws, icc)):
                # the resvg rasterize AND the pillow fold ride the worker beside each other; only the cheap
                # `render.kwargs` dict + optional ICC bytes cross the seam, never the heavy native render as an arg.
                data = _out_of(await LanePolicy.offload(_gated_annotate, render.kwargs(source.keywords()), draws, icc, modality=Modality.PROCESS, retry=RetryClass.OCCT))
            case FigureOp(tag="metadata", metadata=(source, exif, xmp)):
                data = _out_of(await LanePolicy.offload(_gated_metadata, source, exif, xmp, modality=Modality.PROCESS, retry=RetryClass.OCCT))
            case FigureOp(tag="pdf", pdf=(source, scale)):
                # `vl_convert` is a GIL-releasing native render, so only the cheap `source.decode()` crosses.
                data = _out_of(await LanePolicy.offload(vl_convert.svg_to_pdf, source.decode(), scale, modality=Modality.THREAD))
            case _:
                data = _out_of(await LanePolicy.offload(_compose_vector, self.op, modality=Modality.THREAD))
        return ContentIdentity.key(
            f"figure-{self.op.tag}", data
        )  # the whole-byte figure source is infallible (no canonical encode), so a bare `ContentKey` the boundary wraps

    def contribute(self) -> "Iterable[Receipt]":
        # `Some` for the in-process arms (bytes/key/receipt off the ONE sync render), `Nothing` for the
        # gated annotate/metadata arms whose receipt rides the async `Figure.of` outward — no placeholder.
        return _placed(self.op).map(lambda placed: tuple(placed.receipt.contribute())).default_value(())

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return _placed_layers(self.op, names)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _out_of[T](rail: "RuntimeRail[T]", /) -> T:
    # terminal collapse: an offload fault reconstructs the raise the `_FAULTS` boundary folds.
    return rail.default_with(_figure_raise)


def _figure_raise(fault: object) -> object:
    raise ValueError(str(fault))


# `Some` for the pdf/vector arms whose bytes/key/receipt derive from the ONE sync render, `Nothing` for the
# gated annotate/metadata arms that mint only inside async `_emit` — never an empty-byte placeholder key.
def _placed(op: FigureOp) -> Option[Placed]:
    match op:
        case FigureOp(tag="pdf", pdf=(source, scale)):
            data = vl_convert.svg_to_pdf(source.decode(), scale=scale)
            key = ContentIdentity.key("figure-pdf", data)
            return Some(Placed(key, data, ArtifactReceipt.Pdf(key, len(data), 1)))
        case FigureOp(tag="annotate") | FigureOp(tag="metadata"):
            return Nothing
        case _:
            data = _compose_vector(op)
            xmin, ymin, xmax, ymax = _extent(data)
            key = ContentIdentity.key(f"figure-{op.tag}", data)
            return Some(Placed(key, data, ArtifactReceipt.Preview(key, int(xmax - xmin), int(ymax - ymin))))


def _placed_layers(op: FigureOp, names: tuple[str, ...]) -> tuple[Layer, ...]:
    match op:
        case FigureOp(tag="tile", tile=(sources, columns, cell_width, cell_height, gutter)):
            cw, ch = px(cell_width), px(cell_height)
            rows = _rows(len(sources), columns)
            width, height = cw * columns + gutter * (columns - 1), ch * rows + gutter * (rows - 1)
            return tuple(
                Layer(
                    _name(names, index),
                    document(_place(raw, index, columns, cw, ch, gutter), (0.0, 0.0, width, height)),
                    (
                        index % columns * (cw + gutter),
                        index // columns * (ch + gutter),
                        index % columns * (cw + gutter) + cw,
                        index // columns * (ch + gutter) + ch,
                    ),
                )
                for index, raw in enumerate(sources)
            )
        case FigureOp(tag="overlay", overlay=(source, marks)):
            extent = _extent(source)
            base = Layer(_name(names, 0, "base"), document([fragment(shape) for shape in elements(source)], extent), extent)
            return (base, Layer(_name(names, 1, "overlay"), document(_marks(extent, marks), extent), extent))
        case FigureOp(tag="scale_fit") | FigureOp(tag="crop") | FigureOp(tag="rotate") | FigureOp(tag="merge") | FigureOp(tag="matte"):
            # the single placed layer reads its OWN extent off the emitted bytes — these arms re-origin or
            # re-shape the viewport, so the source-document bounds is the wrong frame.
            placed = _compose_vector(op)
            return (Layer(_name(names, 0), placed, _extent(placed)),)
        case _:
            # `pdf`/`annotate`/`metadata` carry no named-layer projection — a VALID empty arm, not an
            # `assert_never` (those tags are reachable here, so it would mis-fire at type-check).
            return ()


def _name(names: tuple[str, ...], index: int, fallback: str = "") -> str:
    return names[index] if index < len(names) else fallback or f"layer-{index}"


# the `_GUARD`-contracted division seams — the one place the placement math admits an external scalar.
@_GUARD
def _fit(width: float, height: float, extent_w: Extent, extent_h: Extent, /) -> float:
    return min(width / extent_w, height / extent_h)


@_GUARD
def _rows(count: int, columns: Columns, /) -> int:
    return -(-count // columns)


# the placement-math arm bodies over the imported vector surface: every arm folds `elements(source)` through
# `fragment(shape, matrix)` onto `document(fragments, viewbox)` — never the imported `transform` (a whole-
# document re-frame this owner does not want) and never a re-parsed `<svg>` shell.
def _compose_vector(op: FigureOp) -> bytes:
    match op:
        case FigureOp(tag="scale_fit", scale_fit=(source, width, height)):
            (xmin, ymin, xmax, ymax), (tw, th) = _extent(source), (px(width), px(height))
            factor = _fit(tw, th, xmax - xmin, ymax - ymin)
            matrix = Matrix.translate(-xmin, -ymin) * Matrix.scale(factor)
            return document([fragment(shape, matrix) for shape in elements(source)], (0.0, 0.0, tw, th))
        case FigureOp(tag="tile", tile=(sources, columns, cell_width, cell_height, gutter)):
            cw, ch = px(cell_width), px(cell_height)
            rows = _rows(len(sources), columns)
            placed = [frag for index, raw in enumerate(sources) for frag in _place(raw, index, columns, cw, ch, gutter)]
            return document(placed, (0.0, 0.0, cw * columns + gutter * (columns - 1), ch * rows + gutter * (rows - 1)))
        case FigureOp(tag="crop", crop=(source, box)):
            # REGION `clip` — a real `PathOp.INTERSECTION` per-shape sever framed to the rect, not a CSS `<clipPath>`.
            return clip(source, box).default_with(_source_fault)
        case FigureOp(tag="merge", merge=(sources, op)):
            # REGION `boolean` — the N-ary `OpBuilder` planar set-op, one composed op, never a per-op method.
            return boolean(sources, op).default_with(_source_fault)
        case FigureOp(tag="matte", matte=(source, width, stroke)):
            # REGION `outline` strokes the silhouette into a closed boundary and frames it UNDER the source so
            # the artwork sits on its own trim/bleed keyline — the offset `svgelements` cannot express.
            matte = outline(source, width).default_with(_source_fault)
            return document([*(fragment(shape) for shape in elements(matte)), *(fragment(shape) for shape in elements(source))], _extent(matte))
        case FigureOp(tag="rotate", rotate=(source, angle, corner)):
            extent = _extent(source)
            ax, ay = _anchor(corner, extent, 0.0)
            pivot = Matrix.translate(ax, ay) * Matrix.rotate(_angle(angle)) * Matrix.translate(-ax, -ay)
            # the corner-pivot rotation expands the AABB, so frame to the pivot-transformed extent corners —
            # the pre-rotation extent clips the turned artwork; the shapes lower through the same pivot.
            xmin, ymin, xmax, ymax = extent
            turned = [Point(x, y).matrix_transform(pivot) for x, y in ((xmin, ymin), (xmax, ymin), (xmax, ymax), (xmin, ymax))]
            frame = (min(p.x for p in turned), min(p.y for p in turned), max(p.x for p in turned), max(p.y for p in turned))
            return document([fragment(shape, pivot) for shape in elements(source)], frame)
        case FigureOp(tag="overlay", overlay=(source, marks)):
            extent = _extent(source)
            return document([*(fragment(shape) for shape in elements(source)), *_marks(extent, marks)], extent)
        case _:
            # only the five geometry arms reach here; `pdf`/`annotate`/`metadata` render elsewhere, so a
            # non-vector op is unreachable — an invariant guard the closed partial fold forbids `assert_never` for.
            raise ValueError(f"figure {op.tag} has no in-process vector composition")


# the ONE rail-to-raise seam shared by `_extent` and the `Crop`/`Merge`/`Matte` clip/boolean/outline arms:
# a foreign `PathFault`/`RegionFault` becomes the `ValueError` the `_FAULTS` boundary classifies, uniform
# with the pillow/`vl_convert` engine raises — never a scattered `.default_value` that swallows the cause.
def _extent(source: bytes) -> Bounds:
    return bounds(source).default_with(_source_fault)


def _source_fault(fault: PathFault | RegionFault) -> "NoReturn":
    raise ValueError(f"figure source: {fault.tag}")


# the registration-overlay mark fold: each `MarkSpec` builds its `svgelements` shape through `MarkKind.shape`,
# serialized to a `Stroked` (d, stroke, width) fragment; both the flat and per-layer egresses compose this fold.
def _marks(extent: Bounds, marks: tuple[MarkSpec, ...]) -> list[Stroked]:
    return [(fragment(spec.kind.shape(_anchor(spec.corner, extent, spec.inset), spec.size)), spec.stroke, spec.width) for spec in marks]


def _anchor(corner: Corner, extent: Bounds, inset: float) -> Anchor:
    xmin, ymin, xmax, ymax = extent
    cx, cy = (xmin + xmax) / 2.0, (ymin + ymax) / 2.0
    match corner:
        case "nw":
            return (xmin + inset, ymin + inset)
        case "ne":
            return (xmax - inset, ymin + inset)
        case "sw":
            return (xmin + inset, ymax - inset)
        case "se":
            return (xmax - inset, ymax - inset)
        case "n":
            return (cx, ymin + inset)
        case "s":
            return (cx, ymax - inset)
        case "e":
            return (xmax - inset, cy)
        case "w":
            return (xmin + inset, cy)
        case "center":
            return (cx, cy)
        case _:
            assert_never(corner)


def _place(source: bytes, index: int, columns: int, cell_w: float, cell_h: float, gutter: float) -> list[str]:
    xmin, ymin, xmax, ymax = _extent(source)
    factor = _fit(cell_w, cell_h, xmax - xmin, ymax - ymin)
    column, row = index % columns, index // columns
    # aspect-fit then center the source within its gutter-spaced cell — a top-left cram is the naive n-up.
    ox = column * (cell_w + gutter) + (cell_w - (xmax - xmin) * factor) / 2.0
    oy = row * (cell_h + gutter) + (cell_h - (ymax - ymin) * factor) / 2.0
    matrix = Matrix.translate(ox - xmin * factor, oy - ymin * factor) * Matrix.scale(factor)
    return [fragment(shape, matrix) for shape in elements(source)]


def _angle(value: str) -> "Angle":
    return Angle.parse(value)


def _face(style: TextStyle) -> "ImageFont.FreeTypeFont | ImageFont.ImageFont":
    # `Layout.RAQM` is gated on `features.check("raqm")` — a wheel built without libraqm silently falls back,
    # so the arm routes on the capability probe rather than assuming the feature.
    complex_ = bool(style.features or style.direction)
    engine = ImageFont.Layout.RAQM if complex_ and features.check("raqm") else ImageFont.Layout.BASIC
    face = ImageFont.truetype(style.font, style.size, layout_engine=engine) if style.font is not None else ImageFont.load_default(style.size)
    if style.variation and style.font is not None:
        face.set_variation_by_axes(list(style.variation))
    return face


def _gated_annotate(render_kwargs: dict[str, object], draws: Sequence[DrawOp], icc: bytes | None) -> bytes:
    # the resvg rasterize AND the pillow draw fold both run on the worker; the lazy proxies reify in the subprocess.
    image = Image.open(BytesIO(resvg_py.svg_to_bytes(**render_kwargs))).convert("RGBA")
    surface = ImageDraw.Draw(image)
    for op in draws:
        match op:
            case DrawOp(tag="text", text=(content, xy, style)):
                surface.multiline_text(
                    xy,
                    content,
                    font=_face(style),
                    fill=style.fill,
                    anchor=style.anchor,
                    align=style.align,
                    spacing=style.spacing,
                    direction=style.direction,
                    features=list(style.features) or None,
                    language=style.language,
                    stroke_width=style.stroke_width,
                    stroke_fill=style.stroke_fill,
                )
            case DrawOp(tag="caption", caption=(content, xy, style, pad, radius, box_fill, box_outline)):
                face = _face(style)
                left, top, right, bottom = surface.multiline_textbbox(
                    xy, content, font=face, align=style.align, spacing=style.spacing, stroke_width=style.stroke_width
                )
                surface.rounded_rectangle((left - pad, top - pad, right + pad, bottom + pad), radius=radius, fill=box_fill, outline=box_outline)
                surface.multiline_text(
                    xy,
                    content,
                    font=face,
                    fill=style.fill,
                    align=style.align,
                    spacing=style.spacing,
                    stroke_width=style.stroke_width,
                    stroke_fill=style.stroke_fill,
                )
            case DrawOp(tag="box", box=(box, outline, fill, width)):
                surface.rectangle(box, outline=outline, fill=fill, width=width)
            case DrawOp(tag="round_box", round_box=(box, radius, outline, fill, width)):
                surface.rounded_rectangle(box, radius=radius, outline=outline, fill=fill, width=width)
            case DrawOp(tag="line", line=(points, fill, width)):
                surface.line(points, fill=fill, width=width)
            case DrawOp(tag="ellipse", ellipse=(box, outline, fill, width)):
                surface.ellipse(box, outline=outline, fill=fill, width=width)
            case DrawOp(tag="arc", arc=(box, start, end, kind, fill, outline, width)):
                match kind:
                    case "arc":
                        surface.arc(box, start, end, fill=fill, width=width)
                    case "chord":
                        surface.chord(box, start, end, fill=fill, outline=outline, width=width)
                    case "pieslice":
                        surface.pieslice(box, start, end, fill=fill, outline=outline, width=width)
                    case _ as unreachable:
                        assert_never(unreachable)
            case DrawOp(tag="polygon", polygon=(points, fill, outline, width)):
                surface.polygon(points, fill=fill, outline=outline, width=width)
            case DrawOp(tag="regular", regular=(center, radius, sides, rotation, fill, outline, width)):
                surface.regular_polygon((center[0], center[1], radius), sides, rotation=int(rotation), fill=fill, outline=outline, width=width)
            case DrawOp(tag="stamp", stamp=(raw, xy, opacity)):
                mark = Image.open(BytesIO(raw)).convert("RGBA")
                if opacity < 1.0:
                    mark.putalpha(mark.getchannel("A").point(lambda level: int(level * opacity)))
                image.alpha_composite(mark, (int(xy[0]), int(xy[1])))
            case DrawOp(tag="blend", blend=(raw, mode)):
                overlay = Image.open(BytesIO(raw)).convert("RGBA")
                image = getattr(ImageChops, mode)(
                    image, overlay if overlay.size == image.size else overlay.resize(image.size, Image.Resampling.LANCZOS)
                )
                surface = ImageDraw.Draw(image)
            case DrawOp(tag="grade", grade=(size, table)):
                image = image.filter(ImageFilter.Color3DLUT(size, list(table)))
                surface = ImageDraw.Draw(image)
            case DrawOp(tag="frame", frame=(kind, radius, border, border_fill, fit, exif_orient)):
                image = ImageOps.exif_transpose(image) if exif_orient else image
                image = ImageOps.expand(image, border=border, fill=border_fill) if border else image
                image = ImageOps.fit(image, fit, method=Image.Resampling.LANCZOS) if fit is not None else image
                match kind:
                    case "unsharp":
                        image = image.filter(ImageFilter.UnsharpMask(radius=radius))
                    case "blur":
                        image = image.filter(ImageFilter.GaussianBlur(radius=radius))
                    case "median":
                        image = image.filter(ImageFilter.MedianFilter(size=max(3, int(radius) | 1)))
                    case "sharpness":
                        image = ImageEnhance.Sharpness(image).enhance(radius)
                    case "contrast":
                        image = ImageEnhance.Contrast(image).enhance(radius)
                    case "brightness":
                        image = ImageEnhance.Brightness(image).enhance(radius)
                    case "saturation":
                        image = ImageEnhance.Color(image).enhance(radius)
                    case "autocontrast":
                        image = ImageOps.autocontrast(image.convert("RGB"), cutoff=radius).convert("RGBA")
                    case "equalize":
                        image = ImageOps.equalize(image.convert("RGB")).convert("RGBA")
                    case "posterize":
                        image = ImageOps.posterize(image.convert("RGB"), min(8, max(1, int(radius)))).convert("RGBA")
                    case "solarize":
                        image = ImageOps.solarize(image.convert("RGB"), int(radius)).convert("RGBA")
                    case None:
                        pass
                    case _:
                        assert_never(kind)
                surface = ImageDraw.Draw(image)
            case _:
                assert_never(op)
    sink = BytesIO()
    # `icc_profile=` tags the PNG with its working-space profile (`None` is a no-op); the full source->dest
    # transform / soft-proof / CMYK separations are `graphic/color/managed#MANAGED`'s, composed outward.
    image.save(sink, format="PNG", icc_profile=icc)
    return sink.getvalue()


def _gated_metadata(payload: bytes, exif_tags: Sequence[tuple[int, str]], xmp: str | None) -> bytes:
    # the figure-egress-local in-worker PNG EXIF/XMP tag write — NOT the authoritative cross-format seal
    # `exchange/metadata#METADATA` owns, composed outward when a full IPTC/XMP/provenance write is the deliverable.
    image = Image.open(BytesIO(payload))
    exif = image.getexif()
    exif.update(exif_tags)
    packet = xmp if xmp is not None else image.info.get("XML:com.adobe.xmp")
    sink = BytesIO()
    image.save(sink, format=image.format or "PNG", exif=exif, xmp=packet.encode() if isinstance(packet, str) else packet)
    return sink.getvalue()
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
