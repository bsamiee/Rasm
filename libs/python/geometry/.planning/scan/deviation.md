# [PY_GEOMETRY_SCAN_DEVIATION]

Scan-vs-model deviation and primitive extraction — the AEC payoff of the host-free scan plane, on top of the registered pose. `ScanDeviation` folds one construction-verification pipeline discriminated by a `DeviationStage` request value, never parallel modes: `SEGMENT` runs RANSAC plane segmentation classifying dominant planar primitives into the `PrimitiveClass` vocabulary by plane-normal axis, `DEVIATE` folds the signed nearest-surface deviation between the registered cloud and the IFC-tessellated reference into one `DeviationBand`, and `ATTRIBUTED` composes both so the per-primitive grouping and the per-face triangle-id attribution ride the one surface-projection pass the colored overlay reads. `signed_distance` is positive inside the watertight design solid and negative outside, so under-build (missing material, positive) and over-build (excess, negative) separate; the verdict reads the absolute band against tolerance while the overlay reads the sign and the triangle id.

A registered transform from `scan/registration#REGISTRATION` is the precondition, never re-derived here; the registered clouds arrive as the `scan/ingestion#INGESTION` `Cloud` array carrier, and the reference surface arrives as the `mesh/cad#BRIDGE` `GlbArtifact` — the wire-keyed welded GLB from `mesh/daemon#DAEMON` fetched over the `Rasm.Bim/Model` seam, scan never re-tessellating and never re-keying. The proximity index is `mesh/spatial#SPATIAL`'s `MeshSpatial`: this page constructs ONE capsule per reference, gates it through `mesh/quality`'s public `closure_fold` on admission, and folds the whole element set through its batched `SpatialQuery.Proximity` sweep, so N elements share one decode, one watertight proof, and one amortized surface index instead of re-spelling a `ProximityQuery` build per element. `evaluate` runs `async` under the `evidence_run` graduation weave seeded on `EvidenceScope.SCAN_DEVIATION`; the RANSAC peel keeps its own `KernelTrait.HOSTILE` kernel because the `open3d` band imports under no isolated subinterpreter and its batch amortizes the process crossing across the element set. Each deviation graduates as `GeometrySubject.SCAN_DEVIATION` keyed to the IFC element GlobalId, `graduates()` returning the local `GeometryHandoff` on a key its own `spec` projection derives from the reference key, the element, and the stage — whose `wire()` projection is the compute crossing and whose ceilings are `DeviationPolicy` rows.

## [01]-[INDEX]

- [02]-[DEVIATION]: plane/primitive segmentation, `PrimitiveClass` classification, and the folded signed `DeviationBand` under one stage-discriminated `async` owner over the open3d RANSAC kernel and the `mesh/spatial` proximity capsule.

## [02]-[DEVIATION]

- Owner: `ScanDeviation`, the frozen owner discriminating by `DeviationStage` over registered `Cloud` carriers and a wire-keyed GLB reference, carrying the composition `ScopeKey` its weave and charter records stamp. `DeviationBand.fold` runs the whole signed reduction once and `verdict(tolerance, fraction)` keeps the band math in one place; `Segment` carries the plane model, unit normal, original-cloud inlier indices, and the `PrimitiveClass` the plane-normal axis resolves, and a per-segment band under `ATTRIBUTED`; `DeviationPolicy` carries every ceiling as a value-object row — segmentation gains, the worst-point `tolerance`, the tighter per-point `working_tolerance`, the noncompliant `fraction`, the slab/wall verticality thresholds — never a module `Final`.
- Cases: `DeviationStage` — `SEGMENT` (RANSAC outlier-peel oversegmentation classifying dominant planar primitives), `DEVIATE` (the signed band folded once over each element), `ATTRIBUTED` (both composed — per-`Segment` band and the per-point triangle-id map off the same projection pass). Three arms of one pipeline keyed by the request value, never three parallel result shapes; `SEGMENT` returns an identity zero-magnitude band the `verdict` reads as the as-yet-unmeasured element, never a vacuous `compliant=True`, so a segmentation-only request never graduates a false-positive handoff.
- Entry: `evaluate` is `async` and absorbs arity at the head — one `(cloud, element)` pair or a whole sequence of them against ONE reference — returning the singular result or the ordered `Block`. It admits the reference once through `_admitted`, folds the whole element set through the shared `MeshSpatial` capsule, and threads the optional `upstream` W3C carrier so the deviation span joins the tessellation producer's trace. A watertight-precondition breach refuses at admission before any query runs; a non-finite band raises inside the picklable module-level kernel and converts through the lane's `async_boundary`; the cleared band records the `rasm.geometry.deviation.*` charter distributions through `_distributed`, parent-side because the worker meter is the no-op.
- Law: the reference is decoded, watertight-proved, and indexed EXACTLY once per evaluation — that is what content-keying the reference buys, so a model-wide pass over N elements pays one GLB decode, one closure fold, and one amortized surface index where a per-element build pays N of each; the surface projection itself is `mesh/spatial#SPATIAL`'s single owner, and a local `ProximityQuery` re-spelled here would fork the index vocabulary and forfeit the batch crossing the capsule already owns.
- Auto: `segment_plane` returns the `[a,b,c,d]` model and the inlier set, `select_by_index(inliers, invert=True)` peels the remainder for the next `Block.unfold` step, and the unit normal's dominant axis resolves `PrimitiveClass` by table lookup, never a per-class extraction method. `SpatialResult.Proximity` carries the signed field and the on-surface triangle ids in one payload, so the attributed overlay reads the third and fourth slots of the SAME pass rather than paying a second query. `noncompliant_fraction` measures against the tighter `working_tolerance`, independent of the worst-point `tolerance` ceiling, so the bulk-surface gate and the max-distance gate stay separate.
- Receipt: `DeviationResult.contribute` yields the one `emitted`-phase `Receipt.of("rasm.geometry.scan.deviation", ("emitted", element, facts))` the weave's harvest emits, the band facts produced once through `DeviationBand.facts` so receipt and graduation ledger read the same fold. `graduates` hands `GeometryHandoff.of(...)` TWO measured keys — `max_distance` against `policy.tolerance` and `noncompliant_fraction` against `policy.fraction` — so an element clearing on its worst point alone but out of band on the bulk surface does not cross clean; a `SEGMENT` result hands an EMPTY measured dict so the spine's unmeasured-ceiling law breaches it. That subject keys to the IFC GlobalId so the per-element deviation reaches the C# owner system and the TS viewer as a colored overlay. The receipt keeps the ELEMENT CENSUS — verdict, folded band, segment tally, per-class roster — and `frame` carries the ROWS at the finest grain the stage produced: one row per `Segment` with its class, inliers, plane, normal, and own sub-band where segmentation ran, one element row otherwise on a roster that is a strict prefix of the segmented one. That split is what keeps the receipt from growing with the segment count, and it lands the per-segment bands that a class-count census flattens away — the evidence a facade or slab verdict is actually read on.
- Packages: `open3d` (the `PointCloud.segment_plane`/`select_by_index` RANSAC peel, one module-scope `lazy import` so the marked distribution stays cold until the peel runs), `trimesh` (the GLB decode at reference admission — the surface index itself belongs to `mesh/spatial`), `numpy` (the band and peel folds), `beartype` (the `SignedField` finiteness refinement under `FAULT_CONF`), `expression` (`Block.unfold` the peel state machine), `msgspec` (the frozen carriers), geometry (`evidence_run`/`charter_record`/`EvidenceScope`/`GeometryHandoff`/`GeometrySubject`/`evidence_key` the graduation spine, `Cloud` the ingestion-minted crossing carrier, `GlbArtifact` the cad-minted reference carrier, `MeshSpatial`/`SpatialQuery`/`SpatialResult` the mesh proximity owner, `mesh/quality.closure_fold` the watertight gate — mesh tiers below the scan producers), runtime (`RuntimeRail`/`FAULT_CONF`, `LanePolicy.offload`/`Kernel`, `ContentKey`, `Receipt`).
- Growth: a new primitive class is one `PrimitiveClass` member and one classification row; a new band statistic is one `DeviationBand` field inside the one fold; a stricter verdict is a `DeviationPolicy` value; a per-storey or per-zone grouping is one segmentation post-fold; a new geometric probe against the reference is one `SpatialQuery` case at `mesh/spatial#SPATIAL`, already batched by the shared capsule.
- Boundary: the registered pose is `scan/registration#REGISTRATION`'s; the reference GLB and its wire key are `mesh/daemon#DAEMON`'s output carried on the `mesh/cad#BRIDGE` `GlbArtifact`, fetched by content key over the `Rasm.Bim/Model` seam, never re-tessellated and never re-hashed here; the surface index is `mesh/spatial#SPATIAL`'s and the watertight truth `mesh/quality.closure_fold`'s; learned semantic segmentation is out of host-free CPU scope; no IFC parse, no durable store, no Rhino/GH mutation.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from collections.abc import Mapping, Sequence
from enum import StrEnum
from functools import partial
from typing import Annotated, Literal, assert_never, overload

import numpy as np
from beartype import beartype
from beartype.vale import Is
from expression import Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, field
from msgspec.structs import replace

from rasm.geometry.graduation import (
    EvidenceFrame,
    EvidenceScope,
    GeometryHandoff,
    GeometrySubject,
    charter_record,
    evidence_key,
    evidence_run,
)
from rasm.geometry.mesh.cad import GlbArtifact
from rasm.geometry.mesh.quality import closure_fold
from rasm.geometry.mesh.spatial import MeshSpatial, SpatialQuery, SpatialResult
from rasm.geometry.scan.ingestion import Cloud
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

# the RANSAC peel's provider, interpreter-marked: the module-scope proxy keeps it cold until the segmentation
# kernel runs, so a DEVIATE-only evaluation never loads it and the GLB decode leg imports nothing native.
lazy import open3d as o3d
lazy import trimesh

# --- [TYPES] ----------------------------------------------------------------------------


class DeviationStage(StrEnum):
    SEGMENT = "segment"  # classify planar primitives only
    DEVIATE = "deviate"  # fold the element signed band only
    ATTRIBUTED = "attributed"  # per-segment band + per-point triangle-id overlay


class PrimitiveClass(StrEnum):
    SLAB = "slab"  # plane normal ~ world-up
    WALL = "wall"  # plane normal ~ horizontal
    COLUMN = "column"  # vertical wall pair, narrow footprint
    GENERIC = "generic"  # unclassified planar primitive


# one element under evaluation: its registered cloud and the IFC GlobalId the deviation keys to.
type Element = tuple[Cloud, str]

# finiteness refinement the `FAULT_CONF` fence on `DeviationBand.fold` checks: a `NaN`/`±inf` sample rails once.
type SignedField = Annotated[np.ndarray, Is[lambda a: bool(np.isfinite(a).all())]]


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class DeviationFault(Exception):
    # raised into the admission fence so an open reference converts through the BoundaryFault taxonomy BEFORE any
    # query runs — one refusal for the whole element set, never one per element against a reference already proved bad.
    tag: Literal["open_reference"] = tag()
    open_reference: str = case()  # the wire key of the reference GLB the closure fold read open


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
    reference_key: ContentKey  # the reference GLB's own wire key, the evidence key's other half
    band: DeviationBand
    segments: tuple[Segment, ...] = ()
    triangle_ids: tuple[int, ...] = ()
    compliant: bool = False

    @staticmethod
    def of(
        stage: DeviationStage,
        element: str,
        reference_key: ContentKey,
        band: DeviationBand,
        policy: DeviationPolicy,
        *,
        segments: tuple[Segment, ...] = (),
        triangle_ids: tuple[int, ...] = (),
    ) -> "DeviationResult":
        compliant = stage is not DeviationStage.SEGMENT and band.verdict(policy.tolerance, policy.fraction)
        return DeviationResult(stage, element, reference_key, band, segments, triangle_ids, compliant)

    @property
    def census(self) -> dict[str, object]:
        # the ELEMENT-grain census the receipt publishes: the verdict, the element's own folded band, the segment
        # tally, and the per-class roster. It answers "what did this element come to" — the row-grain facts behind
        # it ride the frame, so the receipt never grows with the segment count and a consumer reading a support
        # bundle is not handed a table it has to re-aggregate.
        kinds = {f"class.{c.value}": sum(s.kind is c for s in self.segments) for c in PrimitiveClass}
        return {"stage": self.stage.value, "compliant": self.compliant, "segments": len(self.segments), **self.band.facts(), **kinds}

    def contribute(self) -> tuple[Receipt, ...]:
        return (Receipt.of("rasm.geometry.scan.deviation", ("emitted", self.element, self.census)),)

    @property
    def spec(self) -> bytes:
        # the byte projection that DEFINES this evidence: which reference, which element, which stage — two runs of
        # one element against one reference key identically, and a re-tessellated reference keys apart.
        return f"{self.stage.value}|{self.element}|{self.reference_key.hex}".encode()

    def graduates(self, policy: DeviationPolicy) -> GeometryHandoff:
        # a SEGMENT identity band hands an EMPTY measured dict, so the unmeasured-ceiling law breaches it.
        measured: dict[str, float] = (
            {}
            if self.stage is DeviationStage.SEGMENT
            else {"max_distance": self.band.max_distance, "noncompliant_fraction": self.band.noncompliant_fraction}
        )
        return GeometryHandoff.of(
            GeometrySubject.SCAN_DEVIATION,
            evidence_key(GeometrySubject.SCAN_DEVIATION, self.spec),
            measured,
            {"max_distance": policy.tolerance, "noncompliant_fraction": policy.fraction},
        )

    def frame(self) -> "RuntimeRail[EvidenceFrame]":
        # the columnar port at the FINEST grain the stage produced. A segmented evaluation is one row per `Segment`
        # — its class, its inlier count, its plane and unit normal, and the sub-band folded over its own members —
        # because that per-segment band is the evidence a facade or slab verdict is actually read on, and it exists
        # on this result today with no carrier at all: the receipt census counts segments by class and the extremum
        # of one segment's band never reaches a consumer. A `DEVIATE` evaluation produced no segments, so the
        # element IS the row and the roster is exactly the common prefix of the segmented one — a strict subset, so
        # a consumer reads element, stage, and band columns unconditionally and the segment columns where the grain
        # carries them, rather than meeting a forged plane on a row no segmentation ever ran.
        common: dict[str, list[object]] = {"element": [self.element], "stage": [self.stage.value], "compliant": [self.compliant]}
        rows = Block.of_seq(self.segments)
        return EvidenceFrame.of(
            GeometrySubject.SCAN_DEVIATION,
            evidence_key(GeometrySubject.SCAN_DEVIATION, self.spec),
            {name: [value] * max(len(rows), 1) for name, value in common.items()}
            | (
                {name: [value] for name, value in self.band.facts().items()}
                if rows.is_empty()
                else {
                    "kind": [s.kind.value for s in rows],
                    "inliers": [s.inliers for s in rows],
                    **{f"plane_{axis}": [s.plane[i] for s in rows] for i, axis in enumerate(("a", "b", "c", "d"))},
                    **{f"normal_{axis}": [s.normal[i] for s in rows] for i, axis in enumerate(("x", "y", "z"))},
                    **{name: [s.band.facts()[name] for s in rows] for name in DeviationBand.identity().facts()},
                }
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _admitted(reference: GlbArtifact, lane: LanePolicy, composition: ScopeKey) -> "RuntimeRail[MeshSpatial]":
    # ONE decode, ONE watertight proof, ONE index owner per evaluation — the whole point of the reference being
    # content-keyed. A signed distance has no reliable sign off an open surface, so the closure gate refuses the
    # evaluation here rather than letting every element fold a meaningless band.
    def build() -> MeshSpatial:
        mesh = trimesh.load_mesh(io.BytesIO(reference.bytes), file_type="glb")
        if not closure_fold(mesh).watertight:
            raise DeviationFault(open_reference=reference.wire_key.hex)
        return MeshSpatial(mesh, lane, composition=composition)

    return boundary("scan.deviation.reference", build)


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


def _segment_kernel(elements: tuple[Element, ...], policy: DeviationPolicy) -> tuple[tuple[Segment, ...], ...]:
    # module-level HOSTILE kernel over the WHOLE element set: the Cloud arrays cross the pickle seam and each cloud
    # re-inflates its legacy handle where its own peel begins, so one crossing carries the batch and the open3d band
    # imports once on the worker rather than once per element.
    return tuple(_segment(cloud.legacy(), policy) for cloud, _ in elements)


def _deviated(
    stage: DeviationStage,
    element: Element,
    reference_key: ContentKey,
    projected: SpatialResult,
    segments: tuple[Segment, ...],
    policy: DeviationPolicy,
) -> DeviationResult:
    # one projection pass carries BOTH reads the overlay needs: the fourth slot is the signed field the band folds
    # and the third the per-point on-surface triangle id, so the attributed arm pays no second query.
    match projected:
        case SpatialResult(tag="proximity", proximity=(_, _, triangle_ids, signed)):
            band = DeviationBand.fold(signed, policy.working_tolerance)
            attributed = tuple(s.attributed(signed, policy.working_tolerance) for s in segments)
            return DeviationResult.of(
                stage,
                element[1],
                reference_key,
                band,
                policy,
                segments=attributed,
                triangle_ids=tuple(int(t) for t in np.asarray(triangle_ids)) if stage is DeviationStage.ATTRIBUTED else (),
            )
        case _ as unreachable:
            assert_never(unreachable)


def _distributed(result: DeviationResult, composition: ScopeKey) -> DeviationResult:
    # parent-side charter projection: the HOSTILE kernel's meter is the worker's no-op, so the SCAN_DEVIATION
    # charter rows record here off the returned band — spellings derived, never hand-picked; a SEGMENT identity
    # band records nothing, and the composition key partitions an embedded root's series from the process root's.
    if result.stage is not DeviationStage.SEGMENT:
        charter_record(GeometrySubject.SCAN_DEVIATION, result.band.facts(), composition=composition)
    return result


# --- [SERVICES] -------------------------------------------------------------------------


class ScanDeviation(Struct, frozen=True):
    lane: LanePolicy
    policy: DeviationPolicy = DeviationPolicy()
    composition: ScopeKey = DEFAULT_SCOPE  # the custody key every weave and charter record stamps

    @overload
    async def evaluate(
        self, reference: GlbArtifact, element: Element, stage: DeviationStage, upstream: Mapping[str, str] | None = None
    ) -> "RuntimeRail[DeviationResult]": ...
    @overload
    async def evaluate(
        self, reference: GlbArtifact, element: Sequence[Element], stage: DeviationStage, upstream: Mapping[str, str] | None = None
    ) -> "RuntimeRail[Block[DeviationResult]]": ...
    async def evaluate(
        self, reference: GlbArtifact, element: Element | Sequence[Element], stage: DeviationStage, upstream: Mapping[str, str] | None = None
    ) -> "RuntimeRail[DeviationResult] | RuntimeRail[Block[DeviationResult]]":
        # arity absorbs at the head — one element is the degenerate one-member model pass — and `upstream`, the
        # reference producer's W3C carrier beside the content-keyed GLB, joins that trace as a Link at span open.
        lone = isinstance(element, tuple)
        elements = Block.singleton(element) if lone else Block.of_seq(element)
        railed = await evidence_run(
            EvidenceScope.SCAN_DEVIATION,
            f"evaluate.{stage}",
            partial(self._folded, reference, elements, stage),
            upstream=upstream,
            composition=self.composition,
        )
        return railed.map(lambda kept: kept.head()) if lone else railed

    async def _folded(self, reference: GlbArtifact, elements: Block[Element], stage: DeviationStage) -> "RuntimeRail[Block[DeviationResult]]":
        # admit the reference ONCE, then fold every element through that one capsule; a refused admission
        # short-circuits the whole set rather than repeating an identical refusal per element.
        match _admitted(reference, self.lane, self.composition):
            case Result(tag="ok", ok=spatial):
                return (await self._banded(spatial, reference, elements, stage)).map(
                    lambda kept: kept.map(lambda result: _distributed(result, self.composition))
                )
            case Result(tag="error") as refused:
                return refused

    async def _banded(
        self, spatial: MeshSpatial, reference: GlbArtifact, elements: Block[Element], stage: DeviationStage
    ) -> "RuntimeRail[Block[DeviationResult]]":
        # SEGMENT touches no index at all, so it never admits the proximity crossing; the other two stages fold the
        # WHOLE element set through one batched sweep — one offload, one mesh, one amortized surface index.
        segmented = await self._segments(elements) if stage is not DeviationStage.DEVIATE else Ok(elements.map(lambda _e: ()))
        if stage is DeviationStage.SEGMENT:
            return segmented.map(
                lambda peeled: elements.zip(peeled).map(
                    lambda pair: DeviationResult.of(stage, pair[0][1], reference.wire_key, DeviationBand.identity(), self.policy, segments=pair[1])
                )
            )
        projected = await spatial.query(elements.map(lambda row: SpatialQuery.Proximity(row[0].positions, signed=True)))
        # the applicative join is the RAIL's: two rails combine through `map2`, and the aligned per-element walk
        # inside it is `zip`, which proves equal length at the seam rather than trusting two independent folds.
        return projected.map2(
            segmented,
            lambda results, peeled: elements.zip(results).zip(peeled).map(
                lambda pair: _deviated(stage, pair[0][0], reference.wire_key, pair[0][1], pair[1], self.policy)
            ),
        )

    async def _segments(self, elements: Block[Element]) -> "RuntimeRail[Block[tuple[Segment, ...]]]":
        # HOSTILE is the declared trait because the open3d RANSAC band imports under no isolated subinterpreter;
        # the whole element set crosses in ONE hop, so the batch amortizes the process crossing and the band import.
        railed = await self.lane.offload(Kernel.of(_segment_kernel, KernelTrait.HOSTILE), tuple(elements), self.policy)
        return railed.map(Block.of_seq)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
