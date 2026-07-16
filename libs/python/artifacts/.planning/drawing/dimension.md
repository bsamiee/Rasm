# [PY_ARTIFACTS_DRAWING_DIMENSION]

The ISO 129-1 dimensioning producer: `Dimension` is ONE owner over a closed `DimOp` union — `Linear`/`Aligned`/`Angular2L`/`Angular3P`/`AngularCRA`/`Radius`/`Diameter`/`Ordinate`/`Arc3P`/`ArcCRA`/`Chain`/`Baseline`, one case per ISO 129-1 kind and construction form — each carrying ONLY its own typed geometry plus the `drawing/standard#DIMSTYLE` `DimStyleFamily` selector and a `DimTol` tolerance. Every dimension lowers onto its verified `ezdxf` `add_*_dim` builder threaded with the `override=` DIM-variable dict `Standard.dimstyle(family)` derives, so the extension lines, dimension line, native ISO terminators, and measurement text are `ezdxf`-authored ISO 129-1 geometry — never hand-emitted extension-line/terminator/text trigonometry, and never a linear-only slice where the full family is the design claim. A new drafting dimension is one `DimOp` case plus one builder arm, never a per-dimension class family.

Each dimension DUAL-lowers over the `DimTarget` policy value. The `ezdxf`-native path (`DXF` the `Drawing.write` blob, `SVG` the `SVGBackend`, `PDF` the `PyMuPdfBackend`) LEADS with `add_*_dim().render()` and the ISO tolerance as native DIM-variables (`dimtol`/`dimtp`/`dimtm`, `dimlim`, `MTextEditor.stack` for the stacked deviation), while the `LAYERED` path DECOMPOSES each dimension into named editable `export/layered#LAYERED` `Layer` rows — the extension/dimension-line geometry from `ezdxf.math.Construction*` anchor math (never hand-rolled trig), the ISO 129-1 terminators as self-contained filled/stroked marks (the correct default, a tapered variable-width terminator composing the landed `graphic/vector/region#REGION` `outline`), the ISO 3098 measurement text outlined through `ziafont` (`typography/shape#SHAPE` owns the shaped run), and a tolerance/limit expression typeset through `ziamath.Latex` seated at the math axis — all penned by the discipline sRGB `Standard.rgb` resolves. `kiwisolver` `Solver` + `strength` bands solve the dimension-line offset STACK a fixed offset gets wrong. The `override=` DIM-variables are the `drawing/standard#DIMSTYLE` derivation scaled by the ISO 5455 factor, the render offloads onto the runtime thread lane, and the owner contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Drawing` case (or reused `ArtifactReceipt.Pdf` on the `PDF` backend) and one `core/plan#PLAN` `ArtifactWork` node — minting no IFC (`csharp:Rasm.Bim`) and computing no sheet placement, the dimensioned SVG/PDF bytes feeding `composition/sheet#SHEET`'s `FigurePlacement` as a bytes seam.

## [01]-[INDEX]

- [01]-[DIMENSION]: the `Dimension` owner over the closed `DimOp` union (`Linear`/…/`Baseline`), dual-lowering over `DimTarget` into the `ezdxf`-native render (DXF/SVG/PDF) or the `LAYERED` decomposition of named `export/layered#LAYERED` `Layer` rows.

## [02]-[DIMENSION]

- Owner: `Dimension` holds `ops`, the resolved `drawing/standard#STANDARD` `Standard`, and the `DimTarget` value, discriminating over the closed `DimOp` whose every case carries ONLY its own geometry plus the `DimStyleFamily`/`DimTol` facet slots — never a per-dimension `LinearDim`/`RadialDim` class family, never a monolithic bag whose angular/radial fields are dead for most cases. `DimTol` is the closed tolerance vocabulary (`Auto`/`Custom`/`Symmetric`/`Deviation`/`Limits`/`Basic`) every case's second facet carries, so a toleranced dimension is one facet, not a parallel type. `DimTarget` keys the `_ENGINES` dual-lowering table (`DXF`/`SVG`/`PDF` sharing the `_native` arm through `_BACKENDS`, `LAYERED` the `_layered` arm), so a new egress is one row.
- Cases: each `DimOp` case ends in the `(DimStyleFamily, DimTol)` facet pair and lowers onto its verified `ezdxf` builder (`add_linear_dim`/`add_aligned_dim`/`add_angular_dim_2l`/`_3p`/`_cra`/`add_radius_dim`/`add_diameter_dim`/`add_ordinate_x_dim`/`_y_dim`/`add_arc_dim_3p`/`_cra`/the self-rendering `add_multi_point_linear_dim` chain, `Baseline` a fold of `add_linear_dim` stepping by DIMDLI), matched by one total `match` in `_lower` — the `Angular3P`/`Arc3P` and `AngularCRA`/`ArcCRA` payload shapes coincide but each lowers onto a distinct builder. `OrdinateAxis` routes `add_ordinate_x_dim`/`_y_dim`.
- Entry: `Dimension.over` normalizes `DimOp | Iterable[DimOp]` by a structural `match` at the head — never a `batch` knob. `render` returns `RuntimeRail[ArtifactReceipt]` beside the `layered()` `LayerPlan` projection; the `_native` arm seeds the `Standard` resources, solves the offset stack once, folds each `DimOp` through `_lower`, and egresses through the target's `DimBackend`, while the `_layered` arm decomposes each dimension into named `Layer` rows over the `ezdxf.math`/region/`ziafont`/`ziamath` component authors.
- Auto: `_facets` projects each case's `(family, tol)` once through one total or-pattern; `over = dict(standard.dimstyle(family)) | _tol_over(tol)` scales the DIM-variables by the ISO 5455 factor so a `1:50` dimension draws its 2.5 mm text at paper scale with zero per-arm literal. The tolerance lowers by mode onto the three distinct native mechanisms — `Symmetric` onto `dimtol`, `Deviation` onto an `MTextEditor.stack` stacked fraction, `Limits` onto `dimlim`, `Basic` onto a negative-`dimgap` boxed value — never a hand-formatted `± ` string. `_stack` threads one `kiwisolver.Solver`: a `required` anchor, `required` min-separation ≥ DIMDLI, and a custom `strength.create(0,1,0,4)` equal-gap band above plain `weak`; `Constraint.violated()` reads which soft gaps the solve sacrificed, and a dense chain that collapses the distribution falls back to deterministic fixed DIMDLI stepping. The `LAYERED` `_construction` DECOMPOSES a curved dimension to the actual `ConstructionArc`/`ConstructionCircle` it MEASURES (`.flattening(_SAGITTA)`), never the bare centre→edge leader the native render never draws for a radial/angular/arc; the text and tolerance runs are MEASURED via `getsize()`/`getyofst()` so the text floats above the dimension line and the tolerance clears the value, and precision is pinned once so the outlined `d`-floats and the content key are deterministic.
- Growth: a new ISO 129-1 dimension kind or construction form is one `DimOp` case plus one `_lower` builder arm; a new egress is one `DimTarget` member plus one `_ENGINES` row (and one `_BACKENDS` row for a native backend); a new tolerance presentation is one `DimTol` case plus one `_tol_over`/`_dim_text` arm; a new DIM-variable axis is one key on the `drawing/standard#DIMSTYLE` derivation; a new stacking rule is one `kiwisolver` constraint at its `strength` band; a new `LAYERED` component author is one `_layered` layer function over the existing owners. Zero new surface for a new dimension or a new layer.
- Boundary: no IFC, sheet-placement, or annotation-leader logic — `csharp:Rasm.Bim`, `composition/sheet#SHEET`, `drawing/annotate#ANNOTATE`. `ezdxf` owns the ISO 129-1 dimension entity and render, `drawing/standard#DIMSTYLE` the DIM-variable derivation and discipline pen, `graphic/vector/region#REGION` the landed `outline`/`boolean` the tapered-terminator premium composes, `ziafont` the ISO 3098 text outline, `ziamath` the tolerance math, `kiwisolver` the offset solve, `export/layered#LAYERED` the layer binding, and `composition/sheet#SHEET` the placement; identity minting is the runtime's.
- Packages: `ezdxf` the ISO 129-1 dimension family (`add_*_dim` each returning a `DimStyleOverride` whose `.render()` authors geometry, the `math.Construction*`/`.flattening` anchor + measured-arc geometry, `MTextEditor.stack`, the `Frontend`/`SVGBackend`/`PyMuPdfBackend` render, `bbox.extents`, `colors.aci2rgb`); `kiwisolver` the offset stack with the custom equal-gap band and `Constraint.violated` overlap QA; `ziamath` `Latex` the tolerance-math typeset seated at `valign='axis'`, measured; `ziafont` the ISO 3098 text outline, measured, centred, baseline-seated; `numpy` the perpendicular offset normal; `expression`/`msgspec`/`beartype` the vocabulary, value objects, and `over` contract; `graphic/vector/region#REGION` a bare owner pointer — the landed `outline` the tapered-terminator premium composes, NOT imported for the base where the self-contained filled marks are the default; `export/layered#LAYERED` `Layer`; `core/receipt#RECEIPT` `ArtifactReceipt.Drawing`/`Pdf`. The `drawing/standard#DIMSTYLE`/`#STANDARD` substrate is composed as bare owner pointers, its `DimStyleFamily`/`Standard.dimstyle`/`seed`/`LayerName` lowering onto the `ezdxf` tables.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import os
from collections.abc import Callable, Iterable
from enum import StrEnum
from itertools import pairwise
from typing import TYPE_CHECKING, Literal, Self, assert_never
from xml.etree.ElementTree import Element, tostring

import numpy as np
from beartype import beartype
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.drawing.regime import Discipline, LayerName
from artifacts.drawing.standard import DimStyleFamily, Standard
from artifacts.graphic.layer import LayerContent, LayerIntent, LayerNode, LayerPlan, NamingSchema

# each proxy reifies on first render-arm use in the offloaded worker
lazy import ezdxf
lazy import kiwisolver
lazy import ziafont
lazy import ziamath
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
type Box = tuple[float, float, float, float]
type Override = dict[str, object]
type DimTag = Literal[
    "linear", "aligned", "angular2l", "angular3p", "angularcra", "radius", "diameter", "ordinate", "arc3p", "arccra", "chain", "baseline"
]
type TolTag = Literal["auto", "custom", "symmetric", "deviation", "limits", "basic"]
type DimArm = Callable[["Dimension"], tuple[tuple[LayerNode, ...], ArtifactReceipt]]  # the target-keyed lowering arm

# the stdlib bases the ezdxf DXFValueError/DXFKeyError/DXFTypeError render raises derive from — named without
# reifying the lazy ezdxf import; async_boundary classifies each into BoundaryFault, never a parallel DimFault Literal.
_FAULTS: tuple[type[Exception], ...] = (ValueError, KeyError, TypeError, OSError)
_PRECISION: int = 3  # ziafont/ziamath emitted-d-float places — the content-key determinism lever set once per offloaded LAYERED arm
_SAGITTA: float = 0.05  # LAYERED arc/circle flattening tolerance (mm) — ezdxf.math adaptive `.flattening` chord height
_ARC_SPAN: float = 30.0  # reference-arc half-span (deg) a radial dimension draws around its leader angle


class DimTarget(StrEnum):  # the dual-lowering egress — a new target is one `_ENGINES` row, never a subtype
    DXF = "dxf"  # ezdxf Drawing.write CAD blob
    SVG = "svg"  # SVGBackend.get_string native render
    PDF = "pdf"  # PyMuPdfBackend.get_pdf_bytes native render
    LAYERED = "layered"  # ezdxf.math + vector + ziafont + ziamath named export/layered Layer rows


class OrdinateAxis(StrEnum):  # the ISO 129-1 ordinate measurement axis routing add_ordinate_{x,y}_dim
    X = "x"
    Y = "y"


# --- [MODELS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class DimTol:
    # the ISO 129-1 tolerance family every DimOp facet carries; lowered by mode to native dimtol/dimlim/MTextEditor, never a hand-formatted `± value` string.
    tag: TolTag = tag()
    auto: None = case()  # the raw `<>` measurement
    custom: str = case()  # an explicit override string
    symmetric: float = case()  # a ± symmetric band -> dimtol
    deviation: tuple[float, float] = case()  # upper/lower -> MTextEditor.stack
    limits: tuple[float, float] = case()  # stacked max/min -> dimlim
    basic: float = case()  # a boxed theoretically-exact value -> negative dimgap


class DimBackend(Struct, frozen=True):
    # the native ezdxf egress: the byte emitter over the lowered Drawing plus its receipt-shape discriminant.
    egress: "Callable[[Drawing, Modelspace, float, float], bytes]"
    kind: str


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
    chain: tuple[Point, tuple[Point, ...], float, DimStyleFamily, DimTol] = case()
    baseline: tuple[Point, Point, tuple[Point, ...], float, DimStyleFamily, DimTol] = case()

    @staticmethod
    def Linear(base: Point, p1: Point, p2: Point, family: DimStyleFamily, *, angle: float = 0.0, tol: DimTol = DimTol(auto=None)) -> "DimOp":
        return DimOp(linear=(base, p1, p2, angle, family, tol))

    @staticmethod
    def Aligned(p1: Point, p2: Point, distance: float, family: DimStyleFamily, *, tol: DimTol = DimTol(auto=None)) -> "DimOp":
        return DimOp(aligned=(p1, p2, distance, family, tol))

    @staticmethod
    def Angular2L(base: Point, line1: Segment, line2: Segment, family: DimStyleFamily, *, tol: DimTol = DimTol(auto=None)) -> "DimOp":
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
    def Chain(base: Point, points: tuple[Point, ...], family: DimStyleFamily, *, angle: float = 0.0, tol: DimTol = DimTol(auto=None)) -> "DimOp":
        return DimOp(chain=(base, points, angle, family, tol))

    @staticmethod
    def Baseline(
        base: Point, datum: Point, points: tuple[Point, ...], family: DimStyleFamily, *, angle: float = 0.0, tol: DimTol = DimTol(auto=None)
    ) -> "DimOp":
        return DimOp(baseline=(base, datum, points, angle, family, tol))


# --- [SERVICES] -------------------------------------------------------------------------
class Dimension(Struct, frozen=True):
    ops: tuple[DimOp, ...]
    standard: Standard
    target: DimTarget = DimTarget.SVG

    @classmethod
    @beartype
    def over(cls, ops: DimOp | Iterable[DimOp], standard: Standard, /, *, target: DimTarget = DimTarget.SVG) -> Self:
        match ops:  # the one modal-arity head — a lone dimension is the singleton, an iterable the multi-dim set
            case DimOp():
                return cls(ops=(ops,), standard=standard, target=target)
            case _:
                return cls(ops=tuple(ops), standard=standard, target=target)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.ops)))

    @property
    def _key(self) -> ContentKey:
        # key over the frozen INPUT spec, minted pre-run — never over rendered layer bytes.
        return ContentIdentity.of(f"drawing-dimension-{self.target}", (self.ops, self.standard, self.target), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the render thunk — the receipt threads the pre-run key; the layer payload is the layered() projection.
        return (await async_boundary(f"drawing.dimension.{self.target}", self._crossed, catch=_FAULTS)).map(lambda pair: pair[1])

    async def layered(self) -> RuntimeRail[LayerPlan]:
        # the engine rows as one LayerPlan tree — substrate data the layered/sheet consumers compose, not the producer rail.
        return (await async_boundary(f"drawing.dimension.{self.target}", self._crossed, catch=_FAULTS)).map(
            lambda pair: LayerPlan(schema=NamingSchema.ISO13567, roots=pair[0])
        )

    async def _crossed(self) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
        # synchronous native fold — crosses the runtime thread lane.
        crossed = await LanePolicy.offload(_ENGINES[self.target], self, modality=Modality.THREAD, retry=RetryClass.OCCT)
        return crossed.default_with(lambda fault: _fold_raise(fault))


# --- [OPERATIONS] -----------------------------------------------------------------------
def _fold_raise(fault: object) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    # terminal collapse at the render boundary: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))


def _row(*, name: str, source: bytes, bbox: tuple[float, float, float, float] | None = None, group: str | None = None) -> LayerNode:
    # one engine row into the graphic/layer vocabulary — a group name nests as the LayerPlan path prefix, z rides row order.
    return LayerNode(name=name if group is None else f"{group}/{name}", intent=LayerIntent.ANNOTATION, content=Some(LayerContent(fragment=source)))


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
        case _:
            return {}


def _dim_text(tol: DimTol, /) -> str:
    match tol:  # the measurement text; deviation stacks the upper/lower band onto the `<>` measurement
        case DimTol(tag="custom", custom=text):
            return text
        case DimTol(tag="basic", basic=value):
            return f"{value:g}"
        case DimTol(tag="deviation", deviation=(upper, lower)):
            return f"<>{MTextEditor().stack(f'+{upper:g}', f'{lower:g}', '^').text}"
        case _:
            return "<>"


def _shifted(base: Point, p1: Point, p2: Point, offset: float | None, /) -> Point:
    # shift the dimension line perpendicular to the measured direction by the solved stack offset, the normal derived once through numpy.
    if offset is None:
        return base
    direction = np.asarray(p2) - np.asarray(p1)
    normal = np.array([-direction[1], direction[0]])
    unit = normal / (float(np.hypot(*normal)) or 1.0)
    shifted = np.asarray(base) + unit * offset
    return (float(shifted[0]), float(shifted[1]))


def _stack(dim: Dimension, /) -> Map[int, float]:
    # one kiwisolver.Solver over the stackable (linear/aligned/baseline) offsets: a required anchor, a required DIMDLI
    # min-separation, and a custom strength.create equal-gap band above plain weak. Constraint.violated() reads which
    # soft gaps the solve sacrificed; a dense chain that collapses the distribution falls back to fixed DIMDLI stepping.
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
    for (lower, upper), soft in zip(
        pairwise(lanes), even, strict=True
    ):  # Exemption: kiwisolver.Solver is the stateful native sink; constraints add in place
        solver.addConstraint(offsets[upper] - offsets[lower] >= step)
        solver.addConstraint(soft)
    solver.updateVariables()
    if sum(constraint.violated() for constraint in even) * 2 > len(even):
        return Map.of_seq((index, step * (rank + 1)) for rank, index in enumerate(lanes))
    return Map.of_seq((index, offsets[index].value()) for index in lanes)


def _lower(msp: "Modelspace", op: DimOp, standard: Standard, offset: float | None, /) -> None:
    # the one total dispatch onto the verified ezdxf ISO 129-1 builder family; every builder but the
    # self-rendering add_multi_point_linear_dim returns a DimStyleOverride whose .render() authors geometry.
    family, tol = _facets(op)
    over = dict(standard.dimstyle(family)) | _tol_over(tol)
    text, style = _dim_text(tol), family.value
    match op:  # Exemption: ezdxf Modelspace is the GraphicsFactory sink; add_*_dim + .render() mutate the layout in place
        case DimOp(tag="linear", linear=(base, p1, p2, angle, _family, _tol)):
            msp.add_linear_dim(base=_shifted(base, p1, p2, offset), p1=p1, p2=p2, angle=angle, text=text, dimstyle=style, override=over).render()
        case DimOp(tag="aligned", aligned=(p1, p2, distance, _family, _tol)):
            msp.add_aligned_dim(p1=p1, p2=p2, distance=distance if offset is None else offset, text=text, dimstyle=style, override=over).render()
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
                    base=_shifted(base, datum, point, (offset or step) + index * step),
                    p1=datum,
                    p2=point,
                    angle=angle,
                    text=text,
                    dimstyle=style,
                    override=over,
                ).render()
        case _ as unreachable:
            assert_never(unreachable)


def _lowered(dim: Dimension, /) -> tuple["Drawing", "Modelspace", int, str]:
    # seed the ISO resources, solve the offset stack once, fold every DimOp onto its builder — the shared native lowering the DXF/SVG/PDF engines egress differently.
    doc = ezdxf.new("R2018", setup=True)
    msp = doc.modelspace()
    families = tuple({_facets(op)[0] for op in dim.ops})
    dim.standard.seed(doc, layers=(LayerName.of(Discipline.GENERAL, "DIMS"),), families=families)
    offsets = _stack(dim)
    for index, op in enumerate(dim.ops):
        _lower(msp, op, dim.standard, offsets.try_find(index).default_value(None))
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
        case _ as unreachable:
            assert_never(unreachable)


def _polar(center: Point, radius: float, angle: float, /) -> Point:
    rad = float(np.deg2rad(angle))
    return (center[0] + radius * float(np.cos(rad)), center[1] + radius * float(np.sin(rad)))


def _angle_of(center: Point, point: Point, /) -> float:
    # the CCW degree angle from `center` to `point` — the inverse of `_polar`, feeding the ConstructionArc span
    return float(np.degrees(np.arctan2(point[1] - center[1], point[0] - center[0])))


def _arc_verts(construction: object, /) -> tuple[Point, ...]:
    # an ezdxf.math ConstructionArc/ConstructionCircle adaptively flattened to a `(x, y)` polyline — materialized once so the same vertices draw the `<path>` AND bound the envelope.
    return tuple((float(v.x), float(v.y)) for v in construction.flattening(_SAGITTA))


def _measured(op: DimOp, /) -> tuple[Segment, Point]:
    # the measured segment and its unit normal through one numpy fold — the perpendicular the offset stack and terminator geometry shift along.
    start, finish = _endpoints(op)
    direction = np.asarray(finish) - np.asarray(start)
    normal = np.array([-direction[1], direction[0]])
    unit = normal / (float(np.hypot(*normal)) or 1.0)
    return (start, finish), (float(unit[0]), float(unit[1]))


def _offset(point: Point, normal: Point, distance: float, /) -> Point:
    return (point[0] + normal[0] * distance, point[1] + normal[1] * distance)


def _bounds(points: Iterable[Point], /) -> Box:
    pts = tuple(points)
    return (min(p[0] for p in pts), min(p[1] for p in pts), max(p[0] for p in pts), max(p[1] for p in pts)) if pts else (0.0, 0.0, 0.0, 0.0)


def _union(left: Box, right: Box, /) -> Box:
    return (min(left[0], right[0]), min(left[1], right[1]), max(left[2], right[2]), max(left[3], right[3]))


def _scene_box(dim: Dimension, /) -> Box:
    return _bounds(tuple(point for op in dim.ops for point in _endpoints(op)))


def _text_anchor(op: DimOp, /) -> Point:
    (start, finish), _ = _measured(op)
    return ((start[0] + finish[0]) / 2.0, (start[1] + finish[1]) / 2.0)


def _annotation_text(op: DimOp, tol: DimTol, /) -> str:
    match tol:  # an explicit override wins; else the numpy-measured value (angular auto is the native render's)
        case DimTol(tag="custom", custom=text):
            return text
        case DimTol(tag="basic", basic=value):
            return f"{value:g}"
        case _:
            return _measurement(op)


def _measurement(op: DimOp, /) -> str:
    # the layered auto value read once from the anchor geometry (a scalar, not a re-render); the angular auto angle stays the native render's.
    match op:
        case DimOp(tag="radius", radius=(_center, radius, *_)):
            return f"R{radius:g}"
        case DimOp(tag="diameter", diameter=(_center, radius, *_)):
            return f"⌀{radius * 2.0:g}"
        case DimOp(tag="angular2l") | DimOp(tag="angular3p") | DimOp(tag="angularcra"):
            return ""
        case _:
            (start, finish), _ = _measured(op)
            return f"{float(np.hypot(finish[0] - start[0], finish[1] - start[1])):g}"


def _polyline(points: Iterable[Point], /) -> bytes:
    # numeric geometry markup — float coordinates only, never untrusted text; every dynamic label rides ziafont/ziamath outline geometry, never a spliced <text>.
    body = " ".join(f"{'M' if i == 0 else 'L'}{float(x):g},{float(y):g}" for i, (x, y) in enumerate(points))
    return f'<path d="{body}" fill="none"/>'.encode()


def _svg(fragments: Iterable[bytes], box: Box, pen: str = "#1a1a1a", width: float = 0.25, /) -> bytes:
    # the layered geometry rides ONE penned group so the lines actually stroke (a bare fill="none" path is invisible);
    # a filled terminator overrides fill per-path, the group pen its stroke fallback. Numeric coordinates + sRGB pen only.
    xmin, ymin, xmax, ymax = box
    body = b"".join(fragments).decode()
    return (
        f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="{xmin:g} {ymin:g} {xmax - xmin:g} {ymax - ymin:g}">'
        f'<g stroke="{pen}" stroke-width="{width:g}" fill="none">{body}</g></svg>'
    ).encode()


def _pen(standard: Standard, /) -> str:
    # the ISO 13567 DIMS-layer discipline pen resolved to sRGB hex through Standard.rgb — the ONE colour the LAYERED geometry, terminator, text, and tolerance share.
    red, green, blue = standard.rgb(LayerName.of(Discipline.GENERAL, "DIMS"))
    return f"#{red:02x}{green:02x}{blue:02x}"


def _canvas() -> Element:
    return Element("{http://www.w3.org/2000/svg}svg")


def _svg_bytes(canvas: Element, /) -> bytes:
    return tostring(canvas)


def _terminator_kind(over: Override, /) -> str:
    # the ISO 129-1 terminator from the override's dimblk/dimtsz — a positive DIMTSZ is the oblique tick, else the arrow block.
    if float(over.get("dimtsz", 0.0)) > 0.0:
        return "oblique"
    return {"OPEN": "open", "DOT": "dot"}.get(str(over.get("dimblk", "")), "arrow")


def _terminator_mark(point: Point, tangent: Point, normal: Point, size: float, width: float, kind: str, pen: str, /) -> bytes:
    # the self-contained ISO 129-1 terminator — filled arrow / oblique tick / dot / open chevron, directly-renderable
    # SVG; a tapered variable-width terminator composes region outline, but the filled triangle default stays here.
    px, py = point
    wing1 = (px - tangent[0] * size + normal[0] * size * 0.3, py - tangent[1] * size + normal[1] * size * 0.3)
    wing2 = (px - tangent[0] * size - normal[0] * size * 0.3, py - tangent[1] * size - normal[1] * size * 0.3)
    match kind:
        case "oblique":  # the 45-degree architectural tick, stroked
            lead = (tangent[0] + normal[0], tangent[1] + normal[1])
            a, b = (px - lead[0] * size * 0.5, py - lead[1] * size * 0.5), (px + lead[0] * size * 0.5, py + lead[1] * size * 0.5)
            return f'<path d="M{a[0]:g},{a[1]:g} L{b[0]:g},{b[1]:g}" stroke="{pen}" stroke-width="{width:g}" fill="none"/>'.encode()
        case "dot":  # the filled terminator dot
            return f'<circle cx="{px:g}" cy="{py:g}" r="{size * 0.25:g}" fill="{pen}"/>'.encode()
        case "open":  # the open 90-degree chevron, stroked
            return f'<path d="M{wing1[0]:g},{wing1[1]:g} L{px:g},{py:g} L{wing2[0]:g},{wing2[1]:g}" stroke="{pen}" stroke-width="{width:g}" fill="none"/>'.encode()
        case _:  # the closed filled arrowhead
            return f'<path d="M{px:g},{py:g} L{wing1[0]:g},{wing1[1]:g} L{wing2[0]:g},{wing2[1]:g} Z" fill="{pen}"/>'.encode()


def _tol_latex(tol: DimTol, /) -> str | None:
    match tol:  # a Limits/Basic tolerance carrying genuine math ziamath typesets; else None (no math layer)
        case DimTol(tag="limits", limits=(upper, lower)):
            return rf"\frac{{{upper:g}}}{{{lower:g}}}"
        case DimTol(tag="basic", basic=value):
            return rf"\boxed{{{value:g}}}"
        case _:
            return None


# --- [TABLES] ---------------------------------------------------------------------------
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
    key = dim._key
    receipt: ArtifactReceipt = (
        ArtifactReceipt.Pdf(key, len(data), 1)
        if backend.kind == "pdf"
        else ArtifactReceipt.Drawing(key, "drawing-dimension", count, dimstyle, round(width), round(height), len(data))
    )
    return (_row(name=f"dimension.{dim.target.value}", source=data, bbox=(0.0, 0.0, width, height)),), receipt


def _construction(op: DimOp, over: Override, /) -> tuple[Block[bytes], Box]:
    # the LAYERED geometry from the ezdxf.math construction kernel — never hand-rolled trig. A CURVED dimension
    # decomposes to the ARC/CIRCLE it MEASURES, not the bare center->edge leader the native render never draws for a radial/angular/arc.
    match op:
        case DimOp(tag="diameter", diameter=(center, radius, angle, *_)):
            edge, far = _polar(center, radius, angle), _polar(center, radius, angle + 180.0)
            verts = _arc_verts(ezmath.ConstructionCircle(center, radius))  # the ⌀ diametric leader + the measured circle
            return Block.of_seq((_polyline(verts), _polyline((far, edge)))), _bounds((*verts, edge, far))
        case DimOp(tag="radius", radius=(center, radius, angle, *_)):
            edge = _polar(center, radius, angle)
            verts = _arc_verts(
                ezmath.ConstructionArc(center, radius, angle - _ARC_SPAN, angle + _ARC_SPAN)
            )  # the R leader + the reference arc it measures
            return Block.of_seq((_polyline(verts), _polyline((center, edge)))), _bounds((*verts, center, edge))
        case DimOp(tag="angular3p", angular3p=(_base, center, p1, p2, *_)) | DimOp(tag="arc3p", arc3p=(_base, center, p1, p2, *_)):
            radius = float(np.hypot(p1[0] - center[0], p1[1] - center[1]))
            verts = _arc_verts(
                ezmath.ConstructionArc(center, radius, _angle_of(center, p1), _angle_of(center, p2))
            )  # the two legs + the measured arc
            return Block.of_seq((_polyline(verts), _polyline((center, p1)), _polyline((center, p2)))), _bounds((*verts, center, p1, p2))
        case DimOp(tag="angularcra", angularcra=(center, radius, start, end, *_)) | DimOp(tag="arccra", arccra=(center, radius, start, end, *_)):
            lo_pt, hi_pt = _polar(center, radius, start), _polar(center, radius, end)
            verts = _arc_verts(ezmath.ConstructionArc(center, radius, start, end))
            return Block.of_seq((_polyline(verts), _polyline((center, lo_pt)), _polyline((center, hi_pt)))), _bounds((*verts, center, lo_pt, hi_pt))
        case DimOp(tag="angular2l", angular2l=(_base, line1, line2, *_)):
            vertex = ezmath.ConstructionLine(line1[0], line1[1]).intersect(ezmath.ConstructionLine(line2[0], line2[1]))
            apex = (
                (float(vertex.x), float(vertex.y)) if vertex is not None else ((line1[1][0] + line2[1][0]) / 2.0, (line1[1][1] + line2[1][1]) / 2.0)
            )
            radius = float(np.hypot(line1[1][0] - apex[0], line1[1][1] - apex[1]))
            verts = _arc_verts(ezmath.ConstructionArc(apex, radius, _angle_of(apex, line1[1]), _angle_of(apex, line2[1])))
            return Block.of_seq((_polyline(verts), _polyline((apex, line1[1])), _polyline((apex, line2[1])))), _bounds((
                *verts,
                apex,
                line1[1],
                line2[1],
            ))
        case _:
            ext = float(over.get("dimexe", 1.25))
            (start, finish), normal = _measured(op)
            lo, hi = _offset(start, normal, ext), _offset(finish, normal, ext)
            dim_line = ezmath.ConstructionLine(lo, hi)
            witness = (_polyline((start, lo)), _polyline((finish, hi)))
            return Block.of_seq((_polyline((dim_line.start, dim_line.end)), *witness)), _bounds((start, finish, lo, hi))


def _terminator_layer(dim: Dimension, pen: str, /) -> Layer:
    # each terminator drawn as a self-contained filled/stroked ISO 129-1 mark from the resolved dimblk/dimtsz kind —
    # never a hand-tessellated arrowhead; a tapered variable-width terminator composes region outline.
    fragments: Block[bytes] = Block.empty()
    for op in dim.ops:
        over = dim.standard.dimstyle(_facets(op)[0])
        (start, finish), normal = _measured(op)
        tangent = (-normal[1], normal[0])
        size, width = float(over.get("dimasz", 2.5)), max(float(over.get("dimtxt", 2.5)) * 0.1, 0.13)
        kind = _terminator_kind(over)
        fragments = fragments.append(Block.of_seq(_terminator_mark(point, tangent, normal, size, width, kind, pen) for point in (start, finish)))
    box = _scene_box(dim)
    return _row(name="dimension-terminator", source=_svg(tuple(fragments), box, pen), bbox=box, group="dimension")


def _annotation_layer(dim: Dimension, pen: str, /) -> Layer:
    # the ISO 3098 measurement text outlined to font-independent <path> through ziafont so the placed dimension
    # survives without the CAD font. MEASURED: getsize() centres it on the anchor, getyofst() lifts its baseline so the text floats ABOVE the dimension line.
    font, canvas = ziafont.Font(dim.standard.font.default_value(None)), _canvas()
    for op in dim.ops:
        family, tol = _facets(op)
        anchor = _text_anchor(op)
        if label := _annotation_text(op, tol):
            run = font.text(label, size=float(dim.standard.dimstyle(family).get("dimtxt", 2.5)), valign="base", color=pen)
            width, _height = run.getsize()
            run.drawon(canvas, anchor[0] - width / 2.0, anchor[1] - run.getyofst())
    return _row(name="dimension-text", source=_svg_bytes(canvas), bbox=_scene_box(dim), group="dimension")


def _tolerance_layer(dim: Dimension, pen: str, /) -> Layer:
    # a Limits/Basic expression typeset through ziamath.Latex, seated at the math axis (valign='axis') — the true
    # OpenType-MATH stack a plain <text> cannot express. getsize() MEASURES the extent so the tolerance clears the value to the right, never colliding with it.
    canvas = _canvas()
    for op in dim.ops:
        family, tol = _facets(op)
        anchor = _text_anchor(op)
        if (expr := _tol_latex(tol)) is not None:
            text_h = float(dim.standard.dimstyle(family).get("dimtxt", 2.5))
            run = ziamath.Latex(expr, size=text_h, color=pen)
            width, _height = run.getsize()
            run.drawon(canvas, anchor[0] + text_h * 2.0 + width / 2.0, anchor[1], halign="center", valign="axis")
    return _row(name="dimension-tolerance", source=_svg_bytes(canvas), bbox=_scene_box(dim), group="dimension")


def _layered(dim: Dimension, /) -> tuple[tuple[LayerNode, ...], ArtifactReceipt]:
    # decompose each dimension into named editable layers over the component authors — the geometry layer buckets every op's construction lines, terminator/text/tolerance ride their own owners.
    ziafont.config.precision = ziamath.config.precision = _PRECISION  # once inside the offload lane — deterministic d-floats -> stable content key
    geometry, pen = Block.empty(), _pen(dim.standard)
    envelope = _scene_box(dim)
    for op in dim.ops:
        fragments, box = _construction(op, dim.standard.dimstyle(_facets(op)[0]))
        geometry = geometry.append(fragments)
        envelope = _union(envelope, box)
    layers = (
        _row(name="dimension-line", source=_svg(tuple(geometry), envelope, pen), bbox=envelope, group="dimension"),
        _terminator_layer(dim, pen),
        _annotation_layer(dim, pen),
        _tolerance_layer(dim, pen),
    )
    key = dim._key
    facts = ArtifactReceipt.Drawing(
        key,
        "drawing-dimension",
        len(dim.ops),
        "ISO-129",
        round(envelope[2] - envelope[0]),
        round(envelope[3] - envelope[1]),
        sum(len(layer.source) for layer in layers),
    )
    return layers, facts


_ENGINES: frozendict[DimTarget, DimArm] = frozendict({
    DimTarget.DXF: _native,
    DimTarget.SVG: _native,
    DimTarget.PDF: _native,
    DimTarget.LAYERED: _layered,
})

# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = ["Dimension", "DimOp", "DimTarget", "DimTol", "OrdinateAxis"]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
