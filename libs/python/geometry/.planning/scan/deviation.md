# [PY_GEOMETRY_SCAN_DEVIATION]

Scan-vs-model deviation and primitive extraction — the AEC payoff of the host-free scan plane, on top of the registered pose. `ScanDeviation` folds one construction-verification pipeline discriminated by a `DeviationStage` request value, never parallel modes: `SEGMENT` runs RANSAC plane segmentation classifying dominant planar primitives into the `PrimitiveClass` vocabulary by plane-normal axis, `DEVIATE` folds the signed nearest-surface deviation between the registered cloud and the IFC-tessellated reference into one `DeviationBand`, and `ATTRIBUTED` composes both so the per-primitive grouping and the per-face triangle-id attribution ride the one surface-projection pass the colored overlay reads. `signed_distance` is positive inside the watertight design solid and negative outside, so under-build (missing material, positive) and over-build (excess, negative) separate; the verdict reads the absolute band against tolerance while the overlay reads the sign and the triangle id. Where an element arrives carrying the `scan/registration#REGISTRATION` non-rigid arm's per-point deformation magnitudes, `DeformationSplit` partitions that same signed band once more — into the part the recovered field explains and the rigid residual it cannot — so settlement, deflection, and bowing separate from construction error and the compliance verdict is read on the residual a rigid re-fit still owes.

A registered transform from `scan/registration#REGISTRATION` is the precondition, never re-derived here; a deformation field is that same producer's optional second product, arriving as the magnitude band alone on the element carrier — the warped positions ARE the element's cloud, so nothing crosses twice and no warp is re-solved here. The registered clouds arrive as the `scan/ingestion#INGESTION` `Cloud` array carrier, while the reference surface is one generated `ArtifactRef` resolved through the injected `ArtifactTransfer`. `evaluate` retains the helper-owned path for the one GLB decode, watertight proof, and `mesh/spatial#SPATIAL` index build; no raw bytes, retired geometry carrier, or second hash crosses. The proximity index folds the whole element set through one batched `SpatialQuery.Proximity` sweep, so N elements share one decode, one proof, and one amortized surface index. Each deviation graduates as `GeometrySubject.SCAN_DEVIATION` keyed to the IFC element GlobalId, with the reference SHA-256 included as an evidence coordinate rather than reinterpreted as the semantic `ContentKey`.

## [01]-[INDEX]

- [02]-[DEVIATION]: plane/primitive segmentation, `PrimitiveClass` classification, the folded signed `DeviationBand`, and its rigid-versus-deformation partition under one stage-discriminated `async` owner over the open3d RANSAC kernel and the `mesh/spatial` proximity capsule.

## [02]-[DEVIATION]

- Owner: `ScanDeviation` holds one injected `ArtifactTransfer`, discriminates by `DeviationStage` over registered `Cloud` carriers and a generated GLB `ArtifactRef`, and carries the composition `ScopeKey` its weave and charter records stamp. `Element` is the request carrier — cloud, IFC GlobalId, and the optional per-point deformation magnitudes — a STRUCT rather than a tuple, because the arity probe at `evaluate` must separate one element from a sequence of them and a tuple of elements is itself a tuple. `DeviationBand.fold` runs the whole signed reduction once and `verdict(tolerance, fraction)` keeps the band math in one place; `DeformationSplit.fold` runs the second reduction over that same signed field, folding its rigid residual through the SAME band fold so the two halves are measured by identical statistics; `Segment` carries the plane model, unit normal, original-cloud inlier indices, and the `PrimitiveClass` the plane-normal axis resolves, plus a per-segment band and its own split under `ATTRIBUTED`; `DeviationPolicy` carries every ceiling as a value-object row — segmentation gains, the worst-point `tolerance`, the tighter per-point `working_tolerance`, the noncompliant `fraction`, the slab/wall verticality thresholds — never a module `Final`.
- Cases: `DeviationStage` — `SEGMENT` (RANSAC outlier-peel oversegmentation classifying dominant planar primitives), `DEVIATE` (the signed band folded once over each element), `ATTRIBUTED` (both composed — per-`Segment` band and the per-point triangle-id map off the same projection pass). Three arms of one pipeline keyed by the request value, never three parallel result shapes; `SEGMENT` returns an identity zero-magnitude band the `verdict` reads as the as-yet-unmeasured element, never a vacuous `compliant=True`, so a segmentation-only request never graduates a false-positive handoff. The deformation split is NOT a fourth stage: it is the projection the element's own magnitude band admits, so `DEVIATE` and `ATTRIBUTED` both carry it wherever the non-rigid arm ran and neither grows a mode knob for data the request already shows.
- Entry: `evaluate` is `async` and absorbs arity at the head — one `(cloud, element)` pair or a whole sequence of them against ONE reference — returning the singular result or the ordered `Block`. It admits the reference once through `_admitted`, folds the whole element set through the shared `MeshSpatial` capsule, and threads the optional `upstream` W3C carrier so the deviation span joins the tessellation producer's trace. Admission refuses a watertight-precondition breach or a deformation field misaligned with its own cloud's point count before any query runs — `FiniteField` proves finiteness, never arity, so the shape gate is the element boundary's own; a non-finite band raises inside the picklable module-level kernel and converts through the lane's `async_boundary`; the cleared band records the `rasm.geometry.deviation.*` charter distributions through `_distributed`, parent-side because the worker meter is the no-op.
- Law: the reference is decoded, watertight-proved, and indexed EXACTLY once per evaluation — that is what content-keying the reference buys, so a model-wide pass over N elements pays one GLB decode, one closure fold, and one amortized surface index where a per-element build pays N of each; the surface projection itself is `mesh/spatial#SPATIAL`'s single owner, and a local `ProximityQuery` re-spelled here would fork the index vocabulary and forfeit the batch crossing the capsule already owns.
- Auto: `segment_plane` returns the `[a,b,c,d]` model and the inlier set, `select_by_index(inliers, invert=True)` peels the remainder for the next `Block.unfold` step, and the unit normal's dominant axis resolves `PrimitiveClass` by table lookup, never a per-class extraction method. `SpatialResult.Proximity` carries the signed field and the on-surface triangle ids in one payload, so the attributed overlay reads the third and fourth slots of the SAME pass rather than paying a second query. `noncompliant_fraction` measures against the tighter `working_tolerance`, independent of the worst-point `tolerance` ceiling, so the bulk-surface gate and the max-distance gate stay separate. The split explains at most what was measured at each point — `minimum(field, |signed|)` — because an unclipped subtraction lets a warp larger than the deviation manufacture a residual with the sign inverted, and the residual keeps the ORIGINAL sign so over-build and under-build survive the partition the overlay reads them by. `compliant` then reads the CONSTRUCTION band: the rigid residual where a field partitioned it, the whole band where none did, so a deflecting slab within its structural allowance stops failing a construction tolerance that never described it, and the deformation extremum answers its own gate against the working band — a field inside that band is indistinguishable from the residual the band already admits.
- Receipt: `DeviationResult.contribute` yields the one `emitted`-phase `Receipt.of("rasm.geometry.scan.deviation", ("emitted", element, facts))` the weave's harvest emits, the band facts produced once through `DeviationBand.facts` so receipt and graduation ledger read the same fold. `graduates` hands `GeometryHandoff.of(...)` TWO measured keys — `max_distance` against `policy.tolerance` and `noncompliant_fraction` against `policy.fraction` — so an element clearing on its worst point alone but out of band on the bulk surface does not cross clean; both read the same construction band `compliant` reads, so a ledger verdict and a receipt verdict can never disagree, and `deformation_extreme` joins as a third key against the working band ONLY where a field existed, since an unmeasured key breaches by the spine's own law and a rigidly registered element must not fail a monitoring bar it was in no position to measure. A `SEGMENT` result hands an EMPTY measured dict so that same unmeasured-ceiling law breaches it. That subject keys to the IFC GlobalId so the per-element deviation reaches the .NET owner system and the TS viewer as a colored overlay. The receipt keeps the ELEMENT CENSUS — verdict, folded band, split summary, segment tally, per-class roster — and `frame` carries the ROWS at the finest grain the stage produced: one row per `Segment` with its class, inliers, plane, normal, own sub-band, and own rigid/deformation partition where segmentation ran, one element row otherwise on a roster that is a strict prefix of the segmented one. The per-segment split is the grain the monitoring answer lives at — an element-wide share averages one deflected span into its rigid neighbours and reports a facade as uniformly marginal. That split is what keeps the receipt from growing with the segment count, and it lands the per-segment bands that a class-count census flattens away — the evidence a facade or slab verdict is actually read on.
- Packages: `open3d` (the `PointCloud.segment_plane`/`select_by_index` RANSAC peel, one module-scope `lazy import` so the marked distribution stays cold until the peel runs), `trimesh` (path-based GLB decode at reference admission), runtime `transport/artifact` (`ArtifactTransfer.fetch` and its verified helper-owned path), `numpy`, `beartype`, `expression`, `msgspec`, the geometry graduation/mesh/scan owners named by the fence, and runtime rails.
- Growth: a new primitive class is one `PrimitiveClass` member and one classification row; a new band statistic is one `DeviationBand` field inside the one fold and reaches the rigid half free, since the split folds through that same band; a new deformation statistic is one `DeformationSplit` field; a stricter verdict is a `DeviationPolicy` value; a per-storey or per-zone grouping is one segmentation post-fold; a new geometric probe against the reference is one `SpatialQuery` case at `mesh/spatial#SPATIAL`, already batched by the shared capsule.
- Boundary: the registered pose and deformation field are `scan/registration#REGISTRATION`'s; this owner partitions a field it is handed and never solves a warp. The generated `ArtifactRef` identifies the reference body through SHA-256 and extent; `ArtifactTransfer` proves and owns the fetched path, while the surface index is `mesh/spatial#SPATIAL`'s and watertight truth `mesh/quality.closure_fold`'s. No IFC parse, durable-store implementation, raw GLB body, or Rhino/GH mutation enters here.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Mapping, Sequence
from enum import StrEnum
from functools import partial
from pathlib import Path
from typing import Annotated, Final, Literal, assert_never, overload

import numpy as np
from beartype import beartype
from beartype.vale import Is
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, field
from msgspec.structs import replace

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
from rasm.runtime.transport.artifact import ArtifactTransfer
from rasm.contracts.rasm.contracts.artifact.artifact_pb import ArtifactRef
from rasm.geometry.mesh.quality import closure_fold
from rasm.geometry.mesh.spatial import MeshSpatial, SpatialQuery, SpatialResult
from rasm.geometry.scan.ingestion import Cloud
from rasm.runtime.faults import FAULT_CONF, TERMINAL, Catch, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ScopeKey
from rasm.runtime.shapes import admitted, custody
from rasm.runtime.workers import Kernel, KernelTrait

lazy import open3d as o3d
lazy import trimesh

# --- [TYPES] ----------------------------------------------------------------------------


class DeviationStage(StrEnum):
    SEGMENT = "segment"
    DEVIATE = "deviate"
    ATTRIBUTED = "attributed"


class PrimitiveClass(StrEnum):
    SLAB = "slab"
    WALL = "wall"
    COLUMN = "column"
    GENERIC = "generic"


type FiniteField = Annotated[np.ndarray, Is[lambda a: bool(np.isfinite(a).all())]]


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class DeviationFault(Exception):
    tag: Literal["open_reference", "misaligned_field"] = tag()
    open_reference: str = case()
    misaligned_field: tuple[tuple[str, int, int], ...] = case()


# --- [TABLES] ---------------------------------------------------------------------------

DEV_ELEMENTS: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.DEVIATION, point="element", arm="boundary", defect="field-misaligned", retriability=TERMINAL
)
DEV_INTEGRITY: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.DEVIATION,
    point="reference",
    arm="boundary",
    defect="reference-refused",
    retriability=TERMINAL,
    slots=("proof",),
)
DEV_ADMISSION: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.DEVIATION,
    point="reference.admission",
    arm="config",
    defect="reference-refused",
    retriability=TERMINAL,
    slots=("phase",),
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([DEV_ELEMENTS, DEV_INTEGRITY, DEV_ADMISSION]))

_GLB_RAISES: Final[Catch] = (DeviationFault, IndexError, OSError, TypeError, ValueError)

# --- [MODELS] ---------------------------------------------------------------------------


class Element(Struct, frozen=True, gc=False):
    cloud: Cloud
    element: str
    deformation: Option[np.ndarray] = Nothing


class DeviationBand(Struct, frozen=True, gc=False):
    over_extreme: float
    under_extreme: float
    max_distance: float
    mean_distance: float
    std_distance: float
    rms_distance: float
    over_count: int
    under_count: int
    noncompliant_fraction: float

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def fold(signed: FiniteField, working_tolerance: float) -> "DeviationBand":
        signed = np.asarray(signed, dtype=np.float64)
        if signed.size == 0:
            return DeviationBand.identity()
        magnitude = np.abs(signed)
        n = magnitude.size
        sign = np.sign(signed)
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


class DeformationSplit(Struct, frozen=True, gc=False):
    rigid: DeviationBand
    deformation_extreme: float
    deformation_mean: float
    deformation_share: float
    deformed_count: int

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def fold(signed: FiniteField, magnitude: FiniteField, working_tolerance: float) -> "DeformationSplit":
        centered = np.asarray(signed, dtype=np.float64)
        absolute = np.abs(centered)
        field = np.asarray(magnitude, dtype=np.float64)
        explained = np.minimum(field, absolute)
        return DeformationSplit(
            rigid=DeviationBand.fold(np.sign(centered) * (absolute - explained), working_tolerance),
            deformation_extreme=float(field.max()),
            deformation_mean=float(field.mean()),
            deformation_share=float(explained.sum() / max(float(absolute.sum()), 1e-12)),
            deformed_count=int(np.where(field > working_tolerance, 1, 0).sum()),
        )

    def facts(self) -> dict[str, object]:
        return {
            "deformation_extreme": self.deformation_extreme,
            "deformation_mean": self.deformation_mean,
            "deformation_share": self.deformation_share,
            "deformed_count": self.deformed_count,
        } | {f"rigid_{name}": value for name, value in self.rigid.facts().items()}


class Segment(Struct, frozen=True, gc=False):
    plane: tuple[float, float, float, float]
    normal: tuple[float, float, float]
    members: tuple[int, ...]
    kind: PrimitiveClass
    band: DeviationBand = field(default_factory=DeviationBand.identity)
    split: Option[DeformationSplit] = Nothing

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

    def attributed(self, signed: np.ndarray, magnitude: "Option[np.ndarray]", working_tolerance: float) -> "Segment":
        members = list(self.members)
        return replace(
            self,
            band=DeviationBand.fold(signed[members], working_tolerance),
            split=magnitude.map(lambda field: DeformationSplit.fold(signed[members], field[members], working_tolerance)),
        )


class DeviationPolicy(Struct, frozen=True):
    distance_threshold: float = 0.02
    ransac_n: int = 3
    num_iterations: int = 1000
    max_planes: int = 8
    tolerance: float = 0.05
    working_tolerance: float = 0.02
    fraction: float = 0.10
    up_axis: float = 0.85
    flat_axis: float = 0.35

    @property
    def segment_args(self) -> tuple[float, int, int]:
        return (self.distance_threshold, self.ransac_n, self.num_iterations)

    @property
    def verticality(self) -> tuple[float, float]:
        return (self.up_axis, self.flat_axis)

    @property
    def spec(self) -> bytes:
        return (
            f"{self.distance_threshold:.17g}|{self.ransac_n}|{self.num_iterations}|{self.max_planes}"
            f"|{self.tolerance:.17g}|{self.working_tolerance:.17g}|{self.fraction:.17g}"
            f"|{self.up_axis:.17g}|{self.flat_axis:.17g}"
        ).encode()


class DeviationResult(Struct, frozen=True):
    stage: DeviationStage
    element: str
    reference_sha256: bytes
    spec: bytes
    band: DeviationBand
    segments: tuple[Segment, ...] = ()
    triangle_ids: tuple[int, ...] = ()
    deformation: Option[DeformationSplit] = Nothing
    compliant: bool = False

    @staticmethod
    def of(
        stage: DeviationStage,
        element: Element,
        reference_sha256: bytes,
        band: DeviationBand,
        policy: DeviationPolicy,
        *,
        segments: tuple[Segment, ...] = (),
        triangle_ids: tuple[int, ...] = (),
        deformation: Option[DeformationSplit] = Nothing,
    ) -> "DeviationResult":
        construction = deformation.map(lambda split: split.rigid).default_value(band)
        compliant = stage is not DeviationStage.SEGMENT and construction.verdict(policy.tolerance, policy.fraction)
        scanned = ContentIdentity.key(
            "scan-element",
            IdentitySource(parts=(element.cloud.digest.memory, element.deformation.map(lambda field: field.tobytes()).default_value(b""))),
        )
        spec = f"{stage.value}|{element.element}|{reference_sha256.hex()}|{scanned.hex}|".encode() + policy.spec
        return DeviationResult(stage, element.element, reference_sha256, spec, band, segments, triangle_ids, deformation, compliant)

    @property
    def census(self) -> dict[str, object]:
        kinds = {f"class.{c.value}": sum(s.kind is c for s in self.segments) for c in PrimitiveClass}
        split = self.deformation.map(lambda held: held.facts()).default_value({})
        return {
            "stage": self.stage.value,
            "compliant": self.compliant,
            "segments": len(self.segments),
            **self.band.facts(),
            **split,
            **kinds,
        }

    def contribute(self) -> tuple[Receipt, ...]:
        return (Receipt.of("rasm.geometry.scan.deviation", ("emitted", self.element, self.census)),)

    def graduates(self, policy: DeviationPolicy) -> GeometryHandoff:
        construction = self.deformation.map(lambda held: held.rigid).default_value(self.band)
        warp = self.deformation.map(lambda held: {"deformation_extreme": held.deformation_extreme}).default_value({})
        measured: dict[str, float] = (
            {}
            if self.stage is DeviationStage.SEGMENT
            else {"max_distance": construction.max_distance, "noncompliant_fraction": construction.noncompliant_fraction} | warp
        )
        return GeometryHandoff.of(
            GeometrySubject.SCAN_DEVIATION,
            evidence_key(GeometrySubject.SCAN_DEVIATION, self.spec),
            measured,
            {"max_distance": policy.tolerance, "noncompliant_fraction": policy.fraction}
            | ({} if not warp else {"deformation_extreme": policy.working_tolerance}),
        )

    def frame(self) -> "RuntimeRail[EvidenceFrame]":
        common: dict[str, list[object]] = {"element": [self.element], "stage": [self.stage.value], "compliant": [self.compliant]}
        rows = Block.of_seq(self.segments)
        split = self.deformation.map(lambda held: held.facts()).default_value({})
        return EvidenceFrame.of(
            GeometrySubject.SCAN_DEVIATION,
            evidence_key(GeometrySubject.SCAN_DEVIATION, self.spec),
            {name: [value] * max(len(rows), 1) for name, value in common.items()}
            | (
                {name: [value] for name, value in (self.band.facts() | split).items()}
                if rows.is_empty()
                else {
                    "kind": [s.kind.value for s in rows],
                    "inliers": [s.inliers for s in rows],
                    **{f"plane_{axis}": [s.plane[i] for s in rows] for i, axis in enumerate(("a", "b", "c", "d"))},
                    **{f"normal_{axis}": [s.normal[i] for s in rows] for i, axis in enumerate(("x", "y", "z"))},
                    **{name: [s.band.facts()[name] for s in rows] for name in DeviationBand.identity().facts()},
                    **{name: [s.split.facts()[name] for s in rows if s.split] for name in split},
                }
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _aligned(elements: Block[Element]) -> "RuntimeRail[Block[Element]]":
    def prove() -> Block[Element]:
        casualties = tuple(
            (row.element, len(row.cloud.positions), int(row.deformation.size))
            for row in elements
            if row.deformation is not None and row.deformation.shape != (len(row.cloud.positions),)
        )
        if casualties:
            raise DeviationFault(misaligned_field=casualties)
        return elements

    return boundary(DEV_ELEMENTS, prove, catch=DeviationFault)


def _admitted(reference: Path, sha256: bytes, lane: LanePolicy, composition: ScopeKey) -> "RuntimeRail[MeshSpatial]":
    def build() -> MeshSpatial:
        mesh = trimesh.load_mesh(reference, file_type="glb")
        if not closure_fold(mesh).watertight:
            raise DeviationFault(open_reference=sha256.hex())
        return MeshSpatial(mesh, lane, composition=composition)

    return boundary(DEV_INTEGRITY, build, catch=_GLB_RAISES)


def _segment(cloud: "o3d.geometry.PointCloud", policy: DeviationPolicy) -> tuple[Segment, ...]:
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
    return tuple(_segment(row.cloud.legacy(), policy) for row in elements)


def _deviated(
    stage: DeviationStage,
    element: Element,
    reference_sha256: bytes,
    projected: SpatialResult,
    segments: tuple[Segment, ...],
    policy: DeviationPolicy,
) -> DeviationResult:
    match projected:
        case SpatialResult(tag="proximity", proximity=(_, _, triangle_ids, signed)):
            field = element.deformation
            band = DeviationBand.fold(signed, policy.working_tolerance)
            attributed = tuple(s.attributed(signed, field, policy.working_tolerance) for s in segments)
            return DeviationResult.of(
                stage,
                element,
                reference_sha256,
                band,
                policy,
                segments=attributed,
                triangle_ids=tuple(int(t) for t in np.asarray(triangle_ids)) if stage is DeviationStage.ATTRIBUTED else (),
                deformation=field.map(lambda magnitude: DeformationSplit.fold(signed, magnitude, policy.working_tolerance)),
            )
        case _ as unreachable:
            assert_never(unreachable)


def _distributed(result: DeviationResult, composition: ScopeKey) -> DeviationResult:
    if result.stage is not DeviationStage.SEGMENT:
        split = result.deformation.map(lambda held: held.facts()).default_value({})
        charter_record(GeometrySubject.SCAN_DEVIATION, result.band.facts() | split, composition=composition)
    return result


# --- [SERVICES] -------------------------------------------------------------------------


class ScanDeviation(Struct, frozen=True):
    lane: LanePolicy
    artifacts: ArtifactTransfer
    policy: DeviationPolicy = DeviationPolicy()
    composition: ScopeKey = DEFAULT_SCOPE

    @overload
    async def evaluate(
        self, reference: ArtifactRef, element: Element, stage: DeviationStage, upstream: Mapping[str, str] | None = None
    ) -> "RuntimeRail[DeviationResult]": ...
    @overload
    async def evaluate(
        self, reference: ArtifactRef, element: Sequence[Element], stage: DeviationStage, upstream: Mapping[str, str] | None = None
    ) -> "RuntimeRail[Block[DeviationResult]]": ...
    @custody(DEV_INTEGRITY)
    @admitted(DEV_ADMISSION)
    async def evaluate(
        self, reference: ArtifactRef, element: Element | Sequence[Element], stage: DeviationStage, upstream: Mapping[str, str] | None = None
    ) -> "RuntimeRail[DeviationResult] | RuntimeRail[Block[DeviationResult]]":
        lone = isinstance(element, Element)
        elements = Block.singleton(element) if lone else Block.of_seq(element)
        async with self.artifacts.fetch(reference) as owned:
            railed = await evidence_run(
                EvidenceScope.SCAN_DEVIATION,
                f"evaluate.{stage}",
                partial(self._folded, reference.sha256, owned.path, elements, stage),
                upstream=upstream,
                composition=self.composition,
            )
            return railed.map(lambda kept: kept.head()) if lone else railed

    async def _folded(
        self,
        reference_sha256: bytes,
        reference: Path,
        elements: Block[Element],
        stage: DeviationStage,
    ) -> "RuntimeRail[Block[DeviationResult]]":
        match _aligned(elements).bind(lambda _kept: _admitted(reference, reference_sha256, self.lane, self.composition)):
            case Result(tag="ok", ok=spatial):
                return (await self._banded(spatial, reference_sha256, elements, stage)).map(
                    lambda kept: kept.map(lambda result: _distributed(result, self.composition))
                )
            case Result(tag="error") as refused:
                return refused

    async def _banded(
        self, spatial: MeshSpatial, reference_sha256: bytes, elements: Block[Element], stage: DeviationStage
    ) -> "RuntimeRail[Block[DeviationResult]]":
        segmented = await self._segments(elements) if stage is not DeviationStage.DEVIATE else Ok(elements.map(lambda _e: ()))
        if stage is DeviationStage.SEGMENT:
            return segmented.map(
                lambda peeled: elements.zip(peeled).map(
                    lambda pair: DeviationResult.of(
                        stage, pair[0], reference_sha256, DeviationBand.identity(), self.policy, segments=pair[1]
                    )
                )
            )
        projected = await spatial.query(elements.map(lambda row: SpatialQuery.Proximity(row.cloud.positions, signed=True)))
        return projected.map2(
            segmented,
            lambda results, peeled: elements.zip(results).zip(peeled).map(
                lambda pair: _deviated(stage, pair[0][0], reference_sha256, pair[0][1], pair[1], self.policy)
            ),
        )

    async def _segments(self, elements: Block[Element]) -> "RuntimeRail[Block[tuple[Segment, ...]]]":
        railed = await self.lane.offload(Kernel.of(_segment_kernel, KernelTrait.HOSTILE), tuple(elements), self.policy)
        return railed.map(Block.of_seq)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
