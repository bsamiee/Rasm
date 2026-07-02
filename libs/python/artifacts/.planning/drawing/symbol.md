# [PY_ARTIFACTS_DRAWING_SYMBOL]

The AEC drawing-symbol vocabulary and the deep graphic owner that populates the STUB graphic-cell bytes `composition/sheet#SHEET` leaves empty. `Symbol` is ONE owner over a closed `SymbolKind` `expression.tagged_union` — `Section`/`Elevation`/`Detail`/`Grid`/`MatchLine`/`North`/`ScaleBar`/`Revision`/`KeyPlan`/`Datum`/`BreakLine` — each case carrying its own typed geometry plus one `SymbolStyle`, dispatched by ONE total `match` and dual-lowered over the `SymbolTarget` policy value. The vocabulary is closed and total: a new AEC marker is one `SymbolKind` case plus one geometry arm, never a per-marker class family and never an erased attribute `dict`. `SymbolStyle` is the drawing-plane mark-style owner composed from `drawing/standard#STANDARD`'s owned vocabulary — the `fill`/`stroke` palette index into `visualization/chart/spec#CHART`'s `hex_ramp`-projected ramp, the `LayerName` ISO 13567/AIA codec binding the mark's named group, the `LineWeight` ISO 128 pen, the `TextHeight` ISO 3098 lettering height, and the `Terminator` ISO 129-1/128-2 line-end — so a mark's layer, colour, weight, and lettering all trace to one owned row rather than a per-mark literal. It is the ISO-grounded drawing-plane peer of the diagram-plane `visualization/diagram/glyphset#GLYPHSET` `GlyphStyle` (palette-indexed identical), the one mark-style owner the sibling `drawing/annotate` and `drawing/detail` producers will compose rather than re-declaring.

Every mark's compound geometry is composed, never hand-emitted: ONE `schemdraw` `ElementCompound` per marker appends typed `Segment`/`SegmentCircle`/`SegmentArc`/`SegmentText`/`SegmentPoly` primitives to `self.segments` and declares its terminal points in `self.anchors` — the `cut`/`point`/`attach`/`leader` terminals the future `drawing/annotate#FLAGNOTE` and `drawing/detail#CALLOUT` leaders bind — rendered font-independent through `use('svg')` + `svgconfig.text='path'` (the bundled `ziafont` outlining every `SegmentText` to a `<path>`) to `get_imagedata('svg')` bytes, never a hand-emitted `<path d>` string. All eleven marks lower through the ONE total `_element` fold, so `_element` is exhaustive over the family — no partial dispatch dressed as total. Each named-layer bucket serializes to its own `drawsvg` `Group(id=layer)` `<svg>` for the layered-export path, AND each mark dual-lowers to an `ezdxf` reusable block (`doc.blocks.new` authored ONCE per distinct geometry signature, placed N times by `add_blockref` with a per-placement `add_attdef` ATTRIB, seeded onto a `Standard.seed`-authored document) for the DXF drawing path — so a section marker, a grid bubble, and a north arrow read as one mark grammar across both the vector-figure and the CAD egress.

The synchronous `schemdraw`/`ezdxf`/`kiwisolver` render offloads off the event loop through one `CapacityLimiter`-bounded `anyio.to_thread.run_sync` — the shared-address-space thread lane the `msgspec`/`numpy` receipt owners force, exactly as `visualization/diagram/draw#DRAW` takes — and rails through the runtime `RuntimeRail`/`async_boundary` `BoundaryFault`, never a decorative page-local fault union the boundary never reads. A grid-bubble run aligns collinear and blended-even through one REAL `kiwisolver.Solver`: endpoints pinned `required`, the given interior positions honoured `weak`, an even gap preferred `medium`, `updateVariables()` writing each solved `value()` BACK into the mark's re-keyed anchor before the geometry fold. The `glyph` seam rasterizes ONE mark to PNG bytes through the LANDED `graphic/vector#VECTOR` `rasterize` (`resvg`) — because `composition/sheet#SHEET`'s `NorthArrow.glyph`/`KeyPlan.figure` cells feed a `reportlab` `ImageReader`/`drawImage` that reads a raster, never an SVG. `Symbol` mints no IFC (that stays `csharp:Rasm.Bim`), computes no sheet placement (that stays `composition/sheet#SHEET`), and re-renders nothing.

## [01]-[INDEX]

- [01]-[SYMBOL]: the `Symbol` owner over the closed `SymbolKind` `expression.tagged_union` (`Section`/`Elevation`/`Detail`/`Grid`/`MatchLine`/`North`/`ScaleBar`/`Revision`/`KeyPlan`/`Datum`/`BreakLine`) dual-lowering each mark over the `SymbolTarget` policy value into a `drawsvg` named-layer `Group` (self-contained `schemdraw`-rendered, `ziafont`-outlined text) or an `ezdxf` reusable block (`doc.blocks.new` authored once + `add_attdef` + N `add_blockref`, seeded by `drawing/standard#STANDARD` `Standard.seed`/`graphics`), composing `schemdraw` `ElementCompound`/`Segment*`/`self.anchors` for the compound geometry with named terminals, `ezdxf` `add_circle`/`add_lwpolyline` + `add_hatch().set_solid_fill()` for the DXF block shapes at SVG fill parity (the filled north half + datum triangle), and one `kiwisolver.Solver` per collinear grid run for two-axis grid blended-even alignment (endpoint-pinned, `value()` read BACK) — `SymbolStyle` palette-indexed to the `hex_ramp` ramp and ISO-grounded to `drawing/standard#STANDARD`'s `LayerName`/`LineWeight`/`TextHeight`/`Terminator`, layer-bound for `export/layered#LAYERED`, its `glyph` PNG projection feeding the `composition/sheet#SHEET` `NorthArrow.glyph`/`KeyPlan.figure` cells through `graphic/vector#VECTOR` `rasterize`, railed through `RuntimeRail`/`async_boundary`, offloaded via `anyio.to_thread`, contributing one `core/receipt#RECEIPT` `ArtifactReceipt.Drawing` case and one `core/plan#PLAN` `ArtifactWork` node.

## [02]-[SYMBOL]

- Owner: `Symbol` the one drawing-symbol owner holding `marks: tuple[SymbolKind, ...]`, the `visualization/chart/spec#CHART` `Palette`, and the `SymbolTarget` egress policy value (the DXF arm seeding a default `drawing/standard#STANDARD` `Standard.of()` document, the SVG arm needing no profile), discriminating operation over the closed `SymbolKind` `expression.tagged_union` whose every case carries its own typed geometry-and-`SymbolStyle` payload — never a per-marker `SectionMarker`/`GridBubble` class family and never a `StrEnum` keyed against an erased `dict[str, object]`. `SymbolStyle` is the ONE drawing-plane mark-style `Struct` composing `drawing/standard#STANDARD`'s owned vocabulary — the `fill`/`stroke` palette index into the `hex_ramp`-projected ramp (imported from `visualization/chart/spec#CHART` where it is declared once), the `LayerName` ISO 13567/AIA codec whose `.compose()` binds the named `drawsvg.Group` and whose discipline pens the `ezdxf` `GfxAttribs`, the `LineWeight` ISO 128 pen, the `TextHeight` ISO 3098 lettering height, and the `Terminator` ISO 129-1/128-2 line-end — so the drawing plane carries one mark-style owner and never a parallel `AnnotateStyle` or a bare-float pen the future `drawing/annotate`/`detail` producers would re-derive. `SymbolTarget` is the closed `StrEnum` (`SVG`/`DXF`) keying the `_ENGINES` `frozendict[SymbolTarget, arm]` dual-lowering table straight to its engine callable so a new egress is one row, never a per-target `Symbol` subtype and never a one-field engine wrapper. `schemdraw` owns ALL compound-symbol geometry (`ElementCompound` + `Segment`/`SegmentCircle`/`SegmentArc`/`SegmentText`/`SegmentPoly` + the named `self.anchors` terminals, rendered font-independent through `use('svg')` + `svgconfig.text='path'` + `get_imagedata('svg')`); `drawsvg` owns the named-layer `Group` container and the `as_svg` serialization; `ezdxf` owns the DXF block model (`doc.blocks.new`/`add_attdef`/`add_blockref`/`add_circle`/`add_line`/`add_lwpolyline`/`add_text` + `add_hatch().set_solid_fill()` the solid-poche fill bringing the north half and datum triangle to SVG parity) driven onto a `Standard.seed`-authored document; `kiwisolver` owns the grid-bubble collinear-alignment solve; `graphic/vector#VECTOR` `rasterize` owns the SVG→PNG raster for the sheet-cell seam. No sheet-set, dimension, or annotation logic crosses this owner — those are `composition/sheet#SHEET`, `drawing/dimension#DIMENSION`, and `drawing/annotate#ANNOTATE`.
- Cases: `SymbolKind` cases — `Section(center, radius, detail_no, sheet_ref, bearing, style)` (the ISO 7200 section-cut marker: a bisected circle carrying the detail number over the sheet reference, a cutting-plane tail at `bearing` terminated by the `style.terminator` schemdraw `arrow=`, the tail endpoint the named `cut` anchor a `drawing/detail#CALLOUT` cross-reference binds) · `Elevation(center, radius, elev_no, sheet_ref, angle, style)` (the interior-elevation marker: a circle with a `SegmentPoly` pointer triangle rotated to `angle`, the elevation number over the sheet reference, the `point` anchor the pointed wall) · `Detail(center, radius, detail_no, sheet_ref, style)` (the detail-callout bubble: a bisected circle carrying the detail number over the sheet reference, the `leader` anchor the `drawing/detail#CALLOUT`/`drawing/annotate#KEYNOTE` leader lands on) · `Grid(anchor, radius, label, style)` (the ISO grid bubble: a circle at a grid-line end carrying the grid label, the `attach` anchor the grid line meets, aligned collinear along its run by the `kiwisolver` solve) · `MatchLine(vertices, sheet_ref, style)` (the match line: a heavy dashed polyline carrying the "MATCH LINE — SEE {sheet_ref}" caption at its midpoint, the `match` anchor its terminus) · `North(center, radius, bearing, style)` (the in-field north arrow: a filled north half plus a hollow south half rotated to `bearing`, the "N" label above) · `ScaleBar(origin, length, segments, units, ratio, style)` (the graphic scale bar: alternating filled/clear division rects over `segments` divisions plus the units-and-ratio caption, the standalone twin of the `composition/sheet#SHEET` `Scale.bar` geometry) · `Revision(center, size, mark, style)` (the revision triangle: an equilateral triangle carrying the revision number) · `KeyPlan(origin, extent, parcels, highlight, style)` (the key-plan reference figure: the parcel rectangles with the `highlight` index parcel filled, the reference `composition/sheet#SHEET`'s `KeyPlan.figure` cell consumes) · `Datum(anchor, size, level, style)` (the ISO spot-level/benchmark marker: a filled level triangle at the anchor carrying the RL/elevation value) · `BreakLine(vertices, style)` (the ISO 128 break line: a polyline with a zigzag break at its midpoint marking a truncated view) — matched by one total `match`/`case` over `tag` in the `_element` fold, never a per-marker special case.
- Entry: `Symbol.over` is the one modal-arity entrypoint normalizing `SymbolKind | Iterable[SymbolKind]` into the `marks` tuple by a structural `match` at the head (a lone marker the singleton case, a mixed sheet the multi-element case), never a `batch` knob or a per-marker sibling; `render` is `async` over the runtime `async_boundary`, returns a `RuntimeRail[tuple[tuple[Layer, ...], ArtifactReceipt]]`, and offloads the whole synchronous fold onto `to_thread.run_sync(_ENGINES[self.target], self, limiter=_LANES)` — the shared-address-space thread arm (the `schemdraw`/`ezdxf`/`kiwisolver` render touches the `numpy` palette and returns the `msgspec`-backed `Layer`/`ArtifactReceipt` owners a `to_interpreter` isolate cannot load, the same lane the `visualization/diagram/draw#DRAW` sibling takes). The `_svg_engine` runs the `kiwisolver` grid solve, folds each mark through `_svg_mark` (the `schemdraw` `ElementCompound` geometry rendered to a self-contained `<svg>`), buckets each into its `SymbolStyle.layer.compose()` `drawsvg.Group`, serializes each named `Group` to its own per-layer `<svg>` as a `Layer(name, source, bbox)` row, and derives the content key over the joined layer bytes; the `_dxf_engine` seeds one `Standard.seed` document, mints ONE `doc.blocks.new` block per distinct `(tag, extent)` geometry signature populated by the `_dxf_block` builders + `add_attdef` ATTRIBs, places each mark by `add_blockref` under `Standard.graphics(layer)` attributes, and writes the DXF as one `Layer("dxf", data, bbox)` row. `glyph` is the symbol→sheet seam: it renders ONE mark's self-contained SVG then rasterizes it to PNG through `graphic/vector#VECTOR` `rasterize`, the byte form a `composition/sheet#SHEET` `NorthArrow.glyph`/`KeyPlan.figure` cell's `ImageReader`/`drawImage` consumes — never a raw SVG the raster loader cannot read, and never a second geometry fold.
- Auto: `SymbolStyle.fill`/`stroke` are integer indices into the `hex_ramp`-projected ramp resolved once per render, so a recolor is a palette swap never a per-mark hex literal, exactly as `visualization/diagram/glyphset#GLYPHSET` indexes its `GlyphStyle`; `SymbolStyle.layer.compose()` buckets each mark into its named `drawsvg.Group` the `export/layered#LAYERED` OCG/SVG-layer owner binds, and `Standard.graphics(layer)` projects the same `LayerName` into the `ezdxf` `GfxAttribs` so the SVG group and the DXF layer carry one discipline pen. The compound geometry is `schemdraw` for every mark — one `ElementCompound` subclass appending typed `Segment*` to `self.segments` (`SegmentCircle` the bubble, `Segment` the bisector/tail/match-line, `SegmentPoly` the pointer/triangle/parcel, `SegmentArc` the elevation sweep) with the terminal points declared in `self.anchors`, rendered through one `schemdraw.Drawing` under `use('svg')`/`svgconfig.text='path'` (the bundled `ziafont` outlining every `SegmentText` to a font-independent `<path>`) to `get_imagedata('svg')` bytes — so the `North` south-half and the `Revision`/`MatchLine`/`Datum` captions are all drawn geometry, never a promised-but-dropped part; the DXF block marks mirror the SVG fills — `add_hatch().set_solid_fill()` over a closed boundary path closes the filled north half and datum triangle (`add_lwpolyline` alone left them outline-only) at cross-egress parity. Each collinear grid run threads its OWN `kiwisolver.Solver`: `_grid_runs` partitions the grid bubbles into a shared-y horizontal band and a shared-x vertical band so a TWO-AXIS structural grid solves each axis independently rather than collapsing both onto one diagonal; within a run each bubble's position is a `t` `Variable`, the endpoints pinned `required` (`t == 0.0`/`t == 1.0`), each given interior position honoured `weak` (`t == projected_fraction`), an even gap preferred `medium` (`(b - a) == (c - b)`), and a monotone min-separation kept `weak`, so `updateVariables()` writes each solved `value()` back into the mark's re-keyed anchor before `_element` reads it — the `strength` bands separating the hard endpoint snap from the aesthetic spacing.
- Growth: a new AEC marker is one `SymbolKind` case plus one `_element` arm (its `schemdraw` `ElementCompound` builder) and one `_dxf_block` arm — the five compound primitives cover the marker geometry, so a new fixture symbol is a `Segment*`-built compound, never a hand-emitted path; a new egress is one `SymbolTarget` member plus one `_ENGINES` row; a new mark visual axis (a hatch fill, a dash pattern) is one `SymbolStyle` field threaded into the consuming arm; a new named terminal is one `self.anchors` key; a new alignment axis (the realized two-axis grid run partition, a future radial distribution or interior obstacle avoidance) is one `_grid_runs` band plus one `kiwisolver` constraint at its `strength` band; a new line-end is one `Terminator` member on `drawing/standard#STANDARD` plus one `_ARROW` row; a new receipt fact is one scalar the shared `ArtifactReceipt.Drawing` case already carries; a filled-band match line or a unioned north silhouette composes the LANDED `graphic/vector#VECTOR` `boolean`/`outline` (present on that owner) as a variant (not a load-bearing dependency, since `schemdraw` already draws the heavy dashed line and the two arrow halves as the correct self-contained default); zero new surface for a new marker or a new layer.
- Boundary: the deleted forms are a per-marker `SectionMarker`/`GridBubble`/`NorthArrow` class family where one closed `SymbolKind` union states them; a hand-emitted `<path d>` or `<g id>` string where `schemdraw` `Segment*` and the `drawsvg.Group` container author it; a per-placement geometry copy where `doc.blocks.new` authors one block per signature placed N times by `add_blockref`; an outline-only DXF mark where the SVG arm fills it, where `add_hatch().set_solid_fill()` brings the north filled half and the datum triangle to cross-egress parity; a shared revision/datum arm authoring both with one wrong triangle where the split arms author each its own; a single-run grid solve collapsing a two-axis structural grid onto one diagonal where `_grid_runs` partitions it into independent X/Y runs; a per-marker colour literal where the `SymbolStyle` palette index binds through the `hex_ramp` ramp; a bare-float pen or a parallel `Terminator` vocabulary where `drawing/standard#STANDARD`'s `LineWeight`/`TextHeight`/`Terminator` own the ISO codes; a `batch`/`mode` knob where `SymbolTarget` and the modal `over` head discriminate; a hollow `kiwisolver` solve that adds constraints and reads nothing back where `updateVariables()` re-keys each anchor; a `partial` `match` routing a reachable case to `assert_never` where `_element` is total over all eleven; a raw SVG fed to the sheet's raster `ImageReader` where `glyph` rasterizes to PNG; a decorative page-local fault union the boundary never reads where `RuntimeRail`/`async_boundary` carries the `BoundaryFault`; a phantom `Vector.over(ops)._worked(ops)` private-method reach (`_worked` is a private module fold on `graphic/vector`, not a `Vector` method); a synchronous render on the event loop where `to_thread.run_sync` offloads it; a phantom sheet-cell absorb where the PNG seam feeds `composition/sheet#SHEET`'s `NorthArrow.glyph`/`KeyPlan.figure` cells and `sheet.md` keeps its title-block value objects; a parallel drawing receipt where the shared `ArtifactReceipt.Drawing` case carries the mark/extent/byte facts. `schemdraw` owns compound geometry, `drawsvg` the named-layer container, `ezdxf` the DXF block model, `kiwisolver` the constraint solve, `drawing/standard#STANDARD` the owned ISO vocabulary, `graphic/vector#VECTOR` the SVG↔raster and the landed boolean/offset (`VectorOp.Boolean`/`VectorOp.Outline` present on that owner), `export/layered#LAYERED` the layer binding, `composition/sheet#SHEET` the cell placement, and `csharp:Rasm.Bim` the IFC semantics; identity minting is the runtime's.
- Packages: `schemdraw` (`ElementCompound`/`Segment`/`SegmentCircle`/`SegmentArc`/`SegmentText`/`SegmentPoly`/`self.anchors`/`Drawing`/`use`/`svgconfig`/`get_imagedata` the compound-symbol geometry with named terminals, the bundled `ziafont` outlining `SegmentText` under `svgconfig.text='path'`); `drawsvg` (`Drawing`/`Group`/`Raw`/`as_svg` the named-layer container); `ezdxf` (`new`/`doc.blocks.new`/`add_attdef`/`add_blockref`/`add_circle`/`add_line`/`add_lwpolyline`/`add_text`/`add_hatch`/`Hatch.paths.add_polyline_path`/`Hatch.set_solid_fill` the DXF block model with the solid-poche fill parity); `kiwisolver` (`Solver`/`Variable`/`addConstraint`/`updateVariables`/`Variable.value`/`strength` the per-run collinear/two-axis alignment solve); `expression` (`tagged_union`/`tag`/`case`/`Result` the vocabulary and rail); `msgspec` (`Struct(frozen=True)` the value objects); `builtins.frozendict` (the `_ENGINES`/`_ARROW` tables); `anyio` (`CapacityLimiter`/`to_thread` the offload); runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`async_boundary`); `core/receipt#RECEIPT` (`ArtifactReceipt.Drawing`); `export/layered#LAYERED` (`Layer`); `graphic/vector#VECTOR` (`RenderPolicy`/`VectorFault`/`rasterize` the SVG→PNG seam); `drawing/standard#STANDARD` (`LayerName`/`LineWeight`/`TextHeight`/`Terminator`/`Standard` the owned ISO vocabulary + `ezdxf` lowering); `visualization/chart/spec#CHART` (`Palette`/`hex_ramp`).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import math
import os
from collections.abc import Callable, Iterable
from enum import StrEnum
from itertools import pairwise
from typing import Literal, Self, assert_never

from anyio import CapacityLimiter, to_thread
from builtins import frozendict
from expression import Result, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.drawing.standard import LayerName, LineWeight, Standard, Terminator, TextHeight
from artifacts.export.layered import Layer
from artifacts.graphic.vector import RenderPolicy, VectorFault, rasterize
from artifacts.visualization.chart.spec import Palette, hex_ramp

# each proxy reifies on first render-arm use in the `to_thread` worker
lazy import drawsvg
lazy import ezdxf
lazy import kiwisolver
lazy from schemdraw import Drawing as Schematic, elements, segments, svgconfig, use

# --- [TYPES] ----------------------------------------------------------------------------
type Point = tuple[float, float]
type Box = tuple[float, float, float, float]
type SymbolTag = Literal[
    "section", "elevation", "detail", "grid", "matchline", "north", "scale_bar", "revision", "keyplan", "datum", "breakline"
]

_LANES: CapacityLimiter = CapacityLimiter(os.process_cpu_count() or 4)
_MIN_GAP: float = 0.02      # minimum t-separation keeping a dense grid-bubble run monotone and non-overlapping
_GLYPH: RenderPolicy = RenderPolicy(dpi=300.0)  # the sheet-cell PNG raster policy `graphic/vector#VECTOR` `rasterize` reads


class SymbolTarget(StrEnum):  # the dual-lowering egress — a new target is one `_ENGINES` row, never a subtype
    SVG = "svg"  # drawsvg named-layer `<g>` groups the export/layered OCG owner binds
    DXF = "dxf"  # ezdxf reusable-block document the CAD rail reads


# --- [MODELS] ---------------------------------------------------------------------------
class SymbolStyle(Struct, frozen=True):
    # the ONE drawing-plane mark-style owner (the ISO-grounded peer of `visualization/diagram/glyphset#GLYPHSET`
    # `GlyphStyle`) the future `drawing/annotate`/`detail` producers compose, never a parallel `AnnotateStyle`:
    # `fill`/`stroke` index the `hex_ramp` ramp, `layer`/`weight`/`text_height`/`terminator` compose the
    # `drawing/standard#STANDARD` owned ISO vocabulary so the pen traces to one discipline row.
    layer: LayerName
    fill: int = 0
    stroke: int = 0
    weight: LineWeight = LineWeight.W025          # ISO 128 line-weight group `drawing/standard#STANDARD` legislates
    text_height: TextHeight = TextHeight.H2_5     # ISO 3098 lettering height `drawing/standard#STANDARD` legislates
    terminator: Terminator = Terminator.FILLED_ARROW  # ISO 129-1/128-2 line-end the section/elevation tails draw


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
        match marks:  # the one modal-arity head — a lone mark is the singleton, an iterable the multi-mark sheet
            case SymbolKind():
                return cls(marks=(marks,), palette=palette, target=target)
            case _:
                return cls(marks=tuple(marks), palette=palette, target=target)

    async def render(self) -> RuntimeRail[tuple[tuple[Layer, ...], ArtifactReceipt]]:
        # the whole schemdraw/ezdxf/kiwisolver fold is synchronous native/CPU work, so it crosses one
        # `to_thread` seam off the event loop in the shared address space (the subinterpreter arm cannot load
        # the numpy palette / msgspec receipt owners), never inline on the loop.
        return await async_boundary(f"drawing.symbol.{self.target}", lambda: to_thread.run_sync(_ENGINES[self.target], self, limiter=_LANES))

    async def glyph(self, mark: SymbolKind, /) -> RuntimeRail[bytes]:
        # the symbol->sheet seam: ONE mark rasterized to PNG for a `composition/sheet#SHEET` `NorthArrow.glyph`
        # / `KeyPlan.figure` cell whose reportlab `ImageReader`/`drawImage` reads a raster, never an SVG. The
        # palette crosses the offload raw — `hex_ramp` runs inside `_raster`, never on the loop before the seam.
        return await async_boundary("drawing.symbol.glyph", lambda: to_thread.run_sync(_raster, mark, self.palette, limiter=_LANES))


# --- [OPERATIONS] -----------------------------------------------------------------------
def _style(mark: SymbolKind, /) -> SymbolStyle:
    match mark:  # every case's style is its last payload slot; one total projection, never a per-tag getattr
        case (
            SymbolKind(tag="section", section=(*_, style)) | SymbolKind(tag="elevation", elevation=(*_, style))
            | SymbolKind(tag="detail", detail=(*_, style)) | SymbolKind(tag="grid", grid=(*_, style))
            | SymbolKind(tag="matchline", matchline=(*_, style)) | SymbolKind(tag="north", north=(*_, style))
            | SymbolKind(tag="scale_bar", scale_bar=(*_, style)) | SymbolKind(tag="revision", revision=(*_, style))
            | SymbolKind(tag="keyplan", keyplan=(*_, style)) | SymbolKind(tag="datum", datum=(*_, style))
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
            SymbolKind(tag="section", section=(center, *_)) | SymbolKind(tag="elevation", elevation=(center, *_))
            | SymbolKind(tag="detail", detail=(center, *_)) | SymbolKind(tag="grid", grid=(center, *_))
            | SymbolKind(tag="north", north=(center, *_)) | SymbolKind(tag="scale_bar", scale_bar=(center, *_))
            | SymbolKind(tag="revision", revision=(center, *_)) | SymbolKind(tag="keyplan", keyplan=(center, *_))
            | SymbolKind(tag="datum", datum=(center, *_))
        ):
            return center
        case _ as unreachable:
            assert_never(unreachable)


def _extent(mark: SymbolKind, /) -> float:
    match mark:  # the mark's characteristic half-extent, the bbox and the DXF block signature read
        case (
            SymbolKind(tag="section", section=(_, radius, *_)) | SymbolKind(tag="elevation", elevation=(_, radius, *_))
            | SymbolKind(tag="detail", detail=(_, radius, *_)) | SymbolKind(tag="grid", grid=(_, radius, *_))
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
    # the `schemdraw` compound-symbol geometry with NAMED terminal anchors — one `ElementCompound` per marker
    # appending typed `Segment*` to `self.segments` and declaring the terminal `self.anchors` a
    # `drawing/annotate#FLAGNOTE`/`drawing/detail#CALLOUT` leader binds. TOTAL over the eleven-case family.
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
    # the shared section/detail bubble — a bisected circle carrying two stacked labels; the section/detail arms
    # differ only in their tail and named anchor, so the bisected core is one builder, not two copies.
    sym, (stroke, _fill, lw, size) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(segments.SegmentCircle((0.0, 0.0), radius, color=stroke, lw=lw))
    sym.segments.append(segments.Segment([(-radius, 0.0), (radius, 0.0)], color=stroke, lw=lw))
    sym.segments.append(segments.SegmentText((0.0, radius * 0.4), upper, align=("center", "center"), fontsize=size, color=stroke))
    sym.segments.append(segments.SegmentText((0.0, -radius * 0.4), lower, align=("center", "center"), fontsize=size, color=stroke))
    return sym


def _section_element(radius: float, detail_no: str, sheet_ref: str, bearing: float, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
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
    sym.segments.append(segments.SegmentPoly([(0.0, 0.0), (point[0] - flank, point[1]), (point[0] + flank, point[1])], closed=True, color=stroke, fill=fill))
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
    # the heavy dashed match line + its "MATCH LINE — SEE X" caption, drawn through `schemdraw`'s native
    # `lw`/`ls` stroke (the correct default) — a filled-band form composes the landed `graphic/vector#VECTOR` `outline`.
    sym, (stroke, _fill, lw, size) = elements.ElementCompound(), _pen(style, ramp)
    origin, path = vertices[0], [(vx - vertices[0][0], vy - vertices[0][1]) for vx, vy in vertices]
    sym.segments.append(segments.Segment(path, color=stroke, lw=lw * 4.0, ls="--"))
    sym.segments.append(segments.SegmentText(path[len(path) // 2], f"MATCH LINE — SEE {sheet_ref}", align=("center", "bottom"), fontsize=size, color=stroke))
    sym.anchors["match"] = path[-1]
    return sym


def _north_element(radius: float, bearing: float, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    # the north arrow as a filled north half PLUS a hollow south half rotated to `bearing`, the "N" above —
    # both halves and the label are drawn geometry, never a promised-but-dropped part.
    sym, (stroke, fill, _lw, size) = elements.ElementCompound(), _pen(style, ramp)
    rot, base = math.radians(bearing), radius * 0.42
    tip, tail = _rotate((0.0, radius), rot), _rotate((0.0, -radius * 0.25), rot)
    left, right = _rotate((-base, -radius * 0.55), rot), _rotate((base, -radius * 0.55), rot)
    sym.segments.append(segments.SegmentPoly([tip, left, tail], closed=True, color=stroke, fill=fill))   # filled north half
    sym.segments.append(segments.SegmentPoly([tip, right, tail], closed=True, color=stroke))             # hollow south half
    sym.segments.append(segments.SegmentText(_rotate((0.0, radius * 1.25), rot), "N", align=("center", "center"), fontsize=size, color=stroke))
    sym.anchors["north"] = tip
    return sym


def _scale_element(length: float, seg: int, units: str, ratio: str, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    # the divided graphic-scale ruler — the standalone twin of `composition/sheet#SHEET` `Scale.bar`.
    sym, (stroke, fill, lw, size) = elements.ElementCompound(), _pen(style, ramp)
    step, height = length / max(seg, 1), size
    for i in range(max(seg, 1)):  # Exemption: schemdraw builds a symbol by appending Segment* to the mutable self.segments; the alternating-fill ruler assembles in place
        sym.segments.append(segments.SegmentPoly(
            [(i * step, 0.0), ((i + 1) * step, 0.0), ((i + 1) * step, height), (i * step, height)],
            closed=True, color=stroke, lw=lw, fill=fill if i % 2 else None,
        ))
    sym.segments.append(segments.SegmentText((length / 2.0, -height), f"0 — {length:g} {units} ({ratio})", align=("center", "top"), fontsize=size, color=stroke))
    return sym


def _revision_element(size_: float, mark: str, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    sym, (stroke, _fill, lw, text) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(segments.SegmentPoly([(0.0, size_), (-size_ * 0.87, -size_ * 0.5), (size_ * 0.87, -size_ * 0.5)], closed=True, color=stroke, lw=lw))
    sym.segments.append(segments.SegmentText((0.0, -size_ * 0.05), mark, align=("center", "center"), fontsize=text, color=stroke))  # the revision number
    return sym


def _keyplan_element(extent: tuple[float, float], parcels: tuple[Box, ...], highlight: int, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    sym, (stroke, fill, lw, _size) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(segments.SegmentPoly([(0.0, 0.0), (extent[0], 0.0), (extent[0], extent[1]), (0.0, extent[1])], closed=True, color=stroke, lw=lw))
    for index, (x0, y0, x1, y1) in enumerate(parcels):  # Exemption: the parcel rectangles assemble onto the mutable self.segments
        sym.segments.append(segments.SegmentPoly([(x0, y0), (x1, y0), (x1, y1), (x0, y1)], closed=True, color=stroke, fill=fill if index == highlight else None))
    return sym


def _datum_element(size_: float, level: str, style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    # the ISO spot-level / benchmark marker — a filled level triangle carrying the RL/elevation value.
    sym, (stroke, fill, lw, text) = elements.ElementCompound(), _pen(style, ramp)
    sym.segments.append(segments.SegmentPoly([(0.0, 0.0), (-size_ * 0.5, size_), (size_ * 0.5, size_)], closed=True, color=stroke, fill=fill))
    sym.segments.append(segments.SegmentText((size_ * 0.8, size_ * 0.6), level, align=("left", "center"), fontsize=text, color=stroke))
    sym.anchors["level"] = (0.0, 0.0)
    return sym


def _breakline_element(vertices: tuple[Point, ...], style: SymbolStyle, ramp: list[str]) -> "elements.ElementCompound":
    # the ISO 128 break line — a polyline with a zigzag inserted at its midpoint marking a truncated view.
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
    # ONE self-contained SVG mark: the schemdraw compound renders font-independent through `use('svg')` +
    # `svgconfig.text='path'` (the bundled ziafont outlining each SegmentText), wrapped in a viewBox-framed
    # `<svg>` the layered container and the sheet-cell raster seam both read.
    use("svg")
    svgconfig.text = "path"
    element, center = _element(mark, ramp)
    with Schematic(show=False) as sheet:  # Exemption: schemdraw Drawing is a context manager finalizing the layout on __exit__
        sheet += element.at(center)
    return sheet.get_imagedata("svg")


def _raster(mark: SymbolKind, palette: Palette) -> Result[bytes, VectorFault]:
    # the sheet-cell seam: render ONE mark to SVG then rasterize to PNG through `graphic/vector#VECTOR`'s
    # landed resvg floor, the byte form the reportlab `ImageReader`/`drawImage` cell consumes; `hex_ramp`
    # runs here in the worker, never on the loop as a pre-offload argument expression.
    return rasterize(_svg_mark(mark, hex_ramp(palette)), _GLYPH)


def _grid_runs(indices: tuple[int, ...], anchors: tuple[Point, ...]) -> tuple[tuple[int, ...], ...]:
    # partition axis-aligned grid bubbles into collinear runs — a shared-y horizontal band and a shared-x vertical
    # band — so a TWO-AXIS structural grid (letters along X, numbers along Y) solves each axis independently rather
    # than collapsing both onto one diagonal line; each bubble joins whichever band holds more collinear peers.
    rows: dict[float, list[int]] = {}
    cols: dict[float, list[int]] = {}
    for index, (x, y) in zip(indices, anchors, strict=True):  # Exemption: the two-key co-grouping accumulates into local band dicts — the partition seam
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
    # each collinear grid run aligns blended-even through one kiwisolver.Solver: each bubble's position along its OWN
    # run is a `t` Variable, endpoints pinned `required`, given interior positions honoured `weak`, an even gap
    # preferred `medium`, a monotone min-separation kept `weak`; `updateVariables()` writes each solved `value()`
    # BACK into the re-keyed anchor. A two-axis grid solves its X-run and Y-run in separate independent solvers.
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
        solver.addConstraint((ts[0] == 0.0) | kiwisolver.strength.required)   # Exemption: kiwisolver Solver is the stateful native sink; constraints add in place
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
    # bucket the marks of one `SymbolStyle.layer` into a named `drawsvg.Group`; each mark rides as a
    # `drawsvg.Raw` self-contained fragment, the canvas sized to the real content bbox, never a 1x1 stub.
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
        for corner in ((_center(mark)[0] - _extent(mark), _center(mark)[1] - _extent(mark)), (_center(mark)[0] + _extent(mark), _center(mark)[1] + _extent(mark)))
    )
    return (min(c[0] for c in corners), min(c[1] for c in corners), max(c[0] for c in corners), max(c[1] for c in corners)) if corners else (0.0, 0.0, 1.0, 1.0)


def _dxf_block(block: object, mark: SymbolKind) -> object:
    # the parametric block geometry authored ONCE per (tag, extent) signature; a bubble number is an
    # `add_attdef` ATTRIB the `add_blockref` placement fills. TOTAL over the eleven-case family.
    match mark:
        case SymbolKind(tag="north", north=(_, radius, _bearing, _style)):
            # the filled north half + hollow south half at canonical bearing, matching the SVG two-tone arrow; the
            # solid hatch closes the SVG-vs-DXF fill parity gap (add_hatch + set_solid_fill over a closed boundary path).
            solid = block.add_hatch()  # Exemption: ezdxf Hatch is the native fill sink; the boundary path + solid fill mutate in place
            solid.paths.add_polyline_path([(0.0, radius), (-radius * 0.42, -radius * 0.55), (0.0, -radius * 0.25)], is_closed=True)
            solid.set_solid_fill()
            block.add_lwpolyline([(0.0, radius), (radius * 0.42, -radius * 0.55), (0.0, -radius * 0.25)], close=True)  # hollow south half
            block.add_attdef("N", (0.0, radius * 1.2))
        case SymbolKind(tag="datum", datum=(_, size, _level, _style)):
            # the ISO spot-level triangle FILLED to SVG parity (the SVG datum SegmentPoly fills) via a solid hatch,
            # its own triangle geometry + right-set MARK attdef (the prior shared revision arm authored both wrong).
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
            SymbolKind(tag="section", section=(_, radius, *_)) | SymbolKind(tag="elevation", elevation=(_, radius, *_))
            | SymbolKind(tag="detail", detail=(_, radius, *_)) | SymbolKind(tag="grid", grid=(_, radius, *_))
        ):  # the bubble family — a circle carrying its number ATTRIB
            block.add_circle((0.0, 0.0), radius)
            block.add_attdef("NUMBER", (0.0, 0.0))
        case _ as unreachable:
            assert_never(unreachable)
    return block


# --- [TABLES] ---------------------------------------------------------------------------
# ISO 129-1/128-2 line-end -> schemdraw arrow style; the section/elevation cutting-plane tails draw it.
_ARROW: frozendict[Terminator, str] = frozendict({
    Terminator.FILLED_ARROW: "->",
    Terminator.OPEN_ARROW: "->",
    Terminator.OBLIQUE_STROKE: "-|",
    Terminator.DOT: "-o",
    Terminator.NONE: "-",
})


def _svg_engine(symbol: Symbol) -> tuple[tuple[Layer, ...], ArtifactReceipt]:
    ramp = hex_ramp(symbol.palette)
    aligned = _grid_solve(symbol.marks)
    box = _bbox(aligned)
    groups: dict[str, list[bytes]] = {}
    for mark in aligned:  # Exemption: the drawsvg named-layer tree buckets marks by `SymbolStyle.layer` through a mutable dict of fragment lists, as visualization/diagram/draw#DRAW does
        groups.setdefault(_style(mark).layer.compose(), []).append(_svg_mark(mark, ramp))
    layers = tuple(Layer(name=name, source=_layer_svg(name, frags, box), bbox=box) for name, frags in sorted(groups.items()))
    key = ContentIdentity.of("drawing-symbol-svg", b"".join(layer.source for layer in layers))
    return layers, ArtifactReceipt.Drawing(key, "symbol", len(symbol.marks), "drawsvg", int(box[2] - box[0]), int(box[3] - box[1]), sum(len(layer.source) for layer in layers))


def _dxf_engine(symbol: Symbol) -> tuple[tuple[Layer, ...], ArtifactReceipt]:
    # each distinct (tag, extent) signature is authored ONCE as a `doc.blocks.new` block and placed by N
    # `add_blockref` under the `Standard.graphics(layer)` discipline pen — never a per-placement geometry copy.
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
    key = ContentIdentity.of("drawing-symbol-dxf", data)
    return (Layer(name="dxf", source=data, bbox=box),), ArtifactReceipt.Drawing(key, "symbol", len(symbol.marks), "ezdxf", int(box[2] - box[0]), int(box[3] - box[1]), len(data))


_ENGINES: frozendict[SymbolTarget, Callable[[Symbol], tuple[tuple[Layer, ...], ArtifactReceipt]]] = frozendict({
    SymbolTarget.SVG: _svg_engine,
    SymbolTarget.DXF: _dxf_engine,
})


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["Symbol", "SymbolKind", "SymbolStyle", "SymbolTarget"]
```

`Symbol` is the one drawing-symbol grammar every AEC marker is built from: the closed `SymbolKind` union (`Section`/`Elevation`/`Detail`/`Grid`/`MatchLine`/`North`/`ScaleBar`/`Revision`/`KeyPlan`/`Datum`/`BreakLine`) carries each mark's typed geometry and its `SymbolStyle`, and the `SymbolTarget` policy value selects the dual lowering — a `drawsvg` named-layer `Group` whose `schemdraw`-rendered fragments carry `ziafont`-outlined text, or an `ezdxf` reusable block authored once per geometry signature and placed by `add_blockref` under a `Standard.seed`-authored document. The compound geometry is `schemdraw` `ElementCompound` + `Segment*` + named `self.anchors` for ALL eleven marks — one total `_element` fold with no partial dispatch, the `North` south-half and the `Revision`/`MatchLine`/`Datum` captions all drawn geometry, and the DXF block marks mirror the SVG fills through `add_hatch().set_solid_fill()` — and each collinear grid run (a two-axis structural grid partitioned into its X-run and Y-run by `_grid_runs`) aligns through its own `kiwisolver.Solver` that pins endpoints, honours given positions, prefers an even gap, and writes each solved `value()` BACK into the re-keyed anchor. Every mark palette-indexes to the `hex_ramp` ramp and ISO-grounds its pen through `drawing/standard#STANDARD`'s `LayerName`/`LineWeight`/`TextHeight`/`Terminator`; the synchronous render offloads onto `to_thread` off the event loop and rails through `RuntimeRail`/`async_boundary`; and the `glyph` PNG projection feeds the `composition/sheet#SHEET` `NorthArrow.glyph`/`KeyPlan.figure` cells directly through `graphic/vector#VECTOR` `rasterize` — a raster seam that leaves `sheet.md`'s title-block value objects intact. The owner contributes one `ArtifactReceipt.Drawing` (kind/entity/extent/byte facts) on the shared receipt family and one `core/plan#PLAN` `ArtifactWork` node keyed by the content identity, composites nothing, and re-renders nothing.

## [03]-[RESEARCH]

- [SYMBOL_VOCABULARY] [RESOLVED]: the eleven `SymbolKind` cases span the AEC drawing-symbol vocabulary the brief `[04]` names (section/elevation/detail markers, grid bubbles, match lines, north arrow, graphic scale, revision triangle, key plan) PLUS the two the concept demands the brief list omits — `Datum` (the ISO spot-level/benchmark marker on every plan and section) and `BreakLine` (the ISO 128 truncated-view mark on every detail) — as ONE closed `expression.tagged_union` mirroring the `visualization/diagram/glyphset#GLYPHSET` bounded-vocabulary pattern, each case carrying its geometry plus a `SymbolStyle`, matched by one total `_element`/`_style`/`_center`/`_extent` fold, a new marker a case-plus-arm not a class. Justified on DOMAIN (the drawing-symbol vocabulary the brief legislates plus the two core markers it under-lists).
- [COMPOUND_GEOMETRY] [RESOLVED]: ALL eleven marks compose `schemdraw` `ElementCompound` + `Segment`/`SegmentCircle`/`SegmentArc`/`SegmentText`/`SegmentPoly` + named `self.anchors` (verified at `schemdraw.segments`/`schemdraw.elements.ElementCompound`), rendered font-independent through `use('svg')` + `svgconfig.text='path'` + `Drawing.get_imagedata('svg')` (verified; the standalone SVG backend needs no matplotlib and drives the bundled `ziafont` to outline each `SegmentText`). Routing every mark through one engine makes `_element` TOTAL over the family — the prior `_element`/`_vector_ops` split that routed reachable `north`/`revision`/`matchline` cases to `assert_never` (a partial dispatch dressed as total) is deleted. The `ezdxf` block path mirrors it: `doc.blocks.new` + `add_attdef` + `add_blockref` + `add_circle`/`add_lwpolyline` (all verified in the `ezdxf` `.api`), authored ONCE per `(tag, extent)` signature and placed N times — never a per-placement copy. Justified on PACKAGE (the verified `schemdraw`/`ezdxf` surfaces).
- [STANDARD_COMPOSITION] [RESOLVED]: `SymbolStyle` composes `drawing/standard#STANDARD`'s owned ISO vocabulary — `LayerName` (the ISO 13567/AIA codec whose `.compose()` binds the `drawsvg.Group` id and whose discipline pens the `ezdxf` `GfxAttribs` through `Standard.graphics`), `LineWeight` (ISO 128), `TextHeight` (ISO 3098), and `Terminator` (ISO 129-1/128-2) — rather than the prior bare-float `width`/`text_height`/`str` layer and a PARALLEL `Terminator(ARROW/DOT/SLASH/NONE)` vocabulary duplicating `standard.md`'s owned `Terminator`; the duplicate is collapsed into a `_ARROW` `frozendict[Terminator, str]` lowering the owned member to a `schemdraw` `arrow=` style. `SymbolStyle` is the ISO-grounded drawing-plane peer of the diagram-plane `GlyphStyle` (palette-indexed identical), the one mark-style owner the future `drawing/annotate`/`detail` producers compose. Justified on DOMAIN (the drawing plane composes the owned drafting substrate) and CONSUMER (the shared mark-style contract).
- [SHEET_SEAM] [RESOLVED]: the `glyph` projection rasterizes ONE mark's SVG to PNG through the LANDED `graphic/vector#VECTOR` `rasterize` (`resvg`) + `RenderPolicy` (both verified on that owner's `__all__`), because `composition/sheet#SHEET`'s STUB graphic cells are `NorthArrow.glyph: bytes` and `KeyPlan.figure: bytes` (verified) consumed by a `reportlab` `ImageReader(BytesIO(...))` + `drawImage(..., mask="auto")` that reads a RASTER — the prior page's raw-SVG `glyph` bytes the raster loader cannot open. The seam is exactly those two byte cells: `sheet.md`'s `Scale.bar(x, y)` returns divided-ruler GEOMETRY (not bytes) and there is no `RevisionTriangle` cell, so the prior seam prose naming `Scale.bar`/`RevisionTriangle` byte cells was a MODEL-COHERENCE defect. This is a SEAM, not an absorb: `sheet.md` keeps its `NorthArrow`/`KeyPlan` value objects. Recorded as the `drawing/symbol → composition/sheet` `[SHEET]` seam in `ARCHITECTURE.md` `[02]-[SEAMS]`. Justified on CONSUMER (the sheet raster-cell PNG contract).
- [GRID_ALIGNMENT] [RESOLVED]: a grid-bubble run aligns collinear and blended-even through one REAL `kiwisolver.Solver` (verified `Solver`/`Variable`/`addConstraint`/`updateVariables`/`Variable.value`/`strength.required`/`weak`/`medium`) — the endpoints pinned `required`, each given interior position honoured `weak`, an even gap preferred `medium`, a monotone min-separation kept `weak`, and `updateVariables()` writing each solved `value()` BACK into the mark's re-keyed anchor. The prior `_grid_solve` added only meaningless `weak` equal-gap constraints, pinned no endpoint, and `return marks` UNCHANGED (a total no-op whose comment lied about a re-key it never performed); this solve blends given positions toward even spacing and the read-back is the correctness fix. `_grid_runs` now partitions the grid bubbles into collinear runs (a shared-y horizontal band + a shared-x vertical band) so a TWO-AXIS structural grid (letters along X, numbers along Y) solves each axis in its OWN independent `Solver` rather than collapsing both onto one `anchors[0]→anchors[-1]` diagonal — the multi-run generalization the reading map named as growth, and a real correctness fix for the two-axis case the single-run form mis-solved. All required constraints are consistent (two endpoint pins per run), so each run is always satisfiable and no `UnsatisfiableConstraint` arises. Justified on PACKAGE (the verified `kiwisolver` surface) and DOMAIN (a structural column grid is two-axis; collinear even-spacing is the AEC convention).
- [RECEIPT_AND_PLAN] [RESOLVED]: the owner contributes one `ArtifactReceipt.Drawing(key, kind, entities, dimstyle, width, height, bytes_)` case on the shared `core/receipt#RECEIPT` family — the case ALREADY exists there with that shape (verified `Drawing(cls, key, kind, entities, dimstyle, width, height, bytes_)` and `_KEYS["drawing"] = ("kind", "entities", "dimstyle", "width", "height", "bytes")`), so the prior page's `Drawing(key, kind, marks, layers, bytes, engine)` 6-arg call and its RESEARCH claim that the case "must be added" were a MODEL-COHERENCE phantom. This owner fills `kind="symbol"`, `entities` the mark count, the shared `dimstyle` str slot with the render-engine descriptor (`"drawsvg"`/`"ezdxf"` — a symbol carries no dimension style; `drawing/dimension#DIMENSION` fills that slot with the ISO 129-1 style name), `width`/`height` the bbox extent, and `bytes_` the byte count. The `render` coroutine is the `Work[ArtifactReceipt]` thunk one `core/plan#PLAN` `ArtifactWork(key, work, parents, admission, cost)` node schedules, keyed by the content identity the receipt carries. Justified on CONSUMER (the settled shared `Drawing` case and `ArtifactWork` node).
- [VECTOR_BOOLEAN_OFFSET] [RESOLVED]: `graphic/vector#VECTOR`'s `VectorOp` NOW carries `Boolean` and `Outline` cases (its family is `Transform`/`Bounds`/`Fit`/`Serialize`/`Rasterize`/`Measure`/`Sample`/`Flatten`/`Subpaths`/`Project`/`Boolean`/`Outline`/`Region`, with public `boolean(sources, op, fill)`/`outline(source, width, cap, join, miter, dash)`/`region(source)` functions and `VectorResult.raster`/`document` outcomes), so a `VectorOp.Boolean`/`VectorOp.Outline` composition would resolve today; the one still-rejected phantom is the OLD `Vector.over(ops)._worked(ops)` private-method reach (`_worked` is a private MODULE fold, not a `Vector` method). This owner builds the north arrow, the revision triangle, and the match line from `schemdraw` primitives (a filled north half PLUS a hollow south half, a closed `SegmentPoly`, a heavy dashed `Segment`) as the correct self-contained default. The reading-map "single-silhouette north union" is REJECTED, not deferred: the ISO north arrow is TWO-TONE (a filled north half and a hollow south half), so a `VectorOp.Boolean(UNION)` merging them into one silhouette would REGRESS the two-tone reading — the schemdraw two-poly is the correct form, not a naive approximation. The genuinely-available `skia-pathops` REFINEMENTS that do NOT regress — a filled-band / double-line match line and a revision-cloud offset — compose the LANDED `graphic/vector#VECTOR` `outline`/`boolean` (verified `boolean(sources, op, fill)`/`outline(source, width, cap, join, miter, dash)` on that owner's public surface) directly at the SVG-bytes seam, shared with `drawing/annotate#ANNOTATE`/`drawing/dimension#DIMENSION`, never authored twice.
- [DXF_FILL_PARITY] [RESOLVED]: the `_dxf_block` marks rendered OUTLINE-ONLY (`add_lwpolyline`/`dxfpath.render_lines`) where the SVG arm FILLS — the north filled half and the datum triangle — so the DXF egress underperformed the SVG at cross-egress parity. `_dxf_block` now fills them via `block.add_hatch().paths.add_polyline_path(pts, is_closed=True)` + `set_solid_fill()` (verified: `add_hatch` returns a `Hatch` with `set_solid_fill(color=7, style=1, rgb=None)` and `paths.add_polyline_path(is_closed=)`), and the prior shared `revision|datum` arm — which authored BOTH with the revision triangle and the `(0.0, -size*0.05)` attdef (wrong for datum) — SPLITS into a filled datum arm (its own `[(0,0),(-size/2,size),(size/2,size)]` triangle, right-set MARK attdef) and an outline revision arm (its own triangle, matching the SVG which leaves the revision triangle unfilled). The `dxfpath` import is dropped (the decorative 4-point `star` north is replaced by the SVG-matching two-tone arrow). Justified on PACKAGE (the verified `Hatch.set_solid_fill`/`add_polyline_path` surface) and DOMAIN (SVG↔DXF cross-egress fill parity — a symbol reads the same in both rails).
