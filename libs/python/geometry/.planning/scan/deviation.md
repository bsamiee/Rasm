# [PY_GEOMETRY_SCAN_DEVIATION]

Scan-vs-model deviation and primitive extraction — the central AEC value of a host-free scan companion, on top of the registered pose. `ScanDeviation` is one stage-discriminated owner folding a single construction-verification pipeline rather than parallel modes: RANSAC plane/primitive segmentation classifying the extracted planar primitives into the `PrimitiveClass` vocabulary (wall/slab/column/generic) by plane-normal axis, the per-segment signed nearest-surface deviation between the registered cloud and the IFC-tessellated GLB over one amortized `trimesh.proximity.ProximityQuery`, and the folded `DeviationBand` — signed extrema, the absolute `max`/`mean`/`std`/`rms` magnitudes, and the over-build/under-build inlier counts that the construction-verification verdict and the colored overlay both read. The three stages are arms of one `evaluate` fold keyed by a `DeviationStage` request value, never a flat mode flag returning a vacuous result: `SEGMENT` extracts and classifies, `DEVIATE` folds the signed band over the whole element, and `ATTRIBUTED` composes both so the per-primitive grouping and the `on_surface` triangle-id attribution ride the same index the overlay egress reads.

The registered transform from `scan/registration.md#REGISTRATION` is the precondition; the reference surface arrives BY CONTENT KEY — the `mesh/daemon.md#DAEMON` welded GLB fetched keyed over the `Rasm.Bim/Model` seam, scan never re-tessellating. The watertight precondition is a composed gate, not a bare raise: `_query` folds the reference through `mesh/quality`'s public `closure_fold` (quality tiers below the scan producers) and an open reference raises the typed `DeviationFault` the lane fence converts — never a domain `ValueError`. The entry is `async` and the multi-second proximity/segmentation kernel rides `lane.offload` under the graduation `evidence_run` weave (`EvidenceScope.SCAN_DEVIATION` the seed row — no page-local tracer mint). The deviation graduates through the geometry-minted `rasm.geometry.graduation` spine as the `GeometrySubject.SCAN_DEVIATION` member keyed to the IFC element GlobalId — `graduates()` returns the local `GeometryHandoff` carrier whose `wire()` projection is the compute crossing, the tolerance/fraction ceilings `DeviationPolicy` rows. `trimesh.proximity.signed_distance` is positive inside the watertight design solid and negative outside, so under-build (built material missing, positive) is distinguished from over-build (excess material, negative); the verdict reads the absolute band against tolerance while the colored overlay reads the sign and the per-face triangle id.

## [01]-[INDEX]

- [01]-[DEVIATION]: plane/primitive segmentation with `PrimitiveClass` classification and the folded signed `DeviationBand` under one stage-discriminated owner over the open3d segmentation and trimesh `ProximityQuery` surfaces — the entry an `async` composition of the graduation `evidence_run` weave over `lane.offload`, the `@beartype(conf=FAULT_CONF)` `SignedField` finiteness fence on the band fold, the `mesh/quality` `closure_fold` watertight gate, `Block.unfold` the RANSAC-peel state machine, and the weave's harvest the one receipt egress.

## [02]-[DEVIATION]

- Owner: `ScanDeviation` — the frozen owner discriminating by `DeviationStage` request value over a registered `o3d.geometry.PointCloud` and a content-keyed IFC-tessellated GLB reference, carrying a `DeviationPolicy` value object (segmentation gains, the worst-point `tolerance` ceiling, the tighter per-point `working_tolerance` acceptance band, the noncompliant `fraction` bar, the slab/wall verticality thresholds — every ceiling a policy row, never a module `Final`) with derived `segment_args`/`verticality` projections; `DeviationBand` the value object folding the signed proximity array into the signed `over_extreme`/`under_extreme`, the absolute `max`/`mean`/`std`/`rms`, the over/under inlier counts, and the `noncompliant_fraction` residual, with a `DeviationBand.fold` factory that runs the whole numpy reduction once and a `verdict(tolerance, fraction)` method so the band math lives in one place; `Segment` the classified-primitive value object holding the `[a,b,c,d]` plane model, the unit normal, the inlier count, and the `PrimitiveClass` the plane-normal axis resolves, with a per-segment `DeviationBand` when the `ATTRIBUTED` stage groups deviation by primitive; `DeviationFault` the typed `@tagged_union(Exception)` the kernel raises INTO the lane fence when the quality closure fold reads the reference open — never a bare `ValueError` in domain flow; `DeviationResult` the per-element typed receipt carrying the stage, the element GlobalId, the folded element `DeviationBand`, the classified `Segment` tuple, and the optional triangle-id attribution map, with a `DeviationResult.of` factory that defaults the empty arms; `PrimitiveClass` the wall/slab/column/generic vocabulary the plane-normal axis classifies.
- Cases: `DeviationStage` rows `SEGMENT` (RANSAC `segment_plane` iterative outlier-peel oversegmentation over a `Block.unfold` extracting and classifying the dominant planar primitives into `PrimitiveClass`), `DEVIATE` (the signed nearest-surface `ProximityQuery.signed_distance` query folded once into the element `DeviationBand` and the construction-verification verdict), and `ATTRIBUTED` (the full pass composing both — per-`Segment` `DeviationBand` grouping plus the `ProximityQuery.on_surface` triangle-id attribution the colored-overlay egress reads off the same amortized index) — matched by `match`/`assert_never`. The three stages are arms of one pipeline keyed by the request value, never three parallel result shapes; `SEGMENT` returns a `DeviationResult` carrying the classified segments and an identity (zero-magnitude) band the `verdict` reads as the as-yet-unmeasured element, never a vacuous `compliant=True`, so a segmentation-only request never graduates a false-positive compliant handoff.
- Entry: `ScanDeviation.evaluate` is `async` — it admits the registered cloud, the content-keyed reference GLB bytes, an element GlobalId, and a `DeviationStage`, and returns `RuntimeRail[DeviationResult]` by composing `evidence_run(EvidenceScope.SCAN_DEVIATION, f"evaluate.{stage}", partial(self.lane.offload, _deviation_kernel, cloud, reference_glb, element, stage, self.policy))` — the graduation weave opens the seeded span, fences the offload, and flattens the lane's rail; the weave's harvest emits the structurally-conforming `DeviationResult.contribute` stream exactly once on the cleared `Ok`. The module-level `_deviation_kernel` runs the stage through one `match`: the `SEGMENT` arm unfolds `segment_plane` over the residual cloud through `Block.unfold` (re-segmenting the `select_by_index(invert=True)` remainder) into a classified `Segment` tuple; the `DEVIATE` arm builds one `ProximityQuery` over the quality-gated reference and folds `signed_distance` once into the element `DeviationBand` through `DeviationBand.fold`; the `ATTRIBUTED` arm threads both off that one index — grouping the signed band per classified segment by the inlier index sets and composing `on_surface` for the per-face triangle-id map. A watertight-precondition breach or a non-finite band raises inside the kernel and converts through the lane's `async_boundary` onto the rail the weave records on the live span.
- Auto: `PointCloud.segment_plane(distance_threshold, ransac_n, num_iterations)` returns the `[a,b,c,d]` plane model plus the inlier index set, `select_by_index(inliers, invert=True)` peels the remainder so the next `Block.unfold` step extracts the next dominant plane, and the unit normal `[a,b,c]` resolves the `PrimitiveClass` by its dominant axis — the classification is a data-table lookup over the normal axis, never a per-class extraction method; `_query` loads the reference GLB, composes `mesh/quality.closure_fold` as the watertight gate (an open reference raises the typed `DeviationFault(open_reference=element)` the fence converts — the second consumer of the quality owner's one closure truth, never a re-computed per-consumer closure), then one `ProximityQuery(mesh)` amortizes the rtree triangle-index build across the whole point batch, its `signed_distance(points)` returning the per-point signed distance, and `DeviationBand.fold` runs the whole reduction once under the `@beartype(conf=FAULT_CONF)` `SignedField` finiteness fence: the signed `over_extreme` is `signed.min` (excess material outside), the `under_extreme` is `signed.max` (missing material inside), the absolute band is `numpy` `abs` then `max`/`mean`/`std` plus the `rms` as `linalg.norm(magnitude) / sqrt(n)`, the over/under inlier counts are the `numpy.where`/`sign` partition mask sums, and the `noncompliant_fraction` is the fraction of `abs` magnitudes exceeding the tighter per-point `working_tolerance` (distinct from the worst-point `tolerance` ceiling, so the bulk-surface gate stays independent of the max-distance gate) via a `clip`-bounded mask sum — one fold, not a stat-by-stat re-scan; the same query's `on_surface(points)` returns the closest surface point, the unsigned distance, and the triangle id the `ATTRIBUTED` stage composes into the per-face attribution map, never a second rtree build.
- Receipt: `DeviationResult.contribute` returns the one-element `tuple[Receipt, ...]` the graduation weave's harvest emits exactly once on the cleared `Ok` — never a page-local `@receipted` leg — an `emitted`-phase `Receipt.of("geometry.scan.deviation", ("emitted", element, facts))` row carrying the stage, the band's signed extrema and absolute `max`/`mean`/`std`/`rms`, the over/under inlier counts, the `noncompliant_fraction`, the classified-segment count spread by `PrimitiveClass`, and the compliant verdict — the band facts produced once through `DeviationBand.facts` so the receipt and the graduation ledger read the same fold. `DeviationResult.graduates(evidence_key, policy)` returns the local `GeometryHandoff` carrier — `GeometryHandoff.of(GeometrySubject.SCAN_DEVIATION, key, measured, ceilings)` with TWO measured keys, `max_distance` against `policy.tolerance` and `noncompliant_fraction` against `policy.fraction`, so an element whose single worst point clears tolerance but whose bulk surface is out of band does not cross clean on the extremum alone, and a `SEGMENT`-stage result crosses with an EMPTY measured dict so the spine's unmeasured-ceiling law breaches it — the identity band's zeros never admit — both keys ride the graduation owner's residual-over-ceiling `admitted` direction unchanged, and the crossing to compute is the carrier's `wire()` data, never an import. The graduated subject is keyed to the IFC element GlobalId so the per-element deviation reaches the C# owner system and the TS viewer as a colored overlay through the one graduation rail.
- Packages: `open3d` (`geometry.PointCloud.segment_plane`/`select_by_index`/`points` the RANSAC peel surface), `trimesh` (`load_mesh(io.BytesIO(glb), file_type="glb")` the GLB reader, `proximity.ProximityQuery`/`signed_distance`/`on_surface` the one amortized index), `numpy` (`abs`/`sign`/`where`/`clip`/`mean`/`std`/`min`/`max`/`sum`/`sqrt`/`isfinite`/`linalg.norm`/`arange`/`delete` the band and peel folds), `beartype` (`@beartype(conf=FAULT_CONF)` + `vale.Is` the `SignedField` finiteness refinement), `expression` (`Block.unfold` the peel state machine, `Option`/`Some`/`Nothing` its step), `msgspec` (`Struct`/`field`/`structs.replace` the carriers), geometry (`evidence_run`/`EvidenceScope`/`GeometryHandoff`/`GeometrySubject` the graduation spine, `mesh/quality.closure_fold` the watertight gate — quality tiers below the scan producers), runtime (`RuntimeRail`/`FAULT_CONF`, `LanePolicy.offload`, `ContentKey` from `rasm.runtime.identity`, `Receipt`).
- Growth: a new primitive class is one `PrimitiveClass` member plus one classification row; a new band statistic is one `DeviationBand` field inside the one fold; a stricter verdict is a `DeviationPolicy` value; a per-storey or per-zone grouping is one segmentation post-fold; zero new surface.
- Boundary: the registered pose is the precondition (the `register` rail from `scan/registration.md#REGISTRATION` supplies it, never re-derived here); the reference GLB is the `mesh/daemon.md#DAEMON` output fetched BY CONTENT KEY over the `Rasm.Bim/Model` seam, never re-tessellated in scan; the watertight truth is `mesh/quality.closure_fold`'s, never a local closure re-computation; learned semantic segmentation is out of host-free CPU scope; no IFC parse, no durable store, no Rhino/GH mutation. The deleted forms: a sync `evaluate` blocking the event loop on the proximity/segmentation kernel; a `lane: LanePolicy | None` accepted yet never composed; a page-local `trace.get_tracer` mint or `@receipted` `_emit` leg re-emitting what the weave's harvest already emits; a bare `raise ValueError` watertight guard where the typed `DeviationFault` composes the quality fold and converts on the fence; a module-`Final` tolerance/fraction/verticality ceiling where the policy rows carry the bars; a compute-interior graduation binding or a `GraduationReceipt.graduates` call where the local `rasm.geometry.graduation` owner mints the vocabulary and `graduates()` returns the local `GeometryHandoff`; a per-primitive `extract_wall`/`extract_slab` family; a `SEGMENT` arm returning a vacuous `compliant=True`; a stat-by-stat re-scan of the signed array; a mutable `list.append` RANSAC loop; a second `ProximityQuery` for the `ATTRIBUTED` triangle ids; an inline `isfinite` band guard where the `SignedField` refinement fences the fold; a hand-rolled point-to-mesh distance kernel; an unsigned-only reduction; and a deviation that re-tessellates the IFC reference.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from enum import StrEnum
from functools import partial
from typing import TYPE_CHECKING, Annotated, Literal, assert_never

import numpy as np
from beartype import beartype
from beartype.vale import Is
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, field
from msgspec.structs import replace

from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_run
from rasm.geometry.mesh.quality import closure_fold
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:  # type-only: every runtime open3d call rides a method on the passed cloud, so no module-level open3d import
    import open3d as o3d

# --- [TYPES] ----------------------------------------------------------------------------


class DeviationStage(StrEnum):
    SEGMENT = "segment"  # classify planar primitives only
    DEVIATE = "deviate"  # fold the element signed band only
    ATTRIBUTED = "attributed"  # per-segment band + per-face triangle-id overlay


class PrimitiveClass(StrEnum):
    SLAB = "slab"  # plane normal ~ world-up
    WALL = "wall"  # plane normal ~ horizontal
    COLUMN = "column"  # vertical wall pair, narrow footprint
    GENERIC = "generic"  # unclassified planar primitive


# finiteness refinement the `@beartype(conf=FAULT_CONF)` fence on `DeviationBand.fold` checks: a
# `NaN`/`±inf` proximity sample raises the canonical violation INSIDE the fence and the faults
# `CLASSIFY` `api` row rails it once, never silently corrupting the extrema or the mask sums.
type SignedField = Annotated[np.ndarray, Is[lambda a: bool(np.isfinite(a).all())]]


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class DeviationFault(Exception):
    # raised INTO the lane fence so an open reference converts through the one BoundaryFault
    # taxonomy — the composed quality-closure gate, never a domain `raise ValueError`.
    tag: Literal["open_reference"] = tag()
    open_reference: str = case()  # the element whose reference GLB the quality closure fold read open


# --- [MODELS] ---------------------------------------------------------------------------


class DeviationBand(Struct, frozen=True, gc=False):
    over_extreme: float  # signed min: worst excess (outside the design solid)
    under_extreme: float  # signed max: worst missing (inside the design solid)
    max_distance: float  # |signed| extremum, the verdict residual
    mean_distance: float
    std_distance: float
    rms_distance: float
    over_count: int  # points with negative sign (over-build)
    under_count: int  # points with positive sign (under-build)
    noncompliant_fraction: float

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def fold(signed: SignedField, working_tolerance: float) -> "DeviationBand":
        # the `SignedField` `Is[isfinite]` refinement fires here under the shared `FAULT_CONF`, so the
        # band the verdict reads is always finite; one reduction, not a stat-by-stat re-scan.
        signed = np.asarray(signed, dtype=np.float64)
        if signed.size == 0:
            return DeviationBand.identity()
        magnitude = np.abs(signed)
        n = magnitude.size
        sign = np.sign(signed)
        # fraction is measured against the tighter per-point working tolerance, not the worst-point
        # ceiling, so the bulk-surface gate stays independent of the max-distance gate.
        over_band = np.clip(magnitude - working_tolerance, 0.0, None)
        return DeviationBand(
            over_extreme=float(signed.min()),
            under_extreme=float(signed.max()),
            max_distance=float(magnitude.max()),
            mean_distance=float(magnitude.mean()),
            std_distance=float(magnitude.std()),
            rms_distance=float(np.linalg.norm(magnitude) / np.sqrt(n)),
            over_count=int(np.where(sign < 0, 1, 0).sum()),
            under_count=int(np.where(sign > 0, 1, 0).sum()),
            noncompliant_fraction=float(np.where(over_band > 0.0, 1, 0).sum() / n),
        )

    @staticmethod
    def identity() -> "DeviationBand":
        return DeviationBand(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0, 0, 0.0)

    def verdict(self, tolerance: float, fraction: float) -> bool:
        return self.max_distance <= tolerance and self.noncompliant_fraction <= fraction

    def facts(self) -> dict[str, object]:
        # native float/int slots the receipts Encoder(enc_hook=repr) renderer serializes without a coerce.
        return {
            "over_extreme": self.over_extreme,
            "under_extreme": self.under_extreme,
            "max_distance": self.max_distance,
            "mean_distance": self.mean_distance,
            "std_distance": self.std_distance,
            "rms_distance": self.rms_distance,
            "over_count": self.over_count,
            "under_count": self.under_count,
            "noncompliant_fraction": self.noncompliant_fraction,
        }


class Segment(Struct, frozen=True, gc=False):
    plane: tuple[float, float, float, float]
    normal: tuple[float, float, float]
    members: tuple[int, ...]  # inlier indices into the ORIGINAL cloud, surviving the iterative peel
    kind: PrimitiveClass
    band: DeviationBand = field(default_factory=DeviationBand.identity)

    @property
    def inliers(self) -> int:
        return len(self.members)

    @staticmethod
    def classify(model: np.ndarray, members: np.ndarray, verticality: tuple[float, float]) -> "Segment":
        up_axis, flat_axis = verticality
        normal = np.asarray(model[:3], dtype=np.float64)
        unit = normal / max(float(np.linalg.norm(normal)), 1e-12)
        vert = abs(float(unit[2]))
        kind = PrimitiveClass.SLAB if vert >= up_axis else PrimitiveClass.WALL if vert <= flat_axis else PrimitiveClass.GENERIC
        return Segment(tuple(float(c) for c in model), tuple(float(c) for c in unit), tuple(int(i) for i in members), kind)

    def attributed(self, signed: np.ndarray, working_tolerance: float) -> "Segment":
        # fold the per-segment signed sub-band over this segment's original-cloud members so the
        # `ATTRIBUTED` overlay groups deviation by primitive; the index survives the `_segment` peel.
        return replace(self, band=DeviationBand.fold(signed[list(self.members)], working_tolerance))


class DeviationPolicy(Struct, frozen=True):
    distance_threshold: float = 0.02
    ransac_n: int = 3
    num_iterations: int = 1000
    max_planes: int = 8
    tolerance: float = 0.05  # worst-point hard ceiling: no point may exceed it — a policy row, never a module Final
    working_tolerance: float = 0.02  # tighter per-point acceptance band the fraction measures
    fraction: float = 0.10  # max share of points allowed past the working band
    up_axis: float = 0.85  # |n . up| slab threshold; the live policy tunes it per scan campaign
    flat_axis: float = 0.35  # |n . up| wall threshold

    @property
    def segment_args(self) -> tuple[float, int, int]:
        return (self.distance_threshold, self.ransac_n, self.num_iterations)

    @property
    def verticality(self) -> tuple[float, float]:
        return (self.up_axis, self.flat_axis)


class DeviationResult(Struct, frozen=True):
    stage: DeviationStage
    element: str
    band: DeviationBand
    segments: tuple[Segment, ...] = ()
    triangle_ids: tuple[int, ...] = ()
    compliant: bool = False

    @staticmethod
    def of(
        stage: DeviationStage,
        element: str,
        band: DeviationBand,
        policy: DeviationPolicy,
        *,
        segments: tuple[Segment, ...] = (),
        triangle_ids: tuple[int, ...] = (),
    ) -> "DeviationResult":
        # SEGMENT carries no measured deviation, so it never asserts a compliant verdict and never graduates.
        compliant = stage is not DeviationStage.SEGMENT and band.verdict(policy.tolerance, policy.fraction)
        return DeviationResult(stage, element, band, segments, triangle_ids, compliant)

    def contribute(self) -> tuple[Receipt, ...]:
        # the runtime `Receipt.of(owner, evidence)` two-argument contract minting the `fact` case.
        kinds = {f"class.{c.value}": sum(s.kind is c for s in self.segments) for c in PrimitiveClass}
        facts: dict[str, object] = {"stage": self.stage.value, "compliant": self.compliant, **self.band.facts(), **kinds}
        return (Receipt.of("geometry.scan.deviation", ("emitted", self.element, facts)),)

    def graduates(self, evidence_key: ContentKey, policy: DeviationPolicy) -> GeometryHandoff:
        # two measured keys against two policy-row ceilings: the worst point AND the bulk-surface share.
        # SEGMENT carries an identity band, not a measurement: its measured dict stays EMPTY so the
        # spine's unmeasured-ceiling law breaches the crossing — a zero-magnitude band never admits.
        measured: dict[str, float] = (
            {}
            if self.stage is DeviationStage.SEGMENT
            else {"max_distance": self.band.max_distance, "noncompliant_fraction": self.band.noncompliant_fraction}
        )
        return GeometryHandoff.of(
            GeometrySubject.SCAN_DEVIATION,
            evidence_key,
            measured,
            {"max_distance": policy.tolerance, "noncompliant_fraction": policy.fraction},
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _query(reference_glb: bytes, cloud: "o3d.geometry.PointCloud", element: str) -> tuple["trimesh.proximity.ProximityQuery", np.ndarray]:
    import trimesh  # noqa: PLC0415

    mesh = trimesh.load_mesh(io.BytesIO(reference_glb), file_type="glb")
    # the watertight truth is the quality owner's ONE closure fold (quality tiers below the scan
    # producers); an open reference raises the typed fault the lane fence converts — signed sign is
    # unreliable on a non-watertight reference, and the daemon welds vertices upstream.
    if not closure_fold(mesh).watertight:
        raise DeviationFault(open_reference=element)
    # `ProximityQuery` amortizes the one rtree triangle-index build across the whole point batch and
    # both the `signed_distance` and `on_surface` reads, never a fresh index per one-shot call.
    return trimesh.proximity.ProximityQuery(mesh), np.asarray(cloud.points)


def _segment(cloud: "o3d.geometry.PointCloud", policy: DeviationPolicy) -> tuple[Segment, ...]:
    # the iterative RANSAC peel is a stateful unfold, not a mutable-accumulator loop: `Block.unfold`
    # threads the `(remainder, surviving, depth)` state — `surviving` mapping each remainder index
    # back to its ORIGINAL-cloud index across the `select_by_index(invert=True)` complement — emitting
    # one classified `Segment` per dominant plane until the `max_planes` budget or the `ransac_n`
    # floor terminates the generator with `Nothing`, never a `break`-guarded `list.append`.
    type State = tuple["o3d.geometry.PointCloud", np.ndarray, int]

    def peel(state: State) -> Option[tuple[Segment, State]]:
        remainder, surviving, depth = state
        if depth >= policy.max_planes or len(remainder.points) < policy.ransac_n:
            return Nothing
        model, inliers = remainder.segment_plane(*policy.segment_args)
        segment = Segment.classify(np.asarray(model), surviving[inliers], policy.verticality)
        peeled = remainder.select_by_index(inliers, invert=True)
        return Some((segment, (peeled, np.delete(surviving, inliers), depth + 1)))

    return tuple(Block.unfold(peel, (cloud, np.arange(len(cloud.points)), 0)))


def _deviation_kernel(
    cloud: "o3d.geometry.PointCloud", reference_glb: bytes, element: str, stage: DeviationStage, policy: DeviationPolicy
) -> DeviationResult:
    # the module-level picklable kernel the lane offloads; a watertight breach or non-finite band
    # raises here and converts through the lane's async_boundary onto the rail.
    match stage:
        case DeviationStage.SEGMENT:
            return DeviationResult.of(stage, element, DeviationBand.identity(), policy, segments=_segment(cloud, policy))
        case DeviationStage.DEVIATE:
            query, points = _query(reference_glb, cloud, element)
            band = DeviationBand.fold(query.signed_distance(points), policy.working_tolerance)
            return DeviationResult.of(stage, element, band, policy)
        case DeviationStage.ATTRIBUTED:
            query, points = _query(reference_glb, cloud, element)
            # one persistent rtree index serves the element band, the per-segment grouping, AND the
            # per-face triangle ids; `on_surface` returns `(points, distances, triangle_id)`, so the
            # colored-overlay attribution rides the same index the band reads, never a second build.
            signed = query.signed_distance(points)
            _, _, triangle_id = query.on_surface(points)
            segments = tuple(s.attributed(signed, policy.working_tolerance) for s in _segment(cloud, policy))
            return DeviationResult.of(
                stage,
                element,
                DeviationBand.fold(signed, policy.working_tolerance),
                policy,
                segments=segments,
                triangle_ids=tuple(int(t) for t in np.asarray(triangle_id)),
            )
        case unreachable:
            assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------


class ScanDeviation(Struct, frozen=True):
    lane: LanePolicy
    policy: DeviationPolicy = DeviationPolicy()

    async def evaluate(
        self, cloud: "o3d.geometry.PointCloud", reference_glb: bytes, element: str, stage: DeviationStage
    ) -> "RuntimeRail[DeviationResult]":
        # the one composed entry: the graduation weave (seeded span + fence + harvest) wraps the lane
        # offload — the reference GLB arrives BY CONTENT KEY (the Rasm.Bim/Model seam; scan never
        # re-tessellates) — and the weave's harvest emits the conforming `DeviationResult` exactly
        # once on the cleared Ok; no page-local receipt leg exists.
        return await evidence_run(
            EvidenceScope.SCAN_DEVIATION,
            f"evaluate.{stage}",
            partial(self.lane.offload, _deviation_kernel, cloud, reference_glb, element, stage, self.policy),
        )
```
