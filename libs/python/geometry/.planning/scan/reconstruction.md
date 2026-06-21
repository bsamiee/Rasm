# [PY_GEOMETRY_SCAN_RECONSTRUCTION]

Registered-cloud-to-watertight-mesh reconstruction — the producer of the `reconstructed-mesh` graduation subject the compute union already declares. `ScanReconstruction` is one frozen owner discriminating by `ReconstructionMethod` row over a registered `o3d.geometry.PointCloud`: the algorithm is a STATIC `TriangleMesh` constructor choice, never a runtime mode flag, so the `match` selects WHICH open3d constructor builds the surface — `create_from_point_cloud_poisson` (the watertight indicator-function default), `create_from_point_cloud_ball_pivoting` (the detail-preserving rolling-ball surface over the raw samples), and `create_from_point_cloud_alpha_shape` (the concave-hull surface for sparse or open scans). The shared `estimate_normals` plus `orient_normals_consistent_tangent_plane` pre-step conditions every method (Poisson and ball-pivoting both require globally consistent oriented normals), and an optional `cluster_dbscan` split reconstructs each density cluster separately for a multi-object scene. The open3d `TriangleMesh` egresses as GLB through `trimesh` (open3d carries no GLB writer), and watertight conditioning routes the `mesh/repair.md#MESH` `MeshOp.Repair` arm rather than re-implementing repair here. The registered pose from `scan/registration.md#REGISTRATION` is the precondition; the reconstructed mesh graduates via the compute `HandoffAxis` geometry `reconstructed-mesh` subject — the geometry-branch mesh, distinct from the compute `analysis/spatial.md#SPATIAL` "reconstructed boundary, never crossing as a geometry-branch mesh" same-axis handoff.

## [01]-[INDEX]

- [01]-[RECONSTRUCTION]: method-discriminated surface reconstruction, the shared normal-estimation pre-step, the optional multi-object cluster split, and the GLB egress routing `mesh-utility` repair.

## [02]-[RECONSTRUCTION]

- Owner: `ScanReconstruction` — the frozen owner discriminating by `ReconstructionMethod` row over a registered `o3d.geometry.PointCloud`; `ReconPolicy` the frozen reconstruction parameter carrier (Poisson depth/scale, ball-pivoting radius schedule, alpha value, DBSCAN eps/min-points); `ReconReceipt` the typed receipt carrying the method, the input point count, the output vertex/triangle count, and the watertight verdict read off the reconstructed mesh.
- Cases: `ReconstructionMethod` rows `POISSON` (the `create_from_point_cloud_poisson` screened-Poisson indicator-function surface, watertight by construction, the default reconstruction) · `BALL_PIVOTING` (the `create_from_point_cloud_ball_pivoting` rolling-ball surface over the oriented samples across a radius schedule, detail-preserving, never closed) · `ALPHA_SHAPE` (the `create_from_point_cloud_alpha_shape` concave-hull surface for sparse or open scans) — matched by `match`/`assert_never`, each binding the STATIC open3d constructor that owns it; the method is a constructor choice per `open3d.md#105`, never a runtime branch inside one constructor.
- Entry: `ScanReconstruction.reconstruct` admits a registered cloud and a method, runs the pre-step and the method's constructor through one `match`, exports the result to GLB, and returns a `RuntimeRail[tuple[bytes, ReconReceipt]]`; the private `_estimate` binds the shared `estimate_normals`/`orient_normals_consistent_tangent_plane` pre-step once above the `match` (every method consumes oriented normals), the private `_construct` threads the method-selected constructor, and the private `_glb` folds the open3d `TriangleMesh` vertices/triangles into a `trimesh.Trimesh` and writes GLB bytes (open3d's `io` writes `PLY`/`OBJ`/`STL`/`OFF` only, so the GLB egress crosses through `trimesh.export`).
- Auto: `PointCloud.estimate_normals(KDTreeSearchParamHybrid)` fills the per-point normals and `orient_normals_consistent_tangent_plane(k)` propagates a globally consistent orientation across the tangent-plane graph (the orientation Poisson and ball-pivoting both require for a correctly-signed surface); `TriangleMesh.create_from_point_cloud_poisson(cloud, depth, scale)` returns the reconstructed mesh plus the per-vertex density array the low-density-vertex trim reads, `create_from_point_cloud_ball_pivoting(cloud, radii)` rolls the ball over the oriented samples across the `DoubleVector` radius schedule, and `create_from_point_cloud_alpha_shape(cloud, alpha)` builds the alpha-complex concave hull; `cluster_dbscan(eps, min_points)` labels the cloud into density clusters so a multi-object scene reconstructs each cluster separately through `select_by_index`; the reconstructed mesh's vertex/triangle counts and `is_watertight` read once into the receipt, and the open3d `TriangleMesh.vertices`/`.triangles` fold into a `trimesh.Trimesh(vertices, faces)` the `.export(file_type="glb")` writes — the GLB the `mesh/repair.md#MESH` `MeshOp.Repair` arm conditions to a guaranteed watertight solid.
- Receipt: each reconstruction contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` carrying the method, the input point count, the output vertex/triangle count, the watertight verdict, and elapsed; the reconstructed mesh produces a geometry `GraduationReceipt` subject (`reconstructed-mesh`) so the watertight solid graduates through the one geometry rail — the literal already present in the compute `graduation/handoff.md#GRADUATION` `GeometrySubject` union, so this page is the missing PRODUCER of an already-declared subject, never a new literal.
- Packages: `open3d` (`geometry.PointCloud.estimate_normals`/`orient_normals_consistent_tangent_plane`/`cluster_dbscan`/`select_by_index`/`KDTreeSearchParamHybrid`, `geometry.TriangleMesh.create_from_point_cloud_poisson`/`create_from_point_cloud_ball_pivoting`/`create_from_point_cloud_alpha_shape`/`.vertices`/`.triangles`/`.is_watertight`, `utility.DoubleVector`), `trimesh` (`Trimesh`/`Trimesh.export` for the GLB egress open3d does not write), `numpy` (`asarray` over the open3d vertex/triangle/density arrays), runtime (`RuntimeRail`/`ReceiptContributor`/`LanePolicy`).
- Growth: a new reconstruction algorithm is one `ReconstructionMethod` row plus one `_construct` arm binding its static constructor; a new normal-estimation search param or a Poisson density trim threshold is one `ReconPolicy` field; the multi-object split is the one optional `cluster_dbscan` pre-pass already on the owner, never a parallel per-object class; the open3d reconstruction kernel (the CPU-bound Poisson/ball-pivoting solve over the `<'3.15'` companion band) hands across the runtime `execution/lanes#LANES` `LanePolicy.offload` per-subinterpreter variant (`anyio.to_interpreter.run_sync` under one `CapacityLimiter`, the no-pickle PEP-734 hop, degrading to `anyio.to_thread.run_sync` only where a cp315 build ships no runnable `concurrent.interpreters`, NEVER a `to_process` pickle round-trip the lanes owner rejects as the process-pool serialization tax) as ONE `offload(kernel, *args)` hand-off call over the already-landed lane — `LanePolicy` is imported from runtime so the seam exists, the lane never imports the kernel; zero new surface, no parallel per-method class family.
- Boundary: the registered pose is the precondition (the `register` rail from `scan/registration.md#REGISTRATION` supplies the aligned cloud, never re-derived here); raw-scan ingestion and decimation route the `scan/ingestion.md` sibling, never re-owned here; watertight mesh repair and hole-fill route the `mesh/repair.md#MESH` `MeshOp.Repair` arm, never re-implemented here (a non-watertight ball-pivoting or alpha-shape surface becomes a valid solid only through that seam); scan-vs-model deviation routes the `scan/deviation.md#DEVIATION` sibling; no IFC tessellation (that is `tessellation`); no durable store; no Rhino/GH mutation. The compute `analysis/spatial.md#SPATIAL` "reconstructed boundary" is a DIFFERENT, non-mesh handoff on the same geometry axis — this owner's `reconstructed-mesh` is the geometry-branch mesh, the spatial subject crosses the wire as a non-mesh boundary aligned to the scan companion, so the two never collide on the `geometry` axis. A `reconstruct_poisson`/`reconstruct_ball`/`reconstruct_alpha` method family, a runtime-flag branch inside one constructor where the algorithm is a static-constructor choice, a hand-rolled Poisson or alpha-complex kernel where open3d is admitted, a hand-rolled GLB serializer where `trimesh.export` writes it, and a re-implemented watertight repair where `mesh-utility` owns it are the deleted forms.

```python signature
import open3d as o3d
import trimesh
import numpy as np
from enum import StrEnum
from typing import assert_never
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import ReceiptContributor


class ReconstructionMethod(StrEnum):
    POISSON = "poisson"
    BALL_PIVOTING = "ball-pivoting"
    ALPHA_SHAPE = "alpha-shape"


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


class ReconReceipt(Struct, frozen=True):
    method: ReconstructionMethod
    input_points: int
    vertex_count: int
    triangle_count: int
    watertight: bool


class ScanReconstruction(Struct, frozen=True):
    policy: ReconPolicy = ReconPolicy()

    def reconstruct(self, cloud: o3d.geometry.PointCloud, method: ReconstructionMethod) -> "RuntimeRail[tuple[bytes, ReconReceipt]]":
        return boundary(f"reconstruct.{method}", lambda: self._dispatch(cloud, method))

    def _dispatch(self, cloud: o3d.geometry.PointCloud, method: ReconstructionMethod) -> tuple[bytes, ReconReceipt]:
        oriented = self._estimate(cloud)
        clusters = self._cluster(oriented) if self.policy.dbscan_eps > 0.0 else (oriented,)
        mesh = o3d.geometry.TriangleMesh()
        for part in clusters:
            mesh += self._construct(part, method)
        tri = self._trimesh(mesh)
        receipt = ReconReceipt(method, len(oriented.points), len(mesh.vertices), len(mesh.triangles), bool(tri.is_watertight))
        return tri.export(file_type="glb"), receipt

    def _estimate(self, cloud: o3d.geometry.PointCloud) -> o3d.geometry.PointCloud:
        cloud.estimate_normals(o3d.geometry.KDTreeSearchParamHybrid(self.policy.normal_radius, self.policy.normal_max_nn))
        cloud.orient_normals_consistent_tangent_plane(self.policy.orient_k)
        return cloud

    def _cluster(self, cloud: o3d.geometry.PointCloud) -> tuple[o3d.geometry.PointCloud, ...]:
        labels = np.asarray(cloud.cluster_dbscan(self.policy.dbscan_eps, self.policy.dbscan_min_points))
        return tuple(cloud.select_by_index(np.where(labels == label)[0].tolist()) for label in range(int(labels.max()) + 1))

    def _construct(self, cloud: o3d.geometry.PointCloud, method: ReconstructionMethod) -> o3d.geometry.TriangleMesh:
        match method:
            case ReconstructionMethod.POISSON:
                mesh, density = o3d.geometry.TriangleMesh.create_from_point_cloud_poisson(
                    cloud, depth=self.policy.poisson_depth, scale=self.policy.poisson_scale
                )
                cutoff = np.quantile(np.asarray(density), self.policy.poisson_density_quantile)
                mesh.remove_vertices_by_mask(np.asarray(density) < cutoff)
                return mesh
            case ReconstructionMethod.BALL_PIVOTING:
                return o3d.geometry.TriangleMesh.create_from_point_cloud_ball_pivoting(cloud, o3d.utility.DoubleVector(self.policy.ball_radii))
            case ReconstructionMethod.ALPHA_SHAPE:
                return o3d.geometry.TriangleMesh.create_from_point_cloud_alpha_shape(cloud, self.policy.alpha)
            case unreachable:
                assert_never(unreachable)

    def _trimesh(self, mesh: o3d.geometry.TriangleMesh) -> "trimesh.Trimesh":
        return trimesh.Trimesh(vertices=np.asarray(mesh.vertices), faces=np.asarray(mesh.triangles), process=False)
```

## [03]-[RESEARCH]

- [ALPHA_SHAPE_ARITY]: the `TriangleMesh.create_from_point_cloud_alpha_shape(cloud, alpha)` constructor is a folder-`.api`-confirmed reconstruction member row (`open3d.md` row [15]) alongside the `poisson` `(cloud, depth, scale)` return tuple (mesh plus per-vertex density `DoubleVector`) and the `ball_pivoting` `(cloud, radii: DoubleVector)` rows; the arm passes the single `alpha` positional and never the optional `tetra_mesh`/`pt_map` reuse overload, so the only live-run residual is whether a pre-built `tetra_mesh` should be threaded for a multi-cluster split to skip the per-cluster Delaunay rebuild, an owner-local performance choice, not an existence question. The Poisson depth/scale schedule and the ball-pivoting radius schedule are owner-local `ReconPolicy` parameters the live run tunes, not catalogue dependencies.
- [DENSITY_TRIM]: the Poisson low-density-vertex trim folds the per-vertex density `DoubleVector` the `create_from_point_cloud_poisson` second return carries against the `poisson_density_quantile` cutoff through `remove_vertices_by_mask`, removing the balloon artifacts screened-Poisson extrudes past the sample support. The quantile threshold is an owner-local heuristic the live run tunes against the scan density; the `remove_vertices_by_mask(NDArray[bool])` arity is folder-`.api`-confirmed (`open3d.md` row [16]).

## [04]-[UPSTREAM]

- [OPEN3D_ADMISSION]: `open3d` is manifest-absent (zero match), a SHARED admission delta with the `scan/ingestion.md` egress and the two authored consumers `scan/registration.md`/`scan/deviation.md` — one `open3d` row under `python_version < '3.15'` (the companion band) covers all four pages. `open3d` is imported at boundary scope only (the manifest import policy bans module-level import of the companion-band package), and the reconstruction kernel resolves on the `<'3.15'` companion interpreter the geometry scan rail hosts. The orchestrator carries the single shared row; this page records the delta, never edits the central manifest.
- [COMPUTE_SPATIAL_DISAMBIGUATION]: the `reconstructed-mesh` `GeometrySubject` this owner graduates is the geometry-branch mesh, present in the compute `graduation/handoff.md#GRADUATION` `GeometrySubject` union (no new literal). The compute `analysis/spatial.md#SPATIAL` "reconstructed boundary graduates outward ... never crossing as a geometry-branch mesh" subject is a DIFFERENT, non-mesh handoff on the same `geometry` axis — the spatial owner's boundary crosses the wire aligned to the scan companion as a non-mesh artifact, while this owner's reconstructed mesh crosses as the watertight GLB solid. The two are non-colliding subjects on the geometry axis; the one-pass synthesis reconciliation keeps them distinct, never folding the spatial boundary into the geometry-branch mesh surface.
- [OFFLOAD_LANE]: the open3d reconstruction kernel is one of the heavy geometry CPU kernels the `TASKLOG.md` `[GEOMETRY_CPU_OFFLOAD]` task hands to the runtime `execution/lanes#LANES` `LanePolicy.offload` per-subinterpreter variant. The geometry-side realization is a uniform `offload(kernel, *args)` one-call growth on the `_construct` kernel (the multi-second Poisson/ball-pivoting solve passed to the lane over the no-pickle `anyio.to_interpreter.run_sync` hop, degrading to `anyio.to_thread.run_sync` only where a cp315 build ships no runnable `concurrent.interpreters`, never a `to_process` pickle round-trip the lanes owner rejects), never a new owner and never a second concurrency surface in geometry.
