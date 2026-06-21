# [PY_GEOMETRY_SCAN_RECONSTRUCTION]

Registered-cloud-to-watertight-mesh reconstruction — the producer of the `reconstructed-mesh` `GeometrySubject` the compute graduation union already declares. `ScanReconstruction` is one frozen owner discriminating by `ReconstructionMethod` row over a registered `o3d.geometry.PointCloud`, parity with the sibling `ScanRegistration`/`ScanDeviation`: the algorithm is a STATIC `TriangleMesh` constructor choice, never a runtime mode flag, so the `match` selects WHICH open3d constructor builds the surface — `create_from_point_cloud_poisson` (the watertight indicator-function default), `create_from_point_cloud_ball_pivoting` (the detail-preserving rolling-ball surface over the raw samples), `create_from_point_cloud_alpha_shape` (the concave-hull surface for sparse or open scans). The shared `estimate_normals`/`orient_normals_consistent_tangent_plane` pre-step conditions every method (Poisson and ball-pivoting both require globally consistent oriented normals), and an optional `cluster_dbscan` split reconstructs each density cluster separately for a multi-object scene. The open3d `TriangleMesh` egresses as GLB through `trimesh.Trimesh.export` (open3d's `io` writes `PLY`/`OBJ`/`STL`/`OFF` only), and watertight conditioning routes the `mesh/repair.md#MESH` `MeshOp.Repair` arm rather than re-implementing repair here.

`ReconReceipt` graduates the watertight solid through the compute `graduation/handoff.md#GRADUATION` `GraduationReceipt.graduates` admission fold against a one-key `_NONWATERTIGHT_CEILING` residual ledger — the same residual-over-ceiling rail the sibling `RegistrationResult`/`DeviationResult` cross, so the `reconstructed-mesh` `GeometrySubject` literal is imported from `rasm.compute.graduation.handoff`, never minted as a bare `str`. The registered pose from `scan/registration.md#REGISTRATION` is the precondition; the reconstructed mesh graduates as the geometry-branch mesh, distinct from the compute `analysis/spatial.md#SPATIAL` non-mesh "reconstructed boundary" same-axis handoff. This page is the missing PRODUCER of an already-declared subject and the supplier of its measured/ceiling ledger, never the author of the compute interior.

## [01]-[INDEX]

- [01]-[RECONSTRUCTION]: method-discriminated surface reconstruction under one stage-discriminated owner, the shared normal-estimation pre-step, the optional multi-object cluster split, and the GLB egress, the cross-cutting concerns folded as rails — `boundary` the one exception-to-fault aspect, `LanePolicy.offload` the CPU-offload seam, `ReceiptContributor.contribute` the telemetry sink, `GraduationReceipt.graduates` the watertight admission gate.

## [02]-[RECONSTRUCTION]

- Owner: `ScanReconstruction` — the frozen owner discriminating by `ReconstructionMethod` row over a registered `o3d.geometry.PointCloud`, carrying a `ReconPolicy` value object (the normal-search radius/max-nn, the orientation k, the Poisson depth/scale/density-quantile, the ball-pivoting radius schedule, the alpha value, the DBSCAN eps/min-points) with derived `normal_search`/`radii` projections rather than loose scalars, parity with the sibling `RegistrationPolicy`/`DeviationPolicy`; `ReconReceipt` the per-reconstruction typed receipt carrying the method, the input point count, the output vertex/triangle count, the watertight verdict read off the reconstructed mesh, and the cluster count, with a `ReconReceipt.of` factory that reads the open3d mesh and the trimesh body once into the receipt so the three method arms construct through one keyword fold, a `contribute` `ReceiptContributor` egress, and a `graduates(evidence_key)` admission rail; `ReconstructionMethod` the watertight-vs-detail constructor vocabulary the `match` selects.
- Cases: `ReconstructionMethod` rows `POISSON` (the `create_from_point_cloud_poisson` screened-Poisson indicator-function surface, watertight by construction, the default reconstruction) · `BALL_PIVOTING` (the `create_from_point_cloud_ball_pivoting` rolling-ball surface over the oriented samples across a radius schedule, detail-preserving, never closed) · `ALPHA_SHAPE` (the `create_from_point_cloud_alpha_shape` concave-hull surface for sparse or open scans) — matched by `match`/`assert_never`, each binding the STATIC open3d constructor that owns it; the method is a constructor choice per `open3d.md#105`, never a runtime branch inside one constructor. The three methods are arms of one `reconstruct` pipeline keyed by the request value, never three parallel result shapes.
- Entry: `ScanReconstruction.reconstruct` admits a registered cloud and a method, and returns a `RuntimeRail[tuple[bytes, ReconReceipt]]` through the one `boundary(f"reconstruct.{method}", ...)` exception-to-fault conversion (the rail aspect every package returns through, so the interior raises only inside the `boundary` thunk and the open3d/trimesh fault lifts to a `BoundaryFault` exactly once at that egress); the optional `lane: LanePolicy | None` field is the imported per-subinterpreter offload seam the Growth bullet hands the multi-second Poisson/ball-pivoting solve across through the one `LanePolicy.offload(kernel, *args)` call (the same seam the registration/ingestion/deviation siblings carry), `LanePolicy` the imported lane field so the seam exists and the lane never imports the kernel. The interior `_dispatch` binds the shared `_estimate` normal pre-step once above the `match` (every method consumes oriented normals), splits into density clusters through `_cluster` only when `ReconPolicy.dbscan_eps > 0.0`, folds each cluster's method-selected `_construct` mesh into one accumulated `TriangleMesh` through `functools.reduce` over the open3d `+=` join, lifts the vertices/triangles into a `trimesh.Trimesh` through `_trimesh`, and returns the `Trimesh.export(file_type="glb")` bytes paired with the `ReconReceipt.of` row.
- Auto: `PointCloud.estimate_normals(KDTreeSearchParamHybrid(radius, max_nn))` (`open3d.md` rows [03]/[17]) fills the per-point normals and `orient_normals_consistent_tangent_plane(k)` (`open3d.md` row [04]) propagates a globally consistent orientation across the tangent-plane graph (the orientation Poisson and ball-pivoting both require for a correctly-signed surface); `TriangleMesh.create_from_point_cloud_poisson(cloud, depth, scale)` (`open3d.md` row [11]) returns the reconstructed mesh plus the per-vertex density array the low-density trim reads, `create_from_point_cloud_ball_pivoting(cloud, radii: DoubleVector)` (`open3d.md` rows [12]/[18]) rolls the ball over the oriented samples across the `DoubleVector` radius schedule, and `create_from_point_cloud_alpha_shape(cloud, alpha)` (`open3d.md` row [15]) builds the alpha-complex concave hull; the Poisson `remove_vertices_by_mask(NDArray[bool])` (`open3d.md` row [16]) drops the low-density balloon artifacts screened-Poisson extrudes past the sample support, the cutoff the `poisson_density_quantile` order statistic read off `numpy.sort(density)` (`numpy.md` row [15]) at the fractional index and the mask the `numpy.asarray(density) < cutoff` breach (`numpy.md` row `asarray`); `cluster_dbscan(eps, min_points)` (`open3d.md` row [08]) labels the cloud into density clusters so a multi-object scene reconstructs each cluster separately through `select_by_index(numpy.where(labels == label)[0])` (`open3d.md` row [10], `numpy.md` row [09] `where`); the reconstructed mesh's `len(.vertices)`/`len(.triangles)` and the `trimesh.Trimesh.is_watertight` cached property (`trimesh.md` cached-property row [04]) read once into the receipt, and the open3d `TriangleMesh.vertices`/`.triangles` fold into a `trimesh.Trimesh(vertices, faces, process=False)` (`trimesh.md` type row [01]) the `.export(file_type="glb") -> bytes` (`trimesh.md` row [07], the only encode path, never a hand-rolled GLB serializer) writes — the GLB the `mesh/repair.md#MESH` `MeshOp.Repair` arm conditions to a guaranteed watertight solid.
- Receipt: `ReconReceipt.contribute` emits one `emitted`-phase `Receipt.of` row through the `ReceiptContributor` port (`observability/receipts#RECEIPT`) carrying the method, the input point count, the output vertex/triangle count, the watertight verdict, and the cluster count, the receipt facts produced once through `ReconReceipt.facts` so the receipt and the graduation ledger read the same fold; `ReconReceipt.graduates(evidence_key)` produces the geometry `GraduationReceipt` through the compute `GraduationReceipt.graduates` admission fold over `HandoffAxis(geometry=_SUBJECT)`, gating ONE residual key against the compute owner's per-key ceiling fold — the `nonwatertight` residual `0.0` when the mesh is watertight else `1.0`, against `_NONWATERTIGHT_CEILING == 0.0` — so a non-watertight ball-pivoting or alpha-shape surface is an `Error(BoundaryFault)` on the graduation rail rather than a graduated handoff (the watertight solid graduates only after the `mesh/repair.md#MESH` weld), the key riding the compute owner's residual-over-ceiling `_admit` direction unchanged, never a second admission direction minted here. The subject is typed as the compute-owned `GeometrySubject` `"reconstructed-mesh"` literal (imported from `rasm.compute.graduation.handoff`, never a bare `str`, so an unlisted literal fails at the type boundary); this owner is the CONSUMER of the already-declared subject and the supplier of its measured/ceiling ledger, the residual-over-ceiling fold itself the one admission gate the compute owner owns.
- Packages: `open3d` (`geometry.PointCloud.estimate_normals`/`orient_normals_consistent_tangent_plane`/`cluster_dbscan`/`select_by_index`/`KDTreeSearchParamHybrid`, `geometry.TriangleMesh.create_from_point_cloud_poisson`/`create_from_point_cloud_ball_pivoting`/`create_from_point_cloud_alpha_shape`/`remove_vertices_by_mask`/`.vertices`/`.triangles`, `utility.DoubleVector`), `trimesh` (`Trimesh`/`Trimesh.is_watertight`/`Trimesh.export` for the GLB egress open3d does not write), `numpy` (`asarray`/`quantile`/`where` over the open3d vertex/triangle/density/label arrays), stdlib `functools` (`reduce` over the open3d `+=` cluster join), `msgspec` (`Struct`/`gc=False` on the leaf receipt), runtime (`RuntimeRail`/`boundary`/`Receipt`/`ContentKey`/`LanePolicy`, the `ReceiptContributor` port `ReconReceipt.contribute` satisfies structurally), compute (`GeometrySubject`/`GraduationReceipt`/`HandoffAxis`). The `open3d` and `trimesh` compiled imports are function-local at boundary scope under `# noqa: PLC0415` per the manifest import policy; this page shares the `open3d` admission row under `python_version<'3.15'` with `ingestion.md`/`registration.md`/`deviation.md`.
- Growth: a new reconstruction algorithm is one `ReconstructionMethod` row plus one `_construct` arm binding its static constructor; a new normal-estimation search param or a Poisson density-trim threshold is one `ReconPolicy` field threaded through `normal_search`/`radii`; the multi-object split is the one optional `cluster_dbscan` pre-pass already on the owner, never a parallel per-object class; a stricter graduation bar is one tighter ceiling key, never a second admission direction; the open3d reconstruction kernel (the CPU-bound Poisson/ball-pivoting solve over the `<'3.15'` companion band) hands across the optional `lane: LanePolicy | None` field through the runtime `execution/lanes#LANES` `LanePolicy.offload` per-subinterpreter variant (`anyio.to_interpreter.run_sync` under one `CapacityLimiter`, the no-pickle PEP-734 hop, degrading to `anyio.to_thread.run_sync` only where a cp315 build ships no runnable `concurrent.interpreters`, NEVER a `to_process` pickle round-trip the lanes owner rejects as the process-pool serialization tax) as ONE `offload(kernel, *args)` hand-off call over the already-landed lane — the lane never imports the kernel; zero new surface, no parallel per-method class family.
- Boundary: the registered pose is the precondition (the `register` rail from `scan/registration.md#REGISTRATION` supplies the aligned cloud, never re-derived here); raw-scan ingestion and decimation route the `scan/ingestion.md#INGESTION` sibling, never re-owned here; watertight mesh repair and hole-fill route the `mesh/repair.md#MESH` `MeshOp.Repair` arm, never re-implemented here (a non-watertight ball-pivoting or alpha-shape surface becomes a valid solid only through that seam); scan-vs-model deviation routes the `scan/deviation.md#DEVIATION` sibling; no IFC tessellation (that is `tessellation`); no durable store; no Rhino/GH mutation. The compute `analysis/spatial.md#SPATIAL` "reconstructed boundary" is a DIFFERENT, non-mesh handoff on the same geometry axis — this owner's `reconstructed-mesh` is the geometry-branch mesh, the spatial subject crosses the wire as a non-mesh boundary aligned to the scan companion, so the two never collide on the `geometry` axis. A `reconstruct_poisson`/`reconstruct_ball`/`reconstruct_alpha` method family, a runtime-flag branch inside one constructor where the algorithm is a static-constructor choice, a stat-by-stat receipt build instead of one `ReconReceipt.of` fold, a vacuous graduation that mints a `reconstructed-mesh` handoff for a non-watertight surface, a hand-rolled Poisson or alpha-complex kernel where open3d is admitted, a hand-rolled GLB serializer where `trimesh.export` writes it, and a re-implemented watertight repair where `mesh-utility` owns it are the deleted forms.

```python signature
import functools
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


class ReconstructionMethod(StrEnum):
    POISSON = "poisson"                # screened-Poisson indicator surface, watertight by construction
    BALL_PIVOTING = "ball-pivoting"    # rolling-ball surface over oriented samples, detail-preserving, open
    ALPHA_SHAPE = "alpha-shape"        # alpha-complex concave hull for sparse or open scans


# --- [CONSTANTS] ------------------------------------------------------------------------

_SUBJECT: GeometrySubject = "reconstructed-mesh"
# the graduation gate admits only a watertight solid; a non-watertight surface graduates after the
# mesh/repair.md#MESH weld, so the residual is the binary nonwatertight flag against a zero ceiling.
_NONWATERTIGHT_CEILING: float = 0.0


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


class ReconReceipt(Struct, frozen=True, gc=False):
    method: ReconstructionMethod
    input_points: int
    vertex_count: int
    triangle_count: int
    watertight: bool
    clusters: int

    @staticmethod
    def of(method: ReconstructionMethod, *, input_points: int, mesh: "o3d.geometry.TriangleMesh", body: "trimesh.Trimesh", clusters: int) -> "ReconReceipt":
        return ReconReceipt(
            method, int(input_points), len(mesh.vertices), len(mesh.triangles), bool(body.is_watertight), int(clusters)
        )

    def facts(self) -> dict[str, object]:
        return {
            "method": self.method.value, "input_points": self.input_points,
            "vertex_count": self.vertex_count, "triangle_count": self.triangle_count,
            "watertight": self.watertight, "clusters": self.clusters,
        }

    def contribute(self) -> tuple[Receipt, ...]:
        return (Receipt.of("geometry.scan.reconstruction", ("emitted", self.method.value, self.facts())),)

    def graduates(self, evidence_key: ContentKey) -> RuntimeRail[GraduationReceipt]:
        # binary watertight residual against a zero ceiling: a non-watertight surface fails the gate and
        # only graduates after the mesh/repair.md#MESH weld, never on the vacuous reconstruction alone.
        return GraduationReceipt.graduates(
            "geometry.scan.reconstruction", HandoffAxis(geometry=_SUBJECT), evidence_key,
            {"nonwatertight": 0.0 if self.watertight else 1.0}, {"nonwatertight": _NONWATERTIGHT_CEILING},
        )


# --- [SERVICES] -------------------------------------------------------------------------


class ScanReconstruction(Struct, frozen=True):
    policy: ReconPolicy = ReconPolicy()
    lane: LanePolicy | None = None

    def reconstruct(self, cloud: "o3d.geometry.PointCloud", method: ReconstructionMethod) -> "RuntimeRail[tuple[bytes, ReconReceipt]]":
        return boundary(f"reconstruct.{method}", lambda: self._dispatch(cloud, method))

    def _dispatch(self, cloud: "o3d.geometry.PointCloud", method: ReconstructionMethod) -> tuple[bytes, ReconReceipt]:
        oriented = self._estimate(cloud)
        clusters = self._cluster(oriented) if self.policy.dbscan_eps > 0.0 else (oriented,)
        mesh = functools.reduce(lambda acc, part: acc + self._construct(part, method), clusters, o3d.geometry.TriangleMesh())
        body = self._trimesh(mesh)
        receipt = ReconReceipt.of(method, input_points=len(oriented.points), mesh=mesh, body=body, clusters=len(clusters))
        return body.export(file_type="glb"), receipt

    def _estimate(self, cloud: "o3d.geometry.PointCloud") -> "o3d.geometry.PointCloud":
        cloud.estimate_normals(self.policy.normal_search)
        cloud.orient_normals_consistent_tangent_plane(self.policy.orient_k)
        return cloud

    def _cluster(self, cloud: "o3d.geometry.PointCloud") -> tuple["o3d.geometry.PointCloud", ...]:
        labels = np.asarray(cloud.cluster_dbscan(self.policy.dbscan_eps, self.policy.dbscan_min_points))
        return tuple(cloud.select_by_index(np.where(labels == label)[0]) for label in range(int(labels.max()) + 1))

    def _construct(self, cloud: "o3d.geometry.PointCloud", method: ReconstructionMethod) -> "o3d.geometry.TriangleMesh":
        match method:
            case ReconstructionMethod.POISSON:
                mesh, density = o3d.geometry.TriangleMesh.create_from_point_cloud_poisson(
                    cloud, depth=self.policy.poisson_depth, scale=self.policy.poisson_scale
                )
                samples = np.sort(np.asarray(density))
                cutoff = samples[int(self.policy.poisson_density_quantile * (samples.size - 1))]
                mesh.remove_vertices_by_mask(np.asarray(density) < cutoff)
                return mesh
            case ReconstructionMethod.BALL_PIVOTING:
                return o3d.geometry.TriangleMesh.create_from_point_cloud_ball_pivoting(cloud, self.policy.radii)
            case ReconstructionMethod.ALPHA_SHAPE:
                return o3d.geometry.TriangleMesh.create_from_point_cloud_alpha_shape(cloud, self.policy.alpha)
            case unreachable:
                assert_never(unreachable)

    def _trimesh(self, mesh: "o3d.geometry.TriangleMesh") -> "trimesh.Trimesh":
        import trimesh  # noqa: PLC0415

        return trimesh.Trimesh(vertices=np.asarray(mesh.vertices), faces=np.asarray(mesh.triangles), process=False)
```

## [03]-[RESEARCH]

- [ALPHA_SHAPE_ARITY]: the `TriangleMesh.create_from_point_cloud_alpha_shape(cloud, alpha)` constructor is folder-`.api`-confirmed (`open3d.md` row [15]) alongside the `poisson` `(cloud, depth, scale)` return tuple (mesh plus per-vertex density `DoubleVector`, `open3d.md` row [11]) and the `ball_pivoting` `(cloud, radii: DoubleVector)` rows ([12]/[18]); the arm passes the single `alpha` positional and never the optional `tetra_mesh`/`pt_map` reuse overload, so the only live-run residual is whether a pre-built `tetra_mesh` should be threaded for a multi-cluster split to skip the per-cluster Delaunay rebuild, an owner-local performance choice, not an existence question. The Poisson depth/scale schedule and the ball-pivoting radius schedule are owner-local `ReconPolicy` parameters threaded through `normal_search`/`radii`, not catalogue dependencies.
- [DENSITY_TRIM]: the Poisson low-density-vertex trim folds the per-vertex density `DoubleVector` the `create_from_point_cloud_poisson` second return carries against the `poisson_density_quantile` cutoff through `remove_vertices_by_mask(NDArray[bool])` (`open3d.md` row [16]), removing the balloon artifacts screened-Poisson extrudes past the sample support. The cutoff is the order statistic read off `numpy.sort(density)` (`numpy.md` row [15]) at the `int(q * (n - 1))` fractional index — the catalogued sort owns the quantile rather than an uncatalogued `numpy.quantile` member — and the mask the `numpy.asarray(density) < cutoff` breach (`numpy.md` row [10] `asarray`); the quantile threshold is an owner-local heuristic the live run tunes against the scan density.
- [WATERTIGHT_GATE]: the `Trimesh.is_watertight` cached property (`trimesh.md` cached-property row [04], the content-hash-keyed manifold-closure flag) is the single gate the `ReconReceipt.graduates` residual reads — `0.0` when closed, `1.0` otherwise, against the `_NONWATERTIGHT_CEILING == 0.0` zero bar through the compute `GraduationReceipt.graduates` residual-over-ceiling fold. The open3d mesh carries its own `is_watertight`, but the receipt reads the trimesh body's flag because that is the exported GLB the downstream `mesh/repair.md#MESH` seam and the C# owner consume; the gate is a CONSUMER read of the compute admission rail, not a local admission body.

## [04]-[UPSTREAM]

- [OPEN3D_ADMISSION]: `open3d` is manifest-absent (zero match), a SHARED admission delta with the `scan/ingestion.md#INGESTION` egress and the two authored consumers `scan/registration.md#REGISTRATION`/`scan/deviation.md#DEVIATION` — one `open3d` row under `python_version<'3.15'` (the companion band) covers all four pages. `open3d` is imported at boundary scope only (the manifest import policy bans module-level import of the companion-band package), and the reconstruction kernel resolves on the `<'3.15'` companion interpreter the geometry scan rail hosts; `trimesh` itself is cp315-clean pure-Python (`trimesh.md` floor) imported function-local inside `_trimesh` for one-codebase import-policy parity with its `open3d` sibling. The orchestrator carries the single shared row; this page records the delta, never edits the central manifest.
- [GRADUATION_SUBJECT]: the `reconstructed-mesh` `GeometrySubject` this owner graduates is already present in the compute `graduation/handoff.md#GRADUATION` `GeometrySubject` union (joining `registration-transform`, `scan-deviation`, `topology-graph`, `network-graph`, `form-finding`, `numerical-primitive`, `mesh-algebra`), so `_SUBJECT` imports the literal from `rasm.compute.graduation.handoff` rather than minting a bare `str` — an unlisted literal fails at the `GeometrySubject` type boundary, the compute owner owning the union. `ReconReceipt.graduates` routes a ONE-key measured ledger (the binary `nonwatertight` residual) through the one compute `GraduationReceipt.graduates` admission fold against the `_NONWATERTIGHT_CEILING` zero bar — the per-key residual-over-ceiling gate the compute owner owns, not a local admission body — so a non-watertight surface is an `Error(BoundaryFault)` rather than a graduated handoff, the key riding the compute owner's existing upper-bound `_admit` direction with no second admission direction minted here. The compute `analysis/spatial.md#SPATIAL` "reconstructed boundary graduates outward ... never crossing as a geometry-branch mesh" subject is a DIFFERENT, non-mesh handoff on the same `geometry` axis — the spatial owner's boundary crosses the wire aligned to the scan companion as a non-mesh artifact, while this owner's reconstructed mesh crosses as the watertight GLB solid. The two are non-colliding subjects on the geometry axis; the one-pass synthesis reconciliation keeps them distinct, never folding the spatial boundary into the geometry-branch mesh surface. This page is the CONSUMER of the already-declared subject and the compute admission rail, supplying only its measured/ceiling ledger, never authoring the compute interior.
- [OFFLOAD_LANE]: the open3d reconstruction kernel is one of the heavy geometry CPU kernels the runtime `execution/lanes#LANES` `LanePolicy.offload` per-subinterpreter variant absorbs through `anyio.to_interpreter.run_sync` under one `CapacityLimiter` — the no-pickle PEP-734 hop (`lanes.md` ENTRYPOINTS `LanePolicy.offload`), degrading to `anyio.to_thread.run_sync` only where a cp315 build ships no runnable `concurrent.interpreters`, NEVER a `to_process` pickle round-trip the lanes owner explicitly rejects as the process-pool serialization tax. The optional `lane: LanePolicy | None` field on `ScanReconstruction` is the imported seam, the hand-off a uniform one `offload(kernel, *args)` growth on the Growth bullet shared with `ingestion.md#INGESTION`, `registration.md#REGISTRATION`, `deviation.md#DEVIATION`, `daemon.md#DAEMON`, and `repair.md#MESH`, sequence-after the runtime lane, never a second concurrency surface minted in geometry.
