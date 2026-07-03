# [PY_ARTIFACTS_DRAWING_DIMENSION]

The ISO 129-1 dimensioning producer: the closed family of drafting dimensions positioned as annotation geometry onto a drawing, lowered onto the categorical-best `ezdxf` `add_*_dim` renderer rather than hand-emitted extension-line/terminator/text trigonometry. `Dimension` is ONE owner over a closed `DimOp` `expression.tagged_union` — `Linear`/`Aligned`/`Angular2L`/`Angular3P`/`AngularCRA`/`Radius`/`Diameter`/`Ordinate`/`Arc3P`/`ArcCRA`/`Chain`/`Baseline` — one case per ISO 129-1 kind and construction form, each carrying ONLY its own typed geometry plus the `drawing/standard#DIMSTYLE` `DimStyleFamily` selector and a `DimTol` tolerance annotation. Every dimension lowers onto its verified `ezdxf` builder (`msp.add_linear_dim(...).render()`, `add_angular_dim_2l/3p/cra`, `add_radius_dim`, `add_diameter_dim`, `add_ordinate_x_dim`/`add_ordinate_y_dim`, `add_arc_dim_3p/cra`, the self-rendering `add_multi_point_linear_dim` for a chain), each threaded with the `override=` DIM-variable dict `drawing/standard#DIMSTYLE`'s `Standard.dimstyle(family)` derives, so the extension lines, dimension line, native ISO terminators, and measurement text are `ezdxf`-authored ISO 129-1 geometry and a 2-of-8 linear slice where the full family is the design claim is the deleted naive form.

The vocabulary is closed and total: a new drafting dimension is one `DimOp` case plus one builder arm, never a per-dimension class family and never an erased attribute `dict`. Each dimension DUAL-lowers over the `DimTarget` policy value — the `ezdxf`-native path (`DXF` the `Drawing.write` CAD blob, `SVG` the `SVGBackend.get_string` render, `PDF` the `PyMuPdfBackend.get_pdf_bytes` render) LEADS with `add_*_dim().render()` and the ISO tolerance as native DIM-variables (`dimtol`/`dimtp`/`dimtm` symmetric/deviation, `dimlim` limits, `ezdxf.tools.text.MTextEditor.stack` the stacked-deviation MText), while the `LAYERED` path DECOMPOSES each dimension into named editable `export/layered#LAYERED` `Layer` rows — the extension/dimension-line geometry from `ezdxf.math.ConstructionLine`/`ConstructionArc`/`ConstructionCircle` anchor math (never hand-rolled trig) penned by the discipline sRGB `Standard.rgb` resolves, the ISO 129-1 terminator family as self-contained filled/stroked marks (a filled arrow triangle, a stroked oblique tick, a filled dot, an open chevron) the correct ISO 129-1 default — the tapered variable-width terminator stroke-to-outline composing the LANDED `graphic/vector#VECTOR` `outline` (its public `outline(source, width, cap, join, miter, dash)` + `VectorOp.Outline` case, now present on that owner) only for a non-default tapered terminator, the self-contained-default stance the finalized `drawing/symbol#SYMBOL`/`drawing/annotate#ANNOTATE` siblings take — the ISO 3098 measurement text outlined to font-independent `<path>` through `ziafont` (`typography/shape#SHAPE` owns the shaped run), and a tolerance/limit expression carrying genuine math typeset through `ziamath.Latex` seated against the dimension line at `valign='axis'`. `kiwisolver` `Solver` + `strength` bands solve the dimension-line offset STACK (DIMDLI baseline spacing, no-overlap, equal chain distribution) that fixed offsets get wrong. The synchronous `ezdxf`/`kiwisolver`/`ziamath`/`ziafont` render offloads off the event loop through one `CapacityLimiter`-bounded `anyio.to_thread.run_sync`, and every provider raise crosses ONE `async_boundary(catch=_FAULTS)` seam into the runtime `BoundaryFault` rail — no parallel `DimFault` `Literal` the boundary never reads (the settled `composition/sheet#SHEET` producer model, not the double-rail defect). `Dimension` mints no IFC (that stays `csharp:Rasm.Bim` — the drawing plane is a documentation projection, never the semantic model), computes no sheet placement (the dimensioned SVG/PDF bytes feed `composition/sheet#SHEET`'s `FigurePlacement` drawing region as a bytes seam), and re-renders nothing.

## [01]-[INDEX]

- [01]-[DIMENSION]: the `Dimension` owner over the closed `DimOp` `expression.tagged_union` (`Linear`/`Aligned`/`Angular2L`/`Angular3P`/`AngularCRA`/`Radius`/`Diameter`/`Ordinate`/`Arc3P`/`ArcCRA`/`Chain`/`Baseline`) dual-lowering each dimension over the `DimTarget` policy value into the `ezdxf`-native render (`add_*_dim().render()` + `SVGBackend`/`PyMuPdfBackend`/`Drawing.write`, ISO tolerance as `dimtol`/`dimlim`/`MTextEditor.stack` DIM-variables) or the `LAYERED` decomposition (`ezdxf.math.Construction*` geometry + self-contained filled/stroked ISO 129-1 terminator marks + `ziafont` ISO 3098 text + `ziamath.Latex` tolerance math, all penned by the discipline sRGB, into named `export/layered#LAYERED` `Layer` rows), the `override=` DIM-variable dict threaded from `drawing/standard#DIMSTYLE` `Standard.dimstyle(family)`, the dimension-line offset stack solved by one `kiwisolver.Solver` + `strength`, the `DimTol` closed tolerance family (`Auto`/`Custom`/`Symmetric`/`Deviation`/`Limits`/`Basic`), the whole `ezdxf`/`kiwisolver`/`ziamath`/`ziafont` render offloaded via `anyio.to_thread` and railed through one `async_boundary(catch=_FAULTS)`, contributing one `core/receipt#RECEIPT` `ArtifactReceipt.Drawing` case (or reused `ArtifactReceipt.Pdf` on the `PDF` backend) and one `core/plan#PLAN` `ArtifactWork` node keyed by the content identity.

## [02]-[DIMENSION]

- Owner: `Dimension` the one ISO 129-1 dimensioning owner holding `ops: tuple[DimOp, ...]`, the resolved `drawing/standard#STANDARD` `Standard` profile, and the `DimTarget` egress policy value, discriminating operation over the closed `DimOp` `expression.tagged_union` whose every case carries ONLY its own typed geometry plus the `DimStyleFamily` and `DimTol` facet slots — never a per-dimension `LinearDim`/`RadialDim` class family, never a monolithic bag whose angular/radial fields are irrelevant for most cases, and never a `StrEnum` keyed against an erased `dict[str, object]`. `DimTol` is the closed tolerance vocabulary (`Auto` the raw `<>` measurement, `Custom` an explicit string, `Symmetric` a ± band, `Deviation` an upper/lower pair, `Limits` a stacked max/min, `Basic` a boxed theoretically-exact value) every case's second facet slot carries, so a toleranced dimension is one facet not a parallel dimension type. `DimTarget` is the closed `StrEnum` (`DXF`/`SVG`/`PDF`/`LAYERED`) keying the `_ENGINES` `frozendict[DimTarget, DimArm]` dual-lowering table (the `DXF`/`SVG`/`PDF` rows the shared `_native` arm, egressed through the `_BACKENDS` `DimBackend` per-target byte emitter, `LAYERED` the `_layered` arm) so a new egress is one row, never a per-target `Dimension` subtype. `ezdxf` owns the ISO 129-1 dimension entity and its native render (the `add_*_dim` builder family each returning a `DimStyleOverride` whose `.render()` authors the extension lines / dimension line / terminators / measurement text, the `DimStyleOverride` `override=` DIM-variable surface, the `ConstructionLine`/`ConstructionArc`/`ConstructionCircle` anchor geometry, the `MTextEditor.stack` stacked-tolerance MText, and the `Frontend`/`SVGBackend`/`PyMuPdfBackend` render frontend); `drawing/standard#DIMSTYLE` owns the ISO 129-1 DIM-variable derivation (`Standard.dimstyle(family)` scaling the family base parameters by the ISO 5455 factor) and the resource seeding (`Standard.seed(doc, layers, families)`) and the discipline pen (`Standard.rgb` the sRGB the `LAYERED` components carry); the ISO 129-1 terminator marks are self-contained filled/stroked SVG (a filled arrow triangle, a stroked oblique tick, a filled dot, an open chevron) the correct ISO default, the tapered variable-width stroke-to-outline composing the landed `graphic/vector#VECTOR` `outline`/`VectorOp.Outline`; `ziafont` (composed via `typography/shape#SHAPE`) owns the ISO 3098 text outline; `ziamath` owns the tolerance-math typesetting; `kiwisolver` owns the dimension-line offset constraint solve. No IFC, sheet-placement, or annotation-leader logic crosses this owner — those are `csharp:Rasm.Bim`, `composition/sheet#SHEET`, and `drawing/annotate#ANNOTATE`.
- Cases: `DimOp` cases (each ending in the `DimStyleFamily`, `DimTol` facet pair) — `Linear(base, p1, p2, angle, family, tol)` (the ISO 129-1 linear dimension between two points measured at `angle`, lowered onto `add_linear_dim(base, p1, p2, angle=, text=, dimstyle=, override=).render()`, the `base` perpendicular-shifted by the solved stack offset) · `Aligned(p1, p2, distance, family, tol)` (the aligned dimension parallel to the feature at `distance`, `add_aligned_dim`) · `Angular2L(base, line1, line2, family, tol)` (the angle between two lines, `add_angular_dim_2l`) · `Angular3P(base, center, p1, p2, family, tol)` (the angle at a vertex over three points, `add_angular_dim_3p`) · `AngularCRA(center, radius, start, end, distance, family, tol)` (the angle by center/radius/start-end, `add_angular_dim_cra`) · `Radius(center, radius, angle, family, tol)` (the radial dimension with the `R` prefix, `add_radius_dim`) · `Diameter(center, radius, angle, family, tol)` (the diametric dimension with the `⌀` prefix, `add_diameter_dim`) · `Ordinate(feature, offset, axis, origin, family, tol)` (the ordinate dimension from a datum `origin`, the `OrdinateAxis` policy value routing `add_ordinate_x_dim`/`add_ordinate_y_dim`) · `Arc3P(base, center, p1, p2, family, tol)` (the arc-length dimension over three points, `add_arc_dim_3p`) · `ArcCRA(center, radius, start, end, distance, family, tol)` (the arc-length by center/radius/angles, `add_arc_dim_cra`) · `Chain(base, points, angle, family, tol)` (the running continuous chain, the self-rendering `add_multi_point_linear_dim(base, points, angle=)`) · `Baseline(base, datum, points, family, angle, tol)` (the baseline set all measured from one `datum`, a fold of `add_linear_dim` stepping each dimension line by the solved DIMDLI stack offset) — matched by one total `match`/`case` over `tag` in the `_lower` fold, never a per-kind special case, the shared `Angular3P`/`Arc3P` and `AngularCRA`/`ArcCRA` payload shapes coincidental (each lowers onto a distinct `ezdxf` builder).
- Entry: `Dimension.over` is the one modal-arity entrypoint normalizing `DimOp | Iterable[DimOp]` into the `ops` tuple by a structural `match` at the head (a lone dimension the singleton case, a mixed set the multi-element case), never a `batch` knob or a per-kind sibling; `render` is `async` over the runtime `async_boundary`, returns a `RuntimeRail[tuple[tuple[Layer, ...], ArtifactReceipt]]`, and offloads the whole synchronous fold onto `to_thread.run_sync(_ENGINES[self.target], self, limiter=_LANES)` — the shared-address-space thread arm (the `ezdxf`/`kiwisolver`/`ziamath`/`ziafont` render touches the `numpy` anchor math and returns the `msgspec`-backed `Layer`/`ArtifactReceipt` owners a `to_interpreter` isolate cannot load, the same lane the `drawing/symbol#SYMBOL` sibling takes), the boundary minted ONCE with `catch=_FAULTS` (the stdlib `ValueError`/`KeyError`/`TypeError`/`OSError` bases the `ezdxf` `DXFValueError`/`DXFKeyError`/`DXFTypeError` render raises derive from, named without reifying the lazy `ezdxf` import) so each provider raise crosses into the runtime `BoundaryFault` rail rather than a parallel `DimFault` `Literal` the boundary never reads. The `_native` arm seeds the `Standard` resources, solves the offset stack once, folds each `DimOp` through `_lower` onto its `add_*_dim(...).render()`, and egresses through the target's `DimBackend` (`Drawing.write` DXF bytes, `SVGBackend.get_string` SVG, `PyMuPdfBackend.get_pdf_bytes` PDF); the `_layered` arm decomposes each dimension into named `Layer` rows over the `ezdxf.math`/`graphic/vector#VECTOR`/`ziafont`/`ziamath` component authors, deriving the content key over the joined layer bytes.
- Auto: `_facets(op)` is one total or-pattern projecting each case's `(family, tol)` tail once, never a per-tag `getattr`; `_lower(msp, op, standard, offset)` reads the facets, derives `over = dict(standard.dimstyle(family)) | _tol_over(tol)` and `text = _dim_text(tol)`, then matches the op to its verified `ezdxf` builder — the `override=` dict is the `drawing/standard#DIMSTYLE` DIM-variable derivation scaled by the ISO 5455 factor merged with the per-op tolerance variables, so a `1:50` architectural dimension draws its 2.5 mm text and oblique tick at paper scale with zero per-arm literal. The ISO 129-1 tolerance lowers by mode: `Symmetric(pm)` onto `{"dimtol": 1, "dimtp": pm, "dimtm": pm}`, `Deviation(u, l)` onto a `MTextEditor().stack(f"+{u}", f"{l}", "^")` stacked-fraction MText appended to the `<>` measurement, `Limits(u, l)` onto `{"dimlim": 1, "dimtp": u, "dimtm": l}` (ezdxf computes the limit values), `Basic(v)` onto the `{"dimgap": <negative>}` boxed-text convention — the three distinct native mechanisms, never a hand-formatted `± "` string. `_stack(dim)` threads one `kiwisolver.Solver`: each stackable dimension's offset is a `Variable` keyed on the op index, a `required` constraint pins the first at its base offset, `required` min-separation constraints keep consecutive dimension lines ≥ DIMDLI apart, and a custom `strength.create(0, 1, 0, 4)` equal-gap band (a medium-×4 priority above plain `weak`, below `required`) distributes a chain evenly, `updateVariables()` writes each solved `value()` back, and `_shifted` applies each offset perpendicular to the measured direction through one `numpy` normal so a baseline set and a bank of parallel linear dimensions stack without overlap; `Constraint.violated()` reads back which soft gaps the solve sacrificed, and when a dense chain collapses the distribution (a majority sacrificed) deterministic fixed DIMDLI stepping replaces the bunched solve — the `strength` band separating the hard no-overlap snap from the aesthetic even distribution. The `_layered` arm composes the ISO 129-1 components under one `_pen`-resolved discipline sRGB (the DIMS-layer `Standard.rgb`): `_construction` DECOMPOSES each dimension over the `ezdxf.math` construction kernel into the one penned `_svg` group so the strokeless lines actually render: a linear dimension the extension/dimension `ConstructionLine` over the DIMEXE/DIMEXO offsets, and a CURVED dimension the actual `ConstructionArc`/`ConstructionCircle` it MEASURES via `.flattening(_SAGITTA)` — a radial the R leader plus its reference arc, a diameter the ⌀ leader plus the measured circle, an angular/arc the extension legs plus the true measured arc (`_angle_of` deriving the span, a `ConstructionLine.intersect` apex for the 2-line form) — never the bare center->edge leader line the native render would never draw for a radial/angular/arc dimension, `_terminator_layer` draws each terminator as a self-contained filled/stroked ISO 129-1 mark per the resolved `dimblk`/`dimtsz` kind (`_terminator_kind` → a filled arrow triangle, a stroked oblique tick, a filled dot, or an open chevron — the correct ISO 129-1 default, the tapered variable-width stroke-to-outline composing the LANDED `graphic/vector#VECTOR` `outline` public function for a non-default terminator, never a phantom foreign call), `_annotation_layer` outlines the ISO 3098 measurement text through `ziafont.Font(...).text(..., color=pen)` MEASURED via `getsize()` (horizontally centred on the anchor) and `getyofst()` (baseline lifted above the dimension line) then `.drawon(...)` to font-independent `<path>` geometry, and `_tolerance_layer` typesets a `Limits`/`Basic` expression through `ziamath.Latex(expr, color=pen)` MEASURED via `getsize()` (offset to the right of the value so the stacks never collide) then `.drawon(svg, x, y, valign="axis")` seated at the math axis (and `_layered` pins `ziafont.config.precision`/`ziamath.config.precision` once so the outlined `d`-floats and the content key over them are deterministic) — each a named `Layer` row `export/layered#LAYERED` binds, so the placed dimension survives on a sheet without the CAD font, editable and layer-separated.
- Growth: a new ISO 129-1 dimension kind or construction form is one `DimOp` case plus one builder arm in `_lower` — the twelve builders cover linear/aligned/angular/radial/diameter/ordinate/arc/chain/baseline, so a new form (a jogged radius, a folded ordinate) is one case-plus-arm not a class; a new egress is one `DimTarget` member plus one `_ENGINES` row (and one `_BACKEND` row for a native backend); a new tolerance presentation (a geometric datum reference, a projected-tolerance zone within ISO 129-1) is one `DimTol` case plus one `_tol_over`/`_dim_text` arm; a new DIM-variable axis is one key on the `Standard.dimstyle` derivation `drawing/standard#DIMSTYLE` owns; a new stacking rule (radial distribution, symmetric mirroring) is one `kiwisolver` constraint at its `strength` band; a new `LAYERED` component author is one `_layered` layer function over the existing `ezdxf.math`/vector/text owners; a new receipt fact is one scalar the `ArtifactReceipt.Drawing` case already carries; zero new surface for a new dimension or a new layer.
- Boundary: the deleted forms are a per-dimension `LinearDim`/`RadialDim` class family where one closed `DimOp` union states them; a hand-emitted extension line, arrowhead, or text-placement `<path d>` where `add_*_dim().render()` authors the ISO 129-1 geometry and `ezdxf.math.Construction*` owns the `LAYERED` anchor math; a monolithic dimension bag whose angular/radial/ordinate fields are irrelevant for most cases where the per-mode `DimOp` payloads carry only their own geometry; a hand-formatted `± 0.05` tolerance string where `dimtol`/`dimlim`/`MTextEditor.stack` are the native mechanisms; a fixed per-dimension offset literal where the `kiwisolver` stack solve distributes the dimension lines; a `drawsvg.Text`/CAD-font `<text>` on the `LAYERED` path where `ziafont` outlines the ISO 3098 text to a self-contained `<path>`; a hand-typeset fraction where `ziamath.Latex` typesets the tolerance math; a phantom `Vector.over(ops)._worked(ops)` private-method reach (`_worked` is a private module fold on `graphic/vector`, never a `Vector` method) where the self-contained filled arrow triangle / oblique tick / dot / open chevron draws the ISO 129-1 terminator directly, penned by the discipline sRGB — the tapered variable-width form composing the LANDED `graphic/vector#VECTOR` `outline`/`VectorOp.Outline` (its current `VectorOp` family is `transform`/`bounds`/`fit`/`serialize`/`rasterize`/`measure`/`sample`/`flatten`/`subpaths`/`project`/`boolean`/`outline`/`region`) for the non-default premium; a strokeless `fill="none"` construction line where the one `_svg` penned group makes the geometry visible; a `batch`/`mode` knob where `DimTarget` and the modal `over` head discriminate; a parallel `DimFault` `Literal` the boundary never reads where `async_boundary(catch=_FAULTS)` converts each provider raise into the runtime `BoundaryFault` (the settled `composition/sheet#SHEET` rail, not the double-rail defect); a synchronous render on the event loop where `to_thread.run_sync` offloads it; a phantom sheet-placement compute where the dimensioned SVG/PDF bytes feed `composition/sheet#SHEET`'s `FigurePlacement`; a parallel drawing receipt where the shared `ArtifactReceipt.Drawing` case carries the dimension/dimstyle/extent facts. `ezdxf` owns the ISO 129-1 dimension entity and render, `drawing/standard#DIMSTYLE` the DIM-variable derivation and the discipline pen, `graphic/vector#VECTOR` the landed `outline`/`boolean` the tapered-terminator premium composes, `ziafont` the ISO 3098 text outline, `ziamath` the tolerance math, `kiwisolver` the offset solve, `export/layered#LAYERED` the layer binding, `composition/sheet#SHEET` the sheet placement, and `csharp:Rasm.Bim` the IFC semantics; identity minting is the runtime's.
- Packages: `ezdxf` (`new`/`Drawing.write`/`Drawing.modelspace`/`doc.dimstyles`, the `add_linear_dim`/`add_aligned_dim`/`add_angular_dim_2l`/`add_angular_dim_3p`/`add_angular_dim_cra`/`add_radius_dim`/`add_diameter_dim`/`add_ordinate_x_dim`/`add_ordinate_y_dim`/`add_arc_dim_3p`/`add_arc_dim_cra`/`add_multi_point_linear_dim` dimension family each returning a `DimStyleOverride` whose `.render()` generates geometry, `math.ConstructionLine`/`ConstructionLine.intersect`/`ConstructionArc`/`ConstructionCircle`/`.flattening` the anchor + measured-arc/circle geometry, `tools.text.MTextEditor.stack` the stacked-tolerance MText, `addons.drawing.Frontend`/`RenderContext`, `addons.drawing.svg.SVGBackend.get_string`, `addons.drawing.pymupdf.PyMuPdfBackend.get_pdf_bytes`, `addons.drawing.layout.Page`/`Settings`/`Margins`/`Units` the render page, `bbox.extents` the model extents, `colors.aci2rgb` the discipline pen `Standard.rgb` resolves); `graphic/vector#VECTOR` (a bare owner pointer — the landed `outline`/`VectorOp.Outline` the tapered-terminator premium composes, NOT imported for the base case where the self-contained filled ISO 129-1 terminator marks are the correct default); `kiwisolver` (`Solver`/`Variable`/`strength`/`strength.create` the dimension-line offset stack with a custom equal-gap band, `Constraint.violated` the post-solve overlap QA gating the fixed-stepping fallback); `ziamath` (`Latex`/`.svg`/`.drawon`/`.getsize`/`config.precision` the tolerance-math typesetting seated at `valign='axis'`, measured so the stacks never collide); `ziafont` (`Font.text`/`Text.drawon`/`Text.getsize`/`Text.getyofst`/`config.precision` the ISO 3098 text outline, measured, centred, baseline-seated, content-key-deterministic); `numpy` (`np.asarray`/`np.hypot` the perpendicular offset normal); `expression` (`tagged_union`/`tag`/`case`/`Block`/`Map` the vocabulary and folds); `msgspec` (`Struct(frozen=True)` the value objects); `beartype` (`@beartype` the `over` construction contract); `anyio` (`CapacityLimiter`/`to_thread` the offload); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`); `core/receipt#RECEIPT` (`ArtifactReceipt.Drawing`/`ArtifactReceipt.Pdf`); `export/layered#LAYERED` (`Layer`). The `drawing/standard#DIMSTYLE`/`#STANDARD` owned-vocabulary substrate is composed as bare owner pointers, its `DimStyleFamily`/`Standard.dimstyle`/`Standard.seed`/`LayerName` lowering onto the `ezdxf` tables.

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
from anyio import CapacityLimiter, to_thread
from beartype import beartype
from builtins import frozendict
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.drawing.standard import DimStyleFamily, Discipline, LayerName, Standard
from artifacts.export.layered import Layer

# each proxy reifies on first render-arm use in the `to_thread` worker
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
type DimArm = Callable[["Dimension"], tuple[tuple[Layer, ...], ArtifactReceipt]]  # the target-keyed lowering arm

_LANES: CapacityLimiter = CapacityLimiter(os.process_cpu_count() or 4)
# the stdlib bases the ezdxf `DXFValueError`/`DXFKeyError`/`DXFTypeError` render raises and the font/resource
# faults derive from — named without reifying the lazy `ezdxf` import at module scope (the settled
# composition/sheet#SHEET `_FAULTS` pattern); the runtime `async_boundary` classifies each into `BoundaryFault`,
# the closed fault vocabulary this seam, never a parallel `DimFault` Literal the boundary never reads.
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
    # the ISO 129-1 tolerance/annotation family every DimOp facet carries; lowered by mode to the
    # native dimtol/dimlim/MTextEditor mechanisms, never a hand-formatted `± value` string.
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

    async def render(self) -> RuntimeRail[tuple[tuple[Layer, ...], ArtifactReceipt]]:
        return await async_boundary(f"drawing.dimension.{self.target}", self._compute, catch=_FAULTS)

    async def _compute(self) -> tuple[tuple[Layer, ...], ArtifactReceipt]:
        # the whole ezdxf/kiwisolver/ziamath/ziafont fold is synchronous native/CPU work, so it crosses one
        # `to_thread` seam off the event loop in the shared address space (the subinterpreter arm cannot load
        # the numpy anchor math / msgspec receipt owners), never inline on the loop.
        return await to_thread.run_sync(_ENGINES[self.target], self, limiter=_LANES)


# --- [OPERATIONS] -----------------------------------------------------------------------
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
    # shift the dimension-line location perpendicular to the measured direction by the solved stack offset,
    # the normal derived once through numpy so a baseline set / parallel bank does not overlap.
    if offset is None:
        return base
    direction = np.asarray(p2) - np.asarray(p1)
    normal = np.array([-direction[1], direction[0]])
    unit = normal / (float(np.hypot(*normal)) or 1.0)
    shifted = np.asarray(base) + unit * offset
    return (float(shifted[0]), float(shifted[1]))


def _stack(dim: Dimension, /) -> Map[int, float]:
    # one kiwisolver.Solver over the stackable (linear/aligned/baseline) dimension-line offsets: a `required`
    # anchor at the first base offset, a `required` DIMDLI min-separation between consecutive lines, and a custom
    # `strength.create` equal-gap band (a medium-×4 priority above plain `weak`, so even distribution is honored
    # aggressively yet still yields to the hard min-separation the fixed `weak` pair under-weighted). `Constraint`
    # `.violated()` reads back which soft gaps the solve sacrificed; when a dense chain collapses the distribution
    # (a majority sacrificed), deterministic fixed DIMDLI stepping replaces the bunched solve.
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
    # seed the ISO resources, solve the offset stack once, fold every DimOp onto its builder — the shared
    # native lowering the DXF/SVG/PDF engines egress differently and the extents/dimstyle receipt facts read.
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
    # an ezdxf.math ConstructionArc/ConstructionCircle adaptively flattened to a Vec2 polyline of `(x, y)` tuples —
    # materialized once so the same vertices draw the `<path>` AND tightly bound the LAYERED envelope (no circle clip).
    return tuple((float(v.x), float(v.y)) for v in construction.flattening(_SAGITTA))


def _measured(op: DimOp, /) -> tuple[Segment, Point]:
    # the measured segment and its unit normal through one numpy fold — the perpendicular the offset stack and
    # the witness/terminator geometry shift along, never scattered per-arm trig.
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
    # the layered auto value read once from the anchor geometry (a scalar, not a re-render of the ezdxf line);
    # the angular auto angle stays the ezdxf-native render's so no fact is computed twice.
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
    # numeric geometry markup (float coordinates, never untrusted text) — the same <path> form graphic/vector's
    # `path`/`svg` owner emits; every dynamic label rides ziafont/ziamath outline geometry, never a spliced <text>.
    body = " ".join(f"{'M' if i == 0 else 'L'}{float(x):g},{float(y):g}" for i, (x, y) in enumerate(points))
    return f'<path d="{body}" fill="none"/>'.encode()


def _svg(fragments: Iterable[bytes], box: Box, pen: str = "#1a1a1a", width: float = 0.25, /) -> bytes:
    # the layered geometry rides ONE penned group so the extension/dimension lines actually stroke — a bare
    # `fill="none"` path with no stroke is invisible; a filled terminator overrides `fill` per-path, the group
    # pen its stroke fallback. Numeric coordinates + the controlled sRGB pen only, never an untrusted splice.
    xmin, ymin, xmax, ymax = box
    body = b"".join(fragments).decode()
    return (
        f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="{xmin:g} {ymin:g} {xmax - xmin:g} {ymax - ymin:g}">'
        f'<g stroke="{pen}" stroke-width="{width:g}" fill="none">{body}</g></svg>'
    ).encode()


def _pen(standard: Standard, /) -> str:
    # the ISO 13567 DIMS-layer discipline pen resolved to sRGB hex through `Standard.rgb` — the ONE color the
    # LAYERED geometry, terminator, text, and tolerance components share so the editable projection carries the
    # same discipline pen the DXF/SVG native render draws (the `graphic/color/derive#DERIVE` CMYK seam is upstream).
    red, green, blue = standard.rgb(LayerName.of(Discipline.GENERAL, "DIMS"))
    return f"#{red:02x}{green:02x}{blue:02x}"


def _canvas() -> Element:
    return Element("{http://www.w3.org/2000/svg}svg")


def _svg_bytes(canvas: Element, /) -> bytes:
    return tostring(canvas)


def _terminator_kind(over: Override, /) -> str:
    # derive the ISO 129-1 terminator from the drawing/standard#DIMSTYLE override's dimblk/dimtsz, never a
    # second terminator vocabulary — a positive DIMTSZ is the architectural oblique tick, else the arrow block.
    if float(over.get("dimtsz", 0.0)) > 0.0:
        return "oblique"
    return {"OPEN": "open", "DOT": "dot"}.get(str(over.get("dimblk", "")), "arrow")


def _terminator_mark(point: Point, tangent: Point, normal: Point, size: float, width: float, kind: str, pen: str, /) -> bytes:
    # the self-contained ISO 129-1 terminator — a filled arrow triangle, a stroked oblique tick, a filled dot,
    # or an open chevron — directly-renderable SVG, never a strokeless centerline awaiting a foreign outline pass.
    # A variable-width tapered terminator composes the landed `graphic/vector#VECTOR` `outline` public function;
    # a filled triangle is the correct ISO 129-1 filled arrow, so the base terminator stays self-contained here.
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


def _native(dim: Dimension, /) -> tuple[tuple[Layer, ...], ArtifactReceipt]:
    doc, msp, count, dimstyle = _lowered(dim)
    width, height = _extent(msp)
    backend = _BACKENDS[dim.target]
    data = backend.egress(doc, msp, width, height)
    key = ContentIdentity.of(f"drawing-dimension-{dim.target}", data)
    receipt: ArtifactReceipt = (
        ArtifactReceipt.Pdf(key, len(data), 1)
        if backend.kind == "pdf"
        else ArtifactReceipt.Drawing(key, "drawing-dimension", count, dimstyle, round(width), round(height), len(data))
    )
    return (Layer(name=f"dimension.{dim.target.value}", source=data, bbox=(0.0, 0.0, width, height)),), receipt


def _construction(op: DimOp, over: Override, /) -> tuple[Block[bytes], Box]:
    # the LAYERED geometry lifted from the ezdxf.math construction kernel — never hand-rolled trig. A CURVED
    # dimension decomposes to the ARC/CIRCLE it MEASURES (`ConstructionArc`/`ConstructionCircle.flattening(sagitta)`),
    # not the bare center->edge leader line the native render would never draw for a radial/angular/arc dimension —
    # the primary LAYERED capability the prior `ConstructionLine`-only fold under-built.
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
    # each terminator drawn as a self-contained filled/stroked ISO 129-1 mark (filled arrow / oblique tick / dot /
    # open chevron) directly from the resolved dimblk/dimtsz kind — never a hand-tessellated arrowhead and never a
    # phantom foreign stroke-to-outline; a tapered variable-width terminator composes the landed graphic/vector `outline`.
    fragments: Block[bytes] = Block.empty()
    for op in dim.ops:
        over = dim.standard.dimstyle(_facets(op)[0])
        (start, finish), normal = _measured(op)
        tangent = (-normal[1], normal[0])
        size, width = float(over.get("dimasz", 2.5)), max(float(over.get("dimtxt", 2.5)) * 0.1, 0.13)
        kind = _terminator_kind(over)
        fragments = fragments.append(Block.of_seq(_terminator_mark(point, tangent, normal, size, width, kind, pen) for point in (start, finish)))
    box = _scene_box(dim)
    return Layer(name="dimension-terminator", source=_svg(tuple(fragments), box, pen), bbox=box, group="dimension")


def _annotation_layer(dim: Dimension, pen: str, /) -> Layer:
    # the ISO 3098 measurement text outlined to font-independent <path> geometry through ziafont so the placed
    # dimension survives on a sheet without the CAD font — typography/shape#SHAPE owns the full shaped run. Each
    # run is MEASURED: `getsize()` centres it horizontally on the anchor (never the overhanging left-anchor) and
    # `getyofst()` lifts its baseline so the text floats ABOVE the dimension line (ISO 129-1), never dropped on it.
    font, canvas = ziafont.Font(dim.standard.font.default_value(None)), _canvas()
    for op in dim.ops:
        family, tol = _facets(op)
        anchor = _text_anchor(op)
        if label := _annotation_text(op, tol):
            run = font.text(label, size=float(dim.standard.dimstyle(family).get("dimtxt", 2.5)), valign="base", color=pen)
            width, _height = run.getsize()
            run.drawon(canvas, anchor[0] - width / 2.0, anchor[1] - run.getyofst())
    return Layer(name="dimension-text", source=_svg_bytes(canvas), bbox=_scene_box(dim), group="dimension")


def _tolerance_layer(dim: Dimension, pen: str, /) -> Layer:
    # a Limits/Basic expression carrying genuine math typeset through ziamath.Latex, seated at the math axis
    # (valign='axis') against the dimension line — the true OpenType-MATH stack a plain <text> cannot express.
    # `getsize()` MEASURES the typeset extent so the tolerance CENTRES to the RIGHT of the dimension value by a
    # text-height clearance plus half its own width, never colliding with the adjacent measurement text.
    canvas = _canvas()
    for op in dim.ops:
        family, tol = _facets(op)
        anchor = _text_anchor(op)
        if (expr := _tol_latex(tol)) is not None:
            text_h = float(dim.standard.dimstyle(family).get("dimtxt", 2.5))
            run = ziamath.Latex(expr, size=text_h, color=pen)
            width, _height = run.getsize()
            run.drawon(canvas, anchor[0] + text_h * 2.0 + width / 2.0, anchor[1], halign="center", valign="axis")
    return Layer(name="dimension-tolerance", source=_svg_bytes(canvas), bbox=_scene_box(dim), group="dimension")


def _layered(dim: Dimension, /) -> tuple[tuple[Layer, ...], ArtifactReceipt]:
    # decompose each dimension into named editable layers over the component authors; the geometry layer
    # buckets every op's construction lines, and the terminator/text/tolerance layers ride their own owners.
    ziafont.config.precision = ziamath.config.precision = (
        _PRECISION  # set once inside the serialized offload lane — deterministic d-floats -> stable content key
    )
    geometry, pen = Block.empty(), _pen(dim.standard)
    envelope = _scene_box(dim)
    for op in dim.ops:
        fragments, box = _construction(op, dim.standard.dimstyle(_facets(op)[0]))
        geometry = geometry.append(fragments)
        envelope = _union(envelope, box)
    layers = (
        Layer(name="dimension-line", source=_svg(tuple(geometry), envelope, pen), bbox=envelope, group="dimension"),
        _terminator_layer(dim, pen),
        _annotation_layer(dim, pen),
        _tolerance_layer(dim, pen),
    )
    key = ContentIdentity.of("drawing-dimension-layered", b"".join(layer.source for layer in layers))
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

`Dimension` is the one ISO 129-1 dimensioning grammar every drafting dimension is built from: the closed `DimOp` union (`Linear`/`Aligned`/`Angular2L`/`Angular3P`/`AngularCRA`/`Radius`/`Diameter`/`Ordinate`/`Arc3P`/`ArcCRA`/`Chain`/`Baseline`) carries each dimension's typed geometry and its `DimStyleFamily`/`DimTol` facets, and the `DimTarget` policy value selects the dual lowering — the `ezdxf`-native path (`add_*_dim(...).render()` authoring the extension lines, dimension line, ISO terminators, and measurement text, egressed as DXF/SVG/PDF through `Drawing.write`/`SVGBackend.get_string`/`PyMuPdfBackend.get_pdf_bytes`) or the `LAYERED` decomposition (`ezdxf.math.Construction*` geometry, self-contained filled/stroked ISO 129-1 terminator marks, `ziafont` ISO 3098 text outlines, and `ziamath.Latex` tolerance math as named `export/layered#LAYERED` `Layer` rows, all penned by the discipline sRGB `Standard.rgb` resolves). The `override=` DIM-variables are the `drawing/standard#DIMSTYLE` `Standard.dimstyle(family)` derivation scaled by the ISO 5455 factor, the ISO tolerance lowers by mode (`Symmetric` onto `dimtol`, `Deviation` onto an `MTextEditor.stack` band, `Limits` onto `dimlim`, `Basic` onto a boxed value), and one `kiwisolver.Solver` + `strength` distributes the dimension-line offset stack a fixed literal gets wrong. The synchronous render offloads onto `to_thread` off the event loop, every provider raise crosses one `async_boundary(catch=_FAULTS)` seam into the runtime `BoundaryFault` rail, the dimensioned SVG/PDF bytes feed `composition/sheet#SHEET`'s `FigurePlacement` drawing region as a bytes seam, and the owner contributes one `ArtifactReceipt.Drawing` (or reused `ArtifactReceipt.Pdf`) on the shared receipt family and one `core/plan#PLAN` `ArtifactWork` node keyed by the content identity, composites nothing, and re-renders nothing.

## [03]-[RESEARCH]

- [ISO_129_1_FAMILY] [RESOLVED]: the twelve `DimOp` cases span the ISO 129-1 dimensioning vocabulary the brief `[06]` names (linear/aligned/angular/radial/diameter/ordinate/chain/baseline + tolerance) as ONE closed `expression.tagged_union`, each case lowering onto its verified `ezdxf` builder — `add_linear_dim`/`add_aligned_dim`/`add_angular_dim_2l`/`add_angular_dim_3p`/`add_angular_dim_cra`/`add_radius_dim`/`add_diameter_dim`/`add_ordinate_x_dim`/`add_ordinate_y_dim`/`add_arc_dim_3p`/`add_arc_dim_cra`/`add_multi_point_linear_dim`, all reflected present on the installed `ezdxf` 1.4.4 `Modelspace` with each `add_*_dim` returning a `DimStyleOverride` whose `.render()` generates geometry (verified `DimStyleOverride.render(ucs=None, discard=False)`), and `add_multi_point_linear_dim` returning `None` (it self-renders the chain, so the `Chain` arm calls no `.render()`). The angular/arc construction forms (2l/3p/cra) are distinct cases because they are distinct `ezdxf` builders taking distinct geometry, not a permissive bag with a form discriminant — the shared `Angular3P`/`Arc3P` and `AngularCRA`/`ArcCRA` payload shapes are coincidental. Baseline has no single `ezdxf` builder, so the `Baseline` arm folds `add_linear_dim` from one datum stepping each dimension line by the DIMDLI increment — the honest composition, not a phantom `add_baseline_dim`. Justified on DOMAIN (the full ISO 129-1 family the brief legislates) and PACKAGE (every builder reflection-verified on `ezdxf` 1.4.4).
- [DIMSTYLE_OVERRIDE] [RESOLVED]: the `override=` argument every `add_*_dim` accepts (verified `override: Optional[dict]` on every builder signature) is threaded from `drawing/standard#DIMSTYLE` `Standard.dimstyle(family)` — the ISO 129-1 DIM-variable dict (`dimtxt`/`dimasz`/`dimexe`/`dimexo`/`dimgap`/`dimdli`/`dimtsz`/`dimdec`/`dimblk`) scaled by the ISO 5455 `ScaleRatio.factor` the substrate owns — merged with the per-op `_tol_over(tol)`, so a `1:50` architectural dimension draws its text and terminator at paper scale with zero per-arm literal and the drawing plane carries one dimstyle owner. The ISO 129-1 tolerance lowers by three distinct native mechanisms: `Symmetric` onto `dimtol`/`dimtp`/`dimtm`, `Limits` onto `dimlim` (ezdxf computes the limit values from the measurement), and `Deviation` onto an `MTextEditor().stack(upr, lwr, '^')` stacked-fraction MText (verified `MTextEditor.stack(upr, lwr, t='^')`) appended to the `<>` measurement — never a hand-formatted tolerance string. Justified on PACKAGE (the `override=`/`MTextEditor.stack` surfaces) and DOMAIN (ISO 129-1 tolerancing).
- [OFFSET_STACK] [RESOLVED]: the dimension-line offset stack is one `kiwisolver.Solver` + `strength` (verified `Solver.addConstraint`/`updateVariables`, `Variable.value`, `strength.weak`/`required`, `strength.create`, `Constraint.violated`, `UnsatisfiableConstraint`) — a `required` anchor at the first base offset, `required` DIMDLI min-separation between consecutive lines, and a custom `strength.create(0, 1, 0, 4)` equal-gap band (a medium-×4 priority above plain `weak`, the custom band the fixed weak/medium pair ignored, exercised live blending correctly) distributing the chain over the stackable linear/aligned/baseline ops keyed on the stable op index, `Constraint.violated()` reading back the sacrificed soft gaps (verified live returning `True` on a dense chain that forced min-sep to dominate) so a collapsed distribution falls back to deterministic fixed DIMDLI stepping, `updateVariables()` writing each solved `value()` back and `_shifted` applying it perpendicular through one `numpy` normal (verified `np.asarray`/`np.hypot`). This is the reading-map `apiUnderutilized` target closed: a naive page places dimension lines at fixed offsets and a dense chain overlaps; the Cassowary solve distributes them. The `kiwisolver` catalog names dimension/sheet alignment as an explicit constraint-layout target. Justified on PACKAGE (the verified `kiwisolver` surface) and DOMAIN (DIMDLI baseline spacing + no-overlap chain distribution).
- [LAYERED_DECOMPOSITION] [RESOLVED]: the `LAYERED` target composes the reading-map `apiUnderutilized` catalogs (`ziamath`, `ziafont`) into named editable `export/layered#LAYERED` `Layer` rows WITHOUT double-rendering what `ezdxf` already draws — the `DXF`/`SVG`/`PDF` targets LEAD with `add_*_dim().render()` (ezdxf owns all terminators/text natively), while the `LAYERED` target authors the ISO 129-1 COMPONENTS under one `_pen`-resolved discipline sRGB (`Standard.rgb` over the DIMS layer): a linear dimension's extension/dimension-line geometry from `ezdxf.math.ConstructionLine` and — the primary gap closed — a CURVED dimension's true measured `ConstructionArc`/`ConstructionCircle` via `.flattening(_SAGITTA)` (verified: the positional `ConstructionArc(center, radius, start_angle, end_angle)`/`ConstructionCircle(center, radius)` ctors and the `.flattening(sagitta) -> Iterable[Vec2]` adaptive stream, plus `ConstructionLine.intersect` for the 2-line angular apex — all exercised live, the prior fold NAMED the arc/circle in prose but only lifted `ConstructionLine`, an illusory-density defect the radial/angular/arc decomposition now actually draws) inside one penned `_svg` group so the strokeless `fill="none"` lines actually render, the terminator family as SELF-CONTAINED filled/stroked SVG marks (a filled arrow triangle, a stroked oblique tick, a filled dot, an open chevron — directly renderable, no foreign outline pass), the ISO 3098 measurement text outlined through `ziafont.Font(...).text(..., color=pen)` MEASURED via `getsize()`/`getyofst()` (verified both `Text.getsize() -> tuple[float, float]`/`Text.getyofst() -> float`) — centred horizontally on the anchor and baseline-lifted above the dimension line — then `.drawon(...)` (verified `Font.text`/`Text.drawon` + the `color` param), and a `Limits`/`Basic` expression typeset through `ziamath.Latex(expr, color=pen)` MEASURED via `getsize()` (offset right of the value so the stacks never collide) then `.drawon(svg, x, y, halign="center", valign="axis")` (verified `Latex.svg`/`drawon`/`getsize` + the `color`/`halign`/`valign` params, `valign='axis'` seating at the math axis), the whole LAYERED arm pinning `ziafont.config.precision`/`ziamath.config.precision` once inside the offload lane (verified both `config.precision` attrs, ziamath delegating to ziafont) so the outlined `d`-floats — and the content key over them — are deterministic. This is a genuinely distinct deliverable (font-independent, layer-separated, editable) from the flat `SVGBackend` output, so no ISO component is re-drawn twice. Justified on PACKAGE (every member reflection-verified) and DOMAIN (an editable layer-separated dimension is the drawing-editor deliverable the flat render is not).
- [TERMINATOR_SELF_CONTAINED] [RESOLVED]: the prior `_terminator_layer` called `VectorOp.Outline(...)` and `Vector.over(ops)._worked(ops)` — BOTH phantoms. `graphic/vector#VECTOR` exposes NO `VectorOp.Outline` case (its verified `VectorOp` family is `transform`/`bounds`/`serialize`/`rasterize`/`measure`/`sample`/`flatten`/`subpaths`/`project`), and `_worked` is a private MODULE-LEVEL function on that page, not a `Vector` method (the `Vector` rail is the async `over`/`of` returning `RuntimeRail[Block[VectorResult]]`), so the call was a hard `AttributeError` dressed as dense composition, AND its `_polyline` centerlines were strokeless (invisible) awaiting a never-landed outline pass — a self-admitted illusory rail ("the anticipated arm"). The rebuild draws each ISO 129-1 terminator as a self-contained filled/stroked SVG mark (a filled arrow TRIANGLE is the correct closed filled arrowhead; the oblique tick and open chevron are stroked; the dot is filled) penned by the discipline sRGB, and threads the same pen through the construction/text/tolerance layers so the whole editable projection carries the CAD pen — exactly the self-contained stance the finalized `drawing/symbol#SYMBOL`/`drawing/annotate#ANNOTATE` siblings take. The tapered variable-width terminator stroke-to-outline composes the LANDED `graphic/vector#VECTOR` `outline` — its public `outline(source, width, cap=CapStyle.BUTT_CAP, join=JoinStyle.MITER_JOIN, miter=4.0, dash=None)` and `VectorOp.Outline` case are now present on that owner (the current `graphic/vector` `VectorOp` family is `transform`/`bounds`/`fit`/`serialize`/`rasterize`/`measure`/`sample`/`flatten`/`subpaths`/`project`/`boolean`/`outline`/`region`), shared with `symbol`/`annotate`, never authored twice. The self-contained filled arrow triangle is the CORRECT ISO 129-1 filled terminator and stays the default (a fill, not a stroked centerline `outline` would convert), so it is not force-routed through `outline`; the still-rejected phantom is the OLD `Vector.over(ops)._worked(ops)` private-method reach (`_worked` remains a private module fold on that owner, never a `Vector` method). Justified on PACKAGE (the landed `graphic/vector` `outline`/`VectorOp.Outline`) and DOMAIN (a filled arrow triangle IS the ISO 129-1 filled terminator, self-contained by default) and CONSUMER (the tapered outline the composable premium over the correct base).
- [PRODUCER_RAIL] [RESOLVED]: the fault vocabulary is the closed `_FAULTS` stdlib-base tuple (`ValueError`/`KeyError`/`TypeError`/`OSError`) the runtime `async_boundary(subject, self._compute, catch=_FAULTS)` classifies into `BoundaryFault` — NOT a parallel `DimFault` `Literal`/`@tagged_union` the boundary never reads. The `ezdxf` render raises are `DXFValueError(DXFError, ValueError)`/`DXFKeyError(DXFError, KeyError)`/`DXFTypeError(DXFError, TypeError)` — reflection-verified stdlib-derived — so naming the bases catches them WITHOUT reifying the lazy `ezdxf` import at module scope (referencing `ezdxf.DXFError` in the tuple would force the heavy import on load, the defect the settled `composition/sheet#SHEET` `[RAIL_SETTLED]` `_FAULTS` pattern avoids by naming `RuntimeError`/`ValueError` bases); the `kiwisolver` solve is satisfiable by construction (a `required` anchor + `required` min-separation + `weak` equal-gap chain is always feasible, and no edit variable is registered), so `UnsatisfiableConstraint`/`BadRequiredStrength` never raise. This adopts the settled producer model — which deletes a parallel fault `Literal` as an illusory-rail defect and the double-rail of a domain fault nested inside the `RuntimeRail` — over the reading-map "closed DimFault" pointer, which would be exactly that hollow rail. The interior is a pure `to_thread.run_sync` fold raising nothing of its own; the `@beartype` `over` contract admits the construction shape at call time (raising to the caller, the `drawing/standard#STANDARD` `of` pattern), distinct from the render rail. Justified on the corpus-law finalized-sibling inheritance (the `sheet.md`/`imposition.md` `_FAULTS` model the finalized `drawing/symbol#SYMBOL`/`drawing/annotate#ANNOTATE` producers already conform to — neither declares a page-local fault union the boundary never reads, so `dimension` matches the settled drawing-plane rail rather than introducing a `DimFault`).
- [SHEET_AND_RECEIPT_SEAM] [RESOLVED]: the dimensioned SVG/PDF/DXF bytes feed `composition/sheet#SHEET`'s `FigurePlacement` drawing-region cell through `SheetOp.Place` (verified `FigurePlacement(cell: Box, ...)` consuming figure PDF bytes) — a bytes seam, not an absorb, recorded as the `drawing/dimension → composition/sheet` seam in `ARCHITECTURE.md` `[02]-[SEAMS]`. The owner contributes one `core/receipt#RECEIPT` `ArtifactReceipt.Drawing(key, "drawing-dimension", entities, dimstyle, width, height, bytes)` case (already present on `core/receipt.md`, the shared drawing-plane case naming `drawing/dimension#DIMENSION` as a contributor) for the DXF/SVG/LAYERED targets, and reuses `ArtifactReceipt.Pdf(key, bytes, pages)` for the `PDF` backend exactly as `composition/sheet#SHEET` reuses `Pdf`/`Egress` — no new receipt case. The `core/plan#PLAN` `ArtifactWork` node wraps the `render` coroutine's receipt projection keyed by the content identity, the fourteenth producer contributing to the one `ArtifactPipeline`. Justified on CONSUMER (the sheet-cell bytes contract, the shared receipt case) — no `core/receipt.md`/`core/plan.md` edit is needed because the `Drawing` case and the generic `ArtifactWork` node already exist.
