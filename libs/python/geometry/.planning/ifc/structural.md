# [PY_GEOMETRY_IFC_STRUCTURAL]

One cross-section structural-property owner — section-integral and structural-member verbs the analysis and lifecycle hops drop. `IfcStructural` partitions a profile-bearing selector match by RESOLVED profile identity and folds one closed-form section receipt per distinct section: a `numpy` Green's-theorem contour fold over the `MOMENT_KERNELS` weight table (area, first and second area moments, centroid, principal second moments and principal-axis rotation, polar moment, centroid-relative elastic section moduli, thin-walled Bredt torsion constant) runs over each profile's body set, then two gated enrichment layers tier that spine under one `EnrichmentTier` policy: the `ifcopenshell` layer reads `IfcStructuralAnalysisModel`/`IfcStructuralMember` topology onto the member, the `sectionproperties` layer meshes the same rings into a triangular FE section for the warping, plastic, and shear receipts no closed-form integral derives. C#'s `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the numerical section dimension the managed projection does not produce.

A profile is a BODY SET, never one outer ring plus voids: the IFC4 `IfcCompositeProfileDef`, `IfcDerivedProfileDef`, and `IfcMirroredProfileDef` families all resolve to several disjoint regions, so `ProfileRings` carries `ProfileBody` rows and the ring-additive contour fold absorbs them with no second engine. Both enrichments are one `Enrichment` `@tagged_union` on a single `SectionReceipt.enrichment` field — the `entity` case carrying the structural-member GlobalIds, the `warping` case the FE `WarpingEvidence` value object — so a `CORE` receipt carries `Nothing`, never a `None`-slot bag racing the tier. Every foreign EXPRESS closure this owner dispatches on is a closed `StrEnum` whose member VALUE is the IFC class name — `ProfileFamily` over `IfcProfileDef`, `ParametricProfile` over its parameterized leaves, `MaterialProfileSource` over `IfcMaterialSelect`, `CurveForm` over `IfcBoundedCurve`, `SegmentKind` over the indexed-curve segment kinds — elected through ONE narrowest-first `_family` `is_a` probe and consumed by a total `match` ending in `assert_never`, so an unrostered subtype refuses at ADMISSION where a ladder fall-through or a `case _` used to absorb it silently. Every profile-bearing query admits through `IfcSelector.filter`/`IfcSelector.parse` from `ifc/selector#SELECTOR` (the one selection engine, the only `filter_elements` caller), a `<selector>#<analysis-model-guid>` split feeding the member rings and the structural-model guid, so a malformed profile selector is a typed refusal at admission. Every refusal this owner mints is an `ifc/selector#SELECTOR` `IfcFault` case riding the runtime's own `BoundaryFault.domain` slot, so the band's one closed vocabulary crosses the funnel whole and no fence spells a cause string. Whole admission rides the graduation `evidence_run` weave under `EvidenceScope.IFC_SECTION`, so a provider exception, a degenerate ring, an unsupported subtype, and an FE divergence each fold onto one `RuntimeRail[Block[SectionReceipt]]`. Receipt graduates as `GeometrySubject.SECTION_PROPERTY` carried by the `STRUCTURAL_SUBJECT` constant — the section-integral evidence class distinct from the compliance and lifecycle members its siblings bind — and `graduates()` returns the local `GeometryHandoff` whose `wire()` projection is the compute crossing. Spine rides the bare runtime (`numpy`) and never depends on either gated layer; every tier runs caller-floor by charter — the live `ifcopenshell.file` is a pybind11 handle no pickle seam carries and one per-profile FE solve is short-bounded work — with `ifcopenshell` and the `sectionproperties` band (native mesh backend `cytriangle`, LGPLv3) each riding one module-scope `lazy import`: the `ifcopenshell` proxy costs nothing a caller has not already paid, since handing this owner a live `ifcopenshell.file` imports the package first, and the `sectionproperties` proxy reifies on the first `WARPING` solve, so a `CORE` run loads no FE stack.

## [01]-[INDEX]

- [02]-[STRUCTURAL]: profile-partitioned section-integral spine and two gated enrichment tiers under one `EnrichmentTier` owner folding per-tier evidence into one `Enrichment` union, woven through the `evidence_run` graduation weave, emitting the `SECTION_PROPERTY` subject.

## [02]-[STRUCTURAL]

- Owner: `IfcStructural` — boundary capsule over the section-integral spine and two gated enrichment tiers; `EnrichmentTier` discriminant and `Enrichment` evidence shape are the same union, so each tier is one `_section` builder arm, never a sibling per-tier class. `ProfileRings`/`ProfileBody` is the one geometric input owner: `ProfileBody` a CCW outer ring with its CW voids, `ProfileRings` the flat body set every profile family flattens onto, so the contour fold, the FE region set, and the extreme-fibre reach all read one shape. `_family` is the one type-identity ELECTION on the page: every dispatched EXPRESS closure is a `StrEnum` roster keyed by the class name, so the correspondence IS the roster and the four membership guards that elect nothing (`IfcProfileDef`, `IfcRelAssociatesMaterial`, `IfcRelAssignsToGroup`, `IfcStructuralMember`) are the only surviving class literals.
- Cases: `CORE` folds the closed-form section-integral receipt on the bare runtime interpreter; `IFC_ENTITY` adds the `IfcStructuralAnalysisModel`/`IfcStructuralMember` topology behind its `ifcopenshell` tier gate; `WARPING` adds the FE warping/plastic/shear receipts behind its `sectionproperties` tier gate. Spine never depends on either gated layer — upper tiers add evidence only where their gated package resolves.
- Entry: `IfcStructural.run` takes an `ifcopenshell.file`, an `EnrichmentTier`, a `spec` whose meaning the tier fixes — a `<selector>` profile-bearing query for `CORE`/`WARPING` resolving each element's `IfcProfileDef` rings off its material-profile assignment, a `<selector>#<analysis-model-guid>` query for `IFC_ENTITY` joining the members to their structural-analysis model — and the `composition` custody key, returning `RuntimeRail[Block[SectionReceipt]]`: one receipt per DISTINCT resolved profile, never one head section published under every matched GlobalId. Graduation stays the caller's own step on `SectionReceipt.graduates(ceiling)`, which derives its own `ContentKey` from the receipt's `spec` through the spine's `evidence_key`, so no caller mints a key for evidence it did not produce. `subjects` derives from the group's true subject set — the group's profile-bearing GlobalIds for `CORE`/`WARPING`, structural-member GlobalIds for `IFC_ENTITY`.
- Auto: `_dispatch` binds the `IfcSelector` admission, the `_matched` empty-match gate, and the `_grouped` partition, then `traversed(..., ACCUMULATE)` folds one `_section` builder per group so ONE refusal names every divergent profile rather than one run per defect — which is why every refusal on this page RETURNS its rail and none raises: a raise unwinds the fold before it collects its census. `_grouped` keys on the resolved profile entity's own step id through the ordered `Map`, so group order — and every receipt, charter sample, and frame row derived from it — is deterministic. `_sample` is a total `match` over the `ProfileFamily` election: a composite or transformed profile folds its children through the same entry (nesting FLATTENS to one body list, mirroring and the `IfcCartesianTransformationOperator2D` affine applying to the parent's rings), a centreline profile offsets its OPEN path to `Thickness` under squared end caps — the one family whose boundary input never closes — an arbitrary-closed profile reads its curves with the void read gated on the ELECTED `ARBITRARY_VOIDS` member, which is the entity that DECLARES `InnerCurves`, and a parameterized profile elects its `ParametricProfile` row and builds one body from `PROFILE_SAMPLERS`. The open non-centreline family refuses BY NAME as rostered-but-unserved, where the ladder's fall-through used to hand it to the parametric table and report a missing subtype. Missing dimensions name themselves, and a centreline whose miter reversal or inner retraction crosses the offset ring names the offending vertex. `_curve` is a total `match` over the `CurveForm` election, serving the two forms a polygonal boundary admits and naming the other seven: an indexed poly-curve's `Segments` order every line run and tessellate every arc run off the circumcircle through its three indices, a collinear arc triple lowers to its chords, and an explicit closing repeat drops at intake so no zero-length edge reaches the ring. `_profile_of_material` is a total `match` over the `IfcMaterialSelect` closure, so the tapering profile-set usage, the offset-carrying material profile, and the bare `IfcMaterialProfile` all resolve their section where an exact-class ladder answered `Nothing` and sent a real profile-bearing element to a fall-back read of itself. `_rings` is the one admission fold over every ring of every body — `ClosedRing` refinement, non-degenerate signed area, global edge-pair simplicity, then winding normalization to CCW outer and CW void — so the contour integral reads the sign off the vertex order with no `signed` multiplier column and the area divisor is non-zero and the loop simple BY ADMISSION, the centreline's per-vertex miter gates staying the earlier, cause-naming diagnosis the global probe backstops. `_entity` folds the `IsGroupedBy`/`IfcRelAssignsToGroup`-guarded `IfcStructuralMember` set — entity topology only, never re-deriving a section property, since the centroid-relative elastic section moduli are a closed-form spine field every tier carries. `_warping` builds one `pre.CompoundGeometry.from_points` body set — one control point per body outer, one hole marker per void, one closed facet loop per ring in outers-then-voids order, per-body mesh area off `ProfileBody.extent` — runs geometric→warping→plastic in the prerequisite order, and reads `get_area` back to cross-check the `numpy` spine area (the `fe-area` residual); the FE torsion lands on `WarpingEvidence.fe_torsion_constant`, never overwriting the spine's thin-walled `torsion_constant`.
- Receipt: `SectionReceipt` conforms structurally to `ReceiptContributor` — one `row` projection is the single column roster the receipt facts, the frame columns, and the residual ledger all read, so a new integral reaches all three through one field. `contribute` emits that row per group; `graduates` folds the tier-aware `measured` ledger onto `GeometryHandoff.of(STRUCTURAL_SUBJECT, ...)` rather than inlining a ceiling comparison; `framed` projects the whole run as ONE `EvidenceFrame` with one row per group, so the columnar egress carries the real section-to-section spread a board cuts on; and `_distributed` records the `ring-closure` residual as the `rasm.geometry.section.closure` charter measure per group through the graduation `charter_record` derivation at the producing fold, so a six-section partition records six samples where a head-only fold publishes one. `measured` ledger is data-driven by tier — the `ring-closure` residual (polar moment vs principal sum) every tier, and the `WARPING` `fe-area` FE-convergence residual — so a degenerate profile or a diverging FE mesh graduates as an `Error(BoundaryFault)`, never a clean section receipt.
- Packages: `numpy` (the shoelace contour fold over `MOMENT_KERNELS`, `linalg.eigh` for the major-axis principal solution, `linspace`/`stack`/`concatenate` for the curved, rounded, and arc-tessellated polylines, `arctan2`/`mod` for the arc sweep and `diff`/`hypot`/`flatnonzero` for the centreline miter offset and its degeneracy gates, `unique` and the roll-straddle crossing solve for the `_interior_point` ray scan, `ascontiguousarray` normalizing every admitted ring); `expression` (the `railed` `effect.result` rails, `Block` folds for the group partition, member set, and facet loops, ordered `Map` for the profile index, the `Enrichment` union, `Option` rail lifts); `beartype` (the `ClosedRing` `Is` refinement proved through `is_bearable` at admission, `FAULT_CONF` on the `_integrate` numeric leaf); geometry graduation (`evidence_run`/`GeometryHandoff`/`GeometrySubject`/`EvidenceFrame`, `charter_record` the charter measure authority, `evidence_key` the receipt-spec key derivation); `ifc/selector#SELECTOR` (`IfcSelector.filter`/`parse` — the only `filter_elements` caller — and the band-wide `IfcFault` family with the `CurveFlaw`/`IfcRoster`/`SectionMeasure` coordinate vocabularies its cases carry); `ifcopenshell` (the `IfcProfileDef` family attributes, `entity_instance.is_a` the one election probe, `entity_instance.id()` the partition key, `IfcStructuralAnalysisModel`/`IfcStructuralMember` topology over the in-process model); `sectionproperties` (`WARPING` tier only, `pre.CompoundGeometry.from_points` the arity-polymorphic body-set surface, native mesh backend `cytriangle`, LGPLv3); stdlib `enum` (`StrEnum` for every mirrored EXPRESS closure); runtime rails (`FaultRow`/`RAISES` the two raise coordinates this page spends, `boundary` under a named `catch` set, `BoundaryFault.of` the domain door).
- Growth: a new section integral is one `MOMENT_KERNELS` row and one `SectionReceipt` field reaching the receipt, the frame, and the ledger through `row`; a new parametric profile subtype is one `ParametricProfile` member and one `PROFILE_SAMPLERS` row — its `dimensions` roster, its `excluded` waiver, and its ring constructor — the rings staying the universal input and the contour fold shape-agnostic, never a per-shape integral family; a new member on any mirrored EXPRESS closure is one `StrEnum` row whose absence from the total `match` breaks the arm loudly, never a fall-through; a new boundary-curve form moves one `CurveForm` member from the unserved arm to its own; a new enrichment tier is one `EnrichmentTier` row, one `Enrichment` case, and one `_section` arm; a new warping/plastic measure is one `WarpingEvidence` field and one `Section.get_*` accessor; a new selection axis is one `IfcSelector` grammar alternative, never a local query-parse fold; a new band refusal is one `IfcFault` case at `ifc/selector#SELECTOR`, never a cause string spelled here; a stricter residual bar is one tighter ceiling row the caller supplies. A survivor/casualty partition is the named next disposition once a consumer needs the sections a mixed match CAN yield beside the refusals the accumulating fold names.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy (projected in-process); no durable store, Rhino/GH mutation, or mesh/GLB write — the `WARPING` FE section mesh is an in-memory `sectionproperties` artifact consumed for its scalars, never a `mesh/repair.md` payload write. Both `sectionproperties` and `ifcopenshell` bind once at module scope under `lazy import`, so the FE stack stays unloaded on a `CORE` run and neither table nor constant dereferences a proxy at import; a function-local import earns nothing the deferral does not already give. Deterministic `sectionproperties` solves own no transiency — a retry over it (a `stamina.retry` mint included) is a deleted form. Raw `spec` never threads past admission into `filter_elements` — `IfcSelector` re-serializes the validated query, the one selection engine. No sampler re-reads its entity, no fillet, edge, or slope attribute enters a polygonal idealization that cannot carry it — each one rides its row's `excluded` waiver `_seated` READS at import, never a prose carve nothing executes — and no ring reaches the integral unwound, unproven, crossed, or non-contiguous.

```python signature
from collections.abc import Callable
from enum import IntEnum, StrEnum
from typing import Annotated, Final, Literal, assert_never

import numpy as np
from beartype import beartype
from beartype.door import is_bearable
from beartype.vale import Is
from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from numpy.typing import NDArray

lazy import ifcopenshell
lazy import sectionproperties.analysis as spa
lazy import sectionproperties.pre as spp

from rasm.geometry.graduation import (
    EvidenceFrame,
    EvidenceScope,
    GeometryHandoff,
    GeometryLeg,
    GeometrySubject,
    charter_record,
    evidence_key,
    evidence_run,
)
from rasm.geometry.ifc.selector import CurveFlaw, IfcFault, IfcRoster, IfcSelector, SectionMeasure
from rasm.runtime.faults import (
    FAULT_CONF,
    PACKAGE,
    TERMINAL,
    BoundaryFault,
    Catch,
    Disposition,
    FaultRow,
    RuntimeRail,
    boundary,
    railed,
    rostered,
    traversed,
)
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey

# --- [TYPES] ----------------------------------------------------------------------------


class EnrichmentTier(IntEnum):
    CORE = 0
    IFC_ENTITY = 1
    WARPING = 2




class ProfileFamily(StrEnum):
    ARBITRARY_VOIDS = "IfcArbitraryProfileDefWithVoids"
    ARBITRARY = "IfcArbitraryClosedProfileDef"
    CENTERLINE = "IfcCenterLineProfileDef"
    ARBITRARY_OPEN = "IfcArbitraryOpenProfileDef"
    MIRRORED = "IfcMirroredProfileDef"
    DERIVED = "IfcDerivedProfileDef"
    COMPOSITE = "IfcCompositeProfileDef"
    PARAMETERIZED = "IfcParameterizedProfileDef"


class ParametricProfile(StrEnum):
    RECTANGLE_HOLLOW = "IfcRectangleHollowProfileDef"
    ROUNDED_RECTANGLE = "IfcRoundedRectangleProfileDef"
    RECTANGLE = "IfcRectangleProfileDef"
    CIRCLE_HOLLOW = "IfcCircleHollowProfileDef"
    CIRCLE = "IfcCircleProfileDef"
    ELLIPSE = "IfcEllipseProfileDef"
    ASYMMETRIC_I = "IfcAsymmetricIShapeProfileDef"
    I_SHAPE = "IfcIShapeProfileDef"
    L_SHAPE = "IfcLShapeProfileDef"
    U_SHAPE = "IfcUShapeProfileDef"
    T_SHAPE = "IfcTShapeProfileDef"
    Z_SHAPE = "IfcZShapeProfileDef"
    C_SHAPE = "IfcCShapeProfileDef"
    TRAPEZIUM = "IfcTrapeziumProfileDef"


class MaterialProfileSource(StrEnum):
    PROFILE_SET_USAGE_TAPERING = "IfcMaterialProfileSetUsageTapering"
    PROFILE_SET_USAGE = "IfcMaterialProfileSetUsage"
    LAYER_SET_USAGE = "IfcMaterialLayerSetUsage"
    PROFILE_WITH_OFFSETS = "IfcMaterialProfileWithOffsets"
    PROFILE = "IfcMaterialProfile"
    PROFILE_SET = "IfcMaterialProfileSet"
    CONSTITUENT_SET = "IfcMaterialConstituentSet"
    CONSTITUENT = "IfcMaterialConstituent"
    LAYER_WITH_OFFSETS = "IfcMaterialLayerWithOffsets"
    LAYER_SET = "IfcMaterialLayerSet"
    LAYER = "IfcMaterialLayer"
    LIST = "IfcMaterialList"
    MATERIAL = "IfcMaterial"


class CurveForm(StrEnum):
    OUTER_BOUNDARY = "IfcOuterBoundaryCurve"
    BOUNDARY = "IfcBoundaryCurve"
    COMPOSITE_ON_SURFACE = "IfcCompositeCurveOnSurface"
    COMPOSITE = "IfcCompositeCurve"
    RATIONAL_BSPLINE = "IfcRationalBSplineCurveWithKnots"
    BSPLINE = "IfcBSplineCurveWithKnots"
    INDEXED = "IfcIndexedPolyCurve"
    POLYLINE = "IfcPolyline"
    TRIMMED = "IfcTrimmedCurve"


class SegmentKind(StrEnum):
    ARC = "IfcArcIndex"
    LINE = "IfcLineIndex"


type Ring = NDArray[np.float64]
type RingTuple = tuple[Ring, tuple[Ring, ...]]
type Affine = NDArray[np.float64]
type _Moment = Callable[[Ring, Ring, Ring, Ring], Ring]
type _Sampler = Callable[[tuple[float, ...]], RingTuple]
type _ProfileRow = tuple[tuple[str, ...], frozenset[str], _Sampler]

type ClosedRing = Annotated[Ring, Is[lambda r: r.ndim == 2 and r.shape[0] >= 3 and bool(np.isfinite(r).all())]]

# --- [CONSTANTS] ------------------------------------------------------------------------

STRUCTURAL_SUBJECT: Final[GeometrySubject] = GeometrySubject.SECTION_PROPERTY

OWNER: Final[str] = f"{PACKAGE}.{GeometryLeg.STRUCTURAL.value}"

CIRCLE_SEGMENTS: Final[int] = 64

_EPS: Final[float] = 1e-12

_MIRROR: Final[Affine] = np.array([(-1.0, 0.0), (0.0, 1.0), (0.0, 0.0)], dtype=np.float64)

# --- [TABLES] ---------------------------------------------------------------------------


MOMENT_KERNELS: Final[tuple[tuple[str, _Moment, float], ...]] = (
    ("a", lambda x, y, xn, yn: np.ones_like(x), 2.0),
    ("qx", lambda x, y, xn, yn: y + yn, 6.0),
    ("qy", lambda x, y, xn, yn: x + xn, 6.0),
    ("ixx", lambda x, y, xn, yn: y * y + y * yn + yn * yn, 12.0),
    ("iyy", lambda x, y, xn, yn: x * x + x * xn + xn * xn, 12.0),
    ("ixy", lambda x, y, xn, yn: x * yn + 2.0 * x * y + 2.0 * xn * yn + xn * y, 24.0),
)

def _seated(rows: tuple[tuple[ParametricProfile, _ProfileRow], ...]) -> "Map[ParametricProfile, _ProfileRow]":
    seat = Map.of_seq(rows)
    unseated = tuple(leaf.value for leaf in ParametricProfile if leaf not in seat)
    tangled = tuple(f"{leaf.value}.{name}" for leaf, (names, excluded, _) in rows for name in sorted(frozenset(names) & excluded))
    if unseated or tangled or len(rows) != len(seat):
        raise ValueError(",".join((*unseated, *tangled)) or ParametricProfile.__name__)
    return seat


PROFILE_SAMPLERS: Final[Map[ParametricProfile, _ProfileRow]] = _seated((
    (ParametricProfile.RECTANGLE_HOLLOW, (("XDim", "YDim", "WallThickness"), frozenset({"InnerFilletRadius", "OuterFilletRadius"}), lambda d: _box(d[0], d[1], d[2]))),
    (ParametricProfile.ROUNDED_RECTANGLE, (("XDim", "YDim", "RoundingRadius"), frozenset(), lambda d: (_rect(d[0], d[1], d[2]), ()))),
    (ParametricProfile.RECTANGLE, (("XDim", "YDim"), frozenset(), lambda d: (_rect(d[0], d[1], 0.0), ()))),
    (ParametricProfile.CIRCLE_HOLLOW, (("Radius", "WallThickness"), frozenset(), lambda d: (_ellipse(d[0], d[0]), (_ellipse(d[0] - d[1], d[0] - d[1]),)))),
    (ParametricProfile.CIRCLE, (("Radius",), frozenset(), lambda d: (_ellipse(d[0], d[0]), ()))),
    (ParametricProfile.ELLIPSE, (("SemiAxis1", "SemiAxis2"), frozenset(), lambda d: (_ellipse(d[0], d[1]), ()))),
    (
        ParametricProfile.ASYMMETRIC_I,
        (
            ("BottomFlangeWidth", "OverallDepth", "WebThickness", "BottomFlangeThickness", "TopFlangeWidth", "TopFlangeThickness"),
            frozenset({
                "BottomFlangeFilletRadius", "TopFlangeFilletRadius", "BottomFlangeEdgeRadius",
                "TopFlangeEdgeRadius", "BottomFlangeSlope", "TopFlangeSlope",
            }),
            lambda d: (_i_section(*d), ()),
        ),
    ),
    (
        ParametricProfile.I_SHAPE,
        (
            ("OverallWidth", "OverallDepth", "WebThickness", "FlangeThickness"),
            frozenset({"FilletRadius", "FlangeEdgeRadius", "FlangeSlope"}),
            lambda d: (_i_section(d[0], d[1], d[2], d[3], d[0], d[3]), ()),
        ),
    ),
    (ParametricProfile.L_SHAPE, (("Depth", "Width", "Thickness"), frozenset({"FilletRadius", "EdgeRadius", "LegSlope"}), lambda d: (_l_section(*d), ()))),
    (
        ParametricProfile.U_SHAPE,
        (("Depth", "FlangeWidth", "WebThickness", "FlangeThickness"), frozenset({"FilletRadius", "EdgeRadius", "FlangeSlope"}), lambda d: (_u_section(*d), ())),
    ),
    (
        ParametricProfile.T_SHAPE,
        (
            ("Depth", "FlangeWidth", "WebThickness", "FlangeThickness"),
            frozenset({"FilletRadius", "FlangeEdgeRadius", "WebEdgeRadius", "WebSlope", "FlangeSlope"}),
            lambda d: (_t_section(*d), ()),
        ),
    ),
    (
        ParametricProfile.Z_SHAPE,
        (("Depth", "FlangeWidth", "WebThickness", "FlangeThickness"), frozenset({"FilletRadius", "EdgeRadius"}), lambda d: (_z_section(*d), ())),
    ),
    (ParametricProfile.C_SHAPE, (("Depth", "Width", "WallThickness", "Girth"), frozenset({"InternalFilletRadius"}), lambda d: (_c_section(*d), ()))),
    (ParametricProfile.TRAPEZIUM, (("BottomXDim", "TopXDim", "YDim", "TopXOffset"), frozenset(), lambda d: (_trapezium(*d), ()))),
))

# --- [MODELS] ---------------------------------------------------------------------------


class WarpingEvidence(Struct, frozen=True, gc=False):
    fe_torsion_constant: float
    fe_area: float
    shear_center: tuple[float, float]
    shear_areas: tuple[float, float]
    plastic_moduli: tuple[float, float]
    mesh_elements: int


@tagged_union(frozen=True)
class Enrichment:
    tag: Literal["entity", "warping"] = tag()
    entity: tuple[str, ...] = case()
    warping: WarpingEvidence = case()

    def facts(self) -> dict[str, object]:
        match self:
            case Enrichment(tag="entity", entity=members):
                return {"members": len(members)}
            case Enrichment(tag="warping", warping=fe):
                return {"mesh_elements": fe.mesh_elements, "fe_torsion_constant": fe.fe_torsion_constant, "fe_area": fe.fe_area}
            case _ as unreachable:
                assert_never(unreachable)


class ProfileBody(Struct, frozen=True, gc=False):
    outer: ClosedRing
    voids: tuple[ClosedRing, ...]

    @property
    def extent(self) -> float:
        return abs(_shoelace(self.outer)) - sum(abs(_shoelace(void)) for void in self.voids)


class ProfileRings(Struct, frozen=True, gc=False):
    bodies: tuple[ProfileBody, ...]

    @property
    def rings(self) -> tuple[Ring, ...]:
        return (*self.outers, *self.voids)

    @property
    def outers(self) -> tuple[Ring, ...]:
        return tuple(body.outer for body in self.bodies)

    @property
    def voids(self) -> tuple[Ring, ...]:
        return tuple(void for body in self.bodies for void in body.voids)


class ProfileGroup(Struct, frozen=True, gc=False):
    profile: "ifcopenshell.entity_instance"
    subjects: tuple[str, ...]


class SectionReceipt(Struct, frozen=True, gc=False):
    tier: EnrichmentTier
    subjects: tuple[str, ...]
    spec: str
    area: float
    centroid: tuple[float, float]
    second_moments: tuple[float, float, float]
    principal_moments: tuple[float, float]
    principal_angle: float
    polar_moment: float
    torsion_constant: float
    section_moduli: tuple[float, float]
    enrichment: Option[Enrichment] = Nothing

    @property
    def measured(self) -> dict[str, float]:
        ledger = {"ring-closure": abs(self.polar_moment - sum(self.principal_moments)) / max(abs(self.polar_moment), _EPS)}
        match self.enrichment:
            case Option(tag="some", some=Enrichment(tag="warping", warping=fe)):
                return ledger | {"fe-area": abs(fe.fe_area - self.area) / max(self.area, _EPS)}
            case _:
                return ledger

    @property
    def row(self) -> dict[str, object]:
        return {
            "tier": self.tier.name,
            "subjects": len(self.subjects),
            "area": self.area,
            "centroid_x": self.centroid[0],
            "centroid_y": self.centroid[1],
            "ixx": self.second_moments[0],
            "iyy": self.second_moments[1],
            "ixy": self.second_moments[2],
            "i_major": self.principal_moments[0],
            "i_minor": self.principal_moments[1],
            "principal_angle": self.principal_angle,
            "polar_moment": self.polar_moment,
            "torsion_constant": self.torsion_constant,
            "s_x": self.section_moduli[0],
            "s_y": self.section_moduli[1],
            **self.enrichment.map(lambda held: held.facts()).default_value({}),
            **self.measured,
        }

    def contribute(self) -> "Block[Receipt]":
        return Block.singleton(Receipt.of(OWNER, ("emitted", STRUCTURAL_SUBJECT, self.row)))

    def graduates(self, ceiling: dict[str, float]) -> GeometryHandoff:
        return GeometryHandoff.of(STRUCTURAL_SUBJECT, evidence_key(STRUCTURAL_SUBJECT, self.spec), self.measured, ceiling)

    @staticmethod
    def framed(receipts: "Block[SectionReceipt]") -> "RuntimeRail[EvidenceFrame]":
        names = tuple(receipts.head().row) if not receipts.is_empty() else ()
        table: dict[str, list[object]] = {name: [receipt.row[name] for receipt in receipts] for name in names}
        return EvidenceFrame.of(STRUCTURAL_SUBJECT, evidence_key(STRUCTURAL_SUBJECT, "|".join(receipt.spec for receipt in receipts)), table)


# --- [ERRORS] ---------------------------------------------------------------------------


SECTION_REFUSED: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.STRUCTURAL, point="section", arm="boundary", defect="section-refused", retriability=TERMINAL
)
SECTION_WARPING: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.STRUCTURAL, point="warping", arm="boundary", defect="fe-solve", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([SECTION_REFUSED, SECTION_WARPING]))

_FE_RAISES: Final[Catch] = (IndexError, RuntimeError, TypeError, ValueError, ZeroDivisionError)


def _domain(fault: IfcFault) -> BoundaryFault:
    return BoundaryFault.of(SECTION_REFUSED, fault)


# --- [BOUNDARIES] -----------------------------------------------------------------------


def _family[E: StrEnum](entity: "ifcopenshell.entity_instance", vocabulary: type[E]) -> Option[E]:
    return Block.of_seq(vocabulary).choose(lambda member: Some(member) if entity.is_a(member.value) else Nothing).try_head()


# --- [OPERATIONS] -----------------------------------------------------------------------



def _shoelace(ring: Ring) -> float:
    x, y = ring[:, 0], ring[:, 1]
    return float(np.sum(x * np.roll(y, -1) - np.roll(x, -1) * y)) / 2.0


def _rect(xdim: float, ydim: float, radius: float) -> Ring:
    hx, hy = xdim / 2.0, ydim / 2.0
    corner = min(radius, hx, hy)
    theta = np.linspace(0.0, np.pi / 2.0, max(CIRCLE_SEGMENTS // 4, 2))
    return (
        np.array([(hx, -hy), (hx, hy), (-hx, hy), (-hx, -hy)], dtype=np.float64)
        if corner <= _EPS
        else np.concatenate([
            np.stack([cx + corner * np.cos(theta + phase), cy + corner * np.sin(theta + phase)], axis=1)
            for cx, cy, phase in (
                (hx - corner, hy - corner, 0.0),
                (-hx + corner, hy - corner, np.pi / 2.0),
                (-hx + corner, -hy + corner, np.pi),
                (hx - corner, -hy + corner, 3.0 * np.pi / 2.0),
            )
        ]).astype(np.float64)
    )


def _ellipse(semi_x: float, semi_y: float) -> Ring:
    theta = np.linspace(0.0, 2.0 * np.pi, CIRCLE_SEGMENTS, endpoint=False)
    return np.stack([semi_x * np.cos(theta), semi_y * np.sin(theta)], axis=1).astype(np.float64)


def _box(xdim: float, ydim: float, wall: float) -> RingTuple:
    return _rect(xdim, ydim, 0.0), (_rect(xdim - 2.0 * wall, ydim - 2.0 * wall, 0.0),)


def _i_section(bottom_width: float, depth: float, web: float, bottom_flange: float, top_width: float, top_flange: float) -> Ring:
    hb, ht, hd, hw = bottom_width / 2.0, top_width / 2.0, depth / 2.0, web / 2.0
    yb, yt = -hd + bottom_flange, hd - top_flange
    return np.array(
        [(-hb, -hd), (hb, -hd), (hb, yb), (hw, yb), (hw, yt), (ht, yt), (ht, hd), (-ht, hd), (-ht, yt), (-hw, yt), (-hw, yb), (-hb, yb)],
        dtype=np.float64,
    )


def _l_section(depth: float, width: float, thickness: float) -> Ring:
    hb, hd = width / 2.0, depth / 2.0
    return np.array(
        [(-hb, -hd), (hb, -hd), (hb, -hd + thickness), (-hb + thickness, -hd + thickness), (-hb + thickness, hd), (-hb, hd)], dtype=np.float64
    )


def _u_section(depth: float, flange_width: float, web: float, flange: float) -> Ring:
    hb, hd = flange_width / 2.0, depth / 2.0
    return np.array(
        [
            (-hb, -hd),
            (hb, -hd),
            (hb, -hd + flange),
            (-hb + web, -hd + flange),
            (-hb + web, hd - flange),
            (hb, hd - flange),
            (hb, hd),
            (-hb, hd),
        ],
        dtype=np.float64,
    )


def _t_section(depth: float, flange_width: float, web: float, flange: float) -> Ring:
    hb, hd, hw = flange_width / 2.0, depth / 2.0, web / 2.0
    return np.array(
        [(-hw, -hd), (hw, -hd), (hw, hd - flange), (hb, hd - flange), (hb, hd), (-hb, hd), (-hb, hd - flange), (-hw, hd - flange)], dtype=np.float64
    )


def _z_section(depth: float, flange_width: float, web: float, flange: float) -> Ring:
    hd, hw = depth / 2.0, web / 2.0
    reach = flange_width - hw
    return np.array(
        [
            (-reach, -hd),
            (hw, -hd),
            (hw, hd - flange),
            (reach, hd - flange),
            (reach, hd),
            (-hw, hd),
            (-hw, -hd + flange),
            (-reach, -hd + flange),
        ],
        dtype=np.float64,
    )


def _c_section(depth: float, width: float, wall: float, girth: float) -> Ring:
    hb, hd = width / 2.0, depth / 2.0
    return np.array(
        [
            (-hb, -hd),
            (hb, -hd),
            (hb, -hd + girth),
            (hb - wall, -hd + girth),
            (hb - wall, -hd + wall),
            (-hb + wall, -hd + wall),
            (-hb + wall, hd - wall),
            (hb - wall, hd - wall),
            (hb - wall, hd - girth),
            (hb, hd - girth),
            (hb, hd),
            (-hb, hd),
        ],
        dtype=np.float64,
    )


def _trapezium(bottom_x: float, top_x: float, ydim: float, top_offset: float) -> Ring:
    raw = np.array([(0.0, 0.0), (bottom_x, 0.0), (top_offset + top_x, ydim), (top_offset, ydim)], dtype=np.float64)
    return raw - (raw.min(axis=0) + raw.max(axis=0)) / 2.0


def _operator(operator: "ifcopenshell.entity_instance") -> Affine:
    axis = np.asarray(operator.Axis1.DirectionRatios if operator.Axis1 else (1.0, 0.0), dtype=np.float64)
    unit = axis / max(float(np.linalg.norm(axis)), _EPS)
    scale = float(operator.Scale) if operator.Scale is not None else 1.0
    return np.vstack([scale * np.array([unit, (-unit[1], unit[0])], dtype=np.float64), np.asarray(operator.LocalOrigin.Coordinates, dtype=np.float64)])


def _transformed(bodies: tuple[RingTuple, ...], affine: Affine) -> tuple[RingTuple, ...]:
    placed: Callable[[Ring], Ring] = lambda ring: ring @ affine[:2] + affine[2]
    return tuple((placed(outer), tuple(placed(void) for void in voids)) for outer, voids in bodies)


def _crossed(ring: Ring) -> bool:
    count = len(ring)
    tail = np.roll(ring, -1, axis=0)
    span = tail - ring
    to_vertex = ring[None, :, :] - ring[:, None, :]
    side = span[:, None, 0] * to_vertex[:, :, 1] - span[:, None, 1] * to_vertex[:, :, 0]
    straddle = side * np.roll(side, -1, axis=1) < 0.0
    lo, hi = np.minimum(ring, tail), np.maximum(ring, tail)
    within = np.all((ring[None, :, :] >= lo[:, None, :]) & (ring[None, :, :] <= hi[:, None, :]), axis=2)
    index = np.arange(count)
    incident = index[None, :] == index[:, None]
    lawful = incident | np.roll(incident, 1, axis=1)
    return bool(np.any(straddle & straddle.T) or np.any((side == 0.0) & within & ~lawful))


def _flaw(ring: Ring) -> "Option[CurveFlaw]":
    return (
        Some(CurveFlaw.MALFORMED)
        if not is_bearable(ring, ClosedRing)
        else Some(CurveFlaw.ZERO_AREA)
        if abs(_shoelace(ring)) <= _EPS
        else Some(CurveFlaw.SELF_INTERSECTS)
        if _crossed(ring)
        else Nothing
    )


def _wound(ring: Ring, sign: float) -> Ring:
    return np.ascontiguousarray(ring if _shoelace(ring) * sign > 0.0 else np.flip(ring, axis=0))


def _interior_point(ring: Ring) -> tuple[float, float]:
    levels = np.unique(ring[:, 1])
    height = float(levels[len(levels) // 2 - 1] + levels[len(levels) // 2]) / 2.0
    x, y = ring[:, 0], ring[:, 1]
    xn, yn = np.roll(x, -1), np.roll(y, -1)
    straddle = (y > height) != (yn > height)
    crossings = np.sort(x[straddle] + (height - y[straddle]) * (xn[straddle] - x[straddle]) / (yn[straddle] - y[straddle]))
    return float((crossings[0] + crossings[1]) / 2.0), height


def _facet_loop(
    acc: tuple["Block[tuple[float, float]]", "Block[tuple[int, int]]"], ring: Ring
) -> tuple["Block[tuple[float, float]]", "Block[tuple[int, int]]"]:
    points, facets = acc
    start, count = len(points), len(ring)
    coords = Block.of_seq(tuple(p) for p in ring.tolist())
    loop = Block.of_seq((start + i, start + (i + 1) % count) for i in range(count))
    return points.append(coords), facets.append(loop)


def _distributed(receipts: "Block[SectionReceipt]", composition: ScopeKey) -> "Block[SectionReceipt]":
    receipts.fold(lambda _, receipt: charter_record(STRUCTURAL_SUBJECT, receipt.measured, composition=composition), None)
    return receipts


def _extreme_fibers(rings: ProfileRings, centroid: tuple[float, float]) -> tuple[float, float]:
    cx, cy = centroid
    hull = np.concatenate(rings.outers)
    lo, hi = hull.min(axis=0), hull.max(axis=0)
    return (max(abs(float(hi[0]) - cx), abs(cx - float(lo[0]))), max(abs(float(hi[1]) - cy), abs(cy - float(lo[1]))))


def _deduped(ring: Ring) -> Ring:
    return ring[:-1] if len(ring) > 1 and bool(np.all(np.abs(ring[-1] - ring[0]) <= _EPS)) else ring


def _arc(points: Ring) -> Ring:
    (ax, ay), (bx, by), (cx, cy) = points
    area2 = (bx - ax) * (cy - ay) - (cx - ax) * (by - ay)

    def swept(center: Ring) -> Ring:
        spokes = points - center
        angles = np.arctan2(spokes[:, 1], spokes[:, 0])
        turn, mid = np.mod(angles[[2, 1]] - angles[0], 2.0 * np.pi)
        sweep = float(turn if mid < turn else turn - 2.0 * np.pi)
        theta = angles[0] + sweep * np.linspace(0.0, 1.0, max(int(CIRCLE_SEGMENTS * abs(sweep) / (2.0 * np.pi)), 2) + 1)
        return center + float(np.hypot(*spokes[0])) * np.stack([np.cos(theta), np.sin(theta)], axis=1)

    weights = np.array([[by - cy, cy - ay, ay - by], [cx - bx, ax - cx, bx - ax]], dtype=np.float64)
    return points if abs(area2) <= _EPS else swept(weights @ np.sum(points * points, axis=1) / (2.0 * area2))


def _segmented(coords: Ring, segments: tuple["ifcopenshell.entity_instance", ...]) -> "RuntimeRail[Ring]":
    def run(segment: "ifcopenshell.entity_instance") -> "RuntimeRail[Ring]":
        vertices = coords[np.asarray(segment.wrappedValue, dtype=np.intp) - 1]
        match _family(segment, SegmentKind):
            case Option(tag="some", some=SegmentKind.ARC):
                return Ok(_arc(vertices))
            case Option(tag="some", some=SegmentKind.LINE):
                return Ok(vertices)
            case Option(tag="none"):
                return Error(_domain(IfcFault(unrostered=(SegmentKind.__name__, segment.is_a()))))
            case _ as unreachable:
                assert_never(unreachable)

    return traversed(Block.of_seq(segments).map(run), by=Disposition.ACCUMULATE).map(
        lambda runs: np.concatenate([runs.head(), *(later[1:] for later in runs.tail())])
    )


def _curve(curve: "ifcopenshell.entity_instance") -> "RuntimeRail[Ring]":
    match _family(curve, CurveForm):
        case Option(tag="some", some=CurveForm.POLYLINE):
            return Ok(_deduped(np.ascontiguousarray([point.Coordinates for point in curve.Points], dtype=np.float64)))
        case Option(tag="some", some=CurveForm.INDEXED):
            coords = np.asarray(curve.Points.CoordList, dtype=np.float64)
            return _segmented(coords, curve.Segments).map(_deduped) if curve.Segments else Ok(_deduped(np.ascontiguousarray(coords)))
        case Option(
            tag="some",
            some=(
                CurveForm.OUTER_BOUNDARY
                | CurveForm.BOUNDARY
                | CurveForm.COMPOSITE_ON_SURFACE
                | CurveForm.COMPOSITE
                | CurveForm.RATIONAL_BSPLINE
                | CurveForm.BSPLINE
                | CurveForm.TRIMMED
            ) as form,
        ):
            return Error(_domain(IfcFault(unserved=(CurveForm.__name__, form.value))))
        case Option(tag="none"):
            return Error(_domain(IfcFault(unrostered=(CurveForm.__name__, curve.is_a()))))
        case _ as unreachable:
            assert_never(unreachable)


def _centerline(subject: str, path: Ring, thickness: float) -> "RuntimeRail[RingTuple]":
    poly = path[np.concatenate([[True], np.any(np.abs(np.diff(path, axis=0)) > _EPS, axis=1)])] if len(path) > 1 else path
    if len(poly) < 2:
        return Error(_domain(IfcFault(degenerate_measure=(subject, SectionMeasure.CENTRELINE_VERTICES, Some(float(len(poly)))))))
    spans = np.diff(poly, axis=0)
    lengths = np.hypot(spans[:, 0], spans[:, 1])
    units = spans / lengths[:, None]
    normals = np.stack([-units[:, 1], units[:, 0]], axis=1)
    prior, following = normals[:-1], normals[1:]
    turned = 1.0 + np.sum(prior * following, axis=1)
    spread = np.maximum(turned, _EPS)
    crosses = np.abs(units[:-1, 0] * units[1:, 1] - units[:-1, 1] * units[1:, 0])
    retraction = (thickness / 2.0) * crosses / spread
    flawed = Block.of_seq(
        (flaw, (vertex + 1,))
        for flaw, hits in (
            (CurveFlaw.REVERSAL, turned <= _EPS),
            (CurveFlaw.OFFSET_SELF_INTERSECTS, (retraction >= np.minimum(lengths[:-1], lengths[1:])) & (turned > _EPS)),
        )
        for vertex in np.flatnonzero(hits).tolist()
    )
    miters = np.concatenate([normals[:1], (prior + following) / spread[:, None], normals[-1:]])
    sides = poly + (thickness / 2.0) * np.stack([miters, -miters])
    return (
        Ok((np.ascontiguousarray(np.concatenate([sides[0], sides[1][::-1]])), ()))
        if flawed.is_empty()
        else Error(_domain(IfcFault(flawed_curve=(subject, tuple(flawed)))))
    )


class IfcStructural:
    @staticmethod
    def run(
        model: "ifcopenshell.file", tier: EnrichmentTier, spec: str, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[Block[SectionReceipt]]":
        return evidence_run(
            EvidenceScope.IFC_SECTION,
            f"run.{tier.name.lower()}",
            lambda: IfcStructural._dispatch(model, tier, spec).map(lambda receipts: _distributed(receipts, composition)),
            composition=composition,
        )

    @staticmethod
    @railed
    def _dispatch(model: "ifcopenshell.file", tier: EnrichmentTier, spec: str) -> "Block[SectionReceipt]":
        selector, _, model_guid = spec.partition("#")
        selection = yield from IfcSelector.filter(model, selector)
        elements = yield from IfcStructural._matched(selector, selection.elements)
        groups = IfcStructural._grouped(elements)
        return (
            yield from traversed(
                groups.map(lambda group: IfcStructural._section(model, tier, selection.query.filter_string, model_guid, group)),
                by=Disposition.ACCUMULATE,
            )
        )

    @staticmethod
    def _matched(subject: str, elements: tuple["ifcopenshell.entity_instance", ...]) -> "RuntimeRail[Block[ifcopenshell.entity_instance]]":
        matched = Block.of_seq(elements)
        return Ok(matched) if not matched.is_empty() else Error(_domain(IfcFault(empty_roster=(subject, IfcRoster.PROFILE_ELEMENT))))

    @staticmethod
    def _grouped(elements: "Block[ifcopenshell.entity_instance]") -> "Block[ProfileGroup]":
        def step(held: "Map[int, ProfileGroup]", element: "ifcopenshell.entity_instance") -> "Map[int, ProfileGroup]":
            profile = IfcStructural._profile(element)
            prior = held.try_find(profile.id()).map(lambda group: group.subjects).default_value(())
            return held.add(profile.id(), ProfileGroup(profile=profile, subjects=(*prior, element.GlobalId)))

        return Block.of_seq(elements.fold(step, Map.empty()).items()).map(lambda row: row[1])

    @staticmethod
    @railed
    def _section(
        model: "ifcopenshell.file", tier: EnrichmentTier, validated: str, model_guid: str, group: ProfileGroup
    ) -> "SectionReceipt":
        identity = f"{tier.name.lower()}|{validated}|{group.profile.is_a()}#{group.profile.id()}"
        rings = yield from IfcStructural._rings(group.profile)
        spine: SectionReceipt = yield from IfcStructural._integrate(rings, group, identity)
        match tier:
            case EnrichmentTier.CORE:
                return spine
            case EnrichmentTier.IFC_ENTITY:
                enrichment = yield from IfcStructural._entity(model, identity, model_guid)
                return replace(spine, tier=tier, subjects=enrichment.entity, enrichment=Some(enrichment))
            case EnrichmentTier.WARPING:
                enrichment = yield from IfcStructural._warping(rings)
                return replace(spine, tier=tier, enrichment=Some(enrichment))
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def _entity(model: "ifcopenshell.file", subject: str, model_guid: str) -> "RuntimeRail[Enrichment]":
        model_node = model.by_guid(model_guid)
        members = Block.of_seq(
            member.GlobalId
            for rel in (model_node.IsGroupedBy or ())
            if rel.is_a("IfcRelAssignsToGroup")
            for member in rel.RelatedObjects
            if member.is_a("IfcStructuralMember")
        )
        return Ok(Enrichment(entity=tuple(members))) if members else Error(_domain(IfcFault(empty_roster=(subject, IfcRoster.STRUCTURAL_MEMBER))))

    @staticmethod
    def _warping(rings: ProfileRings) -> "RuntimeRail[Enrichment]":
        def solve() -> Enrichment:
            seed: tuple[Block[tuple[float, float]], Block[tuple[int, int]]] = (Block.empty(), Block.empty())
            points, facets = Block.of_seq(rings.rings).fold(_facet_loop, seed)
            holes = [_interior_point(void) for void in rings.voids]
            geom = spp.CompoundGeometry.from_points(
                list(points), list(facets), [_interior_point(outer) for outer in rings.outers], holes or None
            )
            section = spa.Section(geom.create_mesh([body.extent / 100.0 for body in rings.bodies]))
            section.calculate_geometric_properties()
            section.calculate_warping_properties()
            section.calculate_plastic_properties()
            return Enrichment(
                warping=WarpingEvidence(
                    fe_torsion_constant=float(section.get_j()),
                    fe_area=float(section.get_area()),
                    shear_center=tuple(section.get_sc()),
                    shear_areas=tuple(section.get_as()),
                    plastic_moduli=tuple(section.get_s()),
                    mesh_elements=len(section.elements),
                )
            )

        return boundary(SECTION_WARPING, solve, catch=_FE_RAISES)

    @staticmethod
    def _rings(profile: "ifcopenshell.entity_instance") -> "RuntimeRail[ProfileRings]":
        return IfcStructural._sample(profile).bind(lambda bodies: IfcStructural._admitted(profile, bodies))

    @staticmethod
    def _admitted(profile: "ifcopenshell.entity_instance", bodies: tuple[RingTuple, ...]) -> "RuntimeRail[ProfileRings]":
        flawed = (
            Block.of_seq(
                (at, index, ring) for at, (outer, voids) in enumerate(bodies) for index, ring in enumerate((outer, *voids))
            )
            .choose(lambda cell: _flaw(cell[2]).map(lambda flaw: (flaw, (cell[0], cell[1]))))
        )
        return (
            Ok(ProfileRings(bodies=tuple(ProfileBody(outer=_wound(outer, 1.0), voids=tuple(_wound(v, -1.0) for v in voids)) for outer, voids in bodies)))
            if flawed.is_empty()
            else Error(_domain(IfcFault(flawed_curve=(profile.is_a(), tuple(flawed)))))
        )

    @staticmethod
    def _sample(profile: "ifcopenshell.entity_instance") -> "RuntimeRail[tuple[RingTuple, ...]]":
        match _family(profile, ProfileFamily):
            case Option(tag="some", some=ProfileFamily.COMPOSITE):
                return IfcStructural._composed(profile)
            case Option(tag="some", some=ProfileFamily.MIRRORED):
                return IfcStructural._sample(profile.ParentProfile).map(lambda bodies: _transformed(bodies, _MIRROR))
            case Option(tag="some", some=ProfileFamily.DERIVED):
                return IfcStructural._sample(profile.ParentProfile).map(lambda bodies: _transformed(bodies, _operator(profile.Operator)))
            case Option(tag="some", some=ProfileFamily.CENTERLINE):
                return IfcStructural._centered(profile)
            case Option(tag="some", some=(ProfileFamily.ARBITRARY_VOIDS | ProfileFamily.ARBITRARY) as family):
                return IfcStructural._arbitrary(profile, family)
            case Option(tag="some", some=ProfileFamily.PARAMETERIZED):
                return IfcStructural._parametric(profile)
            case Option(tag="some", some=ProfileFamily.ARBITRARY_OPEN as family):
                return Error(_domain(IfcFault(unserved=(ProfileFamily.__name__, family.value))))
            case Option(tag="none"):
                return Error(_domain(IfcFault(unrostered=(ProfileFamily.__name__, profile.is_a()))))
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def _composed(profile: "ifcopenshell.entity_instance") -> "RuntimeRail[tuple[RingTuple, ...]]":
        return traversed(Block.of_seq(profile.Profiles or ()).map(IfcStructural._sample), by=Disposition.ACCUMULATE).map(
            lambda nested: tuple(body for bodies in nested for body in bodies)
        )

    @staticmethod
    def _centered(profile: "ifcopenshell.entity_instance") -> "RuntimeRail[tuple[RingTuple, ...]]":
        raw = getattr(profile, "Thickness", None)
        declared = Some(float(raw)) if isinstance(raw, (int, float)) else Nothing
        width = declared.default_value(0.0)
        return (
            _curve(profile.Curve).bind(lambda path: _centerline(profile.is_a(), path, width)).map(lambda body: (body,))
            if width > _EPS
            else Error(_domain(IfcFault(degenerate_measure=(profile.is_a(), SectionMeasure.PROFILE_THICKNESS, declared))))
        )

    @staticmethod
    def _arbitrary(profile: "ifcopenshell.entity_instance", family: ProfileFamily) -> "RuntimeRail[tuple[RingTuple, ...]]":
        voids = tuple(profile.InnerCurves or ()) if family is ProfileFamily.ARBITRARY_VOIDS else ()
        return traversed(Block.of_seq((profile.OuterCurve, *voids)).map(_curve), by=Disposition.ACCUMULATE).map(
            lambda rings: ((rings.head(), tuple(rings.tail())),)
        )

    @staticmethod
    def _parametric(profile: "ifcopenshell.entity_instance") -> "RuntimeRail[tuple[RingTuple, ...]]":
        return (
            _family(profile, ParametricProfile)
            .to_result(_domain(IfcFault(unrostered=(ParametricProfile.__name__, profile.is_a()))))
            .bind(lambda leaf: IfcStructural._dimensioned(profile, leaf, PROFILE_SAMPLERS[leaf]))
        )

    @staticmethod
    def _dimensioned(profile: "ifcopenshell.entity_instance", leaf: ParametricProfile, row: _ProfileRow) -> "RuntimeRail[tuple[RingTuple, ...]]":
        names, _waived, build = row
        resolved = Block.of_seq(names).map(lambda name: (name, getattr(profile, name, None)))
        absent = resolved.choose(lambda pair: Some(pair[0]) if not isinstance(pair[1], (int, float)) else Nothing)
        return (
            Ok((build(tuple(float(value) for _, value in resolved)),))
            if absent.is_empty()
            else Error(_domain(IfcFault(unresolved_slots=(leaf.value, tuple(absent)))))
        )

    @staticmethod
    def _profile(element: "ifcopenshell.entity_instance") -> "ifcopenshell.entity_instance":
        if element.is_a("IfcProfileDef"):
            return element
        return (
            Block
            .of_seq(element.HasAssociations or ())
            .choose(lambda d: Some(d.RelatingMaterial) if d.is_a("IfcRelAssociatesMaterial") else Nothing)
            .choose(IfcStructural._profile_of_material)
            .try_head()
            .default_with(lambda: element)
        )

    @staticmethod
    def _profile_of_material(material: "ifcopenshell.entity_instance") -> "Option[ifcopenshell.entity_instance]":
        match _family(material, MaterialProfileSource):
            case Option(tag="some", some=MaterialProfileSource.PROFILE_SET):
                return Some(material.CompositeProfile or material.MaterialProfiles[0].Profile)
            case Option(tag="some", some=(MaterialProfileSource.PROFILE_SET_USAGE_TAPERING | MaterialProfileSource.PROFILE_SET_USAGE)):
                return Some(material.ForProfileSet.CompositeProfile or material.ForProfileSet.MaterialProfiles[0].Profile)
            case Option(tag="some", some=(MaterialProfileSource.PROFILE_WITH_OFFSETS | MaterialProfileSource.PROFILE)):
                return Some(material.Profile)
            case (
                Option(
                    tag="some",
                    some=(
                        MaterialProfileSource.LAYER_SET_USAGE
                        | MaterialProfileSource.CONSTITUENT_SET
                        | MaterialProfileSource.CONSTITUENT
                        | MaterialProfileSource.LAYER_WITH_OFFSETS
                        | MaterialProfileSource.LAYER_SET
                        | MaterialProfileSource.LAYER
                        | MaterialProfileSource.LIST
                        | MaterialProfileSource.MATERIAL
                    ),
                )
                | Option(tag="none")
            ):
                return Nothing
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def _moments(rings: ProfileRings) -> dict[str, float]:
        edges = tuple(
            (x, y, np.roll(x, -1), np.roll(y, -1), x * np.roll(y, -1) - np.roll(x, -1) * y)
            for ring in rings.rings
            for x, y in ((ring[:, 0], ring[:, 1]),)
        )
        return {
            name: sum(float(np.sum(weight(x, y, xn, yn) * cross)) / divisor for x, y, xn, yn, cross in edges)
            for name, weight, divisor in MOMENT_KERNELS
        }

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _integrate(rings: ProfileRings, group: ProfileGroup, spec: str) -> "RuntimeRail[SectionReceipt]":
        moments = IfcStructural._moments(rings)
        a, qx, qy, ixx, iyy, ixy = (moments[key] for key in ("a", "qx", "qy", "ixx", "iyy", "ixy"))

        def assembled(area: float) -> SectionReceipt:
            cx, cy = qy / area, qx / area
            ixx_c, iyy_c, ixy_c = ixx - area * cy * cy, iyy - area * cx * cx, ixy - area * cx * cy
            principal, vectors = np.linalg.eigh(np.array([[ixx_c, -ixy_c], [-ixy_c, iyy_c]], dtype=np.float64))
            major = int(np.argmax(principal))
            perimeter = sum(float(np.sum(np.linalg.norm(np.diff(np.vstack([r, r[:1]]), axis=0), axis=1))) for r in rings.rings)
            cx_fibre, cy_fibre = _extreme_fibers(rings, (cx, cy))
            return SectionReceipt(
                tier=EnrichmentTier.CORE,
                subjects=group.subjects,
                spec=spec,
                area=area,
                centroid=(cx, cy),
                second_moments=(ixx_c, iyy_c, ixy_c),
                principal_moments=(float(principal[major]), float(principal[1 - major])),
                principal_angle=float(np.arctan2(vectors[1, major], vectors[0, major])),
                polar_moment=ixx_c + iyy_c,
                torsion_constant=4.0 * area * area / max(perimeter, _EPS),
                section_moduli=(ixx_c / max(cy_fibre, _EPS), iyy_c / max(cx_fibre, _EPS)),
            )

        return (
            Ok(assembled(a))
            if a > _EPS
            else Error(_domain(IfcFault(degenerate_measure=(group.profile.is_a(), SectionMeasure.SECTION_AREA, Some(a)))))
        )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
