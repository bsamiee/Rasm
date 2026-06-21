# [PY_GEOMETRY_SCAN_DEVIATION]

Scan-vs-model deviation and primitive extraction — the central AEC value of a host-free scan companion, on top of the registered pose. `ScanDeviation` is one stage-discriminated owner folding a single construction-verification pipeline rather than two parallel modes: RANSAC plane/primitive segmentation classifying the extracted planar primitives into the `PrimitiveClass` vocabulary (wall/slab/column/generic) by plane-normal axis, the per-segment signed nearest-surface deviation between the registered cloud and the IFC-tessellated GLB over the trimesh proximity surface, and the folded `DeviationBand` — signed extrema, the absolute `max`/`mean`/`std`/`rms` magnitudes, and the over-build/under-build inlier counts that the construction-verification verdict and the colored overlay both read. The two stages are arms of one `evaluate` fold keyed by a `DeviationStage` request value, never a flat mode flag returning a vacuous result: `SEGMENT` extracts and classifies, `DEVIATE` folds the signed band over the whole element, and `ATTRIBUTED` composes both so the per-primitive grouping and the `closest_point` triangle-id attribution ride the same pass the overlay egress reads. The registered transform from `scan/registration.md#REGISTRATION` is the precondition; the reference surface is the `mesh/daemon.md#DAEMON` GLB output keyed by `ContentIdentity`. The deviation graduates through the compute `graduation/handoff.md#GRADUATION` `HandoffAxis` geometry case as the compute-owned `scan-deviation` `GeometrySubject` literal keyed to the IFC element GlobalId, the answer to whether the built geometry matches the IFC design within tolerance — `trimesh.proximity.signed_distance` is positive inside the watertight design solid and negative outside, so under-build (built material missing, the as-built cloud point lies inside the design surface, positive) is distinguished from over-build (excess material, the cloud point lies outside, negative); the construction-verification verdict reads on the absolute band against tolerance while the colored overlay reads the sign and the per-face triangle id. This owner is the CONSUMER of the `scan-deviation` subject the compute `graduation/handoff.md#GRADUATION` owner produces — it imports the literal and the `GraduationReceipt.graduates` admission fold, supplies only its measured/ceiling deviation ledger, and never authors the compute interior.

## [01]-[INDEX]

- [01]-[DEVIATION]: plane/primitive segmentation with `PrimitiveClass` classification and the folded signed `DeviationBand` under one stage-discriminated owner over the open3d segmentation and trimesh proximity surfaces, the cross-cutting concerns folded as rails — `boundary` the one exception-to-fault aspect, `LanePolicy.offload` the CPU-offload seam, the receipt contributor the telemetry sink.

## [02]-[DEVIATION]

- Owner: `ScanDeviation` — the frozen owner discriminating by `DeviationStage` request value over a registered `o3d.geometry.PointCloud` and an IFC-tessellated GLB reference, carrying a `DeviationPolicy` value object (segmentation gains, the worst-point `tolerance` ceiling, the tighter per-point `working_tolerance` acceptance band, the noncompliant `fraction` bar, the slab/wall verticality thresholds) with derived `segment_args`/`verticality` projections rather than loose scalars, parity with the sibling `RegistrationPolicy`/`ReconPolicy`; `DeviationBand` the value object folding the signed proximity array into the signed `over_extreme`/`under_extreme`, the absolute `max`/`mean`/`std`/`rms`, the over/under inlier counts, and the `noncompliant_fraction` residual, with a `DeviationBand.fold` factory that runs the whole numpy reduction once and a `verdict(tolerance, fraction)` method the `DeviationResult` reads against the policy band so the band math lives in one place; `Segment` the classified-primitive value object holding the `[a,b,c,d]` plane model, the unit normal, the inlier count, and the `PrimitiveClass` the plane-normal axis resolves, with a per-segment `DeviationBand` when the `ATTRIBUTED` stage groups deviation by primitive; `DeviationResult` the per-element typed receipt carrying the stage, the element GlobalId, the folded element `DeviationBand`, the classified `Segment` tuple, and the optional triangle-id attribution map, with a `DeviationResult.of` factory that defaults the empty arms so the three stage arms construct through one keyword fold; `PrimitiveClass` the wall/slab/column/generic vocabulary the plane-normal axis classifies.
- Cases: `DeviationStage` rows `SEGMENT` (RANSAC `segment_plane` iterative outlier-peel oversegmentation extracting and classifying the dominant planar primitives into `PrimitiveClass`), `DEVIATE` (the signed nearest-surface `proximity.signed_distance` query folded once into the element `DeviationBand` and the construction-verification verdict), and `ATTRIBUTED` (the full pass composing both — per-`Segment` `DeviationBand` grouping plus the `proximity.closest_point` triangle-id attribution the colored-overlay egress at the graduation seam reads) — matched by `match`/`assert_never`, each binding the engine and the result arm that owns it. The three stages are arms of one pipeline keyed by the request value, never three parallel result shapes; `SEGMENT` returns a `DeviationResult` carrying the classified segments and an identity (zero-magnitude) band that the `verdict` reads as the as-yet-unmeasured element, never a vacuous `compliant=True`, so a segmentation-only request never graduates a false-positive compliant handoff.
- Entry: `ScanDeviation.evaluate` admits the registered cloud, the reference GLB bytes, an element GlobalId, and a `DeviationStage`, and returns a `RuntimeRail[DeviationResult]` through the one `boundary` exception-to-fault conversion (the rail aspect every package returns through, so the interior raises only inside the `boundary` thunk and the watertight precondition lifts to a `BoundaryFault` exactly once at that egress); the optional `lane: LanePolicy | None` field is the imported per-subinterpreter offload seam the Growth bullet hands the multi-second proximity/segmentation kernel across through the one `LanePolicy.offload(kernel, *args)` call (the same seam the registration/ingestion/reconstruction siblings carry), `LanePolicy` the imported lane field so the seam exists and the lane never imports the kernel. The interior `_dispatch` runs the stage through one `match`: the `SEGMENT` arm folds `segment_plane` over the residual cloud iteratively (re-segmenting the `select_by_index(invert=True)` remainder) into a classified `Segment` tuple, the `DEVIATE` arm loads the GLB through `trimesh.load_mesh` over an `io.BytesIO` stream forcing a `Trimesh`, gates the watertight precondition on `Trimesh.is_watertight` for a reliable sign, and folds `proximity.signed_distance` once into the element `DeviationBand` through `DeviationBand.fold`, and the `ATTRIBUTED` arm threads both — grouping the signed band per classified segment by the inlier index sets and composing `proximity.closest_point` for the per-face triangle-id map.
- Auto: `PointCloud.segment_plane(distance_threshold, ransac_n, num_iterations)` returns the `[a,b,c,d]` plane model plus the inlier index set, `select_by_index(inliers, invert=True)` peels the remainder so the next segmentation extracts the next dominant plane (the iterative oversegmentation that separates adjacent walls/slabs), and the unit normal `[a,b,c]` resolves the `PrimitiveClass` by its dominant axis (`abs` of the world-up component over a verticality threshold is a slab/floor, near-zero is a wall, the column being a wall pair the grouping folds) — the classification is a data-table lookup over the normal axis, never a per-class extraction method; `trimesh.proximity.signed_distance(mesh, points)` returns the per-point signed distance (positive inside the watertight design solid, negative outside — no triangle id), and `DeviationBand.fold` runs the whole reduction once: the signed `over_extreme` is `signed.min` (excess material outside), the `under_extreme` is `signed.max` (missing material inside), the absolute band is `numpy` `abs` then `max`/`mean`/`std` plus the `rms` as `linalg.norm(magnitude) / sqrt(n)`, the over/under inlier counts are `count` over the `numpy.where`/`sign` partition of the signed array, and the `noncompliant_fraction` is the fraction of `abs` magnitudes exceeding the tighter per-point `working_tolerance` (distinct from the worst-point `tolerance` ceiling, so the bulk-surface gate stays independent of the max-distance gate rather than collapsing into it) via a `clip`-bounded mask sum — one fold, not a stat-by-stat re-scan; `trimesh.proximity.closest_point(mesh, points)` returns the closest surface point, the unsigned distance, and the triangle id the `ATTRIBUTED` stage composes into the per-face attribution map the colored-overlay egress reads, never inside the band reduction.
- Receipt: `DeviationResult.contribute` emits one `emitted`-phase `Receipt.of` row through `ReceiptContributor` carrying the stage, the element GlobalId, the band's signed extrema and absolute `max`/`mean`/`std`/`rms`, the over/under inlier counts, the `noncompliant_fraction`, the classified-segment count spread by `PrimitiveClass`, and the compliant verdict, the band facts produced once through `DeviationBand.facts` so the receipt and the graduation ledger read the same fold; `DeviationResult.graduates(evidence_key)` produces the geometry `GraduationReceipt` through the compute `GraduationReceipt.graduates` admission fold over `HandoffAxis(geometry=_SUBJECT)`, gating TWO residual keys against the compute owner's per-key ceiling fold — the measured `max_distance` against `_TOLERANCE_CEILING` (the worst absolute deviation the verdict reads) AND the `noncompliant_fraction` against `_FRACTION_CEILING` (so an element whose single worst point clears tolerance but whose bulk surface is out of band does not graduate on the extremum alone) — so a built element exceeding either bar is an `Error(BoundaryFault)` on the graduation rail rather than a graduated handoff, both keys riding the compute owner's residual-over-ceiling `_admit` direction unchanged, never a second admission direction minted here. The subject is typed as the compute-owned `GeometrySubject` `"scan-deviation"` literal (imported from `rasm.compute.graduation.handoff`, never a bare `str`, so an unlisted literal fails at the type boundary); this owner is the CONSUMER of the already-declared subject and the supplier of its measured/ceiling ledger, the residual-over-ceiling fold itself the one admission gate the compute owner owns. The graduated subject is keyed to the IFC element GlobalId so the per-element deviation reaches the C# owner system and the TS viewer as a colored overlay through the one graduation rail.
- Packages: `open3d` (`geometry.PointCloud.segment_plane`/`select_by_index`), `trimesh` (`load_mesh` forcing a `Trimesh` over an `io.BytesIO` GLB stream/`proximity.signed_distance`/`proximity.closest_point`/`Trimesh.is_watertight`), `numpy` (`asarray`/`abs`/`sign`/`where`/`clip`/`max`/`min`/`mean`/`std`/`linalg.norm` over the signed distance array), `msgspec` (`Struct`/`gc=False` on the leaf band/segment value objects), runtime (`RuntimeRail`/`boundary`/`Receipt`/`ContentKey`/`LanePolicy`), compute (`GeometrySubject`/`GraduationReceipt`/`HandoffAxis`). The `open3d` and `trimesh` compiled imports are function-local at boundary scope under `# noqa: PLC0415` per the manifest import policy; this page shares the `open3d` admission row under `python_version<'3.15'` with `ingestion.md`/`registration.md`/`reconstruction.md`.
- Growth: a new primitive class (cylinder/duct) is one `PrimitiveClass` row plus one classification-table arm over the plane-normal axis, never a new extraction method; a new deviation statistic is one field on `DeviationBand` folded inside the one `DeviationBand.fold`; a new tuning axis (the measured tolerance, the noncompliant-fraction bar, the slab/wall verticality thresholds) is one `DeviationPolicy` field threaded through `segment_args`/`verticality` and the `verdict(tolerance, fraction)` read, the graduation contract bars staying the campaign-wide `_TOLERANCE_CEILING`/`_FRACTION_CEILING` rows the graduation ledger passes; a stricter construction-verification bar is one tighter ceiling key, never a second admission direction minted here; the CPU offload of the per-point proximity/segmentation loop is the one `LanePolicy.offload(kernel, *args)` hand-off through the runtime `execution/lanes#LANES` per-subinterpreter variant (`scan/registration.md#REGISTRATION`, `ingestion.md#INGESTION`, and `reconstruction.md#RECONSTRUCTION` share the lane seam); zero new surface, no parallel per-primitive class family.
- Boundary: the registered pose is the precondition (the `register` rail from `scan/registration.md#REGISTRATION` supplies it, never re-derived here); the reference GLB is the `mesh/daemon.md#DAEMON` output, never re-tessellated; learned semantic segmentation is out of host-free CPU scope and informs only the segmentation heuristics; no IFC parse (that is `ifc-analysis`), no durable store, no Rhino/GH mutation; a per-primitive `extract_wall`/`extract_slab` family, a per-mode parallel result class, a `DeviationMode` flag whose `SEGMENT` arm returns a vacuous `compliant=True`, a stat-by-stat re-scan of the signed array instead of one `DeviationBand.fold`, a hand-rolled point-to-mesh distance kernel where `trimesh.proximity` is admitted, an unsigned-only `closest_point` reduction that cannot distinguish over-build from under-build, and a deviation that re-tessellates the IFC reference are the deleted forms — the signed band reads the construction-verification sign, the `closest_point` triangle id feeds only the `ATTRIBUTED` overlay attribution.

```python signature
import io
import numpy as np
from enum import StrEnum
from typing import assert_never

from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt
from rasm.compute.graduation.handoff import GeometrySubject, GraduationReceipt, HandoffAxis

import open3d as o3d


# --- [TYPES] ----------------------------------------------------------------------------


class DeviationStage(StrEnum):
    SEGMENT = "segment"          # classify planar primitives only
    DEVIATE = "deviate"          # fold the element signed band only
    ATTRIBUTED = "attributed"    # per-segment band + per-face triangle-id overlay


class PrimitiveClass(StrEnum):
    SLAB = "slab"        # plane normal ~ world-up
    WALL = "wall"        # plane normal ~ horizontal
    COLUMN = "column"    # vertical wall pair, narrow footprint
    GENERIC = "generic"  # unclassified planar primitive


# --- [CONSTANTS] ------------------------------------------------------------------------

_SUBJECT: GeometrySubject = "scan-deviation"
_TOLERANCE_CEILING: float = 0.05    # worst-point hard ceiling the graduation gate reads
_WORKING_TOLERANCE: float = 0.02    # tighter per-point acceptance band the noncompliant fraction measures
_FRACTION_CEILING: float = 0.10
# verticality of |n . up| defaults; the live policy tunes them per scan campaign.
_UP_AXIS: float = 0.85
_FLAT_AXIS: float = 0.35


# --- [MODELS] ---------------------------------------------------------------------------


class DeviationBand(Struct, frozen=True, gc=False):
    over_extreme: float    # signed min: worst excess (outside the design solid)
    under_extreme: float   # signed max: worst missing (inside the design solid)
    max_distance: float    # |signed| extremum, the verdict residual
    mean_distance: float
    std_distance: float
    rms_distance: float
    over_count: int        # points with negative sign (over-build)
    under_count: int       # points with positive sign (under-build)
    noncompliant_fraction: float

    @staticmethod
    def fold(signed: "np.ndarray", working_tolerance: float) -> "DeviationBand":
        signed = np.asarray(signed, dtype=np.float64)
        if signed.size == 0:
            return DeviationBand.identity()
        magnitude = np.abs(signed)
        n = magnitude.size
        sign = np.sign(signed)
        # fraction is measured against the tighter per-point working tolerance, not the worst-point ceiling,
        # so the bulk-surface gate stays independent of the max-distance gate rather than collapsing into it.
        over_band = np.clip(magnitude - working_tolerance, 0.0, None)
        return DeviationBand(
            over_extreme=float(signed.min()), under_extreme=float(signed.max()),
            max_distance=float(magnitude.max()), mean_distance=float(magnitude.mean()),
            std_distance=float(magnitude.std()), rms_distance=float(np.linalg.norm(magnitude) / np.sqrt(n)),
            over_count=int(np.where(sign < 0, 1, 0).sum()), under_count=int(np.where(sign > 0, 1, 0).sum()),
            noncompliant_fraction=float(np.where(over_band > 0.0, 1, 0).sum() / n),
        )

    @staticmethod
    def identity() -> "DeviationBand":
        return DeviationBand(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0, 0, 0.0)

    def verdict(self, tolerance: float, fraction: float) -> bool:
        return self.max_distance <= tolerance and self.noncompliant_fraction <= fraction

    def facts(self) -> dict[str, str]:
        return {
            "over_extreme": repr(self.over_extreme), "under_extreme": repr(self.under_extreme),
            "max_distance": repr(self.max_distance), "mean_distance": repr(self.mean_distance),
            "std_distance": repr(self.std_distance), "rms_distance": repr(self.rms_distance),
            "over_count": str(self.over_count), "under_count": str(self.under_count),
            "noncompliant_fraction": repr(self.noncompliant_fraction),
        }


class Segment(Struct, frozen=True, gc=False):
    plane: tuple[float, float, float, float]
    normal: tuple[float, float, float]
    inliers: int
    kind: PrimitiveClass
    band: DeviationBand | None = None

    @staticmethod
    def classify(model: "np.ndarray", inliers: int, verticality: tuple[float, float]) -> "Segment":
        up_axis, flat_axis = verticality
        normal = np.asarray(model[:3], dtype=np.float64)
        unit = normal / max(float(np.linalg.norm(normal)), 1e-12)
        vert = abs(float(unit[2]))
        kind = PrimitiveClass.SLAB if vert >= up_axis else PrimitiveClass.WALL if vert <= flat_axis else PrimitiveClass.GENERIC
        return Segment(tuple(float(c) for c in model), tuple(float(c) for c in unit), inliers, kind)


class DeviationPolicy(Struct, frozen=True):
    distance_threshold: float = 0.02
    ransac_n: int = 3
    num_iterations: int = 1000
    max_planes: int = 8
    tolerance: float = _TOLERANCE_CEILING           # worst-point hard ceiling: no point may exceed it
    working_tolerance: float = _WORKING_TOLERANCE    # tighter per-point acceptance band the fraction measures
    fraction: float = _FRACTION_CEILING              # max share of points allowed past the working band
    up_axis: float = _UP_AXIS
    flat_axis: float = _FLAT_AXIS

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
        stage: DeviationStage, element: str, band: DeviationBand, policy: DeviationPolicy,
        *, segments: tuple[Segment, ...] = (), triangle_ids: tuple[int, ...] = (),
    ) -> "DeviationResult":
        # SEGMENT carries no measured deviation, so it never asserts a compliant verdict and never graduates.
        compliant = stage is not DeviationStage.SEGMENT and band.verdict(policy.tolerance, policy.fraction)
        return DeviationResult(stage, element, band, segments, triangle_ids, compliant)

    def contribute(self) -> Receipt:
        kinds = {f"class.{c.value}": sum(s.kind is c for s in self.segments) for c in PrimitiveClass}
        facts = {"stage": self.stage.value, "element": self.element, "compliant": str(self.compliant)}
        facts |= self.band.facts() | {k: str(v) for k, v in kinds.items()}
        return Receipt.of("emitted", "geometry.scan.deviation", self.element, facts)

    def graduates(self, evidence_key: ContentKey) -> RuntimeRail[GraduationReceipt]:
        return GraduationReceipt.graduates(
            "geometry.scan.deviation", HandoffAxis(geometry=_SUBJECT), evidence_key,
            {"max_distance": self.band.max_distance, "noncompliant_fraction": self.band.noncompliant_fraction},
            {"max_distance": _TOLERANCE_CEILING, "noncompliant_fraction": _FRACTION_CEILING},
        )


# --- [SERVICES] -------------------------------------------------------------------------


class ScanDeviation(Struct, frozen=True):
    policy: DeviationPolicy = DeviationPolicy()
    lane: LanePolicy | None = None

    def evaluate(self, cloud: "o3d.geometry.PointCloud", reference_glb: bytes, element: str, stage: DeviationStage) -> "RuntimeRail[DeviationResult]":
        return boundary(f"deviation.{stage}", lambda: self._dispatch(cloud, reference_glb, element, stage))

    def _dispatch(self, cloud: "o3d.geometry.PointCloud", reference_glb: bytes, element: str, stage: DeviationStage) -> DeviationResult:
        match stage:
            case DeviationStage.SEGMENT:
                segments = self._segment(cloud)
                return DeviationResult.of(stage, element, DeviationBand.identity(), self.policy, segments=segments)
            case DeviationStage.DEVIATE:
                signed, _ = self._signed(cloud, reference_glb)
                return DeviationResult.of(stage, element, DeviationBand.fold(signed, self.policy.working_tolerance), self.policy)
            case DeviationStage.ATTRIBUTED:
                import trimesh  # noqa: PLC0415

                signed, mesh = self._signed(cloud, reference_glb)
                _, _, triangle_id = trimesh.proximity.closest_point(mesh, np.asarray(cloud.points))
                segments = self._segment(cloud)
                return DeviationResult.of(
                    stage, element, DeviationBand.fold(signed, self.policy.working_tolerance), self.policy,
                    segments=segments, triangle_ids=tuple(int(t) for t in np.asarray(triangle_id)),
                )
            case unreachable:
                assert_never(unreachable)

    def _signed(self, cloud: "o3d.geometry.PointCloud", reference_glb: bytes) -> tuple["np.ndarray", object]:
        import trimesh  # noqa: PLC0415

        mesh = trimesh.load_mesh(io.BytesIO(reference_glb), file_type="glb")
        if not mesh.is_watertight:  # signed sign is unreliable on a non-watertight reference; daemon welds vertices
            raise ValueError("reference GLB is not watertight; signed deviation requires the welded daemon output")
        return trimesh.proximity.signed_distance(mesh, np.asarray(cloud.points)), mesh

    def _segment(self, cloud: "o3d.geometry.PointCloud") -> tuple[Segment, ...]:
        remainder, found, verticality = cloud, [], self.policy.verticality
        for _ in range(self.policy.max_planes):
            if len(remainder.points) < self.policy.ransac_n:
                break
            model, inliers = remainder.segment_plane(*self.policy.segment_args)
            found.append(Segment.classify(np.asarray(model), len(inliers), verticality))
            remainder = remainder.select_by_index(inliers, invert=True)
        return tuple(found)
```

## [03]-[RESEARCH]

- [SEGMENT_PLANE_ARITY]: the `PointCloud.segment_plane(distance_threshold, ransac_n, num_iterations) -> tuple[list[float], list[int]]` return (the `[a, b, c, d]` plane model and the inlier index list) and the `select_by_index(index, invert=True)` outlier-peel arity are folder-`.api`-confirmed (`open3d.md` rows `segment_plane`/`select_by_index`/`compute_point_cloud_distance`); `Segment.classify` resolves the `PrimitiveClass` purely from the model's `[a,b,c]` normal axis through `numpy.linalg.norm` plus a verticality threshold table (`_UP_AXIS`/`_FLAT_AXIS`), no extra open3d member; the only open item is the owner-local iterative re-segmentation point-count guard and the verticality thresholds, heuristics the live run tunes, not a catalogue dependency.
- [SIGNED_BAND_REDUCTION]: the `DEVIATE`/`ATTRIBUTED` reduction loads the reference GLB through `trimesh.load_mesh(io.BytesIO(reference_glb), file_type="glb")` (folder-`.api`-confirmed `trimesh.md` row [02], "force a `Trimesh` result" over the polymorphic `load` dispatch keyed by `file_type`, never the legacy `force=` kwarg or an unconfirmed `util.wrap_as_stream` helper; `io.BytesIO` is the stdlib file-object adapter the polymorphic reader admits), gates `Trimesh.is_watertight` (`trimesh.md` cached-property row [03]) for a reliable sign, then folds `trimesh.proximity.signed_distance(mesh, points) -> NDArray` (folder-`.api`-confirmed `trimesh.md` row [06], "signed distance field samples", positive inside the watertight solid and negative outside per the trimesh proximity convention) once through `DeviationBand.fold`. The fold composes only `.api`-confirmed numpy members (`numpy.md` rows `abs`/`sign`/`where`/`clip`/`mean`/`std`/`min`/`max`/`sum`/`sqrt`/`linalg.norm`): the over-build (`signed.min`)/under-build (`signed.max`) signed extrema, the absolute `max`/`mean`/`std`/`rms` band (`rms` as `linalg.norm(magnitude)/sqrt(n)`), the over/under inlier counts as the `where(sign<0,1,0).sum()`/`where(sign>0,1,0).sum()` partitions (the catalogue carries no `count_nonzero`, so the count is the boolean-mask `where`+`sum` fold, never an unconfirmed member), and the `noncompliant_fraction` as the `clip(magnitude - tolerance, 0, None)` breach-mask `where`+`sum` over `n` — one reduction, not a stat-by-stat re-scan. `trimesh.proximity.closest_point(mesh, points) -> tuple[NDArray, NDArray, NDArray]` (closest points, unsigned per-point distances, triangle ids — `trimesh.md` row [05]) carries no sign, so it composes ONLY in the `ATTRIBUTED` stage for the colored-overlay per-face attribution at the graduation egress, never inside the band reduction. The watertight-input precondition for a valid `signed_distance` sign (a non-watertight reference returns an unreliable sign) is satisfied because the reference GLB is the `mesh/daemon.md#DAEMON` weld-vertices output; the `_signed` guard raising on a non-watertight mesh converts to a `BoundaryFault` exactly once at the `boundary` egress.

## [04]-[CROSS_FOLDER]

- [GRADUATION_SUBJECT]: the `scan-deviation` `GeometrySubject` literal this owner graduates is already present in the compute `graduation/handoff.md#GRADUATION` `GeometrySubject` union (the producer half compute authored in-pass, joining `registration-transform`, `reconstructed-mesh`, `topology-graph`, `network-graph`, `form-finding`, `numerical-primitive`, and `mesh-algebra`), so `_SUBJECT` imports the literal from `rasm.compute.graduation.handoff` rather than minting a bare `str` — an unlisted literal fails at the `GeometrySubject` type boundary, the compute owner owning the union. This page is the CONSUMER half: `DeviationResult.graduates` routes a TWO-key measured ledger — `max_distance` (the worst absolute deviation) AND `noncompliant_fraction` (the fraction of surface points breaching tolerance) — through the one compute `GraduationReceipt.graduates` admission fold against the `_TOLERANCE_CEILING`/`_FRACTION_CEILING` residual bars, the per-key residual-over-ceiling gate the compute owner owns, not a local admission body, so a built element exceeding either bar — a single worst point over the `_TOLERANCE_CEILING` hard ceiling, OR too large a share of points past the tighter `working_tolerance` acceptance band even where the worst point still clears the ceiling (the two keys independent precisely because the fraction measures the tighter band, never the same ceiling the max key reads) — is an `Error(BoundaryFault)` rather than a graduated handoff, both keys riding the compute owner's existing upper-bound `_admit` direction with no second admission direction minted here — the same two-key residual-ledger shape the sibling `registration.md#REGISTRATION` graduates through (`inlier_rmse`/`misfit`). The subject is distinct from the `reconstructed-mesh` subject `scan/reconstruction.md#RECONSTRUCTION` produces and from the compute `analysis/spatial.md#SPATIAL` "reconstructed boundary, never crossing as a geometry-branch mesh" subject on the same geometry axis: `scan-deviation` carries the per-element signed deviation band keyed to the IFC GlobalId, a non-colliding subject the compute producer admits alongside the present `reconstructed-mesh`. Producer (compute) and consumer (this page) are co-authored in-pass on the one seam; no card, the literal already lands on the compute union and this page consumes it.
