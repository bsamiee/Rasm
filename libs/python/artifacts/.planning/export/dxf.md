# [PY_ARTIFACTS_DXF]

`Dxf` owns the CAD-exchange editable hand-off — ONE owner over the closed `DxfOp` `tagged_union` — `New` authors a fresh document from a `DxfDocument` spec, `Read` ingests a conforming DXF, `Recover` salvages a damaged one, `Render` lowers a DXF figure into the composition/graphic plane over the in-process backend family, `Query` extracts an attribute/spatial sub-selection, `Transform` applies an affine, `Bridge` crosses the DXF↔SVG↔GeoJSON↔glyph geometry wire — each arm a typed payload dispatched by one total `match` and folded ONCE into a `DxfComposed` evidence struct the `of`/`contribute` projections share. `ezdxf` (pure-Python, no interpreter gate) is the sole owner of the DXF R12→R2018 read/write/recover/render surface, so this owner re-implements no tag grammar, OCS/WCS transform, B-spline evaluator, or entity-to-SVG conversion.

`RuntimeRail[ArtifactReceipt]` is the rail the `runtime/faults#FAULTS` owner legislates, minted ONCE at `async_boundary(catch=_FAULTS)` over the real engine raise tuple, each raise discriminated into its own `BoundaryFault` case; the pre-run `_key` mints the `ContentKey` over the canonical op payload before the fold. `ezdxf` defers through a module-scope `lazy` import and the whole fold offloads through `LanePolicy.offload(modality=Modality.THREAD, retry=RetryClass.OCCT)` — thread-carried because the fold returns the `msgspec`-backed `DxfComposed` a subinterpreter cannot load the C-extension for and the GIL-releasing PyMuPDF/Matplotlib render shares the address space, so a per-arm process split is impossible; a transient `OSError` re-arms on the retry row before the boundary converts a persistent one, cancellation excluded. `Render` lowers into `composition/sheet#SHEET` (the one-page PDF the `show_pdf_page` seam places) and `graphic/vector/region#REGION` (the framed SVG the placement owner composites); `Bridge` meets `graphic/vector/region#REGION` at the vertex/`d`-string wire, imported one hop as `svg_frame`. Every op contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Cad` case off the one fold and one `core/plan#PLAN` `ArtifactWork` node.

## [01]-[INDEX]

- [01]-[DXF]: the CAD-exchange owner over the closed `DxfOp` `tagged_union` (`New`/`Read`/`Recover`/`Render`/`Query`/`Transform`/`Bridge`) folded once into `DxfComposed`, thread-offloaded and rail-typed, `ezdxf` the sole read/write/recover/render surface.

## [02]-[DXF]

- Owner: `Dxf` holds `op: DxfOp` and discriminates over the closed `tagged_union`, every arm folding once into `DxfComposed`. `DxfDocument` is the New-arm authoring spec admitting the whole document graph (version/units/setup, `TableEntry` rows, `Xref` references, `BlockDef` blocks, modelspace `DxfEntity` drawables, `DxfFormat` egress). `DxfEntity` is the closed drawable vocabulary; each case bundles a shared `DxfAttribs` whose `.gfx()` projects the ONE `GfxAttribs(...).asdict()` payload the whole `add_*` family takes, so a layer/color/linetype change is one field, never a per-entity setter. `Hatch`'s `HatchFill` sub-axis routes SOLID/PATTERN/GRADIENT through `set_solid_fill`/`set_pattern_fill`/`set_gradient` (the ISO 128-50 material-indication fill `drawing/standard` supplies the `tools.pattern.ISO_PATTERN` definition for); `MultiLeader`'s `MLeaderKind` selects the mtext/block builder; `Dimension`'s `DimKind` keys the `_DIM` table to the matching `add_*_dim` builder. `TableEntry` is the `Layer`/`Linetype`/`Textstyle`/`Dimstyle` symbol-table family the `drawing/standard` ISO 128/3098/129-1/13567 vocabularies lower computed rows onto — dxf owns the table-authoring shape, the ISO semantics stay their owners. `Xref` binds an external drawing by reference through `xref.attach`. `DxfSource` is the closed `Blob`/`File`/`Zip`/`Base64` ingestion the conforming `_ingest` and salvage `_recovered` folds route. `Spatial` is the closed `Window`/`Circle`/`Polygon`/`Fence`/`Point` family the `_SPATIAL_TEST` table routes to the rtree-backed `select.*` predicate.
- Cases: `New` authors from a `DxfDocument` (`ezdxf.new` + the builder folds + `doc.audit()` before egress); `Read` ingests a CONFORMING DXF and re-emits the audit-clean bytes, a malformed source raising the `DXFStructureError` `_FAULTS` admits; `Recover` SALVAGES a damaged file through `recover.readfile`/`read` (the only correct loader for non-conforming DXF), the `auditor.errors`/`fixes` counts riding the `Cad` receipt as honest salvage evidence; `Render` drives the `Frontend`+backend stack selecting the sink (`SVGBackend` for `region#REGION`, `PyMuPdfBackend` for the `sheet#SHEET` PDF and the raster preview, `MatplotlibBackend` for EPS/PS, `CustomJSONBackend`/`GeoJSONBackend` for structured export); `Query` runs `doc.query` refined by the optional `_spatial` fold and copies matches through `addons.Importer`; `Transform` applies a `Matrix44` affine through `transform.inplace`/`copies` under the `TransformMode`; `Bridge` crosses five geometry wires on one `BridgeSpec` family — `ToSvg` folds `path.make_path`/`flattening` over the `_PATH_TYPES`-filtered drawables into a `(N,3)` numpy lane the `region#REGION` `document` owner frames ONCE, `FromSvg` crosses vertex rings through `from_vertices`/`render_lines`, `ToGeoJson`/`FromGeoJson` cross the `addons.geo.GeoProxy` wire, `TextPaths` crosses `addons.text2path` glyph outlines — neither path owner re-implementing the other's geometry.
- Auto: `_composed(op) -> DxfComposed` is the ONE total `match` both `of` and `contribute` read — no second render, no `@cache` memo standing in for the fold. `_build_entity` folds each `DxfEntity` onto its `add_*` builder with the shared `attribs.gfx()` payload; `_table_entry` lowers each `TableEntry` onto its `doc.<table>.add`. ezdxf's `Drawing` is pure-Python and GC-safe (no native handle to bracket, unlike the sibling `pymupdf` documents), so `_serialize` writes one `io.StringIO`/`BytesIO`/`encode_base64` egress; the render backends open and close their own native `pymupdf`/`matplotlib` handles.
- Output: `DxfComposed` is the one evidence struct — the serialized `data`, the `kind` format discriminant, and the `dxfversion`/`units`/`counts`/`layers`/`blocks`/`errors`/`fixes`/`extent` CAD evidence, the `Auditor` counts non-zero only for a salvaged `Recover`.
- Receipt: each op contributes ONE `ArtifactReceipt.Cad` case off the one fold through the receipt owner's named flat-scalar mint — the `dxfversion`, units, `artifact` format (from `composed.kind`), byte count, layer/block roster counts, `Auditor` error+fix counts, and the `Counter(dxftype)` census. `contribute` re-enters the SAME `_composed` fold and mints the content key over `composed.data`; a failed production raises the ezdxf/backend fault the `async_boundary` converts, never a zero-byte placeholder.
- Packages: `ezdxf` owns the read/write/recover/audit surface, the `add_*` builder family + `GfxAttribs`/`colors.RGB`, the hatch-fill and multileader builders, `xref.attach`, the `ezdxf.path` algebra, the `ezdxf.math` kernel, the `addons.drawing` render stack, the `query`/`groupby`/`select`/`bbox` read side, and the `addons.geo`/`text2path`/`Importer` boundary surfaces; `matplotlib` (`figure.Figure` + `savefig`, the GC-safe EPS/PS sink, no `pyplot` global); `graphic/vector/region#REGION` (`document`, the SVG-framing egress the `ToSvg` bridge composes one hop as `svg_frame`); `numpy` (the `(N,3)` vertex lane); `expression`/`msgspec` (the unions and value objects, the GeoJSON `json.encode`/`decode` wire); `beartype` (the `_GUARD` scalar contract); runtime (`identity`, `faults`, `lanes`); `core/receipt#RECEIPT`/`core/plan#PLAN`.
- Growth: a new DXF version is one `DxfVersion` member; a new drawable is one `DxfEntity` case plus one `_build_entity` arm (the `assert_never` tail breaking the fold at type-check); a new hatch fill is one `HatchFill` case; a new dimension kind is one `DimKind` member plus one `_DIM` row; a new symbol-table row is one `TableEntry` case plus one `_table_entry` arm; a new ingestion source is one `DxfSource` case; a new render backend or format is one `DxfBackend`+`DxfArtifact` member plus one `_rendered` arm; a new egress encoding (the `r12writer` streaming fast-writer) is one `DxfFormat` member plus one `_serialize` arm; a new spatial refinement is one `Spatial` case; a new render policy is one `RenderPolicy` field; a new affine mode is one `TransformMode` member; a new bridge direction is one `BridgeSpec` case over the existing `ezdxf.addons` surface; a new query refinement (`groupby`, the `EntityQuery` set-algebra) is one `Selection` field; a new receipt scalar is one `Cad` slot; a new engine raise is one type in the `_FAULTS` tuple. Zero new surface.
- Boundary: a hand-assembled DXF tag stream where the `add_*` family exists; a per-entity `set_layer`/`set_color` setter where the uniform `dxfattribs=` axis applies; a re-implemented affine/B-spline/OCS transform where `ezdxf.math` owns it; a hand-rolled `<svg>` wrapper where the `region#REGION` `document` owner frames the fragment stream; a foreign DXF renderer where the `Frontend`+backend family renders; a `find`/`get_by_layer` query family where `doc.query`/`select` discriminate; the conforming `readfile` on a damaged file where `recover.readfile` is the salvage path; a parallel `DxfFault` `Literal` the boundary never reads where the Auditor evidence rides the `Cad` receipt and the hard raise crosses `_FAULTS`. `csharp:Rasm.Bim` holds the IFC semantic model; the AEC standards vocabularies (ISO 128/129-1/3098/13567) stay the future `drawing/standard` owners that lower onto `TableEntry`; SVG framing meets `graphic/vector/region#REGION` at the `document`/`d`-string wire; PDF page assembly stays `composition/sheet#SHEET`/`composition/imposition#IMPOSE`; font shaping stays `typography` (`text2path` composes `fonttools`); the geospatial CRS authority stays the geospatial owner (`addons.geo` holds only the DXF↔GeoJSON conversion); identity minting the runtime owns.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import base64
import io
import zipfile
from collections import Counter
from collections.abc import Callable, Iterable, Iterator
from enum import IntEnum, StrEnum
from math import tau
from pathlib import Path
from typing import TYPE_CHECKING, Annotated, Literal, assert_never

import msgspec
import numpy as np
from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.graphic.vector.region import document as svg_frame  # the graphic/vector/region#REGION framing egress the ToSvg bridge composes one hop

# `DXFError` is the `ezdxf` error base, `Exception`-derived (NOT a stdlib subclass), so it imports eagerly
# for the `_FAULTS` tuple while the heavy `ezdxf` surface stays lazy behind the module boundary.
from ezdxf import DXFError

lazy import ezdxf
lazy from ezdxf import bbox, colors, recover, select, transform, xref
lazy from ezdxf import path as dxfpath
lazy from ezdxf.addons import Importer
lazy from ezdxf.addons import geo as dxfgeo
lazy from ezdxf.addons import text2path
lazy from ezdxf.addons.drawing import Frontend, RenderContext
lazy from ezdxf.addons.drawing import config as dwgconfig
lazy from ezdxf.addons.drawing import layout as dwglayout
lazy from ezdxf.addons.drawing.json import CustomJSONBackend, GeoJSONBackend
lazy from ezdxf.addons.drawing.matplotlib import MatplotlibBackend
lazy from ezdxf.addons.drawing.pymupdf import PyMuPdfBackend
lazy from ezdxf.addons.drawing.svg import SVGBackend
lazy from ezdxf.fonts.fonts import FontFace
lazy from ezdxf.gfxattribs import GfxAttribs
lazy from ezdxf.math import Matrix44, Vec3
lazy from ezdxf.render import mleader
lazy from matplotlib.figure import Figure

if TYPE_CHECKING:
    from ezdxf.document import Drawing
    from ezdxf.entities import DXFGraphic
    from ezdxf.layouts import BlockLayout, Modelspace
    from ezdxf.query import EntityQuery

    from rasm.runtime.receipts import Receipt


# --- [TYPES] ----------------------------------------------------------------------------
type Point2 = tuple[float, float]
type Point3 = tuple[float, float, float]
type Extent = tuple[float, float, float, float, float, float]  # (min x, y, z, max x, y, z) bbox AABB
type Attribs = dict[str, object]  # the `GfxAttribs.asdict()` `dxfattribs=` payload
type Builder = Callable[["Modelspace | BlockLayout", tuple[Point3, ...], str, Attribs], object]  # a `_DIM` dimension builder
type Positive = Annotated[float, Is[lambda value: value > 0.0]]
type NonNegative = Annotated[float, Is[lambda value: value >= 0.0]]
type PositiveInt = Annotated[int, Is[lambda value: value > 0]]
type PatternLine = tuple[float, Point2, Point2, tuple[float, ...]]  # ezdxf hatch pattern-line def: (angle, base, offset, dash items)
type LeaderLine = tuple[LeaderSide, tuple[Point3, ...]]  # one multileader leader: (dogleg attachment side, vertices)

# real ezdxf/backend raise tuple: `DXFError` a malformed read/bad builder call, `RuntimeError` the native render,
# `ValueError` a matplotlib/msgspec.json/EQL raise, `KeyError` a `_DIM`/`_SPATIAL_TEST` miss, `OSError` a
# font/resource/stream fault, `BeartypeCallHintViolation` the `_admit` non-positive reject — each its own `BoundaryFault`.
_FAULTS: tuple[type[BaseException], ...] = (DXFError, RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)


class DxfVersion(StrEnum):  # the `ezdxf.new(dxfversion=)` target
    R12 = "AC1009"
    R2000 = "AC1015"
    R2004 = "AC1018"
    R2007 = "AC1021"
    R2010 = "AC1024"
    R2013 = "AC1027"
    R2018 = "AC1032"


class DxfUnits(IntEnum):  # the `ezdxf.units` InsertUnits `doc.units` value (drawing insert-units)
    UNITLESS = 0
    INCH = 1
    FOOT = 2
    MILLIMETER = 4
    CENTIMETER = 5
    METER = 6
    KILOMETER = 7


class DxfFormat(StrEnum):  # the egress encoding as a policy value
    ASC = "asc"  # ascii DXF (`doc.write` text stream)
    BIN = "bin"  # binary DXF (`doc.write(fmt="bin")`)
    BASE64 = "b64"  # `doc.encode_base64()` blob


class DxfBackend(StrEnum):  # the `Render` sink — in-process backends, each member its own ezdxf sink
    SVG = "svg"  # SVGBackend.get_string
    PDF = "pdf"  # PyMuPdfBackend.get_pdf_bytes
    PNG = "png"  # PyMuPdfBackend.get_pixmap_bytes
    EPS = "eps"  # MatplotlibBackend + Figure.savefig
    PS = "ps"  # MatplotlibBackend + Figure.savefig
    JSON = "json"  # CustomJSONBackend.get_string
    GEOJSON = "geojson"  # GeoJSONBackend.get_string


class DxfArtifact(StrEnum):  # the `DxfComposed.kind` format discriminant the `Cad` receipt carries
    DXF = "dxf"
    SVG = "svg"
    PDF = "pdf"
    PNG = "png"
    EPS = "eps"
    PS = "ps"
    JSON = "json"
    GEOJSON = "geojson"


class DimKind(StrEnum):  # the ISO 129-1 dimension family the `_DIM` table routes to `add_*_dim`
    LINEAR = "linear"
    ALIGNED = "aligned"
    ANGULAR = "angular"
    RADIUS = "radius"
    DIAMETER = "diameter"
    ORDINATE = "ordinate"
    ARC = "arc"


class SpatialTest(StrEnum):  # the area-shape membership test the `_SPATIAL_TEST` table keys to a `select` predicate
    INSIDE = "inside"  # select.bbox_inside — the shape fully contains the entity bbox
    OUTSIDE = "outside"  # select.bbox_outside — the entity bbox lies fully outside the shape
    OVERLAP = "overlap"  # select.bbox_overlap — the entity bbox intersects the shape


class BridgeSample(StrEnum):  # the `ToSvg` curve-sampling mode over the `ezdxf.path.Path`
    FLATTEN = "flatten"  # Path.flattening(distance) — adaptive polyline for a toolpath/offset consumer
    CONTROL = "control"  # Path.control_vertices() — the exact NURBS control frame


class MLeaderKind(StrEnum):  # multileader content source — selects `add_multileader_mtext` vs `add_multileader_block`
    MTEXT = "mtext"
    BLOCK = "block"


class LeaderSide(StrEnum):  # `render.mleader.ConnectionSide` — values match the ezdxf member names for by-value derivation
    LEFT = "left"
    RIGHT = "right"
    TOP = "top"
    BOTTOM = "bottom"


class TransformMode(StrEnum):  # `transform.inplace` (mutate) vs `transform.copies` (duplicate) — the affine application mode
    INPLACE = "inplace"
    COPIES = "copies"


# owned render-policy vocabularies mirroring the `ezdxf.addons.drawing.config` enums by member name, so the payload
# stays ezdxf-free and `_rendered` derives each ezdxf enum via `getattr(<policy>, member.name)`.
class BackgroundPolicy(StrEnum):  # render canvas background
    DEFAULT = "default"
    WHITE = "white"
    BLACK = "black"
    PAPERSPACE = "paperspace"
    MODELSPACE = "modelspace"
    OFF = "off"
    CUSTOM = "custom"


class LineweightPolicy(StrEnum):  # lineweight resolution — ABSOLUTE honors ISO 128 mm lineweight groups (AEC plot fidelity)
    ABSOLUTE = "absolute"
    RELATIVE = "relative"
    RELATIVE_FIXED = "relative_fixed"


class HatchPolicy(StrEnum):  # hatch rendering — solid/outline/approximate-pattern vs ignore
    NORMAL = "normal"
    IGNORE = "ignore"
    SHOW_OUTLINE = "show_outline"
    SHOW_SOLID = "show_solid"
    SHOW_APPROXIMATE_PATTERN = "show_approximate_pattern"


class TextPolicy(StrEnum):  # text rendering fidelity — filled glyphs, outlines, or bounding-rect placeholders
    FILLING = "filling"
    OUTLINE = "outline"
    REPLACE_RECT = "replace_rect"
    REPLACE_FILL = "replace_fill"
    IGNORE = "ignore"


class ColorPolicy(StrEnum):  # color resolution — MONOCHROME / COLOR_SWAP_BW for monochrome plot sheets
    COLOR = "color"
    COLOR_SWAP_BW = "color_swap_bw"
    COLOR_NEGATIVE = "color_negative"
    MONOCHROME = "monochrome"
    MONOCHROME_DARK_BG = "monochrome_dark_bg"
    MONOCHROME_LIGHT_BG = "monochrome_light_bg"
    BLACK = "black"
    WHITE = "white"
    CUSTOM = "custom"


class ProxyPolicy(StrEnum):  # proxy-graphic handling — derived onto ezdxf `ProxyGraphicPolicy`
    IGNORE = "ignore"
    SHOW = "show"
    PREFER = "prefer"


class LinePolicy(StrEnum):  # linetype rendering accuracy
    SOLID = "solid"
    APPROXIMATE = "approximate"
    ACCURATE = "accurate"


# --- [CONSTANTS] ------------------------------------------------------------------------
_ZERO_EXTENT: Extent = (0.0, 0.0, 0.0, 0.0, 0.0, 0.0)
# the dxftypes `make_path`/`addons.geo.proxy` convert — a TOTAL predicate the bridge folds filter on so a
# non-convertible `Text`/`Insert` never reaches the raise.
_PATH_TYPES: frozenset[str] = frozenset({"LINE", "ARC", "CIRCLE", "ELLIPSE", "SPLINE", "LWPOLYLINE", "POLYLINE", "HATCH"})
_GEO_TYPES: frozenset[str] = frozenset({"LINE", "ARC", "CIRCLE", "ELLIPSE", "SPLINE", "LWPOLYLINE", "POLYLINE", "POINT"})
_UNITS: frozendict[DxfUnits, str] = frozendict({unit: unit.name.lower() for unit in DxfUnits})
# the ISO 129-1 dimension family -> the matching `add_*_dim` builder; each returns a `DimStyleOverride` whose `.render()` generates the geometry.
_DIM: frozendict[DimKind, Builder] = frozendict({
    DimKind.LINEAR: lambda msp, pts, ds, at: msp.add_linear_dim(base=pts[0], p1=pts[1], p2=pts[2], dimstyle=ds, dxfattribs=at),
    DimKind.ALIGNED: lambda msp, pts, ds, at: msp.add_aligned_dim(p1=pts[0], p2=pts[1], distance=pts[2][0], dimstyle=ds, dxfattribs=at),
    DimKind.ANGULAR: lambda msp, pts, ds, at: msp.add_angular_dim_2l(
        base=pts[0], line1=(pts[1], pts[2]), line2=(pts[3], pts[4]), dimstyle=ds, dxfattribs=at
    ),
    DimKind.RADIUS: lambda msp, pts, ds, at: msp.add_radius_dim(center=pts[0], mpoint=pts[1], dimstyle=ds, dxfattribs=at),
    DimKind.DIAMETER: lambda msp, pts, ds, at: msp.add_diameter_dim(center=pts[0], mpoint=pts[1], dimstyle=ds, dxfattribs=at),
    DimKind.ORDINATE: lambda msp, pts, ds, at: msp.add_ordinate_x_dim(feature_location=pts[0], offset=pts[1], dimstyle=ds, dxfattribs=at),
    DimKind.ARC: lambda msp, pts, ds, at: msp.add_arc_dim_3p(base=pts[0], center=pts[1], p1=pts[2], p2=pts[3], dimstyle=ds, dxfattribs=at),
})
# the area-test row -> the rtree-backed `select` predicate; fence/point cross their own `select` functions.
_SPATIAL_TEST: frozendict[SpatialTest, Callable[[object, "EntityQuery"], "Iterable[DXFGraphic]"]] = frozendict({
    SpatialTest.INSIDE: lambda shape, entities: select.bbox_inside(shape, entities),
    SpatialTest.OUTSIDE: lambda shape, entities: select.bbox_outside(shape, entities),
    SpatialTest.OVERLAP: lambda shape, entities: select.bbox_overlap(shape, entities),
})


# --- [MODELS] ---------------------------------------------------------------------------
class DxfAttribs(Struct, frozen=True):
    # `.gfx()` is the uniform `dxfattribs=` payload; the `drawing/standard` codecs supply `layer`/`linetype` as data.
    layer: str = "0"
    color: int = 256  # ACI; 256=ByLayer, 0=ByBlock, 1-255 indexed
    rgb: tuple[int, int, int] | None = None  # true-color; None = ACI `color`
    linetype: str = "ByLayer"
    lineweight: int = -1  # -1=ByLayer, -3=default
    transparency: float | None = None
    ltscale: float = 1.0

    def gfx(self) -> Attribs:
        return GfxAttribs(
            layer=self.layer,
            color=self.color,
            rgb=colors.RGB(*self.rgb) if self.rgb is not None else None,
            linetype=self.linetype,
            lineweight=self.lineweight,
            transparency=self.transparency,
            ltscale=self.ltscale,
        ).asdict()


class Xref(Struct, frozen=True):  # an external DXF reference `xref.attach` binds into the authored document
    block_name: str
    filename: str
    insert: Point3 = (0.0, 0.0, 0.0)
    scale: float = 1.0
    rotation: float = 0.0


class RenderPolicy(Struct, frozen=True):  # the ezdxf render-policy bundle as POLICY_VALUES over owned enums
    background: BackgroundPolicy = BackgroundPolicy.WHITE
    lineweight: LineweightPolicy = LineweightPolicy.ABSOLUTE  # ABSOLUTE honors ISO 128 mm lineweight groups
    hatch: HatchPolicy = HatchPolicy.NORMAL
    text: TextPolicy = TextPolicy.FILLING
    color: ColorPolicy = ColorPolicy.COLOR
    proxy: ProxyPolicy = ProxyPolicy.SHOW
    line: LinePolicy = LinePolicy.ACCURATE
    ctb: str = ""  # `RenderContext(ctb=)` — the CTB/STB plot-style table path (pen/lineweight/color mapping)
    export_mode: bool = False  # `RenderContext(export_mode=)` — paperspace export vs on-screen display resolution


class PageSpec(Struct, frozen=True):  # the `ezdxf.addons.drawing.layout.Page`/`Settings` + `Configuration`/`RenderContext` render model
    width: float = 0.0  # 0 auto-detects from extents
    height: float = 0.0
    margin: float = 10.0
    dpi: int = 300
    fit_page: bool = True
    scale: float = 1.0
    policy: RenderPolicy = RenderPolicy()  # the full render-policy bundle (background/lineweight/hatch/text/color/proxy/line + ctb + export_mode)


class DxfComposed(Struct, frozen=True):  # the one evidence struct of/contribute read
    data: bytes
    kind: DxfArtifact = DxfArtifact.DXF  # the format the bytes ARE, riding the `Cad` receipt's `artifact` slot
    dxfversion: str = ""
    units: str = ""
    counts: frozendict[str, int] = frozendict()  # `Counter(dxftype)` entity-census map
    layers: int = 0
    blocks: int = 0
    errors: int = 0  # `Auditor.errors` count (salvage residual)
    fixes: int = 0  # `Auditor.fixes` count
    extent: Extent = _ZERO_EXTENT  # `bbox.extents` model AABB


@tagged_union(frozen=True)
class TableEntry:
    tag: Literal["layer", "linetype", "textstyle", "dimstyle"] = tag()
    layer: tuple[str, int, str, int] = case()  # (name, color, linetype, lineweight) — ISO 13567 layer codec target
    linetype: tuple[str, tuple[float, ...], str] = case()  # (name, dash/gap pattern, description) — ISO 128 target
    textstyle: tuple[str, str, float, float] = case()  # (name, font, height, width) — ISO 3098 lettering target
    dimstyle: tuple[str, frozendict[str, float]] = case()  # (name, dxf-attr overrides) — ISO 129-1 style target

    @staticmethod
    def Layer(name: str, color: int = 7, linetype: str = "Continuous", lineweight: int = -3) -> "TableEntry":
        return TableEntry(layer=(name, color, linetype, lineweight))

    @staticmethod
    def Linetype(name: str, pattern: tuple[float, ...], description: str = "") -> "TableEntry":
        return TableEntry(linetype=(name, pattern, description))

    @staticmethod
    def Textstyle(name: str, font: str = "isocp.shx", height: float = 0.0, width: float = 1.0) -> "TableEntry":
        return TableEntry(textstyle=(name, font, height, width))

    @staticmethod
    def Dimstyle(name: str, overrides: frozendict[str, float] = frozendict()) -> "TableEntry":
        return TableEntry(dimstyle=(name, overrides))


@tagged_union(frozen=True)
class HatchFill:  # solid color, ISO 128-50 pattern, or two-color gradient
    tag: Literal["solid", "pattern", "gradient"] = tag()
    solid: int = case()  # ACI fill color -> set_solid_fill
    pattern: tuple[str, tuple[PatternLine, ...], int, float, float] = (
        case()
    )  # (name, definition, color, scale, angle); () definition = ezdxf built-in name
    gradient: tuple[tuple[int, int, int], tuple[int, int, int], float] = case()  # (rgb1, rgb2, rotation) -> set_gradient

    @staticmethod
    def Solid(color: int = 7) -> "HatchFill":
        return HatchFill(solid=color)

    @staticmethod
    def Pattern(name: str, definition: tuple[PatternLine, ...] = (), color: int = 7, scale: float = 1.0, angle: float = 0.0) -> "HatchFill":
        return HatchFill(pattern=(name, definition, color, scale, angle))

    @staticmethod
    def Gradient(rgb1: tuple[int, int, int], rgb2: tuple[int, int, int] = (255, 255, 255), rotation: float = 0.0) -> "HatchFill":
        return HatchFill(gradient=(rgb1, rgb2, rotation))


@tagged_union(frozen=True)
class DxfEntity:
    tag: Literal[
        "line",
        "arc",
        "circle",
        "ellipse",
        "spline",
        "lwpolyline",
        "hatch",
        "text",
        "mtext",
        "leader",
        "multileader",
        "dimension",
        "blockref",
        "point",
        "mesh",
    ] = tag()
    line: tuple[Point3, Point3, DxfAttribs] = case()  # (start, end, attribs)
    arc: tuple[Point3, float, float, float, DxfAttribs] = case()  # (center, radius, start_angle, end_angle, attribs)
    circle: tuple[Point3, float, DxfAttribs] = case()  # (center, radius, attribs)
    ellipse: tuple[Point3, Point3, float, float, float, DxfAttribs] = case()  # (center, major_axis, ratio, start, end, attribs)
    spline: tuple[tuple[Point3, ...], int, DxfAttribs] = case()  # (fit_points, degree, attribs)
    lwpolyline: tuple[tuple[tuple[float, ...], ...], bool, DxfAttribs] = case()  # (xyseb rows, close, attribs)
    hatch: tuple[tuple[tuple[Point3, ...], ...], HatchFill, DxfAttribs] = case()  # (boundary loops, fill sub-axis, attribs)
    text: tuple[str, Point3, float, DxfAttribs] = case()  # (text, insert, height, attribs)
    mtext: tuple[str, Point3, float, DxfAttribs] = case()  # (text, insert, char_height, attribs)
    leader: tuple[tuple[Point3, ...], DxfAttribs] = case()  # (vertices, attribs) — the legacy single-line leader
    multileader: tuple[MLeaderKind, str, str, tuple[LeaderLine, ...], Point3, DxfAttribs] = (
        case()
    )  # (kind, content, style, leader lines, insert, attribs) — modern leader-with-content
    dimension: tuple[DimKind, tuple[Point3, ...], str, DxfAttribs] = case()  # (kind, defpoints, dimstyle, attribs)
    blockref: tuple[str, Point3, float, float, DxfAttribs] = case()  # (block name, insert, scale, rotation, attribs)
    point: tuple[Point3, DxfAttribs] = case()  # (location, attribs)
    mesh: tuple[tuple[Point3, ...], tuple[tuple[int, ...], ...], DxfAttribs] = case()  # (vertices, faces, attribs)

    @staticmethod
    def Line(start: Point3, end: Point3, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(line=(start, end, attribs))

    @staticmethod
    def Arc(center: Point3, radius: float, start_angle: float, end_angle: float, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(arc=(center, radius, start_angle, end_angle, attribs))

    @staticmethod
    def Circle(center: Point3, radius: float, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(circle=(center, radius, attribs))

    @staticmethod
    def Ellipse(
        center: Point3, major_axis: Point3, ratio: float, start: float = 0.0, end: float = tau, attribs: DxfAttribs = DxfAttribs()
    ) -> "DxfEntity":
        return DxfEntity(ellipse=(center, major_axis, ratio, start, end, attribs))

    @staticmethod
    def Spline(fit_points: tuple[Point3, ...], degree: int = 3, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(spline=(fit_points, degree, attribs))

    @staticmethod
    def LwPolyline(points: tuple[tuple[float, ...], ...], close: bool = False, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(lwpolyline=(points, close, attribs))

    @staticmethod
    def Hatch(loops: tuple[tuple[Point3, ...], ...], fill: HatchFill = HatchFill.Solid(), attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(hatch=(loops, fill, attribs))

    @staticmethod
    def Text(text: str, insert: Point3, height: float = 2.5, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(text=(text, insert, height, attribs))

    @staticmethod
    def MText(text: str, insert: Point3, char_height: float = 2.5, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(mtext=(text, insert, char_height, attribs))

    @staticmethod
    def Leader(vertices: tuple[Point3, ...], attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(leader=(vertices, attribs))

    @staticmethod
    def MultiLeader(
        content: str,
        lines: tuple[LeaderLine, ...],
        kind: MLeaderKind = MLeaderKind.MTEXT,
        style: str = "Standard",
        insert: Point3 = (0.0, 0.0, 0.0),
        attribs: DxfAttribs = DxfAttribs(),
    ) -> "DxfEntity":
        return DxfEntity(multileader=(kind, content, style, lines, insert, attribs))

    @staticmethod
    def Dimension(kind: DimKind, defpoints: tuple[Point3, ...], dimstyle: str = "Standard", attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(dimension=(kind, defpoints, dimstyle, attribs))

    @staticmethod
    def BlockRef(name: str, insert: Point3, scale: float = 1.0, rotation: float = 0.0, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(blockref=(name, insert, scale, rotation, attribs))

    @staticmethod
    def Point(location: Point3, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(point=(location, attribs))

    @staticmethod
    def Mesh(vertices: tuple[Point3, ...], faces: tuple[tuple[int, ...], ...], attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(mesh=(vertices, faces, attribs))


class BlockDef(Struct, frozen=True):  # a reusable block definition `add_blockref` places n times
    name: str
    entities: tuple[DxfEntity, ...] = ()


class DxfDocument(Struct, frozen=True):  # the New-arm authoring spec — the whole document graph
    version: DxfVersion = DxfVersion.R2018
    units: DxfUnits = DxfUnits.MILLIMETER
    setup: bool = True  # load standard linetypes/styles/dimstyles
    tables: tuple[TableEntry, ...] = ()
    xrefs: tuple[Xref, ...] = ()
    blocks: tuple[BlockDef, ...] = ()
    entities: tuple[DxfEntity, ...] = ()
    fmt: DxfFormat = DxfFormat.ASC


@tagged_union(frozen=True)
class DxfSource:
    tag: Literal["blob", "file", "zip", "base64"] = tag()
    blob: bytes = case()  # a DXF byte stream -> `ezdxf.read` / `recover.read`
    file: str = case()  # a filesystem path -> `ezdxf.readfile` / `recover.readfile`
    zip: tuple[str, str | None] = case()  # (zip path, member) -> `ezdxf.readzip`
    base64: str = case()  # a base64 DXF blob -> `ezdxf.decode_base64`

    @staticmethod
    def Blob(data: bytes) -> "DxfSource":
        return DxfSource(blob=data)

    @staticmethod
    def File(path: str) -> "DxfSource":
        return DxfSource(file=path)

    @staticmethod
    def Zip(path: str, member: str | None = None) -> "DxfSource":
        return DxfSource(zip=(path, member))

    @staticmethod
    def Base64(text: str) -> "DxfSource":
        return DxfSource(base64=text)


@tagged_union(frozen=True)
class Spatial:
    tag: Literal["window", "circle", "polygon", "fence", "point"] = tag()
    window: tuple[Point2, Point2, SpatialTest] = case()  # (corner, corner, area-test) -> select.Window
    circle: tuple[Point2, float, SpatialTest] = case()  # (center, radius, area-test) -> select.Circle
    polygon: tuple[tuple[Point2, ...], SpatialTest] = case()  # (vertices, area-test) -> select.Polygon
    fence: tuple[Point2, ...] = case()  # an open polyline -> select.bbox_crosses_fence
    point: Point2 = case()  # a hit-test point -> select.point_in_bbox

    @staticmethod
    def Window(low: Point2, high: Point2, test: SpatialTest = SpatialTest.INSIDE) -> "Spatial":
        return Spatial(window=(low, high, test))

    @staticmethod
    def Circle(center: Point2, radius: float, test: SpatialTest = SpatialTest.INSIDE) -> "Spatial":
        return Spatial(circle=(center, radius, test))

    @staticmethod
    def Polygon(vertices: tuple[Point2, ...], test: SpatialTest = SpatialTest.OVERLAP) -> "Spatial":
        return Spatial(polygon=(vertices, test))

    @staticmethod
    def Fence(vertices: tuple[Point2, ...]) -> "Spatial":
        return Spatial(fence=vertices)

    @staticmethod
    def Point(location: Point2) -> "Spatial":
        return Spatial(point=location)


class Selection(Struct, frozen=True):  # the `doc.query` EQL + optional `Spatial` read spec
    eql: str = "*"  # the DXF entity-query-language string, `'LINE CIRCLE[layer=="WALLS"]'`
    spatial: Spatial | None = None  # the optional rtree spatial refinement


class TransformSpec(Struct, frozen=True):  # an affine (translate∘scale∘z-rotate) over an ingested doc or a queried sub-selection
    translate: Point3 = (0.0, 0.0, 0.0)
    scale: Point3 = (1.0, 1.0, 1.0)
    rotate: float = 0.0  # z-axis rotation (radians)
    eql: str = "*"  # the `doc.query` selection the affine applies to (`*` = whole modelspace)
    mode: TransformMode = TransformMode.INPLACE


@tagged_union(frozen=True)
class BridgeSpec:  # the DXF<->SVG<->GeoJSON<->glyph geometry wire
    tag: Literal["to_svg", "from_svg", "to_geojson", "from_geojson", "text_paths"] = tag()
    to_svg: tuple[DxfSource, float, BridgeSample] = case()  # (source, flatten distance, sample mode) -> framed SVG
    from_svg: tuple[tuple[tuple[Point2, ...], ...], DxfVersion, DxfAttribs] = case()  # (vertex rings, version, attribs) -> DXF
    to_geojson: DxfSource = case()  # entities -> GeoProxy.__geo_interface__ mapping bytes
    from_geojson: tuple[bytes, DxfVersion, DxfAttribs] = case()  # (GeoJSON bytes, version, attribs) -> DXF
    text_paths: tuple[str, str, float, Point3, DxfVersion, DxfAttribs] = case()  # (text, font family, size, insert, version, attribs) -> DXF outline

    @staticmethod
    def ToSvg(source: DxfSource, distance: float = 0.1, sample: BridgeSample = BridgeSample.FLATTEN) -> "BridgeSpec":
        return BridgeSpec(to_svg=(source, distance, sample))

    @staticmethod
    def FromSvg(rings: tuple[tuple[Point2, ...], ...], version: DxfVersion = DxfVersion.R2018, attribs: DxfAttribs = DxfAttribs()) -> "BridgeSpec":
        return BridgeSpec(from_svg=(rings, version, attribs))

    @staticmethod
    def ToGeoJson(source: DxfSource) -> "BridgeSpec":
        return BridgeSpec(to_geojson=source)

    @staticmethod
    def FromGeoJson(mapping: bytes, version: DxfVersion = DxfVersion.R2018, attribs: DxfAttribs = DxfAttribs()) -> "BridgeSpec":
        return BridgeSpec(from_geojson=(mapping, version, attribs))

    @staticmethod
    def TextPaths(
        text: str,
        font: str = "sans-serif",
        size: float = 10.0,
        insert: Point3 = (0.0, 0.0, 0.0),
        version: DxfVersion = DxfVersion.R2018,
        attribs: DxfAttribs = DxfAttribs(),
    ) -> "BridgeSpec":
        return BridgeSpec(text_paths=(text, font, size, insert, version, attribs))


@tagged_union(frozen=True)
class DxfOp:  # the closed request vocabulary lowered once into DxfComposed
    tag: Literal["new", "read", "recover", "render", "query", "transform", "bridge"] = tag()
    new: DxfDocument = case()
    read: DxfSource = case()
    recover: DxfSource = case()
    render: tuple[DxfSource, DxfBackend, PageSpec] = case()
    query: tuple[DxfSource, Selection] = case()
    transform: tuple[DxfSource, TransformSpec] = case()
    bridge: BridgeSpec = case()

    @staticmethod
    def New(document: DxfDocument) -> "DxfOp":
        return DxfOp(new=document)

    @staticmethod
    def Read(source: DxfSource) -> "DxfOp":
        return DxfOp(read=source)

    @staticmethod
    def Recover(source: DxfSource) -> "DxfOp":
        return DxfOp(recover=source)

    @staticmethod
    def Render(source: DxfSource, backend: DxfBackend = DxfBackend.SVG, page: PageSpec = PageSpec()) -> "DxfOp":
        return DxfOp(render=(source, backend, page))

    @staticmethod
    def Query(source: DxfSource, selection: Selection = Selection()) -> "DxfOp":
        return DxfOp(query=(source, selection))

    @staticmethod
    def Transform(source: DxfSource, spec: TransformSpec = TransformSpec()) -> "DxfOp":
        return DxfOp(transform=(source, spec))

    @staticmethod
    def Bridge(spec: BridgeSpec) -> "DxfOp":
        return DxfOp(bridge=spec)


# --- [OPERATIONS] -----------------------------------------------------------------------
_GUARD = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))


@_GUARD
def _admit(dpi: PositiveInt, scale: Positive, margin: NonNegative, distance: Positive, /) -> None:
    # the scalar admission seam beartype deep-checks: a non-positive `dpi`/`scale`/`distance` rails before the
    # render/bridge divide; the guard sits here because beartype never recurses into a `_composed(op)` case-tuple payload.
    return None


def _ingest(source: DxfSource, /) -> "Drawing":  # the conforming polymorphic ingestion family
    match source:
        case DxfSource(tag="blob", blob=data):
            return ezdxf.read(io.StringIO(data.decode(errors="surrogateescape")))
        case DxfSource(tag="file", file=path):
            return ezdxf.readfile(path)
        case DxfSource(tag="zip", zip=(path, member)):
            return ezdxf.readzip(path, member)
        case DxfSource(tag="base64", base64=text):
            return ezdxf.decode_base64(text.encode())
        case _ as unreachable:
            assert_never(unreachable)


def _binary(source: DxfSource, /) -> bytes:  # normalize any non-file source to a binary stream for salvage
    match source:
        case DxfSource(tag="blob", blob=data):
            return data
        case DxfSource(tag="base64", base64=text):
            return base64.b64decode(text)
        case DxfSource(tag="zip", zip=(path, member)):
            with zipfile.ZipFile(path) as archive:
                return archive.read(member or archive.namelist()[0])
        case DxfSource(tag="file", file=path):
            return Path(path).read_bytes()
        case _ as unreachable:
            assert_never(unreachable)


def _recovered(source: DxfSource, /) -> tuple["Drawing", object]:  # `recover.readfile`/`read` -> (doc, auditor)
    match source:
        case DxfSource(tag="file", file=path):
            return recover.readfile(path)
        case _:
            return recover.read(io.BytesIO(_binary(source)))


def _build_entity(layout: "Modelspace | BlockLayout", entity: DxfEntity, /) -> None:
    # Exemption: the ezdxf `Drawing` is a mutable builder — each `add_*` is platform-forced construction, the domain shape the closed match.
    match entity:
        case DxfEntity(tag="line", line=(start, end, at)):
            layout.add_line(start, end, dxfattribs=at.gfx())
        case DxfEntity(tag="arc", arc=(center, radius, lo, hi, at)):
            layout.add_arc(center, radius, lo, hi, dxfattribs=at.gfx())
        case DxfEntity(tag="circle", circle=(center, radius, at)):
            layout.add_circle(center, radius, dxfattribs=at.gfx())
        case DxfEntity(tag="ellipse", ellipse=(center, major, ratio, lo, hi, at)):
            layout.add_ellipse(center, major, ratio, lo, hi, dxfattribs=at.gfx())
        case DxfEntity(tag="spline", spline=(fit, degree, at)):
            layout.add_spline(fit_points=fit, degree=degree, dxfattribs=at.gfx())
        case DxfEntity(tag="lwpolyline", lwpolyline=(points, close, at)):
            layout.add_lwpolyline(points, format="xyseb", close=close, dxfattribs=at.gfx())
        case DxfEntity(tag="hatch", hatch=(loops, fill, at)):
            hatched = layout.add_hatch(dxfattribs=at.gfx())
            for loop in loops:
                hatched.paths.add_polyline_path(loop, is_closed=True)
            match fill:  # SOLID/PATTERN/GRADIENT fill sub-axis — total over the closed `HatchFill` family
                case HatchFill(tag="solid", solid=color):
                    hatched.set_solid_fill(color=color)
                case HatchFill(tag="pattern", pattern=(name, definition, color, scale, angle)):
                    hatched.set_pattern_fill(
                        name, color=color, scale=scale, angle=angle, definition=list(definition) or None
                    )  # ISO 128-50 pattern; () def = built-in
                case HatchFill(tag="gradient", gradient=(rgb1, rgb2, rotation)):
                    hatched.set_gradient(color1=colors.RGB(*rgb1), color2=colors.RGB(*rgb2), rotation=rotation)
                case _ as unreachable:
                    assert_never(unreachable)
        case DxfEntity(tag="text", text=(body, insert, height, at)):
            layout.add_text(body, height=height, dxfattribs=at.gfx()).set_placement(insert)
        case DxfEntity(tag="mtext", mtext=(body, insert, height, at)):
            layout.add_mtext(body, dxfattribs={**at.gfx(), "char_height": height, "insert": insert})
        case DxfEntity(tag="leader", leader=(vertices, at)):
            layout.add_leader(vertices, dxfattribs=at.gfx())
        case DxfEntity(tag="multileader", multileader=(kind, content, style, lines, insert, at)):
            builder = (
                layout.add_multileader_block(style, dxfattribs=at.gfx())
                if kind is MLeaderKind.BLOCK
                else layout.add_multileader_mtext(style, dxfattribs=at.gfx())
            )
            builder.set_content(content)  # mtext string or block name; `MLeaderKind` selects the builder
            for side, vertices in lines:
                builder.add_leader_line(
                    getattr(mleader.ConnectionSide, side.value), list(vertices)
                )  # derive ConnectionSide by lowercase member value
            builder.build(insert=insert)
        case DxfEntity(tag="dimension", dimension=(kind, defpoints, dimstyle, at)):
            _DIM[kind](layout, defpoints, dimstyle, at.gfx()).render()
        case DxfEntity(tag="blockref", blockref=(name, insert, scale, rotation, at)):
            layout.add_blockref(name, insert, dxfattribs={**at.gfx(), "xscale": scale, "yscale": scale, "rotation": rotation})
        case DxfEntity(tag="point", point=(location, at)):
            layout.add_point(location, dxfattribs=at.gfx())
        case DxfEntity(tag="mesh", mesh=(vertices, faces, at)):
            body = layout.add_mesh(dxfattribs=at.gfx())
            with body.edit_data() as data:
                data.vertices, data.faces = list(vertices), list(faces)
        case _ as unreachable:
            assert_never(unreachable)


def _table_entry(doc: "Drawing", entry: TableEntry, /) -> None:
    match entry:
        case TableEntry(tag="layer", layer=(name, color, linetype, lineweight)):
            doc.layers.add(name, color=color, linetype=linetype, lineweight=lineweight)
        case TableEntry(tag="linetype", linetype=(name, pattern, description)):
            doc.linetypes.add(name, pattern=list(pattern), description=description)
        case TableEntry(tag="textstyle", textstyle=(name, font, height, width)):
            doc.styles.add(name, font=font, dxfattribs={"height": height, "width": width})
        case TableEntry(tag="dimstyle", dimstyle=(name, overrides)):
            doc.dimstyles.add(name, dxfattribs=dict(overrides))
        case _ as unreachable:
            assert_never(unreachable)


def _attach_xref(doc: "Drawing", ref: Xref, /) -> None:  # bind one external DXF reference by name
    xref.attach(doc, block_name=ref.block_name, filename=ref.filename, insert=ref.insert, scale=ref.scale, rotation=ref.rotation)


def _authored(spec: DxfDocument, /) -> tuple["Drawing", object]:
    # Exemption: the `Drawing` is a mutable builder — the construction fold is ezdxf's platform seam.
    doc = ezdxf.new(spec.version.value, setup=spec.setup, units=int(spec.units))
    for entry in spec.tables:
        _table_entry(doc, entry)
    for ref in spec.xrefs:
        _attach_xref(doc, ref)
    for block in spec.blocks:
        layout = doc.blocks.new(block.name)
        for entity in block.entities:
            _build_entity(layout, entity)
    msp = doc.modelspace()
    for entity in spec.entities:
        _build_entity(msp, entity)
    return doc, doc.audit()


def _counts(doc: "Drawing", /) -> frozendict[str, int]:
    return frozendict(Counter(entity.dxftype() for entity in doc.modelspace()))


def _extent(doc: "Drawing", /) -> Extent:
    box = bbox.extents(doc.modelspace())
    return (*box.extmin.xyz, *box.extmax.xyz) if box.has_data else _ZERO_EXTENT


def _serialize(doc: "Drawing", fmt: DxfFormat, /) -> bytes:
    match fmt:
        case DxfFormat.ASC:
            sink = io.StringIO()
            doc.write(sink)
            return sink.getvalue().encode()
        case DxfFormat.BIN:
            binary = io.BytesIO()
            doc.write(binary, fmt="bin")
            return binary.getvalue()
        case DxfFormat.BASE64:
            return doc.encode_base64()
        case _ as unreachable:
            assert_never(unreachable)


def _dxf_composed(doc: "Drawing", auditor: object, fmt: DxfFormat, /) -> DxfComposed:
    data = _serialize(doc, fmt)
    return DxfComposed(
        data=data,
        kind=DxfArtifact.DXF,
        dxfversion=doc.dxfversion,
        units=_UNITS[DxfUnits(doc.units)],
        counts=_counts(doc),
        layers=len(doc.layers),
        blocks=len(doc.blocks),
        errors=len(getattr(auditor, "errors", ())),
        fixes=len(getattr(auditor, "fixes", ())),
        extent=_extent(doc),
    )


def _flattened(doc: "Drawing", distance: float, sample: BridgeSample, /) -> Iterator[np.ndarray]:
    # only `_PATH_TYPES` drawables cross `make_path` (a TOTAL predicate, so no per-element `TypeError` from a
    # `Text`/`Insert`); `flattening` adaptively samples a curve, `control_vertices` reads the exact NURBS frame.
    for entity in doc.modelspace():
        if entity.dxftype() in _PATH_TYPES:
            outline = dxfpath.make_path(entity)
            verts = outline.control_vertices() if sample is BridgeSample.CONTROL else outline.flattening(distance)
            array = np.asarray(Vec3.list(verts))
            if array.size:
                yield array


def _polyline(vertices: np.ndarray, /) -> str:
    # one drawable's flattened polyline as an SVG `<path>` `d`.
    body = "M" + " L".join(f"{x:g},{y:g}" for x, y, _ in vertices)
    return f'<path fill="none" stroke="black" d="{body}"/>'


def _spatial(entities: "EntityQuery", spec: Spatial, /) -> "list[DXFGraphic]":
    match spec:
        case Spatial(tag="window", window=(low, high, test)):
            return list(_SPATIAL_TEST[test](select.Window(low, high), entities))
        case Spatial(tag="circle", circle=(center, radius, test)):
            return list(_SPATIAL_TEST[test](select.Circle(center, radius), entities))
        case Spatial(tag="polygon", polygon=(vertices, test)):
            return list(_SPATIAL_TEST[test](select.Polygon(vertices), entities))
        case Spatial(tag="fence", fence=vertices):
            return list(select.bbox_crosses_fence(vertices, entities))
        case Spatial(tag="point", point=location):
            return list(select.point_in_bbox(location, entities))
        case _ as unreachable:
            assert_never(unreachable)


def _rendered(source: DxfSource, backend: DxfBackend, page: PageSpec, /) -> DxfComposed:
    _admit(page.dpi, page.scale, page.margin, 1.0)
    doc = _ingest(source)
    pol = page.policy
    context = RenderContext(doc, ctb=pol.ctb, export_mode=pol.export_mode)
    config = dwgconfig.Configuration(
        background_policy=getattr(dwgconfig.BackgroundPolicy, pol.background.name),
        lineweight_policy=getattr(dwgconfig.LineweightPolicy, pol.lineweight.name),
        hatch_policy=getattr(dwgconfig.HatchPolicy, pol.hatch.name),
        text_policy=getattr(dwgconfig.TextPolicy, pol.text.name),
        color_policy=getattr(dwgconfig.ColorPolicy, pol.color.name),
        proxy_graphic_policy=getattr(dwgconfig.ProxyGraphicPolicy, pol.proxy.name),
        line_policy=getattr(dwgconfig.LinePolicy, pol.line.name),
    )
    layout = dwglayout.Page(page.width, page.height, dwglayout.Units.mm, dwglayout.Margins.all(page.margin))
    settings = dwglayout.Settings(fit_page=page.fit_page, scale=page.scale)
    shared = {
        "dxfversion": doc.dxfversion,
        "units": _UNITS[DxfUnits(doc.units)],
        "counts": _counts(doc),
        "layers": len(doc.layers),
        "blocks": len(doc.blocks),
        "extent": _extent(doc),
    }
    match backend:
        case DxfBackend.SVG:
            sink = SVGBackend()
            Frontend(context, sink, config).draw_layout(doc.modelspace(), finalize=True)
            return DxfComposed(data=sink.get_string(layout, settings=settings).encode(), kind=DxfArtifact.SVG, **shared)
        case DxfBackend.PDF:
            sink = PyMuPdfBackend()
            Frontend(context, sink, config).draw_layout(doc.modelspace(), finalize=True)
            return DxfComposed(data=sink.get_pdf_bytes(layout, settings=settings), kind=DxfArtifact.PDF, **shared)
        case DxfBackend.PNG:
            sink = PyMuPdfBackend()
            Frontend(context, sink, config).draw_layout(doc.modelspace(), finalize=True)
            return DxfComposed(data=sink.get_pixmap_bytes(layout, settings=settings, dpi=page.dpi), kind=DxfArtifact.PNG, **shared)
        case DxfBackend.EPS | DxfBackend.PS:
            figure = Figure()  # GC-safe (no `pyplot` global registry), so no bracket — the render sink savefig writes
            Frontend(context, MatplotlibBackend(figure.add_axes((0.0, 0.0, 1.0, 1.0))), config).draw_layout(doc.modelspace(), finalize=True)
            vector = io.BytesIO()
            figure.savefig(vector, format=backend.value, dpi=page.dpi)
            return DxfComposed(data=vector.getvalue(), kind=DxfArtifact(backend.value), **shared)
        case DxfBackend.JSON | DxfBackend.GEOJSON:
            sink = CustomJSONBackend() if backend is DxfBackend.JSON else GeoJSONBackend()
            Frontend(context, sink, config).draw_layout(doc.modelspace(), finalize=True)
            return DxfComposed(data=sink.get_string().encode(), kind=DxfArtifact(backend.value), **shared)
        case _ as unreachable:
            assert_never(unreachable)


def _queried(source: DxfSource, selection: Selection, /) -> DxfComposed:
    doc = _ingest(source)
    matched = doc.query(selection.eql)
    entities = _spatial(matched, selection.spatial) if selection.spatial is not None else list(matched)
    extract = ezdxf.new(doc.dxfversion, setup=True)
    importer = Importer(doc, extract)
    importer.import_entities(entities)
    importer.finalize()
    box = bbox.extents(entities)
    return DxfComposed(
        data=_serialize(extract, DxfFormat.ASC),
        kind=DxfArtifact.DXF,
        dxfversion=doc.dxfversion,
        units=_UNITS[DxfUnits(doc.units)],
        counts=frozendict(Counter(entity.dxftype() for entity in entities)),
        layers=len(extract.layers),
        blocks=len(extract.blocks),
        extent=(*box.extmin.xyz, *box.extmax.xyz) if box.has_data else _ZERO_EXTENT,
    )


def _transformed(source: DxfSource, spec: TransformSpec, /) -> DxfComposed:
    # Exemption: `transform.inplace` mutates the ezdxf entities in place — the affine application is ezdxf's platform seam.
    doc = _ingest(source)
    matrix = Matrix44.chain(Matrix44.scale(*spec.scale), Matrix44.z_rotate(spec.rotate), Matrix44.translate(*spec.translate))
    entities = doc.query(spec.eql)  # `*` = whole modelspace; an EQL string scopes the affine to a sub-selection
    match spec.mode:
        case TransformMode.INPLACE:
            transform.inplace(entities, matrix)
        case TransformMode.COPIES:
            _, copied = transform.copies(entities, matrix)  # (logger, new entities) — add the duplicated set to the modelspace
            msp = doc.modelspace()
            for entity in copied:
                msp.add_entity(entity)
        case _ as unreachable:
            assert_never(unreachable)
    return _dxf_composed(doc, doc.audit(), DxfFormat.ASC)


def _bridged(spec: BridgeSpec, /) -> DxfComposed:
    match spec:
        case BridgeSpec(tag="to_svg", to_svg=(source, distance, sample)):
            _admit(1, 1.0, 0.0, distance)
            doc = _ingest(source)
            extent = _extent(doc)
            fragments = tuple(_polyline(verts) for verts in _flattened(doc, distance, sample))
            data = svg_frame(fragments, (extent[0], extent[1], extent[3], extent[4]))
            return DxfComposed(
                data=data,
                kind=DxfArtifact.SVG,
                dxfversion=doc.dxfversion,
                units=_UNITS[DxfUnits(doc.units)],
                counts=_counts(doc),
                layers=len(doc.layers),
                extent=extent,
            )
        case BridgeSpec(tag="from_svg", from_svg=(rings, version, attribs)):
            doc = ezdxf.new(version.value, setup=True)
            paths = [dxfpath.from_vertices(ring, close=False) for ring in rings if ring]
            dxfpath.render_lines(doc.modelspace(), paths, dxfattribs=attribs.gfx())
            return _dxf_composed(doc, doc.audit(), DxfFormat.ASC)
        case BridgeSpec(tag="to_geojson", to_geojson=source):
            doc = _ingest(source)
            proxy = dxfgeo.proxy(entity for entity in doc.modelspace() if entity.dxftype() in _GEO_TYPES)
            return DxfComposed(
                data=msgspec.json.encode(proxy.__geo_interface__),
                kind=DxfArtifact.GEOJSON,
                dxfversion=doc.dxfversion,
                units=_UNITS[DxfUnits(doc.units)],
                counts=_counts(doc),
                layers=len(doc.layers),
                extent=_extent(doc),
            )
        case BridgeSpec(tag="from_geojson", from_geojson=(mapping, version, attribs)):
            # Exemption: the `Drawing` mutable builder — the GeoProxy entity fold is ezdxf's construction seam.
            doc = ezdxf.new(version.value, setup=True)
            msp = doc.modelspace()
            for entity in dxfgeo.GeoProxy.parse(msgspec.json.decode(mapping)).to_dxf_entities(dxfattribs=attribs.gfx()):
                msp.add_entity(entity)
            return _dxf_composed(doc, doc.audit(), DxfFormat.ASC)
        case BridgeSpec(tag="text_paths", text_paths=(text, font, size, insert, version, attribs)):
            doc = ezdxf.new(version.value, setup=True)
            paths = text2path.make_paths_from_str(text, FontFace(family=font), size=size, m=Matrix44.translate(*insert))
            dxfpath.render_lines(doc.modelspace(), paths, dxfattribs=attribs.gfx())
            return _dxf_composed(doc, doc.audit(), DxfFormat.ASC)
        case _ as unreachable:
            assert_never(unreachable)


def _dxf_raise(fault: object) -> "Composed":
    raise ValueError(str(fault))


def _composed(op: DxfOp) -> DxfComposed:  # the one pure fold both `of` and `contribute` read
    match op:
        case DxfOp(tag="new", new=document):
            doc, auditor = _authored(document)
            return _dxf_composed(doc, auditor, document.fmt)
        case DxfOp(tag="read", read=source):
            doc = _ingest(source)
            return _dxf_composed(doc, doc.audit(), DxfFormat.ASC)
        case DxfOp(tag="recover", recover=source):
            doc, auditor = _recovered(source)
            return _dxf_composed(doc, auditor, DxfFormat.ASC)
        case DxfOp(tag="render", render=(source, backend, page)):
            return _rendered(source, backend, page)
        case DxfOp(tag="query", query=(source, selection)):
            return _queried(source, selection)
        case DxfOp(tag="transform", transform=(source, spec)):
            return _transformed(source, spec)
        case DxfOp(tag="bridge", bridge=spec):
            return _bridged(spec)
        case _ as unreachable:
            assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------
async def _offloaded(op: DxfOp, /) -> DxfComposed:
    return (await LanePolicy.offload(_composed, op, modality=Modality.THREAD, retry=RetryClass.OCCT)).default_with(_dxf_raise)


class Dxf(Struct, frozen=True):
    op: DxfOp

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical op payload minted PRE-RUN — never a key over the composed DXF bytes.
        return ContentIdentity.of(f"dxf-{self.op.tag}", self.op, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        return (await async_boundary(f"dxf.{self.op.tag}", lambda: _offloaded(self.op), catch=_FAULTS)).map(
            lambda composed: self._receipt(self._key, composed)
        )

    def _receipt(self, key: ContentKey, composed: "Composed", /) -> ArtifactReceipt:
        return ArtifactReceipt.Cad(
            key,
            composed.dxfversion,
            composed.units,
            composed.kind.value,
            len(composed.data),
            composed.layers,
            composed.blocks,
            composed.errors,
            composed.fixes,
            composed.counts,
        )

    def contribute(self) -> "Iterable[Receipt]":
        yield from self._receipt(self._key, _composed(self.op)).contribute()


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "BackgroundPolicy",
    "BlockDef",
    "BridgeSample",
    "BridgeSpec",
    "ColorPolicy",
    "DimKind",
    "Dxf",
    "DxfArtifact",
    "DxfAttribs",
    "DxfBackend",
    "DxfDocument",
    "DxfEntity",
    "DxfFormat",
    "DxfOp",
    "DxfSource",
    "DxfUnits",
    "DxfVersion",
    "HatchFill",
    "HatchPolicy",
    "LeaderSide",
    "LinePolicy",
    "LineweightPolicy",
    "MLeaderKind",
    "PageSpec",
    "ProxyPolicy",
    "RenderPolicy",
    "Selection",
    "Spatial",
    "SpatialTest",
    "TableEntry",
    "TextPolicy",
    "TransformMode",
    "TransformSpec",
    "Xref",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
