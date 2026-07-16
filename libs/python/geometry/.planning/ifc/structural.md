# [PY_GEOMETRY_IFC_STRUCTURAL]

The cross-section structural-property owner — the section-integral and structural-member verbs the analysis and lifecycle hops drop. `IfcStructural` resolves a closed-form section-property receipt from a profile polygon's ring coordinates by one `numpy` Green's-theorem contour fold over the `MOMENT_KERNELS` weight table (area, first and second area moments, centroid, principal second moments and principal-axis rotation, polar moment, the centroid-relative elastic section moduli, and the thin-walled Bredt torsion constant), then tiers that spine with two gated enrichment layers under one `EnrichmentTier` policy: the `ifcopenshell` layer reading `IfcStructuralAnalysisModel`/`IfcStructuralMember` topology onto the member, and the `sectionproperties` layer meshing the same rings into a triangular FE section for the warping, plastic, and shear receipts no closed-form integral derives. The C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the numerical section dimension the managed projection does not produce.

The two enrichments are one `Enrichment` `@tagged_union` on a single `SectionReceipt.enrichment` field — the `entity` case carrying the structural-member GlobalIds, the `warping` case the FE `WarpingEvidence` value object — so a `CORE` receipt carries `Nothing`, never a `None`-slot bag racing the tier. Every profile-bearing query admits through `IfcSelector.filter`/`IfcSelector.parse` from `geometry:ifc/selector.md#SELECTOR` (the one selection engine, the only `filter_elements` caller), a `<selector>#<analysis-model-guid>` split feeding the member rings and the structural-model guid, so a malformed profile selector is a typed `Error(BoundaryFault)` at admission. The whole admission rides the graduation `evidence_run` weave under `EvidenceScope.IFC_SECTION`, so a provider exception, a degenerate ring, and an FE divergence each fold onto one `RuntimeRail[SectionReceipt]`. The receipt graduates as `GeometrySubject.SECTION_PROPERTY` carried by the `STRUCTURAL_SUBJECT` constant — the section-integral evidence class distinct from the compliance and lifecycle members its siblings bind — and `graduates()` returns the local `GeometryHandoff` whose `wire()` projection is the compute crossing. The spine rides the bare runtime (`numpy`) and never depends on either gated layer; the IFC-entity layer rides the `ifcopenshell` worker lane and the warping layer the `sectionproperties` worker lane (native mesh backend `cytriangle`, LGPLv3).

## [01]-[INDEX]

- [01]-[STRUCTURAL]: the section-integral spine and two gated enrichment tiers under one `EnrichmentTier` owner folding per-tier evidence into one `Enrichment` union, woven through the `evidence_run` graduation weave and emitting the `SECTION_PROPERTY` subject.

## [02]-[STRUCTURAL]

- Owner: `IfcStructural` — the boundary capsule over the section-integral spine and the two gated enrichment tiers; the `EnrichmentTier` discriminant and the `Enrichment` evidence shape are the same union, so each tier is one `_dispatch` builder arm, never a sibling per-tier class.
- Cases: `CORE` folds the closed-form section-integral receipt on the bare runtime interpreter; `IFC_ENTITY` adds the `IfcStructuralAnalysisModel`/`IfcStructuralMember` topology on the `ifcopenshell` worker lane; `WARPING` adds the FE warping/plastic/shear receipts on the `sectionproperties` worker lane. The spine never depends on either gated layer — the upper tiers add evidence only where their interpreter resolves.
- Entry: `IfcStructural.run` takes an `ifcopenshell.file`, an `EnrichmentTier`, and a `spec` whose meaning the tier fixes — a `<selector>` profile-bearing query for `CORE`/`WARPING` resolving each element's `IfcProfileDef` rings off its material-profile assignment, a `<selector>#<analysis-model-guid>` query for `IFC_ENTITY` joining the members to their structural-analysis model — and returns `RuntimeRail[SectionReceipt]`. Graduation stays the caller's own step on `SectionReceipt.graduates(evidence_key, ceiling)`, mirroring `IfcAnalysis.run`/`IfcLifecycle.run`. The `subjects` field derives from the tier's true subject set — profile-bearing GlobalIds for `CORE`/`WARPING`, structural-member GlobalIds for `IFC_ENTITY`.
- Auto: `_dispatch` is one `effect.result` builder — it binds the `IfcSelector` admission, the `_first_profile` head, the ring fold, and the spine `_integrate`, then the tier selects the enrichment. `_sample` dispatches the parametric subtypes through `PROFILE_SAMPLERS` (hollow rows ordered ahead of their solid supertypes so the first `is_a` match lands the hollow sampler) and falls through to the `IfcArbitraryClosedProfileDef` CoordList read — both hand the shape-agnostic contour integral the same ring tuple. `_entity` folds the `IsGroupedBy`/`IfcRelAssignsToGroup`-guarded `IfcStructuralMember` set — entity topology only, never re-deriving a section property, since the centroid-relative elastic section moduli are a closed-form spine field every tier carries. `_warping` builds the `pre.Geometry.from_points` region with one closed facet loop per ring plus each void's interior hole marker so the voids are carved boundaries, runs geometric→warping→plastic in the one prerequisite order, and reads `get_area` back to cross-check the `numpy` spine area (the `fe-area` residual); the FE torsion lands on `WarpingEvidence.fe_torsion_constant`, never overwriting the spine's thin-walled `torsion_constant`.
- Receipt: `SectionReceipt` conforms structurally to `ReceiptContributor` — `contribute` emits one row carrying the tier tag, the section integrals, and the `Enrichment.facts` projection; `graduates` folds the tier-aware `measured` ledger onto `GeometryHandoff.of(STRUCTURAL_SUBJECT, ...)` rather than inlining a ceiling comparison. The `measured` ledger is data-driven by tier — the `ring-closure` residual (polar moment vs principal sum) for every tier, plus the `WARPING` `fe-area` FE-convergence residual — so a degenerate profile or a diverging FE mesh graduates as an `Error(BoundaryFault)`, never a clean section receipt.
- Packages: `numpy` (the shoelace contour fold over `MOMENT_KERNELS`, `linalg.eigh` for the major-axis principal solution, the `PROFILE_SAMPLERS` curved-subtype polylines, `argsort` for the `_interior_point` marker); `expression` (the `effect.result` rail, `Block` folds for the member set and facet loops, the `Enrichment` union, `Option` rail lifts); `beartype` (the `ProfileRings` `Is` finiteness refinement at the `_integrate` fence); geometry graduation (`evidence_run`/`GeometryHandoff`/`GeometrySubject`); `geometry:ifc/selector.md#SELECTOR` (`IfcSelector.filter`/`parse` — the only `filter_elements` caller); `ifcopenshell` (`IfcProfileDef`/`IfcStructuralAnalysisModel`/`IfcStructuralMember` attributes over the in-process model, `CORE` reading only the profile); `sectionproperties` (`WARPING` tier only, native mesh backend `cytriangle`, LGPLv3); runtime rails.
- Growth: a new section integral is one `MOMENT_KERNELS` row plus one `SectionReceipt` field; a new parametric profile subtype is one `PROFILE_SAMPLERS` row plus its ring constructor — the rings stay the universal input and the contour fold stays shape-agnostic, never a per-shape integral family; a new enrichment tier is one `EnrichmentTier` row plus one `Enrichment` case plus one `_dispatch` arm; a new warping/plastic measure is one `WarpingEvidence` field plus one `Section.get_*` accessor; a new selection axis is one `IfcSelector` grammar alternative, never a local query-parse fold; a stricter residual bar is one tighter ceiling row the caller supplies.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy (projected in-process); no durable store, Rhino/GH mutation, or mesh/GLB write — the `WARPING` FE section mesh is an in-memory `sectionproperties` artifact consumed for its scalars, never a `mesh/repair.md` payload write. `ifcopenshell`/`sectionproperties` import function-local at the tier-gated boundary per the manifest import policy, so a `CORE` run never loads a gated package. The deterministic `sectionproperties` solve owns no transiency — a retry over it (a `stamina.retry` mint included) is a deleted form. The raw `spec` never threads past admission into `filter_elements` — `IfcSelector` re-serializes the validated query, the one selection engine.

```python signature
from collections.abc import Callable
from enum import IntEnum
from typing import TYPE_CHECKING, Annotated, Final, Literal, assert_never

import numpy as np
from beartype import beartype
from beartype.vale import Is
from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct
from msgspec.structs import replace
from numpy.typing import NDArray

from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_run
from rasm.geometry.ifc.selector import IfcSelector
from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail, boundary, railed
from rasm.runtime.identity import ContentKey
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:  # function-local tier-boundary imports keep the CORE spine clean (manifest import policy)
    import ifcopenshell

# --- [TYPES] ---------------------------------------------------------------------------


class EnrichmentTier(IntEnum):
    CORE = 0
    IFC_ENTITY = 1
    WARPING = 2


type Ring = NDArray[np.float64]
type RingTuple = tuple[Ring, tuple[Ring, ...]]
type _Moment = Callable[[Ring, Ring, Ring, Ring], Ring]
type _Sampler = Callable[["ifcopenshell.entity_instance"], RingTuple]

# the contract the @beartype(conf=FAULT_CONF) _integrate boundary checks: ≥3 finite vertices; a
# NaN/empty/two-point ring rails through the CLASSIFY `api` row before the fold divides by a zero area.
type ClosedRing = Annotated[Ring, Is[lambda r: r.ndim == 2 and r.shape[0] >= 3 and bool(np.isfinite(r).all())]]

# --- [CONSTANTS] -----------------------------------------------------------------------

# the SECTION_PROPERTY graduation member; an unlisted subject fails at the boundary under `ty`.
STRUCTURAL_SUBJECT: Final[GeometrySubject] = GeometrySubject.SECTION_PROPERTY

# polyline fidelity of the curved parametric subtypes — the one tessellation policy a caller may sharpen.
CIRCLE_SEGMENTS: Final[int] = 64

# denormal floor for the section-modulus and residual denominators: a max(x, 1.0) clamp corrupts a
# sub-unit section (a 0.3 m fibre reads as 1.0), so the floor guards the genuine zero alone.
_EPS: Final[float] = 1e-12

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

# parametric-subtype sampling table: each row folds a subtype's attributes into the (outer, voids)
# ring tuple. Hollow rows precede their solid supertypes so the first is_a match lands the hollow
# sampler; the arbitrary-closed CoordList read is the fall-through, not a row.
PROFILE_SAMPLERS: Final[tuple[tuple[str, _Sampler], ...]] = (
    ("IfcRectangleHollowProfileDef", lambda p: _box_rings(p.XDim, p.YDim, p.WallThickness)),
    ("IfcRectangleProfileDef", lambda p: (_rect(p.XDim, p.YDim), ())),
    ("IfcCircleHollowProfileDef", lambda p: (_circle(p.Radius), (_circle(p.Radius - p.WallThickness),))),
    ("IfcCircleProfileDef", lambda p: (_circle(p.Radius), ())),
    ("IfcIShapeProfileDef", lambda p: (_i_section(p.OverallWidth, p.OverallDepth, p.WebThickness, p.FlangeThickness), ())),
)

# --- [MODELS] --------------------------------------------------------------------------


# the FE payload as one value object, carried only by Enrichment.warping.
class WarpingEvidence(Struct, frozen=True, gc=False):
    fe_torsion_constant: float
    fe_area: float
    shear_center: tuple[float, float]
    shear_areas: tuple[float, float]
    plastic_moduli: tuple[float, float]
    mesh_elements: int


# the tier discriminant and the evidence shape are one union.
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


class ProfileRings(Struct, frozen=True, gc=False):
    outer: ClosedRing
    voids: tuple[Ring, ...]

    @property
    def signed(self) -> tuple[tuple[Ring, float], ...]:
        return ((self.outer, 1.0), *((v, -1.0) for v in self.voids))

    @property
    def rings(self) -> tuple[Ring, ...]:
        return (self.outer, *self.voids)


class SectionReceipt(Struct, frozen=True, gc=False):
    tier: EnrichmentTier
    subjects: tuple[str, ...]
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
            case Some(Enrichment(tag="warping", warping=fe)):
                return ledger | {"fe-area": abs(fe.fe_area - self.area) / max(self.area, _EPS)}
            case _:
                return ledger

    def contribute(self) -> "Block[Receipt]":
        facts: dict[str, object] = {
            "tier": self.tier.name,
            "area": self.area,
            "polar_moment": self.polar_moment,
            "principal_angle": self.principal_angle,
            **self.enrichment.map(lambda e: e.facts()).default_value({}),
            **self.measured,
        }
        return Block.singleton(Receipt.of("rasm.geometry.ifc.structural", ("emitted", STRUCTURAL_SUBJECT, facts)))

    def graduates(self, evidence_key: ContentKey, ceiling: dict[str, float]) -> GeometryHandoff:
        # the carrier's residual-over-ceiling `admitted` verdict gates; wire() is the compute crossing.
        return GeometryHandoff.of(STRUCTURAL_SUBJECT, evidence_key, self.measured, ceiling)


# --- [OPERATIONS] ----------------------------------------------------------------------

# closed-ring float64 coordinates in profile-local axes (centred on the origin per the
# IfcParameterizedProfileDef convention) the contour integral reads with no shape branch.


def _rect(xdim: float, ydim: float) -> Ring:
    hx, hy = xdim / 2.0, ydim / 2.0
    return np.array([(-hx, -hy), (hx, -hy), (hx, hy), (-hx, hy)], dtype=np.float64)


def _circle(radius: float) -> Ring:
    theta = np.linspace(0.0, 2.0 * np.pi, CIRCLE_SEGMENTS, endpoint=False)
    return np.stack([radius * np.cos(theta), radius * np.sin(theta)], axis=1).astype(np.float64)


def _box_rings(xdim: float, ydim: float, wall: float | None) -> RingTuple:
    # WallThickness is an optional schema attribute: an absent wall is a solid rectangle, never a
    # stringly getattr default; the Option fold owns the absence.
    return (
        Option.of_optional(wall).map(lambda w: (_rect(xdim, ydim), (_rect(xdim - 2.0 * w, ydim - 2.0 * w),))).default_value((_rect(xdim, ydim), ()))
    )


def _i_section(width: float, depth: float, web: float, flange: float) -> Ring:
    hw, hd, hwe = width / 2.0, depth / 2.0, web / 2.0
    yf = hd - flange
    return np.array(
        [(-hw, -hd), (hw, -hd), (hw, -yf), (hwe, -yf), (hwe, yf), (hw, yf), (hw, hd), (-hw, hd), (-hw, yf), (-hwe, yf), (-hwe, -yf), (-hw, -yf)],
        dtype=np.float64,
    )


class IfcStructural:
    @staticmethod
    def run(model: "ifcopenshell.file", tier: EnrichmentTier, spec: str) -> "RuntimeRail[SectionReceipt]":
        return evidence_run(EvidenceScope.IFC_SECTION, f"run.{tier.name.lower()}", lambda: IfcStructural._dispatch(model, tier, spec))

    @staticmethod
    @railed
    def _dispatch(model: "ifcopenshell.file", tier: EnrichmentTier, spec: str) -> "SectionReceipt":
        # each `yield from` binds a rail and short-circuits to its first Error — a malformed selector,
        # empty match, degenerate ring, or FE divergence all leave here as one typed BoundaryFault.
        selector, _, model_guid = spec.partition("#")
        elements = yield from IfcSelector.filter(model, selector)
        subjects = tuple(e.GlobalId for e in elements)
        profile = yield from IfcStructural._first_profile(selector, elements)
        rings = IfcStructural._rings(profile)
        spine: SectionReceipt = yield from IfcStructural._integrate(rings, subjects)
        match tier:
            case EnrichmentTier.CORE:
                return spine
            case EnrichmentTier.IFC_ENTITY:
                enrichment = yield from IfcStructural._entity(model, selector, model_guid)
                return replace(spine, tier=tier, subjects=enrichment.entity, enrichment=Some(enrichment))
            case EnrichmentTier.WARPING:
                enrichment = yield from IfcStructural._warping(selector, rings, spine.area)
                return replace(spine, tier=tier, enrichment=Some(enrichment))
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def _first_profile(subject: str, elements: tuple["ifcopenshell.entity_instance", ...]) -> "RuntimeRail[ifcopenshell.entity_instance]":
        # an empty match is a typed BoundaryFault via Block.try_head, never a silent elements[0] index fault.
        return Block.of_seq(elements).try_head().to_result(BoundaryFault(boundary=(subject, "no-profile-element")))

    @staticmethod
    def _entity(model: "ifcopenshell.file", subject: str, model_guid: str) -> "RuntimeRail[Enrichment]":
        # the is_a("IfcRelAssignsToGroup") guard stops a non-grouping inverse raising on a missing
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
    def _warping(subject: str, rings: ProfileRings, area: float) -> "RuntimeRail[Enrichment]":
        # the deterministic cytriangle mesh-and-solve owns no transiency; a mesh failure folds onto the rail through the fence.
        def solve() -> Enrichment:
            import sectionproperties.analysis as spa  # noqa: PLC0415
            import sectionproperties.pre as spp  # noqa: PLC0415

            # each ring folds its own closed facet loop with a per-ring index offset, so the voids are
            # meshed boundaries the triangulator carves out — not unbounded hole markers in a solid mesh.
            seed: tuple[Block[tuple[float, float]], Block[tuple[int, int]]] = (Block.empty(), Block.empty())
            points, facets = Block.of_seq(rings.rings).fold(_facet_loop, seed)
            holes = [IfcStructural._interior_point(v) for v in rings.voids]
            geom = spp.Geometry.from_points(list(points), list(facets), [IfcStructural._interior_point(rings.outer)], holes or None)
            section = spa.Section(geom.create_mesh([area / 100.0]))
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
    def _rings(element: "ifcopenshell.entity_instance") -> ProfileRings:
        outer, voids = IfcStructural._sample(IfcStructural._profile(element))
        return ProfileRings(outer=outer, voids=voids)

    @staticmethod
    def _sample(profile: "ifcopenshell.entity_instance") -> RingTuple:
        # PROFILE_SAMPLERS first-match via Block.choose; the arbitrary-closed read is the default_with fall-through.
        return (
            Block
            .of_seq(PROFILE_SAMPLERS)
            .choose(lambda row: Some(row[1](profile)) if profile.is_a(row[0]) else Nothing)
            .try_head()
            .default_with(lambda: IfcStructural._arbitrary(profile))
        )

    @staticmethod
    def _arbitrary(profile: "ifcopenshell.entity_instance") -> RingTuple:
        outer = np.asarray(profile.OuterCurve.Points.CoordList, dtype=np.float64)
        voids = tuple(np.asarray(c.Points.CoordList, dtype=np.float64) for c in (profile.InnerCurves or ()))
        return outer, voids

    @staticmethod
    def _profile(element: "ifcopenshell.entity_instance") -> "ifcopenshell.entity_instance":
        # the element is its own profile when it is one; else the material-profile chain resolves
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
        match material.is_a():
            case "IfcMaterialProfileSet":
                return Some(material.MaterialProfiles[0].Profile)
            case "IfcMaterialProfileSetUsage":
                return Some(material.ForProfileSet.MaterialProfiles[0].Profile)
            case _:
                return Nothing

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _integrate(rings: ProfileRings, subjects: tuple[str, ...]) -> "RuntimeRail[SectionReceipt]":
        # Is-refined rings reach here finite and non-degenerate, so the area divisor is never zero. Each
        # signed ring lowers once to its (sign, x, y, xn, yn, cross) edge cell; the six integrals are an
        # immutable dict-comprehension over MOMENT_KERNELS, never a `moments[name] +=` accumulator.
        edges = tuple(
            (sign, x, y, np.roll(x, -1), np.roll(y, -1), x * np.roll(y, -1) - np.roll(x, -1) * y)
            for ring, sign in rings.signed
            for x, y in ((ring[:, 0], ring[:, 1]),)
        )
        moments = {
            name: sum(sign * float(np.sum(weight(x, y, xn, yn) * cross)) / divisor for sign, x, y, xn, yn, cross in edges)
            for name, weight, divisor in MOMENT_KERNELS
        }
        a, qx, qy, ixx, iyy, ixy = (moments[k] for k in ("a", "qx", "qy", "ixx", "iyy", "ixy"))
        cx, cy = qy / a, qx / a
        ixx_c, iyy_c, ixy_c = ixx - a * cy * cy, iyy - a * cx * cx, ixy - a * cx * cy
        # eigh returns ascending eigenvalues with column-aligned eigenvectors; index the major axis
        # so principal_moments[0] and principal_angle name the SAME axis rather than racing the eigh ordering.
        principal, vectors = np.linalg.eigh(np.array([[ixx_c, -ixy_c], [-ixy_c, iyy_c]], dtype=np.float64))
        major = int(np.argmax(principal))
        phi = float(np.arctan2(vectors[1, major], vectors[0, major]))
        perimeter = sum(float(np.sum(np.linalg.norm(np.diff(np.vstack([r, r[:1]]), axis=0), axis=1))) for r, _ in rings.signed)
        cx_fibre, cy_fibre = IfcStructural._extreme_fibers(rings, (cx, cy))
        return Ok(
            SectionReceipt(
                tier=EnrichmentTier.CORE,
                subjects=subjects,
                area=abs(a),
                centroid=(cx, cy),
                second_moments=(ixx_c, iyy_c, ixy_c),
                principal_moments=(float(principal[major]), float(principal[1 - major])),
                principal_angle=phi,
                polar_moment=ixx_c + iyy_c,
                torsion_constant=4.0 * abs(a) * abs(a) / max(perimeter, _EPS),
                section_moduli=(ixx_c / max(cy_fibre, _EPS), iyy_c / max(cx_fibre, _EPS)),  # S = I / c over the centroid-to-extreme-fibre reach
            )
        )

    @staticmethod
    def _extreme_fibers(rings: ProfileRings, centroid: tuple[float, float]) -> tuple[float, float]:
        # c is the centroid-to-extreme-fibre distance, NOT half the bounding-box span (exact only for a
        # doubly-symmetric profile); take the larger centroid-relative reach per axis so an asymmetric section is exact.
        cx, cy = centroid
        lo, hi = rings.outer.min(axis=0), rings.outer.max(axis=0)
        return (max(abs(float(hi[0]) - cx), abs(cx - float(lo[0]))), max(abs(float(hi[1]) - cy), abs(cy - float(lo[1]))))

    @staticmethod
    def _interior_point(ring: Ring) -> tuple[float, float]:
        # a guaranteed-interior FE region/hole marker: the mean of the two widest-x-extent vertices lands
        # inside even a non-convex ring, where the bare centroid can fall outside and orphan the region.
        order = np.argsort(ring[:, 0])
        midpoint = (ring[order[0]] + ring[order[-1]]) / 2.0
        return float(midpoint[0]), float(midpoint[1])


def _facet_loop(
    acc: tuple["Block[tuple[float, float]]", "Block[tuple[int, int]]"], ring: Ring
) -> tuple["Block[tuple[float, float]]", "Block[tuple[int, int]]"]:
    points, facets = acc
    start, count = len(points), len(ring)
    coords = Block.of_seq(tuple(p) for p in ring.tolist())
    loop = Block.of_seq((start + i, start + (i + 1) % count) for i in range(count))
    return points.append(coords), facets.append(loop)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
