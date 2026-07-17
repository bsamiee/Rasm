# [PY_ARTIFACTS_DRAWING_SYMBOL]

`Symbol` owns the AEC marker vocabulary and populates graphic-cell bytes for `composition/sheet#SHEET`. One closed `SymbolKind` union carries typed geometry and `SymbolStyle`, then dual-lowers through `SymbolTarget` into named `drawsvg` layers or reusable `ezdxf` blocks. `IdTag` generates room, door, window, wall, and equipment tags from `TagShape` data without per-tag sibling cases.

`SymbolStyle` composes `LayerName`, `LineWeight`, `TextHeight`, and `Terminator` with `fill` and `stroke` indices into `graphic/color/derive#DERIVE` `hex_ramp`. `schemdraw.ElementCompound` owns each SVG mark. DXF authors one block per translation-and-rotation-normalized `_signature`; `add_attdef` and `Insert.add_auto_attribs` carry labels, insert rotation carries orientation, and `_dxf_extras` carries directional geometry. `lane: LanePolicy` offloads synchronous authoring. `SymbolStyle.layer` buckets marks into `LayerNode` rows, while `glyph` composes `applied(RegionOp.Rasterize(...))` for sheet-cell PNG bytes. `ArtifactReceipt.Drawing` and `ArtifactWork` carry the result.

## [01]-[INDEX]

- [01]-[SYMBOL]: the closed `SymbolKind` marker union dual-lowered over `SymbolTarget` into a `drawsvg` named-layer group or an `ezdxf` reusable block.

## [02]-[SYMBOL]

- Owner: `Symbol` holds `marks`, the `Palette`, the `SymbolTarget` egress policy (the DXF arm seeds a default `Standard.of()` document, the SVG arm needs no profile), and the `lane: LanePolicy` execution policy; it discriminates over the closed `SymbolKind` union whose every case carries its typed geometry plus one `SymbolStyle` — never a per-marker `SectionMarker`/`GridBubble` family, never a `StrEnum` over an erased `dict[str, object]`. `SymbolStyle` is the ONE drawing-plane mark-style `Struct`, the ISO-grounded peer of `glyphset#GLYPHSET`'s `GlyphStyle`, whose `LayerName` binds both the named `drawsvg.Group` and the `ezdxf` `GfxAttribs`. `SymbolTarget` keys the `_ENGINES` dual-lowering table straight to its engine callable so a new egress is one row, never a per-target subtype.
- Cases: the twelve marker cases, each carrying its typed geometry, one `SymbolStyle`, and (for the bubble/pointer/grid/match/tag marks) a named `self.anchors` terminal — `cut`/`point`/`leader`/`attach`/`match`/`north`/`level` — a `drawing/detail#DETAIL`/`drawing/annotate` leader binds; matched by one total `_element` fold closed by `assert_never`, never a per-marker special case. `ScaleBar` is the standalone twin of the `composition/sheet#SHEET` `Scale.bar` geometry; `KeyPlan` is the reference figure `sheet`'s `KeyPlan.figure` cell consumes; `IdTag` generates the identity-tag family over `TagShape`, its `bool(sublabel)` joining the block signature because a bisected and a plain tag are distinct geometry.
- Entry: `Symbol.over` normalizes `SymbolKind | Iterable[SymbolKind]` into `marks`; `emit`, `layered`, and `glyph` ride `self.lane.offload(Kernel.of(..., KernelTrait.RELEASING), ...)`. `_svg_engine` fixes `use('svg')`, outlined text, and coordinate precision before solving grid runs and bucketing self-contained marks by `SymbolStyle.layer`. `_dxf_engine` applies the same grid solve, guards one `doc.blocks.new` per `_signature`, places `add_blockref` references with `Standard.graphics(layer)`, fills ATTRIBs through `add_auto_attribs`, and adds directional geometry through `_dxf_extras`. `glyph` maps `RegionFault` into the rail `BoundaryFault` without nesting `Result`.
- Auto: `SymbolStyle.fill`/`stroke` index the `hex_ramp` ramp resolved once per render, so a recolor is a palette swap; `Standard.graphics(layer)` projects the same `LayerName` into the `ezdxf` `GfxAttribs` so the SVG group and the DXF layer carry one discipline pen. `North`'s south-half and the `Revision`/`MatchLine`/`Datum` captions are all DRAWN geometry, never a promised-but-dropped part; the DXF block marks mirror the SVG fills through `add_hatch().set_solid_fill()` over a closed boundary path at cross-egress parity, and the block name is the deterministic authoring-order `SYM_<tag>_<index>` — never a process-random hash forking the content key. Each collinear grid run threads its OWN `kiwisolver.Solver` — endpoints `required`, given positions `weak`, an even gap at `medium`, min-separation `strong`, the `strength` bands ranking the hard endpoint snap above the clearance floor above aesthetic spacing — and `_grid_runs` partitions a two-axis structural grid into independent X/Y runs so both axes never collapse onto one diagonal; `updateVariables()` writes each solved `value()` back into the re-keyed anchor before `_element` reads it.
- Packages: `schemdraw` owns `ElementCompound`, `Segment*`, named anchors, and outlined SVG text; `drawsvg` owns the named-layer `Group`; `ezdxf` owns blocks, references, ATTRIBs, and solid hatches; `kiwisolver` owns grid-run alignment; `graphic/vector/region#REGION` owns `applied(RegionOp.Rasterize(...))`; `drawing/regime#REGIME` and `drawing/standard#STANDARD` own ISO vocabulary and DXF lowering; `graphic/color/derive#DERIVE` owns `Palette` and `hex_ramp`.
- Growth: a new AEC marker is one `SymbolKind` case plus one `_element` arm, one `_dxf_block` arm, and one `_signature` arm — the five compound primitives cover the geometry; a new identity-tag kind is one `TagShape` member, zero new cases; a new egress one `SymbolTarget` member plus one `_ENGINES` row; a new visual axis one `SymbolStyle` field; a new named terminal one `self.anchors` key; a new alignment axis one `_grid_runs` band plus one `kiwisolver` constraint at its `strength` band; a new line-end one `Terminator` member plus one `_ARROW` row; a new receipt fact one scalar the shared `ArtifactReceipt.Drawing` carries.
- Boundary: no sheet-set, dimension, or annotation logic (`composition/sheet#SHEET`/`drawing/dimension#DIMENSION`/`drawing/annotate#ANNOTATE`); no IFC semantics (`csharp:Rasm.Bim`); identity minting is the runtime's. `graphic/vector/region#REGION` owns the SVG↔raster and the landed `boolean`/`outline` a filled-band match line or unioned north silhouette composes; `graphic/layer#LAYER` owns the layer vocabulary; `composition/sheet#SHEET` owns the cell placement.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import math
from collections.abc import Callable, Iterable
from enum import StrEnum
from itertools import pairwise
from typing import Annotated, Final, Literal, Self, assert_never
from xml.etree.ElementTree import fromstring, tostring

from beartype import beartype
from beartype.vale import Is
from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from msgspec import Struct
from msgspec.msgpack import Encoder

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.drawing.regime import LayerName, LayerSchema, LineType, LineWeight, Terminator, TextHeight
from rasm.artifacts.drawing.standard import Standard
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp
from rasm.artifacts.graphic.layer import LayerContent, LayerIntent, LayerMeta, LayerNode, LayerPlan
from rasm.artifacts.graphic.vector.region import RegionFault, RegionOp, RegionResult, RenderPolicy, applied

# each proxy reifies on first render-arm use in the offloaded worker
lazy import drawsvg
lazy import ezdxf
lazy import kiwisolver
lazy from ezdxf.tools.text import MTextEditor
lazy from schemdraw import Drawing as Schematic, elements, segments, svgconfig, use

# --- [TYPES] ----------------------------------------------------------------------------
type Point = tuple[float, float]
type PointRun = tuple[Point, Point, *tuple[Point, ...]]
type Positive = Annotated[float, Is[lambda value: math.isfinite(value) and value > 0.0]]
type PositiveInt = Annotated[int, Is[lambda value: value > 0]]
type Box = tuple[float, float, float, float]
type Signature = tuple[object, ...]  # the translation/rotation-normalized block identity
type SymbolTag = Literal[
    "section", "elevation", "detail", "grid", "matchline", "north", "scale_bar", "revision", "keyplan", "datum", "breakline", "idtag"
]
type TerminatorKind = Literal["filled", "open", "oblique", "dot", "origin", "none"]


class SymbolTarget(StrEnum):  # the dual-lowering egress — a new target is one `_ENGINES` row, never a subtype
    SVG = "svg"  # drawsvg named-layer `<g>` groups the export/layered OCG owner binds
    DXF = "dxf"  # ezdxf reusable-block document the CAD rail reads


class TagShape(StrEnum):  # the identity-tag ring family — room/door/window/wall/equipment tags are rows over this vocabulary
    CIRCLE = "circle"
    HEXAGON = "hexagon"
    DIAMOND = "diamond"
    SQUARE = "square"
    ROUNDED = "rounded"


# --- [CONSTANTS] ------------------------------------------------------------------------
_MIN_GAP: Final[float] = 0.02  # minimum t-separation keeping a dense grid-bubble run monotone and non-overlapping
_GRID_DIGITS: Final[int] = 1  # collinearity bucket precision for the grid-run partition
_PRECISION: Final[int] = 3  # svgconfig coordinate places — the content-key determinism lever
_GLYPH: Final[RenderPolicy] = RenderPolicy(dpi=300.0)  # sheet-cell PNG policy for RegionOp.Rasterize
_CANON: Final[Encoder] = Encoder(order="deterministic")  # stable preimage encoding the bare `ContentIdentity.key` mint addresses


# --- [MODELS] ---------------------------------------------------------------------------
class SymbolStyle(Struct, frozen=True):
    # ONE drawing-plane mark-style owner: fill/stroke index the hex_ramp ramp, layer/weight/text_height/terminator compose regime's ISO vocabulary.
    layer: LayerName
    fill: int = 0
    stroke: int = 0
    weight: LineWeight = LineWeight.W025  # ISO 128 line-weight
    text_height: TextHeight = TextHeight.H2_5  # ISO 3098 lettering height
    terminator: Terminator = Terminator.FILLED_ARROW  # ISO 129-1 section-tail line end
    # font-face identity BOTH lowerings resolve: `font=` on every SVG SegmentText run, an MTextEditor `\f` override on the
    # DXF captions; "" selects each engine's ISO default, and DXF block ATTRIBs ride the doc's seeded ISO-3098 style.
    face: str = ""


# --- [VOCABULARY] -----------------------------------------------------------------------
@tagged_union(frozen=True)
class SymbolKind:
    tag: SymbolTag = tag()
    section: tuple[Point, Positive, str, str, float, SymbolStyle] = case()
    elevation: tuple[Point, Positive, str, str, float, SymbolStyle] = case()
    detail: tuple[Point, Positive, str, str, SymbolStyle] = case()
    grid: tuple[Point, Positive, str, SymbolStyle] = case()
    matchline: tuple[PointRun, str, SymbolStyle] = case()
    north: tuple[Point, Positive, float, SymbolStyle] = case()
    scale_bar: tuple[Point, Positive, PositiveInt, str, str, SymbolStyle] = case()
    revision: tuple[Point, Positive, str, SymbolStyle] = case()
    keyplan: tuple[Point, tuple[float, float], tuple[Box, ...], int, SymbolStyle] = case()
    datum: tuple[Point, Positive, str, SymbolStyle] = case()
    breakline: tuple[PointRun, SymbolStyle] = case()
    idtag: tuple[Point, Positive, str, str, TagShape, SymbolStyle] = case()

    @staticmethod
    def Section(center: Point, radius: Positive, detail_no: str, sheet_ref: str, bearing: float, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(section=(center, radius, detail_no, sheet_ref, bearing, style))

    @staticmethod
    def Elevation(center: Point, radius: Positive, elev_no: str, sheet_ref: str, angle: float, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(elevation=(center, radius, elev_no, sheet_ref, angle, style))

    @staticmethod
    def Detail(center: Point, radius: Positive, detail_no: str, sheet_ref: str, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(detail=(center, radius, detail_no, sheet_ref, style))

    @staticmethod
    def Grid(anchor: Point, radius: Positive, label: str, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(grid=(anchor, radius, label, style))

    @staticmethod
    def MatchLine(vertices: PointRun, sheet_ref: str, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(matchline=(vertices, sheet_ref, style))

    @staticmethod
    def North(center: Point, radius: Positive, bearing: float, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(north=(center, radius, bearing, style))

    @staticmethod
    def ScaleBar(origin: Point, length: Positive, segments: PositiveInt, units: str, ratio: str, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(scale_bar=(origin, length, segments, units, ratio, style))

    @staticmethod
    def Revision(center: Point, size: Positive, mark: str, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(revision=(center, size, mark, style))

    @staticmethod
    def KeyPlan(origin: Point, extent: tuple[float, float], parcels: tuple[Box, ...], highlight: int, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(keyplan=(origin, extent, parcels, highlight, style))

    @staticmethod
    def Datum(anchor: Point, size: Positive, level: str, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(datum=(anchor, size, level, style))

    @staticmethod
    def BreakLine(vertices: PointRun, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(breakline=(vertices, style))

    @staticmethod
    def IdTag(anchor: Point, size: Positive, label: str, sublabel: str, shape: TagShape, style: SymbolStyle) -> "SymbolKind":
        return SymbolKind(idtag=(anchor, size, label, sublabel, shape, style))


# --- [SERVICES] -------------------------------------------------------------------------
class Symbol(Struct, frozen=True):
    marks: tuple[SymbolKind, ...]
    palette: Palette
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    lane: LanePolicy
    target: SymbolTarget = SymbolTarget.SVG

    @classmethod
    @beartype
    def over(
        cls, marks: SymbolKind | Iterable[SymbolKind], palette: Palette, /, *, lane: LanePolicy, target: SymbolTarget = SymbolTarget.SVG
    ) -> Self:
        match marks:  # modal-arity head: a lone mark the singleton, an iterable the multi-mark sheet
            case SymbolKind():
                return cls(marks=(marks,), palette=palette, target=target, lane=lane)
            case _:
                return cls(marks=tuple(marks), palette=palette, target=target, lane=lane)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.marks)))

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT minted PRE-RUN through the bare mint; the palette enters as its resolved hex ramp — the one
        # canonical projection both engines pen — and the lane is execution policy, outside the preimage.
        return ContentIdentity.key(f"drawing-symbol-{self.target}", _CANON.encode((self.marks, hex_ramp(self.palette), self.target)))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # offload rails the synchronous fold itself; the returned rail composes — never re-raised for a second boundary.
        return (await self.lane.offload(Kernel.of(_ENGINES[self.target], KernelTrait.RELEASING), self)).map(lambda pair: pair[1])

    async def layered(self) -> RuntimeRail[LayerPlan]:
        # Engine rows as ONE LayerPlan tree — substrate the layered/sheet consumers compose as parents.
        return (await self.lane.offload(Kernel.of(_ENGINES[self.target], KernelTrait.RELEASING), self)).map(
            lambda pair: LayerPlan(schema=LayerSchema.ISO13567, roots=pair[0])
        )

    async def glyph(self, mark: SymbolKind, /) -> RuntimeRail[bytes]:
        # Symbol->sheet seam: ONE mark rasterized to PNG for a sheet NorthArrow.glyph/KeyPlan.figure cell whose
        # reportlab ImageReader reads a raster; the member RegionFault folds into the rail's own BoundaryFault.
        outcome = await self.lane.offload(Kernel.of(_raster, KernelTrait.RELEASING), mark, self.palette)
        return outcome.bind(lambda res: res.map_error(lambda fault: BoundaryFault(boundary=("symbol.glyph", fault.tag))))


# --- [OPERATIONS] -----------------------------------------------------------------------
def _configured() -> None:
    # process-global schemdraw render policy, idempotent per worker: the standalone svg backend, every SegmentText
    # outlined to <path> by the bundled ziafont, coordinate places pinned for the content key.
    use("svg")
    svgconfig.text = "path"
    svgconfig.precision = _PRECISION


def _row(name: str, source: bytes, aec: Option[LayerName], z: int = 0, /) -> LayerNode:
    # one engine row into the layer vocabulary — a Leaf over LayerMeta; aec present derives the ISO 13567 name downstream, z rides bucket order.
    return LayerNode.Leaf(LayerMeta(name=name, intent=LayerIntent.ANNOTATION, z=z, aec=aec), LayerContent.Fragment(source))


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
            | SymbolKind(tag="idtag", idtag=(*_, style))
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
            | SymbolKind(tag="idtag", idtag=(center, *_))
        ):
            return center
        case _ as unreachable:
            assert_never(unreachable)


def _extent(mark: SymbolKind, /) -> float:
    match mark:  # the mark's characteristic half-extent, the bbox reads
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
        case SymbolKind(tag="idtag", idtag=(_, size, *_)):
            return size * 1.5
        case SymbolKind(tag="scale_bar", scale_bar=(_, length, *_)):
            return length
        case SymbolKind(tag="matchline", matchline=(vertices, *_)) | SymbolKind(tag="breakline", breakline=(vertices, *_)):
            return max((math.dist(vertices[0], vertex) for vertex in vertices), default=1.0)
        case SymbolKind(tag="keyplan", keyplan=(_, extent, *_)):
            return max(extent)
        case _ as unreachable:
            assert_never(unreachable)


def _signature(mark: SymbolKind, /) -> Signature:
    # Translation/rotation-normalized geometry identity ONE block is authored for — labels ride ATTRIBs and
    # rotation rides the insert, so neither joins the key; distinct geometry can never collide onto a stale block.
    match mark:
        case (
            SymbolKind(tag="section", section=(_, radius, *_))
            | SymbolKind(tag="elevation", elevation=(_, radius, *_))
            | SymbolKind(tag="detail", detail=(_, radius, *_))
            | SymbolKind(tag="grid", grid=(_, radius, *_))
            | SymbolKind(tag="north", north=(_, radius, *_))
        ):
            return (mark.tag, radius)
        case SymbolKind(tag="revision", revision=(_, size, *_)) | SymbolKind(tag="datum", datum=(_, size, *_)):
            return (mark.tag, size)
        case SymbolKind(tag="idtag", idtag=(_, size, _label, sublabel, shape, _style)):
            return (mark.tag, size, shape, bool(sublabel))
        case SymbolKind(tag="scale_bar", scale_bar=(_, length, seg, *_)):
            return (mark.tag, length, seg)
        case SymbolKind(tag="keyplan", keyplan=(_, extent, parcels, highlight, _style)):
            return (mark.tag, extent, parcels, highlight)
        case SymbolKind(tag="matchline", matchline=(vertices, *_)) | SymbolKind(tag="breakline", breakline=(vertices, *_)):
            origin = vertices[0]
            return (mark.tag, tuple((vx - origin[0], vy - origin[1]) for vx, vy in vertices))
        case _ as unreachable:
            assert_never(unreachable)


def _rotation(mark: SymbolKind, /) -> float:
    match mark:  # rotation rides the INSERT, keeping the block canonical at 0 degrees
        case SymbolKind(tag="north", north=(_, _, bearing, _)):
            return bearing
        case (
            SymbolKind(tag="section")
            | SymbolKind(tag="elevation")
            | SymbolKind(tag="detail")
            | SymbolKind(tag="grid")
            | SymbolKind(tag="matchline")
            | SymbolKind(tag="scale_bar")
            | SymbolKind(tag="revision")
            | SymbolKind(tag="keyplan")
            | SymbolKind(tag="datum")
            | SymbolKind(tag="breakline")
            | SymbolKind(tag="idtag")
        ):
            return 0.0
        case _ as unreachable:
            assert_never(unreachable)


def _attribs(mark: SymbolKind, /) -> dict[str, str]:
    match mark:  # per-placement ATTRIB values filled into the block's attdefs through Insert.add_auto_attribs
        case SymbolKind(tag="section", section=(_, _, no, ref, *_)) | SymbolKind(tag="elevation", elevation=(_, _, no, ref, *_)):
            return {"NUMBER": no, "SHEET": ref}
        case SymbolKind(tag="detail", detail=(_, _, no, ref, *_)):
            return {"NUMBER": no, "SHEET": ref}
        case SymbolKind(tag="grid", grid=(_, _, label, _)):
            return {"NUMBER": label}
        case SymbolKind(tag="revision", revision=(_, _, mark_no, _)):
            return {"MARK": mark_no}
        case SymbolKind(tag="datum", datum=(_, _, level, _)):
            return {"MARK": level}
        case SymbolKind(tag="north"):
            return {"N": "N"}
        case SymbolKind(tag="idtag", idtag=(_, _, label, sublabel, *_)):
            return {"LABEL": label, **({"SUB": sublabel} if sublabel else {})}
        case SymbolKind(tag="matchline") | SymbolKind(tag="scale_bar") | SymbolKind(tag="keyplan") | SymbolKind(tag="breakline"):
            return {}
        case _ as unreachable:
            assert_never(unreachable)


def _element(mark: SymbolKind, ramp: tuple[str, ...]) -> tuple["elements.ElementCompound", Point]:
    # schemdraw compound geometry with NAMED terminal anchors a detail/annotate leader binds. TOTAL over the family.
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
        case SymbolKind(tag="idtag", idtag=(anchor, size, label, sublabel, shape, style)):
            return _idtag_element(size, label, sublabel, shape, style, ramp), anchor
        case _ as unreachable:
            assert_never(unreachable)


def _pen(style: SymbolStyle, ramp: tuple[str, ...], /) -> tuple[str, str, float, float]:
    # Palette-and-ISO pen every builder splats: (stroke hex, fill hex, ISO-128 line weight, ISO-3098 mm)
    return ramp[style.stroke % len(ramp)], ramp[style.fill % len(ramp)], style.weight.mm, style.text_height.mm


def _bisected_bubble(radius: float, upper: str, lower: str, style: SymbolStyle, ramp: tuple[str, ...]) -> "elements.ElementCompound":
    # Shared section/detail bubble — the arms differ only in tail and named anchor, so the core is one builder.
    sym, (stroke, _fill, lw, size) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(segments.SegmentCircle((0.0, 0.0), radius, color=stroke, lw=lw))
    sym.segments.append(segments.Segment([(-radius, 0.0), (radius, 0.0)], color=stroke, lw=lw))
    sym.segments.append(segments.SegmentText((0.0, radius * 0.4), upper, align=("center", "center"), fontsize=size, font=style.face or None, color=stroke))
    sym.segments.append(segments.SegmentText((0.0, -radius * 0.4), lower, align=("center", "center"), fontsize=size, font=style.face or None, color=stroke))
    return sym


def _section_element(
    radius: float, detail_no: str, sheet_ref: str, bearing: float, style: SymbolStyle, ramp: tuple[str, ...]
) -> "elements.ElementCompound":
    sym, (stroke, fill, lw, _size) = _bisected_bubble(radius, detail_no, sheet_ref, style, ramp), _pen(style, ramp)
    tip = (radius * 2.0 * math.cos(math.radians(bearing)), radius * 2.0 * math.sin(math.radians(bearing)))
    sym.segments.append(segments.Segment([(0.0, 0.0), tip], color=stroke, lw=lw))
    unit = (tip[0] / (radius * 2.0), tip[1] / (radius * 2.0))
    normal = (-unit[1], unit[0])
    wing1 = (tip[0] - unit[0] * radius * 0.45 + normal[0] * radius * 0.18, tip[1] - unit[1] * radius * 0.45 + normal[1] * radius * 0.18)
    wing2 = (tip[0] - unit[0] * radius * 0.45 - normal[0] * radius * 0.18, tip[1] - unit[1] * radius * 0.45 - normal[1] * radius * 0.18)
    match _ARROW[style.terminator]:
        case "filled":
            sym.segments.append(segments.SegmentPoly([tip, wing1, wing2], closed=True, color=stroke, fill=fill))
        case "open":
            sym.segments.append(segments.Segment([wing1, tip, wing2], color=stroke, lw=lw))
        case "oblique":
            sym.segments.append(segments.Segment([wing1, wing2], color=stroke, lw=lw))
        case "dot":
            sym.segments.append(segments.SegmentCircle(tip, radius * 0.16, color=stroke, fill=fill))
        case "origin":
            sym.segments.append(segments.SegmentCircle(tip, radius * 0.22, color=stroke, lw=lw))
        case "none":
            pass
        case _ as unreachable:
            assert_never(unreachable)
    sym.anchors["cut"] = tip  # the named terminal a detail cross-reference binds
    return sym


def _elevation_element(
    radius: float, elev_no: str, sheet_ref: str, angle: float, style: SymbolStyle, ramp: tuple[str, ...]
) -> "elements.ElementCompound":
    sym, (stroke, fill, lw, size) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(segments.SegmentCircle((0.0, 0.0), radius, color=stroke, lw=lw))
    point = (radius * 1.8 * math.cos(math.radians(angle)), radius * 1.8 * math.sin(math.radians(angle)))
    flank = radius * 0.5
    sym.segments.append(
        segments.SegmentPoly([(0.0, 0.0), (point[0] - flank, point[1]), (point[0] + flank, point[1])], closed=True, color=stroke, fill=fill)
    )
    sym.segments.append(segments.SegmentText((0.0, radius * 0.4), elev_no, align=("center", "center"), fontsize=size, font=style.face or None, color=stroke))
    sym.segments.append(segments.SegmentText((0.0, -radius * 0.4), sheet_ref, align=("center", "center"), fontsize=size, font=style.face or None, color=stroke))
    sym.anchors["point"] = point
    return sym


def _bubble_element(radius: float, detail_no: str, sheet_ref: str, style: SymbolStyle, ramp: tuple[str, ...]) -> "elements.ElementCompound":
    sym = _bisected_bubble(radius, detail_no, sheet_ref, style, ramp)
    sym.anchors["leader"] = (radius, 0.0)  # the terminal a keynote/detail leader lands on
    return sym


def _grid_element(radius: float, label: str, style: SymbolStyle, ramp: tuple[str, ...]) -> "elements.ElementCompound":
    sym, (stroke, _fill, lw, size) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(segments.SegmentCircle((0.0, 0.0), radius, color=stroke, lw=lw))
    sym.segments.append(segments.SegmentText((0.0, 0.0), label, align=("center", "center"), fontsize=size, font=style.face or None, color=stroke))
    sym.anchors["attach"] = (0.0, -radius)  # the grid line meets the bubble here
    return sym


def _matchline_element(vertices: tuple[Point, ...], sheet_ref: str, style: SymbolStyle, ramp: tuple[str, ...]) -> "elements.ElementCompound":
    sym, (stroke, _fill, lw, size) = elements.ElementCompound(), _pen(style, ramp)
    path = [(vx - vertices[0][0], vy - vertices[0][1]) for vx, vy in vertices]
    sym.segments.append(segments.Segment(path, color=stroke, lw=lw * 4.0, ls="--"))
    sym.segments.append(
        segments.SegmentText(path[len(path) // 2], f"MATCH LINE — SEE {sheet_ref}", align=("center", "bottom"), fontsize=size, font=style.face or None, color=stroke)
    )
    sym.anchors["match"] = path[-1]
    return sym


def _north_element(radius: float, bearing: float, style: SymbolStyle, ramp: tuple[str, ...]) -> "elements.ElementCompound":
    # both halves and the "N" all drawn geometry, never a dropped part.
    sym, (stroke, fill, _lw, size) = elements.ElementCompound(), _pen(style, ramp)
    rot, base = math.radians(bearing), radius * 0.42
    tip, tail = _rotate((0.0, radius), rot), _rotate((0.0, -radius * 0.25), rot)
    left, right = _rotate((-base, -radius * 0.55), rot), _rotate((base, -radius * 0.55), rot)
    sym.segments.append(segments.SegmentPoly([tip, left, tail], closed=True, color=stroke, fill=fill))  # filled north half
    sym.segments.append(segments.SegmentPoly([tip, right, tail], closed=True, color=stroke))  # hollow south half
    sym.segments.append(segments.SegmentText(_rotate((0.0, radius * 1.25), rot), "N", align=("center", "center"), fontsize=size, font=style.face or None, color=stroke))
    sym.anchors["north"] = tip
    return sym


def _scale_element(length: float, seg: int, units: str, ratio: str, style: SymbolStyle, ramp: tuple[str, ...]) -> "elements.ElementCompound":
    sym, (stroke, fill, lw, size) = elements.ElementCompound(), _pen(style, ramp)
    step, height = length / seg, size
    for i in range(seg):  # Exemption: schemdraw appends Segment* to the mutable self.segments; the ruler assembles in place
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
        segments.SegmentText((length / 2.0, -height), f"0 — {length:g} {units} ({ratio})", align=("center", "top"), fontsize=size, font=style.face or None, color=stroke)
    )
    return sym


def _revision_element(size_: float, mark: str, style: SymbolStyle, ramp: tuple[str, ...]) -> "elements.ElementCompound":
    sym, (stroke, _fill, lw, text) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(
        segments.SegmentPoly([(0.0, size_), (-size_ * 0.87, -size_ * 0.5), (size_ * 0.87, -size_ * 0.5)], closed=True, color=stroke, lw=lw)
    )
    sym.segments.append(segments.SegmentText((0.0, -size_ * 0.05), mark, align=("center", "center"), fontsize=text, font=style.face or None, color=stroke))
    return sym


def _keyplan_element(
    extent: tuple[float, float], parcels: tuple[Box, ...], highlight: int, style: SymbolStyle, ramp: tuple[str, ...]
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


def _datum_element(size_: float, level: str, style: SymbolStyle, ramp: tuple[str, ...]) -> "elements.ElementCompound":
    # ISO spot-level marker.
    sym, (stroke, fill, lw, text) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(segments.SegmentPoly([(0.0, 0.0), (-size_ * 0.5, size_), (size_ * 0.5, size_)], closed=True, color=stroke, fill=fill))
    sym.segments.append(segments.SegmentText((size_ * 0.8, size_ * 0.6), level, align=("left", "center"), fontsize=text, font=style.face or None, color=stroke))
    sym.anchors["level"] = (0.0, 0.0)
    return sym


def _breakline_element(vertices: tuple[Point, ...], style: SymbolStyle, ramp: tuple[str, ...]) -> "elements.ElementCompound":
    # ISO 128 break line — a midpoint zigzag marking a truncated view.
    sym, (stroke, _fill, lw, _size) = elements.ElementCompound(), _pen(style, ramp)
    origin = vertices[0]
    rel = [(vx - origin[0], vy - origin[1]) for vx, vy in vertices]
    mid = rel[len(rel) // 2]
    zig = [(mid[0] - 2.0, mid[1]), (mid[0] - 0.5, mid[1] + 3.0), (mid[0] + 0.5, mid[1] - 3.0), (mid[0] + 2.0, mid[1])]
    sym.segments.append(segments.Segment([*rel[: len(rel) // 2], *zig, *rel[len(rel) // 2 :]], color=stroke, lw=lw))
    return sym


def _idtag_element(
    size_: float, label: str, sublabel: str, shape: TagShape, style: SymbolStyle, ramp: tuple[str, ...]
) -> "elements.ElementCompound":
    # Parameterized identity-tag generator — the ring is a TagShape row, the bisect appears only with a sublabel.
    sym, (stroke, _fill, lw, text) = elements.ElementCompound(), _pen(style, ramp)
    if shape is TagShape.CIRCLE:
        sym.segments.append(segments.SegmentCircle((0.0, 0.0), size_, color=stroke, lw=lw))
    else:
        verts = [(x * size_, y * size_) for x, y in _TAG_RING[shape]]
        radius = size_ * 0.25 if shape is TagShape.ROUNDED else 0
        sym.segments.append(segments.SegmentPoly(verts, closed=True, cornerradius=radius, color=stroke, lw=lw))
    if sublabel:
        sym.segments.append(segments.Segment([(-size_, 0.0), (size_, 0.0)], color=stroke, lw=lw))
        sym.segments.append(segments.SegmentText((0.0, size_ * 0.4), label, align=("center", "center"), fontsize=text, font=style.face or None, color=stroke))
        sym.segments.append(segments.SegmentText((0.0, -size_ * 0.4), sublabel, align=("center", "center"), fontsize=text, font=style.face or None, color=stroke))
    else:
        sym.segments.append(segments.SegmentText((0.0, 0.0), label, align=("center", "center"), fontsize=text, font=style.face or None, color=stroke))
    sym.anchors["leader"] = (size_, 0.0)
    return sym


def _rotate(point: Point, radians: float) -> Point:  # rotate about the compound origin; the mark places at its centre
    return (point[0] * math.cos(radians) - point[1] * math.sin(radians), point[0] * math.sin(radians) + point[1] * math.cos(radians))


def _svg_mark(mark: SymbolKind, ramp: tuple[str, ...]) -> bytes:
    # ONE self-contained SVG mark under the _configured backend policy.
    element, center = _element(mark, ramp)
    with Schematic(show=False) as sheet:  # Exemption: schemdraw Drawing is a context manager finalizing the layout on __exit__
        sheet += element.at(center)
    return sheet.get_imagedata("svg")


def _raster(mark: SymbolKind, palette: Palette) -> Result[bytes, RegionFault]:
    # Sheet-cell seam: render ONE mark to SVG then lower RegionOp.Rasterize through the region dispatch.
    _configured()
    return applied(RegionOp.Rasterize(_svg_mark(mark, hex_ramp(palette)), _GLYPH)).bind(_raster_result)


def _raster_result(result: RegionResult, /) -> Result[bytes, RegionFault]:
    match result:
        case RegionResult(tag="raster", raster=data):
            return Ok(data)
        case RegionResult(tag="document") | RegionResult(tag="facts") | RegionResult(tag="hits"):
            return Error(RegionFault(contract=f"rasterize:{result.tag}"))
        case _ as unreachable:
            assert_never(unreachable)


def _grid_runs(indices: tuple[int, ...], anchors: tuple[Point, ...]) -> tuple[tuple[int, ...], ...]:
    # partition axis-aligned bubbles into collinear runs — a shared-y and a shared-x band — so a two-axis grid
    # solves each axis independently, not on one diagonal; each bubble joins the band with more peers.
    rows: dict[float, list[int]] = {}
    cols: dict[float, list[int]] = {}
    for index, (x, y) in zip(indices, anchors, strict=True):  # Exemption: the two-key co-grouping accumulates into local band dicts
        rows.setdefault(round(y, _GRID_DIGITS), []).append(index)
        cols.setdefault(round(x, _GRID_DIGITS), []).append(index)
    coordinate = dict(zip(indices, anchors, strict=True))
    rows = {key: sorted(band, key=lambda index: coordinate[index][0]) for key, band in rows.items()}
    cols = {key: sorted(band, key=lambda index: coordinate[index][1]) for key, band in cols.items()}
    runs: list[tuple[int, ...]] = []
    seen: set[int] = set()
    for index, (x, y) in zip(indices, anchors, strict=True):
        if index in seen:
            continue
        band = max((rows[round(y, _GRID_DIGITS)], cols[round(x, _GRID_DIGITS)]), key=len)  # the axis with more collinear peers
        runs.append(tuple(idx for idx in band if idx not in seen))
        seen.update(band)
    return tuple(band for band in runs if band)


def _grid_solve(marks: tuple[SymbolKind, ...]) -> tuple[SymbolKind, ...]:
    # each collinear run aligns through one kiwisolver.Solver: each position a `t` Variable, endpoints `required`,
    # given positions `weak`, even gap `medium`, min-separation `strong` — outranking both aesthetic bands so spacing
    # preference can never relax the clearance; `updateVariables()` writes each solved `value()` BACK into the re-keyed anchor.
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
        solver = kiwisolver.Solver()  # one independent solver per collinear run; Exemption: the stateful native sink
        ts = tuple(kiwisolver.Variable(f"t{index}") for index in line)
        solver.addConstraint((ts[0] == 0.0) | kiwisolver.strength.required)
        solver.addConstraint((ts[-1] == 1.0) | kiwisolver.strength.required)
        for k in range(1, len(ts) - 1):
            solver.addConstraint((ts[k] == given[k]) | kiwisolver.strength.weak)
        for (lower, upper), (lo, hi) in zip(pairwise(line), pairwise(ts), strict=True):
            clearance = max(_MIN_GAP, (marks[lower].grid[1] + marks[upper].grid[1]) / span)
            solver.addConstraint((hi - lo >= clearance) | kiwisolver.strength.strong)
        for lo, mid, hi in zip(ts, ts[1:], ts[2:], strict=False):
            solver.addConstraint(((mid - lo) == (hi - mid)) | kiwisolver.strength.medium)
        solver.updateVariables()
        for k, index in enumerate(line):
            t, (_anchor, radius, label, style) = ts[k].value(), marks[index].grid
            solved[index] = SymbolKind.Grid((start[0] + t * (stop[0] - start[0]), start[1] + t * (stop[1] - start[1])), radius, label, style)
    return tuple(solved.get(index, mark) for index, mark in enumerate(marks))


# --- [BOUNDARIES] -----------------------------------------------------------------------
def _placed(fragment: bytes, /) -> "drawsvg.Raw":
    # re-anchor one schemdraw <svg> inside the layer canvas: x/y/width/height restated from its own tight viewBox —
    # a nested <svg> without them scales to the outer viewport and loses its placement.
    root = fromstring(fragment)
    for key, value in zip(("x", "y", "width", "height"), (root.get("viewBox") or "0 0 1 1").split(), strict=True):
        root.set(key, value)
    return drawsvg.Raw(tostring(root).decode())


def _layer_svg(name: str, frags: list[bytes], box: Box) -> bytes:
    # bucket one SymbolStyle.layer's marks into a named drawsvg.Group; the canvas sized to the real content bbox.
    xmin, ymin, xmax, ymax = box
    canvas = drawsvg.Drawing(max(xmax - xmin, 1.0), max(ymax - ymin, 1.0), origin=(xmin, ymin))
    group = drawsvg.Group(id=name)
    for frag in frags:  # Exemption: drawsvg Group is the mutable child-list sink
        group.append(_placed(frag))
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
    # Canonical 0-degree block geometry authored ONCE per _signature at full SVG parity; a label is an add_attdef
    # ATTRIB the placement fills. TOTAL over the family.
    match mark:
        case SymbolKind(tag="north", north=(_, radius, _bearing, _style)):
            solid = block.add_hatch()  # Exemption: ezdxf Hatch is the native fill sink; boundary path + solid fill mutate in place
            solid.paths.add_polyline_path([(0.0, radius), (-radius * 0.42, -radius * 0.55), (0.0, -radius * 0.25)], is_closed=True)
            solid.set_solid_fill()
            block.add_lwpolyline([(0.0, radius), (radius * 0.42, -radius * 0.55), (0.0, -radius * 0.25)], close=True)  # hollow south half
            block.add_attdef("N", (0.0, radius * 1.2))
        case SymbolKind(tag="datum", datum=(_, size, _level, _style)):
            solid = block.add_hatch()  # Exemption: the native Hatch fill sink mutates in place
            solid.paths.add_polyline_path([(0.0, 0.0), (-size * 0.5, size), (size * 0.5, size)], is_closed=True)
            solid.set_solid_fill()
            block.add_attdef("MARK", (size * 0.8, size * 0.6))
        case SymbolKind(tag="revision", revision=(_, size, _mark, _style)):
            block.add_lwpolyline([(0.0, size), (-size * 0.87, -size * 0.5), (size * 0.87, -size * 0.5)], close=True)  # hollow triangle (SVG unfilled)
            block.add_attdef("MARK", (0.0, -size * 0.05))
        case SymbolKind(tag="matchline", matchline=(vertices, _ref, _style)):
            # heavy dashed match line — linetype parity with the SVG ls="--"; seed() has authored every ISO linetype
            block.add_lwpolyline(
                [(vx - vertices[0][0], vy - vertices[0][1]) for vx, vy in vertices], dxfattribs={"linetype": LineType.DASHED.value}
            )
        case SymbolKind(tag="breakline", breakline=(vertices, _style2)):
            origin = vertices[0]
            rel = [(vx - origin[0], vy - origin[1]) for vx, vy in vertices]
            mid = rel[len(rel) // 2]
            zig = [(mid[0] - 2.0, mid[1]), (mid[0] - 0.5, mid[1] + 3.0), (mid[0] + 0.5, mid[1] - 3.0), (mid[0] + 2.0, mid[1])]
            block.add_lwpolyline([*rel[: len(rel) // 2], *zig, *rel[len(rel) // 2 :]])  # zigzag parity with the SVG break
        case SymbolKind(tag="scale_bar", scale_bar=(_, length, seg, _units, _ratio, _style)):
            step, height = length / seg, length * 0.05
            for i in range(seg):  # Exemption: ezdxf BlockLayout is the GraphicsFactory sink; add_* mutate the block in place
                ring = [(i * step, 0.0), ((i + 1) * step, 0.0), ((i + 1) * step, height), (i * step, height)]
                block.add_lwpolyline(ring, close=True)
                if i % 2:  # alternating solid segment — fill parity with the SVG ruler
                    band = block.add_hatch()
                    band.paths.add_polyline_path(ring, is_closed=True)
                    band.set_solid_fill()
        case SymbolKind(tag="keyplan", keyplan=(_, extent, parcels, highlight, _style)):
            block.add_lwpolyline([(0.0, 0.0), (extent[0], 0.0), (extent[0], extent[1]), (0.0, extent[1])], close=True)
            for index, (x0, y0, x1, y1) in enumerate(parcels):  # Exemption: the parcel rectangles assemble onto the block in place
                ring = [(x0, y0), (x1, y0), (x1, y1), (x0, y1)]
                block.add_lwpolyline(ring, close=True)
                if index == highlight:  # highlighted parcel solid — fill parity with the SVG figure
                    lot = block.add_hatch()
                    lot.paths.add_polyline_path(ring, is_closed=True)
                    lot.set_solid_fill()
        case SymbolKind(tag="idtag", idtag=(_, size, _label, sublabel, shape, _style)):
            match shape:
                case TagShape.CIRCLE:
                    block.add_circle((0.0, 0.0), size)
                case TagShape.ROUNDED:
                    # SVG parity: the rounded ring carries the same size*0.25 corner radius `_idtag_element` draws —
                    # lowered natively as lwpolyline bulge arcs (tan(π/8) per 90° corner), never the sharp square path.
                    r, bulge = size * 0.25, math.tan(math.pi / 8.0)
                    edge = size - r
                    block.add_lwpolyline(
                        [
                            (size, -edge, 0.0), (size, edge, bulge), (edge, size, 0.0), (-edge, size, bulge),
                            (-size, edge, 0.0), (-size, -edge, bulge), (-edge, -size, 0.0), (edge, -size, bulge),
                        ],
                        format="xyb",
                        close=True,
                    )
                case _:
                    block.add_lwpolyline([(x * size, y * size) for x, y in _TAG_RING[shape]], close=True)
            if sublabel:
                block.add_lwpolyline([(-size, 0.0), (size, 0.0)])
                block.add_attdef("LABEL", (0.0, size * 0.4))
                block.add_attdef("SUB", (0.0, -size * 0.4))
            else:
                block.add_attdef("LABEL", (0.0, 0.0))
        case SymbolKind(tag="section", section=(_, radius, *_)) | SymbolKind(tag="detail", detail=(_, radius, *_)):
            block.add_circle((0.0, 0.0), radius)
            block.add_line((-radius, 0.0), (radius, 0.0))  # the bisect the SVG bubble draws
            block.add_attdef("NUMBER", (0.0, radius * 0.4))
            block.add_attdef("SHEET", (0.0, -radius * 0.4))
        case SymbolKind(tag="elevation", elevation=(_, radius, *_)):
            block.add_circle((0.0, 0.0), radius)  # the pointer triangle is per-insert directional geometry (_dxf_extras)
            block.add_attdef("NUMBER", (0.0, radius * 0.4))
            block.add_attdef("SHEET", (0.0, -radius * 0.4))
        case SymbolKind(tag="grid", grid=(_, radius, *_)):
            block.add_circle((0.0, 0.0), radius)
            block.add_attdef("NUMBER", (0.0, 0.0))
        case _ as unreachable:
            assert_never(unreachable)
    return block


def _dxf_extras(msp: object, mark: SymbolKind, gfx: dict[str, object]) -> None:
    # per-instance DIRECTIONAL geometry beside the shared block — a bearing-baked tail inside the block would either
    # rotate the label text or fork one block per bearing; landing it per placement keeps both correct.
    match mark:  # Exemption: ezdxf Modelspace is the GraphicsFactory sink; the directional entities land in place
        case SymbolKind(tag="section", section=(center, radius, _n, _s, bearing, style)):
            tip = (center[0] + radius * 2.0 * math.cos(math.radians(bearing)), center[1] + radius * 2.0 * math.sin(math.radians(bearing)))
            msp.add_line(center, tip, dxfattribs=gfx)
            unit = ((tip[0] - center[0]) / (radius * 2.0), (tip[1] - center[1]) / (radius * 2.0))
            normal = (-unit[1], unit[0])
            wing1 = (tip[0] - unit[0] * radius * 0.45 + normal[0] * radius * 0.18, tip[1] - unit[1] * radius * 0.45 + normal[1] * radius * 0.18)
            wing2 = (tip[0] - unit[0] * radius * 0.45 - normal[0] * radius * 0.18, tip[1] - unit[1] * radius * 0.45 - normal[1] * radius * 0.18)
            match _ARROW[style.terminator]:
                case "filled":
                    arrow = msp.add_hatch(dxfattribs=gfx)
                    arrow.paths.add_polyline_path([tip, wing1, wing2], is_closed=True)
                    arrow.set_solid_fill()
                case "open":
                    msp.add_lwpolyline([wing1, tip, wing2], dxfattribs=gfx)
                case "oblique":
                    msp.add_line(wing1, wing2, dxfattribs=gfx)
                case ("dot" | "origin") as kind:
                    ring = [
                        (tip[0] + radius * (0.16 if kind == "dot" else 0.22) * math.cos(step), tip[1] + radius * (0.16 if kind == "dot" else 0.22) * math.sin(step))
                        for step in (index * math.tau / 16.0 for index in range(16))
                    ]
                    if kind == "dot":
                        dot = msp.add_hatch(dxfattribs=gfx)
                        dot.paths.add_polyline_path(ring, is_closed=True)
                        dot.set_solid_fill()
                    else:
                        msp.add_lwpolyline(ring, close=True, dxfattribs=gfx)
                case "none":
                    pass
                case _ as unreachable:
                    assert_never(unreachable)
        case SymbolKind(tag="elevation", elevation=(center, radius, _n, _s, angle, _estyle)):
            # base flanks ride the perpendicular of the pointing direction, so the triangle stays non-degenerate at every angle
            unit = (math.cos(math.radians(angle)), math.sin(math.radians(angle)))
            normal = (-unit[1], unit[0])
            point = (center[0] + radius * 1.8 * unit[0], center[1] + radius * 1.8 * unit[1])
            flank = radius * 0.5
            pointer = msp.add_hatch(dxfattribs=gfx)
            pointer.paths.add_polyline_path(
                [center, (point[0] - normal[0] * flank, point[1] - normal[1] * flank), (point[0] + normal[0] * flank, point[1] + normal[1] * flank)],
                is_closed=True,
            )
            pointer.set_solid_fill()
        case SymbolKind(tag="matchline", matchline=(vertices, sheet_ref, style)):
            mid = vertices[len(vertices) // 2]
            content = str(MTextEditor().font(style.face or "isocp").append(f"MATCH LINE — SEE {sheet_ref}"))
            msp.add_mtext(content, dxfattribs={**gfx, "char_height": style.text_height.mm, "insert": mid})
        case SymbolKind(tag="scale_bar", scale_bar=(origin, length, _seg, units_, ratio, style)):
            caption = str(MTextEditor().font(style.face or "isocp").append(f"0 — {length:g} {units_} ({ratio})"))
            at = (origin[0] + length / 2.0, origin[1] - style.text_height.mm * 2.0)
            msp.add_mtext(caption, dxfattribs={**gfx, "char_height": style.text_height.mm, "insert": at})
        case (
            SymbolKind(tag="detail")
            | SymbolKind(tag="grid")
            | SymbolKind(tag="north")
            | SymbolKind(tag="revision")
            | SymbolKind(tag="keyplan")
            | SymbolKind(tag="datum")
            | SymbolKind(tag="breakline")
            | SymbolKind(tag="idtag")
        ):
            return
        case _ as unreachable:
            assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------
# ISO 129-1 line-end policy generated by both symbol backends.
_ARROW: Final[frozendict[Terminator, TerminatorKind]] = frozendict({
    Terminator.FILLED_ARROW: "filled",
    Terminator.OPEN_ARROW: "open",
    Terminator.OBLIQUE_STROKE: "oblique",
    Terminator.DOT: "dot",
    Terminator.ORIGIN_INDICATION: "origin",
    Terminator.NONE: "none",
})
# unit ring vertices per TagShape; ROUNDED reuses the square ring under a SegmentPoly cornerradius (SVG) and a bulge-arc lwpolyline (DXF).
_TAG_RING: Final[frozendict[TagShape, tuple[Point, ...]]] = frozendict({
    TagShape.HEXAGON: ((1.0, 0.0), (0.5, 0.87), (-0.5, 0.87), (-1.0, 0.0), (-0.5, -0.87), (0.5, -0.87)),
    TagShape.DIAMOND: ((1.0, 0.0), (0.0, 1.0), (-1.0, 0.0), (0.0, -1.0)),
    TagShape.SQUARE: ((1.0, 1.0), (-1.0, 1.0), (-1.0, -1.0), (1.0, -1.0)),
    TagShape.ROUNDED: ((1.0, 1.0), (-1.0, 1.0), (-1.0, -1.0), (1.0, -1.0)),
})


def _svg_engine(symbol: Symbol) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    _configured()
    ramp = hex_ramp(symbol.palette)
    aligned = _grid_solve(symbol.marks)
    box = _bbox(aligned)
    groups: dict[LayerName, list[bytes]] = {}
    for mark in aligned:  # Exemption: the named-layer tree buckets marks by SymbolStyle.layer through a mutable dict of fragment lists
        groups.setdefault(_style(mark).layer, []).append(_svg_mark(mark, ramp))
    payloads = tuple(
        (layer, _layer_svg(layer.compose(), frags, box)) for layer, frags in sorted(groups.items(), key=lambda kv: kv[0].compose())
    )
    layers = tuple(_row(layer.compose(), data, Some(layer), z) for z, (layer, data) in enumerate(payloads))
    total = sum(len(data) for _layer, data in payloads)
    return layers, ArtifactReceipt.Drawing(
        symbol._key, "symbol", len(symbol.marks), "drawsvg", int(box[2] - box[0]), int(box[3] - box[1]), total
    )


def _dxf_engine(symbol: Symbol) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    # ONE block per distinct _signature; N add_blockref placements fill ATTRIBs and land the directional extras.
    doc, std = ezdxf.new("R2018", setup=True), Standard.of()
    ramp = hex_ramp(symbol.palette)  # SVG/DXF parity: both engines resolve the SAME palette
    aligned = _grid_solve(symbol.marks)
    std.seed(doc, layers=tuple(dict.fromkeys(_style(mark).layer for mark in aligned)))
    msp = doc.modelspace()
    blocks: dict[Signature, str] = {}
    for mark in aligned:  # Exemption: ezdxf Modelspace/BlockLayout is the GraphicsFactory sink; definitions accrue and references place in place
        signature = _signature(mark)
        if signature not in blocks:  # setdefault would evaluate doc.blocks.new eagerly and re-author a duplicate name
            name = f"SYM_{mark.tag}_{len(blocks):03d}"  # deterministic authoring-order name — never a process-random hash
            _dxf_block(doc.blocks.new(name), mark)
            blocks[signature] = name
        style = _style(mark)
        # Admitted palette stroke overrides the regime layer default as a true-color attribute, so a changed
        # fill/stroke/text-height axis is semantically visible on BOTH targets, never a layer-default-only DXF.
        ink = int(ramp[style.stroke % len(ramp)].removeprefix("#"), 16)
        gfx = {**std.graphics(style.layer).asdict(), "true_color": ink}
        reference = msp.add_blockref(blocks[signature], _center(mark), dxfattribs={**gfx, "rotation": _rotation(mark)})
        reference.add_auto_attribs(_attribs(mark))
        _dxf_extras(msp, mark, gfx)
    stream = io.StringIO()
    doc.write(stream)
    data, box = stream.getvalue().encode(), _bbox(aligned)
    return (_row("dxf", data, Nothing),), ArtifactReceipt.Drawing(
        symbol._key, "symbol", len(symbol.marks), "ezdxf", int(box[2] - box[0]), int(box[3] - box[1]), len(data)
    )


_ENGINES: Final[frozendict[SymbolTarget, Callable[[Symbol], tuple[tuple[LayerNode, ...], ArtifactReceipt]]]] = frozendict({
    SymbolTarget.SVG: _svg_engine,
    SymbolTarget.DXF: _dxf_engine,
})


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["Symbol", "SymbolKind", "SymbolStyle", "SymbolTarget", "TagShape"]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
