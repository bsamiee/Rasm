# [PY_GEOMETRY_SCAN_RECONSTRUCTION]

`ScanReconstruction` builds a watertight `TriangleMesh` from a registered `Cloud` — the `scan/ingestion#INGESTION` array carrier, never a live `open3d` handle across the worker seam — and produces the `reconstructed-mesh` `GeometrySubject`. Reconstruction is a STATIC open3d constructor choice keyed by `ReconstructionMethod`, never a runtime mode flag, holding parity with the sibling `ScanRegistration`/`ScanDeviation` owners. The worker writes GLB to a parent-owned helper path; the parent seals and publishes that exact path through the injected `ArtifactTransfer` before returning its generated SHA-256 `ArtifactRef`. No raw GLB bytes, process-pipe body, retired carrier, or duplicate hash survives. The closure algebra and watertight conditioning belong to named `mesh/` siblings, never re-implemented here.

A reconstructed body's watertight/winding/euler/volume/area/components algebra reads ONCE through `mesh/quality`'s public `closure_fold` (quality tiers below the scan producers) — `Reconstruction` carries the folded `QualityMetrics`, and the graduation residual ledger projects from that one fold. That ledger gates `nonwatertight` and `noncontiguous` (the `components - 1` over-segmentation residual) against zero ceilings, so a Poisson balloon that closes into two disjoint shells fails a gate the lone watertight flag passes. `reconstruct` runs `async`, riding the `lane.offload` crossing on `Kernel.of(_reconstruct_kernel, KernelTrait.HOSTILE)` under the graduation `evidence_run` weave seeded by `EvidenceScope.SCAN_RECONSTRUCTION` — the `open3d` band imports under no isolated subinterpreter, so the kernel rides the warm process pool, the `Cloud` arrays cross the pickle seam, and the kernel re-inflates through `Cloud.legacy()` where the normal estimation begins. `Reconstruction` retains its closure metrics and source identity directly. A registered pose from `scan/registration#REGISTRATION` is the precondition; watertight conditioning routes the `mesh/repair#MESH` `MeshRepairOp.Condition` arm.

## [01]-[INDEX]

- [02]-[RECONSTRUCTION]: method-discriminated surface reconstruction over `_CONSTRUCT`, composing `mesh/quality`'s `closure_fold` and returning the watertight solid with its measured closure facts.

## [02]-[RECONSTRUCTION]

- Owner: `ScanReconstruction` discriminates by `ReconstructionMethod` over a registered `Cloud` carrier and carries the composition `ScopeKey` its weave and bench fold stamp; `ReconPolicy` carries the per-algorithm knobs (normal search, orientation `k`, Poisson depth/scale/density-quantile, ball-pivoting radius schedule, alpha, DBSCAN eps/min-points) and `Reconstruction` the typed result — method, cluster count, the closure grade, and the source key. This page mints NO quality value object — `mesh/quality.closure_fold` is the one closure truth, this page one of its two scan consumers, the `scan/deviation#DEVIATION` watertight gate the other.
- Cases: `POISSON` is watertight by construction and the default; `BALL_PIVOTING` preserves detail over the oriented samples yet never closes; `ALPHA_SHAPE` is the concave-hull surface for sparse or open scans. Each resolves as one `_CONSTRUCT[method]` row read binding the STATIC open3d constructor directly — the row IS the callable, since a one-field struct over it adds a declaration and a dereference and carries no second column — never a `match` over three near-identical arms.
- Law: `bench` rides the graduation `bench_seam` fold over the whole `reconstruct` crossing — normal estimation, `_CONSTRUCT` row, closure fold, weave — cloud-size-parameterized: the subject keys the method and the input point count as `rasm.geometry.scan.reconstruction.<method>.p<points>`; latency and throughput rows per row, zero instrument rows, graduation's `bench_terminal` wrapping the fold in the runtime `JobRun.bounded` envelope for a process-terminal run.
- Auto: `estimate_normals` then `orient_normals_consistent_tangent_plane` condition every method once above the cluster split — Poisson and ball-pivoting both require globally consistent oriented normals. Poisson's constructor alone returns a per-vertex density array whose low-density balloon artifacts trim away at the `poisson_density_quantile` order statistic; `cluster_dbscan` (only when `dbscan_eps > 0.0`) labels the cloud so a multi-object scene reconstructs each cluster separately, and each cluster solve beats the graduation `GeometryPulse.RECONSTRUCTION` point through `pulsed` over the lane conduit's pickled tap — lossy live progress, never a second observability rail.
- Packages: `open3d` (the `PointCloud` normal/cluster ops and the three `TriangleMesh.create_from_point_cloud_*` constructors, `DoubleVector`, `KDTreeSearchParamHybrid`), `trimesh` (the `Trimesh(...)` lift and path-backed GLB export), runtime `transport/artifact` (output spool, SHA-256 sealing, and confirmed Put), `numpy`, `beartype`, `expression`, `msgspec`, the geometry graduation/quality owners, and runtime rails.
- Growth: a new reconstruction algorithm is one `ReconstructionMethod` member and one `_CONSTRUCT` row binding its constructor; a new pre-step is one composition above the cluster split; a per-cluster method selection is one policy field discriminating the row read.
- Boundary: raw-scan ingestion and decimation route `scan/ingestion#INGESTION`; watertight repair and hole-fill route the `mesh/repair#MESH` `MeshRepairOp.Condition` arm, the only path from a non-watertight ball-pivoting or alpha surface to a valid solid; scan-vs-model deviation routes `scan/deviation#DEVIATION`; the closure algebra is `mesh/quality.closure_fold`'s. No IFC tessellation, no durable store, no Rhino/GH mutation.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from functools import partial
from pathlib import Path
from queue import Queue
from typing import Annotated, Final

import numpy as np
from beartype import beartype
from beartype.vale import Is
from expression import Error, Ok, Result, Some
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.transport.artifact import ArtifactTransfer, output
# Contracts are retired from this logic.
from rasm.geometry.graduation import (
    EvidenceScope,
    GeometryLeg,
    GeometryPulse,
    bench_seam,
    bench_subject,
    evidence_run,
)
from rasm.geometry.mesh.quality import QualityMetrics, closure_fold
from rasm.geometry.scan.ingestion import Cloud
from rasm.runtime.faults import FAULT_CONF, TERMINAL, FaultRow, RuntimeRail, rostered
from rasm.runtime.hooks import StageMark
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy, PulseFact, pulsed
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.profiles import Benchmark
from rasm.runtime.shapes import admitted, custody
from rasm.runtime.workers import Kernel, KernelTrait

lazy import open3d as o3d
lazy import trimesh

# --- [TYPES] ----------------------------------------------------------------------------


class ReconstructionStage(StrEnum):
    CLUSTER = "cluster"


class ReconstructionMethod(StrEnum):
    POISSON = "poisson"
    BALL_PIVOTING = "ball-pivoting"
    ALPHA_SHAPE = "alpha-shape"


type DensityField = Annotated[np.ndarray, Is[lambda a: bool(np.isfinite(a).all())]]


# --- [CONSTANTS] ------------------------------------------------------------------------

RECON_INTEGRITY: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.RECONSTRUCTION,
    point="artifact",
    arm="boundary",
    defect="artifact-refused",
    retriability=TERMINAL,
    slots=("proof",),
)
RECON_ADMISSION: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.RECONSTRUCTION,
    point="artifact.admission",
    arm="config",
    defect="artifact-refused",
    retriability=TERMINAL,
    slots=("phase",),
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([RECON_INTEGRITY, RECON_ADMISSION]))


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
        return o3d.geometry.KDTreeSearchParamHybrid(self.normal_radius, self.normal_max_nn)

    @property
    def radii(self) -> "o3d.utility.DoubleVector":
        return o3d.utility.DoubleVector(self.ball_radii)


class Reconstruction(Struct, frozen=True):
    method: ReconstructionMethod
    input_points: int
    clusters: int
    quality: QualityMetrics
    source_key: ContentKey

    @staticmethod
    def of(method: ReconstructionMethod, *, source: Cloud, body: "trimesh.Trimesh", clusters: int) -> "Reconstruction":
        return Reconstruction(method, len(source), int(clusters), closure_fold(body), source.digest)

    @property
    def residuals(self) -> dict[str, float]:
        return {"nonwatertight": 0.0 if self.quality.watertight else 1.0, "noncontiguous": float(abs(self.quality.components - 1))}

# --- [TABLES] ---------------------------------------------------------------------------


@beartype(conf=FAULT_CONF)
def _trim_poisson(mesh: "o3d.geometry.TriangleMesh", density: DensityField, quantile: float) -> "o3d.geometry.TriangleMesh":
    if density.size == 0:
        return mesh
    samples = np.sort(density)
    cutoff = samples[int(quantile * (samples.size - 1))]
    mesh.remove_vertices_by_mask(density < cutoff)
    return mesh


def _poisson(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.TriangleMesh":
    mesh, density = o3d.geometry.TriangleMesh.create_from_point_cloud_poisson(cloud, depth=policy.poisson_depth, scale=policy.poisson_scale)
    return _trim_poisson(mesh, np.asarray(density), policy.poisson_density_quantile)


def _ball_pivoting(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.TriangleMesh":
    return o3d.geometry.TriangleMesh.create_from_point_cloud_ball_pivoting(cloud, policy.radii)


def _alpha_shape(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.TriangleMesh":
    return o3d.geometry.TriangleMesh.create_from_point_cloud_alpha_shape(cloud, policy.alpha)


_CONSTRUCT: Final[Map[ReconstructionMethod, Callable[["o3d.geometry.PointCloud", ReconPolicy], "o3d.geometry.TriangleMesh"]]] = Map.of_seq([
    (ReconstructionMethod.POISSON, _poisson),
    (ReconstructionMethod.BALL_PIVOTING, _ball_pivoting),
    (ReconstructionMethod.ALPHA_SHAPE, _alpha_shape),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _estimate(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> "o3d.geometry.PointCloud":
    cloud.estimate_normals(policy.normal_search)
    cloud.orient_normals_consistent_tangent_plane(policy.orient_k)
    return cloud


def _cluster(cloud: "o3d.geometry.PointCloud", policy: ReconPolicy) -> tuple["o3d.geometry.PointCloud", ...]:
    labels = np.asarray(cloud.cluster_dbscan(policy.dbscan_eps, policy.dbscan_min_points))
    if labels.size == 0:
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
    pulsed(tap, GeometryPulse.RECONSTRUCTION, StageMark(stage=ReconstructionStage.CLUSTER.value, done=index + 1, total=Some(total)))
    return build(part, policy)


def _reconstruct_kernel(
    cloud: Cloud,
    method: ReconstructionMethod,
    policy: ReconPolicy,
    tap: "Queue[PulseFact | None]",
    target: str,
) -> Reconstruction:
    oriented = _estimate(cloud.legacy(), policy)
    clusters = _cluster(oriented, policy) if policy.dbscan_eps > 0.0 else (oriented,)
    build = _CONSTRUCT[method]
    parts = Block.of_seq(clusters).mapi(lambda i, part: _beat_built(build, part, policy, tap, i, len(clusters)))
    mesh = parts.fold(lambda acc, part: acc + part, o3d.geometry.TriangleMesh())
    body = trimesh.Trimesh(vertices=np.asarray(mesh.vertices), faces=np.asarray(mesh.triangles), process=False)
    body.export(file_obj=Path(target), file_type="glb")
    return Reconstruction.of(method, source=cloud, body=body, clusters=len(clusters))


# --- [SERVICES] -------------------------------------------------------------------------


class ScanReconstruction(Struct, frozen=True):
    lane: LanePolicy
    artifacts: ArtifactTransfer
    policy: ReconPolicy = ReconPolicy()
    composition: ScopeKey = DEFAULT_SCOPE

    @custody(RECON_INTEGRITY)
    @admitted(RECON_ADMISSION)
    async def reconstruct(self, cloud: Cloud, method: ReconstructionMethod) -> "RuntimeRail[tuple[ArtifactRef, Reconstruction]]":
        async def fold() -> "RuntimeRail[tuple[ArtifactRef, Reconstruction]]":
            async with output(suffix=".glb") as sink:
                built = await self.lane.offload(
                    Kernel.of(_reconstruct_kernel, KernelTrait.HOSTILE),
                    cloud,
                    method,
                    self.policy,
                    self.lane.pulses.tap,
                    str(sink.path),
                )
                match built:
                    case Result(tag="ok", ok=reconstruction):
                        owned = await sink.seal()
                        artifact = await self.artifacts.publish(owned)
                        return Ok((artifact, reconstruction))
                    case Result(tag="error") as refused:
                        return refused

        return await evidence_run(
            EvidenceScope.SCAN_RECONSTRUCTION,
            f"reconstruct.{method}",
            fold,
            composition=self.composition,
        )

    def bench(self, cloud: Cloud, method: ReconstructionMethod, *, rounds: int = 32, warmup: int = 4) -> "RuntimeRail[Benchmark]":
        return bench_seam(
            bench_subject(EvidenceScope.SCAN_RECONSTRUCTION, method, f"p{len(cloud)}"), partial(self.reconstruct, cloud, method), rounds=rounds, warmup=warmup
        )

```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
