# [PY_GEOMETRY_SCAN_RECONSTRUCTION]

Registered-cloud-to-watertight-mesh reconstruction — the producer of the `reconstructed-mesh` `GeometrySubject` member. `ScanReconstruction` is one frozen owner discriminating by `ReconstructionMethod` row over a registered `o3d.geometry.PointCloud`, parity with the sibling `ScanRegistration`/`ScanDeviation`: the algorithm is a STATIC `TriangleMesh` constructor choice, never a runtime mode flag, so reconstruction selects WHICH open3d constructor builds the surface — `create_from_point_cloud_poisson` (the watertight indicator-function default), `create_from_point_cloud_ball_pivoting` (the detail-preserving rolling-ball surface over the raw samples), `create_from_point_cloud_alpha_shape` (the concave-hull surface for sparse or open scans). The constructor choice is data, not a `match` arm: `_CONSTRUCT` is the one `Map[ReconstructionMethod, ConstructSpec]` behavior table whose row binds each method's static open3d constructor, so a new algorithm is one row, not a new dispatch arm. The shared `estimate_normals`/`orient_normals_consistent_tangent_plane` pre-step conditions every method (Poisson and ball-pivoting both require globally consistent oriented normals), and an optional `cluster_dbscan` split reconstructs each density cluster separately for a multi-object scene. The open3d `TriangleMesh` egresses as GLB through `trimesh.Trimesh.export` (open3d's `io` writes `PLY`/`OBJ`/`STL`/`OFF` only), and watertight conditioning routes the `mesh/repair.md#MESH` `MeshRepairOp.Condition` arm rather than re-implementing repair here.

The closure truth is `mesh/quality`'s — the reconstructed body's watertight/winding/euler/volume/area/components algebra reads ONCE through the quality owner's public `closure_fold` (quality tiers below the scan producers per the folder order), never a page-local `MeshQuality` twin re-computing the same closure with a second component-count strategy: `ReconReceipt` carries the folded `QualityMetrics`, its `facts()` and the graduation residual ledger both projecting from that one fold. The residual ledger carries `nonwatertight` AND `noncontiguous` (the `components - 1` over-segmentation residual, `0.0` for the single welded solid) so a Poisson balloon that closes into two disjoint shells fails the gate the single watertight flag would pass. The entry is `async` and the multi-second Poisson/ball-pivoting solve rides `lane.offload` under the graduation `evidence_run` weave (`EvidenceScope.SCAN_RECONSTRUCTION` the seed row — no page-local tracer mint). The watertight solid graduates through the geometry-minted `rasm.geometry.graduation` spine as `GeometrySubject.RECONSTRUCTED_MESH` — `graduates()` returns the local `GeometryHandoff` carrier whose `wire()` projection is the compute crossing. The registered pose from `scan/registration.md#REGISTRATION` is the precondition.

## [01]-[INDEX]

- [01]-[RECONSTRUCTION]: method-discriminated surface reconstruction over one frozen owner, the `_CONSTRUCT` constructor table, the composed `mesh/quality` `closure_fold`, the shared normal-estimation pre-step, the optional multi-object cluster split, and the GLB egress — the entry an `async` composition of the graduation `evidence_run` weave over `lane.offload`, the `@beartype(conf=FAULT_CONF)` `DensityField` finiteness fence on the in-page `_trim_poisson` density reduction, and the `@receipted(_REDACTION)` egress aspect.

## [02]-[RECONSTRUCTION]

- Owner: `ScanReconstruction` — the frozen owner discriminating by `ReconstructionMethod` row over a registered `o3d.geometry.PointCloud`, carrying a `ReconPolicy` value object (the normal-search radius/max-nn, the orientation k, the Poisson depth/scale/density-quantile, the ball-pivoting radius schedule, the alpha value, the DBSCAN eps/min-points) with derived `normal_search`/`radii` projections; `ReconReceipt` the per-reconstruction typed receipt carrying the method, the input point count, the cluster count, and the composed `QualityMetrics` closure fold, with a `ReconReceipt.of` factory reading the trimesh body once through `closure_fold`, a structurally-conforming `contribute` egress under the `@receipted(_REDACTION)` aspect, and a `graduates(evidence_key)` carrier fold; `ReconstructionMethod` the watertight-vs-detail constructor vocabulary the `_CONSTRUCT` table keys; `ConstructSpec` the one `Struct` behavior row binding a method's static open3d constructor as a `build: Callable[[o3d.geometry.PointCloud, ReconPolicy], o3d.geometry.TriangleMesh]`. The page mints NO quality value object — `mesh/quality.closure_fold` is the one closure truth, this page one of its two scan consumers (deviation's watertight gate the other).
- Cases: `ReconstructionMethod` rows `POISSON` (the `create_from_point_cloud_poisson` screened-Poisson indicator-function surface, watertight by construction, the default reconstruction) · `BALL_PIVOTING` (the `create_from_point_cloud_ball_pivoting` rolling-ball surface over the oriented samples across a radius schedule, detail-preserving, never closed) · `ALPHA_SHAPE` (the `create_from_point_cloud_alpha_shape` concave-hull surface for sparse or open scans) — each method one `_CONSTRUCT` row binding the STATIC open3d constructor that owns it, resolved by one `_CONSTRUCT[method].build(cloud, policy)` row read rather than a `match` over three near-identical constructor arms.
- Entry: `ScanReconstruction.reconstruct` is `async` — it admits a registered cloud and a method and returns `RuntimeRail[tuple[bytes, ReconReceipt]]` by composing `evidence_run(EvidenceScope.SCAN_RECONSTRUCTION, f"reconstruct.{method}", partial(self.lane.offload, _reconstruct_kernel, cloud, method, self.policy))` — the graduation weave opens the seeded span, fences the offload, and flattens the lane's rail; the cleared `Ok` threads the receipt slot through the page's `@receipted` `_emit` map while the GLB bytes ride through untouched. The module-level `_reconstruct_kernel` binds the shared `_estimate` normal pre-step once above the cluster split, splits into density clusters through `_cluster` only when `ReconPolicy.dbscan_eps > 0.0`, folds each cluster's `_CONSTRUCT[method].build` mesh into one accumulated `TriangleMesh` through `Block.of_seq(...).fold` over the immutable open3d `+` merge — never the in-place `+=` that would mutate the seed across the fold — lifts the vertices/triangles into a `trimesh.Trimesh`, and returns the bare `Trimesh.export(file_type="glb")` bytes paired with the pure `ReconReceipt.of` row.
- Auto: `PointCloud.estimate_normals(KDTreeSearchParamHybrid(radius, max_nn))` fills the per-point normals and `orient_normals_consistent_tangent_plane(k)` propagates a globally consistent orientation across the tangent-plane graph; the `POISSON` row's `build` calls `TriangleMesh.create_from_point_cloud_poisson(cloud, depth, scale)` returning the reconstructed mesh plus the per-vertex density array the low-density trim reads, the `BALL_PIVOTING` row `create_from_point_cloud_ball_pivoting(cloud, radii: DoubleVector)`, and the `ALPHA_SHAPE` row `create_from_point_cloud_alpha_shape(cloud, alpha)`; the Poisson row's `remove_vertices_by_mask(NDArray[bool])` drops the low-density balloon artifacts screened-Poisson extrudes past the sample support, the cutoff the `poisson_density_quantile` order statistic read off `numpy.sort(density)` at the `int(q * (n - 1))` fractional index (the catalogued sort owns the quantile — `numpy.quantile` is uncatalogued) under the `@beartype(conf=FAULT_CONF)` `DensityField` finiteness fence; `cluster_dbscan(eps, min_points)` labels the cloud into density clusters so a multi-object scene reconstructs each cluster separately through `select_by_index(numpy.where(labels == label)[0])`; the exported body reads its closure ONCE through the composed `mesh/quality.closure_fold` — watertight, winding, euler characteristic, boundary/non-manifold edges, components (the quality owner's backend row: `manifold3d` `decompose()` when the tier resolves, `trimesh.body_count` the fall-through), genus, volume, area, and the cell-shape distributions — so the receipt facts and the graduation residual ledger project from one fold, never a page-local re-scan.
- Receipt: `ReconReceipt.contribute` returns the one-element `tuple[Receipt, ...]` — `Receipt.of("geometry.scan.reconstruction", ("emitted", method.value, facts))` against the runtime two-argument `of(owner, evidence)` contract, the facts spread from the composed `QualityMetrics` fold as native `int`/`float`/`bool` slots; the receipt-stream egress rides the `@receipted(_REDACTION)` AOP aspect over the inner `_emit`, never an inline `Signals.emit`. `ReconReceipt.graduates(evidence_key)` returns the local `GeometryHandoff` carrier — `GeometryHandoff.of(GeometrySubject.RECONSTRUCTED_MESH, key, residuals, _CEILING)` gating the `nonwatertight` residual (`0.0` watertight else `1.0`) AND the `noncontiguous` residual (`float(components - 1)`) against the zero bars, so a non-watertight or multi-shell surface breaches the carrier's residual-over-ceiling `admitted` verdict and graduates only after the `mesh/repair.md#MESH` weld — the crossing to compute is the carrier's `wire()` data, never an import.
- Packages: `open3d` (`geometry.PointCloud.estimate_normals`/`orient_normals_consistent_tangent_plane`/`cluster_dbscan`/`select_by_index`, `geometry.TriangleMesh.create_from_point_cloud_poisson`/`create_from_point_cloud_ball_pivoting`/`create_from_point_cloud_alpha_shape`/`remove_vertices_by_mask`/`+` merge, `utility.DoubleVector`, `geometry.KDTreeSearchParamHybrid`), `trimesh` (`Trimesh(vertices, faces, process=False)` the lift, `.export(file_type="glb")` the only encode path), `numpy` (`asarray`/`sort`/`where` the density trim and cluster split), `beartype` (`@beartype(conf=FAULT_CONF)` + `vale.Is` the `DensityField` refinement), `expression` (`Block.of_seq`/`Block.fold` the cluster merge, `Map.of_seq`/`Map.empty` the table and redaction), `msgspec` (`Struct`/`gc=False` the carriers), geometry (`evidence_run`/`EvidenceScope`/`GeometryHandoff`/`GeometrySubject` the graduation spine, `mesh/quality.closure_fold`+`QualityMetrics` the one closure truth), runtime (`RuntimeRail`/`FAULT_CONF`, `LanePolicy.offload`, `ContentKey` from `rasm.runtime.identity`, `Receipt`/`Redaction`/`receipted`).
- Growth: a new reconstruction algorithm is one `ReconstructionMethod` member plus one `_CONSTRUCT` row; a new pre-step is one composition above the cluster split; a per-cluster method selection is one policy field discriminating the row read; zero new surface.
- Boundary: the registered pose is the precondition (the `register` rail from `scan/registration.md#REGISTRATION` supplies the aligned cloud, never re-derived here); raw-scan ingestion and decimation route the `scan/ingestion.md#INGESTION` sibling; watertight mesh repair and hole-fill route the `mesh/repair.md#MESH` `MeshRepairOp.Condition` arm (a non-watertight ball-pivoting or alpha-shape surface becomes a valid solid only through that seam); scan-vs-model deviation routes the `scan/deviation.md#DEVIATION` sibling; the closure algebra is `mesh/quality.closure_fold`'s, never a page-local quality twin; no IFC tessellation, no durable store, no Rhino/GH mutation. The deleted forms: a sync `reconstruct` blocking the event loop on the Poisson solve; a `lane: LanePolicy | None` accepted yet never composed; a page-local `trace.get_tracer` mint where `evidence_run` owns the weave; a page-local `MeshQuality` value object re-computing the closure the quality owner folds (the deleted twin — one closure truth, one component-count strategy); a compute-interior graduation binding or a `GraduationReceipt.graduates` call where the local `rasm.geometry.graduation` owner mints the vocabulary and `graduates()` returns the local `GeometryHandoff`; a `reconstruct_poisson`/`reconstruct_ball`/`reconstruct_alpha` method family; a runtime-flag branch inside one constructor; an inline `Signals.emit`; a vacuous graduation minting a handoff for a non-watertight or multi-shell surface; a `numpy.quantile` call where the catalogued sort owns the cutoff; a hand-rolled Poisson or alpha-complex kernel; a hand-rolled GLB serializer; and a re-implemented watertight repair where `mesh/repair` owns it.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from functools import partial
from typing import TYPE_CHECKING, Annotated, Final

import numpy as np
from beartype import beartype
from beartype.vale import Is
from expression.collections import Block, Map
from msgspec import Struct

from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometrySubject, evidence_run
from rasm.geometry.mesh.quality import QualityMetrics, closure_fold
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt, Redaction, receipted

if TYPE_CHECKING:  # type-only: every runtime open3d call does its own boundary-scope import, so the module loads clean
    import open3d as o3d

# --- [TYPES] ----------------------------------------------------------------------------


class ReconstructionMethod(StrEnum):
    POISSON = "poisson"
    BALL_PIVOTING = "ball-pivoting"
    ALPHA_SHAPE = "alpha-shape"


# the Poisson density array is solver output: a degenerate solve can emit `NaN`/`±inf` that would
# corrupt the `np.sort` ordering and the `< cutoff` mask silently, so the trim folds behind this
# fence under the shared `FAULT_CONF` — the sibling `SignedField`/`_structured` parity.
type DensityField = Annotated[np.ndarray, Is[lambda a: bool(np.isfinite(a).all())]]


# --- [CONSTANTS] ------------------------------------------------------------------------

# graduation gate per closure residual against a zero ceiling: a non-watertight OR multi-shell surface
# fails and graduates only after the mesh/repair.md#MESH weld, never on the vacuous reconstruction alone.
_CEILING: Final[dict[str, float]] = {"nonwatertight": 0.0, "noncontiguous": 0.0}
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # reconstruction facts carry no secret field


# --- [MODELS] ---------------------------------------------------------------------------


class ReconPolicy(Struct, frozen=True):
    normal_radius: float = 0.1
    normal_max_nn: int = 30
    orient_k: int = 30
    poisson_depth: int = 9
    poisson_scale: float = 1.1
    poisson_density_quantile: float = 0.02
    ball_radii: tuple[float, ...] = (0.05, 0.1, 0.2)
    alpha: float = 0.1
    dbscan_eps: float = 0.0
    dbscan_min_points: int = 16

    @property
    def normal_search(self) -> "o3d.geometry.KDTreeSearchParamHybrid":
        import open3d as o3d  # noqa: PLC0415  boundary-scope: the property runs inside the offloaded kernel, never at module load

        return o3d.geometry.KDTreeSearchParamHybrid(self.normal_radius, self.normal_max_nn)

    @property
    def radii(self) -> "o3d.utility.DoubleVector":
        import open3d as o3d  # noqa: PLC0415

        return o3d.utility.DoubleVector(self.ball_radii)


class ReconReceipt(Struct, frozen=True):
    method: ReconstructionMethod
    input_points: int
    clusters: int
    quality: QualityMetrics  # the composed mesh/quality closure fold — the ONE closure truth, never a page-local twin

    @staticmethod
    def of(method: ReconstructionMethod, *, input_points: int, body: "trimesh.Trimesh", clusters: int) -> "ReconReceipt":
        return ReconReceipt(method, int(input_points), int(clusters), closure_fold(body))

    @staticmethod
    @receipted(_REDACTION)
    def _emit(receipt: "ReconReceipt") -> "ReconReceipt":
        return receipt  # the @receipted aspect harvests `contribute` and emits on exit; egress is the decorator rail

    @property
    def residuals(self) -> dict[str, float]:
        # closure residual + the over-segmentation residual off the quality fold's backend components row:
        # a Poisson balloon that closes into two disjoint shells is watertight yet components == 2.
        return {"nonwatertight": 0.0 if self.quality.watertight else 1.0, "noncontiguous": float(self.quality.components - 1)}

    def facts(self) -> dict[str, object]:
        q = self.quality
        return {
            "method": self.method.value,
            "input_points": self.input_points,
            "clusters": self.clusters,
            "vertex_count": q.vertex_count,
            "face_count": q.face_count,
            "components": q.components,
            "watertight": q.watertight,
            "winding_consistent": q.winding_consistent,
            "genus": q.genus,
            "euler_characteristic": q.euler_characteristic,
            "volume": q.volume,
            "area": q.area,
        }

    def contribute(self) -> tuple[Receipt, ...]:
        return (Receipt.of("geometry.scan.reconstruction", ("emitted", self.method.value, self.facts())),)

    def graduates(self, evidence_key: ContentKey) -> GeometryHandoff:
        # both residual keys off the one quality fold against the zero bars; the carrier's
        # residual-over-ceiling `admitted` verdict gates, and the crossing is `wire()` data.
        return GeometryHandoff.of(GeometrySubject.RECONSTRUCTED_MESH, evidence_key, self.residuals, _CEILING)


# --- [TABLES] ---------------------------------------------------------------------------


# one row per method binding its STATIC open3d constructor; a new algorithm is one row, not a new
# dispatch arm. The Poisson row owns the density-trim because its constructor returns the density array.
class ConstructSpec(Struct, frozen=True, gc=False):
    build: Callable[["o3d.geometry.PointCloud", ReconPolicy], "o3d.geometry.TriangleMesh"]


@beartype(conf=FAULT_CONF)
def _trim_poisson(mesh: "o3d.geometry.TriangleMesh", density: DensityField, quantile: float) -> "o3d.geometry.TriangleMesh":
    # the `DensityField` `Is[isfinite]` refinement fires before the order statistic, so a non-finite
    # solver density rails through the faults `CLASSIFY` `api` row rather than corrupting the cutoff.
    if density.size == 0:  # a degenerate solve emits an empty mesh; the order-statistic index would IndexError
        return mesh
    samples = np.sort(density)  # the catalogued sort owns the quantile; `numpy.quantile` is uncatalogued
    cutoff = samples[int(quantile * (samples.size - 1))]
    mesh.remove_vertices_by_mask(density < cutoff)  # drop the low-density balloon artifacts past the sample support
    return mesh


# each builder does its own boundary-scope `import open3d as o3d` so the `_CONSTRUCT` rows never close
# over a module-level open3d global the import-policy ban leaves unbound.
def _poisson(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.TriangleMesh":
    import open3d as o3d  # noqa: PLC0415

    mesh, density = o3d.geometry.TriangleMesh.create_from_point_cloud_poisson(cloud, depth=policy.poisson_depth, scale=policy.poisson_scale)
    return _trim_poisson(mesh, np.asarray(density), policy.poisson_density_quantile)


def _ball_pivoting(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.TriangleMesh":
    import open3d as o3d  # noqa: PLC0415

    return o3d.geometry.TriangleMesh.create_from_point_cloud_ball_pivoting(cloud, policy.radii)


def _alpha_shape(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.TriangleMesh":
    import open3d as o3d  # noqa: PLC0415

    return o3d.geometry.TriangleMesh.create_from_point_cloud_alpha_shape(cloud, policy.alpha)


_CONSTRUCT: Final[Map[ReconstructionMethod, ConstructSpec]] = Map.of_seq([
    (ReconstructionMethod.POISSON, ConstructSpec(build=_poisson)),
    (ReconstructionMethod.BALL_PIVOTING, ConstructSpec(build=_ball_pivoting)),
    (ReconstructionMethod.ALPHA_SHAPE, ConstructSpec(build=_alpha_shape)),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _estimate(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.PointCloud":
    cloud.estimate_normals(policy.normal_search)
    cloud.orient_normals_consistent_tangent_plane(policy.orient_k)
    return cloud


def _cluster(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> tuple["o3d.geometry.PointCloud", ...]:
    labels = np.asarray(cloud.cluster_dbscan(policy.dbscan_eps, policy.dbscan_min_points))
    if labels.size == 0:  # an empty cloud yields empty labels; `.max()` has no identity, so return the cloud whole
        return (cloud,)
    return tuple(cloud.select_by_index(np.where(labels == label)[0]) for label in range(int(labels.max()) + 1))


def _reconstruct_kernel(cloud: "o3d.geometry.PointCloud", method: ReconstructionMethod, policy: ReconPolicy) -> tuple[bytes, ReconReceipt]:
    # the module-level picklable kernel the lane offloads; the fold accumulates over the immutable
    # open3d `+` merge, never the in-place `+=` that would mutate the seed across the fold.
    import open3d as o3d  # noqa: PLC0415
    import trimesh  # noqa: PLC0415

    oriented = _estimate(cloud, policy)
    clusters = _cluster(oriented, policy) if policy.dbscan_eps > 0.0 else (oriented,)
    build = _CONSTRUCT[method].build
    mesh = Block.of_seq(clusters).fold(lambda acc, part: acc + build(part, policy), o3d.geometry.TriangleMesh())
    body = trimesh.Trimesh(vertices=np.asarray(mesh.vertices), faces=np.asarray(mesh.triangles), process=False)
    return body.export(file_type="glb"), ReconReceipt.of(method, input_points=len(oriented.points), body=body, clusters=len(clusters))


# --- [SERVICES] -------------------------------------------------------------------------


class ScanReconstruction(Struct, frozen=True):
    lane: LanePolicy
    policy: ReconPolicy = ReconPolicy()

    async def reconstruct(self, cloud: "o3d.geometry.PointCloud", method: ReconstructionMethod) -> "RuntimeRail[tuple[bytes, ReconReceipt]]":
        # the one composed entry: the graduation weave (seeded span + fence + harvest) wraps the lane
        # offload; the cleared Ok threads the receipt slot through the page's @receipted `_emit` while
        # the GLB bytes ride through untouched — emission stays a decorator rail over the receipt slot.
        rail = await evidence_run(
            EvidenceScope.SCAN_RECONSTRUCTION, f"reconstruct.{method}", partial(self.lane.offload, _reconstruct_kernel, cloud, method, self.policy)
        )
        return rail.map(lambda pair: (pair[0], ReconReceipt._emit(pair[1])))
```
