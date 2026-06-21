# [PY_GEOMETRY_MESH_SPATIAL]

Spatial query over an in-memory triangulation — the proximity, ray, containment, bounds, clearance, and sampling primitive the deviation, clash, and reconstruction hops compose against a built mesh. `SpatialQuery` is one tagged union discriminating by query kind; `query` is one polymorphic `async` entrypoint accepting one query or a batch sequence, amortizing the cached `kdtree` / `triangles_tree` / `ray` indices once per surface and folding every kind through one capability-aware `_route` — a core-resident kind on the synchronous `boundary`, a native-backend kind whose compiled wheel is absent on the local interpreter offloaded onto the `LanePolicy.offload` PEP 734 subinterpreter hop the companion carries — and `SpatialResult` is one union carrier whose case mirrors the query case so a proximity result, a ray result, a containment mask, a bounds-candidate set, a clearance scalar, and a sample set are arms of one result owner rather than six parallel result types. The pure-Python spine is `trimesh` plus `numpy`/`scipy` on the cp315 core: the module-level `proximity.closest_point`/`signed_distance` field samplers, the `mesh.contains` solid test, the `mesh.ray.intersects_location` batch ray intersector (the pure-Python `ray.ray_triangle.RayMeshIntersector`, transparently superseded by the `ray.ray_pyembree` Embree mirror when the native `embreex` wheel resolves), `sample.sample_surface`/`sample_surface_even`, and the `scipy` `kdtree` — never a single flat call. Three arms ride a NATIVE COMPILED backend `trimesh` binds but the manifest does not declare and no cp315 wheel ships, so they resolve only on the `python_version<'3.15'` companion: `Bounds` over the `rtree`-backed `triangles_tree` R-tree, the conservative `Clearance` over the `python-fcl` `collision.CollisionManager.min_distance_single` separation, and the exact `Clearance` over the `manifold3d.Manifold.min_gap` watertight gap that SUPERSEDES the FCL separation when its wheel is the richer one. `SpatialBackend` is the per-interpreter native-capability resolver — `CORE` (every pure-Python arm plus the conservative FCL clearance and the rtree bounds when those wheels happen to resolve in-process), `MANIFOLD3D` (the exact-gap clearance superseding FCL) — computed once through `SpatialBackend.resolve` off real `find_spec` probes of `rtree`/`fcl`/`manifold3d`, never a second query owner and never an asserted-always cp315 floor. The parallel vertex-KNN tiers `open3d.geometry.KDTreeFlann` and `small_gicp.KdTree.batch_knn_search` are NOT this owner's backend: a vertex nearest-neighbor is a coarser, distinct result from `closest_point`'s exact on-surface projection, so that acceleration belongs to the `scan/deviation`+`scan/registration` consumers that own the cloud-to-vertex correspondence, never minted here as a dead spatial row. This owner is a pure kernel implementing `ReceiptContributor`: it indexes an in-memory `trimesh.Trimesh`, returns numpy arrays across the wire, and contributes one typed `SpatialReceipt` row per query — mesh-file decode/encode is the data `MeshPayload` owner (`rasm.data.spatial.mesh`), and the geometry shed never opens or writes a mesh file.

## [01]-[INDEX]

- [01]-[SPATIAL]: the proximity, ray, contains, bounds, clearance, and sample queries under one tagged union over the `trimesh`/`numpy`/`scipy` spine, woven from stacked `.api` members per arm, dispatched through one capability-aware `async` `_route` that runs every pure-Python core kind on the synchronous `boundary` and offloads the native-backend kinds whose compiled wheel is absent in-process (`rtree` bounds, `python-fcl` clearance, the exact `manifold3d.min_gap` clearance) onto the `LanePolicy.offload` PEP 734 subinterpreter the companion carries, resolved per interpreter through `SpatialBackend` off real `find_spec` probes, returning one unified `SpatialResult` union and contributing one typed `SpatialReceipt` stream.

## [02]-[SPATIAL]

- Owner: `MeshSpatial` — the boundary capsule binding the cached `kdtree`/`triangles_tree`/`ray` indices to a surface, dispatching every query kind through one capability-aware `_route`, and implementing `ReceiptContributor` so the last query's facts contribute one `SpatialReceipt` row; `SpatialQuery` the tagged union discriminating by query kind so proximity/ray/contains/bounds/clearance/sample are six cases of one request rather than six entrypoints; `SpatialResult` the union carrier whose case mirrors the query case (`Proximity`/`Ray`/`Contains`/`Bounds`/`Clearance`/`Sample`), so the result owner is one union not six parallel result structs; `SpatialBackend` the closed `StrEnum` naming the resolved exact-clearance kernel — `CORE` clears through the conservative `python-fcl` `CollisionManager.min_distance_single` separation, `MANIFOLD3D` supersedes it with the exact `Manifold.min_gap` watertight-solid scalar — with `SpatialBackend.resolve` mapping the live interpreter to `MANIFOLD3D` only when its wheel loads and to `CORE` otherwise; `_NATIVE` the `Final[Map[QueryKind, str]]` table naming each native-backend kind's probe module (`bounds`→`rtree`, `clearance`→`fcl`) so the `_offload` set is `_NATIVE`'s keys whose `find_spec` is absent in-process UNION `clearance` whenever the resolved exact gap is companion-only — the one data-driven membership the `_route` fence reads rather than a hardcoded per-case `if q.tag == ...` branch, so every pure-Python kind (proximity, ray, contains, sample) runs synchronously under `boundary` and only a kind whose compiled wheel is missing locally hops to the companion lane; `Outcome` the one struct each arm returns carrying its `SpatialResult` plus the receipt facts (query-point count, hit/inside count, validity verdict) so the `_fold` cross-cut folds one `SpatialReceipt` from a uniform payload instead of a per-arm receipt build; `SpatialReceipt` the typed receipt carrying the query kind, the backend tier, the offloaded verdict, the query-point count, the hit/inside count, the watertight-validity verdict (so a `Contains`/`Clearance` over a non-watertight solid lands as an admitted-phase caveat row), and the index-reuse verdict.
- Cases: `SpatialQuery` cases `Proximity(points, signed)` (the module-level `proximity.closest_point` nearest-surface points/unsigned distances/triangle ids, with the fourth result column carrying `proximity.signed_distance` when `signed` — positive inside the watertight solid — else an empty array, so one arm weaves both `.api` field samplers into one 4-array `Proximity` result rather than two entrypoints or a shape that varies by flag), `Ray(origins, directions, max_distance)` (per-ray nearest-hit triangle id, hit point, and parametric distance through the vectorized `.api`-confirmed `mesh.ray.intersects_location(origins, directions) -> (locations, ray_idx, tri_idx)` batch intersector — Embree-accelerated when `ray.has_embree`, the all-hits rows keyed by `ray_idx` reduced to the nearest per ray by one `numpy.lexsort` and scattered into the dense order so a missed ray fills `tri_idx == -1`/`NaN` and a hit past `max_distance` clamps to a miss, never a per-ray Python loop where the catalogue owns the whole batch in one call), `Contains(points)` (`trimesh.Trimesh.contains` inside/outside boolean mask over the watertight solid, the pure-Python ray-stabbing test), `Bounds(boxes)` (broad-phase candidate triangle ids per AABB through the `rtree`-backed `triangles_tree` R-tree — the native-compiled index `trimesh` binds over libspatialindex with no cp315 wheel, so the arm hops to the companion when `rtree` is absent in-process — the prefilter the clash hop narrows before a `Clearance`/`Proximity` exact pass), `Clearance(other, search_length)` (the minimum solid-to-solid gap, both backends native-compiled companion-band — the `CORE` arm folds the `python-fcl` `collision.CollisionManager.min_distance_single(other)` conservative separation, the `MANIFOLD3D` tier supersedes it with the exact `manifold3d.Manifold.min_gap` watertight scalar when that wheel is the richer one — both the clash-clearance gap the deviation/clash hops read), and `Sample(count, even, attribute)` (`sample.sample_surface_even` blue-noise even samples when `even` else `sample.sample_surface` area-weighted, optionally stacked with `proximity.signed_distance` over the drawn points when `attribute` so one arm yields samples already carrying their signed field value) — matched by `match`/`assert_never`, each binding the `.api` members that own the kind. Inputs are numpy `Nx3`/`Nx6` arrays or counts; outputs are numpy arrays carried in the mirrored `SpatialResult` arm. No mesh-file format axis lives here — the surface arrives as an in-memory `trimesh.Trimesh` across the `mesh ← data/spatial` seam.
- Entry: `query` is the one polymorphic `async` entrypoint discriminating a single `SpatialQuery` or a batch `Sequence[SpatialQuery]` by total `match` (a lone `SpatialQuery` as one case, a `Sequence` as the other — never an `isinstance` ladder), with `@overload` arms keyed on the input shape so a caller narrows on what it passes rather than re-matching the widened return. A single query awaits one `_route`; a batch routes each query in turn into a `Block` of rails (the offload lane's own `CapacityLimiter` already bounds the concurrent subinterpreter hops, so the batch needs no second task-group pool) and folds them through `traversed(rails, by=Disposition.ACCUMULATE)` (the runtime `reliability/faults#FAULT` owner's one disposition-keyed fold; `ACCUMULATE` keeps a faulted query addressable in the aggregate while every successful `SpatialResult` already landed, never a junior `accumulate` boolean flag) into one `RuntimeRail[Block[SpatialResult]]`, never a second batch method nor a quadratic singleton-append. `_route` is the one capability-aware fence over the SINGLE module-level `_dispatch(mesh, q, backend) -> Outcome` body: it probes `q.tag in self._offload` (the data-driven set of native-backend kinds whose compiled wheel is absent in-process) and either runs `_dispatch` synchronously inside `boundary(f"mesh.spatial.{q.tag}", lambda: self._fold(q, _dispatch(self._mesh, q, self._backend)))` for a kind whose backend is resident or awaits `self._lane.offload(_dispatch, self._mesh, q, self._backend)` for a companion-band kind (mapping the offloaded `Outcome` through `self._fold`) — the SAME exhaustive `_dispatch` runs either way, so the offload-vs-sync choice never forks the geometry body. The pure-Python `Proximity`/`Ray`/`Contains`/`Sample` arms run on the synchronous `boundary`: `proximity`/`contains`/`sample` ride the `scipy`+`numpy` core and the `Ray` arm is one batch `mesh.ray.intersects_location` call over the in-memory `RayMeshIntersector` (the pure-Python `ray_triangle` form, the `ray_pyembree` Embree mirror a transparent in-process upgrade when `embreex` resolves), not a per-ray Python loop, so none stalls the loop nor carries a hard native dependency. The native-compiled kinds are companion-band: the `rtree` `Bounds` index build plus query, the `python-fcl` `CollisionManager` separation, and the exact `manifold3d.Manifold` build plus `min_gap` fold each ride one PEP 734 subinterpreter hop when their wheel is missing in-process, under the lane's shared `CapacityLimiter` and deadline, the lane importing neither the native module nor `_dispatch` (a module-level arg-only `Callable` the no-pickle hop receives verbatim) and converting a `BrokenWorkerInterpreter`/deadline `TimeoutError` through its own `async_boundary`, exactly the offload-aware parity the `mesh/repair.md#MESH` and `mesh/quality.md#QUALITY` siblings hold. `_dispatch` is one total `match`/`assert_never` over every kind: `Proximity` calls `proximity.closest_point(mesh, points)` for nearest points/distances/triangle ids and stacks `proximity.signed_distance(mesh, points)` into the fourth result column when `signed`; `Ray` calls the vectorized `mesh.ray.intersects_location(origins, _unit(directions))` once for the whole batch and `_nearest_hits` reduces the all-hits `(locations, ray_idx, tri_idx)` to the nearest hit per ray through one `lexsort` on `(ray_idx, dist)` and the first-of-group mask, scattering into the dense origin order — a ray absent from `ray_idx` or whose nearest hit exceeds `max_distance` fills `tri_idx == -1` and a `NaN` hit point — never a per-ray cast; `Contains` gates `Trimesh.is_watertight` BEFORE the test and short-circuits an off-solid query to an admitted caveat rather than running `Trimesh.contains` over a leaky surface and returning a meaningless mask; `Bounds` queries the `rtree`-backed `triangles_tree` R-tree per box for candidate triangle ids; `Sample` draws through the evenness selector and stacks `signed_distance` when `attribute`; `Clearance` gates both surfaces watertight, then folds the exact `manifold3d.Manifold.min_gap(other, search_length)` when `backend is MANIFOLD3D` (building each `Manifold` once via `_to_manifold`) else the conservative `python-fcl` `collision.CollisionManager` `add_object` + `min_distance_single(other)` separation. An offloaded kind rebuilds whatever index its hop needs inside the subinterpreter (a live `triangles_tree` R-tree or `Manifold` cannot cross the no-pickle boundary, so the capsule caches no native handle across hops — the per-hop build is the offload cost the lane isolates, distinct from the `mesh/quality.md#QUALITY` in-process `ExactTopology` VALUE reuse), while the in-process pure-Python arms read off the lazily-cached `kdtree`/`ray` indices the `Trimesh` owns, built once and reused across the capsule's lifetime. The cross-cutting concern — folding each `Outcome` into one `SpatialReceipt` row with the backend tier and index-reuse verdict — rides the one `_fold(q, out)` method `_route` calls on both the synchronous and the offloaded `Outcome`, so a new kind writes only its `_dispatch` arm returning an `Outcome` (the watertight gate stays inside the `Contains`/`Clearance` arms, which carry the shape-specific short-circuit the fold cannot apply uniformly); the `_f64` float64 coercion is a shared helper each arm calls on its own array shape, since proximity points, ray origins/directions, contains points, and bounds boxes carry distinct `Nx3`/`Nx6` shapes the fold cannot coerce uniformly.
- Auto: `Trimesh.kdtree` (the `scipy` vertex kdtree) is a `.api`-confirmed pure-Python lazily-cached spatial-index property (`trimesh.md` cached-property row [13]) recomputed only on geometry change, so the proximity arm indexes once in-process; `Trimesh.triangles_tree` is the SAME row's `rtree` triangle R-tree but over the native libspatialindex backend (`trimesh.md` ABI line 15) with no cp315 wheel, so the `Bounds` arm builds it inside the companion hop rather than the cp315 capsule; `mesh.ray` is the lazily-built `RayMeshIntersector` (`trimesh.md` query-owner rows [02]/[03]) — the pure-Python `ray.ray_triangle` form on the core, transparently superseded by the `ray.ray_pyembree` Embree mirror when `ray.has_embree` (the native `embreex` wheel) resolves, so the arm is core-resident either way and only faster with the accel; `proximity.signed_distance` (`trimesh.md` proximity row [07]) returns positive inside and negative outside a watertight solid, the sign the deviation hop reads for over/under-build, and `proximity.closest_point` (row [06]) returns closest points, unsigned distances, and triangle ids in one call; `mesh.ray.intersects_location(origins, directions) -> (locations, ray_idx, tri_idx)` (`trimesh.md` ray row [04]) returns ALL hit points keyed by `ray_idx`, the vectorized batch the `Ray` arm reduces to the nearest hit per ray; `collision.CollisionManager.min_distance_single(mesh) -> distance` (`trimesh.md` collision row [07]) is the conservative `CORE`-backend clearance separation over the native `python-fcl` engine (`trimesh.md` ABI line 15, no cp315 wheel, companion-band); `sample.sample_surface_even` (sampling row [10]) rejection-samples toward blue-noise spacing while `sample.sample_surface` (row [09]) is area-weighted, the evenness a `Sample` boolean not a parallel entrypoint; on the gated `MANIFOLD3D` tier `Manifold.min_gap(other, search_length)` (`manifold3d.md` query row [10]) supersedes the FCL separation with the exact solid-to-solid gap, the surface entering the kernel as `manifold3d.Mesh` (32-bit `tri_verts`) or `Mesh64` (`manifold3d.md` type rows [03]/[04]) selected by vertex count so a surface past the `uint32` ceiling never silently truncates its triangle indices.
- Receipt: `MeshSpatial` conforms to the runtime `observability/receipts#RECEIPT` `ReceiptContributor` Protocol — the one `_fold` cross-cut turns each arm's returned `Outcome` into one `SpatialReceipt`, and `contribute(self) -> Iterable[Receipt]` YIELDS one `Receipt.of("mesh.spatial", (phase, r.kind, facts))` row (never `return`s a bare `Receipt` — the Protocol port and the `@receipted` aspect's `_stream` normalizer both require the `Iterable[Receipt]` stream, the shape the `mesh/repair.md#MESH` and `mesh/quality.md#QUALITY` siblings hold) through the owner's shape-polymorphic `(Phase, subject, facts)` factory, carrying the query kind as the subject and the `SpatialBackend.value` tier, the offloaded verdict (so the receipt records whether the kind ran on the core or hopped the companion lane), the query-point count, the hit/inside count, and the index-reuse verdict as native scalars the receipt owner's `enc_hook=repr` renderer serializes without a `str()` coerce; a daemon or serve path harvests the stream under the `@receipted(REDACTION)` aspect rather than an inline emit. The `phase: Phase` discriminant is the canonical `observability/receipts#RECEIPT` `Phase` literal imported whole, never an inline `Literal["admitted", "emitted"]` re-spelling that drops the `"planned"` member: a `Contains`/`Clearance` over a non-watertight surface short-circuits at the arm's watertight gate to `valid=False`, so `contribute` keys `phase="admitted"` and the row is a caveat flagging the unreliable verdict rather than asserting one, while a clean query keys `phase="emitted"`. The spatial query produces no `GraduationReceipt` subject — it is the read-side primitive the deviation and clash hops consume, and the graduating subject (`scan-deviation`, `mesh-algebra`) belongs to those owners; the canonical `GeometrySubject` literal (`rasm.compute.graduation.handoff#GeometrySubject`) is never minted here.
- Packages: `trimesh` (`Trimesh`/`Trimesh.contains`/`Trimesh.is_watertight`/`Trimesh.kdtree` pure-Python on the core, `Trimesh.triangles_tree`/`collision.CollisionManager` the native `rtree`/`python-fcl` companion-band arms, `Trimesh.ray`/`ray.ray_triangle.RayMeshIntersector.intersects_location` core-resident with the `ray.ray_pyembree` Embree mirror a transparent native upgrade, `proximity.closest_point`/`proximity.signed_distance`/`sample.sample_surface`/`sample.sample_surface_even`), `numpy` (`ndarray`/`asarray`/`fromiter`/`full`/`empty`/`zeros`/`iinfo`/`zeros_like` query and result arrays over `Nx3`/`Nx6` shapes, `linalg.norm` ray-direction normalization and the origin-to-hit parametric distance, `divide` the zero-direction branchless fold over the `out=`/`where=` ufunc kwargs, `lexsort`/`concatenate` the nearest-per-ray first-of-group reduction over the `intersects_location` all-hits rows and the `ray_idx`-keyed fancy-index scatter into the dense per-ray order), `manifold3d` (the gated exact-clearance tier — `Manifold`/`Mesh`/`Mesh64`/`Manifold.min_gap`, reached only on the `MANIFOLD3D` backend row inside the offloaded kernel), `expression` (`tagged_union`/`tag`/`case` the `SpatialQuery`/`SpatialResult` discriminated unions, `Map`/`Map.of_seq` the `_NATIVE` probe table, `Block`/`Block.of_seq` the batch rail carrier), `msgspec` (`Struct(frozen=True)` the GC-tracked `Outcome` nesting the `SpatialResult` union, `Struct(frozen=True, gc=False)` the leaf-scalar `SpatialReceipt`), runtime `reliability/faults#FAULT` (`RuntimeRail`/`boundary`/`traversed`/`Disposition`), `observability/receipts#RECEIPT` (`Phase`/`Receipt`/`ReceiptContributor`/`Redaction`/`receipted`), `execution/lanes#LANE` (`LanePolicy`/`LanePolicy.offload` the subinterpreter hop the native-backend kinds ride when their wheel is absent in-process). The native `rtree`/`python-fcl`/`embreex` backends are transitive optional `trimesh` extras the workspace manifest does not declare and that ship no cp315 wheel, so they resolve only on the `python_version<'3.15'` companion the daemon hosts.
- Growth: a new query kind is one `SpatialQuery` case plus the mirrored `SpatialResult` arm plus one `_dispatch` match arm (plus one `_NATIVE` row naming its probe module when the kind rides a native-compiled backend) — never a new entrypoint, the `assert_never` tail forcing the arm and the `_fold` cross-cut folding its receipt without a per-case receipt build; a new exact-clearance kernel tier is one `SpatialBackend` row plus its `resolve` branch only when this owner actually dispatches it, never a second query owner; the deviation hop composes `Proximity(points, signed=True)` and reads the signed fourth column of `SpatialResult.Proximity` for signed nearest-surface deviation rather than re-deriving closest-point math, or composes `Sample(count, attribute=True)` to draw verification points already carrying their signed field; the clash hop composes `Bounds` for broad-phase candidate triangles then `Clearance` for the exact gap, never re-deriving overlap; the CPU-bound native-backend kernels — the `rtree` bounds, the `python-fcl` separation, and the exact `manifold3d.Manifold` build plus `min_gap` fold — hand across the runtime `execution/lanes#LANE` `LanePolicy.offload` per-subinterpreter variant (`anyio.to_interpreter.run_sync` under one `CapacityLimiter`, the no-pickle PEP-734 hop, degrading to `anyio.to_thread.run_sync` only where a cp315 build ships no runnable `concurrent.interpreters`, NEVER a `to_process` pickle round-trip the lanes owner rejects as the process-pool serialization tax) as ONE `LanePolicy.offload(kernel, *args)` hand-off over the already-landed lane — the lane never imports the kernel, while every pure-Python kind (proximity, ray, contains, sample) runs synchronously under `boundary` since `trimesh` lazily caches the `kdtree`/`ray` build on the in-memory body the capsule holds; zero new surface, no parallel per-kind class family.
- Boundary: no point-cloud registration (that is `scan-registration`); no mesh repair or boolean (that is `mesh/repair`); no mesh-quality metric (that is `mesh/quality`); the exact `manifold3d.min_gap` kernel is reached only through the `MANIFOLD3D` `SpatialBackend` row superseding the conservative `python-fcl` clearance, never a second direct CSG owner; the parallel vertex-KNN of `open3d.KDTreeFlann`/`small_gicp.KdTree` is NOT a spatial backend row — vertex nearest-neighbor is a distinct coarser result from `closest_point` exact surface projection and belongs to the `scan/deviation`+`scan/registration` cloud-correspondence owners, so a dead `OPEN3D`/`SMALL_GICP` tier resolved-but-never-dispatched here is itself a deleted form; mesh-file decode/encode is NOT this owner — the data `MeshPayload` owner (`rasm.data.spatial.mesh`) holds the canonical three-engine `trimesh`/`meshio`/`rhino3dm` codec plus GLB preview, so geometry hands in-memory `Trimesh` across the `mesh ← data/spatial` seam and never opens or writes a file. A `closest_point`/`signed_distance`/`intersects`/`contains`/`sample`/`min_gap` method family over the `SpatialQuery` row, six parallel `ProximityResult`/`RayResult`/`ContainsResult`/`BoundsResult`/`ClearanceResult`/`SampleResult` structs, a hand-rolled kd-tree or BVH where `trimesh.triangles_tree` is admitted, a hand-rolled ray-triangle intersector where the `mesh.ray` `RayMeshIntersector` (the pure-Python `ray_triangle` core form, the `ray_pyembree` Embree mirror a transparent in-process upgrade) is admitted, gating the core-resident `Ray` arm behind a native-wheel probe where `trimesh.md` rows [02]-[05] confirm the pure-Python ray surface runs in-process and Embree only accelerates it (a companion-band ray is a needless hop and a per-interpreter availability hole), a per-ray Python `ray_cast` comprehension where `intersects_location` owns the whole batch in one vectorized call, a per-arm receipt build duplicating the one `_fold` cross-cut, running `contains`/`min_gap` over a non-watertight surface and flagging the verdict afterward instead of gating before the query, a hardcoded 32-bit `manifold3d.Mesh` that truncates large-mesh triangle indices, ASSERTING the native `rtree` `Bounds` or `python-fcl` `Clearance` as an always-available cp315-core arm where `trimesh.md` line 15 marks both backends native with no cp315 wheel — the false-premise form this rebuild deletes, the correct shape probing `find_spec` and offloading the kind to the companion lane when its wheel is absent in-process rather than railing it to an `ImportError` on the bare core, a synchronous `query` blocking the event loop on a CPU-bound native-backend build where the `LanePolicy.offload` subinterpreter hop isolates it, a `lane` accepted in `__init__` yet never composed (the dead-seam form the offload-aware `async query` resolves by actually awaiting `self._lane.offload(_dispatch, ...)` for a companion-band kind), a capsule-cached `_solid` `Manifold` or live `triangles_tree` reused across offload hops where neither native handle can cross the no-pickle PEP 734 boundary, an `isinstance(q, SpatialQuery)` dispatch where the polymorphic entry is one total `match` over the single-or-`Sequence` shape, a `contribute` returning a bare `Receipt` rather than YIELDING the `Iterable[Receipt]` stream the `ReceiptContributor` Protocol and the `@receipted` `_stream` normalizer declare, an inline `phase: Literal["admitted", "emitted"]` re-spelling that drops the canonical `Phase` literal's `"planned"` member where `Phase` is imported whole, a `traversed(accumulate=...)` boolean flag where the runtime `Disposition` row owns the batch strategy, a `Receipt.of(phase, owner, subject, facts)` positional arity where the owner mints through the one `Receipt.of(owner, (phase, subject, facts))` shape-polymorphic factory, a `str()`/`f"{...:.6f}"`-coerced facts map where the native-scalar `dict[str, object]` rides the receipts `enc_hook=repr` renderer, carrying `open3d`/`small_gicp` as resolved-but-undispatched spatial backend rows, and ANY `MeshFormat`/`Codec`/`load`/`export` arm re-deriving the `MeshPayload` seam are the deleted forms.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Sequence
from enum import StrEnum
from importlib.util import find_spec
from typing import TYPE_CHECKING, Final, Literal, assert_never, overload

import numpy as np
import trimesh
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.faults import Disposition, RuntimeRail, boundary, traversed
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Phase, Receipt, ReceiptContributor

if TYPE_CHECKING:
    import manifold3d

# --- [TYPES] ----------------------------------------------------------------------------

type QueryKind = Literal["proximity", "ray", "contains", "bounds", "clearance", "sample"]


class SpatialBackend(StrEnum):  # the resolved exact-clearance KERNEL; vertex-KNN acceleration (open3d/small_gicp) belongs to the scan/deviation+registration consumers
    CORE = "core"              # the conservative python-fcl CollisionManager separation (companion-band native, no cp315 wheel)
    MANIFOLD3D = "manifold3d"  # the exact Manifold.min_gap superseding the FCL separation when the richer wheel resolves

    @staticmethod
    def resolve() -> "SpatialBackend":  # the exact gap supersedes FCL only where the manifold3d wheel loads (companion); CORE folds the conservative FCL separation otherwise
        return SpatialBackend.MANIFOLD3D if find_spec("manifold3d") is not None else SpatialBackend.CORE


@tagged_union(frozen=True)
class SpatialQuery:
    tag: QueryKind = tag()
    proximity: tuple[np.ndarray, bool] = case()
    ray: tuple[np.ndarray, np.ndarray, float] = case()
    contains: np.ndarray = case()
    bounds: np.ndarray = case()
    clearance: tuple[trimesh.Trimesh, float] = case()
    sample: tuple[int, bool, bool] = case()

    @staticmethod
    def Proximity(points: np.ndarray, signed: bool = False) -> "SpatialQuery":
        return SpatialQuery(proximity=(points, signed))

    @staticmethod
    def Ray(origins: np.ndarray, directions: np.ndarray, max_distance: float = float("inf")) -> "SpatialQuery":  # intersects_location casts infinite rays; a finite max_distance clamps the first hit to a miss past the range
        return SpatialQuery(ray=(origins, directions, max_distance))

    @staticmethod
    def Contains(points: np.ndarray) -> "SpatialQuery":
        return SpatialQuery(contains=points)

    @staticmethod
    def Bounds(boxes: np.ndarray) -> "SpatialQuery":  # Nx6 (xmin,ymin,zmin,xmax,ymax,zmax) AABB rows
        return SpatialQuery(bounds=boxes)

    @staticmethod
    def Clearance(other: trimesh.Trimesh, search_length: float) -> "SpatialQuery":
        return SpatialQuery(clearance=(other, search_length))

    @staticmethod
    def Sample(count: int, even: bool = True, attribute: bool = False) -> "SpatialQuery":
        return SpatialQuery(sample=(count, even, attribute))


@tagged_union(frozen=True)
class SpatialResult:
    tag: QueryKind = tag()
    proximity: tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray] = case()
    ray: tuple[np.ndarray, np.ndarray, np.ndarray] = case()
    contains: np.ndarray = case()
    bounds: tuple[np.ndarray, ...] = case()
    clearance: float = case()
    sample: tuple[np.ndarray, np.ndarray, np.ndarray] = case()

    @staticmethod
    def Proximity(points: np.ndarray, distances: np.ndarray, triangle_ids: np.ndarray, signed: np.ndarray) -> "SpatialResult":
        return SpatialResult(proximity=(points, distances, triangle_ids, signed))

    @staticmethod
    def Ray(triangle_ids: np.ndarray, points: np.ndarray, t: np.ndarray) -> "SpatialResult":
        return SpatialResult(ray=(triangle_ids, points, t))

    @staticmethod
    def Contains(mask: np.ndarray) -> "SpatialResult":
        return SpatialResult(contains=mask)

    @staticmethod
    def Bounds(candidates: tuple[np.ndarray, ...]) -> "SpatialResult":
        return SpatialResult(bounds=candidates)

    @staticmethod
    def Clearance(gap: float) -> "SpatialResult":
        return SpatialResult(clearance=gap)

    @staticmethod
    def Sample(points: np.ndarray, triangle_ids: np.ndarray, signed: np.ndarray) -> "SpatialResult":
        return SpatialResult(sample=(points, triangle_ids, signed))


# --- [CONSTANTS] ------------------------------------------------------------------------

# each native-COMPILED-backend kind mapped to its `find_spec` probe module — `bounds` over the libspatialindex
# `rtree` R-tree, `clearance` over the `python-fcl` engine (import name `fcl`); both are optional `trimesh` extras
# the manifest does not declare and that ship no cp315 wheel. `__init__` folds this into the `_offload` set (the
# keys whose probe is absent in-process, plus `clearance` when only the companion carries the exact gap), so the
# `_route` fence reads one data-driven membership rather than a hardcoded per-case `if q.tag == ...` branch, and
# every pure-Python kind (proximity/ray/contains/sample) stays off the set and runs synchronously under `boundary`.
_NATIVE: Final[Map[QueryKind, str]] = Map.of_seq((("bounds", "rtree"), ("clearance", "fcl")))


# --- [MODELS] ---------------------------------------------------------------------------


class Outcome(Struct, frozen=True):  # one arm's payload plus the receipt facts the `_fold` cross-cut folds; GC-tracked since `result` nests the SpatialResult union and its numpy arrays, never a per-arm receipt build
    result: SpatialResult
    query_count: int
    hit_count: int
    valid: bool


class SpatialReceipt(Struct, frozen=True, gc=False):
    kind: QueryKind
    backend: SpatialBackend
    offloaded: bool
    query_count: int
    hit_count: int
    valid: bool
    indexed: bool


# --- [OPERATIONS] -----------------------------------------------------------------------


def _f64(a: np.ndarray) -> np.ndarray:
    return np.asarray(a, dtype=np.float64)


def _unit(d: np.ndarray) -> np.ndarray:  # zero-direction rows degrade to a zero ray (an absent-ray_idx miss downstream) rather than dividing by zero
    norm = np.linalg.norm(d, axis=1, keepdims=True)
    return np.divide(d, norm, out=np.zeros_like(d), where=norm > 0)


def _nearest_hits(n: int, locations: np.ndarray, ray_idx: np.ndarray, tri_idx: np.ndarray, origins: np.ndarray, max_distance: float) -> tuple[np.ndarray, np.ndarray, np.ndarray]:
    # intersects_location returns ALL hits per ray keyed by ray_idx; reduce to the nearest-per-ray first hit by one
    # lexsort on (ray_idx, dist) and the first-of-group mask, scatter into the dense Nx ray order (a missed ray keeps
    # tri==-1/NaN), and clamp a hit past max_distance back to a miss so a finite range bounds the infinite cast.
    face = np.full(n, -1, dtype=np.int64)
    pos = np.full((n, 3), np.nan, dtype=np.float64)
    dist = np.full(n, np.nan, dtype=np.float64)
    if ray_idx.size == 0:  # every ray missed; the dense miss-filled arrays are the result
        return face, pos, dist
    hit_dist = np.linalg.norm(locations - origins[ray_idx], axis=1)
    order = np.lexsort((hit_dist, ray_idx))  # primary ray_idx, secondary distance ascending => first-of-group is the nearest hit
    ri = ray_idx[order]
    keep = np.concatenate(([True], ri[1:] != ri[:-1])) & (hit_dist[order] <= max_distance)
    face[ri[keep]], pos[ri[keep]], dist[ri[keep]] = tri_idx[order][keep], locations[order][keep], hit_dist[order][keep]
    return face, pos, dist


def _to_manifold(mesh: trimesh.Trimesh) -> "manifold3d.Manifold":  # Mesh64 past the uint32 ceiling so a large surface keeps 64-bit positions and triangle indices
    import manifold3d  # noqa: PLC0415

    verts, faces = np.asarray(mesh.vertices), np.asarray(mesh.faces)
    if len(verts) > np.iinfo(np.uint32).max:  # 32-bit Mesh overflows past ~4.29B verts; Mesh64 is the f64 carrier (.api type row [04]), so its positions are f64 too
        return manifold3d.Manifold(manifold3d.Mesh64(vert_properties=verts.astype(np.float64), tri_verts=faces.astype(np.uint64)))
    return manifold3d.Manifold(manifold3d.Mesh(vert_properties=verts.astype(np.float32), tri_verts=faces.astype(np.uint32)))


# the one exhaustive geometry dispatch over EVERY query kind — module-level and arg-only (`(mesh, query, backend)`)
# so the no-pickle PEP 734 subinterpreter receives it verbatim as the offloaded kernel, the lane importing neither it
# nor any native backend. The pure-Python kinds bind the trimesh/scipy/numpy `.api` members; the native-backend arms
# (`bounds` over rtree, `clearance` over fcl/manifold3d) build their index per call (a live R-tree or Manifold cannot
# cross the subinterpreter boundary, so no cached native handle here, unlike the in-process mesh/quality value reuse).
# The total `match`/`assert_never` forces a new SpatialQuery case to add its arm here, and `_route` decides
# synchronous-vs-offload by the `_offload` set, never a second dispatch body. `backend` selects the clearance kernel.
def _dispatch(mesh: trimesh.Trimesh, q: SpatialQuery, backend: SpatialBackend) -> Outcome:
    match q:
        case SpatialQuery(tag="proximity", proximity=(points, signed)):
            pts = _f64(points)
            near, distance, triangle_ids = trimesh.proximity.closest_point(mesh, pts)
            signed_field = trimesh.proximity.signed_distance(mesh, pts) if signed else np.empty(0)
            return Outcome(SpatialResult.Proximity(near, _f64(distance), np.asarray(triangle_ids), _f64(signed_field)), len(pts), len(pts), True)
        case SpatialQuery(tag="ray", ray=(origins, directions, max_distance)):  # pure-Python RayMeshIntersector batch, Embree only an in-process accel; core-resident, no offload
            o = _f64(origins)
            locations, ray_idx, tri_idx = mesh.ray.intersects_location(o, _unit(_f64(directions)))  # ALL hits per ray, keyed by ray_idx
            face, pos, dist = _nearest_hits(len(o), _f64(locations), np.asarray(ray_idx), np.asarray(tri_idx), o, max_distance)
            return Outcome(SpatialResult.Ray(face, pos, dist), len(o), int((face != -1).sum()), True)
        case SpatialQuery(tag="contains", contains=points):
            if not mesh.is_watertight:  # the inside/outside test is meaningless off a watertight solid; gate before the query, flag the caveat
                return Outcome(SpatialResult.Contains(np.zeros(len(points), dtype=bool)), len(points), 0, False)
            mask = np.asarray(mesh.contains(_f64(points)), dtype=bool)
            return Outcome(SpatialResult.Contains(mask), len(mask), int(mask.sum()), True)
        case SpatialQuery(tag="bounds", bounds=boxes):  # the rtree-backed triangles_tree (native libspatialindex); offloaded when rtree is absent in-process
            tree, rows = mesh.triangles_tree, _f64(boxes)
            candidates = tuple(np.fromiter(tree.intersection(tuple(row)), dtype=np.int64) for row in rows)
            return Outcome(SpatialResult.Bounds(candidates), len(rows), sum(c.size for c in candidates), True)
        case SpatialQuery(tag="sample", sample=(count, even, attribute)):
            sampler = trimesh.sample.sample_surface_even if even else trimesh.sample.sample_surface
            points, triangle_ids = sampler(mesh, count)
            signed = trimesh.proximity.signed_distance(mesh, points) if attribute else np.empty(0)
            return Outcome(SpatialResult.Sample(np.asarray(points), np.asarray(triangle_ids), _f64(signed)), len(points), len(points), True)
        case SpatialQuery(tag="clearance", clearance=(other, search_length)):  # both backends native companion-band; exact manifold3d gap supersedes the conservative fcl separation
            if not (mesh.is_watertight and other.is_watertight):  # a solid-to-solid gap demands both surfaces close on either backend
                return Outcome(SpatialResult.Clearance(float("nan")), 1, 0, False)
            gap = float(_to_manifold(mesh).min_gap(_to_manifold(other), search_length)) if backend is SpatialBackend.MANIFOLD3D else _fcl_gap(mesh, other)
            return Outcome(SpatialResult.Clearance(gap), 1, int(gap <= search_length), True)
        case _ as unreachable:
            assert_never(unreachable)


def _fcl_gap(mesh: trimesh.Trimesh, other: trimesh.Trimesh) -> float:  # the CORE clearance: conservative python-fcl minimum separation, exact min_gap superseding it under MANIFOLD3D
    manager = trimesh.collision.CollisionManager()
    manager.add_object("surface", mesh)
    return float(manager.min_distance_single(other))


# --- [SERVICES] -------------------------------------------------------------------------


class MeshSpatial(ReceiptContributor):
    def __init__(self, mesh: trimesh.Trimesh, lane: LanePolicy, backend: SpatialBackend | None = None) -> None:
        self._mesh = mesh
        self._lane = lane  # the per-subinterpreter offload seam a native-backend arm rides when its wheel is absent; the lane never imports the kernel
        self._backend = backend or SpatialBackend.resolve()
        # a kind offloads iff its native backend module is missing IN-PROCESS: `bounds` probes `rtree`, `clearance`
        # probes the module its resolved kernel needs (`manifold3d` under MANIFOLD3D, else the `fcl` separation). The
        # pure-Python kinds carry no `_NATIVE` row and never offload. Resolving the set once at construction means the
        # `_route` fence is one membership test, and a bare cp315 capsule (no rtree/fcl/manifold3d) hops every
        # native kind to the companion lane rather than railing an ImportError the page once asserted away.
        clearance_mod = "manifold3d" if self._backend is SpatialBackend.MANIFOLD3D else _NATIVE["clearance"]
        probe = _NATIVE.add("clearance", clearance_mod)  # persistent insert overriding the clearance probe with the resolved kernel's module
        self._offload: frozenset[QueryKind] = frozenset(k for k, mod in probe.items() if find_spec(mod) is None)
        self._last: SpatialReceipt | None = None

    @overload
    async def query(self, q: SpatialQuery) -> "RuntimeRail[SpatialResult]": ...
    @overload
    async def query(self, q: Sequence[SpatialQuery]) -> "RuntimeRail[Block[SpatialResult]]": ...
    async def query(self, q: SpatialQuery | Sequence[SpatialQuery]) -> "RuntimeRail[SpatialResult] | RuntimeRail[Block[SpatialResult]]":
        # arity is a property of the argument: a lone query awaits one capability-aware route, a sequence routes each
        # in turn (the offload lane's own CapacityLimiter bounds the concurrent subinterpreter hops, so the batch
        # needs no second pool) and folds the rails under one Disposition — never an isinstance ladder nor a second method.
        match q:
            case SpatialQuery() as one:
                return await self._route(one)
            case batch:
                rails = Block.of_seq([await self._route(one) for one in batch])
                return traversed(rails, by=Disposition.ACCUMULATE)  # one faulted query stays addressable; the runtime owns the strategy row, never a boolean accumulate flag

    async def _route(self, q: SpatialQuery) -> "RuntimeRail[SpatialResult]":
        # the one capability-aware fence over the single `_dispatch` body: a kind whose backend is resident runs
        # `_dispatch` synchronously under `boundary`, a native-backend kind whose wheel is absent in-process offloads
        # the SAME `_dispatch` onto the lane subinterpreter under the shared CapacityLimiter and deadline, the lane
        # converting a BrokenWorkerInterpreter/TimeoutError through its own async_boundary onto the rail. Both paths
        # produce the same Outcome the one `_fold` cross-cut turns into the receipt row plus the SpatialResult, so the
        # offload-vs-sync split never forks the geometry dispatch.
        offloaded = q.tag in self._offload
        if offloaded:
            return (await self._lane.offload(_dispatch, self._mesh, q, self._backend)).map(lambda out: self._fold(q, out, offloaded))
        return boundary(f"mesh.spatial.{q.tag}", lambda: self._fold(q, _dispatch(self._mesh, q, self._backend), offloaded))

    def _fold(self, q: SpatialQuery, out: Outcome, offloaded: bool) -> SpatialResult:  # the cross-cutting receipt aspect: one uniform SpatialReceipt off each Outcome, geometry-only `_dispatch` body elsewhere
        self._last = SpatialReceipt(q.tag, self._backend, offloaded, out.query_count, out.hit_count, out.valid, indexed=not offloaded)
        return out.result

    def contribute(self) -> Iterable[Receipt]:  # the ReceiptContributor port YIELDS the stream the @receipted aspect's _stream normalizes; never a bare Receipt return
        r = self._last or SpatialReceipt("proximity", self._backend, False, 0, 0, True, False)
        phase: Phase = "emitted" if r.valid else "admitted"
        # native int/bool/str ride the EventDict dict[str, object]; the receipts Encoder(enc_hook=repr) serializes them without a str() coerce
        facts: dict[str, object] = {"backend": r.backend.value, "offloaded": r.offloaded, "queries": r.query_count, "hits": r.hit_count, "indexed": r.indexed}
        yield Receipt.of("mesh.spatial", (phase, r.kind, facts))  # the owner's shape-polymorphic (Phase, subject, facts) factory; subject is the query kind
```

## [03]-[RESEARCH]

- [TRIMESH_PROXIMITY]: the `Proximity`/`Sample` arms bind the `.api`-confirmed module-level entrypoints exactly as the `scan/deviation.md#DEVIATION` consumer does — `proximity.closest_point(mesh, points) -> (closest, distance, triangle_id)` (`trimesh.md` proximity row [06]), `proximity.signed_distance(mesh, points) -> NDArray` (row [07], positive inside the watertight solid), `Trimesh.contains(points)` (sampling row [11]), and `sample.sample_surface(mesh, count)`/`sample.sample_surface_even(mesh, count) -> (points, face_index)` (rows [09]/[10]). The earlier `proximity.ProximityQuery(mesh).on_surface` instance form is the persistent query-owner mirror of the same closest-point/sign/triangle-id surface (`trimesh.md` query-owner row [01]); the one-shot module functions carry the identical contract, so the arm cites the module form. The lazily-cached `Trimesh.kdtree`/`triangles_tree` spatial indices confirm against the catalogue's cached-property spatial-index axis (row [13], the `scipy` vertex kdtree and the `rtree` triangle R-tree); the proximity arm's `kdtree` is the pure-Python `scipy` index resident on the core, while the `Bounds` arm's `triangles_tree.intersection(aabb)` rides the native libspatialindex `rtree` backend (`trimesh.md` ABI line 15, no cp315 wheel) and so resolves on the companion hop when `rtree` is absent in-process.
- [RAY_AND_CLEARANCE]: the `Ray` arm binds the `.api`-confirmed spine ray surface — `mesh.ray` is the cached `RayMeshIntersector` (`trimesh.md` query-owner rows [02]/[03], the `ray.ray_pyembree` Embree mirror auto-selected when `ray.has_embree`) and `mesh.ray.intersects_location(ray_origins, ray_directions) -> (locations, ray_idx, tri_idx)` (`trimesh.md` ray row [04]) returns ALL hits keyed by `ray_idx` in ONE vectorized batch call (the live distribution takes no `multiple_hits` kwarg here — that selector lives on `intersects_id` — so the arm reduces the all-hits rows itself). The arm unit-normalizes the directions through `_unit` (a zero-direction row degrades to a zero ray, an absent `ray_idx` miss, rather than a `0/0` NaN), runs the single batch query, and `_nearest_hits` reduces to the nearest hit per ray through one `lexsort` on `(ray_idx, dist)` and the first-of-group mask before scattering into the dense per-ray order — a ray absent from `ray_idx` or whose nearest hit exceeds `max_distance` keeps `tri == -1` and a `NaN` hit point, the parametric distance recovered as the origin-to-hit `linalg.norm` and clamped to a miss past the range. The nearest-hit triangle agrees with `intersects_first`'s dense per-ray result, so the one `intersects_location` call yields the consistent triangle, point, and distance without a second query. This is the core-resident ray path: no companion-band gate, no per-ray Python comprehension, no subinterpreter offload, because `trimesh` ships the pure-Python `ray_triangle.RayMeshIntersector` that owns the whole ray batch in-process (`trimesh.md` query-owner row [02]) and only swaps in the native `ray_pyembree` mirror (row [03]) as a transparent in-process accel when `ray.has_embree` resolves — the arm never depends on a native wheel, it is merely faster with one. The `Clearance` arm has two backends over the same case, BOTH native-compiled and companion-band (`trimesh.md` ABI line 15 marks `python-fcl` and `manifold3d` native with no cp315 wheel): the `CORE` arm folds the `python-fcl` `collision.CollisionManager` (`trimesh.md` query-owner row [04], `min_distance_single(other)` collision row [07]) conservative minimum-separation distance, and the `MANIFOLD3D` tier supersedes it with the exact `Manifold.min_gap(other, search_length) -> float` (`manifold3d.md` query row [10]) watertight solid-to-solid gap — `_route` offloads whichever kernel resolves when its module is absent in-process. The exact tier enters the `manifold3d` kernel as a `manifold3d.Mesh`/`Mesh64` triangle-soup carrier (`manifold3d.md` type rows [03]/[04], `tri_verts` `Nx3` index array, `vert_properties` columns 0-2 XYZ) — `Mesh64` selected when the vertex count exceeds the `uint32` ceiling so a large surface keeps 64-bit positions and triangle indices instead of truncating — wrapped in a `Manifold` INSIDE the `_dispatch` clearance arm the lane offloads (the arm rebuilds the `Manifold` per `_dispatch` call, since a live `Manifold` cannot cross the no-pickle PEP 734 subinterpreter boundary — there is no capsule native handle cache, the per-hop build being the offload cost the lane isolates, the same picklable-VALUE discipline the `mesh/quality.md#QUALITY` sibling holds where the offloaded kernel returns an `ExactTopology` VALUE rather than a live `_solid`). Both clearance kernels and the `rtree` bounds hop the lane when their module is absent in-process; only the pure-Python proximity/ray/contains/sample arms run on the core in-process.
- [BACKEND_TIER]: `SpatialBackend.resolve` probes `importlib.util.find_spec("manifold3d")` and returns `MANIFOLD3D` when the wheel loads, falling to `CORE` otherwise — the per-venv resolution the daemon hosts, confirmed against the companion manifest markers. The enum names the resolved exact-clearance KERNEL, not a per-kind selector: `CORE` clears through the conservative `python-fcl` `CollisionManager` separation, `MANIFOLD3D` supersedes it with the exact `Manifold.min_gap`. The orthogonal availability concern — WHERE each arm runs — is the `_offload` set the capsule folds from `_NATIVE` `find_spec` probes: `bounds` (rtree), `clearance` (the resolved kernel's `fcl`/`manifold3d` module), and any future native kind hop the companion lane when their wheel is absent in-process, while the pure-Python proximity/ray/contains/sample arms are availability-invariant and run on the core. So a kind's KERNEL is one `SpatialBackend` axis and its LOCATION is one `_offload` axis; neither is the asserted-always cp315 floor the prior draft mistook the native `rtree`/`fcl` backends for. `open3d.geometry.KDTreeFlann` (`open3d.md` type row [05]) and `small_gicp.KdTree.batch_knn_search` (`small-gicp.md` search rows [03]/[04]) are deliberately NOT backend rows: they return nearest VERTICES, a coarser correspondence than `proximity.closest_point`'s exact on-surface projection, and re-projecting a vertex-KNN candidate onto its incident triangles to recover surface distance would hand-roll the point-triangle distance `closest_point` already owns. That parallel-KNN acceleration is therefore owned by the `scan/deviation`+`scan/registration` cloud-to-vertex consumers, never minted here as a dead spatial tier.

## [04]-[UPSTREAM]

- [WHEEL_BAND]: `trimesh` is unmarked pure-Python admitted on the cp315 core (manifest line 139), and its `numpy`/`scipy`-backed paths run on the project venv directly — the pure-Python query surface (`proximity`/`contains`/`sample` over `numpy`+`scipy`, `kdtree`, and the `ray` arm over the pure-Python `RayMeshIntersector`) answers in-process and is availability-invariant. Three arms instead ride OPTIONAL NATIVE-COMPILED backends `trimesh` binds but the manifest does not declare and that ship no cp315 wheel (`trimesh.md` ABI line 15, wheel-floor line 14: the bare cp315 venv "raises on the boolean and R-tree paths"): `Bounds` over the libspatialindex `rtree` R-tree, the conservative `Clearance` over the `python-fcl` engine, and the exact `Clearance` over `manifold3d` (manifest line 126, `python_version<'3.15'`). On the bare cp315 core `find_spec("rtree")`/`find_spec("fcl")`/`find_spec("manifold3d")` all return `None`, so the `_offload` set holds `bounds` and `clearance` and `_route` hops both to the companion lane (`forge-companion-env`, the `python_version<'3.15'` interpreter the daemon hosts where the native wheels resolve) rather than railing an `ImportError` in-process — the availability hole the prior draft's "always-available cp315 core" claim left open. The `embreex` ray accel is likewise native, but `mesh.ray` falls back to the pure-Python intersector when it is absent, so the `Ray` arm never enters the offload set. The `open3d`/`small_gicp` companion wheels are admitted for the scan rail's cloud-correspondence owners, not as a spatial backend, so this owner declares no dependency on them.
- [CLEARANCE_BAND]: `Clearance` is the one kind whose KERNEL varies by backend — the `python-fcl` `collision.CollisionManager.min_distance_single` conservative separation under `CORE`, the exact `manifold3d.Manifold.min_gap` watertight gap under `MANIFOLD3D` — and BOTH backends are native-compiled companion-band with no cp315 wheel, so the clearance arm offloads its resolved kernel whenever that module is absent in-process. `_route` reads the `_offload` set the capsule folds at construction: `clearance`'s probe is rewritten to the resolved kernel's module (`manifold3d` under MANIFOLD3D, else `fcl`) so `find_spec` decides the hop precisely — a companion interpreter carrying both wheels runs the exact gap in-process, a bare core hops it. The result never silently misses (a missing wheel hops rather than raises) and the degradation is two-axis: the `backend` fact records the conservative-FCL-versus-exact-gap kernel and the `offloaded` fact records core-versus-companion, both native scalars on the `SpatialReceipt`. The pure-Python kinds — proximity, ray, contains, sample — are availability-invariant and never enter the offload set. The exact-clearance KERNEL is one `SpatialBackend` axis, the run LOCATION one `_offload` axis, never a parallel clearance owner.
