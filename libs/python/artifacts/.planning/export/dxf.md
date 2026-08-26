# [PY_ARTIFACTS_DXF]

`Dxf` owns the CAD-exchange editable hand-off — ONE owner over the closed `DxfOp` `tagged_union` — `New` authors a fresh document from a `DxfDocument` spec, `Read` ingests a conforming DXF, `Recover` salvages a damaged one, `Render` lowers a DXF figure into the composition/graphic plane over the in-process backend family, `Query` extracts an attribute/spatial sub-selection, `Transform` applies an affine, `Diagram` lowers a positioned `DiagramGlyph` sequence into regime-pen-layered native entities, `Bridge` crosses the DXF↔SVG↔GeoJSON↔glyph geometry wire — each arm a typed payload dispatched by one total `match` and folded ONCE into a `DxfComposed` evidence struct. `Dxf.of` is the ONE validated ingress, discriminating on input shape — a bare `DxfDocument` becomes `New`, a `DxfSource` becomes `Read`, a `DiagramLower` becomes `Diagram`, a `BridgeSpec` becomes `Bridge`, a `DxfOp` passes through — and refusing an empty ingestion source, a non-positive render/bridge scalar, degenerate spatial or bridge geometry, and an empty EQL selection into the closed `DxfFault` vocabulary before any worker runs. `ezdxf` (pure-Python, no interpreter gate) is the sole owner of the DXF R12→R2018 read/write/recover/render surface, so this owner re-implements no tag grammar, OCS/WCS transform, B-spline evaluator, or entity-to-SVG conversion.

## [01]-[INDEX]

- [02]-[DXF]: the CAD-exchange owner over the closed `DxfOp` `tagged_union` (`New`/`Read`/`Recover`/`Render`/`Query`/`Transform`/`Diagram`/`Bridge`) admitted at `Dxf.of` and folded once into `DxfComposed`, thread-offloaded and result-typed, `ezdxf` the sole read/write/recover/render surface.

## [02]-[DXF]

- Owner: `Dxf` holds `op: DxfOp` and `lane: LanePolicy`; every generated case constructor enters `Dxf.of`, and every arm folds once into `DxfComposed`. `DxfDocument` admits version, units, setup, optional `Standard` seeding, `TableEntry` rows, `Xref` references, `BlockDef` blocks, modelspace `DxfEntity` drawables, and `DxfFormat` egress. `DxfEntity` spans analytic and rational curves, lightweight and heavy polylines, polymesh/polyface/mesh topology, hatch and `MPOLYGON` fills, text and attributes, leaders/dimensions/tolerances, references, construction geometry, primitives, and SAT/SAB-backed ACIS bodies/solids/surfaces. Every case carries one `DxfAttribs`; `.gfx()` projects the uniform `GfxAttribs(...).asdict()` payload. `DxfFill` routes solid/pattern/gradient subcases. `MLeaderKind` selects the multileader builder, and `DimKind` keys `_DIM`. `TableEntry` owns layer, linetype, text, dimension, and multileader style rows. `DxfSource` separates archive-default and named-member admission as `zip` and `zip_member`. `Spatial` routes window/circle/polygon/fence/point cases through `_SPATIAL_TEST` and `select.*`. `DiagramLower` admits the positioned `DiagramGlyph` sequence with a total `GlyphStyle.layer`-to-`LayerName` binding, so every mark lands ByLayer on a `Standard.seed`-styled regime-pen layer.
- Cases: `New` validates `DxfDocument.issue`, authors through `ezdxf.new`, composes optional `Standard.seed`, folds builders, and audits before egress. `Read` ingests conforming ASCII or binary DXF. `Recover` salvages damaged input and preserves auditor error/fix counts. `Render` selects SVG, PDF, PNG, EPS, PS, JSON, or GeoJSON backends. `Query` composes EQL and optional spatial selection through `Importer`. `Transform` applies `Matrix44` through `transform.inplace` or `transform.copies`. `Diagram` lowers each glyph to `DxfEntity` cases — silhouette rings and bulge arcs as lwpolylines, edges as width-carrying polylines or fit-point splines, each terminator ONE shared cap-block INSERT rotated onto its segment, areas as hatched snappable boundaries, fragments through the svgelements flatten — every label editable placement-aligned source text on the drawio-arm precedent, a dashed glyph riding its minted linetype row, mathematical typesetting staying the draw plane's `Formula` boundary. `Bridge` crosses SVG through `apply_region(RegionOp.Serialize(...))`, GeoJSON through `GeoProxy`, and glyph outlines through `text2path`.
- Auto: `_composed(op) -> Result[DxfComposed, RegionFault]` is the total fold the lane runs. `_build_entity` lowers each admitted `DxfEntity`; `_table_entry` lowers each `TableEntry`; `_evidence` reads shared CAD facts once, and each arm derives `DxfComposed` through `msgspec.structs.replace`. `_glyph_entities` folds the seven diagram marks into the same closed drawable vocabulary and `_diagram_document` re-enters `_authored`, so one construction fold serves authoring and diagram lowering with zero new ezdxf surface; the glyph plane is SVG y-down while DXF model space is y-up, so the lowering mirrors y and negates angles at that one boundary. `Drawing` is pure Python and GC-safe. `_serialize` owns text, binary, and base64 egress; render backends own their native handles.
- Output: `DxfComposed` carries serialized `data`, `kind`, `dxfversion`, `units`, `counts`, `layers`, `blocks`, `errors`, `fixes`, and `extent`. Auditor counts become non-zero only for salvaged input. `DxfUnits` mirrors `ezdxf.units.InsertUnits`, so `DxfUnits(doc.units)` is total over conforming foreign documents.
- Packages: `ezdxf` owns read/write/recover/audit, builders, ACIS decode/export, hatch and multileader construction, xrefs, paths, math, rendering, queries, selection, GeoJSON, text paths, and import. `matplotlib.figure.Figure` owns EPS/PS egress. `apply_region(RegionOp.Serialize(...))` frames SVG. `Standard.seed` authors ISO tables. `visualization/diagram/glyphset` supplies the mark vocabulary and its shared lowering derivations (`DiagramGlyph.mark`, `Port.seat`, `AreaMark.centroid`, `ER_CAPS`, `ENTITY_BAND`). `numpy` carries `(N,3)` vertices. `expression` and `msgspec` own unions, results, wires, and evidence derivation. Runtime supplies `identity.ContentIdentity.key`, `lanes.LanePolicy`, `workers.Kernel`/`KernelTrait`, `faults.RuntimeResult`/`BoundaryFault`, and `journal.Journal` the durable writer.
- Growth: a new DXF version is one `DxfVersion` member; a new drawable is one `DxfEntity` case plus one `_build_entity` arm (the `assert_never` tail breaking the fold at type-check) plus its `_ENTITY_FLOOR` row when it postdates R12; a new hatch fill is one `DxfFill` case; a new dimension kind is one `DimKind` member plus one `_DIM` row; a new symbol-table row is one `TableEntry` case plus one `_table_entry` arm; a new ingestion source is one `DxfSource` case; a new render backend or format is one `DxfBackend`+`DxfArtifact` member plus one `_rendered` arm; a new egress encoding (the `r12writer` streaming fast-writer) is one `DxfFormat` member plus one `_serialize` arm; a new spatial refinement is one `Spatial` case; a new render policy is one `DxfRenderPolicy` field; a new affine mode is one `TransformMode` member; a new bridge direction is one `BridgeSpec` case over the existing `ezdxf.addons` surface; a new query refinement (`groupby`, the `EntityQuery` set-algebra) is one `Selection` field; a new diagram mark lowering is one `_glyph_entities` arm plus its `_GLYPH_TEXT` default-size row; a new node silhouette is one `_SILHOUETTE` row the import-time coverage gate proves reachable; a new terminator lowering is one `_cap_block` arm; a new label placement is one `TextAlign` member; a new output scalar is one `Cad` slot; a new admission invariant is one `DxfFault` case plus one `_checked` guard.
- Boundary: `ezdxf` owns tag construction, affine/B-spline/OCS math, ACIS SAT/SAB decoding, rendering, querying, salvage, and DXF↔GeoJSON conversion. `DxfAttribs` replaces per-entity setters; `doc.query`/`select` replace lookup-method families; `recover.readfile` owns damaged input. `Standard.seed` owns ISO symbol-table derivation. `visualization/diagram/layout#LAYOUT` owns diagram coordinates and routes, `visualization/diagram/draw#DRAW` owns the SVG/.drawio egress, and the glyph vocabulary is `visualization/diagram/glyphset#GLYPHSET`'s — the `Diagram` arm owns only the entity lowering and the y-mirror boundary. `graphic/vector/region#REGION` frames SVG, composition owns PDF assembly, typography owns shaping, and the geospatial owner owns CRS semantics.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import base64
import io
import math
import zipfile
from collections import Counter
from collections.abc import Callable, Iterable, Iterator
from enum import IntEnum, StrEnum
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never, cast, get_args

import msgspec
import numpy as np
from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import Metrics
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import TRANSIENT, BoundaryFault, FaultRow, RuntimeResult, rostered

from rasm.artifacts.core.hooks import BYTE_VOLUME, DOMAIN, ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.drawing.regime import LayerName
from rasm.artifacts.drawing.standard import DimStyleFamily, DimVar, Standard
from rasm.artifacts.graphic.vector.path import PathFault, scene
from rasm.artifacts.graphic.vector.region import Fragment, RegionFault, RegionOp, applied as apply_region
from rasm.artifacts.visualization.diagram.glyphset import (
    AreaMark,
    DiagramGlyph,
    EdgeMark,
    EdgeRoute,
    EndCap,
    ENTITY_BAND,
    ER_CAPS,
    GlyphTag,
    MarkerKind,
    MarkerMark,
    NodeMark,
    NodeShape,
    TextAnchor,
)

lazy import ezdxf
lazy from ezdxf import bbox, colors, recover, select, transform, xref
lazy from ezdxf.acis import api as acis
lazy from ezdxf import path as dxfpath
lazy from ezdxf.addons import Importer
lazy from ezdxf.addons import geo as dxfgeo
lazy from ezdxf.addons import text2path
lazy from svgelements import Close, Path as SvgPath
lazy from ezdxf.addons.drawing import Frontend, RenderContext
lazy from ezdxf.addons.drawing import config as dwgconfig
lazy from ezdxf.addons.drawing import layout as dwglayout
lazy from ezdxf.addons.drawing.json import CustomJSONBackend, GeoJSONBackend
lazy from ezdxf.addons.drawing.matplotlib import MatplotlibBackend
lazy from ezdxf.addons.drawing.pymupdf import PyMuPdfBackend
lazy from ezdxf.addons.drawing.svg import SVGBackend
lazy from ezdxf.enums import TextEntityAlignment
lazy from ezdxf.fonts.fonts import FontFace
lazy from ezdxf.gfxattribs import GfxAttribs
lazy from ezdxf.math import Matrix44, Vec3
lazy from ezdxf.render import mleader
lazy from matplotlib.figure import Figure

if TYPE_CHECKING:
    from ezdxf.audit import Auditor
    from ezdxf.document import Drawing
    from ezdxf.entities import DXFGraphic
    from ezdxf.layouts import BlockLayout, Modelspace
    from ezdxf.query import EntityQuery


# --- [TYPES] ----------------------------------------------------------------------------
type Point2 = tuple[float, float]
type Point3 = tuple[float, float, float]
type PolyRow = tuple[float, float, float, float, float]
type HatchRow = tuple[float, float, float]
type Silhouette = Callable[[float, float, float, float], tuple[PolyRow, ...]]
type Extent = tuple[float, float, float, float, float, float]
type Attribs = dict[str, object]
type Builder = Callable[["Modelspace | BlockLayout", tuple[Point3, ...], str, Attribs], object]
type PatternLine = tuple[float, Point2, Point2, tuple[float, ...]]
type LeaderLine = tuple[LeaderSide, tuple[Point3, ...]]
type AcisData = bytes | tuple[str, ...]


class DxfVersion(StrEnum):
    R12 = "AC1009"
    R2000 = "AC1015"
    R2004 = "AC1018"
    R2007 = "AC1021"
    R2010 = "AC1024"
    R2013 = "AC1027"
    R2018 = "AC1032"


class DxfUnits(IntEnum):
    UNITLESS = 0
    INCH = 1
    FOOT = 2
    MILE = 3
    MILLIMETER = 4
    CENTIMETER = 5
    METER = 6
    KILOMETER = 7
    MICROINCH = 8
    MIL = 9
    YARD = 10
    ANGSTROM = 11
    NANOMETER = 12
    MICRON = 13
    DECIMETER = 14
    DECAMETER = 15
    HECTOMETER = 16
    GIGAMETER = 17
    ASTRONOMICAL_UNIT = 18
    LIGHTYEAR = 19
    PARSEC = 20
    US_SURVEY_FOOT = 21
    US_SURVEY_INCH = 22
    US_SURVEY_YARD = 23
    US_SURVEY_MILE = 24


class DxfFormat(StrEnum):
    ASC = "asc"
    BIN = "bin"
    BASE64 = "b64"


class DxfBackend(StrEnum):
    SVG = "svg"
    PDF = "pdf"
    PNG = "png"
    EPS = "eps"
    PS = "ps"
    JSON = "json"
    GEOJSON = "geojson"


class DxfArtifact(StrEnum):
    DXF = "dxf"
    SVG = "svg"
    PDF = "pdf"
    PNG = "png"
    EPS = "eps"
    PS = "ps"
    JSON = "json"
    GEOJSON = "geojson"


class DimKind(StrEnum):
    LINEAR = "linear"
    ALIGNED = "aligned"
    ANGULAR = "angular"
    RADIUS = "radius"
    DIAMETER = "diameter"
    ORDINATE = "ordinate"
    ARC = "arc"


class SpatialTest(StrEnum):
    INSIDE = "inside"
    OUTSIDE = "outside"
    OVERLAP = "overlap"


@tagged_union(frozen=True)
class BridgeSample:
    tag: Literal["flatten", "control"] = tag()
    flatten: float = case()
    control: None = case()


class MLeaderKind(StrEnum):
    MTEXT = "mtext"
    BLOCK = "block"


class LeaderSide(StrEnum):
    LEFT = "left"
    RIGHT = "right"
    TOP = "top"
    BOTTOM = "bottom"


class TransformMode(StrEnum):
    INPLACE = "inplace"
    COPIES = "copies"


class TextAlign(StrEnum):
    LEFT = "left"
    CENTER = "center"
    RIGHT = "right"
    MIDDLE = "middle"
    BOTTOM_LEFT = "bottom_left"
    BOTTOM_CENTER = "bottom_center"
    BOTTOM_RIGHT = "bottom_right"
    MIDDLE_LEFT = "middle_left"
    MIDDLE_CENTER = "middle_center"
    MIDDLE_RIGHT = "middle_right"
    TOP_LEFT = "top_left"
    TOP_CENTER = "top_center"
    TOP_RIGHT = "top_right"


class BackgroundPolicy(StrEnum):
    DEFAULT = "default"
    WHITE = "white"
    BLACK = "black"
    PAPERSPACE = "paperspace"
    MODELSPACE = "modelspace"
    OFF = "off"
    CUSTOM = "custom"


class LineweightPolicy(StrEnum):
    ABSOLUTE = "absolute"
    RELATIVE = "relative"
    RELATIVE_FIXED = "relative_fixed"


class HatchPolicy(StrEnum):
    NORMAL = "normal"
    IGNORE = "ignore"
    SHOW_OUTLINE = "show_outline"
    SHOW_SOLID = "show_solid"
    SHOW_APPROXIMATE_PATTERN = "show_approximate_pattern"


class TextPolicy(StrEnum):
    FILLING = "filling"
    OUTLINE = "outline"
    REPLACE_RECT = "replace_rect"
    REPLACE_FILL = "replace_fill"
    IGNORE = "ignore"


class ColorPolicy(StrEnum):
    COLOR = "color"
    COLOR_SWAP_BW = "color_swap_bw"
    COLOR_NEGATIVE = "color_negative"
    MONOCHROME = "monochrome"
    MONOCHROME_DARK_BG = "monochrome_dark_bg"
    MONOCHROME_LIGHT_BG = "monochrome_light_bg"
    BLACK = "black"
    WHITE = "white"
    CUSTOM = "custom"


class ProxyPolicy(StrEnum):
    IGNORE = "ignore"
    SHOW = "show"
    PREFER = "prefer"


class LinePolicy(StrEnum):
    SOLID = "solid"
    APPROXIMATE = "approximate"
    ACCURATE = "accurate"


# --- [CONSTANTS] ------------------------------------------------------------------------
_ZERO_EXTENT: Extent = (0.0, 0.0, 0.0, 0.0, 0.0, 0.0)
_PATH_TYPES: frozenset[str] = frozenset({"LINE", "ARC", "CIRCLE", "ELLIPSE", "SPLINE", "LWPOLYLINE", "POLYLINE", "HATCH"})
_GEO_TYPES: frozenset[str] = frozenset({"LINE", "ARC", "CIRCLE", "ELLIPSE", "SPLINE", "LWPOLYLINE", "POLYLINE", "POINT"})
_UNITS: frozendict[DxfUnits, str] = frozendict({unit: unit.name.lower() for unit in DxfUnits})
_GLYPH_TEXT: frozendict[GlyphTag, float] = frozendict({
    "node": 10.0, "edge": 8.0, "swimlane": 9.0, "annotation": 9.0, "marker": 9.0, "area": 9.0, "fragment": 8.0,
})
_ENTITY_FLOOR: Final[frozendict[str, DxfVersion]] = frozendict({
    **dict.fromkeys(
        (
            "ellipse", "spline", "rational_spline", "cad_spline", "lwpolyline", "hatch", "mpolygon", "mtext", "image",
            "wipeout", "leader", "xline", "ray", "tolerance", "body", "region", "solid3d",
        ),
        DxfVersion.R2000,
    ),
    **dict.fromkeys(("multileader", "underlay", "surface", "extruded_surface", "revolved_surface", "swept_surface"), DxfVersion.R2007),
    "mesh": DxfVersion.R2010,
})
_CAP_PREFIX: Final[str] = "RASM_CAP_"
_ANCHOR_ALIGN: frozendict[TextAnchor, TextAlign] = frozendict({
    TextAnchor.START: TextAlign.LEFT,
    TextAnchor.MIDDLE: TextAlign.CENTER,
    TextAnchor.END: TextAlign.RIGHT,
})
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
_DIM_ARITY: frozendict[DimKind, int] = frozendict({
    DimKind.LINEAR: 3,
    DimKind.ALIGNED: 3,
    DimKind.ANGULAR: 5,
    DimKind.RADIUS: 2,
    DimKind.DIAMETER: 2,
    DimKind.ORDINATE: 2,
    DimKind.ARC: 4,
})
_SPATIAL_TEST: frozendict[SpatialTest, Callable[[object, "EntityQuery"], "Iterable[DXFGraphic]"]] = frozendict({
    SpatialTest.INSIDE: lambda shape, entities: select.bbox_inside(shape, entities),
    SpatialTest.OUTSIDE: lambda shape, entities: select.bbox_outside(shape, entities),
    SpatialTest.OVERLAP: lambda shape, entities: select.bbox_overlap(shape, entities),
})



# --- [TABLES] ---------------------------------------------------------------------------

DXF_REGION: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.DXF, point="region", arm="boundary", defect="region-refused", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([DXF_REGION]))

# --- [MODELS] ---------------------------------------------------------------------------
_LINEWEIGHTS: Final[frozenset[int]] = frozenset((
    -3, -2, -1, 0, 5, 9, 13, 15, 18, 20, 25, 30, 35, 40, 50, 53, 60, 70, 80, 90, 100, 106, 120, 140, 158, 200, 211,
))


class DxfAttribs(Struct, frozen=True):
    layer: str = "0"
    color: int = 256
    rgb: Option[tuple[int, int, int]] = Nothing
    linetype: str = "ByLayer"
    lineweight: int = -1
    transparency: Option[float] = Nothing
    ltscale: float = 1.0

    @property
    def issue(self) -> Option[str]:
        faults = (
            Some("layer") if not self.layer else Nothing,
            Some("color") if not 0 <= self.color <= 256 else Nothing,
            self.rgb.bind(lambda rgb: Some("rgb") if any(not 0 <= channel <= 255 for channel in rgb) else Nothing),
            Some("linetype") if not self.linetype else Nothing,
            Some("lineweight") if self.lineweight not in _LINEWEIGHTS else Nothing,
            self.transparency.bind(lambda alpha: Some("transparency") if not 0.0 <= alpha <= 1.0 else Nothing),
            Some("ltscale") if not math.isfinite(self.ltscale) or self.ltscale <= 0.0 else Nothing,
        )
        return next((fault for fault in faults if fault.is_some()), Nothing)

    def gfx(self) -> Attribs:
        return GfxAttribs(
            layer=self.layer,
            color=self.color,
            rgb=self.rgb.map(lambda rgb: colors.RGB(*rgb)).to_optional(),
            linetype=self.linetype,
            lineweight=self.lineweight,
            transparency=self.transparency.to_optional(),
            ltscale=self.ltscale,
        ).asdict()


class Xref(Struct, frozen=True):
    block_name: str
    filename: str
    insert: Point3 = (0.0, 0.0, 0.0)
    scale: float = 1.0
    rotation: float = 0.0


class DxfRenderPolicy(Struct, frozen=True):
    background: BackgroundPolicy = BackgroundPolicy.WHITE
    lineweight: LineweightPolicy = LineweightPolicy.ABSOLUTE
    hatch: HatchPolicy = HatchPolicy.NORMAL
    text: TextPolicy = TextPolicy.FILLING
    color: ColorPolicy = ColorPolicy.COLOR
    proxy: ProxyPolicy = ProxyPolicy.SHOW
    line: LinePolicy = LinePolicy.ACCURATE
    ctb: str = ""
    export_mode: bool = False


class PageSpec(Struct, frozen=True):
    width: float = 0.0
    height: float = 0.0
    margin: float = 10.0
    dpi: int = 300
    fit_page: bool = True
    scale: float = 1.0
    policy: DxfRenderPolicy = DxfRenderPolicy()


class DxfComposed(Struct, frozen=True):
    data: bytes
    kind: DxfArtifact
    dxfversion: str
    units: str
    counts: frozendict[str, int]
    layers: int
    blocks: int
    errors: int
    fixes: int
    extent: Extent


@tagged_union(frozen=True)
class TableEntry:
    tag: Literal["layer", "linetype", "textstyle", "dimstyle", "mleaderstyle"] = tag()
    layer: tuple[str, int, str, int] = case()
    linetype: tuple[str, tuple[float, ...], str] = case()
    textstyle: tuple[str, str, float, float] = case()
    dimstyle: tuple[str, frozendict[str, DimVar]] = case()
    mleaderstyle: tuple[str, str, str, int] = case()

    @property
    def name(self) -> str:
        match self:
            case TableEntry(tag="layer", layer=(name, _, _, _)) | TableEntry(tag="linetype", linetype=(name, _, _)) | TableEntry(
                tag="textstyle", textstyle=(name, _, _, _)
            ) | TableEntry(tag="dimstyle", dimstyle=(name, _)) | TableEntry(tag="mleaderstyle", mleaderstyle=(name, _, _, _)):
                return name
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class DxfFill:
    tag: Literal["solid", "pattern", "gradient"] = tag()
    solid: int = case()
    pattern: tuple[str, tuple[PatternLine, ...], int, float, float] = case()
    gradient: tuple[tuple[int, int, int], tuple[int, int, int], float] = case()

    @property
    def issue(self) -> Option[str]:
        match self:
            case DxfFill(tag="solid", solid=color) if not 0 <= color <= 256:
                return Some("solid")
            case DxfFill(tag="pattern", pattern=(name, _, color, scale, angle)) if (
                not name or not 0 <= color <= 256 or not math.isfinite(scale) or scale <= 0.0 or not math.isfinite(angle)
            ):
                return Some("pattern")
            case DxfFill(tag="gradient", gradient=(first, second, rotation)) if not math.isfinite(rotation) or any(
                not 0 <= channel <= 255 for channel in (*first, *second)
            ):
                return Some("gradient")
            case _:
                return Nothing


@tagged_union(frozen=True)
class DxfEntity:
    tag: Literal[
        "line",
        "arc",
        "circle",
        "ellipse",
        "spline",
        "rational_spline",
        "cad_spline",
        "lwpolyline",
        "polyline2d",
        "polyline3d",
        "polymesh",
        "polyface",
        "hatch",
        "mpolygon",
        "text",
        "mtext",
        "attdef",
        "leader",
        "multileader",
        "dimension",
        "tolerance",
        "blockref",
        "image",
        "underlay",
        "ray",
        "xline",
        "point",
        "solid",
        "trace",
        "shape",
        "face3d",
        "wipeout",
        "mesh",
        "body",
        "region",
        "solid3d",
        "surface",
        "extruded_surface",
        "revolved_surface",
        "swept_surface",
    ] = tag()
    line: tuple[Point3, Point3, DxfAttribs] = case()
    arc: tuple[Point3, float, float, float, DxfAttribs] = case()
    circle: tuple[Point3, float, DxfAttribs] = case()
    ellipse: tuple[Point3, Point3, float, float, float, DxfAttribs] = case()
    spline: tuple[tuple[Point3, ...], int, DxfAttribs] = case()
    rational_spline: tuple[tuple[Point3, ...], tuple[float, ...], int, Option[tuple[float, ...]], DxfAttribs] = case()
    cad_spline: tuple[tuple[Point3, ...], Option[tuple[Point3, Point3]], DxfAttribs] = case()
    lwpolyline: tuple[tuple[tuple[float, ...], ...], bool, DxfAttribs] = case()
    polyline2d: tuple[tuple[Point2, ...], bool, DxfAttribs] = case()
    polyline3d: tuple[tuple[Point3, ...], bool, DxfAttribs] = case()
    polymesh: tuple[int, int, tuple[Point3, ...], DxfAttribs] = case()
    polyface: tuple[tuple[Point3, ...], tuple[tuple[int, ...], ...], DxfAttribs] = case()
    hatch: tuple[tuple[tuple[HatchRow, ...], ...], DxfFill, DxfAttribs] = case()
    mpolygon: tuple[tuple[tuple[HatchRow, ...], ...], int, Option[int], DxfAttribs] = case()
    text: tuple[str, Point3, float, float, TextAlign, DxfAttribs] = case()
    mtext: tuple[str, Point3, float, DxfAttribs] = case()
    attdef: tuple[str, str, Point3, float, DxfAttribs] = case()
    leader: tuple[tuple[Point3, ...], DxfAttribs] = case()
    multileader: tuple[MLeaderKind, str, str, tuple[LeaderLine, ...], Point3, DxfAttribs] = case()
    dimension: tuple[DimKind, tuple[Point3, ...], str, DxfAttribs] = case()
    tolerance: tuple[str, Point3, str, DxfAttribs] = case()
    blockref: tuple[str, Point3, float, float, frozendict[str, str], DxfAttribs] = case()
    image: tuple[str, tuple[int, int], Point3, tuple[float, float], float, DxfAttribs] = case()
    underlay: tuple[str, Point3, Point3, float, DxfAttribs] = case()
    ray: tuple[Point3, Point3, DxfAttribs] = case()
    xline: tuple[Point3, Point3, DxfAttribs] = case()
    point: tuple[Point3, DxfAttribs] = case()
    solid: tuple[tuple[Point3, ...], DxfAttribs] = case()
    trace: tuple[tuple[Point3, ...], DxfAttribs] = case()
    shape: tuple[str, Point3, float, DxfAttribs] = case()
    face3d: tuple[tuple[Point3, ...], DxfAttribs] = case()
    wipeout: tuple[tuple[Point2, ...], DxfAttribs] = case()
    mesh: tuple[tuple[Point3, ...], tuple[tuple[int, ...], ...], DxfAttribs] = case()
    body: tuple[AcisData, DxfAttribs] = case()
    region: tuple[AcisData, DxfAttribs] = case()
    solid3d: tuple[AcisData, DxfAttribs] = case()
    surface: tuple[AcisData, DxfAttribs] = case()
    extruded_surface: tuple[AcisData, DxfAttribs] = case()
    revolved_surface: tuple[AcisData, DxfAttribs] = case()
    swept_surface: tuple[AcisData, DxfAttribs] = case()

    @property
    def issue(self) -> Option[str]:
        if cast(DxfAttribs, getattr(self, self.tag)[-1]).issue.is_some():
            return Some(f"{self.tag}:attribs")
        if not _finite(getattr(self, self.tag)):
            return Some(f"{self.tag}:non-finite")
        match self:
            case DxfEntity(tag="line", line=(start, end, _)) if start == end:
                return Some("line")
            case DxfEntity(tag="arc", arc=(_, radius, _, _, _)) | DxfEntity(tag="circle", circle=(_, radius, _)) if radius <= 0.0:
                return Some(self.tag)
            case DxfEntity(tag="ellipse", ellipse=(_, major, ratio, _, _, _)) if major == (0.0, 0.0, 0.0) or not 0.0 < ratio <= 1.0:
                return Some("ellipse")
            case DxfEntity(tag="spline", spline=(points, degree, _)) if len(points) < 2 or not 1 <= degree <= 11:
                return Some("spline")
            case DxfEntity(tag="rational_spline", rational_spline=(points, weights, degree, _, _)) if (
                len(points) < 2 or len(points) != len(weights) or not 1 <= degree <= 11
            ):
                return Some("rational_spline")
            case DxfEntity(tag="cad_spline", cad_spline=(points, _, _)) if len(points) < 2:
                return Some("cad_spline")
            case DxfEntity(tag="lwpolyline", lwpolyline=(points, _, _)) | DxfEntity(tag="polyline2d", polyline2d=(points, _, _)) | DxfEntity(
                tag="polyline3d", polyline3d=(points, _, _)
            ) if len(points) < 2:
                return Some(self.tag)
            case DxfEntity(tag="polymesh", polymesh=(m_count, n_count, vertices, _)) if (
                m_count < 2 or n_count < 2 or len(vertices) != m_count * n_count
            ):
                return Some("polymesh")
            case DxfEntity(tag="polyface", polyface=(vertices, faces, _)) | DxfEntity(tag="mesh", mesh=(vertices, faces, _)) if (
                not vertices or not faces or any(len(face) < 3 or any(index < 0 or index >= len(vertices) for index in face) for face in faces)
            ):
                return Some(self.tag)
            case DxfEntity(tag="hatch", hatch=(loops, fill, _)) if not loops or any(len(loop) < 3 for loop in loops) or fill.issue.is_some():
                return Some("hatch")
            case DxfEntity(tag="mpolygon", mpolygon=(loops, boundary, fill, _)) if (
                not loops
                or any(len(loop) < 3 for loop in loops)
                or not 0 <= boundary <= 256
                or fill.bind(lambda color: Some(color) if not 0 <= color <= 256 else Nothing).is_some()
            ):
                return Some(self.tag)
            case DxfEntity(tag="text", text=(body, _, height, _, _, _)) | DxfEntity(tag="mtext", mtext=(body, _, height, _)) if not body or height <= 0.0:
                return Some(self.tag)
            case DxfEntity(tag="attdef", attdef=(name, _, _, height, _)) if not name or height <= 0.0:
                return Some("attdef")
            case DxfEntity(tag="leader", leader=(vertices, _)) if len(vertices) < 2:
                return Some("leader")
            case DxfEntity(tag="multileader", multileader=(_, content, style, lines, _, _)) if (
                not content or not style or not lines or any(len(vertices) < 2 for _, vertices in lines)
            ):
                return Some("multileader")
            case DxfEntity(tag="dimension", dimension=(kind, points, style, _)) if len(points) != _DIM_ARITY[kind] or not style:
                return Some("dimension")
            case DxfEntity(tag="tolerance", tolerance=(content, _, style, _)) if not content or not style:
                return Some("tolerance")
            case DxfEntity(tag="blockref", blockref=(name, _, scale, _, _, _)) if not name or scale == 0.0:
                return Some("blockref")
            case DxfEntity(tag="image", image=(filename, size_px, _, size_units, _, _)) if (
                not filename or min(*size_px, *size_units) <= 0.0
            ):
                return Some("image")
            case DxfEntity(tag="underlay", underlay=(filename, _, scale, _, _)) if not filename or 0.0 in scale:
                return Some("underlay")
            case DxfEntity(tag="ray", ray=(_, vector, _)) | DxfEntity(tag="xline", xline=(_, vector, _)) if vector == (0.0, 0.0, 0.0):
                return Some(self.tag)
            case DxfEntity(tag="solid", solid=(vertices, _)) | DxfEntity(tag="trace", trace=(vertices, _)) | DxfEntity(
                tag="face3d", face3d=(vertices, _)
            ) if not 3 <= len(vertices) <= 4:
                return Some(self.tag)
            case DxfEntity(tag="shape", shape=(name, _, size, _)) if not name or size <= 0.0:
                return Some("shape")
            case DxfEntity(tag="wipeout", wipeout=(vertices, _)) if len(vertices) < 3:
                return Some("wipeout")
            case DxfEntity(tag="body", body=(data, _)) | DxfEntity(tag="region", region=(data, _)) | DxfEntity(
                tag="solid3d", solid3d=(data, _)
            ) | DxfEntity(tag="surface", surface=(data, _)) | DxfEntity(tag="extruded_surface", extruded_surface=(data, _)) | DxfEntity(
                tag="revolved_surface", revolved_surface=(data, _)
            ) | DxfEntity(tag="swept_surface", swept_surface=(data, _)) if not data:
                return Some(self.tag)
            case _:
                return Nothing

class BlockDef(Struct, frozen=True):
    name: str
    entities: tuple[DxfEntity, ...] = ()


class DxfDocument(Struct, frozen=True):
    version: DxfVersion = DxfVersion.R2018
    units: DxfUnits = DxfUnits.MILLIMETER
    setup: bool = True
    standard: Option[Standard] = Nothing
    aec_layers: tuple[LayerName, ...] = ()
    dim_families: tuple[DimStyleFamily, ...] = ()
    tables: tuple[TableEntry, ...] = ()
    xrefs: tuple[Xref, ...] = ()
    blocks: tuple[BlockDef, ...] = ()
    entities: tuple[DxfEntity, ...] = ()
    fmt: DxfFormat = DxfFormat.ASC

    @property
    def issue(self) -> Option[str]:
        block_names = tuple(block.name for block in self.blocks)
        table_keys = tuple((entry.tag, entry.name) for entry in self.tables)
        entities = (*self.entities, *(entity for block in self.blocks for entity in block.entities))
        issues = tuple((index, entity.issue) for index, entity in enumerate(entities))
        resource_faults = (
            Some("block") if any(not name for name in block_names) or len(set(block_names)) != len(block_names) else Nothing,
            Some("table") if any(not name for _, name in table_keys) or len(set(table_keys)) != len(table_keys) else Nothing,
            Some("xref")
            if any(
                not ref.block_name or not ref.filename or ref.scale == 0.0 or not _finite((ref.insert, ref.scale, ref.rotation))
                for ref in self.xrefs
            )
            else Nothing,
            Some("version")
            if any(_ENTITY_FLOOR.get(entity.tag, DxfVersion.R12).value > self.version.value for entity in entities)
            or (self.version.value < DxfVersion.R2007.value and any(entry.tag == "mleaderstyle" for entry in self.tables))
            else Nothing,
            Some("standard")
            if self.standard.is_some() and (self.units is not DxfUnits.MILLIMETER or self.version.value < DxfVersion.R2007.value)
            else Nothing,
            next(
                (Some(f"entity:{index}:{issue.default_value('')}") for index, issue in issues if issue.is_some()),
                Nothing,
            ),
        )
        return next((fault for fault in resource_faults if fault.is_some()), Nothing)


@tagged_union(frozen=True)
class DxfSource:
    tag: Literal["blob", "file", "zip", "zip_member", "base64"] = tag()
    blob: bytes = case()
    file: str = case()
    zip: str = case()
    zip_member: tuple[str, str] = case()
    base64: str = case()


@tagged_union(frozen=True)
class Spatial:
    tag: Literal["window", "circle", "polygon", "fence", "point"] = tag()
    window: tuple[Point2, Point2, SpatialTest] = case()
    circle: tuple[Point2, float, SpatialTest] = case()
    polygon: tuple[tuple[Point2, ...], SpatialTest] = case()
    fence: tuple[Point2, ...] = case()
    point: Point2 = case()


class Selection(Struct, frozen=True):
    eql: str = "*"
    spatial: Option[Spatial] = Nothing


class TransformSpec(Struct, frozen=True):
    translate: Point3 = (0.0, 0.0, 0.0)
    scale: Point3 = (1.0, 1.0, 1.0)
    rotate: float = 0.0
    eql: str = "*"
    mode: TransformMode = TransformMode.INPLACE


@tagged_union(frozen=True)
class BridgeSpec:
    tag: Literal["to_svg", "from_svg", "to_geojson", "from_geojson", "text_paths"] = tag()
    to_svg: tuple[DxfSource, BridgeSample] = case()
    from_svg: tuple[bytes, DxfVersion, DxfAttribs, DxfUnits] = case()
    to_geojson: DxfSource = case()
    from_geojson: tuple[bytes, DxfVersion, DxfAttribs] = case()
    text_paths: tuple[str, str, float, Point3, DxfVersion, DxfAttribs] = case()


class DiagramLower(Struct, frozen=True):
    glyphs: tuple[DiagramGlyph, ...]
    layers: frozendict[str, LayerName]
    standard: Standard = Standard()
    cap_size: float = 3.5
    version: DxfVersion = DxfVersion.R2018
    units: DxfUnits = DxfUnits.MILLIMETER
    fmt: DxfFormat = DxfFormat.ASC


@tagged_union(frozen=True)
class DxfOp:
    tag: Literal["new", "read", "recover", "render", "query", "transform", "diagram", "bridge"] = tag()
    new: DxfDocument = case()
    read: DxfSource = case()
    recover: DxfSource = case()
    render: tuple[DxfSource, DxfBackend, PageSpec] = case()
    query: tuple[DxfSource, Selection] = case()
    transform: tuple[DxfSource, TransformSpec] = case()
    diagram: DiagramLower = case()
    bridge: BridgeSpec = case()


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class DxfFault:
    tag: Literal["source", "scalar", "geometry", "selection", "document"] = tag()
    source: str = case()
    scalar: tuple[str, float] = case()
    geometry: str = case()
    selection: None = case()
    document: str = case()


# --- [OPERATIONS] -----------------------------------------------------------------------
def _finite(raw: object, /) -> bool:
    match raw:
        case float():
            return math.isfinite(raw)
        case tuple():
            return all(_finite(item) for item in raw)
        case _:
            return True


def _canonized(raw: object, /) -> object:
    match raw:
        case Option() as slot:
            return slot.to_optional()
        case frozendict() as row:
            return dict(row)
        case DxfOp() | DxfEntity() | DxfFill() | DxfSource() | Spatial() | BridgeSpec() | BridgeSample() | TableEntry() | DiagramGlyph() as node:
            return (node.tag, getattr(node, node.tag))
        case _:
            raise NotImplementedError(type(raw).__name__)


_CANON: Final = msgspec.msgpack.Encoder(order="deterministic", enc_hook=_canonized)


def _source_fault(source: DxfSource, /) -> Option[DxfFault]:
    match source:
        case DxfSource(tag="blob", blob=b"") | DxfSource(tag="file", file="") | DxfSource(tag="zip", zip="") | DxfSource(
            tag="zip_member", zip_member=("", _)
        ) | DxfSource(tag="zip_member", zip_member=(_, "")) | DxfSource(tag="base64", base64=""):
            return Some(DxfFault(source=source.tag))
        case _:
            return Nothing


def _degenerate_polygon(vertices: tuple[Point2, ...], /) -> bool:
    if len(set(vertices)) < 3 or not all(math.isfinite(x) and math.isfinite(y) for x, y in vertices):
        return True
    return math.fsum(x0 * y1 - x1 * y0 for (x0, y0), (x1, y1) in zip(vertices, (*vertices[1:], vertices[0]), strict=True)) == 0.0


def _spatial_fault(spec: Spatial, /) -> Option[DxfFault]:
    match spec:
        case Spatial(tag="window", window=(low, high, _)) if not _finite((low, high)) or low[0] >= high[0] or low[1] >= high[1]:
            return Some(DxfFault(geometry="window"))
        case Spatial(tag="circle", circle=(center, radius, _)) if not _finite((center, radius)) or radius <= 0.0:
            return Some(DxfFault(scalar=("radius", radius)))
        case Spatial(tag="polygon", polygon=(vertices, _)) if _degenerate_polygon(vertices):
            return Some(DxfFault(geometry="polygon"))
        case Spatial(tag="fence", fence=vertices) if not _finite(vertices) or len(set(vertices)) < 2:
            return Some(DxfFault(geometry="fence"))
        case Spatial(tag="point", point=location) if not _finite(location):
            return Some(DxfFault(geometry="point"))
        case _:
            return Nothing


def _fragment_broken(d: str, /) -> bool:
    try:
        return not _fragment_rows(d)
    except (ValueError, TypeError, IndexError, AttributeError):
        return True


def _glyph_fault(glyph: DiagramGlyph, layers: frozendict[str, LayerName], /) -> Option[DxfFault]:
    if not _finite(msgspec.structs.astuple(glyph.mark)):
        return Some(DxfFault(geometry=f"{glyph.tag}:non-finite"))
    match glyph:
        case (DiagramGlyph(tag="node", node=mark) | DiagramGlyph(tag="swimlane", swimlane=mark)) if mark.w <= 0.0 or mark.h <= 0.0:
            return Some(DxfFault(scalar=(glyph.tag, min(mark.w, mark.h))))
        case DiagramGlyph(tag="edge", edge=mark) if len(mark.points) < 2:
            return Some(DxfFault(geometry="edge"))
        case DiagramGlyph(tag="edge", edge=mark) if mark.weight < 0.0 or mark.style.width < 0.0:
            return Some(DxfFault(scalar=("edge", min(mark.weight, mark.style.width))))
        case DiagramGlyph(tag="annotation", annotation=mark) if not mark.text:
            return Some(DxfFault(geometry="annotation"))
        case DiagramGlyph(tag="area", area=mark) if _degenerate_polygon(mark.ring):
            return Some(DxfFault(geometry="area"))
        case DiagramGlyph(tag="fragment", fragment=mark) if not mark.d or _fragment_broken(mark.d):
            return Some(DxfFault(geometry="fragment"))
        case _:
            pass
    style = glyph.mark.style
    if style.text is not None and style.text.size <= 0.0:
        return Some(DxfFault(scalar=("text", style.text.size)))
    if any(not math.isfinite(run) or run <= 0.0 for run in style.dash):
        return Some(DxfFault(scalar=("dash", min(style.dash))))
    return Nothing if style.layer in layers else Some(DxfFault(document=f"diagram:layer:{style.layer}"))


def _diagram_fault(spec: DiagramLower, /) -> Option[DxfFault]:
    head = (
        Some(DxfFault(document="diagram:empty"))
        if not spec.glyphs
        else Some(DxfFault(document="diagram:version"))
        if spec.version is DxfVersion.R12
        else Some(DxfFault(scalar=("cap_size", spec.cap_size)))
        if not math.isfinite(spec.cap_size) or spec.cap_size <= 0.0
        else Nothing
    )
    return head.or_else_with(lambda: next((fault for glyph in spec.glyphs if (fault := _glyph_fault(glyph, spec.layers)).is_some()), Nothing))


def _checked(op: DxfOp, /) -> Result[DxfOp, DxfFault]:
    match op:
        case DxfOp(tag="new", new=document):
            fault = document.issue.map(lambda issue: DxfFault(document=issue))
        case DxfOp(tag="read", read=source) | DxfOp(tag="recover", recover=source):
            fault = _source_fault(source)
        case DxfOp(tag="render", render=(source, _, page)):
            fault = (
                _source_fault(source)
                .or_else_with(lambda: Some(DxfFault(scalar=("dpi", float(page.dpi)))) if page.dpi <= 0 else Nothing)
                .or_else_with(lambda: Some(DxfFault(scalar=("scale", page.scale))) if not math.isfinite(page.scale) or page.scale <= 0.0 else Nothing)
                .or_else_with(lambda: Some(DxfFault(scalar=("margin", page.margin))) if not math.isfinite(page.margin) or page.margin < 0.0 else Nothing)
                .or_else_with(lambda: Some(DxfFault(scalar=("width", page.width))) if not math.isfinite(page.width) or page.width < 0.0 else Nothing)
                .or_else_with(lambda: Some(DxfFault(scalar=("height", page.height))) if not math.isfinite(page.height) or page.height < 0.0 else Nothing)
            )
        case DxfOp(tag="query", query=(source, selection)):
            fault = (
                _source_fault(source)
                .or_else_with(lambda: Some(DxfFault(selection=None)) if not selection.eql else Nothing)
                .or_else_with(lambda: selection.spatial.bind(_spatial_fault))
            )
        case DxfOp(tag="transform", transform=(source, spec)):
            fault = (
                _source_fault(source)
                .or_else_with(lambda: Some(DxfFault(selection=None)) if not spec.eql else Nothing)
                .or_else_with(lambda: next((Some(DxfFault(scalar=("scale", axis))) for axis in spec.scale if axis == 0.0), Nothing))
                .or_else_with(lambda: Some(DxfFault(geometry="transform")) if not _finite((spec.translate, spec.scale, spec.rotate)) else Nothing)
            )
        case DxfOp(tag="diagram", diagram=spec):
            fault = _diagram_fault(spec)
        case DxfOp(tag="bridge", bridge=BridgeSpec(tag="to_svg", to_svg=(source, BridgeSample(tag="flatten", flatten=distance)))):
            fault = _source_fault(source).or_else_with(
                lambda: Some(DxfFault(scalar=("distance", distance))) if not math.isfinite(distance) or distance <= 0.0 else Nothing
            )
        case DxfOp(tag="bridge", bridge=BridgeSpec(tag="to_svg", to_svg=(source, BridgeSample(tag="control")))):
            fault = _source_fault(source)
        case DxfOp(tag="bridge", bridge=BridgeSpec(tag="to_geojson", to_geojson=source)):
            fault = _source_fault(source)
        case DxfOp(tag="bridge", bridge=BridgeSpec(tag="from_svg", from_svg=(source, _, _, _))):
            fault = Some(DxfFault(geometry="from_svg")) if not source else Nothing
        case DxfOp(tag="bridge", bridge=BridgeSpec(tag="from_geojson", from_geojson=(mapping, _, _))):
            fault = Some(DxfFault(geometry="from_geojson")) if not mapping else Nothing
        case DxfOp(tag="bridge", bridge=BridgeSpec(tag="text_paths", text_paths=(text, _, size, _, _, _))):
            fault = (Some(DxfFault(geometry="text_paths")) if not text else Nothing).or_else_with(
                lambda: Some(DxfFault(scalar=("size", size))) if not math.isfinite(size) or size <= 0.0 else Nothing
            )
        case _ as unreachable:
            assert_never(unreachable)
    match fault:
        case Option(tag="some", some=admitted):
            return Error(admitted)
        case _:
            return Ok(op)


def _ingest(source: DxfSource, /) -> "Drawing":
    match source:
        case DxfSource(tag="blob", blob=data):
            with TemporaryDirectory(prefix="rasm-dxf-") as scratch:
                path = Path(scratch) / "source.dxf"
                path.write_bytes(data)
                return ezdxf.readfile(path)
        case DxfSource(tag="file", file=path):
            return ezdxf.readfile(path)
        case DxfSource(tag="zip", zip=path):
            return ezdxf.readzip(path)
        case DxfSource(tag="zip_member", zip_member=(path, member)):
            return ezdxf.readzip(path, member)
        case DxfSource(tag="base64", base64=text):
            return ezdxf.decode_base64(text.encode())
        case _ as unreachable:
            assert_never(unreachable)


def _binary(source: DxfSource, /) -> bytes:
    match source:
        case DxfSource(tag="blob", blob=data):
            return data
        case DxfSource(tag="base64", base64=text):
            return base64.b64decode(text)
        case DxfSource(tag="zip", zip=path):
            with zipfile.ZipFile(path) as archive:
                return archive.read(next(name for name in archive.namelist() if name.lower().endswith(".dxf")))
        case DxfSource(tag="zip_member", zip_member=(path, member)):
            with zipfile.ZipFile(path) as archive:
                return archive.read(member)
        case DxfSource(tag="file", file=path):
            return Path(path).read_bytes()
        case _ as unreachable:
            assert_never(unreachable)


def _recovered(source: DxfSource, /) -> "tuple[Drawing, Auditor]":
    match source:
        case DxfSource(tag="file", file=path):
            return recover.readfile(path)
        case _:
            return recover.read(io.BytesIO(_binary(source)))


def _build_entity(layout: "Modelspace | BlockLayout", entity: DxfEntity, /) -> None:
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
        case DxfEntity(tag="rational_spline", rational_spline=(control, weights, degree, knots, at)):
            layout.add_rational_spline(control, weights, degree=degree, knots=knots.to_optional(), dxfattribs=at.gfx())
        case DxfEntity(tag="cad_spline", cad_spline=(fit, tangents, at)):
            layout.add_cad_spline_control_frame(fit, tangents=tangents.to_optional(), dxfattribs=at.gfx())
        case DxfEntity(tag="lwpolyline", lwpolyline=(points, close, at)):
            layout.add_lwpolyline(points, format="xyseb", close=close, dxfattribs=at.gfx())
        case DxfEntity(tag="polyline2d", polyline2d=(vertices, close, at)):
            layout.add_polyline2d(vertices, close=close, dxfattribs=at.gfx())
        case DxfEntity(tag="polyline3d", polyline3d=(vertices, close, at)):
            layout.add_polyline3d(vertices, close=close, dxfattribs=at.gfx())
        case DxfEntity(tag="polymesh", polymesh=(m_count, n_count, vertices, at)):
            body = layout.add_polymesh((m_count, n_count), dxfattribs=at.gfx())
            for index, vertex in enumerate(vertices):
                body.set_mesh_vertex(divmod(index, n_count), vertex)
        case DxfEntity(tag="polyface", polyface=(vertices, faces, at)):
            body = layout.add_polyface(dxfattribs=at.gfx())
            body.append_faces(tuple(tuple(vertices[index] for index in face) for face in faces))
        case DxfEntity(tag="hatch", hatch=(loops, fill, at)):
            hatched = layout.add_hatch(dxfattribs=at.gfx())
            for loop in loops:
                hatched.paths.add_polyline_path(loop, is_closed=True)
            match fill:
                case DxfFill(tag="solid", solid=color):
                    hatched.set_solid_fill(color=color)
                case DxfFill(tag="pattern", pattern=(name, definition, color, scale, angle)):
                    hatched.set_pattern_fill(name, color=color, scale=scale, angle=angle, definition=list(definition) or None)
                case DxfFill(tag="gradient", gradient=(rgb1, rgb2, rotation)):
                    hatched.set_gradient(color1=colors.RGB(*rgb1), color2=colors.RGB(*rgb2), rotation=rotation)
                case _ as unreachable:
                    assert_never(unreachable)
        case DxfEntity(tag="mpolygon", mpolygon=(loops, boundary_color, fill_color, at)):
            body = layout.add_mpolygon(color=boundary_color, fill_color=fill_color.to_optional(), dxfattribs=at.gfx())
            for loop in loops:
                body.paths.add_polyline_path(loop, is_closed=True)
        case DxfEntity(tag="text", text=(body, insert, height, rotation, align, at)):
            layout.add_text(body, height=height, dxfattribs={**at.gfx(), "rotation": rotation}).set_placement(
                insert, align=getattr(TextEntityAlignment, align.name)
            )
        case DxfEntity(tag="mtext", mtext=(body, insert, height, at)):
            layout.add_mtext(body, dxfattribs={**at.gfx(), "char_height": height, "insert": insert})
        case DxfEntity(tag="attdef", attdef=(tag_name, default, insert, height, at)):
            layout.add_attdef(tag_name, insert, default, height=height, dxfattribs=at.gfx())
        case DxfEntity(tag="leader", leader=(vertices, at)):
            layout.add_leader(vertices, dxfattribs=at.gfx())
        case DxfEntity(tag="multileader", multileader=(kind, content, style, lines, insert, at)):
            builder = (
                layout.add_multileader_block(style, dxfattribs=at.gfx())
                if kind is MLeaderKind.BLOCK
                else layout.add_multileader_mtext(style, dxfattribs=at.gfx())
            )
            builder.set_content(content)
            for side, vertices in lines:
                builder.add_leader_line(getattr(mleader.ConnectionSide, side.value), list(vertices))
            builder.build(insert=insert)
        case DxfEntity(tag="dimension", dimension=(kind, defpoints, dimstyle, at)):
            _DIM[kind](layout, defpoints, dimstyle, at.gfx()).render()
        case DxfEntity(tag="tolerance", tolerance=(content, insert, dimstyle, at)):
            layout.new_entity("TOLERANCE", dxfattribs={**at.gfx(), "content": content, "insert": insert, "dimstyle": dimstyle})
        case DxfEntity(tag="blockref", blockref=(name, insert, scale, rotation, values, at)):
            layout.add_blockref(
                name, insert, dxfattribs={**at.gfx(), "xscale": scale, "yscale": scale, "rotation": rotation}
            ).add_auto_attribs(dict(values))
        case DxfEntity(tag="image", image=(filename, size_px, insert, size_units, rotation, at)):
            image_def = layout.doc.add_image_def(filename, size_px)
            layout.add_image(image_def, insert, size_units, rotation=rotation, dxfattribs=at.gfx())
        case DxfEntity(tag="underlay", underlay=(filename, insert, scale, rotation, at)):
            underlay_def = layout.doc.add_underlay_def(filename)
            layout.add_underlay(underlay_def, insert, scale=scale, rotation=rotation, dxfattribs=at.gfx())
        case DxfEntity(tag="ray", ray=(start, unit_vector, at)):
            layout.add_ray(start, unit_vector, dxfattribs=at.gfx())
        case DxfEntity(tag="xline", xline=(base_point, unit_vector, at)):
            layout.add_xline(base_point, unit_vector, dxfattribs=at.gfx())
        case DxfEntity(tag="point", point=(location, at)):
            layout.add_point(location, dxfattribs=at.gfx())
        case DxfEntity(tag="solid", solid=(vertices, at)):
            layout.add_solid(vertices, dxfattribs=at.gfx())
        case DxfEntity(tag="trace", trace=(vertices, at)):
            layout.add_trace(vertices, dxfattribs=at.gfx())
        case DxfEntity(tag="shape", shape=(name, insert, size, at)):
            layout.add_shape(name, insert, size, dxfattribs=at.gfx())
        case DxfEntity(tag="face3d", face3d=(vertices, at)):
            layout.add_3dface(vertices, dxfattribs=at.gfx())
        case DxfEntity(tag="wipeout", wipeout=(vertices, at)):
            layout.add_wipeout(vertices, dxfattribs=at.gfx())
        case DxfEntity(tag="mesh", mesh=(vertices, faces, at)):
            body = layout.add_mesh(dxfattribs=at.gfx())
            with body.edit_data() as data:
                data.vertices, data.faces = list(vertices), list(faces)
        case DxfEntity(tag="body", body=(data, at)):
            acis.export_dxf(layout.add_body(dxfattribs=at.gfx()), acis.load(data))
        case DxfEntity(tag="region", region=(data, at)):
            acis.export_dxf(layout.add_region(dxfattribs=at.gfx()), acis.load(data))
        case DxfEntity(tag="solid3d", solid3d=(data, at)):
            acis.export_dxf(layout.add_3dsolid(dxfattribs=at.gfx()), acis.load(data))
        case DxfEntity(tag="surface", surface=(data, at)):
            acis.export_dxf(layout.add_surface(dxfattribs=at.gfx()), acis.load(data))
        case DxfEntity(tag="extruded_surface", extruded_surface=(data, at)):
            acis.export_dxf(layout.add_extruded_surface(dxfattribs=at.gfx()), acis.load(data))
        case DxfEntity(tag="revolved_surface", revolved_surface=(data, at)):
            acis.export_dxf(layout.add_revolved_surface(dxfattribs=at.gfx()), acis.load(data))
        case DxfEntity(tag="swept_surface", swept_surface=(data, at)):
            acis.export_dxf(layout.add_swept_surface(dxfattribs=at.gfx()), acis.load(data))
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
        case TableEntry(tag="mleaderstyle", mleaderstyle=(name, text_style, linetype, lineweight)):
            style = doc.mleader_styles.new(name)
            style.set_mtext_style(text_style)
            style.set_leader_properties(linetype=linetype, lineweight=lineweight)
        case _ as unreachable:
            assert_never(unreachable)


def _attach_xref(doc: "Drawing", ref: Xref, /) -> None:
    xref.attach(doc, block_name=ref.block_name, filename=ref.filename, insert=ref.insert, scale=ref.scale, rotation=ref.rotation)


def _authored(spec: DxfDocument, /) -> "tuple[Drawing, Auditor]":
    doc = ezdxf.new(spec.version.value, setup=spec.setup, units=int(spec.units))
    match spec.standard:
        case Option(tag="some", some=standard):
            standard.seed(doc, spec.aec_layers, spec.dim_families)
        case _:
            pass
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
    return frozendict(Counter(entity.dxftype() for block in doc.blocks for entity in block))


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


def _evidence(doc: "Drawing", /) -> DxfComposed:
    return DxfComposed(
        data=b"",
        kind=DxfArtifact.DXF,
        dxfversion=doc.dxfversion,
        units=_UNITS[DxfUnits(doc.units)],
        counts=_counts(doc),
        layers=len(doc.layers),
        blocks=len(doc.blocks),
        errors=0,
        fixes=0,
        extent=_extent(doc),
    )


def _dxf_composed(doc: "Drawing", auditor: "Auditor", fmt: DxfFormat, /) -> DxfComposed:
    return msgspec.structs.replace(_evidence(doc), data=_serialize(doc, fmt), errors=len(auditor.errors), fixes=len(auditor.fixes))


def _flattened(doc: "Drawing", sample: BridgeSample, /) -> Iterator[np.ndarray]:
    for entity in doc.modelspace():
        if entity.dxftype() in _PATH_TYPES:
            outline = dxfpath.make_path(entity)
            match sample:
                case BridgeSample(tag="flatten", flatten=distance):
                    verts = outline.flattening(distance)
                case BridgeSample(tag="control"):
                    verts = outline.control_vertices()
                case _ as unreachable:
                    assert_never(unreachable)
            array = np.asarray(Vec3.list(verts))
            if array.size:
                yield array


def _polyline(vertices: np.ndarray, /) -> str:
    return "M" + " L".join(f"{x:g},{y:g}" for x, y, _ in vertices)


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
    base = _evidence(doc)
    match backend:
        case DxfBackend.SVG:
            sink = SVGBackend()
            Frontend(context, sink, config).draw_layout(doc.modelspace(), finalize=True)
            return msgspec.structs.replace(base, data=sink.get_string(layout, settings=settings).encode(), kind=DxfArtifact.SVG)
        case DxfBackend.PDF:
            sink = PyMuPdfBackend()
            Frontend(context, sink, config).draw_layout(doc.modelspace(), finalize=True)
            return msgspec.structs.replace(base, data=sink.get_pdf_bytes(layout, settings=settings), kind=DxfArtifact.PDF)
        case DxfBackend.PNG:
            sink = PyMuPdfBackend()
            Frontend(context, sink, config).draw_layout(doc.modelspace(), finalize=True)
            return msgspec.structs.replace(base, data=sink.get_pixmap_bytes(layout, settings=settings, dpi=page.dpi), kind=DxfArtifact.PNG)
        case DxfBackend.EPS | DxfBackend.PS:
            figure = Figure()
            Frontend(context, MatplotlibBackend(figure.add_axes((0.0, 0.0, 1.0, 1.0))), config).draw_layout(doc.modelspace(), finalize=True)
            vector = io.BytesIO()
            figure.savefig(vector, format=backend.value, dpi=page.dpi)
            return msgspec.structs.replace(base, data=vector.getvalue(), kind=DxfArtifact(backend.value))
        case DxfBackend.JSON | DxfBackend.GEOJSON:
            sink = CustomJSONBackend() if backend is DxfBackend.JSON else GeoJSONBackend()
            Frontend(context, sink, config).draw_layout(doc.modelspace(), finalize=True)
            return msgspec.structs.replace(base, data=msgspec.json.encode(sink.get_json_data()), kind=DxfArtifact(backend.value))
        case _ as unreachable:
            assert_never(unreachable)


def _queried(source: DxfSource, selection: Selection, /) -> DxfComposed:
    doc = _ingest(source)
    matched = doc.query(selection.eql)
    entities = selection.spatial.map(lambda spatial: _spatial(matched, spatial)).default_with(lambda: list(matched))
    extract = ezdxf.new(doc.dxfversion, setup=True, units=doc.units)
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
        errors=0,
        fixes=0,
        extent=(*box.extmin.xyz, *box.extmax.xyz) if box.has_data else _ZERO_EXTENT,
    )


def _transformed(source: DxfSource, spec: TransformSpec, /) -> DxfComposed:
    doc = _ingest(source)
    matrix = Matrix44.chain(Matrix44.scale(*spec.scale), Matrix44.z_rotate(spec.rotate), Matrix44.translate(*spec.translate))
    entities = doc.query(spec.eql)
    match spec.mode:
        case TransformMode.INPLACE:
            transform.inplace(entities, matrix)
        case TransformMode.COPIES:
            _, copied = transform.copies(entities, matrix)
            msp = doc.modelspace()
            for entity in copied:
                msp.add_entity(entity)
        case _ as unreachable:
            assert_never(unreachable)
    return _dxf_composed(doc, doc.audit(), DxfFormat.ASC)


def _svg_rings(shapes: tuple[object, ...], /) -> Result[tuple[tuple[tuple[Point2, ...], bool], ...], PathFault]:
    rings: list[tuple[tuple[Point2, ...], bool]] = []
    for shape in shapes:
        for sub in SvgPath(shape).as_subpaths():
            pts = tuple((float(point.x), float(point.y)) for point in SvgPath(*sub.segments()).as_points())
            if len(set(pts)) >= 2:
                rings.append((pts, any(isinstance(segment, Close) for segment in sub.segments())))
    return Ok(tuple(rings)) if rings else Error(PathFault(empty=None))


def _fragment_rows(d: str, /) -> tuple[tuple[tuple[Point2, ...], bool], ...]:
    return tuple(
        (pts, any(isinstance(segment, Close) for segment in sub.segments()))
        for sub in SvgPath(d).as_subpaths()
        if len(set(pts := tuple((float(point.x), float(point.y)) for point in SvgPath(*sub.segments()).as_points()))) >= 2
    )


def _xyseb(x: float, y: float, bulge: float = 0.0, width: float = 0.0, /) -> PolyRow:
    return (x, y, width, width, bulge)


def _silhouette_ring(*points: Point2) -> tuple[PolyRow, ...]:
    return tuple(_xyseb(px, py) for px, py in points)


_SILHOUETTE: frozendict[NodeShape, Silhouette] = frozendict({
    NodeShape.RECTANGLE: lambda x, y, w, h: _silhouette_ring((x, y), (x + w, y), (x + w, y + h), (x, y + h)),
    NodeShape.DIAMOND: lambda x, y, w, h: _silhouette_ring((x + w / 2, y), (x + w, y + h / 2), (x + w / 2, y + h), (x, y + h / 2)),
    NodeShape.PARALLELOGRAM: lambda x, y, w, h: _silhouette_ring((x, y), (x + w * 0.8, y), (x + w, y + h), (x + w * 0.2, y + h)),
    NodeShape.HEXAGON: lambda x, y, w, h: _silhouette_ring(
        (x + w * 0.15, y), (x + w * 0.85, y), (x + w, y + h / 2), (x + w * 0.85, y + h), (x + w * 0.15, y + h), (x, y + h / 2)
    ),
    NodeShape.MANUAL_INPUT: lambda x, y, w, h: _silhouette_ring((x, y), (x + w, y), (x + w, y + h), (x, y + h * 0.75)),
    NodeShape.MANUAL_OPERATION: lambda x, y, w, h: _silhouette_ring((x + w * 0.15, y), (x + w * 0.85, y), (x + w, y + h), (x, y + h)),
    NodeShape.OFF_PAGE: lambda x, y, w, h: _silhouette_ring((x + w / 2, y), (x + w, y + h * 0.4), (x + w, y + h), (x, y + h), (x, y + h * 0.4)),
    NodeShape.CARD: lambda x, y, w, h: _silhouette_ring(
        (x, y), (x + w, y), (x + w, y + h), (x + min(w, h) * 0.25, y + h), (x, y + h - min(w, h) * 0.25)
    ),
    NodeShape.CYLINDER: lambda x, y, w, h: (
        _xyseb(x, y + h * 0.88, 2 * (h * 0.12) / w),
        _xyseb(x + w, y + h * 0.88),
        _xyseb(x + w, y + h * 0.12, 2 * (h * 0.12) / w),
        _xyseb(x, y + h * 0.12),
    ),
    NodeShape.DOCUMENT: lambda x, y, w, h: (
        _xyseb(x, y + h),
        _xyseb(x + w, y + h),
        _xyseb(x + w, y + h * 0.15, 4 * (h * 0.15) / w),
        _xyseb(x + w / 2, y + h * 0.15, -4 * (h * 0.15) / w),
        _xyseb(x, y + h * 0.15),
    ),
    NodeShape.STORED_DATA: lambda x, y, w, h: (
        _xyseb(x + w * 0.15, y + h),
        _xyseb(x + w, y + h, -2 * (w * 0.15) / h),
        _xyseb(x + w, y),
        _xyseb(x + w * 0.15, y, 2 * (w * 0.15) / h),
    ),
    NodeShape.DISPLAY: lambda x, y, w, h: (
        _xyseb(x + w * 0.2, y + h),
        _xyseb(x + w * 0.8, y + h, 2 * (w * 0.2) / h),
        _xyseb(x + w * 0.8, y),
        _xyseb(x + w * 0.2, y),
        _xyseb(x, y + h / 2),
    ),
    NodeShape.DELAY: lambda x, y, w, h: (_xyseb(x, y + h), _xyseb(x + w - h / 2, y + h, 1.0), _xyseb(x + w - h / 2, y), _xyseb(x, y)),
    NodeShape.TAPE: lambda x, y, w, h: (
        _xyseb(x, y + h * 0.88, 4 * (h * 0.12) / w),
        _xyseb(x + w / 2, y + h * 0.88, -4 * (h * 0.12) / w),
        _xyseb(x + w, y + h * 0.88),
        _xyseb(x + w, y + h * 0.12, 4 * (h * 0.12) / w),
        _xyseb(x + w / 2, y + h * 0.12, -4 * (h * 0.12) / w),
        _xyseb(x, y + h * 0.12),
    ),
})
_ARMED: Final[frozenset[NodeShape]] = frozenset({
    NodeShape.OVAL, NodeShape.CONNECTOR, NodeShape.ENTITY, NodeShape.PREDEFINED_PROCESS, NodeShape.MULTI_DOCUMENT
})

_COVERED: Final[tuple[tuple[frozenset[object], frozenset[object]], ...]] = (
    (frozenset(_SILHOUETTE) | _ARMED, frozenset(NodeShape)),
    (frozenset(_SILHOUETTE) & {NodeShape.RECTANGLE, NodeShape.DOCUMENT}, frozenset({NodeShape.RECTANGLE, NodeShape.DOCUMENT})),
    (frozenset(_DIM), frozenset(DimKind)),
    (frozenset(_DIM_ARITY), frozenset(DimKind)),
    (frozenset(_SPATIAL_TEST), frozenset(SpatialTest)),
    (frozenset(_ANCHOR_ALIGN), frozenset(TextAnchor)),
    (frozenset(_GLYPH_TEXT), frozenset(get_args(GlyphTag))),
)
if any(rows != vocabulary for rows, vocabulary in _COVERED):
    raise RuntimeError("dxf tables do not cover their vocabularies")


def _label_entity(text: str, x: float, y: float, height: float, rotation: float, at: DxfAttribs, align: TextAlign = TextAlign.MIDDLE_CENTER, /) -> DxfEntity:
    return DxfEntity(text=(text, (x, y, 0.0), height, rotation, align, at))


def _cap_block(cap: EndCap, /) -> tuple[DxfEntity, ...]:
    at = DxfAttribs()
    p = lambda a, b: (a - 2.0, b, 0.0)
    if (er := ER_CAPS.get(cap)) is not None:
        ring, bar, fan = er
        bar_a = -1.2 if fan else 0.4
        return (
            *((DxfEntity(circle=(p(-1.6 if fan else -1.4, 0.0), 0.7, at)),) if ring else ()),
            *((DxfEntity(line=(p(bar_a, -1.4), p(bar_a, 1.4), at)),) if bar else ()),
            *(
                (
                    DxfEntity(line=(p(-0.4, 0.0), p(2.0, -1.4), at)),
                    DxfEntity(line=(p(-0.4, 0.0), p(2.0, 0.0), at)),
                    DxfEntity(line=(p(-0.4, 0.0), p(2.0, 1.4), at)),
                )
                if fan
                else ()
            ),
        )
    match cap:
        case EndCap.ARROW:
            return (DxfEntity(solid=((p(2.0, 0.0), p(-2.0, 1.4), p(-2.0, -1.4)), at)),)
        case EndCap.OPEN:
            return (DxfEntity(line=(p(2.0, 0.0), p(-2.0, 1.4), at)), DxfEntity(line=(p(2.0, 0.0), p(-2.0, -1.4), at)))
        case EndCap.BLOCK:
            return (DxfEntity(solid=((p(-1.4, -1.2), p(1.4, -1.2), p(-1.4, 1.2), p(1.4, 1.2)), at)),)
        case EndCap.DIAMOND_OPEN:
            return (DxfEntity(lwpolyline=(tuple(_xyseb(*p(a, b)[:2]) for a, b in ((-2.0, 0.0), (0.0, -1.4), (2.0, 0.0), (0.0, 1.4))), True, at)),)
        case EndCap.DIAMOND_FILLED:
            return (DxfEntity(solid=((p(-2.0, 0.0), p(0.0, -1.4), p(0.0, 1.4), p(2.0, 0.0)), at)),)
        case EndCap.CIRCLE:
            return (DxfEntity(circle=(p(0.0, 0.0), 1.2, at)),)
        case EndCap.CROSS:
            return (DxfEntity(line=(p(-1.0, -1.4), p(1.0, 1.4), at)), DxfEntity(line=(p(-1.0, 1.4), p(1.0, -1.4), at)))
        case _:
            return ()


def _cap_ref(tip: Point2, back: Point2, cap: EndCap, size: float, at: DxfAttribs, /) -> DxfEntity:
    angle = math.degrees(math.atan2(tip[1] - back[1], tip[0] - back[0]))
    return DxfEntity(blockref=(f"{_CAP_PREFIX}{cap.value}", (tip[0], tip[1], 0.0), size / 4.0, angle, frozendict(), at))


def _dash_name(dash: tuple[float, ...], /) -> str:
    return "RASM_DASH_" + "_".join(f"{run:g}" for run in dash).replace(".", "p")


def _dash_linetype(dash: tuple[float, ...], /) -> TableEntry:
    runs = dash if len(dash) % 2 == 0 else (*dash, *dash)
    pattern = (math.fsum(runs), *(run if index % 2 == 0 else -run for index, run in enumerate(runs)))
    return TableEntry(linetype=(_dash_name(dash), pattern, "glyph dash " + " ".join(f"{run:g}" for run in dash)))


def _node_entities(n: NodeMark, size: float, at: DxfAttribs, /) -> tuple[DxfEntity, ...]:
    bx, by = n.x, -(n.y + n.h)
    seats = tuple(
        DxfEntity(circle=((px, -py, 0.0), max(1.5, n.style.width), at))
        for port in n.ports
        for px, py in (port.seat(n.x, n.y, n.w, n.h),)
    )
    match n.shape:
        case NodeShape.OVAL:
            major, ratio = ((n.w / 2.0, 0.0, 0.0), n.h / n.w) if n.w >= n.h else ((0.0, n.h / 2.0, 0.0), n.w / n.h)
            body = (DxfEntity(ellipse=((bx + n.w / 2, by + n.h / 2, 0.0), major, ratio, 0.0, math.tau, at)),)
        case NodeShape.CONNECTOR:
            body = (DxfEntity(circle=((bx + n.w / 2, by + n.h / 2, 0.0), min(n.w, n.h) / 2, at)),)
        case NodeShape.ENTITY:
            header = by + n.h - ENTITY_BAND
            body = (
                DxfEntity(lwpolyline=(_SILHOUETTE[NodeShape.RECTANGLE](bx, by, n.w, n.h), True, at)),
                DxfEntity(line=((bx, header, 0.0), (bx + n.w, header, 0.0), at)),
            )
        case NodeShape.PREDEFINED_PROCESS:
            bar = n.w * 0.12
            body = (
                DxfEntity(lwpolyline=(_SILHOUETTE[NodeShape.RECTANGLE](bx, by, n.w, n.h), True, at)),
                DxfEntity(line=((bx + bar, by, 0.0), (bx + bar, by + n.h, 0.0), at)),
                DxfEntity(line=((bx + n.w - bar, by, 0.0), (bx + n.w - bar, by + n.h, 0.0), at)),
            )
        case NodeShape.MULTI_DOCUMENT:
            off = min(n.w, n.h) * 0.1
            body = (
                DxfEntity(lwpolyline=(_SILHOUETTE[NodeShape.RECTANGLE](bx + 2 * off, by + 2 * off, n.w - 2 * off, n.h - 2 * off), True, at)),
                DxfEntity(lwpolyline=(_SILHOUETTE[NodeShape.RECTANGLE](bx + off, by + off, n.w - 2 * off, n.h - 2 * off), True, at)),
                DxfEntity(lwpolyline=(_SILHOUETTE[NodeShape.DOCUMENT](bx, by, n.w - 2 * off, n.h - 2 * off), True, at)),
            )
        case silhouette:
            body = (DxfEntity(lwpolyline=(_SILHOUETTE[silhouette](bx, by, n.w, n.h), True, at)),)
    if not n.label:
        return (*body, *seats)
    title_y = by + n.h - ENTITY_BAND / 2 if n.shape is NodeShape.ENTITY else by + n.h / 2
    return (*body, *seats, _label_entity(n.label, bx + n.w / 2, title_y, size, 0.0, at))


def _edge_entities(e: EdgeMark, size: float, cap_size: float, at: DxfAttribs, /) -> tuple[DxfEntity, ...]:
    placed = tuple((px, -py) for px, py in e.points)
    spine = (
        DxfEntity(spline=(tuple((px, py, 0.0) for px, py in placed), 3, at))
        if e.route is EdgeRoute.SPLINES and len(placed) > 2
        else DxfEntity(lwpolyline=(tuple(_xyseb(px, py, 0.0, e.weight) for px, py in placed), False, at))
    )
    solid = msgspec.structs.replace(at, linetype="ByLayer")
    caps = tuple(
        _cap_ref(placed[end], placed[adjacent], cap, cap_size, solid)
        for end, adjacent, cap in ((0, 1, e.caps[0]), (-1, -2, e.caps[1]))
        if cap is not EndCap.NONE
    )
    if not e.label:
        return (spine, *caps)
    mx, my = placed[len(placed) // 2]
    return (spine, *caps, _label_entity(e.label, mx, my, size, 0.0, at))


def _marker_entities(m: MarkerMark, at: DxfAttribs, /) -> tuple[DxfEntity, ...]:
    cx, cy = m.x, -m.y
    spin = math.radians(-m.angle)
    ca, sa = math.cos(spin), math.sin(spin)

    def w(lx: float, ly: float, /) -> Point3:
        return (cx + lx * ca - ly * sa, cy + lx * sa + ly * ca, 0.0)

    match m.kind:
        case MarkerKind.DOT:
            return (DxfEntity(circle=((cx, cy, 0.0), m.style.width * 2.0, at)),)
        case MarkerKind.ARROW:
            return (
                DxfEntity(line=(w(-4.0, 0.0), w(4.0, 0.0), at)),
                DxfEntity(line=(w(1.5, -2.0), w(4.0, 0.0), at)),
                DxfEntity(line=(w(1.5, 2.0), w(4.0, 0.0), at)),
            )
        case MarkerKind.TICK:
            return (DxfEntity(line=(w(-4.0, 0.0), w(4.0, 0.0), at)),)
        case MarkerKind.NORTH:
            return (DxfEntity(solid=((w(0.0, 6.0), w(-3.0, -4.0), w(3.0, -4.0)), at)),)
        case MarkerKind.CROSS:
            return (DxfEntity(line=(w(-3.0, 0.0), w(3.0, 0.0), at)), DxfEntity(line=(w(0.0, -3.0), w(0.0, 3.0), at)))
        case _ as unreachable:
            assert_never(unreachable)


def _area_entities(r: AreaMark, size: float, at: DxfAttribs, /) -> tuple[DxfEntity, ...]:
    ring = tuple((px, -py) for px, py in r.ring)
    body = (
        DxfEntity(hatch=((tuple((px, py, 0.0) for px, py in ring),), DxfFill(solid=256), at)),
        DxfEntity(lwpolyline=(tuple(_xyseb(px, py) for px, py in ring), True, at)),
    )
    text = r.label or (f"{r.magnitude:g}" if r.magnitude else None)
    if text is None:
        return body
    cx, cy = r.centroid
    return (*body, _label_entity(text, cx, -cy, size, 0.0, at))


def _glyph_entities(glyph: DiagramGlyph, spec: DiagramLower, /) -> tuple[DxfEntity, ...]:
    style = glyph.mark.style
    at = DxfAttribs(layer=spec.layers[style.layer].compose(), linetype=_dash_name(style.dash) if style.dash else "ByLayer")
    size = style.text.size if style.text is not None else _GLYPH_TEXT[glyph.tag]
    match glyph:
        case DiagramGlyph(tag="node", node=n):
            return _node_entities(n, size, at)
        case DiagramGlyph(tag="edge", edge=e):
            return _edge_entities(e, size, spec.cap_size, at)
        case DiagramGlyph(tag="swimlane", swimlane=s):
            band = DxfEntity(lwpolyline=(_SILHOUETTE[NodeShape.RECTANGLE](s.x, -(s.y + s.h), s.w, s.h), True, at))
            title = () if not s.title else (_label_entity(s.title, s.x + 4.0, -(s.y + 12.0), size, 0.0, at, TextAlign.LEFT),)
            return (band, *title)
        case DiagramGlyph(tag="annotation", annotation=a):
            return (_label_entity(a.text, a.x, -a.y, size, -a.angle, at, _ANCHOR_ALIGN[a.anchor]),)
        case DiagramGlyph(tag="marker", marker=m):
            return _marker_entities(m, at)
        case DiagramGlyph(tag="area", area=r):
            return _area_entities(r, size, at)
        case DiagramGlyph(tag="fragment", fragment=f):
            paths = tuple(
                DxfEntity(lwpolyline=(tuple(_xyseb(px, -py) for px, py in ring), closed, at)) for ring, closed in _fragment_rows(f.d)
            )
            label = () if not f.label else (_label_entity(f.label, f.anchor[0], -f.anchor[1], size, 0.0, at),)
            return (*paths, *label)
        case _ as unreachable:
            assert_never(unreachable)


def _diagram_entities(spec: DiagramLower, /) -> Result[tuple[DxfEntity, ...], RegionFault]:
    return Ok(tuple(entity for glyph in spec.glyphs for entity in _glyph_entities(glyph, spec)))


def _diagram_document(spec: DiagramLower, entities: tuple[DxfEntity, ...], /) -> DxfDocument:
    used = tuple(dict.fromkeys(
        entity.blockref[0] for entity in entities if entity.tag == "blockref" and entity.blockref[0].startswith(_CAP_PREFIX)
    ))
    dashes = tuple(dict.fromkeys(glyph.mark.style.dash for glyph in spec.glyphs if glyph.mark.style.dash))
    return DxfDocument(
        version=spec.version,
        units=spec.units,
        standard=Some(spec.standard),
        aec_layers=tuple(dict.fromkeys(spec.layers.values())),
        tables=tuple(_dash_linetype(dash) for dash in dashes),
        blocks=tuple(BlockDef(name=name, entities=_cap_block(EndCap(name.removeprefix(_CAP_PREFIX)))) for name in used),
        entities=entities,
        fmt=spec.fmt,
    )


def _bridged(spec: BridgeSpec, /) -> Result[DxfComposed, RegionFault]:
    match spec:
        case BridgeSpec(tag="to_svg", to_svg=(source, sample)):
            doc = _ingest(source)
            base = _evidence(doc)
            fragments = tuple(Fragment(path=_polyline(vertices)) for vertices in _flattened(doc, sample) if len(vertices) >= 2)
            if not fragments:
                return Error(RegionFault(geometry=PathFault(empty=None)))
            return apply_region(
                RegionOp.Serialize(fragments, (base.extent[0], base.extent[1], base.extent[3], base.extent[4]))
            ).map(lambda outcome: msgspec.structs.replace(base, data=outcome.document, kind=DxfArtifact.SVG))
        case BridgeSpec(tag="from_svg", from_svg=(source, version, attribs, units)):
            doc = ezdxf.new(version.value, setup=True, units=units.value)
            match scene(source).bind(_svg_rings):
                case Result(tag="error", error=fault):
                    return Error(RegionFault(geometry=fault))
                case Result(tag="ok", ok=rings):
                    paths = [dxfpath.from_vertices(ring, close=closed) for ring, closed in rings if ring]
                    dxfpath.render_lines(doc.modelspace(), paths, dxfattribs=attribs.gfx())
                    return Ok(_dxf_composed(doc, doc.audit(), DxfFormat.ASC))
        case BridgeSpec(tag="to_geojson", to_geojson=source):
            doc = _ingest(source)
            proxy = dxfgeo.proxy(entity for entity in doc.modelspace() if entity.dxftype() in _GEO_TYPES)
            return Ok(msgspec.structs.replace(_evidence(doc), data=msgspec.json.encode(proxy.__geo_interface__), kind=DxfArtifact.GEOJSON))
        case BridgeSpec(tag="from_geojson", from_geojson=(mapping, version, attribs)):
            doc = ezdxf.new(version.value, setup=True)
            msp = doc.modelspace()
            for entity in dxfgeo.GeoProxy.parse(msgspec.json.decode(mapping)).to_dxf_entities(dxfattribs=attribs.gfx()):
                msp.add_entity(entity)
            return Ok(_dxf_composed(doc, doc.audit(), DxfFormat.ASC))
        case BridgeSpec(tag="text_paths", text_paths=(text, font, size, insert, version, attribs)):
            doc = ezdxf.new(version.value, setup=True)
            paths = text2path.make_paths_from_str(text, FontFace(family=font), size=size, m=Matrix44.translate(*insert))
            dxfpath.render_lines(doc.modelspace(), paths, dxfattribs=attribs.gfx())
            return Ok(_dxf_composed(doc, doc.audit(), DxfFormat.ASC))
        case _ as unreachable:
            assert_never(unreachable)


def _composed(op: DxfOp) -> Result[DxfComposed, RegionFault]:
    match op:
        case DxfOp(tag="new", new=document):
            doc, auditor = _authored(document)
            return Ok(_dxf_composed(doc, auditor, document.fmt))
        case DxfOp(tag="read", read=source):
            doc = _ingest(source)
            return Ok(_dxf_composed(doc, doc.audit(), DxfFormat.ASC))
        case DxfOp(tag="recover", recover=source):
            doc, auditor = _recovered(source)
            return Ok(_dxf_composed(doc, auditor, DxfFormat.ASC))
        case DxfOp(tag="render", render=(source, backend, page)):
            return Ok(_rendered(source, backend, page))
        case DxfOp(tag="query", query=(source, selection)):
            return Ok(_queried(source, selection))
        case DxfOp(tag="transform", transform=(source, spec)):
            return Ok(_transformed(source, spec))
        case DxfOp(tag="diagram", diagram=spec):
            return _diagram_entities(spec).map(lambda entities: _dxf_composed(*_authored(_diagram_document(spec, entities)), spec.fmt))
        case DxfOp(tag="bridge", bridge=spec):
            return _bridged(spec)
        case _ as unreachable:
            assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------
class Dxf(Struct, frozen=True):
    op: DxfOp
    lane: LanePolicy

    @classmethod
    def of(cls, subject: "DxfOp | DxfDocument | DxfSource | DiagramLower | BridgeSpec", /, *, lane: LanePolicy) -> Result[Self, DxfFault]:
        match subject:
            case DxfDocument() as document:
                op = DxfOp(new=document)
            case DxfSource() as source:
                op = DxfOp(read=source)
            case DiagramLower() as lowering:
                op = DxfOp(diagram=lowering)
            case BridgeSpec() as bridge:
                op = DxfOp(bridge=bridge)
            case DxfOp() as full:
                op = full
            case _ as unreachable:
                assert_never(unreachable)
        return _checked(op).map(lambda valid: cls(op=valid, lane=lane))

    def emit(self, /) -> ArtifactWork[DxfComposed]:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"dxf-{self.op.tag}", _CANON.encode(self.op))

    async def _emit(self) -> RuntimeResult[DxfComposed]:
        crossed = await self.lane.offload(Kernel.of(_composed, KernelTrait.RELEASING), self.op)
        settled = crossed.bind(lambda inner: inner.map_error(lambda fault: BoundaryFault(domain=(DXF_REGION.subject, fault))))
        match settled:
            case Result(tag="ok", ok=composed):
                Metrics.record({BYTE_VOLUME: float(len(composed.data))}, domain=DOMAIN, kind="cad", scope=self.lane.scope)
                return Ok(composed)
            case refused:
                return Error(refused.error)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "BackgroundPolicy",
    "BlockDef",
    "BridgeSample",
    "BridgeSpec",
    "ColorPolicy",
    "DiagramLower",
    "DimKind",
    "Dxf",
    "DxfArtifact",
    "DxfAttribs",
    "DxfBackend",
    "DxfDocument",
    "DxfEntity",
    "DxfFault",
    "DxfFill",
    "DxfFormat",
    "DxfOp",
    "DxfSource",
    "DxfUnits",
    "DxfVersion",
    "HatchPolicy",
    "LeaderSide",
    "LinePolicy",
    "LineweightPolicy",
    "MLeaderKind",
    "PageSpec",
    "ProxyPolicy",
    "DxfRenderPolicy",
    "Selection",
    "Spatial",
    "SpatialTest",
    "TableEntry",
    "TextAlign",
    "TextPolicy",
    "TransformMode",
    "TransformSpec",
    "Xref",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
