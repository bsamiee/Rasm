# [PY_ARTIFACTS_COMPOSE]

`Figure` owns post-render figure placement — turning already-emitted SVG graphics into placed, annotated, color-correct figures. It scales-to-fit a viewport, tiles an n-up sheet, constraint-solves a free-form arrangement, crops, rotates, mattes, merges, overlays registration marks, rasterizes-then-annotates, and projects to a one-page vector PDF, discriminating a closed-payload `FigureOp` `tagged_union` by one total `match` — one typed payload per case, never a `StrEnum` over an erased `dict`. It places and finishes graphics already emitted by the chart/mark/table siblings; it re-renders no chart and holds no vector-geometry primitive, only the placement-specific arm bodies the geometry surface does not own.

## [01]-[INDEX]

## [02]-[COMPOSE]

- Owner: `Figure` discriminates over the closed `FigureOp` `expression.tagged_union`, one typed payload per case, never a `StrEnum` over a shared erased `dict`. `svgelements`'s algebra and `graphic/vector/region#REGION`'s `RegionOp`/`Fragment`/`applied` surface live once at those owners; this owner composes them one hop, holding only the placement arm bodies plus the `pillow` `Image` raster surface on the gated floor. `RenderPolicy` is REGION's rasterize-policy owner carried into `Annotate` so the resvg policy has one edit site across the chart/diagram/figure consumers. `TextStyle` is the one measured-text owner both `Text` and `Caption` compose, so a stroked complex-script variable-font caption and a bare label share one shape. `DrawOp` collapses the rejected `CaptionSpec`/`BoxSpec`/`PostSpec` triple into one pillow draw family folded by one total `match`, so a new annotation primitive is one case, never a parallel spec struct; its `frame` case carries ONE `Finish` policy value whose `_FILTERS` row interprets `amount` in its own unit, so filter growth is one table row and the fold never re-opens a nested filter roster. `RasterSource` collapses `svg_string` markup and an `.svg`/`.svgz` file path into one case whose `keywords()` projects the live `svg_to_bytes` source keyword, so a file source never grows a second render call. Each `MarkKind` member carries its own verified primitive name and float-only arg row resolved through the bound `MarkKind.shape` builder, so the overlay fold dispatches total over the member, never a per-call `dict[MarkKind, Callable]` over an erased shape bag. `Rule` is the closed constraint vocabulary the `Arrange` case and the exported `arranged` solve fold onto `kiwisolver` — hard rules required, aesthetics `weak`/`medium` — so an over-constrained layout is a typed refusal, never a hand-computed coordinate wall.
- Cases: dispatch is one total `match` over the closed `FigureOp` — never a sibling op per source media type, a parallel mark emitter per primitive, or a parallel figure class. `Crop` is a real `skia-pathops` `PathOp.INTERSECTION` geometry sever (REGION `clip`), not a CSS `<clipPath>` mask a downstream must honor; `Merge` and `Matte` compose REGION's `boolean`/`outline` for the planar set-op and the fixed-width offset keyline — the offset algebra `svgelements` cannot express, `skia-pathops` exposing no `Path.offset` (the stroke IS the offset), the matte framed UNDER the source. `Arrange` solves the `Rule` set over the sources' measured extents and aspect-fits each source into its solved box — the auto-layout arm that replaces caller-supplied coordinates with declared alignment, flow, and centering rules. `Pdf` is a pure `vl_convert.svg_to_pdf` projection of the placed flat `<svg>`, the post-edit-SVG-then-PDF path the `sheet`/`imposition` consumers draw, never a re-composition. `Annotate`'s trailing `icc` embeds the working-space profile on the PNG egress through `Image.save(icc_profile=)`; the full source->dest transform / soft-proof / CMYK separations ride `graphic/color/managed#MANAGED` outward, never re-owned here. `Metadata` is the figure-egress-local in-worker PNG EXIF/XMP tag convenience — NOT the authoritative cross-format seal `exchange/metadata#METADATA` owns, composed outward when a full descriptive-metadata write is the deliverable.
- Growth: a further vector layout op is one `FigureOp` case plus one `_compose_vector` arm plus one `_TRAIT` row over the imported surface; a new planar set-op is one `BooleanOp` member the `Merge` arm reads and a new offset cap/join one REGION `CapStyle`/`JoinStyle` at `outline`; an n-up refinement is one field on `Tile`; a layout constraint is one `Rule` case plus one `arranged` arm; a registration primitive is one `MarkKind` member through the shared builder; an annotation source mode one `RasterSource` case projecting one `svg_to_bytes` keyword; a raster primitive one `DrawOp` case, a finish filter one `_FILTERS` row, a blend one `BlendKind`, a text axis one `TextStyle` field. A resvg knob grows the imported `RenderPolicy` at its owner with zero edit here; a PDF-projection knob is one field on `Pdf`; a color-tagged egress is the `Annotate` `icc` field, the full ICC transform an outward `graphic/color/managed#MANAGED` compose; the authoritative cross-format metadata write an outward `exchange/metadata#METADATA` compose. Zero new surface.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import math
from collections.abc import Callable, Iterable, Sequence
from enum import Enum
from functools import partial
from io import BytesIO
from itertools import pairwise
from typing import TYPE_CHECKING, Annotated, Final, Literal, NoReturn, Self, assert_never

from concurrent.futures.process import BrokenProcessPool
from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, msgpack, structs

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import Metrics
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import FAULT_CONF, TRANSIENT, FaultRow, RuntimeRail, async_boundary, rostered

from rasm.artifacts.graphic.vector.path import Bounds, PathFault, Span, bounds, fragment, px, scene
from rasm.artifacts.graphic.vector.region import BooleanOp, Fragment, RegionFault, RegionOp, RegionResult, RenderPolicy, applied
from rasm.artifacts.core.hooks import BYTE_VOLUME, DOMAIN, ArtifactKind, ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.export.layered import Layer

lazy import kiwisolver
lazy import resvg_py
lazy import svgelements
lazy import vl_convert
lazy from PIL import Image, ImageChops, ImageDraw, ImageEnhance, ImageFilter, ImageFont, ImageOps, features
lazy from PIL.PngImagePlugin import PngInfo
lazy from svgelements import Angle, Matrix, Point, Shape

# --- [TYPES] ----------------------------------------------------------------------------
type Corner = Literal["nw", "ne", "sw", "se", "n", "s", "e", "w", "center"]
type Anchor = tuple[float, float]
type Axis = Literal["x", "y"]
type AlignEdge = Literal["left", "right", "top", "bottom", "cx", "cy"]
type Extent = Annotated[float, Is[lambda value: value > 0.0]]
type Columns = Annotated[int, Is[lambda count: count >= 1]]
type Sources = Annotated[int, Is[lambda count: count >= 1]]
type Fan = Annotated[tuple[tuple[float, float], ...], Is[lambda extents: len(extents) >= 1]]
type MarkArgs = Callable[[float, float, float], tuple[float, ...]]
type FilterKind = Literal[
    "unsharp", "blur", "median", "sharpness", "contrast", "brightness", "saturation", "autocontrast", "equalize", "posterize", "solarize"
]
type ArcKind = Literal["arc", "chord", "pieslice"]
type BlendKind = Literal["multiply", "screen", "overlay", "soft_light", "hard_light", "difference", "add", "subtract", "darker", "lighter"]


class MarkKind(Enum):
    CORNER = ("Polyline", lambda x, y, s: (x, y - s, x, y, x + s, y))
    TICK = ("SimpleLine", lambda x, y, s: (x - s, y, x + s, y))
    TARGET = ("Circle", lambda x, y, s: (x, y, s))
    REGISTRATION = ("Ellipse", lambda x, y, s: (x, y, s, s * 0.5))
    GUTTER = ("Polyline", lambda x, y, s: (x - s, y, x, y - s, x + s, y, x, y + s, x - s, y))
    MITER = ("Polygon", lambda x, y, s: (x - s, y, x, y - s, x + s, y, x, y + s))
    BLEED = ("Rect", lambda x, y, s: (x - s, y - s, 2.0 * s, 2.0 * s))
    CROSS = ("Polyline", lambda x, y, s: (x - s, y, x, y, x, y - s, x, y, x + s, y, x, y, x, y + s))
    STAR = (
        "Polyline",
        lambda x, y, s: (x, y - s, x, y, x + s, y - s, x, y, x + s, y, x, y, x + s, y + s, x, y, x, y + s, x, y, x - s, y + s, x, y, x - s, y, x, y,
                         x - s, y - s),
    )

    def __init__(self, primitive: str, args: MarkArgs) -> None:
        self.primitive = primitive
        self.args = args

    def shape(self, anchor: Anchor, size: float) -> "Shape":
        return getattr(svgelements, self.primitive)(*self.args(*anchor, size))


# --- [CONSTANTS] ------------------------------------------------------------------------
_FAULTS: tuple[type[Exception], ...] = (ValueError, OSError, BrokenProcessPool, BeartypeCallHintViolation)

_CANON: Final = msgpack.Encoder(order="deterministic")

_GUARD = beartype(conf=FAULT_CONF)

_TRAIT: Final[frozendict[str, KernelTrait]] = frozendict({
    "scale_fit": KernelTrait.INLINE,
    "tile": KernelTrait.HOSTILE,
    "arrange": KernelTrait.HOSTILE,
    "crop": KernelTrait.HOSTILE,
    "merge": KernelTrait.HOSTILE,
    "matte": KernelTrait.HOSTILE,
    "rotate": KernelTrait.INLINE,
    "overlay": KernelTrait.INLINE,
    "pdf": KernelTrait.RELEASING,
    "annotate": KernelTrait.HOSTILE,
    "metadata": KernelTrait.HOSTILE,
})


# --- [TABLES] ---------------------------------------------------------------------------

FIGURE_RENDER: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.COMPOSE, point="render", arm="boundary", defect="figure-fold", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([FIGURE_RENDER]))

# --- [MODELS] ---------------------------------------------------------------------------
class MarkSpec(Struct, frozen=True):
    kind: MarkKind
    corner: Corner = "nw"
    size: float = 12.0
    inset: float = 6.0
    stroke: str = "black"
    width: float = 0.5


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


_FILTERS: frozendict[FilterKind, Callable[["Image.Image", float], "Image.Image"]] = frozendict({
    "unsharp": lambda image, amount: image.filter(ImageFilter.UnsharpMask(radius=amount)),
    "blur": lambda image, amount: image.filter(ImageFilter.GaussianBlur(radius=amount)),
    "median": lambda image, amount: image.filter(ImageFilter.MedianFilter(size=max(3, int(amount) | 1))),
    "sharpness": lambda image, amount: ImageEnhance.Sharpness(image).enhance(amount),
    "contrast": lambda image, amount: ImageEnhance.Contrast(image).enhance(amount),
    "brightness": lambda image, amount: ImageEnhance.Brightness(image).enhance(amount),
    "saturation": lambda image, amount: ImageEnhance.Color(image).enhance(amount),
    "autocontrast": lambda image, amount: ImageOps.autocontrast(image.convert("RGB"), cutoff=amount).convert("RGBA"),
    "equalize": lambda image, _amount: ImageOps.equalize(image.convert("RGB")).convert("RGBA"),
    "posterize": lambda image, amount: ImageOps.posterize(image.convert("RGB"), min(8, max(1, int(amount)))).convert("RGBA"),
    "solarize": lambda image, amount: ImageOps.solarize(image.convert("RGB"), int(amount)).convert("RGBA"),
})


class Finish(Struct, frozen=True):
    filter: FilterKind | None = None
    amount: float = 2.0
    border: int = 0
    border_fill: str = "white"
    fit: tuple[int, int] | None = None
    exif_orient: bool = False

    def applied(self, image: "Image.Image") -> "Image.Image":
        staged = ImageOps.exif_transpose(image) if self.exif_orient else image
        staged = ImageOps.expand(staged, border=self.border, fill=self.border_fill) if self.border else staged
        staged = ImageOps.fit(staged, self.fit, method=Image.Resampling.LANCZOS) if self.fit is not None else staged
        return _FILTERS[self.filter](staged, self.amount) if self.filter is not None else staged


@tagged_union(frozen=True)
class Rule:
    tag: Literal["align", "chain", "pin", "center", "inside"] = tag()
    align: tuple[AlignEdge, tuple[int, ...]] = case()
    chain: tuple[Axis, tuple[int, ...], float] = case()
    pin: tuple[int, Anchor] = case()
    center: tuple[Axis, tuple[int, ...]] = case()
    inside: float = case()

    @staticmethod
    def Align(edge: AlignEdge, members: tuple[int, ...]) -> "Rule":
        return Rule(align=(edge, members))

    @staticmethod
    def Chain(axis: Axis, members: tuple[int, ...], gap: float = 12.0) -> "Rule":
        return Rule(chain=(axis, members, gap))

    @staticmethod
    def Pin(member: int, anchor: Anchor) -> "Rule":
        return Rule(pin=(member, anchor))

    @staticmethod
    def Center(axis: Axis, members: tuple[int, ...]) -> "Rule":
        return Rule(center=(axis, members))

    @staticmethod
    def Inside(margin: float) -> "Rule":
        return Rule(inside=margin)


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
    frame: Finish = case()

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
    def Frame(finish: Finish = Finish()) -> "DrawOp":
        return DrawOp(frame=finish)


@tagged_union(frozen=True)
class FigureOp:
    tag: Literal["scale_fit", "tile", "arrange", "crop", "merge", "matte", "rotate", "overlay", "pdf", "annotate", "metadata"] = tag()
    scale_fit: tuple[bytes, Span, Span] = case()
    tile: tuple[tuple[bytes, ...], int, Span, Span, float] = case()
    arrange: tuple[tuple[bytes, ...], Span, Span, tuple[Rule, ...]] = case()
    crop: tuple[bytes, Bounds] = case()
    merge: tuple[tuple[bytes, ...], BooleanOp] = case()
    matte: tuple[bytes, float, str] = case()
    rotate: tuple[bytes, str, Corner] = case()
    overlay: tuple[bytes, tuple[MarkSpec, ...]] = case()
    pdf: tuple[bytes, float] = case()
    annotate: tuple[RasterSource, RenderPolicy, tuple[DrawOp, ...], bytes | None] = (
        case()
    )
    metadata: tuple[bytes, tuple[tuple[int, str], ...], str | None] = case()

    @staticmethod
    def ScaleFit(source: bytes, width: Span, height: Span) -> "FigureOp":
        return FigureOp(scale_fit=(source, width, height))

    @staticmethod
    def Tile(sources: tuple[bytes, ...], columns: int, cell_width: Span, cell_height: Span, gutter: float = 0.0) -> "FigureOp":
        return FigureOp(tile=(sources, columns, cell_width, cell_height, gutter))

    @staticmethod
    def Arrange(sources: tuple[bytes, ...], width: Span, height: Span, rules: tuple[Rule, ...] = ()) -> "FigureOp":
        return FigureOp(arrange=(sources, width, height, rules))

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


class Placed(Struct, frozen=True):
    key: ContentKey
    data: bytes
    extent: Bounds = (0.0, 0.0, 0.0, 0.0)
    layers: tuple[Layer, ...] = ()


# --- [SERVICES] -------------------------------------------------------------------------
class Figure(Struct, frozen=True):
    op: FigureOp
    lane: LanePolicy
    placed: Option[Placed] = Nothing

    def emit(self, /) -> ArtifactWork[Placed]:
        key = self._key
        return ArtifactWork(key=key, work=partial(self._emit, key), parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"figure-{self.op.tag}", _CANON.encode(self.op))

    def rendered(self) -> Self:
        return structs.replace(self, placed=_placed(self.op))

    async def _emit(self, key: ContentKey, /) -> RuntimeRail[Placed]:
        match await async_boundary(FIGURE_RENDER, partial(self._rendered, key), catch=_FAULTS):
            case Result(tag="ok", ok=placed):
                kind: ArtifactKind = "document" if self.op.tag == "pdf" else "preview"
                Metrics.record({BYTE_VOLUME: float(len(placed.data))}, domain=DOMAIN, kind=kind, scope=self.lane.scope)
                return Ok(placed)
            case refused:
                return Error(refused.error)

    async def _rendered(self, key: ContentKey, /) -> Placed:
        match self.op:
            case FigureOp(tag="annotate", annotate=(source, render, draws, icc)):
                data, width, height = _out_of(
                    await self.lane.offload(Kernel.of(_gated_annotate, _TRAIT[self.op.tag]), render.kwargs(source.keywords()), draws, icc)
                )
                placed = Placed(key, data, (0.0, 0.0, float(width), float(height)))
            case FigureOp(tag="metadata", metadata=(source, exif, xmp)):
                data, width, height = _out_of(
                    await self.lane.offload(Kernel.of(_gated_metadata, _TRAIT[self.op.tag]), source, exif, xmp)
                )
                placed = Placed(key, data, (0.0, 0.0, float(width), float(height)))
            case _:
                placed = _out_of(await self.lane.offload(Kernel.of(_placed_now, _TRAIT[self.op.tag]), self.op))
        return placed

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return self.placed.map(lambda live: Layer.renamed(live.layers, names)).default_value(())


# --- [OPERATIONS] -----------------------------------------------------------------------
def _out_of[T](rail: "RuntimeRail[T]", /) -> T:
    return rail.default_with(_figure_raise)


def _figure_raise(fault: object) -> NoReturn:
    raise ValueError(str(fault))


def _placed(op: FigureOp) -> Option[Placed]:
    match op:
        case FigureOp(tag="annotate") | FigureOp(tag="metadata"):
            return Nothing
        case _:
            return Some(_placed_now(op))


def _placed_now(op: FigureOp) -> Placed:
    match op:
        case FigureOp(tag="pdf", pdf=(source, scale)):
            data = vl_convert.svg_to_pdf(source.decode(), scale=scale)
            key = ContentIdentity.key(f"figure-{op.tag}", _CANON.encode(op))
            return Placed(key, data)
        case _:
            data, layers = _compose_vector(op)
            extent = _extent(data)
            key = ContentIdentity.key(f"figure-{op.tag}", _CANON.encode(op))
            return Placed(key, data, extent, layers)


@_GUARD
def _fit(width: float, height: float, extent_w: Extent, extent_h: Extent, /) -> float:
    return min(width / extent_w, height / extent_h)


@_GUARD
def _rows(count: Sources, columns: Columns, /) -> int:
    return -(-count // columns)


def _compose_vector(op: FigureOp) -> tuple[bytes, tuple[Layer, ...]]:
    match op:
        case FigureOp(tag="scale_fit", scale_fit=(source, width, height)):
            (xmin, ymin, xmax, ymax), (tw, th) = _extent(source), (px(width), px(height))
            factor = _fit(tw, th, xmax - xmin, ymax - ymin)
            matrix = Matrix.translate(-xmin, -ymin) * Matrix.scale(factor)
            extent = (0.0, 0.0, tw, th)
            data = _region(RegionOp.Serialize((Fragment(path=fragment(shape, matrix)) for shape in _shapes(source)), extent))
            return data, (Layer("layer-0", data, extent),)
        case FigureOp(tag="tile", tile=(sources, columns, cell_width, cell_height, gutter)):
            cw, ch = px(cell_width), px(cell_height)
            rows = _rows(len(sources), columns)
            extent = (0.0, 0.0, cw * columns + gutter * (columns - 1), ch * rows + gutter * (rows - 1))
            cells = tuple(
                (
                    tuple(_place(raw, index, columns, cw, ch, gutter)),
                    (
                        index % columns * (cw + gutter),
                        index // columns * (ch + gutter),
                        index % columns * (cw + gutter) + cw,
                        index // columns * (ch + gutter) + ch,
                    ),
                )
                for index, raw in enumerate(sources)
            )
            data = _region(RegionOp.Serialize((fragment_ for fragments, _box in cells for fragment_ in fragments), extent))
            return data, tuple(
                Layer(f"layer-{index}", _region(RegionOp.Serialize(fragments, extent)), box) for index, (fragments, box) in enumerate(cells)
            )
        case FigureOp(tag="arrange", arrange=(sources, width, height, rules)):
            tw, th = px(width), px(height)
            extent = (0.0, 0.0, tw, th)
            boxes = arranged(tuple(_measured(raw) for raw in sources), (0.0, 0.0, tw, th), rules)
            cells = tuple((tuple(_fitted(raw, box)), box) for raw, box in zip(sources, boxes, strict=True))
            data = _region(RegionOp.Serialize((fragment_ for fragments, _box in cells for fragment_ in fragments), extent))
            return data, tuple(
                Layer(f"layer-{index}", _region(RegionOp.Serialize(fragments, extent)), box) for index, (fragments, box) in enumerate(cells)
            )
        case FigureOp(tag="crop", crop=(source, box)):
            data = _region(RegionOp.Clip(source, box))
            return data, (Layer("layer-0", data, _extent(data)),)
        case FigureOp(tag="merge", merge=(sources, set_op)):
            data = _region(RegionOp.Boolean(sources, set_op))
            return data, (Layer("layer-0", data, _extent(data)),)
        case FigureOp(tag="matte", matte=(source, width, stroke)):
            matte = _region(RegionOp.Outline(source, width))
            extent = _extent(matte)
            data = _region(
                RegionOp.Serialize(
                    (*(Fragment(filled=(fragment(shape), stroke)) for shape in _shapes(matte)), *(Fragment(path=fragment(shape)) for shape in _shapes(source))),
                    extent,
                )
            )
            return data, (Layer("layer-0", data, extent),)
        case FigureOp(tag="rotate", rotate=(source, angle, corner)):
            extent = _extent(source)
            ax, ay = _anchor(corner, extent, 0.0)
            pivot = Matrix.translate(ax, ay) * Matrix.rotate(_angle(angle)) * Matrix.translate(-ax, -ay)
            xmin, ymin, xmax, ymax = extent
            turned = [Point(x, y).matrix_transform(pivot) for x, y in ((xmin, ymin), (xmax, ymin), (xmax, ymax), (xmin, ymax))]
            frame = (min(p.x for p in turned), min(p.y for p in turned), max(p.x for p in turned), max(p.y for p in turned))
            data = _region(RegionOp.Serialize((Fragment(path=fragment(shape, pivot)) for shape in _shapes(source)), frame))
            return data, (Layer("layer-0", data, frame),)
        case FigureOp(tag="overlay", overlay=(source, marks)):
            extent = _extent(source)
            base = tuple(Fragment(path=fragment(shape)) for shape in _shapes(source))
            overlay = tuple(_marks(extent, marks))
            data = _region(RegionOp.Serialize((*base, *overlay), extent))
            return data, (
                Layer("base", _region(RegionOp.Serialize(base, extent)), extent),
                Layer("overlay", _region(RegionOp.Serialize(overlay, extent)), extent),
            )
        case _:
            raise ValueError(f"figure {op.tag} has no in-process vector composition")


def _extent(source: bytes) -> Bounds:
    return bounds(source).default_with(_source_fault)


def _shapes(source: bytes) -> tuple["Shape", ...]:
    return scene(source).default_with(_source_fault)


def _source_fault(fault: PathFault | RegionFault) -> "NoReturn":
    raise ValueError(f"figure source: {fault.tag}")


def _region(op: RegionOp) -> bytes:
    match applied(op).default_with(_source_fault):
        case RegionResult(tag="document", document=data):
            return data
        case result:
            raise ValueError(f"figure region: expected document, received {result.tag}")


def _measured(source: bytes) -> tuple[float, float]:
    xmin, ymin, xmax, ymax = _extent(source)
    return (xmax - xmin, ymax - ymin)


@_GUARD
def arranged(extents: Fan, region: Bounds, rules: tuple[Rule, ...]) -> tuple[Bounds, ...]:
    solver = kiwisolver.Solver()
    xs = tuple(kiwisolver.Variable(f"x{index}") for index in range(len(extents)))
    ys = tuple(kiwisolver.Variable(f"y{index}") for index in range(len(extents)))
    insides = tuple(rule.inside for rule in rules if rule.tag == "inside")
    if len(insides) > 1:
        raise ValueError(f"figure arrange: conflicting inside rules {insides}")
    margin = insides[0] if insides else 0.0
    if not math.isfinite(margin) or margin < 0.0:
        raise ValueError(f"figure arrange: inside margin {margin}")
    if bad_gaps := tuple(rule.chain[2] for rule in rules if rule.tag == "chain" and (not math.isfinite(rule.chain[2]) or rule.chain[2] < 0.0)):
        raise ValueError(f"figure arrange: chain gap {bad_gaps}")
    x0, y0, x1, y1 = region[0] + margin, region[1] + margin, region[2] - margin, region[3] - margin

    def _members(rule: Rule) -> tuple[int, ...]:
        match rule:
            case Rule(tag="align", align=(_, members)) | Rule(tag="chain", chain=(_, members, _)) | Rule(tag="center", center=(_, members)):
                return members
            case Rule(tag="pin", pin=(member, _)):
                return (member,)
            case _:
                return ()

    if rogue := tuple(member for rule in rules for member in _members(rule) if not 0 <= member < len(extents)):
        raise ValueError(f"figure arrange: rule member out of range {rogue}")
    try:
        for (width, height), x, y in zip(extents, xs, ys, strict=True):
            for bound in (x >= x0, y >= y0, x + width <= x1, y + height <= y1):
                solver.addConstraint(bound)
            solver.addConstraint((x == x0) | "weak")
            solver.addConstraint((y == y0) | "weak")
        for rule in rules:
            match rule:
                case Rule(tag="align", align=(edge, members)):
                    for member in members[1:]:
                        solver.addConstraint(_edge(edge, xs, ys, extents, members[0]) == _edge(edge, xs, ys, extents, member))
                case Rule(tag="chain", chain=(axis, members, gap)):
                    lead, span = (xs, 0) if axis == "x" else (ys, 1)
                    for former, latter in pairwise(members):
                        solver.addConstraint(lead[latter] >= lead[former] + extents[former][span] + gap)
                case Rule(tag="pin", pin=(member, anchor)):
                    solver.addConstraint(xs[member] == anchor[0])
                    solver.addConstraint(ys[member] == anchor[1])
                case Rule(tag="center", center=(axis, members)):
                    for member in members:
                        span = extents[member][0 if axis == "x" else 1]
                        lead = xs[member] if axis == "x" else ys[member]
                        mid = (x0 + x1) / 2.0 if axis == "x" else (y0 + y1) / 2.0
                        solver.addConstraint((lead + span / 2.0 == mid) | "medium")
                case Rule(tag="inside"):
                    pass
                case _ as unreachable:
                    assert_never(unreachable)
        solver.updateVariables()
    except (kiwisolver.UnsatisfiableConstraint, kiwisolver.DuplicateConstraint) as fault:
        raise ValueError(f"figure arrange: {type(fault).__name__}") from fault
    return tuple((x.value(), y.value(), x.value() + width, y.value() + height) for (width, height), x, y in zip(extents, xs, ys, strict=True))


def _edge(
    edge: AlignEdge,
    xs: "tuple[kiwisolver.Variable, ...]",
    ys: "tuple[kiwisolver.Variable, ...]",
    extents: tuple[tuple[float, float], ...],
    member: int,
) -> "kiwisolver.Variable | kiwisolver.Expression":
    width, height = extents[member]
    match edge:
        case "left":
            return xs[member]
        case "right":
            return xs[member] + width
        case "top":
            return ys[member]
        case "bottom":
            return ys[member] + height
        case "cx":
            return xs[member] + width / 2.0
        case "cy":
            return ys[member] + height / 2.0
        case _ as unreachable:
            assert_never(unreachable)


def _marks(extent: Bounds, marks: tuple[MarkSpec, ...]) -> list[Fragment]:
    return [
        Fragment(stroke=(fragment(spec.kind.shape(_anchor(spec.corner, extent, spec.inset), spec.size)), spec.stroke, spec.width))
        for spec in marks
    ]


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


def _fitted(source: bytes, box: Bounds) -> list[Fragment]:
    xmin, ymin, xmax, ymax = _extent(source)
    factor = _fit(box[2] - box[0], box[3] - box[1], xmax - xmin, ymax - ymin)
    ox = box[0] + (box[2] - box[0] - (xmax - xmin) * factor) / 2.0
    oy = box[1] + (box[3] - box[1] - (ymax - ymin) * factor) / 2.0
    matrix = Matrix.translate(ox - xmin * factor, oy - ymin * factor) * Matrix.scale(factor)
    return [Fragment(path=fragment(shape, matrix)) for shape in _shapes(source)]


def _place(source: bytes, index: int, columns: int, cell_w: float, cell_h: float, gutter: float) -> list[Fragment]:
    column, row = index % columns, index // columns
    x0, y0 = column * (cell_w + gutter), row * (cell_h + gutter)
    return _fitted(source, (x0, y0, x0 + cell_w, y0 + cell_h))


def _angle(value: str) -> "Angle":
    return Angle.parse(value)


def _face(style: TextStyle) -> "ImageFont.FreeTypeFont | ImageFont.ImageFont":
    complex_ = bool(style.features or style.direction)
    engine = ImageFont.Layout.RAQM if complex_ and features.check("raqm") else ImageFont.Layout.BASIC
    face = ImageFont.truetype(style.font, style.size, layout_engine=engine) if style.font is not None else ImageFont.load_default(style.size)
    if style.variation and style.font is not None:
        face.set_variation_by_axes(list(style.variation))
    return face


def _gated_annotate(render_kwargs: dict[str, object], draws: Sequence[DrawOp], icc: bytes | None) -> tuple[bytes, int, int]:
    with Image.open(BytesIO(resvg_py.svg_to_bytes(**render_kwargs))) as opened:
        image = opened.convert("RGBA")
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
                shaping = {
                    "anchor": style.anchor,
                    "direction": style.direction,
                    "features": list(style.features) or None,
                    "language": style.language,
                }
                left, top, right, bottom = surface.multiline_textbbox(
                    xy, content, font=face, align=style.align, spacing=style.spacing, stroke_width=style.stroke_width, **shaping
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
                    **shaping,
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
                with Image.open(BytesIO(raw)) as opened:
                    mark = opened.convert("RGBA")
                if opacity < 1.0:
                    mark.putalpha(mark.getchannel("A").point(lambda level: int(level * opacity)))
                image.alpha_composite(mark, (int(xy[0]), int(xy[1])))
            case DrawOp(tag="blend", blend=(raw, mode)):
                with Image.open(BytesIO(raw)) as opened:
                    overlay = opened.convert("RGBA")
                image = getattr(ImageChops, mode)(
                    image, overlay if overlay.size == image.size else overlay.resize(image.size, Image.Resampling.LANCZOS)
                )
                surface = ImageDraw.Draw(image)
            case DrawOp(tag="grade", grade=(size, table)):
                image = image.filter(ImageFilter.Color3DLUT(size, list(table)))
                surface = ImageDraw.Draw(image)
            case DrawOp(tag="frame", frame=finish):
                image = finish.applied(image)
                surface = ImageDraw.Draw(image)
            case _:
                assert_never(op)
    sink = BytesIO()
    image.save(sink, format="PNG", icc_profile=icc)
    return sink.getvalue(), image.width, image.height


def _gated_metadata(payload: bytes, exif_tags: Sequence[tuple[int, str]], xmp: str | None) -> tuple[bytes, int, int]:
    with Image.open(BytesIO(payload)) as image:
        exif = image.getexif()
        exif.update(exif_tags)
        packet = xmp if xmp is not None else image.info.get("XML:com.adobe.xmp")
        info = PngInfo()
        if packet is not None:
            info.add_itxt("XML:com.adobe.xmp", packet)
        sink = BytesIO()
        image.save(sink, format="PNG", exif=exif, pnginfo=info, icc_profile=image.info.get("icc_profile"))
        return sink.getvalue(), image.width, image.height


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "DrawOp",
    "Figure",
    "FigureOp",
    "Finish",
    "MarkKind",
    "MarkSpec",
    "Placed",
    "RasterSource",
    "Rule",
    "TextStyle",
    "arranged",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
