# [PY_ARTIFACTS_DRAWING_SYMBOL]

The AEC drawing-symbol vocabulary and the deep graphic owner populating the STUB graphic-cell bytes `composition/sheet#SHEET` leaves empty. `Symbol` is ONE owner over a closed `SymbolKind` union — `Section`/`Elevation`/`Detail`/`Grid`/`MatchLine`/`North`/`ScaleBar`/`Revision`/`KeyPlan`/`Datum`/`BreakLine` — each case carrying its typed geometry plus one `SymbolStyle`, dispatched by one total `match` and dual-lowered over the `SymbolTarget` policy value into a `drawsvg` named-layer group or an `ezdxf` reusable block. A new AEC marker is one case plus one geometry arm, never a per-marker class family and never an erased attribute `dict`.

`SymbolStyle` is the ONE drawing-plane mark-style owner composing `drawing/regime#REGIME`'s `LayerName`/`LineWeight`/`TextHeight`/`Terminator` with `fill`/`stroke` indexing the `hex_ramp` ramp `visualization/chart/spec#CHART` declares once — the ISO-grounded peer of the diagram-plane `visualization/diagram/glyphset#GLYPHSET` `GlyphStyle`, the mark-style owner the sibling `drawing/annotate` and `drawing/detail` producers compose rather than a parallel `AnnotateStyle`. Compound geometry is `schemdraw` `ElementCompound` for all eleven marks; the DXF arm authors one block per geometry signature onto a `drawing/standard#STANDARD` `Standard.seed` document, so a section marker, a grid bubble, and a north arrow read as one grammar across the vector-figure and CAD egress. The synchronous render offloads on the runtime thread lane and rails through `RuntimeRail`/`async_boundary`; `SymbolStyle.layer.compose()` buckets marks for `export/layered#LAYERED`; and the `glyph` PNG projection feeds `composition/sheet#SHEET`'s `NorthArrow.glyph`/`KeyPlan.figure` cells through `graphic/vector/region#REGION` `rasterize` — the raster the sheet cell's `reportlab` `ImageReader` reads. The owner contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Drawing` and one `core/plan#PLAN` `ArtifactWork` node.

## [01]-[INDEX]

- [01]-[SYMBOL]: the closed `SymbolKind` marker union dual-lowered over `SymbolTarget` into a `drawsvg` named-layer group or an `ezdxf` reusable block.

## [02]-[SYMBOL]

- Owner: `Symbol` holds `marks`, the `Palette`, and the `SymbolTarget` egress policy (the DXF arm seeds a default `Standard.of()` document, the SVG arm needs no profile); it discriminates over the closed `SymbolKind` union whose every case carries its typed geometry plus one `SymbolStyle` — never a per-marker `SectionMarker`/`GridBubble` family, never a `StrEnum` over an erased `dict[str, object]`. `SymbolStyle` is the ONE drawing-plane mark-style `Struct`, the ISO-grounded peer of `glyphset#GLYPHSET`'s `GlyphStyle`, whose `LayerName.compose()` binds both the named `drawsvg.Group` and the `ezdxf` `GfxAttribs`. `SymbolTarget` keys the `_ENGINES` dual-lowering table straight to its engine callable so a new egress is one row, never a per-target subtype.
- Cases: the eleven marker cases, each carrying its typed geometry, one `SymbolStyle`, and (for the bubble/pointer/grid/match marks) a named `self.anchors` terminal — `cut`/`point`/`leader`/`attach`/`match`/`north`/`level` — a `drawing/detail#CALLOUT`/`drawing/annotate` leader binds; matched by one total `_element` fold closed by `assert_never`, never a per-marker special case. `ScaleBar` is the standalone twin of the `composition/sheet#SHEET` `Scale.bar` geometry; `KeyPlan` is the reference figure `sheet`'s `KeyPlan.figure` cell consumes.
- Entry: `Symbol.over` normalizes `SymbolKind | Iterable[SymbolKind]` into `marks` by a structural head-`match` (a lone mark the singleton, an iterable the multi-mark sheet), never a `batch` knob; `render`/`layered` are `async` over `async_boundary`, offloading the whole synchronous fold onto the runtime thread lane. `_svg_engine` runs the grid solve, folds each mark to a self-contained `<svg>`, buckets by `SymbolStyle.layer.compose()` into a `drawsvg.Group`, and emits one per-layer `Layer` row each; `_dxf_engine` mints ONE `doc.blocks.new` per distinct `(tag, extent)` signature placed by N `add_blockref` under `Standard.graphics(layer)` attributes. `glyph` renders ONE mark's SVG then rasterizes to PNG through `region` `rasterize` for a sheet cell whose `ImageReader` reads a raster, never an SVG or a second geometry fold.
- Auto: `SymbolStyle.fill`/`stroke` index the `hex_ramp` ramp resolved once per render, so a recolor is a palette swap; `Standard.graphics(layer)` projects the same `LayerName` into the `ezdxf` `GfxAttribs` so the SVG group and the DXF layer carry one discipline pen. The `North` south-half and the `Revision`/`MatchLine`/`Datum` captions are all DRAWN geometry, never a promised-but-dropped part; the DXF block marks mirror the SVG fills through `add_hatch().set_solid_fill()` over a closed boundary path (`add_lwpolyline` alone left the north half and datum triangle outline-only) at cross-egress parity. Each collinear grid run threads its OWN `kiwisolver.Solver` — endpoints `required`, given positions `weak`, an even gap at `medium`, min-separation `weak`, the `strength` bands separating the hard endpoint snap from aesthetic spacing — and `_grid_runs` partitions a two-axis structural grid into independent X/Y runs so both axes never collapse onto one diagonal; `updateVariables()` writes each solved `value()` back into the re-keyed anchor before `_element` reads it.
- Packages: `schemdraw` owns all compound geometry (`ElementCompound` + `Segment*` + the named `self.anchors` terminals, rendered font-independent through `use('svg')`/`svgconfig.text='path'` — the bundled `ziafont` outlines every `SegmentText` to a `<path>`); `drawsvg` the named-layer `Group` container; `ezdxf` the DXF block model, its `add_hatch().set_solid_fill()` bringing the north half and datum triangle to SVG fill parity; `kiwisolver` the per-run collinear/two-axis alignment solve; `graphic/vector/region#REGION` `rasterize` the SVG→PNG raster; `drawing/regime#REGIME` and `drawing/standard#STANDARD` the ISO vocabulary and the `ezdxf` lowering; `graphic/color/derive#DERIVE` the `hex_ramp` ramp. The runtime rails, `ArtifactReceipt.Drawing`, and `export/layered#LAYERED` `Layer` compose silently.
- Growth: a new AEC marker is one `SymbolKind` case plus one `_element` arm and one `_dxf_block` arm — the five compound primitives cover the geometry; a new egress one `SymbolTarget` member plus one `_ENGINES` row; a new visual axis one `SymbolStyle` field; a new named terminal one `self.anchors` key; a new alignment axis one `_grid_runs` band plus one `kiwisolver` constraint at its `strength` band; a new line-end one `Terminator` member plus one `_ARROW` row; a new receipt fact one scalar the shared `ArtifactReceipt.Drawing` carries.
- Boundary: no sheet-set, dimension, or annotation logic (`composition/sheet#SHEET`/`drawing/dimension#DIMENSION`/`drawing/annotate#ANNOTATE`); no IFC semantics (`csharp:Rasm.Bim`); identity minting is the runtime's. `graphic/vector/region#REGION` owns the SVG↔raster and the landed `boolean`/`outline` a filled-band match line or unioned north silhouette composes; `export/layered#LAYERED` owns the layer binding; `composition/sheet#SHEET` owns the cell placement.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import math
import os
from collections.abc import Callable, Iterable
from enum import StrEnum
from itertools import pairwise
from typing import Literal, Self, assert_never

from expression import Result, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.drawing.regime import LayerName, LineWeight, Terminator, TextHeight
from artifacts.drawing.standard import Standard
from artifacts.graphic.layer import LayerContent, LayerIntent, LayerNode, LayerPlan, NamingSchema
from artifacts.graphic.vector.region import RegionFault, RenderPolicy, rasterize
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp

# each proxy reifies on first render-arm use in the offloaded worker
lazy import drawsvg
lazy import ezdxf
lazy import kiwisolver
lazy from schemdraw import Drawing as Schematic, elements, segments, svgconfig, use

# --- [TYPES] ----------------------------------------------------------------------------
type Point = tuple[float, float]
type Box = tuple[float, float, float, float]
type SymbolTag = Literal["section", "elevation", "detail", "grid", "matchline", "north", "scale_bar", "revision", "keyplan", "datum", "breakline"]

_MIN_GAP: float = 0.02  # minimum t-separation keeping a dense grid-bubble run monotone and non-overlapping
_GLYPH: RenderPolicy = RenderPolicy(dpi=300.0)  # the sheet-cell PNG raster policy region rasterize reads


class SymbolTarget(StrEnum):  # the dual-lowering egress — a new target is one `_ENGINES` row, never a subtype
    SVG = "svg"  # drawsvg named-layer `<g>` groups the export/layered OCG owner binds
    DXF = "dxf"  # ezdxf reusable-block document the CAD rail reads


# --- [MODELS] ---------------------------------------------------------------------------
class SymbolStyle(Struct, frozen=True):
    # the ONE drawing-plane mark-style owner: fill/stroke index the hex_ramp ramp, layer/weight/text_height/terminator compose regime's ISO vocabulary.
    layer: LayerName
    fill: int = 0
    stroke: int = 0
    weight: LineWeight = LineWeight.W025  # ISO 128 line-weight
    text_height: TextHeight = TextHeight.H2_5  # ISO 3098 lettering height
    terminator: Terminator = Terminator.FILLED_ARROW  # ISO 129-1 line-end the section/elevation tails draw


# --- [VOCABULARY] -----------------------------------------------------------------------
@tagged_union(frozen=True)
class SymbolKind:
    tag: SymbolTag = tag()
    section: tuple[Point, float, str, str, float, SymbolStyle] = case()
    elevation: tuple[Point, float, str, str, float, SymbolStyle] = case()
    detail: tuple[Point, float, str, str, SymbolStyle] = case()
    grid: tuple[Point, float, str, SymbolStyle] = case()
    matchline: tuple[tuple[Point, ...], str, SymbolStyle] = case()
    north: tuple[Point, float, float, SymbolStyle] = case()
    scale_bar: tuple[Point, float, int, str, str, SymbolStyle] = case()
    revision: tuple[Point, float, str, SymbolStyle] = case()
    keyplan: tuple[Point, tuple[float, float], tuple[Box, ...], int, SymbolStyle] = case()
    datum: tuple[Point, float, str, SymbolStyle] = case()
    breakline: tuple[tuple[Point, ...], SymbolStyle] = case()

    @staticmethod
    def Section(center: Point, radius: float, detail_no: str, sheet_ref: str, bearing: float, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(section=(center, radius, detail_no, sheet_ref, bearing, style))

    @staticmethod
    def Elevation(center: Point, radius: float, elev_no: str, sheet_ref: str, angle: float, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(elevation=(center, radius, elev_no, sheet_ref, angle, style))

    @staticmethod
    def Detail(center: Point, radius: float, detail_no: str, sheet_ref: str, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(detail=(center, radius, detail_no, sheet_ref, style))

    @staticmethod
    def Grid(anchor: Point, radius: float, label: str, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(grid=(anchor, radius, label, style))

    @staticmethod
    def MatchLine(vertices: tuple[Point, ...], sheet_ref: str, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(matchline=(vertices, sheet_ref, style))

    @staticmethod
    def North(center: Point, radius: float, bearing: float, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(north=(center, radius, bearing, style))

    @staticmethod
    def ScaleBar(origin: Point, length: float, segments: int, units: str, ratio: str, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(scale_bar=(origin, length, segments, units, ratio, style))

    @staticmethod
    def Revision(center: Point, size: float, mark: str, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(revision=(center, size, mark, style))

    @staticmethod
    def KeyPlan(origin: Point, extent: tuple[float, float], parcels: tuple[Box, ...], highlight: int, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(keyplan=(origin, extent, parcels, highlight, style))

    @staticmethod
    def Datum(anchor: Point, size: float, level: str, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(datum=(anchor, size, level, style))

    @staticmethod
    def BreakLine(vertices: tuple[Point, ...], style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(breakline=(vertices, style))


# --- [SERVICES] -------------------------------------------------------------------------
class Symbol(Struct, frozen=True):
    marks: tuple[SymbolKind, ...]
    palette: Palette
    target: SymbolTarget = SymbolTarget.SVG

    @classmethod
    def over(cls, marks: SymbolKind | Iterable[SymbolKind], palette: Palette, /, *, target: SymbolTarget = SymbolTarget.SVG) -> Self:
        match marks:  # modal-arity head: a lone mark the singleton, an iterable the multi-mark sheet
            case SymbolKind():
                return cls(marks=(marks,), palette=palette, target=target)
            case _:
                return cls(marks=tuple(marks), palette=palette, target=target)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.marks)))

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: minted PRE-RUN, never over rendered bytes.
        return ContentIdentity.of(f"drawing-symbol-{self.target}", (self.marks, self.palette, self.target), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the receipt threads the PRE-RUN key; the layer payload is the layered() projection.
        return (await async_boundary(f"drawing.symbol.{self.target}", self._crossed)).map(lambda pair: pair[1])

    async def layered(self) -> RuntimeRail[LayerPlan]:
        # the engine rows as ONE LayerPlan tree — substrate the layered/sheet consumers compose as parents.
        return (await async_boundary(f"drawing.symbol.{self.target}", self._crossed)).map(
            lambda pair: LayerPlan(schema=NamingSchema.ISO13567, roots=pair[0])
        )

    async def _crossed(self) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
        # synchronous fold — crosses the runtime thread lane.
        crossed = await LanePolicy.offload(_ENGINES[self.target], self, modality=Modality.THREAD, retry=RetryClass.OCCT)
        return crossed.default_with(lambda fault: _fold_raise(fault))

    async def glyph(self, mark: SymbolKind, /) -> RuntimeRail[bytes]:
        # the symbol->sheet seam: ONE mark rasterized to PNG for a sheet NorthArrow.glyph/KeyPlan.figure cell
        # whose reportlab ImageReader reads a raster, never an SVG; the palette crosses the offload raw.
        return await LanePolicy.offload(_raster, mark, self.palette, modality=Modality.THREAD, retry=RetryClass.OCCT)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _fold_raise(fault: object) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    # terminal collapse: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))


def _row(*, name: str, source: bytes, bbox: tuple[float, float, float, float] | None = None, group: str | None = None) -> LayerNode:
    # lowers one engine row into the graphic/layer vocabulary: a group name nests as the LayerPlan path prefix, z rides row order.
    return LayerNode(name=name if group is None else f"{group}/{name}", intent=LayerIntent.ANNOTATION, content=Some(LayerContent(fragment=source)))


def _style(mark: SymbolKind, /) -> SymbolStyle:
    match mark:  # every case's style is its last payload slot; one total projection, never a per-tag getattr
        case (
            SymbolKind(tag="section", section=(*_, style))
            | SymbolKind(tag="elevation", elevation=(*_, style))
            | SymbolKind(tag="detail", detail=(*_, style))
            | SymbolKind(tag="grid", grid=(*_, style))
            | SymbolKind(tag="matchline", matchline=(*_, style))
            | SymbolKind(tag="north", north=(*_, style))
            | SymbolKind(tag="scale_bar", scale_bar=(*_, style))
            | SymbolKind(tag="revision", revision=(*_, style))
            | SymbolKind(tag="keyplan", keyplan=(*_, style))
            | SymbolKind(tag="datum", datum=(*_, style))
            | SymbolKind(tag="breakline", breakline=(*_, style))
        ):
            return style
        case _ as unreachable:
            assert_never(unreachable)


def _center(mark: SymbolKind, /) -> Point:
    match mark:  # each case's placement centre is its first payload slot; the polyline marks anchor at vertices[0]
        case SymbolKind(tag="matchline", matchline=(vertices, *_)) | SymbolKind(tag="breakline", breakline=(vertices, *_)):
            return vertices[0]
        case (
            SymbolKind(tag="section", section=(center, *_))
            | SymbolKind(tag="elevation", elevation=(center, *_))
            | SymbolKind(tag="detail", detail=(center, *_))
            | SymbolKind(tag="grid", grid=(center, *_))
            | SymbolKind(tag="north", north=(center, *_))
            | SymbolKind(tag="scale_bar", scale_bar=(center, *_))
            | SymbolKind(tag="revision", revision=(center, *_))
            | SymbolKind(tag="keyplan", keyplan=(center, *_))
            | SymbolKind(tag="datum", datum=(center, *_))
        ):
            return center
        case _ as unreachable:
            assert_never(unreachable)


def _extent(mark: SymbolKind, /) -> float:
    match mark:  # the mark's characteristic half-extent, the bbox and the DXF block signature read
        case (
            SymbolKind(tag="section", section=(_, radius, *_))
            | SymbolKind(tag="elevation", elevation=(_, radius, *_))
            | SymbolKind(tag="detail", detail=(_, radius, *_))
            | SymbolKind(tag="grid", grid=(_, radius, *_))
            | SymbolKind(tag="north", north=(_, radius, *_))
        ):
            return radius * 2.0
        case SymbolKind(tag="revision", revision=(_, size, *_)) | SymbolKind(tag="datum", datum=(_, size, *_)):
            return size * 1.5
        case SymbolKind(tag="scale_bar", scale_bar=(_, length, *_)):
            return length
        case SymbolKind(tag="matchline", matchline=(vertices, *_)) | SymbolKind(tag="breakline", breakline=(vertices, *_)):
            return max((math.dist(vertices[0], vertex) for vertex in vertices), default=1.0)
        case SymbolKind(tag="keyplan", keyplan=(_, extent, *_)):
            return max(extent)
        case _ as unreachable:
            assert_never(unreachable)


def _element(mark: SymbolKind, ramp: list[str]) -> tuple["elements.ElementCompound", Point]:
    # the schemdraw compound geometry with NAMED terminal anchors a detail/annotate leader binds. TOTAL over the eleven-case family.
    match mark:
        case SymbolKind(tag="section", section=(center, radius, detail_no, sheet_ref, bearing, style)):
            return _section_element(radius, detail_no, sheet_ref, bearing, style, ramp), center
        case SymbolKind(tag="elevation", elevation=(center, radius, elev_no, sheet_ref, angle, style)):
            return _elevation_element(radius, elev_no, sheet_ref, angle, style, ramp), center
        case SymbolKind(tag="detail", detail=(center, radius, detail_no, sheet_ref, style)):
            return _bubble_element(radius, detail_no, sheet_ref, style, ramp), center
        case SymbolKind(tag="grid", grid=(anchor, radius, label, style)):
            return _grid_element(radius, label, style, ramp), anchor
        case SymbolKind(tag="matchline", matchline=(vertices, sheet_ref, style)):
            return _matchline_element(vertices, sheet_ref, style, ramp), vertices[0]
        case SymbolKind(tag="north", north=(center, radius, bearing, style)):
            return _north_element(radius, bearing, style, ramp), center
        case SymbolKind(tag="scale_bar", scale_bar=(origin, length, seg, units, ratio, style)):
            return _scale_element(length, seg, units, ratio, style, ramp), origin
        case SymbolKind(tag="revision", revision=(center, size, mark_no, style)):
            return _revision_element(size, mark_no, style, ramp), center
        case SymbolKind(tag="keyplan", keyplan=(origin, extent, parcels, highlight, style)):
            return _keyplan_element(extent, parcels, highlight, style, ramp), origin
        case SymbolKind(tag="datum", datum=(anchor, size, level, style)):
            return _datum_element(size, level, style, ramp), anchor
        case SymbolKind(tag="breakline", breakline=(vertices, style)):
            return _breakline_element(vertices, style, ramp), vertices[0]
        case _ as unreachable:
            assert_never(unreachable)


def _pen(style: SymbolStyle, ramp: list[str], /) -> tuple[str, str, float, float]:
    # the palette-and-ISO pen every builder splats: (stroke hex, fill hex, ISO-128 line weight, ISO-3098 mm)
    return ramp[style.stroke % len(ramp)], ramp[style.fill % len(ramp)], float(style.weight.value), style.text_height.mm


def _bisected_bubble(radius: float, upper: str, lower: str, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    # the shared section/detail bubble — the arms differ only in tail and named anchor, so the core is one builder.
    sym, (stroke, _fill, lw, size) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(segments.SegmentCircle((0.0, 0.0), radius, color=stroke, lw=lw))
    sym.segments.append(segments.Segment([(-radius, 0.0), (radius, 0.0)], color=stroke, lw=lw))
    sym.segments.append(segments.SegmentText((0.0, radius * 0.4), upper, align=("center", "center"), fontsize=size, color=stroke))
    sym.segments.append(segments.SegmentText((0.0, -radius * 0.4), lower, align=("center", "center"), fontsize=size, color=stroke))
    return sym


def _section_element(
    radius: float, detail_no: str, sheet_ref: str, bearing: float, style: SymbolStyle, ramp: list[str]
) -> "elements.ElementCompound":
    sym, (stroke, _fill, lw, _size) = _bisected_bubble(radius, detail_no, sheet_ref, style, ramp), _pen(style, ramp)
    tip = (radius * 2.0 * math.cos(math.radians(bearing)), radius * 2.0 * math.sin(math.radians(bearing)))
    sym.segments.append(segments.Segment([(0.0, 0.0), tip], color=stroke, lw=lw, arrow=_ARROW[style.terminator]))
    sym.anchors["cut"] = tip  # the named terminal a detail cross-reference binds
    return sym


def _elevation_element(radius: float, elev_no: str, sheet_ref: str, angle: float, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    sym, (stroke, fill, lw, size) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(segments.SegmentCircle((0.0, 0.0), radius, color=stroke, lw=lw))
    point = (radius * 1.8 * math.cos(math.radians(angle)), radius * 1.8 * math.sin(math.radians(angle)))
    flank = radius * 0.5
    sym.segments.append(
        segments.SegmentPoly([(0.0, 0.0), (point[0] - flank, point[1]), (point[0] + flank, point[1])], closed=True, color=stroke, fill=fill)
    )
    sym.segments.append(segments.SegmentText((0.0, radius * 0.4), elev_no, align=("center", "center"), fontsize=size, color=stroke))
    sym.segments.append(segments.SegmentText((0.0, -radius * 0.4), sheet_ref, align=("center", "center"), fontsize=size, color=stroke))
    sym.anchors["point"] = point
    return sym


def _bubble_element(radius: float, detail_no: str, sheet_ref: str, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    sym = _bisected_bubble(radius, detail_no, sheet_ref, style, ramp)
    sym.anchors["leader"] = (radius, 0.0)  # the terminal a keynote/detail leader lands on
    return sym


def _grid_element(radius: float, label: str, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    sym, (stroke, _fill, lw, size) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(segments.SegmentCircle((0.0, 0.0), radius, color=stroke, lw=lw))
    sym.segments.append(segments.SegmentText((0.0, 0.0), label, align=("center", "center"), fontsize=size, color=stroke))
    sym.anchors["attach"] = (0.0, -radius)  # the grid line meets the bubble here
    return sym


def _matchline_element(vertices: tuple[Point, ...], sheet_ref: str, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    sym, (stroke, _fill, lw, size) = elements.ElementCompound(), _pen(style, ramp)
    origin, path = vertices[0], [(vx - vertices[0][0], vy - vertices[0][1]) for vx, vy in vertices]
    sym.segments.append(segments.Segment(path, color=stroke, lw=lw * 4.0, ls="--"))
    sym.segments.append(
        segments.SegmentText(path[len(path) // 2], f"MATCH LINE — SEE {sheet_ref}", align=("center", "bottom"), fontsize=size, color=stroke)
    )
    sym.anchors["match"] = path[-1]
    return sym


def _north_element(radius: float, bearing: float, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    # both halves and the "N" all drawn geometry, never a dropped part.
    sym, (stroke, fill, _lw, size) = elements.ElementCompound(), _pen(style, ramp)
    rot, base = math.radians(bearing), radius * 0.42
    tip, tail = _rotate((0.0, radius), rot), _rotate((0.0, -radius * 0.25), rot)
    left, right = _rotate((-base, -radius * 0.55), rot), _rotate((base, -radius * 0.55), rot)
    sym.segments.append(segments.SegmentPoly([tip, left, tail], closed=True, color=stroke, fill=fill))  # filled north half
    sym.segments.append(segments.SegmentPoly([tip, right, tail], closed=True, color=stroke))  # hollow south half
    sym.segments.append(segments.SegmentText(_rotate((0.0, radius * 1.25), rot), "N", align=("center", "center"), fontsize=size, color=stroke))
    sym.anchors["north"] = tip
    return sym


def _scale_element(length: float, seg: int, units: str, ratio: str, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    sym, (stroke, fill, lw, size) = elements.ElementCompound(), _pen(style, ramp)
    step, height = length / max(seg, 1), size
    for i in range(
        max(seg, 1)
    ):  # Exemption: schemdraw appends Segment* to the mutable self.segments; the ruler assembles in place
        sym.segments.append(
            segments.SegmentPoly(
                [(i * step, 0.0), ((i + 1) * step, 0.0), ((i + 1) * step, height), (i * step, height)],
                closed=True,
                color=stroke,
                lw=lw,
                fill=fill if i % 2 else None,
            )
        )
    sym.segments.append(
        segments.SegmentText((length / 2.0, -height), f"0 — {length:g} {units} ({ratio})", align=("center", "top"), fontsize=size, color=stroke)
    )
    return sym


def _revision_element(size_: float, mark: str, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    sym, (stroke, _fill, lw, text) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(
        segments.SegmentPoly([(0.0, size_), (-size_ * 0.87, -size_ * 0.5), (size_ * 0.87, -size_ * 0.5)], closed=True, color=stroke, lw=lw)
    )
    sym.segments.append(
        segments.SegmentText((0.0, -size_ * 0.05), mark, align=("center", "center"), fontsize=text, color=stroke)
    )  # the revision number
    return sym


def _keyplan_element(
    extent: tuple[float, float], parcels: tuple[Box, ...], highlight: int, style: SymbolStyle, ramp: list[str]
) -> "elements.ElementCompound":
    sym, (stroke, fill, lw, _size) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(
        segments.SegmentPoly([(0.0, 0.0), (extent[0], 0.0), (extent[0], extent[1]), (0.0, extent[1])], closed=True, color=stroke, lw=lw)
    )
    for index, (x0, y0, x1, y1) in enumerate(parcels):  # Exemption: the parcel rectangles assemble onto the mutable self.segments
        sym.segments.append(
            segments.SegmentPoly([(x0, y0), (x1, y0), (x1, y1), (x0, y1)], closed=True, color=stroke, fill=fill if index == highlight else None)
        )
    return sym


def _datum_element(size_: float, level: str, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    # the ISO spot-level marker.
    sym, (stroke, fill, lw, text) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(segments.SegmentPoly([(0.0, 0.0), (-size_ * 0.5, size_), (size_ * 0.5, size_)], closed=True, color=stroke, fill=fill))
    sym.segments.append(segments.SegmentText((size_ * 0.8, size_ * 0.6), level, align=("left", "center"), fontsize=text, color=stroke))
    sym.anchors["level"] = (0.0, 0.0)
    return sym


def _breakline_element(vertices: tuple[Point, ...], style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    # the ISO 128 break line — a midpoint zigzag marking a truncated view.
    sym, (stroke, _fill, lw, _size) = elements.ElementCompound(), _pen(style, ramp)
    origin = vertices[0]
    rel = [(vx - origin[0], vy - origin[1]) for vx, vy in vertices]
    mid = rel[len(rel) // 2]
    zig = [(mid[0] - 2.0, mid[1]), (mid[0] - 0.5, mid[1] + 3.0), (mid[0] + 0.5, mid[1] - 3.0), (mid[0] + 2.0, mid[1])]
    sym.segments.append(segments.Segment([*rel[: len(rel) // 2], *zig, *rel[len(rel) // 2 :]], color=stroke, lw=lw))
    return sym


def _rotate(point: Point, radians: float) -> Point:  # rotate about the compound origin; the mark places at its centre
    return (point[0] * math.cos(radians) - point[1] * math.sin(radians), point[0] * math.sin(radians) + point[1] * math.cos(radians))


def _svg_mark(mark: SymbolKind, ramp: list[str]) -> bytes:
    # ONE self-contained SVG mark: schemdraw renders font-independent via use('svg') + svgconfig.text='path' (ziafont outlines each SegmentText).
    use("svg")
    svgconfig.text = "path"
    element, center = _element(mark, ramp)
    with Schematic(show=False) as sheet:  # Exemption: schemdraw Drawing is a context manager finalizing the layout on __exit__
        sheet += element.at(center)
    return sheet.get_imagedata("svg")


def _raster(mark: SymbolKind, palette: Palette) -> Result[bytes, RegionFault]:
    # the sheet-cell seam: render ONE mark to SVG then rasterize to PNG through region; hex_ramp runs here in the worker, never on the loop.
    return rasterize(_svg_mark(mark, hex_ramp(palette)), _GLYPH)


def _grid_runs(indices: tuple[int, ...], anchors: tuple[Point, ...]) -> tuple[tuple[int, ...], ...]:
    # partition axis-aligned bubbles into collinear runs — a shared-y and a shared-x band — so a two-axis grid
    # solves each axis independently, not on one diagonal; each bubble joins the band with more peers.
    rows: dict[float, list[int]] = {}
    cols: dict[float, list[int]] = {}
    for index, (x, y) in zip(
        indices, anchors, strict=True
    ):  # Exemption: the two-key co-grouping accumulates into local band dicts — the partition seam
        rows.setdefault(round(y, 1), []).append(index)
        cols.setdefault(round(x, 1), []).append(index)
    runs: list[tuple[int, ...]] = []
    seen: set[int] = set()
    for index, (x, y) in zip(indices, anchors, strict=True):
        if index in seen:
            continue
        band = max((rows[round(y, 1)], cols[round(x, 1)]), key=len)  # the axis with more collinear peers
        runs.append(tuple(idx for idx in band if idx not in seen))
        seen.update(band)
    return tuple(band for band in runs if band)


def _grid_solve(marks: tuple[SymbolKind, ...]) -> tuple[SymbolKind, ...]:
    # each collinear run aligns through one kiwisolver.Solver: each position a `t` Variable, endpoints `required`,
    # given positions `weak`, even gap `medium`, min-separation `weak`; `updateVariables()` writes each solved
    # `value()` BACK into the re-keyed anchor.
    grid = tuple(index for index, mark in enumerate(marks) if mark.tag == "grid")
    if len(grid) < 3:  # two endpoints fully determine a line; the solve refines three or more
        return marks
    solved: dict[int, SymbolKind] = {}
    for line in _grid_runs(grid, tuple(marks[index].grid[0] for index in grid)):
        if len(line) < 3:  # a lone / paired run is already determined by its ends
            continue
        start, stop = marks[line[0]].grid[0], marks[line[-1]].grid[0]
        span = math.dist(start, stop) or 1.0
        given = tuple(math.dist(start, marks[index].grid[0]) / span for index in line)
        solver = kiwisolver.Solver()  # one independent solver per collinear run
        ts = tuple(kiwisolver.Variable(f"t{index}") for index in line)
        solver.addConstraint(
            (ts[0] == 0.0) | kiwisolver.strength.required
        )  # Exemption: kiwisolver Solver is the stateful native sink; constraints add in place
        solver.addConstraint((ts[-1] == 1.0) | kiwisolver.strength.required)
        for k in range(1, len(ts) - 1):
            solver.addConstraint((ts[k] == given[k]) | kiwisolver.strength.weak)
        for lo, hi in pairwise(ts):
            solver.addConstraint((hi - lo >= _MIN_GAP) | kiwisolver.strength.weak)
        for lo, mid, hi in zip(ts, ts[1:], ts[2:], strict=False):
            solver.addConstraint(((mid - lo) == (hi - mid)) | kiwisolver.strength.medium)
        solver.updateVariables()
        for k, index in enumerate(line):
            t, (_anchor, radius, label, style) = ts[k].value(), marks[index].grid
            solved[index] = SymbolKind.Grid((start[0] + t * (stop[0] - start[0]), start[1] + t * (stop[1] - start[1])), radius, label, style)
    return tuple(solved.get(index, mark) for index, mark in enumerate(marks))


# --- [BOUNDARIES] -----------------------------------------------------------------------
def _layer_svg(name: str, frags: list[bytes], box: Box) -> bytes:
    # bucket one SymbolStyle.layer's marks into a named drawsvg.Group; the canvas sized to the real content bbox.
    xmin, ymin, xmax, ymax = box
    canvas = drawsvg.Drawing(xmax - xmin, ymax - ymin, origin=(xmin, ymin))
    group = drawsvg.Group(id=name)
    for frag in frags:
        group.append(drawsvg.Raw(frag.decode()))
    canvas.append(group)
    return canvas.as_svg().encode()


def _bbox(marks: tuple[SymbolKind, ...]) -> Box:
    corners = tuple(
        corner
        for mark in marks
        for corner in (
            (_center(mark)[0] - _extent(mark), _center(mark)[1] - _extent(mark)),
            (_center(mark)[0] + _extent(mark), _center(mark)[1] + _extent(mark)),
        )
    )
    return (
        (min(c[0] for c in corners), min(c[1] for c in corners), max(c[0] for c in corners), max(c[1] for c in corners))
        if corners
        else (0.0, 0.0, 1.0, 1.0)
    )


def _dxf_block(block: object, mark: SymbolKind) -> object:
    # the parametric block geometry authored ONCE per (tag, extent) signature; a number is an add_attdef ATTRIB. TOTAL over the family.
    match mark:
        case SymbolKind(tag="north", north=(_, radius, _bearing, _style)):
            # filled north half + hollow south half matching the SVG two-tone arrow; the solid hatch closes the SVG-vs-DXF fill parity gap.
            solid = block.add_hatch()  # Exemption: ezdxf Hatch is the native fill sink; boundary path + solid fill mutate in place
            solid.paths.add_polyline_path([(0.0, radius), (-radius * 0.42, -radius * 0.55), (0.0, -radius * 0.25)], is_closed=True)
            solid.set_solid_fill()
            block.add_lwpolyline([(0.0, radius), (radius * 0.42, -radius * 0.55), (0.0, -radius * 0.25)], close=True)  # hollow south half
            block.add_attdef("N", (0.0, radius * 1.2))
        case SymbolKind(tag="datum", datum=(_, size, _level, _style)):
            # the ISO spot-level triangle FILLED to SVG parity via a solid hatch, its own geometry + right-set MARK attdef.
            solid = block.add_hatch()  # Exemption: the native Hatch fill sink mutates in place
            solid.paths.add_polyline_path([(0.0, 0.0), (-size * 0.5, size), (size * 0.5, size)], is_closed=True)
            solid.set_solid_fill()
            block.add_attdef("MARK", (size * 0.8, size * 0.6))
        case SymbolKind(tag="revision", revision=(_, size, _mark, _style)):
            block.add_lwpolyline([(0.0, size), (-size * 0.87, -size * 0.5), (size * 0.87, -size * 0.5)], close=True)  # hollow triangle (SVG unfilled)
            block.add_attdef("MARK", (0.0, -size * 0.05))
        case SymbolKind(tag="matchline", matchline=(vertices, _ref, _style)) | SymbolKind(tag="breakline", breakline=(vertices, _style2)):
            block.add_lwpolyline([(vx - vertices[0][0], vy - vertices[0][1]) for vx, vy in vertices])
        case SymbolKind(tag="scale_bar", scale_bar=(_, length, seg, _units, _ratio, _style)):
            step = length / max(seg, 1)
            for i in range(max(seg, 1)):  # Exemption: ezdxf BlockLayout is the GraphicsFactory sink; add_* mutate the block in place
                block.add_lwpolyline([(i * step, 0.0), ((i + 1) * step, 0.0), ((i + 1) * step, length * 0.05), (i * step, length * 0.05)], close=True)
        case SymbolKind(tag="keyplan", keyplan=(_, extent, parcels, _highlight, _style)):
            block.add_lwpolyline([(0.0, 0.0), (extent[0], 0.0), (extent[0], extent[1]), (0.0, extent[1])], close=True)
            for x0, y0, x1, y1 in parcels:  # Exemption: the parcel rectangles assemble onto the block in place
                block.add_lwpolyline([(x0, y0), (x1, y0), (x1, y1), (x0, y1)], close=True)
        case (
            SymbolKind(tag="section", section=(_, radius, *_))
            | SymbolKind(tag="elevation", elevation=(_, radius, *_))
            | SymbolKind(tag="detail", detail=(_, radius, *_))
            | SymbolKind(tag="grid", grid=(_, radius, *_))
        ):  # the bubble family — a circle carrying its number ATTRIB
            block.add_circle((0.0, 0.0), radius)
            block.add_attdef("NUMBER", (0.0, 0.0))
        case _ as unreachable:
            assert_never(unreachable)
    return block


# --- [TABLES] ---------------------------------------------------------------------------
# ISO 129-1 line-end -> schemdraw arrow style; the section/elevation cutting-plane tails draw it.
_ARROW: frozendict[Terminator, str] = frozendict({
    Terminator.FILLED_ARROW: "->",
    Terminator.OPEN_ARROW: "->",
    Terminator.OBLIQUE_STROKE: "-|",
    Terminator.DOT: "-o",
    Terminator.NONE: "-",
})


def _svg_engine(symbol: Symbol) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    ramp = hex_ramp(symbol.palette)
    aligned = _grid_solve(symbol.marks)
    box = _bbox(aligned)
    groups: dict[str, list[bytes]] = {}
    for mark in aligned:  # Exemption: the drawsvg named-layer tree buckets marks by SymbolStyle.layer through a mutable dict of fragment lists
        groups.setdefault(_style(mark).layer.compose(), []).append(_svg_mark(mark, ramp))
    layers = tuple(_row(name=name, source=_layer_svg(name, frags, box), bbox=box) for name, frags in sorted(groups.items()))
    key = symbol._key
    return layers, ArtifactReceipt.Drawing(
        key, "symbol", len(symbol.marks), "drawsvg", int(box[2] - box[0]), int(box[3] - box[1]), sum(len(layer.source) for layer in layers)
    )


def _dxf_engine(symbol: Symbol) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    # each (tag, extent) signature authored ONCE as a doc.blocks.new block, placed by N add_blockref under Standard.graphics(layer) — never a per-placement copy.
    doc, std = ezdxf.new("R2018", setup=True), Standard.of()
    std.seed(doc, layers=tuple({_style(mark).layer for mark in symbol.marks}))
    msp = doc.modelspace()
    blocks: dict[tuple[str, int], object] = {}
    for mark in symbol.marks:  # Exemption: ezdxf Modelspace/BlockLayout is the GraphicsFactory sink; blocks accrue and blockrefs place in place
        signature = (mark.tag, round(_extent(mark)))
        block = blocks.setdefault(signature, _dxf_block(doc.blocks.new(f"SYM_{signature[0]}_{signature[1]}"), mark))
        msp.add_blockref(block.name, _center(mark), dxfattribs=std.graphics(_style(mark).layer).asdict())
    stream = io.StringIO()
    doc.write(stream)
    data, box = stream.getvalue().encode(), _bbox(symbol.marks)
    key = symbol._key
    return (_row(name="dxf", source=data, bbox=box),), ArtifactReceipt.Drawing(
        key, "symbol", len(symbol.marks), "ezdxf", int(box[2] - box[0]), int(box[3] - box[1]), len(data)
    )


_ENGINES: frozendict[SymbolTarget, Callable[[Symbol], tuple[tuple[LayerNode, ...], ArtifactReceipt]]] = frozendict({
    SymbolTarget.SVG: _svg_engine,
    SymbolTarget.DXF: _dxf_engine,
})


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["Symbol", "SymbolKind", "SymbolStyle", "SymbolTarget"]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
