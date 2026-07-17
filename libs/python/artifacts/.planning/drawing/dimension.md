# [PY_ARTIFACTS_DRAWING_DIMENSION]

`Dimension` owns ISO 129-1 and ISO 1101 production through one closed `DimOp` union. ISO 129-1 cases carry their construction geometry, `DimStyleFamily`, and `DimTol`; GD&T cases carry `GdtFrame` or datum-feature data. Native cases lower through verified `ezdxf` builders with `Standard.dimstyle(family)` overrides, while `Fcf` and `DatumFeature` lower through `TOLERANCE` entities. `GdtFrame` admits the `csharp:Rasm.Fabrication -> Drawing [SHAPE]: Tolerance` wire without a parallel tolerance owner.

Each dimension DUAL-lowers over the `DimTarget` policy value. `ezdxf`'s native path (`DXF` the `Drawing.write` blob, `SVG` the `SVGBackend`, `PDF` the `PyMuPdfBackend`) LEADS with `add_*_dim().render()` and the ISO tolerance as native DIM-variables (`dimtol`/`dimtp`/`dimtm`, `dimlim`, `MTextEditor.stack` for the stacked deviation), while the `LAYERED` path DECOMPOSES each dimension into named editable `graphic/layer#LAYER` `LayerNode` rows at full semantic parity — the extension/dimension-line geometry from `ezdxf.math.Construction*` anchor math (never hand-rolled trig) authored as `drawsvg` elements (never hand-formatted `<path d>` strings), per-case ISO 129-1 terminators anchored where the case actually terminates (arc ends tangent to the measured arc, one leader arrow on a radial, a datum triangle on a datum feature), the true measured value for EVERY case including the angular degrees and the `⌢`-prefixed arc length, the ISO 3098 measurement text outlined through `ziafont` (`typography/shape#SHAPE` owns the shaped run), and every `DimTol` mode typeset through the `typography/math#MATH` `Formula` owner seated through `seat` — `\pm` symmetric, stacked deviation, stacked limits, boxed basic — all penned by the discipline sRGB `Standard.rgb` resolves. `kiwisolver` `Solver` + `strength` bands solve the dimension-line offset STACK a fixed offset gets wrong. Rendering offloads through the owner's `lane: LanePolicy` instance seam onto the runtime thread lane, and the owner contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Drawing` case (or reused `ArtifactReceipt.Pdf` on the `PDF` backend) and one `core/plan#PLAN` `ArtifactWork` node — minting no IFC (`csharp:Rasm.Bim`) and computing no sheet placement, the dimensioned SVG/PDF bytes feeding `composition/sheet#SHEET`'s `FigurePlacement` as a bytes seam.

## [01]-[INDEX]

- [01]-[DIMENSION]: the `Dimension` owner over the closed `DimOp` union (`Linear`/…/`Baseline`/`Fcf`/`DatumFeature`), dual-lowering over `DimTarget` into the `ezdxf`-native render (DXF/SVG/PDF) or the `LAYERED` decomposition of named `graphic/layer#LAYER` rows.

## [02]-[DIMENSION]

- Owner: `Dimension` holds `ops`, the resolved `drawing/standard#STANDARD` `Standard`, the `DimTarget` value, and the `lane: LanePolicy` execution policy, discriminating over the closed `DimOp` whose every case carries ONLY its own geometry plus the `DimStyleFamily`/`DimTol` facet slots — never a per-dimension `LinearDim`/`RadialDim` class family, never a monolithic bag whose angular/radial fields are dead for most cases. `DimTol` is the closed dimensional-tolerance vocabulary (`Auto`/`Custom`/`Symmetric`/`Deviation`/`Limits`/`Basic`) every case's second facet carries, and `GdtFrame` — `GdtChar` characteristic, zone value with the `⌀` cylindrical flag, `GdtModifier` material condition, datum letters — is the geometric-tolerance sibling the `Fcf` case carries whole; together they are where the `Fabrication -> Drawing` tolerance wire admits. `DimTarget` keys the `_ENGINES` dual-lowering table (`DXF`/`SVG`/`PDF` sharing the `_native` arm through `_BACKENDS`, `LAYERED` the `_layered` arm), so a new egress is one row.
- Cases: each dimensional `DimOp` case ends in the `(DimStyleFamily, DimTol)` facet pair and lowers onto its verified `ezdxf` builder (`add_linear_dim`/`add_aligned_dim`/`add_angular_dim_2l`/`_3p`/`_cra`/`add_radius_dim`/`add_diameter_dim`/`add_ordinate_x_dim`/`_y_dim`/`add_arc_dim_3p`/`_cra`/the self-rendering `add_multi_point_linear_dim` chain, `Baseline` a fold of `add_linear_dim` stepping by DIMDLI), matched by one total `match` in `_lower` — the `Angular3P`/`Arc3P` and `AngularCRA`/`ArcCRA` payload shapes coincide but each lowers onto a distinct builder. `OrdinateAxis` routes `add_ordinate_x_dim`/`_y_dim`. `Fcf` and `DatumFeature` lower onto `add_leader` plus the `TOLERANCE` entity whose `content` `_gdt_content` derives from the frame value.
- Entry: `Dimension.over` normalizes `DimOp | Iterable[DimOp]` by a structural `match` at the head — never a `batch` knob. `emit` returns `ArtifactWork` beside the `layered()` `RuntimeRail[LayerPlan]` projection; both execution paths ride `self.lane.offload(Kernel.of(..., KernelTrait.RELEASING), ...)`, whose returned rail composes directly. `_native` seeds `Standard`, solves the offset stack, lowers every `DimOp`, and egresses through the target `DimBackend`; `_layered` decomposes each dimension into named rows over `ezdxf.math`, `drawsvg`, `ziafont`, and the `typography/math#MATH` `Formula` owner, with `aec=Some(_DIMS)` deriving ISO 13567 names downstream.
- Auto: `_facets` projects each case's `(family, tol)` once through one total or-pattern; `over = dict(standard.dimstyle(family)) | _tol_over(tol)` scales the DIM-variables by the ISO 5455 factor so a `1:50` dimension draws its 2.5 mm text at paper scale with zero per-arm literal. Each `DimTol` mode lowers onto its native mechanism — `Symmetric` onto `dimtol`, `Deviation` onto an `MTextEditor.stack` stacked fraction, `Limits` onto `dimlim`, `Basic` onto a negative-`dimgap` boxed value — never a hand-formatted `± ` string, and the `LAYERED` tolerance layer typesets ALL FOUR through `_tol_latex`, so no admitted tolerance is silently absent on either arm. `_stack` threads one `kiwisolver.Solver`: a `required` anchor, `required` min-separation ≥ DIMDLI, and a custom `strength.create(0,1,0,4)` equal-gap band above plain `weak`; `Constraint.violated()` reads which soft gaps the solve sacrificed, and a dense chain that collapses the distribution falls back to deterministic fixed DIMDLI stepping. `LAYERED`'s `_construction` DECOMPOSES a curved dimension to the actual `ConstructionArc`/`ConstructionCircle` it MEASURES (`.flattening(_SAGITTA)`) and an ordinate to its axis dogleg; `_measurement` reads the true value per case — length, `R`/`⌀`, the angular degrees off the construction geometry, the `⌢` arc length — and `_terminator_anchors` places each mark where its case terminates with the tangent the mark aligns to. Text and tolerance runs are MEASURED via `getsize()`/`getyofst()` so the text floats above the dimension line and the tolerance clears the value, the annotation font falls to the bundled `ziafont` face when the profile names an `.shx` CAD font no sfnt reader parses, and `_PRECISION` pins the emitted `d`-floats so the content key stays deterministic.
- Growth: a new ISO 129-1 dimension kind or construction form is one `DimOp` case plus one `_lower` builder arm; a new ISO 1101 characteristic is one `GdtChar` member plus one `_GDT_GLYPH` row; a new egress is one `DimTarget` member plus one `_ENGINES` row (and one `_BACKENDS` row for a native backend); a new tolerance presentation is one `DimTol` case plus one `_tol_over`/`_tol_latex` arm; a new DIM-variable axis is one key on the `drawing/standard#STANDARD` `dimstyle` derivation; a new stacking rule is one `kiwisolver` constraint at its `strength` band; a new `LAYERED` component author is one layer function over the existing owners. Zero new surface for a new dimension or a new layer.
- Boundary: no IFC, sheet-placement, or annotation-leader logic — `csharp:Rasm.Bim`, `composition/sheet#SHEET`, `drawing/annotate#ANNOTATE`. `ezdxf` owns the ISO 129-1 dimension entity, the `TOLERANCE` entity, and the render; `drawing/standard#STANDARD` the DIM-variable derivation and discipline pen; `graphic/vector/region#REGION` the landed `outline`/`boolean` the tapered-terminator premium composes; `ziafont` the ISO 3098 text outline; `typography/math#MATH` the tolerance math; `kiwisolver` the offset solve; `graphic/layer#LAYER` the layer vocabulary; `composition/sheet#SHEET` the placement; identity minting is the runtime's.
- Packages: `ezdxf` the ISO 129-1 dimension family (`add_*_dim` each returning a `DimStyleOverride` whose `.render()` authors geometry, the `new_entity("TOLERANCE", ...)` GD&T frame, the `math.Construction*`/`.flattening` anchor + measured-arc geometry, `MTextEditor.stack`, the `Frontend`/`SVGBackend`/`PyMuPdfBackend` render, `bbox.extents`); `drawsvg` the LAYERED geometry authoring (`Drawing`/`Group`/`Lines`/`Circle` — no hand-formatted `d` string); `kiwisolver` the offset stack with the custom equal-gap band and `Constraint.violated` overlap QA; `typography/math#MATH` `Formula`/`LatexSpec` the tolerance-math typeset, seat-placed, measured; `ziafont` the ISO 3098 text outline, measured, centred, baseline-seated; `numpy` the perpendicular offset normal and the angular/arc measurement; `expression`/`msgspec`/`beartype` the vocabulary, value objects, and `over` contract; `graphic/vector/region#REGION` a bare owner pointer — the landed `outline` the tapered-terminator premium composes, NOT imported for the base where the self-contained marks are the default; `core/receipt#RECEIPT` `ArtifactReceipt.Drawing`/`Pdf`. `drawing/standard#STANDARD` composes as bare owner pointers, its `DimStyleFamily`/`Standard.dimstyle`/`seed`/`LayerName` lowering onto the `ezdxf` tables.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Callable, Iterable
from enum import StrEnum
from itertools import accumulate, pairwise
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never
from xml.etree.ElementTree import Element, fromstring, tostring

import numpy as np
from beartype import beartype
from builtins import frozendict
from expression import Nothing, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.msgpack import Encoder

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.drawing.regime import Discipline, LayerName, LayerSchema
from rasm.artifacts.drawing.standard import DimStyleFamily, Standard
from rasm.artifacts.graphic.layer import LayerContent, LayerIntent, LayerMeta, LayerNode, LayerPlan
from rasm.artifacts.typography.math import Formula, FormulaSpec, LatexSpec, seat

# each proxy reifies on first render-arm use in the offloaded worker
lazy import drawsvg
lazy import ezdxf
lazy import kiwisolver
lazy import ziafont
lazy from ezdxf import bbox
lazy from ezdxf import math as ezmath
lazy from ezdxf.addons.drawing import Frontend, RenderContext
lazy from ezdxf.addons.drawing import layout as dxflayout
lazy from ezdxf.addons.drawing.pymupdf import PyMuPdfBackend
lazy from ezdxf.addons.drawing.svg import SVGBackend
lazy from ezdxf.tools.text import MTextEditor

if TYPE_CHECKING:
    from ezdxf.document import Drawing
    from ezdxf.layouts import Modelspace

# --- [TYPES] ----------------------------------------------------------------------------
type Point = tuple[float, float]
type Segment = tuple[Point, Point]
type PointRun = tuple[Point, Point, *tuple[Point, ...]]
type PointSet = tuple[Point, *tuple[Point, ...]]
type Box = tuple[float, float, float, float]
type Override = dict[str, object]
type Fragment = "drawsvg.DrawingElement"  # one authored LAYERED geometry element
type DimTag = Literal[
    "linear", "aligned", "angular2l", "angular3p", "angularcra", "radius", "diameter", "ordinate",
    "arc3p", "arccra", "chain", "baseline", "fcf", "datum_feature",
]
type TolTag = Literal["auto", "custom", "symmetric", "deviation", "limits", "basic"]
type MarkKind = Literal["arrow", "open", "oblique", "dot", "origin", "none", "datum"]
type DimArm = Callable[["Dimension"], tuple[tuple[LayerNode, ...], ArtifactReceipt]]  # the target-keyed lowering arm


class DimTarget(StrEnum):  # the dual-lowering egress — a new target is one `_ENGINES` row, never a subtype
    DXF = "dxf"  # ezdxf Drawing.write CAD blob
    SVG = "svg"  # SVGBackend.get_string native render
    PDF = "pdf"  # PyMuPdfBackend.get_pdf_bytes native render
    LAYERED = "layered"  # ezdxf.math + drawsvg + ziafont + typography/math Formula named graphic/layer rows


class OrdinateAxis(StrEnum):  # the ISO 129-1 ordinate measurement axis routing add_ordinate_{x,y}_dim
    X = "x"
    Y = "y"


class GdtChar(StrEnum):  # ISO 1101 geometric characteristics — value is the DXF gdt-font code the TOLERANCE content embeds
    STRAIGHTNESS = "u"
    FLATNESS = "c"
    CIRCULARITY = "e"
    CYLINDRICITY = "g"
    PROFILE_LINE = "k"
    PROFILE_SURFACE = "d"
    ANGULARITY = "a"
    PERPENDICULARITY = "b"
    PARALLELISM = "f"
    POSITION = "j"
    CONCENTRICITY = "r"
    SYMMETRY = "i"
    CIRCULAR_RUNOUT = "h"
    TOTAL_RUNOUT = "t"

    @property
    def glyph(self) -> str:  # the unicode twin the LAYERED text layer outlines
        return _GDT_GLYPH[self]


class GdtModifier(StrEnum):  # ISO 2692 material-condition modifiers — DXF gdt-font codes
    NONE = ""
    MMC = "m"
    LMC = "l"
    RFS = "s"


# --- [CONSTANTS] ------------------------------------------------------------------------
_PRECISION: Final[int] = 3  # ziafont emitted-d-float places — the content-key determinism lever set once per offloaded arm
_SAGITTA: Final[float] = 0.05  # LAYERED arc/circle flattening tolerance (mm) — ezdxf.math adaptive `.flattening` chord height
_ARC_SPAN: Final[float] = 30.0  # reference-arc half-span (deg) a radial dimension draws around its leader angle
_DIMS: Final[LayerName] = LayerName.of(Discipline.GENERAL, "DIMS")  # the one ISO 13567 layer every dimension row derives from
_DATUM_LETTERS: Final[frozenset[str]] = frozenset("ABCDEFGHJKLMNPRSTUVWXYZ")  # ISO 5459 reference letters — I, O, Q excluded as digit-confusable


def _canonized(raw: object) -> object:
    # msgpack enc_hook: an Option field canonicalizes as its projected list — present `[value]`, absent `[]`.
    match raw:
        case Option():
            return raw.to_list()
        case _:
            raise NotImplementedError(type(raw).__name__)


_CANON: Final[Encoder] = Encoder(order="deterministic", enc_hook=_canonized)  # stable preimage encoding the bare `ContentIdentity.key` mint addresses


# --- [MODELS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class DimTol:
    # ISO 129-1 dimensional-tolerance family every DimOp facet carries; lowered by mode to native dimtol/dimlim/MTextEditor, never a hand-formatted `± value` string.
    tag: TolTag = tag()
    auto: None = case()  # the raw `<>` measurement
    custom: str = case()  # an explicit override string
    symmetric: float = case()  # a ± symmetric band -> dimtol
    deviation: tuple[float, float] = case()  # upper/lower -> MTextEditor.stack
    limits: tuple[float, float] = case()  # stacked max/min -> dimlim
    basic: float = case()  # a boxed theoretically-exact value -> negative dimgap


def _admit_datums(datums: tuple[str, ...], /) -> None:
    # datum-chain admission both GD&T constructors share: at most primary/secondary/tertiary references, each one
    # single ISO 5459 letter, so a malformed chain rails at mint, never inside a lowering arm.
    if len(datums) > 3 or any(letter not in _DATUM_LETTERS for letter in datums):
        raise ValueError(f"datum chain is 0-3 single ISO 5459 letters: {datums!r}")


class GdtFrame(Struct, frozen=True):
    # one ISO 1101 feature-control frame — the admission target of the Fabrication -> Drawing Tolerance wire.
    characteristic: GdtChar
    tolerance: str  # zone value, e.g. "0.1"
    diameter: bool = False  # ⌀ cylindrical-zone prefix
    modifier: GdtModifier = GdtModifier.NONE
    datums: tuple[str, ...] = ()  # primary/secondary/tertiary datum letters

    def __post_init__(self) -> None:
        # ISO 1101 admission at mint: the zone value is a parseable positive decimal magnitude and the datum chain
        # admits through the shared validator — the ⌀ prefix and material modifier ride their own typed fields.
        zone = self.tolerance
        if not zone or zone.count(".") > 1 or not zone.replace(".", "", 1).isdigit() or float(zone) <= 0.0:
            raise ValueError(f"gdt zone value is a positive decimal: {zone!r}")
        _admit_datums(self.datums)


class DimBackend(Struct, frozen=True):
    # Native ezdxf egress: the byte emitter over the lowered Drawing plus its receipt-shape discriminant.
    egress: "Callable[[Drawing, Modelspace, float, float], bytes]"
    kind: Literal["drawing", "pdf"]


# --- [VOCABULARY] -----------------------------------------------------------------------
@tagged_union(frozen=True)
class DimOp:
    tag: DimTag = tag()
    linear: tuple[Point, Point, Point, float, DimStyleFamily, DimTol] = case()
    aligned: tuple[Point, Point, float, DimStyleFamily, DimTol] = case()
    angular2l: tuple[Point, Segment, Segment, DimStyleFamily, DimTol] = case()
    angular3p: tuple[Point, Point, Point, Point, DimStyleFamily, DimTol] = case()
    angularcra: tuple[Point, float, float, float, float, DimStyleFamily, DimTol] = case()
    radius: tuple[Point, float, float, DimStyleFamily, DimTol] = case()
    diameter: tuple[Point, float, float, DimStyleFamily, DimTol] = case()
    ordinate: tuple[Point, Point, OrdinateAxis, Point, DimStyleFamily, DimTol] = case()
    arc3p: tuple[Point, Point, Point, Point, DimStyleFamily, DimTol] = case()
    arccra: tuple[Point, float, float, float, float, DimStyleFamily, DimTol] = case()
    chain: tuple[Point, PointRun, float, DimStyleFamily, DimTol] = case()
    baseline: tuple[Point, Point, PointSet, float, DimStyleFamily, DimTol] = case()
    fcf: tuple[Point, Point, GdtFrame, DimStyleFamily, DimTol] = case()
    datum_feature: tuple[Point, Point, str, DimStyleFamily, DimTol] = case()

    @staticmethod
    def Linear(base: Point, p1: Point, p2: Point, family: DimStyleFamily, *, angle: float = 0.0, tol: DimTol = DimTol(auto=None)) -> "DimOp":
        return DimOp(linear=(base, p1, p2, angle, family, tol))

    @staticmethod
    def Aligned(p1: Point, p2: Point, distance: float, family: DimStyleFamily, *, tol: DimTol = DimTol(auto=None)) -> "DimOp":
        return DimOp(aligned=(p1, p2, distance, family, tol))

    @staticmethod
    def Angular2L(base: Point, line1: Segment, line2: Segment, family: DimStyleFamily, *, tol: DimTol = DimTol(auto=None)) -> "DimOp":
        # parallel construction lines have no vertex to measure from — refused at admission through the page's
        # ValueError family, so no downstream arm ever fabricates an apex for a measurement that does not exist.
        d1 = (line1[1][0] - line1[0][0], line1[1][1] - line1[0][1])
        d2 = (line2[1][0] - line2[0][0], line2[1][1] - line2[0][1])
        if abs(d1[0] * d2[1] - d1[1] * d2[0]) <= 1e-9 * float(np.hypot(*d1) * np.hypot(*d2)):
            raise ValueError(f"angular2l lines are parallel: {line1!r} {line2!r}")
        return DimOp(angular2l=(base, line1, line2, family, tol))

    @staticmethod
    def Angular3P(base: Point, center: Point, p1: Point, p2: Point, family: DimStyleFamily, *, tol: DimTol = DimTol(auto=None)) -> "DimOp":
        return DimOp(angular3p=(base, center, p1, p2, family, tol))

    @staticmethod
    def AngularCRA(
        center: Point, radius: float, start: float, end: float, distance: float, family: DimStyleFamily, *, tol: DimTol = DimTol(auto=None)
    ) -> "DimOp":
        return DimOp(angularcra=(center, radius, start, end, distance, family, tol))

    @staticmethod
    def Radius(center: Point, radius: float, family: DimStyleFamily, *, angle: float = 45.0, tol: DimTol = DimTol(auto=None)) -> "DimOp":
        return DimOp(radius=(center, radius, angle, family, tol))

    @staticmethod
    def Diameter(center: Point, radius: float, family: DimStyleFamily, *, angle: float = 45.0, tol: DimTol = DimTol(auto=None)) -> "DimOp":
        return DimOp(diameter=(center, radius, angle, family, tol))

    @staticmethod
    def Ordinate(
        feature: Point,
        offset: Point,
        family: DimStyleFamily,
        *,
        axis: OrdinateAxis = OrdinateAxis.X,
        origin: Point = (0.0, 0.0),
        tol: DimTol = DimTol(auto=None),
    ) -> "DimOp":
        return DimOp(ordinate=(feature, offset, axis, origin, family, tol))

    @staticmethod
    def Arc3P(base: Point, center: Point, p1: Point, p2: Point, family: DimStyleFamily, *, tol: DimTol = DimTol(auto=None)) -> "DimOp":
        return DimOp(arc3p=(base, center, p1, p2, family, tol))

    @staticmethod
    def ArcCRA(
        center: Point, radius: float, start: float, end: float, distance: float, family: DimStyleFamily, *, tol: DimTol = DimTol(auto=None)
    ) -> "DimOp":
        return DimOp(arccra=(center, radius, start, end, distance, family, tol))

    @staticmethod
    def Chain(base: Point, points: PointRun, family: DimStyleFamily, *, angle: float = 0.0, tol: DimTol = DimTol(auto=None)) -> "DimOp":
        return DimOp(chain=(base, points, angle, family, tol))

    @staticmethod
    def Baseline(
        base: Point, datum: Point, points: PointSet, family: DimStyleFamily, *, angle: float = 0.0, tol: DimTol = DimTol(auto=None)
    ) -> "DimOp":
        return DimOp(baseline=(base, datum, points, angle, family, tol))

    @staticmethod
    def Fcf(anchor: Point, insert: Point, frame: GdtFrame, family: DimStyleFamily) -> "DimOp":
        # a GD&T op carries its zone tolerance inside `frame.tolerance` and the native TOLERANCE entity spells no
        # dimensional DimTol, so neither GD&T constructor admits one — the silent-discard combination is unspellable.
        return DimOp(fcf=(anchor, insert, frame, family, DimTol(auto=None)))

    @staticmethod
    def DatumFeature(anchor: Point, insert: Point, letter: str, family: DimStyleFamily) -> "DimOp":
        _admit_datums((letter,))  # exactly one ISO 5459 reference letter labels a datum-feature symbol
        return DimOp(datum_feature=(anchor, insert, letter, family, DimTol(auto=None)))


# --- [SERVICES] -------------------------------------------------------------------------
class Dimension(Struct, frozen=True):
    ops: tuple[DimOp, ...]
    standard: Standard
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    lane: LanePolicy
    target: DimTarget = DimTarget.SVG

    @classmethod
    @beartype
    def over(
        cls, ops: DimOp | Iterable[DimOp], standard: Standard, /, *, lane: LanePolicy, target: DimTarget = DimTarget.SVG
    ) -> Self:
        match ops:  # the one modal-arity head — a lone dimension is the singleton, an iterable the multi-dim set
            case DimOp():
                return cls(ops=(ops,), standard=standard, target=target, lane=lane)
            case _:
                return cls(ops=tuple(ops), standard=standard, target=target, lane=lane)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.ops)))

    @property
    def _key(self) -> ContentKey:
        # key over the frozen INPUT spec, minted pre-run through the bare mint (`.of` is the railed form and never
        # keys a plan) — never over rendered layer bytes; the lane is execution policy, outside the preimage.
        return ContentIdentity.key(f"drawing-dimension-{self.target}", _CANON.encode((self.ops, self.standard, self.target)))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # offload rails the synchronous fold itself; the returned rail composes — never re-raised for a second boundary.
        return (await self.lane.offload(Kernel.of(_ENGINES[self.target], KernelTrait.RELEASING), self)).map(lambda pair: pair[1])

    async def layered(self) -> RuntimeRail[LayerPlan]:
        # Engine rows as one LayerPlan tree — substrate data the layered/sheet consumers compose, not the producer rail.
        return (await self.lane.offload(Kernel.of(_layered, KernelTrait.RELEASING), self)).map(
            lambda pair: LayerPlan(schema=LayerSchema.ISO13567, roots=pair[0])
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
def _row(name: str, source: bytes, z: int = 0, /) -> LayerNode:
    # one engine row into the layer vocabulary — a Leaf over LayerMeta whose aec carries the DIMS LayerName so ISO 13567 naming derives downstream.
    return LayerNode.Leaf(LayerMeta(name=name, intent=LayerIntent.ANNOTATION, z=z, aec=Some(_DIMS)), LayerContent.Fragment(source))


def _facets(op: DimOp, /) -> tuple[DimStyleFamily, DimTol]:
    match op:  # every case's (family, tol) is its last two payload slots; one total projection
        case (
            DimOp(tag="linear", linear=(*_, family, tol))
            | DimOp(tag="aligned", aligned=(*_, family, tol))
            | DimOp(tag="angular2l", angular2l=(*_, family, tol))
            | DimOp(tag="angular3p", angular3p=(*_, family, tol))
            | DimOp(tag="angularcra", angularcra=(*_, family, tol))
            | DimOp(tag="radius", radius=(*_, family, tol))
            | DimOp(tag="diameter", diameter=(*_, family, tol))
            | DimOp(tag="ordinate", ordinate=(*_, family, tol))
            | DimOp(tag="arc3p", arc3p=(*_, family, tol))
            | DimOp(tag="arccra", arccra=(*_, family, tol))
            | DimOp(tag="chain", chain=(*_, family, tol))
            | DimOp(tag="baseline", baseline=(*_, family, tol))
            | DimOp(tag="fcf", fcf=(*_, family, tol))
            | DimOp(tag="datum_feature", datum_feature=(*_, family, tol))
        ):
            return family, tol
        case _ as unreachable:
            assert_never(unreachable)


def _tol_over(tol: DimTol, /) -> Override:
    match tol:  # the ISO 129-1 tolerance -> native DIM-variable dict; deviation renders via stacked text
        case DimTol(tag="symmetric", symmetric=pm):
            return {"dimtol": 1, "dimtp": pm, "dimtm": pm}
        case DimTol(tag="limits", limits=(upper, lower)):
            return {"dimlim": 1, "dimtp": upper, "dimtm": lower}
        case DimTol(tag="basic"):
            return {"dimgap": -0.9}  # negative DIMGAP boxes the theoretically-exact value
        case DimTol(tag="auto") | DimTol(tag="custom") | DimTol(tag="deviation"):
            return {}
        case _ as unreachable:
            assert_never(unreachable)


def _dim_text(tol: DimTol, /) -> str:
    match tol:  # the native measurement text; deviation stacks the upper/lower band onto the `<>` measurement
        case DimTol(tag="custom", custom=text):
            return text
        case DimTol(tag="basic", basic=value):
            return f"{value:g}"
        case DimTol(tag="deviation", deviation=(upper, lower)):
            return f"<>{MTextEditor().stack(f'+{upper:g}', f'{lower:g}', '^').text}"
        case DimTol(tag="auto") | DimTol(tag="symmetric") | DimTol(tag="limits"):
            return "<>"
        case _ as unreachable:
            assert_never(unreachable)


def _gdt_content(frame: GdtFrame, /) -> str:
    # TOLERANCE entity content grammar: {\Fgdt;<char>} symbol cell, %%v compartment separators, ⌀/modifier as gdt codes.
    zone = (
        ("{\\Fgdt;n}" if frame.diameter else "")
        + frame.tolerance
        + (f"{{\\Fgdt;{frame.modifier.value}}}" if frame.modifier is not GdtModifier.NONE else "")
    )
    return "%%v".join((f"{{\\Fgdt;{frame.characteristic.value}}}", zone, *frame.datums))


def _shifted(base: Point, p1: Point, p2: Point, offset: Option[float], /) -> Point:
    # shift the dimension line perpendicular to the measured direction by the solved stack offset.
    direction = np.asarray(p2) - np.asarray(p1)
    normal = np.array([-direction[1], direction[0]])
    unit = normal / (float(np.hypot(*normal)) or 1.0)
    shifted = np.asarray(base) + unit * offset.default_value(0.0)
    return (float(shifted[0]), float(shifted[1]))


def _stack(dim: Dimension, /) -> Map[int, float]:
    # one kiwisolver.Solver over the stackable (linear/aligned/baseline) offsets: a required anchor, a required DIMDLI
    # min-separation, and a custom strength.create equal-gap band above plain weak; a dense chain that collapses the
    # distribution falls back to fixed DIMDLI stepping.
    lanes = tuple(index for index, op in enumerate(dim.ops) if op.tag in ("linear", "aligned", "baseline"))
    if len(lanes) < 2:
        return Map.empty()
    family, _ = _facets(dim.ops[lanes[0]])
    step = float(dim.standard.dimstyle(family).get("dimdli", 8.0))
    solver = kiwisolver.Solver()
    offsets = {index: kiwisolver.Variable(f"d{index}") for index in lanes}
    gap = kiwisolver.strength.create(0.0, 1.0, 0.0, 4.0)
    even = tuple((offsets[upper] - offsets[lower] == step) | gap for lower, upper in pairwise(lanes))
    solver.addConstraint(offsets[lanes[0]] == step)
    for (lower, upper), soft in zip(pairwise(lanes), even, strict=True):  # Exemption: kiwisolver.Solver is the stateful native sink
        solver.addConstraint(offsets[upper] - offsets[lower] >= step)
        solver.addConstraint(soft)
    solver.updateVariables()
    if sum(constraint.violated() for constraint in even) * 2 > len(even):
        return Map.of_seq((index, step * (rank + 1)) for rank, index in enumerate(lanes))
    return Map.of_seq((index, offsets[index].value()) for index in lanes)


def _lower(msp: "Modelspace", op: DimOp, standard: Standard, offset: Option[float], /) -> None:
    # one total dispatch onto the verified ezdxf ISO 129-1 builder family; every builder but the
    # self-rendering add_multi_point_linear_dim returns a DimStyleOverride whose .render() authors geometry.
    family, tol = _facets(op)
    over = dict(standard.dimstyle(family)) | _tol_over(tol)
    text, style = _dim_text(tol), family.value
    match op:  # Exemption: ezdxf Modelspace is the GraphicsFactory sink; add_* + .render() mutate the layout in place
        case DimOp(tag="linear", linear=(base, p1, p2, angle, _family, _tol)):
            msp.add_linear_dim(base=_shifted(base, p1, p2, offset), p1=p1, p2=p2, angle=angle, text=text, dimstyle=style, override=over).render()
        case DimOp(tag="aligned", aligned=(p1, p2, distance, _family, _tol)):
            msp.add_aligned_dim(p1=p1, p2=p2, distance=offset.default_value(distance), text=text, dimstyle=style, override=over).render()
        case DimOp(tag="angular2l", angular2l=(base, line1, line2, _family, _tol)):
            msp.add_angular_dim_2l(base=base, line1=line1, line2=line2, text=text, dimstyle=style, override=over).render()
        case DimOp(tag="angular3p", angular3p=(base, center, p1, p2, _family, _tol)):
            msp.add_angular_dim_3p(base=base, center=center, p1=p1, p2=p2, text=text, dimstyle=style, override=over).render()
        case DimOp(tag="angularcra", angularcra=(center, radius, start, end, distance, _family, _tol)):
            msp.add_angular_dim_cra(
                center=center, radius=radius, start_angle=start, end_angle=end, distance=distance, text=text, dimstyle=style, override=over
            ).render()
        case DimOp(tag="radius", radius=(center, radius, angle, _family, _tol)):
            msp.add_radius_dim(center=center, radius=radius, angle=angle, text=text, dimstyle=style, override=over).render()
        case DimOp(tag="diameter", diameter=(center, radius, angle, _family, _tol)):
            msp.add_diameter_dim(center=center, radius=radius, angle=angle, text=text, dimstyle=style, override=over).render()
        case DimOp(tag="ordinate", ordinate=(feature, feat_offset, axis, origin, _family, _tol)):
            (msp.add_ordinate_x_dim if axis is OrdinateAxis.X else msp.add_ordinate_y_dim)(
                feature_location=feature, offset=feat_offset, origin=origin, text=text, dimstyle=style, override=over
            ).render()
        case DimOp(tag="arc3p", arc3p=(base, center, p1, p2, _family, _tol)):
            msp.add_arc_dim_3p(base=base, center=center, p1=p1, p2=p2, text=text, dimstyle=style, override=over).render()
        case DimOp(tag="arccra", arccra=(center, radius, start, end, distance, _family, _tol)):
            msp.add_arc_dim_cra(
                center=center, radius=radius, start_angle=start, end_angle=end, distance=distance, text=text, dimstyle=style, override=over
            ).render()
        case DimOp(tag="chain", chain=(base, points, angle, _family, _tol)):
            msp.add_multi_point_linear_dim(base=base, points=list(points), angle=angle, dimstyle=style, override=over)
        case DimOp(tag="baseline", baseline=(base, datum, points, angle, _family, _tol)):
            step = float(over.get("dimdli", 8.0))
            for index, point in enumerate(points):  # Exemption: baseline has no single ezdxf builder; each line steps by DIMDLI from one datum
                msp.add_linear_dim(
                    base=_shifted(base, datum, point, Some(offset.default_value(step) + index * step)),
                    p1=datum,
                    p2=point,
                    angle=angle,
                    text=text,
                    dimstyle=style,
                    override=over,
                ).render()
        case DimOp(tag="fcf", fcf=(anchor, insert, frame, _family, _tol)):
            msp.add_leader([anchor, insert], dimstyle=style, override=over)
            msp.new_entity("TOLERANCE", dxfattribs={"content": _gdt_content(frame), "insert": insert, "dimstyle": style})
        case DimOp(tag="datum_feature", datum_feature=(anchor, insert, letter, _family, _tol)):
            msp.add_leader([anchor, insert], dimstyle=style, override={**over, "dimldrblk": "DATUMFILLED"})
            msp.new_entity("TOLERANCE", dxfattribs={"content": letter, "insert": insert, "dimstyle": style})
        case _ as unreachable:
            assert_never(unreachable)


def _lowered(dim: Dimension, /) -> tuple["Drawing", "Modelspace", int, str]:
    # seed the ISO resources, solve the offset stack once, fold every DimOp onto its builder — the shared native lowering the DXF/SVG/PDF engines egress differently.
    doc = ezdxf.new("R2018", setup=True)
    msp = doc.modelspace()
    families = tuple(dict.fromkeys(_facets(op)[0] for op in dim.ops))
    dim.standard.seed(doc, layers=(_DIMS,), families=families)
    offsets = _stack(dim)
    for index, op in enumerate(dim.ops):
        _lower(msp, op, dim.standard, offsets.try_find(index))
    return doc, msp, len(dim.ops), families[0].value if families else "ISO-129"


def _page(width: float, height: float, /) -> "dxflayout.Page":
    return dxflayout.Page(max(width, 1.0), max(height, 1.0), dxflayout.Units.mm, margins=dxflayout.Margins.all(0.0))


def _extent(msp: "Modelspace", /) -> tuple[float, float]:
    box = bbox.extents(msp, fast=True)
    return (float(box.size.x), float(box.size.y)) if box.has_data else (0.0, 0.0)


# --- [BOUNDARIES] -----------------------------------------------------------------------
def _endpoints(op: DimOp, /) -> Segment:
    match op:  # the measured start/finish each case's construction geometry and text anchor read
        case DimOp(tag="linear", linear=(_base, p1, p2, *_)):
            return p1, p2
        case DimOp(tag="aligned", aligned=(p1, p2, *_)):
            return p1, p2
        case DimOp(tag="ordinate", ordinate=(p1, p2, *_)):
            return p1, p2
        case DimOp(tag="chain", chain=(_base, points, *_)):
            return points[0], points[-1]
        case DimOp(tag="baseline", baseline=(_base, _datum, points, *_)):
            return points[0], points[-1]
        case DimOp(tag="radius", radius=(center, radius, *_)):
            return center, (center[0] + radius, center[1])
        case DimOp(tag="diameter", diameter=(center, radius, *_)):
            return center, (center[0] + radius, center[1])
        case DimOp(tag="angular3p", angular3p=(_base, _center, p1, p2, *_)):
            return p1, p2
        case DimOp(tag="arc3p", arc3p=(_base, _center, p1, p2, *_)):
            return p1, p2
        case DimOp(tag="angular2l", angular2l=(_base, line1, line2, *_)):
            return line1[1], line2[1]
        case DimOp(tag="angularcra", angularcra=(center, radius, start, end, *_)):
            return _polar(center, radius, start), _polar(center, radius, end)
        case DimOp(tag="arccra", arccra=(center, radius, start, end, *_)):
            return _polar(center, radius, start), _polar(center, radius, end)
        case DimOp(tag="fcf", fcf=(anchor, insert, *_)):
            return anchor, insert
        case DimOp(tag="datum_feature", datum_feature=(anchor, insert, *_)):
            return anchor, insert
        case _ as unreachable:
            assert_never(unreachable)


def _polar(center: Point, radius: float, angle: float, /) -> Point:
    rad = float(np.deg2rad(angle))
    return (center[0] + radius * float(np.cos(rad)), center[1] + radius * float(np.sin(rad)))


def _angle_of(center: Point, point: Point, /) -> float:
    # CCW degree angle from `center` to `point` — the inverse of `_polar`, feeding the ConstructionArc span
    return float(np.degrees(np.arctan2(point[1] - center[1], point[0] - center[0])))


def _arc_angles(center: Point, p1: Point, p2: Point, through: Point, /) -> tuple[float, float]:
    start, end, witness = _angle_of(center, p1), _angle_of(center, p2), _angle_of(center, through)
    return (start, end) if (witness - start) % 360.0 <= (end - start) % 360.0 else (end, start)


def _span(center: Point, p1: Point, p2: Point, through: Point, /) -> float:
    start, end = _arc_angles(center, p1, p2, through)
    return (end - start) % 360.0


def _unit(tail: Point, head: Point, /) -> Point:
    direction = np.asarray(head) - np.asarray(tail)
    scaled = direction / (float(np.hypot(*direction)) or 1.0)
    return (float(scaled[0]), float(scaled[1]))


def _tangent(center: Point, point: Point, /) -> Point:
    radial = _unit(center, point)
    return (-radial[1], radial[0])


def _arc_verts(construction: object, /) -> tuple[Point, ...]:
    # an ezdxf.math ConstructionArc/ConstructionCircle adaptively flattened to a `(x, y)` polyline — materialized once so the same vertices draw the fragment AND bound the envelope.
    return tuple((float(v.x), float(v.y)) for v in construction.flattening(_SAGITTA))


def _measured(op: DimOp, /) -> tuple[Segment, Point]:
    # Measured segment and its unit normal — the perpendicular the offset stack and witness geometry shift along.
    start, finish = _endpoints(op)
    direction = np.asarray(finish) - np.asarray(start)
    normal = np.array([-direction[1], direction[0]])
    unit = normal / (float(np.hypot(*normal)) or 1.0)
    return (start, finish), (float(unit[0]), float(unit[1]))


def _offset(point: Point, normal: Point, distance: float, /) -> Point:
    return (point[0] + normal[0] * distance, point[1] + normal[1] * distance)


def _chain_line(op: DimOp, shift: float, /) -> tuple[Point, Point, Point]:
    # chain dimension-line mirror of native `add_multi_point_linear_dim`: through `base` at the declared `angle`,
    # shifted by the `_stack`-solved offset along the angle normal — never the raw point run's own direction.
    base, _points, angle, *_ = op.chain
    direction = _polar((0.0, 0.0), 1.0, angle)
    normal = (-direction[1], direction[0])
    return _offset(base, normal, shift), direction, normal


def _chain_anchor(op: DimOp, point: Point, shift: float, /) -> Point:
    # a measured point's seat ON the chain dimension line — the affine projection, so a pairwise midpoint projects
    # onto the midpoint of its projected anchors and text, terminators, and construction all share one line.
    origin, direction, _normal = _chain_line(op, shift)
    along = (point[0] - origin[0]) * direction[0] + (point[1] - origin[1]) * direction[1]
    return (origin[0] + direction[0] * along, origin[1] + direction[1] * along)


def _bounds(points: Iterable[Point], /) -> Box:
    pts = tuple(points)
    return (min(p[0] for p in pts), min(p[1] for p in pts), max(p[0] for p in pts), max(p[1] for p in pts)) if pts else (0.0, 0.0, 0.0, 0.0)


def _union(left: Box, right: Box, /) -> Box:
    return (min(left[0], right[0]), min(left[1], right[1]), max(left[2], right[2]), max(left[3], right[3]))


def _scene_box(dim: Dimension, /) -> Box:
    return _bounds(tuple(point for op in dim.ops for point in _scene_points(op)))


def _scene_points(op: DimOp, /) -> tuple[Point, ...]:
    match op:
        case DimOp(tag="chain", chain=(base, points, *_)):
            return (base, *points)
        case DimOp(tag="baseline", baseline=(base, datum, points, *_)):
            return (base, datum, *points)
        case DimOp(
            tag=(
                "linear"
                | "aligned"
                | "angular2l"
                | "angular3p"
                | "angularcra"
                | "radius"
                | "diameter"
                | "ordinate"
                | "arc3p"
                | "arccra"
                | "fcf"
                | "datum_feature"
            )
        ):
            return _endpoints(op)
        case _ as unreachable:
            assert_never(unreachable)


def _gdt_zone(frame: GdtFrame, /) -> str:
    # the one zone-compartment spelling — ⌀ prefix, magnitude, material-condition glyph — every GD&T consumer reads.
    return ("⌀" if frame.diameter else "") + frame.tolerance + _MOD_GLYPH[frame.modifier]


def _gdt_cells(op: DimOp, over: Override, /) -> tuple[tuple[str, Box], ...]:
    # one ISO 1101 compartment derivation both the construction outlines and the per-cell text seats read: each cell
    # is (content, box) off a single edge fold, so a compartment gains its frame and its centred text from one row
    # and a non-GD&T case yields no cells.
    cell = float(over.get("dimtxt", 2.5)) * 2.0
    match op:
        case DimOp(tag="fcf", fcf=(_anchor, insert, frame, *_)):
            zone = _gdt_zone(frame)
            texts = (frame.characteristic.glyph, zone, *frame.datums)
            widths = (cell, max(len(zone) * cell * 0.35, cell), *(cell,) * len(frame.datums))
        case DimOp(tag="datum_feature", datum_feature=(_anchor, insert, letter, *_)):
            texts, widths = (letter,), (cell,)
        case _:
            return ()
    x0, y0 = insert
    return tuple((text, (x0 + lo, y0, x0 + hi, y0 + cell)) for text, (lo, hi) in zip(texts, pairwise(accumulate(widths, initial=0.0)), strict=True))


def _text_anchor(op: DimOp, /) -> Point:
    match op:  # a GD&T leader seats at the frame insert — per-cell text centres derive through `_gdt_cells`; every dimensional case centres on the measured span
        case DimOp(tag="fcf", fcf=(_anchor, insert, *_)) | DimOp(tag="datum_feature", datum_feature=(_anchor, insert, *_)):
            return insert
        case DimOp(
            tag=(
                "linear"
                | "aligned"
                | "angular2l"
                | "angular3p"
                | "angularcra"
                | "radius"
                | "diameter"
                | "ordinate"
                | "arc3p"
                | "arccra"
                | "chain"
                | "baseline"
            )
        ):
            (start, finish), _normal = _measured(op)
            return ((start[0] + finish[0]) / 2.0, (start[1] + finish[1]) / 2.0)
        case _ as unreachable:
            assert_never(unreachable)


def _annotation_text(op: DimOp, tol: DimTol, /) -> str:
    return _resolved_text(_measurement(op), tol)


def _resolved_text(measurement: str, tol: DimTol, /) -> str:
    match tol:  # an explicit override wins; else the true measured value per case
        case DimTol(tag="custom", custom=text):
            return text
        case DimTol(tag="basic", basic=value):
            return f"{value:g}"
        case DimTol(tag="auto") | DimTol(tag="symmetric") | DimTol(tag="deviation") | DimTol(tag="limits"):
            return measurement
        case _ as unreachable:
            assert_never(unreachable)


def _annotations(op: DimOp, tol: DimTol, over: Override, /) -> tuple[tuple[str, Point], ...]:
    match op:
        case DimOp(tag="fcf") | DimOp(tag="datum_feature"):
            # ISO 1101 per-compartment seating: each characteristic, zone, and datum text centres in ITS cell off the
            # shared `_gdt_cells` fold — never the whole frame string dropped at the insert corner.
            return tuple((text, ((x0 + x1) / 2.0, (y0 + y1) / 2.0)) for text, (x0, y0, x1, y1) in _gdt_cells(op, over))
        case DimOp(tag="chain", chain=(_base, points, angle, *_)):
            # native chain parity: each segment value is the distance PROJECTED along the angle direction — the
            # measurement `add_multi_point_linear_dim` renders — never the raw euclidean point spacing.
            direction = _polar((0.0, 0.0), 1.0, angle)
            return tuple(
                (
                    _resolved_text(f"{abs((finish[0] - start[0]) * direction[0] + (finish[1] - start[1]) * direction[1]):g}", tol),
                    ((start[0] + finish[0]) / 2.0, (start[1] + finish[1]) / 2.0),
                )
                for start, finish in pairwise(points)
            )
        case DimOp(tag="baseline", baseline=(_base, datum, points, *_)):
            return tuple(
                (
                    _resolved_text(f"{float(np.hypot(point[0] - datum[0], point[1] - datum[1])):g}", tol),
                    ((datum[0] + point[0]) / 2.0, (datum[1] + point[1]) / 2.0),
                )
                for point in points
            )
        case DimOp(
            tag=(
                "linear"
                | "aligned"
                | "angular2l"
                | "angular3p"
                | "angularcra"
                | "radius"
                | "diameter"
                | "ordinate"
                | "arc3p"
                | "arccra"
            )
        ):
            return ((_annotation_text(op, tol), _text_anchor(op)),)
        case _ as unreachable:
            assert_never(unreachable)


def _measurement(op: DimOp, /) -> str:
    # Layered auto value per case — length, R/⌀, angular degrees, ⌢ arc length — never an empty arm for an admitted case.
    match op:
        case DimOp(tag="radius", radius=(_center, radius, *_)):
            return f"R{radius:g}"
        case DimOp(tag="diameter", diameter=(_center, radius, *_)):
            return f"⌀{radius * 2.0:g}"
        case DimOp(tag="angular2l", angular2l=(_base, line1, line2, *_)):
            # normalize over the full turn THEN take the smaller ray angle — a bare `% 180` folds antiparallel
            # lines (spread exactly 180°) onto 0°, erasing the measurement.
            turn = abs(_angle_of(line1[0], line1[1]) - _angle_of(line2[0], line2[1])) % 360.0
            return f"{min(turn, 360.0 - turn):.1f}°"
        case DimOp(tag="angular3p", angular3p=(base, center, p1, p2, *_)):
            return f"{_span(center, p1, p2, base):.1f}°"
        case DimOp(tag="angularcra", angularcra=(_center, _radius, start, end, *_)):
            return f"{(end - start) % 360.0:.1f}°"
        case DimOp(tag="arc3p", arc3p=(base, center, p1, p2, *_)):
            radius = float(np.hypot(p1[0] - center[0], p1[1] - center[1]))
            return f"⌢{radius * float(np.deg2rad(_span(center, p1, p2, base))):.1f}"
        case DimOp(tag="arccra", arccra=(_center, radius, start, end, *_)):
            return f"⌢{radius * float(np.deg2rad((end - start) % 360.0)):.1f}"
        case DimOp(tag="fcf", fcf=(_anchor, _insert, frame, *_)):
            return " ".join((frame.characteristic.glyph, _gdt_zone(frame), *frame.datums))
        case DimOp(tag="datum_feature", datum_feature=(_anchor, _insert, letter, *_)):
            return letter
        case DimOp(tag="ordinate", ordinate=(feature, _leader, axis, origin, *_)):
            # ordinate value IS the datum-relative coordinate along the measured axis — never the leader length
            return f"{(feature[0] - origin[0]) if axis is OrdinateAxis.X else (feature[1] - origin[1]):g}"
        case (
            DimOp(tag="linear")
            | DimOp(tag="aligned")
            | DimOp(tag="chain")
            | DimOp(tag="baseline")
        ):
            (start, finish), _ = _measured(op)
            return f"{float(np.hypot(finish[0] - start[0], finish[1] - start[1])):g}"
        case _ as unreachable:
            assert_never(unreachable)


def _tol_latex(tol: DimTol, /) -> Option[str]:
    match tol:  # every non-auto tolerance mode typesets — symmetric/deviation/limits/basic all reach the math layer
        case DimTol(tag="symmetric", symmetric=pm):
            return Some(rf"\pm{pm:g}")
        case DimTol(tag="deviation", deviation=(upper, lower)):
            return Some(rf"{{}}^{{+{upper:g}}}_{{{lower:g}}}")
        case DimTol(tag="limits", limits=(upper, lower)):
            return Some(rf"\frac{{{upper:g}}}{{{lower:g}}}")
        case DimTol(tag="basic", basic=value):
            return Some(rf"\boxed{{{value:g}}}")
        case DimTol(tag="auto") | DimTol(tag="custom"):
            return Nothing
        case _ as unreachable:
            assert_never(unreachable)


def _lines(points: Iterable[Point], /, *, close: bool = False, fill: str = "none") -> Fragment:
    # one drawsvg polyline author — never a hand-formatted `<path d>` string.
    flat = tuple(coordinate for point in points for coordinate in point)
    return drawsvg.Lines(*flat, close=close, fill=fill)


def _layer_bytes(fragments: Iterable[Fragment], box: Box, pen: str, width: float, /) -> bytes:
    # one penned drawsvg group per layer so the lines actually stroke; a filled mark overrides fill per element.
    xmin, ymin, xmax, ymax = box
    canvas = drawsvg.Drawing(max(xmax - xmin, 1.0), max(ymax - ymin, 1.0), origin=(xmin, ymin))
    group = drawsvg.Group(stroke=pen, stroke_width=width, fill="none")
    for fragment in fragments:  # Exemption: drawsvg Group is the mutable child-list sink
        group.append(fragment)
    canvas.append(group)
    return canvas.as_svg().encode()


def _canvas(box: Box, /) -> Element:
    xmin, ymin, xmax, ymax = box
    canvas = Element("{http://www.w3.org/2000/svg}svg")
    canvas.set("viewBox", f"{xmin:g} {ymin:g} {max(xmax - xmin, 1.0):g} {max(ymax - ymin, 1.0):g}")
    return canvas


def _pen(standard: Standard, /) -> str:
    # ISO 13567 DIMS-layer discipline pen resolved to sRGB hex through Standard.rgb — the ONE colour every layer shares.
    red, green, blue = standard.rgb(_DIMS)
    return f"#{red:02x}{green:02x}{blue:02x}"


def _outline_font(standard: Standard, /) -> "ziafont.Font":
    # ziafont reads sfnt only — a profile naming an .shx CAD font falls to the bundled face, never a parse fault.
    named = standard.font.to_optional()
    return ziafont.Font(named if named is not None and named.endswith((".ttf", ".otf")) else None)


def _terminator_kind(over: Override, /) -> MarkKind:
    # ISO 129-1 terminator from the override's dimblk/dimtsz — a positive DIMTSZ is the oblique tick, else the arrow block.
    if float(over.get("dimtsz", 0.0)) > 0.0:
        return "oblique"
    match str(over.get("dimblk", "")):
        case "OPEN":
            return "open"
        case "DOT":
            return "dot"
        case "ORIGIN":
            return "origin"
        case "NONE":
            return "none"
        case _:
            return "arrow"


def _terminator_anchors(op: DimOp, /) -> tuple[tuple[Point, Point], ...]:
    # per-case (point, outward tangent) pairs — a curved dimension arrows tangent to the arc it measures, a radial
    # leader arrows at its edge alone, a GD&T frame arrows at its feature anchor.
    match op:
        case DimOp(tag="radius", radius=(center, radius, angle, *_)):
            edge = _polar(center, radius, angle)
            return ((edge, _unit(center, edge)),)
        case DimOp(tag="diameter", diameter=(center, radius, angle, *_)):
            edge, far = _polar(center, radius, angle), _polar(center, radius, angle + 180.0)
            return ((edge, _unit(center, edge)), (far, _unit(center, far)))
        case DimOp(tag="angular3p", angular3p=(_base, center, p1, p2, *_)) | DimOp(tag="arc3p", arc3p=(_base, center, p1, p2, *_)):
            return ((p1, _tangent(center, p1)), (p2, _tangent(center, p2)))
        case DimOp(tag="angularcra", angularcra=(center, radius, start, end, *_)) | DimOp(tag="arccra", arccra=(center, radius, start, end, *_)):
            lo, hi = _polar(center, radius, start), _polar(center, radius, end)
            return ((lo, _tangent(center, lo)), (hi, _tangent(center, hi)))
        case DimOp(tag="fcf", fcf=(anchor, insert, *_)) | DimOp(tag="datum_feature", datum_feature=(anchor, insert, *_)):
            return ((anchor, _unit(insert, anchor)),)
        case DimOp(tag="chain", chain=(_base, points, angle, *_)):
            # chain marks align to the base/angle dimension line the anchors project onto, never the raw point-run direction.
            direction = _polar((0.0, 0.0), 1.0, angle)
            return tuple(
                anchor
                for start, finish in pairwise(points)
                for anchor in ((start, (-direction[0], -direction[1])), (finish, direction))
            )
        case DimOp(tag="baseline", baseline=(_base, datum, points, *_)):
            return tuple(
                anchor
                for point in points
                for anchor in ((datum, tuple(-axis for axis in _unit(datum, point))), (point, _unit(datum, point)))
            )
        case DimOp(tag="ordinate"):  # ISO 129-1 ordinate leaders terminate bare — no mark on either end
            return ()
        case DimOp(tag="linear") | DimOp(tag="aligned") | DimOp(tag="angular2l"):
            (start, finish), _normal = _measured(op)
            direction = _unit(start, finish)
            return ((start, (-direction[0], -direction[1])), (finish, direction))
        case _ as unreachable:
            assert_never(unreachable)


def _mark(point: Point, tangent: Point, size: float, width: float, kind: MarkKind, pen: str, /) -> Fragment:
    # Self-contained ISO 129-1 terminator — filled arrow / oblique tick / dot / origin circle / open chevron /
    # datum triangle; a tapered variable-width terminator composes region outline, the filled default stays here.
    px, py = point
    normal = (-tangent[1], tangent[0])
    wing1 = (px - tangent[0] * size + normal[0] * size * 0.3, py - tangent[1] * size + normal[1] * size * 0.3)
    wing2 = (px - tangent[0] * size - normal[0] * size * 0.3, py - tangent[1] * size - normal[1] * size * 0.3)
    match kind:
        case "oblique":  # the 45-degree architectural tick, stroked
            lead = (tangent[0] + normal[0], tangent[1] + normal[1])
            return drawsvg.Line(
                px - lead[0] * size * 0.5, py - lead[1] * size * 0.5, px + lead[0] * size * 0.5, py + lead[1] * size * 0.5, stroke=pen, stroke_width=width
            )
        case "dot":
            return drawsvg.Circle(px, py, size * 0.25, fill=pen)
        case "origin":  # the ISO ordinate/chain origin indication — a small open circle
            return drawsvg.Circle(px, py, size * 0.4, fill="none", stroke=pen, stroke_width=width)
        case "open":
            return _lines((wing1, point, wing2))
        case "datum":  # the ISO 5459 filled datum triangle
            return _lines((point, wing1, wing2), close=True, fill=pen)
        case "arrow":
            return _lines((point, wing1, wing2), close=True, fill=pen)
        case "none":
            return drawsvg.Group()
        case _ as unreachable:
            assert_never(unreachable)


def _construction(op: DimOp, over: Override, shift: float = 0.0, /) -> tuple[Block[Fragment], Box]:
    # LAYERED geometry from the ezdxf.math construction kernel — never hand-rolled trig. A CURVED dimension
    # decomposes to the ARC/CIRCLE it MEASURES; an ordinate to its axis dogleg; a GD&T case to leader + frame boxes.
    # `shift` is the `_stack`-solved dimension-line offset, so the layered target shares the SAME DIMDLI
    # separation the native target renders — one solved offset map governs every target.
    match op:
        case DimOp(tag="diameter", diameter=(center, radius, angle, *_)):
            edge, far = _polar(center, radius, angle), _polar(center, radius, angle + 180.0)
            verts = _arc_verts(ezmath.ConstructionCircle(center, radius))  # the ⌀ diametric leader + the measured circle
            return Block.of_seq((_lines(verts), _lines((far, edge)))), _bounds((*verts, edge, far))
        case DimOp(tag="radius", radius=(center, radius, angle, *_)):
            edge = _polar(center, radius, angle)
            verts = _arc_verts(ezmath.ConstructionArc(center, radius, angle - _ARC_SPAN, angle + _ARC_SPAN))  # the R leader + its reference arc
            return Block.of_seq((_lines(verts), _lines((center, edge)))), _bounds((*verts, center, edge))
        case DimOp(tag="angular3p", angular3p=(base, center, p1, p2, *_)) | DimOp(tag="arc3p", arc3p=(base, center, p1, p2, *_)):
            radius = float(np.hypot(p1[0] - center[0], p1[1] - center[1]))
            start, end = _arc_angles(center, p1, p2, base)
            verts = _arc_verts(ezmath.ConstructionArc(center, radius, start, end))
            return Block.of_seq((_lines(verts), _lines((center, p1)), _lines((center, p2)))), _bounds((*verts, center, p1, p2))
        case DimOp(tag="angularcra", angularcra=(center, radius, start, end, *_)) | DimOp(tag="arccra", arccra=(center, radius, start, end, *_)):
            lo_pt, hi_pt = _polar(center, radius, start), _polar(center, radius, end)
            verts = _arc_verts(ezmath.ConstructionArc(center, radius, start, end))
            return Block.of_seq((_lines(verts), _lines((center, lo_pt)), _lines((center, hi_pt)))), _bounds((*verts, center, lo_pt, hi_pt))
        case DimOp(tag="angular2l", angular2l=(_base, line1, line2, *_)):
            vertex = ezmath.ConstructionLine(line1[0], line1[1]).intersect(ezmath.ConstructionLine(line2[0], line2[1]))
            if vertex is None:  # unreachable past the Angular2L parallel refusal; a numeric near-parallel edge still refuses typed
                raise ValueError(f"angular2l lines are parallel: {line1!r} {line2!r}")
            apex = (float(vertex.x), float(vertex.y))
            radius = float(np.hypot(line1[1][0] - apex[0], line1[1][1] - apex[1]))
            verts = _arc_verts(ezmath.ConstructionArc(apex, radius, _angle_of(apex, line1[1]), _angle_of(apex, line2[1])))
            return Block.of_seq((_lines(verts), _lines((apex, line1[1])), _lines((apex, line2[1])))), _bounds((*verts, apex, line1[1], line2[1]))
        case DimOp(tag="ordinate", ordinate=(feature, leader_end, axis, _origin, *_)):
            knee = (feature[0], leader_end[1]) if axis is OrdinateAxis.X else (leader_end[0], feature[1])
            return Block.singleton(_lines((feature, knee, leader_end))), _bounds((feature, knee, leader_end))
        case DimOp(tag="fcf", fcf=(anchor, insert, *_)) | DimOp(tag="datum_feature", datum_feature=(anchor, insert, *_)):
            # both GD&T outlines derive from the same `_gdt_cells` fold the per-cell text seats read, so frame
            # geometry and text placement cannot drift apart.
            cells = _gdt_cells(op, over)
            boxes = Block.of_seq(_lines(((x0, y0), (x1, y0), (x1, y1), (x0, y1)), close=True) for _text, (x0, y0, x1, y1) in cells)
            far = cells[-1][1]
            return boxes.cons(_lines((anchor, insert))), _bounds((anchor, insert, (far[2], far[3])))
        case DimOp(tag="chain", chain=(_base, points, *_)):
            # native `add_multi_point_linear_dim` parity: chain segments ride the base/angle dimension line at the
            # solved offset, each measured point contributing one extension line up to its projected anchor plus DIMEXE.
            ext = float(over.get("dimexe", 1.25))
            anchors = tuple(_chain_anchor(op, point, shift) for point in points)
            _origin, _direction, normal = _chain_line(op, shift)
            witnesses = Block.of_seq(_lines((point, _offset(anchor, normal, ext))) for point, anchor in zip(points, anchors, strict=True))
            return witnesses.append(Block.of_seq(_lines(pair) for pair in pairwise(anchors))), _bounds((*points, *anchors))
        case DimOp(tag="baseline", baseline=(_base, datum, points, *_)):
            # Solved shift plus the DIMDLI lane fan mirror the native per-line stepping, so each layered baseline
            # lane sits on the parallel dimension line `_lower` renders at that offset.
            step = float(over.get("dimdli", 8.0))
            _, normal = _measured(op)
            lanes = tuple((_offset(datum, normal, shift + rank * step), _offset(point, normal, shift + rank * step)) for rank, point in enumerate(points))
            return Block.of_seq(_lines(pair) for pair in lanes), _bounds((datum, *points, *(end for pair in lanes for end in pair)))
        case DimOp(tag="linear") | DimOp(tag="aligned"):
            ext = float(over.get("dimexe", 1.25)) + shift
            (start, finish), normal = _measured(op)
            lo, hi = _offset(start, normal, ext), _offset(finish, normal, ext)
            return Block.of_seq((_lines((lo, hi)), _lines((start, lo)), _lines((finish, hi)))), _bounds((start, finish, lo, hi))
        case _ as unreachable:
            assert_never(unreachable)


def _stack_shift(op: DimOp, shift: float, point: Point, /, *, lane: int = 0, step: float = 0.0) -> Point:
    # translate a stackable case's anchor along its measured normal by the `_stack`-solved offset — a baseline anchor
    # adds its `lane` ordinal times DIMDLI, the native per-line stepping; a chain anchor instead PROJECTS onto its
    # base/angle dimension line — so terminators, measurement text, and tolerance text land on the SAME shifted
    # dimension line the construction drew.
    if op.tag == "chain":
        return _chain_anchor(op, point, shift)  # projection, never a bare translation: even at zero shift the anchor seats on the base line
    if op.tag not in ("linear", "aligned", "baseline"):
        return point
    distance = shift + (lane * step if op.tag == "baseline" else 0.0)
    if distance == 0.0:
        return point
    _, normal = _measured(op)
    return (point[0] + normal[0] * distance, point[1] + normal[1] * distance)


def _terminator_bytes(dim: Dimension, pen: str, envelope: Box, offsets: Map[int, float] = Map.empty(), /) -> bytes:
    # each terminator drawn as a self-contained mark at the anchor its case terminates on, aligned to that case's tangent;
    # the caller-computed envelope is the ONE canvas box every sublayer shares, so stacked or extended geometry never clips
    # and the layers register onto one coordinate frame.
    fragments: Block[Fragment] = Block.empty()
    for index, op in enumerate(dim.ops):
        over = dim.standard.dimstyle(_facets(op)[0])
        shift = offsets.try_find(index).default_value(0.0)
        size, width = float(over.get("dimasz", 2.5)), max(float(over.get("dimtxt", 2.5)) * 0.1, 0.13)
        kind = "datum" if op.tag == "datum_feature" else _terminator_kind(over)
        step = float(over.get("dimdli", 8.0))
        marks = Block.of_seq(  # baseline anchors arrive in datum/point pairs per lane, so `rank // 2` is the lane ordinal
            _mark(_stack_shift(op, shift, point, lane=rank // 2, step=step), tangent, size, width, kind, pen)
            for rank, (point, tangent) in enumerate(_terminator_anchors(op))
        )
        fragments = fragments.append(marks)
    return _layer_bytes(fragments, envelope, pen, 0.13)


def _annotation_bytes(dim: Dimension, pen: str, envelope: Box, offsets: Map[int, float] = Map.empty(), /) -> bytes:
    # ISO 3098 measurement text outlined to font-independent <path> through ziafont; getsize() centres the run on the
    # anchor and getyofst() lifts its baseline so the text floats ABOVE the dimension line.
    font, canvas = _outline_font(dim.standard), _canvas(envelope)
    for index, op in enumerate(dim.ops):  # Exemption: ziafont Text.drawon composes <symbol> defs into the mutable ET canvas
        family, tol = _facets(op)
        over = dim.standard.dimstyle(family)
        shift = offsets.try_find(index).default_value(0.0)
        for rank, (label, raw_anchor) in enumerate(_annotations(op, tol, over)):
            anchor = _stack_shift(op, shift, raw_anchor, lane=rank, step=float(over.get("dimdli", 8.0)))
            run = font.text(label, size=float(over.get("dimtxt", 2.5)), valign="base", color=pen)
            width, _height = run.getsize()
            run.drawon(canvas, anchor[0] - width / 2.0, anchor[1] - run.getyofst())
    return tostring(canvas)


def _tolerance_bytes(dim: Dimension, pen: str, envelope: Box, offsets: Map[int, float] = Map.empty(), /) -> bytes:
    # every non-auto DimTol typesets through the typography/math Formula owner — never a second ziamath import —
    # and `seat` resolves the seated origin so the tolerance clears the value to the right, never colliding with it.
    canvas = _canvas(envelope)
    for index, op in enumerate(dim.ops):  # Exemption: the equation fragments compose into the mutable ET canvas
        family, tol = _facets(op)
        over = dim.standard.dimstyle(family)
        shift = offsets.try_find(index).default_value(0.0)
        for rank, (_label, raw_anchor) in enumerate(_annotations(op, tol, over)):
            anchor = _stack_shift(op, shift, raw_anchor, lane=rank, step=float(over.get("dimdli", 8.0)))
            for expr in _tol_latex(tol).to_seq():
                text_h = float(over.get("dimtxt", 2.5))
                laid = Formula(spec=FormulaSpec(latex=LatexSpec(source=expr, size=text_h, display=False, color=pen))).laid()
                match laid:
                    case Result(tag="ok", ok=frag):
                        sx, sy = seat(frag, anchor[0] + text_h * 2.0, anchor[1])
                        node = fromstring(frag.svg)
                        node.set("x", f"{sx:g}")
                        node.set("y", f"{sy:g}")
                        canvas.append(node)
                    case Result(tag="error", error=fault):
                        # terminal collapse: the refusal raises so the offload lane folds it onto the owning rail —
                        # a layered receipt never omits a tolerance the spec demanded.
                        raise ValueError(f"tolerance typeset refused: {op.tag} {expr}: {fault}")
                    case _ as unreachable:
                        assert_never(unreachable)
    return tostring(canvas)


# --- [TABLES] ---------------------------------------------------------------------------
_GDT_GLYPH: Final[Map[GdtChar, str]] = Map.of_seq([
    (GdtChar.STRAIGHTNESS, "⏤"),
    (GdtChar.FLATNESS, "⏥"),
    (GdtChar.CIRCULARITY, "○"),
    (GdtChar.CYLINDRICITY, "⌭"),
    (GdtChar.PROFILE_LINE, "⌒"),
    (GdtChar.PROFILE_SURFACE, "⌓"),
    (GdtChar.ANGULARITY, "∠"),
    (GdtChar.PERPENDICULARITY, "⊥"),
    (GdtChar.PARALLELISM, "∥"),
    (GdtChar.POSITION, "⌖"),
    (GdtChar.CONCENTRICITY, "◎"),
    (GdtChar.SYMMETRY, "⌯"),
    (GdtChar.CIRCULAR_RUNOUT, "↗"),
    (GdtChar.TOTAL_RUNOUT, "⌰"),
])
_MOD_GLYPH: Final[Map[GdtModifier, str]] = Map.of_seq([
    (GdtModifier.NONE, ""),
    (GdtModifier.MMC, "Ⓜ"),
    (GdtModifier.LMC, "Ⓛ"),
    (GdtModifier.RFS, "Ⓢ"),
])


def _egress_dxf(doc: "Drawing", _msp: "Modelspace", _w: float, _h: float, /) -> bytes:
    stream = io.StringIO()
    doc.write(stream)
    return stream.getvalue().encode()


def _egress_svg(doc: "Drawing", msp: "Modelspace", width: float, height: float, /) -> bytes:
    backend = SVGBackend()
    Frontend(RenderContext(doc), backend).draw_layout(msp, finalize=True)
    return backend.get_string(_page(width, height)).encode()


def _egress_pdf(doc: "Drawing", msp: "Modelspace", width: float, height: float, /) -> bytes:
    backend = PyMuPdfBackend()
    Frontend(RenderContext(doc), backend).draw_layout(msp, finalize=True)
    return backend.get_pdf_bytes(_page(width, height))


_BACKENDS: frozendict[DimTarget, DimBackend] = frozendict({
    DimTarget.DXF: DimBackend(egress=_egress_dxf, kind="drawing"),
    DimTarget.SVG: DimBackend(egress=_egress_svg, kind="drawing"),
    DimTarget.PDF: DimBackend(egress=_egress_pdf, kind="pdf"),
})


def _native(dim: Dimension, /) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    doc, msp, count, dimstyle = _lowered(dim)
    width, height = _extent(msp)
    backend = _BACKENDS[dim.target]
    data = backend.egress(doc, msp, width, height)
    receipt: ArtifactReceipt = (
        ArtifactReceipt.Pdf(dim._key, len(data), 1)
        if backend.kind == "pdf"
        else ArtifactReceipt.Drawing(dim._key, "drawing-dimension", count, dimstyle, round(width), round(height), len(data))
    )
    return (_row(f"dimension.{dim.target.value}", data),), receipt


def _layered(dim: Dimension, /) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    # decompose each dimension into named editable layers over the component authors — geometry buckets every op's
    # construction fragments; terminator/text/tolerance ride their own rows in z order.
    ziafont.config.precision = _PRECISION  # once inside the offload lane — deterministic d-floats -> stable content key; math precision is Formula-owned config
    geometry, pen = Block.empty(), _pen(dim.standard)
    envelope, offsets = _scene_box(dim), _stack(dim)
    for index, op in enumerate(dim.ops):
        fragments, box = _construction(op, dim.standard.dimstyle(_facets(op)[0]), offsets.try_find(index).default_value(0.0))
        geometry = geometry.append(fragments)
        envelope = _union(envelope, box)
    rows = (
        # ONE envelope — scene box unioned with every construction box — feeds all four sublayers, so extended
        # geometry never clips and every layer registers onto the same coordinate frame.
        ("dimension-line", _layer_bytes(geometry, envelope, pen, 0.25)),
        ("dimension-terminator", _terminator_bytes(dim, pen, envelope, offsets)),
        ("dimension-text", _annotation_bytes(dim, pen, envelope, offsets)),
        ("dimension-tolerance", _tolerance_bytes(dim, pen, envelope, offsets)),
    )
    layers = tuple(_row(name, source, z) for z, (name, source) in enumerate(rows))
    facts = ArtifactReceipt.Drawing(
        dim._key,
        "drawing-dimension",
        len(dim.ops),
        "ISO-129",
        round(envelope[2] - envelope[0]),
        round(envelope[3] - envelope[1]),
        sum(len(source) for _name, source in rows),
    )
    return layers, facts


_ENGINES: frozendict[DimTarget, DimArm] = frozendict({
    DimTarget.DXF: _native,
    DimTarget.SVG: _native,
    DimTarget.PDF: _native,
    DimTarget.LAYERED: _layered,
})

# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["Dimension", "DimOp", "DimTarget", "DimTol", "GdtChar", "GdtFrame", "GdtModifier", "OrdinateAxis"]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
