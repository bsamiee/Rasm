# [PY_GEOMETRY_IFC_STRUCTURAL]

One cross-section structural-property owner — section-integral and structural-member verbs the analysis and lifecycle hops drop. `IfcStructural` partitions a profile-bearing selector match by RESOLVED profile identity and folds one closed-form section receipt per distinct section: a `numpy` Green's-theorem contour fold over the `MOMENT_KERNELS` weight table (area, first and second area moments, centroid, principal second moments and principal-axis rotation, polar moment, centroid-relative elastic section moduli, thin-walled Bredt torsion constant) runs over each profile's body set, then two gated enrichment layers tier that spine under one `EnrichmentTier` policy: the `ifcopenshell` layer reads `IfcStructuralAnalysisModel`/`IfcStructuralMember` topology onto the member, the `sectionproperties` layer meshes the same rings into a triangular FE section for the warping, plastic, and shear receipts no closed-form integral derives. C#'s `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the numerical section dimension the managed projection does not produce.

A profile is a BODY SET, never one outer ring plus voids: the IFC4 `IfcCompositeProfileDef`, `IfcDerivedProfileDef`, and `IfcMirroredProfileDef` families all resolve to several disjoint regions, so `ProfileRings` carries `ProfileBody` rows and the ring-additive contour fold absorbs them with no second engine. Both enrichments are one `Enrichment` `@tagged_union` on a single `SectionReceipt.enrichment` field — the `entity` case carrying the structural-member GlobalIds, the `warping` case the FE `WarpingEvidence` value object — so a `CORE` receipt carries `Nothing`, never a `None`-slot bag racing the tier. Every profile-bearing query admits through `IfcSelector.filter`/`IfcSelector.parse` from `ifc/selector#SELECTOR` (the one selection engine, the only `filter_elements` caller), a `<selector>#<analysis-model-guid>` split feeding the member rings and the structural-model guid, so a malformed profile selector is a typed `Error(BoundaryFault)` at admission. Whole admission rides the graduation `evidence_run` weave under `EvidenceScope.IFC_SECTION`, so a provider exception, a degenerate ring, an unsupported subtype, and an FE divergence each fold onto one `RuntimeRail[Block[SectionReceipt]]`. Receipt graduates as `GeometrySubject.SECTION_PROPERTY` carried by the `STRUCTURAL_SUBJECT` constant — the section-integral evidence class distinct from the compliance and lifecycle members its siblings bind — and `graduates()` returns the local `GeometryHandoff` whose `wire()` projection is the compute crossing. Spine rides the bare runtime (`numpy`) and never depends on either gated layer; every tier runs caller-floor by charter — the live `ifcopenshell.file` is a pybind11 handle no pickle seam carries and one per-profile FE solve is short-bounded work — with the `ifcopenshell` and `sectionproperties` bands (native mesh backend `cytriangle`, LGPLv3) bound function-local at their tier gates, so a `CORE` run loads neither.

## [01]-[INDEX]

- [02]-[STRUCTURAL]: profile-partitioned section-integral spine and two gated enrichment tiers under one `EnrichmentTier` owner folding per-tier evidence into one `Enrichment` union, woven through the `evidence_run` graduation weave, emitting the `SECTION_PROPERTY` subject.

## [02]-[STRUCTURAL]

- Owner: `IfcStructural` — boundary capsule over the section-integral spine and two gated enrichment tiers; `EnrichmentTier` discriminant and `Enrichment` evidence shape are the same union, so each tier is one `_section` builder arm, never a sibling per-tier class. `ProfileRings`/`ProfileBody` is the one geometric input owner: `ProfileBody` a CCW outer ring with its CW voids, `ProfileRings` the flat body set every profile family flattens onto, so the contour fold, the FE region set, and the extreme-fibre reach all read one shape.
- Cases: `CORE` folds the closed-form section-integral receipt on the bare runtime interpreter; `IFC_ENTITY` adds the `IfcStructuralAnalysisModel`/`IfcStructuralMember` topology behind its `ifcopenshell` tier gate; `WARPING` adds the FE warping/plastic/shear receipts behind its `sectionproperties` tier gate. Spine never depends on either gated layer — upper tiers add evidence only where their gated package resolves.
- Entry: `IfcStructural.run` takes an `ifcopenshell.file`, an `EnrichmentTier`, a `spec` whose meaning the tier fixes — a `<selector>` profile-bearing query for `CORE`/`WARPING` resolving each element's `IfcProfileDef` rings off its material-profile assignment, a `<selector>#<analysis-model-guid>` query for `IFC_ENTITY` joining the members to their structural-analysis model — and the `composition` custody key, returning `RuntimeRail[Block[SectionReceipt]]`: one receipt per DISTINCT resolved profile, never one head section published under every matched GlobalId. Graduation stays the caller's own step on `SectionReceipt.graduates(ceiling)`, which derives its own `ContentKey` from the receipt's `spec` through the spine's `evidence_key`, so no caller mints a key for evidence it did not produce. `subjects` derives from the group's true subject set — the group's profile-bearing GlobalIds for `CORE`/`WARPING`, structural-member GlobalIds for `IFC_ENTITY`.
- Auto: `_dispatch` binds the `IfcSelector` admission, the `_matched` empty-match gate, and the `_grouped` partition, then `traversed(..., ACCUMULATE)` folds one `_section` builder per group so ONE refusal names every divergent profile rather than one run per defect. `_grouped` keys on the resolved profile entity's own step id through the ordered `Map`, so group order — and every receipt, charter sample, and frame row derived from it — is deterministic. `_sample` resolves three families narrowest-first: a composite or transformed profile folds its children through the same entry (nesting FLATTENS to one body list, mirroring and the `IfcCartesianTransformationOperator2D` affine applying to the parent's rings), a parametric subtype resolves its row's declared `dimensions` roster and builds one body from `PROFILE_SAMPLERS`, and the arbitrary-closed curve read is the fall-through — voids subtype-gated on `IfcArbitraryProfileDefWithVoids`, which is the entity that DECLARES `InnerCurves`. An uncovered subtype names itself, a missing dimension names itself, and an indexed poly-curve carrying `Segments` refuses rather than chording its arcs. `_rings` is the one admission fold over every ring of every body — `ClosedRing` refinement, non-degenerate signed area, then winding normalization to CCW outer and CW void — so the contour integral reads the sign off the vertex order with no `signed` multiplier column and the area divisor is non-zero BY ADMISSION. `_entity` folds the `IsGroupedBy`/`IfcRelAssignsToGroup`-guarded `IfcStructuralMember` set — entity topology only, never re-deriving a section property, since the centroid-relative elastic section moduli are a closed-form spine field every tier carries. `_warping` builds one `pre.CompoundGeometry.from_points` body set — one control point per body outer, one hole marker per void, one closed facet loop per ring in outers-then-voids order, per-body mesh area off `ProfileBody.extent` — runs geometric→warping→plastic in the prerequisite order, and reads `get_area` back to cross-check the `numpy` spine area (the `fe-area` residual); the FE torsion lands on `WarpingEvidence.fe_torsion_constant`, never overwriting the spine's thin-walled `torsion_constant`.
- Receipt: `SectionReceipt` conforms structurally to `ReceiptContributor` — one `row` projection is the single column roster the receipt facts, the frame columns, and the residual ledger all read, so a new integral reaches all three through one field. `contribute` emits that row per group; `graduates` folds the tier-aware `measured` ledger onto `GeometryHandoff.of(STRUCTURAL_SUBJECT, ...)` rather than inlining a ceiling comparison; `framed` projects the whole run as ONE `EvidenceFrame` with one row per group, so the columnar egress carries the real section-to-section spread a board cuts on; and `_distributed` records the `ring-closure` residual as the `rasm.geometry.section.closure` charter measure per group through the graduation `charter_record` derivation at the producing fold, so a six-section partition records six samples where a head-only fold publishes one. `measured` ledger is data-driven by tier — the `ring-closure` residual (polar moment vs principal sum) every tier, and the `WARPING` `fe-area` FE-convergence residual — so a degenerate profile or a diverging FE mesh graduates as an `Error(BoundaryFault)`, never a clean section receipt.
- Packages: `numpy` (the shoelace contour fold over `MOMENT_KERNELS`, `linalg.eigh` for the major-axis principal solution, `linspace`/`stack`/`concatenate` for the curved and rounded subtype polylines, `unique` and the roll-straddle crossing solve for the `_interior_point` ray scan, `ascontiguousarray` normalizing every admitted ring); `expression` (the `railed` `effect.result` rails, `Block` folds for the group partition, member set, and facet loops, ordered `Map` for the profile index, the `Enrichment` union, `Option` rail lifts); `beartype` (the `ClosedRing` `Is` refinement proved through `is_bearable` at admission, `FAULT_CONF` on the `_integrate` numeric leaf); geometry graduation (`evidence_run`/`GeometryHandoff`/`GeometrySubject`/`EvidenceFrame`, `charter_record` the charter measure authority, `evidence_key` the receipt-spec key derivation); `ifc/selector#SELECTOR` (`IfcSelector.filter`/`parse` — the only `filter_elements` caller); `ifcopenshell` (the `IfcProfileDef` family attributes, `entity_instance.id()` the partition key, `IfcStructuralAnalysisModel`/`IfcStructuralMember` topology over the in-process model, `CORE` reading only the profile); `sectionproperties` (`WARPING` tier only, `pre.CompoundGeometry.from_points` the arity-polymorphic body-set surface, native mesh backend `cytriangle`, LGPLv3); runtime rails.
- Growth: a new section integral is one `MOMENT_KERNELS` row and one `SectionReceipt` field reaching the receipt, the frame, and the ledger through `row`; a new parametric profile subtype is one `PROFILE_SAMPLERS` row — its `is_a` key, its `dimensions` roster, and its ring constructor — the rings staying the universal input and the contour fold shape-agnostic, never a per-shape integral family; a new boundary-curve form is one `_curve` arm; a new enrichment tier is one `EnrichmentTier` row, one `Enrichment` case, and one `_section` arm; a new warping/plastic measure is one `WarpingEvidence` field and one `Section.get_*` accessor; a new selection axis is one `IfcSelector` grammar alternative, never a local query-parse fold; a stricter residual bar is one tighter ceiling row the caller supplies. `IfcCenterLineProfileDef` is the named next family — an open centreline offset to its `Thickness` — and a survivor/casualty partition is the named next disposition once a consumer needs the sections a mixed match CAN yield beside the refusals the accumulating fold names.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy (projected in-process); no durable store, Rhino/GH mutation, or mesh/GLB write — the `WARPING` FE section mesh is an in-memory `sectionproperties` artifact consumed for its scalars, never a `mesh/repair.md` payload write. `ifcopenshell`/`sectionproperties` import function-local at the tier-gated boundary per the manifest import policy, so a `CORE` run never loads a gated package. Deterministic `sectionproperties` solves own no transiency — a retry over it (a `stamina.retry` mint included) is a deleted form. Raw `spec` never threads past admission into `filter_elements` — `IfcSelector` re-serializes the validated query, the one selection engine. No sampler re-reads its entity, no fillet or slope attribute enters a polygonal idealization that cannot carry it, and no ring reaches the integral unwound, unproven, or non-contiguous.

```python signature
from collections.abc import Callable
from enum import IntEnum
from typing import TYPE_CHECKING, Annotated, Final, Literal, assert_never

import numpy as np
from beartype import beartype
from beartype.door import is_bearable
from beartype.vale import Is
from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from numpy.typing import NDArray

from rasm.geometry.graduation import (
    EvidenceFrame,
    EvidenceScope,
    GeometryHandoff,
    GeometrySubject,
    charter_record,
    evidence_key,
    evidence_run,
)
from rasm.geometry.ifc.selector import IfcSelector
from rasm.runtime.faults import FAULT_CONF, BoundaryFault, Disposition, RuntimeRail, boundary, railed, traversed
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey

if TYPE_CHECKING:  # function-local tier-boundary imports keep the CORE spine clean (manifest import policy)
    import ifcopenshell

# --- [TYPES] ---------------------------------------------------------------------------


class EnrichmentTier(IntEnum):
    CORE = 0
    IFC_ENTITY = 1
    WARPING = 2


type Ring = NDArray[np.float64]
type RingTuple = tuple[Ring, tuple[Ring, ...]]
type Affine = NDArray[np.float64]  # 3x2 row-vector transform: rows [U1, U2, origin]
type _Moment = Callable[[Ring, Ring, Ring, Ring], Ring]
type _Sampler = Callable[[tuple[float, ...]], RingTuple]

# ring admission contract `_rings` proves with `is_bearable` over every ring of every body: ≥3 finite vertices. It is a
# SHAPE proof only — three collinear points satisfy it — so the paired `_flaw` area probe carries the degeneracy the
# refinement cannot express, and the two together are what make the centroid divide total.
type ClosedRing = Annotated[Ring, Is[lambda r: r.ndim == 2 and r.shape[0] >= 3 and bool(np.isfinite(r).all())]]

# --- [CONSTANTS] -----------------------------------------------------------------------

# SECTION_PROPERTY graduation member; an unlisted subject fails at the boundary under `ty`.
STRUCTURAL_SUBJECT: Final[GeometrySubject] = GeometrySubject.SECTION_PROPERTY

# polyline fidelity of the curved parametric subtypes — the one tessellation policy a caller may sharpen. A rounded
# corner takes a quarter of it, so the whole-turn budget and the corner budget stay one number.
CIRCLE_SEGMENTS: Final[int] = 64

# denormal floor for the section-modulus and residual denominators: a max(x, 1.0) clamp corrupts a
# sub-unit section (a 0.3 m fibre reads as 1.0), so the floor guards the genuine zero alone.
_EPS: Final[float] = 1e-12

# IfcMirroredProfileDef's operator is a DERIVED attribute the schema fixes to a y-axis mirror, so the transform is
# spelled here rather than read off a slot the wrapper need not surface; winding flips and `_rings` re-normalizes it.
_MIRROR: Final[Affine] = np.array([(-1.0, 0.0), (0.0, 1.0), (0.0, 0.0)], dtype=np.float64)

# Green's-theorem contour-moment table: each row maps a ring to one origin moment as
# `sum(weight(x, y, xn, yn) * cross) / divisor` — six integrals as one data-driven projection.
MOMENT_KERNELS: Final[tuple[tuple[str, _Moment, float], ...]] = (
    ("a", lambda x, y, xn, yn: np.ones_like(x), 2.0),
    ("qx", lambda x, y, xn, yn: y + yn, 6.0),
    ("qy", lambda x, y, xn, yn: x + xn, 6.0),
    ("ixx", lambda x, y, xn, yn: y * y + y * yn + yn * yn, 12.0),
    ("iyy", lambda x, y, xn, yn: x * x + x * xn + xn * xn, 12.0),
    ("ixy", lambda x, y, xn, yn: x * yn + 2.0 * x * y + 2.0 * xn * yn + xn * y, 24.0),
)

# Parametric-subtype table over the WHOLE IFC4 `IfcParameterizedProfileDef` family: each row is
# `(ifc_class, dimensions, constructor)`, `dimensions` naming the attributes the constructor consumes IN ORDER so
# `_dimensioned` resolves them once, refuses a missing one BY NAME, and no constructor re-reads the entity. Narrowest
# first: hollow and rounded rows precede the supertypes they specialize, and the asymmetric I precedes the symmetric
# one because IFC2X3 seats it as that subtype. Rows spell IFC4 attribute names; an attribute the running schema does
# not declare reads absent through the same gate rather than raising. Fillet, edge, and slope attributes are
# deliberately unread — a polygonal idealization cannot carry them, and reading one would imply a fidelity it lacks.
PROFILE_SAMPLERS: Final[tuple[tuple[str, tuple[str, ...], _Sampler], ...]] = (
    ("IfcRectangleHollowProfileDef", ("XDim", "YDim", "WallThickness"), lambda d: _box(d[0], d[1], d[2])),
    ("IfcRoundedRectangleProfileDef", ("XDim", "YDim", "RoundingRadius"), lambda d: (_rect(d[0], d[1], d[2]), ())),
    ("IfcRectangleProfileDef", ("XDim", "YDim"), lambda d: (_rect(d[0], d[1], 0.0), ())),
    ("IfcCircleHollowProfileDef", ("Radius", "WallThickness"), lambda d: (_ellipse(d[0], d[0]), (_ellipse(d[0] - d[1], d[0] - d[1]),))),
    ("IfcCircleProfileDef", ("Radius",), lambda d: (_ellipse(d[0], d[0]), ())),
    ("IfcEllipseProfileDef", ("SemiAxis1", "SemiAxis2"), lambda d: (_ellipse(d[0], d[1]), ())),
    (
        "IfcAsymmetricIShapeProfileDef",
        ("BottomFlangeWidth", "OverallDepth", "WebThickness", "BottomFlangeThickness", "TopFlangeWidth", "TopFlangeThickness"),
        lambda d: (_i_section(*d), ()),
    ),
    ("IfcIShapeProfileDef", ("OverallWidth", "OverallDepth", "WebThickness", "FlangeThickness"), lambda d: (_i_section(d[0], d[1], d[2], d[3], d[0], d[3]), ())),
    ("IfcLShapeProfileDef", ("Depth", "Width", "Thickness"), lambda d: (_l_section(*d), ())),
    ("IfcUShapeProfileDef", ("Depth", "FlangeWidth", "WebThickness", "FlangeThickness"), lambda d: (_u_section(*d), ())),
    ("IfcTShapeProfileDef", ("Depth", "FlangeWidth", "WebThickness", "FlangeThickness"), lambda d: (_t_section(*d), ())),
    ("IfcZShapeProfileDef", ("Depth", "FlangeWidth", "WebThickness", "FlangeThickness"), lambda d: (_z_section(*d), ())),
    ("IfcCShapeProfileDef", ("Depth", "Width", "WallThickness", "Girth"), lambda d: (_c_section(*d), ())),
    ("IfcTrapeziumProfileDef", ("BottomXDim", "TopXDim", "YDim", "TopXOffset"), lambda d: (_trapezium(*d), ())),
)

# --- [MODELS] --------------------------------------------------------------------------


# FE payload as one value object, carried only by Enrichment.warping.
class WarpingEvidence(Struct, frozen=True, gc=False):
    fe_torsion_constant: float
    fe_area: float
    shear_center: tuple[float, float]
    shear_areas: tuple[float, float]
    plastic_moduli: tuple[float, float]
    mesh_elements: int


# tier discriminant and evidence shape are one union.
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
    # one closed region: a CCW outer ring and its CW voids, both wound and proven at `_rings`, so every consumer
    # reads the sign off the vertex order rather than a multiplier travelling beside the ring.
    outer: ClosedRing
    voids: tuple[ClosedRing, ...]

    @property
    def extent(self) -> float:
        # net material area of THIS body — the FE mesh-size budget per region, derived off the one shoelace owner so a
        # composite of very different bodies meshes each to its own scale rather than the whole section's.
        return abs(_shoelace(self.outer)) - sum(abs(_shoelace(void)) for void in self.voids)


class ProfileRings(Struct, frozen=True, gc=False):
    # a profile is a BODY SET: composite, derived, and mirrored IFC4 profiles all resolve to several disjoint regions,
    # and the contour integral is ring-additive, so the widened owner absorbs them with no second engine.
    bodies: tuple[ProfileBody, ...]

    @property
    def rings(self) -> tuple[Ring, ...]:
        # outers then voids — the facet order `CompoundGeometry.from_points` documents, and the order the contour
        # fold is indifferent to, so one projection serves the integral and the FE boundary alike.
        return (*self.outers, *self.voids)

    @property
    def outers(self) -> tuple[Ring, ...]:
        return tuple(body.outer for body in self.bodies)

    @property
    def voids(self) -> tuple[Ring, ...]:
        return tuple(void for body in self.bodies for void in body.voids)


class ProfileGroup(Struct, frozen=True, gc=False):
    # one partition cell: the resolved profile entity and every GlobalId whose element resolves to it.
    profile: "ifcopenshell.entity_instance"
    subjects: tuple[str, ...]


class SectionReceipt(Struct, frozen=True, gc=False):
    tier: EnrichmentTier
    subjects: tuple[str, ...]
    # run identity of THIS group — tier, admitted spec, and the resolved profile's class and step id — from which
    # `graduates`/`framed` derive their own `ContentKey`, so no caller mints a key for evidence it did not produce.
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
        # relative residuals, so the denominator floors at _EPS, never 1.0.
        ledger = {"ring-closure": abs(self.polar_moment - sum(self.principal_moments)) / max(abs(self.polar_moment), _EPS)}
        match self.enrichment:
            case Option(tag="some", some=Enrichment(tag="warping", warping=fe)):
                return ledger | {"fe-area": abs(fe.fe_area - self.area) / max(self.area, _EPS)}
            case _:
                return ledger

    @property
    def row(self) -> dict[str, object]:
        # ONE column roster the receipt facts, the frame columns, and the tier residual ledger all read, so a new
        # integral is one field reaching all three rather than three parallel projections drifting apart.
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
        return Block.singleton(Receipt.of("rasm.geometry.ifc.structural", ("emitted", STRUCTURAL_SUBJECT, self.row)))

    def graduates(self, ceiling: dict[str, float]) -> GeometryHandoff:
        # carrier's residual-over-ceiling `admitted` verdict gates; wire() is the compute crossing. The key derives
        # from this receipt's own `spec` through the spine, so the crossing is content-addressed by what produced it.
        return GeometryHandoff.of(STRUCTURAL_SUBJECT, evidence_key(STRUCTURAL_SUBJECT, self.spec), self.measured, ceiling)

    @staticmethod
    def framed(receipts: "Block[SectionReceipt]") -> "RuntimeRail[EvidenceFrame]":
        # ONE frame per run, one row per resolved profile group — the partition's real table, where a per-receipt
        # frame publishes a single row and loses the section-to-section spread a board cuts on. Column order is the
        # first row's roster; a group whose ledger keys differ (the WARPING `fe-area` residual on some bodies and not
        # others) rails on the port's own width check, naming its offending COLUMNS at this producer. The run key
        # folds every group's `spec` in the partition's deterministic order, so it addresses the run, not one section.
        names = tuple(receipts.head().row) if not receipts.is_empty() else ()
        table: dict[str, list[object]] = {name: [receipt.row[name] for receipt in receipts] for name in names}
        return EvidenceFrame.of(STRUCTURAL_SUBJECT, evidence_key(STRUCTURAL_SUBJECT, "|".join(receipt.spec for receipt in receipts)), table)


# --- [OPERATIONS] ----------------------------------------------------------------------

# Closed float64 rings in profile-local axes, centred on the bounding box per the IfcParameterizedProfileDef
# convention and wound CCW, that the contour integral reads with no shape branch. Every constructor takes resolved
# scalars — `_dimensioned` already proved them present — so none touches an `entity_instance`.


def _shoelace(ring: Ring) -> float:
    # signed contour area: the sign IS the winding, so `_rings` normalizes orientation off it and every downstream
    # integral, extent, and residual reads that sign from the vertex order with no multiplier column beside the ring.
    x, y = ring[:, 0], ring[:, 1]
    return float(np.sum(x * np.roll(y, -1) - np.roll(x, -1) * y)) / 2.0


def _rect(xdim: float, ydim: float, radius: float) -> Ring:
    # rectangle and rounded rectangle are one constructor: the radius clamps to the half-extents, and a sub-epsilon
    # radius returns the sharp four-vertex loop rather than four coincident arc fans the triangulator rejects.
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
    # circle and ellipse are one constructor — a circle passes its radius twice, so the curved family costs one row.
    theta = np.linspace(0.0, 2.0 * np.pi, CIRCLE_SEGMENTS, endpoint=False)
    return np.stack([semi_x * np.cos(theta), semi_y * np.sin(theta)], axis=1).astype(np.float64)


def _box(xdim: float, ydim: float, wall: float) -> RingTuple:
    # a hollow rectangle whose wall the dimension gate proved present; a model omitting it refuses by name rather than
    # degrading to a solid section, which would publish a section modulus several times the member's real one.
    return _rect(xdim, ydim, 0.0), (_rect(xdim - 2.0 * wall, ydim - 2.0 * wall, 0.0),)


def _i_section(bottom_width: float, depth: float, web: float, bottom_flange: float, top_width: float, top_flange: float) -> Ring:
    # I/H outline, both flanges centred on the y axis so the bounding-box origin holds for either row: the symmetric
    # subtype feeds its one width and thickness twice, the asymmetric one its own four — one topology, two rows.
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
    # channel opening +x: web on the left, both flanges reaching right, matching the IFC4 U-shape orientation.
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
    # FlangeWidth measures from the web centreline outward, so the bounding half-width is `b - web/2` and the bottom
    # flange reaches -x while the top reaches +x.
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
    # cold-formed lipped channel opening +x: one closed thin-walled loop around the web, both flanges, and both lips,
    # so the contour integral needs no centreline offset and the section is exact rather than thin-wall approximated.
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
    # the schema builds the trapezium off a corner origin, so the loop re-centres on its own bounding box to hold the
    # IfcParameterizedProfileDef convention every other row is constructed under.
    raw = np.array([(0.0, 0.0), (bottom_x, 0.0), (top_offset + top_x, ydim), (top_offset, ydim)], dtype=np.float64)
    return raw - (raw.min(axis=0) + raw.max(axis=0)) / 2.0


def _operator(operator: "ifcopenshell.entity_instance") -> Affine:
    # IfcCartesianTransformationOperator2D as one row-vector affine: `Axis1` seeds U1 (the schema default is +x), U2 is
    # its left normal (the derived orthogonal complement), `Scale` defaults 1, and `LocalOrigin` translates.
    axis = np.asarray(operator.Axis1.DirectionRatios if operator.Axis1 else (1.0, 0.0), dtype=np.float64)
    unit = axis / max(float(np.linalg.norm(axis)), _EPS)
    scale = float(operator.Scale) if operator.Scale is not None else 1.0
    return np.vstack([scale * np.array([unit, (-unit[1], unit[0])], dtype=np.float64), np.asarray(operator.LocalOrigin.Coordinates, dtype=np.float64)])


def _transformed(bodies: tuple[RingTuple, ...], affine: Affine) -> tuple[RingTuple, ...]:
    # one affine over every ring of every body; a reflecting operator flips winding, which `_rings` re-normalizes, so
    # no transform arm carries an orientation special case.
    placed: Callable[[Ring], Ring] = lambda ring: ring @ affine[:2] + affine[2]
    return tuple((placed(outer), tuple(placed(void) for void in voids)) for outer, voids in bodies)


def _flaw(ring: Ring) -> str:
    # one probe, two named causes: a shape the `ClosedRing` refinement rejects, and a sub-epsilon signed area — the
    # collinear or zero-extent loop the refinement admits and the centroid divide cannot survive.
    return "malformed" if not is_bearable(ring, ClosedRing) else "zero-area" if abs(_shoelace(ring)) <= _EPS else ""


def _wound(ring: Ring, sign: float) -> Ring:
    # winding normalization to the ring's role — outer CCW, void CW — re-materialized C-contiguous, because a reversed
    # slice is a negative-stride view every downstream numpy reduction pays for.
    return np.ascontiguousarray(ring if _shoelace(ring) * sign > 0.0 else np.flip(ring, axis=0))


def _interior_point(ring: Ring) -> tuple[float, float]:
    # guaranteed-interior FE region or hole marker by horizontal-ray span scan. The ray sits strictly BETWEEN two
    # adjacent vertex levels near mid-height, so every crossing is a clean edge interior and none degenerates on a
    # vertex; sorting the crossings and taking the midpoint of the FIRST span returns a point inside the material for
    # ANY simple ring, re-entrant included, and reduces to the obvious answer on a convex one. An extrema-midpoint
    # rule lands in the notch of a U- or C-channel — outside the material — orphaning the region, so
    # `sectionproperties` meshes nothing inside the boundary and the `fe-area` residual rails for the wrong reason.
    # `_rings` proved a non-zero area, so at least two distinct vertex levels exist and the mid pair is well defined.
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
    # charter projection PER GROUP, off the one `measured` ledger the receipt, the frame, and the graduation handoff
    # already read — the `ring-closure` residual IS the `rasm.geometry.section.closure` measure, so the spelling
    # derives from the charter row and a six-section partition records six samples where a head-only fold publishes
    # one. The `WARPING` `fe-area` residual records nothing until a charter row claims it. Every tier runs
    # caller-floor, so this is already the parent side.
    receipts.fold(lambda _, receipt: charter_record(STRUCTURAL_SUBJECT, receipt.measured, composition=composition), None)
    return receipts


def _extreme_fibers(rings: ProfileRings, centroid: tuple[float, float]) -> tuple[float, float]:
    # c is the centroid-to-extreme-fibre distance, NOT half the bounding-box span (exact only for a doubly-symmetric
    # profile); take the larger centroid-relative reach per axis ACROSS EVERY BODY, so a built-up section measures to
    # its outermost fibre rather than to one constituent's.
    cx, cy = centroid
    hull = np.concatenate(rings.outers)
    lo, hi = hull.min(axis=0), hull.max(axis=0)
    return (max(abs(float(hi[0]) - cx), abs(cx - float(lo[0]))), max(abs(float(hi[1]) - cy), abs(cy - float(lo[1]))))


def _curve(curve: "ifcopenshell.entity_instance") -> "RuntimeRail[Ring]":
    # two bounded-curve forms carry a profile boundary and they read differently: IfcPolyline as an IfcCartesianPoint
    # list, IfcIndexedPolyCurve as a CartesianPointList `CoordList`. An indexed curve carrying `Segments` orders — and
    # may arc — its points, so reading the raw CoordList under one silently chords or re-orders the ring; refused by
    # name instead. Every other bounded-curve subtype names itself rather than surfacing as a missing attribute.
    return (
        Ok(np.ascontiguousarray([point.Coordinates for point in curve.Points], dtype=np.float64))
        if curve.is_a("IfcPolyline")
        else Error(BoundaryFault(boundary=(curve.is_a(), "indexed-segments-unsupported")))
        if curve.is_a("IfcIndexedPolyCurve") and curve.Segments
        else Ok(np.ascontiguousarray(curve.Points.CoordList, dtype=np.float64))
        if curve.is_a("IfcIndexedPolyCurve")
        else Error(BoundaryFault(boundary=(curve.is_a(), "unsupported-profile-curve")))
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
        # each `yield from` binds a rail and short-circuits to its first Error — a malformed selector and an empty
        # match leave here typed. The per-group fold ACCUMULATES, so one refusal names EVERY divergent profile in the
        # match rather than costing one run per defect; a survivor/casualty partition is the named next disposition.
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
        # empty-match gate ONLY: the head read is the partition's job, so a selector
        # matching six sections integrates six rather than publishing the first under every matched GlobalId.
        matched = Block.of_seq(elements)
        return Ok(matched) if not matched.is_empty() else Error(BoundaryFault(boundary=(subject, "no-profile-element")))

    @staticmethod
    def _grouped(elements: "Block[ifcopenshell.entity_instance]") -> "Block[ProfileGroup]":
        # partition by RESOLVED profile identity — the profile entity's own step id, which `Map` keeps in total order,
        # so group order and every receipt, charter sample, and frame row derived from it are deterministic.
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
        # one group, one section: the receipt's `spec` carries the tier, the CANONICAL selector the engine ran, and the
        # resolved profile's class and step id, so the evidence key addresses THIS section rather than the whole match
        # and two spellings of one query key alike.
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
                enrichment = yield from IfcStructural._warping(identity, rings)
                return replace(spine, tier=tier, enrichment=Some(enrichment))
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def _entity(model: "ifcopenshell.file", subject: str, model_guid: str) -> "RuntimeRail[Enrichment]":
        # is_a("IfcRelAssignsToGroup") guards stop a non-grouping inverse raising on a missing
        # RelatedObjects; an empty topology is a typed fault, not a silent ().
        model_node = model.by_guid(model_guid)
        members = Block.of_seq(
            member.GlobalId
            for rel in (model_node.IsGroupedBy or ())
            if rel.is_a("IfcRelAssignsToGroup")
            for member in rel.RelatedObjects
            if member.is_a("IfcStructuralMember")
        )
        return Ok(Enrichment(entity=tuple(members))) if members else Error(BoundaryFault(boundary=(subject, "no-structural-member")))

    @staticmethod
    def _warping(subject: str, rings: ProfileRings) -> "RuntimeRail[Enrichment]":
        # deterministic cytriangle mesh-and-solve owns no transiency; a mesh failure folds onto the rail through the fence.
        def solve() -> Enrichment:
            import sectionproperties.analysis as spa  # ruff:ignore[import-outside-top-level]
            import sectionproperties.pre as spp  # ruff:ignore[import-outside-top-level]

            # each ring folds its own closed facet loop with a per-ring index offset, so voids are meshed boundaries
            # the triangulator carves out rather than unbounded hole markers in a solid mesh. `CompoundGeometry`
            # is the arity-polymorphic surface — `Geometry.from_points` refuses anything but exactly one control
            # point, so a composite section would need a second call shape; this one serves one body or many, and
            # `create_mesh` takes the per-region area budget in the same control-point order.
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
                    mesh_elements=len(section.elements),  # Section carries `elements`; `num_elements` is a phantom
                )
            )

        return boundary(f"structural.warping.{subject}", solve)

    @staticmethod
    def _rings(profile: "ifcopenshell.entity_instance") -> "RuntimeRail[ProfileRings]":
        # sample then admit: one fold over EVERY ring of EVERY body proves the `ClosedRing` shape refinement AND a
        # non-degenerate signed area, then winds each ring to its role. Both halves are load-bearing — the refinement
        # alone admits three collinear points, whose zero area divides the centroid — so `_integrate`'s divisor is
        # non-zero BY ADMISSION rather than by assertion, and every ring reaches the fold C-contiguous.
        return IfcStructural._sample(profile).bind(lambda bodies: IfcStructural._admitted(profile, bodies))

    @staticmethod
    def _admitted(profile: "ifcopenshell.entity_instance", bodies: tuple[RingTuple, ...]) -> "RuntimeRail[ProfileRings]":
        flawed = Block.of_seq(
            f"{at}.{index}:{cause}"
            for at, (outer, voids) in enumerate(bodies)
            for index, ring in enumerate((outer, *voids))
            for cause in (_flaw(ring),)
            if cause
        )
        return (
            Ok(ProfileRings(bodies=tuple(ProfileBody(outer=_wound(outer, 1.0), voids=tuple(_wound(v, -1.0) for v in voids)) for outer, voids in bodies)))
            if flawed.is_empty()
            else Error(BoundaryFault(boundary=(profile.is_a(), f"degenerate-ring:{';'.join(flawed)}")))
        )

    @staticmethod
    def _sample(profile: "ifcopenshell.entity_instance") -> "RuntimeRail[tuple[RingTuple, ...]]":
        # three resolution families, narrowest `is_a` first: composite folds its children through this same entry so
        # nesting FLATTENS to one body list, the two transform families apply an affine over the parent's rings
        # (mirrored before derived, its supertype), the arbitrary-closed curve read is the fall-through, and the
        # parametric table is the terminal that NAMES an uncovered subtype instead of reaching for an `OuterCurve`
        # the parameterized family never declares.
        return (
            IfcStructural._composed(profile)
            if profile.is_a("IfcCompositeProfileDef")
            else IfcStructural._sample(profile.ParentProfile).map(lambda bodies: _transformed(bodies, _MIRROR))
            if profile.is_a("IfcMirroredProfileDef")
            else IfcStructural._sample(profile.ParentProfile).map(lambda bodies: _transformed(bodies, _operator(profile.Operator)))
            if profile.is_a("IfcDerivedProfileDef")
            else IfcStructural._arbitrary(profile)
            if profile.is_a("IfcArbitraryClosedProfileDef")
            else IfcStructural._parametric(profile)
        )

    @staticmethod
    def _composed(profile: "ifcopenshell.entity_instance") -> "RuntimeRail[tuple[RingTuple, ...]]":
        # a built-up section's children fold through the same entry, so the body list flattens whatever the nesting
        # depth and the ring-additive integral needs no second engine; the FE tier meshes one region per body.
        return traversed(Block.of_seq(profile.Profiles or ()).map(IfcStructural._sample), by=Disposition.ACCUMULATE).map(
            lambda nested: tuple(body for bodies in nested for body in bodies)
        )

    @staticmethod
    def _arbitrary(profile: "ifcopenshell.entity_instance") -> "RuntimeRail[tuple[RingTuple, ...]]":
        # `InnerCurves` is IfcArbitraryProfileDefWithVoids' OWN attribute, never the closed supertype's, so the void
        # read is subtype-gated — reading it off a plain IfcArbitraryClosedProfileDef raises. Outer and voids route
        # through one curve reader, so neither boundary assumes a representation the other does not.
        voids = tuple(profile.InnerCurves or ()) if profile.is_a("IfcArbitraryProfileDefWithVoids") else ()
        return traversed(Block.of_seq((profile.OuterCurve, *voids)).map(_curve), by=Disposition.ACCUMULATE).map(
            lambda rings: ((rings.head(), tuple(rings.tail())),)
        )

    @staticmethod
    def _parametric(profile: "ifcopenshell.entity_instance") -> "RuntimeRail[tuple[RingTuple, ...]]":
        # PROFILE_SAMPLERS first-`is_a` match, the table ordered narrowest-first; no match is a NAMED refusal, which
        # is the diagnosis an `AttributeError` converted at a distant fence never carries.
        return (
            Block.of_seq(PROFILE_SAMPLERS)
            .choose(lambda row: Some(row) if profile.is_a(row[0]) else Nothing)
            .try_head()
            .to_result(BoundaryFault(boundary=(profile.is_a(), "unsupported-profile-subtype")))
            .bind(lambda row: IfcStructural._dimensioned(profile, row))
        )

    @staticmethod
    def _dimensioned(profile: "ifcopenshell.entity_instance", row: tuple[str, tuple[str, ...], _Sampler]) -> "RuntimeRail[tuple[RingTuple, ...]]":
        # the row's roster IS the constructor's argument contract: `getattr(..., None)` collapses an attribute the
        # model left unset and one the running schema never declares onto one absent reading, so an IFC2X3 profile
        # under IFC4 row spellings refuses BY NAME instead of raising, and every constructor receives proven floats.
        ifc_class, names, build = row
        resolved = Block.of_seq(names).map(lambda name: (name, getattr(profile, name, None)))
        absent = resolved.choose(lambda pair: Some(pair[0]) if not isinstance(pair[1], (int, float)) else Nothing)
        return (
            Ok((build(tuple(float(value) for _, value in resolved)),))
            if absent.is_empty()
            else Error(BoundaryFault(boundary=(ifc_class, f"missing-profile-dimension:{','.join(absent)}")))
        )

    @staticmethod
    def _profile(element: "ifcopenshell.entity_instance") -> "ifcopenshell.entity_instance":
        # an element is its own profile when it is one; else the material-profile chain resolves
        # through a total is_a() match, falling back to the element for the arbitrary-closed read.
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
        # `CompositeProfile` is the schema's OWN whole-section representation of a multi-profile set, so it wins over
        # the head `MaterialProfiles` read — which stays the schema's fallback for a set that declares no composite.
        match material.is_a():
            case "IfcMaterialProfileSet":
                return Some(material.CompositeProfile or material.MaterialProfiles[0].Profile)
            case "IfcMaterialProfileSetUsage":
                return Some(material.ForProfileSet.CompositeProfile or material.ForProfileSet.MaterialProfiles[0].Profile)
            case _:
                return Nothing

    @staticmethod
    def _moments(rings: ProfileRings) -> dict[str, float]:
        # every ring is wound to its role, so its own cross term carries the sign and the fold needs no multiplier:
        # each ring lowers once to its (x, y, xn, yn, cross) edge cell and the six integrals are one immutable dict
        # comprehension over MOMENT_KERNELS, never a `moments[name] +=` accumulator.
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
        # per-ring admission proved each loop non-degenerate; the NET area is the remaining degeneracy — voids that
        # meet or exceed their outer — so it gates here and every derivation past the gate is total.
        moments = IfcStructural._moments(rings)
        a, qx, qy, ixx, iyy, ixy = (moments[key] for key in ("a", "qx", "qy", "ixx", "iyy", "ixy"))

        def assembled(area: float) -> SectionReceipt:
            cx, cy = qy / area, qx / area
            ixx_c, iyy_c, ixy_c = ixx - area * cy * cy, iyy - area * cx * cx, ixy - area * cx * cy
            # eigh returns ascending eigenvalues with column-aligned eigenvectors; index the major axis
            # so principal_moments[0] and principal_angle name the SAME axis rather than racing the eigh ordering.
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
                section_moduli=(ixx_c / max(cy_fibre, _EPS), iyy_c / max(cx_fibre, _EPS)),  # S = I / c over the centroid-to-extreme-fibre reach
            )

        return Ok(assembled(a)) if a > _EPS else Error(BoundaryFault(boundary=(group.profile.is_a(), f"non-positive-area:{a:.6g}")))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [SEGMENT_INDEX_SELECT]-[OPEN]: how does `ifcopenshell` surface an `IfcIndexedPolyCurve.Segments` member, so a `IfcLineIndex` run is distinguishable from an `IfcArcIndex` run and `_curve` can order line runs instead of refusing every segmented curve; the `IfcSegmentIndexSelect` members are LIST-of-integer defined types, whose wrapper representation the entity-JSON schema dump does not carry. Route: read a segmented curve through a built `ifcopenshell` wrapper on an interpreter its extension targets, else `ifcopenshell/express/schema_class.py` for the defined-type SELECT lowering.
- [PROFILE_ATTRIBUTE_OPTIONALITY]-[OPEN]: which `IfcParameterizedProfileDef` dimensional attributes the IFC4 schema declares OPTIONAL, and whether `IfcAsymmetricIShapeProfileDef.TopFlangeThickness` carries the schema's bottom-flange default — the `PROFILE_SAMPLERS` rosters refuse every absent dimension by name, so a declared default would turn one refusal into a resolvable row. Route: `ifcopenshell.ifcopenshell_wrapper.schema_by_name("IFC4")` attribute optionality under a built wrapper; the installed `util/schema/ifc4_entities.json` carries attribute names and prose only.
