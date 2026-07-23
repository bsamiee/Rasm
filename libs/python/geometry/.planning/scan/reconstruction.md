# [PY_GEOMETRY_SCAN_RECONSTRUCTION]

`ScanReconstruction` builds a watertight `TriangleMesh` from a registered `Cloud` — the `scan/ingestion.md#INGESTION` array carrier, never a live `open3d` handle across the worker seam — and produces the `reconstructed-mesh` `GeometrySubject`. Reconstruction is a STATIC open3d constructor choice keyed by `ReconstructionMethod`, never a runtime mode flag, holding parity with the sibling `ScanRegistration`/`ScanDeviation` owners. That surface egresses as GLB and graduates to compute; the closure algebra and watertight conditioning belong to named `mesh/` siblings, never re-implemented here.

A reconstructed body's watertight/winding/euler/volume/area/components algebra reads ONCE through `mesh/quality`'s public `closure_fold` (quality tiers below the scan producers) — `ReconReceipt` carries the folded `QualityMetrics`, and both `facts()` and the graduation residual ledger project from that one fold. That ledger gates `nonwatertight` and `noncontiguous` (the `components - 1` over-segmentation residual) against zero ceilings, so a Poisson balloon that closes into two disjoint shells fails a gate the lone watertight flag passes. `reconstruct` runs `async`, riding the `lane.offload` crossing on `Kernel.of(_reconstruct_kernel, KernelTrait.HOSTILE)` under the graduation `evidence_run` weave seeded by `EvidenceScope.SCAN_RECONSTRUCTION` — the `open3d` band imports under no isolated subinterpreter, so the kernel rides the warm process pool, the `Cloud` arrays cross the pickle seam, and the kernel re-inflates through `Cloud.legacy()` where the normal estimation begins. That watertight solid graduates through the `rasm.geometry.graduation` spine as `GeometrySubject.RECONSTRUCTED_MESH`, `graduates()` returning the `GeometryHandoff` whose `wire()` projection is the compute crossing. A registered pose from `scan/registration.md#REGISTRATION` is the precondition; watertight conditioning routes the `mesh/repair.md#MESH` `MeshRepairOp.Condition` arm.

## [01]-[INDEX]

- [01]-[RECONSTRUCTION]: method-discriminated surface reconstruction over the `_CONSTRUCT` constructor table, composing `mesh/quality`'s `closure_fold` and graduating the watertight solid to compute.

## [02]-[RECONSTRUCTION]

- Owner: `ScanReconstruction` discriminates by `ReconstructionMethod` over a registered `Cloud` carrier; `ReconPolicy` carries the per-algorithm knobs (normal search, orientation `k`, Poisson depth/scale/density-quantile, ball-pivoting radius schedule, alpha, DBSCAN eps/min-points) and `ReconReceipt` the typed receipt. This page mints NO quality value object — `mesh/quality.closure_fold` is the one closure truth, this page one of its two scan consumers, the `scan/deviation.md#DEVIATION` watertight gate the other.
- Cases: `POISSON` is watertight by construction and the default; `BALL_PIVOTING` preserves detail over the oriented samples yet never closes; `ALPHA_SHAPE` is the concave-hull surface for sparse or open scans. Each resolves as one `_CONSTRUCT[method].build(cloud, policy)` row read binding the STATIC open3d constructor, never a `match` over three near-identical arms.
- Auto: `estimate_normals` then `orient_normals_consistent_tangent_plane` condition every method once above the cluster split — Poisson and ball-pivoting both require globally consistent oriented normals. Poisson's constructor alone returns a per-vertex density array whose low-density balloon artifacts trim away at the `poisson_density_quantile` order statistic; `cluster_dbscan` (only when `dbscan_eps > 0.0`) labels the cloud so a multi-object scene reconstructs each cluster separately, and each cluster solve beats the graduation `GeometryPulse.RECONSTRUCTION` point through `pulsed` over the lane conduit's pickled tap — lossy live progress, never a second observability rail.
- Packages: `open3d` (the `PointCloud` normal/cluster ops and the three `TriangleMesh.create_from_point_cloud_*` constructors, `DoubleVector`, `KDTreeSearchParamHybrid`), `trimesh` (the `Trimesh(...)` lift and `.export(file_type="glb")`, the only GLB encode path — open3d `io` writes PLY/OBJ/STL/OFF only), `numpy` (the density trim and cluster split), `beartype` + `vale.Is` (the `DensityField` finiteness refinement), `expression` (`Block.fold` cluster merge, `Map` table and redaction), `msgspec` carriers, the geometry graduation spine (`evidence_run`/`EvidenceScope`/`GeometryHandoff`/`GeometrySubject`, `closure_fold`/`QualityMetrics`), and the runtime rails per the fence imports.
- Bench: `bench` rides the graduation `bench_seam` fold over the whole `reconstruct` crossing — normal estimation, `_CONSTRUCT` row, closure fold, weave — cloud-size-parameterized: the subject keys the method and the input point count as `rasm.geometry.scan.reconstruction.<method>.p<points>`; latency and throughput rows per row, zero instrument rows, graduation's `bench_terminal` wrapping the fold in the runtime `JobRun.bounded` envelope for a process-terminal run.
- Growth: a new reconstruction algorithm is one `ReconstructionMethod` member and one `_CONSTRUCT` row; a new pre-step is one composition above the cluster split; a per-cluster method selection is one policy field discriminating the row read.
- Boundary: raw-scan ingestion and decimation route `scan/ingestion.md#INGESTION`; watertight repair and hole-fill route the `mesh/repair.md#MESH` `MeshRepairOp.Condition` arm, the only path from a non-watertight ball-pivoting or alpha surface to a valid solid; scan-vs-model deviation routes `scan/deviation.md#DEVIATION`; the closure algebra is `mesh/quality.closure_fold`'s. No IFC tessellation, no durable store, no Rhino/GH mutation.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from functools import partial
from queue import Queue
from typing import TYPE_CHECKING, Annotated, Final

import numpy as np
from beartype import beartype
from beartype.vale import Is
from expression.collections import Block, Map
from msgspec import Struct

from rasm.geometry.graduation import EvidenceScope, GeometryHandoff, GeometryPulse, GeometrySubject, PulseBeat, bench_seam, evidence_run
from rasm.geometry.mesh.quality import QualityMetrics, closure_fold
from rasm.geometry.scan.ingestion import Cloud
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy, PulseFact, pulsed
from rasm.runtime.profiles import BenchmarkReceipt
from rasm.runtime.receipts import OPEN, Receipt, receipted
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:  # type-only: each runtime open3d/trimesh call self-imports at boundary scope, so the module loads clean
    import open3d as o3d
    import trimesh

# --- [TYPES] ----------------------------------------------------------------------------


class ReconstructionMethod(StrEnum):
    POISSON = "poisson"
    BALL_PIVOTING = "ball-pivoting"
    ALPHA_SHAPE = "alpha-shape"


# solver-output density; a degenerate solve's `NaN`/`±inf` would silently corrupt the sort/mask, so the trim gates on finiteness under `FAULT_CONF`.
type DensityField = Annotated[np.ndarray, Is[lambda a: bool(np.isfinite(a).all())]]


# --- [CONSTANTS] ------------------------------------------------------------------------

# zero ceiling per closure residual.
_CEILING: Final[dict[str, float]] = {"nonwatertight": 0.0, "noncontiguous": 0.0}


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
        import open3d as o3d  # ruff:ignore[import-outside-top-level]

        return o3d.geometry.KDTreeSearchParamHybrid(self.normal_radius, self.normal_max_nn)

    @property
    def radii(self) -> "o3d.utility.DoubleVector":
        import open3d as o3d  # ruff:ignore[import-outside-top-level]

        return o3d.utility.DoubleVector(self.ball_radii)


class ReconReceipt(Struct, frozen=True):
    method: ReconstructionMethod
    input_points: int
    clusters: int
    quality: QualityMetrics  # the composed mesh/quality closure fold

    @staticmethod
    def of(method: ReconstructionMethod, *, input_points: int, body: "trimesh.Trimesh", clusters: int) -> "ReconReceipt":
        return ReconReceipt(method, int(input_points), int(clusters), closure_fold(body))

    @staticmethod
    @receipted(OPEN)  # reconstruction facts carry no secret field, so the runtime keep-all policy binds
    def _emit(receipt: "ReconReceipt") -> "ReconReceipt":
        return receipt

    @property
    def residuals(self) -> dict[str, float]:
        # `abs(components - 1)` deviation from the one-shell target off the quality fold's component count: an empty
        # body (zero components) fails the gate exactly as a split shell does, never a negative residual passing a zero ceiling.
        return {"nonwatertight": 0.0 if self.quality.watertight else 1.0, "noncontiguous": float(abs(self.quality.components - 1))}

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
        return (Receipt.of("rasm.geometry.scan.reconstruction", ("emitted", self.method.value, self.facts())),)

    def graduates(self, evidence_key: ContentKey) -> GeometryHandoff:
        return GeometryHandoff.of(GeometrySubject.RECONSTRUCTED_MESH, evidence_key, self.residuals, _CEILING)


# --- [TABLES] ---------------------------------------------------------------------------


# one `_CONSTRUCT` row per method; the Poisson row owns the density-trim, its constructor alone
# returning the density array.
class ConstructSpec(Struct, frozen=True, gc=False):
    build: Callable[["o3d.geometry.PointCloud", ReconPolicy], "o3d.geometry.TriangleMesh"]


@beartype(conf=FAULT_CONF)
def _trim_poisson(mesh: "o3d.geometry.TriangleMesh", density: DensityField, quantile: float) -> "o3d.geometry.TriangleMesh":
    if density.size == 0:  # a degenerate solve emits an empty mesh; the order-statistic index would IndexError
        return mesh
    samples = np.sort(density)  # the catalogued sort owns the quantile; `numpy.quantile` is uncatalogued
    cutoff = samples[int(quantile * (samples.size - 1))]
    mesh.remove_vertices_by_mask(density < cutoff)  # drop the low-density balloon artifacts past the sample support
    return mesh


def _poisson(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.TriangleMesh":
    import open3d as o3d  # ruff:ignore[import-outside-top-level]

    mesh, density = o3d.geometry.TriangleMesh.create_from_point_cloud_poisson(cloud, depth=policy.poisson_depth, scale=policy.poisson_scale)
    return _trim_poisson(mesh, np.asarray(density), policy.poisson_density_quantile)


def _ball_pivoting(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.TriangleMesh":
    import open3d as o3d  # ruff:ignore[import-outside-top-level]

    return o3d.geometry.TriangleMesh.create_from_point_cloud_ball_pivoting(cloud, policy.radii)


def _alpha_shape(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.TriangleMesh":
    import open3d as o3d  # ruff:ignore[import-outside-top-level]

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


def _beat_built(
    build: Callable[["o3d.geometry.PointCloud", ReconPolicy], "o3d.geometry.TriangleMesh"],
    part: "o3d.geometry.PointCloud",
    policy: ReconPolicy,
    tap: "Queue[PulseFact | None]",
    index: int,
    total: int,
) -> "o3d.geometry.TriangleMesh":
    # per-cluster convergence beat ahead of each constructor solve — lossy by lane law, the kernel's whole
    # observability reach staying the pickled queue proxy.
    pulsed(tap, GeometryPulse.RECONSTRUCTION, PulseBeat(stage="cluster", done=index + 1, total=total))
    return build(part, policy)


def _reconstruct_kernel(
    cloud: Cloud, method: ReconstructionMethod, policy: ReconPolicy, tap: "Queue[PulseFact | None]"
) -> tuple[bytes, ReconReceipt]:
    # module-level HOSTILE kernel: the Cloud arrays cross the pickle seam, the legacy handle rebuilds here, and the
    # fold accumulates over the immutable open3d `+` merge, never the in-place `+=` that mutates the seed.
    import open3d as o3d  # ruff:ignore[import-outside-top-level]
    import trimesh  # ruff:ignore[import-outside-top-level]

    oriented = _estimate(cloud.legacy(), policy)
    clusters = _cluster(oriented, policy) if policy.dbscan_eps > 0.0 else (oriented,)
    build = _CONSTRUCT[method].build
    parts = Block.of_seq(clusters).mapi(lambda i, part: _beat_built(build, part, policy, tap, i, len(clusters)))
    mesh = parts.fold(lambda acc, part: acc + part, o3d.geometry.TriangleMesh())
    body = trimesh.Trimesh(vertices=np.asarray(mesh.vertices), faces=np.asarray(mesh.triangles), process=False)
    return body.export(file_type="glb"), ReconReceipt.of(method, input_points=len(oriented.points), body=body, clusters=len(clusters))


# --- [SERVICES] -------------------------------------------------------------------------


class ScanReconstruction(Struct, frozen=True):
    lane: LanePolicy
    policy: ReconPolicy = ReconPolicy()

    async def reconstruct(self, cloud: Cloud, method: ReconstructionMethod) -> "RuntimeRail[tuple[bytes, ReconReceipt]]":
        # graduation weave wraps the lane offload; the cleared Ok threads the receipt slot through `_emit` while
        # GLB bytes ride through untouched. HOSTILE is the declared trait — the open3d band imports under no
        # isolated subinterpreter, so the kernel rides the warm process pool.
        rail = await evidence_run(
            EvidenceScope.SCAN_RECONSTRUCTION,
            f"reconstruct.{method}",
            partial(self.lane.offload, Kernel.of(_reconstruct_kernel, KernelTrait.HOSTILE), cloud, method, self.policy, self.lane.pulses.tap),
        )
        return rail.map(lambda pair: (pair[0], ReconReceipt._emit(pair[1])))

    def bench(self, cloud: Cloud, method: ReconstructionMethod, *, rounds: int = 32, warmup: int = 4) -> BenchmarkReceipt:
        # cloud-size-parameterized macro-bench per _CONSTRUCT row: the subject keys the method and the input point
        # count; each round drives the whole reconstruct crossing — normal estimation, constructor row, closure
        # fold, weave — never an in-kernel probe (the pulse boundary).
        return bench_seam(f"{EvidenceScope.SCAN_RECONSTRUCTION.value}.{method}.p{len(cloud)}", partial(self.reconstruct, cloud, method), rounds=rounds, warmup=warmup)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
