# [PY_GEOMETRY_SCAN_DEVIATION]

Scan-vs-model deviation and primitive extraction — the AEC payoff of a host-free scan companion, on top of the registered pose. `ScanDeviation` folds one construction-verification pipeline discriminated by a `DeviationStage` request value, never parallel modes: `SEGMENT` runs RANSAC plane segmentation classifying dominant planar primitives into the `PrimitiveClass` vocabulary by plane-normal axis, `DEVIATE` folds the signed nearest-surface deviation between the registered cloud and the IFC-tessellated reference into one `DeviationBand`, and `ATTRIBUTED` composes both so the per-primitive grouping and the per-face triangle-id attribution ride the one amortized proximity index the colored overlay reads. `signed_distance` is positive inside the watertight design solid and negative outside, so under-build (missing material, positive) and over-build (excess, negative) separate; the verdict reads the absolute band against tolerance while the overlay reads the sign and the triangle id.

A registered transform from `scan/registration.md#REGISTRATION` is the precondition, never re-derived here; the registered cloud arrives as the `scan/ingestion.md#INGESTION` `Cloud` array carrier, and the reference surface arrives by content key — the welded GLB from `mesh/daemon.md#DAEMON` fetched over the `Rasm.Bim/Model` seam, scan never re-tessellating. `_query` folds that reference through `mesh/quality`'s public `closure_fold` watertight gate and raises the typed `DeviationFault` on an open reference. `evaluate` runs `async`, riding the `lane.offload` crossing on `Kernel.of(_deviation_kernel, KernelTrait.HOSTILE)` under the `evidence_run` graduation weave seeded on `EvidenceScope.SCAN_DEVIATION` — the `open3d` RANSAC band imports under no isolated subinterpreter, so the kernel rides the warm process pool, the `Cloud` arrays cross the pickle seam whole, and the RANSAC arm re-inflates through `Cloud.legacy()` where its native peel begins. Each deviation graduates through the geometry-minted `rasm.geometry.graduation` spine as `GeometrySubject.SCAN_DEVIATION` keyed to the IFC element GlobalId, `graduates()` returning the local `GeometryHandoff` whose `wire()` projection is the compute crossing and whose ceilings are `DeviationPolicy` rows.

## [01]-[INDEX]

- [01]-[DEVIATION]: plane/primitive segmentation, `PrimitiveClass` classification, and the folded signed `DeviationBand` under one stage-discriminated `async` owner over the open3d RANSAC and trimesh `ProximityQuery` surfaces.

## [02]-[DEVIATION]

- Owner: `ScanDeviation`, the frozen owner discriminating by `DeviationStage` over a registered `Cloud` carrier and a content-keyed GLB reference. `DeviationBand.fold` runs the whole signed reduction once and `verdict(tolerance, fraction)` keeps the band math in one place; `Segment` carries the plane model, unit normal, original-cloud inlier indices, and the `PrimitiveClass` the plane-normal axis resolves, and a per-segment band under `ATTRIBUTED`; `DeviationPolicy` carries every ceiling as a value-object row — segmentation gains, the worst-point `tolerance`, the tighter per-point `working_tolerance`, the noncompliant `fraction`, the slab/wall verticality thresholds — never a module `Final`.
- Cases: `DeviationStage` — `SEGMENT` (RANSAC outlier-peel oversegmentation classifying dominant planar primitives), `DEVIATE` (the signed band folded once over the whole element), `ATTRIBUTED` (both composed — per-`Segment` band and the `on_surface` triangle-id map off the same index). Three arms of one pipeline keyed by the request value, never three parallel result shapes; `SEGMENT` returns an identity zero-magnitude band the `verdict` reads as the as-yet-unmeasured element, never a vacuous `compliant=True`, so a segmentation-only request never graduates a false-positive handoff.
- Entry: `evaluate` is `async` — it admits the registered cloud, the reference GLB bytes, an element GlobalId, a stage, and an optional `upstream` W3C band (the reference producer's `traceparent` carried beside the content-keyed GLB), and returns `RuntimeRail[DeviationResult]` by composing `evidence_run` over `lane.offload(_deviation_kernel, …)` with the band threaded to the weave's `_linked` fold, so the deviation span joins the tessellation producer's trace. A watertight-precondition breach or a non-finite band raises inside the picklable module-level kernel and converts through the lane's `async_boundary` onto the rail the weave records; the cleared band records the `rasm.geometry.deviation.*` charter distributions through `_distributed`, parent-side because the worker meter is the no-op.
- Auto: `segment_plane` returns the `[a,b,c,d]` model and the inlier set, `select_by_index(inliers, invert=True)` peels the remainder for the next `Block.unfold` step, and the unit normal's dominant axis resolves `PrimitiveClass` by table lookup, never a per-class extraction method. `_query` amortizes one `ProximityQuery` rtree build across the whole batch and both the `signed_distance` and `on_surface` reads. `noncompliant_fraction` measures against the tighter `working_tolerance`, independent of the worst-point `tolerance` ceiling, so the bulk-surface gate and the max-distance gate stay separate.
- Receipt: `DeviationResult.contribute` yields the one `emitted`-phase `Receipt.of("rasm.geometry.scan.deviation", ("emitted", element, facts))` the weave's harvest emits, the band facts produced once through `DeviationBand.facts` so receipt and graduation ledger read the same fold. `graduates` hands `GeometryHandoff.of(GeometrySubject.SCAN_DEVIATION, key, measured, ceilings)` TWO measured keys — `max_distance` against `policy.tolerance` and `noncompliant_fraction` against `policy.fraction` — so an element clearing on its worst point alone but out of band on the bulk surface does not cross clean; a `SEGMENT` result hands an EMPTY measured dict so the spine's unmeasured-ceiling law breaches it. That subject keys to the IFC GlobalId so the per-element deviation reaches the C# owner system and the TS viewer as a colored overlay. `frame` projects the same band facts and classification census as one `EvidenceFrame` row through the graduation port, so the data plane aggregates deviation evidence beside the energy `ResultFrame` with zero receipt re-parsing.
- Packages: `open3d` (the `PointCloud.segment_plane`/`select_by_index` RANSAC peel), `trimesh` (the GLB reader and the `proximity.ProximityQuery`/`signed_distance`/`on_surface` amortized index), `numpy` (the band and peel folds), `beartype` (the `SignedField` finiteness refinement under `FAULT_CONF`), `expression` (`Block.unfold` the peel state machine), `msgspec` (the frozen carriers), geometry (`evidence_run`/`charter_record`/`EvidenceScope`/`GeometryHandoff`/`GeometrySubject` the graduation spine, `Cloud` the ingestion-minted crossing carrier, `mesh/quality.closure_fold` the watertight gate — quality tiers below the scan producers), runtime (`RuntimeRail`/`FAULT_CONF`, `LanePolicy.offload`/`Kernel`, `ContentKey`, `Receipt`).
- Growth: a new primitive class is one `PrimitiveClass` member and one classification row; a new band statistic is one `DeviationBand` field inside the one fold; a stricter verdict is a `DeviationPolicy` value; a per-storey or per-zone grouping is one segmentation post-fold.
- Boundary: the registered pose is `scan/registration.md#REGISTRATION`'s; the reference GLB is `mesh/daemon.md#DAEMON`'s output fetched by content key over the `Rasm.Bim/Model` seam, never re-tessellated here; the watertight truth is `mesh/quality.closure_fold`'s; learned semantic segmentation is out of host-free CPU scope; no IFC parse, no durable store, no Rhino/GH mutation.

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

from rasm.geometry.graduation import EvidenceFrame, EvidenceScope, GeometryHandoff, GeometrySubject, charter_record, evidence_run
from rasm.geometry.mesh.quality import closure_fold
from rasm.geometry.scan.ingestion import Cloud
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:  # annotation-only names; every runtime call rides a worker-side boundary import or a rebuilt handle
    import open3d as o3d
    import trimesh

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


# finiteness refinement the `FAULT_CONF` fence on `DeviationBand.fold` checks: a `NaN`/`±inf` sample rails once.
type SignedField = Annotated[np.ndarray, Is[lambda a: bool(np.isfinite(a).all())]]


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class DeviationFault(Exception):
    # raised into the lane fence so an open reference converts through the BoundaryFault taxonomy.
    tag: Literal["open_reference"] = tag()
    open_reference: str = case()  # the element whose reference GLB the closure fold read open


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
        # `SignedField` refinement fires here, so the band the verdict reads is always finite.
        signed = np.asarray(signed, dtype=np.float64)
        if signed.size == 0:
            return DeviationBand.identity()
        magnitude = np.abs(signed)
        n = magnitude.size
        sign = np.sign(signed)
        over_band = np.clip(magnitude - working_tolerance, 0.0, None)  # fraction gate on the tighter working band
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
        # per-segment sub-band over this segment's original-cloud members, surviving the peel.
        return replace(self, band=DeviationBand.fold(signed[list(self.members)], working_tolerance))


class DeviationPolicy(Struct, frozen=True):
    distance_threshold: float = 0.02
    ransac_n: int = 3
    num_iterations: int = 1000
    max_planes: int = 8
    tolerance: float = 0.05  # worst-point hard ceiling
    working_tolerance: float = 0.02  # tighter per-point band the fraction measures against
    fraction: float = 0.10  # max share past the working band
    up_axis: float = 0.85  # |n · up| slab threshold
    flat_axis: float = 0.35  # |n · up| wall threshold

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
        compliant = stage is not DeviationStage.SEGMENT and band.verdict(policy.tolerance, policy.fraction)
        return DeviationResult(stage, element, band, segments, triangle_ids, compliant)

    def contribute(self) -> tuple[Receipt, ...]:
        kinds = {f"class.{c.value}": sum(s.kind is c for s in self.segments) for c in PrimitiveClass}
        facts: dict[str, object] = {"stage": self.stage.value, "compliant": self.compliant, **self.band.facts(), **kinds}
        return (Receipt.of("rasm.geometry.scan.deviation", ("emitted", self.element, facts)),)

    def graduates(self, evidence_key: ContentKey, policy: DeviationPolicy) -> GeometryHandoff:
        # a SEGMENT identity band hands an EMPTY measured dict, so the unmeasured-ceiling law breaches it.
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

    def frame(self, evidence_key: ContentKey) -> EvidenceFrame:
        # one columnar row per evaluated element — band facts + classification census — through the graduation
        # frame port; the data plane aggregates across elements, so per-face attribution stays overlay evidence.
        kinds = {f"class.{c.value}": [sum(s.kind is c for s in self.segments)] for c in PrimitiveClass}
        table: dict[str, list[object]] = {
            "element": [self.element],
            "stage": [self.stage.value],
            "compliant": [self.compliant],
            **{name: [value] for name, value in self.band.facts().items()},
            **kinds,
        }
        return EvidenceFrame.of(GeometrySubject.SCAN_DEVIATION, evidence_key, table)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _query(reference_glb: bytes, cloud: Cloud, element: str) -> tuple["trimesh.proximity.ProximityQuery", np.ndarray]:
    import trimesh  # noqa: PLC0415

    mesh = trimesh.load_mesh(io.BytesIO(reference_glb), file_type="glb")
    # signed sign is unreliable on a non-watertight reference, so an open closure raises the typed fault.
    if not closure_fold(mesh).watertight:
        raise DeviationFault(open_reference=element)
    # one rtree index amortized across the batch and both the `signed_distance` and `on_surface` reads.
    return trimesh.proximity.ProximityQuery(mesh), cloud.positions


def _segment(cloud: "o3d.geometry.PointCloud", policy: DeviationPolicy) -> tuple[Segment, ...]:
    # consumes the worker-side legacy rebuild; the peel is a stateful `Block.unfold`, not a mutable accumulator:
    # `surviving` maps each remainder index back to its ORIGINAL-cloud index across the `invert=True` complement.
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


def _deviation_kernel(cloud: Cloud, reference_glb: bytes, element: str, stage: DeviationStage, policy: DeviationPolicy) -> DeviationResult:
    # module-level HOSTILE kernel: the Cloud arrays cross the pickle seam; the RANSAC arms alone re-inflate the legacy handle.
    match stage:
        case DeviationStage.SEGMENT:
            return DeviationResult.of(stage, element, DeviationBand.identity(), policy, segments=_segment(cloud.legacy(), policy))
        case DeviationStage.DEVIATE:
            query, points = _query(reference_glb, cloud, element)
            band = DeviationBand.fold(query.signed_distance(points), policy.working_tolerance)
            return DeviationResult.of(stage, element, band, policy)
        case DeviationStage.ATTRIBUTED:
            query, points = _query(reference_glb, cloud, element)
            # `on_surface` returns `(points, distances, triangle_id)` off the same index the band reads.
            signed = query.signed_distance(points)
            _, _, triangle_id = query.on_surface(points)
            segments = tuple(s.attributed(signed, policy.working_tolerance) for s in _segment(cloud.legacy(), policy))
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


def _distributed(result: DeviationResult) -> DeviationResult:
    # parent-side charter projection: the HOSTILE kernel's meter is the worker's no-op, so the SCAN_DEVIATION
    # charter rows record here off the returned band — spellings derived, never hand-picked; a SEGMENT identity
    # band records nothing.
    if result.stage is not DeviationStage.SEGMENT:
        charter_record(GeometrySubject.SCAN_DEVIATION, result.band.facts(), EvidenceScope.SCAN_DEVIATION.value)
    return result


# --- [SERVICES] -------------------------------------------------------------------------


class ScanDeviation(Struct, frozen=True):
    lane: LanePolicy
    policy: DeviationPolicy = DeviationPolicy()

    async def evaluate(
        self, cloud: Cloud, reference_glb: bytes, element: str, stage: DeviationStage, upstream: str | None = None
    ) -> "RuntimeRail[DeviationResult]":
        # graduation weave wraps the lane offload; its harvest emits the conforming result once, `upstream` — the
        # reference producer's W3C band carried beside the content-keyed GLB — joins that trace as a Link at span
        # open, and the cleared band records its charter distributions. HOSTILE is the declared trait because the
        # open3d band imports under no isolated subinterpreter.
        rail = await evidence_run(
            EvidenceScope.SCAN_DEVIATION,
            f"evaluate.{stage}",
            partial(self.lane.offload, Kernel.of(_deviation_kernel, KernelTrait.HOSTILE), cloud, reference_glb, element, stage, self.policy),
            upstream=upstream,
        )
        return rail.map(_distributed)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
